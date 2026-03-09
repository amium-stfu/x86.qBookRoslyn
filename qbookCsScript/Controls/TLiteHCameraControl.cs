using QB.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace QB.Controls
{
    public class TLiteHCameraControl : Panel
    {
        TLiteHCameraComClient _comClient;

        BoxStringSignal DateTime;
        BoxSignal Tmin;
        BoxSignal Tmax;
        BoxSignal Tavg;
        public TLiteHCameraControl(string name, double x = 0, double y = 0, double w = 90, double h = 30) : base(name, x: x, y: y, w: w, h: h)
        {
            Clickable = false;
               
        }


        public void StartCamera(string portName, int baudRate = 115200)
        {
            if (_comClient != null)
            {
                _comClient.StopReceiving();
                _comClient = null;
            }
            _comClient = new TLiteHCameraComClient(portName, baudRate);
          

            _comClient.OnUpdate += (s, e) =>
            {
                Bitmap frame = e.Bitmap;


                Bitmap smallFrame = ResizeBitmap(frame,frame.Width * 3, frame.Height * 3);

                BackgroundImage = e.Bitmap;
            };
            
            _comClient.StartReceivingAsync();

            _comClient.OnUpdate += (s, e) => moveDisplay();
     

        }

        void moveDisplay() 
        {

            if (DateTime != null) return;

            double imageWidth = BackgroundImage.Width;
            double imageHeight = BackgroundImage.Height;
            double offsetX = ((Bounds.W - imageWidth) / 2) / Draw.mmToPx;
            double offsetY = ((Bounds.H - imageHeight) / 2) / Draw.mmToPx;

            offsetX = offsetX + 1;

            DateTime = new BoxStringSignal(name: "DateTime", locked: true, target: _comClient.DateTime, x: Bounds.X + offsetX, y: Bounds.Y-2, w: 100, h: 7) { Page = this.Directory, Backcolor = System.Drawing.Color.Transparent };
            Tmin = new BoxSignal(name: "Tmin",format:"0.0", locked: true, target: _comClient.Tmin, x: Bounds.X + offsetX, y: Bounds.Y + 7, w: 15, h: 7) { Page = this.Directory, Backcolor = System.Drawing.Color.Transparent };
            Tmax = new BoxSignal(name: "Tmax",format:"0.0", locked: true, target: _comClient.Tmax, x: Bounds.X + offsetX, y: Bounds.Y + 14, w: 15, h: 7) { Page = this.Directory, Backcolor = System.Drawing.Color.Transparent };
            Tavg = new BoxSignal(name: "Tavg",format:"0.0", locked: true, target: _comClient.Tavg, x: Bounds.X + offsetX, y: Bounds.Y + 21, w: 15, h: 7) { Page = this.Directory, Backcolor = System.Drawing.Color.Transparent };
           

            DateTime.Create();
            Tmin.Create();
            Tmax.Create();
            Tavg.Create();

            _comClient.OnUpdate -= (s, e) => moveDisplay();





        }

        private Bitmap ResizeBitmap(Bitmap source, int width, int height)
        {
            Bitmap small = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(small))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                g.DrawImage(source, 0, 0, width, height);
            }

            return small;
        }

        public override void Destroy()
        {
            _comClient.OnUpdate += (s, e) => moveDisplay(); 
        }
    }
}
