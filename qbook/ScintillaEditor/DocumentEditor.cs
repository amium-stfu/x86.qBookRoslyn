using ActiproSoftware.Text.Languages.DotNet;
using DevExpress.XtraEditors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.SolutionPersistence.Model;
using QB.Controls;
using qbook.CodeEditor;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Forms;

using static System.ComponentModel.Design.ObjectSelectorEditor;
using RoslynDocument = Microsoft.CodeAnalysis.Document; // Alias gegen Kollision mit ScintillaNET.Document


namespace qbook.ScintillaEditor
{
    public class DocumentEditor : Scintilla
    {

        class LineRange
        {
            internal int Start { get; set; }
            internal int End { get; set; }

            public LineRange(int start, int end)
            {
                Start = start;
                End = end;
            }
        }
        
        internal enum EditorTheme { Light, Dark }
        private EditorTheme _currentTheme = EditorTheme.Light;

        private bool _awaitingCtrlK; // für Tastatur-Akkord (Ctrl+K, Ctrl+C/U)
        private bool _awaitingCtrlM; // für Tastatur-Akkord (Ctrl+M, Ctrl+O/L)

        public bool Active = true;
        private readonly System.Windows.Forms.Timer _chordTimer = new() { Interval = 1500 };
        public System.Windows.Forms.Timer DebounceTimer = new System.Windows.Forms.Timer();

        public RoslynDocument RoslynDoc;

        public Func<Task> UpdateRoslyn;
        public async Task TriggerUpdateAsync()
        {
            if (UpdateRoslyn != null)
                await UpdateRoslyn();
        }

        public Func<Task> GoToDefinition;
        public async Task TriggerGoToDefinitionAsync()
        {
            if (GoToDefinition != null)
                await GoToDefinition();
        }

        public Func<Task> RenameSymbol;
        public async Task TriggerRenameSymbolAsync()
        {
            if (RenameSymbol != null)
                await RenameSymbol();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (RoslynAutoComplete.IsVisible == true)
            {
                switch (keyData)
                {
                    case Keys.Down:
                        RoslynAutoComplete.Next();
                        return true;
                    case Keys.Up:
                        RoslynAutoComplete.Previous();
                        return true;
                    case Keys.Enter:
                        RoslynAutoComplete.Commit();
                        return true;
                    case Keys.Tab:
                        RoslynAutoComplete.Commit();
                        return true;

                    case Keys.Escape:
                        RoslynAutoComplete.HidePopup();
                        return true;
                }
            }

            if (RosylnSignatureHelper.IsVisible)
            {
                switch (keyData)
                {
                    case Keys.Down:
                        RosylnSignatureHelper.Next();
                        return true;
                    case Keys.Up:
                        RosylnSignatureHelper.Previous();
                        return true;
                    case Keys.Enter:
                        RosylnSignatureHelper.Commit();
                        return true;
                    case Keys.Tab:
                        RosylnSignatureHelper.Commit();
                        return true;

                    case Keys.Escape:
                        RosylnSignatureHelper.HidePopup();
                        return true;

                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }



        public void OnUi(System.Action a)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke(a); else a();
        }

        public void Init()
        {

            InitEditorContextMenu();
            HScrollBar = false;
            VScrollBar = false;
            BorderStyle = ScintillaNET.BorderStyle.None;

            const int MARGIN_LINE_NUMBERS = 0;
            Margins[MARGIN_LINE_NUMBERS].Type = MarginType.Number;
            Margins[MARGIN_LINE_NUMBERS].Width = 40;

            WhitespaceSize = 2;
            ViewWhitespace = WhitespaceMode.Invisible;
            IndentationGuides = IndentView.LookBoth;


            Indicators[0].Style = IndicatorStyle.Squiggle;
            Indicators[0].ForeColor = Color.Red;
            Indicators[0].Under = true;

            // 2..9 = Semantik-Overlay (Textfarbe oder Unterstreichung)
            Indicators[2].Style = IndicatorStyle.TextFore;   // Methoden
            Indicators[2].Under = true;
            Indicators[2].ForeColor = Color.DarkOrange;

            Indicators[3].Style = IndicatorStyle.TextFore;    // Klassen
            Indicators[3].ForeColor = Color.DarkViolet;

            Indicators[4].Style = IndicatorStyle.TextFore;    // Interfaces
            Indicators[4].ForeColor = Color.Red;

            Indicators[5].Style = IndicatorStyle.TextFore;    // Structs
            Indicators[5].ForeColor = Color.Firebrick;

            Indicators[6].Style = IndicatorStyle.TextFore;    // Enums
            Indicators[6].ForeColor = Color.FromArgb(0xCE, 0x91, 0x78);

            Indicators[7].Style = IndicatorStyle.TextFore;    // Delegates
            Indicators[7].ForeColor = Color.FromArgb(0xD4, 0xD4, 0xD4);


            Indicators[8].Style = IndicatorStyle.TextFore;    // Properties
            Indicators[8].ForeColor = Color.Black;

            Indicators[9].Style = IndicatorStyle.TextFore;    // Fields
            Indicators[9].ForeColor = Color.Black;

            // 10..12 zusätzliche Syntax (Keywords, Zahlen, Strings/Chars)
            Indicators[10].Style = IndicatorStyle.TextFore; // Keywords
            Indicators[10].ForeColor = Color.FromArgb(0x00, 0x33, 0xCC);
            Indicators[11].Style = IndicatorStyle.TextFore; // Numbers
            Indicators[11].ForeColor = Color.FromArgb(0x00, 0x80, 0x80);
            Indicators[12].Style = IndicatorStyle.TextFore; // Strings / Chars
            Indicators[12].ForeColor = Color.FromArgb(0xA3, 0x15, 0x15);

            Indicators[13].Style = IndicatorStyle.TextFore; // Kommentare
            Indicators[13].ForeColor = Color.FromArgb(0x00, 0x80, 0x00);

            Indicators[14].Style = IndicatorStyle.TextFore; // If, Else, Switch,..
            Indicators[14].ForeColor = Color.FromArgb(0x00, 0x80, 0x00);

            Indicators[15].Style = IndicatorStyle.TextFore; // If, Else, Switch,..
            Indicators[15].ForeColor = Color.FromArgb(0x00, 0x80, 0x00);

            Indicators[16].Style = IndicatorStyle.StraightBox; //FInd Code
            Indicators[16].ForeColor = Color.Yellow;


            Indicators[1].Style = IndicatorStyle.StraightBox;
            Indicators[1].Under = true;
            Indicators[1].ForeColor = Color.Red;

            UseTabs = true;
            TabWidth = 4;
            IndentWidth = 4;
            WrapMode = WrapMode.None;


            DebounceTimer.Interval = 100;

            DebounceTimer.Tick += async (s, e) =>
            {
                await UpdateRoslyn();
                DebounceTimer.Stop();
            };


            CharAdded += async (s, e) =>
            {

                char c = (char)e.Char;
                DebounceTimer.Stop(); DebounceTimer.Start();
            };

            CharAdded += (sender, e) =>
            {
                var pos = CurrentPosition;
                var ch = (char)e.Char;

                switch (ch)
                {
                    case '(':
                        InsertText(pos, ")");
                        GotoPosition(pos);
                        break;
                    case '[':
                        InsertText(pos, "]");
                        GotoPosition(pos);
                        break;
                    case '{':
                        InsertText(pos, "}");
                        GotoPosition(pos);
                        break;
                    case '\'':
                        InsertText(pos, "'");
                        GotoPosition(pos);
                        break;
                    case '"':
                        InsertText(pos, "\"");
                        GotoPosition(pos);
                        break;
                    case '`':
                        InsertText(pos, "`");
                        GotoPosition(pos);
                        break;

                        // Optional: Komma oder andere Zeichen
                        // case ',':
                        //     ...
                        //     break;
                }
            };




            ApplyLightTheme();

            CharAdded += OnlineFormat_CharAdded;
            KeyDown += DocumentEditor_KeyDown;
            KeyDown += ProtectedLines_KeyDown;

            KeyUp += async (s, e) =>
            {

                if (e.Control && e.KeyCode == Keys.Z) await UpdateRoslyn();
                if (e.Control && e.KeyCode == Keys.V) await UpdateRoslyn();
            };
        }

        private void ProtectedLines_KeyDown(object sender, KeyEventArgs e)
        {
            int currentLine = LineFromPosition(CurrentPosition);


            if (e.Control && e.KeyCode == Keys.A)
            {
                e.SuppressKeyPress = true;

                // Sichtbare Zeilen ermitteln
                var visibleLines = Enumerable.Range(0, Lines.Count)
                                             .Where(i => !LineIsProtected(i))
                                             .ToList();

                if (visibleLines.Count > 0)
                {
                    int startPos = Lines[visibleLines.First()].Position;
                    int endPos = Lines[visibleLines.Last()].EndPosition;

                    SelectionStart = startPos;
                    SelectionEnd = endPos;
                    CurrentPosition = endPos;
                }
            }

            if (e.KeyCode == Keys.Up)
            {
                int targetLine = currentLine - 1;
                while (targetLine >= 0 && LineIsProtected(targetLine))
                    targetLine--;

                if (targetLine >= 0)
                {
                    int pos = Lines[targetLine].Position;
                    CurrentPosition = pos;
                    SelectionStart = pos;
                    SelectionEnd = pos;
                }

                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                int targetLine = currentLine + 1;
                while (targetLine < Lines.Count && LineIsProtected(targetLine))
                    targetLine++;

                if (targetLine < Lines.Count)
                {
                    int pos = Lines[targetLine].Position;
                    CurrentPosition = pos;
                    SelectionStart = pos;
                    SelectionEnd = pos;
                }

                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
            {
                if (LineIsProtected(currentLine))
                {
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Control && (e.KeyCode == Keys.X || e.KeyCode == Keys.V)) // Cut oder Paste
            {
                if (LineIsProtected(currentLine))
                {
                    e.SuppressKeyPress = true;
                }
            }
        }

        private List<LineRange> protectedRanges = new List<LineRange>();
        private List<LineRange> hidenRanges = new List<LineRange>();

        private bool linesHiden = false;

        public void HideProtectLines(int start, int stop)
        {
            protectedRanges.Add(new LineRange(start, stop));
            hidenRanges.Add(new LineRange(start, stop));
            HideLines(start, stop);
            linesHiden = true;
        }

        public bool LineIsProtected(int line)
        {
            foreach (LineRange range in protectedRanges)
            {
                if (line >= range.Start && line <= range.End)
                    return true;
            }
            return false;
        }


        public void ToggleHidenLines()
        {
            if (linesHiden)
            {
                foreach (LineRange range in hidenRanges)
                {
                    ShowLines(range.Start, range.End);
                    linesHiden = false;

                }
            }
            else
            {
                foreach (LineRange range in hidenRanges)
                {
                    HideLines(range.Start, range.End);
                    linesHiden = true;

                }
            }

        }

        internal static class ScintillaConstants
        {
            public const int SCI_SETLINEINDENTATION = 2126;
            public const int SCI_GETLINEINDENTPOSITION = 2128;
            public const int SCI_GETFOLDLEVEL = 2223;
            public const int SCI_TOGGLEFOLD = 2231;
            public const int SC_FOLDLEVELHEADERFLAG = 0x2000;
            public const int SCI_EXPANDCHILDREN = 2239;
            public const int SCI_GETFOLDEXPANDED = 2230;
            public const int SCI_GETLASTCHILD = 2224;
            public const int SCI_LINEFROMPOSITION = 2166;
            public const int SCI_POSITIONFROMLINE = 2167;
            public const int SCI_FINDCOLUMN = 2456;
        }
        private void OnlineFormat_CharAdded(object sender, CharAddedEventArgs e)
        {
           
            if (e.Char == '\n')
            {
                BeginInvoke(new Action(() =>
                {
                    int currentLine = LineFromPosition(CurrentPosition);
                    int newLine = currentLine;

                    int indentSpaces = GetBraceIndentLevel(Text, CurrentPosition, IndentWidth);

                    DirectMessage(
                        ScintillaConstants.SCI_SETLINEINDENTATION,
                        new IntPtr(newLine),
                        new IntPtr(indentSpaces)
                    );

                    var pos = DirectMessage(
                        ScintillaConstants.SCI_GETLINEINDENTPOSITION,
                        new IntPtr(newLine),
                         IntPtr.Zero
                      );

                    GotoPosition((int)pos);
                }));
            }
        }
        private int GetBraceIndentLevel(string text, int position, int indentSize = 4)
        {
            int level = 0;
            for (int i = 0; i < position; i++)
            {
                char c = text[i];
                if (c == '{') level++;
                else if (c == '}') level = Math.Max(0, level - 1);
            }

            // Sonderfall: Wenn direkt vor dem Cursor eine öffnende Klammer steht
            if (position > 0 && text[position - 1] == '{')
                level++;

            return level * indentSize;
        }
        private void DocumentEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back) DebounceTimer.Start();
            if (e.KeyCode == Keys.Tab) DebounceTimer.Start();

            if (e.Control && e.KeyCode == Keys.K)
            {
                _awaitingCtrlK = true;
                _chordTimer.Stop();
                _chordTimer.Start();
                e.SuppressKeyPress = true;
                return;
            }
            if (_awaitingCtrlK && e.Control && e.KeyCode == Keys.C)
            {
                e.SuppressKeyPress = true;
                _awaitingCtrlK = false; _chordTimer.Stop();
                CommentSelection();

                if (UpdateRoslyn != null)
                    _ = TriggerUpdateAsync();

                return;
            }
            if (_awaitingCtrlK && e.Control && e.KeyCode == Keys.U)
            {
                e.SuppressKeyPress = true;
                _awaitingCtrlK = false; _chordTimer.Stop();
                UncommentSelection();

                if (UpdateRoslyn != null)
                    _ = TriggerUpdateAsync();

                return;
            }

            if (e.Control && e.KeyCode == Keys.C)
            {
                CopyWithFoldedBlockSupport();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }


        }
        public void RefreshView()
        {
            if (Theme.IsDark)
                ApplyDarkTheme();
            else
                ApplyLightTheme();

        }
        private void CommentSelection()
        {
            int selStart = SelectionStart;
            int selEnd = SelectionEnd;
            if (selEnd < selStart) (selStart, selEnd) = (selEnd, selStart);
            if (selStart == selEnd)
            {
                // keine Selektion -> aktuelle Zeile
                int line = LineFromPosition(selStart);
                selStart = Lines[line].Position;
                selEnd = Lines[line].EndPosition;
            }
            int startLine = LineFromPosition(selStart);
            int endLine = LineFromPosition(Math.Max(selEnd - 1, selStart));
            BeginUndoAction();
            try
            {
                int delta = 0;
                for (int line = startLine; line <= endLine; line++)
                {
                    int lineStart = Lines[line].Position;
                    int lineEnd = Lines[line].EndPosition; // inkl. \r\n vor letztem char
                    // Finde erste nicht-Whitespace Position
                    int p = lineStart;
                    while (p < lineEnd)
                    {
                        int ch = GetCharAt(p);
                        if (ch != ' ' && ch != '\t') break;
                        p++;
                    }
                    if (p >= lineEnd) continue; // leere / whitespace-only Zeile
                    // Prüfen ob bereits // vorhanden
                    bool already = p + 1 < lineEnd && GetCharAt(p) == '/' && GetCharAt(p + 1) == '/';
                    if (already) continue;
                    InsertText(p, "//");
                    if (line == startLine) selStart += 2; // Cursor verschiebt sich nach Einfügung vor Start
                    selEnd += 2;
                    delta += 2;
                }
                // Auswahl erneut setzen (optional beibehalten)
                SetSelection(selEnd, selStart); // ScintillaNET erwartet anchor/caret; Umkehren damit Selektion gleich bleibt
            }
            finally { EndUndoAction(); }
        }
        private void UncommentSelection()
        {
            int selStart = SelectionStart;
            int selEnd = SelectionEnd;
            if (selEnd < selStart) (selStart, selEnd) = (selEnd, selStart);
            if (selStart == selEnd)
            {
                int line = LineFromPosition(selStart);
                selStart = Lines[line].Position;
                selEnd = Lines[line].EndPosition;
            }
            int startLine = LineFromPosition(selStart);
            int endLine = LineFromPosition(Math.Max(selEnd - 1, selStart));
            BeginUndoAction();
            try
            {
                for (int line = startLine; line <= endLine; line++)
                {
                    int lineStart = Lines[line].Position;
                    int lineEnd = Lines[line].EndPosition;
                    int p = lineStart;
                    while (p < lineEnd)
                    {
                        int ch = GetCharAt(p);
                        if (ch != ' ' && ch != '\t') break;
                        p++;
                    }
                    if (p + 1 >= lineEnd) continue;
                    if (GetCharAt(p) == '/' && GetCharAt(p + 1) == '/')
                    {
                        DeleteRange(p, 2);
                        if (p < selStart) selStart = Math.Max(lineStart, selStart - 2);
                        selEnd = Math.Max(selStart, selEnd - 2);
                    }
                }
                SetSelection(selEnd, selStart);
            }
            finally { EndUndoAction(); }
        }
        public void ApplyTheme()
        {
            if (Theme.IsDark)
                ApplyDarkTheme();
            else
                ApplyLightTheme();
        }
        public void ApplyLightTheme()
        {
            _currentTheme = EditorTheme.Light;

            //Background

            Color _backColor = Color.FromArgb(230, 230, 230);

            _currentTheme = EditorTheme.Light;
            // Grundstil (Visual Studio Light / VS Code Light+ nahe)
            StyleResetDefault();
            Styles[Style.Default].Font = "Cascadia Code";
            Styles[Style.Default].Size = 10;
            Styles[Style.Default].BackColor = Active ? Color.White : Color.FromArgb(180, 180, 180);                // Editor Hintergrund
            Styles[Style.Default].ForeColor = Color.Black;                // Standard Text
            StyleClearAll();
            CaretForeColor = Color.Black;
            SetSelectionBackColor(true, Color.FromArgb(0xD7, 0xE4, 0xF2)); // Standard VS Light Selektionsblau ähnlich
            WhitespaceSize = 2;
            ViewWhitespace = WhitespaceMode.Invisible;

            // Brace Highlight (angepasst an VS Light – leicht gelblicher Hintergrund)
            Styles[Style.BraceLight].ForeColor = Color.Black;
            Styles[Style.BraceLight].BackColor = Color.FromArgb(0xFF, 0xF4, 0xC1);
            Styles[Style.BraceBad].ForeColor = Color.White;
            Styles[Style.BraceBad].BackColor = Color.FromArgb(0xE5, 0x51, 0x51);

            // Folding Marker Farben (neutral grau wie VS)
            var foldFore = Color.FromArgb(0x80, 0x80, 0x80);
            var foldBack = foldFore;
            for (int i = Marker.FolderEnd; i <= Marker.FolderOpenMid; i++)
            {
                Markers[i].SetForeColor(foldFore);
                Markers[i].SetBackColor(foldBack);
            }

            // Visual Studio Light / VS Code Light+ typische Farben:
            // Keywords: #0000FF
            // Strings: #A31515
            // Comments: #008000
            // Numbers: #098658
            // Method: #795E26
            // Types (Class/Struct/Interface/Enum): #267F99 (annähernd #2B91AF aus altem VS; etwas entsättigt für moderne Light Themes)
            // Property: #001080
            // Field: #D4D4D4 (Standard)

            Color vsKeyword = Color.FromArgb(0x00, 0x00, 0xFF);   // Blau
            Color vsString = Color.FromArgb(0xA3, 0x15, 0x15);    // Dunkelrot
            Color vsComment = Color.FromArgb(0x00, 0x80, 0x00);   // Grün
            Color vsNumber = Color.FromArgb(0x09, 0x86, 0x58);    // Grünlich
            Color vsMethod = Color.FromArgb(0x79, 0x5E, 0x26);    // Braun
            Color vsType = Color.FromArgb(0x26, 0x7F, 0x99);      // Cyan / Type
            Color vsProperty = Color.FromArgb(0x00, 0x10, 0x80);  // Dunkelblau
            Color Commands = Color.FromArgb(183, 0, 219);
            Color NameSpaceName = Color.FromArgb(0x26, 0x7F, 0x99);

            // Errors (bleibt Rot)
            Indicators[0].ForeColor = Color.Red;
            // Methoden
            Indicators[2].ForeColor = vsMethod;
            // Klassen
            Indicators[3].ForeColor = vsType;
            // Interfaces
            Indicators[4].ForeColor = vsType;
            // Structs
            Indicators[5].ForeColor = vsType;
            // Enums
            Indicators[6].ForeColor = vsType;
            // Delegates (wie Methoden leicht abheben)
            Indicators[7].ForeColor = vsMethod;
            // Properties
            Indicators[8].ForeColor = vsProperty;
            // Fields
            Indicators[9].ForeColor = Color.Black;
            // Keywords
            Indicators[10].ForeColor = vsKeyword;
            // Numbers
            Indicators[11].ForeColor = vsNumber;
            // Strings
            Indicators[12].ForeColor = vsString;
            // Comments
            Indicators[13].ForeColor = vsComment;

            //Commands
            Indicators[14].ForeColor = Commands;

            //NameSpace
            Indicators[15].ForeColor = NameSpaceName;

            // Caret Line
            CaretLineVisible = true;
            CaretLineBackColor = Color.FromArgb(0xF3, 0xF9, 0xFF);

            // Line Numbers / Folding Ränder
            Styles[Style.LineNumber].BackColor = Color.White;
            SetFoldMarginColor(true, Color.White);
            SetFoldMarginHighlightColor(true, Color.White);


            //Error Style
            int STYLE_ERROR = 20;
            Styles[STYLE_ERROR].BackColor = Color.Red;
            Styles[STYLE_ERROR].ForeColor = Color.White; // optional

            // Autocomplete Farben
            AutocompleteListBackColor = Color.White;
            AutocompleteListTextColor = Color.Black;
        }
        public void ApplyDarkTheme()
        {
            _currentTheme = EditorTheme.Dark;
            Color _backColor = Color.FromArgb(40, 40, 40);



            _currentTheme = EditorTheme.Dark;
            // Grundstil (Visual Studio Dark / VS Code Dark+)
            StyleResetDefault();
            Styles[Style.Default].Font = "Cascadia Code";
            Styles[Style.Default].Size = 10;
            Styles[Style.Default].BackColor = Active ? Color.FromArgb(25, 25, 25) : Color.FromArgb(80, 25, 25);          // #1E1E1E
            Styles[Style.Default].ForeColor = Color.FromArgb(200, 200, 200);          // Standard Text
            StyleClearAll();
            CaretForeColor = Color.White;
            SetSelectionBackColor(true, Color.FromArgb(0x26, 0x4F, 0x78));               // Auswahlblau dunkel
            WhitespaceSize = 2;
            ViewWhitespace = WhitespaceMode.Invisible;

            // Brace Highlight (VS Dark ähnlich)
            Styles[Style.BraceLight].ForeColor = Color.FromArgb(0xFF, 0xC0, 0x40);
            Styles[Style.BraceLight].BackColor = Color.FromArgb(0x33, 0x33, 0x33);
            Styles[Style.BraceBad].ForeColor = Color.White;
            Styles[Style.BraceBad].BackColor = Color.FromArgb(0x90, 0x2B, 0x2B);

            // Folding Marker (leicht kontrastreich)
            var foldFore = Color.FromArgb(0xAA, 0xAA, 0xAA);
            var foldBack = Color.FromArgb(0x2D, 0x2D, 0x2D);
            for (int i = Marker.FolderEnd; i <= Marker.FolderOpenMid; i++)
            {
                Markers[i].SetForeColor(foldFore);
                Markers[i].SetBackColor(foldBack);
            }

            // VS Dark / VS Code Dark+ Referenzfarben:
            // Keyword: #569CD6
            // String: #CE9178
            // Comment: #6A9955
            // Number: #B5CEA8
            // Method: #DCDCAA
            // Class: #4EC9B0
            // Interface: #B8D7A3
            // Struct: #86C691
            // Enum: #B8D7A3
            // Delegate: #DCDCAA (wie Method)
            // Property: #9CDCFE
            // Field: #D4D4D4 (Standard)

            Color vsDarkKeyword = Color.FromArgb(0x56, 0x9C, 0xD6);
            Color vsDarkString = Color.FromArgb(0xCE, 0x91, 0x78);
            Color vsDarkComment = Color.FromArgb(84, 153, 85);
            Color vsDarkNumber = Color.FromArgb(0xB5, 0xCE, 0xA8);
            Color vsDarkMethod = Color.FromArgb(0xDC, 0xDC, 0xAA);
            Color vsDarkClass = Color.FromArgb(0x4E, 0xC9, 0xB0);
            Color vsDarkInterface = Color.FromArgb(0xB8, 0xD7, 0xA3);
            Color vsDarkStruct = Color.FromArgb(0x86, 0xC6, 0x91);
            Color vsDarkEnum = Color.FromArgb(0xB8, 0xD7, 0xA3);
            Color vsDarkDelegate = vsDarkMethod;
            Color vsDarkProperty = Color.FromArgb(0x9C, 0xDC, 0xFE);
            Color vsDarkField = Color.FromArgb(0xD4, 0xD4, 0xD4);
            Color Commands = Color.FromArgb(216, 160, 215);
            Color NameSpaceName = vsDarkClass;

            // Errors
            Indicators[0].ForeColor = Color.FromArgb(0xF4, 0x43, 0x36);
            // Methoden
            Indicators[2].ForeColor = vsDarkMethod;
            // Klassen
            Indicators[3].ForeColor = vsDarkClass;
            // Interfaces
            Indicators[4].ForeColor = vsDarkInterface;
            // Structs
            Indicators[5].ForeColor = vsDarkStruct;
            // Enums
            Indicators[6].ForeColor = vsDarkEnum;
            // Delegates
            Indicators[7].ForeColor = vsDarkDelegate;
            // Properties
            Indicators[8].ForeColor = vsDarkProperty;
            // Fields
            Indicators[9].ForeColor = vsDarkField;
            // Keywords
            Indicators[10].ForeColor = vsDarkKeyword;
            // Numbers
            Indicators[11].ForeColor = vsDarkNumber;
            // Strings
            Indicators[12].ForeColor = vsDarkString;
            // Comments
            Indicators[13].ForeColor = vsDarkComment;

            //Commands
            Indicators[14].ForeColor = Commands;
            //NameSpace
            Indicators[15].ForeColor = NameSpaceName;

            CaretLineVisible = true;
            CaretLineBackColor = Color.FromArgb(0x2A, 0x2A, 0x2A);


            // Line Numbers & Folding
            Styles[Style.LineNumber].BackColor = Color.FromArgb(25, 25, 25); ;
            SetFoldMarginColor(true, Color.FromArgb(25, 25, 25));
            SetFoldMarginHighlightColor(true, Color.FromArgb(25, 25, 25));


            // Autocomplete
            AutocompleteListBackColor = Color.FromArgb(0x25, 0x25, 0x25);
            AutocompleteListTextColor = Color.FromArgb(0xD4, 0xD4, 0xD4);


        }
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DocumentEditor
            // 
            this.HScrollBar = false;
            this.VScrollBar = false;
            this.ZoomChanged += new System.EventHandler<System.EventArgs>(this.DocumentEditor_ZoomChanged);
            this.ResumeLayout(false);

        }
        public System.Drawing.Font GetFont()
        {
            var style = Styles[Style.Default];
            string fontName = style.Font;
            int baseSize = style.Size;
            int zoom = Zoom;
            int effectiveSize = baseSize + zoom;

            return new System.Drawing.Font(fontName, effectiveSize);
        }
        private void DocumentEditor_ZoomChanged(object sender, EventArgs e)
        {

        }

        public void SelectRange(int pos, int length)
        {
            int lineNumber = LineFromPosition(pos);
            int lineStartPos = Lines[lineNumber].Position;
            GotoPosition(lineStartPos);
            SelectionStart = pos;
            SelectionEnd = pos + length;
        }

        int lastHighlightedStart = -1;
        int lastHighlightedLength = 0;
        public void HighlightLine(int lineNumber, System.Drawing.Color color)
        {
            // Vorherige Markierung löschen
            IndicatorClearRange(16, TextLength);

            FirstVisibleLine = lineNumber - 17;
            // Neue Positionen berechnen
            int startPos = Lines[lineNumber].Position;
            int endPos = Lines[lineNumber].EndPosition;


            IndicatorCurrent = 16;
            Indicators[16].ForeColor = color;
            IndicatorFillRange(startPos, endPos - startPos);

            // Position merken
            lastHighlightedStart = startPos;
            lastHighlightedLength = endPos - startPos;
        }

        public async Task FormatDocumentAsync()
        {
            string input = Text;

            string? roslyn = await RoslynService.FormatCSharpAsync(
            input,
            useTabs: false,
            indentSize: 4
            );
            string output = roslyn ?? SimpleIndentFormatter.Reindent(input, 4);
            if (!string.Equals(input, output, StringComparison.Ordinal))
            {
                int pos = CurrentPosition; // Cursor merken
                Text = output;

                SetEmptySelection(Math.Min(pos, TextLength));
            }
        }

        public void ToggleViewEol()
        {
            if (ViewEol)
            {
                ViewEol = false;         // Zeigt die EOL-Zeichen visuell an
                ViewWhitespace = WhitespaceMode.Invisible;

            }
            else
            {
                EolMode = Eol.CrLf;
                ViewEol = true;         // Zeigt die EOL-Zeichen visuell an
                ViewWhitespace = WhitespaceMode.VisibleAlways;
            }
        }

        ContextMenuStrip EditorContextMenu = new ContextMenuStrip();
        private void InitEditorContextMenu()
        {


            var miGoto = new ToolStripMenuItem("Go To Definition (F12)");
            miGoto.Click += async (_, __) => await TriggerGoToDefinitionAsync();
            EditorContextMenu.Items.Add(miGoto);

            var miRename = new ToolStripMenuItem("Rename...");
            miRename.Click += async (_, __) => await TriggerRenameSymbolAsync();
            EditorContextMenu.Items.Add(miRename);

            EditorContextMenu.Items.Add(new ToolStripSeparator());

            var miCollapse = new ToolStripMenuItem("Toggle folds at current indent level");
            miCollapse.Click += (_, __) => ToggleAllFoldsAtCurrentLevel();
            EditorContextMenu.Items.Add(miCollapse);

            var miExpand = new ToolStripMenuItem("Expand All");
            miExpand.Click += (_, __) => ExpandAllFolds();
            EditorContextMenu.Items.Add(miExpand);

            EditorContextMenu.Items.Add(new ToolStripSeparator());

            ContextMenuStrip = EditorContextMenu;
        }
        private void ToggleAllFoldsAtCurrentLevel()
        {
            int currentLine = LineFromPosition(CurrentPosition);
            string text = Text;

            // Level der aktuellen Zeile bestimmen
            int lineStartPos = Lines[currentLine].Position;
            int currentLevel = GetBraceIndentLevel(text, lineStartPos, IndentWidth) / IndentWidth;

            int totalLines = Lines.Count;
            for (int line = 0; line < totalLines; line++)
            {
                int linePos = Lines[line].Position;
                int lineLevel = GetBraceIndentLevel(text, linePos, IndentWidth) / IndentWidth;

                if (lineLevel == currentLevel)
                {
                    // Check ob Zeile foldbar ist (hat also ein Fold-Header)
                    var foldLevel = (int)DirectMessage(
                        ScintillaConstants.SCI_GETFOLDLEVEL,
                        new IntPtr(line),
                        IntPtr.Zero
                    );

                    // Scintilla verwendet das 0x2000-Bit (SC_FOLDLEVELHEADERFLAG) für foldbare Zeilen
                    bool isFoldHeader = (foldLevel & 0x2000) != 0;
                    if (isFoldHeader)
                    {
                        DirectMessage(
                            ScintillaConstants.SCI_TOGGLEFOLD,
                            new IntPtr(line),
                            IntPtr.Zero
                        );
                    }
                }
            }
        }
        public void ExpandAllFolds()
        {
            int lineCount = Lines.Count;

            for (int line = 0; line < lineCount; line++)
            {
                // Fold-Level holen
                int foldLevel = (int)DirectMessage(
                    ScintillaConstants.SCI_GETFOLDLEVEL,
                    new IntPtr(line),
                    IntPtr.Zero
                );

                bool isFoldHeader = (foldLevel & ScintillaConstants.SC_FOLDLEVELHEADERFLAG) != 0;
                if (isFoldHeader)
                {
                    // Zeile vollständig expandieren (rekursiv)
                    DirectMessage(
                        ScintillaConstants.SCI_EXPANDCHILDREN,
                        new IntPtr(line),
                        new IntPtr(1) // 1 = expand, 0 = collapse
                    );
                }
            }
        }

        public void CopyWithFoldedBlockSupport()
        {
            int selStart = SelectionStart;
            int line = LineFromPosition(selStart);

            // Prüfen ob aktuelle Zeile ein Fold-Header ist
            int foldLevel = (int)DirectMessage(
                ScintillaConstants.SCI_GETFOLDLEVEL,
                new IntPtr(line),
                IntPtr.Zero
            );

            bool isFoldHeader = (foldLevel & ScintillaConstants.SC_FOLDLEVELHEADERFLAG) != 0;
            if (isFoldHeader)
            {
                // Prüfen ob dieser Fold eingeklappt ist
                bool isExpanded = DirectMessage(
                    ScintillaConstants.SCI_GETFOLDEXPANDED,
                    new IntPtr(line),
                    IntPtr.Zero
                ) != IntPtr.Zero;

                if (!isExpanded)
                {
                    // Endzeile des Folds bestimmen
                    int lastLine = (int)DirectMessage(
                        ScintillaConstants.SCI_GETLASTCHILD,
                        new IntPtr(line),
                        new IntPtr(-1)
                    );

                    int startPos = Lines[line].Position;
                    int endPos = Lines[lastLine].EndPosition;

                    // Textbereich auslesen
                    string textToCopy = GetTextRange(startPos, endPos - startPos);

                    // In Zwischenablage
                    Clipboard.SetText(textToCopy);
                    return;
                }
            }

            // Fallback: normales Copy
            Copy();
        }
        public int GetPositionFromLineColumn(int line, int column)
        {
            return (int)DirectMessage(
                ScintillaConstants.SCI_FINDCOLUMN,
                new IntPtr(line),
                new IntPtr(column)
            );
        }

        public void FindReplace(string find, string replace, SearchFlags flags = SearchFlags.None)
        {

            //SearchFlags.None         // Keine besonderen Einschränkungen
            //SearchFlags.MatchCase    // Groß-/Kleinschreibung beachten
            //SearchFlags.WholeWord    // Nur ganze Wörter finden
            //SearchFlags.WordStart    // Nur Wörter, die mit dem Suchbegriff beginnen
            //SearchFlags.Regex        // Reguläre Ausdrücke verwende

            Debug.WriteLine("find '" + find + "'");

            if (string.IsNullOrEmpty(find))
                return;

            // Setze den Suchbereich auf das gesamte Dokument
            TargetStart = 0;
            TargetEnd = TextLength;

            SearchFlags = flags; // Optional: z. B. CaseSensitive, WholeWord, etc.

            int pos = SearchInTarget(find);

            while (pos >= 0)
            {
                ReplaceTarget(replace);

                // Suche ab dem Ende des letzten Ersetzens weiter
                TargetStart = pos + replace.Length;
                TargetEnd = TextLength;

                pos = SearchInTarget(find);
            }
        }







    }
    public static class SimpleIndentFormatter
    {
        public static string Reindent(string source, int indentSize = 4)
        {
            if (string.IsNullOrEmpty(source)) return source;
            var sb = new StringBuilder(source.Length + 1024);
            var lines = source.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            int level = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                var raw = lines[i];
                var trimmed = raw.Trim();
                if (trimmed.Length == 0) { sb.AppendLine(); continue; }

                // Wenn die Zeile mit '}' beginnt, vorher Level reduzieren
                if (trimmed.Length > 0 && trimmed[0] == '}')
                    level = Math.Max(0, level - 1);

                // Neue Einrückung berechnen
                int spaces = level * indentSize;
                sb.Append(' ', spaces);
                sb.AppendLine(trimmed);

                // Level-Anpassung je nach Brace-Bilanz am Ende der Zeile
                // (einfacher Heuristik-Ansatz; Strings/Comments werden nicht geparst)
                int open = CountChar(trimmed, '{');
                int close = CountChar(trimmed, '}');
                level += open - close;
                if (level < 0) level = 0; // robust gegen unbalancierte Fälle
            }
            return sb.ToString();
        }
        private static int CountChar(string s, char c)
        {
            int n = 0; foreach (var ch in s) if (ch == c) n++; return n;
        }
    }



}
