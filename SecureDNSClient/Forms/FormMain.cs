using System.Net;
using System.Diagnostics;
using MsmhTools;
using MsmhTools.HTTPProxyServer;
using MsmhTools.Themes;
using CustomControls;
using System.Text;
using System.Reflection;
// https://github.com/msasanmh/SecureDNSClient

namespace SecureDNSClient
{
    public partial class FormMain : Form
    {
        private static readonly CustomLabel LabelMoving = new();
        public List<Tuple<long, string>> WorkingDnsList = new();
        public List<string> SavedDnsList = new();
        public List<string> SavedEncodedDnsList = new();
        private List<string> WorkingDnsListToFile = new();
        private List<Tuple<long, string>> WorkingDnsAndLatencyListToFile = new();
        private bool Once = true;
        private bool IsCheckingForUpdate = false;
        private bool IsCheckingStarted = false;
        private bool StopChecking = false;
        private bool IsCheckDone = false;
        private bool IsConnecting = false;
        private bool IsDisconnecting = false;
        private bool IsConnected = false;
        public static bool IsDNSConnected { get; set; } = false;
        public bool IsDoHConnected = false;
        private int ConnectedDohPort = 443;
        private bool IsDPIActive = false;
        private bool IsGoodbyeDPIActive = false;
        private bool IsDNSSet = false;
        private bool IsDNSSetting = false;
        private bool IsDNSUnsetting = false;
        private int NumberOfWorkingServers = 0;
        private int LocalDnsLatency = -1;
        private int LocalDohLatency = -1;
        private bool ConnectAllClicked = false;
        private IPAddress? LocalIP = IPAddress.Loopback; // as default
        public Settings AppSettings;
        private readonly ToolStripMenuItem ToolStripMenuItemIcon = new();
        private bool AudioAlertOnline = true;
        private bool AudioAlertOffline = false;
        private bool AudioAlertRequestsExceeded = false;
        private readonly Stopwatch StopWatchCheckDPIWorks = new();
        private readonly Stopwatch StopWatchAudioAlertDelay = new();
        private string TheDll = string.Empty;
        private readonly string NL = Environment.NewLine;
        private readonly int LogHeight;
        private bool IsExiting = false;

        // PIDs
        private int PIDDNSProxy { get; set; } = -1;
        private int PIDDNSCrypt { get; set; } = -1;
        private int PIDGoodbyeDPI { get; set; } = -1;

        // Camouflage
        private HTTPProxyServer CamouflageProxyServer = new();
        private CamouflageDNSServer? CamouflageDNSServer;
        private bool IsBypassProxyActive { get; set; } = false;
        private bool IsBypassDNSActive { get; set; } = false;
        private int PIDDNSCryptBypass = -1;
        private int PIDDNSProxyBypass = -1;
        private int PIDGoodbyeDPIBypass = -1;

        // HTTP Proxy
        private int PIDHttpProxy { get; set; } = -1;
        private Process? ProxyProcess;
        private bool IsProxyActivated = false;
        private bool IsProxyActivating = false;
        private bool IsProxyDeactivating = false;
        private bool IsSharing = false;
        private int ProxyPort = -1;
        private int ProxyRequests = 0;
        private int ProxyMaxRequests = 250;
        private bool IsProxyDPIActive = false;
        private HTTPProxyServer.Program.DPIBypass.Mode ProxyStaticDPIBypassMode = HTTPProxyServer.Program.DPIBypass.Mode.Disable;
        private HTTPProxyServer.Program.DPIBypass.Mode ProxyDPIBypassMode = HTTPProxyServer.Program.DPIBypass.Mode.Disable;
        private HTTPProxyServer.Program.UpStreamProxy.Mode ProxyUpStreamMode = HTTPProxyServer.Program.UpStreamProxy.Mode.Disable;
        private HTTPProxyServer.Program.Dns.Mode ProxyDNSMode = HTTPProxyServer.Program.Dns.Mode.Disable;
        private HTTPProxyServer.Program.FakeDns.Mode ProxyFakeDnsMode = HTTPProxyServer.Program.FakeDns.Mode.Disable;
        private HTTPProxyServer.Program.BlackWhiteList.Mode ProxyBWListMode = HTTPProxyServer.Program.BlackWhiteList.Mode.Disable;
        private HTTPProxyServer.Program.DontBypass.Mode DontBypassMode = HTTPProxyServer.Program.DontBypass.Mode.Disable;
        private bool IsProxySet = false;
        private static bool UpdateProxyBools = true;

        // Fake Proxy
        private int PIDFakeProxy { get; set; } = -1;
        private Process? FakeProxyProcess;

        public FormMain()
        {
            InitializeComponent();
            //CustomStatusStrip1.SizingGrip = false;

            // Set Min Size for Toggle Log View
            MinimumSize = new Size(Width, Height - CustomGroupBoxLog.Height);
            LogHeight = CustomGroupBoxLog.Height;

            // Fix Screen DPI
            ScreenDPI.ScaleForm(this, true, false);
            FixScreenDPI(this);

            // Rightclick on NotifyIcon
            ToolStripMenuItemIcon.Text = "Exit";
            ToolStripMenuItemIcon.Click += ToolStripMenuItemIcon_Click;
            CustomContextMenuStripIcon.Items.Add(ToolStripMenuItemIcon);

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

            // In case application closed unexpectedly Kill processes and set DNS to dynamic
            KillAll(true);

            // Initialize and load Settings
            if (File.Exists(SecureDNS.SettingsXmlPath) && Xml.IsValidXMLFile(SecureDNS.SettingsXmlPath))
                AppSettings = new(this, SecureDNS.SettingsXmlPath);
            else
                AppSettings = new(this);

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

            Controls.Add(LabelMoving);
            LabelMoving.Text = "Now Moving...";
            LabelMoving.Size = new(300, 150);
            LabelMoving.Location = new((ClientSize.Width / 2) - (LabelMoving.Width / 2), (ClientSize.Height / 2) - (LabelMoving.Height / 2));
            LabelMoving.TextAlign = ContentAlignment.MiddleCenter;
            LabelMoving.Font = new(FontFamily.GenericSansSerif, 12);
            Theme.SetColors(LabelMoving);
            LabelMoving.Visible = false;
            LabelMoving.SendToBack();

            Shown += FormMain_Shown;
            Move += FormMain_Move;
            ResizeEnd += FormMain_ResizeEnd;
            Resize += FormMain_Resize;
        }

        private async void FormMain_Shown(object? sender, EventArgs e)
        {
            if (Once)
            {
                // Write binaries if not exist or needs update
                await WriteNecessaryFilesToDisk();

                // Load Saved Servers
                SavedDnsLoad();

                Once = false;
            }
        }

        private void FormMain_Move(object? sender, EventArgs e)
        {
            SplitContainerMain.Visible = false;
            LabelMoving.Location = new((ClientSize.Width / 2) - (LabelMoving.Width / 2), (ClientSize.Height / 2) - (LabelMoving.Height / 2));
            LabelMoving.Visible = true;
            LabelMoving.BringToFront();
        }

        private void FormMain_ResizeEnd(object? sender, EventArgs e)
        {
            SplitContainerMain.Visible = true;
            LabelMoving.Visible = false;
            LabelMoving.SendToBack();
        }

        private void FormMain_Resize(object? sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            {
                SplitContainerMain.Visible = true;
                LabelMoving.Visible = false;
                LabelMoving.SendToBack();
            }
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
        }

        //============================== Buttons

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
            ToolStripItem tsiBrowse = new ToolStripMenuItem("Browse");
            tsiBrowse.Click -= TsiBrowse_Click;
            tsiBrowse.Click += TsiBrowse_Click;
            void TsiBrowse_Click(object? sender, EventArgs e)
            {
                browse();
            }

            ToolStripItem tsiEdit = new ToolStripMenuItem("Edit");
            tsiEdit.Click -= TsiEdit_Click;
            tsiEdit.Click += TsiEdit_Click;
            void TsiEdit_Click(object? sender, EventArgs e)
            {
                edit();
            }

            ToolStripItem tsiViewWorkingServers = new ToolStripMenuItem("View working servers");
            tsiViewWorkingServers.Click -= tsiViewWorkingServers_Click;
            tsiViewWorkingServers.Click += tsiViewWorkingServers_Click;
            void tsiViewWorkingServers_Click(object? sender, EventArgs e)
            {
                viewWorkingServers();
            }

            CustomContextMenuStrip cms = new();
            cms.Items.Clear();
            cms.Items.Add(tsiBrowse);
            cms.Items.Add(tsiEdit);
            cms.Items.Add(tsiViewWorkingServers);
            Theme.SetColors(cms);
            Control b = CustomButtonEditCustomServers;
            cms.Show(b, 0, 0);
            
            async void browse()
            {
                if (IsCheckingStarted)
                {
                    string msgCheck = $"{NL}Wait for previous task to finish.{NL}{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCheck, Color.DodgerBlue));
                    return;
                }

                using OpenFileDialog ofd = new();
                ofd.Filter = "Custom Servers|*.txt";
                ofd.Multiselect = true;
                ofd.RestoreDirectory = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Reading...{NL}", Color.DodgerBlue));

                    string allContent = string.Empty;
                    string[] files = ofd.FileNames;
                    for (int n = 0; n < files.Length; n++)
                    {
                        string file = files[n];
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

                    if (allContent.Length > 0)
                    {
                        try
                        {
                            await FileDirectory.WriteAllTextAsync(SecureDNS.CustomServersPath, allContent, new UTF8Encoding(false));
                            CustomRadioButtonBuiltIn.Checked = false;
                            CustomRadioButtonCustom.Checked = true;

                            string msg;
                            if (files.Length == 1)
                                msg = $"Loaded 1 file to Custom Servers.{NL}";
                            else
                                msg = $"Loaded {files.Length} files to Custom Servers.{NL}";

                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));

                        }
                        catch (Exception ex)
                        {
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(ex.Message + NL, Color.IndianRed));
                        }
                    }
                }
            }

            void edit()
            {
                FileDirectory.CreateEmptyFile(SecureDNS.CustomServersPath);
                int notepad = ProcessManager.ExecuteOnly(out Process _, "notepad", SecureDNS.CustomServersPath, false, false, SecureDNS.CurrentPath);
                if (notepad == -1)
                {
                    string msg = "Notepad is not installed on your system.";
                    CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
                }
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
            StartCheck();
        }

        private async void CustomButtonCheckUpdate_Click(object sender, EventArgs e)
        {
            await CheckUpdate(true);
        }

        private void CustomButtonConnectAll_Click(object sender, EventArgs e)
        {
            
        }

        private async void CustomButtonWriteSavedServersDelay_Click(object sender, EventArgs e)
        {
            await WriteSavedServersDelayToLog();
        }

        private void CustomButtonConnect_Click(object? sender, EventArgs? e)
        {
            StartConnect();
        }

        private void CustomButtonDPIBasic_Click(object sender, EventArgs e)
        {
            DPIBasic();
        }

        private void CustomButtonDPIBasicDeactivate_Click(object sender, EventArgs e)
        {
            DPIDeactive();
        }

        private void CustomButtonDPIAdvBlacklist_Click(object sender, EventArgs e)
        {
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
            DPIAdvanced();
        }

        private void CustomButtonDPIAdvDeactivate_Click(object sender, EventArgs e)
        {
            DPIDeactive();
        }

        private void CustomButtonSetDNS_Click(object sender, EventArgs e)
        {
            SetDNS();
        }

        private void CustomButtonShare_Click(object sender, EventArgs e)
        {
            Share();
        }

        private void CustomButtonSetProxy_Click(object sender, EventArgs e)
        {
            SetProxy();
        }
        private void CustomButtonPDpiApplyChanges_Click(object sender, EventArgs e)
        {
            if (CustomCheckBoxSettingHTTPProxyEnableFakeProxy.Checked && FakeProxyProcess != null)
                ApplyPDpiChangesFakeProxy(FakeProxyProcess);
            if (ProxyProcess != null)
                ApplyPDpiChanges(ProxyProcess);
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

        private void CustomButtonToolsIpScanner_Click(object sender, EventArgs e)
        {
            FormIpScanner formIpScanner = new();
            formIpScanner.Show();
        }

        private void CustomButtonToolsDnsLookup_Click(object sender, EventArgs e)
        {
            FormDnsLookup formDnsLookup = new();
            formDnsLookup.Show();
        }

        private void CustomButtonToolsStampReader_Click(object sender, EventArgs e)
        {
            FormStampReader formStampReader = new();
            formStampReader.Show();
        }

        private void CustomButtonToolsStampGenerator_Click(object sender, EventArgs e)
        {
            FormStampGenerator formStampGenerator = new();
            formStampGenerator.Show();
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

        //============================== Events
        private void SecureDNSClient_CheckedChanged(object sender, EventArgs e)
        {
            string msgCommandError = $"Couldn't send command to Http Proxy, try again.{NL}";
            if (sender is CustomCheckBox checkBoxR && checkBoxR.Name == CustomCheckBoxHTTPProxyEventShowRequest.Name)
            {
                UpdateProxyBools = false;
                if (checkBoxR.Checked)
                {
                    string command = "writerequests -true";
                    if (IsProxyActivated && ProxyProcess != null)
                    {
                        bool isSent = ProcessManager.SendCommand(ProxyProcess, command);
                        if (!isSent)
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCommandError, Color.IndianRed));
                    }
                }
                else
                {
                    string command = "writerequests -false";
                    if (IsProxyActivated && ProxyProcess != null)
                    {
                        bool isSent = ProcessManager.SendCommand(ProxyProcess, command);
                        if (!isSent)
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCommandError, Color.IndianRed));
                    }
                }
                UpdateProxyBools = true;
            }

            if (sender is CustomCheckBox checkBoxC && checkBoxC.Name == CustomCheckBoxHTTPProxyEventShowChunkDetails.Name)
            {
                UpdateProxyBools = false;
                if (checkBoxC.Checked)
                {
                    string command = "writechunkdetails -true";
                    if (IsProxyActivated && ProxyProcess != null)
                    {
                        bool isSent = ProcessManager.SendCommand(ProxyProcess, command);
                        if (!isSent)
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCommandError, Color.IndianRed));
                    }
                }
                else
                {
                    string command = "writechunkdetails -false";
                    if (IsProxyActivated && ProxyProcess != null)
                    {
                        bool isSent = ProcessManager.SendCommand(ProxyProcess, command);
                        if (!isSent)
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCommandError, Color.IndianRed));
                    }
                }
                UpdateProxyBools = true;
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
                if (!Visible) Show();
                BringToFront();
            }
            else if (e.Button == MouseButtons.Right)
            {
                CustomContextMenuStripIcon.Show();
            }
        }

        private async void ToolStripMenuItemIcon_Click(object? sender, EventArgs e)
        {
            if (IsExiting) return;
            IsExiting = true;

            // Write Closing message to log
            string msg = "Exiting...";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.LightGray));
            NotifyIconMain.BalloonTipText = msg;
            NotifyIconMain.ShowBalloonTip(500);

            // Deactivate GoodbyeDPI
            if (IsGoodbyeDPIActive)
            {
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Deactivating GoodbyeDPI...{NL}", Color.LightGray));
                ProcessManager.KillProcessByPID(PIDGoodbyeDPI);

                // Wait
                Task wait1 = Task.Run(async () =>
                {
                    while (ProcessManager.FindProcessByPID(PIDGoodbyeDPI))
                    {
                        if (!ProcessManager.FindProcessByPID(PIDGoodbyeDPI))
                            break;
                        await Task.Delay(50);
                    }
                });
                await wait1.WaitAsync(TimeSpan.FromSeconds(5));
            }

            // Deactivate GoodbyeDPIBypass (Connect Method 3)
            if (ProcessManager.FindProcessByPID(PIDGoodbyeDPIBypass))
            {
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Deactivating GoodbyeDPI Bypass...{NL}", Color.LightGray));
                ProcessManager.KillProcessByPID(PIDGoodbyeDPIBypass);

                // Wait
                Task wait1 = Task.Run(async () =>
                {
                    while (ProcessManager.FindProcessByPID(PIDGoodbyeDPIBypass))
                    {
                        if (!ProcessManager.FindProcessByPID(PIDGoodbyeDPIBypass))
                            break;
                        await Task.Delay(50);
                    }
                });
                await wait1.WaitAsync(TimeSpan.FromSeconds(5));
            }

            // Unset Proxy
            if (IsProxySet)
            {
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Unsetting Proxy...{NL}", Color.LightGray));
                Network.UnsetProxy(false, false);

                // Wait
                Task wait1 = Task.Run(async () =>
                {
                    while (IsProxySet)
                    {
                        if (!IsProxySet)
                            break;
                        await Task.Delay(50);
                    }
                });
                await wait1.WaitAsync(TimeSpan.FromSeconds(5));
            }

            // Deactivate Proxy
            if (IsProxyActivated || IsProxyActivating)
            {
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Deactivating Proxy...{NL}", Color.LightGray));
                ProcessManager.KillProcessByPID(PIDHttpProxy);

                // Wait
                Task wait1 = Task.Run(async () =>
                {
                    while (ProcessManager.FindProcessByPID(PIDHttpProxy))
                    {
                        if (!ProcessManager.FindProcessByPID(PIDHttpProxy))
                            break;
                        await Task.Delay(50);
                    }
                });
                await wait1.WaitAsync(TimeSpan.FromSeconds(5));
            }

            // Deactivate Fake Proxy
            if (ProcessManager.FindProcessByPID(PIDFakeProxy))
            {
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Deactivating Fake Proxy...{NL}", Color.LightGray));
                ProcessManager.KillProcessByPID(PIDFakeProxy);

                // Wait
                Task wait1 = Task.Run(async () =>
                {
                    while (ProcessManager.FindProcessByPID(PIDFakeProxy))
                    {
                        if (!ProcessManager.FindProcessByPID(PIDFakeProxy))
                            break;
                        await Task.Delay(50);
                    }
                });
                await wait1.WaitAsync(TimeSpan.FromSeconds(5));
            }

            // Unset DNS
            if (IsDNSSet || IsDNSSetting)
            {
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Unsetting DNS...{NL}", Color.LightGray));
                await Task.Run(() => UnsetSavedDNS());
                IsDNSSet = false;
            }

            // Disconnect -  Kill all processes
            if (IsConnected || IsConnecting)
            {
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Disconnecting...{NL}", Color.LightGray));
                await Task.Run(() => KillAll());

                // Wait
                Task wait1 = Task.Run(async () =>
                {
                    while (IsConnected)
                    {
                        if (!IsConnected)
                            break;
                        await Task.Delay(50);
                    }
                });
                await wait1.WaitAsync(TimeSpan.FromSeconds(5));
            }

            // Flush DNS On Exit
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Flushing DNS...{NL}", Color.LightGray));
            await Task.Run(() => FlushDnsOnExit());

            // Select Control type and properties to save
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Saving Settings...{NL}", Color.LightGray));
            AppSettings.AddSelectedControlAndProperty(typeof(CustomCheckBox), "Checked");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomNumericUpDown), "Value");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomRadioButton), "Checked");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomTextBox), "Text");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomTextBox), "Texts");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomComboBox), "SelectedIndex");

            // Add Settings to save
            AppSettings.AddSelectedSettings(this);

            // Save Application Settings
            await AppSettings.SaveAsync(SecureDNS.SettingsXmlPath);

            // Hide NotifyIcon
            NotifyIconMain.Visible = false;

            // Exit
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Goodbye.{NL}", Color.LightGray));
            Environment.Exit(0);
            Application.Exit();
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