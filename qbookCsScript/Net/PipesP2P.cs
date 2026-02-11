using System;
using System.IO.Pipes;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Management;
using CSScripting;

namespace QB.Net
{

    public class PipeMessageEventArgs : EventArgs
    {
        public string Message { get; }
        public PipeMessageEventArgs(string message)
        {
            Message = message;
        }
    }
    public class PipesClient : Item
    {
        private string PipeName;
        private NamedPipeClientStream Client;
        private bool IsRunning = false;
        private StreamWriter writer;
        private BinaryWriter writerBinary;

        public PipesClient(string name, string pipeName) : base(name)
        {
            PipeName = pipeName;
        }

        public void Start()
        {
            IsRunning = true;
            QB.Logger.Info("PipesClient started.");
        }

        public void Stop()
        {
            IsRunning = false;

            if (Client != null)
            {
                Client.Close();
                Client = null;
            }

            QB.Logger.Info("PipesClient stopped.");
        }

        public void Transmit(string data)
        {
            if (!IsPipeOnline())
            {
                QB.Logger.Warn($"Pipe '{PipeName}' is offline. Transmission aborted.");
                return;
            }

            try
            {
                EnsureConnected();
                writer.WriteLine(data);
                QB.Logger.Info($"Sent: {data}");
            }
            catch (IOException ioEx)
            {
                QB.Logger.Error($"Pipe write error: {ioEx.Message}");
                Reconnect();
            }
        }

        public void TransmitBinary(byte[] data)
        {
            if (!IsPipeOnline())
            {
                QB.Logger.Warn($"Pipe '{PipeName}' is offline. Transmission aborted.");
                return;
            }

            try
            {
                EnsureConnected();

                Console.WriteLine($"[CLIENT] Preparing to send {data.Length} bytes...");

                if (writerBinary == null)
                {
                    writerBinary = new BinaryWriter(Client, Encoding.UTF8, true);
                }

                // **Länge schreiben, aber KEIN Flush()**
                Console.WriteLine($"[CLIENT] Writing length: {data.Length} (bytes: {BitConverter.ToString(BitConverter.GetBytes(data.Length))})");
                writerBinary.Write(data.Length);

                // **Daten schreiben, aber KEIN Flush()**
                Console.WriteLine("[CLIENT] Writing data...");
                writerBinary.Write(data);

                // **Jetzt nur einmal die Pipe zum Leeren bringen**
                Client.WaitForPipeDrain();

                Console.WriteLine($"[CLIENT] Successfully sent {data.Length} bytes.");
            }
            catch (IOException ioEx)
            {
                QB.Logger.Error($"Pipe write error: {ioEx.Message}");

                // Falls die Pipe kaputt ist, warte kurz und versuche es erneut
                Thread.Sleep(500);
                EnsureConnected();
            }
        }













        private void EnsureConnected()
        {
            if (Client == null || !Client.IsConnected)
            {
                Client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.Asynchronous);
                Client.Connect(5000);
                QB.Logger.Info("Connected to named pipe for transmission.");

                writer = new StreamWriter(Client, Encoding.UTF8) { AutoFlush = true };
                writerBinary = new BinaryWriter(Client);
            }
        }

        private void Reconnect()
        {
            try
            {
                if (Client != null)
                {
                    Client.Dispose();
                    Client = null;
                }

                QB.Logger.Info("Reconnecting to named pipe...");
                Client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.Asynchronous);
                Client.Connect(5000);

                writer = new StreamWriter(Client, Encoding.UTF8) { AutoFlush = true };
                writerBinary = new BinaryWriter(Client); // WICHTIG!

                QB.Logger.Info("Reconnected to named pipe.");
            }
            catch (Exception ex)
            {
                QB.Logger.Error($"Reconnect failed: {ex.Message}");
            }
        }

        public override void Destroy()
        {
            QB.Logger.Info("Closing Client");
        }

        private List<string> GetActivePipes()
        {
            List<string> pipeNames = new List<string>();

            try
            {
                string[] pipes = System.IO.Directory.GetFiles(@"\\.\pipe\");
                foreach (string pipe in pipes)
                {
                    pipeNames.Add(Path.GetFileName(pipe)); // Extrahiert nur den Pipe-Namen
                }
            }
            catch (UnauthorizedAccessException)
            {
                QB.Logger.Error("No permission to access pipe");
            }
            catch (Exception ex)
            {
                QB.Logger.Error($"Failed to get pipes list: {ex.Message}");
            }

            return pipeNames;
        }

        public bool IsPipeOnline()
        {
            return GetActivePipes().Contains(PipeName);
        }
    }


    public class PipesServer : Item
    {
        private string PipeName;
        private NamedPipeServerStream Server;
        private bool IsRunning = false;
        private Thread receiveThread;
        private ConcurrentQueue<string> ReceivedQueue = new ConcurrentQueue<string>();

        public event EventHandler<PipeMessageEventArgs> MessageReceived;

        public PipesServer(string name, string pipeName) : base(name)
        {
            PipeName = pipeName;
        }

       
        public void Start()
        {
            IsRunning = true;
            receiveThread = new Thread(Receive)
            {
                IsBackground = true
            };
            receiveThread.Start();
        }

        public void Stop()
        {
            IsRunning = false;
            if (receiveThread != null && receiveThread.IsAlive)
            {
                receiveThread.Join();
                receiveThread = null;
            }

            if (Server != null)
            {
                Server.Dispose();
                Server = null;
            }
        }

        private void Receive()
        {
            try
            {
                while (IsRunning)
                {
                    if (Server == null || !Server.IsConnected)
                    {
                        Console.WriteLine("[Server] Creating new PipeServer instance...");
                        Server = new NamedPipeServerStream(PipeName, PipeDirection.InOut, -1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                    }

                    if (!Server.IsConnected)
                    {
                        Console.WriteLine("[Server] Waiting for client connection...");
                        Server.WaitForConnection();
                        Console.WriteLine("[Server] Client connected.");
                    }

                    using (StreamReader reader = new StreamReader(Server, Encoding.UTF8))
                    {
                        while (IsRunning && Server.IsConnected)
                        {
                            try
                            {
                                string message = reader.ReadLine();
                                if (!string.IsNullOrEmpty(message))
                                {
                                    ReceivedQueue.Enqueue(message);
                                    OnReceived(new PipeMessageEventArgs(message));
                                    Console.WriteLine($"[Server] Received: {message}");
                                }
                            }
                            catch (IOException ioEx)
                            {
                                Console.WriteLine($"[Server] Pipe read error: {ioEx.Message}");
                                break;  // Client disconnected
                            }
                        }
                    }

                    Console.WriteLine("[Server] Client disconnected. Waiting for new connection...");
                    Server.Disconnect();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] PipesServer failed: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("[Server] Server stopping.");
            }
        }

        protected virtual void OnReceived(PipeMessageEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        public override void Destroy()
        {
            Console.WriteLine("Closing Server");
            Stop();
        }
    }




}
