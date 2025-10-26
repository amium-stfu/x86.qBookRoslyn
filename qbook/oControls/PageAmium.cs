using QB.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;

namespace qbook
{
    public class PageAmium : oItem
    {

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
           IntPtr pdv, [System.Runtime.InteropServices.In] ref uint pcFonts);
        private static PrivateFontCollection fonts = new PrivateFontCollection();

        static bool fontInit = false;


        public static Bitmap GetImageByName(string imageName)
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            string resourceName = asm.GetName().Name + ".Properties.Resources";
            var rm = new System.Resources.ResourceManager(resourceName, asm);
            return (Bitmap)rm.GetObject(imageName);

        }

        public static void AmiumDefault(int Width, int Height, bool frame, string format)
        {
            /*
            try
            {
                if (false)//!fontInit)
                {
                    // specify embedded resource name
                    string resource = "ft4.ttf";

                    // receive resource stream
                    Stream fontStream = (new StreamReader(resource)).BaseStream;

                    // create an unsafe memory block for the font data
                    System.IntPtr data = Marshal.AllocCoTaskMem((int)fontStream.Length);

                    // create a buffer to read in to
                    byte[] fontdata = new byte[fontStream.Length];

                    // read the font data from the resource
                    fontStream.Read(fontdata, 0, (int)fontStream.Length);

                    // copy the bytes to the unsafe memory block
                    Marshal.Copy(fontdata, 0, data, (int)fontStream.Length);

                    // pass the font to the font collection
                    fonts.AddMemoryFont(data, (int)fontStream.Length);

                    // close the resource stream
                    fontStream.Close();

                    // free up the unsafe memory
                    Marshal.FreeCoTaskMem(data);

                    // fontLogo = new Font(fonts.Families[0], 9.5f);

                    fontInit = true;
                }
            }
            catch
            { 
            }
            */
            Draw.g.DrawRectangle(Pens.LightSlateGray, 0, 0, Width - 1, Height - 1);
            oTag.rCursorX = Draw.Width - qbook.Core.SelectedPage.rItemWidth;
            oTag.lCursorX = qbook.Core.SelectedPage.lItemWidth;

            oTag.lCursor[0] = 30;
            oTag.lCursor[1] = 100;
            oTag.lCursor[2] = 150;
            oTag.rCursor[0] = 30;
            oTag.rCursor[1] = 100;
            oTag.rCursor[2] = 150;

            if (Draw.renderBounds != null)
            {
                Draw.mmToPx = Width / Draw.renderBounds.W;
            }
            else
            {
                Draw.mmToPx = Width / Draw.Width;
            }

            string dFont = "SegoeUI";//"Calibri"

            string ff = "Cascadia Mono";
            ff = "iA Writer Mono S";
            ff = "Monoid";
            ff = "Fira Code Retina";

            ff = "Hack";
            ff = "iA Writer Mono S";
            ff = "Cascadia Mono";

            Draw.fontHeader1 = new Font(dFont, (float)Draw.mmToPx * 6.0f, FontStyle.Regular);
            Draw.fontHeader2 = new Font(dFont, (float)Draw.mmToPx * 5.0f, FontStyle.Regular);
            Draw.fontHeader3 = new Font(dFont, (float)Draw.mmToPx * 4.0f, FontStyle.Regular);
            Draw.fontText = new Font(dFont, (float)Draw.mmToPx * 3.0f, FontStyle.Regular);
            Draw.fontHeader1Fixed = new Font(ff, (float)Draw.mmToPx * 6.0f, FontStyle.Regular);
            Draw.fontHeader2Fixed = new Font(ff, (float)Draw.mmToPx * 5.0f, FontStyle.Regular);
            Draw.fontHeader3Fixed = new Font(ff, (float)Draw.mmToPx * 4.0f, FontStyle.Regular);
            Draw.fontTextFixed = new Font(ff, (float)Draw.mmToPx * 3.2f, FontStyle.Regular);
            Draw.fontFootnote = new Font(dFont, (float)Draw.mmToPx * 2.6f, FontStyle.Regular);
            Draw.fontFootnoteFixed = new Font(ff, (float)Draw.mmToPx * 2.6f, FontStyle.Regular);
            if (QB.Book.CompactView)
                Draw.fontFootnoteFixed = new Font(ff, (float)Draw.mmToPx * 2.2f, FontStyle.Regular);
            Draw.fontTerminalFixed = new Font(ff, (float)Draw.mmToPx * 2.0f, FontStyle.Regular);
            Draw.fontTerminal = new Font("SegoeUI", (float)Draw.mmToPx * 2.0f, FontStyle.Regular);

            Draw.pxHeight = Height;
            Draw.pxWidth = Width;

            //MIGRATION: add
            /*
            Main.Qb.Widget.Draw.fontFootnote = Draw.fontHeader1;
            Main.Qb.Widget.Draw.fontHeader2=Draw.fontHeader2;
            Main.Qb.Widget.Draw.fontHeader3=Draw.fontHeader3;
            Main.Qb.Widget.Draw.fontText=Draw.fontText;
            Main.Qb.Widget.Draw.fontHeader1Fixed=Draw.fontHeader1Fixed;
            Main.Qb.Widget.Draw.fontHeader2Fixed=Draw.fontHeader2Fixed;
            Main.Qb.Widget.Draw.fontHeader3Fixed=Draw.fontHeader3Fixed;
            Main.Qb.Widget.Draw.fontTextFixed=Draw.fontTextFixed;
            Main.Qb.Widget.Draw.fontFootnote=Draw.fontFootnote;
            Main.Qb.Widget.Draw.fontFootnoteFixed =Draw.fontFootnoteFixed;
            Main.Qb.Widget.Draw.fontTerminalFixed =Draw.fontTerminalFixed;
            Main.Qb.Widget.Draw.pxHeight = Draw.pxHeight;
            Main.Qb.Widget.Draw.pxWidth = Draw.pxWidth;
            Main.Qb.Widget.Draw.mmToPx = Draw.mmToPx;
            */

            int formatOffset = 0;

            switch(format)
            {
                case "A4":
                
                    formatOffset = 0;
                    break;
                case "16/9":
              
                    formatOffset = -43;
                    break;
                case "16/10":
                 
                    formatOffset = -24;
                    break;
                default:
              
                    formatOffset = 0;
                    break;
            }

            Draw.Line(new Pen(Color.Black, 1), 10, 19, Draw.Width - 7, 19);
            Draw.Line(new Pen(Color.Black, 1), 10, Draw.Height + formatOffset - 9, Draw.Width - 7, Draw.Height + formatOffset - 9);



            // Font fontLogo;

            if (!frame)
                return;

            /*
                try
            {
                if (fonts.Families.Length > 0)
                    fontLogo = new Font(fonts.Families[0], Draw.mmToPx * 8f, FontStyle.Regular);
                else
                    fontLogo = new Font("ft4", Draw.mmToPx * 8f, FontStyle.Regular);
            }
            catch
            {
                fontLogo = new Font("ft4", Draw.mmToPx * 8f, FontStyle.Regular);
            }
            */
            SizeF size;// = Draw.g.MeasureString("amium", fontLogo);
            //  Draw.Text("amium", Draw.Width - size.Width / Draw.mmToPx - 5, 7.0f, 0,fontLogo, Color.Black);

            object o = Properties.Resources.ResourceManager.GetObject("amium");
            if (o is Bitmap)
            {
                // Image i = Draw.ResizeImage(o as Bitmap, 100, 100 * (o as Bitmap).Height / (o as Bitmap).Width);
                Draw.Image(o as Bitmap, Draw.Width - 46, 5.0f, Draw.Width - 7, 20);
            }

            oItem parent = (qbook.Core.ThisBook.Main.GetParent(qbook.Core.SelectedPage));

            string headerText = qbook.Core.SelectedPage.TextL;
            if ((headerText == null) || (headerText == "") || (headerText == "#"))
            {
                headerText = qbook.Core.SelectedPage.Name;
            }
            if (QB.Book.PageHeader != null)
                headerText = QB.Book.PageHeader;


            Draw.Text(((parent != qbook.Core.ThisBook.Main) && (parent != null) ? parent.TextL + "/" : "") + headerText, 10.0 - 1.0, 10f, 0, Draw.fontHeader2, Color.Black, ContentAlignment.MiddleLeft);

            Font fontSection = Draw.fontHeader3;

            //  if (Studio.SelectedPage.Section.Length > 9)
            //    fontSection = Draw.fontText;
            size = Draw.g.MeasureString(qbook.Core.SelectedPage.Section, fontSection);


            //Draw.Text(Page.Section, Draw.mmWidth / 2 - (size.Width / 2) / Draw.mmToPx, Draw.mmHeight - 10f, fontSection, Color.Black);

            Draw.Text(qbook.Core.SelectedPage.Section, 10, Draw.Height - 9f, 0, fontSection, Color.Black);

            //   Draw.Line(new Pen(Color.Black, 1), Draw.mmWidth - 40, Draw.mmHeight - 10, Draw.mmWidth - 40, Draw.mmHeight - 1);
            //  Draw.Line(new Pen(Color.Black, 1), Draw.mmWidth - 40, Draw.mmHeight - 1, Draw.mmWidth - 8, Draw.mmHeight - 1);
            //  Draw.Line(new Pen(Color.Black, 1), Draw.mmWidth - 8, Draw.mmHeight - 1, Draw.mmWidth - 8, Draw.mmHeight - 10);

            string fn = "";

            if (qbook.Core.ThisBook.Filename.Contains("\\") || qbook.Core.ThisBook.Filename.Contains("/"))
                //fn = Main.Qb.File.Filename.Substring(Main.Qb.File.Filename.LastIndexOf('\\'));//, 10, Draw.mmHeight - 10, Draw.fontFootnote, Color.Black);
                fn = Path.GetFileNameWithoutExtension(qbook.Core.ThisBook.Filename);//, 10, Draw.mmHeight - 10, Draw.fontFootnote, Color.Black);
            else
                fn = qbook.Core.ThisBook.Filename;//, 10, Draw.mmHeight - 10, Draw.fontFootnote, Color.Black);;


            List<oItem> pages = qbook.Core.ActualMain.Objects.Where(item => item is oPage).ToList();

            string l1 = fn + " " + qbook.Core.PageNumber(qbook.Core.SelectedPage) + "/" + pages.Count;
            //fn += ".qbook";
            //size = Draw.g.MeasureString(fn, Draw.fontFootnote);
            //Draw.Text(fn, Draw.Width - 6 - (size.Width / Draw.mmToPx), Draw.Height - 9, 0, Draw.fontFootnote, Color.Black);


            

           

                Draw.Text(fn, Draw.Width - 5.0, Draw.Height + formatOffset - 9, 0, Draw.fontFootnote, Color.Black, ContentAlignment.MiddleRight);
            

            l1 = qbook.Core.PageNumber(qbook.Core.SelectedPage) + "/" + pages.Count;
            //size = Draw.g.MeasureString(l1, Draw.fontFootnote);
            //Draw.Text(l1, Draw.Width - 6 - (size.Width / Draw.mmToPx), Draw.Height - 5, 0, Draw.fontFootnote, Color.Black);
            Draw.Text(l1, Draw.Width - 6.0, Draw.Height + formatOffset - 5, 0, Draw.fontFootnote, Color.Black, ContentAlignment.MiddleRight);
        }
    }
}
