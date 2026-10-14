#nullable enable annotations

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public static class VsCodeLauncher
{
    public static async Task<bool> OpenFolder(string folderPath)
    {
        folderPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(folderPath));
        if (!Directory.Exists(folderPath) || !File.Exists(Path.Combine(folderPath, "Book.json")) ||
            Directory.GetFiles(folderPath, "*.csproj").Length == 0 || !File.Exists(Path.Combine(folderPath, "pipes.json")))
            throw new InvalidOperationException("The book workspace must contain Book.json, a project file and pipes.json: " + folderPath);

        string? codeExe = TryFindVsCodeExe();
        if (codeExe == null) return false;
        string? cli = TryFindVsCodeCli(codeExe);
        if (cli == null) return false;

        // Invoke the CLI itself so its terminal exit code confirms the single dispatch.
        var start = new ProcessStartInfo(codeExe)
        {
            WorkingDirectory = folderPath,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        start.Environment["ELECTRON_RUN_AS_NODE"] = "1";
        start.ArgumentList.Add(cli);
        // Protect existing workspaces. VS Code resolves an already open folder by
        // its URI before deciding whether an unmatched folder needs a new window.
        bool editorRunning = false;
        foreach (var editor in Process.GetProcessesByName("Code"))
        {
            using (editor)
            {
                try { editorRunning |= !editor.HasExited; }
                catch (InvalidOperationException) { }
            }
        }
        if (editorRunning) start.ArgumentList.Add("--new-window");
        start.ArgumentList.Add("--folder-uri");
        start.ArgumentList.Add(new Uri(folderPath + Path.DirectorySeparatorChar).AbsoluteUri.TrimEnd('/'));
        using var process = Process.Start(start);
        if (process == null) return false;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        string output = await stdout;
        string error = await stderr;
        if (process.ExitCode != 0)
            throw new InvalidOperationException("VS Code could not open the book workspace (exit " + process.ExitCode + "): " + error + output);
        return true;
    }

    private static string? TryFindVsCodeCli(string codeExe)
    {
        string installation = Path.GetFullPath(Path.GetDirectoryName(codeExe)!);
        string binDirectory = Path.Combine(installation, "bin");
        string launcher = Path.Combine(binDirectory, "code.cmd");
        if (File.Exists(launcher))
        {
            // Read only the shipped direct invocation, never execute or interpret batch commands.
            foreach (string line in File.ReadLines(launcher))
            {
                var declaration = Regex.Match(line,
                    "^\\s*@?\\\"%~dp0(?<exe>[^\\\"]+)\\\"\\s+\\\"%~dp0(?<cli>[^\\\"]+\\\\resources\\\\app\\\\out\\\\cli\\.js)\\\"\\s+%\\*\\s*$",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                if (!declaration.Success) continue;

                string executablePath = declaration.Groups["exe"].Value;
                string cliPath = declaration.Groups["cli"].Value;
                if (executablePath.IndexOfAny(new[] { '%', '!' }) >= 0 ||
                    cliPath.IndexOfAny(new[] { '%', '!' }) >= 0 ||
                    Path.IsPathRooted(executablePath) || Path.IsPathRooted(cliPath))
                    return null;

                string declaredExe = Path.GetFullPath(Path.Combine(binDirectory, executablePath));
                string declaredCli = Path.GetFullPath(Path.Combine(binDirectory, cliPath));
                if (!string.Equals(declaredExe, Path.GetFullPath(codeExe), StringComparison.OrdinalIgnoreCase) ||
                    !declaredCli.StartsWith(installation + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    return null;

                // A declaration selects the active version; a stale declaration must not pick another version.
                return File.Exists(declaredCli) ? declaredCli : null;
            }
        }

        string directCli = Path.Combine(installation, "resources", "app", "out", "cli.js");
        return File.Exists(directCli) ? directCli : null;
    }

    private static string? TryFindVsCodeExe()
    {
        foreach (var directory in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
        {
            string path = directory.Trim().Trim('"');
            if (path.Length == 0) continue;
            if (File.Exists(Path.Combine(path, "code.cmd")))
            {
                string candidate = Path.GetFullPath(Path.Combine(path, "..", "Code.exe"));
                if (File.Exists(candidate)) return candidate;
            }
        }
        string userInstall = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Microsoft VS Code", "Code.exe");
        string machineInstall = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft VS Code", "Code.exe");
        string machineInstallX86 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft VS Code", "Code.exe");
        if (File.Exists(userInstall)) return userInstall;
        if (File.Exists(machineInstall)) return machineInstall;
        if (File.Exists(machineInstallX86)) return machineInstallX86;
        return null;
    }
}
