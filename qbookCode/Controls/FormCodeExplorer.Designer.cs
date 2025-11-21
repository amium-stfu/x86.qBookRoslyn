namespace qbookCode.Controls
{
    partial class FormCodeExplorer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCodeExplorer));
            LayoutRoot = new TableLayoutPanel();
            panelExplorer = new Panel();
            panelHeader = new Panel();
            flowLayoutPageData = new FlowLayoutPanel();
            btnToggleTheme = new Button();
            btnSave = new Button();
            btnReload = new Button();
            btnRebuild = new Button();
            btnRun = new Button();
            btnStop = new Button();
            panelFooter = new Panel();
            labelStatus = new Label();
            LayoutRoot.SuspendLayout();
            panelHeader.SuspendLayout();
            panelFooter.SuspendLayout();
            SuspendLayout();
            // 
            // LayoutRoot
            // 
            LayoutRoot.BackColor = Color.White;
            LayoutRoot.ColumnCount = 1;
            LayoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            LayoutRoot.Controls.Add(panelExplorer, 0, 1);
            LayoutRoot.Controls.Add(panelHeader, 0, 0);
            LayoutRoot.Controls.Add(panelFooter, 0, 2);
            LayoutRoot.Dock = DockStyle.Fill;
            LayoutRoot.Location = new Point(0, 0);
            LayoutRoot.Margin = new Padding(4, 3, 4, 3);
            LayoutRoot.Name = "LayoutRoot";
            LayoutRoot.RowCount = 3;
            LayoutRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            LayoutRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            LayoutRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 69F));
            LayoutRoot.Size = new Size(1577, 738);
            LayoutRoot.TabIndex = 0;
            // 
            // panelExplorer
            // 
            panelExplorer.Dock = DockStyle.Fill;
            panelExplorer.Location = new Point(0, 52);
            panelExplorer.Margin = new Padding(0);
            panelExplorer.Name = "panelExplorer";
            panelExplorer.Size = new Size(1577, 617);
            panelExplorer.TabIndex = 0;
            // 
            // panelHeader
            // 
            panelHeader.Controls.Add(flowLayoutPageData);
            panelHeader.Controls.Add(btnToggleTheme);
            panelHeader.Controls.Add(btnSave);
            panelHeader.Controls.Add(btnReload);
            panelHeader.Controls.Add(btnRebuild);
            panelHeader.Controls.Add(btnRun);
            panelHeader.Controls.Add(btnStop);
            panelHeader.Dock = DockStyle.Fill;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Margin = new Padding(0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1577, 52);
            panelHeader.TabIndex = 1;
            // 
            // flowLayoutPageData
            // 
            flowLayoutPageData.Dock = DockStyle.Fill;
            flowLayoutPageData.FlowDirection = FlowDirection.TopDown;
            flowLayoutPageData.Location = new Point(540, 0);
            flowLayoutPageData.Margin = new Padding(23, 0, 0, 0);
            flowLayoutPageData.Name = "flowLayoutPageData";
            flowLayoutPageData.Padding = new Padding(23, 0, 0, 0);
            flowLayoutPageData.Size = new Size(1037, 52);
            flowLayoutPageData.TabIndex = 14;
            // 
            // btnToggleTheme
            // 
            btnToggleTheme.Dock = DockStyle.Left;
            btnToggleTheme.FlatAppearance.BorderColor = Color.White;
            btnToggleTheme.FlatStyle = FlatStyle.Flat;
            btnToggleTheme.Image = (Image)resources.GetObject("btnToggleTheme.Image");
            btnToggleTheme.ImageAlign = ContentAlignment.MiddleLeft;
            btnToggleTheme.Location = new Point(450, 0);
            btnToggleTheme.Margin = new Padding(4, 3, 4, 3);
            btnToggleTheme.Name = "btnToggleTheme";
            btnToggleTheme.Size = new Size(90, 52);
            btnToggleTheme.TabIndex = 9;
            btnToggleTheme.Text = "Theme";
            btnToggleTheme.TextAlign = ContentAlignment.MiddleRight;
            btnToggleTheme.UseVisualStyleBackColor = true;
            btnToggleTheme.Click += btnToggleTheme_Click;
            // 
            // btnSave
            // 
            btnSave.Dock = DockStyle.Left;
            btnSave.FlatAppearance.BorderColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Image = (Image)resources.GetObject("btnSave.Image");
            btnSave.ImageAlign = ContentAlignment.MiddleLeft;
            btnSave.Location = new Point(360, 0);
            btnSave.Margin = new Padding(4, 3, 4, 3);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(90, 52);
            btnSave.TabIndex = 11;
            btnSave.Text = "Save\r\n";
            btnSave.TextAlign = ContentAlignment.MiddleRight;
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnReload
            // 
            btnReload.Dock = DockStyle.Left;
            btnReload.FlatAppearance.BorderColor = Color.White;
            btnReload.FlatStyle = FlatStyle.Flat;
            btnReload.Image = (Image)resources.GetObject("btnReload.Image");
            btnReload.ImageAlign = ContentAlignment.MiddleLeft;
            btnReload.Location = new Point(270, 0);
            btnReload.Margin = new Padding(4, 3, 4, 3);
            btnReload.Name = "btnReload";
            btnReload.Size = new Size(90, 52);
            btnReload.TabIndex = 12;
            btnReload.Text = "Reload";
            btnReload.TextAlign = ContentAlignment.MiddleRight;
            btnReload.UseVisualStyleBackColor = true;
            btnReload.Click += btnReload_Click;
            // 
            // btnRebuild
            // 
            btnRebuild.Dock = DockStyle.Left;
            btnRebuild.FlatAppearance.BorderColor = Color.White;
            btnRebuild.FlatStyle = FlatStyle.Flat;
            btnRebuild.Image = (Image)resources.GetObject("btnRebuild.Image");
            btnRebuild.ImageAlign = ContentAlignment.MiddleLeft;
            btnRebuild.Location = new Point(180, 0);
            btnRebuild.Margin = new Padding(4, 3, 4, 3);
            btnRebuild.Name = "btnRebuild";
            btnRebuild.Size = new Size(90, 52);
            btnRebuild.TabIndex = 10;
            btnRebuild.Text = "Rebuild";
            btnRebuild.TextAlign = ContentAlignment.MiddleRight;
            btnRebuild.UseVisualStyleBackColor = true;
            btnRebuild.Click += btnRebuild_Click;
            // 
            // btnRun
            // 
            btnRun.Dock = DockStyle.Left;
            btnRun.FlatAppearance.BorderColor = Color.White;
            btnRun.FlatStyle = FlatStyle.Flat;
            btnRun.Image = (Image)resources.GetObject("btnRun.Image");
            btnRun.ImageAlign = ContentAlignment.MiddleLeft;
            btnRun.Location = new Point(90, 0);
            btnRun.Margin = new Padding(4, 3, 4, 3);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(90, 52);
            btnRun.TabIndex = 13;
            btnRun.Text = "Run";
            btnRun.TextAlign = ContentAlignment.MiddleRight;
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += btnRun_Click;
            // 
            // btnStop
            // 
            btnStop.Dock = DockStyle.Left;
            btnStop.FlatAppearance.BorderColor = Color.White;
            btnStop.FlatStyle = FlatStyle.Flat;
            btnStop.Image = Properties.Resources.stop_icon_48p;
            btnStop.ImageAlign = ContentAlignment.MiddleLeft;
            btnStop.Location = new Point(0, 0);
            btnStop.Margin = new Padding(4, 3, 4, 3);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(90, 52);
            btnStop.TabIndex = 9;
            btnStop.Text = "Stop";
            btnStop.TextAlign = ContentAlignment.MiddleRight;
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // panelFooter
            // 
            panelFooter.Controls.Add(labelStatus);
            panelFooter.Dock = DockStyle.Fill;
            panelFooter.Location = new Point(0, 669);
            panelFooter.Margin = new Padding(0);
            panelFooter.Name = "panelFooter";
            panelFooter.Size = new Size(1577, 69);
            panelFooter.TabIndex = 2;
            // 
            // labelStatus
            // 
            labelStatus.Dock = DockStyle.Left;
            labelStatus.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelStatus.Location = new Point(0, 0);
            labelStatus.Margin = new Padding(4, 0, 4, 0);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(455, 69);
            labelStatus.TabIndex = 1;
            labelStatus.TextAlign = ContentAlignment.BottomLeft;
            // 
            // FormCodeExplorer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1577, 738);
            Controls.Add(LayoutRoot);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            Name = "FormCodeExplorer";
            Text = "qbookCode";
            WindowState = FormWindowState.Maximized;
            LayoutRoot.ResumeLayout(false);
            panelHeader.ResumeLayout(false);
            panelFooter.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel LayoutRoot;
        private System.Windows.Forms.Panel panelExplorer;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPageData;
        private System.Windows.Forms.Button btnToggleTheme;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.Button btnRebuild;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Panel panelFooter;
        private System.Windows.Forms.Label labelStatus;
        private Button btnStop;
    }
}