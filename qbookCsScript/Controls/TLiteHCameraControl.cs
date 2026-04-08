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
        public TLiteHCameraControl(string name, TLiteHCameraComClient camera, double x = 0, double y = 0, double w = 90, double h = 30) : base(name, x: x, y: y, w: w, h: h)
        {
            Clickable = false;
            _comClient = camera;


            OnVisibilityChanged += (s, e) =>
            {
                if (DateTime == null) return;
                if (Visible)
                {
                    DateTime.Visible = true;
                    Tmin.Visible = true;
                    Tmax.Visible = true;
                    Tavg.Visible = true;
                }
                else
                {

                    DateTime.Visible = false;
                    Tmin.Visible = false;
                    Tmax.Visible = false;
                    Tavg.Visible = false;
                }
            };



        }


        public void StartCamera()
        {
            
            if (_comClient != null)
            {
                _comClient.StartReceivingAsync();
            }

            if (_comClient == null) return; 
       
            

            _comClient.OnUpdate +=  moveDisplay;
            _comClient.OnUpdate += updateFrame;


        }

        void updateFrame(object sender, TLiteHCameraComClient.UpdateEventArgs e)
        {
            if (_comClient == null) return;
            Bitmap frame = e.Bitmap;
            if (frame == null) return;
            Bitmap smallFrame = ResizeBitmap(frame, frame.Width * 3, frame.Height * 3);
            BackgroundImage = e.Bitmap;
        }

        void moveDisplay(object sender, EventArgs e) 
        {

            if(BackgroundImage == null)  return;
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

            Tmin.Visible = Visible;
            Tmax.Visible = Visible;
            Tavg.Visible = Visible;
            DateTime.Visible = Visible;

            _comClient.OnUpdate -= moveDisplay;





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
           if(_comClient != null)
            _comClient.OnUpdate -= moveDisplay;
            _comClient.OnUpdate -= updateFrame;
        }
    }
}
