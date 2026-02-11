using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
using System.Windows.Forms;
using System.Drawing;

namespace qbook.ScintillaEditor.InputControls
{
    internal class ControlAutoComplete
    {
        private  ToolStripDropDown dropDown;
        private  ListBox listBox;

        private List<string> _currentCompletions = new();
        private Timer _completionTimer;
        private string prefix = string.Empty;
        private char? _lastTriggerChar = null;
        private int _lastRow = -1;
        private bool _newClass = false;

        private ToolStripControlHost host;
        public FormPopup popup;
        private DocumentEditor Editor;

        public ControlAutoComplete(DocumentEditor editor)
        {
            Editor = editor;

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

            dropDown = new ToolStripDropDown
            {
                Padding = Padding.Empty,
                AutoClose = false

            };
            host = new ToolStripControlHost(listBox);

            popup = new FormPopup();
            popup.ListView.ItemSelected += item =>
            {
                CommitSelection(item.Text);
                popup.Hide();
            };
        }

        public void ShowCompletionList(IEnumerable<string> suggestions)
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

            popup.EditorFont = Editor.GetFont();

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

            popup.Height = popup.ListView.GetAutoHeightForItems(maxVisibleItems: 10);
            popup.Width = 300;

            popup.Location = screenPoint;
            popup.Show();
            Editor.Focus();
            Editor.GotoPosition(pos);
        }

        public  void Next()
        {
            if (popup.Visible)
            {
                popup.ListView.SelectNext();
            }
        }

        public  void Previous()
        {
            if (popup.Visible)
            {
                popup.ListView.SelectPrevious();
            }
        }

        public  void Commit(string complete)
        {
            prefix = complete;
            if (popup.Visible)
            {
                var text = popup.ListView.SelectedText;


               // text = 

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



        public void CommitSelection()
        {
            string? selected = listBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selected))
            {
                popup.Hide();
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
            popup.Hide();
        }

        private  void CommitSelection(string selectedText)
        {
            if (string.IsNullOrEmpty(selectedText))
            {
                popup.Hide();
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

            popup.Hide();
        }
        public void Hide() => popup.Hide();
        public bool Visible => popup.Visible;

    }
}
