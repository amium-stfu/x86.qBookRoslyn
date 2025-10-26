
using QB.Automation;
using System;
using System.Net;
using System.Net.Sockets;

namespace QB.Net
{
    public class Can
    {
        public class Client : Machine
        {
            public delegate void OnMessageReceivedDelegate(Client can, Message cm);
            public OnMessageReceivedDelegate OnMessageReceived;

            public Can.Link CanClient;
            public string uri = "127.0.0.1:9001";

            public Client(string name, string uri = "127.0.0.1:9001") : base(name)
            {
                Timer.Interval = 1000;
                this.uri = uri;
                Open(uri);
            }

            public override void Destroy()
            {
                base.Destroy();
                if (CanClient != null)
                    CanClient.Close();
            }

            public override void Run()
            {
                base.Run();
                Timer.OnElapsed -= _Elapsed;
                Timer.OnElapsed += _Elapsed;
            }

            private void _Elapsed(Timer t, TimerEventArgs ea)
            {
            }

            public bool RemoteTime = false;

            public void Open(string uri)
            {
                try
                {
                    if (CanClient != null)
                        CanClient.Close();

                    CanClient = new Can.Link(uri);
                    CanClient.MessageReceived += Can_MessageReived;

                    Run();
                }
                catch
                {
                }
            }

            private void Can_MessageReived(object sender, uint id, byte dlc, byte[] data)
            {
                byte[] data2 = new byte[dlc];
                Array.Copy(data, data2, dlc);
                Message cm = new Message(id, data2);
                if (OnMessageReceived != null)
                    OnMessageReceived(this, cm);

                //   Console.WriteLine(id + " " + dlc);
            }

            public void Transmit(uint id, byte[] data)
            {
                CanClient.Transmit(new Message(id, data));
            }

            public void Transmit(Message cm)
            {
                if (CanClient != null)
                    CanClient.Transmit(cm);
            }
        }


        public class Message
        {
            public DateTime Date = DateTime.Now;
            public UInt32 Id { get; set; }
            public byte[] Data = new byte[8];
            public Message(UInt32 id, byte[] bytes)
            {
                Date = DateTime.Now;
                Id = id;
                Data = bytes;
            }
            public override string ToString()
            {
                string text = Id.ToString("X4") + ":";
                for (int i = 0; i < 8; i++)
                {
                    if (i < Data.Length)
                        text += " " + Data[i].ToString("X2");
                    else
                        text += " --";
                }
                return text;
            }
        }

        public class Link
        {
            public delegate void LogEventHandler(object s, string message);
            public event LogEventHandler LogEvent;

            public delegate void OnMessageReived(object s, UInt32 id, byte dlc, byte[] data);
            public event OnMessageReived MessageReceived;

            public static int readMsgs = 0;

            UdpClient udpClient = null;
            System.Threading.Thread rxThread = null;
            System.Threading.Thread txThread = null;

            public Link(string socket)
            {
                udpClient = new UdpClient();

                string s = socket.Split('-')[0];
                string port = "9001";
                if (s.Contains(":"))
                    port = s.Split(':')[1];

                udpClient.Connect(s.Split(':')[0], int.Parse(port));

                rxThread = new System.Threading.Thread(rxThreadIdle);
                rxThread.IsBackground = true;
                rxThread.Priority = System.Threading.ThreadPriority.Highest;
                rxThread.Start();

                txThread = new System.Threading.Thread(txThreadIdle);
                txThread.IsBackground = true;
                txThread.Start();
            }

            public void Close()
            {
                if (rxThread != null)
                    rxThread.Abort();
                if (txThread != null)
                    txThread.Abort();
                if (udpClient != null)
                    udpClient.Close();
            }

            System.Collections.Generic.Queue<Message> txBuffer = new System.Collections.Generic.Queue<Message>();
            public void Transmit(Message canMessage)
            {
                lock (txBuffer)
                {
                    txBuffer.Enqueue(canMessage);
                    //FormCANview.OnCanMessageReceived(canMessage.Id, (byte)canMessage.Data.Length, canMessage.Data, true);
                }

            }
            void txThreadIdle()
            {
                byte[] bytes = new byte[1500];
                int packageCounter = 0;
                while (true)
                {
                    if (txBuffer.Count > 0)
                    {
                        try
                        {
                            bytes[0] = (byte)(packageCounter >> 24);
                            bytes[1] = (byte)(packageCounter >> 16);
                            bytes[2] = (byte)(packageCounter >> 8);
                            bytes[3] = (byte)(packageCounter >> 0);
                            int offset = 4;

                            while ((txBuffer.Count > 0) && (offset < 1450))
                            {
                                Message canMessage;
                                lock (txBuffer)
                                {
                                    canMessage = txBuffer.Dequeue();
                                }

                                long timestamp = DateTime.Now.Ticks;
                                bytes[offset + 0] = (byte)(canMessage.Data.Length + 12);
                                bytes[offset + 1] = (byte)(timestamp >> 24);
                                bytes[offset + 2] = (byte)(timestamp >> 16);
                                bytes[offset + 3] = (byte)(timestamp >> 8);
                                bytes[offset + 4] = (byte)(timestamp >> 0);
                                bytes[offset + 5] = (byte)0;// numericUpDownChannel.Value;
                                bytes[offset + 6] = (byte)0;// Convert.ToByte(textBoxMsgType.Text);
                                bytes[offset + 7] = (byte)(canMessage.Id >> 24);
                                bytes[offset + 8] = (byte)(canMessage.Id >> 16);
                                bytes[offset + 9] = (byte)(canMessage.Id >> 08);
                                bytes[offset + 10] = (byte)(canMessage.Id >> 00);
                                bytes[offset + 11] = (byte)(canMessage.Data.Length);
                                if (canMessage.Data.Length > 0)
                                {
                                    for (int i = 0; i < canMessage.Data.Length; i++)
                                        bytes[offset + 12 + i] = canMessage.Data[i];
                                }

                                offset = (int)(offset + 12 + canMessage.Data.Length);
                            }

                            packageCounter++;

                            if (udpClient.Client.Connected)
                                udpClient.Send(bytes, offset);

                        }
                        catch (Exception ex)
                        {
                            if (LogEvent != null)
                                LogEvent(this, "txThreadIdle " + ex.ToString());
                            //   System.Windows.Forms.MessageBox.Show("CanUdp TransmitUdp " + ex.ToString());
                        }
                    }
                    System.Threading.Thread.Sleep(10);
                }
            }

            public System.Collections.Generic.Queue<byte[]> msgs = new System.Collections.Generic.Queue<byte[]>();

            void rxThreadIdle()
            {
                IPEndPoint ipep;
                UdpClient newsock = null;
                IPEndPoint sender = null;
                System.Threading.Thread.Sleep(100);
                try
                {
                    int port = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    ipep = new IPEndPoint(IPAddress.Any, port);
                    newsock = new UdpClient((IPEndPoint)udpClient.Client.LocalEndPoint);
                    newsock.DontFragment = true;
                    if (LogEvent != null)
                        LogEvent(this, "Waiting for a client...");
                    sender = new IPEndPoint(IPAddress.Any, port);
                }
                catch (Exception ex)
                {
                    if (LogEvent != null)
                        LogEvent(this, "rxThreadIdle " + ex.ToString());
                }
                if (newsock == null)
                    return;

                byte[] data = new byte[64];
                while (true)
                {
                    try
                    {
                        while (newsock.Available > 0)
                        {
                            byte[] bytes = newsock.Receive(ref sender);
                            /*
    UInt32 msgCounter =
        ((UInt32)bytes[0] << 24) +
        ((UInt32)bytes[1] << 16) +
        ((UInt32)bytes[2] << 08) +
        ((UInt32)bytes[3] << 00);
    */
                            int offset = 4;
                            if (bytes != null)
                                while (bytes.Length >= (offset + 12))
                                {
                                    int totalLen = bytes[offset + 0];
                                    /*
                                    long timestamp =
                                        ((UInt32)bytes[offset + 1] << 24) +
                                        ((UInt32)bytes[offset + 2] << 16) +
                                        ((UInt32)bytes[offset + 3] << 08) +
                                        ((UInt32)bytes[offset + 4] << 00);
                                    */
                                    int channel = bytes[offset + 5];
                                    byte msgType = bytes[offset + 6];
                                    UInt32 id =
                                        ((UInt32)bytes[offset + 7] << 24) +
                                        ((UInt32)bytes[offset + 8] << 16) +
                                        ((UInt32)bytes[offset + 9] << 08) +
                                        ((UInt32)bytes[offset + 10] << 00);
                                    byte dlc = bytes[offset + 11];

                                    if ((dlc >= 0) && (dlc <= 8))
                                    {
                                        Array.Copy(bytes, offset + 12, data, 0, dlc);

                                        // FormCANview.OnCanMessageReceived(id, dlc, data,false);

                                        if (MessageReceived != null)
                                            MessageReceived(this, id, dlc, data);
                                        readMsgs++;
                                        /*
                                        if ((id >= 0x180) && (id <= 0x4ff))
                                            Udl.PdoOnCanMessageReceived(id, dlc, data);
                                    else if ((id >= 0x580) && (id <= 0x5ff))
                                            Udl.SdoOnCanMessageReceived(id, dlc, data);
                                    else if ((id >= 0x700) && (id <= 0x7ff))
                                        Udl.HbOnCanMessageReceived(id, dlc, data);
                                    else if ((id >= 0x7c) && (id <= 0x7f))
                                        UdlProgrammer.OnCanMessageReceived(id, dlc, data);*/
                                    }
                                    else
                                    {
                                        if (LogEvent != null)
                                            LogEvent(this, "txThreadIdle ?dlc");
                                    }
                                    offset += 12 + dlc;
                                }
                            //  readMsgs++;
                            //if (bytes.Length > 0)
                            //  MsgIdle(bytes);
                            // msgs.Enqueue(bytes);
                        }

                    }
                    catch (Exception ex)
                    {
                        if (LogEvent != null)
                            LogEvent(this, "txThreadIdle " + ex.ToString());
                    }
                    System.Threading.Thread.Sleep(1);
                }
            }
        }
    }
}