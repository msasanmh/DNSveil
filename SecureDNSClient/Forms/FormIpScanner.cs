using CustomControls;
using MsmhToolsClass;
using MsmhToolsWinFormsClass;
using MsmhToolsWinFormsClass.Themes;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace SecureDNSClient;

public partial class FormIpScanner : Form
{
    // AgnosticSettings XML path
    private static readonly string SettingsXmlPath = SecureDNS.SettingsXmlIpScanner;
    private readonly Settings AppSettings;

    private readonly IpScanner Scanner = new();
    private readonly ToolStripMenuItem ToolStripMenuItemCopy = new();
    private bool IsExiting { get; set; } = false;
    private bool IsExitDone { get; set; } = false;

    public FormIpScanner()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        InitializeComponent();

        // Invariant Culture
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        // Load Theme
        Theme.LoadTheme(this, Theme.Themes.Dark);

        CustomLabelChecking.Text = "Checking: ";

        ToolStripMenuItemCopy.Text = "Copy IP";
        ToolStripMenuItemCopy.Click -= ToolStripMenuItemCopy_Click;
        ToolStripMenuItemCopy.Click += ToolStripMenuItemCopy_Click;
        CustomContextMenuStripMain.Items.Add(ToolStripMenuItemCopy);

        FillComboBox(CustomComboBoxSelectCIDR);

        // Set Tooltips
        string msgCheckWebsite = "An open website with chosen CDN to check. e.g. https://www.cloudflare.com";
        CustomLabelCheckWebsite.SetToolTip(FormMain.MainToolTip, "Info", msgCheckWebsite);
        CustomTextBoxCheckWebsite.SetToolTip(FormMain.MainToolTip, "Info", msgCheckWebsite);

        // Initialize and load AgnosticSettings
        if (File.Exists(SettingsXmlPath) && XmlTool.IsValidXMLFile(SettingsXmlPath))
            AppSettings = new(this, SettingsXmlPath);
        else
            AppSettings = new(this);

        FixScreenDpi();
    }

    private async void FixScreenDpi()
    {
        // Setting Width Of Controls
        await ScreenDPI.SettingWidthOfControls(this);

        // Fix Controls Location
        int shw = TextRenderer.MeasureText("I", Font).Width;
        int spaceBottom = 10, spaceRight = 12, spaceV = 10, spaceH = shw, spaceHH = (spaceH * 3);
        CustomRadioButtonSourceCloudflare.Location = new Point(spaceRight, spaceBottom);

        CustomLabelDelay.Left = CustomRadioButtonSourceCloudflare.Right + spaceHH;
        CustomLabelDelay.Top = CustomRadioButtonSourceCloudflare.Top;

        CustomNumericUpDownDelay.Left = CustomLabelDelay.Right + spaceH;
        CustomNumericUpDownDelay.Top = CustomLabelDelay.Top - 2;

        CustomButtonStartStop.Left = CustomNumericUpDownDelay.Right + spaceHH;
        CustomButtonStartStop.Top = CustomNumericUpDownDelay.Top;

        CustomLabelSelectCIDR.Left = spaceRight;
        CustomLabelSelectCIDR.Top = CustomButtonStartStop.Bottom + spaceV;

        CustomComboBoxSelectCIDR.Left = CustomLabelSelectCIDR.Right + spaceH;
        CustomComboBoxSelectCIDR.Top = CustomLabelSelectCIDR.Top - 2;

        CustomLabelCheckWebsite.Left = spaceRight;
        CustomLabelCheckWebsite.Top = CustomLabelSelectCIDR.Bottom + (spaceV * 2);

        CustomTextBoxCheckWebsite.Left = CustomLabelCheckWebsite.Right + spaceH;
        CustomTextBoxCheckWebsite.Top = CustomLabelCheckWebsite.Top - 2;

        CustomLabelCheckIpWithThisPort.Left = spaceRight;
        CustomLabelCheckIpWithThisPort.Top = CustomTextBoxCheckWebsite.Bottom + spaceV;

        CustomNumericUpDownCheckIpWithThisPort.Left = CustomLabelCheckIpWithThisPort.Right + spaceH;
        CustomNumericUpDownCheckIpWithThisPort.Top = CustomLabelCheckIpWithThisPort.Top - 2;

        CustomCheckBoxRandomScan.Left = spaceRight;
        CustomCheckBoxRandomScan.Top = CustomNumericUpDownCheckIpWithThisPort.Bottom + spaceV;

        CustomLabelChecking.Left = spaceRight;
        CustomLabelChecking.Top = CustomCheckBoxRandomScan.Bottom + spaceV;
        CustomLabelChecking.Width = ClientRectangle.Width - (spaceRight * 2);

        CustomDataGridViewResult.Left = spaceRight;
        CustomDataGridViewResult.Top = CustomLabelChecking.Bottom + spaceV;
        CustomDataGridViewResult.Width = ClientRectangle.Width - (spaceRight * 2);
        CustomDataGridViewResult.Height = ClientRectangle.Height - CustomDataGridViewResult.Top - spaceBottom;

        ClientSize = new(CustomButtonStartStop.Right + spaceRight, ClientSize.Height);

        CustomButtonStartStop.Text = "Start";

        Controllers.SetDarkControl(this);
    }

    private static void FillComboBox(CustomComboBox ccb)
    {
        // Built-in CF IPs
        List<string> cloudflareCIDRs = new()
        {
            "103.21.244.0/22",
            "103.22.200.0/22",
            "103.31.4.0/22",
            "104.16.0.0/13",
            "104.24.0.0/14",
            "108.162.192.0/18",
            "131.0.72.0/22",
            "141.101.64.0/18",
            "162.158.0.0/15",
            "172.64.0.0/13",
            "173.245.48.0/20",
            "188.114.96.0/20",
            "190.93.240.0/20",
            "197.234.240.0/22",
            "198.41.128.0/17",
            "2400:cb00::/32",
            "2405:8100::/32",
            "2405:b500::/32",
            "2606:4700::/32",
            "2803:f800::/32",
            "2a06:98c0::/29",
            "2c0f:f248::/32"
        };

        ccb.InvokeIt(() =>
        {
            ccb.DataSource = cloudflareCIDRs;
            ccb.SetDarkControl();
        });
    }

    private void CustomButtonStartStop_Click(object? sender, EventArgs? e)
    {
        if (!Scanner.IsRunning) start();
        else stop();

        void start()
        {
            try
            {
                string? cidr = string.Empty;
                this.InvokeIt(() =>
                {
                    if (CustomComboBoxSelectCIDR.SelectedIndex == -1 && CustomComboBoxSelectCIDR.Items.Count > 0)
                        CustomComboBoxSelectCIDR.SelectedIndex = 0;
                    cidr = CustomComboBoxSelectCIDR.SelectedItem.ToString();
                });
                if (string.IsNullOrWhiteSpace(cidr)) return;

                // Start
                CustomDataGridViewResult.Rows.Clear();

                Scanner.SetIpRange(new List<string> { cidr });
                Scanner.CheckPort = Convert.ToInt32(CustomNumericUpDownCheckIpWithThisPort.Value);
                Scanner.CheckWebsite = CustomTextBoxCheckWebsite.Text;
                Scanner.RandomScan = CustomCheckBoxRandomScan.Checked;
                Scanner.Timeout = Convert.ToInt32(CustomNumericUpDownDelay.Value * 1000);

                Scanner.OnWorkingIpReceived -= Scanner_OnWorkingIpReceived;
                Scanner.OnWorkingIpReceived += Scanner_OnWorkingIpReceived;
                Scanner.OnFullReportChanged -= Scanner_OnFullReportChanged;
                Scanner.OnFullReportChanged += Scanner_OnFullReportChanged;

                if (!Scanner.IsRunning)
                {
                    Scanner.Start();
                    this.InvokeIt(() => CustomButtonStartStop.Text = "Stop");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FormIpScanner Start: " + ex.Message);
            }
        }

        async void stop()
        {
            // Stop
            if (Scanner.IsRunning)
            {
                this.InvokeIt(() =>
                {
                    CustomButtonStartStop.Enabled = false;
                    CustomButtonStartStop.Text = "Stopping";
                    CustomLabelChecking.Text = "Checking: ";
                });

                Scanner.Stop();

                await Task.Run(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(100);
                        if (!Scanner.IsRunning) break;
                    }
                });

                this.InvokeIt(() =>
                {
                    CustomButtonStartStop.Enabled = true;
                    CustomButtonStartStop.Text = "Start";
                });
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
                row.Cells[0].Value = result.RealDelay;
                row.Cells[1].Value = result.TcpDelay;
                row.Cells[2].Value = result.PingDelay;
                row.Cells[3].Value = result.IP;
                row.Height = TextRenderer.MeasureText("It doesn't matter what we write here!", Font).Height + 5;

                CustomDataGridViewResult.Sort(CustomDataGridViewResult.Columns[0], ListSortDirection.Ascending);
                CustomDataGridViewResult.EndEdit();

                if (CustomDataGridViewResult.RowCount >= 20)
                    CustomButtonStartStop_Click(null, null);
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

                CustomContextMenuStripMain.RoundedCorners = 5;
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
        if (!IsExiting)
        {
            e.Cancel = true;
            IsExiting = true;

            this.InvokeIt(() =>
            {
                CustomButtonStartStop.Enabled = false;
                CustomButtonStartStop.Text = "Exiting";
            });

            if (Scanner.IsRunning) Scanner.Stop();

            // Select Control type and properties to save
            AppSettings.AddSelectedControlAndProperty(typeof(CustomCheckBox), "Checked");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomNumericUpDown), "Value");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomRadioButton), "Checked");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomTextBox), "Text");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomTextBox), "Texts");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomComboBox), "SelectedIndex");

            // Add AgnosticSettings to save
            AppSettings.AddSelectedSettings(this);

            // Save Application AgnosticSettings
            await AppSettings.SaveAsync(SettingsXmlPath);

            await Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(100);
                    if (!Scanner.IsRunning) break;
                }
            });
            
            IsExitDone = true;

            e.Cancel = false;
            Close();
        }
        else
        {
            if (!IsExitDone)
            {
                e.Cancel = true;
                return;
            }

            Dispose();
        }
    }
}