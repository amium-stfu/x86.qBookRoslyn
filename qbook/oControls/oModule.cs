using QB; //qbookCsScript
using QB.Controls;
using System;
using System.Drawing;

namespace qbook
{
    [Serializable]
    public class oModule : oControl
    {
        static Font font0 = new Font("Consolas", 7.75f, FontStyle.Regular);
        static Font font1 = new Font("Consolas", 8.25f);
        static Font font2 = new Font("Consolas", 12f, FontStyle.Bold);
        static Font font1Consolas = new Font("Consolas", 8.25f);
        static Font font3Consolas = new Font("Consolas", 22f, FontStyle.Bold);

        public oModule()
            : base()
        {
        }

        SolidBrush tGray = new SolidBrush(Color.FromArgb(50, Color.Gray));
        SolidBrush tDarkOrange = new SolidBrush(Color.FromArgb(50, Color.DarkOrange));

        public override void Render()
        {

            // float y = 1;
            ushort key = (ushort)Text.ToDouble();
            /*
            if (!Udl.Module.ContainsKey(key))
            {
                Frame(true, true);
                return;
            }
            */

            Module module = null;// = Udl.Module[key];

            if (module == null)
            {
                Frame(true, true);
                return;
            }

            Draw.g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            int Width = (int)(Bounds.W * Draw.mmToPx);
            int Height = (int)(Bounds.H * Draw.mmToPx);

            Brush backgroundBrush = Brushes.WhiteSmoke;
            if (module.LastUpdate < (DateTime.UtcNow - TimeSpan.FromSeconds(3)))
                backgroundBrush = Brushes.Silver;

            int xo = (int)(Bounds.X * Draw.mmToPx);
            int yo = (int)(Bounds.Y * Draw.mmToPx);

            int y = 0;
            string id = module.NetId.ToString("X4");

            if ((module.NetId & 0x0f) == 0)
            {
                y = 5;

                if (module.Alert > 0)
                    Draw.FillRectangle(Draw.RedBrush, Bounds.X, Bounds.Y, Bounds.W, Bounds.H);
                //  Draw.g.FillRectangle(Brushes.Tomato, xo +7, yo +2, Width - 8, Height - 2 - 1);
                else
                    Draw.FillRectangle(Draw.BgDesignBrush, Bounds.X, Bounds.Y, Bounds.W, Bounds.H);
                //   Draw.g.FillRectangle(backgroundBrush, xo + 7, yo + 2, Width - 8, Height - 2 - 1);
                //      Draw.g.DrawRectangle(Pens.Gainsboro, xo + 7, yo + 2, Width - 8, Height - 2 - 1);


                Draw.Text(id.Substring(0, 3).TrimStart('0'), Bounds.X, Bounds.Y, Bounds.W, Draw.fontHeader1, Color.DimGray);
                //     Draw.g.DrawString(id.Substring(0, 3).TrimStart('0'), font3Consolas, Brushes.DimGray, xo+10, yo + y);

                // if (!UdlProgrammer.Flashing)
                {
                    y = 10;
                    /*
                    Draw.Text(module.CardType.ToString(),  Bounds.X+25, Bounds.Y+2, 10, Draw.fontFootnote, Color.Black, Draw.Alignment.R);// 50, y);
                    Draw.Text(module.CSn.Substring(0, 4),  Bounds.X+35, Bounds.Y+2, 10, Draw.fontFootnote, Color.Black, Draw.Alignment.R);//80, y);
                    y = 21;
                    Draw.Text(module.SensorType.ToString(),  Bounds.X+25, Bounds.Y+6, 10, Draw.fontFootnote, Color.Black, Draw.Alignment.R);//50, y);
                    Draw.Text(module.CSn.Substring(4),  Bounds.X+35, Bounds.Y+6, 10, Draw.fontFootnote, Color.Black, Draw.Alignment.R);//80, y);
               */
                }
            }
            else
            {
                Draw.FillRectangle(Draw.BgDesignBrush, Bounds.X, Bounds.Y, Bounds.W, Bounds.H);
                // Draw.g.FillRectangle(backgroundBrush, xo + 5, yo + 40, Width - 6, Height - 40 - 1);
                //     Draw.g.DrawLine(Pens.Gainsboro, xo + 5, yo + 40, xo + Width - 1, yo + 40);
                //    Draw.g.DrawLine(Pens.Gainsboro, xo + Width - 1, yo + 40, xo + Width - 1, yo + Height - 1);
                //   Draw.g.DrawLine(Pens.Gainsboro, xo + 5, yo + Height - 1, xo + Width - 1, yo + Height - 1);
                y = 17;

                Draw.Text(id.TrimStart('0'), Bounds.X, Bounds.Y, Bounds.W, Draw.fontHeader1, Color.DimGray);
                //     Draw.g.DrawString(id.TrimStart('0'), font2, Brushes.DimGray, xo + 10, yo + y);
            }
            /*
            if (UdlProgrammer.Flashing &&( UdlProgrammer.serialNumber == Module.CardSerialNumber))
            {
                int h = (Height - 10)* UdlProgrammer.Percentage / 100;
                Draw.g.FillRectangle(Brushes.DarkOliveGreen, Width - 30, Height - h, 25, h-5);
                Module.State = uint.MaxValue;
                Module.Set = float.NaN;
              //  textBoxCommand.Hide();
              //  textBoxSet.Hide();
                return;
            }
            */


            /*
            y = 41;
            Draw.Text(module.Text + "-" + module.Label, Bounds.X + 5, Bounds.Y + 12, Bounds.W - 5, Draw.fontHeader3, Color.Black);
            //   Draw.g.DrawString(Module.Text + "-" + Module.Label, font1, Brushes.Black, xo + 15, yo + y);

            //SCAN obsolete ? miniChart(module, xo, Bounds.Y + 42, 60 );

            if (!double.IsNaN(module.Offset))
            {
                Draw.Text("O:" + module.Offset.ToString("0.00"), Bounds.X + 5, Bounds.Y + 17, Bounds.W - 5, Draw.fontText, Color.Black);
         //       Draw.g.DrawString("O:" + Module.Offset.Formatted(), font1, Brushes.DarkOliveGreen, xo + 10, yo + y + 25);
            }

            if (!double.IsNaN(module.Gain))
            {
                Draw.Text("G:" + module.Gain.ToString("0.00"), Bounds.X + 5, Bounds.Y + 22, Bounds.W - 5, Draw.fontText, Color.Black);
          //      Draw.g.DrawString("G:" + Module.Gain.Formatted(), font1, Brushes.DarkOliveGreen, xo + 10, yo + y + 35);
            }

            if (!double.IsNaN(module.OperatingHours))
            {
                Draw.Text("OH:" + module.OperatingHours.ToString("0.00"), Bounds.X + 5, Bounds.Y + 27, Bounds.W - 5, Draw.fontText, Color.Black);
          //      Draw.g.DrawString("OH:" + Module.OperatingHours.Formatted(), font1, Brushes.DarkOliveGreen, xo + 10, yo + y + 45);
            }


            y = 120;// textBoxSet.Location.Y;
            if (module.Timer != 0)
            {
                if (module.Timer > 127)
                    Draw.Text("T:" + (module.Timer - 128).ToString("0") + "min", Bounds.X + 5, Bounds.Y + 32, Bounds.W - 5, Draw.fontText, Color.Black);
          //     Draw.g.DrawString("T:" + (Module.Timer - 128).ToString("0") + "min", font1, Brushes.Black, xo + 10, yo + y - 30);
                else
                    Draw.Text("T:" + module.Timer.ToString("0") + "s", Bounds.X + 5, Bounds.Y + 37, Bounds.W - 5, Draw.fontText, Color.Black);
            //    Draw.g.DrawString("T:" + Module.Timer.ToString("0") + "s", font1, Brushes.Black, xo + 10, yo + y - 30);
            }
            */

            if (module.Mode != 0)
            {
                string mode = "0x" + module.Mode.ToString("X2");
                if (module.Mode == 0xc1) mode = "PID";
                if (module.Mode == 0xc2) mode = "iPID";
                if (module.Mode == 0xc8) mode = "PWM";
                if (module.Mode == 0xca) mode = "Switch";
                if (module.Mode == 0xcb) mode = "ON";
                Draw.Text(mode, Bounds.X + 5, Bounds.Y + 42, Bounds.W - 5, Draw.fontText, Color.Black);

                //Draw.g.DrawString(mode, font0, Brushes.Black, xo + 70, yo + y - 30);// + 12);
            }


            if (!double.IsNaN(module.Value))
            {
                /*
                string read = module.Read.Value.ToString("G06");// 0.000");
                if (module.CardType == 487)
                {
                    read = "0x" + ((uint)module.Read.Value).ToString("X4");
                    Draw.g.DrawString(read, font2, Brushes.Black, xo + 10, yo + y - 20);
                }
                else if (module.CardType == 468)
                {
                    read = "0x" + ((uint)module.Read.Value).ToString("X2");
                    Draw.g.DrawString(read, font2, Brushes.Black, xo + 10, yo + y - 20);
                }
                else
                {
                    if (read.Length > 6)
                        read = read.Substring(0, 6).TrimEnd(',').TrimEnd('.');
                    Draw.Text(read, Bounds.X + 30, Bounds.Y + 53, Bounds.W - 5, Draw.fontHeader1, Color.Black);
                    if (module.Unit != "")
                        Draw.Text("" + module.Unit, Bounds.X + 30, Bounds.Y + 53, Bounds.W - 50, Draw.fontHeader3, Color.Black);
                }
                */
            }

            if (!double.IsNaN(module.Set.Value))
            {
                //  if (!textBoxSet.Focused)
                Draw.g.DrawString("" + module.Set.Value.ToString("G7"), font1, Brushes.Black, xo + 9, yo + 120);
                //textBoxSet.Text = Module.Set.ToString("G7");//0.000");
            }

            if (module.State != uint.MaxValue)
            {
                string status = "0x" + module.State.ToString("X2");
                if (module.State == 0x00) status = "Off";
                if (module.State == 0x01) status = "Sp1";
                if (module.State == 0x02) status = "Sp2";
                if (module.State == 0x05) status = "Ext";
                if (module.State == 0x21) status = "Pause";
                if (module.State == 0x22) status = "Ready";
                if (module.State == 0x23) status = "Sample";
                if (module.State == 0x32) status = "S.Zero";
                if (module.State == 0x33) status = "S.Span";
                if (module.State == 0x52) status = "A.Zero";
                if (module.State == 0x53) status = "A.Span";
                Draw.Text("" + status, Bounds.X + 5, Bounds.Y + 62, Bounds.W - 50, Draw.fontHeader3, Color.Black);
            }

            if (module.Alert != int.MaxValue)
                Draw.Text("0x" + module.Alert.ToString("X2"), Bounds.X + 45, Bounds.Y + 62, Bounds.W, Draw.fontHeader3, Color.Black, System.Drawing.ContentAlignment.MiddleRight);
            //   Draw.g.DrawString("0x" + Module.Alert.ToString("X2"), font0, Module.Alert == 0 ? Brushes.Gainsboro : Brushes.Red, xo + 65, yo + y - 7);

            if (!double.IsNaN(module.Out.Value))
                Draw.Text("" + module.Out.Value.ToString("0.0") + module.OutUnit, Bounds.X + 45, Bounds.Y + 67, Bounds.W, Draw.fontHeader3, Color.Black, System.Drawing.ContentAlignment.MiddleRight);
            //   Draw.g.DrawString("" + Module.Out.ToString("0.0") + Module.OutUnit, font0, Module.Out == 0 ? Brushes.Silver : Brushes.Blue, xo + 65, yo + y + 3);


            /*
            if (module.SensorType != 0)
            {
                string mode = "0x" + module.SensorType.ToString("X2");
                if (module.SensorType == 0x421) mode = "Pt100";
                Draw.Text(mode, Bounds.X + 45, Bounds.Y + 72, 10, Draw.fontHeader3, Color.Black, Draw.Alignment.R);
               // Draw.g.DrawString(mode, font0, Brushes.Black, xo + 65, yo + y + 12);
            }
            */
            if (Height > 190)
            {
                y = Height - 65;

                /*
                lock (module.LogList)
                {
                    float o = 0;
                    foreach (string text in module.LogList)
                    {
                        Draw.Text(text, Bounds.X + 5, Bounds.Y + 72 + o, Bounds.W-5, Draw.fontFootnote, Color.Black, Draw.Alignment.R);
                  //      Draw.g.DrawString(text, font0, Brushes.DimGray, xo + 6, yo + y);
                        o += 5;
                    }
                }
                */
            }
            Frame(true, true);
        }



        public void Render2()
        {

            Frame(true, true);

            ushort key = (ushort)Text.ToDouble();

            ushort mid = (ushort)SettingsReadValue("mid", null, 0, 0);


            /*
            if (Udl.Module.ContainsKey(mid))
            {
                Module m = Udl.Module[mid];
                Draw.Text("M" + m.MId.ToString("X4"), Bounds.X + 1, Bounds.Y + y, Bounds.W, Draw.fontText, Selected ? Color.Orange : Color.Black, false);
                y += 6f;
                Draw.Text("" + m.Read, Bounds.X + 1, Bounds.Y + y, Bounds.W, Draw.fontText, Selected ? Color.Orange : Color.Black, false);
                y += 6f;
                Draw.Text("" + m.Set, Bounds.X + 1, Bounds.Y + y, Bounds.W, Draw.fontText, Selected ? Color.Orange : Color.Black, false);
                y += 6f;
                Draw.Text("" + m.State, Bounds.X + 1, Bounds.Y + y, Bounds.W, Draw.fontText, Selected ? Color.Orange : Color.Black, false);
                y += 6f;
                Draw.Text("" + m.Alert, Bounds.X + 1, Bounds.Y + y, Bounds.W, Draw.fontText, Selected ? Color.Orange : Color.Black, false);
                y += 6f;
                //    Udl.Module[0x210].Paint(Draw.g, 0, 50, 200);
            }
            if (Main.Qb.File.DesignMode)
            {
                Draw.Text("[" + Marker + "] ", Bounds.X, Bounds.Y - 5, Bounds.W, Draw.fontFootnote, Selected ? Color.Orange : Color.Gray, false);
                Draw.Rectangle(Selected ? tDarkOrange : tGray, Bounds.X, Bounds.Y, Bounds.W, Bounds.H, 0f);
                Draw.Rectangle(Selected ? tDarkOrange : tGray, Bounds.X, Bounds.Y, 5, 5, 0.5f);
                //    Draw.Rectangle(Selected ? Honeydew : Honeydew, Bounds.X, Bounds.Y, Bounds.W, Bounds.H, 0);
                //   Draw.Rectangle(Selected ? Green : LightGreen, Bounds.X, Bounds.Y, Bounds.W, Bounds.H, 1);
            }
            */
        }


        //SCAN obsolete? 
        /*
        double[] miniChartBuffer = new double[100];
        void miniChart(UDL.Module module, int xo, double y, double miniChartHeight)
        {
            y += miniChartHeight;

            int index = 0;
            int i = 0;
            lock (module.MiniChart)
            {
                foreach (double value in module.MiniChart)
                    miniChartBuffer[index++] = value;
            }
            if (index > 1)
            {

                if ((module.CardType == 464) ||
                    (module.CardType == 468) ||
                    (module.CardType == 487))
                {
                    int bits = 4;
                    int size = 16;
                    if (module.CardType == 468) bits = 8;
                    if (module.CardType == 468) size = 8;
                    if (module.CardType == 487) bits = 16;
                    if (module.CardType == 487) size = 4;



                    i = 0;
                    while (i < index)
                    {
                        while (i < index)
                        {
                            if (!double.IsNaN(miniChartBuffer[i]))
                            {
                            }
                            else
                            {
                                Draw.g.DrawLine(Pens.Salmon, xo +i + 7, (float)y - 60, i + 7, (float)y);
                            }
                            i++;
                        }
                    }

                    for (int b = 0; b < bits; b++)
                    {
                        i = 0;
                        while (i < index)
                        {

                            if (!double.IsNaN(miniChartBuffer[i]) && !double.IsInfinity(miniChartBuffer[i]))// && !float.i(miniChartBuffer[i]))
                            {
                                bool v = (((int)miniChartBuffer[i] >> b) & 0x01) != 0;

                                if (v)
                                {
                                    Draw.g.DrawLine(Pens.LightGray, xo + i + 7, (float)(y - (b * size)), xo + i + 7, (float)(y - (b * size) - (v ? (int)(size * 0.7) : 1)));
                                    //   Draw.g.DrawLine(Pens.DarkGoldenrod, i + 7, y - (b * size), i + 7, y - (b * size) - (v ? (int)(size * 0.7) : 1));
                                    Draw.g.FillRectangle(Brushes.DarkGoldenrod, xo + i + 7, (float)(y - (b * size) - (int)(size * 0.7)), 0.7f, 0.7f);

                                }
                                else
                                {
                                    Draw.g.FillRectangle(Brushes.Silver, xo + i + 7, (float)(y - (b * size)), 0.7f, 0.7f);
                                }
                            }
                            i++;
                        }
                        if (bits <= 8)
                            Draw.g.DrawString((1 << b).ToString("0"), font0, Brushes.Black, xo + 7, (float)(y - (b * size) - 9));
                    }

                }
                else
                {
                    double min = double.MaxValue;
                    double max = double.MinValue;
                    for (int j = 0; j < index; j++)
                    {
                        if (!double.IsNaN(miniChartBuffer[j]))
                        {
                            min = Math.Min(min, miniChartBuffer[j]);
                            max = Math.Max(max, miniChartBuffer[j]);
                        }
                    }
                    if (max - min > 0)
                    {
                        i = 0;
                        while (i < index)
                        {
                            List<Point> line = new List<Point>();
                            while (i < index)
                            {
                                if (!double.IsNaN(miniChartBuffer[i]))
                                {
                                    line.Add(new Point(xo + i + 7, (int)y - (int)((miniChartBuffer[i] - min) / (max - min) * miniChartHeight)));
                                    i++;
                                }
                                else
                                {
                                    Draw.Line(Pens.Salmon, xo + i + 7, y - 60, xo + i + 7, y);

                                    i++;
                                    break;
                                }
                            }
                            try
                            {
                                if (line.Count > 1)
                                    Draw.g.DrawLines(Draw.DrawPen, line.ToArray());
                            }
                            catch
                            { }
                        }
                        Draw.g.DrawString(max.ToString("0.00"), font0, Brushes.Black, xo + 7, (float)y - 60);
                        Draw.g.DrawString(min.ToString("0.00"), font0, Brushes.Black, xo + 7, (float)y - 10);
                    }
                }
            }
        }*/

    }
}
