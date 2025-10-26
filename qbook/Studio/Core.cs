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
using Microsoft.VisualStudio.SolutionPersistence.Model;
using QB; //qbookCsScript
using QB.Net;
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
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Xml;
using static IronPython.Modules._ast;
using static IronPython.Runtime.Exceptions.PythonExceptions;
using static log4net.Appender.ColoredConsoleAppender;

namespace qbook
{
    public class Core
    {


        public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

        internal static async Task<bool> CsScriptRebuildAll(string toastInfoText = "Rebuilding Script-Code")
        {
            //csScript = null;
            //LastBuildResult = null;
            //LastBuildErrors = null;

            //    ErrorList.Clear();

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            QB.UI.Toast toast = null;
            try
            {
                MainForm.SetStatusText("building...");
                CsScript_Destroy();

                csScript = null;
                LastBuildResult = null;
                LastBuildErrors = null;

                IsBuilding = true;
                if (false)
                {
                    toast = QB.UI.Toast.Show("Please wait...", toastInfoText, 30.0);
                }

                UpdateProjectAssemblyQbRoot("Core.CsScriptRebuildAll");

                //this.Cursor = Cursors.WaitCursor;
                //TODO: save all code changes
                foreach (var page in qbook.Core.ThisBook.Main.Objects)
                {
                }
                //TODO: for now, just save this editor's code
                //Item.CsCode = codeEditor.Text;
                //Item.CsCodeHeader = textBoxHeader.Text.Replace("\r", "").Replace("\n", "\r\n");
                //Item.CsCodeFooter = textBoxFooter.Text.Replace("\r", "").Replace("\n", "\r\n");
                //Main.Qb.Book.Serialize();
                //Main.Qb.Book.Modified = false;

                //textBoxOutput.Text = "";
                //toolStripLabelInfo.Text = "building...";
                //mainToolStrip.Refresh();
                CSScript.EvaluatorConfig.Access = EvaluatorAccess.Singleton;
                CSScript.Evaluator.DebugBuild = true; //generate debugging/reflection information
                //string mainEntry = " public void Main() { page1.Go(); }";

                //1) get all pages' (header-)usings and join together

                if (false)
                {
                    List<string> usingList = new List<string>();
                    foreach (var page in qbook.Core.ThisBook.Main.Objects)
                        usingList.AddRange(ScriptHelpers.GetUsingsFromCode(page.CsCodeHeader));
                    usingList = usingList.Distinct().ToList();
                    string usingsCode = string.Join("\r\n", usingList.Select(i => "using " + i + ";"));
                }


                if (false)
                {
                    string programMain = qbook.Core.ProgramMainCode;


                    Regex includeMatchRegex = new Regex(@"\n\s*//\+include (?<include>.*)");

                    //2) join all pages' code
                    string code = "";
                    foreach (var page in qbook.Core.ThisBook.Main.Objects)
                    {
                        code += "\r\n\r\n//=== class '" + page.FullName + "' ===\r\n";
                        code += "public class @class_" + page.FullName + " {";
                        code += "\r\nprivate static string _classpath_ = \"" + page.FullName + "\";";
                        if (false) //CsCode + add all CsCodeExtras before footer
                        {
                            code += "\r\n" + ScriptHelpers.StripUsingsFromCode(page.CsCode, out int offset);
                            foreach (var subCode in page.CsCodeExtra)
                            {
                                //--- Page.sub 'p2' ---
                                code += "\r\n\r\n//=== class.sub '" + page.FullName + "." + subCode.Key + "' ===";
                                code += "\r\n" + subCode.Value;
                                code += "\r\n//=== \\class.sub '" + page.FullName + "." + subCode.Key + "' ===";
                            }
                        }
                        if (true) //CsCode + CsCodeExtras added using //+include nameOfPage -> relative: name or ./name or absolute: /root/page2/name
                        {
                            string pageCode = page.CsCode;
                            //MatchEvaluator ReplaceCodeIncludeEvaluator = new MatchEvaluator(ReplaceCodeInclude);
                            //var pageCode2 = includeMatchRegex.Replace(pageCode, ReplaceCodeIncludeEvaluator);
                            var pageCode2 = includeMatchRegex.Replace(pageCode, match => ReplaceCodeInclude(match, page.FullName));
                            code += pageCode2;
                        }
                        code += "\r\n}";
                        code += "\r\n//=== \\Class '" + page.FullName + "' ===\r\n";
                    }
                }



                //3) fullCode = (joinedHeader + programMain + joinedPages + commonFooter)
                //string fullCode = usingsCode + Main.Qb.ProgramMainCode + code; // + "}";

                //4) add WatchItem code? -> object[] EvalWatchItems(string[] items)
                //4) add Immediate code? -> object EvalImmediateCode(string code)

                sw.Restart();
                //string fullCode = programMain + "\r\n\r\n//---pages---"
                //    + textBoxHeader.Text
                //    + codeEditor.Text
                //    + textBoxFooter.Text;

                CSScript.Evaluator.Reset();


                bool inMemory = true;
                if (inMemory)
                {
                    string usingsCode = GetUsingsCode();
                    string csScriptCode = CsScriptCombineCode();
                    string fullCode = usingsCode + "\r\n" + qbook.Core.ProgramMainCode + csScriptCode;

                    await Task.Run((System.Action)(() =>
                    {
                        int assCount1 = 0;
                        int assCount2 = 0;
                        if (Core.ActiveCsAssembly != null)
                        {
                            try
                            {
                                //ActiveCsAssembly.Unload();
                                assCount1 = AppDomain.CurrentDomain.GetAssemblies().Length;

                                //2025--07-24 stfu
                                //  var scriptAssemblyType = (Type)Core.csScript.GetType();
                                //    scriptAssemblyType.Assembly.Unload();


                                if (Core.csScript != null)
                                {
                                    var scriptAssemblyType = Core.csScript.GetType();
                                    scriptAssemblyType.Assembly.Unload();
                                }
                                //


                                assCount2 = AppDomain.CurrentDomain.GetAssemblies().Length;
                                QB.Logger.Info($"assemblyCount pre/after unload: {assCount1}/{assCount2}");
                            }
                            catch (Exception ex)
                            {
                                assCount2 = AppDomain.CurrentDomain.GetAssemblies().Length;
                                QB.Logger.Debug("#EX unloading active assembly: " + ex.Message + (QB.Logger.ShowStackTrace ? ex.StackTrace : ""));
                                QB.Logger.Debug($"assemblyCount pre/after unload: {assCount1}/{assCount2}");
                            }
                        }

                        //string fullCode = usingsCode + "\r\n" + Main.Qb.ProgramMainCode + code;

                        Core.LastCsScriptBuildCode = fullCode;
                        CSScript.Evaluator.ReferenceAssembliesFromCode(fullCode);
                        //csScript = CSScript.Evaluator
                        //    .ReferenceAssemblyByName("qbookCsScript")
                        //    .LoadCode(fullCode);
                        try
                        {
                            if (Core.CsScript_ass != null)
                                Core.CsScript_ass.Unload();
                        }
                        catch (Exception ex)
                        {

                        }

                        /*
                        //Assembly ass = CSScript.Evaluator
                        CsScript_ass = CSScript.Evaluator
                            .ReferenceAssemblyByName("qbookCsScript")
                            .CompileCode(fullCode);
                        //CsScript_ass = ass;

                        csScript = CSScript.Evaluator
                            .ReferenceAssemblyOf(CsScript_ass)
                            .LoadCode(fullCode);
                        */

                        int assCount3 = AppDomain.CurrentDomain.GetAssemblies().Length;
                        bool useFileDll = false;
                        if (useFileDll) //compile to dll and load
                        {
                            var info = new CompileInfo { RootClass = "QbRoot" };
                            //CsScript_ass?.Unload();
                            Assembly ass = CSScript.Evaluator
                                .With(eval => { eval.IsCachingEnabled = true; })
                                .ReferenceAssemblyByName("qbookCsScript")
                                .CompileCode(fullCode, info);
                            Core.ActiveCsAssembly = ass;

                            Core.csScript = ass.CreateObject("*");

                            //csScript = CSScript.Evaluator                               
                            //    //.ReferenceAssembly(System.IO.Path.GetFullPath("QbRoot.dll"))
                            //    //.ReferenceAssemblyByName("qbookCsScript")
                            //    .LoadFile(fullCode);

                            //TODO: goal: compile to script and then assign to csScript= ... maybe compile all the pages-code to .dll and then .LoadFile(Program&QbRoot-code only?!)
                            //TODO: doesn't load code to csScrip
                            //TODO: unload?
                        }


                        if (!useFileDll) //in-memory
                        {
                            //fullCode = "class Test {}";
                            try
                            {
                                Core.csScript = CSScript.Evaluator
                                    //.With(eval => { eval.IsAssemblyUnloadingEnabled = true; })
                                    //.With(eval => { eval.IsCachingEnabled = true; })
                                    //.With(eval => { eval.IsAssemblyUnloadingEnabled = true; eval.IsCachingEnabled = true; })
                                    .ReferenceAssembliesFromCode("using System.Windows.Forms;\r\n" + fullCode)
                                    .ReferenceAssemblyByName("qbookCsScript")
                                    .LoadCode(fullCode);
                                MainForm.SetStatusText("Rebuild OK", 3000);
                            }
                            catch (Exception ex)
                            {
                                Core.LastBuildErrors = ex.Message;
                                MainForm.SetStatusText("#ERR: build error(s)");

                                string LastErrorDir = Path.Combine(ProgramWorkingDir, "LastError");

                                if (!Directory.Exists(LastErrorDir))
                                    Directory.CreateDirectory(LastErrorDir);

                                File.WriteAllText(Path.Combine(LastErrorDir, "fullErrorCode.txt"), fullCode);
                                File.WriteAllText(Path.Combine(LastErrorDir, "ErrorDescription.txt"), ex.Message);


                                //Create ErrorList


                                string errorString = ex.Message;
                                string[] lines = fullCode.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

                                var errorRegex = new Regex(@"\((\d+),(\d+)\):\s*error\s+\w+:\s*(.+)");

                                foreach (Match match in errorRegex.Matches(errorString))
                                {
                                    int line = int.Parse(match.Groups[1].Value);
                                    int col = int.Parse(match.Groups[2].Value);
                                    string description = match.Groups[3].Value.Trim();

                                    string codeLine = lines[line - 1];
                                    int idx = col - 1;

                                    string fragment = "";
                                    if (idx < codeLine.Length)
                                    {
                                        var tokenMatch = Regex.Match(codeLine.Substring(idx), @"\w+");
                                        fragment = tokenMatch.Success ? tokenMatch.Value : codeLine.Substring(idx, 1);
                                    }

                                    int length = fragment.Length;

                                    //   ErrorFragment _error = new(line, col, length, description, fullCode);
                                    //Debug.WriteLine(_error.ToString());

                                    // ErrorList.Add(_error);
                                }

                                throw;
                                //return ex.Message;
                                //toolStripLabelInfo.Text = "#EX: " + ex.Message;
                                //toolStripLabelInfo.Text = "#ERR: rebuild took " + sw.ElapsedMilliseconds.ToString() + "ms";
                                //tabControl3.SelectedTab = tabPageOutput;
                                //OutputWriteLine("#EX: " + ex.Message);
                            }
                        }

                        int assCount4 = AppDomain.CurrentDomain.GetAssemblies().Length;
                        Console.WriteLine($"assCount: {assCount1}/{assCount2}/{assCount3}/{assCount4}");
                    }));

                    qbook.Core.ActiveCsAssembly = (System.Reflection.Assembly)csScript.GetType().Assembly; //hope this works?!
                    CsScript_Init();
                    LastBuildResult = null;
                }
                else
                {

                    int assCount1 = AppDomain.CurrentDomain.GetAssemblies().Length;
                    /*
                    string usingsCode = GetUsingsCode();
                    string csScriptCode = CsScriptCombineCode();
                    string fullCode = usingsCode + Main.Qb.ProgramMainCode + csScriptCode;
                    string assCode = usingsCode + csScriptCode;
                    CSScript.Evaluator.ReferenceAssembliesFromCode(fullCode);

                    if (true)
                    {
                        var info = new CompileInfo { RootClass = "QbRoot" };
                        //CsScript_ass?.Unload();
                        Assembly ass = CSScript.Evaluator
                            .ReferenceAssemblyByName("qbookCsScript")
                            .CompileCode(fullCode, info);

                        CsScript_ass = ass;
                        csScript = CSScript.Evaluator
                            .ReferenceAssembly(ass)
                            .LoadMethod(Main.Qb.MethodMainCode_2step);
                    }                  

                    csScript.Test();
                    csScript.Main1(null);
                    */

                    if (scriptDomain != null)
                    {
                        try
                        {
                            //csScript._DestroyClasses(null);
                            AppDomain.Unload(scriptDomain);
                        }
                        catch (Exception ex)
                        {
                            //ERR
                        }
                        scriptDomain = null;
                    }
                    int assCount2 = AppDomain.CurrentDomain.GetAssemblies().Length;

                    scriptDomain = AppDomain.CreateDomain("MyAppDomain", null, new AppDomainSetup
                    {
                        ApplicationName = "MyAppDomain",
                        ShadowCopyFiles = "true",
                        PrivateBinPath = "MyAppDomainBin",
                    });

                    qbook.Core.LastScriptDllFilename = null;
                    var job = (Job)scriptDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(Job).FullName);
                    int assCount3 = AppDomain.CurrentDomain.GetAssemblies().Length;
                    var assFile = job.Do(qbook.Core.ThisBook);
                    qbook.Core.LastScriptDllFilename = assFile;
                    //Main.Qb.Book = book;

                    if (false)
                    {
                        //Assembly ass = Assembly.LoadFrom(assFile);
                        Assembly ass = Assembly.Load(File.ReadAllBytes(assFile));

                        csScript = ass.CreateObject("*");

                        //csScript.Print("foo1");
                        //csScript.Test01();
                        //csScript.ShowDialog("Hello World!");

                        qbook.Core.ActiveCsAssembly = (System.Reflection.Assembly)csScript.GetType().Assembly; //hope this works?! 
                        CsScript_Init();
                        //csScript._RunClasses(null);
                        LastBuildResult = null;
                        //return csScript;
                    }

                    int assCount4 = AppDomain.CurrentDomain.GetAssemblies().Length;
                    Console.WriteLine($"assCount: {assCount1}/{assCount2}/{assCount3}/{assCount4}");


                }

                //Main.Qb.ActiveCsAssembly = (System.Reflection.Assembly)csScript.GetType().Assembly; //hope this works?!

                toast?.Close();

                if (false) //1-by-1
                {
                    foreach (var page in qbook.Core.ThisBook.Main.Objects) //.Where(p => p.Name == "p2"))
                    {
                        string pageCode = page.CsCodeHeader + "\r\n" + page.CsCode + "\r\n" + page.CsCodeFooter;
                        //CSScript.Evaluator.ReferenceAssembliesFromCode(pageCode);
                        CSScript.Evaluator.LoadCode(pageCode);
                    }
                    CSScript.Evaluator.ReferenceAssembliesFromCode(qbook.Core.ProgramMainCode);
                    csScript = CSScript.Evaluator.LoadCode(qbook.Core.ProgramMainCode);
                    foreach (var page in qbook.Core.ThisBook.Main.Objects) //.Where(p => p.Name == "p2"))
                    {
                        string pageCode = page.CsCodeHeader + "\r\n" + page.CsCode + "\r\n" + page.CsCodeFooter;
                        csScript = CSScript.Evaluator.LoadCode(pageCode);
                    }
                }
                //CSScript.Evaluator.ReferenceAssembliesFromCode(Main.Qb.ProgramMainCode);
                //script = CSScript.Evaluator.LoadCode(Main.Qb.ProgramMainCode);

                /*
                script = CSScript.Evaluator
                    //.ReferenceAssemblyOf(this)
                    //.ReferenceAssemblyByName("qbookCsScript")
                    //.LoadMethod(fullCode)
                    .ReferenceAssembliesFromCode(usingsCode + Main.Qb.ProgramMainCode)
                    .ReferenceAssembliesFromCode("using static QbRoot;\r\n" + code)
                    //.LoadMethod("using static QbRoot;\r\n" + code)
                    //.LoadMethod("\r\n" + code)
                    //.LoadMethod("class Foo {}")
                    .LoadMethod(Main.Qb.ProgramMainCode)
                    ;
                */
                sw.Stop();
                //toolStripLabelInfo.Text = "Success: rebuild took " + sw.ElapsedMilliseconds.ToString() + "ms";
                //CsScript_Init();
                //LastBuildResult = null;
                return true;
            }
            catch (Exception ex)
            {
                //toolStripLabelInfo.Text = "#ERR: rebuild took " + sw.ElapsedMilliseconds.ToString() + "ms";
                //tabControl3.SelectedTab = tabPageOutput;
                //OutputWriteLine("--- ERRORS ---\r\n" + ex.Message);
                LastBuildResult = "#ERR in script code";
                QB.Logger.Error("#EX building C# code:" + ex.Message + "\r\n" + ex.StackTrace);
                throw ex;
                return false;
            }
            finally
            {
                IsBuilding = false;
                //Main.QB.Logger.InvalidateLog(); //HACK: otherwise no more log messages are received?!
                toast?.Close();
                sw.Stop();
                //this.Cursor = Cursors.Default;
            }
        }

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

            CsScriptInit();
            PyScriptInit();
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

        public static FormScintillaEditor ScintillaEditor = null;
        internal static void ShowFormScintillaEditor(oPage page = null)
        {
            if (!VerifyDeveloperLicense()) return;


            if (!Core.ScintillaEditor.Visible)
            {
                if (Core.ScintillaEditor.InvokeRequired)
                    Core.ScintillaEditor.Invoke((MethodInvoker)(() => Core.ScintillaEditor.Show()));
                else
                    Core.ScintillaEditor.Show();
            }
            else
            {
                if (Core.ScintillaEditor.InvokeRequired)
                    Core.ScintillaEditor.Invoke((MethodInvoker)(() => Core.ScintillaEditor.BringToFront()));
                else
                    Core.ScintillaEditor.BringToFront();
            }

            ScintillaEditor.SelectNodeByName(page.Name);

        }


        static qbook.CodeEditor.FormCodeEditor _codeEditorForm = null;
        internal static void ShowFormCodeEditor(oPage page = null)
        {
            if (!VerifyDeveloperLicense())
                return;

            try
            {
                Application.OpenForms[0].Cursor = Cursors.WaitCursor;
                if (_codeEditorForm == null || _codeEditorForm.IsDisposed || !_codeEditorForm.IsHandleCreated)
                {
                    _codeEditorForm = new qbook.CodeEditor.FormCodeEditor();
                    _codeEditorForm.Show();
                }
                _codeEditorForm.SelectPage(page);
                _codeEditorForm.BringToFront();
            }
            catch { }
            finally
            {
                Application.OpenForms[0].Cursor = Cursors.Default;
            }
        }
        internal static qbook.CodeEditor.FormCodeEditor FormCodeEditor
        {
            get { return _codeEditorForm; }
        }
        internal static bool IsFormCodeEditorVisible
        {
            get
            {
                return _codeEditorForm != null && _codeEditorForm.Visible;
            }
        }
        internal static void CloseFormCodeEditor()
        {
            if (_codeEditorForm != null)
            {
                _codeEditorForm.Close();
            }
        }
        internal static void FormCodeEditorRebuild(bool reinit = true)
        {
            if (_codeEditorForm != null)
            {
                if (reinit)
                    _codeEditorForm.ReInit();
                _codeEditorForm.DoRebuild();
            }
        }

        static void InitLogger()
        {
            //string logFilename = Main.Qb.Book.LogFilename.Replace("{date}", DateTime.Now.ToString("yyyy-MM-dd_HHmmss")).Replace('\\', '/');
            string logFilename = Path.Combine(qbook.Core.ThisBook.TempDirectory, "qbook.{date}.log");
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

            //logger.Info("Hello");
            //logger.Error("World");

        }

        static bool IsXml(string filePath)
        {
            try
            {
                using var reader = XmlReader.Create(filePath);
                while (reader.Read()) { } // Versucht, das gesamte Dokument zu lesen
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static MruFilesManager MruFilesManager = new MruFilesManager();
        internal static async Task OpenBookFolderAsync(string fullPath = @"T:\qSave")
        {
            try
            {

                MainForm.SetStatusText("opening qbook: " + fullPath);
                MruFilesManager.Add(fullPath);
                Properties.Settings.Default.MruFileList = MruFilesManager.GetMruCsvString();
                Properties.Settings.Default.Save();

                qbook.Core.ThisBook = await ScintillaEditor.LoadProjectAsync(fullPath, "");
                QB.Root.ActiveQbook = qbook.Core.ThisBook;

                InitLogger();
                QB.Logger.Info($"Opening qbook: {fullPath}");

                Book.OnStaticPropertyChangedEvent("new Book", "");


                qbook.Core.ThisBook.DesignMode = false;
                qbook.Core.ThisBook.TagMode = false;
                // Main.Qb.Book.SrcMode = false;
                qbook.Core.ThisBook.Recorder = false;

                //Bounds = Main.Qb.Book.Bounds;

                if (qbook.Core.ThisBook.Main == null)
                    qbook.Core.ThisBook.Main = new oControl();


                qbook.Core.ThisBook.Init();
                qbook.Core.UpdateProjectAssemblyQbRoot("Core.OpenProjectAsyncZip");

                MainForm.SetStatusText("initializing qbook...");
                qbook.Core.Init();
                if ((qbook.Core.ActualMain?.Objects?.Count ?? 0) > 0)
                    qbook.Core.SelectedPage = qbook.Core.ActualMain.Objects[0] as oPage;


                MainForm mainForm = (MainForm)Application.OpenForms["MainForm"];
                mainForm.InitView = true;
                mainForm.UpdateStartMenuItems();
                MainForm.SetStatusText("qbook loaded successfully!", 3000);

                ActualMain = ThisBook.Main;

                await ScintillaEditor.Rebuild();
                if (!ScintillaEditor.HasError)
                {
                    ScintillaEditor.Show();
                }
                else
                {
                    PageRuntime.RunAll();
                    ScintillaEditor.SetStatusText("[Build] running...");
                }



            }
            catch (Exception ex)
            {
                QB.Logger.Error("#EX opening qbook: " + ex.Message + (QB.Logger.ShowStackTrace ? ex.StackTrace : ""));
                MessageBox.Show(ex.Message, "ERROR OPENING QBOOK", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MainForm.SetStatusText("#ERR: loading qbook");
            }

        }
        internal static async Task OpenQbookAsync(string fullPath)
        {
            string filename = fullPath.GetFileName();
            string directory = fullPath.GetDirName();


            if (ScintillaEditor == null)
            {
                ScintillaEditor = new ScintillaEditor.FormScintillaEditor();
                await Task.Delay(1000);
            }


            if (!IsXml(fullPath))
            {
             //   string tempFolder = ScintillaEditor.ExtractZipToTemp(fullPath);
                MainForm.SetStatusText("opening qbook: " + filename);
                await OpenBookFolderAsync(fullPath.Replace(".qbook",".code"));
                QB.Logger.Info($"Opening qbook: {fullPath} from code folder");
            }
            else
            {
                await OpenQbookXMLAsync(fullPath);
                QB.Logger.Info($"Opening qbook: {fullPath} as xml");
            }
        }
        internal static async Task OpenQbookXMLAsync(string fullPath/*string directory, string filename*/)
        {
            string filename = fullPath.GetFileName();
            string directory = fullPath.GetDirName();

            bool autoBuild = true;
            bool autoRun = true;


            try
            {

                MainForm.SetStatusText("opening qbook: " + filename);

                if (!filename.ToLower().EndsWith(".qbook"))
                    filename += ".qbook";


                MruFilesManager.Add(fullPath);
                Properties.Settings.Default.MruFileList = MruFilesManager.GetMruCsvString();
                Properties.Settings.Default.Save();

                Console.WriteLine($"Opening qbook: {fullPath}");


                qbook.Core.ThisBook = Book.Deserialize(fullPath);

                QB.Root.ActiveQbook = qbook.Core.ThisBook;

                InitLogger();
                QB.Logger.Info($"Opening qbook: {fullPath}");

                Book.OnStaticPropertyChangedEvent("new Book", "");


                qbook.Core.ThisBook.DesignMode = false;
                qbook.Core.ThisBook.TagMode = false;
                // Main.Qb.Book.SrcMode = false;
                qbook.Core.ThisBook.Recorder = false;

                //Bounds = Main.Qb.Book.Bounds;

                if (qbook.Core.ThisBook.Main == null)
                    qbook.Core.ThisBook.Main = new oControl();

                qbook.Core.ActualMain = qbook.Core.ThisBook.Main;
                if (qbook.Core.ActualMain.Objects.Count == 0)
                    qbook.Core.ActualMain.Add(new oPage("", "page 1"));

                //HALE: no longer necessary?! formulas moved in oItem.Settings
                //Main.Qb.ScriptingEngine.RebuildBookFormulas();


                qbook.Core.ThisBook.Init();
                qbook.Core.UpdateProjectAssemblyQbRoot("Core.OpenProjectAsyncXML");

                MainForm.SetStatusText("initializing qbook...");
                qbook.Core.Init();
                if ((qbook.Core.ActualMain?.Objects?.Count ?? 0) > 0)
                    qbook.Core.SelectedPage = qbook.Core.ActualMain.Objects[0] as oPage;

                //2025-09-06 STFU

                await Core.ScintillaEditor.LoadXML();


                MainForm mainForm = (MainForm)Application.OpenForms["MainForm"];
                mainForm.InitView = true;
                mainForm.UpdateStartMenuItems();
                MainForm.SetStatusText("qbook loaded successfully!", 3000);



            }
            catch (Exception ex)
            {
                QB.Logger.Error("#EX opening qbook: " + ex.Message + (QB.Logger.ShowStackTrace ? ex.StackTrace : ""));
                MessageBox.Show(ex.Message, "ERROR OPENING QBOOK", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MainForm.SetStatusText("#ERR: loading qbook");
            }


            if ((System.Windows.Forms.Control.ModifierKeys == (Keys.Control | Keys.Shift)))
            {
                //don't auto build/run if ctrl-shift are beeing held down at startup
                autoBuild = false;
                autoRun = false;
            }

            // await ScintillaEditor.CreateAssemblyFromTree();

            if (autoBuild)
            {
                await ScintillaEditor.Rebuild();
                if (!ScintillaEditor.HasError)
                {
                    ScintillaEditor.Show();
                }
                else
                {
                    PageRuntime.RunAll();
                    ScintillaEditor.SetStatusText("[Build] running...");
                }

            }


            return;


            //write a temp file, and delete after successfull build/run of the qbook.
            //if build/run fails and qbook crashes, show info on next startup and offer not do build/run qbook automatically...
            var buildRunTrialPath = fullPath + "~buildrun~";
            if (File.Exists(buildRunTrialPath))
            {
                var dr = MessageBox.Show("The most recent attempt to build this qbook failed!\r\n\r\n[Retry]\tto try and rebuild now\r\n[Cancel]\tto fix the problem(s), then manually rebuild", "LAST BUILD/RUN FAILED", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
                if (dr == DialogResult.Cancel)
                {
                    autoBuild = false;
                    autoRun = false;
                }
                //File.Delete(buildRunTrialPath);
            }

            //if (autoBuildRun)
            {
                try
                {

                    _ = Task.Run((Func<Task>)(async () =>
                    {
                        if (autoBuild)
                        {
                            //File.Create(buildRunTrialPath);
                            try
                            {
                                using (File.Create(buildRunTrialPath)) { } //HACK: otherwise the file is locked
                                bool firstPageContainsCsCode = !string.IsNullOrEmpty(qbook.Core.ThisBook.Main.Objects.FirstOrDefault()?.CsCode);
                                QB.UI.Toast infoToast = null;
                                if (firstPageContainsCsCode)
                                {
                                    Application.OpenForms[0].EnsureBeginInvoke((System.Action)(() =>
                                    {
                                        infoToast = QB.UI.Toast.Show($"{Path.GetFileNameWithoutExtension(filename)}\r\n\r\n// please wait... //", "starting qbook", timeout: 20000, backColor: System.Drawing.Color.Gainsboro);
                                    }));
                                }
                                QB.Logger.Debug($"rebuilding and calling qbook.Init()");
                                await qbook.Core.CsScriptRebuildAll("Initializing");
                                Application.OpenForms[0].EnsureBeginInvoke(() =>
                                {
                                    infoToast?.Close();
                                });
                            }
                            catch (Exception ex)
                            {
                                QB.Logger.Error($"#ERR error building qbook: " + ex.Message);
                            }
                        }

                        if (autoRun)
                        {
                            if (Program.Args == null || !Program.Args.Contains("-norun"))
                            {
                                QB.Logger.Debug($"calling qbook.Run()");
                                try
                                {
                                    _ = qbook.Core.RunCsScript_Run();
                                }
                                catch (Exception ex)
                                {
                                    QB.Logger.Error($"#ERR error running qbook: " + ex.Message);
                                }
                            }
                        }

                        if (File.Exists(buildRunTrialPath))
                        {
                            try
                            {
                                File.Delete(buildRunTrialPath);
                            }
                            catch (Exception ex)
                            {
                                //file locked?! -> should be fixed now...
                                QB.Logger.Warn($"#WRN cannot delete {buildRunTrialPath}: " + ex.Message);
                            }
                        }
                    }));
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message, "ERROR OPENING QBOOK", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    if (File.Exists(buildRunTrialPath))
                    {
                        try
                        {
                            File.Delete(buildRunTrialPath);
                        }
                        catch (Exception ex)
                        {
                            QB.Logger.Warn($"#WRN cannot delete(2) {buildRunTrialPath}: " + ex.Message);
                        }
                    }
                }
            }
        }
        internal static async Task SaveThisBook()
        {
            if (qbook.Core.ThisBook != null)
            {
                MainForm.SetStatusText("saving qbook: " + qbook.Core.ThisBook.Filename);
                await ScintillaEditor.SaveInFolder();
                qbook.Core.ThisBook.Modified = false;
                qbook.Properties.Settings.Default.Save();
                MainForm.SetStatusText("qbook saved successfully!", 3000);
            }
        }

        internal static void ShowOpenQbookFileDialog(object sender)
        {
            if (qbook.Core.ThisBook != null && qbook.Core.ThisBook.Modified)
            {
                DialogResult result = MessageBox.Show("save " + qbook.Core.ThisBook.Filename + "?", "qBook NOT SAVED", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                    qbook.Core.ThisBook.Serialize();
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
                    qbook.Core.ThisBook.Serialize();
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
