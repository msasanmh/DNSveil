﻿using CustomControls;
using MsmhToolsClass;
using MsmhToolsClass.DnsTool;
using MsmhToolsWinFormsClass;
using MsmhToolsWinFormsClass.Themes;
using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;

namespace SecureDNSClient
{
    public partial class FormDnsScanner : Form
    {
        // Settings XML path
        private static readonly string SettingsXmlPath = SecureDNS.SettingsXmlDnsScanner;
        private readonly Settings AppSettings;
        private readonly string NL = Environment.NewLine;
        private string ScanContent = string.Empty;
        private readonly List<string> SelectedFilesList = new();
        private static List<string> DomainsToCheckForSmartDNS = new();
        private readonly CheckDns DnsScanner = new(true, ProcessPriorityClass.Normal);
        private readonly string DNS = string.Empty;
        private readonly List<Tuple<string, string, bool, int, bool, int, Tuple<bool, bool>>> ExportList = new();
        private readonly List<string> ExportListPrivate = new();
        private bool IsRunning = false;
        private bool Exit = false;

        public FormDnsScanner(string? dns = null)
        {
            // Fix Screed DPI
            ScreenDPI.FixDpiBeforeInitializeComponent(this);
            InitializeComponent();
            ScreenDPI.FixDpiAfterInitializeComponent(this);

            // Load Theme
            Theme.LoadTheme(this, Theme.Themes.Dark);

            // Initialize and load Settings
            if (File.Exists(SettingsXmlPath) && XmlTool.IsValidXMLFile(SettingsXmlPath))
                AppSettings = new(this, SettingsXmlPath);
            else
                AppSettings = new(this);

            SelectedFilesList.Clear();
            DomainsToCheckForSmartDNS.Clear();
            CustomButtonExport.Enabled = false;

            if (!string.IsNullOrEmpty(dns)) DNS = dns;

            Shown += FormDnsScanner_Shown;
        }

        private void FormDnsScanner_Shown(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DNS))
            {
                CustomRadioButtonDnsUrl.Checked = true;
                CustomRadioButtonDnsBrowse.Checked = false;
                CustomRadioButtonCustomServers.Checked = false;
                CustomTextBoxDnsUrl.Text = DNS;
                CustomButtonScan_Click(null, null);
            }
        }

        private void CustomRadioButtonDnsBrowse_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is CustomRadioButton crb && crb.Checked)
                ReadSelectedFiles(SelectedFilesList);
        }

        private void CustomButtonDnsBrowse_Click(object sender, EventArgs e)
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

        private async void ReadSelectedFiles(List<string> selectedFiles)
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
            CustomLabelDnsBrowse.Text = $"{selectedFiles.Count} file{s} selected.";

        }

        private void CustomButtonSmartDnsSelect_Click(object sender, EventArgs e)
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

        private async void CustomButtonScan_Click(object? sender, EventArgs? e)
        {
            if (!IsRunning)
            {
                // Start Scanning
                IsRunning = true;
                CustomButtonScan.Text = "Stop";
                CustomButtonExport.Enabled = false;

                Task taskCheck = Task.Run(async () => await StartScan());

                await taskCheck.ContinueWith(_ =>
                {
                    string msg = $"{NL}Scanner Task: {taskCheck.Status}{NL}";
                    CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue);

                    IsRunning = false;
                    Exit = false;
                    CustomButtonScan.Text = "Scan";
                    CustomButtonScan.Enabled = true;

                    if (ExportList.Any())
                        CustomButtonExport.Enabled = true;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                // Stop Scanning
                Exit = true;
                CustomButtonScan.Text = "Stopping...";
                CustomButtonScan.Enabled = false;
            }
        }

        private async Task<bool> StartScan()
        {
            // Clear Log On New Scan
            this.InvokeIt(() => CustomRichTextBoxLog.ResetText());

            if (!FormMain.IsDNSConnected)
            {
                string msg = $"SDC must be online.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return false;
            }

            if (FormMain.IsCheckingStarted)
            {
                string msg = $"Cannot start operation while SDC is checking servers.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return false;
            }

            // Clear Export Data on new Check
            if (CustomCheckBoxClearExportData.Checked)
            {
                ExportList.Clear();
                ExportListPrivate.Clear();
            }

            // Get Bootstrap IP
            string bootstrapIp = "8.8.8.8";
            this.InvokeIt(() => bootstrapIp = CustomTextBoxBootstrapIpPort.Text);
            bootstrapIp = bootstrapIp.Trim();

            if (!NetworkTool.IsIPv4Valid(bootstrapIp, out _))
            {
                string msg = $"Bootstrap IP is not valid.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return false;
            }

            // Get Bootstrap Port
            int bootstrapPort = 53;
            this.InvokeIt(() => bootstrapPort = decimal.ToInt32(CustomNumericUpDownBootstrapPort.Value));

            if (CustomRadioButtonDnsUrl.Checked)
                ScanContent = CustomTextBoxDnsUrl.Text;
            else if (CustomRadioButtonDnsBrowse.Checked)
                ReadSelectedFiles(SelectedFilesList);
            else if (CustomRadioButtonCustomServers.Checked)
            {

                string msg = $"Reading Custom Servers...{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));

                ScanContent = await FormMain.ReadCustomServersXml(SecureDNS.CustomServersXmlPath);
            }

            string localDns = $"{IPAddress.Loopback}";

            string msg1 = $"Generating Google Safe Search IPs...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg1, Color.DodgerBlue));

            DnsScanner.GenerateGoogleSafeSearchIps(localDns);

            string msg2 = $"Generating Adult IPs...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg2, Color.DodgerBlue));

            DnsScanner.GenerateAdultDomainIps(localDns);

            // Get Company Data Content
            string? companyDataContent = await ResourceTool.GetResourceTextFileAsync("SecureDNSClient.HostToCompany.txt", Assembly.GetExecutingAssembly()); // Load from Embedded Resource

            string msg3 = $"Scanning...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg3, Color.DodgerBlue));

            if (ScanContent.Length > 0)
            {
                List<string> lines = ScanContent.SplitToLines();
                for (int n = 0; n < lines.Count; n++)
                {
                    if (Exit) return false;

                    string dns = lines[n];

                    if (FormMain.IsDnsProtocolSupported(dns))
                    {
                        await ScanDns(dns, bootstrapIp, bootstrapPort, companyDataContent);
                    }

                    // Percent
                    int percent = n * 100 / lines.Count;
                    percent = n == lines.Count - 1 ? 100 : percent;
                    this.InvokeIt(() => CustomButtonScan.Text = $"Stop ({percent}%)");
                }
            }

            string msg4 = $"===== End of Scan ====={NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg4, Color.DodgerBlue));

            return true;
        }

        private async Task ScanDns(string dns, string bootstrapIp, int bootstrapPort, string? companyDataContent)
        {
            await Task.Run(async () =>
            {
                // Export Data
                string secureAddress = dns;
                bool secureSuccess = false;
                int secureLatency = -1;
                string insecureAddress = string.Empty;
                bool insecureSuccess = false;
                int insecureLatency = -1;
                bool isGoogleSafeSearchActive = false;
                bool isAdultContentFilter = false;

                int timeoutMS = 5000;
                string domain = "youtube.com";
                this.InvokeIt(() => timeoutMS = decimal.ToInt32(CustomNumericUpDownDnsTimeout.Value * 1000));

                int localPort = 5390;

                // Add New Line
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(NL));

                bool isPortOpen = NetworkTool.IsPortOpen(IPAddress.Loopback.ToString(), localPort, 3);
                if (isPortOpen)
                {
                    localPort = NetworkTool.GetNextPort(localPort);
                    isPortOpen = NetworkTool.IsPortOpen(IPAddress.Loopback.ToString(), localPort, 3);
                    if (isPortOpen)
                    {
                        localPort = NetworkTool.GetNextPort(localPort);
                        string existingProcessName = ProcessManager.GetProcessNameByListeningPort(localPort);
                        existingProcessName = existingProcessName == string.Empty ? "Unknown" : existingProcessName;
                        string msg = $"Port {localPort} is occupied by \"{existingProcessName}\". You need to resolve the conflict.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                        return;
                    }
                }

                DnsReader dnsReader = new(dns, companyDataContent);

                // DNS Address
                string r1 = $"DNS Address: ";
                string r2 = $"{dnsReader.Dns}{NL}";

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
                string? ip = dnsReader.IP;
                if (string.IsNullOrEmpty(ip) || NetworkTool.IsLocalIP(ip))
                {
                    if (!string.IsNullOrEmpty(dnsReader.Host))
                        ip = await GetIP.GetIpFromPlainDNS(dnsReader.Host, IPAddress.Loopback.ToString(), 53, timeoutMS * 2);
                }
                r2 = $"{ip}{NL}";

                if (!string.IsNullOrEmpty(ip))
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
                if (!string.IsNullOrEmpty(dnsReader.Path) && dnsReader.Protocol == DnsReader.DnsProtocol.DoH)
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
                    string r3 = $"{dnsReader.StampProperties.IsDnsSec}, ";
                    string r4 = "Is No Filter: ";
                    string r5 = $"{dnsReader.StampProperties.IsNoFilter}, ";
                    string r6 = "Is No Log: ";
                    string r7 = $"{dnsReader.StampProperties.IsNoLog}{NL}";

                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r3, Color.Orange));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r4, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r5, Color.Orange));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r6, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r7, Color.Orange));
                }

                // Company Name
                r1 = $"Company Name: ";
                r2 = $"{dnsReader.CompanyName}{NL}";

                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.Orange));

                // Check Online Secure
                await DnsScanner.CheckDnsAsync(false, domain, dns, timeoutMS, localPort, bootstrapIp, bootstrapPort);

                secureSuccess = DnsScanner.IsDnsOnline;
                secureLatency = DnsScanner.DnsLatency;

                if (DnsScanner.IsDnsOnline)
                {
                    r1 = $"Secure Status: ";
                    r2 = $"Online{NL}";

                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.MediumSeaGreen));

                    r1 = $"Latency: ";
                    r2 = $"{DnsScanner.DnsLatency} ms{NL}";

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
                if (DnsScanner.IsDnsOnline)
                    insecureCheck = new(false, ProcessPriorityClass.Normal);
                else
                    insecureCheck = new(true, ProcessPriorityClass.Normal);
                insecureCheck.GenerateGoogleSafeSearchIps(IPAddress.Loopback.ToString());
                insecureCheck.GenerateAdultDomainIps(IPAddress.Loopback.ToString());

                // Generate Insecure Address
                insecureAddress = dns;
                if (!string.IsNullOrEmpty(ip))
                {
                    NetworkTool.GetUrlDetails(dns, dnsReader.Port, out string scheme, out _, out _, out _, out _);
                    if (dnsReader.IsDnsCryptStamp && dnsReader.Protocol == DnsReader.DnsProtocol.DoH) scheme = "https://";
                    if (dnsReader.IsDnsCryptStamp && dnsReader.Protocol == DnsReader.DnsProtocol.DoT) scheme = "tls://";
                    if (dnsReader.IsDnsCryptStamp && dnsReader.Protocol == DnsReader.DnsProtocol.DoQ) scheme = "quic://";

                    dns = $"{scheme}{ip}:{dnsReader.Port}{dnsReader.Path}";
                    insecureAddress = dns;
                }

                if (CustomCheckBoxCheckInsecure.Checked && !DnsScanner.IsDnsOnline)
                    await insecureCheck.CheckDnsAsync(true, domain, dns, timeoutMS, localPort, bootstrapIp, bootstrapPort);

                insecureSuccess = insecureCheck.IsDnsOnline;
                insecureLatency = insecureCheck.DnsLatency;

                if (insecureCheck.IsDnsOnline)
                {
                    r1 = $"Insecure Status: ";
                    r2 = $"Online{NL}";

                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.MediumSeaGreen));

                    r1 = $"Latency: ";
                    r2 = $"{insecureCheck.DnsLatency} ms{NL}";

                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.Orange));

                    r1 = $"Insecure DNS Address: ";
                    r2 = $"{insecureAddress}{NL}";

                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.Orange));
                }
                else
                {
                    if (CustomCheckBoxCheckInsecure.Checked && !DnsScanner.IsDnsOnline)
                    {
                        string r40 = $"Insecure Status: ";
                        string r41 = $"Offline{NL}";

                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r40, Color.LightGray));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r41, Color.IndianRed));
                    }
                }

                // Filters
                CheckDns? checkFilters = null;

                if (DnsScanner.IsDnsOnline)
                    checkFilters = DnsScanner;

                if (!DnsScanner.IsDnsOnline && insecureCheck.IsDnsOnline)
                    checkFilters = insecureCheck;

                if (checkFilters != null)
                {
                    isGoogleSafeSearchActive = checkFilters.IsSafeSearch;
                    isAdultContentFilter = checkFilters.IsAdultFilter;

                    // Google Safe Search
                    r1 = $"Is Google Safe Search Active: ";
                    r2 = $"{checkFilters.IsSafeSearch}{NL}";

                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r2, Color.Orange));

                    // Adult Content
                    r1 = $"Is Adult Content Filter: ";
                    r2 = $"{checkFilters.IsAdultFilter}{NL}";

                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(r1, Color.LightGray));
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
                                bool isSmart = checkFilters.CheckAsSmartDns(IPAddress.Loopback.ToString(), website);
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
                if (!ExportListPrivate.IsContain(secureAddress))
                {
                    ExportListPrivate.Add(secureAddress);
                    Tuple<bool, bool> filters = new(isGoogleSafeSearchActive, isAdultContentFilter);
                    Tuple<string, string, bool, int, bool, int, Tuple<bool, bool>> tuple = new(secureAddress, insecureAddress, secureSuccess, secureLatency, insecureSuccess, insecureLatency, filters);
                    ExportList.Add(tuple);
                }
            });
        }

        private void CustomButtonExport_Click(object sender, EventArgs e)
        {
            FormDnsScannerExport formDnsScannerExport = new(ExportList);
            formDnsScannerExport.StartPosition = FormStartPosition.CenterParent;
            formDnsScannerExport.FormClosing += (s, e) => { formDnsScannerExport.Dispose(); };
            formDnsScannerExport.ShowDialog(this);
        }

        private async void FormDnsScanner_FormClosing(object sender, FormClosingEventArgs e)
        {
            Exit = true;

            SelectedFilesList.Clear();
            DomainsToCheckForSmartDNS.Clear();

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
