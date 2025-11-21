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

        static bool isRunning = false;

        public static void Stop()
        {
            isRunning = false;
            monitoringTask?.Wait(500);
            monitoringTask = null;
        }

        public static void Start()
        {
            ErrorCounter = GlobalExceptions.ErrorCounter;
            isRunning = true;

            monitoringTask = Task.Run(async () =>
            {
                while (isRunning)
                {
                    Debug.WriteLine("E " + GlobalExceptions.ErrorCounter);
                    await Task.Delay(1000);
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
