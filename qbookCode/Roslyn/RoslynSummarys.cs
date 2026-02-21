using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static ScintillaNET.Style;
using RoslynDocument = Microsoft.CodeAnalysis.Document;

namespace qbookCode.Roslyn
{
    public class MethodDocumentations
    {
        public string Summary { get; set; } = "";
        public Dictionary<string, string> Parameters { get; } = new();
        public string Returns { get; set; } = "";

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(Summary))
            {
                sb.AppendLine(Summary);
            }
            foreach (var param in Parameters)
            {
                sb.AppendLine($"PARAM {param.Key}: {param.Value}");
            }
            if (!string.IsNullOrWhiteSpace(Returns))
            {
                sb.AppendLine($"RETURNS: {Returns}");
            }
            return sb.ToString().Trim();
        }
    }

    public static class RoslynSummarys
    {
        // Key: string key (DisplayString and/or DocumentationCommentId), Value: structured documentation
        private static readonly ConcurrentDictionary<string, MethodDocumentations> _documentation = new();

        public static void Clear() => _documentation.Clear();

        public static void AddOrUpdate(string key, MethodDocumentations documentation)
            => _documentation[key] = documentation;

        public static string GetSummary(string key)
            => _documentation.TryGetValue(key, out var documentation) ? documentation.Summary : null;

        public static MethodDocumentations? GetMethodDocumentation(string key)
            => _documentation.TryGetValue(key, out var documentation) ? documentation : null;

        // Neue, robuste Varianten: erst DocumentationCommentId, dann Fallback auf DisplayString
        public static string? GetSummary(ISymbol symbol) => GetMethodDocumentation(symbol)?.Summary;

        public static MethodDocumentations? GetMethodDocumentation(ISymbol symbol)
        {
            if (symbol == null) return null;

            var docId = symbol.GetDocumentationCommentId();
            if (!string.IsNullOrWhiteSpace(docId) &&
                _documentation.TryGetValue(docId, out var byDocId))
            {
                return byDocId;
            }

            var display = symbol.ToDisplayString();
            if (!string.IsNullOrWhiteSpace(display) &&
                _documentation.TryGetValue(display, out var byDisplay))
            {
                return byDisplay;
            }

            return null;
        }

        private static void AddOrUpdateForSymbol(ISymbol symbol, MethodDocumentations documentation)
        {
            if (symbol == null || documentation == null) return;

            // Für bestehende Aufrufer (ToDisplayString-Lookups)
            var display = symbol.ToDisplayString();
            if (!string.IsNullOrWhiteSpace(display))
            {
                AddOrUpdate(display, documentation);
            }

            // Für XML-Doku / robustes Matching
            var docId = symbol.GetDocumentationCommentId();
            if (!string.IsNullOrWhiteSpace(docId))
            {
                AddOrUpdate(docId, documentation);
            }
        }

        // Hilfsmethode: Füllt den Cache für ein Dokument (Source-Code)
        public static async Task CollectSummariesAsync(Document doc)
        {
            if (doc == null) return;
            var root = await doc.GetSyntaxRootAsync().ConfigureAwait(false);
            if (root == null) return;
            var model = await doc.GetSemanticModelAsync().ConfigureAwait(false);
            if (model == null) return;

            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var symbol = model.GetDeclaredSymbol(method);
                if (symbol == null) continue;

                var xml = symbol.GetDocumentationCommentXml();
                if (!string.IsNullOrWhiteSpace(xml))
                {
                    var documentation = ExtractDocumentation(xml);
                    if (documentation != null)
                        AddOrUpdateForSymbol(symbol, documentation);
                }
            }
        }

        // Extracts <summary>, <param>, and <returns> into a structured object
        private static MethodDocumentations? ExtractDocumentation(string xml)
        {
            try
            {
                var x = XElement.Parse("<root>" + xml + "</root>");
                var doc = new MethodDocumentations();

                var summaryElem = x.Descendants("summary").FirstOrDefault();
                if (summaryElem != null)
                {
                    doc.Summary = NormalizeDocText(summaryElem.Value);
                }

                foreach (var paramElem in x.Descendants("param"))
                {
                    var paramName = paramElem.Attribute("name")?.Value;
                    if (!string.IsNullOrEmpty(paramName))
                    {
                        doc.Parameters[paramName] = NormalizeDocText(paramElem.Value);
                    }
                }

                var returnsElem = x.Descendants("returns").FirstOrDefault();
                if (returnsElem != null)
                {
                    doc.Returns = NormalizeDocText(returnsElem.Value);
                }

                if (string.IsNullOrWhiteSpace(doc.Summary) && doc.Parameters.Count == 0 && string.IsNullOrWhiteSpace(doc.Returns))
                {
                    return null;
                }

                return doc;
            }
            catch
            {
                return null;
            }
        }

        private static string NormalizeDocText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return string.Join(" ",
                text.Split(new[] { '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        public static async Task CollectAll(IEnumerable<string>? referenceIncludeList = null)
        {
            Clear();
            List<RoslynDocument> documents = Core.Roslyn.GetAllDocuments();

            foreach (var document in documents)
            {
                await CollectSummariesAsync(document).ConfigureAwait(false);
            }

            var project = Core.Roslyn.GetProject;
            if (project != null && referenceIncludeList != null)
            {
                await CollectSummariesFromReferencesAsync(project, referenceIncludeList).ConfigureAwait(false);
            }

            Debug.WriteLine($"--------[Adding Summaries]--------------");
            foreach (var item in _documentation)
            {
                Debug.WriteLine($"{item.Key}");
            }
        }

        private static async Task CollectSummariesFromReferencesAsync(Project project, IEnumerable<string> includedAssemblies)
        {
            if (project == null)
            {
                Debug.WriteLine("[Summaries] Project is null. Cannot load from references.");
                return;
            }

            var compilation = await project.GetCompilationAsync().ConfigureAwait(false);
            if (compilation == null)
            {
                Debug.WriteLine("[Summaries] Compilation is null. Cannot load from references.");
                return;
            }

            var includeSet = new HashSet<string>(includedAssemblies, StringComparer.OrdinalIgnoreCase);
            Debug.WriteLine($"[Summaries] Include list for summaries: {string.Join(", ", includeSet)}");

            Debug.WriteLine($"[Summaries] Checking {project.MetadataReferences.Count} metadata references for summaries...");

            foreach (var reference in project.MetadataReferences)
            {
                if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assemblySymbol)
                {
                    Debug.WriteLine($"[Summaries] Skipping reference (not an assembly): {reference.Display}");
                    continue;
                }

                if (!includeSet.Contains(assemblySymbol.Name))
                {
                    continue;
                }

                Debug.WriteLine($"[Summaries] ==> Loading summaries from reference: {assemblySymbol.Name}");

                // 1) Bevorzugt: XML-Datei direkt importieren (robust, unabhängig von DocumentationProvider)
                var loadedFromXml = TryLoadReferenceXml(reference, assemblySymbol.Name);

                // 2) Fallback: Roslyn traversal (funktioniert nur, wenn DocumentationProvider korrekt gebunden ist)
                if (!loadedFromXml)
                {
                    Debug.WriteLine($"[Summaries] XML import failed/not found for {assemblySymbol.Name}. Fallback to Roslyn symbol traversal.");
                    TraverseNamespace(assemblySymbol.GlobalNamespace);
                }
            }

            Debug.WriteLine("[Summaries] Finished checking references for summaries.");
        }

        private static bool TryLoadReferenceXml(MetadataReference reference, string assemblyName)
        {
            try
            {
                if (reference is not PortableExecutableReference peRef)
                {
                    Debug.WriteLine($"[Summaries] Reference is not PortableExecutableReference: {reference.Display}");
                    return false;
                }

                var dllPath = peRef.FilePath;
                if (string.IsNullOrWhiteSpace(dllPath))
                {
                    Debug.WriteLine($"[Summaries] No FilePath for reference: {assemblyName}");
                    return false;
                }

                var xmlPath = Path.ChangeExtension(dllPath, ".xml");
                if (!File.Exists(xmlPath))
                {
                    Debug.WriteLine($"[Summaries] XML file not found: {xmlPath}");
                    return false;
                }

                var added = LoadSummariesFromXml(xmlPath);
                Debug.WriteLine($"[Summaries] Loaded {added} XML summaries from: {xmlPath}");
                return added > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Summaries] Error loading XML for {assemblyName}: {ex.Message}");
                return false;
            }
        }

        // Liest eine XML-Dokumentationsdatei direkt ein und speichert unter DocumentationCommentId-Keys (M:, P:, T:, ...)
        private static int LoadSummariesFromXml(string xmlPath)
        {
            var added = 0;
            var xdoc = XDocument.Load(xmlPath);

            foreach (var memberElem in xdoc.Descendants("member"))
            {
                var key = memberElem.Attribute("name")?.Value;
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                var doc = new MethodDocumentations();

                var summaryElem = memberElem.Element("summary");
                if (summaryElem != null)
                    doc.Summary = NormalizeDocText(summaryElem.Value);

                foreach (var paramElem in memberElem.Elements("param"))
                {
                    var paramName = paramElem.Attribute("name")?.Value;
                    if (!string.IsNullOrWhiteSpace(paramName))
                        doc.Parameters[paramName] = NormalizeDocText(paramElem.Value);
                }

                var returnsElem = memberElem.Element("returns");
                if (returnsElem != null)
                    doc.Returns = NormalizeDocText(returnsElem.Value);

                if (string.IsNullOrWhiteSpace(doc.Summary) && doc.Parameters.Count == 0 && string.IsNullOrWhiteSpace(doc.Returns))
                    continue;

                AddOrUpdate(key, doc); // key ist DocumentationCommentId
                added++;
            }

            return added;
        }

        private static void TraverseNamespace(INamespaceSymbol namespaceSymbol)
        {
            foreach (var typeMember in namespaceSymbol.GetTypeMembers())
            {
                TraverseType(typeMember);
            }

            foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
            {
                TraverseNamespace(childNamespace);
            }
        }

        private static void TraverseType(INamedTypeSymbol typeSymbol)
        {
            // Filter out compiler-generated types
            if (typeSymbol.IsImplicitlyDeclared)
            {
                return;
            }

            foreach (var member in typeSymbol.GetMembers())
            {
                // Filter out compiler-generated or non-referenceable members
                if (member.IsImplicitlyDeclared || !member.CanBeReferencedByName)
                {
                    continue;
                }

                switch (member.Kind)
                {
                    case SymbolKind.Method:
                    case SymbolKind.Property:
                    case SymbolKind.Field:
                    case SymbolKind.Event:
                        var xml = member.GetDocumentationCommentXml();

                        if (string.IsNullOrWhiteSpace(xml))
                        {
                            Debug.WriteLine($"[Summaries] No XML doc for: {member.ToDisplayString()}");
                            break;
                        }

                        Debug.WriteLine($"[Summaries] XML found for: {member.ToDisplayString()}");

                        var documentation = ExtractDocumentation(xml);
                        if (documentation != null)
                        {
                            AddOrUpdateForSymbol(member, documentation);
                        }
                        break;
                }
            }

            // Recurse for nested types
            foreach (var nestedType in typeSymbol.GetTypeMembers())
            {
                TraverseType(nestedType);
            }
        }
    }
}
