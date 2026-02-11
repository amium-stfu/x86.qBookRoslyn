using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace qbook
{

    public partial class StringForm : Form
    {
        public static void Edit(int x, int y, ref string text, List<string> preSelctions)
        {
            StringForm tf = new StringForm(text, preSelctions);
            tf.Location = new Point(x, y);
            tf.ShowDialog();
            text = tf.Text();
            tf.Close();
        }

        public StringForm(string text, List<string> preSelctions)
        {
            InitializeComponent();
            textBox1.Text = text;
            textBox1.SelectAll();
            if (preSelctions != null)
                foreach (string sel in preSelctions)
                {
                    Button b = new Button();
                    b.Text = sel;
                    b.Click += B_Click;
                    b.Location = new Point(0, Height);
                    b.Size = new Size(200, 23);
                    Height += b.Size.Height;
                    Controls.Add(b);
                }
        }

        private void B_Click(object sender, EventArgs e)
        {
            textBox1.Text = (sender as Button).Text;
            Close();
        }

        public new string Text()
        {
            return textBox1.Text;
        }
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                Close();
            if (e.KeyCode == Keys.Escape)
                Close();
        }


        private void buttonOk_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("delete " + textBox1.Text + "?", "delete object!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                textBox1.Text = "";
                Close();
            }
        }
    }
}
