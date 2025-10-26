using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using WebDav;
using System.Text.RegularExpressions;
using qbook_publisher.Properties;

namespace qbook_publisher
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }


        string SetupPath = null;
        string NextCloudUri = "https://drive.amium.at";
        string NextCloudUriEx = "/remote.php/dav/files/amium/";
        string RemotePath = "Public/Software/qbook/";
        private void FormMain_Load(object sender, EventArgs e)
        {
            if (Directory.Exists("setup"))
                SetupPath = @"setup\";
            else if (Directory.Exists(@"..\..\..\..\setup"))
                SetupPath = @"..\..\..\..\setup\";

            textBoxSetupPath.Text = SetupPath;
            textBoxNextCloudUri.Text = NextCloudUri;
            textBoxNextCloudUriEx.Text = NextCloudUriEx;
            textBoxRemotePath.Text = RemotePath;

            PopulateLocalFiles();
        }


        private async void PopulateLocalFiles()
        {
            var files = await GetLocalFiles(textBoxSetupPath.Text, @"qbook.*.zip");
            dgvLocalFiles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            var orderdFiles = files.OrderByDescending(f => f.Modified).ToList();
            if (orderdFiles.Count > 0)
                orderdFiles[0].Selected = true;
            dgvLocalFiles.DataSource = orderdFiles;
        }

        private async void buttonGetRemoteDir_Click(object sender, EventArgs e)
        {
            await PopulateRemoteFiles();
        }

        private async Task PopulateRemoteFiles()
        {
            var files = await WebDavGetRemoteFiles(textBoxRemotePath.Text.TrimEnd('/'), @".*/(?<filename>qbook\..*\.zip)$");
            dgvRemoteFiles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvRemoteFiles.DataSource = files;
        }

        string ExecuteShell(string command, string args)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = command;
            p.StartInfo.Arguments = args;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return output;
        }

        class FileItem
        {
            public bool Selected { get; set; }
            public string Filename { get; set; }
            public string Size { get => ((double)SizeLong / 1024.0 / 1024.0).ToString("0.00")+"MB"; }

            public DateTime? Modified { get; set; }
            public string Path { get; set; }
            public long SizeLong { get; set; }
        }
        async Task<List<FileItem>> WebDavGetRemoteFiles(string path, string regex)
        {
            string uri = textBoxNextCloudUri.Text + textBoxNextCloudUriEx.Text + path; // Public/Software/qbook";
            var clientParams = new WebDavClientParams
            {
                //BaseAddress = new Uri(uri),
                Credentials = new NetworkCredential("amium", "amium07#")
            };
            IWebDavClient client = new WebDavClient(clientParams);
            var response = await client.Propfind(uri);
            Regex fileRegex = new Regex(regex);

            List<FileItem> fileItems = new List<FileItem>();
            foreach (WebDavResource resource in response.Resources)
            {
                Match m = fileRegex.Match(resource.Uri);
                if (m.Success)
                {
                    var fullpath = m.Value;
                    var filename = m.Groups["filename"].Value;
                    fileItems.Add(new FileItem { 
                        Selected = true, 
                        Path = fullpath, 
                        Filename = filename, 
                        Modified = resource.LastModifiedDate, 
                        SizeLong = Convert.ToInt32(resource.Properties.Where(i => i.Name.LocalName == "getcontentlength").FirstOrDefault()?.Value) });
                }
            }

            return fileItems;
        }

        async Task<string> WebDavDeleteRemoteFile(string path)
        {
            string uri = textBoxNextCloudUri.Text + path; // Public/Software/qbook";
            var clientParams = new WebDavClientParams
            {
                //BaseAddress = new Uri(uri),
                Credentials = new NetworkCredential("amium", "amium07#")
            };
            IWebDavClient client = new WebDavClient(clientParams);
            var reponse = await client.Delete(uri);

            return reponse.IsSuccessful ? null : reponse.StatusCode + ":" + reponse.Description;
        }

        async Task<string> WebDavUploadRemoteFile(string localPath)
        {
            string uri = textBoxNextCloudUri.Text + textBoxNextCloudUriEx.Text + textBoxRemotePath.Text + Path.GetFileName(localPath); 
            var clientParams = new WebDavClientParams
            {
                //BaseAddress = new Uri(uri),
                Credentials = new NetworkCredential("amium", "amium07#")
            };
            IWebDavClient client = new WebDavClient(clientParams);
            FileStream fs = new FileStream(localPath, FileMode.Open, FileAccess.Read);
            PutFileParameters pfp = new PutFileParameters();
            var reponse = await client.PutFile(uri, fs, "application/zip");

            return reponse.IsSuccessful ? null : reponse.StatusCode + ":" + reponse.Description;
        }

        async Task<List<FileItem>> GetLocalFiles(string path, string pattern)
        {
            List<FileItem> fileItems = new List<FileItem>();
            string[] fileEntries = Directory.GetFiles(path, pattern);
            foreach (string fileName in fileEntries)
            {
                FileInfo fileInfo = new FileInfo(fileName);
                fileItems.Add(new FileItem { 
                    Selected = false, 
                    Path = fileInfo.FullName, 
                    Filename = fileInfo.Name, 
                    Modified = fileInfo.LastWriteTime, 
                    SizeLong = fileInfo.Length
                });
            }
            return fileItems;
        }

        private async void buttonDeleteSelected_Click(object sender, EventArgs e)
        {
            SetStatusText($"deleting...");
            List<string> errorList = new List<string>();
            var selectedFiles = (dgvRemoteFiles.DataSource as List<FileItem>).Where(i => i.Selected);
            progressBar.Minimum = 0;
            progressBar.Maximum = selectedFiles.Count();
            progressBar.Value = 0;
            foreach (FileItem fileItem in selectedFiles)
            {
                progressBar.Value++;
                SetStatusText($"deleting file {progressBar.Value}/{progressBar.Maximum}...");
                string err = await WebDavDeleteRemoteFile(fileItem.Path);
                if (err != null)
                {
                    errorList.Add($"could not delete '{fileItem.Path}': " + err);
                }
            }
            if (errorList.Count > 0) 
            {
                SetStatusText("#ERR: error deleting remote files");
                MessageBox.Show(string.Join("\r\n\r\n", errorList), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                SetStatusText($"successfully delted {selectedFiles.Count()} files");
            }

            await PopulateRemoteFiles();
        }

        private async void buttonUploadSelected_Click(object sender, EventArgs e)
        {
            SetStatusText($"uploading...");
            List<string> errorList = new List<string>();
            var selectedFiles = (dgvLocalFiles.DataSource as List<FileItem>).Where(i => i.Selected);
            progressBar.Minimum = 0;
            progressBar.Maximum = selectedFiles.Count();
            progressBar.Value = 0;
            foreach (FileItem fileItem in selectedFiles)
            {
                progressBar.Value++;
                SetStatusText($"uploading file {progressBar.Value}/{progressBar.Maximum}...");
                string err = await WebDavUploadRemoteFile(fileItem.Path);
                if (err != null)
                {
                    errorList.Add($"could not upload '{fileItem.Path}': " + err);
                }
            }
            if (errorList.Count > 0)
            {
                SetStatusText("#ERR: error uploading files");
                MessageBox.Show(string.Join("\r\n\r\n", errorList), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                SetStatusText($"successfully uploaded {selectedFiles.Count()} files");
            }

            await PopulateRemoteFiles();
        }

        void SetStatusText(string text)
        {
            labelStatus.BeginInvoke((MethodInvoker)delegate {
                labelStatus.Text = text;
                if (text.StartsWith("#E"))
                {
                    labelStatus.ForeColor = Color.White;
                    labelStatus.BackColor = Color.Red;
                }
                else
                {
                    labelStatus.ForeColor = Color.Black;
                    labelStatus.BackColor = Color.Transparent;
                }

            });
        }
    }

}
