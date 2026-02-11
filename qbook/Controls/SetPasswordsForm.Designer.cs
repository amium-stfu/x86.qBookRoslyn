namespace qbook.Controls
{
    partial class SetPasswordsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetPasswordsForm));
            this.labelUser = new System.Windows.Forms.Label();
            this.textBoxUser = new qbook.Controls.SetPasswordsForm.TextBoxEx();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textBoxService = new qbook.Controls.SetPasswordsForm.TextBoxEx();
            this.labelService = new System.Windows.Forms.Label();
            this.textBoxAdmin = new qbook.Controls.SetPasswordsForm.TextBoxEx();
            this.labelAdmin = new System.Windows.Forms.Label();
            this.checkBoxVisibleAdmin = new System.Windows.Forms.CheckBox();
            this.checkBoxVisibleService = new System.Windows.Forms.CheckBox();
            this.checkBoxVisibleUser = new System.Windows.Forms.CheckBox();
            this.buttonOpenKbd = new qbook.Controls.SetPasswordsForm.NoFocusButton();
            this.SuspendLayout();
            // 
            // labelUser
            // 
            this.labelUser.AutoSize = true;
            this.labelUser.Location = new System.Drawing.Point(12, 19);
            this.labelUser.Name = "labelUser";
            this.labelUser.Size = new System.Drawing.Size(29, 13);
            this.labelUser.TabIndex = 0;
            this.labelUser.Text = "User";
            // 
            // textBoxUser
            // 
            this.textBoxUser.Location = new System.Drawing.Point(80, 16);
            this.textBoxUser.Name = "textBoxUser";
            this.textBoxUser.PasswordChar = '•';
            this.textBoxUser.Size = new System.Drawing.Size(125, 20);
            this.textBoxUser.TabIndex = 0;
            this.textBoxUser.TextChanged += new System.EventHandler(this.textBoxUser_TextChanged);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(165, 140);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(82, 140);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // textBoxService
            // 
            this.textBoxService.Location = new System.Drawing.Point(80, 55);
            this.textBoxService.Name = "textBoxService";
            this.textBoxService.PasswordChar = '•';
            this.textBoxService.Size = new System.Drawing.Size(125, 20);
            this.textBoxService.TabIndex = 1;
            this.textBoxService.TextChanged += new System.EventHandler(this.textBoxService_TextChanged);
            // 
            // labelService
            // 
            this.labelService.AutoSize = true;
            this.labelService.Location = new System.Drawing.Point(12, 58);
            this.labelService.Name = "labelService";
            this.labelService.Size = new System.Drawing.Size(43, 13);
            this.labelService.TabIndex = 4;
            this.labelService.Text = "Service";
            // 
            // textBoxAdmin
            // 
            this.textBoxAdmin.Location = new System.Drawing.Point(80, 95);
            this.textBoxAdmin.Name = "textBoxAdmin";
            this.textBoxAdmin.PasswordChar = '•';
            this.textBoxAdmin.Size = new System.Drawing.Size(125, 20);
            this.textBoxAdmin.TabIndex = 2;
            this.textBoxAdmin.TextChanged += new System.EventHandler(this.textBoxAdmin_TextChanged);
            // 
            // labelAdmin
            // 
            this.labelAdmin.AutoSize = true;
            this.labelAdmin.Location = new System.Drawing.Point(12, 98);
            this.labelAdmin.Name = "labelAdmin";
            this.labelAdmin.Size = new System.Drawing.Size(36, 13);
            this.labelAdmin.TabIndex = 6;
            this.labelAdmin.Text = "Admin";
            // 
            // checkBoxVisibleAdmin
            // 
            this.checkBoxVisibleAdmin.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBoxVisibleAdmin.Font = new System.Drawing.Font("Cooper Black", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxVisibleAdmin.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxVisibleAdmin.Image")));
            this.checkBoxVisibleAdmin.Location = new System.Drawing.Point(211, 94);
            this.checkBoxVisibleAdmin.Name = "checkBoxVisibleAdmin";
            this.checkBoxVisibleAdmin.Size = new System.Drawing.Size(22, 22);
            this.checkBoxVisibleAdmin.TabIndex = 7;
            this.checkBoxVisibleAdmin.Text = "/";
            this.checkBoxVisibleAdmin.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxVisibleAdmin.UseVisualStyleBackColor = true;
            this.checkBoxVisibleAdmin.CheckedChanged += new System.EventHandler(this.checkBoxVisibleAdmin_CheckedChanged);
            // 
            // checkBoxVisibleService
            // 
            this.checkBoxVisibleService.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBoxVisibleService.Font = new System.Drawing.Font("Cooper Black", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxVisibleService.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxVisibleService.Image")));
            this.checkBoxVisibleService.Location = new System.Drawing.Point(211, 54);
            this.checkBoxVisibleService.Name = "checkBoxVisibleService";
            this.checkBoxVisibleService.Size = new System.Drawing.Size(22, 22);
            this.checkBoxVisibleService.TabIndex = 6;
            this.checkBoxVisibleService.Text = "/";
            this.checkBoxVisibleService.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxVisibleService.UseVisualStyleBackColor = true;
            this.checkBoxVisibleService.CheckedChanged += new System.EventHandler(this.checkBoxVisibleService_CheckedChanged);
            // 
            // checkBoxVisibleUser
            // 
            this.checkBoxVisibleUser.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBoxVisibleUser.Font = new System.Drawing.Font("Cooper Black", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxVisibleUser.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxVisibleUser.Image")));
            this.checkBoxVisibleUser.Location = new System.Drawing.Point(212, 14);
            this.checkBoxVisibleUser.Name = "checkBoxVisibleUser";
            this.checkBoxVisibleUser.Size = new System.Drawing.Size(22, 22);
            this.checkBoxVisibleUser.TabIndex = 5;
            this.checkBoxVisibleUser.Text = "/";
            this.checkBoxVisibleUser.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxVisibleUser.UseVisualStyleBackColor = true;
            this.checkBoxVisibleUser.CheckedChanged += new System.EventHandler(this.checkBoxVisibleUser_CheckedChanged);
            // 
            // buttonOpenKbd
            // 
            this.buttonOpenKbd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOpenKbd.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonOpenKbd.BackgroundImage")));
            this.buttonOpenKbd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonOpenKbd.FlatAppearance.BorderSize = 0;
            this.buttonOpenKbd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonOpenKbd.Location = new System.Drawing.Point(12, 142);
            this.buttonOpenKbd.Name = "buttonOpenKbd";
            this.buttonOpenKbd.Size = new System.Drawing.Size(32, 18);
            this.buttonOpenKbd.TabIndex = 8;
            this.buttonOpenKbd.TabStop = false;
            this.buttonOpenKbd.UseVisualStyleBackColor = true;
            this.buttonOpenKbd.Click += new System.EventHandler(this.buttonOpenKbd_Click);
            // 
            // SetPasswordsForm
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(252, 175);
            this.Controls.Add(this.buttonOpenKbd);
            this.Controls.Add(this.checkBoxVisibleAdmin);
            this.Controls.Add(this.checkBoxVisibleService);
            this.Controls.Add(this.checkBoxVisibleUser);
            this.Controls.Add(this.textBoxAdmin);
            this.Controls.Add(this.labelAdmin);
            this.Controls.Add(this.textBoxService);
            this.Controls.Add(this.labelService);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBoxUser);
            this.Controls.Add(this.labelUser);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SetPasswordsForm";
            this.Text = "SET PASSWORDS";
            this.Load += new System.EventHandler(this.SetPasswordsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelUser;
        private TextBoxEx textBoxUser;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private TextBoxEx textBoxService;
        private System.Windows.Forms.Label labelService;
        private TextBoxEx textBoxAdmin;
        private System.Windows.Forms.Label labelAdmin;
        private System.Windows.Forms.CheckBox checkBoxVisibleUser;
        private System.Windows.Forms.CheckBox checkBoxVisibleService;
        private System.Windows.Forms.CheckBox checkBoxVisibleAdmin;
        private NoFocusButton buttonOpenKbd;
    }
}