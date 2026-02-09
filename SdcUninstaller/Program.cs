using MsmhToolsClass;
using System.Diagnostics;
using Microsoft.Win32.TaskScheduler;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using Task = System.Threading.Tasks.Task;

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
    private static readonly string CertSubjectName = "SecureDNSClient";
    private static readonly string PublisherName = "MSasanMH";
    private static readonly string AppDirName = "Secure DNS Client";
    public static readonly Architecture ArchProcess = RuntimeInformation.ProcessArchitecture;

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static async Task Main()
    {
        try
        {
            // Hide
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            // Kill Processes
            await ProcessManager.KillProcessByNameAsync("SDCAgnosticServer", true);
            await ProcessManager.KillProcessByNameAsync("dnslookup", true);
            await ProcessManager.KillProcessByNameAsync("goodbyedpi", true);
            await ProcessManager.KillProcessByNameAsync("SecureDNSClient", true);

            // Delete GoodbyeDPI And WinDivert Services
            await DeleteGoodbyeDpiAndWinDivertServices_Async();

            // Remove Startup
            ActivateWindowsStartup(false);

            // Uninstall Certificate
            List<Tuple<string, StoreName, StoreLocation>> stores = new()
            {
                // Add Root Cert
                new Tuple<string, StoreName, StoreLocation>(CertIssuerSubjectName, StoreName.AddressBook, StoreLocation.CurrentUser),
                new Tuple<string, StoreName, StoreLocation>(CertIssuerSubjectName, StoreName.AddressBook, StoreLocation.LocalMachine),
                new Tuple<string, StoreName, StoreLocation>(CertIssuerSubjectName, StoreName.AuthRoot, StoreLocation.CurrentUser),
                new Tuple<string, StoreName, StoreLocation>(CertIssuerSubjectName, StoreName.AuthRoot, StoreLocation.LocalMachine),
                new Tuple<string, StoreName, StoreLocation>(CertIssuerSubjectName, StoreName.CertificateAuthority, StoreLocation.CurrentUser),
                new Tuple<string, StoreName, StoreLocation>(CertIssuerSubjectName, StoreName.CertificateAuthority, StoreLocation.LocalMachine),
                new Tuple<string, StoreName, StoreLocation>(CertIssuerSubjectName, StoreName.Disallowed, StoreLocation.CurrentUser),
                new Tuple<string, StoreName, StoreLocation>(CertIssuerSubjectName, StoreName.Disallowed, StoreLocation.LocalMachine),
                new Tuple<string, StoreName, StoreLocation>(CertIssuerSubjectName, StoreName.My, StoreLocation.CurrentUser),
                new Tuple<string, StoreName, StoreLocation>(CertIssuerSubjectName, StoreName.My, StoreLocation.LocalMachine),
                new Tuple<string, StoreName, StoreLocation>(CertIssuerSubjectName, StoreName.Root, StoreLocation.CurrentUser),
                new Tuple<string, StoreName, StoreLocation>(CertIssuerSubjectName, StoreName.Root, StoreLocation.LocalMachine),
                new Tuple<string, StoreName, StoreLocation>(CertIssuerSubjectName, StoreName.TrustedPeople, StoreLocation.CurrentUser),
                new Tuple<string, StoreName, StoreLocation>(CertIssuerSubjectName, StoreName.TrustedPeople, StoreLocation.LocalMachine),
                new Tuple<string, StoreName, StoreLocation>(CertIssuerSubjectName, StoreName.TrustedPublisher, StoreLocation.CurrentUser),
                new Tuple<string, StoreName, StoreLocation>(CertIssuerSubjectName, StoreName.TrustedPublisher, StoreLocation.LocalMachine),

                // Add Cert
                new Tuple<string, StoreName, StoreLocation>(CertSubjectName, StoreName.AddressBook, StoreLocation.CurrentUser),
                new Tuple<string, StoreName, StoreLocation>(CertSubjectName, StoreName.AddressBook, StoreLocation.LocalMachine),
                new Tuple<string, StoreName, StoreLocation>(CertSubjectName, StoreName.AuthRoot, StoreLocation.CurrentUser),
                new Tuple<string, StoreName, StoreLocation>(CertSubjectName, StoreName.AuthRoot, StoreLocation.LocalMachine),
                new Tuple<string, StoreName, StoreLocation>(CertSubjectName, StoreName.CertificateAuthority, StoreLocation.CurrentUser),
                new Tuple<string, StoreName, StoreLocation>(CertSubjectName, StoreName.CertificateAuthority, StoreLocation.LocalMachine),
                new Tuple<string, StoreName, StoreLocation>(CertSubjectName, StoreName.Disallowed, StoreLocation.CurrentUser),
                new Tuple<string, StoreName, StoreLocation>(CertSubjectName, StoreName.Disallowed, StoreLocation.LocalMachine),
                new Tuple<string, StoreName, StoreLocation>(CertSubjectName, StoreName.My, StoreLocation.CurrentUser),
                new Tuple<string, StoreName, StoreLocation>(CertSubjectName, StoreName.My, StoreLocation.LocalMachine),
                new Tuple<string, StoreName, StoreLocation>(CertSubjectName, StoreName.Root, StoreLocation.CurrentUser),
                new Tuple<string, StoreName, StoreLocation>(CertSubjectName, StoreName.Root, StoreLocation.LocalMachine),
                new Tuple<string, StoreName, StoreLocation>(CertSubjectName, StoreName.TrustedPeople, StoreLocation.CurrentUser),
                new Tuple<string, StoreName, StoreLocation>(CertSubjectName, StoreName.TrustedPeople, StoreLocation.LocalMachine),
                new Tuple<string, StoreName, StoreLocation>(CertSubjectName, StoreName.TrustedPublisher, StoreLocation.CurrentUser),
                new Tuple<string, StoreName, StoreLocation>(CertSubjectName, StoreName.TrustedPublisher, StoreLocation.LocalMachine),
            };

            foreach (Tuple<string, StoreName, StoreLocation> store in stores)
            {
                try
                {
                    bool isCertInstalled = CertificateTool.IsCertificateInstalled(store.Item1, store.Item2, store.Item3);
                    if (isCertInstalled)
                    {
                        CertificateTool.UninstallCertificate(store.Item1, store.Item2, store.Item3);
                    }
                }
                catch (Exception) { }
            }

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

    private static async Task DeleteGoodbyeDpiAndWinDivertServices_Async()
    {
        string service1 = "GoodbyeDPI", service2 = "WinDivert";
        await ServiceTool.DeleteWhereAsync(service1);
        await DriverTool.DeleteWhereAsync(service1);
        await ServiceTool.DeleteWhereAsync(service2);
        await DriverTool.DeleteWhereAsync(service2);
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

    private static async void RemoveProgramFiles(string programFiles)
    {
        try
        {
            string appDir = Path.GetFullPath(Path.Combine(programFiles, PublisherName, AppDirName));
            Debug.WriteLine(appDir);
            if (Directory.Exists(appDir))
            {
                List<string> allFiles = await FileDirectory.GetAllFilesAsync(appDir);
                foreach (string file in allFiles)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception) { }
                }

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