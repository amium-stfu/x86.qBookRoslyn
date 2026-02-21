using ActiproSoftware.UI.WinForms.Controls.Commands;
//using Microsoft.Extensions.Primitives;
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

namespace QB.Logging
{
    /// <summary>
    /// Represents a logger that writes data to an SQLite database file.
    /// </summary>
    public class SqlLogger : Item
    {
        Dictionary<string,LogObject> logList = new Dictionary<string,LogObject>();
        private System.Threading.CancellationTokenSource cts;
        /// <summary>
        /// A thread-safe queue that holds the SQL commands to be written to the database.
        /// </summary>
        public ConcurrentQueue<string> Lines = new ConcurrentQueue<string>();
        /// <summary>
        /// Gets the relative time elapsed since the logger started.
        /// </summary>
        public TimeSpan timeRel;
        /// <summary>
        /// Gets the exact date and time when the logger was started.
        /// </summary>
        public DateTime start;
        /// <summary>
        /// Gets a value indicating whether the logger is currently running.
        /// </summary>
        public bool Running = false;
        bool initDb = false;

        string connectionString;

        /// <summary>
        /// Gets or sets the path to the database file. If set to "default", a new file is created with a timestamp.
        /// </summary>
        public string File = "default";
        private Task writingTask;

        List<Task> loggingTasks = new List<Task>();

        protected Dictionary<string, int> loggers = new Dictionary<string, int>();
        string insertString;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlLogger"/> class with a specified name.
        /// </summary>
        /// <param name="name">The name of the logger instance.</param>
        public SqlLogger(string name) : base(name)
        {

        }
        /// <summary>
        /// Adds a data point to be logged periodically.
        /// </summary>
        /// <param name="name">The unique name for the log value (used as a column name).</param>
        /// <param name="text">A descriptive text for the value.</param>
        /// <param name="unit">The unit of the value.</param>
        /// <param name="format">The string format for the value.</param>
        /// <param name="period">The logging interval in milliseconds.</param>
        /// <param name="value">A function that returns the value to be logged.</param>
        public void Add(string name, string text, string unit, string format, int period, Func<object> value)
        {
            string type = "TEXT";
            object result = value(); // Call the Func<object> to get the actual object

            if (result is double)
                type = "REAL";
            else if (result is Int16 || result is Int32 || result is Int64) 
                type = "REAL";
            else if (result is DateTime)
                type = "TEXT";

            string tbl = "p" + period;

            if (!loggers.ContainsKey(tbl))
            {
                loggers.Add(tbl, period);
                Console.WriteLine(tbl);
            }
            if (!logList.ContainsKey(name))
                logList.Add(name, new LogObject(name, unit, format, value, type, tbl, text));
            else
                QB.Logger.Error($"SQLlogger '{Name}' already contains Key: '" + name + "'");
        }
        /// <summary>
        /// Adds a Signal to be logged.
        /// </summary>
        /// <param name="signal">The signal to log.</param>
        /// <param name="period">The logging interval in milliseconds.</param>
        public void AddSignal(Signal signal, int period) 
        {
            Add(signal.Name, signal.Text, signal.Unit, signal.DefaultDisplayFormat, period, () => signal.Value);

        }
        /// <summary>
        /// Adds a StringSignal to be logged.
        /// </summary>
        /// <param name="signal">The string signal to log.</param>
        /// <param name="period">The logging interval in milliseconds.</param>
        public void AddStringSignal(StringSignal signal, int period)
        {
            Add(signal.Name, signal.Text, "", "", period, () => signal.Value);

        }
        /// <summary>
        /// Initializes the database. This includes creating the DB file and setting up tables.
        /// </summary>
        /// <returns><c>true</c> if initialization is successful; otherwise, <c>false</c>.</returns>
        public bool Init()
        {
            initDb = true;
            if (File == "default")
            {
                string dbFile = Path.Combine(QB.Book.DataDirectory,DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss") + "_" + Name + ".db");
                SQLiteConnection.CreateFile(dbFile);
                connectionString = $"Data Source={dbFile};Version=3;";
            }
            else
            {
                SQLiteConnection.CreateFile(File);
                connectionString = $"Data Source={File};Version=3;";
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

                    Dictionary<string, List<string>> tables = new Dictionary<string, List<string>>();
                    foreach (var i in logList)
                    {
                        cmd = $"INSERT INTO valueData (name, description, unit, valueType, format, sqlTable) VALUES ('{i.Value.Name}','{i.Value.Description}', '{i.Value.Unit}', '{i.Value.ValueType}', '{i.Value.Format}','{i.Value.SqlTable}');";
                        command = new SQLiteCommand(cmd, database);
                        command.ExecuteNonQuery();

                        if (!tables.ContainsKey(i.Value.SqlTable))
                        {
                            tables.Add(i.Value.SqlTable, new List<string>() 
                                { i.Value.Name + " " + i.Value.ValueType });
                        }
                        else
                        {
                            tables[i.Value.SqlTable].Add(i.Value.Name + " " + i.Value.ValueType);
                        }
                    }

                    foreach (var entry in tables)
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
        /// <summary>
        /// Resets the initialization status of the database, forcing a re-initialization on the next <see cref="Start"/>.
        /// </summary>
        public void Reset()
        {
            initDb = false;
        }
        /// <summary>
        /// Starts the logging process by initiating background tasks for data collection and writing.
        /// </summary>
        public void Start()
        {

            if (!initDb)
                Init();

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
        /// <summary>
        /// Stops the logging process gracefully and waits for all pending data to be written to the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous stop operation.</returns>
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
        /// <summary>
        /// Checks if the database connection can be successfully opened.
        /// </summary>
        /// <returns><c>true</c> if the database is accessible; otherwise, <c>false</c>.</returns>
        public bool DatabaseIsOpen()
        {
            try
            {
                using (var database = new SQLiteConnection(connectionString))
                {
                    database.Open();
                    database.Close();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Checks if the database connection is closed.
        /// </summary>
        /// <returns><c>true</c> if the database is not accessible; otherwise, <c>false</c>.</returns>
        public bool DatabaseIsClosed() { 
            return !DatabaseIsOpen();
        }
        private void RunLogger(System.Threading.CancellationToken token, int interval, string logger)
        {
            string insertValues = "";

            foreach(var item in logList) 
                if (item.Value.SqlTable == logger) insertValues +=  item.Value.Name + ",";

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
        /// <summary>
        /// Retrieves and formats the current values of all data points for a specific logger table.
        /// </summary>
        /// <param name="logger">The name of the logger table (e.g., "p1000").</param>
        /// <returns>A comma-separated string of formatted values for an SQL INSERT statement.</returns>
        public virtual string getValues(string logger)
        {

            try
            {
                var stringBuilder = new StringBuilder();
                TimeSpan t = DateTime.Now - start;

                foreach (var item in logList)
                {
                    if (item.Value.SqlTable != logger) continue;
                    object obj = item.Value.GetLogObject;
                    //    Debug.WriteLine(item.Name + " '" + item.CurrentValue + "' -> " + item.Object.GetType().Name);

                    string TypeName = item.Value.Object.GetType().Name;
                    if (item.Value.Object is Double)
                    {
                        double i = (double)item.Value.CurrentValue;
                        stringBuilder.Append("'" + i.ToString(item.Value.Format)+ "'").Append(",");
                    }

                    if (item.Value.Object is DateTime)
                    {
                        DateTime dt = (DateTime)item.Value.CurrentValue;
                        stringBuilder.Append("'"+dt.ToString(item.Value.Format)+ "'").Append(",");

                    }

                    if (item.Value.Object is Int16 || item.Value.Object is Int32 || item.Value.Object is Int64)
                        stringBuilder.Append("'"+item.Value.CurrentValue+ "'").Append(",");
                    if (item.Value.Object is string)
                        stringBuilder.Append("'"+item.Value.CurrentValue+ "'").Append(",");
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
