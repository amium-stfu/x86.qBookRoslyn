using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Text.Json;

public class RuntimeError
{
    public string Key { get; set; }
    public string File { get; set; }

    public string Methode { get; set; }
    public int Line { get; set; }
    public int Col { get; set; }
    public string Exception { get; set; }
    public int Count { get; set; } = 1;
    internal uint epoch { get; set; }
    public int RepeatMs { get; set; } = 0;
    public int Length { get; set; }
    public string Snippet { get; set; }

}

internal static class RuntimeManager
{
    public static DataTable RuntimeErrors = new DataTable();
    private static readonly ConcurrentQueue<string[]> _errorQueue = new();

    public static void InitRuntimeErrors()
    {
        RuntimeErrors = new DataTable();
        RuntimeErrors.Columns.Add("Key", typeof(string));
        RuntimeErrors.Columns.Add("File", typeof(string));
        RuntimeErrors.Columns.Add("Methode", typeof(string));
        RuntimeErrors.Columns.Add("Line", typeof(int));
        RuntimeErrors.Columns.Add("Col", typeof(int));
        RuntimeErrors.Columns.Add("Reason", typeof(string));
        RuntimeErrors.Columns.Add("Count", typeof(int));
        RuntimeErrors.Columns.Add("RepeatMs", typeof(int));
        RuntimeErrors.PrimaryKey = new[] { RuntimeErrors.Columns["Key"] };

    }

    public static void Reset()
    {
        RuntimeErrors.Rows.Clear();
    }

    public static void EnqueueCommand(string[] errorData)
    {
        if (errorData == null) return;
        _errorQueue.Enqueue(errorData);

        Debug.WriteLine("E " + errorData.Count());

        // optional: sofort einen UI-Processing-Trigger schicken
        TriggerProcess();
    }

    // damit wir nicht 100x gleichzeitig processen:
    private static int _processRequested;

    private static void TriggerProcess()
    {
        // sorgt dafür, dass nur ein ProcessQueue-Lauf gleichzeitig geplant wird
        if (Interlocked.Exchange(ref _processRequested, 1) == 0)
        {
            UiDispatcher.Post(() =>
            {
                try
                {
                    ProcessQueueOnUiThread();
                }
                finally
                {
                    Interlocked.Exchange(ref _processRequested, 0);
                }
            });
        }
    }

    private static void ProcessQueueOnUiThread()
    {
        while (_errorQueue.TryDequeue(out var errorData))
        {
            RuntimeError error = null;
            foreach (string raw in errorData)
            {
                error = JsonSerializer.Deserialize<RuntimeError>(raw);

            }


            AddRuntimeError(error);  // hier sicher im UI-Thread
        }
    }

    private static void AddRuntimeError(RuntimeError err)
    {
        if (err == null) return;

        string key = err.Key;
        string reason = err.Exception;

        var row = RuntimeErrors.Rows.Find(key);
        if (row == null)
        {
            row = RuntimeErrors.NewRow();
            row["Key"] = err.Key;
            row["File"] = err.File;
            row["Methode"] = err.Methode;
            row["Line"] = err.Line;
            row["Col"] = err.Col;
            row["Reason"] = err.Exception;
            row["Count"] = err.Count;
            row["RepeatMs"] = err.RepeatMs;
            RuntimeErrors.Rows.Add(row);
        }
        else
        {
            row["Count"] = err.Count ;
            row["RepeatMs"] = err.RepeatMs;
        }

        Debug.WriteLine("Rows" + RuntimeErrors.Rows.Count);
    }
}

