using System;
using System.Drawing;
using System.Windows.Forms;

namespace qbook.CodeEditor
{
    internal enum ReplaceScope { Selection, Document }

    internal sealed class ReplaceBar : UserControl
    {
        private readonly TextBox _txtFind = new() { BorderStyle = BorderStyle.FixedSingle, Width = 150 };
        private readonly TextBox _txtReplace = new() { BorderStyle = BorderStyle.FixedSingle, Width = 150};
        private readonly ComboBox _cmbScope = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 90 };
        private readonly Button _btnNext = new() { Text = "Next", Width = 50 };
        private readonly Button _btnRep = new() { Text = "Replace", Width = 70 };
        private readonly Button _btnAll = new() { Text = "All", Width = 45 };
        private readonly Button _btnClose = new() { Text = "X", Width = 28, FlatStyle = FlatStyle.Flat };

        public event EventHandler? FindNextRequested;
        public event EventHandler? ReplaceRequested;
        public event EventHandler? ReplaceAllRequested;
        public event EventHandler? CloseRequested;

        public string FindText => _txtFind.Text;
        public string ReplaceText => _txtReplace.Text;
        public ReplaceScope Scope => (ReplaceScope)_cmbScope.SelectedItem!;

        Color HoverColor = Color.FromArgb(200, 200, 200);

        public void DarkTheme()
        {
            BackColor = Color.FromArgb(45, 45, 48);
            _txtFind.BackColor = Color.FromArgb(30, 30, 30);
            _txtFind.ForeColor = Color.FromArgb(220, 220, 220);
            _txtReplace.BackColor = Color.FromArgb(30, 30, 30);
            _txtReplace.ForeColor = Color.FromArgb(220, 220, 220);
            _cmbScope.BackColor = Color.FromArgb(30, 30, 30);
            _cmbScope.ForeColor = Color.FromArgb(220, 220, 220);
            _btnNext.BackColor = BackColor;
            _btnNext.ForeColor = Color.White;
            _btnRep.BackColor = BackColor;
            _btnRep.ForeColor = Color.White;
            _btnAll.BackColor = BackColor;
            _btnAll.ForeColor = Color.White;
            _btnClose.ForeColor = Color.FromArgb(220, 220, 220);
        }

        public void LightTheme()
        {
            BackColor = Color.FromArgb(245, 245, 245);
            _txtFind.BackColor = Color.White;
            _txtFind.ForeColor = Color.Black;
            _txtReplace.BackColor = Color.White;
            _txtReplace.ForeColor = Color.Black;
            _cmbScope.BackColor = Color.White;
            _cmbScope.ForeColor = Color.Black;
            _btnNext.BackColor = BackColor;
            _btnNext.ForeColor = Color.Black;
            _btnRep.BackColor = BackColor;
            _btnRep.ForeColor = Color.Black;
            _btnAll.BackColor = BackColor;
            _btnAll.ForeColor = Color.Black;
            _btnClose.ForeColor = Color.Black;
        }

        public ReplaceBar()
        {
            Height = 32; BackColor = Color.FromArgb(245,245,245); BorderStyle = BorderStyle.FixedSingle; Padding = new Padding(4,4,4,4);
            foreach (var v in Enum.GetValues(typeof(ReplaceScope))) _cmbScope.Items.Add(v);
            _cmbScope.SelectedIndex = 1; // Document default

            _btnClose.FlatAppearance.BorderSize = 0; _btnClose.BackColor = Color.Transparent; _btnClose.Cursor = Cursors.Hand;

            _btnNext.Click += (s,e)=> FindNextRequested?.Invoke(this,EventArgs.Empty);
            _btnRep.Click += (s,e)=> ReplaceRequested?.Invoke(this,EventArgs.Empty);
            _btnAll.Click += (s,e)=> ReplaceAllRequested?.Invoke(this,EventArgs.Empty);
            _btnClose.Click += (s,e)=> CloseRequested?.Invoke(this,EventArgs.Empty);

            _txtFind.KeyDown += (s,e)=>{ if(e.KeyCode==Keys.Enter){ e.SuppressKeyPress=true; FindNextRequested?.Invoke(this,EventArgs.Empty);} else if(e.KeyCode==Keys.Escape){ e.SuppressKeyPress=true; CloseRequested?.Invoke(this,EventArgs.Empty);} };
            _txtReplace.KeyDown += (s,e)=>{ if(e.KeyCode==Keys.Enter){ e.SuppressKeyPress=true; ReplaceRequested?.Invoke(this,EventArgs.Empty);} else if(e.KeyCode==Keys.Escape){ e.SuppressKeyPress=true; CloseRequested?.Invoke(this,EventArgs.Empty);} };
            _cmbScope.KeyDown += (s,e)=>{ if(e.KeyCode==Keys.Escape){ e.SuppressKeyPress=true; CloseRequested?.Invoke(this,EventArgs.Empty);} };
            _btnNext.KeyDown += (s,e)=>{ if(e.KeyCode==Keys.Escape){ e.SuppressKeyPress=true; CloseRequested?.Invoke(this,EventArgs.Empty);} };

            Controls.Add(_txtFind); Controls.Add(_txtReplace); Controls.Add(_cmbScope);
            Controls.Add(_btnNext); Controls.Add(_btnRep); Controls.Add(_btnAll); Controls.Add(_btnClose);

            _btnNext.FlatStyle = FlatStyle.Flat;
            _btnNext.FlatAppearance.BorderSize = 0;
            _btnRep.FlatStyle = FlatStyle.Flat;
            _btnRep.FlatAppearance.BorderSize = 0;
            _btnAll.FlatStyle = FlatStyle.Flat;
            _btnAll.FlatAppearance.BorderSize = 0;

            _btnClose.MouseEnter += (s,e)=>{ _btnClose.BackColor=HoverColor; };
            _btnClose.MouseLeave += (s,e)=>{ _btnClose.BackColor=Color.Transparent; };
            _btnNext.MouseEnter += (s,e)=>{ _btnNext.BackColor=HoverColor; };
            _btnNext.MouseLeave += (s,e)=>{ _btnNext.BackColor=Color.Transparent; };
            _btnRep.MouseEnter += (s,e)=>{ _btnRep.BackColor=HoverColor; };
            _btnRep.MouseLeave += (s,e)=>{ _btnRep.BackColor=Color.Transparent; };
            _btnAll.MouseEnter += (s,e)=>{ _btnAll.BackColor=HoverColor; };
            _btnAll.MouseLeave += (s,e)=>{ _btnAll.BackColor=Color.Transparent; };


            PerformLayout();
        }
        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            int x=4; int y=4;
            _txtFind.Location=new Point(x,y); x+=_txtFind.Width+4;
            _txtReplace.Location=new Point(x,y); x+=_txtReplace.Width+4;
            _cmbScope.Location=new Point(x,y); x+=_cmbScope.Width+4;
            _btnNext.Location=new Point(x,y-1); x+=_btnNext.Width+4;
            _btnRep.Location=new Point(x,y-1); x+=_btnRep.Width+4;
            _btnAll.Location=new Point(x,y-1); x+=_btnAll.Width+4;
            _btnClose.Location=new Point(x,y-1); x+=_btnClose.Width+4;
            Width=x; Height=Math.Max(_txtFind.Height+8,32);
        }
        public void FocusFind(){ _txtFind.Focus(); _txtFind.SelectAll(); }
        public void SetFindText(string t){ _txtFind.Text=t; _txtFind.SelectAll(); }
        public void ClearAll(){ _txtFind.Clear(); _txtReplace.Clear(); }
    }
}
