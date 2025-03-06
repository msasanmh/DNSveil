using System.Diagnostics;
using System.ServiceProcess;

namespace MsmhToolsClass;

public static class ServiceTool
{
    public static List<string> GetAllServiceNames()
    {
        List<string> serviceList = new();
        if (OperatingSystem.IsWindows())
        {
            // Get All Services
            try
            {
                ServiceController[] services = ServiceController.GetServices();
                for (int n = 0; n < services.Length; n++)
                {
                    using ServiceController service = services[n];
                    serviceList.Add(service.ServiceName);
                }
            }
            catch (Exception) { }

            // Get All Driver Services
            try
            {
                ServiceController[] driverServices = ServiceController.GetDevices();
                for (int n = 0; n < driverServices.Length; n++)
                {
                    using ServiceController driverService = driverServices[n];
                    serviceList.Add(driverService.ServiceName);
                }
            }
            catch (Exception) { }

            // Sort
            try
            {
                serviceList.Sort();
            }
            catch (Exception) { }
        }
        return serviceList;
    }

    public static List<string> GetServiceNamesWhere(string contains)
    {
        List<string> serviceList = new();
        try
        {
            List<string> allServices = GetAllServiceNames();
            for (int n = 0; n < allServices.Count; n++)
            {
                string serviceName = allServices[n];
                if (serviceName.Contains(contains, StringComparison.OrdinalIgnoreCase)) serviceList.Add(serviceName);
            }
        }
        catch (Exception) { }
        return serviceList;
    }

    public static ServiceController? GetServiceByName(string serviceName)
    {
        ServiceController? service = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(serviceName) && OperatingSystem.IsWindows())
                service = new ServiceController(serviceName);
        }
        catch (Exception) { }
        return service;
    }

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

    public static async Task<string> ChangeStatusAsync(string serviceName, ServiceControllerStatus? status, int timeoutSec = 10)
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
                    var p = await ProcessManager.ExecuteAsync("sc", null, args, true, true);
                    if (p.IsSeccess)
                    {
                        stdout = p.Output;

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
                        try { await wait.WaitAsync(TimeSpan.FromSeconds(timeoutSec)); } catch (Exception) { }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ServiceTool ChangeStatusAsync: " + ex.Message);
        }
        return stdout;
    }

    public static async Task<string> ChangeStartModeAsync(string serviceName, ServiceStartMode? startMode, int timeoutSec = 10)
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
                    var p = await ProcessManager.ExecuteAsync("sc", null, args, true, true);
                    if (p.IsSeccess)
                    {
                        stdout = p.Output;

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
                        try { await wait.WaitAsync(TimeSpan.FromSeconds(timeoutSec)); } catch (Exception) { }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ServiceTool ChangeStartModeAsync: " + ex.Message);
        }
        return stdout;
    }

    public static async Task<string> DeleteAsync(string serviceName, int timeoutSec = 10)
    {
        string stdout = string.Empty;
        try
        {
            if (!string.IsNullOrWhiteSpace(serviceName) && OperatingSystem.IsWindows())
            {
                // Stop Service
                stdout = await ChangeStatusAsync(serviceName, ServiceControllerStatus.Stopped, timeoutSec);

                // Delete Service
                string args = $"delete {serviceName}";
                var p = await ProcessManager.ExecuteAsync("sc", null, args, true, true);
                if (p.IsSeccess)
                {
                    stdout += Environment.NewLine;
                    stdout += p.Output;

                    // Wait
                    Task wait = Task.Run(async () =>
                    {
                        while (true)
                        {
                            GetStatus(serviceName, out ServiceControllerStatus? currentStatus, out _);
                            if (currentStatus == null) break;
                            await Task.Delay(50);
                        }
                    });
                    try { await wait.WaitAsync(TimeSpan.FromSeconds(timeoutSec)); } catch (Exception) { }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ServiceTool DeleteAsync: " + ex.Message);
        }
        return stdout.RemoveEmptyLines();
    }

    public static async Task<string> DeleteWhereAsync(string contains, int timeoutSec = 10)
    {
        string stdout = string.Empty;
        try
        {
            List<string> services = GetServiceNamesWhere(contains);
            for (int n = 0; n < services.Count; n++)
            {
                string serviceName = services[n];
                stdout += await DeleteAsync(serviceName, timeoutSec);
                stdout += Environment.NewLine;
            }
        }
        catch (Exception) { }
        return stdout.RemoveEmptyLines();
    }

}