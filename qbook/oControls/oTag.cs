using QB.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;

namespace qbook
{
    [Serializable]
    public class oTag : oControl
    {
        public oTag()
        {
        }
        public oTag(string name, string text, double x, double y) : base(name, text)
        {
            Bounds = new Bounds(x, y, 0, 0);
        }
        public double XT { get; set; }
        public double YT { get; set; }
        public string Position { get; set; }

        [XmlIgnore]
        public static double[] rCursor = new double[3] { 0, 0, 0 };
        [XmlIgnore]
        public static double[] lCursor = new double[3] { 0, 0, 0 };
        [XmlIgnore]
        public static double rCursorX { get; set; }
        [XmlIgnore]
        public static double lCursorX { get; set; }

        PointF[] Lines(double x1, double y1, double x2, double y2, bool left)
        {
            List<PointF> l = new List<PointF>();
            l.Add(new PointF((float)x1, (float)y1));
            double x = 0;
            if ((x2 - x1) > 0)
            {
                if ((y1 - y2) > 0)
                    x = x1 + (y1 - y2);
                else
                    x = x1 + (y2 - y1);
                if (x > x2)
                    x = x2;
            }
            else
            {
                if ((y1 - y2) > 0)
                    x = x1 - (y1 - y2);
                else
                    x = x1 - (y2 - y1);
                if (x < x2)
                    x = x2;
            }
            l.Add(new PointF((float)x, (float)y2));
            l.Add(new PointF((float)x2, (float)y2));
            if (left)
                l.Add(new PointF((float)x2 - 55, (float)y2));
            else
                l.Add(new PointF((float)x2 + 55, (float)y2));
            return l.ToArray();
        }


        SolidBrush tDarkOrange = new SolidBrush(Color.FromArgb(50, Color.DarkOrange));
        double labelh = 10;
        double labelw = 10;
        public double widthFromPage = 10;
        public override void Render()
        {
            if (Position == null)
                Position = "TR";
            bool flushRight = Position.Contains("L") ? true : false;

            List<PointF> line2 = new List<PointF>();

            //   if (flushRight)
            //   line2.Add(new PointF(Bounds.X - Bounds.W, Bounds.Y));
            // else
            //   line2.Add(new PointF(Bounds.X, Bounds.Y));


            if (flushRight)
            {
                //   line2.Add(new PointF(Bounds.X - Bounds.W, Bounds.Y));
                line2.Add(new PointF((float)Bounds.X, (float)Bounds.Y));
            }
            else
            {
                //   line2.Add(new PointF(Bounds.X, Bounds.Y));
                line2.Add(new PointF((float)Bounds.X + (float)Bounds.W, (float)Bounds.Y));
            }


            //     Bounds.W = 0;
            //   Bounds.H = 15;

            Draw.FillRectangle(Draw.BgDesignBrush, Bounds.X, Bounds.Y, Bounds.W, Bounds.H);
            Draw.Rectangle(Draw.GetPen2(Draw.GrayBrush.Color, 0.5f), Bounds.X, Bounds.Y, Bounds.W, Bounds.H);





            Frame(false, true);

            double mmCursorX = 0;
            double mmCursorY = 1;
            if (flushRight)
            {
                if (Position.Contains("T"))
                    mmCursorY = lCursor[0];
                if (Position.Contains("M"))
                    mmCursorY = lCursor[1];
                if (Position.Contains("B"))
                    mmCursorY = lCursor[2];
                mmCursorX = lCursorX;
            }
            else
            {
                if (Position.Contains("T"))
                    mmCursorY = rCursor[0];
                if (Position.Contains("M"))
                    mmCursorY = rCursor[1];
                if (Position.Contains("B"))
                    mmCursorY = rCursor[2];
                mmCursorX = rCursorX;
            }

            line2.Add(new PointF((float)mmCursorX, (float)mmCursorY));

            if (flushRight)
            {

            }
            else
            {
                //    line2.Add(new PointF(mmCursorX + 55, mmCursorY));
            }

            XT = mmCursorX;
            YT = mmCursorY;

            Draw.Lines(new Pen(Color.White, 5), Lines(line2[0].X, line2[0].Y, line2[1].X, line2[1].Y, flushRight), true);
            Draw.Lines(new Pen(Selected ? Color.Orange : Color.ForestGreen, 3), Lines(line2[0].X, line2[0].Y, line2[1].X, line2[1].Y, flushRight), true);

            if (flushRight)
            {
                Draw.Circle(Selected ? Color.Orange : Color.ForestGreen, Bounds.X, Bounds.Y, 4.0f, 0);
                Draw.Circle(Color.White, Bounds.X, Bounds.Y, 4.0f, 0.8f);
            }

            else
            {
                Draw.Circle(Selected ? Color.Orange : Color.ForestGreen, Bounds.X + Bounds.W, Bounds.Y, 4.0f, 0);
                Draw.Circle(Color.White, Bounds.X + Bounds.W, Bounds.Y, 4.0f, 0.8f);
            }
            if (TextL != null)
            {
                string[] lines = TextL.Replace("\r\n", "\n").Trim('\n').Split('\n');
                bool header = true;

                double textBlockWidth = 0;
                /*//     if (flushRight)
                foreach (string line in lines)
                {
                    SizeF size = Draw.g.MeasureString(header ? line : line, header ? Draw.fontHeader3 : Draw.fontText);
                    textBlockWidth = Math.Max(textBlockWidth, size.Width +5);
                    textBlockWidth = Math.Min(textBlockWidth, (widthFromPage) *Draw.mmToPx);
                    header = false;
                }
                */
                textBlockWidth = 55 * Draw.mmToPx;

                labelw = textBlockWidth / Draw.mmToPx;

                if (!flushRight)
                    textBlockWidth = 0;

                header = true;

                if (!flushRight)
                    Draw.FillRectangle(Draw.WhiteBrush, XT - 0.5f, YT - 5f - 0.5f, labelw + 1, labelh + 1);
                else
                    Draw.FillRectangle(Draw.WhiteBrush, XT - labelw - 0.5f, YT - 5f - 0.5f, labelw + 1, labelh + 1);
                foreach (string line in lines)
                {
                    if (line.Trim().Length > 0)
                    {
                        if (header)
                        {
                            if (qbook.Core.ThisBook.DesignMode)
                            {
                                Draw.Text("[" + Marker + "] " + this.GetType().ToString().Replace("Main.o", "") + " " + Name, mmCursorX - textBlockWidth / Draw.mmToPx, mmCursorY - 5f - 5, 0, Draw.fontFootnote, Selected ? Color.Orange : Color.LightSlateGray);
                                //                 Draw.Text("[" + Marker + "] ", mmCursorX - textBlockWidth / Draw.mmToPx, mmCursorY - 5f - 5, 0, Draw.fontHeader3, Selected ? Color.Orange : Color.Black, false);
                            }

                            foreach (string line3 in SplitText(line, Draw.fontHeader3, labelw * Draw.mmToPx))
                            {
                                Draw.Text(line3, mmCursorX - textBlockWidth / Draw.mmToPx, mmCursorY - 6f, textBlockWidth, Draw.fontHeader3, Selected ? Color.Orange : Color.ForestGreen, System.Drawing.ContentAlignment.MiddleLeft);
                                mmCursorY += 5.5f;
                            }

                            //       Draw.Text(line, mmCursorX - textBlockWidth / Draw.mmToPx, mmCursorY - 5f, 0, Draw.fontHeader3, Selected ? Color.Orange : Color.SteelBlue, false);
                            //  mmCursorY += 7f;
                        }
                        else
                        {
                            foreach (string line3 in SplitText(line, Draw.fontText, labelw * Draw.mmToPx))
                            {
                                Draw.Text(line3, mmCursorX - textBlockWidth / Draw.mmToPx, mmCursorY - 5f, textBlockWidth, Draw.fontText, Selected ? Color.Orange : Color.ForestGreen, System.Drawing.ContentAlignment.MiddleLeft);
                                mmCursorY += 5f;
                            }


                            //  Draw.Text(line, mmCursorX - textBlockWidth / Draw.mmToPx, mmCursorY - 5f, 0, Draw.fontText, Selected ? Color.Orange : Color.SteelBlue, false);
                            //mmCursorY += 6f;
                        }
                        header = false;
                    }
                }
                labelh = mmCursorY - YT;
                mmCursorY += 7f;
            }
            if (flushRight)
            {
                if (Position.Contains("T"))
                    lCursor[0] = mmCursorY;
                if (Position.Contains("M"))
                    lCursor[1] = mmCursorY;
                if (Position.Contains("B"))
                    lCursor[2] = mmCursorY;
            }
            else
            {
                if (Position.Contains("T"))
                    rCursor[0] = mmCursorY;
                if (Position.Contains("M"))
                    rCursor[1] = mmCursorY;
                if (Position.Contains("B"))
                    rCursor[2] = mmCursorY;
            }
        }
    }
}
