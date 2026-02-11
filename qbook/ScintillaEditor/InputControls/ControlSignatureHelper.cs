using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace qbook.ScintillaEditor.InputControls
{
    internal class ControlSignatureHelper
    {

        static FormPopup popup;
        public static Font ListFont;
        private DocumentEditor Editor;

        public Font EditorFont => Editor.GetFont();
        public ControlSignatureHelper(DocumentEditor editor)
        {
            Editor = editor;
            popup = new FormPopup();

            popup.EditorFont = new Font("Consolas", 10);

            popup.Config(400, 0, 0, 50, 30, 20);

            if (Theme.IsDark)
                popup.ListView.ApplyDarkTheme();
            else
                popup.ListView.ApplyLightTheme();

        }

        public bool Visible => popup?.Visible ?? false;
        public void Hide() => popup?.Hide();

        public async Task ShowSignaturePopupAsync()
        {
            if (Editor?.Target?.Document == null) return;

            var caretPos = Editor.CurrentPosition;
            var parameters = await Core.Roslyn.GetSignatureParametersAsync(Editor.Target.Document, caretPos);

            if (parameters.Count == 0)
            {
                Hide();
                return;
            }

            var items = parameters.Select(p => new CompletionItem
            {
                Type = p.Type,
                Text = p.Name + ":",
                Value = p.CurrentValue.Replace("-", "????"),
                Description = p.Type
            }).ToList();

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
            Editor.Focus();
            Editor.GotoPosition(pos);
        }
        private string? GetArgumentValue(string paramName, ArgumentListSyntax args)
        {
            foreach (var arg in args.Arguments)
            {
                if (arg.NameColon?.Name.Identifier.ValueText == paramName)
                    return arg.Expression.ToString();
            }
            return null;
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

        public void Commit()
        {
            if (popup.Visible)
            {
                var text = popup.ListView.SelectedText;
                if (!string.IsNullOrEmpty(text))
                {
                    CommitSelection();
                }
                else
                {
                    CommitSelection();
                }
            }
        }

        private void CommitSelection()
        {
            string? selected = popup.ListView.SelectedItem?.Text;
            if (string.IsNullOrEmpty(selected)) { Hide(); return; }

            string prefix = GetPrefix();
            int pos = Editor.CurrentPosition;
            int start = pos - prefix.Length;
            Editor.DeleteRange(start, prefix.Length);
            Editor.InsertText(start, selected);
            Editor.GotoPosition(start + selected.Length);
            Hide();
        }

        private string GetPrefix()
        {
            int pos = Editor.CurrentPosition;
            int start = Editor.WordStartPosition(pos, true);
            return Editor.GetTextRange(start, pos - start);
        }


    }
}
