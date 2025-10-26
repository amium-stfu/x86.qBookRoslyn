using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace qbook.Controls
{
    public partial class SetPasswordsForm : Form
    {
        public SetPasswordsForm()
        {
            InitializeComponent();
        }

        private void checkBoxVisibleUser_CheckedChanged(object sender, EventArgs e)
        {
            ((CheckBox)sender).Text = ((CheckBox)sender).Checked ? "" : "/";
            textBoxUser.PasswordChar = checkBoxVisibleUser.Checked ? '\0' : '•';
        }

        private void checkBoxVisibleService_CheckedChanged(object sender, EventArgs e)
        {
            ((CheckBox)sender).Text = ((CheckBox)sender).Checked ? "" : "/";
            textBoxService.PasswordChar = checkBoxVisibleService.Checked ? '\0' : '•';
        }

        private void checkBoxVisibleAdmin_CheckedChanged(object sender, EventArgs e)
        {
            ((CheckBox)sender).Text = ((CheckBox)sender).Checked ? "" : "/";
            textBoxAdmin.PasswordChar = checkBoxVisibleAdmin.Checked ? '\0' : '•';
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("Are you sure you want to update the passwords for\r\nUser, Service and Admin?", "UPDATE PASSWORDS", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                if (textBoxUser.Text.Trim().Length == 0)
                    qbook.Core.ThisBook.PasswordUser = null;
                else
                    qbook.Core.ThisBook.PasswordUser = textBoxUser.Text;

                if (textBoxService.Text.Trim().Length == 0)
                    qbook.Core.ThisBook.PasswordService = null;
                else
                    qbook.Core.ThisBook.PasswordService = textBoxService.Text;

                if (textBoxAdmin.Text.Trim().Length == 0)
                    qbook.Core.ThisBook.PasswordAdmin = null;
                else
                    qbook.Core.ThisBook.PasswordAdmin = textBoxAdmin.Text;

                this.Close();
            }
        }

        private void SetPasswordsForm_Load(object sender, EventArgs e)
        {
            textBoxUser.Text = qbook.Core.ThisBook.PasswordUser;
            textBoxService.Text = qbook.Core.ThisBook.PasswordService;
            textBoxAdmin.Text = qbook.Core.ThisBook.PasswordAdmin;
        }

        public class TextBoxEx : TextBox
        {
            public string PlaceholderText = null;

            Label labelDefault = new Label();
            public TextBoxEx()
            {
                // Set initial properties for the placeholder
                //SetStyle(ControlStyles.UserPaint, true);
                labelDefault.Left = 2;
                labelDefault.Top = 0;
                this.Controls.Add(labelDefault);
                labelDefault.Text = "<default>";
                labelDefault.ForeColor = Color.Gray;
                labelDefault.BackColor = this.BackColor;
                labelDefault.Visible = false;
                //labelDefault.GotFocus += LabelDefault_GotFocus;
                labelDefault.Click += LabelDefault_Click;
            }

            public override string Text
            {
                get
                {
                    return base.Text;
                }
                set
                {
                    base.Text = value;
                    this.OnTextChanged(EventArgs.Empty);
                }
            }

            private void LabelDefault_Click(object sender, EventArgs e)
            {
                this.Focus();
            }

            private void LabelDefault_GotFocus(object sender, EventArgs e)
            {
                this.Focus();
            }

            //protected override void OnPaint(PaintEventArgs e)
            //{
            //    base.OnPaint(e);
            //}

            //protected override void OnPaintBackground(PaintEventArgs pevent)
            //{
            //    base.OnPaintBackground(pevent);
            //    if (this.Text.Length == 0) 
            //    {

            //    }
            //}

            protected override void OnTextChanged(EventArgs e)
            {
                base.OnTextChanged(e);
                labelDefault.Visible = this.Text.Length == 0;
            }

        }

        class NoFocusButton : Button
        {
            public NoFocusButton()
            {
                SetStyle(ControlStyles.Selectable, false);
            }
        }

        private void labelServiceDefault_Click(object sender, EventArgs e)
        {
            textBoxService.Focus();
        }

        private void textBoxUser_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBoxService_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBoxAdmin_TextChanged(object sender, EventArgs e)
        {
        }

        private void buttonOpenKbd_Click(object sender, EventArgs e)
        {
            try
            {
                Program.OpenTouchKeyboard();
                if (!Program.ToggleTabTip())
                {
                    MessageBox.Show("Error opening Touch-Keyboard");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception opening Touch-Keyboard");
            }
        }
    }
}
