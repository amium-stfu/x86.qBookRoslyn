using System;
using System.Windows.Forms;

namespace QB.UI
{
    public partial class InputDialog : Form
    {
        public InputDialog()
        {
            InitializeComponent();
        }

        public string Result { get; set; } = null;
        public string Title { get => this.Text; set => this.Text = value; }
        public string Info { get => labelInfo.Text; set => this.labelInfo.Text = value; }

        public string Value { get => textBoxInput.Text; set => this.textBoxInput.Text = value; }

        public char PasswordChar { get => textBoxInput.PasswordChar; set => this.textBoxInput.PasswordChar = value; }


        public double ValueD
        {
            get
            {
                if (double.TryParse(textBoxInput.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double d))
                    return d;
                else
                    return double.NaN;
            }
            set
            {
                this.textBoxInput.Text = value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        public static DialogResult ShowDialog(string title, string info, ref string value, char passwordChar = '\0', bool showKeyboardButton = true)
        {
            QB.UI.InputDialog dialog = new QB.UI.InputDialog();
            dialog.Title = title; // "CELL VALUE";
            dialog.Info = info; // $"Change Value at {rowId},{colId} from\r\n   {value}\r\nto:";
            dialog.Value = value; // value.ToString();
            dialog.PasswordChar = passwordChar;

            dialog.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            var parentForm = System.Windows.Forms.Application.OpenForms[0];
            dialog.Location = new System.Drawing.Point(parentForm.Left + (parentForm.Width - dialog.Width) / 2, parentForm.Top + (parentForm.Height - dialog.Height) / 2);

            var dr = dialog.ShowDialog();
            value = dialog.Value;

            return dr;
        }

        private void buttonOpenKeyboard_Click(object sender, EventArgs e)
        {
            try
            {
                QB.UI.Windows.OpenTouchKeyboard();
                if (!QB.UI.Windows.ToggleTabTip())
                {
                    MessageBox.Show("Cannot open Touch-Keyboard");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot open Touch-Keyboard (2)");
            }
        }
    }
}
