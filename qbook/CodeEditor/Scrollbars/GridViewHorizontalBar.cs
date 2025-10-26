using System;
using System.Drawing;
using System.Windows.Forms;

namespace qbook.CodeEditor
{
    public class GridViewHorizontalBar : UserControl
    {
        private DataGridView grid;
        private Panel scrollBarPanel;
        private Panel scrollThumb;
        private bool dragging = false;
        private int dragOffsetX;

        public Color SetBackColor
        {
            get => scrollBarPanel.BackColor;
            set { scrollBarPanel.BackColor = value; Invalidate(); }
        }
        public Color SetForeColor
        {
            get => scrollThumb.BackColor;
            set { scrollThumb.BackColor = value; Invalidate(); }
        }

        public GridViewHorizontalBar()
        {
            this.Height = 12;

            scrollBarPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightGray
            };
            scrollBarPanel.Paint += (s, e) => e.Graphics.Clear(scrollBarPanel.BackColor);
            scrollBarPanel.MouseDown += ScrollBar_MouseDown;

            scrollThumb = new Panel
            {
                Width = 40,
                Height = this.Height,
                BackColor = Color.DodgerBlue,
                Left = 0
            };
            scrollThumb.MouseDown += ScrollThumb_MouseDown;
            scrollThumb.MouseMove += ScrollThumb_MouseMove;
            scrollThumb.MouseUp += ScrollThumb_MouseUp;

            scrollBarPanel.Controls.Add(scrollThumb);
            Controls.Add(scrollBarPanel);
        }

        public void Init(DataGridView gridView)
        {
            grid = gridView;
            grid.Scroll += (s, e) => SyncScrollBar();
            grid.ColumnAdded += (s, e) => UpdateScrollBar();
            grid.ColumnRemoved += (s, e) => UpdateScrollBar();
            grid.Resize += (s, e) => UpdateScrollBar();
            scrollThumb.Visible = false;
            UpdateScrollBar();
        }

        private void UpdateScrollBar()
        {
            if (grid == null) return;
            if (grid.Rows.Count < 1) return;

            int totalCols = grid.ColumnCount;
            int visibleCols = grid.DisplayedColumnCount(true);
            if (totalCols <= 0 || visibleCols <= 0) return;

            int thumbWidth = Math.Max(scrollBarPanel.Width * visibleCols / totalCols, 20);
            scrollThumb.Width = thumbWidth;
            scrollThumb.Visible = totalCols > visibleCols;

            SyncScrollBar();
        }

        private void SyncScrollBar()
        {
            if (grid == null || grid.ColumnCount == 0) return;

            int totalCols = grid.ColumnCount;
            int visibleCols = grid.DisplayedColumnCount(true);
            int firstVisible = grid.FirstDisplayedScrollingColumnIndex;
            int max = Math.Max(totalCols - visibleCols, 1);
            int trackWidth = scrollBarPanel.Width - scrollThumb.Width;

            scrollThumb.Left = trackWidth * firstVisible / max;
        }

        private void ScrollThumb_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragOffsetX = e.X;
        }

        private void ScrollThumb_MouseMove(object sender, MouseEventArgs e)
        {
            if (!dragging || grid == null) return;

            int newLeft = scrollThumb.Left + e.X - dragOffsetX;
            newLeft = Math.Max(0, Math.Min(scrollBarPanel.Width - scrollThumb.Width, newLeft));
            scrollThumb.Left = newLeft;

            int totalCols = grid.ColumnCount;
            int visibleCols = grid.DisplayedColumnCount(true);
            int max = Math.Max(totalCols - visibleCols, 1);
            int firstVisible = max * newLeft / (scrollBarPanel.Width - scrollThumb.Width);
            grid.FirstDisplayedScrollingColumnIndex = Math.Min(firstVisible, totalCols - 1);
        }

        private void ScrollThumb_MouseUp(object sender, MouseEventArgs e) => dragging = false;

        private void ScrollBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (scrollThumb.Bounds.Contains(e.Location)) return;

            int newLeft = Math.Max(0, Math.Min(scrollBarPanel.Width - scrollThumb.Width, e.X - scrollThumb.Width / 2));
            scrollThumb.Left = newLeft;

            int totalCols = grid.ColumnCount;
            int visibleCols = grid.DisplayedColumnCount(true);
            int max = Math.Max(totalCols - visibleCols, 1);
            int firstVisible = max * newLeft / (scrollBarPanel.Width - scrollThumb.Width);
            grid.FirstDisplayedScrollingColumnIndex = Math.Min(firstVisible, totalCols - 1);
        }
    }
}
