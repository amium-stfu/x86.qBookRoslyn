
namespace qbook
{
    partial class NewPageForm
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
            this.buttonClear = new System.Windows.Forms.Button();
            this.panelObjects = new System.Windows.Forms.Panel();
            this.buttonAkc = new System.Windows.Forms.Button();
            this.buttonScratch = new System.Windows.Forms.Button();
            this.buttonPage = new System.Windows.Forms.Button();
            this.buttonGrid = new System.Windows.Forms.Button();
            this.buttonText = new System.Windows.Forms.Button();
            this.buttonUdl = new System.Windows.Forms.Button();
            this.panelObjects.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonClear
            // 
            this.buttonClear.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClear.Location = new System.Drawing.Point(3, 83);
            this.buttonClear.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(352, 45);
            this.buttonClear.TabIndex = 3;
            this.buttonClear.Text = "Cancel";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // panelObjects
            // 
            this.panelObjects.Controls.Add(this.buttonAkc);
            this.panelObjects.Controls.Add(this.buttonClear);
            this.panelObjects.Controls.Add(this.buttonScratch);
            this.panelObjects.Controls.Add(this.buttonPage);
            this.panelObjects.Controls.Add(this.buttonGrid);
            this.panelObjects.Controls.Add(this.buttonText);
            this.panelObjects.Controls.Add(this.buttonUdl);
            this.panelObjects.Location = new System.Drawing.Point(2, 5);
            this.panelObjects.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelObjects.Name = "panelObjects";
            this.panelObjects.Size = new System.Drawing.Size(356, 228);
            this.panelObjects.TabIndex = 25;
            // 
            // buttonAkc
            // 
            this.buttonAkc.BackColor = System.Drawing.SystemColors.Control;
            this.buttonAkc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonAkc.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAkc.Location = new System.Drawing.Point(262, 40);
            this.buttonAkc.Name = "buttonAkc";
            this.buttonAkc.Size = new System.Drawing.Size(81, 35);
            this.buttonAkc.TabIndex = 22;
            this.buttonAkc.Text = "AKC";
            this.buttonAkc.UseVisualStyleBackColor = true;
            this.buttonAkc.Visible = false;
            // 
            // buttonScratch
            // 
            this.buttonScratch.BackColor = System.Drawing.SystemColors.Control;
            this.buttonScratch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonScratch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonScratch.Location = new System.Drawing.Point(3, 148);
            this.buttonScratch.Name = "buttonScratch";
            this.buttonScratch.Size = new System.Drawing.Size(81, 35);
            this.buttonScratch.TabIndex = 21;
            this.buttonScratch.Text = "Scratch";
            this.buttonScratch.UseVisualStyleBackColor = false;
            this.buttonScratch.Visible = false;
            this.buttonScratch.Click += new System.EventHandler(this.buttonScratch_Click);
            // 
            // buttonPage
            // 
            this.buttonPage.BackColor = System.Drawing.SystemColors.Control;
            this.buttonPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPage.Location = new System.Drawing.Point(3, 3);
            this.buttonPage.Name = "buttonPage";
            this.buttonPage.Size = new System.Drawing.Size(231, 72);
            this.buttonPage.TabIndex = 21;
            this.buttonPage.Text = "Default Page";
            this.buttonPage.UseVisualStyleBackColor = false;
            this.buttonPage.Click += new System.EventHandler(this.buttonPage_Click);
            // 
            // buttonGrid
            // 
            this.buttonGrid.BackColor = System.Drawing.SystemColors.Control;
            this.buttonGrid.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonGrid.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonGrid.Location = new System.Drawing.Point(177, 148);
            this.buttonGrid.Name = "buttonGrid";
            this.buttonGrid.Size = new System.Drawing.Size(81, 35);
            this.buttonGrid.TabIndex = 21;
            this.buttonGrid.Text = "Grid";
            this.buttonGrid.UseVisualStyleBackColor = false;
            this.buttonGrid.Visible = false;
            this.buttonGrid.Click += new System.EventHandler(this.buttonGrid_Click);
            // 
            // buttonText
            // 
            this.buttonText.BackColor = System.Drawing.SystemColors.Control;
            this.buttonText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonText.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonText.Location = new System.Drawing.Point(90, 148);
            this.buttonText.Name = "buttonText";
            this.buttonText.Size = new System.Drawing.Size(81, 35);
            this.buttonText.TabIndex = 21;
            this.buttonText.Text = "Text";
            this.buttonText.UseVisualStyleBackColor = false;
            this.buttonText.Visible = false;
            this.buttonText.Click += new System.EventHandler(this.buttonText_Click);
            // 
            // buttonUdl
            // 
            this.buttonUdl.BackColor = System.Drawing.SystemColors.Control;
            this.buttonUdl.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonUdl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonUdl.Location = new System.Drawing.Point(262, 3);
            this.buttonUdl.Name = "buttonUdl";
            this.buttonUdl.Size = new System.Drawing.Size(81, 35);
            this.buttonUdl.TabIndex = 21;
            this.buttonUdl.Text = "Udl";
            this.buttonUdl.UseVisualStyleBackColor = true;
            this.buttonUdl.Visible = false;
            // 
            // NewPageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.LightSlateGray;
            this.ClientSize = new System.Drawing.Size(362, 158);
            this.Controls.Add(this.panelObjects);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "NewPageForm";
            this.Opacity = 0.9D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.panelObjects.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Panel panelObjects;
        private System.Windows.Forms.Button buttonGrid;
        private System.Windows.Forms.Button buttonScratch;
        private System.Windows.Forms.Button buttonUdl;
        private System.Windows.Forms.Button buttonAkc;
        private System.Windows.Forms.Button buttonText;
        private System.Windows.Forms.Button buttonPage;
    }
}