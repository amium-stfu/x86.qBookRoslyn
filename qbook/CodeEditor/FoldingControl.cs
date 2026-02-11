using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Shapes;
using static qbook.CodeEditor.FormEditor;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ExplorerBar;

namespace qbook.CodeEditor
{
    public class FoldingStateByContent
    {
        public string LineText { get; set; }
        public bool IsFolded { get; set; }
        public int HeaderIndex { get; set; }  // 👈 neu
    }


    internal class FoldingControl
    {
        ScintillaNET.Scintilla Editor;
        FormEditor Root;
        private int expectedFoldingHeaderCount = 0;

        public FoldingControl(FormEditor root)
        {
            Root = root;
            this.Editor = Root.Editor;

           // Editor.KeyDown += Editor_KeyDown;
        }

        public void Initialize()
        {
            const int MARGIN_FOLD = 2;
            Editor.SetProperty("fold", "1");
            Editor.SetProperty("fold.compact", "1");
            Editor.SetProperty("fold.preprocessor", "1");
            Editor.SetProperty("fold.line", "0");


            Editor.Margins[MARGIN_FOLD].Type = MarginType.Symbol;
            Editor.Margins[MARGIN_FOLD].Sensitive = true;
            Editor.Margins[MARGIN_FOLD].Mask = Marker.MaskFolders;
            Editor.Margins[MARGIN_FOLD].Width = 20;

            Editor.Markers[Marker.Folder].Symbol = MarkerSymbol.Arrow;
            Editor.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.ArrowDown;
            Editor.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            Editor.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;
            Editor.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            Editor.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.Arrow;
            Editor.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.ArrowDown;

            int[] folderMarkers = new[] {
            Marker.Folder,
            Marker.FolderOpen,
            Marker.FolderEnd,
            Marker.FolderOpenMid,
            Marker.FolderMidTail,
            Marker.FolderSub,
            Marker.FolderTail
            };

            var fore = Color.FromArgb(0x60, 0x60, 0x60);
            var back = Color.FromArgb(0xE0, 0xE0, 0xE0);


            fore = Color.DarkGray;
            back = Color.DarkGray;



            foreach (var i in folderMarkers)
            {
                Editor.Markers[i].SetForeColor(fore);
                Editor.Markers[i].SetBackColor(back);
            }

            Editor.AutomaticFold = AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change;
         
        }
        public class BraceBlock
        {
            public int OpenPos;
            public int ClosePos;
            public int OpenLine;
            public int CloseLine;
            public List<BraceBlock> Children = new();
            public BraceBlock? Parent; // NEU
            public bool Contains(int pos) => OpenPos <= pos && pos <= ClosePos;
        }
        public List<BraceBlock> BuildBraceForest()
        {
            var forest = new List<BraceBlock>();
            var stack = new Stack<BraceBlock>();
            for (int pos = 0; pos < Editor.TextLength; pos++)
            {
                int ch = Editor.GetCharAt(pos);
                if (ch == '{')
                {
                    var blk = new BraceBlock { OpenPos = pos, OpenLine = Editor.LineFromPosition(pos) };
                    if (stack.Count == 0) forest.Add(blk); else { var parent = stack.Peek(); parent.Children.Add(blk); blk.Parent = parent; }
                    stack.Push(blk);
                }
                else if (ch == '}' && stack.Count > 0)
                {
                    var blk = stack.Pop();
                    blk.ClosePos = pos;
                    blk.CloseLine = Editor.LineFromPosition(pos);
                }
            }
            forest = forest.Where(b => b.ClosePos > b.OpenPos).ToList();

            // Filter: entferne Auto-Property Accessor-Blöcke (z.B. { get; set; }) damit Methoden als direkte Kinder erkannt werden
            bool IsAccessorBlock(BraceBlock b)
            {
                if (b.OpenLine != b.CloseLine) return false; // nur einzeilige
                int innerLen = b.ClosePos - b.OpenPos - 1;
                if (innerLen <= 0) return false;
                string inner = Editor.GetTextRange(b.OpenPos + 1, innerLen);
                // Nur Schlüsselwörter get/set/init ; und evtl. Whitespace
                if (System.Text.RegularExpressions.Regex.IsMatch(inner, @"^\s*(?:get|set|init);(?:\s*(?:get|set|init);)*\s*$")) return true;
                return false;
            }
            void Prune(List<BraceBlock> list)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var b = list[i];
                    if (IsAccessorBlock(b)) { list.RemoveAt(i); continue; }
                    Prune(b.Children);
                }
            }
            Prune(forest);
            return forest;
        }
        public BraceBlock? FindDeepestContaining(BraceBlock blk, int caret)
        {
            if (!blk.Contains(caret)) return null;
            foreach (var child in blk.Children)
            {
                var hit = FindDeepestContaining(child, caret);
                if (hit != null) return hit;
            }
            return blk;
        }
        public ((int openLine, int closeLine, int openPos, int closePos)? parent, List<(int openLine, int closeLine, int openPos, int closePos)> directChildren) GetParentAndDirectChildren()
        {
            var forest = BuildBraceForest();
            int caret = Editor.CurrentPosition;
            BraceBlock? deepest = null;
            foreach (var root in forest)
            {
                var hit = FindDeepestContaining(root, caret);
                if (hit != null)
                {
                    if (deepest == null || (hit.OpenPos >= deepest.OpenPos && hit.ClosePos <= deepest.ClosePos)) deepest = hit;
                }
            }
            if (deepest == null) return (null, new());
            // Heuristik: Wenn wir in einem Block ohne Geschwister (oder mit Parent der viele Kinder hat) und der Block klein ist (z.B. if/loop innerhalb einer Methode), wähle einen höheren Block mit mehreren Kindern (z.B. Methoden innerhalb Klasse)
            BraceBlock chosen = deepest;
            bool IsSmall(BraceBlock b) => (b.CloseLine - b.OpenLine) < 3; // sehr kleiner Block
            if (chosen.Parent != null)
            {
                // Wenn der Parent >1 Kinder hat und chosen entweder klein ist oder Parent deutlich größer ist -> Parent benutzen
                if (chosen.Parent.Children.Count > 1 && (IsSmall(chosen) || (chosen.Parent.CloseLine - chosen.Parent.OpenLine) > (chosen.CloseLine - chosen.OpenLine) * 2))
                {
                    chosen = chosen.Parent;
                }
                else
                {
                    // Eine Ebene höher weiter gehen bis wir einen Block mit >1 Kindern finden (Klassenblock) falls aktueller nur genau ein Child enthält
                    var ascend = chosen;
                    while (ascend.Parent != null && ascend.Parent.Children.Count <= 1)
                        ascend = ascend.Parent;
                    if (ascend.Parent != null && ascend.Parent.Children.Count > 1)
                        chosen = ascend.Parent;
                }
            }
            var resultChildren = chosen.Children
                .Where(c => c.ClosePos > c.OpenPos)
                .Select(c => (c.OpenLine, c.CloseLine, c.OpenPos, c.ClosePos))
                .ToList();
            return ((chosen.OpenLine, chosen.CloseLine, chosen.OpenPos, chosen.ClosePos), resultChildren);
        }
        public async Task CollapseAllInnerBlocksExceptOuter()
        {
            EditorNode node = Root.SelectedNode;
            
            var (parent, directChildren) = GetParentAndDirectChildren();
            if (parent == null || directChildren.Count == 0) return;
            foreach (var b in directChildren)
                if (!LineIsCollapsed(Editor, b.openLine)) Editor.Lines[b.openLine].ToggleFold();

        }
        public void ExpandAllBlocksAtCaret()
        {
            var (parent, directChildren) = GetParentAndDirectChildren();
            if (parent == null) return;
            foreach (var b in directChildren)
                if (LineIsCollapsed(Editor, b.openLine)) Editor.Lines[b.openLine].ToggleFold();
        }
        public static bool LineIsCollapsed(Scintilla sci, int headerLine)
        {
            if (headerLine + 1 >= sci.Lines.Count) return false;
            return !sci.Lines[headerLine + 1].Visible;
        }



        // --- SAVE: Speichert alle Zustände, LOG nur wenn eingeklappt ---
        public void SaveFolding()
        {
            var states = new List<FoldingStateByContent>();

            for (int i = 0; i < Editor.Lines.Count; i++)
            {
                var line = Editor.Lines[i];
                if (line.FoldLevelFlags.HasFlag(FoldLevelFlags.Header))
                {
                    string signatureText = line.Text.Trim();
                    if (signatureText == "{" && i > 0)
                    {
                        var prev = Editor.Lines[i - 1].Text.Trim();
                        if (!string.IsNullOrEmpty(prev))
                            signatureText = prev;
                    }

                    bool isFolded = !line.Expanded;

                    states.Add(new FoldingStateByContent
                    {
                        LineText = signatureText,
                        IsFolded = isFolded,
                        HeaderIndex = i
                    });

                    if (isFolded)
                        Debug.WriteLine($"Save {signatureText} {line.Expanded}: {i}");
                }
            }

            for (int i = 0; i < Editor.Lines.Count; i++)
            {
                if (Editor.Lines[i].FoldLevelFlags.HasFlag(FoldLevelFlags.Header))
                {
                    Debug.WriteLine($"HeaderCheck {i}: '{Editor.Lines[i].Text.Trim()}', Expanded={Editor.Lines[i].Expanded}");
                }
            }

            Root.SelectedNode.Foldings = states;
        }

        // --- LOAD: Stellt Zustand her, LOG nur wenn eingeklappt wird ---
        public async Task LoadFolding()
        {
      
            var states = Root.SelectedNode?.Foldings;
            if (states == null || states.Count == 0)
                return;

            // 🟡 1. Erst ALLE Header aufklappen
            for (int i = 0; i < Editor.Lines.Count; i++)
            {
                var line = Editor.Lines[i];
                if (line.FoldLevelFlags.HasFlag(FoldLevelFlags.Header) && !line.Expanded)
                    line.ToggleFold();
            }

            // 🟢 2. Dann gespeicherte Zustände wiederherstellen
            foreach (var saved in states)
            {
                if (saved.HeaderIndex < 0 || saved.HeaderIndex >= Editor.Lines.Count)
                    continue;

                var line = Editor.Lines[saved.HeaderIndex];
                if (!line.FoldLevelFlags.HasFlag(FoldLevelFlags.Header))
                    continue;

                if (saved.IsFolded && line.Expanded)
                {
                    line.ToggleFold();
                    Debug.WriteLine($"LoadFolding: folded '{saved.LineText}' at {saved.HeaderIndex}");
                }
            }
        }

        public async Task WaitForFoldingReady(int expectedCount, int timeoutMs = 5000)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                int headerCount = 0;
                for (int i = 0; i < Editor.Lines.Count; i++)
                {
                    if (Editor.Lines[i].FoldLevelFlags.HasFlag(FoldLevelFlags.Header))
                        headerCount++;
                }

                if (headerCount >= expectedCount)
                {
                    Debug.WriteLine($"Folding ready after {sw.ElapsedMilliseconds} ms ({headerCount}/{expectedCount})");
                    return;
                }

                await Task.Delay(20);
            }

            Debug.WriteLine($"Folding NOT ready after {timeoutMs} ms");
        }




        // --- HELFER: exakte Signaturzeile per Text finden ---
        private int FindSignatureLineIndex(string signatureText)
        {
            for (int i = 0; i < Editor.Lines.Count; i++)
            {
                var current = Editor.Lines[i].Text.Trim();

                // Kommentare entfernen (alles nach //)
                int commentIdx = current.IndexOf("//", StringComparison.Ordinal);
                if (commentIdx >= 0)
                    current = current.Substring(0, commentIdx).Trim();

                // Whitespace normalisieren
                var normCurrent = System.Text.RegularExpressions.Regex.Replace(current, @"\\s+", " ");
                var normSignature = System.Text.RegularExpressions.Regex.Replace(signatureText, @"\\s+", " ");

                if (normCurrent.Equals(normSignature, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }


        // --- HELFER: korrekte Header-Zeile zur Signatur finden ---
        private int FindHeaderLineIndexFromSignature(int sigIndex)
        {
            // Stil: Klammer auf derselben Zeile ⇒ Signatur ist Header
            if (Editor.Lines[sigIndex].FoldLevelFlags.HasFlag(FoldLevelFlags.Header))
                return sigIndex;

            // Stil: Klammer auf folgender oder übernächster Zeile (leere Zeilen überspringen)
            for (int offs = 1; offs <= 2; offs++)
            {
                int idx = sigIndex + offs;
                if (idx >= Editor.Lines.Count)
                    break;

                var ln = Editor.Lines[idx];

                // erste Folgelinie leer? → einmal überspringen
                if (offs == 1 && string.IsNullOrWhiteSpace(ln.Text))
                    continue;

                if (ln.FoldLevelFlags.HasFlag(FoldLevelFlags.Header))
                    return idx;
            }

            return -1; // kein Header in kurzer Distanz gefunden
        }
    }

}
