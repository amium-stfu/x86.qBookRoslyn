
namespace qbook
{
    partial class TagForm1
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
            this.buttonEnter = new System.Windows.Forms.Button();
            this.buttonClear = new System.Windows.Forms.Button();
            this.buttonTR = new System.Windows.Forms.Button();
            this.buttonBR = new System.Windows.Forms.Button();
            this.buttonBL = new System.Windows.Forms.Button();
            this.buttonMR = new System.Windows.Forms.Button();
            this.buttonML = new System.Windows.Forms.Button();
            this.buttonTL = new System.Windows.Forms.Button();
            this.textBoxTag = new System.Windows.Forms.TextBox();
            this.listBoxTag = new System.Windows.Forms.ListBox();
            this.buttonTranslate = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonEnter
            // 
            this.buttonEnter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEnter.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonEnter.Location = new System.Drawing.Point(12, 12);
            this.buttonEnter.Name = "buttonEnter";
            this.buttonEnter.Size = new System.Drawing.Size(67, 29);
            this.buttonEnter.TabIndex = 1;
            this.buttonEnter.Text = "Enter";
            this.buttonEnter.UseVisualStyleBackColor = true;
            this.buttonEnter.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonClear
            // 
            this.buttonClear.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClear.Location = new System.Drawing.Point(12, 47);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(67, 29);
            this.buttonClear.TabIndex = 3;
            this.buttonClear.Text = "Delete";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // buttonTR
            // 
            this.buttonTR.BackColor = System.Drawing.SystemColors.Control;
            this.buttonTR.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonTR.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonTR.Location = new System.Drawing.Point(48, 139);
            this.buttonTR.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonTR.Name = "buttonTR";
            this.buttonTR.Size = new System.Drawing.Size(31, 23);
            this.buttonTR.TabIndex = 16;
            this.buttonTR.Text = "TR";
            this.buttonTR.UseVisualStyleBackColor = false;
            this.buttonTR.Click += new System.EventHandler(this.button_Click);
            // 
            // buttonBR
            // 
            this.buttonBR.BackColor = System.Drawing.SystemColors.Control;
            this.buttonBR.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonBR.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonBR.Location = new System.Drawing.Point(48, 193);
            this.buttonBR.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonBR.Name = "buttonBR";
            this.buttonBR.Size = new System.Drawing.Size(31, 23);
            this.buttonBR.TabIndex = 17;
            this.buttonBR.Text = "BR";
            this.buttonBR.UseVisualStyleBackColor = false;
            this.buttonBR.Click += new System.EventHandler(this.button_Click);
            // 
            // buttonBL
            // 
            this.buttonBL.BackColor = System.Drawing.SystemColors.Control;
            this.buttonBL.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonBL.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonBL.Location = new System.Drawing.Point(12, 193);
            this.buttonBL.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonBL.Name = "buttonBL";
            this.buttonBL.Size = new System.Drawing.Size(31, 23);
            this.buttonBL.TabIndex = 18;
            this.buttonBL.Text = "BL";
            this.buttonBL.UseVisualStyleBackColor = false;
            this.buttonBL.Click += new System.EventHandler(this.button_Click);
            // 
            // buttonMR
            // 
            this.buttonMR.BackColor = System.Drawing.SystemColors.Control;
            this.buttonMR.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMR.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonMR.Location = new System.Drawing.Point(48, 166);
            this.buttonMR.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonMR.Name = "buttonMR";
            this.buttonMR.Size = new System.Drawing.Size(31, 23);
            this.buttonMR.TabIndex = 19;
            this.buttonMR.Text = "MR";
            this.buttonMR.UseVisualStyleBackColor = false;
            this.buttonMR.Click += new System.EventHandler(this.button_Click);
            // 
            // buttonML
            // 
            this.buttonML.BackColor = System.Drawing.SystemColors.Control;
            this.buttonML.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonML.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonML.Location = new System.Drawing.Point(12, 166);
            this.buttonML.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonML.Name = "buttonML";
            this.buttonML.Size = new System.Drawing.Size(31, 23);
            this.buttonML.TabIndex = 20;
            this.buttonML.Text = "ML";
            this.buttonML.UseVisualStyleBackColor = false;
            this.buttonML.Click += new System.EventHandler(this.button_Click);
            // 
            // buttonTL
            // 
            this.buttonTL.BackColor = System.Drawing.SystemColors.Control;
            this.buttonTL.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonTL.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonTL.Location = new System.Drawing.Point(12, 139);
            this.buttonTL.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonTL.Name = "buttonTL";
            this.buttonTL.Size = new System.Drawing.Size(31, 23);
            this.buttonTL.TabIndex = 21;
            this.buttonTL.Text = "TL";
            this.buttonTL.UseVisualStyleBackColor = false;
            this.buttonTL.Click += new System.EventHandler(this.button_Click);
            // 
            // textBoxTag
            // 
            this.textBoxTag.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTag.BackColor = System.Drawing.Color.White;
            this.textBoxTag.Font = new System.Drawing.Font("Calibri", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxTag.Location = new System.Drawing.Point(84, 12);
            this.textBoxTag.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBoxTag.Multiline = true;
            this.textBoxTag.Name = "textBoxTag";
            this.textBoxTag.Size = new System.Drawing.Size(238, 120);
            this.textBoxTag.TabIndex = 15;
            this.textBoxTag.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxTag_KeyDown);
            // 
            // listBoxTag
            // 
            this.listBoxTag.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxTag.BackColor = System.Drawing.SystemColors.Control;
            this.listBoxTag.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxTag.FormattingEnabled = true;
            this.listBoxTag.ItemHeight = 19;
            this.listBoxTag.Location = new System.Drawing.Point(84, 136);
            this.listBoxTag.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listBoxTag.Name = "listBoxTag";
            this.listBoxTag.Size = new System.Drawing.Size(238, 61);
            this.listBoxTag.TabIndex = 22;
            this.listBoxTag.SelectedIndexChanged += new System.EventHandler(this.listBoxTag_SelectedIndexChanged);
            // 
            // buttonTranslate
            // 
            this.buttonTranslate.Font = new System.Drawing.Font("Calibri", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonTranslate.Location = new System.Drawing.Point(12, 83);
            this.buttonTranslate.Name = "buttonTranslate";
            this.buttonTranslate.Size = new System.Drawing.Size(67, 34);
            this.buttonTranslate.TabIndex = 23;
            this.buttonTranslate.Text = "Force translation";
            this.buttonTranslate.UseVisualStyleBackColor = true;
            this.buttonTranslate.Click += new System.EventHandler(this.buttonTranslate_Click);
            // 
            // TagForm1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.LightSlateGray;
            this.ClientSize = new System.Drawing.Size(333, 227);
            this.Controls.Add(this.buttonTranslate);
            this.Controls.Add(this.listBoxTag);
            this.Controls.Add(this.buttonTR);
            this.Controls.Add(this.buttonBR);
            this.Controls.Add(this.buttonBL);
            this.Controls.Add(this.buttonMR);
            this.Controls.Add(this.buttonML);
            this.Controls.Add(this.buttonTL);
            this.Controls.Add(this.textBoxTag);
            this.Controls.Add(this.buttonClear);
            this.Controls.Add(this.buttonEnter);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "TagForm1";
            this.Opacity = 0.9D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonEnter;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Button buttonTR;
        private System.Windows.Forms.Button buttonBR;
        private System.Windows.Forms.Button buttonBL;
        private System.Windows.Forms.Button buttonMR;
        private System.Windows.Forms.Button buttonML;
        private System.Windows.Forms.Button buttonTL;
        private System.Windows.Forms.TextBox textBoxTag;
        private System.Windows.Forms.ListBox listBoxTag;
        private System.Windows.Forms.Button buttonTranslate;
    }
}