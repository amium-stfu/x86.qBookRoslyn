using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;



namespace qbookCode.Controls.InputControls
{
    public partial class TextBoxWithLabel : UserControl
    {
        private Func<string> getter;
        private Action<string> setter;
        private Action OnUpdate;


        List<string> values;

        public bool ReadOnly
        {
            get => textBoxValue.ReadOnly;
            set => textBoxValue.ReadOnly = value;
        }

        public TextBoxWithLabel(string label, Func<string> getter, Action<string> setter, List<string> values = null, Action onUpdate = null)
        {
            InitializeComponent();
            this.getter = getter;
            this.setter = setter;
            this.OnUpdate = onUpdate;

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
                        if (OnUpdate != null)
                            OnUpdate();
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
                if (setter != null)
                {
                    setter(textBoxValue.Text);
                    if(OnUpdate != null)
                        OnUpdate();

                }
                EnterPressed?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
    }
}
