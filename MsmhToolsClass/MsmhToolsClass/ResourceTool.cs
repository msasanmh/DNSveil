using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace MsmhToolsClass;

public class ResourceTool
{
    public static void WriteResourceToFile(string resourcePath, string filePath, Assembly assembly)
    {
        resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourcePath));
        using Stream? resource = assembly.GetManifestResourceStream(resourcePath);
        using FileStream file = new(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        if (resource != null)
            resource.CopyTo(file);
        else
            Debug.WriteLine("WriteResourceToFile: Copy to disk faild, resource was null.");
    }

    public static void WriteResourceToFile(byte[] resource, string filePath)
    {
        File.WriteAllBytes(filePath, resource);
    }

    public static async Task WriteResourceToFileAsync(string resourcePath, string filePath, Assembly assembly)
    {
        resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourcePath));
        using Stream? resource = assembly.GetManifestResourceStream(resourcePath);
        using FileStream file = new(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        if (resource != null)
            await resource.CopyToAsync(file);
        else
            Debug.WriteLine("WriteResourceToFile: Copy to disk faild, resource was null.");
    }

    public static string? GetResourceTextFile(string resourcePath, Assembly assembly)
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
            else
                return null;
        }
        else
            return null;
    }

    public static string? GetResourceTextFile(byte[] resource)
    {
        return Encoding.UTF8.GetString(resource);
    }

    public static async Task<string?> GetResourceTextFileAsync(string path, Assembly assembly)
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
            else
                return null;
        }
        else
            return null;
    }
    //-----------------------------------------------------------------------------------
    public static bool ResourceExists(string resourceName, Assembly assembly)
    {
        string[] resourceNames = assembly.GetManifestResourceNames();
        Debug.WriteLine("Resource Exist: " + resourceNames.Contains(resourceName));
        return resourceNames.Contains(resourceName);
    }
}