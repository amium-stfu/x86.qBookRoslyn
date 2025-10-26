using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace qbook.CodeEditor
{
    /// <summary>
    /// Custom Vertical Scrollbar for a WinForms TreeView.
    /// Fixes:
    /// - Hides native V-Scrollbar robustly (NativeWindow hook + ShowScrollBar on key messages)
    /// - Correct thumb sizing using pixel-based viewport (ItemHeight) and safe clamping (<100%)
    /// - Drag only when LMB is down; no unintended movement
    /// - Syncs on expand/collapse, keyboard, wheel, and native scroll messages
    /// </summary>
    public class TreeViewVerticalBar : UserControl
    {
        private TreeView tree;
        private Panel scrollBarPanel;
        private Panel scrollThumb;
        private bool dragging = false;
        private int dragOffsetY;
        private bool hideNative;
        private ScrollMessageFilter? msgFilter;
        private ScrollHiderWindow? hiderWnd;

        #region WinAPI – Hide native vertical scrollbar
        [DllImport("user32.dll")]
        private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);
        private const int SB_VERT = 1; // vertical scrollbar
        private const int WM_VSCROLL = 0x0115;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_NCCALCSIZE = 0x0083;
        private const int WM_SIZE = 0x0005;
        private const int WM_WINDOWPOSCHANGED = 0x0047;
        private const int WM_STYLECHANGED = 0x007D;
        private const int WM_PAINT = 0x000F;
        #endregion

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

        public TreeViewVerticalBar()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            Width = 12;

            scrollBarPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightGray
            };
            scrollBarPanel.Paint += (s, e) => e.Graphics.Clear(scrollBarPanel.BackColor);
            scrollBarPanel.MouseDown += ScrollBar_MouseDown;
            scrollBarPanel.Cursor = Cursors.Hand;

            scrollThumb = new Panel
            {
                Width = Width,
                Height = 40,
                BackColor = Color.DodgerBlue,
                Top = 0
            };
            scrollThumb.MouseDown += ScrollThumb_MouseDown;
            scrollThumb.MouseMove += ScrollThumb_MouseMove;
            scrollThumb.MouseUp += ScrollThumb_MouseUp;
            scrollThumb.Cursor = Cursors.SizeNS;

            scrollBarPanel.Controls.Add(scrollThumb);
            Controls.Add(scrollBarPanel);
        }

        public void Init(TreeView treeView, bool hideNativeScrollbar = false)
        {
            tree = treeView ?? throw new ArgumentNullException(nameof(treeView));
            hideNative = hideNativeScrollbar;
            tree.Scrollable = true; // ensure scrolling is allowed

            // Native hook for reliably hiding the OS scrollbar
            if (hideNative)
            {
                if (hiderWnd != null) { hiderWnd.ReleaseHandle(); hiderWnd = null; }
                hiderWnd = new ScrollHiderWindow(this);
                if (tree.IsHandleCreated) hiderWnd.AssignHandle(tree.Handle);
                tree.HandleCreated += (s, e) => { hiderWnd?.AssignHandle(tree.Handle); TryHideNativeVScrollBar(); };
                tree.HandleDestroyed += (s, e) => { hiderWnd?.ReleaseHandle(); };
            }

            // Sync on structural/content changes
            tree.SizeChanged += (s, e) => { TryHideNativeVScrollBar(); UpdateScrollBar(); };
            tree.AfterExpand += (s, e) => { TryHideNativeVScrollBar(); UpdateScrollBar(); };
            tree.AfterCollapse += (s, e) => { TryHideNativeVScrollBar(); UpdateScrollBar(); };
            tree.AfterSelect += (s, e) => BeginInvoke((Action)SyncScrollBar); // selection can autoscroll
            tree.NodeMouseClick += (s, e) => BeginInvoke((Action)SyncScrollBar);
            tree.MouseWheel += (s, e) => BeginInvoke((Action)SyncScrollBar);   // wheel scrolling
            tree.KeyDown += (s, e) => BeginInvoke((Action)SyncScrollBar);      // keyboard scrolling

            // Listen for native scroll messages to keep perfectly in sync
            msgFilter = new ScrollMessageFilter(tree, SyncScrollBar);
            Application.AddMessageFilter(msgFilter);

            // Initial state
            scrollThumb.Visible = false;
            TryHideNativeVScrollBar();
            if (tree.IsHandleCreated)
                BeginInvoke((Action)UpdateScrollBar);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (msgFilter != null)
                {
                    Application.RemoveMessageFilter(msgFilter);
                    msgFilter = null;
                }
                if (hiderWnd != null)
                {
                    try { hiderWnd.ReleaseHandle(); } catch { }
                    hiderWnd = null;
                }
            }
            base.Dispose(disposing);
        }

        private void TryHideNativeVScrollBar()
        {
            if (!hideNative) return;
            try { if (tree?.IsHandleCreated == true) ShowScrollBar(tree.Handle, SB_VERT, false); } catch { /* ignore */ }
        }

        private void UpdateScrollBar()
        {
            if (tree == null || !tree.IsHandleCreated)
            {
                scrollThumb.Visible = false;
                return;
            }

            int totalVisibleNodes = GetTotalVisibleNodeCount();
            int visibleSlots = GetViewportRowSlots();

            if (totalVisibleNodes <= 0)
            {
                scrollThumb.Visible = false;
                return;
            }

            // Compute thumb height using pixel-based viewport to avoid 100% thumb when slight overflow exists
            int trackHeightMax = Math.Max(1, scrollBarPanel.Height);
            int computed = (int)Math.Round((double)trackHeightMax * Math.Min(visibleSlots, totalVisibleNodes) / Math.Max(1, totalVisibleNodes));
            int minThumb = 20;
            int maxThumb = Math.Max(1, trackHeightMax - 2); // never 100%
            int thumbHeight = Math.Max(minThumb, Math.Min(computed, maxThumb));

            scrollThumb.Height = thumbHeight;
            scrollThumb.Visible = totalVisibleNodes > visibleSlots; // only show when scrolling is possible

            SyncScrollBar();
        }

        private void SyncScrollBar()
        {
            if (tree == null || !scrollThumb.Visible) return;

            int totalVisibleNodes = GetTotalVisibleNodeCount();
            int visibleSlots = GetViewportRowSlots();
            int topIndex = GetTopVisibleIndex();
            int max = Math.Max(totalVisibleNodes - visibleSlots, 1);
            int trackHeight = Math.Max(1, scrollBarPanel.Height - scrollThumb.Height);

            int newTop = (int)Math.Round((double)trackHeight * topIndex / max);
            scrollThumb.Top = Math.Max(0, Math.Min(trackHeight, newTop));
            scrollThumb.Width = Width; // follow parent width
        }

        private void ScrollThumb_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            dragging = true;
            dragOffsetY = e.Y;
            scrollThumb.Capture = true;
        }

        private void ScrollThumb_MouseMove(object sender, MouseEventArgs e)
        {
            if (!dragging || tree == null || (e.Button & MouseButtons.Left) == 0) return;

            int track = Math.Max(1, scrollBarPanel.Height - scrollThumb.Height);
            int newTop = Math.Max(0, Math.Min(track, scrollThumb.Top + e.Y - dragOffsetY));
            if (newTop == scrollThumb.Top) return;
            scrollThumb.Top = newTop;

            int totalVisibleNodes = GetTotalVisibleNodeCount();
            int visibleSlots = GetViewportRowSlots();
            int max = Math.Max(totalVisibleNodes - visibleSlots, 1);
            int targetIndex = (int)Math.Round((double)max * newTop / track);
            SetTopVisibleIndex(targetIndex);

            // After programmatic scroll, resync to reflect any snapping done by TreeView
            BeginInvoke((Action)SyncScrollBar);
        }

        private void ScrollThumb_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
            scrollThumb.Capture = false;
        }

        private void ScrollBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (tree == null || e.Button != MouseButtons.Left) return;
            if (scrollThumb.Bounds.Contains(e.Location)) return;

            int track = Math.Max(1, scrollBarPanel.Height - scrollThumb.Height);
            int newTop = Math.Max(0, Math.Min(track, e.Y - scrollThumb.Height / 2));
            scrollThumb.Top = newTop;

            int totalVisibleNodes = GetTotalVisibleNodeCount();
            int visibleSlots = GetViewportRowSlots();
            int max = Math.Max(totalVisibleNodes - visibleSlots, 1);
            int targetIndex = (int)Math.Round((double)max * newTop / track);
            SetTopVisibleIndex(targetIndex);
            BeginInvoke((Action)SyncScrollBar);
        }

        // ===== Visible / viewport helpers =====
        private int GetViewportRowSlots()
        {
            // Pixel-based viewport estimate (includes partially visible last row)
            int ih = Math.Max(1, tree.ItemHeight);
            int h = Math.Max(1, tree.ClientSize.Height);
            // ceil to account for partial row visibility (so scrolling becomes available slightly earlier)
            return (h + ih - 1) / ih;
        }

        private int GetTotalVisibleNodeCount()
        {
            if (tree.Nodes.Count == 0) return 0;
            int count = 0;
            TreeNode n = tree.Nodes[0];
            while (n != null)
            {
                count++;
                n = n.NextVisibleNode;
            }
            return count;
        }

        private int GetTopVisibleIndex()
        {
            if (tree.TopNode == null || tree.Nodes.Count == 0) return 0;

            int idx = 0;
            TreeNode n = tree.Nodes[0];
            while (n != null && n != tree.TopNode)
            {
                idx++;
                n = n.NextVisibleNode;
            }
            return idx;
        }

        private void SetTopVisibleIndex(int index)
        {
            if (tree.Nodes.Count == 0) return;

            TreeNode n = tree.Nodes[0];
            int i = 0;
            while (n != null && i < index)
            {
                n = n.NextVisibleNode;
                i++;
            }
            if (n != null)
            {
                tree.TopNode = n; // scrolls the TreeView
            }
        }

        /// <summary>
        /// Captures native scroll messages and syncs the thumb position.
        /// </summary>
        private sealed class ScrollMessageFilter : IMessageFilter
        {
            private readonly Control ctrl;
            private readonly Action onScroll;
            public ScrollMessageFilter(Control ctrl, Action onScroll)
            { this.ctrl = ctrl; this.onScroll = onScroll; }
            public bool PreFilterMessage(ref Message m)
            {
                if ((m.Msg == WM_VSCROLL || m.Msg == WM_MOUSEWHEEL) && ctrl != null && m.HWnd == ctrl.Handle)
                    ctrl.BeginInvoke(onScroll);
                return false;
            }
        }

        /// <summary>
        /// Hooks the TreeView window proc to repeatedly hide the native vertical scrollbar.
        /// </summary>
        private sealed class ScrollHiderWindow : NativeWindow
        {
            private readonly TreeViewVerticalBar owner;
            public ScrollHiderWindow(TreeViewVerticalBar owner) { this.owner = owner; }
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (owner.tree == null) return;
                if (m.Msg == WM_NCCALCSIZE || m.Msg == WM_SIZE || m.Msg == WM_WINDOWPOSCHANGED || m.Msg == WM_STYLECHANGED || m.Msg == WM_PAINT)
                {
                    owner.TryHideNativeVScrollBar();
                }
            }
        }
    }
}
