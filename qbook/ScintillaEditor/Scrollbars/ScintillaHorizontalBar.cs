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

        private bool dragging = false;
        private int dragOffsetX;

        public Color SetBackColor
        {
            get => scrollBarPanel.BackColor;
            set => scrollBarPanel.BackColor = value;
        }

        public Color SetForeColor
        {
            get => scrollThumb.BackColor;
            set => scrollThumb.BackColor = value;
        }

        public ScintillaHorizontalBar()
        {
            Height = 16;

            scrollBarPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightGray
            };

            scrollBarPanel.Paint += (s, e) => e.Graphics.Clear(scrollBarPanel.BackColor);
            scrollBarPanel.MouseDown += ScrollBar_MouseDown;

            scrollThumb = new Panel
            {
                Height = this.Height,
                Width = 40,
                BackColor = Color.DodgerBlue,
                Left = 0,
                Top = 0
            };

            scrollThumb.MouseDown += ScrollThumb_MouseDown;
            scrollThumb.MouseMove += ScrollThumb_MouseMove;
            scrollThumb.MouseUp += ScrollThumb_MouseUp;

            scrollBarPanel.Controls.Add(scrollThumb);
            Controls.Add(scrollBarPanel);
        }

        // ---------------------------------------------------------------
        // INIT
        // ---------------------------------------------------------------
        public void Init(Scintilla editor)
        {
            scintilla = editor;

            scintilla.WrapMode = WrapMode.None;

            scintilla.TextChanged += (s, e) =>
            {
                UpdateMaxScrollWidth();
                UpdateScrollBar();
            };

            scintilla.ZoomChanged += (s, e) =>
            {
                UpdateMaxScrollWidth();
                UpdateScrollBar();
            };

            scintilla.Resize += (s, e) => UpdateScrollBar();
            scintilla.UpdateUI += (s, e) => SyncScrollBar();

            UpdateMaxScrollWidth();
            UpdateScrollBar();
        }


        // ---------------------------------------------------------------
        // CALCULATE MAX WIDTH
        // ---------------------------------------------------------------
        private void UpdateMaxScrollWidth()
        {
            if (scintilla == null)
                return;

            int maxWidth = 0;

            for (int i = 0; i < scintilla.Lines.Count; i++)
            {
                string txt = scintilla.Lines[i].Text;

                // Scintilla-eigene Messung, zoom-aware
                int w = scintilla.TextWidth(Style.Default, txt);

                if (w > maxWidth)
                    maxWidth = w;
            }

            // etwas Luft geben, damit man wirklich ans Ende kommt
            int padding = scintilla.TextWidth(Style.Default, "WWWWWWWWW");

            // mindestens so breit wie der sichtbare Bereich
            scintilla.ScrollWidth = Math.Max(maxWidth + padding, scintilla.ClientRectangle.Width);
        }


        // ---------------------------------------------------------------
        // UPDATE SCROLLBAR
        // ---------------------------------------------------------------
        public void UpdateScrollBar()
        {
            if (scintilla == null) return;

            int maxX = scintilla.ScrollWidth;
            int visibleX = scintilla.ClientRectangle.Width;

            if (maxX <= 0 || visibleX <= 0)
            {
                scrollThumb.Visible = false;
                return;
            }

            scrollThumb.Visible = maxX > visibleX;

            // Thumb width proportional to visible area
            int thumbWidth = Math.Max(scrollBarPanel.Width * visibleX / maxX, 20);

            scrollThumb.Width = thumbWidth;

            SyncScrollBar();
        }

        private void SyncScrollBar()
        {
            if (scintilla == null)
                return;

            int maxX = scintilla.ScrollWidth;
            int visibleX = scintilla.ClientRectangle.Width;

            int maxOffset = maxX - visibleX;
            if (maxOffset <= 0)
            {
                scrollThumb.Left = 0;
                return;
            }

            int trackWidth = scrollBarPanel.Width - scrollThumb.Width;
            if (trackWidth <= 0)
            {
                scrollThumb.Left = 0;
                return;
            }

            scrollThumb.Left = trackWidth * scintilla.XOffset / maxOffset;
        }


        // ---------------------------------------------------------------
        // INTERACTION
        // ---------------------------------------------------------------
        private void ScrollThumb_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragOffsetX = e.X;
        }

        private void ScrollThumb_MouseMove(object sender, MouseEventArgs e)
        {
            if (!dragging || scintilla == null)
                return;

            int trackWidth = scrollBarPanel.Width - scrollThumb.Width;
            if (trackWidth <= 0)
                return;

            int newLeft = scrollThumb.Left + e.X - dragOffsetX;
            newLeft = Math.Max(0, Math.Min(trackWidth, newLeft));
            scrollThumb.Left = newLeft;

            int maxX = scintilla.ScrollWidth;
            int visibleX = scintilla.ClientRectangle.Width;
            int maxOffset = maxX - visibleX;
            if (maxOffset <= 0)
                return;

            scintilla.XOffset = maxOffset * newLeft / trackWidth;
        }


        private void ScrollThumb_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void ScrollBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (scrollThumb.Bounds.Contains(e.Location) || scintilla == null)
                return;

            int newLeft = Math.Max(0,
                Math.Min(scrollBarPanel.Width - scrollThumb.Width,
                         e.X - scrollThumb.Width / 2));

            scrollThumb.Left = newLeft;

            int maxX = scintilla.ScrollWidth;
            int visibleX = scintilla.ClientRectangle.Width;
            int maxOffset = Math.Max(maxX - visibleX, 1);

            scintilla.XOffset = maxOffset * newLeft / (scrollBarPanel.Width - scrollThumb.Width);
        }
    }
}
