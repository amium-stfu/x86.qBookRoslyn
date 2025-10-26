
namespace qbook.oControls
{
    partial class oHtmlSettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(oHtmlSettingsForm));
            this.textBoxCss = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.radioButtonLog = new System.Windows.Forms.RadioButton();
            this.radioButtonCss = new System.Windows.Forms.RadioButton();
            this.radioButtonHtml = new System.Windows.Forms.RadioButton();
            this.textBoxHtml = new System.Windows.Forms.TextBox();
            this.textBoxSettings = new System.Windows.Forms.TextBox();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.listBoxLog = new System.Windows.Forms.ListBox();
            this.checkBoxScroll = new System.Windows.Forms.CheckBox();
            this.buttonLogToClip = new System.Windows.Forms.Button();
            this.buttonLogClear = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxCss
            // 
            this.textBoxCss.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCss.BackColor = System.Drawing.Color.AntiqueWhite;
            this.textBoxCss.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxCss.Location = new System.Drawing.Point(3, 28);
            this.textBoxCss.Multiline = true;
            this.textBoxCss.Name = "textBoxCss";
            this.textBoxCss.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxCss.Size = new System.Drawing.Size(962, 429);
            this.textBoxCss.TabIndex = 15;
            this.textBoxCss.Text = resources.GetString("textBoxCss.Text");
            this.textBoxCss.Visible = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radioButtonLog);
            this.panel1.Controls.Add(this.radioButtonCss);
            this.panel1.Controls.Add(this.radioButtonHtml);
            this.panel1.Location = new System.Drawing.Point(3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(163, 24);
            this.panel1.TabIndex = 14;
            // 
            // radioButtonLog
            // 
            this.radioButtonLog.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioButtonLog.AutoSize = true;
            this.radioButtonLog.BackColor = System.Drawing.Color.Plum;
            this.radioButtonLog.Location = new System.Drawing.Point(92, 1);
            this.radioButtonLog.Name = "radioButtonLog";
            this.radioButtonLog.Size = new System.Drawing.Size(35, 23);
            this.radioButtonLog.TabIndex = 2;
            this.radioButtonLog.Text = "Log";
            this.radioButtonLog.UseVisualStyleBackColor = false;
            this.radioButtonLog.CheckedChanged += new System.EventHandler(this.radioButtonLog_CheckedChanged);
            // 
            // radioButtonCss
            // 
            this.radioButtonCss.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioButtonCss.AutoSize = true;
            this.radioButtonCss.BackColor = System.Drawing.Color.AntiqueWhite;
            this.radioButtonCss.Location = new System.Drawing.Point(48, 0);
            this.radioButtonCss.Name = "radioButtonCss";
            this.radioButtonCss.Size = new System.Drawing.Size(38, 23);
            this.radioButtonCss.TabIndex = 1;
            this.radioButtonCss.Text = "CSS";
            this.radioButtonCss.UseVisualStyleBackColor = false;
            this.radioButtonCss.CheckedChanged += new System.EventHandler(this.radioButtonCss_CheckedChanged);
            // 
            // radioButtonHtml
            // 
            this.radioButtonHtml.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioButtonHtml.AutoSize = true;
            this.radioButtonHtml.BackColor = System.Drawing.Color.LightSteelBlue;
            this.radioButtonHtml.Checked = true;
            this.radioButtonHtml.Location = new System.Drawing.Point(0, 0);
            this.radioButtonHtml.Name = "radioButtonHtml";
            this.radioButtonHtml.Size = new System.Drawing.Size(47, 23);
            this.radioButtonHtml.TabIndex = 0;
            this.radioButtonHtml.TabStop = true;
            this.radioButtonHtml.Text = "HTML";
            this.radioButtonHtml.UseVisualStyleBackColor = false;
            this.radioButtonHtml.CheckedChanged += new System.EventHandler(this.radioButtonHtml_CheckedChanged);
            // 
            // textBoxHtml
            // 
            this.textBoxHtml.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxHtml.BackColor = System.Drawing.Color.LightSteelBlue;
            this.textBoxHtml.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxHtml.Location = new System.Drawing.Point(5, 28);
            this.textBoxHtml.Multiline = true;
            this.textBoxHtml.Name = "textBoxHtml";
            this.textBoxHtml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxHtml.Size = new System.Drawing.Size(960, 429);
            this.textBoxHtml.TabIndex = 13;
            this.textBoxHtml.Text = resources.GetString("textBoxHtml.Text");
            // 
            // textBoxSettings
            // 
            this.textBoxSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSettings.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxSettings.Location = new System.Drawing.Point(3, 463);
            this.textBoxSettings.Multiline = true;
            this.textBoxSettings.Name = "textBoxSettings";
            this.textBoxSettings.Size = new System.Drawing.Size(962, 117);
            this.textBoxSettings.TabIndex = 12;
            this.textBoxSettings.Text = resources.GetString("textBoxSettings.Text");
            // 
            // buttonApply
            // 
            this.buttonApply.Location = new System.Drawing.Point(890, 3);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(75, 23);
            this.buttonApply.TabIndex = 16;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // buttonDelete
            // 
            this.buttonDelete.Location = new System.Drawing.Point(809, 3);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(75, 23);
            this.buttonDelete.TabIndex = 17;
            this.buttonDelete.Text = "[ x ] Delete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // listBoxLog
            // 
            this.listBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxLog.BackColor = System.Drawing.Color.Thistle;
            this.listBoxLog.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxLog.FormattingEnabled = true;
            this.listBoxLog.ItemHeight = 14;
            this.listBoxLog.Location = new System.Drawing.Point(3, 28);
            this.listBoxLog.Name = "listBoxLog";
            this.listBoxLog.Size = new System.Drawing.Size(962, 424);
            this.listBoxLog.TabIndex = 19;
            // 
            // checkBoxScroll
            // 
            this.checkBoxScroll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxScroll.AutoSize = true;
            this.checkBoxScroll.BackColor = System.Drawing.Color.Thistle;
            this.checkBoxScroll.Checked = true;
            this.checkBoxScroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxScroll.Location = new System.Drawing.Point(919, 33);
            this.checkBoxScroll.Name = "checkBoxScroll";
            this.checkBoxScroll.Size = new System.Drawing.Size(44, 17);
            this.checkBoxScroll.TabIndex = 20;
            this.checkBoxScroll.Text = "end";
            this.checkBoxScroll.UseVisualStyleBackColor = false;
            // 
            // buttonLogToClip
            // 
            this.buttonLogToClip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLogToClip.Location = new System.Drawing.Point(882, 30);
            this.buttonLogToClip.Name = "buttonLogToClip";
            this.buttonLogToClip.Size = new System.Drawing.Size(35, 21);
            this.buttonLogToClip.TabIndex = 21;
            this.buttonLogToClip.Text = "clip";
            this.buttonLogToClip.UseVisualStyleBackColor = true;
            this.buttonLogToClip.Click += new System.EventHandler(this.buttonLogToClip_Click);
            // 
            // buttonLogClear
            // 
            this.buttonLogClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLogClear.Location = new System.Drawing.Point(846, 30);
            this.buttonLogClear.Name = "buttonLogClear";
            this.buttonLogClear.Size = new System.Drawing.Size(35, 21);
            this.buttonLogClear.TabIndex = 22;
            this.buttonLogClear.Text = "clr";
            this.buttonLogClear.UseVisualStyleBackColor = true;
            this.buttonLogClear.Click += new System.EventHandler(this.buttonLogClear_Click);
            // 
            // oHtmlSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(968, 583);
            this.Controls.Add(this.buttonLogClear);
            this.Controls.Add(this.buttonLogToClip);
            this.Controls.Add(this.checkBoxScroll);
            this.Controls.Add(this.listBoxLog);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.textBoxCss);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.textBoxHtml);
            this.Controls.Add(this.textBoxSettings);
            this.Name = "oHtmlSettingsForm";
            this.Text = "oHtmlSettingsForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.oHtmlSettingsForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.oHtmlSettingsForm_FormClosed);
            this.Load += new System.EventHandler(this.oHtmlSettingsForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxCss;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton radioButtonCss;
        private System.Windows.Forms.RadioButton radioButtonHtml;
        private System.Windows.Forms.TextBox textBoxHtml;
        private System.Windows.Forms.TextBox textBoxSettings;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.RadioButton radioButtonLog;
        private System.Windows.Forms.ListBox listBoxLog;
        private System.Windows.Forms.CheckBox checkBoxScroll;
        private System.Windows.Forms.Button buttonLogToClip;
        private System.Windows.Forms.Button buttonLogClear;
    }
}