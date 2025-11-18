namespace qbook.ScintillaEditor
{
    partial class FormCodeEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCodeEditor));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.EditorLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.panelFunctions = new System.Windows.Forms.Panel();
            this.btnShowHidden = new System.Windows.Forms.Button();
            this.btnParagraph = new System.Windows.Forms.Button();
            this.btnSnippets = new System.Windows.Forms.Button();
            this.btnFindReplace = new System.Windows.Forms.Button();
            this.btnFind = new System.Windows.Forms.Button();
            this.btnFormat = new System.Windows.Forms.Button();
            this.panelSplitter3 = new System.Windows.Forms.Panel();
            this.panelEditor = new System.Windows.Forms.Panel();
            this.TablePanelOutputs = new System.Windows.Forms.TableLayoutPanel();
            this.panelOutputs = new System.Windows.Forms.Panel();
            this.btnShowFindReplaceOutput = new System.Windows.Forms.Button();
            this.btnBuildOutput = new System.Windows.Forms.Button();
            this.btnEditorOutput = new System.Windows.Forms.Button();
            this.panelSplitter2 = new System.Windows.Forms.Panel();
            this.panelSplitter4 = new System.Windows.Forms.Panel();
            this.panelOutput = new System.Windows.Forms.Panel();
            this.dataGridViewFindReplace = new System.Windows.Forms.DataGridView();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.ContainerLeft = new System.Windows.Forms.SplitContainer();
            this.tblViewMethodes = new System.Windows.Forms.TableLayoutPanel();
            this.gridViewMethodes = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tbMethodeFilter = new System.Windows.Forms.TextBox();
            this.lblMethodes = new System.Windows.Forms.Label();
            this.vBarMethodes = new qbook.CodeEditor.GridViewVerticalBar();
            this.vBarEditor = new qbook.CodeEditor.ScintillaVerticalBar();
            this.hBarEditor = new qbook.CodeEditor.ScintillaHorizontalBar();
            this.PanelTabs = new qbook.ScintillaEditor.DoubleBufferedPanel();
            this.vBarOutputs = new qbook.CodeEditor.GridViewVerticalBar();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.EditorLayoutPanel.SuspendLayout();
            this.panelFunctions.SuspendLayout();
            this.TablePanelOutputs.SuspendLayout();
            this.panelOutputs.SuspendLayout();
            this.panelOutput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFindReplace)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ContainerLeft)).BeginInit();
            this.ContainerLeft.Panel2.SuspendLayout();
            this.ContainerLeft.SuspendLayout();
            this.tblViewMethodes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewMethodes)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.EditorLayoutPanel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.TablePanelOutputs);
            this.splitContainer1.Size = new System.Drawing.Size(1150, 603);
            this.splitContainer1.SplitterDistance = 464;
            this.splitContainer1.TabIndex = 0;
            // 
            // EditorLayoutPanel
            // 
            this.EditorLayoutPanel.BackColor = System.Drawing.SystemColors.ActiveCaption;
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
            this.EditorLayoutPanel.Size = new System.Drawing.Size(1150, 464);
            this.EditorLayoutPanel.TabIndex = 1;
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
            this.panelFunctions.Size = new System.Drawing.Size(40, 402);
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
            this.btnFormat.UseVisualStyleBackColor = false;
            this.btnFormat.Click += new System.EventHandler(this.btnFormat_Click);
            // 
            // panelSplitter3
            // 
            this.panelSplitter3.BackColor = System.Drawing.Color.Silver;
            this.panelSplitter3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSplitter3.Location = new System.Drawing.Point(0, 36);
            this.panelSplitter3.Margin = new System.Windows.Forms.Padding(0);
            this.panelSplitter3.Name = "panelSplitter3";
            this.EditorLayoutPanel.SetRowSpan(this.panelSplitter3, 2);
            this.panelSplitter3.Size = new System.Drawing.Size(2, 428);
            this.panelSplitter3.TabIndex = 5;
            // 
            // panelEditor
            // 
            this.panelEditor.BackColor = System.Drawing.Color.White;
            this.panelEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelEditor.Location = new System.Drawing.Point(51, 36);
            this.panelEditor.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.panelEditor.Name = "panelEditor";
            this.panelEditor.Size = new System.Drawing.Size(1056, 405);
            this.panelEditor.TabIndex = 7;
            this.panelEditor.FontChanged += new System.EventHandler(this.panelEditor_FontChanged);
            this.panelEditor.SizeChanged += new System.EventHandler(this.panelEditor_SizeChanged);
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
            this.TablePanelOutputs.Size = new System.Drawing.Size(1150, 135);
            this.TablePanelOutputs.TabIndex = 2;
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
            this.panelOutputs.Size = new System.Drawing.Size(1084, 25);
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
            this.btnShowFindReplaceOutput.Click += new System.EventHandler(this.btnShowFindReplaceOutput_Click);
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
            this.panelSplitter2.Size = new System.Drawing.Size(1150, 2);
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
            this.panelSplitter4.Size = new System.Drawing.Size(2, 131);
            this.panelSplitter4.TabIndex = 4;
            // 
            // panelOutput
            // 
            this.panelOutput.Controls.Add(this.dataGridViewFindReplace);
            this.panelOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelOutput.Location = new System.Drawing.Point(46, 29);
            this.panelOutput.Margin = new System.Windows.Forms.Padding(0, 0, 0, 20);
            this.panelOutput.Name = "panelOutput";
            this.panelOutput.Size = new System.Drawing.Size(1084, 86);
            this.panelOutput.TabIndex = 7;
            this.panelOutput.Paint += new System.Windows.Forms.PaintEventHandler(this.panelOutput_Paint);
            // 
            // dataGridViewFindReplace
            // 
            this.dataGridViewFindReplace.BackgroundColor = System.Drawing.Color.White;
            this.dataGridViewFindReplace.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewFindReplace.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewFindReplace.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewFindReplace.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewFindReplace.Location = new System.Drawing.Point(366, 27);
            this.dataGridViewFindReplace.Name = "dataGridViewFindReplace";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewFindReplace.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridViewFindReplace.Size = new System.Drawing.Size(441, 109);
            this.dataGridViewFindReplace.TabIndex = 8;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.ContainerLeft);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer1);
            this.splitContainer2.Size = new System.Drawing.Size(1475, 603);
            this.splitContainer2.SplitterDistance = 321;
            this.splitContainer2.TabIndex = 1;
            // 
            // ContainerLeft
            // 
            this.ContainerLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ContainerLeft.Location = new System.Drawing.Point(0, 0);
            this.ContainerLeft.Name = "ContainerLeft";
            this.ContainerLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // ContainerLeft.Panel2
            // 
            this.ContainerLeft.Panel2.Controls.Add(this.tblViewMethodes);
            this.ContainerLeft.Size = new System.Drawing.Size(321, 603);
            this.ContainerLeft.SplitterDistance = 247;
            this.ContainerLeft.TabIndex = 0;
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
            this.tblViewMethodes.Size = new System.Drawing.Size(321, 352);
            this.tblViewMethodes.TabIndex = 1;
            // 
            // gridViewMethodes
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridViewMethodes.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.gridViewMethodes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridViewMethodes.DefaultCellStyle = dataGridViewCellStyle5;
            this.gridViewMethodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridViewMethodes.Location = new System.Drawing.Point(30, 25);
            this.gridViewMethodes.Margin = new System.Windows.Forms.Padding(30, 3, 0, 20);
            this.gridViewMethodes.Name = "gridViewMethodes";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridViewMethodes.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.gridViewMethodes.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridViewMethodes.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.gridViewMethodes.Size = new System.Drawing.Size(271, 307);
            this.gridViewMethodes.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tbMethodeFilter);
            this.panel1.Controls.Add(this.lblMethodes);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(301, 22);
            this.panel1.TabIndex = 3;
            // 
            // tbMethodeFilter
            // 
            this.tbMethodeFilter.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbMethodeFilter.Dock = System.Windows.Forms.DockStyle.Right;
            this.tbMethodeFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbMethodeFilter.Location = new System.Drawing.Point(95, 0);
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
            this.lblMethodes.Size = new System.Drawing.Size(158, 19);
            this.lblMethodes.TabIndex = 3;
            this.lblMethodes.Text = "Methodes Classes:";
            this.lblMethodes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // vBarMethodes
            // 
            this.vBarMethodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vBarMethodes.Location = new System.Drawing.Point(301, 22);
            this.vBarMethodes.Margin = new System.Windows.Forms.Padding(0, 0, 0, 20);
            this.vBarMethodes.Name = "vBarMethodes";
            this.vBarMethodes.SetBackColor = System.Drawing.Color.LightGray;
            this.vBarMethodes.SetForeColor = System.Drawing.Color.DodgerBlue;
            this.vBarMethodes.Size = new System.Drawing.Size(20, 310);
            this.vBarMethodes.TabIndex = 2;
            // 
            // vBarEditor
            // 
            this.vBarEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vBarEditor.Location = new System.Drawing.Point(1110, 36);
            this.vBarEditor.Margin = new System.Windows.Forms.Padding(0);
            this.vBarEditor.Name = "vBarEditor";
            this.vBarEditor.SetBackColor = System.Drawing.Color.White;
            this.vBarEditor.SetForeColor = System.Drawing.Color.Black;
            this.vBarEditor.Size = new System.Drawing.Size(20, 408);
            this.vBarEditor.TabIndex = 3;
            // 
            // hBarEditor
            // 
            this.hBarEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hBarEditor.Location = new System.Drawing.Point(48, 444);
            this.hBarEditor.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.hBarEditor.Name = "hBarEditor";
            this.hBarEditor.SetBackColor = System.Drawing.Color.White;
            this.hBarEditor.SetForeColor = System.Drawing.Color.Black;
            this.hBarEditor.Size = new System.Drawing.Size(1062, 17);
            this.hBarEditor.TabIndex = 4;
            // 
            // PanelTabs
            // 
            this.PanelTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelTabs.Location = new System.Drawing.Point(51, 0);
            this.PanelTabs.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.PanelTabs.Name = "PanelTabs";
            this.PanelTabs.Size = new System.Drawing.Size(1059, 36);
            this.PanelTabs.TabIndex = 8;
            // 
            // vBarOutputs
            // 
            this.vBarOutputs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vBarOutputs.Location = new System.Drawing.Point(1133, 32);
            this.vBarOutputs.Name = "vBarOutputs";
            this.vBarOutputs.SetBackColor = System.Drawing.Color.LightGray;
            this.vBarOutputs.SetForeColor = System.Drawing.Color.DodgerBlue;
            this.vBarOutputs.Size = new System.Drawing.Size(14, 100);
            this.vBarOutputs.TabIndex = 6;
            // 
            // FormCodeEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1475, 603);
            this.Controls.Add(this.splitContainer2);
            this.Name = "FormCodeEditor";
            this.Text = "FormCodeEditor";
            this.Shown += new System.EventHandler(this.FormCodeEditor_Shown);
            this.ResizeEnd += new System.EventHandler(this.FormCodeEditor_ResizeEnd);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.EditorLayoutPanel.ResumeLayout(false);
            this.panelFunctions.ResumeLayout(false);
            this.TablePanelOutputs.ResumeLayout(false);
            this.panelOutputs.ResumeLayout(false);
            this.panelOutput.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFindReplace)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ContainerLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ContainerLeft)).EndInit();
            this.ContainerLeft.ResumeLayout(false);
            this.tblViewMethodes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridViewMethodes)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel TablePanelOutputs;
        private System.Windows.Forms.Panel panelOutputs;
        private System.Windows.Forms.Button btnShowFindReplaceOutput;
        private System.Windows.Forms.Button btnBuildOutput;
        private System.Windows.Forms.Button btnEditorOutput;
        private System.Windows.Forms.Panel panelSplitter2;
        private System.Windows.Forms.Panel panelSplitter4;
        private CodeEditor.GridViewVerticalBar vBarOutputs;
        private System.Windows.Forms.Panel panelOutput;
        private System.Windows.Forms.DataGridView dataGridViewFindReplace;
        private System.Windows.Forms.TableLayoutPanel EditorLayoutPanel;
        private System.Windows.Forms.Panel panelFunctions;
        private System.Windows.Forms.Button btnShowHidden;
        private System.Windows.Forms.Button btnParagraph;
        private System.Windows.Forms.Button btnSnippets;
        public System.Windows.Forms.Button btnFindReplace;
        public System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.Button btnFormat;
        private CodeEditor.ScintillaVerticalBar vBarEditor;
        private CodeEditor.ScintillaHorizontalBar hBarEditor;
        private System.Windows.Forms.Panel panelSplitter3;
        private System.Windows.Forms.Panel panelEditor;
        private DoubleBufferedPanel PanelTabs;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer ContainerLeft;
        private System.Windows.Forms.TableLayoutPanel tblViewMethodes;
        private System.Windows.Forms.DataGridView gridViewMethodes;
        private CodeEditor.GridViewVerticalBar vBarMethodes;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox tbMethodeFilter;
        private System.Windows.Forms.Label lblMethodes;
    }
}