using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace qbook.CodeEditor
{
    internal static class RoslynFolding
    {
        private const int MARGIN_FOLD = 2;

        // 🧠 gespeicherte Folding-Zustände
        private static List<string> _collapsedHeaders = new();

        #region Initialization
        public static void InitializeFolding(Scintilla editor)
        {
            if (editor == null)
                return;

            editor.SetProperty("fold", "1");
            editor.SetProperty("fold.compact", "1");
            editor.SetProperty("fold.preprocessor", "1");

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
        #endregion

        #region Folding Generation
        public static void ApplyFolding(Scintilla editor)
        {
            if (editor == null || string.IsNullOrEmpty(editor.Text))
                return;

            var tree = CSharpSyntaxTree.ParseText(editor.Text);
            var root = tree.GetRoot();

            var foldingRegions = new List<(int startLine, int endLine, string name)>();

            // Klassen & Methoden
            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                AddFoldingRegion(editor, classDecl, foldingRegions, $"class {classDecl.Identifier}");
                foreach (var method in classDecl.Members.OfType<MethodDeclarationSyntax>())
                    AddFoldingRegion(editor, method, foldingRegions, method.Identifier.Text);
            }

            // Methoden auf oberster Ebene
            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .Where(m => m.Parent is not ClassDeclarationSyntax))
            {
                AddFoldingRegion(editor, method, foldingRegions, method.Identifier.Text);
            }

            // #region
            AddRegionFolding(editor, root, foldingRegions);

            // Kontrollstrukturen
            foreach (var node in root.DescendantNodes())
            {
                switch (node)
                {
                    case IfStatementSyntax ifStmt:
                        AddFoldingRegion(editor, ifStmt.Statement, foldingRegions, "if");
                        break;
                    case ForStatementSyntax forStmt:
                        AddFoldingRegion(editor, forStmt.Statement, foldingRegions, "for");
                        break;
                    case ForEachStatementSyntax foreachStmt:
                        AddFoldingRegion(editor, foreachStmt.Statement, foldingRegions, "foreach");
                        break;
                    case WhileStatementSyntax whileStmt:
                        AddFoldingRegion(editor, whileStmt.Statement, foldingRegions, "while");
                        break;
                    case SwitchStatementSyntax switchStmt:
                        AddFoldingRegion(editor, switchStmt, foldingRegions, "switch");
                        break;
                }
            }

            // alte Foldings löschen
            for (int i = 0; i < editor.Lines.Count; i++)
            {
                editor.Lines[i].FoldLevelFlags = FoldLevelFlags.White;
                editor.Lines[i].FoldLevel = 1024;
            }

            // neue Foldings setzen
            foreach (var region in foldingRegions)
            {
                if (region.startLine >= editor.Lines.Count || region.endLine >= editor.Lines.Count)
                    continue;

                var headerLine = editor.Lines[region.startLine];
                headerLine.FoldLevelFlags |= FoldLevelFlags.Header;
                headerLine.FoldLevel = 1024;

                for (int i = region.startLine + 1; i <= region.endLine && i < editor.Lines.Count; i++)
                {
                    editor.Lines[i].FoldLevel = 1025;
                }
            }
        }

        private static void AddFoldingRegion(
            Scintilla editor, SyntaxNode node,
            List<(int startLine, int endLine, string name)> list,
            string name)
        {
            if (node == null)
                return;

            int startLine = -1;
            int endLine = -1;

            switch (node)
            {
                case MethodDeclarationSyntax method when method.Body != null:
                    startLine = editor.LineFromPosition(method.Body.OpenBraceToken.Span.Start);
                    endLine = editor.LineFromPosition(method.Body.CloseBraceToken.Span.End);
                    break;

                case ClassDeclarationSyntax cls when cls.OpenBraceToken.IsKind(SyntaxKind.OpenBraceToken):
                    startLine = editor.LineFromPosition(cls.OpenBraceToken.Span.Start);
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

        #endregion

        private static void AddRegionFolding(Scintilla editor, SyntaxNode root, List<(int startLine, int endLine, string name)> list)
        {
            var regionDirectives = root.DescendantTrivia()
                .Where(t => t.IsKind(SyntaxKind.RegionDirectiveTrivia))
                .ToList();

            var endRegionDirectives = root.DescendantTrivia()
                .Where(t => t.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
                .ToList();

            foreach (var region in regionDirectives)
            {
                var startLine = editor.LineFromPosition(region.SpanStart);
                var end = endRegionDirectives.FirstOrDefault(e => e.SpanStart > region.SpanStart);

                if (end.SpanStart > 0)
                {
                    var endLine = editor.LineFromPosition(end.SpanStart);
                    if (endLine > startLine)
                        list.Add((startLine, endLine, "#region"));
                }
            }
        }

    }
}
