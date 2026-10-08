using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;




namespace QB.Helpers
{

    //@STFU 2024-11-12 New
    public class SignalCsvLogger : Item
    {

        List<Signal> logList;

        ConcurrentQueue<string> lines = new ConcurrentQueue<string>();
        private DateTime start;
        private System.Threading.CancellationTokenSource cts;
        private Task loggingTask;
        private Task writingTask;

        private StreamWriter myWriter;

        public string Filename = "default";
        public string Seperator = ";";
        public string DecimalSeperator = ".";
        public string Folder = QB.Book.DataDirectory;

        TimeSpan totalSeconds;


        public int Interval = 1000;

        public bool Running = false;


        public SignalCsvLogger(string Name) : base(Name, "")
        {
            logList = new List<Signal>();
        }

        /// <summary>
        /// Interal default = 1000
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="filename"></param>
        public void Start(int interval = -1, string filename = "default")
        {
            init(filename);
            Running = true;
            if(interval > 0)
                Interval = interval;

            cts = new System.Threading.CancellationTokenSource();

            loggingTask = Task.Run(() => RunLogger(cts.Token));
            writingTask = Task.Run(() => WriteLogsToFile(cts.Token));
        }

        public async Task Stop()
        {
            Running = false;
            var errors = new List<Exception>();
            if (cts != null)
            {
                try { cts.Cancel(); }
                catch (Exception error) { errors.Add(error); }
            }
            foreach (var task in new[] { loggingTask, writingTask })
            {
                if (task == null) continue;
                try { await task.ConfigureAwait(false); }
                catch (OperationCanceledException) when (cts != null && cts.IsCancellationRequested) { }
                catch (Exception error) { errors.Add(error); }
            }
            loggingTask = writingTask = null;
            var cancellation = cts;
            cts = null;
            try { cancellation?.Dispose(); }
            catch (Exception error) { errors.Add(error); }
            if (errors.Count != 0) throw new AggregateException("The signal logger could not be stopped.", errors);
        }

        void init(string filename = "default")
        {
            Filename = filename;
            string write;
            if (Filename == "default")
                Filename = DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + "_" + Name + ".csv";

            List<string> line = new List<string>();

            myWriter = new StreamWriter(Folder + "\\" + Filename, append: true, encoding: Encoding.UTF8);
            foreach (object i in logList)
            {

                if (i is Signal)
                {
                    Signal sig = (Signal)i;
                    line.Add(sig.Name);
                }
            }
            write = string.Join(";", line);
            myWriter.WriteLine($"DateTime;Time rel.;Status;{write}");
            line.Clear();

            foreach (object i in logList)
            {

                if (i is Signal)
                {
                    Signal sig = (Signal)i;
                    line.Add(sig.Text);
                }

            }
            write = string.Join(";", line);
            myWriter.WriteLine($";;;{write}");
            line.Clear();
            foreach (object i in logList)
            {

                if (i is Signal)
                {
                    Signal sig = (Signal)i;
                    if (sig.Unit != null)
                        line.Add(sig.Unit);
                    else
                        line.Add("");
                }
            }
            write = string.Join(";", line);
            myWriter.WriteLine($";;;{write}");
            myWriter.Close();
            start = DateTime.Now;
        }


        public void OpenFolder()
        {
            System.Diagnostics.Process.Start("explorer.exe", Folder);
        }

        string read()
        {
            var stringBuilder = new StringBuilder();
            TimeSpan t = DateTime.Now - start;

            stringBuilder.Append(DateTime.Now.ToString("yyy-MM-dd HH:mm:ss.fff")).Append(Seperator);
            stringBuilder.Append(t.TotalSeconds.ToString("0.000")).Append(Seperator);


            foreach (var o in logList)
            {
                stringBuilder.Append(o.Value.ToString().Replace(".",DecimalSeperator)).Append(Seperator);
            }
            // Remove the last comma
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Length--;
            }
            return stringBuilder.ToString();
        }

        public void Add(Signal obj)
        {
            logList.Add(obj);
        }

        private void RunLogger(System.Threading.CancellationToken token)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (!token.IsCancellationRequested)
            {
                lines.Enqueue(read());

                // Wait for the next interval using Stopwatch
                while (!token.IsCancellationRequested && stopwatch.ElapsedMilliseconds < Interval)
                {
                    System.Threading.Thread.SpinWait(1); // Busy-wait to avoid sleep inaccuracy
                }
                stopwatch.Restart();
            }
        }

        private async Task WriteLogsToFile(System.Threading.CancellationToken token)
        {
            using (var writer = new StreamWriter(Folder + "\\" + Filename, append: true, encoding: Encoding.UTF8))
            {
                while (!token.IsCancellationRequested)
                {
                    while (lines.TryDequeue(out string result))
                        await writer.WriteLineAsync(result).ConfigureAwait(false);
                    await writer.FlushAsync().ConfigureAwait(false);
                    await Task.Delay(1000, token).ConfigureAwait(false);
                }
            }
        }

        public override void Destroy()
        {
            var errors = new List<Exception>();
            try { Stop().GetAwaiter().GetResult(); }
            catch (Exception error) { errors.Add(error); }
            try { myWriter?.Dispose(); }
            catch (Exception error) { errors.Add(error); }
            myWriter = null;
            logList.Clear();
            while (lines.TryDequeue(out _)) { }
            if (errors.Count != 0) throw new AggregateException("The signal logger could not be cleaned up.", errors);
        }



    }

}
