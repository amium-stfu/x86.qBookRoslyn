using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Scripting.Utils;
using qbook.CodeEditor.AutoComplete;
using qbook.CodeEditor.SignatureHelp;
using qbook.UI;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization; // force English diagnostics
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions; // add near other using directives
using System.Threading.Tasks;
using System.Windows.Forms;
using qbook.ScintillaEditor;

using static qbook.Core;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using AccessibilityCode = Microsoft.CodeAnalysis.Accessibility;
using RoslynDocument = Microsoft.CodeAnalysis.Document; // Alias gegen Kollision mit ScintillaNET.Document

namespace qbook.CodeEditor
{
    public partial class FormEditor : Form
    {

        // Helper to satisfy any legacy references that still call FindDocument from the form level
        private RoslynDocument? FindDocument(string? file) => file == null ? null : _roslyn.GetDocument(file);

        private string? _currentFile;
        private string? _projectRoot;
        private RoslynServices _roslyn = new RoslynServices();
        private System.Windows.Forms.Timer _debounceTimer = new System.Windows.Forms.Timer();

        internal enum EditorTheme { Light, Dark }
        private EditorTheme _currentTheme = EditorTheme.Light;

        public bool DarkTheme => _currentTheme == EditorTheme.Dark;

        // Fehler-Highlighting / Tooltips
        private List<(int start, int length, string message)> _errorSpans = new();
        private readonly List<Diagnostic> _currentDiagnostics = new();

        private bool _awaitingCtrlK; // für Tastatur-Akkord (Ctrl+K, Ctrl+C/U)
        private bool _awaitingCtrlM; // für Tastatur-Akkord (Ctrl+M, Ctrl+O/L)
        private readonly System.Windows.Forms.Timer _chordTimer = new() { Interval = 1500 };
        private readonly System.Windows.Forms.Timer _undoTimer = new() { Interval = 1000 };

        private int _lastSearchPos = -1;
        private string? _projectSearchPatternCache;
        private string[]? _projectSearchFileCache;
        private int _projectSearchCurrentFileIndex = -1;
        private Point _editorTopRightCache;

        // --- neue Felder für Replace-Projekt-Suche ---
        private string? _replaceProjectPatternCache;
        private string[]? _replaceProjectFileCache;
        private int _replaceProjectCurrentFileIndex = -1;
        private int _replaceProjectLastPos = -1;



        DataTable tblEditorOutputs = new DataTable();
        DataTable tblBuildOutputs = new DataTable();
        DataTable tblFindReplaceOutputs = new DataTable();

        DataTable tblMethodes = new DataTable();

        DataGridView GridViewDiagnosticOutput;
        DataGridView dataGridViewBuildOutput;


        int lastHighlightedStart = -1;
        int lastHighlightedLength = 0;

        EditorNode RootNode;

        void HighlightLine(int lineNumber, System.Drawing.Color color)
        {
            // Vorherige Markierung löschen
            Editor.IndicatorClearRange(16, Editor.TextLength);

            Editor.FirstVisibleLine = lineNumber - 17;
            // Neue Positionen berechnen
            int startPos = Editor.Lines[lineNumber].Position;
            int endPos = Editor.Lines[lineNumber].EndPosition;

            // Markierung setzen
            Editor.IndicatorCurrent = 16;
            Editor.Indicators[16].ForeColor = color;
            Editor.IndicatorFillRange(startPos, endPos - startPos);

            // Position merken
            lastHighlightedStart = startPos;
            lastHighlightedLength = endPos - startPos;
        }
        void HighlightText(int startPos, int endPos, System.Drawing.Color color)
        {

            Debug.WriteLine("Highlight " + startPos + ".." + endPos);
            // Vorherige Markierung löschen
            Editor.IndicatorClearRange(16, Editor.TextLength);

            // Markierung setzen
            Editor.IndicatorCurrent = 16;
            Editor.Indicators[16].ForeColor = color;
            Editor.GotoPosition(startPos);
            Editor.IndicatorFillRange(startPos, endPos - startPos);

            // Position merken
            lastHighlightedStart = startPos;
            lastHighlightedLength = endPos - startPos;
        }


        private AutoCompleteDropDown _acDropDown;
        private SignatureHelper _signatureHelper;
      //  private FoldingControl _foldingControl;
        private Diagnostic _diagnostic;

        public FormEditor()
        {
            InitializeComponent();
         

          //  TablePanelOutputs.Controls.Add(this.dataGridViewEditorOutput, 1, 2);

            tblMethodes.Columns.Add("Row", typeof(int));
            tblMethodes.Columns.Add("Name", typeof(string));
            gridViewMethodes.DataSource = tblMethodes;
            gridViewMethodes.AllowUserToResizeColumns = false;
            gridViewMethodes.AllowUserToAddRows = false;
            gridViewMethodes.RowHeadersVisible = false;
            gridViewMethodes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridViewMethodes.MultiSelect = false;
            gridViewMethodes.ReadOnly = true;
            gridViewMethodes.BackgroundColor = Color.White;
            gridViewMethodes.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            gridViewMethodes.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            gridViewMethodes.ColumnHeadersVisible = false;
            gridViewMethodes.RowHeadersVisible = false;
            gridViewMethodes.Dock = DockStyle.Fill;
            gridViewMethodes.AllowUserToAddRows = false;
            gridViewMethodes.AllowUserToDeleteRows = false;
            gridViewMethodes.AllowUserToOrderColumns = true;
            gridViewMethodes.AllowUserToResizeColumns = false;
            gridViewMethodes.BackgroundColor = System.Drawing.Color.LightGray;
            gridViewMethodes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            gridViewMethodes.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            gridViewMethodes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridViewMethodes.ColumnHeadersVisible = false;
            gridViewMethodes.Dock = System.Windows.Forms.DockStyle.Fill;
            gridViewMethodes.Location = new System.Drawing.Point(46, 25);
            gridViewMethodes.TabIndex = 0;

            gridViewMethodes.DataBindingComplete += (s, e) =>
            {
                gridViewMethodes.Columns["Row"].Width = 40;
                gridViewMethodes.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            };

            gridViewMethodes.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    int line = (int)gridViewMethodes.Rows[e.RowIndex].Cells["Row"].Value - 1;
                    HighlightLine(line, Color.Yellow);
                }
            };

            Editor.MouseDown += (s, e) =>
            {
                int currentPos = Editor.CurrentPosition;

                // Prüfen, ob Cursor außerhalb der markierten Zeile liegt
                if (lastHighlightedStart >= 0 &&
                    (currentPos < lastHighlightedStart || currentPos > lastHighlightedStart + lastHighlightedLength))
                {
                    Editor.IndicatorClearRange(16, Editor.TextLength);
                    lastHighlightedStart = -1;
                    lastHighlightedLength = 0;
                }
            };

            vBarEditor.Init(Editor);
            hBarEditor.Init(Editor);
            vBarMethodes.Init(gridViewMethodes);
            vBarProjectTree.Init(ProjectTree, hideNativeScrollbar: true);



            //

            //for deaktivate embedded undo
            Editor.ClearCmdKey(Keys.Control | Keys.Z);
            Editor.ClearCmdKey(Keys.Control | Keys.Y);
            const int SCI_SETUNDOCOLLECTION = 2012;
            try { Editor.DirectMessage(SCI_SETUNDOCOLLECTION, new IntPtr(0), IntPtr.Zero); } catch { /* ignorieren, wenn nicht nötig */ }


            // Grund-Einstellungen verschoben in Theme Methoden

            const int MARGIN_LINE_NUMBERS = 0;
            Editor.Margins[MARGIN_LINE_NUMBERS].Type = MarginType.Number;
            Editor.Margins[MARGIN_LINE_NUMBERS].Width = 40;

            Editor.WhitespaceSize = 2;
            Editor.ViewWhitespace = WhitespaceMode.Invisible;
            Editor.IndentationGuides = IndentView.LookBoth;

            //Highlighting

            // 0 = Errors (hast du)

            Editor.Indicators[0].Style = IndicatorStyle.Squiggle;
            Editor.Indicators[0].ForeColor = Color.Red;
            Editor.Indicators[0].Under = true;

            // 2..9 = Semantik-Overlay (Textfarbe oder Unterstreichung)
            Editor.Indicators[2].Style = IndicatorStyle.TextFore;   // Methoden
            Editor.Indicators[2].Under = true;
            Editor.Indicators[2].ForeColor = Color.DarkOrange;

            Editor.Indicators[3].Style = IndicatorStyle.TextFore;    // Klassen
            Editor.Indicators[3].ForeColor = Color.DarkViolet;

            Editor.Indicators[4].Style = IndicatorStyle.TextFore;    // Interfaces
            Editor.Indicators[4].ForeColor = Color.Red;

            Editor.Indicators[5].Style = IndicatorStyle.TextFore;    // Structs
            Editor.Indicators[5].ForeColor = Color.Firebrick;

            Editor.Indicators[6].Style = IndicatorStyle.TextFore;    // Enums
            Editor.Indicators[6].ForeColor = Color.FromArgb(0xCE, 0x91, 0x78);

            Editor.Indicators[7].Style = IndicatorStyle.TextFore;    // Delegates
            Editor.Indicators[7].ForeColor = Color.FromArgb(0xD4, 0xD4, 0xD4);


            Editor.Indicators[8].Style = IndicatorStyle.TextFore;    // Properties
            Editor.Indicators[8].ForeColor = Color.Black;

            Editor.Indicators[9].Style = IndicatorStyle.TextFore;    // Fields
            Editor.Indicators[9].ForeColor = Color.Black;

            // 10..12 zusätzliche Syntax (Keywords, Zahlen, Strings/Chars)
            Editor.Indicators[10].Style = IndicatorStyle.TextFore; // Keywords
            Editor.Indicators[10].ForeColor = Color.FromArgb(0x00, 0x33, 0xCC);
            Editor.Indicators[11].Style = IndicatorStyle.TextFore; // Numbers
            Editor.Indicators[11].ForeColor = Color.FromArgb(0x00, 0x80, 0x80);
            Editor.Indicators[12].Style = IndicatorStyle.TextFore; // Strings / Chars
            Editor.Indicators[12].ForeColor = Color.FromArgb(0xA3, 0x15, 0x15);

            Editor.Indicators[13].Style = IndicatorStyle.TextFore; // Kommentare
            Editor.Indicators[13].ForeColor = Color.FromArgb(0x00, 0x80, 0x00);

            Editor.Indicators[14].Style = IndicatorStyle.TextFore; // If, Else, Switch,..
            Editor.Indicators[14].ForeColor = Color.FromArgb(0x00, 0x80, 0x00);

            Editor.Indicators[15].Style = IndicatorStyle.TextFore; // If, Else, Switch,..
            Editor.Indicators[15].ForeColor = Color.FromArgb(0x00, 0x80, 0x00);

            Editor.Indicators[16].Style = IndicatorStyle.StraightBox; //FInd Code
            Editor.Indicators[16].ForeColor = Color.Yellow;


            Editor.Indicators[1].Style = IndicatorStyle.StraightBox;
            Editor.Indicators[1].Under = true;
            Editor.Indicators[1].ForeColor = Color.Red;

            Editor.UpdateUI += (s, e) =>
            {
                var caret = Editor.CurrentPosition;
                var bracePos1 = -1;
                if (caret > 0 && IsBrace(Editor.GetCharAt(caret - 1))) bracePos1 = caret - 1;
                else if (IsBrace(Editor.GetCharAt(caret))) bracePos1 = caret;
                var bracePos2 = bracePos1 >= 0 ? Editor.BraceMatch(bracePos1) : -1;
                Editor.BraceHighlight(bracePos1, bracePos2);
                if ((e.Change & UpdateChange.Selection) != 0 && Editor.CallTipActive)
                {
                    _ = _signatureHelper.TrySignatureHelpAsync(updateOnly: true);
                }
            };
            Editor.LexerName = "cpp";
            Editor.SetKeywords(0, CsKeywords);
            Editor.UseTabs = true;
            Editor.TabWidth = 4;
            Editor.IndentWidth = 4;
            Editor.WrapMode = WrapMode.None;


            KeyPreview = true;

            // Entferne alte/defekte Handler (falls vorhanden)

            Editor.CharAdded += async (s, e) =>
            {

                char c = (char)e.Char;
                _debounceTimer.Stop(); _debounceTimer.Start();

                return;


                if (c == '\n')
                {
                    try
                    {
                        int curLine = Editor.LineFromPosition(Editor.CurrentPosition);
                        if (curLine > 0)
                        {
                            var prev = Editor.Lines[curLine - 1];
                            string prevText = prev.Text.TrimEnd('\r', '\n');
                            int indent = prev.Indentation;
                            if (prevText.TrimEnd().EndsWith("{"))
                                indent += Editor.IndentWidth;
                            Editor.Lines[curLine].Indentation = indent;
                            int target = Editor.Lines[curLine].Position + Editor.Lines[curLine].Indentation;
                            Editor.GotoPosition(target);
                        }
                    }
                    catch { }
                    return;
                }

                // Smart Outdent bei '}'
                if (c == '}')
                {
                    try
                    {
                        int bracePos = Editor.CurrentPosition - 1;
                        int match = Editor.BraceMatch(bracePos);
                        if (match < 0)
                        {
                            int depth = 0;
                            for (int i = bracePos - 1; i >= 0; i--)
                            {
                                int ch = Editor.GetCharAt(i);
                                if (ch == '}') depth++;
                                else if (ch == '{')
                                {
                                    if (depth == 0) { match = i; break; }
                                    depth--;
                                }
                            }
                        }
                        int currentLine = Editor.LineFromPosition(bracePos);
                        int targetIndent = 0;
                        if (match >= 0)
                        {
                            int openLine = Editor.LineFromPosition(match);
                            targetIndent = Editor.Lines[openLine].Indentation;
                        }
                        else
                        {
                            for (int l = currentLine - 1; l >= 0; l--)
                            {
                                string raw = Editor.Lines[l].Text.TrimEnd('\r', '\n');
                                if (raw.Trim().Length == 0) continue;
                                if (raw.TrimEnd().EndsWith("{"))
                                    targetIndent = Editor.Lines[l].Indentation;
                                else
                                    targetIndent = Math.Max(0, Editor.Lines[l].Indentation - Editor.IndentWidth);
                                break;
                            }
                        }
                        Editor.Lines[currentLine].Indentation = targetIndent;
                        Editor.GotoPosition(bracePos + 1);
                    }
                    catch { }
                }


            };

            _debounceTimer.Interval = 100;
            _debounceTimer.Tick += async (s, e) =>
            {
                if (SelectedNode.Active)
                {
                    RoslynFoldingHelper.SaveFolding(Editor);
                    await RefreshEditor("DebounceTimer");
                }

                SelectedNode.Code = Editor.Text;
            };
            Editor.TextChanged += (s, e) =>
            {
                if (refreshing) return;
                //   Debug.WriteLine("."); 
                _debounceTimer.Stop();
                _debounceTimer.Start();
            };


            // Fehler-Tooltip (CallTip) via Dwell
            Editor.MouseDwellTime = 500; // ms
            Editor.DwellStart += (s, e) =>
            {
                if (e.Position < 0) return;
                // Suche erstes Matching
                var hit = _errorSpans.FirstOrDefault(span => e.Position >= span.start && e.Position < span.start + span.length);
                if (hit.length > 0)
                {
                    if (!Editor.CallTipActive) Editor.CallTipShow(e.Position, hit.message);
                }
            };
            Editor.DwellEnd += (s, e) =>
            {
                if (Editor.CallTipActive) Editor.CallTipCancel();
            };

            InitEditorContextMenu();
            _chordTimer.Tick += (s, e) => { _awaitingCtrlK = false; _chordTimer.Stop(); };
            _chordTimer.Tick += (s, e) => { _awaitingCtrlM = false; _chordTimer.Stop(); };
            _undoTimer.Tick += (s, e) =>
            {
                SelectedNode.AddUndo(Editor.Text, Editor.CurrentPosition, Editor.FirstVisibleLine);
                _undoTimer.Stop();
            };



            //Undo Control
            Editor.KeyPress += (s, e) =>
            {
                if (SelectedNode == null) return;

                // CTRL+Z = ASCII 26
                if (e.KeyChar == (char)26)
                {
                    e.Handled = true; // verhindert weitere Verarbeitung
                    return;
                }

                // Nur bei echten Zeichen starten
                if (!char.IsControl(e.KeyChar))
                {
                    _undoTimer.Stop();
                    _undoTimer.Start();
                }
            };

            Editor.KeyUp += (s, e) =>
            {


                if (e.Control && e.KeyCode == Keys.V)
                {
                    SelectedNode.AddUndo(Editor.Text, Editor.CurrentPosition, Editor.FirstVisibleLine);
                }

            };
            KeyDown += (s, e) =>
            {
                BeginnUpdate();
                RoslynFoldingHelper.SaveFolding(Editor);
                if (e.KeyCode == Keys.Enter ||
                    e.KeyCode == Keys.OemPeriod ||
                    e.KeyCode == Keys.OemSemicolon ||
                    e.KeyCode == Keys.OemCloseBrackets ||
                    e.KeyCode == Keys.Delete)
                {
                    SelectedNode.AddUndo(Editor.Text, Editor.CurrentPosition, Editor.FirstVisibleLine);
                    _undoTimer.Stop();
                    return;
                }
                

                if (e.KeyCode == Keys.Back)
                {
                    _undoTimer.Stop();
                    _undoTimer.Start();
                    _debounceTimer.Start();

                }
                if (e.KeyCode == Keys.Tab) _debounceTimer.Start();

                if (e.Control && e.KeyCode == Keys.Z)
                {
                    if (SelectedNode != null)
                    {

                        if (!SelectedNode.hasUndo) return;
                        //Beginn Update Editor
                        SendMessage(Editor.Handle, WM_SETREDRAW, false, 0);

                        Undo undo = SelectedNode.GetUndo();
                        if (undo == null) return;
                        Editor.Text = undo.Text;
                        Editor.FirstVisibleLine = undo.FirstLine;
                        Editor.GotoPosition(undo.Position);
                        //Beginn Update Editor

                        SendMessage(Editor.Handle, WM_SETREDRAW, true, 0);
                    }
                }

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
                    return;
                }
                if (_awaitingCtrlK && e.Control && e.KeyCode == Keys.U)
                {
                    e.SuppressKeyPress = true;
                    _awaitingCtrlK = false; _chordTimer.Stop();
                    UncommentSelection();
                    return;
                }

            };


            //Diagnostics
            _diagnostic = new Diagnostic(this, Editor, _roslyn);

            //AutoCompleteInitialize
            _acDropDown = new AutoCompleteDropDown(this, _roslyn);

            //Initialize SignatureHelper
            _signatureHelper = new SignatureHelper(this, _roslyn);


            //InitOutputTables
            InitOutput();

            //Initialize Folding Control
         //   _foldingControl = new FoldingControl(this);

            RoslynFoldingHelper.InitializeFolding(Editor);


            //KeyDown += (s, e) =>
            //{
            //    if (e.Control && e.KeyCode == Keys.M)
            //    {
            //        _awaitingCtrlM = true;
            //        _chordTimer.Stop();
            //        _chordTimer.Start();
            //        e.SuppressKeyPress = true;
            //        return;
            //    }
            //    if (_awaitingCtrlM && e.Control && e.KeyCode == Keys.O)
            //    {
            //        e.SuppressKeyPress = true;
            //        _awaitingCtrlM = false; _chordTimer.Stop();
            //        _foldingControl.CollapseAllInnerBlocksExceptOuter();
            //        return;
            //    }
            //    if (_awaitingCtrlM && e.Control && e.KeyCode == Keys.L)
            //    {
            //        e.SuppressKeyPress = true;
            //        _awaitingCtrlM = false; _chordTimer.Stop();
            //        _foldingControl.ExpandAllBlocksAtCaret();
            //        return;
            //    }

            //};


            KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.H)
                {
                    e.SuppressKeyPress = true;
                    btnFindReplace.PerformClick();

                }
            };

            KeyDown += async (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.F)
                {
                    e.SuppressKeyPress = true;
                    btnFind.PerformClick();

                }
            };


            //Init Icons
            foreach (System.Windows.Forms.Button b in panelFunctions.Controls)
            {

                Bitmap pic = b.Image as Bitmap;
                pic = BitmapTools.ReplaceColor(pic, Color.Black, Color.FromArgb(120, 120, 120), 100); //Init Icon Color
                pic = BitmapTools.ReplaceColor(pic, ButtonForeColor, Color.FromArgb(120, 120, 120));
                var old = b.Image;
                b.Image = pic;
                old?.Dispose();
            }

            ApplyLightTheme();


        }

        ContextMenuStrip EditorContextMenu = new ContextMenuStrip();
        private void InitEditorContextMenu()
        {


            var miGoto = new ToolStripMenuItem("Go To Definition (F12)");
            miGoto.Click += async (_, __) => await GoToDefinitionAsync();
            EditorContextMenu.Items.Add(miGoto);

            var miRename = new ToolStripMenuItem("Rename...");
            miRename.Click += async (_, __) => await RenameSymbolAsync();
            EditorContextMenu.Items.Add(miRename);

            EditorContextMenu.Items.Add(new ToolStripSeparator());

            var miCollapse = new ToolStripMenuItem("Collapse to Definition");
          //  miCollapse.Click += (_, __) => _foldingControl.CollapseAllInnerBlocksExceptOuter();
            EditorContextMenu.Items.Add(miCollapse);

            var miExpand = new ToolStripMenuItem("Expand All");
       //     miExpand.Click += (_, __) => _foldingControl.ExpandAllBlocksAtCaret();
            EditorContextMenu.Items.Add(miExpand);

            EditorContextMenu.Items.Add(new ToolStripSeparator());

            Editor.ContextMenuStrip = EditorContextMenu;
        }
        private void CommentSelection()
        {
            int selStart = Editor.SelectionStart;
            int selEnd = Editor.SelectionEnd;
            if (selEnd < selStart) (selStart, selEnd) = (selEnd, selStart);
            if (selStart == selEnd)
            {
                // keine Selektion -> aktuelle Zeile
                int line = Editor.LineFromPosition(selStart);
                selStart = Editor.Lines[line].Position;
                selEnd = Editor.Lines[line].EndPosition;
            }
            int startLine = Editor.LineFromPosition(selStart);
            int endLine = Editor.LineFromPosition(Math.Max(selEnd - 1, selStart));
            Editor.BeginUndoAction();
            try
            {
                int delta = 0;
                for (int line = startLine; line <= endLine; line++)
                {
                    int lineStart = Editor.Lines[line].Position;
                    int lineEnd = Editor.Lines[line].EndPosition; // inkl. \r\n vor letztem char
                    // Finde erste nicht-Whitespace Position
                    int p = lineStart;
                    while (p < lineEnd)
                    {
                        int ch = Editor.GetCharAt(p);
                        if (ch != ' ' && ch != '\t') break;
                        p++;
                    }
                    if (p >= lineEnd) continue; // leere / whitespace-only Zeile
                    // Prüfen ob bereits // vorhanden
                    bool already = p + 1 < lineEnd && Editor.GetCharAt(p) == '/' && Editor.GetCharAt(p + 1) == '/';
                    if (already) continue;
                    Editor.InsertText(p, "//");
                    if (line == startLine) selStart += 2; // Cursor verschiebt sich nach Einfügung vor Start
                    selEnd += 2;
                    delta += 2;
                }
                // Auswahl erneut setzen (optional beibehalten)
                Editor.SetSelection(selEnd, selStart); // ScintillaNET erwartet anchor/caret; Umkehren damit Selektion gleich bleibt
            }
            finally { Editor.EndUndoAction(); }
        }
        private void UncommentSelection()
        {
            int selStart = Editor.SelectionStart;
            int selEnd = Editor.SelectionEnd;
            if (selEnd < selStart) (selStart, selEnd) = (selEnd, selStart);
            if (selStart == selEnd)
            {
                int line = Editor.LineFromPosition(selStart);
                selStart = Editor.Lines[line].Position;
                selEnd = Editor.Lines[line].EndPosition;
            }
            int startLine = Editor.LineFromPosition(selStart);
            int endLine = Editor.LineFromPosition(Math.Max(selEnd - 1, selStart));
            Editor.BeginUndoAction();
            try
            {
                for (int line = startLine; line <= endLine; line++)
                {
                    int lineStart = Editor.Lines[line].Position;
                    int lineEnd = Editor.Lines[line].EndPosition;
                    int p = lineStart;
                    while (p < lineEnd)
                    {
                        int ch = Editor.GetCharAt(p);
                        if (ch != ' ' && ch != '\t') break;
                        p++;
                    }
                    if (p + 1 >= lineEnd) continue;
                    if (Editor.GetCharAt(p) == '/' && Editor.GetCharAt(p + 1) == '/')
                    {
                        Editor.DeleteRange(p, 2);
                        if (p < selStart) selStart = Math.Max(lineStart, selStart - 2);
                        selEnd = Math.Max(selStart, selEnd - 2);
                    }
                }
                Editor.SetSelection(selEnd, selStart);
            }
            finally { Editor.EndUndoAction(); }
        }

        public void BeginnUpdate()
        {
            SendMessage(Editor.Handle, WM_SETREDRAW, false, 0);
        }
        public void EndUpdate()
        {
            SendMessage(Editor.Handle, WM_SETREDRAW, true, 0);
        }

        public RoslynDocument? GetCurrentDocument()
        {
            return SelectedNode?.RoslynDoc;
        }

        public void ResetCurrentFile()
        {
            _currentFile = null;
        }

        // UI Invoke Helper
        public void OnUi(System.Action a)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke(a); else a();
        }
        private static bool IsBrace(int c) => c is '(' or ')' or '[' or ']' or '{' or '}';

        private static readonly string CsKeywords = "abstract as base bool break byte case catch char checked class const continue decimal default delegate do double else enum event explicit extern false finally fixed float for foreach goto if implicit in int interface internal is lock long namespace new null object operator out override params private protected public readonly ref return sbyte sealed short sizeof stackalloc static string struct switch this throw true try typeof uint ulong unchecked unsafe ushort using virtual void volatile while var dynamic partial record global alias async await nameof when where yield get set add remove file required scoped nint nuint not and or";

        private static readonly string[] FallbackKeywords = new[] {
            "System","public","private","protected","internal","class","interface","struct","record","void","string","int","var","using","namespace","return","async","await","new","if","else","for","foreach","while","switch","case","break","continue","try","catch","finally","true","false","null","partial","static","readonly","override","virtual","sealed","params","out","ref" };

        // --- Helper (bereinigt) -------------------------------------------------
        private static readonly Regex RxTypeDecl = new(@"\b(class|record|struct|interface)\s+([A-Za-z_][A-ZaZ0-9_]*)", RegexOptions.Compiled);
        private static readonly Regex RxVarDecl = new(@"\b(?:(?:public|private|protected|internal|static|readonly|volatile|const|ref|out|in|unsafe|sealed|new|partial|extern)\s+)*([A-ZaZ_][A-ZaZ0-9_<>,\.?]*)\s+(?<name>[A-ZaZ_][A-ZaZ0-9_]*)\s*(?=[=;,)\r\n])", RegexOptions.Compiled);
        private static readonly Regex RxPropertyDecl = new(@"\b(?:(?:public|private|protected|internal|static|readonly|sealed|new|partial)\s+)*([A-ZaZ_][A-ZaZ0-9_<>,\.?]*)\s+(?<pname>[A-ZaZ_][A-ZaZ0-9_]*)\s*\{", RegexOptions.Compiled);
        private static readonly Regex RxParameter = new(@"[(,]\s*(?:this\s+)?(?:ref|out|in\s+)?[A-Za-z_][A-ZaZ0-9_<>,\.?]*\s+(?<param>[A-ZaZ_][A-ZaZ0-9_]*)\s*(?=,|\)|=)", RegexOptions.Compiled);
        private List<string> ExtractLocalTypeNames(string text)
        {
            var list = new List<string>();
            foreach (Match m in RxTypeDecl.Matches(text)) if (m.Success) list.Add(m.Groups[2].Value);
            return list;
        }
        private HashSet<string> CollectDeclaredIdentifiers(string text, int caret)
        {
            if (caret < text.Length)
                text = text.Substring(0, caret);

            var set = new HashSet<string>(StringComparer.Ordinal);

            foreach (Match m in RxVarDecl.Matches(text))
                if (m.Success)
                    set.Add(m.Groups["name"].Value);

            foreach (Match m in RxPropertyDecl.Matches(text))
                if (m.Success)
                    set.Add(m.Groups["pname"].Value);

            foreach (Match m in RxParameter.Matches(text))
                if (m.Success)
                    set.Add(m.Groups["param"].Value);

            return set;
        }
        private IEnumerable<string> SuggestVariableNames(string typeName)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            var tokens = typeName.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            var merged = string.Concat(tokens);

            if (merged.Length > 0)
                set.Add(char.ToLowerInvariant(merged[0]) + merged.Substring(1));

            if (tokens.Length > 0)
            {
                var first = tokens[0];
                if (first.Length > 0)
                    set.Add(char.ToLowerInvariant(first[0]) + first.Substring(1));

                var last = tokens[tokens.Length - 1];
                if (last.Length > 0)
                    set.Add(char.ToLowerInvariant(last[0]) + last.Substring(1));

                foreach (string t in tokens)
                {
                    if (string.Equals(t, "Page", StringComparison.OrdinalIgnoreCase))
                    {
                        set.Add("page");
                        break;
                    }
                }

                if (tokens.Length > 1 && string.Equals(tokens[0], "Page", StringComparison.OrdinalIgnoreCase))
                    set.Add("page" + last);
            }

            return set;
        }
        private string GetPreviousIdentifier(int position)
        {
            int pos = position - 1; while (pos >= 0 && char.IsWhiteSpace((char)Editor.GetCharAt(pos))) pos--; if (pos < 0) return string.Empty;
            int end = pos; while (pos >= 0) { char ch = (char)Editor.GetCharAt(pos); if (!(char.IsLetterOrDigit(ch) || ch == '_')) break; pos--; }
            int start = pos + 1; return end >= start ? Editor.GetTextRange(start, end - start + 1) : string.Empty;
        }
        private bool IsInMemberAccessContext(int caret)
        {
            if (caret <= 0) return false;
            int pos = caret - 1;
            while (pos >= 0)
            {
                char ch = (char)Editor.GetCharAt(pos);
                if (char.IsLetterOrDigit(ch) || ch == '_') pos--; else break;
            }
            return pos >= 0 && Editor.GetCharAt(pos) == '.';
        }
        public string GetCurrentIdentifierPrefix(int caret)
        {
            if (caret == 0) return string.Empty;
            int pos = caret - 1;
            while (pos >= 0)
            {
                char ch = (char)Editor.GetCharAt(pos);
                if (!(char.IsLetterOrDigit(ch) || ch == '_')) break;
                pos--;
            }
            int start = pos + 1; return Editor.GetTextRange(start, caret - start);
        }
        private async Task TryAddUsingForIdentifierBeforeDotAsync()
        {
            if (SelectedNode == null || !_roslyn.IsProjectLoaded) return;
            int caret = Editor.CurrentPosition;
            int dotPos = caret - 1; // just inserted '.'
            if (dotPos <= 0) return;
            int pos = dotPos - 1;
            while (pos >= 0)
            {
                char ch = (char)Editor.GetCharAt(pos);
                if (!(char.IsLetterOrDigit(ch) || ch == '_')) break;
                pos--;
            }
            int start = pos + 1;
            if (start >= dotPos) return;
            string ident = Editor.GetTextRange(start, dotPos - start);
            if (string.IsNullOrWhiteSpace(ident)) return;
            char prev = start > 0 ? (char)Editor.GetCharAt(start - 1) : '\0';
            if (prev == ')') return; // probable method call
            var newText = await _roslyn.AddUsingIfUniqueTypeAsync(SelectedNode.RoslynDoc, Editor.Text, ident);
            if (newText != null && !ReferenceEquals(newText, Editor.Text))
            {
                int oldPos = Editor.CurrentPosition;
                Editor.Text = newText;
                Editor.GotoPosition(Math.Min(oldPos, Editor.TextLength));
            }
        }

        public async Task Reload()
        {
            SaveExpandedNodes();

            string NodeName = SelectedNode.Name;
            int firstLine = Editor.FirstVisibleLine;
            int carret = Editor.CurrentPosition;
            await LoadBook();
            SelectNodeByName(NodeName);
            Editor.Focus();
            Editor.FirstVisibleLine = firstLine;
            Editor.GotoPosition(carret);

            RestoreExpandedNodes();


        }
        public async Task LoadBook()
        {
           
            _roslyn.Reset();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            _roslyn = new RoslynServices();
            ProjectTree.Nodes.Clear();

            tblBuildOutputs.Clear();
            ProjectTree.BeginUpdate();
            ProjectTree.ImageList = imageList1;
            ProjectTree.Nodes.Clear();

            RootNode = new EditorNode(qbook.Core.ThisBook.Filename)
            {
                ImageIndex = 1
            };

            ProjectTree.Font = new Font("Calibri", 12);
            ProjectTree.Nodes.Add(RootNode);

            string program = "namespace QB\r\n{\r\n   public static class Program {\r\n";
            var roslynFiles = new List<(string fileName, string code)>();
            int pageCount = -1;
            string firstFile = null;

            foreach (oPage page in qbook.Core.ActualMain.Objects.OfType<oPage>())
            {
                pageCount++;
                string code = page.CsCode;

                List<string> includes = ReplaceIncludesWithBlock(ref code);

                int index = code.IndexOf("public class");
                string insertText = "\r\nnamespace class_" + page.Name + "\r\n{\r\n";
                code = code.Insert(index - 1, insertText) + "\r\n}";

                string className = "class_" + page.Name + ".@class_" + page.Name;
                program += "   public static " + className + " " + page.Name + " { get; } = new " + className + "();\r\n";

                string fileName = $"class_{page.Name}.cs";
                roslynFiles.Add((fileName, code));

                EditorNode pageNode = new EditorNode(page, _roslyn, fileName, EditorNode.NodeType.Page);
                pageNode.Code = code;
                pageNode.Active = true;
                ProjectTree.Nodes[0].Nodes.Add(pageNode);

                if (firstFile == null)
                    firstFile = fileName;

                var lines = code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                var usings = lines
                    .TakeWhile(l => !l.TrimStart().StartsWith("public class"))
                    .Where(l => l.TrimStart().StartsWith("using"))
                    .ToList();


                int subCount = 0;

                foreach (var subClass in page.CsCodeExtra)
                {
                    subCount++;
                    string sub = string.Join("\r\n", usings) + "\r\n\r\nnamespace class_" + page.Name + "\r\n{\r\n" + subClass.Value + "\r\n}";
                    string subFileName = $"sub_{page.Name}_{subClass.Key}.cs";

                    EditorNode subNode = new EditorNode(page, _roslyn, subFileName, EditorNode.NodeType.SubCode, subClass.Key, pageNode);
                    subNode.Code = sub;

                    subNode.Active = includes.Contains(subClass.Key);
                    if (subNode.Active)
                        roslynFiles.Add((subFileName, sub));

                    subNode.ImageIndex = subNode.Active ? 3 : 5;
                    ProjectTree.Nodes[0].Nodes[pageCount].Nodes.Add(subNode);
                }
            }

            program += "   }\r\n}";



            roslynFiles.Add(("Program.cs", program));

            roslynFiles.Add(("GlobalUsing.cs", "global using static QB.Program;"));


            string root = AppDomain.CurrentDomain.BaseDirectory;
            string baseDir = Path.Combine(root, "libs");

            var dllFiles = Directory.GetFiles(baseDir, "*.dll").ToArray();
            string[] exclude = { "Microsoft.CodeAnalysis" };

            dllFiles = dllFiles
                .Where(path => !exclude.Any(prefix => Path.GetFileNameWithoutExtension(path)
                    .StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            var references = dllFiles.Select(path => MetadataReference.CreateFromFile(path)).ToList();
            await _roslyn.LoadInMemoryProjectAsync(roslynFiles, references);



            // Verknüpfe alle EditorNodes mit den Roslyn Documents

            foreach (TreeNode node in ProjectTree.Nodes[0].Nodes)
            {
                if (node is EditorNode editorNode)
                {
                    editorNode.RoslynDoc = _roslyn.GetDocument(editorNode.FilePath);
                }

                foreach (TreeNode sub in node.Nodes)
                {
                    if (sub is EditorNode subNode)
                    {
                        subNode.RoslynDoc = _roslyn.GetDocument(subNode.FilePath);
                    }
                }
            }

            //if (firstFile != null)
            //{
            //    var text = await _roslyn.GetDocumentTextAsync(firstFile);
            //    Editor.Text = text;
            //    _currentFile = firstFile;
            //    await _roslyn.UpdateOpenDocumentAsync(firstFile, text);
            //    await _roslyn.GetCompletionsAsync(firstFile, text, 0);
            //}

            Core.ProgramWorkingDir = "InMemory";
            QB.Logger.Info("Using In-Memory Working Directory");
            ProjectTree.EndUpdate();
        }
        private void SaveFile()
        {
            if (string.IsNullOrEmpty(_currentFile))
            {
                SaveFileAs();
                return;
            }
            File.WriteAllText(_currentFile!, Editor.Text);
        }
        private void SaveFileAs()
        { using var sfd = new SaveFileDialog { Filter = "C# files|*.cs|All files|*.*" }; if (sfd.ShowDialog(this) == DialogResult.OK) { _currentFile = sfd.FileName; SaveFile(); } }

        public List<string> ReplaceIncludesWithBlock(ref string source)
        {
            Debug.WriteLine("===== ExtractIncludes ======");
            Debug.WriteLine(source);

            List<string> includes = new List<string>();
            if (string.IsNullOrWhiteSpace(source)) return includes;

            var regex = new Regex(@"//\+include\s+(\w+)", RegexOptions.Compiled);
            var lines = source.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var newLines = new List<string>();
            var includeLines = new List<string>();
            int lineNumber = 0;
            int includeLineNumber = 0;

            bool inIncludeBlock = false;
            bool includeStartExists = false;
            bool includeEndExists = false;

            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (match.Success)
                {
                    includes.Add(match.Groups[1].Value);
                    includeLines.Add(line);
                }
                else
                {
                    if (!line.Contains("//<IncludeStart>") && !line.Contains("//<IncludeEnd>"))
                    {
                        newLines.Add(line);
                    }

                }

                if (line.Contains("public class @"))
                {
                    includeLineNumber = lineNumber;
                }
                //{
                //    // Stoppe das Sammeln, wenn ein Include-Block bereits existiert
                //    includeLines.Clear();
                //}
                lineNumber++;

            }
            Debug.WriteLine("Insert Startline = " + includeLineNumber);
            Debug.WriteLine("===== Includes ======");

            List<string> includeBlock = new List<string>();

            foreach (string l in includes) Debug.WriteLine(l);

            if (includeLines.Count > 0)
            {
                includeBlock.Add("//<IncludeStart>");
                includeBlock.AddRange(includeLines);
                includeBlock.Add("//<IncludeEnd>");

                // Optional: Du kannst entscheiden, wo der Block eingefügt wird.
                // Hier wird er am Anfang eingefügt.

            }
            else
            {
                includeBlock.Add("\t//<IncludeStart>");
                includeBlock.Add("");
                includeBlock.Add("\t//<IncludeEnd>");
            }
            newLines.InsertRange(includeLineNumber + 2, includeBlock);

            source = string.Join("\n", newLines);
            Debug.WriteLine("===== Updated Source ======");
            Debug.WriteLine(source);
            return includes;
        }

        // ---------------- Check Code ----------------

        bool buildError = false;
        async Task CheckCode()
        {
            _diagnostic.BuildError = false;


            foreach (EditorNode page in ProjectTree.Nodes[0].Nodes)
            {

                await _diagnostic.BuildRunDiagnosticsAsync(page);
                foreach (EditorNode subcode in page.Nodes)
                {
                    await _diagnostic.BuildRunDiagnosticsAsync(subcode);
                }
            }
        }

        public async Task HandleRebuildAsync()
        {
            _diagnostic.BuildOutput.Clear();
            SaveCode();
            SetStatusText("Rebuild ...");
            await Task.Yield();
            await CheckCode();

            if (_diagnostic.BuildError)
            {
                MessageBox.Show("Your code contains errors. Please correct them before proceeding.");
                return;
            }

            await RebuildCodeAllAsync();

        }
        async Task RebuildCodeAllAsync()
        {
            SetStatusText("Rebuilding Script...");
            TableOutputClear();
            log4net.Repository.Hierarchy.Hierarchy hierarchy = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
            hierarchy.Root.RemoveAllAppenders();
            QB.Logger.Info("Rebuilding Script...");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Restart();
            try
            {
                this.Cursor = Cursors.WaitCursor;
                try
                {
                    await qbook.Core.CsScriptRebuildAll(); //BuildCsScriptAll();
                    sw.Stop();
                    SetStatusText("Success: rebuild took " + sw.ElapsedMilliseconds.ToString() + "ms");
                    this.Cursor = Cursors.Default;

                }
                catch (Exception ex)
                {
                    QB.Logger.Error("#ERR Rebuilding Script: " + ex.Message);
                    this.Cursor = Cursors.Default;
                    sw.Stop();

                }
            }
            catch (Exception ex)
            {

                sw.Stop();
                SetStatusText("#ERR Rebuilding: rebuild took " + sw.ElapsedMilliseconds.ToString() + "ms");
                return;
            }

        }



        // ---------------- Roslyn wiring / Completion ----------------

        private const int WM_SETREDRAW = 0x000B;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, bool wParam, int lParam);

        bool refreshing = false;

        public static async Task ApplyDiagnosticsAsync(Scintilla editor, RoslynDocument roslynDoc, int indicatorIndex = 0)
        {
            if (roslynDoc == null || editor == null) return;

            var semanticModel = await roslynDoc.GetSemanticModelAsync();
            var diagnostics = semanticModel.GetDiagnostics();

            editor.IndicatorCurrent = indicatorIndex;
            editor.IndicatorClearRange(0, editor.TextLength);

            foreach (var diag in diagnostics)
            {
                var span = diag.Location.SourceSpan;
                int start = Math.Max(0, Math.Min(span.Start, editor.TextLength));
                int length = Math.Max(0, Math.Min(span.Length, editor.TextLength - start));
                if (length > 0)
                {
                    editor.IndicatorFillRange(start, length);
                }
            }
        }



        public async Task RefreshEditor(string sender)
        {
        
            refreshing = true;
            _debounceTimer.Stop();
            if (SelectedNode == null) return;

            //Beginn Update Editor
            BeginnUpdate();
            vBarEditor.SyncPause = true;

            int firstLine = Editor.FirstVisibleLine;
            int pos = Editor.CurrentPosition;

            string code = await SelectedNode.UpdatedCodeForQbook("Refresh Editor", Editor.Text);
            Editor.Text = code;
            RoslynFoldingHelper.ApplyFolding(Editor);
            RoslynFoldingHelper.RestoreFolding(Editor);
            await ApplyEditorTextToRoslyn(SelectedNode, code);
            await qbookStyleHide();

            if (SelectedNode.Active)
            {
                await SemanticHighlighter.ApplyAsync(Editor, SelectedNode.RoslynDoc);
                await _diagnostic.ApplyAsync();
            }
            Editor.FirstVisibleLine = firstLine;
            Editor.GotoPosition(pos);


           
            EndUpdate();
            Editor.Refresh();

            UpdateMethodes();
            
            refreshing = false;
            vBarEditor.SyncPause = false;
        }

        async Task qbookStyleHide()
        {
            //Hide usings in subcode <- is mangaged by PageNode
            if (SelectedNode.Type == EditorNode.NodeType.SubCode)
            {
                var (startLine, endLine) = await SelectedNode.GetUsingBlockLineRange();
                Editor.HideLines(startLine, endLine);
            }
            //Hide includes in pagecode <- is mangaged by Treeview UI
            if (SelectedNode.Type == EditorNode.NodeType.Page)
            {
                var (startLine, endLine) = await SelectedNode.GetIncludeBlockLineRange();
                Editor.HideLines(startLine, endLine);
            }
        }

        public async Task ApplyEditorTextToRoslyn(EditorNode node, string newEditorText)
        {
            var workspace = node.RoslynDoc.Project.Solution.Workspace;
            var docId = node.RoslynDoc.Id;
            var doc = workspace.CurrentSolution.GetDocument(docId);
            if (doc == null) return;


            var newText = SourceText.From(newEditorText);

            var oldText = await doc.GetTextAsync();

            if (!newText.ContentEquals(oldText)) // schneller als GetTextChanges
            {
                var updatedDoc = doc.WithText(newText);
                if (workspace.TryApplyChanges(updatedDoc.Project.Solution))
                    node.RoslynDoc = workspace.CurrentSolution.GetDocument(docId);
            }
        }




        //async Task ApplyEditorTextToRoslyn(EditorNode node, string newText)
        //{
        //    var doc = node.RoslynDoc;
        //    if (doc == null) return;
        //    var updated = doc.WithText(SourceText.From(newText));
        //    // if (doc.Project.Solution.Workspace.TryApplyChanges(updated.Project.Solution))
        //    node.RoslynDoc = updated; // Referenz aktualisieren


        //}

        private async Task GoToDefinitionAsync()
        {
            if (SelectedNode == null) return;

            int caret = Editor.CurrentPosition;
            var loc = await _roslyn.GoToDefinitionAsync(SelectedNode.RoslynDoc, caret);
            if (loc == null) return;

            var (doc, line, column) = loc.Value;

            EditorNode? node = GetNodeByDocument(doc);
            if (node == null) return;

            await OpenNode(node);

            int pos = GetPositionFromLineColumn(Editor, line, column);
            Editor.GotoPosition(pos);
            Editor.ScrollCaret();
        }
        private EditorNode? GetNodeByDocument(RoslynDocument doc)
        {
            if (doc == null) return null;

            foreach (EditorNode node in ProjectTree.Nodes[0].Nodes)
            {

                if (node.RoslynDoc.Name == doc.Name)
                {
                    Debug.WriteLine("node found: " + node.Name);
                    return node;
                }

                foreach (EditorNode sub in node.Nodes)
                {
                    if (sub.RoslynDoc.Name == doc.Name)
                    {
                        Debug.WriteLine("node found: " + sub.Name);
                        return sub;
                    }
                }
            }

            return null;
        }

        public static string ShowReplaceDialog(string prompt, string title, string defaultValue = "")
        {
            Form inputForm = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterScreen
            };

            System.Windows.Forms.Label textLabel = new System.Windows.Forms.Label() { Left = 10, Top = 20, Text = prompt, AutoSize = true };
            System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox() { Left = 10, Top = 50, Width = 360, Text = defaultValue };
            System.Windows.Forms.Button confirmation = new System.Windows.Forms.Button() { Text = "OK", Left = 290, Width = 80, Top = 80, DialogResult = System.Windows.Forms.DialogResult.OK };

            inputForm.Controls.Add(textLabel);
            inputForm.Controls.Add(textBox);
            inputForm.Controls.Add(confirmation);
            inputForm.AcceptButton = confirmation;

            return inputForm.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }
        private async Task RenameSymbolAsync()
        {
            if (SelectedNode?.RoslynDoc == null)
            {
                MessageBox.Show(this, "Project not loaded or no document selected.", "Rename", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int caretBefore = Editor.CurrentPosition;
            int selStartBefore = Editor.SelectionStart;
            int selEndBefore = Editor.SelectionEnd;
            int firstLine = Editor.FirstVisibleLine;

            var input = ShowReplaceDialog("Neuer Name:", "Rename Symbol", "NewName");
            if (string.IsNullOrWhiteSpace(input)) return;
            string newName = input.Trim();

            var doc = SelectedNode.RoslynDoc;
            var semanticModel = await doc.GetSemanticModelAsync();
            var syntaxTree = await doc.GetSyntaxTreeAsync();


            var root = await syntaxTree.GetRootAsync();
            var position = caretBefore > 0 ? caretBefore - 1 : caretBefore;
            var token = root.FindToken(position);
            var nodeParent = token.Parent;

            var symbol = semanticModel.GetSymbolInfo(nodeParent).Symbol ?? semanticModel.GetDeclaredSymbol(nodeParent);


            Debug.WriteLine($"Rename '{symbol}' -> '{input}'");
            if (symbol == null)
            {
                MessageBox.Show(this, $"Rename failed (symbol not found). {input}", "Rename", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var solution = doc.Project.Solution;
            var newSolution = await Renamer.RenameSymbolAsync(solution, symbol, newName, solution.Workspace.Options);

            var allDocs = _roslyn.GetAllDocuments();
            int updatedCount = 0;

            foreach (var oldDoc in allDocs)
            {
                var newDoc = newSolution.GetDocument(oldDoc.Id);
                if (newDoc == null) continue;

                var newText = await newDoc.GetTextAsync();
                var newTextStr = newText.ToString();
                var oldTextStr = (await oldDoc.GetTextAsync()).ToString();

                if (!string.Equals(newTextStr, oldTextStr, StringComparison.Ordinal))
                {
                    var node = GetNodeByDocument(oldDoc);
                    if (node != null)
                    {
                        node.AddUndo(Editor.Text, Editor.CurrentPosition, Editor.FirstVisibleLine);
                        node.RoslynDoc = newDoc;
                        updatedCount++;

                        if (node == SelectedNode)
                        {
                            Editor.BeginUndoAction();
                            Editor.Text = newTextStr;
                            Editor.EndUndoAction();
                        }
                    }
                }
            }

            if (updatedCount == 0)
            {
                MessageBox.Show(this, $"Rename failed (no changes).  {input}", "Rename", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Cursor und Auswahl wiederherstellen
            int newLen = Editor.TextLength;
            Editor.SetSelection(Math.Min(selEndBefore, newLen), Math.Min(selStartBefore, newLen));
            Editor.GotoPosition(Math.Min(caretBefore, newLen));
            Editor.Lines[firstLine].Goto();

            await RefreshEditor("RenameSymbol");
           // await RefreshDiagnosticsAsync();
            Editor.FirstVisibleLine = firstLine;
            Editor.GotoPosition(caretBefore);
        }

        private static int GetPositionFromLineColumn(Scintilla ed, int line, int column) => ed.Lines[line].Position + column;

        //=========== ProjectTree =========== 

        public EditorNode SelectedNode;



        public bool NodeIsActive => SelectedNode.Active;

        public void ResetNode() => SelectedNode = null;
        private void UpdateAllNodes(System.Windows.Forms.TreeView tree, Color color, int nodeLevel, int newIndex)
        {
            foreach (EditorNode node in tree.Nodes)
            {
                UpdateNodeByLevelRecursive(node, color, nodeLevel, newIndex);
            }
        }
        private void UpdateNodeByLevelRecursive(EditorNode node, Color color, int oldIndex, int newIndex)
        {
            node.ForeColor = color;
            node.ImageIndex = node.ImageIndex == oldIndex ? newIndex : node.ImageIndex;
            node.SelectedImageIndex = newIndex;

            foreach (EditorNode child in node.Nodes)
            {
                UpdateNodeByLevelRecursive(child, color, oldIndex, newIndex);
            }
        }

        EditorNode _clickedNode;
        private async void TreeProject_NodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                await OpenNode(e.Node as EditorNode);
                UpdateTabs();
            }

            if (e.Button == MouseButtons.Right)
            {
                _clickedNode = e.Node as EditorNode;

                if (_clickedNode.Active)
                {
                    toolStripMenuIncludeCode.Text = "Exclude Code";
                }
                else
                {
                    toolStripMenuIncludeCode.Text = "Include Code";
                }


                oPage page = FindPageByName(Regex.Replace(SelectedNode.Text, @"^[^_]*_", ""));
                if (page != null)
                {
                    hidePageToolStripMenuItem.Text = page.Hidden ? "Show Page" : "Hide Page";
                }

                var clickedNode = ((System.Windows.Forms.TreeView)sender).GetNodeAt(new Point(e.X, e.Y));
                if (e.Node.Level == 0)
                {
                    addPageBeforeToolStripMenuItem.Visible = false;
                    addPageAfterToolStripMenuItem.Visible = false;
                    hidePageToolStripMenuItem.Visible = false;
                    addSubCodeToolStripMenuItem.Visible = false;
                    deleteStripMenuItem.Visible = false;
                    toolStripMenuOpenWorkspace.Visible = true;
                    renamePageToolStripMenuItem.Visible = false;
                    renameCodeToolStripMenuItem.Visible = false;
                    toolStripMenuIncludeCode.Visible = false;
                }
                if (e.Node.Level == 1)
                {
                    addPageBeforeToolStripMenuItem.Visible = true;
                    addPageAfterToolStripMenuItem.Visible = true;
                    hidePageToolStripMenuItem.Visible = true;
                    addSubCodeToolStripMenuItem.Visible = true;
                    renamePageToolStripMenuItem.Visible = false;
                    deleteStripMenuItem.Visible = false;
                    toolStripMenuOpenWorkspace.Visible = false;
                    renameCodeToolStripMenuItem.Visible = false;
                    toolStripMenuIncludeCode.Visible = false;
                }

                if (e.Node.Level == 2)
                {
                    addPageBeforeToolStripMenuItem.Visible = false;
                    addPageAfterToolStripMenuItem.Visible = false;
                    hidePageToolStripMenuItem.Visible = false;
                    addSubCodeToolStripMenuItem.Visible = false;
                    toolStripMenuOpenWorkspace.Visible = false;
                    toolStripMenuIncludeCode.Visible = true;

                    renamePageToolStripMenuItem.Visible = false;

                    if (SelectedNode.Text.StartsWith("0_"))
                    {
                        renameCodeToolStripMenuItem.Visible = false;
                        deleteStripMenuItem.Visible = false;
                    }
                    else
                    {
                        renameCodeToolStripMenuItem.Visible = true;
                        deleteStripMenuItem.Visible = true;
                    }


                }
                contextMenuTreeView.Show(ProjectTree, e.Location);
            }



        }

        List<string> expandedNodes = new List<string>();
        void SaveExpandedNodes()
        {
            foreach (TreeNode node in ProjectTree.Nodes)
            {
                if (node.IsExpanded)
                {
                    expandedNodes.Add(node.FullPath); // oder node.Name, falls gesetzt
                }

                foreach (TreeNode subnode in node.Nodes)
                {
                    if (subnode.IsExpanded)
                    {
                        expandedNodes.Add(subnode.FullPath); // oder subnode.Name, falls gesetzt
                    }

                }
            }
        }
        void RestoreExpandedNodes()
        {
            _restoreExpandedNodes(ProjectTree.Nodes);
        }
        void _restoreExpandedNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (expandedNodes.Contains(node.FullPath))
                {
                    node.Expand();
                }

                _restoreExpandedNodes(node.Nodes);
            }
        }


        //=========== Methodes List ===========

        private void UpdateMethodes()
        {
            tblMethodes.Clear();

            // Editor-Text in Zeilen zerlegen
            var lines = Editor.Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            // Regex für Methoden
            var methodPattern = new Regex(@"\b(void|double|int|float|bool|public|private|protected|internal|static|async)?\s+(?:[\w<>]+\s+)+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\(.*\)\s*(\{|=>)");

            // Regex für Klassen
            var classPattern = new Regex(@"\b(public|private|protected|internal)?\s*(partial\s+)?class\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)");

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                // Methoden suchen
                var methodMatch = methodPattern.Match(line);
                if (methodMatch.Success)
                {
                    string methodName = methodMatch.Groups["name"].Value;
                    DataRow row = tblMethodes.NewRow();
                    row["Row"] = i + 1;
                    row["Name"] = "[M] " + methodName;
                    tblMethodes.Rows.Add(row);
                }

                // Klassen suchen
                var classMatch = classPattern.Match(line);
                if (classMatch.Success)
                {
                    string className = classMatch.Groups["name"].Value;
                    DataRow row = tblMethodes.NewRow();
                    row["Row"] = i + 1;
                    row["Name"] = "[C] " + className;
                    tblMethodes.Rows.Add(row);
                }
            }

            gridViewMethodes.ClearSelection();
        }
        private System.Windows.Forms.Timer debounceTimer;
        private void Editor_KeyDown(object sender, KeyEventArgs e)
        {
            debounceTimer?.Stop();
            debounceTimer = new System.Windows.Forms.Timer();
            debounceTimer.Interval = 1000;
            debounceTimer.Tick += (s, args) =>
            {
                debounceTimer.Stop();
                UpdateMethodes();
            };
            debounceTimer.Start();
        }


        //=========== Helpers ===========


        private oPage FindPageByName(string name)
        {
            Debug.WriteLine("Searching oPage '" + name + "'");
            if (string.IsNullOrEmpty(name)) return null;
            if (qbook.Core.ThisBook == null) return null;
            foreach (oPage page in qbook.Core.ActualMain.Objects.Where(item => item is oPage))
                if (string.Equals(page.Name, name, StringComparison.OrdinalIgnoreCase))
                    return page;
            return null;
        }
        //private (int start, int len) GetNiceErrorRange(Microsoft.CodeAnalysis.Diagnostic d)
        //{
        //    var span = d.Location.SourceSpan;
        //    int docLen = Editor.TextLength;

        //    // Spezieller Fall fehlendes Semikolon
        //    if (d.Id == "CS1002") // ; expected
        //    {
        //        if (TryGetMissingSemicolonRange(span.Start, out int s, out int l)) return (s, l);
        //    }

        //    // Normale Wort-Umrandung
        //    int s1 = Editor.WordStartPosition(Math.Max(0, span.Start), true);
        //    int e1 = Editor.WordEndPosition(Math.Min(docLen, span.Start + Math.Max(1, span.Length)), true);
        //    if (e1 > s1) return (s1, Math.Min(docLen - s1, e1 - s1));

        //    // Rückwärts zum nächsten Token
        //    int p = Math.Max(0, Math.Min(docLen - 1, span.Start));
        //    string txt = Editor.Text;
        //    while (p > 0 && char.IsWhiteSpace(txt[p])) p--;
        //    s1 = Editor.WordStartPosition(p, true);
        //    e1 = Editor.WordEndPosition(p, true);
        //    if (e1 > s1) return (s1, e1 - s1);

        //    // Fallback: einzelnes Zeichen
        //    int start = Math.Max(0, Math.Min(span.Start, docLen - 1));
        //    return (start, 1);
        //}
        //private bool TryGetMissingSemicolonRange(int pos, out int start, out int len)
        //{
        //    int docLen = Editor.TextLength;
        //    string txt = Editor.Text;
        //    int p = Math.Max(0, Math.Min(docLen - 1, pos > 0 ? pos - 1 : 0));
        //    while (p > 0 && char.IsWhiteSpace(txt[p])) p--;
        //    int s = Editor.WordStartPosition(p, true);
        //    int e = Editor.WordEndPosition(p, true);
        //    if (e > s)
        //    {
        //        start = s; len = e - s; return true;
        //    }
        //    start = p; len = 1; return true;
        //}
        private static IEnumerable<MetadataReference> GetBasicMetadataReferences()
        {
            var assemblies = new[] {
                typeof(object).Assembly,
                typeof(Console).Assembly,
                typeof(Enumerable).Assembly,
                typeof(List<>).Assembly,
                typeof(Task).Assembly,
                typeof(System.Runtime.GCSettings).Assembly
            };
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var list = new List<MetadataReference>();
            foreach (var asm in assemblies)
            {
                try
                {
                    var path = asm.Location;
                    if (!string.IsNullOrEmpty(path) && File.Exists(path) && seen.Add(path))
                        list.Add(MetadataReference.CreateFromFile(path));
                }
                catch { }
            }
            return list;
        }


        private ControlFindReplace _findReplaceControl;
        private void ShowSearchBar()
        {
            //if (_findReplaceControl == null)
            //{
            //    _findReplaceControl = new ControlFindReplace(this, _roslyn, dataGridViewFindReplace);
            //    Editor.Controls.Add(_findReplaceControl);
            //}

            //_findReplaceControl.Location = new Point(Editor.Width - _findReplaceControl.Width, 0); // Rechts oben
            //if (_currentTheme == EditorTheme.Light) _findReplaceControl.LightTheme();
            //if (_currentTheme == EditorTheme.Dark) _findReplaceControl.DarkTheme();

            //_findReplaceControl.BringToFront();
            //_findReplaceControl.ShowFind();
            //_findReplaceControl.Visible = true;
            //_findReplaceControl.FocusFind();
        }
        private void ShowReplaceBar()
        {
            //if (_findReplaceControl == null)
            //{
            //    _findReplaceControl = new ControlFindReplace(this, _roslyn, dataGridViewFindReplace);
            //    _findReplaceControl.Location = new Point(Editor.Width - _findReplaceControl.Width, 0); // Rechts oben
            //    Editor.Controls.Add(_findReplaceControl);
            //}


            //if (_currentTheme == EditorTheme.Light) _findReplaceControl.LightTheme();
            //if (_currentTheme == EditorTheme.Dark) _findReplaceControl.DarkTheme();

            //_findReplaceControl.BringToFront();
            //_findReplaceControl.ShowReplace();

            //_findReplaceControl.Visible = true;
            //_findReplaceControl.FocusFind();
        }

        private TreeNode? FindNodeRecursive(TreeNode node, string filePath)
        {
            if (node.Tag is string tagPath && string.Equals(tagPath, filePath, StringComparison.OrdinalIgnoreCase))
                return node;
            foreach (TreeNode child in node.Nodes)
            {
                var hit = FindNodeRecursive(child, filePath);
                if (hit != null) return hit;
            }
            return null;
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
        private async Task FormatDocumentAsync()
        {
            string input = Editor.Text;



            string? roslyn = await RoslynServices.FormatCSharpAsync(
            input,
            useTabs: false,
            indentSize: 4
            );


            string output = roslyn ?? SimpleIndentFormatter.Reindent(input, 4);


            if (!string.Equals(input, output, StringComparison.Ordinal))
            {
                int pos = Editor.CurrentPosition; // Cursor merken
                Editor.Text = output;


                if (SelectedNode.Type == EditorNode.NodeType.SubCode)
                {

                    var (startLine, endLine) = await SelectedNode.GetUsingBlockLineRange();
                    Editor.HideLines(startLine, endLine);


                }

                Editor.SetEmptySelection(Math.Min(pos, Editor.TextLength));
            }
        }


        //================= Code Edit //=================

        private async Task IncludeCode(EditorNode node)
        {
            node.PageNode.AddUndo(Editor.Text, Editor.CurrentPosition, Editor.FirstVisibleLine);

            string code = await node.PageNode.GetCodeAsync("FormEditor.IncludeCode");
            List<string> lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();

            string newInclude = "//+include " + node.Text;
            bool includeExists = lines.Any(line => line.Trim() == newInclude);

            // Wenn Include bereits existiert, nichts tun
            if (includeExists)
            {
                Debug.WriteLine("Include already exists");
                return;
            }

            // Finde Start- und Endindex des Include-Bereichs
            int startIndex = lines.FindIndex(line => line.Trim() == "//<IncludeStart>");
            int endIndex = lines.FindIndex(line => line.Trim() == "//<IncludeEnd>");

            if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
            {
                Debug.WriteLine("Include markers not found or invalid");
                return;
            }

            // Sammle alle bisherigen Includes im Bereich
            var includeLines = lines
                .Skip(startIndex + 1)
                .Take(endIndex - startIndex - 1)
                .Where(line => line.Trim().StartsWith("//+include"))
                .Select(line => line.Trim())
                .ToList();

            // Füge neuen Include hinzu
            includeLines.Add(newInclude);

            // Entferne Duplikate und sortiere optional
            includeLines = includeLines.Distinct().OrderBy(x => x).ToList();

            // Ersetze den Bereich zwischen Start und End durch die neuen Includes
            var updatedLines = new List<string>();
            updatedLines.AddRange(lines.Take(startIndex + 1));
            updatedLines.AddRange(includeLines.Select(line => "\t" + line));
            updatedLines.AddRange(lines.Skip(endIndex));

            string updatedCode = string.Join(Environment.NewLine, updatedLines);
            node.PageNode.RoslynDoc = node.PageNode.RoslynDoc.WithText(SourceText.From(updatedCode));
            SaveCode();
            Reload();
        }
        private async Task ExcludeCode(EditorNode node)
        {
            node.PageNode.AddUndo(Editor.Text, Editor.CurrentPosition, Editor.FirstVisibleLine);
            string code = await node.PageNode.GetCodeAsync("FormEditor.ExcludeCode");
            List<string> lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
            string remove = $"//+include {node.Text}";
            lines.RemoveAll(line => line.Trim().Contains("//+include") && line.EndsWith(node.Text));
            node.Active = false;
            string updatedCode = string.Join(Environment.NewLine, lines);
            node.PageNode.RoslynDoc = node.PageNode.RoslynDoc.WithText(SourceText.From(updatedCode));

            SaveCode();
            Reload();

        }

        //================= Output / Build Output =================

        void InitOutput()
        {
            GridViewDiagnosticOutput = new DataGridView();
      

            GridViewDiagnosticOutput.DataSource = _diagnostic.Output;

            GridViewDiagnosticOutput.DataBindingComplete += (s, e) =>
            {
                GridViewDiagnosticOutput.Columns["Page"].Width = 100;
                GridViewDiagnosticOutput.Columns["Class"].Width = 100;
                GridViewDiagnosticOutput.Columns["Position"].Width = 0;
                GridViewDiagnosticOutput.Columns["Length"].Width = 0;
                GridViewDiagnosticOutput.Columns["Type"].Width = 80;
                GridViewDiagnosticOutput.Columns["Node"].Width = 0;

                GridViewDiagnosticOutput.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            };


            GridViewDiagnosticOutput.AllowUserToResizeColumns = false;
            GridViewDiagnosticOutput.AllowUserToAddRows = false;
            GridViewDiagnosticOutput.RowHeadersVisible = false;
            GridViewDiagnosticOutput.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            GridViewDiagnosticOutput.MultiSelect = false;
            GridViewDiagnosticOutput.ReadOnly = true;
            GridViewDiagnosticOutput.BackgroundColor = Color.White;
            GridViewDiagnosticOutput.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            GridViewDiagnosticOutput.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            GridViewDiagnosticOutput.ColumnHeadersVisible = false;
            GridViewDiagnosticOutput.RowHeadersVisible = false;
            GridViewDiagnosticOutput.Dock = DockStyle.Fill;
            GridViewDiagnosticOutput.AllowUserToAddRows = false;
            GridViewDiagnosticOutput.AllowUserToDeleteRows = false;
            GridViewDiagnosticOutput.AllowUserToOrderColumns = true;
            GridViewDiagnosticOutput.AllowUserToResizeColumns = false;
            GridViewDiagnosticOutput.BackgroundColor = System.Drawing.Color.LightGray;
            GridViewDiagnosticOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            GridViewDiagnosticOutput.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            GridViewDiagnosticOutput.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            GridViewDiagnosticOutput.ColumnHeadersVisible = false;
            GridViewDiagnosticOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            GridViewDiagnosticOutput.Location = new System.Drawing.Point(46, 25);
            GridViewDiagnosticOutput.Margin = new System.Windows.Forms.Padding(0);
            GridViewDiagnosticOutput.Name = "dataGridOutput";
            GridViewDiagnosticOutput.Size = new System.Drawing.Size(832, 115);
            GridViewDiagnosticOutput.TabIndex = 0;
            GridViewDiagnosticOutput.ScrollBars = ScrollBars.None;
            GridViewDiagnosticOutput.CellFormatting += (s, e) =>
            {
                if (GridViewDiagnosticOutput.Columns[e.ColumnIndex].Name == "Type")
                {
                    string severity = e.Value?.ToString();
                    switch (severity)
                    {
                        case "Error":
                            GridViewDiagnosticOutput.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Tomato;
                            break;
                        case "Warning":
                            GridViewDiagnosticOutput.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                            break;
                        case "Info":
                            GridViewDiagnosticOutput.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightBlue;
                            break;
                    }
                }
            };
            GridViewDiagnosticOutput.CellClick += (s, e) => Task.Run(async () =>
            {
                if (e.RowIndex >= 0)
                {
                    string pos = GridViewDiagnosticOutput.Rows[e.RowIndex].Cells["Position"].Value.ToString();
                    int length = Convert.ToInt32(GridViewDiagnosticOutput.Rows[e.RowIndex].Cells["Length"].Value);
                    if (Editor.InvokeRequired)
                    {
                        Editor.Invoke(new Action(() =>
                        {
                            EditorOutputJumpToPosition(pos, length);
                            }));
                    }
                    else
                    {
                         EditorOutputJumpToPosition(pos, length);
                    }
                }
                });
      
          
           
            dataGridViewBuildOutput = new DataGridView();
            dataGridViewBuildOutput.DataBindingComplete += (s, e) =>
            {
                dataGridViewBuildOutput.Columns["Page"].Width = 100;
                dataGridViewBuildOutput.Columns["Class"].Width = 100;
                dataGridViewBuildOutput.Columns["Position"].Width = 0;
                dataGridViewBuildOutput.Columns["Length"].Width = 0;
                dataGridViewBuildOutput.Columns["Type"].Width = 80;
                dataGridViewBuildOutput.Columns["Node"].Width = 0;

                dataGridViewBuildOutput.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            };
            dataGridViewBuildOutput.AllowUserToResizeColumns = false;

            dataGridViewBuildOutput.DataSource = _diagnostic.BuildOutput;
            dataGridViewBuildOutput.AllowUserToResizeColumns = false;
            dataGridViewBuildOutput.AllowUserToAddRows = false;
            dataGridViewBuildOutput.RowHeadersVisible = false;
            dataGridViewBuildOutput.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewBuildOutput.MultiSelect = false;
            dataGridViewBuildOutput.ReadOnly = true;
            dataGridViewBuildOutput.BackgroundColor = Color.White;
            dataGridViewBuildOutput.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridViewBuildOutput.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewBuildOutput.ColumnHeadersVisible = false;
            dataGridViewBuildOutput.RowHeadersVisible = false;
            dataGridViewBuildOutput.Dock = DockStyle.Fill;
            dataGridViewBuildOutput.AllowUserToAddRows = false;
            dataGridViewBuildOutput.AllowUserToDeleteRows = false;
            dataGridViewBuildOutput.AllowUserToOrderColumns = true;
            dataGridViewBuildOutput.AllowUserToResizeColumns = false;
            dataGridViewBuildOutput.BackgroundColor = System.Drawing.Color.LightGray;
            dataGridViewBuildOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewBuildOutput.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dataGridViewBuildOutput.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewBuildOutput.ColumnHeadersVisible = false;
            dataGridViewBuildOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            dataGridViewBuildOutput.Location = new System.Drawing.Point(46, 25);
            dataGridViewBuildOutput.Margin = new System.Windows.Forms.Padding(0);
            dataGridViewBuildOutput.Name = "dataGridOutput";
            dataGridViewBuildOutput.Size = new System.Drawing.Size(832, 115);
            dataGridViewBuildOutput.TabIndex = 0;
            dataGridViewBuildOutput.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridViewBuildOutput.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewBuildOutput.AllowUserToAddRows = false;
            dataGridViewBuildOutput.CellFormatting += (s, e) =>
            {
                if (dataGridViewBuildOutput.Columns[e.ColumnIndex].Name == "Type")
                {
                    string severity = e.Value?.ToString();
                    switch (severity)
                    {
                        case "Error":
                            dataGridViewBuildOutput.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Tomato;
                            break;
                        case "Warning":
                            dataGridViewBuildOutput.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                            break;
                        case "Info":
                            dataGridViewBuildOutput.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightBlue;
                            break;
                    }
                }
            };
            dataGridViewBuildOutput.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    EditorNode node = (EditorNode)dataGridViewBuildOutput.Rows[e.RowIndex].Cells["Node"].Value;
                    OpenNodeSafe(node);
                    btnEditorOutput.PerformClick();
                }
            };

            return;

        }
        public void TableOutputClear()
        {
            if (tblEditorOutputs == null) return;
            tblEditorOutputs.Clear();
        }
        public void TableBuildClear()
        {
            if (tblBuildOutputs == null) return;
            tblBuildOutputs.Clear();
        }

        public void TableFindReplaceClear()
        {
            if (tblFindReplaceOutputs == null) return;
            tblFindReplaceOutputs.Clear();
        }
        public void TableOutputAdd(int n, string position, int length, string type, string description)
        {
            if (tblEditorOutputs == null) return;
            tblEditorOutputs.Rows.Add(n, position, length, type, description);
        }

        internal void TableBuildAdd(string page, string className, string position, int length, string type, string description, EditorNode node)
        {
            Debug.WriteLine(description);
            if (tblBuildOutputs == null) return;
            tblBuildOutputs.Rows.Add(page, className, position, length, type, description, node);
        }

        internal void EditorOutputJumpToPosition(string pos, int length)
        {
            int p = Convert.ToInt32(pos);

            int lineNumber = Editor.LineFromPosition(p);
            int col = Editor.GetColumn(p);
            Debug.WriteLine("Column number " + col);
            int lineStartPos = Editor.Lines[lineNumber].Position;
            Editor.GotoPosition(lineStartPos);

            Editor.SelectionStart = p;
            Editor.SelectionEnd = p + length;
        }
        internal async Task TableBuildJumpToPosition(string pos, int length, EditorNode node)
        {
            if (node.Name != SelectedNode.Name)
            {
                if (Editor.InvokeRequired)
                {
                    Editor.Invoke(new Action(() =>
                    {
                        OpenNodeSafe(node); // Fire-and-forget
                        int p = int.Parse(pos);
                        int lineNumber = Editor.LineFromPosition(p);
                        int col = Editor.GetColumn(p);
                        int lineStartPos = Editor.Lines[lineNumber].Position;
                        Editor.GotoPosition(lineStartPos);
                        Editor.SelectionStart = p;
                        Editor.SelectionEnd = p + length;
                    }));
                }
                else
                {
                    await OpenNode(node);
                    int p = int.Parse(pos);
                    int lineNumber = Editor.LineFromPosition(p);
                    int col = Editor.GetColumn(p);
                    int lineStartPos = Editor.Lines[lineNumber].Position;
                    Editor.GotoPosition(lineStartPos);
                    Editor.SelectionStart = p;
                    Editor.SelectionEnd = p + length;
                }
            }

  
        }

        private async void OpenNodeSafe(EditorNode node)
        {
            await OpenNode(node);
        }

        internal async void TableFindReplaceJumpToPosition(int pos, int length, EditorNode node)
        {
            await OpenNode(node);
            SelectedNode.ForeColor = _currentTheme == EditorTheme.Dark ? Color.Plum : Color.Purple;
            int p = pos;
            int lineNumber = Editor.LineFromPosition(p);
            int col = Editor.GetColumn(p);
            //   Debug.WriteLine("Column number " + col);
            int lineStartPos = Editor.Lines[lineNumber].Position;
            Editor.GotoPosition(lineStartPos);
            Editor.SelectionStart = p;
            Editor.SelectionEnd = p + length;
        }

        public void JumpToDataGridViewFindReplace(int next)
        {
            int index = dataGridViewFindReplace.CurrentRow?.Index ?? -1;

            Debug.WriteLine("SelectedRow = " + index);

            index += next;

            if (index < 0 || index >= dataGridViewFindReplace.Rows.Count) return;

            dataGridViewFindReplace.Rows[index].Selected = true;
            int pos = (int)dataGridViewFindReplace.Rows[index].Cells["Position"].Value;
            int length = Convert.ToInt32(dataGridViewFindReplace.Rows[index].Cells["Length"].Value);
            EditorNode node = (EditorNode)dataGridViewFindReplace.Rows[index].Cells["Node"].Value;
            TableFindReplaceJumpToPosition(pos, length, node);
        }


        public List<string> ExtractIncludes(string source)
        {

            Debug.WriteLine("===== ExtractIncludes ======");
            Debug.WriteLine(source);
            if (string.IsNullOrWhiteSpace(source)) return new List<string>();
            var result = new List<string>();
            var regex = new Regex(@"//\+include\s+(\w+)", RegexOptions.Compiled);

            foreach (Match match in regex.Matches(source))
            {
                result.Add(match.Groups[1].Value);
            }
            Debug.WriteLine("=====  ======");
            return result;
        }
        public void SaveCode()
        {

            foreach (EditorNode node in RootNode.Nodes)
            {
                node.Save();
                foreach (EditorNode subnode in node.Nodes)
                {
                    Debug.WriteLine("Save " + subnode.Text);
                    subnode.Save();
                }
            }
            qbook.Core.ThisBook.Serialize();
            qbook.Core.ThisBook.Modified = false;
        }

        EditorNode lastNode;

        internal async Task OpenNode(EditorNode node)
        {
            if (SelectedNode != null)
            {
                SelectedNode.LastCursorPos = Editor.CurrentPosition;
                SelectedNode.LastFirstLine = Editor.FirstVisibleLine;
                SelectedNode.ForeColor = ProjectTree.ForeColor;
            }


            SelectedNode = node;
            _currentFile = node.FilePath;

            if (lastNode != SelectedNode)
            {
                lastNode = SelectedNode;
            }


            if (SelectedNode.Tab == null)
            {
                SelectedNode.Tab = new EditorTab(SelectedNode, this);
            }
            bool newTab = true;
            foreach (EditorTab tab in PanelTabs.Controls)
            {
                if (tab.Name == node.Name) newTab = false;
            }
            if (newTab)
            {
                PanelTabs.Controls.Add(SelectedNode.Tab);
            }



            UpdateTabs();

            if (node.Active)
                Editor.Text = await node.GetCodeAsync("FormEditor.OpenNode");
            else
                Editor.Text = node.Code;

            if (node.LastCursorPos >= 0)
            {
                Editor.FirstVisibleLine = node.LastFirstLine;
                Editor.GotoPosition(node.LastCursorPos);
            }

            if (!SelectedNode.hasUndo) SelectedNode.AddUndo(Editor.Text, 1, 1);



            SelectedNode.ForeColor = _currentTheme == EditorTheme.Dark ? Color.Plum : Color.Purple;
          //  _foldingControl.Initialize();

            RoslynFoldingHelper.ApplyFolding(Editor);

            _ = RefreshEditor("OpenNode");
       
            ProjectTree.SelectedNode = node;
            node.EnsureVisible();
            node.Expand();
          
        }
        public void SelectPageNodeByName(string pageName)
        {
            ProjectTree.SelectedNode = null;
            foreach (EditorNode node in ProjectTree.Nodes[0].Nodes)
            {
                if (node.Text.EndsWith(pageName))
                {
                    ProjectTree.SelectedNode = node;
                    node.EnsureVisible();
                    node.Expand();
                    OpenNode(node);
                    UpdateTabs();
                    break;


                }
            }
        }



        public void SelectNodeByName(string NodeName)
        {
            //     Debug.WriteLine($"========== Looking for Node '{NodeName}' ==========");
            ProjectTree.SelectedNode = null;
            foreach (EditorNode node in ProjectTree.Nodes[0].Nodes)
            {
                if (node.Text.EndsWith(NodeName))
                {
                    ProjectTree.SelectedNode = node;
                    node.EnsureVisible();
                    node.Expand();
                    OpenNode(node);
                    UpdateTabs();
                    break;
                }

                foreach (EditorNode subnode in node.Nodes)
                {
                    //    Debug.WriteLine(subnode.Name);
                    if (subnode.Name.EndsWith(NodeName))
                    {
                        ProjectTree.SelectedNode = subnode;
                        subnode.EnsureVisible();
                        subnode.Expand();
                        OpenNode(subnode);
                        UpdateTabs();
                        break;
                    }
                }
            }
        }

        void RenameSubCode(oPage page, string oldKey, string newKey)
        {
            if (page.CsCodeExtra.ContainsKey(newKey))
            {
                MessageBox.Show("A custom code with this name already exists. Please choose a different name.");
                return;
            }

            var list = page.CsCodeExtra.ToList();
            list.Reverse();
            SerializableDictionary<string, string> dict = new SerializableDictionary<string, string>();
            foreach (var code in list)
            {
                if (code.Key != oldKey)
                    dict[code.Key] = code.Value;
                else
                    dict[newKey] = code.Value;
            }

            page.CsCodeExtra = dict;
        }
        private async Task DoRun()
        {
            if (qbook.Core.csScript == null)
            {
                try
                {
                    await qbook.Core.CsScriptRebuildAll(); //BuildCsScriptAll();
                }
                catch (Exception ex)
                {
                    SetStatusText("#ERR: rebuilding... (see Output)", Color.Red);
                    return;
                }
            }

            try
            {
                SetStatusText("running...", Color.White);


                var buildRunTrialPath = Path.Combine(qbook.Core.ThisBook.Directory, qbook.Core.ThisBook.Filename) + "~buildrun~";
                if (File.Exists(buildRunTrialPath))
                    File.Delete(buildRunTrialPath);

                qbook.Core.RunCsScript_Run();

            }
            catch (Exception ex)
            {
                SetStatusText("#ERR running... (see Output)", Color.Red);
            }
        }

        void SetStatusText(string text, Color? color = null, Color? backColor = null)
        {
            if (color == null)
                color = Color.Black;
            if (backColor == null)
                backColor = SystemColors.Control;

            labelStatus.BeginInvoke((System.Action)(() =>
            {
                labelStatus.Text = text;
            }));
        }


        //========================= UI Events =========================

        private void btnFind_Click(object sender, EventArgs e)
        {
            ShowSearchBar();
            btnShowFindReplaceOutput.PerformClick();
        }
        private void btnFindReplace_Click(object sender, EventArgs e)
        {
            ShowReplaceBar();
            btnShowFindReplaceOutput.PerformClick();
        }
        private void btnSnippets_Click(object sender, EventArgs e)
        {

        }
        private void btnParagraph_Click(object sender, EventArgs e)
        {
            if (Editor.ViewEol)
            {
                Editor.ViewEol = false;         // Zeigt die EOL-Zeichen visuell an
                Editor.ViewWhitespace = WhitespaceMode.Invisible;

            }
            else
            {
                Editor.EolMode = Eol.CrLf;
                Editor.ViewEol = true;         // Zeigt die EOL-Zeichen visuell an
                Editor.ViewWhitespace = WhitespaceMode.VisibleAlways;
            }

        }
        private async void btnFormat_Click(object sender, EventArgs e)
        {
            await FormatDocumentAsync();
        }
        private async void btnRebuild_Click(object sender, EventArgs e)
        {
            await HandleRebuildAsync();

        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveCode();
        }
        private async void btnReload_Click(object sender, EventArgs e)
        {

            await Reload();

        }
        private void btnToggleTheme_Click(object sender, EventArgs e)
        {
            ToggleTheme();
        }
        private async void btnRun_Click(object sender, EventArgs e)
        {
            await DoRun();
        }
        private async void renameCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorNode page = SelectedNode.PageNode;
            EditorNode code = SelectedNode;

            string pageName = Regex.Replace(page.Text, @"^[^_]*_", "");
            string codeName = Regex.Replace(code.Text, @"^[^_]*_", "").Replace(".cs", "");

            oPage edit = FindPageByName(pageName);

            string newName = codeName;
            if (!UiExtentions.ShowEditTextDialog(ref newName, "New name of the custom code:"))
                return;

            RenameSubCode(edit, codeName, newName);
            code.Text = $"{newName}";

            await LoadBook();
        }
        private void btnEditorOutput_Click(object sender, EventArgs e)
        {
            if (this.TablePanelOutputs.Controls.Contains(this.dataGridViewBuildOutput)) ;
            this.TablePanelOutputs.Controls.Remove(this.dataGridViewBuildOutput);

            if (this.TablePanelOutputs.Controls.Contains(this.dataGridViewFindReplace)) ;
            this.TablePanelOutputs.Controls.Remove(this.dataGridViewFindReplace);


            this.TablePanelOutputs.Controls.Add(this.GridViewDiagnosticOutput, 1, 2);
            vBarOutputs.Init(this.GridViewDiagnosticOutput);
            UpdateOutputButtons();

        }
        private void btnBuildOutput_Click(object sender, EventArgs e)
        {
            if (this.TablePanelOutputs.Controls.Contains(this.GridViewDiagnosticOutput)) ;
            this.TablePanelOutputs.Controls.Remove(this.GridViewDiagnosticOutput);

            if (this.TablePanelOutputs.Controls.Contains(this.dataGridViewFindReplace)) ;
            this.TablePanelOutputs.Controls.Remove(this.dataGridViewFindReplace);

            this.TablePanelOutputs.Controls.Add(this.dataGridViewBuildOutput, 1, 2);
            vBarOutputs.Init(this.dataGridViewBuildOutput);
            UpdateOutputButtons();
        }

        private void btnFindReplaceOutput_Click(object sender, EventArgs e)
        {
            if (this.TablePanelOutputs.Controls.Contains(this.GridViewDiagnosticOutput)) ;
            this.TablePanelOutputs.Controls.Remove(this.GridViewDiagnosticOutput);

            if (this.TablePanelOutputs.Controls.Contains(this.dataGridViewBuildOutput)) ;
            this.TablePanelOutputs.Controls.Remove(this.dataGridViewBuildOutput);

            this.TablePanelOutputs.Controls.Add(this.dataGridViewFindReplace, 1, 2);
            vBarOutputs.Init(this.dataGridViewFindReplace);
            UpdateOutputButtons();
        }

        private void ProjectTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ProjectTree.SelectedNode = null;
        }
        private void hidePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            oPage page = FindPageByName(Regex.Replace(SelectedNode.Text, @"^[^_]*_", ""));

            page.Hidden = !page.Hidden;
            SelectedNode.ImageIndex = page.Hidden ? 4 : 2;
        }
        private void hidePageToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {

            //if (_lastClickedNode != null && _lastClickedNode.Tag is qbook.oPage)
            //    (_lastClickedNode.Tag as qbook.oPage).Hidden = (sender as ToolStripMenuItem).Checked;
            //UpdatePageTreeView(treeViewCodePages.Nodes);
        }
        private void toolStripMenuOpenWorkspace_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", RootNode.FilePath);

        }
        private void FormEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
        private async void addPageBeforeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewPageBelowSelectedPage(true);
            await LoadBook();
        }
        private void AddNewPageBelowSelectedPage(bool beforeSelectedPage = false)
        {
            qbook.Core.ThisBook.Modified = true;
            //qbook.Core.ActualMain = (sender as oIcon).Parent;

            string newName = "newPage";


        _AddPageStart:

            if (!UiExtentions.ShowEditTextDialog(ref newName, "Page name:"))
                return;


            bool pageNameExists = false;
            foreach (oPage page in qbook.Core.ActualMain.Objects.OfType<oPage>())
            {
                string newFullName = null;
                int index = page.FullName.LastIndexOf(".");
                if (index == -1)
                    newFullName = newName.Trim();
                else
                    newFullName = page.FullName.Substring(0, index) + newName.Trim();
                if (page.FullName == newFullName)
                {
                    pageNameExists = true;
                    break;
                }
            }
            if (pageNameExists)
            {
                MessageBox.Show($"A page with the name '{newName}' already exists.\r\nPlease choose a different name."
                    , "PAGE NAME EXISTS", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                goto _AddPageStart;
            }

            oPage newPage = new oPage(newName, newName);

            //TODO: insert after selected page/node
            //int indx = (sender as oIcon).Parent.Objects.IndexOf(qbook.Core.SelectedPage);
            int indx = 0;
            if (SelectedPage == null)
                indx = 0;
            else
                indx = qbook.Core.ThisBook.Main.Objects.OfType<oPage>().ToList().IndexOf(SelectedPage);

            if (beforeSelectedPage)
                indx = indx + 0;
            else
                indx = indx + 1;

            qbook.Core.ThisBook.Main.Objects.Insert(indx, newPage);
            qbook.Core.SelectedPage = newPage;

            //   Main.Qb.SelectedLayer.Add(newItem);
            qbook.Core.ThisBook.Modified = true;
            //PopulatePageTreeView(false, true);

        }
        private async void addPageAfterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewPageBelowSelectedPage(false);
            await LoadBook();
        }
        private async void customToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode page = SelectedNode;
            //     Debug.WriteLine("Customizing page " + page.Text);
            string pageName = Regex.Replace(page.Text, @"^[^_]*_", ""); ;
            oPage edit = FindPageByName(pageName);

            string title = "CustomCode";

            if (!UiExtentions.ShowEditTextDialog(ref title, "Name of the custom code:"))
                return;

            //        Debug.WriteLine("Add costum code: " + title);


            if (!edit.CsCodeExtra.ContainsKey(title))
            {
                edit.CsCodeExtra[title] = "";
                await LoadBook();
            }
            else
            {
                MessageBox.Show("A custom code with this name already exists. Please choose a different name.");
            }
        }
        private async void deleteStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode page = SelectedNode.Parent;
            TreeNode code = SelectedNode;

            string pageName = Regex.Replace(page.Text, @"^[^_]*_", "");
            string codeName = Regex.Replace(code.Text, @"^[^_]*_", "").Replace(".cs", "");

            oPage edit = FindPageByName(pageName);

            if (edit.CsCodeExtra.ContainsKey(codeName))
            {

                DialogResult result = MessageBox.Show(
                    $"Delete {codeName}?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    edit.CsCodeExtra.Remove(codeName);
                    ProjectTree.Nodes.Remove(code);
                    await LoadBook();
                }

            }
            else
            {
                MessageBox.Show($"A custom code '{codeName}' with this name not found");
            }

        }

        //========================= Theme =============================

        public void UpdateTabs()
        {
            foreach (EditorTab tab in PanelTabs.Controls)
            {
                tab.BackColor = tab.Name == SelectedNode.Name ? Editor.Styles[Style.Default].BackColor : ProjectTree.BackColor;
                tab.ForeColor = ProjectTree.ForeColor;
            }
        }
        void UpdateOutputButtons()
        {
            bool isBuild = this.TablePanelOutputs.Controls.Contains(this.dataGridViewBuildOutput);
            bool isEditor = this.TablePanelOutputs.Controls.Contains(this.GridViewDiagnosticOutput);
            bool isFindReplace = this.TablePanelOutputs.Controls.Contains(this.dataGridViewFindReplace);

            btnBuildOutput.ForeColor = ProjectTree.ForeColor;
            btnEditorOutput.ForeColor = ProjectTree.ForeColor;
            btnShowFindReplaceOutput.ForeColor = ProjectTree.ForeColor;

            if (_currentTheme == EditorTheme.Light)
            {

                Color sel = Color.FromArgb(220, 220, 220);
                Color usel = Color.FromArgb(190, 190, 190);
                btnEditorOutput.BackColor = isEditor ? sel : usel;
                btnBuildOutput.BackColor = isBuild ? sel : usel;
                btnShowFindReplaceOutput.BackColor = isFindReplace ? sel : usel;
            }

            if (_currentTheme == EditorTheme.Dark)
            {
                Color sel = Color.FromArgb(70, 70, 70);
                Color usel = Color.FromArgb(50, 50, 50);
                btnEditorOutput.BackColor = isEditor ? sel : usel;
                btnBuildOutput.BackColor = isBuild ? sel : usel;
                btnShowFindReplaceOutput.BackColor = isFindReplace ? sel : usel;
            }
        }
        Color ButtonForeColor = Color.Transparent;

        public Font GetFont()
        {
            var style = Editor.Styles[Style.Default];
            string fontName = style.Font;
            int baseSize = style.Size;
            int zoom = Editor.Zoom;
            int effectiveSize = baseSize + zoom;

            return new Font(fontName, effectiveSize);
        }


        private void ApplyLightTheme()
        {
            _currentTheme = EditorTheme.Light;

            //Background

            Color _backColor = Color.FromArgb(230, 230, 230);

            panelSplttter1.BackColor = Color.FromArgb(180, 180, 180);
            panelSplitter2.BackColor = Color.FromArgb(180, 180, 180);
            panelSplitter3.BackColor = Color.FromArgb(180, 180, 180);
            panelSplitter4.BackColor = Color.FromArgb(180, 180, 180);

            vBarEditor.SetBackColor = _backColor;
            vBarEditor.SetForeColor = Color.FromArgb(180, 180, 180);

            vBarMethodes.SetBackColor = _backColor;
            vBarMethodes.SetForeColor = Color.FromArgb(180, 180, 180);

            hBarEditor.SetBackColor = _backColor;
            hBarEditor.SetForeColor = Color.FromArgb(180, 180, 180);


            DwmTitleBar.SetImmersiveDarkMode(this.Handle, enabled: false);
            tableLayoutPanel1.BackColor = _backColor;
            TablePanelOutputs.BackColor = _backColor;
            Editor.BorderStyle = ScintillaNET.BorderStyle.None;

            //ProjectTree
            ProjectTree.BackColor = _backColor;
            ProjectTree.ForeColor = Color.Black;
            ProjectTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            UpdateAllNodes(ProjectTree, ProjectTree.ForeColor, 2, 2);
            //UpdateAllNodes(ProjectTree, ProjectTree.ForeColor, 2, 3);
            //UpdateAllNodes(ProjectTree, ProjectTree.ForeColor, 3, 4);
            ProjectTree.SelectedImageIndex = 10;
            if (SelectedNode != null)
                SelectedNode.ForeColor = Color.Purple;

            //Methodes
            lblMethodes.BackColor = _backColor;
            lblMethodes.ForeColor = ProjectTree.ForeColor;
            gridViewMethodes.BackgroundColor = _backColor;
            gridViewMethodes.BackColor = _backColor;
            gridViewMethodes.RowsDefaultCellStyle.BackColor = _backColor;
            gridViewMethodes.RowsDefaultCellStyle.ForeColor = ProjectTree.ForeColor;


            vBarMethodes.SetBackColor = _backColor;
            vBarMethodes.SetForeColor = Color.FromArgb(180, 180, 180);


            vBarProjectTree.SetBackColor = _backColor;
            vBarProjectTree.SetForeColor = Color.FromArgb(70, 70, 70);


            //Status
            labelStatus.BackColor = _backColor;
            labelStatus.ForeColor = ProjectTree.ForeColor;


            if (ButtonForeColor == Color.Transparent) ButtonForeColor = Color.Black;


            //PanelControl Top
            _backColor = Color.FromArgb(220, 220, 220);
            panelControl.BackColor = _backColor;

            foreach (System.Windows.Forms.Button b in panelControl.Controls)
            {
                b.BackColor = _backColor;
                //  b.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                b.ForeColor = Color.Black;
                b.FlatAppearance.BorderColor = _backColor;
                Bitmap pic = b.Image as Bitmap;
                pic = BitmapTools.ReplaceColor(pic, Color.Black, Color.FromArgb(120, 120, 120), 100); //Init Icon Color
                pic = BitmapTools.ReplaceColor(pic, Color.FromArgb(150, 150, 150), Color.FromArgb(100, 100, 100), 100);
                pic = BitmapTools.ResizeExact(pic, 28, 28);
                var old = b.Image;
                b.Image = pic;
                old?.Dispose(); ;
            }



            //Output
            UpdateOutputButtons();

            //Buttons Editor Left
            _backColor = Color.FromArgb(230, 230, 230);
            foreach (System.Windows.Forms.Button b in panelFunctions.Controls)
            {
                b.BackColor = tableLayoutPanel1.BackColor;
                b.Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
                b.ForeColor = System.Drawing.Color.Black;
                b.FlatAppearance.BorderColor = _backColor;
                Bitmap pic = b.Image as Bitmap;
                //  pic = BitmapTools.ReplaceColor(pic, Color.White, Color.Black,100);
                pic = BitmapTools.ResizeExact(pic, 32, 32);
                var old = b.Image;
                b.Image = pic;
                old?.Dispose();
            }

            ButtonForeColor = Color.Black;
            GridViewDiagnosticOutput.BackgroundColor = Color.FromArgb(220, 220, 220);
            dataGridViewBuildOutput.BackgroundColor = Color.FromArgb(220, 220, 220);

            dataGridViewFindReplace.BackgroundColor = Color.FromArgb(220, 220, 220);
            dataGridViewFindReplace.ForeColor = Color.Black;

            _currentTheme = EditorTheme.Light;
            // Grundstil (Visual Studio Light / VS Code Light+ nahe)
            Editor.StyleResetDefault();
            Editor.Styles[Style.Default].Font = "Cascadia Code";
            Editor.Styles[Style.Default].Size = 10;
            Editor.Styles[Style.Default].BackColor = Color.White;                // Editor Hintergrund
            Editor.Styles[Style.Default].ForeColor = Color.Black;                // Standard Text
            Editor.StyleClearAll();
            Editor.CaretForeColor = Color.Black;
            Editor.SetSelectionBackColor(true, Color.FromArgb(0xD7, 0xE4, 0xF2)); // Standard VS Light Selektionsblau ähnlich
            Editor.WhitespaceSize = 2;
            Editor.ViewWhitespace = WhitespaceMode.Invisible;

            // Brace Highlight (angepasst an VS Light – leicht gelblicher Hintergrund)
            Editor.Styles[Style.BraceLight].ForeColor = Color.Black;
            Editor.Styles[Style.BraceLight].BackColor = Color.FromArgb(0xFF, 0xF4, 0xC1);
            Editor.Styles[Style.BraceBad].ForeColor = Color.White;
            Editor.Styles[Style.BraceBad].BackColor = Color.FromArgb(0xE5, 0x51, 0x51);

            // Folding Marker Farben (neutral grau wie VS)
            var foldFore = Color.FromArgb(0x80, 0x80, 0x80);
            var foldBack = foldFore;
            for (int i = Marker.FolderEnd; i <= Marker.FolderOpenMid; i++)
            {
                Editor.Markers[i].SetForeColor(foldFore);
                Editor.Markers[i].SetBackColor(foldBack);
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
            Editor.Indicators[0].ForeColor = Color.Red;
            // Methoden
            Editor.Indicators[2].ForeColor = vsMethod;
            // Klassen
            Editor.Indicators[3].ForeColor = vsType;
            // Interfaces
            Editor.Indicators[4].ForeColor = vsType;
            // Structs
            Editor.Indicators[5].ForeColor = vsType;
            // Enums
            Editor.Indicators[6].ForeColor = vsType;
            // Delegates (wie Methoden leicht abheben)
            Editor.Indicators[7].ForeColor = vsMethod;
            // Properties
            Editor.Indicators[8].ForeColor = vsProperty;
            // Fields
            Editor.Indicators[9].ForeColor = Color.Black;
            // Keywords
            Editor.Indicators[10].ForeColor = vsKeyword;
            // Numbers
            Editor.Indicators[11].ForeColor = vsNumber;
            // Strings
            Editor.Indicators[12].ForeColor = vsString;
            // Comments
            Editor.Indicators[13].ForeColor = vsComment;

            //Commands
            Editor.Indicators[14].ForeColor = Commands;

            //NameSpace
            Editor.Indicators[15].ForeColor = NameSpaceName;

            // Caret Line
            Editor.CaretLineVisible = true;
            Editor.CaretLineBackColor = Color.FromArgb(0xF3, 0xF9, 0xFF);

            // Line Numbers / Folding Ränder
            Editor.Styles[Style.LineNumber].BackColor = Color.White;
            Editor.SetFoldMarginColor(true, Color.White);
            Editor.SetFoldMarginHighlightColor(true, Color.White);


            //Error Style
            int STYLE_ERROR = 20;
            Editor.Styles[STYLE_ERROR].BackColor = Color.Red;
            Editor.Styles[STYLE_ERROR].ForeColor = Color.White; // optional

            // Autocomplete Farben
            Editor.AutocompleteListBackColor = Color.White;
            Editor.AutocompleteListTextColor = Color.Black;

            UpdateTabs();
        }
        private void ApplyDarkTheme()
        {
            _currentTheme = EditorTheme.Dark;
            Color _backColor = Color.FromArgb(40, 40, 40);

            panelSplttter1.BackColor = Color.FromArgb(70, 70, 70);
            panelSplitter2.BackColor = Color.FromArgb(70, 70, 70);
            panelSplitter3.BackColor = Color.FromArgb(70, 70, 70);
            panelSplitter4.BackColor = Color.FromArgb(70, 70, 70);


            //Background
            DwmTitleBar.SetImmersiveDarkMode(this.Handle, enabled: true);

            tableLayoutPanel1.BackColor = _backColor;
            TablePanelOutputs.BackColor = _backColor;
            Editor.BorderStyle = ScintillaNET.BorderStyle.None;

            vBarEditor.SetBackColor = _backColor;
            vBarEditor.SetForeColor = Color.FromArgb(70, 70, 70);
            vBarMethodes.SetBackColor = _backColor;
            vBarMethodes.SetForeColor = Color.FromArgb(70, 70, 70);
            hBarEditor.SetBackColor = _backColor;
            hBarEditor.SetForeColor = Color.FromArgb(70, 70, 70);

            vBarProjectTree.SetBackColor = _backColor;
            vBarProjectTree.SetForeColor = Color.FromArgb(70, 70, 70);

            //ProjectTree
            ProjectTree.BackColor = _backColor;
            ProjectTree.ForeColor = Color.FromArgb(190, 190, 190);
            ProjectTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            ProjectTree.SelectedImageIndex = 10;

            UpdateAllNodes(ProjectTree, ProjectTree.ForeColor, 1, 1);
            //UpdateAllNodes(ProjectTree, ProjectTree.ForeColor, 2, 3);
            //UpdateAllNodes(ProjectTree, ProjectTree.ForeColor, 3, 4);

            //Methodes
            lblMethodes.BackColor = _backColor;
            lblMethodes.ForeColor = ProjectTree.ForeColor;
            gridViewMethodes.BackgroundColor = _backColor;
            gridViewMethodes.BackColor = _backColor;
            gridViewMethodes.RowsDefaultCellStyle.BackColor = _backColor;
            gridViewMethodes.RowsDefaultCellStyle.ForeColor = ProjectTree.ForeColor;

            if (SelectedNode != null)
                SelectedNode.ForeColor = Color.Plum;

            //Status
            labelStatus.BackColor = _backColor;
            labelStatus.ForeColor = ProjectTree.ForeColor;


            //PanelControl
            _backColor = Color.FromArgb(60, 60, 60);
            panelControl.BackColor = _backColor;

            foreach (System.Windows.Forms.Button b in panelControl.Controls)
            {
                b.BackColor = _backColor;

                b.ForeColor = Color.FromArgb(200, 200, 200);
                b.FlatAppearance.BorderColor = _backColor;
                Bitmap pic = b.Image as Bitmap;
                pic = BitmapTools.ReplaceColor(pic, Color.Black, Color.FromArgb(190, 190, 190), 100);
                //  pic = BitmapTools.ResizeExact(pic, 28, 28);
                var old = b.Image;
                b.Image = pic;
                old?.Dispose();
            }


            //Buttons
            _backColor = Color.FromArgb(40, 40, 40);
            if (ButtonForeColor == Color.Transparent) ButtonForeColor = Color.White;

            foreach (System.Windows.Forms.Button b in panelFunctions.Controls)
            {
                b.BackColor = _backColor;
                b.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                b.ForeColor = Color.Black;
                b.FlatAppearance.BorderColor = _backColor;
                Bitmap pic = b.Image as Bitmap;
                pic = BitmapTools.ReplaceColor(pic, Color.Black, Color.FromArgb(120, 120, 120), 100); //Init Icon Color
                pic = BitmapTools.ReplaceColor(pic, ButtonForeColor, Color.FromArgb(120, 120, 120));
                //    pic = BitmapTools.ResizeExact(pic, 32, 32);
                var old = b.Image;
                b.Image = pic;
                old?.Dispose();
            }
            ButtonForeColor = Color.FromArgb(120, 120, 120);

            //Outputs
            UpdateOutputButtons();
            GridViewDiagnosticOutput.BackgroundColor = Color.FromArgb(70, 70, 70);
            GridViewDiagnosticOutput.ForeColor = Color.Black;

            dataGridViewBuildOutput.BackgroundColor = Color.FromArgb(70, 70, 70);
            dataGridViewBuildOutput.ForeColor = Color.Black;

            dataGridViewFindReplace.BackgroundColor = Color.FromArgb(70, 70, 70);
            dataGridViewFindReplace.ForeColor = Color.Black;

            //



            _currentTheme = EditorTheme.Dark;
            // Grundstil (Visual Studio Dark / VS Code Dark+)
            Editor.StyleResetDefault();
            Editor.Styles[Style.Default].Font = "Cascadia Code";
            Editor.Styles[Style.Default].Size = 10;
            Editor.Styles[Style.Default].BackColor = Color.FromArgb(25, 25, 25);          // #1E1E1E
            Editor.Styles[Style.Default].ForeColor = Color.FromArgb(200, 200, 200);          // Standard Text
            Editor.StyleClearAll();
            Editor.CaretForeColor = Color.White;
            Editor.SetSelectionBackColor(true, Color.FromArgb(0x26, 0x4F, 0x78));               // Auswahlblau dunkel
            Editor.WhitespaceSize = 2;
            Editor.ViewWhitespace = WhitespaceMode.Invisible;

            // Brace Highlight (VS Dark ähnlich)
            Editor.Styles[Style.BraceLight].ForeColor = Color.FromArgb(0xFF, 0xC0, 0x40);
            Editor.Styles[Style.BraceLight].BackColor = Color.FromArgb(0x33, 0x33, 0x33);
            Editor.Styles[Style.BraceBad].ForeColor = Color.White;
            Editor.Styles[Style.BraceBad].BackColor = Color.FromArgb(0x90, 0x2B, 0x2B);

            // Folding Marker (leicht kontrastreich)
            var foldFore = Color.FromArgb(0xAA, 0xAA, 0xAA);
            var foldBack = Color.FromArgb(0x2D, 0x2D, 0x2D);
            for (int i = Marker.FolderEnd; i <= Marker.FolderOpenMid; i++)
            {
                Editor.Markers[i].SetForeColor(foldFore);
                Editor.Markers[i].SetBackColor(foldBack);
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
            Editor.Indicators[0].ForeColor = Color.FromArgb(0xF4, 0x43, 0x36);
            // Methoden
            Editor.Indicators[2].ForeColor = vsDarkMethod;
            // Klassen
            Editor.Indicators[3].ForeColor = vsDarkClass;
            // Interfaces
            Editor.Indicators[4].ForeColor = vsDarkInterface;
            // Structs
            Editor.Indicators[5].ForeColor = vsDarkStruct;
            // Enums
            Editor.Indicators[6].ForeColor = vsDarkEnum;
            // Delegates
            Editor.Indicators[7].ForeColor = vsDarkDelegate;
            // Properties
            Editor.Indicators[8].ForeColor = vsDarkProperty;
            // Fields
            Editor.Indicators[9].ForeColor = vsDarkField;
            // Keywords
            Editor.Indicators[10].ForeColor = vsDarkKeyword;
            // Numbers
            Editor.Indicators[11].ForeColor = vsDarkNumber;
            // Strings
            Editor.Indicators[12].ForeColor = vsDarkString;
            // Comments
            Editor.Indicators[13].ForeColor = vsDarkComment;

            //Commands
            Editor.Indicators[14].ForeColor = Commands;
            //NameSpace
            Editor.Indicators[15].ForeColor = NameSpaceName;

            Editor.CaretLineVisible = true;
            Editor.CaretLineBackColor = Color.FromArgb(0x2A, 0x2A, 0x2A);


            // Line Numbers & Folding
            Editor.Styles[Style.LineNumber].BackColor = Color.FromArgb(25, 25, 25); ;
            Editor.SetFoldMarginColor(true, Color.FromArgb(25, 25, 25));
            Editor.SetFoldMarginHighlightColor(true, Color.FromArgb(25, 25, 25));


            // Autocomplete
            Editor.AutocompleteListBackColor = Color.FromArgb(0x25, 0x25, 0x25);
            Editor.AutocompleteListTextColor = Color.FromArgb(0xD4, 0xD4, 0xD4);

            UpdateTabs();
        }
        public async Task ToggleTheme()
        {
            if (_currentTheme == EditorTheme.Light) ApplyDarkTheme(); else ApplyLightTheme();
            //    RefreshSemanticOverlaysAsync();
           await  RefreshEditor("ToggleTheme");
            if (_findReplaceControl == null) return;
            if (_currentTheme == EditorTheme.Dark) _findReplaceControl.DarkTheme(); else _findReplaceControl.LightTheme();

        }

        private void toolStripMenuIncludeCode_Click(object sender, EventArgs e)
        {
            if (_clickedNode == null) return;

            if (_clickedNode.Active)
            {
                ExcludeCode(_clickedNode);
                _clickedNode.ImageIndex = 5;
            }
            else
            {
                IncludeCode(_clickedNode);
                _clickedNode.ImageIndex = 3;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await RefreshEditor("Button1");
        }

        private async void btnShowHidden_Click(object sender, EventArgs e)
        {

            BeginnUpdate();
            RoslynFoldingHelper.SaveFolding(Editor);

            int firstLine = Editor.FirstVisibleLine;
            int pos = Editor.CurrentPosition;

            string text = Editor.Text;
            Editor.Text = text;
            RoslynFoldingHelper.ApplyFolding(Editor);

            RoslynFoldingHelper.RestoreFolding(Editor);

            Editor.FirstVisibleLine = firstLine;
            Editor.GotoPosition(pos);
            EndUpdate();
            Editor.Refresh();


            return;
     
            
            //Hide usings in subcode <- is mangaged by PageNode
            if (SelectedNode.Type == EditorNode.NodeType.SubCode)
            {
                var (startLine, endLine) = await SelectedNode.GetUsingBlockLineRange();
                Editor.ShowLines(startLine, endLine);
            }
            //Hide includes in pagecode <- is mangaged by Treeview UI
            if (SelectedNode.Type == EditorNode.NodeType.Page)
            {
                var (startLine, endLine) = await SelectedNode.GetIncludeBlockLineRange();
                Editor.ShowLines(startLine, endLine);
            }
        }

        private void FormEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
         
        }

        private void FormEditor_ResizeBegin(object sender, EventArgs e)
        {
 
        }

        private void FormEditor_Resize(object sender, EventArgs e)
        {

        }

        private void FormEditor_Move(object sender, EventArgs e)
        {

        }

        private void Editor_SizeChanged(object sender, EventArgs e)
        {
            if (_findReplaceControl != null)
                _findReplaceControl.Location = new Point(Editor.Width - _findReplaceControl.Width, 0); // Rechts oben
        }
    }

    public class EditorTab : System.Windows.Forms.Button
    {

        public string Name;

        FormEditor Root;

        EditorNode Node;
        internal EditorTab(EditorNode node, FormEditor root)
        {
            Node = node;
            Root = root;

            Name = node.Name;

            string text = node.Text;

            if (node.Type == EditorNode.NodeType.SubCode)
                text = node.PageNode.Text + "." + node.Text;


            BackColor = System.Drawing.Color.LightGray;
            Dock = System.Windows.Forms.DockStyle.Left;
            FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
            FlatAppearance.BorderSize = 0;
            FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            Location = new System.Drawing.Point(87, 0);
            Margin = new System.Windows.Forms.Padding(0);
            Name = node.Name;
            var textSize = System.Windows.Forms.TextRenderer.MeasureText(Name, Font);
            Size = new System.Drawing.Size(textSize.Width + 20, 25);
            TabIndex = 1;
            Text = Name;
            UseVisualStyleBackColor = false;
            Click += new EventHandler((sender, e) => JumpToPosition());


            MouseUp += (sender, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    Remove();
                }
            };


        }

        public void JumpToPosition()
        {
            Root.OpenNode(Node);
            Root.Editor.Focus();

            Root.UpdateTabs();


        }

        public void Remove()
        {
            if (Root.PanelTabs.Controls.Count == 1)
            {
                Root.ResetCurrentFile();
                Root.PanelTabs.Controls.Remove(this);
                Root.ResetNode();
                Root.Editor.Text = "";
                return;
            }
            Root.PanelTabs.Controls.Remove(this);
            int last = Root.PanelTabs.Controls.Count - 1;
            ((EditorTab)Root.PanelTabs.Controls[0]).JumpToPosition();
        }
    }




    internal static class DwmTitleBar
    {
        // DWMWINDOWATTRIBUTE Werte (Windows 11+)
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20; // (19 bei älteren Win10-Insider Builds)
        private const int DWMWA_BORDER_COLOR = 34;
        private const int DWMWA_CAPTION_COLOR = 35;
        private const int DWMWA_TEXT_COLOR = 36;


        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);


        public static bool IsWindows11OrNewer()
        {
            // Windows 11: Build >= 22000
            try { return Environment.OSVersion.Version.Build >= 22000; } catch { return false; }
        }


        public static bool SetImmersiveDarkMode(IntPtr hwnd, bool enabled)
        {
            int v = enabled ? 1 : 0;
            int hr = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref v, sizeof(int));
            return hr >= 0; // S_OK = 0; negative = Fehler
        }


        public static bool SetCaptionColor(IntPtr hwnd, Color color)
        {
            // COLORREF (BGR) – ColorTranslator.ToWin32 liefert korrektes Format
            int colorRef = ColorTranslator.ToWin32(color);
            int hr = DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref colorRef, sizeof(int));
            return hr >= 0;
        }


        public static bool SetTextColor(IntPtr hwnd, Color color)
        {
            int colorRef = ColorTranslator.ToWin32(color);
            int hr = DwmSetWindowAttribute(hwnd, DWMWA_TEXT_COLOR, ref colorRef, sizeof(int));
            return hr >= 0;
        }


        public static bool SetBorderColor(IntPtr hwnd, Color color)
        {
            int colorRef = ColorTranslator.ToWin32(color);
            int hr = DwmSetWindowAttribute(hwnd, DWMWA_BORDER_COLOR, ref colorRef, sizeof(int));
            return hr >= 0;
        }
    }

}
