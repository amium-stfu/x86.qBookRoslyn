
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using ScintillaNET;

using System.Data;
using System.Diagnostics;
using System.Text;

using RoslynDocument = Microsoft.CodeAnalysis.Document; // Alias gegen Kollision mit ScintillaNET.Document


namespace qbookCode.Controls
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

   

            //if (Type == NodeType.Page)
            //{
            //    //this.Text = page.FullName;
            //    //this.Name = page.FullName;
            //    this.ImageIndex = 2;
            //}
            //else
            //{
            //   if(page == null)
                  

            //    this.ImageIndex = 3;
            //    this.Text = TrimCodeText(subcodeKey);
            //    if (page == null)
            //        this.Name = subcodeKey;
            //    else
            //      this.Name = $"{page.FullName}_{subcodeKey}";

            //    this.PageNode = pageNode;
            //}

            Editor = new DocumentEditor(null, page);
            Editor.Init();
        

        }
        public void Select()
        {

        }
        public CodeNode(string name) : base(name)
        {
            Type = NodeType.Book;
        }
    }
}
