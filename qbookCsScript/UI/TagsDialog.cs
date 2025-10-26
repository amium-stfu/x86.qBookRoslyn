using System;
using System.Windows.Forms;


namespace QB.UI
{
    public partial class TagsDialog : Form
    {
        public TagsDialog()
        {
            InitializeComponent();
        }

        Signal signal;
        public Signal Signal
        {
            set
            {
                listBox1.Items.Clear();
                signal = value;
                foreach (string name in value.Tags.Keys)
                {
                    listBox1.Items.Add(name + " -> " + value.Tags[name]);
                }
                this.textBoxInput.Text = value.Name;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {

            if ((signal is Module) && (textBoxPreSet.Text.Length > 0))
                (signal as Module).PreSet.Value = double.Parse(textBoxPreSet.Text);

            DialogResult = DialogResult.OK;


            this.Close();
        }

        public static DialogResult ShowDialog(string title, string info, ref string value)
        {
            QB.UI.InputDialog dialog = new QB.UI.InputDialog();
            dialog.Title = title; // "CELL VALUE";
            dialog.Info = info; // $"Change Value at {rowId},{colId} from\r\n   {value}\r\nto:";
            dialog.Value = value; // value.ToString();

            dialog.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            var parentForm = System.Windows.Forms.Application.OpenForms[0];
            dialog.Location = new System.Drawing.Point(parentForm.Left + (parentForm.Width - dialog.Width) / 2, parentForm.Top + (parentForm.Height - dialog.Height) / 2);

            var dr = dialog.ShowDialog();
            value = dialog.Value;

            return dr;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)

                textBoxInput.Text = "" + signal.Tags[signal.Tags.Keys[listBox1.SelectedIndex]];
            else
                textBoxInput.Text = "*";
        }

        private void textBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                signal.Tags[signal.Tags.Keys[listBox1.SelectedIndex]] = textBoxInput.Text;
            }
        }

        private void buttonSet_Click(object sender, EventArgs e)
        {
            signal.Tags[signal.Tags.Keys[listBox1.SelectedIndex]] = textBoxInput.Text;
            Signal = signal;
        }
        //
        private void textBoxPreSet_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
            {
                if (signal is Module)
                    (signal as Module).PreSet.Value = double.Parse(textBoxPreSet.Text);
                this.Close();
            }
        }
    }
}
