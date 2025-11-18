using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ScintillaNET;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace qbook.ScintillaEditor
{
    internal class RoslynFoldingHelper
    {
        private readonly List<string> _collapsedHeaders = new();
       
      
        public void InitializeFolding(Scintilla editor)
        {
            if (editor == null)
                return;

            editor.SetProperty("fold", "1");
            editor.SetProperty("fold.compact", "1");
            editor.SetProperty("fold.preprocessor", "1");

            const int MARGIN_FOLD = 2;
            editor.Margins[MARGIN_FOLD].Type = MarginType.Symbol;
            editor.Margins[MARGIN_FOLD].Sensitive = true;
            editor.Margins[MARGIN_FOLD].Mask = Marker.MaskFolders;
            editor.Margins[MARGIN_FOLD].Width = 20;

            editor.Markers[Marker.Folder].Symbol = MarkerSymbol.Arrow;
            editor.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.ArrowDown;
            editor.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            editor.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;
            editor.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            editor.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.Arrow;
            editor.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.ArrowDown;

            var fore = Color.FromArgb(0x60, 0x60, 0x60);
            var back = Color.FromArgb(0xE0, 0xE0, 0xE0);
            foreach (var i in new[]
            {
        Marker.Folder,
        Marker.FolderOpen,
        Marker.FolderEnd,
        Marker.FolderOpenMid,
        Marker.FolderMidTail,
        Marker.FolderSub,
        Marker.FolderTail
    })
            {
                editor.Markers[i].SetForeColor(fore);
                editor.Markers[i].SetBackColor(back);
            }

            editor.AutomaticFold = AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change;
        }

        private void AddFoldingRegion(Scintilla editor, SyntaxNode node,
            List<(int headerLine, int endLine, string name)> list, string name)
        {
            if (node == null)
                return;

            int headerLine;
            int endLine;

            switch (node)
            {
                case MethodDeclarationSyntax method when method.Body != null:
                    headerLine = editor.LineFromPosition(method.SpanStart);

                    int closeBraceLine = editor.LineFromPosition(method.Body.CloseBraceToken.Span.Start);
                    endLine = closeBraceLine;   // WICHTIG: NICHT -1 !
                    break;

                case ClassDeclarationSyntax cls when cls.OpenBraceToken.IsKind(SyntaxKind.OpenBraceToken):
                    headerLine = editor.LineFromPosition(cls.SpanStart);

                    int clsClose = editor.LineFromPosition(cls.CloseBraceToken.Span.Start);
                    endLine = clsClose;
                    break;

                default:
                    var span = node.Span;
                    headerLine = editor.LineFromPosition(span.Start);
                    endLine = editor.LineFromPosition(span.End);
                    break;
            }

            if (endLine > headerLine)
                list.Add((headerLine, endLine, name));
        }

        public void ApplyFolding(Scintilla editor)
        {
            if (editor == null || string.IsNullOrEmpty(editor.Text))
                return;

            var tree = CSharpSyntaxTree.ParseText(editor.Text);
            var root = tree.GetRoot();
            var foldingRegions = new List<(int headerLine, int endLine, string name)>();

            foreach (var cls in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                AddFoldingRegion(editor, cls, foldingRegions, $"class {cls.Identifier}");
                foreach (var method in cls.Members.OfType<MethodDeclarationSyntax>())
                    AddFoldingRegion(editor, method, foldingRegions, $"method {method.Identifier.Text}");
            }

            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                     .Where(m => m.Parent is not ClassDeclarationSyntax))
            {
                AddFoldingRegion(editor, method, foldingRegions, method.Identifier.Text);
            }

            // Reset
            for (int i = 0; i < editor.Lines.Count; i++)
            {
                editor.Lines[i].FoldLevelFlags = FoldLevelFlags.White;
                editor.Lines[i].FoldLevel = 1024;
            }

            // Apply regions
            foreach (var region in foldingRegions)
            {
                if (region.headerLine < 0 || region.endLine < 0)
                    continue;
                if (region.headerLine >= editor.Lines.Count || region.endLine >= editor.Lines.Count)
                    continue;
                if (region.endLine <= region.headerLine)
                    continue;

                var header = editor.Lines[region.headerLine];
                header.FoldLevelFlags = FoldLevelFlags.Header;

                int level = region.name.StartsWith("class") ? 1024 : 1025;
                header.FoldLevel = level;

                // Body-Level (von headerLine+1 BIS einschließlich endLine)
                for (int i = region.headerLine + 1; i <= region.endLine && i < editor.Lines.Count; i++)
                    editor.Lines[i].FoldLevel = level + 1;

                // Die nächste Zeile NACH der '}' wieder Parent-Level
                int nextLine = region.endLine + 1;
                if (nextLine < editor.Lines.Count)
                    editor.Lines[nextLine].FoldLevel = level;
            }
        }

        public void SaveCollapsedFoldings(Scintilla editor)
        {
            _collapsedHeaders.Clear();
            var tree = CSharpSyntaxTree.ParseText(editor.Text);
            var root = tree.GetRoot();

            foreach (var node in root.DescendantNodes().OfType<MemberDeclarationSyntax>())
            {
                string name = GetNodeIdentifier(node);
                if (string.IsNullOrEmpty(name))
                    continue;

                int line = editor.LineFromPosition(node.SpanStart);
                if ((editor.Lines[line].FoldLevelFlags & FoldLevelFlags.Header) != 0 &&
                    !editor.Lines[line].Expanded)
                {
                    _collapsedHeaders.Add(name);
                }
            }
        }

        public void RestoreCollapsedFoldings(Scintilla editor)
        {
            var tree = CSharpSyntaxTree.ParseText(editor.Text);
            var root = tree.GetRoot();

            foreach (var node in root.DescendantNodes().OfType<MemberDeclarationSyntax>())
            {
                string name = GetNodeIdentifier(node);
                if (string.IsNullOrEmpty(name))
                    continue;

                if (_collapsedHeaders.Contains(name))
                {
                    int line = editor.LineFromPosition(node.SpanStart);
                    if ((editor.Lines[line].FoldLevelFlags & FoldLevelFlags.Header) != 0)
                    {
                        editor.Lines[line].ToggleFold();
                    }
                }
            }
        }

        private string GetNodeIdentifier(SyntaxNode node)
        {
            return node switch
            {
                ClassDeclarationSyntax cls => $"class:{cls.Identifier.Text}",
                MethodDeclarationSyntax method => $"method:{method.Identifier.Text}",
                ConstructorDeclarationSyntax ctor => $"ctor:{ctor.Identifier.Text}",
                PropertyDeclarationSyntax prop => $"prop:{prop.Identifier.Text}",
                _ => null
            };
        }
    
    }
}