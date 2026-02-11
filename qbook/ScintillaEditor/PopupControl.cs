using qbook.ScintillaEditor.Scrollbars;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace qbook.ScintillaEditor
{

    public class CompletionItem
    {
        public string Text { get; set; }
        public string Type { get; set; } = null; // optional, z.B. "method", "property", "class" etc.
        public Image Icon { get; set; }
        public string Description { get; set; } // optional
        public string Value { get; set; } = null;

    }

    public class PopupControl : Control
    {
        public oPage Page { get; private set; }
        public object DynamicInstance { get; private set; }

        private List<CompletionItem> _items = new();
        private int _selectedIndex = -1;
        private int _itemHeight = 20;

        private PopupVerticalBar _bar;
        private int _firstVisibleIndex = 0; // früher: _scrollBar.Value

        public event Action<CompletionItem> ItemSelected;

        public int SelectedIndex => _selectedIndex;

        public CompletionItem SelectedItem =>
            (_selectedIndex >= 0 && _selectedIndex < _items.Count) ? _items[_selectedIndex] : null;

        public string SelectedText => SelectedItem?.Text;

        private Font _editorFont = new Font("Consolas", 10);
        public Font EditorFont
        {
            get => _editorFont;
            set
            {
                _editorFont = value;
                Font = value;
                AutoSizeHeight();
            }
        }


        public PopupControl()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);

            Font = new Font("Consolas", 10);

            _bar = new PopupVerticalBar
            {
                Dock = DockStyle.Right,
                TrackColor = Color.FromArgb(230, 230, 230),
                ThumbColor = Color.FromArgb(90, 150, 255)
            };

            // Delegates mit der Liste verdrahten
            _bar.GetTotalItems = () => _items.Count;
            _bar.GetVisibleItems = () => Math.Max(1, Height / _itemHeight);
            _bar.GetFirstVisible = () => _firstVisibleIndex;
            _bar.SetFirstVisible = (i) => { _firstVisibleIndex = Math.Max(0, Math.Min(i, Math.Max(0, _items.Count - 1))); Invalidate(); };
            Controls.Add(_bar);

        }

        public void SetItems(List<CompletionItem> items)
        {
            _items = items ?? new();
            _selectedIndex = _items.Count > 0 ? Math.Max(0, Math.Min(_selectedIndex, _items.Count - 1)) : -1;
            _firstVisibleIndex = 0; // bei neuem Satz nach oben
            UpdateScrollBar();
            Invalidate();
        }
        private void UpdateScrollBar()
        {
            if (_bar != null)
            {
                _bar.Visible = _items.Count > (Height / _itemHeight);
                _bar.Sync();
            }
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScrollBar();
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_items.Count == 0) return;
            int dir = e.Delta > 0 ? -1 : 1;
            int visible = Math.Max(1, Height / _itemHeight);
            _firstVisibleIndex = Math.Max(0, Math.Min(_firstVisibleIndex + dir, Math.Max(0, _items.Count - visible)));
            _bar.Sync();
            Invalidate();
        }
        private void ScrollIntoView()
        {
            int visible = Math.Max(1, Height / _itemHeight);
            int lastVisible = _firstVisibleIndex + visible - 1;
            if (_selectedIndex < _firstVisibleIndex)
                _firstVisibleIndex = _selectedIndex;
            else if (_selectedIndex > lastVisible)
                _firstVisibleIndex = _selectedIndex - visible + 1;
            _firstVisibleIndex = Math.Max(0, Math.Min(_firstVisibleIndex, Math.Max(0, _items.Count - visible)));
            _bar.Sync();
        }
        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Enter)
                return true;
            return base.IsInputKey(keyData);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_items.Count == 0) return;

            switch (e.KeyCode)
            {
                case Keys.Up:
                    _selectedIndex = Math.Max(0, _selectedIndex - 1);
                    ScrollIntoView();
                    Invalidate();
                    e.Handled = true;
                    break;
                case Keys.Down:
                    _selectedIndex = Math.Min(_items.Count - 1, _selectedIndex + 1);
                    ScrollIntoView();
                    Invalidate();
                    e.Handled = true;
                    break;
                case Keys.Enter:
                    if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
                        ItemSelected?.Invoke(_items[_selectedIndex]);
                    e.Handled = true;
                    break;
            }

            base.OnKeyDown(e);
        }

        // Theme-Properties
        public Color ItemBackColor { get; set; } = Color.White;
        public Color ItemForeColor { get; set; } = Color.Black;
        public Color ItemBackColorSelected { get; set; } = Color.FromArgb(204, 232, 255); // VS-ähnlich
        public Color ItemForeColorSelected { get; set; } = Color.Black;
        public Color ControlBackColor { get; set; } = Color.White; // Hintergrund der Liste

        public void ApplyLightTheme()
        {
            ControlBackColor = Color.FromArgb(200,200,200);
            ItemBackColor = Color.FromArgb(240, 240, 240);
            ItemForeColor = Color.Black;
            ItemBackColorSelected = Color.Orange;
            ItemForeColorSelected = Color.Black;
            _bar.TrackColor = Color.FromArgb(230, 230, 230);
            _bar.ThumbColor = Color.FromArgb(150, 150, 150);
            Invalidate();
        }

        public void ApplyDarkTheme()
        {
            ControlBackColor = Color.FromArgb(25, 25, 25);
            ItemBackColor = Color.FromArgb(35, 35, 35);
            ItemForeColor = Color.FromArgb(220, 220, 220);
            ItemBackColorSelected = Color.FromArgb(60, 60, 60);
            ItemForeColorSelected = Color.White;
            _bar.TrackColor = Color.FromArgb(45, 45, 45);
            _bar.ThumbColor = Color.FromArgb(120, 120, 120);
            Invalidate();
        }

        public int TypeWidth = 0; // in %
        public int IconWidth = 6; // in %
        public int DescriptionWidth = 0; // in %
        public int TextWidth = 90; // in %
        public int ValueWidth = 0;
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(ControlBackColor);

        

            int visible = Math.Max(1, Height / _itemHeight);
            for (int i = 0; i < visible; i++)
            {
                int itemIndex = _firstVisibleIndex + i;
                if (itemIndex >= _items.Count) break;
                var item = _items[itemIndex];
                var rect = new Rectangle(0, i * _itemHeight, Width - (_bar.Visible ? _bar.Width : 0), _itemHeight);

                // Hintergrund je nach Auswahl
                using (var bg = new SolidBrush(itemIndex == _selectedIndex ? ItemBackColorSelected : ItemBackColor))
                    g.FillRectangle(bg, rect);

                // Icon
                int iS = (int)(Width / 100 * IconWidth);
                if (IconWidth > 0)
                {
                    
                    if (item.Icon != null)
                        g.DrawImage(item.Icon, rect.X + 2, rect.Y + 2, iS, iS);
                }

                // Type
                int tyS = (int)(Width / 100 * TypeWidth);
                if (TypeWidth > 0)
                {
                    var typeRect = new Rectangle(rect.X + 2 + iS , rect.Y, tyS, rect.Height);
                    var foreType = itemIndex == _selectedIndex ? ItemForeColorSelected : ItemForeColor;
                    TextRenderer.DrawText(g, item.Type, EditorFont, typeRect, foreType,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }

                // Text
                int tS = (int)(Width / 100 * TextWidth);
                var textRect = new Rectangle(rect.X + 2 + iS + tyS, rect.Y, tS, rect.Height);
                var fore = itemIndex == _selectedIndex ? ItemForeColorSelected : ItemForeColor;
                TextRenderer.DrawText(g, item.Text, EditorFont, textRect, fore,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                // Description
                int dS = (int)(Width / 100 * DescriptionWidth);
                if (DescriptionWidth > 0)
                {
                    var descRect = new Rectangle(rect.X + 2 + iS + tyS + tS, rect.Y, dS, rect.Height);
                    var foreDesc = itemIndex == _selectedIndex ? ItemForeColorSelected : ItemForeColor;
                    TextRenderer.DrawText(g, item.Description, EditorFont, descRect, foreDesc,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }
                // Value
                int vS = (int)(Width / 100 * ValueWidth);
                if (ValueWidth > 0)
                {
                    var valRect = new Rectangle(rect.X + 2 + iS + tyS + tS + dS, rect.Y, vS, rect.Height);
                    var foreVal = itemIndex == _selectedIndex ? ItemForeColorSelected : ItemForeColor;
                    TextRenderer.DrawText(g, item.Value, EditorFont, valRect, foreVal,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }
            }
        }

        public void SelectNext()
        {
            if (_items.Count == 0) return;
            _selectedIndex = Math.Min(_items.Count - 1, _selectedIndex + 1);
            ScrollIntoView();
            Invalidate();
        }
        public void SelectPrevious()
        {
            if (_items.Count == 0) return;
            _selectedIndex = Math.Max(0, _selectedIndex - 1);
            ScrollIntoView();
            Invalidate();
        }

        private void RecalcItemHeight()
        {
            // TextRenderer ist für WinForms-Messung zuverlässiger als Font.Height
            var h = TextRenderer.MeasureText("Wg", EditorFont).Height;
            _itemHeight = Math.Max(16, h + 2); // kleiner Padding, min 16
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            RecalcItemHeight();
            UpdateScrollBar();
            Invalidate();
        }

        public int GetAutoHeightForItems(int maxVisibleItems = 10)
        {
            int visible = Math.Min(_items.Count, Math.Max(1, maxVisibleItems));
            int result = visible * _itemHeight;
            return result;
        }
        public void AutoSizeHeight(int max = 10)
        {
            this.Height = GetAutoHeightForItems(max);
            UpdateScrollBar();
            Invalidate();
        }
    }


}
