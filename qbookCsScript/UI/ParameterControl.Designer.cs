namespace QB.UI
{
    partial class ParameterControl
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelName = new System.Windows.Forms.Label();
            this.textBoxValue = new System.Windows.Forms.TextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.labelUnit = new System.Windows.Forms.Label();
            this.labelValueRule = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.labelName.Location = new System.Drawing.Point(4, 1);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(53, 21);
            this.labelName.TabIndex = 0;
            this.labelName.Text = "Name";
            // 
            // textBoxValue
            // 
            this.textBoxValue.Font = new System.Drawing.Font("Tahoma", 14F);
            this.textBoxValue.Location = new System.Drawing.Point(3, 23);
            this.textBoxValue.Name = "textBoxValue";
            this.textBoxValue.Size = new System.Drawing.Size(156, 41);
            this.textBoxValue.TabIndex = 1;
            this.textBoxValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Italic);
            this.labelDescription.Location = new System.Drawing.Point(181, 0);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(94, 21);
            this.labelDescription.TabIndex = 2;
            this.labelDescription.Text = "Description";
            // 
            // labelUnit
            // 
            this.labelUnit.AutoSize = true;
            this.labelUnit.Font = new System.Drawing.Font("Tahoma", 12F);
            this.labelUnit.Location = new System.Drawing.Point(165, 23);
            this.labelUnit.Name = "labelUnit";
            this.labelUnit.Size = new System.Drawing.Size(43, 29);
            this.labelUnit.TabIndex = 3;
            this.labelUnit.Text = "[x]";
            // 
            // labelValueRule
            // 
            this.labelValueRule.AutoSize = true;
            this.labelValueRule.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.labelValueRule.Location = new System.Drawing.Point(214, 37);
            this.labelValueRule.Name = "labelValueRule";
            this.labelValueRule.Size = new System.Drawing.Size(89, 21);
            this.labelValueRule.TabIndex = 4;
            this.labelValueRule.Text = "Value Rule";
            // 
            // ParameterControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelValueRule);
            this.Controls.Add(this.labelUnit);
            this.Controls.Add(this.labelDescription);
            this.Controls.Add(this.textBoxValue);
            this.Controls.Add(this.labelName);
            this.Name = "ParameterControl";
            this.Size = new System.Drawing.Size(288, 68);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox textBoxValue;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Label labelUnit;
        private System.Windows.Forms.Label labelValueRule;
    }
}
