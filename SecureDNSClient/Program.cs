using MsmhToolsClass;
using System.Reflection;

namespace SecureDNSClient;

internal static class Program
{
    internal static bool Startup = false;
    internal static int StartupDelaySec = 10;

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Exit If It's Root Directory
        if (FileDirectory.IsRootDirectory())
        {
            string isRoot = "Application cannot run on root directory.";
            MessageBox.Show(isRoot, "Not Supported", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Environment.Exit(0);
            Application.Exit();
            return;
        }

        string[] args = Environment.GetCommandLineArgs();
        if (args.Any())
        {
            if (args.Length >= 2)
            {
                string su = args[1].Trim().ToLower();
                if (su.Equals("startup")) Startup = true;
            }

            if (args.Length >= 3)
            {
                string d = args[2].Trim().ToLower();
                bool isInt = int.TryParse(d, out int value);
                if (isInt) StartupDelaySec = value;
            }
        }
        
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        // Prevent multiple instances
        using Mutex mutex = new(false, Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductName);
        if (!mutex.WaitOne(0, true))
        {
            MessageBox.Show($"{Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductName} is already running.");
            Environment.Exit(0);
            Application.Exit();
            return;
        }
        GC.KeepAlive(mutex);

        Application.Run(new FormMain());
    }
}