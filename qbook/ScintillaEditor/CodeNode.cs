using DevExpress.DocumentServices.ServiceModel.DataContracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
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
        public int NodeID { get; set; }
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


        public CodeNode(string fileName,
                (AdhocWorkspace Workspace, ProjectId Id) adhoc,
                RoslynDocument doc,
                NodeType type = NodeType.Program)
    : base(fileName.Replace(".cs", ""))
        {
            FileName = fileName;
            Type = type;
            Adhoc = adhoc;
            RoslynDoc = doc;

            Editor = new DocumentEditor();
            Editor.Init();
            Editor.Text = doc != null ? doc.GetTextAsync().Result.ToString() : "";
            Editor.EmptyUndoBuffer();

            Editor.ReadOnly = true;
           // Editor.UpdateRoslyn = () => RoslynDoc; // kein Update nötig
            Editor.ApplyLightTheme();

            ImageIndex = 1;
            SelectedImageIndex = 1;
            Active = true;
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

            Editor = new DocumentEditor();
            Editor.Init();
            Editor.UpdateRoslyn = () => UpdateRoslyn();

            Editor.ZoomChanged += (s, e) =>
            {
                RosylnSignatureHelper.ListFont = GetFont();
                RoslynAutoComplete.ListFont = GetFont();
            };

        }
        public void Select()
        {
            RosylnSignatureHelper.ActiveNode = this;
            RoslynAutoComplete.ActiveNode = this;
            RosylnSignatureHelper.ListFont = GetFont();
            RoslynAutoComplete.ListFont = GetFont();

            Editor.KeyDown -= RosylnSignatureHelper.Editor_KeyDown;
            Editor.CharAdded -= RosylnSignatureHelper.Editor_CharAdded;

            Editor.KeyDown += RosylnSignatureHelper.Editor_KeyDown;
            Editor.CharAdded += RosylnSignatureHelper.Editor_CharAdded;

           // Editor.KeyDown -= RoslynAutoComplete.Editor_KeyDown;
            Editor.CharAdded -= RoslynAutoComplete.Editor_CharAdded;

          //  Editor.KeyDown += RoslynAutoComplete.Editor_KeyDown;
           
            Editor.CharAdded += RoslynAutoComplete.Editor_CharAdded;
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
                var newText = SourceText.From(Editor.Text);
                if (Editor.ReadOnly)
                {
                    // If the editor is read-only, we can't update the document
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
            if (Type == NodeType.Book) return;
            Editor.Active = Active;

           
            if (!Active)
            {
                Editor.Text = Editor.Text;
                LockNecessary();
                return;

            }
          
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
        public async Task FormatCode()
        {
            int pos = Editor.CurrentPosition;
            int firstVisibleLine = Editor.FirstVisibleLine;
            await Editor.FormatDocumentAsync();
            await UpdateRoslyn();
            Editor.FirstVisibleLine = firstVisibleLine;
            Editor.GotoPosition(pos);
        }
        public async Task<List<string>> GetUsings()
        {
            var usings = new List<string>();

            if (RoslynDoc == null) return usings;

            var text = await RoslynDoc.GetTextAsync();
            string code = Editor.Text;

            var usingMatch = Regex.Match(code, @"^(using\s.+?;\s*)+", RegexOptions.Singleline);
            if (usingMatch.Success)
            {
                var usingLines = usingMatch.Value
                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line));

                usings.AddRange(usingLines);
            }

          //  foreach (var u in usings) Debug.WriteLine(u);
            return usings;
        }

        //================ Qbook custom rules =====================

        public (int Start, int End) UsingLines;
        public async Task SyncSubcodeUsings()

        {
            if (Type != NodeType.SubCode || !Active) return;
            if (Page == null || PageNode == null || RoslynDoc == null) return;

            var usings = await PageNode.GetUsings();
            if (usings == null || usings.Count == 0) return;

            var lines = Editor.Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            // Finde den using-Block
            int start = -1, end = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("using "))
                {
                    if (start == -1) start = i;
                    end = i;
                }
                else if (start != -1 && !string.IsNullOrWhiteSpace(line))
                {
                    break;
                }
            }

            if (start == -1 || end == -1) return;

            // Berechne Positionen im Editor
            int startPos = Editor.Lines[start].Position;
            int endPos = Editor.Lines[end].EndPosition;

            // Lösche alten using-Block
            Editor.TargetStart = startPos;
            Editor.TargetEnd = endPos;
            Editor.ReplaceTarget("");

            // Füge neue using-Zeilen ein
            string newUsings = string.Join("\r\n", usings.Select(u => u.Trim())) + "\r\n";
            Editor.InsertText(startPos, newUsings);

            int startLine = Editor.LineFromPosition(startPos);
            int endLine = Editor.LineFromPosition(startPos + newUsings.Length);

            UsingLines.Start = startLine;
            UsingLines.End = endLine;

        }

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
            }
        }
        public void HideIncludeBlock()
        {
            if (Type != NodeType.Page) return;

            var editor = Editor;
            string code = editor.Text;
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            int startIndex = -1;
            int endIndex = -1;

            // Marker suchen (nullbasierter Index!)
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Contains("//<IncludeStart>")) startIndex = i;
                if (line.Contains("//<IncludeEnd>"))
                {
                    endIndex = i;
                    break;
                }
            }
            IncudeBlock.Start = startIndex;
            IncudeBlock.End = endIndex;

            Editor.HideProtectLines(IncudeBlock.Start, IncudeBlock.End);
            Editor.Refresh();
            usingsHidden = true;
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

                var sourceText = SourceText.From(Editor.Text);

                RoslynDoc = Adhoc.Workspace.AddDocument(Adhoc.Id, FileName, SourceText.From(Editor.Text));

                await UpdateRoslyn();
              //  HideIncludeBlock();
            }

            Editor.Active = Active;
            Editor.RefreshView();

            await PageNode.Editor.UpdateRoslyn();
        }
        public async Task Rename(string newName)
        {
            await UpdateRoslyn();

            string fileName = newName.EndsWith(".cs") ? newName : newName + ".cs";

            var originalDocument = RoslynDoc;
            var solution = Adhoc.Workspace.CurrentSolution;

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
            RoslynDoc = Adhoc.Workspace.AddDocument(Adhoc.Id, FileName, SourceText.From(Editor.Text));
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


    //<CodeEnd>
}}
";
        }

    }
}
