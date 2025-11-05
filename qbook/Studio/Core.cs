using ActiproSoftware.Text;
using ActiproSoftware.Text.Implementation;
using ActiproSoftware.Text.Languages.CSharp.Implementation;
using ActiproSoftware.Text.Languages.DotNet.Reflection;
using ActiproSoftware.Text.Languages.Python.Implementation;
using ActiproSoftware.Text.Tagging.Implementation;
using ActiproSoftware.UI.WinForms.Controls.SyntaxEditor;
using ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.Highlighting.Implementation;
using CefSharp.DevTools.Debugger;
using CSScripting;
using CSScriptLib;
using IronPython.Runtime.Operations;
using log4net;
using log4net.Appender;
using log4net.Layout;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.SolutionPersistence.Model;
using Newtonsoft.Json;
using QB; //qbookCsScript
using QB.Net;
using qbook.CodeEditor;
using qbook.ScintillaEditor;
using qbook.Scripting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Printing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Xml;
using System.Text.Json;
using static IronPython.Modules._ast;
using static IronPython.Runtime.Exceptions.PythonExceptions;
using static log4net.Appender.ColoredConsoleAppender;
using static qbook.ScintillaEditor.FormScintillaEditor;



namespace qbook
{
    public class Core
    {
        public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static RoslynService Roslyn;
        public static AdhocWorkspace Workspace => Roslyn.GetWorkspace;
        public static ProjectId Id => Roslyn.GetProjectId;



        public static List<string> KnownObjectNames = (new[]
        {
            "true" , "false" , "return" , "var" , "let" , "const" , "for" , "while" , "do" , "module" ,
            "if" , "else" , "break" , "continue" , "yield" , "async" , "await" , "class" , "static" , "new" , "delete" , "pi" , "match",
            ////SystemPlugin
            //"oCounter",
            ////ModulePlugin
            //"oModule",
            ////TablePlugin
            //"oTable",
            ////WidgetPlugin
            //"wButton", "wGauge",
        }).ToList();

        //2025-09_23 STFU: new location for ProgramMainCode
        public static string ProgramWorkingDir = null;


        public static void Move<T>(List<T> list, int oldIndex, int newIndex)
        {
            // exit if positions are equal or outside array
            if ((oldIndex == newIndex) || (0 > oldIndex) || (oldIndex >= list.Count) || (0 > newIndex) ||
                (newIndex >= list.Count)) return;
            // local variables
            var i = 0;
            T tmp = list[oldIndex];
            // move element down and shift other elements up
            if (oldIndex < newIndex)
            {
                for (i = oldIndex; i < newIndex; i++)
                {
                    list[i] = list[i + 1];
                }
            }
            // move element up and shift other elements down
            else
            {
                for (i = oldIndex; i > newIndex; i--)
                {
                    list[i] = list[i - 1];
                }
            }
            // put element from position 1 to destination
            list[newIndex] = tmp;
        }

        public static Book ThisBook = null;



        private static oControl _actualMain = null;
        //  public static string PagesRootText = "";
        public static oControl ActualMain
        {
            get
            {
                if (ThisBook == null)
                    return null;


                if (_actualMain == null)
                {
                    _actualMain = ThisBook.Main;
                    if (_actualMain == null)
                        return null;
                    _actualMain.Name = "";
                }
                return _actualMain;
            }
            set
            {
                _actualMain = value;
            }
        }


        static Dictionary<string, QB.Signal> _signalDict = new Dictionary<string, QB.Signal>();
        public static QB.Signal GetSignal(string name)
        {
            lock (_signalDict)
            {
                if (!_signalDict.ContainsKey(name))
                    _signalDict.Add(name, new QB.Signal(name));

                _signalDict[name].Name = name;
                return _signalDict[name];
            }
        }
        static Dictionary<string, QB.Module> _moduleDict = new Dictionary<string, QB.Module>();
        public static QB.Module GetModule(string name)
        {
            lock (_moduleDict)
            {
                if (!_moduleDict.ContainsKey(name))
                    _moduleDict.Add(name, new QB.Module(name)); //TODO: name?!

                _moduleDict[name].Name = name;
                return _moduleDict[name];
            }
        }
        static Dictionary<string, QB.Module> _udlmoduleDict = new Dictionary<string, QB.Module>();
        public static QB.Module GetUdlModule(string name)
        {
            lock (_udlmoduleDict)
            {
                if (!_udlmoduleDict.ContainsKey(name))
                    _udlmoduleDict.Add(name, new QB.Module(name)); //TODO: name?!

                _udlmoduleDict[name].Name = name;
                return _udlmoduleDict[name];
            }
        }



        public static oPage SelectedPage
        {
            get
            {
                if (qbook.Core.ActualMain == null)
                    return null;

                List<oItem> pages = qbook.Core.ActualMain.Objects.Where(item => item is oPage).ToList();
                foreach (oPage page in pages)
                    if (page.Selected) return page;
                if (pages.Count < 1)
                    pages.Add(new oPage("", "page 1"));

                pages[0].Selected = true;
                return pages[0] as oPage;
            }
            set
            {
                var oldPage = ActualMain.Objects.OfType<oPage>().FirstOrDefault(p => p.Selected);
                foreach (oItem page in ActualMain.Objects.Where(item => item is oPage))
                    page.Selected = false;
                if (value != null)
                    value.Selected = true;

                if (oldPage != value)
                    OnSelectedPageChanged(oldPage, value);
            }
        }
        public class SelectedPageChangedEventArgs : EventArgs
        {
            public oPage OldPage;
            public oPage NewPage;
        }
        public delegate void SelectedPageChangedEventHandler(SelectedPageChangedEventArgs e);
        public static event SelectedPageChangedEventHandler SelectedPageChangedEvent;
        static void OnSelectedPageChanged(oPage oldPage, oPage newPage)
        {
            if (SelectedPageChangedEvent != null)
            {
                SelectedPageChangedEventArgs ea = new SelectedPageChangedEventArgs() { OldPage = oldPage, NewPage = newPage };
                SelectedPageChangedEvent(ea);
            }
        }


        public static void ItemMoveForward(oItem item)
        {
            Move(ActualMain.Objects, ActualMain.Objects.IndexOf(item), ActualMain.Objects.IndexOf(item) + 1);
        }
        public static void ItemMoveBack(oItem item)
        {
            Move(ActualMain.Objects, ActualMain.Objects.IndexOf(item), ActualMain.Objects.IndexOf(item) - 1);
        }
        public static int PageNumber(oPage page)
        {
            int i = 1;
            List<oItem> pages = qbook.Core.ActualMain.Objects.Where(item => item is oPage).ToList();


            foreach (oPage _page in pages)
            {
                if (page == _page)
                    return i;
                i++;
            }
            return 0;

        }


        public static oLayer SelectedLayer
        {
            get
            {
                List<oItem> layers = SelectedPage.Objects.Where(item => item is oLayer).ToList();
                foreach (oLayer layer in layers)
                    if (layer.Selected) return layer;
                if (layers.Count < 1)
                    // layers.Add(new oLayer("Main", ""));
                    layers[0].Selected = true;
                return layers[0] as oLayer;
            }
            set
            {
                foreach (oItem layer in SelectedPage.Objects.Where(item => item is oLayer))
                    layer.Selected = false;
                if (value != null)
                    value.Selected = true;
            }
        }




        public static void Init()
        {


            //    System.Threading.Thread IdleThread = new System.Threading.Thread(Idle);
            //    IdleThread.IsBackground = true;
            //  IdleThread.Start();
        }

        /*
        public static void Idle()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(50);
                if (Main.Qb.File !=null)
                    RootPage.Idle();
                
            }
        }
        */


        //cs-script 
        // A project assembly (similar to a Visual Studio project) contains source files and assembly references for reflection
        internal static IProjectAssembly CsScriptAssembly;
        internal static IProjectAssembly PyScriptAssembly;
        internal static System.Reflection.Assembly CsScript_ass;
        internal static CSharpSyntaxLanguage CsScriptLanguage = null;// new CSharpSyntaxLanguage();
        internal static PythonSyntaxLanguage PyScriptLanguage = null;
        internal static System.Reflection.Assembly ActiveCsAssembly = null;
        internal static string LastScriptDllFilename = null;



        private static string GetQbookScriptDllPath()
        {
            var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.GetFullPath(Path.Combine(exeDir, "libs", "qbookCsScript.dll"));
        }

        private static bool IsAssemblyLoaded(string assemblyName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => string.Equals(a.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase));
        }

        static void CsScriptInit()
        {
            if (CsScriptAssembly == null)
            {
                CsScriptAssembly = new CSharpProjectAssembly("qbScript");

                try
                {
                    var qbookPath = GetQbookScriptDllPath();
                    if (!IsAssemblyLoaded("qbookCsScript"))
                        CsScriptAssembly.AssemblyReferences.AddFrom(qbookPath);
                }
                catch (Exception ex)
                {
                    QB.Logger.Error("#EX in CsScriptInit - Could not reference qbookCsScript.dll: " + ex.Message + (QB.Logger.ShowStackTrace ? ex.StackTrace : ""));
                }

                var assemblyLoader = new BackgroundWorker();
                assemblyLoader.DoWork += DotNetProjectAssemblyReferenceLoader;
                assemblyLoader.RunWorkerAsync();

                CsScriptLanguage = new CSharpSyntaxLanguage();
                CsScriptLanguage.RegisterService(CsScriptAssembly);
                CsScriptLanguage.RegisterService(new CodeDocumentTaggerProvider<qbook.CodeEditor.IncludeCsCodeTagger>(typeof(qbook.CodeEditor.IncludeCsCodeTagger)));
                CsScriptLanguage.RegisterService(new CodeDocumentTaggerProvider<qbook.CodeEditor.HighlightRangeTagger>(typeof(qbook.CodeEditor.HighlightRangeTagger)));
            }
        }



        static void PyScriptInit()
        {
            if (PyScriptAssembly == null)
            {
                PyScriptAssembly = new CSharpProjectAssembly("qbScript");

                try
                {
                    var qbookPath = GetQbookScriptDllPath();
                    if (!IsAssemblyLoaded("qbookCsScript"))
                        PyScriptAssembly.AssemblyReferences.AddFrom(qbookPath);
                }
                catch (Exception ex)
                {
                    QB.Logger.Error("#EX in PyScriptInit - Could not reference qbookCsScript.dll: " + ex.Message + (QB.Logger.ShowStackTrace ? ex.StackTrace : ""));
                }

                var assemblyLoader = new BackgroundWorker();
                assemblyLoader.DoWork += DotNetProjectAssemblyReferenceLoader;
                assemblyLoader.RunWorkerAsync();

                PyScriptLanguage = new PythonSyntaxLanguage();
                PyScriptLanguage.RegisterService(PyScriptAssembly);
            }
        }


        private static string _LastCsScriptBuildCode = "";
        internal static string LastCsScriptBuildCode
        {
            get
            {
                return _LastCsScriptBuildCode;
            }
            set
            {
                _LastCsScriptBuildCode = value;
                //_LastCsScriptBuildCodeLines = LastCsScriptBuildCode.Replace("\r", "").Split('\n');
                _LastCsScriptBuildCodeLines = LastCsScriptBuildCode.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            }
        }
        private static string[] _LastCsScriptBuildCodeLines = null;
        internal static string[] LastCsScriptBuildCodeLines
        {
            get
            {
                return _LastCsScriptBuildCodeLines;
            }
            //set
            //{

            //}
        }


        internal static dynamic csScript;
        internal static bool IsBuilding = false;
        internal static string LastBuildResult = null; //null=OK
        internal static string LastBuildErrors = null; //null=OK

        //static internal void CsScriptRebuildAllAsync(string toastInfoText = "Rebuilding Script-Code")
        //{
        //    new Task(() => CsScriptRebuildAll(toastInfoText));
        //}

        internal static string DefaultUsings = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text.RegularExpressions;
using QB;
using QB.UI;
using QB.Logging;
using QB.Controls;
using QB.Net;
using QB.Automation;
using QB.Amium.Controls;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;
";

        internal static string GetUsingsCode(Book book = null)
        {
            if (book == null)
                book = qbook.Core.ThisBook;

            List<string> usingList = new List<string>();
            foreach (var page in book.Main.Objects)
            {
                string pageCode = page.CsCode;
                pageCode = pageCode.Replace("using @__default_usings__;", DefaultUsings);
                usingList.AddRange(ScriptHelpers.GetUsingsFromCode(pageCode)); //.CsCodeHeader));
            }
            usingList = usingList.Distinct().ToList();
            string usingsCode = string.Join("\r\n", usingList.Select(i => "using " + i + ";"));

            return usingsCode;
        }


        internal static string CsScriptCombineCode(Book book = null)
        {
            if (book == null)
                book = qbook.Core.ThisBook;

            string programMain = qbook.Core.ProgramMainCode;

            Regex includeMatchRegex = new Regex(@"\n\s*//\+include (?<include>.*)");

            //2) join all pages' code
            string code = "";
            foreach (var page in book.Main.Objects)
            {
                //code += "\r\n\r\n//=== class '" + page.FullName + $"' === ////cs:{page.FullName}@ln:0\r\n";
                if (false) //CsCode + add all CsCodeExtras before footer
                {
                    code += "\r\n\r\n//=== class '" + page.FullName + $"' === ////cs:{page.FullName}@ln:0\r\n";
                    code += "public class @class_" + page.FullName + " {";
                    code += "\r\nprivate static string _classpath_ = \"" + page.FullName + "\";";
                    code += "\r\n" + ScriptHelpers.StripUsingsFromCode(page.CsCode, out int offset);
                    foreach (var subCode in page.CsCodeExtra)
                    {
                        //--- Page.sub 'p2' ---
                        code += "\r\n\r\n//=== Page.sub '" + page.FullName + "." + subCode.Key + "' ===";
                        code += "\r\n" + subCode.Value;
                        code += "\r\n//=== \\Page.sub '" + page.FullName + "." + subCode.Key + "' ===";
                    }
                    code += "\r\n}";
                }

                if (true) //CsCode + CsCodeExtras added using //+include nameOfPage -> relative: name or ./name or absolute: /root/page2/name
                {
                    //string pageCode = page.CsCode;
                    string pageCode = page.CsCode;

                    //perform important auto-upgrad-code changes here:
                    pageCode = pageCode.Replace("Main.QB.Logger.", "QB.Logger.");
                    page.CsCode = pageCode;
                    //-end- perform important auto-upgrad-code changes here

                    /*HALE:temp?*/
                    pageCode = pageCode.Replace("using @__default_usings__;", DefaultUsings);
                    /*HALE:temp?*/
                    pageCode = pageCode.Replace("@__page__", $"_page_{(page.FullName.Replace('.', '_'))}");
                    pageCode = ScriptHelpers.StripUsingsFromCode(pageCode, out int offset);
                    code += "\r\n\r\n//=== class '" + page.FullName + $"' === ////cs:{page.FullName}@ln:{offset}\r\n";
                    //MatchEvaluator ReplaceCodeIncludeEvaluator = new MatchEvaluator(ReplaceCodeInclude);
                    //var pageCode2 = includeMatchRegex.Replace(pageCode, ReplaceCodeIncludeEvaluator);
                    var pageCode2 = includeMatchRegex.Replace(pageCode, match => ReplaceCodeInclude(match, page.FullName));
                    code += pageCode2;
                }
                code += "\r\n//=== \\Class '" + page.FullName + "' ===\r\n";
            }

            return code;
        }



        public class Job : MarshalByRefObject
        {
            public string Do(Book book)
            {
                try
                {
                    ActiproSoftware.Products.ActiproLicenseManager.RegisterLicense(Licensing.Decrypt("v0XW+hEwOMFQr5Hymgo7iw=="), Licensing.Decrypt("zht+EQ4LRTJKXl7EOd7Xwfu27VNxniquzsoMHWaHyvo="));

                    Console.WriteLine("---Job.Do()");

                    //CSScript.Evaluator.DebugBuild = true;
                    qbook.Core.ThisBook = book;
                    qbook.Core.UpdateProjectAssemblyQbRoot("Core.Job.Do");
                    string usingsCode = GetUsingsCode(); // book);
                    string csScriptCode = CsScriptCombineCode(); // book);
                    string fullCode = usingsCode + "\r\n" + qbook.Core.ProgramMainCode + csScriptCode;

                    //                    fullCode = @"
                    //using System;
                    //class Test
                    //{
                    //public void Write(string text)
                    //{
                    //Console.WriteLine(""text: "" + text);
                    //}
                    //}
                    //";

                    CSScript.Evaluator.DebugBuild = true;
                    //CSScript.EvaluatorConfig.PdbFormat = DebugInformationFormat.Embedded;
                    string assFileName = CSScript.Evaluator
                                             .ReferenceAssembly(Path.GetFullPath(Path.Combine("libs", "qbookCsScript.dll")))
                                             .CompileAssemblyFromCode(fullCode, "ScriptDll.dll");
                    //.CompileAssemblyFromCode(fullCode, "ScriptDll." + Guid.NewGuid() + ".dll");


                    if (true)
                    {
                        Assembly ass = Assembly.LoadFrom(assFileName);
                        csScript = ass.CreateObject("*");
                        qbook.Core.ActiveCsAssembly = (System.Reflection.Assembly)csScript.GetType().Assembly; //hope this works?! 
                        qbook.Core.CsScript_Init();
                        //csScript._RunClasses(null);
                        qbook.Core.RunCsScript_Run();
                        //csScript.Write("hi");
                        LastBuildResult = null;
                    }
                    return assFileName;

                }
                catch (Exception ex)
                {
                    return null;
                }
            }


            public string DoExternal(Book book)
            {
                try
                {
                    ActiproSoftware.Products.ActiproLicenseManager.RegisterLicense(Licensing.Decrypt("v0XW+hEwOMFQr5Hymgo7iw=="), Licensing.Decrypt("zht+EQ4LRTJKXl7EOd7Xwfu27VNxniquzsoMHWaHyvo="));

                    Console.WriteLine("---Job.DoExternal()");

                    //CSScript.Evaluator.DebugBuild = true;
                    qbook.Core.ThisBook = book;
                    qbook.Core.UpdateProjectAssemblyQbRoot("Job.DoExternal");
                    return "";

                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }


        static AppDomain scriptDomain = null;


        //public class ErrorFragment
        //{
        //    public string PageName { get; set; }
        //    public string ClassName { get; set; }
        //    public string Description { get; set; }
        //    public int Line { get; set; }
        //    public int Column { get; set; }
        //    public int Length { get; set; }
        //    public string[] FullCode { get; set; }

        //    private int _offset = 0;

        //    public ErrorFragment(int line, int col, int length, string description, string fullCode)
        //    {
        //        Line = line;
        //        Column = col;
        //        Length = length;
        //        Description = description;
        //        FullCode = fullCode.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        //        GetClassPageName();
        //    }

        //    public override string ToString() => $"{PageName},{ClassName},{Line},{Column},{Length},{Description}";

        //    private void GetClassPageName()
        //    {
        //        string pageName = null;
        //        string className = null;
        //        bool inIncludeBlock = false;

        //        int pageStart = 0;

        //        // 1. PageName suchen
        //        for (int i = Line; i >= 0; i--)
        //        {
        //            var line = FullCode[i];
        //            var match = System.Text.RegularExpressions.Regex.Match(line, @"//=== class '([^']+)' ===");
        //            if (match.Success)
        //            {
        //                pageStart = i;
        //                pageName = match.Groups[1].Value;
        //                break;
        //            }
        //        }

        //        bool insideInclude = IsInsideIncludeBlock(FullCode, Line, pageStart);
        //        Debug.WriteLine("isInside " + insideInclude + " : " + Description);
        //        if (insideInclude)
        //        {
        //            className = GetClassNameInsideInclude(FullCode, Line, pageStart);
        //            Debug.WriteLine("ClassName= " + className);
        //        }
        //        else 
        //        {
        //            className = GetClassNameOutsideInclude(FullCode, Line, pageStart);
        //            Debug.WriteLine("ClassName= " + className);
        //        }

        //        Line = Line - _offset;
        //        PageName = pageName;
        //        ClassName = className;
        //    }

        //    private bool IsInsideIncludeBlock(string[] lines, int lineIndex, int pageIndex)
        //    {
        //        bool inIncludeBlock = false;

        //        for (int i = lineIndex; i <= lines.Count()-1; i++)
        //        {
        //            var line = lines[i].Trim();

        //            if (line.StartsWith("//+include") && line.Contains("end"))
        //            {
        //                return true;
        //            }
        //        }

        //        return false;
        //    }

        //    private string GetClassNameInsideInclude(string[] lines, int lineIndex, int pageIndex)
        //    {
        //        string searchString = "public class ";
        //        for (int i = lineIndex; i >= pageIndex; i--)
        //        {
        //            var line = lines[i].Trim();
        //           // Debug.WriteLine(i + ". " + line);
        //            if (line.StartsWith(searchString))
        //            {
        //                _offset = i;
        //                var rest = line.Substring(searchString.Length).Trim();
        //                Debug.WriteLine(i + ".found");

        //                return rest.Split(new[] { ' ', '{', ':' }, StringSplitOptions.RemoveEmptyEntries)[0];
        //            }
        //        }
        //        return null;
        //    }

        //    private string GetClassNameOutsideInclude(string[] lines, int lineIndex, int pageIndex)
        //    {
        //        string searchString = "public class ";
        //        bool inIncludeBlock = false;
        //        _offset = lineIndex+1;
        //        for (int i = lineIndex; i >= pageIndex; i--)
        //        {
        //            var line = lines[i].Trim();
        //            Debug.WriteLine(i + ". " + line);

        //            if (!inIncludeBlock) _offset--;
        //                if (line.StartsWith("//+include") && line.Contains("end"))
        //                inIncludeBlock = true;
        //            if (line.StartsWith("//+include") && line.Contains("start"))
        //                inIncludeBlock = false;

        //            if (!inIncludeBlock && line.StartsWith(searchString))
        //            {

        //                var rest = line.Substring(searchString.Length).Trim();
        //                return rest.Split(new[] { ' ', '{', ':' }, StringSplitOptions.RemoveEmptyEntries)[0];
        //            }
        //        }
        //        return null;
        //    }




        //}

        //public static List<ErrorFragment> ErrorList = new();

    
        static string ReplaceCodeInclude(Match m, string pathToPage = "")
        {
            if (!m.Groups[1].Success)
                return m.ToString();
            else
            {
                string key = m.Groups["include"].Value.Trim();
                string fullName = null;
                //string fullName = key.Substring(1).Replace('/', '.');
                if (key.StartsWith("/")) // || key.StartsWith("."))
                {
                    //absolute
                    fullName = key.Substring(1).Replace('/', '.');
                }
                else
                {
                    //relative
                    if (key.StartsWith("./"))
                        key = key.Substring(2);
                    fullName = pathToPage + "/" + key;
                    fullName = fullName.TrimStart(new char[] { '/', '.' });
                }
                fullName = fullName.Replace('/', '.');

                foreach (var page in qbook.Core.ThisBook.Main.Objects.OfType<oPage>())
                {
                    if (page.FullName == fullName)
                    {
                        //found as page-code
                        return ""
                            //+ "\r\n" 
                            + m.Groups[0].Value.Trim() + $" --- start --- ////cs:{fullName}@ln:1"
                            + "\r\n"
                            + page.CsCode
                            + "\r\n"
                            + m.Groups[0].Value.Trim() + " --- end ---"
                            + "\r\n";
                    }
                    foreach (var subCode in page.CsCodeExtra)
                    {
                        if (page.FullName + "." + subCode.Key == fullName)
                        {
                            //found in subCode
                            return ""
                                //+ "\r\n"  
                                + m.Groups[0].Value.Trim() + $" --- start --- ////cs:{fullName}@ln:1"
                                + "\r\n"
                                + subCode.Value
                                + "\r\n"
                                + m.Groups[0].Value.Trim() + " --- end ---"
                                + "\r\n";
                        }
                    }
                }
                //not found
                return m.Groups[0].Value
                   //+ "\r\n"
                   + $"//###ERR: code for '{key}' ('{fullName}') not found"
                   + "\r\n"
                   + "\r\n";
            }
        }
        internal static void CsScript_Init()
        {
            if (csScript != null)
            {
                try
                {
                    //SCAN UDL.Client.ResetClients();
                    QB.Net.Ak.Client.ResetClients();
                    //SCAN  Net.AK.Server.ResetClients();
                    QB.Root.ResetObjectDict();
                    QB.Root.ResetWidgetDict();
                    //SCAN         Main.Qb.Automation.Signalgenerator.SignalGenList.Clear();
                    // System.Threading.Thread.Sleep(1000);

                    csScript._InitClasses(null);
                    //QB.Root.UpdateBoxBounds();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("#EX CsScript: " + ex.Message + (QB.Logger.ShowStackTrace ? ex.StackTrace : ""));
                }
            }
        }

        internal static async Task<bool> RunCsScript_Run()
        {
            if (csScript != null)
            {
                try
                {
                    //Main.Qb.Root.ResetObjectDict();
                    //Main.Qb.Root.ResetWidgetDict();
                    //Main.Qb.Automation.SignalGen.SignalGenList.Clear();
                    csScript._RunClasses(null);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("#EX RunCsScript_Run: " + ex.Message + (QB.Logger.ShowStackTrace ? ex.StackTrace : ""));
                    QB.Logger.Error("#EX in RunCsScript_Run: " + ex.ToString());
                    return false;
                }
            }
            return false;
        }

        internal static void CsScript_Destroy()
        {
            if (csScript != null)
            {
                try
                {
                    //Main.Qb.Root.ResetObjectDict();
                    //Main.Qb.Root.ResetWidgetDict();
                    //Main.Qb.Automation.SignalGen.SignalGenList.Clear();
                    csScript._DestroyClasses(null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("#EX CsScript_Destroy: " + ex.Message + (QB.Logger.ShowStackTrace ? ex.StackTrace : ""));
                    QB.Logger.Error("#EX in CsScript_Destroy: " + ex.ToString());
                }
            }
        }



        private static void DotNetProjectAssemblyReferenceLoader(object sender, DoWorkEventArgs e)
        {
            // Add some common assemblies for reflection (any custom assemblies could be added using various Add overloads instead)
            SyntaxEditorHelper.AddCommonDotNetSystemAssemblyReferences(CsScriptAssembly);
        }

        static ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.SyntaxEditor syntaxEditorMain = null;
        internal static string ProgramMainCode;
        internal static string MethodMainCode_2step;
        internal static void UpdateProjectAssemblyQbRoot(string sender)
        {
            ProgramMainCode = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QbRoot;
using log4net;

[Serializable]
public class QbScript
{
public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

static QbScript()
{
//log4net.Repository.ILoggerRepository repository = log4net.LogManager.GetRepository(System.Reflection.Assembly.GetCallingAssembly());
//var fileInfo = new System.IO.FileInfo(@""log4net.config"");
//log4net.Config.XmlConfigurator.Configure(repository, fileInfo);

log4net.Repository.Hierarchy.Hierarchy hierarchy = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
hierarchy.Root.RemoveAllAppenders(); //start anew...
//RollingFileAppender
var rollerPatternLayout = new log4net.Layout.PatternLayout();
rollerPatternLayout.ConversionPattern = ""%date [%thread] %-5level %logger - %message%newline"";
rollerPatternLayout.ActivateOptions();
log4net.Appender.RollingFileAppender roller = new log4net.Appender.RollingFileAppender();
roller.AppendToFile = false;
roller.File = ""qbook.log""/*{logfilename}*/;
roller.Layout = rollerPatternLayout;
roller.MaxSizeRollBackups = 5; 
roller.MaximumFileSize = ""10MB"";
roller.RollingStyle = log4net.Appender.RollingFileAppender.RollingMode.Size;
roller.StaticLogFileName = true;
roller.ActivateOptions();
hierarchy.Root.AddAppender(roller);

//UdpAppender
var udpPatternLayout = new log4net.Layout.PatternLayout();
udpPatternLayout.ConversionPattern = ""%date [%thread] %-5level %logger - %message%newline"";
udpPatternLayout.ActivateOptions();
var udpAppender = new log4net.Appender.UdpAppender();
udpAppender.RemoteAddress = new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 });
udpAppender.RemotePort = 39999;
udpAppender.Layout = udpPatternLayout;
udpAppender.ActivateOptions();
hierarchy.Root.AddAppender(udpAppender);

hierarchy.Root.Level = log4net.Core.Level.All;
hierarchy.Configured = true;

ILog logger = log4net.LogManager.GetLogger(hierarchy.Name, ""qbLogger"");
}

public void _InitClasses(string[] args)
{
  log.Debug(""_InitClasses()"");
  try
  {
    AppDomain.CurrentDomain.UnhandledException += (sender,e)
      => FatalExceptionObject(e.ExceptionObject);

//SCAN    Application.ThreadException += (sender,e)
//SCAN      => FatalExceptionHandler.Handle(e.Exception);

/*{execPageInitList}*/

  }
  catch (Exception huh)
  {
    FatalExceptionHandler.Handle(huh);
  }
}

public void _RunClasses(string[] args)
{
  log.Debug(""_RunClasses()"");  try
  {
  //  AppDomain.CurrentDomain.UnhandledException += (sender,e)
  //    => FatalExceptionObject(e.ExceptionObject);
  //  Application.ThreadException += (sender,e)
  //    => FatalExceptionHandler.Handle(e.Exception);

/*{execPageRunList}*/
    
  }
  catch (Exception huh)
  {
    FatalExceptionHandler.Handle(huh);
  }
}

public void _DestroyClasses(string[] args)
{
  log.Debug(""_DestroyClasses()"");  try
  {
  //  AppDomain.CurrentDomain.UnhandledException += (sender,e)
  //    => FatalExceptionObject(e.ExceptionObject);
  //  Application.ThreadException += (sender,e)
  //    => FatalExceptionHandler.Handle(e.Exception);

/*{execPageDestroyList}*/
    
  }
  catch (Exception huh)
  {
    FatalExceptionHandler.Handle(huh);
  }
}

static void FatalExceptionObject(object exceptionObject) {
  var huh = exceptionObject as Exception;
  if (huh == null) 
  {
    huh = new NotSupportedException(
      ""Unhandled exception doesn't derive from System.Exception: ""
      + exceptionObject.ToString());
  }
  FatalExceptionHandler.Handle(huh);
}
} //\public class Program

static System.Text.RegularExpressions.Regex cssSourceCodeRegex = new System.Text.RegularExpressions.Regex(
  @""\b(?<filename>[A-Z]\:.*\d+\.[0-9a-z]+\-[0-9a-z]{4}\-[0-9a-z]{4}\-[0-9a-z]{4}\-[0-9a-z]+.tmp)\:line (?<linenr>\d+)"", System.Text.RegularExpressions.RegexOptions.Compiled);
static class FatalExceptionHandler 
{
  public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
  public static void Handle(Exception ex)
  {
    string stackTrace = ex.StackTrace;
    int endOfStackPos = stackTrace.IndexOf(""--- End of stack trace"");
    if (endOfStackPos > 0)
        stackTrace = stackTrace.Substring(0, endOfStackPos).TrimEnd();
    log.Debug(""#EX#FATAL: "" + ex.Message + "" => StackTrace:"" + stackTrace);
    var m = cssSourceCodeRegex.Match(stackTrace);
    if (m.Success)
    {
        try 
        {
            int.TryParse(m.Groups[""linenr""].Value, out int linenr);
            string line = System.IO.File.ReadAllLines(m.Groups[""filename""].Value).Skip(linenr-1).First();
            log.Debug("">>failed source code:"" + line);
        }
        catch (Exception ex1)
        {
            log.Debug(""#EX getting source code information..."" + ex1.Message);
        }
    }
    QB.Logger.Error(""#EX#FATAL: "" + ex.Message + "" => StackTrace:"" + stackTrace);
    //MessageBox.Show(""#EX#FATAL: "" + ex.Message + "" => StackTrace:"" + ex.StackTrace, ""FATAL"");
  }
} //class QbScript

static public class QbRoot
{
/*{staticClassList}*/
}
";

            MethodMainCode_2step = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QbRoot;


public void Main(string[] args)
{
/*{newPageList}*/
/*{execPageInitList}*/
          
/*{execPageRunList}*/

/*{execPageDestroyList}*/
}

public void Main1(string[] args)
{
/*{newPageList}*/
}


public void Test()
{
var @class_main = new @class_main();
}
";


            bool autoAddRootCode = false;
            if (autoAddRootCode)
            {
                if (syntaxEditorMain == null)
                {
                    syntaxEditorMain = new ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.SyntaxEditor();
                    syntaxEditorMain.Document.Language = qbook.Core.CsScriptLanguage;
                }
            }

            if (autoAddRootCode)
            {
                //HACK: (temporarily) add all pages' CsScript so all symbols are mutually known
                foreach (var page in qbook.Core.ThisBook.Main.Objects)
                {
                    //var codeEditorTemp =
                    page.MySyntaxEditor = new ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.SyntaxEditor();
                    page.MySyntaxEditor.Document.Language = qbook.Core.CsScriptLanguage;
                    page.MySyntaxEditor.Document.SetHeaderAndFooterText(page.CsCodeHeader, page.CsCodeFooter);
                    page.MySyntaxEditor.Text = page.CsCode;
                    page.CsCodeSourceFileKey = qbook.Core.CsScriptAssembly.SourceFiles.Last().Key;
                }
            }

            //Main.Qb.CsScriptAssembly.AssemblyReferences.AddFrom(@".\libs\qbookCsScript.dll");

            List<string> staticClassList = new List<string>();
            foreach (var page in qbook.Core.ThisBook.Main.Objects)
            {
                if (string.IsNullOrEmpty(page.Name))
                    continue;

                staticClassList.Add($"public static @class_{page.Name} {page.Name} = new @class_{page.Name}();");
            }
            ProgramMainCode = ProgramMainCode.Replace("/*{staticClassList}*/", String.Join("\r\n", staticClassList));

            List<string> newPageList = new List<string>();
            List<string> execPageInitList = new List<string>();
            List<string> execPageRunList = new List<string>();
            List<string> execPageDestroyList = new List<string>();
            foreach (var page in qbook.Core.ThisBook.Main.Objects)
            {
                if (string.IsNullOrEmpty(page.Name))
                    continue;

                newPageList.Add($"var @class_{page.Name} = new @class_{page.Name}();");
                //execPageInitList.Add($"@class_{page.Name}.Init();");
                //execPageRunList.Add($"@class_{page.Name}.Run();");
                execPageInitList.Add("try { QbRoot."
                    + page.Name
                    + ".Initialize(); } catch (Exception ex) { QB.Logger.Error(\"#EX in class " + page.Name + ".Initialize(): \" + ex.ToString()); }");
                execPageRunList.Add("try { QbRoot."
                    + page.Name
                    + ".Run(); } catch (Exception ex) { QB.Logger.Error(\"#EX in class " + page.Name + ".Run(): \" + ex.ToString()); }");
                execPageDestroyList.Add("try { QbRoot."
                    + page.Name
                    + ".Destroy(); } catch (Exception ex) { QB.Logger.Error(\"#EX in class " + page.Name + ".Destroy(): \" + ex.ToString()); }");

                string key = "QbRoot." + page.Name;
                if (!Root.ClassDict.ContainsKey(key))
                    Root.ClassDict.Add(key, page);
            }
            ProgramMainCode = ProgramMainCode.Replace("/*{newPageList}*/", String.Join("\r\n", newPageList));
            ProgramMainCode = ProgramMainCode.Replace("/*{execPageInitList}*/", String.Join("\r\n", execPageInitList));
            ProgramMainCode = ProgramMainCode.Replace("/*{execPageRunList}*/", String.Join("\r\n", execPageRunList));
            ProgramMainCode = ProgramMainCode.Replace("/*{execPageDestroyList}*/", String.Join("\r\n", execPageDestroyList));
            //set logfilename and remove every log older than 7 days
            //ProgramMainCode = ProgramMainCode.Replace("\"qbook.log\"/*{logfilename}*/", "\"" + Main.Qb.Book.LogFilename.Replace("{date}",DateTime.Now.ToString("yyyy-MM-dd_HHmmss")).Replace('\\', '/') + "\""); 
            ProgramMainCode = ProgramMainCode.Replace("\"qbook.log\"/*{logfilename}*/", "\"" + qbook.Core.ThisBook.LogFilename + "\"");
            string logDir = Path.GetDirectoryName(qbook.Core.ThisBook.LogFilename);
            //  if (logDir.Length > 1)
            try
            {
                DirectoryInfo directory = new DirectoryInfo(logDir);
                var filesToDelete = directory.GetFiles("qbook.*.log")
                    .Where(file => file.LastWriteTime < DateTime.Now.AddDays(-7));
                foreach (var fileToDelete in filesToDelete)
                {
                    try
                    {
                        fileToDelete.Delete();
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }

            MethodMainCode_2step = MethodMainCode_2step.Replace("/*{newPageList}*/", String.Join("\r\n", newPageList));
            MethodMainCode_2step = MethodMainCode_2step.Replace("/*{execPageInitList}*/", String.Join("\r\n", execPageInitList));
            MethodMainCode_2step = MethodMainCode_2step.Replace("/*{execPageRunList}*/", String.Join("\r\n", execPageRunList));
            MethodMainCode_2step = MethodMainCode_2step.Replace("/*{execPageDestroyList}*/", String.Join("\r\n", execPageDestroyList));


            //Main.Qb.CsScriptAssembly.SourceFiles.Add(new )
            //HALE: add the main source file progammatically
            //don't know how to do it properly, so for now, i add a SyntaxEditor control and set it's .Document property...

            if (autoAddRootCode)
            {
                syntaxEditorMain.Text = ProgramMainCode;
            }
        }




        //HALE:20230919 static UdpLogger udpLogger = null;
        //HALE:20230919 static public string UdpLoggerStatus = null;
        static Core()
        {
            log4net.GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;
            string version = "0.0.0.0";
            try
            {
                version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            catch { }

            QB.Logger.Info($"=== qbook v{version} started");

            (new System.Threading.Thread(IdleThread) { IsBackground = true }).Start();

            //CsScriptInit();
            //PyScriptInit();
        }

        //HALE: HACK? -> attempt a static-destructor for 
        private static readonly Destructor Finalise = new Destructor();
        private sealed class Destructor
        {
            ~Destructor()
            {
            }
        }
        //public static void Dispose(object sender, EventArgs e)
        //{
        //    ScriptingEngine.Cleanup();
        //}

        static void IdleThread()
        {
            //approx 200ms cycle?!
            while (true)
            {
                //try to reach a 200ms cycle 
                long ticks = DateTime.Now.Ticks;
                long sleepMs = (DateTime.Now.Ticks - ticks) / TimeSpan.TicksPerMillisecond;
                sleepMs -= 180;
                if (sleepMs < 0)
                    sleepMs = 10;
                Thread.Sleep((int)sleepMs);
            }
        }


        public static BindingList<LogEntry> LogItems = new BindingList<LogEntry>();
        static int _LogCount = 0;
        public static void AddLog(char type, string text, string style = null)
        {
            AddLog(DateTime.Now, type, text, style);
        }
        public static void AddLog(DateTime timestamp, char type, string text, string style = null)
        {
            lock (LogItems)
            {
                while (LogItems.Count > 2000)
                    LogItems.RemoveAt(0);
                _LogCount++;
                LogItems.Add(new LogEntry() { Count = _LogCount, Timestamp = timestamp, Type = type, Text = text, Style = style });
                //modifided = true;
            }
        }

        static LogForm _logForm = null;
        public static void ShowLogForm()
        {
            if (_logForm == null || _logForm.IsDisposed || !_logForm.IsHandleCreated)
            {
                _logForm = new LogForm();
                _logForm.Show();
            }
            _logForm.BringToFront();
        }

        static FormCsScript _csScriptForm = null;
        internal static void ShowCsScriptingForm()
        {
            if (_csScriptForm == null || _csScriptForm.IsDisposed || !_csScriptForm.IsHandleCreated)
            {
                _csScriptForm = new FormCsScript();
                _csScriptForm.Show();
            }
            _csScriptForm.BringToFront();
        }

        static internal bool VerifyDeveloperLicense()
        {
            //TEMP LICENSE-HACK:
            return true;

            try
            {
                DateTime? devLicenseExpires = MainForm.GetLicenseExpires("qbookD:");
                if (devLicenseExpires == null)
                {
                    MessageBox.Show($"You do not have a developer license.\r\nPlease contact amium to aquire a developer license.", "NO DEVELOPER LICENSE", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return false;
                }
                if ((DateTime)devLicenseExpires < DateTime.Now)
                {
                    MessageBox.Show($"Your developer license expired on {((DateTime)devLicenseExpires).ToString("yyyy-MM-dd")}.\r\nPlease contact amium to aquire/update your developer license.", "DEVELOPER LICENSE EXPIRED", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return false;
                }
                return true;
            }
            catch
            {
            }

            return false;
        }

        public static FormScintillaEditor _editor = null;

        internal async static Task ShowFormScintillaEditor(oPage page = null)
        {
            if (!VerifyDeveloperLicense()) return;

            if (_editor == null || _editor.IsDisposed || !_editor.IsHandleCreated)
            {
                _editor = null;
                if (Application.OpenForms.Count > 0)
                {
                    Application.OpenForms[0].Invoke((MethodInvoker)(() =>
                    {
                        _editor = new FormScintillaEditor();
                    }));
                   
                }
                else
                {
                    _editor = new FormScintillaEditor();
                    
                }
            }


            if (_editor.InvokeRequired)
                _editor.Invoke((System.Action)(async () =>
                {
                    await _editor.CreateProjectTree();
                   

                    if (Roslyn.ErrorFiles.Count > 0)
                        await _editor.VisualRebuild();
                    else
                        await _editor.OpenNodeByName(page.Name);

                    _editor.Show();
                }));
            else
            {
                await _editor.CreateProjectTree();
                

                if (Roslyn.ErrorFiles.Count > 0)
                    await _editor.VisualRebuild();
                else
                    await _editor.OpenNodeByName(page.Name);

                _editor.Show();

            }
        }

        public static void InitLogger()
        {
            //string logFilename = Main.Qb.Book.LogFilename.Replace("{date}", DateTime.Now.ToString("yyyy-MM-dd_HHmmss")).Replace('\\', '/');
            string logFilename = Path.Combine(qbook.Core.ThisBook.TempDirectory, ThisBook.Filename.Replace(".qbook","") +".{date}.log");
            logFilename = logFilename.Replace("{date}", DateTime.Now.ToString("yyyy-MM-dd_HHmmss")).Replace('\\', '/');
            log4net.Repository.Hierarchy.Hierarchy hierarchy = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
            hierarchy.Root.RemoveAllAppenders(); //start anew...

            //RollingFileAppender
            PatternLayout rollerPatternLayout = new log4net.Layout.PatternLayout();
            rollerPatternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            rollerPatternLayout.ActivateOptions();
            RollingFileAppender roller = new log4net.Appender.RollingFileAppender();
            roller.AppendToFile = true;
            roller.File = logFilename; // @"qbook.log";
            roller.Layout = rollerPatternLayout;
            roller.MaxSizeRollBackups = 5;
            roller.MaximumFileSize = "10MB";
            roller.RollingStyle = RollingFileAppender.RollingMode.Once; //.Size;
            roller.StaticLogFileName = true;
            roller.LockingModel = new FileAppender.ExclusiveLock();
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            ////FileAppender
            //FileAppender filer = new log4net.Appender.FileAppender();
            //filer.AppendToFile = true;
            //filer.File = logFilename.Replace("qbook.", "qbook2.");
            //filer.Layout = rollerPatternLayout;
            ////filer.LockingModel = new FileAppender.MinimalLock();
            //filer.ActivateOptions();
            //hierarchy.Root.AddAppender(filer);

            //UdpAppender
            var udpPatternLayout = new log4net.Layout.PatternLayout();
            udpPatternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            udpPatternLayout.ActivateOptions();
            var udpAppender = new log4net.Appender.UdpAppender();
            udpAppender.RemoteAddress = new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 });
            udpAppender.RemotePort = 39999;
            udpAppender.Layout = udpPatternLayout;
            udpAppender.ActivateOptions();
            hierarchy.Root.AddAppender(udpAppender);

            hierarchy.Root.Level = log4net.Core.Level.All;
            hierarchy.Configured = true;

            ILog logger = log4net.LogManager.GetLogger(hierarchy.Name, "qbLogger");
            QB.Logger.SetILog(logger);
            qbook.Core.ThisBook.LogFilename = logFilename;


        }
        static bool IsXml(string filePath)
        {
            try
            {
                using var reader = XmlReader.Create(filePath);
                while (reader.Read()) { }
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static MruFilesManager MruFilesManager = new MruFilesManager();

        internal static void CleanupBeforeLoad()
        {
            BookRuntime.DestroyAll();

            // 1) Editor schließen
            if (_editor != null)
            {
                try
                {
                    if (_editor.Visible) _editor.Hide();
                    _editor.RemoveProjectTree();
                    _editor.Dispose();
                }
                catch { }
                _editor = null;
            }

            // 2) Script zerstören
            try { CsScript_Destroy(); } catch { }

            // 3) BookRuntime freigeben
            try { BookRuntime.DestroyAll(); } catch { }

            // 4) AppDomain für Script entladen (falls verwendet)
            try
            {
                var scriptDomainField = typeof(Core).GetField("scriptDomain", BindingFlags.NonPublic | BindingFlags.Static);
                var dom = scriptDomainField?.GetValue(null) as AppDomain;
                if (dom != null) { AppDomain.Unload(dom); scriptDomainField.SetValue(null, null); }
            }
            catch { }

            // 5) Roslyn hart zurücksetzen
            try
            {
                if (Roslyn != null)
                {
                    Roslyn.Reset(hard: true, externalDocHolders: Array.Empty<object>());
                    Roslyn = null;
                }
            }
            catch { }

            // 6) Klassen-/Dicts leeren
            try
            {
                QB.Root.ResetObjectDict();
                QB.Root.ResetWidgetDict();
            }
            catch { }

            // 7) GC-Fenster
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }


        internal static async Task OpenQbookAsync(string fullPath = @"T:\qSave")
        {
            try
            {
                CleanupBeforeLoad();

                string filename = fullPath.GetFileName();
                string directory = fullPath.GetDirName();


                MainForm.SetStatusText("opening qbook: " + fullPath);
                MruFilesManager.Add(fullPath);
                Properties.Settings.Default.MruFileList = MruFilesManager.GetMruCsvString();
                Properties.Settings.Default.Save();

                QB.Root.ActiveQbook = null;

                if(ThisBook!=null)
                {
                    if (qbook.Core.ThisBook.Main == null)
                        qbook.Core.ThisBook.Main = new oControl();  
                }

                ThisBook = null;

                string backupDir = fullPath.Replace(".qbook", ".backup");
                if (!Directory.Exists(backupDir))
                    Directory.CreateDirectory(backupDir);

                string dataDir = "";
                string settingsDir = "";
                bool fromXml = false;
                if (IsXml(fullPath))
                {
                    fromXml = true;
                    ThisBook = Book.Deserialize(fullPath);
                    ThisBook.PageOrder.Clear();
                    dataDir = ThisBook.DataDirectory;
                    settingsDir = ThisBook.SettingsDirectory;
                    XmlToFolder();
                    QB.Logger.Info($"Opening qbook: {fullPath} from XML-File");

                    File.Copy(fullPath, Path.Combine(backupDir, filename), true);

                }
                else
                {
                    ThisBook = BookFromFolder(fullPath.Replace(".qbook", ".code"), "");

                    if (fromXml)
                    {
                        ThisBook.SetDataDirectory(dataDir);
                        ThisBook.SetSettingsDirectory(settingsDir);
                    }
                }

                GlobalExceptions.InitRuntimeErrors();

                ActualMain = ThisBook.Main;
                QB.Root.ActiveQbook = ThisBook;
             

                InitLogger();
                QB.Logger.Info($"Opening qbook: {fullPath}");

                Book.OnStaticPropertyChangedEvent("new Book", "");
                qbook.Core.ThisBook.DesignMode = false;
                qbook.Core.ThisBook.TagMode = false;
                qbook.Core.ThisBook.Recorder = false;

                MainForm.SetStatusText("initializing qbook...");
                ActualMain = ThisBook.Main;
                await BookRuntime.BuildBookAssembly();

                bool autoBuild = true;
              


                qbook.Core.Init();
                if ((qbook.Core.ActualMain?.Objects?.Count ?? 0) > 0)
                    qbook.Core.SelectedPage = qbook.Core.ActualMain.Objects[0] as oPage;


                MainForm mainForm = (MainForm)Application.OpenForms["MainForm"];
                mainForm.InitView = true;
                mainForm.UpdateStartMenuItems();
                if (Roslyn.ErrorFiles.Count > 0)
                {
                    autoBuild = false;
                    MainForm.SetStatusText("#E Error qbook build failed!", 0);

                }
                else
                {
                    MainForm.SetStatusText("qbook loaded successfully!", 3000);
                }

                ThisBook.Init();

                
                if ((System.Windows.Forms.Control.ModifierKeys == (Keys.Control | Keys.Shift)))
                {
                    //don't auto build/run if ctrl-shift are beeing held down at startup

                    autoBuild = false;
                }

                if (autoBuild)
                {
                    BookRuntime.InitializeAll();
                    BookRuntime.RunAll();
                }

            }
            catch (Exception ex)
            {
                QB.Logger.Error("#EX opening qbook: " + ex.Message + (QB.Logger.ShowStackTrace ? ex.StackTrace : ""));
                MessageBox.Show(ex.Message, "ERROR OPENING QBOOK", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MainForm.SetStatusText("#ERR: loading qbook");
            }

        }

        internal static void XmlToFolder()
        {
            Debug.WriteLine("Reset Roslyn");

            Roslyn = new RoslynService();
            Roslyn.EnsureWorkspace();

            Debug.WriteLine("GC Reset");
      
        
          

            Debug.WriteLine("Creating Roslyn Project");
            Core.Roslyn.CreateProject();


            Debug.WriteLine("Processing Pages...");
            string program = "namespace QB\r\n{\r\n\tpublic static class Program \r\n\t{\r\n";
            var roslynFiles = new List<(string fileName, string code)>();
            int pageCount = -1;
            string firstFile = null;

            List<string> Pages = new List<string>();

            int CodeIndex = 0;
            int PageIndex = 0;
            foreach (oPage page in qbook.Core.ActualMain.Objects.OfType<oPage>())
            {
                page.CodeOrder.Clear();
                page.Includes.Clear();
    
                string className = "Definition" + page.Name + ".qPage";
                pageCount++;
                string code = page.CsCode;

                List<string> includes = CutInludesBlock(ref code);

                string pageCode = "namespace Definition" + page.Name + "{\r\n//<CodeStart>\r\n";
                pageCode += Regex.Replace(code, @"public class\s+@class_\w+", "public class qPage");
                pageCode += "\r\n//<CodeEnd>\r\n}";
                program += "\t\tpublic static " + className + " " + page.Name + " { get; } = new " + className + "();\r\n";
                pageCode = ReplaceClassToDefinition(pageCode);

                Pages.Add(page.Name);
                page.Code = pageCode;

                string PageFileName = $"{page.Name}.qPage.cs";

                page.Filename = $"{page.Name}.qPage.cs";

                Core.ThisBook.PageOrder.Add(page.Name);
                page.CodeOrder.Add(PageFileName);

                page.OrderIndex = PageIndex;
                if (firstFile == null)
                    firstFile = PageFileName;

                var lines = code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                var usings = lines
                    .TakeWhile(l => !l.TrimStart().StartsWith("public class"))
                    .Where(l => l.TrimStart().StartsWith("using"))
                    .ToList();

                foreach (var subClass in page.CsCodeExtra)
                {
                    string subCode = "\r\n\r\nnamespace Definition" + page.Name
                        + "\r\n{\r\n//<CodeStart>\r\n"
                        + string.Join("\r\n", usings)
                        + subClass.Value
                        + "\r\n//<CodeEnd>\r\n"
                        + "\r\n}";

                    subCode = ReplaceClassToDefinition(subCode);
                    string subFileName = $"{page.Name}.{subClass.Key}.cs";

                    page.CodeOrder.Add(subFileName);
                    page.SubCodes[subFileName] = new oCode(subFileName, includes.Contains(subClass.Key), null, subCode);

                    if (includes.Contains(subClass.Key))
                    {
                        page.SubCodes[subFileName].RoslynDoc = Core.Roslyn.AddDocument(subFileName, subCode);
                        page.Includes.Add(subFileName);
                    }
                }
            }

        }

        public class PageInfo
        {
            public string Name { get; set; } = "";
            public string PagePath { get; set; } = "";
            public string ObjectPath { get; set; } = "";
        }
        public class PageDefinition
        {
            public string Name { get; set; }
            public string Text { get; set; }
            public int OrderIndex { get; set; }
            public bool Hidden { get; set; }
            public string Format { get; set; }
            public List<string> Includes { get; set; }
            public List<string> CodeOrder { get; set; }
            public string Section { get; set; }
            public string Url { get; set; }
        }

        internal static oPage oPageFromString(string json)
        {
            var data = JsonConvert.DeserializeObject(json, typeof(PageDefinition)) as PageDefinition;
            return new oPage
            {
                Name = data.Name,
                Text = data.Text,
                OrderIndex = data.OrderIndex,
                Hidden = data.Hidden,
                Format = data.Format,
                Includes = data.Includes ?? new List<string>(),
                CodeOrder = data.CodeOrder,
                Section = data.Section,
                Url = data.Url
            };
        }
        internal static Book BookFromFolder(string folderPath, string bookname)
        {

            Roslyn = new RoslynService();
            Roslyn.EnsureWorkspace();
            Roslyn.CreateProject();

            Book newBook = new Book();
            newBook.Main = new oControl();

            string bookJson = File.ReadAllText(Path.Combine(folderPath, "Book.json"));
            var qbook = JsonConvert.DeserializeObject(bookJson, typeof(qBookDefinition)) as qBookDefinition;

            newBook.Version = qbook.Version;
            newBook.VersionHistory = qbook.VersionHistory;
            newBook.VersionEpoch = qbook.VersionEpoch;
            newBook.StartFullScreen = qbook.StartFullScreen;
            newBook.HidPageMenuBar = qbook.HidPageMenuBar;
            newBook.PasswordAdmin = qbook.PasswordAdmin;
            newBook.PasswordService = qbook.PasswordService;
            newBook.PasswordUser = qbook.PasswordUser;
            newBook.Directory = qbook.Directory;
            newBook.Filename = qbook.Filename;
            newBook.SettingsDirectory = qbook.SettingsDirectory;
            newBook.DataDirectory = qbook.DataDirectory;
            newBook.TempDirectory = qbook.TempDirectory;
            newBook.Language = qbook.Language;
            newBook.PageOrder = qbook.PageOrder;


            List<string> reversePageOrder = newBook.PageOrder.AsEnumerable().Reverse().ToList();

            List<oPage> pages = new List<oPage>();

            foreach (string page in reversePageOrder)
            {
                oPage opage = null;
              
                string pageFolder = Path.Combine(folderPath, "Pages", page);
                string oPageJson = File.ReadAllText(Path.Combine(pageFolder, "oPage.json"));
                opage = oPageFromString(oPageJson);
                string filename = page + ".qPage.cs";
                opage.Filename = filename;
                opage.Code = File.ReadAllText(Path.Combine(pageFolder, filename));
     
                List<string> reverseCodeOrder = opage.CodeOrder.AsEnumerable().Reverse().ToList();

                foreach (string codeFile in reverseCodeOrder)
                {
                    if (codeFile.EndsWith("qPage.cs")) continue;
                    string subCode = File.ReadAllText(Path.Combine(pageFolder, codeFile));
                    opage.SubCodes[codeFile] = new oCode(codeFile, opage.Includes.Contains(codeFile), null, subCode);

                }
                pages.Add(opage);
            }
            pages.Reverse();

            foreach (oPage p in pages)
            {
                newBook.Main.Objects.Add(p);
            }

            newBook.Program = Core.Roslyn.AddDocument("Program.cs", File.ReadAllText(Path.Combine(folderPath, "Program.cs")));
            newBook.Global = Core.Roslyn.AddDocument("GlobalUsing.cs", "global using static QB.Program;");
            return newBook;

        }
        public static async Task SaveInFolder()
        {
            string uri = Path.Combine(Core.ThisBook.Directory, Core.ThisBook.Filename.Replace(".qbook", "") + ".code");

            if (Directory.Exists(uri))
            {
                string backupFile = Core.ThisBook.Filename.Replace(".qbook", "") + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".code";
                string backupUri = Path.Combine(Core.ThisBook.BackupDirectory, backupFile);
                Directory.Move(uri, backupUri);
            }

            Directory.CreateDirectory(uri);
            string link = "SaveDate: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            File.WriteAllText(Path.Combine(Core.ThisBook.Directory, Core.ThisBook.Filename), link);

            await SaveProjectAsync(uri);
        }
        public static async Task SaveProjectAsync(string newFile = @"T:\qSave")
        {
           

            if (!Directory.Exists(newFile))
                Directory.CreateDirectory(newFile);

            string codeDir = Path.Combine(newFile, "Pages");
            Directory.CreateDirectory(codeDir);


            // 🧩 Projektbeschreibung vorbereiten
            var project = new qBookDefinition
            {
                ProjectName = Core.ThisBook.Filename.Replace(".qbook", ""),
                Version = Core.ThisBook.Version,
                VersionHistory = Core.ThisBook.VersionHistory,
                VersionEpoch = Core.ThisBook.VersionEpoch,
                StartFullScreen = Core.ThisBook.StartFullScreen,
                HidPageMenuBar = Core.ThisBook.HidPageMenuBar,
                PasswordAdmin = Core.ThisBook.PasswordAdmin,
                PasswordService = Core.ThisBook.PasswordService,
                PasswordUser = Core.ThisBook.PasswordUser,
                Directory = Core.ThisBook.Directory,
                Filename = Core.ThisBook.Filename,
                SettingsDirectory = Core.ThisBook.SettingsDirectory,
                DataDirectory = Core.ThisBook.DataDirectory,
                BackupDirectory = Core.ThisBook.BackupDirectory,
                TempDirectory = Core.ThisBook.TempDirectory,
                Language = Core.ThisBook.Language,
                PageOrder = Core.ThisBook.PageOrder
            };

            foreach (oPage page in qbook.Core.ActualMain.Objects.OfType<oPage>())
            {
                string pageDir = Path.Combine(codeDir, $"{page.Name}");
                Directory.CreateDirectory(pageDir);

                var temp = await Core.Roslyn.GetDocumentTextAsync(page.Filename);
                string csCode = temp.ToString();
                System.IO.File.WriteAllText(Path.Combine(pageDir, page.Filename), csCode);

                foreach(oCode sub in page.SubCodes.Values)
                {
                    temp = await Core.Roslyn.GetDocumentTextAsync(sub.Filename);
                    if(temp == null)
                        temp = sub.Code;
                    csCode = temp.ToString();
                    System.IO.File.WriteAllText(Path.Combine(pageDir, sub.Filename), csCode);
                }

                var dto = new PageDefinition
                {
                    Name = page.Name,
                    Text = page.Text,

                    OrderIndex = page.OrderIndex,

                    Hidden = page.Hidden,
                    Format = page.Format,
                    Includes = page.Includes,
                    Section = page.Section,
                    Url = page.Url,
                    CodeOrder = page.CodeOrder,

                };

                string oPageJson = JsonConvert.SerializeObject(dto, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(Path.Combine(pageDir, "oPage.json"), oPageJson);
            }

            var code = await Core.Roslyn.GetDocumentTextAsync("Program.cs");
            string path = Path.Combine(newFile, "Program.cs");
            File.WriteAllText(path, code);

            code = await Core.Roslyn.GetDocumentTextAsync("GlobalUsing.cs");
            path = Path.Combine(newFile, "GlobalUsing.cs");
            File.WriteAllText(path, code);

            string bookJson = JsonConvert.SerializeObject(project, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(Path.Combine(newFile, "Book.json"), bookJson);

        }
        private static List<string> CutInludesBlock(ref string source)
        {
            List<string> includes = new List<string>();
            if (string.IsNullOrWhiteSpace(source)) return includes;

            var regex = new Regex(@"//\+include\s+(\w+)", RegexOptions.Compiled);
            var lines = source.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var newLines = new List<string>();
            var includeLines = new List<string>();
            int lineNumber = 0;
            int includeLineNumber = 0;

            bool inIncludeBlock = false;
            bool includeStartExists = false;
            bool includeEndExists = false;

            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (match.Success)
                {
                    includes.Add(match.Groups[1].Value);
                    includeLines.Add(line);
                }
                else
                {
                    if (!line.Contains("//<IncludeStart>") && !line.Contains("//<IncludeEnd>"))
                    {
                        newLines.Add(line);
                    }

                }

                if (line.Contains("public class @"))
                {
                    includeLineNumber = lineNumber;
                }
                //{
                //    // Stoppe das Sammeln, wenn ein Include-Block bereits existiert
                //    includeLines.Clear();
                //}
                lineNumber++;

            }
            //Debug.WriteLine("Insert Startline = " + includeLineNumber);
            //Debug.WriteLine("===== Includes ======");

            List<string> includeBlock = new List<string>();

            foreach (string l in includes) Debug.WriteLine(l);

            if (includeLines.Count > 0)
            {
                includeBlock.Add("//<IncludeStart>");
                includeBlock.AddRange(includeLines);
                includeBlock.Add("//<IncludeEnd>");

                // Optional: Du kannst entscheiden, wo der Block eingefügt wird.
                // Hier wird er am Anfang eingefügt.

            }
            else
            {
                includeBlock.Add("\t//<IncludeStart>");
                includeBlock.Add("");
                includeBlock.Add("\t//<IncludeEnd>");
            }

            //  newLines.InsertRange(includeLineNumber + 2, includeBlock);

            source = string.Join("\n", newLines);
            //Debug.WriteLine("===== Updated Source ======");
            //Debug.WriteLine(source);
            return includes;
        }
        private static string ReplaceClassToDefinition(string code)
        {
            string result = code;


            foreach (oPage page in qbook.Core.ActualMain.Objects.OfType<oPage>())
            {
                string find = $"class_{page.Name}";
                string replace = $"Definition{page.Name}";
                Debug.WriteLine("find '" + find + "'");
                Debug.WriteLine("repl '" + replace + "'");

                string pattern = $@"\b{find}\b";

                result = Regex.Replace(result, pattern, replace);
            }

            return result;
        }
        internal static async Task SaveThisBook()
        {
            if (qbook.Core.ThisBook != null)
            {
                MainForm.SetStatusText("saving qbook: " + qbook.Core.ThisBook.Filename);
                await SaveInFolder();
                qbook.Core.ThisBook.Modified = false;
                qbook.Properties.Settings.Default.Save();
                MainForm.SetStatusText("qbook saved successfully!", 3000);
            }
        }
        internal static async Task ShowOpenQbookFileDialog(object sender)
        {
            if (qbook.Core.ThisBook != null && qbook.Core.ThisBook.Modified)
            {
                DialogResult result = MessageBox.Show("save " + qbook.Core.ThisBook.Filename + "?", "qBook NOT SAVED", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                    await SaveInFolder();
            }
            if (_editor != null)
            {
                if (_editor.Visible)
                    _editor.Close();

                _editor.Dispose();
                _editor = null;
            }

            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "qbook files (*.qbook)|*.qbook|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    OpenQbookAsync(openFileDialog.FileName);
             
                }
            }
        }
        internal static void ShowNewQbookFileDialog(object sender)
        {
            if (qbook.Core.ThisBook != null && qbook.Core.ThisBook.Modified)
            {
                DialogResult result = MessageBox.Show("save " + qbook.Core.ThisBook.Filename + "?", "qBook NOT SAVED", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                    qbook.Core.SaveInFolder();
            }

            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "qbook files (*.qbook)|*.qbook|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                   
                    OpenQbookAsync(openFileDialog.FileName);
                   
                }
            }
        }
    }
}
