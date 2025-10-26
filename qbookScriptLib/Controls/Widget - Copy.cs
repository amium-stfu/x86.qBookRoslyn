
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

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
        public Widget(string name, Signal signal = null, double x = 0, double y = 0, double w = 30, double h = 15) :base(name, x:x, y:y, w:w, h:h)
        {
            
            this.Signal = signal;
        }

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
                    Draw.Text(Signal.Text, Bounds.X + Bounds.W * 0.5f, Bounds.Y, 0, Draw.fontTextFixed, Signal.Color, System.Drawing.ContentAlignment.MiddleCenter);

<<<<<<< HEAD
                Draw.Text(Signal.Value.ToString("0.0"), Bounds.X + Bounds.W * 0.5f, Bounds.Y + 5, 0, Draw.fontTextFixed, Signal.Color, System.Drawing.ContentAlignment.MiddleCenter);
=======

                string value = Signal.Value.ToString("0.0") + (Signal.Unit == null ? "" : " " + Signal.Unit);

                Draw.Text(value, Bounds.X + Bounds.W * 0.5f, Bounds.Y + 5, 0, Draw.fontTextFixed, Signal.Color, Draw.Alignment.C);
>>>>>>> 2e559cf9a745ab27a7168025976681e3e8872392

            }

            if (Signal is Module)
            {
                Module module = Signal as Module;
              //  string col = module.Color;// Qb.GetS(parent?.FullName, readName + ".color");
               // System.Drawing.Color color = Misc.ParseColor(col);

                string subInfo = "";
                if (!double.IsNaN(module.Out.Value))
                {
                    subInfo += "(" + module.Out.Value.ToString("0.0") + "%)";
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
