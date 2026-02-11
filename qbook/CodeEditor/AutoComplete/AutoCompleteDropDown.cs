using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RoslynDocument = Microsoft.CodeAnalysis.Document;

namespace qbook.CodeEditor.AutoComplete
{
    public class AutoCompleteDropDown
    {
        private readonly ToolStripDropDown dropDown;
        private readonly ListBox listBox;
        private readonly FormEditor Root;
        private readonly Scintilla Editor;
        private readonly RoslynServices _roslyn;

        private List<string> _currentCompletions = new();
        private readonly Timer _completionTimer;
        private string prefix = string.Empty;
        private char? _lastTriggerChar = null;
        private int _lastRow = -1;
        private bool _newClass = false;

        public AutoCompleteDropDown(FormEditor parent, RoslynServices roslyn)
        {
            Root = parent;
            _roslyn = roslyn;
            Editor = Root.Editor;

            listBox = new ListBox
            {
                BorderStyle = System.Windows.Forms.BorderStyle.None,
                IntegralHeight = true,
                SelectionMode = SelectionMode.One,
                Font = new Font("Consolas", 10),
                BackColor = Color.White,
                ForeColor = Color.Black
            };

            
            listBox.Click += (s, e) => CommitSelection();
            listBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    CommitSelection();
                    e.Handled = true;
                }


            };

            var host = new ToolStripControlHost(listBox)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                AutoSize = false
            };

            dropDown = new ToolStripDropDown
            {
                Padding = Padding.Empty,
                AutoClose = false
                
            };
            dropDown.Items.Add(host);

            // Debounce-Timer: 150 ms
            _completionTimer = new Timer { Interval = 150 };
            _completionTimer.Tick += async (s, e) =>
            {
                _completionTimer.Stop();
                await UpdateCompletionsAsync();
            };

            Editor.CharAdded += Editor_CharAdded;
            Editor.KeyDown += Editor_KeyDown;
        }

        private async void Editor_CharAdded(object sender, CharAddedEventArgs e)
        {
           
            char c = (char)e.Char;
            _lastTriggerChar = c;

            if (Root.GetCurrentDocument() == null)
                return;


            // Prüfen, ob zuletzt "new " eingegeben wurde
            int pos = Editor.CurrentPosition;
            string lastText = Editor.GetTextRange(Math.Max(0, pos - 4), 4); // "new "


            if (lastText == "new ")
            {
                string classType = await GetClassType();
                if (!string.IsNullOrEmpty(classType))
                {
                    _newClass = true;
                    // Optional: Vorschlag anzeigen
                    ShowCompletionList(new List<string> { classType });
                }
                return;
            }




            if (char.IsWhiteSpace(c) || ";){}[]".Contains(c))
            {
                Hide();
                return;
            }

            prefix = GetPrefix();

            if (c == '.' || char.IsLetterOrDigit(c))
            {
                _completionTimer.Stop();
                _completionTimer.Start();
            }
            else
            {
                Hide();
            }

            if (Root.GetCurrentDocument() == null)
                return;
        }

        private async Task UpdateCompletionsAsync()
        {
         

                int caret = Editor.CurrentPosition;
            int currentRow = Editor.LineFromPosition(caret);

            if (currentRow != _lastRow)
            {
                _currentCompletions.Clear();
                _lastRow = currentRow;
            }

            var document = Root.GetCurrentDocument();
            if (document == null)
                return;

            // Wort unter Cursor
            string prefix = GetWordAtCursor();

            try
            {
                // Prüfen, ob ein Triggerzeichen vorliegt
                bool isTriggerChar = _lastTriggerChar == '.' ||
                                     _lastTriggerChar == ' ' ||
                                     _lastTriggerChar == '(' ||
                                     _lastTriggerChar == '\0' ||
                                     _currentCompletions.Count == 0; // erste Öffnung

                if (isTriggerChar)
                {
                    // Document synchronisieren
                    document = document.WithText(Microsoft.CodeAnalysis.Text.SourceText.From(Editor.Text));

                    var (items, _) = await GetCompletionsAsync(document, caret, _lastTriggerChar);
                    if (items != null && items.Length > 0)
                    {
                        _currentCompletions = items
                            .Select(i => i.DisplayText)
                            .Distinct()
                            .OrderBy(s => s)
                            .ToList();
                    }
                    else
                    {
                        Hide();
                        return;
                    }
                }

                // Wenn kein Trigger → lokal filtern
                var filtered = string.IsNullOrEmpty(prefix)
                    ? _currentCompletions
                    : _currentCompletions
                        .Where(s => s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                if (filtered.Count == 0)
                {
                    Hide();
                    return;
                }

            

                ShowCompletionList(filtered);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Completion Error: " + ex);
            }
        }

        private async Task HandleBackspaceAsync()
        {
            int caret = Editor.CurrentPosition;
            int currentRow = Editor.LineFromPosition(caret);

            // Zeilenwechsel? => Context reset
            if (currentRow != _lastRow)
            {
                _currentCompletions.Clear();
                Hide();
                _lastRow = currentRow;
                return;
            }

            // Neues Prefix nach dem Löschen
            string prefix = GetWordAtCursor();

            // Wenn gar kein Wort mehr vorhanden ist → Liste schließen
            if (string.IsNullOrEmpty(prefix))
            {
                _currentCompletions.Clear();
                Hide();
                return;
            }

            // 🧠 Wenn du noch in demselben Member-Kontext bist (z. B. nach ".")
            // dann reicht lokales Filtern – kein Roslyn-Neutrigger
            if (_currentCompletions.Count > 0)
            {
                var filtered = _currentCompletions
                    .Where(s => s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(s => s)
                    .ToList();

                if (filtered.Count > 0)
                {
                    ShowCompletionList(filtered);
                    return;
                }
            }

            // 🧩 Wenn kein Cache passt (z. B. du hast das Wort vor dem '.' gelöscht),
            // dann neue Roslyn-Abfrage starten
            var document = Root.GetCurrentDocument()?.WithText(
                Microsoft.CodeAnalysis.Text.SourceText.From(Editor.Text));

            if (document != null)
            {
                var (items, _) = await GetCompletionsAsync(document, caret, null);
                if (items != null && items.Length > 0)
                {
                    _currentCompletions = items.Select(i => i.DisplayText).Distinct().OrderBy(s => s).ToList();
                    ShowCompletionList(_currentCompletions);
                }
                else
                {
                    Hide();
                }
            }
        }
        private string GetPrefix()
        {
            int pos = Editor.CurrentPosition;
            int start = Editor.WordStartPosition(pos, true);
            return Editor.GetTextRange(start, pos - start);
        }

        private void CommitSelection()
        {
            string? selected = listBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selected))
            {
                Hide();
                return;
            }

            int pos = Editor.CurrentPosition;

            
            int start = pos - prefix.Length;

            if (_newClass)
            {

                Editor.InsertText(pos, selected);
                Editor.GotoPosition(pos + selected.Length);
                _newClass = false;
                start = pos;
                _newClass = false;
            }
            else
            {
                Editor.DeleteRange(start, prefix.Length);
                Editor.InsertText(start, selected);
                Editor.GotoPosition(start + selected.Length);
            }



            Hide();
        }

        private void Editor_KeyDown(object sender, KeyEventArgs e)
        {
            if (!Visible)
                return;

            if (e.KeyCode == Keys.Back)
            {
                // wichtig: BeginInvoke auf Editor, nicht auf DropDown
                Editor.BeginInvoke(new Action(async () => await HandleBackspaceAsync()));
            }


                switch (e.KeyCode)
                {
                    case Keys.Enter:
                    case Keys.Tab:
                        CommitSelection();

                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;

                    case Keys.Escape:
                        Hide();

                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;

                    case Keys.Up:
                        if (listBox.SelectedIndex > 0)
                            listBox.SelectedIndex--;
                        e.Handled = true;
                        break;

                    case Keys.Down:
                        if (listBox.SelectedIndex < listBox.Items.Count - 1)
                            listBox.SelectedIndex++;
                        e.Handled = true;
                        break;
                }
            }
        

        private void ShowCompletionList(IEnumerable<string> suggestions)
        {
            var list = suggestions.ToList();
            if (list.Count == 0)
            {
                Hide();
                return;
            }

            listBox.BeginUpdate();
            listBox.Items.Clear();
 
            int maxLength = 0;
            int manWidth = 0;
            foreach (var item in list)
            {
                listBox.Items.Add(item);
                if (item.Length > maxLength) maxLength = item.Length;
                if (TextRenderer.MeasureText(item, listBox.Font).Width > manWidth) manWidth = TextRenderer.MeasureText(item, listBox.Font).Width;
            }

            listBox.SelectedIndex = 0;

            listBox.Font = Root.GetFont();
            if (Root.DarkTheme)
            {
                listBox.BackColor = Color.FromArgb(25, 25, 25);
                listBox.ForeColor = Color.FromArgb(200, 200, 200);

            }
            else
            {
                listBox.BackColor = Color.White;
                listBox.ForeColor = Color.Black;
            }

            listBox.EndUpdate();

            int pos = Editor.CurrentPosition;
            int x = Editor.PointXFromPosition(pos);
            int y = Editor.PointYFromPosition(pos) + 18;
            Point screenPoint = Editor.PointToScreen(new Point(x, y));

            int items = listBox.Items.Count;
            int h = 10 * TextRenderer.MeasureText("W", listBox.Font).Height;
            if (items < 10)
                h = items * TextRenderer.MeasureText("W", listBox.Font).Height;

           // int w = (int)maxLength * TextRenderer.MeasureText("W", listBox.Font).Width / 1.5;
          //  Debug.WriteLine("char " + w);


            listBox.Size = new Size(manWidth + 17, h);
            dropDown.Show(screenPoint);
        }

        public void Hide() => dropDown.Close();
        public bool Visible => dropDown.Visible;


        public async Task<(CompletionItem[] items, int spanStart)> GetCompletionsAsync(
        RoslynDocument doc, int caretPosition, char? triggerChar = null)
        {
            //Debug.WriteLine($"=== UpdateCompletionsAsync ===");
            //Debug.WriteLine($"Caret: {Editor.CurrentPosition}");
            //Debug.WriteLine($"LastTrigger: '{_lastTriggerChar}' ({(int?)_lastTriggerChar ?? 0})");
            //Debug.WriteLine($"Trigger: '{triggerChar}' ({(int?)triggerChar ?? 0})");
            //Debug.WriteLine($"Text: '{Editor.Text.Substring(Math.Max(0, Editor.CurrentPosition - 10), Math.Min(10, Editor.Text.Length - Editor.CurrentPosition + 10))}'");



            if (doc == null)
                return (Array.Empty<CompletionItem>(), caretPosition);

            var completionService = CompletionService.GetService(doc);
            if (completionService == null)
                return (Array.Empty<CompletionItem>(), caretPosition);

            var trigger = triggerChar.HasValue
                ? CompletionTrigger.CreateInsertionTrigger(triggerChar.Value)
                : CompletionTrigger.Invoke;

            var completions = await completionService.GetCompletionsAsync(doc, caretPosition, trigger);
            if (completions == null)
                return (Array.Empty<CompletionItem>(), caretPosition);

            return (completions.Items.ToArray(), completions.Span.Start);
        }


        int lastPos;
        private string GetWordAtCursor()
        {
            int pos = Editor.CurrentPosition;

            // Wortgrenzen bestimmen
            int start = Editor.WordStartPosition(pos, true);
            int end = Editor.WordEndPosition(pos, true);

            // Text zwischen den Grenzen holen
            string word = Editor.GetTextRange(start, end - start);

            return word;
        }

        private async Task<string> GetClassType()
        {
            var caret = Editor.CurrentPosition;
            var document = Root.GetCurrentDocument();
            var tree = await document.GetSyntaxTreeAsync();
            var root = await tree.GetRootAsync();
            var token = root.FindToken(caret);
            var node = token.Parent;
            var semanticModel = await document.GetSemanticModelAsync();

            if (node != null)
            {
                // Suche nach Zuweisung mit "new"
                var variableDecl = node.AncestorsAndSelf().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
                if (variableDecl != null)
                {
                    var variableSymbol = semanticModel.GetDeclaredSymbol(variableDecl);
                    if (variableSymbol is ILocalSymbol localSymbol)
                    {
                        var expectedType = localSymbol.Type;
                        if (expectedType != null)
                        {
                            return expectedType.Name;
                        }
                    }
                }
            }
            return string.Empty;
        }
    }
}
