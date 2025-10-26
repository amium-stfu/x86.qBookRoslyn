using QB; //qbookCsScript
using QB.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace qbook
{
    public partial class PageControl : UserControl
    {
        public PageControl()
        {

            InitializeComponent();

            if (!DesignMode)
            {
                this.MouseWheel += new MouseEventHandler(this.PageControl_MouseWheel);
                this.AllowDrop = true;
                this.DragEnter += new DragEventHandler(Form1_DragEnter);
                this.DragDrop += new DragEventHandler(Form1_DragDrop);
                Draw.MmToPxChangedEvent += Draw_MmToPxChangedEvent;
            }

            DoubleBuffered = true;

        }

        public oPage Page;
        bool _Popout = false;
        public bool Popout
        {
            get { return _Popout; }
            set
            {
                _Popout = value;
                if (_Popout)
                    //RenderBounds = new QB.Rectangle(10, 20, Draw.Width - 10 - 10, Draw.Height - 20 - 10);
                    RenderBounds = new QB.Rectangle(10, 20, 280.0, 180.0);
            }
        }

        public bool Frame = true;
        public oPage _DialogPage = null;
        public oPage DialogPage
        {
            get { return _DialogPage; }
            set
            {
                _DialogPage = value;
            }
        }
        public QB.Rectangle RenderBounds = null;

        private void Draw_MmToPxChangedEvent(Draw.MmToPxChangedEventArgs e)
        {
            DoSizeChanged(new EventArgs());
        }

        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (!Core.ThisBook.DesignMode)
            {
                QB.Book.DroppedItems.AddRange((string[])e.Data.GetData(DataFormats.FileDrop));
            //    System.IO.Directory.GetDirectories
            //    QB.Logger.Log("import");

                return;
            }
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                if (file.ToUpper().EndsWith(".QBOOK"))
                {
                    oPage item = new oPage("", "Page");
                    if (!Directory.Exists(Core.ThisBook.Filename))
                        Directory.CreateDirectory(Core.ThisBook.Filename);
                    item.Url = file;
                    Core.ActualMain.Add(item);
                }


                if (file.ToUpper().EndsWith(".PNG") ||
                    file.ToUpper().EndsWith(".JPG") ||
                    file.ToUpper().EndsWith(".GIF") ||
                    file.ToUpper().EndsWith(".BMP"))
                {
                    Core.ThisBook.Modified = true;
                    Image imageRaw = Image.FromFile(file);
                    Image image = Image.FromFile(file);
                    using (Graphics g = Graphics.FromImage(image))
                    {
                        g.Clear(Color.White);
                        g.DrawImage(imageRaw, 0, 0, imageRaw.Width, imageRaw.Height);
                    }
                    DialogImage(image);
                }
                if (file.ToUpper().EndsWith(".WMF"))
                {
                    Core.ThisBook.Modified = true;
                    //                    System.Drawing.Imaging.Metafile metafile = new System.Drawing.Imaging.Metafile(file);
                    oImage item = new oImage("", "Image");
                    //                  TypeConverter imageConverter = TypeDescriptor.GetConverter(metafile.GetType());
                    item.Data = Convert.ToBase64String(File.ReadAllBytes(file));
                    Core.SelectedLayer.Add(item);
                }

                //TODO MIGRATION-old
                /*
                if (file.EndsWith(".csv"))
                {
                    Qb.Book.Modified = true;

                    x_oGrid item = new x_oGrid("", "Grid");
                    item.Bounds.X = 10;
                    item.Bounds.Y = 10;// e.Y / Draw.mmToPx;
                    item.Bounds.W = 60;
                    item.Bounds.H = 50;
                    item.Data = new StreamReader(file).ReadToEnd();
                    Qb.SelectedLayer.Add(item);
                }
                */
            }
        }


        public void DialogImage(Image image)
        {
            ImageForm form = new ImageForm(image);
            form.Done += ImportImage;
            form.Show();
        }

        private void ImportImage(Image image)
        {
            oImage item = new oImage("", "Image");

            //item.Data = Draw.ImageToBase64(Draw.ResizeImage(image, 1200, 1200), System.Drawing.Imaging.ImageFormat.Jpeg);
            int maxPixel = 1200;
            if (image.Width > image.Height)
                item.Data = Draw.ImageToBase64(Draw.ResizeImage(image, maxPixel, maxPixel * image.Height / image.Width), System.Drawing.Imaging.ImageFormat.Jpeg);
            else
                item.Data = Draw.ImageToBase64(Draw.ResizeImage(image, maxPixel * image.Width / image.Height, maxPixel), System.Drawing.Imaging.ImageFormat.Jpeg);
            Core.SelectedLayer.Add(item);
        }



        public void Images()
        {
            List<oControl> visibleImages = new List<oControl>();
            visibleImages.AddRange(Page.Objects.OfType<oImage>().Where(item => item.Visible));

            foreach (oLayer layer in Page.Objects.Where(item => item is oLayer).ToList())
                visibleImages.AddRange(layer.Objects.OfType<oImage>().Where(item => item.Visible));

            int i = 0;
            foreach (oImage image in visibleImages)
            {
                Image img = Draw.Base64ToImage(image.Data);
                if (img == null)
                    continue;

                double w = (Draw.Width + 3) - Page.rItemWidth - Page.lItemWidth - 20;
                double x = Page.lItemWidth + 10;

                // if (Studio.Page.Csv != null)
                {
                    //    Draw.Image(img, x + ((i) * w / 3) + 0.2f, Draw.Height - 50, x + ((i) * w / 3) + w / 3 - 0.2f, Draw.Height - 12);
                }
                // else
                {

                    if (i == 0)
                    {
                        if (image.Bounds.H <= 1)
                        {
                            image.Bounds.X = x;
                            image.Bounds.Y = 20;
                            image.Bounds.W = w;
                            image.Bounds.H = Draw.Height - 55 - image.Bounds.Y;
                        }
                        //                        Draw.Image(img, x, 20, x + w, Draw.Height - 50);
                    }
                    else
                    {
                        if (image.Bounds.H <= 1)
                        {
                            image.Bounds.X = x + ((i - 1) * w / 3) + 0.2f;
                            image.Bounds.Y = Draw.Height - 50;
                            image.Bounds.W = w / 3 - 0.2f;
                            image.Bounds.H = Draw.Height - 12 - image.Bounds.Y;
                        }
                        //                      Draw.Image(img, x + ((i - 1) * w / 3) + 0.2f, Draw.Height - 50, x + ((i - 1) * w / 3) + w / 3 - 0.2f, Draw.Height - 12);
                    }
                }
                i++;
                if (i > 3)
                    break;
            }
        }

        private static bool isoTag(oItem item)
        {
            return item.GetType() == typeof(oTag);
        }

        public void Render()
        {
            if (!Popout)
                Page = Core.SelectedPage;

            if (DialogPage != null)
                Page = DialogPage;

            if (Page == null) //HALE
                return;

            //@stfu 2025-06-07 pageFormat

            //switch (Page.Format)
            //{
            //    case "16/9":
                    
            //        Height = (int)(Width / 16 * 9);
            //        break;

            //    case "16/10":
            //        Height = (int)(Width / 16 * 10);
            //        break;
            //}
            //

            PageAmium.AmiumDefault(Width, Height, Frame,Page.Format);
            //Chart_();

            Images();
            foreach (oLayer layer in Page.Objects.OfType<oLayer>())
                if (layer.Visible)
                    foreach (oControl item in layer.Objects.OfType<oControl>())
                    {
                        if (item is oTag)
                        {

                        }
                        else
                        {
                            item.Marker = layer.TextL;
                            item.Render();
                        }
                    }


            oLayer mainLayer = Page.Objects.OfType<oLayer>().FirstOrDefault(l => l.Name == "Main");
            if (mainLayer != null)
            {
                // List<oItem> list = new List<oItem>();
                foreach (oTag oTag in mainLayer.Objects.OfType<oTag>())
                {
                    oLayer tagLayer = Page.Objects.OfType<oLayer>().FirstOrDefault(l => l.Name == "Tags");
                    if (tagLayer == null)
                    {
                        oLayer layer = new oLayer("Tags", "");
                        Page.Objects.Add(layer);
                        tagLayer = layer;
                    }
                    tagLayer.Add(oTag);
                    //   list.Add(oTag);

                }
                mainLayer.Objects.RemoveAll(isoTag);
                //    foreach (oItem item in list)
                //      mainLayer.Remove(item);
            }
            //.FirstOrDefault<oLayer>();/ .Where(l => (l.GetType() == typeof(oLayer)) && ();

            SortedDictionary<double, oTag> tags = new SortedDictionary<double, oTag>();
            foreach (oLayer layer in Page.Objects.OfType<oLayer>())
                if (layer.Visible)
                    foreach (oTag tag in layer.Objects.OfType<oTag>())
                    {
                        tag.Marker = layer.TextL;
                        double key = tag.Bounds.X + tag.Bounds.Y;
                        while (tags.ContainsKey(key))
                            key++;
                        tags.Add(key, tag as oTag);
                    }
            foreach (oTag tag in tags.Values)
            {
                tag.widthFromPage = Page.rItemWidth - 5;
                tag.Render();
            }

            if (Core.ThisBook.Grid > 0)
            {
                for (int x = 10; x <= Draw.Width - 7; x += Core.ThisBook.Grid)
                    Draw.Line(gridLo, x, 20, x, Draw.Height - 10);

                for (int y = 20; y <= Draw.Height - 10; y += Core.ThisBook.Grid)
                    Draw.Line(gridLo, 10, y, Draw.Width - 7, y);

                for (int x = 10; x <= Draw.Width - 7; x += 10)
                {
                    if (x == 70)
                        Draw.Line(gridHi, x, 20, x, Draw.Height - 10);
                    else if (x == 150)
                        Draw.Line(gridHi, x, 20, x, Draw.Height - 10);
                    else if (x == 230)
                        Draw.Line(gridHi, x, 20, x, Draw.Height - 10);
                    else

                        Draw.Line(grideMid, x, 20, x, Draw.Height - 10);
                }
                for (int y = 20; y <= Draw.Height - 10; y += 10)
                    Draw.Line(grideMid, 10, y, Draw.Width - 7, y);
            }

            if (selectOrNew)
                Draw.FillRectangle(Draw.DesignBrush, Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x2 - x1), Math.Abs(y2 - y1));


            Page.Render();
            /*
            if (Page is oUdl)
            {
                int y = 25;
                Udl udl = (Page as oUdl).Udl;
                if (udl != null)
                foreach (UdlModule module in udl.Module.Values)
                {
                    Draw.Text(module.MId+"", 10, y, 200, Draw.fontFootnote, Color.Black, false);
                    y += 6;
                }
            }

            if (Page is oAkc)
            {
                int y = 25;
                    foreach (AkModule module in (Page as oAkc).AkModules)
                    {
                        Draw.Text(module.Name.PadLeft(10) + module.ReadCommand.PadLeft(10) + ("" + module.Read).PadLeft(10)  + module.Response?.PadLeft(10) , 10, y, 200, Draw.fontTextFixed, Color.Black, false);
                        y += 6;
                    }
            }
            */
        }

        Pen gridLo = new Pen(Color.FromArgb(20, Color.LightSlateGray), 0.5f);
        Pen grideMid = new Pen(Color.FromArgb(50, Color.LightSlateGray), 0.5f);
        Pen gridHi = new Pen(Color.FromArgb(80, Color.LightSlateGray), 1f);
        //   SolidBrush sb = new SolidBrush(Color.FromArgb(50, Color.Green));

        double toGrid(double value)
        {
            if (Core.ThisBook.Grid == 0)
                return value;
            return ((int)(value / Core.ThisBook.Grid)) * Core.ThisBook.Grid;
        }

        double MmToPx = 10.0;


        
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Core.ThisBook == null)
                return;

            if (RenderBounds != null)
                MmToPx = Width / Draw.Width * ((Draw.Width - 00) / RenderBounds.W);
            else
                MmToPx = Width / Draw.Width; //Draw.Width = PageA4.With;

            Draw.g = e.Graphics;
            Draw.DesignMode = Core.ThisBook?.DesignMode ?? true;
            base.OnPaint(e);

            if (DesignMode)
                return;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            lock (Core.ThisBook)
            {
                Draw.renderBounds = RenderBounds;
                Render();
            }
        }

        public void DoSizeChanged(EventArgs e)
        {
            OnSizeChanged(e);  //needed for HTML browser (CefSharp)
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            try
            {
                if (!Popout)
                    Page = Core.SelectedPage;

                if (DialogPage != null)
                    Page = DialogPage;
            }
            catch (Exception ex)
            {

            }
            base.OnSizeChanged(e);

            if (Page != null)
            {
                foreach (var htmlItem in Page.HtmlItems)
                {
                    //htmlItem.MyControl.Left = (int)(htmlItem.Bounds.X * Draw.mmToPx);
                    //htmlItem.MyControl.Top = (int)(htmlItem.Bounds.Y * Draw.mmToPx);
                    //htmlItem.MyControl.Width = (int)(htmlItem.Bounds.W * Draw.mmToPx);
                    //htmlItem.MyControl.Height = (int)(htmlItem.Bounds.H * Draw.mmToPx);
                    //htmlItem.MyControl.SetBrowserZoom((double)this.Width / (double)Draw.Width);

                    htmlItem.MyControl?.Resize();
                }
            }
        }

        private void PageControl_Paint(object sender, PaintEventArgs e)
        {
            return; //handled in OnPaint()

            if (DesignMode)
                return;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            Draw.g = e.Graphics;
            Draw.DesignMode = Core.ThisBook.DesignMode;

            Render();
        }

        oControl selectedItem = null;
        QB.Controls.Control mouseDownControl = null;
        bool moveOrEdit = false;
        bool edit = false;
        bool resize = false;
        bool settings = false;
        bool selectOrNew = false;
        bool scratch = false;
        bool button = false;
        bool control = false;
        bool moved = false;
        double x1;
        double y1;
        double x2;
        double y2;
        double xObject;
        double yObject;
        double wObject;
        double hObject;

        private void PageControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Popout)
                Page = Core.SelectedPage;

            if (DialogPage != null)
                Page = DialogPage;

            if (RenderBounds != null)
            {
                x2 = e.Location.X / MmToPx + RenderBounds.X;// + 10;
                y2 = e.Location.Y / MmToPx + RenderBounds.Y;// + 20;
            }
            else
            {
                x2 = e.Location.X / MmToPx;
                y2 = e.Location.Y / MmToPx;
            }
            if (Math.Abs(x2 - x1) > 2)
                moved = true;
            if (Math.Abs(y2 - y1) > 2)
                moved = true;
            if ((selectedItem != null) && (Core.ThisBook.DesignMode || (Core.ThisBook.TagMode && (selectedItem.GetType() == typeof(oTag)))))
            {
                if (scratch)
                {
                    if (moved)
                    {
                        string p = (Math.Round(x1 - selectedItem.Bounds.X, 1) + "|" + Math.Round(y1 - selectedItem.Bounds.Y, 1)).Replace(",", ".").Replace("|", ",");
                        (selectedItem as oScratch).Data += ";" + p;
                        x1 = x2;
                        y1 = y2;
                        Core.ThisBook.Modified = true;
                    }

                }
                if (resize)
                {
                    selectedItem.Bounds.W = toGrid(wObject + (x2 - x1));
                    selectedItem.Bounds.H = toGrid(hObject + (y2 - y1));
                    Core.ThisBook.Modified = true;
                    //selectedItem.ObjectSettings.Modified = true;
                }
                if (moveOrEdit)
                {
                    selectedItem.Bounds.X = toGrid(xObject + (x2 - x1));
                    selectedItem.Bounds.Y = toGrid(yObject + (y2 - y1));
                    Core.ThisBook.Modified = true;
                    //selectedItem.ObjectSettings.Modified = true;
                }
            }

            bool hand = false;
            if (Page != null)
            {
                var controlUnderCursor = Page.GetControlUnderCursor(new PointF((float)x2, (float)y2), true);

                if (controlUnderCursor != null)
                {
                    bool isVisible = controlUnderCursor.Visible;
                    if (controlUnderCursor is Box)
                        isVisible = (controlUnderCursor as Box).IsVisible();

                    if (isVisible)
                        hand = true;
                }

                //HALE: TODO - this no longer works... page,layers,components,controls,??? obsolete!?
                /*
                foreach (oLayer layer in Page.Objects.OfType<oLayer>())
                {
                    foreach (oControl item in layer.Objects.OfType<oControl>())
                    {
                        if (item.Hover(new PointF((float)x2, (float)y2)) )
                            hand = true;
                    }
                }
                */


                Cursor = hand ? Cursors.Hand : Cursors.Default;

                if (button || control)
                {
                    Page.Drag(new PointF((float)x2, (float)y2));
                    foreach (oLayer layer in Page.Objects.OfType<oLayer>())
                        foreach (oControl item in layer.Objects.OfType<oControl>())
                            item.Drag(new PointF((float)x2, (float)y2));
                }

                /*
                if (false) //cursor=hand if above clickable object
                {
                    oControl foundObject = null;
                    foreach (var layer in Page.Objects.OfType<oLayer>())
                    {
                        foundObject = layer.Objects.OfType<oControl>().FirstOrDefault(o => o.Visible && o.Bounds.Contains(x2, y2));
                    }

                    bool isClickableObject = false;
                    if (foundObject is oControl)
                    {
                        //if ((foundObject as oControl).onClick != null)
                        //{
                        //    isClickableObject = true;
                        //}
                    }

                    if (foundObject is oGauge)
                    {
                        //isClickableObject = ((foundObject as oGauge).labels.Any(l => l.onClick != null));
                    }

                    if (isClickableObject)
                        Cursor = Cursors.Hand;
                    else
                        Cursor = Cursors.Default;
                }
                */
            }

            if (Page != null)
            {
                var controlUnderCursor = Page.GetControlUnderCursor(new PointF((float)x2, (float)y2), true);

                if (controlUnderCursor != null)
                {
                    bool isVisible = controlUnderCursor.Visible;
                    if (controlUnderCursor is Box)
                        isVisible = (controlUnderCursor as Box).IsVisible();

                    if (isVisible)
                    {
                        controlUnderCursor.RaiseOnMove(x2, y2);
                    }
                }
            }

        }

        private void PageControl_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (Page != null)
            {
                var controlUnderCursor = Page.GetControlUnderCursor(new PointF((float)x2, (float)y2), true);

                if (controlUnderCursor != null)
                {
                    bool isVisible = controlUnderCursor.Visible;
                    if (controlUnderCursor is Box)
                        isVisible = (controlUnderCursor as Box).IsVisible();

                    if (isVisible)
                    {
                        controlUnderCursor.RaiseOnWheel(e.Delta);
                    }
                }
            }
        }

        private void PageControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (!Popout)
                Page = Core.SelectedPage;

            if (DialogPage != null)
                Page = DialogPage;

            int x = Parent.Location.X + Location.X + e.Location.X + 10;
            int y = Parent.Location.Y + Location.Y + e.Location.Y;
            x = Math.Min(x, Screen.PrimaryScreen.Bounds.Width - 800 - 20);
            y = Math.Min(y, Screen.PrimaryScreen.Bounds.Height - 600 - 40);

            //  if (!scratch && selectOrNew && Studio.File.DesignMode)
            if (selectOrNew && (Core.ThisBook.DesignMode || Core.ThisBook.TagMode))
            {
                Point location = new Point(x, y);
                double w = Math.Max(20, Math.Abs(x1 - x2));
                double h = Math.Max(10, Math.Abs(y1 - y2));
                Bounds bounds = new Bounds(Math.Min(x1, x2), Math.Min(y1, y2), w, h);
                var newItem = NewObjectForm.Edit(location, bounds);
                selectOrNew = false;

                if (newItem is oHtml)
                {
                    oHtml htmlItem = (newItem as oHtml);
                    Page.HtmlItems.Add(htmlItem);
                    cHtml htmlControl = new cHtml(htmlItem);
                    (newItem as oHtml).MyControl = htmlControl;
                    (newItem as oHtml).MyPage = DialogPage ?? Page;
                    htmlControl.Left = (int)(htmlItem.Bounds.X * Draw.mmToPx);
                    htmlControl.Top = (int)(htmlItem.Bounds.Y * Draw.mmToPx);
                    htmlControl.Width = (int)(htmlItem.Bounds.W * Draw.mmToPx);
                    htmlControl.Height = (int)(htmlItem.Bounds.H * Draw.mmToPx);
                    htmlControl.CreateControl();
                    this.Controls.Add(htmlControl);
                    htmlControl.BringToFront();

                    //newControl = new cHtml();
                    //newControl.Name = "html" + Qb.SelectedLayer.Objects.OfType<cHtml>().Count();
                    //newControl.Text = "";
                    ////newControl.Bounds = new Rectangle((int)(Bounds_.X * Draw.mmToPx), (int)(Bounds_.Y * Draw.mmToPx), (int)(Bounds_.W * Draw.mmToPx), (int)(Bounds_.H * Draw.mmToPx));
                    //newControl.Bounds = new Rectangle((int)(Bounds_.X)), (int)(Bounds_.Y), (int)(Bounds_.W), (int)(Bounds_.H));
                }
            }

            if (scratch)
            {
                (selectedItem as oScratch).Data += ";#";
            }

            bool mouseClickHandled = false;
            if (mouseDownControl != null)
            {
                mouseDownControl.RaiseOnClick(new Point(x, y)); //not sure if this Point is correct?!
                mouseClickHandled = true;
            }

            if (!mouseClickHandled && (button || control))
            {
                //                x1 = (float)e.Location.X / Draw.mmToPx;
                //                y1 = (float)e.Location.Y / Draw.mmToPx;
                if (RenderBounds != null)
                {
                    x1 = e.Location.X / MmToPx + RenderBounds.X; //+ 10;
                    y1 = e.Location.Y / MmToPx + RenderBounds.Y; // + 20;
                }
                else
                {
                    x1 = e.Location.X / MmToPx;
                    y1 = e.Location.Y / MmToPx;
                }
                if (selectedItem is oControl)
                    (selectedItem as oControl).Clicked(new PointF((float)x1, (float)y1));
            }

            //      if (moveOrEdit && !moved && Studio.File.DesignMode)
            if (edit && (Core.ThisBook.DesignMode || Core.ThisBook.TagMode))
            {
                if (selectedItem is oTag)
                {


                    EditObjectForm.Edit(x, y, selectedItem);
                    Core.ThisBook.Modified = true;
                }
                else
                {
                    EditObjectForm.Edit(x, y, selectedItem);
                    Core.ThisBook.Modified = true;
                }
            }
            if (settings && (Core.ThisBook.DesignMode || Core.ThisBook.TagMode))
            {
                if (selectedItem is oTag)
                {
                    EditObjectForm.Edit(x, y, selectedItem);
                    Core.ThisBook.Modified = true;
                }
                else
                {
                    EditObjectForm.Edit(x, y, selectedItem);
                    Core.ThisBook.Modified = true;
                }



                //  SettingsForm.Edit(Parent.Location.X + Location.X + e.Location.X + 30, Parent.Location.Y + Location.Y+100, selectedItem);
                //  Qb.File.Modified = true;
            }

            selectOrNew = false;
            moveOrEdit = false;
            settings = false;
            edit = false;
            resize = false;
            scratch = false;
            button = false;
            control = false;
        }
        private void Page_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDownControl = null;

            if (!Popout)
                Page = Core.SelectedPage;

            if (DialogPage != null)
                Page = DialogPage;

            if (Page == null)
                return;

            selectOrNew = true;
            moveOrEdit = false;
            resize = false;
            moved = false;
            settings = false;
            button = false;
            button = control;
            scratch = false;
            edit = false;

            //x1 = (float)e.Location.X / Draw.mmToPx;
            //y1 = (float)e.Location.Y / Draw.mmToPx;
            if (RenderBounds != null)
            {
                x1 = e.Location.X / MmToPx + RenderBounds.X;// + 10;
                y1 = e.Location.Y / MmToPx + RenderBounds.Y;// + 20;
            }
            else
            {
                x1 = e.Location.X / MmToPx;
                y1 = e.Location.Y / MmToPx;
            }


            if (Page != null)
            {
                var controlUnderCursor = Page.GetControlUnderCursor(new PointF((float)x2, (float)y2), true);

                if (controlUnderCursor != null)
                {
                    bool isVisible = controlUnderCursor.Visible;
                    if (controlUnderCursor is Box)
                        isVisible = (controlUnderCursor as Box).IsVisible();

                    if (isVisible)
                    {
                        mouseDownControl = controlUnderCursor; //HALE: new -> prefer mouseDownControl over selectedItem when clicking
                        selectedItem = null;
                        selectOrNew = false;
                        control = true;
                        return;
                    }
                }
            }


                    SortedDictionary<double, oItem> items = new SortedDictionary<double, oItem>();
            foreach (oLayer layer in Page.Objects.OfType<oLayer>())
                foreach (oControl item in layer.Objects.OfType<oControl>())
                {
                    double size = item.Bounds.W * item.Bounds.H;
                    while (items.ContainsKey(size))
                        size++;
                    if (!Core.ThisBook.TagMode || (item is oTag))
                        if (item.Visible)
                            items.Add(size, item);
                }
            if (!Core.ThisBook.TagMode && (!Core.ThisBook.DesignMode || (e.Button == MouseButtons.Right)))
                items.Add(100000000, Page);

            foreach (oControl item in items.Values)
                if (item.Bounds != null)
                {
                    //if ( !(selectedItem is oTag) && item.Bounds.InScaleBox(x1, y1))
                    if (item.Visible && (e.Button == MouseButtons.Right) && item.Bounds.Contains(x1, y1))
                    {
                        selectedItem = item;
                        xObject = item.Bounds.X;
                        yObject = item.Bounds.Y;
                        wObject = item.Bounds.W;
                        hObject = item.Bounds.H;
                        selectOrNew = false;
                        moveOrEdit = false;
                        settings = true;
                        resize = false;
                        edit = false;
                        return;
                    }
                    else if (item.Visible && (e.Button == MouseButtons.Middle) && item.Bounds.Contains(x1, y1)
                        || (item.Bounds.InMoveBox(x1, y1))) //NearHome(x1, y1))
                    {
                        selectedItem = item;
                        xObject = item.Bounds.X;
                        yObject = item.Bounds.Y;
                        wObject = item.Bounds.W;
                        hObject = item.Bounds.H;
                        selectOrNew = false;
                        moveOrEdit = true;
                        settings = false;
                        resize = false;
                        edit = false;
                        return;
                    }
                    else if (item.Visible && item.Bounds.InScaleBox(x1, y1))
                    {
                        selectedItem = item;
                        xObject = item.Bounds.X;
                        yObject = item.Bounds.Y;
                        wObject = item.Bounds.W;
                        hObject = item.Bounds.H;
                        selectOrNew = false;
                        moveOrEdit = false;
                        settings = false;
                        edit = false;
                        resize = true;
                        return;
                    }
                    //else if (item.Bounds.InMoveBox(x1,y1))//NearHome(x1, y1))
                    //{
                    //    selectedItem = item;
                    //    xObject = item.Bounds.X;
                    //    yObject = item.Bounds.Y;
                    //    wObject = item.Bounds.W;
                    //    hObject = item.Bounds.H;
                    //    selectOrNew = false;
                    //    moveOrEdit = true;
                    //    settings = false;
                    //    resize = false;
                    //    edit = false;
                    //    return;
                    //}
                    else if (item.Visible && item.Bounds.InSettingsBox(x1, y1))
                    {
                        selectedItem = item;
                        xObject = item.Bounds.X;
                        yObject = item.Bounds.Y;
                        wObject = item.Bounds.W;
                        hObject = item.Bounds.H;
                        selectOrNew = false;
                        moveOrEdit = false;
                        settings = true;
                        resize = false;
                        edit = false;
                        return;
                    }
                    /*
                    else if (item.Bounds.InEditBox(x1, y1))
                    {
                        selectedItem = item;
                        xObject = item.Bounds.X;
                        yObject = item.Bounds.Y;
                        wObject = item.Bounds.W;
                        hObject = item.Bounds.H;
                        selectOrNew = false;
                        moveOrEdit = false;
                        settings = false;
                        resize = false;
                        edit = true;
                        return;
                    }*/
                    else if (item.Visible && (item is oScratch) && (item.Bounds.Contains(x1, y1)))
                    {
                        selectedItem = item;
                        scratch = true;
                        selectOrNew = false;
                        return;
                    }
                    //TODO MIGRATION-old
                    //else if ((item is oButton) && (item.Bounds.Contains(x1, y1)))
                    //{
                    //    selectedItem = item;
                    //    selectOrNew = false;
                    //    button = true;
                    //    return;
                    //}
                    else if (item.Visible && (item is oControl) && (item.Bounds.Contains(x1, y1)))
                    {
                        if (item is oPage)
                        {
                            //return;
                        }
                        else
                        {
                            selectedItem = item;
                            selectOrNew = false;
                            control = true;
                            return;
                        }
                    }
                    else if (item.Visible && (item is oTag) && (item.Bounds.NearHome(x1, y1)))
                    {
                        selectedItem = item;
                        xObject = item.Bounds.X;
                        yObject = item.Bounds.Y;
                        wObject = item.Bounds.W;
                        hObject = item.Bounds.H;
                        selectOrNew = false;
                        moveOrEdit = true;
                        settings = false;
                        resize = false;
                        edit = false;
                        return;
                    }
                }
        }

        public void DoPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            PageControl_PreviewKeyDown(sender, e);
        }
        private void PageControl_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Control)
            {
                e.IsInputKey = true;
                return;
            }
            if (e.KeyCode == Keys.Down)
            {
                int i = Core.ActualMain.Objects.IndexOf(Page);
                while (i < (Core.ActualMain.Objects.Count - 1))
                {
                    i++;
                    if (Core.ActualMain.Objects[i] is oPage)
                    {
                        Core.SelectedPage = Core.ActualMain.Objects[i] as oPage;
                        return;
                    }
                }
                return;
            }
            if (e.KeyCode == Keys.Up)
            {
                int i = Core.ActualMain.Objects.IndexOf(Page);
                while (i > 0)
                {
                    i--;
                    if (Core.ActualMain.Objects[i] is oPage)
                    {
                        Core.SelectedPage = Core.ActualMain.Objects[i] as oPage;
                        return;
                    }
                }
                return;
            }
        }

        public void DoKeyDown(object sender, KeyEventArgs e)
        {
            PageControl_KeyDown(sender, e);
        }
        private void PageControl_KeyDown(object sender, KeyEventArgs e)
        {

            //if (e.Control && e.Shift && e.KeyCode == Keys.S)
            //{
            //    //Qb.ScriptingEngine.ShowLogForm();
            //    //Qb.ScriptingEngine.ShowScriptingForm();
            //    return;
            //}
            //if (e.Control && e.KeyCode == Keys.S)
            //{
            //    Qb.ShowLogForm();
            //    Qb.ShowCsScriptingForm();
            //    return;
            //}
            if (e.Control && e.KeyCode == Keys.L)
            {
                Core.ShowLogForm();
                return;
            }
            if (e.Control && e.KeyCode == Keys.E) //show/hide CodeEditor
            {
                if (Core.IsFormCodeEditorVisible)
                    Core.FormCodeEditor.SendToBack();
                else
                {
                    if (QB.Book.AccessLevel >= AccessLevel.Admin)
                    {
                        Core.ShowFormCodeEditor(Page);
                    }
                }
                return;
            }
            if (e.Control && e.KeyCode == Keys.O) //open
            {
                Core.ShowOpenQbookFileDialog(sender);
                return;
            }

            if (e.KeyCode == Keys.Down && e.Control == true)
            {
                Core.ThisBook.Modified = true;
                Core.ItemMoveForward(Page);
            }
            if (e.KeyCode == Keys.Up && e.Control == true)
            {
                Core.ThisBook.Modified = true;
                Core.ItemMoveBack(Page);
            }
            if (e.KeyCode == Keys.X && e.Control == true)
            {
                Clipboard.SetText(Page.Serialize());
                DialogResult result = MessageBox.Show("delete page '" + (Page) + "' ?", "delete page!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    Core.ThisBook.Main.Remove(Page);
                }
            }
            if (e.KeyCode == Keys.C && e.Control == true)
            {
                Clipboard.SetText(Page.Serialize());
            }
            if (e.KeyCode == Keys.V && e.Control == true)
            {
                Core.ThisBook.Modified = true;
                oItem item = oItem.Deserialize(Clipboard.GetText());
                item.TextL += " (clipboard)";

                int indx = Core.ActualMain.Objects.IndexOf(Page);
                Core.ActualMain.Objects.Insert(indx + 1, item);
                //  Studio.Pages.Add(item);
                Page = item as oPage;


            }
            if (e.KeyCode == Keys.D && e.Control == true)
            {
                PopoutForm form = new PopoutForm();
                form.Page = Page;

                form.Show();
            }

        }

        private void PageControl_Scroll(object sender, ScrollEventArgs e)
        {
            string scroll = "";
        }

        private void PageControl_SizeChanged(object sender, EventArgs e)
        {
            if (Core.ThisBook == null)
                return;
            if (Core.ActualMain == null)
                return;
            if (Page == null)
                return;
            //foreach (oLayer layer in Page.Objects.OfType<oLayer>())
            //    foreach (oControl item in layer.Objects.OfType<oControl>())
            //        item.ObjectSettings.Modified = true;
        }
    }
}

