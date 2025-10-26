using DevExpress.DocumentView;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Scripting.Utils;
using QB;
using ScintillaNET;
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

namespace qbook.CodeEditor
{
    internal class Diagnostic
    {
        private readonly FormEditor Root;
        private readonly Scintilla Editor;
        private readonly RoslynServices _roslyn;
        private readonly List<Microsoft.CodeAnalysis.Diagnostic> _currentDiagnostics = new();
        private List<(int start, int length, string message)> _errorSpans = new();

        public DataTable Output = new DataTable();
        public DataTable BuildOutput = new DataTable();

        public bool BuildError = false;

        public Diagnostic(FormEditor root, Scintilla editor, RoslynServices roslyn)
        {
            Root = root;
            Editor = editor;
            _roslyn = roslyn;

            // Initialize the DataTable
            Output.Columns.Add("Page", typeof(string));
            Output.Columns.Add("Class", typeof(string));
            Output.Columns.Add("Position", typeof(string));
            Output.Columns.Add("Length", typeof(int));
            Output.Columns.Add("Type", typeof(string));
            Output.Columns.Add("Description", typeof(string));
            Output.Columns.Add("Node", typeof(EditorNode));

            BuildOutput.Columns.Add("Page", typeof(string));
            BuildOutput.Columns.Add("Class", typeof(string));
            BuildOutput.Columns.Add("Position", typeof(string));
            BuildOutput.Columns.Add("Length", typeof(int));
            BuildOutput.Columns.Add("Type", typeof(string));
            BuildOutput.Columns.Add("Description", typeof(string));
            BuildOutput.Columns.Add("Node", typeof(EditorNode));

        }

        public async Task ApplyAsync()
        {
            if (Root.SelectedNode == null) return;
            if (!Root.SelectedNode.Active) return;

            try
            {
                string buffer = Editor.Text;
                var collected = new List<Microsoft.CodeAnalysis.Diagnostic>();

                RoslynDocument doc = Root.GetCurrentDocument();
                if (doc != null)
                {
                    // Text in dasselbe Document pushen
                    var updated = doc.WithText(SourceText.From(buffer));
                    if (doc.Project.Solution.Workspace.TryApplyChanges(updated.Project.Solution))
                        Root.SelectedNode.RoslynDoc = doc = updated;

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
                    var (s, l) = GetNiceErrorRange(d);
                    if (l <= 0) l = 1;
                    s = Math.Max(0, Math.Min(s, Editor.TextLength));
                    l = Math.Max(1, Math.Min(l, Editor.TextLength - s));
                    errorSpansTmp.Add((s, l, d.GetMessage(CultureInfo.GetCultureInfo("en-US"))));
                }

                Root.OnUi(() =>
                {
                   if(Root.SelectedNode == null) return;
                    Editor.IndicatorCurrent = 0;
                    Editor.IndicatorClearRange(0, Editor.TextLength);
                    string pageName = Root.SelectedNode.Page?.Name ?? "";
                    string nodeName = Root.SelectedNode.Text;

                    Output.Clear();
                    foreach (var tup in errorSpansTmp)
                    {
                        string pos = tup.start.ToString();
                        Output.Rows.Add(pageName, nodeName, pos, tup.length, "Error", tup.message, Root.SelectedNode);
                        Editor.IndicatorFillRange(tup.start, tup.length);
                    }
                    _errorSpans = errorSpansTmp;
                    _currentDiagnostics.Clear();
                    _currentDiagnostics.AddRange(final);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Diagnostics error: " + ex.Message);
            }
        }

        private (int start, int len) GetNiceErrorRange(Microsoft.CodeAnalysis.Diagnostic d)
        {
            var span = d.Location.SourceSpan;
            int docLen = Editor.TextLength;
            string txt = Editor.Text;

            // Spezialfall: Modifier an falscher Stelle (z. B. CS1585)
            if (d.Id == "CS1585" || d.Id == "CS1519") // CS1519 = Invalid token in class/struct/namespace
            {
                int p = Math.Max(0, span.Start - 1);
                while (p > 0 && !char.IsWhiteSpace(txt[p])) p--;
                int s = Editor.WordStartPosition(p, true);
                int e = Editor.WordEndPosition(p, true);
                if (e > s) return (s, e - s);
            }

            // Standardverhalten: Wortumrandung
            int s1 = Editor.WordStartPosition(Math.Max(0, span.Start), true);
            int e1 = Editor.WordEndPosition(Math.Min(docLen, span.Start + Math.Max(1, span.Length)), true);
            if (e1 > s1) return (s1, Math.Min(docLen - s1, e1 - s1));

            // Rückwärts zum nächsten Token
            int p2 = Math.Max(0, Math.Min(docLen - 1, span.Start));
            while (p2 > 0 && char.IsWhiteSpace(txt[p2])) p2--;
            s1 = Editor.WordStartPosition(p2, true);
            e1 = Editor.WordEndPosition(p2, true);
            if (e1 > s1) return (s1, e1 - s1);

            // Fallback: einzelnes Zeichen
            int start = Math.Max(0, Math.Min(span.Start, docLen - 1));
            return (start, 1);
        }

        private bool TryGetMissingSemicolonRange(int pos, out int start, out int len)
        {
            int docLen = Editor.TextLength;
            string txt = Editor.Text;
            int p = Math.Max(0, Math.Min(docLen - 1, pos > 0 ? pos - 1 : 0));
            while (p > 0 && char.IsWhiteSpace(txt[p])) p--;
            int s = Editor.WordStartPosition(p, true);
            int e = Editor.WordEndPosition(p, true);
            if (e > s)
            {
                start = s; len = e - s; return true;
            }
            start = p; len = 1; return true;
        }

        public async Task BuildRunDiagnosticsAsync(EditorNode node)
        {
            try
            {
                if (!node.Active) return;

                string pageName = "";
                string nodeName = "";

                Scintilla validator = new();
                validator.Text = await node.GetCodeAsync("FormEditor.BuildRunDiagnostics");

                if (node.Type == EditorNode.NodeType.Page)
                {
                    pageName = node.Text;
                }
                else
                {
                    pageName = node.PageNode.Text;
                    nodeName = node.Text;
                }

                string buffer = validator.Text;
                var collected = new List<Microsoft.CodeAnalysis.Diagnostic>();

                var doc = node.RoslynDoc;
                if (doc != null)
                {
                    // Text in dasselbe Document pushen
                    var updated = doc.WithText(SourceText.From(buffer));
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
                    var (s, l) = GetNiceErrorRange(d);
                    if (l <= 0) l = 1;
                    s = Math.Max(0, Math.Min(s, validator.TextLength));
                    l = Math.Max(1, Math.Min(l, validator.TextLength - s));
                    errorSpansTmp.Add((s, l, d.GetMessage(CultureInfo.GetCultureInfo("en-US"))));
                }

             
                foreach (var tup in errorSpansTmp)
                {
                    string pos = tup.start.ToString();
                    BuildOutput.Rows.Add(pageName, nodeName, pos, tup.length, "Error", tup.message, node);
                    BuildError = true;
                }
               
                _errorSpans = errorSpansTmp;
                _currentDiagnostics.Clear();
                _currentDiagnostics.AddRange(final);
          
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Diagnostics error: " + ex.Message);
            }

        }

    }
}
