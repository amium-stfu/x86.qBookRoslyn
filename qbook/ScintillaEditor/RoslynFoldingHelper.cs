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
       
        public void ApplyFolding(Scintilla editor)
        {
            if (editor == null || string.IsNullOrEmpty(editor.Text))
                return;

            var tree = CSharpSyntaxTree.ParseText(editor.Text);
            var root = tree.GetRoot();
            var foldingRegions = new List<(int startLine, int endLine, string name)>();

            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                AddFoldingRegion(editor, classDecl, foldingRegions, $"class {classDecl.Identifier}");
                foreach (var method in classDecl.Members.OfType<MethodDeclarationSyntax>())
                    AddFoldingRegion(editor, method, foldingRegions, method.Identifier.Text);
            }

            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                     .Where(m => m.Parent is not ClassDeclarationSyntax))
            {
                AddFoldingRegion(editor, method, foldingRegions, method.Identifier.Text);
            }

            // Clear old folding
            for (int i = 0; i < editor.Lines.Count; i++)
            {
                editor.Lines[i].FoldLevelFlags = FoldLevelFlags.White;
                editor.Lines[i].FoldLevel = 1024;
            }

            // Apply new folding
            foreach (var region in foldingRegions)
            {
                if (region.startLine >= editor.Lines.Count || region.endLine >= editor.Lines.Count)
                    continue;

                var headerLine = editor.Lines[region.startLine];
                headerLine.FoldLevelFlags = FoldLevelFlags.Header;
                headerLine.FoldLevel = 1024;

                for (int i = region.startLine + 1; i <= region.endLine && i < editor.Lines.Count; i++)
                {
                    editor.Lines[i].FoldLevel = 1025;
                }
            }
        }


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

        private void AddFoldingRegion(Scintilla editor, SyntaxNode node, List<(int startLine, int endLine, string name)> list, string name)
        {
            if (node == null)
                return;

            int startLine = -1;
            int endLine = -1;

            switch (node)
            {


                case MethodDeclarationSyntax method:
                    startLine = editor.LineFromPosition(method.SpanStart);
                    endLine = editor.LineFromPosition(method.Span.End);
                    break;


                case ClassDeclarationSyntax cls when cls.OpenBraceToken.IsKind(SyntaxKind.OpenBraceToken):
                    startLine = editor.LineFromPosition(cls.SpanStart); // statt cls.OpenBraceToken
                    endLine = editor.LineFromPosition(cls.CloseBraceToken.Span.End);
                    break;

                default:
                    var span = node.Span;
                    startLine = editor.LineFromPosition(span.Start);
                    endLine = editor.LineFromPosition(span.End);
                    break;
            }

            if (endLine > startLine)
                list.Add((startLine, endLine, name));
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