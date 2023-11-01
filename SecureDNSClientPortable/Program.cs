namespace SecureDNSClientPortable;

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
        Application.Run(new FormMain());
    }
}