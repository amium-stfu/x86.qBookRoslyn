using System.Drawing;
using System.Windows.Forms;

namespace qbook.CodeEditor
{
    partial class FormEditor
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEditor));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.ProjectTree = new System.Windows.Forms.TreeView();
            this.vBarProjectTree = new qbook.CodeEditor.TreeViewVerticalBar();
            this.panelSplttter1 = new System.Windows.Forms.Panel();
            this.tblViewMethodes = new System.Windows.Forms.TableLayoutPanel();
            this.gridViewMethodes = new System.Windows.Forms.DataGridView();
            this.vBarMethodes = new qbook.CodeEditor.GridViewVerticalBar();
            this.lblMethodes = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.Editor = new ScintillaNET.Scintilla();
            this.panelFunctions = new System.Windows.Forms.Panel();
            this.btnShowHidden = new System.Windows.Forms.Button();
            this.btnParagraph = new System.Windows.Forms.Button();
            this.btnSnippets = new System.Windows.Forms.Button();
            this.btnFindReplace = new System.Windows.Forms.Button();
            this.btnFind = new System.Windows.Forms.Button();
            this.btnFormat = new System.Windows.Forms.Button();
            this.vBarEditor = new qbook.CodeEditor.ScintillaVerticalBar();
            this.hBarEditor = new qbook.CodeEditor.ScintillaHorizontalBar();
            this.panelSplitter3 = new System.Windows.Forms.Panel();
            this.PanelTabs = new System.Windows.Forms.Panel();
            this.TablePanelOutputs = new System.Windows.Forms.TableLayoutPanel();
            this.panelOutputs = new System.Windows.Forms.Panel();
            this.btnShowFindReplaceOutput = new System.Windows.Forms.Button();
            this.btnBuildOutput = new System.Windows.Forms.Button();
            this.btnEditorOutput = new System.Windows.Forms.Button();
            this.panelSplitter2 = new System.Windows.Forms.Panel();
            this.panelSplitter4 = new System.Windows.Forms.Panel();
            this.vBarOutputs = new qbook.CodeEditor.GridViewVerticalBar();
            this.panelControl = new DevExpress.XtraEditors.PanelControl();
            this.btnToggleTheme = new System.Windows.Forms.Button();
            this.btnReload = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnRebuild = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.panelStatus = new System.Windows.Forms.Panel();
            this.labelStatus = new System.Windows.Forms.Label();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.contextMenuTreeView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addPageBeforeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addPageAfterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addSubCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.customToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uDLClientToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aKClientToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aKServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.streamClientToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hidePageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuOpenWorkspace = new System.Windows.Forms.ToolStripMenuItem();
            this.renameCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renamePageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuIncludeCode = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridViewBuildOutput = new System.Windows.Forms.DataGridView();
            this.dataGridViewFindReplace = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tblViewMethodes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewMethodes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panelFunctions.SuspendLayout();
            this.TablePanelOutputs.SuspendLayout();
            this.panelOutputs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl)).BeginInit();
            this.panelControl.SuspendLayout();
            this.panelStatus.SuspendLayout();
            this.contextMenuTreeView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewBuildOutput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFindReplace)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 17F));
            this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panelControl, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panelStatus, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 47F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 4F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1916, 838);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 54);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1910, 725);
            this.splitContainer1.SplitterDistance = 353;
            this.splitContainer1.SplitterWidth = 6;
            this.splitContainer1.TabIndex = 4;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.tableLayoutPanel4);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.tblViewMethodes);
            this.splitContainer3.Size = new System.Drawing.Size(353, 725);
            this.splitContainer3.SplitterDistance = 376;
            this.splitContainer3.SplitterWidth = 6;
            this.splitContainer3.TabIndex = 0;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Controls.Add(this.ProjectTree, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.vBarProjectTree, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.panelSplttter1, 0, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 2F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(353, 376);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // ProjectTree
            // 
            this.ProjectTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProjectTree.Location = new System.Drawing.Point(3, 3);
            this.ProjectTree.Name = "ProjectTree";
            this.ProjectTree.Scrollable = false;
            this.ProjectTree.ShowLines = false;
            this.ProjectTree.Size = new System.Drawing.Size(327, 368);
            this.ProjectTree.TabIndex = 0;
            this.ProjectTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ProjectTree_AfterSelect);
            this.ProjectTree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeProject_NodeMouseClick);
            // 
            // vBarProjectTree
            // 
            this.vBarProjectTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vBarProjectTree.Location = new System.Drawing.Point(336, 3);
            this.vBarProjectTree.Name = "vBarProjectTree";
            this.vBarProjectTree.SetBackColor = System.Drawing.Color.LightGray;
            this.vBarProjectTree.SetForeColor = System.Drawing.Color.DodgerBlue;
            this.vBarProjectTree.Size = new System.Drawing.Size(14, 368);
            this.vBarProjectTree.TabIndex = 1;
            // 
            // panelSplttter1
            // 
            this.panelSplttter1.BackColor = System.Drawing.Color.DarkGray;
            this.tableLayoutPanel4.SetColumnSpan(this.panelSplttter1, 2);
            this.panelSplttter1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSplttter1.Location = new System.Drawing.Point(0, 374);
            this.panelSplttter1.Margin = new System.Windows.Forms.Padding(0);
            this.panelSplttter1.Name = "panelSplttter1";
            this.panelSplttter1.Size = new System.Drawing.Size(353, 2);
            this.panelSplttter1.TabIndex = 2;
            // 
            // tblViewMethodes
            // 
            this.tblViewMethodes.ColumnCount = 2;
            this.tblViewMethodes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblViewMethodes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblViewMethodes.Controls.Add(this.gridViewMethodes, 0, 1);
            this.tblViewMethodes.Controls.Add(this.vBarMethodes, 1, 1);
            this.tblViewMethodes.Controls.Add(this.lblMethodes, 0, 0);
            this.tblViewMethodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblViewMethodes.Location = new System.Drawing.Point(0, 0);
            this.tblViewMethodes.Name = "tblViewMethodes";
            this.tblViewMethodes.RowCount = 2;
            this.tblViewMethodes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tblViewMethodes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblViewMethodes.Size = new System.Drawing.Size(353, 343);
            this.tblViewMethodes.TabIndex = 0;
            // 
            // gridViewMethodes
            // 
            this.gridViewMethodes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridViewMethodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridViewMethodes.Location = new System.Drawing.Point(30, 30);
            this.gridViewMethodes.Margin = new System.Windows.Forms.Padding(30, 0, 0, 0);
            this.gridViewMethodes.Name = "gridViewMethodes";
            this.gridViewMethodes.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridViewMethodes.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.gridViewMethodes.Size = new System.Drawing.Size(303, 313);
            this.gridViewMethodes.TabIndex = 0;
            // 
            // vBarMethodes
            // 
            this.vBarMethodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vBarMethodes.Location = new System.Drawing.Point(333, 30);
            this.vBarMethodes.Margin = new System.Windows.Forms.Padding(0);
            this.vBarMethodes.Name = "vBarMethodes";
            this.vBarMethodes.SetBackColor = System.Drawing.Color.LightGray;
            this.vBarMethodes.SetForeColor = System.Drawing.Color.DodgerBlue;
            this.vBarMethodes.Size = new System.Drawing.Size(20, 313);
            this.vBarMethodes.TabIndex = 2;
            // 
            // lblMethodes
            // 
            this.lblMethodes.AutoSize = true;
            this.lblMethodes.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblMethodes.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMethodes.Location = new System.Drawing.Point(30, 0);
            this.lblMethodes.Margin = new System.Windows.Forms.Padding(30, 0, 3, 0);
            this.lblMethodes.Name = "lblMethodes";
            this.lblMethodes.Size = new System.Drawing.Size(49, 30);
            this.lblMethodes.TabIndex = 3;
            this.lblMethodes.Text = "Items:";
            this.lblMethodes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.tableLayoutPanel2);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.TablePanelOutputs);
            this.splitContainer2.Size = new System.Drawing.Size(1551, 725);
            this.splitContainer2.SplitterDistance = 541;
            this.splitContainer2.SplitterWidth = 2;
            this.splitContainer2.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 5;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 2F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.Editor, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.panelFunctions, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.vBarEditor, 3, 1);
            this.tableLayoutPanel2.Controls.Add(this.hBarEditor, 2, 2);
            this.tableLayoutPanel2.Controls.Add(this.panelSplitter3, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.PanelTabs, 2, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1551, 541);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // Editor
            // 
            this.Editor.AutocompleteListSelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(212)))));
            this.Editor.BorderStyle = ScintillaNET.BorderStyle.FixedSingle;
            this.Editor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Editor.HScrollBar = false;
            this.Editor.LexerName = null;
            this.Editor.Location = new System.Drawing.Point(48, 25);
            this.Editor.Margin = new System.Windows.Forms.Padding(0);
            this.Editor.Name = "Editor";
            this.Editor.Size = new System.Drawing.Size(1463, 496);
            this.Editor.TabIndex = 1;
            this.Editor.Text = "scintilla1";
            this.Editor.VScrollBar = false;
            this.Editor.SizeChanged += new System.EventHandler(this.Editor_SizeChanged);
            this.Editor.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Editor_KeyDown);
            // 
            // panelFunctions
            // 
            this.panelFunctions.Controls.Add(this.btnShowHidden);
            this.panelFunctions.Controls.Add(this.btnParagraph);
            this.panelFunctions.Controls.Add(this.btnSnippets);
            this.panelFunctions.Controls.Add(this.btnFindReplace);
            this.panelFunctions.Controls.Add(this.btnFind);
            this.panelFunctions.Controls.Add(this.btnFormat);
            this.panelFunctions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelFunctions.Location = new System.Drawing.Point(5, 28);
            this.panelFunctions.Name = "panelFunctions";
            this.panelFunctions.Size = new System.Drawing.Size(40, 490);
            this.panelFunctions.TabIndex = 2;
            // 
            // btnShowHidden
            // 
            this.btnShowHidden.BackColor = System.Drawing.Color.IndianRed;
            this.btnShowHidden.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnShowHidden.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnShowHidden.FlatAppearance.BorderSize = 2;
            this.btnShowHidden.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShowHidden.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnShowHidden.Image = global::qbook.Properties.Resources.HiddenCode_p48;
            this.btnShowHidden.Location = new System.Drawing.Point(0, 195);
            this.btnShowHidden.Margin = new System.Windows.Forms.Padding(9);
            this.btnShowHidden.Name = "btnShowHidden";
            this.btnShowHidden.Size = new System.Drawing.Size(40, 39);
            this.btnShowHidden.TabIndex = 0;
            this.btnShowHidden.UseVisualStyleBackColor = false;
            this.btnShowHidden.Click += new System.EventHandler(this.btnShowHidden_Click);
            // 
            // btnParagraph
            // 
            this.btnParagraph.BackColor = System.Drawing.Color.IndianRed;
            this.btnParagraph.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnParagraph.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnParagraph.FlatAppearance.BorderSize = 2;
            this.btnParagraph.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnParagraph.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnParagraph.Image = global::qbook.Properties.Resources.paragraph_icon_icons_com_72298;
            this.btnParagraph.Location = new System.Drawing.Point(0, 156);
            this.btnParagraph.Margin = new System.Windows.Forms.Padding(9);
            this.btnParagraph.Name = "btnParagraph";
            this.btnParagraph.Size = new System.Drawing.Size(40, 39);
            this.btnParagraph.TabIndex = 0;
            this.toolTip1.SetToolTip(this.btnParagraph, "Show invisible characters such as whitespace, tabs, and end-of-line markers.");
            this.btnParagraph.UseVisualStyleBackColor = false;
            this.btnParagraph.Click += new System.EventHandler(this.btnParagraph_Click);
            // 
            // btnSnippets
            // 
            this.btnSnippets.BackColor = System.Drawing.Color.IndianRed;
            this.btnSnippets.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSnippets.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnSnippets.FlatAppearance.BorderSize = 2;
            this.btnSnippets.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSnippets.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnSnippets.Image = global::qbook.Properties.Resources.snippets_icon_237382;
            this.btnSnippets.Location = new System.Drawing.Point(0, 117);
            this.btnSnippets.Name = "btnSnippets";
            this.btnSnippets.Size = new System.Drawing.Size(40, 39);
            this.btnSnippets.TabIndex = 0;
            this.toolTip1.SetToolTip(this.btnSnippets, "Snippets");
            this.btnSnippets.UseVisualStyleBackColor = false;
            this.btnSnippets.Click += new System.EventHandler(this.btnSnippets_Click);
            // 
            // btnFindReplace
            // 
            this.btnFindReplace.BackColor = System.Drawing.Color.IndianRed;
            this.btnFindReplace.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnFindReplace.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnFindReplace.FlatAppearance.BorderSize = 2;
            this.btnFindReplace.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFindReplace.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnFindReplace.Image = global::qbook.Properties.Resources._find_replace_89967;
            this.btnFindReplace.Location = new System.Drawing.Point(0, 78);
            this.btnFindReplace.Name = "btnFindReplace";
            this.btnFindReplace.Size = new System.Drawing.Size(40, 39);
            this.btnFindReplace.TabIndex = 0;
            this.toolTip1.SetToolTip(this.btnFindReplace, "Find/Replace (CTRL + H)");
            this.btnFindReplace.UseVisualStyleBackColor = false;
            this.btnFindReplace.Click += new System.EventHandler(this.btnFindReplace_Click);
            // 
            // btnFind
            // 
            this.btnFind.BackColor = System.Drawing.Color.IndianRed;
            this.btnFind.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnFind.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnFind.FlatAppearance.BorderSize = 2;
            this.btnFind.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFind.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnFind.Image = global::qbook.Properties.Resources.seo_social_web_network_internet_340_icon_icons_com_61497;
            this.btnFind.Location = new System.Drawing.Point(0, 39);
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = new System.Drawing.Size(40, 39);
            this.btnFind.TabIndex = 0;
            this.toolTip1.SetToolTip(this.btnFind, "Find (CTRL + F)");
            this.btnFind.UseVisualStyleBackColor = false;
            this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
            // 
            // btnFormat
            // 
            this.btnFormat.BackColor = System.Drawing.Color.IndianRed;
            this.btnFormat.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnFormat.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnFormat.FlatAppearance.BorderSize = 2;
            this.btnFormat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFormat.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnFormat.Image = global::qbook.Properties.Resources._format_indent_increase_89789;
            this.btnFormat.Location = new System.Drawing.Point(0, 0);
            this.btnFormat.Name = "btnFormat";
            this.btnFormat.Size = new System.Drawing.Size(40, 39);
            this.btnFormat.TabIndex = 0;
            this.toolTip1.SetToolTip(this.btnFormat, "Format Code (CTRL -> K -> F)");
            this.btnFormat.UseVisualStyleBackColor = false;
            this.btnFormat.Click += new System.EventHandler(this.btnFormat_Click);
            // 
            // vBarEditor
            // 
            this.vBarEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vBarEditor.Location = new System.Drawing.Point(1511, 25);
            this.vBarEditor.Margin = new System.Windows.Forms.Padding(0);
            this.vBarEditor.Name = "vBarEditor";
            this.vBarEditor.SetBackColor = System.Drawing.Color.White;
            this.vBarEditor.SetForeColor = System.Drawing.Color.Black;
            this.vBarEditor.Size = new System.Drawing.Size(20, 496);
            this.vBarEditor.TabIndex = 3;
            // 
            // hBarEditor
            // 
            this.hBarEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hBarEditor.Location = new System.Drawing.Point(48, 521);
            this.hBarEditor.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.hBarEditor.Name = "hBarEditor";
            this.hBarEditor.SetBackColor = System.Drawing.Color.White;
            this.hBarEditor.SetForeColor = System.Drawing.Color.Black;
            this.hBarEditor.Size = new System.Drawing.Size(1463, 17);
            this.hBarEditor.TabIndex = 4;
            // 
            // panelSplitter3
            // 
            this.panelSplitter3.BackColor = System.Drawing.Color.Silver;
            this.panelSplitter3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSplitter3.Location = new System.Drawing.Point(0, 25);
            this.panelSplitter3.Margin = new System.Windows.Forms.Padding(0);
            this.panelSplitter3.Name = "panelSplitter3";
            this.tableLayoutPanel2.SetRowSpan(this.panelSplitter3, 2);
            this.panelSplitter3.Size = new System.Drawing.Size(2, 516);
            this.panelSplitter3.TabIndex = 5;
            // 
            // PanelTabs
            // 
            this.PanelTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelTabs.Location = new System.Drawing.Point(48, 0);
            this.PanelTabs.Margin = new System.Windows.Forms.Padding(0);
            this.PanelTabs.Name = "PanelTabs";
            this.PanelTabs.Size = new System.Drawing.Size(1463, 25);
            this.PanelTabs.TabIndex = 6;
            // 
            // TablePanelOutputs
            // 
            this.TablePanelOutputs.ColumnCount = 3;
            this.TablePanelOutputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.TablePanelOutputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TablePanelOutputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TablePanelOutputs.Controls.Add(this.panelOutputs, 1, 1);
            this.TablePanelOutputs.Controls.Add(this.panelSplitter2, 0, 0);
            this.TablePanelOutputs.Controls.Add(this.panelSplitter4, 0, 1);
            this.TablePanelOutputs.Controls.Add(this.vBarOutputs, 2, 2);
            this.TablePanelOutputs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TablePanelOutputs.Location = new System.Drawing.Point(0, 0);
            this.TablePanelOutputs.Name = "TablePanelOutputs";
            this.TablePanelOutputs.RowCount = 3;
            this.TablePanelOutputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 4F));
            this.TablePanelOutputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.TablePanelOutputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TablePanelOutputs.Size = new System.Drawing.Size(1551, 182);
            this.TablePanelOutputs.TabIndex = 1;
            // 
            // panelOutputs
            // 
            this.panelOutputs.Controls.Add(this.btnShowFindReplaceOutput);
            this.panelOutputs.Controls.Add(this.btnBuildOutput);
            this.panelOutputs.Controls.Add(this.btnEditorOutput);
            this.panelOutputs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelOutputs.Location = new System.Drawing.Point(46, 4);
            this.panelOutputs.Margin = new System.Windows.Forms.Padding(0);
            this.panelOutputs.Name = "panelOutputs";
            this.panelOutputs.Size = new System.Drawing.Size(1485, 25);
            this.panelOutputs.TabIndex = 2;
            // 
            // btnShowFindReplaceOutput
            // 
            this.btnShowFindReplaceOutput.BackColor = System.Drawing.Color.LightGray;
            this.btnShowFindReplaceOutput.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnShowFindReplaceOutput.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
            this.btnShowFindReplaceOutput.FlatAppearance.BorderSize = 0;
            this.btnShowFindReplaceOutput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShowFindReplaceOutput.Location = new System.Drawing.Point(165, 0);
            this.btnShowFindReplaceOutput.Margin = new System.Windows.Forms.Padding(0);
            this.btnShowFindReplaceOutput.Name = "btnShowFindReplaceOutput";
            this.btnShowFindReplaceOutput.Size = new System.Drawing.Size(87, 25);
            this.btnShowFindReplaceOutput.TabIndex = 1;
            this.btnShowFindReplaceOutput.Text = "Find/Replace Output";
            this.btnShowFindReplaceOutput.UseVisualStyleBackColor = false;
            this.btnShowFindReplaceOutput.Click += new System.EventHandler(this.btnFindReplaceOutput_Click);
            // 
            // btnBuildOutput
            // 
            this.btnBuildOutput.BackColor = System.Drawing.Color.LightGray;
            this.btnBuildOutput.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnBuildOutput.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
            this.btnBuildOutput.FlatAppearance.BorderSize = 0;
            this.btnBuildOutput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBuildOutput.Location = new System.Drawing.Point(78, 0);
            this.btnBuildOutput.Margin = new System.Windows.Forms.Padding(0);
            this.btnBuildOutput.Name = "btnBuildOutput";
            this.btnBuildOutput.Size = new System.Drawing.Size(87, 25);
            this.btnBuildOutput.TabIndex = 1;
            this.btnBuildOutput.Text = "Build Output";
            this.btnBuildOutput.UseVisualStyleBackColor = false;
            this.btnBuildOutput.Click += new System.EventHandler(this.btnBuildOutput_Click);
            // 
            // btnEditorOutput
            // 
            this.btnEditorOutput.BackColor = System.Drawing.Color.LightGray;
            this.btnEditorOutput.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnEditorOutput.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
            this.btnEditorOutput.FlatAppearance.BorderSize = 0;
            this.btnEditorOutput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditorOutput.Location = new System.Drawing.Point(0, 0);
            this.btnEditorOutput.Margin = new System.Windows.Forms.Padding(0);
            this.btnEditorOutput.Name = "btnEditorOutput";
            this.btnEditorOutput.Size = new System.Drawing.Size(78, 25);
            this.btnEditorOutput.TabIndex = 1;
            this.btnEditorOutput.Text = "Editor Output";
            this.btnEditorOutput.UseVisualStyleBackColor = false;
            this.btnEditorOutput.Click += new System.EventHandler(this.btnEditorOutput_Click);
            // 
            // panelSplitter2
            // 
            this.panelSplitter2.BackColor = System.Drawing.Color.LightGray;
            this.TablePanelOutputs.SetColumnSpan(this.panelSplitter2, 3);
            this.panelSplitter2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSplitter2.Location = new System.Drawing.Point(0, 0);
            this.panelSplitter2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.panelSplitter2.Name = "panelSplitter2";
            this.panelSplitter2.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.panelSplitter2.Size = new System.Drawing.Size(1551, 2);
            this.panelSplitter2.TabIndex = 3;
            // 
            // panelSplitter4
            // 
            this.panelSplitter4.BackColor = System.Drawing.Color.Silver;
            this.panelSplitter4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSplitter4.Location = new System.Drawing.Point(0, 4);
            this.panelSplitter4.Margin = new System.Windows.Forms.Padding(0, 0, 44, 0);
            this.panelSplitter4.Name = "panelSplitter4";
            this.TablePanelOutputs.SetRowSpan(this.panelSplitter4, 2);
            this.panelSplitter4.Size = new System.Drawing.Size(2, 178);
            this.panelSplitter4.TabIndex = 4;
            // 
            // vBarOutputs
            // 
            this.vBarOutputs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vBarOutputs.Location = new System.Drawing.Point(1534, 32);
            this.vBarOutputs.Name = "vBarOutputs";
            this.vBarOutputs.SetBackColor = System.Drawing.Color.LightGray;
            this.vBarOutputs.SetForeColor = System.Drawing.Color.DodgerBlue;
            this.vBarOutputs.Size = new System.Drawing.Size(14, 147);
            this.vBarOutputs.TabIndex = 6;
            // 
            // panelControl
            // 
            this.panelControl.Appearance.BackColor = System.Drawing.Color.White;
            this.panelControl.Appearance.Options.UseBackColor = true;
            this.panelControl.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelControl.Controls.Add(this.btnToggleTheme);
            this.panelControl.Controls.Add(this.btnReload);
            this.panelControl.Controls.Add(this.btnSave);
            this.panelControl.Controls.Add(this.btnRebuild);
            this.panelControl.Controls.Add(this.btnRun);
            this.panelControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl.Location = new System.Drawing.Point(3, 3);
            this.panelControl.Name = "panelControl";
            this.panelControl.Size = new System.Drawing.Size(1910, 41);
            this.panelControl.TabIndex = 5;
            // 
            // btnToggleTheme
            // 
            this.btnToggleTheme.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnToggleTheme.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnToggleTheme.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleTheme.Image = global::qbook.Properties.Resources.dark_theme_64p;
            this.btnToggleTheme.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnToggleTheme.Location = new System.Drawing.Point(314, 0);
            this.btnToggleTheme.Name = "btnToggleTheme";
            this.btnToggleTheme.Size = new System.Drawing.Size(91, 41);
            this.btnToggleTheme.TabIndex = 0;
            this.btnToggleTheme.Text = "Theme";
            this.btnToggleTheme.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnToggleTheme.UseVisualStyleBackColor = true;
            this.btnToggleTheme.Click += new System.EventHandler(this.btnToggleTheme_Click);
            // 
            // btnReload
            // 
            this.btnReload.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnReload.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnReload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReload.Image = global::qbook.Properties.Resources.arrow_load_24p;
            this.btnReload.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReload.Location = new System.Drawing.Point(233, 0);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(81, 41);
            this.btnReload.TabIndex = 3;
            this.btnReload.Text = "Reload";
            this.btnReload.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // btnSave
            // 
            this.btnSave.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnSave.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Image = global::qbook.Properties.Resources.save_24p;
            this.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSave.Location = new System.Drawing.Point(159, 0);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(74, 41);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save\r\n";
            this.btnSave.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnRebuild
            // 
            this.btnRebuild.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnRebuild.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnRebuild.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRebuild.Image = global::qbook.Properties.Resources.cogBig_48p;
            this.btnRebuild.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRebuild.Location = new System.Drawing.Point(70, 0);
            this.btnRebuild.Name = "btnRebuild";
            this.btnRebuild.Size = new System.Drawing.Size(89, 41);
            this.btnRebuild.TabIndex = 1;
            this.btnRebuild.Text = "Save\r\nRebuild";
            this.btnRebuild.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnRebuild.UseVisualStyleBackColor = true;
            this.btnRebuild.Click += new System.EventHandler(this.btnRebuild_Click);
            // 
            // btnRun
            // 
            this.btnRun.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnRun.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnRun.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRun.Image = global::qbook.Properties.Resources.play_black_triangle48p;
            this.btnRun.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRun.Location = new System.Drawing.Point(0, 0);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(70, 41);
            this.btnRun.TabIndex = 0;
            this.btnRun.Text = "Run";
            this.btnRun.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // panelStatus
            // 
            this.panelStatus.Controls.Add(this.labelStatus);
            this.panelStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelStatus.Location = new System.Drawing.Point(3, 785);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Size = new System.Drawing.Size(1910, 50);
            this.panelStatus.TabIndex = 6;
            // 
            // labelStatus
            // 
            this.labelStatus.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(0, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(212, 50);
            this.labelStatus.TabIndex = 0;
            this.labelStatus.Text = "label1";
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "play_red_button_icon_227850.png");
            this.imageList1.Images.SetKeyName(1, "book_48p.png");
            this.imageList1.Images.SetKeyName(2, "Page_48p.png");
            this.imageList1.Images.SetKeyName(3, "C#.png");
            this.imageList1.Images.SetKeyName(4, "hidden_eye_40p_black.png");
            this.imageList1.Images.SetKeyName(5, "CodeOff_Light.png");
            this.imageList1.Images.SetKeyName(6, "foldergrey_93178.png");
            this.imageList1.Images.SetKeyName(7, "folderorangeopen_93000.png");
            this.imageList1.Images.SetKeyName(8, "forbidden_icon_242335.png");
            // 
            // contextMenuTreeView
            // 
            this.contextMenuTreeView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addPageBeforeToolStripMenuItem,
            this.addPageAfterToolStripMenuItem,
            this.addSubCodeToolStripMenuItem,
            this.hidePageToolStripMenuItem,
            this.deleteStripMenuItem,
            this.toolStripMenuOpenWorkspace,
            this.renameCodeToolStripMenuItem,
            this.renamePageToolStripMenuItem,
            this.toolStripMenuIncludeCode});
            this.contextMenuTreeView.Name = "contextMenuTreeView";
            this.contextMenuTreeView.Size = new System.Drawing.Size(165, 202);
            // 
            // addPageBeforeToolStripMenuItem
            // 
            this.addPageBeforeToolStripMenuItem.Name = "addPageBeforeToolStripMenuItem";
            this.addPageBeforeToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.addPageBeforeToolStripMenuItem.Text = "Add Page before";
            this.addPageBeforeToolStripMenuItem.Click += new System.EventHandler(this.addPageBeforeToolStripMenuItem_Click);
            // 
            // addPageAfterToolStripMenuItem
            // 
            this.addPageAfterToolStripMenuItem.Name = "addPageAfterToolStripMenuItem";
            this.addPageAfterToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.addPageAfterToolStripMenuItem.Text = "Add Page after";
            this.addPageAfterToolStripMenuItem.Click += new System.EventHandler(this.addPageAfterToolStripMenuItem_Click);
            // 
            // addSubCodeToolStripMenuItem
            // 
            this.addSubCodeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.customToolStripMenuItem,
            this.uDLClientToolStripMenuItem,
            this.aKClientToolStripMenuItem,
            this.aKServerToolStripMenuItem,
            this.streamClientToolStripMenuItem});
            this.addSubCodeToolStripMenuItem.Name = "addSubCodeToolStripMenuItem";
            this.addSubCodeToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.addSubCodeToolStripMenuItem.Text = "Add SubCode";
            // 
            // customToolStripMenuItem
            // 
            this.customToolStripMenuItem.Name = "customToolStripMenuItem";
            this.customToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.customToolStripMenuItem.Text = "Custom";
            this.customToolStripMenuItem.Click += new System.EventHandler(this.customToolStripMenuItem_Click);
            // 
            // uDLClientToolStripMenuItem
            // 
            this.uDLClientToolStripMenuItem.Name = "uDLClientToolStripMenuItem";
            this.uDLClientToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.uDLClientToolStripMenuItem.Text = "UDL Client";
            // 
            // aKClientToolStripMenuItem
            // 
            this.aKClientToolStripMenuItem.Name = "aKClientToolStripMenuItem";
            this.aKClientToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.aKClientToolStripMenuItem.Text = "AK Client";
            // 
            // aKServerToolStripMenuItem
            // 
            this.aKServerToolStripMenuItem.Name = "aKServerToolStripMenuItem";
            this.aKServerToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.aKServerToolStripMenuItem.Text = "AK Server";
            // 
            // streamClientToolStripMenuItem
            // 
            this.streamClientToolStripMenuItem.Name = "streamClientToolStripMenuItem";
            this.streamClientToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.streamClientToolStripMenuItem.Text = "Stream Client";
            // 
            // hidePageToolStripMenuItem
            // 
            this.hidePageToolStripMenuItem.Name = "hidePageToolStripMenuItem";
            this.hidePageToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.hidePageToolStripMenuItem.Text = "Hide Page";
            this.hidePageToolStripMenuItem.CheckedChanged += new System.EventHandler(this.hidePageToolStripMenuItem_CheckedChanged);
            this.hidePageToolStripMenuItem.Click += new System.EventHandler(this.hidePageToolStripMenuItem_Click);
            // 
            // deleteStripMenuItem
            // 
            this.deleteStripMenuItem.Name = "deleteStripMenuItem";
            this.deleteStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.deleteStripMenuItem.Text = "Delete Code";
            this.deleteStripMenuItem.Click += new System.EventHandler(this.deleteStripMenuItem_Click);
            // 
            // toolStripMenuOpenWorkspace
            // 
            this.toolStripMenuOpenWorkspace.Name = "toolStripMenuOpenWorkspace";
            this.toolStripMenuOpenWorkspace.Size = new System.Drawing.Size(164, 22);
            this.toolStripMenuOpenWorkspace.Text = "Open Workspace";
            this.toolStripMenuOpenWorkspace.Click += new System.EventHandler(this.toolStripMenuOpenWorkspace_Click);
            // 
            // renameCodeToolStripMenuItem
            // 
            this.renameCodeToolStripMenuItem.Name = "renameCodeToolStripMenuItem";
            this.renameCodeToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.renameCodeToolStripMenuItem.Text = "Rename Code";
            this.renameCodeToolStripMenuItem.Click += new System.EventHandler(this.renameCodeToolStripMenuItem_Click);
            // 
            // renamePageToolStripMenuItem
            // 
            this.renamePageToolStripMenuItem.Name = "renamePageToolStripMenuItem";
            this.renamePageToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.renamePageToolStripMenuItem.Text = "Rename Page";
            // 
            // toolStripMenuIncludeCode
            // 
            this.toolStripMenuIncludeCode.Name = "toolStripMenuIncludeCode";
            this.toolStripMenuIncludeCode.Size = new System.Drawing.Size(164, 22);
            this.toolStripMenuIncludeCode.Text = "Include Code";
            this.toolStripMenuIncludeCode.Click += new System.EventHandler(this.toolStripMenuIncludeCode_Click);
            // 
            // dataGridViewBuildOutput
            // 
            this.dataGridViewBuildOutput.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewBuildOutput.Name = "dataGridViewBuildOutput";
            this.dataGridViewBuildOutput.Size = new System.Drawing.Size(240, 150);
            this.dataGridViewBuildOutput.TabIndex = 0;
            // 
            // dataGridViewFindReplace
            // 
            this.dataGridViewFindReplace.BackgroundColor = System.Drawing.Color.White;
            this.dataGridViewFindReplace.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridViewFindReplace.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewFindReplace.Location = new System.Drawing.Point(1588, 54);
            this.dataGridViewFindReplace.Name = "dataGridViewFindReplace";
            this.dataGridViewFindReplace.Size = new System.Drawing.Size(325, 147);
            this.dataGridViewFindReplace.TabIndex = 5;
            // 
            // FormEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1916, 838);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FormEditor";
            this.Text = "AmiumCode C#";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormEditor_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormEditor_FormClosed);
            this.ResizeBegin += new System.EventHandler(this.FormEditor_ResizeBegin);
            this.Move += new System.EventHandler(this.FormEditor_Move);
            this.Resize += new System.EventHandler(this.FormEditor_Resize);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tblViewMethodes.ResumeLayout(false);
            this.tblViewMethodes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewMethodes)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.panelFunctions.ResumeLayout(false);
            this.TablePanelOutputs.ResumeLayout(false);
            this.panelOutputs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl)).EndInit();
            this.panelControl.ResumeLayout(false);
            this.panelStatus.ResumeLayout(false);
            this.contextMenuTreeView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewBuildOutput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFindReplace)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        public TreeView ProjectTree;
        public ScintillaNET.Scintilla Editor;
        private ImageList imageList1;
        private Panel panelFunctions;
        private Button btnFormat;
        private Button btnParagraph;
        private Button btnSnippets;
        public Button btnFind;
        public Button btnFindReplace;
        private ToolTip toolTip1;
        private SplitContainer splitContainer1;
        private TableLayoutPanel tableLayoutPanel2;
        private SplitContainer splitContainer2;

        private DevExpress.XtraEditors.PanelControl panelControl;
        private Button btnRebuild;
        private Button btnSave;
        private Button btnReload;
        private Button btnToggleTheme;
        private ContextMenuStrip contextMenuTreeView;
        private ToolStripMenuItem addPageBeforeToolStripMenuItem;
        private ToolStripMenuItem addPageAfterToolStripMenuItem;
        private ToolStripMenuItem addSubCodeToolStripMenuItem;
        private ToolStripMenuItem customToolStripMenuItem;
        private ToolStripMenuItem uDLClientToolStripMenuItem;
        private ToolStripMenuItem aKClientToolStripMenuItem;
        private ToolStripMenuItem aKServerToolStripMenuItem;
        private ToolStripMenuItem streamClientToolStripMenuItem;
        private ToolStripMenuItem hidePageToolStripMenuItem;
        private ToolStripMenuItem deleteStripMenuItem;
        private ToolStripMenuItem toolStripMenuOpenWorkspace;
        private Button btnRun;
        private ToolStripMenuItem renameCodeToolStripMenuItem;
        private ToolStripMenuItem renamePageToolStripMenuItem;
        private TableLayoutPanel TablePanelOutputs;
        private ScintillaVerticalBar vBarEditor;
        private Panel panelOutputs;
        private Button btnBuildOutput;
        private Button btnEditorOutput;
        private SplitContainer splitContainer3;
        private TableLayoutPanel tblViewMethodes;
        private DataGridView gridViewMethodes;

        private GridViewVerticalBar vBarMethodes;
        private Label lblMethodes;
        private TableLayoutPanel tableLayoutPanel4;
        private Panel panelStatus;
        private Label labelStatus;
        private TreeViewVerticalBar vBarProjectTree;
        private Panel panelSplttter1;
        private Panel panelSplitter2;
        private Panel panelSplitter3;
        private Panel panelSplitter4;
        public Panel PanelTabs;
        private ToolStripMenuItem toolStripMenuIncludeCode;
        private Button btnShowHidden;
        private Button btnShowFindReplaceOutput;
        private GridViewVerticalBar vBarOutputs;
        private ScintillaHorizontalBar hBarEditor;
        private DataGridView dataGridViewFindReplace;
    }
}