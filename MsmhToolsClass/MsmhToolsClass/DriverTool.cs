using System.Diagnostics;

namespace MsmhToolsClass;

public static class DriverTool
{
    public static async Task<string> DeleteAsync(string driverName, int timeoutSec = 15)
    {
        string stdout = string.Empty;
        if (!string.IsNullOrWhiteSpace(driverName) && OperatingSystem.IsWindows())
        {
            string args = $"sysdriver where name=\"{driverName}\" delete /nointeractive";
            var p = await ProcessManager.ExecuteAsync("wmic", null, args, true, true, null, ProcessPriorityClass.Normal, timeoutSec);
            if (p.IsSeccess) stdout = p.Output;
        }
        return stdout;
    }

    public static async Task<string> DeleteWhereAsync(string contains, int timeoutSec = 15)
    {
        string stdout = string.Empty;
        if (!string.IsNullOrWhiteSpace(contains) && OperatingSystem.IsWindows())
        {
            string args = $"sysdriver where \"name like '%{contains}%'\" delete /nointeractive";
            var p = await ProcessManager.ExecuteAsync("wmic", null, args, true, true, null, ProcessPriorityClass.Normal, timeoutSec);
            if (p.IsSeccess) stdout = p.Output;
        }
        return stdout;
    }

}