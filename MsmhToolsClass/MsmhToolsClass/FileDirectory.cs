using System.Text;
using System.Diagnostics;

namespace MsmhToolsClass;

public class FileDirectory
{
    //-----------------------------------------------------------------------------------
    public static async Task MoveDirectory(string sourceDir, string destDir, bool overWrite, CancellationToken token)
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
                    FileInfo fileInfo = fileInfos[n];
                    fileInfo.MoveTo(Path.GetFullPath(Path.Combine(destDir, fileInfo.Name)), overWrite);
                }

                DirectoryInfo[] dirInfos = rootDirInfo.GetDirectories();
                for (int n2 = 0; n2 < dirInfos.Length; n2++)
                {
                    DirectoryInfo dirInfo = dirInfos[n2];
                    await MoveDirectory(dirInfo.FullName, Path.GetFullPath(Path.Combine(destDir, dirInfo.Name)), overWrite, token);
                }

                if (Directory.Exists(sourceDir)) Directory.Delete(sourceDir, false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MoveDirectory: " + ex.Message);
            }
        }, token);
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
        if (!File.Exists(filePath))
            File.Create(filePath).Dispose();
    }
    //-----------------------------------------------------------------------------------
    /// <summary>
    /// Creates an empty directory if not already exist.
    /// </summary>
    public static void CreateEmptyDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);
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
            Debug.WriteLine(ex.Message);
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
            Debug.WriteLine("AppendTextLineAsync: " + ex.Message);
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
            Debug.WriteLine("AppendText: " + ex.Message);
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
            Debug.WriteLine("AppendTextAsync: " + ex.Message);
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
            Debug.WriteLine("WriteAllText: " + ex.Message);
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
            Debug.WriteLine("WriteAllTextAsync: " + ex.Message);
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
            Debug.WriteLine("IsFileLocked: " + ex.Message);
            return true;
        }
    }
    //-----------------------------------------------------------------------------------
    public static List<string>? FindFilesByPartialName(string partialName, string dirPath)
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
        Console.WriteLine("Directory Not Exist: " + dirPath);
        return null;
    }
    //-----------------------------------------------------------------------------------
    // ===== Old Methods ================================================================
    public static byte[] ReadAllBytes(MemoryStream memoryStream)
    {
        return memoryStream.ToArray();
    }
    //-----------------------------------------------------------------------------------
    public static byte[]? ReadAllBytes(string filePath)
    {
        try
        {
            using FileStream fsSource = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            // Read the source file into a byte array.
            byte[] bytes = new byte[fsSource.Length];
            int numBytesToRead = (int)fsSource.Length;
            int numBytesRead = 0;
            while (numBytesToRead > 0)
            {
                // Read may return anything from 0 to numBytesToRead.
                int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);
                // Break when the end of the file is reached.
                if (n == 0) break;
                numBytesRead += n;
                numBytesToRead -= n;
            }
            numBytesToRead = bytes.Length;
            return bytes;
        }
        catch (FileNotFoundException ioEx)
        {
            Console.WriteLine(ioEx.Message);
            return null;
        }
    }
    //-----------------------------------------------------------------------------------
    public static async Task<byte[]?> ReadAllBytesAsync(string filePath)
    {
        try
        {
            using FileStream fsSource = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            // Read the source file into a byte array.
            byte[] bytes = new byte[fsSource.Length];
            int numBytesToRead = (int)fsSource.Length;
            int numBytesRead = 0;
            while (numBytesToRead > 0)
            {
                // Read may return anything from 0 to numBytesToRead.
                int n = await fsSource.ReadAsync(bytes.AsMemory(numBytesRead, numBytesToRead));
                // Break when the end of the file is reached.
                if (n == 0) break;
                numBytesRead += n;
                numBytesToRead -= n;
            }
            numBytesToRead = bytes.Length;
            return bytes;
        }
        catch (FileNotFoundException ioEx)
        {
            Console.WriteLine(ioEx.Message);
            return null;
        }
    }
    //-----------------------------------------------------------------------------------
    public static void WriteAllBytes(string filePath, byte[] bytes)
    {
        try
        {
            int numBytesToRead = bytes.Length;
            // Write the byte array to the other FileStream.
            using FileStream fsNew = new(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            fsNew.Write(bytes, 0, numBytesToRead);
        }
        catch (FileNotFoundException ioEx)
        {
            Console.WriteLine(ioEx.Message);
        }
    }
    //-----------------------------------------------------------------------------------
    public static async Task WriteAllBytesAsync(string filePath, byte[] bytes)
    {
        try
        {
            int numBytesToRead = bytes.Length;
            // Write the byte array to the other FileStream.
            using FileStream fsNew = new(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            await fsNew.WriteAsync(bytes.AsMemory(0, numBytesToRead));
        }
        catch (FileNotFoundException ioEx)
        {
            Console.WriteLine(ioEx.Message);
        }
    }
    //-----------------------------------------------------------------------------------
}