using ActiproSoftware.UI.WinForms.Controls.Commands;
//using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace QB.Logging
{
    /// <summary>
    /// Represents a logger that writes data to an SQLite database file.
    /// </summary>
    public class SqlLogger : Item
    {
        private static readonly object sqliteFunctionSync = new object();
        private static bool sqliteFunctionsRegistered = false;
        private readonly object syncRoot = new object();
        Dictionary<string,LogObject> logList = new Dictionary<string,LogObject>();
        List<StatisticDefinition> statisticDefinitions = new List<StatisticDefinition>();
        private CancellationTokenSource cts;
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
        private bool stopping = false;
        private bool destroyed = false;

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
            EnsureSqlFunctionsRegistered();
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
            lock (syncRoot)
            {
                if (Running)
                {
                    QB.Logger.Error($"{Name}.SqlLogger Add is not allowed while logger is running.");
                    return;
                }

                string type = "TEXT";
                object result = value(); // Call the Func<object> to get the actual object

                if (result is double)
                    type = "REAL";
                else if (result is Int16 || result is Int32 || result is Int64) 
                    type = "REAL";
                else if (result is DateTime)
                    type = "TEXT";
                else if (result is Image)
                    type = "BLOB";

                else if (result is Bitmap)
                    type = "BLOB";

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
        /// Adds a derivative statistic based on a previously defined raw log value or statistic.
        /// </summary>
        /// <param name="name">The unique name of the calculated statistic.</param>
        /// <param name="sourceName">The source value name to derive.</param>
        /// <param name="text">A descriptive text for the statistic.</param>
        /// <param name="unit">The unit of the calculated statistic.</param>
        /// <param name="format">The display format of the calculated statistic.</param>
        /// <param name="factor">A scaling factor applied after the derivative calculation.</param>
        /// <param name="deadband">Absolute value changes smaller than this threshold are treated as zero.</param>
        /// <param name="minDtSeconds">The minimum allowed delta time in seconds. Smaller intervals return <c>NULL</c>.</param>
        public void AddDerivative(string name, string sourceName, string text, string unit, string format, double factor = 1.0, double deadband = 0.0, double minDtSeconds = 0.0)
        {
            lock (syncRoot)
            {
                if (Running)
                {
                    QB.Logger.Error($"{Name}.SqlLogger AddDerivative is not allowed while logger is running.");
                    return;
                }

                if (!TryResolveValueSource(sourceName, logList, statisticDefinitions, out ValueSourceDefinition source))
                {
                    QB.Logger.Error($"{Name}.SqlLogger AddDerivative source '{sourceName}' was not found.");
                    return;
                }

                if (source.ValueType != "REAL")
                {
                    QB.Logger.Error($"{Name}.SqlLogger AddDerivative source '{sourceName}' must be numeric.");
                    return;
                }

                AddStatistic(new StatisticDefinition(
                    StatisticType.Derivative,
                    name,
                    sourceName,
                    text,
                    unit,
                    format,
                    factor,
                    deadband,
                    minDtSeconds,
                    0.0));
            }
        }
        /// <summary>
        /// Adds an integral statistic based on a previously defined raw log value or statistic.
        /// </summary>
        /// <param name="name">The unique name of the calculated statistic.</param>
        /// <param name="sourceName">The source value name to integrate.</param>
        /// <param name="text">A descriptive text for the statistic.</param>
        /// <param name="unit">The unit of the calculated statistic.</param>
        /// <param name="format">The display format of the calculated statistic.</param>
        /// <param name="factor">A scaling factor applied after multiplying the source value with the elapsed time.</param>
        /// <param name="minDtSeconds">The minimum allowed delta time in seconds. Smaller intervals contribute <c>0</c> to the integral.</param>
        public void AddIntegral(string name, string sourceName, string text, string unit, string format, double factor = 1.0, double minDtSeconds = 0.0)
        {
            lock (syncRoot)
            {
                if (Running)
                {
                    QB.Logger.Error($"{Name}.SqlLogger AddIntegral is not allowed while logger is running.");
                    return;
                }

                if (!TryResolveValueSource(sourceName, logList, statisticDefinitions, out ValueSourceDefinition source))
                {
                    QB.Logger.Error($"{Name}.SqlLogger AddIntegral source '{sourceName}' was not found.");
                    return;
                }

                if (source.ValueType != "REAL")
                {
                    QB.Logger.Error($"{Name}.SqlLogger AddIntegral source '{sourceName}' must be numeric.");
                    return;
                }

                AddStatistic(new StatisticDefinition(
                    StatisticType.Integral,
                    name,
                    sourceName,
                    text,
                    unit,
                    format,
                    factor,
                    0.0,
                    minDtSeconds,
                    0.0));
            }
        }
        /// <summary>
        /// Adds a moving average statistic based on a previously defined raw log value or statistic.
        /// </summary>
        /// <param name="name">The unique name of the calculated statistic.</param>
        /// <param name="sourceName">The source value name to average.</param>
        /// <param name="text">A descriptive text for the statistic.</param>
        /// <param name="unit">The unit of the calculated statistic.</param>
        /// <param name="format">The display format of the calculated statistic.</param>
        /// <param name="windowSeconds">The averaging time window in seconds.</param>
        public void AddMovingAverage(string name, string sourceName, string text, string unit, string format, double windowSeconds)
        {
            AddWindowStatistic(StatisticType.MovingAverage, name, sourceName, text, unit, format, windowSeconds);
        }
        /// <summary>
        /// Adds a minimum statistic based on a previously defined raw log value or statistic.
        /// </summary>
        /// <param name="name">The unique name of the calculated statistic.</param>
        /// <param name="sourceName">The source value name to evaluate.</param>
        /// <param name="text">A descriptive text for the statistic.</param>
        /// <param name="unit">The unit of the calculated statistic.</param>
        /// <param name="format">The display format of the calculated statistic.</param>
        /// <param name="windowSeconds">The evaluation time window in seconds.</param>
        public void AddMin(string name, string sourceName, string text, string unit, string format, double windowSeconds)
        {
            AddWindowStatistic(StatisticType.Minimum, name, sourceName, text, unit, format, windowSeconds);
        }
        /// <summary>
        /// Adds a maximum statistic based on a previously defined raw log value or statistic.
        /// </summary>
        /// <param name="name">The unique name of the calculated statistic.</param>
        /// <param name="sourceName">The source value name to evaluate.</param>
        /// <param name="text">A descriptive text for the statistic.</param>
        /// <param name="unit">The unit of the calculated statistic.</param>
        /// <param name="format">The display format of the calculated statistic.</param>
        /// <param name="windowSeconds">The evaluation time window in seconds.</param>
        public void AddMax(string name, string sourceName, string text, string unit, string format, double windowSeconds)
        {
            AddWindowStatistic(StatisticType.Maximum, name, sourceName, text, unit, format, windowSeconds);
        }
        /// <summary>
        /// Adds a population standard deviation statistic based on a previously defined raw log value or statistic.
        /// </summary>
        /// <param name="name">The unique name of the calculated statistic.</param>
        /// <param name="sourceName">The source value name to evaluate.</param>
        /// <param name="text">A descriptive text for the statistic.</param>
        /// <param name="unit">The unit of the calculated statistic.</param>
        /// <param name="format">The display format of the calculated statistic.</param>
        /// <param name="windowSeconds">The evaluation time window in seconds.</param>
        public void AddStdDevPopulation(string name, string sourceName, string text, string unit, string format, double windowSeconds)
        {
            AddWindowStatistic(StatisticType.StdDevPopulation, name, sourceName, text, unit, format, windowSeconds);
        }
        /// <summary>
        /// Adds a sample standard deviation statistic based on a previously defined raw log value or statistic.
        /// </summary>
        /// <param name="name">The unique name of the calculated statistic.</param>
        /// <param name="sourceName">The source value name to evaluate.</param>
        /// <param name="text">A descriptive text for the statistic.</param>
        /// <param name="unit">The unit of the calculated statistic.</param>
        /// <param name="format">The display format of the calculated statistic.</param>
        /// <param name="windowSeconds">The evaluation time window in seconds.</param>
        public void AddStdDevSample(string name, string sourceName, string text, string unit, string format, double windowSeconds)
        {
            AddWindowStatistic(StatisticType.StdDevSample, name, sourceName, text, unit, format, windowSeconds);
        }

        private void AddWindowStatistic(StatisticType type, string name, string sourceName, string text, string unit, string format, double windowSeconds)
        {
            lock (syncRoot)
            {
                if (Running)
                {
                    QB.Logger.Error($"{Name}.SqlLogger {type} is not allowed while logger is running.");
                    return;
                }

                if (windowSeconds <= 0.0)
                {
                    QB.Logger.Error($"{Name}.SqlLogger {type} windowSeconds must be greater than zero.");
                    return;
                }

                if (!TryResolveValueSource(sourceName, logList, statisticDefinitions, out ValueSourceDefinition source))
                {
                    QB.Logger.Error($"{Name}.SqlLogger {type} source '{sourceName}' was not found.");
                    return;
                }

                if (source.ValueType != "REAL")
                {
                    QB.Logger.Error($"{Name}.SqlLogger {type} source '{sourceName}' must be numeric.");
                    return;
                }

                AddStatistic(new StatisticDefinition(
                    type,
                    name,
                    sourceName,
                    text,
                    unit,
                    format,
                    1.0,
                    0.0,
                    0.0,
                    windowSeconds));
            }
        }
        /// <summary>
        /// Initializes the database. This includes creating the DB file and setting up tables.
        /// </summary>
        /// <returns><c>true</c> if initialization is successful; otherwise, <c>false</c>.</returns>
        public bool Init()
        {
            EnsureSqlFunctionsRegistered();

            Dictionary<string, LogObject> logListSnapshot;
            List<StatisticDefinition> statisticDefinitionsSnapshot;

            lock (syncRoot)
            {
                if (Running)
                {
                    QB.Logger.Error($"{Name}.SqlLogger Init is not allowed while logger is running.");
                    return false;
                }

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

                logListSnapshot = new Dictionary<string, LogObject>(logList);
                statisticDefinitionsSnapshot = statisticDefinitions.ToList();
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

                    cmd = @"
                     DROP TABLE IF EXISTS statisticData;
                     CREATE TABLE statisticData (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT,
                        description TEXT,
                        unit TEXT,
                        valueType TEXT,
                        format TEXT,
                        sqlTable TEXT,
                        sourceName TEXT,
                        statisticType TEXT,
                        factor REAL,
                        deadband REAL,
                        minDtSeconds REAL,
                        windowSeconds REAL
                    );";
                    command = new SQLiteCommand(cmd, database);
                    command.ExecuteNonQuery();

                    Dictionary<string, List<string>> tables = new Dictionary<string, List<string>>();
                    foreach (var i in logListSnapshot)
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

                    List<StatisticDefinition> availableStatistics = new List<StatisticDefinition>();
                    foreach (var statistic in statisticDefinitionsSnapshot)
                    {
                        if (!TryResolveValueSource(statistic.SourceName, logListSnapshot, availableStatistics, out ValueSourceDefinition source))
                            throw new InvalidOperationException($"Statistic source '{statistic.SourceName}' was not found.");

                        cmd = BuildStatisticViewSql(statistic, source);
                        command = new SQLiteCommand(cmd, database);
                        command.ExecuteNonQuery();

                        cmd = $"INSERT INTO valueData (name, description, unit, valueType, format, sqlTable) VALUES ('{EscapeSqlLiteral(statistic.Name)}','{EscapeSqlLiteral(statistic.Description)}', '{EscapeSqlLiteral(statistic.Unit)}', '{EscapeSqlLiteral(statistic.ValueType)}', '{EscapeSqlLiteral(statistic.Format)}','{EscapeSqlLiteral(statistic.SqlTable)}');";
                        command = new SQLiteCommand(cmd, database);
                        command.ExecuteNonQuery();

                        cmd = $"INSERT INTO statisticData (name, description, unit, valueType, format, sqlTable, sourceName, statisticType, factor, deadband, minDtSeconds, windowSeconds) VALUES ('{EscapeSqlLiteral(statistic.Name)}','{EscapeSqlLiteral(statistic.Description)}', '{EscapeSqlLiteral(statistic.Unit)}', '{EscapeSqlLiteral(statistic.ValueType)}', '{EscapeSqlLiteral(statistic.Format)}','{EscapeSqlLiteral(statistic.SqlTable)}','{EscapeSqlLiteral(statistic.SourceName)}','{EscapeSqlLiteral(statistic.Type.ToString())}',{ToSqlNumberLiteral(statistic.Factor)},{ToSqlNumberLiteral(statistic.Deadband)},{ToSqlNumberLiteral(statistic.MinDtSeconds)},{ToSqlNumberLiteral(statistic.WindowSeconds)});";
                        command = new SQLiteCommand(cmd, database);
                        command.ExecuteNonQuery();

                        availableStatistics.Add(statistic);
                    }
                    database.Close();
                }

                catch (SQLiteException ex)
                {
                    lock (syncRoot)
                    {
                        initDb = false;
                    }

                    QB.Logger.Error($"{Name}.SqlLogger SQLite error on init: {ex.Message}");
                    database.Close();
                    return false;
                }
                catch (Exception ex)
                {
                    lock (syncRoot)
                    {
                        initDb = false;
                    }

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
            lock (syncRoot)
            {
                if (Running)
                {
                    QB.Logger.Error($"{Name}.SqlLogger Reset is not allowed while logger is running.");
                    return;
                }

                initDb = false;
            }
        }
        /// <summary>
        /// Starts the logging process by initiating background tasks for data collection and writing.
        /// </summary>
        public void Start()
        {
            CancellationToken token;
            List<KeyValuePair<string, int>> loggerSnapshot;

            lock (syncRoot)
            {
                if (destroyed)
                {
                    QB.Logger.Error($"{Name}.SqlLogger cannot be started after Destroy.");
                    return;
                }

                if (Running)
                {
                    QB.Logger.Info($"{Name}.SqlLogger Start ignored because logger is already running.");
                    return;
                }

                if (stopping)
                {
                    QB.Logger.Info($"{Name}.SqlLogger Start ignored because logger is stopping.");
                    return;
                }

                if (!initDb && !Init())
                    return;

                Lines = new ConcurrentQueue<string>();
                loggingTasks = new List<Task>();
                cts = new CancellationTokenSource();
                token = cts.Token;
                start = DateTime.Now;
                timeRel = TimeSpan.Zero;
                stopping = false;
                Running = true;
                loggerSnapshot = loggers.ToList();
            }

            foreach(var item in loggerSnapshot)
            {
                string tbl = item.Key;
                int i = item.Value;
                Task logging = Task.Run(() => RunLogger(token, i, tbl));
                lock (syncRoot)
                {
                    loggingTasks.Add(logging);
                }

            }
            lock (syncRoot)
            {
                writingTask = Task.Run(() => WriteLogsToFile(token));
            }
        }
        /// <summary>
        /// Stops the logging process gracefully and waits for all pending data to be written to the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous stop operation.</returns>
        public async Task Stop()
        {
            CancellationTokenSource localCts = null;
            List<Task> tasksToAwait = null;
            Task localWritingTask = null;

            try
            {
                lock (syncRoot)
                {
                    if (stopping)
                        return;

                    Running = false;
                    stopping = true;

                    localCts = cts;
                    if (localCts == null)
                    {
                        stopping = false;
                        return;
                    }

                    tasksToAwait = new List<Task>(loggingTasks);
                    localWritingTask = writingTask;
                }

                localCts.Cancel();

                foreach(Task i in tasksToAwait)
                    await i;

                if (localWritingTask != null)
                {
                    await localWritingTask;
                }

            }
            catch (TaskCanceledException)
            {
            }
            catch (OperationCanceledException)
            {
            }
            catch
            {
                using (var database = new SQLiteConnection(connectionString))
                {
                    database.Close();
                }
            }
            finally
            {
                if (localCts != null)
                    localCts.Dispose();

                lock (syncRoot)
                {
                    if (ReferenceEquals(cts, localCts))
                        cts = null;

                    loggingTasks.Clear();
                    writingTask = null;
                    stopping = false;
                }
            }

        }

        public override void Destroy()
        {
            lock (syncRoot)
            {
                if (destroyed)
                    return;

                destroyed = true;
            }

            try
            {
                Stop().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                QB.Logger.Error($"{Name}.SqlLogger destroy encountered an error: {ex.Message}");
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
        /// <summary>
        /// Executes a scalar SQL query against the SQLite database.
        /// </summary>
        /// <param name="sql">The SQL query to execute.</param>
        /// <returns>The first column of the first row in the result set; otherwise, <c>null</c>.</returns>
        public object QueryScalar(string sql)
        {
            return QueryScalarInternal(sql, null);
        }

        private object QueryScalarInternal(string sql, params SQLiteParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(sql) || string.IsNullOrWhiteSpace(connectionString))
                return null;

            try
            {
                using (var database = new SQLiteConnection(connectionString))
                {
                    database.Open();

                    using (var command = new SQLiteCommand(sql, database))
                    {
                        if (parameters != null && parameters.Length > 0)
                            command.Parameters.AddRange(parameters);

                        object result = command.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                            return null;

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                QB.Logger.Error($"{Name}.SqlLogger QueryScalar failed: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// Executes a scalar SQL query and converts the result to a <see cref="string"/>.
        /// </summary>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="defaultValue">The fallback value returned when no valid string value is available.</param>
        /// <returns>The scalar result as a string if available; otherwise, <paramref name="defaultValue"/>.</returns>
        public string QueryString(string sql, string defaultValue = null)
        {
            object value = QueryScalar(sql);
            if (value == null)
                return defaultValue;

            try
            {
                return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaultValue;
            }
        }
        /// <summary>
        /// Executes a scalar SQL query and converts the result to a <see cref="double"/>.
        /// </summary>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="defaultValue">The fallback value returned when no valid numeric value is available.</param>
        /// <returns>The scalar result as a double if available; otherwise, <paramref name="defaultValue"/>.</returns>
        public double QueryDouble(string sql, double defaultValue = double.NaN)
        {
            object value = QueryScalar(sql);
            if (value == null)
                return defaultValue;

            try
            {
                return Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaultValue;
            }
        }
        /// <summary>
        /// Gets the latest persisted raw or statistic value from the SQLite database.
        /// </summary>
        /// <param name="name">The name of the raw log value or statistic definition.</param>
        /// <returns>The latest persisted value if available; otherwise, <c>null</c>.</returns>
        public object GetLatest(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            string sqlTable;
            string valueType;

            lock (syncRoot)
            {
                if (!TryResolveValueSource(name, logList, statisticDefinitions, out ValueSourceDefinition source))
                    return null;

                sqlTable = source.SqlTable;
                valueType = source.ValueType;
            }

            if (string.IsNullOrWhiteSpace(connectionString))
                return null;

            try
            {
                using (var database = new SQLiteConnection(connectionString))
                {
                    database.Open();

                    string cmdText = $"SELECT {EscapeSqlIdentifier(name)} FROM {EscapeSqlIdentifier(sqlTable)} ORDER BY id DESC LIMIT 1;";
                    using (var command = new SQLiteCommand(cmdText, database))
                    {
                        object result = command.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                            return null;

                        if (valueType == "REAL")
                            return Convert.ToDouble(result, System.Globalization.CultureInfo.InvariantCulture);

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                QB.Logger.Error($"{Name}.SqlLogger GetLatest('{name}') failed: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// Gets the latest persisted raw or statistic value from the SQLite database as a <see cref="double"/>.
        /// </summary>
        /// <param name="name">The name of the raw log value or statistic definition.</param>
        /// <param name="defaultValue">The fallback value returned when no valid numeric value is available.</param>
        /// <returns>The latest persisted numeric value if available; otherwise, <paramref name="defaultValue"/>.</returns>
        public double GetLatestDouble(string name, double defaultValue = double.NaN)
        {
            object value = GetLatest(name);
            if (value == null)
                return defaultValue;

            try
            {
                return Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaultValue;
            }
        }
        private void RunLogger(CancellationToken token, int interval, string logger)
        {
            string insertValues = "";

            Dictionary<string, LogObject> logListSnapshot;
            lock (syncRoot)
            {
                logListSnapshot = new Dictionary<string, LogObject>(logList);
            }

            foreach(var item in logListSnapshot) 
                if (item.Value.SqlTable == logger) insertValues +=  item.Value.Name + ",";

            if (insertValues.Length == 0)
                return;

            insertValues = insertValues.Substring(0, insertValues.Length-1);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    timeRel = DateTime.Now - start;
                    Lines.Enqueue($"INSERT INTO {logger} (datetime, timeRel, {insertValues}) VALUES ('{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")}','{timeRel.TotalSeconds.ToString("0.000")}',{getValues(logger)})");

                    WaitForNextInterval(stopwatch, interval, token);
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
        private async Task WriteLogsToFile(CancellationToken token)
        {
            //myWriter = new StreamWriter(Folder + "\\" + Filename, append: true, encoding: Encoding.UTF8);

            using (var database = new SQLiteConnection(connectionString))
            {
                try
                {
                    database.Open();
                    while (!token.IsCancellationRequested || !Lines.IsEmpty)
                    {
                        FlushQueue(database);

                        if (!token.IsCancellationRequested && Lines.IsEmpty)
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
            Dictionary<string, LogObject> logListSnapshot;

            lock (syncRoot)
            {
                logListSnapshot = new Dictionary<string, LogObject>(logList);
            }

            try
            {
                var stringBuilder = new StringBuilder();
                TimeSpan t = DateTime.Now - start;

                foreach (var item in logListSnapshot)
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

                    if (item.Value.Object is Image)
                    {
                        var imageValue = item.Value.CurrentValue as Image;
                        stringBuilder.Append(ToSqlBlobLiteral(imageValue)).Append(",");
                    }

                    if (item.Value.Object is Bitmap)
                    {

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

        private void FlushQueue(SQLiteConnection database)
        {
            while (Lines.TryDequeue(out string cmd))
            {
                SQLiteCommand command = new SQLiteCommand(cmd, database);
                command.ExecuteNonQuery();
            }
        }

        private static void WaitForNextInterval(Stopwatch stopwatch, int interval, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                long remaining = interval - stopwatch.ElapsedMilliseconds;
                if (remaining <= 0)
                    return;

                if (remaining > 2)
                {
                    Thread.Sleep(1);
                    continue;
                }

                Thread.SpinWait(50);
            }
        }

        private static string ToSqlBlobLiteral(Image image)
        {
            if (image == null)
                return "NULL";

            try
            {
                using (var ms = new MemoryStream())
                {
                    image.Save(ms, ImageFormat.Jpeg);
                    return ToSqlBlobLiteral(ms.ToArray());
                }
            }
            catch
            {
                return "NULL";
            }
        }

        private static string ToSqlBlobLiteral(byte[] data)
        {
            if (data == null || data.Length == 0)
                return "NULL";

            var hex = new StringBuilder(data.Length * 2);
            for (int i = 0; i < data.Length; i++)
            {
                hex.Append(data[i].ToString("X2"));
            }

            return "X'" + hex + "'";
        }

        private void AddStatistic(StatisticDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(definition.Name))
            {
                QB.Logger.Error($"{Name}.SqlLogger statistic name must not be empty.");
                return;
            }

            if (ContainsValueDefinition(definition.Name))
            {
                QB.Logger.Error($"SQLlogger '{Name}' already contains Key: '{definition.Name}'");
                return;
            }

            statisticDefinitions.Add(definition);
        }

        private bool ContainsValueDefinition(string name)
        {
            return logList.ContainsKey(name) || statisticDefinitions.Any(i => i.Name == name);
        }

        private static bool TryResolveValueSource(string sourceName, IDictionary<string, LogObject> rawDefinitions, IEnumerable<StatisticDefinition> statistics, out ValueSourceDefinition source)
        {
            if (rawDefinitions.TryGetValue(sourceName, out LogObject logObject))
            {
                source = new ValueSourceDefinition(logObject.Name, logObject.SqlTable, logObject.ValueType);
                return true;
            }

            StatisticDefinition statistic = statistics.FirstOrDefault(i => i.Name == sourceName);
            if (statistic != null)
            {
                source = new ValueSourceDefinition(statistic.Name, statistic.SqlTable, statistic.ValueType);
                return true;
            }

            source = null;
            return false;
        }

        private static string BuildStatisticViewSql(StatisticDefinition definition, ValueSourceDefinition source)
        {
            string viewName = EscapeSqlIdentifier(definition.SqlTable);
            string sourceTable = EscapeSqlIdentifier(source.SqlTable);
            string sourceColumn = EscapeSqlIdentifier(source.Name);
            string targetColumn = EscapeSqlIdentifier(definition.Name);

            if (definition.Type == StatisticType.Derivative)
            {
                return $@"
DROP VIEW IF EXISTS {viewName};
CREATE VIEW {viewName} AS
SELECT
    cur.id AS id,
    cur.datetime AS datetime,
    cur.timeRel AS timeRel,
    CASE
        WHEN prev.id IS NULL THEN NULL
        WHEN (cur.timeRel - prev.timeRel) <= {ToSqlNumberLiteral(definition.MinDtSeconds)} THEN NULL
        WHEN ABS(CAST(cur.{sourceColumn} AS REAL) - CAST(prev.{sourceColumn} AS REAL)) <= {ToSqlNumberLiteral(definition.Deadband)} THEN 0.0
        ELSE ((CAST(cur.{sourceColumn} AS REAL) - CAST(prev.{sourceColumn} AS REAL)) / (cur.timeRel - prev.timeRel)) * {ToSqlNumberLiteral(definition.Factor)}
    END AS {targetColumn}
FROM {sourceTable} cur
LEFT JOIN {sourceTable} prev ON prev.id = (
    SELECT MAX(p.id)
    FROM {sourceTable} p
    WHERE p.id < cur.id
);";
            }

            if (definition.Type == StatisticType.Integral)
            {
                return $@"
DROP VIEW IF EXISTS {viewName};
CREATE VIEW {viewName} AS
SELECT
    cur.id AS id,
    cur.datetime AS datetime,
    cur.timeRel AS timeRel,
    (
        SELECT SUM(
            CASE
                WHEN prev.id IS NULL THEN 0.0
                WHEN (src.timeRel - prev.timeRel) <= {ToSqlNumberLiteral(definition.MinDtSeconds)} THEN 0.0
                ELSE CAST(src.{sourceColumn} AS REAL) * (src.timeRel - prev.timeRel) * {ToSqlNumberLiteral(definition.Factor)}
            END)
        FROM {sourceTable} src
        LEFT JOIN {sourceTable} prev ON prev.id = (
            SELECT MAX(p.id)
            FROM {sourceTable} p
            WHERE p.id < src.id
        )
        WHERE src.id <= cur.id
    ) AS {targetColumn}
FROM {sourceTable} cur;";
            }

            if (definition.Type == StatisticType.Minimum)
            {
                return BuildWindowAggregateViewSql(viewName, sourceTable, sourceColumn, targetColumn, definition.WindowSeconds, "MIN");
            }

            if (definition.Type == StatisticType.Maximum)
            {
                return BuildWindowAggregateViewSql(viewName, sourceTable, sourceColumn, targetColumn, definition.WindowSeconds, "MAX");
            }

            if (definition.Type == StatisticType.StdDevPopulation)
            {
                string valueExpression = $"CAST(src.{sourceColumn} AS REAL)";
                return $@"
DROP VIEW IF EXISTS {viewName};
CREATE VIEW {viewName} AS
SELECT
    cur.id AS id,
    cur.datetime AS datetime,
    cur.timeRel AS timeRel,
    (
        SELECT CASE
            WHEN COUNT(*) = 0 THEN NULL
            ELSE QBSQRT(
                CASE
                    WHEN (AVG({valueExpression} * {valueExpression}) - AVG({valueExpression}) * AVG({valueExpression})) < 0 THEN 0.0
                    ELSE (AVG({valueExpression} * {valueExpression}) - AVG({valueExpression}) * AVG({valueExpression}))
                END)
            END
        FROM {sourceTable} src
        WHERE src.timeRel >= cur.timeRel - {ToSqlNumberLiteral(definition.WindowSeconds)}
          AND src.timeRel <= cur.timeRel
          AND src.{sourceColumn} IS NOT NULL
    ) AS {targetColumn}
FROM {sourceTable} cur;";
            }

            if (definition.Type == StatisticType.StdDevSample)
            {
                string valueExpression = $"CAST(src.{sourceColumn} AS REAL)";
                return $@"
DROP VIEW IF EXISTS {viewName};
CREATE VIEW {viewName} AS
SELECT
    cur.id AS id,
    cur.datetime AS datetime,
    cur.timeRel AS timeRel,
    (
        SELECT CASE
            WHEN COUNT(*) <= 1 THEN NULL
            ELSE QBSQRT(
                CASE
                    WHEN ((SUM({valueExpression} * {valueExpression}) - ((SUM({valueExpression}) * SUM({valueExpression})) / COUNT(*))) / (COUNT(*) - 1)) < 0 THEN 0.0
                    ELSE ((SUM({valueExpression} * {valueExpression}) - ((SUM({valueExpression}) * SUM({valueExpression})) / COUNT(*))) / (COUNT(*) - 1))
                END)
            END
        FROM {sourceTable} src
        WHERE src.timeRel >= cur.timeRel - {ToSqlNumberLiteral(definition.WindowSeconds)}
          AND src.timeRel <= cur.timeRel
          AND src.{sourceColumn} IS NOT NULL
    ) AS {targetColumn}
FROM {sourceTable} cur;";
            }

            return BuildWindowAggregateViewSql(viewName, sourceTable, sourceColumn, targetColumn, definition.WindowSeconds, "AVG");
        }

        private static string BuildWindowAggregateViewSql(string viewName, string sourceTable, string sourceColumn, string targetColumn, double windowSeconds, string aggregateName)
        {
            return $@"
DROP VIEW IF EXISTS {viewName};
CREATE VIEW {viewName} AS
SELECT
    cur.id AS id,
    cur.datetime AS datetime,
    cur.timeRel AS timeRel,
    (
        SELECT {aggregateName}(CAST(src.{sourceColumn} AS REAL))
        FROM {sourceTable} src
        WHERE src.timeRel >= cur.timeRel - {ToSqlNumberLiteral(windowSeconds)}
          AND src.timeRel <= cur.timeRel
          AND src.{sourceColumn} IS NOT NULL
    ) AS {targetColumn}
FROM {sourceTable} cur;";
        }

        private static void EnsureSqlFunctionsRegistered()
        {
            lock (sqliteFunctionSync)
            {
                if (sqliteFunctionsRegistered)
                    return;

                SQLiteFunction.RegisterFunction(typeof(QbSqrtFunction));
                sqliteFunctionsRegistered = true;
            }
        }

        private static string EscapeSqlIdentifier(string name)
        {
            return "\"" + name.Replace("\"", "\"\"") + "\"";
        }

        private static string EscapeSqlLiteral(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }

        private static string ToSqlNumberLiteral(double value)
        {
            return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        private enum StatisticType
        {
            Derivative,
            Integral,
            MovingAverage,
            Minimum,
            Maximum,
            StdDevPopulation,
            StdDevSample
        }

        private sealed class StatisticDefinition
        {
            public StatisticType Type { get; private set; }
            public string Name { get; private set; }
            public string SourceName { get; private set; }
            public string Description { get; private set; }
            public string Unit { get; private set; }
            public string Format { get; private set; }
            public string ValueType { get; private set; }
            public string SqlTable { get; private set; }
            public double Factor { get; private set; }
            public double Deadband { get; private set; }
            public double MinDtSeconds { get; private set; }
            public double WindowSeconds { get; private set; }

            public StatisticDefinition(StatisticType type, string name, string sourceName, string description, string unit, string format, double factor, double deadband, double minDtSeconds, double windowSeconds)
            {
                Type = type;
                Name = name;
                SourceName = sourceName;
                Description = description;
                Unit = unit;
                Format = format;
                ValueType = "REAL";
                SqlTable = "s_" + name;
                Factor = factor;
                Deadband = deadband;
                MinDtSeconds = minDtSeconds;
                WindowSeconds = windowSeconds;
            }
        }

        private sealed class ValueSourceDefinition
        {
            public string Name { get; private set; }
            public string SqlTable { get; private set; }
            public string ValueType { get; private set; }

            public ValueSourceDefinition(string name, string sqlTable, string valueType)
            {
                Name = name;
                SqlTable = sqlTable;
                ValueType = valueType;
            }
        }

        [SQLiteFunction(Name = "QBSQRT", Arguments = 1, FuncType = FunctionType.Scalar)]
        private sealed class QbSqrtFunction : SQLiteFunction
        {
            public override object Invoke(object[] args)
            {
                if (args == null || args.Length == 0 || args[0] == null || args[0] == DBNull.Value)
                    return DBNull.Value;

                double value = Convert.ToDouble(args[0], System.Globalization.CultureInfo.InvariantCulture);
                if (value < 0.0)
                    return 0.0;

                return Math.Sqrt(value);
            }
        }
    }
}
