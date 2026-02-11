namespace qbook.ScintillaEditor
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
            this.LayoutRoot = new System.Windows.Forms.TableLayoutPanel();
            this.panelExplorer = new System.Windows.Forms.Panel();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.flowLayoutPageData = new System.Windows.Forms.FlowLayoutPanel();
            this.btnToggleTheme = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnReload = new System.Windows.Forms.Button();
            this.btnRebuild = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.panelFooter = new System.Windows.Forms.Panel();
            this.labelStatus = new System.Windows.Forms.Label();
            this.LayoutRoot.SuspendLayout();
            this.panelHeader.SuspendLayout();
            this.panelFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // LayoutRoot
            // 
            this.LayoutRoot.BackColor = System.Drawing.Color.White;
            this.LayoutRoot.ColumnCount = 1;
            this.LayoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LayoutRoot.Controls.Add(this.panelExplorer, 0, 1);
            this.LayoutRoot.Controls.Add(this.panelHeader, 0, 0);
            this.LayoutRoot.Controls.Add(this.panelFooter, 0, 2);
            this.LayoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LayoutRoot.Location = new System.Drawing.Point(0, 0);
            this.LayoutRoot.Name = "LayoutRoot";
            this.LayoutRoot.RowCount = 3;
            this.LayoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.LayoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LayoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.LayoutRoot.Size = new System.Drawing.Size(1352, 640);
            this.LayoutRoot.TabIndex = 0;
            // 
            // panelExplorer
            // 
            this.panelExplorer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelExplorer.Location = new System.Drawing.Point(0, 45);
            this.panelExplorer.Margin = new System.Windows.Forms.Padding(0);
            this.panelExplorer.Name = "panelExplorer";
            this.panelExplorer.Size = new System.Drawing.Size(1352, 535);
            this.panelExplorer.TabIndex = 0;
            // 
            // panelHeader
            // 
            this.panelHeader.Controls.Add(this.flowLayoutPageData);
            this.panelHeader.Controls.Add(this.btnToggleTheme);
            this.panelHeader.Controls.Add(this.btnSave);
            this.panelHeader.Controls.Add(this.btnReload);
            this.panelHeader.Controls.Add(this.btnRebuild);
            this.panelHeader.Controls.Add(this.btnRun);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Margin = new System.Windows.Forms.Padding(0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(1352, 45);
            this.panelHeader.TabIndex = 1;
            // 
            // flowLayoutPageData
            // 
            this.flowLayoutPageData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPageData.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPageData.Location = new System.Drawing.Point(432, 0);
            this.flowLayoutPageData.Margin = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.flowLayoutPageData.Name = "flowLayoutPageData";
            this.flowLayoutPageData.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.flowLayoutPageData.Size = new System.Drawing.Size(920, 45);
            this.flowLayoutPageData.TabIndex = 14;
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
            this.btnToggleTheme.Size = new System.Drawing.Size(118, 45);
            this.btnToggleTheme.TabIndex = 9;
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
            this.btnSave.Size = new System.Drawing.Size(74, 45);
            this.btnSave.TabIndex = 11;
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
            this.btnReload.Size = new System.Drawing.Size(81, 45);
            this.btnReload.TabIndex = 12;
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
            this.btnRebuild.Size = new System.Drawing.Size(89, 45);
            this.btnRebuild.TabIndex = 10;
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
            this.btnRun.Size = new System.Drawing.Size(70, 45);
            this.btnRun.TabIndex = 13;
            this.btnRun.Text = "Run";
            this.btnRun.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // panelFooter
            // 
            this.panelFooter.Controls.Add(this.labelStatus);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelFooter.Location = new System.Drawing.Point(0, 580);
            this.panelFooter.Margin = new System.Windows.Forms.Padding(0);
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Size = new System.Drawing.Size(1352, 60);
            this.panelFooter.TabIndex = 2;
            // 
            // labelStatus
            // 
            this.labelStatus.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(0, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(390, 60);
            this.labelStatus.TabIndex = 1;
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // FormCodeExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1352, 640);
            this.Controls.Add(this.LayoutRoot);
            this.Name = "FormCodeExplorer";
            this.Text = "FormCodeExplorer";
            this.LayoutRoot.ResumeLayout(false);
            this.panelHeader.ResumeLayout(false);
            this.panelFooter.ResumeLayout(false);
            this.ResumeLayout(false);

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
    }
}