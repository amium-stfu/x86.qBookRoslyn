using qbook.ScintillaEditor.InputControls;

namespace qbook.ScintillaEditor
{
    partial class FormScintillaEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormScintillaEditor));
            this.panelSplitter4 = new System.Windows.Forms.Panel();
            this.btnShowFindReplaceOutput = new System.Windows.Forms.Button();
            this.panelStatus = new System.Windows.Forms.Panel();
            this.labelStatus = new System.Windows.Forms.Label();
            this.BookTreeViewIcons = new System.Windows.Forms.ImageList(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnParagraph = new System.Windows.Forms.Button();
            this.btnSnippets = new System.Windows.Forms.Button();
            this.btnFindReplace = new System.Windows.Forms.Button();
            this.btnFind = new System.Windows.Forms.Button();
            this.btnFormat = new System.Windows.Forms.Button();
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
            this.deletePageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnBuildOutput = new System.Windows.Forms.Button();
            this.dataGridViewFindReplace = new System.Windows.Forms.DataGridView();
            this.btnEditorOutput = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.ProjectTree = new qbook.ScintillaEditor.CustomTreeView();
            this.vBarProjectTree = new qbook.CodeEditor.TreeViewVerticalBar();
            this.panelSplttter1 = new System.Windows.Forms.Panel();
            this.tblViewMethodes = new System.Windows.Forms.TableLayoutPanel();
            this.gridViewMethodes = new System.Windows.Forms.DataGridView();
            this.vBarMethodes = new qbook.CodeEditor.GridViewVerticalBar();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tbMethodeFilter = new System.Windows.Forms.TextBox();
            this.lblMethodes = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.EditorLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.panelFunctions = new System.Windows.Forms.Panel();
            this.btnShowHidden = new System.Windows.Forms.Button();
            this.vBarEditor = new qbook.CodeEditor.ScintillaVerticalBar();
            this.hBarEditor = new qbook.CodeEditor.ScintillaHorizontalBar();
            this.panelSplitter3 = new System.Windows.Forms.Panel();
            this.panelEditor = new System.Windows.Forms.Panel();
            this.PanelTabs = new qbook.ScintillaEditor.DoubleBufferedPanel();
            this.TablePanelOutputs = new System.Windows.Forms.TableLayoutPanel();
            this.panelOutputs = new System.Windows.Forms.Panel();
            this.panelSplitter2 = new System.Windows.Forms.Panel();
            this.vBarOutputs = new qbook.CodeEditor.GridViewVerticalBar();
            this.panelOutput = new System.Windows.Forms.Panel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPageData = new System.Windows.Forms.FlowLayoutPanel();
            this.panelControl = new System.Windows.Forms.Panel();
            this.btnToggleTheme = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnReload = new System.Windows.Forms.Button();
            this.btnRebuild = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.dataGridViewBuildOutput = new System.Windows.Forms.DataGridView();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.panelStatus.SuspendLayout();
            this.contextMenuTreeView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFindReplace)).BeginInit();
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
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.EditorLayoutPanel.SuspendLayout();
            this.panelFunctions.SuspendLayout();
            this.TablePanelOutputs.SuspendLayout();
            this.panelOutputs.SuspendLayout();
            this.panelOutput.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.panelControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewBuildOutput)).BeginInit();
            this.SuspendLayout();
            // 
            // panelSplitter4
            // 
            this.panelSplitter4.BackColor = System.Drawing.Color.Silver;
            this.panelSplitter4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSplitter4.Location = new System.Drawing.Point(0, 4);
            this.panelSplitter4.Margin = new System.Windows.Forms.Padding(0, 0, 44, 0);
            this.panelSplitter4.Name = "panelSplitter4";
            this.TablePanelOutputs.SetRowSpan(this.panelSplitter4, 2);
            this.panelSplitter4.Size = new System.Drawing.Size(2, 134);
            this.panelSplitter4.TabIndex = 4;
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
            this.btnShowFindReplaceOutput.Click += new System.EventHandler(this.btnShowFindReplaceOutput_Click);
            // 
            // panelStatus
            // 
            this.panelStatus.Controls.Add(this.labelStatus);
            this.panelStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelStatus.Location = new System.Drawing.Point(3, 645);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Size = new System.Drawing.Size(1610, 50);
            this.panelStatus.TabIndex = 6;
            // 
            // labelStatus
            // 
            this.labelStatus.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(0, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(390, 50);
            this.labelStatus.TabIndex = 0;
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // BookTreeViewIcons
            // 
            this.BookTreeViewIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("BookTreeViewIcons.ImageStream")));
            this.BookTreeViewIcons.TransparentColor = System.Drawing.Color.Transparent;
            this.BookTreeViewIcons.Images.SetKeyName(0, "play_red_button_icon_227850.png");
            this.BookTreeViewIcons.Images.SetKeyName(1, "book_48p.png");
            this.BookTreeViewIcons.Images.SetKeyName(2, "Page_48p.png");
            this.BookTreeViewIcons.Images.SetKeyName(3, "C#.png");
            this.BookTreeViewIcons.Images.SetKeyName(4, "ic_radio_button_off_48p.png");
            this.BookTreeViewIcons.Images.SetKeyName(5, "view_hide_icon_124813.png");
            this.BookTreeViewIcons.Images.SetKeyName(6, "CodeOff_Light.png");
            this.BookTreeViewIcons.Images.SetKeyName(7, "foldergrey_93178.png");
            this.BookTreeViewIcons.Images.SetKeyName(8, "folderorangeopen_93000.png");
            this.BookTreeViewIcons.Images.SetKeyName(9, "forbidden_icon_242335.png");
            this.BookTreeViewIcons.Images.SetKeyName(10, "vcsconflicting_48p.png");
            this.BookTreeViewIcons.Images.SetKeyName(11, "hidden_eye_40p_black.png");
            this.BookTreeViewIcons.Images.SetKeyName(12, "find_magnifying_glass_icon_176383.png");
            // 
            // btnParagraph
            // 
            this.btnParagraph.BackColor = System.Drawing.Color.IndianRed;
            this.btnParagraph.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnParagraph.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnParagraph.FlatAppearance.BorderSize = 2;
            this.btnParagraph.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnParagraph.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnParagraph.Image = ((System.Drawing.Image)(resources.GetObject("btnParagraph.Image")));
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
            this.btnSnippets.Image = ((System.Drawing.Image)(resources.GetObject("btnSnippets.Image")));
            this.btnSnippets.Location = new System.Drawing.Point(0, 117);
            this.btnSnippets.Name = "btnSnippets";
            this.btnSnippets.Size = new System.Drawing.Size(40, 39);
            this.btnSnippets.TabIndex = 0;
            this.toolTip1.SetToolTip(this.btnSnippets, "Snippets");
            this.btnSnippets.UseVisualStyleBackColor = false;
            // 
            // btnFindReplace
            // 
            this.btnFindReplace.BackColor = System.Drawing.Color.IndianRed;
            this.btnFindReplace.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnFindReplace.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnFindReplace.FlatAppearance.BorderSize = 2;
            this.btnFindReplace.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFindReplace.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnFindReplace.Image = ((System.Drawing.Image)(resources.GetObject("btnFindReplace.Image")));
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
            this.btnFind.Image = ((System.Drawing.Image)(resources.GetObject("btnFind.Image")));
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
            this.btnFormat.Image = ((System.Drawing.Image)(resources.GetObject("btnFormat.Image")));
            this.btnFormat.Location = new System.Drawing.Point(0, 0);
            this.btnFormat.Name = "btnFormat";
            this.btnFormat.Size = new System.Drawing.Size(40, 39);
            this.btnFormat.TabIndex = 0;
            this.toolTip1.SetToolTip(this.btnFormat, "Format Code (CTRL -> K -> F)");
            this.btnFormat.UseVisualStyleBackColor = false;
            this.btnFormat.Click += new System.EventHandler(this.btnFormat_Click);
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
            this.toolStripMenuIncludeCode,
            this.deletePageToolStripMenuItem});
            this.contextMenuTreeView.Name = "contextMenuTreeView";
            this.contextMenuTreeView.Size = new System.Drawing.Size(165, 224);
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
            this.renamePageToolStripMenuItem.Click += new System.EventHandler(this.renamePageToolStripMenuItem_Click);
            // 
            // toolStripMenuIncludeCode
            // 
            this.toolStripMenuIncludeCode.Name = "toolStripMenuIncludeCode";
            this.toolStripMenuIncludeCode.Size = new System.Drawing.Size(164, 22);
            this.toolStripMenuIncludeCode.Text = "Include Code";
            this.toolStripMenuIncludeCode.Click += new System.EventHandler(this.toolStripMenuIncludeCode_Click);
            // 
            // deletePageToolStripMenuItem
            // 
            this.deletePageToolStripMenuItem.Name = "deletePageToolStripMenuItem";
            this.deletePageToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.deletePageToolStripMenuItem.Text = "Delete Page";
            this.deletePageToolStripMenuItem.Click += new System.EventHandler(this.deletePageToolStripMenuItem_Click);
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
            // dataGridViewFindReplace
            // 
            this.dataGridViewFindReplace.BackgroundColor = System.Drawing.Color.White;
            this.dataGridViewFindReplace.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridViewFindReplace.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewFindReplace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewFindReplace.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewFindReplace.Name = "dataGridViewFindReplace";
            this.dataGridViewFindReplace.Size = new System.Drawing.Size(1156, 109);
            this.dataGridViewFindReplace.TabIndex = 8;
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
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panelStatus, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 78F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1616, 698);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 93);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1610, 546);
            this.splitContainer1.SplitterDistance = 382;
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
            this.splitContainer3.Size = new System.Drawing.Size(382, 546);
            this.splitContainer3.SplitterDistance = 282;
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
            this.tableLayoutPanel4.Size = new System.Drawing.Size(382, 282);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // ProjectTree
            // 
            this.ProjectTree.AllowDrop = true;
            this.ProjectTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProjectTree.InsertLineY = -1;
            this.ProjectTree.Location = new System.Drawing.Point(3, 3);
            this.ProjectTree.Name = "ProjectTree";
            this.ProjectTree.Scrollable = false;
            this.ProjectTree.ShowLines = false;
            this.ProjectTree.Size = new System.Drawing.Size(356, 274);
            this.ProjectTree.TabIndex = 0;
            this.ProjectTree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.ProjectTree_NodeMouseClick);
            this.ProjectTree.DragDrop += new System.Windows.Forms.DragEventHandler(this.ProjectTree_DragDrop);
            // 
            // vBarProjectTree
            // 
            this.vBarProjectTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vBarProjectTree.Location = new System.Drawing.Point(365, 3);
            this.vBarProjectTree.Name = "vBarProjectTree";
            this.vBarProjectTree.SetBackColor = System.Drawing.Color.LightGray;
            this.vBarProjectTree.SetForeColor = System.Drawing.Color.DodgerBlue;
            this.vBarProjectTree.Size = new System.Drawing.Size(14, 274);
            this.vBarProjectTree.TabIndex = 1;
            // 
            // panelSplttter1
            // 
            this.panelSplttter1.BackColor = System.Drawing.Color.DarkGray;
            this.tableLayoutPanel4.SetColumnSpan(this.panelSplttter1, 2);
            this.panelSplttter1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSplttter1.Location = new System.Drawing.Point(0, 280);
            this.panelSplttter1.Margin = new System.Windows.Forms.Padding(0);
            this.panelSplttter1.Name = "panelSplttter1";
            this.panelSplttter1.Size = new System.Drawing.Size(382, 2);
            this.panelSplttter1.TabIndex = 2;
            // 
            // tblViewMethodes
            // 
            this.tblViewMethodes.ColumnCount = 2;
            this.tblViewMethodes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblViewMethodes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblViewMethodes.Controls.Add(this.gridViewMethodes, 0, 1);
            this.tblViewMethodes.Controls.Add(this.vBarMethodes, 1, 1);
            this.tblViewMethodes.Controls.Add(this.panel1, 0, 0);
            this.tblViewMethodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblViewMethodes.Location = new System.Drawing.Point(0, 0);
            this.tblViewMethodes.Name = "tblViewMethodes";
            this.tblViewMethodes.RowCount = 2;
            this.tblViewMethodes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tblViewMethodes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblViewMethodes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblViewMethodes.Size = new System.Drawing.Size(382, 258);
            this.tblViewMethodes.TabIndex = 0;
            // 
            // gridViewMethodes
            // 
            this.gridViewMethodes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridViewMethodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridViewMethodes.Location = new System.Drawing.Point(30, 25);
            this.gridViewMethodes.Margin = new System.Windows.Forms.Padding(30, 3, 0, 0);
            this.gridViewMethodes.Name = "gridViewMethodes";
            this.gridViewMethodes.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridViewMethodes.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.gridViewMethodes.Size = new System.Drawing.Size(332, 233);
            this.gridViewMethodes.TabIndex = 0;
            // 
            // vBarMethodes
            // 
            this.vBarMethodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vBarMethodes.Location = new System.Drawing.Point(362, 22);
            this.vBarMethodes.Margin = new System.Windows.Forms.Padding(0);
            this.vBarMethodes.Name = "vBarMethodes";
            this.vBarMethodes.SetBackColor = System.Drawing.Color.LightGray;
            this.vBarMethodes.SetForeColor = System.Drawing.Color.DodgerBlue;
            this.vBarMethodes.Size = new System.Drawing.Size(20, 236);
            this.vBarMethodes.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tbMethodeFilter);
            this.panel1.Controls.Add(this.lblMethodes);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(362, 22);
            this.panel1.TabIndex = 3;
            // 
            // tbMethodeFilter
            // 
            this.tbMethodeFilter.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbMethodeFilter.Dock = System.Windows.Forms.DockStyle.Right;
            this.tbMethodeFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbMethodeFilter.Location = new System.Drawing.Point(156, 0);
            this.tbMethodeFilter.Multiline = true;
            this.tbMethodeFilter.Name = "tbMethodeFilter";
            this.tbMethodeFilter.Size = new System.Drawing.Size(206, 22);
            this.tbMethodeFilter.TabIndex = 4;
            this.tbMethodeFilter.TextChanged += new System.EventHandler(this.tbMethodeFilter_TextChanged);
            // 
            // lblMethodes
            // 
            this.lblMethodes.AutoSize = true;
            this.lblMethodes.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblMethodes.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMethodes.Location = new System.Drawing.Point(0, 0);
            this.lblMethodes.Margin = new System.Windows.Forms.Padding(0);
            this.lblMethodes.Name = "lblMethodes";
            this.lblMethodes.Padding = new System.Windows.Forms.Padding(30, 0, 0, 0);
            this.lblMethodes.Size = new System.Drawing.Size(79, 19);
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
            this.splitContainer2.Panel1.Controls.Add(this.EditorLayoutPanel);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.TablePanelOutputs);
            this.splitContainer2.Size = new System.Drawing.Size(1222, 546);
            this.splitContainer2.SplitterDistance = 406;
            this.splitContainer2.SplitterWidth = 2;
            this.splitContainer2.TabIndex = 0;
            // 
            // EditorLayoutPanel
            // 
            this.EditorLayoutPanel.ColumnCount = 5;
            this.EditorLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 2F));
            this.EditorLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.EditorLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.EditorLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.EditorLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.EditorLayoutPanel.Controls.Add(this.panelFunctions, 1, 1);
            this.EditorLayoutPanel.Controls.Add(this.vBarEditor, 3, 1);
            this.EditorLayoutPanel.Controls.Add(this.hBarEditor, 2, 2);
            this.EditorLayoutPanel.Controls.Add(this.panelSplitter3, 0, 1);
            this.EditorLayoutPanel.Controls.Add(this.panelEditor, 2, 1);
            this.EditorLayoutPanel.Controls.Add(this.PanelTabs, 2, 0);
            this.EditorLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EditorLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.EditorLayoutPanel.Name = "EditorLayoutPanel";
            this.EditorLayoutPanel.RowCount = 3;
            this.EditorLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.EditorLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.EditorLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.EditorLayoutPanel.Size = new System.Drawing.Size(1222, 406);
            this.EditorLayoutPanel.TabIndex = 0;
            this.EditorLayoutPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel2_Paint);
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
            this.panelFunctions.Location = new System.Drawing.Point(5, 39);
            this.panelFunctions.Name = "panelFunctions";
            this.panelFunctions.Size = new System.Drawing.Size(40, 344);
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
            this.btnShowHidden.Image = ((System.Drawing.Image)(resources.GetObject("btnShowHidden.Image")));
            this.btnShowHidden.Location = new System.Drawing.Point(0, 195);
            this.btnShowHidden.Margin = new System.Windows.Forms.Padding(9);
            this.btnShowHidden.Name = "btnShowHidden";
            this.btnShowHidden.Size = new System.Drawing.Size(40, 39);
            this.btnShowHidden.TabIndex = 0;
            this.btnShowHidden.UseVisualStyleBackColor = false;
            this.btnShowHidden.Click += new System.EventHandler(this.btnShowHidden_Click);
            // 
            // vBarEditor
            // 
            this.vBarEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vBarEditor.Location = new System.Drawing.Point(1182, 36);
            this.vBarEditor.Margin = new System.Windows.Forms.Padding(0);
            this.vBarEditor.Name = "vBarEditor";
            this.vBarEditor.SetBackColor = System.Drawing.Color.White;
            this.vBarEditor.SetForeColor = System.Drawing.Color.Black;
            this.vBarEditor.Size = new System.Drawing.Size(20, 350);
            this.vBarEditor.TabIndex = 3;
            // 
            // hBarEditor
            // 
            this.hBarEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hBarEditor.Location = new System.Drawing.Point(48, 386);
            this.hBarEditor.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.hBarEditor.Name = "hBarEditor";
            this.hBarEditor.SetBackColor = System.Drawing.Color.White;
            this.hBarEditor.SetForeColor = System.Drawing.Color.Black;
            this.hBarEditor.Size = new System.Drawing.Size(1134, 17);
            this.hBarEditor.TabIndex = 4;
            // 
            // panelSplitter3
            // 
            this.panelSplitter3.BackColor = System.Drawing.Color.Silver;
            this.panelSplitter3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSplitter3.Location = new System.Drawing.Point(0, 36);
            this.panelSplitter3.Margin = new System.Windows.Forms.Padding(0);
            this.panelSplitter3.Name = "panelSplitter3";
            this.EditorLayoutPanel.SetRowSpan(this.panelSplitter3, 2);
            this.panelSplitter3.Size = new System.Drawing.Size(2, 370);
            this.panelSplitter3.TabIndex = 5;
            // 
            // panelEditor
            // 
            this.panelEditor.BackColor = System.Drawing.Color.White;
            this.panelEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelEditor.Location = new System.Drawing.Point(51, 36);
            this.panelEditor.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.panelEditor.Name = "panelEditor";
            this.panelEditor.Size = new System.Drawing.Size(1128, 347);
            this.panelEditor.TabIndex = 7;
            // 
            // PanelTabs
            // 
            this.PanelTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelTabs.Location = new System.Drawing.Point(51, 0);
            this.PanelTabs.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.PanelTabs.Name = "PanelTabs";
            this.PanelTabs.Size = new System.Drawing.Size(1131, 36);
            this.PanelTabs.TabIndex = 8;
            this.PanelTabs.Resize += new System.EventHandler(this.PanelTabs_Resize);
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
            this.TablePanelOutputs.Controls.Add(this.panelOutput, 1, 2);
            this.TablePanelOutputs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TablePanelOutputs.Location = new System.Drawing.Point(0, 0);
            this.TablePanelOutputs.Name = "TablePanelOutputs";
            this.TablePanelOutputs.RowCount = 3;
            this.TablePanelOutputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 4F));
            this.TablePanelOutputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.TablePanelOutputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TablePanelOutputs.Size = new System.Drawing.Size(1222, 138);
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
            this.panelOutputs.Size = new System.Drawing.Size(1156, 25);
            this.panelOutputs.TabIndex = 2;
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
            this.panelSplitter2.Size = new System.Drawing.Size(1222, 2);
            this.panelSplitter2.TabIndex = 3;
            // 
            // vBarOutputs
            // 
            this.vBarOutputs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vBarOutputs.Location = new System.Drawing.Point(1205, 32);
            this.vBarOutputs.Name = "vBarOutputs";
            this.vBarOutputs.SetBackColor = System.Drawing.Color.LightGray;
            this.vBarOutputs.SetForeColor = System.Drawing.Color.DodgerBlue;
            this.vBarOutputs.Size = new System.Drawing.Size(14, 103);
            this.vBarOutputs.TabIndex = 6;
            // 
            // panelOutput
            // 
            this.panelOutput.Controls.Add(this.dataGridViewFindReplace);
            this.panelOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelOutput.Location = new System.Drawing.Point(46, 29);
            this.panelOutput.Margin = new System.Windows.Forms.Padding(0);
            this.panelOutput.Name = "panelOutput";
            this.panelOutput.Size = new System.Drawing.Size(1156, 109);
            this.panelOutput.TabIndex = 7;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 407F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.flowLayoutPageData, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.panelControl, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1610, 72);
            this.tableLayoutPanel3.TabIndex = 8;
            // 
            // flowLayoutPageData
            // 
            this.flowLayoutPageData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPageData.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPageData.Location = new System.Drawing.Point(407, 0);
            this.flowLayoutPageData.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPageData.Name = "flowLayoutPageData";
            this.flowLayoutPageData.Size = new System.Drawing.Size(1203, 72);
            this.flowLayoutPageData.TabIndex = 8;
            // 
            // panelControl
            // 
            this.panelControl.Controls.Add(this.btnToggleTheme);
            this.panelControl.Controls.Add(this.btnSave);
            this.panelControl.Controls.Add(this.btnReload);
            this.panelControl.Controls.Add(this.btnRebuild);
            this.panelControl.Controls.Add(this.btnRun);
            this.panelControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl.Location = new System.Drawing.Point(0, 0);
            this.panelControl.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.panelControl.Name = "panelControl";
            this.panelControl.Size = new System.Drawing.Size(405, 72);
            this.panelControl.TabIndex = 7;
            // 
            // btnToggleTheme
            // 
            this.btnToggleTheme.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnToggleTheme.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnToggleTheme.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleTheme.Image = ((System.Drawing.Image)(resources.GetObject("btnToggleTheme.Image")));
            this.btnToggleTheme.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnToggleTheme.Location = new System.Drawing.Point(314, 0);
            this.btnToggleTheme.Name = "btnToggleTheme";
            this.btnToggleTheme.Size = new System.Drawing.Size(91, 72);
            this.btnToggleTheme.TabIndex = 0;
            this.btnToggleTheme.Text = "Theme";
            this.btnToggleTheme.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnToggleTheme.UseVisualStyleBackColor = true;
            this.btnToggleTheme.Click += new System.EventHandler(this.btnToggleTheme_Click);
            // 
            // btnSave
            // 
            this.btnSave.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnSave.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
            this.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSave.Location = new System.Drawing.Point(240, 0);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(74, 72);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save\r\n";
            this.btnSave.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnReload
            // 
            this.btnReload.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnReload.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnReload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReload.Image = ((System.Drawing.Image)(resources.GetObject("btnReload.Image")));
            this.btnReload.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReload.Location = new System.Drawing.Point(159, 0);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(81, 72);
            this.btnReload.TabIndex = 3;
            this.btnReload.Text = "Reload";
            this.btnReload.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // btnRebuild
            // 
            this.btnRebuild.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnRebuild.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnRebuild.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRebuild.Image = ((System.Drawing.Image)(resources.GetObject("btnRebuild.Image")));
            this.btnRebuild.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRebuild.Location = new System.Drawing.Point(70, 0);
            this.btnRebuild.Name = "btnRebuild";
            this.btnRebuild.Size = new System.Drawing.Size(89, 72);
            this.btnRebuild.TabIndex = 1;
            this.btnRebuild.Text = "Rebuild";
            this.btnRebuild.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnRebuild.UseVisualStyleBackColor = true;
            this.btnRebuild.Click += new System.EventHandler(this.btnRebuild_Click);
            // 
            // btnRun
            // 
            this.btnRun.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnRun.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnRun.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRun.Image = ((System.Drawing.Image)(resources.GetObject("btnRun.Image")));
            this.btnRun.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRun.Location = new System.Drawing.Point(0, 0);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(70, 72);
            this.btnRun.TabIndex = 5;
            this.btnRun.Text = "Run";
            this.btnRun.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // dataGridViewBuildOutput
            // 
            this.dataGridViewBuildOutput.Location = new System.Drawing.Point(-556, 0);
            this.dataGridViewBuildOutput.Name = "dataGridViewBuildOutput";
            this.dataGridViewBuildOutput.Size = new System.Drawing.Size(240, 150);
            this.dataGridViewBuildOutput.TabIndex = 7;
            // 
            // FormScintillaEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1616, 698);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.dataGridViewBuildOutput);
            this.Name = "FormScintillaEditor";
            this.Text = "FormScintillaEditor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormScintillaEditor_FormClosing);
            this.panelStatus.ResumeLayout(false);
            this.contextMenuTreeView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFindReplace)).EndInit();
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
            ((System.ComponentModel.ISupportInitialize)(this.gridViewMethodes)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.EditorLayoutPanel.ResumeLayout(false);
            this.panelFunctions.ResumeLayout(false);
            this.TablePanelOutputs.ResumeLayout(false);
            this.panelOutputs.ResumeLayout(false);
            this.panelOutput.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.panelControl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewBuildOutput)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelSplitter4;
        private System.Windows.Forms.TableLayoutPanel TablePanelOutputs;
        private System.Windows.Forms.Panel panelOutputs;
        private System.Windows.Forms.Button btnShowFindReplaceOutput;
        private System.Windows.Forms.Button btnBuildOutput;
        private System.Windows.Forms.Button btnEditorOutput;
        private System.Windows.Forms.Panel panelSplitter2;
        private CodeEditor.GridViewVerticalBar vBarOutputs;
        private System.Windows.Forms.Button btnToggleTheme;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnRebuild;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Panel panelStatus;
        private System.Windows.Forms.Label labelStatus;
        public System.Windows.Forms.ImageList BookTreeViewIcons;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnParagraph;
        private System.Windows.Forms.Button btnSnippets;
        public System.Windows.Forms.Button btnFindReplace;
        public System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.Button btnFormat;
        private System.Windows.Forms.ContextMenuStrip contextMenuTreeView;
        private System.Windows.Forms.ToolStripMenuItem addPageBeforeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addPageAfterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addSubCodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem customToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uDLClientToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aKClientToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aKServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem streamClientToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hidePageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuOpenWorkspace;
        private System.Windows.Forms.ToolStripMenuItem renameCodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renamePageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuIncludeCode;
        private System.Windows.Forms.DataGridView dataGridViewFindReplace;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        public CustomTreeView ProjectTree;
        private CodeEditor.TreeViewVerticalBar vBarProjectTree;
        private System.Windows.Forms.Panel panelSplttter1;
        private System.Windows.Forms.TableLayoutPanel tblViewMethodes;
        private System.Windows.Forms.DataGridView gridViewMethodes;
        private CodeEditor.GridViewVerticalBar vBarMethodes;
        private System.Windows.Forms.Label lblMethodes;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TableLayoutPanel EditorLayoutPanel;
        private System.Windows.Forms.Panel panelFunctions;
        private System.Windows.Forms.Button btnShowHidden;
        private CodeEditor.ScintillaVerticalBar vBarEditor;
        private CodeEditor.ScintillaHorizontalBar hBarEditor;
        private System.Windows.Forms.Panel panelSplitter3;
        private System.Windows.Forms.DataGridView dataGridViewBuildOutput;
        private System.Windows.Forms.Panel panelEditor;
        private System.Windows.Forms.Panel panelOutput;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox tbMethodeFilter;
        private System.Windows.Forms.ToolStripMenuItem deletePageToolStripMenuItem;
        private System.Windows.Forms.Panel panelControl;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPageData;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private DoubleBufferedPanel PanelTabs;
    }
}