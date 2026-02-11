
namespace qbook
{
    partial class NewObjectForm
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
            this.buttonTag = new System.Windows.Forms.Button();
            this.buttonText = new System.Windows.Forms.Button();
            this.buttonValve = new System.Windows.Forms.Button();
            this.buttonPump = new System.Windows.Forms.Button();
            this.buttonFilter = new System.Windows.Forms.Button();
            this.panelObjects = new System.Windows.Forms.Panel();
            this.buttonHtml = new System.Windows.Forms.Button();
            this.buttonControl = new System.Windows.Forms.Button();
            this.buttonPc = new System.Windows.Forms.Button();
            this.buttonScratch = new System.Windows.Forms.Button();
            this.buttonGrid = new System.Windows.Forms.Button();
            this.buttonModule = new System.Windows.Forms.Button();
            this.panelObjects.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonClear
            // 
            this.buttonClear.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClear.Location = new System.Drawing.Point(3, 46);
            this.buttonClear.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(352, 45);
            this.buttonClear.TabIndex = 3;
            this.buttonClear.Text = "Cancel";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // buttonTag
            // 
            this.buttonTag.BackColor = System.Drawing.SystemColors.Control;
            this.buttonTag.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonTag.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonTag.Location = new System.Drawing.Point(262, 3);
            this.buttonTag.Name = "buttonTag";
            this.buttonTag.Size = new System.Drawing.Size(81, 35);
            this.buttonTag.TabIndex = 21;
            this.buttonTag.Text = "Tag";
            this.buttonTag.UseVisualStyleBackColor = false;
            this.buttonTag.Visible = false;
            this.buttonTag.Click += new System.EventHandler(this.buttonTag_Click);
            // 
            // buttonText
            // 
            this.buttonText.BackColor = System.Drawing.SystemColors.Control;
            this.buttonText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonText.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonText.Location = new System.Drawing.Point(3, 3);
            this.buttonText.Name = "buttonText";
            this.buttonText.Size = new System.Drawing.Size(81, 35);
            this.buttonText.TabIndex = 21;
            this.buttonText.Text = "Text";
            this.buttonText.UseVisualStyleBackColor = false;
            this.buttonText.Click += new System.EventHandler(this.buttonText_Click);
            // 
            // buttonValve
            // 
            this.buttonValve.BackColor = System.Drawing.SystemColors.Control;
            this.buttonValve.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonValve.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonValve.Location = new System.Drawing.Point(3, 108);
            this.buttonValve.Name = "buttonValve";
            this.buttonValve.Size = new System.Drawing.Size(81, 35);
            this.buttonValve.TabIndex = 21;
            this.buttonValve.Text = "Valve";
            this.buttonValve.UseVisualStyleBackColor = true;
            this.buttonValve.Visible = false;
            this.buttonValve.Click += new System.EventHandler(this.buttonValve_Click);
            // 
            // buttonPump
            // 
            this.buttonPump.BackColor = System.Drawing.SystemColors.Control;
            this.buttonPump.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPump.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPump.Location = new System.Drawing.Point(90, 108);
            this.buttonPump.Name = "buttonPump";
            this.buttonPump.Size = new System.Drawing.Size(81, 35);
            this.buttonPump.TabIndex = 21;
            this.buttonPump.Text = "Pump";
            this.buttonPump.UseVisualStyleBackColor = true;
            this.buttonPump.Visible = false;
            this.buttonPump.Click += new System.EventHandler(this.buttonPump_Click);
            // 
            // buttonFilter
            // 
            this.buttonFilter.BackColor = System.Drawing.SystemColors.Control;
            this.buttonFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonFilter.Location = new System.Drawing.Point(177, 108);
            this.buttonFilter.Name = "buttonFilter";
            this.buttonFilter.Size = new System.Drawing.Size(81, 35);
            this.buttonFilter.TabIndex = 21;
            this.buttonFilter.Text = "Filter";
            this.buttonFilter.UseVisualStyleBackColor = true;
            this.buttonFilter.Visible = false;
            this.buttonFilter.Click += new System.EventHandler(this.buttonFilter_Click);
            // 
            // panelObjects
            // 
            this.panelObjects.Controls.Add(this.buttonHtml);
            this.panelObjects.Controls.Add(this.buttonClear);
            this.panelObjects.Controls.Add(this.buttonControl);
            this.panelObjects.Controls.Add(this.buttonTag);
            this.panelObjects.Controls.Add(this.buttonPc);
            this.panelObjects.Controls.Add(this.buttonScratch);
            this.panelObjects.Controls.Add(this.buttonGrid);
            this.panelObjects.Controls.Add(this.buttonText);
            this.panelObjects.Controls.Add(this.buttonModule);
            this.panelObjects.Controls.Add(this.buttonValve);
            this.panelObjects.Controls.Add(this.buttonFilter);
            this.panelObjects.Controls.Add(this.buttonPump);
            this.panelObjects.Location = new System.Drawing.Point(2, 5);
            this.panelObjects.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelObjects.Name = "panelObjects";
            this.panelObjects.Size = new System.Drawing.Size(356, 197);
            this.panelObjects.TabIndex = 25;
            // 
            // buttonHtml
            // 
            this.buttonHtml.BackColor = System.Drawing.SystemColors.Control;
            this.buttonHtml.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonHtml.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonHtml.Location = new System.Drawing.Point(177, 3);
            this.buttonHtml.Name = "buttonHtml";
            this.buttonHtml.Size = new System.Drawing.Size(81, 35);
            this.buttonHtml.TabIndex = 26;
            this.buttonHtml.Text = "HTML";
            this.buttonHtml.UseVisualStyleBackColor = false;
            this.buttonHtml.Click += new System.EventHandler(this.buttonHtml_Click);
            // 
            // buttonControl
            // 
            this.buttonControl.BackColor = System.Drawing.SystemColors.Control;
            this.buttonControl.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonControl.Location = new System.Drawing.Point(90, 3);
            this.buttonControl.Name = "buttonControl";
            this.buttonControl.Size = new System.Drawing.Size(81, 35);
            this.buttonControl.TabIndex = 24;
            this.buttonControl.Text = "Control";
            this.buttonControl.UseVisualStyleBackColor = true;
            this.buttonControl.Visible = false;
            this.buttonControl.Click += new System.EventHandler(this.buttonControl_Click);
            // 
            // buttonPc
            // 
            this.buttonPc.BackColor = System.Drawing.SystemColors.Control;
            this.buttonPc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPc.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPc.Location = new System.Drawing.Point(264, 108);
            this.buttonPc.Name = "buttonPc";
            this.buttonPc.Size = new System.Drawing.Size(81, 35);
            this.buttonPc.TabIndex = 21;
            this.buttonPc.Text = "PC";
            this.buttonPc.UseVisualStyleBackColor = false;
            this.buttonPc.Visible = false;
            // 
            // buttonScratch
            // 
            this.buttonScratch.BackColor = System.Drawing.SystemColors.Control;
            this.buttonScratch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonScratch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonScratch.Location = new System.Drawing.Point(90, 148);
            this.buttonScratch.Name = "buttonScratch";
            this.buttonScratch.Size = new System.Drawing.Size(81, 35);
            this.buttonScratch.TabIndex = 21;
            this.buttonScratch.Text = "Scratch";
            this.buttonScratch.UseVisualStyleBackColor = false;
            this.buttonScratch.Visible = false;
            this.buttonScratch.Click += new System.EventHandler(this.buttonScratch_Click);
            // 
            // buttonGrid
            // 
            this.buttonGrid.BackColor = System.Drawing.SystemColors.Control;
            this.buttonGrid.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonGrid.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonGrid.Location = new System.Drawing.Point(177, 149);
            this.buttonGrid.Name = "buttonGrid";
            this.buttonGrid.Size = new System.Drawing.Size(81, 35);
            this.buttonGrid.TabIndex = 21;
            this.buttonGrid.Text = "Grid";
            this.buttonGrid.UseVisualStyleBackColor = false;
            this.buttonGrid.Visible = false;
            this.buttonGrid.Click += new System.EventHandler(this.buttonGrid_Click);
            // 
            // buttonModule
            // 
            this.buttonModule.BackColor = System.Drawing.SystemColors.Control;
            this.buttonModule.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonModule.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonModule.Location = new System.Drawing.Point(3, 148);
            this.buttonModule.Name = "buttonModule";
            this.buttonModule.Size = new System.Drawing.Size(81, 35);
            this.buttonModule.TabIndex = 21;
            this.buttonModule.Text = "Module";
            this.buttonModule.UseVisualStyleBackColor = true;
            this.buttonModule.Visible = false;
            this.buttonModule.Click += new System.EventHandler(this.buttonModule_Click);
            // 
            // NewObjectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.LightSlateGray;
            this.ClientSize = new System.Drawing.Size(362, 106);
            this.Controls.Add(this.panelObjects);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "NewObjectForm";
            this.Opacity = 0.9D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.panelObjects.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Button buttonTag;
        private System.Windows.Forms.Button buttonText;
        private System.Windows.Forms.Button buttonValve;
        private System.Windows.Forms.Button buttonPump;
        private System.Windows.Forms.Button buttonFilter;
        private System.Windows.Forms.Panel panelObjects;
        private System.Windows.Forms.Button buttonPc;
        private System.Windows.Forms.Button buttonModule;
        private System.Windows.Forms.Button buttonGrid;
        private System.Windows.Forms.Button buttonScratch;
        private System.Windows.Forms.Button buttonControl;
        private System.Windows.Forms.Button buttonHtml;
    }
}