using Newtonsoft.Json;
using qbookCode.Controls;
using qbookCode.Net;
using qbookCode.Roslyn;
using qbookCode.Studio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RoslynDocument = Microsoft.CodeAnalysis.Document;

namespace qbookCode
{
    internal static class Core
    {
        public static RoslynService Roslyn { get; } = new RoslynService();
        public static Book ThisBook { get; set; } = new Book();
        public static DataTable ComLog;
        public static FormCodeExplorer? Explorer { get; set; }
        private static ClientSide? ComChannel;
        public static void InitPipeCom()
        {
            ComChannel = new ClientSide();
            ComChannel.OnReceived += async (evt) =>
            {
                PipeCommandManager.EnqueueCommand(evt);
                Program.LogInfo($"Received command from qBook: {evt.Command} with args: {string.Join(", ", evt.Args)}");
            };
            PipeCommandManager.RegisterCommandHandler("CloseEditor", async cmd => await PipeCommands.CloseEditor());
            PipeCommandManager.RegisterCommandHandler("RuntimeErrors", async cmd => await PipeCommands.RuntimeErrors(cmd));
            PipeCommandManager.Start();
        }
        public static void SendToQbook(string command, params string[] args)
        {
            ComChannel?.Send(new PipeCommand
            {
                Command = command,
                Args = args
            });
        }
        public class PageDefinition
        {
            public string Name { get; set; }
            public string Text { get; set; }
            public int OrderIndex { get; set; }
            public bool Hidden { get; set; }
            public string Format { get; set; }
            public List<string> Includes { get; set; }
            public List<string> CodeOrder { get; set; }
            public string Section { get; set; }
            public string Url { get; set; }
        }

        public class qBookDefinition
        {
            public string ProjectName { get; set; } = "Unnamed";
            public string Version { get; set; } = "0.1.0";
            public string VersionHistory { get; set; } = "";
            public long VersionEpoch { get; set; } = 0;
            public bool StartFullScreen { get; set; } = false;
            public bool HidPageMenuBar { get; set; } = false;
            public string PasswordAdmin { get; set; } = null; //overrides the default Admin-Password
            public string PasswordService { get; set; } = null; //overrides the default Service-Password
            public string PasswordUser { get; set; } = null; //overrides the default User-Password
            public string Directory { get; set; } = null;
            public string Filename { get; set; } = null;
            public string SettingsDirectory { get; set; } = null;
            public string DataDirectory { get; set; } = null;
            public string TempDirectory { get; set; } = null;
            public string BackupDirectory { get; set; } = null;
            public string Language { get; set; } = null;
            public List<string> PageOrder { get; set; } = new List<string>();


        }

        internal static oPage oPageFromString(string json)
        {
            var data = JsonConvert.DeserializeObject(json, typeof(PageDefinition)) as PageDefinition;
            return new oPage
            {
                Name = data.Name,
                Text = data.Text,
                OrderIndex = data.OrderIndex,
                Hidden = data.Hidden,
                Format = data.Format,
                Includes = data.Includes ?? new List<string>(),
                CodeOrder = data.CodeOrder,
                Section = data.Section,
                Url = data.Url
            };
        }

        internal static async Task ShowCodeExploror(oPage page = null)
        {

            Application.OpenForms[0].Invoke((MethodInvoker)(() =>
            {
                if (Core.Explorer == null || Core.Explorer.IsDisposed)
                {
                    Core.Explorer = new FormCodeExplorer();
                }

                if (!Core.Explorer.Visible)
                    Core.Explorer.Show();
                else
                    Core.Explorer.BringToFront();
            }));

            // Hintergrundarbeit (optional)
            await Task.Run(() =>
            {
                // Hier darfst du alles machen, was nicht UI ist
            });
        }

        internal static void CleanupBeforeLoad()
        {

            Debug.WriteLine("CleanupBeforeLoad");
            // Editor schließen, Script zerstören, BookRuntime freigeben wie bisher

        
            if(ThisBook != null)
            {
                Debug.WriteLine("ThisBook Pages Clear");
                ThisBook.Pages.Clear();
            }
            


            Debug.WriteLine("Reset Roslyn Documents");
            if (Roslyn != null)
            {
                Debug.WriteLine("Roslyn ResetDocumentsOnly");
                Roslyn.ResetDocumentsOnly();
            }

           
            Core.Explorer?.Editor?.Reset();

            Debug.WriteLine("Clear Dicts");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
        }



        internal static async Task<Book> BookFromFolder(string folderPath, string bookname)
        {

            RuntimeManager.InitRuntimeErrors();


            Debug.WriteLine("BookFromFolder: " + folderPath);
            Book newBook = new Book();
             
            Debug.WriteLine("Read Book.json");
            string bookJson = File.ReadAllText(Path.Combine(folderPath, "Book.json"));
            var qbook = JsonConvert.DeserializeObject(bookJson, typeof(qBookDefinition)) as qBookDefinition;

            newBook.Version = qbook.Version;
            newBook.VersionHistory = qbook.VersionHistory;
            newBook.VersionEpoch = qbook.VersionEpoch;
            newBook.StartFullScreen = qbook.StartFullScreen;
            newBook.HidPageMenuBar = qbook.HidPageMenuBar;
            newBook.PasswordAdmin = qbook.PasswordAdmin;
            newBook.PasswordService = qbook.PasswordService;
            newBook.PasswordUser = qbook.PasswordUser;
            newBook.Directory = qbook.Directory;
            newBook.Filename = qbook.Filename;

            newBook.Language = qbook.Language;
            newBook.PageOrder = qbook.PageOrder;
       


            List<string> reversePageOrder = newBook.PageOrder.AsEnumerable().Reverse().ToList();

            Debug.WriteLine("Processing Pages...");
            foreach (string page in reversePageOrder)
            {
                Debug.WriteLine(" - " + page);
            }

            List<oPage> pages = new List<oPage>();

            foreach (string page in newBook.PageOrder)
            {
                oPage opage = null;
                Debug.WriteLine(" -- " + page);

                Debug.WriteLine(" --- read page data");
                string pageFolder = Path.Combine(folderPath, "Pages", page);
                Debug.WriteLine(" --- page folder: " + pageFolder);
                string oPageJson = File.ReadAllText(Path.Combine(pageFolder, "oPage.json"));
                Debug.WriteLine(" --- deserialize page");
                opage = oPageFromString(oPageJson);
                Debug.WriteLine(" --- read page code");
                string filename = page + ".qPage.cs";
                Debug.WriteLine(" --- page code file: " + filename);
                opage.Filename = filename;
                Debug.WriteLine(" --- read code text");
                opage.Code = File.ReadAllText(Path.Combine(pageFolder, filename));
                Debug.WriteLine(" --- add page code to Roslyn");
                Roslyn.AddCodeDocument(filename, opage.Code, true);
                Debug.WriteLine(" --- get page code document");
                opage.RoslynCodeDoc = Roslyn.GetCodeDocument(filename);


                List<string> reverseCodeOrder = opage.CodeOrder.AsEnumerable().Reverse().ToList();

                foreach (string codeFile in opage.CodeOrder)
                {
                    if (codeFile.EndsWith("qPage.cs")) continue;

                    Debug.WriteLine(" --- " + codeFile);
                    string subCode = File.ReadAllText(Path.Combine(pageFolder, codeFile));

                    CodeDocument doc = new CodeDocument(codeFile, subCode, false, Roslyn);

                    opage.SubCodeDocuments[codeFile] = doc;
                    if (opage.Includes.Contains(codeFile))
                    {
                        await opage.SubCodeDocuments[codeFile].Include();
                    }
                }
                Debug.WriteLine(" ---- add page to book: " + opage.Name);
                newBook.Pages.Add(opage.Name,opage);
            }
            pages.Reverse();

            Roslyn.AddCodeDocument("Program.cs", File.ReadAllText(Path.Combine(folderPath, "Program.cs")), true);
            Roslyn.AddCodeDocument("GlobalUsing.cs", "global using static QB.Program;", true);

    

            string name = newBook.Filename.Replace(".qbook", "");


            return newBook;

        }

        internal static async Task SaveBook() 
        { 
            string bookFolder = Program.ActualBookPath;
            string backupFolder = Program.ActualBookPath.Replace(".code", ".backup");
            string pagesFolder = Path.Combine(bookFolder, "Pages");

            string csproj = File.ReadAllText(Path.Combine(bookFolder, $"{Core.ThisBook.Filename.Replace(".qbook", "")}.csproj"));

            //Backup 
            string bookBackup = Core.ThisBook.Filename.Replace(".qbook", "") + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmms") + ".code";
            string backupUri = Path.Combine(backupFolder, bookBackup);
            if (!Directory.Exists(backupFolder))
                Directory.CreateDirectory(backupFolder);

            Directory.Move(bookFolder, backupUri);


            //Save Book
            Directory.CreateDirectory(bookFolder);
            string link = "SaveDate: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            File.WriteAllText(Path.Combine(Core.ThisBook.Directory, Core.ThisBook.Filename), link);
            File.WriteAllText(Path.Combine(bookFolder, $"{Core.ThisBook.Filename.Replace(".qbook", "")}.csproj"), csproj);


            await SaveProjectAsync(bookFolder);


        }

        public static async Task SaveProjectAsync(string newFile = @"T:\qSave")
        {


            if (!Directory.Exists(newFile))
                Directory.CreateDirectory(newFile);

            string codeDir = Path.Combine(newFile, "Pages");
            Directory.CreateDirectory(codeDir);


            // 🧩 Projektbeschreibung vorbereiten
            var project = new qBookDefinition
            {
                ProjectName = Core.ThisBook.Filename.Replace(".qbook", ""),
                Version = Core.ThisBook.Version,
                VersionHistory = Core.ThisBook.VersionHistory,
                VersionEpoch = Core.ThisBook.VersionEpoch,
                StartFullScreen = Core.ThisBook.StartFullScreen,
                HidPageMenuBar = Core.ThisBook.HidPageMenuBar,
                PasswordAdmin = Core.ThisBook.PasswordAdmin,
                PasswordService = Core.ThisBook.PasswordService,
                PasswordUser = Core.ThisBook.PasswordUser,
                Directory = Core.ThisBook.Directory,
                Filename = Core.ThisBook.Filename,
                SettingsDirectory = Core.ThisBook.SettingsDirectory,
                DataDirectory = Core.ThisBook.DataDirectory,
                BackupDirectory = Core.ThisBook.BackupDirectory,
                TempDirectory = Core.ThisBook.TempDirectory,
                Language = Core.ThisBook.Language,
                PageOrder = Core.ThisBook.PageOrder
            };

            foreach (oPage page in Core.ThisBook.Pages.Values)
            {
                string pageDir = Path.Combine(codeDir, $"{page.Name}");
                Directory.CreateDirectory(pageDir);

                var temp = await Core.Roslyn.GetDocumentTextAsync(page.Filename);
                string csCode = temp.ToString();
                System.IO.File.WriteAllText(Path.Combine(pageDir, page.Filename), csCode);

                foreach (CodeDocument sub in page.SubCodeDocuments.Values)
                {
                    temp = await Core.Roslyn.GetDocumentTextAsync(sub.Filename);
                    if (temp == null)
                        temp = sub.Code;
                    csCode = temp.ToString();
                    System.IO.File.WriteAllText(Path.Combine(pageDir, sub.Filename), csCode);
                }

                var dto = new PageDefinition
                {
                    Name = page.Name,
                    Text = page.Text,

                    OrderIndex = page.OrderIndex,
                    Hidden = page.Hidden,
                    Format = page.Format,
                    Includes = page.Includes,
                    Section = page.Section,
                    Url = page.Url,
                    CodeOrder = page.CodeOrder,

                };

                string oPageJson = JsonConvert.SerializeObject(dto, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(Path.Combine(pageDir, "oPage.json"), oPageJson);
            }

            var code = await Core.Roslyn.GetDocumentTextAsync("Program.cs");
            string path = Path.Combine(newFile, "Program.cs");
            File.WriteAllText(path, code);

            code = await Core.Roslyn.GetDocumentTextAsync("GlobalUsing.cs");
            path = Path.Combine(newFile, "GlobalUsing.cs");
            File.WriteAllText(path, code);

            string bookJson = JsonConvert.SerializeObject(project, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(Path.Combine(newFile, "Book.json"), bookJson);

            Debug.WriteLine("Project saved to " + newFile);

            string programFile = Path.Combine(newFile, "Program.cs");
            string programCs = CreateProgramCs();
            File.WriteAllText(programFile, programCs);

        }
        private static string CreateProgramCs()
        {
            var sbProgram = new StringBuilder();
            sbProgram.AppendLine("namespace QB");
            sbProgram.AppendLine("{");
            sbProgram.AppendLine("\tpublic static class Program");
            sbProgram.AppendLine("\t{");
            foreach (oPage page in ThisBook.Pages.Values)
                sbProgram.AppendLine($"\t\tpublic static Definition{page.Name}.qPage {page.Name} {{ get; }} = new Definition{page.Name}.qPage();");

            sbProgram.AppendLine("\t\tpublic static void Initialize()");
            sbProgram.AppendLine("\t\t{");

            foreach (oPage page in ThisBook.Pages.Values)
                sbProgram.AppendLine($"\t\t\t{page.Name}.Initialize();");

            sbProgram.AppendLine("\t\t}");

            sbProgram.AppendLine("\t\tpublic static void Run()");
            sbProgram.AppendLine("\t\t{");
            foreach (oPage page in ThisBook.Pages.Values)
                sbProgram.AppendLine($"\t\t\t{page.Name}.Run();");
            sbProgram.AppendLine("\t\t}");

            sbProgram.AppendLine("\t\tpublic static void Destroy()");
            sbProgram.AppendLine("\t\t{");
            foreach (oPage page in ThisBook.Pages.Values)
                sbProgram.AppendLine($"\t\t\t{page.Name}.Destroy();");
            sbProgram.AppendLine("\t\t}");

            sbProgram.AppendLine("\t}");
            sbProgram.AppendLine("}");


            return sbProgram.ToString();
        }



    }







    public class Book() 
    {
        public string ProjectName { get; set; } = "Unnamed";
        public string Version { get; set; } = "0.1.0";
        public string VersionHistory { get; set; } = "";
        public long VersionEpoch { get; set; } = 0;
        public bool StartFullScreen { get; set; } = false;
        public bool HidPageMenuBar { get; set; } = false;
        public string PasswordAdmin { get; set; } = null; 
        public string PasswordService { get; set; } = null;
        public string PasswordUser { get; set; } = null; 
        public string Directory { get; set; } = null;
        public string Filename { get; set; } = null;
        public string SettingsDirectory { get; set; } = null;
        public string DataDirectory { get; set; } = null;
        public string TempDirectory { get; set; } = null;
        public string BackupDirectory { get; set; } = null;
        public string Language { get; set; } = null;
        public List<string> PageOrder { get; set; } = new List<string>();

        [JsonIgnore]
        public Dictionary<string, oPage> Pages { get; set; } = new Dictionary<string, oPage>();
    }
    public class oPage
    {

        public string Filename { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public int OrderIndex { get; set; }
        public bool Hidden { get; set; }
        public string Format { get; set; }
        public List<string> Includes { get; set; }
        public List<string> CodeOrder { get; set; }
        public string Section { get; set; }
        public string Url { get; set; }

        public string Code { get; set; }
        public oPage(string name, string text)
        {
            Name = name;
            Text = text;
        }

        public oPage() { }


        public RoslynDocument RoslynDoc;
        public CodeDocument RoslynCodeDoc;
        public Dictionary<string, oCode> SubCodes = new Dictionary<string, oCode>();
        public Dictionary<string, CodeDocument> SubCodeDocuments = new Dictionary<string, CodeDocument>();

    }
    public class oCode
    {
        public bool Active { get; set; } = true;
        public RoslynDocument RoslynDocument;
        public string Code;
        public string Filename;

        public oCode(string filename, bool active, RoslynDocument doc, string code)
        {
            this.Filename = filename;
            this.Active = active;
            this.RoslynDocument = doc;
            this.Code = code;
        }
    }
}
