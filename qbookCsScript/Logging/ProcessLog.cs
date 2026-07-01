using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace QB.Logging
{

/// <summary>
/// Provides structured process logging with buffered entries and file persistence.
/// </summary>
public class ProcessLog
{
    private const string ContextPropertyName = "ProcessLogContext";
    private const string MessagePropertyName = "ProcessLogMessage";
    private const string OwnerPropertyName = "ProcessLogOwner";
    private const string SenderPropertyName = "ProcessLogSender";
    private const int MaxBufferedRows = 1000;

    private readonly object _bufferLock = new object();
    private readonly DataTable _bufferTable = CreateBufferTable();
    private ILogger _log;
    private bool _showDebug = true;
    private bool _showInfo = true;
    private bool _showWarning = true;
    private bool _showError = true;
    private bool _showFatal = true;
    private bool _pause;
    private string _logDirectory;

    /// <summary>
    /// Initializes a new <see cref="ProcessLog"/> instance.
    /// </summary>
    /// <param name="logDirectory">The target directory for persisted log files.</param>
    /// <param name="owner">The logical owner written to each log entry.</param>
    public ProcessLog(string logDirectory, string owner)
    {
        ValidateNotNullOrWhiteSpace(owner, nameof(owner));
        Owner = owner;
        _logDirectory = logDirectory;
        InitializeLog(logDirectory);
    }

    /// <summary>
    /// Gets or sets the logical name of the log.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the owner of this log instance.
    /// </summary>
    public string Owner { get; }

    /// <summary>
    /// Gets or sets the target directory for persisted log files.
    /// </summary>
    public string Output
    {
        get => _logDirectory;
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            InitializeLog(value);
        }
    }

    /// <summary>
    /// Gets the configured Serilog logger.
    /// </summary>
    public ILogger Log => _log ?? throw new InvalidOperationException("ProcessLog not initialized. Call InitializeLog(...) first.");

    /// <summary>
    /// Raised when display-related settings change.
    /// </summary>
    public event System.Action DisplaySettingsChanged;

    /// <summary>
    /// Raised when a buffered entry was added and is visible.
    /// </summary>
    public event System.Action<ProcessLogEntry> EntryAdded;

    /// <summary>
    /// Gets or sets whether debug entries are visible in the buffered view.
    /// </summary>
    public bool ShowDebug
    {
        get => _showDebug;
        set
        {
            if (_showDebug == value)
            {
                return;
            }

            _showDebug = value;
            OnDisplaySettingsChanged();
        }
    }

    /// <summary>
    /// Gets or sets whether information entries are visible in the buffered view.
    /// </summary>
    public bool ShowInfo
    {
        get => _showInfo;
        set
        {
            if (_showInfo == value)
            {
                return;
            }

            _showInfo = value;
            OnDisplaySettingsChanged();
        }
    }

    /// <summary>
    /// Gets or sets whether warning entries are visible in the buffered view.
    /// </summary>
    public bool ShowWarning
    {
        get => _showWarning;
        set
        {
            if (_showWarning == value)
            {
                return;
            }

            _showWarning = value;
            OnDisplaySettingsChanged();
        }
    }

    /// <summary>
    /// Gets or sets whether error entries are visible in the buffered view.
    /// </summary>
    public bool ShowError
    {
        get => _showError;
        set
        {
            if (_showError == value)
            {
                return;
            }

            _showError = value;
            OnDisplaySettingsChanged();
        }
    }

    /// <summary>
    /// Gets or sets whether fatal entries are visible in the buffered view.
    /// </summary>
    public bool ShowFatal
    {
        get => _showFatal;
        set
        {
            if (_showFatal == value)
            {
                return;
            }

            _showFatal = value;
            OnDisplaySettingsChanged();
        }
    }

    /// <summary>
    /// Gets or sets whether live entry notifications are paused.
    /// </summary>
    public bool Pause
    {
        get => _pause;
        set
        {
            if (_pause == value)
            {
                return;
            }

            _pause = value;
            OnDisplaySettingsChanged();
        }
    }

    /// <summary>
    /// Gets the configured log directory.
    /// </summary>
    public string LogDirectory => _logDirectory;

    /// <summary>
    /// Reinitializes the logger for the specified directory.
    /// </summary>
    /// <param name="directory">The target directory for persisted log files.</param>
    public void InitializeLog(string directory)
    {
        ValidateNotNullOrWhiteSpace(directory, nameof(directory));
        SetLogDirectory(directory);

        string logFilePath = System.IO.Path.Combine(directory, "process-.log");
        if (_log is IDisposable disposableLog)
        {
            disposableLog.Dispose();
        }

            _log = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                path: logFilePath,
                fileSizeLimitBytes: 10 * 1024 * 1024,
                rollOnFileSizeLimit: true,
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Sink(new ProcessLogSink(processLog: this))
            .CreateLogger();
        }
        

    /// <summary>
    /// Updates the directory used for persisted log files.
    /// </summary>
    /// <param name="directory">The target directory for persisted log files.</param>
    public void SetLogDirectory(string directory)
    {
        ValidateNotNullOrWhiteSpace(directory, nameof(directory));
        Directory.CreateDirectory(directory);
        _logDirectory = directory;
    }

    /// <summary>
    /// Opens the current log directory in Windows Explorer.
    /// </summary>
    public void OpenLogDirectory()
    {
        if (string.IsNullOrWhiteSpace(_logDirectory))
        {
            return;
        }

        Directory.CreateDirectory(_logDirectory);
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = _logDirectory,
            UseShellExecute = true
        });
    }

    public void Error(string sender, string message, Exception exception) =>
        WriteWithContext(level: LogEventLevel.Error, sender: sender, message: message, exception: exception);

    public void Fatal(string sender, string message, Exception exception) =>
        WriteWithContext(level: LogEventLevel.Fatal, sender: sender, message: message, exception: exception);

    public void Info(string sender, string message) =>
        WriteWithContext(level: LogEventLevel.Information, sender: sender, message: message);

    public void Debug(string sender, string message) =>
        WriteWithContext(level: LogEventLevel.Debug, sender: sender, message: message);

    public void Warning(string sender, string message) =>
        WriteWithContext(level: LogEventLevel.Warning, sender: sender, message: message);

    /// <summary>
    /// Writes a log entry by string level.
    /// </summary>
    public void Write(string sender, string level, string message)
    {
        switch (level)
        {
            case "Debug":
                Debug(sender: sender, message: message);
                break;
            case "Info":
                Info(sender: sender, message: message);
                break;
            case "Warning":
                Warning(sender: sender, message: message);
                break;
            case "Error":
                Error(sender: sender, message: message, exception: null);
                break;
            case "Fatal":
                Fatal(sender: sender, message: message, exception: null);
                break;
            default:
                Info(sender: sender, message: message);
                break;
        }
    }

    public void WriteEntry(string sender, LogEventLevel level, string message)
    {
        WriteWithContext(level: level, sender: sender, message: message);
    }

    public DataTable GetBufferedLogs(string levelFilter = null, string textFilter = null)
    {
        lock (_bufferLock)
        {
            DataTable result = _bufferTable.Clone();

            foreach (DataRow row in _bufferTable.Rows)
            {
                string level = row["Level"]?.ToString() ?? string.Empty;
                string message = row["Message"]?.ToString() ?? string.Empty;

                if (TryParseLevel(level: level, parsedLevel: out LogEventLevel parsedLevel) && !IsLevelVisible(parsedLevel))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(levelFilter) && !string.Equals(level, levelFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string context = row["Context"]?.ToString() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(textFilter)
                    && message.IndexOf(textFilter, StringComparison.OrdinalIgnoreCase) < 0
                    && context.IndexOf(textFilter, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                result.ImportRow(row);
            }

            return result;
        }
    }

    public IReadOnlyList<ProcessLogEntry> GetEntries(string levelFilter = null, string textFilter = null)
    {
        DataTable table = GetBufferedLogs(levelFilter: levelFilter, textFilter: textFilter);
        return table.Rows
            .Cast<DataRow>()
            .Select(row => new ProcessLogEntry(
                row["Timestamp"] is DateTime timestamp ? timestamp : DateTime.MinValue,
                row["Owner"]?.ToString() ?? string.Empty,
                row["Sender"]?.ToString() ?? string.Empty,
                row["Context"]?.ToString() ?? string.Empty,
                row["Level"]?.ToString() ?? string.Empty,
                row["Message"]?.ToString() ?? string.Empty,
                row["Exception"]?.ToString() ?? string.Empty))
            .ToList();
    }

    public bool IsLevelVisible(LogEventLevel level)
    {
        switch (level)
        {
            case LogEventLevel.Verbose:
                return true;
            case LogEventLevel.Debug:
                return ShowDebug;
            case LogEventLevel.Information:
                return ShowInfo;
            case LogEventLevel.Warning:
                return ShowWarning;
            case LogEventLevel.Error:
                return ShowError;
            case LogEventLevel.Fatal:
                return ShowFatal;
            default:
                return true;
        }
    }

    internal void AddBufferedEntry(LogEvent logEvent, string message)
    {
        string owner = TryGetPropertyValue(logEvent: logEvent, propertyName: OwnerPropertyName) ?? Owner;
        string sender = TryGetPropertyValue(logEvent: logEvent, propertyName: SenderPropertyName) ?? owner;
        string context = TryGetPropertyValue(logEvent: logEvent, propertyName: ContextPropertyName) ?? BuildContext(owner: owner, sender: sender);
        string entryMessage = TryGetPropertyValue(logEvent: logEvent, propertyName: MessagePropertyName) ?? message;

        ProcessLogEntry entry = new ProcessLogEntry(
            logEvent.Timestamp.LocalDateTime,
            owner,
            sender,
            context,
            logEvent.Level.ToString(),
            entryMessage,
            logEvent.Exception?.ToString() ?? string.Empty);

        AddEntry(entry: entry, level: logEvent.Level);
    }

    private void AddEntry(ProcessLogEntry entry, LogEventLevel level)
    {
        lock (_bufferLock)
        {
            _bufferTable.Rows.Add(entry.Timestamp, entry.Owner, entry.Sender, entry.Context, entry.Level, entry.Message, entry.Exception);

            while (_bufferTable.Rows.Count > MaxBufferedRows)
            {
                _bufferTable.Rows.RemoveAt(0);
            }
        }

        if (!Pause && IsLevelVisible(level))
        {
            EntryAdded?.Invoke(entry);
        }
    }

    private void OnDisplaySettingsChanged()
    {
        DisplaySettingsChanged?.Invoke();
    }

    private static bool TryParseLevel(string level, out LogEventLevel parsedLevel)
    {
        return Enum.TryParse(level, true, out parsedLevel);
    }

    private void WriteWithContext(
        LogEventLevel level,
        string sender,
        string message,
        Exception exception = null)
    {
        ValidateNotNullOrWhiteSpace(sender, nameof(sender));

        string context = BuildContext(owner: Owner, sender: sender);

        Log.Write(
            level: level,
            exception: exception,
            messageTemplate: "{ProcessLogContext:l} {ProcessLogMessage:l}",
            propertyValues: new object[]
            {
                context,
                message
            });
    }

    private static string BuildContext(string owner, string sender)
    {
        if (string.IsNullOrWhiteSpace(sender) || string.Equals(owner, sender, StringComparison.OrdinalIgnoreCase))
        {
            return $"[{owner}]";
        }

        return $"[{owner}:{sender}]";
    }

    private static string TryGetPropertyValue(LogEvent logEvent, string propertyName)
    {
        LogEventPropertyValue value;
        if (!logEvent.Properties.TryGetValue(propertyName, out value))
        {
            return null;
        }

        ScalarValue scalarValue = value as ScalarValue;
        if (scalarValue != null)
        {
            return scalarValue.Value != null ? scalarValue.Value.ToString() : null;
        }

        return value.ToString();
    }

    private static DataTable CreateBufferTable()
    {
        DataTable table = new DataTable("ProcessLogBuffer");
        table.Columns.Add("Timestamp", typeof(DateTime));
        table.Columns.Add("Owner", typeof(string));
        table.Columns.Add("Sender", typeof(string));
        table.Columns.Add("Context", typeof(string));
        table.Columns.Add("Level", typeof(string));
        table.Columns.Add("Message", typeof(string));
        table.Columns.Add("Exception", typeof(string));
        return table;
    }

    private static void ValidateNotNullOrWhiteSpace(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", parameterName);
        }
    }
}

/// <summary>
/// Represents a buffered process log entry.
/// </summary>
public sealed class ProcessLogEntry
{
    public ProcessLogEntry(DateTime timestamp, string owner, string sender, string context, string level, string message, string exception)
    {
        Timestamp = timestamp;
        Owner = owner;
        Sender = sender;
        Context = context;
        Level = level;
        Message = message;
        Exception = exception;
    }

    public DateTime Timestamp { get; private set; }

    public string Owner { get; private set; }

    public string Sender { get; private set; }

    public string Context { get; private set; }

    public string Level { get; private set; }

    public string Message { get; private set; }

    public string Exception { get; private set; }
}

/// <summary>
/// Forwards Serilog events into the <see cref="ProcessLog"/> buffer.
/// </summary>
public sealed class ProcessLogSink : ILogEventSink
{
    private readonly IFormatProvider _formatProvider;
    private readonly ProcessLog _processLog;

    /// <summary>
    /// Initializes a new <see cref="ProcessLogSink"/> instance.
    /// </summary>
    public ProcessLogSink(ProcessLog processLog, IFormatProvider formatProvider = null)
    {
        _processLog = processLog;
        _formatProvider = formatProvider;
    }

    /// <summary>
    /// Emits a Serilog event into the process log buffer.
    /// </summary>
    public void Emit(LogEvent logEvent)
    {
        string message = logEvent.RenderMessage(_formatProvider);
        _processLog.AddBufferedEntry(logEvent: logEvent, message: message);
    }
}
}
