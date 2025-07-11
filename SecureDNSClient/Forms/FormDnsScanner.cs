using CustomControls;
using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using MsmhToolsWinFormsClass;
using MsmhToolsWinFormsClass.Themes;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;

namespace SecureDNSClient;

public partial class FormDnsScanner : Form
{
    // Settings XML path
    private static readonly string SettingsXmlPath = SecureDNS.SettingsXmlDnsScanner;
    private readonly Settings AppSettings;
    private readonly string NL = Environment.NewLine;
    private int PidDnsServer = -1;
    private string ScanContent = string.Empty;
    private readonly List<string> SelectedFilesList = new();
    private static List<string> SelectedURLsList = new();
    private static List<string> DomainsToCheckForSmartDNS = new();
    private CheckDns DnsScanner = new(false, true);
    private readonly CheckDns DnsScannerInsecureWithFilters = new(true, true);
    private readonly CheckDns DnsScannerInsecureNoFilters = new(true, false);
    private readonly string DNS = string.Empty;
    private bool IsSimpleTest = false;
    private readonly List<Tuple<string, string, bool, int, bool, int, Tuple<bool, bool, bool, bool>>> ExportList = new();
    private readonly List<string> ExportListPrivate = new();
    private bool IsRunning = false;
    private bool IsExiting { get; set; } = false;
    private bool IsExitDone { get; set; } = false;
    private bool Exit = false;

    public FormDnsScanner(string? dns = null)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        InitializeComponent();

        if (!string.IsNullOrEmpty(dns))
        {
            DNS = dns;
            IsSimpleTest = true;
        }

        // Invariant Culture
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        // Load Theme
        Theme.LoadTheme(this, Theme.Themes.Dark);

        // Initialize and load Settings
        if (File.Exists(SettingsXmlPath) && XmlTool.IsValidFile(SettingsXmlPath))
            AppSettings = new(this, SettingsXmlPath);
        else
            AppSettings = new(this);

        SelectedFilesList.Clear();
        SelectedURLsList.Clear();
        DomainsToCheckForSmartDNS.Clear();
        CustomButtonExport.Enabled = false;

        FixScreenDpi();

        Shown -= FormDnsScanner_Shown;
        Shown += FormDnsScanner_Shown;
    }

    private async void FixScreenDpi()
    {
        try
        {
            // Setting Width Of Controls
            await ScreenDPI.SettingWidthOfControls(this);

            // Fix Controls Location
            int shw = TextRenderer.MeasureText("I", Font).Width;
            int spaceBottom = 10, spaceRight = 12, spaceV = 10, spaceH = shw;

            // Left Side
            CustomRadioButtonDnsUrl.Location = new Point(spaceRight, spaceBottom);

            CustomTextBoxDnsUrl.Left = CustomRadioButtonDnsUrl.Right + spaceH;
            CustomTextBoxDnsUrl.Top = CustomRadioButtonDnsUrl.Top - 2;
            CustomTextBoxDnsUrl.Width = ClientRectangle.Width - CustomTextBoxDnsUrl.Left - spaceRight;

            CustomRadioButtonDnssFromFiles.Left = spaceRight;
            CustomRadioButtonDnssFromFiles.Top = CustomTextBoxDnsUrl.Bottom + spaceV;

            CustomButtonDnssFromFiles.Left = CustomRadioButtonDnssFromFiles.Right + spaceH;
            CustomButtonDnssFromFiles.Top = CustomRadioButtonDnssFromFiles.Top - 2;

            CustomLabelDnssFromFiles.Left = CustomButtonDnssFromFiles.Right + spaceH;
            CustomLabelDnssFromFiles.Top = CustomRadioButtonDnssFromFiles.Top + 2;

            CustomRadioButtonDnssFromURLs.Left = CustomRadioButtonDnssFromFiles.Left;
            CustomRadioButtonDnssFromURLs.Top = CustomButtonDnssFromFiles.Bottom + spaceV;

            CustomButtonDnssFromURLs.Left = CustomButtonDnssFromFiles.Left;
            CustomButtonDnssFromURLs.Top = CustomRadioButtonDnssFromURLs.Top - 2;

            CustomRadioButtonCustomServers.Left = CustomRadioButtonDnssFromURLs.Left;
            CustomRadioButtonCustomServers.Top = CustomButtonDnssFromURLs.Bottom + spaceV;

            // Right Side
            CustomButtonScan.Left = ClientRectangle.Width - CustomButtonScan.Width - spaceRight;
            CustomButtonScan.Top = CustomTextBoxDnsUrl.Bottom + spaceV;

            CustomButtonExport.Left = CustomButtonScan.Left;
            CustomButtonExport.Top = CustomButtonScan.Bottom + spaceV;

            CustomCheckBoxClearExportData.Left = ClientRectangle.Width - CustomCheckBoxClearExportData.Width - spaceRight;
            CustomCheckBoxClearExportData.Top = CustomButtonExport.Bottom + spaceV;

            CustomCheckBoxCheckInsecure.Left = CustomCheckBoxClearExportData.Left;
            CustomCheckBoxCheckInsecure.Top = CustomCheckBoxClearExportData.Bottom + spaceV;

            // Left Side Part 2
            CustomCheckBoxSmartDNS.Left = spaceRight;
            CustomCheckBoxSmartDNS.Top = CustomRadioButtonCustomServers.Bottom + spaceV;

            CustomButtonSmartDnsSelect.Left = spaceRight * 2;
            CustomButtonSmartDnsSelect.Top = CustomCheckBoxSmartDNS.Bottom + spaceH;

            CustomLabelSmartDnsStatus.Left = CustomButtonSmartDnsSelect.Right + spaceH;
            CustomLabelSmartDnsStatus.Top = CustomButtonSmartDnsSelect.Top + 4;

            CustomLabelBootstrapIp.Left = spaceRight;
            CustomLabelBootstrapIp.Top = CustomButtonSmartDnsSelect.Bottom + (spaceV * 2);

            CustomTextBoxBootstrapIpPort.Left = CustomLabelBootstrapIp.Right + spaceH;
            CustomTextBoxBootstrapIpPort.Top = CustomLabelBootstrapIp.Top - 2;

            CustomLabelBootstrapPort.Left = CustomTextBoxBootstrapIpPort.Right + (spaceH * 2);
            CustomLabelBootstrapPort.Top = CustomLabelBootstrapIp.Top;

            CustomNumericUpDownBootstrapPort.Left = CustomLabelBootstrapPort.Right + spaceH;
            CustomNumericUpDownBootstrapPort.Top = CustomTextBoxBootstrapIpPort.Top;

            CustomLabelDnsTimeout.Left = CustomLabelBootstrapIp.Left;
            CustomLabelDnsTimeout.Top = CustomTextBoxBootstrapIpPort.Bottom + (spaceV * 2);

            CustomNumericUpDownDnsTimeout.Left = CustomLabelDnsTimeout.Right + spaceH;
            CustomNumericUpDownDnsTimeout.Top = CustomLabelDnsTimeout.Top - 2;

            try
            {
                SplitContainerMain.Panel1MinSize = CustomNumericUpDownDnsTimeout.Bottom;
                SplitContainerMain.SplitterDistance = CustomNumericUpDownDnsTimeout.Bottom + (spaceV * 2);
            }
            catch (Exception) { }

            CustomRichTextBoxLog.Location = new Point(0, 0);

            Controllers.SetDarkControl(this);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FormDnsScanner FixScreenDpi: " + ex.Message);
        }
    }

    private void FormDnsScanner_Shown(object? sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(DNS))
        {
            IsSimpleTest = true;
            CustomRadioButtonDnsUrl.Checked = true;
            CustomRadioButtonDnssFromFiles.Checked = false;
            CustomRadioButtonDnssFromURLs.Checked = false;
            CustomRadioButtonCustomServers.Checked = false;
            CustomTextBoxDnsUrl.Text = DNS;
            CustomButtonScan_Click(null, null);
        }
    }

    private async void ReadSelectedFiles(List<string> selectedFiles)
    {
        try
        {
            string allContent = string.Empty;

            for (int n = 0; n < selectedFiles.Count; n++)
            {
                string file = selectedFiles[n];
                if (File.Exists(file))
                {
                    string content = await File.ReadAllTextAsync(file, new UTF8Encoding(false));
                    if (content.Length > 0)
                    {
                        allContent += content;
                        allContent += NL;
                    }
                }
            }

            ScanContent = allContent;
            string s = selectedFiles.Count > 1 ? "s" : string.Empty;
            CustomLabelDnssFromFiles.Text = $"{selectedFiles.Count} file{s} selected.";
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FormDnsScanner ReadSelectedFiles: " + ex.Message);
        }
    }

    private void ReadRdrList(List<FormMain.ReadDnsResult> rdrList)
    {
        try
        {
            string allContent = string.Empty;

            for (int n = 0; n < rdrList.Count; n++)
            {
                FormMain.ReadDnsResult rdr = rdrList[n];
                allContent += rdr.DNS;
                allContent += NL;
            }

            ScanContent = allContent;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FormDnsScanner ReadRdrList: " + ex.Message);
        }
    }

    private void CustomRadioButton_CheckedChanged(object sender, EventArgs e)
    {
        IsSimpleTest = false;
    }

    private void CustomRadioButtonDnssFromFiles_CheckedChanged(object sender, EventArgs e)
    {
        if (sender is CustomRadioButton crb && crb.Checked)
            ReadSelectedFiles(SelectedFilesList);
        IsSimpleTest = false;
    }

    private void CustomButtonDnssFromFiles_Click(object sender, EventArgs e)
    {
        try
        {
            // Browse
            using OpenFileDialog ofd = new();
            ofd.Filter = "DNS Servers|*.txt";
            ofd.Multiselect = true;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string[] files = ofd.FileNames;

                SelectedFilesList.Clear();
                SelectedFilesList.AddRange(files);

                ReadSelectedFiles(SelectedFilesList);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FormDnsScanner CustomButtonDnssFromFiles_Click: " + ex.Message);
        }
    }

    private void CustomButtonDnssFromURLs_Click(object sender, EventArgs e)
    {
        try
        {
            string urls = string.Empty;

            if (SelectedURLsList.Any())
            {
                for (int n = 0; n < SelectedURLsList.Count; n++)
                {
                    string url = SelectedURLsList[n].Trim();
                    if (!string.IsNullOrEmpty(url))
                        urls += $"{url}{NL}";
                }
            }

            urls = urls.Trim(Environment.NewLine.ToCharArray());

            string msg = "Extract and read DNS Servers from URLs.\n";
            msg += "Each line one URL. e.g.\n";
            msg += "          http://example.com/servers.txt\n";
            msg += "          https://google.com/dns.html\n";
            DialogResult dr = CustomInputBox.Show(this, ref urls, msg, true, "Extract DNS Servers", 300);
            if (dr == DialogResult.OK)
            {
                urls = urls.Trim(Environment.NewLine.ToCharArray());
                SelectedURLsList = urls.SplitToLines();
                SelectedURLsList = SelectedURLsList.RemoveDuplicates();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FormDnsScanner CustomButtonDnssFromURLs_Click: " + ex.Message);
        }
    }

    private void CustomButtonSmartDnsSelect_Click(object sender, EventArgs e)
    {
        try
        {
            string domains = string.Empty;

            if (DomainsToCheckForSmartDNS.Any())
            {
                for (int n = 0; n < DomainsToCheckForSmartDNS.Count; n++)
                {
                    string domain = DomainsToCheckForSmartDNS[n].Trim();
                    if (!string.IsNullOrEmpty(domain))
                        domains += $"{domain}{NL}";
                }
            }

            domains = domains.Trim(Environment.NewLine.ToCharArray());

            string msg = "Domains to check for SmartDNS support.\n";
            msg += "Each line one domain. e.g.\n";
            msg += "          example.com\n";
            msg += "          google.com\n";
            DialogResult dr = CustomInputBox.Show(this, ref domains, msg, true, "SmartDNS Check", 10, 10);
            if (dr == DialogResult.OK)
            {
                if (domains.ToLower().Contains("http://") || domains.ToLower().Contains("https://"))
                {
                    msg = "Only domains, without \"HTTP://\" or \"HTTPS://\"";
                    CustomMessageBox.Show(this, msg, "Only Domains", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                domains = domains.Trim(Environment.NewLine.ToCharArray());
                DomainsToCheckForSmartDNS = domains.SplitToLines();
                DomainsToCheckForSmartDNS = DomainsToCheckForSmartDNS.RemoveDuplicates();

                string s = DomainsToCheckForSmartDNS.Count > 1 ? "s" : string.Empty;
                CustomLabelSmartDnsStatus.Text = $"Contains {DomainsToCheckForSmartDNS.Count} domain{s}.";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FormDnsScanner CustomButtonSmartDnsSelect_Click: " + ex.Message);
        }
    }

    private async void CustomButtonScan_Click(object? sender, EventArgs? e)
    {
        try
        {
            if (!IsRunning)
            {
                // Start Scanning
                IsRunning = true;
                CustomButtonScan.Text = "Stop";
                CustomButtonExport.Enabled = false;

                Task taskCheck = Task.Run(async () => await StartScanAsync());

                await taskCheck.ContinueWith(async _ =>
                {
                    string msg = $"{NL}Scanner Task: {taskCheck.Status}{NL}";
                    CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue);

                    // Kill Server
                    await ProcessManager.KillProcessByPidAsync(PidDnsServer);

                    IsRunning = false;
                    Exit = false;
                    CustomButtonScan.Text = "Scan";
                    CustomButtonScan.Enabled = true;

                    if (ExportList.Any()) CustomButtonExport.Enabled = true;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                // Stop Scanning
                Exit = true;
                CustomButtonScan.Text = "Stopping...";
                CustomButtonScan.Enabled = false;
                // Kill Server
                await ProcessManager.KillProcessByPidAsync(PidDnsServer);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FormDnsScanner CustomButtonScan_Click: " + ex.Message);
        }
    }

    private async Task<bool> StartScanAsync()
    {
        try
        {
            // Clear Log On New Scan
            this.InvokeIt(() => CustomRichTextBoxLog.ResetText());

            bool isSimpleTest = IsSimpleTest;

            // Clear Export Data on new Check
            if (CustomCheckBoxClearExportData.Checked)
            {
                ExportList.Clear();
                ExportListPrivate.Clear();
            }

            // Get Bootstrap IP
            string bootstrapIpStr = "8.8.8.8";
            this.InvokeIt(() => bootstrapIpStr = CustomTextBoxBootstrapIpPort.Text);
            bootstrapIpStr = bootstrapIpStr.Trim();

            bool isBootstrapIpValid = NetworkTool.IsIPv4Valid(bootstrapIpStr, out IPAddress? bootstrapIP);
            if (!isBootstrapIpValid || bootstrapIP == null)
            {
                string msg = $"Bootstrap IP Is Not Valid.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return false;
            }

            // Get Bootstrap Port
            int bootstrapPort = 53;
            this.InvokeIt(() => bootstrapPort = decimal.ToInt32(CustomNumericUpDownBootstrapPort.Value));

            // Create ScanContent
            if (CustomRadioButtonDnsUrl.Checked)
                ScanContent = CustomTextBoxDnsUrl.Text;
            else if (CustomRadioButtonDnssFromFiles.Checked)
                ReadSelectedFiles(SelectedFilesList);
            else if (CustomRadioButtonDnssFromURLs.Checked)
            {
                List<string> dnsServers = new();
                List<string> selectedURLsList = SelectedURLsList.ToList();
                for (int n = 0; n < selectedURLsList.Count; n++)
                {
                    string url = selectedURLsList[n].Trim();
                    if (url.StartsWith("http://") || url.StartsWith("https://"))
                    {
                        string msg = $"Downloading And Extracting Servers From:{NL}";
                        msg += url + NL;
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));

                        List<string> servers = await FormMain.GetServersFromLinkAsync(url);
                        dnsServers.AddRange(servers);
                    }
                }
                ScanContent = dnsServers.ToString(NL);
            }
            else if (CustomRadioButtonCustomServers.Checked)
            {
                string msg = $"Reading Custom Servers...{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));

                FormMain.CheckRequest crC = new() { CheckMode = FormMain.CheckMode.CustomServers };
                List<FormMain.ReadDnsResult> rdrListC = await FormMain.ReadCustomServersXml(SecureDNS.CustomServersXmlPath, crC);
                ReadRdrList(rdrListC);
            }

            if (string.IsNullOrWhiteSpace(ScanContent))
            {
                string msg = "There is no server to check.";
                CustomMessageBox.Show(this, msg, "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            List<string> dnss = await FormMain.GetServersFromContentAsync(ScanContent);
            
            if (!isSimpleTest)
            {
                // Find a port for temp local dns server
                int port = 15053;
                bool isPortOpen = NetworkTool.IsPortOpen(port);
                if (isPortOpen)
                {
                    port = NetworkTool.GetNextPort(port);
                    isPortOpen = NetworkTool.IsPortOpen(port);
                    if (isPortOpen)
                    {
                        port = NetworkTool.GetNextPort(port);
                        isPortOpen = NetworkTool.IsPortOpen(port);
                        if (isPortOpen)
                        {
                            port = NetworkTool.GetNextPort(port);
                        }
                    }
                }

                // Find Uncensored dns servers
                string msg0 = $"Finding Uncensored DNS Server...{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg0, Color.DodgerBlue));

                FormMain.CheckRequest crB = new() { CheckMode = FormMain.CheckMode.BuiltIn };
                List<FormMain.ReadDnsResult> rdrList = await FormMain.ReadBuiltInServersAsync(crB, false, false);

                int maxServers = 5;
                int countServers = 0;
                string domainToCheck = "youtube.com";
                List<string> uncensoredDnss = new();
                CheckDns checkDns = new(false, false);
                var lists = rdrList.SplitToLists(5);
                for (int n = 0; n < lists.Count; n++)
                {
                    if (Exit) return false;
                    if (countServers >= maxServers) break;
                    List<FormMain.ReadDnsResult> list = lists[n];
                    await Parallel.ForEachAsync(list, async (rdr, cancellationToken) =>
                    {
                        if (Exit) return;
                        if (countServers >= maxServers) return;
                        CheckDns.CheckDnsResult cdr = await checkDns.CheckDnsAsync(domainToCheck, rdr.DNS, 3000, bootstrapIP, bootstrapPort);
                        if (cdr.IsDnsOnline)
                        {
                            uncensoredDnss.Add(rdr.DNS);
                            countServers++;
                        }
                    });
                    if (Exit) return false;
                }

                // Create a temp DNS Server
                msg0 = $"Creating A Temporary DNS Server On Port {port}...{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg0, Color.DodgerBlue));

                ProcessConsole dnsConsole = new();
                PidDnsServer = dnsConsole.Execute(SecureDNS.AgnosticServerPath, null, true, true, SecureDNS.CurrentPath);
                await Task.Delay(50);

                // Wait For DNS Server
                Task wait1 = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (Exit) break;
                        if (ProcessManager.FindProcessByPID(PidDnsServer)) break;
                        await Task.Delay(50);
                    }
                });
                try { await wait1.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

                if (!ProcessManager.FindProcessByPID(PidDnsServer))
                {
                    string msg = $"Couldn't Start DNS Server. Try Again.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                    await ProcessManager.KillProcessByPidAsync(PidDnsServer);
                    return false;
                }

                if (Exit) return false;

                // Send Set Profile
                await dnsConsole.SendCommandAsync("Profile DNS");

                // Send DNS Settings
                string dnsSettingsCmd = $"Setting -Port={port} -WorkingMode=Dns -MaxRequests=1000000 -DnsTimeoutSec=10 -KillOnCpuUsage=40";
                dnsSettingsCmd += $" -AllowInsecure=True -DNSs={uncensoredDnss.ToString(",")}";
                dnsSettingsCmd += $" -BootstrapIp={bootstrapIP} -BootstrapPort={bootstrapPort}";
                await dnsConsole.SendCommandAsync(dnsSettingsCmd);
                // Send Start Command
                await dnsConsole.SendCommandAsync("Start");

                string localDns = $"udp://{IPAddress.Loopback}:{port}";

                // Wait Until DNS Gets Online
                bool isLocalDnsServerOnline = false;
                Task wait2 = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (Exit) break;
                        if (!ProcessManager.FindProcessByPID(PidDnsServer)) break;

                        List<int> pids = await ProcessManager.GetProcessPidsByUsingPortAsync(port);
                        foreach (int pid in pids) if (pid != PidDnsServer) await ProcessManager.KillProcessByPidAsync(pid);

                        CheckDns.CheckDnsResult cdr = await checkDns.CheckDnsExternalAsync(domainToCheck, localDns, 2000);

                        if (cdr.IsDnsOnline)
                        {
                            isLocalDnsServerOnline = true;
                            break;
                        }
                        await Task.Delay(25);
                    }
                });
                try { await wait2.WaitAsync(TimeSpan.FromSeconds(20)); } catch (Exception) { }

                if (!isLocalDnsServerOnline)
                {
                    string msgFailed = "DNS Server Can't Get Online.";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgFailed + NL, Color.IndianRed));
                    await ProcessManager.KillProcessByPidAsync(PidDnsServer);
                    return false;
                }

                string msg1 = $"Generating Google Safe Search IPs...{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg1, Color.DodgerBlue));
                int generated1 = await DnsScanner.GenerateGoogleSafeSearchIpsAsync(localDns);
                int generated2 = await DnsScannerInsecureWithFilters.GenerateGoogleSafeSearchIpsAsync(localDns);
                if (generated1 < 1 || generated2 < 1)
                {
                    msg1 = $"Couldn't Generate Google Safe Search IPs.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg1, Color.IndianRed));
                }

                string msg2 = $"Generating Bing Safe Search IPs...{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg2, Color.DodgerBlue));
                generated1 = await DnsScanner.GenerateBingSafeSearchIpsAsync(localDns);
                generated2 = await DnsScannerInsecureWithFilters.GenerateBingSafeSearchIpsAsync(localDns);
                if (generated1 < 1 || generated2 < 1)
                {
                    msg2 = $"Couldn't Generate Bing Safe Search IPs.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg2, Color.IndianRed));
                }

                string msg3 = $"Generating Youtube Restrict IPs...{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg3, Color.DodgerBlue));
                generated1 = await DnsScanner.GenerateYoutubeRestrictIpsAsync(localDns);
                generated2 = await DnsScannerInsecureWithFilters.GenerateYoutubeRestrictIpsAsync(localDns);
                if (generated1 < 1 || generated2 < 1)
                {
                    msg3 = $"Couldn't Generate Youtube Restrict IPs.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg3, Color.IndianRed));
                }

                string msg4 = $"Generating Adult IPs...{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg4, Color.DodgerBlue));
                generated1 = await DnsScanner.GenerateAdultDomainIpsAsync(localDns);
                generated2 = await DnsScannerInsecureWithFilters.GenerateAdultDomainIpsAsync(localDns);
                if (generated1 < 1 || generated2 < 1)
                {
                    msg4 = $"Couldn't Generate Adult IPs.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg4, Color.IndianRed));
                }
            }

            // Get Company Data Content
            string? companyDataContent = await ResourceTool.GetResourceTextFileAsync("SecureDNSClient.HostToCompany.txt", Assembly.GetExecutingAssembly()); // Load from Embedded Resource

            string msg5 = $"Scanning...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg5, Color.DodgerBlue));

            if (dnss.Count > 0)
            {
                for (int n = 0; n < dnss.Count; n++)
                {
                    if (Exit) return false;

                    string dns = dnss[n].Trim();

                    if (FormMain.IsDnsProtocolSupported(dns))
                    {
                        await ScanDnsAsync(dns, companyDataContent, bootstrapIP, bootstrapPort, isSimpleTest);
                    }

                    // Percent
                    int percent = n * 100 / dnss.Count;
                    percent = n == dnss.Count - 1 ? 100 : percent;
                    this.InvokeIt(() => CustomButtonScan.Text = $"Stop ({percent}%)");
                }
            }

            string msg6 = $"===== End of Scan ====={NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg6, Color.DodgerBlue));

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FormDnsScanner StartScanAsync: " + ex.Message);
            return false;
        }
    }

    private async Task ScanDnsAsync(string dns, string? companyDataContent, IPAddress bootstrapIP, int bootstrapPort, bool isSimpleTest)
    {
        await Task.Run(async () =>
        {
            try
            {
                // Export Data
                string secureAddress = dns;
                bool secureSuccess = false;
                int secureLatency = -1;
                string insecureAddress = string.Empty;
                bool insecureSuccess = false;
                int insecureLatency = -1;
                bool isGoogleSafeSearchActive = false;
                bool isBingSafeSearchActive = false;
                bool isYoutubeRestrictActive = false;
                bool isAdultContentFilter = false;

                string domain = isSimpleTest ? "youtube.com" : "google.com";
                int timeoutSec = 5;
                this.InvokeIt(() => timeoutSec = decimal.ToInt32(CustomNumericUpDownDnsTimeout.Value));
                int timeoutMS = timeoutSec * 1000;

                DnsReader dnsReader = new(dns, companyDataContent);
                if (dnsReader.Protocol == DnsEnums.DnsProtocol.AnonymizedDNSCryptRelay) return;
                if (dnsReader.Protocol == DnsEnums.DnsProtocol.ObliviousDohRelay) return;
                if (dnsReader.Protocol == DnsEnums.DnsProtocol.Unknown) return;

                // Add New Line
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(NL));

                // DNS Address
                string r1 = $"DNS Address: ";
                string r2 = $"{dnsReader.DnsWithRelay}{NL}";

                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.Orange));

                // Protocol
                if (!string.IsNullOrEmpty(dnsReader.ProtocolName))
                {
                    r1 = $"Protocol: ";
                    r2 = $"{dnsReader.ProtocolName}{NL}";

                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.Orange));
                }

                // Host
                if (!string.IsNullOrEmpty(dnsReader.Host))
                {
                    r1 = $"Host: ";
                    r2 = $"{dnsReader.Host}{NL}";

                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.Orange));
                }

                // IP
                r1 = $"IP: ";
                IPAddress? ip = null;
                if (!string.IsNullOrEmpty(dnsReader.Host))
                    ip = await GetIP.GetIpFromDnsAddressAsync(dnsReader.Host, $"udp://{IPAddress.Loopback}:53", true, timeoutSec, false, bootstrapIP, bootstrapPort);
                if (ip == null || ip.Equals(IPAddress.None))
                    ip = dnsReader.IP;
                r2 = $"{ip}{NL}";

                if (ip != null && !ip.Equals(IPAddress.None))
                {
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.Orange));
                }

                // Port
                r1 = $"Port: ";
                r2 = $"{dnsReader.Port}{NL}";

                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.Orange));

                // Path
                if (!string.IsNullOrEmpty(dnsReader.Path) && dnsReader.Protocol == DnsEnums.DnsProtocol.DoH)
                {
                    r1 = $"Path: ";
                    r2 = $"{dnsReader.Path}{NL}";

                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.Orange));
                }

                // Stamp Properties
                if (dnsReader.IsDnsCryptStamp)
                {
                    r1 = $"Stamp Properties: {NL}";
                    r2 = "Is DnsSec: ";
                    string r3 = $"{dnsReader.StampReader.IsDnsSec}, ";
                    string r4 = "Is No Filter: ";
                    string r5 = $"{dnsReader.StampReader.IsNoFilter}, ";
                    string r6 = "Is No Log: ";
                    string r7 = $"{dnsReader.StampReader.IsNoLog}{NL}";

                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r3, Color.Orange));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r4, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r5, Color.Orange));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r6, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r7, Color.Orange));
                }

                // Company Name
                if (!string.IsNullOrEmpty(dnsReader.CompanyName))
                {
                    r1 = $"Company Name: ";
                    r2 = $"{dnsReader.CompanyName}{NL}";

                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.Orange));
                }

                // Query
                r1 = $"Getting Response For: ";
                r2 = $"{domain}{NL}";

                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.Orange));

                // Check Online Secure
                if (isSimpleTest) DnsScanner = new(false, false);
                CheckDns.CheckDnsResult cdrSecure = await DnsScanner.CheckDnsAsync(domain, dns, timeoutMS, bootstrapIP, bootstrapPort);

                secureSuccess = cdrSecure.IsDnsOnline;
                secureLatency = cdrSecure.DnsLatency;

                if (cdrSecure.IsDnsOnline)
                {
                    r1 = $"Secure Status: ";
                    r2 = $"Online{NL}";

                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.MediumSeaGreen));

                    r1 = $"Latency: ";
                    r2 = $"{cdrSecure.DnsLatency} ms{NL}";

                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.Orange));
                }
                else
                {
                    r1 = $"Secure Status: ";
                    r2 = $"Offline{NL}";

                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.IndianRed));
                }

                // Start Check Insecure
                CheckDns insecureCheck;
                CheckDns.CheckDnsResult cdrInsecure = new();
                if (cdrSecure.IsDnsOnline)
                    insecureCheck = DnsScannerInsecureNoFilters;
                else
                    insecureCheck = DnsScannerInsecureWithFilters;

                // Generate Insecure Address
                insecureAddress = dns;
                if (ip != null && !ip.Equals(IPAddress.None))
                {
                    NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(dns, dnsReader.Port);
                    string scheme = urid.Scheme;

                    if (dnsReader.IsDnsCryptStamp && dnsReader.Protocol == DnsEnums.DnsProtocol.DoT)
                    {
                        scheme = "tls://";
                        insecureAddress = $"{scheme}{ip}:{dnsReader.Port}{dnsReader.Path}";
                    }
                    else if (dnsReader.IsDnsCryptStamp && dnsReader.Protocol == DnsEnums.DnsProtocol.DoH)
                    {
                        scheme = "https://";
                        insecureAddress = $"{scheme}{ip}:{dnsReader.Port}{dnsReader.Path}";
                    }
                    else if (dnsReader.IsDnsCryptStamp && dnsReader.Protocol == DnsEnums.DnsProtocol.DoQ)
                    {
                        scheme = "quic://";
                        insecureAddress = $"{scheme}{ip}:{dnsReader.Port}{dnsReader.Path}";
                    }
                }

                // Check Insecure Only For These Protocols
                if (dnsReader.Protocol == DnsEnums.DnsProtocol.DoT ||
                    dnsReader.Protocol == DnsEnums.DnsProtocol.DoH ||
                    dnsReader.Protocol == DnsEnums.DnsProtocol.ObliviousDohTarget ||
                    dnsReader.Protocol == DnsEnums.DnsProtocol.DnsCrypt ||
                    dnsReader.Protocol == DnsEnums.DnsProtocol.AnonymizedDNSCrypt)
                {
                    if (CustomCheckBoxCheckInsecure.Checked && !cdrSecure.IsDnsOnline)
                        cdrInsecure = await insecureCheck.CheckDnsAsync(domain, dns, timeoutMS, bootstrapIP, bootstrapPort);
                }

                insecureSuccess = cdrInsecure.IsDnsOnline;
                insecureLatency = cdrInsecure.DnsLatency;

                if (cdrInsecure.IsDnsOnline)
                {
                    r1 = $"Insecure Status: ";
                    r2 = $"Online{NL}";

                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.MediumSeaGreen));

                    r1 = $"Latency: ";
                    r2 = $"{cdrInsecure.DnsLatency} ms{NL}";

                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.Orange));

                    r1 = $"Insecure DNS Address: ";
                    r2 = $"{insecureAddress}{NL}";

                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.Orange));
                }
                else
                {
                    if (CustomCheckBoxCheckInsecure.Checked && !cdrSecure.IsDnsOnline)
                    {
                        string r40 = $"Insecure Status: ";
                        string r41 = $"Offline{NL}";

                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r40, Color.LightGray));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r41, Color.IndianRed));
                    }
                }

                // Filters
                CheckDns? checkFilters = null;
                CheckDns.CheckDnsResult? checkFiltersCdr = null;

                if (cdrSecure.IsDnsOnline)
                {
                    checkFilters = DnsScanner;
                    checkFiltersCdr = cdrSecure;
                }

                if (!cdrSecure.IsDnsOnline && cdrInsecure.IsDnsOnline)
                {
                    checkFilters = insecureCheck;
                    checkFiltersCdr = cdrInsecure;
                }

                if (isSimpleTest)
                {
                    checkFilters = null;
                    checkFiltersCdr = null;
                }

                if (checkFilters != null && checkFiltersCdr != null)
                {
                    isGoogleSafeSearchActive = checkFiltersCdr.IsGoogleSafeSearchEnabled;
                    isBingSafeSearchActive = checkFiltersCdr.IsBingSafeSearchEnabled;
                    isYoutubeRestrictActive = checkFiltersCdr.IsYoutubeRestricted;
                    isAdultContentFilter = checkFiltersCdr.IsAdultFilter;

                    // Google Safe Search
                    r1 = $"Is Google Safe Search Active: ";
                    r2 = $"{isGoogleSafeSearchActive}{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.DodgerBlue));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.Orange));

                    // Bing Safe Search
                    r1 = $"Is Bing Safe Search Active: ";
                    r2 = $"{isBingSafeSearchActive}{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.DodgerBlue));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.Orange));

                    // Youtube Restrict
                    r1 = $"Is Youtube Restriction Active: ";
                    r2 = $"{isYoutubeRestrictActive}{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.DodgerBlue));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.Orange));

                    // Adult Content
                    r1 = $"Is Adult Content Filter: ";
                    r2 = $"{isAdultContentFilter}{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.DodgerBlue));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.Orange));

                    // Smart DNS
                    if (CustomCheckBoxSmartDNS.Checked && DomainsToCheckForSmartDNS.Any())
                    {
                        for (int n = 0; n < DomainsToCheckForSmartDNS.Count; n++)
                        {
                            if (Exit) break;

                            string website = DomainsToCheckForSmartDNS[n].Trim();
                            if (!string.IsNullOrEmpty(website))
                            {
                                bool isSmart = await checkFilters.CheckAsSmartDnsAsync($"tcp://{bootstrapIP}:{bootstrapPort}", website);
                                if (!isSmart)
                                    isSmart = await checkFilters.CheckAsSmartDnsAsync($"udp://{IPAddress.Loopback}:53", website);
                                Color color = isSmart ? Color.MediumSeaGreen : Color.DodgerBlue;
                                r1 = "Act as SmartDNS for \"";
                                r2 = "\": ";

                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(website, Color.Orange));
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.LightGray));
                                string maybe = isSmart ? "Maybe " : string.Empty;
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"{maybe}{isSmart}{NL}", color));
                            }
                        }
                    }
                }

                string end = $"--------------------------------------------------------------------------------{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(end, Color.LightGray));

                // Store Export Data
                if (!isSimpleTest)
                {
                    string dedup = $"{secureAddress}{secureSuccess}{insecureAddress}{insecureSuccess}";
                    if (!ExportListPrivate.IsContain(dedup))
                    {
                        ExportListPrivate.Add(dedup);
                        Tuple<bool, bool, bool, bool> filters = new(isGoogleSafeSearchActive, isBingSafeSearchActive, isYoutubeRestrictActive, isAdultContentFilter);
                        Tuple<string, string, bool, int, bool, int, Tuple<bool, bool, bool, bool>> tuple = new(secureAddress, insecureAddress, secureSuccess, secureLatency, insecureSuccess, insecureLatency, filters);
                        ExportList.Add(tuple);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FormDnsScanner ScanDnsAsync: " + ex.Message);
            }
        });
    }

    private void CustomButtonExport_Click(object sender, EventArgs e)
    {
        try
        {
            FormDnsScannerExport formDnsScannerExport = new(ExportList);
            formDnsScannerExport.StartPosition = FormStartPosition.CenterParent;
            formDnsScannerExport.FormClosing += (s, e) => { formDnsScannerExport.Dispose(); };
            formDnsScannerExport.ShowDialog(this);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FormDnsScanner CustomButtonExport_Click: " + ex.Message);
        }
    }

    private async void FormDnsScanner_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!IsExiting)
        {
            e.Cancel = true;
            IsExiting = true;

            Exit = true;
            this.InvokeIt(() =>
            {
                CustomButtonScan.Text = "Exiting...";
                CustomButtonScan.Enabled = false;
            });

            SelectedFilesList.Clear();
            DomainsToCheckForSmartDNS.Clear();

            // Kill Server
            await ProcessManager.KillProcessByPidAsync(PidDnsServer);

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

            // Wait For Cancel
            await Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(100);
                    if (!IsRunning) break;
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