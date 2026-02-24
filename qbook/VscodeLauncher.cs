using System;
using System.Diagnostics;
using System.IO;

public static class VsCodeLauncher
{
    public static bool OpenFolder(string folderPath, bool newWindow = true)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"Ordner nicht gefunden: {folderPath}");

        string windowArg = newWindow ? "--new-window" : "--reuse-window";

        // 1) Erst 'code' aus PATH versuchen (ohne sichtbare Konsole)
        try
        {
            var psiCli = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c code {windowArg} \"{folderPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process? p = Process.Start(psiCli);
            if (p != null)
                return true;
        }
        catch
        {
            // Fallback unten
        }

        // 2) Typische Installationspfade suchen (userunabhängig)
        string? codeExe = TryFindVsCodeExe();
        if (codeExe == null)
            return false;

        var psiExe = new ProcessStartInfo
        {
            FileName = codeExe,
            Arguments = $"{windowArg} \"{folderPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        Process.Start(psiExe);
        return true;
    }

    private static string? TryFindVsCodeExe()
    {
        // User-Installation (beliebiger User auf dem aktuellen System)
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string userInstall = Path.Combine(localAppData, "Programs", "Microsoft VS Code", "Code.exe");

        // Systemweite Installation
        string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        string machineInstall = Path.Combine(programFiles, "Microsoft VS Code", "Code.exe");

        // 32-Bit Program Files auf 64-Bit Windows (selten nötig, aber möglich)
        string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        string machineInstallX86 = Path.Combine(programFilesX86, "Microsoft VS Code", "Code.exe");

        if (File.Exists(userInstall)) return userInstall;
        if (File.Exists(machineInstall)) return machineInstall;
        if (File.Exists(machineInstallX86)) return machineInstallX86;

        return null;
    }
}