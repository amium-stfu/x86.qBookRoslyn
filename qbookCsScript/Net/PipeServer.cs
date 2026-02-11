using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Drawing;

namespace QB.Net
{

    public class MeasurementData
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public string Unit { get; set; }
        public double Value { get; set; }
        public double Epoch { get; set; }
        public DateTime Timestamp { get; set; }
    }
    public class PipeData
    {
        public string Name
        {
            get; set;
        }
        public string Text
        {
            get; set;
        }
        public string Unit
        {
            get; set;
        }
        public string Color
        {
            get; set;
        }
        public string Datetime
        {
            get; set;
        }
        public double Value
        {
            get; set;
        }

        public PipeData()
        {
        }

        public PipeData(Signal signal)
        {
            Name = signal.Name;
            Text = signal.Text;
            Unit = signal.Unit;
            Color = ColorTranslator.ToHtml(signal.Color);
            Datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Value = NormalizeValue(signal.Value);
        }

        private double NormalizeValue(double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                return 0.0; // Replace with a valid number
            }
            return value;
        }
    }

    public class PipeServer : Item
    {
        NamedPipeServerStream Server;
        List<Signal> Signals = new List<Signal>();
        string TransferString = "";

        bool IsRunning = false;
        System.Threading.Thread write;

        private readonly object signalsLock = new object();

        public PipeServer(string name) : base(name) { }

        public void Add(Signal signal)
        {
            lock (signalsLock)
            {
                Signals.Add(signal);
            }
        }

        public void Start()
        {
            IsRunning = true;
            write = new System.Threading.Thread(Write)
            {
                IsBackground = true
            };
            QB.Logger.Info("PipeServer thread starting.");
            write.Start();
        }

        public void Stop()
        {
            IsRunning = false;
            if (write != null && write.IsAlive)
            {
                write.Join();
                write = null;
            }
            QB.Logger.Info("PipeServer thread stopped.");
        }

        void Write()
        {
            try
            {
                while (IsRunning)
                {
                    using (Server = new NamedPipeServerStream(Name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                    {
                        QB.Logger.Info("Waiting for client connection...");

                        try
                        {
                            Server.WaitForConnection();
                        }
                        catch (ObjectDisposedException)
                        {
                            // This is expected when Destroy() disposes the pipe
                            QB.Logger.Info("Pipe server was disposed. Exiting.");
                            break;
                        }

                        QB.Logger.Info("Client connected.");

                        StreamReader reader = new StreamReader(Server, Encoding.UTF8);
                        StreamWriter writer = new StreamWriter(Server, Encoding.UTF8)
                        {
                            AutoFlush = true
                        };

                        while (IsRunning && Server.IsConnected)
                        {
                            List<PipeData> list;
                            lock (signalsLock)
                            {
                                list = Signals.Select(s => new PipeData(s)).ToList();
                            }

                            TransferString = JsonSerializer.Serialize(list, new JsonSerializerOptions
                            {
                                IncludeFields = true
                            });

                            try
                            {
                                writer.WriteLine(TransferString);
                                //QB.Logger.Info(TransferString);
                            }
                            catch (IOException ioEx)
                            {
                                QB.Logger.Error($"Pipe write error: {ioEx.Message}");
                                break;  // Client disconnected
                            }

                            System.Threading.Thread.Sleep(1000); // 100Hz
                        }

                        QB.Logger.Info("Client disconnected, waiting for next client...");
                    }
                }
            }
            catch (Exception ex)
            {
                QB.Logger.Error($"PipeServer failed: {ex.Message}");
            }
            finally
            {
                QB.Logger.Info("Server stopping.");
            }
        }


        public override void Destroy()
        {
            QB.Logger.Info("Closing Server");

            IsRunning = false;  // Request graceful shutdown

            if (Server != null)
            {
                Server.Close();   // Will cause WaitForConnection to throw if waiting
                Server.Dispose();
            }

            if (write != null && write.IsAlive)
            {
                write.Join();  // Wait for the write thread to actually exit
                write = null;
            }

            base.Destroy();
        }

    }



}
