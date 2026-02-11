
namespace qbook
{
    partial class ModuleControl
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
            this.textBoxDummy = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBoxDummy
            // 
            this.textBoxDummy.BackColor = System.Drawing.Color.LightBlue;
            this.textBoxDummy.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxDummy.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(254)));
            this.textBoxDummy.Location = new System.Drawing.Point(0, 0);
            this.textBoxDummy.Multiline = true;
            this.textBoxDummy.Name = "textBoxDummy";
            this.textBoxDummy.Size = new System.Drawing.Size(1, 1);
            this.textBoxDummy.TabIndex = 2;
            this.textBoxDummy.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ModuleControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxDummy);
            this.Margin = new System.Windows.Forms.Padding(0, 0, 0, 20);
            this.Name = "ModuleControl";
            this.Size = new System.Drawing.Size(95, 200);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ModuleControl_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBoxDummy;
    }
}
