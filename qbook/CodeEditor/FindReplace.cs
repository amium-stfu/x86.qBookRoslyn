using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using ScintillaNET;
using Microsoft.CodeAnalysis.Text;
using RoslynDocument = Microsoft.CodeAnalysis.Document;
using System.Diagnostics;

namespace qbook.CodeEditor
{
    class FindObject
    {
        public object Source { get; }
        public int Carret { get; }
        public int Length { get; }

        public bool InEditor => Source is Scintilla;

        public FindObject(object source, int carret, int length)
        {
            Source = source;
            Carret = carret;
            Length = length;
        }
    }

    internal class FindReplace
    {
        List<FindObject> findObjects = new List<FindObject>();
        System.Windows.Forms.TreeView ProjectTree;
        string findText;
        int currentIndex = -1;
        Scintilla Editor;

        public FindReplace(System.Windows.Forms.TreeView projectTree, Scintilla editor)
        {
            ProjectTree = projectTree;
            Editor = editor;
        }

        public bool HasFindObjects => findObjects.Count > 0;

        public async Task FindAsyncInProject(string text)
        {
            if (findText != text)
            {
                findObjects.Clear();
                currentIndex = -1;
            }
            else
            {
                Next();
                return;
            }

            findText = text;
            findObjects.Clear();

            foreach (EditorNode node in ProjectTree.Nodes[0].Nodes)
            {
                var roslynText = await node.RoslynDoc.GetTextAsync();
                string textContent = roslynText.ToString();

                int index = 0;
                while ((index = textContent.IndexOf(text, index, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    findObjects.Add(new FindObject(node, index, text.Length));
                    Debug.WriteLine(node.Name);
                    index += text.Length;
                }

                foreach (EditorNode childnode in node.Nodes)
                {
                    roslynText = await childnode.RoslynDoc.GetTextAsync();
                    textContent = roslynText.ToString();

                    index = 0;
                    while ((index = textContent.IndexOf(text, index, StringComparison.OrdinalIgnoreCase)) != -1)
                    {
                        findObjects.Add(new FindObject(childnode, index, text.Length));
                        Debug.WriteLine(childnode.Name);
                        index += text.Length;
                    }
                }

            }
       
        }

        public async Task FindInEditor(string text)
        {
            if (findText != text)
            {
                findObjects.Clear();
                currentIndex = -1;
            }
            findText = text;

            if (Editor != null)
            {
                string textContent = Editor.Text;

                int index = 0;
                while ((index = textContent.IndexOf(text, index, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    findObjects.Add(new FindObject(Editor, index, text.Length));
                    index += text.Length;
                }
            }
        }

        public FindObject Next()
        {
            if (currentIndex + 1 < findObjects.Count)
                currentIndex++;
            else
                currentIndex = 0;


            return findObjects[currentIndex];
          
        }
    }
}