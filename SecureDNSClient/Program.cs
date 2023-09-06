using MsmhToolsClass;
using System.Reflection;

namespace SecureDNSClient
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

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
}