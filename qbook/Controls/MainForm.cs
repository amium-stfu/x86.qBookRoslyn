using CefSharp;
using ColoredTextBox;
using CSScripting;
using log4net;
using qbook.Controls;
using QB.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using qbook.CodeEditor;

namespace qbook
{
    public partial class MainForm : Form
    {

        public static MainForm Instance { get; private set; }
        public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //MruFilesManager mruFilesManager = new MruFilesManager();
        private static string GetApplicationPath()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");
        }

        string GetNamedArgValue(string[] args, string name, string defaultValue = null)
        {
            var names = name.Split('|');
            foreach (string n in names)
            {
                //int fIndex = Array.FindIndex(args, a => a.StartsWith(n));
                string arg = args.FirstOrDefault(a => a.Split('=')[0] == n);
                if (arg != null)
                {
                    string[] argKvp = arg.Split('=');
                    if (argKvp.Length > 1)
                        return argKvp[1].Trim().Trim('\"');
                    else
                        return ""; //exists, but no value
                }
            }
            return defaultValue; //not found
        }
        bool ExistsNamedArg(string[] args, string name)
        {
            return GetNamedArgValue(args, name) != null;
        }

        string[] _ProgramArgs = null;

        static MainForm _staticMainForm = null;
        public MainForm(string[] args)
        {
            Instance = this;
            VEXhubCommon.Licensing.EnableLicense2 = true;

            _ProgramArgs = args;
            log.Info("qbook Main starting. args=" + string.Join(" ", args));
            InitializeComponent();
            
            _staticStatusLabel = statusLabelStatus;
            _staticMainForm = this;

            if (Properties.Settings.Default.UpgradeRequired)
            {
                log.Info("settings upgrade required!");
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }

            this.KeyPreview = true;

            string widthArg = GetNamedArgValue(args, "-width");
            if (widthArg != null)
            {
                string heightArg = GetNamedArgValue(args, "-height");
                int width = Convert.ToInt32(widthArg);
                if (width == 1920 && heightArg == null)
                    heightArg = "1080";
                if (width == 1680 && heightArg == null)
                    heightArg = "1050";
                int height = Convert.ToInt32(heightArg);
                this.Size = new Size(width, height);
            }


            uriStartupToolStripMenuItem.Text = "";
            string uriArgs = GetNamedArgValue(args, "-uri");
            if (uriArgs != null)
            {
                QB.Book.UriStartup = uriArgs;
                uriStartupToolStripMenuItem.Text = uriArgs;

            }

            if (ExistsNamedArg(args, "-full|--fullscreen")) // args.Contains("-f") || args.Contains("--fullscreen"))
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
            }

            string mruCsvList = Properties.Settings.Default.MruFileList;
            //MessageBox.Show(mruCsvList.Replace(",", "\r\n"), "MRU");
            qbook.Core.MruFilesManager.SetMruByCsvString(mruCsvList);

            // SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
            ResizePageControl();



            Book.StaticPropertyChanged += Book_StaticPropertyChanged;

            try
            {
                QB.Net.Serial.ComPortListChangedEvent += Serial_ComPortListChangedEvent;
            }
            catch
            {

                MessageBox.Show("QB.Net.Serial.ComPortListChangedEvent");
            }

            UpdateFormTitle();
            hideButtonText();
        }


        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (Instance == this)
                Instance = null;
        }

        public PageControl GetPageControl()
        {
            return pageControl;
        }

        void hideButtonText()
        {
            fileToolStripMenuItem.Text = "";
            viewToolStripMenuItem.Text = "";
            toolsToolStripMenuItem.Text = "";
            helpToolStripMenuItem.Text = "";
            fullscreenToolStripMenuItem.Text = "";
            pagebarToolStripMenuItem.Text = "";
            logToolStripMenuItem.Text = "";
            optionsToolStripMenuItem.Text = "";
        }

        private void Serial_ComPortListChangedEvent(EventArgs e)
        {
            this.Invoke((MethodInvoker)(() =>
            {
                cOMPortsToolStripMenuItem.BackColor = Color.Orange;
            }));
        }

        private void Qb_SelectedPageChangedEvent(qbook.Core.SelectedPageChangedEventArgs e)
        {
            log.Info($"Main.Qb.selectedpagechanged: {e.OldPage?.Name} -> {e.NewPage?.Name}");
            if (e.OldPage != null)
            {
                foreach (var htmlItem in e.OldPage.HtmlItems)
                {
                    if (htmlItem.MyControl != null)
                    {
                        pageControl.Invoke((MethodInvoker)(() =>
                        {
                            pageControl.Controls.Remove(htmlItem.MyControl);
                            htmlItem.MyControl.Dispose();
                        }));
                        pageControl.Controls.Remove(htmlItem.MyControl);
                    }
                }
            }

            if (e.NewPage != null)
            {
                foreach (var htmlItem in e.NewPage.HtmlItems)
                {
                    if (htmlItem.MyControl != null)
                    {
                        pageControl.Invoke((MethodInvoker)(() =>
                        {
                            pageControl.Controls.Remove(htmlItem.MyControl);
                            htmlItem.MyControl.Dispose();
                        }));
                    }

                    cHtml htmlControl = new cHtml(htmlItem);
                    htmlItem.MyControl = htmlControl;
                    htmlItem.MyPage = e.NewPage;
                    htmlControl.Left = (int)(htmlItem.Bounds.X * Draw.mmToPx);
                    htmlControl.Top = (int)(htmlItem.Bounds.Y * Draw.mmToPx);
                    htmlControl.Width = (int)(htmlItem.Bounds.W * Draw.mmToPx);
                    htmlControl.Height = (int)(htmlItem.Bounds.H * Draw.mmToPx);

                    if (pageControl != null && pageControl.IsHandleCreated)
                    {
                        pageControl.Invoke((MethodInvoker)(() =>
                        {
                            htmlControl.CreateControl();
                            pageControl.Controls.Add(htmlControl);
                            htmlControl.BringToFront();
                            //htmlControl.Resize();
                        }));
                    }
                    else
                    {
                        htmlControl.CreateControl();
                        pageControl.Controls.Add(htmlControl);
                        htmlControl.BringToFront();
                        //htmlControl.Resize();
                    }
                }
                //pageControl.DoSizeChanged(new EventArgs());
            }
        }

        float ratio = 29.7f / 21.0f; //A4
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            ResizePageControl();
        }




        void toggleFullScreen()
        {
            if (this.WindowState == FormWindowState.Maximized && this.FormBorderStyle == FormBorderStyle.None)
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
                menuStrip.Visible = true;
                statusStrip.Visible = true;
            }
            else
            {
                //Workaround sometimes Display bug
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
                //

                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                menuStrip.Visible = false;
                statusStrip.Visible = false;
            }
        }


        int PageControlBar = 300;
        void ResizePageControl()
        {
            int paddingTop = 0;
            int paddingBottom = 0;
            if (this.FormBorderStyle == FormBorderStyle.None) //F11 FullScreen (no border, no menu-/status-bar)
            {
                paddingTop = 0;
                paddingBottom = 0;
            }
            else
            {
                paddingTop = 20;
                paddingBottom = 20;
            }

            float w = this.ClientRectangle.Width - PageControlBar;
            float h = this.ClientRectangle.Height - 11 - paddingTop - paddingBottom;

            //if (!showOldCommadBarToolStripMenuItem.Checked)
            //    h += 46;

            if (w / h > ratio)
                pageControl.Bounds = new System.Drawing.Rectangle(3 + (int)((w - h * ratio) / 2), 5 + paddingTop, (int)(h * ratio), (int)h - 3);
            else
                pageControl.Bounds = new System.Drawing.Rectangle(3, (int)((h - w / ratio) / 2) + paddingTop, (int)w, (int)(w / ratio - 3));


            QB.Root.InvalidateBoxBounds();
        }

        bool _FormClosingNoUserInteraction = false;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (qbook.Core.ThisBook != null)
                qbook.Core.ThisBook.Bounds = Bounds;

            if (!_FormClosingNoUserInteraction)
            {
                if (DialogResult.Cancel == MessageBox.Show("Do you really want to quit?", "qbook EXIT", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand))
                {
                    e.Cancel = true;
                    return;
                }

                log.Info($"MainForm closing...");

                SendToBack();
                if (qbook.Core.ThisBook.Modified)
                {
                    DialogResult result = MessageBox.Show("Save " + qbook.Core.ThisBook.Filename + "?", "qbook NOT SAVED", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                        qbook.Core.ThisBook.Serialize();

                    //Main.Properties.Settings.Default.Save();
                }
            }

            Properties.Settings.Default.MruFileList = qbook.Core.MruFilesManager.GetMruCsvString();
            qbook.Properties.Settings.Default.Save();

            //MessageBox.Show(Main.Qb.MruFilesManager.GetMruCsvString(), "MRU");
        }


        DateTime recDate = DateTime.Now.AddSeconds(1);
        StreamWriter sr = null;
        int flusher = 0;
        List<string> names = new List<string>();
        void RecorderIdle()
        {
            if (qbook.Core.ThisBook != null && qbook.Core.ThisBook.Recorder)
            {
                if (DateTime.Now > recDate)
                {
                    if (!Directory.Exists(qbook.Core.ThisBook.Filename))
                        Directory.CreateDirectory(qbook.Core.ThisBook.Filename);
                    if (sr == null)
                    {
                        sr = new StreamWriter(Path.Combine(qbook.Core.ThisBook.Filename, DateTime.Now.ToString("yyyy-MM-dd HH.mm") + " recorder.csv"), true);
                        string header = "date;time";
                        sr.WriteLine(header);
                    }

                    string line = DateTime.Now.ToString("yyyy-MM-dd;HH:mm:ss");
                    sr.WriteLine(line);
                    flusher++;
                    if (flusher % 10 == 0)
                        sr.FlushAsync();
                    recDate = DateTime.Now.AddSeconds(1);
                }
            }
            else
            {
                if (sr != null)
                    sr.Close();
                sr = null;
            }
        }
        private void timer100ms_Tick(object sender, EventArgs e)
        {
            RecorderIdle();
            if (qbook.Core.ThisBook == null)
                return;

            //this.Text = $"{Main.Qb.Book.Filename}-{Main.Qb.SelectedPage.TextL} [v{AssemblyVersionStringEx}]";
            pageControl.Refresh();//.Invalidate();
            Invalidate();

            if (doPdf)
            {
                doPdf = false;
            }
        }

        int start = 0;
        int yrect = 0;
        List<oIcon> Icons = new List<oIcon>();

        void IconsAdd(oIcon icon)
        {

            lock (Icons)
            {
                Icons.Add(icon);
            }
        }

        bool showPageControl = true;

        void setPageFormat(string format)
        {
            switch (format)
            {
                case "A4":
                    ratio = 29.7f / 21.0f;
                    break;
                case "16/9":
                    ratio = 16f / 9f;
                    break;
                case "16/10":
                    ratio = 16f / 10f;
                    break;
            }
            //2025-09-03 STFU
            QB.Book.PageFormat = format;

            ResizePageControl();
        }


        //2025-09-06 STFU
        public bool InitView = true;
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            if (qbook.Core.ThisBook == null)
                return;

            //2025-09-06 STFU
            if (InitView)
            {
                if (qbook.Core.ThisBook.HidPageMenuBar != HidPageMenuBar)
                    HidPageMenuBar = qbook.Core.ThisBook.HidPageMenuBar;

                if (qbook.Core.ThisBook.StartFullScreen != isFullScreen)
                    FullScreen = qbook.Core.ThisBook.StartFullScreen;

                InitView = false;
            }
            //

            setPageFormat(qbook.Core.SelectedPage.Format);

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            int y = pageControl.Location.Y;
            int h = 40;
            int w = 20;
            int wBrowser = 280;
            int dist = 1;
            lock (Icons)
            {
                Icons.Clear();
            }

            y = pageControl.Location.Y + pageControl.Height + 10;
            int wo = 30;
            int xo = pageControl.Location.X + pageControl.Width - 2;

           
            //page control icons
            int yOffset = 0; //HALE: due to new menu; will be removed eventually?!
            y = pageControl.Location.Y - h + yOffset;
            if (qbook.Core.ThisBook.Main != qbook.Core.ActualMain)
            {
                y += (h + dist);
                IconsAdd(new oIcon(null, qbook.Icon.Button, Pens.Black, "<<", Width - wBrowser, y, 25, h, Root));
            }
            xo = 10;
            wo = 50;
            int skip = start;

            List<oPage> viewList = new List<oPage>();

            foreach(string pageName in Core.ThisBook.PageOrder)
            {
                foreach (oPage page in qbook.Core.ActualMain.Objects.Where(item => item is oPage))
                {
                    if (page.Name == pageName)
                        viewList.Add(page);
                }

            }
            foreach (oPage page in viewList)
              //  foreach (oPage page in qbook.Core.ActualMain.Objects.Where(item => item is oPage))
            {
                if (!showPageControl) break;

                if (page.Hidden && !qbook.Core.ThisBook.DesignMode)
                    continue;

                if (skip > 0)
                {
                    skip--;
                    continue;
                }
                int ys = y;
                int xs = Width - wBrowser;
                xs = pageControl.Width + pageControl.Bounds.X + 3;
                int bw = Width - xs - 3;
                int x = xs;
                y += (h + dist);



                if (page.Url != null)
                {
                    page.Text = page.Url;
                    IconsAdd(new oIcon(page, qbook.Icon.Button, Pens.LightSlateGray, "" + qbook.Core.PageNumber(page), x, y, (float)(h), (float)(h), PageSelect));

                    string itemText = "   ";
                    if (qbook.Core.ThisBook.DesignMode)
                        itemText = ("    .\\" + qbook.Core.ActualMain.TextL + "\\").Replace("\\\\", "\\") + page.Name + " (" + page.TextL + ")";
                    else
                    {
                        itemText = page.TextL;
                        if (itemText.Trim() == "#")
                            itemText = page.Name;
                    }
                    IconsAdd(new oIcon(page, qbook.Icon.Text, Pens.LightSlateGray, itemText, x, y, 198, h, EditText));
                }
                else
                {
                    IconsAdd(new oIcon(page, qbook.Icon.PageNumber, Pens.LightSlateGray, "" + qbook.Core.PageNumber(page), x, y, h, h, PageSelect));

                    string itemText = "    ";
                    if (qbook.Core.ThisBook.DesignMode)
                        itemText = ("     .\\" + qbook.Core.ActualMain.TextL + "\\").Replace("\\\\", "\\") + page.Name + " (" + page.TextL + ")";
                    else
                    {
                        itemText = page.TextL;
                        if (itemText.Trim() == "#")
                            itemText = page.Name;
                    }
                  
                    IconsAdd(new oIcon(page, qbook.Icon.Text, Pens.LightSlateGray, itemText, x + h, y, xs, h, null));

                    if (QB.Book.AccessLevel >= QB.AccessLevel.Admin)
                    {
                        IconsAdd(new oIcon(page, qbook.Icon.PageFunction, Pens.LightSlateGray, "Code", x + bw - h * 2, y, h, (float)(h * 0.5),EditText));
                        IconsAdd(new oIcon(page, qbook.Icon.PageFunction, Pens.LightSlateGray, page.Format, x + bw - h * 2, y + (float)(h * 0.5), h, (float)(h * 0.5), SwitchFormat));

                    }
           
                }


                if (y > pageControl.Location.Y + pageControl.Height - 2 * h)
                    break;
            }


            lock (Icons)
            {
                foreach (oIcon pic in Icons)
                    pic.Paint(e.Graphics);
            }

        }

        void TagMode(object sender)
        {
            qbook.Core.ThisBook.TagMode = !qbook.Core.ThisBook.TagMode;
            qbook.Core.ThisBook.DesignMode = false;
            if (qbook.Core.ThisBook.TagMode)
            {
                qbook.Core.ThisBook.Grid = 5;
                qbook.Core.ThisBook.Language = "en";
            }
            else
            {
                qbook.Core.ThisBook.Grid = 0;
            }
        }

        void SetAsActualMain(object sender)
        {
            qbook.Core.ActualMain = (sender as oIcon).Parent;
            //     Studio.PagesRootText = (sender as oIcon).Parent.Name + "/"+ (sender as oIcon).Parent.TextL;
        }

        void Root(object sender)
        {
            qbook.Core.ActualMain = qbook.Core.ThisBook.Main;
            //   Main.Qb.PagesRootText = "";
        }

        void Template(object sender)
        {
            if (qbook.Core.ThisBook.Modified)
            {

                DialogResult result = MessageBox.Show("save " + qbook.Core.ThisBook.Filename + "?", "qBook NOT SAVED", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                    qbook.Core.ThisBook.Serialize();

                //Main.Properties.Settings.Default.LastFiles = Main.Properties.Settings.Default.LastFiles.Replace(Main.Qb.Book.Filename, "").Replace("||", "|").Trim('|');
                //Main.Properties.Settings.Default.LastFiles += "|" + Main.Qb.Book.Filename;
                //Main.Properties.Settings.Default.Save();
            }

            qbook.Core.ThisBook = Book.Deserialize(Path.Combine("qbooks", "template.qbook")); // ""GetApplicationPath(), "template"); 
            QB.Root.ActiveQbook = qbook.Core.ThisBook;

            if (qbook.Core.ThisBook.Main == null)
                qbook.Core.ThisBook.Main = new oControl();
            qbook.Core.ActualMain = qbook.Core.ThisBook.Main;
            //   Main.Qb.PagesRootText = "";
            if (qbook.Core.ActualMain.Objects.Count == 0)
                qbook.Core.ActualMain.Add(new oPage("", "page 1"));

            qbook.Core.ThisBook.Init();
            qbook.Core.ThisBook.PropertyChanged -= Book_PropertyChanged;
            qbook.Core.ThisBook.PropertyChanged += Book_PropertyChanged;
            if (Debugger.IsAttached)
                QB.Book.AccessLevel = QB.AccessLevel.Admin;
            UpdateMenuBar();
            qbook.Core.UpdateProjectAssemblyQbRoot("MainForm.Template");
        }

        void UpdateMenuBar()
        {
            Book_PropertyChanged(this, new Book.PropertyChangedEventArgs() { Property = "Filename", Value = qbook.Core.ThisBook.Filename });
            Book_PropertyChanged(this, new Book.PropertyChangedEventArgs() { Property = "Modified", Value = qbook.Core.ThisBook.Modified });
            Book_PropertyChanged(this, new Book.PropertyChangedEventArgs() { Property = "DesignMode", Value = qbook.Core.ThisBook.DesignMode });
            Book_PropertyChanged(this, new Book.PropertyChangedEventArgs() { Property = "TagMode", Value = qbook.Core.ThisBook.TagMode });
            Book_PropertyChanged(this, new Book.PropertyChangedEventArgs() { Property = "Recorder", Value = qbook.Core.ThisBook.Recorder });
            Book_PropertyChanged(this, new Book.PropertyChangedEventArgs() { Property = "AccessLevel", Value = null });
        }

        private void Book_StaticPropertyChanged(object sender, Book.PropertyChangedEventArgs e)
        {
            switch (e.Property)
            {
                case "Directory":
                    StatusUpdateFilename();
                    break;
                case "Filename":
                    StatusUpdateFilename();
                    break;
                case "new Book":
                    StatusUpdateFilename();
                    break;
            }
        }

        void StatusUpdateFilename()
        {
            if (qbook.Core.ThisBook == null)
                return;

            if (qbook.Core.ThisBook.DesignMode)
                statusLabelName.Text = Path.GetFileNameWithoutExtension(qbook.Core.ThisBook.Filename) + $" [{qbook.Core.ThisBook.Directory}]";
            else
                statusLabelName.Text = Path.GetFileNameWithoutExtension(qbook.Core.ThisBook.Filename);

            ForceUpdateFormTitle = true;
        }

        private void Book_PropertyChanged(object sender, Book.PropertyChangedEventArgs e)
        {
            switch (e.Property)
            {
                case "DesignMode":
                    //if (Main.Qb.Book.DesignMode)
                    //    designToolStripMenuItem.BackColor = Color.DeepSkyBlue;
                    //else
                    //    designToolStripMenuItem.BackColor = SystemColors.Control;
                    if (qbook.Core.ThisBook.DesignMode)
                        designToolStripMenuItem.Image = imageListMenuItems.Images[1];
                    else
                        designToolStripMenuItem.Image = imageListMenuItems.Images[0];
                    //insertToolStripMenuItem.Enabled = Main.Qb.Book.DesignMode;
                    //optionsToolStripMenuItem.Enabled = Main.Qb.Book.DesignMode;
                    StatusUpdateFilename();
                    break;
                case "TagMode":
                    if (qbook.Core.ThisBook.TagMode)
                        tagToolStripMenuItem.Image = imageListMenuItems.Images[3];
                    else
                        tagToolStripMenuItem.Image = imageListMenuItems.Images[2];
                    break;
                case "Recorder":
                    if (qbook.Core.ThisBook.Recorder)
                        recorderToolStripMenuItem.Image = imageListMenuItems.Images[5];
                    else
                        recorderToolStripMenuItem.Image = imageListMenuItems.Images[4];
                    break;
                case "Language":
                    switch (qbook.Core.ThisBook.Language)
                    {
                        case "de":
                            SelectMenuLanguage("de");
                            break;
                        case "es":
                            SelectMenuLanguage("es");
                            break;
                        default:
                            SelectMenuLanguage("en");
                            break;
                    }
                    break;
                case "Modified":
                    if (qbook.Core.ThisBook.Modified)
                        statusLabelName.Text = Path.GetFileNameWithoutExtension(qbook.Core.ThisBook.Filename) + "*";
                    else
                        statusLabelName.Text = Path.GetFileNameWithoutExtension(qbook.Core.ThisBook.Filename);
                    break;
                case "AccessLevel":
                    toolStripUser.Text = QB.Book.AccessLevel.ToString();

                    rootToolStripMenuItem.Visible = false;

                    designToolStripMenuItem.Visible = false;
                    tagToolStripMenuItem.Visible = false;
                    recorderToolStripMenuItem.Visible = false;

                    switch (QB.Book.AccessLevel)
                    {
                        case QB.AccessLevel.Online:
                            toolStripUser.Image = toolStripUserOnline.Image;
                            insertToolStripMenuItem.Visible = false;
                            optionsToolStripMenuItem.Visible = false;
                            toolsToolStripMenuItem.Visible = false;
                            debugToolStripMenuItem.Visible = false;
                            break;
                        case QB.AccessLevel.Offline:
                            toolStripUser.Image = toolStripUserOffline.Image;
                            insertToolStripMenuItem.Visible = false;
                            optionsToolStripMenuItem.Visible = false;
                            toolsToolStripMenuItem.Visible = false;
                            debugToolStripMenuItem.Visible = false;
                            break;
                        case QB.AccessLevel.User:
                            toolStripUser.Image = toolStripUserUser.Image;
                            insertToolStripMenuItem.Visible = false;
                            optionsToolStripMenuItem.Visible = false;
                            toolsToolStripMenuItem.Visible = false;
                            debugToolStripMenuItem.Visible = false;
                            break;
                        case QB.AccessLevel.Service:
                            toolStripUser.Image = toolStripUserService.Image;
                            insertToolStripMenuItem.Visible = false;
                            optionsToolStripMenuItem.Visible = false;
                            toolsToolStripMenuItem.Visible = false;
                            debugToolStripMenuItem.Visible = false;
                            break;
                        case QB.AccessLevel.Admin:
                            toolStripUser.Image = toolStripUserAdmin.Image;
                            insertToolStripMenuItem.Visible = false;
                            optionsToolStripMenuItem.Visible = true;
                            toolsToolStripMenuItem.Visible = false;
                            designToolStripMenuItem.Visible = false;
                            tagToolStripMenuItem.Visible = false;
                            recorderToolStripMenuItem.Visible = false;
                            debugToolStripMenuItem.Visible = false;
                            break;
                        case QB.AccessLevel.Root:
                            toolStripUser.Image = toolStripUserRoot.Image;
                            insertToolStripMenuItem.Visible = true;
                            optionsToolStripMenuItem.Visible = true;
                            toolsToolStripMenuItem.Visible = true;
                            designToolStripMenuItem.Visible = false;
                            tagToolStripMenuItem.Visible = true;
                            recorderToolStripMenuItem.Visible = true;

                            rootToolStripMenuItem.Visible = true;
                            break;
                        default:
                            toolStripUser.Image = null;
                            insertToolStripMenuItem.Visible = false;
                            optionsToolStripMenuItem.Visible = false;
                            toolsToolStripMenuItem.Visible = false;
                            break;
                    }
                    break;
            }
        }
        void SelectMenuLanguage(string language)
        {
            enEnglishToolStripMenuItem.Checked = false;
            deDeutschToolStripMenuItem.Checked = false;
            esEspanolToolStripMenuItem.Checked = false;
            switch (language)
            {
                case "de":
                    deDeutschToolStripMenuItem.Checked = true;
                    break;
                case "es":
                    esEspanolToolStripMenuItem.Checked = true;
                    break;
                default:
                    enEnglishToolStripMenuItem.Checked = true;
                    break;
            }
        }

        void Outlook(object sender)
        {
            OutlookMail.Read();
        }

        void Webcam(object sender)
        {

            WebcamForm form = new WebcamForm();
            form.Show();

        }

        void Screenshot(object sender)
        {
            WindowState = FormWindowState.Minimized;
            System.Threading.Thread.Sleep(500);
            Bitmap bm = Draw.GetSreenshot();
            WindowState = FormWindowState.Normal;
            pageControl.DialogImage(bm);
        }

        bool doPdf = false;
        void ExportPdf(object sender, bool allPages = false)
        {
            doPdf = true;
            Draw.PdfOpen(Path.Combine(qbook.Core.ThisBook.Directory, DateTime.Now.ToString("yyyy-MM-dd HH.mm ") + qbook.Core.ThisBook.Filename) + ".pdf", qbook.Core.ThisBook.Filename);

            foreach (oPage page in qbook.Core.ActualMain.Objects.Where(item => item is oPage))
            {
                if (!allPages && page != qbook.Core.SelectedPage)
                    continue;

                qbook.Core.SelectedPage = page;
                qbook.Core.ActualMain = qbook.Core.ThisBook.Main;
                Draw.PdfGraphics = Draw.PdfNewPage();
                pageControl.Refresh();
                foreach (oPage subPage in page.Objects.Where(item => item is oPage))
                {
                    qbook.Core.SelectedPage = subPage;
                    qbook.Core.ActualMain = page;
                    Draw.PdfGraphics = Draw.PdfNewPage();
                    pageControl.Refresh();
                }
            }
            Draw.PdfGraphics = null;

            Draw.PdfSave(false);// checkBoxPdfEncrypted.Checked);
            Draw.PdfShow();
        }

        void NewQbook(object sender)
        {
            if (qbook.Core.ThisBook.Modified)
            {

                DialogResult result = MessageBox.Show("save " + qbook.Core.ThisBook.Filename + "?", "qBook NOT SAVED", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                    qbook.Core.ThisBook.Serialize();

                //Main.Properties.Settings.Default.LastFiles = Main.Properties.Settings.Default.LastFiles.Replace(Main.Qb.Book.Filename, "").Replace("||", "|").Trim('|');
                //Main.Properties.Settings.Default.LastFiles += "|" + Main.Qb.Book.Filename;
                //Main.Properties.Settings.Default.Save();
            }
            string directory = Path.Combine(GetApplicationPath(), "qbooks");
            qbook.Core.ThisBook = Book.Deserialize(Path.Combine(directory, DateTime.Now.ToString("yyyy-MM-dd hh.mm") + " new")); // GetApplicationPath(), DateTime.Now.ToString("yyyy-MM-dd hh.mm") + " new"); 
            QB.Root.ActiveQbook = qbook.Core.ThisBook;

            if (qbook.Core.ThisBook.Main == null)
                qbook.Core.ThisBook.Main = new oControl();
            qbook.Core.ActualMain = qbook.Core.ThisBook.Main;
            //   Main.Qb.PagesRootText = "";
            if (qbook.Core.ActualMain.Objects.Count == 0)
                qbook.Core.ActualMain.Add(new oPage("", "page 1"));
            qbook.Core.ThisBook.Init();
            qbook.Core.ThisBook.PropertyChanged -= Book_PropertyChanged;
            qbook.Core.ThisBook.PropertyChanged += Book_PropertyChanged;
            if (Debugger.IsAttached)
                QB.Book.AccessLevel = QB.AccessLevel.Admin;
            UpdateMenuBar();
            qbook.Core.UpdateProjectAssemblyQbRoot("MainForm.NewQbook");
        }

        async void ShowOpenQbookFileDialog(object sender)
        {
            qbook.Core.ShowOpenQbookFileDialog(sender);
        }

        void ShowMruList(object sender)
        {
            contextMenuMru.Items.Clear();
            //Properties.Settings.Default.MruFileList = Main.Qb.MruFilesManager.GetMruCsvString();
            List<MruFilesManager.MruItem> mruItems = qbook.Core.MruFilesManager.MruItems; //.GetMruStringList();

            ToolStripMenuItemEx[] items = new ToolStripMenuItemEx[mruItems.Count]; // You would obviously calculate this value at runtime
            for (int i = 0; i < items.Length; i++)
            {
                var mruItem = mruItems[i];

                items[i] = new ToolStripMenuItemEx();
                items[i].Name = mruItem.FullPath;
                items[i].Tag = null;

                string qbookname = mruItem.FullPath.GetFileNameWithoutExtension();
                string directory = mruItem.FullPath.GetDirName();
                string lastModifiedStr = "xxxx-xx-xx xx:xx:xx";
                if (mruItem.LastModified != null)
                    lastModifiedStr = mruItem.LastModified?.ToString("yyyy-MM-dd HH:mm:ss");
                items[i].Text = (mruItem.FileExists ? "" : "[x] ") + qbookname + " @" + lastModifiedStr + " [" + directory + "\\]";
                items[i].MouseUp += MruItemClicked;
                items[i].DeleteItemClicked += MruItemDeleteIconClicked;
            }
            contextMenuMru.Items.AddRange(items);

            //open qbook menuitem
            contextMenuMru.Items.Add(new ToolStripSeparator());
            var openMenuItem = new ToolStripMenuItem("Open qbook...");
            openMenuItem.MouseUp += MruItemOpen;
            contextMenuMru.Items.Add(openMenuItem);


            if (false && contextMenuMru.Items.Count > 0)
            {
                contextMenuMru.Items.Add(new ToolStripSeparator());
                var clearMruListMenuItem = new ToolStripMenuItem("Clear MRU List");
                clearMruListMenuItem.MouseUp += MruItemClearList;
                contextMenuMru.Items.Add(clearMruListMenuItem);
            }

            contextMenuMru.Show(Cursor.Position);
        }

        private void MruItemDeleteIconClicked(object sender, ToolStripMenuItemEx.DeleteItemClickedEventArgs e)
        {
            string filename = (sender as ToolStripMenuItem)?.Name;
            if (filename != null)
            {
                qbook.Core.MruFilesManager.Remove(filename);
                Properties.Settings.Default.MruFileList = qbook.Core.MruFilesManager.GetMruCsvString();
                Properties.Settings.Default.Save();

                List<ToolStripItem> itemsToRemove = new List<ToolStripItem>();
                foreach (ToolStripItem item in contextMenuMru.Items)
                {
                    if (item.Name == filename)
                        itemsToRemove.Add(item);
                }
                foreach (var item in itemsToRemove)
                    contextMenuMru.Items.Remove(item);
            }
        }

        public class ToolStripMenuItemEx : ToolStripMenuItem
        {
            public ToolStripMenuItemEx() { }
            public ToolStripMenuItemEx(string text, Image image) : base(text, image) { }
            public ToolStripMenuItemEx(string text) : base(text) { }
            public ToolStripMenuItemEx(Image image) : base(image) { }
            public ToolStripMenuItemEx(string text, Image image, EventHandler onClick) : base(text, image, onClick) { }
            public ToolStripMenuItemEx(string text, Image image, int id) : base(text, image) { this.ID = id; }
            public ToolStripMenuItemEx(string text, Image image, int id, EventHandler onClick) : base(text, image, onClick) { this.ID = id; }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                if (false) //custom paint -> TODO!
                {
                    Rectangle textRect = new Rectangle(0, 1, this.Width, 20);
                    StringFormat fmt = new StringFormat();
                    e.Graphics.DrawString(this.Text, this.Font, Brushes.Black, textRect, fmt);
                }


                /*
                if (base.Checked == false)
                {
                    Rectangle rect = new Rectangle(this.Width - 20, 1, 20, 20);
                    e.Graphics.DrawImage(Properties.Resources.delete_cross_icon2, rect);
                }
                else
                {
                    Rectangle rect = new Rectangle(this.Width - 20, 1, 20, 20);
                    e.Graphics.DrawImage(Properties.Resources.delete_cross_icon2, rect);
                }
                */
                Rectangle rect = new Rectangle(this.Width - 20, 1, 20, 20);
                e.Graphics.DrawImage(Properties.Resources.delete_cross_icon2, rect);
            }
            public int ID { get; set; }
            public bool DeleteIconClicked { get; set; }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                DeleteIconClicked = e.X > (this.Width - 20);
                if (DeleteIconClicked)
                {
                    //this.Checked = this.Checked == true ? false : true;
                    OnDeleteItemClicked();
                }
                else
                {
                    base.OnClick(e);
                    base.OnMouseDown(e);
                }
                //base.OnMouseDown(e);
            }

            public class DeleteItemClickedEventArgs : EventArgs
            {
            }
            public delegate void DeleteItemClickedHandler(object sender, DeleteItemClickedEventArgs e);
            public event DeleteItemClickedHandler DeleteItemClicked;
            void OnDeleteItemClicked()
            {
                if (DeleteItemClicked != null)
                {
                    DeleteItemClickedEventArgs ea = new DeleteItemClickedEventArgs() { };
                    DeleteItemClicked(this, ea);
                }
            }

        }

        private void MruItemClearList(object sender, EventArgs e)
        {
            var dr = MessageBox.Show($"Really clear the list of recently opened qbooks?", "CLEAR LIST", MessageBoxButtons.OKCancel, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.OK)
            {
                qbook.Core.MruFilesManager.Clear();
                Properties.Settings.Default.MruFileList = null;
                Properties.Settings.Default.Save();
            }
        }
        private void MruItemOpen(object sender, EventArgs e)
        {
            qbook.Core.ShowOpenQbookFileDialog(null);
        }
        private void MruItemClicked(object sender, EventArgs e)
        {
            ToolStripMenuItem clickedItem = (ToolStripMenuItem)sender;
            if (!System.IO.File.Exists(clickedItem.Name))
            {
                var dr = MessageBox.Show($"The file:\r\n\r\n{clickedItem.Name}\r\n\r\ncannot be found.\r\n\r\nRemove from List?", "QBOOK NOT FOUND", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (dr == DialogResult.Yes)
                {
                    qbook.Core.MruFilesManager.Remove(clickedItem.Name);
                    Properties.Settings.Default.MruFileList = qbook.Core.MruFilesManager.GetMruCsvString();
                    Properties.Settings.Default.Save();
                }
                return;
            }
            else
            {
                //MessageBox.Show($"nyi: {clickedItem.Name}", "OPEN");
                qbook.Core.OpenQbookAsync(clickedItem.Name);
                if (qbook.Core.ThisBook != null)
                    Bounds = qbook.Core.ThisBook.Bounds;
            }
        }

        async Task Save(object sender)
        {
            await Core.SaveThisBook();

            //qbook.Core.ThisBook.Serialize();
            //qbook.Core.ThisBook.Modified = false;
            //qbook.Properties.Settings.Default.Save();
        }
        void Home(object sender)
        {
            start = 0;
        }
        void Forward5(object sender)
        {
            start += 5;
            if (start > qbook.Core.ActualMain.Objects.Where(item => item is oPage).Count() - 10)
                start = qbook.Core.ActualMain.Objects.Where(item => item is oPage).Count() - 10;

            if (start < 0)
                start = 0;
        }
        void Back5(object sender)

        {
            start -= 5;
            if (start < 0)
                start = 0;
        }
        void End(object sender)
        {
            start = qbook.Core.ActualMain.Objects.Where(item => item is oPage).Count() - 10;
            if (start < 0)
                start = 0;
        }
        void EditSection(object sender)
        {
            qbook.Core.ThisBook.Modified = true;
            List<string> preSelections = new List<string>();
            preSelections.Add("abstract");
            preSelections.Add("description");
            preSelections.Add("handling");
            preSelections.Add("interface");
            preSelections.Add("plan");
            preSelections.Add("service");
            preSelections.Add("repair");
            preSelections.Add("info");
            preSelections.Add("qs");
            preSelections.Add("flyer");


            int x = Location.X + (sender as oIcon).e.X;
            int y = Location.Y + (sender as oIcon).e.Y + 25;
            x = Math.Min(x, Screen.PrimaryScreen.Bounds.Width - 362 - 20);
            y = Math.Min(y, Screen.PrimaryScreen.Bounds.Height - 266 - 40);

            StringForm.Edit(x, y, ref ((sender as oIcon).Parent as oPage).Section, preSelections);

        }


        async Task EditTextAsync(object sender)
        {
            //Main.Qb.Book.Modified = true; //HALE: why modified here? (i.e. already modified when opening script-editor)

            int x = Location.X + (sender as oIcon).e.X;
            int y = Location.Y + (sender as oIcon).e.Y + 25;
            x = Math.Min(x, Screen.PrimaryScreen.Bounds.Width - 900 - 20);
            y = Math.Min(y, Screen.PrimaryScreen.Bounds.Height - 500 - 40);


            if (QB.Book.AccessLevel >= QB.AccessLevel.Admin)
            {
                if (((sender as oIcon).Parent is oPage)
                    && ((ModifierKeys & Keys.Control) == Keys.Control))
                {
                    //old editor
                    EditObjectForm.Edit(x, y, (sender as oIcon).Parent);
                }
                else
                {
                    //new editor (MDI)

                    //oPage page = (sender as oIcon).Parent as oPage;

                    Core.ShowFormScintillaEditor((sender as oIcon).Parent as oPage);

           
                  //  Core.ShowFormCodeEditor((sender as oIcon).Parent as oPage);

                }
            }
        }

        void EditText(object sender)
        {
            Task.Run(async () =>
            {
                await EditTextAsync(sender);
            });
        }


        void SwitchFormat(object sender)
        {
            oPage page = qbook.Core.SelectedPage;

            switch (page.Format)
            {
                case "A4":
                    page.Format = "16/9";
                    break;
                case "16/9":
                    page.Format = "16/10";
                    break;
                case "16/10":
                    page.Format = "A4";
                    break;
            }

            qbook.Core.SelectedPage = page;
        }

        void EditName(object sender)
        {
            if (!qbook.Core.ThisBook.DesignMode)
                return;
            qbook.Core.ThisBook.Modified = true;


            int x = Location.X + (sender as oIcon).e.X;
            int y = Location.Y + (sender as oIcon).e.Y + 25;
            x = Math.Min(x, Screen.PrimaryScreen.Bounds.Width - 362 - 20);
            y = Math.Min(y, Screen.PrimaryScreen.Bounds.Height - 266 - 40);

            StringForm.Edit(x, y, ref (sender as oIcon).Parent.Name, null);


            if ((sender as oIcon).Parent.Name == "")
                qbook.Core.ThisBook.Main.Remove((sender as oIcon).Parent);
        }
        void EditFileName(object sender)
        {
            string oldFilename = qbook.Core.ThisBook.Filename;
            qbook.Core.ThisBook.Modified = true;

            //HACK: handle .qbook-extension
            string filename = qbook.Core.ThisBook.Filename;
            while (filename.EndsWith(".qbook"))
                filename = filename.Substring(0, filename.Length - ".qbook".Length);

            if (sender is oIcon) //from old commandbar
            {
                int x = Location.X + (sender as oIcon).e.X;
                int y = Location.Y + (sender as oIcon).e.Y + 25;
                x = Math.Min(x, Screen.PrimaryScreen.Bounds.Width - 362 - 20);
                y = Math.Min(y, Screen.PrimaryScreen.Bounds.Height - 266 - 40);
                StringForm.Edit(x, y, ref filename, null);
            }
            else
            {
                string newFilename = filename;
                var dr = QB.UI.InputDialog.ShowDialog("RENAME QBOOK", "Please enter the new name for this qbook:", ref newFilename);
                if (dr == DialogResult.OK)
                    filename = newFilename;
            }

            if (filename.EndsWith(".qbook"))
                filename = filename.Substring(0, filename.Length - ".qbook".Length);

            qbook.Core.ThisBook.Filename = filename + ".qbook";

            if (qbook.Core.ThisBook.Filename != oldFilename)
            {
                //add it to MRU (first entry)
                qbook.Core.MruFilesManager.Add(Path.Combine(qbook.Core.ThisBook.Directory, qbook.Core.ThisBook.Filename));
                Properties.Settings.Default.MruFileList = qbook.Core.MruFilesManager.GetMruCsvString();
                Properties.Settings.Default.Save();
            }
        }

        void OpenCsScriptEditor(object sender)
        {
            //open first page with code in it...
            var firstPageWithCode = qbook.Core.ThisBook.Main.Objects.FirstOrDefault(p => p.CsCode.Trim().Length > 0);
            if (firstPageWithCode != null)
            {
                //var editForm = EditObjectForm.Edit(0, 0, firstPageWithCode);
                qbook.Core.ShowFormCodeEditor(firstPageWithCode as oPage);
                Application.DoEvents();
                qbook.Core.FormCodeEditorRebuild(true);
            }
        }


        //void EditUser(object sender)
        //{

        //    Main.Qb.Book.Modified = true;
        //    string u = Main.Properties.Settings.Default.DefaultLayer;

        //    int x = Location.X + (sender as oIcon).e.X;
        //    int y = Location.Y + (sender as oIcon).e.Y + 25;
        //    x = Math.Min(x, Screen.PrimaryScreen.Bounds.Width - 362 - 20);
        //    y = Math.Min(y, Screen.PrimaryScreen.Bounds.Height - 266 - 40);

        //    StringForm.Edit(x, y, ref u, null);
        //    Main.Properties.Settings.Default.DefaultLayer = u;
        //    Main.Properties.Settings.Default.Save();
        //}
        void AddLayer(object sender)
        {
            qbook.Core.ThisBook.Modified = true;
            int lCount = qbook.Core.SelectedPage.Objects.Where(item => item is oLayer).ToList().Count;
            string text = (lCount == 1) ? qbook.Properties.Settings.Default.DefaultLayer : "L" + (lCount + 1);
            oLayer page = new oLayer(text, "");
            qbook.Core.SelectedPage.Add(page);
        }
        void AddPage(object sender)
        {

            qbook.Core.ThisBook.Modified = true;
            qbook.Core.ActualMain = (sender as oIcon).Parent;

            string newName = "p" + ((sender as oIcon).Parent.Objects.Where(item => item is oPage).ToList().Count + 1);
        _AddPageStart:
            if (DialogResult.OK == QB.UI.InputDialog.ShowDialog("Add New Page", "Page Name:", ref newName))
            {
                bool pageNameExists = false;
                foreach (oPage page in qbook.Core.ActualMain.Objects.OfType<oPage>())
                {
                    string newFullName = null;
                    int index = page.FullName.LastIndexOf(".");
                    if (index == -1)
                        newFullName = newName.Trim();
                    else
                        newFullName = page.FullName.Substring(0, index) + newName.Trim();
                    if (page.FullName == newFullName)
                    {
                        pageNameExists = true;
                        break;
                    }
                }
                if (pageNameExists)
                {
                    MessageBox.Show($"A page with the name '{newName}' already exists.\r\nPlease choose a different name."
                        , "PAGE NAME EXISTS", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    goto _AddPageStart;
                }

                oPage newPage = new oPage(newName, newName);
                int indx = (sender as oIcon).Parent.Objects.IndexOf(qbook.Core.SelectedPage);
                (sender as oIcon).Parent.Objects.Insert(indx + 1, newPage);
                qbook.Core.SelectedPage = newPage;

                //   Main.Qb.SelectedLayer.Add(newItem);
                qbook.Core.ThisBook.Modified = true;

                //   NewPageForm.Edit((sender as oIcon).Parent);

                /*

                      string text = "page " + ((sender as oIcon).Parent.Objects.Where(item => item is oPage).ToList().Count + 1);
                      oPage page = new oPage("", text);
                      int indx = (sender as oIcon).Parent.Objects.IndexOf(Main.Qb.SelectedPage);
                      (sender as oIcon).Parent.Objects.Insert(indx + 1, page);
                      Main.Qb.SelectedPage = page;*/
            }
        }
        //void CenterLayout(object sender)
        //{
        //    Main.Qb.Book.Modified = true;
        //    ((sender as oIcon).Parent as oPage).lItemWidth = Draw.DefaultItemWidth;
        //    ((sender as oIcon).Parent as oPage).rItemWidth = Draw.DefaultItemWidth;
        //}
        //void LeftLayout(object sender)
        //{
        //    Main.Qb.Book.Modified = true;
        //    ((sender as oIcon).Parent as oPage).lItemWidth = 20;
        //    ((sender as oIcon).Parent as oPage).rItemWidth = Draw.DefaultItemWidth * 2 - 20;
        //}
        void ToggleVisibility(object sender)
        {
            qbook.Core.ThisBook.Modified = true;
            (sender as oIcon).Parent.Visible = !(sender as oIcon).Parent.Visible;
        }
        void PageSelect(object sender)
        {
            if (qbook.Core.SelectedPage == (sender as oIcon).Parent as oPage)
            {
                (sender as oIcon).Parent.Selected = false;
                qbook.Core.SelectedPage = null;
            }
            else
            {
                //var oldPage = Main.Qb.SelectedPage;
                qbook.Core.SelectedPage = (sender as oIcon).Parent as oPage;
                //if (Main.Qb.SelectedPage != oldPage)
                //OnSelectedPageChanged(oldPage, Main.Qb.SelectedPage);
            }
        }

        public class SelectedPageChangedEventArgs : EventArgs
        {
            public oPage OldPage;
            public oPage NewPage;
        }
        public delegate void SelectedPageChangedEventHandler(SelectedPageChangedEventArgs e);
        public event SelectedPageChangedEventHandler SelectedPageChangedEvent;
        void OnSelectedPageChanged(oPage oldPage, oPage newPage)
        {
            if (SelectedPageChangedEvent != null)
            {
                SelectedPageChangedEventArgs ea = new SelectedPageChangedEventArgs() { OldPage = oldPage, NewPage = newPage };
                SelectedPageChangedEvent(ea);
            }
        }

        void LayerSelect(object sender)
        {
            qbook.Core.SelectedLayer = (sender as oIcon).Parent as oLayer;
        }
        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Right)
            //{

            //    return;
            //}

            try
            {
                lock (Icons)
                {
                    foreach (oIcon icon in new List<oIcon>(Icons))
                    {
                        icon.MouseDown(e);
                    }
                }
            }
            catch { }
        }



        static Version AssemblyVersion = null; // System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        static string AssemblyVersionString = null; // System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        static string AssemblyVersionStringEx = null; // System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        static string AssemblyBuildDateTimeString = "";
        private void MainForm_Load(object sender, EventArgs e)
        {
            rootToolStripMenuItem.Visible = false;
            debugToolStripMenuItem.Visible = Debugger.IsAttached;

            menuStrip.MouseEnter += MenuStrip_MouseEnter; //HACK: avoid necessity to click 2x after activation
            statusStrip.MouseEnter += StatusStrip_MouseEnter; //HACK: avoid necessity to click 2x after activation

            pageControl.BackColor = Color.Transparent;

            AssemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            AssemblyVersionString = $"{AssemblyVersion.Major}.{AssemblyVersion.Minor}.{AssemblyVersion.Build}";
            AssemblyVersionStringEx = AssemblyVersionString + "." + AssemblyVersion.Revision;
            //AssemblyBuildDateTimeString = (new DateTime(2000, 1, 1).AddDays(AssemblyVersion.Build).AddSeconds(AssemblyVersion.MinorRevision * 2)).ToString("yyyy-MM-dd") + "." + AssemblyVersion.MinorRevision;


            AssemblyBuildDateTimeString = (new DateTime(AssemblyVersion.Major, AssemblyVersion.Minor, AssemblyVersion.Build).AddSeconds(AssemblyVersion.MinorRevision * 2)).ToString("yyyy-MM-dd") + "." + AssemblyVersion.MinorRevision;

            //2025-04-06 stfu Workaround
            //if (AssemblyVersion.Major > 0 && AssemblyVersion.Minor > 0 && AssemblyVersion.Build > 0)
            //{
            //    AssemblyBuildDateTimeString = (new DateTime(AssemblyVersion.Major, AssemblyVersion.Minor, AssemblyVersion.Build).AddSeconds(AssemblyVersion.MinorRevision * 2)).ToString("yyyy-MM-dd") + "." + AssemblyVersion.MinorRevision;
            //}
            //else
            //{
            //    AssemblyBuildDateTimeString = "Invalid Version";
            //}

            QB.Book.AppVersion = AssemblyVersionString;
            QB.Book.AppVersionEx = AssemblyVersionStringEx;
            QB.Book.AppBuildDate = AssemblyBuildDateTimeString;

            ////Task.Run(() =>
            ////{
            //    var args = _ProgramArgs;
            //    //try to get a filename
            //    strin g directory = Path.Combine(GetApplicationPath(), "qbooks");
            //    string filename = DateTime.Now.ToString("yyyy-MM-dd") + " new.qbook";

            //    if ((args.Length == 1) && args[0].ToLower().EndsWith(".qbook")) //only on arg ening in .qbook
            //    {
            //        directory = Path.GetDirectoryName(args[0]);
            //        filename = Path.GetFileName(args[0]);
            //    }
            //    else if (args.Length > 1 && ExistsNamedArg(args, "-o|-open"))
            //    {
            //        string fullPath = GetNamedArgValue(args, "-o|-open");
            //        if (fullPath != null)
            //        {
            //            directory = Path.GetDirectoryName(fullPath);
            //            filename = Path.GetFileName(fullPath);
            //        }
            //    }
            //    else
            //    {
            //        //open first available qbook in MRU
            //        var mruItems = Main.Qb.MruFilesManager.MruItems; //.GetMruStringList();
            //        if (mruItems != null)
            //        {
            //            foreach (var item in mruItems)
            //            {
            //                if (System.IO.File.Exists(item.FullPath))
            //                {
            //                    directory = Path.GetDirectoryName(item.FullPath);
            //                    filename = Path.GetFileName(item.FullPath);
            //                    break;
            //                }
            //            }
            //        }
            //    }

            //    Main.Qb.SelectedPageChangedEvent += Qb_SelectedPageChangedEvent;

            //    if (directory != null && filename != null)
            //    {
            //        Main.Qb.OpenQbookAsync(Path.Combine(directory, filename));
            //        if (Main.Qb.Book != null)
            //            Bounds = Main.Qb.Book.Bounds;
            //    }

            this.timer100ms.Enabled = true;
            //});

            Application.DoEvents();
            InitMainForm();
            Application.DoEvents();
        }

        private void StatusStrip_MouseEnter(object sender, EventArgs e)
        {
            ToolStrip toolStrip = sender as ToolStrip;
            if (toolStrip != null)
            {
                bool existsActiveBoxEditor = Controls.OfType<System.Windows.Forms.Control>().Count(c => c.Name == "BoxEditor") > 0;
                if (toolStrip.CanFocus && !toolStrip.Focused && !existsActiveBoxEditor)
                    toolStrip.Focus();
            }
            base.OnMouseEnter(e);
        }

        private void MenuStrip_MouseEnter(object sender, EventArgs e)
        {
            ToolStrip toolStrip = sender as ToolStrip;
            if (toolStrip != null)
            {
                bool existsActiveBoxEditor = Controls.OfType<System.Windows.Forms.Control>().Count(c => c.Name == "BoxEditor") > 0;
                if (toolStrip.CanFocus && !toolStrip.Focused && !existsActiveBoxEditor)
                    toolStrip.Focus();
            }
            base.OnMouseEnter(e);
        }

        bool _IsFirstActivation = true;
        private async void MainForm_Activated(object sender, EventArgs e)
        {
            if (_IsFirstActivation)
            {
                _IsFirstActivation = false;

                if (false) //
                {
                    if (!(System.Windows.Forms.Control.ModifierKeys == (Keys.Control | Keys.Shift))) //don't auto build/run if ctrl-shift is pressed
                    {
                        try
                        {
                            await qbook.Core.CsScriptRebuildAll("Initializing");

                            if (Program.Args == null || !Program.Args.Contains("-norun"))
                                qbook.Core.RunCsScript_Run();
                        }
                        catch (Exception ex)
                        {
                            //MessageBox.Show(ex.Message, "--- ERRORS ---");
                        }
                    }
                }

                if (true)
                {
                    //InitMainForm();
                }

                //Main.Qb.RunCsScript();
            }
        }

        private void InitMainForm()
        {
            //MessageBox.Show("InitMainForm()");
            var args = _ProgramArgs;
            //try to get a filename
            string directory = Path.Combine(GetApplicationPath(), "qbooks");
            string filename = DateTime.Now.ToString("yyyy-MM-dd") + " new.qbook";

            if ((args.Length == 1) && args[0].ToLower().EndsWith(".qbook")) //only on arg ening in .qbook
            {
                directory = Path.GetDirectoryName(args[0]);
                filename = Path.GetFileName(args[0]);
            }
            else if (args.Length > 1 && ExistsNamedArg(args, "-o|-open"))
            {
                string fullPath = GetNamedArgValue(args, "-o|-open");
                if (fullPath != null)
                {
                    directory = Path.GetDirectoryName(fullPath);
                    filename = Path.GetFileName(fullPath);
                }
            }
            else
            {
                //open first available qbook in MRU
                var mruItems = qbook.Core.MruFilesManager.MruItems; //.GetMruStringList();
                if (mruItems != null)
                {
                    foreach (var item in mruItems)
                    {
                        if (System.IO.File.Exists(item.FullPath))
                        {
                            directory = Path.GetDirectoryName(item.FullPath);
                            filename = Path.GetFileName(item.FullPath);
                            break;
                        }
                    }
                }
            }

            qbook.Core.SelectedPageChangedEvent += Qb_SelectedPageChangedEvent;

            if (directory != null && filename != null)
            {
                qbook.Core.OpenQbookAsync(Path.Combine(directory, filename));
                if (qbook.Core.ThisBook != null)
                {
                    qbook.Core.ThisBook.PropertyChanged -= Book_PropertyChanged;
                    qbook.Core.ThisBook.PropertyChanged += Book_PropertyChanged;
                    if (Debugger.IsAttached)
                        QB.Book.AccessLevel = QB.AccessLevel.Admin;
                    UpdateMenuBar();
                    //@SCAN       Bounds = Main.Qb.Book.Bounds;
                }
            }
            this.timer100ms.Enabled = true;
        }

        private void toolStripMenuItemShowLog_Click(object sender, EventArgs e)
        {
            qbook.Core.ShowLogForm();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                toggleFullScreen();
            }

            if (e.KeyCode == Keys.F12)
            {
                TogglePageControlBar();
            }



        }

        private void designToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!qbook.Core.VerifyDeveloperLicense())
                return;

            qbook.Core.ThisBook.DesignMode = !qbook.Core.ThisBook.DesignMode;
            qbook.Core.ThisBook.TagMode = false;
            if (qbook.Core.ThisBook.DesignMode)
            {
                qbook.Core.ThisBook.Grid = 5;
                qbook.Core.ThisBook.Language = "en";
            }
            else
            {
                qbook.Core.ThisBook.Grid = 0;
            }
        }

        private void tagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.TagMode = !qbook.Core.ThisBook.TagMode;
            qbook.Core.ThisBook.DesignMode = false;
            if (qbook.Core.ThisBook.TagMode)
            {
                qbook.Core.ThisBook.Grid = 5;
                qbook.Core.ThisBook.Language = "en";
            }
            else
            {
                qbook.Core.ThisBook.Grid = 0;
            }
        }

        private void recorderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.Recorder = !qbook.Core.ThisBook.Recorder;
        }

        private void statusLabelName_Click(object sender, EventArgs e)
        {
            EditFileName(this);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewQbook(this);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

            ShowOpenQbookFileDialog(this);
            //MRU
        }

        private void openRecentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowMruList(this);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save(this);
        }

        private void pDFcurrentPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportPdf(this, false);
        }

        private void pDFallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportPdf(this, true);
        }

        private void enEnglishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.Language = "en";
        }

        private void deDeutschToolStripMenuItem_Click(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.Language = "de";
        }

        private void esEspanolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.Language = "es";
        }

        private void leftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.Modified = true;
            ((sender as oIcon).Parent as oPage).lItemWidth = 20;
            ((sender as oIcon).Parent as oPage).rItemWidth = Draw.DefaultItemWidth * 2 - 20;
        }

        private void centerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.Modified = true;
            ((sender as oIcon).Parent as oPage).lItemWidth = Draw.DefaultItemWidth;
            ((sender as oIcon).Parent as oPage).rItemWidth = Draw.DefaultItemWidth;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void menuItemGrid0_Click(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.Grid = 0;
        }

        private void menuItemGrid1_Click(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.Grid = 1;
        }

        private void menuItemGrid5_Click(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.Grid = 5;
        }

        private void menuItemGrid10_Click(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.Grid = 10;
        }

        private void menuItemScreenshot_Click(object sender, EventArgs e)
        {
            try
            {
                Screenshot(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuItemWebcam_Click(object sender, EventArgs e)
        {
            //var dr = MessageBox.Show("Ensure Webcam is attached and drivers are installed...", "WARNING");
            try
            {
                Webcam(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuItemOutlook_Click(object sender, EventArgs e)
        {
            try
            {
                Outlook(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuItemClipboard_Click(object sender, EventArgs e)
        {
            MessageBox.Show("nyi");
        }

        private void menuItemTemplate_Click(object sender, EventArgs e)
        {
            try
            {
                Template(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = qbook.Core.ThisBook.Directory;
            saveFileDialog1.Filter = "qbook (*.qbook)|*.qbook|All Files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 0;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string oldDirectory = qbook.Core.ThisBook.Directory;
                string oldFilename = qbook.Core.ThisBook.Filename;

                qbook.Core.ThisBook.Directory = Path.GetDirectoryName(saveFileDialog1.FileName);
                qbook.Core.ThisBook.Filename = Path.GetFileName(saveFileDialog1.FileName);
                Core.ThisBook.DataDirectory = null;
                Core.ThisBook.SettingsDirectory = null;
                Core.ThisBook.TempDirectory = null;
                StatusUpdateFilename();

                if (qbook.Core.ThisBook.Filename != oldFilename || qbook.Core.ThisBook.Directory != oldDirectory)
                {
                    //add it to MRU (first entry)
                    qbook.Core.MruFilesManager.Add(Path.Combine(qbook.Core.ThisBook.Directory, qbook.Core.ThisBook.Filename));
                    Properties.Settings.Default.MruFileList = qbook.Core.MruFilesManager.GetMruCsvString();
                    Properties.Settings.Default.Save();
                    await Core.SaveThisBook();
                }
            }
        }

        private void DropDown_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked || e.CloseReason == ToolStripDropDownCloseReason.AppFocusChange)
                e.Cancel = true;
        }

        private void ComPort_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            menuItem.Checked = !menuItem.Checked;
            string portName = menuItem.Text.Replace("[busy]", "").Trim();
            if (menuItem.Checked)
            {
                //Main.Qb.Net.Serial.ComPorts.Remove(portName);
                //Main.Qb.Net.Serial.ComPort newComPort = new Main.Qb.Net.Serial.ComPort() { PortName = portName, IsBusy = IsPortInUse(portName), Selected = true };
                //Main.Qb.Net.Serial.ComPorts.Add(portName, newComPort);
            }
            else
            {
                //Main.Qb.Net.Serial.ComPorts.Remove(portName);
            }
        }

        // Function to check if a COM port is in use
        static bool IsPortInUse(string portName)
        {
            try
            {
                using (var port = new SerialPort(portName))
                {
                    port.Open();
                    return false; // Port is available
                }
            }
            catch (UnauthorizedAccessException)
            {
                return true; // Port is in use
            }
            catch (IOException)
            {
                return true; // Port is in use
            }
        }

        private void cOMPortsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            //(sender as ToolStripItem).auto
            cOMPortsToolStripMenuItem.BackColor = Color.Transparent;
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            //menuItem.DropDown.AutoClose = false;
            menuItem.DropDown.Closing += new ToolStripDropDownClosingEventHandler(DropDown_Closing);

            menuItem.DropDownItems.Clear();

            // Get the list of available COM ports
            string[] availablePorts = SerialPort.GetPortNames();

            if (availablePorts.Length == 0)
            {
                menuItem.DropDownItems.Add(new ToolStripMenuItem($"### No COM Ports") { Enabled = false });
                return;
            }

            foreach (string portName in availablePorts)
            {
                bool inUse = IsPortInUse(portName);
                ToolStripMenuItem newItem = new ToolStripMenuItem($"{portName}{(inUse ? " [busy]" : "")}");
                newItem.Click += ComPort_Click;
                //newItem.CheckOnClick = true;
                newItem.Enabled = !inUse;
                menuItem.DropDownItems.Add(newItem);
            }
        }

        private void toolStripUserOnline_Click(object sender, EventArgs e)
        {
            QB.Book.AccessLevel = QB.AccessLevel.Online;
        }

        private void toolStripUserOffline_Click(object sender, EventArgs e)
        {
            QB.Book.AccessLevel = QB.AccessLevel.Offline;
        }

        private void toolStripUserUser_Click(object sender, EventArgs e)
        {
            //password if currently "online", else switch directly
            if (QB.Book.AccessLevel == QB.AccessLevel.Online)
            {
                if (CheckPassword("Enter USER Password", "PASSWORD", qbook.Core.ThisBook.PasswordUser ?? "user"))
                    QB.Book.AccessLevel = QB.AccessLevel.User;
                else
                    MessageBox.Show("Incorrect Password", "PASSWORD", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            else
            {
                QB.Book.AccessLevel = QB.AccessLevel.User;
            }
        }

        private void toolStripUserService_Click(object sender, EventArgs e)
        {
            //password if currently <"service", else switch directly
            if (QB.Book.AccessLevel < QB.AccessLevel.Service)
            {
                if (CheckPassword("Enter SERVICE Password", "PASSWORD", qbook.Core.ThisBook.PasswordService ?? "service"))
                    QB.Book.AccessLevel = QB.AccessLevel.Service;
                else
                    MessageBox.Show("Incorrect Password", "PASSWORD", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            else
            {
                QB.Book.AccessLevel = QB.AccessLevel.Service;
            }
        }

        private void toolStripUserAdmin_Click(object sender, EventArgs e)
        {
            if (System.Windows.Forms.Control.ModifierKeys == Keys.Control)
            {
                if (CheckPassword("Enter ROOT Password", "PASSWORD", "root#"))
                    QB.Book.AccessLevel = QB.AccessLevel.Root;
                else
                    MessageBox.Show("Incorrect Password", "PASSWORD", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            else
            {
                //password if currently <"service", else switch directly
                if (QB.Book.AccessLevel < QB.AccessLevel.Admin)
                {
                    if (CheckPassword("Enter ADMIN Password", "PASSWORD", qbook.Core.ThisBook.PasswordAdmin ?? "admin"))
                        QB.Book.AccessLevel = QB.AccessLevel.Admin;
                    else
                        MessageBox.Show("Incorrect Password", "PASSWORD", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
                else
                {
                    QB.Book.AccessLevel = QB.AccessLevel.Admin;
                }
            }
        }

        private void toolStripUserRoot_Click(object sender, EventArgs e)
        {
            //invisible - should not be called...
        }

        bool CheckPassword(string text, string caption, string password)
        {
            string input = "";
            if (DialogResult.OK == QB.UI.InputDialog.ShowDialog(caption, text, ref input, passwordChar: '*'))
            {
                if (input == password)
                    return true;
            }
            return false;
        }

        private void setPasswordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetPasswordsForm f = new SetPasswordsForm();
            f.ShowDialog();
        }

        private void openQbookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(qbook.Core.ThisBook.Directory, qbook.Core.ThisBook.Filename);
            if (File.Exists(path))
            {
                string args = string.Format("/e, /select, \"{0}\"", path);
                Process.Start("explorer.exe", args);
            }
            else
            {
                MessageBox.Show("cannot find\r\n" + path, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void openDataDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", qbook.Core.ThisBook.DataDirectory);
        }

        private void openSettingsDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", qbook.Core.ThisBook.SettingsDirectory);
        }

        private void openTempDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", qbook.Core.ThisBook.TempDirectory);
        }

        private void toolStripStatusLabelKeyboard_Click(object sender, EventArgs e)
        {
            try
            {
                Program.OpenTouchKeyboard();
                if (!Program.ToggleTabTip())
                {
                    MessageBox.Show("Cannot open Touch-Keyboard");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot open Touch-Keyboard (2)");
            }
        }

        private void tempDisableUDLTimeoutToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            QB.Global.TempDisableUdlTimeout = tempDisableUDLTimeoutToolStripMenuItem.Checked;
        }

        private void tempDisablePdoUploadForId0ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            QB.Global.TempDisablePdoUploadForId0 = tempDisablePdoUploadForId0ToolStripMenuItem.Checked;
        }

        //string LicenseString = null;
        internal static string LicenseString = null;

        internal static string GetLicense(string key)
        {
            var m = System.Text.RegularExpressions.Regex.Match(LicenseString, @".*" + key + @"(.*?),");
            if (m.Success)
            {
                DateTime expireDate = DateTime.MinValue;
                return m.Groups[1].Value;
            }
            return null;
        }

        internal static DateTime? GetLicenseExpires(string key)
        {
            try
            {
                string value = GetLicense(key);
                if (value == "F")
                    return DateTime.MaxValue; //never
                else
                    return DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            catch { }
            return null;
        }


        bool LicenseChecked = false;
        bool LicenseCheckPending = false;
        bool BadLicenseDialogIsShowing = false;
        bool ForceUpdateFormTitle = false;
        private void timerIdle_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now.Second == 42 || !LicenseChecked || ForceUpdateFormTitle)
                UpdateFormTitle();

            //HACK: this should not be necessary
            if (toolStripUser.Text != QB.Book.AccessLevel.ToString())
                Book_PropertyChanged(this, new Book.PropertyChangedEventArgs() { Property = "AccessLevel", Value = null });
        }

        void UpdateFormTitle()
        {
            ForceUpdateFormTitle = false;
            string title = "qbook";
            if (qbook.Core.ThisBook != null)
                title = $"{qbook.Core.ThisBook.Filename}-{qbook.Core.SelectedPage.TextL} (v{AssemblyVersionStringEx})";
            else
                title = $"qbook (v{AssemblyVersionStringEx})";

            //TEMP LICENSE-HACK:
            bool DISABLE_LICENSE_CHECK = true; //2023-08-xx: Audi Ingostadt: issues when moving to another virtual-server
            if (VEXhubCommon.Licensing.EnableLicense2)
            {
                if (DISABLE_LICENSE_CHECK)
                    LicenseChecked = true;

                int licensePosInText = title.IndexOf(" [");
                if (licensePosInText >= 0)
                    title = title.Substring(0, licensePosInText);
                if (!LicenseChecked)
                {
                    this.Text = title + $" [CHECKING LICENSE...]";

                    if (!LicenseCheckPending)
                    {
                        LicenseCheckPending = true;
                        Task.Run(() =>
                        {
                            //call async, as checking USB may take some seconds
                            string LicenseSource = "";
                            string licenseFilename = $@"..\amium.{Environment.MachineName}.lic";
                            if (!File.Exists(licenseFilename))
                                licenseFilename = @"..\amium.lic"; //default
                            if (!File.Exists(licenseFilename))
                            {
                                MessageBox.Show("Cannot find a Product-License.\r\nPlease contact Amium for a Product-License!", "ERROR: NO LICENSE");
                                this.Invoke((MethodInvoker)(() =>
                                {
                                    _FormClosingNoUserInteraction = true;
                                    this.Close();
                                }));
                            }

                            log.Debug("checking license...");
                            LicenseString = Amium.Helpers.License2.GetLicense(licenseFilename, out LicenseSource);
                            log.Debug("checking license... done!");
                            System.Threading.Thread.Sleep(5000); //give the impression of a more complicated license-check
                            LicenseChecked = true;
                            LicenseCheckPending = false;

                            //MessageBox.Show(LicenseString == null ? "null" : LicenseString, "License: " + LicenseSource);
                            ForceUpdateFormTitle = true;
                        });
                    }
                }
                else //if (LicenseChecked) //(DateTime.Now - Process.GetCurrentProcess().StartTime).Seconds > 30)
                {
                    if (DISABLE_LICENSE_CHECK)
                    {
                        log.Debug("verifying license (check disabled)");
                        this.Text = title + $" [LICENSED]";
                    }
                    else
                    {
                        log.Debug("verifying license...");
                        if (LicenseString != null)
                        {
                            var m = System.Text.RegularExpressions.Regex.Match(LicenseString, @".*qbookC:(.*?),");
                            if (m.Success)
                            {
                                DateTime expireDate = DateTime.MinValue;
                                if (m.Groups[1].Value == "F")
                                {
                                    this.Text = title + $" [LICENSED]";
                                    ///*TEMP*/MessageBox.Show("[LICENSED]");
                                }
                                else if (DateTime.TryParseExact(m.Groups[1].Value, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out expireDate))
                                {
                                    int expiresInDays = (int)(expireDate - DateTime.Now).TotalDays;
                                    ///*TEMP*/MessageBox.Show("[Expires in " + expiresInDays);
                                    if (expiresInDays < 0 && !BadLicenseDialogIsShowing)
                                    {
                                        this.Text = title + $" [EXPIRED {-expiresInDays} days ago]";
                                        BadLicenseDialogIsShowing = true;
                                        MessageBox.Show("Software license expired!\r\n\r\nPlease contact Amium for an updated Product-License!", "ERROR: LICENSE EXPIRED", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        BadLicenseDialogIsShowing = false;
                                        this.Close();
                                    }
                                    else
                                    {
                                        this.Text = title + $" [EXPIRES in {expiresInDays} days]";
                                    }
                                    //this.Close();
                                }
                                else
                                {
                                    this.Text = title + $" [LICENSE ???]";
                                }
                            }
                        }
                        else
                        {
                            this.Text = title + $" [BAD LICENSE]";
                            if (!BadLicenseDialogIsShowing)
                            {
                                BadLicenseDialogIsShowing = true;
                                MessageBox.Show("Software not licensed.\r\n\r\nPlease contact Amium for a Product-License!", "ERROR: BAD LICENSE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                BadLicenseDialogIsShowing = false;
                            }
                            this.Close();
                        }
                        log.Debug("verifying license... done!");
                    }
                }
                return;
            }
            this.Text = title;
        }

        private void aboutQbookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutDialog dlg = new AboutDialog();
            dlg.Show(this);
        }

        static ToolStripStatusLabel _staticStatusLabel = null;
        static System.Windows.Forms.Timer statusTextResetTimer = null;
        internal static void SetStatusText(string text, int timeout = 0)
        {
            if (_staticStatusLabel == null)
                return;

            _staticMainForm.Invoke((MethodInvoker)(() =>
            {
                _staticStatusLabel.Text = text;
                if (text.Contains("#E"))
                {
                    _staticStatusLabel.BackColor = Color.Red;
                    _staticStatusLabel.ForeColor = Color.White;
                }
                else if (text.Contains("#W"))
                {
                    _staticStatusLabel.BackColor = Color.Orange;
                    _staticStatusLabel.ForeColor = Color.Black;
                }
                else
                {
                    _staticStatusLabel.BackColor = Color.Transparent;
                    _staticStatusLabel.ForeColor = Color.Black;
                }
            }));

            if (statusTextResetTimer == null)
            {
                statusTextResetTimer = new System.Windows.Forms.Timer();
                statusTextResetTimer.Tick += StatusTextResetTimer_Tick;
            }
            if (timeout > 0)
            {
                statusTextResetTimer.Interval = timeout;
                statusTextResetTimer.Enabled = true;
            }
            else
            {
                statusTextResetTimer.Enabled = false;
            }
        }

        private static void StatusTextResetTimer_Tick(object sender, EventArgs e)
        {
            statusTextResetTimer.Enabled = false;
            SetStatusText("Ready");
        }

        private void statusLabelStatus_Click(object sender, EventArgs e)
        {
            if (_staticStatusLabel.Text == "#ERR: build error(s)")
            {
                qbook.Core.ShowFormCodeEditor(null);
            }
        }

        private void openLogFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(qbook.Core.ThisBook.LogFilename);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot open file '{qbook.Core.ThisBook.LogFilename}'\r\n\r\nError: " + ex.Message, "ERROR OPENING LOG-FILE");
            }
        }

        DebugConsole debugConsole = null;
        private void debugConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (debugConsole == null || debugConsole.IsDisposed || !debugConsole.IsHandleCreated)
            {
                debugConsole = new DebugConsole();
            }
            debugConsole.Show();
            debugConsole.BringToFront();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            ratio = 29.7f / 16.7f; // 16/9
            ResizePageControl();
            qbook.Core.SelectedPage.Format = "16/9";
        }

        private void pageControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TogglePageControlBar();
            //showPageControl = !showPageControl;
            //PageControlBar = showPageControl?300:0;
            //// pageControl.Dock = DockStyle.Fill;
            //ResizePageControl();


        }

        private void a4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ratio = 29.7f / 21.0f;
            ResizePageControl();
            qbook.Core.SelectedPage.Format = "A4";

        }

        private void fullScreenF11ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetFullScreen();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            ratio = 29.7f / 18.56f; // 16/10
            ResizePageControl();
            qbook.Core.SelectedPage.Format = "16/10";
        }

        private void showLogCtrlLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            qbook.Core.ShowLogForm();
        }


        public bool HidPageMenuBar
        {
            get { return PageControlBar == 0; }
            set
            {
                if (value)
                    hidePageControlBar();
                else
                    showPageControlBar();

            }
        }


        public void TogglePageControlBar()
        {
            if (HidPageMenuBar)
                showPageControlBar();
            else
                hidePageControlBar();
        }

        void hidePageControlBar()
        {
            showPageControl = false;
            PageControlBar = 0;
            ResizePageControl();
        }
        void showPageControlBar()
        {

            showPageControl = true;
            PageControlBar = 300;
            ResizePageControl();
        }



        public bool FullScreen
        {
            get { return isFullScreen; }
            set
            {
                if (value)
                    SetFullScreen();
                else
                    ResetFullScreen();
            }

        }

        bool isFullScreen = false;
        public void SetFullScreen()
        {
            //Workaround sometimes Display bug
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Normal;
            //

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            menuStrip.Visible = false;
            statusStrip.Visible = false;
            isFullScreen = true;
        }
        public void ResetFullScreen()
        {
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Normal;
            menuStrip.Visible = true;
            statusStrip.Visible = true;
            isFullScreen = false;
        }


        bool fullScreenMenuVisible = false;

        FullScreenMenu menuFullscreen;
        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {

            if (!isFullScreen) return;


            if (e.Y <= 0 && e.X <= 0)
            {
                if (menuFullscreen == null)
                {
                    menuFullscreen = new FullScreenMenu();

                    menuFullscreen.Show(this);
                    menuFullscreen.AutoScaleMode = AutoScaleMode.None;
                    menuFullscreen.Size = new Size(108, 36);
                    fullScreenMenuVisible = true;
                }
            }

            if (e.Y > 60)
            {
                if (menuFullscreen != null)
                {
                    menuFullscreen.Close();
                    menuFullscreen = null;
                }
            }
        }

        private void pageControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (menuFullscreen == null) return;

            if (e.Y > 60)
            {
                menuFullscreen.Close();
                menuFullscreen = null;
            }
        }

        private void startFullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.StartFullScreen = !qbook.Core.ThisBook.StartFullScreen;
            startFullToolStripMenuItem.Checked = qbook.Core.ThisBook.StartFullScreen;
          //  startFullToolStripMenuItem.BackColor = qbook.Core.ThisBook.StartFullScreen ? Color.Orange : Color.Transparent;


        }

        private void startWithHiddenPagebarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            qbook.Core.ThisBook.HidPageMenuBar = !qbook.Core.ThisBook.HidPageMenuBar;
            startWithHiddenPagebarToolStripMenuItem.Checked = qbook.Core.ThisBook.HidPageMenuBar;
           // startWithHiddenPagebarToolStripMenuItem.BackColor = qbook.Core.ThisBook.HidPageMenuBar ? Color.Orange : Color.Transparent;
        }

        public void UpdateStartMenuItems()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateStartMenuItems));
                return;
            }

            startWithHiddenPagebarToolStripMenuItem.Checked = qbook.Core.ThisBook.HidPageMenuBar;
            startFullToolStripMenuItem.Checked = qbook.Core.ThisBook.StartFullScreen;
           
        }

        private void pagebarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HidPageMenuBar = !HidPageMenuBar;
        }

        private void fullscreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FullScreen = true;
        }

        private void logToolStripMenuItem_Click(object sender, EventArgs e)
        {
            qbook.Core.ShowLogForm();
        }
        

        private void codeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string program = "";

            program += "namespace QB\r\n{\r\n";
            program += "   public static class Program {\r\n";


            string uri = Path.Combine(@"T:\pageCode", qbook.Core.ThisBook.Filename.Replace(".qbook",""));
            Directory.CreateDirectory(uri);

            Directory.CreateDirectory(Path.Combine(uri,"dlls"));

            int pageCount = 0;
            foreach (oPage page in qbook.Core.ActualMain.Objects.Where(item => item is oPage))
            {
                pageCount++;
                string code = page.CsCode;

                int index = code.IndexOf("public class");
                Debug.WriteLine("index= " + index);
                string insertText = "\r\nnamespace class_" + page.Name + "\r\n" + "{" + "\r\n";
                code = code.Insert(index-1, insertText);
                code += "\r\n}";

                string className = "class_" + page.Name + ".@class_" + page.Name;
                program += "   public static " + className + " " + page.Name +  " { get;} = new " + className +"();\r\n";


                Directory.CreateDirectory(Path.Combine(uri, pageCount + "_" + page.Name));

                File.WriteAllText(Path.Combine(uri, pageCount + "_" + page.Name, "0_class_" + page.Name + ".cs"), code);


                string global = "global using static QB.Program;\r\n";
          
                File.WriteAllText(Path.Combine(uri, "GlobalUsing.cs"), global);



            var lines = code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                var usings = lines
                .TakeWhile(l => !l.TrimStart().StartsWith("public class"))
                .Where(l => l.TrimStart().StartsWith("using"))
                .ToList();

                int subCount = 0;
                foreach (var subClass in page.CsCodeExtra)
                {
                    subCount++;
                    string sub = "";

                    foreach (string use in usings)
                    {
                        sub += $"{use}\r\n";
                    }
                    
                    sub += "\r\nnamespace class_"+ page.Name +  "\r\n{\r\n\r\n";
           
                    sub += subClass.Value;
                  

                    sub += "\r\n}";

                    File.WriteAllText(Path.Combine(uri, pageCount + "_" + page.Name, subCount + "_"+ subClass.Key + ".cs"), sub);

                  

                }
          
            }

            program += "   }\r\n}";
            File.WriteAllText(Path.Combine(uri, "Program.cs"), program);

            string root = CsprojGenerator.GetSolutionDirectory();

            string csprojXml = GenerateCsprojWithAbsolutePaths(root, uri);
            File.WriteAllText(Path.Combine(uri, "Generated.csproj"), csprojXml);

        }
        public static string GenerateCsprojWithAbsolutePaths(string root, string uri)
        {
            string projectRoot = uri;
            string projectDlls = Path.Combine(projectRoot, "dlls");
            string baseDir = Path.Combine(root, "qbookCsScript","bin","Debug");

            var dllFiles = Directory.GetFiles(baseDir, "*.dll")
                .Concat(Directory.Exists(projectDlls) ? Directory.GetFiles(projectDlls, "*.dll") : Array.Empty<string>())
                .ToArray();

            string[] exclude = { "Microsoft.CodeAnalysis" };

            var referenceItems = dllFiles
                .Where(path =>
                {
                    string fileName = Path.GetFileNameWithoutExtension(path);
                    return !exclude.Any(prefix => fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
                })
                .Select(path =>
                {
                    string fileName = Path.GetFileNameWithoutExtension(path);
                    return $@"
    <Reference Include=""{fileName}"">
      <HintPath>{path}</HintPath>
    </Reference>";
                });

            // WICHTIG: Alle C#-Dateien wie in deinem Build einbinden
            // (wildcards decken Unterordner ab)
            var compileItems = $@"
    <Compile Include=""{uri}\**\*.cs"" />";

            return $@"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <EnableDefaultItems>false</EnableDefaultItems>
  </PropertyGroup>

  <ItemGroup>
{compileItems}
  </ItemGroup>

  <ItemGroup>
{string.Join("\n", referenceItems)}
  </ItemGroup>
</Project>";
        }

        private void exportToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            qbook.Core.ShowFormCodeEditor(Core.SelectedPage);
        }

        private async void newToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            await Core.OpenBookFolderAsync();
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Core.ThisBook.PageOrder.Reverse();
        }
    }


}
