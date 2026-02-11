
using QB.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Serialization;

namespace qbook
{
    [
       //XmlInclude(typeof(x_oGrid)),
       XmlInclude(typeof(oImage)),
       XmlInclude(typeof(oLayer)),
       XmlInclude(typeof(oModule)),
       XmlInclude(typeof(oPage)),
       XmlInclude(typeof(oPart)),
       XmlInclude(typeof(oScratch)),
       XmlInclude(typeof(oTag)),
       XmlInclude(typeof(oText)),
       XmlInclude(typeof(oHtml)),
   ]

    [Serializable]
    public class oControl : oItem
    {
        public enum Alignment { TL, T, TR, L, C, R, BL, B, BR };

        public oControl()
        {
        }
        public oControl(string name, string text) : base(name, text)
        {
            Visible = true;
        }

        public Bounds Bounds = new Bounds(0, 0, 0, 0);
        public bool Visible = true;


        public virtual void Render()
        {
            //MIGRATE-new
            var widgets = QB.Root.GetControlsByDirectory(this.FullName).OfType<QB.Controls.Control>().OrderBy(c=>c.ZOrder);
            int count = 0;
            foreach (var widget in widgets)
            {
                if (widget.Visible)
                {
                    if (widget is QB.Controls.Box)
                    {
                        //only top-level Box-es
                        if ((widget as QB.Controls.Box).Parent == null)
                            widget.Render(null);
                    }
                    else
                    {
                        widget.Render(null);
                    }
                }
                count++;
            }
        }

        public virtual void Frame(bool showHeader, bool showResize)
        {
            if (qbook.Core.ThisBook.DesignMode || (qbook.Core.ThisBook.TagMode && (GetType() == typeof(oTag))))
            {
                if (showHeader)
                    Draw.Text("[" + Marker + "] " + this.GetType().ToString().Replace("Main.o", "") + " " + Name, Bounds.X, Bounds.Y + Bounds.H - 5, 0, Draw.fontFootnoteFixed, Selected ? Color.Orange : Color.LightSlateGray);
                Draw.Rectangle(Draw.GetPen2(Selected ? Draw.DesignBrush.Color : Draw.DesignBrush.Color, 0.5f), Bounds.X, Bounds.Y, Bounds.W, Bounds.H);
                //      Draw.Rectangle(Selected ? Draw.DesignBrush : Draw.DesignBrush, Bounds.X, Bounds.Y, 5, 5, 0.8f);

                //   Draw.Rectangle(Draw.WhiteBrush, Bounds.X + Bounds.W - 5, Bounds.Y + Bounds.H - 20, 5, 5, 0);
                Draw.FillRectangle(Draw.WhiteBrush, Bounds.X + Bounds.W - 5, Bounds.Y + Bounds.H - 15, 5, 5);
                Draw.FillRectangle(Draw.WhiteBrush, Bounds.X + Bounds.W - 5, Bounds.Y + Bounds.H - 10, 5, 5);
                //  Draw.Rectangle(Selected ? Draw.DesignBrush : Draw.DesignBrush, Bounds.X + Bounds.W - 5, Bounds.Y + Bounds.H - 20, 5, 5, 0.3f);
                Draw.Rectangle(Draw.GetPen2(Selected ? Draw.DesignBrush.Color : Draw.DesignBrush.Color, 0.3f), Bounds.X + Bounds.W - 5, Bounds.Y + Bounds.H - 15, 5, 5);
                Draw.Rectangle(Draw.GetPen2(Selected ? Draw.DesignBrush.Color : Draw.DesignBrush.Color, 0.3f), Bounds.X + Bounds.W - 5, Bounds.Y + Bounds.H - 10, 5, 5);
                double x, y;

                if (showResize)
                {
                    Draw.FillRectangle(Draw.WhiteBrush, Bounds.X + Bounds.W - 5, Bounds.Y + Bounds.H - 5, 5, 5);
                    Draw.Rectangle(Draw.GetPen2(Selected ? Draw.DesignBrush.Color : Draw.DesignBrush.Color, 0.3f), Bounds.X + Bounds.W - 5, Bounds.Y + Bounds.H - 5, 5, 5);
                    x = Bounds.X + Bounds.W - 2.5f;
                    y = Bounds.Y + Bounds.H - 2.5f;
                    Draw.Arrow(Pens.LightSlateGray, x, y, 2, -45);
                    Draw.Arrow(Pens.LightSlateGray, x, y, 2, 135);
                }

                x = Bounds.X + Bounds.W - 2.5f;
                y = Bounds.Y + Bounds.H - 7.5f;
                Draw.Arrow(Pens.LightSlateGray, x, y, 1.5f, 0);
                Draw.Arrow(Pens.LightSlateGray, x, y, 1.5f, 90);
                Draw.Arrow(Pens.LightSlateGray, x, y, 1.5f, 180);
                Draw.Arrow(Pens.LightSlateGray, x, y, 1.5f, 270);

                x = Bounds.X + Bounds.W - 2.5f;
                y = Bounds.Y + Bounds.H - 12.5f;
                Draw.Circle(Color.WhiteSmoke, x, y, 2, 0);
                Draw.Circle(Color.LightSlateGray, x, y, 2.5f, 0.7f);
            }
        }

        public override QB.Controls.Control GetControlUnderCursor(PointF point, bool clickableOnly = false)
        {
            var controls = QB.Root.GetControlsByDirectory(this.FullName).OfType<QB.Controls.Control>().OrderBy(f => f.Bounds.Size).OrderByDescending(c => c.ZOrder);

            foreach (var control in controls)
            {
                control.Hover = control.Bounds.Contains(point.X, point.Y);
                //  if (control.Hover) //&& (c.type == ComponentType.Button))
                //    return control;
            }

            foreach (var control in controls)
            {
                //      control.Hover = control.Bounds.Contains(point.X, point.Y);
                if (clickableOnly)
                {
                    if (control.Hover && control.Clickable)
                        return control;
                }
                else
                {
                    if (control.Hover) //&& (c.type == ComponentType.Button))
                        return control;
                }
            }
            return null;
        }

        public override void Drag(PointF point)
        {
            var widgets = QB.Root.GetControlsByDirectory(this.FullName).OfType<QB.Controls.Control>();
            foreach (var c in widgets.Where(c => c.Bounds.Contains(point.X, point.Y)))
                c.Drag(point);
        }

        public virtual void Clicked(PointF point)
        {
            if (true) //MIGRATION-new
            {
                var widgets = QB.Root.GetControlsByDirectory(this.FullName).OfType<QB.Controls.Control>();

                SortedDictionary<double, Control> items = new SortedDictionary<double, Control>();


                foreach (var c in widgets.Where(c => c.Bounds.Contains(point.X, point.Y)))
                {
                    double size = c.Bounds.W * c.Bounds.H;
                    while (items.ContainsKey(size))
                        size++;

                    if (c.Visible)
                        items.Add(size, c);
                }

                //var widget = widgets.FirstOrDefault(c => c.Bounds.Contains(point));

                Control widget = null;
                if (items.Count > 0)
                    widget = items.Values.First();


                if (widget != null)
                {
                    //if (widget.OnClick != null && widget.Visible)
                    //{
                    //    //DoAction(FullName + "\\" + widget.onclick.code.Replace('\\', '+'));
                    //    widget.OnClick(widget);
                    //}
                    if (widget.Visible)
                    {
                        widget.RaiseOnClick(point);
                    }

                    //if (widget is Button)
                    //    (widget as Button).RaiseOnClick();

                    //if (widget is Button)
                    //    (widget as Button).OnClick2();


                    if (widget is QB.Controls.TableView && widget.Visible)
                    {
                        (widget as QB.Controls.TableView).HandleClick(point);
                    }
                    else if (widget is QB.Controls.Widget && widget.Visible)
                    {
                        (widget as QB.Controls.Widget).HandleClick(point);
                    }
                }
            }

            //if (true) //MIGRATION-old
            //{
            //    var widgets = qbObjectHelper.GetWidgetsByKeyPrefix(this.FullName);
            //    //    if (widgets.Count > 0)


            //    var widget = widgets.Values.OfType<qbWidgets.Widget>().FirstOrDefault(c => c.bounds.Contains(point));
            //    if (widget != null)
            //    {
            //        if (widget.onclick != null)
            //        {
            //            DoAction(FullName + "\\" + widget.onclick.code.Replace('\\', '+'));


            //            //  DoAction(widget.onClickCode.Replace(" ", "").Replace("()=>", ""));
            //        }

            //        if (widget is qbWidgets.TableWidget)
            //        {
            //            (widget as qbWidgets.TableWidget).HandleClick(point);
            //        }
            //    }
            //}


            //var comp = Components.FirstOrDefault(c => c.bounds.Contains(point));
            //if (comp != null)
            //{
            //    Main.Qb.Book.Grid = 0;
            //    Main.Qb.Book.DesignMode = false;
            //    Main.Qb.Book.TagMode = false;

            //    if (!comp.enable)
            //        return;

            //    //HALE: allow editing table -> "table"-cell (double click?!)
            //    if (comp.type is ComponentType.Table && System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Control)
            //    {
            //        //get col, row
            //        double cW = 20;
            //        double rH = 5;
            //        double x = point.X - comp.bounds.X;
            //        double y = point.Y - comp.bounds.Y;
            //        int row = (int)(y / rH);
            //        int col = (int)(x / cW);
            //        bool allowEdit = true;
            //        if (allowEdit)
            //        {
            //            string data = Main.Qb.GetS(this.FullName, comp.source.Trim('\"'));
            //            if (data == null)
            //                return;
            //            string[] rows = data.Replace("\r\n", "\n").Split('\n');
            //            if (row < rows.Length)
            //            {
            //                string[] cols = rows[row].Split(';');
            //                if (col < cols.Length)
            //                {
            //                    string value = cols[col];
            //                    ScriptInputDialog dialog = new ScriptInputDialog();
            //                    dialog.Title = "CELL VALUE";
            //                    dialog.Info = $"Change Value from\r\n   {value}\r\nto:";
            //                    dialog.Value = value;
            //                    //dialog.Width = width;
            //                    //dialog.Height = heigth;

            //                    dialog.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            //                    var parentForm = System.Windows.Forms.Application.OpenForms[0];
            //                    dialog.Location = new System.Drawing.Point(parentForm.Left + (parentForm.Width - dialog.Width) / 2, parentForm.Top + (parentForm.Height - dialog.Height) / 2);

            //                    var dr = dialog.ShowDialog();
            //                    if (dr == System.Windows.Forms.DialogResult.OK)
            //                    {
            //                        cols[col] = dialog.Value;
            //                        rows[row] = string.Join(";", cols);
            //                        Main.Qb.Set(null, comp.source.Trim('\"'), string.Join("\n", rows));
            //                    }
            //                }
            //            }
            //        }
            //    }

            //MIGRATION-old
            //if (comp.onClickProp != null)
            //{
            //    if (comp.onClickProp.ToLower().Trim().StartsWith("lincheck("))
            //        (new Task(() => x_Macros.Lincheck(comp.onClickProp))).Start();
            //    else if (comp.onClickProp.ToLower().Trim().StartsWith("egc"))
            //        (new Task(() => x_Macros.Egc(comp.onClickProp))).Start();
            //    else
            //        DoAction(comp.onClickProp);
            //}
            //}
        }

        /*
        public static T DeepCopy<T>(T other)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, other);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }*/


        public override string ToString()
        {
            return Name + " " + Bounds.ToString();
        }


    }
}
