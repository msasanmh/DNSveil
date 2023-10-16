using System;
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
        Assembly assembly = Assembly.GetExecutingAssembly();
        GuidAttribute attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
        return attribute.Value;
    }

    public static string GetUniqueIdString(bool getEncodedId)
    {
        string idPrincipal = Environment.MachineName + Environment.UserName;
        string idDateTime = DateTime.UtcNow.ToString("yyyyMMddHHmmssfffffff", CultureInfo.InvariantCulture);
        string idInt1 = $"{Guid.NewGuid().GetHashCode()}";
        if (idInt1.StartsWith('-')) idInt1 = idInt1.Replace("-", string.Empty);
        string idInt2 = $"{BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0)}";
        if (idInt2.StartsWith('-')) idInt2 = idInt2.Replace("-", string.Empty);
        string result = idPrincipal + idDateTime + idInt1 + idInt2;
        return getEncodedId ? EncodingTool.GetSHA512(result).ToLower() : result;
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
        Version versionNew = new(newVersion);
        Version versionOld = new(oldVersion);
        int result = versionNew.CompareTo(versionOld);
        if (result > 0) return 1; // versionNew is greater
        else if (result < 0) return -1; // versionOld is greater
        else return 0; // versions are equal
    }

    public static void SetCulture(CultureInfo cultureInfo)
    {
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    }

    public static bool IsRunningOnWindows
    {
        get
        {
            OSPlatform platform = GetPlatform();
            return platform == OSPlatform.Windows;
        }
    }

    public static bool IsRunningOnLinux
    {
        get
        {
            OSPlatform platform = GetPlatform();
            return platform == OSPlatform.Linux;
        }
    }

    public static bool IsRunningOnMac
    {
        get
        {
            OSPlatform platform = GetPlatform();
            return platform == OSPlatform.OSX;
        }
    }

    private static OSPlatform GetPlatform()
    {
        // Current versions of Mono report MacOSX platform as Unix
        return Environment.OSVersion.Platform == PlatformID.MacOSX || (Environment.OSVersion.Platform == PlatformID.Unix && Directory.Exists("/Applications") && Directory.Exists("/System") && Directory.Exists("/Users"))
             ? OSPlatform.OSX
             : Environment.OSVersion.Platform == PlatformID.Unix
             ? OSPlatform.Linux
             : OSPlatform.Windows;
    }
}