using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

namespace qbook
{


    internal class UdpLogListener
    {
        public Thread listenThread = null;
        UdpClient udpClient = null;
        Regex log4NetSplitRegex = new Regex(@"(?<date>\d\d\d\d-\d\d-\d\d)\s+(?<time>\d\d:\d\d:\d\d[\.,]\d\d\d)\s+\[(?<thread>[^\]]+)\]\s+(?<type>[^\s]+)\s+(?<logger>[^\s:]+):?\s+(?<text>.*)", RegexOptions.Compiled | RegexOptions.Singleline);
        Regex log4NetTextStyleRegex = new Regex(@"(?<text>.*)({style=(?<style>[^}]*)}\s*)", RegexOptions.Compiled | RegexOptions.RightToLeft);
        public bool udpLoggerListenThreadIsRunning = false;
        public bool udpLoggerListenThreadFailed = false;

        public string UdpLoggerStatus = "n/a";

        internal bool StartListening(int port)
        {
            try
            {
                UdpLoggerStatus = "starting...";
                Thread listenThread = new Thread(new ParameterizedThreadStart(ListenThread));
                listenThread.IsBackground = true;
                listenThread.Start(port);
                return true;
            }
            catch (Exception ex)
            {
                UdpLoggerStatus = "#EX: " + ex.Message;
                return false;
            }
        }

        private void ListenThread(object paramObject) //int port) //object sender, DoWorkEventArgs e)
        {
            UdpLoggerStatus = "starting (thread)...";
            int port = 39999; //default port
            if ((paramObject is int))
                port = (int)(paramObject);

            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            //UdpClient udpClient;
            byte[] buffer;
            string log4NetEntry;

            try
            {
                using (udpClient = new UdpClient(port))
                {
                    UdpLoggerStatus = "listening on port " + port;
                    //udpClient = new UdpClient();
                    //udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    //udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));

                    udpLoggerListenThreadIsRunning = true;
                    while (true)
                    {
                        try
                        {
                            buffer = udpClient.Receive(ref remoteEndPoint);
                            log4NetEntry = System.Text.Encoding.UTF8.GetString(buffer).TrimEnd();
                            //split log-info according to log4net pattern, i.e.
                            Match m = log4NetSplitRegex.Match(log4NetEntry);
                            if (m.Success)
                            {
                                try
                                {
                                    DateTime origTimestamp = DateTime.ParseExact(m.Groups["date"].Value + "T" + m.Groups["time"].Value.Replace(',', '.')
                                        , "yyyy-MM-dd'T'HH:mm:ss.fff", null);
                                    string text = m.Groups["text"].Value;
                                    Match mStyle = log4NetTextStyleRegex.Match(text);
                                    if (mStyle.Success)
                                    {
                                        qbook.Core.AddLog(origTimestamp, m.Groups["type"].Value.ToString()[0], mStyle.Groups["text"].Value, mStyle.Groups["style"].Value.Trim(new char[] { '\'', '\"', ' ' }));
                                    }
                                    else
                                    {
                                        qbook.Core.AddLog(origTimestamp, m.Groups["type"].Value.ToString()[0], text);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    qbook.Core.AddLog(m.Groups["type"].Value.ToString()[0], m.Groups["text"].Value);
                                }
                            }
                            else
                            {
                                qbook.Core.AddLog('L', log4NetEntry);
                            }
                        }
                        catch (Exception ex1)
                        {
                            qbook.Core.AddLog('X', "#EX:" + ex1.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UdpLoggerStatus = "#EX: " + ex.Message;
                //Console.WriteLine(e.ToString());
                udpLoggerListenThreadFailed = true;
                qbook.Core.AddLog('X', "#EX:" + ex.Message);
            }
        }
    }
}
