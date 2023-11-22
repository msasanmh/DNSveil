using System.Diagnostics;
using System.DirectoryServices.AccountManagement;

namespace MsmhToolsClass;

public class OsTool
{
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
                return TimeSpan.MaxValue;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LastRebootTime: {ex.Message}");
                return TimeSpan.MaxValue;
            }
        });
    }

    public static bool IsWin7()
    {
        bool result = false;
        OperatingSystem os = Environment.OSVersion;
        Version vs = os.Version;

        if (os.Platform == PlatformID.Win32NT)
        {
            if (vs.Minor == 1 && vs.Major == 6)
                result = true;
        }

        return result;
    }
}
