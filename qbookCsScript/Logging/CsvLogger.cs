using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace QB.Logging
{
    //@STFU 2024-11-12 New
    public class CsvLogger : Item
    {

        public List<LogObject> LogList = new List<LogObject>();
        public ConcurrentQueue<string> Lines = new ConcurrentQueue<string>();
        private DateTime start;
        private System.Threading.CancellationTokenSource cts;
        private Task loggingTask;
        private Task writingTask;

        private StreamWriter myWriter;

        public string Filename = "default";
        public string Seperator = ";";
        public string DecimalSeperator = ".";
        public string Folder = QB.Book.DataDirectory;

        public int Interval = 1000;
        public bool Running = false;

        public TimeSpan TimeRelative;

        public CsvLogger(string name) : base(name)
        {
            Name = name;

        }

        /// <summary>
        /// Interal default = 1000
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="filename"></param>
        public void Start(int interval = -1)
        {
   
            Running = true;
            if (interval > 0)
                Interval = interval;

            cts = new System.Threading.CancellationTokenSource();

            loggingTask = Task.Run(() => RunLogger(cts.Token));
            writingTask = Task.Run(() => WriteLogsToFile(cts.Token));
            start = DateTime.Now;
          

        }

        public void Start(string file, int interval = -1)
        {
            Init(file);
            Running = true;
            if (interval > 0)
                Interval = interval;

            cts = new System.Threading.CancellationTokenSource();

            loggingTask = Task.Run(() => RunLogger(cts.Token));
            writingTask = Task.Run(() => WriteLogsToFile(cts.Token));
            start = DateTime.Now;


        }

        public void Add(string name, string unit, string format, Func<object> value)
        {
            string type = "word";
            object result = value(); // Call the Func<object> to get the actual object

            if (result is double)
                type = "float";
            else if (result is Int16 || result is Int32 || result is Int64) // Fixed typo
                type = "int";
            else if (result is DateTime)
                type = "timestamp";

            LogList.Add(new LogObject(name, unit, format, value, type));
        }

      

        public void Reset()
        {
            LogList.Clear();
        }

        public async Task Stop()
        {
            if (myWriter == null) return;
            try
            {
                Running = false;

                if (cts == null) return;
                cts.Cancel();

                if (loggingTask != null)
                {
                    await loggingTask;
                }
                if (writingTask != null)
                {
                    await writingTask;
                }
            }
            catch
            {
                
                myWriter.Close();
            }
        }

        public void Init(string file = null)
        {
            if(file != null)
            {
                Folder = Path.GetDirectoryName(file);
                Filename = Path.GetFileName(file);
            }
            else
            {
                Filename = DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + "_" + Name + ".csv";
            }
           
            
            StreamWriter myWriter = new StreamWriter(Folder + "\\" + Filename, append: true, encoding: Encoding.UTF8);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var obj in LogList)
            {
               
                stringBuilder.Append(obj.Name).Append(Seperator);
               // Debug.WriteLine(obj.Name);
            }
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Length--;
            }

            myWriter.WriteLine(stringBuilder.ToString());
            stringBuilder = new StringBuilder();
            foreach (var obj in LogList)
            {
                stringBuilder.Append(obj.Unit).Append(Seperator);
            }
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Length--;
            }

            myWriter.WriteLine(stringBuilder.ToString());
            myWriter.Close();
        }
        public void OpenFolder()
        {
            System.Diagnostics.Process.Start("explorer.exe", Folder);
        }
        public virtual string getValues()
        {
            try
            {
                var stringBuilder = new StringBuilder();
                TimeSpan t = DateTime.Now - start;

                foreach (var item in LogList)
                {
                    object obj = item.GetLogObject;
                //    Debug.WriteLine(item.Name + " '" + item.CurrentValue + "' -> " + item.Object.GetType().Name);

                    string TypeName = item.Object.GetType().Name;
                    if (item.Object is Double)
                    {
                        double i = (double)item.CurrentValue;
                        stringBuilder.Append(i.ToString(item.Format).Replace(".",DecimalSeperator)).Append(Seperator);
                    }

                    if (item.Object is DateTime)
                    {
                        DateTime dt = (DateTime)item.CurrentValue;
                        stringBuilder.Append(dt.ToString(item.Format).Replace(".", DecimalSeperator)).Append(Seperator);

                    }

                    if (item.Object is Int16 || item.Object is Int32 || item.Object is Int64)
                        stringBuilder.Append(item.CurrentValue).Append(Seperator);
                    if (item.Object is string)
                        stringBuilder.Append(item.CurrentValue).Append(Seperator);
                }
                // Remove the last comma
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Length--;
                }
                return stringBuilder.ToString();
            }
            catch
            {
                return "Error";
            }

        }
        private void RunLogger(System.Threading.CancellationToken token)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (!token.IsCancellationRequested)
            {
                TimeRelative =  DateTime.Now - start;
                Lines.Enqueue(getValues());

                // Wait for the next interval using Stopwatch
                while (stopwatch.ElapsedMilliseconds < Interval)
                {
                    System.Threading.Thread.SpinWait(1); // Busy-wait to avoid sleep inaccuracy
                }
                stopwatch.Restart();
            }
        }

        private async Task WriteLogsToFile(System.Threading.CancellationToken token)
        {
            myWriter = new StreamWriter(Folder + "\\" + Filename, append: true, encoding: Encoding.UTF8);

            while (!token.IsCancellationRequested)
            {
                if (!Lines.IsEmpty)
                {
                    while (Lines.TryDequeue(out string result))
                    {
                        await myWriter.WriteLineAsync(result);
                    }
                    await myWriter.FlushAsync();
                }
                await Task.Delay(50, token); // Adjust delay for batch writing
            }
            myWriter.Close();
        }

        public override void Destroy()
        {
            if (cts != null)
                cts.Cancel();
            Running = false;

        }
    }

    public class LogObject
    {
        public string Name { get; set; }
        public string Unit { get; set; }
        public string Description { get; set; }
        public string Format { get; set; }
        public string ValueType { get; set; }

        public string SqlTable { get; set; }
        public Func<object> GetLogObject { get; set; } // Delegate for real-time value access

        public object Object;

        

        public LogObject(string name,string unit,string format, Func<object> value,string type, string sqlTable = "", string description = "")
        {
            GetLogObject = value;
            Name = name;
            Unit = unit;
            Format = format;
            ValueType = type;
            SqlTable = sqlTable;
            Description = description;
            Object = GetLogObject();
        }

        public object CurrentValue => GetLogObject();

    }
}
