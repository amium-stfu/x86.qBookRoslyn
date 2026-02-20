using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace qbookCode.Controls
{
    internal class FormSummary :Form
    {

        SummaryPopup summary;

        public Font EditorFont
        {
            get => Font;
            set => summary.Font = value;
        }

        public FormSummary()
        {
            
            summary = new SummaryPopup();
            summary.Dock = DockStyle.Fill;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            Controls.Add(summary);


        }

        public void ShowSummary(string text, Point location, int width)
        {
            this.Size = summary.GetPreferredSizeForText(text, width);
            summary.SetText(text);
            Location = location;
            Show();
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

