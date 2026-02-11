using System;
using System.Drawing;
using System.Windows.Forms;

namespace qbook.CodeEditor
{
    internal enum SearchScope { Document, Project }

    internal sealed class SearchBar : UserControl
    {
        private readonly TextBox _txtSearch = new() { BorderStyle = BorderStyle.FixedSingle, Width = 180};
        private readonly ComboBox _cmbScope = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 90 };
        private readonly Button _btnNext = new() { Text = "Next", Width = 55};
        private readonly Button _btnClose = new() { Text = "X", Width = 28, FlatStyle = FlatStyle.Flat };

        public event EventHandler? FindNextRequested;
        public event EventHandler? CloseRequested;

        public string SearchText => _txtSearch.Text;
        public SearchScope Scope => (SearchScope)_cmbScope.SelectedItem!;

        Color HoverColor = Color.FromArgb(200, 200, 200);
        public void DarkTheme()
        {
            BackColor = Color.FromArgb(45, 45, 48);
            _txtSearch.BackColor = Color.FromArgb(30, 30, 30);
            _txtSearch.ForeColor = Color.White;
            _cmbScope.BackColor = Color.Black;
            _cmbScope.ForeColor = Color.FromArgb(220, 220, 220);
            _btnNext.BackColor = Color.Transparent;
            _btnNext.ForeColor = Color.White;
            _btnClose.ForeColor = Color.FromArgb(220, 220, 220);
            _btnClose.FlatAppearance.BorderColor = Color.Black;
            this.ActiveControl = null;
        }
        public void LightTheme()
        {
            BackColor = Color.FromArgb(245, 245, 245);
            _txtSearch.BackColor = Color.White;
            _txtSearch.ForeColor = Color.Black;
            _cmbScope.BackColor = Color.White;
            _cmbScope.ForeColor = Color.Black;
            _btnNext.BackColor = Color.Transparent;
            _btnNext.ForeColor = Color.Black;
            _btnClose.ForeColor = Color.Black;
            _btnClose.FlatAppearance.BorderColor = Color.White;

            HoverColor = Color.DarkGray;

            this.ActiveControl = null;
        }


        public SearchBar()
        {
            Height = 30;
            BackColor = Color.FromArgb(245, 245, 245);
            BorderStyle = BorderStyle.FixedSingle;
            Padding = new Padding(4, 4, 4, 4);
            _cmbScope.Items.Add(SearchScope.Document);
            _cmbScope.Items.Add(SearchScope.Project);
            _cmbScope.SelectedIndex = 0;

            _btnNext.FlatStyle = FlatStyle.Flat;
            _btnNext.FlatAppearance.BorderSize = 0;
            _btnNext.Margin = new Padding(4, 0, 0, 0);
            _btnClose.Margin = new Padding(4, 0, 0, 0);
            _btnClose.FlatAppearance.BorderSize = 0;
            _btnClose.BackColor = Color.Transparent;
            _btnClose.Cursor = Cursors.Hand;

            _btnClose.MouseEnter += (s, e) => { _btnClose.BackColor = HoverColor; };
            _btnClose.MouseLeave += (s, e) => { _btnClose.BackColor = Color.Transparent; };

            _btnNext.MouseEnter += (s, e) => { _btnNext.BackColor = HoverColor; };
            _btnNext.MouseLeave += (s, e) => { _btnNext.BackColor = Color.Transparent; };

            _btnClose.Click += (s, e) => CloseRequested?.Invoke(this, EventArgs.Empty);
            _btnNext.Click += (s, e) => FindNextRequested?.Invoke(this, EventArgs.Empty);

            _txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true; FindNextRequested?.Invoke(this, EventArgs.Empty);
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    e.SuppressKeyPress = true; CloseRequested?.Invoke(this, EventArgs.Empty);
                }
            };
            _cmbScope.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) { e.SuppressKeyPress = true; CloseRequested?.Invoke(this, EventArgs.Empty); } };
            _btnNext.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) { e.SuppressKeyPress = true; CloseRequested?.Invoke(this, EventArgs.Empty); } };

            Controls.Add(_txtSearch);
            Controls.Add(_cmbScope);
            Controls.Add(_btnNext);
            Controls.Add(_btnClose);
            PerformLayout();
            this.ActiveControl = null;
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            int x = 4;
            int y = 4;
            _txtSearch.Location = new Point(x, y); x += _txtSearch.Width + 4;
            _cmbScope.Location = new Point(x, y); x += _cmbScope.Width + 4;
            _btnNext.Location = new Point(x, y - 1); x += _btnNext.Width + 4;
            _btnClose.Location = new Point(x, y - 1); x += _btnClose.Width + 4;
            Width = x;
            Height = Math.Max(_txtSearch.Height + 8, 30);
        }

        public void FocusSearch()
        {
            _txtSearch.Focus();
            _txtSearch.SelectAll();
        }

        public void ClearAndFocus()
        {
            _txtSearch.Clear();
            FocusSearch();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SearchBar
            // 
            this.Name = "SearchBar";
            this.Size = new System.Drawing.Size(311, 103);
            this.ResumeLayout(false);

        }
    }
}
