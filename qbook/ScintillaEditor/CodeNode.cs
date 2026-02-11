
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.SolutionPersistence.Model;
using QB;
using qbook.CodeEditor;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Shapes;
using RoslynDocument = Microsoft.CodeAnalysis.Document; // Alias gegen Kollision mit ScintillaNET.Document


namespace qbook.ScintillaEditor
{
    public class CodeNode : TreeNode
    {
  
        public oPage Page { get; set; }
        public bool Active { get; set; } = true;
        public RoslynDocument RoslynDoc { get; set; }
        public (AdhocWorkspace Workspace, ProjectId Id) Adhoc;
        public DocumentEditor Editor { get; set; }
        public DataTable Output = new DataTable();
        public DataTable MethodesClasses = new DataTable();
        private RoslynFoldingHelper RoslynFoldingHelper;
        public int CodeIndex { get; set; } = 0;
        public bool HasErrors => Output.Rows.Count > 0;
        public string SubcodeKey { get; set; }
        public string FileName { get; set; }

        public CodeNode PageNode;

        System.Windows.Forms.Panel EditorPanel;
        public NodeType Type { get; set; }
        public enum NodeType
        {
            Page,
            SubCode,
            Book,
            Program
        }

        bool init = true;
        private static string TrimCodeText(string file)
        {
            file = file.Replace(".cs", "");
            string[] parts = file.Split('.').ToArray();

            if (parts.Count() > 1)
                return parts[1];
            else
                return parts[0];
        }
        public CodeNode(oPage page, string fileName, NodeType type, string subcodeKey = null, CodeNode pageNode = null) : base(fileName.Replace(".cs",""))
        {
   
            Page = page;
            FileName = fileName;
            Type = type;
            PageNode = pageNode;
            SubcodeKey = subcodeKey;

            Output.Columns.Add("Page", typeof(string));
            Output.Columns.Add("Class", typeof(string));
            Output.Columns.Add("Position", typeof(string));
            Output.Columns.Add("Length", typeof(int));
            Output.Columns.Add("Type", typeof(string));
            Output.Columns.Add("Description", typeof(string));
            Output.Columns.Add("Node", typeof(CodeNode));

            MethodesClasses.Columns.Add("Row", typeof(int));
            MethodesClasses.Columns.Add("Name", typeof(string));

            RoslynFoldingHelper = new RoslynFoldingHelper();

            if (Type == NodeType.Page)
            {
                this.Text = page.FullName;
                this.Name = page.FullName;
                this.ImageIndex = 2;
            }
            else
            {
               if(page == null)
                  

                this.ImageIndex = 3;
                this.Text = TrimCodeText(subcodeKey);
                if (page == null)
                    this.Name = subcodeKey;
                else
                  this.Name = $"{page.FullName}_{subcodeKey}";

                this.PageNode = pageNode;
            }

            Editor = new DocumentEditor(null, page);
            Editor.Init();
        

            //Editor.ZoomChanged += (s, e) =>
            //{
            //    RosylnSignatureHelper.ListFont = GetFont();
            //    RoslynAutoComplete.ListFont = GetFont();
            //};

        }
        public void Select()
        {
            RosylnSignatureHelper.ActiveNode = this;
            RoslynAutoComplete.ActiveNode = this;
            RosylnSignatureHelper.ListFont = GetFont();
            RoslynAutoComplete.ListFont = GetFont();

            //Editor.KeyDown -= RosylnSignatureHelper.Editor_KeyDown;
            //Editor.CharAdded -= RosylnSignatureHelper.Editor_CharAdded;

            //Editor.KeyDown += RosylnSignatureHelper.Editor_KeyDown;
            //Editor.CharAdded += RosylnSignatureHelper.Editor_CharAdded;

           // Editor.KeyDown -= RoslynAutoComplete.Editor_KeyDown;
         //   Editor.CharAdded -= RoslynAutoComplete.Editor_CharAdded;

          //  Editor.KeyDown += RoslynAutoComplete.Editor_KeyDown;
           
        //    Editor.CharAdded += RoslynAutoComplete.Editor_CharAdded;
        }
        public CodeNode(string name) : base(name)
        {
            Type = NodeType.Book;
        }
        public RoslynDocument UpdateDocument()
        {
            try
            {
        
                if (!Active) return RoslynDoc;
               // var workspace = Adhoc.Workspace;
                var docId = RoslynDoc.Id;
                var doc = Adhoc.Workspace.CurrentSolution.GetDocument(docId);
                var newText = SourceText.From(Editor.Text, Encoding.UTF8);
                if (Editor.ReadOnly)
                {
                  
                    return RoslynDoc;
                }
                var updatedDoc = doc.WithText(newText);
                if (Adhoc.Workspace.TryApplyChanges(updatedDoc.Project.Solution))
                    RoslynDoc = Adhoc.Workspace.CurrentSolution.GetDocument(docId);

              

                return RoslynDoc;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdateDocument failed: " + ex.Message);
                return null;
            }
        }
        public async Task UpdateRoslyn()
        {
            await Editor.UpdateRoslyn("CodeNode " + Name);

            return;
            if (Type == NodeType.Book) return;
            Editor.Active = Active;

            

            if (!Active)
            {
                Editor.Text = Editor.Text;
            }
            else
            {

                if (init)
                {
                    Editor.RoslynDoc = RoslynDoc;
                    await Editor.FormatDocumentAsync();
                    RoslynFoldingHelper.InitializeFolding(Editor);
                    RoslynFoldingHelper.ApplyFolding(Editor);
                    init = false;
                }

                UpdateDocument();
                RoslynFoldingHelper.InitializeFolding(Editor);
                LockNecessary();
                await RosylnSemantic.ApplyAsync(Editor, RoslynDoc);
                Output = await RoslynDiagnostic.ApplyAsync(this);
                await UpdateMethodesFromRoslynAsync();
            }

            LockNecessary();

        }
        public async Task FormatCode()
        {
            int pos = Editor.CurrentPosition;
            int firstVisibleLine = Editor.FirstVisibleLine;
            await Editor.FormatDocumentAsync();
            await UpdateRoslyn();
            Editor.FirstVisibleLine = firstVisibleLine;
            Editor.GotoPosition(pos);
        }

        //================ Qbook custom rules =====================

        public (int Start, int End) UsingLines;
        bool usingsHidden = false;
        public System.Drawing.Font GetFont()
        {
            var style = Editor.Styles[Style.Default];
            string fontName = style.Font;
            int baseSize = style.Size;
            int zoom = Editor.Zoom;
            int effectiveSize = baseSize + zoom;

            return new System.Drawing.Font(fontName, effectiveSize);
        }

        public (int Start, int End) IncudeBlock;
        public void LockNecessary()
        {
            Editor.ResetHideProteced();
            var lines = Editor.Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            int startLine = -1;
            int endLine = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                if (startLine == -1 && lines[i].Contains("//<CodeStart>"))
                    startLine = i;

                if (startLine != -1 && lines[i].Contains("//<CodeEnd>"))
                {
                    endLine = i;
                    break;
                }
            }

            if (startLine != -1 && endLine != -1)
            {
                // Zeilen außerhalb des SubCode-Bereichs ausblenden
                Editor.HideProtectLines(0, startLine); // Zeilen vor SubCode
                Editor.HideProtectLines(endLine, Editor.Lines.Count-1); // Zeilen nach SubCode
                Debug.WriteLine("StartLine " + startLine);
                Debug.WriteLine("EndLine " + endLine + ".." + Editor.Lines.Count);
            }
        }
        public async Task UpdateMethodesFromRoslynAsync()
        {
            MethodesClasses.Clear();

            var root = await RoslynDoc.GetSyntaxRootAsync();
            if (root == null) return;

            // Alle Klassen im Dokument finden
            var classNodes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classNode in classNodes)
            {
                string className = classNode.Identifier.Text;

                // Klasse eintragen
                DataRow classRow = MethodesClasses.NewRow();
                classRow["Row"] = classNode.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                classRow["Name"] = "[C] " + className;
                MethodesClasses.Rows.Add(classRow);

                // Alle Methoden in der Klasse finden
                var methodNodes = classNode.DescendantNodes().OfType<MethodDeclarationSyntax>();

                foreach (var method in methodNodes)
                {
                    string methodName = method.Identifier.Text;
                    string fullName = $"{className}.{methodName}";

                    DataRow methodRow = MethodesClasses.NewRow();
                    methodRow["Row"] = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    methodRow["Name"] = "[M] " + fullName;
                    MethodesClasses.Rows.Add(methodRow);
                }
            }
        }

        // Code Include / Exclude 
        public async Task ToggleInclude()
        {
            if (Type != NodeType.SubCode || PageNode == null) return;

            if (Active)
            {
              //  ExcludeCode(this);
                Active = false;

                this.ImageIndex = 4; // Inactive icon

                var solution = Adhoc.Workspace.CurrentSolution;
                var newSolution = solution.RemoveDocument(RoslynDoc.Id);
                Adhoc.Workspace.TryApplyChanges(newSolution);

                await UpdateRoslyn();
            }
            else
            {
             //   IncludeCode(this);
                Active = true;
                this.ImageIndex = 3; // Active icon

                var sourceText = SourceText.From(Editor.Text, Encoding.UTF8);

                RoslynDoc = Adhoc.Workspace.AddDocument(Adhoc.Id, FileName, SourceText.From(Editor.Text, Encoding.UTF8));

                await UpdateRoslyn();
              //  HideIncludeBlock();
            }

            Editor.Active = Active;
            Editor.RefreshView();

            await PageNode.Editor.UpdateRoslyn("CodeNode -> Page" + Name);
        }
        public async Task Rename(string newName)
        {
            await UpdateRoslyn();

            string fileName = newName.EndsWith(".cs") ? newName : newName + ".cs";

            var originalDocument = RoslynDoc;
            var solution = Core.Workspace.CurrentSolution;

            var originText = await originalDocument.GetTextAsync();

            // Neues DocumentId erzeugen
            Microsoft.CodeAnalysis.DocumentId newDocId = Microsoft.CodeAnalysis.DocumentId.CreateNewId(originalDocument.Project.Id, fileName);

            // Neue Lösung mit entferntem alten und hinzugefügtem neuen Dokument
            var newSolution = solution
                .RemoveDocument(originalDocument.Id)
                .AddDocument(newDocId, fileName, originText);

            // Änderungen anwenden
            Adhoc.Workspace.TryApplyChanges(newSolution);

            // Neues Dokument setzen
            RoslynDoc = newSolution.GetDocument(newDocId);

            if (Type == NodeType.Page)
            {
                Editor.FindReplace($"Definition{Text}", $"Definition{newName}");
                foreach (CodeNode sub in Nodes)
                {
                    Debug.WriteLine(sub.Text);
                    sub.Editor.FindReplace($"Definition{Text}", $"Definiton{newName}");
                    sub.UpdateDocument();
                }
            }

            this.Text = newName;
            this.Name = $"{Page.FullName}_{newName}";

            UpdateDocument();

        }
        public async Task AddFileToRoslyn()
        {
            if (Type == NodeType.Book) return;
            var sourceText = SourceText.From(Editor.Text);
            RoslynDoc = Adhoc.Workspace.AddDocument(Adhoc.Id, FileName, SourceText.From(Editor.Text, Encoding.UTF8));
            await UpdateRoslyn();
        }
        public async Task RemoveFileFromRoslyn()
        {
            if (Type == NodeType.Book) return;
            var solution = Adhoc.Workspace.CurrentSolution;
            var newSolution = solution.RemoveDocument(RoslynDoc.Id);
            Adhoc.Workspace.TryApplyChanges(newSolution);
            RoslynDoc = null;
          
        }
        public string NewPageCode(string name) => newPage(name);
        private string newPage(string name)
        {
            return $@"namespace Definition{name} {{ //<CodeStart>
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using QB;

    public class qPage
    {{
        //common fields/properties/methods/classes/types go here

        public void Initialize()
        {{
            //initialization code goes here

        }}

        public void Run()
        {{
            //run/work code goes here

        }}

        public void Destroy()
        {{
            //destroy/cleanup code goes here
        }}
    }}
    //<CodeEnd>
}}
";
        }

        public string NewSubCode() => newSubCode();
        private string newSubCode()
        {
            return $@"namespace Definition{Page.Name} {{
    //<CodeStart>
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using QB;

    public class CustomCode()
    {{

    }}
    //<CodeEnd>
}}
";
        }

    }
}
