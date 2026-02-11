namespace qbookCode.Controls
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBookTree));
            tableLayoutPanel1 = new TableLayoutPanel();
            panelTreeView = new Panel();
            VBar = new qbookCode.Controls.Scrollbars.TreeViewVerticalBar();
            BookTreeViewIcons = new ImageList(components);
            contextMenuTreeView = new ContextMenuStrip(components);
            addPageBeforeToolStripMenuItem = new ToolStripMenuItem();
            addPageAfterToolStripMenuItem = new ToolStripMenuItem();
            addSubCodeToolStripMenuItem = new ToolStripMenuItem();
            customToolStripMenuItem = new ToolStripMenuItem();
            uDLClientToolStripMenuItem = new ToolStripMenuItem();
            aKClientToolStripMenuItem = new ToolStripMenuItem();
            aKServerToolStripMenuItem = new ToolStripMenuItem();
            streamClientToolStripMenuItem = new ToolStripMenuItem();
            hidePageToolStripMenuItem = new ToolStripMenuItem();
            deleteStripMenuItem = new ToolStripMenuItem();
            toolStripMenuOpenWorkspace = new ToolStripMenuItem();
            renameCodeToolStripMenuItem = new ToolStripMenuItem();
            renamePageToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuIncludeCode = new ToolStripMenuItem();
            deletePageToolStripMenuItem = new ToolStripMenuItem();
            tableLayoutPanel1.SuspendLayout();
            contextMenuTreeView.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 23F));
            tableLayoutPanel1.Controls.Add(panelTreeView, 0, 0);
            tableLayoutPanel1.Controls.Add(VBar, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(756, 519);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // panelTreeView
            // 
            panelTreeView.BackColor = Color.IndianRed;
            panelTreeView.Dock = DockStyle.Fill;
            panelTreeView.Location = new Point(0, 0);
            panelTreeView.Margin = new Padding(0);
            panelTreeView.Name = "panelTreeView";
            panelTreeView.Size = new Size(733, 519);
            panelTreeView.TabIndex = 0;
            // 
            // VBar
            // 
            VBar.Dock = DockStyle.Fill;
            VBar.Location = new Point(733, 0);
            VBar.Margin = new Padding(0);
            VBar.Name = "VBar";
            VBar.SetBackColor = Color.LightGray;
            VBar.SetForeColor = Color.DodgerBlue;
            VBar.Size = new Size(23, 519);
            VBar.TabIndex = 2;
            // 
            // BookTreeViewIcons
            // 
            BookTreeViewIcons.ColorDepth = ColorDepth.Depth32Bit;
            BookTreeViewIcons.ImageStream = (ImageListStreamer)resources.GetObject("BookTreeViewIcons.ImageStream");
            BookTreeViewIcons.TransparentColor = Color.Transparent;
            BookTreeViewIcons.Images.SetKeyName(0, "play_red_button_icon_227850.png");
            BookTreeViewIcons.Images.SetKeyName(1, "book_48p.png");
            BookTreeViewIcons.Images.SetKeyName(2, "Page_48p.png");
            BookTreeViewIcons.Images.SetKeyName(3, "C#.png");
            BookTreeViewIcons.Images.SetKeyName(4, "ic_radio_button_off_48p.png");
            BookTreeViewIcons.Images.SetKeyName(5, "view_hide_icon_124813.png");
            BookTreeViewIcons.Images.SetKeyName(6, "CodeOff_Light.png");
            BookTreeViewIcons.Images.SetKeyName(7, "foldergrey_93178.png");
            BookTreeViewIcons.Images.SetKeyName(8, "folderorangeopen_93000.png");
            BookTreeViewIcons.Images.SetKeyName(9, "forbidden_icon_242335.png");
            BookTreeViewIcons.Images.SetKeyName(10, "vcsconflicting_48p.png");
            BookTreeViewIcons.Images.SetKeyName(11, "hidden_eye_40p_black.png");
            BookTreeViewIcons.Images.SetKeyName(12, "find_magnifying_glass_icon_176383.png");
            // 
            // contextMenuTreeView
            // 
            contextMenuTreeView.Items.AddRange(new ToolStripItem[] { addPageBeforeToolStripMenuItem, addPageAfterToolStripMenuItem, addSubCodeToolStripMenuItem, hidePageToolStripMenuItem, deleteStripMenuItem, toolStripMenuOpenWorkspace, renameCodeToolStripMenuItem, renamePageToolStripMenuItem, toolStripMenuIncludeCode, deletePageToolStripMenuItem });
            contextMenuTreeView.Name = "contextMenuTreeView";
            contextMenuTreeView.Size = new Size(165, 224);
            // 
            // addPageBeforeToolStripMenuItem
            // 
            addPageBeforeToolStripMenuItem.Name = "addPageBeforeToolStripMenuItem";
            addPageBeforeToolStripMenuItem.Size = new Size(164, 22);
            addPageBeforeToolStripMenuItem.Text = "Add Page before";
            addPageBeforeToolStripMenuItem.Click += addPageBeforeToolStripMenuItem_Click;
            // 
            // addPageAfterToolStripMenuItem
            // 
            addPageAfterToolStripMenuItem.Name = "addPageAfterToolStripMenuItem";
            addPageAfterToolStripMenuItem.Size = new Size(164, 22);
            addPageAfterToolStripMenuItem.Text = "Add Page after";
            addPageAfterToolStripMenuItem.Click += addPageAfterToolStripMenuItem_Click;
            // 
            // addSubCodeToolStripMenuItem
            // 
            addSubCodeToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { customToolStripMenuItem, uDLClientToolStripMenuItem, aKClientToolStripMenuItem, aKServerToolStripMenuItem, streamClientToolStripMenuItem });
            addSubCodeToolStripMenuItem.Name = "addSubCodeToolStripMenuItem";
            addSubCodeToolStripMenuItem.Size = new Size(164, 22);
            addSubCodeToolStripMenuItem.Text = "Add SubCode";
            // 
            // customToolStripMenuItem
            // 
            customToolStripMenuItem.Name = "customToolStripMenuItem";
            customToolStripMenuItem.Size = new Size(145, 22);
            customToolStripMenuItem.Text = "Custom";
            // 
            // uDLClientToolStripMenuItem
            // 
            uDLClientToolStripMenuItem.Name = "uDLClientToolStripMenuItem";
            uDLClientToolStripMenuItem.Size = new Size(145, 22);
            uDLClientToolStripMenuItem.Text = "UDL Client";
            // 
            // aKClientToolStripMenuItem
            // 
            aKClientToolStripMenuItem.Name = "aKClientToolStripMenuItem";
            aKClientToolStripMenuItem.Size = new Size(145, 22);
            aKClientToolStripMenuItem.Text = "AK Client";
            // 
            // aKServerToolStripMenuItem
            // 
            aKServerToolStripMenuItem.Name = "aKServerToolStripMenuItem";
            aKServerToolStripMenuItem.Size = new Size(145, 22);
            aKServerToolStripMenuItem.Text = "AK Server";
            // 
            // streamClientToolStripMenuItem
            // 
            streamClientToolStripMenuItem.Name = "streamClientToolStripMenuItem";
            streamClientToolStripMenuItem.Size = new Size(145, 22);
            streamClientToolStripMenuItem.Text = "Stream Client";
            // 
            // hidePageToolStripMenuItem
            // 
            hidePageToolStripMenuItem.Name = "hidePageToolStripMenuItem";
            hidePageToolStripMenuItem.Size = new Size(164, 22);
            hidePageToolStripMenuItem.Text = "Hide Page";
            // 
            // deleteStripMenuItem
            // 
            deleteStripMenuItem.Name = "deleteStripMenuItem";
            deleteStripMenuItem.Size = new Size(164, 22);
            deleteStripMenuItem.Text = "Delete Code";
            // 
            // toolStripMenuOpenWorkspace
            // 
            toolStripMenuOpenWorkspace.Name = "toolStripMenuOpenWorkspace";
            toolStripMenuOpenWorkspace.Size = new Size(164, 22);
            toolStripMenuOpenWorkspace.Text = "Open Workspace";
            // 
            // renameCodeToolStripMenuItem
            // 
            renameCodeToolStripMenuItem.Name = "renameCodeToolStripMenuItem";
            renameCodeToolStripMenuItem.Size = new Size(164, 22);
            renameCodeToolStripMenuItem.Text = "Rename Code";
            // 
            // renamePageToolStripMenuItem
            // 
            renamePageToolStripMenuItem.Name = "renamePageToolStripMenuItem";
            renamePageToolStripMenuItem.Size = new Size(164, 22);
            renamePageToolStripMenuItem.Text = "Rename Page";
            // 
            // toolStripMenuIncludeCode
            // 
            toolStripMenuIncludeCode.Name = "toolStripMenuIncludeCode";
            toolStripMenuIncludeCode.Size = new Size(164, 22);
            toolStripMenuIncludeCode.Text = "Include Code";
            // 
            // deletePageToolStripMenuItem
            // 
            deletePageToolStripMenuItem.Name = "deletePageToolStripMenuItem";
            deletePageToolStripMenuItem.Size = new Size(164, 22);
            deletePageToolStripMenuItem.Text = "Delete Page";
            // 
            // FormBookTree
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(756, 519);
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(4, 3, 4, 3);
            Name = "FormBookTree";
            Text = "FormBookTree";
            tableLayoutPanel1.ResumeLayout(false);
            contextMenuTreeView.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panelTreeView;
        public System.Windows.Forms.ImageList BookTreeViewIcons;
        private qbookCode.Controls.Scrollbars.TreeViewVerticalBar VBar;
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