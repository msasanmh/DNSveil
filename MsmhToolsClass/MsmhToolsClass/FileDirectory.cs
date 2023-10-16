using System;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;

namespace MsmhToolsClass;

public class FileDirectory
{
    //-----------------------------------------------------------------------------------

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
        int path1Length = File.ReadAllText(path1).Length;
        int path2Length = File.ReadAllText(path2).Length;
        if (path1Length == path2Length)
            return true;
        else
            return false;
    }
    //-----------------------------------------------------------------------------------
    public static bool CompareByReadBytes(string path1, string path2)
    {
        byte[] path1Bytes = File.ReadAllBytes(path1);
        byte[] path2Bytes = File.ReadAllBytes(path2);
        if (path1Bytes == path2Bytes)
            return true;
        else
            return false;
    }
    //-----------------------------------------------------------------------------------
    public static bool CompareByUTF8Bytes(string path1, string path2)
    {
        byte[] path1Bytes = Encoding.UTF8.GetBytes(path1);
        byte[] path2Bytes = Encoding.UTF8.GetBytes(path2);
        if (path1Bytes == path2Bytes)
            return true;
        else
            return false;
    }
    //-----------------------------------------------------------------------------------
    public static bool CompareBySHA512(string path1, string path2)
    {
        string path1CRC = GetSHA512(path1);
        string path2CRC = GetSHA512(path2);
        if (path1CRC == path2CRC)
            return true;
        else
            return false;
    }
    //-----------------------------------------------------------------------------------
    public static bool CompareByReadLines(string path1, string path2)
    {
        return File.ReadLines(path1).SequenceEqual(File.ReadLines(path2));
    }
    //-----------------------------------------------------------------------------------
    public static void AppendTextLine(string filePath, string textToAppend, Encoding encoding)
    {
        try
        {
            if (!File.Exists(filePath))
                CreateEmptyFile(filePath);

            string fileContent = File.ReadAllText(filePath);
            List<string> splitByLine = fileContent.SplitToLines();
            int count = splitByLine.Count;
            using FileStream fileStream = new(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using StreamWriter writer = new(fileStream, encoding);
            if (count == 0)
                writer.Write(textToAppend);
            else
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
        if (!File.Exists(filePath))
            CreateEmptyFile(filePath);

        string fileContent = await File.ReadAllTextAsync(filePath);
        List<string> splitByLine = fileContent.SplitToLines();
        int count = splitByLine.Count;
        using FileStream fileStream = new(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        using StreamWriter writer = new(fileStream, encoding);
        if (count == 0)
            await writer.WriteAsync(textToAppend);
        else
            await writer.WriteLineAsync(textToAppend);
    }
    //-----------------------------------------------------------------------------------
    public static void AppendText(string filePath, string textToAppend, Encoding encoding)
    {
        using FileStream fileStream = new(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        using StreamWriter writer = new(fileStream, encoding);
        writer.Write(textToAppend);
    }
    //-----------------------------------------------------------------------------------
    public static async Task AppendTextAsync(string filePath, string textToAppend, Encoding encoding)
    {
        using FileStream fileStream = new(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        using StreamWriter writer = new(fileStream, encoding);
        await writer.WriteAsync(textToAppend);
    }
    //-----------------------------------------------------------------------------------
    public static void WriteAllText(string filePath, string fileContent, Encoding encoding)
    {
        using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        using StreamWriter writer = new(fileStream, encoding);
        //fileStream.SetLength(0); // Overwrite File When FileMode is FileMode.OpenOrCreate
        writer.AutoFlush = true;
        writer.Write(fileContent);
    }
    //-----------------------------------------------------------------------------------
    public static async Task WriteAllTextAsync(string filePath, string fileContent, Encoding encoding)
    {
        using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        using StreamWriter writer = new(fileStream, encoding);
        //fileStream.SetLength(0); // Overwrite File When FileMode is FileMode.OpenOrCreate
        writer.AutoFlush = true;
        await writer.WriteAsync(fileContent);
    }
    //-----------------------------------------------------------------------------------
    public static bool IsFileLocked(string fileNameOrPath)
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
    public static string GetSHA512(string filePath)
    {
        if (File.Exists(filePath))
        {
            byte[]? bytes = ReadAllBytes(filePath);
            using var hash = SHA512.Create();
            byte[] hashedInputBytes = bytes != null ? hash.ComputeHash(bytes) : Array.Empty<byte>();
            // Convert to text
            // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
            var hashedInputStringBuilder = new StringBuilder(128);
            foreach (byte b in hashedInputBytes)
                hashedInputStringBuilder.Append(b.ToString("X2"));
            return hashedInputStringBuilder.ToString();
        }
        return string.Empty;
    }
    //-----------------------------------------------------------------------------------
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
                if (n == 0)
                    break;
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
                if (n == 0)
                    break;
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

    //-----------------------------------------------------------------------------------
}