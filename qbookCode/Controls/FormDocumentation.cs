using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qbookCode.Controls
{
    public sealed class DocPopup : Form
    {
        private readonly Label _label;
        private int _maxWidth = 520;         // anpassbar
        private int _padding = 8;            // Innenabstand

 

        DocumentEditor? _editor;

        public DocPopup(DocumentEditor editor)
        {
            _editor = editor;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;

            // Optional: eigenes Theme
            BackColor = Color.FromArgb(40, 40, 40);
            Opacity = 0.98;
            _label = new Label
            {
                AutoSize = false,
                ForeColor = Color.Gainsboro,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9f),
            };

            Controls.Add(_label);
            Padding = new Padding(_padding);
        }

        public void SetText(string text)
        {
            _label.Text = text ?? string.Empty;
            LayoutToContent();
        }

        private void LayoutToContent()
        {
            BackColor = Theme.ButtonBackColor;
            _label.ForeColor = Theme.ButtonForeColor;
            _label.Font = _editor?.GetFont();



            int maxW = _maxWidth - (Padding.Left + Padding.Right);
            // Größe des Textes bestimmen (mit Wrap)
            var proposed = new Size(maxW, int.MaxValue);
            var sz = TextRenderer.MeasureText(_label.Text, _label.Font, proposed, TextFormatFlags.WordBreak);

            _label.Bounds = new Rectangle(Padding.Left, Padding.Top, maxW, sz.Height);
            this.Size = new Size(Math.Min(_maxWidth, sz.Width + Padding.Horizontal), sz.Height + Padding.Vertical);
        }

        protected override bool ShowWithoutActivation => true;

        // Optional: kleine Schattenkante
        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x00020000;
                var cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }
    }
}
