using System.Net;
using System.Diagnostics;
using CustomControls;
using System.Reflection;
using System.Text;
using System.IO.Compression;
using System.Globalization;
using Microsoft.Win32;
using MsmhToolsClass;
using MsmhToolsClass.HTTPProxyServer;
using MsmhToolsWinFormsClass;
using MsmhToolsWinFormsClass.Themes;
// https://github.com/msasanmh/SecureDNSClient

namespace SecureDNSClient
{
    public partial class FormMain : Form
    {
        private FormWindowState LastWindowState;
        private static readonly CustomLabel LabelMoving = new();
        public List<Tuple<long, string>> WorkingDnsList = new();
        public List<string> SavedDnsList = new();
        public List<string> SavedEncodedDnsList = new();
        private List<string> WorkingDnsListToFile = new();
        private List<Tuple<long, string>> WorkingDnsAndLatencyListToFile = new();
        public static ProcessMonitor MonitorProcess { get; set; } = new();
        private bool Once { get; set; } = true;
        private bool IsCheckingForUpdate { get; set; } = false;
        private int NumberOfWorkingServers { get; set; } = 0;
        public static bool IsCheckingStarted { get; set; } = false;
        private static bool StopChecking { get; set; } = false;
        private bool IsConnecting { get; set; } = false;
        private bool IsDisconnecting { get; set; } = false;
        private bool IsConnected { get; set; } = false;
        public static bool IsDNSConnected { get; set; } = false;
        private int LocalDnsLatency { get; set; } = -1;
        public static bool IsDoHConnected { get; set; } = false;
        private int LocalDohLatency { get; set; } = -1;
        public static int ConnectedDohPort { get; set; } = 443; // as default
        private bool IsDNSSetting { get; set; } = false;
        private bool IsDNSUnsetting { get; set; } = false;
        private bool IsDNSSet { get; set; } = false;
        private bool IsDPIActive { get; set; } = false;
        private bool IsGoodbyeDPIBasicActive { get; set; } = false;
        private bool IsGoodbyeDPIAdvancedActive { get; set; } = false;
        private bool ConnectAllClicked { get; set; } = false;
        public static IPAddress? LocalIP { get; set; } = IPAddress.Loopback; // as default
        public Settings AppSettings { get; set; }
        private readonly ToolStripMenuItem ToolStripMenuItemIcon = new();
        private bool AudioAlertOnline = true;
        private bool AudioAlertOffline = false;
        private bool AudioAlertRequestsExceeded = false;
        private readonly Stopwatch StopWatchCheckDPIWorks = new();
        private readonly Stopwatch StopWatchAudioAlertDelay = new();
        private string TheDll = string.Empty;
        private static readonly string NL = Environment.NewLine;
        private int LogHeight;
        private bool IsExiting = false;

        // PIDs
        public static int PIDDNSProxy { get; set; } = -1;
        public static int PIDDNSCrypt { get; set; } = -1;
        private static int PIDGoodbyeDPIBasic { get; set; } = -1;
        private static int PIDGoodbyeDPIAdvanced { get; set; } = -1;

        // Camouflage
        private HTTPProxyServer CamouflageHttpProxyServer { get; set; } = new();
        private CamouflageDNSServer? CamouflageDNSServer { get; set; }
        private bool IsBypassHttpProxyActive { get; set; } = false;
        private bool IsBypassDNSActive { get; set; } = false;
        private static int PIDDNSCryptBypass { get; set; } = -1;
        private static int PIDDNSProxyBypass { get; set; } = -1;
        private static int PIDGoodbyeDPIBypass { get; set; } = -1;

        // HTTP Proxy
        private static int PIDHttpProxy { get; set; } = -1;
        private Process? HttpProxyProcess { get; set; }
        private bool IsHttpProxyActivated { get; set; } = false;
        private bool IsHttpProxyActivating { get; set; } = false;
        private bool IsHttpProxyDeactivating { get; set; } = false;
        public static bool IsHttpProxyRunning { get; set; } = false;
        public static int HttpProxyPort { get; set; } = -1;
        private int HttpProxyRequests { get; set; } = 0;
        private int HttpProxyMaxRequests { get; set; } = 250;
        private bool IsHttpProxyDpiBypassActive { get; set; } = false;
        private HTTPProxyServer.Program.DPIBypass.Mode ProxyStaticDPIBypassMode = HTTPProxyServer.Program.DPIBypass.Mode.Disable;
        private HTTPProxyServer.Program.DPIBypass.Mode ProxyDPIBypassMode = HTTPProxyServer.Program.DPIBypass.Mode.Disable;
        private HTTPProxyServer.Program.UpStreamProxy.Mode ProxyUpStreamMode = HTTPProxyServer.Program.UpStreamProxy.Mode.Disable;
        private HTTPProxyServer.Program.Dns.Mode ProxyDNSMode = HTTPProxyServer.Program.Dns.Mode.Disable;
        private HTTPProxyServer.Program.FakeDns.Mode ProxyFakeDnsMode = HTTPProxyServer.Program.FakeDns.Mode.Disable;
        private HTTPProxyServer.Program.BlackWhiteList.Mode ProxyBWListMode = HTTPProxyServer.Program.BlackWhiteList.Mode.Disable;
        private HTTPProxyServer.Program.DontBypass.Mode DontBypassMode = HTTPProxyServer.Program.DontBypass.Mode.Disable;
        private bool IsHttpProxySet { get; set; } = false;
        private static bool UpdateHttpProxyBools { get; set; } = true;

        // Fake Proxy
        private static int PIDFakeHttpProxy { get; set; } = -1;
        private Process? FakeHttpProxyProcess { get; set; }

        public FormMain()
        {
            InitializeComponent();

            // Invariant Culture
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            // Update NICs
            SecureDNS.UpdateNICs(CustomComboBoxNICs);

            // Startup Defaults
            string productName = Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductName ?? "SDC - Secure DNS Client";
            string productVersion = Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductVersion ?? "0.0.0";
            Text = $"{productName} v{productVersion}";
            CustomButtonSetDNS.Enabled = false;
            CustomButtonSetProxy.Enabled = false;
            CustomTextBoxHTTPProxy.Enabled = false;
            DefaultSettings();

            // Set NotifyIcon Text
            NotifyIconMain.Text = Text;

            // Create User Dir if not Exist
            FileDirectory.CreateEmptyDirectory(SecureDNS.UserDataDirPath);

            // Move User Data to the new location
            MoveToNewLocation();

            // Add Tooltips
            string msgCheckInParallel = "a) Don't use parallel on slow network.";
            msgCheckInParallel += $"{NL}b) Parallel doesn't support bootstrap.";
            msgCheckInParallel += $"{NL}c) Parallel doesn't support insecure mode.";
            CustomCheckBoxCheckInParallel.SetToolTip("Info", msgCheckInParallel);

            // Add Tooltips to advanced DPI
            string msgP = "Block passive DPI.";
            CustomCheckBoxDPIAdvP.SetToolTip("Info", msgP);
            string msgR = "Replace Host with hoSt.";
            CustomCheckBoxDPIAdvR.SetToolTip("Info", msgR);
            string msgS = "Remove space between host header and its value.";
            CustomCheckBoxDPIAdvS.SetToolTip("Info", msgS);
            string msgM = "Mix Host header case (test.com -> tEsT.cOm).";
            CustomCheckBoxDPIAdvM.SetToolTip("Info", msgM);
            string msgF = "Set HTTP fragmentation to value";
            CustomCheckBoxDPIAdvF.SetToolTip("Info", msgF);
            string msgK = "Enable HTTP persistent (keep-alive) fragmentation and set it to value.";
            CustomCheckBoxDPIAdvK.SetToolTip("Info", msgK);
            string msgN = "Do not wait for first segment ACK when -k is enabled.";
            CustomCheckBoxDPIAdvN.SetToolTip("Info", msgN);
            string msgE = "Set HTTPS fragmentation to value.";
            CustomCheckBoxDPIAdvE.SetToolTip("Info", msgE);
            string msgA = "Additional space between Method and Request-URI (enables -s, may break sites).";
            CustomCheckBoxDPIAdvA.SetToolTip("Info", msgA);
            string msgW = "Try to find and parse HTTP traffic on all processed ports (not only on port 80).";
            CustomCheckBoxDPIAdvW.SetToolTip("Info", msgW);
            string msgPort = "Additional TCP port to perform fragmentation on (and HTTP tricks with -w).";
            CustomCheckBoxDPIAdvPort.SetToolTip("Info", msgPort);
            string msgIpId = "Handle additional IP ID (decimal, drop redirects and TCP RSTs with this ID).";
            CustomCheckBoxDPIAdvIpId.SetToolTip("Info", msgIpId);
            string msgAllowNoSni = "Perform circumvention if TLS SNI can't be detected with --blacklist enabled.";
            CustomCheckBoxDPIAdvAllowNoSNI.SetToolTip("Info", msgAllowNoSni);
            string msgSetTtl = "Activate Fake Request Mode and send it with supplied TTL value.\nDANGEROUS! May break websites in unexpected ways. Use with care(or--blacklist).";
            CustomCheckBoxDPIAdvSetTTL.SetToolTip("Info", msgSetTtl);
            string msgAutoTtl = "Activate Fake Request Mode, automatically detect TTL and decrease\nit based on a distance. If the distance is shorter than a2, TTL is decreased\nby a2. If it's longer, (a1; a2) scale is used with the distance as a weight.\nIf the resulting TTL is more than m(ax), set it to m.\nDefault (if set): --auto-ttl 1-4-10. Also sets --min-ttl 3.\nDANGEROUS! May break websites in unexpected ways. Use with care (or --blacklist).";
            CustomCheckBoxDPIAdvAutoTTL.SetToolTip("[a1-a2-m]", msgAutoTtl);
            string msgMinTtl = "Minimum TTL distance (128/64 - TTL) for which to send Fake Request\nin --set - ttl and--auto - ttl modes.";
            CustomCheckBoxDPIAdvMinTTL.SetToolTip("Info", msgMinTtl);
            string msgWrongChksum = "Activate Fake Request Mode and send it with incorrect TCP checksum.\nMay not work in a VM or with some routers, but is safer than set - ttl.";
            CustomCheckBoxDPIAdvWrongChksum.SetToolTip("Info", msgWrongChksum);
            string msgWrongSeq = "Activate Fake Request Mode and send it with TCP SEQ/ACK in the past.";
            CustomCheckBoxDPIAdvWrongSeq.SetToolTip("Info", msgWrongSeq);
            string msgNativeFrag = "Fragment (split) the packets by sending them in smaller packets, without\nshrinking the Window Size. Works faster(does not slow down the connection)\nand better.";
            CustomCheckBoxDPIAdvNativeFrag.SetToolTip("Info", msgNativeFrag);
            string msgReverseFrag = "Fragment (split) the packets just as --native-frag, but send them in the\nreversed order. Works with the websites which could not handle segmented\nHTTPS TLS ClientHello(because they receive the TCP flow \"combined\").";
            CustomCheckBoxDPIAdvReverseFrag.SetToolTip("Info", msgReverseFrag);
            string msgMaxPayload = "Packets with TCP payload data more than [value] won't be processed.\nUse this option to reduce CPU usage by skipping huge amount of data\n(like file transfers) in already established sessions.\nMay skip some huge HTTP requests from being processed.\nDefault(if set): --max-payload 1200.";
            CustomCheckBoxDPIAdvMaxPayload.SetToolTip("Info", msgMaxPayload);
            string msgBlacklist = "Perform circumvention tricks only to host names and subdomains from\nsupplied text file(HTTP Host / TLS SNI).";
            CustomCheckBoxDPIAdvBlacklist.SetToolTip("Info", msgBlacklist);

            // Load Theme
            Theme.LoadTheme(this, Theme.Themes.Dark);

            // Add colors and texts to About page
            CustomLabelAboutThis.ForeColor = Color.DodgerBlue;
            CustomLabelAboutVersion.Text = " v" + Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductVersion;
            CustomLabelAboutThis2.ForeColor = Color.IndianRed;

            // Initialize and load Settings
            if (File.Exists(SecureDNS.SettingsXmlPath) && XmlTool.IsValidXMLFile(SecureDNS.SettingsXmlPath))
                AppSettings = new(this, SecureDNS.SettingsXmlPath);
            else
                AppSettings = new(this);

            UpdateNotifyIconIconAuto();
            CheckUpdateAuto();
            LogClearAuto();
            UpdateBoolsAuto();
            UpdateBoolDnsDohAuto();
            UpdateBoolHttpProxyAuto();
            UpdateStatusShortAuto();
            UpdateStatusLongAuto();
            UpdateStatusCpuUsageAuto();

            // Auto Save Settings (Timer)
            AutoSaveSettings();

            // Set Window State
            LastWindowState = WindowState;

            // Label Moving
            Controls.Add(LabelMoving);
            LabelMoving.Text = "Now Moving...";
            LabelMoving.Size = new(300, 150);
            LabelMoving.Location = new((ClientSize.Width / 2) - (LabelMoving.Width / 2), (ClientSize.Height / 2) - (LabelMoving.Height / 2));
            LabelMoving.TextAlign = ContentAlignment.MiddleCenter;
            LabelMoving.Font = new(FontFamily.GenericSansSerif, 12);
            Theme.SetColors(LabelMoving);
            LabelMoving.Visible = false;
            LabelMoving.SendToBack();

            // Save Log to File
            CustomRichTextBoxLog.TextAppended += CustomRichTextBoxLog_TextAppended;

            Shown += FormMain_Shown;
            Move += FormMain_Move;
            ResizeEnd += FormMain_ResizeEnd;
            Resize += FormMain_Resize;
            MouseUp += FormMain_MouseUp;
            MaximizedBoundsChanged += FormMain_MaximizedBoundsChanged;
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }

        private void HideLabelMoving()
        {
            LabelMoving.Visible = false;
            LabelMoving.SendToBack();
            SplitContainerMain.Visible = true;
        }

        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
        {
            if (ScreenDPI.GetSystemDpi() != 96)
            {
                using Graphics g = CreateGraphics();
                PaintEventArgs args = new(g, DisplayRectangle);
                OnPaint(args);
                string msg = "Display Settings Changed.\n";
                msg += "You may need restart the app to fix display blurriness.";
                CustomMessageBox.Show(this, msg);
            }

            HideLabelMoving();
        }

        private void CustomRichTextBoxLog_TextAppended(object? sender, EventArgs e)
        {
            if (sender is string text)
            {
                // Write to file
                try
                {
                    if (CustomCheckBoxSettingWriteLogWindowToFile.Checked)
                        FileDirectory.AppendText(SecureDNS.LogWindowPath, text, new UTF8Encoding(false));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Write Log to file: {ex.Message}");
                }
            }
        }

        private async void FormMain_Shown(object? sender, EventArgs e)
        {
            if (Once)
            {
                // Fix Microsoft bugs. Like always!
                await ScreenHighDpiScaleStartup(this);

                // Set Min Size for Toggle Log View
                LogHeight = CustomGroupBoxLog.Height;

                // Write binaries if not exist or needs update
                await WriteNecessaryFilesToDisk();

                // Load Saved Servers
                SavedDnsLoad();

                // In case application closed unexpectedly Kill processes and set DNS to dynamic
                KillAll(true);
                await UnsetSavedDNS();

                // Delete Log File on > 500KB
                DeleteFileOnSize(SecureDNS.LogWindowPath, 500);

                // Load Proxy Port
                HttpProxyPort = GetHTTPProxyPortSetting();

                // Start Trace Event Session
                MonitorProcess.SetPID(GetPids(true));
                MonitorProcess.Start(true);

                // Drag bar color
                SplitContainerMain.BackColor = Color.IndianRed;

                HideLabelMoving();

                Once = false;
            }
        }

        private void FormMain_Move(object? sender, EventArgs e)
        {
            SplitContainerMain.Visible = false;
            LabelMoving.Text = "Now Moving...";
            LabelMoving.Location = new((ClientSize.Width / 2) - (LabelMoving.Width / 2), (ClientSize.Height / 2) - (LabelMoving.Height / 2));
            LabelMoving.Visible = true;
            LabelMoving.BringToFront();
        }

        private void FormMain_ResizeEnd(object? sender, EventArgs e)
        {
            HideLabelMoving();
        }

        private void FormMain_Resize(object? sender, EventArgs e)
        {
            if (WindowState != LastWindowState)
            {
                LastWindowState = WindowState;
                if (WindowState == FormWindowState.Normal || WindowState == FormWindowState.Maximized)
                    HideLabelMoving();
            }
            else
            {
                SplitContainerMain.Visible = false;
                LabelMoving.Text = "Now Resizing...";
                LabelMoving.Location = new((ClientSize.Width / 2) - (LabelMoving.Width / 2), (ClientSize.Height / 2) - (LabelMoving.Height / 2));
                LabelMoving.Visible = true;
                LabelMoving.BringToFront();
            }
        }

        private void FormMain_MouseUp(object? sender, MouseEventArgs e)
        {
            HideLabelMoving();
        }

        private void FormMain_MaximizedBoundsChanged(object? sender, EventArgs e)
        {
            HideLabelMoving();
        }

        //============================== Constant

        private void DefaultSettings()
        {
            // Check
            CustomRadioButtonBuiltIn.Checked = true;
            CustomRadioButtonCustom.Checked = false;
            CustomCheckBoxCheckInParallel.Checked = false;
            CustomCheckBoxInsecure.Checked = false;
            LinkLabelCheckUpdate.Text = string.Empty;

            // Connect
            CustomRadioButtonConnectCheckedServers.Checked = true;
            CustomRadioButtonConnectFakeProxyDohViaProxyDPI.Checked = false;
            CustomRadioButtonConnectFakeProxyDohViaGoodbyeDPI.Checked = false;
            CustomRadioButtonConnectDNSCrypt.Checked = false;
            CustomTextBoxHTTPProxy.Text = string.Empty;

            // Share
            CustomCheckBoxHTTPProxyEventShowRequest.Checked = false;
            CustomCheckBoxHTTPProxyEventShowChunkDetails.Checked = false;
            CustomCheckBoxPDpiEnableDpiBypass.Checked = true;
            CustomNumericUpDownPDpiBeforeSniChunks.Value = (decimal)50;
            CustomComboBoxPDpiSniChunkMode.SelectedIndex = 0;
            CustomNumericUpDownPDpiSniChunks.Value = (decimal)5;
            CustomNumericUpDownPDpiAntiPatternOffset.Value = (decimal)2;
            CustomNumericUpDownPDpiFragDelay.Value = (decimal)1;

            // DPI Basic
            CustomRadioButtonDPIMode1.Checked = false;
            CustomRadioButtonDPIMode2.Checked = false;
            CustomRadioButtonDPIMode3.Checked = false;
            CustomRadioButtonDPIMode4.Checked = false;
            CustomRadioButtonDPIMode5.Checked = false;
            CustomRadioButtonDPIMode6.Checked = false;
            CustomRadioButtonDPIModeLight.Checked = true;
            CustomRadioButtonDPIModeMedium.Checked = false;
            CustomRadioButtonDPIModeHigh.Checked = false;
            CustomRadioButtonDPIModeExtreme.Checked = false;
            CustomNumericUpDownSSLFragmentSize.Value = (decimal)40;

            // DPI Advanced
            CustomCheckBoxDPIAdvP.Checked = true;
            CustomCheckBoxDPIAdvR.Checked = true;
            CustomCheckBoxDPIAdvS.Checked = true;
            CustomCheckBoxDPIAdvM.Checked = true;
            CustomCheckBoxDPIAdvF.Checked = false;
            CustomNumericUpDownDPIAdvF.Value = (decimal)2;
            CustomCheckBoxDPIAdvK.Checked = false;
            CustomNumericUpDownDPIAdvK.Value = (decimal)2;
            CustomCheckBoxDPIAdvN.Checked = false;
            CustomCheckBoxDPIAdvE.Checked = true;
            CustomNumericUpDownDPIAdvE.Value = (decimal)40;
            CustomCheckBoxDPIAdvA.Checked = false;
            CustomCheckBoxDPIAdvW.Checked = true;
            CustomCheckBoxDPIAdvPort.Checked = false;
            CustomNumericUpDownDPIAdvPort.Value = (decimal)80;
            CustomCheckBoxDPIAdvIpId.Checked = false;
            CustomTextBoxDPIAdvIpId.Text = string.Empty;
            CustomCheckBoxDPIAdvAllowNoSNI.Checked = false;
            CustomCheckBoxDPIAdvSetTTL.Checked = false;
            CustomNumericUpDownDPIAdvSetTTL.Value = (decimal)1;
            CustomCheckBoxDPIAdvAutoTTL.Checked = false;
            CustomTextBoxDPIAdvAutoTTL.Text = "1-4-10";
            CustomCheckBoxDPIAdvMinTTL.Checked = false;
            CustomNumericUpDownDPIAdvMinTTL.Value = (decimal)3;
            CustomCheckBoxDPIAdvWrongChksum.Checked = false;
            CustomCheckBoxDPIAdvWrongSeq.Checked = false;
            CustomCheckBoxDPIAdvNativeFrag.Checked = true;
            CustomCheckBoxDPIAdvReverseFrag.Checked = false;
            CustomCheckBoxDPIAdvMaxPayload.Checked = true;
            CustomNumericUpDownDPIAdvMaxPayload.Value = (decimal)1200;

            // Settings Working Mode
            CustomRadioButtonSettingWorkingModeDNS.Checked = true;
            CustomRadioButtonSettingWorkingModeDNSandDoH.Checked = false;
            CustomNumericUpDownSettingWorkingModeSetDohPort.Value = (decimal)443;

            // Settings Check
            CustomNumericUpDownSettingCheckTimeout.Value = (decimal)5;
            CustomTextBoxSettingCheckDPIHost.Text = "www.youtube.com";
            CustomCheckBoxSettingProtocolDoH.Checked = true;
            CustomCheckBoxSettingProtocolTLS.Checked = true;
            CustomCheckBoxSettingProtocolDNSCrypt.Checked = true;
            CustomCheckBoxSettingProtocolDNSCryptRelay.Checked = true;
            CustomCheckBoxSettingProtocolDoQ.Checked = true;
            CustomCheckBoxSettingProtocolPlainDNS.Checked = false;
            CustomCheckBoxSettingSdnsDNSSec.Checked = false;
            CustomCheckBoxSettingSdnsNoLog.Checked = false;
            CustomCheckBoxSettingSdnsNoFilter.Checked = true;

            // Settings Connect
            CustomCheckBoxSettingEnableCache.Checked = true;
            CustomNumericUpDownSettingMaxServers.Value = (decimal)5;
            CustomNumericUpDownSettingCamouflageDnsPort.Value = (decimal)5380;

            // Settings Set/Unset DNS
            CustomRadioButtonSettingUnsetDnsToDhcp.Checked = false;
            CustomRadioButtonSettingUnsetDnsToStatic.Checked = true;
            CustomTextBoxSettingUnsetDns1.Text = "8.8.8.8";
            CustomTextBoxSettingUnsetDns2.Text = "8.8.4.4";

            // Settings Share Basic
            CustomNumericUpDownSettingHTTPProxyPort.Value = (decimal)8080;
            CustomNumericUpDownSettingHTTPProxyHandleRequests.Value = (decimal)2000;
            CustomCheckBoxSettingProxyBlockPort80.Checked = true;
            CustomNumericUpDownSettingHTTPProxyKillRequestTimeout.Value = (decimal)20;
            CustomCheckBoxSettingHTTPProxyUpstream.Checked = false;
            CustomCheckBoxSettingHTTPProxyUpstreamOnlyBlockedIPs.Checked = true;
            CustomComboBoxSettingHttpProxyUpstreamMode.SelectedIndex = 1;
            CustomTextBoxSettingHTTPProxyUpstreamHost.Text = IPAddress.Loopback.ToString();
            CustomNumericUpDownSettingHTTPProxyUpstreamPort.Value = (decimal)1090;

            // Settings Share Advanced
            CustomCheckBoxSettingHTTPProxyEnableFakeProxy.Checked = false;
            CustomCheckBoxSettingHTTPProxyCfCleanIP.Checked = false;
            CustomTextBoxSettingHTTPProxyCfCleanIP.Text = string.Empty;
            CustomCheckBoxSettingHTTPProxyEnableFakeDNS.Checked = false;
            CustomCheckBoxSettingHTTPProxyEnableBlackWhiteList.Checked = false;
            CustomCheckBoxSettingHTTPProxyEnableDontBypass.Checked = false;

            // Settings Fake Proxy
            CustomNumericUpDownSettingFakeProxyPort.Value = (decimal)8070;
            CustomTextBoxSettingFakeProxyDohAddress.Text = "https://dns.cloudflare.com/dns-query";
            CustomTextBoxSettingFakeProxyDohCleanIP.Text = "104.16.132.229";

            // Settings CPU
            CustomRadioButtonSettingCPUHigh.Checked = false;
            CustomRadioButtonSettingCPUAboveNormal.Checked = false;
            CustomRadioButtonSettingCPUNormal.Checked = true;
            CustomRadioButtonSettingCPUBelowNormal.Checked = false;
            CustomRadioButtonSettingCPULow.Checked = false;
            CustomNumericUpDownSettingCpuKillProxyRequests.Value = (decimal)40;

            // Settings Others
            CustomTextBoxSettingBootstrapDnsIP.Text = "8.8.8.8";
            CustomNumericUpDownSettingBootstrapDnsPort.Value = (decimal)53;
            CustomTextBoxSettingFallbackDnsIP.Text = "8.8.8.8";
            CustomNumericUpDownSettingFallbackDnsPort.Value = (decimal)53;
            CustomCheckBoxSettingDontAskCertificate.Checked = false;
            CustomCheckBoxSettingDisableAudioAlert.Checked = false;
            CustomCheckBoxSettingWriteLogWindowToFile.Checked = false;
        }

        //============================== Buttons

        private void CustomButtonProcessMonitor_Click(object sender, EventArgs e)
        {
            // Check if it's already open
            Form f = Application.OpenForms[nameof(FormProcessMonitor)];
            if (f != null)
            {
                f.BringToFront();
                return;
            }

            FormProcessMonitor formProcessMonitor = new();
            formProcessMonitor.StartPosition = FormStartPosition.Manual;
            formProcessMonitor.Location = new Point(MousePosition.X - formProcessMonitor.Width, MousePosition.Y - formProcessMonitor.Height);
            formProcessMonitor.FormClosing += (s, e) => { formProcessMonitor.Dispose(); };
            formProcessMonitor.Show();
        }

        private void CustomButtonToggleLogView_Click(object sender, EventArgs e)
        {
            int logHeight = LogHeight;

            if (CustomGroupBoxLog.Visible)
            {
                SuspendLayout();
                CustomGroupBoxLog.Visible = false;
                SplitContainerMain.Panel2Collapsed = true;
                SplitContainerMain.Panel2.Hide();
                Size = new(Width, Height - logHeight);
                ResumeLayout();
                Invalidate();
            }
            else
            {
                SuspendLayout();
                Size = new(Width, Height + logHeight);
                CustomGroupBoxLog.Visible = true;
                SplitContainerMain.Panel2Collapsed = false;
                SplitContainerMain.Panel2.Show();
                ResumeLayout();
                Invalidate();
            }
        }

        private void CustomButtonEditCustomServers_Click(object sender, EventArgs e)
        {
            ToolStripItem tsiEdit = new ToolStripMenuItem("Manage custom servers");
            tsiEdit.Font = Font;
            tsiEdit.Click -= tsiEdit_Click;
            tsiEdit.Click += tsiEdit_Click;
            void tsiEdit_Click(object? sender, EventArgs e)
            {
                edit();
            }

            ToolStripItem tsiViewWorkingServers = new ToolStripMenuItem("View working servers");
            tsiViewWorkingServers.Font = Font;
            tsiViewWorkingServers.Click -= tsiViewWorkingServers_Click;
            tsiViewWorkingServers.Click += tsiViewWorkingServers_Click;
            void tsiViewWorkingServers_Click(object? sender, EventArgs e)
            {
                viewWorkingServers();
            }

            ToolStripItem tsiClearWorkingServers = new ToolStripMenuItem("Clear working servers");
            tsiClearWorkingServers.Font = Font;
            tsiClearWorkingServers.Click -= tsiClearWorkingServers_Click;
            tsiClearWorkingServers.Click += tsiClearWorkingServers_Click;
            void tsiClearWorkingServers_Click(object? sender, EventArgs e)
            {
                try
                {
                    File.Delete(SecureDNS.WorkingServersPath);
                    string msg = $"{NL}Working Servers Cleared.{NL}{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            CustomContextMenuStrip cms = new();
            cms.Font = Font;
            cms.Items.Clear();
            cms.Items.Add(tsiEdit);
            cms.Items.Add(tsiViewWorkingServers);
            cms.Items.Add(tsiClearWorkingServers);
            Theme.SetColors(cms);
            Control b = CustomButtonEditCustomServers;
            cms.Show(b, 0, 0);

            void edit()
            {
                if (IsInAction(true, true, true, true, true, true, true, true, false, false)) return;

                using FormCustomServers formCustomServers = new();
                formCustomServers.ShowDialog(this);
            }

            void viewWorkingServers()
            {
                FileDirectory.CreateEmptyFile(SecureDNS.WorkingServersPath);
                int notepad = ProcessManager.ExecuteOnly(out Process _, "notepad", SecureDNS.WorkingServersPath, false, false, SecureDNS.CurrentPath);
                if (notepad == -1)
                {
                    string msg = "Notepad is not installed on your system.";
                    CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
                }
            }
        }

        private void CustomButtonCheck_Click(object? sender, EventArgs? e)
        {
            if (IsInAction(true, true, true, false, true, true, true, true, true, false)) return;
            StartCheck(null);
        }

        private async void CustomButtonQuickConnect_Click(object sender, EventArgs e)
        {
            await StartQuickConnect(null);
        }

        private async void CustomButtonCheckUpdate_Click(object sender, EventArgs e)
        {
            if (IsInAction(true, true, false, false, false, false, false, false, false, false)) return;
            await CheckUpdate(true);
        }

        private async void CustomButtonWriteSavedServersDelay_Click(object sender, EventArgs e)
        {
            if (IsInAction(true, true, true, true, true, true, true, true, true, false)) return;
            await WriteSavedServersDelayToLog();
        }

        private async void CustomButtonConnect_Click(object? sender, EventArgs? e)
        {
            if (IsInAction(true, true, true, true, false, false, true, true, true, true)) return;
            await StartConnect(GetConnectMode());
        }

        private void CustomButtonSetDNS_Click(object sender, EventArgs e)
        {
            if (IsInAction(true, true, true, true, true, true, true, true, true, false)) return;
            SetDNS();
        }

        private async void CustomButtonShare_Click(object sender, EventArgs e)
        {
            if (IsInAction(true, true, true, true, true, true, true, true, true, true)) return;
            await StartHttpProxy();
        }

        private void CustomButtonSetProxy_Click(object sender, EventArgs e)
        {
            if (IsInAction(true, false, false, false, false, false, false, false, true, true)) return;
            SetProxy();
        }

        private void CustomButtonPDpiApplyChanges_Click(object sender, EventArgs e)
        {
            if (CustomCheckBoxSettingHTTPProxyEnableFakeProxy.Checked && FakeHttpProxyProcess != null)
                ApplyPDpiChangesFakeProxy(FakeHttpProxyProcess);
            if (HttpProxyProcess != null)
                ApplyPDpiChanges(HttpProxyProcess);
        }

        private void CustomButtonPDpiPresetDefault_Click(object sender, EventArgs e)
        {
            CustomNumericUpDownPDpiBeforeSniChunks.Value = (decimal)50;
            CustomComboBoxPDpiSniChunkMode.SelectedIndex = 0;
            CustomNumericUpDownPDpiSniChunks.Value = (decimal)5;
            CustomNumericUpDownPDpiAntiPatternOffset.Value = (decimal)2;
            CustomNumericUpDownPDpiFragDelay.Value = (decimal)1;

            if (IsCheckingStarted) return;
            string msg1 = "Proxy DPI Bypass Mode: ";
            string msg2 = $"Default{NL}";
            CustomRichTextBoxLog.AppendText(msg1, Color.LightGray);
            CustomRichTextBoxLog.AppendText(msg2, Color.DodgerBlue);
        }

        private void CustomButtonDPIBasic_Click(object sender, EventArgs e)
        {
            // Activate/Reactivate GoodbyeDPI Basic
            GoodbyeDPIBasic();
        }

        private void CustomButtonDPIBasicDeactivate_Click(object sender, EventArgs e)
        {
            //Deactivate GoodbyeDPI Basic
            GoodbyeDPIDeactive(true, false);
        }

        private void CustomButtonDPIAdvBlacklist_Click(object sender, EventArgs e)
        {
            // Edit GoodbyeDPI Advanced Blacklist
            FileDirectory.CreateEmptyFile(SecureDNS.DPIBlacklistPath);
            int notepad = ProcessManager.ExecuteOnly(out Process _, "notepad", SecureDNS.DPIBlacklistPath, false, false, SecureDNS.CurrentPath);
            if (notepad == -1)
            {
                string msg = "Notepad is not installed on your system.";
                CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
            }
        }

        private void CustomButtonDPIAdvActivate_Click(object sender, EventArgs e)
        {
            // Activate/Reactivate GoodbyeDPI Advanced
            GoodbyeDPIAdvanced();
        }

        private void CustomButtonDPIAdvDeactivate_Click(object sender, EventArgs e)
        {
            // Deactivate GoodbyeDPI Advanced
            GoodbyeDPIDeactive(false, true);
        }

        private void CustomButtonToolsDnsScanner_Click(object sender, EventArgs e)
        {
            // Check if it's already open
            Form f = Application.OpenForms[nameof(FormDnsScanner)];
            if (f != null)
            {
                f.BringToFront();
                return;
            }

            FormDnsScanner formDnsScanner = new();
            formDnsScanner.StartPosition = FormStartPosition.CenterParent;
            formDnsScanner.FormClosing += (s, e) => { formDnsScanner.Dispose(); };
            formDnsScanner.ShowDialog(this);
        }

        private void CustomButtonToolsDnsLookup_Click(object sender, EventArgs e)
        {
            FormDnsLookup formDnsLookup = new();
            formDnsLookup.FormClosing += (s, e) => { formDnsLookup.Dispose(); };
            formDnsLookup.Show(this);
        }

        private void CustomButtonToolsStampReader_Click(object sender, EventArgs e)
        {
            // Check if it's already open
            Form f = Application.OpenForms[nameof(FormStampReader)];
            if (f != null)
            {
                f.BringToFront();
                return;
            }

            FormStampReader formStampReader = new();
            formStampReader.FormClosing += (s, e) => { formStampReader.Dispose(); };
            formStampReader.Show();
        }

        private void CustomButtonToolsStampGenerator_Click(object sender, EventArgs e)
        {
            // Check if it's already open
            Form f = Application.OpenForms[nameof(FormStampGenerator)];
            if (f != null)
            {
                f.BringToFront();
                return;
            }

            FormStampGenerator formStampGenerator = new();
            formStampGenerator.FormClosing += (s, e) => { formStampGenerator.Dispose(); };
            formStampGenerator.Show();
        }

        private void CustomButtonToolsIpScanner_Click(object sender, EventArgs e)
        {
            // Check if it's already open
            Form f = Application.OpenForms[nameof(FormIpScanner)];
            if (f != null)
            {
                f.BringToFront();
                return;
            }

            FormIpScanner formIpScanner = new();
            formIpScanner.FormClosing += (s, e) => { formIpScanner.Dispose(); };
            formIpScanner.Show();
        }

        private void CustomButtonSettingUninstallCertificate_Click(object sender, EventArgs e)
        {
            UninstallCertificate();
        }

        private void CustomButtonSettingHTTPProxyFakeDNS_Click(object sender, EventArgs e)
        {
            FileDirectory.CreateEmptyFile(SecureDNS.FakeDnsRulesPath);
            int notepad = ProcessManager.ExecuteOnly(out Process _, "notepad", SecureDNS.FakeDnsRulesPath, false, false, SecureDNS.CurrentPath);
            if (notepad == -1)
            {
                string msg = "Notepad is not installed on your system.";
                CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
            }
        }

        private void CustomButtonSettingHTTPProxyBlackWhiteList_Click(object sender, EventArgs e)
        {
            FileDirectory.CreateEmptyFile(SecureDNS.BlackWhiteListPath);
            int notepad = ProcessManager.ExecuteOnly(out Process _, "notepad", SecureDNS.BlackWhiteListPath, false, false, SecureDNS.CurrentPath);
            if (notepad == -1)
            {
                string msg = "Notepad is not installed on your system.";
                CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
            }
        }

        private void CustomButtonSettingHTTPProxyDontBypass_Click(object sender, EventArgs e)
        {
            FileDirectory.CreateEmptyFile(SecureDNS.DontBypassListPath);
            int notepad = ProcessManager.ExecuteOnly(out Process _, "notepad", SecureDNS.DontBypassListPath, false, false, SecureDNS.CurrentPath);
            if (notepad == -1)
            {
                string msg = "Notepad is not installed on your system.";
                CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
            }
        }

        private void CustomButtonSettingRestoreDefault_Click(object sender, EventArgs e)
        {
            if (IsCheckingStarted)
            {
                string msgChecking = "Stop check operation first." + NL;
                CustomRichTextBoxLog.AppendText(msgChecking, Color.IndianRed);
                return;
            }

            if (IsConnected)
            {
                string msgConnected = "Disconnect first." + NL;
                CustomRichTextBoxLog.AppendText(msgConnected, Color.IndianRed);
                return;
            }

            if (IsDNSSet)
            {
                string msgDNSIsSet = "Unset DNS first." + NL;
                CustomRichTextBoxLog.AppendText(msgDNSIsSet, Color.IndianRed);
                return;
            }

            DefaultSettings();

            string msgDefault = "Settings restored to default." + NL;
            CustomRichTextBoxLog.AppendText(msgDefault, Color.MediumSeaGreen);
        }

        private void CustomButtonExportUserData_Click(object sender, EventArgs e)
        {
            using SaveFileDialog sfd = new();
            sfd.Filter = "SDC User Data (*.sdcud)|*.sdcud";
            sfd.DefaultExt = ".sdcud";
            sfd.AddExtension = true;
            sfd.RestoreDirectory = true;
            sfd.FileName = $"sdc_user_data_{DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss", CultureInfo.InvariantCulture)}";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ZipFile.CreateFromDirectory(SecureDNS.UserDataDirPath, sfd.FileName);
                    CustomMessageBox.Show(this, "Data exported successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(this, ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CustomButtonImportUserData_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new();
            ofd.Filter = "SDC User Data (*.sdcud)|*.sdcud";
            ofd.DefaultExt = ".sdcud";
            ofd.AddExtension = true;
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ZipFile.ExtractToDirectory(ofd.FileName, SecureDNS.UserDataDirPath, true);
                    Task.Delay(1000).Wait();

                    try
                    {
                        // Load Settings
                        if (File.Exists(SecureDNS.SettingsXmlPath) && XmlTool.IsValidXMLFile(SecureDNS.SettingsXmlPath))
                            AppSettings = new(this, SecureDNS.SettingsXmlPath);
                        else
                            AppSettings = new(this);

                        string msg = "Data imported seccessfully.";
                        CustomMessageBox.Show(this, msg, "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception)
                    {
                        string msg = "Failed importing user data.";
                        CustomMessageBox.Show(this, msg, "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(this, ex.Message, "Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        //============================== Events
        private void SecureDNSClient_CheckedChanged(object sender, EventArgs e)
        {
            string msgCommandError = $"Couldn't send command to Http Proxy, try again.{NL}";
            if (sender is CustomCheckBox checkBoxR && checkBoxR.Name == CustomCheckBoxHTTPProxyEventShowRequest.Name)
            {
                UpdateHttpProxyBools = false;
                if (checkBoxR.Checked)
                {
                    string command = "writerequests -true";
                    if (IsHttpProxyActivated && HttpProxyProcess != null)
                    {
                        bool isSent = ProcessManager.SendCommand(HttpProxyProcess, command);
                        if (!isSent)
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCommandError, Color.IndianRed));
                    }
                }
                else
                {
                    string command = "writerequests -false";
                    if (IsHttpProxyActivated && HttpProxyProcess != null)
                    {
                        bool isSent = ProcessManager.SendCommand(HttpProxyProcess, command);
                        if (!isSent)
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCommandError, Color.IndianRed));
                    }
                }
                UpdateHttpProxyBools = true;
            }

            if (sender is CustomCheckBox checkBoxC && checkBoxC.Name == CustomCheckBoxHTTPProxyEventShowChunkDetails.Name)
            {
                UpdateHttpProxyBools = false;
                if (checkBoxC.Checked)
                {
                    string command = "writechunkdetails -true";
                    if (IsHttpProxyActivated && HttpProxyProcess != null)
                    {
                        bool isSent = ProcessManager.SendCommand(HttpProxyProcess, command);
                        if (!isSent)
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCommandError, Color.IndianRed));
                    }
                }
                else
                {
                    string command = "writechunkdetails -false";
                    if (IsHttpProxyActivated && HttpProxyProcess != null)
                    {
                        bool isSent = ProcessManager.SendCommand(HttpProxyProcess, command);
                        if (!isSent)
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCommandError, Color.IndianRed));
                    }
                }
                UpdateHttpProxyBools = true;
            }

            if (AppSettings == null) return;

            if (sender is CustomRadioButton crbBuiltIn && crbBuiltIn.Name == CustomRadioButtonBuiltIn.Name)
            {
                AppSettings.AddSetting(CustomRadioButtonBuiltIn, nameof(CustomRadioButtonBuiltIn.Checked), CustomRadioButtonBuiltIn.Checked);
            }

            if (sender is CustomRadioButton crbCustom && crbCustom.Name == CustomRadioButtonCustom.Name)
            {
                AppSettings.AddSetting(CustomRadioButtonCustom, nameof(CustomRadioButtonCustom.Checked), CustomRadioButtonCustom.Checked);
            }
        }

        //============================== Closing
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.WindowsShutDown)
            {
                e.Cancel = true;
                Hide();
                //ShowInTaskbar = false; // Makes Titlebar white (I use Show and Hide instead)
                NotifyIconMain.BalloonTipText = "Minimized to tray.";
                NotifyIconMain.BalloonTipIcon = ToolTipIcon.Info;
                NotifyIconMain.ShowBalloonTip(500);
            }
        }

        private void NotifyIconMain_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.SetDarkTitleBar(true); // Just in case
                Show();
                BringToFront();
            }
            else if (e.Button == MouseButtons.Right)
            {
                ShowMainContextMenu();
            }
        }

        //============================== About
        private void CustomLabelAboutThis_Click(object sender, EventArgs e)
        {
            OpenLinks.OpenUrl("https://github.com/msasanmh/SecureDNSClient/");
        }

        private void LinkLabelDNSLookup_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenLinks.OpenUrl("https://github.com/ameshkov/dnslookup");
        }

        private void LinkLabelDNSProxy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenLinks.OpenUrl("https://github.com/AdguardTeam/dnsproxy");
        }

        private void LinkLabelDNSCrypt_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenLinks.OpenUrl("https://github.com/DNSCrypt/dnscrypt-proxy");
        }

        private void LinkLabelGoodbyeDPI_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenLinks.OpenUrl("https://github.com/ValdikSS/GoodbyeDPI");
        }

        private void LinkLabelStAlidxdydz_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenLinks.OpenUrl("https://github.com/alidxdydz");
        }

    }
}