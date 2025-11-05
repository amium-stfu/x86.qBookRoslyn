using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using QB;
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
            Root.ActiveQbook = Core.ThisBook;
            Debug.WriteLine("InitializeAll");

          
            var proj = Core.Roslyn.GetCurrentProject();
            Debug.WriteLine("[Diag] Id  =" + proj?.Id);
            Debug.WriteLine("[Diag] Docs=" + proj?.Documents.Count());
            Debug.WriteLine("[Diag] Has Program.cs=" + (proj?.Documents.Any(d => d.Name == "Program.cs")));

            if (!IsRuntimeReady)
            {
                QB.Logger.Error("[PageRuntime] InitializeAll aborted: assembly missing or Program type not bound. BuildResult=" + BuildResult);
                Debug.WriteLine("[PageRuntime] InitializeAll aborted: assembly missing or Program type not bound. BuildResult=" + BuildResult);
                return;
            }

            // Option 1: Falls du die generierte Program.Initialize() trotzdem zuerst ausführen willst:
            MethodInfo initGlobal = _programType.GetMethod("Initialize");
            if (initGlobal != null)
            {
                try
                {
                    initGlobal.Invoke(null, null);
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
                Core.UpdateProjectAssemblyQbRoot("PageRuntime.DestroyAll");
                QB.Logger.Info("Core.UpdateProjectAssemblyQbRoot() executed.");
            }
            catch (Exception ex)
            {
                QB.Logger.Warn($"UpdateProjectAssemblyQbRoot failed: {ex.Message}");
            }

            QB.Root.ResetWidgetDict();
            QB.Root.ResetObjectDict();

            QB.Logger.Info("=== PageRuntime.DestroyAll() completed ===");
        }


        private static int _buildVersion = 0;
        private static ProjectId? _lastProjectId;
        private static int _lastDocumentCount = 0;

        private static bool WorkspaceLooksValid()
        {
            var ws = Core.Roslyn?.GetWorkspace;
            var projId = Core.Roslyn?.GetProjectId;
            if (ws == null || projId == null) return false;
            var proj = ws.CurrentSolution.GetProject(projId);
            return proj != null && proj.Documents.Any(d => d.Name == "Program.cs");
        }


        public static List<string> ErrorFiles = new List<string>();

        static Stopwatch buildWatch = new Stopwatch();

        public static int BuildDuration = 0;
        public static string BuildResult = "";
        public static bool BuildSuccess = false;

        public static async Task BuildBookAssembly(bool rebuild = false)
        {
            BuildSuccess = true;
            BuildDuration = 0;
            buildWatch.Stop();
            buildWatch.Reset();
            buildWatch.Start();
            Debug.WriteLine("======== CreateAssembly ========");
            ErrorFiles.Clear();

            if (!rebuild)
            {
                Core.Roslyn.Reset();
                Core.Roslyn = new RoslynService();
                Core.Roslyn.CreateProject();
            }

            try
            {
                BuildResult = "[Rebuild] Destroying old runtime...";
                BookRuntime.DestroyAll();
                qbook.Core.ActiveCsAssembly = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Debug.WriteLine("[Rebuild] Old runtime destroyed.");
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[Rebuild] Destroy failed: {ex.Message}");
                buildWatch.Stop();
                BuildResult = "[Rebuild] Destroy failed";
                BuildSuccess = false;

            }

          

            BuildResult = "[Rebuild] Collecting source files...";
            var pages = new List<string>();
            var roslynFiles = new List<(string fileName, string code)>();
            var sbProgram = new StringBuilder();
            sbProgram.AppendLine("namespace QB");
            sbProgram.AppendLine("{");
            sbProgram.AppendLine("\tpublic static class Program");
            sbProgram.AppendLine("\t{");

            foreach (oPage page in qbook.Core.ActualMain.Objects.OfType<oPage>())
            {
                sbProgram.AppendLine($"\t\tpublic static Definition{page.Name}.qPage {page.Name} {{ get; }} = new Definition{page.Name}.qPage();");
                foreach (var htmlItem in page.HtmlItems)
                {
                    if (htmlItem.MyControl != null)
                    {
                        htmlItem.MyControl.Dispose();
                    }
                }

                pages.Add(page.Name);
                string code;
                if (rebuild)
                {
                    code = await Core.Roslyn.GetDocumentTextAsync(page.Filename);
                }
                else
                {
                    code = page.Code;
                }
       
                roslynFiles.Add((page.Filename, code));

                foreach (oCode sub in page.SubCodes.Values)
                {
                    if (sub.Active)
                    {
                        if (rebuild)
                        {
                            code = await Core.Roslyn.GetDocumentTextAsync(sub.Filename);
                        }
                        else
                        {
                            code = sub.Code;
                        }

                            roslynFiles.Add((sub.Filename, code));

                    }
                }
            }

            // 4️⃣ Methoden: Initialize / Run / Destroy
            sbProgram.AppendLine("\t\tpublic static void Initialize()");
            sbProgram.AppendLine("\t\t{");
            foreach (string p in pages)
                sbProgram.AppendLine($"\t\t\ttry {{ {p}.Initialize(); }} catch (System.Exception ex) {{ " +
                         $"QB.Logger.Error(\"Init failed for page '{p}': \" + ex.Message); throw; }}");
            sbProgram.AppendLine("\t\t}");

            sbProgram.AppendLine("\t\tpublic static void Run()");
            sbProgram.AppendLine("\t\t{");
            foreach (string p in pages)
                sbProgram.AppendLine($"\t\t\ttry {{ {p}.Run(); }} catch (System.Exception ex) {{ " +
                         $"QB.Logger.Error(\"Init failed for page '{p}': \" + ex.Message); throw; }}");
            sbProgram.AppendLine("\t\t}");

            sbProgram.AppendLine("\t\tpublic static void Destroy()");
            sbProgram.AppendLine("\t\t{");
            foreach (string p in pages)
                sbProgram.AppendLine($"\t\t\ttry {{ {p}.Destroy(); }} catch (System.Exception ex) {{ " +
                         $"QB.Logger.Error(\"Init failed for page '{p}': \" + ex.Message); throw; }}");
            sbProgram.AppendLine("\t\t}");

            sbProgram.AppendLine("\t}");
            sbProgram.AppendLine("}");

         //   Debug.WriteLine("========= Program.cs =========");
           // Debug.WriteLine(sbProgram.ToString());
         //   Debug.WriteLine("======== End of Program.cs ========");

            // 5️⃣ Dateien hinzufügen
            roslynFiles.Add(("Program.cs", sbProgram.ToString()));
            roslynFiles.Add(("GlobalUsing.cs", "global using static QB.Program;\r\n"));



            // 6️⃣ Referenzen aufbauen
            List<MetadataReference> references = new List<MetadataReference>();

            // Basisreferenzen aus dem laufenden .NET
            references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(System.Windows.Forms.Form).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(System.Drawing.Point).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly.Location));


            string netstandardPath = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "netstandard.dll");
            if (File.Exists(netstandardPath))
                references.Add(MetadataReference.CreateFromFile(netstandardPath));

            // Zusätzliche DLLs aus libs/, aber nur managed Assemblies
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs");
            if (Directory.Exists(baseDir))
            {
                foreach (string dllPath in Directory.GetFiles(baseDir, "*.dll"))
                {
                    try
                    {
                        using var fs = new FileStream(dllPath, FileMode.Open, FileAccess.Read);
                        using var pe = new System.Reflection.PortableExecutable.PEReader(fs);
                        if (!pe.HasMetadata)
                        {
                            //                Debug.WriteLine($"[Roslyn] Skip native DLL: {Path.GetFileName(dllPath)}");
                            continue;
                        }

                        references.Add(MetadataReference.CreateFromFile(dllPath));
                        //     Debug.WriteLine($"[Roslyn] +Reference: {Path.GetFileName(dllPath)}");
                    }
                    catch (System.Exception ex)
                    {
                        //         Debug.WriteLine($"[Roslyn] Skip invalid: {Path.GetFileName(dllPath)} ({ex.Message})");
                    }
                }
            }
            string runtimeDir = RuntimeEnvironment.GetRuntimeDirectory();
            string[] requiredAssemblies = new[]
            {
                "System.dll", // Basistypen wie Component
                "System.ComponentModel.Primitives.dll",
                "System.ComponentModel.TypeConverter.dll",
                "System.Runtime.dll",
                "System.Collections.dll",
                "System.Linq.dll",
                "System.Threading.dll"
            };

            foreach (string asmName in requiredAssemblies)
            {
                string asmPath = Path.Combine(runtimeDir, asmName);
                if (File.Exists(asmPath))
                {
                    try
                    {
                        references.Add(MetadataReference.CreateFromFile(asmPath));
                        //  Debug.WriteLine($"[Roslyn] +Reference: {asmName}");
                    }
                    catch (Exception ex)
                    {
                        QB.Logger.Error($"[Roslyn] Failed to add {asmName}: {ex.Message}");


                    }
                }
            }

            
            BuildResult = "[Rebuild] Resetting workspace...";
            if (rebuild)
            {
                Core.Roslyn.Reset();
                Core.Roslyn = new RoslynService();
                Core.Roslyn.CreateProject();
            }

            BuildResult = "[Rebuild] Loading project into workspace...";
            await Core.Roslyn.LoadInMemoryProjectAsync(roslynFiles, references);

            foreach (oPage page in qbook.Core.ActualMain.Objects.OfType<oPage>())
            {
                page.RoslynDoc = Core.Roslyn.GetDocumentByFilename(page.Filename);

                foreach (oCode sub in page.SubCodes.Values)
                {
                    if (sub.Active)
                        sub.RoslynDoc = Core.Roslyn.GetDocumentByFilename(sub.Filename);
                }
            }

            Core.ThisBook.Program = Core.Roslyn.GetDocumentByFilename("Program.cs");
            Core.ThisBook.Global = Core.Roslyn.GetDocumentByFilename("GlobalUsing.cs");

            BuildResult = "[Rebuild] Building assembly...";
            var asm = await Core.Roslyn.BuildAssemblyAsync();

            if (asm == null)
            {
                ErrorFiles = Core.Roslyn.ErrorFiles;
                BuildDuration = Core.Roslyn.BuildDuration;
                BuildSuccess = Core.Roslyn.BuildSuccess;

                BuildResult = "[Rebuild] Build failed.";
                BuildSuccess = false;
                QB.Logger.Error("[Build] Assembly build returned null. See previous diagnostics.");
                return;
            }

            buildWatch.Stop();
            qbook.Core.ActiveCsAssembly = asm;
            BindAllPagesToAssembly(asm);

            ErrorFiles = Core.Roslyn.ErrorFiles;
            BuildDuration = (int)buildWatch.ElapsedMilliseconds;
            BuildSuccess = Core.Roslyn.BuildSuccess;

            Debug.WriteLine("Errors   " + Core.Roslyn.ErrorFiles.Count);
            Debug.WriteLine("Duration " + Core.Roslyn.BuildDuration + "ms");
            Debug.WriteLine("Success  " + Core.Roslyn.BuildSuccess);

            BuildResult = $"[Rebuild] Build success ({BuildDuration}ms)";

        }

    }
}
