
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace qbook.UI
{
    partial class Keyboard
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
            this.panelQuertz = new System.Windows.Forms.FlowLayoutPanel();
            this.tbResult = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.Numblock = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCheck = new System.Windows.Forms.Button();
            this.btnAbort = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelQuertz
            // 
            this.panelQuertz.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelQuertz.Location = new System.Drawing.Point(0, 42);
            this.panelQuertz.Margin = new System.Windows.Forms.Padding(0);
            this.panelQuertz.Name = "panelQuertz";
            this.panelQuertz.Size = new System.Drawing.Size(410, 165);
            this.panelQuertz.TabIndex = 0;
            // 
            // tbResult
            // 
            this.tbResult.BackColor = System.Drawing.Color.LightGray;
            this.tbResult.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tableLayoutPanel1.SetColumnSpan(this.tbResult, 2);
            this.tbResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbResult.Font = new System.Drawing.Font("Calibri", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbResult.Location = new System.Drawing.Point(3, 3);
            this.tbResult.Name = "tbResult";
            this.tbResult.Size = new System.Drawing.Size(467, 36);
            this.tbResult.TabIndex = 16;
            this.tbResult.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbResult_KeyDown);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 410F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 63F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 63F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel1.Controls.Add(this.tbResult, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panelQuertz, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.Numblock, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnCheck, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnAbort, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnClear, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 165F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(546, 252);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // Numblock
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.Numblock, 2);
            this.Numblock.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Numblock.Location = new System.Drawing.Point(413, 45);
            this.Numblock.Name = "Numblock";
            this.Numblock.Size = new System.Drawing.Size(120, 159);
            this.Numblock.TabIndex = 18;
            // 
            // btnCheck
            // 
            this.btnCheck.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCheck.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCheck.Location = new System.Drawing.Point(476, 210);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(57, 39);
            this.btnCheck.TabIndex = 19;
            this.btnCheck.UseVisualStyleBackColor = true;
            this.btnCheck.Click += new System.EventHandler(this.btnCheck_Click);
            // 
            // btnAbort
            // 
            this.btnAbort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAbort.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAbort.Location = new System.Drawing.Point(413, 210);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(57, 39);
            this.btnAbort.TabIndex = 19;
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // btnClear
            // 
            this.btnClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClear.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.Location = new System.Drawing.Point(476, 3);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(57, 36);
            this.btnClear.TabIndex = 19;
            this.btnClear.Text = "DEL";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // Keyboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(546, 252);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Keyboard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Keyboard";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.FlowLayoutPanel panelQuertz;
        private System.Windows.Forms.TextBox tbResult;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel Numblock;
        private FontAwesome.Sharp.IconButton btnClose;
        private Button btnCheck;
        private Button btnAbort;
        private Button btnClear;
    }
}