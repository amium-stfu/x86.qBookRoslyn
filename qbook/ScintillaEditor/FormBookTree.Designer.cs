namespace qbook.ScintillaEditor
{
    partial class FormBookTree
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBookTree));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panelTreeView = new System.Windows.Forms.Panel();
            this.VBar = new qbook.CodeEditor.TreeViewVerticalBar();
            this.BookTreeViewIcons = new System.Windows.Forms.ImageList(this.components);
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
            this.tableLayoutPanel1.SuspendLayout();
            this.contextMenuTreeView.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.Controls.Add(this.panelTreeView, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.VBar, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(648, 450);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // panelTreeView
            // 
            this.panelTreeView.BackColor = System.Drawing.Color.IndianRed;
            this.panelTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTreeView.Location = new System.Drawing.Point(0, 0);
            this.panelTreeView.Margin = new System.Windows.Forms.Padding(0);
            this.panelTreeView.Name = "panelTreeView";
            this.panelTreeView.Size = new System.Drawing.Size(623, 450);
            this.panelTreeView.TabIndex = 0;
            // 
            // VBar
            // 
            this.VBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VBar.Location = new System.Drawing.Point(623, 0);
            this.VBar.Margin = new System.Windows.Forms.Padding(0);
            this.VBar.Name = "VBar";
            this.VBar.SetBackColor = System.Drawing.Color.LightGray;
            this.VBar.SetForeColor = System.Drawing.Color.DodgerBlue;
            this.VBar.Size = new System.Drawing.Size(25, 450);
            this.VBar.TabIndex = 2;
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
            this.contextMenuTreeView.Size = new System.Drawing.Size(181, 246);
            // 
            // addPageBeforeToolStripMenuItem
            // 
            this.addPageBeforeToolStripMenuItem.Name = "addPageBeforeToolStripMenuItem";
            this.addPageBeforeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.addPageBeforeToolStripMenuItem.Text = "Add Page before";
            this.addPageBeforeToolStripMenuItem.Click += new System.EventHandler(this.addPageBeforeToolStripMenuItem_Click);
            // 
            // addPageAfterToolStripMenuItem
            // 
            this.addPageAfterToolStripMenuItem.Name = "addPageAfterToolStripMenuItem";
            this.addPageAfterToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
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
            this.addSubCodeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.addSubCodeToolStripMenuItem.Text = "Add SubCode";
            // 
            // customToolStripMenuItem
            // 
            this.customToolStripMenuItem.Name = "customToolStripMenuItem";
            this.customToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.customToolStripMenuItem.Text = "Custom";
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
            this.hidePageToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.hidePageToolStripMenuItem.Text = "Hide Page";
            // 
            // deleteStripMenuItem
            // 
            this.deleteStripMenuItem.Name = "deleteStripMenuItem";
            this.deleteStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.deleteStripMenuItem.Text = "Delete Code";
            // 
            // toolStripMenuOpenWorkspace
            // 
            this.toolStripMenuOpenWorkspace.Name = "toolStripMenuOpenWorkspace";
            this.toolStripMenuOpenWorkspace.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuOpenWorkspace.Text = "Open Workspace";
            // 
            // renameCodeToolStripMenuItem
            // 
            this.renameCodeToolStripMenuItem.Name = "renameCodeToolStripMenuItem";
            this.renameCodeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.renameCodeToolStripMenuItem.Text = "Rename Code";
            // 
            // renamePageToolStripMenuItem
            // 
            this.renamePageToolStripMenuItem.Name = "renamePageToolStripMenuItem";
            this.renamePageToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.renamePageToolStripMenuItem.Text = "Rename Page";
            // 
            // toolStripMenuIncludeCode
            // 
            this.toolStripMenuIncludeCode.Name = "toolStripMenuIncludeCode";
            this.toolStripMenuIncludeCode.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuIncludeCode.Text = "Include Code";
            // 
            // deletePageToolStripMenuItem
            // 
            this.deletePageToolStripMenuItem.Name = "deletePageToolStripMenuItem";
            this.deletePageToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.deletePageToolStripMenuItem.Text = "Delete Page";
            // 
            // FormBookTree
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(648, 450);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FormBookTree";
            this.Text = "FormBookTree";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.contextMenuTreeView.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panelTreeView;
        public System.Windows.Forms.ImageList BookTreeViewIcons;
        private CodeEditor.TreeViewVerticalBar VBar;
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
        private System.Windows.Forms.ToolStripMenuItem deletePageToolStripMenuItem;
    }
}