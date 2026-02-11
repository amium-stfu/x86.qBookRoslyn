namespace qbook
{
    partial class PopoutForm
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
            this.pageControl = new qbook.PageControl();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // pageControl
            // 
            this.pageControl.AllowDrop = true;
            this.pageControl.BackColor = System.Drawing.Color.White;
            this.pageControl.Location = new System.Drawing.Point(5, 6);
            this.pageControl.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.pageControl.Name = "pageControl";
            this.pageControl.Size = new System.Drawing.Size(491, 303);
            this.pageControl.TabIndex = 2;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // PopoutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 345);
            this.Controls.Add(this.pageControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "PopoutForm";
            this.Text = "PopoutForm";
            this.SizeChanged += new System.EventHandler(this.PopoutForm_SizeChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private PageControl pageControl;
        private System.Windows.Forms.Timer timer1;
    }
}