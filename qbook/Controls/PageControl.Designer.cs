
namespace qbook
{
    partial class PageControl
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
            // PageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Name = "PageControl";
            this.Size = new System.Drawing.Size(766, 459);
            this.SizeChanged += new System.EventHandler(this.PageControl_SizeChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.PageControl_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PageControl_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Page_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PageControl_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PageControl_MouseUp);
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.PageControl_PreviewKeyDown);
            this.ResumeLayout(false);

        }

        

        #endregion
    }
}
