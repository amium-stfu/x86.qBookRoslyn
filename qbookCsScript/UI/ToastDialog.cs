using System;
using System.Drawing;
using System.Windows.Forms;

namespace QB.UI
{
    public partial class Toast : Form
    {
        public Toast()
        {
            InitializeComponent();
        }

        protected override bool ShowWithoutActivation => true;
        public string Title
        {
            get
            {
                return labelTitle.Text;
            }
            set
            {
                labelTitle.Text = value;
                if (string.IsNullOrEmpty(value))
                {
                    labelTitle.Visible = false;
                    labelText.Top = 8;
                    labelText.Height = 117;
                }
                else
                {
                    labelText.Top = 52;
                    labelText.Height = 73;
                }
            }
        }

        public string Text
        {
            get
            {
                return labelText.Text;
            }
            set
            {
                labelText.Text = value;
            }
        }

        public override Color BackColor { get => panelMain.BackColor; set => panelMain.BackColor = value; }
        public Color TextColor
        {
            get
            {
                return labelText.ForeColor;
            }
            set
            {
                labelText.ForeColor = value;
                labelTitle.ForeColor = value;
            }
        }

        System.Timers.Timer _AutocloseTimer = null;
        double _AutocloseSeconds = 0;
        public double AutocloseSeconds
        {
            get
            {
                return _AutocloseSeconds;
            }
            set
            {
                _AutocloseSeconds = value;
                if (_AutocloseSeconds > 0)
                {
                    if (_AutocloseTimer == null)
                    {
                        _AutocloseTimer = new System.Timers.Timer();
                        _AutocloseTimer.Elapsed += _AutocloseTimer_Elapsed;
                    }
                    _AutocloseTimer.Interval = _AutocloseSeconds;
                    _AutocloseTimer.Start();
                }
                else
                {
                    _AutocloseTimer?.Stop();
                }
            }
        }

        private void _AutocloseTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _AutocloseTimer?.Stop();
            _AutocloseTimer = null;

            this.EnsureBeginInvoke(() => this.Close());
        }

        /// <summary>
        /// Shows a Toast (centered in the qbook window)
        /// </summary>
        /// <param name="text">The text to show</param>
        /// <param name="title">An optional title</param>
        /// <param name="timeout">Timeout until the toast disappears automatically (default=2.0 seconds)</param>
        /// <param name="backColor">Backcolor of the Toast window</param>
        /// <param name="textColor">Textcolor of the Toast's texts</param>
        /// <returns></returns>
        public static Toast Show(string text, string title = null, double timeout = 2000, Color? backColor = null, Color? textColor = null)
        {
            if (System.Windows.Forms.Application.OpenForms.Count > 0)
                return Show(System.Windows.Forms.Application.OpenForms[0], text, title, timeout, backColor, textColor);
            else
                return Show(null, text, title, timeout, backColor, textColor);
        }
        internal static Toast Show(Form parent, string text, string title = null, double timeout = 2000, Color? backColor = null, Color? textColor = null)
        {
            Toast toast = new Toast();
            toast.TopMost = true;
            toast.ShowInTaskbar = false;
            //toast.Enabled = false;

            if (backColor != null)
                toast.BackColor = (Color)backColor;
            if (textColor != null)
                toast.TextColor = (Color)textColor;

            toast.Text = text;
            toast.Title = title;

            toast.StartPosition = FormStartPosition.Manual;
            //toast.Parent = parent;
            toast.Location = new Point(parent.Left + (parent.Width - toast.Width) / 2, parent.Top + (parent.Height - toast.Height) / 2);
            toast.Show(parent);
            toast.Refresh();

            toast.AutocloseSeconds = timeout;

            return toast;
        }



        private void Toast_Activated(object sender, EventArgs e)
        {
            buttonFooFocus.Height = 0;
            buttonFooFocus.Width = 0;
            buttonFooFocus.SendToBack();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            _AutocloseTimer?.Stop();
            _AutocloseTimer = null;

            this.Close();
        }
    }
}
