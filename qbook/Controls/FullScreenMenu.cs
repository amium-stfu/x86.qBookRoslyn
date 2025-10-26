using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace qbook.Controls
{
    public partial class FullScreenMenu : Form
    {

        MainForm mainForm = (MainForm)Application.OpenForms["MainForm"];

        public FullScreenMenu()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mainForm.ResetFullScreen();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mainForm.TogglePageControlBar();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            qbook.Core.ShowLogForm();
        }
    }
}
