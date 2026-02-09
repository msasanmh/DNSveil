using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Principal;

namespace MsmhToolsClass;

public class OsTool
{
    public static bool IsAdministrator()
    {
        try
        {
            if (!OperatingSystem.IsWindows()) return false;
            using WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new(currentIdentity);
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("OsTool IsAdministrator: " + ex.Message);
            return false;
        }
    }

    public static bool IsWin7()
    {
        bool result = false;
        OperatingSystem os = Environment.OSVersion;
        Version vs = os.Version;

        if (os.Platform == PlatformID.Win32NT)
        {
            if (vs.Minor == 1 && vs.Major == 6) result = true;
        }

        return result;
    }

    public static bool IsWindows10OrGreater(int build = -1)
    {
        return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
    }

    public static bool IsWindows11OrGreater(int build = 22000)
    {
        return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
    }

    public static bool IsLightTheme()
    {
        if (!OperatingSystem.IsWindows()) return false;
        bool result = false;

        try
        {
            using RegistryKey? registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
            object? value = registry?.GetValue("AppsUseLightTheme");
            result = value is int i && i > 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("OsTool IsLightTheme: " + ex.Message);
        }

        return result;
    }

    /// <summary>
    /// Get Last Reboot Time (Windows Only)
    /// </summary>
    /// <returns>Returns TimeSpan</returns>
    public static async Task<TimeSpan> LastRebootTimeAsync()
    {
        if (!OperatingSystem.IsWindows()) return TimeSpan.MaxValue;
        if (typeof(PerformanceCounter) == null) return TimeSpan.MaxValue;

        return await Task.Run(async () =>
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    using PerformanceCounter performanceCounter = new("System", "System Up Time");
                    performanceCounter.NextValue(); // Returns 0
                    await Task.Delay(1); // Needs time to calculate // 1ms is enough for Up Time
                    return TimeSpan.FromSeconds(performanceCounter.NextValue());
                }
                return TimeSpan.MinValue;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OsTool LastRebootTimeAsync: " + ex.Message);
                return TimeSpan.MinValue;
            }
        });
    }

}
