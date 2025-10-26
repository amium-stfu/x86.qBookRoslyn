
using Google.Protobuf.WellKnownTypes;
using QB.Amium.Controls;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Documents;

namespace QB.Controls
{
    public class View : Control
    {
        // public Timer Timer;
        public View(string name, double x = 0, double y = 0, double w = 30, double h = 30) : base(name, x: x, y: y, w: w, h: h)
        {
            //   Timer = new Timer(name, 1 * 1000);           
        }

        public class CButton
        {
            public LuiView luiView;
            Button Dummy;// = new Button("dummy");

            public Button this[string name]
            {
                get
                {                    
                    if ((luiView.NavPanel != null) && luiView.NavPanel.Controls.Contains(name))
                        return (Button)luiView.NavPanel.Controls[name];

                    if ((luiView.MediaPanel != null) && luiView.MediaPanel.Controls.Contains(name))
                        return (Button)luiView.MediaPanel.Controls[name];                    

                    if (luiView.StatusPanel.Controls.Contains(name))
                        return (Button)luiView.StatusPanel.Controls[name];

                    if (luiView.StatusPanel.Controls.Contains(name))
                        return (Button)luiView.StatusPanel.Controls[name];

                    if (luiView.DataPanel.Controls.Contains(name))
                        return (Button)luiView.DataPanel.Controls[name];

                    if (luiView.ControlPanel.Controls.Contains(name))
                        return (Button)luiView.ControlPanel.Controls[name];

                    if (luiView.Chart.Control.Contains(name))
                        return (Button)luiView.Chart.Control[name];

                    if (luiView.LogPanel.Controls.Contains(name))
                        return (Button)luiView.LogPanel.Controls[name];

                    if (Dummy == null)
                        Dummy = new Button("$dummy");
                    return Dummy;
                }
            }
        }

        public CButton Buttons = new CButton();



        public void Add(Item item, Panel panel = null)
        {
            if (panel != null)
            {
                

                if ((panel is Log) && (!(item is QB.Controls.Control)))
                {
                    (panel as Log).Items[item.Name] = item;
                }
                else if (item is QB.Controls.Control)
                {
                    QB.Controls.Control control = (QB.Controls.Control)item;
                    control.ParentPanel = panel;
                    control.addIndex = panel.Controls.Values.Count;
                    if (panel.Controls.Contains(control.Name))  //remove from root-dict if already exists (=overwrite/update)
                        Root.RemoveControl(panel.Controls[control.Name].Id);
                    panel.Controls[control.Name] = control;
                }
            }
        }

        public void Add(QB.Controls.Control control, Chart panel = null)
        {
            if (panel != null)
            {
                
                control.addIndex = panel.Control.Values.Count;
                if (panel.Control.Contains(control.Name))  //remove from root-dict if already exists (=overwrite/update)
                    Root.RemoveControl(panel.Control[control.Name].Id);
                panel.Control[control.Name] = control;
            }
        }

        public void Add(Signal signal, Drawing.Axis axis = null, Chart panel = null)
        {
            if (panel != null)
            {
                // Kein addIndex mehr nötig!
                panel.Signals[signal.Name] = signal;
                panel.Log.Add(signal);

                if (axis != null)
                    panel.Axes[signal.Name] = axis;
                else
                    panel.Axes[signal.Name] = panel.AxisY1;
            }
        }




        /*
        public void Remove(Control control, Panel panel = null)
        {
            if (panel != null)
                panel.Controls.TryRemove(control.Name, out control);
        }

        public void Remove(Control control, Chart panel = null)
        {
            if (panel != null)
                panel.Controls.TryRemove(control.Name, out control);
        }

        public void Remove(Signal signal, Chart panel = null)
        {
            if (panel != null)
                panel.Signals.TryRemove(signal.Name, out signal);
        }
        */
        /*
        public Button Button(int id, Panel panel = null)
        {
            return Button(id.ToString("00"), panel                );
        }
            public Button Button(string name, Panel panel = null)
        {
            if (panel != null)
            {
                
                return (Button)panel.Controls[name];
            }

            if (StatusPanel.Controls.ContainsKey(name))
                return (Button)StatusPanel.Controls[name];

            if (DataPanel.Controls.ContainsKey(name))
                return (Button)DataPanel.Controls[name];

            if (ControlPanel.Controls.ContainsKey(name))
                return (Button)ControlPanel.Controls[name];            

            if (Chart.Controls.ContainsKey(name))
                return (Button)Chart.Controls[name];

            if (LogPanel.Controls.ContainsKey(name))
                return (Button)LogPanel.Controls[name];          
           

            return null;
        }
        */










        /*
        public virtual void Run()
        {
            Timer.Run();
            Timer.OnElapsed -= _Elapsed;
            Timer.OnElapsed += _Elapsed;
        }

        public virtual void Abort()
        {
            Timer.Destroy();
            Timer.OnElapsed -= _Elapsed;
        }

        public delegate void OnElapsedDelegate(Control c);
        public OnElapsedDelegate OnElapsed;

        private void _Elapsed(Timer s)
        {
            if (OnElapsed != null)
                OnElapsed(this);
        }
    */

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


        private string _Text;
        public string Text
        {
            get
            {
                if (this is Box)
                    return (this as Box).Text.ToString();
                else
                    return _Text;
            }
            set
            { 
                if (this is Box)
                    (this as Box).Text = value;
                else
                    _Text = value;
            }
        }
        public string Color = null;
        public bool Enabled = true;
        public bool Hover = false;
        public bool Visible = true;
        public bool Clickable = true;
        public bool Transparent = false;
        private int _ZOrder = 0;
        public int ZOrder
        {
            get { return _ZOrder; }
            set 
            { 
                int delta = value - _ZOrder;
                _ZOrder = value; 
                this.IterateOverChildren(c => c.ZOrder = value);
                //this.IterateOverChildren(c => c.ZOrder = c.ZOrder + delta);
            }
        }

        public object Tag = null;

        public Image BackgroundImage = null;
        //public string BackgroundImageName = null;
        string lastBackgroundImageFileName = null;

        public Icon LeftIcon = null;
        public Icon RightIcon = null;

        internal System.Collections.Generic.List<Control> Children = new System.Collections.Generic.List<Control> ();
        public void IterateOverChildren(Action<Control> action)
        {
            foreach (Control child in Children)
            {
                action(child);
                child.IterateOverChildren(action);  // Recursive call
            }
        }
        private Control _Parent = null;
        internal Control Parent
        {
            get
            {
                return _Parent;
            }
            set
            {
                _Parent = value;
                if (!_Parent.Children.Contains(this))
                {
                    _Parent.Children.Add(this);
                }
            }
        }
        
        public Control this[string name]
        {
            get => Children.FirstOrDefault(box => box.Name == name) 
                ?? new Control(Guid.NewGuid().ToString()); //TODO: is this a good idea or is it better to return null and let the script fail?!
        }


        public bool IsVisible() //recursively
        {
            // Check the visibility of the current box
            if (!this.Visible)
            {
                return false;
            }

            // Recursively check the visibility of the parent box if it exists
            if (Parent != null)
            {
                return Parent.IsVisible();
            }

            // If no parent or all parents are visible, return true
            return true;
        }



        public PointF ClickPosition;
        public bool LeftIconClicked()
        {
            if (ClickPosition.X < (Bounds.X + Bounds.H))
                return true;
            return false;

        }
        public bool RightIconClicked()
        {
            if (ClickPosition.X > (Bounds.X +  Bounds.W - Bounds.H))
                return true;
            return false;

        }
        

        //public delegate void OnClickDelegate(Control s);
        //public OnClickDelegate OnClick;
        //HALE:event-new 2022-12-14
        public delegate void ClickEventHandler(Control s);
        public event ClickEventHandler OnClick;
        internal virtual void RaiseOnClick(PointF point)
        {
            try
            {
                ClickPosition = point;
                if (OnClick != null)
                {
                    OnClick(this);
                }
            }
            catch (Exception ex)
            {
                QB.Logger.Error("#EX in OnClick: " + ex.Message);
            }
        }

        public delegate void WheelEventHandler(Control s, int delta);
        public event WheelEventHandler OnWheel;
        internal virtual void RaiseOnWheel(int delta)
        {
            try
            {
                if (OnWheel != null)
                {
                    OnWheel(this, delta/120);
                }
            }
            catch (Exception ex)
            {
                QB.Logger.Error("#EX in OnWheel: " + ex.Message);
            }
        }

        public delegate void MoveEventHandler(Control s, double x, double y);
        public event MoveEventHandler OnMove;
        internal virtual void RaiseOnMove(double x, double y)
        {
            try
            {
                if (OnMove != null)
                {
                    OnMove(this, x, y);
                }
            }
            catch (Exception ex)
            {
                QB.Logger.Error("#EX in OnMove: " + ex.Message);
            }
        }


        public void Hide()
        {
            Visible = false;
        }
        public void Show()
        {
            Visible = true;
        }


        /*
        public delegate void ClickEventHandler(object sender);
        private ClickEventHandler _ClickEventHandler;
        public event ClickEventHandler OnClick
        {
            add { _ClickEventHandler += value; }
            remove { _ClickEventHandler -= value; }
        }
        internal void RaiseOnClick()
        {
            if (_ClickEventHandler != null)
            {
                _ClickEventHandler.Invoke(this);
            }
        }
        */





        //public delegate void Click2EventHandler(object sender, EventArgs e);
        //public event Click2EventHandler Click2;
        //internal void OnClick2()
        //{
        //    if (Click2 != null)
        //    {
        //        EventArgs ea = new EventArgs() { };
        //        Click2(this, ea);
        //    }
        //}



        public string BackColor_ = null;

        public object BackColor
        {
            get { return BackColor_; }
            set
            {
                if (value == null)
                    return;
                if (value is System.Drawing.Color)
                {
                    // System.Drawing.Color color = (System.Drawing.Color) value;
                    BackColor_ = ColorTranslator.ToHtml((System.Drawing.Color)value);
                }
                else
                    BackColor_ = value?.ToString();
            }
        }

        public string ForeColor = null;
        public string Border = null; //e.g.: "brown:2.5"

        public int addIndex = 0;

        public Panel ParentPanel= null;

        public Control(string name, double x = 0, double y = 0, double w = 30, double h = 15) : base(name, null)
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
                var guid = Guid.NewGuid().ToString();
                this.Id = guid;
                Root.ControlDict.Add(guid, this);
            }

            ////try to set .directory
            //try
            //{
            //    var callingClassName = GetNameOfCallingClass();
            //    if (callingClassName.StartsWith("css_root+@class_"))
            //    {
            //        this.directory = callingClassName.Substring("css_root+@class_".Length);
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

        internal virtual void Stop()
        {
            this.OnClick = null;
            //HALE:event this._ClickEventHandler = null;
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
                _bounds = new Rectangle(value.X + 10, value.Y + 20, value.W, value.H) { OwnerControl = this };
            }
        }

        private System.Drawing.Image img = null;
        // private string imgData = null;

        public Pen BorderPen = System.Drawing.Pens.DarkGray;


        internal virtual void Render(Control parent)
        {
            /*
            if (BackgroundImage != null)
                img = BackgroundImage;

            if ((img == null) && (BackgroundImageName != null) && !BackgroundImageName.ToUpper().Contains(".JPG"))
            {
                if (Draw.Resources.ContainsKey(BackgroundImageName))
                    if (Draw.Resources[BackgroundImageName] is Image)
                        img = Draw.Resources[BackgroundImageName] as Image;
            }

            if (BackgroundImageName != lastBackgroundImageFileName)
            {
                lastBackgroundImageFileName = BackgroundImageName;
                img = null;
                //if (img == null)
                {
                    // imgData = Data;
                    try
                    {
                        if (BackgroundImageName.Contains("."))
                            img = new System.Drawing.Bitmap(Root.ActiveQbook.DataDirectory + "\\" + BackgroundImageName,);// Draw.Base64ToImage(imgData);
                                                                                                                         //else
                                                                                                                         //  img
                                                                                                                         //img = Draw.Base64ToImage(BackgroundImageFileName);
                    }
                    catch { img = null; }
                }

            }
            */

            //TEST
            //Draw.Rectangle(Pens.Red, Bounds.X, Bounds.Y, Bounds.W - 0.5f, Bounds.H - 0.5f);

            if (BackgroundImage != null)
                Draw.Image(BackgroundImage, Bounds.X, Bounds.Y, Bounds.X + Bounds.W, Bounds.Y + Bounds.H);

            if (!Transparent && (Border != null))
            {
                System.Drawing.Pen pen = BorderPen;
                string[] splits = Border.Split(':');
                double width = 1.0;
                if (splits.Length > 1)
                    double.TryParse(splits[1], out width);
                if (splits.Length > 0)
                    pen = Draw.GetPen(splits[0], width);
                Draw.Rectangle(pen, Bounds.X, Bounds.Y, Bounds.W-0.5f, Bounds.H-0.5f);
            }

            

                if (Name != null && Draw.DesignMode)
            {
                Draw.Text(Name, Bounds.X, Bounds.Y, Bounds.W, Draw.fontTerminalFixed, System.Drawing.Color.Magenta, System.Drawing.ContentAlignment.TopLeft);
            }
        }

        public virtual void Drag(System.Drawing.PointF point)
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
            /*
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
            */
        }
    }
}
