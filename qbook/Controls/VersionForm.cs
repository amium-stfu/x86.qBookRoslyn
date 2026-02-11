using System;
using System.Windows.Forms;

namespace qbook.CodeEditor
{
    public partial class VersionForm : Form
    {
        public VersionForm()
        {
            InitializeComponent();
        }



        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("Are you sure you want to update version and version-history?", "UPDATE HISTORY", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                qbook.Core.ThisBook.Version = textBoxVersion.Text;
                qbook.Core.ThisBook.VersionHistory = textBoxVersionHistory.Text;

                this.Close();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if ((textBoxVersion.Text != origVersion) || (textBoxVersionHistory.Text != origVersionHistory))
            {
                var dr = MessageBox.Show("Version and/or version-history have been modified.\r\nDo you want to\r\n[Yes]\tSave\r\n[No]\tDon't save\r\n[Cancel]\tCancel", "MODIFIED"
                    , MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.Cancel)
                    return;
                if (dr == DialogResult.No)
                    this.Close();
                if (dr == DialogResult.Yes)
                {
                    qbook.Core.ThisBook.Version = textBoxVersion.Text;
                    qbook.Core.ThisBook.VersionHistory = textBoxVersionHistory.Text;
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }
        }

        string origVersion = null;
        string origVersionHistory = null;
        private void VersionForm_Load(object sender, EventArgs e)
        {
            origVersion = qbook.Core.ThisBook.Version;
            origVersionHistory = qbook.Core.ThisBook.VersionHistory;

            textBoxVersion.Text = qbook.Core.ThisBook.Version;
            textBoxVersionHistory.Lines = qbook.Core.ThisBook.VersionHistory.Replace("\r\n", "\n").Split('\n');
        }

        private void buttonUndo_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("Undo changes?", "UNDO", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                textBoxVersion.Text = origVersion;
                textBoxVersionHistory.Text = origVersionHistory;
            }
        }
    }
}
