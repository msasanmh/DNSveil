using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Management;
using Process = System.Diagnostics.Process;
using System.Text.RegularExpressions;

namespace MsmhToolsClass;

public static class ProcessManager
{
    //-----------------------------------------------------------------------------------

    /// <summary>
    /// Get CPU Usage By Process
    /// </summary>
    /// <param name="process">Process</param>
    /// <param name="delay">Delay to calculate usage (ms)</param>
    /// <returns>Returns -1 if fail</returns>
    public static async Task<float> GetCpuUsageAsync(Process process, int delay)
    {
        string processName = process.ProcessName;
        return await GetCpuUsageAsync(processName, delay);
    }

    /// <summary>
    /// Get CPU Usage By Process ID
    /// </summary>
    /// <param name="pid">PID</param>
    /// <param name="delay">Delay to calculate usage (ms)</param>
    /// <returns>Returns -1 if fail</returns>
    public static async Task<float> GetCpuUsageAsync(int pid, int delay)
    {
        if (pid < 0) return -1;
        string processName = GetProcessNameByPID(pid);
        if (!string.IsNullOrEmpty(processName))
            return await GetCpuUsageAsync(processName, delay);
        return -1;
    }

    /// <summary>
    /// Get CPU Usage By Process Name (Windows Only)
    /// </summary>
    /// <param name="processName">Process Name</param>
    /// <param name="delay">Delay to calculate usage (ms)</param>
    /// <returns>Returns -1 if fail</returns>
    public static async Task<float> GetCpuUsageAsync(string processName, int delay)
    {
        // To Get CPU Total Usage:
        // new PerformanceCounter("Processor", "% Processor Time", "_Total");
        float result = -1;
        if (!OperatingSystem.IsWindows()) return result;
        if (typeof(PerformanceCounter) == null) return result;

        await Task.Run(async () =>
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    using PerformanceCounter performanceCounter = new("Process", "% Processor Time", processName, true);
                    CounterSample first = performanceCounter.NextSample();
                    await Task.Delay(delay); // Needs time to calculate
                    CounterSample second = performanceCounter.NextSample();
                    float final = CounterSample.Calculate(first, second);
                    result = final / Environment.ProcessorCount;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ProcessManager GetCpuUsageAsync: {ex.Message}");
            }
        });

        return result;
    }
    //-----------------------------------------------------------------------------------

    /// <summary>
    /// Windows Only
    /// </summary>
    public static void GetPerformanceCounterCategories()
    {
        if (!OperatingSystem.IsWindows()) return;
        if (typeof(PerformanceCounterCategory) == null) return;
        PerformanceCounterCategory[] categories = PerformanceCounterCategory.GetCategories();
        for (int a = 0; a < categories.Length; a++)
        {
            PerformanceCounterCategory category = categories[a];

            Debug.WriteLine(category.CategoryName);

            string[] instanceNames = category.GetInstanceNames();
            for (int b = 0; b < instanceNames.Length; b++)
            {
                string instanceName = instanceNames[b];
                Debug.WriteLine("    " + instanceName);

                if (category.InstanceExists(instanceName))
                    foreach (var counter in category.GetCounters(instanceName))
                    {
                        Debug.WriteLine("        " + counter.CounterName);
                    }
            }
        }
    }
    //-----------------------------------------------------------------------------------

    /// <summary>
    /// Send Command to a Process and Get Result
    /// </summary>
    /// <param name="process">Process</param>
    /// <param name="command">Commands</param>
    /// <returns>Returns True if success</returns>
    public static async Task<bool> SendCommandAsync(Process process, string command)
    {
        try
        {
            await process.StandardInput.WriteLineAsync(command);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    //-----------------------------------------------------------------------------------

    /// <summary>
    /// Send Command to a Process and Get Result
    /// </summary>
    /// <param name="process">Process</param>
    /// <param name="command">Commands</param>
    /// <returns>Returns True if success</returns>
    public static bool SendCommand(Process process, string command)
    {
        try
        {
            process.StandardInput.WriteLine(command);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    //-----------------------------------------------------------------------------------

    /// <summary>
    /// Returns stdout or Stderr after process finished.
    /// </summary>
    public static async Task<(bool IsSeccess, string Output)> ExecuteAsync(string processName, Dictionary<string, string>? environmentVariables = null, string? args = null, bool hideWindow = true, bool runAsAdmin = false, string? workingDirectory = null, ProcessPriorityClass processPriorityClass = ProcessPriorityClass.Normal, int timeoutSec = 30)
    {
        bool isSuccess = false;
        string output = string.Empty;
        Process? process0 = null;

        Task task = Task.Run(async () =>
        {
            try
            {
                // Create Process
                process0 = new();
                process0.StartInfo.FileName = processName;

                if (environmentVariables != null)
                {
                    try
                    {
                        foreach (KeyValuePair<string, string> kvp in environmentVariables)
                            process0.StartInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
                    }
                    catch (Exception) { }
                }

                if (args != null)
                    process0.StartInfo.Arguments = args;

                if (hideWindow)
                {
                    process0.StartInfo.CreateNoWindow = true;
                    process0.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }
                else
                {
                    process0.StartInfo.CreateNoWindow = false;
                    process0.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                }

                if (runAsAdmin)
                {
                    process0.StartInfo.Verb = "runas";
                }
                else
                {
                    process0.StartInfo.Verb = "";
                }

                // Redirect input output to get ability of reading process output
                process0.StartInfo.UseShellExecute = false;
                process0.StartInfo.RedirectStandardInput = false; // We're not sending
                process0.StartInfo.RedirectStandardOutput = true;
                process0.StartInfo.RedirectStandardError = true;

                if (workingDirectory != null)
                    process0.StartInfo.WorkingDirectory = workingDirectory;

                process0.Start();

                // Set process priority
                process0.PriorityClass = processPriorityClass;

                string stdout = await process0.StandardOutput.ReadToEndAsync();
                stdout = stdout.ReplaceLineEndings(Environment.NewLine);
                string errout = await process0.StandardError.ReadToEndAsync();
                errout = errout.ReplaceLineEndings(Environment.NewLine);
                output = stdout + Environment.NewLine + errout;

                // Wait For Process To Finish
                await process0.WaitForExitAsync();
                isSuccess = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ProcessManager ExecuteAsync 1: " + ex.Message);
                output = ex.Message;
            }
            finally
            {
                process0?.Dispose();
            }
        });

        try
        {
            await task.WaitAsync(TimeSpan.FromSeconds(timeoutSec));
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProcessManager ExecuteAsync 2: " + ex.Message);
            output = ex.Message;
        }
        finally
        {
            process0?.Dispose();
        }

        return (isSuccess, output);
    }
    //-----------------------------------------------------------------------------------

    /// <summary>
    /// Execute and returns PID, if fails return -1
    /// </summary>
    public static int ExecuteOnly(string processName, Dictionary<string, string>? environmentVariables = null, string? args = null, bool hideWindow = true, bool runAsAdmin = false, string? workingDirectory = null, ProcessPriorityClass processPriorityClass = ProcessPriorityClass.Normal)
    {
        int pid;
        
        try
        {
            // Create process
            Process process0 = new();
            process0.StartInfo.FileName = processName;

            if (environmentVariables != null)
            {
                foreach (KeyValuePair<string, string> kvp in environmentVariables)
                    process0.StartInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
            }

            if (args != null)
                process0.StartInfo.Arguments = args;

            if (hideWindow)
            {
                process0.StartInfo.CreateNoWindow = true;
                process0.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }
            else
            {
                process0.StartInfo.CreateNoWindow = false;
                process0.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            }

            if (runAsAdmin)
            {
                process0.StartInfo.Verb = "runas";
            }
            else
            {
                process0.StartInfo.Verb = "";
            }

            // Redirect input output to get ability of sending and reading process output
            process0.StartInfo.UseShellExecute = false;
            process0.StartInfo.RedirectStandardInput = true;
            process0.StartInfo.RedirectStandardOutput = true;
            process0.StartInfo.RedirectStandardError = true;

            if (workingDirectory != null)
                process0.StartInfo.WorkingDirectory = workingDirectory;

            process0.Start();

            // Set process priority
            process0.PriorityClass = processPriorityClass;
            pid = process0.Id;

            // Dispose
            process0.Dispose();
        }
        catch (Exception ex)
        {
            pid = -1;
            Debug.WriteLine($"ProcessManager ExecuteOnly: {ex.Message}");
        }

        return pid;
    }
    //-----------------------------------------------------------------------------------

    public static bool FindProcessByName(string processName)
    {
        int result = 0;

        try
        {
            Process[] processes = Process.GetProcessesByName(processName);
            result = processes.Length;
            try
            {
                for (int n = 0; n < processes.Length; n++)
                    processes[n].Dispose();
            }
            catch (Exception) { }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProcessManager FindProcessByName: " + ex.Message);
        }

        return result > 0;
    }
    //-----------------------------------------------------------------------------------

    public static bool FindProcessByPID(int pid)
    {
        bool result = false;

        try
        {
            Process[] processes = Process.GetProcesses();
            for (int n = 0; n < processes.Length; n++)
            {
                if (processes[n].Id == pid)
                {
                    result = true;
                    break;
                }
                processes[n].Dispose();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProcessManager FindProcessByPID: " + ex.Message);
        }

        return result;
    }
    //-----------------------------------------------------------------------------------

    public static async Task KillProcessByPidAsync(int pid, bool killEntireProcessTree = false)
    {
        try
        {
            if (FindProcessByPID(pid))
            {
                try
                {
                    using Process process = Process.GetProcessById(pid);
                    process.Kill(killEntireProcessTree);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("ProcessManager KillProcessByPidAsync 1: " + e.Message);
                }

                if (FindProcessByPID(pid))
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        string command = "taskkill";
                        string taskKillArgs = $"/F /PID {pid}";
                        if (killEntireProcessTree) taskKillArgs += " /T";
                        await ExecuteAsync(command, null, taskKillArgs, true, true);

                        if (FindProcessByPID(pid))
                        {
                            string wmicArgs = $"process where processid=\"{pid}\" delete";
                            await ExecuteAsync("wmic", null, wmicArgs, true, true);
                        }
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        string command = "kill";
                        string killArgs = $"-9 {pid}";
                        await ExecuteAsync(command, null, killArgs, true, true);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProcessManager KillProcessByPidAsync 2: " + ex.Message);
        }
    }
    //-----------------------------------------------------------------------------------

    public static async Task KillProcessByNameAsync(string processName, bool killEntireProcessTree = false)
    {
        try
        {
            Process[] processes = Process.GetProcessesByName(processName);
            for (int n = 0; n < processes.Length; n++)
                await KillProcessByPidAsync(processes[n].Id, killEntireProcessTree);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProcessManager KillProcessByNameAsync: " + ex.Message);
        }
    }
    //-----------------------------------------------------------------------------------

    /// <summary>
    /// Returns first PID, if faild returns -1
    /// </summary>
    public static int GetFirstPidByName(string processName)
    {
        int pid = -1;

        try
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Any()) pid = processes[0].Id;

            for (int n = 0; n < processes.Length; n++)
                processes[n].Dispose();
        }
        catch (Exception) { }

        return pid;
    }
    //-----------------------------------------------------------------------------------

    /// <summary>
    /// Returns A List Of PIDs
    /// </summary>
    public static async Task<List<int>> GetProcessPidsByUsingPortAsync(int port, bool onlyListeningPorts = true)
    {
        List<int> list = new();
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            list = await GetProcessPidsByUsingPort_Windows_Async(port, onlyListeningPorts);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            list = await GetProcessPidsByUsingPort_Linux_Async(port, onlyListeningPorts);
        }

        return list;
    }

    /// <summary>
    /// Returns A List Of PIDs (Windows Only)
    /// </summary>
    private static async Task<List<int>> GetProcessPidsByUsingPort_Windows_Async(int port, bool onlyListeningPorts = true)
    {
        List<int> list = new();
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return list;

        try
        {
            string command = "netstat";
            string netstatArgs = "-a -n -o";
            var p = await ExecuteAsync(command, null, netstatArgs, true, true);
            if (p.IsSeccess)
            {
                string? stdout = p.Output;
                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    List<string> lines = stdout.SplitToLines();
                    for (int n = 0; n < lines.Count; n++)
                    {
                        string line = lines[n].Trim();
                        if (!string.IsNullOrEmpty(line) && line.Contains($":{port} "))
                        {
                            if (onlyListeningPorts && line.StartsWith("TCP") && !line.Contains("LISTENING")) continue;
                            string[] splitLine = line.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                            if (splitLine.Length > 3) // Has 4 Or 5 Columns
                            {
                                string localAddress = splitLine[1];
                                if (localAddress.EndsWith($":{port}"))
                                {
                                    string pidStr = splitLine[^1]; // Last Column Is PID
                                    bool isBool = int.TryParse(pidStr, out int pid);
                                    if (isBool && pid != 0 && !list.IsContain(pid)) list.Add(pid);
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProcessManager GetProcessPidsByUsingPort_Windows_Async: " + ex.Message);
        }

        return list;
    }

    /// <summary>
    /// Returns A List Of PIDs (Linux/WSL Only)
    /// </summary>
    private static async Task<List<int>> GetProcessPidsByUsingPort_Linux_Async(int port, bool onlyListeningPorts = true)
    {
        List<int> list = new();
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return list;

        try
        {
            string command = "ss";
            // string args = $"/bin/bash -c \"ss -tulpn | grep :{port}\"";
            string args = "-tulpn";
            var p = await ExecuteAsync(command, null, args, true, true);
            if (p.IsSeccess)
            {
                string? stdout = p.Output;
                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    List<string> lines = stdout.SplitToLines();
                    for (int n = 0; n < lines.Count; n++)
                    {
                        string line = lines[n].Trim();
                        if (!string.IsNullOrEmpty(line) && line.Contains($":{port} "))
                        {
                            if (onlyListeningPorts && line.StartsWith("tcp") && !line.Contains("LISTEN")) continue;
                            string[] splitLine = line.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                            if (splitLine.Length > 5) // Has 6 Or 7 Columns
                            {
                                string localAddress = splitLine[4];
                                if (localAddress.EndsWith($":{port}"))
                                {
                                    string users = splitLine[^1]; // Last Column Has PID In It
                                    Match pidMatch = Regex.Match(users, @"pid=(\d+)");
                                    if (pidMatch.Success)
                                    {
                                        string pidStr = pidMatch.Groups[1].Value;
                                        bool isBool = int.TryParse(pidStr, out int pid);
                                        if (isBool && pid != 0 && !list.IsContain(pid)) list.Add(pid);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProcessManager GetProcessPidsByUsingPort_Linux_Async: " + ex.Message);
        }

        return list;
    }
    //-----------------------------------------------------------------------------------

    /// <summary>
    /// Get Process By PID
    /// </summary>
    /// <param name="pid">PID</param>
    /// <returns>Returns null if not exist.</returns>
    public static Process? GetProcessByPID(int pid)
    {
        try
        {
            Process[] processes = Process.GetProcesses();
            for (int n = 0; n < processes.Length; n++)
            {
                Process process = processes[n];
                if (process.Id == pid) return process;
                else process.Dispose();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProcessManager GetProcessByPID: " + ex.Message);
        }
        return null;
    }
    //-----------------------------------------------------------------------------------

    /// <summary>
    /// Get Process Name By PID
    /// </summary>
    /// <param name="pid">PID</param>
    /// <returns>Returns string.Empty if not exist.</returns>
    public static string GetProcessNameByPID(int pid)
    {
        string result = string.Empty;
        if (pid < 0) return result;

        try
        {
            Process[] processes = Process.GetProcesses();
            for (int n = 0; n < processes.Length; n++)
            {
                Process process = processes[n];
                if (process.Id == pid)
                {
                    result = process.ProcessName;
                }
                process.Dispose();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProcessManager GetProcessNameByPID: " + ex.Message);
        }

        return result;
    }
    //-----------------------------------------------------------------------------------

    public static int GetParentPID(int pid)
    {
        int parentPid = -1;
        if (!OperatingSystem.IsWindows()) return parentPid;

        try
        {
            using ManagementObject mo = new($"win32_process.handle='{pid}'");
            mo.Get();
            parentPid = Convert.ToInt32(mo["ParentProcessId"]);
        }
        catch (Exception) { }

        return parentPid;
    }
    //-----------------------------------------------------------------------------------

    private static List<int> GetAllChildProcesses(int parentPID)
    {
        List<int> pids = new();
        if (!OperatingSystem.IsWindows()) return pids;

        try
        {
            using ManagementObjectSearcher searcher = new(
                "SELECT * " +
                "FROM Win32_Process " +
                "WHERE ParentProcessId=" + parentPID);
            using ManagementObjectCollection collection = searcher.Get();

            if (collection.Count > 0)
            {
                foreach (ManagementBaseObject item in collection)
                {
                    int childPID = Convert.ToInt32(item["ProcessId"]);
                    if (childPID != Environment.ProcessId && childPID != parentPID)
                    {
                        if (!pids.IsContain(childPID)) pids.Add(childPID);

                        List<int> grandChildPIDs = GetAllChildProcesses(childPID);
                        for (int n = 0; n < grandChildPIDs.Count; n++)
                        {
                            int grandChildPID = grandChildPIDs[n];
                            if (grandChildPID != Environment.ProcessId && grandChildPID != parentPID && grandChildPID != childPID)
                                if (!pids.IsContain(grandChildPID)) pids.Add(grandChildPID);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProcessManager GetAllChildProcesses: " + ex.Message);
        }

        return pids;
    }
    //-----------------------------------------------------------------------------------

    /// <summary>
    /// Windows Only
    /// </summary>
    public static string GetArguments(this Process process)
    {
        string result = string.Empty;

        try
        {
            if (!OperatingSystem.IsWindows()) return result;
            if (typeof(ManagementObjectSearcher) == null) return result;
            if (typeof(ManagementObjectCollection) == null) return result;
            if (typeof(ManagementBaseObject) == null) return result;

            using ManagementObjectSearcher searcher = new("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id);
            using ManagementObjectCollection objects = searcher.Get();
            result = objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProcessManager GetArguments: " + ex.Message);
        }

        return result;
    }
    //-----------------------------------------------------------------------------------

    public static void SetProcessPriority(ProcessPriorityClass processPriorityClass)
    {
        Process.GetCurrentProcess().PriorityClass = processPriorityClass;
    }
    //-----------------------------------------------------------------------------------

    [Flags]
    private enum ThreadAccess : int
    {
        TERMINATE = (0x0001),
        SUSPEND_RESUME = (0x0002),
        GET_CONTEXT = (0x0008),
        SET_CONTEXT = (0x0010),
        SET_INFORMATION = (0x0020),
        QUERY_INFORMATION = (0x0040),
        SET_THREAD_TOKEN = (0x0080),
        IMPERSONATE = (0x0100),
        DIRECT_IMPERSONATION = (0x0200)
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

    [DllImport("kernel32.dll")]
    private static extern uint SuspendThread(IntPtr hThread);

    [DllImport("kernel32.dll")]
    private static extern int ResumeThread(IntPtr hThread);

    [DllImport("kernel32.dll")]
    private static extern int CloseHandle(IntPtr hThread);

    /// <summary>
    /// Windows Only
    /// </summary>
    public static void ThrottleProcess(int processId, double limitPercent)
    {
        if (!OperatingSystem.IsWindows()) return;
        if (typeof(ManagementObjectSearcher) == null) return;

        using Process process = Process.GetProcessById(processId);
        string processName = process.ProcessName;
        PerformanceCounter p = new("Process", "% Processor Time", processName);

        Task.Run(async () =>
        {
            while (true)
            {
                if (!OperatingSystem.IsWindows()) break;
                int interval = 1000;
                p.NextValue();
                await Task.Delay(interval);
                float currentUsage = p.NextValue() / Environment.ProcessorCount;
                Debug.WriteLine(currentUsage);
                if (currentUsage < limitPercent) continue;
                SuspendProcess(processId);
                await Task.Delay(interval);
                ResumeProcess(processId);
            }
        });
    }
    public static void SuspendProcess(int pId)
    {
        using Process process = Process.GetProcessById(pId);
        SuspendProcess(process);
    }
    public static void SuspendProcess(Process process)
    {
        foreach (ProcessThread thread in process.Threads)
        {
            var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
            if (pOpenThread == IntPtr.Zero)
            {
                break;
            }
            _ = SuspendThread(pOpenThread);
        }
    }
    public static void ResumeProcess(int pId)
    {
        using Process process = Process.GetProcessById(pId);
        ResumeProcess(process);
    }
    public static void ResumeProcess(Process process)
    {
        foreach (ProcessThread thread in process.Threads)
        {
            var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
            if (pOpenThread == IntPtr.Zero)
            {
                break;
            }
            _ = ResumeThread(pOpenThread);
        }
    }
    //-----------------------------------------------------------------------------------

}