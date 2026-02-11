
//using OpenCvSharp.Internal.Vectors;
//using OpenCvSharp.Internal;
using QB.Controls;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace qbook
{
    public partial class WebcamForm : Form
    {
        public WebcamForm()
        {
            InitializeComponent();

            //   capture = new VideoCapture();
            //  capture.Set(VideoCaptureProperties.FrameHeight, 2000);
            //  capture.Set(VideoCaptureProperties.FrameWidth, 4000);

            //  capture.Open(0);
        }

        //  public static  VideoCapture capture = new VideoCapture();

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image.Save(DateTime.Now.ToString("HH-mm-ss") + "test.jpg", ImageFormat.Jpeg);
            ImageForm form = new ImageForm(pictureBox1.Image);
            form.Done += ImportImage;
            form.Show();
        }

        private void ImportImage(Image image)
        {
            oImage item = new oImage("", "Image");
            int maxPixel = 1200;
            if (image.Width > image.Height)
                item.Data = Draw.ImageToBase64(Draw.ResizeImage(image, maxPixel, maxPixel * image.Height / image.Width), System.Drawing.Imaging.ImageFormat.Jpeg);
            else
                item.Data = Draw.ImageToBase64(Draw.ResizeImage(image, maxPixel * image.Width / image.Height, maxPixel), System.Drawing.Imaging.ImageFormat.Jpeg);
            qbook.Core.SelectedLayer.Add(item);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            /*
            if (capture != null)
                if (capture.IsOpened())
                {
                    Mat frame = new Mat();
                    capture.Read(frame);
                    Bitmap image = BitmapConverter.ToBitmap(frame);
                    pictureBox1.Image = image;
                }
            */
        }
    }
}
