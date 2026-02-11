using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using qbook.CodeEditor;
using RoslynDocument = Microsoft.CodeAnalysis.Document; // Alias gegen Kollision mit ScintillaNET.Document

public static class SemanticHighlighter
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
       
        
        
        if(doc == null) return;
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
}