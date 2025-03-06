using System.Diagnostics;

namespace MsmhToolsClass;

public static class OpenLinks
{
    public static void OpenFolderFromFileName(string fileName)
    {
        try
        {
            string? folderName = Path.GetDirectoryName(fileName);
            if (string.IsNullOrEmpty(folderName)) return;
            if (Info.IsRunningOnWindows)
            {
                var argument = @"/select, " + fileName;
                Process.Start("explorer.exe", argument);
            }
            else
            {
                OpenFolder(folderName);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("OpenLinks OpenFolderFromFileName: " + ex.Message);
        }
    }

    public static void OpenFolder(string folder)
    {
        OpenItem(folder, "folder");
    }

    public static void OpenUrl(string url)
    {
        OpenItem(url, "url");
    }

    public static void OpenFile(string file)
    {
        OpenItem(file, "file");
    }

    public static void OpenItem(string item, string type)
    {
        try
        {
            if (Info.IsRunningOnWindows || Info.IsRunningOnMac)
            {
                var startInfo = new ProcessStartInfo(item)
                {
                    UseShellExecute = true
                };

                Process.Start(startInfo);
            }
            else if (Info.IsRunningOnLinux)
            {
                Process process = new()
                {
                    EnableRaisingEvents = false,
                    StartInfo = { FileName = "xdg-open", Arguments = item }
                };
                process.Start();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OpenLinks Cannot open {type}: {item}{Environment.NewLine}{Environment.NewLine}{ex.Source}: {ex.Message}");
        }
    }
}