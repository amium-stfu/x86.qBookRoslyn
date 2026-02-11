
using System;
using System.Drawing;
using System.Windows.Forms;

namespace qbookCode.Controls.Scrollbars
{
   

    public class PopupVerticalBar : UserControl
    {
        private Panel track;
        private Panel thumb;
        private bool dragging;
        private int dragOffsetY;

        // Delegates zum Binden an die Host-Liste
        public Func<int> GetTotalItems { get; set; } = () => 0;
        public Func<int> GetVisibleItems { get; set; } = () => 0;
        public Func<int> GetFirstVisible { get; set; } = () => 0;
        public Action<int> SetFirstVisible { get; set; } = _ => { };

        public Color TrackColor { get => track.BackColor; set { track.BackColor = value; Invalidate(); } }
        public Color ThumbColor { get => thumb.BackColor; set { thumb.BackColor = value; Invalidate(); } }

        public PopupVerticalBar()
        {
            Width = 12;
            MinimumSize = new Size(10, 20);
            DoubleBuffered = true;

            track = new Panel { Dock = DockStyle.Fill, BackColor = Color.LightGray };
            track.Paint += (s, e) => e.Graphics.Clear(track.BackColor);
            track.MouseDown += Track_MouseDown;

            thumb = new Panel { Width = Width, Height = 40, BackColor = Color.DodgerBlue, Top = 0 };
            thumb.MouseDown += Thumb_MouseDown;
            thumb.MouseMove += Thumb_MouseMove;
            thumb.MouseUp += Thumb_MouseUp;

            track.Controls.Add(thumb);
            Controls.Add(track);
        }

        public void Sync()
        {
            if(thumb == null) return;
            int total = Math.Max(0, GetTotalItems());
            int visible = Math.Max(0, GetVisibleItems());
            if (total <= 0 || visible <= 0)
            {
                thumb.Visible = false;
                return;
            }

            int trackHeight = Math.Max(0, track.Height);
            int thumbHeight = Math.Max(trackHeight * visible / Math.Max(total, 1), 20);
            thumb.Height = Math.Min(trackHeight, thumbHeight);
            thumb.Visible = total > visible;

            int first = Math.Min(Math.Max(0, GetFirstVisible()), Math.Max(0, total - visible));
            int range = Math.Max(1, total - visible);
            int maxTop = Math.Max(0, trackHeight - thumb.Height);
            thumb.Top = range == 0 ? 0 : (int)Math.Round((double)first * maxTop / range);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Sync();
        }

        private void Thumb_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragOffsetY = e.Y;
        }

        private void Thumb_MouseMove(object sender, MouseEventArgs e)
        {
            if (!dragging) return;
            int maxTop = Math.Max(0, track.Height - thumb.Height);
            int newTop = Math.Max(0, Math.Min(maxTop, thumb.Top + e.Y - dragOffsetY));
            thumb.Top = newTop;

            int total = Math.Max(0, GetTotalItems());
            int visible = Math.Max(0, GetVisibleItems());
            int range = Math.Max(1, total - visible);
            int first = range == 0 ? 0 : (int)Math.Round((double)newTop * range / Math.Max(1, maxTop));
            SetFirstVisible(Math.Min(first, Math.Max(0, total - 1)));
        }

        private void Thumb_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void Track_MouseDown(object sender, MouseEventArgs e)
        {
            if (thumb.Bounds.Contains(e.Location)) return;
            int maxTop = Math.Max(0, track.Height - thumb.Height);
            int newTop = Math.Max(0, Math.Min(maxTop, e.Y - thumb.Height / 2));
            thumb.Top = newTop;

            int total = Math.Max(0, GetTotalItems());
            int visible = Math.Max(0, GetVisibleItems());
            int range = Math.Max(1, total - visible);
            int first = range == 0 ? 0 : (int)Math.Round((double)newTop * range / Math.Max(1, maxTop));
            SetFirstVisible(Math.Min(first, Math.Max(0, total - 1)));
        }
    }

}
