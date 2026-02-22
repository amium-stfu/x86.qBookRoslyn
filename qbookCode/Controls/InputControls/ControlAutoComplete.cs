using qbookCode.Roslyn;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;

namespace qbookCode.Controls.InputControls
{
    internal class ControlAutoComplete
    {
        private  ToolStripDropDown dropDown;
        private  ListBox listBox;

        private List<string> _currentCompletions = new();
        private System.Windows.Forms.Timer _completionTimer;
        private string prefix = string.Empty;
        private char? _lastTriggerChar = null;
        private int _lastRow = -1;
        private bool _newClass = false;

        private ToolStripControlHost host;
        public FormPopup popup;
        public FormSummary summary;
      

        private DocumentEditor Editor;
        private Label summaryLabel;
       

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

                //if (e.KeyCode == Keys.Down) ShowSummaryForSelectedItem();
                //if (e.KeyCode == Keys.Up) ShowSummaryForSelectedItem();
            };

            dropDown = new ToolStripDropDown
            {
                Padding = Padding.Empty,
                AutoClose = false

            };
            host = new ToolStripControlHost(listBox);

            popup = new FormPopup();
          
            summary = new FormSummary();


            popup.ListView.ItemSelected += item =>
            {
                CommitSelection(item.Text);
                popup.Hide();
            };

            popup.ListView.OnSelected = () => { Editor.Focus(); };
        }

        //public void ShowSummaryForSelectedItem()
        //{

        //    Debug.WriteLine(popup.ListView.SelectedItem.FullyQualifiedName);
        //    var selectedItem = popup.ListView.SelectedItem as CompletionItem;
        //    var fqName = selectedItem.FullyQualifiedName;

        //    if (selectedItem == null || string.IsNullOrEmpty(popup.ListView.SelectedItem.FullyQualifiedName))
        //    {
        //        summary.Hide();
        //        return;
        //    }

        //    var summaryText =  qbookCode.Roslyn.RoslynSummarys.GetSummary(popup.ListView.SelectedItem.FullyQualifiedName);

        //    if (!string.IsNullOrWhiteSpace(summaryText))
        //    {
        //        int pos = Editor.CurrentPosition;
        //        int x = Editor.PointXFromPosition(pos);
        //        int y = Editor.PointYFromPosition(pos) + 18;
        //        Point screenPoint = Editor.PointToScreen(new Point(x + popup.ListView.Width, y));
        //        summary.EditorFont = Editor.GetFont();
        //        summary.Height = popup.Height;
        //        summary.ShowSummary(popup.ListView.SelectedItem.FullyQualifiedName + "\r\n\r\n" + summaryText, screenPoint,600);
        //        Editor.Focus();
        //        Editor.GotoPosition(pos);
        //    }
        //    else
        //    {
        //        summary.Hide();
        //    }
        //}

        public void ShowCompletionList(IEnumerable<CompletionItem> suggestions)
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
                Text = s.Text,
                FullyQualifiedName = s.FullyQualifiedName,
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

         //   popup.ListView.SelectedItem = 

         //   ShowSummaryForSelectedItem();
        }

        public  void Next()
        {
            if (popup.Visible)
            {
                popup.ListView.SelectNext();
              //  ShowSummaryForSelectedItem();
            }
        }

        public  void Previous()
        {
            if (popup.Visible)
            {
                popup.ListView.SelectPrevious();
            //    ShowSummaryForSelectedItem();
            }
        }

        public  void Commit(string complete)
        {
            summary.Hide();
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
        public void Hide() { popup.Hide(); summary.Hide(); }
        public bool Visible => popup.Visible;

    }
}
