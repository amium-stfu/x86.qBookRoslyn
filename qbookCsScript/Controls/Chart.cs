using log4net.Appender;
//using PdfSharp.Charting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Media.TextFormatting;
using QB.Helpers;
using QB.Logging;

namespace QB.Controls
{
    public class Chart : Control
    {

        static bool USE_CLIP = true;
        public SignalCsvLogger Log;

        public class Layout
        {
            public int yAxis { get; set; }
        }


        public bool SignalLegendVisible = true;

        public Chart(string name, double x = 0, double y = 0, double w = 30, double h = 30) : base(name, x: x, y: y, w: w, h: h)
        {
            AxisX.Range = 100 * 1000;
            AxisX.MinorTicks = 5 * 1000;
            AxisX.MajorTicks = 10 * 1000;
            //@STFU 2024-11-12
            Log = new SignalCsvLogger(name);
            


            /*
            AxisY1.Min = 0;
            AxisY1.Max = 100;
            AxisY1.MinorTicks = 5;
            AxisY1.MajorTicks = 10;*/
            //    this.Text = text;
        }

        //public void Add(Signal signal)
        //{
        //    if (signal.getRecorder() == null)
        //        signal.initRecorder(100000); // (Collections.RECORDERSIZE_);
        //    Signals[signal.Name] = signal;
        //    Drawing.Axis a = Axes[signal.Name];
        //}
        public void Add(Signal signal, Drawing.Axis axis = null)
        {
            //@STFU 2024-11-12 AxisY1 as default
            if (axis == null) axis = AxisY1;
            {
                Debug.WriteLine("Add signal to Axis Y1: " + signal.Name);
                if (signal.getRecorder() == null)
                    signal.initRecorder(100000); // (Collections.RECORDERSIZE_);
                Signals[signal.Name] = signal;
                Axes[signal.Name] = axis;
                //@STFU 2024-11-12
                Log.Add(signal);

            }

            
            if (signal.getRecorder() == null)
                signal.initRecorder(100000); // (Collections.RECORDERSIZE_);
            Signals[signal.Name] = signal;
            Axes[signal.Name] = axis;


        }

        public void Remove(Signal signal)
        {
            lock (Signals.Values)
            {
                if (Signals.Values.Contains(signal))
                    Signals.Values.Remove(signal);
            }
        }
        public void Remove(string signalName) //by Name
        {
            lock (Signals.Values)
            {
                var signal = Signals.Values.FirstOrDefault(x => x.Name == signalName);
                if (signal != null)
                    Signals.Values.Remove(signal);
            }
        }
        public void RemoveAll()
        {
            Signals.Values.Clear();
        }



        public class CAxis
        {
            Dictionary<string, Drawing.Axis> Dict = new Dictionary<string, Drawing.Axis>();

            public List<Drawing.Axis> Values
            {
                get
                {
                    return Dict.Values.ToList();
                }
            }
            public Drawing.Axis this[string name]
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
                        var newItem = new Drawing.Axis();
                        newItem.Min = 0;
                        newItem.Max = 100;
                        newItem.MinorTicks = 5;
                        newItem.MajorTicks = 10;

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

        public class CSignal
        {
            Dictionary<string, Signal> Dict = new Dictionary<string, Signal>();

            public List<Signal> Values
            {
                get
                {
                    return Dict.Values.ToList();
                }
            }
            public Signal this[string name]
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
                        var newItem = new Module(name);
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

        public CSignal Signals = new CSignal();
        public CAxis Axes = new CAxis();

        public class CControl
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
            //  qbModules.Signal signal = null;
            //  signal = Qb.GetSignal(parent?.FullName, onclick);

            try
            {
                //              if (grid)
                if (USE_CLIP)
                    Draw.SetClip(Bounds.X, Bounds.Y, Bounds.W, Bounds.H);




                //**  

                //   Signal s = Qb.GetSignal(parent?.FullName, onclick);
                //   if (s != null)
                //   sources.Add(s);



                List<Drawing.Axis> drawed = new List<Drawing.Axis>();


                int xOffset = 0;

                bool xAxis = true;

                foreach (Signal signal in Signals.Values.ToList())
                {
                    Pen pen = new Pen(signal.Color, 1f);
                    Drawing.Axis axis = Axes[signal.Name];

                    if (!drawed.Contains(axis))
                    {
                        Grid(this, signal, xOffset, xAxis);
                        xOffset += 12;
                        drawed.Add(axis);
                        xAxis = false;
                    }

                    var recorder = signal.getRecorder();
                    if (recorder != null)
                    {
                        double seconds = AxisX.Range / 1000;

                        if (seconds >= 10)
                        {
                            Tiggerdate = DateTime.Now.AddSeconds(-seconds);
                        }

                        DateTime dateStart = Tiggerdate.AddMilliseconds(AxisX.Offset);
                        DateTime dateEnd = DateTime.Now.AddMilliseconds(AxisX.Offset);

                        var points = recorder
                            .GetItemsSnapshot()
                            .Where(p => p.Timestamp >= dateStart && p.Timestamp <= dateEnd)
                            .OrderBy(p => p.Timestamp)
                            .ToList();

                        List<PointF> line = new List<PointF>();

                        foreach (var item in points)
                        {
                            TimeSpan timeSpan = item.Timestamp - dateStart;
                            double x = Draw.scale(timeSpan.TotalMilliseconds, 0, seconds * 1000, Bounds.X, Bounds.X + Bounds.W);
                            Drawing.Axis yAxis = Axes[signal.Name];
                            double y = Draw.scale(item.Value, yAxis.Min, yAxis.Max, Bounds.Y + Bounds.H, Bounds.Y);

                            if (!double.IsNaN(x) && !double.IsNaN(y))
                                line.Add(new PointF((float)(x * Draw.mmToPx), (float)(y * Draw.mmToPx)));
                        }

                        Draw.Lines(pen, line.ToArray(), false);
                    }
                }



                //Legend

                if (SignalLegendVisible)
                {
                    double maxW = 0;
                    for (int s = 0; s < Signals.Values.Count; s++)
                    {
                        Signal signal = Signals.Values.ElementAt(s);
                        if (signal.Text != null)
                        {
                            string text = signal.Text?.PadLeft(20) + " " + signal.Value.ToString(6, 1).PadLeft(7) + " " + signal.Unit?.PadRight(5);
                            SizeF size = Draw.g.MeasureString(text, Draw.fontTerminalFixed);
                            maxW = Math.Max(maxW, size.Width / Draw.mmToPx);
                        }

                    }

                    double yPos = 0;
                    for (int s = 0; s < Signals.Values.Count; s++)
                    {
                        Signal signal = Signals.Values.ElementAt(s);
                        //  Color color = Misc.ParseColor(signal.Color);// Qb.ScriptingEngine.InterpretScript(col)?.ToString());
                        if (signal.Text != null)
                        {
                            string text = signal.Text?.PadLeft(20) + " " + signal.Value.ToString(6, 1).PadLeft(7) + " " + signal.Unit?.PadRight(5);

                            SizeF size = Draw.g.MeasureString(text, Draw.fontTerminalFixed);

                            double w = maxW;// size.Width / Draw.mmToPx;
                            double h = size.Height / Draw.mmToPx;

                            Draw.FillRectangle(Draw.WhiteBrush, Bounds.X + 29.5, Bounds.Y + 2.5 + yPos, w + 1, h + 0);
                            Draw.Text(text, Bounds.X + 30, Bounds.Y + 3 + yPos, Bounds.W, Draw.fontTerminalFixed, signal.Color, ContentAlignment.MiddleLeft);// //@SCAN 2023-11-20 .MiddleLeft);
                                                                                                                                                             //   Draw.Text(text, Bounds.X + 5, Bounds.Y + 3 + yPos, 30, Draw.fontTerminalFixed, signal.Color, ContentAlignment.MiddleLeft);
                        }
                        yPos += 3.5;
                    }
                }
            }
            catch { }
            finally
            {
                if (USE_CLIP)
                    Draw.ClearClip();
            }
        }
      
        public Drawing.Axis AxisX = new Drawing.Axis();
        //@STFU 2024-11-12 new default Axes
        public Drawing.Axis AxisY1 = new Drawing.Axis();
        public Drawing.Axis AxisY2 = new Drawing.Axis();
        public Drawing.Axis AxisY3 = new Drawing.Axis();
        public Drawing.Axis AxisY4 = new Drawing.Axis();
        //

        public DateTime Tiggerdate = DateTime.MinValue;


        public static void Grid(Chart c, Signal signal, int xOffsetAxis, bool xAxis)
        {
            // if (Axes[signal.Name])

            Drawing.Axis axis = c.Axes[signal.Name];

            if(axis == null)
                axis = c.AxisY1 as Drawing.Axis;

            // if (c.AxisY1.MinorTicks == c.AxisY1.MajorTicks)

            if (axis.MinorTicks == axis.MajorTicks)
                return;


            if (xAxis)
            {
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

                        double textX = x;
                        if (USE_CLIP)
                        {
                            if (textX > (c.Bounds.X + c.Bounds.W - 5))
                            {
                                textX -= 6.7;
                                Draw.Line(Draw.penMajorTicks, c.Bounds.X + c.Bounds.W - 0.1f, c.Bounds.Y + c.Bounds.H - 0.2, c.Bounds.X + c.Bounds.W - 0.1f, c.Bounds.Y + c.Bounds.H - 4.0);
                            }
                            if (textX < (c.Bounds.X))
                            {
                                textX += 1.5;
                            }
                        }
                        if ((i % 2) == 0)
                        {
                            double s = seconds - (seconds * i) / ticks;
                            DateTime date = DateTime.Now.AddSeconds(-s);


                            if (i > 0)
                            {
                                if (i == ticks)
                                {
                                    DateTime xD = date.AddMilliseconds(c.AxisX.Offset);
                                    Draw.Text(xD.ToString("HH:mm:ss"), textX, c.Bounds.Y + c.Bounds.H - 3.5f, 0, Draw.fontTerminalFixed, System.Drawing.Color.DimGray, System.Drawing.ContentAlignment.MiddleCenter);
                                }

                                else
                                {
                                    if (seconds <= 600)
                                    {
                                        Draw.Text("-" + (s) + "s", textX, c.Bounds.Y + c.Bounds.H - 3.5f, 0, Draw.fontTerminalFixed, System.Drawing.Color.DimGray, System.Drawing.ContentAlignment.MiddleCenter);
                                    }
                                    else
                                    {
                                        Draw.Text("-" + (Math.Round(s / 60, 2)) + "min", textX, c.Bounds.Y + c.Bounds.H - 3.5f, 0, Draw.fontTerminalFixed, System.Drawing.Color.DimGray, System.Drawing.ContentAlignment.MiddleCenter);
                                    }
                                }
                            }
                        }
                    }
                }
            }


            for (double v = axis.Min; v <= axis.Max; v += axis.MinorTicks)
            {
                double y = Draw.scale(v, axis.Min, axis.Max, c.Bounds.Y + c.Bounds.H, c.Bounds.Y);
                Draw.Line(Draw.penMinorTicksLight, c.Bounds.X, y, c.Bounds.X + c.Bounds.W, y);
            }

            for (double v = axis.Min; v <= axis.Max; v += axis.MajorTicks)
            {
                double y = Draw.scale(v, axis.Min, axis.Max, c.Bounds.Y + c.Bounds.H, c.Bounds.Y);
                Draw.Line(Draw.penMajorTicksLight, c.Bounds.X, y, c.Bounds.X + c.Bounds.W, y);

                Draw.FillRectangle(Draw.WhiteBrush, c.Bounds.X + 0.1f + xOffsetAxis, y - 2f, 8, 4);
                double textY = y - 1.4f;
                if (USE_CLIP)
                {
                    if (textY > (c.Bounds.Y + c.Bounds.H - 5))
                    {
                        textY -= 1.5;
                        Draw.Line(Draw.penMajorTicks, c.Bounds.X + 0.1f, c.Bounds.Y + c.Bounds.H - 0.2, c.Bounds.X + 2.5f, c.Bounds.Y + c.Bounds.H - 0.2);
                    }
                    if (textY < (c.Bounds.Y))
                    {
                        Draw.Line(Draw.penMajorTicks, c.Bounds.X + 0.1f, c.Bounds.Y + 0.2, c.Bounds.X + 2.5f, c.Bounds.Y + 0.2);
                        textY += 1.5;
                    }
                }
                //@STFU 2024-11-12 axis.GetFormat()
                Draw.Text(v.ToString(axis.GetFormat()), c.Bounds.X + 0.1f + xOffsetAxis, textY, 0, Draw.fontTerminalFixed, System.Drawing.Color.DimGray, System.Drawing.ContentAlignment.MiddleLeft);
            }
        }
    }
}
