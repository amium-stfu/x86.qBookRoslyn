using System;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace QB.UI
{
    public partial class ParameterControl : UserControl
    {
        internal Signal MySignal;
        public ParameterControl(string name, Signal signal, string description = null)
        {
            InitializeComponent();
            MySignal = signal;
            if (signal != null)
            {
                labelName.Text = signal.Name + ":" + signal.Text + " (" + name + ")";
                textBoxValue.Text = signal.Value.ToString();
                labelUnit.Text = signal.Unit;
                labelDescription.Text = description;
                labelValueRule.Text = signal.ValueRule;
            }
            else
            {
                labelName.Text = "<null>";
                textBoxValue.Text = "<null>";
                labelUnit.Text = "";
                labelDescription.Text = description;
            }

        }

        public void SetEditor(string editor)
        {
            editor = editor.Replace("..", "§");
            Regex sliderEditorRegex = new Regex(@"Slider:(?<min>[\d\.]*)§(?<max>[\d\.]*)(:(?<step>[\d\.]*))?");
            Match m = sliderEditorRegex.Match(editor);
            if (m.Success)
            {
                double.TryParse(m.Groups["min"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double min);
                double.TryParse(m.Groups["max"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double max);
                double.TryParse(m.Groups["step"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double step);

                DoubleTrackBar tb = new DoubleTrackBar();
                tb.Minimum = min;
                tb.Maximum = max;
                tb.SmallChange = step;
                tb.LargeChange = step * 10.0;

                tb.Location = new Point(labelDescription.Left, 0);
                tb.Size = new Size(120, 26);
                labelDescription.Visible = false;
                this.Controls.Add(tb);
                tb.ValueChanged += Tb_ValueChanged;
            }
        }

        private void Tb_ValueChanged(object sender, EventArgs e)
        {
            DoubleTrackBar tb = sender as DoubleTrackBar;
            MySignal.Value = tb.Value;
            textBoxValue.Text = MySignal.Value.ToString("#.00");
        }

        public ParameterControl(string name, object source, string description = null)
        {
            InitializeComponent();
            //Parameter = parameter;
            labelName.Text = name;
            textBoxValue.Text = "n/a";
            labelDescription.Text = description;
        }

        public string ValueString
        {
            get
            {
                return textBoxValue.Text;
            }
            set
            {
                textBoxValue.Text = value;
            }
        }

        public double Value
        {
            get
            {
                double.TryParse(ValueString.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double d);
                return d;
            }
            set
            {
                textBoxValue.Text = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }


        class DoubleTrackBar : TrackBar
        {
            private double precision = 0.01f;

            public double Precision
            {
                get { return precision; }
                set
                {
                    precision = value;
                    // todo: update the 5 properties below
                }
            }
            public new double LargeChange
            { get { return base.LargeChange * precision; } set { base.LargeChange = (int)(value / precision); } }
            public new double Maximum
            { get { return base.Maximum * precision; } set { base.Maximum = (int)(value / precision); } }
            public new double Minimum
            { get { return base.Minimum * precision; } set { base.Minimum = (int)(value / precision); } }
            public new double SmallChange
            { get { return base.SmallChange * precision; } set { base.SmallChange = (int)(value / precision); } }
            public new double Value
            { get { return base.Value * precision; } set { base.Value = (int)(value / precision); } }
        }

    }
}
