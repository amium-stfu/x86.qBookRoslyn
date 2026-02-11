using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace qbook.Controls
{
    public partial class AboutDialog : Form
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        private void AboutDialog_Load(object sender, EventArgs e)
        {
            labelVersion.Text = $"{QB.Book.AppVersionEx} [built: {QB.Book.AppBuildDate}]";

            string licenseSource = "";
            string licenseFilename = $@"..\amium.{Environment.MachineName}.lic";
            if (!File.Exists(licenseFilename))
                licenseFilename = @"..\amium.lic"; //default
            if (!File.Exists(licenseFilename))
            {
                labelLicense.Text = "#Error: cannot find license-file";
            }
            else
            {
                Task.Run(() =>
                {
                    this.Invoke((MethodInvoker)(() =>
                    {
                        labelLicense.Text = "...";
                        var licenseString = Amium.Helpers.License2.GetLicense(licenseFilename, out licenseSource);
                        //labelLicense.Text = licenseSource+ "\r\n" + licenseString;

                        labelLicenseTitle.Text = "License/" + licenseSource + ":";
                        string info = "";
                        Match match = null;
                        match = System.Text.RegularExpressions.Regex.Match(licenseString, @".*qbookS:(.*?),");
                        if (match.Success)
                        {
                            info += "Server: " + (match.Groups[1].Value == "F" ? "full" : "expires " + match.Groups[1].Value) + "\r\n";
                        }

                        match = System.Text.RegularExpressions.Regex.Match(licenseString, @".*qbookC:(.*?),");
                        if (match.Success) 
                        {
                            info += "Client: " + (match.Groups[1].Value == "F" ? "full" : "expires " + match.Groups[1].Value) + "\r\n";
                        }

                        match = System.Text.RegularExpressions.Regex.Match(licenseString, @".*qbookD:(.*?),");
                        if (match.Success)
                        {
                            info += "Developer: " + (match.Groups[1].Value == "F" ? "full" : "expires " + match.Groups[1].Value) + "\r\n";
                        }

                        labelLicense.Text = info;

                    }));
                });
            }
        }
    }
}
