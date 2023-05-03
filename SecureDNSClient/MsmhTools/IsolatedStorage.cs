using System;
using System.IO.IsolatedStorage;
using System.Text;
using System.Security.Cryptography;
using Force.Crc32;
using System.Diagnostics;

namespace MsmhTools
{
    public class IsolatedStorage
    {
        //-----------------------------------------------------------------------------------
        public static IDictionary<string, string>? DicLineByLine(string fileName)
        {
            string? read = ReadIsolatedTextFile(fileName);
            if (read == null) return null;
            string[] split1 = read.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            IDictionary<string, string> Dic = new Dictionary<string, string>();
            int a = 0;
            int b = 1;
            for (; b < split1.Length; a += 2, b += 2)
            {
                if (!Dic.ContainsKey(split1[a]))
                    Dic.Add(split1[a], split1[b]);
            }
            return Dic;
        }
        //-----------------------------------------------------------------------------------
        public static List<string>? ListLineByLine(string fileName)
        {
            string? read = ReadIsolatedTextFile(fileName);
            if (read == null) return null;
            string[] split1 = read.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            List<string> list = new();
            foreach (var line in split1)
                list.Add(line);
            return list;
            // Usage: List<string> items = ListLineByLine();
        }
        //-----------------------------------------------------------------------------------
        public static int? CountLines(string fileName)
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            if (isoStore.FileExists(fileName))
            {
                using IsolatedStorageFileStream isoStream = new(fileName, FileMode.Open, isoStore);
                using StreamReader reader = new(isoStream);
                int count = 0;
                while (reader.ReadLine() != null)
                {
                    count++;
                }
                isoStore.Close();
                return count;
            }
            return null;
        }
        //-----------------------------------------------------------------------------------
        public static int FilesTotalNumber
        {
            get
            {
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
                int count = 0;
                // Retrieve all the files in the directory by calling the GetAllFiles
                foreach (string file in GetAllFiles("*", isoStore))
                {
                    if (isoStore.FileExists(file))
                        count++;
                }
                return count;
            }
        }
        //-----------------------------------------------------------------------------------
        public static bool IsFileExist(string fileName)
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            if (isoStore.FileExists(fileName))
            {
                isoStore.Close();
                //Console.WriteLine("File Exist: " + fileName);
                return true;
            }
            isoStore.Close();
            Debug.WriteLine("File Not Exist: " + fileName);
            return false;
        }
        public static bool IsDirectoryExist(string directoryName)
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            if (isoStore.DirectoryExists(directoryName))
            {
                isoStore.Close();
                Debug.WriteLine("Directory Exist: " + directoryName);
                return true;
            }
            isoStore.Close();
            Debug.WriteLine("Directory Not Exist: " + directoryName);
            return false;
        }
        //-----------------------------------------------------------------------------------
        public static string GetCRC32(string fileName)
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            if (isoStore.FileExists(fileName))
            {
                IsolatedStorageFileStream isoStream = new(fileName, FileMode.Open, isoStore);
                StreamReader reader = new(isoStream);
                var r = reader.ReadToEnd();
                var bytes = Encoding.UTF8.GetBytes(r);
                uint crc32 = Crc32Algorithm.Compute(bytes);
                reader.Close();
                isoStream.Close();
                isoStore.Close();
                return crc32.ToString();
            }
            Console.WriteLine("File Not Exist: " + fileName);
            return string.Empty;
        }
        //-----------------------------------------------------------------------------------
        public static string GetSHA512(string fileName)
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            if (isoStore.FileExists(fileName))
            {
                IsolatedStorageFileStream isoStream = new(fileName, FileMode.Open, isoStore);
                StreamReader reader = new(isoStream);
                var r = reader.ReadToEnd();
                var bytes = Encoding.UTF8.GetBytes(r);
                using var hash = SHA512.Create();
                var hashedInputBytes = hash.ComputeHash(bytes);
                // Convert to text
                // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
                var hashedInputStringBuilder = new StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                reader.Close();
                isoStream.Close();
                isoStore.Close();
                return hashedInputStringBuilder.ToString();
            }
            return string.Empty;
        }
        //-----------------------------------------------------------------------------------
        public static async void SaveIsolatedTextFile(string fileName, string content, Encoding encoding)
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            //if (isoStore.FileExists("multiple_replace.xml")) { isoStore.DeleteFile("multiple_replace.xml"); }
            using IsolatedStorageFileStream isoStream = new(fileName, FileMode.Create, isoStore);
            using StreamWriter writer = new(isoStream, encoding);
            await writer.WriteAsync(content);
            isoStore.Close();
        }
        //-----------------------------------------------------------------------------------
        public static void SaveIsolatedTextFileAppend(string fileName, string content)
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            //if (isoStore.FileExists("multiple_replace.xml")) { isoStore.DeleteFile("multiple_replace.xml"); }
            if (isoStore.FileExists(fileName))
            {
                using IsolatedStorageFileStream isoStream = new(fileName, FileMode.Append, isoStore);
                using StreamWriter writer = new(isoStream);
                writer.Write(content);
            }
            else
            {
                using IsolatedStorageFileStream isoStream = new(fileName, FileMode.CreateNew, isoStore);
                using StreamWriter writer = new(isoStream);
                writer.Write(content);
            }
            isoStore.Close();
        }
        //-----------------------------------------------------------------------------------
        public static string? ReadIsolatedTextFile(string fileName)
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            //if (isoStore.FileExists("multiple_replace.xml")) { isoStore.DeleteFile("multiple_replace.xml"); }

            if (isoStore.FileExists(fileName))
            {
                using IsolatedStorageFileStream isoStream = new(fileName, FileMode.Open, FileAccess.Read, isoStore);
                using StreamReader reader = new(isoStream);
                var r = reader.ReadToEnd();
                isoStore.Close();
                return r;
            }
            else
            {
                Debug.WriteLine("Isolated Storage File Does Not Exist.");
                isoStore.Close();
                return null;
            }
        }
        //-----------------------------------------------------------------------------------
        public static void RemoveIsolatedStorage()
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            try
            {
                isoStore.Remove();
                Debug.WriteLine("Isolated Storage removed.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
            isoStore.Close();
            isoStore.Dispose();
        }
        //-----------------------------------------------------------------------------------
        public static void DeleteFile(string fileName)
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            // Retrieve all the files in the directory by calling the GetAllFiles
            foreach (string file in GetAllFiles(fileName, isoStore))
            {
                try
                {
                    if (isoStore.FileExists(file))
                        isoStore.DeleteFile(file);
                    if (!isoStore.FileExists(file))
                        Debug.WriteLine("File Deleted: " + file);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex.Message);
                }
            }
        }
        //-----------------------------------------------------------------------------------
        public static void DeleteIsolatedFilesAndDirectories()
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            // Retrieve all the files in the directory by calling the GetAllFiles
            foreach (string file in GetAllFiles("*", isoStore))
            {
                try
                {
                    if (isoStore.FileExists(file))
                        isoStore.DeleteFile(file);
                    if (!isoStore.FileExists(file))
                        Console.WriteLine("File Deleted: " + file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    MessageBox.Show("Error: " + ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            // Retrieve all the directories in Isolated Storage by calling the GetAllDirectories
            foreach (string directory in GetAllDirectories("*", isoStore))
            { // Exception will thrown when directory in not empty or exist, so delete directories after deleting files.
                try
                {
                    if (isoStore.DirectoryExists(directory))
                        isoStore.DeleteDirectory(directory);
                    if (!isoStore.DirectoryExists(directory))
                        Console.WriteLine("Directory Deleted: " + directory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    MessageBox.Show("Error: " + ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        //-----------------------------------------------------------------------------------
        // Method to retrieve all directories, recursively, within a store.
        private static List<string> GetAllDirectories(string pattern, IsolatedStorageFile storeFile)
        {
            // Get the root of the search string.
            string root = Path.GetDirectoryName(pattern);
            if (root != "")
            {
                root += "/";
            }
            // Retrieve directories.
            List<string> directoryList = new(storeFile.GetDirectoryNames(pattern));
            // Retrieve subdirectories of matches.
            for (int i = 0, max = directoryList.Count; i < max; i++)
            {
                string directory = directoryList[i] + "/";
                List<string> more = GetAllDirectories(root + directory + "*", storeFile);
                // For each subdirectory found, add in the base path.
                for (int j = 0; j < more.Count; j++)
                {
                    more[j] = directory + more[j];
                }
                // Insert the subdirectories into the list and
                // update the counter and upper bound.
                directoryList.InsertRange(i + 1, more);
                i += more.Count;
                max += more.Count;
            }
            return directoryList;
        } // End of GetAllDirectories.
          //-----------------------------------------------------------------------------------
        private static List<string> GetAllFiles(string pattern, IsolatedStorageFile storeFile)
        {
            // Get the root and file portions of the search string.
            string fileString = Path.GetFileName(pattern);
            List<string> fileList = new(storeFile.GetFileNames(pattern));
            // Loop through the subdirectories, collect matches,
            // and make separators consistent.
            foreach (string directory in GetAllDirectories("*", storeFile))
            {
                foreach (string file in storeFile.GetFileNames(directory + "/" + fileString))
                {
                    fileList.Add((directory + "/" + file));
                }
            }
            return fileList;
        } // End of GetAllFiles.
    }
}
