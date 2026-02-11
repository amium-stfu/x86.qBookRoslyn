using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using qbook.CodeEditor;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using RoslynCompletionItem = global::Microsoft.CodeAnalysis.Completion.CompletionItem;
using RoslynDocument = Microsoft.CodeAnalysis.Document;

namespace qbook.ScintillaEditor
{
    public static class RoslynAutoComplete
    {
        private static ToolStripDropDown dropDown;
        private static  ListBox listBox;
      
 
        private static List<string> _currentCompletions = new();
        private static Timer _completionTimer;
        private static string prefix = string.Empty;
        private static char? _lastTriggerChar = null;
        private static int _lastRow = -1;
        private static bool _newClass = false;

        private static ToolStripControlHost host;
        public static FormPopup popup;


        private static RoslynService _roslyn;
        public static CodeNode ActiveNode;
     

        public static DocumentEditor Editor;

        public static System.Drawing.Font ListFont;

        public static Font EditorFont => Editor.GetFont();
        public static void Init(RoslynService roslyn)
        {
            _roslyn = roslyn;

            listBox = new ListBox
            {
                BorderStyle = System.Windows.Forms.BorderStyle.None,
                IntegralHeight = false,
                SelectionMode = SelectionMode.One,
                Font = new System.Drawing.Font("Consolas", 10),
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.Black,
                
                
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

            host = new ToolStripControlHost(listBox)
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

            host = new ToolStripControlHost(listBox);

            // Debounce-Timer: 150 ms
            _completionTimer = new Timer { Interval = 150 };
            _completionTimer.Tick += async (s, e) =>
            {
                _completionTimer.Stop();
                await UpdateCompletionsAsync();
            };

          
            popup = new FormPopup();
            popup.ListView.ItemSelected += item =>
            {
                CommitSelection(item.Text);
                popup.Hide();
            };
        }

        public static async void Editor_CharAdded(object sender, CharAddedEventArgs e, DocumentEditor editor)
        {
            Editor = editor;
            char c = (char)e.Char;
            _lastTriggerChar = c;

            if (!Editor.Target.Active) return;

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

            if (ActiveNode.RoslynDoc == null)
                return;
        }

        private static async Task UpdateCompletionsAsync()
        {
            int caret = Editor.CurrentPosition;
            int currentRow = Editor.LineFromPosition(caret);

            if (currentRow != _lastRow)
            {
                _currentCompletions.Clear();
                _lastRow = currentRow;
            }

            var document = ActiveNode.RoslynDoc;
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
                    document = document.WithText(Microsoft.CodeAnalysis.Text.SourceText.From(Editor.Text, Encoding.UTF8));

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

        private static async Task HandleBackspaceAsync()
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
            var document = ActiveNode.RoslynDoc?.WithText(
                Microsoft.CodeAnalysis.Text.SourceText.From(Editor.Text, Encoding.UTF8));

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
        private static string GetPrefix()
        {
            int pos = Editor.CurrentPosition;
            int start = Editor.WordStartPosition(pos, true);
            return Editor.GetTextRange(start, pos - start);
        }
        public static void CommitSelection()
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

        private static void CommitSelection(string selectedText)
        {
            if (string.IsNullOrEmpty(selectedText))
            {
                Hide();
                return;
            }

            int pos = Editor.CurrentPosition;
            int start = pos - prefix.Length;

            if (_newClass)
            {
                Editor.InsertText(pos, selectedText);
                Editor.GotoPosition(pos + selectedText.Length);
                _newClass = false;
            }
            else
            {
                Editor.DeleteRange(start, prefix.Length);
                Editor.InsertText(start, selectedText);
                Editor.GotoPosition(start + selectedText.Length);
            }

            Hide();
        }


        public static bool IsVisible => popup.Visible;

        public static void HidePopup() => popup.Hide();

        public static void Next()
        {
            if (popup.Visible)
            {
                popup.ListView.SelectNext();
            }
        }

        public static void Previous()
        {
            if (popup.Visible)
            {
                popup.ListView.SelectPrevious();
            }
        }

        public static void Commit()
        {
            if (popup.Visible)
            {
                var text = popup.ListView.SelectedText;
                if (!string.IsNullOrEmpty(text))
                {
                    CommitSelection(text);
                }
                else
                {
                    CommitSelection("");
                }
            }
        }

        public static void Editor_KeyDown(object sender, KeyEventArgs e)
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
                    {
                        var text = popup?.ListView?.SelectedText;
                        if (!string.IsNullOrEmpty(text))
                        {
                            CommitSelection(text);
                        }
                        else
                        {
                            CommitSelection("");
                        }

                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                    }

                case Keys.Escape:
                    Hide();

                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;

                case Keys.Up:
                    popup.ListView.SelectPrevious();
                    e.Handled = true;
                    e.SuppressKeyPress = true; // verhindert Cursorbewegung im Editor
                    break;

                case Keys.Down:
                    popup.ListView.SelectNext();
                    e.Handled = true;
                    e.SuppressKeyPress = true; // verhindert Cursorbewegung im Editor
                    break;

                case Keys.PageUp:
                    if (popup?.Visible == true)
                    {
                        for (int i = 0; i < 10; i++) popup.ListView.SelectPrevious();
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                    break;


                case Keys.PageDown:
                    if (popup?.Visible == true)
                    {
                        for (int i = 0; i < 10; i++) popup.ListView.SelectNext();
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                    break;

                case Keys.Home:
                    if (popup?.Visible == true)
                    {
                        // schnell an den Anfang
                        for (int i = 0; i < 1000; i++) popup.ListView.SelectPrevious();
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                    break;


                case Keys.End:
                    if (popup?.Visible == true)
                    {
                        for (int i = 0; i < 1000; i++) popup.ListView.SelectNext();
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                    break;
            }
        }

        private static void ShowCompletionList(IEnumerable<string> suggestions)
        {
            var list = suggestions.ToList();
            if (list.Count == 0)
            {
                Hide();
                return;
            }

            // CompletionItem-Liste bauen (du kannst später Icons ergänzen)
            var completionItems = list.Select(s => new CompletionItem
            {
                Text = s,
                Icon = null // oder aus Symboltyp ableiten
            }).ToList();

            popup.EditorFont = EditorFont;

            if (Theme.IsDark)
            {
                popup.ListView.ApplyDarkTheme();
            }
            else
            {
                popup.ListView.ApplyLightTheme();
            }

            popup.ListView.SetItems(completionItems);
           
            // Position relativ zur Caret-Position bestimmen
            int pos = Editor.CurrentPosition;
            int x = Editor.PointXFromPosition(pos);
            int y = Editor.PointYFromPosition(pos) + 18;
            Point screenPoint = Editor.PointToScreen(new Point(x, y));

            popup.Height = popup.ListView.GetAutoHeightForItems(maxVisibleItems:10);
            popup.Width = 300;
        
            popup.Location = screenPoint;
            popup.Show();
            Editor.Focus();
            Editor.GotoPosition(pos);
        }



        public static void Hide() => popup.Hide();
        public static bool Visible => popup.Visible;


        public static async Task<(RoslynCompletionItem[] items, int spanStart)> GetCompletionsAsync(
    RoslynDocument doc, int caretPosition, char? triggerChar = null)
        {
            if (doc == null)
                return (Array.Empty<RoslynCompletionItem>(), caretPosition);

            var completionService = CompletionService.GetService(doc);
            if (completionService == null)
                return (Array.Empty<RoslynCompletionItem>(), caretPosition);

            var trigger = triggerChar.HasValue
                ? CompletionTrigger.CreateInsertionTrigger(triggerChar.Value)
                : CompletionTrigger.Invoke;

            var completions = await completionService.GetCompletionsAsync(doc, caretPosition, trigger);
            if (completions == null)
                return (Array.Empty<RoslynCompletionItem>(), caretPosition);

            return (completions.Items.ToArray(), completions.Span.Start);
        }



        static int lastPos;
        private static string GetWordAtCursor()
        {
            int pos = Editor.CurrentPosition;

            // Wortgrenzen bestimmen
            int start = Editor.WordStartPosition(pos, true);
            int end = Editor.WordEndPosition(pos, true);

            // Text zwischen den Grenzen holen
            string word = Editor.GetTextRange(start, end - start);

            return word;
        }

        private static async Task<string> GetClassType()
        {
            var caret = Editor.CurrentPosition;
            var document = ActiveNode.RoslynDoc;
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
