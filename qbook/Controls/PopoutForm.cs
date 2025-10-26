using System;
using System.Windows.Forms;

namespace qbook
{
    public partial class PopoutForm : Form
    {
        public PopoutForm()
        {
            InitializeComponent();
            ResizePageControl();
        }
        public oPage Page
        {
            set
            {
                //  Text = Main.Qb.Book.Filename + " - " + Main.Qb.SelectedPage.TextL;
                Text = qbook.Core.ThisBook.Filename + " - " + value.Name + " " + value.TextL;
                pageControl.Page = value;
                pageControl.Popout = true;
                pageControl.Frame = false;
            }
        }
        double ratio = 29.7f / 21f;

        //void ResizePageControl()
        //{
        //    //Width = (int)((Height - 30 + 20) * ratio * 1.10f) + 80;
        //    Width = (int)((Height - 30) * ratio * 1.10f);

        //    float w = Width * 1.055f -15;
        //    float h = Height * 1.14f -38;


        //    if (w / h > ratio)
        //        pageControl.Bounds = new System.Drawing.Rectangle((int)((w - h * ratio) / 2), 0, (int)(h * ratio), (int)h);
        //    else
        //        pageControl.Bounds = new System.Drawing.Rectangle(0, (int)((h - w / ratio) / 2), (int)w, (int)(w / ratio));


        ////    pageControl.Bounds = new Rectangle(0, 0, 100, 100);

        //    pageControl.Invalidate();
        //}

        void ResizePageControl()
        {
            //double ratio = 29.7f / 21f;
            double ratio = 280.0 / 180.0f; //page drawing area (client-rectangle)
            var clientRect = this.ClientRectangle;
            var w = clientRect.Width;
            var h = clientRect.Height;
            if ((double)w / (double)h > ratio)
            {
                int cw = (int)(h * ratio);
                pageControl.Bounds = new System.Drawing.Rectangle((int)((w - cw) / 2), 0, (int)(cw), (int)h);
            }
            else
            {
                int ch = (int)(w / ratio);
                pageControl.Bounds = new System.Drawing.Rectangle(0, (int)((h - ch) / 2), (int)w, (int)(ch));
            }

            pageControl.Invalidate();
        }


        private void PopoutForm_SizeChanged(object sender, EventArgs e)
        {
            ResizePageControl();
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            pageControl.Invalidate();
        }
    }
}
