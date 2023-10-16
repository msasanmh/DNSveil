using System.Diagnostics;
using Process = System.Diagnostics.Process;

namespace MsmhToolsClass;

public class ProcessConsole
{
    private string Stdout { get; set; } = string.Empty;
    private string Stderr { get; set; } = string.Empty;
    private int Pid { get; set; } = -1;
    public Process? Process_ { get; set; }
    public event EventHandler<DataReceivedEventArgs>? StandardDataReceived;
    public event EventHandler<DataReceivedEventArgs>? ErrorDataReceived;

    public ProcessConsole()
    {
        
    }

    public string GetStdout => Stdout;
    public string GetStderr => Stderr;
    public int GetPid => Pid;

    /// <summary>
    /// Execute and returns PID, if faild returns -1
    /// </summary>
    public int Execute(string processName, string? args = null, bool hideWindow = true, bool runAsAdmin = false, string? workingDirectory = null, ProcessPriorityClass processPriorityClass = ProcessPriorityClass.Normal)
    {
        int pid;
        // Create process
        Process_ = new();
        Process_.StartInfo.FileName = processName;

        if (args != null)
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

        if (workingDirectory != null)
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
            Debug.WriteLine($"ExecuteOnly: {ex.Message}");
        }

        Process_.OutputDataReceived -= Process__OutputDataReceived;
        Process_.OutputDataReceived += Process__OutputDataReceived;
        Process_.ErrorDataReceived -= Process__ErrorDataReceived;
        Process_.ErrorDataReceived += Process__ErrorDataReceived;

        Process_.BeginOutputReadLine();
        Process_.BeginErrorReadLine();

        Pid = pid;
        return pid;
    }

    private void Process__OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        string? msg = e.Data;
        if (msg != null)
        {
            Stdout = msg;
            StandardDataReceived?.Invoke(this, e);
        }
    }

    private void Process__ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        string? msg = e.Data;
        if (msg != null)
        {
            Stderr = msg;
            ErrorDataReceived?.Invoke(this, e);
        }
    }

    /// <summary>
    /// Send Command to the Process and Get Result by GetStdout or GetStderr
    /// </summary>
    /// <param name="command">Command</param>
    /// <returns>Returns True if success</returns>
    public async Task<bool> SendCommandAsync(string command)
    {
        try
        {
            if (Process_ != null && ProcessManager.FindProcessByPID(GetPid))
            {
                Task<bool> timeout = Task.Run(async () =>
                {
                    await Process_.StandardInput.WriteLineAsync(command);
                    await Task.Delay(100);
                    return true;
                });
                try { await timeout.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }
                return timeout.Result;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

}