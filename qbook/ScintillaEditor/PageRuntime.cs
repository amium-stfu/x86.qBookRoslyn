using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using QB;

namespace qbook.ScintillaEditor
{
    public static class PageRuntime
    {
        private static Type? _programType;

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

        // 🔧 globale Methoden (aufrufbar über statische QB.Program)
        public static void InitializeAll()
        {
            if (_programType == null)
            {
                Debug.WriteLine("[PageRuntime] InitializeAll: _programType is null – nothing to initialize.");
                return;
            }

            try
            {
                var initMethod = _programType.GetMethod("Initialize");
                if (initMethod == null)
                {
                    Debug.WriteLine("[PageRuntime] InitializeAll: No Initialize() method found in Program.");
                    return;
                }

                Debug.WriteLine($"[PageRuntime] Calling {_programType.FullName}.Initialize() ...");
                initMethod.Invoke(null, null);
                Debug.WriteLine($"[PageRuntime] InitializeAll completed successfully.");
            }
            catch (TargetInvocationException tex)
            {
                var inner = tex.InnerException;
                Debug.WriteLine("[PageRuntime] InitializeAll → Inner exception:");
                Debug.WriteLine(inner?.GetType().Name + ": " + inner?.Message);
                Debug.WriteLine(inner?.StackTrace);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PageRuntime] InitializeAll failed: {ex}");
            }
        }


        public static void RunAll()
        {
            if (_programType == null)
            {
                Debug.WriteLine("[PageRuntime] RunAll: _programType is null – nothing to run.");
                return;
            }

            try
            {
                var runMethod = _programType.GetMethod("Run");
                if (runMethod == null)
                {
                    Debug.WriteLine("[PageRuntime] RunAll: No Run() method found in Program.");
                    return;
                }

                Debug.WriteLine($"[PageRuntime] Calling {_programType.FullName}.Run() ...");
                runMethod.Invoke(null, null);
                Debug.WriteLine($"[PageRuntime] RunAll completed successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PageRuntime] RunAll failed: {ex.Message}");
            }
        }


        public static void DestroyAll()
        {

            if (Core.ThisBook == null) return;
        

            QB.Logger.Info("=== PageRuntime.DestroyAll() start ===");

            // 1️⃣ Altes Script-System stoppen (CSScript)
            try
            {
                Core.CsScript_Destroy();
                QB.Logger.Info("Core.CsScript_Destroy() executed successfully.");
            }
            catch (Exception ex)
            {
                QB.Logger.Warn($"Core.CsScript_Destroy() failed: {ex.Message}");
            }

            // 2️⃣ Wenn QB.Program existiert, dessen statische Destroy() aufrufen
            try
            {
                _programType?.GetMethod("Destroy", BindingFlags.Public | BindingFlags.Static)
                            ?.Invoke(null, null);
                QB.Logger.Info("QB.Program.Destroy() executed.");
            }
            catch (Exception ex)
            {
                QB.Logger.Warn($"QB.Program.Destroy() failed: {ex.Message}");
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
                                QB.Logger.Info($"Destroyed instance: {type.FullName}");
                            }
                            catch (Exception ex)
                            {
                                QB.Logger.Warn($"Destroy failed for {type.FullName}: {ex.Message}");
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

 
    }
}
