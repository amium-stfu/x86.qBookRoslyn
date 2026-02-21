

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using ScintillaNET;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using static qbookCode.Roslyn.RoslynService;
using AccessibilityCode = Microsoft.CodeAnalysis.Accessibility;
using RoslynDocument = Microsoft.CodeAnalysis.Document;

namespace qbookCode.Roslyn
{
    public class CodeDocument
    {

        public string Filename { get; }
        public string Code { get; set; }
        public RoslynDocument? Document => Core.Roslyn.GetDocumentByFilename(Filename);

        public DocumentId? Id => Document?.Id;

        private readonly RoslynService _roslyn;

        public bool Active
        {
            get;
            set;
        }

        public CodeDocument(string filename, string code, bool active, RoslynService roslyn)
        {
            Filename = filename;
            Code = code;
            _roslyn = roslyn;
            Active = active;
        }

        public async Task<string> GetTextAsync()
        {
            if (Document != null)
            {
                var code = await Document.GetTextAsync();
                return code.ToString();
            }
            return Code;
        }
        public void UpdateCode()
        {
            var newText = SourceText.From(Code, Encoding.UTF8);
            if (Document != null)
            {
                var updatedDoc = Document.WithText(newText);
                _roslyn.GetWorkspace.TryApplyChanges(updatedDoc.Project.Solution);

            }
        }

        public void Exclude()
        {
            if (Document != null)
            {
                _roslyn.ExcludeDocument(Document.Id);
            }
            Active = false;
        }

        public async Task Include()
        {
            if (Document == null)
            {
                await _roslyn.IncludeDocument(Filename, Code);
            }
            Active = true;
        }
    }



    public sealed partial class RoslynService
    {

      
        private MSBuildWorkspace? _ws;
        private Project? _project;
        private readonly SemaphoreSlim _loadSemaphore = new(1, 1);
        private bool _isLoading;

        public DocumentationService Documentation = new DocumentationService(capacity: 4000);


        public Project GetProject => _project;

        private readonly object _buildLock = new();
        private bool _isBuildingAssembly = false;
        public Project? GetCurrentProject() => _adhocWs?.CurrentSolution.GetProject(_projectId);
        public ProjectId? GetCurrentProjectId() => _projectId;

        private bool _useInMemory = true;
        private AdhocWorkspace? _adhocWs;

        private readonly Dictionary<string, CodeDocument> _docMap = new();

        // NEW: cache a single MEF host to avoid repeatedly allocating composition containers
        private static readonly HostServices s_host = CreateMefHost();

        // NEW: cache metadata references by path so we don’t re-open PE files repeatedly
        private static readonly object s_refLock = new();
        private static volatile List<MetadataReference>? s_cachedReferences;


        public bool IsProjectLoaded => _project != null && ((_useInMemory && _adhocWs != null) || (!_useInMemory && _ws != null));
        public async Task LoadProjectAsync(string csprojPath)
        {
            Debug.WriteLine("LoadProjectAsync: '" + csprojPath + "'");

            if (string.IsNullOrWhiteSpace(csprojPath) || !File.Exists(csprojPath)) return;
            await _loadSemaphore.WaitAsync();
            try
            {
                if (_project != null || _isLoading) return;
                _isLoading = true;
                if (!MSBuildLocator.IsRegistered)
                {
                    try
                    {
                        var instances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
                        if
                            (instances.Length > 0) MSBuildLocator.RegisterInstance(instances.OrderByDescending(i => i.Version).First());
                        else
                            MSBuildLocator.RegisterDefaults();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("MSBuildLocator registration failed: " + ex.Message);
                        Debug.WriteLine("[RoslynServices]: LoadProjectAsync MSBuildLocator registration failed: " + ex.Message);
                    }
                }
                var props = new Dictionary<string, string> { { "DesignTimeBuild", "true" }, { "BuildingInsideVisualStudio", "true" } };



                _ws = MSBuildWorkspace.Create(props);

                _ws.WorkspaceFailed += (s, e) => Debug.WriteLine($"[WorkspaceFailed] {e.Diagnostic.Kind}: {e.Diagnostic.Message}");
                try
                {
                    _project = await _ws.OpenProjectAsync(csprojPath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("OpenProjectAsync failed: " + ex.Message);
                    _project = null;
                }
            }
            finally
            {
                _isLoading = false;
                _loadSemaphore.Release();
            }
        }

        private EventHandler<WorkspaceDiagnosticEventArgs>? _workspaceFailedHandler;


        public void ResetDocumentsOnly()
        {
            if (_adhocWs == null || _projectId == null)
                return;

            var solution = _adhocWs.CurrentSolution;
            var project = solution.GetProject(_projectId);
            if (project == null)
                return;

            foreach (var doc in project.Documents.ToList())
            {
                solution = solution.RemoveDocument(doc.Id);
            }

            _adhocWs.TryApplyChanges(solution);
            _docMap.Clear();
        }

 
        public async Task ReloadDocumentsAsync(IEnumerable<(string fileName, string code)> files)
        {
            ResetDocumentsOnly();

            foreach (var (fileName, code) in files)
            {
                AddCodeDocument(fileName, code, true);
            }
        }


        public void EnsureWorkspace()
        {
            if (_adhocWs == null)
                _adhocWs = new AdhocWorkspace(s_host);
        }

        public class ReferenceCacheFile
        {
            public List<string> References { get; set; } = new();
            public string AppVersion { get; set; } = "1.0";
        }



        private string ReferenceCachePath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RoslynReferenceCache.json");

        private bool TryLoadReferenceCache(out List<MetadataReference> refs)
        {
            refs = new();

            try
            {
                if (!File.Exists(ReferenceCachePath))
                    return false;

                var json = File.ReadAllText(ReferenceCachePath);
                var data = JsonSerializer.Deserialize<ReferenceCacheFile>(json);
                if (data == null || data.References.Count == 0)
                    return false;

                foreach (var path in data.References)
                {
                    if (!File.Exists(path))
                        return false; // Cache ungültig → neu erzeugen

                    refs.Add(MetadataReference.CreateFromFile(path));
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        private void SaveReferenceCache(IEnumerable<MetadataReference> refs)
        {
            try
            {
                var file = new ReferenceCacheFile();

                foreach (var r in refs.OfType<PortableExecutableReference>())
                {
                    if (!string.IsNullOrWhiteSpace(r.FilePath))
                        file.References.Add(r.FilePath);
                }

                var json = JsonSerializer.Serialize(file, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(ReferenceCachePath, json);
            }
            catch
            {
                // Ignorieren – Cache ist optional
            }
        }

        private static List<MetadataReference> AddLoadedAssembliesAsReferences()
        {
            var refs = new List<MetadataReference>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    string? location = asm.Location;
                    if (string.IsNullOrEmpty(location)) continue; // Dynamische oder Ref-Assemblies ohne Pfad ignorieren
                    if (!seen.Add(location)) continue; // Duplikate vermeiden

                    refs.Add(MetadataReference.CreateFromFile(location));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Roslyn] Skip assembly {asm.FullName}: {ex.Message}");
                }
            }

            return refs;
        }
        private static List<MetadataReference> GetOrBuildDefaultReferences()
        {
            lock (s_refLock)
            {
                if (s_cachedReferences != null)
                    return s_cachedReferences;

                var refPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            typeof(object).Assembly.Location,
            typeof(Console).Assembly.Location,
            typeof(Enumerable).Assembly.Location,
            typeof(System.Runtime.GCSettings).Assembly.Location
        };

                try
                {
                    refPaths.Add(typeof(System.Drawing.Point).Assembly.Location);
                    refPaths.Add(typeof(System.Windows.Forms.Control).Assembly.Location);
                }
                catch { }

                string netstandardPath = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "netstandard.dll");
                if (File.Exists(netstandardPath)) refPaths.Add(netstandardPath);

                string libsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs");
                if (Directory.Exists(libsDir))
                {
                    foreach (string path in Directory.GetFiles(libsDir, "*.dll"))
                    {
                        if (!IsManagedAssembly(path))
                        {
                            Debug.WriteLine($"[Roslyn] Skip unmanaged: {Path.GetFileName(path)}");
                            continue;
                        }

                        try
                        {
                            refPaths.Add(path);
                            Debug.WriteLine($"[Roslyn] +ManagedRef: {Path.GetFileName(path)}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[Roslyn] Skip {Path.GetFileName(path)}: {ex.Message}");
                        }
                    }
                }

                var refs = new List<MetadataReference>();
                foreach (string p in refPaths)
                {
                    try
                    {
                        refs.Add(MetadataReference.CreateFromFile(p));
                    }
                    catch (BadImageFormatException)
                    {
                        Debug.WriteLine($"[Roslyn] Bad image skipped: {p}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[Roslyn] Metadata load failed: {p} → {ex.Message}");
                    }
                }

                s_cachedReferences = refs;
                return refs;
            }
        }

        public void CreateEmptyProjectOld(string csprojPath)
        {
            // Workspace vorbereiten
            if (_adhocWs == null)
                _adhocWs = new AdhocWorkspace(s_host);
            else
                try { _adhocWs.ClearSolution(); } catch { }

            var projectId = ProjectId.CreateNewId();
            _projectId = projectId;

            // Basis-Referenzen aus der csproj-Datei laden
            var references = new List<MetadataReference>();
            if (File.Exists(csprojPath))
            {
                var doc = XDocument.Load(csprojPath);
                var refElements = doc.Descendants("Reference");
                foreach (var r in refElements)
                {
                    var hintPath = r.Element("HintPath")?.Value;
                    if (!string.IsNullOrWhiteSpace(hintPath) && File.Exists(hintPath))
                    {
                        references.Add(MetadataReference.CreateFromFile(hintPath));
                        Debug.WriteLine($"[Roslyn] +Ref from csproj: {hintPath}");
                    }
                }
            }

            // Neues Projekt anlegen
            var projectInfo = ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                name: "DynamicProject",
                assemblyName: "DynamicAssembly",
                language: LanguageNames.CSharp,
                metadataReferences: references,
                parseOptions: new CSharpParseOptions(LanguageVersion.Preview),
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            _adhocWs.AddProject(projectInfo);
            _project = _adhocWs.CurrentSolution.GetProject(projectId);
            RoslynDiagnostic.InitDiagnostic();
        }

        /// <summary>
        /// Creates an empty project in the ad-hoc workspace, optionally loading references from a specified .csproj
        /// file.
        /// </summary>
        /// <remarks>If a valid .csproj path is provided, the method parses the file to gather references,
        /// including <Reference> and <ProjectReference> elements. It also adds basic references from the .NET runtime
        /// and local libraries. The method initializes the workspace if it is not already set up and clears any
        /// existing solution.</remarks>
        /// <param name="csprojPath">The path to the .csproj file from which to load project references. If the path is null or does not exist,
        /// no references will be loaded from the project file.</param>
        public void CreateEmptyProject(string csprojPath)
        {
            // 0) Workspace vorbereiten
            if (_adhocWs == null)
                _adhocWs = new AdhocWorkspace(s_host);
            else
                try { _adhocWs.ClearSolution(); } catch { /* ignore */ }

            var projectId = ProjectId.CreateNewId();
            _projectId = projectId;

            // 1) .csproj parsen (optional)
            XDocument csproj = null;
            string csprojDir = null;
            if (!string.IsNullOrWhiteSpace(csprojPath) && File.Exists(csprojPath))
            {
                csproj = XDocument.Load(csprojPath);
                csprojDir = Path.GetDirectoryName(Path.GetFullPath(csprojPath));
            }

            // 2) Referenzen sammeln
            var references = new List<MetadataReference>();
            var dedup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void AddRefPath(string dllPath)
            {
                if (string.IsNullOrWhiteSpace(dllPath) || !File.Exists(dllPath)) return;
                var full = Path.GetFullPath(dllPath);
                if (!dedup.Add(full)) return;

                var r = CreateReferenceWithDocs(full); // hängt XML-Doku an, wenn vorhanden
                if (r != null) references.Add(r);
            }

            // 2a) <Reference><HintPath> aus der csproj
            if (csproj != null)
            {
                foreach (var r in csproj.Descendants().Where(e => e.Name.LocalName == "Reference"))
                {
                    var hint = r.Elements().FirstOrDefault(e => e.Name.LocalName == "HintPath")?.Value?.Trim();
                    if (string.IsNullOrWhiteSpace(hint)) continue;

                    var abs = ResolvePath(csprojDir, hint);
                    if (abs != null) AddRefPath(abs);
                    Debug.WriteLine($"[Roslyn] +Ref from csproj (HintPath): {abs ?? hint} {(abs == null ? "[missing]" : "")}");
                }

                // 2b) <ProjectReference> (wir laden nur die Ausgabe-DLL, falls vorhanden)
                foreach (var pr in csproj.Descendants().Where(e => e.Name.LocalName == "ProjectReference"))
                {
                    var include = pr.Attribute("Include")?.Value?.Trim();
                    if (string.IsNullOrWhiteSpace(include)) continue;

                    var projAbs = ResolvePath(csprojDir, include);
                    if (projAbs == null || !File.Exists(projAbs)) continue;

                    // Heuristik: Versuche, zum Output zu kommen (bin/Debug|Release/<TFM>/<Name>.dll)
                    // Ohne MSBuild-Evaluation ist das unzuverlässig, deshalb:
                    //  - Versuche bin/Debug; wenn Release existiert, alternativ prüfen.
                    var projDir = Path.GetDirectoryName(projAbs);
                    var projName = Path.GetFileNameWithoutExtension(projAbs);

                    // Probiere einige typische Orte für .NET SDK-Projekte (Debug/Release + häufige TFMs)
                    var candidateDirs = new[]
                    {
                Path.Combine(projDir, "bin", "Debug"),
                Path.Combine(projDir, "bin", "Release")
            };

                    var tfms = new[] { "net8.0", "net7.0", "net6.0", "net48", "net472", "netstandard2.1", "netstandard2.0" };
                    string foundDll = null;

                    foreach (var baseOut in candidateDirs)
                    {
                        if (!Directory.Exists(baseOut)) continue;

                        // Falls TFM-Verzeichnisse existieren
                        foreach (var tfm in tfms)
                        {
                            var dll = Path.Combine(baseOut, tfm, projName + ".dll");
                            if (File.Exists(dll)) { foundDll = dll; break; }
                        }
                        if (foundDll != null) break;

                        // Oder direkt im baseOut (ältere .NET Framework Projekte)
                        var directDll = Path.Combine(baseOut, projName + ".dll");
                        if (File.Exists(directDll)) { foundDll = directDll; break; }
                    }

                    if (foundDll != null)
                    {
                        AddRefPath(foundDll);
                        Debug.WriteLine($"[Roslyn] +Ref from ProjectReference: {foundDll}");
                    }
                    else
                    {
                        Debug.WriteLine($"[Roslyn] ProjectReference output not found: {projAbs}");
                    }
                }

                // 2c) (Optional) <PackageReference>: einfache Heuristik über globales NuGet-Cacheverzeichnis
                // Achtung: Ohne MSBuild-Evaluation und TFM-Kenntnis ist das nicht 100% robust.
                // Wenn du willst, kannst du das aktivieren und weiter verfeinern.

                /*
                foreach (var pr in csproj.Descendants().Where(e => e.Name.LocalName == "PackageReference"))
                {
                    var id = pr.Attribute("Include")?.Value?.Trim();
                    var version = pr.Attribute("Version")?.Value?.Trim()
                                 ?? pr.Elements().FirstOrDefault(e => e.Name.LocalName == "Version")?.Value?.Trim();
                    if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(version)) continue;

                    var pkgDlls = TryResolvePackageDlls(id, version);
                    foreach (var dll in pkgDlls)
                        AddRefPath(dll);
                }
                */
            }

            // 2d) Basis-Referenzen (BCL etc.)
            AddRefPath(typeof(object).Assembly.Location);
            AddRefPath(typeof(Enumerable).Assembly.Location);
            AddRefPath(typeof(System.Windows.Forms.Form).Assembly.Location);
            AddRefPath(typeof(System.Drawing.Point).Assembly.Location);
            AddRefPath(typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly.Location);

            // 2e) .NET Runtime-Verzeichnis (nur Managed-Assemblies; einfache Heuristik)
            string runtimeDir = RuntimeEnvironment.GetRuntimeDirectory();
            foreach (var dllPath in SafeEnumerateFiles(runtimeDir, "*.dll"))
            {
                if (!IsManagedAssembly(dllPath)) continue;
                AddRefPath(dllPath);
            }

            // 2f) Dein lokaler libs-Ordner
            string libsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs");
            foreach (var dllPath in SafeEnumerateFiles(libsDir, "*.dll"))
            {
                if (!IsManagedAssembly(dllPath)) continue;
                AddRefPath(dllPath);
            }

            // 3) Parse-/Compilation-Optionen (LangVersion aus csproj lesen, Fallback Preview)
            var langVersion = TryReadLangVersion(csproj) ?? LanguageVersion.Preview;
            var parseOptions = new CSharpParseOptions(langVersion);

            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            // 4) Projekt erstellen
            var projectInfo = ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                name: "DynamicProject",
                assemblyName: "DynamicAssembly",
                language: LanguageNames.CSharp,
                metadataReferences: references,
                parseOptions: parseOptions,
                compilationOptions: compilationOptions
            );

            _adhocWs.AddProject(projectInfo);
            _project = _adhocWs.CurrentSolution.GetProject(projectId);

            Debug.WriteLine($"[Roslyn] Project created with {references.Count} references.");

            // Optional: Dein Diagnostik-Setup
            RoslynDiagnostic.InitDiagnostic();
        }





        /// <summary>
        /// (Optional) Sehr einfache Auflösung von PackageReference-DLLs im globalen NuGet-Verzeichnis.
        /// Für robuste Ergebnisse wäre MSBuild-Evaluation ideal.
        /// </summary>
        private static IEnumerable<string> TryResolvePackageDlls(string packageId, string version)
        {
            var results = new List<string>();
            try
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var root = Path.Combine(home, ".nuget", "packages", packageId.ToLowerInvariant(), version);
                if (!Directory.Exists(root)) return results;

                // Häufige TFM-Verzeichnisse durchsuchen
                var libDir = Path.Combine(root, "lib");
                if (!Directory.Exists(libDir)) return results;

                var tfms = new[] { "net8.0", "net7.0", "net6.0", "netstandard2.1", "netstandard2.0", "net48", "net472" };
                foreach (var tfm in tfms)
                {
                    var dir = Path.Combine(libDir, tfm);
                    if (!Directory.Exists(dir)) continue;

                    foreach (var dll in Directory.GetFiles(dir, "*.dll"))
                        results.Add(dll);
                }
            }
            catch { /* ignore */ }

            return results;
        }

        /// <summary>
        /// Liest die C#-LangVersion aus der csproj (PropertyGroup -> LangVersion), sonst null.
        /// </summary>
        private static LanguageVersion? TryReadLangVersion(XDocument csproj)
        {
            if (csproj == null) return null;

            var lang = csproj
                .Descendants()
                .Where(e => e.Name.LocalName == "LangVersion")
                .Select(e => e.Value?.Trim())
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(lang)) return null;

            // Roslyn-Parser für LanguageVersion:
            if (LanguageVersionFacts.TryParse(lang, out var parsed))
                return parsed;

            // Häufige Synonyme
            if (string.Equals(lang, "default", StringComparison.OrdinalIgnoreCase)) return LanguageVersion.Default;
            if (string.Equals(lang, "latest", StringComparison.OrdinalIgnoreCase)) return LanguageVersion.Latest;
            if (string.Equals(lang, "preview", StringComparison.OrdinalIgnoreCase)) return LanguageVersion.Preview;

            return null;
        }

        /// <summary>
        /// Sehr einfache Heuristik: Prüft, ob Datei wie eine .NET-Assembly aussieht.
        /// </summary>
        private static bool IsManagedAssembly(string path)
        {
            try
            {
                // Minimal: auf ".dll" prüfen und grob Größe > 0
                if (!path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) return false;
                var info = new FileInfo(path);
                return info.Exists && info.Length > 0;
            }
            catch { return false; }
        }

        /// <summary>
        /// Gibt absolute Pfade zurück; löst relative Pfade relativ zur csproj aus.
        /// </summary>
        private static string ResolvePath(string baseDir, string candidate)
        {
            if (string.IsNullOrWhiteSpace(candidate)) return null;

            // Umgebungsvariablen expandieren
            candidate = Environment.ExpandEnvironmentVariables(candidate);

            // Falls bereits absolut
            if (Path.IsPathRooted(candidate))
                return File.Exists(candidate) ? candidate : null;

            if (string.IsNullOrWhiteSpace(baseDir) || !Directory.Exists(baseDir))
                return null;

            var combined = Path.GetFullPath(Path.Combine(baseDir, candidate));
            return File.Exists(combined) ? combined : null;
        }

        /// <summary>
        /// Sicheres Enumerieren von Dateien (Verzeichnisse dürfen nicht existieren, Fehler werden abgefangen).
        /// </summary>
        private static IEnumerable<string> SafeEnumerateFiles(string dir, string pattern)
        {
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir)) yield break;

            string[] files = Array.Empty<string>();
            try { files = Directory.GetFiles(dir, pattern); } catch { yield break; }
            foreach (var f in files) yield return f;
        }




        /// <summary>
        /// Erzeugt eine MetadataReference inkl. XmlDocumentationProvider (falls .xml neben .dll liegt).
        /// </summary>
        private static MetadataReference CreateReferenceWithDocs(string dllPath)
        {
            if(dllPath.Contains("qbookCsScript"))
            Debug.WriteLine("Check " + dllPath);
            if (string.IsNullOrWhiteSpace(dllPath) || !File.Exists(dllPath))
                return null;

            try
            {
                var xmlPath = Path.ChangeExtension(dllPath, ".xml");
                if (File.Exists(xmlPath))
                {
                    var provider = Microsoft.CodeAnalysis.XmlDocumentationProvider.CreateFromFile(xmlPath);
                    return MetadataReference.CreateFromFile(dllPath, documentation: provider);
                }
                return MetadataReference.CreateFromFile(dllPath);
            }
            catch
            {
                return MetadataReference.CreateFromFile(dllPath);
            }
        }

        public void CreateProject()
        {
            // Workspace bereitstellen / leeren
            if (_adhocWs == null)
                _adhocWs = new AdhocWorkspace(s_host);
            else
                try { _adhocWs.ClearSolution(); } catch { /* ignore */ }

            // Neues Projekt vorbereiten
            var projectId = ProjectId.CreateNewId();
            _projectId = projectId;

            List<MetadataReference> references;

            // 1) Versuchen, Referenz-Cache zu laden
            if (TryLoadReferenceCache(out references))
            {
                Debug.WriteLine("[Roslyn] Reference cache loaded.");
            }
            else
            {
                Debug.WriteLine("[Roslyn] Building references (no cache available)...");

                references = new List<MetadataReference>();
                var dedup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                void AddRef(string path)
                {
                    if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                        return;

                    var full = Path.GetFullPath(path);
                    if (!dedup.Add(full))
                        return; // dedupe

                    var r = CreateReferenceWithDocs(full);
                    if (r != null)
                        references.Add(r);
                }

                // --- Basis-Referenzen
                AddRef(typeof(object).Assembly.Location);
                AddRef(typeof(Enumerable).Assembly.Location);
                AddRef(typeof(System.Windows.Forms.Form).Assembly.Location);
                AddRef(typeof(System.Drawing.Point).Assembly.Location);
                AddRef(typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly.Location);

                // --- Bereits geladene Assemblies (deine bestehende Logik)
                foreach (var r in AddLoadedAssembliesAsReferences())
                {
                    // Wenn du hier bereits PortableExecutableReference-Objekte lieferst,
                    // kannst du – falls FilePath vorhanden – auf "CreateReferenceWithDocs" umstellen.
                    references.Add(r);
                }

                // --- .NET Runtime-Verzeichnis (nur Managed-Assemblies)
                string runtimeDir = RuntimeEnvironment.GetRuntimeDirectory();
                foreach (var dllPath in Directory.GetFiles(runtimeDir, "*.dll"))
                {
                    if (!IsManagedAssembly(dllPath))
                        continue;

                    try { AddRef(dllPath); } catch { /* ignore */ }
                }

                // --- Lokaler "libs"-Ordner
                string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs");
                if (Directory.Exists(baseDir))
                {
                    foreach (var dllPath in Directory.GetFiles(baseDir, "*.dll"))
                    {
                        if (!IsManagedAssembly(dllPath))
                            continue;

                        try { AddRef(dllPath); } catch { /* ignore */ }
                    }
                }

                // 2) Cache speichern (optional, für schnelleren App-Start)
                SaveReferenceCache(references);
            }

            // 3) Projekt erstellen (Parse/Compilation-Optionen nach Bedarf)
            var projectInfo = ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                name: "InMemoryProject",
                assemblyName: "InMemoryAssembly",
                language: LanguageNames.CSharp,
                metadataReferences: references,
                parseOptions: new CSharpParseOptions(LanguageVersion.Preview),
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            _adhocWs.AddProject(projectInfo);
            _project = _adhocWs.CurrentSolution.GetProject(projectId);

            Debug.WriteLine("[Roslyn] Project created with " + references.Count + " references.");
        }

        public void CreateProjectOld()
        {
            if (_adhocWs == null)
                _adhocWs = new AdhocWorkspace(s_host);
            else
                try { _adhocWs.ClearSolution(); } catch { }

            var projectId = ProjectId.CreateNewId();
            _projectId = projectId;

            List<MetadataReference> references;

            // ✅ 1. Versuchen, Cache zu laden
            if (TryLoadReferenceCache(out references))
            {
                Debug.WriteLine("[Roslyn] Reference cache loaded.");
            }
            else
            {
                Debug.WriteLine("[Roslyn] Building references (no cache available)...");

                references = new List<MetadataReference>();

                // deine bisherigen Logiken (Basisreferenzen, AppDomain, netstandard usw.)
                references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
                references.Add(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));
                references.Add(MetadataReference.CreateFromFile(typeof(System.Windows.Forms.Form).Assembly.Location));
                references.Add(MetadataReference.CreateFromFile(typeof(System.Drawing.Point).Assembly.Location));
                references.Add(MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly.Location));
                references.AddRange(AddLoadedAssembliesAsReferences());

                string runtimeDir = RuntimeEnvironment.GetRuntimeDirectory();
                foreach (var dllPath in Directory.GetFiles(runtimeDir, "*.dll"))
                {
                    if (!IsManagedAssembly(dllPath)) continue;
                    try { references.Add(MetadataReference.CreateFromFile(dllPath)); }
                    catch { }
                }

                string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs");
                if (Directory.Exists(baseDir))
                {
                    foreach (var dllPath in Directory.GetFiles(baseDir, "*.dll"))
                    {
                        if (!IsManagedAssembly(dllPath)) continue;
                        try { references.Add(MetadataReference.CreateFromFile(dllPath)); }
                        catch { }
                    }
                }

                // ✅ 2. Cache speichern
                SaveReferenceCache(references);
            }

            // ✅ Projekt erstellen
            var projectInfo = ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                "InMemoryProject",
                "InMemoryAssembly",
                LanguageNames.CSharp,
                metadataReferences: references,
                parseOptions: new CSharpParseOptions(LanguageVersion.Preview),
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            _adhocWs.AddProject(projectInfo);
            _project = _adhocWs.CurrentSolution.GetProject(projectId);
        }


        public void CreateEmptyProjectUsingRefPacks(string csprojPath)
        {
            // 0) Workspace vorbereiten
            if (_adhocWs == null)
                _adhocWs = new AdhocWorkspace(s_host);
            else
                try { _adhocWs.ClearSolution(); } catch { /* ignore */ }

            var projectId = ProjectId.CreateNewId();
            _projectId = projectId;

            // 1) .csproj laden (TFM + optional LangVersion)
            XDocument csproj = null;
            string csprojDir = null;
            if (!string.IsNullOrWhiteSpace(csprojPath) && File.Exists(csprojPath))
            {
                csproj = XDocument.Load(csprojPath);
                csprojDir = Path.GetDirectoryName(Path.GetFullPath(csprojPath));
            }
            string tfm = TryReadTargetFramework(csproj) ?? "net8.0"; // Fallback
            string normalizedTfm = NormalizeTfmForPacks(tfm);        // z.B. "net8.0-windows10.0.19041.0" -> "net8.0-windows" -> "net8.0"

            // 2) Packs-Wurzel finden (DOTNET_ROOT oder übliche Installationsorte)
            string dotnetRoot = FindDotnetRoot();
            if (string.IsNullOrEmpty(dotnetRoot))
                throw new InvalidOperationException("DOTNET_ROOT/.NET SDK nicht gefunden – Reference Packs können nicht geladen werden.");

            string packsRoot = Path.Combine(dotnetRoot, "packs");
            string packBase = Path.Combine(packsRoot, "Microsoft.NETCore.App.Ref");
            if (!Directory.Exists(packBase))
                throw new DirectoryNotFoundException($"Reference Pack-Basis nicht gefunden: {packBase}");

            // 3) Höchste installierte Pack-Version wählen, die das gewünschte TFM enthält
            var candidates = SafeEnumerateDirectories(packBase)
                .OrderByDescending(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            string refDir = null;
            foreach (var packVersionDir in candidates)
            {
                var tryDir = Path.Combine(packVersionDir, "ref", normalizedTfm);
                if (Directory.Exists(tryDir)) { refDir = tryDir; break; }
            }

            if (refDir == null)
            {
                // Fallback: z. B. von "net8.0-windows" auf "net8.0"
                var baseTfm = CutAtDash(normalizedTfm); // "net8.0-windows" -> "net8.0"
                if (!string.Equals(baseTfm, normalizedTfm, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var packVersionDir in candidates)
                    {
                        var tryDir = Path.Combine(packVersionDir, "ref", baseTfm);
                        if (Directory.Exists(tryDir)) { refDir = tryDir; break; }
                    }
                }
            }

            if (refDir == null)
                throw new DirectoryNotFoundException($"Kein Reference-Pack für TFM '{tfm}' gefunden (versucht: '{normalizedTfm}').");

            // 4) Kandidaten sammeln (zuerst: Ref-Pack-DLLs -> hohe Priorität)
            var byName = new Dictionary<string, (int prio, Version ver, string path)>(StringComparer.OrdinalIgnoreCase);

            void AddCandidate(string dllPath, int priority)
            {
                if (string.IsNullOrWhiteSpace(dllPath) || !File.Exists(dllPath)) return;
                if (!TryGetAssemblyIdentity(dllPath, out var name, out var ver)) return;

                if (byName.TryGetValue(name, out var cur))
                {
                    // Bevorzuge höhere Priorität (Ref-Pack > alles andere).
                    if (priority > cur.prio)
                    {
                        byName[name] = (priority, ver, dllPath);
                    }
                    else if (priority == cur.prio && ver > cur.ver)
                    {
                        // Bei gleicher Priorität: höhere Version
                        byName[name] = (priority, ver, dllPath);
                    }
                }
                else
                {
                    byName[name] = (priority, ver, dllPath);
                }
            }

            // 4a) Alle DLLs aus dem Reference-Pack
            foreach (var dll in SafeEnumerateFiles(refDir, "*.dll"))
                AddCandidate(dll, priority: 100); // höchste Priorität

            // 4b) Aus csproj: HintPaths (nur ergänzen, BCL-Namen bleiben vom Pack belegt)
            if (csproj != null)
            {
                foreach (var r in csproj.Descendants().Where(e => e.Name.LocalName == "Reference"))
                {
                    var hint = r.Elements().FirstOrDefault(e => e.Name.LocalName == "HintPath")?.Value?.Trim();
                    if (string.IsNullOrWhiteSpace(hint)) continue;
                    var abs = ResolvePath(csprojDir, hint);
                    if (abs != null) AddCandidate(abs, priority: 10);
                }

                // 4c) ProjectReference → Output-DLL heuristisch (Debug/Release + TFM)
                foreach (var pr in csproj.Descendants().Where(e => e.Name.LocalName == "ProjectReference"))
                {
                    var include = pr.Attribute("Include")?.Value?.Trim();
                    if (string.IsNullOrWhiteSpace(include)) continue;

                    var projAbs = ResolvePath(csprojDir, include);
                    if (projAbs == null || !File.Exists(projAbs)) continue;

                    var projDir = Path.GetDirectoryName(projAbs);
                    var projName = Path.GetFileNameWithoutExtension(projAbs);

                    var outs = new[]
                    {
                Path.Combine(projDir, "bin", "Debug"),
                Path.Combine(projDir, "bin", "Release")
            };
                    var tfms = new[] { normalizedTfm, CutAtDash(normalizedTfm), "net8.0", "net7.0", "net6.0", "netstandard2.1", "netstandard2.0" };

                    string found = null;
                    foreach (var baseOut in outs)
                    {
                        if (!Directory.Exists(baseOut)) continue;

                        foreach (var t in tfms.Where(t => !string.IsNullOrEmpty(t)))
                        {
                            var dll = Path.Combine(baseOut, t, projName + ".dll");
                            if (File.Exists(dll)) { found = dll; break; }
                        }
                        if (found != null) break;

                        var direct = Path.Combine(baseOut, projName + ".dll");
                        if (File.Exists(direct)) { found = direct; break; }
                    }
                    if (found != null) AddCandidate(found, priority: 50);
                }
            }

            // 5) Jetzt erst MetadataReferences erzeugen (mit Xml-Doku-Provider)
            var references = new List<MetadataReference>(byName.Count);
            foreach (var kv in byName.Values)
            {
                var r = CreateReferenceWithDocs(kv.path); // hängt Xml-Doku an, wenn vorhanden
                if (r != null) references.Add(r);
            }

            // 6) Optionen (LangVersion aus csproj, Fallback: Preview)
            var langVersion = TryReadLangVersion(csproj) ?? LanguageVersion.Preview;
            var parseOptions = new CSharpParseOptions(langVersion);
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            // 7) Projekt anlegen
            var projectInfo = ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                name: "DynamicProject",
                assemblyName: "DynamicAssembly",
                language: LanguageNames.CSharp,
                metadataReferences: references,
                parseOptions: parseOptions,
                compilationOptions: compilationOptions);

            _adhocWs.AddProject(projectInfo);
            _project = _adhocWs.CurrentSolution.GetProject(projectId);

            RoslynDiagnostic.InitDiagnostic();

            Debug.WriteLine($"[Roslyn] RefPack-Project created ({references.Count} references) for {tfm}.");
        }


        private static string FindDotnetRoot()
        {
            // 1) DOTNET_ROOT (empfohlen von Microsoft)
            var env = Environment.GetEnvironmentVariable("DOTNET_ROOT");
            if (!string.IsNullOrWhiteSpace(env) && Directory.Exists(env))
                return env;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // 2) Windows: %ProgramFiles%\dotnet
                var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                var path = Path.Combine(pf, "dotnet");
                if (Directory.Exists(path)) return path;
            }
            else
            {
                // 3) Linux/macOS: übliche Orte
                var candidates = new[]
                {
            Environment.GetEnvironmentVariable("DOTNET_ROOT(x86)"),
            "/usr/share/dotnet",
            "/usr/local/share/dotnet"
        };
                foreach (var c in candidates)
                    if (!string.IsNullOrWhiteSpace(c) && Directory.Exists(c))
                        return c;
            }
            return null;
        }

        private static string NormalizeTfmForPacks(string tfm)
        {
            if (string.IsNullOrWhiteSpace(tfm)) return null;
            // MSBuild-TFMs können RIDs enthalten, z. B. net8.0-windows10.0.19041.0
            // Für Packs existiert i. d. R. "net8.0-windows" oder "net8.0".
            // Reduziere ggf. mehrfach.
            var t = tfm.Trim();
            if (DirectoryNameExistsInPacksPattern(t)) return t; // bereits nutzbar

            // Schrittweise kürzen
            var dashIdx = t.IndexOf('-');
            if (dashIdx > 0)
            {
                var short1 = t[..dashIdx];           // "net8.0"
                var prefix = t[..t.LastIndexOf('-')];// z.B. "net8.0-windows"
                                                     // Bevorzugt: "net8.0-windows", sonst "net8.0"
                return prefix.Contains('-') ? prefix : short1;
            }
            return t;
        }

        private static string CutAtDash(string tfm)
        {
            if (string.IsNullOrWhiteSpace(tfm)) return tfm;
            var dashIdx = tfm.IndexOf('-');
            return dashIdx > 0 ? tfm[..dashIdx] : tfm;
        }

        // Du kannst hier smarter prüfen, aber oft reicht es, die Form "netX.Y" oder "netX.Y-qualifier" zu akzeptieren.
        private static bool DirectoryNameExistsInPacksPattern(string tfm) => tfm.StartsWith("net", StringComparison.OrdinalIgnoreCase);

        private static IEnumerable<string> SafeEnumerateDirectories(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir)) yield break;
            string[] d; try { d = Directory.GetDirectories(dir); } catch { yield break; }
            foreach (var x in d) yield return x;
        }

     

     

        private static bool TryGetAssemblyIdentity(string path, out string name, out Version ver)
        {
            name = null; ver = null;
            try
            {
                var an = System.Reflection.AssemblyName.GetAssemblyName(path);
                name = an.Name;
                ver = an.Version ?? new Version(0, 0, 0, 0);
                return true;
            }
            catch { return false; }
        }

   

        private static string TryReadTargetFramework(XDocument csproj)
        {
            if (csproj == null) return null;

            // TargetFrameworks hat Vorrang (erstes nehmen)
            var multi = csproj.Descendants().FirstOrDefault(e => e.Name.LocalName == "TargetFrameworks")?.Value?.Trim();
            if (!string.IsNullOrWhiteSpace(multi))
            {
                var first = multi.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
                if (!string.IsNullOrWhiteSpace(first)) return first;
            }

            // Single-TargetFramework
            var single = csproj.Descendants().FirstOrDefault(e => e.Name.LocalName == "TargetFramework")?.Value?.Trim();
            return string.IsNullOrWhiteSpace(single) ? null : single;
        }

        /// <summary>
        /// Erzeugt eine MetadataReference inkl. XmlDocumentationProvider (falls .xml neben .dll liegt).
        /// </summary>
    




        private List<MetadataReference> _referenceCache = new();

        public async Task RebuildProjectWithActiveFilesAsync()
        {

            List<RoslynDocument> docs = new List<RoslynDocument>();

            foreach (RoslynDocument doc in _project.Documents)
                docs.Add(doc);


            _adhocWs.ClearSolution();

            var projectId = ProjectId.CreateNewId();
            var refs = GetOrBuildDefaultReferences();

            var projectInfo = ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                "InMemoryProject",
                "InMemoryAssembly",
                LanguageNames.CSharp,
                metadataReferences: refs,
                parseOptions: new CSharpParseOptions(LanguageVersion.Preview),
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            _adhocWs.AddProject(projectInfo);

            foreach (RoslynDocument doc in docs)
            {
                var text = await doc.GetTextAsync();
                _adhocWs.AddDocument(projectId, doc.Name, text);
            }

            _project = _adhocWs.CurrentSolution.GetProject(projectId);
        }

        public AdhocWorkspace GetWorkspace => _adhocWs;
        public ProjectId GetProjectId => _projectId;

        ProjectId _projectId;

        public async Task LoadInMemoryProjectAsync(
    IEnumerable<(string fileName, string code)> files,
    IEnumerable<MetadataReference>? extraReferences = null)
        {
            _useInMemory = true;
            _adhocWs ??= new AdhocWorkspace();

            var parseOptions = new CSharpParseOptions(LanguageVersion.Latest, kind: SourceCodeKind.Regular);
            var compOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Debug);

            _projectId = ProjectId.CreateNewId();
            var projInfo = ProjectInfo.Create(
                _projectId,
                VersionStamp.Create(),
                name: "InMemoryQBook",
                assemblyName: "InMemoryQBook",
                language: LanguageNames.CSharp,
                parseOptions: parseOptions,
                compilationOptions: compOptions,
                metadataReferences: extraReferences ?? Enumerable.Empty<MetadataReference>()
            );

            _adhocWs.AddProject(projInfo);
            _project = _adhocWs.CurrentSolution.GetProject(_projectId)!;

            foreach (var (fileName, code) in files)
            {
                var absolutePath = Path.GetFullPath(fileName);
                // WICHTIG: Encoding setzen, sonst CS8055 bei Portable PDB
                var source = SourceText.From(code ?? string.Empty, Encoding.UTF8);

                // Einfacher: direkt AddDocument statt eigenes DocumentInfo mit falschen Parametern
                var doc = _adhocWs.AddDocument(_projectId, fileName, source);

                Debug.WriteLine("Add " + doc.Name);
                // FilePath zuweisen (für StackTrace / Debug)
                var withPath = doc.WithFilePath(absolutePath);

                _adhocWs.TryApplyChanges(withPath.Project.Solution);
                _project = _adhocWs.CurrentSolution.GetProject(_projectId)!;
                _docMap[fileName] = new CodeDocument(fileName, code, true, this);
            }
            Debug.WriteLine("[Diag] Id  =" + _project?.Id);
            Debug.WriteLine("[Diag] Docs=" + _project?.Documents.Count());
            Debug.WriteLine("[Diag] Has Program.cs=" + (_project?.Documents.Any(d => d.Name == "Program.cs")));


        }

        public async Task<string?> GetDocumentTextAsync(string fileName)
        {
            var doc = GetDocumentByFilename(fileName);
            if (doc == null) return null;

            var text = await doc.GetTextAsync();
            return text.ToString();
        }

        public async Task ExcludeDocumentFromProject(string fileName)
        {

            Debug.WriteLine("=== Looking for file " + fileName);
            RoslynDocument doc = null;
            foreach (RoslynDocument d in _project.Documents)
            {
                //  Debug.WriteLine($"'{d.Name}' <> '{fileName}'");
                if (fileName == d.Name.ToString())
                {
                    Debug.WriteLine("found " + d.Name);
                    doc = d;
                }

            }

            if (doc != null)
            {
                Debug.WriteLine("=== Removing " + doc.Name);
                var newSolution = _adhocWs.CurrentSolution.RemoveDocument(doc.Id);
                if (_adhocWs.TryApplyChanges(newSolution))
                {
                    _project = _adhocWs.CurrentSolution.GetProject(doc.Project.Id);
                }
            }

            foreach (RoslynDocument d in _project.Documents) Debug.WriteLine(d.Name);
            Debug.WriteLine("=== done ");

            await RebuildProjectWithActiveFilesAsync();
        }

        public CodeDocument AddCodeDocument(string filename, string code, bool active)
        {
            if (_adhocWs == null || _projectId == null)
                throw new InvalidOperationException("Workspace/Project not initialized.");

            var path = Path.GetFullPath(filename);
            var source = SourceText.From(code ?? string.Empty, Encoding.UTF8);
            var doc = _adhocWs.AddDocument(_projectId, filename, source);
            var withPath = doc.WithFilePath(path);

            _adhocWs.TryApplyChanges(withPath.Project.Solution);
            _project = _adhocWs.CurrentSolution.GetProject(_projectId)!;

            lock (_docMap)
            {
                _docMap[filename] = new CodeDocument(filename, code, active, this);
            }

            return _docMap[filename];
        }

     

        public void RemoveCodeDocument(string filename)
        {
            if (_adhocWs == null || _projectId == null)
                throw new InvalidOperationException("Workspace/Project not initialized.");
            var doc = _adhocWs.CurrentSolution.Projects
                .SelectMany(p => p.Documents)
                .FirstOrDefault(d => d.Name == filename);
            if (doc == null) return;
            var newSolution = _adhocWs.CurrentSolution.RemoveDocument(doc.Id);
            if (_adhocWs.TryApplyChanges(newSolution))
            {
                _project = _adhocWs.CurrentSolution.GetProject(_projectId);
            }
            lock (_docMap)
            {
                _docMap.Remove(filename);
            }
        }

        internal void ExcludeDocument(DocumentId id)
        {
            if (_adhocWs == null || _projectId == null)
                throw new InvalidOperationException("Workspace/Project not initialized.");

            var doc = _adhocWs.CurrentSolution.GetDocument(id);
            if (doc == null) return;

            var filename = doc.Name;
            var newSolution = _adhocWs.CurrentSolution.RemoveDocument(id);
            if (_adhocWs.TryApplyChanges(newSolution))
            {
                _project = _adhocWs.CurrentSolution.GetProject(_projectId);
            }

            lock (_docMap)
            {
                _docMap.Remove(filename);
            }
        }
        public CodeDocument GetCodeDocument(string fileName)
        {
            lock (_docMap)
            {
                _docMap.TryGetValue(fileName, out var codeDoc);
                return codeDoc;
            }
        }

        public async Task IncludeDocument(string fileName, string code)
        {
            var projectId = _project.Id;

            // Prüfen, ob das Dokument schon existiert
            var existingDoc = _project.Documents.FirstOrDefault(d => d.Name == fileName);
            if (existingDoc != null)
                return; // Schon vorhanden

            _adhocWs.AddDocument(projectId, fileName, SourceText.From(code, Encoding.UTF8));
            _project = _adhocWs.CurrentSolution.GetProject(projectId);
            var compilation = await _project.GetCompilationAsync();
        }

        public async Task ReactivateDocumentAsync(string fileName, RoslynDocument roslynDoc)
        {
            var projectId = _project.Id;

            // Prüfen, ob das Dokument schon existiert
            var existingDoc = _project.Documents.FirstOrDefault(d => d.Name == fileName);
            if (existingDoc != null)
                return; // Schon vorhanden

            var sourceText = await roslynDoc.GetTextAsync();
            var encoded = SourceText.From(sourceText.ToString(), Encoding.UTF8);
            _adhocWs.AddDocument(projectId, fileName, encoded);
            _project = _adhocWs.CurrentSolution.GetProject(projectId);
            var compilation = await _project.GetCompilationAsync();
        }

        private static HostServices CreateMefHost()
        {
            var assemblies = MefHostServices.DefaultAssemblies
                .Concat(new[]
                {
                    typeof(CompletionService).Assembly,
                    typeof(CSharpCompilation).Assembly,
                    typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions).Assembly,
                })
                .Distinct();
            return MefHostServices.Create(assemblies);
        }

        private static AdhocWorkspace CreateAdhocWorkspace()
        {
            var assemblies = MefHostServices.DefaultAssemblies
                .Concat(new[]
                {
            typeof(CompletionService).Assembly, // Microsoft.CodeAnalysis.Features
            typeof(CSharpCompilation).Assembly, // Microsoft.CodeAnalysis.CSharp
            typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions).Assembly, // CSharp.Workspaces
                })
                .Distinct();
            var host = MefHostServices.Create(assemblies);
            return new AdhocWorkspace(host);
        }

        public RoslynDocument? GetDocumentByFilename(string fileName)
        {
            return _adhocWs.CurrentSolution.Projects
                .SelectMany(p => p.Documents)
                .FirstOrDefault(d => d.Name == fileName);
        }

        public async Task UpdateOpenDocumentAsync(RoslynDocument doc, string text)
        {
            if (doc == null) return;

            var sourceText = SourceText.From(text, Encoding.UTF8);
            var updatedDoc = doc.WithText(sourceText);

            _adhocWs.TryApplyChanges(updatedDoc.Project.Solution);
        }

        public async Task<(Microsoft.CodeAnalysis.Completion.CompletionItem[] items, int spanStart)> GetCompletionsAsync(RoslynDocument doc, int caretPosition)
        {
            if (doc == null)
                return (new Microsoft.CodeAnalysis.Completion.CompletionItem[0], caretPosition);

            var completionService = CompletionService.GetService(doc);
            if (completionService == null)
                return (new Microsoft.CodeAnalysis.Completion.CompletionItem[0], caretPosition);

            var completions = await completionService.GetCompletionsAsync(doc, caretPosition);
            if (completions == null)
                return (new Microsoft.CodeAnalysis.Completion.CompletionItem[0], caretPosition);

            var items = completions.Items.ToArray();
            var spanStart = completions.Span.Start;

            return (items, spanStart);
        }
        public async Task<(RoslynDocument Document, int Line, int Column)?> GoToDefinitionAsync(RoslynDocument doc, int caret)
        {
            if (doc == null) return null;

            var semanticModel = await doc.GetSemanticModelAsync();
            var syntaxTree = await doc.GetSyntaxTreeAsync();
            if (semanticModel == null || syntaxTree == null) return null;

            var root = await syntaxTree.GetRootAsync();
            var position = caret > 0 ? caret - 1 : caret;
            var token = root.FindToken(position);
            var node = token.Parent;
            if (node == null) return null;

            var symbol = semanticModel.GetSymbolInfo(node).Symbol ?? semanticModel.GetDeclaredSymbol(node);

            Debug.WriteLine($"symbol: '{symbol}'");

            if (symbol == null) return null;

            var definition = await SymbolFinder.FindSourceDefinitionAsync(symbol, doc.Project.Solution) ?? symbol;
            var location = definition.Locations.FirstOrDefault(loc => loc.IsInSource);
            if (location == null) return null;

            var linePosition = location.GetLineSpan().StartLinePosition;
            var document = doc.Project.Solution.GetDocument(location.SourceTree);
            if (document == null) return null;

            Debug.WriteLine($"found in: '{document.Name}'");
            return (document, linePosition.Line, linePosition.Character);
        }
        public async Task<Dictionary<string, string>?> RenameSymbolAsync(RoslynDocument doc, int caret, string newName)
        {
            if (doc == null || string.IsNullOrWhiteSpace(newName)) return null;
            var semantic = await doc.GetSemanticModelAsync();
            var tree = await doc.GetSyntaxTreeAsync();
            if (semantic == null || tree == null) return null;
            var root = await tree.GetRootAsync();
            var token = root.FindToken(Math.Max(0, caret - 1));
            var symbol = semantic.GetSymbolInfo(token.Parent!).Symbol ?? semantic.GetDeclaredSymbol(token.Parent!);
            if (symbol == null) return null;
            var solution = doc.Project.Solution;
            var newSolution = await Renamer.RenameSymbolAsync(solution, symbol, newName, solution.Workspace.Options);
            if (!solution.Workspace.TryApplyChanges(newSolution)) return null;
            // Optional: Änderungen sammeln wie bisher
            return new Dictionary<string, string>();
        }
        public async Task<IReadOnlyList<string>> LookupInstanceMemberNamesAsync(RoslynDocument document, string text, int position)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            if (_ws == null || _project == null) return names.ToList();
            await UpdateOpenDocumentAsync(document, text);
            var doc = document;
            if (doc == null) return names.ToList();
            var semantic = await doc.GetSemanticModelAsync();
            var root = await doc.GetSyntaxRootAsync();
            if (semantic == null || root == null) return names.ToList();
            var token = root.FindToken(Math.Min(position, root.FullSpan.End - 1));
            var node = token.Parent;
            INamedTypeSymbol? typeSym = null;
            while (node != null && typeSym == null)
            {
                if (node is TypeDeclarationSyntax tds) typeSym = semantic.GetDeclaredSymbol(tds) as INamedTypeSymbol;
                node = node.Parent;
            }
            if (typeSym == null) return names.ToList();
            foreach (var sym in semantic.LookupSymbols(position).Where(s => !s.IsStatic && s.ContainingType != null))
            {
                var ct = sym.ContainingType;
                bool inHierarchy = ct != null && (SymbolEqualityComparer.Default.Equals(ct, typeSym) || IsBaseOf(typeSym, ct));
                if (!inHierarchy) continue;
                switch (sym)
                {
                    case IMethodSymbol ms when ms.MethodKind == MethodKind.Ordinary: names.Add(ms.Name); break;
                    case IPropertySymbol ps: names.Add(ps.Name); break;
                    case IFieldSymbol fs when !fs.IsImplicitlyDeclared && !fs.Name.StartsWith("<"): names.Add(fs.Name); break;
                    case IEventSymbol es: names.Add(es.Name); break;
                }
            }
            return names.ToList();
            static bool IsBaseOf(INamedTypeSymbol derived, INamedTypeSymbol candidate)
            {
                var b = derived.BaseType;
                while (b != null)
                {
                    if (SymbolEqualityComparer.Default.Equals(b, candidate)) return true;
                    b = b.BaseType;
                }
                return false;
            }
        }
        public async Task<IReadOnlyList<string>> EnumerateHierarchyInstanceMembersAsync(RoslynDocument document, string text, int position)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            if (_ws == null || _project == null) return names.ToList();
            await UpdateOpenDocumentAsync(document, text);
            var doc = document;
            if (doc == null) return names.ToList();
            var semantic = await doc.GetSemanticModelAsync();
            var root = await doc.GetSyntaxRootAsync();
            if (semantic == null || root == null) return names.ToList();
            var token = root.FindToken(Math.Min(position, root.FullSpan.End - 1));
            var node = token.Parent;
            INamedTypeSymbol? typeSym = null;
            while (node != null && typeSym == null)
            {
                if (node is TypeDeclarationSyntax tds)
                    typeSym = semantic.GetDeclaredSymbol(tds) as INamedTypeSymbol;
                node = node?.Parent;
            }
            if (typeSym == null) return names.ToList();
            void Add(INamedTypeSymbol t)
            {
                foreach (var member in t.GetMembers())
                {
                    if (member.IsImplicitlyDeclared) continue;
                    if (member.IsStatic) continue;
                    if (member.DeclaredAccessibility == AccessibilityCode.Private && !SymbolEqualityComparer.Default.Equals(t, typeSym)) continue;
                    switch (member)
                    {
                        case IPropertySymbol p: names.Add(p.Name); break;
                        case IFieldSymbol f when !f.Name.StartsWith("<"): names.Add(f.Name); break;
                        case IMethodSymbol m when m.MethodKind == MethodKind.Ordinary: names.Add(m.Name); break;
                        case IEventSymbol e: names.Add(e.Name); break;
                    }
                }
            }
            for (var t = typeSym; t != null && t.SpecialType != SpecialType.System_Object; t = t.BaseType)
                Add(t);
            return names.ToList();
        }
        public async Task<IReadOnlyList<string>> FindTypeCandidatesAsync(RoslynDocument document, string text, string prefix)
        {
            var list = new HashSet<string>(StringComparer.Ordinal);
            if (_ws == null || _project == null || string.IsNullOrWhiteSpace(prefix)) return list.ToList();
            await UpdateOpenDocumentAsync(document, text);
            var doc = document;
            if (doc == null) return list.ToList();
            var root = await doc.GetSyntaxRootAsync();
            var semantic = await doc.GetSemanticModelAsync();
            if (root == null || semantic == null) return list.ToList();

            string currentNs = "";
            var nsDecl = root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
            if (nsDecl != null)
                currentNs = nsDecl.Name.ToString();
            else
            {
                var blockNs = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
                if (blockNs != null) currentNs = blockNs.Name.ToString();
            }

            var fileUsings = root.DescendantNodes().OfType<UsingDirectiveSyntax>()
                                  .Select(u => u.Name.ToString())
                                  .ToHashSet(StringComparer.Ordinal);

            var compilation = semantic.Compilation;
            void VisitNamespace(INamespaceSymbol ns)
            {
                foreach (var member in ns.GetMembers())
                {
                    if (member is INamespaceSymbol childNs)
                    {
                        VisitNamespace(childNs);
                    }
                    else if (member is INamedTypeSymbol typeSym)
                    {
                        if (typeSym.Name.Length == 0) continue;
                        if (!typeSym.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
                        if (typeSym.TypeKind == TypeKind.Error) continue;
                        if (typeSym.DeclaredAccessibility is not (AccessibilityCode.Public or AccessibilityCode.Internal)) continue;
                        if (typeSym.DeclaredAccessibility == AccessibilityCode.Internal && !SymbolEqualityComparer.Default.Equals(typeSym.ContainingAssembly, compilation.Assembly))
                            continue;
                        var typeNs = typeSym.ContainingNamespace?.ToDisplayString() ?? string.Empty;
                        string shortName = typeSym.Name;
                        string fullyQualified = typeSym.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", string.Empty);
                        if (string.Equals(typeNs, currentNs, StringComparison.Ordinal) || string.IsNullOrEmpty(typeNs))
                        {
                            list.Add(shortName);
                        }
                        else
                        {
                            bool imported = fileUsings.Contains(typeNs);
                            if (imported) list.Add(shortName); // allow short name because namespace imported
                            string minimal = typeNs + "." + shortName;
                            list.Add(minimal);
                            list.Add(fullyQualified);
                        }
                    }
                }
            }
            VisitNamespace(compilation.GlobalNamespace);
            return list.ToList();
        }
        public async Task<string?> AddUsingIfUniqueTypeAsync(RoslynDocument document, string text, string typeName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(typeName) || _ws == null || _project == null) return null;
                await UpdateOpenDocumentAsync(document, text);
                var doc = document;
                if (doc == null) return null;
                var semantic = await doc.GetSemanticModelAsync();
                var root = await doc.GetSyntaxRootAsync() as CSharpSyntaxNode;
                if (semantic == null || root == null) return null;
                // Already resolvable?
                var existing = semantic.LookupNamespacesAndTypes(root.FullSpan.Start, name: typeName);
                if (existing.Any(sym => sym is INamedTypeSymbol)) return null; // already available
                // Collect candidate types (public/internal) by traversing namespaces
                var compilation = semantic.Compilation;
                var candidates = new List<INamedTypeSymbol>();
                void VisitNamespace(INamespaceSymbol ns)
                {
                    foreach (var m in ns.GetMembers())
                    {
                        if (m is INamespaceSymbol child) VisitNamespace(child);
                        else if (m is INamedTypeSymbol t && t.Name.Equals(typeName, StringComparison.Ordinal))
                        {
                            if (t.DeclaredAccessibility is AccessibilityCode.Public or AccessibilityCode.Internal)
                            {
                                if (t.DeclaredAccessibility == AccessibilityCode.Internal && !SymbolEqualityComparer.Default.Equals(t.ContainingAssembly, compilation.Assembly)) continue;
                                candidates.Add(t);
                            }
                        }
                    }
                }
                VisitNamespace(compilation.GlobalNamespace);
                if (candidates.Count != 1) return null; // only auto if unique
                var target = candidates[0];
                var nsName = target.ContainingNamespace?.ToDisplayString() ?? string.Empty;
                if (string.IsNullOrEmpty(nsName)) return null;
                // Already has using?
                if (root.DescendantNodes().OfType<UsingDirectiveSyntax>().Any(u => u.Name.ToString() == nsName)) return null;
                // Insert using (keep at top, after existing usings, before namespace/type decl)
                var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(nsName)).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
                var firstNonUsing = root.ChildNodes().FirstOrDefault(n => n is not UsingDirectiveSyntax && n.Kind() != SyntaxKind.ShebangDirectiveTrivia);
                var usings = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToList();
                SyntaxNode newRoot;
                if (usings.Count > 0)
                {
                    var lastUsing = usings.Last();
                    newRoot = root.InsertNodesAfter(lastUsing, new[] { usingDirective });
                }
                else if (firstNonUsing != null)
                {
                    newRoot = root.InsertNodesBefore(firstNonUsing, new[] { usingDirective });
                }
                else
                {
                    // root has no usings; prepend at top
                    newRoot = root.WithLeadingTrivia(usingDirective.GetLeadingTrivia())
                                   .InsertNodesBefore(root.ChildNodes().FirstOrDefault()!, new[] { usingDirective });
                }
                var newText = newRoot.NormalizeWhitespace().ToFullString();
                return newText;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AddUsingIfUniqueTypeAsync error: " + ex.Message);
                return null;
            }
        }
        public static async Task<string?> FormatCSharpAsync(string source, bool useTabs, int indentSize, CancellationToken ct = default)
        {
            try
            {
                // Workspace mit Features-Host (siehe CreateAdhocWorkspace in RoslynServices)
                using var workspace = CreateAdhocWorkspace();


                // Optionen (ähnlich VS/VSCode)
                var options = workspace.Options
                .WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, useTabs)
                .WithChangedOption(FormattingOptions.IndentationSize, LanguageNames.CSharp, indentSize)
                .WithChangedOption(FormattingOptions.TabSize, LanguageNames.CSharp, indentSize);


                // Parse- und Compilation-Optionen
                var parse = new CSharpParseOptions(LanguageVersion.Preview);
                var comp = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);


                // Adhoc-Projekt + Dokument aufbauen
                var projId = ProjectId.CreateNewId();
                var docId = DocumentId.CreateNewId(projId);


                var solution = workspace.CurrentSolution
                .AddProject(projId, "AdhocProject", "AdhocProject", LanguageNames.CSharp)
                .WithProjectParseOptions(projId, parse)
                .WithProjectCompilationOptions(projId, comp)
                .AddDocument(docId, "Temp.cs", SourceText.From(source, Encoding.UTF8));


                if (!workspace.TryApplyChanges(solution))
                    return null;


                var document = workspace.CurrentSolution.GetDocument(docId)!;


                // Formatieren (reicht für Whitespaces/Einrückungen/Braces etc.)
                var formattedDoc = await Formatter.FormatAsync(document, options, ct).ConfigureAwait(false);
                var formattedText = await formattedDoc.GetTextAsync(ct).ConfigureAwait(false);
                return formattedText.ToString();
            }
            catch
            {
                return null; // Fallback zulassen
            }
        }

        public List<RoslynDocument> GetAllDocuments()
        {
            return _adhocWs?.CurrentSolution.Projects
                .SelectMany(p => p.Documents)
                .ToList() ?? new List<RoslynDocument>();
        }


        public List<string> ErrorFiles = new List<string>();

        Stopwatch buildWatch = new Stopwatch();

        public int BuildDuration = 0;
        public string BuildResult = "";
        public bool BuildSuccess = false;

        public async Task<Assembly?> BuildAssemblyAsync()
        {
            Debug.WriteLine("=== Build Assembly");
            Debug.WriteLine("[Diag] Id  =" + _project?.Id);
            Debug.WriteLine("[Diag] Docs=" + _project?.Documents.Count());
            Debug.WriteLine("[Diag] Has Program.cs=" + (_project?.Documents.Any(d => d.Name == "Program.cs")));

           

            lock (_buildLock)
            {
                if (_isBuildingAssembly) return null;
                _isBuildingAssembly = true;
            }
            try
            {
                if (_projectId == null || _adhocWs == null)
                {
                    Debug.WriteLine("[Roslyn] BuildAssemblyAsync: no projectId/workspace.");
                    BuildSuccess = false;
                    return null;
                }

                // Hole immer gezielt das aktuelle Projekt über _projectId (nicht FirstOrDefault)
                var project = _adhocWs.CurrentSolution.GetProject(_projectId);
                if (project == null)
                {
                    Debug.WriteLine("[Roslyn] BuildAssemblyAsync: projectId not found in CurrentSolution.");
                    BuildSuccess = false;
                    return null;
                }

                // Merke aktuelle Projekt-ID/Doc-Anzahl zur Diagnose
                Debug.WriteLine($"[Build] Using ProjectId={project.Id} Docs={project.Documents.Count()}");

                var compilation = await project.GetCompilationAsync();
                if (compilation == null)
                {
                    Debug.WriteLine("[Roslyn] BuildAssemblyAsync: compilation null.");
                    BuildSuccess = false;
                    return null;
                }

                compilation = compilation.WithOptions(
                    compilation.Options.WithOptimizationLevel(OptimizationLevel.Debug));

                using var peStream = new MemoryStream();
                using var pdbStream = new MemoryStream();

                var emitResult = compilation.Emit(
                    peStream,
                    pdbStream,
                    options: new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb));

                if (!emitResult.Success)
                {
                    var diags = emitResult.Diagnostics
                        .Where(d => d.Severity == DiagnosticSeverity.Error)
                        .ToList();

                    ErrorFiles = diags
                        .Select(d => d.Location.GetMappedLineSpan().Path)
                        .Where(p => !string.IsNullOrEmpty(p))
                        .Distinct()
                        .ToList();

                    string log =

                    "[Roslyn] Emit failed:\n" +
                        string.Join("\n", diags.Select(d =>
                        {
                            var span = d.Location.GetMappedLineSpan();
                            return $"{Path.GetFileName(span.Path)}({span.StartLinePosition.Line + 1},{span.StartLinePosition.Character + 1}): {d.Id}: {d.GetMessage()}";
                        }));

             
                    Debug.WriteLine(log);

                    BuildSuccess = false;
                    return null;
                }

                peStream.Position = 0;
                pdbStream.Position = 0;
                var asm = Assembly.Load(peStream.ToArray(), pdbStream.ToArray());
                BuildSuccess = true;
                return asm;
            }
            finally
            {
                lock (_buildLock) _isBuildingAssembly = false;
            }
        }


        //private static bool IsManagedAssembly(string path)
        //{
        //    try
        //    {
        //        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
        //        using (var br = new BinaryReader(fs))
        //        {
        //            if (fs.Length < 0x3C + 4) return false;
        //            fs.Position = 0x3C;
        //            int peHeaderOffset = br.ReadInt32();
        //            if (peHeaderOffset + 0x18 + 2 > fs.Length) return false;
        //            fs.Position = peHeaderOffset + 0x18;
        //            ushort magic = br.ReadUInt16();
        //            long pos = (magic == 0x010b) ? peHeaderOffset + 0xF8 : peHeaderOffset + 0x108;
        //            if (pos + 0x70 + 8 > fs.Length) return false;
        //            fs.Position = pos + 0x70;
        //            uint cliHeaderRva = br.ReadUInt32();
        //            uint cliHeaderSize = br.ReadUInt32();
        //            return cliHeaderRva != 0 && cliHeaderSize != 0;
        //        }
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}


        #region Auto-Complete

        public async Task<IReadOnlyList<string>> GetAutoCompleteSuggestionsAsync(
            RoslynDocument document,
            string text,
            int caretPosition,
            string prefix)
        {
            if (document == null) return Array.Empty<string>();

            var updatedDoc = document.WithText(SourceText.From(text, Encoding.UTF8));
            var (items, _) = await GetCompletionsAsync(updatedDoc, caretPosition);

            if (items == null || items.Length == 0) return Array.Empty<string>();

            return items
                .Select(i => i.DisplayText)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(s => string.IsNullOrEmpty(prefix) || s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s)
                .ToList();
        }

        public async Task<IReadOnlyList<qbookCode.Controls.CompletionItem>> GetFullAutoCompleteSuggestionsAsync(
    RoslynDocument document,
    string text,
    int caretPosition,
    string prefix)
        {
            if (document == null) return Array.Empty<qbookCode.Controls.CompletionItem>();

            var updatedDoc = document.WithText(SourceText.From(text, Encoding.UTF8));
            var (items, _) = await GetCompletionsAsync(updatedDoc, caretPosition);

            if (items == null || items.Length == 0) return Array.Empty<qbookCode.Controls.CompletionItem>();

            var semanticModel = await updatedDoc.GetSemanticModelAsync();
            var root = await updatedDoc.GetSyntaxRootAsync();

            var result = new List<qbookCode.Controls.CompletionItem>();

            foreach (var item in items)
            {
                // Filter nach Prefix
                if (!string.IsNullOrEmpty(prefix) && !item.DisplayText.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                string? fqName = null;

                // Versuche das Symbol zu bekommen (z.B. für Methoden)
                if (semanticModel != null && root != null)
                {
                    var completionChange = await CompletionService.GetService(updatedDoc)
                        .GetChangeAsync(updatedDoc, item);

                    // Versuche, die Position des eingefügten Textes zu bestimmen
                    int pos = caretPosition;
                    if (completionChange != null)
                    {
                        pos = completionChange.TextChange.Span.Start;
                    }

                    var token = root.FindToken(Math.Max(0, pos - 1));
                    var node = token.Parent;

                    // Versuche, das Symbol zu bekommen
                    ISymbol? symbol = null;
                    if (item.Properties.TryGetValue("SymbolId", out var symbolId))
                    {
                        // SymbolId ist nicht immer verfügbar, daher heuristisch:
                        symbol = semanticModel.LookupSymbols(pos, name: item.DisplayText).FirstOrDefault();
                    }
                    else
                    {
                        symbol = semanticModel.LookupSymbols(pos, name: item.DisplayText).FirstOrDefault();
                    }

                    if (symbol is IMethodSymbol ms)
                    {
                        fqName = ms.ToDisplayString();
                    }
                    else if (symbol != null)
                    {
                        fqName = symbol.ToDisplayString();
                    }
                }

                result.Add(new qbookCode.Controls.CompletionItem()
                {
                    Text = item.DisplayText,
                    Value = item.FilterText ?? item.DisplayText,
                    FullyQualifiedName = fqName,
                    // Icon etc. kann hier ergänzt werden
                });
            }

            // Optional: Duplikate entfernen (z.B. nach fqName)
            return result
                .GroupBy(c => c.FullyQualifiedName ?? c.Text)
                .Select(g => g.First())
                .OrderBy(c => c.Text)
                .ToList();
        }

        #endregion

        #region Signature Help

        public class SignatureParameter
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string DefaultValue { get; set; }
            public string CurrentValue { get; set; }
        }

        public static async Task<string?> GetFullQualityNameOfCarretAsync(RoslynDocument doc, int position)
        {
            var root = await doc.GetSyntaxRootAsync().ConfigureAwait(false);
            if (root == null) return null;

            var token = root.FindToken(position);
            var node = token.Parent;

            // Find the invocation or creation expression
            var expression = node?.AncestorsAndSelf().OfType<ExpressionSyntax>()
                .FirstOrDefault(e => e is InvocationExpressionSyntax || e is ObjectCreationExpressionSyntax);

            if (expression == null) return null;

            var model = await doc.GetSemanticModelAsync().ConfigureAwait(false);
            if (model == null) return null;

            var symbolInfo = model.GetSymbolInfo(expression);
            var symbol = symbolInfo.Symbol;

            // For generic methods, we might get a candidate symbol list
            if (symbol == null && symbolInfo.CandidateSymbols.Any())
            {
                symbol = symbolInfo.CandidateSymbols.FirstOrDefault();
            }

            return symbol?.ToDisplayString();
        }

        public async Task<IReadOnlyList<SignatureParameter>> GetSignatureParametersAsync(RoslynDocument document, int caretPosition)
        {
            if (document == null) return Array.Empty<SignatureParameter>();

            var tree = await document.GetSyntaxTreeAsync();
            if (tree == null) return Array.Empty<SignatureParameter>();

            var root = await tree.GetRootAsync();
            var token = root.FindToken(Math.Max(0, caretPosition - 1));
            var node = token.Parent;

            while (node != null)
            {
                if (node is InvocationExpressionSyntax inv && inv.ArgumentList != null)
                    return await ExtractParametersAsync(inv.ArgumentList, document, inv);

                if (node is ObjectCreationExpressionSyntax obj && obj.ArgumentList != null)
                    return await ExtractParametersAsync(obj.ArgumentList, document, obj);

                node = node.Parent;
            }

            return Array.Empty<SignatureParameter>();
        }

        private async Task<IReadOnlyList<SignatureParameter>> ExtractParametersAsync(ArgumentListSyntax argumentList, RoslynDocument doc, SyntaxNode node)
        {
            var model = await doc.GetSemanticModelAsync();
            if (model == null) return Array.Empty<SignatureParameter>();

            IMethodSymbol? methodSymbol = null;

            if (node is InvocationExpressionSyntax inv)
            {
                var info = model.GetSymbolInfo(inv);
                methodSymbol = info.Symbol as IMethodSymbol ?? info.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
            }
            else if (node is ObjectCreationExpressionSyntax obj)
            {
                var type = model.GetTypeInfo(obj).Type as INamedTypeSymbol ??
                           model.GetSymbolInfo(obj.Type).Symbol as INamedTypeSymbol;
                methodSymbol = type?.InstanceConstructors.FirstOrDefault();
            }

            if (methodSymbol == null) return Array.Empty<SignatureParameter>();

            var usedParams = argumentList.Arguments
                .Select(arg => arg.NameColon?.Name.Identifier.ValueText)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToHashSet();

            var list = new List<SignatureParameter>();
            foreach (var param in methodSymbol.Parameters)
            {
                if (usedParams.Contains(param.Name)) continue;

                string defaultValue = param.HasExplicitDefaultValue ? param.ExplicitDefaultValue?.ToString() ?? "null" : "????";
                string type = param.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                string currentValue = argumentList.Arguments.FirstOrDefault(a => a.NameColon?.Name.Identifier.ValueText == param.Name)?.Expression.ToString() ?? defaultValue;

                list.Add(new SignatureParameter
                {
                    Name = param.Name,
                    Type = type,
                    DefaultValue = defaultValue,
                    CurrentValue = currentValue
                });
            }

            return list;
        }


        #endregion









    }

    public static class SpanMapper
    {
        /// <summary>
        /// Überträgt Roslyn-Spans in Scintilla-Markierungen – offsetbasiert und tab-korrigiert.
        /// </summary>
        public static Task ApplyBucketsAsync(Scintilla editor, int roslynLength, Dictionary<int, List<TextSpan>> buckets)
        {
            string editorText = editor.Text;
            int tabWidth = editor.TabWidth;

            foreach (var kvp in buckets)
            {
                editor.IndicatorCurrent = kvp.Key;
                editor.IndicatorClearRange(0, editor.TextLength); // optional: nur wenn nötig

                foreach (var span in kvp.Value)
                {
                    int start = Clamp(span.Start, 0, editor.TextLength);
                    int length = Clamp(span.Length, 0, editor.TextLength - start);
                    editor.IndicatorFillRange(start, length);
                }
            }

            return Task.CompletedTask;
        }

        public static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }



        /// <summary>
        /// Übersetzt Roslyn-Offset in visuelle Scintilla-Position unter Berücksichtigung von Tabs.
        /// </summary>
        private static int RoslynOffsetToScintillaPosition(string text, int offset, int tabWidth)
        {
            int position = 0;
            for (int i = 0; i < offset && i < text.Length; i++)
            {
                if (text[i] == '\t')
                {
                    int spacesToNextTabStop = tabWidth - (position % tabWidth);
                    position += spacesToNextTabStop;
                }
                else
                {
                    position++;
                }
            }
            return position;
        }
    }
}
