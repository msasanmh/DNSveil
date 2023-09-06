using CustomControls;
using MsmhToolsClass;
using MsmhToolsWinFormsClass;
using MsmhToolsWinFormsClass.Themes;
using System.ComponentModel;

namespace SecureDNSClient
{
    public partial class FormIpScanner : Form
    {
        // Settings XML path
        private static readonly string SettingsXmlPath = SecureDNS.SettingsXmlIpScanner;
        private readonly Settings AppSettings;

        private readonly IpScanner Scanner = new();
        private readonly ToolStripMenuItem ToolStripMenuItemCopy = new();
        public FormIpScanner()
        {
            // Fix Screed DPI
            ScreenDPI.FixDpiBeforeInitializeComponent(this);
            InitializeComponent();
            ScreenDPI.FixDpiAfterInitializeComponent(this);

            // Load Theme
            Theme.LoadTheme(this, Theme.Themes.Dark);

            CustomLabelChecking.Text = "Checking: ";

            ToolStripMenuItemCopy.Text = "Copy IP";
            ToolStripMenuItemCopy.Click -= ToolStripMenuItemCopy_Click;
            ToolStripMenuItemCopy.Click += ToolStripMenuItemCopy_Click;
            CustomContextMenuStripMain.Items.Add(ToolStripMenuItemCopy);

            // Set Tooltips
            string msgCheckWebsite = "An open website with chosen CDN to check. e.g. https://www.cloudflare.com";
            CustomLabelCheckWebsite.SetToolTip("Info", msgCheckWebsite);
            CustomTextBoxCheckWebsite.SetToolTip("Info", msgCheckWebsite);

            // Initialize and load Settings
            if (File.Exists(SettingsXmlPath) && XmlTool.IsValidXMLFile(SettingsXmlPath))
                AppSettings = new(this, SettingsXmlPath);
            else
                AppSettings = new(this);
        }

        private void CustomButtonStartStop_Click(object sender, EventArgs e)
        {
            // Built-in CF IPs
            string defaultCfIPs = "103.21.244.0 - 103.21.244.255\n";
            defaultCfIPs += "103.22.200.0 - 103.22.200.255\n";
            defaultCfIPs += "103.31.4.0 - 103.31.5.255\n";
            defaultCfIPs += "104.16.0.0 - 104.31.255.255\n";
            defaultCfIPs += "108.162.192.0 - 108.162.207.255\n";
            defaultCfIPs += "131.0.72.0 - 131.0.75.255\n";
            defaultCfIPs += "141.101.64.0 - 141.101.65.255\n";
            defaultCfIPs += "162.158.0.0 - 162.158.3.255\n";
            defaultCfIPs += "172.64.0.0 - 172.67.255.255\n";
            defaultCfIPs += "173.245.48.0 - 173.245.48.255\n";
            defaultCfIPs += "188.114.96.0 - 188.114.99.255\n";
            defaultCfIPs += "190.93.240.0 - 190.93.243.255\n";
            defaultCfIPs += "197.234.240.0 - 197.234.243.255\n";
            defaultCfIPs += "198.41.128.0 - 198.41.143.255";

            if (!Scanner.IsRunning)
                start();
            else
                stop();

            void start()
            {
                // Start
                CustomDataGridViewResult.Rows.Clear();

                Scanner.SetIpRange(defaultCfIPs);
                Scanner.CheckPort = Convert.ToInt32(CustomNumericUpDownCheckIpWithThisPort.Value);
                Scanner.CheckWebsite = CustomTextBoxCheckWebsite.Text;
                Scanner.ProxyServerPort = Convert.ToInt32(CustomNumericUpDownProxyPort.Value);
                Scanner.RandomScan = CustomCheckBoxRandomScan.Checked;
                Scanner.Timeout = Convert.ToInt32(CustomNumericUpDownDelay.Value * 1000);

                Scanner.OnWorkingIpReceived -= Scanner_OnWorkingIpReceived;
                Scanner.OnWorkingIpReceived += Scanner_OnWorkingIpReceived;
                Scanner.OnFullReportChanged += Scanner_OnFullReportChanged;

                if (!Scanner.IsRunning)
                    Scanner.Start();
            }

            void stop()
            {
                // Stop
                if (Scanner.IsRunning)
                {
                    Scanner.Stop();
                    this.InvokeIt(() => CustomLabelChecking.Text = "Checking: ");
                }
            }

        }

        private void Scanner_OnWorkingIpReceived(object? sender, EventArgs e)
        {
            if (sender is IpScannerResult result)
            {
                this.InvokeIt(() =>
                {
                    int rowId = CustomDataGridViewResult.Rows.Add();
                    DataGridViewRow row = CustomDataGridViewResult.Rows[rowId];

                    CustomDataGridViewResult.BeginEdit(false);
                    row.Height = 20;
                    row.Cells[0].Value = result.RealDelay;
                    row.Cells[1].Value = result.TcpDelay;
                    row.Cells[2].Value = result.PingDelay;
                    row.Cells[3].Value = result.IP;

                    CustomDataGridViewResult.Sort(CustomDataGridViewResult.Columns[0], ListSortDirection.Ascending);
                    CustomDataGridViewResult.EndEdit();
                });


            }
        }

        private void Scanner_OnFullReportChanged(object? sender, EventArgs e)
        {
            if (sender is string report)
            {
                this.InvokeIt(() => CustomLabelChecking.Text = report);
            }
        }

        private void CustomDataGridViewResult_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                CustomDataGridViewResult.Select(); // Set Focus on Control
                int currentMouseOverRow = CustomDataGridViewResult.HitTest(e.X, e.Y).RowIndex;
                if (currentMouseOverRow != -1)
                {
                    CustomDataGridViewResult.Rows[currentMouseOverRow].Cells[0].Selected = true;
                    CustomDataGridViewResult.Rows[currentMouseOverRow].Selected = true;

                    CustomContextMenuStripMain.Show(CustomDataGridViewResult, e.X, e.Y);
                }

            }
        }

        private void ToolStripMenuItemCopy_Click(object? sender, EventArgs e)
        {
            if (CustomDataGridViewResult.SelectedCells.Count > 0)
            {
                string? ip = CustomDataGridViewResult.CurrentRow.Cells[3].Value.ToString();
                if (!string.IsNullOrEmpty(ip))
                {
                    Clipboard.SetText(ip);
                }
            }
        }

        private async void FormIpScanner_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.WindowsShutDown)
            {
                if (Scanner.IsRunning)
                    Scanner.Stop();

                // Select Control type and properties to save
                AppSettings.AddSelectedControlAndProperty(typeof(CustomCheckBox), "Checked");
                AppSettings.AddSelectedControlAndProperty(typeof(CustomNumericUpDown), "Value");
                AppSettings.AddSelectedControlAndProperty(typeof(CustomRadioButton), "Checked");
                AppSettings.AddSelectedControlAndProperty(typeof(CustomTextBox), "Text");
                AppSettings.AddSelectedControlAndProperty(typeof(CustomTextBox), "Texts");

                // Add Settings to save
                AppSettings.AddSelectedSettings(this);

                // Save Application Settings
                await AppSettings.SaveAsync(SettingsXmlPath);
            }
        }
    }
}
