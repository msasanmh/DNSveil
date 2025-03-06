using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Globalization;

namespace MsmhToolsClass;

public static class Info
{
    // System.Reflection.Assembly.Location' always returns an empty string for assemblies
    // embedded in a single-file app. If the path to the app directory is needed,
    // consider calling 'System.AppContext.BaseDirectory'.

    public static readonly string CurrentPath = AppContext.BaseDirectory;
    public static AssemblyName CallingAssemblyName => Assembly.GetCallingAssembly().GetName();
    public static AssemblyName? EntryAssemblyName => Assembly.GetEntryAssembly()?.GetName();
    public static AssemblyName ExecutingAssemblyName => Assembly.GetExecutingAssembly().GetName();
    public static FileVersionInfo InfoCallingAssembly => FileVersionInfo.GetVersionInfo(Assembly.GetCallingAssembly().Location);
    public static FileVersionInfo InfoEntryAssembly => FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()?.Location ?? string.Empty);
    public static FileVersionInfo InfoExecutingAssembly2 => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

    public static FileVersionInfo GetAppInfo(Assembly assembly)
    {
        return FileVersionInfo.GetVersionInfo(assembly.Location);
    }

    public static string GetAppGUID()
    {
        try
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            GuidAttribute attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            return attribute.Value;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Info GetAppGUID: " + ex.Message);
            return string.Empty;
        }
    }

    public static string GetUniqueIdString(bool getEncodedId)
    {
        try
        {
            string idPrincipal = Environment.MachineName + Environment.UserName;
            string idDateTime = DateTime.UtcNow.ToString("yyyyMMddHHmmssfffffff", CultureInfo.InvariantCulture);
            string idInt1 = $"{Guid.NewGuid().GetHashCode()}";
            if (idInt1.StartsWith('-')) idInt1 = idInt1.TrimStart('-');
            string idInt2 = $"{BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0)}";
            if (idInt2.StartsWith('-')) idInt2 = idInt2.TrimStart('-');
            string result = idPrincipal + idDateTime + idInt1 + idInt2;
            return getEncodedId ? EncodingTool.GetSHA512(result).ToLower() : result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Info GetUniqueIdString: " + ex.Message);
            return "0123456789";
        }
    }

    public static int GetUniqueIdInt()
    {
        try
        {
            return Guid.NewGuid().GetHashCode() + BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetUniqueIdInt: " + ex.Message);
            return Guid.NewGuid().GetHashCode();
        }
    }

    /// <returns>
    /// 1 if newVersion &gt; oldVersion.
    /// <br>0 if newVersion = oldVersion.</br>
    /// <br>-1 if newVersion &lt; oldVersion</br>
    /// </returns>
    public static int VersionCompare(string newVersion, string oldVersion)
    {
        try
        {
            Version versionNew = new(newVersion);
            Version versionOld = new(oldVersion);
            int result = versionNew.CompareTo(versionOld);
            if (result > 0) return 1; // versionNew is greater
            else if (result < 0) return -1; // versionOld is greater
            else return 0; // versions are equal
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Info VersionCompare: " + ex.Message);
            return 0;
        }
    }

    public static void SetCulture(CultureInfo cultureInfo)
    {
        try
        {
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Info SetCulture: " + ex.Message);
        }
    }

    public static bool IsRunningOnWindows
    {
        get
        {
            try
            {
                OSPlatform platform = GetPlatform();
                return platform == OSPlatform.Windows;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Info IsRunningOnWindows: " + ex.Message);
                return false;
            }
        }
    }

    public static bool IsRunningOnLinux
    {
        get
        {
            try
            {
                OSPlatform platform = GetPlatform();
                return platform == OSPlatform.Linux;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Info IsRunningOnLinux: " + ex.Message);
                return false;
            }
        }
    }

    public static bool IsRunningOnMac
    {
        get
        {
            try
            {
                OSPlatform platform = GetPlatform();
                return platform == OSPlatform.OSX;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Info IsRunningOnMac: " + ex.Message);
                return false;
            }
        }
    }

    private static OSPlatform GetPlatform()
    {
        try
        {
            // Current Versions Of Mono Report MacOSX Platform As Unix
            return Environment.OSVersion.Platform == PlatformID.MacOSX || (Environment.OSVersion.Platform == PlatformID.Unix && Directory.Exists("/Applications") && Directory.Exists("/System") && Directory.Exists("/Users"))
                 ? OSPlatform.OSX
                 : Environment.OSVersion.Platform == PlatformID.Unix
                 ? OSPlatform.Linux
                 : OSPlatform.Windows;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Info GetPlatform: " + ex.Message);
            return OSPlatform.Windows;
        }
    }
}