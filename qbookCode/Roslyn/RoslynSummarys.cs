using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks;
using RoslynDocument = Microsoft.CodeAnalysis.Document;
using System.Diagnostics;
using System.Xml.Linq;
using System.Text;

namespace qbookCode.Roslyn
{
    public class MethodDocumentation
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
        // Key: fully qualified method name, Value: structured documentation
        private static readonly ConcurrentDictionary<string, MethodDocumentation> _documentation = new();

        public static void Clear() => _documentation.Clear();

        public static void AddOrUpdate(string fullyQualifiedName, MethodDocumentation documentation)
            => _documentation[fullyQualifiedName] = documentation;

        public static string GetSummary(string fullyQualifiedName)
            => _documentation.TryGetValue(fullyQualifiedName, out var documentation) ? documentation.Summary : null;

        public static MethodDocumentation? GetMethodDocumentation(string fullyQualifiedName)
            => _documentation.TryGetValue(fullyQualifiedName, out var documentation) ? documentation : null;

        // Hilfsmethode: Füllt den Cache für ein Dokument
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
                    var fqName = symbol.ToDisplayString(); // z.B. Namespace.Class.Method
                    var documentation = ExtractDocumentation(xml);
                    if (documentation != null)
                        AddOrUpdate(fqName, documentation);
                }
            }
        }

        // Extracts <summary>, <param>, and <returns> into a structured object
        private static MethodDocumentation? ExtractDocumentation(string xml)
        {
            try
            {
                var x = XElement.Parse("<root>" + xml + "</root>");
                var doc = new MethodDocumentation();

                var summaryElem = x.Descendants("summary").FirstOrDefault();
                if (summaryElem != null)
                {
                    doc.Summary = summaryElem.Value.Trim();
                }

                foreach (var paramElem in x.Descendants("param"))
                {
                    var paramName = paramElem.Attribute("name")?.Value;
                    if (!string.IsNullOrEmpty(paramName))
                    {
                        doc.Parameters[paramName] = paramElem.Value.Trim();
                    }
                }

                var returnsElem = x.Descendants("returns").FirstOrDefault();
                if (returnsElem != null)
                {
                    doc.Returns = returnsElem.Value.Trim();
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

        public static async Task CollectAll()
        {
            Clear();
            List<RoslynDocument> documents = Core.Roslyn.GetAllDocuments();

            foreach (var document in documents)
            {
                await CollectSummariesAsync(document).ConfigureAwait(false);
            }
            Debug.WriteLine("---------------");
            foreach (var item in _documentation)
                Debug.WriteLine(item.Key + " : " + item.Value.ToString());
        }
    }
}
