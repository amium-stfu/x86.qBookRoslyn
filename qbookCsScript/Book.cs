using QB; //qbookCsScript
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;




[assembly: InternalsVisibleTo("qbookStudio")]

namespace QB
{
    public static class Book
    {
        internal static Func<object> ActiveBookResolver;

        private static dynamic GetActiveBook()
        {
            object book = ActiveBookResolver != null ? ActiveBookResolver() : Root.ActiveQbook;
            if (book == null)
                throw new InvalidOperationException("No active book is bound to the runtime facade.");
            Root.ActiveQbook = book;
            return book;
        }

        public static List<string> DroppedItems = new List<string>();
        public static bool CompactView = false;
        public static string UriStartup = "";
        public static string PageHeader = null;
        //2025-09-03 STFU
        public static string PageFormat = "A4";
        public static bool StartFullScreen = false;
        public static bool HidePageControlBar = false;

        

        /// <summary>
        /// Returns the filename (including extension) of the active qbook
        /// </summary>
        public static string Filename
        {
            get
            {
                try
                {
                    return Root.ActiveQbook.Filename;
                }
                catch (Exception ex)
                {
                    return "n/a";
                }
            }
        }

        public static string Directory
        {
            get
            {
                try
                {
                    return Root.ActiveQbook.Directory;
                }
                catch (Exception ex)
                {
                    return "n/a";
                }
            }
        }

        public static string DataDirectory
        {
            get
            {
                try
                {
                    string directory = GetActiveBook().DataDirectory;
                    if (string.IsNullOrWhiteSpace(directory))
                        throw new InvalidOperationException("The active book returned an empty DataDirectory.");
                    return directory;
                }
                catch (Exception ex)
                {
                    Logger.Error("Active book DataDirectory access failed: " + ex.ToString());
                    throw;
                }
            }
        }

        public static string SettingsDirectory
        {
            get
            {
                try
                {
                    string directory = GetActiveBook().SettingsDirectory;
                    if (string.IsNullOrWhiteSpace(directory))
                        throw new InvalidOperationException("The active book returned an empty SettingsDirectory.");
                    return directory;
                }
                catch (Exception ex)
                {
                    Logger.Error("Active book SettingsDirectory access failed: " + ex.ToString());
                    throw;
                }
            }
        }

        static string _AppVersion = "n/a";
        /// <summary>
        /// Holds the version-number of qbook.exe application (major.minor.build)
        /// </summary>
        public static string AppVersion
        {
            get
            {
                try
                {
                    return _AppVersion;

                }
                catch (Exception ex)
                {
                    return "n/a";
                }
            }
            internal set
            {
                _AppVersion = value;
            }
        }

        static string _AppVersionEx = "n/a";
        /// <summary>
        /// Holds the version-number of qbook.exe application (major.minor.build.revision)
        /// </summary>
        public static string AppVersionEx
        {
            get
            {
                try
                {
                    return _AppVersionEx;

                }
                catch (Exception ex)
                {
                    return "n/a";
                }
            }
            internal set
            {
                _AppVersionEx = value;
            }
        }

        static string _AppBuildDate = "n/a";
        /// <summary>
        /// Holds the build date of book.exe application
        /// </summary>
        public static string AppBuildDate
        {
            get
            {
                try
                {
                    return _AppBuildDate;

                }
                catch (Exception ex)
                {
                    return "n/a";
                }
            }
            internal set
            {
                _AppBuildDate = value;
            }
        }

        /// <summary>
        /// Holds the version-number of the qbook 
        /// </summary>
        public static string Version
        {
            get
            {
                try
                {
                    return Root.ActiveQbook.Book.Version;
                }
                catch (Exception ex)
                {
                    return "n/a";
                }
            }
            set
            {
                Root.ActiveQbook.Book.Version = value;
            }
        }
        /// <summary>
        /// Holds the last epoch when the qbook was saved
        /// </summary>
        public static long VersionEpoch
        {
            get
            {
                try
                {
                    return Root.ActiveQbook.Book.VersionEpoch;
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
            internal set
            {
                Root.ActiveQbook.Book.VersionEpoch = value;
            }
        }

        //public enum ALevel { Online, Offline, User, Service, Admin, Root };
        static AccessLevel _AccessLevel = AccessLevel.Offline;
        public static AccessLevel AccessLevel
        {
            get { return _AccessLevel; }
            set
            {
                Logger.Info($"new AccessLevel: {_AccessLevel} -> {value}");
                bool changed = _AccessLevel != value;
                _AccessLevel = value;
                if (changed)
                {
                    Logger.Info($"new AccessLevel: changed; book={(QB.Root.ActiveQbook)}");
                    Root.ActiveQbook?.OnPropertyChangedEvent("AccessLevel", _AccessLevel);
                }
                else
                {
                    Logger.Info($"new AccessLevel: not changed");
                }
            }
        }

  


    }
}
