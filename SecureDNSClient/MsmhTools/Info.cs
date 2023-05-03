using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace MsmhTools
{
    public static class Info
    {
        public static readonly string CurrentPath = AppContext.BaseDirectory;
        public static readonly string CurrentPath2 = Path.GetDirectoryName(Application.ExecutablePath);
        public static readonly string ApplicationName = Path.GetFileName(Application.ExecutablePath);
        public static readonly string ApplicationNameWithoutExtension = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
        public static readonly string ApplicationFullPath = Application.ExecutablePath;
        public static readonly string ApplicationFullPathWithoutExtension = Path.Combine(CurrentPath, ApplicationNameWithoutExtension);
        public static AssemblyName CallingAssemblyName => Assembly.GetCallingAssembly().GetName();
        public static AssemblyName EntryAssemblyName => Assembly.GetEntryAssembly().GetName();
        public static AssemblyName ExecutingAssemblyName => Assembly.GetExecutingAssembly().GetName();
        public static FileVersionInfo InfoCallingAssembly => FileVersionInfo.GetVersionInfo(Assembly.GetCallingAssembly().Location);
        public static FileVersionInfo InfoEntryAssembly => FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
        public static FileVersionInfo InfoExecutingAssembly => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

        public static string GetAppGUID()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            GuidAttribute attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            return attribute.Value;
        }

        /// <returns>
        /// 1 if newVersion &gt; oldVersion.
        /// <br>0 if newVersion = oldVersion.</br>
        /// <br>-1 if newVersion &lt; oldVersion</br>
        /// </returns>
        public static int VersionCompare(string newVersion, string oldVersion)
        {
            var versionNew = new Version(newVersion);
            var versionOld = new Version(oldVersion);
            var result = versionNew.CompareTo(versionOld);
            if (result > 0)
                return 1; // versionNew is greater
            else if (result < 0)
                return -1; // versionOld is greater
            else
                return 0; // versions are equal
        }

        public static bool IsRunningOnWindows
        {
            get
            {
                var platform = GetPlatform();
                if (platform == OSPlatform.Windows)
                    return true;
                else
                    return false;
            }
        }

        public static bool IsRunningOnLinux
        {
            get
            {
                var platform = GetPlatform();
                if (platform == OSPlatform.Linux)
                    return true;
                else
                    return false;
            }
        }

        public static bool IsRunningOnMac
        {
            get
            {
                var platform = GetPlatform();
                if (platform == OSPlatform.OSX)
                    return true;
                else
                    return false;
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
}
