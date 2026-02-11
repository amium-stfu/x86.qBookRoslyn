
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace QB.Controls
{
    public class Gauge : Control
    {

        public Gauge(string name, double x = 0, double y = 0, double w = 30, double h = 30) : base(name, x: x, y: y, w: w, h: h)
        {
        }
    }

    public class RadialGauge : Gauge
    {
        public RadialGauge(string name, double x = 0, double y = 0, double w = 30, double h = 30) : base(name, x: x, y: y, w: w, h: h)
        {
            Axis.Min = 0;
            Axis.Max = 100;
            Axis.MinorTicks = 5;
            Axis.MajorTicks = 10;
        }

        Signal _Signal = null;
        public Signal Signal
        {
            get
            {
                return _Signal;
            }
            set
            {
                _Signal = value;
                if (_Signal.getRecorder() == null)
                    _Signal.initRecorder(100000); //HALE: we need fewer items here than in a chart?!
            }
        }
        //      public Module Module = null;


        public override void Drag(System.Drawing.PointF point)
        {
                double x = (point.X - (Bounds.X + Bounds.W)) / Bounds.W;
                double y = -(point.Y - (Bounds.Y + Bounds.H)) / Bounds.H;
                double _angle = (float)Math.Atan2(y, x);
                double angle = _angle * 180.0f / Math.PI;
                double set = Draw.scale(angle, 180, 90, Axis.Min, Axis.Max);



                for (double v = Axis.Min; v <= Axis.Max; v += Axis.MinorTicks)
                    if (v > set)
                    {
                        if ((v - set) > (set - (v - Axis.MinorTicks)))
                            set = v - Axis.MinorTicks;
                        else
                            set = v;
                        break;
                    }

                if (Signal is Module)
                    (Signal as Module).Set.Value = set;

        }

        internal override void Render(Control parent)
        {
            base.Render(parent);

            if (Signal == null)
            {
                Draw.Rectangle(Draw.penMajorTicks, Bounds.X + 0.5f, Bounds.Y + 0.5f, Bounds.W - 1, Bounds.H - 1);
                Draw.Text("Gauge '" + this.Name + "'\n<signal not set>", Bounds.X + Bounds.W / 2, Bounds.Y + Bounds.H / 2 - 2.5f, 0, Draw.fontTextFixed, Draw.penMajorTicks.Color, ContentAlignment.MiddleCenter);
                return;
            }

            try
            {
                double minAngle = 180;
                double maxAngle = 90;
                double deltaAngle = Math.Abs(minAngle - maxAngle);

                for (double v = Axis.Min; v <= Axis.Max; v += Axis.MajorTicks)
                {
                    double angle = Draw.scale(v, Axis.Min, Axis.Max, minAngle, maxAngle);
                    PointF p = Draw.pointG(Bounds, angle, 1);
                    Draw.Line(Draw.penMajorTicks, Draw.pointG(Bounds, angle, 0.94f), Draw.pointG(Bounds, angle, 1.0f));
                    Draw.Text("" + v, p.X - 1, p.Y - 4, 1, Draw.fontFootnoteFixed, System.Drawing.Color.DimGray, ContentAlignment.MiddleRight);
                }

                var recorder = Signal.getRecorder();
                if (recorder != null)
                {
                    var mini = recorder.GetMiniSnapshot();
                    var miniMin = recorder.GetMiniMinSnapshot();
                    var miniMax = recorder.GetMiniMaxSnapshot();

                    List<PointF> line = new List<PointF>();
                    List<PointF> lineMin = new List<PointF>();
                    List<PointF> lineMax = new List<PointF>();
                    List<PointF> lineRange = new List<PointF>();

                    double lastAngle = double.NaN;
                    double lastAngleMin = double.NaN;
                    double lastAngleMax = double.NaN;

                    int j = 0;
                    foreach (var item in mini)
                    {
                        if (!double.IsNaN(item.Value))
                        {
                            double progress = 0.5f + j * 0.5f / Collections.MINISIZE;

                            double angle = Draw.scale(item.Value, Axis.Min, Axis.Max, minAngle, maxAngle);
                            lastAngle = SmoothStep(line, lastAngle, angle, progress);

                            if (j < miniMin.Length)
                            {
                                double angleMin = Draw.scale(miniMin[j].Value, Axis.Min, Axis.Max, minAngle, maxAngle);
                                lastAngleMin = SmoothStep(lineMin, lastAngleMin, angleMin, progress);
                            }

                            if (j < miniMax.Length)
                            {
                                double angleMax = Draw.scale(miniMax[j].Value, Axis.Min, Axis.Max, minAngle, maxAngle);
                                lastAngleMax = SmoothStep(lineMax, lastAngleMax, angleMax, progress);
                            }
                        }
                        j++;
                    }

                    lineRange.AddRange(lineMin);
                    lineRange.AddRange(lineMax.Reverse<PointF>());
                    //STFU 2025-07-24
                    if(lineRange.Count > 3)
                    Draw.FillPolygon(Brushes.Lavender, lineRange.ToArray(), true);
                    //

                    Draw.Lines(Pens.LightSteelBlue, lineMin.ToArray(), true);
                    Draw.Lines(Pens.LightSteelBlue, lineMax.ToArray(), true);
                    Draw.Lines(Pens.SteelBlue, line.ToArray(), true);
                }

                for (double v = Axis.Min; v <= Axis.Max; v += Axis.MinorTicks)
                {
                    double angle = Draw.scale(v, Axis.Min, Axis.Max, minAngle, maxAngle);
                    Draw.Line(Draw.penMinorTicks, Draw.pointG(Bounds, angle, 0.496f), Draw.pointG(Bounds, angle, 1.0f));
                }

                if (!double.IsNaN(Signal.Value))
                {
                    double angle = Draw.scale(Signal.Value, Axis.Min, Axis.Max, minAngle, maxAngle);
                    Pen pen = new Pen(Signal.Color, 3f);
                    Draw.Line(pen, Draw.pointG(Bounds, (float)angle, 0.5f), Draw.pointG(Bounds, (float)angle, 0.97f));
                }

                if (Signal is Module)
                {
                    Module module = Signal as Module;
                    string subInfo = "";

                    if (!double.IsNaN(module.Set.Value))
                    {
                        double angle = Draw.scale(module.Set.Value, Axis.Min, Axis.Max, minAngle, maxAngle);
                        Draw.Line(Draw.penSet, Draw.pointG(Bounds, angle, 0.95f), Draw.pointG(Bounds, angle, 0.99f));
                        subInfo += module.Set.Value.ToString("0.00");
                    }

                    if (!double.IsNaN(module.Out.Value))
                        subInfo += "(" + module.Out.Value.ToString("0.0") + "%)";

                    Draw.Text(subInfo, Bounds.X + Bounds.W * 0.79f, Bounds.Y + Bounds.H * 0.75f + 10, 0, Draw.fontTerminalFixed, Signal.Color, ContentAlignment.MiddleCenter);

                    string nameText = module.Name ?? Signal.Name;
                    Draw.Text(nameText, Bounds.X + Bounds.W * 0.79f, Bounds.Y + Bounds.H * 0.75f, 0, Draw.fontTextFixed, Signal.Color, ContentAlignment.MiddleCenter);
                }

                Draw.Text(Signal.Value.ToString("0.0"), Bounds.X + Bounds.W * 0.79f, Bounds.Y + Bounds.H * 0.75f + 5, 0, Draw.fontTextFixed, Signal.Color, ContentAlignment.MiddleCenter);
            }
            catch
            {
                // optional: Log oder visualer Fallback
            }
        }

        private double SmoothStep(List<PointF> list, double lastAngle, double targetAngle, double progress)
        {
            if (double.IsNaN(lastAngle))
                lastAngle = targetAngle;

            int maxLoop = 50;
            while (Math.Abs(lastAngle - targetAngle) > 11 && maxLoop-- > 0)
            {
                lastAngle += (lastAngle < targetAngle) ? 10 : -10;
                list.Add(Draw.pointG(Bounds, lastAngle, progress));
            }

            list.Add(Draw.pointG(Bounds, targetAngle, progress));
            return targetAngle;
        }


        public Drawing.Axis Axis = new Drawing.Axis();
        //    public string link;


    }
}
