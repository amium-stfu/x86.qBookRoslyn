
namespace QB.UI
{
    partial class TextDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextDialog));
            this.labelInfo = new System.Windows.Forms.Label();
            this.imageListIcons = new System.Windows.Forms.ImageList(this.components);
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOpenKeyboard = new QB.UI.Windows.NoFocusButton();
            this.richTextBoxEx1 = new RichTextBoxEx.RichTextBoxEx();
            this.SuspendLayout();
            // 
            // labelInfo
            // 
            this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelInfo.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelInfo.Location = new System.Drawing.Point(12, 9);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(806, 21);
            this.labelInfo.TabIndex = 0;
            this.labelInfo.Text = "text...";
            // 
            // imageListIcons
            // 
            this.imageListIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListIcons.ImageStream")));
            this.imageListIcons.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListIcons.Images.SetKeyName(0, "i_check.png");
            this.imageListIcons.Images.SetKeyName(1, "i_info.png");
            this.imageListIcons.Images.SetKeyName(2, "i_question.png");
            this.imageListIcons.Images.SetKeyName(3, "i_warn2.png");
            this.imageListIcons.Images.SetKeyName(4, "i_x.png");
            this.imageListIcons.Images.SetKeyName(5, "i_error.png");
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(743, 448);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(662, 448);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOpenKeyboard
            // 
            this.buttonOpenKeyboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOpenKeyboard.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonOpenKeyboard.BackgroundImage")));
            this.buttonOpenKeyboard.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonOpenKeyboard.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonOpenKeyboard.FlatAppearance.BorderSize = 0;
            this.buttonOpenKeyboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonOpenKeyboard.Location = new System.Drawing.Point(12, 453);
            this.buttonOpenKeyboard.Name = "buttonOpenKeyboard";
            this.buttonOpenKeyboard.Size = new System.Drawing.Size(32, 18);
            this.buttonOpenKeyboard.TabIndex = 4;
            this.buttonOpenKeyboard.UseVisualStyleBackColor = true;
            this.buttonOpenKeyboard.Click += new System.EventHandler(this.buttonOpenKeyboard_Click);
            // 
            // richTextBoxEx1
            // 
            this.richTextBoxEx1.AllowBullets = true;
            this.richTextBoxEx1.AllowDefaultInsertText = true;
            this.richTextBoxEx1.AllowDefaultSmartText = true;
            this.richTextBoxEx1.AllowDrop = true;
            this.richTextBoxEx1.AllowHyphenation = true;
            this.richTextBoxEx1.AllowPictures = true;
            this.richTextBoxEx1.AllowSpellCheck = true;
            this.richTextBoxEx1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxEx1.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.richTextBoxEx1.FilePath = "";
            this.richTextBoxEx1.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxEx1.Location = new System.Drawing.Point(12, 33);
            this.richTextBoxEx1.Name = "richTextBoxEx1";
            this.richTextBoxEx1.Rtf = "{\\rtf1\\ansi\\deff0\\nouicompat{\\fonttbl{\\f0\\fnil\\fcharset0 Arial;}}\r\n{\\*\\generator " +
    "Riched20 10.0.19041}\\viewkind4\\uc1 \r\n\\pard\\fs20\\lang1031 richTextBoxEx1\\par\r\n}\r\n" +
    "";
            this.richTextBoxEx1.SetColorWithFont = true;
            this.richTextBoxEx1.ShowToolStrip = true;
            this.richTextBoxEx1.Size = new System.Drawing.Size(806, 409);
            this.richTextBoxEx1.TabIndex = 5;
            // 
            // TextDialog
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(830, 483);
            this.Controls.Add(this.richTextBoxEx1);
            this.Controls.Add(this.buttonOpenKeyboard);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelInfo);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "TextDialog";
            this.Text = "Text Dialog";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.ImageList imageListIcons;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private QB.UI.Windows.NoFocusButton buttonOpenKeyboard;
        private RichTextBoxEx.RichTextBoxEx richTextBoxEx1;
    }
}