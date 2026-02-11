using CefSharp;
using CefSharp.Dom;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//using Main.Qb.Controls;
using QB; //qbookCsScript

namespace qbook
{
    [Serializable]
    public class cHtml : UserControl
    {

        ChromiumWebBrowser browser = null;
        DevToolsContext devtoolsContext = null;
        System.Windows.Forms.Timer timerRender;

        oHtml MyHtmlItem = null;

        static cHtml()
        {
            CefSharpSettings.WcfEnabled = false;
            var settings = new CefSettings();
            settings.IgnoreCertificateErrors = true;
            settings.PersistSessionCookies = false;
            //settings.abc = ...
            //settings.xyz = ...
            if (false) //some example settings
            {
                settings.CefCommandLineArgs.Add("--disable-features", "FormControlsRefresh,IntensiveWakeUpThrottling");
                settings.CefCommandLineArgs.Add("no-proxy-server", "1");
                settings.CefCommandLineArgs.Add("v8-cache-options", "code");
                settings.CefCommandLineArgs.Add("disable-touch-adjustment", "1");
                settings.CefCommandLineArgs.Add("plugin-policy", "block");
                settings.CefCommandLineArgs.Add("disable-backgrounding-occluded-windows");
                settings.CefCommandLineArgs.Remove("disable-site-isolation-trials");

                settings.MultiThreadedMessageLoop = true;
                settings.ExternalMessagePump = false;
                settings.JavascriptFlags = "--expose-gc --allow-natives-syntax";
            }

            if (false) //disable GPU
            {
                settings.CefCommandLineArgs.Add("disable-gpu", "1");
                settings.CefCommandLineArgs.Add("disable-gpu-vsync", "1");
                settings.CefCommandLineArgs.Add("disable-gpu-compositing", "1");
            }

            bool ok = Cef.Initialize(settings, true);
            if (!ok)
            {
                MessageBox.Show("Could not initialize CefSharp", "ERROR");
            }
        }

        public class CustomMenuHandler : CefSharp.IContextMenuHandler
        {
            cHtml Owner = null;
            public CustomMenuHandler(cHtml owner)
            {
                Owner = owner;
            }
            public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
            {
                model.Clear();
                Owner?.ShowEditForm();
            }
            public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
            {
                return false;
            }
            public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
            {
            }
            public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
            {
                return false;
            }
        }

        public class CustomKeyboardHandler : IKeyboardHandler
        {
            cHtml Owner = null;
            public CustomKeyboardHandler(cHtml owner)
            {
                Owner = owner;
            }
            public bool OnKeyEvent(IWebBrowser browserControl, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey)
            {
                //bool handled = Owner.OnKeyEvent(windowsKeyCode, nativeKeyCode, modifiers, isSystemKey);
                //if (handled)
                //    return true;

                return true;
                /*
                if (windowsKeyCode == 0x0D)
                {
                    //string script = @"(function(){
                    //            return 1 + 1;
                    //            })();";
                    //var task = browserControl.EvaluateScriptAsync(script);
                    //task.Wait();
                    //var response = task.Result; //< --**never gets here**
                    //return true;
                }
                return true;
                */
            }

            public bool OnPreKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
            {
                bool handled = Owner.OnKeyEvent(windowsKeyCode, nativeKeyCode, modifiers, isSystemKey);
                if (handled)
                    return true;

                return false;
            }
        }

        bool OnKeyEvent(int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey)
        {
            PageControl pc = (this.Parent as PageControl);
            if (pc != null)
            {
                Keys keys = 0;
                if ((modifiers & CefEventFlags.ControlDown) > 0)
                    keys |= Keys.Control;
                if ((modifiers & CefEventFlags.ShiftDown) > 0)
                    keys |= Keys.Shift;
                if ((modifiers & CefEventFlags.AltDown) > 0)
                    keys |= Keys.Alt;
                keys |= (Keys)windowsKeyCode;
                pc.DoPreviewKeyDown(this, new PreviewKeyDownEventArgs(keys));
                pc.DoKeyDown(this, new KeyEventArgs(keys));
            }

            return true;
        }

        //bool isFirstVisible = true;
        //protected override void OnVisibleChanged(EventArgs e)
        //{
        //    base.OnVisibleChanged(e);

        //    if (this.Visible && isFirstVisible)
        //    {
        //        isFirstVisible = false;
        //        Resize();
        //    }
        //}


        public cHtml(oHtml myHtmlItem)
        {
            MyHtmlItem = myHtmlItem;

            browser = new ChromiumWebBrowser("");


            browser.JavascriptMessageReceived += OnJavascriptMessageReceived;
            browser.FrameLoadEnd += OnBrowserFrameLoadEnd;
            browser.Dock = DockStyle.Fill;
            browser.MenuHandler = new CustomMenuHandler(this);
            browser.KeyboardHandler = new CustomKeyboardHandler(this);
            browser.LoadError += Browser_LoadError;
            //browser.

            var x = browser.ResourceRequestHandlerFactory;
            browser.RegisterResourceHandler("assets/background01.png", new FileStream("assets/background01.png", FileMode.Open, FileAccess.Read)); //, "image/png");


            //browser.KeyDown += Browser_KeyDown;
            //browser.MouseDown += Browser_MouseDown;
            //browser.MouseMove += Browser_MouseMove;
            browser.MouseUp += Browser_MouseUp;
            this.Controls.Add(browser);

            InitHtml();

            timerRender = new System.Windows.Forms.Timer();
            timerRender.Tick += TimerRender_Tick;
            timerRender.Interval = 200;
            timerRender.Start();
        }

        public void Resize()
        {
            this.Left = (int)(this.MyHtmlItem.Bounds.X * QB.Controls.Draw.mmToPx);
            this.Top = (int)(this.MyHtmlItem.Bounds.Y * QB.Controls.Draw.mmToPx);
            this.Width = (int)(this.MyHtmlItem.Bounds.W * QB.Controls.Draw.mmToPx);
            this.Height = (int)(this.MyHtmlItem.Bounds.H * QB.Controls.Draw.mmToPx);

            this.SetBrowserZoom((double)(this.Parent as PageControl).Width / (double)QB.Controls.Draw.Width);
        }

        private void Browser_LoadError(object sender, LoadErrorEventArgs e)
        {
            Console.Write("#ERR LoadError: " + e.ErrorText);
            editForm?.AddLog("#ERR (LoadError): " + e.ErrorText);

        }

        //Point _mouseDownPos = Point.Empty;
        private void Browser_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseDownPos = e.Location;
            if (qbook.Core.ThisBook.DesignMode && e.Button == MouseButtons.Middle)
            {
                browser.Cursor = Cursors.SizeAll;
            }
        }

        private void Browser_MouseUp(object sender, MouseEventArgs e)
        {
            _mouseDownPos = Point.Empty;
            browser.Cursor = Cursors.Default;
        }

        private void Browser_MouseMove(object sender, MouseEventArgs e)
        {
            if (qbook.Core.ThisBook.DesignMode && e.Button == MouseButtons.Middle)
            {
                int dx = e.Location.X - _mouseDownPos.X;
                int dy = e.Location.Y - _mouseDownPos.Y;
                this.Left = (int)(_mouseDownPos.X * QB.Controls.Draw.mmToPx);
                this.Top = (int)(_mouseDownPos.X * QB.Controls.Draw.mmToPx);
            }
        }


        private void Browser_KeyDown(object sender, KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        async void InitHtml()
        {
            _enableRender = false;
            //File.WriteAllText("lastHtml.html", textBoxHtml.Text);
            //File.WriteAllText("lastHtml.css", textBoxCss.Text);
            await UseHtml();
            await ParseSettings();
            _enableRender = true;
        }

        private void TimerRender_Tick(object sender, EventArgs e)
        {
            Render();
        }

        class OnAssignItem
        {
            public string MethodName;
            public object[] Params;
        }

        Dictionary<string, Action<object>> OnEventDict = new Dictionary<string, Action<object>>();
        Dictionary<string, Func<object>> SetAttributeValueDict = new Dictionary<string, Func<object>>();
        Dictionary<string, ElementHandle> ElementDict = new Dictionary<string, ElementHandle>();
        Dictionary<string, OnAssignItem> OnAssignDict = new Dictionary<string, OnAssignItem>();

        private async void DevtoolsContext_Console(object sender, ConsoleEventArgs e)
        {
            //Console.WriteLine("console: " + e.Message);
            editForm?.AddLog("console: " + e.Message.Text);
            string prefix = "#onClick(";
            if (e.Message.Text.StartsWith(prefix))
            {
                string id = e.Message.Text.Substring(prefix.Length).TrimEnd(')');
                Console.WriteLine("onClick:" + id);
                string key = "#" + id + ".onClick";
                await _semaphore.WaitAsync();
                try
                {
                    if (OnEventDict.ContainsKey(key))
                        OnEventDict[key].Invoke(null);
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            prefix = "#onValueChanged(";
            if (e.Message.Text.StartsWith(prefix))
            {
                string[] args = e.Message.Text.Substring(prefix.Length).TrimEnd(')').SplitOutsideQuotes();
                Console.WriteLine("onValueChanged:" + args);
                if (args.Length == 2)
                {
                    string key = "#" + args[0].TrimStart('#').Trim() + ".onValueChanged";
                    await _semaphore.WaitAsync();
                    try
                    {
                        if (OnEventDict.ContainsKey(key))
                            OnEventDict[key].Invoke(args[1].Trim());
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
            }
        }


        private async void OnBrowserFrameLoadEnd(object sender, FrameLoadEndEventArgs args)
        {
            //this.Invoke((MethodInvoker)(() =>
            //{
            //    this.Visible = true;
            //}));

            //hide scrollbars
            if (args.Frame.IsMain)
            {
                if (true) //hide scrollbars
                {
                    args.Browser.MainFrame.ExecuteJavaScriptAsync(
                        "document.body.style.overflow = 'hidden'");
                }
                //add "click" event handler //TODO: we would need this for every individual button!? really? no other way?
                args.Browser.MainFrame.ExecuteJavaScriptAsync(@"
                    document.addEventListener('click', function(e) {
                        var parent = e.target.parentElement;

                        // run some validation with if(){..}
                        // some more javascript

                        CefSharp.PostMessage(parent.outerHTML); 
                    }, false);
                ");


                browser.ExecuteScriptAsync(@"
                    document.addEventListener('mousedown', function(e) {
                        var parent = e.target.parentElement;

                        // run some validation with if(){..}
                        // some more javascript

                        CefSharp.PostMessage('mousedown');
                    }, false);
                ");

                browser.ExecuteScriptAsync(@"
                    document.addEventListener('mouseup', function(e) {
                        var parent = e.target.parentElement;

                        // run some validation with if(){..}
                        // some more javascript

                        CefSharp.PostMessage('mouseup');
                    }, false);
                ");

                browser.ExecuteScriptAsync(@"
                    document.addEventListener('mousemove', function(e) {
                        var parent = e.target.parentElement;

                        // run some validation with if(){..}
                        // some more javascript

                        CefSharp.PostMessage('mousemove');
                    }, false);
                ");

            }

            this.SetBrowserZoom((double)(this.Parent as PageControl).Width / (double)QB.Controls.Draw.Width);

            ////HACK?: need to initially set the control's size. as this triggers a browser.SetZoomLevel (which is not allowed
            //this.Invoke((MethodInvoker)(() =>
            //{
            //    Resize();
            //}));
        }

        Point _mouseDownPos = Point.Empty;
        Point _lastControlPos = Point.Empty;
        Bounds _origItemBounds = new Bounds(0, 0, 0, 0);
        Bounds _origCtrlBounds = new Bounds(0, 0, 0, 0);
        bool _isMouseMove = false;
        bool _isMouseSize = false;
        private void OnJavascriptMessageReceived(object sender, JavascriptMessageReceivedEventArgs e)
        {
            if (e.Message != null)
            {
                if ((string)e.Message == "mousedown")
                {
                    _origItemBounds = this.MyHtmlItem.Bounds;
                    _origCtrlBounds = new Bounds(this.Left, this.Top, this.Width, this.Height);
                    Point pos = Cursor.Position;
                    _mouseDownPos = pos;
                    _lastControlPos = this.Location;
                    if (qbook.Core.ThisBook.DesignMode && System.Windows.Forms.Control.MouseButtons == MouseButtons.Middle)
                    {
                        Point ctrlTLScreen = Point.Empty;
                        Point ctrlBRScreen = Point.Empty;
                        this.Invoke((MethodInvoker)(() =>
                        {
                            ctrlTLScreen = this.Parent.PointToScreen(new Point(this.Left, this.Top));
                            ctrlBRScreen = this.Parent.PointToScreen(new Point(this.Right, this.Bottom));
                        }));
                        if ((pos.X > (ctrlBRScreen.X - 30)) && (pos.Y > (ctrlBRScreen.Y - 30)))
                        {
                            //bottom right corner
                            _isMouseSize = true;
                            _isMouseMove = false;
                            this.Invoke((MethodInvoker)(() =>
                            {
                                browser.Cursor = Cursors.SizeNWSE;
                            }));
                        }
                        else
                        {
                            _isMouseSize = false;
                            _isMouseMove = true;
                            this.Invoke((MethodInvoker)(() =>
                            {
                                browser.Cursor = Cursors.SizeAll;
                            }));
                        }
                    }
                }

                if ((string)e.Message == "mouseup")
                {
                    _mouseDownPos = Point.Empty;
                    _isMouseSize = false;
                    _isMouseMove = false;
                    (this.Parent as PageControl)?.DoSizeChanged(new EventArgs());
                    this.Invoke((MethodInvoker)(() =>
                    {
                        browser.Cursor = Cursors.Default;
                    }));
                }

                if ((string)e.Message == "mousemove")
                {
                    if (qbook.Core.ThisBook.DesignMode && Control.MouseButtons == MouseButtons.Middle)
                    {
                        Point pos = Cursor.Position;
                        int x = _lastControlPos.X + (int)((pos.X - _mouseDownPos.X) / QB.Controls.Draw.mmToPx);
                        int y = _lastControlPos.Y + (int)((pos.Y - _mouseDownPos.Y) / QB.Controls.Draw.mmToPx);

                        this.Invoke((MethodInvoker)(() =>
                        {
                            if (_isMouseMove)
                            {
                                this.Left = (int)_origCtrlBounds.X + (pos.X - _mouseDownPos.X);
                                this.Top = (int)_origCtrlBounds.Y + (pos.Y - _mouseDownPos.Y);

                                this.MyHtmlItem.Bounds.X = this.Left / QB.Controls.Draw.mmToPx;
                                this.MyHtmlItem.Bounds.Y = this.Top / QB.Controls.Draw.mmToPx;
                                qbook.Core.ThisBook.Modified = true;
                            }
                            if (_isMouseSize)
                            {
                                //if (_mouseDownPos != Point.Empty) //HACK: sometimes t
                                {
                                    this.Width = (int)_origCtrlBounds.W + (pos.X - _mouseDownPos.X);
                                    this.Height = (int)_origCtrlBounds.H + (pos.Y - _mouseDownPos.Y);

                                    this.MyHtmlItem.Bounds.W = this.Width / QB.Controls.Draw.mmToPx;
                                    this.MyHtmlItem.Bounds.H = this.Height / QB.Controls.Draw.mmToPx;
                                }
                                if (this.Width > 1200 || this.Height > 800
                                || this.Width < 10 || this.Height < 10)
                                {

                                }
                                qbook.Core.ThisBook.Modified = true;
                            }
                        }));
                    }
                }
            }
        }


        public void SetBrowserZoom(double scale)
        {
            if (browser.BrowserHwnd == IntPtr.Zero)
                return;

            double factor = 25.0;
            double level = 5.46149645 * Math.Log(scale * factor, Math.E) - 25.12; //see: https://www.magpcss.org/ceforum/viewtopic.php?f=6&t=11491
            browser.SetZoomLevel(level);
        }

        //public Helpers.ObjectSettings ObjectSettings = new Helpers.ObjectSettings();
        //ObjectSettings Settings = new ObjectSettings();
        //string _Settings = "";
        //public string Settings
        //{
        //    get
        //    {
        //        return _Settings;
        //    }
        //    set
        //    {
        //        bool changed = _Settings != value;
        //        _Settings = value;
        //        if (changed)
        //            ObjectSettings.Parse(value);
        //    }
        //}

        //string htmlCode = "";
        //string cssCode = "";
        //string settingsCode = "";
        FileSystemWatcher fileUriWatcher = null;
        private async Task UseHtml()
        {
            //if (this.IsHandleCreated)
            //{
            //    this.Invoke((MethodInvoker)(() =>
            //    {
            //        this.Visible = false;
            //    }));
            //}

            var url = MyHtmlItem.CodeHtml.TrimStart();
            if (url.StartsWith("file://")
                || url.StartsWith("http://")
                || url.StartsWith("https://"))
            {
                if (url.StartsWith("file://./"))
                {
                    url = url.Replace("file://./", "file://" + Directory.GetCurrentDirectory().Replace("\\", "/") + "/");

                    //automatically detect changes and reload
                    var filePath = url.Replace("file://", "").Replace('/', '\\');
                    try
                    {
                        if (fileUriWatcher != null)
                        {
                            fileUriWatcher.Changed -= OnFileUriWatcherChanged;
                            fileUriWatcher.Dispose();
                        }
                        fileUriWatcher = new FileSystemWatcher();
                        fileUriWatcher.Path = Path.GetDirectoryName(filePath);
                        fileUriWatcher.Filter = Path.GetFileName(filePath);
                        fileUriWatcher.NotifyFilter = NotifyFilters.LastWrite;
                        fileUriWatcher.Changed += OnFileUriWatcherChanged;
                        fileUriWatcher.EnableRaisingEvents = true;
                    }
                    catch (Exception ex)
                    {

                    }
                }

                if (this.Parent != null) //try to reduce render-flickering
                    this.SetBrowserZoom((double)(this.Parent as PageControl).Width / (double)QB.Controls.Draw.Width);

                browser.LoadUrl(url);
                await browser.WaitForInitialLoadAsync();

                if (devtoolsContext == null)
                {
                    devtoolsContext = await browser.CreateDevToolsContextAsync();
                    devtoolsContext.Console += DevtoolsContext_Console;
                    devtoolsContext.Error += DevtoolsContext_Error;
                    devtoolsContext.PageError += DevtoolsContext_PageError;
                    devtoolsContext.RequestFailed += DevtoolsContext_RequestFailed;
                    devtoolsContext.Request += DevtoolsContext_Request;

                }
                return;
            }


            var html =
@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <title> qbook test </title>
    <script src='gauge.min.js'></script>
    <script>
        function onClick(id)
        {
            console.log('#onClick('+id + ')');
        }
    </script>
</head>
<body>
";
            //if (checkBoxUseCss.Checked)
            html += "<style>" + MyHtmlItem.CodeCss + "</style>";

            html += MyHtmlItem.CodeHtml + "</body></html>";

            string gaugeJs = System.IO.File.ReadAllText("gauge.min.js");
            html = html.Replace("<script src='gauge.min.js'></script>", "<script>" + gaugeJs + "</script>");

            browser.LoadHtml(html);
            await browser.WaitForInitialLoadAsync();

            if (devtoolsContext == null)
            {
                devtoolsContext = await browser.CreateDevToolsContextAsync();
                devtoolsContext.Console += DevtoolsContext_Console;
                devtoolsContext.Error += DevtoolsContext_Error;
                devtoolsContext.PageError += DevtoolsContext_PageError;
                devtoolsContext.RequestFailed += DevtoolsContext_RequestFailed;
                devtoolsContext.Request += DevtoolsContext_Request;
            }
        }

        private void OnFileUriWatcherChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            UseHtml();
        }

        private void DevtoolsContext_Request(object sender, RequestEventArgs e)
        {

        }

        private void DevtoolsContext_RequestFailed(object sender, RequestEventArgs e)
        {
            Console.Write("#ERR RequestFailed: " + e.Request.Failure.ToString());
        }

        private void DevtoolsContext_PageError(object sender, PageErrorEventArgs e)
        {
            Console.Write("#ERR PageError: " + e.Message);
            editForm?.AddLog("#ERR (PageError): " + e.Message);
        }

        private void DevtoolsContext_Error(object sender, CefSharp.Dom.ErrorEventArgs e)
        {
            Console.Write("#ERR Error: " + e.Error);
            editForm?.AddLog("#ERR (Error): " + e.Error);
        }


        class SettingItem
        {
            public string type;
            public string onChanged;
            public string onClick;
            public string onValueChanged;
            public string value;
            public string data_value;
            public string text;
            public string color;
        }

        private async Task ParseSettings()
        {
            try
            {
                //_suspendRender = true;
                //System.Threading.Thread.Sleep(500);

                //lock (_parseLockObject)
                await _semaphore.WaitAsync();
                try
                {
                    OnEventDict.Clear();
                    OnAssignDict.Clear();
                    SetAttributeValueDict.Clear();
                    ElementDict.Clear();


                    //TODO: MAGES->CsScript:
                    /*
                    Main.Qb.ScriptingEngine.UnsubscribeAssignEventRegex("^html." + this.GetHashCode() + @"\..*");
                    var errors = new List<ObjectSettings.ErrorInfo>();
                    Settings.Parse(2, MyHtmlItem.CodeSettings, out errors, MyHtmlItem);
                    */

                    //TODO MIGRATION-new -> new settings storage needed!!!
                    List<SettingItem> settingItems = new List<SettingItem>();
                    foreach (var item in settingItems)
                    {
                        if (item.type.StartsWith("@")) //changed-event
                        {
                            string onChanged = item.onChanged?.ToString().Trim() ?? "";
                            if (onChanged.StartsWith("js(") && onChanged.EndsWith(")"))
                            {
                                onChanged = onChanged.Substring(3, onChanged.Length - 4);
                                var nameAndParams = onChanged.SplitOutsideQuotes();
                                if (nameAndParams != null)
                                {
                                    OnAssignDict.Add(item.type, new OnAssignItem() { MethodName = nameAndParams[0], Params = nameAndParams.Skip(1).ToArray<object>() });
                                    //Action<object>((param) => Main.Qb.ScriptingEngine.InterpretScript(item["onChanged"].Replace("{value}", param.ToString()))));
                                    //TODO: MAGES->CsScript: Main.Qb.ScriptingEngine.SubscribeAssignEventPrefix(item.Type.Substring(1), ScriptingEngine_AssignEvent, "html." + this.GetHashCode() + "." + item.Type);
                                }
                            }
                        }
                        else if (item.type.StartsWith("#")) //element-name
                        {
                            var element = devtoolsContext.QuerySelectorAsync(item.type);
                            if (element != null)
                            {
                                if (item.onClick != null)
                                {
                                    //OnClickDict.Add(item.Type + ".onClick", new Action(() => InterpretScript(item["onClick"])));
                                    //TODO: MAGES->CsScript: OnEventDict.Add(item.Type + ".onClick", new Action<object>((param) => Main.Qb.ScriptingEngine.InterpretScript(item["onClick"]?.ToString())));
                                }
                                if (item.onValueChanged != null)
                                {
                                    //TODO: MAGES->CsScript: OnEventDict.Add(item.Type + ".onValueChanged", new Action<object>((param) => Main.Qb.ScriptingEngine.InterpretScript(item["onValueChanged"]?.ToString().Replace("{value}", param.ToString()))));
                                }

                                if (item.value != null)
                                {
                                    //TODO: MAGES->CsScript:  SetAttributeValueDict.Add(item.Type + ".value", new Func<object>(() => Main.Qb.ScriptingEngine.InterpretScript(item["value"]?.ToString())?.ToDouble() ?? double.NaN));
                                }
                                if (item/*["data-value"]*/.data_value != null)
                                {
                                    //TODO: MAGES->CsScript: SetAttributeValueDict.Add(item.Type + ".data-value", new Func<object>(() => Main.Qb.ScriptingEngine.InterpretScript(item["data-value"]?.ToString())?.ToDouble() ?? double.NaN));
                                }

                                if (item.text != null)
                                {
                                    //TODO: MAGES->CsScript: SetAttributeValueDict.Add(item.Type + ".text", new Func<object>(() => Main.Qb.ScriptingEngine.InterpretScript(item["text"]?.ToString())?.ToString()));
                                }

                                if (item.color != null)
                                {
                                    //TODO: MAGES->CsScript: SetAttributeValueDict.Add(item.Type + ".color", new Func<object>(() => Main.Qb.ScriptingEngine.InterpretScript(item["color"]?.ToString())?.ToString())); //TODO: ParseColor
                                }
                            }
                        }
                        else
                        {
                            //error
                        }
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            finally
            {
                //_suspendRender = false;
            }
        }


        private async void ScriptingEngine_AssignEvent(/* //TODO: MAGES->CsScript: ScriptingClass.WriteEventArgs e */)
        {
            //TODO: MAGES->CsScript: //string key = "@" + e.Key;
            string key = "@";

            await _semaphore.WaitAsync();
            try
            {
                if (OnAssignDict.ContainsKey(key))
                {
                    //OnAssignDict[key].Invoke(null);
                    var assignItem = OnAssignDict[key];
                    object[] results = new object[assignItem.Params.Length];
                    for (int i = 0; i < assignItem.Params.Length; i++)
                    {
                        //TODO: MAGES->CsScript: results[i] = Main.Qb.ScriptingEngine.InterpretScript(assignItem.Params[i].ToString());
                    }

                    editForm?.AddLog(">>js: " + assignItem.MethodName + "(" + string.Join(", ", results) + ")");

                    if (browser != null && !browser.IsDisposed)
                        browser.ExecuteScriptAsync(assignItem.MethodName, results);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }


        bool _enableRender = false;
        SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        bool _inRender = false;
        async void Render()
        {
            if (this.browser?.IsDisposed ?? true)
            {
                timerRender.Stop();
                this.Dispose();
                return;
            }

            if (!_enableRender)
                return;

            if (_inRender)
                return;

            _inRender = true;
            await _semaphore.WaitAsync();
            try
            {
                foreach (var item in SetAttributeValueDict)
                {
                    try
                    {
                        int lastDotPos = item.Key.LastIndexOf('.');
                        if (lastDotPos >= 0)
                        {
                            string key = item.Key.Substring(0, item.Key.LastIndexOf('.'));
                            string attribute = item.Key.Substring(item.Key.LastIndexOf('.') + 1);

                            //lock (_parseLockObject)
                            {
                                if (!ElementDict.ContainsKey(key))
                                {
                                    ElementDict.Add(key, await devtoolsContext.QuerySelectorAsync(key));
                                }
                            }
                            var element = ElementDict[key];
                            if (element.Disposed)
                            {
                                //TODO: cleanup?!
                            }
                            else
                            {
                                switch (attribute)
                                {
                                    case "value":
                                    case "data-value": //gauge.js
                                        var v = item.Value.Invoke();
                                        if (v != null)
                                        {
                                            double d = v.ToDouble();
                                        }
                                        await element.SetAttributeValueAsync(attribute, item.Value.Invoke()?.ToDouble());
                                        break;
                                    case "text":
                                        await element.SetAttributeValueAsync(attribute, item.Value.Invoke()?.ToString());
                                        await element.SetPropertyValueAsync("textContent", item.Value.Invoke()?.ToString() ?? "&ensp;"); //for buttons
                                                                                                                                         //await element.SetPropertyValueAsync("value", item.Value.Invoke()?.ToString() ?? "&ensp;"); //for buttons
                                        break;
                                    case "color":
                                        await element.SetAttributeValueAsync(attribute, item.Value.Invoke()?.ToString());
                                        break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            finally
            {
                _semaphore.Release();
                _inRender = false;
            }

        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // cHtml
            // 
            this.Name = "cHtml";
            this.ResumeLayout(false);

        }


        private async void EditForm_CallbackEvent(object sender, oControls.oHtmlSettingsForm.CallbackEventArgs e)
        {


            if (e.Action == "apply")
            {
                qbook.Core.ThisBook.Modified = true;

                oControls.oHtmlSettingsForm editForm = sender as oControls.oHtmlSettingsForm;
                MyHtmlItem.CodeHtml = editForm.CodeHtml;
                MyHtmlItem.CodeCss = editForm.CodeCss;
                MyHtmlItem.CodeSettings = editForm.CodeSettings;

                _enableRender = false;
                await UseHtml();
                await ParseSettings();
                _enableRender = true;
            }

            if (e.Action == "delete")
            {
                qbook.Core.ThisBook.Modified = true;

                this.Parent?.Invoke((MethodInvoker)(() =>
                {
                    this.Parent?.Controls?.Remove(this);
                }));
                this.MyHtmlItem?.MyPage?.HtmlItems?.Remove(this.MyHtmlItem);
            }

            if (e.Action == "closed")
            {
                editForm?.Dispose();
                editForm = null;
            }
        }

        oControls.oHtmlSettingsForm editForm = null;
        private void ShowEditForm()
        {
            if (editForm == null || !editForm.IsHandleCreated || editForm.IsDisposed)
            {
                editForm = new oControls.oHtmlSettingsForm();
                editForm.CallbackEvent += EditForm_CallbackEvent;
                editForm.Show();
            }
            editForm.Text = $"{this.MyHtmlItem.Name} [{this.MyHtmlItem.Text}]";
            editForm.CodeHtml = MyHtmlItem.CodeHtml.Replace("\r", "").Replace("\n", "\r\n");
            editForm.CodeCss = MyHtmlItem.CodeCss.Replace("\r", "").Replace("\n", "\r\n");
            editForm.CodeSettings = MyHtmlItem.CodeSettings.Replace("\r", "").Replace("\n", "\r\n");
            editForm.BringToFront();
        }

        //public override void Init()
        //{

        //}

        bool DesignMode = true;
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (DesignMode)
            {
                e.Graphics.DrawLine(Pens.Black, e.ClipRectangle.Left, e.ClipRectangle.Top, e.ClipRectangle.Right, e.ClipRectangle.Bottom);
            }

        }
    }
}
