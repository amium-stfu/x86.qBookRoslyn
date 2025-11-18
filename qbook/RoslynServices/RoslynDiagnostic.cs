using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using QB;
using qbook.CodeEditor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using RoslynDocument = Microsoft.CodeAnalysis.Document; // Alias gegen Kollision mit ScintillaNET.Document

namespace qbook.ScintillaEditor
{
    public static class RoslynDiagnostic
    {
        public static DataTable Output = new DataTable();
        private static readonly List<Microsoft.CodeAnalysis.Diagnostic> _currentDiagnostics = new();
        private static List<(int start, int length, string message)> _errorSpans = new();

        public static void InitDiagnostic()
        {
            Output = new DataTable();
            Output.Columns.Add("Page", typeof(string));
            Output.Columns.Add("Class", typeof(string));
            Output.Columns.Add("Position", typeof(string));
            Output.Columns.Add("Length", typeof(int));
            Output.Columns.Add("Type", typeof(string));
            Output.Columns.Add("Description", typeof(string));
            Output.Columns.Add("Node", typeof(CodeNode));
        }


        static void AddResult(string page, string cls, string pos, int len, string type, string desc, CodeNode node)
        {
            DataRow row = Output.NewRow();
            row["Page"] = page;
            row["Class"] = cls;
            row["Position"] = pos;
            row["Length"] = len;
            row["Type"] = type;
            row["Description"] = desc;
            row["Node"] = node;
            Output.Rows.Add(row);

        }


        public static async Task<DataTable> ApplyAsync(DocumentEditor editor)
        {
            if (!editor.Active) return Output;
            try
            {
                string buffer = editor.Text;
                var collected = new List<Microsoft.CodeAnalysis.Diagnostic>();

                RoslynDocument doc = editor.Target.Document;

                if (doc != null)
                {
                    // Text in dasselbe Document pushen
                    //var updated = doc.WithText(SourceText.From(buffer));
                    //if (doc.Project.Solution.Workspace.TryApplyChanges(updated.Project.Solution))
                    //    edit.Target.Code = doc = updated;

                    var tree = await doc.GetSyntaxTreeAsync();
                    if (tree != null) collected.AddRange(tree.GetDiagnostics());

                    var model = await doc.GetSemanticModelAsync();
                    if (model != null) collected.AddRange(model.GetDiagnostics());

                    var comp = await doc.Project.GetCompilationAsync();
                    if (comp != null && tree != null)
                    {
                        collected.AddRange(comp.GetDiagnostics()
                            .Where(d => d.Location.IsInSource && d.Location.SourceTree == tree));
                    }
                }

                var final = collected
                    .Where(d => d.Severity == DiagnosticSeverity.Error && !d.IsSuppressed && d.Location.IsInSource)
                    .GroupBy(d => (d.Id, d.Location.SourceSpan.Start, d.Location.SourceSpan.Length))
                    .Select(g => g.First())
                    .ToList();

                var errorSpansTmp = new List<(int start, int length, string message)>();
                foreach (var d in final)
                {
                    var (s, l) = GetNiceErrorRange(editor, d);
                    if (l <= 0) l = 1;
                    s = Math.Max(0, Math.Min(s, editor.TextLength));
                    l = Math.Max(1, Math.Min(l, editor.TextLength - s));
                    errorSpansTmp.Add((s, l, d.GetMessage(CultureInfo.GetCultureInfo("en-US"))));
                }

                editor.OnUi(() =>
                {

                    editor.IndicatorCurrent = 0;
                    editor.IndicatorClearRange(0, editor.TextLength);

                    string pageName = "";
                    string nodeName = "";

                    if (editor.Page == null)
                    {
                        nodeName = editor.Target.Filename;
                    }
                    else 
                    { 
                        pageName = editor.Page?.Name ?? "";
                        nodeName = editor.Page.Text;
                    }

                    Output.Clear();
                    foreach (var tup in errorSpansTmp)
                    {
                        string pos = tup.start.ToString();

                        AddResult(pageName, nodeName, pos, tup.length, "Error", tup.message, null);
                        editor.IndicatorFillRange(tup.start, tup.length);
                    }
                    _errorSpans = errorSpansTmp;
                    _currentDiagnostics.Clear();
                    _currentDiagnostics.AddRange(final);
                });

                return Output;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Diagnostics error: " + ex.Message);
                return Output;
            }
        }

        public static async Task <DataTable> ApplyAsync(CodeNode node)
        {
            if (!node.Active) return Output;
            try
            {
                string buffer = node.Editor.Text;
                var collected = new List<Microsoft.CodeAnalysis.Diagnostic>();

                RoslynDocument doc = node.RoslynDoc;
                if (doc != null)
                {
                    // Text in dasselbe Document pushen
                    var updated = doc.WithText(SourceText.From(buffer, Encoding.UTF8));
                    if (doc.Project.Solution.Workspace.TryApplyChanges(updated.Project.Solution))
                        node.RoslynDoc = doc = updated;

                    var tree = await doc.GetSyntaxTreeAsync();
                    if (tree != null) collected.AddRange(tree.GetDiagnostics());

                    var model = await doc.GetSemanticModelAsync();
                    if (model != null) collected.AddRange(model.GetDiagnostics());

                    var comp = await doc.Project.GetCompilationAsync();
                    if (comp != null && tree != null)
                    {
                        collected.AddRange(comp.GetDiagnostics()
                            .Where(d => d.Location.IsInSource && d.Location.SourceTree == tree));
                    }
                }

                var final = collected
                    .Where(d => d.Severity == DiagnosticSeverity.Error && !d.IsSuppressed && d.Location.IsInSource)
                    .GroupBy(d => (d.Id, d.Location.SourceSpan.Start, d.Location.SourceSpan.Length))
                    .Select(g => g.First())
                    .ToList();

                var errorSpansTmp = new List<(int start, int length, string message)>();
                foreach (var d in final)
                {
                    var (s, l) = GetNiceErrorRange(node, d);
                    if (l <= 0) l = 1;
                    s = Math.Max(0, Math.Min(s, node.Editor.TextLength));
                    l = Math.Max(1, Math.Min(l, node.Editor.TextLength - s));
                    errorSpansTmp.Add((s, l, d.GetMessage(CultureInfo.GetCultureInfo("en-US"))));
                }

                node.Editor.OnUi(() =>
                {
                 
                    node.Editor.IndicatorCurrent = 0;
                    node.Editor.IndicatorClearRange(0, node.Editor.TextLength);
                    string pageName = node.Page?.Name ?? "";
                    string nodeName = node.Text;

                    Output.Clear();
                    foreach (var tup in errorSpansTmp)
                    {
                        string pos = tup.start.ToString();

                        AddResult(pageName, nodeName, pos, tup.length, "Error", tup.message, node);
                        node.Editor.IndicatorFillRange(tup.start, tup.length);
                    }
                    _errorSpans = errorSpansTmp;
                    _currentDiagnostics.Clear();
                    _currentDiagnostics.AddRange(final);
                });

                return Output;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Diagnostics error: " + ex.Message);
                return Output;
            }
        }
        private static (int start, int len) GetNiceErrorRange(CodeNode node,Microsoft.CodeAnalysis.Diagnostic d)
        {
            var span = d.Location.SourceSpan;
            int docLen = node.Editor.TextLength;
            string txt = node.Editor.Text;

            // Spezialfall: Modifier an falscher Stelle (z. B. CS1585)
            if (d.Id == "CS1585" || d.Id == "CS1519") // CS1519 = Invalid token in class/struct/namespace
            {
                int p = Math.Max(0, span.Start - 1);
                while (p > 0 && !char.IsWhiteSpace(txt[p])) p--;
                int s = node.Editor.WordStartPosition(p, true);
                int e = node.Editor.WordEndPosition(p, true);
                if (e > s) return (s, e - s);
            }

            // Standardverhalten: Wortumrandung
            int s1 = node.Editor.WordStartPosition(Math.Max(0, span.Start), true);
            int e1 = node.Editor.WordEndPosition(Math.Min(docLen, span.Start + Math.Max(1, span.Length)), true);
            if (e1 > s1) return (s1, Math.Min(docLen - s1, e1 - s1));

            // Rückwärts zum nächsten Token
            int p2 = Math.Max(0, Math.Min(docLen - 1, span.Start));
            while (p2 > 0 && char.IsWhiteSpace(txt[p2])) p2--;
            s1 = node.Editor.WordStartPosition(p2, true);
            e1 = node.Editor.WordEndPosition(p2, true);
            if (e1 > s1) return (s1, e1 - s1);

            // Fallback: einzelnes Zeichen
            int start = Math.Max(0, Math.Min(span.Start, docLen - 1));
            return (start, 1);
        }

        private static (int start, int len) GetNiceErrorRange(DocumentEditor editor , Microsoft.CodeAnalysis.Diagnostic d)
        {
            var span = d.Location.SourceSpan;
            int docLen = editor.TextLength;
            string txt = editor.Text;

            // Spezialfall: Modifier an falscher Stelle (z. B. CS1585)
            if (d.Id == "CS1585" || d.Id == "CS1519") // CS1519 = Invalid token in class/struct/namespace
            {
                int p = Math.Max(0, span.Start - 1);
                while (p > 0 && !char.IsWhiteSpace(txt[p])) p--;
                int s = editor.WordStartPosition(p, true);
                int e = editor.WordEndPosition(p, true);
                if (e > s) return (s, e - s);
            }

            // Standardverhalten: Wortumrandung
            int s1 = editor.WordStartPosition(Math.Max(0, span.Start), true);
            int e1 = editor.WordEndPosition(Math.Min(docLen, span.Start + Math.Max(1, span.Length)), true);
            if (e1 > s1) return (s1, Math.Min(docLen - s1, e1 - s1));

            // Rückwärts zum nächsten Token
            int p2 = Math.Max(0, Math.Min(docLen - 1, span.Start));
            while (p2 > 0 && char.IsWhiteSpace(txt[p2])) p2--;
            s1 = editor.WordStartPosition(p2, true);
            e1 = editor.WordEndPosition(p2, true);
            if (e1 > s1) return (s1, e1 - s1);

            // Fallback: einzelnes Zeichen
            int start = Math.Max(0, Math.Min(span.Start, docLen - 1));
            return (start, 1);
        }
    }
}
