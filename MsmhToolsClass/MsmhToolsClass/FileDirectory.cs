using System.Text;
using System.Diagnostics;

namespace MsmhToolsClass;

public class FileDirectory
{
    //-----------------------------------------------------------------------------------
    public static async Task<List<string>> GetAllFilesAsync(string dirPath, CancellationToken ct = default)
    {
        return await Task.Run(async () =>
        {
            List<string> paths = new();

            try
            {
                if (!Directory.Exists(dirPath)) return paths;
                DirectoryInfo rootDirInfo = new(dirPath);

                FileInfo[] fileInfos = rootDirInfo.GetFiles();
                for (int n = 0; n < fileInfos.Length; n++)
                {
                    if (ct.IsCancellationRequested) break;
                    FileInfo fileInfo = fileInfos[n];
                    paths.Add(fileInfo.FullName);
                }

                DirectoryInfo[] dirInfos = rootDirInfo.GetDirectories();
                for (int n = 0; n < dirInfos.Length; n++)
                {
                    if (ct.IsCancellationRequested) break;
                    DirectoryInfo dirInfo = dirInfos[n];
                    paths.AddRange(await GetAllFilesAsync(dirInfo.FullName, ct));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FileDirectory GetAllFiles: " + ex.Message);
            }

            return paths;
        }, ct);
    }
    //-----------------------------------------------------------------------------------
    public static async Task MoveDirectoryAsync(string sourceDir, string destDir, bool overWrite, CancellationToken ct)
    {
        await Task.Run(async () =>
        {
            try
            {
                if (!Directory.Exists(sourceDir)) return;
                CreateEmptyDirectory(destDir);
                DirectoryInfo rootDirInfo = new(sourceDir);

                FileInfo[] fileInfos = rootDirInfo.GetFiles();
                for (int n = 0; n < fileInfos.Length; n++)
                {
                    if (ct.IsCancellationRequested) break;
                    FileInfo fileInfo = fileInfos[n];
                    fileInfo.MoveTo(Path.GetFullPath(Path.Combine(destDir, fileInfo.Name)), overWrite);
                }

                DirectoryInfo[] dirInfos = rootDirInfo.GetDirectories();
                for (int n = 0; n < dirInfos.Length; n++)
                {
                    if (ct.IsCancellationRequested) break;
                    DirectoryInfo dirInfo = dirInfos[n];
                    await MoveDirectoryAsync(dirInfo.FullName, Path.GetFullPath(Path.Combine(destDir, dirInfo.Name)), overWrite, ct);
                }

                if (Directory.Exists(sourceDir)) Directory.Delete(sourceDir, false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FileDirectory MoveDirectory: " + ex.Message);
            }
        }, ct);
    }
    //-----------------------------------------------------------------------------------
    public static bool IsPathTooLong(string path)
    {
        try
        {
            Path.GetFullPath(path);
            return false;
        }
        catch (PathTooLongException)
        {
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    //-----------------------------------------------------------------------------------
    public static bool IsRootDirectory(string? dirPath = null)
    {
        bool isRoot = false;
        try
        {
            DirectoryInfo? info = null;
            if (string.IsNullOrEmpty(dirPath))
                info = new(Path.GetFullPath(AppContext.BaseDirectory));
            else
                info = new(Path.GetFullPath(dirPath));
            if (info.Parent == null) isRoot = true;
        }
        catch (Exception)
        {
            isRoot = true;
        }
        return isRoot;
    }
    //-----------------------------------------------------------------------------------
    public static async Task<bool> IsFileEmptyAsync(string filePath)
    {
        if (!File.Exists(filePath)) return true;
        string content = string.Empty;
        try
        {
            content = await File.ReadAllTextAsync(filePath);
        }
        catch (Exception) { }
        return content.Length == 0;
    }
    //-----------------------------------------------------------------------------------
    public static bool IsDirectoryEmpty(string dirPath)
    {
        if (!Directory.Exists(dirPath)) return true;
        try
        {
            string[] files = Directory.GetFiles(dirPath);
            string[] folders = Directory.GetDirectories(dirPath);
            return !files.Any() && !folders.Any();
        }
        catch (Exception)
        { 
            return false;
        }
    }
    //-----------------------------------------------------------------------------------
    /// <summary>
    /// Creates an empty file if not already exist.
    /// </summary>
    public static void CreateEmptyFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) File.Create(filePath).Dispose();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory CreateEmptyFile: " + ex.Message);
        }
    }
    //-----------------------------------------------------------------------------------
    /// <summary>
    /// Creates an empty directory if not already exist.
    /// </summary>
    public static void CreateEmptyDirectory(string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory CreateEmptyDirectory: " + ex.Message);
        }
    }
    //-----------------------------------------------------------------------------------
    public static bool CompareByLength(string path1, string path2)
    {
        try
        {
            int path1Length = File.ReadAllText(path1).Length;
            int path2Length = File.ReadAllText(path2).Length;
            return path1Length == path2Length;
        }
        catch (Exception)
        {
            return false;
        }
    }
    //-----------------------------------------------------------------------------------
    public static bool CompareByReadBytes(string path1, string path2)
    {
        try
        {
            byte[] path1Bytes = File.ReadAllBytes(path1);
            byte[] path2Bytes = File.ReadAllBytes(path2);
            return path1Bytes == path2Bytes;
        }
        catch (Exception)
        {
            return false;
        }
    }
    //-----------------------------------------------------------------------------------
    public static bool CompareByUTF8Bytes(string path1, string path2)
    {
        try
        {
            byte[] path1Bytes = Encoding.UTF8.GetBytes(path1);
            byte[] path2Bytes = Encoding.UTF8.GetBytes(path2);
            return path1Bytes == path2Bytes;
        }
        catch (Exception)
        {
            return false;
        }
    }
    //-----------------------------------------------------------------------------------
    public static bool CompareBySHA512(string path1, string path2)
    {
        string path1CRC = EncodingTool.GetSHA512(path1);
        string path2CRC = EncodingTool.GetSHA512(path2);
        return path1CRC == path2CRC;
    }
    //-----------------------------------------------------------------------------------
    public static bool CompareByReadLines(string path1, string path2)
    {
        try
        {
            return File.ReadLines(path1).SequenceEqual(File.ReadLines(path2));
        }
        catch (Exception)
        {
            return false;
        }
    }
    //-----------------------------------------------------------------------------------
    public static void AppendTextLine(string filePath, string textToAppend, Encoding encoding)
    {
        try
        {
            if (!File.Exists(filePath)) CreateEmptyFile(filePath);

            using FileStream fileStream = new(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using StreamWriter writer = new(fileStream, encoding);
            writer.WriteLine(textToAppend);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory AppendTextLine: " + ex.Message);
        }
    }
    //-----------------------------------------------------------------------------------
    public static async Task AppendTextLineAsync(string filePath, string textToAppend, Encoding encoding)
    {
        try
        {
            if (!File.Exists(filePath)) CreateEmptyFile(filePath);

            using FileStream fileStream = new(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using StreamWriter writer = new(fileStream, encoding);
            await writer.WriteLineAsync(textToAppend);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory AppendTextLineAsync: " + ex.Message);
        }
    }
    //-----------------------------------------------------------------------------------
    public static void AppendText(string filePath, string textToAppend, Encoding encoding)
    {
        try
        {
            using FileStream fileStream = new(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using StreamWriter writer = new(fileStream, encoding);
            writer.Write(textToAppend);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory AppendText: " + ex.Message);
        }
    }
    //-----------------------------------------------------------------------------------
    public static async Task AppendTextAsync(string filePath, string textToAppend, Encoding encoding)
    {
        try
        {
            using FileStream fileStream = new(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using StreamWriter writer = new(fileStream, encoding);
            await writer.WriteAsync(textToAppend);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory AppendTextAsync: " + ex.Message);
        }
    }
    //-----------------------------------------------------------------------------------
    public static void WriteAllText(string filePath, string fileContent, Encoding encoding)
    {
        try
        {
            using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using StreamWriter writer = new(fileStream, encoding);
            //fileStream.SetLength(0); // Overwrite File When FileMode is FileMode.OpenOrCreate
            writer.AutoFlush = true;
            writer.Write(fileContent);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory WriteAllText: " + ex.Message);
        }
    }
    //-----------------------------------------------------------------------------------
    public static async Task WriteAllTextAsync(string filePath, string fileContent, Encoding encoding)
    {
        try
        {
            using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using StreamWriter writer = new(fileStream, encoding);
            //fileStream.SetLength(0); // Overwrite File When FileMode is FileMode.OpenOrCreate
            writer.AutoFlush = true;
            await writer.WriteAsync(fileContent);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory WriteAllTextAsync: " + ex.Message);
        }
    }
    //-----------------------------------------------------------------------------------
    public static bool IsFileLocked(string fileNameOrPath)
    {
        try
        {
            string filePath = Path.GetFullPath(fileNameOrPath);
            if (File.Exists(filePath))
            {
                FileStream? stream = null;
                FileInfo fileInfo = new(filePath);
                try
                {
                    stream = fileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                }
                catch (IOException e) when ((e.HResult & 0x0000FFFF) == 32)
                {
                    Console.WriteLine("File is in use by another process.");
                    return true;
                }
                finally
                {
                    stream?.Close();
                }
                //file is not locked
                return false;
            }
            else
            {
                Console.WriteLine("File not exist: " + filePath);
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory IsFileLocked: " + ex.Message);
            return true;
        }
    }
    //-----------------------------------------------------------------------------------
    public static List<string>? FindFilesByPartialName(string partialName, string dirPath)
    {
        try
        {
            if (Directory.Exists(dirPath))
            {
                DirectoryInfo hdDirectoryInWhichToSearch = new(dirPath);
                FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles("*" + partialName + "*.*");
                List<string> list = new();
                foreach (FileInfo foundFile in filesInDir)
                {
                    string fullName = foundFile.FullName;
                    list.Add(fullName);
                }
                return list;
            }
            Console.WriteLine("FileDirectory Directory Not Exist: " + dirPath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory FindFilesByPartialName: " + ex.Message);
        }

        return null;
    }
    //-----------------------------------------------------------------------------------
}