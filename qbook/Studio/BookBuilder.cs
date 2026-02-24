using Microsoft.CodeAnalysis;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Community.CsharpSqlite.Sqlite3;


namespace qbook.Studio
{

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




    internal static class BookBuilder
    {
        public static string CreateBook(string directory = null,string name = null)
        {
            if (!System.IO.Directory.Exists(directory) && directory != null)
            {
                System.IO.Directory.CreateDirectory(directory);
            }


            if (directory == null)
                directory = Core.ThisBook.DataDirectory;

            if(name == null)
                name = Core.ThisBook.ProjectName;

            string book = System.IO.Path.Combine(directory, name + ".code");

            if (!System.IO.Directory.Exists(book))
            {
                System.IO.Directory.CreateDirectory(book);
            }

          
            string bookJson = CreateBookJson(folder:book,name: name);
            string programCs = CreateProgramCs(System.IO.Path.Combine(book, "Program.cs"));
            string csproj = CreateCsproj("InMemoryProject", "net8.0", System.IO.Path.Combine(book, name+ ".csproj"));
            string globalUsings = CreateGlobalUsing(System.IO.Path.Combine(book, "GlobalUsings.cs"));
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(book, "Pages"));
            SaveAllPages(book);

            string pagesInfo = string.Join("\n\n", qbook.Core.ActualMain.Objects.OfType<oPage>().Select(p => CreatePageDefinition(p)));
            System.IO.File.WriteAllText(System.IO.Path.Combine(directory, name + ".qbook"), pagesInfo);

            return bookJson + "\n\n" + programCs + "\n\n" + csproj;

        }

        #region Build
        public static string CreateProgramCs(string file = "T:\\qbooksave\\Program.cs")
        {
            List<oPage> pages = qbook.Core.ActualMain.Objects.OfType<oPage>().ToList();
            var sbProgram = new StringBuilder();
            sbProgram.AppendLine("namespace QB");
            sbProgram.AppendLine("{");
            sbProgram.AppendLine("\tpublic static class Program");
            sbProgram.AppendLine("\t{");
            foreach (oPage page in pages)
                sbProgram.AppendLine($"\t\tpublic static Definition{page.Name}.qPage {page.Name} {{ get; }} = new Definition{page.Name}.qPage();");

            sbProgram.AppendLine("\t\tpublic static void Initialize()");
            sbProgram.AppendLine("\t\t{");

            foreach (oPage page in pages)
                sbProgram.AppendLine($"\t\t\t{page.Name}.Initialize();");

            sbProgram.AppendLine("\t\t}");

            sbProgram.AppendLine("\t\tpublic static void Run()");
            sbProgram.AppendLine("\t\t{");
            foreach (oPage page in pages)
                sbProgram.AppendLine($"\t\t\t{page.Name}.Run();");
            sbProgram.AppendLine("\t\t}");

            sbProgram.AppendLine("\t\tpublic static void Destroy()");
            sbProgram.AppendLine("\t\t{");
            foreach (oPage page in pages)
                sbProgram.AppendLine($"\t\t\t{page.Name}.Destroy();");
            sbProgram.AppendLine("\t\t}");

            sbProgram.AppendLine("\t}");
            sbProgram.AppendLine("}");

            System.IO.File.WriteAllText(file, sbProgram.ToString());



            return sbProgram.ToString();
        }
        public static string CreateCsproj(string projectName = "InMemoryProject", string targetFramework = "net8.0", string file = "T:\\qbooksave\\Program.csproj")
        {
            var sb = new StringBuilder();
            sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
            sb.AppendLine("  <PropertyGroup>");
            sb.AppendLine($"    <OutputType>Library</OutputType>");
            sb.AppendLine($"    <TargetFramework>{targetFramework}</TargetFramework>");
            sb.AppendLine($"    <UseWindowsForms>true</UseWindowsForms>");
            sb.AppendLine($"    <AssemblyName>{projectName}</AssemblyName>");
            sb.AppendLine("  </PropertyGroup>");

            bool needsSystemDrawingCommon = false;
            sb.AppendLine("  <ItemGroup>");

            var refs = Core.Roslyn.GetProject?.MetadataReferences.OfType<PortableExecutableReference>() ?? Enumerable.Empty<PortableExecutableReference>();
            foreach (var r in refs)
            {
                if (string.IsNullOrWhiteSpace(r.FilePath))
                    continue;

                string fileName = System.IO.Path.GetFileNameWithoutExtension(r.FilePath);
                if (string.Equals(fileName, "System.Drawing", StringComparison.OrdinalIgnoreCase))
                {
                    needsSystemDrawingCommon = true;
                    continue; // nicht das alte Framework-Assembly einbetten
                }

                sb.AppendLine($"    <Reference Include=\"{fileName}\">");
                sb.AppendLine($"      <HintPath>{r.FilePath}</HintPath>");
                sb.AppendLine("    </Reference>");
            }

            sb.AppendLine("  </ItemGroup>");

            if (needsSystemDrawingCommon)
            {
                sb.AppendLine("  <ItemGroup>");
                sb.AppendLine("    <PackageReference Include=\"System.Drawing.Common\" Version=\"8.0.0\" />");
                sb.AppendLine("  </ItemGroup>");
            }

            sb.AppendLine("</Project>");
            System.IO.File.WriteAllText(file, sb.ToString());
            return sb.ToString();
        }

        public static string CreateGlobalUsing(string file = "T:\\qbooksave\\GlobalUsings.cs")
        {
            var sb = new StringBuilder();
            sb.AppendLine("global using static QB.Program;");
            System.IO.File.WriteAllText(file, sb.ToString());
            return sb.ToString();
        }

        public static string CreateBookJson(string folder = "T:\\qbooksave\\book.json", string name = "")
        {
           
           
            
            qBookDefinition def = new qBookDefinition
            {
                ProjectName = Core.ThisBook.ProjectName,
                Version = Core.ThisBook.Version,
                VersionHistory = Core.ThisBook.VersionHistory,
                VersionEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                StartFullScreen = Core.ThisBook.StartFullScreen,
                HidPageMenuBar = Core.ThisBook.HidPageMenuBar,
                PasswordAdmin = Core.ThisBook.PasswordAdmin,
                PasswordService = Core.ThisBook.PasswordService,
                PasswordUser = Core.ThisBook.PasswordUser,
                Directory = System.IO.Path.Combine(folder,name,".code"),
                Filename = name + ".qbook",
                SettingsDirectory = System.IO.Path.Combine(folder, name, ".settings"),
                DataDirectory = System.IO.Path.Combine(folder, name, ".data"),
                TempDirectory = System.IO.Path.Combine(folder, name, ".temp"),
                BackupDirectory = System.IO.Path.Combine(folder, name, ".backup"),
                Language = Core.ThisBook.Language,
                PageOrder = qbook.Core.ActualMain.Objects.OfType<oPage>().Select(p => p.Name).ToList()
            };
            string json = System.Text.Json.JsonSerializer.Serialize(def, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(System.IO.Path.Combine(folder, "book.json"), json);
            return json;
        }

        public static string CreatePageDefinition(oPage page)
        {
            PageDefinition def = new PageDefinition
            {
                Name = page.Name,
                Text = page.Text,
                OrderIndex = page.OrderIndex,
                Hidden = page.Hidden,
                Format = page.Format == null ? "A4" : page.Format,
                Includes = page.Includes,
                CodeOrder = page.CodeOrder,
                Section = page.Section,
                Url = page.Url
            };
            string json = System.Text.Json.JsonSerializer.Serialize(def, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return json;
        }

        public static async Task SaveAllPages(string bookFolder)
        {
            foreach (oPage page in qbook.Core.ActualMain.Objects.OfType<oPage>())
            {
                string folder = System.IO.Path.Combine(bookFolder, "Pages",page.Name);
                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);

                string pageJson = CreatePageDefinition(page);
                string file = System.IO.Path.Combine(folder, "oPage.json");
                System.IO.File.WriteAllText(file, pageJson);

                string code = await Core.Roslyn.GetDocumentText(page.Name + ".qPage.cs");
                string codeFile = System.IO.Path.Combine(folder, page.Name + ".qPage.cs");
                System.IO.File.WriteAllText(codeFile, code);


                foreach (string codeName in page.CodeOrder)
                {
                    if (page.SubCodes != null)
                    {
                        code = await Core.Roslyn.GetDocumentText(codeName);
                        codeFile = System.IO.Path.Combine(folder, codeName);
                        System.IO.File.WriteAllText(codeFile, code);
                    }
                }

            }
        }

        #endregion





        #region Load


        #endregion



    }
}
