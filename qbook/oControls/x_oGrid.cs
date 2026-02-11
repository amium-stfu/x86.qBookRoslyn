using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Drawing;
using System.IO;

namespace Main
{
    public class x_oGrid : oControl
    {
        public x_oGrid()
        {
        }
        public x_oGrid(string name, string text) : base(name, text)
        {
        }

        public string Data;

        override public void Render(Graphics g = null)
        {
            foreach (var item in ObjectSettings.Items.Where(i => i.Type == "grid"))
            {
                if ((item["source"] != null))// && (item["name"] != null))
                {
                    string data = Main.Qb.GetS(null, item["source"]?.ToString());
                    if (data != null)
                        Data = data;
                    Grid_(item["x"]?.ToString().ToInt() ?? 1, item["y"]?.ToString().ToInt() ?? 1);
                }
            }

            if (SettingsIs("mode", "chart"))
                Chart_();
            if (SettingsIs("mode", "grid"))
                Grid();
            Frame(true, true);
        }


        void Grid_(int x, int y)
        {
            if (Data == null)
                return;
            string[] lines = Data.Replace("\r\n","\n").Split('\n');

            double cW = 20;
            double rH = 5;
            int row = 0;


            bool bgToggle = false;
            while ((row * rH) < Bounds.H)
            {
                bgToggle = !bgToggle;
                if (bgToggle)
                    Draw.FillRectangle(Draw.BgDesignBrush, Bounds.X, Bounds.Y + row * rH, Bounds.W, rH);
                int column = 0;
                while ((column * cW) < Bounds.W)
                {
                 //   Draw.Rectangle(Draw.BgDesignBrush, Bounds.X + column * cW, Bounds.Y + row * rH, cW-1, 4, 0.1f);

                    if ((y - 1 + row) < lines.Length)
                    {
                        string[] value = lines[y - 1 + row].Split(';');// [x - 1 + row];
                        if ((x - 1 + column) < value.Length)
                        {
                            string text = value[x - 1 + column];

                            text = text.Replace("\t", "");

                            double n;
                     //       bool isNumeric = float.TryParse(text, out n);
                            if (double.TryParse(text, out n))
                            {
                                Draw.Text(text, Bounds.X + column * cW, Bounds.Y + row * rH+0.5f, cW, Draw.fontFootnoteFixed, Color.Black, Draw.Alignment.R);

                            }
                            else
                            {
                                Draw.Text(text, Bounds.X + column * cW, Bounds.Y + row * rH+ 0.5f, cW, Draw.fontFootnoteFixed, Color.Black, Draw.Alignment.L);
                                
                            }

                            /*
                            if (text.StartsWith("\t\t"))
                                Draw.Text(text.Replace("\t\t", ""), Bounds.X + column * cW, Bounds.Y + row * rH, cW, Draw.fontTextFixed, Color.Black, Draw.Alignment.R);
                            else if (text.StartsWith("\t"))
                                Draw.Text(text.Replace("\t", ""), Bounds.X + column * cW, Bounds.Y + row * rH, cW, Draw.fontTextFixed, Color.Black, Draw.Alignment.C);
                            else
                                Draw.Text(text, Bounds.X + column * cW, Bounds.Y + row * rH, cW, Draw.fontText, Color.Black, Draw.Alignment.L);
                          */
                            
                            /*
                                if ((lines.Length > (r - 1)) && (lines[r - 1].Split(';').Length > (c - 1)))
                                {
                                    string text = lines[r - 1].Split(';')[c - 1];
                                    // SizeF size = Draw.g.MeasureString(text, Draw.fontFootnote);
                                    // g.DrawString(Text, fontText, Brushes.LightSlateGray, x_2 - size.Width / 2, ymin);
                                    Draw.Text(text, x, y, w, Draw.fontFootnote, Color.Black);
                                }*/
                        }
                    }
                    column++;
                }
                row++;
            }
        }

        void Grid()
        {
            if (Data == null)
                return;
            string[] lines = Data.Split('\n');

            int colFrom = (int)SettingsReadValue("cRange", "..", 0, 1);
            int colTo = (int)SettingsReadValue("cRange", "..", 1, 5);
            int rowFrom = (int)SettingsReadValue("rRange", "..", 0, 1);
            int rowTo = (int)SettingsReadValue("rRange", "..", 1, 5);

            for (int c = colFrom; c <= colTo; c++)
            {
                double w = Bounds.W / (colTo - colFrom + 1);
                double x = Bounds.X + (c - colFrom) * w;
                for (int r = rowFrom; r <= rowTo; r++)
                {
                    double h = Bounds.H / (rowTo - rowFrom + 1);
                    double y = Bounds.Y + (r - rowFrom) * h;

                    Draw.Rectangle(Draw.GetPen2(Draw.DesignBrush.Color, 0.1f), x, y, w, h);
                    if ((lines.Length > (r - 1)) && (lines[r - 1].Split(';').Length > (c - 1)))
                    {
                        string text = lines[r - 1].Split(';')[c - 1];
                        // SizeF size = Draw.g.MeasureString(text, Draw.fontFootnote);
                        // g.DrawString(Text, fontText, Brushes.LightSlateGray, x_2 - size.Width / 2, ymin);
                        Draw.Text(text, x, y, w, Draw.fontFootnote, Color.Black);
                    }
                }
            }
        }

        public void Chart_()
        {
            if (Data == null)
                return;
            string[] lines = Data.Split('\n');

            int xmin = (int)SettingsReadValue("xRange", "..", 0, 0);
            int xmax = (int)SettingsReadValue("xRange", "..", 1, lines.Length);
            int column = (int)SettingsReadValue("column", null, 0, 1);
            Pen pen = new Pen(Color.Red, 3);

            try
            {
                Color lColor = ColorTranslator.FromHtml(SettingsRead("pen", ",", 0, "black"));
                pen = new Pen(
               ColorTranslator.FromHtml(SettingsRead("pen", ",", 0, "black")),
               (float)SettingsReadValue("pen", ",", 1, 1));
            }
            catch
            { }


            List<PointF> curve = new List<PointF>();
            double min = double.MaxValue;
            double max = double.MinValue;
            for (int i = 0; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(';');
                try
                {
                    double value = double.Parse(values[column].Replace(",", "."), System.Globalization.NumberFormatInfo.InvariantInfo);
                    min = Math.Min(min, value);
                    max = Math.Max(max, value);
                }
                catch
                { }
            }

            try
            {
                min = SettingsReadValue("yRange", "..", 0, min);
                max = SettingsReadValue("yRange", "..", 1, max);
            }
            catch
            { }

            for (int i = xmin; i < xmax; i++)
            {
                try
                {
                    string[] values = lines[i].Split(';');

                    curve.Add(
                        new PointF(
                            (float)(Bounds.X + (i - xmin) / (xmax - xmin) * Bounds.W), (float)toYmm(double.Parse(values[column].Replace(",", "."), System.Globalization.NumberFormatInfo.InvariantInfo), min, max)));
                    //          (Bounds.Y + Bounds.H - (float.Parse(values[column]) - min) / (max - min) * Bounds.H))); ;
                }
                catch
                { }
            }

            if (max > min)
            {
                double dy = dGrid(max - min);
                double y = dDigits(min, dy);

                while (y < max)
                {
                    if (y > min)
                    {
                        double vmm = toYmm(y, min, max);
                        Draw.Line(Draw.DrawPen, Bounds.X, vmm, Bounds.X + Bounds.W, vmm);
                        Draw.Text("" + y, Bounds.X - 10, vmm - 4.5f, 0, Draw.fontFootnote, Color.SteelBlue);
                    }
                    y = Math.Round(y + dy, 5);
                }
            }

            if (xmax > xmin)
            {
                double dx = dGrid(xmax - xmin);
                double x = dDigits(xmin, dx);

                while (x < xmax)
                {
                    if (x > xmin)
                    {
                        double vmm = toXmm(x, xmin, xmax);
                        Draw.Line(Draw.DrawPen, vmm, Bounds.Y, vmm, Bounds.Y + Bounds.H);
                        Draw.Text("" + x, vmm, Bounds.Y + Bounds.H + 1, 0, Draw.fontFootnote, Color.SteelBlue);
                    }
                    x = Math.Round(x + dx, 8);
                }
            }

            Draw.Line(Pens.SteelBlue, Bounds.X, Bounds.Y, Bounds.X, Bounds.Y + Bounds.H + 5);
            Draw.Line(Pens.SteelBlue, Bounds.X - 5, Bounds.Y + Bounds.H, Bounds.X + Bounds.W, Bounds.Y + Bounds.H);

            //        Draw.Text("" + min, Bounds.X - 10, Bounds.Y + Bounds.H - 4.5f, 0, Draw.fontFootnote, Color.SteelBlue, false);
            //        Draw.Text("" + max, Bounds.X - 10, Bounds.Y - 4.5f, 0, Draw.fontFootnote, Color.SteelBlue, false);

            Draw.Text("" + xmin, Bounds.X, Bounds.Y + Bounds.H + 1, 0, Draw.fontFootnote, Color.SteelBlue);
            Draw.Text("" + xmax, Bounds.X + Bounds.W, Bounds.Y + Bounds.H + 1, 0, Draw.fontFootnote, Color.SteelBlue);

            Draw.Lines(pen, curve.ToArray(), true);
        }

        float dDigits(double v, double range)
        {
            string v1 = v.ToString("00000000.00000000000");
            string v2 = range.ToString("00000000.00000000000");
            int i = v1.Length - 1;
            while (v2[i] == '0')
            {
                v1 = v1.Remove(i);
                i--;
            }

            return float.Parse(v1, System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        double toYmm(double v, double min, double max)
        {
            return
                Bounds.Y + Bounds.H - (v - min) / (max - min) * Bounds.H;
        }

        double toXmm(double v, double min, double max)
        {
            return
                Bounds.X + (v - min) / (max - min) * Bounds.W;
        }

        double dGrid(double range)
        {
            if (range < 0.0001)
                return 0.000005f;
            if (range < 0.0002)
                return 0.00001f;
            if (range < 0.0005)
                return 0.00002f;
            if (range < 0.001)
                return 0.00005f;
            if (range < 0.002)
                return 0.0001f;
            if (range < 0.005)
                return 0.0002f;
            if (range < 0.01)
                return 0.0005f;
            if (range < 0.02)
                return 0.001f;
            if (range < 0.05)
                return 0.002f;
            if (range < 0.1)
                return 0.005f;
            if (range < 0.2)
                return 0.01f;
            if (range < 0.5)
                return 0.02f;
            if (range < 1)
                return 0.05f;
            if (range < 2)
                return 0.1f;
            if (range < 5)
                return 0.2f;
            if (range < 10)
                return 0.5f;
            if (range < 20)
                return 1;
            if (range < 50)
                return 2;
            if (range < 100)
                return 5;
            if (range < 200)
                return 10;
            if (range < 500)
                return 20;
            if (range < 1000)
                return 50;
            if (range < 2000)
                return 100;
            if (range < 5000)
                return 200;
            if (range < 10000)
                return 500;
            if (range < 20000)
                return 1000;
            if (range < 50000)
                return 2000;
            if (range < 100000)
                return 5000;
            if (range < 200000)
                return 10000;
            if (range < 500000)
                return 20000;
            return 100000;
        }

        //public override void Clicked(PointF point)
        //{
        //}
    }
}
