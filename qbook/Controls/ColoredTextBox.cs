using qbook.Scripting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColoredTextBox
{
    public partial class ColoredTextBoxControl : UserControl
    {
        public bool ShowDebugInfo = false;

        public string _parentObjectName = null;
        public string ParentObjectName
        {
            get => _parentObjectName;
            set => _parentObjectName = value;
        }

        public ColoredTextBoxControl()
        {
            InitializeComponent();

            this.Font = new Font("Consolas", 11);

            this.BackColor = Color.White;
            BackColorBrush = Brushes.White;

            panelText.AutoScroll = false;
            panelText.HorizontalScroll.Enabled = true;
            panelText.HorizontalScroll.Visible = true;
            panelText.HorizontalScroll.Minimum = 0;
            panelText.HorizontalScroll.Maximum = 100;
            panelText.VerticalScroll.Enabled = true;
            panelText.VerticalScroll.Visible = true;
            panelText.VerticalScroll.Minimum = 0;
            panelText.VerticalScroll.Maximum = 100;
            panelText.Scroll += PanelText_Scroll;
            panelText.MouseWheel += ColoredTextBoxControl_MouseWheel;
            panelText.LostFocus += PanelText_LostFocus;
            //panelText.PreviewKeyDown += PanelText_PreviewKeyDown;
            (panelText as Control).KeyDown += ColoredTextBoxControl_KeyDown;
            (panelText as Control).KeyPress += ColoredTextBoxControl_KeyPress;

            this.Enabled = true;

            foreach (var control in panelFind.Controls)
                (control as Control).KeyDown += AnyPanelFindControl_KeyDown;

            TestHighlightList.Add(new HighlightItem()
            {
                Name = "Comment",
                Regex = new Regex(@"(//.*$)", RegexOptions.Compiled),
                Brush = Brushes.SeaGreen,
                FontStyle = FontStyle.Italic,
                StopAfterThisRule = true
            });


            //TestHighlightList.Add(new HighlightItem()
            //{
            //    Name = "Comment1",
            //    Regex = new Regex(@"(/\*.*\*/)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline),
            //    Brush = Brushes.SeaGreen,
            //    FontStyle = FontStyle.Italic,
            //    StopAfterThisRule = true
            //});


            TestHighlightList.Add(new HighlightItem()
            {
                Name = "String",
                Regex = new Regex(@"(""""|"".*?[^\\]"")", RegexOptions.Compiled),
                Brush = Brushes.Brown,
                FontStyle = FontStyle.Italic,
                StopAfterThisRule = true
            });


            List<string> includedFunctions = new List<string>()
            { "sqrt", "pow", "factorial", "abs","sign", "ceil", "floor", "round", "exp", "log",
              "min", "max", "lt","eq","gt", "sin", "cos", "tan", "arcsin", "arccos", "arctan", "isnan", "isint", "isprime", "isinfty",
              "rand", "type", "is", "as", "length", "throw", "catch", "eval", "map", "clamp", "lerp", "regex", "shuffle",

              //keywords
              "true" , "false" , "return" , "var" , "let" , "const" , "for" , "while" , "do" , "module" , "if" , "else" , "break" , "continue" , "yield" , "async" , "await" , "class" , "static" , "new" , "delete" , "pi" , "match"
            };
            List<string> customFunctions = new List<string>()
            {
                "counterAdd", "counterStop", "counterStart", "counterReset", "counterSet", "coounterRemove",
                "sleep", "rxReplace", "replace", "date", "dateUtc", "time", "timeUtc", "epoch", "epochMs", "epochToDateTime",
                "format",
                "canSend", "udlSetState", "udlSetSet",
                "shl", "shr", "ror", "rol", "isBitSet", "setBit", "clearBit",
                "subStr", "padLeft", "padRight", "dialog", "gotoPage",
                "object"
            };
            TestHighlightList.Add(new HighlightItem()
            {
                Name = "Included Functions",
                Regex = new Regex(@"\b(" + string.Join(@"|", includedFunctions) + @")\b", RegexOptions.Compiled),
                Brush = Brushes.Magenta,
                //BackBrush = new SolidBrush(Color.FromArgb(64, 255, 0, 0)),
                FontStyle = FontStyle.Bold
            });
            TestHighlightList.Add(new HighlightItem()
            {
                Name = "Custom Functions",
                Regex = new Regex(@"\b(" + string.Join(@"|", customFunctions) + @")\b", RegexOptions.Compiled),
                Brush = Brushes.Purple,
                FontStyle = FontStyle.Bold
            });

            List<string> knownTypes = new List<string>()
            {
                "alias:", "button:", "chart:", "gauge:"
            };
            TestHighlightList.Add(new HighlightItem()
            {
                Name = "Known Types",
                Regex = new Regex(@"^\s*(" + string.Join(@"|", knownTypes) + @")", RegexOptions.Compiled),
                Brush = Brushes.DodgerBlue,
                FontStyle = FontStyle.Bold
            });


            TestHighlightList.Add(new HighlightItem()
            {
                Name = "@Function",

                Regex = new Regex(@"(^\s*@\d+(s|ms)'[^']*'\s*\=\>)|(^\s*@\d+(s|ms)\s*\=\>)|(^\s*@[_a-zA-Z][_a-zA-Z0-9\.]*:on[_a-zA-Z0-9\.]+\s*\=\>)"),

                Brush = Brushes.Blue,
                FontStyle = FontStyle.Bold
            });
            TestHighlightList.Add(new HighlightItem()
            {
                Name = "Mages Function Definition",
                Regex = new Regex(@"\([_a-zA-Z\s][_a-zA-Z0-9,\s]*\)\s*=>\s*", RegexOptions.Compiled),
                Brush = Brushes.Orange,
                FontStyle = FontStyle.Bold
            });

        }


        private void ColoredTextBoxControl_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                bool didUndo = false;
                var undo = new UndoItem { Text = Text, ColStart = selStartCol, ColEnd = selEndCol, RowStart = selStartRow, RowEnd = selEndRow };

                switch (e.KeyCode)
                {
                    case Keys.ShiftKey:
                        break;
                    case Keys.Down:
                        if (fAutoComplete.Visible)
                        {
                            fAutoComplete.NextItem();
                        }
                        else
                        {
                            row = Math.Min(row + 1, lines.Count - 1);
                            col = Math.Min(col, lines[row].Text.Length);
                            HandleSelection();
                            caretOn = true; Redraw(true, true);
                        }
                        break;
                    case Keys.Up:
                        if (fAutoComplete.Visible)
                        {
                            fAutoComplete.PrevItem();
                        }
                        else
                        {
                            row = Math.Max(row - 1, 0);
                            col = Math.Min(col, lines[row].Text.Length);
                            HandleSelection();
                            caretOn = true; Redraw(true, true);
                        }
                        break;
                    case Keys.Right:
                        if (col < lines[row].Text.Length)
                            col++;
                        else
                        {
                            if (row < lines.Count - 1)
                            {
                                col = 0;
                                row = Math.Min(row + 1, lines.Count - 1);
                            }
                        }
                        HandleSelection();
                        caretOn = true; Redraw(true, true);
                        break;
                    case Keys.Left:
                        if (col == 0 && row > 0)
                        {
                            row--;
                            col = lines[row].Text.Length - 1;
                        }
                        else
                        {
                            col = Math.Max(col - 1, 0);
                        }
                        HandleSelection();
                        caretOn = true; Redraw(true, true);
                        break;
                    case Keys.Back:
                        if (fAutoComplete.Visible)
                        {
                            if (fAutoComplete.Filter.Length > 0)
                            {
                                fAutoComplete.Filter = fAutoComplete.Filter.Substring(0, fAutoComplete.Filter.Length - 1);
                                selEndCol--; selEndCol--;
                                if (selEndCol < selStartCol)
                                    selEndCol = selStartCol;
                            }
                        }
                        if (isTextSelected && !fAutoComplete.Visible) //selStartCol != selEndCol || selStartRow != selEndRow)
                            DeleteSelectedText();
                        else
                        {
                            if (col > 0)
                            {
                                lines[row].Text = lines[row].Text.Remove(col - 1, 1);
                                col--;
                            }
                            else
                            {
                                if (row > 0)
                                {
                                    col = lines[row - 1].Text.Length;
                                    lines[row - 1].Text = lines[row - 1].Text + lines[row];
                                    lines.RemoveAt(row);
                                    row--;
                                }

                            }
                            col = Math.Min(lines[row].Text.Length, Math.Max(0, col));
                        }
                        caretOn = true; Redraw(true, true);
                        break;
                    case Keys.Delete:
                        if (isTextSelected) //selStartCol != selEndCol || selStartRow != selEndRow)
                            DeleteSelectedText();
                        else
                        {
                            if (col < lines[row].Text.Length && col >= 0)
                            {
                                lines[row].Text = lines[row].Text.Remove(col, 1);
                            }
                            else
                            {
                                if (row < lines.Count)
                                {
                                    if (lines.Count > row + 1)
                                    {
                                        lines[row].Text = lines[row].Text + lines[row + 1].Text;
                                        lines.RemoveAt(row + 1);
                                    }
                                }
                            }
                        }
                        caretOn = true; Redraw(true, true);
                        break;
                    case Keys.PageUp:
                        firstVisibleRow = Math.Max(0, firstVisibleRow - ((int)visibleRowCount - 2));
                        row = firstVisibleRow;
                        Redraw();
                        break;
                    case Keys.PageDown:
                        firstVisibleRow = Math.Min(lines.Count - (int)visibleRowCount, firstVisibleRow + ((int)visibleRowCount - 2));
                        row = firstVisibleRow + (int)visibleRowCount - 2;
                        Redraw();
                        break;
                    case Keys.Tab:
                        if (fAutoComplete.Visible)
                        {
                            if (col >= lines[row].Text.Length - 1)
                                col--;
                            while (col >= 0 && lines[row].Text[col] != '.' && lines[row].Text[col] != '\\')
                                col--;
                            col++;
                            selStartCol = col;
                            PasteText(fAutoComplete.SelectedText);
                            fAutoComplete.Hide();
                        }
                        else
                        {
                            if (selStartRow != selEndRow)
                            {
                                if (Control.ModifierKeys == Keys.Shift)
                                {
                                    for (int r = Math.Min(selStartRow, selEndRow); r <= Math.Max(selStartRow, selEndRow); r++)
                                    {
                                        //remove only leading spaces
                                        int spaceCount = lines[r].Text.TakeWhile(Char.IsWhiteSpace).Count();
                                        lines[r].Text = lines[r].Text.Remove(0, Math.Min(tabLen, spaceCount));
                                    }
                                    selStartCol -= tabLen; if (selStartCol < 0) selStartCol = 0;
                                    selEndCol -= tabLen; if (selEndCol < 0) selEndCol = 0;
                                }
                                else
                                {
                                    for (int r = Math.Min(selStartRow, selEndRow); r <= Math.Max(selStartRow, selEndRow); r++)
                                        lines[r].Text = lines[r].Text.Insert(0, "".PadLeft(tabLen));
                                    selStartCol += tabLen;
                                    selEndCol += tabLen;
                                }
                            }
                            else
                            {
                                lines[row].Text = lines[row].Text.Insert(col, "".PadLeft(tabLen));
                            }
                            col += tabLen;
                            caretOn = true; Redraw(true, true);
                        }
                        break;
                    case Keys.Enter:
                        if (e.Shift && e.Control)
                        {
                            OnSpecialKey(e);
                        }
                        else
                        {
                            if (fAutoComplete.Visible)
                            {
                                if (col >= lines[row].Text.Length - 1)
                                    col--;
                                while (col >= 0 && lines[row].Text[col] != '.' && lines[row].Text[col] != '\\')
                                    col--;
                                col++;
                                selStartCol = col;
                                PasteText(fAutoComplete.SelectedText);
                                fAutoComplete.Hide();
                            }
                            else
                            {
                                if (row < lines.Count && col <= lines[row].Text.Length)
                                    lines[row].Text = lines[row].Text.Insert(col, "\n");
                                else
                                    lines.Insert(row, new LineItem() { HighlightList = TestHighlightList, Text = "" });
                                row++;
                                col = 0;
                                Text = Text; //re-split all lines
                                caretOn = true; Redraw(true, true);
                            }
                        }
                        break;
                    case Keys.Escape:
                        if (fAutoComplete.Visible)
                        {
                            fAutoComplete.Hide();
                            e.Handled = true;
                        }
                        break;
                    case Keys.End:
                        col = lines[row].Text.Length;
                        HandleSelection();
                        caretOn = true; Redraw(true, true);
                        break;
                    case Keys.Home:
                        col = 0;
                        HandleSelection();
                        caretOn = true; Redraw(true, true);
                        break;
                    case Keys.Z:
                        if (Control.ModifierKeys == Keys.Control) //undo
                        {
                            didUndo = Undo(didUndo);
                        }
                        break;
                    case Keys.Y:
                        if (Control.ModifierKeys == Keys.Control) //redo
                        {
                            didUndo = Redo(didUndo);
                        }
                        break;
                    case Keys.C:
                        if (Control.ModifierKeys == Keys.Control)
                        {
                            if (ctrlSequence == "k")
                            {
                                CommentSelection();
                                ctrlSequence = "";
                            }
                            else
                            {
                                CopyToClipboard();
                            }
                        }
                        break;
                    case Keys.X:
                        if (Control.ModifierKeys == Keys.Control)
                        {
                            CutToClipboard();
                        }
                        break;
                    case Keys.V:
                        if (Control.ModifierKeys == Keys.Control)
                        {
                            PasteClipboardText();
                        }
                        break;
                    case Keys.K:
                        if (Control.ModifierKeys == Keys.Control)
                        {
                            ctrlSequence = "k";
                        }
                        break;
                    case Keys.U:
                        if (Control.ModifierKeys == Keys.Control)
                        {
                            if (ctrlSequence == "k")
                            {
                                UncommentSelection();
                                ctrlSequence = "";
                            }
                        }
                        break;
                    case Keys.A:
                        if (Control.ModifierKeys == Keys.Control)
                        {
                            SelectAll();
                            Redraw(true, true);
                        }
                        break;
                    case Keys.F:
                    case Keys.H:
                        if (Control.ModifierKeys == Keys.Control)
                        {
                            panelFind.Visible = true;
                            textBoxFind.Focus();
                        }
                        break;

                    default:
                        //if (fAutoComplete.Visible)
                        //{
                        //    char c = (char)e.KeyValue;
                        //    if (Char.IsLetterOrDigit(c))
                        //        fAutoComplete.Filter += c;
                        //}
                        break;
                }

                if (row > (firstVisibleRow + visibleRowCount - 1))
                    firstVisibleRow = row - (int)visibleRowCount + 1;

                if (row < firstVisibleRow)
                    firstVisibleRow = row;

                if (col >= firstVisibleCol + visibleColCount - 3)
                    firstVisibleCol = col - (int)visibleColCount + 3;

                if (col < firstVisibleCol + 3)
                    firstVisibleCol = Math.Max(0, col - 3);

                if (!didUndo && Text != undo.Text)
                {
                    //while (undoHistory.Count > 100)
                    //{
                    //    undoHistory.RemoveAt(0);
                    //    undoHistoryPos--;
                    //}

                    undoHistory.Push(undo);

                    //undoHistoryPos++;
                }

                TextChanged();

                Redraw(true, true);
                UpdateScrollbars();
            }
            catch (Exception ex)
            {
            }
        }

        private void CommentSelection()
        {
            //comment lines
            int row1 = Math.Min(selStartRow, selEndRow);
            int row2 = Math.Max(selStartRow, selEndRow);
            if (row2 == selEndRow && selEndCol == 0)
                row2--;
            for (int r = row1; r <= row2; r++)
                lines[r].Text = lines[r].Text.Insert(0, "//");
            this.Invalidate();
        }

        private void UncommentSelection()
        {
            //uncomment lines
            int row1 = Math.Min(selStartRow, selEndRow);
            int row2 = Math.Max(selStartRow, selEndRow);
            if (row2 == selEndRow && selEndCol == 0)
                row2--;
            for (int r = row1; r <= row2; r++)
            {
                //remove 2 leading '//'
                if (lines[r].Text.TrimStart().StartsWith("//"))
                {
                    int commentPos = lines[r].Text.IndexOf("//");
                    lines[r].Text = lines[r].Text.Substring(0, commentPos) + lines[r].Text.Substring(commentPos + 2);
                }
            }
            this.Invalidate();
        }

        private bool Redo(bool didUndo)
        {
            if (undoHistory.Count > 0)
            {
                //var undoItem = undoHistory.Last();
                var undoItem = undoHistory.Pop(); // undoHistory[undoHistoryPos];
                this.Text = undoItem.Text;
                selStartRow = undoItem.RowStart;
                selEndRow = undoItem.RowEnd;
                selStartCol = undoItem.ColStart;
                selEndCol = undoItem.ColEnd;
                row = selStartRow;
                col = selStartCol;
                //undoHistoryPos--;
                //undoHistory.RemoveAt(undoHistory.Count - 1);

                didUndo = true;
                panelText.Invalidate();
            }
            else
            {
                FlashError(Color.Gray);
            }

            return didUndo;
        }

        private bool Undo(bool didUndo)
        {
            //var undoItem = undoHistory.Last();
            if (undoHistory.Count > 0)
            {
                var undoItem = undoHistory.Pop(); // undoHistory[undoHistoryPos];
                this.Text = undoItem.Text;
                selStartRow = undoItem.RowStart;
                selEndRow = undoItem.RowEnd;
                selStartCol = undoItem.ColStart;
                selEndCol = undoItem.ColEnd;
                row = selStartRow;
                col = selStartCol;

                //if (undoHistory.Peek().Text != Text)
                //{
                //    undoHistory.RemoveAt(0);
                //    undoHistory.Push(new UndoItem { Text = Text, ColStart = selStartCol, ColEnd = selEndCol, RowStart = selStartRow, RowEnd = selEndRow });
                //    didUndo = true;
                //}
                didUndo = true;
                panelText.Invalidate();
            }
            else
            {
                FlashError(Color.Gray);
            }

            return didUndo;
        }

        private void CopyToClipboard()
        {
            if (!string.IsNullOrEmpty(SelectedText))
                Clipboard.SetText(SelectedText);
        }

        private void CutToClipboard()
        {
            if (!string.IsNullOrEmpty(SelectedText))
                Clipboard.SetText(SelectedText);
            DeleteSelectedText();
            panelText.Invalidate();
        }

        private void PasteClipboardText()
        {
            string text = Clipboard.GetText().Replace("\r", "");
            PasteText(text);
            /*
            if (SelectionStartIndex > SelectionEndIndex)
                (SelectionStartIndex, SelectionEndIndex) = (SelectionEndIndex, SelectionStartIndex);
            DeleteSelectedText();
            //Text = Text.Substring(0, SelectionStartIndex) + Text.Substring(SelectionEndIndex);
            string text = Clipboard.GetText().Replace("\r", "");
            Text = Text.Insert(SelectionStartIndex, text);
            SelectionEndIndex = SelectionStartIndex + text.Length;
            row = selEndRow;
            col = selEndCol;
            selStartRow = row;
            selStartCol = col;
            panelText.Invalidate();
            */
        }

        internal void PasteText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            if (SelectionStartIndex > SelectionEndIndex)
                (SelectionStartIndex, SelectionEndIndex) = (SelectionEndIndex, SelectionStartIndex);
            DeleteSelectedText();
            //Text = Text.Substring(0, SelectionStartIndex) + Text.Substring(SelectionEndIndex);
            Text = Text.Insert(SelectionStartIndex, text);
            SelectionEndIndex = SelectionStartIndex + text.Length;
            row = selEndRow;
            col = selEndCol;
            selStartRow = row;
            selStartCol = col;
            panelText.Invalidate();
        }


        private void SelectAll()
        {
            selStartRow = 0;
            selStartCol = 0;
            selEndRow = lines.Count - 1;
            selEndCol = lines[selEndRow].Text.Length; // - 1;
            panelText.Invalidate();
        }

        Timer _textChangedDelayedTimer = null;
        void TextChanged()
        {
            //Item.Settings = textBoxSettings.Text.Replace("\r\n", "\n").Trim('\n').Replace("\n", "\r\n");
            if (_textChangedDelayedTimer == null)
            {
                _textChangedDelayedTimer = new Timer();
                _textChangedDelayedTimer.Interval = 1000;
                _textChangedDelayedTimer.Tick += _textChangedDelayedTimer_Tick;
            }
            _textChangedDelayedTimer.Stop(); // Resets the timer
            _textChangedDelayedTimer.Start();

        }
        private void _textChangedDelayedTimer_Tick(object sender, EventArgs e)
        {
            var timer = sender as Timer;
            if (timer == null)
            {
                return;
            }
            timer.Stop();

            //Item.Settings = textBoxSettings.Text.Replace("\r\n", "\n").Trim('\n').Replace("\n", "\r\n");
            OnTextChangedDelayed();
        }

        public class TextChangedEventArgs : EventArgs
        {
        }
        public delegate void TextChangedEventHandler(TextChangedEventArgs e);
        public event TextChangedEventHandler TextChangedDelayed;
        void OnTextChangedDelayed()
        {
            if (TextChangedDelayed != null)
            {
                TextChangedEventArgs ea = new TextChangedEventArgs() { };
                TextChangedDelayed(ea);
            }
        }

        public class KeyPressedEventArgs : EventArgs
        {
            public char KeyChar;
            public string TextBeforeChar;
            public List<string> AutoCompleteItems;
        }
        public delegate void KeyPressedEventHandler(KeyPressedEventArgs e);
        public event KeyPressedEventHandler KeyPressed;
        void OnKeyPressed(char c, string textBeforeChar, out List<string> autoCompleteItems)
        {
            if (KeyPressed != null)
            {
                KeyPressedEventArgs ea = new KeyPressedEventArgs() { KeyChar = c, TextBeforeChar = textBeforeChar };
                KeyPressed(ea);
                autoCompleteItems = ea.AutoCompleteItems;
            }
            else
            {
                autoCompleteItems = null;
            }
        }


        private void ColoredTextBoxControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            var undo = new UndoItem { Text = Text, ColStart = selStartCol, ColEnd = selEndCol, RowStart = selStartRow, RowEnd = selEndRow };

            if (!char.IsControl(e.KeyChar))
            {
                if (selStartCol != selEndCol || selStartRow != selEndRow)
                {
                    if (!fAutoComplete.Visible)
                        DeleteSelectedText();
                }

                if (row == lines.Count) // && lines[row] == null)
                {
                    lines.Add(new LineItem() { HighlightList = TestHighlightList, Text = "" });
                    lines[row].Text = e.KeyChar + "";
                    col = 1;
                    selStartCol = col;
                    selEndCol = col;
                }
                else if (row < lines.Count && string.IsNullOrEmpty(lines[row].Text))
                {
                    lines[row].Text = e.KeyChar + "";
                    col = 1;
                    selStartCol = col;
                    selEndCol = col;
                }
                else if (row < lines.Count && col <= lines[row].Text.Length && col >= 0)
                {
                    lines[row].Text = lines[row].Text.Insert(col, e.KeyChar + "");
                    col++;
                    if (!fAutoComplete.Visible)
                    {
                        selStartCol = col;
                        selEndCol = col;
                    }
                }
            }

            if (Text != undo.Text)
            {
                while (undoHistory.Count > 100)
                {
                    undoHistory.RemoveAt(0);
                }
                undoHistory.Push(new UndoItem { Text = Text, ColStart = selStartCol, ColEnd = selEndCol, RowStart = selStartRow, RowEnd = selEndRow });
            }


            if (EnableAutocomplete)
            {
                if (fAutoComplete.Visible)
                {
                    char c = e.KeyChar;
                    if (Char.IsLetterOrDigit(c))
                        fAutoComplete.Filter += c;
                    if (fAutoComplete.Filter.Length > 0)
                        selEndCol++;
                }
                if (e.KeyChar == '.')
                {
                    string textBeforeDot = lines[row].Text.Substring(0, col);
                    HandleAutocomplete(textBeforeDot, _parentObjectName);
                }
                else if (e.KeyChar == '\\')
                {
                    string textBeforeDot = lines[row].Text.Substring(0, col);
                    HandleAutocomplete(textBeforeDot, _parentObjectName);
                }
                else if (Char.IsLetterOrDigit(e.KeyChar))
                {
                    string textBeforeDot = lines[row].Text.Substring(0, col);
                    HandleAutocomplete(textBeforeDot, _parentObjectName);
                }
                //else
            }

            UpdateScrollbars();
        }

        private Brush BackColorBrush = Brushes.White;

        private void PanelText_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
            {
                firstVisibleCol = e.NewValue;
                Redraw(true, true);
            }
            else
            {
                firstVisibleRow = e.NewValue;
                Redraw(true, true);
            }
        }

        private void ColoredTextBoxControl_MouseWheel(object sender, MouseEventArgs e)
        {
            int line = firstVisibleRow - e.Delta / 40;
            firstVisibleRow = Math.Max(0, Math.Min(line, lines.Count - (int)visibleRowCount));
            Redraw(true, true);
            UpdateScrollbars();
        }

        public bool EnableAutocomplete = false;

        public string OrigText = null;
        public string Text
        {
            get
            {
                return string.Join("\n", lines);
            }
            set
            {
                var newlines = value.Split('\n').ToList();
                lines.Clear();
                foreach (var line in newlines)
                    lines.Add(new LineItem() { HighlightList = TestHighlightList, Text = line });

                if (OrigText == null)
                    OrigText = Text;

                Redraw(true, true);
                UpdateScrollbars();
            }
        }


        public List<string> Lines
        {
            get
            {
                return lines.Select(l => l.Text).ToList();
            }
        }

        class LineItem
        {
            public List<HighlightItem> HighlightList;
            string _Text = null;
            public string Text
            {
                get
                {
                    return _Text;
                }
                set
                {
                    _Text = value.Replace("\r", "");
                    SplitItems = TokenizeText(value, HighlightList);
                }
            }
            public List<SplitItem> SplitItems { get; set; }

            public override string ToString()
            {
                return Text;
            }

            public string Error = null;
        }

        public void ClearErrors()
        {
            lines.ForEach(l => l.Error = null);
        }
        public void SetError(int row, string info)
        {
            lines[row].Error = info;
        }



        List<LineItem> lines = new List<LineItem>();
        int firstVisibleRow = 0;
        int firstVisibleCol = 0;
        float visibleRowCount = 10;
        float visibleColCount = 10;
        int selStartRow = 0;
        int selStartCol { get; set; } = 0;
        int selEndRow = 0;
        int selEndCol { get; set; } = 0;

        int tabLen = 4;
        int _row = 0;

        bool isTextSelected
        {
            get => (selStartCol != selEndCol || selStartRow != selEndRow);
        }


        int row
        {
            get => _row;
            set => _row = value;
        }
        int col = 0;
        Brush selectionBrush = new SolidBrush(Color.FromArgb(64, 30, 30, 200));

        public bool ShowLineNumbers = true;

        List<HighlightItem> TestHighlightList = new List<HighlightItem>();
        class HighlightItem
        {
            public string Name;
            public Regex Regex;
            private Regex _HighlightRegex = null;
            public Regex HighlightRegex
            {
                get
                {
                    if (_HighlightRegex != null)
                        return _HighlightRegex;
                    else
                        return Regex;
                }
                set
                {
                    _HighlightRegex = value;
                }
            }
            public Brush Brush = Brushes.Black;
            public Brush BackBrush = null;
            public FontStyle FontStyle = FontStyle.Regular;
            public bool StopAfterThisRule = false;
        }

        Font lineNrFont = new Font("Consolas", 8);

        int VisibleHeight => (this.Height - SystemInformation.HorizontalScrollBarHeight); // - hScrollBar.Height;
        int VisibleWidth => this.Width; // - vScrollBar.Width;
        Font fontBold = null;
        Font fontItalic = null;
        Font fontBoldItalic = null;


        class SplitItem
        {
            public string Text;
            public HighlightItem HLI;
            public int Start;
            public int Length;
            public int End => Start + Length;
            public string HliName => HLI?.Name ?? null;

            public override string ToString()
            {
                return $"[{Start}:{Length}] {Text} ({HLI.Name}/{((SolidBrush)HLI.Brush).Color})";
            }
        }
        static List<SplitItem> TokenizeText(string text, List<HighlightItem> highlightList)
        {
            List<SplitItem> items = new List<SplitItem>();
            foreach (var hli in highlightList) //.Take(1))
            {
                var matches = hli.Regex.Matches(text);
                foreach (Match match in matches)
                {
                    int start = match.Captures[0].Index;
                    int len = match.Captures[0].Length;
                    int end = start + len;
                    string subText = match.Captures[0].Value;

                    bool rangeInUse = items.Any(i => (start > i.Start && start < i.End) || (end > i.Start && end < i.End));
                    if (!rangeInUse)
                        items.Add(new SplitItem() { Text = subText, HLI = hli, Start = start, Length = len });
                }
            }

            items = items.OrderBy(i => i.Start).ToList();
            return items;
        }


        int mouseStartRow = -1;
        int mouseStartCol = -1;

        int GetRowFromPoint(Point p)
        {
            int r = firstVisibleRow + (int)(p.Y / lineHeight);
            if (r >= lines.Count)
                r = lines.Count - 1;
            if (r < 0)
                r = 0;
            return r;
        }
        int GetColFromPoint(Point p)
        {
            int r = GetRowFromPoint(p);
            int c = firstVisibleCol + (int)((p.X - 2.0f) / charWidth);
            if (r < lines.Count && c > lines[r].Text.Length)
                c = lines[r].Text.Length;
            if (c < 0)
                c = 0;
            return c;
        }

        public Point CursorPositionInPixel
        {
            get
            {
                return new Point(
                    (int)((col - firstVisibleCol) * charWidth) + panelGutter.Width,
                    (int)((row - firstVisibleRow) * lineHeight) + panelColumn.Height
                    );
            }
        }

        public void IndexToRowCol(int index, out int row, out int col)
        {
            int charCount = 0;
            row = 0;
            col = 0;
            while (charCount < index)
            {
                charCount += lines[row].Text.Length + 1;
                row++;
            }
            row--;
            row = Math.Max(0, Math.Min(row, lines.Count - 1));
            col = charCount - index;
            col = Math.Max(0, Math.Min(col, lines[row].Text.Length - 1));
        }


        float lineHeight = 10.0f;
        float charWidth = 0.0f;
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            lineHeight = this.Font.Size * 1.7f;
            Redraw(true, true);

            UpdateScrollbars();
        }

        bool caretOn = false;
        private void timer1_Tick(object sender, EventArgs e)
        {
            ShowDebugInfo = false;
            if (ShowDebugInfo)
            {
                this.FindForm().Text = $"{row},{col}; sel=[{selStartRow},{selStartCol}]-[{selEndRow},{selEndCol}];";
                //(this.Parent as Form).Text = $"undo: {undoHistory.Count} [{(undoHistory.Count>0 ? undoHistory.Peek().Text : "")}]";
            }

            if (DesignMode)
                return;

            //if (!this.FindForm().Focused && fAutoComplete.Visible)
            //    fAutoComplete.Hide();

            caretOn = !caretOn;
            Redraw(true, true);
        }

        class UndoItem
        {
            public string Text;
            public int ColStart = 0;
            public int ColEnd = 0;
            public int RowStart = 0;
            public int RowEnd = 0;
        }

        public class StackEx<T>
        {
            private List<T> items = new List<T>();

            public void Push(T item)
            {
                items.Add(item);
            }
            public T Pop()
            {
                if (items.Count > 0)
                {
                    T temp = items[items.Count - 1];
                    items.RemoveAt(items.Count - 1);
                    return temp;
                }
                else
                    return default(T);
            }
            public T Peek()
            {
                if (items.Count > 0)
                {
                    T temp = items[items.Count - 1];
                    return temp;
                }
                else
                    return default(T);
            }
            public void RemoveAt(int itemAtPosition)
            {
                items.RemoveAt(itemAtPosition);
            }

            public void Clear()
            {
                items.Clear();
            }

            public int Count => items.Count;

        }

        StackEx<UndoItem> undoHistory = new StackEx<UndoItem>();
        string ctrlSequence = "";

        Dictionary<Color, SolidBrush> BrushFromColor = new Dictionary<Color, SolidBrush>();
        void FlashError(Color? color = null)
        {
            if (color == null)
                color = Color.LightCoral;

            if (!BrushFromColor.ContainsKey((Color)color))
                BrushFromColor.Add((Color)color, new SolidBrush((Color)color));
            var brush = BrushFromColor[(Color)color];

            new Task(() =>
            {
                panelText.Invoke((MethodInvoker)(() =>
                {
                    panelText.BackColor = (Color)color;
                    BackColorBrush = brush;
                    panelText.Refresh();
                    System.Threading.Thread.Sleep(20);
                    panelText.BackColor = BackColor;
                    BackColorBrush = Brushes.White;
                    panelText.Refresh();
                }));
            }).Start();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            switch (e.KeyCode)
            {
                case Keys.ControlKey:
                    ctrlSequence = "";
                    break;
            }
        }

        private void UpdateScrollbars()
        {
            try
            {
                panelText.VerticalScroll.Minimum = 0;
                panelText.VerticalScroll.Maximum = lines.Count; // - (int)visibleLineCount;
                                                                //vScrollBar.Value = Math.Max(0, firstVisibleLine - (int)(visibleLineCount / 2.0f));
                panelText.VerticalScroll.Value = selStartRow;

                panelText.HorizontalScroll.Minimum = 0;
                panelText.HorizontalScroll.Maximum = lines.Count > 0 ? lines.Max(l => l.Text.Length) : 0;
                panelText.HorizontalScroll.Value = Math.Max(panelText.HorizontalScroll.Minimum, Math.Min(panelText.HorizontalScroll.Maximum, selStartCol));
            }
            catch (Exception ex)
            { }
        }

        void DeleteSelectedText()
        {
            try
            {
                if (!isTextSelected) //selStartCol == selEndCol && selStartRow == selEndRow) //i.e. nothing is selected
                {
                    //if (col < lines[row].Text.Length && col >= 0)
                    //{
                    //    lines[row].Text = lines[row].Text.Remove(col, 1);
                    //}
                    //else
                    //{
                    //    if (row < lines.Count)
                    //    {
                    //        if (lines.Count > row + 1)
                    //        {
                    //            lines[row].Text = lines[row].Text + lines[row + 1].Text;
                    //            lines.RemoveAt(row + 1);
                    //        }
                    //    }
                    //}
                }
                else
                {
                    int row1 = selStartRow;
                    int row2 = selEndRow;
                    int col1 = selStartCol;
                    int col2 = selEndCol;

                    if (row1 > row2)
                    {
                        (row1, row2) = (row2, row1);
                        (col1, col2) = (col2, col1);
                    }
                    if (row2 >= lines.Count)
                        row2 = lines.Count - 1;

                    if ((row1 == row2) && (col1 > col2))
                        (col1, col2) = (col2, col1);

                    col1 = Math.Min(col1, lines[row1].Text.Length);
                    col2 = Math.Min(col2, lines[row2].Text.Length);
                    lines[row1].Text = lines[row1].Text.Substring(0, col1) + lines[row2].Text.Substring(col2);
                    if (row2 > row1)
                        lines.RemoveRange(row1 + 1, row2 - (row1));
                    row = row1;
                    col = col1;
                    selStartRow = row;
                    selEndRow = row;
                    if (!fAutoComplete.Visible)
                    {
                        selStartCol = col;
                        selEndCol = col;
                    }
                }
            }
            catch (Exception ex)
            { }

        }

        int SelectionStartIndex
        {
            get
            {
                if (!isTextSelected) //selStartRow == selEndRow && selStartCol == selEndCol)
                {
                    selStartRow = row;
                    selEndRow = row;
                    selStartCol = col;
                    selEndCol = col;
                }
                return lines.Take(selStartRow).Sum(l => l.Text.Length + 1) + selStartCol;
            }
            set
            {
                if (value < 0 || value >= Text.Length)
                    return;
                /*
                int index = 0;
                int r = 0;
                while (index < value)
                    index += lines[r++].Text.Length;
                selStartRow = r - 1;
                selStartCol = (value - index);
                */
                int index = 0;
                int r = 0;
                while (index <= value)
                {
                    if (r < lines.Count)
                    {
                        index += lines[r].Text.Length + 1;
                        r++;
                    }
                }
                index -= lines[r - 1].Text.Length + 1;
                selStartCol = (value - index);
                selStartRow = r - 1;
            }
        }
        int SelectionEndIndex
        {
            get
            {
                return lines.Take(selEndRow).Sum(l => l.Text.Length + 1) + selEndCol;
            }
            set
            {
                if (value < 0 || value > Text.Length)
                    return;

                /*
                int index = 0;
                int r = 0;
                while (index < value)
                    index += lines[r++].Text.Length;
                selEndRow = r - 1;
                selEndCol = (value - index);
                */

                int index = 0;
                int r = 0;
                while (index <= value)
                {
                    if (r < lines.Count)
                    {
                        index += lines[r].Text.Length + 1;
                        r++;
                    }
                }
                index -= lines[r - 1].Text.Length + 1;
                selEndCol = (value - index);
                selEndRow = r - 1;
            }
        }

        string SelectedText
        {
            get
            {
                int start = SelectionStartIndex;
                int end = SelectionEndIndex;
                if (start > end)
                    (start, end) = (end, start);
                return Text.Substring(start, end - start);
            }
        }


        void HandleSelection()
        {
            if (Control.ModifierKeys == Keys.Shift)
            {
                selEndCol = col;
                selEndRow = row;
            }
            else
            {
                if (!fAutoComplete.Visible)
                {
                    selStartCol = col;
                    selStartRow = row;
                    selEndCol = col;
                    selEndRow = row;
                }
            }
        }

        //protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        //{
        //    base.OnPreviewKeyDown(e);

        //    e.IsInputKey = true;
        //}

        private void vScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            firstVisibleRow = e.NewValue;
            Redraw(true, true);
        }

        private void hScrollBar_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private void panelText_Paint(object sender, PaintEventArgs e)
        {
            if (row < 0)
                row = 0;
            if (col < 0)
                col = 0;
            try
            {
                if (fontBold == null)
                    fontBold = new Font(this.Font, FontStyle.Bold);
                if (fontItalic == null)
                    fontItalic = new Font(this.Font, FontStyle.Italic);
                if (fontBoldItalic == null)
                    fontBoldItalic = new Font(this.Font, FontStyle.Italic | FontStyle.Bold);
                base.OnPaint(e);

                if (charWidth == 0.0)
                {
                    var w1 = e.Graphics.MeasureString("1234567890", this.Font).Width;
                    var w2 = e.Graphics.MeasureString("12345678901", this.Font).Width;
                    charWidth = w2 - w1;
                }

                StringFormat sf = new StringFormat() { FormatFlags = StringFormatFlags.MeasureTrailingSpaces };
                visibleRowCount = panelText.Height / lineHeight - 0.99f;
                visibleColCount = panelText.Width / charWidth - 0.99f;

                //int gutterWidth = ShowLineNumbers ? gutterWidthSet : 0;

                //if (!paintCaret)
                {
                    if (Enabled)
                        e.Graphics.Clear(panelText.BackColor);

                    float x = 0.0f;
                    float y = 0.0f;

                    int lineCount = 0;
                    foreach (var line in lines.Skip(firstVisibleRow).Take((int)visibleRowCount + 1))
                    {
                        lineCount++;

                        x = -firstVisibleCol * charWidth;

                        string text = line.Text;
                        if (showSpecialCharactersToolStripMenuItem.Checked)
                        {
                            text = text.Replace(' ', '·');
                            text = text.Replace('\t', '→');
                            text += '¶';
                        }
                        e.Graphics.DrawString(text, this.Font, Brushes.Black, new RectangleF(x, y, this.Width - x + 100, lineHeight), sf);
                        foreach (var item in line.SplitItems)
                        {
                            text = item.Text;
                            if (showSpecialCharactersToolStripMenuItem.Checked)
                            {
                                text = text.Replace(' ', '·');
                                text = text.Replace('\t', '→');
                            }
                            var size = e.Graphics.MeasureString(text, this.Font, 99999, sf); //, layoutArea);
                            Brush backBrush = item.HLI.BackBrush ?? BackColorBrush;
                            e.Graphics.FillRectangle(backBrush, new RectangleF(x + 2.0f + item.Start * charWidth, y + 1.5f, size.Width - 3.9f, lineHeight - 1.5f));

                            Font font = this.Font;
                            if (item.HLI.FontStyle == FontStyle.Bold)
                                font = fontBold;
                            if (item.HLI.FontStyle == FontStyle.Italic)
                                font = fontItalic;
                            if (item.HLI.FontStyle == (FontStyle.Bold | FontStyle.Italic))
                                font = fontBoldItalic;
                            e.Graphics.DrawString(text.Replace(' ', '·'), font, item.HLI.Brush, new RectangleF(x + item.Start * charWidth, y, this.Width - x + 100, lineHeight), sf);
                        }
                        y += lineHeight;
                        x = 0;

                    }

                    if (isTextSelected) //selStartCol != selEndCol || selStartRow != selEndRow)
                    {
                        /*
                        int row1 = selStartRow;
                        int row2 = selEndRow;
                        if (row1 > row2)
                            (row1, row2) = (row2, row1);
                        */

                        int row1 = selStartRow;
                        int row2 = selEndRow;
                        int col1 = selStartCol;
                        int col2 = selEndCol;

                        if (row1 > row2)
                        {
                            (row1, row2) = (row2, row1);
                            (col1, col2) = (col2, col1);
                        }

                        if ((row1 == row2) && (col1 > col2))
                            (col1, col2) = (col2, col1);

                        for (int r = row1; r <= row2; r++)
                        {
                            if (r >= lines.Count || r < 0)
                                continue;

                            int col1x = col1;
                            int col2x = col2;

                            if (row2 > row1)
                            {
                                if (r == row1)
                                {
                                    //col1 = col1;
                                    col2x = lines[r].Text.Length;
                                }
                                else if (r == row2)
                                {
                                    col1x = 0;
                                    //col2 = selStartCol;
                                }
                                else
                                {
                                    col1x = 0;
                                    col2x = lines[r].Text.Length;
                                }
                            }

                            //int col1 = 0;
                            //int col2 = lines[r].Text.Length;
                            //if (selStartRow > selEndRow)
                            //{
                            //    if (r == row2)
                            //    {
                            //        col1 = 0;
                            //        col2 = selStartCol;
                            //    }
                            //    if (r == row1)
                            //    {
                            //        col1 = selEndCol;
                            //        col2 = lines[r].Text.Length;
                            //    }
                            //}
                            //else
                            //{
                            //    if (r == selStartRow)
                            //        col1 = selStartCol;
                            //    if (r == selEndRow)
                            //        col2 = selEndCol;
                            //}

                            //bool err = false;
                            //if (col1 > lines[r].Text.Length)
                            //{
                            //    err = true;
                            //}
                            //if (col2 > lines[r].Text.Length)
                            //{
                            //    err = true;
                            //}
                            //if (selStartRow == selEndRow && col1 > col2)
                            //{
                            //    (col1, col2) = (col2, col1);
                            //}

                            //if (col1 < 0)
                            //{
                            //    err = true;
                            //}
                            //if (col2 < 0)
                            //{
                            //    err = true;
                            //}

                            //if (!err)
                            {
                                float selX1 = (col1x - firstVisibleCol) * charWidth + 2.5f; //e.Graphics.MeasureString(lines[r].Text.Substring(0, col1), this.Font, 99999, sf).Width - 3.0f;
                                float selX2 = (col2x - firstVisibleCol) * charWidth + 2.5f; //e.Graphics.MeasureString(lines[r].Text.Substring(0, col2), this.Font, 99999, sf).Width - 3.0f;
                                e.Graphics.FillRectangle(selectionBrush, selX1 + /*gutterWidth*/ +0.0f, (r - firstVisibleRow) * lineHeight, selX2 - selX1, lineHeight);
                            }
                        }
                    }
                }

                e.Graphics.DrawRectangle(Pens.LightGray, 0, (row - firstVisibleRow) * lineHeight, this.Width - 1, lineHeight);
                if (caretOn && panelText.Focused)
                {
                    float caretX = 0.0f;
                    try
                    {
                        //caretX = e.Graphics.MeasureString(lines[row].Text.Substring(0, Math.Min(lines[row].Text.Length, col)), this.Font, 99999, sf).Width; //, layoutArea);
                        caretX = (col - firstVisibleCol) * charWidth;
                    }
                    catch
                    {

                    }
                    caretX += 2.5f;
                    if (caretX < 0.0f)
                        caretX = 0.0f;

                    e.Graphics.DrawLine(Pens.Black, caretX, (row - firstVisibleRow) * lineHeight, caretX, (row - firstVisibleRow) * lineHeight + lineHeight);
                }
            }
            catch (Exception ex)
            { }
        }

        private void panelGutter_Paint(object sender, PaintEventArgs e)
        {
            panelGutter.Width = 28; //why?!
            try
            {
                int lineCount = 0;
                float x = 0.0f;
                float y = 0.0f;
                foreach (var line in lines.Skip(firstVisibleRow).Take((int)visibleRowCount + 1))
                {
                    lineCount++;

                    if (ShowLineNumbers)
                    {
                        e.Graphics.DrawString((firstVisibleRow + lineCount).ToString().PadLeft(4), lineNrFont, Brushes.Black, x, 3.0f + (lineHeight * (lineCount - 1)));
                    }

                    if (line.Error != null)
                    {
                        if (line.Error.Contains("#WRN"))
                            e.Graphics.FillEllipse(Brushes.Orange, x, 3.0f + (lineHeight * (lineCount - 1)), 12.0f, 12.0f);
                        else
                            e.Graphics.FillEllipse(Brushes.Red, x, 3.0f + (lineHeight * (lineCount - 1)), 12.0f, 12.0f);
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        void Redraw(bool redrawText = true, bool redrawGutter = true)
        {
            if (redrawGutter)
            {
                panelGutter.Invalidate();
                panelColumn.Invalidate();
            }

            if (redrawText)
                panelText.Invalidate();
        }

        private void panelText_MouseDown(object sender, MouseEventArgs e)
        {
            panelText.Select();
            if (e.Button == MouseButtons.Left)
            {
                mouseStartRow = GetRowFromPoint(e.Location);
                mouseStartCol = GetColFromPoint(e.Location);
                //Console.WriteLine($"mouseDown: r:{mouseStartRow}, C:{mouseStartCol}");
                if (mouseStartRow == -1 && mouseStartCol == -1)
                {
                    selStartRow = row;
                    selStartCol = Math.Min(lines[row].Text.Length, col);
                    selEndRow = selStartRow;
                    selEndCol = selStartCol;
                }
                else
                {
                    selStartRow = mouseStartRow; // + firstVisibleRow;
                    selStartCol = mouseStartCol; // + firstVisibleCol;
                    selEndRow = selStartRow;
                    selEndCol = selStartCol;
                    //selEndRow = mouseStartRow + firstVisibleRow;
                    //selEndCol = mouseStartCol + firstVisibleCol;
                    selStartCol = Math.Min(lines[selStartRow].Text.Length, selStartCol);
                }
            }
        }

        private void panelText_MouseUp(object sender, MouseEventArgs e)
        {
            mouseStartRow = -1;
            mouseStartCol = -1;

            Redraw(true, true);

        }

        private void panelText_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (mouseStartRow != -1 && mouseStartCol != -1)
                {
                    selStartRow = mouseStartRow;
                    selStartCol = mouseStartCol;

                    selEndRow = GetRowFromPoint(e.Location);
                    selEndCol = GetColFromPoint(e.Location);

                    Redraw(true, true);
                }
            }
        }

        private void panelText_MouseClick(object sender, MouseEventArgs e)
        {
            //panelText.Select();
            //panelText.Focus();
            if (e.Button == MouseButtons.Left)
            {
                row = GetRowFromPoint(e.Location);
                col = GetColFromPoint(e.Location);

                if (mouseStartRow == -1 && mouseStartCol == -1)
                {
                    selStartRow = row;
                    selStartCol = Math.Min(lines[row].Text.Length, col);
                    selEndRow = row;
                    selEndCol = Math.Min(lines[row].Text.Length, col);
                }
                Redraw(true, true);
            }
        }

        private void panelColumn_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                panelColumn.Height = 22; //why?!
                int x = panelGutter.Width + 2;
                for (int c = firstVisibleCol; c <= (firstVisibleCol + panelColumn.Width / charWidth); c++)
                {
                    if ((c % 10) == 0)
                    {
                        e.Graphics.DrawLine(Pens.Gray, x + (c - firstVisibleCol) * charWidth, 0, x + (c - firstVisibleCol) * charWidth, panelColumn.Height);
                        e.Graphics.DrawString(c.ToString(), lineNrFont, Brushes.Black, x + (c - firstVisibleCol) * charWidth - 0, -1);
                    }
                    else
                    {
                        e.Graphics.DrawLine(Pens.Gray, x + (c - firstVisibleCol) * charWidth, 12, x + (c - firstVisibleCol) * charWidth, panelColumn.Height);
                    }
                }
                e.Graphics.FillRectangle(Brushes.Black, x + (col - firstVisibleCol) * charWidth + 2, 12, charWidth - 4, 9);

                //top-left
                e.Graphics.FillPolygon(Brushes.LightGray,
                    new Point[] { new Point(0, 0),
                new Point(panelGutter.Width, panelColumn.Height),
                new Point(0, panelColumn.Height)});

                e.Graphics.DrawLine(Pens.Gray, 0, panelColumn.Height - 1, panelGutter.Width, panelColumn.Height - 1);
                e.Graphics.DrawString((col + 1).ToString().PadLeft(3), lineNrFont, Brushes.Black, 8, -1);
                e.Graphics.DrawString((row + 1).ToString(), lineNrFont, Brushes.Black, 0, 9);
            }
            catch (Exception ex)
            { }
        }


        public delegate void SpecialKeyEventHandler(KeyEventArgs e);
        public event SpecialKeyEventHandler SpecialKeyEvent;
        void OnSpecialKey(KeyEventArgs e)
        {
            if (SpecialKeyEvent != null)
            {
                KeyEventArgs ea = new KeyEventArgs(e.KeyData);// { Key = key };
                SpecialKeyEvent(ea);
            }
        }

        private void buttonFindClose_Click(object sender, EventArgs e)
        {
            panelFind.Hide();
            panelText.Focus();
            panelText.Select();
        }

        private void buttonFindNext_Click(object sender, EventArgs e)
        {
            if (checkBoxFindUseRegex.Checked)
                FindNextRegex();
            else
                FindNextNormal();
        }

        private int FindNextNormal()
        {
            int findStartIndex = SelectionEndIndex;

            int foundIndex = -1;
            if (checkBoxFindMatchCase.Checked)
                foundIndex = findStartIndex + Text.Substring(findStartIndex).IndexOf(textBoxFind.Text);
            else
                foundIndex = findStartIndex + Text.Substring(findStartIndex).ToLower().IndexOf(textBoxFind.Text.ToLower());

            if (foundIndex > findStartIndex)
            {
                textBoxFind.BackColor = SystemColors.ControlLight;
                SelectionStartIndex = foundIndex;
                SelectionEndIndex = foundIndex + textBoxFind.Text.Length;
                panelText.Invalidate();
                return SelectionStartIndex;
            }
            else
            {
                textBoxFind.BackColor = Color.LightSalmon;
                return -1;
            }
        }

        private Match FindNextRegex(string replace = null)
        {
            int findStartIndex = SelectionEndIndex;
            RegexOptions options = RegexOptions.None;
            if (!checkBoxFindMatchCase.Checked)
                options |= RegexOptions.IgnoreCase;

            Match match = Regex.Match(Text.Substring(findStartIndex), textBoxFind.Text, options);
            if (match.Success)
            {
                if (replace == null)
                {
                    //just show the match
                    textBoxFind.BackColor = SystemColors.ControlLight;
                    SelectionStartIndex = findStartIndex + match.Index;
                    SelectionEndIndex = findStartIndex + match.Index + match.Length;
                    panelText.Invalidate();
                }
                else
                {
                    //replace the 1st match
                    Regex rx = new Regex(textBoxFind.Text, options);
                    Text = Text.Substring(0, findStartIndex) + rx.Replace(Text.Substring(findStartIndex), replace, 1);
                }
                return match;
            }
            else
            {
                textBoxFind.BackColor = Color.LightSalmon;
                return null;
            }
        }

        private Match FindPrevRegex()
        {
            int findStartIndex = SelectionStartIndex;
            RegexOptions options = RegexOptions.RightToLeft;
            if (!checkBoxFindMatchCase.Checked)
                options |= RegexOptions.IgnoreCase;

            Match match = Regex.Match(Text.Substring(0, findStartIndex), textBoxFind.Text, options);
            if (match.Success)
            {
                textBoxFind.BackColor = SystemColors.ControlLight;
                SelectionStartIndex = match.Index;
                SelectionEndIndex = match.Index + match.Length;
                panelText.Invalidate();
                return match;
            }
            else
            {
                textBoxFind.BackColor = Color.LightSalmon;
                return null;
            }

        }

        private int FindPrevNormal()
        {
            int findStartIndex = SelectionStartIndex;
            int foundIndex = -1;
            if (checkBoxFindMatchCase.Checked)
                foundIndex = Text.Substring(0, findStartIndex).LastIndexOf(textBoxFind.Text);
            else
                foundIndex = Text.Substring(0, findStartIndex).ToLower().LastIndexOf(textBoxFind.Text.ToLower());

            if (foundIndex >= 0 && foundIndex < findStartIndex)
            {
                textBoxFind.BackColor = SystemColors.ControlLight;
                SelectionStartIndex = foundIndex;
                SelectionEndIndex = foundIndex + textBoxFind.Text.Length;
                panelText.Invalidate();
                return SelectionStartIndex;
            }
            else
            {
                textBoxFind.BackColor = Color.LightSalmon;
                return -1;
            }
        }

        private void buttonFindPrev_Click(object sender, EventArgs e)
        {
            if (checkBoxFindUseRegex.Checked)
                FindPrevRegex();
            else
                FindPrevNormal();

        }

        private void buttonFindReplaceNext_Click(object sender, EventArgs e)
        {
            if (checkBoxFindUseRegex.Checked)
            {
                Match match = FindNextRegex(textBoxReplace.Text);
            }
            else
            {
                int foundIndex = FindNextNormal();
                if (foundIndex >= 0)
                {
                    DeleteSelectedText();
                    lines[row].Text = lines[row].Text.Insert(col, textBoxReplace.Text);
                }
            }
        }

        private void buttonFindReplaceAll_Click(object sender, EventArgs e)
        {
            if (checkBoxFindUseRegex.Checked)
            {
                RegexOptions options = RegexOptions.None;
                if (!checkBoxFindMatchCase.Checked)
                    options |= RegexOptions.IgnoreCase;
                //replace all
                Text = Regex.Replace(Text, textBoxFind.Text, textBoxReplace.Text, options);
            }
            else
            {
                if (checkBoxFindMatchCase.Checked)
                {
                    Text = Text.Replace(textBoxFind.Text, textBoxReplace.Text);
                }
                else
                {
                    Text = ReplaceCaseInsensitive(Text, textBoxFind.Text, textBoxReplace.Text);
                }
            }
        }

        public static string ReplaceCaseInsensitive(string str, string old, string @new, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            @new = @new ?? "";
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(old) || old.Equals(@new, comparison))
                return str;
            int foundAt = 0;
            while ((foundAt = str.IndexOf(old, foundAt, comparison)) != -1)
            {
                str = str.Remove(foundAt, old.Length).Insert(foundAt, @new);
                foundAt += @new.Length;
            }
            return str;
        }

        private void textBoxFind_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void textBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.Escape)
            //{
            //    panelFind.Hide();
            //    panelText.Focus();
            //    panelText.Select();
            //}

            if (e.KeyCode == Keys.Enter)
            {
                if (e.Shift)
                {
                    if (checkBoxFindUseRegex.Checked)
                        FindPrevRegex();
                    else
                        FindPrevNormal();
                }
                else
                {
                    if (checkBoxFindUseRegex.Checked)
                        FindNextRegex();
                    else
                        FindNextNormal();
                }
            }
        }

        private void AnyPanelFindControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                panelFind.Hide();
                panelText.Focus();
                panelText.Select();
            }
        }

        private void panelText_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }

        private void panelText_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //try to select word underunder cursor
                int r = GetRowFromPoint(e.Location);
                r = Math.Min(r, lines.Count - 1);
                int c = GetColFromPoint(e.Location);
                c = Math.Min(c, lines[r].Text.Length - 1);
                string text = lines[r].Text;
                int c1 = c;
                while (c1 >= 0 && (Char.IsLetterOrDigit(text[c1]) || text[c1] == '_' || text[c1] == '$'))
                    c1--;
                int c2 = c;
                while ((c2 >= 0 && c2 <= lines[r].Text.Length) && (Char.IsLetterOrDigit(text[c2]) || text[c2] == '_' || text[c2] == '$'))
                    c2++;
                selStartRow = r;
                selStartCol = c1 + 1;
                selEndRow = r;
                selEndCol = c2;
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool didUndo = Undo(false);
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool didUndo = Redo(false);
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CutToClipboard();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyToClipboard();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteClipboardText();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelectedText();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        private void findReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelFind.Visible = true;
            textBoxFind.Focus();
        }


        private void showSpecialCharactersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showSpecialCharactersToolStripMenuItem.Checked = !showSpecialCharactersToolStripMenuItem.Checked;
            panelText.Refresh();
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            undoToolStripMenuItem.Enabled = undoHistory.Count > 0;
            redoToolStripMenuItem.Enabled = false; // redoHistory.Count > 0;

            cutToolStripMenuItem.Enabled = isTextSelected;
            copyToolStripMenuItem.Enabled = isTextSelected;
            deleteToolStripMenuItem.Enabled = isTextSelected;
            pasteToolStripMenuItem.Enabled = Clipboard.ContainsText();


        }

        private void commentSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommentSelection();
        }

        private void uncommentSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UncommentSelection();
        }

        class HintPanel : Label
        {

        }
        HintPanel hintPanel = null;
        private void panelGutter_MouseMove(object sender, MouseEventArgs e)
        {
            int row = firstVisibleRow + (int)(e.Location.Y / lineHeight);
            if (row < 0 || row >= lines.Count)
            {
                hintPanel?.Hide();
                return;
            }

            if (lines[row].Error != null)
            {
                ShowHint(e.Location, lines[row].Error);
            }
            else
            {
                hintPanel?.Hide();
            }
        }

        void ShowHint(Point location, string text, Color? color = null)
        {
            if (hintPanel == null)
            {
                hintPanel = new HintPanel();
                hintPanel.BorderStyle = BorderStyle.FixedSingle;
                hintPanel.Paint += HintPanel_Paint;
                hintPanel.Font = new Font("Tahoma", 10);
                this.Controls.Add(hintPanel);
            }

            hintPanel.Location = location;
            hintPanel.Show();
            hintPanel.BringToFront();
            hintPanel.Text = text;
            hintPanel.BackColor = color ?? Color.Yellow;
        }

        private void HintPanel_Paint(object sender, PaintEventArgs e)
        {
            HintPanel hp = sender as HintPanel;
            var size = e.Graphics.MeasureString(hp.Text, hp.Font);
            if ((hp.Width != size.Width + 4) || (hp.Height != size.Height + 4))
            {
                hp.Width = (int)size.Width + 4;
                hp.Height = (int)size.Height + 4;
            }

            e.Graphics.Clear(hp.BackColor);
            e.Graphics.DrawString(hp.Text, hp.Font, Brushes.Black, hp.ClientRectangle);
        }

        private void panelGutter_MouseLeave(object sender, EventArgs e)
        {
            hintPanel?.Hide();
        }


        FormAutocomplete fAutoComplete = new FormAutocomplete();
        private void HandleAutocomplete(string textBefore, string _this)
        {
            //List<FormAutocomplete.AutoCompleteItem> autocompleteItems = Main.Qb.ScriptingEngine.DefaultAutocompleteHandler(textBefore, _this);
            List<FormAutocomplete.AutoCompleteItem> autocompleteItems = new List<FormAutocomplete.AutoCompleteItem>();
            if (autocompleteItems?.Count > 0)
            {
                fAutoComplete.Items = autocompleteItems;
                fAutoComplete.StartPosition = FormStartPosition.Manual;
                var cursorPos = this.CursorPositionInPixel;
                fAutoComplete.Location = this.PointToScreen(new Point(cursorPos.X + 4, cursorPos.Y + 20));
                fAutoComplete.KeyPress -= FAutoComplete_KeyPress;
                fAutoComplete.KeyPress += FAutoComplete_KeyPress;
                fAutoComplete.KeyDown -= FAutoComplete_KeyDown;
                fAutoComplete.KeyDown += FAutoComplete_KeyDown;
                fAutoComplete.KeyPreview = true;
                fAutoComplete.PreviewKeyDown -= FAutoComplete_PreviewKeyDown;
                fAutoComplete.PreviewKeyDown += FAutoComplete_PreviewKeyDown;

                //fAutoComplete.TopMost = true;
                fAutoComplete.TopLevel = true;
                //fAutoComplete.Parent = this;

                int lastSeparator = textBefore.LastIndexOfAny(new char[] { '.', '»' });
                if (lastSeparator >= 0)
                    fAutoComplete.Filter = textBefore.Substring(lastSeparator + 1);
                else
                    fAutoComplete.Filter = textBefore;

                fAutoComplete.VisibleChanged -= FAutoComplete_VisibleChanged;
                fAutoComplete.VisibleChanged += FAutoComplete_VisibleChanged;
                if (!fAutoComplete.Visible)
                    fAutoComplete.Show(this);
                fAutoComplete.BringToFront();
                fAutoComplete.InfoCallbackEvent -= FAutoComplete_InfoCallbackEvent;
                fAutoComplete.InfoCallbackEvent += FAutoComplete_InfoCallbackEvent;
                //fAutoComplete.Enabled = false;
                this.Focus();

                selStartCol = col;
                selStartRow = row;
                selEndCol = col;
                selEndRow = row;
            }
        }

        private void FAutoComplete_VisibleChanged(object sender, EventArgs e)
        {
        }

        private void FAutoComplete_InfoCallbackEvent(FormAutocomplete.InfoCallbackEventArgs e)
        {
            //MIGRATION-old
            //var props = ScriptingClass.GetObjectProps(e.Parent, e.Name);
            //var methods = ScriptingClass.GetObjectMethods(e.Parent, e.Name);

            //List<string> infoItems = new List<string>();
            //infoItems.AddRange(props);
            //infoItems.AddRange(methods);
            //e.InfoLines = infoItems.ToArray();
        }

        private void FAutoComplete_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void FAutoComplete_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
            {
                if (col >= lines[row].Text.Length - 1)
                    col--;
                while (col >= 0 && lines[row].Text[col] != '.' && lines[row].Text[col] != '\\')
                    col--;
                col++;
                selStartCol = col;
                PasteText(fAutoComplete.SelectedText);
                this.FindForm().BringToFront();
                fAutoComplete.Hide();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.FindForm().BringToFront();
                fAutoComplete.Hide();
            }
        }

        private void FAutoComplete_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13) //enter
            {

            }
        }

        private void panelText_Leave(object sender, EventArgs e)
        {
        }

        private void PanelText_LostFocus(object sender, EventArgs e)
        {
            //if (fAutoComplete.Visible)
            //    fAutoComplete.Hide();
        }
    }
}

