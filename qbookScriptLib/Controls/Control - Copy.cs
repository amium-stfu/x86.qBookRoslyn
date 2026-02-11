
using System;
using System.Collections;
using System.Collections.Generic;
using static QB.Automation.Machine;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace QB.Controls
{
    public class View : Control
    {
        protected Timer _timer;
        public View(string name, double x = 0, double y = 0, double w = 30, double h = 30) : base(name, x: x, y: y, w: w, h: h)
        {
            _timer = new Timer(name, 1 * 1000);
        }

        public virtual void Run()
        {
            _timer.Run();
            _timer.OnElapsed -= _Elapsed;
            _timer.OnElapsed += _Elapsed;
        }

        public virtual void Abort()
        {
            _timer.Destroy();
            _timer.OnElapsed -= _Elapsed;
        }

        public delegate void OnElapsedDelegate(Control c);
        public OnElapsedDelegate OnElapsed;
        public void _Elapsed(Timer s)
        {
            if (OnElapsed != null)
                OnElapsed(this);
        }
    

    }


    public class Control : Item
    {
        //public static string GetNameOfCallingClass()
        //{
        //    string fullName;
        //    Type declaringType;
        //    int skipFrames = 4;
        //    do
        //    {
        //        MethodBase method = new StackFrame(skipFrames, false).GetMethod();
        //        declaringType = method.DeclaringType;
        //        if (declaringType == null)
        //        {
        //            return method.Name;
        //        }
        //        skipFrames++;
        //        fullName = declaringType.FullName;
        //    }
        //    while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

        //    return fullName;
        //}

        //Regex getNewObjectAssignee = new Regex(@"\b(?<name>[_a-zA-Z\.][_0-9a-zA-Z\.]+)\s+=\s+new\s+.*");

      public string Text { get; set; }
        public string Color = null;
        public bool Enabled = true;
        public bool Hover = false;
        public bool Visible = true;
        public bool Clickable = true;
        public delegate void OnClickDelegate(Control s);
        public OnClickDelegate OnClick;

        public string BackColor = null;
        public string ForeColor = null;
        public string Border = null; //e.g.: "brown:2.5"

        public int addIndex = 0;

        public Control(string name, double x = 0, double y = 0, double w = 30, double h = 15) :  base(name, null) 
    //    {this(name, name, id)
        {
            Bounds = new Rectangle(x, y, w, h);
            Text = "#" + name;
  //      }
  //      public Control(string name, string text, string id=null) : base(name, id) 
    //    {
        //    Text = text;    
            lock (Root.ControlDict)
            {
                Root.ControlDict.Add(Guid.NewGuid().ToString(), this);
            }

            ////try to set .directory
            //try
            //{
            //    var callingClassName = GetNameOfCallingClass();
            //    if (callingClassName.StartsWith("css_root+_page_"))
            //    {
            //        this.directory = callingClassName.Substring("css_root+_page_".Length);
            //    }
            //} 
            //catch
            //{
            //    //this.directory = null;
            //}

            ////try to set .name
            //try
            //{
            //    var srcFile = new StackFrame(3, true).GetFileName();
            //    var srcLineNr = new StackFrame(3, true).GetFileLineNumber();
            //    string line = System.IO.File.ReadLines(srcFile).Skip(srcLineNr - 1).Take(1).First();
            //    Match m = getNewObjectAssignee.Match(line);
            //    if (m.Success)
            //    {
            //        this.name = m.Groups["name"].Value;
            //        //this.text = m.Groups["name"].Value;
            //    }
            //}
            //catch
            //{
            //    //this.name = null;
            //}
        }

        virtual internal void Stop()
        {
            this.OnClick = null;
        }

        //public Control(string text, string id, string createdBy) :base(text, id, createdBy)
        //{
        //    lock (Root.ControlDict)
        //    {
        //        while (Root.ControlDict.ContainsKey(id))
        //            id += "_";
        //        Root.ControlDict.Add(id, this);
        //    }
        //}

        Rectangle _bounds = new QB.Rectangle();
        public Rectangle Bounds
        {
            get => _bounds;
            set
            {
                _bounds = new Rectangle(value.X + 10, value.Y + 20, value.W, value.H);
            }
        }

        internal virtual void Render(Control parent)
        {
            try
            {
<<<<<<< .merge_file_a43764
                if (Name != null && Draw.DesignMode)
                {
                    Draw.Text(Name, Bounds.X, Bounds.Y, 1, Draw.fontTerminalFixed, System.Drawing.Color.DarkGray);
                }

                if (Border != null)
                {
                    System.Drawing.Pen pen = System.Drawing.Pens.DarkGray;
                    string[] splits = Border.Split(':');
                    double width = 1.0;
                    if (splits.Length > 1)
                        double.TryParse(splits[1], out width);
                    if (splits.Length > 0)
                        pen = Draw.GetPen(splits[0], width);
                    Draw.Rectangle(pen, Bounds.X, Bounds.Y, Bounds.W, Bounds.H);
                }
            } 
            catch (Exception ex)
=======
                Draw.Text(Name, Bounds.X, Bounds.Y, 1, Draw.fontTerminalFixed, System.Drawing.Color.LightGray);
            }
            if (Border != null)
>>>>>>> .merge_file_a42128
            {

            }
        }

        internal void Drag(System.Drawing.PointF point)
        {
            /*
            if (this is  == ComponentType.Slider)
            {
                double set = Draw.scale(point.Y, bounds.Y + bounds.H, bounds.Y, axisy1.min, axisy1.max);
                for (double v = axisy1.min; v <= axisy1.max; v += axisy1.minorticks)
                    if (v > set)
                    {
                        if ((v - set) > (set - (v - axisy1.minorticks)))
                            set = v - axisy1.minorticks;
                        else
                            set = v;
                        break;
                    }Ga


                Qb.Set(null, setName, set);
            }
            */
            if (this is RadialGauge)
            {
                RadialGauge gauge = this as RadialGauge;
                double x = (point.X - (Bounds.X + Bounds.W)) / Bounds.W;
                double y = -(point.Y - (Bounds.Y + Bounds.H)) / Bounds.H;
                double _angle = (float)Math.Atan2(y, x);
                double angle = _angle * 180.0f / Math.PI;
                double set = Draw.scale(angle, 180, 90, gauge.Axis.Min, gauge.Axis.Max);



                for (double v = gauge.Axis.Min; v <= gauge.Axis.Max; v += gauge.Axis.MinorTicks)
                    if (v > set)
                    {
                        if ((v - set) > (set - (v - gauge.Axis.MinorTicks)))
                            set = v - gauge.Axis.MinorTicks;
                        else
                            set = v;
                        break;
                    }

                if (gauge.Signal is Module)
                    (gauge.Signal as Module).Set.Value = set;

            }
        }
    }
}
