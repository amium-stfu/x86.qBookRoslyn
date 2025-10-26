//using CefSharp.DevTools.DOM;
using QB.Automation;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QB.Net
{
    public partial class Ak
    {

        public class Server : Machine
        {
            public class AkServerMessageReceivedEventArgs : EventArgs
            {
                public char Dcb = ' ';
                public int Port = 0;
                public string Client = "";
                public string Command = "????";
                public string Channel = "K0";
                public string[] Parameters = null;
            }

            //   static List<Server> servers = new List<Server>();

            public delegate string OnMessageReceivedDelegate(Server aks, AkServerMessageReceivedEventArgs ea);// int port =0, string client="", char dcb=' ', string command="????", string channel="K0", string[] parameters = null);
            public OnMessageReceivedDelegate OnMessageReceived;


            static Dictionary<int, AkListener> listener = new Dictionary<int, AkListener>();


            public Server(string name, int port = 0, List<int> ports = null) : base(name)
            {


                foreach (AkListener list in listener.Values)
                    list.OnMessageReceived -= Port_MessageReceived;

                if (port != 0)
                {
                    if (!listener.ContainsKey(port))
                        listener.Add(port, new AkListener(port, 10000));
                    Destroy();


                    listener[port].OnMessageReceived += Port_MessageReceived;
                }
                else if (ports != null)
                {
                    foreach (int p in ports)
                    {
                        if (!listener.ContainsKey(p))
                            listener.Add(p, new AkListener(p, 10000));
                    }
                    Destroy();

                    foreach (int p in ports)
                        listener[p].OnMessageReceived += Port_MessageReceived;

                }
                else return;



            }

            /*
            public Server(string name, List<int> ports = null) : base(name)
            {
                foreach (AkListener list in listener.Values)
                    list.OnMessageReceived -= Port_MessageReceived;

                if (ports == null)
                    return;
                foreach (int port in ports)
                {
                    if (!listener.ContainsKey(port))
                        listener.Add(port, new AkListener(port, 10000));
                }

                Destroy();

                foreach (int port in ports)
                    listener[port].OnMessageReceived += Port_MessageReceived;
            }
            */

            public override void Destroy()
            {
                base.Destroy();

                this.OnMessageReceived = null;  //removes all existing subscribers

                foreach (AkListener list in listener.Values)
                {
                    list.OnMessageReceived -= Port_MessageReceived;

                    //@SCAN 2023-04-20 add
                    list.Destroy();

                }
            }

            public char Dcb = ' ';
            public char Fsb = '0';
            public string Status = "SPAU";
            public string Access = "SREM";

            private void Port_MessageReceived(AkListener s, int port, string client, string message)
            {
                lock (this)
                {
                    if ((message.Length < 8) || (message[5] != ' '))
                    {
                        s.Transmit("*???? 0");
                        return;
                    }

                    try
                    {
                        char dcb = message.Substring(0, 1)[0];
                        string command = message.Substring(1, 4);
                        message = message.Substring(6);
                        string channel = message.Split()[0];
                        string parameter = "";
                        if (message.Split().Length > 1)
                            parameter = message.Substring(message.IndexOf(' ') + 1);

                        if (OnMessageReceived != null)
                        {
                            AkServerMessageReceivedEventArgs ea = new AkServerMessageReceivedEventArgs();
                            ea.Port = port;
                            ea.Client = client;
                            ea.Dcb = dcb;
                            ea.Command = command;
                            ea.Channel = channel;
                            ea.Parameters = parameter.Trim().Split();
                            string response = OnMessageReceived(this, ea);// port, client, dcb, command, channel, parameter.Trim().Split());
                            if (response != null)
                            {
                                string responseString = response.ToString();
                                s.Transmit(Dcb + command + " " + Fsb + (responseString.Length > 0 ? " " + responseString : ""));
                            }
                            else
                                s.Transmit("!???? 0");
                        }
                        else
                            s.Transmit("!???? 0");
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }




        class AkListener
        {
            public delegate void OnMessageReceivedDelegate(AkListener sender, int port, string client, string message);
            public OnMessageReceivedDelegate OnMessageReceived;

            private TcpListener TcpListener = null;
            private Socket TcpSocket;
            public string Address;
            int Port;
            DateTime Timeout = DateTime.MaxValue;
            int timeout = 10000;
            Thread IdleThread = null;
            public AkListener(int port, int timeout)
            {
                Port = port;
                this.timeout = timeout;
                IdleThread = new Thread(Idle);
                IdleThread.IsBackground = true;
                IdleThread.Start();
            }


            //@SCAN 2023-04-20 add
            public void Destroy()
            {
                //     if (IdleThread!= null) { IdleThread.Abort();}
                Disconnect();
                OnMessageReceived = null;
                //      if (TcpListener != null)
                //         TcpListener.Stop();
            }

            public void Disconnect()
            {
                Address = "";
                Timeout = DateTime.MaxValue;
                if (TcpSocket != null)
                {
                    try
                    {
                        //  TcpSocket.Disconnect(true);
                        TcpSocket.Close();
                    }
                    catch (Exception ex)
                    {
                        string x = ex.Message;
                    }
                }
                TcpSocket = null;
            }
            public bool Connected
            {
                get
                {
                    if (TcpSocket == null) return false;
                    if (!TcpSocket.Connected) return false;
                    return true;
                }
            }

            public List<byte> resp = new List<byte>();
            protected void Idle()
            {
                while (true)
                {
                    try
                    {
                        if (!Connected)
                        {
                            try
                            {
                                Timeout = DateTime.MaxValue;
                                if (TcpListener != null)
                                    TcpListener.Stop();
                                if (TcpListener == null)
                                    TcpListener = new TcpListener(IPAddress.Any, Port);
                                TcpListener.Start();

                                TcpSocket = TcpListener.AcceptSocket();

                                Address = ((IPEndPoint)TcpSocket.RemoteEndPoint).Address.ToString();
                                Timeout = DateTime.Now.AddMilliseconds(timeout);
                            }
                            catch (Exception ex)
                            {

                                string x = ex.Message;
                                Disconnect();
                                Thread.Sleep(5000);
                            }
                        }
                        else
                        {
                            if ((TcpSocket != null) && (TcpSocket.Available > 0))
                            {
                                Byte[] receiveBytes = new Byte[TcpSocket.ReceiveBufferSize];
                                int readBytes = TcpSocket.Receive(receiveBytes);
                                for (int i = 0; i < readBytes; i++)
                                {
                                    if (receiveBytes[i] == 0x02)
                                    {
                                        resp.Clear();
                                    }
                                    else if (receiveBytes[i] == 0x03)
                                    {
                                        Timeout = DateTime.Now.AddMilliseconds(timeout);
                                        //TimeoutTimer.Stop();
                                        //TimeoutTimer.Start();
                                        if (OnMessageReceived != null)
                                            OnMessageReceived(this, Port, Address, System.Text.Encoding.Default.GetString(resp.ToArray()));
                                    }
                                    else resp.Add(receiveBytes[i]);
                                }
                            }

                            if (DateTime.Now > Timeout)
                            {
                                Disconnect();
                                Thread.Sleep(1000);
                            }
                        }
                        Thread.Sleep(5);
                    }
                    catch (Exception ex)
                    {

                        string x = ex.Message;
                        Disconnect();
                        Thread.Sleep(5000);
                    }
                }
            }

            public bool Transmit(string text)
            {
                try
                {
                    if (!Connected)
                        return false;
                    byte[] mes = Encoding.UTF8.GetBytes(text);
                    byte[] bytes = new byte[mes.Length + 2];
                    bytes[0] = 0x02;
                    for (int nCnt = 1; nCnt < mes.Length + 1; nCnt++)
                        bytes[nCnt] = Convert.ToByte(mes[nCnt - 1]);
                    bytes[bytes.Length - 1] = 0x03;
                    TcpSocket.Send(bytes);
                }
                catch
                {
                    Disconnect();
                    return false;
                }
                return true;
            }
        }

    }

}