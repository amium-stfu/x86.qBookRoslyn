using CSScripting;
using QB.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace QB
{
    static class GlobalExceptions
    {
        private static int _timeoutCount = 0;
        public static void Init()
        {
            // Optional: FirstChance (nur für Diagnose, sonst sehr laut)
            // AppDomain.CurrentDomain.FirstChanceException += (s, e) => {
            //     if (e.Exception is TimeoutException)
            //         QB.Logger.Debug("[FirstChance] Timeout: " + e.Exception.Message);
            // };
        }

        public static void Handle(Exception ex, string source)
        {
            if (ex == null) return;
           // Debug.WriteLine($"Global catch:");
            // Timeout speziell behandeln
            if (ex is TimeoutException)
            {
                _timeoutCount++;
                QB.Logger.Warn($"[Timeout] ({_timeoutCount}) in {source}: {ex.Message}");
                SafeLogRich($"{source}/Timeout", ex);
                return; // Falls du trotzdem normal weiterloggst, entferne return
            }

            // TaskCanceledException oft harmlos
            if (ex is TaskCanceledException)
            {
                QB.Logger.Debug($"[TaskCanceled] in {source}: {ex.Message}");
                return;
            }

            // Restliche Fehler
           // QB.Logger.Error($"[EX] in {source}: {ex.GetType().Name} - {ex.Message}");
            SafeLogRich($"{source}", ex);
        }

        private static void SafeLogRich(string context, Exception ex)
        {
            try
            {
                LogException(context, ex);
            }
            catch
            {
                // Fallback falls Logger selbst wirft
                QB.Logger.Error("[EX] Logging failure: " + ex.Message);
            }
        }

        // Utility: zentral verwendbar für manuelles Einwickeln
        public static void SafeInvoke(string context, System.Action act)
        {
            try { act(); }
            catch (Exception ex) { Handle(ex, context); }
        }

        public static T SafeInvoke<T>(string context, Func<T> func)
        {
            try { return func(); }
            catch (Exception ex) { Handle(ex, context); return default; }
        }

        public static void LogException(string context, Exception ex) => LogRichException(context, ex);

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

            public RuntimeError(string file,string methode, string ex, int line, int col)
            {
                File = file.GetFileName();
                Methode = methode;
                Exception = ex;
                Line = line;
                Col = col;
                epoch = (uint)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                Key = $"{File}:{Line}:{Col}";
                AddError(this);
            }
        }
        public static DataTable RuntimeErrors = new DataTable();
        private static Dictionary<string, RuntimeError> _runtimeErrorDict = new Dictionary<string, RuntimeError>();

        private static readonly object _syncLock = new object();
        public static Dictionary<string, RuntimeError> Errors => _runtimeErrorDict;



        private static void AddError(RuntimeError err)
        {
            ErrorCounter++;
            if (_runtimeErrorDict.ContainsKey(err.Key))
            {
                _runtimeErrorDict[err.Key].Count++;
                uint now = (uint)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                _runtimeErrorDict[err.Key].RepeatMs = (int)(now - _runtimeErrorDict[err.Key].epoch);
                _runtimeErrorDict[err.Key].epoch = now;
            }
            else
            {
                _runtimeErrorDict.Add(err.Key, err);
            } 
        }

        public static int ErrorCounter = 0;
        public static void InitRuntimeErrors()
        {
            _runtimeErrorDict.Clear();
            RuntimeErrors.Clear();
            RuntimeErrors.Rows.Clear();
            ErrorCounter = 0;
            if (RuntimeErrors.Columns.Count > 0) return;
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

        public static void LogRichException(string context, Exception ex)
        {
            try
            {
                RuntimeError error;

                Debug.WriteLine("Log Error: " + context + " " + ex.Message);
                //var sb = new StringBuilder();
                //sb.AppendLine($"[EX] Context={context}");
                int depth = 0;
                string log = string.Empty;
                for (var cur = ex; cur != null; cur = cur.InnerException)
                {
                    //sb.AppendLine($"-- InnerLevel {depth} -- {cur.GetType().FullName}: {cur.Message}");
                    //if (cur.TargetSite != null)
                    //    sb.AppendLine($"   TargetSite: {cur.TargetSite.DeclaringType?.FullName}.{cur.TargetSite.Name}()");
                    //sb.AppendLine("   Stack:");
                    var st = new StackTrace(cur, true);
                    string name = $"{cur.TargetSite.DeclaringType?.FullName}.{cur.TargetSite.Name}()";
                    log = $"[EX] {name.Replace("Definition","")} -> {cur.GetType().FullName}: {cur.Message} ";
                    
                    QB.Logger.Error(log);


                    foreach (var frame in st.GetFrames() ?? Array.Empty<StackFrame>())
                    {
                        var method = frame.GetMethod();
                        var file = frame.GetFileName();
                        int line = frame.GetFileLineNumber();
                        int col = frame.GetFileColumnNumber();

                        if (frame.GetFileLineNumber() > 0)
                        {
                            new RuntimeError(file ?? "<unknown>",$"{cur.TargetSite.Name}()", $"{ cur.GetType().FullName }: { cur.Message}", line, col);
                            break; // Nur erste relevante Frame
                        }
                    }

                    //foreach (var frame in st.GetFrames() ?? Array.Empty<StackFrame>())
                    //{
                    //    var method = frame.GetMethod();
                    //    var file = frame.GetFileName();
                    //    int line = frame.GetFileLineNumber();
                    //    int col = frame.GetFileColumnNumber();

                    //    error = new RuntimeError(
                    //        file ?? "<unknown>",
                    //        cur.Message,
                    //        line,
                    //        col);

                    //    sb.Append("     at ");
                    //    sb.Append(method?.DeclaringType?.FullName);
                    //    sb.Append(".");
                    //    sb.Append(method?.Name);
                    //    sb.Append("(");
                    //    sb.Append(string.Join(", ",
                    //        method?.GetParameters()
                    //              .Select(p => p.ParameterType.Name + " " + p.Name) ?? Array.Empty<string>()));
                    //    sb.Append(")");
                    //    if (!string.IsNullOrEmpty(file))
                    //        sb.Append($" in {Path.GetFileName(file)}:{line}:{col}");
                    //    sb.AppendLine();
                    //    // Quellcode-Snippet (wenn Datei lokal existiert)
                    //    if (!string.IsNullOrEmpty(file) && File.Exists(file) && line > 0)
                    //    {
                    //        try
                    //        {
                    //            var allLines = File.ReadAllLines(file);
                    //            int start = Math.Max(0, line - 2);
                    //            int end = Math.Min(allLines.Length - 1, line + 1);
                    //            for (int i = start; i <= end; i++)
                    //            {
                    //                string marker = (i + 1 == line) ? ">>" : "  ";
                    //                sb.AppendLine($"       {marker} {i + 1:000}: {allLines[i]}");
                    //            }
                    //        }
                    //        catch { /* ignore snippet errors */ }
                    //    }
                    //}
                    depth++;
                }



               
            }
            catch (Exception logEx)
            {
                QB.Logger.Error("[EX] Failed building rich exception log: " + logEx.Message);
         
                
            
            }


        }

        public static void TryRich(string context, Exception ex, bool rethrow = false)
        {
            try
            {
                LogRichException(context, ex);
            }
            catch (Exception logEx)
            {
                Debug.WriteLine("[TryRich] Logging failure: " + logEx.Message);
            }

            if (rethrow)
                throw ex; // korrekt, da TryRich nicht in einem catch-Block steht
        }

    }
    public static class ExceptionBridge
    {
        /// <summary>
        /// Führt eine Aktion sicher aus, protokolliert Exceptions über GlobalExceptions,
        /// entpackt TargetInvocationExceptions und reicht sie optional weiter.
        /// </summary>
        public static void Safe(string context, System.Action action, bool rethrow = false)
        {
            try
            {
                action();
            }
            catch (TargetInvocationException tex)
            {
                Exception real = tex.InnerException ?? tex;

                GlobalExceptions.TryRich(context, real);

                if (rethrow)
                    throw; // wichtig: echtes Rethrow, StackTrace bleibt erhalten
            }
            catch (Exception ex)
            {
                GlobalExceptions.TryRich(context, ex);

                if (rethrow)
                    throw; // echtes Rethrow
            }
        }

        /// <summary>
        /// Wie oben, aber mit Rückgabewert. Gibt default(T) zurück, wenn rethrow=false.
        /// </summary>
        public static T Safe<T>(string context, Func<T> func, bool rethrow = false)
        {
            try
            {
                return func();
            }
            catch (TargetInvocationException tex)
            {
                Exception real = tex.InnerException ?? tex;

                GlobalExceptions.TryRich(context, real);

                if (rethrow)
                    throw;
            }
            catch (Exception ex)
            {
                GlobalExceptions.TryRich(context, ex);

                if (rethrow)
                    throw;
            }

            return default(T); // klassisch & kompatibel
        }
    }


}
