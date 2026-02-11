using System;
using System.Drawing;
using System.Windows.Forms;

namespace qbook
{

    public partial class TagForm1 : Form
    {

        public static void Edit(int x, int y, oItem tag)
        {

            TagForm1 tf = new TagForm1(tag);
            tf.Location = new Point(x, y);
            tf.ShowDialog();
            qbook.Core.ThisBook.Modified = true;
            tf.Close();
        }


        public oItem Item;
        public TagForm1(oItem item)
        {
            InitializeComponent();
            Item = item;
            textBoxTag.Text = item.TextL.Replace("\r\n", "\n").Replace("\n", "\r\n"); ;
            listBoxTag.Items.Add("dimensions");
            listBoxTag.Items.Add("powersupply");
            textBoxTag.Focus();
            textBoxTag.SelectAll();

            if (qbook.Core.ThisBook.Language == "en")
                buttonTranslate.Show();
            else
                buttonTranslate.Hide();
        }


        private void buttonDelete_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("delete " + Item.Name + "/" + Item.Text + "?", "delete tag/object!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                qbook.Core.ThisBook.Main.Remove(Item);
                Close();
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.Modified = true;
            if (Item is oTag)
                (Item as oTag).Position = (sender as Control).Text;
            Close();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.Main.UnSelectTag();
            Item.TextL = textBoxTag.Text.Replace("\r\n", "\n").Trim('\n').Replace("\n", "\r\n");
            Close();
            //   panelTags.Hide();
            //   pageControl.Update_();
            //   PageRedraw();
        }

        private void textBoxTag_KeyDown(object sender, KeyEventArgs e)
        {
            Item.TextL = textBoxTag.Text.Replace("\r\n", "\n").Trim('\n').Replace("\n", "\r\n");
            // if (e.KeyCode == Keys.Enter)
            //     Close();
            if (e.KeyCode == Keys.Escape)
                Close();
        }

        private void listBoxTag_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (listBoxTag.Items[listBoxTag.SelectedIndex].ToString() == "dimensions")
                {
                    qbook.Core.ThisBook.Modified = true;
                    textBoxTag.Text = "dimensions\n- height=\txxx mm\n- width=\txxx mm\n- depth=\txxx mm".Replace("\n", "\r\n");
                }
                if (listBoxTag.Items[listBoxTag.SelectedIndex].ToString() == "powersupply")
                {
                    qbook.Core.ThisBook.Modified = true;
                    textBoxTag.Text = "powersupply\n- voltage\t230VAC\n- current\tmax. xxx A".Replace("\n", "\r\n");
                }
            }
            catch
            { }
        }

        private void buttonTranslate_Click(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.Modified = true;
            Item.TextDE = null;
            Item.TextES = null;
        }
    }
}
