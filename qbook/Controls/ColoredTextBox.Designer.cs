namespace ColoredTextBox
{
    partial class ColoredTextBoxControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findReplaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.showSpecialCharactersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commentSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uncommentSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelText = new ColoredTextBox.PanelEx();
            this.panelFind = new System.Windows.Forms.Panel();
            this.buttonFindClose = new System.Windows.Forms.Button();
            this.checkBoxFindMatchCase = new System.Windows.Forms.CheckBox();
            this.checkBoxFindMatchWholeWord = new System.Windows.Forms.CheckBox();
            this.checkBoxFindUseRegex = new System.Windows.Forms.CheckBox();
            this.buttonFindReplaceAll = new System.Windows.Forms.Button();
            this.buttonFindReplaceNext = new System.Windows.Forms.Button();
            this.buttonFindPrev = new System.Windows.Forms.Button();
            this.buttonFindNext = new System.Windows.Forms.Button();
            this.textBoxReplace = new System.Windows.Forms.TextBox();
            this.textBoxFind = new System.Windows.Forms.TextBox();
            this.panelGutter = new ColoredTextBox.PanelEx();
            this.panelColumn = new ColoredTextBox.PanelEx();
            this.contextMenuStrip.SuspendLayout();
            this.panelText.SuspendLayout();
            this.panelFind.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripMenuItem1,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripMenuItem2,
            this.selectAllToolStripMenuItem,
            this.findReplaceToolStripMenuItem,
            this.toolStripSeparator1,
            this.showSpecialCharactersToolStripMenuItem,
            this.commentSelectionToolStripMenuItem,
            this.uncommentSelectionToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(249, 264);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            this.undoToolStripMenuItem.Text = "Undo [Ctrl+Z]";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            this.redoToolStripMenuItem.Text = "Redo [Ctrl+Y]";
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(245, 6);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            this.cutToolStripMenuItem.Text = "Cut [Ctrl+X]";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            this.copyToolStripMenuItem.Text = "Copy [Ctrl+C]";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            this.pasteToolStripMenuItem.Text = "Paste [Ctrl+V]";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            this.deleteToolStripMenuItem.Text = "Delete [Del]";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(245, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            this.selectAllToolStripMenuItem.Text = "Select All [Ctrl+A]";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // findReplaceToolStripMenuItem
            // 
            this.findReplaceToolStripMenuItem.Name = "findReplaceToolStripMenuItem";
            this.findReplaceToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            this.findReplaceToolStripMenuItem.Text = "Find / Replace [Ctrl+F / Ctrl+H]";
            this.findReplaceToolStripMenuItem.Click += new System.EventHandler(this.findReplaceToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(245, 6);
            // 
            // showSpecialCharactersToolStripMenuItem
            // 
            this.showSpecialCharactersToolStripMenuItem.Name = "showSpecialCharactersToolStripMenuItem";
            this.showSpecialCharactersToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            this.showSpecialCharactersToolStripMenuItem.Text = "Show Special Characters";
            this.showSpecialCharactersToolStripMenuItem.Click += new System.EventHandler(this.showSpecialCharactersToolStripMenuItem_Click);
            // 
            // commentSelectionToolStripMenuItem
            // 
            this.commentSelectionToolStripMenuItem.Name = "commentSelectionToolStripMenuItem";
            this.commentSelectionToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            this.commentSelectionToolStripMenuItem.Text = "Comment Selection [Ctrl+K,C]";
            this.commentSelectionToolStripMenuItem.Click += new System.EventHandler(this.commentSelectionToolStripMenuItem_Click);
            // 
            // uncommentSelectionToolStripMenuItem
            // 
            this.uncommentSelectionToolStripMenuItem.Name = "uncommentSelectionToolStripMenuItem";
            this.uncommentSelectionToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
            this.uncommentSelectionToolStripMenuItem.Text = "Uncomment Selection [Ctrl+K,U]";
            this.uncommentSelectionToolStripMenuItem.Click += new System.EventHandler(this.uncommentSelectionToolStripMenuItem_Click);
            // 
            // panelText
            // 
            this.panelText.AutoScroll = true;
            this.panelText.BackColor = System.Drawing.Color.White;
            this.panelText.ContextMenuStrip = this.contextMenuStrip;
            this.panelText.Controls.Add(this.panelFind);
            this.panelText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelText.Location = new System.Drawing.Point(28, 22);
            this.panelText.Name = "panelText";
            this.panelText.Size = new System.Drawing.Size(684, 455);
            this.panelText.TabIndex = 3;
            this.panelText.Paint += new System.Windows.Forms.PaintEventHandler(this.panelText_Paint);
            this.panelText.Leave += new System.EventHandler(this.panelText_Leave);
            this.panelText.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelText_MouseClick);
            this.panelText.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panelText_MouseDoubleClick);
            this.panelText.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelText_MouseDown);
            this.panelText.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelText_MouseMove);
            this.panelText.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelText_MouseUp);
            this.panelText.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.panelText_PreviewKeyDown);
            // 
            // panelFind
            // 
            this.panelFind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelFind.BackColor = System.Drawing.Color.LightGray;
            this.panelFind.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFind.Controls.Add(this.buttonFindClose);
            this.panelFind.Controls.Add(this.checkBoxFindMatchCase);
            this.panelFind.Controls.Add(this.checkBoxFindMatchWholeWord);
            this.panelFind.Controls.Add(this.checkBoxFindUseRegex);
            this.panelFind.Controls.Add(this.buttonFindReplaceAll);
            this.panelFind.Controls.Add(this.buttonFindReplaceNext);
            this.panelFind.Controls.Add(this.buttonFindPrev);
            this.panelFind.Controls.Add(this.buttonFindNext);
            this.panelFind.Controls.Add(this.textBoxReplace);
            this.panelFind.Controls.Add(this.textBoxFind);
            this.panelFind.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelFind.Location = new System.Drawing.Point(504, 0);
            this.panelFind.Name = "panelFind";
            this.panelFind.Size = new System.Drawing.Size(179, 72);
            this.panelFind.TabIndex = 0;
            this.panelFind.Visible = false;
            // 
            // buttonFindClose
            // 
            this.buttonFindClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFindClose.BackColor = System.Drawing.Color.IndianRed;
            this.buttonFindClose.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonFindClose.ForeColor = System.Drawing.Color.White;
            this.buttonFindClose.Location = new System.Drawing.Point(153, 47);
            this.buttonFindClose.Name = "buttonFindClose";
            this.buttonFindClose.Size = new System.Drawing.Size(24, 24);
            this.buttonFindClose.TabIndex = 11;
            this.buttonFindClose.Text = "X";
            this.buttonFindClose.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonFindClose.UseVisualStyleBackColor = false;
            this.buttonFindClose.Click += new System.EventHandler(this.buttonFindClose_Click);
            // 
            // checkBoxFindMatchCase
            // 
            this.checkBoxFindMatchCase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxFindMatchCase.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBoxFindMatchCase.FlatAppearance.CheckedBackColor = System.Drawing.Color.CornflowerBlue;
            this.checkBoxFindMatchCase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxFindMatchCase.Location = new System.Drawing.Point(2, 47);
            this.checkBoxFindMatchCase.Name = "checkBoxFindMatchCase";
            this.checkBoxFindMatchCase.Size = new System.Drawing.Size(32, 22);
            this.checkBoxFindMatchCase.TabIndex = 10;
            this.checkBoxFindMatchCase.Text = "Aa";
            this.checkBoxFindMatchCase.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxFindMatchCase.UseVisualStyleBackColor = true;
            // 
            // checkBoxFindMatchWholeWord
            // 
            this.checkBoxFindMatchWholeWord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxFindMatchWholeWord.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBoxFindMatchWholeWord.Enabled = false;
            this.checkBoxFindMatchWholeWord.FlatAppearance.CheckedBackColor = System.Drawing.Color.CornflowerBlue;
            this.checkBoxFindMatchWholeWord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxFindMatchWholeWord.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxFindMatchWholeWord.Location = new System.Drawing.Point(40, 47);
            this.checkBoxFindMatchWholeWord.Name = "checkBoxFindMatchWholeWord";
            this.checkBoxFindMatchWholeWord.Size = new System.Drawing.Size(32, 22);
            this.checkBoxFindMatchWholeWord.TabIndex = 9;
            this.checkBoxFindMatchWholeWord.Text = "Aa|";
            this.checkBoxFindMatchWholeWord.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxFindMatchWholeWord.UseVisualStyleBackColor = true;
            // 
            // checkBoxFindUseRegex
            // 
            this.checkBoxFindUseRegex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxFindUseRegex.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBoxFindUseRegex.Checked = true;
            this.checkBoxFindUseRegex.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFindUseRegex.FlatAppearance.CheckedBackColor = System.Drawing.Color.CornflowerBlue;
            this.checkBoxFindUseRegex.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxFindUseRegex.Location = new System.Drawing.Point(78, 47);
            this.checkBoxFindUseRegex.Name = "checkBoxFindUseRegex";
            this.checkBoxFindUseRegex.Size = new System.Drawing.Size(32, 22);
            this.checkBoxFindUseRegex.TabIndex = 8;
            this.checkBoxFindUseRegex.Text = ".*";
            this.checkBoxFindUseRegex.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxFindUseRegex.UseVisualStyleBackColor = true;
            // 
            // buttonFindReplaceAll
            // 
            this.buttonFindReplaceAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFindReplaceAll.Location = new System.Drawing.Point(154, 24);
            this.buttonFindReplaceAll.Name = "buttonFindReplaceAll";
            this.buttonFindReplaceAll.Size = new System.Drawing.Size(23, 23);
            this.buttonFindReplaceAll.TabIndex = 5;
            this.buttonFindReplaceAll.Text = "A";
            this.buttonFindReplaceAll.UseVisualStyleBackColor = true;
            this.buttonFindReplaceAll.Click += new System.EventHandler(this.buttonFindReplaceAll_Click);
            // 
            // buttonFindReplaceNext
            // 
            this.buttonFindReplaceNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFindReplaceNext.Location = new System.Drawing.Point(131, 24);
            this.buttonFindReplaceNext.Name = "buttonFindReplaceNext";
            this.buttonFindReplaceNext.Size = new System.Drawing.Size(23, 23);
            this.buttonFindReplaceNext.TabIndex = 4;
            this.buttonFindReplaceNext.Text = "N";
            this.buttonFindReplaceNext.UseVisualStyleBackColor = true;
            this.buttonFindReplaceNext.Click += new System.EventHandler(this.buttonFindReplaceNext_Click);
            // 
            // buttonFindPrev
            // 
            this.buttonFindPrev.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFindPrev.Location = new System.Drawing.Point(154, 2);
            this.buttonFindPrev.Name = "buttonFindPrev";
            this.buttonFindPrev.Size = new System.Drawing.Size(23, 23);
            this.buttonFindPrev.TabIndex = 3;
            this.buttonFindPrev.Text = "<";
            this.buttonFindPrev.UseVisualStyleBackColor = true;
            this.buttonFindPrev.Click += new System.EventHandler(this.buttonFindPrev_Click);
            // 
            // buttonFindNext
            // 
            this.buttonFindNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFindNext.Location = new System.Drawing.Point(131, 2);
            this.buttonFindNext.Name = "buttonFindNext";
            this.buttonFindNext.Size = new System.Drawing.Size(23, 23);
            this.buttonFindNext.TabIndex = 2;
            this.buttonFindNext.Text = ">";
            this.buttonFindNext.UseVisualStyleBackColor = true;
            this.buttonFindNext.Click += new System.EventHandler(this.buttonFindNext_Click);
            // 
            // textBoxReplace
            // 
            this.textBoxReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxReplace.Location = new System.Drawing.Point(2, 25);
            this.textBoxReplace.Name = "textBoxReplace";
            this.textBoxReplace.Size = new System.Drawing.Size(127, 21);
            this.textBoxReplace.TabIndex = 1;
            // 
            // textBoxFind
            // 
            this.textBoxFind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFind.Location = new System.Drawing.Point(2, 3);
            this.textBoxFind.Name = "textBoxFind";
            this.textBoxFind.Size = new System.Drawing.Size(127, 21);
            this.textBoxFind.TabIndex = 0;
            this.textBoxFind.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxFind_KeyDown);
            this.textBoxFind.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFind_KeyPress);
            // 
            // panelGutter
            // 
            this.panelGutter.BackColor = System.Drawing.Color.LightGray;
            this.panelGutter.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelGutter.Location = new System.Drawing.Point(0, 22);
            this.panelGutter.Name = "panelGutter";
            this.panelGutter.Size = new System.Drawing.Size(28, 455);
            this.panelGutter.TabIndex = 2;
            this.panelGutter.Paint += new System.Windows.Forms.PaintEventHandler(this.panelGutter_Paint);
            this.panelGutter.MouseLeave += new System.EventHandler(this.panelGutter_MouseLeave);
            this.panelGutter.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelGutter_MouseMove);
            // 
            // panelColumn
            // 
            this.panelColumn.BackColor = System.Drawing.Color.DarkGray;
            this.panelColumn.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelColumn.Location = new System.Drawing.Point(0, 0);
            this.panelColumn.Name = "panelColumn";
            this.panelColumn.Size = new System.Drawing.Size(712, 22);
            this.panelColumn.TabIndex = 0;
            this.panelColumn.Paint += new System.Windows.Forms.PaintEventHandler(this.panelColumn_Paint);
            // 
            // ColoredTextBoxControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.panelText);
            this.Controls.Add(this.panelGutter);
            this.Controls.Add(this.panelColumn);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ColoredTextBoxControl";
            this.Size = new System.Drawing.Size(712, 477);
            this.contextMenuStrip.ResumeLayout(false);
            this.panelText.ResumeLayout(false);
            this.panelFind.ResumeLayout(false);
            this.panelFind.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private PanelEx panelGutter;
        private PanelEx panelText;
        private PanelEx panelColumn;
        private System.Windows.Forms.Panel panelFind;
        private System.Windows.Forms.TextBox textBoxFind;
        private System.Windows.Forms.TextBox textBoxReplace;
        private System.Windows.Forms.Button buttonFindPrev;
        private System.Windows.Forms.Button buttonFindNext;
        private System.Windows.Forms.Button buttonFindReplaceAll;
        private System.Windows.Forms.Button buttonFindReplaceNext;
        private System.Windows.Forms.CheckBox checkBoxFindMatchCase;
        private System.Windows.Forms.CheckBox checkBoxFindMatchWholeWord;
        private System.Windows.Forms.CheckBox checkBoxFindUseRegex;
        private System.Windows.Forms.Button buttonFindClose;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findReplaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem showSpecialCharactersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem commentSelectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uncommentSelectionToolStripMenuItem;
    }
}
