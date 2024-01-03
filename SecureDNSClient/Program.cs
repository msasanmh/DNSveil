using MsmhToolsClass;
using System.Diagnostics;
using System.Reflection;

namespace SecureDNSClient;

internal static partial class Program
{
    internal static bool IsPortable = false;
    internal static bool IsStartup = false;
    internal static int StartupDelaySec = 0; // Default

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Exit If It's Root Directory
        if (FileDirectory.IsRootDirectory())
        {
            try
            {
                string isRoot = "Application cannot run on root directory.";
                MessageBox.Show(isRoot, "Not Supported", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Environment.Exit(0);
                Application.Exit();
                return;
            }
            catch (Exception) { }
        }

        try
        {
            string[] args = Environment.GetCommandLineArgs();
            bool oldCli = false;
            if (args.Any())
            {
                // Support Old Cli
                if (args.Length >= 2)
                {
                    string su = args[1].Trim().ToLower();
                    if (su.Equals("startup"))
                    {
                        IsPortable = true;
                        IsStartup = true;
                        oldCli = true;
                    }
                }

                if (oldCli && args.Length >= 3)
                {
                    string d = args[2].Trim().ToLower();
                    bool isInt = int.TryParse(d, out int value);
                    if (isInt) StartupDelaySec = value;
                }

                // New Cli
                if (!oldCli)
                {
                    for (int n = 0; n < args.Length; n++)
                    {
                        string arg = args[n].ToLower().Trim(); // e.g. -IsStartup=True

                        KeyValue kv = GetValue(arg);
                        if (string.IsNullOrEmpty(kv.Key)) continue;
                        if (kv.Key.Equals(Key.IsPortable, StringComparison.InvariantCultureIgnoreCase) && kv.Type == typeof(bool)) IsPortable = kv.ValueBool;
                        if (kv.Key.Equals(Key.IsStartup, StringComparison.InvariantCultureIgnoreCase) && kv.Type == typeof(bool)) IsStartup = kv.ValueBool;
                        if (kv.Key.Equals(Key.StartupDelaySec, StringComparison.InvariantCultureIgnoreCase) && kv.Type == typeof(int)) StartupDelaySec = kv.ValueInt;
                    }
                }
            }
        }
        catch (Exception) { }

#if DEBUG
        Debug.WriteLine("=========");
        Debug.WriteLine("Is Portable: " + IsPortable);
        Debug.WriteLine("Is Startup: " + IsStartup);
        Debug.WriteLine("Startup Delay: " + StartupDelaySec);
        Debug.WriteLine("=========");
#endif

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        // Prevent multiple instances
        string productName = Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductName ?? "SDC - Secure DNS Client";
        using Mutex mutex = new(false, productName);
        if (!mutex.WaitOne(0, true))
        {
            MessageBox.Show($"{productName} is already running.");
            Environment.Exit(0);
            Application.Exit();
            return;
        }
        GC.KeepAlive(mutex);

        Application.Run(new FormMain());
    }
}