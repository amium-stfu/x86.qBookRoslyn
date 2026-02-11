
using System;
using System.Drawing;
using System.Windows.Forms;
using ScintillaNET;

namespace qbookCode.Controls.Scrollbars
{
    public class ScintillaVerticalBar : UserControl
    {
        private Scintilla scintilla;
        private Panel scrollBarPanel;
        private Panel scrollThumb;

        public bool SyncPause = false;
 

        Color backColor = Color.White;
       public Color SetBackColor { 
            get { return backColor; } 
            set {
                scrollBarPanel.BackColor = value; 
                this.Invalidate(); 
            }
        }
        Color foreColor = Color.Black;
        public Color SetForeColor { 
            get { return foreColor; } 
            set { scrollThumb.BackColor = value; this.Invalidate(); }
        }

        public ScintillaVerticalBar()
        {
            this.Size = new Size(600, 400);

            scrollBarPanel = new Panel();
            scrollBarPanel.Width = 12;
            scrollBarPanel.Dock = DockStyle.Fill;
            scrollBarPanel.Paint += (s, e) => DrawScrollBar(e.Graphics);
            scrollBarPanel.MouseDown += ScrollBar_MouseDown;

            scrollThumb = new Panel();
            scrollThumb.Width = scrollBarPanel.Width;
            scrollThumb.Height = 40;
            scrollThumb.BackColor = Color.LightBlue;
            scrollThumb.Top = 0;
            scrollThumb.Left = 0;
            scrollThumb.MouseDown += ScrollThumb_MouseDown;
            scrollThumb.MouseMove += ScrollThumb_MouseMove;
            scrollThumb.MouseUp += ScrollThumb_MouseUp;

            scrollBarPanel.Controls.Add(scrollThumb);
            this.Controls.Add(scrollBarPanel);
        }

        public void Init(Scintilla editor)
        {
            this.scintilla = editor;
          
            scintilla.TextChanged += (s, e) => UpdateScrollBar();
            scintilla.KeyDown += (s, e) => UpdateScrollBar();
            scintilla.MouseWheel += (s, e) => UpdateScrollBar();
            scintilla.MouseMove += (s, e) => UpdateScrollBar();
            scintilla.Resize += (s, e) => UpdateScrollBar();
            scintilla.ZoomChanged += (s, e) => UpdateScrollBar();
            UpdateScrollBar();
            scrollThumb.Width = Width;


        }

        private void DrawScrollBar(Graphics g)
        {
            g.Clear(scrollBarPanel.BackColor);
        }

        public void UpdateScrollBar()
        {
            try
            {
                if (SyncPause) return;

                int lines = scintilla.Lines.Count;
                for (int i = 0; i < lines; i++)
                {
                    if (!scintilla.Lines[i].Visible)
                    {
                        lines--;
                    }
                }


                int visibleLines = scintilla.LinesOnScreen;
                int max = Math.Max(lines - visibleLines, 1);
                int thumbHeight = Math.Max(scrollBarPanel.Height * visibleLines / lines, 20);
                scrollThumb.Height = thumbHeight;

                scrollThumb.Visible = lines < visibleLines ? false : true;

                SyncScrollBar();
            }
            catch (Exception)
            {
                // Ignore exceptions during scrollbar update
            }
        }

        private void SyncScrollBar()
        {
            if (SyncPause) return;
            int lines = scintilla.Lines.Count;
            //for (int i = 0; i < lines; i++)
            //{
            //    if (scintilla.Lines[i].Visible)
            //    {
            //        lines--;
            //    }
            //}

            int visibleLines = scintilla.LinesOnScreen;
            int firstVisible = scintilla.FirstVisibleLine;
            int max = Math.Max(lines - visibleLines, 1);
            int trackHeight = scrollBarPanel.Height - scrollThumb.Height;
            scrollThumb.Top = trackHeight * firstVisible / max;
            scrollThumb.Width = this.Width;
        }

        private bool dragging = false;
        private int dragOffsetY;

        private void ScrollThumb_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragOffsetY = e.Y;
        }

        private void ScrollThumb_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                int newTop = scrollThumb.Top + e.Y - dragOffsetY;
                newTop = Math.Max(0, Math.Min(scrollBarPanel.Height - scrollThumb.Height, newTop));
                scrollThumb.Top = newTop;

                int lines = scintilla.Lines.Count;
                int visibleLines = scintilla.LinesOnScreen;
                int max = Math.Max(lines - visibleLines, 1);
                int firstVisible = max * newTop / (scrollBarPanel.Height - scrollThumb.Height);
                scintilla.FirstVisibleLine = firstVisible;
            }
        }

        private void ScrollThumb_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void ScrollBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (!scrollThumb.Bounds.Contains(e.Location))
            {
                int newTop = Math.Max(0, Math.Min(scrollBarPanel.Height - scrollThumb.Height, e.Y - scrollThumb.Height / 2));
                scrollThumb.Top = newTop;

                int lines = scintilla.Lines.Count;
                //for (int i = 0; i < lines; i++)
                //{
                //    if (scintilla.Lines[i].Visible)
                //    {
                //        lines--;
                //    }
                //}



                int visibleLines = scintilla.LinesOnScreen;
                int max = Math.Max(lines - visibleLines, 1);
                int firstVisible = max * newTop / (scrollBarPanel.Height - scrollThumb.Height);
                scintilla.FirstVisibleLine = firstVisible;
            }
        }
    }
}
