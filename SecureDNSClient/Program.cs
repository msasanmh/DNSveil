using MsmhToolsClass;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace SecureDNSClient;

internal static partial class Program
{
    private static readonly ReaderWriterLockSlim Locker = new();

    internal static bool IsPortable = false;
    internal static bool IsStartup = false;
    internal static int StartupDelaySec = 0; // Default

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        try
        {
            // Setting Culture Is Necessary To Read Args In Any Windows Display Language Other Than English
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        }
        catch (Exception) { }

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

                // New CLI
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
        Mutex mutex = new(false, productName);
        if (!mutex.WaitOne(0, true))
        {
            MessageBox.Show($"{productName} is already running.");
            Environment.Exit(0);
            Application.Exit();
            return;
        }
        GC.KeepAlive(mutex);

        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        Application.ThreadException += Application_ThreadException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        Application.Run(new FormMain());
    }

    private static async void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        try
        {
            string err = e.Exception.GetInnerExceptions() + Environment.NewLine;
            Debug.WriteLine("-=-=-=-=-=-=-=-=-=-=-=-= UnobservedTaskException:" + Environment.NewLine + err);

            try
            {
                Locker.EnterWriteLock();
                await FileDirectory.AppendTextLineAsync(SecureDNS.ErrorLogPath, err, new UTF8Encoding(false));
            }
            catch (Exception) { }
            finally
            {
                Locker.ExitWriteLock();
            }
        }
        catch (Exception) { }
    }

    private static async void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
        try
        {
            string err = e.Exception.GetInnerExceptions() + Environment.NewLine;
            Debug.WriteLine("-=-=-=-=-=-=-=-=-=-=-=-= ThreadException:" + Environment.NewLine + err);

            try
            {
                Locker.EnterWriteLock();
                await FileDirectory.AppendTextLineAsync(SecureDNS.ErrorLogPath, err, new UTF8Encoding(false));
            }
            catch (Exception) { }
            finally
            {
                Locker.ExitWriteLock();
            }
        }
        catch (Exception) { }
    }

    private static async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            string? err = e.ExceptionObject.ToString() + Environment.NewLine;
            if (string.IsNullOrEmpty(err)) return;
            Debug.WriteLine("-=-=-=-=-=-=-=-=-=-=-=-= UnhandledException:" + Environment.NewLine + err);

            try
            {
                Locker.EnterWriteLock();
                await FileDirectory.AppendTextLineAsync(SecureDNS.ErrorLogPath, err, new UTF8Encoding(false));
            }
            catch (Exception) { }
            finally
            {
                Locker.ExitWriteLock();
            }
        }
        catch (Exception) { }
    }
}