using System.Diagnostics;

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
                Process.Start(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "SecureDNSClient", "SecureDNSClient.exe")));
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