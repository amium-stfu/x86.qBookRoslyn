using CSScripting;
using Newtonsoft.Json.Schema;
using QB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ScintillaNET.Style;

namespace qbook.Studio
{
    internal static class RuntimeWatchdog
    {
      
        private static Task monitoringTask;

        static int ErrorCounter = 0;

        private static System.Threading.CancellationTokenSource cancellation;

        public static void Stop()
        {
            cancellation?.Cancel();
            if (monitoringTask != null && !monitoringTask.Wait(2000))
                throw new InvalidOperationException("The runtime watchdog has not stopped.");
            monitoringTask = null;
            cancellation?.Dispose();
            cancellation = null;
        }
        public static void Start()
        {
            ErrorCounter = GlobalExceptions.ErrorCounter;
            Stop();
            cancellation = new System.Threading.CancellationTokenSource();
            var token = cancellation.Token;

            monitoringTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    //Debug.WriteLine("E " + GlobalExceptions.ErrorCounter);
                    try { await Task.Delay(1000, token); }
                    catch (OperationCanceledException) when (token.IsCancellationRequested) { return; }
                    if (ErrorCounter != GlobalExceptions.ErrorCounter)
                    {
                        List<string> errors = null;

                        foreach (GlobalExceptions.RuntimeError error in GlobalExceptions.Errors.Values)
                        {
                            errors = new List<string>();
                            string json = System.Text.Json.JsonSerializer.Serialize(error);
                            errors.Add(json);
                        }
                        ErrorCounter = GlobalExceptions.ErrorCounter;
                        Core.SendToEditor("RuntimeErrors", args: errors.ToArray());
                    }
                }
            });
        }
    }

}
