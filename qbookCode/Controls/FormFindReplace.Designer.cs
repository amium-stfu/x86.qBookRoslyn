namespace qbookCode.Controls
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
            tableLayoutPanel1 = new TableLayoutPanel();
            btnMatchCase = new Button();
            btnReplaceAll = new FontAwesome.Sharp.IconButton();
            dataGridResult = new DataGridView();
            vBar = new qbookCode.Controls.Scrollbars.GridViewVerticalBar();
            btnReplaceNext = new FontAwesome.Sharp.IconButton();
            btnFindPrevious = new FontAwesome.Sharp.IconButton();
            btnFindNext = new FontAwesome.Sharp.IconButton();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridResult).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.BackColor = SystemColors.ActiveBorder;
            tableLayoutPanel1.ColumnCount = 7;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 23F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 404F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 23F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 22F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 151F));
            tableLayoutPanel1.Controls.Add(btnMatchCase, 2, 3);
            tableLayoutPanel1.Controls.Add(btnReplaceAll, 3, 2);
            tableLayoutPanel1.Controls.Add(dataGridResult, 5, 4);
            tableLayoutPanel1.Controls.Add(vBar, 5, 5);
            tableLayoutPanel1.Controls.Add(btnReplaceNext, 2, 2);
            tableLayoutPanel1.Controls.Add(btnFindPrevious, 2, 1);
            tableLayoutPanel1.Controls.Add(btnFindNext, 3, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 33F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 33F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 33F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 9F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            tableLayoutPanel1.Size = new Size(639, 304);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // btnMatchCase
            // 
            btnMatchCase.Dock = DockStyle.Fill;
            btnMatchCase.FlatAppearance.BorderSize = 0;
            btnMatchCase.FlatStyle = FlatStyle.Flat;
            btnMatchCase.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnMatchCase.Location = new Point(431, 70);
            btnMatchCase.Margin = new Padding(4, 3, 4, 3);
            btnMatchCase.Name = "btnMatchCase";
            btnMatchCase.Size = new Size(72, 27);
            btnMatchCase.TabIndex = 0;
            btnMatchCase.Text = "Aa";
            btnMatchCase.UseVisualStyleBackColor = true;
            btnMatchCase.Click += btnMatchCase_Click;
            // 
            // btnReplaceAll
            // 
            btnReplaceAll.Dock = DockStyle.Fill;
            btnReplaceAll.FlatAppearance.BorderSize = 0;
            btnReplaceAll.FlatStyle = FlatStyle.Flat;
            btnReplaceAll.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnReplaceAll.IconChar = FontAwesome.Sharp.IconChar.None;
            btnReplaceAll.IconColor = Color.Black;
            btnReplaceAll.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnReplaceAll.Location = new Point(511, 37);
            btnReplaceAll.Margin = new Padding(4, 3, 4, 3);
            btnReplaceAll.Name = "btnReplaceAll";
            btnReplaceAll.Size = new Size(72, 27);
            btnReplaceAll.TabIndex = 6;
            btnReplaceAll.Text = "All";
            btnReplaceAll.UseVisualStyleBackColor = true;
            btnReplaceAll.Click += btnReplaceAll_Click;
            // 
            // dataGridResult
            // 
            dataGridResult.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            tableLayoutPanel1.SetColumnSpan(dataGridResult, 5);
            dataGridResult.Dock = DockStyle.Fill;
            dataGridResult.Location = new Point(0, 109);
            dataGridResult.Margin = new Padding(0);
            dataGridResult.Name = "dataGridResult";
            dataGridResult.Size = new Size(610, 195);
            dataGridResult.TabIndex = 9;
            // 
            // vBar
            // 
            vBar.Dock = DockStyle.Fill;
            vBar.Location = new Point(613, 112);
            vBar.Name = "vBar";
            vBar.SetBackColor = Color.LightGray;
            vBar.SetForeColor = Color.DodgerBlue;
            vBar.Size = new Size(16, 189);
            vBar.TabIndex = 10;
            // 
            // btnReplaceNext
            // 
            btnReplaceNext.Dock = DockStyle.Fill;
            btnReplaceNext.FlatAppearance.BorderSize = 0;
            btnReplaceNext.FlatStyle = FlatStyle.Flat;
            btnReplaceNext.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnReplaceNext.IconChar = FontAwesome.Sharp.IconChar.None;
            btnReplaceNext.IconColor = Color.Black;
            btnReplaceNext.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnReplaceNext.Location = new Point(430, 37);
            btnReplaceNext.Name = "btnReplaceNext";
            btnReplaceNext.Size = new Size(74, 27);
            btnReplaceNext.TabIndex = 11;
            btnReplaceNext.Text = "Next";
            btnReplaceNext.UseVisualStyleBackColor = true;
            btnReplaceNext.Click += btnReplaceNext_Click;
            // 
            // btnFindPrevious
            // 
            btnFindPrevious.Dock = DockStyle.Fill;
            btnFindPrevious.FlatAppearance.BorderSize = 0;
            btnFindPrevious.FlatStyle = FlatStyle.Flat;
            btnFindPrevious.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnFindPrevious.IconChar = FontAwesome.Sharp.IconChar.None;
            btnFindPrevious.IconColor = Color.Black;
            btnFindPrevious.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnFindPrevious.Location = new Point(430, 4);
            btnFindPrevious.Name = "btnFindPrevious";
            btnFindPrevious.Size = new Size(74, 27);
            btnFindPrevious.TabIndex = 11;
            btnFindPrevious.Text = "Last";
            btnFindPrevious.UseVisualStyleBackColor = true;
            btnFindPrevious.Click += btnFindPrevious_Click;
            // 
            // btnFindNext
            // 
            btnFindNext.Dock = DockStyle.Fill;
            btnFindNext.FlatAppearance.BorderSize = 0;
            btnFindNext.FlatStyle = FlatStyle.Flat;
            btnFindNext.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnFindNext.IconChar = FontAwesome.Sharp.IconChar.None;
            btnFindNext.IconColor = Color.Black;
            btnFindNext.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnFindNext.Location = new Point(510, 4);
            btnFindNext.Name = "btnFindNext";
            btnFindNext.Size = new Size(74, 27);
            btnFindNext.TabIndex = 11;
            btnFindNext.Text = "Next";
            btnFindNext.UseVisualStyleBackColor = true;
            btnFindNext.Click += btnFindNext_Click;
            // 
            // FormFindReplace
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(639, 304);
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormFindReplace";
            ShowIcon = false;
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "Find / Replace";
            Shown += FormFindReplace_Shown;
            tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridResult).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private FontAwesome.Sharp.IconButton btnReplaceAll;
        private System.Windows.Forms.Button btnMatchCase;
        private System.Windows.Forms.DataGridView dataGridResult;
        private Scrollbars.GridViewVerticalBar vBar;
        private FontAwesome.Sharp.IconButton btnReplaceNext;
        private FontAwesome.Sharp.IconButton btnFindPrevious;
        private FontAwesome.Sharp.IconButton btnFindNext;
    }
}
