using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;


public static class CsprojGenerator
{
    public static string GetSolutionDirectory()
    {
        var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

        while (directory != null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }
        if (directory == null)
        {
            // Fallback: Nutze das aktuelle BaseDirectory
            directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            Debug.WriteLine("Fallback auf BaseDirectory: " + directory.FullName);
        }
        else
        {
            Debug.WriteLine("Root: " + directory.FullName);
        }
        return directory.FullName;
    }
    public static string GenerateCsprojWithAbsolutePaths(
        string projectRoot,
        IEnumerable<string> sourceItems,
        IEnumerable<string>? extraCompile = null)
    {
        if (string.IsNullOrWhiteSpace(projectRoot))
            throw new ArgumentException("projectRoot must be a valid directory path");

        projectRoot = Path.GetFullPath(projectRoot);
        string projectDlls = Path.Combine(projectRoot, "libs");
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        // === Collect DLL references ===
        var dllFiles = Directory.GetFiles(baseDir, "*.dll")
            .Concat(Directory.Exists(projectDlls) ? Directory.GetFiles(projectDlls, "*.dll") : Array.Empty<string>())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        string[] excludePrefixes = { "Microsoft.CodeAnalysis" }; // add more if needed

        var referenceItems = dllFiles
            .Where(path =>
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
                return !excludePrefixes.Any(prefix => fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
            })
            .Select(path =>
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
                return $@"
<Reference Include=""{XmlEscape(fileName)}"">
    <HintPath>{XmlEscape(path)}</HintPath>
</Reference>";
            });

        // === Build <Compile> entries for external/absolute folders/files ===
        var compileExternal = new List<string>();

        foreach (var item in sourceItems ?? Enumerable.Empty<string>())
        {
            if (string.IsNullOrWhiteSpace(item)) continue;

            // Resolve relative items against projectRoot to allow ..\\ paths
            string resolved = Path.GetFullPath(Path.IsPathRooted(item) ? item : Path.Combine(projectRoot, item));

            if (File.Exists(resolved) && string.Equals(Path.GetExtension(resolved), ".cs", StringComparison.OrdinalIgnoreCase))
            {
                // Single file include
                string link = BuildLinkVirtualPath(projectRoot, resolved);
                compileExternal.Add($@"
<Compile Include=""{XmlEscape(resolved)}"">
    <Link>
        {XmlEscape(link)}
    </Link>
</Compile>");
            }
            else if (Directory.Exists(resolved))
            {
                // Directory include (all .cs recursively)
                string linkBase = BuildLinkVirtualPath(projectRoot, resolved);
                // Use **/*.cs + Link with MSBuild metadata to mirror subfolders
                compileExternal.Add($@"
<Compile Include=""{XmlEscape(resolved)}\\**\\*.cs\"">
    <Link>
        {XmlEscape(linkBase)}\\%(RecursiveDir)%(Filename)%(Extension)
    </Link>
</Compile>");
            }
            else
            {
                // still emit a comment to help debugging
                compileExternal.Add($"    <!-- Skipped: {XmlEscape(resolved)} (not found) -->");
            }
        }

        // === Additional compile includes that live INSIDE the project directory ===
        var compileLocal = new List<string>();
        foreach (var rel in (extraCompile ?? Array.Empty<string>()))
        {
            var norm = rel.Replace('/', '\\');
            compileLocal.Add($"    <Compile Include=\"{XmlEscape(norm)}\" />");
        }

        // === Assemble .csproj ===
        string compileItemsXml = string.Join(Environment.NewLine, compileLocal.Concat(compileExternal));
        string referenceItemsXml = string.Join(Environment.NewLine, referenceItems);

        return $@"
<Project Sdk=""Microsoft.NET.Sdk"">  
    <PropertyGroup>   
        <OutputType>Exe</OutputType>
        <TargetFramework>net9.0-windows</TargetFramework> 
        <UseWindowsForms>true</UseWindowsForms>
        <!-- We manage all items explicitly so we can include external folders/files -->
        <EnableDefaultItems>false</EnableDefaultItems>
    </PropertyGroup>

    <ItemGroup>
    {compileItemsXml}
    </ItemGroup>

    <ItemGroup>
        {referenceItemsXml}
    </ItemGroup>
</Project>
";
    }

    /// <summary>
    /// Creates a neat virtual folder for external sources under "External\\...".
    /// </summary>
    private static string BuildLinkVirtualPath(string projectRoot, string absolutePath)
    {
        // Try to keep a meaningful label: External\\<TopFolder>\\<Tail>
        // Example: ..\\qbook\\Controls => External\\qbook\\Controls
        var dir = Directory.Exists(absolutePath) ? absolutePath : Path.GetDirectoryName(absolutePath)!;
        string top = new DirectoryInfo(dir).Name; // e.g., Controls

        // find the first folder outside projectRoot to use as group name (e.g., qbook, qbookCsScript)
        string group = TryGetTopAncestorName(projectRoot, dir) ?? top;

        if (Directory.Exists(absolutePath))
        {
            return Path.Combine("External", group, top);
        }
        else
        {
            return Path.Combine("External", group, top, Path.GetFileName(absolutePath));
        }
    }

    private static string? TryGetTopAncestorName(string projectRoot, string path)
    {
        projectRoot = Path.GetFullPath(projectRoot).TrimEnd(Path.DirectorySeparatorChar);
        var current = new DirectoryInfo(Path.GetFullPath(path));
        DirectoryInfo? last = null;
        while (current != null && !current.FullName.Equals(projectRoot, StringComparison.OrdinalIgnoreCase))
        {
            last = current;
            current = current.Parent;
        }
        return last?.Name; // e.g., qbookCsScript
    }

    private static string XmlEscape(string s)
    {
        return s
            .Replace("&", "&amp;")
            .Replace("\"", "&quot;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }
}

// =========================
// Example usage:
// var csprojXml = CsprojGenerator.GenerateCsprojWithAbsolutePaths(
//     projectRoot: @"C:\\dev\\YourHostProject",
//     sourceItems: new []
//     {
//         @"..\\qbook\\Controls",
//         @"..\\qbookCsScript\\Controls",
//         @"..\\qbookCsScript\\Logging",
//         @"..\\qbookCsScript\\Net",
//         @"..\\qbook\\Some\\SingleFile.cs" // files are fine, too
//     },
//     extraCompile: new []
//     {
//         "project.cs",                           // your entry point inside the host project
//         @"Shared\\SignalPool.cs",
//         @"Shared\\Classes\\**\\*.cs",
//         @"Pages\\**\\*.cs"
//     }
// );
// File.WriteAllText(Path.Combine(projectRoot, "Generated.csproj"), csprojXml);


// Beispielnutzung (angepasst an deine funktionierende Variante) :
// var xml = CsprojGenerator.GenerateCsprojForEditor(
//     projectRoot: @"B:\Github\amium-at\qbook.studio\GeneratedHost",
//     relativeSourceGlobs: new[]
//     {
//         "project.cs",
//         @"Shared\SignalPool.cs",
//         @"Shared\Classes\**\*.cs",
//         @"Pages\**\*.cs",
//         // wenn du wirklich externe Ordner einbinden willst (Editor-only)
//         @"..\qbookCsScript\Controls\**\*.cs",
//         @"..\qbookCsScript\Logging\**\*.cs",
//     },
//     dllHintPaths: new[]
//     {
//         // optional: zusätzliche DLLs, Pfade werden relativ abgespeichert
//         @"B:\Bin\ThirdParty\Foo.dll"
//     },
//     projectReferences: new[]
//     {
//         // optional: statt Source-Mirroring lieber echte Projekt-Refs (besseres IntelliSense)
//         @"..\qbook\qbook.csproj",
//         @"..\qbookCsScript\qbookCsScript.csproj"
//     }
// );
// File.WriteAllText(Path.Combine(@"B:\Github\amium-at\qbook.studio\GeneratedHost", "Generated.csproj"), xml);
