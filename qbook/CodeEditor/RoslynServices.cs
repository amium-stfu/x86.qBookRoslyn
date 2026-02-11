using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Elfie.Model; // Alias gegen Kollision mit ScintillaNET.Document
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.SolutionPersistence.Model;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AccessibilityCode = Microsoft.CodeAnalysis.Accessibility;
using RoslynDocument = Microsoft.CodeAnalysis.Document;

namespace qbook.CodeEditor
{
    public sealed partial class RoslynServices
    {

        private MSBuildWorkspace? _ws;
        private Project? _project;
        private readonly SemaphoreSlim _loadSemaphore = new(1, 1); 
        private bool _isLoading;

 
        private bool _useInMemory = true;
        private AdhocWorkspace? _adhocWs;
        private Dictionary<string, DocumentId> _docMap = new();

        // NEW: cache a single MEF host to avoid repeatedly allocating composition containers
        private static readonly HostServices s_host = CreateMefHost();

        // NEW: cache metadata references by path so we don’t re-open PE files repeatedly
        private static readonly object s_refLock = new();
        private static volatile List<MetadataReference>? s_cachedReferences;
        

        public bool IsProjectLoaded => _project != null && ( (_useInMemory && _adhocWs != null) || (!_useInMemory && _ws != null) );
        public async Task LoadProjectAsync(string csprojPath)
        {
            QB.Logger.Debug("LoadProjectAsync: '" + csprojPath + "'");

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
                        QB.Logger.Error("[RoslynServices]: LoadProjectAsync MSBuildLocator registration failed: " + ex.Message);
                    }
                }
                var props = new Dictionary<string, string> { { "DesignTimeBuild", "true" }, { "BuildingInsideVisualStudio", "true" } };



                _ws = MSBuildWorkspace.Create(props);

                _ws.WorkspaceFailed += (s, e) => QB.Logger.Error($"[WorkspaceFailed] {e.Diagnostic.Kind}: {e.Diagnostic.Message}");
                try
                {
                    _project = await _ws.OpenProjectAsync(csprojPath);
                }
                catch (Exception ex)
                {
                    QB.Logger.Error("OpenProjectAsync failed: " + ex.Message);
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

        public void Reset()
        {

            if (_ws != null && _workspaceFailedHandler != null)
                _ws.WorkspaceFailed -= _workspaceFailedHandler;


            _ws?.Dispose();
            _adhocWs?.Dispose();
            _ws = null;
            _adhocWs = null;
            _project = null;
            _docMap.Clear();
            _useInMemory = true;
        }


        private  List<MetadataReference> _referenceCache = new();
        private static List<MetadataReference> GetOrBuildDefaultReferences()
        {
            if (s_cachedReferences != null) return s_cachedReferences;
            lock (s_refLock)
            {
                if (s_cachedReferences != null) return s_cachedReferences;

                var refPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    typeof(object).Assembly.Location,
                    typeof(Console).Assembly.Location,
                    typeof(Enumerable).Assembly.Location,
                    typeof(System.Runtime.GCSettings).Assembly.Location
                };

                // Try optional desktop assemblies
                try
                {
                    refPaths.Add(typeof(System.Drawing.Point).Assembly.Location);
                    refPaths.Add(typeof(System.Windows.Forms.AccessibleEvents).Assembly.Location);
                    refPaths.Add(typeof(System.Diagnostics.Debug).Assembly.Location);
                }
                catch { }

                var netstandardPath = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "netstandard.dll");
                if (File.Exists(netstandardPath)) refPaths.Add(netstandardPath);

                // Collect DLLs from libs/
                string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs");
                if (Directory.Exists(baseDir))
                {
                    foreach (var path in Directory.GetFiles(baseDir, "*.dll"))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(path);
                        if (fileName.StartsWith("Microsoft.CodeAnalysis", StringComparison.OrdinalIgnoreCase)) continue;
                        refPaths.Add(path);
                    }
                }

                s_cachedReferences = refPaths
                    .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
                    .ToList();
                return s_cachedReferences;
            }
        }

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

            foreach(RoslynDocument doc in docs)
            {
                var text = await doc.GetTextAsync();
                _adhocWs.AddDocument(projectId, doc.Name, text);
            }

            _project = _adhocWs.CurrentSolution.GetProject(projectId);
        }

        public async Task LoadInMemoryProjectAsync(
             IEnumerable<(string fileName, string code)> files,
             IEnumerable<MetadataReference>? extraReferences = null)
        {
            // Instead of disposing MEF/Adhoc repeatedly, reset the solution and reuse the host
            if (_adhocWs == null)
            {
                _adhocWs = new AdhocWorkspace(s_host);
            }
            else
            {
                try { _adhocWs.ClearSolution(); } catch { }
            }

            var projectId = ProjectId.CreateNewId();

            var refs = new List<MetadataReference>();
            refs.AddRange(GetOrBuildDefaultReferences());
            if (extraReferences != null)
            {
                // de-duplicate by file path if possible
                var existingPaths = new HashSet<string>(refs.OfType<PortableExecutableReference>().Select(r => r.FilePath!), StringComparer.OrdinalIgnoreCase);
                foreach (var r in extraReferences.OfType<PortableExecutableReference>())
                {
                    if (r.FilePath is string fp && existingPaths.Contains(fp)) continue;
                    refs.Add(r);
                }
            }

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

            foreach (var (fileName, code) in files)
            {
                _adhocWs.AddDocument(projectId, fileName, SourceText.From(code));
            }

            _project = _adhocWs.CurrentSolution.GetProject(projectId);
        }

        public async Task<string?> GetDocumentTextAsync(string fileName)
        {
            var doc = GetDocument(fileName);
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


        public async Task IncludeDocument(string fileName, string code)
        {
            var projectId = _project.Id;

            // Prüfen, ob das Dokument schon existiert
            var existingDoc = _project.Documents.FirstOrDefault(d => d.Name == fileName);
            if (existingDoc != null)
                return; // Schon vorhanden

            _adhocWs.AddDocument(projectId, fileName, SourceText.From(code));
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
            _adhocWs.AddDocument(projectId, fileName, sourceText);
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



        public RoslynDocument? GetDocument(string fileName)
        {
            return _adhocWs.CurrentSolution.Projects
                .SelectMany(p => p.Documents)
                .FirstOrDefault(d => d.Name == fileName);
        }


        public async Task UpdateOpenDocumentAsync(RoslynDocument doc, string text)
        {
            if (doc == null) return;

            var sourceText = SourceText.From(text);
            var updatedDoc = doc.WithText(sourceText);

            _adhocWs.TryApplyChanges(updatedDoc.Project.Solution);
        }

        public async Task<(CompletionItem[] items, int spanStart)> GetCompletionsAsync(RoslynDocument doc, int caretPosition)
        {
            if (doc == null)
                return (new CompletionItem[0], caretPosition);

            var completionService = CompletionService.GetService(doc);
            if (completionService == null)
                return (new CompletionItem[0], caretPosition);

            var completions = await completionService.GetCompletionsAsync(doc, caretPosition);
            if (completions == null)
                return (new CompletionItem[0], caretPosition);

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
        public async Task<IReadOnlyList<string>> EnumerateHierarchyInstanceMembersAsync(RoslynDocument document , string text, int position)
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



    }

    public static class RoslynSpanMapper
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
