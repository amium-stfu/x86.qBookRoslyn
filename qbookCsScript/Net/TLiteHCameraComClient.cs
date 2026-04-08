using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QB.Net
{
    public class TLiteHCameraComClient : Item
    {
        private readonly string _portName;
        private readonly int _baudRate;
        private System.IO.Ports.SerialPort _serialPort;

        public StringSignal DateTime = new StringSignal("DateTime", "");
        public Signal Tmin = new Signal("Tmin", "Min", unit: "°C") { DefaultDisplayFormat = "0.0" };
        public Signal Tmax = new Signal("Tmax", "Max", unit: "°C") { DefaultDisplayFormat = "0.0" };
        public Signal Tavg = new Signal("Tavg", "Avg", unit: "°C") { DefaultDisplayFormat = "0.0" };

        public Bitmap LastFrameBitmap = null;


        public TLiteHCameraComClient(string name, string portName = "COM1", int baudRate = 115200) : base(name)
        {
            _portName = portName;
            _baudRate = baudRate;
            _serialPort = new System.IO.Ports.SerialPort(_portName, _baudRate)
            {
                Encoding = Encoding.UTF8,
                ReadTimeout = 1000,
                NewLine = "\n"
                
            };
            OnUpdate += (s, e) =>
            {
                DateTime.Value = e.Frame.DateTimeText;
                Tmin.Value = e.Frame.Lowest;
                Tmax.Value = e.Frame.Highest;
                Tavg.Value = e.Frame.Average;
                LastFrameBitmap = e.Bitmap;

                
            };
        }

        public void Open()
        {
            if (!_serialPort.IsOpen)
                _serialPort.Open();
        }

        public void Close()
        {
            if (_serialPort.IsOpen)
                _serialPort.Close();
        }


        public TLiteJsonFrame LastReceived = null;
        public System.Drawing.Bitmap LastBitmap = null;

        public event System.Action<TLiteJsonFrame, System.Drawing.Bitmap> FrameReceived;

        private bool _receiving = false;
        private System.Threading.CancellationTokenSource _cts;

        public async void StartReceivingAsync()
        {
            if (_serialPort == null)
            {
                QB.Logger.Error($"[TLiteHCameraComClient] Serialport instance is null for {_portName}");
                return;
            }

            // Check available ports to ensure the configured port exists
            string[] availablePorts;
            try
            {
                availablePorts = System.IO.Ports.SerialPort.GetPortNames();
            }
            catch (Exception ex)
            {
                QB.Logger.Error($"[TLiteHCameraComClient] Failed to enumerate serial ports: {ex.Message}");
                return;
            }

            bool portExists = Array.Exists(availablePorts, p => string.Equals(p, _portName, StringComparison.OrdinalIgnoreCase));
            if (!portExists)
            {
                string avail = availablePorts.Length > 0 ? string.Join(", ", availablePorts) : "<none>";
                QB.Logger.Error($"[TLiteHCameraComClient] Port '{_portName}' not found. Available ports: {avail}");
                return;
            }

            // Try to open the port and handle exceptions (e.g. access denied, IO errors)
            try
            {
                if (!_serialPort.IsOpen)
                    _serialPort.Open();
            }
            catch (Exception ex)
            {
                QB.Logger.Error($"[TLiteHCameraComClient] Failed to open serial port '{_portName}': {ex.Message}");
                return;
            }

            if (_receiving) return;
            _receiving = true;
            _cts = new System.Threading.CancellationTokenSource();
            var token = _cts.Token;
            await System.Threading.Tasks.Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var frame = ReceiveFrame();
                        if (frame != null && LastBitmap != null)
                        {
                            FrameReceived?.Invoke(frame, LastBitmap);
                        }
                    }
                    catch
                    {
                        // swallow exceptions to keep loop running; optionally add a small delay
                        System.Threading.Thread.Sleep(10);
                    }
                }
            }, token);
        }

        public void StopReceiving()
        {
            if (!_receiving) return;
            _cts?.Cancel();
            _receiving = false;
            if (_serialPort.IsOpen)
                _serialPort.Close();
        }

        public override void Destroy()
        {
            StopReceiving();
            _serialPort.Dispose();
        }

        public event EventHandler<UpdateEventArgs> OnUpdate;


        public class UpdateEventArgs : EventArgs
        {
            public TLiteJsonFrame Frame { get; set; }
            public Bitmap Bitmap { get; set; }
        }

        public TLiteJsonFrame ReceiveFrame()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                bool started = false;
                while (true)
                {
                    string line = _serialPort.ReadLine();
                    //    Debug.WriteLine(line);
                    if (!started && line.Trim().StartsWith("{"))
                        started = true;
                    if (started)
                    {
                        sb.AppendLine(line);
                        if (line.Trim().EndsWith("}"))
                            break;
                    }
                }
                string json = sb.ToString();
              //  Debug.WriteLine(json);
                LastReceived = JsonSerializer.Deserialize<TLiteJsonFrame>(json);
                LastBitmap = CreateHeatingMap(LastReceived);

                OnUpdate?.Invoke(this, new UpdateEventArgs { Frame = LastReceived, Bitmap = LastBitmap });

                return LastReceived;
            }
            catch
            {
                return null;
            }
        }

        public System.Drawing.Bitmap CreateHeatingMap(TLiteJsonFrame frame)
        {
            if (frame == null || frame.Frame == null || frame.Frame.Length != frame.Width * frame.Height)
            {
                QB.Logger.Error("CreateHeatingMap: Ungültiger Frame oder Frame-Länge. pixel: " + frame.Frame.Length);
                return null;
            }



            var bmp = new System.Drawing.Bitmap(frame.Width, frame.Height);
            double min = frame.Lowest;
            double max = frame.Highest;
            for (int y = 0; y < frame.Height; y++)
            {
                for (int x = 0; x < frame.Width; x++)
                {
                    int idx = y * frame.Width + x;
                    double temp = frame.Frame[idx];
                    var color = TemperatureToColor(temp, min, max);
                    bmp.SetPixel(x, y, color);
                }
            }
          //  System.Diagnostics.Debug.WriteLine($"CreateHeatingMap: Bitmap erzeugt ({bmp.Width}x{bmp.Height})");
            return bmp;
        }

        private System.Drawing.Color TemperatureToColor(double temp, double min, double max)
        {
            double t = (temp - min) / (max - min);
            t = Math.Max(0, Math.Min(1, t));
            if (t < 0.25)
                return System.Drawing.Color.FromArgb(0, (int)(255 * t / 0.25), 255);
            else if (t < 0.5)
                return System.Drawing.Color.FromArgb(0, 255, (int)(255 * (1 - (t - 0.25) / 0.25)));
            else if (t < 0.75)
                return System.Drawing.Color.FromArgb((int)(255 * ((t - 0.5) / 0.25)), 255, 0);
            else
                return System.Drawing.Color.FromArgb(255, (int)(255 * (1 - (t - 0.75) / 0.25)), 0);

            
        }
    }


    public sealed class TLiteJsonFrame
    {
        [JsonPropertyName("pwd")]
        public string Password { get; set; }

        [JsonPropertyName("datetime")]
        public string DateTimeText { get; set; }

        [JsonPropertyName("interval")]
        public int Interval { get; set; }

        [JsonPropertyName("macaddr")]
        public string MacAddress { get; set; }

        [JsonPropertyName("center")]
        public double Center { get; set; }

        [JsonPropertyName("average")]
        public double Average { get; set; }

        [JsonPropertyName("highest")]
        public double Highest { get; set; }

        [JsonPropertyName("lowest")]
        public double Lowest { get; set; }

        [JsonPropertyName("frame")]
        public double[] Frame { get; set; } = null;

        [JsonIgnore]
        public int Width => 32;

        [JsonIgnore]
        public int Height => 24;

        public double GetValue(int x, int y)
        {
            int index = y * Width + x;
            return Frame[index];
        }

        public double[,] ToMatrix()
        {
            var matrix = new double[Height, Width];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    matrix[y, x] = Frame[y * Width + x];
                }
            }

            return matrix;
        }

    

        public static TLiteJsonFrame Parse(string json)
        {
            return JsonSerializer.Deserialize<TLiteJsonFrame>(json)
                   ?? throw new System.InvalidOperationException("JSON konnte nicht gelesen werden.");
        }
    }
}
