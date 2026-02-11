using qbookCode.Controls;
using System.Diagnostics;
using System.IO;
using qbookCode.Roslyn;

namespace qbookCode
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            RoslynDiagnostic.InitDiagnostic();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "qbook files (*.csproj)|*.csproj|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Debug.WriteLine(openFileDialog.FileName);

                    filePath = Path.GetDirectoryName(openFileDialog.FileName);
                    fileContent = Path.GetFileName(openFileDialog.FileName).Replace(".csproj", "");

                    Core.Roslyn.CreateEmptyProject(openFileDialog.FileName);
                    Core.ThisBook = await Core.BookFromFolder(filePath, fileContent);
                    Core.ThisBook.DataDirectory = null;
                    Core.ThisBook.SettingsDirectory = null;
                    Core.ThisBook.TempDirectory = null;
                    Core.ThisBook.Directory = filePath;
                    


                    foreach (oPage page in Core.ThisBook.Pages.Values)
                    {
                        Debug.WriteLine($"Page: {page.Name} - {page.Text}");
                    }

                    // await OpenQbookAsync(openFileDialog.FileName);

                }
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await ShowCodeExploror();
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
    }
}
