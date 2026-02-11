
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qbookCode.Roslyn;

using System.Windows.Forms;

namespace qbookCode.Controls
{
    public partial class FormBookTree : Form
    {
        public BookTreeView bookTreeView;
        
        public FormBookTree(FormCodeExplorer view = null)
        {
            InitializeComponent();
            bookTreeView = new BookTreeView(BookTreeViewIcons, view);
            bookTreeView.Dock = DockStyle.Fill;
            panelTreeView.Controls.Add(bookTreeView);
            VBar.Init(bookTreeView, true);
        }

        public void CreateTree()
        {
            bookTreeView.Create();

        }

       
        public void CleanupBeforeLoad()
        {
            if (bookTreeView == null)
                return;

            // Alle Editor-Instanzen und Nodes sauber aufräumen
            bookTreeView.CleanupEditorsAndNodes();

            // Scrollbar wieder auf definierten Anfangszustand setzen
            VBar.Init(bookTreeView, true);
        }



        public async Task<bool> CheckCode()
        {
            return await bookTreeView.CheckCode();
        }
        public DocumentEditor GetSelectedEditor()
        {
            return bookTreeView.GetSelectedEditor()!;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            bookTreeView.Create();

           
        }
        public BookNode? SelectedNode => bookTreeView.SelectedCodeNode;
        public async Task OpenNodeByName(string name)
        {
            await bookTreeView.OpenNodeByName(name);
        }
        public void ApplyTheme()
        {
            panelTreeView.BackColor = Theme.FormBackColor;
            BackColor = Theme.FormBackColor;
            VBar.SetBackColor = Theme.PanelBackColor;
            VBar.SetForeColor = Theme.ThumbColor;
            bookTreeView.ApplyTheme();
        }
        private void addPageBeforeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InsertPage(offset: -1);
        }
        private void InsertPage(int offset = 1)
        {
            bookTreeView.BeginUpdate();
            oPage page = null;
            BookNode pageNode = null;
            string name = ShowInputDialog("Input page name:", $"New subcode", "NewPage");
            if (!string.IsNullOrWhiteSpace(name))
            {
                page = new oPage(name,name);
                page.RoslynCodeDoc = new CodeDocument(name + ".qPage.cs", Snippets.NewPageCode(name),true,Core.Roslyn);

                pageNode = new BookNode(page.RoslynCodeDoc.Filename, NodeType.Page) { ImageIndex = 2 };


            }
            if (page == null)
            {
                bookTreeView.EndUpdate();
                return;
            }
            System.Windows.Forms.TreeNodeCollection tree = bookTreeView.Nodes[0].Nodes;
            int index = -1;
            if (offset == 0)
            {
                bookTreeView.Nodes[0].Nodes.Add(pageNode);
            }
            else
            {
                if (offset == -1)
                {
                    offset = 0;
                }
                index = tree.IndexOf(bookTreeView.ClickedNode);
                tree.Insert(index + offset, pageNode);

                int origin = Core.ThisBook.PageOrder.IndexOf(bookTreeView.ClickedNode.Text);
                Core.ThisBook.PageOrder.Insert(origin + offset, pageNode.Text);
               
            }
            bookTreeView.EndUpdate();
          
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

        private void addPageAfterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InsertPage(offset: +1);
        }

        public async Task GoToDefinition() => await bookTreeView.GoToDefinitionAsync();
        public async Task RenameSymbol() => await bookTreeView.RenameSymbolAsync();

        public BookNode SelectedCodeNode => bookTreeView.SelectedCodeNode;

        public BookNode GetNodeByFilename(string filename) => bookTreeView.GetNodeByFilename(filename);

        public System.Windows.Forms.TreeView BookTree => bookTreeView;


    }
}
