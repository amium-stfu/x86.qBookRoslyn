using CSScripting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Rename;
using Newtonsoft.Json;
using qbook.CodeEditor;
using qbook.ScintillaEditor.InputControls;
using qbook.UI;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
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
       
        


        CodeNode RootNode;
        CodeNode ProgramNode;
        oPage SelectedPage = new oPage();

        DataGridView GridViewDiagnosticOutput;
        public DataTable TblFindReplaceOutputs = new DataTable();

        DataTable tblBuildDiagnosic = new DataTable();
        private FormFindReplace _findReplaceControl;

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
            RosylnSignatureHelper.Init(Core.Roslyn);
            RoslynAutoComplete.Init(Core.Roslyn);
          

            InitGridViews();
      
            ApplyLightTheme();

            panelOutput.Controls.Clear();
            panelOutput.Controls.Add(GridViewDiagnosticOutput);
            vBarOutputs.Init(GridViewDiagnosticOutput);
            UpdateOutputButtons();

            flowLayoutPageData.Controls.Add(new TextBoxWithLabel ("Page Name:", () => SelectedPage.Name, v => SelectedPage.Name = v) {ReadOnly = true });
            flowLayoutPageData.Controls.Add(new TextBoxWithLabel ("Page Text:", () => SelectedPage.Text, v => SelectedPage.Text = v));

            flowLayoutPageData.Controls.Add(
                new TextBoxWithLabel(
                    "Page Format:",
                    () => SelectedPage.Format,
                    v => SelectedPage.Format = v,
                    new List<string> { "A4", "16/9", "16/10" }
                )
            );

        }


        public void ClearTabs()
        {
            PanelTabs.Controls.Clear();
        }

        #region TreeView


        private void ProjectTree_DragDrop(object sender, DragEventArgs e)
        {
            List<string> pageOder = new List<string>();
            foreach (CodeNode page in ProjectTree.Nodes[0].Nodes)
                if (page.Type == CodeNode.NodeType.Page) pageOder.Add(page.Text);

            Core.ThisBook.PageOrder = new List<string>(pageOder);

        }


        #endregion


        #region Load Save Book

        public async Task NewBook()
        {
            Book newBook;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            using (SaveFileDialog openFileDialog = new SaveFileDialog())
            {
                openFileDialog.Title = "New qBook";
                openFileDialog.Filter = "qbook files (*.qbook)|*.qbook|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    newBook = new Book();
                    newBook.Main = new oControl();

                    Directory.CreateDirectory(Path.GetDirectoryName(openFileDialog.FileName));
                    newBook.Filename = Path.GetFileName( openFileDialog.FileName);
                    newBook.Directory = Path.GetDirectoryName(openFileDialog.FileName);
                }
                else
                {
                    return;
                }
            }

            Core.Roslyn.Reset();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Core.Roslyn = new RoslynService();
            ProjectTree.Nodes.Clear();

            ProjectTree.BeginUpdate();
            ProjectTree.ImageList = BookTreeViewIcons;
            ProjectTree.Nodes.Clear();

     

           

            RootNode = new CodeNode(qbook.Core.ThisBook.Filename)
            {
                ImageIndex = 1
            };

            ProjectTree.Font = new Font("Calibri", 12);
            ProjectTree.Nodes.Add(RootNode);

            string program = "namespace QB\r\n{\r\n\tpublic static class Program \r\n\t{\r\n";
            var roslynFiles = new List<(string fileName, string code)>();

            List<string> Pages = new List<string>();
            Pages.Add("Page1");
            oPage page1 = new oPage("Page1", "Page 1");
            CodeNode page = new CodeNode(page1, "Page1.qPage.cs", CodeNode.NodeType.Page, "Page1");
            page.CodeIndex = 1;
            page.Editor.Text = page.NewPageCode("Page1");
            roslynFiles.Add(("Page1.qPage.cs", page.Editor.Text));
            newBook.PageOrder.Add("Page1");
            page1.CodeOrder.Add("Page1.qPage.cs");

            newBook.Main.Objects.Add(page1);

            ProjectTree.Nodes[0].Nodes.Add(page);



            program += "\t\tpublic static " + "DefinitionPage1.qPage" + " " + page.Name + " { get; } = new " + "DefinitionPage1.qPage" + "();\r\n";


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

            ProgramNode = new CodeNode(null, "Program.cs", CodeNode.NodeType.Program, "Program.cs");
            ProgramNode.CodeIndex = 0;
            ProgramNode.Editor.Text = program;
            ProgramNode.Editor.EmptyUndoBuffer();
            ProgramNode.Active = true;

            ProgramNode.Editor.GoToDefinition = () => GoToDefinitionAsync();
            ProgramNode.Editor.RenameSymbol = () => RenameSymbolAsync();
            RootNode.Nodes.Add(ProgramNode);



            Debug.WriteLine("=========  Program ===========");
            Debug.WriteLine(program);
            Debug.WriteLine("==============================");
            roslynFiles.Add(("GlobalUsing.cs", "global using static QB.Program;"));
            CodeNode Global = new CodeNode(null, "GlobalUsing.cs", CodeNode.NodeType.Program, "GlobalUsing.cs");
            Global.CodeIndex = 0;
            Global.Editor.Text = "global using static QB.Program;";
            Global.Editor.EmptyUndoBuffer();
            Global.Active = true;

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
            await Core.Roslyn.LoadInMemoryProjectAsync(roslynFiles, references);

            AdhocWorkspace ws = Core.Roslyn.GetWorkspace;
            ProjectId id = Core.Roslyn.GetProjectId;


            // Verknüpfe alle EditorNodes mit den Roslyn Documents
            foreach (System.Windows.Forms.TreeNode node in ProjectTree.Nodes[0].Nodes)
            {
                if (node is CodeNode editorNode)
                {
                    editorNode.RoslynDoc = Core.Roslyn.GetDocumentByFilename(editorNode.FileName);
                    editorNode.Adhoc.Workspace = ws;
                    editorNode.Adhoc.Id = id;
                }

                foreach (System.Windows.Forms.TreeNode sub in node.Nodes)
                {
                    if (sub is CodeNode subNode)
                    {
                        subNode.RoslynDoc = Core.Roslyn.GetDocumentByFilename(subNode.FileName);
                        subNode.Adhoc.Workspace = ws;
                        subNode.Adhoc.Id = id;
                    }
                }
            }

            Core.ThisBook = newBook;
            Core.ActualMain = newBook.Main;
            Core.ThisBook.Init();


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
            public string BackupDirectory { get; set; } = null;
            public string Language { get; set; } = null;
            public List<string > PageOrder { get; set; } = new List<string>();


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
                BackupDirectory = Core.ThisBook.BackupDirectory,
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

            var code = await Core.Roslyn.GetDocumentTextAsync("Program.cs");
            string path = Path.Combine(newFile, "Program.cs");
            File.WriteAllText(path, code);

            code = await Core.Roslyn.GetDocumentTextAsync("GlobalUsing.cs");
            path = Path.Combine(newFile, "GlobalUsing.cs");
            File.WriteAllText(path, code);

            // 4️⃣ Projektbeschreibung schreiben
            string projectJson = JsonConvert.SerializeObject(project, Formatting.Indented);
            File.WriteAllText(Path.Combine(newFile, "Book.json"), projectJson);
 
        }

        class Node : System.Windows.Forms.TreeNode
        {
            public DocumentEditor Editor;
            public Node(string name) : base(name) { }
        }

        class PageEditor
        {
            public DocumentEditor PageRoot;
            public List<DocumentEditor> SubCodes;

            public PageEditor(DocumentEditor page, List<DocumentEditor> subs)
            {
                PageRoot = page;
                SubCodes = subs;
            } 
        }

        Dictionary<string, PageEditor> PageEditors = new Dictionary<string, PageEditor>();
        public async Task SetProjectTree()
        {
            PageEditors.Clear();

            Debug.WriteLine("Creating ProjectTree");
            PanelTabs.Controls.Clear();
            ProjectTree.BeginUpdate();
            ProjectTree.ImageList = BookTreeViewIcons;
            ProjectTree.Nodes.Clear();
            Node Root = new Node(Core.ThisBook.Filename.Replace(".qbook", ""));
            RootNode.ImageIndex = 1;
            ProjectTree.Font = new Font("Calibri", 12);
            ProjectTree.Nodes.Add(Root);

            foreach (oPage page in qbook.Core.ActualMain.Objects.OfType<oPage>())
            {
                Node pageNode = new Node(page.Name);
                pageNode.Editor = new DocumentEditor(page.RoslynCodeDoc, page);
     
                List<DocumentEditor> subEditors = new List<DocumentEditor>();
                foreach (CodeDocument doc in page.SubCodeDocuments.Values)
                {
                    Node subNode = new Node(doc.Filename);
                    subNode.Editor = new DocumentEditor(doc, page);
                    subEditors.Add(subNode.Editor);

                    pageNode.Nodes.Add(subNode);
                }

                PageEditors[page.Name] = new PageEditor(pageNode.Editor, subEditors);


            }

            ProjectTree.EndUpdate();

        }



        public async Task CreateProjectTree(bool rebuild = false)
        {

            Debug.WriteLine("Creating ProjectTree");
            if(!rebuild)
            PanelTabs.Controls.Clear();
            ProjectTree.BeginUpdate();
            ProjectTree.ImageList = BookTreeViewIcons;
            ProjectTree.Nodes.Clear();
            RootNode = new CodeNode(Core.ThisBook.Filename.Replace(".qbook",""));
            RootNode.ImageIndex = 1;
            ProjectTree.Font = new Font("Calibri", 12);
            ProjectTree.Nodes.Add(RootNode);

            foreach (oPage page in qbook.Core.ActualMain.Objects.OfType<oPage>())
            {
                CodeNode pageNode = new CodeNode(page, page.Filename, CodeNode.NodeType.Page, page.Name);
                pageNode.Editor.Text = await Core.Roslyn.GetDocumentTextAsync(page.Filename);
                pageNode.Editor.EmptyUndoBuffer();
                pageNode.Active = true;
                pageNode.Editor.GoToDefinition = () => GoToDefinitionAsync();
                pageNode.Editor.RenameSymbol = () => RenameSymbolAsync();
                pageNode.RoslynDoc = Core.Roslyn.GetDocumentByFilename(page.Filename); ;
                pageNode.Adhoc.Workspace = Core.Roslyn.GetWorkspace;
                pageNode.Adhoc.Id = Core.Roslyn.GetProjectId;

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

                RootNode.Nodes.Add(pageNode);


                foreach (oCode sub in page.SubCodes.Values)
                {
                    page.CodeOrder.Add(sub.Filename);
                    CodeNode subNode = new CodeNode(page, sub.Filename, CodeNode.NodeType.SubCode, sub.Filename, pageNode);
                    subNode.Editor.Text = await Core.Roslyn.GetDocumentTextAsync(sub.Filename);
                    subNode.Active = sub.Active;
                    subNode.RoslynDoc = Core.Roslyn.GetDocumentByFilename(sub.Filename);
                    subNode.Editor.EmptyUndoBuffer();
                    subNode.Editor.GoToDefinition = () => GoToDefinitionAsync();
                    subNode.Editor.RenameSymbol = () => RenameSymbolAsync();
                    subNode.Adhoc.Workspace = Core.Roslyn.GetWorkspace;
                    subNode.Adhoc.Id = Core.Roslyn.GetProjectId;
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
                    subNode.ImageIndex = subNode.Active ? 3 : 4;
                    pageNode.Nodes.Add(subNode);
                    subNode.Editor.ApplyLightTheme();
                }
            }

            CodeNode ProgramNode = new CodeNode(null, "Program.cs", CodeNode.NodeType.Program, "Program.cs");
            ProgramNode.CodeIndex = 0;

            var code = await Core.ThisBook.Program.GetTextAsync();

            ProgramNode.Editor.Text = code.ToString();
            ProgramNode.Editor.EmptyUndoBuffer();
            ProgramNode.Active = true;
            ProgramNode.RoslynDoc = Core.Roslyn.GetDocumentByFilename("Program.cs");
            ProgramNode.Adhoc.Workspace = Core.Roslyn.GetWorkspace;
            ProgramNode.Adhoc.Id = Core.Roslyn.GetProjectId;

            ProgramNode.Editor.GoToDefinition = () => GoToDefinitionAsync();
            ProgramNode.Editor.RenameSymbol = () => RenameSymbolAsync();
            RootNode.Nodes.Add(ProgramNode);

            CodeNode Global = new CodeNode(null, "GlobalUsing.cs", CodeNode.NodeType.Program, "GlobalUsing.cs");
            Global.CodeIndex = 0;
            code = await Core.ThisBook.Global.GetTextAsync();
            Global.RoslynDoc = Core.Roslyn.GetDocumentByFilename("GlobalUsing.cs");
            Global.Editor.Text = code.ToString();
            Global.Editor.EmptyUndoBuffer();
            Global.Active = true;
            Global.Adhoc.Workspace = Core.Roslyn.GetWorkspace;
            Global.Adhoc.Id = Core.Roslyn.GetProjectId;

            Global.Editor.GoToDefinition = () => GoToDefinitionAsync();
            Global.Editor.RenameSymbol = () => RenameSymbolAsync();
            RootNode.Nodes.Add(Global);

            ResetTreeViewNodes();
            RootNode.Expand();
            ProjectTree.EndUpdate();
        }

        public void RemoveProjectTree()
        {
            // Zusätzliche: Find/Replace Control freigeben
            if (_findReplaceControl != null)
            {
                _findReplaceControl.Dispose();
                _findReplaceControl = null;
            }

            panelEditor.Controls.Clear();

            foreach (CodeNode node in ProjectTree.Nodes)
            {
                DisposeNode(node);
            }

            ProjectTree.Nodes.Clear();
            SelectedNode = null;
            RootNode = null;
            ProgramNode = null;
}

        void DisposeNode(CodeNode node)
        {
            // Editor-Events lösen (KeyDown / eigene Delegates)
            if (node.Editor != null)
            {
                try
                {
                    node.Editor.KeyDown -= null; // keine direkte Referenz – falls du eigene Handler speicherst, explizit abmelden
                    node.Editor.GoToDefinition = null;
                    node.Editor.RenameSymbol = null;
                }
                catch { }

                node.Editor.Dispose();
                node.Editor = null;
            }

            // Roslyn-Referenzen lösen
            node.RoslynDoc = null;
            if (node.Adhoc.Workspace != null)
            {
                node.Adhoc.Workspace = null;
                node.Adhoc.Id = null;
            }

            node.PageNode = null;
            node.Page = null;

            foreach (CodeNode child in node.Nodes)
                DisposeNode(child);

            node.Nodes.Clear();
        }


        #endregion

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }


        CodeNode _clickedNode;

        public void RefreshPageData()
        {
            foreach(System.Windows.Forms.Control c in flowLayoutPageData.Controls)
            {
                if(c is InputControls.TextBoxWithLabel tb)
                {
                    tb.RefreshValue();
                }
            }
        }

        private async void ProjectTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                
                if (e.Node is CodeNode) {

                    await OpenNode(e.Node as CodeNode);
               
                }
                //   
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
                    deletePageToolStripMenuItem.Visible = false;

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
                dataGridViewBuildOutput.Columns["Count"].Width = 40;
                dataGridViewBuildOutput.Columns["RepeatMs"].Width = 40;
                dataGridViewBuildOutput.Columns["Key"].Visible = false;
                dataGridViewBuildOutput.Columns["File"].Width = 150;
                dataGridViewBuildOutput.Columns["Methode"].Width = 150;
                dataGridViewBuildOutput.Columns["Line"].Visible = false;
                dataGridViewBuildOutput.Columns["Col"].Visible = false;

                dataGridViewBuildOutput.Columns["Reason"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                

               
            };
            dataGridViewBuildOutput.AllowUserToResizeColumns = false;

            dataGridViewBuildOutput.DataSource = QB.GlobalExceptions.RuntimeErrors;
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
            dataGridViewBuildOutput.DefaultCellStyle.BackColor = Color.Tomato;
            dataGridViewBuildOutput.CellClick += (s, e)  =>
            {
                if (e.RowIndex >= 0)
                {
                    string file = dataGridViewBuildOutput.Rows[e.RowIndex].Cells["File"].Value.ToString();
                    int line = (int)dataGridViewBuildOutput.Rows[e.RowIndex].Cells["Line"].Value;
                    CodeNode node = GetNodeByFilename(file);
                    OpenNode(node);
                    node.Editor.HighlightLine(line, Color.Red);
                    node.Editor.Refresh();
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

            foreach (System.Windows.Forms.Control control in panelControl.Controls)
            {

                if (control is System.Windows.Forms.Button) {
                    System.Windows.Forms.Button b = control as System.Windows.Forms.Button;
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
                    old?.Dispose();
                }
            }

            flowLayoutPageData.BackColor = _backColor;
            foreach (System.Windows.Forms.Control c in flowLayoutPageData.Controls)
            {
                if (c is InputControls.TextBoxWithLabel tb) tb.ApplyTheme();
              
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
            RefreshTabs();

     
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


            flowLayoutPageData.BackColor = _backColor;
            foreach (System.Windows.Forms.Control c in flowLayoutPageData.Controls)
            {
                if (c is InputControls.TextBoxWithLabel tb) tb.ApplyTheme();

            }

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

            RefreshTabs();
        }
        public void ToggleTheme()
        {
            if (!Theme.IsDark) 
            {
                ApplyDarkTheme();
                if(panelEditor.Controls[0] is DocumentEditor)
                ((DocumentEditor)panelEditor.Controls[0]).ApplyDarkTheme();
            }
            else 
            {
                ApplyLightTheme();
                if (panelEditor.Controls[0] is DocumentEditor)
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
            if(SelectedNode != null) node.SelectedImageIndex = SelectedNode.ImageIndex;

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

        public async Task OpenNodeByName(string NodeName)
        {

            SafeInvoke(ProjectTree, async () =>
            {

                ProjectTree.SelectedNode = null;
                foreach (CodeNode node in ProjectTree.Nodes[0].Nodes)
                {
                    if (node.Name == NodeName)
                    {
                        ProjectTree.SelectedNode = node;
                        node.EnsureVisible();
                        node.Expand();
                        await OpenNode(node);
                        break;
                    }

                    foreach (CodeNode subnode in node.Nodes)
                    {
                        //    Debug.WriteLine(subnode.Name);
                        if (subnode.Name == NodeName)
                        {
                            ProjectTree.SelectedNode = subnode;
                            subnode.EnsureVisible();
                            subnode.Expand();
                            await OpenNode(subnode);
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

            if (node.Type == CodeNode.NodeType.Page || node.Type == CodeNode.NodeType.SubCode)
            {
                SelectedPage = node.Page;
            }
            else
            {
                SelectedPage = new oPage();
            }

            RefreshPageData();
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

            UpdateTabs(node);



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
                    if (!sub.Active) continue;
                    if (sub.RoslynDoc.Name == doc.Name)
                    {
                        Debug.WriteLine("node found: " + sub.Name);
                        return sub;
                    }
                }
            }

            return null;
        }

        private CodeNode? GetNodeByFilename(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return null;

            foreach (CodeNode node in ProjectTree.Nodes[0].Nodes)
            {

                if (node.FileName == filename)
                {
                    Debug.WriteLine("node found: " + node.Name);
                    return node;
                }

                foreach (CodeNode sub in node.Nodes)
                {
                    if (sub.FileName == filename)
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
            var loc = await Core.Roslyn.GoToDefinitionAsync(SelectedNode.RoslynDoc, caret);
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
            pageNode.Adhoc.Workspace = Core.Roslyn.GetWorkspace;
            pageNode.Adhoc.Id = Core.Roslyn.GetProjectId;
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

        #endregion

        #region FindReplace 
        private void ShowFindReplaceControl()
        {
            if(!panelEditor.Controls.Contains(_findReplaceControl))
                panelEditor.Controls.Add(_findReplaceControl);

            _findReplaceControl.Location = new Point(panelEditor.Width - _findReplaceControl.Width, 0); // Rechts oben
            //if (!Theme.IsDark) _findReplaceControl.LightTheme();
            //if (Theme.IsDark) _findReplaceControl.DarkTheme();
            _findReplaceControl.Editor = SelectedNode.Editor;
            _findReplaceControl.BringToFront();
            _findReplaceControl.Visible = true;


        }
        private void ShowSearchBar()
        {
           //if (SelectedNode == null) return;

           // if (_findReplaceControl == null)
           // {
           //     _findReplaceControl = new ControlFindReplace(Root);
           //     panelEditor.Controls.Add(_findReplaceControl);
           // }

           // ShowFindReplaceControl();
           // _findReplaceControl.ShowFind();
           // _findReplaceControl.FocusFind();
        }
        private void ShowReplaceBar()
        {
            //if (SelectedNode == null) return;

          

            //if (_findReplaceControl == null)
            //{
            //    _findReplaceControl = new ControlFindReplace(this, Core.Roslyn, dataGridViewFindReplace);
            //    panelEditor.Controls.Add(_findReplaceControl);
            //}

            //ShowFindReplaceControl();
            //_findReplaceControl.ShowReplace();
            //_findReplaceControl.FocusFind();
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

            var allDocs = Core.Roslyn.GetAllDocuments();
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


        #region Tab Control

        public Dictionary<string, ControlTab> DictTabs = new Dictionary<string, ControlTab>();
        private void UpdateTabs(CodeNode node)
        {
           

            if (!DictTabs.ContainsKey(node.Name))
            {
                ControlTab tab = new ControlTab(node)
                {
                    SelectTab = () => OpenNodeByName(node.Name),
                    RemoveTab = () => RemoveTab(node),
                    CloseOtherTabs = () => CloseAllOtherTabs(),
                    LoactionForm = this,
                    Height = 35
                };

                DictTabs.Add(node.Name, tab);

                RefreshTabs();
            }
        }

        private void RemoveTab(CodeNode node)
        {
            DictTabs.Remove(node.Name);
            RefreshTabs();
        }

        private void CloseAllOtherTabs()
        {
            List<string> keysToRemove = new List<string>();
            foreach (var key in DictTabs.Keys)
            {
                if (DictTabs[key].Node != SelectedNode)
                {
                    keysToRemove.Add(key);
                }
            }
            foreach (var key in keysToRemove)
            {
                DictTabs.Remove(key);
            }
            RefreshTabs();
        }


        private void RefreshTabs()
        {
         
            PanelTabs.SuspendLayout();
            PanelTabs.Invoke((System.Action)(() =>
            {
                PanelTabs.Controls.Clear();
                foreach (ControlTab tab in DictTabs.Values.OrderBy(t => t.Name))
                {
                    PanelTabs.Controls.Add(tab);
                }
            }));

            double controlWidth = 0;
            int count = 0;
            foreach (ControlTab tab in PanelTabs.Controls)
            {
                count++;
                controlWidth += tab.Width;
                tab.ApplyTheme();
                if(tab.NodeName == SelectedNode.Name)
                {
                    tab.Selected();
                } 
            }
            Debug.WriteLine("c " + count);
            int rows = (int)Math.Ceiling( controlWidth / PanelTabs.Width);

            EditorLayoutPanel.RowStyles[0].Height = rows * 35;

            PanelTabs.ResumeLayout();
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
            await VisualRebuild();
        }

        private async void btnRebuild_Click(object sender, EventArgs e)
        {
            bool result = await VisualRebuild();

            if (result)
            {
                BookRuntime.InitializeAll();
                MainForm.SetStatusText("qbook rebuils successfully!", 3000);
            }

        }

        public async Task<bool> VisualRebuild()
        {
            string nodeName = "none";
            RootNode.Expand();

            ResetTreeViewNodes(collapse: true);

            if (SelectedNode != null)
                nodeName = SelectedNode.Name;
            //var task = BookRuntime.BuildBookAssembly(rebuild: true);
            var task = BookRuntime.BuildBookAssembly();
            while (!task.IsCompleted)
            {
                SetStatusText(BookRuntime.BuildResult);
                await Task.Delay(100);
            }
            await task;
            await CreateProjectTree();
            if (nodeName != "none")
                await OpenNodeByName(nodeName);

            if (!BookRuntime.BuildSuccess)
            {
                ProjectTree.SelectedNode = null;
                ShowBuildErrors();
                return false;
            }
            return true;
        }

        public bool HasError => Core.Roslyn.BuildSuccess;

        public async Task Rebuild()
        {
           ResetTreeViewNodes(collapse:true);

            var task = Core.Roslyn.CreateAssemblyFromTree(ProjectTree);

            while (!task.IsCompleted)
            {
                SetStatusText(Core.Roslyn.BuildResult);
                await Task.Delay(100);
            }
            await task;

            SetStatusText(Core.Roslyn.BuildResult);
            ShowBuildErrors();

        }

        public void ResetTreeViewNodes(bool collapse = false)
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
            try
            {
                if (BookRuntime.ErrorFiles.Count > 0)
                {
                    foreach (string err in BookRuntime.ErrorFiles)
                    {
                        string file = err.GetFileName();
                        CodeNode node = GetNodeByDocument(Core.Roslyn.GetDocumentByFilename(file));
                        if (node == null) continue;
                        node.PageNode.ImageIndex = 10;
                        node.ImageIndex = 10;
                    }
                }
            }
            catch(System.Exception ex)
            {
                Debug.WriteLine("[ShowBuildError] failed: " + ex.Message);
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
            if (newname != null)
            {
                ProgramNode.Editor.FindReplace($"Definition{_clickedNode.Text}", $"Definition{newname}");
                await _clickedNode.Rename(newname);
        
            }
                

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
            BookRuntime.RunAll();
          
            SetStatusText("[Build] running...");

        }

        private void btnShowHidden_Click(object sender, EventArgs e)
        {
            SelectedNode.Editor.ToggleHidenLines();
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            await Core.SaveInFolder();


 
        }

        private async void customToolStripMenuItem_Click(object sender, EventArgs e)
        {

            string name = ShowInputDialog("Input code name:", $"New subcode", "CustomCode");

            if (!string.IsNullOrWhiteSpace(name))
            {
                _clickedNode.Nodes.Add(await newSubCode(_clickedNode, name));
            }
            ResetTreeViewNodes(collapse: false);

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

        private void PanelTabs_Resize(object sender, EventArgs e)
        {
            RefreshTabs();
        }
    }

    public class DoubleBufferedPanel : FlowLayoutPanel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }
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

    public class CustomTreeView : System.Windows.Forms.TreeView
    {
        public int InsertLineY { get; set; } = -1;

        public CustomTreeView()
        {
            DragDrop += ProjectTree_DragDrop;
            DragEnter += ProjectTree_DragEnter;
            DragOver += ProjectTree_DragOver;
            DragLeave += ProjectTree_DragLeave;
            ItemDrag += ProjectTree_ItemDrag;
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            base.WndProc(ref m);

            const int WM_PAINT = 0x000F;

            if (m.Msg == WM_PAINT && InsertLineY >= 0)
            {
                using (Graphics g = Graphics.FromHwnd(this.Handle))
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    g.DrawLine(pen, 0, InsertLineY, this.Width, InsertLineY);
                }
            }
        }
        private void ProjectTree_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void ProjectTree_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void ProjectTree_DragOver(object sender, DragEventArgs e)
        {
            Point pt = PointToClient(new Point(e.X, e.Y));
            System.Windows.Forms.TreeNode targetNode = GetNodeAt(pt);

            if (targetNode != null)
            {
                // Linie oberhalb oder unterhalb des Knotens je nach Mausposition
                InsertLineY = pt.Y < targetNode.Bounds.Top + targetNode.Bounds.Height / 2
                    ? targetNode.Bounds.Top
                    : targetNode.Bounds.Bottom;

                Invalidate(); // Neu zeichnen
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                InsertLineY = -1;
                Invalidate();
                e.Effect = DragDropEffects.None;
            }
        }

        private void ProjectTree_DragLeave(object sender, EventArgs e)
        {
            InsertLineY = -1;
            Invalidate(); // Linie entfernen
        }

        private void ProjectTree_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(CodeNode)))
            {
                Point pt = PointToClient(new Point(e.X, e.Y));
                System.Windows.Forms.TreeNode targetNode = GetNodeAt(pt);
                CodeNode draggedNode = (CodeNode)e.Data.GetData(typeof(CodeNode));

                if (targetNode != null &&
                    draggedNode != targetNode &&
                    !IsChildNode(draggedNode, targetNode) &&
                    draggedNode.Parent == targetNode.Parent)
                {
                    System.Windows.Forms.TreeNodeCollection siblings = targetNode.Parent?.Nodes ?? Nodes;

                    int targetIndex = targetNode.Index;

                    // Wenn draggedNode vor targetNode steht, wird der Index durch Remove verschoben
                    if (draggedNode.Index < targetIndex)
                        targetIndex--;

                    // Mausposition entscheidet, ob oberhalb oder unterhalb eingefügt wird
                    int insertIndex = pt.Y < targetNode.Bounds.Top + targetNode.Bounds.Height / 2
                        ? targetIndex
                        : targetIndex + 1;

                    draggedNode.Remove();
                    siblings.Insert(insertIndex, draggedNode);
                }

                InsertLineY = -1;
                Invalidate();
            }
        }
        private bool IsChildNode(System.Windows.Forms.TreeNode parent, System.Windows.Forms.TreeNode child)
        {
            while (child.Parent != null)
            {
                if (child.Parent == parent)
                    return true;
                child = child.Parent;
            }
            return false;
        }
    }


}
