namespace qbook.ScintillaEditor
{
    partial class FormFindReplace
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnFindNext = new System.Windows.Forms.Button();
            this.btnFindPrevious = new System.Windows.Forms.Button();
            this.btnReplaceNext = new System.Windows.Forms.Button();
            this.btnReplaceAll = new FontAwesome.Sharp.IconButton();
            this.btnMatchCase = new System.Windows.Forms.Button();
            this.dataGridResult = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridResult)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 415F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 78F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 67F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 41F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tableLayoutPanel1.Controls.Add(this.btnMatchCase, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.btnFindNext, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnFindPrevious, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnReplaceNext, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnReplaceAll, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.dataGridResult, 5, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(640, 527);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // btnFindNext
            // 
            this.btnFindNext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnFindNext.FlatAppearance.BorderSize = 0;
            this.btnFindNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFindNext.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFindNext.Location = new System.Drawing.Point(516, 23);
            this.btnFindNext.Name = "btnFindNext";
            this.btnFindNext.Padding = new System.Windows.Forms.Padding(5);
            this.btnFindNext.Size = new System.Drawing.Size(61, 34);
            this.btnFindNext.TabIndex = 3;
            this.btnFindNext.Text = ">";
            this.btnFindNext.UseVisualStyleBackColor = true;
            this.btnFindNext.Click += new System.EventHandler(this.btnFindNext_Click);
            // 
            // btnFindPrevious
            // 
            this.btnFindPrevious.BackColor = System.Drawing.Color.IndianRed;
            this.btnFindPrevious.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnFindPrevious.FlatAppearance.BorderSize = 0;
            this.btnFindPrevious.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFindPrevious.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFindPrevious.Location = new System.Drawing.Point(438, 23);
            this.btnFindPrevious.Name = "btnFindPrevious";
            this.btnFindPrevious.Padding = new System.Windows.Forms.Padding(5);
            this.btnFindPrevious.Size = new System.Drawing.Size(72, 34);
            this.btnFindPrevious.TabIndex = 4;
            this.btnFindPrevious.Text = "<";
            this.btnFindPrevious.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnFindPrevious.UseVisualStyleBackColor = false;
            this.btnFindPrevious.Click += new System.EventHandler(this.btnFindPrevious_Click);
            // 
            // btnReplaceNext
            // 
            this.btnReplaceNext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReplaceNext.FlatAppearance.BorderSize = 0;
            this.btnReplaceNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReplaceNext.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReplaceNext.Location = new System.Drawing.Point(438, 63);
            this.btnReplaceNext.Name = "btnReplaceNext";
            this.btnReplaceNext.Padding = new System.Windows.Forms.Padding(5);
            this.btnReplaceNext.Size = new System.Drawing.Size(72, 34);
            this.btnReplaceNext.TabIndex = 5;
            this.btnReplaceNext.Text = ">";
            this.btnReplaceNext.UseVisualStyleBackColor = true;
            this.btnReplaceNext.Click += new System.EventHandler(this.btnReplaceNext_Click);
            // 
            // btnReplaceAll
            // 
            this.btnReplaceAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReplaceAll.FlatAppearance.BorderSize = 0;
            this.btnReplaceAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReplaceAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReplaceAll.IconChar = FontAwesome.Sharp.IconChar.None;
            this.btnReplaceAll.IconColor = System.Drawing.Color.Black;
            this.btnReplaceAll.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnReplaceAll.Location = new System.Drawing.Point(516, 63);
            this.btnReplaceAll.Name = "btnReplaceAll";
            this.btnReplaceAll.Size = new System.Drawing.Size(61, 34);
            this.btnReplaceAll.TabIndex = 6;
            this.btnReplaceAll.Text = "*";
            this.btnReplaceAll.UseVisualStyleBackColor = true;
            this.btnReplaceAll.Click += new System.EventHandler(this.btnReplaceAll_Click);
            // 
            // btnMatchCase
            // 
            this.btnMatchCase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMatchCase.FlatAppearance.BorderSize = 0;
            this.btnMatchCase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMatchCase.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMatchCase.Location = new System.Drawing.Point(438, 103);
            this.btnMatchCase.Name = "btnMatchCase";
            this.btnMatchCase.Size = new System.Drawing.Size(72, 34);
            this.btnMatchCase.TabIndex = 0;
            this.btnMatchCase.Text = "Aa";
            this.btnMatchCase.UseVisualStyleBackColor = true;
            this.btnMatchCase.Click += new System.EventHandler(this.btnMatchCase_Click);
            // 
            // dataGridResult
            // 
            this.dataGridResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableLayoutPanel1.SetColumnSpan(this.dataGridResult, 6);
            this.dataGridResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridResult.Location = new System.Drawing.Point(3, 151);
            this.dataGridResult.Name = "dataGridResult";
            this.dataGridResult.Size = new System.Drawing.Size(659, 373);
            this.dataGridResult.TabIndex = 9;
            // 
            // FormFindReplace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 527);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FormFindReplace";
            this.Text = "Find / Replace";
            this.Shown += new System.EventHandler(this.FormFindReplace_Shown);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridResult)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnFindNext;
        private System.Windows.Forms.Button btnFindPrevious;
        private System.Windows.Forms.Button btnReplaceNext;
        private FontAwesome.Sharp.IconButton btnReplaceAll;
        private System.Windows.Forms.Button btnMatchCase;
        private System.Windows.Forms.DataGridView dataGridResult;
    }
}
