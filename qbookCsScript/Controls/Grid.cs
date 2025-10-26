
using System;
using System.Drawing;


namespace QB.Controls
{
    class Controls
    {
        /*
        public static void Grid(Chart c)
        {
            if (c.AxisY1.MinorTicks == c.AxisY1.MajorTicks)
                return;
            int subTicks = (int)c.AxisX.MinorTicks / 1000;
            int ticks = (int)c.AxisX.MajorTicks / 1000;

            if (subTicks == 0)
                return;
            if (ticks == 0)
                return;

            double seconds = c.AxisX.Range / 1000;
            if (seconds > 0)
            {
                for (int i = 0; i < subTicks + 1; i++)
                {
                    double x = Draw.scale(i, 0, subTicks, c.Bounds.X, c.Bounds.X + c.Bounds.W);
                    Draw.Line(Draw.penMinorTicksLight, x, c.Bounds.Y, x, c.Bounds.Y + c.Bounds.H);
                }
                for (int i = 0; i < ticks + 1; i++)
                {
                    double x = Draw.scale(i, 0, ticks, c.Bounds.X, c.Bounds.X + c.Bounds.W);
                    Draw.Line(Draw.penMajorTicksLight, x, c.Bounds.Y, x, c.Bounds.Y + c.Bounds.H);
                    if ((i % 2) == 0)
                    {
                        double s = seconds - (seconds * i) / ticks;
                        DateTime date = DateTime.Now.AddSeconds(-s);
                        if (i > 0)
                        {
                            if (i == ticks)
                                Draw.Text(date.ToString("HH:mm:ss"), x, c.Bounds.Y + c.Bounds.H + 0.5f, 0, Draw.fontTerminalFixed, Color.DimGray, System.Drawing.ContentAlignment.MiddleCenter);
                            else
                            {
                                if (seconds <= 600)
                                {
                                    Draw.Text("-" + (s) + "s", x, c.Bounds.Y + c.Bounds.H + 0.5f, 0, Draw.fontTerminalFixed, Color.DimGray, System.Drawing.ContentAlignment.MiddleCenter);

                                }
                                else
                                {
                                    Draw.Text("-" + (Math.Round(s / 60, 2)) + "min", x, c.Bounds.Y + c.Bounds.H + 0.5f, 0, Draw.fontTerminalFixed, Color.DimGray, System.Drawing.ContentAlignment.MiddleCenter);

                                }
                            }
                        }
                    }
                }
            }

            for (double v = c.AxisY1.Min; v <= c.AxisY1.Max; v += c.AxisY1.MinorTicks)
            {
                double y = Draw.scale(v, c.AxisY1.Min, c.AxisY1.Max, c.Bounds.Y + c.Bounds.H, c.Bounds.Y);
                Draw.Line(Draw.penMinorTicksLight, c.Bounds.X, y, c.Bounds.X + c.Bounds.W, y);
            }

            for (double v = c.AxisY1.Min; v <= c.AxisY1.Max; v += c.AxisY1.MajorTicks)
            {
                double y = Draw.scale(v, c.AxisY1.Min, c.AxisY1.Max, c.Bounds.Y + c.Bounds.H, c.Bounds.Y);
                Draw.Line(Draw.penMajorTicksLight, c.Bounds.X, y, c.Bounds.X + c.Bounds.W, y);
                Draw.Text(v.ToString("0"), c.Bounds.X - 0.1f, y - 1.4f, 0, Draw.fontTerminalFixed, Color.DimGray, System.Drawing.ContentAlignment.MiddleRight);
            }
        }
        */
    }
}
