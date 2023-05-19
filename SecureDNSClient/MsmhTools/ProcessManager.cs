using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Management;
using System.Security;

namespace MsmhTools
{
    public static class ProcessManager
    {
        //-----------------------------------------------------------------------------------
        /// <summary>
        /// Returns stdout or errour after process finished.
        /// </summary>
        public static string Execute(string processName, string? args = null, bool hideWindow = true, bool runAsAdmin = false, string? workingDirectory = null, ProcessPriorityClass processPriorityClass = ProcessPriorityClass.Normal)
        {
            // Create process
            Process process = new();
            process.StartInfo.FileName = processName;
            
            if (args != null)
                process.StartInfo.Arguments = args;

            if (hideWindow)
            {
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }
            else
            {
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            }

            if (runAsAdmin)
            {
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.Verb = "runas";
            }
            else
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.Verb = "";
            }

            // Set output of program to be written to process output stream
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            if (workingDirectory != null)
                process.StartInfo.WorkingDirectory = workingDirectory;

            try
            {
                process.Start();

                // Set process priority
                process.PriorityClass = processPriorityClass;

                string stdout = process.StandardOutput.ReadToEnd();
                string errout = process.StandardError.ReadToEnd();
                //string output = stdout + Environment.NewLine + errout;

                // Wait for process to finish
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    return stdout;
                }
                else
                {
                    return errout;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return string.Empty;
            }
        }
        //-----------------------------------------------------------------------------------
        /// <summary>
        /// Execute and returns PID, if faild returns -1
        /// </summary>
        public static int ExecuteOnly(string processName, string? args = null, bool hideWindow = true, bool runAsAdmin = false, string? workingDirectory = null, ProcessPriorityClass processPriorityClass = ProcessPriorityClass.Normal)
        {
            int pid;
            // Create process
            Process process = new();
            process.StartInfo.FileName = processName;

            if (args != null)
                process.StartInfo.Arguments = args;

            if (hideWindow)
            {
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }
            else
            {
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            }

            if (runAsAdmin)
            {
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.Verb = "runas";
            }
            else
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.Verb = "";
            }

            if (workingDirectory != null)
                process.StartInfo.WorkingDirectory = workingDirectory;

            try
            {
                process.Start();
                
                // Set process priority
                process.PriorityClass = processPriorityClass;
                pid = process.Id;
            }
            catch (Exception ex)
            {
                pid = -1;
                Debug.WriteLine($"ExecuteOnly: {ex.Message}");
            }
            return pid;
        }
        //-----------------------------------------------------------------------------------
        public static bool FindProcessByName(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }
        //-----------------------------------------------------------------------------------
        public static bool FindProcessByID(int pid)
        {
            bool result = false;
            Process[] processes = Process.GetProcesses();
            for (int n = 0; n < processes.Length; n++)
            {
                if (processes[n].Id == pid)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
        //-----------------------------------------------------------------------------------
        public static void KillProcessByName(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            for (int n = 0; n < processes.Length; n++)
                processes[n].Kill();
        }
        //-----------------------------------------------------------------------------------
        public static void KillProcessByID(int pid)
        {
            try
            {
                Process process = Process.GetProcessById(pid);
                process.Kill();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        //-----------------------------------------------------------------------------------
        /// <summary>
        /// Returns first PID, if faild returns -1
        /// </summary>
        public static int GetFirstPIDByName(string processName)
        {
            int pid = -1;
            Process[] processes = Process.GetProcessesByName(processName);
            for (int n = processes.Length - 1; n >= 0; n--)
                pid = processes[n].Id;
            return pid;
        }
        //-----------------------------------------------------------------------------------
        /// <summary>
        /// Returns process PID, if faild returns -1
        /// </summary>
        public static int GetProcessPidByListeningPort(int port)
        {
            string netstatArgs = "-a -n -o";
            string? stdout = ProcessManager.Execute("netstat", netstatArgs);
            if (!string.IsNullOrWhiteSpace(stdout))
            {
                List<string> lines = stdout.SplitToLines();
                for (int n = 0; n < lines.Count; n++)
                {
                    string line = lines[n];
                    if (!string.IsNullOrWhiteSpace(line) && line.Contains("LISTENING") && line.Contains($":{port} "))
                    {
                        string[] split1 = line.Split("LISTENING", StringSplitOptions.TrimEntries);
                        bool isBool = int.TryParse(split1[1], out int pid);
                        if (isBool)
                        {
                            return pid;
                        }
                    }
                }
            }
            return -1;
        }
        //-----------------------------------------------------------------------------------
        /// <summary>
        /// Returns process name, if failed returns string.empty
        /// </summary>
        public static string GetProcessNameByListeningPort(int port)
        {
            int pid = GetProcessPidByListeningPort(port);
            if (pid != -1)
            {
                try
                {
                    return Process.GetProcessById(pid).ProcessName;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Get Process Name By Listening Port:");
                    Debug.WriteLine(ex.Message);
                }
            }
            return string.Empty;
        }
        //-----------------------------------------------------------------------------------
        public static string GetArguments(this Process process)
        {
            using ManagementObjectSearcher searcher = new("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id);
            using ManagementObjectCollection objects = searcher.Get();
            return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString() ?? string.Empty;

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

        public static void ThrottleProcess(int processId, double limitPercent)
        {
            var process = Process.GetProcessById(processId);
            var processName = process.ProcessName;
            var p = new PerformanceCounter("Process", "% Processor Time", processName);
            Task.Run(() =>
            {
                while (true)
                {
                    var interval = 100;
                    Thread.Sleep(interval);
                    var currentUsage = p.NextValue() / Environment.ProcessorCount;
                    Debug.WriteLine(currentUsage);
                    if (currentUsage < limitPercent) continue;
                    var suspensionTime = (currentUsage - limitPercent) / currentUsage * interval;
                    SuspendProcess(processId);
                    Thread.Sleep((int)suspensionTime);
                    ResumeProcess(processId);
                }
            });
        }
        public static void SuspendProcess(int pId)
        {
            var process = Process.GetProcessById(pId);
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
            var process = Process.GetProcessById(pId);
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
}
