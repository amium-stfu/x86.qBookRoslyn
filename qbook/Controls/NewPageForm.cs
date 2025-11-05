using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace qbook
{

    public partial class NewPageForm : Form
    {

        public static void Edit(oControl parent)
        {

            NewPageForm tf = new NewPageForm(parent);
            //    tf.Location = location;
            tf.ShowDialog();
            tf.Close();


        }

        // Bounds Bounds_ = null;
        oPage newPage = null;
        oControl ParentControl;

        // float x = 0;
        public NewPageForm(oControl parent)
        {
            InitializeComponent();
            ParentControl = parent;
            //   Bounds_ = bounds;
            //   x = bounds.X;
            //    Tag = tag;
            //  textBoxTag.Text = tag.Text.Replace("\r\n", "\n").Replace("\n", "\r\n"); ;
            /*
            listBoxTag.Items.Add("dimensions");
            listBoxTag.Items.Add("powersupply");
            textBoxTag.Focus();
            textBoxTag.SelectAll();

            buttonEnter.Hide();
            panelTag.Hide();
            */
            /*
               // Pics.Add(new sIcon(null, Main.Icon.Object, "Modul", xo +=h, 1, wo, h, NextSelect));
              Pics.Add(new oIcon(null, Main.Icon.Object, Pens.Black, "Item", xo += wo, 1, wo, h, NextObject));
            Pics.Add(new oIcon(null, Main.Icon.Object, Pens.Black, "Tube", xo += wo, 1, wo, h, NextObject));
             Pics.Add(new oIcon(null, Main.Icon.Object, Pens.Black, "Regulator", xo += wo, 1, wo, h, NextObject));
            Pics.Add(new oIcon(null, Main.Icon.Object, Pens.Black, "schedu", xo += wo, 1, wo, h, NextObject));
            */
        }


        private void buttonClear_Click(object sender, EventArgs e)
        {
            //  Main.Qb.File.Modified = true;
            Close();
        }


        void Edit()
        {
           


            //    string text = "page " + ((sender as oIcon).Parent.Objects.Where(item => item is oPage).ToList().Count + 1);
            //    oPage page = new oPage("", text);
            int indx = ParentControl.Objects.IndexOf(qbook.Core.SelectedPage);
            ParentControl.Objects.Insert(indx + 1, newPage);
            qbook.Core.SelectedPage = newPage;

            //   Main.Qb.SelectedLayer.Add(newItem);
            qbook.Core.ThisBook.Modified = true;
            //   Main.Qb.File.Object.UnSelectTag();
            Close();
        }



        private void buttonText_Click(object sender, EventArgs e)
        {
            //    newItem = new oText("Text", "");
            //   newItem.Bounds = Bounds_;
            Edit();
        }



        private void buttonGrid_Click(object sender, EventArgs e)
        {
            // newItem = new oGrid("Grid", "");
            //  newItem.Bounds = Bounds_;
            Edit();
        }

        private void buttonScratch_Click(object sender, EventArgs e)
        {
            //  newItem = new oScratch("Scratch","");
            //  newItem.Bounds = Bounds_;
            Edit();
        }


        private void buttonPage_Click(object sender, EventArgs e)
        {
            string text = "page " + (ParentControl.Objects.Where(item => item is oPage).ToList().Count + 1);
            newPage = new oPage("", text);

            Edit();
        }
    }
}
