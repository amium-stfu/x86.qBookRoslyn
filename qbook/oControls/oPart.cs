using QB.Controls;
using System;
using System.Drawing;

namespace qbook
{
    public enum PartType { None, Valve22, Valve32, Pump, Filter, PC, TC };
    [Serializable]
    public class oPart : oControl
    {
        public oPart()
        {
        }
        public oPart(PartType type)
            : base()
        {
            PartType = type;
        }

        public PartType PartType;

        SolidBrush Honeydew = new SolidBrush(Color.Honeydew);
        SolidBrush Green = new SolidBrush(Color.Green);
        SolidBrush LightGreen = new SolidBrush(Color.LightGreen);
        SolidBrush Black = new SolidBrush(Color.Black);
        Pen penP = new Pen(Color.Black, 2);

        public override void Render()
        {
            double y_2 = Bounds.Y + ((Bounds.H - 0) / 2);
            double x_2 = Bounds.X + ((Bounds.W - 0) / 2);
            double xmax = Bounds.X + Bounds.W - 2;
            double ymax = Bounds.Y + Bounds.H - 2;
            double xmin = Bounds.X + 2;
            double ymin = Bounds.Y + 2;

            Frame(true, true);

            penP.Width = 1 * (float)Draw.mmToPx;

            if (PartType == PartType.Valve22)
            {
                //     Draw.Rectangle(Selected ? Green : , Bounds.X, Bounds.Y, Bounds.W, Bounds.H, 0.5f);                

                Draw.Line(penP, xmin, ymin, xmax, ymax);
                Draw.Line(penP, xmax, ymax, xmax, ymin);
                Draw.Line(penP, xmax, ymin, xmin, ymax);
                Draw.Line(penP, xmin, ymax, xmin, ymin);

                Draw.Line(penP, xmin - 2, y_2, xmin, y_2);
                Draw.Line(penP, xmax, y_2, xmax + 2, y_2);

                return;
            }

            if (PartType == PartType.Pump)
            {
                //    Draw.Rectangle(Selected ? Green : LightGreen, Bounds.X, Bounds.Y, Bounds.W, Bounds.H, 1);
                Draw.Circle(Color.Black, x_2, y_2, (ymax - ymin), 1);
                Draw.Line(penP, xmin - 2, y_2, xmin, y_2);
                Draw.Line(penP, xmax, y_2, xmax + 2, y_2);
                return;
            }

            if (PartType == PartType.Filter)
            {
                //  Draw.Rectangle(Selected ? Green : LightGreen, Bounds.X, Bounds.Y, Bounds.W, Bounds.H, 1);
                Draw.Rectangle(Draw.GetPen2(Color.Black, 1), xmin, ymin, xmax - xmin, ymax - ymin);
                Draw.Line(penP, x_2, ymin, x_2, ymax);
                Draw.Line(penP, xmin - 2, y_2, xmin, y_2);
                Draw.Line(penP, xmax, y_2, xmax + 2, y_2);
                return;
            }
            Draw.FillRectangle(Selected ? Honeydew : Honeydew, Bounds.X, Bounds.Y, Bounds.W, Bounds.H);
            Draw.Rectangle(Draw.GetPen2(Selected ? Color.Green : Color.LightGreen, 1), Bounds.X, Bounds.Y, Bounds.W, Bounds.H);
        }
    }
}
