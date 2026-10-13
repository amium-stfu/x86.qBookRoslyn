//using CefSharp.DevTools.DOM;
using QB.Automation;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace QB.Net
{
    public partial class Ak
    {
        /*
        class Query
        {
            public Query()
            { }
            public string command = "";
            public string response = "";
            public string name = "";
            public DateTime lastQuery = DateTime.Now;
            public double repetionTime = 0;
        }*/

        public class Client : Machine
        {
            static List<Client> AkClients = new List<Client>();

            internal static void ResetClients()
            {
                foreach (var c in AkClients)
                    c.Destroy();//SCAN //TODO
                AkClients.Clear();
            }

            public Client(string name, string uri = "tcp://127.0.0.1:7700") : base(name)
            {
                Uri = uri;
                AkClients.Add(this);
                Connect(uri.Split('/')[uri.Split('/').Length - 1]);

            }

            public string Uri;

            System.Threading.Thread idleThread;
            private CancellationTokenSource idleCancellation = new CancellationTokenSource();
            private bool idleCancellationDisposed;

            private void WaitIdle(int milliseconds)
            {
                if (idleCancellation.Token.WaitHandle.WaitOne(milliseconds))
                    throw new OperationCanceledException(idleCancellation.Token);
            }

            private void StopIdle()
            {
                if (!idleCancellationDisposed) idleCancellation.Cancel();

                var worker = idleThread;
                if (worker == Thread.CurrentThread) return;
                if (worker != null && worker.IsAlive && !worker.Join(2000))
                    throw new InvalidOperationException("The previous network worker has not stopped cooperatively.");
                idleThread = null;
                if (!idleCancellationDisposed) idleCancellation.Dispose();
                idleCancellationDisposed = true;
            }

            public string socket;
            _AkClient akClient;
            /*
            List<Query> akQueries = new List<Query>();

            public void Add(string name, string command, double time)
            {
                Query query = new Query();
                query.name = name;
                query.command = command;
                query.repetionTime = time;
                akQueries.Add(query);
            }
            */
            public override void Destroy()
            {
                base.Destroy();
                //    return;
                if (akClient != null)
                    akClient.Abort();
                if (idleThread != null)
                    StopIdle();
                akClient = null;
                //akQueries.Clear();
            }

            public void Connect(string socket)
            {
                StopIdle();
                if (idleThread == Thread.CurrentThread)
                    throw new InvalidOperationException("A network worker cannot replace itself.");
                akClient?.Abort();
                idleCancellation = new CancellationTokenSource();
                idleCancellationDisposed = false;
                this.socket = socket;
                akClient = new _AkClient(Name, socket);

                idleThread = new System.Threading.Thread(() =>
                {
                    try { Idle(); }
                    catch (OperationCanceledException) when (idleCancellation.IsCancellationRequested) { }
                    catch (Exception) when (idleCancellation.IsCancellationRequested) { }
                });
                idleThread.IsBackground = true;
                idleThread.Start();
            }

            /*
            public string Transceive(string command)//, ref int duration)
            {
                if (command.StartsWith("\""))
                    command = command.Trim('"');
                int duration = 500;
                lock (akClient)
                {
                    return akClient.Transceive(command, ref duration);
                }
            }*/

            // public string astz = "";
            public QB.Action onreceived = new QB.Action();

            public char Fsb = '?';
            public string Status = "????";
            public string Access = "????";


            public string Transceive(string command)
            {
                int duration = 1000;
                return Transceive(command, ref duration);
            }

            public string Transceive(string command, ref int duration)
            {
                lock (this)
                {
                    //   bool lockTaken = false;
                    string response = null;// "#";
                    try
                    {
                        //     System.Threading.Monitor.TryEnter(this, timeout, ref lockTaken);
                        //    if (lockTaken)
                        {
                            DateTime start = DateTime.Now;
                            int dur = 500;
                            if (akClient != null)
                                response = akClient.Transceive(command, ref dur);
                            duration = (int)(DateTime.Now - start).TotalMilliseconds;
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

                    if (response == null)
                        response = "#";

                    return response;
                }

            }


            private void Idle()
            {
                while (!idleCancellation.IsCancellationRequested)
                {
                    /*
                    foreach (Query query in akQueries)
                    {
                        if (DateTime.Now > query.lastQuery.AddSeconds(query.repetionTime))
                        {
                            int dur = 500;
                            string response = akClient.Transceive(query.command, ref dur);
                            if (response != null)
                            {

                                string r = response;

                                try
                                {
                                    string code = Directory + "\\" + onreceived.code.Replace('\\', '+');

                                    char dcb = r.Substring(0, 1)[0];
                                    string cmd = r.Substring(1, 4);
                                    r = r.Substring(6);
                                    string fsb_ = r.Split()[0];
                                    string param = "";
                                    if (r.Split().Length > 1)
                                        param = r.Substring(r.IndexOf(' ') + 1);

                                    string scriptP =
                                        "(\"" + dcb + "\"" + "," +
                                        "\"" + cmd + "\"" + "," +
                                        "\"" + query.command.Substring(6).Split()[0] + "\"" + "," +
                                        "\"" + param + "\")";

                                    string script = code.Replace("()", scriptP);
                                    //MIGRATE Qb.ScriptingEngine.InterpretScript(script);
                                }
                                catch (Exception ex)
                                {
                                }


                                if (response.Length >= 7)
                                    Fsb = response.Substring(6, 1);

                                if (response.Length >= 9)
                                    response = response.Substring(8);
                                else if (response.Length == 7)
                                    response = "";
                                else
                                    response = "!";


                                if (query.command == " ASTZ K0")
                                {
                                    string[] responseSplit = response.Split();
                                    if (responseSplit.Length >= 3)
                                    {
                                        if (responseSplit[0].StartsWith("K"))
                                        {
                                            Access = responseSplit[1];
                                            Status = responseSplit[2];
                                        }
                                        else
                                        {
                                            Access = responseSplit[0];
                                            Status = responseSplit[1];
                                        }
                                    }
                                    else
                                    {
                                        Access = responseSplit[0];
                                        Status = responseSplit[1];
                                    }
                                }
                                //MIGRATE Qb.ScriptingEngine.InterpretScript(directory + "\\" + name + "." + query.name + "=\"" + response + "\"");
                            }

                            query.lastQuery = DateTime.Now;
                            //  qbObjectHelper.GetObject(directory + "\\" + query.name, "oString()");
                        }
                    }
                    */
                    // astz = transfer(" ASTZ K0");
                    WaitIdle(100);
                }
            }

        }


        class _AkClient
        {
            string Text;
            // string Name;
            SerialPort serialPort = null;
            bool tcp = false;
            public string uri = "127.0.0.1:9001";

            #region SignalHive: Semi-Automatic mamangement of Modules & Signals
            //Helpers.SignalHive shive = new Helpers.SignalHive();
            private Dictionary<string, Module> ModuleDict = new Dictionary<string, Module>();
            private Dictionary<string, Module> SignalDict = new Dictionary<string, Module>();
            private Dictionary<string, string> AliasDict = new Dictionary<string, string>();
            public Module Module(string name)
            {
                if (ModuleDict.ContainsKey(name))
                    return ModuleDict[name];
                else if (AliasDict.ContainsKey(name) && ModuleDict.ContainsKey(AliasDict[name]))
                    return ModuleDict[AliasDict[name]];
                else
                    return null;
            }
            //public UdlModule UdlModule(string key)
            //{
            //    if (ModuleDict.ContainsKey(key))
            //        return ModuleDict[key] as UdlModule;
            //    else if (AliasDict.ContainsKey(key) && ModuleDict.ContainsKey(AliasDict[key]))
            //        return ModuleDict[AliasDict[key]] as UdlModule;
            //    else
            //        return null;
            //}
            public void AddModule(Module m)
            {
                AddModule(m.Name, m);
            }
            public void AddModule(string name)
            {
                if (!ModuleDict.ContainsKey(name))
                {
                    Module m = new Module(name);
                    ModuleDict.Add(name, m);
                }
            }
            public void AddModule(string name, Module m)
            {
                if (!ModuleDict.ContainsKey(name))
                    ModuleDict.Add(name, m);
                else
                    ModuleDict[name] = m;
            }
            public void AddAlias(string alias, string key)
            {
                if (!AliasDict.ContainsKey(alias))
                    AliasDict.Add(alias, key);
                else
                    AliasDict[alias] = key;
            }
            #endregion



            static List<_AkClient> AkClients = new List<_AkClient>();

            internal static void ResetClients()
            {
                foreach (var c in AkClients)
                    c.Close();
                AkClients.Clear();
            }

            public _AkClient(string text, string socket)
            {
                AkClients.Add(this);
                Text = text;
                Socket = socket;// settings.GetItemValue("Socket", "").Replace("'","");
                Status = "init";

                if (Socket.ToUpper().Contains("COM") && (Socket.Split(' ').Length == 3))
                {
                    if (Socket.Split(' ')[2].ToUpper().Trim() == "8N1")

                        serialPort = new SerialPort(Socket.Split(' ')[0], Socket.Split(' ')[1].ToInt(), Parity.None, 8, StopBits.One);
                }
                else if (Socket.ToUpper().Contains("COM") && (Socket.Split(' ').Length == 1))
                {
                    serialPort = new SerialPort(Socket.Split(' ')[0], 9600, Parity.None, 8, StopBits.One);
                }
                else
                {
                    //  IpEndPoint = new IPEndPoint(IPAddress.Parse(Socket.Split(':')[0]), int.Parse(Socket.Split(':')[1]));
                    tcp = true;

                    tcpClient = new TcpClient();
                }


                idleThread = new System.Threading.Thread(() =>
                {
                    try { Idle(); }
                    catch (OperationCanceledException) when (idleCancellation.IsCancellationRequested) { }
                    catch (Exception) when (idleCancellation.IsCancellationRequested) { }
                });
                idleThread.IsBackground = true;
                idleThread.Start();
            }

            public void Close()
            {
                StopIdle();
            }

            public string Socket;


            static TimeSpan timeout = TimeSpan.FromMilliseconds(500);



            public delegate void LogEventHandler(object sender, string message);
            public event LogEventHandler LogEvent;

            private TcpClient tcpClient = null;
            private Thread idleThread;
            private CancellationTokenSource idleCancellation = new CancellationTokenSource();
            private bool idleCancellationDisposed;

            private void WaitIdle(int milliseconds)
            {
                if (idleCancellation.Token.WaitHandle.WaitOne(milliseconds))
                    throw new OperationCanceledException(idleCancellation.Token);
            }

            private void StopIdle()
            {
                if (!idleCancellationDisposed) idleCancellation.Cancel();
                try { tcpClient?.Close(); } catch (ObjectDisposedException) { }
                try { serialPort?.Close(); } catch (ObjectDisposedException) { }
                var worker = idleThread;
                if (worker == Thread.CurrentThread)
                    throw new InvalidOperationException("A network callback cannot synchronously stop or replace its own transport.");
                if (worker != null && worker.IsAlive && !worker.Join(2000))
                    throw new InvalidOperationException("The previous network worker has not stopped cooperatively.");
                tcpClient?.Close();
                serialPort?.Dispose();
                idleThread = null;
                if (!idleCancellationDisposed) idleCancellation.Dispose();
                idleCancellationDisposed = true;
            }

            IPEndPoint IpEndPoint;
            public string Status { get; set; }

            void Log(object sender, string message)
            {
                //log.Info(message);
                if (LogEvent != null)
                    LogEvent(sender, message);
            }

            public void Abort() => Close();

            bool _isDisconnecting = false;
            public void Disconnect(string message)
            {
                if (idleCancellation.IsCancellationRequested) return;
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
                    catch (OperationCanceledException) when (idleCancellation.IsCancellationRequested) { throw; }
                    catch
                    {
                        if (idleCancellation.IsCancellationRequested) throw new OperationCanceledException();
                        exceptionCounter++;
                    }

                    /*
                    try
                    {
                        string msg = " SREM K0";
                        sendBuffer[0] = 0x02;
                        int byteLength = message.Length;
                        for (int i = 1; i < byteLength + 1; i++)
                            sendBuffer[i] = Convert.ToByte(message[i - 1]);
                        byteLength += 2;

                        sendBuffer[byteLength - 1] = 0x03;
                        checkTries = 1; 
                        TcpClient.Client.Send(sendBuffer, byteLength, SocketFlags.None);
                        Thread.Sleep(500);
                        checkTries++;
                        TcpClient.Client.Send(sendBuffer, byteLength, SocketFlags.None);
                        Thread.Sleep(500);
                        checkTries++;
                        TcpClient.Client.Send(sendBuffer, byteLength, SocketFlags.None);
                        Thread.Sleep(500);
                    }
                    catch
                    {
                  //      exceptionCounter++;
                        _connected = false;
                        Log(this, Name + " disconnected ok " + checkTries);
                    }
                    */

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
                    catch (OperationCanceledException) when (idleCancellation.IsCancellationRequested) { throw; }
                    catch
                    {
                        if (idleCancellation.IsCancellationRequested) throw new OperationCanceledException();
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
                while (!idleCancellation.IsCancellationRequested)
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


                                    using SocketAsyncEventArgs m_socketConnectionArgs = new SocketAsyncEventArgs();
                                    m_socketConnectionArgs.RemoteEndPoint = IpEndPoint;
                                    m_socketConnectionArgs.Completed += M_socketConnectionArgs_Completed;
                                    tcpClient.ConnectAsync(Socket.Split(':')[0], int.Parse(Socket.Split(':')[1]), idleCancellation.Token)
                                        .AsTask().WaitAsync(TimeSpan.FromSeconds(5), idleCancellation.Token).GetAwaiter().GetResult();
                                    DateTime dateTime = DateTime.Now.AddSeconds(5);
                                    while (!idleCancellation.IsCancellationRequested && !tcpClient.Connected)
                                    {
                                        WaitIdle(500);
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
                                catch (OperationCanceledException) when (idleCancellation.IsCancellationRequested) { throw; }
                                catch (Exception ex)
                                {
                                    if (idleCancellation.IsCancellationRequested) throw new OperationCanceledException();
                                    //    log.Error($"{Name}/{IpEndPoint.Address}: {IpEndPoint.Port} retry ({ex.Message})");
                                    Log(this, Text + "/" + IpEndPoint.Address + ":" + IpEndPoint.Port + " retry " + ex.Message);
                                    WaitIdle(4000);
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
                    }
                    catch (OperationCanceledException) when (idleCancellation.IsCancellationRequested) { throw; }
                    catch
                    {
                        if (idleCancellation.IsCancellationRequested) throw new OperationCanceledException();
                        exceptionCounter++;
                        Disconnect("!!!! c1");
                    }

                    WaitIdle(1000);

                    if (tcpClient != null)
                    {
                        try
                        {
                            if (connectionEstablished && (tcpClient.Client != null) && tcpClient.Client.Connected && !_connected)
                            {
                                _connected = true;
                                Status = "ok " + disconnectCounter + " !(" + errorcounter + "/" + exceptionCounter + ")";

                            }
                        }
                        catch (OperationCanceledException) when (idleCancellation.IsCancellationRequested) { throw; }
                        catch (Exception ex)
                        {
                            if (idleCancellation.IsCancellationRequested) throw new OperationCanceledException();
                            exceptionCounter++;
                            Disconnect("!!!! c2 " + ex.ToString());
                            WaitIdle(2000);
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
                        catch (OperationCanceledException) when (idleCancellation.IsCancellationRequested) { throw; }
                        catch (Exception ex)
                        {
                            if (idleCancellation.IsCancellationRequested) throw new OperationCanceledException();
                            exceptionCounter++;
                            Disconnect("!!!! c3 " + ex.ToString());
                            WaitIdle(2000);
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
            public string Transceive(string message, ref int duration)
            {
                if (message == null)
                    return null;


                if (tcpClient != null)
                {
                    if (!Connected)
                    {
                        //  log.Warn($"{Name}/{IpEndPoint.Address}: {IpEndPoint.Port} not connected/not sent " + message);
                        //**       Log(this, $"{Name}/{IpEndPoint.Address}: {IpEndPoint.Port} not connected/not sent " + message);
                        return null;
                    }

                    try
                    {
                        while (!idleCancellation.IsCancellationRequested && tcpClient.Client.Available > 0)
                            tcpClient.Client.Receive(receiveBytes, 4096, SocketFlags.None);
                        receivBufferIndex = 0;
                    }
                    catch (OperationCanceledException) when (idleCancellation.IsCancellationRequested) { throw; }
                    catch
                    {
                        if (idleCancellation.IsCancellationRequested) throw new OperationCanceledException();
                        exceptionCounter++;
                        Disconnect("$dummy");
                        return null;
                    }
                }


                if (serialPort != null)
                    if (serialPort.IsOpen)
                        serialPort.ReadExisting();

                try
                {
                    sendBuffer[0] = 0x02;
                    int byteLength = message.Length;
                    for (int i = 1; i < byteLength + 1; i++)
                        sendBuffer[i] = Convert.ToByte(message[i - 1]);
                    byteLength += 2;
                    sendBuffer[byteLength - 1] = 0x03;

                    if (tcpClient != null)
                        tcpClient.Client.Send(sendBuffer, byteLength, SocketFlags.None);


                    if (serialPort != null)
                        if (serialPort.IsOpen)
                            serialPort.Write(sendBuffer, 0, byteLength);

                    TxMsgCount++;
                }
                catch (OperationCanceledException) when (idleCancellation.IsCancellationRequested) { throw; }
                catch
                {
                    if (idleCancellation.IsCancellationRequested) throw new OperationCanceledException();
                    exceptionCounter++;
                    Disconnect("send");
                    return null;
                }





                int counter = duration / 10;
                try
                {
                    while (!idleCancellation.IsCancellationRequested && counter > 0)
                    {
                        if ((tcpClient != null) && (tcpClient.Client != null))
                        {
                            while (!idleCancellation.IsCancellationRequested && tcpClient.Client.Available > 0)
                            {
                                int readBytes = tcpClient.Client.Receive(receiveBytes, 4096, SocketFlags.None);
                                if (readBytes > 0)
                                {
                                    for (int i = 0; i < readBytes; i++)
                                    {
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
                                    }
                                }
                            }
                        }

                        if (serialPort != null)
                        {
                            while (!idleCancellation.IsCancellationRequested && serialPort.BytesToRead > 0)
                            {
                                int readBytes = serialPort.Read(receiveBytes, 0, serialPort.BytesToRead);
                                if (readBytes > 0)
                                {
                                    for (int i = 0; i < readBytes; i++)
                                    {
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
                                    }
                                }
                            }
                        }



                        WaitIdle(7); // -> ca. 10ms
                        counter--;
                    }
                    if (duration > 0)
                    {
                        errorcounter++;
                        if (errorcounter > 2)
                        {
                            Disconnect("retries");
                        }
                    }
                }
                catch (OperationCanceledException) when (idleCancellation.IsCancellationRequested) { throw; }
                catch
                {
                    if (idleCancellation.IsCancellationRequested) throw new OperationCanceledException();
                    exceptionCounter++;
                    Disconnect("receive");
                }
                return null;
            }
        }
    }
}