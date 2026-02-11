using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Windows.Forms;
//using OpenCvSharp;
//using OpenCvSharp.Extensions;

namespace QB.Controls
{
    public class Draw
    {
        /*
        public enum Alignment { TL, T, TR, L, C, R, BL, B, BR };

        public static Alignment AlignmentFromString(string align)
        {
            if (Enum.TryParse(align, true, out Alignment alignment))
                return alignment;
            else
                return Alignment.L;
        }

        public static Alignment AlignmentFromSystemDrawingAlignment(System.Drawing.ContentAlignment align)
        {
            switch (align)
            {
                case ContentAlignment.TopCenter:
                    return Alignment.T;
                case ContentAlignment.TopLeft:
                    return Alignment.TL;
                case ContentAlignment.TopRight:
                    return Alignment.TR;

                case ContentAlignment.MiddleLeft:
                    return Alignment.L;
                case ContentAlignment.MiddleCenter:
                    return Alignment.C;
                case ContentAlignment.MiddleRight:
                    return Alignment.R;

                case ContentAlignment.BottomCenter:
                    return Alignment.B;
                case ContentAlignment.BottomLeft:
                    return Alignment.BL;
                case ContentAlignment.BottomRight:
                    return Alignment.BR;
            }
            return Alignment.C;
        }
        */

        internal static Graphics g;
        internal static bool DesignMode = false;
        internal static bool noFrame = false;
        internal static Rectangle renderBounds = null;
        static double _mmToPx = 1f;



        public static double mmToPx
        {
            get
            {
                return _mmToPx;
            }
            set
            {
                bool changed = _mmToPx != value;
                _mmToPx = value;

                if (false)
                {   //HALE: need this for sizing HTML controls. but it's currently not working correctly as this 
                    //changes permanently in case of multiple windows (UnDocked, PageDialogs, etc)
                    if (changed)
                        OnMmToPxChanged(value);
                }
            }
        }

        public class MmToPxChangedEventArgs : EventArgs
        {
            public double MmToPx;
        }
        public delegate void MmToPxChangedEventHandler(MmToPxChangedEventArgs e);
        public static event MmToPxChangedEventHandler MmToPxChangedEvent;
        static void OnMmToPxChanged(double mmToPx)
        {
            if (MmToPxChangedEvent != null)
            {
                MmToPxChangedEventArgs ea = new MmToPxChangedEventArgs() { MmToPx = mmToPx };
                MmToPxChangedEvent(ea);
            }
        }

        public static double Width = 297f;
        public static double Height = 210f;

        public static double DefaultItemWidth = 60f;
        public static double mmToPxPdf = 2.835f; //@72dpi, 595dots, 21cm)


        public static double pxWidth = 1f;
        public static double pxHeight = 1f;

        public static Font fontHeader1;
        public static Font fontHeader2;
        public static Font fontHeader3;
        public static Font fontText;
        public static Font fontFootnote;

        public static Font fontTerminal;

        public static Font fontHeader1Fixed;
        public static Font fontHeader2Fixed;
        public static Font fontHeader3Fixed;
        public static Font fontTextFixed;
        public static Font fontFootnoteFixed;
        public static Font fontTerminalFixed;

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

        public static SolidBrush BgDesignBrush = new SolidBrush(Color.FromArgb(25, Color.LightSlateGray));
        public static Pen BgDesignPen = new Pen(Color.FromArgb(90, Color.DarkGray), 0.2f);

        public static SolidBrush WhiteBrush = new SolidBrush(Color.FromArgb(180, Color.White));
        public static SolidBrush GrayBrush = new SolidBrush(Color.FromArgb(150, Color.Black));
        public static SolidBrush RedBrush = new SolidBrush(Color.FromArgb(150, Color.Red));
        public static SolidBrush SetBrush = new SolidBrush(Color.FromArgb(150, Color.BlueViolet));
        public static SolidBrush SteelBlueBrush = new SolidBrush(Color.FromArgb(150, Color.SteelBlue));
        //  SolidBrush White = new SolidBrush(Color.White);
        public static XGraphics PdfGraphics { get; set; }


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
                _penDict2.Add(_color, new Pen(color, (float)width));
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
        public static Pen GetPen(string color, double width = 1.0)
        {
            string[] splits = color.ToLower().Split(':');
            color = splits[0]; //ignore width, etc...
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

        static LimitedSizeDictionary<string, Font> _fontDict = new LimitedSizeDictionary<string, Font>(100);
        internal static Font GetFont(string family, double size, string fontStyleStr)
        {
            size = (double)Math.Round(size, 1);

            string key = family + ":" + size + ":" + fontStyleStr;
            if (!_fontDict.ContainsKey(key))
            {
                FontStyle fontStyle = FontStyle.Regular;
                var styles = fontStyleStr.Split('|');
                foreach (var style in styles)
                {
                    switch (style.ToLower())
                    {
                        case "bold":
                        case "b":
                            fontStyle |= FontStyle.Bold;
                            break;
                        case "italic":
                        case "i":
                            fontStyle |= FontStyle.Italic;
                            break;
                        case "strikeout":
                        case "strike":
                        case "s":
                            fontStyle |= FontStyle.Strikeout;
                            break;
                        case "underline":
                        case "line":
                        case "u":
                            fontStyle |= FontStyle.Underline;
                            break;
                    }
                }

                _fontDict.Add(key, new Font(family, (float)size, fontStyle));
            }
            return _fontDict[key];
        }

        internal static Font GetFont(string family, double size, FontStyle fontStyle)
        {
            size = Math.Max((double)Math.Round(size * 2.0) / 2.0, 0.5); //round to 0.5; min=0.5

            string key = family + ":" + size + ":" + fontStyle;
            if (!_fontDict.ContainsKey(key))
            {
                _fontDict.Add(key, new Font(family, (float)size, fontStyle));
            }
            return _fontDict[key];
        }


        public static string PdfFileName { get; set; }
        public static PdfDocument PdfDocument { get; set; }

        public static Bitmap GetSreenshot()
        {

            Bitmap bm = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(bm);
            g.CopyFromScreen(0, 0, 0, 0, bm.Size);
            return bm;
        }


        public static void PdfOpen(string filename, string title)
        {
            if (!filename.ToUpper().EndsWith(".PDF")) filename += ".pdf";
            PdfFileName = filename;
            PdfDocument = new PdfDocument();
            PdfDocument.Info.Title = title;
        }

        public static void PdfSave(bool protect)
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

        public static void PdfShow()
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

        public static int PdfPageCount = 0;
        public static XGraphics PdfNewPage()
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

        public static SizeF MeasureText(string text, Font font)
        {
            return g.MeasureString(text, font);
        }



        public static void Text(string text, double x, double y, double width, Font font, Color color)
        {
            Text(text, x, y, width, font, color, 0);
        }


        public static void Text(string text, double x, double y, double width, Font font, Color color, System.Drawing.ContentAlignment align)
        {
            if (false && noFrame)
            {
                x -= 10;
                y -= 20;
            }
            if (renderBounds != null)
            {
                x -= renderBounds.X;
                y -= renderBounds.Y;
            }

            if (text == null)
                return;
            //0->left
            //1->center
            //2->right
            try
            {
                if (align == System.Drawing.ContentAlignment.MiddleRight || align == System.Drawing.ContentAlignment.TopRight || align == System.Drawing.ContentAlignment.BottomRight)
                {
                    //Right
                    //StringFormat sf = new StringFormat(StringFormatFlags.DirectionRightToLeft);
                    SizeF sg = g.MeasureString(text, font);
                    g.DrawString(text, font, new SolidBrush(color), new PointF((float)(x * mmToPx + width * mmToPx - sg.Width), (float)(y * mmToPx)));
                }
                else if (align == System.Drawing.ContentAlignment.MiddleLeft || align == System.Drawing.ContentAlignment.TopLeft || align == System.Drawing.ContentAlignment.BottomLeft)
                {
                    //width = 0;
                    if (width == 0)
                        g.DrawString(text, font, new SolidBrush(color), new PointF((float)(x * mmToPx), (float)(y * mmToPx)));
                    else
                    {
                        RectangleF lrec = new RectangleF((float)(x * mmToPx), (float)(y * mmToPx), (float)(width * mmToPx), (float)(100 * mmToPx));
                        g.DrawString(text, font, new SolidBrush(color), lrec);
                    }
                }
                else
                {
                    //Center
                    SizeF sg = g.MeasureString(text, font);
                    g.DrawString(text, font, new SolidBrush(color), new PointF((float)(x * mmToPx + width * mmToPx / 2 - sg.Width / 2), (float)(y * mmToPx)));
                }

                if (PdfGraphics != null)
                {
                    XFontStyle style = XFontStyle.Regular;
                    if (font.Style == FontStyle.Bold) style = XFontStyle.Bold;
                    if (font.Style == FontStyle.Italic) style = XFontStyle.Italic;
                    if (font.Style == FontStyle.Underline) style = XFontStyle.Underline;
                    XFont xFont = new XFont(font.Name, font.Size * 1.40 / Draw.mmToPx * mmToPxPdf, style);
                    XBrush brush = new XSolidBrush(XColor.FromArgb(color.ToArgb()));



                    if (align == System.Drawing.ContentAlignment.MiddleRight || align == System.Drawing.ContentAlignment.TopRight || align == System.Drawing.ContentAlignment.BottomRight)
                    {
                        XStringFormat sf = new XStringFormat();
                        XSize xs = PdfGraphics.MeasureString(text, xFont);
                        PdfGraphics.DrawString(text, xFont, brush, (x + width) * mmToPxPdf - xs.Width, y * mmToPxPdf, XStringFormats.TopLeft);
                    }
                    else if (align == System.Drawing.ContentAlignment.MiddleLeft || align == System.Drawing.ContentAlignment.TopLeft || align == System.Drawing.ContentAlignment.BottomLeft)
                    {
                        PdfGraphics.DrawString(text, xFont, brush, x * mmToPxPdf, y * mmToPxPdf, XStringFormats.TopLeft);
                    }
                    else
                    {
                        XSize xs = PdfGraphics.MeasureString(text, xFont);
                        PdfGraphics.DrawString(text, xFont, brush, (x + width / 2) * mmToPxPdf - xs.Width / 2, y * mmToPxPdf, XStringFormats.TopLeft);
                    }
                }
            }
            catch
            {
            }
        }

        public static void Text(string text, double x, double y, double width, double height, Font font, Color color, System.Drawing.ContentAlignment align)
        {
            if (false && noFrame)
            {
                x -= 10;
                y -= 20;
            }
            if (renderBounds != null)
            {
                x -= renderBounds.X;
                y -= renderBounds.Y;
            }

            if (text == null)
                return;
            //0->left
            //1->center
            //2->right
            try
            {

                StringFormat format = new StringFormat();
                switch (align)
                {
                    case ContentAlignment.TopLeft:
                        format.LineAlignment = StringAlignment.Near;
                        format.Alignment = StringAlignment.Near;
                        break;
                    case ContentAlignment.TopCenter:
                        format.LineAlignment = StringAlignment.Near;
                        format.Alignment = StringAlignment.Center;
                        break;
                    case ContentAlignment.TopRight:
                        format.LineAlignment = StringAlignment.Near;
                        format.Alignment = StringAlignment.Far;
                        break;

                    case ContentAlignment.MiddleLeft:
                        format.LineAlignment = StringAlignment.Center;
                        format.Alignment = StringAlignment.Near;
                        break;
                    case ContentAlignment.MiddleCenter:
                        format.LineAlignment = StringAlignment.Center;
                        format.Alignment = StringAlignment.Center;
                        break;
                    case ContentAlignment.MiddleRight:
                        format.LineAlignment = StringAlignment.Center;
                        format.Alignment = StringAlignment.Far;
                        break;

                    case ContentAlignment.BottomLeft:
                        format.LineAlignment = StringAlignment.Far;
                        format.Alignment = StringAlignment.Near;
                        break;
                    case ContentAlignment.BottomCenter:
                        format.LineAlignment = StringAlignment.Far;
                        format.Alignment = StringAlignment.Center;
                        break;
                    case ContentAlignment.BottomRight:
                        format.LineAlignment = StringAlignment.Far;
                        format.Alignment = StringAlignment.Far;
                        break;
                }
                g.DrawString(text, font, new SolidBrush(color), new RectangleF((float)(x * mmToPx), (float)(y * mmToPx), (float)(width * mmToPx), (float)(height * mmToPx)), format);

                if (PdfGraphics != null)
                {
                    XStringFormat pdfFormat = new XStringFormat();
                    switch (align)
                    {
                        case ContentAlignment.TopLeft:
                            pdfFormat.LineAlignment = XLineAlignment.Near;
                            pdfFormat.Alignment = XStringAlignment.Near;
                            break;
                        case ContentAlignment.TopCenter:
                            pdfFormat.LineAlignment = XLineAlignment.Near;
                            pdfFormat.Alignment = XStringAlignment.Center;
                            break;
                        case ContentAlignment.TopRight:
                            pdfFormat.LineAlignment = XLineAlignment.Near;
                            pdfFormat.Alignment = XStringAlignment.Far;
                            break;

                        case ContentAlignment.MiddleLeft:
                            pdfFormat.LineAlignment = XLineAlignment.Center;
                            pdfFormat.Alignment = XStringAlignment.Near;
                            break;
                        case ContentAlignment.MiddleCenter:
                            pdfFormat.LineAlignment = XLineAlignment.Center;
                            pdfFormat.Alignment = XStringAlignment.Center;
                            break;
                        case ContentAlignment.MiddleRight:
                            pdfFormat.LineAlignment = XLineAlignment.Center;
                            pdfFormat.Alignment = XStringAlignment.Far;
                            break;

                        case ContentAlignment.BottomLeft:
                            pdfFormat.LineAlignment = XLineAlignment.Far;
                            pdfFormat.Alignment = XStringAlignment.Near;
                            break;
                        case ContentAlignment.BottomCenter:
                            pdfFormat.LineAlignment = XLineAlignment.Far;
                            pdfFormat.Alignment = XStringAlignment.Center;
                            break;
                        case ContentAlignment.BottomRight:
                            pdfFormat.LineAlignment = XLineAlignment.Far;
                            pdfFormat.Alignment = XStringAlignment.Far;
                            break;
                    }


                    XFontStyle style = XFontStyle.Regular;
                    if (font.Style == FontStyle.Bold) style = XFontStyle.Bold;
                    if (font.Style == FontStyle.Italic) style = XFontStyle.Italic;
                    if (font.Style == FontStyle.Underline) style = XFontStyle.Underline;
                    XFont xFont = new XFont(font.Name, font.Size * 1.40 / Draw.mmToPx * mmToPxPdf, style);
                    XBrush brush = new XSolidBrush(XColor.FromArgb(color.ToArgb()));


                    XRect xr = new XRect((float)(x * mmToPxPdf), (float)(y * mmToPxPdf), (float)(width * mmToPxPdf), (float)(height * mmToPxPdf));

                    PdfGraphics.DrawString(text, xFont, brush, xr, pdfFormat);
                }
            }
            catch
            {
            }
        }

        public static void Line(Pen pen, double x1, double y1, double x2, double y2)
        {

            if (false && noFrame)
            {
                x1 -= 10;
                y1 -= 20;
                x2 -= 10;
                y2 -= 20;
            }
            if (renderBounds != null)
            {
                x1 -= renderBounds.X;
                y1 -= renderBounds.Y;
                x2 -= renderBounds.X;
                y2 -= renderBounds.Y;
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
                    XPen xpen = new XPen(XColor.FromArgb(pen.Color.ToArgb()), pen.Width);
                    PdfGraphics.DrawLine(xpen, x1 * mmToPxPdf, y1 * mmToPxPdf, x2 * mmToPxPdf, y2 * mmToPxPdf);
                }
            }
            catch
            {
            }
        }



        public static void Arrow(Pen pen, double x1, double y1, double length, double angle)
        {
            if (false && noFrame)
            {
                x1 -= 10;
                y1 -= 20;
            }
            if (renderBounds != null)
            {
                x1 -= renderBounds.X;
                y1 -= renderBounds.Y;
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

        public static void Polygon(Pen pen, int vertices, double x1, double y1, double diameter, double angle)
        {
            if (false && noFrame)
            {
                x1 -= 10;
                y1 -= 20;
            }
            if (renderBounds != null)
            {
                x1 -= renderBounds.X;
                y1 -= renderBounds.Y;
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
        //public static PointF pointG(Bounds bounds, double angle, double scale)
        //{
        //    PointF p = new PointF();
        //    p.X = (float)(bounds.X + bounds.W + bounds.W * Math.Cos(angle * Math.PI / 180) * scale);
        //    p.Y = (float)(bounds.Y + bounds.H - bounds.H * Math.Sin(angle * Math.PI / 180) * scale);
        //    return p;
        //}
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

        public static void Line(Pen pen, PointF p1, PointF p2)
        {
            Line(pen, p1.X, p1.Y, p2.X, p2.Y);
        }

        public static void Lines(Pen pen, PointF[] lines, bool mm)
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
                    XPen xpen = new XPen(XColor.FromArgb(pen.Color.ToArgb()), pen.Width);
                    XPoint[] points = new XPoint[lines.Length]; //MIGRATE
                    for (int i = 0; i < lines.Length; i++)
                        points[i] = new XPoint(lines[i].X / Draw.mmToPx * Draw.mmToPxPdf, lines[i].Y / Draw.mmToPx * Draw.mmToPxPdf);
                    PdfGraphics.DrawLines(xpen, points);
                }
            }
            catch
            {
            }
        }

        public static void FillPolygon(Brush brush, PointF[] lines, bool mm)
        {
            try
            {
                if (mm)
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        lines[i].Y = lines[i].Y * (float)Draw.mmToPx;
                        lines[i].X = lines[i].X * (float)Draw.mmToPx;

                        if (false && noFrame)
                        {
                            lines[i].X -= 10.0f;
                            lines[i].Y -= 20.0f;
                        }
                        if (renderBounds != null)
                        {
                            lines[i].X -= (float)renderBounds.X;
                            lines[i].Y -= (float)renderBounds.Y;
                        }

                    }
                }



                g.FillPolygon(brush, lines);

                if (PdfGraphics != null)
                {
                   
                    XBrush xbrush = new XSolidBrush(XColor.FromArgb(((SolidBrush)brush).Color.ToArgb()));
                    XPen xpen = new XPen(XColor.FromArgb(Color.Blue.ToArgb()), 5);
                    XPoint[] points = new XPoint[lines.Length]; //MIGRATE
                    for (int i = 0; i < lines.Length; i++)
                        points[i] = new XPoint(lines[i].X / Draw.mmToPx * Draw.mmToPxPdf, lines[i].Y / Draw.mmToPx * Draw.mmToPxPdf);
                    PdfGraphics.DrawPolygon(xbrush, points, XFillMode.Winding);
                    //     PdfGraphics.DrawPolygon(xpen, xbrush, points, XFillMode.Alternate);
                }
            }
            catch
            {
            }
        }


        public static void Ellipse(Color color, PointF p, double w, double h, double penWidth)
        {
            Ellipse(color, p.X, p.Y, w, h, penWidth);
        }

        public static void Ellipse(Color color, double x, double y, double w, double h, double penWidth)
        {
            if (false && noFrame)
            {
                x -= 10;
                y -= 20;
            }
            if (renderBounds != null)
            {
                x -= renderBounds.X;
                y -= renderBounds.Y;
            }


            try
            {
                if (penWidth == 0)
                {
                    SolidBrush brush = new SolidBrush(color);
                    g.FillEllipse(brush, (float)((x - w / 2) * mmToPx), (float)((y - h / 2) * mmToPx), (float)(w * mmToPx), (float)(h * mmToPx));
                }
                else
                {
                    //Pen pen = new Pen(color, penWidth * mmToPx);
                    Pen pen = GetPen(color, penWidth * mmToPx);
                    g.DrawEllipse(pen, (float)((x - w / 2) * mmToPx), (float)((y - h / 2) * mmToPx), (float)(w * mmToPx), (float)(h * mmToPx));
                }

                if (PdfGraphics != null)
                {
                    if (penWidth == 0)
                    {
                        XBrush xbrush = new XSolidBrush(XColor.FromArgb(color.ToArgb()));
                        PdfGraphics.DrawEllipse(xbrush, (x - w / 2) * mmToPxPdf, (y - h / 2) * mmToPxPdf, w * mmToPxPdf, h * mmToPxPdf);
                    }
                    else
                    {
                        XPen xpen = new XPen(XColor.FromArgb(color.ToArgb()), penWidth * mmToPxPdf);
                        PdfGraphics.DrawEllipse(xpen, (x - w / 2) * mmToPxPdf, (y - h / 2) * mmToPxPdf, w * mmToPxPdf, h * mmToPxPdf);
                    }
                }
            }
            catch
            {
            }
        }


        public static void Circle(Color color, PointF p, double diameter, double penWidth)
        {
            Circle(color, p.X, p.Y, diameter, penWidth);
        }

        public static void Circle(Color color, double x, double y, double diameter, double penWidth)
        {
            if (false && noFrame)
            {
                x -= 10;
                y -= 20;
            }
            if (renderBounds != null)
            {
                x -= renderBounds.X;
                y -= renderBounds.Y;
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
                        XBrush xbrush = new XSolidBrush(XColor.FromArgb(color.ToArgb()));
                        PdfGraphics.DrawEllipse(xbrush, (x - diameter / 2) * mmToPxPdf, (y - diameter / 2) * mmToPxPdf, diameter * mmToPxPdf, diameter * mmToPxPdf);
                    }
                    else
                    {
                        XPen xpen = new XPen(XColor.FromArgb(color.ToArgb()), penWidth * mmToPxPdf);
                        PdfGraphics.DrawEllipse(xpen, (x - diameter / 2) * mmToPxPdf, (y - diameter / 2) * mmToPxPdf, diameter * mmToPxPdf, diameter * mmToPxPdf);
                    }
                }
            }
            catch
            {
            }
        }

        public static void FillCircle(SolidBrush brush, double x, double y, double w, double h)
        {
            if (false && noFrame)
            {
                x -= 10;
                y -= 20;
            }
            if (renderBounds != null)
            {
                x -= renderBounds.X;
                y -= renderBounds.Y;
            }

            try
            {
                g.FillEllipse(brush, (float)(x * mmToPx), (float)(y * mmToPx), (float)(w * mmToPx), (float)(h * mmToPx));


                if (PdfGraphics != null)
                {
                    XBrush xbrush = new XSolidBrush(XColor.FromArgb(brush.Color.ToArgb()));
                    PdfGraphics.DrawEllipse(xbrush, x * mmToPxPdf, y * mmToPxPdf, w * mmToPxPdf, h * mmToPxPdf);
                }
            }
            catch
            {
            }



        }

        public static void Circle(Color color, double x, double y, double w, double h, double penWidth)
        {
            if (false && noFrame)
            {
                x -= 10;
                y -= 20;
            }
            if (renderBounds != null)
            {
                x -= renderBounds.X;
                y -= renderBounds.Y;
            }

            try
            {
                Pen pen = GetPen(color, penWidth * mmToPx);
                g.DrawEllipse(pen, (float)(x * mmToPx), (float)(y * mmToPx), (float)(w * mmToPx), (float)(h * mmToPx));


                if (PdfGraphics != null)
                {
                    XPen xpen = new XPen(XColor.FromArgb(color.ToArgb()), penWidth * mmToPxPdf);
                    PdfGraphics.DrawEllipse(xpen, x * mmToPxPdf, y * mmToPxPdf, w * mmToPxPdf, h * mmToPxPdf);
                }
            }
            catch
            {
            }



        }

        public static void Rectangle(Pen pen, double x, double y, double w, double h)
        {
            if (false && noFrame)
            {
                x -= 10;
                y -= 20;
            }
            if (renderBounds != null)
            {
                x -= renderBounds.X;
                y -= renderBounds.Y;
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
                    XPen xpen = new XPen(XColor.FromArgb(pen.Color.ToArgb()), pen.Width * mmToPxPdf);
                    PdfGraphics.DrawRectangle(xpen, x * mmToPxPdf, y * mmToPxPdf, w * mmToPxPdf, h * mmToPxPdf);
                }
            }
            catch
            {
            }
        }

        public static void FillRectangle(SolidBrush brush, double x, double y, double w, double h)
        {
            if (false && noFrame)
            {
                x -= 10;
                y -= 20;
            }
            if (renderBounds != null)
            {
                x -= renderBounds.X;
                y -= renderBounds.Y;
            }

            try
            {
                g.FillRectangle(brush, (float)(x * mmToPx), (float)(y * mmToPx), (float)(w * mmToPx), (float)(h * mmToPx));


                if (PdfGraphics != null)
                {
                    XBrush xbrush = new XSolidBrush(XColor.FromArgb(brush.Color.ToArgb()));
                    PdfGraphics.DrawRectangle(xbrush, x * mmToPxPdf, y * mmToPxPdf, w * mmToPxPdf, h * mmToPxPdf);
                }
            }
            catch
            {
            }
        }


        public static void Image(Image image, double mmX1, double mmY1, double mmX2, double mmY2)
        {
            if (image == null)
                return;

            if (false && noFrame)
            {
                mmX1 -= 10;
                mmY1 -= 20;
                mmX2 -= 10;
                mmY2 -= 20;
            }
            if (renderBounds != null)
            {
                mmX1 -= renderBounds.X;
                mmY1 -= renderBounds.Y;
                mmX2 -= renderBounds.X;
                mmY2 -= renderBounds.Y;
            }


            try
            {
                double ratioX = image.PhysicalDimension.Width / (mmX2 - mmX1);
                double ratioY = image.PhysicalDimension.Height / (mmY2 - mmY1);
                double ratio = Math.Max(ratioX, ratioY);
                double mmW = image.PhysicalDimension.Width / ratio;
                double mmH = image.PhysicalDimension.Height / ratio;
                double mmHReduction = (mmY2 - mmY1) - mmH;
                double mmWReduction = (mmX2 - mmX1) - mmW;

                Bitmap bitmap3 = new Bitmap(image);

                //ImageFlags.

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
                        bitmap.SetResolution((float)200, (float)200);
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            g.DrawImage(metafile, 0, 0, (float)wi, (float)he);
                            g.Dispose();
                        }

                        //@SCAN   XImage xImage = XImage.FromGdiPlusImage(bitmap);
                        //@SCAN   PdfGraphics.DrawImage(xImage, new XRect((mmX1 + (mmWReduction / 2)) * mmToPxPdf, (mmY1 + (mmHReduction / 2)) * mmToPxPdf, mmW * mmToPxPdf, mmH * mmToPxPdf));
                    }
                    else
                    {
                        //@SCAN   XImage xImage = XImage.FromGdiPlusImage(new Bitmap(image));
                        //@SCAN   PdfGraphics.DrawImage(xImage, new XRect((mmX1 + (mmWReduction / 2)) * mmToPxPdf, (mmY1 + (mmHReduction / 2)) * mmToPxPdf, mmW * mmToPxPdf, mmH * mmToPxPdf));
                    }
                }
            }
            catch
            { }
        }


        public static void SetClip(double x, double y, double w, double h)
        {
            g.Clip = new Region(new RectangleF((float)(x * mmToPx), (float)(y * mmToPx), (float)(w * mmToPx), (float)(h * mmToPx)));
        }
        public static void ClearClip()
        {
            g.Clip = new Region();
        }

        public static Size ResizeKeepAspect(Size src, int maxWidth, int maxHeight, bool enlarge = false)
        {
            maxWidth = enlarge ? maxWidth : Math.Min(maxWidth, src.Width);
            maxHeight = enlarge ? maxHeight : Math.Min(maxHeight, src.Height);
            decimal rnd = Math.Min(maxWidth / (decimal)src.Width, maxHeight / (decimal)src.Height);
            return new Size((int)Math.Round(src.Width * rnd), (int)Math.Round(src.Height * rnd));
        }

        public static Image ResizeImage(Image image, int maxWidth, int maxHeight)
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

        public static Dictionary<string, object> Resources = new Dictionary<string, object>();





        public static string ImageToBase64(Image image, System.Drawing.Imaging.ImageFormat format)
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

        public static Image Base64ToImage(string base64String)
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


        public static string ImageToBase64(string fileName)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                System.Drawing.Image image = null;

                if (fileName.ToUpper().EndsWith(".PNG") ||
                    fileName.ToUpper().EndsWith(".JPG") ||
                    fileName.ToUpper().EndsWith(".GIF") ||
                    fileName.ToUpper().EndsWith(".BMP"))
                {
                    image = System.Drawing.Image.FromFile(fileName);

                    Image rImage = ResizeImage(image, 600, 600);

                    return ImageToBase64(rImage, System.Drawing.Imaging.ImageFormat.Png);
                }
                if (fileName.ToUpper().EndsWith(".WMF"))
                {
                    //                    System.Drawing.Imaging.Metafile metafile = new System.Drawing.Imaging.Metafile(file);
                    //            oImage item = new oImage("", "Image");
                    //                  TypeConverter imageConverter = TypeDescriptor.GetConverter(metafile.GetType());
                    //  item.Data = Convert.ToBase64String(File.ReadAllBytes(file));
                }
                return null;
            }
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
