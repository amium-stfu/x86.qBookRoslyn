using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Rename;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using qbookCode.Roslyn;

using RoslynDocument = Microsoft.CodeAnalysis.Document;

namespace qbookCode.Controls
{

    public class BookTreeView : System.Windows.Forms.TreeView
    {

        public ImageList BookTreeViewIcons;
        private ContextMenuStrip Menu = new ContextMenuStrip();
        private ToolStripMenuItem addPageBeforeToolStripMenuItem = new ToolStripMenuItem();
        private ToolStripMenuItem addPageAfterToolStripMenuItem = new ToolStripMenuItem();
        private ToolStripMenuItem addSubCodeToolStripMenuItem = new ToolStripMenuItem();
        private ToolStripMenuItem customToolStripMenuItem = new ToolStripMenuItem();
        private ToolStripMenuItem uDLClientToolStripMenuItem = new ToolStripMenuItem();
        private ToolStripMenuItem aKClientToolStripMenuItem = new ToolStripMenuItem();
        private ToolStripMenuItem aKServerToolStripMenuItem = new ToolStripMenuItem();
        private ToolStripMenuItem streamClientToolStripMenuItem = new ToolStripMenuItem();
        private ToolStripMenuItem hidePageToolStripMenuItem = new ToolStripMenuItem();
        private ToolStripMenuItem deleteStripMenuItem = new ToolStripMenuItem();
        private ToolStripMenuItem toolStripMenuOpenWorkspace = new ToolStripMenuItem();
        private ToolStripMenuItem renameCodeToolStripMenuItem = new ToolStripMenuItem();
        private ToolStripMenuItem renamePageToolStripMenuItem = new ToolStripMenuItem();
        private ToolStripMenuItem toolStripMenuIncludeCode = new ToolStripMenuItem();
        private ToolStripMenuItem deletePageToolStripMenuItem = new ToolStripMenuItem();
       

 
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



        FormCodeExplorer View;
        Dictionary<string, PageEditor> PageEditors = new Dictionary<string, PageEditor>();

        BookNode? _SelectedCodeNode = null;
        public BookNode? SelectedCodeNode
        {
            get => _SelectedCodeNode;
            set
            {
                _SelectedCodeNode = value;
                ApplyTheme();
            }
        }
        public BookNode ClickedNode { get; set; } = null;

        public BookTreeView(ImageList imageList, FormCodeExplorer view = null)
        {
            this.HideSelection = true;
            View = view;
            BookTreeViewIcons = imageList;
            AllowDrop = true;


            DragDrop += ProjectTree_DragDrop;
            DragEnter += ProjectTree_DragEnter;
            DragOver += ProjectTree_DragOver;
            DragLeave += ProjectTree_DragLeave;
            ItemDrag += ProjectTree_ItemDrag;

            ApplyTheme();
            initMenu();
            NodeMouseClick += ProjectTree_NodeMouseClick;
        }

        protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
        {
            e.Cancel = true; // verhindert, dass TreeView den Node markiert
            base.OnBeforeSelect(e);
        }

        public DocumentEditor GetSelectedEditor() => SelectedCodeNode?.Editor;

        #region Drag & Drop
        public int InsertLineY { get; set; } = -1;
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
            var item = e.Item;
            if (item is null)
                return; // oder ignore

            DoDragDrop(item, DragDropEffects.Move);
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
            var item = e.Data;

            if (item == null) return;

            if (item.GetDataPresent(typeof(BookNode)))
            {
                Point pt = PointToClient(new Point(e.X, e.Y));
                System.Windows.Forms.TreeNode targetNode = GetNodeAt(pt);
                BookNode? draggedNode = (BookNode?)item.GetData(typeof(BookNode));

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

                List<string> pageOder = new List<string>();
                foreach (BookNode page in Nodes[0].Nodes)
                    if (page.Type == NodeType.Page) pageOder.Add(page.Text);

                Core.SendToQbook("PageOrder", pageOder.ToArray() );
            }
        }
        private bool IsChildNode(System.Windows.Forms.TreeNode? parent, System.Windows.Forms.TreeNode? child)
        {
            if (parent == null || child == null)
                return false;

            while (child.Parent != null)
            {
                if (child.Parent == parent)
                    return true;
                child = child.Parent;
            }
            return false;
        }

        #endregion

        #region Theme

    
        public void ApplyTheme()
        {
            BeginUpdate();
            BackColor = Theme.PanelBackColor;
            ForeColor = Theme.GridForeColor;
            BorderStyle = System.Windows.Forms.BorderStyle.None;
            UpdateAllNodes();
            EndUpdate();
        }


        bool CodeError { get; set; } = false;
        public bool CheckCode()
        {
            UpdateAllNodes();
            return CodeError;
        }


        private void UpdateAllNodes()
        {
            CodeError = false;
            foreach (BookNode node in Nodes)
            {
             
                UpdateNodeByLevelRecursive(node);
            }
        }
        private void UpdateNodeByLevelRecursive(BookNode node)
        {
            node.ForeColor = Theme.TreeNodeDefaultColor;
            if (node == SelectedCodeNode)
                node.ForeColor = Theme.TreeNodeSelectColor;

            int index = 1;
            if (node.Type == NodeType.Book) index = 1;
            if (node.Type == NodeType.Page) index = 2;
            if (node.Type == NodeType.SubCode) index = 3;
            if (node.Type == NodeType.Program) index = 3;
            if (node.Editor != null) 
            {
                if (node.Editor.Page != null)
                    if (node.Editor.Page.Hidden) index = 5;

                if(!node.Editor.Active) index = 4;
                if (node.Editor.HasErrors && node.Editor.Active)
                {
                    index = 10;
                    CodeError = true;

                    if (node.Type == NodeType.SubCode)
                    {
                        node.Parent.Expand();
                    }
                }
            }

            node.ImageIndex = index;
            foreach (BookNode child in node.Nodes)
            {
                UpdateNodeByLevelRecursive(child);
            }
        }

        #endregion

        // In BookTreeView.cs
        public void CleanupEditorsAndNodes()
        {
            BeginUpdate();
            try
            {
                // 1) Alle bekannten Page-Editoren entsorgen
                foreach (var page in PageEditors.Values)
                {
                    page.PageRoot?.Dispose();

                    if (page.SubCodes != null)
                    {
                        foreach (var ed in page.SubCodes)
                        {
                            ed?.Dispose();
                        }
                    }
                }
                PageEditors.Clear();

                // 2) Alle Editor-Instanzen, die direkt an Nodes hängen (z.B. Program, GlobalUsing)
                foreach (BookNode node in EnumerateNodes(Nodes))
                {
                    if (node.Editor != null)
                    {
                        node.Editor.Dispose();
                        node.Editor = null;
                    }
                }

                // 3) TreeView-Zustand zurücksetzen
                SelectedCodeNode = null;
                ClickedNode = null;
                Nodes.Clear();
            }
            finally
            {
                EndUpdate();
            }
        }

        private IEnumerable<BookNode> EnumerateNodes(TreeNodeCollection nodes)
        {
            foreach (BookNode node in nodes)
            {
                yield return node;

                foreach (BookNode child in EnumerateNodes(node.Nodes))
                    yield return child;
            }
        }


        #region CleanUp




        #endregion

        #region Create Update Delete Nodes
        public void Create()
        {
            using (var splash = new FormExplorerSplashScreen())
            {
                splash.Show();
                Application.DoEvents();

                PageEditors.Clear();
                splash.SetStatus("Creating BookTree");

                BeginUpdate();
                ImageList = BookTreeViewIcons;
                Nodes.Clear();
                BookNode Root = new BookNode(Core.ThisBook.Filename.Replace(".qbook", ""), NodeType.Book);

                Root.ImageIndex = 1;
                Font = new System.Drawing.Font("Calibri", 12);
                Nodes.Add(Root);
                SelectedCodeNode = null;
                foreach (oPage page in Core.ThisBook.Pages.Values)
                {
                    splash.SetStatus("Adding Page " + page.Name);
                    BookNode pageNode = new BookNode(page.RoslynCodeDoc.Filename, NodeType.Page) { ImageIndex = 2 };
                    pageNode.Editor = new DocumentEditor(page.RoslynCodeDoc, page);
                    pageNode.Editor.GoToDefinition = async () => await GoToDefinitionAsync();


                    if (SelectedCodeNode == null)
                        SelectedCodeNode = pageNode;

                    List<DocumentEditor> subEditors = new List<DocumentEditor>();
                    foreach (CodeDocument doc in page.SubCodeDocuments.Values)
                    {
                        BookNode subNode = new BookNode(doc.Filename, NodeType.SubCode) { ImageIndex = 3 };
                        subNode.Text = doc.Filename.Split('.')[1];
                        splash.SetStatus("Adding SubCode " + subNode.Text);
                        subNode.Editor = new DocumentEditor(doc, page);

                        subEditors.Add(subNode.Editor);
                        subNode.Editor.Page = page;
                        pageNode.Nodes.Add(subNode);
                    }

                    Root.Nodes.Add(pageNode);
                    PageEditors[page.Name] = new PageEditor(pageNode.Editor, subEditors);
                }

                BookNode Program = new BookNode("Program.cs", NodeType.Program) { ImageIndex = 3 };
                Program.Editor = new DocumentEditor(Core.Roslyn.GetCodeDocument("Program.cs"), null);
                BookNode Global = new BookNode("GlobalUsing.cs", NodeType.Program) { ImageIndex = 3 };
                Global.Editor = new DocumentEditor(Core.Roslyn.GetCodeDocument("GlobalUsing.cs"), null);
                Root.Nodes.Add(Program);
                Root.Nodes.Add(Global);

                Root.Expand();
                EndUpdate();
               
                splash.SetStatus("Verifying Code");
            }
          
        }

        public async Task CheckFullCode()
        {
            using (var splash = new FormExplorerSplashScreen())
            {
                splash.Show();
                Application.DoEvents();

                splash.SetStatus("Checking Code");

                await UpdateAllAfterInExclude();

                splash.SetStatus("Code Check Complete");
            }
        }

        #endregion


        #region Events

        
        BookNode LastSelectedNode = null;
        private async void ProjectTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
          
            if (e.Button == MouseButtons.Left)
            {
                if (e.Node is BookNode)
                {
         
                    if(LastSelectedNode != null)
                    {
                        LastSelectedNode.ForeColor = Theme.GridForeColor;
                    }

                    SelectedCodeNode = e.Node as BookNode;
                    SelectedCodeNode.ForeColor = Theme.IsDark ? Color.Plum : Color.DarkOrchid;
                    if (View != null)
                    {
                        await SelectedCodeNode.Editor.UpdateRoslyn("TreeView");
                        View.SetTarget("Mouse click", SelectedCodeNode.Editor);
                    }

                    LastSelectedNode = SelectedCodeNode;
                }
               
            }


            if (e.Button == MouseButtons.Right)
            {
                ClickedNode = e.Node as BookNode;
                SelectedCodeNode = e.Node as BookNode;

                if (ClickedNode.Editor.Active)
                {
                    toolStripMenuIncludeCode.Text = "Exclude Code";
                }
                else
                {
                    toolStripMenuIncludeCode.Text = "Include Code";
                }

                if(ClickedNode.Editor != null && ClickedNode.Editor.Page != null)
                {
                    oPage page = ClickedNode.Editor.Page;
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
                    renameCodeToolStripMenuItem.Visible = true;
                    deleteStripMenuItem.Visible = true;

             if(((BookNode)e.Node).Type == NodeType.Program)
                    {
                        addPageBeforeToolStripMenuItem.Visible = false;
                        addPageAfterToolStripMenuItem.Visible = false;
                        hidePageToolStripMenuItem.Visible = false;
                        addSubCodeToolStripMenuItem.Visible = false;
                        deleteStripMenuItem.Visible = false;
                        toolStripMenuOpenWorkspace.Visible = false;
                        renamePageToolStripMenuItem.Visible = false;
                        renameCodeToolStripMenuItem.Visible = false;
                        toolStripMenuIncludeCode.Visible = false;

                    }

                }
                Menu.Show(this, e.Location);
            }
      

        }


        #endregion


        #region Menu Items

        void initMenu()
        {
   
            Menu.Name = "contextMenuTreeView";
            Menu.Size = new System.Drawing.Size(165, 224);
            // 
            // addPageBeforeToolStripMenuItem
            // 
            addPageBeforeToolStripMenuItem.Name = "addPageBeforeToolStripMenuItem";
            addPageBeforeToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            addPageBeforeToolStripMenuItem.Text = "Add Page before";
            addPageBeforeToolStripMenuItem.Click += addPageBeforeToolStripMenuItem_Click;

            // 
            // addPageAfterToolStripMenuItem
            // 
            addPageAfterToolStripMenuItem.Name = "addPageAfterToolStripMenuItem";
            addPageAfterToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            addPageAfterToolStripMenuItem.Text = "Add Page after";
            addPageAfterToolStripMenuItem.Click += addPageAfterToolStripMenuItem_Click;
            // 
            // addSubCodeToolStripMenuItem
            // 
            addSubCodeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            customToolStripMenuItem,
            uDLClientToolStripMenuItem,
            aKClientToolStripMenuItem,
            aKServerToolStripMenuItem,
            streamClientToolStripMenuItem});
            addSubCodeToolStripMenuItem.Name = "addSubCodeToolStripMenuItem";
            addSubCodeToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            addSubCodeToolStripMenuItem.Text = "Add SubCode";
            // 
            // customToolStripMenuItem
            // 
            customToolStripMenuItem.Name = "customToolStripMenuItem";
            customToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            customToolStripMenuItem.Text = "Custom";
            customToolStripMenuItem.Click += customToolStripMenuItem_Click;
        
            // 
            // uDLClientToolStripMenuItem
            // 
            uDLClientToolStripMenuItem.Name = "uDLClientToolStripMenuItem";
            uDLClientToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            uDLClientToolStripMenuItem.Text = "UDL Client";
            // 
            // aKClientToolStripMenuItem
            // 
            aKClientToolStripMenuItem.Name = "aKClientToolStripMenuItem";
            aKClientToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            aKClientToolStripMenuItem.Text = "AK Client";
            // 
            // aKServerToolStripMenuItem
            // 
            aKServerToolStripMenuItem.Name = "aKServerToolStripMenuItem";
            aKServerToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            aKServerToolStripMenuItem.Text = "AK Server";
            // 
            // streamClientToolStripMenuItem
            // 
            streamClientToolStripMenuItem.Name = "streamClientToolStripMenuItem";
            streamClientToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            streamClientToolStripMenuItem.Text = "Stream Client";
            // 
            // hidePageToolStripMenuItem
            // 
            hidePageToolStripMenuItem.Name = "hidePageToolStripMenuItem";
            hidePageToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            hidePageToolStripMenuItem.Text = "Hide Page";
            hidePageToolStripMenuItem.Click += (s, e) =>
            {
                if (ClickedNode != null && ClickedNode.Editor != null && ClickedNode.Editor.Page != null)
                {
                    oPage page = ClickedNode.Editor.Page;
                    page.Hidden = !page.Hidden;
                    ApplyTheme();
                    Core.SendToQbook("HidePage", page.Name, page.Hidden.ToString());
                }
            };
            // 
            // deleteStripMenuItem
            // 
            deleteStripMenuItem.Name = "deleteStripMenuItem";
            deleteStripMenuItem.Size = new System.Drawing.Size(164, 22);
            deleteStripMenuItem.Text = "Delete Code";
            deleteStripMenuItem.Click += deletePageToolStripMenuItem_Click;
            // 
            // toolStripMenuOpenWorkspace
            // 
            toolStripMenuOpenWorkspace.Name = "toolStripMenuOpenWorkspace";
            toolStripMenuOpenWorkspace.Size = new System.Drawing.Size(164, 22);
            toolStripMenuOpenWorkspace.Text = "Open Workspace";
            // 
            // renameCodeToolStripMenuItem
            // 
            renameCodeToolStripMenuItem.Name = "renameCodeToolStripMenuItem";
            renameCodeToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            renameCodeToolStripMenuItem.Text = "Rename Code";
            renameCodeToolStripMenuItem.Click += renameCodeToolStripMenuItem_Click;
            // 
            // renamePageToolStripMenuItem
            // 
            renamePageToolStripMenuItem.Name = "renamePageToolStripMenuItem";
            renamePageToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            renamePageToolStripMenuItem.Text = "Rename Page";
            renamePageToolStripMenuItem.Click += renamePageToolStripMenuItem_Click;
            // 
            // toolStripMenuIncludeCode
            // 
            toolStripMenuIncludeCode.Name = "toolStripMenuIncludeCode";
            toolStripMenuIncludeCode.Size = new System.Drawing.Size(164, 22);
            toolStripMenuIncludeCode.Text = "Include Code";
            toolStripMenuIncludeCode.Click += toolStripMenuIncludeCode_Click;
            // 
            // deletePageToolStripMenuItem
            // 
            deletePageToolStripMenuItem.Name = "deletePageToolStripMenuItem";
            deletePageToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            deletePageToolStripMenuItem.Text = "Delete Page";
            deletePageToolStripMenuItem.Click += deletePageToolStripMenuItem_Click;


            Menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] 
            {
            addPageBeforeToolStripMenuItem,
            addPageAfterToolStripMenuItem,
            addSubCodeToolStripMenuItem,
            hidePageToolStripMenuItem,
            deleteStripMenuItem,
            toolStripMenuOpenWorkspace,
            renameCodeToolStripMenuItem,
            renamePageToolStripMenuItem,
            toolStripMenuIncludeCode,
            deletePageToolStripMenuItem
            });
        }

        private async void toolStripMenuIncludeCode_Click(object sender, EventArgs e)
        {
            if (ClickedNode != null && ClickedNode.Editor != null)
            {
                if (ClickedNode.Editor.Target.Active)
                {
                    ClickedNode.Editor.Target.Active = false;
                    ClickedNode.Editor.Target.Exclude();
                    await ClickedNode.Editor.UpdateRoslyn("Toggle Exclude Code");
                    ClickedNode.Editor.ApplyTheme();
                }
                else
                {
                    ClickedNode.Editor.Target.Active = true;
                    await ClickedNode.Editor.Target.Include();
                    await ClickedNode.Editor.UpdateRoslyn("Toggle Include Code");
                    ClickedNode.Editor.ApplyTheme();
                }

               await UpdateAllAfterInExclude();

            }
        }

        private async Task UpdateAllAfterInExclude()
        {
            try
            {
                foreach (BookNode Page in Nodes[0].Nodes)
                {
                    await Page.Editor.UpdateRoslyn("Update afer In/Exclude");

                    foreach (BookNode code in Page.Nodes)
                        await code.Editor.UpdateRoslyn("Update afer In/Exclude");
                }
                ApplyTheme();
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error updating after In/Exclude: " + ex.Message);
                ApplyTheme();
            }
        }

        private async void renameCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
           BeginUpdate();
            string name = ShowInputDialog($"Input new name:", $"Rename Code {ClickedNode.Text}", ClickedNode.Text);
            if (!string.IsNullOrWhiteSpace(name))
            {
                View.Editor.RemoveTab(ClickedNode.Editor.Target.Filename);

                await ClickedNode.Editor.UpdateRoslyn("Rename Code");
                string newFilename = ClickedNode.Name.Replace(ClickedNode.Text, name);
                BookNode newNode = new BookNode(newFilename, NodeType.SubCode) { ImageIndex = 3 };
                CodeDocument doc = Core.Roslyn.AddCodeDocument(newFilename, ClickedNode.Editor.Target.Code, true);
                newNode.Editor = new DocumentEditor(doc, ClickedNode.Editor.Page);
                await newNode.Editor.UpdateRoslyn("Rename Code");

                System.Windows.Forms.TreeNodeCollection tree = ClickedNode.Parent.Nodes;
                int origin = Core.ThisBook.PageOrder.IndexOf(ClickedNode.Text);

                PageEditors[ClickedNode.Editor.Page.Name].SubCodes.Remove(ClickedNode.Editor);
                PageEditors[ClickedNode.Editor.Page.Name].SubCodes.Add(newNode.Editor);

                Core.Roslyn.RemoveCodeDocument(ClickedNode.Editor.Target.Filename);
                tree.Remove(ClickedNode);
               


                tree.Insert(origin, newNode);
            }

            EndUpdate();

        }

        private void renamePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string name = ShowInputDialog($"Input new name:", $"Rename Page {ClickedNode.Editor.Page.Name}", ClickedNode.Editor.Page.Name);
                if (!string.IsNullOrWhiteSpace(name))
                {

                    System.Windows.Forms.TreeNodeCollection tree = ClickedNode.Parent.Nodes;
                    int originPageIndex = Core.ThisBook.PageOrder.IndexOf(ClickedNode.Text);
                    oPage originPage = ClickedNode.Editor.Page;

                    View.Editor.RemoveTab(originPage.RoslynCodeDoc.Filename);

                    oPage newPage = new oPage(name, name);
                    newPage.OrderIndex = originPage.OrderIndex;
                    newPage.Hidden = originPage.Hidden;
                    newPage.Format = originPage.Format;
                    newPage.Includes = new List<string>(originPage.Includes);
                    newPage.CodeOrder = new List<string>(originPage.CodeOrder);
                    newPage.Section = originPage.Section;
                    newPage.Url = originPage.Url;

                    newPage.RoslynCodeDoc = Core.Roslyn.AddCodeDocument
                        (
                        filename: originPage.RoslynCodeDoc.Filename.Replace(originPage.Name, name),
                        code: originPage.RoslynCodeDoc.Code.Replace($"Definition{originPage.Name}", $"Definition{name}"),
                        active: true
                        );

                    Core.Roslyn.RemoveCodeDocument(originPage.RoslynCodeDoc.Filename);

                    foreach (var item in originPage.SubCodeDocuments)
                    {
                        CodeDocument old = item.Value as CodeDocument;
                        string key = item.Key;

                        View.Editor.RemoveTab(old.Filename);
                        newPage.SubCodeDocuments[key] = Core.Roslyn.AddCodeDocument
                        (
                        filename: old.Filename.Replace(originPage.Name, name),
                        code: old.Code.Replace($"Definition{originPage.Name}", $"Definition{name}"),
                        active: old.Active
                        );
                        Core.Roslyn.RemoveCodeDocument(old.Filename);
                    }

           
                    Core.ThisBook.PageOrder[originPageIndex] = newPage.Name;

                    ClickedNode.Name = ClickedNode.Name.Replace(originPage.Name, name);
                    ClickedNode.Editor.Target = Core.Roslyn.GetCodeDocument(newPage.RoslynCodeDoc.Filename);
                    ClickedNode.Editor.Text = ClickedNode.Editor.Target.Code;
                    ClickedNode.Editor.EmptyUndoBuffer();

                    foreach (BookNode sub in ClickedNode.Nodes)
                    {
                        string file = originPage.RoslynCodeDoc.Filename.Replace(originPage.Name, name);
                        sub.Editor.Target = Core.Roslyn.GetCodeDocument(file);
                        sub.Editor.Text = ClickedNode.Editor.Target.Code;
                        sub.Editor.EmptyUndoBuffer();
                    }
                }
                Create();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error renaming page: " + ex.Message);
                Create();

            }
        }
   


        private void deletePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Core.Roslyn.RemoveCodeDocument(ClickedNode.Editor.Target.Filename);
          //  Core.ThisBook.Main.Objects.Remove(ClickedNode.Editor.Page);
            Core.ThisBook.PageOrder.Remove(ClickedNode.Text);
            Nodes.Remove(ClickedNode);
        }



        private async void customToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BeginUpdate();
            string name = ShowInputDialog("Input code name:", $"New subcode", "CustomCode");
            if (!string.IsNullOrWhiteSpace(name))
            {
                string PageName = ClickedNode.Text;
               // oPage page = qbook.Core.ThisBook.Main.Objects.OfType<oPage>().FirstOrDefault(p => p.Name == PageName);

                //string filename = page.Name + "." + name + ".cs";
                //BookNode subNode = new BookNode(filename, NodeType.SubCode) { ImageIndex = 3 };
                //page.SubCodeDocuments[filename] = Core.Roslyn.AddCodeDocument(filename, Snippets.NewSubCode(page, name), true);
                //subNode.Editor = new DocumentEditor(page.SubCodeDocuments[filename], page);
                //await subNode.Editor.UpdateRoslyn("New CustomCode");
                //ClickedNode.Nodes.Add(subNode);

            }
            EndUpdate();
        }


        private async void addPageBeforeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await InsertPage(offset: -1);
        }
        private async Task InsertPage(int offset = 1)
        {
            BeginUpdate();
            oPage page = null;
            BookNode pageNode = null;
            string name = ShowInputDialog("Input page name:", $"New subcode", "NewPage");
            if (!string.IsNullOrWhiteSpace(name))
            {
                page = new oPage(name, name);

                string filename = name + ".qPage.cs";
                page.RoslynCodeDoc = Core.Roslyn.AddCodeDocument(filename, Snippets.NewPageCode(name), true);
                page.RoslynCodeDoc.UpdateCode();
                
                pageNode = new BookNode(page.RoslynCodeDoc.Filename, NodeType.Page) { ImageIndex = 2 };
                pageNode.Editor = new DocumentEditor(page.RoslynCodeDoc, page);
                await pageNode.Editor.UpdateRoslyn("InsertPage");

            }
            if (page == null)
            {
                EndUpdate();
                return;
            }
            System.Windows.Forms.TreeNodeCollection tree = Nodes[0].Nodes;
            int index = -1;
            if (offset == 0)
            {
                Nodes[0].Nodes.Add(pageNode);
            }
            else
            {
                if (offset == -1)
                {
                    offset = 0;
                }
                index = tree.IndexOf(ClickedNode);
                tree.Insert(index + offset, pageNode);

                int origin = Core.ThisBook.PageOrder.IndexOf(ClickedNode.Text);
                if (origin + offset > Core.ThisBook.PageOrder.Count)
                {
                    Core.ThisBook.PageOrder.Add(pageNode.Text);
                }
                else
                {
                    Core.ThisBook.PageOrder.Insert(origin + offset, pageNode.Text);
                }
             //   qbook.Core.ThisBook.Main.Objects.Add(page);
            }
            EndUpdate();

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



        #endregion


        #region Select Edit Node

        public async Task RenameSymbolAsync()
        {
            if (SelectedCodeNode?.Editor == null)
            {
                MessageBox.Show(this, "Project not loaded or no document selected.", "Rename", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int caretBefore = SelectedCodeNode.Editor.CurrentPosition;
            int selStartBefore = SelectedCodeNode.Editor.SelectionStart;
            int selEndBefore = SelectedCodeNode.Editor.SelectionEnd;
            int firstLine = SelectedCodeNode.Editor.FirstVisibleLine;

            var doc = Core.Roslyn.GetDocumentByFilename(SelectedCodeNode.Editor.Target.Filename);
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
                    var node = GetNodeByTargetFilename(oldDoc.Name);
                    if (node != null)
                    {
                        node.Editor.Text = newTextStr;
                        node.Editor.UpdateDocument(); 
                        updatedCount++;
                    }
                }
            }

            if (updatedCount == 0)
            {
                MessageBox.Show(this, $"Rename failed (no changes).  {input}", "Rename", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

          //  OpenNode(SelectedNode);
            // Cursor und Auswahl wiederherstellen
            int newLen = SelectedCodeNode.Editor.TextLength;
            SelectedCodeNode.Editor.SetSelection(Math.Min(selEndBefore, newLen), Math.Min(selStartBefore, newLen));
            SelectedCodeNode.Editor.GotoPosition(Math.Min(caretBefore, newLen));
            SelectedCodeNode.Editor.Lines[firstLine].Goto();

            await SelectedCodeNode.Editor.UpdateRoslyn("Rename Symbol");
            // await RefreshDiagnosticsAsync();
            SelectedCodeNode.Editor.FirstVisibleLine = firstLine;
            SelectedCodeNode.Editor.GotoPosition(caretBefore);
        }
        public async Task GoToDefinitionAsync()
        {
            if (SelectedCodeNode == null) return;

            int caret = SelectedCodeNode.Editor.CurrentPosition;
            var loc = await Core.Roslyn.GoToDefinitionAsync(Core.Roslyn.GetDocumentByFilename(SelectedCodeNode.Editor.Target.Filename), caret);
            if (loc == null) return;

            var (doc, line, column) = loc.Value;

            string documentName = doc.Name;

            BookNode node = GetNodeByFilename(documentName);
            if (node == null) return;

            await OpenNodeByName(node.Name);

            int pos = GetPositionFromLineColumn(node.Editor, line + 1, column);
            node.Editor.GotoPosition(pos);
            node.Editor.HighlightLine(line + 1, Color.Yellow);
            node.Editor.ScrollCaret();
        }
        private int GetPositionFromLineColumn(Scintilla ed, int line, int column) => ed.Lines[line].Position + column;
        private BookNode GetNodeByTargetFilename(string filename)
        {
            foreach (BookNode node in Nodes[0].Nodes)
            {
                if (node.Editor.Target.Filename == filename)
                {
                    return node;
                }

                foreach (BookNode sub in node.Nodes) // <-- Hier der Fix
                {
                    Debug.WriteLine($"R '{sub.Editor.Target.Filename}'");
                    Debug.WriteLine($"S '{filename}'");
                    if (sub.Editor.Target.Filename == filename)
                    {
                        return sub;
                    }
                }
            }
            return null;
        }

        public BookNode GetNodeByFilename(string name)
        {
            Debug.WriteLine($"========");
            foreach (BookNode node in Nodes[0].Nodes)
            {
                if (node.Type == NodeType.Page)
                {
                    BookNode found = GetNodeByFilenameRecursive(node, name);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }
        private BookNode GetNodeByFilenameRecursive(BookNode node, string name)
        {
                //Debug.WriteLine($"R {node.Editor.Target.Filename}");
                //Debug.WriteLine($"S {name}");

            if (node.Editor.Target.Filename == name)
                    return node;

                foreach (BookNode child in node.Nodes)
                {
                    BookNode found = GetNodeByFilenameRecursive(child, name);
                    if (found != null)
                        return found;
                }
            

            return null;
        }



        public async Task OpenNodeByName(string name)
        {

            foreach (BookNode node in Nodes)
            {
                BookNode found = FindNodeByNameRecursive(node, name);
                if (found != null)
                {
                    SelectedCodeNode = found;
                    

                    await SelectedCodeNode.Editor.UpdateRoslyn("TreeView OpenNodeByName");
                    if (View != null)
                    {
                        View.SetTarget("Open Node", SelectedCodeNode.Editor);
                        
                    }
                    SelectedCodeNode.Editor.ApplyTheme();
                    SelectedCodeNode.EnsureVisible();
                    break;

                    
                }
            }
         
        }
        private BookNode FindNodeByNameRecursive(BookNode node, string name)
        {
            Debug.WriteLine("." + node.Name);
            if (node.Name == name)
            {
                return node;
            }
            foreach (BookNode child in node.Nodes)
            {
                BookNode found = FindNodeByNameRecursive(child, name);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        #endregion

       


    }

    public enum NodeType
    {
        Page,
        SubCode,
        Book,
        Program,
        Settings,
        Folder
    }


    public class BookNode : System.Windows.Forms.TreeNode
    {
        public NodeType Type { get; set; }

        public DocumentEditor Editor;
        public BookNode(string name, NodeType type) : base(name)
        {
            Type = type;
            Name = name;

            if (Type == NodeType.SubCode)
            {
                Text = name.Split('.')[1];
            }

            if (Type == NodeType.Page)
            {
                Text = name.Split('.')[0];
            }



        }
    }
}
