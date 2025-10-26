namespace qbook_publisher
{
    partial class FormMain
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
            this.dgvRemoteFiles = new System.Windows.Forms.DataGridView();
            this.dgvLocalFiles = new System.Windows.Forms.DataGridView();
            this.buttonGetRemoteDir = new System.Windows.Forms.Button();
            this.buttonDeleteSelected = new System.Windows.Forms.Button();
            this.buttonUploadSelected = new System.Windows.Forms.Button();
            this.panelStatus = new System.Windows.Forms.Panel();
            this.labelStatus = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.textBoxSetupPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxNextCloudUri = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxRemotePath = new System.Windows.Forms.TextBox();
            this.textBoxNextCloudUriEx = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRemoteFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLocalFiles)).BeginInit();
            this.panelStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvRemoteFiles
            // 
            this.dgvRemoteFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRemoteFiles.Location = new System.Drawing.Point(6, 341);
            this.dgvRemoteFiles.Name = "dgvRemoteFiles";
            this.dgvRemoteFiles.Size = new System.Drawing.Size(776, 294);
            this.dgvRemoteFiles.TabIndex = 0;
            // 
            // dgvLocalFiles
            // 
            this.dgvLocalFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLocalFiles.Location = new System.Drawing.Point(6, 38);
            this.dgvLocalFiles.Name = "dgvLocalFiles";
            this.dgvLocalFiles.Size = new System.Drawing.Size(776, 257);
            this.dgvLocalFiles.TabIndex = 1;
            // 
            // buttonGetRemoteDir
            // 
            this.buttonGetRemoteDir.Location = new System.Drawing.Point(788, 358);
            this.buttonGetRemoteDir.Name = "buttonGetRemoteDir";
            this.buttonGetRemoteDir.Size = new System.Drawing.Size(131, 51);
            this.buttonGetRemoteDir.TabIndex = 2;
            this.buttonGetRemoteDir.Text = "get remote (drive.amium.at)";
            this.buttonGetRemoteDir.UseVisualStyleBackColor = true;
            this.buttonGetRemoteDir.Click += new System.EventHandler(this.buttonGetRemoteDir_Click);
            // 
            // buttonDeleteSelected
            // 
            this.buttonDeleteSelected.Location = new System.Drawing.Point(788, 415);
            this.buttonDeleteSelected.Name = "buttonDeleteSelected";
            this.buttonDeleteSelected.Size = new System.Drawing.Size(131, 23);
            this.buttonDeleteSelected.TabIndex = 3;
            this.buttonDeleteSelected.Text = "delete selected";
            this.buttonDeleteSelected.UseVisualStyleBackColor = true;
            this.buttonDeleteSelected.Click += new System.EventHandler(this.buttonDeleteSelected_Click);
            // 
            // buttonUploadSelected
            // 
            this.buttonUploadSelected.Location = new System.Drawing.Point(788, 50);
            this.buttonUploadSelected.Name = "buttonUploadSelected";
            this.buttonUploadSelected.Size = new System.Drawing.Size(131, 23);
            this.buttonUploadSelected.TabIndex = 4;
            this.buttonUploadSelected.Text = "upload selected";
            this.buttonUploadSelected.UseVisualStyleBackColor = true;
            this.buttonUploadSelected.Click += new System.EventHandler(this.buttonUploadSelected_Click);
            // 
            // panelStatus
            // 
            this.panelStatus.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.panelStatus.Controls.Add(this.labelStatus);
            this.panelStatus.Controls.Add(this.progressBar);
            this.panelStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelStatus.Location = new System.Drawing.Point(0, 641);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Size = new System.Drawing.Size(937, 26);
            this.panelStatus.TabIndex = 5;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(3, 7);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(33, 13);
            this.labelStatus.TabIndex = 1;
            this.labelStatus.Text = "ready";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(606, 3);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(328, 20);
            this.progressBar.TabIndex = 0;
            // 
            // textBoxSetupPath
            // 
            this.textBoxSetupPath.Location = new System.Drawing.Point(75, 12);
            this.textBoxSetupPath.Name = "textBoxSetupPath";
            this.textBoxSetupPath.Size = new System.Drawing.Size(707, 20);
            this.textBoxSetupPath.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "\'setup\' path:";
            // 
            // textBoxNextCloudUri
            // 
            this.textBoxNextCloudUri.Location = new System.Drawing.Point(65, 315);
            this.textBoxNextCloudUri.Name = "textBoxNextCloudUri";
            this.textBoxNextCloudUri.Size = new System.Drawing.Size(126, 20);
            this.textBoxNextCloudUri.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 318);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "NextCloud:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(384, 318);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Remote Path:";
            // 
            // textBoxRemotePath
            // 
            this.textBoxRemotePath.Location = new System.Drawing.Point(459, 315);
            this.textBoxRemotePath.Name = "textBoxRemotePath";
            this.textBoxRemotePath.Size = new System.Drawing.Size(323, 20);
            this.textBoxRemotePath.TabIndex = 9;
            // 
            // textBoxNextCloudUriEx
            // 
            this.textBoxNextCloudUriEx.Location = new System.Drawing.Point(195, 315);
            this.textBoxNextCloudUriEx.Name = "textBoxNextCloudUriEx";
            this.textBoxNextCloudUriEx.Size = new System.Drawing.Size(181, 20);
            this.textBoxNextCloudUriEx.TabIndex = 11;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(937, 667);
            this.Controls.Add(this.textBoxNextCloudUriEx);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxRemotePath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxNextCloudUri);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxSetupPath);
            this.Controls.Add(this.panelStatus);
            this.Controls.Add(this.buttonUploadSelected);
            this.Controls.Add(this.buttonDeleteSelected);
            this.Controls.Add(this.buttonGetRemoteDir);
            this.Controls.Add(this.dgvLocalFiles);
            this.Controls.Add(this.dgvRemoteFiles);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormMain";
            this.Text = "qbook publish";
            this.Load += new System.EventHandler(this.FormMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRemoteFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLocalFiles)).EndInit();
            this.panelStatus.ResumeLayout(false);
            this.panelStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvRemoteFiles;
        private System.Windows.Forms.DataGridView dgvLocalFiles;
        private System.Windows.Forms.Button buttonGetRemoteDir;
        private System.Windows.Forms.Button buttonDeleteSelected;
        private System.Windows.Forms.Button buttonUploadSelected;
        private System.Windows.Forms.Panel panelStatus;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox textBoxSetupPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxNextCloudUri;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxRemotePath;
        private System.Windows.Forms.TextBox textBoxNextCloudUriEx;
    }
}

