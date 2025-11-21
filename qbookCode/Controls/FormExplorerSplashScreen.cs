using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace qbookCode.Controls
{
    public partial class FormExplorerSplashScreen : Form
    {
        public FormExplorerSplashScreen()
        {
            InitializeComponent();
        }

        public void SetStatus(string status)
        {
            if (StatusText.InvokeRequired)
            {
                StatusText.Invoke(new Action(() => StatusText.Text = status));
                StatusText.Refresh();
            }
            else
            {
                StatusText.Text = status;
                StatusText.Refresh();
            }

        }
    }


}
