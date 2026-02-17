using qbookCode.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qbookCode.Roslyn
{
    internal class ReferencesHelperControl
    {
        private DocumentEditor Editor;
        FormPopup popup;
        public  Font EditorFont => Editor.GetFont();

        public ReferencesHelperControl(DocumentEditor editor)
        {
            Editor = editor;
            popup = new FormPopup();

            popup.EditorFont = new Font("Consolas", 10);

            popup.Config(800, 0, 0, 800, 0, 0);

            if (Theme.IsDark)
                popup.ListView.ApplyDarkTheme();
            else
                popup.ListView.ApplyLightTheme();

            popup.ListView.KeyDown += Popup_KeyDown;

            popup.ListView.ItemSelected += item =>
            {
                try
                {
                    string[] data = item.Value.Split('|');

                    if (data.Length == 2)
                    {

                        Core.Explorer.GotoReference(data[0], int.Parse(data[1]));
                        popup.Hide();

                    }
                    else
                    {
                        popup.Hide();
                    }
                }
                catch 
                { 
                
                
                }
            };
        }

           
        

        private void Popup_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Hide();
            }
        }

        public bool Visible => popup?.Visible ?? false;
        public void Hide() => popup?.Hide();

        public async Task ShowAsync()
        {
            if (Editor?.Target?.Document == null) return;

            List<CompletionItem> items = await Editor.GetReferencesItemsAsync();

            var caretPos = Editor.CurrentPosition;
           

            if (items.Count == 0)
            {
                Hide();
                return;
            }

            popup.ListView.SetItems(items);
            popup.EditorFont = EditorFont;
            popup.Height = popup.ListView.GetAutoHeightForItems(maxVisibleItems: 10);

            if (Theme.IsDark)
                popup.ListView.ApplyDarkTheme();
            else
                popup.ListView.ApplyLightTheme();

            int pos = Editor.CurrentPosition;
            int x = Editor.PointXFromPosition(pos);
            int y = Editor.PointYFromPosition(pos) + 18;
            Point screenPoint = Editor.PointToScreen(new Point(x, y));
            popup.Location = screenPoint;
            popup.Show();
            //Editor.Focus();
            //Editor.GotoPosition(pos);
        }

        public void Next()
        {
            if (popup.Visible)
            {
                popup.ListView.SelectNext();
            }
        }

        public void Previous()
        {
            if (popup.Visible)
            {
                popup.ListView.SelectPrevious();
            }
        }

    }
}
