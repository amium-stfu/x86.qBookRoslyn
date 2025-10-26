using QB.Controls;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace qbook
{
    public partial class ImageControl : UserControl
    {

        public delegate void DoneHandler(Image image);
        public event DoneHandler Done;

        public ImageControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);


        }

        private void ImageControl_Paint(object sender, PaintEventArgs e)
        {
            if (Image == null)
                return;
            e.Graphics.DrawImage(Image, 0, 0);

            if (!down)
                return;
            //    e.Graphics.DrawRectangle(Pens.Green, 0, 0, 20, 20);


            e.Graphics.DrawRectangle(Pens.Green, p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);
        }

        private static Image cropImage(Image img, System.Drawing.Rectangle cropArea)
        {
            if (cropArea.Height < 0)
            {
                cropArea.Y += cropArea.Height;
                cropArea.Height = -cropArea.Height;
            }

            if (cropArea.Width < 0)
            {
                cropArea.X += cropArea.Width;
                cropArea.Width = -cropArea.Width;
            }

            Bitmap bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);

        }

        public Image RawImage { get; set; }
        public Image Image { get; set; }


        Point p1;
        Point p2;
        bool down = false;


        private void ImageControl_MouseDown(object sender, MouseEventArgs e)
        {
            down = true;
            p1 = e.Location;
        }

        private void ImageControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (!down)
                return;
            p2 = e.Location;
            Refresh();
        }

        private void ImageControl_MouseUp(object sender, MouseEventArgs e)
        {
            down = false;


            int x = Math.Min(p1.X, p2.X);
            int y = Math.Min(p1.Y, p2.Y);
            int w = Math.Abs(p1.X - p2.X);
            int h = Math.Abs(p1.Y - p2.Y);

            if ((w > 20) && (h > 20))
            {
                float xScale = (float)RawImage.Width / Image.Width;
                float yScale = (float)RawImage.Height / Image.Height;

                Image = cropImage(RawImage, new System.Drawing.Rectangle((int)(x * xScale), (int)(y * yScale), (int)(w * xScale), (int)(h * yScale)));
                Image = Draw.ResizeImage(Image, 1200, 1200);
                //  Image = cropImage(Image, new Rectangle(x, y, w,h));
                if (Done != null)
                    Done(Image);
            }
        }

        private void ImageControl_Load(object sender, EventArgs e)
        {

        }

        private void ImageControl_DoubleClick(object sender, EventArgs e)
        {

            if (Done != null)
                Done(Image);
        }
    }
}
