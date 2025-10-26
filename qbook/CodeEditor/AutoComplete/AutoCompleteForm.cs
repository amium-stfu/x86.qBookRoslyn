using ScintillaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace qbook.CodeEditor.AutoComplete
{
    public partial class AutoCompleteForm : Form
    {
        Scintilla Editor;


        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_SHOWNOACTIVATE = 4;

        public AutoCompleteForm(Scintilla editor)
        {
            InitializeComponent();

            Deactivate += (s, e) => Hide();
            Editor = editor;
        }
        public void ShowAtCaret(Point screenLocation, int width = 250, int height = 200)
        {
            Location = screenLocation;
            Width = width;
            Height = height;
            ShowWindow(this.Handle, SW_SHOWNOACTIVATE);
            BringToFront();
           
        }
        public void UpdateItems(IEnumerable<string> items)
        {
            List.BeginUpdate();
            List.Items.Clear();
            foreach (var s in items)
                List.Items.Add(s);
            if (List.Items.Count > 0) List.SelectedIndex = 0;
            List.EndUpdate();
        }
        public string? SelectedText => List.SelectedItem?.ToString();

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                cp.ExStyle |= 0x00000008; // WS_EX_TOPMOST
                cp.ExStyle |= 0x80;       // WS_EX_TOOLWINDOW
                return cp;
            }
        }

    }



}
