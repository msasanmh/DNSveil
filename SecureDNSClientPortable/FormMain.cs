using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SecureDNSClientPortable
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            Hide();
            Opacity = 0;

            try
            {
                Architecture arch = RuntimeInformation.OSArchitecture;
                string architecture = arch.ToString();
                string? appPath = null;

                if (arch == Architecture.X64)
                    appPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "SecureDNSClient", "SecureDNSClient.exe"));
                else if (arch == Architecture.X86 || arch == Architecture.Arm || arch == Architecture.Arm64)
                    appPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "SecureDNSClient_X86", "SecureDNSClient.exe"));

                if (!string.IsNullOrEmpty(appPath))
                {
                    string startupArgs = $"startup {Program.StartupDelaySec}";
                    if (Program.Startup)
                        Process.Start(appPath, startupArgs);
                    else
                        Process.Start(appPath);
                }
                else
                {
                    string msgNotSupported = $"Your CPU Architecture ({architecture}) is Not Supported.";
                    MessageBox.Show(msgNotSupported, "Not Supported", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
            Task.Delay(3000).Wait();
            Close();
            Environment.Exit(0);
            Application.Exit();
        }
    }
}