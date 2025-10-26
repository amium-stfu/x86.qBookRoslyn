using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace qbook.ScintillaEditor
{
    public class FormPopup : Form
    {
        public PopupControl ListView { get; }

        public Font EditorFont
        {
            get => ListView.EditorFont;
            set => ListView.EditorFont = value;
        }

        public void AutoSize()
        {
            ListView.AutoSizeHeight();
        }

        public void Config(int width, int iconW, int typeW,int textW, int descW, int valueW)
        {
            Width = width;
            ListView.IconWidth = iconW;
            ListView.TypeWidth = typeW;
            ListView.DescriptionWidth = descW;
            ListView.ValueWidth = valueW;
            ListView.TextWidth = textW;
            
        }

        public FormPopup()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;

            ListView = new PopupControl { Dock = DockStyle.Fill };
            Controls.Add(ListView);
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW
                return cp;
            }
        }


    }

}
