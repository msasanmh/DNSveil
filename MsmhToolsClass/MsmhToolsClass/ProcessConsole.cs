using System.Collections.Concurrent;
using System.Diagnostics;
using Process = System.Diagnostics.Process;
using ThreadState = System.Diagnostics.ThreadState;

namespace MsmhToolsClass;

public class ProcessConsole
{
    private string Stdout { get; set; } = string.Empty;
    private ConcurrentBag<string> StdoutBag { get; set; } = new();
    private string Stderr { get; set; } = string.Empty;
    private ConcurrentBag<string> StderrBag { get; set; } = new();
    private int Pid { get; set; } = -1;
    public Process? Process_ { get; set; }
    public event EventHandler<DataReceivedEventArgs>? StandardDataReceived;
    public event EventHandler<DataReceivedEventArgs>? ErrorDataReceived;

    public ProcessConsole() { }

    public string GetStdout => Stdout;
    public ConcurrentBag<string> GetStdoutBag
    {
        get
        {
            try
            {
                return new ConcurrentBag<string>(StdoutBag);
            }
            catch (Exception)
            {
                return new ConcurrentBag<string>();
            }
        }
    }
    public string GetStderr => Stderr;
    public ConcurrentBag<string> GetStderrBag
    {
        get
        {
            try
            {
                return new ConcurrentBag<string>(StderrBag);
            }
            catch (Exception)
            {
                return new ConcurrentBag<string>();
            }
        }
    }
    public int GetPid => Pid;

    /// <summary>
    /// Execute and returns PID, if faild returns -1
    /// </summary>
    public int Execute(string processName, string? args = null, bool hideWindow = true, bool runAsAdmin = false, string? workingDirectory = null, ProcessPriorityClass processPriorityClass = ProcessPriorityClass.Normal)
    {
        try
        {
            // Clear Bags
            StdoutBag.Clear();
            StderrBag.Clear();

            int pid;
            // Create process
            Process_ = new();
            Process_.StartInfo.FileName = processName;

            if (!string.IsNullOrEmpty(args))
                Process_.StartInfo.Arguments = args;

            if (hideWindow)
            {
                Process_.StartInfo.CreateNoWindow = true;
                Process_.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }
            else
            {
                Process_.StartInfo.CreateNoWindow = false;
                Process_.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            }

            if (runAsAdmin)
            {
                Process_.StartInfo.Verb = "runas";
            }
            else
            {
                Process_.StartInfo.Verb = "";
            }

            // Redirect input output to get ability of sending and reading process output
            Process_.StartInfo.UseShellExecute = false;
            Process_.StartInfo.RedirectStandardInput = true;
            Process_.StartInfo.RedirectStandardOutput = true;
            Process_.StartInfo.RedirectStandardError = true;

            if (!string.IsNullOrEmpty(workingDirectory))
                Process_.StartInfo.WorkingDirectory = workingDirectory;

            try
            {
                Process_.Start();

                // Set process priority
                Process_.PriorityClass = processPriorityClass;
                pid = Process_.Id;
            }
            catch (Exception ex)
            {
                pid = -1;
                Debug.WriteLine("ProcessConsole Execute Start: " + ex.Message);
            }

            Process_.OutputDataReceived -= Process__OutputDataReceived;
            Process_.OutputDataReceived += Process__OutputDataReceived;
            Process_.ErrorDataReceived -= Process__ErrorDataReceived;
            Process_.ErrorDataReceived += Process__ErrorDataReceived;

            Process_.BeginOutputReadLine();
            Process_.BeginErrorReadLine();

            GC.KeepAlive(Process_);

            Pid = pid;
            return pid;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProcessConsole Execute: " + ex.Message);
            return -1;
        }
    }

    private void Process__OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        string? msg = e.Data;
        if (msg != null)
        {
            // Add To Bag
            if (!string.IsNullOrWhiteSpace(msg))
            {
                Stdout = msg;
                StdoutBag.Add(msg);
            }

            StandardDataReceived?.Invoke(this, e);
        }
    }

    private void Process__ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        string? msg = e.Data;
        if (msg != null)
        {
            // Add To Bag
            if (!string.IsNullOrWhiteSpace(msg))
            {
                Stderr = msg;
                StderrBag.Add(msg);
            }

            ErrorDataReceived?.Invoke(this, e);
        }
    }

    /// <summary>
    /// Send Command to the Process and Get Result by GetStdout or GetStderr
    /// </summary>
    /// <param name="command">Command</param>
    /// <returns>Returns True if success</returns>
    public async Task<bool> SendCommandAsync(string command, int delayMS = 50, int timeoutSec = 15, string confirmMsg = "")
    {
        bool isSent = false;

        try
        {
            if (Process_ != null && ProcessManager.FindProcessByPID(GetPid))
            {
                // Delay
                Task wait1 = Task.Run(async () =>
                {
                    while (true)
                    {
                        int n1 = GetStdoutBag.Count;
                        await Task.Delay(delayMS);
                        int n2 = GetStdoutBag.Count;
                        if (n1 == n2) break;
                    }
                });
                try { await wait1.WaitAsync(CancellationToken.None); } catch (Exception) { }

                // Send Command
                Task timeout = Task.Run(async () =>
                {
                    await Process_.StandardInput.WriteLineAsync(command);
                    isSent = true;
                });
                try { await timeout.WaitAsync(TimeSpan.FromSeconds(timeoutSec)); } catch (Exception) { }

                // Get Confirm
                if (!string.IsNullOrWhiteSpace(confirmMsg))
                {
                    Task confirm = Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (GetStdout.Equals(confirmMsg))
                            {
                                isSent = true;
                                break;
                            }
                            else
                            {
                                isSent = false;
                                await Task.Delay(delayMS);
                            }
                        }
                    });
                    try { await confirm.WaitAsync(TimeSpan.FromSeconds(timeoutSec)); } catch (Exception) { }
                }
            }
        }
        catch (Exception ex)
        {
            //Debug.WriteLine("SendCommandAsync: " + command);
            Debug.WriteLine("SendCommandAsync: " + ex.Message);
            isSent = false;
        }

        return isSent;
    }

}