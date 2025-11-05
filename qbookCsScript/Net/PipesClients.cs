using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QB.Net
{
    public class PipesSignalStreamClient : Item
    {
        private string PipeName;
        private NamedPipeClientStream Client;
        private BinaryWriter Writer;
        private bool isConnected = false; // Verbindung bleibt offen

        public string SentString;

        private List<Signal> signals = new List<Signal>();

        public PipesSignalStreamClient(string name, string pipeName) : base(name)
        {
            PipeName = pipeName;
        }

        public void Add(Signal signal)
        {
            signals.Add(signal);
        }

        private List<MeasurementData> Signals()
        {
            List<MeasurementData> data = new List<MeasurementData>();
            DateTimeOffset now = DateTimeOffset.Now;
            double Epoch = now.ToUnixTimeSeconds();
            DateTime Timestamp = DateTime.Now;

            foreach (Signal measurement in signals)
            {
                data.Add(new MeasurementData()
                {
                    Name = measurement.Name,
                    Text = measurement.Text,
                    Unit = measurement.Unit,
                    Value = measurement.Value == null ? double.NaN : measurement.Value,
                    Epoch = Epoch,
                    Timestamp = Timestamp
                });
            }
            return data;
        }

        public void Connect()
        {
            if (isConnected) return; // Falls bereits verbunden, nichts tun

            try
            {
                Client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.Asynchronous);
                Console.WriteLine("[CLIENT] Connecting to server...");

                Client.Connect(5000); // **Timeout von 5 Sekunden**
                Writer = new BinaryWriter(Client);
                isConnected = true;
                Console.WriteLine("[CLIENT] Connected.");
        }
            catch (TimeoutException)
            {
                Console.WriteLine("[CLIENT] Connection timeout: Server not available.");
                isConnected = false;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[CLIENT] Pipe error: {ex.Message}");
                isConnected = false;
            }
        }

        public void Transmit()

        {
            List<MeasurementData> measurements = Signals();
            if (!isConnected || Writer == null)
            {
                Console.WriteLine("[CLIENT] Pipe is not connected. Attempting to reconnect...");
                Connect();

                if (!isConnected || Writer == null)
                {
                    Console.WriteLine("[CLIENT] ERROR: Failed to establish a connection.");
                    return;
                }
            }

            try
            {
                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    // **Liste serialisieren**
                    bw.Write(measurements.Count);

                    foreach (var measurement in measurements)
                    {
                        WriteString(bw, measurement.Name);
                        WriteString(bw, measurement.Text);
                        WriteString(bw, measurement.Unit);
                        bw.Write(measurement.Value);
                        bw.Write(measurement.Epoch);
                        bw.Write(measurement.Timestamp.ToBinary());
                    }

                    byte[] data = ms.ToArray();

                    SentString = BitConverter.ToString(data);
                    // **HEX Debugging vor dem Senden**
                    Console.WriteLine($"[DEBUG] Raw Data Sent (Hex): {SentString}");

                    // **Länge senden**
                    int length = data.Length;
                    Console.WriteLine($"[CLIENT] Sending length: {length} bytes");
                    Writer.Write(length);
                    Writer.Flush();

                    // **Daten senden**
                    Console.WriteLine("[CLIENT] Sending measurement list...");
                    Writer.Write(data);
                    Writer.Flush();

                    Console.WriteLine("[CLIENT] Transmission complete.");
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[CLIENT] Pipe error: {ex.Message}. Reconnecting...");
                isConnected = false;
                Connect();
            }
        }
        private void WriteString(BinaryWriter writer, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                writer.Write(0); // **Leerer String wird mit Länge 0 gespeichert**
            }
            else
            {
                byte[] stringBytes = Encoding.UTF8.GetBytes(value);
                writer.Write(stringBytes.Length);
                writer.Write(stringBytes);
            }
        }

        public void Close()
        {
            Console.WriteLine("[CLIENT] Closing connection.");
            Writer?.Close();
            Client?.Close();
            isConnected = false;
        }

        public override void Destroy()
        {
            Close();
        }
    }
}
