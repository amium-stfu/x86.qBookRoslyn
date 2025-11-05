using log4net;
using log4net.Appender;
using log4net.Layout;
using QB;
using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace qbook
{

    static class Program
    {
        static MainForm LandingPage;
        
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern void ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

        [ComImport, Guid("4ce576fa-83dc-4F88-951c-9d0782b4e376")]
        class UIHostNoLaunch
        {
        }

        [ComImport, Guid("37c994e7-432b-4834-a2f7-dce1f13b834b")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface ITipInvocation
        {
            void Toggle(IntPtr hwnd);
        }

        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();


        internal static string[] Args = null;

        internal static UdpLogListener AppUdpLogListener;



        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {


            GlobalExceptions.SafeInvoke("Main", () =>
            {



                if (false) //testing
                {
                    log4net.Repository.Hierarchy.Hierarchy hierarchy = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();

                    //RollingFileAppender
                    PatternLayout rollerPatternLayout = new log4net.Layout.PatternLayout();
                    rollerPatternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
                    rollerPatternLayout.ActivateOptions();
                    RollingFileAppender roller = new log4net.Appender.RollingFileAppender();
                    roller.AppendToFile = false;
                    roller.File = @"qbook.log";
                    roller.Layout = rollerPatternLayout;
                    roller.MaxSizeRollBackups = 5;
                    roller.MaximumFileSize = "10MB";
                    roller.RollingStyle = RollingFileAppender.RollingMode.Size;
                    roller.StaticLogFileName = true;
                    roller.ActivateOptions();
                    hierarchy.Root.AddAppender(roller);

                    //UdpAppender
                    var udpPatternLayout = new log4net.Layout.PatternLayout();
                    udpPatternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
                    udpPatternLayout.ActivateOptions();
                    var udpAppender = new log4net.Appender.UdpAppender();
                    udpAppender.RemoteAddress = new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 });
                    udpAppender.RemotePort = 39999;
                    udpAppender.Layout = udpPatternLayout;
                    udpAppender.ActivateOptions();
                    hierarchy.Root.AddAppender(udpAppender);

                    hierarchy.Root.Level = log4net.Core.Level.All;
                    hierarchy.Configured = true;

                    ILog logger = log4net.LogManager.GetLogger(hierarchy.Name, "qbLogger");
                }


                //license for SyntaxEditor (Actipro+.NetLang+PythonLang)
               
                Args = args;
                System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");
                System.Globalization.CultureInfo.CurrentCulture = ci;
                System.Globalization.CultureInfo.DefaultThreadCurrentCulture = ci;
                System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = ci;


                //1: load some assemblies from /libs/ -> BEST wuld be a "<probing privatePath="libs" />" in app.config; however SCAN doesn't want extra files like app.config (as ILMerge is used to create ONE file qbook.exe only)
                //so if we want more complex possibilities (like the HTML (CefSharp) plugin, we need to put these files in /libs/)

                //2: load some assemblies from /libs/ -> OK, but obsolete :/
                //AppDomain.CurrentDomain.AppendPrivatePath("libs"); 

                //3: load some assemblies from /libs/ -> OK, but nut sure if everything gets loaded?!
                //AppDomain currentDomain = AppDomain.CurrentDomain;
                //currentDomain.AssemblyResolve += new ResolveEventHandler(MyResolveEventHandler);


                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);


                AppUdpLogListener = new UdpLogListener();
                AppUdpLogListener.StartListening(39999);


                if (false) //HALE //TEMP testing
                {
                    Form f = new Form();
                    f.Width = 800;
                    f.Height = 600;

                    TextBox tb = new TextBox();
                    tb.Dock = DockStyle.Top;
                    f.Controls.Add(tb);

                    ColoredTextBox.ColoredTextBoxControl ctb = new ColoredTextBox.ColoredTextBoxControl();
                    ctb.ParentObjectName = "";
                    ctb.Dock = DockStyle.Fill;
                    f.Controls.Add(ctb);
                    ctb.BringToFront();
                    ctb.Text = "";
                    ctb.ShowDebugInfo = true;

                    f.ShowDialog();
                    return;
                }

                //if (false)
                //{
                //    CancellationTokenSource Timer10CancellationTokenSource = new CancellationTokenSource(); // TimeSpan.FromSeconds(300));
                //    Timer10CancellationTokenSource = new CancellationTokenSource(); // TimeSpan.FromSeconds(300));
                //    var Timer10msCancellationToken = Timer10CancellationTokenSource.Token;
                //    _ = AccurateTimer.PrecisionRepeatActionOnIntervalAsync(Action10ms(), TimeSpan.FromMilliseconds(10), Timer10msCancellationToken);
                //    MessageBox.Show("running");
                //}

                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                Application.Run(LandingPage = new MainForm(args));

            });


        }

        //static List<long> _ticks10ms = new List<long>();
        //static public Action Action10ms() => () =>
        //{System.Reflection.TargetInvocationException: 'Exception has been thrown by the target of an invocation.'

        //    while (_ticks10ms.Count > 20)
        //        _ticks10ms.RemoveAt(0);
        //    _ticks10ms.Add(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
        //    return;
        //};


        private static Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            string libsDir = ".\\libs\\";
            //This handler is called only when the common language runtime tries to bind to the assembly and fails.

            //Retrieve the list of referenced assemblies in an array of AssemblyName.
            Assembly MyAssembly, objExecutingAssembly;
            string strTempAssmbPath = "";

            objExecutingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName[] arrReferencedAssmbNames = objExecutingAssembly.GetReferencedAssemblies();

            //Loop through the array of referenced assembly names.
            foreach (AssemblyName strAssmbName in arrReferencedAssmbNames)
            {
                //Check for the assembly names that have raised the "AssemblyResolve" event.
                if (strAssmbName.FullName.Substring(0, strAssmbName.FullName.IndexOf(",")) == args.Name.Substring(0, args.Name.IndexOf(",")))
                {
                    //HALE: first check "./" directory
                    /*
                    string dllFilename = args.Name.Substring(0, args.Name.IndexOf(",")) + ".dll";
                    if (File.Exists(dllFilename))
                    {
                        strTempAssmbPath = dllFilename;
                        break;
                    }
                    */
                    //Build the path of the assembly from where it has to be loaded.                
                    strTempAssmbPath = libsDir + args.Name.Substring(0, args.Name.IndexOf(",")) + ".dll";
                    break;
                }

            }

            //Load the assembly from the specified path.                    
            MyAssembly = Assembly.LoadFrom(strTempAssmbPath);

            //Return the loaded assembly.
            return MyAssembly;
        }


        //static private Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
        //{
        //    string libsDir = ".\\libs\\";
        //    //This handler is called only when the common language runtime tries to bind to the assembly and fails.

        //    //Retrieve the list of referenced assemblies in an array of AssemblyName.
        //    Assembly MyAssembly, objExecutingAssembly;
        //    string strTempAssmbPath = "";

        //    objExecutingAssembly = Assembly.GetExecutingAssembly();
        //    AssemblyName[] arrReferencedAssmbNames = objExecutingAssembly.GetReferencedAssemblies();

        //    //Loop through the array of referenced assembly names.
        //    foreach (AssemblyName strAssmbName in arrReferencedAssmbNames)
        //    {
        //        //Check for the assembly names that have raised the "AssemblyResolve" event.
        //        string a = strAssmbName.FullName.Substring(0, strAssmbName.FullName.IndexOf(","));
        //        string b = "";
        //        if (args.Name.Contains(";"))
        //          b=  args.Name.Substring(0, args.Name.IndexOf(","));
        //        if (a == b)
        //        {
        //            //Build the path of the assembly from where it has to be loaded.                
        //            strTempAssmbPath = libsDir + args.Name.Substring(0, args.Name.IndexOf(",")) + ".dll";
        //            break;
        //        }
        //    }


        //    if (strTempAssmbPath.Length == 0)
        //        return null;
        //    //Load the assembly from the specified path.                    
        //    MyAssembly = Assembly.LoadFrom(strTempAssmbPath);

        //    //Return the loaded assembly.
        //    return MyAssembly;
        //}

        public static bool ToggleTabTip()
        {
            try
            {
                var uiHostNoLaunch = new UIHostNoLaunch();
                var tipInvocation = (ITipInvocation)uiHostNoLaunch;
                //MessageBox.Show("tipInvocation: " + tipInvocation);
                //var w = GetDesktopWindow();
                //MessageBox.Show("GetDesktopWindow: " + w);
                tipInvocation.Toggle(GetDesktopWindow());
                Marshal.ReleaseComObject(uiHostNoLaunch);
                return true;
            }
            catch 
            {
                return false;
            }
        }

        public static void OpenTouchKeyboard()
        {
            //string commonProgramFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);
            var programFiles = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
            string tabTipExePath = System.IO.Path.Combine(programFiles, "Common Files", "Microsoft Shared", "ink", "TabTip.exe");
            string tabTip32ExePath = System.IO.Path.Combine(programFiles, "Common Files", "Microsoft Shared", "ink", "TabTip32.exe");
            if (System.IO.File.Exists(tabTipExePath))
                ShellExecute(IntPtr.Zero, "open", tabTipExePath, "", "", 0);
            else if (System.IO.File.Exists(tabTip32ExePath))
                ShellExecute(IntPtr.Zero, "open", tabTip32ExePath, "", "", 0);
        }
    }
}
