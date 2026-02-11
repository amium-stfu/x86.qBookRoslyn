using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace qbook
{
    internal class MruFilesManager
    {
        public class MruItem
        {
            public MruItem(string fullPath)
            {
                this.FullPath = fullPath;
            }

            string _FullPath = null;
            public string FullPath
            {
                get
                {
                    return _FullPath;
                }
                set
                {
                    _FullPath = value;
                }
            }

            string Filename
            {
                get
                {
                    return System.IO.Path.GetFileName(_FullPath);
                }
            }
            string Directory
            {
                get
                {
                    return System.IO.Path.GetDirectoryName(_FullPath);
                }
            }

            public bool FileExists
            {
                get { return System.IO.File.Exists(_FullPath); }
            }

            public DateTime? LastModified
            {
                get
                {
                    if (System.IO.File.Exists(_FullPath))
                        return new FileInfo(_FullPath).LastWriteTime;
                    else
                        return null;
                }
            }

        }

        List<MruItem> mruFiles = new List<MruItem>();

        public int Size = 6;

        public void Add(string fullPath)
        {
            mruFiles.RemoveAll(mru => mru.FullPath == fullPath);

            if (mruFiles.Count >= Size)
                mruFiles.RemoveAt(mruFiles.Count - 1);

            mruFiles.Insert(0, new MruItem(fullPath));
        }

        public void Clear()
        {
            mruFiles.Clear();
        }

        public void Remove(string fullPath)
        {
            mruFiles.RemoveAll(mru => mru.FullPath == fullPath);
        }

        public void RemoveNonExisting()
        {
            mruFiles.RemoveAll(mru => !mru.FileExists);
        }

        public List<string> GetMruStringList()
        {
            List<string> list = new List<string>();
            foreach (var item in mruFiles)
            {
                string info = "";
                //if (!item.FileExists)
                //    info += "[x] ";
                info += item.FullPath;
                list.Add(info);
            }
            return list;
        }

        public List<MruItem> MruItems
        {
            get
            {
                return mruFiles;
            }
        }

        public string GetMruCsvString()
        {
            return string.Join(",", GetMruStringList());
        }

        public void SetMruByCsvString(string csv)
        {
            var items = csv.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in items.Reverse())
            {
                Add(item);
            }
        }
    }
}
