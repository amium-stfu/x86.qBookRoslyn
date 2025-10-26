using CSScripting;
using DevExpress.DocumentView;
using DevExpress.Drawing.Printing.Internal;
using DevExpress.Utils.DPI;
using DevExpress.XtraPrinting;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Build.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using QB.Controls;
using qbook.CodeEditor;
using qbook.UI;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Windows.Documents;
using System.Windows.Forms;
using static IronPython.Modules._ast;
using static qbook.Core;
using static ScintillaNET.Style;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using RoslynDocument = Microsoft.CodeAnalysis.Document;
using Style = ScintillaNET.Style;

namespace qbook.ScintillaEditor
{

    
    
    public partial class FormScintillaEditor : Form
    {
        RoslynService _roslyn = new RoslynService();

        CodeNode RootNode;

        DataGridView GridViewDiagnosticOutput;
        public DataTable TblFindReplaceOutputs = new DataTable();

        DataTable tblBuildDiagnosic = new DataTable();
        private ControlFindReplace _findReplaceControl;

        public FormScintillaEditor()
        {
            
            InitializeComponent();

            tblBuildDiagnosic.Columns.Add("Page", typeof(string));
            tblBuildDiagnosic.Columns.Add("Class", typeof(string));
            tblBuildDiagnosic.Columns.Add("Position", typeof(string));
            tblBuildDiagnosic.Columns.Add("Length", typeof(int));
            tblBuildDiagnosic.Columns.Add("Type", typeof(string));
            tblBuildDiagnosic.Columns.Add("Description", typeof(string));
            tblBuildDiagnosic.Columns.Add("Node", typeof(CodeNode));

            vBarMethodes.Init(gridViewMethodes);
            vBarProjectTree.Init(ProjectTree, hideNativeScrollbar: true);

            RoslynDiagnostic.InitDiagnostic();
            RosylnSignatureHelper.Init(_roslyn);
           
            RoslynAutoComplete.Init(_roslyn);
          

            InitGridViews();
      

            ApplyLightTheme();

            panelOutput.Controls.Clear();
            panelOutput.Controls.Add(GridViewDiagnosticOutput);
            vBarOutputs.Init(GridViewDiagnosticOutput);
            UpdateOutputButtons();

        }

        //============================ Load Book ============================
        //public async Task LoadBook()
        //{

        //    _roslyn.Reset();
        //    GC.Collect();
        //    GC.WaitForPendingFinalizers();
        //    GC.Collect();

        //    _roslyn = new RoslynService();
        //    ProjectTree.Nodes.Clear();

        //    //   tblBuildOutputs.Clear();
        //    ProjectTree.BeginUpdate();
        //    ProjectTree.ImageList = imageList1;
        //    ProjectTree.Nodes.Clear();

        //    RootNode = new CodeNode(qbook.Core.ThisBook.Filename)
        //    {
        //        ImageIndex = 1
        //    };

        //    ProjectTree.Font = new Font("Calibri", 12);
        //    ProjectTree.Nodes.Add(RootNode);

        //    string program = "namespace QB\r\n{\r\n   public static class Program {\r\n";
        //    var roslynFiles = new List<(string fileName, string code)>();
        //    int pageCount = -1;
        //    string firstFile = null;

        //    foreach (oPage page in qbook.Core.ActualMain.Objects.OfType<oPage>())
        //    {

        //        string className = "class_" + page.Name + ".@class_" + page.Name;

        //        pageCount++;
        //        string code = page.CsCode;

        //        List<string> includes = ReplaceIncludesWithBlock(ref code);

        //        string pageCode = "namespace class_" + page.Name + "{\r\n//<CodeStart>\r\n";
        //        pageCode += code;
        //        pageCode += "\r\n//<CodeEnd>\r\n}";

        //        //int index = code.IndexOf("public class");
        //        //string insertText = "\r\nnamespace class_" + page.Name + "\r\n{\r\n";
        //        //code = code.Insert(index - 1, insertText) + "\r\n}";

        //        program += "   public static " + className + " " + page.Name + " { get; } = new " + className + "();\r\n";

        //        string fileName = $"class_{page.Name}.cs";
        //        roslynFiles.Add((fileName, code));

        //        CodeNode pageNode = new CodeNode(page, fileName, CodeNode.NodeType.Page, page.Name);
        //        pageNode.Editor.Text = pageCode;
        //        pageNode.Editor.EmptyUndoBuffer();
        //        pageNode.Active = true;
        //        pageNode.Editor.GoToDefinition = () => GoToDefinitionAsync();
        //        pageNode.Editor.RenameSymbol = () => RenameSymbolAsync();
        //        pageNode.Editor.KeyDown += (s, e) =>
        //        {

        //            if (e.Control && e.KeyCode == Keys.H)
        //            {
        //                e.SuppressKeyPress = true;
        //                btnFindReplace.PerformClick();

        //            }

        //            if (e.Control && e.KeyCode == Keys.F)
        //            {
        //                e.SuppressKeyPress = true;
        //                btnFind.PerformClick();

        //            }

        //        };

        //        ProjectTree.Nodes[0].Nodes.Add(pageNode);

        //        if (firstFile == null)
        //            firstFile = fileName;

        //        var lines = code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        //        var usings = lines
        //            .TakeWhile(l => !l.TrimStart().StartsWith("public class"))
        //            .Where(l => l.TrimStart().StartsWith("using"))
        //            .ToList();


        //        int subCount = 0;

        //        foreach (var subClass in page.CsCodeExtra)
        //        {
        //            subCount++;
        //            string sub = string.Join("\r\n", usings)
        //                + "\r\n\r\nnamespace class_" + page.Name
        //                + "\r\n{\r\n//<CodeStart>\r\n"
        //                + subClass.Value
        //                + "\r\n//<CodeEnd>\r\n"
        //                + "\r\n}";

        //            string subFileName = $"sub_{page.Name}_{subClass.Key}.cs";

        //            CodeNode subNode = new CodeNode(page, subFileName, CodeNode.NodeType.SubCode, subClass.Key, pageNode);
        //            subNode.Editor.Text = sub;
        //            subNode.Editor.EmptyUndoBuffer();
        //            subNode.Editor.GoToDefinition = () => GoToDefinitionAsync();
        //            subNode.Editor.RenameSymbol = () => RenameSymbolAsync();
        //            subNode.Editor.KeyDown += (s, e) =>
        //            {

        //                if (e.Control && e.KeyCode == Keys.H)
        //                {
        //                    e.SuppressKeyPress = true;
        //                    btnFindReplace.PerformClick();

        //                }

        //                if (e.Control && e.KeyCode == Keys.F)
        //                {
        //                    e.SuppressKeyPress = true;
        //                    btnFind.PerformClick();

        //                }
        //            };

        //            subNode.Active = includes.Contains(subClass.Key);
        //            if (subNode.Active)
        //                roslynFiles.Add((subFileName, sub));

        //            subNode.ImageIndex = subNode.Active ? 3 : 4;
        //            ProjectTree.Nodes[0].Nodes[pageCount].Nodes.Add(subNode);
        //            subNode.Editor.ApplyLightTheme();
        //        }
        //    }

        //    program += "   }\r\n}";
        //    roslynFiles.Add(("Program.cs", program));

        //    roslynFiles.Add(("GlobalUsing.cs", "global using static QB.Program;"));


        //    string root = AppDomain.CurrentDomain.BaseDirectory;
        //    string baseDir = System.IO.Path.Combine(root, "libs");

        //    var dllFiles = System.IO.Directory.GetFiles(baseDir, "*.dll").ToArray();
        //    string[] exclude = { "Microsoft.CodeAnalysis" };

        //    dllFiles = dllFiles
        //        .Where(path => !exclude.Any(prefix => System.IO.Path.GetFileNameWithoutExtension(path)
        //            .StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        //        .ToArray();

        //    var references = dllFiles.Select(path => MetadataReference.CreateFromFile(path)).ToList();
        //    await _roslyn.LoadInMemoryProjectAsync(roslynFiles, references);



        //    // Verknüpfe alle EditorNodes mit den Roslyn Documents

        //    foreach (CodeNode node in ProjectTree.Nodes[0].Nodes)
        //    {
        //        if (node is CodeNode editorNode)
        //        {
        //            editorNode.RoslynDoc = _roslyn.GetDocument(editorNode.FileName);
        //            editorNode.Adhoc.Workspace = _roslyn.GetWorkspace;
        //            editorNode.Adhoc.Id = _roslyn.GetProjectId;
        //        }

        //        foreach (CodeNode sub in node.Nodes)
        //        {
        //            if (sub is CodeNode subNode)
        //            {
        //                subNode.RoslynDoc = _roslyn.GetDocument(subNode.FileName);
        //                subNode.Adhoc.Workspace = _roslyn.GetWorkspace;
        //                subNode.Adhoc.Id = _roslyn.GetProjectId;
        //            }
        //        }
        //    }

        //    Core.ProgramWorkingDir = "InMemory";
        //    QB.Logger.Info("Using In-Memory Working Directory");
        //    ProjectTree.EndUpdate();
        //}


        #region Load Save Book

        public async Task LoadXML()
        {
            _roslyn.Reset();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            _roslyn = new RoslynService();
            ProjectTree.Nodes.Clear();

            ProjectTree.BeginUpdate();
            ProjectTree.ImageList = imageList1;
            ProjectTree.Nodes.Clear();

            RootNode = new CodeNode(qbook.Core.ThisBook.Filename)
            {
                ImageIndex = 1
            };

            ProjectTree.Font = new Font("Calibri", 12);
            ProjectTree.Nodes.Add(RootNode);

            string program = "namespace QB\r\n{\r\n\tpublic static class Program \r\n\t{\r\n";
            var roslynFiles = new List<(string fileName, string code)>();
            int pageCount = -1;
            string firstFile = null;

            List<string> Pages = new List<string>();

            int CodeIndex = 0;
            int PageIndex = 0;
            foreach (oPage page in qbook.Core.ActualMain.Objects.OfType<oPage>())
            {
                page.Includes.Clear();
                CodeIndex = 0;
                PageIndex++;
                string className = "Definition" + page.Name + ".qPage";
                pageCount++;
                string code = page.CsCode;

                List<string> includes = CutInludesBlock(ref code);
                //List<string> includes = new List<string>();

                string pageCode = "namespace Definition" + page.Name + "{\r\n//<CodeStart>\r\n";
                

                pageCode += Regex.Replace(code, @"public class\s+@class_\w+", "public class qPage");
                pageCode += "\r\n//<CodeEnd>\r\n}";

                program += "\t\tpublic static " + className + " " + page.Name + " { get; } = new " + className + "();\r\n";

                pageCode = ReplaceClassToDefinition(pageCode);

                Pages.Add(page.Name);

               // string PageFileName = $"{PageIndex}.0_Page.cs";
                string PageFileName = $"{page.Name}.qPage.cs";
                roslynFiles.Add((PageFileName, code));

                Core.ThisBook.PageOrder.Add(page.Name);
                page.CodeOrder.Add(PageFileName);

                CodeNode pageNode = new CodeNode(page, PageFileName, CodeNode.NodeType.Page, page.Name);
                pageNode.CodeIndex = PageIndex;
                page.OrderIndex = PageIndex;
                pageNode.Editor.Text = pageCode;
                pageNode.Editor.EmptyUndoBuffer();
                pageNode.Active = true;
                pageNode.Editor.GoToDefinition = () => GoToDefinitionAsync();
                pageNode.Editor.RenameSymbol = () => RenameSymbolAsync();

                pageNode.Editor.KeyDown += (s, e) =>
                {
                    if (e.Control && e.KeyCode == Keys.H)
                    {
                        e.SuppressKeyPress = true;
                        btnFindReplace.PerformClick();
                    }
                    if (e.Control && e.KeyCode == Keys.F)
                    {
                        e.SuppressKeyPress = true;
                        btnFind.PerformClick();
                    }
                };

                ProjectTree.Nodes[0].Nodes.Add(pageNode);

                if (firstFile == null)
                    firstFile = PageFileName;

                var lines = code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                var usings = lines
                    .TakeWhile(l => !l.TrimStart().StartsWith("public class"))
                    .Where(l => l.TrimStart().StartsWith("using"))
                    .ToList();

                foreach (var subClass in page.CsCodeExtra)
                {
                   
                    CodeIndex++;
                    string sub = "\r\n\r\nnamespace Definition" + page.Name
                        + "\r\n{\r\n//<CodeStart>\r\n"
                        + string.Join("\r\n", usings)
                        + subClass.Value
                        + "\r\n//<CodeEnd>\r\n"
                        + "\r\n}";

                    sub = ReplaceClassToDefinition(sub);

                   // string subFileName = $"{PageIndex}.{CodeIndex}_{subClass.Key}.cs";
                    string subFileName = $"{page.Name}.{subClass.Key}.cs";
                    page.CodeOrder.Add(subFileName);
                    CodeNode subNode = new CodeNode(page, subFileName, CodeNode.NodeType.SubCode, subClass.Key, pageNode);
                    subNode.CodeIndex = CodeIndex;
                    subNode.Editor.Text = sub;
                    subNode.Editor.EmptyUndoBuffer();
                    subNode.Editor.GoToDefinition = () => GoToDefinitionAsync();
                    subNode.Editor.RenameSymbol = () => RenameSymbolAsync();

                    subNode.Editor.KeyDown += (s, e) =>
                    {
                        if (e.Control && e.KeyCode == Keys.H)
                        {
                            e.SuppressKeyPress = true;
                            btnFindReplace.PerformClick();
                        }
                        if (e.Control && e.KeyCode == Keys.F)
                        {
                            e.SuppressKeyPress = true;
                            btnFind.PerformClick();
                        }
                    };

                    subNode.Active = includes.Contains(subClass.Key);
                    if (subNode.Active)
                    {
                        roslynFiles.Add((subFileName, sub));
                        page.Includes.Add(subFileName);
                    }

                    subNode.ImageIndex = subNode.Active ? 3 : 4;
                    ProjectTree.Nodes[0].Nodes[pageCount].Nodes.Add(subNode);
                    subNode.Editor.ApplyLightTheme();
                }

                //page.CodeOrder.Reverse();

                Debug.WriteLine("======= includes =============");
                foreach (string f in page.Includes) Debug.WriteLine(f);
            }

            //Core.ThisBook.PageOrder.Reverse();

            program += "\t\tpublic static void Initialize()\r\n\t\t{\r\n";

            foreach (string p in Pages)
            {
                program += "\t\t\t" + p + ".Initialize();\r\n";
            }

            program += "\t\t}\r\n";



            program += "\t\tpublic static void Run()\r\n\t\t{\r\n";

            foreach (string p in Pages)
            {
                program += "\t\t\t" + p + ".Run();\r\n";
            }

            program += "\t\t}\r\n";

            program += "\t\tpublic static void Destroy()\r\n\t\t{\r\n";

            foreach (string p in Pages)
            {
                program += "\t\t\t" + p + ".Destroy();\r\n";
            }

            program += "\t\t}\r\n";


            program += "   }\r\n}";
            roslynFiles.Add(("Program.cs", program));


  

            CodeNode Program = new CodeNode(null, "Program.cs", CodeNode.NodeType.Program, "Program.cs");
            Program.CodeIndex = 0;
            Program.Editor.Text = program;
            Program.Editor.EmptyUndoBuffer();
            Program.Active = true;
            Program.Editor.ReadOnly = true;
            Program.Editor.GoToDefinition = () => GoToDefinitionAsync();
            Program.Editor.RenameSymbol = () => RenameSymbolAsync();
            RootNode.Nodes.Add(Program);



            Debug.WriteLine("=========  Program ===========");
            Debug.WriteLine(program);
            Debug.WriteLine("==============================");
            roslynFiles.Add(("GlobalUsing.cs", "global using static QB.Program;"));
            CodeNode Global = new CodeNode(null, "GlobalUsing.cs", CodeNode.NodeType.Program, "GlobalUsing.cs");
            Global.CodeIndex = 0;
            Global.Editor.Text = "global using static QB.Program;";
            Global.Editor.EmptyUndoBuffer();
            Global.Active = true;
            Global.Editor.ReadOnly = true;
            Global.Editor.GoToDefinition = () => GoToDefinitionAsync();
            Global.Editor.RenameSymbol = () => RenameSymbolAsync();
            RootNode.Nodes.Add(Global);


            // ==============================================================
            // 🔧 Korrigierter Referenz-Aufbau (nur managed Assemblies!)
            // ==============================================================


            List<MetadataReference> references = new List<MetadataReference>();

            // Basisreferenzen aus dem laufenden .NET
            references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(System.Windows.Forms.Form).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(System.Drawing.Point).Assembly.Location));

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

            // ==============================================================
            // Projekt erzeugen
            // ==============================================================
            await _roslyn.LoadInMemoryProjectAsync(roslynFiles, references);

            // Verknüpfe alle EditorNodes mit den Roslyn Documents
            foreach (System.Windows.Forms.TreeNode node in ProjectTree.Nodes[0].Nodes)
            {
                if (node is CodeNode editorNode)
                {
                    editorNode.RoslynDoc = _roslyn.GetDocumentByFilename(editorNode.FileName);
                    editorNode.Adhoc.Workspace = _roslyn.GetWorkspace;
                    editorNode.Adhoc.Id = _roslyn.GetProjectId;
                }

                foreach (System.Windows.Forms.TreeNode sub in node.Nodes)
                {
                    if (sub is CodeNode subNode)
                    {
                        subNode.RoslynDoc = _roslyn.GetDocumentByFilename(subNode.FileName);
                        subNode.Adhoc.Workspace = _roslyn.GetWorkspace;
                        subNode.Adhoc.Id = _roslyn.GetProjectId;
                    }
                }
            }

            Program.RoslynDoc = _roslyn.GetDocumentByFilename("Program.cs");
            Program.Adhoc.Workspace = _roslyn.GetWorkspace;
            Program.Adhoc.Id = _roslyn.GetProjectId;

            Global.RoslynDoc = _roslyn.GetDocumentByFilename("GlobalUsing.cs");
            Global.Adhoc.Workspace = _roslyn.GetWorkspace;
            Global.Adhoc.Id = _roslyn.GetProjectId;


            Core.ProgramWorkingDir = "InMemory";
            QB.Logger.Info("Using In-Memory Working Directory");
            ProjectTree.EndUpdate();
        }


        private string ReplaceClassToDefinition(string code)
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


        public class qBookDefinition
        {
            public string ProjectName { get; set; } = "Unnamed";
            public string Version { get; set; } = "0.1.0";
            public string VersionHistory { get; set; } = "";
            public long VersionEpoch { get; set; } = 0;
            public bool StartFullScreen { get; set; } = false;
            public bool HidPageMenuBar { get; set; } = false;
            public string PasswordAdmin { get; set; } = null; //overrides the default Admin-Password
            public string PasswordService { get; set; } = null; //overrides the default Service-Password
            public string PasswordUser { get; set; } = null; //overrides the default User-Password
            public string Directory { get; set; } = null;
            public string Filename { get; set; } = null;
            public string SettingsDirectory { get; set; } = null;
            public string DataDirectory { get; set; } = null;
            public string TempDirectory { get; set; } = null;
            public string Language { get; set; } = null;

            public List<string > PageOrder { get; set; } = new List<string>();


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


        public async Task SaveInFolder()
        {
            string uri = Path.Combine(Core.ThisBook.Directory, Core.ThisBook.Filename.Replace(".qbook","") +".code");
            Directory.CreateDirectory(uri);

            string link = "SaveDate: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            File.WriteAllText(Path.Combine(Core.ThisBook.Directory, Core.ThisBook.Filename), link);

            await SaveProjectAsync(uri);
        }

        public async Task SaveProjectAsync(string newFile = @"T:\qSave")
        {
            System.Windows.Forms.TreeView projectTree = ProjectTree;

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
                TempDirectory = Core.ThisBook.TempDirectory,
                Language = Core.ThisBook.Language,
                PageOrder = Core.ThisBook.PageOrder
            };

            // 🧠 Durch alle CodeNodes iterieren
            foreach (CodeNode node in ProjectTree.Nodes[0].Nodes)
            {
                string saveName = "";
                int PageIndex = -1;
                if (node.Type == CodeNode.NodeType.Page)
                {
                    PageIndex = node.Page.OrderIndex;
                    CodeNode pageNode = node;
                    string pagePath = Path.Combine(codeDir, $"{node.Text}");
                    Directory.CreateDirectory(pagePath);

                    saveName = $"{node.FileName}";

                   // string csPath = Path.Combine(pagePath, $"{node.FileName}");
                    string csPath = Path.Combine(pagePath, saveName);

                    string jsonPath = Path.Combine(pagePath, $"oPage.json");

                    // 1️⃣ Code speichern
                    var text = node.Editor.Text;
                    System.IO.File.WriteAllText(csPath, text);

                    // 2️⃣ Layout (oPage) speichern
                    if (node.Page != null && node.Type == CodeNode.NodeType.Page)
                    {
           
                        foreach (CodeNode sub in node.Nodes)
                        {
                            saveName = $"{sub.FileName}";
                           // csPath = Path.Combine(pagePath, $"{sub.FileName}");
                            csPath = Path.Combine(pagePath, saveName);
                            text = sub.Editor.Text;
                            System.IO.File.WriteAllText(csPath, text);
                        }
                    }

                    var dto = new PageDefinition
                    {
                        Name = node.Page.Name,
                        Text = node.Page.Text,

                        OrderIndex = node.Page.OrderIndex,
                  
                        Hidden = node.Page.Hidden,
                        Format = node.Page.Format,
                        Includes = node.Page.Includes,
                        Section = node.Page.Section,
                        Url = node.Page.Url,
                        CodeOrder = node.Page.CodeOrder,

                    };

                    string oPageJson = JsonConvert.SerializeObject(dto, Formatting.Indented);
                    File.WriteAllText(Path.Combine(pagePath, "oPage.json"), oPageJson);
                }
            }

            var code = await _roslyn.GetDocumentTextAsync("Program.cs");
            string path = Path.Combine(newFile, "Program.cs");
            File.WriteAllText(path, code);

            code = await _roslyn.GetDocumentTextAsync("GlobalUsing.cs");
            path = Path.Combine(newFile, "GlobalUsing.cs");
            File.WriteAllText(path, code);

            // 4️⃣ Projektbeschreibung schreiben
            string projectJson = JsonConvert.SerializeObject(project, Formatting.Indented);
            File.WriteAllText(Path.Combine(newFile, "Book.json"), projectJson);

          
        }


        public async Task<Book> LoadProjectAsync(string folderPath,string bookname)
        {
            PageRuntime.DestroyAll();

            _roslyn.Reset();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            _roslyn = new RoslynService();

            ProjectTree.BeginUpdate();
            ProjectTree.ImageList = imageList1;
            ProjectTree.Nodes.Clear();

            RootNode = new CodeNode(bookname)
            {
                ImageIndex = 1
            };

            ProjectTree.Font = new Font("Calibri", 12);
            ProjectTree.Nodes.Add(RootNode);

            Book newBook = new Book();
            newBook.Main = new oControl();

            var roslynFiles = new List<(string fileName, string code)>();
            string program = File.ReadAllText(Path.Combine(folderPath, "Program.cs"));
            roslynFiles.Add(("Program.cs", program));
            string globalUsing = File.ReadAllText(Path.Combine(folderPath, "GlobalUsing.cs"));
            roslynFiles.Add(("GlobalUsing.cs", globalUsing));

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

            List<CodeNode> pages = new List<CodeNode>();

            foreach (string page in reversePageOrder)
            {
                CodeNode pageNode = null;
                oPage opage = null;
                string pageFolder = Path.Combine(folderPath, "Pages", page);
                string oPageJson = File.ReadAllText(Path.Combine(pageFolder, "oPage.json"));
                opage = oPageFromString(oPageJson);

                string filename = page + ".qPage.cs";
                pageNode = new CodeNode(opage, filename, CodeNode.NodeType.Page, page);
                pageNode.Editor.Text = File.ReadAllText(Path.Combine(pageFolder, filename));
                pageNode.Editor.EmptyUndoBuffer();
                pageNode.Active = true;
                pageNode.Editor.GoToDefinition = () => GoToDefinitionAsync();
                pageNode.Editor.RenameSymbol = () => RenameSymbolAsync();
                pageNode.CodeIndex = newBook.PageOrder.IndexOf(page);

                List<string> reverseCodeOrder = opage.CodeOrder.AsEnumerable().Reverse().ToList();

                foreach (string codeFile in reverseCodeOrder)
                {
                    if (codeFile.EndsWith("qPage.cs")) continue;
                    CodeNode sub = new CodeNode(opage, codeFile, CodeNode.NodeType.SubCode, codeFile, pageNode);
                    sub.Editor.Text = File.ReadAllText(Path.Combine(pageFolder, codeFile));
                    sub.Editor.EmptyUndoBuffer();
                    sub.Active = opage.Includes.Contains(codeFile);
                    sub.Editor.GoToDefinition = () => GoToDefinitionAsync();
                    sub.Editor.RenameSymbol = () => RenameSymbolAsync();
                    sub.CodeIndex = opage.CodeOrder.IndexOf(codeFile);
                    pageNode.Nodes.Add(sub);


                }
                pages.Add(pageNode);
            }
            pages.Reverse();

            foreach(CodeNode p in pages)
            {
                RootNode.Nodes.Add(p);
                newBook.Main.Objects.Add(p.Page);

            }

            //string codeDir = Path.Combine(folderPath, "Pages");

            //var pageFolders = Directory.GetDirectories(codeDir)
            //.OrderBy(d =>
            //{
            //    var name = Path.GetFileName(d);
            //    var prefix = name.Split('_')[0];
            //    double index = 0;
            //    double.TryParse(prefix.Replace('.', ','), out index);
            //    return index;
            //})
            //.ToList();

            //int pageCounter = 0;
            //int codeCounter = 0;

            //foreach (string pageFolder in pageFolders)
            //{
            //    pageCounter ++;
            //    codeCounter = -1;
            //    string pageFolderName = Path.GetFileName(pageFolder);
            //    int PageIndex = int.Parse(pageFolderName.Split('_')[0].Split('.')[0]);
            //    string pageName = pageFolder.Split('_')[1];


            //    CodeNode pageNode = null;
            //    oPage opage = null;
            //    var files = Directory.GetFiles(pageFolder)
            //    .OrderBy(f =>
            //    {
            //        var name = Path.GetFileNameWithoutExtension(f);
            //        var prefix = name.Split('_')[0];
            //        double index = 0;
            //        double.TryParse(prefix.Replace('.', ','), out index);
            //        return index;
            //    })
            //    .ToList();

            //    foreach (string file in files)

            //    {

            //        if (file.EndsWith(".cs"))
            //        {
            //            codeCounter++;
            //            string code = File.ReadAllText(file);
            //            string fileName = Path.GetFileName(file);
            //            fileName = fileName.Split('_')[1];
            //            roslynFiles.Add((fileName, code));

            //            //int CodeIndex = int.Parse(fileName.Split('_')[0].Split('.')[1]);
            //            //string CodeName = fileName.Split('_')[1];

            //            int CodeIndex = codeCounter;
            //            string CodeName = fileName.Replace(".cs","");

            //            if (CodeIndex == 0)
            //            {
            //                string oPageJson = File.ReadAllText(Path.Combine(pageFolder, "oPage.json"));
            //                opage = oPageFromString(oPageJson);

            //                pageNode = new CodeNode(opage, fileName, CodeNode.NodeType.Page, CodeName);
            //                pageNode.Editor.Text = File.ReadAllText(Path.Combine(codeDir,pageFolder,file));
            //                pageNode.Editor.EmptyUndoBuffer();
            //                pageNode.Active = true;
            //                pageNode.Editor.GoToDefinition = () => GoToDefinitionAsync();
            //                pageNode.Editor.RenameSymbol = () => RenameSymbolAsync();
            //                pageNode.CodeIndex = CodeIndex;
            //            }
            //            else
            //            {
            //                CodeNode sub = new CodeNode(opage, fileName, CodeNode.NodeType.SubCode, CodeName, pageNode);
            //                sub.Editor.Text = File.ReadAllText(Path.Combine(codeDir, pageFolder, file));
            //                sub.Editor.EmptyUndoBuffer();
            //                sub.Active = opage.Includes.Contains(fileName);
            //                sub.Editor.GoToDefinition = () => GoToDefinitionAsync();
            //                sub.Editor.RenameSymbol = () => RenameSymbolAsync();
            //                sub.CodeIndex = CodeIndex;
            //                pageNode.Nodes.Add(sub);
            //            }
            //        }
            //    }
            //    RootNode.Nodes.Add(pageNode);
            //    newBook.Main.Objects.Add(pageNode.Page);
            //}

            List<MetadataReference> references = new List<MetadataReference>();

            // Basisreferenzen aus dem laufenden .NET
            references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(System.Windows.Forms.Form).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(System.Drawing.Point).Assembly.Location));

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

            // ==============================================================
            // Projekt erzeugen
            // ==============================================================

            Debug.WriteLine("========= Files ==========");
            foreach (var rf in roslynFiles)
            {
                Debug.WriteLine("======== " +rf.fileName);
                Debug.WriteLine(rf.code);
                Debug.WriteLine("==");
            }


            await _roslyn.LoadInMemoryProjectAsync(roslynFiles, references);


            // Rebind all CodeNodes to new Roslyn documents
            foreach (CodeNode node in RootNode.Nodes)
            {
                if (node.Type == CodeNode.NodeType.Page)
                {
                    node.RoslynDoc = _roslyn.GetDocumentByFilename(node.FileName);
                    node.Adhoc.Workspace = _roslyn.GetWorkspace;
                    node.Adhoc.Id = _roslyn.GetProjectId;


                    foreach (CodeNode sub in node.Nodes)
                    {
                        if (sub.Type == CodeNode.NodeType.SubCode)
                        {
                            sub.RoslynDoc = _roslyn.GetDocumentByFilename(sub.FileName);
                            sub.Adhoc.Workspace = _roslyn.GetWorkspace;
                            sub.Adhoc.Id = _roslyn.GetProjectId;
                        }
                    }
                }
            }

            //Core.ThisBook.Init();
            //Core.UpdateProjectAssemblyQbRoot();

            //Core.ProgramWorkingDir = "InMemory";
            QB.Logger.Info("Using In-Memory Working Directory");
            ProjectTree.EndUpdate();

            return newBook;

        }



        private oPage oPageFromString(string json)
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




        #endregion

        public async Task CreateAssemblyFromTree(System.Windows.Forms.TreeView project)
        {
            Debug.WriteLine("======== CreateAssemblyFromTree ========");

            // 1️⃣ Alte Assembly + Threads zerstören
            try
            {
                PageRuntime.DestroyAll();
                qbook.Core.ActiveCsAssembly = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Debug.WriteLine("[Rebuild] Old runtime destroyed.");
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[Rebuild] Destroy failed: {ex.Message}");
            }

            // 2️⃣ Roslyn-Service resetten
            _roslyn = new RoslynService();
            _roslyn.Reset();  // <-- wichtig, Workspace komplett leeren

            // 3️⃣ Code und Program-Klasse aus TreeView aufbauen
            var pages = new List<string>();
            var roslynFiles = new List<(string fileName, string code)>();
            var sbProgram = new StringBuilder();
            sbProgram.AppendLine("namespace QB");
            sbProgram.AppendLine("{");
            sbProgram.AppendLine("\tpublic static class Program");
            sbProgram.AppendLine("\t{");

            foreach (CodeNode node in ProjectTree.Nodes)
            {
                if (node.Type == CodeNode.NodeType.Page)
                {
                    pages.Add(node.Text);
                    string code = node.Editor.Text;
                    roslynFiles.Add((node.FileName, code));

                    // Page-Instanz als statische Property
                    sbProgram.AppendLine($"\t\tpublic static Definition{node.Text} {node.Text} {{ get; }} = new Definition{node.Text}();");

                    // Unterknoten (Subpages, Controls, etc.)
                    foreach (CodeNode sub in node.Nodes)
                    {
                        string subcode = sub.Editor.Text;
                        roslynFiles.Add((sub.FileName, subcode));
                    }
                }
            }

            // 4️⃣ Methoden: Initialize / Run / Destroy
            sbProgram.AppendLine("\t\tpublic static void Initialize()");
            sbProgram.AppendLine("\t\t{");
            foreach (string p in pages)
                sbProgram.AppendLine($"\t\t\t{p}.Initialize();");
            sbProgram.AppendLine("\t\t}");

            sbProgram.AppendLine("\t\tpublic static void Run()");
            sbProgram.AppendLine("\t\t{");
            foreach (string p in pages)
                sbProgram.AppendLine($"\t\t\t{p}.Run();");
            sbProgram.AppendLine("\t\t}");

            sbProgram.AppendLine("\t\tpublic static void Destroy()");
            sbProgram.AppendLine("\t\t{");
            foreach (string p in pages)
                sbProgram.AppendLine($"\t\t\t{p}.Destroy();");
            sbProgram.AppendLine("\t\t}");

            sbProgram.AppendLine("\t}");
            sbProgram.AppendLine("}");

            // 5️⃣ Dateien hinzufügen
            roslynFiles.Add(("Program.cs", sbProgram.ToString()));
            roslynFiles.Add(("GlobalUsing.cs", "global using static QB.Program;\r\nusing Thread = qbook.Core.BookThreadFactory;"));

            // 6️⃣ Referenzen aufbauen
            var references = new List<MetadataReference>
    {
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(System.Windows.Forms.Form).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(System.Drawing.Point).Assembly.Location)
    };

            string netstandardPath = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "netstandard.dll");
            if (File.Exists(netstandardPath))
                references.Add(MetadataReference.CreateFromFile(netstandardPath));

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
                            Debug.WriteLine($"[Roslyn] Skip native DLL: {Path.GetFileName(dllPath)}");
                            continue;
                        }

                        references.Add(MetadataReference.CreateFromFile(dllPath));
                        Debug.WriteLine($"[Roslyn] +Reference: {Path.GetFileName(dllPath)}");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.WriteLine($"[Roslyn] Skip invalid: {Path.GetFileName(dllPath)} ({ex.Message})");
                    }
                }
            }

            // 7️⃣ Neues Roslyn-Projekt erzeugen und kompilieren
            await _roslyn.LoadInMemoryProjectAsync(roslynFiles, references);

            Debug.WriteLine("======== Rebuild complete ========");

        }


        public List<string> CutInludesBlock(ref string source)
        {
            //Debug.WriteLine("===== ExtractIncludes ======");
            //Debug.WriteLine(source);

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

        //============================ Load Book End ========================

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }


        CodeNode _clickedNode;
        private async void ProjectTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
               if(e.Node is CodeNode)
                    await OpenNode(e.Node as CodeNode);
            //    UpdateTabs();
            }


            if (e.Button == MouseButtons.Right)
            {
                _clickedNode = e.Node as CodeNode;

                if (_clickedNode.Active)
                {
                    toolStripMenuIncludeCode.Text = "Exclude Code";
                }
                else
                {
                    toolStripMenuIncludeCode.Text = "Include Code";
                }


                oPage page = _clickedNode.Page;
                if (page != null)
                {
                    hidePageToolStripMenuItem.Text = page.Hidden ? "Show Page" : "Hide Page";
                }

                var clickedNode = ((System.Windows.Forms.TreeView)sender).GetNodeAt(new Point(e.X, e.Y));
                if (e.Node.Level == 0)
                {
                    addPageBeforeToolStripMenuItem.Visible = false;
                    addPageAfterToolStripMenuItem.Visible = false;
                    hidePageToolStripMenuItem.Visible = false;
                    addSubCodeToolStripMenuItem.Visible = false;
                    deleteStripMenuItem.Visible = false;
                    toolStripMenuOpenWorkspace.Visible = true;
                    renamePageToolStripMenuItem.Visible = false;
                    renameCodeToolStripMenuItem.Visible = false;
                    toolStripMenuIncludeCode.Visible = false;
                }
                if (e.Node.Level == 1)
                {
                    addPageBeforeToolStripMenuItem.Visible = true;
                    addPageAfterToolStripMenuItem.Visible = true;
                    hidePageToolStripMenuItem.Visible = true;
                    addSubCodeToolStripMenuItem.Visible = true;
                    renamePageToolStripMenuItem.Visible = true;
                    deleteStripMenuItem.Visible = false;
                    deletePageToolStripMenuItem.Visible = true;
                    toolStripMenuOpenWorkspace.Visible = false;
                    renameCodeToolStripMenuItem.Visible = false;
                    toolStripMenuIncludeCode.Visible = false;
                }

                if (e.Node.Level == 2)
                {
                    addPageBeforeToolStripMenuItem.Visible = false;
                    addPageAfterToolStripMenuItem.Visible = false;
                    hidePageToolStripMenuItem.Visible = false;
                    addSubCodeToolStripMenuItem.Visible = false;
                    toolStripMenuOpenWorkspace.Visible = false;
                    toolStripMenuIncludeCode.Visible = true;

                    renamePageToolStripMenuItem.Visible = false;

                    if (SelectedNode.Text.StartsWith("0_"))
                    {
                        renameCodeToolStripMenuItem.Visible = false;
                        deleteStripMenuItem.Visible = false;
                    }
                    else
                    {
                        renameCodeToolStripMenuItem.Visible = true;
                        deleteStripMenuItem.Visible = true;
                    }


                }
                contextMenuTreeView.Show(ProjectTree, e.Location);
            }

        }
        private oPage FindPageByName(string name)
        {
            Debug.WriteLine("Searching oPage '" + name + "'");
            if (string.IsNullOrEmpty(name)) return null;
            if (qbook.Core.ThisBook == null) return null;
            foreach (oPage page in qbook.Core.ActualMain.Objects.Where(item => item is oPage))
                if (string.Equals(page.Name, name, StringComparison.OrdinalIgnoreCase))
                    return page;
            return null;
        }

        #region GridViews

        BindingSource MethodenBindingSource = new BindingSource();
        void InitGridViews()
        {
            #region Diagnostic Output Gridview
            GridViewDiagnosticOutput = new DataGridView();
            GridViewDiagnosticOutput.DataBindingComplete += (s, e) =>
            {
                GridViewDiagnosticOutput.Columns["Page"].Width = 100;
                GridViewDiagnosticOutput.Columns["Class"].Width = 100;
                GridViewDiagnosticOutput.Columns["Position"].Width = 0;
                GridViewDiagnosticOutput.Columns["Length"].Width = 0;
                GridViewDiagnosticOutput.Columns["Type"].Width = 80;
                GridViewDiagnosticOutput.Columns["Node"].Width = 0;

                GridViewDiagnosticOutput.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            };

            GridViewDiagnosticOutput.AllowUserToResizeColumns = false;
            GridViewDiagnosticOutput.AllowUserToAddRows = false;
            GridViewDiagnosticOutput.RowHeadersVisible = false;
            GridViewDiagnosticOutput.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            GridViewDiagnosticOutput.MultiSelect = false;
            GridViewDiagnosticOutput.ReadOnly = true;
            GridViewDiagnosticOutput.BackgroundColor = Color.White;
            GridViewDiagnosticOutput.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            GridViewDiagnosticOutput.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            GridViewDiagnosticOutput.ColumnHeadersVisible = false;
            GridViewDiagnosticOutput.RowHeadersVisible = false;
            GridViewDiagnosticOutput.Dock = DockStyle.Fill;
            GridViewDiagnosticOutput.AllowUserToAddRows = false;
            GridViewDiagnosticOutput.AllowUserToDeleteRows = false;
            GridViewDiagnosticOutput.AllowUserToOrderColumns = true;
            GridViewDiagnosticOutput.AllowUserToResizeColumns = false;
            GridViewDiagnosticOutput.BackgroundColor = System.Drawing.Color.LightGray;
            GridViewDiagnosticOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            GridViewDiagnosticOutput.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            GridViewDiagnosticOutput.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            GridViewDiagnosticOutput.ColumnHeadersVisible = false;
            GridViewDiagnosticOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            GridViewDiagnosticOutput.Location = new System.Drawing.Point(46, 25);
            GridViewDiagnosticOutput.Margin = new System.Windows.Forms.Padding(0);
            GridViewDiagnosticOutput.Name = "dataGridOutput";
            GridViewDiagnosticOutput.Size = new System.Drawing.Size(832, 115);
            GridViewDiagnosticOutput.TabIndex = 0;
            GridViewDiagnosticOutput.ScrollBars = System.Windows.Forms.ScrollBars.None;
            GridViewDiagnosticOutput.CellFormatting += (s, e) =>
            {
                if (GridViewDiagnosticOutput.Columns[e.ColumnIndex].Name == "Type")
                {
                    string severity = e.Value?.ToString();
                    switch (severity)
                    {
                        case "Error":
                            GridViewDiagnosticOutput.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Tomato;
                            break;
                        case "Warning":
                            GridViewDiagnosticOutput.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                            break;
                        case "Info":
                            GridViewDiagnosticOutput.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightBlue;
                            break;
                    }
                }
            };
            GridViewDiagnosticOutput.CellClick += (s, e) => Task.Run(async () =>
            {
                if (e.RowIndex >= 0)
                {
                    string pos = GridViewDiagnosticOutput.Rows[e.RowIndex].Cells["Position"].Value.ToString();
                    int length = Convert.ToInt32(GridViewDiagnosticOutput.Rows[e.RowIndex].Cells["Length"].Value);
                    if (SelectedNode.Editor.InvokeRequired)
                    {
                        SelectedNode.Editor.Invoke(new System.Action(() =>
                        {
                            int p = Convert.ToInt32(pos);
                            SelectedNode.Editor.SelectRange(p, length);
                        }));
                    }
                    else
                    {
                        int p = Convert.ToInt32(pos);
                        SelectedNode.Editor.SelectRange(p, length);
                    }
                }
            });
            #endregion

           

            dataGridViewBuildOutput = new DataGridView();
            dataGridViewBuildOutput.DataBindingComplete += (s, e) =>
            {
                dataGridViewBuildOutput.Columns["Page"].Width = 100;
                dataGridViewBuildOutput.Columns["Class"].Width = 100;
                dataGridViewBuildOutput.Columns["Position"].Width = 0;
                dataGridViewBuildOutput.Columns["Length"].Width = 0;
                dataGridViewBuildOutput.Columns["Type"].Width = 80;
                dataGridViewBuildOutput.Columns["Node"].Width = 0;

                dataGridViewBuildOutput.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            };
            dataGridViewBuildOutput.AllowUserToResizeColumns = false;

            dataGridViewBuildOutput.DataSource = tblBuildDiagnosic;
            dataGridViewBuildOutput.AllowUserToResizeColumns = false;
            dataGridViewBuildOutput.AllowUserToAddRows = false;
            dataGridViewBuildOutput.RowHeadersVisible = false;
            dataGridViewBuildOutput.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewBuildOutput.MultiSelect = false;
            dataGridViewBuildOutput.ReadOnly = true;
            dataGridViewBuildOutput.BackgroundColor = Color.White;
            dataGridViewBuildOutput.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridViewBuildOutput.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewBuildOutput.ColumnHeadersVisible = false;
            dataGridViewBuildOutput.RowHeadersVisible = false;
            dataGridViewBuildOutput.Dock = DockStyle.Fill;
            dataGridViewBuildOutput.AllowUserToAddRows = false;
            dataGridViewBuildOutput.AllowUserToDeleteRows = false;
            dataGridViewBuildOutput.AllowUserToOrderColumns = true;
            dataGridViewBuildOutput.AllowUserToResizeColumns = false;
            dataGridViewBuildOutput.BackgroundColor = System.Drawing.Color.LightGray;
            dataGridViewBuildOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewBuildOutput.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dataGridViewBuildOutput.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewBuildOutput.ColumnHeadersVisible = false;
            dataGridViewBuildOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            dataGridViewBuildOutput.Location = new System.Drawing.Point(46, 25);
            dataGridViewBuildOutput.Margin = new System.Windows.Forms.Padding(0);
            dataGridViewBuildOutput.Name = "dataGridOutput";
            dataGridViewBuildOutput.Size = new System.Drawing.Size(832, 115);
            dataGridViewBuildOutput.TabIndex = 0;
            dataGridViewBuildOutput.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridViewBuildOutput.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewBuildOutput.AllowUserToAddRows = false;
            dataGridViewBuildOutput.CellFormatting += (s, e) =>
            {
                if (dataGridViewBuildOutput.Columns[e.ColumnIndex].Name == "Type")
                {
                    string severity = e.Value?.ToString();
                    switch (severity)
                    {
                        case "Error":
                            dataGridViewBuildOutput.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Tomato;
                            break;
                        case "Warning":
                            dataGridViewBuildOutput.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                            break;
                        case "Info":
                            dataGridViewBuildOutput.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightBlue;
                            break;
                    }
                }
            };
            dataGridViewBuildOutput.CellClick += (s, e)  =>
            {
                if (e.RowIndex >= 0)
                {
                    CodeNode node = (CodeNode)dataGridViewBuildOutput.Rows[e.RowIndex].Cells["Node"].Value;
                    OpenNode(node);
                }
            };

            DataTable MethodeDummy = new DataTable();
            MethodeDummy.Columns.Add("Row", typeof(int));
            MethodeDummy.Columns.Add("Name", typeof(string));

            //Methodes Gridview
            MethodenBindingSource.DataSource = MethodeDummy;
            gridViewMethodes.DataSource = MethodenBindingSource;
            gridViewMethodes.AllowUserToResizeColumns = false;
            gridViewMethodes.AllowUserToAddRows = false;
            gridViewMethodes.RowHeadersVisible = false;
            gridViewMethodes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridViewMethodes.MultiSelect = false;
            gridViewMethodes.ReadOnly = true;
            gridViewMethodes.BackgroundColor = Color.White;
            gridViewMethodes.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            gridViewMethodes.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            gridViewMethodes.ColumnHeadersVisible = false;
            gridViewMethodes.RowHeadersVisible = false;
            gridViewMethodes.Dock = DockStyle.Fill;
            gridViewMethodes.AllowUserToAddRows = false;
            gridViewMethodes.AllowUserToDeleteRows = false;
            gridViewMethodes.AllowUserToOrderColumns = true;
            gridViewMethodes.AllowUserToResizeColumns = false;
            gridViewMethodes.BackgroundColor = System.Drawing.Color.LightGray;
            gridViewMethodes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            gridViewMethodes.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            gridViewMethodes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridViewMethodes.ColumnHeadersVisible = false;
            gridViewMethodes.Dock = System.Windows.Forms.DockStyle.Fill;
            gridViewMethodes.Location = new System.Drawing.Point(46, 25);
            gridViewMethodes.TabIndex = 0;

            gridViewMethodes.DataBindingComplete += (s, e) =>
            {
                gridViewMethodes.Columns["Row"].Width = 40;
                gridViewMethodes.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            };

            gridViewMethodes.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {

                    int line = (int)gridViewMethodes.Rows[e.RowIndex].Cells["Row"].Value - 1;
                    if (SelectedNode.Editor.InvokeRequired)
                    {
                        SelectedNode.Editor.Invoke(new System.Action(() =>
                        {

                            SelectedNode.Editor.HighlightLine(line, Color.Yellow);
                        }));
                    }
                    else
                    {
                        SelectedNode.Editor.HighlightLine(line, Color.Yellow);
                    }
                }
            };


        }
        void UpdateOutputButtons()
        {
            bool isBuild = this.panelOutput.Controls.Contains(this.dataGridViewBuildOutput);
            bool isEditor = this.panelOutput.Controls.Contains(this.GridViewDiagnosticOutput);
            bool isFindReplace = this.panelOutput.Controls.Contains(this.dataGridViewFindReplace);

            btnBuildOutput.ForeColor = ProjectTree.ForeColor;
            btnEditorOutput.ForeColor = ProjectTree.ForeColor;
            btnShowFindReplaceOutput.ForeColor = ProjectTree.ForeColor;

            if (!Theme.IsDark)
            {

                Color sel = Color.FromArgb(220, 220, 220);
                Color usel = Color.FromArgb(190, 190, 190);
                btnEditorOutput.BackColor = isEditor ? sel : usel;
                btnBuildOutput.BackColor = isBuild ? sel : usel;
                btnShowFindReplaceOutput.BackColor = isFindReplace ? sel : usel;
            }
            else
            {
                Color sel = Color.FromArgb(70, 70, 70);
                Color usel = Color.FromArgb(50, 50, 50);
                btnEditorOutput.BackColor = isEditor ? sel : usel;
                btnBuildOutput.BackColor = isBuild ? sel : usel;
                btnShowFindReplaceOutput.BackColor = isFindReplace ? sel : usel;
            }
        }
        Color ButtonForeColor = Color.Transparent;
        public Font GetFont()
        {
            Scintilla edit = SelectedNode.Editor;
            var style = edit.Styles[Style.Default];
            string fontName = style.Font;
            int baseSize = style.Size;
            int zoom = edit.Zoom;
            int effectiveSize = baseSize + zoom;

            return new Font(fontName, effectiveSize);
        }

        #endregion

        #region Themes

        private void ApplyLightTheme()
        {
          
            Theme.Current = Theme.EditorTheme.Light;

            panelEditor.BackColor = Color.White;

            //Background

            Color _backColor = Color.FromArgb(230, 230, 230);

            panelSplttter1.BackColor = Color.FromArgb(180, 180, 180);
            panelSplitter2.BackColor = Color.FromArgb(180, 180, 180);
            panelSplitter3.BackColor = Color.FromArgb(180, 180, 180);
            panelSplitter4.BackColor = Color.FromArgb(180, 180, 180);

            vBarEditor.SetBackColor = _backColor;
            vBarEditor.SetForeColor = Color.FromArgb(180, 180, 180);

            vBarMethodes.SetBackColor = _backColor;
            vBarMethodes.SetForeColor = Color.FromArgb(180, 180, 180);

            hBarEditor.SetBackColor = _backColor;
            hBarEditor.SetForeColor = Color.FromArgb(180, 180, 180);

     
            vBarOutputs.SetBackColor = _backColor;
            vBarOutputs.SetForeColor = Color.FromArgb(180, 180, 180);


            DwmTitleBar.SetImmersiveDarkMode(this.Handle, enabled: false);
            tableLayoutPanel1.BackColor = _backColor;
            TablePanelOutputs.BackColor = _backColor;
           // Editor.BorderStyle = ScintillaNET.BorderStyle.None;

            //ProjectTree
            ProjectTree.BackColor = _backColor;
            ProjectTree.ForeColor = Color.Black;
            ProjectTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            UpdateAllNodes(ProjectTree, ProjectTree.ForeColor, 2, 2);
            //UpdateAllNodes(ProjectTree, ProjectTree.ForeColor, 2, 3);
            //UpdateAllNodes(ProjectTree, ProjectTree.ForeColor, 3, 4);
            ProjectTree.SelectedImageIndex = 10;
            //if (SelectedNode != null)
            //    SelectedNode.ForeColor = Color.Purple;

            //Methodes
            lblMethodes.BackColor = _backColor;
            lblMethodes.ForeColor = ProjectTree.ForeColor;
            gridViewMethodes.BackgroundColor = _backColor;
            gridViewMethodes.BackColor = _backColor;
            gridViewMethodes.RowsDefaultCellStyle.BackColor = _backColor;
            gridViewMethodes.RowsDefaultCellStyle.ForeColor = ProjectTree.ForeColor;

        


            vBarMethodes.SetBackColor = _backColor;
            vBarMethodes.SetForeColor = Color.FromArgb(180, 180, 180);


            vBarProjectTree.SetBackColor = _backColor;
            vBarProjectTree.SetForeColor = Color.FromArgb(70, 70, 70);

            tbMethodeFilter.BackColor = Color.FromArgb(250, 250, 250);
            tbMethodeFilter.ForeColor = vBarProjectTree.SetForeColor;


            //Status
            labelStatus.BackColor = _backColor;
            labelStatus.ForeColor = ProjectTree.ForeColor;


            if (ButtonForeColor == Color.Transparent) ButtonForeColor = Color.Black;


            //PanelControl Top
            _backColor = Color.FromArgb(220, 220, 220);
            panelControl.BackColor = _backColor;

            foreach (System.Windows.Forms.Button b in panelControl.Controls)
            {
                b.BackColor = _backColor;
                //  b.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                b.ForeColor = Color.Black;
                b.FlatAppearance.BorderColor = _backColor;
                Bitmap pic = b.Image as Bitmap;
                pic = BitmapTools.ReplaceColor(pic, Color.Black, Color.FromArgb(120, 120, 120), 100); //Init Icon Color
                pic = BitmapTools.ReplaceColor(pic, Color.FromArgb(150, 150, 150), Color.FromArgb(100, 100, 100), 100);
                pic = BitmapTools.ResizeExact(pic, 28, 28);
                var old = b.Image;
                b.Image = pic;
                old?.Dispose(); ;
            }



            //Output
            UpdateOutputButtons();

            //Buttons Editor Left
            _backColor = Color.FromArgb(230, 230, 230);
            foreach (System.Windows.Forms.Button b in panelFunctions.Controls)
            {
                b.BackColor = tableLayoutPanel1.BackColor;
                b.Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
                b.ForeColor = System.Drawing.Color.Black;
                b.FlatAppearance.BorderColor = _backColor;
                Bitmap pic = b.Image as Bitmap;
                //  pic = BitmapTools.ReplaceColor(pic, Color.White, Color.Black,100);
                pic = BitmapTools.ResizeExact(pic, 32, 32);
                var old = b.Image;
                b.Image = pic;
                old?.Dispose();
            }

            ButtonForeColor = Color.Black;
            GridViewDiagnosticOutput.BackgroundColor = Color.FromArgb(220, 220, 220);
            dataGridViewBuildOutput.BackgroundColor = Color.FromArgb(220, 220, 220);

            dataGridViewFindReplace.BackgroundColor = Color.FromArgb(220, 220, 220);
            dataGridViewFindReplace.ForeColor = Color.Black;

            Theme.Current = Theme.EditorTheme.Light;


        //    UpdateTabs();
        }
        private void ApplyDarkTheme()
        {
            Theme.Current = Theme.EditorTheme.Dark;
            Color _backColor = Color.FromArgb(40, 40, 40);

            panelEditor.BackColor = Color.Black;

            panelSplttter1.BackColor = Color.FromArgb(70, 70, 70);
            panelSplitter2.BackColor = Color.FromArgb(70, 70, 70);
            panelSplitter3.BackColor = Color.FromArgb(70, 70, 70);
            panelSplitter4.BackColor = Color.FromArgb(70, 70, 70);


            //Background
            DwmTitleBar.SetImmersiveDarkMode(this.Handle, enabled: true);

            tableLayoutPanel1.BackColor = _backColor;
            TablePanelOutputs.BackColor = _backColor;

            vBarEditor.SetBackColor = _backColor;
            vBarEditor.SetForeColor = Color.FromArgb(70, 70, 70);
            vBarMethodes.SetBackColor = _backColor;
            vBarMethodes.SetForeColor = Color.FromArgb(70, 70, 70);
            hBarEditor.SetBackColor = _backColor;
            hBarEditor.SetForeColor = Color.FromArgb(70, 70, 70);

            vBarOutputs.SetBackColor = _backColor;
            vBarOutputs.SetForeColor = Color.FromArgb(70, 70, 70);

            vBarProjectTree.SetBackColor = _backColor;
            vBarProjectTree.SetForeColor = Color.FromArgb(70, 70, 70);

            //ProjectTree
            ProjectTree.BackColor = _backColor;
            ProjectTree.ForeColor = Color.FromArgb(190, 190, 190);
            ProjectTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            ProjectTree.SelectedImageIndex = 10;

            tbMethodeFilter.BackColor = Color.FromArgb(60, 60, 60);
            tbMethodeFilter.ForeColor = ProjectTree.ForeColor;
          

            UpdateAllNodes(ProjectTree, ProjectTree.ForeColor, 1, 1);
            //UpdateAllNodes(ProjectTree, ProjectTree.ForeColor, 2, 3);
            //UpdateAllNodes(ProjectTree, ProjectTree.ForeColor, 3, 4);

            //Methodes
            lblMethodes.BackColor = _backColor;
            lblMethodes.ForeColor = ProjectTree.ForeColor;
            gridViewMethodes.BackgroundColor = _backColor;
            gridViewMethodes.BackColor = _backColor;
            gridViewMethodes.RowsDefaultCellStyle.BackColor = _backColor;
            gridViewMethodes.RowsDefaultCellStyle.ForeColor = ProjectTree.ForeColor;

            //if (SelectedNode != null)
            //    SelectedNode.ForeColor = Color.Plum;

            //Status
            labelStatus.BackColor = _backColor;
            labelStatus.ForeColor = ProjectTree.ForeColor;


            //PanelControl
            _backColor = Color.FromArgb(60, 60, 60);
            panelControl.BackColor = _backColor;

            foreach (System.Windows.Forms.Button b in panelControl.Controls)
            {
                b.BackColor = _backColor;

                b.ForeColor = Color.FromArgb(200, 200, 200);
                b.FlatAppearance.BorderColor = _backColor;
                Bitmap pic = b.Image as Bitmap;
                pic = BitmapTools.ReplaceColor(pic, Color.Black, Color.FromArgb(190, 190, 190), 100);
                //  pic = BitmapTools.ResizeExact(pic, 28, 28);
                var old = b.Image;
                b.Image = pic;
                old?.Dispose();
            }


            //Buttons
            _backColor = Color.FromArgb(40, 40, 40);
            if (ButtonForeColor == Color.Transparent) ButtonForeColor = Color.White;

            foreach (System.Windows.Forms.Button b in panelFunctions.Controls)
            {
                b.BackColor = _backColor;
                b.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                b.ForeColor = Color.Black;
                b.FlatAppearance.BorderColor = _backColor;
                Bitmap pic = b.Image as Bitmap;
                pic = BitmapTools.ReplaceColor(pic, Color.Black, Color.FromArgb(120, 120, 120), 100); //Init Icon Color
                pic = BitmapTools.ReplaceColor(pic, ButtonForeColor, Color.FromArgb(120, 120, 120));
                //    pic = BitmapTools.ResizeExact(pic, 32, 32);
                var old = b.Image;
                b.Image = pic;
                old?.Dispose();
            }
            ButtonForeColor = Color.FromArgb(120, 120, 120);

            //Outputs
            UpdateOutputButtons();
            GridViewDiagnosticOutput.BackgroundColor = Color.FromArgb(70, 70, 70);
            GridViewDiagnosticOutput.ForeColor = Color.Black;

            dataGridViewBuildOutput.BackgroundColor = Color.FromArgb(70, 70, 70);
            dataGridViewBuildOutput.ForeColor = Color.Black;

            dataGridViewFindReplace.BackgroundColor = Color.FromArgb(70, 70, 70);
            dataGridViewFindReplace.ForeColor = Color.Black;

            //


            //UpdateTabs();
        }

        public async Task ToggleTheme()
        {
            if (!Theme.IsDark) 
            {
                ApplyDarkTheme();
                ((DocumentEditor)panelEditor.Controls[0]).ApplyDarkTheme();
            }
            else 
            {
                ApplyLightTheme();
                ((DocumentEditor)panelEditor.Controls[0]).ApplyLightTheme();
            }
            ;
            //    RefreshSemanticOverlaysAsync();
            //await RefreshEditor("ToggleTheme");
            //if (_findReplaceControl == null) return;
            //if (_currentTheme == EditorTheme.Dark) _findReplaceControl.DarkTheme(); else _findReplaceControl.LightTheme();

        }

        private void UpdateAllNodes(System.Windows.Forms.TreeView tree, Color color, int nodeLevel, int newIndex)
        {
            foreach (CodeNode node in tree.Nodes)
            {
                UpdateNodeByLevelRecursive(node, color, nodeLevel, newIndex);
            }
        }
        private void UpdateNodeByLevelRecursive(CodeNode node, Color color, int oldIndex, int newIndex)
        {
            node.ForeColor = color;
            if (node == SelectedNode)
                node.ForeColor = Theme.IsDark ? Color.Plum : Color.DarkOrchid;

            int index = node.ImageIndex == oldIndex ? newIndex : node.ImageIndex;

            node.ImageIndex = index;

            if (node.HasErrors)
            {
                index = 9;
            }

            node.SelectedImageIndex = SelectedNode.ImageIndex;

            foreach (CodeNode child in node.Nodes)
            {
                UpdateNodeByLevelRecursive(child, color, oldIndex, newIndex);
            }
        }

        #endregion

      

        #region Page Control (Add,Remove, Rename)

        private void AddNewPageBelowSelectedPage(bool beforeSelectedPage = false)
        {
            qbook.Core.ThisBook.Modified = true;
            //qbook.Core.ActualMain = (sender as oIcon).Parent;

            string newName = "newPage";


            _AddPageStart:

            if (!UiExtentions.ShowEditTextDialog(ref newName, "Page name:"))
                return;


            bool pageNameExists = false;
            foreach (oPage page in qbook.Core.ActualMain.Objects.OfType<oPage>())
            {
                string newFullName = null;
                int index = page.FullName.LastIndexOf(".");
                if (index == -1)
                    newFullName = newName.Trim();
                else
                    newFullName = page.FullName.Substring(0, index) + newName.Trim();
                if (page.FullName == newFullName)
                {
                    pageNameExists = true;
                    break;
                }
            }
            if (pageNameExists)
            {
                MessageBox.Show($"A page with the name '{newName}' already exists.\r\nPlease choose a different name."
                    , "PAGE NAME EXISTS", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                goto _AddPageStart;
            }

            oPage newPage = new oPage(newName, newName);

            //TODO: insert after selected page/node
            //int indx = (sender as oIcon).Parent.Objects.IndexOf(qbook.Core.SelectedPage);
            int indx = 0;
            if (SelectedPage == null)
                indx = 0;
            else
                indx = qbook.Core.ThisBook.Main.Objects.OfType<oPage>().ToList().IndexOf(SelectedPage);

            if (beforeSelectedPage)
                indx = indx + 0;
            else
                indx = indx + 1;

            qbook.Core.ThisBook.Main.Objects.Insert(indx, newPage);
            
            qbook.Core.SelectedPage = newPage;

            //   Main.Qb.SelectedLayer.Add(newItem);
            qbook.Core.ThisBook.Modified = true;
            //PopulatePageTreeView(false, true);

        }
        #endregion

        #region Node Selection

        void SafeInvoke(System.Windows.Forms.Control control, System.Action action)
        {
            if (control.InvokeRequired)
                control.Invoke(action);
            else
                action();
        }

        public CodeNode SelectedNode;

        public async Task SelectNodeByName(string NodeName)
        {
            //     Debug.WriteLine($"========== Looking for Node '{NodeName}' ==========");

            SafeInvoke(ProjectTree, async () =>
            {

                ProjectTree.SelectedNode = null;
                foreach (CodeNode node in ProjectTree.Nodes[0].Nodes)
                {
                    if (node.Text.EndsWith(NodeName))
                    {
                        ProjectTree.SelectedNode = node;
                        node.EnsureVisible();
                        node.Expand();
                        await OpenNode(node);
                        //   UpdateTabs();
                        break;
                    }

                    foreach (CodeNode subnode in node.Nodes)
                    {
                        //    Debug.WriteLine(subnode.Name);
                        if (subnode.Name.EndsWith(NodeName))
                        {
                            ProjectTree.SelectedNode = subnode;
                            subnode.EnsureVisible();
                            subnode.Expand();
                            await OpenNode(subnode);
                            //        UpdateTabs();
                            break;
                        }
                    }
                }
            });
        }

        public async Task OpenNode(CodeNode node)
        {
            if (_findReplaceControl != null)
            {
                if (_findReplaceControl.Visible)
                    _findReplaceControl.BringToFront();
            }


            if (node == null) return;
            if (node == SelectedNode) return;
            if (node.Editor == null) return;
            SelectedNode = node;
            if (Theme.IsDark)
            {
                ApplyDarkTheme();
                node.Editor.ApplyDarkTheme();
            }
            else
            {
                ApplyLightTheme();
                node.Editor.ApplyLightTheme();
            }

            bool find = panelEditor.Controls.Contains(_findReplaceControl) && _findReplaceControl.Visible;


            //qbook custom
            await node.SyncSubcodeUsings();
            

            panelEditor.Controls.Clear();
            node.Editor.Dock = DockStyle.Fill;
            panelEditor.Controls.Add(node.Editor);

            

            if (find) ShowFindReplaceControl();

            panelEditor.Visible = true;
            node.Editor.Focus();
            await node.UpdateRoslyn();

            //node.HideUsingsFromSubCode();
          //  node.HideIncludeBlock();
           

            vBarEditor.Init(node.Editor);
            hBarEditor.Init(node.Editor);

          

            GridViewDiagnosticOutput.DataSource = node.Output;
            MethodenBindingSource.DataSource = node.MethodesClasses;
            node.Select();
         

        }
        private CodeNode? GetNodeByDocument(RoslynDocument doc)
        {
            if (doc == null) return null;

            foreach (CodeNode node in ProjectTree.Nodes[0].Nodes)
            {

                if (node.RoslynDoc.Name == doc.Name)
                {
                    Debug.WriteLine("node found: " + node.Name);
                    return node;
                }

                foreach (CodeNode sub in node.Nodes)
                {
                    if (sub.RoslynDoc.Name == doc.Name)
                    {
                        Debug.WriteLine("node found: " + sub.Name);
                        return sub;
                    }
                }
            }

            return null;
        }
        private async Task GoToDefinitionAsync()
        {
            if (SelectedNode == null) return;

            int caret = SelectedNode.Editor.CurrentPosition;
            var loc = await _roslyn.GoToDefinitionAsync(SelectedNode.RoslynDoc, caret);
            if (loc == null) return;

            var (doc, line, column) = loc.Value;

            CodeNode? node = GetNodeByDocument(doc);
            if (node == null) return;

            await OpenNode(node);

            int pos = GetPositionFromLineColumn(node.Editor, line, column);
            node.Editor.GotoPosition(pos);
            node.Editor.ScrollCaret();
        }
        private static int GetPositionFromLineColumn(Scintilla ed, int line, int column) => ed.Lines[line].Position + column;


        private async Task <CodeNode> newPage(string name)
        {
            
            CodeNode pageNode = new CodeNode(new oPage(name,name), name + ".qPage.cs", CodeNode.NodeType.Page, name);
          
            pageNode.Editor.Text = pageNode.NewPageCode(name);
            pageNode.Editor.EmptyUndoBuffer();
            pageNode.Adhoc.Workspace = _roslyn.GetWorkspace;
            pageNode.Adhoc.Id = _roslyn.GetProjectId;
            pageNode.Active = true;
            pageNode.Editor.GoToDefinition = () => GoToDefinitionAsync();
            pageNode.Editor.RenameSymbol = () => RenameSymbolAsync();
            await pageNode.AddFileToRoslyn();

            return pageNode;
        }

        private async Task<CodeNode> newSubCode(CodeNode page, string name)
        {
            int index = page.Nodes.Count + 1;
            string fileName = $"{page.Name}.{name}.cs";
            CodeNode subCode = new CodeNode(page.Page, fileName, CodeNode.NodeType.SubCode, fileName, page);
            subCode.CodeIndex = index;
            subCode.Adhoc = page.Adhoc;
            subCode.Editor.Text = subCode.NewSubCode();
            subCode.Editor.EmptyUndoBuffer();
            subCode.Active = true;
            subCode.Editor.GoToDefinition = () => GoToDefinitionAsync();
            subCode.Editor.RenameSymbol = () => RenameSymbolAsync();
            await subCode.AddFileToRoslyn();
            return subCode;
        }


        private void WritePageIndex()
        {
            int index = 0;
            foreach (CodeNode page in ProjectTree.Nodes[0].Nodes) page.CodeIndex = index++;
           
        }


        #endregion

        #region FindReplace 
        private void ShowFindReplaceControl()
        {
            if(!panelEditor.Controls.Contains(_findReplaceControl))
                panelEditor.Controls.Add(_findReplaceControl);

            _findReplaceControl.Location = new Point(panelEditor.Width - _findReplaceControl.Width, 0); // Rechts oben
            if (!Theme.IsDark) _findReplaceControl.LightTheme();
            if (Theme.IsDark) _findReplaceControl.DarkTheme();
            _findReplaceControl.Editor = SelectedNode.Editor;
            _findReplaceControl.BringToFront();
            _findReplaceControl.Visible = true;


        }

        private void ShowSearchBar()
        {
           if (SelectedNode == null) return;

            if (_findReplaceControl == null)
            {
                _findReplaceControl = new ControlFindReplace(this, _roslyn, dataGridViewFindReplace);
                panelEditor.Controls.Add(_findReplaceControl);
            }

            ShowFindReplaceControl();
            _findReplaceControl.ShowFind();
            _findReplaceControl.FocusFind();
        }

        private void ShowReplaceBar()
        {
            if (SelectedNode == null) return;

          

            if (_findReplaceControl == null)
            {
                _findReplaceControl = new ControlFindReplace(this, _roslyn, dataGridViewFindReplace);
                panelEditor.Controls.Add(_findReplaceControl);
            }

            ShowFindReplaceControl();
            _findReplaceControl.ShowReplace();
            _findReplaceControl.FocusFind();
        }

        public static string ShowInputDialog(string prompt, string title, string defaultValue = "")
        {
            Form inputForm = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterScreen
            };

            System.Windows.Forms.Label textLabel = new System.Windows.Forms.Label() { Left = 10, Top = 20, Text = prompt, AutoSize = true };
            System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox() { Left = 10, Top = 50, Width = 360, Text = defaultValue };
            System.Windows.Forms.Button confirmation = new System.Windows.Forms.Button() { Text = "OK", Left = 290, Width = 80, Top = 80, DialogResult = System.Windows.Forms.DialogResult.OK };

            inputForm.Controls.Add(textLabel);
            inputForm.Controls.Add(textBox);
            inputForm.Controls.Add(confirmation);
            inputForm.AcceptButton = confirmation;

            return inputForm.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }
        private async Task RenameSymbolAsync()
        {
            if (SelectedNode?.RoslynDoc == null)
            {
                MessageBox.Show(this, "Project not loaded or no document selected.", "Rename", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int caretBefore = SelectedNode.Editor.CurrentPosition;
            int selStartBefore = SelectedNode.Editor.SelectionStart;
            int selEndBefore = SelectedNode.Editor.SelectionEnd;
            int firstLine = SelectedNode.Editor.FirstVisibleLine;

         

            var doc = SelectedNode.RoslynDoc;
            var semanticModel = await doc.GetSemanticModelAsync();
            var syntaxTree = await doc.GetSyntaxTreeAsync();


            var root = await syntaxTree.GetRootAsync();
            var position = caretBefore > 0 ? caretBefore - 1 : caretBefore;
            var token = root.FindToken(position);
            var nodeParent = token.Parent;

            var symbol = semanticModel.GetSymbolInfo(nodeParent).Symbol ?? semanticModel.GetDeclaredSymbol(nodeParent);

            var input = ShowInputDialog("Input new name:", $"Rename Symbol {nodeParent}", $"NewName");
            if (string.IsNullOrWhiteSpace(input)) return;
            string newName = input.Trim();


            Debug.WriteLine($"Rename '{symbol}' -> '{input}'");
            if (symbol == null)
            {
                MessageBox.Show(this, $"Rename failed (symbol not found). {input}", "Rename", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var solution = doc.Project.Solution;
            var newSolution = await Renamer.RenameSymbolAsync(solution, symbol, newName, solution.Workspace.Options);

            var allDocs = _roslyn.GetAllDocuments();
            int updatedCount = 0;

            foreach (RoslynDocument oldDoc in allDocs)
            {
                var newDoc = newSolution.GetDocument(oldDoc.Id);
                if (newDoc == null) continue;

                var newText = await newDoc.GetTextAsync();
                var newTextStr = newText.ToString();
                var oldTextStr = (await oldDoc.GetTextAsync()).ToString();

                if (!string.Equals(newTextStr, oldTextStr, StringComparison.Ordinal))
                {
                    var node = GetNodeByDocument(oldDoc);
                    if (node != null)
                    {
                        node.RoslynDoc = newDoc;
                        node.Editor.Text = newTextStr;
                        await node.UpdateRoslyn();
                        updatedCount++;

                        if (node == SelectedNode)
                        {
                            SelectedNode.Editor.BeginUndoAction();
                            SelectedNode.Editor.Text = newTextStr;
                            SelectedNode.Editor.EndUndoAction();
                        }
                    }
                }
            }

            if (updatedCount == 0)
            {
                MessageBox.Show(this, $"Rename failed (no changes).  {input}", "Rename", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OpenNode(SelectedNode);
            // Cursor und Auswahl wiederherstellen
            int newLen = SelectedNode.Editor.TextLength;
            SelectedNode.Editor.SetSelection(Math.Min(selEndBefore, newLen), Math.Min(selStartBefore, newLen));
            SelectedNode.Editor.GotoPosition(Math.Min(caretBefore, newLen));
            SelectedNode.Editor.Lines[firstLine].Goto();

            await SelectedNode.UpdateRoslyn();
            // await RefreshDiagnosticsAsync();
            SelectedNode.Editor.FirstVisibleLine = firstLine;
            SelectedNode.Editor.GotoPosition(caretBefore);
        }

        #endregion

        public string GetStatus => labelStatus.Text;
        private void btnEditorOutput_Click(object sender, EventArgs e)
        {
            panelOutput.Controls.Clear();
            panelOutput.Controls.Add(GridViewDiagnosticOutput);
            vBarOutputs.Init(GridViewDiagnosticOutput);
            UpdateOutputButtons();
        }
        private void btnBuildOutput_Click(object sender, EventArgs e)
        {
            panelOutput.Controls.Clear();
            panelOutput.Controls.Add(dataGridViewBuildOutput);
            vBarOutputs.Init(dataGridViewBuildOutput);
            UpdateOutputButtons();
        }
        private void btnShowFindReplaceOutput_Click(object sender, EventArgs e)
        {
            panelOutput.Controls.Clear();
            dataGridViewFindReplace.Dock = DockStyle.Fill;
            panelOutput.Controls.Add(dataGridViewFindReplace);
            vBarOutputs.Init(dataGridViewFindReplace);
            UpdateOutputButtons();
        }
        private void FormScintillaEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
        private void btnToggleTheme_Click(object sender, EventArgs e)
        {
            ToggleTheme();
        }
        private void tbMethodeFilter_TextChanged(object sender, EventArgs e)
        {
            string filterText = tbMethodeFilter.Text.Trim();


            if (string.IsNullOrEmpty(filterText))
            {
                MethodenBindingSource.RemoveFilter(); // zeigt alles an
            }
            else
            {
                MethodenBindingSource.Filter = $"Name LIKE '%{filterText}%'";
            }

        }
        private async void btnFormat_Click(object sender, EventArgs e)
        {
           await SelectedNode?.FormatCode();  
        }
        private void btnParagraph_Click(object sender, EventArgs e)
        {
            SelectedNode?.Editor.ToggleViewEol();
        }
        private void btnFind_Click(object sender, EventArgs e)
        {
            ShowSearchBar();
            btnShowFindReplaceOutput.PerformClick();
        }
        private void btnFindReplace_Click(object sender, EventArgs e)
        {
            ShowReplaceBar();
            btnShowFindReplaceOutput.PerformClick();
        }
        private async void toolStripMenuIncludeCode_Click(object sender, EventArgs e)
        {
           await _clickedNode?.ToggleInclude();
        }

        private async void btnReload_Click(object sender, EventArgs e)
        {
            await Core.OpenQbookAsync(Path.Combine(Core.ThisBook.Directory, Core.ThisBook.Filename));
        }

        private async void btnRebuild_Click(object sender, EventArgs e)
        {
            await Rebuild();
            
        }

        public bool HasError => _roslyn.BuildSuccess;

        public async Task Rebuild()
        {
           ResetTreeView();

   

            var task = _roslyn.CreateAssemblyFromTree(ProjectTree);
            while (!task.IsCompleted)
            {
                SetStatusText(_roslyn.BuildResult);
                await Task.Delay(100);
            }
            await task;

            SetStatusText(_roslyn.BuildResult);
            ShowBuildErrors();

        }

        public void ResetTreeView(bool collapse = false)
        {
            ProjectTree.BeginUpdate();
            ProjectTree.SelectedNode = null;
            foreach (CodeNode node in ProjectTree.Nodes[0].Nodes)
            {
                if(collapse)
                node.Collapse();

                ResetNode(node);

                if (node.Type == CodeNode.NodeType.Page)
                    foreach (CodeNode sub in node.Nodes) ResetNode(sub);
            }

            ProjectTree.EndUpdate();

            ProjectTree.Update();
            ProjectTree.Invalidate();
            ProjectTree.Refresh();
        }

        private void ResetNode(CodeNode node)
        {
            if (node.Type == CodeNode.NodeType.Page)
                node.ImageIndex = 2;

            if (node.Type == CodeNode.NodeType.SubCode)
                node.ImageIndex = 3;

            if (node.Page != null)
                if (node.Page.Hidden && node.Type == CodeNode.NodeType.Page)
                    node.ImageIndex = 5;
            if (!node.Active)
                node.ImageIndex = 4;
        }


        public void ShowBuildErrors()
        {

            if (_roslyn.ErrorFiles.Count > 0)
            {
                foreach (string err in _roslyn.ErrorFiles)
                {
                    CodeNode node = GetNodeByDocument(_roslyn.GetDocumentByFilename(err));
                    if(node == null) continue;
                    node.PageNode.ImageIndex = 10;
                    node.ImageIndex = 10;
                }
            }
        }

        private void hidePageToolStripMenuItem_Click(object sender, EventArgs e)
        {

            _clickedNode.Page.Hidden = !_clickedNode.Page.Hidden;
            _clickedNode.ImageIndex = _clickedNode.Page.Hidden ? 5 : 2;
        }

        private async void addPageBeforeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await InsertPage(offset: -1);
        }

        private async void addPageAfterToolStripMenuItem_Click(object sender, EventArgs e)
        {

            await InsertPage(offset: +1);

        }

        private async Task InsertPage(int offset = 1)
        {
            ProjectTree.BeginUpdate();
            CodeNode Page = null;
            string name = ShowInputDialog("Input code name:", $"New subcode", "CustomCode");
            if (!string.IsNullOrWhiteSpace(name))
            {
                Page = await newPage(name);
                Page.ImageIndex = 2;
            }
            if (Page == null)
            {
                ProjectTree.EndUpdate();
                return;
            }
            System.Windows.Forms.TreeNodeCollection tree = ProjectTree.Nodes[0].Nodes;
            int index = -1;
            if (offset == 0)
            {
                ProjectTree.Nodes[0].Nodes.Add(Page);
            }
            else
            {
    

                if (offset == -1)
                {
                    offset = 0;
                }
                index = tree.IndexOf(_clickedNode);
                tree.Insert(index + offset, Page);

                int origin = Core.ThisBook.PageOrder.IndexOf(_clickedNode.Text);
                Core.ThisBook.PageOrder.Insert(origin + offset, Page.Text);
                qbook.Core.ThisBook.Main.Objects.Add(Page.Page);



            }
            ProjectTree.EndUpdate();
            index = 0;
            foreach (CodeNode page in ProjectTree.Nodes[0].Nodes)
            {
                page.CodeIndex = index++;
                page.Page.OrderIndex = page.CodeIndex;
            }
        }

        private async void renameCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newname = ShowInputDialog($"Input new name:", $"Rename Code {_clickedNode.Text}", _clickedNode.Text);
            if(newname != null) await _clickedNode.Rename(newname);
        }

        private async void renamePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newname = ShowInputDialog($"Input new name:", $"Rename Page {_clickedNode.Text}", _clickedNode.Text);
            if (newname != null) await _clickedNode.Rename(newname);

        }

        public void SetStatusText(string text, Color? color = null, Color? backColor = null)
        {
            if (color == null)
                color = Color.Black;
            if (backColor == null)
                backColor = SystemColors.Control;

            labelStatus.BeginInvoke((System.Action)(() =>
            {
                labelStatus.Text = text;
            }));
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            PageRuntime.RunAll();
            SetStatusText("[Build] running...");

        }

        private void btnShowHidden_Click(object sender, EventArgs e)
        {
            SelectedNode.Editor.ToggleHidenLines();
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            int index = 0;
            foreach (CodeNode page in ProjectTree.Nodes[0].Nodes)
            {
                page.CodeIndex = index++;
                page.Page.OrderIndex = page.CodeIndex;
            }
            await SaveInFolder();
        }

        private async void customToolStripMenuItem_Click(object sender, EventArgs e)
        {

            string name = ShowInputDialog("Input code name:", $"New subcode", "CustomCode");

            if (!string.IsNullOrWhiteSpace(name))
            {
                _clickedNode.Nodes.Add(await newSubCode(_clickedNode, name));
            }

        }

        private async void deleteStripMenuItem_Click(object sender, EventArgs e)
        {
            await _clickedNode.RemoveFileFromRoslyn();
            ProjectTree.Nodes.Remove(_clickedNode);

        }

        private async void deletePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await _clickedNode.RemoveFileFromRoslyn();
            Core.ThisBook.Main.Objects.Remove(_clickedNode.Page);
            Core.ThisBook.PageOrder.Remove(_clickedNode.Text);
            ProjectTree.Nodes.Remove(_clickedNode);
        }
    }

    public static class Theme
    {
        public enum EditorTheme { Light, Dark }
        public static EditorTheme Current = EditorTheme.Light;

        public static bool IsDark => Current == EditorTheme.Dark;

    }

    internal static class DwmTitleBar
    {
        // DWMWINDOWATTRIBUTE Werte (Windows 11+)
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20; // (19 bei älteren Win10-Insider Builds)
        private const int DWMWA_BORDER_COLOR = 34;
        private const int DWMWA_CAPTION_COLOR = 35;
        private const int DWMWA_TEXT_COLOR = 36;


        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);


        public static bool IsWindows11OrNewer()
        {
            // Windows 11: Build >= 22000
            try { return Environment.OSVersion.Version.Build >= 22000; } catch { return false; }
        }


        public static bool SetImmersiveDarkMode(IntPtr hwnd, bool enabled)
        {
            int v = enabled ? 1 : 0;
            int hr = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref v, sizeof(int));
            return hr >= 0; // S_OK = 0; negative = Fehler
        }


        public static bool SetCaptionColor(IntPtr hwnd, Color color)
        {
            // COLORREF (BGR) – ColorTranslator.ToWin32 liefert korrektes Format
            int colorRef = ColorTranslator.ToWin32(color);
            int hr = DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref colorRef, sizeof(int));
            return hr >= 0;
        }


        public static bool SetTextColor(IntPtr hwnd, Color color)
        {
            int colorRef = ColorTranslator.ToWin32(color);
            int hr = DwmSetWindowAttribute(hwnd, DWMWA_TEXT_COLOR, ref colorRef, sizeof(int));
            return hr >= 0;
        }


        public static bool SetBorderColor(IntPtr hwnd, Color color)
        {
            int colorRef = ColorTranslator.ToWin32(color);
            int hr = DwmSetWindowAttribute(hwnd, DWMWA_BORDER_COLOR, ref colorRef, sizeof(int));
            return hr >= 0;
        }
    }


}
