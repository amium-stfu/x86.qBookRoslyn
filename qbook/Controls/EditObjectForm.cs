using ActiproSoftware.Text.Languages.DotNet.Reflection;
using ActiproSoftware.UI.WinForms.Controls.SyntaxEditor;
using ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.IntelliPrompt;
using ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.IntelliPrompt.Implementation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
//using static Main.ScriptingClass;

namespace qbook
{
    public partial class EditObjectForm : Form
    {
        public static void SetAllOpenEditorsToUnModified()
        {
        }

        private int documentNumber;
        private bool hasPendingParseData;
        private System.Threading.Timer refreshReferencesTimer;

        //// A project assembly (similar to a Visual Studio project) contains source files and assembly references for reflection
        ///*HALE*/static private IProjectAssembly projectAssembly;
        //static CSharpSyntaxLanguage language = null;// new CSharpSyntaxLanguage();

        public static EditObjectForm Edit(int x, int y, oItem item)
        {
            if (item == null)
                return null;

            return null;
        }

        public static void EnableTab(TabPage page, bool enable)
        {
            foreach (System.Windows.Forms.Control ctl in page.Controls) ctl.Enabled = enable;
        }

        oItem Item;
        public EditObjectForm(oItem item)
        {
            InitializeComponent();

            Item = item;

            //if (Item.MySyntaxEditor != null)
            //{
            //    codeEditor = Item.MySyntaxEditor;
            //    codeEditor.Document.Language = Item.MySyntaxEditor.Document.Language;
            //    codeEditor.Document.SetHeaderAndFooterText(Item.CsCodeHeader, Item.CsCodeFooter);
            //}
            //else
            //{

            //}

            if (item is oText)
                tabControl1.SelectedIndex = 1;

            //         if (item is oPage)
            //           tabControl1.SelectedIndex = 1;

            if (item is oImage)
                buttonDelete.Show();

            if (item is oScratch)
                buttonDelete.Show();


            if (item is oTag)
            {
                tabControl1.SelectedIndex = 1;
            }
            else
                tabControl1.Controls.Remove(tabPageTag);

            textBoxText.Text = Item.TextL.Replace("\r\n", "\n").Trim('\n').Replace("\n", "\r\n");
            textBoxName.Text = Item.Name.Replace("\r\n", "\n").Trim('\n').Replace("\n", "\r\n");
            //ParseSettingsText(1);

            listBoxTag.Items.Add("dimensions");
            listBoxTag.Items.Add("powersupply");

            if (qbook.Core.ThisBook.Language == "en")
                buttonTranslate.Show();
            else
                buttonTranslate.Hide();

            TogglePageTreeView();
        }


        public static ICodeSnippetFolder LoadSampleCSharpCodeSnippetsFromResources()
        {
            // NOTE: If you have file system access, the static CodeSnippetFolder.LoadFrom(path) method easily
            //       loads snippets within a specified file path and should be used instead

            //string[] childPaths = new string[] {
            //    SnippetsPath + "CSharp.Sample_Child_Folder.while.snippet",
            //};
            //ICodeSnippetFolder childFolder = LoadCodeSnippetFolderFromResources("Sample Child Folder", childPaths);

            CodeSnippetSerializer serializer = new CodeSnippetSerializer();
            List<string> rootPaths = new List<string>();
            ICodeSnippetFolder folder = new CodeSnippetFolder("Default");

            if (Directory.Exists(@".\SyntaxEditor.Snippets"))
                foreach (string file in Directory.GetFiles(@".\SyntaxEditor.Snippets", "*.snippet"))
                {
                    rootPaths.Add(file);
                    IEnumerable<ICodeSnippet> snippets = serializer.LoadFromFile(file);
                    if (snippets != null)
                    {
                        foreach (ICodeSnippet snippet in snippets)
                            folder.Items.Add(snippet);
                    }
                }
            //ICodeSnippetFolder rootFolder = LoadCodeSnippetFolderFromResources("Root", rootPaths.ToArray());
            //rootFolder.Folders.Add(childFolder);
            return folder;
        }

        private static ICodeSnippetFolder LoadCodeSnippetFolderFromResources(string folderName, string[] paths)
        {
            ICodeSnippetFolder folder = new CodeSnippetFolder(folderName);
            CodeSnippetSerializer serializer = new CodeSnippetSerializer();

            foreach (string path in paths)
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
                {
                    if (stream != null)
                    {
                        IEnumerable<ICodeSnippet> snippets = serializer.LoadFromStream(stream);
                        if (snippets != null)
                        {
                            foreach (ICodeSnippet snippet in snippets)
                                folder.Items.Add(snippet);
                        }
                    }
                }
            }

            return folder;
        }

        private void CodeEditor_DocumentTextChanged(object sender, EditorSnapshotChangedEventArgs e)
        {
            return;
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.Modified = true;
            DialogResult result = MessageBox.Show("delete " + Item.Name + "/" + Item.Text + "?", "delete object!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                qbook.Core.ThisBook.Main.Remove(Item);
                Close();
            }
        }

        private void buttonTranslate_Click(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.Modified = true;
            Item.TextDE = null;
            Item.TextES = null;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            //Item.CsCode = codeEditor.Text;
            //Item.CsCodeHeader = textBoxHeader.Text.Replace("\r", "").Replace("\n", "\r\n");
            //Item.CsCodeFooter = textBoxFooter.Text.Replace("\r", "").Replace("\n", "\r\n");
            //ApplyCsCode();


            //Main.Qb.Book.Serialize();
            //Main.Qb.Book.Modified = false;

            //ParseSettingsText(2);
            Close();
        }

        private void TextForm_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //Main.Qb.Book.Modified = true; //HALE: compare orig text and current text
            if (e.Control)
            {
                e.IsInputKey = true;
            }
        }

        private void TextForm_KeyDown(object sender, KeyEventArgs e)
        {
            //Main.Qb.Book.Modified = true; //HALE: compare orig text and current text
            //if (e.KeyCode == Keys.Escape)
            //{
            //    if (textBoxSettings.OrigText != textBoxSettings.Text)
            //    {
            //        if (DialogResult.OK == MessageBox.Show("Settings-Text was modified!\r\n\r\nWould you like to undo all recent changes?", "MODIFIED", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning))
            //        {
            //            Item.Settings = textBoxSettings.OrigText.Replace("\r\n", "\n").Trim('\n').Replace("\n", "\r\n");
            //            this.Close();
            //        }
            //    }
            //    //Item.Settings = textBoxSettings.OrigText.Replace("\r\n", "\n").Trim('\n').Replace("\n", "\r\n");
            //    this.Close();
            //}

            if (false)
            {
                if (e.KeyCode == Keys.X && e.Control == true)
                {
                    Clipboard.SetText(qbook.Core.SelectedPage.Serialize());
                    DialogResult result = MessageBox.Show("delete page '" + (qbook.Core.SelectedPage) + "' ?", "delete page!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        //    Main.Qb.File.Object.Remove(Main.Qb.SelectedPage);
                    }
                }
            }
        }


        private void textBoxText_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void textBoxSettings_KeyUp(object sender, KeyEventArgs e)
        {
            //OnTextChanged(textBoxSettings);

        }

        //Timer _textChangedDelayedTimer;

        //void OnTextChanged(ColoredTextBox.ColoredTextBoxControl sender)
        //{
        //    Item.Settings = textBoxSettings.Text.Replace("\r\n", "\n").Trim('\n').Replace("\n", "\r\n");

        //    ////Item.Settings = textBoxSettings.Text.Replace("\r\n", "\n").Trim('\n').Replace("\n", "\r\n");
        //    //if (_textChangedDelayedTimer == null)
        //    //{
        //    //    _textChangedDelayedTimer = new Timer();
        //    //    _textChangedDelayedTimer.Interval = 1000;
        //    //    _textChangedDelayedTimer.Tick += _textChangedDelayedTimer_Tick;
        //    //}
        //    //_textChangedDelayedTimer.Stop(); // Resets the timer
        //    //_textChangedDelayedTimer.Tag = (sender as ColoredTextBox.ColoredTextBoxControl).Text; // This should be done with EventArgs
        //    //_textChangedDelayedTimer.Start();
        //}

        //private void _textChangedDelayedTimer_Tick(object sender, EventArgs e)
        //{
        //    var timer = sender as Timer;
        //    if (timer == null)
        //    {
        //        return;
        //    }
        //    timer.Stop();

        //    Item.Settings = textBoxSettings.Text.Replace("\r\n", "\n").Trim('\n').Replace("\n", "\r\n");
        //}



        private void textBoxName_KeyUp(object sender, KeyEventArgs e)
        {
            qbook.Core.ThisBook.Modified = true;
            Item.Name = textBoxName.Text.Replace("\r\n", "\n").Trim('\n').Replace("\n", "\r\n");
        }

        private void textBoxName_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void textBoxText_KeyUp(object sender, KeyEventArgs e)
        {
            qbook.Core.ThisBook.Modified = true;
            Item.TextL = textBoxText.Text.Replace("\r\n", "\n").Trim('\n').Replace("\n", "\r\n");
        }


        private void listBoxTag_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (listBoxTag.Items[listBoxTag.SelectedIndex].ToString() == "dimensions")
                {
                    qbook.Core.ThisBook.Modified = true;
                    textBoxText.Text = "dimensions\n- height=\txxx mm\n- width=\txxx mm\n- depth=\txxx mm".Replace("\n", "\r\n");
                    Item.TextL = textBoxText.Text.Replace("\r\n", "\n").Trim('\n').Replace("\n", "\r\n");
                    tabControl1.SelectedIndex = 1;
                }
                if (listBoxTag.Items[listBoxTag.SelectedIndex].ToString() == "powersupply")
                {
                    qbook.Core.ThisBook.Modified = true;
                    textBoxText.Text = "powersupply\n- voltage\t230VAC\n- current\tmax. xxx A".Replace("\n", "\r\n");
                    Item.TextL = textBoxText.Text.Replace("\r\n", "\n").Trim('\n').Replace("\n", "\r\n");
                    tabControl1.SelectedIndex = 1;
                }
            }
            catch
            { }

        }

        private void buttonPosition_Click(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.Modified = true;
            if (Item is oTag)
                (Item as oTag).Position = (sender as System.Windows.Forms.Control).Text;
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {

            //if (Item is oAkc)
            //{
            //    lock ((Item as oAkc).AkQueries)
            //    {
            //        (Item as oAkc).AkQueries.Clear();
            //    }
            //}
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //this.Text = this.ActiveControl.Name;
        }

        private void EditObjectForm_Load(object sender, EventArgs e)
        {
            this.CancelButton = null;
        }

        private void CodeEditor_DocumentIsModifiedChanged(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.Modified = true;
            //codeEditor.Document.IsModified = false; //HACK //TODO trigger for next time 
        }

        //private void DotNetProjectAssemblyReferenceLoader(object sender, DoWorkEventArgs e)
        //{
        //    // Add some common assemblies for reflection (any custom assemblies could be added using various Add overloads instead)
        //    SyntaxEditorHelper.AddCommonDotNetSystemAssemblyReferences(projectAssembly);
        //}

        private void AssemblyReferences_Changed(object sender, ActiproSoftware.Text.Utility.CollectionChangeEventArgs<IProjectAssemblyReference> e)
        {
            if (refreshReferencesTimer is null)
                refreshReferencesTimer = new System.Threading.Timer(RefreshReferenceListCallback);

            // Reset the timer each time a new event is raised (without auto-restart)
            refreshReferencesTimer.Change(dueTime: 250, period: System.Threading.Timeout.Infinite);
        }

        private void RefreshReferenceListCallback(object stateInfo)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((System.Action)(() => this.RefreshReferenceListCallback(stateInfo)));
                return;
            }
        }

        //private void xOnAssemblyReferencesChanged(object sender, Text.Utility.CollectionChangeEventArgs<IProjectAssemblyReference> e)
        //{
        //    // Assessmblies can be added/removed quickly, especially during initial discovery.
        //    // Throttle UI refreshing until no "change" events have been received for a given time.
        //    if (refreshReferencesTimer is null)
        //        refreshReferencesTimer = new System.Threading.Timer(RefreshReferenceListCallback);

        //    // Reset the timer each time a new event is raised (without auto-restart)
        //    refreshReferencesTimer.Change(dueTime: 250, period: System.Threading.Timeout.Infinite);
        //}


        string lastAutoApplyText = null;
        private void textBoxSettings_TextChangedDelayed(ColoredTextBox.ColoredTextBoxControl.TextChangedEventArgs e)
        {
            //Item.Settings = textBoxSettings.Text.Replace("\r\n", "\n").Trim('\n').Replace("\n", "\r\n");
        }
        private void EditObjectForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void buttonUndo_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Undo all Setting-Text changes?", "UNDO SETTINGS", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                //TODO
            }
        }

        private void textBoxName_TextChanged(object sender, EventArgs e)
        {
        }




        private void checkBoxShowTree_CheckedChanged(object sender, EventArgs e)
        {
            TogglePageTreeView();
        }

        void TogglePageTreeView()
        {
            if (checkBoxShowTree.Checked)
            {
                treeViewCodePages.Visible = true;
                tabControl1.Left = treeViewCodePages.Width + 4;
                tabControl1.Width = this.Width - treeViewCodePages.Width - 24;

                PopulatePageTreeView();
            }
            else
            {
                treeViewCodePages.Visible = false;
                tabControl1.Left = 0 + 4;
                tabControl1.Width = this.Width - 0 - 24;
            }
        }

        void PopulatePageTreeView()
        {
            treeViewCodePages.Nodes.Clear();
            var pageList = qbook.Core.ThisBook.Main.Objects.OfType<oPage>();
            //var ListOfObjectsSorted = ListOfObjects.OrderBy(r => r.Nr).ToList();
            var mainGroup = "Items/Pages";
            var topNode = new TreeNode(mainGroup);
            treeViewCodePages.Nodes.Add(topNode);
            string currentGroup = mainGroup; // ListOfObjectsSorted.First().Name;
            var treeNodes = new List<TreeNode>();
            var childNodes = new List<TreeNode>();
            foreach (var page in pageList)
            {
                if (currentGroup == mainGroup) // rule.Group)
                {
                    var pageNode = new TreeNode(page.Name);
                    pageNode.Tag = page;
                    childNodes.Add(pageNode);
                }
                else
                {
                    if (childNodes.Count > 0)
                    {
                        treeNodes.Add(new TreeNode(currentGroup, childNodes.ToArray()));
                        childNodes = new List<TreeNode>();
                    }
                    childNodes.Add(new TreeNode(page.Name));
                    currentGroup = mainGroup; // item.Group;
                }
            }
            //if (childNodes.Count > 0)
            //{
            //    treeNodes.Add(new TreeNode(currentGroup, childNodes.ToArray()));

            //}
            //topNode.Nodes.Add(new TreeNode(currentGroup, childNodes.ToArray()));
            topNode.Nodes.AddRange(childNodes.ToArray());

            treeViewCodePages.Nodes[0].Nodes.AddRange(treeNodes.ToArray());
            treeViewCodePages.ExpandAll();
        }

        System.Windows.Forms.TreeNode lastClickedNode = null;
        private void treeViewCodePages_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.Node.Level == 1)
            {
                lastClickedNode = e.Node;
                //var clickedNode = ((TreeView)sender).GetNodeAt(new Point(e.X, e.Y));
                contextMenuPageTree.Show(treeViewCodePages, e.Location);
            }
            else
            {
                lastClickedNode = e.Node;
            }
        }

        private void addSubCodeClassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newNode = new System.Windows.Forms.TreeNode("new Code");
            lastClickedNode.Nodes.Add(newNode);
            lastClickedNode.ExpandAll();
            newNode.BeginEdit();
            treeViewCodePages.LabelEdit = true;
        }

        private void treeViewCodePages_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            //TODO: ensure name is unique

        }

        private void treeViewCodePages_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        //private bool AnyCollapsed(OutliningNode Node)
        //{
        //    if (!Node.Expanded)
        //        return true;
        //    foreach (OutliningNode childNode in Node)
        //    {
        //        if (AnyCollapsed(childNode))
        //            return true;
        //    }
        //    return false;
        //}

        //protected void CollapseXmlMenu_Click(object sender, EventArgs e)
        //{
        //    if (AnyCollapsed(this.Document.Outlining.RootNode))
        //        this.Document.Outlining.RootNode.ExpandDescendants();
        //    else
        //        this.Document.Outlining.RootNode.CollapseDescendants();
        //}
    }

}
