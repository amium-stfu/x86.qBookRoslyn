using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace QB.Controls
{
    public class Sheet
    {
        public Sheet(Color color, double x, double y, double z, double w, double d, double h, string type)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
            D = d;
            H = h;
            Color = color;
            Type = type;
        }
        public string Type { get; set; }
        public Color Color { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double W { get; set; }
        public double D { get; set; }
        public double H { get; set; }

        public Sheet Clone()
        {
            return this.MemberwiseClone() as Sheet;
        }
    }

    public class Bounds3d
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double Z1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double Z2 { get; set; }

        public bool Behing(Bounds3d bounds, bool reverse)
        {
            //  if (reverse)
            {
                //    if (maxX <= rx.minX)
                //      return true;
            }
            // else
            {
                if (bounds.X2 <= X1)
                    return true;
            }

            if (Y2 <= bounds.Y1)
                return true;
            if (bounds.Z2 <= Z1)
                return true;
            return false;
        }
    }


    public class SketchItem

    {
        public Bounds3d Bounds3d;
        public virtual void Draw_()
        {
        }
        public virtual Polygon Clone()
        {
            return null;
        }
    }

    public class Polygon : SketchItem
    {
        public Polygon(Brush brush, Bounds3d bounds3d)
        {
            this.Brush = brush;
            this.Bounds3d = bounds3d;
        }
        public Polygon(Brush brush, List<PointF> line)
        {
            this.Brush = brush;
            this.Line = line;
        }
        public Brush Brush;
        public List<PointF> Line = new List<PointF>();

        public override void Draw_()
        {
            if(Line.Count > 3 )
            Draw.FillPolygon(Brush, Line.ToArray(), true);
        }
        public override Polygon Clone()
        {
            return this.MemberwiseClone() as Polygon;

        }
    }


    public class Sketch : Rectangle
    {
        public List<Sheet> Sheets = new List<Sheet>();

        Color color(Color color, int alpha)
        {
            return System.Drawing.Color.FromArgb(alpha * 255 / 100, color);
        }

        public Sketch(string name) : base(0, 0, 0, 0)
        {
        }

        public Sheet Add(string name, Color color, int alpha, double x, double y, double z, double w = 10, double d = 10, double h = 10, string type = "cube")
        {
            Sheet sheet = new Sheet(this.color(color, alpha), x, y, z, w, d, h, type);
            Sheets.Add(sheet);
            return sheet;
        }
        public Sheet Add(string name, Color color, double x, double y, double z, double w = 10, double d = 10, double h = 10, string type = "cube")
        {
            Sheet sheet = new Sheet(color, x, y, z, w, d, h, type);
            Sheets.Add(sheet);
            return sheet;
        }

        public Sketch Clone()
        {
            Sketch s = new Sketch("");
            s.X = X;
            s.Y = Y;
            s.W = W;
            s.H = H;
            foreach (Sheet sheet in Sheets)
                s.Sheets.Add(sheet.Clone());
            return s;
        }
    }

    public class Canvas : Control
    {
        List<SketchItem> sketchItems = new List<SketchItem>();

        bool reverse = false;
        bool refresh = false;

        PointF toCanvasPoint(Sketch s, double x, double y, double z)
        {
            // cabinet projection: y->45°, y->1/2
            float a = 45f * ((float)Math.PI / 180f);
            float x_ = (float)x + 0.5f * (float)y * (float)Math.Cos(a);
            if (reverse)
                x_ = (float)x - 0.5f * (float)y * (float)Math.Cos(a);

            float y_ = (float)z + 0.5f * (float)y * (float)Math.Sin(a);
            return new PointF((float)(s.X + x_), (float)(s.Y - y_));
        }
        PointF toCanvasPoint(Sketch s, double x, double y, double z, double diameter, double a)
        {
            PointF point = toCanvasPoint(s, x, y, z);
            a = a * ((float)Math.PI / 180f);
            point.X += (float)diameter * 0.5f * (float)Math.Cos(a);
            if (reverse)
                point.X -= (float)diameter * 0.5f * (float)Math.Cos(a);
            point.Y -= (float)diameter * 0.5f * (float)Math.Sin(a);
            return point;
        }

        Color color(Color color, int alpha)
        {
            return System.Drawing.Color.FromArgb(alpha * 255 / 100, color);
        }

        Color color(Color color, int alpha, float brightness)
        {
            brightness /= 2;
            if (brightness > 50) brightness = 50;
            if (brightness >= 0)
            {
                int r1 = 255 - (int)((50 - brightness) / 50 * (255 - color.R));
                int g1 = 255 - (int)((50 - brightness) / 50 * (255 - color.G));
                int b1 = 255 - (int)((50 - brightness) / 50 * (255 - color.B));

                return System.Drawing.Color.FromArgb(alpha * 255 / 100, r1, g1, b1);
            }

            if (brightness < -50) brightness = -50;
            int r = (int)((50 + brightness) / 50 * (color.R));
            int g = (int)((50 + brightness) / 50 * (color.G));
            int b = (int)((50 + brightness) / 50 * (color.B));

            return System.Drawing.Color.FromArgb(alpha * 255 / 100, r, g, b);
        }

        SketchItem insertSketchItem(SketchItem sketchItem)
        {
            for (int i = 0; i < sketchItems.Count; i++)
            {
                if (sketchItems[i].Bounds3d.Behing(sketchItem.Bounds3d, reverse))
                {
                    sketchItems.Insert(i, sketchItem);
                    return sketchItem;
                }
            }
            sketchItems.Add(sketchItem);
            return sketchItem;
        }

        void toSketchItems(Sketch sketch, Sheet sheet)
        {
            //    List<PointF> line = new List<PointF>();
            Bounds3d bounds3d = new Bounds3d();
            bounds3d.X1 = sheet.X;
            bounds3d.X2 = sheet.X + sheet.W;
            bounds3d.Y1 = sheet.Y;
            bounds3d.Y2 = sheet.Y + sheet.D;
            bounds3d.Z1 = sheet.Z;
            bounds3d.Z2 = sheet.Z + sheet.H;

            Polygon polygon;

            if (sheet.Type == "tube:y")
            {
                polygon = (Polygon)insertSketchItem(new Polygon(Draw.GetBrush2(sheet.Color), bounds3d));
                for (int a = 0; a < 360; a += 10)
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y, sheet.Z, sheet.W, a));


                PointF p1 = new PointF((float)(sketch.X + sheet.X - sheet.W / 2) * (float)Draw.mmToPx, (float)(sheet.Z - sheet.H / 2) * (float)Draw.mmToPx);
                PointF p2 = new PointF((float)(sketch.X + sheet.X + sheet.W / 2) * (float)Draw.mmToPx, (float)(sheet.Z + sheet.H / 2) * (float)Draw.mmToPx);
                LinearGradientBrush brush = new LinearGradientBrush(p1, p2, color(sheet.Color, 80, 50), color(sheet.Color, 80, -50));
                polygon = (Polygon)insertSketchItem(new Polygon(brush, bounds3d));

                for (int a = 135; a >= -45; a -= 10)
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y + sheet.D, sheet.Z, sheet.W, a));
                for (int a = -45; a <= 135; a += 10)
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y, sheet.Z, sheet.W, a));
            }
            else
            {
                /*
                if (sheet.W == 0)
                {
                    line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y, sheet.Z));
                    line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y + sheet.D, sheet.Z));
                    line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y + sheet.D, sheet.Z + sheet.H));
                    line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y, sheet.Z + sheet.H));
                    addPolygon(position, new Polygon(Draw.GetBrush2(sheet.Color), line));
                    //Draw.FillPolygon(Draw.GetBrush2(sheet.Color), line.ToArray(), true);
                }
                else if (sheet.D == 0)
                {
                    line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y, sheet.Z));
                    line.Add(toCanvasPoint(sketch, sheet.X + sheet.W, sheet.Y, sheet.Z));
                    line.Add(toCanvasPoint(sketch, sheet.X + sheet.W, sheet.Y, sheet.Z + sheet.H));
                    line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y, sheet.Z + sheet.H));
                    addPolygon(position, new Polygon(Draw.GetBrush2(sheet.Color), line));
                    //Draw.FillPolygon(Draw.GetBrush2(sheet.Color), line.ToArray(), true);
                }
                else if (sheet.H == 0)
                {
                    line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y, sheet.Z));
                    line.Add(toCanvasPoint(sketch, sheet.X + sheet.W, sheet.Y, sheet.Z));
                    line.Add(toCanvasPoint(sketch, sheet.X + sheet.W, sheet.Y + sheet.D, sheet.Z));
                    line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y + sheet.D, sheet.Z));
                    addPolygon(position, new Polygon(Draw.GetBrush2(sheet.Color), line));
                    //Draw.FillPolygon(Draw.GetBrush2(sheet.Color), line.ToArray(), true);
                }
                else*/
                {
                    // back
                    polygon = (Polygon)insertSketchItem(new Polygon(Draw.GetBrush2(color(sheet.Color, 80)), bounds3d));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y + sheet.D, sheet.Z));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X + sheet.W, sheet.Y + sheet.D, sheet.Z));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X + sheet.W, sheet.Y + sheet.D, sheet.Z + sheet.H));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y + sheet.D, sheet.Z + sheet.H));

                    // left
                    polygon = (Polygon)insertSketchItem(new Polygon(Draw.GetBrush2(color(sheet.Color, 80)), bounds3d));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y, sheet.Z));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y + sheet.D, sheet.Z));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y + sheet.D, sheet.Z + sheet.H));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y, sheet.Z + sheet.H));

                    // bottom
                    polygon = (Polygon)insertSketchItem(new Polygon(Draw.GetBrush2(color(sheet.Color, 80)), bounds3d));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y, sheet.Z));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X + sheet.W, sheet.Y, sheet.Z));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X + sheet.W, sheet.Y + sheet.D, sheet.Z));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y + sheet.D, sheet.Z));

                    // front
                    polygon = (Polygon)insertSketchItem(new Polygon(Draw.GetBrush2(color(sheet.Color, 50, 40)), bounds3d));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y, sheet.Z));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X + sheet.W, sheet.Y, sheet.Z));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X + sheet.W, sheet.Y, sheet.Z + sheet.H));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y, sheet.Z + sheet.H));

                    // right
                    polygon = (Polygon)insertSketchItem(new Polygon(Draw.GetBrush2(color(sheet.Color, 50)), bounds3d));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X + sheet.W, sheet.Y, sheet.Z));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X + sheet.W, sheet.Y + sheet.D, sheet.Z));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X + sheet.W, sheet.Y + sheet.D, sheet.Z + sheet.H));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X + sheet.W, sheet.Y, sheet.Z + sheet.H));

                    // top
                    polygon = (Polygon)insertSketchItem(new Polygon(Draw.GetBrush2(color(sheet.Color, 50, 80)), bounds3d));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y, sheet.Z + sheet.H));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X + sheet.W, sheet.Y, sheet.Z + sheet.H));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X + sheet.W, sheet.Y + sheet.D, sheet.Z + sheet.H));
                    polygon.Line.Add(toCanvasPoint(sketch, sheet.X, sheet.Y + sheet.D, sheet.Z + sheet.H));
                }

            }
        }

        public Canvas(string name, double x = 0, double y = 0, double w = 0, double h = 0) : base(name, x: x, y: y, w: w, h: h)
        {
        }

        List<Sketch> sketches = new List<Sketch>();
        public Sketch Add(string name, Sketch sketch, double x, double y)
        {
            sketch.X += Bounds.X + x;
            sketch.Y += Bounds.Y + y;
            sketches.Add(sketch);
            refresh = true;
            return sketch;
        }
        internal override void Render(Control parent)
        {
            base.Render(parent);
            if (refresh)
            {
                sketchItems.Clear();
                refresh = false;
            }

            if (sketchItems.Count > 0)
            {
                foreach (SketchItem item in sketchItems)
                    item.Draw_();
            }
            else foreach (Sketch sketch in sketches)
                {
                    foreach (Sheet sheet in sketch.Sheets)
                        toSketchItems(sketch, sheet);
                }
        }
    }
}
