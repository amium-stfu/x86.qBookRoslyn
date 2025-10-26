
namespace qbook
{
    partial class ImageControl
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
            this.SuspendLayout();
            // 
            // ImageControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Name = "ImageControl";
            this.Size = new System.Drawing.Size(293, 291);
            this.Load += new System.EventHandler(this.ImageControl_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ImageControl_Paint);
            this.DoubleClick += new System.EventHandler(this.ImageControl_DoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ImageControl_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ImageControl_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ImageControl_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
