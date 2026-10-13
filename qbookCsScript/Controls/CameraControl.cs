using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using OpenCvSharp;
using QB;
using QB.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QB.Controls
{
    public class CameraControl : Panel
    {
        private readonly object updateGate = new object();

        private IEnumerable<UIBox> OwnedDisplays()
        {
            return (new UIBox[] { _btnTakeSnapshot, _btnMenu }.Concat(_devices)).Where(display => display != null).ToArray();
        }

        internal override IEnumerable<Control> OwnedControls()
        {
            return base.OwnedControls().Concat(OwnedDisplays().SelectMany(display => display.OwnedUIControls()));
        }

        internal override void DetachControl(Control control)
        {
            base.DetachControl(control);
            foreach (var display in OwnedDisplays()) display.DetachUIControl(control);
        }

        internal override void ReleaseControl(List<Exception> errors)
        {
            foreach (var display in OwnedDisplays()) TryCleanup(errors, display.ReleaseOwnedUI);
            _btnTakeSnapshot = _btnMenu = null;
            _devices.Clear();
            TryCleanup(errors, () => LastFrame?.Dispose());
            LastFrame = null;
            base.ReleaseControl(errors);
        }

        internal override void PrepareRemoval(List<Exception> errors)
        {
            lock (updateGate) OnUpdate = null;
            _startRequested = false;
            TryCleanup(errors, StopCamera);
            var image = BackgroundImage;
            BackgroundImage = null;
            if (image != null) TryCleanup(errors, image.Dispose);
            init = false;
            base.PrepareRemoval(errors);
        }

        public IReadOnlyList<CameraDevice> VideoDevices;
        private CameraCapture videoSource;
        private int captureGeneration;
        private int framePending;

        int _frameCounter = 0;
        public Dictionary<string, int> Devices = new Dictionary<string, int>();


        int _selectedDeviceId = 0;
        public int SelectedDeviceId
        {
            get => _selectedDeviceId;
            set
            {
                if (_selectedDeviceId == value) return;
                bool restart = init;
                if (restart) StopCamera();
                _selectedDeviceId = value;
                if (restart) StartCamera();
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

            VideoDevices = CameraDevice.Enumerate();

            int c = 0;
            foreach (CameraDevice device in VideoDevices)
            {
                int d = c;
                Devices[device.Name] = d;
                
                Debug.WriteLine($"Found video device: {device.Name}");
                QB.Logger.Info($"Found video device: {device.Name}");
                _devices.Add(new BoxButton(name: "",description:device.Name, x: 0, y: 0, w: 60, h: 7,onClick:(e) => _=switchDevice(d)));

                c++;
            }

            OnVisibilityChanged += (s, e) =>
            {
                if (Visible)
                {
                    if (_startRequested) StartCamera();
                }
                else if (init)
                {
                    StopCamera();
                }

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
            if (IsRemoved) return;
            ShowDeviceList(false);
            SelectedDeviceId = id;
            StopCamera();
            await Task.Delay(1000);
            if (IsRemoved) return;
            StartCamera();

        }


        public void TakeSnapShoot() => SaveFrames = true;

        bool init = false;
        bool _startRequested = false;

        public void StartCamera()
        {
            if (IsRemoved) return;
            _startRequested = true;
            if (init || !Visible) return;
            if (videoSource != null) StopCamera();
            if (SelectedDeviceId < 0 || SelectedDeviceId >= VideoDevices.Count)
                throw new InvalidOperationException("No camera is available at the selected device index.");
            var selected = VideoDevices[SelectedDeviceId];
            var devices = CameraDevice.Enumerate();
            var current = devices.FirstOrDefault(device => device.MonikerString == selected.MonikerString);
            if (current == null)
                throw new InvalidOperationException("The selected camera is no longer available: " + selected.Name);
            int generation = Interlocked.Increment(ref captureGeneration);
            videoSource = new CameraCapture(current.Index, bitmap => QueueFrame(bitmap, generation));
            init = true;
        }

        public void StopCamera()
        {
            Interlocked.Increment(ref captureGeneration);
            var source = videoSource;
            source?.Dispose();
            videoSource = null;
            lock (updateGate)
            {
                var image = BackgroundImage;
                BackgroundImage = null;
                image?.Dispose();
                LastFrame?.Dispose();
                LastFrame = null;
                init = false;
            }
        }

        private void QueueFrame(Bitmap bitmap, int generation)
        {
            if (IsRemoved || generation != captureGeneration || Interlocked.Exchange(ref framePending, 1) != 0) return;
            var frame = (Bitmap)bitmap.Clone();
            var form = System.Windows.Forms.Application.OpenForms.Cast<System.Windows.Forms.Form>().FirstOrDefault();
            if (form == null || !form.IsHandleCreated || form.IsDisposed)
            {
                frame.Dispose();
                Interlocked.Exchange(ref framePending, 0);
                return;
            }
            try
            {
                form.BeginInvoke(new System.Action(() =>
                {
                    try
                    {
                        if (!IsRemoved && generation == captureGeneration) ProcessFrame(frame);
                    }
                    finally
                    {
                        frame.Dispose();
                        Interlocked.Exchange(ref framePending, 0);
                    }
                }));
            }
            catch (InvalidOperationException)
            {
                frame.Dispose();
                Interlocked.Exchange(ref framePending, 0);
            }
        }
        bool _deviceMenuVisible = false;
        void ShowDeviceList(bool visible)
        {
            if (IsRemoved) return;
            if (BackgroundImage == null) return;
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


        public Bitmap LastFrame;



        private void ProcessFrame(Bitmap sourceFrame)
        {
            lock (updateGate)
            {
                if (IsRemoved) return;
                _frameCounter++;
                if (SnapshotCounter != 0)
                {
                    _lastSnapshotCounter++;
                    if (_lastSnapshotCounter == _frameCounter) SaveFrames = true;
                }
                if (_frameCounter < Math.Max(1, FrameRate)) return;

                using var frame = (Bitmap)sourceFrame.Clone();
                var previousFrame = LastFrame;
                LastFrame = (Bitmap)sourceFrame.Clone();
                previousFrame?.Dispose();
                var smallFrame = ResizeBitmap(frame, frame.Width / 4, frame.Height / 4);
                var previousImage = BackgroundImage;
                BackgroundImage = smallFrame;
                previousImage?.Dispose();
                _frameCounter = 0;
                try
                {
                    if (SaveFrames)
                    {
                        string fileName = $"frame_{DateTime.Now:yyyyMMdd_HHmmssfff}.jpg";
                        smallFrame.Save(System.IO.Path.Combine(QB.Book.DataDirectory, fileName), System.Drawing.Imaging.ImageFormat.Jpeg);
                        SaveFrames = false;
                        _lastSnapshotCounter = 0;
                    }
                    OnUpdate?.Invoke(this, new UpdateEventArgs { Bitmap = smallFrame });
                }
                catch (Exception ex) { QB.Logger.Error("Camera frame update failed: " + ex.Message); }
            }
        }
        public void CreateMenu(object sender, UpdateEventArgs e)
        {
            lock (updateGate)
            {
                if (IsRemoved) return;
                if(BackgroundImage == null) return;
                if (BackgroundImage == null) return;
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
        }


        public override void Destroy()
        {
            _startRequested = false;
            StopCamera();
            base.Destroy();
        }

        private Bitmap ResizeBitmap(Bitmap source, int width, int height)
        {
            Bitmap small = new Bitmap(Math.Max(1, width), Math.Max(1, height));

            using (Graphics g = Graphics.FromImage(small))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                g.DrawImage(source, 0, 0, Math.Max(1, width), Math.Max(1, height));

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

namespace QB.Controls
{
    public sealed class CameraDevice
    {
        public string Name { get; }
        public string MonikerString { get; }
        public int Index { get; }
        private CameraDevice(string name, string moniker, int index)
        {
            Name = name;
            MonikerString = moniker;
            Index = index;
        }

        [ComImport, Guid("29840822-5B84-11D0-BD3B-00A0C911CE86"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ICreateDevEnum
        {
            [PreserveSig] int CreateClassEnumerator(ref Guid category, out IEnumMoniker enumerator, int flags);
        }

        [ComImport, Guid("55272A00-42CB-11CE-8135-00AA004BB851"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPropertyBag
        {
            [PreserveSig] int Read([MarshalAs(UnmanagedType.LPWStr)] string name,
                [MarshalAs(UnmanagedType.Struct)] out object value, IntPtr errorLog);
            [PreserveSig] int Write([MarshalAs(UnmanagedType.LPWStr)] string name,
                [MarshalAs(UnmanagedType.Struct)] ref object value);
        }

        public static IReadOnlyList<CameraDevice> Enumerate()
        {
            var devices = new List<CameraDevice>();
            object devEnum = Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("62BE5D10-60EB-11D0-BD3B-00A0C911CE86")));
            IEnumMoniker enumerator = null;
            try
            {
                var category = new Guid("860BB310-5D01-11D0-BD3B-00A0C911CE86");
                int result = ((ICreateDevEnum)devEnum).CreateClassEnumerator(ref category, out enumerator, 0);
                if (result == 1) return devices;
                Marshal.ThrowExceptionForHR(result);
                var monikers = new IMoniker[1];
                int index = 0;
                while (enumerator.Next(1, monikers, IntPtr.Zero) == 0)
                {
                    object bag = null;
                    try
                    {
                        var bagId = typeof(IPropertyBag).GUID;
                        monikers[0].BindToStorage(null, null, ref bagId, out bag);
                        Marshal.ThrowExceptionForHR(((IPropertyBag)bag).Read("FriendlyName", out object name, IntPtr.Zero));
                        monikers[0].GetDisplayName(null, null, out string moniker);
                        devices.Add(new CameraDevice(Convert.ToString(name), moniker, index));
                    }
                    finally
                    {
                        if (bag != null) Marshal.ReleaseComObject(bag);
                        Marshal.ReleaseComObject(monikers[0]);
                        index++;
                    }
                }
                return devices;
            }
            finally
            {
                if (enumerator != null) Marshal.ReleaseComObject(enumerator);
                Marshal.ReleaseComObject(devEnum);
            }
        }
    }

    internal sealed class CameraCapture : IDisposable
    {
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();
        private readonly Thread worker;
        private readonly VideoCapture capture;
        private bool disposed;

        public CameraCapture(int index, System.Action<Bitmap> onFrame, bool preferDialogResolution = false)
        {
            capture = new VideoCapture();
            try
            {
                // Device names and indices both use the DirectShow video-input enumeration.
                if (!capture.Open(index, VideoCaptureAPIs.DSHOW))
                    throw new InvalidOperationException("Cannot open the selected DirectShow camera.");
                if (preferDialogResolution && !capture.Set(VideoCaptureProperties.FrameHeight, 600))
                    if (!capture.Set(VideoCaptureProperties.FrameHeight, 1080)) capture.Set(VideoCaptureProperties.FrameWidth, 1920);
                worker = new Thread(() =>
                {
                    try
                    {
                        using var mat = new Mat();
                        while (!cancellation.IsCancellationRequested)
                        {
                            if (!capture.Read(mat) || mat.Empty())
                            {
                                if (cancellation.Token.WaitHandle.WaitOne(50)) break;
                                continue;
                            }
                            using var bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
                            if (!cancellation.IsCancellationRequested) onFrame(bitmap);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!cancellation.IsCancellationRequested) QB.Logger.Error("Camera capture failed: " + ex.Message);
                    }
                    finally { capture.Dispose(); }
                }) { IsBackground = true, Name = "qbook-camera" };
                worker.Start();
            }
            catch
            {
                capture.Dispose();
                cancellation.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            cancellation.Cancel();
            if (worker == Thread.CurrentThread)
                throw new InvalidOperationException("Camera capture cannot synchronously stop itself.");
            if (!worker.Join(2000))
                throw new InvalidOperationException("The camera has not stopped cooperatively; restart is unavailable until capture exits.");
            cancellation.Dispose();
            disposed = true;
        }
    }
}