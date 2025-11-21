using log4net;
using log4net.Appender;
using log4net.Layout;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using QB;
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
       

        private static Type? _programType;
        public static bool IsRuntimeReady => _programType != null && qbook.Core.ActiveCsAssembly != null;
        public static void BindAllPagesToAssembly(Assembly asm)
        {
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
            }
        }
        public static void EnsureInit(oPage page)
        {
            if (page?.DynInstance == null || page.DynInitialized) return;
            page.DynInstance.GetType().GetMethod("Init")?.Invoke(page.DynInstance, null);
            page.DynInitialized = true;
        }
        public static void ExecuteRender(oPage page)
        {
            if (page?.DynInstance == null) return;
            page.DynInstance.GetType().GetMethod("Render")?.Invoke(page.DynInstance, null);
        }
        public static void InitializeAll()
        {
            GlobalExceptions.InitRuntimeErrors();
            Debug.WriteLine("InitializeAll");
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
                }
                catch (Exception ex)
                {
                    QB.GlobalExceptions.LogRichException("Program.Initialize (global)", ex);
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
                    ExceptionBridge.Safe("Program.Run", () =>
                    {
                        runGlobal.Invoke(null, null);
                    }, rethrow: true);
                }
                catch (Exception ex)
                {
                    GlobalExceptions.Handle(ex, "Program.Run Reflection");
                }

            }

        }
        public static void DestroyAll()
        {

            if (Core.ThisBook == null) return;
        

            QB.Logger.Debug("=== PageRuntime.DestroyAll() start ===");

            // 1️⃣ Altes Script-System stoppen (CSScript)
            try
            {
                Core.CsScript_Destroy();
                QB.Logger.Debug("Core.CsScript_Destroy() executed successfully.");
                RuntimeWatchdog.Stop();
            }
            catch (Exception ex)
            {
                QB.Logger.Error($"Core.CsScript_Destroy() failed: {ex.Message}");
            }

            // 2️⃣ Wenn QB.Program existiert, dessen statische Destroy() aufrufen
            try
            {
                _programType?.GetMethod("Destroy", BindingFlags.Public | BindingFlags.Static)
                            ?.Invoke(null, null);
                QB.Logger.Debug("QB.Program.Destroy() executed.");
            }
            catch (Exception ex)
            {
                QB.Logger.Debug($"QB.Program.Destroy() failed: {ex.Message}");
            }

        

            // 3️⃣ Fallback: Alle Klassen im Script-Assembly nach Destroy() durchsuchen
            try
            {
                var asm = qbook.Core.ActiveCsAssembly;
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

    }
}
