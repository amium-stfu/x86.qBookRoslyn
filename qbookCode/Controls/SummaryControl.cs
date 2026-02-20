using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qbookCode.Controls
{
    public class SummaryPopup : Control
    {
        private string _text;
        private Padding _padding = new Padding(5);

        public Font EditorFont
        {
            get => Font;
            set => Font = value;
        }

        public SummaryPopup()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);
            Font = new Font("Segoe UI", 9);
            BackColor = Color.White;
            ForeColor = Color.Black;
         
        }

        public void SetText(string text)
        {
            if (_text == text) return;
            _text = text;

            if (IsHandleCreated && Visible && Width > 0 && Height > 0)
                Invalidate();           // oder: Refresh(); für "sofort"
        }

        public Size GetPreferredSizeForText(string text, int width)
        {
            if (Theme.IsDark)
            {
                BackColor = Color.FromArgb(35, 35, 35); 
                ForeColor = Color.FromArgb(220, 220, 220);
            }
            else
            {
                BackColor = Color.FromArgb(200, 200, 200);
                ForeColor = Color.Black;

            }


            if (string.IsNullOrEmpty(text))
                return Size.Empty;

            var proposedSize = new Size(width - _padding.Horizontal, int.MaxValue);
            var textSize = TextRenderer.MeasureText(text, Font, proposedSize, TextFormatFlags.WordBreak | TextFormatFlags.GlyphOverhangPadding);

            return new Size(width, textSize.Height + _padding.Vertical);
        }


        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible && !string.IsNullOrEmpty(_text))
                Invalidate(); // oder Refresh();
        }

        

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(BackColor);

            if (!string.IsNullOrEmpty(_text))
            {
                var textRect = new Rectangle(
                    ClientRectangle.X + _padding.Left,
                    ClientRectangle.Y + _padding.Top,
                    ClientRectangle.Width - _padding.Horizontal,
                    ClientRectangle.Height - _padding.Vertical);

                TextRenderer.DrawText(g, _text, Font, textRect, ForeColor, TextFormatFlags.WordBreak | TextFormatFlags.GlyphOverhangPadding);
            }
        }
    }
}
