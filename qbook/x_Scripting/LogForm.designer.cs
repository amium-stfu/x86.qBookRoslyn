namespace qbook
{
    partial class LogForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogForm));
            this.checkBoxScroll = new System.Windows.Forms.CheckBox();
            this.timerIdle = new System.Windows.Forms.Timer(this.components);
            this.buttonClear = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.checkBoxShowExtendedInfo = new System.Windows.Forms.CheckBox();
            this.buttonCopyToClip = new System.Windows.Forms.Button();
            this.checkBoxL = new System.Windows.Forms.CheckBox();
            this.checkBoxD = new System.Windows.Forms.CheckBox();
            this.checkBoxI = new System.Windows.Forms.CheckBox();
            this.checkBoxW = new System.Windows.Forms.CheckBox();
            this.checkBoxE = new System.Windows.Forms.CheckBox();
            this.buttonClearFilter = new System.Windows.Forms.Button();
            this.checkBoxFilterIsRegex = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxFilter = new System.Windows.Forms.TextBox();
            this.listControl = new qbook.ListControl();
            this.listBoxLog = new qbook.ListBoxEx();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBoxScroll
            // 
            this.checkBoxScroll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxScroll.AutoSize = true;
            this.checkBoxScroll.Checked = true;
            this.checkBoxScroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxScroll.Location = new System.Drawing.Point(1214, 6);
            this.checkBoxScroll.Name = "checkBoxScroll";
            this.checkBoxScroll.Size = new System.Drawing.Size(50, 17);
            this.checkBoxScroll.TabIndex = 1;
            this.checkBoxScroll.Text = "scroll";
            this.checkBoxScroll.UseVisualStyleBackColor = true;
            // 
            // timerIdle
            // 
            this.timerIdle.Enabled = true;
            this.timerIdle.Interval = 250;
            this.timerIdle.Tick += new System.EventHandler(this.timerIdle_Tick);
            // 
            // buttonClear
            // 
            this.buttonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClear.Location = new System.Drawing.Point(1266, 2);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(50, 23);
            this.buttonClear.TabIndex = 2;
            this.buttonClear.Text = "clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightSkyBlue;
            this.panel1.Controls.Add(this.checkBoxShowExtendedInfo);
            this.panel1.Controls.Add(this.buttonCopyToClip);
            this.panel1.Controls.Add(this.checkBoxL);
            this.panel1.Controls.Add(this.checkBoxD);
            this.panel1.Controls.Add(this.checkBoxI);
            this.panel1.Controls.Add(this.checkBoxW);
            this.panel1.Controls.Add(this.checkBoxE);
            this.panel1.Controls.Add(this.buttonClearFilter);
            this.panel1.Controls.Add(this.checkBoxFilterIsRegex);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.textBoxFilter);
            this.panel1.Controls.Add(this.checkBoxScroll);
            this.panel1.Controls.Add(this.buttonClear);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1367, 27);
            this.panel1.TabIndex = 4;
            // 
            // checkBoxShowExtendedInfo
            // 
            this.checkBoxShowExtendedInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxShowExtendedInfo.AutoSize = true;
            this.checkBoxShowExtendedInfo.Location = new System.Drawing.Point(761, 6);
            this.checkBoxShowExtendedInfo.Name = "checkBoxShowExtendedInfo";
            this.checkBoxShowExtendedInfo.Size = new System.Drawing.Size(58, 17);
            this.checkBoxShowExtendedInfo.TabIndex = 13;
            this.checkBoxShowExtendedInfo.Text = "ExInfo";
            this.checkBoxShowExtendedInfo.UseVisualStyleBackColor = true;
            this.checkBoxShowExtendedInfo.CheckedChanged += new System.EventHandler(this.checkBoxShowExtendedInfo_CheckedChanged);
            // 
            // buttonCopyToClip
            // 
            this.buttonCopyToClip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCopyToClip.Location = new System.Drawing.Point(1315, 2);
            this.buttonCopyToClip.Name = "buttonCopyToClip";
            this.buttonCopyToClip.Size = new System.Drawing.Size(50, 23);
            this.buttonCopyToClip.TabIndex = 12;
            this.buttonCopyToClip.Text = "clip";
            this.buttonCopyToClip.UseVisualStyleBackColor = true;
            this.buttonCopyToClip.Click += new System.EventHandler(this.buttonCopyToClip_Click);
            // 
            // checkBoxL
            // 
            this.checkBoxL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxL.AutoSize = true;
            this.checkBoxL.Checked = true;
            this.checkBoxL.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxL.Location = new System.Drawing.Point(566, 6);
            this.checkBoxL.Name = "checkBoxL";
            this.checkBoxL.Size = new System.Drawing.Size(31, 17);
            this.checkBoxL.TabIndex = 11;
            this.checkBoxL.Text = "L";
            this.checkBoxL.UseVisualStyleBackColor = true;
            this.checkBoxL.CheckedChanged += new System.EventHandler(this.checkBoxTypes_CheckedChanged);
            // 
            // checkBoxD
            // 
            this.checkBoxD.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxD.AutoSize = true;
            this.checkBoxD.Location = new System.Drawing.Point(715, 6);
            this.checkBoxD.Name = "checkBoxD";
            this.checkBoxD.Size = new System.Drawing.Size(33, 17);
            this.checkBoxD.TabIndex = 10;
            this.checkBoxD.Text = "D";
            this.checkBoxD.UseVisualStyleBackColor = true;
            this.checkBoxD.CheckedChanged += new System.EventHandler(this.checkBoxTypes_CheckedChanged);
            // 
            // checkBoxI
            // 
            this.checkBoxI.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxI.AutoSize = true;
            this.checkBoxI.Checked = true;
            this.checkBoxI.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxI.Location = new System.Drawing.Point(680, 6);
            this.checkBoxI.Name = "checkBoxI";
            this.checkBoxI.Size = new System.Drawing.Size(30, 17);
            this.checkBoxI.TabIndex = 9;
            this.checkBoxI.Text = "I";
            this.checkBoxI.UseVisualStyleBackColor = true;
            this.checkBoxI.CheckedChanged += new System.EventHandler(this.checkBoxTypes_CheckedChanged);
            // 
            // checkBoxW
            // 
            this.checkBoxW.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxW.AutoSize = true;
            this.checkBoxW.Checked = true;
            this.checkBoxW.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxW.Location = new System.Drawing.Point(639, 6);
            this.checkBoxW.Name = "checkBoxW";
            this.checkBoxW.Size = new System.Drawing.Size(36, 17);
            this.checkBoxW.TabIndex = 8;
            this.checkBoxW.Text = "W";
            this.checkBoxW.UseVisualStyleBackColor = true;
            this.checkBoxW.CheckedChanged += new System.EventHandler(this.checkBoxTypes_CheckedChanged);
            // 
            // checkBoxE
            // 
            this.checkBoxE.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxE.AutoSize = true;
            this.checkBoxE.Checked = true;
            this.checkBoxE.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxE.Location = new System.Drawing.Point(602, 6);
            this.checkBoxE.Name = "checkBoxE";
            this.checkBoxE.Size = new System.Drawing.Size(32, 17);
            this.checkBoxE.TabIndex = 7;
            this.checkBoxE.Text = "E";
            this.checkBoxE.UseVisualStyleBackColor = true;
            this.checkBoxE.CheckedChanged += new System.EventHandler(this.checkBoxTypes_CheckedChanged);
            // 
            // buttonClearFilter
            // 
            this.buttonClearFilter.Location = new System.Drawing.Point(463, 2);
            this.buttonClearFilter.Name = "buttonClearFilter";
            this.buttonClearFilter.Size = new System.Drawing.Size(24, 23);
            this.buttonClearFilter.TabIndex = 6;
            this.buttonClearFilter.Text = "X";
            this.buttonClearFilter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonClearFilter.UseVisualStyleBackColor = true;
            this.buttonClearFilter.Click += new System.EventHandler(this.buttonClearFilter_Click);
            // 
            // checkBoxFilterIsRegex
            // 
            this.checkBoxFilterIsRegex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxFilterIsRegex.AutoSize = true;
            this.checkBoxFilterIsRegex.Location = new System.Drawing.Point(495, 6);
            this.checkBoxFilterIsRegex.Name = "checkBoxFilterIsRegex";
            this.checkBoxFilterIsRegex.Size = new System.Drawing.Size(54, 17);
            this.checkBoxFilterIsRegex.TabIndex = 5;
            this.checkBoxFilterIsRegex.Text = "regex";
            this.checkBoxFilterIsRegex.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Filter";
            // 
            // textBoxFilter
            // 
            this.textBoxFilter.Location = new System.Drawing.Point(39, 3);
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.Size = new System.Drawing.Size(425, 21);
            this.textBoxFilter.TabIndex = 3;
            this.textBoxFilter.TextChanged += new System.EventHandler(this.textBoxFilter_TextChanged);
            // 
            // listControl
            // 
            this.listControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listControl.Location = new System.Drawing.Point(0, 27);
            this.listControl.Name = "listControl";
            this.listControl.Size = new System.Drawing.Size(1367, 728);
            this.listControl.TabIndex = 3;
            this.listControl.Scroll += new System.Windows.Forms.ScrollEventHandler(this.listControl_Scroll);
            // 
            // listBoxLog
            // 
            this.listBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxLog.BackColor = System.Drawing.Color.Yellow;
            this.listBoxLog.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxLog.FormattingEnabled = true;
            this.listBoxLog.IntegralHeight = false;
            this.listBoxLog.ItemHeight = 14;
            this.listBoxLog.Location = new System.Drawing.Point(338, 109);
            this.listBoxLog.Name = "listBoxLog";
            this.listBoxLog.Size = new System.Drawing.Size(575, 525);
            this.listBoxLog.TabIndex = 0;
            this.listBoxLog.Visible = false;
            // 
            // LogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1367, 755);
            this.Controls.Add(this.listControl);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.listBoxLog);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LogForm";
            this.Text = "LOG";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ScriptingLogForm_FormClosing);
            this.Load += new System.EventHandler(this.LogForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private qbook.ListBoxEx listBoxLog;
        private System.Windows.Forms.CheckBox checkBoxScroll;
        private System.Windows.Forms.Timer timerIdle;
        private System.Windows.Forms.Button buttonClear;
        private ListControl listControl;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox checkBoxFilterIsRegex;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxFilter;
        private System.Windows.Forms.Button buttonClearFilter;
        private System.Windows.Forms.CheckBox checkBoxL;
        private System.Windows.Forms.CheckBox checkBoxD;
        private System.Windows.Forms.CheckBox checkBoxI;
        private System.Windows.Forms.CheckBox checkBoxW;
        private System.Windows.Forms.CheckBox checkBoxE;
        private System.Windows.Forms.Button buttonCopyToClip;
        private System.Windows.Forms.CheckBox checkBoxShowExtendedInfo;
    }
}