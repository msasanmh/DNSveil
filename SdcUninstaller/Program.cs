using MsmhToolsClass;
using System.Diagnostics;
using Microsoft.Win32.TaskScheduler;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;

namespace SdcUninstaller;

internal static class Program
{
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;
    const int SW_SHOW = 5;

    private static readonly string CertIssuerSubjectName = "SecureDNSClient Authority";
    private static readonly string PublisherName = "MSasanMH";
    private static readonly string AppDirName = "Secure DNS Client";
    public static readonly Architecture ArchProcess = RuntimeInformation.ProcessArchitecture;

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        try
        {
            // Hide
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            // Kill Processes
            ProcessManager.KillProcessByName("SDCProxyServer", true);
            ProcessManager.KillProcessByName("dnslookup", true);
            ProcessManager.KillProcessByName("dnsproxy", true);
            ProcessManager.KillProcessByName("dnscrypt-proxy", true);
            ProcessManager.KillProcessByName("goodbyedpi", true);
            ProcessManager.KillProcessByName("SecureDNSClient", true);

            // Remove Startup
            ActivateWindowsStartup(false);

            // Uninstall Certificate
            bool isCertInstalled = CertificateTool.IsCertificateInstalled(CertIssuerSubjectName, StoreName.Root, StoreLocation.CurrentUser);
            if (isCertInstalled)
                CertificateTool.UninstallCertificate(CertIssuerSubjectName, StoreName.Root, StoreLocation.CurrentUser);

            // Remove Install Dir
            try
            {
                string programFilesX64 = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
                RemoveProgramFiles(programFilesX64);
            }
            catch (Exception) { }

            try
            {
                string programFilesX86 = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
                RemoveProgramFiles(programFilesX86);
            }
            catch (Exception) { }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Main Entry Point: " + ex.Message);
        }
    }

    public static void ActivateWindowsStartup(bool active)
    {
        try
        {
            string taskName = "SecureDnsClientStartup";
            using TaskService ts = new();
            TaskDefinition td = ts.NewTask();
            td.RegistrationInfo.Description = "Secure DNS Client Startup";
            td.Triggers.Add(new LogonTrigger()); // Trigger at Logon
            td.Principal.RunLevel = TaskRunLevel.Highest;
            td.Settings.Enabled = active;
            td.Settings.AllowDemandStart = true;
            td.Settings.DisallowStartIfOnBatteries = false;
            td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;
            td.Settings.StopIfGoingOnBatteries = false;
            td.Settings.Hidden = false; // Don't Hide App
            if (active)
                ts.RootFolder.RegisterTaskDefinition(taskName, td); // Add Task
            else
                ts.RootFolder.DeleteTask(taskName); // Remove Task
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ActivateWindowsStartup: " + ex.Message);
        }
    }

    private static void RemoveProgramFiles(string programFiles)
    {
        try
        {
            string appDir = Path.GetFullPath(Path.Combine(programFiles, PublisherName, AppDirName));
            Debug.WriteLine(appDir);
            if (Directory.Exists(appDir))
            {
                try
                {
                    Directory.Delete(appDir, true);
                }
                catch (Exception) { }
            }

            // Remove Publisher Dir If It's Empty
            string pubDir = Path.GetFullPath(Path.Combine(programFiles, PublisherName));
            if (Directory.Exists(pubDir) && FileDirectory.IsDirectoryEmpty(pubDir))
            {
                try
                {
                    Directory.Delete(pubDir, true);
                }
                catch (Exception) { }
            }
        }
        catch (Exception) { }
    }
}