using ActiproSoftware.UI.WinForms.Controls.Commands;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace QB.Helpers
{

    public class SqlLogger : Item
    {
        List<LogObject> logList = new List<LogObject>();
        private System.Threading.CancellationTokenSource cts;
        public ConcurrentQueue<string> Lines = new ConcurrentQueue<string>();
        public TimeSpan timeRel;
        public DateTime start;
        public bool Running = false;
 
        string connectionString;

        public string file = "default";

        private Task writingTask;

        List<Task> loggingTasks = new List<Task>();

        Dictionary<string, int> loggers = new Dictionary<string, int>();
        string insertString;


        public SqlLogger(string name) : base(name)
        {

        }

        public void Add(string name, string description, string unit, string format, string logger, Func<object> value)
        {
            string type = "TEXT";
            object result = value(); // Call the Func<object> to get the actual object

            if (result is double)
                type = "REAL";
            else if (result is Int16 || result is Int32 || result is Int64) 
                type = "REAL";
            else if (result is DateTime)
                type = "TEXT";

            logList.Add(new LogObject(name, unit, format, value, type, logger, description));
        }

        public bool init()
        {
            if (file == "default")
            {
                string dbFile = Path.Combine(QB.Book.DataDirectory, Name + ".db");
                SQLiteConnection.CreateFile(dbFile);
                connectionString = $"Data Source={dbFile};Version=3;";
            }
            else
            {
                SQLiteConnection.CreateFile(file);
                connectionString = $"Data Source={file};Version=3;";
            }

            string cmd = "";
            using (var database = new SQLiteConnection(connectionString))
            {
                try
                {
                    database.Open();
                    cmd = @"
                     DROP TABLE IF EXISTS valueData;
                     CREATE TABLE valueData (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT,
                        description TEXT,
                        unit TEXT,
                        valueType TEXT,
                        format TEXT,
                        sqlTable TEXT
                    );";
                    SQLiteCommand command = new SQLiteCommand(cmd, database);
                    command.ExecuteNonQuery();

                    Dictionary<string,List<string>> tables = new Dictionary<string, List<string>>();
                    foreach (LogObject i in logList)
                        {
                        cmd = $"INSERT INTO valueData (name, description, unit, valueType, format, sqlTable) VALUES ('{i.Name}','{i.Description}', '{i.Unit}', '{i.ValueType}', '{i.Format}','{i.SqlTable}');";
                        command = new SQLiteCommand(cmd, database);
                        command.ExecuteNonQuery();
                        if (!tables.ContainsKey(i.SqlTable))
                        {
                            tables.Add(i.SqlTable, new List<string>() { i.Name + " " + i.ValueType });
                        }
                        else
                        {
                            tables[i.SqlTable].Add(i.Name + " " + i.ValueType);
                        }
                
                        }

                    foreach(var entry in tables)
                        { 
                        string tbl = entry.Key;
                        string values = string.Join(", ", entry.Value);
                        cmd = $"DROP TABLE IF EXISTS {tbl};  CREATE TABLE {tbl}(id INTEGER PRIMARY KEY AUTOINCREMENT,datetime TEXT, timeRel REAL, {values});";
                        command = new SQLiteCommand(cmd, database); 
                        command.ExecuteNonQuery();
                        }
                    database.Close();

                }

                catch (SQLiteException ex)
                {
                    QB.Logger.Error($"{Name}.SqlLogger SQLite error on init: {ex.Message}");
                    database.Close();
                    return false;
                }
                catch (Exception ex)
                {
                    QB.Logger.Error($"{Name}.SqlLogger general error on init: {ex.Message}");
                    database.Close();
                    return false;
                }

                return true;

            }
        }


        public void Start()
        {

            Running = true;
  
            cts = new System.Threading.CancellationTokenSource();

            foreach(var item in loggers)
            {
                string tbl = item.Key;
                int i = item.Value;
                Task logging = Task.Run(() => RunLogger(cts.Token, i, tbl));
                loggingTasks.Add(logging);

            }
            writingTask = Task.Run(() => WriteLogsToFile(cts.Token));
            start = DateTime.Now;
        }

        public async Task Stop()
        {
            try
            {
                Running = false;

                if (cts == null) return;
                cts.Cancel();

                foreach(Task i in loggingTasks)
                    await i;

                if (writingTask != null)
                {
                    await writingTask;
                }

            }
            catch
            {
                using (var database = new SQLiteConnection(connectionString))
                {
                    database.Close();
                }
            }

        }




        public void AddLogger(string tableName, int interval) 
        {
            QB.Logger.Debug($"Adding logger: table '{tableName}' interval '{interval}'");
            loggers.Add(tableName, interval);
        }

        private void RunLogger(System.Threading.CancellationToken token, int interval, string logger)
        {
            string insertValues = "";

            foreach(LogObject item in logList) 
                if (item.SqlTable == logger) insertValues +=  item.Name + ",";

            insertValues = insertValues.Substring(0, insertValues.Length-1);

            string insert = $"INSERT INTO {logger} ({insertValues}) VALUES (";

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    timeRel = DateTime.Now - start;
                    Lines.Enqueue($"INSERT INTO {logger} (datetime, timeRel, {insertValues}) VALUES ('{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")}','{timeRel.TotalSeconds.ToString("0.000")}',{getValues(logger)})");

                    // Wait for the next interval using Stopwatch
                    while (stopwatch.ElapsedMilliseconds < interval)
                    {
                        System.Threading.Thread.SpinWait(1); // Busy-wait to avoid sleep inaccuracy
                    }
                    stopwatch.Restart();
                }
            }
            catch (TaskCanceledException)
            {
                QB.Logger.Info($"{Name}.SqlLogger logging task canceled successfully.");
            }
            catch (Exception ex)
            {
                QB.Logger.Error($"{Name}.SqlLogger logging task encountered an error: {ex.Message}");
            }
     

        }

        //public void OpenFolder()
        //{
        //    System.Diagnostics.Process.Start("explorer.exe", Folder);
        //}
        private async Task WriteLogsToFile(System.Threading.CancellationToken token)
        {
            //myWriter = new StreamWriter(Folder + "\\" + Filename, append: true, encoding: Encoding.UTF8);

            using (var database = new SQLiteConnection(connectionString))
            {
                try
                {
                    database.Open();
                    while (!token.IsCancellationRequested)
                    {
                        if (!Lines.IsEmpty)
                        {
                            while (Lines.TryDequeue(out string cmd))
                            {
                                SQLiteCommand command = new SQLiteCommand(cmd, database);
                                command.ExecuteNonQuery();
                            }
                        }
                        await Task.Delay(50, token); // Adjust delay for batch writing
                    }
                    database.Close();
                }
                catch (TaskCanceledException)
                {
                    QB.Logger.Info($"{Name}.SqlLogger writing task canceled successfully.");
                }
                catch (Exception ex)
                {
                    QB.Logger.Error($"{Name}.SqlLogger writing task encountered an error: {ex.Message}");
                }
                finally
                {
                    database.Close();
                }
            }

        }

        public virtual string getValues(string logger)
        {

            try
            {
                var stringBuilder = new StringBuilder();
                TimeSpan t = DateTime.Now - start;

                foreach (LogObject item in logList)
                {
                    if (item.SqlTable != logger) continue;
                    object obj = item.GetLogObject;
                    //    Debug.WriteLine(item.Name + " '" + item.CurrentValue + "' -> " + item.Object.GetType().Name);

                    string TypeName = item.Object.GetType().Name;
                    if (item.Object is Double)
                    {
                        double i = (double)item.CurrentValue;
                        stringBuilder.Append("'" + i.ToString(item.Format)+ "'").Append(",");
                    }

                    if (item.Object is DateTime)
                    {
                        DateTime dt = (DateTime)item.CurrentValue;
                        stringBuilder.Append("'"+dt.ToString(item.Format)+ "'").Append(",");

                    }

                    if (item.Object is Int16 || item.Object is Int32 || item.Object is Int64)
                        stringBuilder.Append("'"+item.CurrentValue+ "'").Append(",");
                    if (item.Object is string)
                        stringBuilder.Append("'"+item.CurrentValue+ "'").Append(",");
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
    }
}
