using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Security;

using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using System.IO;
using QB.Widget;

namespace Main
{
    public class xDraw
    {
        public enum Alignment { TL, T, TR, L, C, R, BL, B, BR };

        public static Alignment AlignmentFromString(string align)
        {
            if (Enum.TryParse(align, true, out Alignment alignment))
                return alignment;
            else
                return Alignment.L;
        }

        static public Graphics g;
        static public bool noFrame = false;
        static double _mmToPx = 1f;
        static public double mmToPx
        {
            get
            {
                return _mmToPx;
            }
            set
            {
                bool changed = _mmToPx != value;
                _mmToPx = value;
                if (changed)
                    OnMmToPxChanged(value);
            }
        }

        public class MmToPxChangedEventArgs : EventArgs
        {
            public double MmToPx;
        }
        public delegate void MmToPxChangedEventHandler(MmToPxChangedEventArgs e);
        static public event MmToPxChangedEventHandler MmToPxChangedEvent;
        static void OnMmToPxChanged(double mmToPx)
        {
            if (MmToPxChangedEvent != null)
            {
                MmToPxChangedEventArgs ea = new MmToPxChangedEventArgs() { MmToPx = mmToPx};
                MmToPxChangedEvent(ea);
            }
        }

        static public double Width = 297f;
        static public double Height = 210f;

        static public double DefaultItemWidth = 60f;
        static double mmToPxPdf = 2.835f; //@72dpi, 595dots, 21cm)


        static public double pxWidth = 1f;
        static public double pxHeight = 1f;

        static public Font fontHeader1;
        static public Font fontHeader2;
        static public Font fontHeader3;
        static public Font fontText;
        static public Font fontFootnote;

        static public Font fontHeader1Fixed;
        static public Font fontHeader2Fixed;
        static public Font fontHeader3Fixed;
        static public Font fontTextFixed;
        static public Font fontFootnoteFixed;
        static public Font fontTerminalFixed;

        public static Pen DesignPen = new Pen(Color.FromArgb(150, Color.LightSlateGray));

        public static Pen DrawPen = new Pen(Color.FromArgb(150, Color.SteelBlue));
        public static Pen penSet = new Pen(Color.FromArgb(180, Color.BlueViolet), 5);
        public static Pen penRead = new Pen(Color.FromArgb(180, Color.SteelBlue), 1);
        //public static Pen penNeedle1 = new Pen(Color.FromArgb(150, Color.Silver), 6);
        public static Pen penNeedle2 = new Pen(Color.FromArgb(255, Color.DodgerBlue), 3);
        public static Pen penMajorTicks = new Pen(Color.FromArgb(255, Color.DimGray), 2);
        public static Pen penMinorTicks = new Pen(Color.FromArgb(120, Color.DarkGray), 1);

        public static Pen penMajorTicksLight = new Pen(Color.FromArgb(90, Color.DimGray), 1);
        public static Pen penMinorTicksLight = new Pen(Color.FromArgb(90, Color.DarkGray), 1);

        public static SolidBrush DesignBrush = new SolidBrush(Color.FromArgb(150, Color.LightSlateGray));

        public static SolidBrush BgDesignBrush = new SolidBrush(Color.FromArgb(30, Color.LightSlateGray));

        public static SolidBrush WhiteBrush = new SolidBrush(Color.FromArgb(180, Color.White));
        public static SolidBrush GrayBrush = new SolidBrush(Color.FromArgb(150, Color.Black));
        public static SolidBrush RedBrush = new SolidBrush(Color.FromArgb(150, Color.Red));
        public static SolidBrush SetBrush = new SolidBrush(Color.FromArgb(150, Color.BlueViolet));
        public static SolidBrush SteelBlueBrush = new SolidBrush(Color.FromArgb(150, Color.SteelBlue));
        //  SolidBrush White = new SolidBrush(Color.White);
        static public XGraphics PdfGraphics { get; set; }


        static Dictionary<Color, SolidBrush> _brushDict2 = new Dictionary<Color, SolidBrush>();
        public static SolidBrush GetBrush2(Color color)
        {
            if (!_brushDict2.ContainsKey(color))
                _brushDict2.Add(color, new SolidBrush(color));
            return _brushDict2[color];
        }

        static Dictionary<string, Pen> _penDict2 = new Dictionary<string, Pen>();
        public static Pen GetPen2(Color color, double width)
        {
            string _color = color.ToArgb() + "" + width;
            if (!_penDict2.ContainsKey(_color))
                _penDict2.Add(_color, new Pen(color,(float)width));
            return _penDict2[_color];
        }

        static Dictionary<string, Brush> _brushDict = new Dictionary<string, Brush>();
        public static Brush GetBrush(string color)
        {
            color = color.Split(':')[0]; //ignore width, etc...
            if (!_brushDict.ContainsKey(color))
                _brushDict.Add(color, new SolidBrush(Misc.ParseColor(color)));
            return _brushDict[color];
        }

        public static double GetBrushWidth(string color)
        {
            var splits = color.Split(':');
            if (splits.Length > 1)
                return splits[1].ToFloat(0.2f);
            else
                return 0.2f;
        }

        static Dictionary<string, Pen> _penDict = new Dictionary<string, Pen>();
        public static Pen GetPen(string color)
        {
            string[] splits = color.ToLower().Split(':');
            color = splits[0]; //ignore width, etc...
            double width = 1.0f;
            if (splits.Length > 1)
                double.TryParse(splits[1], out width);
            width = Math.Round(width, 1);

            string key = color + ":" + width;
            if (!_penDict.ContainsKey(key))
                _penDict.Add(key, new Pen(Misc.ParseColor(color), (float)width));
            return _penDict[key];
        }

        static Pen GetPen(Color color, double width)
        {
            //return GetPen(color.Name +":" + width.ToString("F1"));
            width = (double)Math.Round(width, 1);

            string key = color.Name + ":" + width;
            if (!_penDict.ContainsKey(key))
                _penDict.Add(key, new Pen(color, (float)width));
            return _penDict[key];
        }

        /*
        SolidBrush DarkOrange = new SolidBrush(Color.DarkOrange);
        SolidBrush WhiteSmoke = new SolidBrush(Color.WhiteSmoke);
        SolidBrush tGray = new SolidBrush(Color.FromArgb(50, Color.Gray));
        SolidBrush tDarkOrange = new SolidBrush(Color.FromArgb(50, Color.DarkOrange));


                SolidBrush Honeydew = new SolidBrush(Color.Honeydew);
        SolidBrush Green = new SolidBrush(Color.Green);
        SolidBrush LightGreen = new SolidBrush(Color.LightGreen);
        SolidBrush DarkOrange = new SolidBrush(Color.DarkOrange);
        SolidBrush WhiteSmoke = new SolidBrush(Color.WhiteSmoke);
        */

        static public string PdfFileName { get; set; }
        static public PdfDocument PdfDocument { get; set; }

        static public Bitmap GetSreenshot()
        {

            Bitmap bm = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(bm);
            g.CopyFromScreen(0, 0, 0, 0, bm.Size);
            return bm;
        }


        static public void PdfOpen(string filename, string title)
        {
            if (!filename.ToUpper().EndsWith(".PDF")) filename += ".pdf";
            PdfFileName = filename;
            PdfDocument = new PdfDocument();
            PdfDocument.Info.Title = title;
        }

        static public void PdfSave(bool protect)
        {
            if (PdfDocument == null) return;
            try
            {
                if (protect) //write protect?
                {
                    //-->http://www.pdfsharp.net/wiki/ProtectDocument-sample.ashx?AspxAutoDetectCookieSupport=1
                    PdfSecuritySettings securitySettings = PdfDocument.SecuritySettings;
                    securitySettings.PermitModifyDocument = false;

                    //securitySettings.PermitExtractContent = false;

                    //securitySettings.UserPassword = "amium";
                    //securitySettings.OwnerPassword = "amium07";

                    //(additionally) sign with certificate?
                    //-->https://www.codeproject.com/Articles/84797/iSafePDF-The-Open-Source-PDF-Signature-Tool
                }

                //                if (Xero.Arena.PdfSecurity.Parameter.Value >= 1)
                {
                    PdfSecuritySettings securitySettings = PdfDocument.SecuritySettings;
                    // Setting one of the passwords automatically sets the security level to 
                    // PdfDocumentSecurityLevel.Encrypted128Bit.

                    //securitySettings.UserPassword = PdfUserPassword; //password to open/view the document
                    securitySettings.OwnerPassword = "amium07root#";   //password to change permissions

                    // Don't use 40 bit encryption unless needed for compatibility
                    //securitySettings.DocumentSecurityLevel = PdfDocumentSecurityLevel.Encrypted40Bit;

                    //security=1234567
                    //1: (1) PermitPrint / PermitFullQualityPrint
                    //2: (0) PermitModifyDocument
                    //3: (0) PermitAssembleDocument
                    //4: (1) PermitExtractContent / copy
                    //5: (1) PermitAccessibilityExtractContent / copy
                    //6: (0) PermitAnnotations
                    //7: (0) PermitFormsFill

                    // Restrict some rights.
                    securitySettings.PermitAccessibilityExtractContent = true;
                    securitySettings.PermitAnnotations = false;
                    securitySettings.PermitAssembleDocument = false;
                    securitySettings.PermitExtractContent = true;
                    securitySettings.PermitFormsFill = false;
                    securitySettings.PermitFullQualityPrint = true;
                    securitySettings.PermitModifyDocument = false;
                    securitySettings.PermitPrint = true;
                }
                PdfDocument.Save(PdfFileName);
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("PDF File open!!");
                //  throw ex;
            }
        }

        static public void PdfShow()
        {
            try
            {
                Process.Start(PdfFileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        PageSize[] pageSizes = (PageSize[])Enum.GetValues(typeof(PageSize));

        static public int PdfPageCount = 0;
        static public XGraphics PdfNewPage()
        {



            try
            {
                if (PdfDocument == null) return null;
                PdfPageCount++;
                PdfPage PdfPage = PdfDocument.AddPage();
                PdfPage.Size = PageSize.A4;
                PdfPage.Orientation = PageOrientation.Landscape;

                return XGraphics.FromPdfPage(PdfPage);
            }
            catch
            {
            }
            return null;
        }

        static public SizeF MeasureText(string text, Font font)
        {
            return g.MeasureString(text, font);
        }

        static public void Text(string text, double x, double y, double width, Font font, Color color)
        {
            Text(text, x, y, width, font, color, 0);
        }

        static public void Text(string text, double x, double y, double width, Font font, Color color, Alignment align)
        {
            if (noFrame)
            {
                x -= 10;
                y -= 20;
            }
            if (text == null)
                return;
            //0->left
            //1->center
            //2->right
            try
            {
                if (align == Alignment.R)
                {
                    //StringFormat sf = new StringFormat(StringFormatFlags.DirectionRightToLeft);
                    SizeF sg = g.MeasureString(text, font);
                    g.DrawString(text, font, new SolidBrush(color), new PointF((float)(x * mmToPx + width * mmToPx - sg.Width), (float)(y * mmToPx)));
                }
                else if (align == Alignment.C)
                {
                    SizeF sg = g.MeasureString(text, font);
                    g.DrawString(text, font, new SolidBrush(color), new PointF((float)(x * mmToPx + width * mmToPx / 2 - sg.Width / 2), (float)(y * mmToPx)));
                }
                else //.L
                    g.DrawString(text, font, new SolidBrush(color), new PointF((float)(x * mmToPx), (float)(y * mmToPx)));

                if (PdfGraphics != null)
                {
                    XFontStyle style = XFontStyle.Regular;
                    if (font.Style == FontStyle.Bold) style = XFontStyle.Bold;
                    if (font.Style == FontStyle.Italic) style = XFontStyle.Italic;
                    if (font.Style == FontStyle.Underline) style = XFontStyle.Underline;
                    XFont xFont = new XFont(font.Name, font.Size * 1.40 / Draw.mmToPx * mmToPxPdf, style);
                    XBrush brush = new XSolidBrush(XColor.FromArgb(color.ToArgb()));



                    if (align == Alignment.R)
                    {
                        XStringFormat sf = new XStringFormat();
                        XSize xs = PdfGraphics.MeasureString(text, xFont);
                        PdfGraphics.DrawString(text, xFont, brush, (x + width) * mmToPxPdf - xs.Width, y * mmToPxPdf, XStringFormats.TopLeft);
                    }
                    else if (align == Alignment.C)
                    {
                        XSize xs = PdfGraphics.MeasureString(text, xFont);
                        PdfGraphics.DrawString(text, xFont, brush, (x + width / 2) * mmToPxPdf - xs.Width / 2, y * mmToPxPdf, XStringFormats.TopLeft);
                    }
                    else //.L
                        PdfGraphics.DrawString(text, xFont, brush, x * mmToPxPdf, y * mmToPxPdf, XStringFormats.TopLeft);
                }
            }
            catch
            {
            }
        }

        static public void Line(Pen pen, double x1, double y1, double x2, double y2)
        {
            if (noFrame)
            {
                x1 -= 10;
                y1 -= 20;
                x2 -= 10;
                y2 -= 20;
            }
            if (double.IsNaN(x1))
                return;
            if (double.IsNaN(y1))
                return;
            if (double.IsNaN(x2))
                return;
            if (double.IsNaN(y2))
                return;
            try
            {
                //        pen.Width = width * Draw.mmToPx;
                g.DrawLine(pen, (float)(x1 * mmToPx), (float)(y1 * mmToPx), (float)(x2 * mmToPx), (float)(y2 * mmToPx));
                if (PdfGraphics != null)
                {
                    XPen xpen = new XPen(XColor.FromArgb(pen.Color), pen.Width);
                    PdfGraphics.DrawLine(xpen, x1 * mmToPxPdf, y1 * mmToPxPdf, x2 * mmToPxPdf, y2 * mmToPxPdf);
                }
            }
            catch
            {
            }
        }



        static public void Arrow(Pen pen, double x1, double y1, double length, double angle)
        {
            if (noFrame)
            {
                x1 -= 10;
                y1 -= 20;
            }
                
            try
            {
                double dx = length * Math.Cos(angle * Math.PI / 180);
                double dy = -length * Math.Sin(angle * Math.PI / 180);
                Line(pen, x1, y1, x1 + dx, y1 + dy);
                Circle(pen.Color, x1 + dx, y1 + dy, 1f, 0);
            }
            catch
            {
            }
        }

        static public void Polygon(Pen pen, int vertices, double x1, double y1, double diameter, double angle)
        {
            if (noFrame)
            {
                x1 -= 10;
                y1 -= 20;
            }
            try
            {
                diameter /= 2;
                double x = x1 + diameter * Math.Cos(angle * Math.PI / 180);
                double y = y1 - diameter * Math.Sin(angle * Math.PI / 180);
                for (int i = 0; i < vertices; i++)
                {
                    angle += 360 / vertices;
                    double x2 = x1 + diameter * Math.Cos(angle * Math.PI / 180);
                    double y2 = y1 - diameter * Math.Sin(angle * Math.PI / 180);
                    Line(pen, x, y, x2, y2);
                    x = x2;
                    y = y2;

                }


            }
            catch
            {
            }
        }
        public static PointF pointG(Bounds bounds, double angle, double scale)
        {
            PointF p = new PointF();
            p.X = (float)(bounds.X + bounds.W + bounds.W * Math.Cos(angle * Math.PI / 180) * scale);
            p.Y = (float)(bounds.Y + bounds.H - bounds.H * Math.Sin(angle * Math.PI / 180) * scale);
            return p;
        }
        public static PointF pointG(QB.Rectangle bounds, double angle, double scale)
        {
            PointF p = new PointF();
            p.X = (float)(bounds.X + bounds.W + bounds.W * Math.Cos(angle * Math.PI / 180) * scale);
            p.Y = (float)(bounds.Y + bounds.H - bounds.H * Math.Sin(angle * Math.PI / 180) * scale);
            return p;
        }
        public static double scale(double value, double min1, double max1, double min2, double max2)
        {
            if (min1 > max1)
                return min2 - (Math.Abs(max2 - min2) / Math.Abs(max1 - min1) * (value - min1));
            if (min2 > max2)
                return min2 - (Math.Abs(max2 - min2) / Math.Abs(max1 - min1) * (value - min1));
            return min2 + (Math.Abs(max2 - min2) / Math.Abs(max1 - min1) * (value - min1));
        }

        static public void Line(Pen pen, PointF p1, PointF p2)
        {
            Line(pen, p1.X, p1.Y, p2.X, p2.Y);
        }

        static public void Lines(Pen pen, PointF[] lines, bool mm)
        {
            if (lines.Length < 2)
                return;
            try
            {
              //  float x1
                if (mm)
                {
                    for (int i = 0; i < lines.Length; i++)
                    {

                        float x1 = lines[i].X * (float)Draw.mmToPx;
                        float y1 = lines[i].Y * (float)Draw.mmToPx;
                        if (float.IsInfinity(x1) ||
                        float.IsInfinity(y1))
                        {
                            string a = "";
                                lines[i].Y = 0;
                            lines[i].X = 0;
                        }
                        else
                        {

                            lines[i].Y = y1;
                            lines[i].X = x1;
                        }
                    }
                }

                g.DrawLines(pen, lines);

                if (PdfGraphics != null)
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        lines[i].Y = lines[i].Y / (float)(Draw.mmToPx * Draw.mmToPxPdf);
                        lines[i].X = lines[i].X / (float)(Draw.mmToPx * Draw.mmToPxPdf);
                    }
                    XPen xpen = new XPen(XColor.FromArgb(pen.Color), pen.Width);
                    PdfGraphics.DrawLines(xpen, lines);
                }
            }
            catch
            {
            }
        }

        static public void FillPolygon(Brush brush, PointF[] lines, bool mm)
        {
            try
            {
                if (mm)
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        lines[i].Y = lines[i].Y * (float)Draw.mmToPx;
                        lines[i].X = lines[i].X * (float)Draw.mmToPx;

                        if (noFrame)
                        {
                            lines[i].X -= 10;
                            lines[i].Y -= 20;
                        }

                    }
                }
                g.FillPolygon(brush, lines);
                /*
                if (PdfGraphics != null)
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        lines[i].Y = lines[i].Y / Draw.mmToPx * Draw.mmToPxPdf;
                        lines[i].X = lines[i].X / Draw.mmToPx * Draw.mmToPxPdf;
                    }
                    XPen xpen = new XPen(XColor.FromArgb(pen.Color), pen.Width);
                    PdfGraphics.DrawLines(xpen, lines);
                }*/
            }
            catch
            {
            }
        }


        static public void Circle(Color color, PointF p, double diameter, double penWidth)
        {
            Circle(color, p.X, p.Y, diameter, penWidth);
        }

        static public void Circle(Color color, double x, double y, double diameter, double penWidth)
        {
            if (noFrame)
            {
                x -= 10;
                y -= 20;
            }

            try
            {
                if (penWidth == 0)
                {
                    SolidBrush brush = new SolidBrush(color);
                    g.FillEllipse(brush, (float)((x - diameter / 2) * mmToPx), (float)((y - diameter / 2) * mmToPx), (float)(diameter * mmToPx), (float)(diameter * mmToPx));
                }
                else
                {
                    //Pen pen = new Pen(color, penWidth * mmToPx);
                    Pen pen = GetPen(color, penWidth * mmToPx);
                    g.DrawEllipse(pen, (float)((x - diameter / 2) * mmToPx), (float)((y - diameter / 2) * mmToPx), (float)(diameter * mmToPx), (float)(diameter * mmToPx));

                }

                if (PdfGraphics != null)
                {
                    if (penWidth == 0)
                    {
                        XBrush xbrush = new XSolidBrush(color);
                        PdfGraphics.DrawEllipse(xbrush, (x - diameter / 2) * mmToPxPdf, (y - diameter / 2) * mmToPxPdf, diameter * mmToPxPdf, diameter * mmToPxPdf);
                    }
                    else
                    {
                        XPen xpen = new XPen(color, penWidth * mmToPxPdf);
                        PdfGraphics.DrawEllipse(xpen, (x - diameter / 2) * mmToPxPdf, (y - diameter / 2) * mmToPxPdf, diameter * mmToPxPdf, diameter * mmToPxPdf);
                    }
                }
            }
            catch
            {
            }
        }


        static public void Rectangle(Pen pen, double x, double y, double w, double h)
        {
            if (noFrame)
            {
                x -= 10;
                y -= 20;
            }
            try
            {
                if (double.IsNaN(x))
                    return;
                if (double.IsNaN(y))
                    return;
                Pen penPx = GetPen(pen.Color, pen.Width * mmToPx);
                g.DrawRectangle(penPx, (float)(x * mmToPx), (float)(y * mmToPx), (float)(w * mmToPx), (float)(h * mmToPx));

                if (PdfGraphics != null)
                {
                    XPen xpen = new XPen(pen.Color, pen.Width * mmToPxPdf);
                    PdfGraphics.DrawRectangle(xpen, x * mmToPxPdf, y * mmToPxPdf, w * mmToPxPdf, h * mmToPxPdf);

                }
            }
            catch
            {
            }
        }

        static public void FillRectangle(SolidBrush brush, double x, double y, double w, double h)
        {
            if (noFrame)
            {
                x -= 10;
                y -= 20;
            }
            try
            {
                g.FillRectangle(brush, (float)(x * mmToPx), (float)(y * mmToPx), (float)(w * mmToPx), (float)(h * mmToPx));


                if (PdfGraphics != null)
                {
                    XBrush xbrush = new XSolidBrush(brush.Color);
                    PdfGraphics.DrawRectangle(xbrush, x * mmToPxPdf, y * mmToPxPdf, w * mmToPxPdf, h * mmToPxPdf);
                }
            }
            catch
            {
            }
        }


        static public void Image(Image image, double mmX1, double mmY1, double mmX2, double mmY2)
        {
            if (noFrame)
            {
                mmX1 -= 10;
                mmY1 -= 20;
                mmX2 -= 10;
                mmY2 -= 20;
            }

            try
            {
                double ratioX = image.PhysicalDimension.Width / (mmX2 - mmX1);
                double ratioY = image.PhysicalDimension.Height /(mmY2 - mmY1);
                double ratio = Math.Max(ratioX, ratioY);
                double mmW = image.PhysicalDimension.Width / ratio;
                double mmH = image.PhysicalDimension.Height / ratio;
                double mmHReduction = (mmY2 - mmY1) - mmH;
                double mmWReduction = (mmX2 - mmX1) - mmW;

                g.DrawImage(image, new RectangleF((float)((mmX1 + (mmWReduction / 2)) * mmToPx), (float)((mmY1 + (mmHReduction / 2)) * mmToPx), (float)(mmW * mmToPx), (float)(mmH * mmToPx)));

                if (PdfGraphics != null)
                {

                    
                    Bitmap bitmap = null;

                    Metafile metafile = image as Metafile;

                    if (metafile != null)
                    {
                        int wi = (int)image.PhysicalDimension.Width;
                        int he = (int)image.PhysicalDimension.Height;

                        while (wi > 2000)
                        {
                            wi /= 2;
                            he /= 2;
                        }

                        bitmap = new Bitmap(wi, he);
                        bitmap.SetResolution((float)100, (float)100);
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            g.DrawImage(metafile, 0, 0, (float)wi, (float)he);
                            g.Dispose();
                        }
                       
                        XImage xImage = XImage.FromGdiPlusImage(bitmap);// image);
                        PdfGraphics.DrawImage(xImage, new XRect((mmX1 + (mmWReduction / 2)) * mmToPxPdf, (mmY1 + (mmHReduction / 2)) * mmToPxPdf, mmW * mmToPxPdf, mmH * mmToPxPdf));
                    }
                    else
                    {
                        XImage xImage = XImage.FromGdiPlusImage(image);// bitmap);// image);
                        PdfGraphics.DrawImage(xImage, new XRect((mmX1 + (mmWReduction / 2)) * mmToPxPdf, (mmY1 + (mmHReduction / 2)) * mmToPxPdf, mmW * mmToPxPdf, mmH * mmToPxPdf));
                    }
                }
            }
            catch
            { }
        }


        static public Size ResizeKeepAspect(Size src, int maxWidth, int maxHeight, bool enlarge = false)
        {
            maxWidth = enlarge ? maxWidth : Math.Min(maxWidth, src.Width);
            maxHeight = enlarge ? maxHeight : Math.Min(maxHeight, src.Height);
            decimal rnd = Math.Min(maxWidth / (decimal)src.Width, maxHeight / (decimal)src.Height);
            return new Size((int)Math.Round(src.Width * rnd), (int)Math.Round(src.Height * rnd));
        }

        static public Image ResizeImage(Image image, int maxWidth, int maxHeight)
        {
            Size newSize = ResizeKeepAspect(image.Size, maxWidth, maxHeight);

            var destRect = new System.Drawing.Rectangle(0, 0, newSize.Width, newSize.Height);
            var destImage = new Bitmap(newSize.Width, newSize.Height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        static public string ImageToBase64(Image image, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        static public Image Base64ToImage(string base64String)
        {
            if (base64String == null)
                return null;
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image;
            try
            {
                MemoryStream memoryStream = new MemoryStream(imageBytes);

                //WMF? magic bytes: D7,CD,C6,9A
                if (imageBytes.Length >= 4 && imageBytes[0] == 0xD7 && imageBytes[1] == 0xCD && imageBytes[2] == 0xC6 && imageBytes[3] == 0x9A)
                    image = new Metafile(memoryStream);
                else //other images (jpg, png, bmp, ...)
                    image = System.Drawing.Image.FromStream(ms, true);
            }
            catch
            {
                image = null;
            }
            return image;
        }

        public static string ReplaceUnicodeEntity(string source)
        {
            if (string.IsNullOrEmpty(source)) return source;

            if (!(source.IndexOf("&") < source.IndexOf(";")) && !(source.Contains("\\u") || source.Contains("\\U")))
                return source;

            // Create an array and populate from the dictionary keys, then convert
            // the array to a pipe-delimited string to serve as the regex search
            // values and replace

            //1st: replace "\\u1234" with the unicode-char
            Match m1 = UnicodeRegex.Match(source);
            if (m1.Success)
            {
                int hex = Int16.Parse(m1.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                source = source.Replace("\\u" + m1.Groups[1].Value, Char.ConvertFromUtf32(hex));
            }

            //2nd: replace known html-codes
            return Regex.Replace(source, string.Join("|", xmlEntityReplacements.Keys
        .Select(k => k.ToString()).ToArray()), m => xmlEntityReplacements[m.Value]);
        }
        static Regex UnicodeRegex = new Regex(@"\\u([0-9a-fA-F]{4})");
        static Dictionary<string, string> xmlEntityReplacements = new Dictionary<string, string> {
{"&not;","¬"},
{"&macr;","¯"},
{"&deg;","°"},
{"&plusmn;","±"},
{"&sup2;","²"},
{"&sup3;","³"},
{"&middot;","·"},
{"&sup1;","¹"},
{"&frac14;","¼"},
{"&frac12;","½"},
{"&frac34;","¾"},
{"&times;","×"},
{"&divide;","÷"},
{"&lt;","<"},
{"&gt;",">"},
{"&permil;","‰"},
{"&frasl;","⁄"},
{"&sub2;","₂"},
{"&sub3;","₃"},
{"&sum;","∑"},
{"&minus;","−"},
{"&radic;","√"},
{"&infin;","∞"},
{"&ang;","∠"},
{"&sim;","∼"},
{"&cong;","≅"},
{"&asymp;;","≈"},
{"&ne;","≠"},
{"&equiv;","≡"},
{"&le;","≤"},
{"&ge;","≥"},
{"&oplus;","⊕"},
{"&otimes;","⊗"},
};
    }
}


/*                 if (label.autoSize)
                       {
                           label.font = new Font(Draw.fontHeader3.Name, Draw.fontHeader3.Size* label.size);
               SizeF sg = Draw.MeasureText(text, label.font);
               label.bounds.W = (sg.Width) / Draw.mmToPx; // * this.Bounds.W / sg.Width;
                           label.bounds.H = (sg.Height) / Draw.mmToPx; // * this.Bounds.H / sg.Height;

                           switch (label.align)
                           {
                               case Draw.Alignment.TL:
                                   label.bounds.X = Bounds.X;
                                   label.bounds.Y = Bounds.Y;
                                   break;
                               case Draw.Alignment.T:
                                   label.bounds.X = Bounds.X + (Bounds.W - label.bounds.W) / 2.0f;
                                   label.bounds.Y = Bounds.Y;
                                   break;
                               case Draw.Alignment.TR:
                                   label.bounds.X = Bounds.X + Bounds.W - label.bounds.W;
                                   label.bounds.Y = Bounds.Y;
                                   break;
                               case Draw.Alignment.L:
                                   label.bounds.X = Bounds.X;
                                   label.bounds.Y = Bounds.Y + (Bounds.H - label.bounds.H) / 2.0f;
                                   break;
                               case Draw.Alignment.C:
                                   label.bounds.X = Bounds.X + (Bounds.W - label.bounds.W) / 2.0f;
                                   label.bounds.Y = Bounds.Y + (Bounds.H - label.bounds.H) / 2.0f;
                                   break;
                               case Draw.Alignment.R:
                                   label.bounds.X = Bounds.X + Bounds.W - label.bounds.W;
                                   label.bounds.Y = Bounds.Y + (Bounds.H - label.bounds.H) / 2.0f;
                                   break;
                               case Draw.Alignment.BL:
                                   label.bounds.X = Bounds.X;
                                   label.bounds.Y = Bounds.Y + Bounds.H - label.bounds.H;
                                   break;
                               case Draw.Alignment.B:
                                   label.bounds.X = Bounds.X + (Bounds.W - label.bounds.W) / 2.0f;
                                   label.bounds.Y = Bounds.Y + Bounds.H - label.bounds.H;
                                   break;
                               case Draw.Alignment.BR:
                                   label.bounds.X = Bounds.X + Bounds.W - label.bounds.W;
                                   label.bounds.Y = Bounds.Y + Bounds.H - label.bounds.H;
                                   break;
                           }
       }
       */
