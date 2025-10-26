//using CefSharp.DevTools.DOM;
using QB.Automation;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace QB.Net
{
    public partial class Stream
    {
        class Query
        {
            public Query()
            { }
            public string command = "";
            public string response = "";
            public string name = "";
            public DateTime lastQuery = DateTime.Now;
            public double repetionTime = 0;
        }

        public class Client : Machine
        {
            static List<Client> TcpClients = new List<Client>();

            public class StreamClientMessageReceivedEventArgs : EventArgs
            {
             //   public char Dcb = ' ';
                public int Port = 0;
                public string Client = "";
             //   public string Command = "????";
             //   public string Channel = "K0";
             //   public string[] Parameters = null;
                public byte[] Data;
                public int Length;
            }

            public Client(string name, string uri = "tcp://127.0.0.1:7700") : base(name)
            {
                Uri = uri;
                Connect(uri.Split('/')[uri.Split('/').Length - 1]);

            }

            public bool Connected => tcpClient.Connected;


            public string Uri;

            System.Threading.Thread idleThread;
            public string socket;
            _TcpClient tcpClient;

            public delegate void OnMessageReceivedDelegate(Client client, StreamClientMessageReceivedEventArgs ea);// int port =0, string client="", char dcb=' ', string command="????", string channel="K0", string[] parameters = null);
            public OnMessageReceivedDelegate OnMessageReceived;


            public override void Destroy()
            {
                
                Console.WriteLine("-------------------Destroying Client " + Name);
                base.Destroy();

               
                //    return;
                if (tcpClient != null)
                {
                    tcpClient.OnMessageReceived -= Port_MessageReceived;
                    tcpClient.Close();
                    tcpClient.Abort();
                }
                if (idleThread != null)
                    idleThread.Abort();

                tcpClient = null;
            }

            public void Connect(string socket)
            {
                //Close();
                this.socket = socket;
                tcpClient = new _TcpClient(Name, socket);
                tcpClient.OnMessageReceived += Port_MessageReceived;

                if (idleThread != null)
                    idleThread.Abort();

                idleThread = new System.Threading.Thread(Idle);
                idleThread.IsBackground = true;
                idleThread.Start();
            }


            private void Port_MessageReceived(_TcpClient s, byte[] data, int length)
            {
                lock (this)
                {
                    // if ((message.Length < 8) || (message[5] != ' '))
                    {
                        //   s.Transmit("*???? 0");
                        //  return;
                    }

                    try
                    {
                        /*
                        char dcb = message.Substring(0, 1)[0];
                        string command = message.Substring(1, 4);
                        message = message.Substring(6);
                        string channel = message.Split()[0];
                        string parameter = "";
                        if (message.Split().Length > 1)
                            parameter = message.Substring(message.IndexOf(' ') + 1);
                        */
                        if (OnMessageReceived != null)
                        {
                            StreamClientMessageReceivedEventArgs ea = new StreamClientMessageReceivedEventArgs();
                            ea.Data = data;
                            ea.Length = length;
                            //      ea.Port = port;
                            //     ea.Client = client;
                            // ea.Dcb = dcb;
                            // ea.Command = command;
                            // ea.Channel = channel;
                            // ea.Parameters = parameter.Trim().Split();
                            OnMessageReceived(this, ea);// port, client, dcb, command, channel, parameter.Trim().Split());

                            /*
                            if (response != null)
                            {
                                string responseString = response.ToString();
                                s.Transmit(Dcb + command + " " + Fsb + (responseString.Length > 0 ? " " + responseString : ""));
                            }
                            else
                                s.Transmit("!???? 0");
                            */
                        }
                        // else
                        //   s.Transmit("!???? 0");
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }


            public bool Transmit(string command)
            {
                lock (this)
                {
                    try
                    {
                        //     System.Threading.Monitor.TryEnter(this, timeout, ref lockTaken);
                        //    if (lockTaken)
                        {
                            DateTime start = DateTime.Now;
                            int dur = 500;
                            if (tcpClient != null)
                                return tcpClient.Transmit(command);
                        }
                        //      else
                        {
                            // The lock was not acquired.
                        }
                    }
                    finally
                    {
                        // Ensure that the lock is released.
                        //      if (lockTaken)
                        {
                            //        System.Threading.Monitor.Exit(this);
                        }
                    }
                    return false;
                }
            }

            private void Idle()
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }

        }


        class _TcpClient
        {
            string Text;
            // string Name;
            SerialPort serialPort = null;
            bool tcp = false;
            public string uri = "127.0.0.1:9001";


            public delegate void OnMessageReceivedDelegate(_TcpClient sender, byte[] data, int length);
            public OnMessageReceivedDelegate OnMessageReceived;

            public enum ReadType { Read, Write };
            public class ReadItem
            {
                public string Name;
                public string Command;
                public Int32 Index = -1;
                public string Format;
                public Int32 Timer = 0;
                public ReadType Type = ReadType.Read;
                public Int32 Value;
            }
            public List<ReadItem> ReadList = new List<ReadItem>();


            static List<_TcpClient> TcpClients = new List<_TcpClient>();

            internal static void ResetClients()
            {
                foreach (var c in TcpClients)
                    c.Close();
                TcpClients.Clear();
            }

            public _TcpClient(string text, string socket)
            {
                TcpClients.Add(this);
                Text = text;
                Socket = socket;// settings.GetItemValue("Socket", "").Replace("'","");
                Status = "init";

                if (Socket.ToUpper().Contains("COM") && (Socket.Split(',').Length == 3))
                {
                    if (Socket.Split(',')[2].ToUpper().Trim() == "8N1")

                        serialPort = new SerialPort(Socket.Split(',')[0], Socket.Split(',')[1].ToInt(), Parity.None, 8, StopBits.One);
                }
                else if (Socket.ToUpper().Contains("COM") && (Socket.Split(',').Length == 1))
                {
                    serialPort = new SerialPort(Socket.Split(',')[0], 9600, Parity.None, 8, StopBits.One);
                }
                else
                {
                    //  IpEndPoint = new IPEndPoint(IPAddress.Parse(Socket.Split(':')[0]), int.Parse(Socket.Split(':')[1]));
                    tcp = true;

                    tcpClient = new TcpClient();
                }


                idleThread = new System.Threading.Thread(Idle);
                idleThread.IsBackground = true;
                idleThread.Start();
            }

            public void Close()
            {
                if (idleThread != null)
                    idleThread.Abort();

                if (tcpClient != null)
                    tcpClient.Close();

                if (serialPort != null)
                    serialPort.Close();
            }


            public string Socket;


            static TimeSpan timeout = TimeSpan.FromMilliseconds(500);



            public delegate void LogEventHandler(object sender, string message);
            public event LogEventHandler LogEvent;

            private TcpClient tcpClient = null;
            private Thread idleThread;
            IPEndPoint IpEndPoint;
            public string Status { get; set; }

            void Log(object sender, string message)
            {
                //log.Info(message);
                if (LogEvent != null)
                    LogEvent(sender, message);
            }

            public void Abort()
            {
                try
                {
                    idleThread.Abort();
                }
                catch { }

                try
                {
                    if (tcpClient != null)
                        tcpClient.Close();
                }
                catch { }
            }

            bool _isDisconnecting = false;
            public void Disconnect(string message)
            {
                if (_isDisconnecting) return;
                //  log.Info($"disconnecting Client '{Name}': info:{message}");
                _isDisconnecting = true;
                try
                {
                    //          int checkTries = 0;
                    try
                    {
                        if (tcpClient != null)
                            tcpClient.Close();
                    }
                    catch
                    {
                        exceptionCounter++;
                    }

                    _connected = false;
                    connectionEstablished = false;
                    errorcounter = 0;
                    disconnectCounter++;
                    //      log.Info(Name + " disconnected <" + message + "," + exceptionCounter + ">");
                    Log(this, Text + " disconnected <" + message + "," + exceptionCounter + ">");
                    Status = "nc " + message + " " + disconnectCounter + " !(" + errorcounter + "/" + exceptionCounter + ")";

                }
                finally
                {
                    _isDisconnecting = false;
                }
            }

            private bool _connected = false;
            public bool Connected
            {
                get
                {
                    if (!_connected) return false;
                    try
                    {
                        if (serialPort != null)
                        {
                        }
                        if (tcpClient != null)
                        {
                            if (tcpClient.Client == null) return false; // !!!!
                            if (!tcpClient.Client.Connected) return false;
                        }

                    }
                    catch
                    {
                        exceptionCounter += 1000;
                    }
                    return true;
                }
            }

            bool connectionEstablished = false;
            private int disconnectCounter = 0;
            private int errorcounter = 0;
            private int exceptionCounter = 0;

            public void SocketConnect_Completed()
            {

            }

            public void Idle()
            {
                while (true)
                {
                    try
                    {
                        if (!Connected)
                        {
                            if (connectionEstablished)
                                Disconnect("reset");

                            if (tcp && (tcpClient == null))
                                tcpClient = new TcpClient();

                            if (tcpClient != null)
                            {
                                tcpClient = new TcpClient();
                                IpEndPoint = new IPEndPoint(IPAddress.Parse(Socket.Split(':')[0]), int.Parse(Socket.Split(':')[1]));
                                try
                                {


                                    SocketAsyncEventArgs m_socketConnectionArgs = new SocketAsyncEventArgs();
                                    m_socketConnectionArgs.RemoteEndPoint = IpEndPoint;
                                    m_socketConnectionArgs.Completed += M_socketConnectionArgs_Completed;
                                    tcpClient.ConnectAsync(Socket.Split(':')[0], int.Parse(Socket.Split(':')[1]));
                                    DateTime dateTime = DateTime.Now.AddSeconds(5);
                                    while (!tcpClient.Connected)
                                    {
                                        Thread.Sleep(500);
                                        if (DateTime.Now > dateTime)
                                            break;
                                    }
                                    // TcpClient.Connect(IpEndPoint);
                                    //      log.Info($"{Name}/{IpEndPoint.Address}: {IpEndPoint.Port} connected");
                                    if (tcpClient.Connected)
                                    {
                                        Log(this, $"{Text}/{IpEndPoint.Address}: {IpEndPoint.Port} connected");
                                        connectionEstablished = true;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //    log.Error($"{Name}/{IpEndPoint.Address}: {IpEndPoint.Port} retry ({ex.Message})");
                                    Log(this, Text + "/" + IpEndPoint.Address + ":" + IpEndPoint.Port + " retry " + ex.Message);
                                    Thread.Sleep(4000);
                                    connectionEstablished = false;
                                }
                            }
                            if (serialPort != null)
                            {
                                if (!serialPort.IsOpen)
                                    serialPort.Open();
                                _connected = true;
                            }
                        }
                        else
                        {
                            if ((tcpClient != null) && (tcpClient.Client != null))
                            {
                                while (tcpClient.Client.Available > 0)
                                {
                                    int readBytes = tcpClient.Client.Receive(receiveBytes, 4096, SocketFlags.None);
                                    if (readBytes > 0)
                                    {
                                        receivBufferIndex = 0;
                                        for (int i = 0; i < readBytes; i++)
                                        {
                                            receiveBuffer[receivBufferIndex] = receiveBytes[i];
                                            receivBufferIndex++;
                                            receivBufferIndex %= 8192;
                                            receiveBuffer[receivBufferIndex] = 0;
                                            /*                                        if (receiveBytes[i] == 0x02)
                                                                                        receivBufferIndex = 0;
                                                                                    else if (receiveBytes[i] == 0x03)
                                                                                    {
                                                                                        errorcounter = 0;
                                                                                        return Encoding.UTF8.GetString(receiveBuffer, 0, receivBufferIndex);

                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        receiveBuffer[receivBufferIndex] = receiveBytes[i];
                                                                                        receivBufferIndex++;
                                                                                        receivBufferIndex %= 8192;
                                                                                        receiveBuffer[receivBufferIndex] = 0;
                                                                                    }
                                            */
                                        }

                                        if (OnMessageReceived != null)
                                            OnMessageReceived(this, receiveBuffer, receivBufferIndex);// , System.Text.Encoding.Default.GetString(resp.ToArray()));

                                        // return Encoding.UTF8.GetString(receiveBuffer, 0, receivBufferIndex);
                                    }
                                }
                            }

                            if (serialPort != null)
                            {
                                bool msg = false;
                                receivBufferIndex = 0;


                                while (serialPort.BytesToRead > 0)
                                {
                                    int readBytes = serialPort.Read(receiveBytes, 0, serialPort.BytesToRead);
                                    if (readBytes > 0)
                                    {
                                        msg = true;

                                        for (int i = 0; i < readBytes; i++)
                                        {

                                            /*
                                            if (receiveBytes[i] == 0x02)
                                                receivBufferIndex = 0;
                                            else if (receiveBytes[i] == 0x03)
                                            {
                                                errorcounter = 0;
                                                return Encoding.UTF8.GetString(receiveBuffer, 0, receivBufferIndex);

                                            }
                                            else
                                            {
                                                receiveBuffer[receivBufferIndex] = receiveBytes[i];
                                                receivBufferIndex++;
                                                receivBufferIndex %= 8192;
                                                receiveBuffer[receivBufferIndex] = 0;
                                            }
                                            */
                                            receiveBuffer[receivBufferIndex] = receiveBytes[i];
                                            receivBufferIndex++;
                                            receivBufferIndex %= 8192;
                                            receiveBuffer[receivBufferIndex] = 0;
                                        }


                                    }
                                    Thread.Sleep(50);

                                }

                                if (msg && (OnMessageReceived != null))
                                    OnMessageReceived(this, receiveBuffer, receivBufferIndex);// System.Text.Encoding.Default.GetString(resp.ToArray()));
                                                                                              // return Encoding.UTF8.GetString(receiveBuffer, 0, receivBufferIndex);
                            }

                            Thread.Sleep(7); // -> ca. 10ms
                        }
                    }
                    catch
                    {
                        exceptionCounter++;
                        Disconnect("!!!! c1");
                    }

                    Thread.Sleep(10);



                    if (tcpClient != null)
                    {

                        if (connectionEstablished && (tcpClient.Client != null) && tcpClient.Client.Connected)
                        {

                        }



                        try
                        {
                            if (connectionEstablished && (tcpClient.Client != null) && tcpClient.Client.Connected && !_connected)
                            {
                                _connected = true;
                                Status = "ok " + disconnectCounter + " !(" + errorcounter + "/" + exceptionCounter + ")";

                            }
                        }
                        catch (Exception ex)
                        {
                            exceptionCounter++;
                            Disconnect("!!!! c2 " + ex.ToString());
                            Thread.Sleep(2000);
                        }

                        try
                        {
                            if (connectionEstablished && (tcpClient.Client != null) && !tcpClient.Client.Connected)
                            {
                                //   log.Info($"{Name}/{IpEndPoint.Address}: {IpEndPoint.Port} disconnecting");
                                Log(this, $"{Text}/{IpEndPoint.Address}: {IpEndPoint.Port} disconnecting");
                                Status = "nc** " + disconnectCounter + " !(" + errorcounter + "/" + exceptionCounter + ")";

                            }
                        }
                        catch (Exception ex)
                        {
                            exceptionCounter++;
                            Disconnect("!!!! c3 " + ex.ToString());
                            Thread.Sleep(2000);
                        }
                    }
                }
            }

            private void M_socketConnectionArgs_Completed(object sender, SocketAsyncEventArgs e)
            {
                throw new NotImplementedException();
            }



            byte[] receiveBytes = new byte[8192];
            byte[] receiveBuffer = new byte[8192];
            byte[] sendBuffer = new byte[8192];
            int receivBufferIndex = 0;
            public int TxMsgCount = 0;
            public bool Transmit(string message)
            {
                if (message == null)
                    return false;


                if (tcpClient != null)
                {
                    if (!Connected)
                    {
                        //  log.Warn($"{Name}/{IpEndPoint.Address}: {IpEndPoint.Port} not connected/not sent " + message);
                        //**       Log(this, $"{Name}/{IpEndPoint.Address}: {IpEndPoint.Port} not connected/not sent " + message);
                        return false;
                    }

                    try
                    {
                        while (tcpClient.Client.Available > 0)
                            tcpClient.Client.Receive(receiveBytes, 4096, SocketFlags.None);
                        receivBufferIndex = 0;
                    }
                    catch
                    {
                        exceptionCounter++;
                        Disconnect("$dummy");
                        return false;
                    }
                }


                if (serialPort != null)
                    if (serialPort.IsOpen)
                        serialPort.ReadExisting();

                try
                {
                    // sendBuffer[0] = 0x02;
                    int byteLength = message.Length;
                    for (int i = 0; i < byteLength; i++)
                        sendBuffer[i] = Convert.ToByte(message[i]);
                    //   byteLength += 2;
                    // sendBuffer[byteLength - 1] = 0x03;

                    if (tcpClient != null)
                        tcpClient.Client.Send(sendBuffer, byteLength, SocketFlags.None);


                    if (serialPort != null)
                        if (serialPort.IsOpen)
                            serialPort.Write(sendBuffer, 0, byteLength);

                    return true;

                    TxMsgCount++;
                }
                catch
                {
                    exceptionCounter++;
                    Disconnect("send");

                }
                return false;
            }
        }
    }
}