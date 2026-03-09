using AForge.Video;
using AForge.Video.DirectShow;
using CefSharp.DevTools.DOM;
using Org.BouncyCastle.Tls;
using QB;
using QB.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace QB.Controls
{
    public class CameraControl : Panel
    {
        public FilterInfoCollection VideoDevices;
        private VideoCaptureDevice videoSource;

        int _frameCounter = 0;
        public Dictionary<string, int> Devices = new Dictionary<string, int>();


        int _selectedDeviceId = 0;
        public int SelectedDeviceId
        {
            get => _selectedDeviceId;
            set
            {
                _selectedDeviceId = value;
               Debug.WriteLine($"Selected video device: {value}");
            }
        }

      


        public int FrameRate { get; set; } = 2;

        public int SnapshotCounter { get; set; } = 0;

        bool SaveFrames = false;

        public string PictureName { get; set; } = "Dut 4711111";

        BoxButton _btnTakeSnapshot;
        BoxButton _btnMenu;

        List<BoxButton> _devices = new List<BoxButton>();




        public CameraControl(string name, double x = 0, double y = 0, double w = 90, double h = 30) : base(name, x: x, y: y, w: w, h: h)
        {
            Clickable = false;
            init = false;

            VideoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            int c = 0;
            foreach (FilterInfo device in VideoDevices)
            {
                int d = c;
                Devices.Add(device.Name, d);
                
                Debug.WriteLine($"Found video device: {device.Name}");
                QB.Logger.Info($"Found video device: {device.Name}");
                _devices.Add(new BoxButton(name: "",description:device.Name, x: 0, y: 0, w: 60, h: 7,onClick:(e) => _=switchDevice(d)));

                c++;
            }

            OnVisibilityChanged += (s, e) =>
            {
                if(_btnMenu == null) return;
                if (Visible)
                {
                    _btnTakeSnapshot.Visible = true;
                    _btnMenu.Visible = true;
                }
                else
                {
                    
                    _btnMenu.Visible = false;
                    _btnTakeSnapshot.Visible = false;
                }
            };

            OnUpdate += CreateMenu;
           

        }

    

        async Task switchDevice(int id)
        {
            ShowDeviceList(false);
            SelectedDeviceId = id;
            StopCamera();
            await Task.Delay(1000);
            StartCamera();

        }


        public void TakeSnapShoot() => SaveFrames = true;

        bool init = false;
        public void StartCamera()
        {
            if (init) return;

            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.NewFrame -= VideoSource_NewFrame;
                videoSource.SignalToStop();
                videoSource.WaitForStop();

                foreach (VideoCaptureDevice device in VideoDevices)
                {
                    device.Stop();
                }
            }

            VideoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            foreach (FilterInfo device in VideoDevices)
            {
                Debug.WriteLine($"Found video device: {device.Name}");
                QB.Logger.Info($"Found video device: {device.Name}");
            }

            if (VideoDevices.Count > 0)
            {
                videoSource = new VideoCaptureDevice(VideoDevices[SelectedDeviceId].MonikerString);
                videoSource.Start();
                videoSource.NewFrame += VideoSource_NewFrame;
            }
            init = true;
        }

        public void StopCamera()
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.NewFrame -= VideoSource_NewFrame;
                videoSource.SignalToStop();
                videoSource.WaitForStop();
                videoSource.Stop();
                videoSource = null;
            }

            if (BackgroundImage != null)
            {
                var oldImage = BackgroundImage;
                BackgroundImage = null;
                oldImage.Dispose();
            }
            
            init = false;
        }

        bool _deviceMenuVisible = false;
        void ShowDeviceList(bool visible)
        {
            double imageWidth = BackgroundImage.Width;
            double imageHeight = BackgroundImage.Height;
            double offsetX = (Bounds.W - imageWidth / Draw.mmToPx) / 2;
            double offsetY = (Bounds.H - (imageHeight / (imageWidth / Bounds.W))) / 2;

            _deviceMenuVisible = visible;
          
            foreach (BoxButton button in _devices)
            {
                button.Visible = _deviceMenuVisible;
            }
            int c = 0;
            foreach (BoxButton button in _devices)
            {
                button.Frame.Top = Bounds.Y + offsetY + button.H * c;
                button.Frame.BackgroundColor = c == SelectedDeviceId ? System.Drawing.Color.Orange : System.Drawing.Color.LightGray;
                button.Backcolor =  System.Drawing.Color.Red;
                c++;
            }
        }

        int _lastSnapshotCounter = 0;

        public event EventHandler<UpdateEventArgs> OnUpdate;

        public class UpdateEventArgs : EventArgs
        {
            public Bitmap Bitmap { get; set; }
        }
        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
 
           

            _frameCounter++;

            if (SnapshotCounter != 0)
            {
                _lastSnapshotCounter++;
                if (_lastSnapshotCounter == _frameCounter) SaveFrames = true;
            }


            if (_frameCounter < FrameRate) return;

            Bitmap frame = (Bitmap)eventArgs.Frame.Clone();
            
            Bitmap smallFrame = ResizeBitmap(frame, frame.Width / 4, frame.Height / 4); // Zielgröße

            OnUpdate?.Invoke(this, new UpdateEventArgs { Bitmap = smallFrame });



            if (SaveFrames)
            {
                string fileName = $"frame_{DateTime.Now:yyyyMMdd_HHmmssfff}.jpg";
                smallFrame.Save(System.IO.Path.Combine(QB.Book.DataDirectory,fileName), System.Drawing.Imaging.ImageFormat.Jpeg);
                SaveFrames = false;
                _lastSnapshotCounter = 0;
            }

            Image oldImage = null;
            if (BackgroundImage != null)
            {
                oldImage = BackgroundImage;
                BackgroundImage = null;  
            }

            BackgroundImage = smallFrame;

            oldImage?.Dispose();
            _frameCounter = 0;
            frame.Dispose();
        }


        public void CreateMenu(object sender, UpdateEventArgs e)
        {
            if(BackgroundImage == null) return;
            double imageWidth = BackgroundImage.Width;
            double imageHeight = BackgroundImage.Height;
            double offsetX = (Bounds.W - imageWidth / Draw.mmToPx) / 2 ;
            double offsetY = (Bounds.H - (imageHeight / (imageWidth/Bounds.W))) / 2;

            offsetX = offsetX + 1;

            _btnTakeSnapshot = new BoxButton(name: "",
               x: Bounds.X + Bounds.W - 17,
               y: (Bounds.Y + Bounds.H - 17) - offsetY,
               w: 20,
               h: 20,
               icon: "fa:camera:yellow",
               onClick: (s) => SaveFrames = true)
            { Page = this.Directory, Backcolor = System.Drawing.Color.Transparent };
            _btnTakeSnapshot.Create();
            _btnTakeSnapshot.Visible = Visible;

            _btnMenu = new BoxButton(name: "",
                x: Bounds.X + Bounds.W - 17,
                y: Bounds.Y - 3 + + offsetY,
                w: 20,
                h: 20,
                icon: "fa:bars:yellow",
                onClick: (s) => ShowDeviceList(!_deviceMenuVisible))
            { Page = this.Directory, Backcolor = System.Drawing.Color.Transparent };
            _btnMenu.Create();
            _btnMenu.Visible = Visible;

            int c = 0;
            foreach (BoxButton button in _devices)
            {
                button.X = Bounds.X + Bounds.W - button.W - 20;
                button.Y = Bounds.Y + c * button.H;
                button.Page = this.Directory;
                button.Create();
                button.Frame.BackgroundColorHover = System.Drawing.Color.Transparent;
                button.Visible = false;
                c++;

            }

            OnUpdate -= CreateMenu;
        }


        public override void Destroy()
        {
            StopCamera();

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

                // Zeitstempel
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                using (Font font = new Font("Arial", 14, FontStyle.Bold))
                using (Brush brush = new SolidBrush(System.Drawing.Color.Yellow))
                {
                    SizeF textSize = g.MeasureString(PictureName + " " + timestamp, font);

                    float x = 5;
                    float y = 5;

                    g.DrawString(PictureName + " " + timestamp, font, brush, x, y);
                }
            }

            return small;
        }
    }
}
