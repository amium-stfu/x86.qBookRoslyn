namespace qbook.Scripting
{
    partial class FormCsScript
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCsScript));
            this.codeEditor = new ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.SyntaxEditor();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.timerWatch = new System.Windows.Forms.Timer(this.components);
            this.buttonEval = new System.Windows.Forms.Button();
            this.tabControl3 = new System.Windows.Forms.TabControl();
            this.tabPageErrors = new System.Windows.Forms.TabPage();
            this.errorListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPageOutput = new System.Windows.Forms.TabPage();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.buttonClearOutput = new System.Windows.Forms.Button();
            this.listBoxObjects = new System.Windows.Forms.CheckedListBox();
            this.checkBoxObjects = new System.Windows.Forms.CheckBox();
            this.checkBoxWidgets = new System.Windows.Forms.CheckBox();
            this.timerObjectList = new System.Windows.Forms.Timer(this.components);
            this.textBoxObjectInfo = new System.Windows.Forms.TextBox();
            this.checkBoxMethods = new System.Windows.Forms.CheckBox();
            this.labelInfo = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelInfoWatch = new System.Windows.Forms.Label();
            this.colTerm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colResult = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tabControl3.SuspendLayout();
            this.tabPageErrors.SuspendLayout();
            this.tabPageOutput.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // codeEditor
            // 
            this.codeEditor.AllowDrop = true;
            this.codeEditor.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.codeEditor.IsLineNumberMarginVisible = true;
            this.codeEditor.Location = new System.Drawing.Point(0, 512);
            this.codeEditor.Name = "codeEditor";
            this.codeEditor.Size = new System.Drawing.Size(1135, 140);
            this.codeEditor.TabIndex = 2;
            this.codeEditor.Text = resources.GetString("codeEditor.Text");
            this.codeEditor.DocumentParseDataChanged += new System.EventHandler(this.syntaxEditor1_DocumentParseDataChanged);
            this.codeEditor.UserInterfaceUpdate += new System.EventHandler(this.codeEditor_UserInterfaceUpdate);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTerm,
            this.colResult,
            this.Column1});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Right;
            this.dataGridView1.Location = new System.Drawing.Point(750, 26);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(385, 486);
            this.dataGridView1.TabIndex = 7;
            this.dataGridView1.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellEndEdit);
            // 
            // timerWatch
            // 
            this.timerWatch.Enabled = true;
            this.timerWatch.Interval = 500;
            this.timerWatch.Tick += new System.EventHandler(this.timerWatch_Tick);
            // 
            // buttonEval
            // 
            this.buttonEval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEval.Location = new System.Drawing.Point(1083, 613);
            this.buttonEval.Name = "buttonEval";
            this.buttonEval.Size = new System.Drawing.Size(36, 23);
            this.buttonEval.TabIndex = 8;
            this.buttonEval.Text = ">>";
            this.buttonEval.UseVisualStyleBackColor = true;
            this.buttonEval.Click += new System.EventHandler(this.buttonEval_Click);
            // 
            // tabControl3
            // 
            this.tabControl3.Controls.Add(this.tabPageErrors);
            this.tabControl3.Controls.Add(this.tabPageOutput);
            this.tabControl3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tabControl3.Location = new System.Drawing.Point(0, 652);
            this.tabControl3.Name = "tabControl3";
            this.tabControl3.SelectedIndex = 0;
            this.tabControl3.Size = new System.Drawing.Size(1135, 141);
            this.tabControl3.TabIndex = 19;
            // 
            // tabPageErrors
            // 
            this.tabPageErrors.Controls.Add(this.errorListView);
            this.tabPageErrors.Location = new System.Drawing.Point(4, 22);
            this.tabPageErrors.Name = "tabPageErrors";
            this.tabPageErrors.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageErrors.Size = new System.Drawing.Size(1127, 115);
            this.tabPageErrors.TabIndex = 0;
            this.tabPageErrors.Text = "Errors";
            this.tabPageErrors.UseVisualStyleBackColor = true;
            // 
            // errorListView
            // 
            this.errorListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.errorListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.errorListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.errorListView.FullRowSelect = true;
            this.errorListView.HideSelection = false;
            this.errorListView.Location = new System.Drawing.Point(3, 3);
            this.errorListView.Name = "errorListView";
            this.errorListView.Size = new System.Drawing.Size(1121, 109);
            this.errorListView.TabIndex = 2;
            this.errorListView.UseCompatibleStateImageBehavior = false;
            this.errorListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Line";
            this.columnHeader1.Width = 40;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Char";
            this.columnHeader2.Width = 40;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Description";
            this.columnHeader3.Width = 450;
            // 
            // tabPageOutput
            // 
            this.tabPageOutput.Controls.Add(this.textBoxOutput);
            this.tabPageOutput.Location = new System.Drawing.Point(4, 22);
            this.tabPageOutput.Name = "tabPageOutput";
            this.tabPageOutput.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageOutput.Size = new System.Drawing.Size(1127, 115);
            this.tabPageOutput.TabIndex = 1;
            this.tabPageOutput.Text = "Output";
            this.tabPageOutput.UseVisualStyleBackColor = true;
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.AcceptsReturn = true;
            this.textBoxOutput.AcceptsTab = true;
            this.textBoxOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxOutput.Location = new System.Drawing.Point(3, 3);
            this.textBoxOutput.Multiline = true;
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxOutput.Size = new System.Drawing.Size(1121, 109);
            this.textBoxOutput.TabIndex = 0;
            // 
            // buttonClearOutput
            // 
            this.buttonClearOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearOutput.ForeColor = System.Drawing.Color.Red;
            this.buttonClearOutput.Location = new System.Drawing.Point(1087, 676);
            this.buttonClearOutput.Name = "buttonClearOutput";
            this.buttonClearOutput.Size = new System.Drawing.Size(24, 22);
            this.buttonClearOutput.TabIndex = 20;
            this.buttonClearOutput.Text = "X";
            this.buttonClearOutput.UseVisualStyleBackColor = true;
            this.buttonClearOutput.Click += new System.EventHandler(this.buttonClearOutput_Click);
            // 
            // listBoxObjects
            // 
            this.listBoxObjects.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxObjects.CheckOnClick = true;
            this.listBoxObjects.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxObjects.FormattingEnabled = true;
            this.listBoxObjects.IntegralHeight = false;
            this.listBoxObjects.Location = new System.Drawing.Point(0, 25);
            this.listBoxObjects.Name = "listBoxObjects";
            this.listBoxObjects.Size = new System.Drawing.Size(251, 487);
            this.listBoxObjects.TabIndex = 21;
            // 
            // checkBoxObjects
            // 
            this.checkBoxObjects.AutoSize = true;
            this.checkBoxObjects.Checked = true;
            this.checkBoxObjects.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxObjects.Location = new System.Drawing.Point(6, 6);
            this.checkBoxObjects.Name = "checkBoxObjects";
            this.checkBoxObjects.Size = new System.Drawing.Size(62, 17);
            this.checkBoxObjects.TabIndex = 22;
            this.checkBoxObjects.Text = "Objects";
            this.checkBoxObjects.UseVisualStyleBackColor = true;
            this.checkBoxObjects.CheckedChanged += new System.EventHandler(this.checkBoxObjects_CheckedChanged);
            // 
            // checkBoxWidgets
            // 
            this.checkBoxWidgets.AutoSize = true;
            this.checkBoxWidgets.Location = new System.Drawing.Point(73, 6);
            this.checkBoxWidgets.Name = "checkBoxWidgets";
            this.checkBoxWidgets.Size = new System.Drawing.Size(65, 17);
            this.checkBoxWidgets.TabIndex = 23;
            this.checkBoxWidgets.Text = "Widgets";
            this.checkBoxWidgets.UseVisualStyleBackColor = true;
            this.checkBoxWidgets.CheckedChanged += new System.EventHandler(this.checkBoxWidgets_CheckedChanged);
            // 
            // timerObjectList
            // 
            this.timerObjectList.Enabled = true;
            this.timerObjectList.Interval = 500;
            this.timerObjectList.Tick += new System.EventHandler(this.timerObjectList_Tick);
            // 
            // textBoxObjectInfo
            // 
            this.textBoxObjectInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxObjectInfo.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxObjectInfo.Location = new System.Drawing.Point(251, 25);
            this.textBoxObjectInfo.Multiline = true;
            this.textBoxObjectInfo.Name = "textBoxObjectInfo";
            this.textBoxObjectInfo.Size = new System.Drawing.Size(500, 487);
            this.textBoxObjectInfo.TabIndex = 24;
            // 
            // checkBoxMethods
            // 
            this.checkBoxMethods.AutoSize = true;
            this.checkBoxMethods.Location = new System.Drawing.Point(253, 5);
            this.checkBoxMethods.Name = "checkBoxMethods";
            this.checkBoxMethods.Size = new System.Drawing.Size(67, 17);
            this.checkBoxMethods.TabIndex = 25;
            this.checkBoxMethods.Text = "Methods";
            this.checkBoxMethods.UseVisualStyleBackColor = true;
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(334, 6);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(24, 13);
            this.labelInfo.TabIndex = 26;
            this.labelInfo.Text = "info";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.labelInfoWatch);
            this.panel1.Controls.Add(this.labelInfo);
            this.panel1.Controls.Add(this.checkBoxObjects);
            this.panel1.Controls.Add(this.checkBoxWidgets);
            this.panel1.Controls.Add(this.checkBoxMethods);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1135, 26);
            this.panel1.TabIndex = 27;
            // 
            // labelInfoWatch
            // 
            this.labelInfoWatch.AutoSize = true;
            this.labelInfoWatch.Location = new System.Drawing.Point(752, 6);
            this.labelInfoWatch.Name = "labelInfoWatch";
            this.labelInfoWatch.Size = new System.Drawing.Size(24, 13);
            this.labelInfoWatch.TabIndex = 27;
            this.labelInfoWatch.Text = "info";
            // 
            // colTerm
            // 
            this.colTerm.DataPropertyName = "Term";
            this.colTerm.HeaderText = "Term";
            this.colTerm.Name = "colTerm";
            // 
            // colResult
            // 
            this.colResult.DataPropertyName = "Result";
            this.colResult.HeaderText = "Result";
            this.colResult.Name = "colResult";
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column1.DataPropertyName = "CompileError";
            this.Column1.HeaderText = "Error";
            this.Column1.Name = "Column1";
            // 
            // FormCsScript
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1135, 793);
            this.Controls.Add(this.textBoxObjectInfo);
            this.Controls.Add(this.listBoxObjects);
            this.Controls.Add(this.buttonClearOutput);
            this.Controls.Add(this.buttonEval);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.codeEditor);
            this.Controls.Add(this.tabControl3);
            this.Controls.Add(this.panel1);
            this.KeyPreview = true;
            this.Name = "FormCsScript";
            this.Text = "CS Script";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormCsScript_FormClosing);
            this.Load += new System.EventHandler(this.FormCsScript_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormCsScript_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tabControl3.ResumeLayout(false);
            this.tabPageErrors.ResumeLayout(false);
            this.tabPageOutput.ResumeLayout(false);
            this.tabPageOutput.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.SyntaxEditor codeEditor;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Timer timerWatch;
        private System.Windows.Forms.Button buttonEval;
        private System.Windows.Forms.TabControl tabControl3;
        private System.Windows.Forms.TabPage tabPageErrors;
        private System.Windows.Forms.ListView errorListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.TabPage tabPageOutput;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Button buttonClearOutput;
        private System.Windows.Forms.CheckedListBox listBoxObjects;
        private System.Windows.Forms.CheckBox checkBoxObjects;
        private System.Windows.Forms.CheckBox checkBoxWidgets;
        private System.Windows.Forms.Timer timerObjectList;
        private System.Windows.Forms.TextBox textBoxObjectInfo;
        private System.Windows.Forms.CheckBox checkBoxMethods;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTerm;
        private System.Windows.Forms.DataGridViewTextBoxColumn colResult;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelInfoWatch;
    }
}