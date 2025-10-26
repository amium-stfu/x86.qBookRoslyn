
namespace qbook
{
    partial class EditObjectForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditObjectForm));
            this.buttonDelete = new System.Windows.Forms.Button();
            this.textBoxText = new System.Windows.Forms.TextBox();
            this.buttonTranslate = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageText = new System.Windows.Forms.TabPage();
            this.tabPageTag = new System.Windows.Forms.TabPage();
            this.listBoxTag = new System.Windows.Forms.ListBox();
            this.buttonTR = new System.Windows.Forms.Button();
            this.buttonBR = new System.Windows.Forms.Button();
            this.buttonBL = new System.Windows.Forms.Button();
            this.buttonMR = new System.Windows.Forms.Button();
            this.buttonML = new System.Windows.Forms.Button();
            this.buttonTL = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.findReferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gotoDefinitionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameSymbolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.showDebuggerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showLogConsoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.buttonReset = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.buttonUndo = new System.Windows.Forms.Button();
            this.treeViewCodePages = new System.Windows.Forms.TreeView();
            this.checkBoxShowTree = new System.Windows.Forms.CheckBox();
            this.contextMenuPageTree = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addSubCodeClassToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1.SuspendLayout();
            this.tabPageText.SuspendLayout();
            this.tabPageTag.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.contextMenuPageTree.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonDelete
            // 
            this.buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonDelete.BackColor = System.Drawing.Color.WhiteSmoke;
            this.buttonDelete.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDelete.Location = new System.Drawing.Point(1, 712);
            this.buttonDelete.Margin = new System.Windows.Forms.Padding(2);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(60, 28);
            this.buttonDelete.TabIndex = 99;
            this.buttonDelete.Text = "Delete Object";
            this.buttonDelete.UseVisualStyleBackColor = false;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // textBoxText
            // 
            this.textBoxText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxText.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxText.Location = new System.Drawing.Point(1, 37);
            this.textBoxText.Margin = new System.Windows.Forms.Padding(1);
            this.textBoxText.Multiline = true;
            this.textBoxText.Name = "textBoxText";
            this.textBoxText.Size = new System.Drawing.Size(1079, 360);
            this.textBoxText.TabIndex = 99;
            this.textBoxText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxText_KeyDown);
            this.textBoxText.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxText_KeyUp);
            // 
            // buttonTranslate
            // 
            this.buttonTranslate.BackColor = System.Drawing.Color.WhiteSmoke;
            this.buttonTranslate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonTranslate.Font = new System.Drawing.Font("Calibri", 10F);
            this.buttonTranslate.Location = new System.Drawing.Point(2, 3);
            this.buttonTranslate.Margin = new System.Windows.Forms.Padding(2);
            this.buttonTranslate.Name = "buttonTranslate";
            this.buttonTranslate.Size = new System.Drawing.Size(98, 31);
            this.buttonTranslate.TabIndex = 99;
            this.buttonTranslate.Text = "Retranslate";
            this.buttonTranslate.UseVisualStyleBackColor = false;
            this.buttonTranslate.Visible = false;
            this.buttonTranslate.Click += new System.EventHandler(this.buttonTranslate_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClose.Location = new System.Drawing.Point(1019, 712);
            this.buttonClose.Margin = new System.Windows.Forms.Padding(2);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(72, 28);
            this.buttonClose.TabIndex = 99;
            this.buttonClose.Text = "OK/Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPageText);
            this.tabControl1.Controls.Add(this.tabPageTag);
            this.tabControl1.Location = new System.Drawing.Point(183, 10);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(1);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(911, 693);
            this.tabControl1.TabIndex = 99;
            // 
            // tabPageText
            // 
            this.tabPageText.Controls.Add(this.textBoxText);
            this.tabPageText.Controls.Add(this.buttonTranslate);
            this.tabPageText.Location = new System.Drawing.Point(4, 22);
            this.tabPageText.Margin = new System.Windows.Forms.Padding(1);
            this.tabPageText.Name = "tabPageText";
            this.tabPageText.Padding = new System.Windows.Forms.Padding(1);
            this.tabPageText.Size = new System.Drawing.Size(903, 667);
            this.tabPageText.TabIndex = 99;
            this.tabPageText.Text = "Text";
            this.tabPageText.UseVisualStyleBackColor = true;
            // 
            // tabPageTag
            // 
            this.tabPageTag.Controls.Add(this.listBoxTag);
            this.tabPageTag.Controls.Add(this.buttonTR);
            this.tabPageTag.Controls.Add(this.buttonBR);
            this.tabPageTag.Controls.Add(this.buttonBL);
            this.tabPageTag.Controls.Add(this.buttonMR);
            this.tabPageTag.Controls.Add(this.buttonML);
            this.tabPageTag.Controls.Add(this.buttonTL);
            this.tabPageTag.Location = new System.Drawing.Point(4, 22);
            this.tabPageTag.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageTag.Name = "tabPageTag";
            this.tabPageTag.Size = new System.Drawing.Size(903, 667);
            this.tabPageTag.TabIndex = 99;
            this.tabPageTag.Text = "Tag";
            this.tabPageTag.UseVisualStyleBackColor = true;
            // 
            // listBoxTag
            // 
            this.listBoxTag.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxTag.BackColor = System.Drawing.SystemColors.Control;
            this.listBoxTag.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxTag.FormattingEnabled = true;
            this.listBoxTag.ItemHeight = 19;
            this.listBoxTag.Location = new System.Drawing.Point(1, 34);
            this.listBoxTag.Margin = new System.Windows.Forms.Padding(1);
            this.listBoxTag.Name = "listBoxTag";
            this.listBoxTag.Size = new System.Drawing.Size(172, 213);
            this.listBoxTag.TabIndex = 99;
            this.listBoxTag.SelectedIndexChanged += new System.EventHandler(this.listBoxTag_SelectedIndexChanged);
            // 
            // buttonTR
            // 
            this.buttonTR.BackColor = System.Drawing.SystemColors.Control;
            this.buttonTR.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonTR.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonTR.Location = new System.Drawing.Point(221, 34);
            this.buttonTR.Margin = new System.Windows.Forms.Padding(1);
            this.buttonTR.Name = "buttonTR";
            this.buttonTR.Size = new System.Drawing.Size(35, 24);
            this.buttonTR.TabIndex = 99;
            this.buttonTR.Text = "TR";
            this.buttonTR.UseVisualStyleBackColor = false;
            this.buttonTR.Click += new System.EventHandler(this.buttonPosition_Click);
            // 
            // buttonBR
            // 
            this.buttonBR.BackColor = System.Drawing.SystemColors.Control;
            this.buttonBR.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonBR.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonBR.Location = new System.Drawing.Point(221, 86);
            this.buttonBR.Margin = new System.Windows.Forms.Padding(1);
            this.buttonBR.Name = "buttonBR";
            this.buttonBR.Size = new System.Drawing.Size(35, 24);
            this.buttonBR.TabIndex = 99;
            this.buttonBR.Text = "BR";
            this.buttonBR.UseVisualStyleBackColor = false;
            this.buttonBR.Click += new System.EventHandler(this.buttonPosition_Click);
            // 
            // buttonBL
            // 
            this.buttonBL.BackColor = System.Drawing.SystemColors.Control;
            this.buttonBL.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonBL.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonBL.Location = new System.Drawing.Point(184, 86);
            this.buttonBL.Margin = new System.Windows.Forms.Padding(1);
            this.buttonBL.Name = "buttonBL";
            this.buttonBL.Size = new System.Drawing.Size(35, 24);
            this.buttonBL.TabIndex = 99;
            this.buttonBL.Text = "BL";
            this.buttonBL.UseVisualStyleBackColor = false;
            this.buttonBL.Click += new System.EventHandler(this.buttonPosition_Click);
            // 
            // buttonMR
            // 
            this.buttonMR.BackColor = System.Drawing.SystemColors.Control;
            this.buttonMR.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMR.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonMR.Location = new System.Drawing.Point(221, 60);
            this.buttonMR.Margin = new System.Windows.Forms.Padding(1);
            this.buttonMR.Name = "buttonMR";
            this.buttonMR.Size = new System.Drawing.Size(35, 24);
            this.buttonMR.TabIndex = 99;
            this.buttonMR.Text = "MR";
            this.buttonMR.UseVisualStyleBackColor = false;
            this.buttonMR.Click += new System.EventHandler(this.buttonPosition_Click);
            // 
            // buttonML
            // 
            this.buttonML.BackColor = System.Drawing.SystemColors.Control;
            this.buttonML.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonML.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonML.Location = new System.Drawing.Point(184, 60);
            this.buttonML.Margin = new System.Windows.Forms.Padding(1);
            this.buttonML.Name = "buttonML";
            this.buttonML.Size = new System.Drawing.Size(35, 24);
            this.buttonML.TabIndex = 99;
            this.buttonML.Text = "ML";
            this.buttonML.UseVisualStyleBackColor = false;
            this.buttonML.Click += new System.EventHandler(this.buttonPosition_Click);
            // 
            // buttonTL
            // 
            this.buttonTL.BackColor = System.Drawing.SystemColors.Control;
            this.buttonTL.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonTL.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonTL.Location = new System.Drawing.Point(184, 34);
            this.buttonTL.Margin = new System.Windows.Forms.Padding(1);
            this.buttonTL.Name = "buttonTL";
            this.buttonTL.Size = new System.Drawing.Size(35, 24);
            this.buttonTL.TabIndex = 99;
            this.buttonTL.Text = "TL";
            this.buttonTL.UseVisualStyleBackColor = false;
            this.buttonTL.Click += new System.EventHandler(this.buttonPosition_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.findReferencesToolStripMenuItem,
            this.gotoDefinitionToolStripMenuItem,
            this.renameSymbolToolStripMenuItem,
            this.toolStripSeparator5,
            this.expandAllToolStripMenuItem,
            this.collapseAllToolStripMenuItem,
            this.toolStripSeparator6,
            this.showDebuggerToolStripMenuItem,
            this.showLogConsoleToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(195, 170);
            // 
            // findReferencesToolStripMenuItem
            // 
            this.findReferencesToolStripMenuItem.Name = "findReferencesToolStripMenuItem";
            this.findReferencesToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.findReferencesToolStripMenuItem.Text = "Find References";
            // 
            // gotoDefinitionToolStripMenuItem
            // 
            this.gotoDefinitionToolStripMenuItem.Name = "gotoDefinitionToolStripMenuItem";
            this.gotoDefinitionToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.gotoDefinitionToolStripMenuItem.Text = "Go To Definition";
            // 
            // renameSymbolToolStripMenuItem
            // 
            this.renameSymbolToolStripMenuItem.Name = "renameSymbolToolStripMenuItem";
            this.renameSymbolToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.renameSymbolToolStripMenuItem.Text = "Rename Symbol";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(191, 6);
            // 
            // expandAllToolStripMenuItem
            // 
            this.expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
            this.expandAllToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.expandAllToolStripMenuItem.Text = "Expand All Outlining";
            // 
            // collapseAllToolStripMenuItem
            // 
            this.collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
            this.collapseAllToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.collapseAllToolStripMenuItem.Text = "Collapse To Definitions";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(191, 6);
            // 
            // showDebuggerToolStripMenuItem
            // 
            this.showDebuggerToolStripMenuItem.Name = "showDebuggerToolStripMenuItem";
            this.showDebuggerToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.showDebuggerToolStripMenuItem.Text = "Show Debugger";
            // 
            // showLogConsoleToolStripMenuItem
            // 
            this.showLogConsoleToolStripMenuItem.Name = "showLogConsoleToolStripMenuItem";
            this.showLogConsoleToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.showLogConsoleToolStripMenuItem.Text = "Show Log/Console";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(342, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 99;
            this.label1.Text = "Name";
            // 
            // textBoxName
            // 
            this.textBoxName.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxName.Location = new System.Drawing.Point(379, 2);
            this.textBoxName.Margin = new System.Windows.Forms.Padding(1);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(149, 27);
            this.textBoxName.TabIndex = 99;
            this.textBoxName.TextChanged += new System.EventHandler(this.textBoxName_TextChanged);
            this.textBoxName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxName_KeyDown);
            this.textBoxName.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxName_KeyUp);
            // 
            // buttonReset
            // 
            this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonReset.BackColor = System.Drawing.Color.WhiteSmoke;
            this.buttonReset.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonReset.Location = new System.Drawing.Point(60, 712);
            this.buttonReset.Margin = new System.Windows.Forms.Padding(2);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(65, 28);
            this.buttonReset.TabIndex = 99;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = false;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // buttonUndo
            // 
            this.buttonUndo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUndo.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonUndo.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonUndo.Location = new System.Drawing.Point(797, 712);
            this.buttonUndo.Margin = new System.Windows.Forms.Padding(2);
            this.buttonUndo.Name = "buttonUndo";
            this.buttonUndo.Size = new System.Drawing.Size(107, 28);
            this.buttonUndo.TabIndex = 100;
            this.buttonUndo.Text = "Undo Settings";
            this.buttonUndo.UseVisualStyleBackColor = true;
            this.buttonUndo.Click += new System.EventHandler(this.buttonUndo_Click);
            // 
            // treeViewCodePages
            // 
            this.treeViewCodePages.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treeViewCodePages.Location = new System.Drawing.Point(1, 10);
            this.treeViewCodePages.Name = "treeViewCodePages";
            this.treeViewCodePages.Size = new System.Drawing.Size(178, 693);
            this.treeViewCodePages.TabIndex = 102;
            this.treeViewCodePages.Visible = false;
            this.treeViewCodePages.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeViewCodePages_AfterLabelEdit);
            this.treeViewCodePages.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewCodePages_AfterSelect);
            this.treeViewCodePages.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewCodePages_NodeMouseClick);
            // 
            // checkBoxShowTree
            // 
            this.checkBoxShowTree.AutoSize = true;
            this.checkBoxShowTree.Location = new System.Drawing.Point(538, 8);
            this.checkBoxShowTree.Name = "checkBoxShowTree";
            this.checkBoxShowTree.Size = new System.Drawing.Size(48, 17);
            this.checkBoxShowTree.TabIndex = 103;
            this.checkBoxShowTree.Text = "Tree";
            this.checkBoxShowTree.UseVisualStyleBackColor = true;
            this.checkBoxShowTree.CheckedChanged += new System.EventHandler(this.checkBoxShowTree_CheckedChanged);
            // 
            // contextMenuPageTree
            // 
            this.contextMenuPageTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addSubCodeClassToolStripMenuItem});
            this.contextMenuPageTree.Name = "contextMenuPageTree";
            this.contextMenuPageTree.Size = new System.Drawing.Size(189, 26);
            // 
            // addSubCodeClassToolStripMenuItem
            // 
            this.addSubCodeClassToolStripMenuItem.Name = "addSubCodeClassToolStripMenuItem";
            this.addSubCodeClassToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.addSubCodeClassToolStripMenuItem.Text = "add Sub-Code / Class";
            this.addSubCodeClassToolStripMenuItem.Click += new System.EventHandler(this.addSubCodeClassToolStripMenuItem_Click);
            // 
            // EditObjectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSlateGray;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(1095, 744);
            this.Controls.Add(this.checkBoxShowTree);
            this.Controls.Add(this.treeViewCodePages);
            this.Controls.Add(this.buttonUndo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonDelete);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(1);
            this.Name = "EditObjectForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditObjectForm_FormClosing);
            this.Load += new System.EventHandler(this.EditObjectForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextForm_KeyDown);
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.TextForm_PreviewKeyDown);
            this.tabControl1.ResumeLayout(false);
            this.tabPageText.ResumeLayout(false);
            this.tabPageText.PerformLayout();
            this.tabPageTag.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuPageTree.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.Button buttonTranslate;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageText;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPageTag;
        private System.Windows.Forms.ListBox listBoxTag;
        private System.Windows.Forms.Button buttonTR;
        private System.Windows.Forms.Button buttonBR;
        private System.Windows.Forms.Button buttonBL;
        private System.Windows.Forms.Button buttonMR;
        private System.Windows.Forms.Button buttonML;
        private System.Windows.Forms.Button buttonTL;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button buttonUndo;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem findReferencesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gotoDefinitionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameSymbolToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem showDebuggerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showLogConsoleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapseAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.TreeView treeViewCodePages;
        private System.Windows.Forms.CheckBox checkBoxShowTree;
        private System.Windows.Forms.ContextMenuStrip contextMenuPageTree;
        private System.Windows.Forms.ToolStripMenuItem addSubCodeClassToolStripMenuItem;
    }
}