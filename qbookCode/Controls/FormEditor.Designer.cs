namespace qbookCode.Controls
{
    partial class FormEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEditor));
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            splitContainer1 = new SplitContainer();
            EditorLayoutPanel = new TableLayoutPanel();
            panelFunctions = new Panel();
            btnShowHidden = new Button();
            btnParagraph = new Button();
            btnSnippets = new Button();
            btnFindReplace = new Button();
            btnFind = new Button();
            btnFormat = new Button();
            panelSplitter3 = new Panel();
            panelEditor = new Panel();
            TablePanelOutputs = new TableLayoutPanel();
            panelOutputs = new Panel();
            btnComLog = new Button();
            btnRuntime = new Button();
            btnEditorOutput = new Button();
            panelSplitter2 = new Panel();
            panelSplitter4 = new Panel();
            panelOutput = new Panel();
            splitContainer2 = new SplitContainer();
            ContainerLeft = new SplitContainer();
            tblViewMethodes = new TableLayoutPanel();
            gridViewMethodes = new DataGridView();
            panel1 = new Panel();
            tbMethodeFilter = new TextBox();
            lblMethodes = new Label();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            EditorLayoutPanel.SuspendLayout();
            panelFunctions.SuspendLayout();
            TablePanelOutputs.SuspendLayout();
            panelOutputs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ContainerLeft).BeginInit();
            ContainerLeft.Panel2.SuspendLayout();
            ContainerLeft.SuspendLayout();
            tblViewMethodes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridViewMethodes).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(EditorLayoutPanel);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(TablePanelOutputs);
            splitContainer1.Size = new Size(1342, 696);
            splitContainer1.SplitterDistance = 535;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            // 
            // EditorLayoutPanel
            // 
            EditorLayoutPanel.BackColor = SystemColors.ActiveCaption;
            EditorLayoutPanel.ColumnCount = 4;
            EditorLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 2F));
            EditorLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 54F));
            EditorLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            EditorLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 23F));
            EditorLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            EditorLayoutPanel.Controls.Add(panelFunctions, 1, 1);
            EditorLayoutPanel.Controls.Add(panelSplitter3, 0, 1);
            EditorLayoutPanel.Controls.Add(panelEditor, 2, 1);
            EditorLayoutPanel.Dock = DockStyle.Fill;
            EditorLayoutPanel.Location = new Point(0, 0);
            EditorLayoutPanel.Margin = new Padding(4, 3, 4, 3);
            EditorLayoutPanel.Name = "EditorLayoutPanel";
            EditorLayoutPanel.RowCount = 3;
            EditorLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            EditorLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            EditorLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));
            EditorLayoutPanel.Size = new Size(1342, 535);
            EditorLayoutPanel.TabIndex = 1;
            // 
            // panelFunctions
            // 
            panelFunctions.Controls.Add(btnShowHidden);
            panelFunctions.Controls.Add(btnParagraph);
            panelFunctions.Controls.Add(btnSnippets);
            panelFunctions.Controls.Add(btnFindReplace);
            panelFunctions.Controls.Add(btnFind);
            panelFunctions.Controls.Add(btnFormat);
            panelFunctions.Dock = DockStyle.Fill;
            panelFunctions.Location = new Point(6, 45);
            panelFunctions.Margin = new Padding(4, 3, 4, 3);
            panelFunctions.Name = "panelFunctions";
            panelFunctions.Size = new Size(46, 464);
            panelFunctions.TabIndex = 2;
            // 
            // btnShowHidden
            // 
            btnShowHidden.BackColor = Color.IndianRed;
            btnShowHidden.Dock = DockStyle.Top;
            btnShowHidden.FlatAppearance.BorderColor = Color.White;
            btnShowHidden.FlatAppearance.BorderSize = 2;
            btnShowHidden.FlatStyle = FlatStyle.Flat;
            btnShowHidden.Font = new Font("Segoe UI", 10F);
            btnShowHidden.Image = (Image)resources.GetObject("btnShowHidden.Image");
            btnShowHidden.Location = new Point(0, 225);
            btnShowHidden.Margin = new Padding(10);
            btnShowHidden.Name = "btnShowHidden";
            btnShowHidden.Size = new Size(46, 45);
            btnShowHidden.TabIndex = 0;
            btnShowHidden.UseVisualStyleBackColor = false;
            btnShowHidden.Click += btnShowHidden_Click;
            // 
            // btnParagraph
            // 
            btnParagraph.BackColor = Color.IndianRed;
            btnParagraph.Dock = DockStyle.Top;
            btnParagraph.FlatAppearance.BorderColor = Color.White;
            btnParagraph.FlatAppearance.BorderSize = 2;
            btnParagraph.FlatStyle = FlatStyle.Flat;
            btnParagraph.Font = new Font("Segoe UI", 10F);
            btnParagraph.Image = (Image)resources.GetObject("btnParagraph.Image");
            btnParagraph.Location = new Point(0, 180);
            btnParagraph.Margin = new Padding(10);
            btnParagraph.Name = "btnParagraph";
            btnParagraph.Size = new Size(46, 45);
            btnParagraph.TabIndex = 0;
            btnParagraph.UseVisualStyleBackColor = false;
            btnParagraph.Click += btnParagraph_Click;
            // 
            // btnSnippets
            // 
            btnSnippets.BackColor = Color.IndianRed;
            btnSnippets.Dock = DockStyle.Top;
            btnSnippets.FlatAppearance.BorderColor = Color.White;
            btnSnippets.FlatAppearance.BorderSize = 2;
            btnSnippets.FlatStyle = FlatStyle.Flat;
            btnSnippets.Font = new Font("Segoe UI", 10F);
            btnSnippets.Image = (Image)resources.GetObject("btnSnippets.Image");
            btnSnippets.Location = new Point(0, 135);
            btnSnippets.Margin = new Padding(4, 3, 4, 3);
            btnSnippets.Name = "btnSnippets";
            btnSnippets.Size = new Size(46, 45);
            btnSnippets.TabIndex = 0;
            btnSnippets.UseVisualStyleBackColor = false;
            // 
            // btnFindReplace
            // 
            btnFindReplace.BackColor = Color.IndianRed;
            btnFindReplace.Dock = DockStyle.Top;
            btnFindReplace.FlatAppearance.BorderColor = Color.White;
            btnFindReplace.FlatAppearance.BorderSize = 2;
            btnFindReplace.FlatStyle = FlatStyle.Flat;
            btnFindReplace.Font = new Font("Segoe UI", 10F);
            btnFindReplace.Image = (Image)resources.GetObject("btnFindReplace.Image");
            btnFindReplace.Location = new Point(0, 90);
            btnFindReplace.Margin = new Padding(4, 3, 4, 3);
            btnFindReplace.Name = "btnFindReplace";
            btnFindReplace.Size = new Size(46, 45);
            btnFindReplace.TabIndex = 0;
            btnFindReplace.UseVisualStyleBackColor = false;
            btnFindReplace.Click += btnFindReplace_Click;
            // 
            // btnFind
            // 
            btnFind.BackColor = Color.IndianRed;
            btnFind.Dock = DockStyle.Top;
            btnFind.FlatAppearance.BorderColor = Color.White;
            btnFind.FlatAppearance.BorderSize = 2;
            btnFind.FlatStyle = FlatStyle.Flat;
            btnFind.Font = new Font("Segoe UI", 10F);
            btnFind.Image = (Image)resources.GetObject("btnFind.Image");
            btnFind.Location = new Point(0, 45);
            btnFind.Margin = new Padding(4, 3, 4, 3);
            btnFind.Name = "btnFind";
            btnFind.Size = new Size(46, 45);
            btnFind.TabIndex = 0;
            btnFind.UseVisualStyleBackColor = false;
            btnFind.Click += btnFind_Click;
            // 
            // btnFormat
            // 
            btnFormat.BackColor = Color.IndianRed;
            btnFormat.Dock = DockStyle.Top;
            btnFormat.FlatAppearance.BorderColor = Color.White;
            btnFormat.FlatAppearance.BorderSize = 2;
            btnFormat.FlatStyle = FlatStyle.Flat;
            btnFormat.Font = new Font("Segoe UI", 10F);
            btnFormat.Image = (Image)resources.GetObject("btnFormat.Image");
            btnFormat.Location = new Point(0, 0);
            btnFormat.Margin = new Padding(4, 3, 4, 3);
            btnFormat.Name = "btnFormat";
            btnFormat.Size = new Size(46, 45);
            btnFormat.TabIndex = 0;
            btnFormat.UseVisualStyleBackColor = false;
            btnFormat.Click += btnFormat_Click;
            // 
            // panelSplitter3
            // 
            panelSplitter3.BackColor = Color.Silver;
            panelSplitter3.Dock = DockStyle.Fill;
            panelSplitter3.Location = new Point(0, 42);
            panelSplitter3.Margin = new Padding(0);
            panelSplitter3.Name = "panelSplitter3";
            EditorLayoutPanel.SetRowSpan(panelSplitter3, 2);
            panelSplitter3.Size = new Size(2, 493);
            panelSplitter3.TabIndex = 5;
            // 
            // panelEditor
            // 
            panelEditor.BackColor = Color.White;
            panelEditor.Dock = DockStyle.Fill;
            panelEditor.Location = new Point(56, 42);
            panelEditor.Margin = new Padding(0);
            panelEditor.Name = "panelEditor";
            panelEditor.Size = new Size(1263, 470);
            panelEditor.TabIndex = 7;
            panelEditor.FontChanged += panelEditor_FontChanged;
            panelEditor.SizeChanged += panelEditor_SizeChanged;
            // 
            // TablePanelOutputs
            // 
            TablePanelOutputs.ColumnCount = 3;
            TablePanelOutputs.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 54F));
            TablePanelOutputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            TablePanelOutputs.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 23F));
            TablePanelOutputs.Controls.Add(panelOutputs, 1, 1);
            TablePanelOutputs.Controls.Add(panelSplitter2, 0, 0);
            TablePanelOutputs.Controls.Add(panelSplitter4, 0, 1);
            TablePanelOutputs.Controls.Add(panelOutput, 1, 2);
            TablePanelOutputs.Dock = DockStyle.Fill;
            TablePanelOutputs.Location = new Point(0, 0);
            TablePanelOutputs.Margin = new Padding(4, 3, 4, 3);
            TablePanelOutputs.Name = "TablePanelOutputs";
            TablePanelOutputs.Padding = new Padding(0, 0, 0, 15);
            TablePanelOutputs.RowCount = 3;
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 5F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 13F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TablePanelOutputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TablePanelOutputs.Size = new Size(1342, 156);
            TablePanelOutputs.TabIndex = 2;
            // 
            // panelOutputs
            // 
            panelOutputs.Controls.Add(btnComLog);
            panelOutputs.Controls.Add(btnRuntime);
            panelOutputs.Controls.Add(btnEditorOutput);
            panelOutputs.Dock = DockStyle.Fill;
            panelOutputs.Location = new Point(54, 5);
            panelOutputs.Margin = new Padding(0);
            panelOutputs.Name = "panelOutputs";
            panelOutputs.Size = new Size(1265, 29);
            panelOutputs.TabIndex = 2;
            // 
            // btnComLog
            // 
            btnComLog.BackColor = Color.LightGray;
            btnComLog.Dock = DockStyle.Left;
            btnComLog.FlatAppearance.BorderColor = Color.LightGray;
            btnComLog.FlatAppearance.BorderSize = 0;
            btnComLog.FlatStyle = FlatStyle.Flat;
            btnComLog.Location = new Point(193, 0);
            btnComLog.Margin = new Padding(0);
            btnComLog.Name = "btnComLog";
            btnComLog.Size = new Size(102, 29);
            btnComLog.TabIndex = 1;
            btnComLog.Text = "ComLog";
            btnComLog.UseVisualStyleBackColor = false;
            btnComLog.Visible = false;
            btnComLog.Click += btnShowFindReplaceOutput_Click;
            // 
            // btnRuntime
            // 
            btnRuntime.BackColor = Color.LightGray;
            btnRuntime.Dock = DockStyle.Left;
            btnRuntime.FlatAppearance.BorderColor = Color.LightGray;
            btnRuntime.FlatAppearance.BorderSize = 0;
            btnRuntime.FlatStyle = FlatStyle.Flat;
            btnRuntime.Location = new Point(91, 0);
            btnRuntime.Margin = new Padding(0);
            btnRuntime.Name = "btnRuntime";
            btnRuntime.Size = new Size(102, 29);
            btnRuntime.TabIndex = 1;
            btnRuntime.Text = "Runtime";
            btnRuntime.UseVisualStyleBackColor = false;
            btnRuntime.Click += btnBuildOutput_Click;
            // 
            // btnEditorOutput
            // 
            btnEditorOutput.BackColor = Color.LightGray;
            btnEditorOutput.Dock = DockStyle.Left;
            btnEditorOutput.FlatAppearance.BorderColor = Color.LightGray;
            btnEditorOutput.FlatAppearance.BorderSize = 0;
            btnEditorOutput.FlatStyle = FlatStyle.Flat;
            btnEditorOutput.Location = new Point(0, 0);
            btnEditorOutput.Margin = new Padding(0);
            btnEditorOutput.Name = "btnEditorOutput";
            btnEditorOutput.Size = new Size(91, 29);
            btnEditorOutput.TabIndex = 1;
            btnEditorOutput.Text = "Editor";
            btnEditorOutput.UseVisualStyleBackColor = false;
            btnEditorOutput.Click += btnEditorOutput_Click;
            // 
            // panelSplitter2
            // 
            panelSplitter2.BackColor = Color.LightGray;
            TablePanelOutputs.SetColumnSpan(panelSplitter2, 3);
            panelSplitter2.Dock = DockStyle.Fill;
            panelSplitter2.Location = new Point(0, 0);
            panelSplitter2.Margin = new Padding(0, 0, 0, 2);
            panelSplitter2.Name = "panelSplitter2";
            panelSplitter2.Padding = new Padding(0, 0, 0, 6);
            panelSplitter2.Size = new Size(1342, 3);
            panelSplitter2.TabIndex = 3;
            // 
            // panelSplitter4
            // 
            panelSplitter4.BackColor = Color.Silver;
            panelSplitter4.Dock = DockStyle.Fill;
            panelSplitter4.Location = new Point(0, 5);
            panelSplitter4.Margin = new Padding(0, 0, 51, 0);
            panelSplitter4.Name = "panelSplitter4";
            TablePanelOutputs.SetRowSpan(panelSplitter4, 2);
            panelSplitter4.Size = new Size(3, 136);
            panelSplitter4.TabIndex = 4;
            // 
            // panelOutput
            // 
            panelOutput.Dock = DockStyle.Fill;
            panelOutput.Location = new Point(54, 34);
            panelOutput.Margin = new Padding(0);
            panelOutput.Name = "panelOutput";
            panelOutput.Size = new Size(1265, 107);
            panelOutput.TabIndex = 7;
            panelOutput.Paint += panelOutput_Paint;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Margin = new Padding(4, 3, 4, 3);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(ContainerLeft);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(splitContainer1);
            splitContainer2.Size = new Size(1721, 696);
            splitContainer2.SplitterDistance = 374;
            splitContainer2.SplitterWidth = 5;
            splitContainer2.TabIndex = 1;
            // 
            // ContainerLeft
            // 
            ContainerLeft.Dock = DockStyle.Fill;
            ContainerLeft.Location = new Point(0, 0);
            ContainerLeft.Margin = new Padding(4, 3, 4, 3);
            ContainerLeft.Name = "ContainerLeft";
            ContainerLeft.Orientation = Orientation.Horizontal;
            // 
            // ContainerLeft.Panel2
            // 
            ContainerLeft.Panel2.Controls.Add(tblViewMethodes);
            ContainerLeft.Size = new Size(374, 696);
            ContainerLeft.SplitterDistance = 285;
            ContainerLeft.SplitterWidth = 5;
            ContainerLeft.TabIndex = 0;
            // 
            // tblViewMethodes
            // 
            tblViewMethodes.ColumnCount = 2;
            tblViewMethodes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblViewMethodes.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 23F));
            tblViewMethodes.Controls.Add(gridViewMethodes, 0, 1);
            tblViewMethodes.Controls.Add(panel1, 0, 0);
            tblViewMethodes.Dock = DockStyle.Fill;
            tblViewMethodes.Location = new Point(0, 0);
            tblViewMethodes.Margin = new Padding(4, 3, 4, 3);
            tblViewMethodes.Name = "tblViewMethodes";
            tblViewMethodes.RowCount = 2;
            tblViewMethodes.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tblViewMethodes.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblViewMethodes.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));
            tblViewMethodes.Size = new Size(374, 406);
            tblViewMethodes.TabIndex = 1;
            // 
            // gridViewMethodes
            // 
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            gridViewMethodes.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            gridViewMethodes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            gridViewMethodes.DefaultCellStyle = dataGridViewCellStyle2;
            gridViewMethodes.Dock = DockStyle.Fill;
            gridViewMethodes.Location = new Point(35, 28);
            gridViewMethodes.Margin = new Padding(35, 3, 0, 15);
            gridViewMethodes.Name = "gridViewMethodes";
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Control;
            dataGridViewCellStyle3.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle3.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            gridViewMethodes.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            gridViewMethodes.RowTemplate.DefaultCellStyle.Font = new Font("Calibri", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            gridViewMethodes.ScrollBars = ScrollBars.None;
            gridViewMethodes.Size = new Size(316, 363);
            gridViewMethodes.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(tbMethodeFilter);
            panel1.Controls.Add(lblMethodes);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Margin = new Padding(0);
            panel1.Name = "panel1";
            panel1.Size = new Size(351, 25);
            panel1.TabIndex = 3;
            // 
            // tbMethodeFilter
            // 
            tbMethodeFilter.BorderStyle = BorderStyle.None;
            tbMethodeFilter.Dock = DockStyle.Right;
            tbMethodeFilter.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tbMethodeFilter.Location = new Point(111, 0);
            tbMethodeFilter.Margin = new Padding(4, 3, 4, 3);
            tbMethodeFilter.Multiline = true;
            tbMethodeFilter.Name = "tbMethodeFilter";
            tbMethodeFilter.Size = new Size(240, 25);
            tbMethodeFilter.TabIndex = 4;
            tbMethodeFilter.TextChanged += tbMethodeFilter_TextChanged;
            // 
            // lblMethodes
            // 
            lblMethodes.AutoSize = true;
            lblMethodes.Dock = DockStyle.Left;
            lblMethodes.Font = new Font("Calibri", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            lblMethodes.Location = new Point(0, 0);
            lblMethodes.Margin = new Padding(0);
            lblMethodes.Name = "lblMethodes";
            lblMethodes.Padding = new Padding(35, 0, 0, 0);
            lblMethodes.Size = new Size(163, 19);
            lblMethodes.TabIndex = 3;
            lblMethodes.Text = "Methodes Classes:";
            lblMethodes.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // FormEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1721, 696);
            Controls.Add(splitContainer2);
            Margin = new Padding(4, 3, 4, 3);
            Name = "FormEditor";
            Text = "FormCodeEditor";
            Shown += FormCodeEditor_Shown;
            ResizeEnd += FormCodeEditor_ResizeEnd;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            EditorLayoutPanel.ResumeLayout(false);
            panelFunctions.ResumeLayout(false);
            TablePanelOutputs.ResumeLayout(false);
            panelOutputs.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ContainerLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)ContainerLeft).EndInit();
            ContainerLeft.ResumeLayout(false);
            tblViewMethodes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridViewMethodes).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel TablePanelOutputs;
        private System.Windows.Forms.Panel panelOutputs;
        private System.Windows.Forms.Button btnComLog;
        private System.Windows.Forms.Button btnRuntime;
        private System.Windows.Forms.Button btnEditorOutput;
        private System.Windows.Forms.Panel panelSplitter2;
        private System.Windows.Forms.Panel panelSplitter4;
       
        private System.Windows.Forms.Panel panelOutput;
        private System.Windows.Forms.TableLayoutPanel EditorLayoutPanel;
        private System.Windows.Forms.Panel panelFunctions;
        private System.Windows.Forms.Button btnShowHidden;
        private System.Windows.Forms.Button btnParagraph;
        private System.Windows.Forms.Button btnSnippets;
        public System.Windows.Forms.Button btnFindReplace;
        public System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.Button btnFormat;
     
        private System.Windows.Forms.Panel panelSplitter3;
        private System.Windows.Forms.Panel panelEditor;
      
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer ContainerLeft;
        private System.Windows.Forms.TableLayoutPanel tblViewMethodes;
        private System.Windows.Forms.DataGridView gridViewMethodes;
      
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox tbMethodeFilter;
        private System.Windows.Forms.Label lblMethodes;
 
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
     
    }
}