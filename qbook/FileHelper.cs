using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace qbook
{
    public static class FileHelper
    {
        public static void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string targetFilePath = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, targetFilePath, overwrite: true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string targetSubDir = Path.Combine(targetDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, targetSubDir);
            }
        }

        public static async Task DeleteDirectorySafeAsync(string path, int maxRetries = 5, int delayMs = 200)
        {
            if (!Directory.Exists(path)) return;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    Directory.Delete(path, recursive: true);
                    return;
                }
                catch (IOException)
                {
                    await Task.Delay(delayMs);
                }
                catch (UnauthorizedAccessException)
                {
                    await Task.Delay(delayMs);
                }
            }

            throw new IOException($"Konnte Verzeichnis '{path}' nach {maxRetries} Versuchen nicht löschen.");
        }

        public static bool IsFileLocked(string filePath)
        {
            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
        }
        public static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        public static void DeleteDirectorySafe(string path)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, recursive: true);
                    }
                    return;
                }
                catch (IOException)
                {
                    Thread.Sleep(200); // kurz warten und nochmal versuchen
                }
            }
        }

    }
}
