
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
        public DocumentEditor Editor { get; set; }
        public string FileName { get; set; }

        public NodeType Type { get; set; }
        public enum NodeType
        {
            Page,
            SubCode,
            Book,
            Program
        }

        public CodeNode(oPage page, string fileName, NodeType type, string subcodeKey = null, CodeNode pageNode = null) : base(fileName.Replace(".cs",""))
        {
   
            Page = page;
            FileName = fileName;
            Type = type;
        
    
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
