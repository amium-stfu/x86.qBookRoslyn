using System;
using System.Drawing;
using System.Windows.Forms;

namespace QB.Controls
{
    public class Widget : Control
    {
        /*
        public Widget(string text, Module module, string id = null) : base(text, id)
        {
            this.Bounds = new Rectangle(0, 0, 30, 15);
            this.Module = module;
        }
        */
        public Widget(string name, Signal signal = null, double x = 0, double y = 0, double w = 30, double h = 15, string displayFormat = null) : base(name, x: x, y: y, w: w, h: h)
        {
            this.Signal = signal;
            this.DisplayFormat = displayFormat;
        }

        public Widget(Signal signal, double x = 0, double y = 0, double w = 30, double h = 15, string displayFormat = null) : base(signal.Name, x: x, y: y, w: w, h: h)
        {
            this.Signal = signal;
            this.DisplayFormat = displayFormat;
        }

        public void HandleClick(PointF point)
        {
            try
            {
                double deltaX = point.X - Bounds.X;
                double deltaY = point.Y - Bounds.Y;

                if (Signal != null)
                {
                    QB.UI.TagsDialog dialog = new QB.UI.TagsDialog();
                    dialog.Signal = Signal; // "CELL VALUE";

                    dialog.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
                    var parentForm = System.Windows.Forms.Application.OpenForms[0];
                    dialog.Location = new System.Drawing.Point(parentForm.Left + (parentForm.Width - dialog.Width) / 2, parentForm.Top + (parentForm.Height - dialog.Height) / 2);

                    var dr = dialog.ShowDialog();
                    //    value = dialog.Value;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"#EX: " + ex.Message, "ERROR");
            }
        }

        public string DisplayFormat = null; //HALE: temporary?!

        //     public Module Module = null;
        public Signal Signal = null;

        internal override void Render(Control parent)
        {

            if (Signal != null)
            {
                //     string col = Signal.Color;// Qb.GetS(parent?.FullName, readName + ".color");
                //    System.Drawing.Color color = Misc.ParseColor(col);

                string subInfo = "";

                Draw.Text(subInfo, Bounds.X + Bounds.W * 0.5f, Bounds.Y + 10, 0, Draw.fontTerminalFixed, Signal.Color, System.Drawing.ContentAlignment.MiddleCenter);
                //string text = Qb.GetS(null, readName + ".text");// Qb.ScriptingEngine.InterpretScript(Qb.GetS(name + ".text"))?.ToString();
                //       string text = Qb.ScriptingEngine.InterpretScript(c.textScript)?.ToString();

                if (Signal.Text != null)
                    Draw.Text(Signal.Text, Bounds.X + Bounds.W * 0.5f, Bounds.Y, 0, Draw.fontFootnote, Signal.Color, System.Drawing.ContentAlignment.MiddleCenter);



                string value = "";
                /* //@SCAN 2023-07
                if (DisplayFormat == null)
                {
                    if (Signal.Value < 0.05)
                        value = Signal.Value.ToString("0.00000") + (Signal.Unit == null ? "" : " " + Signal.Unit);
                    else if (Signal.Value < 0.5)
                        value = Signal.Value.ToString("0.0000") + (Signal.Unit == null ? "" : " " + Signal.Unit);
                    else if (Signal.Value < 5)
                        value = Signal.Value.ToString("0.000") + (Signal.Unit == null ? "" : " " + Signal.Unit);
                    else
                        value = Signal.Value.ToString("0.00") + (Signal.Unit == null ? "" : " " + Signal.Unit);
                }
                else
                {
                    value = Signal.Value.ToString(DisplayFormat) + (Signal.Unit == null ? "" : " " + Signal.Unit);
                }
                */
                value = Signal.ToString();

                Draw.Text(value, Bounds.X + Bounds.W * 0.5f, Bounds.Y + 5, 0, Draw.fontTextFixed, Signal.Color, System.Drawing.ContentAlignment.MiddleCenter);

            }

            if (Signal is Module)
            {
                Module module = Signal as Module;
                //  string col = module.Color;// Qb.GetS(parent?.FullName, readName + ".color");
                // System.Drawing.Color color = Misc.ParseColor(col);



                string subInfo = "";

                if (!double.IsNaN(module.PreSet.Value))
                {
                    subInfo += "PS:" + module.PreSet.Value.ToString("0.00") + " ";
                }
                if (!double.IsNaN(module.Set.Value))
                {
                    subInfo += "S:" + module.Set.Value.ToString("0.00");
                }
                if (!double.IsNaN(module.Out.Value))
                {
                    subInfo += "O:" + module.Out.Value.ToString("0.0") + "%";
                }

                Draw.Text(subInfo, Bounds.X + Bounds.W * 0.5f, Bounds.Y + 10, 0, Draw.fontTerminalFixed, module.Color, System.Drawing.ContentAlignment.MiddleCenter);
                //string text = Qb.GetS(null, readName + ".text");// Qb.ScriptingEngine.InterpretScript(Qb.GetS(name + ".text"))?.ToString();
                //       string text = Qb.ScriptingEngine.InterpretScript(c.textScript)?.ToString();

                /*
                if (module.Text != null)
                    Draw.Text(module.Text, Bounds.X + Bounds.W * 0.5f, Bounds.Y, 0, Draw.fontTextFixed, color, Draw.Alignment.C);
               
                Draw.Text(module.Value.ToString("0.0"), Bounds.X + Bounds.W * 0.5f, Bounds.Y + 5, 0, Draw.fontTextFixed, color, Draw.Alignment.C);
                */

            }
            System.Drawing.Pen pen = Draw.GetPen2(System.Drawing.Color.DarkGray, 0.2);
            Draw.Rectangle(pen, Bounds.X, Bounds.Y, Bounds.W, Bounds.H);
            base.Render(parent);
        }
    }
}
