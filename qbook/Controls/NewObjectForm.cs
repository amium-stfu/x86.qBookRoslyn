using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace qbook
{

    public partial class NewObjectForm : Form
    {

        public static object Edit(Point location, Bounds bounds)
        {
            if (!qbook.Core.ThisBook.DesignMode)
            {
                oControl newItem = new oTag("Tag", "", bounds.X, bounds.Y);
                
                qbook.Core.SelectedLayer.Add(newItem);
                qbook.Core.ThisBook.Modified = true;
                qbook.Core.ThisBook.Main.UnSelectTag();
                return newItem;
            }

            NewObjectForm tf = new NewObjectForm(bounds);
            tf.Location = location;
            tf.ShowDialog();
            tf.Close();



            if (newControl != null)
                return newControl;
            else
                return newItem;
        }

        Bounds Bounds_ = null;
        static oControl newItem = null;
        static Control newControl = null;

        double x = 0;
        public NewObjectForm(Bounds bounds)
        {
            InitializeComponent();
            Bounds_ = bounds;
            x = bounds.X;
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
          
            qbook.Core.SelectedLayer.Add(newItem);
            qbook.Core.ThisBook.Modified = true;
            qbook.Core.ThisBook.Main.UnSelectTag();
            Close();
        }


        private void buttonTag_Click(object sender, EventArgs e)
        {
            double xc = x;
            newItem = new oTag("Tag", "", Bounds_.X, Bounds_.Y);
            Edit();
        }

        private void buttonText_Click(object sender, EventArgs e)
        {
            newItem = new oText("Text", "");
            newItem.Bounds = Bounds_;
            Edit();
        }

        private void buttonValve_Click(object sender, EventArgs e)
        {
            newItem = new oPart(PartType.Valve22);
            if (Bounds_.W < 6)
                Bounds_.W = 20;
            if (Bounds_.H < 6)
                Bounds_.H = 10;
            newItem.Bounds = Bounds_;
            Edit();
        }

        private void buttonPump_Click(object sender, EventArgs e)
        {
            newItem = new oPart(PartType.Pump);
            if (Bounds_.W < 6)
                Bounds_.W = 20;
            if (Bounds_.H < 6)
                Bounds_.H = 10;
            newItem.Bounds = Bounds_;
            Edit();
        }

        private void buttonFilter_Click(object sender, EventArgs e)
        {
            newItem = new oPart(PartType.Filter);
            if (Bounds_.W < 6)
                Bounds_.W = 20;
            if (Bounds_.H < 6)
                Bounds_.H = 10;
            newItem.Bounds = Bounds_;
            Edit();
        }

        private void buttonModule_Click(object sender, EventArgs e)
        {
            newItem = new oModule();
            if (Bounds_.W < 6)
                Bounds_.W = 20;
            if (Bounds_.H < 6)
                Bounds_.H = 10;
            newItem.Bounds = Bounds_;
            Edit();
        }

        private void buttonGrid_Click(object sender, EventArgs e)
        {
            //newItem = new x_oGrid("Grid", "");
            //newItem.Bounds = Bounds_;
            //Edit();
        }

        private void buttonScratch_Click(object sender, EventArgs e)
        {
            newItem = new oScratch("Scratch", "");
            newItem.Bounds = Bounds_;
            Edit();
        }


        private void buttonHtml_Click(object sender, EventArgs e)
        {
            oHtml htmlItem = new oHtml();
            newItem = htmlItem;
            newItem.Name = "html" + qbook.Core.SelectedLayer.Objects.OfType<oHtml>().Count();
            newItem.Text = "";
            newItem.Bounds = Bounds_;
            htmlItem.CodeHtml = "Hello HTML!";
            htmlItem.CodeCss = "";
            htmlItem.CodeSettings = "";
            Edit();

            //newControl = new cHtml();
            //newControl.Name = "html" + Main.Qb.SelectedLayer.Objects.OfType<cHtml>().Count();
            //newControl.Text = "";
            ////newControl.Bounds = new Rectangle((int)(Bounds_.X * Draw.mmToPx), (int)(Bounds_.Y * Draw.mmToPx), (int)(Bounds_.W * Draw.mmToPx), (int)(Bounds_.H * Draw.mmToPx));
            //newControl.Bounds = new Rectangle((int)(Bounds_.X)), (int)(Bounds_.Y), (int)(Bounds_.W), (int)(Bounds_.H));

        }





        private void buttonControl_Click(object sender, EventArgs e)
        {
            newItem = new oControl();
            newItem.Name = "c" + qbook.Core.SelectedLayer.Objects.OfType<oControl>().Count();
            newItem.Text = "";
            newItem.Bounds = Bounds_;
            Edit();
        }


    }
}
