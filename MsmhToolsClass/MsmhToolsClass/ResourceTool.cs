using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace MsmhToolsClass;

public class ResourceTool
{
    public static void WriteResourceToFile(string resourcePath, string filePath, Assembly assembly)
    {
        try
        {
            resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourcePath));
            using Stream? stream = assembly.GetManifestResourceStream(resourcePath);
            if (stream != null)
            {
                using FileStream file = new(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                stream.CopyTo(file);
            }
            else
                Debug.WriteLine("WriteResourceToFile: Copy to disk faild, resource was null.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WriteResourceToFile: " + ex.Message);
        }
    }

    public static void WriteResourceToFile(byte[] resource, string filePath)
    {
        try
        {
            File.WriteAllBytes(filePath, resource);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WriteResourceToFile: " + ex.Message);
        }
    }

    public static async Task WriteResourceToFileAsync(string resourcePath, string filePath, Assembly assembly)
    {
        try
        {
            resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourcePath));
            using Stream? stream = assembly.GetManifestResourceStream(resourcePath);
            if (stream != null)
            {
                using FileStream file = new(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                await stream.CopyToAsync(file);
            }
            else
                Debug.WriteLine("WriteResourceToFile: Copy to disk faild, resource was null.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WriteResourceToFileAsync: " + ex.Message);
        }
    }

    public static string GetResourceTextFile(string resourcePath, Assembly assembly)
    {
        try
        {
            if (ResourceExists(resourcePath, assembly))
            {
                // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
                resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourcePath));
                using Stream? stream = assembly.GetManifestResourceStream(resourcePath);
                if (stream != null)
                {
                    using StreamReader reader = new(stream);
                    return reader.ReadToEnd();
                }
                else return string.Empty;
            }
            else return string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetResourceTextFile: " + ex.Message);
            return string.Empty;
        }
    }

    public static string? GetResourceTextFile(byte[] resource)
    {
        string result = string.Empty;
        try
        {
            result = Encoding.UTF8.GetString(resource);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetResourceTextFile: " + ex.Message);
        }
        return result;
    }

    public static async Task<string> GetResourceTextFileAsync(string path, Assembly assembly)
    {
        try
        {
            if (ResourceExists(path, assembly))
            {
                // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
                path = assembly.GetManifestResourceNames().Single(str => str.EndsWith(path));
                using Stream? stream = assembly.GetManifestResourceStream(path);
                if (stream != null)
                {
                    using StreamReader reader = new(stream);
                    return await reader.ReadToEndAsync();
                }
                else return string.Empty;
            }
            else return string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetResourceTextFileAsync: " + ex.Message);
            return string.Empty;
        }
    }

    public static async Task<byte[]> GetResourceBinFileAsync(string path, Assembly assembly)
    {
        try
        {
            if (ResourceExists(path, assembly))
            {
                // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
                path = assembly.GetManifestResourceNames().Single(str => str.EndsWith(path));
                using Stream? stream = assembly.GetManifestResourceStream(path);
                if (stream != null)
                {
                    using MemoryStream ms = new();
                    await stream.CopyToAsync(ms);
                    return ms.ToArray();
                }
                else return Array.Empty<byte>();
            }
            else return Array.Empty<byte>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetResourceBinFileAsync: " + ex.Message);
            return Array.Empty<byte>();
        }
    }
    //-----------------------------------------------------------------------------------
    public static bool ResourceExists(string resourceName, Assembly assembly)
    {
        try
        {
            string[] resourceNames = assembly.GetManifestResourceNames();
            bool exist = resourceNames.Contains(resourceName);
            if (!exist) Debug.WriteLine("ResourceExists: False");
            return exist;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ResourceExists: " + ex.Message);
            return false;
        }
    }
}