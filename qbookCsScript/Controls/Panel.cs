using AForge;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace QB.Controls
{
    public class Panel : Control
    {
        public Panel(string name, double x = 0, double y = 0, double w = 90, double h = 30) : base(name, x: x, y: y, w: w, h: h)
        {
            Clickable = false;

    
        }

        public string Header = null;

        // public Dictionary<string Control> Controls = new Dictionary<string, Control>();

        public void Add(Control control)
        {
            //remove from root-dict if already exists (=overwrite/update)
            string id = null;
            if (Controls.Contains(control.Name))
            {
                id = Controls[control.Name].Id;
                Root.RemoveControl(id);
            }
            else
            {

            }

            //if (Controls[control.Name].Id == null)
            //{
            //    var guid = Guid.NewGuid().ToString();
            //    this.Id = guid;
            //    Root.ControlDict.Add(guid, this);
            //}

            Controls[control.Name] = control;
            control.ParentPanel = this;

            if (id == null)
            {
                //if (!Root.ControlDict.ContainsKey(id))
                {
                    if (control.Id != null)
                        Root.AddControl(control.Id, control);
                }
            }
        }

        public class CControl
        {
            Dictionary<string, Control> Dict = new Dictionary<string, Control>();

            public void Clear()
            {
                lock (Dict)
                {
                    foreach (var id in Dict.Values.Select(x => x.Id).ToList())
                        Root.RemoveControl(id);
                    Dict.Clear();
                }
            }
            public void Hide()
            {
                lock (Dict)
                {
                    foreach (Control control in Dict.Values)
                        control.Visible = false;
                }
            }
            public void Show()
            {
                lock (Dict)
                {
                    foreach (Control control in Dict.Values)
                        control.Visible = true;
                }
            }

            public void Remove(string name)
            {
                lock (Dict)
                {
                    if (Dict.ContainsKey(name))
                    {
                        string id = Dict[name].Id;
                        if (!string.IsNullOrEmpty(id))
                            Root.RemoveControl(id);
                        Dict.Remove(name);
                    }
                }
            }


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

        public CControl Controls = new CControl();

        public void Hide()
        {
            Visible = false;
            Controls.Hide();
        }
        public void Show()
        {
            Visible = true;
            Controls.Show();
        }


        //        public ConcurrentDictionary<string, Control> Controls = new ConcurrentDictionary<string, Control>();
        /*
        public Control Control(string key)
        {
            lock (Controls)
            {
                if (!Controls.ContainsKey(key))
                    Controls.Add(key, new Control(key));
                return Controls[key];
            }
        }
        public Control Control(string key, Control control)
        {
            lock (Controls)
            {
                if (!Controls.ContainsKey(key))
                    Controls.TryAdd(key, null);
                Controls[key] = control;
                return Controls[key];
            }
        }
        */

        Regex PosXYRegex = new Regex(@"\$pos=(x:(?<x>[\d\.\+]*);?)?(y:(?<y>[\d\.\+]*))?");
        internal override void Render(Control parent)
        {
            //System.Drawing.Pen pen = System.Drawing.Pens.DarkGray;
            //Draw.Rectangle(pen, Bounds.X, Bounds.Y, Bounds.W, Bounds.H);

            System.Drawing.Pen pen = Draw.GetPen2(System.Drawing.Color.DarkGray, 0.1);
            //  System.Drawing.SolidBrush brush = Draw.GetBrush(System.Drawing.Color.DarkGray, 0.2);


            if (Header != null)
            {
                List<PointF> line = new List<PointF>();
                line.Add(new PointF((float)Bounds.X, (float)Bounds.Y-1));
                line.Add(new PointF((float)Bounds.X+ (float)Bounds.W+1, (float)Bounds.Y-1));
                line.Add(new PointF((float)Bounds.X + (float)Bounds.W+1, (float)Bounds.Y + (float)Bounds.H));
                Draw.Lines(Draw.DesignPen, line.ToArray(), true);
                Draw.Text(Header, Bounds.X, Bounds.Y-6, Bounds.W, Draw.fontFootnote, System.Drawing.Color.Gray, System.Drawing.ContentAlignment.TopLeft);
            }
            //    Draw.FillRectangle(Draw.BgDesignBrush, Bounds.X, Bounds.Y, Bounds.W, Bounds.H);
            //    Draw.Rectangle(Draw.BgDesignPen, Bounds.X, Bounds.Y, Bounds.W, Bounds.H);
        

            if (!Transparent)
            {
                Draw.FillRectangle(Draw.BgDesignBrush, Bounds.X, Bounds.Y, Bounds.W, Bounds.H);
                Draw.Rectangle(Draw.BgDesignPen, Bounds.X, Bounds.Y, Bounds.W, Bounds.H);
            }

            if ((Text != null) && (!Text.StartsWith("#")))
            {
                Draw.Text(Text, Bounds.X, Bounds.Y, Bounds.W, Draw.fontText, System.Drawing.Color.Black, System.Drawing.ContentAlignment.TopLeft);
            }

            double x = Bounds.X;
            double y = Bounds.Y;

            foreach (Control control in Controls.Values.OrderBy(control => control.addIndex))
            {
                if (control is Control)
                {
                    if (control.Text != null && control.Text.StartsWith("$pos="))
                    {
                        var m = PosXYRegex.Match(control.Text);
                        if (m.Success)
                        {
                            string xs = m.Groups["x"].Value;
                            string ys = m.Groups["y"].Value;
                            if (xs == "+")
                                x += control.Bounds.W;
                            else
                            {
                                if (double.TryParse(xs, NumberStyles.Any, CultureInfo.InvariantCulture, out double dx))
                                    x = Bounds.X + dx;
                            }

                            if (ys == "+")
                                y += control.Bounds.H;
                            else
                            {
                                if (double.TryParse(ys, NumberStyles.Any, CultureInfo.InvariantCulture, out double dy))
                                    y = Bounds.Y + dy;
                            }
                        }
                        control.Visible = false;
                        continue;
                    }
                }

                if (x + control.Bounds.W > Bounds.X + Bounds.W)
                {
                    x = Bounds.X;
                    y += control.Bounds.H;
                }
                control.Bounds.X = x;
                control.Bounds.Y = y;
                x += control.Bounds.W;
            }

            base.Render(parent);
        }
    }
}
