using CSScripting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using RoslynDocument = Microsoft.CodeAnalysis.Document;


namespace qbook
{
    [Serializable]
    public class Book
    {
        public oControl Main;
        public string Version = "0.1.0";
        public string VersionHistory = "";
        public long VersionEpoch = 0;

        //2025-09-06 STFU
        public bool StartFullScreen = false;
        public bool HidPageMenuBar = false;
        public List<string> PageOrder = new List<string>();

        [XmlIgnore]
        public RoslynDocument Program;
        [XmlIgnore]
        public RoslynDocument Global;

        public string PasswordAdmin { get; set; } = null; //overrides the default Admin-Password
        public string PasswordService { get; set; } = null; //overrides the default Service-Password
        public string PasswordUser { get; set; } = null; //overrides the default User-Password

        bool _Modified = false;
        [XmlIgnore]
        public bool Modified
        {
            get
            {
                return _Modified;
            }
            set
            {
                var changed = _Modified != value;
                _Modified = value;
                if (changed)
                {
                    OnPropertyChangedEvent("Modified", _Modified);
                }
            }
        }

        public System.Drawing.Rectangle Bounds = new System.Drawing.Rectangle(0, 0, 800, 600);

        public string _Directory;
        [XmlIgnore]
        public string Directory
        {
            get
            {
                return _Directory;
            }
            set
            {
                var changed = _Directory != value;
                _Directory = value;
                if (changed)
                {
                    //OnStaticPropertyChangedEvent("Directory", _Directory);
                }
            }
        }

 

        public string _Filename;
        [XmlIgnore]
        public string Filename
        {
            get
            {
                return _Filename;
            }
            set
            {
                var changed = _Filename != value;
                _Filename = value;
                if (changed)
                {
                    //OnStaticPropertyChangedEvent("Filename", _Filename);
                }
            }
        }

        public void SetDataDirectory(string dir) { _DataDirectory = dir; }
        public void SetSettingsDirectory(string dir) { _SettingsDirectory = dir; }
        public void SetTempDirectory(string dir) { _TempDirectory = dir; }


        static Regex NameVersionExtRegex = new Regex(@"(?<name>.*?)(\.v(?<version>\d.*))?(?<ext>\.qbook)");
        string _DataDirectory = null;
        public string DataDirectory
        {
            get
            {
                if (_DataDirectory == null)
                {
                    //create/use default data-directory
                    string qbookName = Core.ThisBook.Filename;
                    Match m = NameVersionExtRegex.Match(qbookName);
                    if (m.Success)
                    {
                        string name = m.Groups["name"].Value;
                        string version = m.Groups["version"].Value; 
                        string ext = m.Groups["ext"].Value.ToLower();
                        if (ext == ".qbook")
                        {
                            string dir = Path.Combine(Core.ThisBook.Directory, name + ".data");
                            if (!System.IO.Directory.Exists(dir))
                                System.IO.Directory.CreateDirectory(dir);

                            _DataDirectory = dir;
                        }
                    }
                    else
                    {
                        _DataDirectory = qbookName+ ".data";
                    }
                }
                return _DataDirectory;
            }
            set
            {
                _DataDirectory = value; // nicht auf null setzen
            }
        }

        string _BackupDirectory = null;
        public string BackupDirectory
        {
            get
            {
                if (_BackupDirectory == null)
                {
                    //create/use default data-directory
                    string qbookName = Core.ThisBook.Filename;
                    Match m = NameVersionExtRegex.Match(qbookName);
                    if (m.Success)
                    {
                        string name = m.Groups["name"].Value;
                        string version = m.Groups["version"].Value;
                        string ext = m.Groups["ext"].Value.ToLower();
                        if (ext == ".qbook")
                        {
                            string dir = Path.Combine(Core.ThisBook.Directory, name + ".backup");
                            if (!System.IO.Directory.Exists(dir))
                                System.IO.Directory.CreateDirectory(dir);

                            _BackupDirectory = dir;
                        }
                    }
                    else
                    {
                        _BackupDirectory = qbookName + ".backup";
                    }
                }
                return _BackupDirectory;
            }
            set
            {
                _BackupDirectory = null;// value;
            }
        }

        string _SettingsDirectory = null;
        public string SettingsDirectory
        {
            get
            {
                if (_SettingsDirectory == null)
                {
                    //create/use default data-directory
                    string qbookName = Core.ThisBook.Filename;
                    Match m = NameVersionExtRegex.Match(qbookName);
                    if (m.Success)
                    {
                        string name = m.Groups["name"].Value;
                        string version = m.Groups["version"].Value;
                        string ext = m.Groups["ext"].Value.ToLower();
                        if (ext == ".qbook")
                        {
                            string dir = Path.Combine(Core.ThisBook.Directory, name + ".settings");
                            if (!System.IO.Directory.Exists(dir))
                                System.IO.Directory.CreateDirectory(dir);

                            _SettingsDirectory = dir;
                        }
                    }
                    else
                    {
                        _SettingsDirectory = qbookName + ".settings";
                    }
                }
                return _SettingsDirectory;
            }
            set
            {
                _SettingsDirectory = value;
            }
        }

        string _TempDirectory = null;
        [XmlIgnore]
        public string TempDirectory
        {
            get
            {
                if (_TempDirectory == null)
                {
                    //create/use default data-directory
                    string qbookName = Core.ThisBook.Filename;
                    Match m = NameVersionExtRegex.Match(qbookName);
                    if (m.Success)
                    {
                        string name = m.Groups["name"].Value;
                        string version = m.Groups["version"].Value;
                        string ext = m.Groups["ext"].Value.ToLower();
                        if (ext == ".qbook")
                        {
                            string dir = Path.Combine(Core.ThisBook.Directory, name + ".temp");
                            if (!System.IO.Directory.Exists(dir))
                                System.IO.Directory.CreateDirectory(dir);

                            _TempDirectory = dir;
                        }
                    }
                    else
                    {
                        _TempDirectory = qbookName + ".temp";
                    }
                }
                return _TempDirectory;
            }
            set
            {
                _TempDirectory = value;
            }
        }

        public string _LogFilename = "qbook.log";
        [XmlIgnore]
        public string LogFilename
        {
            get
            {
                //return Path.Combine(TempDirectory, "qbook.{date}.log");
                return _LogFilename;
            }
            internal set
            {
                //this is set during OpenQbookAsync() as it is in a different directory (<qbook>.temp\) for each <qbook>.qbook)
                _LogFilename = value;
            }
        }

        public string _Language = "en";
        public string Language
        {
            get
            {
                return _Language;
            }
            set
            {
                var changed = _Language != value;
                _Language = value;
                if (changed)
                {
                    OnPropertyChangedEvent("Language", _Language);
                }
            }
        }
        public int Grid = 0;
        bool _DesignMode = true;
        public bool DesignMode
        {
            get
            {
                return _DesignMode;
            }
            set
            {
                var changed = _DesignMode != value;
                _DesignMode = value;
                if (changed)
                {
                    OnPropertyChangedEvent("DesignMode", _DesignMode);
                }
            }
        }

        bool _TagMode = false;
        public bool TagMode
        {
            get
            {
                return _TagMode;
            }
            set
            {
                var changed = _TagMode != value;
                _TagMode = value;
                if (changed)
                {
                    OnPropertyChangedEvent("TagMode", _TagMode);
                }
            }
        }

        //  public bool SrcMode = false;
        bool _Recorder = false;
        public bool Recorder
        {
            get
            {
                return _Recorder;
            }
            set
            {
                var changed = _Recorder != value;
                _Recorder = value;
                if (changed)
                {
                    OnPropertyChangedEvent("Recorder", _Recorder);
                }
            }
        }


        public class PropertyChangedEventArgs : EventArgs
        {
            public string Property { get; set; }
            public object Value { get; set; }
        }

        // Define an event using the EventHandler delegate and custom event args.
        public event EventHandler<PropertyChangedEventArgs> PropertyChanged;

        //public delegate void PropertyChangedEventHandler(PropertyChangedEventArgs e);
        //public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChangedEvent(string property, object value)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs() { Property = property, Value = value });
        }


        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        //public delegate void PropertyChangedEventHandler(PropertyChangedEventArgs e);
        //public event PropertyChangedEventHandler PropertyChanged;
        internal static void OnStaticPropertyChangedEvent(string property, object value)
        {
            if (StaticPropertyChanged != null)
                StaticPropertyChanged(null, new PropertyChangedEventArgs() { Property = property, Value = value });
        }


        public static void RebuildItemHierarchy(oControl control)
        {
            if (control.Objects != null)
            {
                foreach (var obj in control.Objects.OfType<oControl>())
                {
                    obj.Parent = control;
                    RebuildItemHierarchy(obj);
                }
            }
        }

        public void Init()
        {
            RebuildItemHierarchy(Main);
           

            Main.Init();
        }



        //public void Serialize()
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(Directory))
        //            Directory = System.Windows.Forms.Application.StartupPath;
        //        string filename = Path.Combine(Path.GetFullPath(Directory), Filename); // + ".qbook";
        //        CreateBackups(filename, 5, "qbook.backup");

        //        //EditObjectForm.ApplyAllCodeChanges();
        //        this.VersionEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        //        var xmlWriterSettings = new XmlWriterSettings() { Indent = true };
        //        XmlSerializer xmlSerializer = new XmlSerializer(typeof(Book));
        //        XmlWriter xmlWriter = XmlWriter.Create(filename, xmlWriterSettings);
        //        xmlWriter.WriteProcessingInstruction("xml-stylesheet", "type='text/xsl' href='qbook.xsl'");
        //        using (xmlWriter)
        //        {
        //            xmlSerializer.Serialize(xmlWriter, this);
        //            xmlWriter.Close();
        //        }
        //        EditObjectForm.SetAllOpenEditorsToUnModified();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, "ERROR SAVING QBOOK");
        //    }
        //}

        //void CreateBackups(string filename, int count, string subdirectory = null)
        //{
        //    try
        //    {
        //        var ext = Path.GetExtension(filename);
        //        var bakDir = Path.GetDirectoryName(filename);
        //        if (!string.IsNullOrEmpty(subdirectory))
        //        {
        //            bakDir = Path.Combine(bakDir, subdirectory);
        //            if (!System.IO.Directory.Exists(bakDir))
        //                System.IO.Directory.CreateDirectory(bakDir);
        //        }
        //        var bakFiles = new DirectoryInfo(bakDir).GetFiles(Path.GetFileNameWithoutExtension(filename) + ".*.bak" + ext)
        //                                                      //.Where(f => Regex.IsMatch(f.FullName, @".*\.\d{8}_\d{6}\.bak"))
        //                                                      .OrderByDescending(f => f.LastWriteTime)
        //                                                      .ToList();
        //        //delete all existing backups (except n-1)
        //        if (count > 1 && (bakFiles.Count() > (count - 1)))
        //        {
        //            foreach (var f in bakFiles.Skip(count - 1))
        //            {
        //                File.Delete(f.FullName);
        //            }
        //        }

        //        //move orig file to .bak
        //        if (File.Exists(filename))
        //        {
        //            FileInfo fi = new FileInfo(filename);
        //            string backFilename = Path.Combine(bakDir, Path.GetFileNameWithoutExtension(filename)) + "." + fi.LastWriteTime.ToString("yyyyMMdd_HHmmss") /*+ ".bak"*/ + ext;
        //            File.Move(filename, backFilename);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        QB.Logger.Error("#EX creating qbook backups: " + ex.Message + (QB.Logger.ShowStackTrace ? ex.StackTrace : ""));
        //    }
        //}


        public static Book Deserialize(string fullPath/*string directory, string filename*/)
        {

            string filename = fullPath.GetFileName();
            string directory = fullPath.GetDirName();

            Book file = new Book();
            if (System.IO.File.Exists(fullPath))
            {
                string r = System.IO.File.ReadAllText(Path.Combine(directory, filename));
                r = r.Replace("<File xml", "<Book xml").Replace("</File>", "</Book>");
                r = r.Replace("<Object>", "<Main>").Replace("</Object>", "</Main>");
                //r = r.Replace("<Root>", "<Main>").Replace("</Root>", "</Main>");
                System.IO.File.WriteAllText(Path.Combine(directory, filename), r);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Book));
                using (TextReader textReader = new StreamReader(Path.Combine(directory, filename)))
                {
                    try
                    {
                        //disable "apply settings" DURING serialization
                        oItem._IsDeserializing = true;
                        file = (Book)xmlSerializer.Deserialize(textReader);
                        if (file == null) file = new Book();
                        file.Filename = filename;
                        file.Directory = directory;

                        return file;
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                    finally
                    {
                        oItem._IsDeserializing = false;
                    }
                }
            }
            file.Filename = filename;
            file.Directory = directory;

            return file;
        }


    }
}
