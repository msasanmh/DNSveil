using System.Text;
using System.Diagnostics;

namespace MsmhToolsClass;

public static class FileDirectory
{
    public static bool IsPathTooLong(string path)
    {
        try
        {
            _ = Path.GetFullPath(path);
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

    public static async Task<bool> IsFileLockedAsync(string fileNameOrPath)
    {
        bool isFileLocked = false;

        try
        {
            string filePath = Path.GetFullPath(fileNameOrPath);
            if (File.Exists(filePath))
            {
                FileStream? fileStream = null;
                FileInfo fileInfo = new(filePath);

                try
                {
                    fileStream = fileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                }
                catch (IOException e) when ((e.HResult & 0x0000FFFF) == 32)
                {
                    Debug.WriteLine("FileDirectory IsFileLocked: File Is In Use By Another Process: " + e.Message);
                    isFileLocked = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("FileDirectory IsFileLocked: " + ex.Message);
                    isFileLocked = true;
                }
                finally
                {
                    await fileStream!.DisposeAsync();
                }
            }
            else
            {
                Debug.WriteLine("FileDirectory IsFileLocked: File Not Exist: " + filePath);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory IsFileLocked: " + ex.Message);
            isFileLocked = true;
        }

        return isFileLocked;
    }

    public static bool IsFileNewer(string newFilePath, string currentFilePath)
    {
        try
        {
            FileInfo infoNewFile = new(Path.GetFullPath(newFilePath));
            FileInfo infoCurrentFile = new(Path.GetFullPath(currentFilePath));
            return infoNewFile.LastWriteTime > infoCurrentFile.LastWriteTime;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory IsFileNewer: " + ex.Message);
            return false;
        }
    }

    public static void AppendText(string filePath, string textToAppend, Encoding encoding)
    {
        try
        {
            using FileStream fileStream = new(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using StreamWriter streamWriter = new(fileStream, encoding);
            streamWriter.Write(textToAppend);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory AppendText: " + ex.Message);
        }
    }

    public static async Task AppendTextAsync(string filePath, string textToAppend, Encoding encoding)
    {
        try
        {
            using FileStream fileStream = new(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using StreamWriter streamWriter = new(fileStream, encoding);
            await streamWriter.WriteAsync(textToAppend);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory AppendTextAsync: " + ex.Message);
        }
    }

    public static void AppendTextLine(string filePath, string textToAppend, Encoding encoding)
    {
        try
        {
            using FileStream fileStream = new(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using StreamWriter streamWriter = new(fileStream, encoding);
            streamWriter.WriteLine(textToAppend);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory AppendTextLine: " + ex.Message);
        }
    }

    public static async Task AppendTextLineAsync(string filePath, string textToAppend, Encoding encoding)
    {
        try
        {
            using FileStream fileStream = new(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using StreamWriter streamWriter = new(fileStream, encoding);
            await streamWriter.WriteLineAsync(textToAppend);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory AppendTextLineAsync: " + ex.Message);
        }
    }

    public static void WriteAllText(string filePath, string fileContent, Encoding encoding)
    {
        try
        {
            using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using StreamWriter streamWriter = new(fileStream, encoding);
            //fileStream.SetLength(0); // Overwrite File When FileMode is FileMode.OpenOrCreate
            streamWriter.AutoFlush = true;
            streamWriter.Write(fileContent);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory WriteAllText: " + ex.Message);
        }
    }

    public static async Task<bool> WriteAllTextAsync(string filePath, string fileContent, Encoding encoding)
    {
        try
        {
            using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using StreamWriter streamWriter = new(fileStream, encoding);
            //fileStream.SetLength(0); // Overwrite File When FileMode is FileMode.OpenOrCreate
            streamWriter.AutoFlush = true;
            await streamWriter.WriteAsync(fileContent);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory WriteAllTextAsync: " + ex.Message);
            return false;
        }
    }

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

    public static async Task<bool> CompareByLengthAsync(string path1, string path2)
    {
        try
        {
            string path1Str = await File.ReadAllTextAsync(path1);
            string path2Str = await File.ReadAllTextAsync(path2);
            return path1Str.Length == path2Str.Length;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
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

    public static async Task<bool> CompareByReadBytesAsync(string path1, string path2)
    {
        try
        {
            byte[] path1Bytes = await File.ReadAllBytesAsync(path1);
            byte[] path2Bytes = await File.ReadAllBytesAsync(path2);
            return path1Bytes == path2Bytes;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
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
    
    public static async Task<bool> CompareBySHA512Async(string path1, string path2)
    {
        try
        {
            string content1 = await File.ReadAllTextAsync(path1);
            bool isSuccess1 = EncodingTool.TryGetSHA512(content1, out string path1CRC);
            if (!isSuccess1) return false;
            string content2 = await File.ReadAllTextAsync(path2);
            bool isSuccess2 = EncodingTool.TryGetSHA512(content2, out string path2CRC);
            if (!isSuccess2) return false;
            return path1CRC == path2CRC;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory CompareBySHA512Async: " + ex.Message);
            return false;
        }
    }
    
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
            Debug.WriteLine("FileDirectory FindFilesByPartialName: Directory Not Exist: " + dirPath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FileDirectory FindFilesByPartialName: " + ex.Message);
        }

        return null;
    }
    
}