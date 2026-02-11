using QB.Controls;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace qbook
{
    public partial class ImageForm : Form
    {
        public delegate void DoneHandler(Image image);
        public event DoneHandler Done;

        public ImageForm(Image image)
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            Bitmap img = new Bitmap(image);



            // imageControl1.Image = Draw.ResizeImage(img, 1280, 1280);
            imageControl1.RawImage = img;
            imageControl1.Image = Draw.ResizeImage(img, 800, 800);

            Width = (int)(imageControl1.Image.Width + 25);
            Height = (int)(imageControl1.Image.Height + 40);

            imageControl1.Location = new Point(5, 10);


            imageControl1.Done += ImageControl1_Done;

            // SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //   this.BackgroundImage = image;
            //  this.BackgroundImageLayout = ImageLayout.Zoom;
        }

        private void ImageControl1_Done(Image image)
        {
            if (Done != null)
                Done(image);
            Close();
        }

        private void ImageForm_Load(object sender, EventArgs e)
        {

        }
    }
}
