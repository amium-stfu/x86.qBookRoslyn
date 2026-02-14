using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using RoslynDocument = Microsoft.CodeAnalysis.Document; // Alias gegen Kollision mit ScintillaNET.Document

public static class RosylnSemantic
{
    private static readonly Dictionary<string, int> IndicatorMap = new(StringComparer.Ordinal)
    {
        [ClassificationTypeNames.MethodName] = 2,
        [ClassificationTypeNames.ExtensionMethodName] = 2,
        [ClassificationTypeNames.ClassName] = 3,
        ["record class name"] = 3,
        [ClassificationTypeNames.InterfaceName] = 4,
        [ClassificationTypeNames.StructName] = 5,
        [ClassificationTypeNames.EnumName] = 6,
        [ClassificationTypeNames.DelegateName] = 7,
        [ClassificationTypeNames.PropertyName] = 8,
        [ClassificationTypeNames.FieldName] = 9,
        [ClassificationTypeNames.Keyword] = 10,
        [ClassificationTypeNames.ControlKeyword] = 14,
        [ClassificationTypeNames.PreprocessorKeyword] = 14,
        [ClassificationTypeNames.NumericLiteral] = 11,
        [ClassificationTypeNames.StringLiteral] = 12,
        [ClassificationTypeNames.VerbatimStringLiteral] = 12,
        [ClassificationTypeNames.Comment] = 13,
        [ClassificationTypeNames.XmlDocCommentText] = 13,
        [ClassificationTypeNames.NamespaceName] = 15,
    };

    public static async Task ApplyAsync(Scintilla editor, RoslynDocument doc)
    {

        if (doc == null) return;
        var text = await doc.GetTextAsync();
        var span = new TextSpan(0, text.Length);
        var classifiedSpans = await Classifier.GetClassifiedSpansAsync(doc, span);

        int docLen = editor.TextLength;
        const int maxPerBucket = 4000;

        var buckets = new Dictionary<int, List<(int start, int length)>>();

        foreach (var c in classifiedSpans)
        {
            if (!IndicatorMap.TryGetValue(c.ClassificationType, out int indicator)) continue;

            int start = Math.Max(0, Math.Min(c.TextSpan.Start, docLen));
            int length = Math.Max(0, Math.Min(c.TextSpan.Length, docLen - start));
            if (length == 0) continue;

            if (!buckets.TryGetValue(indicator, out var list))
                buckets[indicator] = list = new List<(int, int)>();

            if (list.Count < maxPerBucket)
                list.Add((start, length));
        }

        foreach (var ind in buckets.Keys)
        {
            editor.IndicatorCurrent = ind;
            editor.IndicatorClearRange(0, docLen);
        }

        foreach (var kvp in buckets)
        {
            editor.IndicatorCurrent = kvp.Key;
            foreach (var (s, l) in kvp.Value)
                editor.IndicatorFillRange(s, l);
        }
    }

    public static async Task<DataTable> FindAllMethodReferencesAsync(RoslynDocument doc)
    {
        var table = new DataTable();
        // Schema: strings for names to enable filtering, plus location info
        table.Columns.Add("Method", typeof(string));           // Fully qualified (error format) for display
        table.Columns.Add("MethodName", typeof(string));       // Simple method name
        table.Columns.Add("ContainingType", typeof(string));   // Simple class/record/struct name
        table.Columns.Add("Document", typeof(string));
        table.Columns.Add("Line", typeof(int));
        table.Columns.Add("Span", typeof(string));
        table.Columns.Add("Code", typeof(string));

        if (doc == null) return table;

        var semanticModel = await doc.GetSemanticModelAsync().ConfigureAwait(false);
        var root = await doc.GetSyntaxRootAsync().ConfigureAwait(false);
        if (semanticModel == null || root == null) return table;

        var methodNodes = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
        var solution = doc.Project.Solution;

        foreach (var methodNode in methodNodes)
        {
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodNode);
            if (methodSymbol == null) continue;

            var refs = await SymbolFinder.FindReferencesAsync(methodSymbol, solution).ConfigureAwait(false);

            foreach (var r in refs)
            {
                foreach (var loc in r.Locations)
                {
                    var lineSpan = loc.Location.GetLineSpan();
                    var span = loc.Location.SourceSpan;

                    // 0-based line index for SourceText.Lines
                    int lineIndex = lineSpan.StartLinePosition.Line;

                    // Get the full line text
                    var text = await loc.Document.GetTextAsync().ConfigureAwait(false);
                    string lineText = lineIndex >= 0 && lineIndex < text.Lines.Count
                        ? text.Lines[lineIndex].ToString()
                        : string.Empty;

                    var row = table.NewRow();
                    row["Method"] = methodSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
                    row["MethodName"] = methodSymbol.Name;
                    row["ContainingType"] = methodSymbol.ContainingType?.Name ?? string.Empty;

                    row["Document"] = loc.Document.Name;
                    row["Line"] = lineIndex + 1; // 1-based for display
                    row["Span"] = $"{span.Start}..{span.End}";
                    row["Code"] = lineText;
                    table.Rows.Add(row);
                }
            }
        }

        return table;
    }
}
