using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SecureDNSClientPortable
{
    public partial class FormMain : Form
    {
        public static readonly Architecture ArchOs = RuntimeInformation.OSArchitecture;
        public static readonly Architecture ArchProcess = RuntimeInformation.ProcessArchitecture;

        public FormMain()
        {
            InitializeComponent();
            Hide();
            Opacity = 0;

            try
            {
                string? appPath = null;

                bool x64 = ArchProcess == Architecture.X64 && ArchOs == Architecture.X64;
                bool x86 = ArchProcess == Architecture.X86 &&
                           (ArchOs == Architecture.X86 || ArchOs == Architecture.X64 ||
                           ArchOs == Architecture.Arm || ArchOs == Architecture.Arm64);

                if (x64 || x86)
                    appPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "SecureDNSClient", "SecureDNSClient.exe"));

                if (!string.IsNullOrEmpty(appPath))
                {
                    string args = $"-IsPortable=True";
                    Process.Start(appPath, args);
                }
                else
                {
                    string msgNotSupported = $"Can't Run {ArchProcess} Application On {ArchOs} OS.";
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