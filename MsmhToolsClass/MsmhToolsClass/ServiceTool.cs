using System.Diagnostics;
using System.ServiceProcess;
using static MsmhToolsClass.MsmhAgnosticServer.Socks;

namespace MsmhToolsClass;

public static class ServiceTool
{
    public static void GetStatus(string serviceName, out ServiceControllerStatus? status, out ServiceStartMode? startMode)
    {
        status = null; startMode = null;

        try
        {
            if (!OperatingSystem.IsWindows()) return;
            using ServiceController sc = new(serviceName);
            status = sc.Status;
            startMode = sc.StartType;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ServiceTool GetStatus: " + ex.Message);
        }
    }

    public static async Task<string> ChangeStatusAsync(string serviceName, ServiceControllerStatus? status)
    {
        string stdout = string.Empty;

        try
        {
            if (!string.IsNullOrWhiteSpace(serviceName) && status != null && OperatingSystem.IsWindows())
            {
                string stat = string.Empty;
                if (status == ServiceControllerStatus.Running || status == ServiceControllerStatus.StartPending) stat = "start";
                else if (status == ServiceControllerStatus.Paused || status == ServiceControllerStatus.PausePending) stat = "pause";
                else if (status == ServiceControllerStatus.ContinuePending) stat = "continue";
                else if (status == ServiceControllerStatus.Stopped || status == ServiceControllerStatus.StopPending) stat = "stop";

                if (!string.IsNullOrEmpty(stat))
                {
                    string args = $"{stat} {serviceName}";
                    stdout = await ProcessManager.ExecuteAsync("sc", null, args, true, true);

                    // Wait
                    Task wait = Task.Run(async () =>
                    {
                        while (true)
                        {
                            GetStatus(serviceName, out ServiceControllerStatus? currentStatus, out _);
                            if (currentStatus == null) break;
                            if (currentStatus == status) break;
                            await Task.Delay(50);
                        }
                    });
                    try { await wait.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ServiceTool ChangeStatusAsync: " + ex.Message);
        }

        return stdout;
    }

    public static async Task<string> ChangeStartModeAsync(string serviceName, ServiceStartMode? startMode)
    {
        string stdout = string.Empty;

        try
        {
            if (!string.IsNullOrWhiteSpace(serviceName) && startMode != null && OperatingSystem.IsWindows())
            {
                string stat = string.Empty;
                if (startMode == ServiceStartMode.Automatic) stat = "auto";
                else if (startMode == ServiceStartMode.Boot) stat = "boot";
                else if (startMode == ServiceStartMode.Disabled) stat = "disabled";
                else if (startMode == ServiceStartMode.Manual) stat = "demand";
                else if (startMode == ServiceStartMode.System) stat = "system";

                if (!string.IsNullOrEmpty(stat))
                {
                    // An "space" is required between the equal sign and the value
                    string args = $"config {serviceName} start= {stat}";
                    stdout = await ProcessManager.ExecuteAsync("sc", null, args, true, true);

                    // Wait
                    Task wait = Task.Run(async () =>
                    {
                        while (true)
                        {
                            GetStatus(serviceName, out _, out ServiceStartMode? currentStartMode);
                            if (currentStartMode == null) break;
                            if (currentStartMode == startMode) break;
                            await Task.Delay(50);
                        }
                    });
                    try { await wait.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ServiceTool ChangeStartModeAsync: " + ex.Message);
        }

        return stdout;
    }
}