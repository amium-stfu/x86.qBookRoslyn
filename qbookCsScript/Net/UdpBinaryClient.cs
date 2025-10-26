using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace QB.Net
{
    public class ValueDescriptor
    {
        public UInt16 Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public string Unit { get; set; }
        public UInt16 Interval { get; set; }

        public bool StepMode { get; set; }

        public ValueDescriptor()
        {

        }

        public ValueDescriptor(UInt16 id, string name, string text, string unit, UInt16 interval, bool stepMode)
        {
            Id = id;
            Name = name;
            Text = text;
            Unit = unit;
            Interval = interval;
            StepMode = stepMode;
        }
    }
    public class ValueDescriptorList
    {
        public UInt16 idCount = 0;

        public Dictionary<string, UInt16> ValueTypes = new Dictionary<string, UInt16>();
        public ValueDescriptorList()
        {
            ValueType = new List<ValueDescriptor>();
            CommandSpecifier = 1;
            DataCount = 1;
        }
        public UInt16 RollingCounter;
        public UInt16 CommandSpecifier;
        public UInt16 DataCount;
        public List<ValueDescriptor> ValueType { get; set; }

        public UInt16 Add(string name, string text, UInt16 interval, bool stepMode, string unit = "")
        {
            ValueTypes.Add(name, idCount);
            ValueType.Add(new ValueDescriptor(id: idCount, name: name, text: text, unit: unit, interval: interval, stepMode: stepMode));
            UInt16 id = idCount;
            idCount++;
            return id;
        }
    }
    public struct EpochValue
    {
        public UInt64 Epoch;
        public float Value;
        public UInt16 Id;
    }
    public struct DataPacket
    {
        public UInt16 RollingCounter;
        public UInt16 CommandSpecifier;
        public UInt16 DataCount;
        public EpochValue[] Values;
    }
    public struct MessageCounter
    {
        public UInt16 RollingCounter;
        public UInt16 CommandSpecifier;
        public UInt16 DataCount;
        public UInt16 Messages;
    }
    public static class BinaryPacketByteSeralizer
    {
        public static byte[] StructArrayToBytes<T>(T[] structArray) where T : struct
        {
            int totalSize = structArray.Length * Marshal.SizeOf<T>();
            byte[] byteArray = new byte[totalSize];
            MemoryMarshal.Cast<T, byte>(structArray).CopyTo(byteArray);
            return byteArray;
        }
        static byte[] StructToBytes<T>(T data) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] byteArray = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(data, ptr, false);
                Marshal.Copy(ptr, byteArray, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return byteArray;
        }

        // Converts byte[] back to struct
        public static T BytesToStruct<T>(byte[] data) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            if (data.Length < size)
                throw new ArgumentException($"Byte array too small for struct {typeof(T).Name}");

            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(data, 0, ptr, size);
                return Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public static byte[] SerializeDataPacket(DataPacket data)
        {
            byte[] d = StructArrayToBytes(data.Values);
            int size = 6 + d.Length;
            byte[] bytes = new byte[size]; // 4 (int) + 4 (float) + 2 (short)
            System.Buffer.BlockCopy(BitConverter.GetBytes(data.RollingCounter), 0, bytes, 0, 2);
            System.Buffer.BlockCopy(BitConverter.GetBytes(data.CommandSpecifier), 0, bytes, 2, 2);
            System.Buffer.BlockCopy(BitConverter.GetBytes(data.DataCount), 0, bytes, 4, 2);
            System.Buffer.BlockCopy(d, 0, bytes, 6, d.Length);
            return bytes;
        }

        public static byte[] SerializeDescriptorList(ValueDescriptorList data)
        {
            String jsonString = JsonSerializer.Serialize(data);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            //byte[] d = StructArrayToBytes(data.Values);
            int size = 6 + jsonBytes.Length;
            byte[] bytes = new byte[size]; // 4 (int) + 4 (float) + 2 (short)
            System.Buffer.BlockCopy(BitConverter.GetBytes(data.RollingCounter), 0, bytes, 0, 2);
            System.Buffer.BlockCopy(BitConverter.GetBytes(data.CommandSpecifier), 0, bytes, 2, 2);
            System.Buffer.BlockCopy(BitConverter.GetBytes(data.DataCount), 0, bytes, 4, 2);
            System.Buffer.BlockCopy(jsonBytes, 0, bytes, 6, jsonBytes.Length);
            return bytes;
        }
        public static byte[] SerializeMessageCounter(MessageCounter data)
        {

            byte[] d = StructToBytes(data);
            int size = d.Length;
            byte[] bytes = new byte[size]; // 4 (int) + 4 (float) + 2 (short)
            System.Buffer.BlockCopy(BitConverter.GetBytes(data.RollingCounter), 0, bytes, 0, 2);
            System.Buffer.BlockCopy(BitConverter.GetBytes(data.CommandSpecifier), 0, bytes, 2, 2);
            System.Buffer.BlockCopy(BitConverter.GetBytes(data.DataCount), 0, bytes, 4, 2);
            System.Buffer.BlockCopy(BitConverter.GetBytes(data.DataCount), 0, bytes, 6, 2);
            return bytes;
        }


        // Deserializes byte[] back into ValueDescriptorList
        public static ValueDescriptorList DeserializeDescriptorList(byte[] data)
        {
            int jsonSize = data.Length - 6;
            string jsonString = Encoding.UTF8.GetString(data, 6, jsonSize);
            return JsonSerializer.Deserialize<ValueDescriptorList>(jsonString);
        }
        public static DataPacket DeserializeDataPacket(byte[] data, out EpochValue[] values)
        {
            DataPacket packet = new DataPacket
            {
                RollingCounter = BitConverter.ToUInt16(data, 0),
                CommandSpecifier = BitConverter.ToUInt16(data, 2),
                DataCount = BitConverter.ToUInt16(data, 4),
            };

            // Anzahl der EpochValue-Einträge berechnen
            int structSize = Marshal.SizeOf<EpochValue>();
            int valueCount = (data.Length - 6) / structSize;

            values = new EpochValue[valueCount];

            // Werte extrahieren
            MemoryMarshal.Cast<byte, EpochValue>(data.AsSpan(6)).CopyTo(values);

            return packet;
        }

        // Deserializes byte[] back into MessageCounter
        public static MessageCounter DeserializeMessageCounter(byte[] data)
        {
            return BytesToStruct<MessageCounter>(data);
        }


        // Extension methods for easier conversion
        public static byte[] ToBytes(this DataPacket data) => SerializeDataPacket(data);
        public static byte[] ToBytes(this MessageCounter data) => SerializeMessageCounter(data);
        public static byte[] ToBytes(this ValueDescriptorList data) => SerializeDescriptorList(data);



    }
    internal class UdpBinaryClient
    {
        public string Name { get; }
        public int Port { get; set; }

        bool Send = false;

        bool descriptors = false;
        bool values = false;

        ValueDescriptorList Descriptors = new ValueDescriptorList();

        DataPacket valuePacket = new DataPacket() { CommandSpecifier = 2 };
        List<EpochValue> Values = new List<EpochValue>();

        UInt16 RollingCounter;

        public bool IsRunning { get; private set; } = false;
        private Thread idleThread;
        public UdpBinaryClient(string name)
        {
            Name = name;
          
            

            Descriptors.Add(name: "vdl", text: "ValueDescriptorList", unit: "", interval: 0, stepMode: false);
            Descriptors.Add(name: "dps", text: "DataPackets", unit: "", interval: 0, stepMode: false);
            Descriptors.idCount = 100; //CommandSpecifier 0..100 only for System
        }

        public UInt16 AddDescriptor(string name, string text, UInt16 interval, string unit = "", bool stepMode = false)
        {
            return Descriptors.Add(name: name, text: text, unit: unit, interval: interval, stepMode: stepMode);
        }
        public void AddValueToPacket(UInt16 id, string name, UInt64 epoch, float value)
        {
            //  Debug.WriteLine($"{id} {name} {epoch} {value}");
            Values.Add(new EpochValue() { Id = id, Epoch = epoch, Value = value });
        }
        public UInt16 Type(string name)
        {
            return Descriptors.ValueTypes[name];
        }

        public void Start()
        {
            Port = (int)FindAvailableUdpPort(startPort: 9100, endPort: 9200);

            if (Port == null)
            {
                QB.Logger.Error("UDP Client -> Find free port failed");
                return;
            }
         


            if (!IsRunning)
            {
                QB.Logger.Info($"[{Name}] Starting UDP sending on port {Port}...");
                IsRunning = true;
                idleThread = new Thread(Idle) { IsBackground = true };
                idleThread.Start();
            }
        }
        public void Stop()
        {
            Debug.WriteLine($"[{Name}] Stopping UDP sender...");
            IsRunning = false;
        }
        public void SendDiscriptors()
        {
            descriptors = true;
            Send = true;

        }

        public void SendValues()
        {
            //descriptors = false;
            values = true;
            Send = true;
        }

        void Idle()
        {
            using (UdpClient udpClient = new UdpClient())
            {

                udpClient.EnableBroadcast = true;
                IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, Port);
                try
                {
                    while (IsRunning)
                    {
                        Thread.Sleep(1);
                        if (Send)
                        {
                            byte[] data = null;
                            RollingCounter = 0;

                            if (descriptors)
                            {
                                descriptors = false;
                                RollingCounter++;
                                Descriptors.RollingCounter = RollingCounter;
                                data = Descriptors.ToBytes();
                                udpClient.Send(data, data.Length, broadcastEndPoint);
                            }

                            if (values)
                            {
                                values = false;
                                valuePacket.Values = Values.ToArray();
                                valuePacket.DataCount = (UInt16)Values.Count;
                                data = valuePacket.ToBytes();
                                udpClient.Send(data, data.Length, broadcastEndPoint);
                                Values.Clear();
                            }

                            Send = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[{Name}] Error sending UDP message: {ex.Message}");
                }
            }
        }
        public static int? FindAvailableUdpPort(int startPort, int endPort)
        {
            for (int port = startPort; port <= endPort; port++)
            {
                try
                {
                    using (UdpClient udpClient = new UdpClient(port))
                    {
                        // Port ist verfügbar
                        return port;
                    }
                }
                catch (SocketException)
                {
                    // Port ist belegt, weiter prüfen
                    continue;
                }
            }

            // Kein freier Port gefunden
            return null;
        }
        public static bool IsUdpPortAvailable(int port)
        {
            try
            {
                using (UdpClient udpClient = new UdpClient(port))
                {

                    return true;
                }
            }
            catch (SocketException)
            {
                return false;
            }
        }

    }


    public class BinaryStream
    {
        public UInt16 Id { get; set; }
        public string Name { get; set; }
        public int Interval { get; set; }
        public List<UInt64> Timestamps { get; set; }
        public List<float> Values { get; set; }
        Func<float> Source { get; set; }

        Stopwatch stopwatch = new Stopwatch();

        private float? newValue;
        public float? NewValue
        {
            get
            {
                var temp = newValue;
                newValue = null;
                return temp;
            }
            set
            {
                newValue = value;
            }
        }

        public BinaryStream(UInt16 id, string name, int interval, Func<float> source)
        {
            Id = id;
            Name = name;
            Interval = interval;
            Timestamps = new List<UInt64>();
            Values = new List<float>();
            Source = source;
        }

        public void SavePointOnInterval()
        {
            if (stopwatch.ElapsedMilliseconds < Interval) return;

            Timestamps.Add((UInt64)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            Values.Add(Source());
            stopwatch.Restart();
            //  Debug.WriteLine(Name + "Interval: " + Interval + " -> " + DateTime.Now.ToString());
        }

        public void ValueOnInterval()
        {
            if (stopwatch.ElapsedMilliseconds < Interval) return;

            NewValue = (float)Source();
            stopwatch.Restart();
            //  Debug.WriteLine(Name + "Interval: " + Interval + " -> " + DateTime.Now.ToString());
        }
        public void Start()
        {
            // Debug.WriteLine(Name + "Interval: " + Interval + " -> " + DateTime.Now.ToString());
            Timestamps.Add((UInt64)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            Values.Add(Source());
            stopwatch.Start();
        }
    }
    internal class StreamerWriterUDP : UdpBinaryClient
    {

        bool isRunning = false;
        Thread RecordThread;
        Thread SendThread;

        int ValueInterval;
        int DescriptorInterval;

        DataPacket dataPacket = new DataPacket();

        public Dictionary<string, BinaryStream> Streams = new Dictionary<string, BinaryStream>();
        public StreamerWriterUDP(string name, bool sendDirect, int valueInterval, int descriptorInterval) : base(name)
        {
            ValueInterval = valueInterval;
            DescriptorInterval = descriptorInterval;
        }
        public void AddStream(string name, UInt16 interval, Func<float> source, string text, string unit)
        {
            UInt16 id = AddDescriptor(name: name, text: text, unit: unit, interval: interval);
            Streams.Add(name, new BinaryStream(id: id, name, interval, source));
        }

        public void Add(Signal signal, UInt16 interval = 100)
        {

            if (Streams.ContainsKey(signal.Name)) {
                QB.Logger.Error("Stream " + signal.Name + " already exists in dictionary");
                return;
                    };
            Func<float> source = () => (float)signal.Value;
            UInt16 id = AddDescriptor(name: signal.Name, text: signal.Text, unit: signal.Unit, interval: interval);

            Streams.Add(signal.Name, new BinaryStream(id: id, signal.Name, interval, source));
            QB.Logger.Info($"[{this.GetType().Name}] {Name}: " + "Stream add -> " + signal.Name);
        }

        public void StartSteam()
        {
            QB.Logger.Info($"[{this.GetType().Name}] {Name}: " + "UDP -> Start Streaming");
            Start();
            QB.Logger.Info($"[{this.GetType().Name}] {Name}: " + "UDP -> SendDiscriptors");
            SendDiscriptors();

            isRunning = true;
            foreach (var stream in Streams.Values)
                stream.Start();

            RecordThread = new Thread(RecordIdle);
            RecordThread.IsBackground = true;
            RecordThread.Start();

            SendThread = new Thread(SendIdle);
            SendThread.IsBackground = true;
            SendThread.Start();
            QB.Logger.Info($"[{this.GetType().Name}] {Name}: " + "UDP -> Start Streaming");

        }
        public void StopStream()
        {
            Stop();
            isRunning = false;

        }
        void RecordIdle()
        {
            QB.Logger.Info($"[{this.GetType().Name}] {Name}: " + "UDP -> Start Recording");
            QB.Logger.Info($"[{this.GetType().Name}] {Name}: " + "UDP -> Signals " + Streams.Count);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (isRunning)
            {             
                 UInt64 epoch = (UInt64)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                foreach (var stream in Streams.Values)
                {
                    stream.ValueOnInterval();
                    var value = stream.NewValue;
                    if (value != null) AddValueToPacket(id: Type(stream.Name), name: stream.Name, epoch: epoch, value: (float)value);
                }
                Thread.SpinWait(1);
            }
        }

        void SendIdle()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DateTime start = DateTime.Now;

            while (isRunning)
            {
                if ((DateTime.Now - start).TotalMilliseconds > DescriptorInterval)
                {
                    SendDiscriptors();
                    start = DateTime.Now;
                }

                while (stopwatch.ElapsedMilliseconds < ValueInterval)
                {
                    Thread.Sleep(1);
                }
                stopwatch.Restart();
                SendValues();

            }
        }
    }


}
