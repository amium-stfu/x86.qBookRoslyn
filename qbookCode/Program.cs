using qbookCode.Controls;
using qbookCode.Net;
using Serilog;
using Serilog.Sinks.WinForms.Base;
using System.Collections;
using System.Diagnostics;


namespace qbookCode
{
    public static class Logger
    {
        public static ILogger Log { get; }
        public static BindingSource LogBindingSource { get; } = new BindingSource();

        public static SynchronizationContext UiContext;

        // maximale Anzahl Zeilen
        private const int MaxLogEntries = 5000;

        static Logger()
        {
            string logFilePath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "log",
                "log-.txt");

            Log = new LoggerConfiguration()
                .WriteTo.File(
                    path: logFilePath,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:l}{NewLine}{Exception}")
      
                .CreateLogger();

          //  WindFormsSink.GridLogSink.OnGridLogReceived += OnGridLogReceived;
        }
        private static void OnGridLogReceived(GridLogEvent logEvent)
        {
            if (LogBindingSource?.List is IList list)
            {
                try
                {
                    void Add()
                    {
                        list.Add(logEvent);

                        // --- wichtig: Begrenzen ---
                        if (list.Count > MaxLogEntries)
                        {
                            list.RemoveAt(0); // älteste Zeile löschen
                        }
                    }

                    if (UiContext != null)
                    {
                        // Immer auf dem echten UI-Thread ausführen
                        UiContext.Post(_ => Add(), null);
                    }
                    else
                    {
                        // Fallback: noch kein UIContext gesetzt (sehr früh im Startup)
                        Add();
                    }
                }
                catch
                {

                }
            }
        }
    }


    internal static class Program
    {
        public static string ActualBookPath = string.Empty;
        [STAThread]
        static void Main(string[] args)
        {
            RuntimeManager.InitRuntimeErrors();

            ApplicationConfiguration.Initialize();
            Logger.UiContext = SynchronizationContext.Current!;
            Thread uiThread = new Thread(() =>
            {
                if (args.Length > 0)
                {
                    PipeNames.Server = args[1];
                    PipeNames.Client = args[2];
                    RunWithSplash(args[0]);
                    
   
                }
                else
                {
                    Application.Run(new Form1());
                }
            });

            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();
            uiThread.Join();
        }

        public static void LogError(string message, Exception ex)
        {
            Logger.Log.Error(ex, message);
            Debug.WriteLine($"Error: {message}\n{ex.Message}\n{ex.StackTrace}");
        }

        public static void LogFatal(string message, Exception ex)
        {
            Logger.Log.Fatal(ex, message);
            Debug.WriteLine($"Fatal: {message}\n{ex.Message}\n{ex.StackTrace}");
        }

        public static void LogInfo(string message)
        {
            Logger.Log.Information(message);
            Debug.WriteLine($"Info: {message}");
        }

        private static void RunWithSplash(string inputPath)
        {
            using (var splash = new FormExplorerSplashScreen())
            {
                splash.Show();
                Application.DoEvents();
             

                splash.SetStatus("Loading Project...");
                Thread.Sleep(500);

                string directory = Path.GetDirectoryName(inputPath)!;
                string name = Path.GetFileNameWithoutExtension(inputPath);

                ActualBookPath = directory;

                Core.Roslyn.CreateEmptyProject(inputPath);

                splash.SetStatus("Creating Workspace...");
                Core.ThisBook = Core.BookFromFolder(directory, name).GetAwaiter().GetResult();

                splash.SetStatus("Starting Explorer...");
                Thread.Sleep(500);

               
                Core.Explorer = new FormCodeExplorer();
                Core.Explorer.StartPosition = FormStartPosition.CenterScreen;
                Core.Explorer.Opacity = 0; 
                Core.Explorer.Show();     
                Application.DoEvents();
                Core.Explorer.RefreshPageData();
                Core.Explorer.InitView();



                 splash.Close();

              
                Theme.Current = Theme.EditorTheme.Light;
                Core.Explorer.ApplyTheme("Init");
                Core.InitPipeCom();
               
                Core.Explorer.Editor.InitGridViews();

                // Jetzt sichtbar machen
                Core.Explorer.Opacity = 1;

                // Message-Loop starten
                Application.Run(Core.Explorer);
            }
        }
    }
}