using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace qbook.Net
{
    public static class PipeCommands
    {

        private static async Task ExecuteLifecycle(string action, Func<Task> execute, Func<string> validate = null)
        {
            var watch = Stopwatch.StartNew();
            string failureMessage = action + " failed. See runtime logs for details.";
            Core.SendToEditor("LogInfo", "[HOST] " + action + " BEGIN");
            Core.SendToEditor("Progress:" + action, DateTime.Now.ToString(), "Lifecycle");
            try
            {
                string rejection = validate?.Invoke();
                if (rejection != null)
                {
                    failureMessage = rejection;
                    throw new InvalidOperationException(rejection);
                }
                await execute();
                Core.SendToEditor("LogInfo", "[HOST] " + action + " END duration=" + watch.ElapsedMilliseconds + "ms");
                Core.SendToEditor("Status:" + action, DateTime.Now.ToString(), "Lifecycle");
            }
            catch (Exception ex)
            {
                Core.SendToEditor("LogError", "[HOST] " + action + " FAILED duration=" + watch.ElapsedMilliseconds + "ms " + ex);
                QB.GlobalExceptions.LogRichException("PipeCommands." + action, ex);
                Core.SendToEditor("Alert:" + action, DateTime.Now.ToString(), failureMessage, "Lifecycle");
            }
        }

        public static Task Rebuild() => ExecuteLifecycle("Rebuild", async () =>
        {
            Core.PublishStopStatus("Status:Rebuild");
            string bookPath = Path.Combine(Core.ThisBook.Directory, Core.ThisBook.Filename);
            if (!await Core.OpenQbookAsync(bookPath))
                throw new InvalidOperationException("Rebuild " + Core.LastLoadStage + " failed: " + (Core.LastLoadError?.Message ?? BookRuntime.BuildResult), Core.LastLoadError);
        });

        public static Task Initialize() => ExecuteLifecycle("Initialize", () => Task.Run(() =>
        {
            if (BookRuntime.InitializeSucceeded)
                throw new InvalidOperationException("Initialize already succeeded. Stop before initializing again.");
            if (!BookRuntime.InitializeAll())
                throw new InvalidOperationException("Runtime initialization failed.", BookRuntime.LastRuntimeError);
        }));

        public static Task Run() => ExecuteLifecycle("Run", () => Task.Run(() =>
        {
            if (!BookRuntime.RunAll())
                throw new InvalidOperationException("Runtime Run failed.", BookRuntime.LastRuntimeError);
        }), validate: () =>
        {
            if (!BookRuntime.InitializeSucceeded || !BookRuntime.CanRenderRuntime)
                return "Run unavailable: Initialize first.";
            if (!BookRuntime.BuildSuccess || !BookRuntime.IsRuntimeReady)
                return "Run unavailable: Rebuild first.";
            if (BookRuntime.RunSucceeded)
                return "Run already succeeded. Stop before running again.";
            return null;
        });

        public static Task Stop() => ExecuteLifecycle("Stop", () => Task.Run(BookRuntime.DestroyProgram));

        public static async Task Destroy()
        {
            await Task.Run(() =>
            {
                try
                {
                    BookRuntime.DestroyAll();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PipeCommands.Destroy error: {ex.Message}");
                }
            });
            Core.PublishStopStatus("Status:Stop");
        }




        public static async Task PageText(PipeCommand command)
        {

            await Task.Run(() =>
            {
                try
                {
                    string pageName = command.Args[0];
                    string newText = command.Args[1];

                    foreach (oPage page in Core.ThisBook.Main.Objects)
                    {
                        if (page.Name == pageName)
                        {
                            page.Text = newText;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PipeCommands.PageText error: {ex.Message}");
                }
            });
        }

        public static async Task PageFormat(PipeCommand command)
        {

            await Task.Run(() =>
            {
                try
                {
                    string pageName = command.Args[0];
                    string newFormat = command.Args[1];

                    foreach (oPage page in Core.ThisBook.Main.Objects)
                    {
                        if (page.Name == pageName)
                        {
                            page.Format = newFormat;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PipeCommands.PageFormat error: {ex.Message}");
                }
            });
        }

        public static async Task HidePage(PipeCommand command)
        {
            Debug.WriteLine(command.Command + ": " + string.Join(",",command.Args));
            await Task.Run(() =>
            {
                try
                {
                    string pageName = command.Args[0];
                    string set = command.Args[1];

                    foreach (oPage page in Core.ThisBook.Main.Objects)
                    {
                        if (page.Name == pageName)
                        {
                            page.Hidden = set.ToLower() == "true" ? true : false;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PipeCommands.PageHiden error: {ex.Message}");
                }
            });
        }

        public static async Task PageOrder(PipeCommand command)
        {

            await Task.Run(() =>
            {
                try
                {
                    Core.ThisBook.PageOrder = command.Args.ToList();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PipeCommands.PageOrder error: {ex.Message}");
                }
            });
        }




    }
}
