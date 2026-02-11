
using System;
using System.Drawing;
using System.Windows.Forms;
using ScintillaNET;

namespace qbook.CodeEditor
{
    public class ScintillaHorizontalBar : UserControl
    {
        private Scintilla scintilla;
        private Panel scrollBarPanel;
        private Panel scrollThumb;
        private bool darkMode = true;
        private bool dragging = false;
        private int dragOffsetX;

        Color backColor = Color.White;
        public Color SetBackColor
        {
            get { return backColor; }
            set
            {
                scrollBarPanel.BackColor = value;
                this.Invalidate();
            }
        }
        Color foreColor = Color.Black;
        public Color SetForeColor
        {
            get { return foreColor; }
            set { scrollThumb.BackColor = value; this.Invalidate(); }
        }

        public ScintillaHorizontalBar()
        {
            this.Size = new Size(600, 400);

            scrollBarPanel = new Panel();
            scrollBarPanel.Dock = DockStyle.Fill;
            scrollBarPanel.Height = this.Height;
            scrollBarPanel.BackColor = Color.LightGray;
            scrollBarPanel.Paint += (s, e) => DrawScrollBar(e.Graphics);
            scrollBarPanel.MouseDown += ScrollBar_MouseDown;

            scrollThumb = new Panel();
            scrollThumb.Height = scrollBarPanel.Height;
            scrollThumb.Width = 40;
            scrollThumb.BackColor = Color.DarkBlue;
            scrollThumb.Left = 0;
            scrollThumb.Top = 0;
            scrollThumb.MouseDown += ScrollThumb_MouseDown;
            scrollThumb.MouseMove += ScrollThumb_MouseMove;
            scrollThumb.MouseUp += ScrollThumb_MouseUp;

            scrollBarPanel.Controls.Add(scrollThumb);
            this.Controls.Add(scrollBarPanel);
        }

        public void Init(Scintilla editor)
        {
            this.scintilla = editor;
            scintilla.UpdateUI += (s, e) => UpdateScrollBar();
            scintilla.Resize += (s, e) => UpdateScrollBar();
            UpdateScrollBar();
        }

        private void DrawScrollBar(Graphics g)
        {
            g.Clear(scrollBarPanel.BackColor);
        }

        private void UpdateScrollBar()
        {
            if (scintilla == null) return;
            int maxX = scintilla.ScrollWidth;
            int visibleX = scintilla.ClientRectangle.Width;
            int max = Math.Max(maxX - visibleX, 1);
            int thumbWidth = Math.Max(scrollBarPanel.Width * visibleX / maxX, 20);
            scrollThumb.Width = thumbWidth;

            scrollThumb.Visible = maxX < visibleX ? false : true;

            SyncScrollBar();
        }

        private void SyncScrollBar()
        {
            if (scintilla == null) return;
            int xOffset = scintilla.XOffset;
            int maxX = scintilla.ScrollWidth;
            int visibleX = scintilla.ClientRectangle.Width;
            int max = Math.Max(maxX - visibleX, 1);
            int trackWidth = scrollBarPanel.Width - scrollThumb.Width;
            scrollThumb.Left = trackWidth * xOffset / max;
        }

        private void ScrollThumb_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragOffsetX = e.X;
        }

        private void ScrollThumb_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging && scintilla != null)
            {
                int newLeft = scrollThumb.Left + e.X - dragOffsetX;
                newLeft = Math.Max(0, Math.Min(scrollBarPanel.Width - scrollThumb.Width, newLeft));
                scrollThumb.Left = newLeft;

                int maxX = scintilla.ScrollWidth;
                int visibleX = scintilla.ClientRectangle.Width;
                int max = Math.Max(maxX - visibleX, 1);
                int xOffset = max * newLeft / (scrollBarPanel.Width - scrollThumb.Width);
                scintilla.XOffset = xOffset;
            }
        }

        private void ScrollThumb_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void ScrollBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (!scrollThumb.Bounds.Contains(e.Location) && scintilla != null)
            {
                int newLeft = Math.Max(0, Math.Min(scrollBarPanel.Width - scrollThumb.Width, e.X - scrollThumb.Width / 2));
                scrollThumb.Left = newLeft;

                int maxX = scintilla.ScrollWidth;
                int visibleX = scintilla.ClientRectangle.Width;
                int max = Math.Max(maxX - visibleX, 1);
                int xOffset = max * newLeft / (scrollBarPanel.Width - scrollThumb.Width);
                scintilla.XOffset = xOffset;
            }
        }
    }
}
