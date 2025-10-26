using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Security;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace QB
{

    public class PdfWriter : Item
    {
        public XGraphics PdfGraphics { get; set; }
        public string Filename { get; set; }
        public PdfDocument PdfDocument { get; set; }

        public PdfWriter(string name) : base(name, null)
        {
        }

        public void Open(string title)
        {

            PdfDocument = new PdfDocument();
            PdfDocument.Info.Title = title;
        }




        public void Save(string filename, bool protect)
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
                if (!filename.ToUpper().EndsWith(".PDF")) filename += ".pdf";
                //  filename = 

                PdfDocument.Save(Path.Combine(QB.Root.ActiveQbook.DataDirectory, filename));
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("PDF File open!!");
                //  throw ex;
            }
        }

        public void Show()
        {
            try
            {
                Process.Start(Filename);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public int PdfPageCount = 0;
        public void NewPage()
        {

            try
            {
                if (PdfDocument == null) return;
                PdfPageCount++;
                PdfPage PdfPage = PdfDocument.AddPage();
                PdfPage.Size = PageSize.A4;
                PdfPage.Orientation = PageOrientation.Portrait;

                PdfGraphics = XGraphics.FromPdfPage(PdfPage);
            }
            catch
            {
            }
        }

        bool noFrame = false;
        public Rectangle Bounds = null;
        double _mmToPx = 1f;
        public double mmToPxPdf = 2.835f; //@72dpi, 595dots, 21cm)


        double nextY = 0f;
        double nextX = 0f;
        double nextW = 0f;
        double nextFontSize = 2.5;
        Color nextForeColor = Color.Black;
        string nextFontName = "Calibri";
        System.Drawing.ContentAlignment nextTextAlign = System.Drawing.ContentAlignment.TopLeft;

        public double lineFeed = 5;

        public void Text(string text, string fontName = "Calibri", double size = double.NaN, object color = null, double x = double.NaN, double y = double.NaN, double w = double.NaN, object align = null)
        {
            if (double.IsNaN(x))
                x = nextX;
            if (double.IsNaN(y))
                y = nextY + lineFeed;
            if (double.IsNaN(w))
                w = nextW;

            if (double.IsNaN(size))
                size = nextFontSize;
            if (color == null)
                color = nextForeColor;

            if (color is string)
                nextForeColor = ColorTranslator.FromHtml(color as string);
            if (color is Color)
                nextForeColor = (Color)color;


            if (align == null)
                align = nextTextAlign;

            if (align is string)
            {
                string ta = ((string)align).ToUpper().Trim();
                if ((ta == "LEFT") || (ta == "L"))
                    align = System.Drawing.ContentAlignment.TopLeft;
                if ((ta == "MIDDLE") || (ta == "M") || (ta == "CENTER") || (ta == "C"))
                    align = System.Drawing.ContentAlignment.MiddleCenter;
                if ((ta == "RIGHT") || (ta == "R"))
                    align = System.Drawing.ContentAlignment.TopRight;
            }

            if (align is System.Drawing.ContentAlignment)
                nextTextAlign = (System.Drawing.ContentAlignment)align;

            Text(text, x, y, w, new Font(fontName, (float)size), nextForeColor, System.Drawing.ContentAlignment.TopLeft);

            nextFontName = fontName;
            //    nextFontSize = size;
            nextY = y;
            nextX = x;
            nextW = w;

        }
        public void Text(string text, double x, double y, double width, Font font, Color color, System.Drawing.ContentAlignment align)
        {
            if (Bounds != null)
            {
                x += Bounds.X;
                y += Bounds.Y;
            }

            if (text == null)
                return;
            //0->left
            //1->center
            //2->right
            try
            {
                if (PdfGraphics != null)
                {
                    XFontStyle style = XFontStyle.Regular;
                    if (font.Style == FontStyle.Bold) style = XFontStyle.Bold;
                    if (font.Style == FontStyle.Italic) style = XFontStyle.Italic;
                    if (font.Style == FontStyle.Underline) style = XFontStyle.Underline;
                    XFont xFont = new XFont(font.Name, font.Size * 1.40 * mmToPxPdf, style);
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


        Color nextLineColor = Color.Black;
        double nextLineWidth = 0.2;
        double nextLineX = 0;
        double nextLineY = 0;
        public void Line(object color = null, double width = double.NaN, double x1 = double.NaN, double y1 = double.NaN, double x2 = double.NaN, double y2 = double.NaN)
        {
            if (color == null)
                color = nextLineColor;

            if (color is string)
                nextLineColor = ColorTranslator.FromHtml(color as string);
            if (color is Color)
                nextLineColor = (Color)color;

            if (double.IsNaN(width))
                width = nextLineWidth;
            nextLineWidth = width;

            if (double.IsNaN(x1))
                x1 = nextLineX;
            if (double.IsNaN(y1))
                y1 = nextLineY;

            nextLineX = x2;
            nextLineY = y2;


            if (Bounds != null)
            {
                x1 += Bounds.X;
                y1 += Bounds.Y;
                x2 += Bounds.X;
                y2 += Bounds.Y;
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
                if (PdfGraphics != null)
                {
                    XPen xpen = new XPen(XColor.FromArgb(nextLineColor.ToArgb()), nextLineWidth * mmToPxPdf);
                    PdfGraphics.DrawLine(xpen, x1 * mmToPxPdf, y1 * mmToPxPdf, x2 * mmToPxPdf, y2 * mmToPxPdf);
                }
            }
            catch
            {
            }
        }

        Color nextRectColor = Color.Black;
        double nextRectWidth = 0.2;
        public void Rectangle(object color = null, double width = double.NaN, double x = 0, double y = 0, double w = 10, double h = 10)
        {
            if (color == null)
                color = nextRectColor;

            if (color is string)
                nextRectColor = ColorTranslator.FromHtml(color as string);
            if (color is Color)
                nextRectColor = (Color)color;

            if (double.IsNaN(width))
                width = nextRectWidth;
            nextRectWidth = width;

            if (Bounds != null)
            {
                x += Bounds.X;
                y += Bounds.Y;
            }

            try
            {
                if (double.IsNaN(x))
                    return;
                if (double.IsNaN(y))
                    return;

                if (PdfGraphics != null)
                {
                    XPen xpen = new XPen(XColor.FromArgb(nextRectColor.ToArgb()), nextRectWidth * mmToPxPdf);
                    PdfGraphics.DrawRectangle(xpen, x * mmToPxPdf, y * mmToPxPdf, w * mmToPxPdf, h * mmToPxPdf);
                }
            }
            catch
            {
            }
        }

        Color nextFillColor = Color.Black;
        public void FillRectangle(object color = null, double x = 0, double y = 0, double w = 10, double h = 10)
        {

            if (color == null)
                color = nextRectColor;

            if (color is string)
                nextFillColor = ColorTranslator.FromHtml(color as string);
            if (color is Color)
                nextFillColor = (Color)color;


            if (Bounds != null)
            {
                x += Bounds.X;
                y += Bounds.Y;
            }

            try
            {
                if (PdfGraphics != null)
                {
                    XBrush xbrush = new XSolidBrush(XColor.FromArgb(nextFillColor.ToArgb()));
                    PdfGraphics.DrawRectangle(xbrush, x * mmToPxPdf, y * mmToPxPdf, w * mmToPxPdf, h * mmToPxPdf);
                }
            }
            catch
            {
            }
        }

        public void Table(object table, double x = 0, double y = 0, double w = 20, double h = double.NaN)
        {

            if (!(table is Table))
                return;

            if (double.IsNaN(h))
                h = lineFeed;

            Table _table = (Table)table;
            if (Bounds != null)
            {
                x += Bounds.X;
                y += Bounds.Y;
            }
            foreach (var row in _table.Rows)
            {
                //if (row.Key.StartsWith("$$$")) //do NOT export virtual rows like '$$$header'
                //    continue;

                //string line = "";
                //if (addRowId)
                //    line = row.Key + csvDelimiter;
                double xr = x;
                foreach (var cell in row.Value.Cells)
                {
                    try
                    {
                        Text(cell.Value.Value?.ToString(), x: xr, y: y, w: w);
                    }
                    catch
                    {
                        Text(".", x: xr, y: y, w: w);
                    }
                    // object v = cell.Value;
                    // if (v == null) v = "#";
                    // else v = cell.Value.Value?.ToString();

                    // Text(v.ToString(), x: xr, y: y, w: w);
                    xr += w;
                    //  line += cell.Value.Value?.ToString() + csvDelimiter;
                }
                y += h;
                // lines.Add(line.TrimEnd(csvDelimiter));
                // if (maxRows > 0 && lines.Count >= maxRows)
                //     break;
            }


        }

        public void Image(object image, double x = 0, double y = 0, double w = 10, double h = 10)
        {
            Image _image = null;
            if (image is Image)
                _image = (Image)image;

            if (image is string)
            {
                _image = new System.Drawing.Bitmap(Path.Combine(QB.Root.ActiveQbook.DataDirectory, image as string));
            }


            if (Bounds != null)
            {
                x += Bounds.X;
                y += Bounds.Y;
            }

            try
            {
                double ratioX = _image.PhysicalDimension.Width / (w);
                double ratioY = _image.PhysicalDimension.Height / (h);
                double ratio = Math.Max(ratioX, ratioY);
                double mmW = _image.PhysicalDimension.Width / ratio;
                double mmH = _image.PhysicalDimension.Height / ratio;
                double mmHReduction = (h) - mmH;
                double mmWReduction = (w) - mmW;

                Bitmap bitmap3 = new Bitmap(_image);


                if (PdfGraphics != null)
                {

                    Bitmap bitmap = null;

                    Metafile metafile = _image as Metafile;

                    if (metafile != null)
                    {
                        int wi = (int)_image.PhysicalDimension.Width;
                        int he = (int)_image.PhysicalDimension.Height;

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

                        //@SCAN  XImage xImage = XImage.FromGdiPlusImage(bitmap);
                        //@SCAN  PdfGraphics.DrawImage(xImage, new XRect((x + (mmWReduction / 2)) * mmToPxPdf, (y + (mmHReduction / 2)) * mmToPxPdf, mmW * mmToPxPdf, mmH * mmToPxPdf));
                    }
                    else
                    {
                        //@SCAN  XImage xImage = XImage.FromGdiPlusImage(new Bitmap(_image));
                        //@SCAN  PdfGraphics.DrawImage(xImage, new XRect((x + (mmWReduction / 2)) * mmToPxPdf, (y + (mmHReduction / 2)) * mmToPxPdf, mmW * mmToPxPdf, mmH * mmToPxPdf));
                    }
                }
            }
            catch
            { }
        }

        public void Clear()
        {

        }

        public void Add(double value)
        {

        }


    }
}