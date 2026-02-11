using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace QB.Controls
{
    public class ChartXY : Control
    {


        public ChartXY(string name, double x = 0, double y = 0, double w = 30, double h = 30) : base(name, x: x, y: y, w: w, h: h)
        {
            AxisX.Min = 0;
            AxisX.Max = 100;
            //AxisX.Range = 100 * 1000;
            AxisX.MinorTicks = 5;// * 1000;
            AxisX.MajorTicks = 10;// * 1000;

            AxisY1.Min = 0;
            AxisY1.Max = 100;
            AxisY1.MinorTicks = 5;
            AxisY1.MajorTicks = 10;
            //    this.Text = text;
        }

        public class Series
        {
            public string Name;
            public string Text;
            public List<Point> Points = new List<Point>();
            public System.Drawing.Color Color = System.Drawing.Color.Blue;
            public double Width = 1.0;
            public string LineStyle = null; //dash, dot, etc...
            public bool Visible = true;
        }

        public class Point
        {
            public double X;
            public double Y;
            public double Z = double.NaN; //maybe 3d sometime?!
            public bool Visible = true;

            public Point()
            {
            }
            public Point(double x, double y, double z = double.NaN)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        Dictionary<string, Series> SeriesDict = new Dictionary<string, Series>();

        public Series AddSeries(string name, Color color, double width = 1.0)
        {
            lock (SeriesDict)
            {
                if (!SeriesDict.ContainsKey(name))
                    SeriesDict.Add(name, new Series() { Name = name, Color = color, Width = width });
            }
            return SeriesDict[name];
        }

        public void RemoveSeries(string name)
        {
            lock (SeriesDict)
            {
                if (SeriesDict.ContainsKey(name))
                    SeriesDict.Remove(name);
            }
        }

        public void RemoveSeries(Series series)
        {
            lock (SeriesDict)
            {
                if (SeriesDict.ContainsValue(series))
                    SeriesDict.Remove(series.Name);
            }
        }

        public void ClearPoints(Series s)
        {
            lock (s.Points)
            {
                s.Points.Clear();
            }
        }
        public void ClearPoints(string seriesName)
        {
            if (SeriesDict.ContainsKey(seriesName))
                ClearPoints(SeriesDict[seriesName]);
        }

        public void ClearAll(Series s)
        {
            SeriesDict.Clear();
        }


        public void AddPoint(Series s, double x, double y)
        {
            if (!SeriesDict.ContainsKey(s.Name))
                SeriesDict.Add(s.Name, s);

            lock (s.Points)
            {
                s.Points.Add(new Point() { X = x, Y = y });
            }
        }
        public void AddPoint(string seriesName, double x, double y)
        {
            lock (SeriesDict)
            {
                if (SeriesDict.ContainsKey(seriesName))
                    AddPoint(SeriesDict[seriesName], x, y);
            }
        }

        public void AddPoints(Series s, List<double> points) //list of x and y's (even number!)
        {
            if (!SeriesDict.ContainsKey(s.Name))
                SeriesDict.Add(s.Name, s);

            lock (s.Points)
            {
                for (int i = 0; i < points.Count; i += 2)
                {
                    s.Points.Add(new Point() { X = points[i], Y = points[i + 1] });
                }
            }
        }
        public void AddPoints(string seriesName, List<double> points)
        {
            lock (SeriesDict)
            {
                if (SeriesDict.ContainsKey(seriesName))
                    AddPoints(SeriesDict[seriesName], points);
            }
        }




        /*
         * public class CControl
        {
            Dictionary<string, Control> Dict = new Dictionary<string, Control>();

            public bool Contains(string name)
            {
                return Dict.ContainsKey(name);
            }

            public List<Control> Values
            {
                get
                {
                    return Dict.Values.ToList();
                }
            }
            public Control this[string name]
            {
                get
                {
                    if (Dict.ContainsKey(name))
                    {
                        return Dict[name];
                    }
                    else
                    {
                        //return null;
                        var newItem = new Control(name);
                        Dict.Add(name, newItem);
                        return newItem;
                    }
                }
                set
                {
                    if (Dict.ContainsKey(name))
                    {
                        Dict[name] = value;
                    }
                    else
                    {
                        //    var newItem = new Module(key);
                        Dict.Add(name, value);
                    }
                }
            }
        }

        public CControl Control = new CControl();
        */




        //   public ConcurrentDictionary<string, Signal> Signals = new ConcurrentDictionary<string, Signal>();
        //  public ConcurrentDictionary<string, Control> Controls = new ConcurrentDictionary<string, Control>();

        /*
        public Signal Signal(string key)
        {
            lock (Signals)
            {
                if (!Signals.ContainsKey(key))
                    Signals.Add(key, new Signal(key));
                return Signals[key];
            }
        }
        public Signal Signal(string key, Signal signal)
        {
            lock (Signals)
            {
                if (!Signals.ContainsKey(key))
                    Signals.Add(key, null);
                Signals[key] = signal;
                return Signals[key];
            }
        }
        */
        /*
         public void addsignal(object read)//, object set, object @out)
         {
             if (read is WrapperObject)
             {
                 if ((read as WrapperObject).Content is Signal)
                     sources.Add((read as WrapperObject).Content as Signal);
             }
         }
         public void addmodule(object read)//, object set, object @out)
         {
             if (read is WrapperObject)
             {
                 if ((read as WrapperObject).Content is Module)
                 {
                     Module module = (read as WrapperObject).Content as Module;
                     if (module.read != null)
                         sources.Add(module.read);
                     if (module.set != null)
                         sources.Add(module.set);
                     if (module.@out != null)
                         sources.Add(module.@out);
                 }
             }
         }*/
        internal override void Render(Control parent)
        {
            base.Render(parent);

            try
            {
                Draw.SetClip(Bounds.X, Bounds.Y, Bounds.W, Bounds.H);
                Grid(this);

                foreach (Series series in SeriesDict.Values.ToList())
                {
                    if (!series.Visible)
                        continue;
                    Pen pen = new Pen(series.Color, (float)series.Width);
                    //PointF[] points = series.Points.Where(p => p.Visible).Select(p => new PointF((float)p.X, (float)p.Y)).ToArray();
                    List<PointF> points = new List<PointF>();
                    foreach (Point p in series.Points.Where(p => p.Visible))
                    {
                        //double x = Draw.scale(timeSpan.TotalMilliseconds, 0, seconds * 1000, Bounds.X, Bounds.X + Bounds.W);
                        double x = Draw.scale(p.X, AxisX.Min, AxisX.Max, Bounds.X, Bounds.X + Bounds.W);
                        double y = Draw.scale(p.Y, AxisY1.Min, AxisY1.Max, Bounds.Y + Bounds.H, Bounds.Y);
                        points.Add(new PointF((float)(x * Draw.mmToPx), (float)(y * Draw.mmToPx)));
                    }

                    Draw.Lines(pen, points.ToArray(), false);
                }

                //Legend
                double maxW = 0;
                foreach (Series series in SeriesDict.Values.ToList())
                {
                    if (!series.Visible)
                        continue;
                    string text = (series.Text ?? series.Name).PadLeft(20);
                    SizeF size = Draw.g.MeasureString(text, Draw.fontTerminalFixed);
                    maxW = Math.Max(maxW, size.Width / Draw.mmToPx);
                }

                double yPos = 0;
                foreach (Series series in SeriesDict.Values.ToList())
                {
                    if (!series.Visible)
                        continue;
                    string text = (series.Text ?? series.Name).PadLeft(20);
                    SizeF size = Draw.g.MeasureString(text, Draw.fontTerminalFixed);
                    double w = maxW;// size.Width / Draw.mmToPx;
                    double h = size.Height / Draw.mmToPx;
                    Draw.FillRectangle(Draw.WhiteBrush, Bounds.X + 4.5, Bounds.Y + 2.5 + yPos, w + 1, h + 0);
                    Draw.Text(text, Bounds.X + 5, Bounds.Y + 3 + yPos, 30, Draw.fontTerminalFixed, series.Color, ContentAlignment.MiddleLeft);
                    yPos += 3.5;
                }
            }
            catch { }
            finally { Draw.ClearClip(); }
        }
        public Drawing.Axis AxisX = new Drawing.Axis();
        public Drawing.Axis AxisY1 = new Drawing.Axis();
        public DateTime Tiggerdate = DateTime.MinValue;


        public static void Grid(ChartXY c)
        {
            //if (c.AxisY1.MinorTicks == c.AxisY1.MajorTicks)
            //    return;
            //int subTicks = (int)c.AxisX.MinorTicks / 1000;
            //int ticks = (int)c.AxisX.MajorTicks / 1000;

            //if (subTicks == 0)
            //    return;
            //if (ticks == 0)
            //    return;

            for (double v = c.AxisX.Min; v <= c.AxisX.Max; v += c.AxisX.MinorTicks)
            {
                double x = Draw.scale(v, c.AxisX.Min, c.AxisX.Max, c.Bounds.X, c.Bounds.X + c.Bounds.W);
                Draw.Line(Draw.penMinorTicksLight, x, c.Bounds.Y, x, c.Bounds.Y + c.Bounds.H);
            }
            for (double v = c.AxisX.Min; v <= c.AxisX.Max; v += c.AxisX.MajorTicks)
            {
                double x = Draw.scale(v, c.AxisX.Min, c.AxisX.Max, c.Bounds.X, c.Bounds.X + c.Bounds.W);
                Draw.Line(Draw.penMajorTicksLight, x, c.Bounds.Y, x, c.Bounds.Y + c.Bounds.H);

                //Draw.FillRectangle(Draw.WhiteBrush, c.Bounds.X + 0.1f, y - 2f, 8, 4);
                Draw.Text(v.ToString("0"), x, c.Bounds.Y + c.Bounds.H - 2.8f, 0, Draw.fontTerminalFixed, System.Drawing.Color.DimGray, System.Drawing.ContentAlignment.MiddleLeft);
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

                Draw.FillRectangle(Draw.WhiteBrush, c.Bounds.X + 0.1f, y - 2f, 8, 4);
                Draw.Text(v.ToString("0"), c.Bounds.X + 0.1f, y - 1.4f, 0, Draw.fontTerminalFixed, System.Drawing.Color.DimGray, System.Drawing.ContentAlignment.MiddleLeft);
            }
        }
    }
}
