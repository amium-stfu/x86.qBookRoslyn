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
        public static bool IsRuntimeReady => _programType != null && qbook.Core.ActiveCsAssembly != null;
        public static void BindAllPagesToAssembly(Assembly asm)
        {
            Core.SendToEditor("LogInfo", "[HOST] <<<<<< Starting Assembly >>>>>>");
            Core.SendToEditor("LogInfo", "[HOST] Project ID             " + Core.Roslyn.GetProjectId);
            Core.SendToEditor("LogInfo", "[HOST] Assembly Name          " + asm.FullName);
            Core.SendToEditor("LogInfo", "[HOST] LoadedAssemblyMvid     " + asm.ManifestModule.ModuleVersionId);
            Core.SendToEditor("LogInfo", "[HOST] LoadedAssemblyLocation " + (string.IsNullOrEmpty(asm.Location) ? "<In-Memory>" : asm.Location));
            Core.SendToEditor("LogInfo", "[HOST] LoadedTimestamp        " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

    
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
        public static void InitializeAll()
        {
            Core.StatusText = "Status:Rebuild";
            GlobalExceptions.InitRuntimeErrors();
            var proj = Core.Roslyn.GetCurrentProject();
            Debug.WriteLine("[Diag] Id  =" + proj?.Id);
            Debug.WriteLine("[Diag] Docs=" + proj?.Documents.Count());
            Debug.WriteLine("[Diag] Has Program.cs=" + (proj?.Documents.Any(d => d.Name == "Program.cs")));

            if (!IsRuntimeReady)
            {
                QB.Logger.Error("[BookRuntime] InitializeAll aborted: assembly missing or Program type not bound. BuildResult=" + BuildResult);
                Debug.WriteLine("[BookRuntime] InitializeAll aborted: assembly missing or Program type not bound. BuildResult=" + BuildResult);
                return;
            }
          
            // Option 1: Falls du die generierte Program.Initialize() trotzdem zuerst ausführen willst:
            MethodInfo initGlobal = _programType.GetMethod("Initialize");
            if (initGlobal != null)
            {
                try
                {
                    initGlobal.Invoke(null, null);
                    RuntimeWatchdog.Start();
                   
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
            }
        }
        public static void RunAll()
        {
            if (!IsRuntimeReady)
            {
                QB.Logger.Error("[PageRuntime] RunAll aborted: runtime not ready. BuildResult=" + BuildResult);
                Debug.WriteLine("[PageRuntime] RunAll aborted: runtime not ready. BuildResult=" + BuildResult);
                return;
            }
            QB.Root.ActiveQbook = qbook.Core.ThisBook;
            MethodInfo runGlobal = _programType.GetMethod("Run");
            if (runGlobal != null)
            {

                try
                {
                    Core.SendToEditor("LogInfo", "[HOST] Program.Run START");
                    ExceptionBridge.Safe("Program.Run", () =>
                    {
                        runGlobal.Invoke(null, null);
                    }, rethrow: true);
                    Core.SendToEditor("LogInfo", "[HOST] Program.Run END");
                
                    Core.StatusText = "Status:Run";
                }
                catch (Exception ex)
                {
                    GlobalExceptions.Handle(ex, "Program.Run Reflection");
                    Core.StatusText = "Alert:Run";
                }

            }

        }
        public static void DestroyAll()
        {

            _loggedInitPages.Clear();
            _loggedRenderPages.Clear();

            if (Core.ThisBook == null) return;

            Core.StatusText = "Status:Stop";
            QB.Logger.Debug("=== PageRuntime.DestroyAll() start ===");

            // 1️⃣ Altes Script-System stoppen (CSScript)
            //try
            //{
            //    Core.CsScript_Destroy();
            //    QB.Logger.Debug("Core.CsScript_Destroy() executed successfully.");
            //    RuntimeWatchdog.Stop();
            //    Core.Roslyn?.ClearBuildArtifacts();
            //}
            //catch (Exception ex)
            //{
            //    QB.Logger.Error($"Core.CsScript_Destroy() failed: {ex.Message}");
            //    Core.StatusText = "Alert:Stop";
            //}

            //// 2️⃣ Wenn QB.Program existiert, dessen statische Destroy() aufrufen
            try
            {
                _programType?.GetMethod("Destroy", BindingFlags.Public | BindingFlags.Static)
                            ?.Invoke(null, null);
                QB.Logger.Debug("QB.Program.Destroy() executed.");
            }
            catch (Exception ex)
            {
                QB.Logger.Debug($"QB.Program.Destroy() failed: {ex.Message}");
                Core.StatusText = "Alert:Stop";
            }



            // 3️⃣ Fallback: Alle Klassen im Script-Assembly nach Destroy() durchsuchen
            try
            {
                var asm = qbook.Core.ActiveCsAssembly;

                ItemDestroyHandler.DestroyAll(); // Alle registrierten Destroyer aufrufen (z.B. von Controls)
                if (asm != null)
                {
                    foreach (var type in asm.GetTypes())
                    {
                        // Nur konkrete, nicht-generische Typen mit parameterloser Destroy-Methode
                        var destroyMethod = type.GetMethod("Destroy",
                            BindingFlags.Public | BindingFlags.Instance,
                            Type.DefaultBinder,
                            Type.EmptyTypes,
                            null);

                        if (destroyMethod == null)
                            continue;

                        object? instance = null;

                        try
                        {
                            // 🧩 versuchen, vorhandene Instanz aus QB.Program zu holen
                            var prop = _programType?
                                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                                .FirstOrDefault(p => p.PropertyType == type);
                            if (prop != null)
                                instance = prop.GetValue(null);

                            // 🧩 andernfalls selbst erzeugen (nur wenn erlaubt)
                            if (instance == null && !type.IsAbstract && type.GetConstructor(Type.EmptyTypes) != null)
                                instance = Activator.CreateInstance(type);
                        }
                        catch
                        {
                            continue; // wenn Instanzierung fehlschlägt, Typ überspringen
                        }

                        // 🧩 Destroy aufrufen
                        if (instance != null)
                        {
                            try
                            {
                                destroyMethod.Invoke(instance, null);
                                QB.Logger.Debug($"Destroyed instance: {type.FullName}");
                            }
                            catch (Exception ex)
                            {
                                QB.Logger.Error($"Destroy failed for {type.FullName}: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                QB.Logger.Warn($"Global Destroy scan failed: {ex.Message}");
            }

            // 4️⃣ QBook-Projektwurzel neu erzeugen
            try
            {
        
                QB.Logger.Info("Core.UpdateProjectAssemblyQbRoot() executed.");
            }
            catch (Exception ex)
            {
                QB.Logger.Warn($"UpdateProjectAssemblyQbRoot failed: {ex.Message}");
            }

            QB.Root.ResetAllDicts();
            QB.Logger.Info("=== PageRuntime.DestroyAll() completed ===");
        }

        private static int _buildVersion = 0;
        private static ProjectId? _lastProjectId;
        private static int _lastDocumentCount = 0;


        public static List<string> ErrorFiles = new List<string>();
        static Stopwatch buildWatch = new Stopwatch();
        public static int BuildDuration = 0;
        public static string BuildResult = "";
        public static bool BuildSuccess = false;

        public static async Task BuildAssembly()
        {
            DestroyAll();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GlobalExceptions.InitRuntimeErrors();
            
            BuildSuccess = true;
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
            BuildDuration = (int)buildWatch.ElapsedMilliseconds;
            BuildResult = $"[Rebuild] Build success ({BuildDuration}ms)";

        }
        public static async Task BuildBookAssembly()
        {
            BuildSuccess = true;
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
