#nullable enable annotations

using log4net;
using log4net.Appender;
using log4net.Layout;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using QB;
using QB.Controls;
using qbook.Studio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static IronPython.Modules._ast;

namespace qbook
{
    public static class BookRuntime
    {
       
        private static readonly HashSet<string> _loggedInitPages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> _loggedRenderPages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static MethodInfo? ResolveInitMethod(Type pageType)
        {
            return pageType.GetMethod("Init") ?? pageType.GetMethod("Initialize");
        }

        private static MethodInfo? ResolveRenderMethod(Type pageType)
        {
            return pageType.GetMethod("Render") ?? pageType.GetMethod("Run");
        }

        private static Type? _programType;
        private static bool _resourcesInitialized;
        private static bool _resourcesReleased;
        private static bool _resourceCycleFailed;
        public static bool IsRuntimeReady => _programType != null && qbook.Core.ActiveCsAssembly != null;
        public static void BindAllPagesToAssembly(Assembly asm)
        {
            Core.SendToEditor("LogInfo", "[HOST] <<<<<< Starting Assembly >>>>>>");
            Core.SendToEditor("LogInfo", "[HOST] Project ID             " + Core.Roslyn.GetProjectId);
            Core.SendToEditor("LogInfo", "[HOST] Assembly Name          " + asm.FullName);
            Core.SendToEditor("LogInfo", "[HOST] LoadedAssemblyMvid     " + asm.ManifestModule.ModuleVersionId);
            Core.SendToEditor("LogInfo", "[HOST] LoadedAssemblyLocation " + (string.IsNullOrEmpty(asm.Location) ? "<In-Memory>" : asm.Location));
            Core.SendToEditor("LogInfo", "[HOST] LoadedTimestamp        " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

    
            _resourcesInitialized = false;
            _resourcesReleased = false;
            _resourceCycleFailed = false;
            _programType = asm.GetType("QB.Program");
            if (_programType == null)
            {
                QB.Logger.Warn("QB.Program not found in compiled assembly.");
                return;
            }

            foreach (var page in qbook.Core.ThisBook.Main.Objects.OfType<oPage>())
            {
                var prop = _programType.GetProperty(page.Name);
                page.DynInstance = prop?.GetValue(null); // static property → null target ok
                page.DynInitialized = false;

                var dynType = page.DynInstance?.GetType();
                var initMethod = dynType == null ? null : ResolveInitMethod(dynType);
                var renderMethod = dynType == null ? null : ResolveRenderMethod(dynType);
                Core.SendToEditor(
                    "LogInfo",
                    "[HOST] PageBind page=" + page.Name +
                    " hasDyn=" + (page.DynInstance != null) +
                    " type=" + (dynType?.FullName ?? "<null>") +
                    " initMethod=" + (initMethod?.Name ?? "<none>") +
                    " renderMethod=" + (renderMethod?.Name ?? "<none>"));
            }
        }
        public static void EnsureInit(oPage page)
        {
            if (page?.DynInstance == null || page.DynInitialized) return;
            var initMethod = ResolveInitMethod(page.DynInstance.GetType());
            if (_loggedInitPages.Add(page.Name))
            {
                Core.SendToEditor("LogInfo", "[HOST] EnsureInit page=" + page.Name + " type=" + page.DynInstance.GetType().FullName + " method=" + (initMethod?.Name ?? "<none>"));
            }
            if (initMethod == null)
            {
                Core.SendToEditor("LogInfo", "[HOST] EnsureInit SKIP page=" + page.Name + " reason=no-init-method");
                return;
            }
            initMethod.Invoke(page.DynInstance, null);
            page.DynInitialized = true;
        }
        public static void ExecuteRender(oPage page)
        {
            if (page?.DynInstance == null) return;
            var renderMethod = ResolveRenderMethod(page.DynInstance.GetType());
            if (_loggedRenderPages.Add(page.Name))
            {
                Core.SendToEditor("LogInfo", "[HOST] ExecuteRender page=" + page.Name + " type=" + page.DynInstance.GetType().FullName + " method=" + (renderMethod?.Name ?? "<none>"));
            }
            if (renderMethod == null)
            {
                Core.SendToEditor("LogInfo", "[HOST] ExecuteRender SKIP page=" + page.Name + " reason=no-render-method");
                return;
            }
            renderMethod.Invoke(page.DynInstance, null);
        }
        public static bool InitializeAll()
        {
            _resourcesInitialized = false;
            _resourceCycleFailed = true;
            try
            {
                GlobalExceptions.InitRuntimeErrors();
                if (!BuildSuccess || !IsRuntimeReady)
                    throw new InvalidOperationException("Runtime assembly missing or Program type not bound. BuildResult=" + BuildResult);

                MethodInfo initGlobal = _programType.GetMethod("Initialize");
                if (initGlobal == null)
                    throw new MissingMethodException("QB.Program.Initialize was not found.");

                initGlobal.Invoke(null, null);
                RuntimeWatchdog.Start();
                _resourcesInitialized = true;
                _resourceCycleFailed = false;
                Core.StatusText = "Status:Rebuild";
                return true;
            }
            catch (TargetInvocationException tex)
            {
                QB.GlobalExceptions.LogRichException("Program.Initialize (global)", tex.InnerException ?? tex);
                Core.StatusText = "Alert:Rebuild";
            }
            catch (Exception ex)
            {
                QB.GlobalExceptions.LogRichException("Program.Initialize (global)", ex);
                Core.StatusText = "Alert:Rebuild";
            }
            return false;
        }
        public static bool RunAll()
        {
            try
            {
                if (!BuildSuccess || !IsRuntimeReady || !_resourcesInitialized || _resourcesReleased || _resourceCycleFailed)
                    throw new InvalidOperationException("Runtime is not successfully built and initialized. BuildResult=" + BuildResult);

                MethodInfo runGlobal = _programType.GetMethod("Run");
                if (runGlobal == null)
                    throw new MissingMethodException("QB.Program.Run was not found.");

                QB.Root.ActiveQbook = qbook.Core.ThisBook;
                Core.SendToEditor("LogInfo", "[HOST] Program.Run START");
                ExceptionBridge.Safe("Program.Run", () =>
                {
                    runGlobal.Invoke(null, null);
                }, rethrow: true);
                Core.SendToEditor("LogInfo", "[HOST] Program.Run END");
                Core.StatusText = "Status:Run";
                return true;
            }
            catch (Exception ex)
            {
                GlobalExceptions.Handle(ex, "Program.Run Reflection");
                Core.StatusText = "Alert:Run";
                return false;
            }
        }
        public static void StopAll()
        {
            try
            {
                Core.PublishStopStatus("Progress:Stop");
                DestroyAll();
                Core.PublishStopStatus("Status:Stop");
            }
            catch (Exception ex)
            {
                _resourceCycleFailed = true;
                QB.GlobalExceptions.LogRichException("BookRuntime.StopAll", ex);
                Core.PublishStopStatus("Alert:Stop", "Runtime cleanup or UI finalization failed: " + ex.Message);
                throw;
            }
        }

        private static void ReleaseResources()
        {
            if (_resourcesReleased) return;
            _resourcesInitialized = false;
            _resourceCycleFailed = true;
            try
            {
                RuntimeWatchdog.Stop();
                var destroy = _programType?.GetMethod("Destroy", BindingFlags.Public | BindingFlags.Static);
                if (destroy != null) destroy.Invoke(null, null);
                else if (Core.ThisBook != null)
                {
                    foreach (var page in Core.ThisBook.Main.Objects.OfType<oPage>())
                        page.DynInstance?.GetType().GetMethod("Destroy", Type.EmptyTypes)?.Invoke(page.DynInstance, null);
                }
                ItemDestroyHandler.DestroyAll();
                foreach (var control in QB.Root.ControlDict.Values.ToArray()) control.Remove();
                QB.Root.ResetAllDicts();
                if (Core.ThisBook != null)
                {
                    foreach (var page in Core.ThisBook.Main.Objects.OfType<oPage>())
                        page.DynInitialized = false;
                }
                _loggedInitPages.Clear();
                _loggedRenderPages.Clear();
                _resourcesReleased = true;
                _resourceCycleFailed = false;
            }
            catch (Exception ex)
            {
                Core.StatusText = "Alert:Stop";
                QB.Logger.Error("Book stop failed; reload cancelled: " + ex.Message);
                throw;
            }
        }

        public static void DestroyAll()
        {
            MainForm form = MainForm.Instance;
            PageControl surface = null;
            try
            {
                surface = form?.BeginBookTeardown();
                ReleaseResources();
                if (Core.ThisBook != null)
                {
                    foreach (var page in Core.ThisBook.Main.Objects.OfType<oPage>())
                    {
                        page.DynInstance = null;
                        page.DynInitialized = false;
                    }
                }
                _programType = null;
                Core.ActiveCsAssembly = null;
                _resourcesInitialized = false;
                BuildSuccess = false;
                _loggedInitPages.Clear();
                _loggedRenderPages.Clear();
                Core.Roslyn?.UnloadRuntimeAssembly();
            }
            finally
            {
                form?.EndBookTeardown(surface);
            }
            _resourceCycleFailed = false;
            Core.StatusText = "Status:Stop";
        }
        public static List<string> ErrorFiles = new List<string>();
        static Stopwatch buildWatch = new Stopwatch();
        public static int BuildDuration = 0;
        public static string BuildResult = "";
        public static bool BuildSuccess = false;

        public static async Task BuildAssembly()
        {
            BuildSuccess = false;
            BuildResult = "[Rebuild] Build failed.";
            DestroyAll();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GlobalExceptions.InitRuntimeErrors();

            BuildDuration = 0;
            buildWatch.Restart();
            ErrorFiles.Clear();

            // Assembly bauen
            var asm = await Core.Roslyn.BuildAssemblyAsync();
            if (asm == null)
            {
                ErrorFiles = Core.Roslyn.ErrorFiles;
                BuildSuccess = false;
                BuildResult = "[Rebuild] Build failed.";
                return;
            }
            QB.Root.ActiveQbook = qbook.Core.ThisBook;
            qbook.Core.ActiveCsAssembly = asm;
            BindAllPagesToAssembly(asm);
            if (!IsRuntimeReady)
            {
                DestroyAll();
                return;
            }
            BuildSuccess = true;
            BuildDuration = (int)buildWatch.ElapsedMilliseconds;
            BuildResult = $"[Rebuild] Build success ({BuildDuration}ms)";

        }
        public static async Task BuildBookAssembly()
        {
            BuildSuccess = false;
            BuildResult = "[Rebuild] Build failed.";
            BuildDuration = 0;
            buildWatch.Restart();
            ErrorFiles.Clear();

            // Alte Runtime zerstören
            BookRuntime.DestroyAll();
            qbook.Core.ActiveCsAssembly = null;

            // Dokumente neu laden
            var roslynFiles = CollectSourceFiles(); // deine Logik für Pages/SubCodes
            Core.Roslyn.ResetDocumentsOnly();
            await Core.Roslyn.ReloadDocumentsAsync(roslynFiles);

            // Assembly bauen
            var asm = await Core.Roslyn.BuildAssemblyAsync();
            if (asm == null)
            {
                ErrorFiles = Core.Roslyn.ErrorFiles;
                BuildSuccess = false;
                BuildResult = "[Rebuild] Build failed.";
                return;
            }

            qbook.Core.ActiveCsAssembly = asm;
            BindAllPagesToAssembly(asm);
            if (!IsRuntimeReady)
            {
                DestroyAll();
                return;
            }
            BuildSuccess = true;
            BuildDuration = (int)buildWatch.ElapsedMilliseconds;
            BuildResult = $"[Rebuild] Build success ({BuildDuration}ms)";
        }
        private static List<(string fileName, string code)> CollectSourceFiles()
        {
            var roslynFiles = new List<(string fileName, string code)>();
            foreach (oPage page in qbook.Core.ThisBook.Main.Objects.OfType<oPage>())
            {
                roslynFiles.Add((page.Filename, page.Code));
                foreach (var sub in page.SubCodes.Values)
                {
                    if (sub.Active)
                    {
                        roslynFiles.Add((sub.Filename, sub.Code));
                    }
                }
            }
            // Füge hier ggf. weitere Dateien wie Program.cs hinzu
            return roslynFiles;
        }
//        public static void EnsureVsCodeLaunchJson(string projectFolder)
//        {
//            var vscodeDir = Path.Combine(projectFolder, ".vscode");
//            Directory.CreateDirectory(vscodeDir);

//            var launchPath = Path.Combine(vscodeDir, "launch.json");
//            if (File.Exists(launchPath))
//                return; // nicht überschreiben, falls der User es angepasst hat

//            var symbolPath = Path.Combine(Path.GetTempPath(), "qbookRoslyn");
//            var jsonSymbolPath = symbolPath.Replace("\\", "\\\\");

//            var json = $@"{{
//  ""version"": ""0.2.0"",
//  ""configurations"": [
//    {{
//      ""name"": ""Attach to qBook Host (auto)"",
//      ""type"": ""coreclr"",
//      ""request"": ""attach"",
//      ""processId"": ""{command:qbook.getHostProcessId}"",
//      ""justMyCode"": false,
//      ""symbolOptions"": {{
//        ""searchPaths"": [
//          ""{jsonSymbolPath}""
//        ]
//      }}
//    }}
//  ]
//}}";

//            File.WriteAllText(launchPath, json, Encoding.UTF8);
//        }
    }
}
