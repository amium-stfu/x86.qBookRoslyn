namespace qbook.ScintillaEditor
{
    partial class ControlFindReplace
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
            this.tbFind = new System.Windows.Forms.TextBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnFindNext = new System.Windows.Forms.Button();
            this.btnFindPrevious = new System.Windows.Forms.Button();
            this.btnReplaceNext = new System.Windows.Forms.Button();
            this.btnReplaceAll = new FontAwesome.Sharp.IconButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbTarget = new System.Windows.Forms.ComboBox();
            this.btnMatchCase = new System.Windows.Forms.Button();
            this.tbReplace = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5.45977F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 94.54023F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutPanel1.Controls.Add(this.tbFind, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnClose, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnFindNext, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnFindPrevious, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnReplaceNext, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnReplaceAll, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.tbReplace, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(450, 120);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tbFind
            // 
            this.tbFind.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbFind.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbFind.Location = new System.Drawing.Point(17, 5);
            this.tbFind.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.tbFind.Name = "tbFind";
            this.tbFind.Size = new System.Drawing.Size(303, 29);
            this.tbFind.TabIndex = 0;
            this.tbFind.Text = "g";
            this.tbFind.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbFind_KeyDown);
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(413, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Padding = new System.Windows.Forms.Padding(5);
            this.btnClose.Size = new System.Drawing.Size(34, 34);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnFindNext
            // 
            this.btnFindNext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnFindNext.FlatAppearance.BorderSize = 0;
            this.btnFindNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFindNext.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFindNext.Location = new System.Drawing.Point(368, 3);
            this.btnFindNext.Name = "btnFindNext";
            this.btnFindNext.Padding = new System.Windows.Forms.Padding(5);
            this.btnFindNext.Size = new System.Drawing.Size(39, 34);
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
            this.btnFindPrevious.Location = new System.Drawing.Point(323, 3);
            this.btnFindPrevious.Name = "btnFindPrevious";
            this.btnFindPrevious.Padding = new System.Windows.Forms.Padding(5);
            this.btnFindPrevious.Size = new System.Drawing.Size(39, 34);
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
            this.btnReplaceNext.Location = new System.Drawing.Point(323, 43);
            this.btnReplaceNext.Name = "btnReplaceNext";
            this.btnReplaceNext.Padding = new System.Windows.Forms.Padding(5);
            this.btnReplaceNext.Size = new System.Drawing.Size(39, 34);
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
            this.btnReplaceAll.Location = new System.Drawing.Point(368, 43);
            this.btnReplaceAll.Name = "btnReplaceAll";
            this.btnReplaceAll.Size = new System.Drawing.Size(39, 34);
            this.btnReplaceAll.TabIndex = 6;
            this.btnReplaceAll.Text = "*";
            this.btnReplaceAll.UseVisualStyleBackColor = true;
            this.btnReplaceAll.Click += new System.EventHandler(this.btnReplaceAll_Click);
            // 
            // panel1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.panel1, 3);
            this.panel1.Controls.Add(this.cbTarget);
            this.panel1.Controls.Add(this.btnMatchCase);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(20, 83);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(387, 34);
            this.panel1.TabIndex = 7;
            // 
            // cbTarget
            // 
            this.cbTarget.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbTarget.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTarget.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbTarget.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbTarget.FormattingEnabled = true;
            this.cbTarget.Location = new System.Drawing.Point(51, 0);
            this.cbTarget.Name = "cbTarget";
            this.cbTarget.Size = new System.Drawing.Size(336, 25);
            this.cbTarget.TabIndex = 1;
            this.cbTarget.SelectedIndexChanged += new System.EventHandler(this.cbTarget_SelectedIndexChanged);
            // 
            // btnMatchCase
            // 
            this.btnMatchCase.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnMatchCase.FlatAppearance.BorderSize = 0;
            this.btnMatchCase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMatchCase.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMatchCase.Location = new System.Drawing.Point(0, 0);
            this.btnMatchCase.Name = "btnMatchCase";
            this.btnMatchCase.Size = new System.Drawing.Size(51, 34);
            this.btnMatchCase.TabIndex = 0;
            this.btnMatchCase.Text = "Aa";
            this.btnMatchCase.UseVisualStyleBackColor = true;
            this.btnMatchCase.Click += new System.EventHandler(this.btnMatchCase_Click);
            // 
            // tbReplace
            // 
            this.tbReplace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbReplace.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbReplace.Location = new System.Drawing.Point(17, 45);
            this.tbReplace.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.tbReplace.Name = "tbReplace";
            this.tbReplace.Size = new System.Drawing.Size(303, 29);
            this.tbReplace.TabIndex = 8;
            // 
            // ControlFindReplace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ControlFindReplace";
            this.Size = new System.Drawing.Size(450, 120);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox tbFind;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnFindNext;
        private System.Windows.Forms.Button btnFindPrevious;
        private System.Windows.Forms.Button btnReplaceNext;
        private FontAwesome.Sharp.IconButton btnReplaceAll;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnMatchCase;
        protected System.Windows.Forms.ComboBox cbTarget;
        private System.Windows.Forms.TextBox tbReplace;
    }
}
