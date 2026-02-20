using Microsoft.Build.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.CodeAnalysis.Text;
using qbookCode.Controls.InputControls;
using qbookCode.Roslyn;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Forms;
using VPKSoft.ScintillaLexers;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using RoslynDocument = Microsoft.CodeAnalysis.Document; // Alias gegen Kollision mit ScintillaNET.Document


namespace qbookCode.Controls
{
   
   


    public class DocumentEditor : Scintilla
    {


        ControlAutoComplete AutoComplete;

        ControlSignatureHelper SignatureHelper;

        ReferencesHelperControl ReferencesList;

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

        public DataTable Output = new DataTable();
        public DataTable MethodesClasses = new DataTable();
        
        public CodeDocument Target;
        public bool HasErrors => Output.Rows.Count > 0;

        private readonly System.Windows.Forms.Timer _chordTimer = new() { Interval = 1500 };
        public System.Windows.Forms.Timer DebounceTimer = new System.Windows.Forms.Timer();

        public oPage Page { get; set; }

        public RoslynDocument KeyRoslynDoc;
        
        public bool Active 
        { 
            get {
                if (Target == null) return false;
                return Target.Active; 
            }
            set { Target.Active = value; }
        }
        public RoslynDocument RoslynDoc;
        public string Filename;

        public Func<Task> GoToDefinition;
        public Func<Task> RenameSymbol;

        public Form View { get; set; }

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
        {
            if (AutoComplete.Visible)
            {
                switch (keyData)
                {
                    case Keys.Down:
                        AutoComplete.Next();
                        return true;
                    case Keys.Up:
                        AutoComplete.Previous();
                        return true;
                    case Keys.Enter:
                        AutoComplete.Commit(completeText);
                        return true;
                    case Keys.Tab:
                        AutoComplete.Commit(completeText);
                        return true;

                    case Keys.Escape:
                        completeText = string.Empty;
                        AutoComplete.Hide();
                        return true;
                }  
            }

            if (SignatureHelper.Visible)
            {
                switch (keyData)
                {
                    case Keys.Down:
                        SignatureHelper.Next();
                        return true;
                    case Keys.Up:
                        SignatureHelper.Previous();
                        return true;
                    case Keys.Enter:
                        SignatureHelper.Commit();
                        return true;
                    case Keys.Tab:
                        SignatureHelper.Commit();
                        return true;

                    case Keys.Escape:
                        SignatureHelper.Hide();
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

        DataTable References = new DataTable();
        DataTable ReferencesFiltered = new DataTable();
        public DocumentEditor(CodeDocument doc, oPage page) : base()
        {
            References = new DataTable();
            References.Columns.Add("Method", typeof(string));           // Fully qualified (error format) for display
            References.Columns.Add("MethodName", typeof(string));       // Simple method name
            References.Columns.Add("ContainingType", typeof(string));   // Simple class/record/struct name
            References.Columns.Add("Document", typeof(string));
            References.Columns.Add("Line", typeof(int));
            References.Columns.Add("Span", typeof(string));
            References.Columns.Add("Code", typeof(string));

            ReferencesFiltered = References.Copy();

            Init();
            InitFolding();
            Page = page;
            Target = doc;
            Text = doc.Code;
            EmptyUndoBuffer();
            AutoComplete = new ControlAutoComplete(this);
            SignatureHelper = new ControlSignatureHelper(this);
            ReferencesList = new ReferencesHelperControl(this);



            this.HScrollBar = false;
            this.VScrollBar = false;
            this.ZoomChanged += new System.EventHandler<System.EventArgs>(this.DocumentEditor_ZoomChanged);
            this.ResumeLayout(false);

            Output.Columns.Add("Page", typeof(string));
            Output.Columns.Add("Class", typeof(string));
            Output.Columns.Add("Position", typeof(string));
            Output.Columns.Add("Length", typeof(int));
            Output.Columns.Add("Type", typeof(string));
            Output.Columns.Add("Description", typeof(string));
         

            MethodesClasses.Columns.Add("Row", typeof(int));
            MethodesClasses.Columns.Add("Name", typeof(string));

    

            if (Theme.IsDark)
                ApplyDarkTheme();
            else
                ApplyLightTheme();

           
           
            CharAdded += RoslynAutoComplete_CharAdded;
            CharAdded += SignatureHelper_CharAdded;
            CharAdded += AutoInsertSummary_CharAdded;
            KeyDown += RoslynAutoComplete_KeyDown;
            KeyDown += CursorUpDown;
            KeyDown += ReferenceList_KeyDown;


            //this.LostFocus += (s, e) =>
            //{
            //    completeText = string.Empty;
            //    AutoComplete.Hide();
            //    SignatureHelper.Hide();
            //};



            Click += (s, e) =>
            {
                IndicatorClearRange(16, TextLength);
            };

            MouseDwellTime = 400;
            DwellStart += DocumentEditor_DwellStart;
            DwellEnd += DocumentEditor_DwellEnd;
            // Optional for keyboard navigation:
            UpdateUI += DocumentEditor_UpdateUI;
        }

        private void CursorUpDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                int currentPos = CurrentPosition;
                int currentLine = LineFromPosition(currentPos);
                int currentColumn = GetColumn(currentPos);

                int targetLine;

                if (e.KeyCode == Keys.Up)
                {
                    targetLine = currentLine - 1;
                    while (targetLine >= 0 && LineIsProtected(targetLine))
                    {
                        targetLine--;
                    }
                }
                else // Keys.Down
                {
                    targetLine = currentLine + 1;
                    while (targetLine < Lines.Count && LineIsProtected(targetLine))
                    {
                        targetLine++;
                    }
                }

                if (targetLine >= 0 && targetLine < Lines.Count)
                {
                    // Find the position on the target line at the desired column.
                    int newPos = GetPositionFromLineColumn(targetLine, currentColumn);

                    // Set the new caret position.
                    // If the line is shorter, FindColumn correctly places the caret at the end of the line.
                    CurrentPosition = newPos;
                    SelectionStart = newPos;
                    SelectionEnd = newPos;
                }

                // We've handled it, so prevent further processing of this key event.
                e.SuppressKeyPress = true;
            }
        }

        public void HidePopups()
        {
            completeText = string.Empty;
            AutoComplete.Hide();
            SignatureHelper.Hide();
            ReferencesList.Hide();
        }



        #region AutoComplete
        string completeText = string.Empty;
        private async void RoslynAutoComplete_CharAdded(object sender, CharAddedEventArgs e)
        {
            if (!Target.Active) return;

            char c = (char)e.Char;

            // Sonderzeichen → Liste schließen
            if (char.IsWhiteSpace(c) || ";){}[]".Contains(c))
            {
                completeText = string.Empty;
                AutoComplete.Hide();
                return;
            }

            // Punkt → Prefix zurücksetzen und sofort Vorschläge anzeigen
            if (c == '.')
            {
                completeText = string.Empty; // Reset
                UpdateDocument();

                var suggestions = await Core.Roslyn.GetFullAutoCompleteSuggestionsAsync(
                    Target.Document,
                    Target.Code,
                    CurrentPosition,
                    string.Empty // kein Prefix
                );

                if (suggestions.Count > 0)
                {
                    AutoComplete.ShowCompletionList(suggestions);
                    AutoComplete.ShowSummaryForSelectedItem();
                }
                else
                    AutoComplete.Hide();

                return; // WICHTIG: Hier abbrechen, damit kein weiteres Handling passiert
            }

            // Normaler Buchstabe → Prefix erweitern
            completeText += c;

            UpdateDocument();

            var filteredSuggestions = await Core.Roslyn.GetFullAutoCompleteSuggestionsAsync(
                Target.Document,
                Target.Code,
                CurrentPosition,
                completeText
            );

            //foreach (var item in filteredSuggestions)
            //    Debug.WriteLine(item);

            if (filteredSuggestions.Count > 0)
                AutoComplete.ShowCompletionList(filteredSuggestions);
           // else
             //   Hide();
        }

        private async void ReferenceList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                ReferencesList.Hide();

            }

        }


        private async void RoslynAutoComplete_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Back && completeText.Length > 0)
            {
                completeText = completeText.Substring(0, completeText.Length - 1);
                UpdateDocument(); // Synchronisiert den Editor-Inhalt mit Roslyn

                var suggestions = await Core.Roslyn.GetFullAutoCompleteSuggestionsAsync(
                       Target.Document,
                       Target.Code,
                       CurrentPosition,
                       completeText // Prefix übergeben
                   );

                if (suggestions.Count > 0)
                {
                    AutoComplete.ShowCompletionList(suggestions);
                }
                else
                {
                    //  Hide();
                }
            }
        }

        #endregion


        #region SignatureHelp

        public async void SignatureHelper_CharAdded(object sender, CharAddedEventArgs e)
        {
            char c = (char)e.Char;
            if (c == '(' || c == ',')
            {
                await SignatureHelper.ShowSignaturePopupAsync();
                SignatureHelper.ShowDocumentation();
            }
         

            if (c == ';')
                    SignatureHelper.Hide();
        }

        #endregion


        public void Init()
        {

            InitEditorContextMenu();
      
          //  HScrollBar = false;
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

            Indicators[20].Style = IndicatorStyle.TextFore; // has refs
            Indicators[20].ForeColor = Color.Brown; // cyan-ish
            Indicators[20].Under = false;

            Indicators[21].Style = IndicatorStyle.Squiggle; // no refs
            Indicators[21].ForeColor = Color.Blue; // red-ish
            Indicators[21].Under = true;


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
                DebounceTimer.Stop();
                await UpdateRoslyn("DebounceTimer");
                
            };


            CharAdded += async (s, e) =>
            {
              //  Core.ThisBook.Modified = true;
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

           // CharAdded += OnlineFormat_CharAdded;
            CharAdded += onlineFormat_CharAdded;
            InsertCheck += onlineFormat_InsertCheck;
            KeyDown += DocumentEditor_KeyDown;
            KeyDown += ProtectedLines_KeyDown;
            KeyUp += async (s, e) =>
            {

                if (e.Control && e.KeyCode == Keys.Z) await UpdateRoslyn("Keys CTRLZ");
                if (e.Control && e.KeyCode == Keys.V) await UpdateRoslyn("Keys CTRLV");
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
        public void ResetHideProteced()
        {
            protectedRanges.Clear();
            hidenRanges.Clear();
        }
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
                    ShowLines(0, range.End);
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
            public const int SCI_COLOURISE = 4003;
        }
        private void OnlineFormat_CharAdded(object sender, CharAddedEventArgs e)
        {
   
            if (e.Char == '\n')
            {
                BeginInvoke(new System.Action(() =>
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

        #region onlineFormat
        /// <summary>
        /// Beim Einfügen von Text eingreifen (hier beim Enter),
        /// um automatisch den richtigen Einzug mit einzufügen.
        /// </summary>
        private void onlineFormat_InsertCheck(object sender, InsertCheckEventArgs e)
        {
            var sci = (Scintilla)sender;

            // Reagieren wir nur auf Enter (CR/LF je nach Einstellung)
            if (!e.Text.EndsWith("\n")) return;

            // Aktuelle Zeile, in die der Umbruch eingefügt wird (neue Zeile = curLine+1)
            int curPos = e.Position;
            int curLine = sci.LineFromPosition(curPos);
            var prev = sci.Lines[Math.Max(0, curLine)];        // Zeile vor der NEUEN Zeile (die, aus der wir Enter drücken)
            var next = sci.Lines[Math.Min(curLine + 1, sci.Lines.Count - 1)];

            // Basisindent = Einrückung der vorherigen Zeile
            int targetIndent = prev.Indentation;

            // Text der vorherigen Zeile ohne Zeilenende
            string prevText = prev.Text.TrimEnd('\r', '\n');
            string prevTrim = prevText.TrimEnd();

            // Wenn vorherige Zeile mit "{" endet -> eine Stufe mehr
            if (prevTrim.EndsWith("{"))
                targetIndent += sci.IndentWidth;

            // Blick nach vorne: Wenn die neue Zeile direkt mit '}' beginnt, wieder reduzieren
            int lineStartPos = next.Position; // Beginn der neuen Zeile
            int lookaheadLen = Math.Min(2, sci.TextLength - lineStartPos);
            string lookahead = lookaheadLen > 0 ? sci.GetTextRange(lineStartPos, lookaheadLen) : string.Empty;

            if (lookahead.Length > 0 && lookahead[0] == '}')
                targetIndent = Math.Max(0, targetIndent - sci.IndentWidth);

            // Whitespace-String basierend auf Tabs/Spaces erzeugen
            string indentStr = BuildIndentString(sci, targetIndent);

            // Den eingefügten Text (das Enter) um den Indent erweitern
            // Achtung: e.Text enthält das CR/LF – wir hängen unmittelbar den Indent an
            e.Text += indentStr;
        }

        /// <summary>
        /// „Brace outdent“ bei Eingabe von '}' in leerer/whitespace-only Zeile.
        /// </summary>
        private void onlineFormat_CharAdded(object sender, CharAddedEventArgs e)
        {
            if (e.Char != '}') return;

            var sci = (Scintilla)sender;
            int lineIndex = sci.LineFromPosition(sci.CurrentPosition);
            var line = sci.Lines[lineIndex];

            // Nur wenn vor der '}'-Klammer nur Whitespace steht
            string text = line.Text.TrimEnd('\r', '\n');
            int firstNonWs = FirstNonWhitespaceIndex(text);
            if (firstNonWs < 0) return; // leere Zeile

            // Steht die '}' direkt am ersten Nicht-Whitespace?
            if (text[firstNonWs] != '}') return;

            // Zielindentation = Einrückung der vorherigen sinnvollen Zeile
            int targetIndent = 0;
            if (lineIndex > 0)
            {
                var prev = sci.Lines[lineIndex - 1];
                targetIndent = prev.Indentation;

                // Wenn vorige Zeile mit '{' endet, stehen wir auf gleicher Ebene wie die '{'-Zeile
                string prevTrim = prev.Text.TrimEnd('\r', '\n').TrimEnd();
                if (prevTrim.EndsWith("{"))
                    targetIndent = Math.Max(0, targetIndent - sci.IndentWidth);
            }

            // Setze Einzug der aktuellen Zeile neu
            line.Indentation = targetIndent;

            // Cursor vor die '}' schieben (also hinter die Whitespaces)
            int start = line.Position;
            int i = start;
            while (i < sci.TextLength)
            {
                char c = (char)sci.GetCharAt(i);
                if (c == ' ' || c == '\t') { i++; continue; }
                break;
            }
            sci.GotoPosition(i);
        }

        /// <summary>
        /// Baut Whitespaces passend zu UseTabs/IndentWidth/TabWidth für eine gegebene Spalteneinrückung.
        /// </summary>
        private static string BuildIndentString(Scintilla sci, int indentationColumns)
        {
            if (indentationColumns <= 0) return string.Empty;

            if (sci.UseTabs)
            {
                int tabs = indentationColumns / sci.TabWidth;
                int spaces = indentationColumns % sci.TabWidth;
                return new string('\t', tabs) + new string(' ', spaces);
            }
            else
            {
                return new string(' ', indentationColumns);
            }
        }

        private static int FirstNonWhitespaceIndex(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] != ' ' && s[i] != '\t') return i;
            }
            return -1;
        }


        #endregion

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

                UpdateRoslyn("Keys CTRLK C");
                return;
            }
            if (_awaitingCtrlK && e.Control && e.KeyCode == Keys.U)
            {
                e.SuppressKeyPress = true;
                _awaitingCtrlK = false; _chordTimer.Stop();
                UncommentSelection();

                UpdateRoslyn("Keys CTRLK U");
                return;
            }

            if (e.Control && e.KeyCode == Keys.C)
            {
                CopyWithFoldedBlockSupport();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }

            if (e.Control && e.KeyCode == Keys.F)
            {
                Core.Explorer.FindReplace.Show();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            if (e.Control && e.KeyCode == Keys.H)
            {
                Core.Explorer.FindReplace.Show();
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
            Styles[ScintillaNET.Style.Default].Font = "Cascadia Code";
            Styles[ScintillaNET.Style.Default].Size = 10;
            Styles[ScintillaNET.Style.Default].BackColor = Active ? Color.White : Color.FromArgb(180, 180, 180);                // Editor Hintergrund
            Styles[ScintillaNET.Style.Default].ForeColor = Color.Black;                // Standard Text
            StyleClearAll();
            CaretForeColor = Color.Black;
            SetSelectionBackColor(true, Color.FromArgb(0xD7, 0xE4, 0xF2)); // Standard VS Light Selektionsblau ähnlich
            WhitespaceSize = 2;
            ViewWhitespace = WhitespaceMode.Invisible;

            // Brace Highlight (angepasst an VS Light – leicht gelblicher Hintergrund)
            Styles[ScintillaNET.Style.BraceLight].ForeColor = Color.Black;
            Styles[ScintillaNET.Style.BraceLight].BackColor = Color.FromArgb(0xFF, 0xF4, 0xC1);
            Styles[ScintillaNET.Style.BraceBad].ForeColor = Color.White;
            Styles[ScintillaNET.Style.BraceBad].BackColor = Color.FromArgb(0xE5, 0x51, 0x51);

     

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


            Indicators[20].ForeColor = vsMethod; // red-ish
            Indicators[21].Style = IndicatorStyle.Squiggle; // no refs
            Indicators[21].ForeColor = Color.Blue; // red-ish
            Indicators[21].Under = true;

            // Caret Line
            CaretLineVisible = true;
            CaretLineBackColor = Color.FromArgb(0xF3, 0xF9, 0xFF);

            // Line Numbers / Folding Ränder
            Styles[ScintillaNET.Style.LineNumber].BackColor = Color.White;
            SetFoldMarginColor(true, Color.White);
            SetFoldMarginHighlightColor(true, Color.White);
            Markers[Marker.Folder].SetForeColor(Color.Black);
            Markers[Marker.FolderOpen].SetForeColor(Color.Black);
            

            // Farben und Linien für Fold-Verbindungen
            Markers[Marker.FolderSub].SetForeColor(Color.Red);
            Markers[Marker.FolderTail].SetForeColor(Color.Gray);
            Markers[Marker.FolderMidTail].SetForeColor(Color.Gray);
            Markers[Marker.FolderEnd].SetForeColor(Color.Gray);
            Markers[Marker.FolderOpenMid].SetForeColor(Color.Gray);


            //Error Style
            int STYLE_ERROR = 20;
            Styles[STYLE_ERROR].BackColor = Color.Red;
            Styles[STYLE_ERROR].ForeColor = Color.White; // optional

            var foldFore = Color.FromArgb(200, 200, 200);
            var foldBack = Color.Black;
            for (int i = Marker.FolderEnd; i <= Marker.FolderOpenMid; i++)
            {
                Markers[i].SetForeColor(foldFore);
                Markers[i].SetBackColor(foldBack);
            }

            // Autocomplete Farben
            //AutocompleteListBackColor = Color.White;
            //AutocompleteListTextColor = Color.Black;

        }
        public void ApplyDarkTheme()
        {
            
            _currentTheme = EditorTheme.Dark;
            Color _backColor = Color.FromArgb(40, 40, 40);
            _currentTheme = EditorTheme.Dark;
            // Grundstil (Visual Studio Dark / VS Code Dark+)
            StyleResetDefault();
            Styles[ScintillaNET.Style.Default].Font = "Cascadia Code";
            Styles[ScintillaNET.Style.Default].Size = 10;
            Styles[ScintillaNET.Style.Default].BackColor = Active ? Color.FromArgb(25, 25, 25) : Color.FromArgb(80, 25, 25);          // #1E1E1E
            Styles[ScintillaNET.Style.Default].ForeColor = Color.FromArgb(200, 200, 200);          // Standard Text
            StyleClearAll();
            CaretForeColor = Color.White;
            SetSelectionBackColor(true, Color.FromArgb(0x26, 0x4F, 0x78));               // Auswahlblau dunkel
            WhitespaceSize = 2;
            ViewWhitespace = WhitespaceMode.Invisible;

            // Brace Highlight (VS Dark ähnlich)
            Styles[ScintillaNET.Style.BraceLight].ForeColor = Color.FromArgb(0xFF, 0xC0, 0x40);
            Styles[ScintillaNET.Style.BraceLight].BackColor = Color.FromArgb(0x33, 0x33, 0x33);
            Styles[ScintillaNET.Style.BraceBad].ForeColor = Color.White;
            Styles[ScintillaNET.Style.BraceBad].BackColor = Color.FromArgb(0x90, 0x2B, 0x2B);

            // Folding Marker (leicht kontrastreich)
     

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

            Indicators[20].ForeColor = vsDarkMethod; // red-ish
            Indicators[21].Style = IndicatorStyle.Squiggle; // no refs
            Indicators[21].ForeColor = Color.Blue; // red-ish
            Indicators[21].Under = true;

            CaretLineVisible = true;
            CaretLineBackColor = Color.FromArgb(0x2A, 0x2A, 0x2A);


            // Line Numbers & Folding
            Styles[ScintillaNET.Style.LineNumber].BackColor = Color.FromArgb(25, 25, 25); ;
            SetFoldMarginColor(true, Color.FromArgb(25, 25, 25));
            SetFoldMarginHighlightColor(true, Color.FromArgb(25, 25, 25));


            var foldFore = Color.FromArgb(0xAA, 0xAA, 0xAA);
            var foldBack = Color.FromArgb(0x2D, 0x2D, 0x2D);
            for (int i = Marker.FolderEnd; i <= Marker.FolderOpenMid; i++)
            {
                Markers[i].SetForeColor(foldFore);
                Markers[i].SetBackColor(foldBack);
            }

            Indicators[17].Style = IndicatorStyle.StraightBox;
            Indicators[17].ForeColor = Color.FromArgb(0x4E, 0xC9, 0xB0); // matches your vsDarkClass
            Indicators[17].Under = true;

            Indicators[18].Style = IndicatorStyle.StraightBox;
            Indicators[18].ForeColor = Color.FromArgb(0xF4, 0x43, 0x36); // error red
            Indicators[18].Under = true;
        }

   
        public System.Drawing.Font GetFont()
        {
            var style = Styles[ScintillaNET.Style.Default];
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

            FirstVisibleLine = lineNumber;
            // Neue Positionen berechnen
            int startPos = Lines[lineNumber-1].Position;
            int endPos = Lines[lineNumber-1].EndPosition;


            IndicatorCurrent = 16;
            Indicators[16].ForeColor = color;
            IndicatorFillRange(startPos, endPos - startPos);

            // Position merken
            lastHighlightedStart = startPos;
            lastHighlightedLength = endPos - startPos;
            GotoPosition(startPos);
           
         
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

            var miRef = new ToolStripMenuItem("Show References");
            miRef.Click += async (_, __) => await ReferencesList.ShowAsync();
            EditorContextMenu.Items.Add(miRef);

            var miGoto = new ToolStripMenuItem("Go To Definition");
            miGoto.Click += async (_, __) => await Core.Explorer.GoToDefinition();
            EditorContextMenu.Items.Add(miGoto);

            var miRename = new ToolStripMenuItem("Rename...");
            miRename.Click += async (_, __) => await Core.Explorer.RenameSymbol();
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

        public async Task ApplyMethodReferenceHighlightsAsync()
        {
            try
            {
                // Clear previous method highlights (both indicators)
                IndicatorClearRange(20, TextLength);
                IndicatorClearRange(21, TextLength);

                var doc = Target?.Document;
                if (doc == null) return;

                var root = await doc.GetSyntaxRootAsync().ConfigureAwait(false);
                if (root == null) return;

                // Get the full reference table (not the filtered 'References')
                var all = await RosylnSemantic.FindAllMethodReferencesAsync(doc);

                // Build counts by type+method
                var countsByName = new Dictionary<(string type, string method), int>();
                foreach (DataRow row in all.Rows)
                {
                    var type = row["ContainingType"]?.ToString() ?? "";
                    var method = row["MethodName"]?.ToString() ?? "";
                    var key = (type, method);
                    countsByName[key] = countsByName.TryGetValue(key, out var n) ? n + 1 : 1;
                }

                // Paint identifier spans
                var methodDecls = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
                foreach (var md in methodDecls)
                {
                    var typeName = (md.Parent as ClassDeclarationSyntax)?.Identifier.Text ?? "";
                    var methodName = md.Identifier.Text;
                    var key = (typeName, methodName);
                    var count = countsByName.TryGetValue(key, out var n) ? n : 0;

                    var span = md.Identifier.Span;
                    IndicatorCurrent = count > 0 ? 20 : 21;
                    IndicatorFillRange(span.Start, span.Length);
                }
            }
            catch (Exception ex) 
            { 
            
            }
        }


        #region Roslyn Integration




        bool init = true;
        public void UpdateDocument()
        {
            try
            {
                Target.Code = Text;
                Target.UpdateCode();
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("UpdateDocument failed: " + ex.Message);
            }
        }

        public async Task UpdateRoslyn(string sender)
        {
            using (this.SuspendUpdates())
            {
                if (Target == null) return;
                if (Active)
                {
                    if (init)
                    {
                        await FormatDocumentAsync();
                        init = false;
                    }

                    UpdateDocument();
                    LockNecessary();

                    Output.Rows.Clear();
                    var newData = await RoslynDiagnostic.ApplyAsync(this);
                    foreach (DataRow row in newData.Rows)
                        Output.ImportRow(row);

                    await RosylnSemantic.ApplyAsync(this, Target.Document);
                    await UpdateMethodesFromRoslynAsync();

                    // Update reference cache for highlighting
                  //  await UpdateReferences();
                    References = await RosylnSemantic.FindAllMethodReferencesAsync(Target.Document);
                    await ApplyMethodReferenceHighlightsAsync();
                }
                LockNecessary();
            }
        }


        public async Task<List<CompletionItem>> GetReferencesItemsAsync()
        {
            List<CompletionItem> items = new List<CompletionItem>();

            DataTable filtered = References.Copy();

            var root = await Target.Document.GetSyntaxRootAsync().ConfigureAwait(false);
            if (root == null) return items;
            

            int caretPos = CurrentPosition;
            var token = root.FindToken(Math.Max(0, Math.Min(caretPos, TextLength)));
            var node = token.Parent;

            Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax? methodDecl = null;
            for (var n = node; n != null; n = n.Parent)
            {
                if (n is Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax m)
                {
                    methodDecl = m;
                    break;
                }
            }

            if (methodDecl == null) return items;
            

            // Names for filtering
            var classDecl = methodDecl.Parent as Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax;
            string methodName = methodDecl.Identifier.Text;
            string? className = classDecl?.Identifier.Text;

            filtered.Rows.Clear();

            foreach (DataRow row in References.Rows)
            {
                var rowMethodName = row["MethodName"]?.ToString() ?? string.Empty;
                var rowContainingType = row["ContainingType"]?.ToString() ?? string.Empty;

                if (string.Equals(rowMethodName, methodName, StringComparison.Ordinal) &&
                    (string.IsNullOrEmpty(className) || string.Equals(rowContainingType, className, StringComparison.Ordinal)))
                {
                    Debug.WriteLine(rowMethodName);
                    Debug.WriteLine($"{row["Document"].ToString().Replace(".cs", "")}.{row["Line"]}: {row["Code"].ToString().Trim()}");
                    filtered.ImportRow(row);
                }
            }

         //   Debug.WriteLine("items: " + filtered.Rows.Count);

            if (filtered.Rows.Count == 0) return items;

            foreach (DataRow row in filtered.Rows)
            {
                items.Add(new CompletionItem
                {
                    Text = $"{row["Document"].ToString().Replace(".cs", "")}.{row["Line"]}: {row["Code"].ToString().Trim()}",
                    Value = $"{row["Document"]}|{row["Line"]}"
                });
            }

            return items;



        }






        #endregion


        #region Custom Events

        public void LockNecessary()
        {
            ResetHideProteced();
            var lines = Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            int startLine = -1;
            int endLine = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                if (startLine == -1 && lines[i].Contains("//<CodeStart>"))
                    startLine = i;

                if (startLine != -1 && lines[i].Contains("//<CodeEnd>"))
                {
                    endLine = i;
                    break;
                }
            }

            if (startLine != -1 && endLine != -1)
            {
                // Zeilen außerhalb des SubCode-Bereichs ausblenden
                HideProtectLines(0, startLine); // Zeilen vor SubCode
                HideProtectLines(endLine, Lines.Count - 1); // Zeilen nach SubCode
                //Debug.WriteLine("StartLine " + startLine);
                //Debug.WriteLine("EndLine " + endLine + ".." + Lines.Count);
            }
        }
        public async Task UpdateMethodesFromRoslynAsync()
        {
            MethodesClasses.Clear();

            var root = await Target.Document.GetSyntaxRootAsync();
            if (root == null) return;

            // Alle Klassen im Dokument finden
            var classNodes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classNode in classNodes)
            {
                string className = classNode.Identifier.Text;

                // Klasse eintragen
                DataRow classRow = MethodesClasses.NewRow();
                classRow["Row"] = classNode.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                classRow["Name"] = "[C] " + className;
                MethodesClasses.Rows.Add(classRow);

                // Alle Methoden in der Klasse finden
                var methodNodes = classNode.DescendantNodes().OfType<MethodDeclarationSyntax>();

                foreach (var method in methodNodes)
                {
                    string methodName = method.Identifier.Text;
                    string fullName = $"{className}.{methodName}";

                    DataRow methodRow = MethodesClasses.NewRow();
                    methodRow["Row"] = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    methodRow["Name"] = "[M] " + fullName;
                    MethodesClasses.Rows.Add(methodRow);
                }
            }
        }

        #endregion

        #region Create Summary

        private async void AutoInsertSummary_CharAdded(object? sender, CharAddedEventArgs e)
        {
            if (e.Char != '/') return;

            int pos = CurrentPosition;
            int line = LineFromPosition(pos);
            var lineText = Lines[line].Text.TrimEnd('\r', '\n');

            if (!lineText.Trim().Equals("///")) return;

            // Finde die nächste sinnvolle Zeile (nicht leer)
            int nextLine = line + 1;
            while (nextLine < Lines.Count && string.IsNullOrWhiteSpace(Lines[nextLine].Text))
                nextLine++;

            if (nextLine >= Lines.Count) return;

            int methodLineStart = Lines[nextLine].Position;
            int methodLineEnd = Lines[nextLine].EndPosition;
            string methodLineText = Lines[nextLine].Text.Trim();

            // Mit Roslyn prüfen, ob es wirklich eine Methode ist
            var doc = Target?.Document;
            if (doc == null) return;
            var root = await doc.GetSyntaxRootAsync().ConfigureAwait(false);
            if (root == null) return;

            // Ermittle die Methode an der Position
            var methodNode = root.DescendantNodes()
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
                .FirstOrDefault(m =>
                {
                    var span = m.GetLocation().GetLineSpan();
                    return span.StartLinePosition.Line == nextLine;
                });

            if (methodNode == null) return;

            // XML-Stub generieren
            string indent = new string(' ', Lines[line].Indentation);
            var sb = new StringBuilder();
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// ");
            sb.AppendLine($"{indent}/// </summary>");

            foreach (var param in methodNode.ParameterList.Parameters)
            {
                sb.AppendLine($"{indent}/// <param name=\"{param.Identifier.Text}\"></param>");
            }

            if (methodNode.ReturnType is not Microsoft.CodeAnalysis.CSharp.Syntax.PredefinedTypeSyntax predef ||
                predef.Keyword.Text != "void")
            {
                sb.AppendLine($"{indent}/// <returns></returns>");
            }

            string summaryStub = sb.ToString().TrimEnd('\r', '\n');

            // Ersetze die aktuelle Zeile durch den Stub
            BeginUndoAction();
            try
            {
                int lineStart = Lines[line].Position;
                int lineEnd = Lines[line].EndPosition;
                SelectionStart = lineStart;
                SelectionEnd = lineEnd;
                ReplaceSelection(summaryStub + "\n");
                // Cursor in die Mitte der summary setzen
                int summaryPos = lineStart + summaryStub.IndexOf($"{indent}/// ") + ($"{indent}/// ").Length;
                GotoPosition(summaryPos);
            }
            finally
            {
                EndUndoAction();
            }
        }

        #endregion


        #region Folding Helpers

        public void InitFolding()
        {
         //   ScintillaLexers.CreateLexer(this,LexerEnumerations.LexerType.Cs);

            LexerName = "cpp";

            SetProperty("fold", "1");
            SetProperty("fold.compact", "0");         // 0 = leere Zeilen *nicht* mit einklappen
            SetProperty("fold.preprocessor", "1");    // für #region etc.
            SetProperty("fold.braces", "1");          // Folding für Blöcke
            SetProperty("fold.at.else", "1");         // separates Folding für else
            SetProperty("fold.cpp.syntax.based", "1");
            SetProperty("fold.cpp.comment.explicit", "0"); // alle Kommentarblöcke foldbar


            // Folding‑Margin (Pfeile wie in Visual Studio)
            const int FOLD_MARGIN = 2;
            Margins[FOLD_MARGIN].Type = MarginType.Symbol;
            Margins[FOLD_MARGIN].Mask = Marker.MaskFolders;
            Margins[FOLD_MARGIN].Sensitive = true;
            Margins[FOLD_MARGIN].Width = 18;

            Markers[Marker.Folder].Symbol = MarkerSymbol.Arrow;
            Markers[Marker.FolderOpen].Symbol = MarkerSymbol.ArrowDown;
            Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;
            Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            Markers[Marker.FolderEnd].Symbol = MarkerSymbol.Arrow;
            Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.ArrowDown;

            AutomaticFold = AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change;
            SetFoldFlags(FoldFlags.LineAfterContracted);

        }

        #endregion

        public void InitReferenceCountMargin()
        {
            const int REFCOUNT_MARGIN = 3; // pick a free margin index
            Margins[REFCOUNT_MARGIN].Type = MarginType.Text;
            Margins[REFCOUNT_MARGIN].Sensitive = false;
            Margins[REFCOUNT_MARGIN].Width = 40; // adjust for expected digits
        }

        public async Task RenderReferenceCountsInMarginAsync()
        {
            const int REFCOUNT_MARGIN = 3;
            // Clear previous texts
            for (int i = 0; i < Lines.Count; i++)
                Lines[i].MarginText = string.Empty;

            var doc = Target?.Document;
            if (doc == null) return;

            var root = await doc.GetSyntaxRootAsync().ConfigureAwait(false);
            if (root == null) return;

            var methodDecls = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (var md in methodDecls)
            {
                var typeName = (md.Parent as ClassDeclarationSyntax)?.Identifier.Text ?? "";
                var methodName = md.Identifier.Text;
                var key = (typeName, methodName);
                var count = 0;
                if (References.Rows.Count > 0)
                {
                    // same aggregation as above
                    count = References.AsEnumerable().Count(r =>
                        string.Equals(r["ContainingType"]?.ToString() ?? "", typeName, StringComparison.Ordinal) &&
                        string.Equals(r["MethodName"]?.ToString() ?? "", methodName, StringComparison.Ordinal));
                }

                var line = md.GetLocation().GetLineSpan().StartLinePosition.Line; // 0-based
                if (line >= 0 && line < Lines.Count)
                {
                    Lines[line].MarginText = count > 0 ? count.ToString() : string.Empty;
                }
            }
        }

        private async void DocumentEditor_DwellStart(object? sender, DwellEventArgs e)
        {
            if (e.Position < 0) return;

            var doc = Target?.Document;
            if (doc == null) return;

            var root = await doc.GetSyntaxRootAsync().ConfigureAwait(false);
            if (root == null) return;

            var model = await doc.GetSemanticModelAsync().ConfigureAwait(false);
            if (model == null) return;

            var token = root.FindToken(Math.Clamp(e.Position, 0, TextLength));
            var node = token.Parent;

            // Prüfe, ob es eine Methodendeklaration ist
            if (node is MethodDeclarationSyntax md)
            {
                // Zeige Referenzanzahl wie bisher
                string typeName = (md.Parent as ClassDeclarationSyntax)?.Identifier.Text ?? "";
                string methodName = md.Identifier.Text;

                int refCount = References.AsEnumerable().Count(r =>
                    string.Equals(r["ContainingType"]?.ToString() ?? "", typeName, StringComparison.Ordinal) &&
                    string.Equals(r["MethodName"]?.ToString() ?? "", methodName, StringComparison.Ordinal));

                string summary = refCount == 1 ? "1 Reference" : $"{refCount} References";
                CallTipShow(e.Position, summary);
                return;
            }

            // Prüfe, ob es ein Methodenaufruf ist
            // Suche nach dem nächsten aufsteigenden InvocationExpressionSyntax-Knoten
            var invocation = node.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            if (invocation != null)
            {
                var symbolInfo = model.GetSymbolInfo(invocation);
                var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
                if (methodSymbol != null)
                {
                    var fqName = methodSymbol.ToDisplayString();
                    var summary = qbookCode.Roslyn.RoslynSummarys.GetSummary(fqName);
                    if (!string.IsNullOrWhiteSpace(summary))
                    {
                        CallTipShow(e.Position, summary);
                        return;
                    }
                }
            }

            // Fallback: Kein Tooltip
            CallTipCancel();

        }

        private void DocumentEditor_DwellEnd(object? sender, DwellEventArgs e)
        {
            CallTipCancel();
        }

        // Optional: show count when caret moves onto a method identifier
        private async void DocumentEditor_UpdateUI(object? sender, UpdateUIEventArgs e)
        {
            if ((e.Change & UpdateChange.Selection) == 0) return;

            int pos = CurrentPosition;
            var doc = Target?.Document;
            if (doc == null) return;

            var root = await doc.GetSyntaxRootAsync().ConfigureAwait(false);
            if (root == null) return;

           
            var token = root.FindToken(Math.Clamp(pos, 0, TextLength));
            if (token.Parent is not MethodDeclarationSyntax md)
            {
                CallTipCancel();
                return;
            }

            string typeName = (md.Parent as ClassDeclarationSyntax)?.Identifier.Text ?? "";
            string methodName = md.Identifier.Text;


            int refCount = References.AsEnumerable().Count(r =>
                string.Equals(r["ContainingType"]?.ToString() ?? "", typeName, StringComparison.Ordinal) &&
                string.Equals(r["MethodName"]?.ToString() ?? "", methodName, StringComparison.Ordinal));

            string tip = refCount == 1 ? "1 Reference" : $"{refCount} References";
            CallTipShow(pos, tip);
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

    public static class ScintillaUpdateScopes
    {
        private const int WM_SETREDRAW = 0x000B; // Windows Message: Set Redraw on/off

        // Scintilla messages (SCI_*):
        private const int SCI_BEGINUNDOACTION = 2078;
        private const int SCI_ENDUNDOACTION = 2079;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        ///  Friert Repaint ein und fasst Änderungen optional zu einer Undo-Aktion zusammen.
        ///  Verwendung immer mit using: using (scintilla.SuspendUpdates()) { /* viele Änderungen */ }
        /// </summary>
        public static IDisposable SuspendUpdates(this Scintilla scintilla, bool coalesceUndo = true)
        {
            if (scintilla is null) throw new ArgumentNullException(nameof(scintilla));

            // Repaint aus
            SendMessage(scintilla.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);

            if (coalesceUndo)
                scintilla.DirectMessage(SCI_BEGINUNDOACTION, IntPtr.Zero, IntPtr.Zero);

            return new RestoreScope(scintilla, coalesceUndo);
        }

        private sealed class RestoreScope : IDisposable
        {
            private readonly Scintilla _scintilla;
            private readonly bool _coalesceUndo;
            private bool _disposed;

            public RestoreScope(Scintilla scintilla, bool coalesceUndo)
            {
                _scintilla = scintilla;
                _coalesceUndo = coalesceUndo;
            }

            public void Dispose()
            {
                if (_disposed) return;

                if (_coalesceUndo)
                    _scintilla.DirectMessage(SCI_ENDUNDOACTION, IntPtr.Zero, IntPtr.Zero);

                // Repaint an + Refresh erzwingen
                SendMessage(_scintilla.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
                _scintilla.Refresh();

                _disposed = true;
            }
        }




    }





}
