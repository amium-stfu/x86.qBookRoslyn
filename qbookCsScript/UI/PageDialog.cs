using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QB.UI
{
    public partial class PageDialog : Form
    {
        public static dynamic MyQbook = null;

        public PageDialog()
        {
            InitializeComponent();

            //var qbookAssembly = System.Reflection.Assembly.GetEntryAssembly();
            //var pageControl = qbookAssembly.CreateInstance("Main.PageControl");
            ////object pageControl = Activator.CreateInstance(("qbook", "Main.PageControl");
            //if (pageControl != null)
            //{
            //    //string pageTypeName = pageControl.GetType().Name;
            //    //Console.WriteLine("pageTypeName=" + pageTypeName);
            //    pageControl.GetType().GetField("Page").SetValue(pageControl, MyPage);
            //    //pageControl.GetType().GetField("Popout").SetValue(pageControl, true);
            //    //pageControl.GetType().GetField("Dialog").SetValue(pageControl, true);
            //    pageControl.GetType().GetField("Dialog").SetValue(pageControl, true);
            //    //PageX = PageX ?? 10.0;
            //    //PageY = PageY ?? 20.0;
            //    //PageW = PageW ?? 297.0 - 2*10.0;
            //    //PageH = PageH ?? 210.0 - 2*20.0;
            //    //QB.Rectangle renderBounds = new QB.Rectangle((double)PageX, (double)PageY, (double)PageW, (double)PageH);
            //    //pageControl.GetType().GetField("RenderBounds").SetValue(pageControl, renderBounds);
            //    (pageControl as Control).Dock = DockStyle.Fill;
            //    panelPage.Controls.Add(pageControl as Control);
            //}
        }

        public string Result { get; set; } = null;
        public string Title { get => this.Text; set => this.Text = value; }

        public object MyPage;
        public double? PageX = null;
        public double? PageY = null;
        public double? PageW = null;
        public double? PageH = null;

        public Guid Id { get; set; }
        public bool IsModal = false;


        public string ButtonList
        {
            set
            {
                List<string> buttonList = value?.Split(',').Select(p => p.Trim()).ToList();
                if (buttonList != null)
                {
                    foreach (string button in buttonList)
                    {
                        Button b = new Button();
                        string[] splits = System.Text.RegularExpressions.Regex.Split(button, @"(?<!\\):");
                        b.Text = splits[0].Replace("\\:", ":");
                        if (splits.Length > 1)
                            b.Tag = splits[1];
                        else
                            b.Tag = splits[0];
                        b.Click += button_Click;
                        flowLayoutPanelButtons.Controls.Add(b);
                    }
                }
            }
        }



        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        void SetPageControl(Control pageControl)
        {
            panelPage.Controls.Add(pageControl as Control);
        }
        void SetPageControlSize(double w, double h, double scale = 1.0) //in mm
        {
            int panelWidth = (int)(w * 297.0 / 80.0 * scale);
            int panelHeight = (int)(h * /*210.0*/297.0 / 80.0 * scale);
            this.Width = panelWidth + 26;
            this.Height = panelHeight + 76; // 88;
            //panelPage.Width = panelWidth;
            //panelPage.Height = panelHeight;
        }


        static System.Collections.Generic.Dictionary<Guid, QB.UI.PageDialog> DialogDict = new System.Collections.Generic.Dictionary<Guid, QB.UI.PageDialog>();
        //public static async System.Threading.Tasks.Task<DialogResult> ShowDialog(string title, string pageName, double? x = null, double? y = null, double? w = null, double? h = null, double? scale = 1.0)

        public static string ShowDialog(string title, string pageName, double? x = null, double? y = null, double? w = null, double? h = null, double? scale = 1.0, string buttons = "OK, Cancel")
        {
            //TODO: this seems over-complicated?!
            //but: we need the thread to be STA (or Task to use FromCurrentSynchronizationContext())
            try
            {
                Guid id = Guid.Empty;
                Task reportTask = Task.Factory.StartNew(
                    () =>
                    {
                        id = Show(title, pageName, x, y, w, h, scale, true, buttons);
                    }
                    , CancellationToken.None
                    , TaskCreationOptions.None
                    , TaskScheduler.FromCurrentSynchronizationContext()
                    );
                reportTask.Wait();
                lock (DialogDict)
                {
                    if (DialogDict.ContainsKey(id))
                    {
                        string result = DialogDict[id].Result;
                        if (DialogDict.ContainsKey(id))
                            DialogDict.Remove(id);
                        return result;
                    }
                }
                return null;
            }
            catch (AggregateException ex)
            {
                foreach (var exception in ex.InnerExceptions)
                {
                    throw ex.InnerException;
                }
            }
            return null;
        }

        //public static async System.Threading.Tasks.Task<string> ShowDialogAsync(string title, string pageName, double? x = null, double? y = null, double? w = null, double? h = null, double? scale = 1.0, string buttons = "OK, Cancel")
        //{
        //    Guid id = Show(title, pageName, x, y, w, h, scale, true, buttons);

        //    //DialogResult dr = await WaitForDialogClose(id);

        //    while (DialogDict.ContainsKey(id) && DialogDict[id].Result == null) //DialogResult.None)
        //        await Task.Delay(50);
        //    //DialogResult dr = DialogDict[id];
        //    string result = DialogDict[id].Result;

        //    //DialogResult dr = DialogResult.None;
        //    lock (DialogDict)
        //    {
        //        if (DialogDict.ContainsKey(id))
        //        {
        //            DialogDict.Remove(id);
        //        }
        //    }

        //    return result ;
        //}
        public static Guid Show(string title, string pageName, double? x = null, double? y = null, double? w = null, double? h = null, double? scale = 1.0, bool modal = false, string buttons = null)
        {


            //Item myPage = new Item("$pageDialog."+Guid.NewGuid().ToString());
            QB.UI.PageDialog dialog = new QB.UI.PageDialog();
            dialog.Id = Guid.NewGuid();
            dialog.IsModal = modal;
            dialog.ButtonList = buttons;
            DialogDict.Add(dialog.Id, dialog); // DialogResult.None);

            //Item myPage = null;

            foreach (var o in Item.MyQbook.Main.Objects)
            {
                try
                {
                    if (o.GetType().Name == "oPage")
                    {
                        if (o.FullName == pageName)
                        {
                            //var page = o as Main.oPage;
                            //_renderMethod = o.GetType().GetMethod("Render");
                            //renderMethod.Invoke(o, new object[] { });
                            dialog.MyPage = o;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("#EX in PageDialog.Show: " + ex.Message);
                }
            }
            if (dialog.MyPage == null)
            {
                //ERROR: 
                return Guid.Empty; // DialogResult.None;
            }

            dialog.PageX = x;
            dialog.PageY = y;
            dialog.PageW = w;
            dialog.PageH = h;

            var qbookAssembly = System.Reflection.Assembly.GetEntryAssembly();
            var pageControl = qbookAssembly.CreateInstance("qbook.PageControl");
            //object pageControl = Activator.CreateInstance(("qbook", "Main.PageControl");

            double defaultX = 10.0;
            double defaultY = 20.0;
            double defaultW = 297.0 - 10.0 - 10.0;
            double defaultH = 210.0 - 20.0 - 10.0;

            if (pageControl != null)
            {
                //pageControl.GetType().GetProperty("Dialog").SetValue(pageControl, true);
                //pageControl.GetType().GetField("Page").SetValue(pageControl, dialog.MyPage);
                pageControl.GetType().GetProperty("DialogPage").SetValue(pageControl, dialog.MyPage);
                (pageControl as Control).Dock = DockStyle.Fill;
                dialog.SetPageControl(pageControl as Control);
                dialog.SetPageControlSize(w ?? defaultW, h ?? defaultH, scale ?? 1.0);
            }

            QB.Rectangle renderBounds = new QB.Rectangle(
                (double)(x ?? defaultX),
                (double)(y ?? defaultY),
                (double)(w ?? defaultW),
                (double)(h ?? defaultH));
            pageControl?.GetType().GetField("RenderBounds").SetValue(pageControl, renderBounds);

            dialog.Title = title; // "CELL VALUE";
            //dialog.MyPage = myPage;

            dialog.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            var parentForm = System.Windows.Forms.Application.OpenForms[0];
            dialog.Location = new System.Drawing.Point(parentForm.Left + (parentForm.Width - dialog.Width) / 2, parentForm.Top + (parentForm.Height - dialog.Height) / 2);

            //var dr = dialog.ShowDialog();
            if (modal)
            {
                //dialog.FormClosing += Dialog_FormClosing;
                dialog.ShowDialog(Application.OpenForms[0]);
                dialog.BringToFront();
            }
            else
            {
                dialog.Show();
                dialog.BringToFront();
                //return DialogResult.None;
            }
            return dialog.Id;
        }

        private void button_Click(object sender, EventArgs e)
        {
            Button b = sender as Button;
            Result = b.Tag.ToString();
            this.Close();
        }

        private static void Dialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            QB.UI.PageDialog dialog = sender as QB.UI.PageDialog;
            if (dialog != null)
            {
                if (DialogDict.ContainsKey(dialog.Id))
                {
                    if (DialogDict[dialog.Id].Result == null)
                        DialogDict[dialog.Id].Result = "Cancel";
                }
            }
        }

        MethodInfo _renderMethod = null;
        private void panelPage_Paint(object sender, PaintEventArgs e)
        {
            return;

            if (MyPage != null)
            {
                if (_renderMethod == null)
                    _renderMethod = MyPage.GetType().GetMethod("Render");

                if (_renderMethod != null)
                    _renderMethod.Invoke(MyPage, new object[] { });
            }
        }

        private void timerRender_Tick(object sender, EventArgs e)
        {
            if (panelPage.Controls.Count > 0)
                panelPage.Controls[0].Invalidate();
        }
    }
}
