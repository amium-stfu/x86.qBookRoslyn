using System;
using System.Drawing;
using System.Windows.Forms;

namespace qbookCode.Controls.Scrollbars
{
    public class GridViewVerticalBar : UserControl
    {
        private DataGridView grid;
        private Panel scrollBarPanel;
        private Panel scrollThumb;
        private bool dragging = false;
        private int dragOffsetY;

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

        public GridViewVerticalBar()
        {
            this.Width = 12;

            scrollBarPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightGray
            };
            scrollBarPanel.Paint += (s, e) => e.Graphics.Clear(scrollBarPanel.BackColor);
            scrollBarPanel.MouseDown += ScrollBar_MouseDown;

            scrollThumb = new Panel
            {
                Width = this.Width,
                Height = 40,
                BackColor = Color.DodgerBlue,
                Top = 0
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
            grid.Scroll -= null;
            grid.RowsAdded -= null;
            grid.RowsRemoved -= null;
            grid.Resize -= null;
            grid.MouseWheel -= null;


            grid.Scroll += (s, e) => SyncScrollBar();
            grid.RowsAdded += (s, e) => UpdateScrollBar();
            grid.RowsRemoved += (s, e) => UpdateScrollBar();
            grid.Resize += (s, e) => UpdateScrollBar();
            grid.MouseWheel += Grid_MouseWheel;
            scrollThumb.Visible = false;
            UpdateScrollBar();

            scrollThumb.Width = Width;
        }



        public void UpdateScrollBar()
        {
            if (grid == null) return;
            if (grid.Rows.Count < 1) {
                scrollThumb.Visible = false;
                return;
                    };

            int totalRows = grid.RowCount;
            int visibleRows = grid.DisplayedRowCount(true);
            if (totalRows <= 0 || visibleRows <= 0) return;

            int thumbHeight = Math.Max(scrollBarPanel.Height * visibleRows / totalRows, 20);
            scrollThumb.Height = thumbHeight;
            scrollThumb.Visible = totalRows > visibleRows;

            SyncScrollBar();
        }
        private void SyncScrollBar()
        {
            if (grid == null || grid.RowCount == 0) return;

            int totalRows = grid.RowCount;
            int visibleRows = grid.DisplayedRowCount(true);
            int firstVisible = grid.FirstDisplayedScrollingRowIndex;
            int max = Math.Max(totalRows - visibleRows, 1);
            int trackHeight = scrollBarPanel.Height - scrollThumb.Height;

            scrollThumb.Top = trackHeight * firstVisible / max;
        }

        private void ScrollThumb_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragOffsetY = e.Y;
        }

        private void ScrollThumb_MouseMove(object sender, MouseEventArgs e)
        {
            if (!dragging || grid == null) return;

            int newTop = scrollThumb.Top + e.Y - dragOffsetY;
            newTop = Math.Max(0, Math.Min(scrollBarPanel.Height - scrollThumb.Height, newTop));
            scrollThumb.Top = newTop;

            int totalRows = grid.RowCount;
            int visibleRows = grid.DisplayedRowCount(true);
            int max = Math.Max(totalRows - visibleRows, 1);
            int firstVisible = max * newTop / (scrollBarPanel.Height - scrollThumb.Height);
            grid.FirstDisplayedScrollingRowIndex = Math.Min(firstVisible, totalRows - 1);
        }

        private void ScrollThumb_MouseUp(object sender, MouseEventArgs e) => dragging = false;

        private void ScrollBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (scrollThumb.Bounds.Contains(e.Location)) return;

            int newTop = Math.Max(0, Math.Min(scrollBarPanel.Height - scrollThumb.Height, e.Y - scrollThumb.Height / 2));
            scrollThumb.Top = newTop;

            int totalRows = grid.RowCount;
            int visibleRows = grid.DisplayedRowCount(true);
            int max = Math.Max(totalRows - visibleRows, 1);
            int firstVisible = max * newTop / (scrollBarPanel.Height - scrollThumb.Height);
            grid.FirstDisplayedScrollingRowIndex = Math.Min(firstVisible, totalRows - 1);
        }

        private void Grid_MouseWheel(object sender, MouseEventArgs e)
        {
            if (grid == null || grid.RowCount == 0) return;

            int newIndex = grid.FirstDisplayedScrollingRowIndex;

            // Scrollrichtung: e.Delta > 0 = nach oben, < 0 = nach unten
            if (e.Delta > 0)
                newIndex = Math.Max(0, newIndex - 1);
            else
                newIndex = Math.Min(grid.RowCount - 1, newIndex + 1);

            grid.FirstDisplayedScrollingRowIndex = newIndex;
            SyncScrollBar();
        }
    }
}
