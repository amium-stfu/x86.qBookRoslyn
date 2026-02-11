using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;



namespace qbook.ScintillaEditor.InputControls
{
    public partial class TextBoxWithLabel : UserControl
    {
        private Func<string> getter;
        private Action<string> setter;

        List<string> values;

        public bool ReadOnly
        {
            get => textBoxValue.ReadOnly;
            set => textBoxValue.ReadOnly = value;
        }

        public TextBoxWithLabel(string label, Func<string> getter, Action<string> setter, List<string> values = null)
        {
            InitializeComponent();
            this.getter = getter;
            this.setter = setter;

            this.values = values;

            label1.Text = label;
            textBoxValue.Text = getter();

            textBoxValue.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    setter(textBoxValue.Text);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };
            this.values = values;
            
        }

        private Panel dropdownPanel;

        void textBoxValue_Click(object sender, EventArgs e)
        {
            if (values != null && values.Count > 0)
            {
                ContextMenuStrip menu = new ContextMenuStrip();

                // Schriftart und Hintergrundfarbe setzen
                menu.Font = textBoxValue.Font;
                menu.BackColor = textBoxValue.BackColor;

                foreach (var val in values)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem(val);
                    item.Font = textBoxValue.Font; // optional, wenn du pro Item setzen willst
                    item.ForeColor = textBoxValue.ForeColor;
                    item.BackColor = textBoxValue.BackColor; // optional
                    item.Image = new Bitmap(1, 1);



                    item.Click += (s, ev) =>
                    {
                        textBoxValue.Text = val;
                        setter(val);
                    };
                    menu.Items.Add(item);
                }

                menu.Show(textBoxValue, 0, textBoxValue.Height);
            }
        }


        // Optional: Methode, um den Wert im Textfeld zu aktualisieren
        public void RefreshValue()
        {
            textBoxValue.Text = getter();
            ApplyTheme();
        }

        public void ApplyTheme()
        {

            this.BackColor = Theme.BackColor;
            this.ForeColor = Theme.BackColor;
            textBoxValue.BackColor = Theme.PanelBackColor;
            textBoxValue.ForeColor = Theme.GridForeColor;
            label1.ForeColor = Theme.GridForeColor;
            label1.BackColor = Theme.BackColor;


        }

        void ApplyDarkMode()
        {
            this.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.ForeColor = System.Drawing.Color.White;
            textBoxValue.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            textBoxValue.ForeColor = System.Drawing.Color.White;
            label1.ForeColor = System.Drawing.Color.White;
            label1.BackColor = this.BackColor;
        }
        public void ApplyLightMode()
        {
            this.BackColor = System.Drawing.Color.FromArgb(220, 220, 220);
            this.ForeColor = System.Drawing.Color.Black;
            textBoxValue.BackColor = System.Drawing.Color.White;
            textBoxValue.ForeColor = System.Drawing.Color.Black;
            label1.ForeColor = System.Drawing.Color.Black;
            label1.BackColor = this.BackColor;
        }

        class ListButton : Button
        {
            public ListButton(string text, System.Drawing.Font font)
            {
                this.Font = font;
                this.Text = text;
                Dock = DockStyle.Top;
                this.Height = 20;
                FlatStyle = FlatStyle.Flat;
                FlatAppearance.BorderSize = 1;
            }


        }

        public event EventHandler EnterPressed;

        private void textBoxValue_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                if(setter != null) setter(textBoxValue.Text);
                EnterPressed?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
    }
}
