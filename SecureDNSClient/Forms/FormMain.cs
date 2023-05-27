using System.Net;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using MsmhTools;
using MsmhTools.HTTPProxyServer;
using MsmhTools.Themes;
using CustomControls;
using System.Net.NetworkInformation;
using System.Text;
using DNSCrypt;
using SecureDNSClient.DPIBasic;
using System.Media;
// https://github.com/msasanmh/SecureDNSClient

namespace SecureDNSClient
{
    public partial class FormMain : Form
    {
        public List<Tuple<long, string>> WorkingDnsList = new();
        private List<string> WorkingDnsListToFile = new();
        private bool IsCheckingStarted = false;
        private bool StopChecking = false;
        private bool IsCheckDone = false;
        private bool IsConnecting = false;
        private bool IsConnected = false;
        private bool IsDNSConnected = false;
        private bool IsDoHConnected = false;
        private bool IsDPIActive = false;
        private bool IsDNSSet = false;
        private bool IsSharing = false;
        private bool IsProxySet = false;
        private int LocalDnsLatency = -1;
        private int LocalDohLatency = -1;
        private int LastProxyPort = 0;
        private bool ConnectAllClicked = false;
        private int NumberOfWorkingServers = 0;
        private IPAddress? LocalIP = IPAddress.Loopback; // as default
        public Settings AppSettings;
        private ToolStripMenuItem ToolStripMenuItemIcon = new();
        private HTTPProxyServer? HTTPProxy;
        private bool AudioAlertOnline = true;
        private bool AudioAlertOffline = false;
        private readonly Stopwatch CheckDPIWorksStopWatch = new();
        private string TheDll = string.Empty;
        private readonly string NL = Environment.NewLine;
        private readonly int LogHeight;

        // PIDs
        private int PIDDNSProxy { get; set; } = -1;
        private int PIDDNSCrypt { get; set; } = -1;
        private int PIDGoodbyeDPI { get; set; } = -1;

        // Camouflage
        private bool IsBypassCloudflareActive = false;
        private bool IsBypassCloudflareDNSActive = false;
        private bool IsBypassCloudflareDPIActive = false;
        private CamouflageDNSServer? CamouflageDNSServer;
        private int PIDDNSProxyCF = -1;
        private int PIDGoodbyeDPICF = -1;

        public FormMain()
        {
            InitializeComponent();
            CustomStatusStrip1.SizingGrip = false;

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

            // Startup Defaults
            Text = Info.InfoExecutingAssembly.ProductName + " v" + Info.InfoExecutingAssembly.ProductVersion;
            CustomButtonSetDNS.Enabled = false;
            CustomTextBoxHTTPProxy.Enabled = false;
            DefaultSettings();

            // Set NotifyIcon Text
            NotifyIconMain.Text = Text;

            // Add Tooltips
            string msgViewCustomServers = "View working custom servers";
            CustomButtonViewWorkingServers.SetToolTip("Info", msgViewCustomServers);

            string msgFragmentChunks = "More chunks means more CPU usage.";
            CustomNumericUpDownHTTPProxyDivideBy.SetToolTip("Warning", msgFragmentChunks);

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
            CustomLabelAboutVersion.Text = " v" + Info.InfoExecutingAssembly.ProductVersion;
            CustomLabelAboutThis2.ForeColor = Color.IndianRed;

            // In case application closed unexpectedly Kill processes and set DNS to dynamic
            KillAll();

            // Initialize and load Settings
            if (File.Exists(SecureDNS.SettingsXmlPath) && Xml.IsValidXMLFile(SecureDNS.SettingsXmlPath))
                AppSettings = new(this, SecureDNS.SettingsXmlPath);
            else
                AppSettings = new(this);

            // Update NICs
            SecureDNS.UpdateNICs(CustomComboBoxNICs);

            UpdateBools();
            UpdateBoolDnsDohAuto();
            UpdateStatusShort();
            UpdateStatusLong();

            // Auto Save Settings (Timer)
            AutoSaveSettings();

            Shown += FormMain_Shown;
        }

        private async void FormMain_Shown(object? sender, EventArgs e)
        {
            // Write binaries if not exist or needs update
            await WriteNecessaryFilesToDisk();
        }

        //============================== Constant

        private void DefaultSettings()
        {
            // Check
            CustomRadioButtonBuiltIn.Checked = true;
            CustomRadioButtonCustom.Checked = false;
            CustomCheckBoxInsecure.Checked = false;

            // Connect
            CustomRadioButtonConnectCheckedServers.Checked = true;
            CustomRadioButtonConnectCloudflare.Checked = false;
            CustomRadioButtonConnectDNSCrypt.Checked = false;
            CustomTextBoxHTTPProxy.Text = string.Empty;

            // Share
            CustomNumericUpDownHTTPProxyPort.Value = (decimal)8080;
            CustomCheckBoxHTTPProxyEventShowRequest.Checked = false;
            CustomCheckBoxHTTPProxyEnableDpiBypass.Checked = true;
            CustomNumericUpDownHTTPProxyDataLength.Value = (decimal)60;
            CustomNumericUpDownHTTPProxyFragmentSize.Value = (decimal)2;
            CustomNumericUpDownHTTPProxyDivideBy.Value = (decimal)60;

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

            // Settings Check
            CustomNumericUpDownSettingCheckTimeout.Value = (decimal)5;
            CustomTextBoxSettingCheckDPIHost.Text = "www.youtube.com";
            CustomCheckBoxSettingSdnsDNSSec.Checked = false;
            CustomCheckBoxSettingSdnsNoLog.Checked = false;
            CustomCheckBoxSettingSdnsNoFilter.Checked = true;

            // Settings Connect
            CustomCheckBoxSettingEnableCache.Checked = true;
            CustomNumericUpDownSettingMaxServers.Value = (decimal)5;
            CustomNumericUpDownSettingCamouflagePort.Value = (decimal)5380;

            // Settings CPU
            CustomRadioButtonSettingCPUHigh.Checked = false;
            CustomRadioButtonSettingCPUAboveNormal.Checked = false;
            CustomRadioButtonSettingCPUNormal.Checked = true;
            CustomRadioButtonSettingCPUBelowNormal.Checked = false;
            CustomRadioButtonSettingCPULow.Checked = false;

            // Settings Others
            CustomTextBoxSettingBootstrapDnsIP.Text = "8.8.8.8";
            CustomNumericUpDownSettingBootstrapDnsPort.Value = (decimal)53;
            CustomTextBoxSettingFallbackDnsIP.Text = "8.8.8.8";
            CustomNumericUpDownSettingFallbackDnsPort.Value = (decimal)53;
            CustomCheckBoxSettingDontAskCertificate.Checked = false;
            CustomCheckBoxSettingDisableAudioAlert.Checked = false;
        }

        //============================== Methods

        private void AutoSaveSettings()
        {
            // Using System.Timers.Timer needs Invoke.
            System.Windows.Forms.Timer autoSaveTimer = new();
            autoSaveTimer.Interval = int.Parse(TimeSpan.FromMinutes(1).TotalMilliseconds.ToString());
            autoSaveTimer.Tick += async (s, e) =>
            {
                // Select Control type and properties to save
                AppSettings.AddSelectedControlAndProperty(typeof(CustomCheckBox), "Checked");
                AppSettings.AddSelectedControlAndProperty(typeof(CustomNumericUpDown), "Value");
                AppSettings.AddSelectedControlAndProperty(typeof(CustomRadioButton), "Checked");
                AppSettings.AddSelectedControlAndProperty(typeof(CustomTextBox), "Text");
                AppSettings.AddSelectedControlAndProperty(typeof(CustomTextBox), "Texts");

                // Add Settings to save
                AppSettings.AddSelectedSettings(this);

                // Save Application Settings
                await AppSettings.SaveAsync(SecureDNS.SettingsXmlPath);
            };
            autoSaveTimer.Start();
        }

        private void FixScreenDPI(Form form)
        {
            using Graphics g = form.CreateGraphics();

            int x1 = 90; int x2 = 120;
            int y1 = 21; int y2 = 207;

            if (form.AutoScaleDimensions == form.CurrentAutoScaleDimensions)
            {
                // 96 = 100%
                // 120 = 125%
                // 144 = 150%
                if (g.DpiX == 120) // 125%
                {
                    setSize(x1 + 15, y1 + 10, x2 + 30, y2 + ((25 * y2) / 100));
                }
                else if (g.DpiX == 144) // 150%
                {
                    setSize(x1 + 65, y1 + 20, x2 + 60, y2 + ((50 * y2) / 100));
                }
                
                void setSize(int x1, int y1, int x2, int y2)
                {
                    CustomTabControlMain.ItemSize = new Size(x1, y1);
                    CustomTabControlSecureDNS.ItemSize = new Size(x1, y1);
                    CustomTabControlDPIBasicAdvanced.ItemSize = new Size(x1, y1);
                    CustomTabControlSettings.ItemSize = new Size(y1 + 9, x1);
                    CustomGroupBoxLog.Height = y2;
                    ToolStripStatusLabelDNS.Width = x2;
                    ToolStripStatusLabelDoH.Width = x2;
                }
            }
        }

        private bool IsInternetAlive()
        {
            if (!Network.IsInternetAlive())
            {
                string msgNet = "There is no Internet connectivity." + NL;
                CustomRichTextBoxLog.AppendText(msgNet, Color.IndianRed);
                return false;
            }
            else
                return true;
        }

        private void UpdateBools()
        {
            System.Timers.Timer updateBoolsTimer = new();
            updateBoolsTimer.Interval = 4000;
            updateBoolsTimer.Elapsed += (s, e) =>
            {
                // Update bool IsConnected
                IsConnected = ProcessManager.FindProcessByID(PIDDNSProxy) ||
                              ProcessManager.FindProcessByID(PIDDNSProxyCF) ||
                              ProcessManager.FindProcessByID(PIDDNSCrypt);

                // In case dnsproxy process terminated
                if (!IsConnected)
                {
                    IsDNSConnected = IsConnected;
                    IsDoHConnected = IsConnected;
                    LocalDnsLatency = -1;
                    LocalDohLatency = -1;
                    if (IsDNSSet) UnsetSavedDNS();
                }

                // Update bool IsDnsSet
                IsDNSSet = UpdateBoolIsDnsSet(out bool _);

                // Update bool IsHTTPProxyRunning
                if (HTTPProxy != null)
                    IsSharing = HTTPProxy.IsRunning;
                else
                    IsSharing = false;

                // Update bool IsProxySet
                IsProxySet = UpdateBoolIsProxySet();

                // Update bool IsDPIActive
                if (ProcessManager.FindProcessByID(PIDGoodbyeDPI) ||
                   (HTTPProxy != null && HTTPProxy.IsRunning && HTTPProxy.IsDpiActive))
                    IsDPIActive = true;
                else
                    IsDPIActive = false;
            };
            updateBoolsTimer.Start();
        }

        private void UpdateBoolDnsDohAuto()
        {
            int timeout = 10000;
            System.Timers.Timer dnsLatencyTimer = new();
            dnsLatencyTimer.Interval = timeout + 500;
            dnsLatencyTimer.Elapsed += (s, e) =>
            {
                Parallel.Invoke(
                    () => UpdateBoolDnsOnce(timeout),
                    () => UpdateBoolDohOnce(timeout)
                );
            };
            dnsLatencyTimer.Start();
        }

        private void UpdateBoolDnsOnce(int timeout)
        {
            if (IsConnected)
            {
                // DNS
                LocalDnsLatency = SecureDNS.CheckDns("google.com", IPAddress.Loopback.ToString(), timeout, GetCPUPriority());
                IsDNSConnected = LocalDnsLatency != -1;

                try
                {
                    if (!string.IsNullOrEmpty(TheDll))
                        if (File.Exists(TheDll) && IsDNSConnected) File.Delete(TheDll);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            else
            {
                LocalDnsLatency = -1;
                IsDNSConnected = LocalDnsLatency != -1;
            }
        }

        private void UpdateBoolDohOnce(int timeout)
        {
            if (IsConnected && CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
            {
                // DoH
                string dohServer = "https://" + IPAddress.Loopback.ToString() + "/dns-query";
                LocalDohLatency = SecureDNS.CheckDns("google.com", dohServer, timeout, GetCPUPriority());
                IsDoHConnected = LocalDohLatency != -1;

                try
                {
                    if (!string.IsNullOrEmpty(TheDll))
                        if (File.Exists(TheDll) && IsDoHConnected) File.Delete(TheDll);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            else
            {
                LocalDohLatency = -1;
                IsDoHConnected = LocalDohLatency != -1;
            }
        }

        private bool UpdateBoolIsDnsSet(out bool isAnotherDnsSet)
        {
            isAnotherDnsSet = false;
            if (File.Exists(SecureDNS.NicNamePath))
            {
                string nicName = File.ReadAllText(SecureDNS.NicNamePath).Replace(NL, string.Empty);
                if (nicName.Length > 0)
                {
                    NetworkInterface? nic = Network.GetNICByName(nicName);
                    if (nic != null)
                    {
                        bool isDnsSet = Network.IsDnsSet(nic, out string dnsServer1, out string dnsServer2);
                        if (!isDnsSet) return false; // DNS is set to DHCP
                        else
                        {
                            if (dnsServer1 == IPAddress.Loopback.ToString())
                                return true;
                            else
                            {
                                isAnotherDnsSet = true;
                                return false;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private bool UpdateBoolIsProxySet()
        {
            if (IsSharing)
            {
                bool isAnyProxySet = Network.IsProxySet(out string httpProxy, out string _, out string _, out string _);
                if (isAnyProxySet)
                    if (!string.IsNullOrEmpty(httpProxy))
                        if (httpProxy.Contains(':'))
                        {
                            string[] split = httpProxy.Split(':');
                            string ip = split[0];
                            string portS = split[1];
                            bool isPortInt = int.TryParse(portS, out int port);
                            if (isPortInt)
                                if (ip == IPAddress.Loopback.ToString() && port == LastProxyPort)
                                    return true;
                        }
            }
            return false;
        }

        private void UpdateStatusShort()
        {
            System.Windows.Forms.Timer timer = new();
            timer.Interval = 500;
            timer.Tick += (s, e) =>
            {
                // Update Status Working Servers
                NumberOfWorkingServers = WorkingDnsList.Count;
                CustomRichTextBoxStatusWorkingServers.ResetText();
                CustomRichTextBoxStatusWorkingServers.AppendText("Working Servers: ", ForeColor);
                CustomRichTextBoxStatusWorkingServers.AppendText(NumberOfWorkingServers.ToString(), Color.DodgerBlue);

                // Connect Button
                if (CustomRadioButtonConnectCloudflare.Checked || CustomRadioButtonConnectDNSCrypt.Checked)
                    CustomButtonConnect.Enabled = true;
                else
                {
                    if (WorkingDnsList.Any() && IsCheckDone && !IsConnecting)
                        CustomButtonConnect.Enabled = true;
                    else
                        CustomButtonConnect.Enabled = IsConnected;
                }

                // Check Button
                CustomButtonCheck.Enabled = !IsConnecting;

                // SetDNS Button
                if (IsConnected)
                    CustomButtonSetDNS.Enabled = true;

                // Live Status
                ToolStripStatusLabelDNS.Text = IsDNSConnected ? "DNS status: Online." : "DNS status: Offline.";
                ToolStripStatusLabelDnsLatency.Text = $"Latency: {LocalDnsLatency}";
                ToolStripStatusLabelDoH.Text = IsDoHConnected ? "DoH status: Online." : "DoH status: Offline.";
                ToolStripStatusLabelDohLatency.Text = $"Latency: {LocalDohLatency}";

                if (!CustomCheckBoxSettingDisableAudioAlert.Checked && !IsCheckingStarted)
                    PlayAudioAlert();
            };
            timer.Start();
        }

        private void UpdateStatusLong()
        {
            System.Windows.Forms.Timer timer = new();
            timer.Interval = 4000;
            timer.Tick += (s, e) =>
            {
                UpdateStatus();
            };
            timer.Start();
        }

        private void UpdateStatus()
        {
            // Update Status IsConnected
            string textConnect = IsConnected ? "Yes" : "No";
            Color colorConnect = IsConnected ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomRichTextBoxStatusIsConnected.ResetText());
            this.InvokeIt(() => CustomRichTextBoxStatusIsConnected.AppendText("Is Connected: ", ForeColor));
            this.InvokeIt(() => CustomRichTextBoxStatusIsConnected.AppendText(textConnect, colorConnect));

            // Update Status IsDnsSet
            string textDNS = IsDNSSet ? "Yes" : "No";
            Color colorDNS = IsDNSSet ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomRichTextBoxStatusIsDNSSet.ResetText());
            this.InvokeIt(() => CustomRichTextBoxStatusIsDNSSet.AppendText("Is DNS Set: ", ForeColor));
            this.InvokeIt(() => CustomRichTextBoxStatusIsDNSSet.AppendText(textDNS, colorDNS));

            // Update Status IsDpiActive
            string textDPI = IsDPIActive ? "Yes" : "No";
            Color colorDPI = IsDPIActive ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomRichTextBoxStatusIsDPIActive.ResetText());
            this.InvokeIt(() => CustomRichTextBoxStatusIsDPIActive.AppendText("Is DPI Active: ", ForeColor));
            this.InvokeIt(() => CustomRichTextBoxStatusIsDPIActive.AppendText(textDPI, colorDPI));

            // Update Status IsSharing
            string textSharing = IsSharing ? "Yes" : "No";
            Color colorSharing = IsSharing ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomRichTextBoxStatusIsSharing.ResetText());
            this.InvokeIt(() => CustomRichTextBoxStatusIsSharing.AppendText("Is Sharing: ", ForeColor));
            this.InvokeIt(() => CustomRichTextBoxStatusIsSharing.AppendText(textSharing, colorSharing));

            // Update Status IsProxySet
            string textProxySet = IsProxySet ? "Yes" : "No";
            Color colorProxySet = IsProxySet ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomRichTextBoxStatusIsProxySet.ResetText());
            this.InvokeIt(() => CustomRichTextBoxStatusIsProxySet.AppendText("Is Proxy Set: ", ForeColor));
            this.InvokeIt(() => CustomRichTextBoxStatusIsProxySet.AppendText(textProxySet, colorProxySet));
        }

        private void PlayAudioAlert()
        {
            if ((IsDNSConnected || IsDoHConnected) && AudioAlertOnline)
            {
                AudioAlertOnline = false;
                AudioAlertOffline = true;

                Task.Run(() =>
                {
                    SoundPlayer soundPlayer = new(Audio.Resource1.DNS_Online);
                    soundPlayer.PlaySync();
                    soundPlayer.Dispose();
                });
            }

            if (!IsDNSConnected && !IsDoHConnected && AudioAlertOffline)
            {
                AudioAlertOffline = false;
                AudioAlertOnline = true;

                int softEtherPID = -1;
                if (ProcessManager.FindProcessByName("vpnclient_x64"))
                    softEtherPID = ProcessManager.GetFirstPIDByName("vpnclient_x64");

                if (softEtherPID != -1)
                    ProcessManager.SuspendProcess(softEtherPID); // On net disconnect SoftEther cause noise to audio.

                Task.Run(() =>
                {
                    Task.Delay(1000).Wait();
                    SoundPlayer soundPlayer = new(Audio.Resource1.DNS_Offline);
                    soundPlayer.PlaySync();
                    soundPlayer.Dispose();
                    Task.Delay(5000).Wait();
                });
                
                if (softEtherPID != -1)
                    ProcessManager.ResumeProcess(softEtherPID);
            }
        }

        private void FlushDNS()
        {
            string? flushDNS = ProcessManager.Execute("ipconfig", "/flushdns");
            if (!string.IsNullOrWhiteSpace(flushDNS))
            {
                // Write flush DNS message to log
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(flushDNS + NL, Color.LightGray));
            }
        }

        private void FlushDnsOnExit()
        {
            ProcessManager.ExecuteOnly("ipconfig", "/flushdns");
            ProcessManager.ExecuteOnly("ipconfig", "/registerdns");
            ProcessManager.ExecuteOnly("ipconfig", "/release");
            ProcessManager.ExecuteOnly("ipconfig", "/renew");
            //ProcessManager.Execute("netsh", "winsock reset"); // Needs PC Restart
        }

        private void KillAll()
        {
            if (ProcessManager.FindProcessByName("dnslookup"))
                ProcessManager.KillProcessByName("dnslookup");
            if (ProcessManager.FindProcessByName("dnsproxy"))
                ProcessManager.KillProcessByName("dnsproxy");
            if (ProcessManager.FindProcessByName("dnscrypt-proxy"))
                ProcessManager.KillProcessByName("dnscrypt-proxy");
            if (ProcessManager.FindProcessByName("goodbyedpi"))
                ProcessManager.KillProcessByName("goodbyedpi");
            UnsetSavedDNS();
        }

        private void UnsetSavedDNS()
        {
            if (File.Exists(SecureDNS.NicNamePath))
            {
                string nicName = File.ReadAllText(SecureDNS.NicNamePath).Replace(NL, string.Empty);
                if (nicName.Length > 0)
                {
                    NetworkInterface? nic = Network.GetNICByName(nicName);
                    if (nic != null)
                    {
                        Network.UnsetDNS(nic);
                        IsDNSSet = false;
                    }
                }
            }
        }

        private ProcessPriorityClass GetCPUPriority()
        {
            if (CustomRadioButtonSettingCPUHigh.Checked)
                return ProcessPriorityClass.High;
            else if (CustomRadioButtonSettingCPUAboveNormal.Checked)
                return ProcessPriorityClass.AboveNormal;
            else if (CustomRadioButtonSettingCPUNormal.Checked)
                return ProcessPriorityClass.Normal;
            else if (CustomRadioButtonSettingCPUBelowNormal.Checked)
                return ProcessPriorityClass.BelowNormal;
            else if (CustomRadioButtonSettingCPULow.Checked)
                return ProcessPriorityClass.Idle;
            else
                return ProcessPriorityClass.Normal;
        }

        private bool CheckNecessaryFiles(bool showMessage = true)
        {
            if (!File.Exists(SecureDNS.DnsLookup) || !File.Exists(SecureDNS.DnsProxy) || !File.Exists(SecureDNS.DNSCrypt) ||
                !File.Exists(SecureDNS.DNSCryptConfigPath) || !File.Exists(SecureDNS.GoodbyeDpi) || !File.Exists(SecureDNS.WinDivert)
                 || !File.Exists(SecureDNS.WinDivert32) || !File.Exists(SecureDNS.WinDivert64))
            {
                if (showMessage)
                {
                    string msg = "ERROR: Some of binary files are missing!" + NL;
                    CustomRichTextBoxLog.AppendText(msg, Color.IndianRed);
                }
                return false;
            }
            else
                return true;
        }

        private async Task WriteNecessaryFilesToDisk()
        {
            // Get New Versions
            string dnslookupNewVer = SecureDNS.GetBinariesVersionFromResource("dnslookup");
            string dnsproxyNewVer = SecureDNS.GetBinariesVersionFromResource("dnsproxy");
            string dnscryptNewVer = SecureDNS.GetBinariesVersionFromResource("dnscrypt-proxy");
            string goodbyedpiNewVer = SecureDNS.GetBinariesVersionFromResource("goodbyedpi");

            // Get Old Versions
            string dnslookupOldVer = SecureDNS.GetBinariesVersion("dnslookup");
            string dnsproxyOldVer = SecureDNS.GetBinariesVersion("dnsproxy");
            string dnscryptOldVer = SecureDNS.GetBinariesVersion("dnscrypt-proxy");
            string goodbyedpiOldVer = SecureDNS.GetBinariesVersion("goodbyedpi");

            // Get Version Result
            int dnslookupResult = Info.VersionCompare(dnslookupNewVer, dnslookupOldVer);
            int dnsproxyResult = Info.VersionCompare(dnsproxyNewVer, dnsproxyOldVer);
            int dnscryptResult = Info.VersionCompare(dnscryptNewVer, dnscryptOldVer);
            int goodbyedpiResult = Info.VersionCompare(goodbyedpiNewVer, goodbyedpiOldVer);

            // Check Missing/Update Binaries
            if (!CheckNecessaryFiles(false) || dnslookupResult == 1 || dnsproxyResult == 1 || dnscryptResult == 1 || goodbyedpiResult == 1)
            {
                string msg1 = "Creating/Updating binaries. Please Wait..." + NL;
                CustomRichTextBoxLog.AppendText(msg1, Color.LightGray);

                await writeBinariesAsync();
            }

            async Task writeBinariesAsync()
            {
                if (!Directory.Exists(SecureDNS.BinaryDirPath))
                    Directory.CreateDirectory(SecureDNS.BinaryDirPath);

                if (!File.Exists(SecureDNS.DnsLookup) || dnslookupResult == 1)
                    await Resource.WriteResourceToFileAsync(NecessaryFiles.Resource1.dnslookup, SecureDNS.DnsLookup);

                if (!File.Exists(SecureDNS.DnsProxy) || dnsproxyResult == 1)
                    await Resource.WriteResourceToFileAsync(NecessaryFiles.Resource1.dnsproxy, SecureDNS.DnsProxy);

                if (!File.Exists(SecureDNS.DNSCrypt) || dnscryptResult == 1)
                    await Resource.WriteResourceToFileAsync(NecessaryFiles.Resource1.dnscrypt_proxyEXE, SecureDNS.DNSCrypt);

                if (!File.Exists(SecureDNS.DNSCryptConfigPath))
                    await Resource.WriteResourceToFileAsync(NecessaryFiles.Resource1.dnscrypt_proxyTOML, SecureDNS.DNSCryptConfigPath);

                if (!File.Exists(SecureDNS.GoodbyeDpi) || goodbyedpiResult == 1)
                    await Resource.WriteResourceToFileAsync(NecessaryFiles.Resource1.goodbyedpi, SecureDNS.GoodbyeDpi);

                if (!File.Exists(SecureDNS.WinDivert))
                    await Resource.WriteResourceToFileAsync(NecessaryFiles.Resource1.WinDivert, SecureDNS.WinDivert);

                if (!File.Exists(SecureDNS.WinDivert32))
                    await Resource.WriteResourceToFileAsync(NecessaryFiles.Resource1.WinDivert32, SecureDNS.WinDivert32);

                if (!File.Exists(SecureDNS.WinDivert64))
                    await Resource.WriteResourceToFileAsync(NecessaryFiles.Resource1.WinDivert64, SecureDNS.WinDivert64);

                // Update old version numbers
                await File.WriteAllTextAsync(SecureDNS.BinariesVersionPath, NecessaryFiles.Resource1.versions);

                string msg2 = $"{Info.InfoExecutingAssembly.ProductName} is ready.{NL}";
                CustomRichTextBoxLog.AppendText(msg2, Color.LightGray);
            }
        }

        private void GenerateCertificate()
        {
            // Create certificate directory
            FileDirectory.CreateEmptyDirectory(SecureDNS.CertificateDirPath);
            string issuerSubjectName = "CN=SecureDNSClient Authority";
            string subjectName = "CN=SecureDNSClient";

            // Generate certificate
            if (!File.Exists(SecureDNS.IssuerCertPath) || !File.Exists(SecureDNS.CertPath) || !File.Exists(SecureDNS.KeyPath))
            {
                IPAddress? gateway = Network.GetDefaultGateway();
                if (gateway != null)
                {
                    Network.GenerateCertificate(SecureDNS.CertificateDirPath, gateway, issuerSubjectName, subjectName);
                    Network.CreateP12(SecureDNS.IssuerCertPath, SecureDNS.IssuerKeyPath);
                    Network.CreateP12(SecureDNS.CertPath, SecureDNS.KeyPath);
                }
            }

            // Install certificate
            if (File.Exists(SecureDNS.IssuerCertPath) && !CustomCheckBoxSettingDontAskCertificate.Checked)
            {
                bool certInstalled = Network.InstallCertificate(SecureDNS.IssuerCertPath, StoreName.Root, StoreLocation.CurrentUser);
                if (!certInstalled)
                {
                    string msg = "Local DoH Server doesn't work without certificate.\nYou can remove certificate anytime from Windows.\nTry again?";
                    using (new CenterWinDialog(this))
                    {
                        DialogResult dr = CustomMessageBox.Show(msg, "Certificate", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (dr == DialogResult.Yes)
                            Network.InstallCertificate(SecureDNS.IssuerCertPath, StoreName.Root, StoreLocation.CurrentUser);
                    }
                }
            }
        }

        private static bool IsDnsProtocolSupported(string dns)
        {
            StringComparison sc = StringComparison.OrdinalIgnoreCase;
            if (dns.StartsWith("tcp://", sc) || dns.StartsWith("udp://", sc) || dns.StartsWith("http://", sc) || dns.StartsWith("https://", sc) ||
                dns.StartsWith("tls://", sc) || dns.StartsWith("quic://", sc) || dns.StartsWith("h3://", sc) || dns.StartsWith("sdns://", sc))
                return true;
            else
                return isPlainDnsWithUnusualPort(dns);

            static bool isPlainDnsWithUnusualPort(string dns) // Support for plain DNS with unusual port
            {
                if (dns.Contains(':'))
                {
                    string[] split = dns.Split(':');
                    string ip = split[0];
                    string port = split[1];
                    if (Network.IsIPv4Valid(ip, out IPAddress _))
                    {
                        bool isPortValid = int.TryParse(port, out int outPort);
                        if (isPortValid && outPort >= 1 && outPort <= 65535)
                            return true;
                    }
                }
                return false;
            }
        }

        //============================== Check

        private async Task CheckServers()
        {
            // Get and check blocked domain is valid
            bool isBlockedDomainValid = SecureDNS.IsBlockedDomainValid(CustomTextBoxSettingCheckDPIHost, CustomRichTextBoxLog, out string blockedDomain);
            if (!isBlockedDomainValid) return;

            // strip www. from blocked domain
            string blockedDomainNoWww = blockedDomain;
            if (blockedDomainNoWww.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                blockedDomainNoWww = blockedDomainNoWww[4..];

            // sdns error message
            string sdnsErrorMsg = "Couldn't decrypt sdns.";

            // Warn users to deactivate DPI before checking servers
            if (IsDPIActive)
            {
                string msg = "It's better to not check servers while DPI is active.\nStart checking servers?";
                var resume = CustomMessageBox.Show(msg, "DPI is active", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (resume == DialogResult.No) return;
            }

            // Warn users to Unset DNS before checking servers
            if (IsDNSSet)
            {
                string msg = "It's better to not check servers while DNS is set.\nStart checking servers?";
                var resume = CustomMessageBox.Show(msg, "DNS is set", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (resume == DialogResult.No) return;
            }

            // Get Bootstrap IP and Port
            string bootstrap = SecureDNS.BootstrapDnsIPv4.ToString();
            int bootstrapPort = SecureDNS.BootstrapDnsPort;
            bool isBootstrap = Network.IsIPv4Valid(CustomTextBoxSettingBootstrapDnsIP.Text, out IPAddress? bootstrapIP);
            if (isBootstrap && bootstrapIP != null)
            {
                bootstrap = bootstrapIP.ToString();
                bootstrapPort = int.Parse(CustomNumericUpDownSettingBootstrapDnsPort.Value.ToString());
            }

            // Get insecure state
            bool insecure = CustomCheckBoxInsecure.Checked;
            int localPortInsecure = 5390;
            if (insecure)
            {
                // Check open ports
                bool isPortOpen = Network.IsPortOpen(IPAddress.Loopback.ToString(), localPortInsecure, 3);
                if (isPortOpen)
                {
                    string existingProcessName = ProcessManager.GetProcessNameByListeningPort(53);
                    existingProcessName = existingProcessName == string.Empty ? "Unknown" : existingProcessName;
                    string msg = $"Port {localPortInsecure} is occupied by \"{existingProcessName}\". You need to resolve the conflict.";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed));
                    IsConnecting = false;
                    return;
                }
            }

            // Check servers comment
            string checkingServers = "Checking servers:" + NL + NL;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(checkingServers, Color.MediumSeaGreen));

            // Built-in or Custom
            bool builtInMode = CustomRadioButtonBuiltIn.Checked;

            // Clear working list to file on new check
            WorkingDnsListToFile.Clear();

            string? fileContent = string.Empty;
            if (builtInMode)
                fileContent = Resource.GetResourceTextFile("SecureDNSClient.DNS-Servers.txt");
            else
            {
                FileDirectory.CreateEmptyFile(SecureDNS.CustomServersPath);
                fileContent = await File.ReadAllTextAsync(SecureDNS.CustomServersPath);

                // Load saved working servers
                WorkingDnsListToFile.LoadFromFile(SecureDNS.WorkingServersPath, true, true);
            }

            // Check if servers exist 1
            if (string.IsNullOrEmpty(fileContent) || string.IsNullOrWhiteSpace(fileContent))
            {
                string msg = "Servers list is empty." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return;
            }

            // Add Servers to list
            List<string> dnsList = fileContent.SplitToLines();
            int dnsCount = dnsList.Count;

            // Get SDNS Properties
            bool sdnsDNSSec = CustomCheckBoxSettingSdnsDNSSec.Checked;
            bool sdnsNoLog = CustomCheckBoxSettingSdnsNoLog.Checked;
            bool sdnsNoFilter = CustomCheckBoxSettingSdnsNoFilter.Checked;

            // Check if servers exist 2
            if (dnsCount < 1)
            {
                string msg = "Servers list is empty." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return;
            }

            // FlushDNS(); // Flush DNS makes first line always return failed

            // Clear temp working list on new check
            WorkingDnsList.Clear();

            for (int n = 0; n < dnsCount; n++)
            {
                if (StopChecking) return;

                // Percentage
                int persent = n * 100 / dnsCount;
                this.InvokeIt(() => ToolStripStatusLabelPercent.Text = persent.ToString() + "%");

                string dns = dnsList[n].Trim();

                if (!string.IsNullOrEmpty(dns) && !string.IsNullOrWhiteSpace(dns) && IsDnsProtocolSupported(dns))
                {
                    // Define company name
                    string company;
                    
                    // SDNS
                    DNSCryptStampReader? dnsCryptStampReader = null;
                    if (dns.StartsWith("sdns://"))
                    {
                        // Decode Stamp
                        dnsCryptStampReader = new(dns);

                        if (dnsCryptStampReader != null)
                        {
                            // Apply SDNS rules
                            if ((sdnsDNSSec == true && !dnsCryptStampReader.IsDnsSec) ||
                                (sdnsNoLog == true && !dnsCryptStampReader.IsNoLog) ||
                                (sdnsNoFilter == true && !dnsCryptStampReader.IsNoFilter)) continue;

                            // Get Company Name (SDNS)
                            string stampHost = dnsCryptStampReader.Host;
                            if (string.IsNullOrEmpty(stampHost))
                                stampHost = dnsCryptStampReader.IP;
                            company = await SecureDNS.HostToCompanyOffline(stampHost);

                            //test1();
                            //void test1()
                            //{
                            //    if (company.ToString() == "Couldn't retrieve information.")
                            //    {
                            //        CustomMessageBox.Show(stampHost);
                            //        company = stampHost;
                            //    }
                            //    string test = $"IP: {dnsCryptStampReader.IP}{NL}";
                            //    test += $"Port: {dnsCryptStampReader.Port}{NL}";
                            //    test += $"Host: {dnsCryptStampReader.Host}{NL}";
                            //    test += $"Path: {dnsCryptStampReader.Path}{NL}";
                            //    test += $"Protocol: {dnsCryptStampReader.ProtocolName}{NL}";
                            //    test += $"DNSSec: {dnsCryptStampReader.IsDnsSec}{NL}";
                            //    test += $"No Log: {dnsCryptStampReader.IsNoLog}{NL}";
                            //    test += $"No Filter: {dnsCryptStampReader.IsNoFilter}{NL}";
                            //    CustomMessageBox.Show(test);
                            //}
                        }
                        else
                            company = sdnsErrorMsg;
                    }
                    else
                    {
                        // Get Company Name (Not SDNS)
                        company = await SecureDNS.UrlToCompanyOffline(dns);
                    }

                    // Get Check timeout value
                    decimal timeoutSec = CustomNumericUpDownSettingCheckTimeout.Value;
                    int timeoutMS = (int)(timeoutSec * 1000);

                    // Get Status and Latency
                    bool dnsOK = false;
                    int latency = -1;
                    if (insecure)
                        latency = SecureDNS.CheckDns(true, blockedDomainNoWww, dns, timeoutMS, localPortInsecure, bootstrap, bootstrapPort, GetCPUPriority());
                    else
                        latency = SecureDNS.CheckDns(blockedDomainNoWww, dns, timeoutMS, GetCPUPriority());
                    dnsOK = latency != -1;

                    if (StopChecking) return;

                    writeStatusToLog();
                    void writeStatusToLog()
                    {
                        // Write status to log
                        string status = dnsOK ? "OK" : "Failed";
                        Color color = dnsOK ? Color.MediumSeaGreen : Color.IndianRed;
                        string resultStatus = $"[{status}]";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultStatus, color));

                        // Write latency to log
                        string resultLatency = $" [{latency}]";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultLatency, Color.DodgerBlue));

                        // Write host to log
                        string resultHost = $" {dns}";
                        if (dnsCryptStampReader == null)
                        {
                            if (!builtInMode)
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultHost, Color.LightGray));
                        }
                        else
                        {
                            string sdnsHostMsg1 = $" SDNS: ";
                            string sdnsHostMsg2 = $"{dnsCryptStampReader.ProtocolName}";
                            if (dnsCryptStampReader.IsDnsSec)
                                sdnsHostMsg2 += $", DNSSec";
                            if (dnsCryptStampReader.IsNoLog)
                                sdnsHostMsg2 += $", No Log";
                            if (dnsCryptStampReader.IsNoFilter)
                                sdnsHostMsg2 += $", No Filter";

                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(sdnsHostMsg1, Color.Orange));
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(sdnsHostMsg2, Color.LightGray));
                        }

                        // Write company name to log
                        string resultCompany = $" [{company}]";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultCompany + NL, Color.Gray));

                    }

                    // Add working DNS to list
                    if (dnsOK)
                    {
                        WorkingDnsList.Add(new Tuple<long, string>(latency, dns));

                        // Add working DNS to list to export
                        if (!builtInMode)
                            WorkingDnsListToFile.Add(dns);
                    }
                }
            }

            // Percentage (100%)
            this.InvokeIt(() => ToolStripStatusLabelPercent.Text = "100%");

            // Return if there is no working server
            if (!WorkingDnsList.Any())
            {
                string noWorkingServer = NL + "There is no working server." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(noWorkingServer, Color.IndianRed));
                return;
            }

            if (StopChecking) return;

            // Sort by latency comment
            string allWorkingServers = NL + "All working servers sorted by latency:" + NL + NL;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(allWorkingServers, Color.MediumSeaGreen));

            // Sort by latency
            if (WorkingDnsList.Count > 1)
                WorkingDnsList = WorkingDnsList.OrderByDescending(t => t.Item1).ToList();

            for (int n = 0; n < WorkingDnsList.Count; n++)
            {
                if (StopChecking) return;

                var latencyHost = WorkingDnsList[n];
                long latency = latencyHost.Item1;
                string host = latencyHost.Item2;
                string dns = host;

                // Define Company Name
                string company;

                // SDNS
                DNSCryptStampReader? dnsCryptStampReader = null;
                if (dns.StartsWith("sdns://"))
                {
                    // Decode Stamp
                    dnsCryptStampReader = new(dns);

                    if (dnsCryptStampReader != null)
                    {
                        // Get Company Name (SDNS)
                        string stampHost = dnsCryptStampReader.Host;
                        if (string.IsNullOrEmpty(stampHost))
                            stampHost = dnsCryptStampReader.IP;
                        company = await SecureDNS.HostToCompanyOffline(stampHost);
                    }
                    else
                        company = sdnsErrorMsg;

                }
                else
                {
                    // Get Company Name (Not SDNS)
                    company = await SecureDNS.UrlToCompanyOffline(dns);
                }
                
                // write sorted result to log
                writeSortedStatusToLog();
                void writeSortedStatusToLog()
                {
                    // Write latency to log
                    string resultLatency1 = "[Latency:";
                    string resultLatency2 = $" {latency}";
                    string resultLatency3 = " ms]";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultLatency1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultLatency2, Color.DodgerBlue));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultLatency3, Color.LightGray));

                    // Write host to log
                    if (dnsCryptStampReader == null)
                    {
                        if (!builtInMode)
                        {
                            string resultHost1 = " [Host:";
                            string resultHost2 = $" {host}";
                            string resultHost3 = "]";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultHost1, Color.LightGray));
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultHost2, Color.DodgerBlue));
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultHost3, Color.LightGray));
                        }
                    }
                    else
                    {
                        string sdnsHostMsg1 = $" SDNS: ";
                        string sdnsHostMsg2 = $"{dnsCryptStampReader.ProtocolName}";
                        if (dnsCryptStampReader.IsDnsSec)
                            sdnsHostMsg2 += $", DNSSec";
                        if (dnsCryptStampReader.IsNoLog)
                            sdnsHostMsg2 += $", No Log";
                        if (dnsCryptStampReader.IsNoFilter)
                            sdnsHostMsg2 += $", No Filter";

                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(sdnsHostMsg1, Color.Orange));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(sdnsHostMsg2, Color.LightGray));
                    }

                    // Write company name to log
                    string resultCompany1 = " [Company:";
                    string resultCompany2 = $" {company}";
                    string resultCompany3 = "]";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultCompany1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultCompany2, Color.DodgerBlue));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultCompany3, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(NL));
                }
            }
        }

        //============================== Connect

        private void Connect()
        {
            // Write Connecting message to log
            string msgConnecting = "Connecting... Please wait..." + NL;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnecting, Color.MediumSeaGreen));

            // Check open ports
            bool port53 = Network.IsPortOpen(IPAddress.Loopback.ToString(), 53, 3);
            bool port443 = Network.IsPortOpen(IPAddress.Loopback.ToString(), 443, 3);
            if (port53)
            {
                string existingProcessName = ProcessManager.GetProcessNameByListeningPort(53);
                existingProcessName = existingProcessName == string.Empty ? "Unknown" : existingProcessName;
                string msg = $"Port 53 is occupied by \"{existingProcessName}\". You need to resolve the conflict." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                IsConnecting = false;
                return;
            }
            if (port443)
            {
                string existingProcessName = ProcessManager.GetProcessNameByListeningPort(443);
                existingProcessName = existingProcessName == string.Empty ? "Unknown" : existingProcessName;
                string msg = $"Port 443 is occupied by \"{existingProcessName}\". You need to resolve the conflict." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                IsConnecting = false;
                return;
            }

            // Flush DNS
            FlushDNS();

            // Generate Certificate for DoH
            if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
                GenerateCertificate();

            // Connect modes
            if (CustomRadioButtonConnectCheckedServers.Checked)
            {
                //=== Connect DNSProxy (With working servers)
                ConnectDNSProxy();
                if (ProcessManager.FindProcessByName("dnsproxy"))
                {
                    // Connected with DNSProxy
                    internalConnect();

                    // Write Connected message to log
                    connectMessage();
                }
                else
                {
                    // Write DNSProxy Error to log
                    string msg = "DNSProxy couldn't connect, try again.";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed));
                }
            }
            else if (CustomRadioButtonConnectCloudflare.Checked)
            {
                //=== Connect Cloudflare
                int bootstrapCfPort = int.Parse(CustomNumericUpDownSettingCamouflagePort.Value.ToString());
                bool connected = BypassCloudflareStart(bootstrapCfPort);
                if (connected)
                {
                    // Write Connected message to log
                    connectMessage();
                }
                
            }
            else if (CustomRadioButtonConnectDNSCrypt.Checked)
            {
                //=== Connect DNSCryptProxy
                ConnectDNSCrypt();
                if (ProcessManager.FindProcessByName("dnscrypt-proxy"))
                {
                    // Connected with DNSCrypt
                    internalConnect();
                }
                else
                {
                    // Write DNSCryptProxy Error to log
                    string msg = "DNSCryptProxy couldn't connect, try again.";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed));
                }
            }

            void internalConnect()
            {
                // To see online status immediately
                Parallel.Invoke(
                    () => UpdateBoolDnsOnce(10000),
                    () => UpdateBoolDohOnce(10000)
                );

                // Update Groupbox Status
                UpdateStatus();

                // Go to DPI Tab if DPI is not already active
                if (ConnectAllClicked && !IsDPIActive)
                {
                    this.InvokeIt(() => CustomTabControlMain.SelectedIndex = 0);
                    this.InvokeIt(() => CustomTabControlSecureDNS.SelectedIndex = 2);
                    this.InvokeIt(() => CustomTabControlDPIBasicAdvanced.SelectedIndex = 2);
                }
            }

            void connectMessage()
            {
                // Update Local IP
                LocalIP = Network.GetLocalIPv4();

                // Get Loopback IP
                IPAddress loopbackIP = IPAddress.Loopback;

                // Write local DNS addresses to log
                string msgLocalDNS1 = "Local DNS Proxy:";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDNS1, Color.LightGray));
                string msgLocalDNS2 = NL + loopbackIP;
                if (LocalIP != null)
                    msgLocalDNS2 += NL + LocalIP.ToString();
                msgLocalDNS2 += NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDNS2, Color.DodgerBlue));

                // Write local DoH addresses to log
                if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
                {
                    string msgLocalDoH1 = "Local DNS Over HTTPS:";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDoH1, Color.LightGray));
                    string msgLocalDoH2 = NL + "https://" + loopbackIP.ToString() + "/dns-query";
                    if (LocalIP != null)
                        msgLocalDoH2 += NL + "https://" + LocalIP.ToString() + "/dns-query";
                    msgLocalDoH2 += NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDoH2, Color.DodgerBlue));
                }
            }
        }

        //---------------------------- Connect: DNSProxy (With working servers)
        private void ConnectDNSProxy()
        {
            // Write Check first to log
            if (NumberOfWorkingServers < 1)
            {
                string msgCheck = "Check servers first." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCheck, Color.IndianRed));
                return;
            }

            // Get Bootstrap IP and Port
            string bootstrap = SecureDNS.BootstrapDnsIPv4.ToString();
            int bootstrapPort = SecureDNS.BootstrapDnsPort;
            bool isBootstrap = Network.IsIPv4Valid(CustomTextBoxSettingBootstrapDnsIP.Text, out IPAddress? bootstrapIP);
            if (isBootstrap && bootstrapIP != null)
            {
                bootstrap = bootstrapIP.ToString();
                bootstrapPort = int.Parse(CustomNumericUpDownSettingBootstrapDnsPort.Value.ToString());
            }

            string hosts = string.Empty;
            int countUsingServers = 0;

            // Sort by latency
            if (WorkingDnsList.Count > 1)
                WorkingDnsList = WorkingDnsList.OrderByDescending(t => t.Item1).ToList();

            // Get number of max servers
            int maxServers = decimal.ToInt32(CustomNumericUpDownSettingMaxServers.Value);

            TheDll = new SecureDNS().DnsProxyDll;
            using StreamWriter theDll = new(TheDll, false);

            // Find fastest servers, max 10
            for (int n = 0; n < WorkingDnsList.Count; n++)
            {
                Tuple<long, string> latencyHost = WorkingDnsList[n];
                long latency = latencyHost.Item1;
                string host = latencyHost.Item2;

                hosts += " -u " + host;
                theDll.WriteLine(host);

                countUsingServers = n + 1;
                if (n >= maxServers - 1) break;
            }

            // Start dnsproxy
            string dnsproxyArgs = "-l 0.0.0.0";

            // Add Legacy DNS args
            dnsproxyArgs += " -p 53";

            // Add DoH args
            if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
            {
                if (File.Exists(SecureDNS.CertPath) && File.Exists(SecureDNS.KeyPath))
                    dnsproxyArgs += " --https-port=443 --tls-crt=\"" + SecureDNS.CertPath + "\" --tls-key=\"" + SecureDNS.KeyPath + "\"";
            }

            // Add Cache args
            if (CustomCheckBoxSettingEnableCache.Checked)
                dnsproxyArgs += " --cache";
            
            // Add Insecure
            if (CustomCheckBoxInsecure.Checked)
                dnsproxyArgs += " --insecure";

            // Add upstream args
            //dnsproxyArgs += hosts;
            dnsproxyArgs += $" -u \"{TheDll}\"";
            if (countUsingServers > 1)
                dnsproxyArgs += $" --all-servers -b {bootstrap}:{bootstrapPort}";
            else
                dnsproxyArgs += $" -b {bootstrap}:{bootstrapPort}";

            // Execute DnsProxy
            PIDDNSProxy = ProcessManager.ExecuteOnly(SecureDNS.DnsProxy, dnsproxyArgs, true, false, Info.CurrentPath, GetCPUPriority());

            // Write dnsproxy message to log
            string msgDnsProxy = string.Empty;
            if (ProcessManager.FindProcessByID(PIDDNSProxy))
            {
                if (countUsingServers > 1)
                    msgDnsProxy = "Local DNS Server started using " + countUsingServers + " fastest servers in parallel." + NL;
                else
                    msgDnsProxy = "Local DNS Server started using " + countUsingServers + " server." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDnsProxy, Color.MediumSeaGreen));
            }
            else
            {
                msgDnsProxy = "Error: Couldn't start DNSProxy!";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDnsProxy, Color.IndianRed));
            }
        }

        //---------------------------- Connect: DNSCrypt
        private async void ConnectDNSCrypt()
        {
            if (!CustomRadioButtonConnectDNSCrypt.Checked) return;
            string? proxyScheme = CustomTextBoxHTTPProxy.Text;

            void proxySchemeIncorrect()
            {
                string msgWrongProxy = "HTTP(S) proxy scheme must be like: \"https://myproxy.net:8080\"";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgWrongProxy + NL, Color.IndianRed));
            }

            // Check if proxy scheme is correct 1
            if (string.IsNullOrWhiteSpace(proxyScheme) || !proxyScheme.Contains("//") || proxyScheme.EndsWith('/'))
            {
                proxySchemeIncorrect();
                return;
            }

            // Check if proxy scheme is correct 2
            Uri? uri = Network.UrlToUri(proxyScheme);
            if (uri == null)
            {
                proxySchemeIncorrect();
                return;
            }

            // Check if proxy works
            bool canPing = Network.CanPing(uri.Host, uri.Port, 15);
            if (!canPing)
            {
                string msgWrongProxy = "Proxy doesn't work.";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgWrongProxy + NL, Color.IndianRed));
                return;
            }

            // Check if config file exist
            if (!File.Exists(SecureDNS.DNSCryptConfigPath))
            {
                string msg = "Error: Configuration file doesn't exist";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed));
                return;
            }

            IPAddress bootstrap = SecureDNS.BootstrapDnsIPv4;
            int bootstrapPort = SecureDNS.BootstrapDnsPort;
            bool isBootstrap = Network.IsIPv4Valid(CustomTextBoxSettingBootstrapDnsIP.Text, out IPAddress? bootstrapIP);
            if (isBootstrap && bootstrapIP != null)
            {
                bootstrap = bootstrapIP;
                bootstrapPort = int.Parse(CustomNumericUpDownSettingBootstrapDnsPort.Value.ToString());
            }

            // Edit DNSCrypt Config File
            DNSCryptConfigEditor dnsCryptConfig = new(SecureDNS.DNSCryptConfigPath);
            dnsCryptConfig.EditHTTPProxy(proxyScheme);
            dnsCryptConfig.EditBootstrapDNS(bootstrap, bootstrapPort);

            // Edit DNSCrypt Config File: Enable DoH
            if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
            {
                if (File.Exists(SecureDNS.CertPath) && File.Exists(SecureDNS.KeyPath))
                {
                    dnsCryptConfig.EnableDoH();
                    dnsCryptConfig.EditCertKeyPath(SecureDNS.KeyPath);
                    dnsCryptConfig.EditCertPath(SecureDNS.CertPath);
                }
                else
                {
                    dnsCryptConfig.DisableDoH();
                }
            }
            else
            {
                dnsCryptConfig.DisableDoH();
            }

            // Save DNSCrypt Config File
            await dnsCryptConfig.WriteAsync();

            // Execute DNSCrypt
            Process process = new();
            //process.PriorityClass = GetCPUPriority(); // Exception: No process is associated with this object.
            process.StartInfo.FileName = SecureDNS.DNSCrypt;
            process.StartInfo.Arguments = SecureDNS.DNSCryptConfigPath;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WorkingDirectory = Info.CurrentPath;

            process.OutputDataReceived += (sender, args) =>
            {
                string? data = args.Data;
                if (data != null)
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(data + NL, Color.LightGray));
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                // DNSCrypt writes its output data in error event!
                string? data = args.Data;
                if (data != null)
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(data + NL, Color.LightGray));
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                PIDDNSCrypt = process.Id;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        //---------------------------- Connect: Bypass Cloudflare
        private bool BypassCloudflareStart(int bootstrapCfPort)
        {
            // Just in case something left running
            BypassCloudflareStop(true, true, true, false);

            // Check port
            if (Network.IsPortOpen(IPAddress.Loopback.ToString(), bootstrapCfPort, 3))
            {
                string existingProcessName = ProcessManager.GetProcessNameByListeningPort(bootstrapCfPort);
                existingProcessName = existingProcessName == string.Empty ? "Unknown" : existingProcessName;
                string msg = $"Port {bootstrapCfPort} is occupied by \"{existingProcessName}\". Change Camouflage port from settings1.";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed));
                return false;
            }

            // Create blacklist file
            string cfDoH = "https://dns.cloudflare.com/dns-query";
            string cfHost = "dns.cloudflare.com";
            File.WriteAllText(SecureDNS.DPIBlacklistCFPath, cfHost);

            // Check Cloudflare message
            string msgCheckCF = $"Checking Cloudflare...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCheckCF, Color.Orange));

            // Get and check blocked domain is valid
            bool isBlockedDomainValid = SecureDNS.IsBlockedDomainValid(CustomTextBoxSettingCheckDPIHost, CustomRichTextBoxLog, out string blockedDomain);
            if (!isBlockedDomainValid) return false;

            // strip www. from blocked domain
            string blockedDomainNoWww = blockedDomain;
            if (blockedDomainNoWww.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                blockedDomainNoWww = blockedDomainNoWww[4..];

            // Set timeout (ms)
            int timeoutMS = 5000;

            // Check Cloudflare
            int latency = SecureDNS.CheckDns(blockedDomainNoWww, cfDoH, timeoutMS, GetCPUPriority());
            bool isCfOpen = latency != -1;
            if (isCfOpen)
            {
                // Not blocked, connect normally
                return connectToCfNormally();
            }
            else
            {
                // It's blocked, tryn to bypass
                return tryToBypassCF();
            }

            bool connectToCfNormally()
            {
                // It's available message
                string msgAvailable = $"It's available. Connecting...{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgAvailable, Color.MediumSeaGreen));

                // Get loopback
                string loopback = IPAddress.Loopback.ToString();

                // Start dnsproxy
                string dnsproxyArgs = "-l 0.0.0.0";

                // Add Legacy DNS args
                dnsproxyArgs += " -p 53";

                // Add DoH args
                if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
                {
                    if (File.Exists(SecureDNS.CertPath) && File.Exists(SecureDNS.KeyPath))
                        dnsproxyArgs += " --https-port=443 --tls-crt=\"" + SecureDNS.CertPath + "\" --tls-key=\"" + SecureDNS.KeyPath + "\"";
                }

                // Add Cache args
                if (CustomCheckBoxSettingEnableCache.Checked)
                    dnsproxyArgs += " --cache";

                // Add upstream args
                dnsproxyArgs += $" -u {cfDoH}";

                // Add bootstrap args
                dnsproxyArgs += $" -b {loopback}:{bootstrapCfPort}";

                // Execute DNSProxy
                PIDDNSProxyCF = ProcessManager.ExecuteOnly(SecureDNS.DnsProxy, dnsproxyArgs, true, true, Info.CurrentPath, GetCPUPriority());
                Task.Delay(500).Wait();

                if (ProcessManager.FindProcessByID(PIDDNSProxyCF))
                {
                    // Set domain to check
                    string domainToCheck = "google.com";

                    // Delay
                    int latency = SecureDNS.CheckDns(domainToCheck, cfDoH, timeoutMS * 10, GetCPUPriority());
                    bool result = latency != -1;
                    if (result)
                    {
                        // Connected
                        string msgConnected = $"Connected to Cloudflare.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnected, Color.MediumSeaGreen));

                        // Write delay to log
                        string msgDelay1 = "Server delay: ";
                        string msgDelay2 = $" ms.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDelay1, Color.Orange));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(latency.ToString(), Color.DodgerBlue));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDelay2, Color.Orange));

                        return true;
                    }
                    else
                    {
                        // Couldn't connect normally!
                        string connectNormallyFailed = $"Couldn't connect. It's really weird!{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(connectNormallyFailed, Color.IndianRed));

                        // Kill DNSProxy
                        ProcessManager.KillProcessByID(PIDDNSProxyCF);
                        return false;
                    }
                }
                else
                {
                    // DNSProxy failed to execute
                    string msgDNSProxyFailed = $"DNSProxy failed to execute. Try again.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDNSProxyFailed, Color.IndianRed));
                    return false;
                }
            }

            bool tryToBypassCF()
            {
                // It's blocked message
                string msgBlocked = $"It's blocked.{NL}";
                msgBlocked += $"Trying to bypass Cloudflare{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgBlocked, Color.Orange));

                // Start Camouflage DNS Server
                CamouflageDNSServer = new(bootstrapCfPort);
                CamouflageDNSServer.Start();
                Task.Delay(500).Wait();
                IsBypassCloudflareDNSActive = CamouflageDNSServer.IsRunning;

                if (IsBypassCloudflareDNSActive)
                {
                    string msgCfServer1 = $"Camouflage DNS Server activated. Port: ";
                    string msgCfServer2 = $"{bootstrapCfPort}{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCfServer1, Color.Orange));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCfServer2, Color.DodgerBlue));

                    // Attempt 1
                    // Write attempt 1 message to log
                    string msgAttempt1 = $"Attempt 1, please wait...{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgAttempt1, Color.Orange));

                    // Get loopback
                    string loopback = IPAddress.Loopback.ToString();

                    // Start dnsproxy
                    string dnsproxyArgs = "-l 0.0.0.0";

                    // Add Legacy DNS args
                    dnsproxyArgs += " -p 53";

                    // Add DoH args
                    if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
                    {
                        if (File.Exists(SecureDNS.CertPath) && File.Exists(SecureDNS.KeyPath))
                            dnsproxyArgs += " --https-port=443 --tls-crt=\"" + SecureDNS.CertPath + "\" --tls-key=\"" + SecureDNS.KeyPath + "\"";
                    }

                    // Add Cache args
                    if (CustomCheckBoxSettingEnableCache.Checked)
                        dnsproxyArgs += " --cache";

                    // Add upstream args
                    dnsproxyArgs += $" -u {cfDoH}";

                    // Add bootstrap args
                    dnsproxyArgs += $" -b {loopback}:{bootstrapCfPort}";

                    // Execute DNSProxy
                    PIDDNSProxyCF = ProcessManager.ExecuteOnly(SecureDNS.DnsProxy, dnsproxyArgs, true, true, Info.CurrentPath, GetCPUPriority());
                    Task.Delay(500).Wait();

                    if (ProcessManager.FindProcessByID(PIDDNSProxyCF))
                    {
                        // Start attempt 1
                        bool success1 = bypassCF(DPIBasicBypassMode.Light);
                        if (success1)
                        {
                            IsBypassCloudflareActive = true;

                            // Success message
                            string msgBypassed1 = $"Successfully bypassed on first attempt.{NL}";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgBypassed1, Color.MediumSeaGreen));

                            return true;
                        }
                        else
                        {
                            // Write attempt 1 failed message to log
                            string msgAttempt1Failed = $"Attempt 1 failed.{NL}";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgAttempt1Failed, Color.IndianRed));

                            // Attempt 2
                            // Write attempt 2 message to log
                            string msgAttempt2 = $"Attempt 2, please wait...{NL}";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgAttempt2, Color.Orange));

                            // Deactive GoodbyeDPI of attempt 1
                            BypassCloudflareStop(false, false, true, false);

                            // Start attempt 2
                            bool success2 = bypassCF(DPIBasicBypassMode.Medium);
                            if (success2)
                            {
                                IsBypassCloudflareActive = true;

                                // Success message
                                string msgBypassed2 = $"Successfully bypassed on second attempt.{NL}";
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgBypassed2, Color.MediumSeaGreen));

                                return true;
                            }
                            else
                            {
                                // Not seccess after 2 attempts
                                BypassCloudflareStop(true, true, true, true);
                                string msgFailure1 = "Failure: ";
                                string msgFailure2 = $"Camouflage mode is not compatible with your ISP.{NL}";
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgFailure1, Color.IndianRed));
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgFailure2, Color.LightGray));
                            }
                        }

                        bool bypassCF(DPIBasicBypassMode bypassMode)
                        {
                            // Get Bootsrap IP & Port
                            string bootstrap = SecureDNS.BootstrapDnsIPv4.ToString();
                            int bootstrapPort = SecureDNS.BootstrapDnsPort;
                            bool isBootstrap = Network.IsIPv4Valid(CustomTextBoxSettingBootstrapDnsIP.Text, out IPAddress? bootstrapIP);
                            if (isBootstrap && bootstrapIP != null)
                            {
                                bootstrap = bootstrapIP.ToString();
                                bootstrapPort = int.Parse(CustomNumericUpDownSettingBootstrapDnsPort.Value.ToString());
                            }

                            // Start GoodbyeDPI
                            DPIBasicBypass dpiBypass = new(bypassMode, CustomNumericUpDownSSLFragmentSize.Value, bootstrap, bootstrapPort);
                            string args = $"{dpiBypass.Args} --blacklist {SecureDNS.DPIBlacklistCFPath}";
                            PIDGoodbyeDPICF = ProcessManager.ExecuteOnly(SecureDNS.GoodbyeDpi, args, true, true, SecureDNS.BinaryDirPath, GetCPUPriority());
                            Task.Delay(500).Wait();

                            if (ProcessManager.FindProcessByID(PIDGoodbyeDPICF))
                            {
                                IsBypassCloudflareDPIActive = true;

                                // Get loopback
                                string loopback = IPAddress.Loopback.ToString();

                                // Message
                                string msg1 = "Bypassing";
                                string msg2 = "..";
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg1, Color.MediumSeaGreen));

                                for (int n = 0; n < 9; n++) // About 45 seconds
                                {
                                    // Message before
                                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg2, Color.MediumSeaGreen));
                                    
                                    // Delay
                                    int latency = SecureDNS.CheckDns(blockedDomainNoWww, loopback, timeoutMS, GetCPUPriority());
                                    bool result = latency != -1;
                                    Task.Delay(500).Wait(); // Wait a moment
                                    if (result)
                                    {
                                        // Message add NL on success
                                        msg2 += NL;
                                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg2, Color.MediumSeaGreen));

                                        // Write delay to log
                                        string msgDelay1 = "Server delay: ";
                                        string msgDelay2 = $" ms.{NL}";
                                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDelay1, Color.Orange));
                                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(latency.ToString(), Color.DodgerBlue));
                                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDelay2, Color.Orange));
                                        return true;
                                    }

                                    // Message after
                                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg2, Color.MediumSeaGreen));

                                    Task.Delay(500).Wait();
                                }

                                // Message add NL on failure
                                msg2 += NL;
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg2, Color.MediumSeaGreen));

                                return false;
                            }
                            else
                            {
                                // GoodbyeDPI failed to execute
                                string msgGoodbyeDPIFailed = $"GoodbyeDPI failed to execute. Try again.{NL}";
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgGoodbyeDPIFailed, Color.IndianRed));
                                return false;
                            }
                        }
                    }
                    else
                    {
                        // DNSProxy failed to execute
                        string msgDNSProxyFailed = $"DNSProxy failed to execute. Try again.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDNSProxyFailed, Color.IndianRed));

                        // Kill
                        BypassCloudflareStop(true, true, true, false);
                        return false;
                    }
                }
                else
                {
                    // Camouflage DNS Server couldn't start
                    string msg = "Couldn't start camouflage DNS server, please try again.";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                }
                return false;
            }
        }

        private void BypassCloudflareStop(bool stopCamouflageServer, bool stopDNSProxy, bool stopDPI, bool writeToLog)
        {
            if (stopCamouflageServer && CamouflageDNSServer != null && CamouflageDNSServer.IsRunning)
            {
                CamouflageDNSServer.Stop();
                IsBypassCloudflareDNSActive = false;
            }

            if (stopDNSProxy && ProcessManager.FindProcessByID(PIDDNSProxyCF))
                ProcessManager.KillProcessByID(PIDDNSProxyCF);

            if (stopDPI && ProcessManager.FindProcessByID(PIDGoodbyeDPICF))
            {
                ProcessManager.KillProcessByID(PIDGoodbyeDPICF);
                IsBypassCloudflareDPIActive = false;
            }

            if (writeToLog)
            {
                string msg = $"Camouflage mode deactivated.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
            }
        }

        //============================== DNS

        private async void SetDNS()
        {
            // Get NIC Name
            string? nicName = CustomComboBoxNICs.SelectedItem as string;

            // Check if NIC Name is empty
            if (string.IsNullOrEmpty(nicName))
            {
                string msg = "Select a Network Interface first." + NL;
                CustomRichTextBoxLog.AppendText(msg, Color.LightGray);
                return;
            }

            // Check if NIC is null
            NetworkInterface? nic = Network.GetNICByName(nicName);
            if (nic == null) return;

            string loopbackIP = IPAddress.Loopback.ToString();
            string dnss = loopbackIP;
            if (LocalIP != null)
                dnss += "," + LocalIP;

            if (!IsDNSSet)
            {
                // Set DNS
                // Write Connect first to log
                if (!IsDNSConnected)
                {
                    string msgConnect = string.Empty;
                    if (!IsDoHConnected)
                        msgConnect = "Connect first." + NL;
                    else
                        msgConnect = "Activate legacy DNS Server first." + NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnect, Color.IndianRed));
                    return;
                }

                // Check Internet Connectivity
                if (!IsInternetAlive()) return;

                // Get and check blocked domain is valid
                bool isBlockedDomainValid = SecureDNS.IsBlockedDomainValid(CustomTextBoxSettingCheckDPIHost, CustomRichTextBoxLog, out string blockedDomain);
                if (!isBlockedDomainValid) return;

                // Show warning while connected using dnscrypt + proxy
                if (ProcessManager.FindProcessByID(PIDDNSCrypt) && CustomRadioButtonConnectDNSCrypt.Checked)
                {
                    string msg = "Set DNS while connected via proxy is not a good idea.\nYou may break the connection.\nContinue?";
                    DialogResult dr = CustomMessageBox.Show(msg, "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (dr == DialogResult.No) return;
                }

                // Set DNS
                Network.UnsetDNS(nic); // Unset first
                Task.Delay(200).Wait(); // Wait a moment
                Network.SetDNS(nic, dnss); // Set DNS
                IsDNSSet = true;

                // Save NIC name to file
                FileDirectory.CreateEmptyFile(SecureDNS.NicNamePath);
                File.WriteAllText(SecureDNS.NicNamePath, nicName);

                // Update Groupbox Status
                UpdateStatus();

                // Write Set DNS message to log
                string msg1 = "Local DNS ";
                string msg2 = loopbackIP;
                string msg3 = " set to ";
                string msg4 = nicName + " (" + nic.Description + ")";
                CustomRichTextBoxLog.AppendText(msg1, Color.LightGray);
                CustomRichTextBoxLog.AppendText(msg2, Color.DodgerBlue);
                CustomRichTextBoxLog.AppendText(msg3, Color.LightGray);
                CustomRichTextBoxLog.AppendText(msg4 + NL, Color.DodgerBlue);

                // Go to Check Tab
                if (ConnectAllClicked && IsConnected)
                {
                    this.InvokeIt(() => CustomTabControlMain.SelectedIndex = 0);
                    this.InvokeIt(() => CustomTabControlSecureDNS.SelectedIndex = 0);
                    ConnectAllClicked = false;
                }

                // Check DPI works if DPI is Active
                if (IsDPIActive)
                    await CheckDPIWorks(blockedDomain);
            }
            else
            {
                // Unset DNS
                Network.UnsetDNS(nic);
                Task.Delay(200).Wait();
                UnsetSavedDNS();
                IsDNSSet = false;

                // Flush DNS
                FlushDNS();

                // Update Groupbox Status
                UpdateStatus();

                // Write Unset DNS message to log
                string msg1 = "Local DNS ";
                string msg2 = loopbackIP;
                string msg3 = " removed from ";
                string msg4 = nicName + " (" + nic.Description + ")";
                CustomRichTextBoxLog.AppendText(msg1, Color.LightGray);
                CustomRichTextBoxLog.AppendText(msg2, Color.DodgerBlue);
                CustomRichTextBoxLog.AppendText(msg3, Color.LightGray);
                CustomRichTextBoxLog.AppendText(msg4 + NL, Color.DodgerBlue);
            }
        }

        //============================== Share

        private void Share()
        {
            if (!IsSharing)
            {
                //// Write Set DNS first to log
                //if (!IsDNSSet)
                //{
                //    string msg = "Set DNS first." + NL;
                //    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                //    return;
                //}

                // Check port
                int httpProxyPort = SettingsWindow.GetHTTPProxyPort(CustomNumericUpDownHTTPProxyPort);
                bool portHTTPProxy = Network.IsPortOpen(IPAddress.Loopback.ToString(), httpProxyPort, 3);
                if (portHTTPProxy)
                {
                    string existingProcessName = ProcessManager.GetProcessNameByListeningPort(httpProxyPort);
                    existingProcessName = existingProcessName == string.Empty ? "Unknown" : existingProcessName;
                    string msg = $"Port {httpProxyPort} is occupied by \"{existingProcessName}\". Change the port from Connect tab.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                    IsConnecting = false;
                    return;
                }

                // Start Share
                HTTPProxy = new();
                if (!HTTPProxy.IsRunning)
                {
                    HTTPProxy.OnRequestReceived -= HTTPProxy_OnRequestReceived;
                    HTTPProxy.OnRequestReceived += HTTPProxy_OnRequestReceived;
                    HTTPProxy.OnErrorOccurred -= HTTPProxy_OnErrorOccurred;
                    HTTPProxy.OnErrorOccurred += HTTPProxy_OnErrorOccurred;

                    // Get fragment settings
                    bool enableDpiBypass = CustomCheckBoxHTTPProxyEnableDpiBypass.Checked;
                    int dataLength = int.Parse(CustomNumericUpDownHTTPProxyDataLength.Value.ToString());
                    int fragmentSize = int.Parse(CustomNumericUpDownHTTPProxyFragmentSize.Value.ToString());
                    int divideBy = int.Parse(CustomNumericUpDownHTTPProxyDivideBy.Value.ToString());

                    if (enableDpiBypass)
                    {
                        HTTPProxy.EnableDpiBypassProgram(DPIBypass.Mode.Program, dataLength, fragmentSize, divideBy);
                        IsDPIActive = true;
                    }

                    HTTPProxy.Start(IPAddress.Any, SettingsWindow.GetHTTPProxyPort(CustomNumericUpDownHTTPProxyPort), 100);
                    Task.Delay(500).Wait();

                    // Delete error log on > 1MB
                    if (File.Exists(SecureDNS.HTTPProxyServerErrorLogPath))
                    {
                        try
                        {
                            long lenth = new FileInfo(SecureDNS.HTTPProxyServerErrorLogPath).Length;
                            if (FileDirectory.ConvertSize(lenth, FileDirectory.SizeUnits.Byte, FileDirectory.SizeUnits.MB) > 1)
                                File.Delete(SecureDNS.HTTPProxyServerErrorLogPath);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Delete HTTP Proxy log file: {ex.Message}");
                        }
                    }

                    // Proxy Event Requests
                    void HTTPProxy_OnRequestReceived(object? sender, EventArgs e)
                    {
                        if (sender is string req)
                        {
                            if (CustomCheckBoxHTTPProxyEventShowRequest.Checked)
                            {
                                req += NL; // Adding an additional line break.
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(req, Color.Gray));
                            }
                        }
                    }

                    // Proxy Event Errors
                    void HTTPProxy_OnErrorOccurred(object? sender, EventArgs e)
                    {
                        if (sender is string error)
                        {
                            error += NL; // Adding an additional line break.
                            FileDirectory.AppendTextLine(SecureDNS.HTTPProxyServerErrorLogPath, error, new UTF8Encoding(false));
                        }
                    }

                    if (HTTPProxy.IsRunning)
                    {
                        // Update bool
                        IsSharing = true;

                        // Set Last Proxy Port
                        LastProxyPort = HTTPProxy.ListeningPort;

                        // Write Sharing Address to log
                        LocalIP = Network.GetLocalIPv4(); // Update Local IP
                        IPAddress localIP = LocalIP ?? IPAddress.Loopback;
                        string msgHTTPProxy1 = "Local HTTP Proxy:" + NL;
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgHTTPProxy1, Color.LightGray));
                        string msgHTTPProxy2 = $"http://{localIP}:{SettingsWindow.GetHTTPProxyPort(CustomNumericUpDownHTTPProxyPort)}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgHTTPProxy2 + NL, Color.DodgerBlue));
                    }
                    else
                    {
                        // Update bool
                        IsSharing = false;

                        // Write Sharing Error to log
                        string msgHTTPProxyError = $"HTTP Proxy Server couldn't run.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgHTTPProxyError, Color.IndianRed));
                    }
                }
            }
            else
            {
                // Stop Share
                if (HTTPProxy != null)
                {
                    if (HTTPProxy.IsRunning)
                    {
                        // Unset Proxy First
                        if (IsProxySet) SetProxy();
                        Task.Delay(100).Wait();
                        Network.UnsetProxy(false);

                        HTTPProxy.Stop();
                        Task.Delay(500).Wait();

                        if (!HTTPProxy.IsRunning)
                        {
                            // Update bool
                            IsSharing = false;

                            // Write deactivated message to log
                            string msgDiactivated = $"HTTP Proxy Server deactivated.{NL}";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDiactivated, Color.MediumSeaGreen));
                        }
                        else
                        {
                            // Couldn't stop
                            string msg = $"Couldn't stop HTTP Proxy Server.{NL}";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                        }
                    }
                    else
                    {
                        // Already deactivated
                        string msg = $"It's already deactivated.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
                    }
                }
                else
                {
                    // Already deactivated
                    string msg = $"It's already deactivated.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
                }
            }
        }

        private async void SetProxy()
        {
            if (!IsProxySet)
            {
                // Set Proxy
                // Write Enable Proxy first to log
                if (!IsSharing)
                {
                    string msg = "Enable Proxy first." + NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                    return;
                }

                if (HTTPProxy != null)
                {
                    // Get IP:Port
                    string ip = IPAddress.Loopback.ToString();
                    int port = HTTPProxy.ListeningPort;

                    // Start Set Proxy
                    Network.SetHttpProxy(ip, port);

                    Task.Delay(300).Wait(); // Wait a moment

                    bool isProxySet = Network.IsProxySet(out string _, out string _, out string _, out string _);
                    if (isProxySet)
                    {
                        // Update bool
                        IsProxySet = true;

                        // Write Set Proxy message to log
                        string msg1 = "HTTP Proxy ";
                        string msg2 = $"{ip}:{port}";
                        string msg3 = " set to system.";
                        CustomRichTextBoxLog.AppendText(msg1, Color.LightGray);
                        CustomRichTextBoxLog.AppendText(msg2, Color.DodgerBlue);
                        CustomRichTextBoxLog.AppendText(msg3 + NL, Color.LightGray);

                        // Check DPI Works
                        if (CustomCheckBoxHTTPProxyEnableDpiBypass.Checked)
                        {
                            // Get and check blocked domain is valid
                            bool isBlockedDomainValid = SecureDNS.IsBlockedDomainValid(CustomTextBoxSettingCheckDPIHost, CustomRichTextBoxLog, out string blockedDomain);
                            if (isBlockedDomainValid)
                                await CheckDPIWorks(blockedDomain);
                        }
                    }
                    else
                    {
                        // Write Set Proxy error to log
                        string msg = "Couldn't set HTTP Proxy to system.";
                        CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
                    }
                }
            }
            else
            {
                // Unset Proxy
                Network.UnsetProxy(false);

                Task.Delay(300).Wait(); // Wait a moment

                bool isProxySet = Network.IsProxySet(out string _, out string _, out string _, out string _);
                if (!isProxySet)
                {
                    // Update bool
                    IsProxySet = false;

                    // Write Unset Proxy message to log
                    string msg1 = "HTTP Proxy removed from system.";
                    CustomRichTextBoxLog.AppendText(msg1 + NL, Color.LightGray);
                }
                else
                {
                    // Write Unset Proxy error to log
                    string msg = "Couldn't unset HTTP Proxy from system.";
                    CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
                }
            }
        }

        //============================== DPI

        private async void DPIBasic()
        {
            // Write Connect first to log
            if (!IsDNSConnected && !IsDoHConnected)
            {
                string msgConnect = "Connect first." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnect, Color.IndianRed));
                return;
            }

            // Check Internet Connectivity
            if (!IsInternetAlive()) return;

            // Get and check blocked domain is valid
            bool isBlockedDomainValid = SecureDNS.IsBlockedDomainValid(CustomTextBoxSettingCheckDPIHost, CustomRichTextBoxLog, out string blockedDomain);
            if (!isBlockedDomainValid) return;

            // Kill GoodbyeDPI
            if (ProcessManager.FindProcessByID(PIDGoodbyeDPI))
                ProcessManager.KillProcessByID(PIDGoodbyeDPI);

            string args = string.Empty;
            string text = string.Empty;
            string fallbackDNS = SecureDNS.BootstrapDnsIPv4.ToString();
            int fallbackDnsPort = SecureDNS.BootstrapDnsPort;
            bool isfallBackDNS = Network.IsIPv4Valid(CustomTextBoxSettingBootstrapDnsIP.Text, out IPAddress? fallBackDNSIP);
            if (isfallBackDNS && fallBackDNSIP != null)
            {
                fallbackDNS = fallBackDNSIP.ToString();
                fallbackDnsPort = int.Parse(CustomNumericUpDownSettingBootstrapDnsPort.Value.ToString());
            }

            if (CustomRadioButtonDPIMode1.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Mode1, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIMode2.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Mode2, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIMode3.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Mode3, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIMode4.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Mode4, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIMode5.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Mode5, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIMode6.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Mode6, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIModeLight.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Light, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIModeMedium.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Medium, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIModeHigh.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.High, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIModeExtreme.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Extreme, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }

            // Execute GoodByeDPI
            PIDGoodbyeDPI = ProcessManager.ExecuteOnly(SecureDNS.GoodbyeDpi, args, true, true, SecureDNS.BinaryDirPath, GetCPUPriority());

            if (ProcessManager.FindProcessByID(PIDGoodbyeDPI))
            {
                // Write DPI Mode to log
                string msg = "DPI bypass is active, mode: ";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(text + NL, Color.DodgerBlue));

                // Update Groupbox Status
                UpdateStatus();

                // Set DPI Active true
                IsDPIActive = true;

                // Go to SetDNS Tab if it's not already set
                if (ConnectAllClicked && !IsDNSSet)
                {
                    this.InvokeIt(() => CustomTabControlMain.SelectedIndex = 0);
                    this.InvokeIt(() => CustomTabControlSecureDNS.SelectedIndex = 3);
                }

                // Check DPI works
                await CheckDPIWorks(blockedDomain);
            }
            else
            {
                // Write DPI Error to log
                string msg = "DPI bypass couldn't connect, try again.";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed));
            }
        }

        private async void DPIAdvanced()
        {
            // Write Connect first to log
            if (!IsDNSConnected && !IsDoHConnected)
            {
                string msgConnect = "Connect first." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnect, Color.IndianRed));
                return;
            }

            // Check Internet Connectivity
            if (!IsInternetAlive()) return;

            // Get and check blocked domain is valid
            bool isBlockedDomainValid = SecureDNS.IsBlockedDomainValid(CustomTextBoxSettingCheckDPIHost, CustomRichTextBoxLog, out string blockedDomain);
            if (!isBlockedDomainValid) return;

            // Write IP Error to log
            if (CustomCheckBoxDPIAdvIpId.Checked)
            {
                bool isIpValid = Network.IsIPv4Valid(CustomTextBoxDPIAdvIpId.Text, out IPAddress? tempIP);
                if (!isIpValid)
                {
                    string msgIp = "IP Address is not valid." + NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgIp, Color.IndianRed));
                    return;
                }
            }

            // Write Blacklist file Error to log
            if (CustomCheckBoxDPIAdvBlacklist.Checked)
            {
                if (!File.Exists(SecureDNS.DPIBlacklistPath))
                {
                    string msgError = "Blacklist file not exist." + NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgError, Color.IndianRed));
                    return;
                }
                else
                {
                    string content = File.ReadAllText(SecureDNS.DPIBlacklistPath);
                    if (content.Length < 1 || string.IsNullOrWhiteSpace(content))
                    {
                        string msgError = "Blacklist file is empty." + NL;
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgError, Color.IndianRed));
                        return;
                    }
                }
            }

            // Get args
            int checkCount = 0;
            string args = string.Empty;

            if (CustomCheckBoxDPIAdvP.Checked)
            {
                args += "-p "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvR.Checked)
            {
                args += "-r "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvS.Checked)
            {
                args += "-s "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvM.Checked)
            {
                args += "-m "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvF.Checked)
            {
                args += $"-f {CustomNumericUpDownDPIAdvF.Value} "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvK.Checked)
            {
                args += $"-k {CustomNumericUpDownDPIAdvK.Value} "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvN.Checked)
            {
                args += "-n "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvE.Checked)
            {
                args += $"-e {CustomNumericUpDownDPIAdvE.Value} "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvA.Checked)
            {
                args += "-a "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvW.Checked)
            {
                args += "-w "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvPort.Checked)
            {
                args += $"--port {CustomNumericUpDownDPIAdvPort.Value} "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvIpId.Checked)
            {
                IPAddress ip = IPAddress.Parse(CustomTextBoxDPIAdvIpId.Text);
                args += $"--ip-id {ip} "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvAllowNoSNI.Checked)
            {
                args += "--allow-no-sni "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvSetTTL.Checked)
            {
                args += $"--set-ttl {CustomNumericUpDownDPIAdvSetTTL.Value} "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvAutoTTL.Checked)
            {
                args += "--auto-ttl "; checkCount++;
                if (CustomTextBoxDPIAdvAutoTTL.Text.Length > 0 && !string.IsNullOrWhiteSpace(CustomTextBoxDPIAdvAutoTTL.Text))
                    args += CustomTextBoxDPIAdvAutoTTL.Text + " ";
            }
            if (CustomCheckBoxDPIAdvMinTTL.Checked)
            {
                args += $"--min-ttl {CustomNumericUpDownDPIAdvMinTTL.Value} "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvWrongChksum.Checked)
            {
                args += "--wrong-chksum "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvWrongSeq.Checked)
            {
                args += "--wrong-seq "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvNativeFrag.Checked)
            {
                args += "--native-frag "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvReverseFrag.Checked)
            {
                args += "--reverse-frag "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvMaxPayload.Checked)
            {
                args += $"--max-payload {CustomNumericUpDownDPIAdvMaxPayload.Value} "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvBlacklist.Checked)
            {
                args += $"--blacklist {SecureDNS.DPIBlacklistPath} "; checkCount++;
            }

            string fallbackDNS = SecureDNS.BootstrapDnsIPv4.ToString();
            int fallbackDnsPort = SecureDNS.BootstrapDnsPort;
            bool isfallBackDNS = Network.IsIPv4Valid(CustomTextBoxSettingBootstrapDnsIP.Text, out IPAddress? fallBackDNSIP);
            if (isfallBackDNS && fallBackDNSIP != null)
            {
                fallbackDNS = fallBackDNSIP.ToString();
                fallbackDnsPort = int.Parse(CustomNumericUpDownSettingBootstrapDnsPort.Value.ToString());
            }

            if (checkCount > 0)
            {
                args += $"--dns-addr {fallbackDNS} --dns-port {fallbackDnsPort} --dnsv6-addr {SecureDNS.BootstrapDnsIPv6} --dnsv6-port {SecureDNS.BootstrapDnsPort}";
            }

            // Write Args Error to log
            if (args.Length < 1 && string.IsNullOrWhiteSpace(args))
            {
                string msgError = "Error occurred: Arguments." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgError, Color.IndianRed));
                return;
            }

            // Kill GoodbyeDPI
            if (ProcessManager.FindProcessByID(PIDGoodbyeDPI))
            {
                ProcessManager.KillProcessByID(PIDGoodbyeDPI);
                Task.Delay(100).Wait();
            }

            string text = "Advanced";

            // Execute GoodByeDPI
            PIDGoodbyeDPI = ProcessManager.ExecuteOnly(SecureDNS.GoodbyeDpi, args, true, true, SecureDNS.BinaryDirPath, GetCPUPriority());

            if (ProcessManager.FindProcessByID(PIDGoodbyeDPI))
            {
                // Write DPI Mode to log
                string msg = "DPI bypass is active, mode: ";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(text + NL, Color.DodgerBlue));

                // Update Groupbox Status
                UpdateStatus();

                // Set DPI Active true
                IsDPIActive = true;

                // Go to SetDNS Tab if it's not already set
                if (ConnectAllClicked && !IsDNSSet)
                {
                    this.InvokeIt(() => CustomTabControlMain.SelectedIndex = 0);
                    this.InvokeIt(() => CustomTabControlSecureDNS.SelectedIndex = 3);
                }

                // Check DPI works
                await CheckDPIWorks(blockedDomain);
            }
            else
            {
                // Write DPI Error to log
                string msg = "DPI bypass couldn't connect, try again.";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed));
            }
        }

        private void DPIDeactive()
        {
            if (ProcessManager.FindProcessByID(PIDGoodbyeDPI))
            {
                // Kill GoodbyeDPI
                ProcessManager.KillProcessByID(PIDGoodbyeDPI);

                // Update Groupbox Status
                UpdateStatus();

                // Write to log
                if (ProcessManager.FindProcessByID(PIDGoodbyeDPI))
                {
                    string msgDC = "Couldn't deactivate DPI Bypass. Try again." + NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDC, Color.IndianRed));
                }
                else
                {
                    string msgDC = "DPI bypass deactivated." + NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDC, Color.LightGray));
                }
            }
        }

        private async Task CheckDPIWorks(string host, int timeoutSec = 30) //Default timeout: 100 sec
        {
            if (string.IsNullOrWhiteSpace(host)) return;

            // If user changing DPI mode fast, return.
            if (CheckDPIWorksStopWatch.IsRunning)
                return;

            Task.Delay(1000).Wait();

            // Start StopWatch
            CheckDPIWorksStopWatch.Start();

            // Write start DPI checking to log
            string msgDPI = $"Checking DPI Bypass ({host})...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI, Color.LightGray));

            if (IsDNSSet)
            {
                if (IsDPIActive)
                {
                    try
                    {
                        string url = $"https://{host}/";
                        Uri uri = new(url, UriKind.Absolute);

                        if (HTTPProxy != null && HTTPProxy.IsRunning && HTTPProxy.IsDpiActive && IsSharing)
                        {
                            string proxyScheme = $"http://{IPAddress.Loopback}:{LastProxyPort}";
                            using SocketsHttpHandler socketsHttpHandler = new();
                            socketsHttpHandler.Proxy = new WebProxy(proxyScheme, true);
                            using HttpClient httpClientWithProxy = new(socketsHttpHandler);
                            httpClientWithProxy.Timeout = new TimeSpan(0, 0, timeoutSec);

                            HttpResponseMessage r = await httpClientWithProxy.GetAsync(uri);
                            Task.Delay(500).Wait();

                            if (r.IsSuccessStatusCode || r.StatusCode.ToString() == "NotFound")
                            {
                                msgSuccess();
                                r.Dispose();
                            }
                            else
                                msgFailed(r);
                        }
                        else
                        {
                            using HttpClient httpClient = new();
                            httpClient.Timeout = new TimeSpan(0, 0, timeoutSec);

                            HttpResponseMessage r = await httpClient.GetAsync(uri);
                            Task.Delay(500).Wait();

                            if (r.IsSuccessStatusCode || r.StatusCode.ToString() == "NotFound")
                            {
                                msgSuccess();
                                r.Dispose();
                            }
                            else
                                msgFailed(r);
                        }

                        void msgSuccess()
                        {
                            // Write Success to log
                            var elapsedTime = Math.Round((double)CheckDPIWorksStopWatch.ElapsedMilliseconds / 1000);
                            string msgDPI1 = $"DPI Check: ";
                            string msgDPI2 = $"Successfully opened {host} in {elapsedTime} seconds.{NL}";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI1, Color.LightGray));
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI2, Color.MediumSeaGreen));
                        }

                        void msgFailed(HttpResponseMessage r)
                        {
                            // Write Status to log
                            string msgDPI1 = $"DPI Check: ";
                            string msgDPI2 = $"Status {r.StatusCode}: {r.ReasonPhrase}.{NL}";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI1, Color.LightGray));
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI2, Color.DodgerBlue));

                            r.Dispose();
                        }

                        CheckDPIWorksStopWatch.Stop();
                        CheckDPIWorksStopWatch.Reset();
                    }
                    catch (Exception ex)
                    {
                        // Write Failed to log
                        string msgDPI1 = $"DPI Check: ";
                        string msgDPI2 = $"{ex.Message}{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI1, Color.LightGray));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI2, Color.IndianRed));
                        CheckDPIWorksStopWatch.Stop();
                        CheckDPIWorksStopWatch.Reset();
                    }
                }
                else
                {
                    // Write activate DPI first to log
                    string msgDPI1 = $"DPI Check: ";
                    string msgDPI2 = $"Activate DPI to check.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI2, Color.IndianRed));
                    CheckDPIWorksStopWatch.Stop();
                    CheckDPIWorksStopWatch.Reset();
                }
            }
            else
            {
                // Write set DNS first to log
                string msgDPI1 = $"DPI Check: ";
                string msgDPI2 = $"Set DNS to check.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI1, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI2, Color.IndianRed));
                CheckDPIWorksStopWatch.Stop();
                CheckDPIWorksStopWatch.Reset();
            }
        }

        //============================== Buttons

        private void CustomButtonToggleLogView_Click(object sender, EventArgs e)
        {
            if (CustomGroupBoxLog.Visible)
            {
                SuspendLayout();
                CustomGroupBoxLog.Visible = false;
                Size = new(Width, Height - LogHeight);
                ResumeLayout();
                Invalidate();
            }
            else
            {
                SuspendLayout();
                Size = new(Width, Height + LogHeight);
                CustomGroupBoxLog.Visible = true;
                ResumeLayout();
                Invalidate();
            }
        }

        private void CustomButtonEditCustomServers_Click(object sender, EventArgs e)
        {
            FileDirectory.CreateEmptyFile(SecureDNS.CustomServersPath);
            int notepad = ProcessManager.ExecuteOnly("notepad", SecureDNS.CustomServersPath, false, false, Info.CurrentPath);
            if (notepad == -1)
            {
                string msg = "Notepad is not installed on your system.";
                CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
            }
        }

        private void CustomButtonViewWorkingServers_Click(object sender, EventArgs e)
        {
            FileDirectory.CreateEmptyFile(SecureDNS.WorkingServersPath);
            int notepad = ProcessManager.ExecuteOnly("notepad", SecureDNS.WorkingServersPath, false, false, Info.CurrentPath);
            if (notepad == -1)
            {
                string msg = "Notepad is not installed on your system.";
                CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
            }
        }

        private void CustomButtonCheck_Click(object? sender, EventArgs? e)
        {
            // Return if binary files are missing
            if (!CheckNecessaryFiles()) return;

            if (!IsCheckingStarted)
            {
                // Start Checking
                // Check Internet Connectivity
                if (!IsInternetAlive()) return;

                // Unset DNS if it's not connected before checking.
                if (!IsConnected)
                {
                    if (IsDNSSet)
                        SetDNS(); // Unset DNS
                    else
                        UnsetSavedDNS(); // Unset Saved DNS
                }

                try
                {
                    Task task = Task.Run(async () =>
                            {
                                IsCheckingStarted = true;
                                IsCheckDone = false;
                                await CheckServers();
                            });

                    task.ContinueWith(_ =>
                      {
                          // Save working servers to file
                          if (!CustomRadioButtonBuiltIn.Checked && WorkingDnsListToFile.Any())
                          {
                              WorkingDnsListToFile = WorkingDnsListToFile.RemoveDuplicates();
                              WorkingDnsListToFile.SaveToFile(SecureDNS.WorkingServersPath);
                          }

                          IsCheckingStarted = false;
                          IsCheckDone = true;

                          string msg = NL + "Check operation finished." + NL;
                          CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue);
                          CustomButtonCheck.Enabled = true;

                          // Go to Connect Tab if it's not already connected
                          if (ConnectAllClicked && !IsConnected && NumberOfWorkingServers > 0)
                          {
                              this.InvokeIt(() => CustomTabControlMain.SelectedIndex = 0);
                              this.InvokeIt(() => CustomTabControlSecureDNS.SelectedIndex = 1);
                          }
                          Debug.WriteLine("Checking Task: " + task.Status);
                          StopChecking = false;
                      }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Stop Checking
                StopChecking = true;
                this.InvokeIt(() => CustomButtonCheck.Enabled = false);
                Task.Delay(100).Wait();
                string msg = NL + "Canceling Check operation..." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));
            }
        }

        private async void CustomButtonConnectAll_Click(object sender, EventArgs e)
        {
            if (!IsCheckingStarted && !IsConnected && !ProcessManager.FindProcessByName("goodbyedpi") && !IsDNSSet)
            {
                if (HTTPProxy == null)
                {
                    await connectAll();
                }
                else
                {
                    if (!HTTPProxy.IsRunning)
                        await connectAll();
                    else
                        disconnectAll();
                }
            }
            else
                disconnectAll();

            async Task connectAll()
            {
                ConnectAllClicked = true;
                CustomButtonCheck_Click(null, null);
                Task taskWait1 = await Task.Run(async () =>
                {
                    while (IsCheckingStarted)
                    {
                        if (!IsCheckingStarted)
                            return Task.CompletedTask;
                        await Task.Delay(1000);
                    }
                    return Task.CompletedTask;
                });
                await Task.Delay(1000);
                if (!IsCheckingStarted)
                {
                    CustomButtonConnect_Click(null, null);
                    Task taskWait2 = await Task.Run(async () =>
                    {
                        while (!IsDNSConnected && !IsDoHConnected)
                        {
                            if (NumberOfWorkingServers < 1)
                                return Task.CompletedTask;
                            await Task.Delay(1000);
                        }
                        return Task.CompletedTask;
                    });
                    await Task.Delay(1000);
                    if (IsDNSConnected || IsDoHConnected)
                    {
                        UpdateStatus();
                        if (!ProcessManager.FindProcessByID(PIDGoodbyeDPI))
                            DPIBasic();
                        UpdateStatus();
                        await Task.Delay(1000);
                        if (!IsDNSSet)
                            SetDNS();
                        UpdateStatus();
                    }
                    ConnectAllClicked = false;
                }
            }

            void disconnectAll()
            {
                if (IsConnected)
                {
                    CustomButtonConnect_Click(null, null);
                    UpdateStatus();
                }
                if (ProcessManager.FindProcessByName("goodbyedpi"))
                {
                    DPIBasic();
                    UpdateStatus();
                }
                if (IsDNSSet)
                {
                    SetDNS();
                    UpdateStatus();
                }
                if (IsCheckingStarted)
                    CustomButtonCheck_Click(null, null);
            }
        }

        private void CustomButtonConnect_Click(object? sender, EventArgs? e)
        {
            // Return if binary files are missing
            if (!CheckNecessaryFiles()) return;

            if (!ProcessManager.FindProcessByName("dnsproxy") && !ProcessManager.FindProcessByName("dnscrypt-proxy"))
            {
                try
                {
                    // Connect
                    // Check Internet Connectivity
                    if (!IsInternetAlive()) return;

                    // Update NICs
                    SecureDNS.UpdateNICs(CustomComboBoxNICs);

                    Task taskConnect = Task.Run(() =>
                    {
                        // Stop Check
                        if (IsCheckingStarted)
                        {
                            CustomButtonCheck_Click(null, null);

                            // Wait until check is done
                            while (!IsCheckDone)
                                Task.Delay(100).Wait();
                        }

                        IsConnecting = true;
                        Connect();
                    });

                    taskConnect.ContinueWith(_ =>
                    {
                        IsConnecting = false;
                        Debug.WriteLine("Connect Task: " + taskConnect.Status);
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                try
                {
                    // Disconnect
                    // Write Disconnecting message to log
                    string msgDisconnecting = "Disconnecting..." + NL;
                    CustomRichTextBoxLog.AppendText(msgDisconnecting, Color.MediumSeaGreen);

                    // Unset DNS
                    if (IsDNSSet)
                        UnsetSavedDNS();

                    // Deactivate DPI
                    DPIDeactive();

                    // Kill processes (DNSProxy, DNSCrypt)
                    if (ProcessManager.FindProcessByName("dnsproxy"))
                        ProcessManager.KillProcessByName("dnsproxy");
                    if (ProcessManager.FindProcessByName("dnscrypt-proxy"))
                        ProcessManager.KillProcessByName("dnscrypt-proxy");

                    // Stop Cloudflare Bypass
                    BypassCloudflareStop(true, true, true, false);

                    // Stop HTTP Proxy (Sharing)
                    if (HTTPProxy != null && HTTPProxy.IsRunning)
                        HTTPProxy.Stop();

                    // Unset Proxy
                    Network.UnsetProxy(false);

                    Task.Delay(500).Wait();

                    // Write Disconnected message to log
                    string msgDisconnected = "Disconnected." + NL;
                    CustomRichTextBoxLog.AppendText(msgDisconnected, Color.MediumSeaGreen);

                    // Update Groupbox Status
                    UpdateStatus();

                    // To see offline status immediately
                    Parallel.Invoke(
                        () => UpdateBoolDnsOnce(1000),
                        () => UpdateBoolDohOnce(1000)
                    );
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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
            int notepad = ProcessManager.ExecuteOnly("notepad", SecureDNS.DPIBlacklistPath, false, false, Info.CurrentPath);
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

        private void CustomButtonRestoreDefault_Click(object sender, EventArgs e)
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
            if (sender is CustomRadioButton crbBuiltIn && crbBuiltIn.Name == CustomRadioButtonBuiltIn.Name)
            {
                AppSettings.AddSetting(CustomRadioButtonBuiltIn, nameof(CustomRadioButtonBuiltIn.Checked), CustomRadioButtonBuiltIn.Checked);
            }

            if (sender is CustomRadioButton crbCustom && crbCustom.Name == CustomRadioButtonCustom.Name)
            {
                AppSettings.AddSetting(CustomRadioButtonCustom, nameof(CustomRadioButtonCustom.Checked), CustomRadioButtonCustom.Checked);
            }

            if (sender is CustomCheckBox cchInsecure && cchInsecure.Name == CustomCheckBoxInsecure.Name)
            {
                AppSettings.AddSetting(CustomCheckBoxInsecure, nameof(CustomCheckBoxInsecure.Checked), CustomCheckBoxInsecure.Checked);
            }

            if (sender is CustomRadioButton crbConnectDNSCrypt && crbConnectDNSCrypt.Name == CustomRadioButtonConnectDNSCrypt.Name)
            {
                CustomTextBoxHTTPProxy.Enabled = crbConnectDNSCrypt.Checked;
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
            // Write Closing message to log
            string msg = "Exiting..." + NL;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.LightGray));

            // Hide NotifyIcon
            NotifyIconMain.Visible = false;

            // Disconnect
            if (IsConnected)
                CustomButtonConnect_Click(null, null);

            // Stop Cloudflare Bypass
            BypassCloudflareStop(true, true, true, false);

            // Unset Saved DNS
            UnsetSavedDNS();

            // Stop HTTP Proxy
            if (HTTPProxy != null && HTTPProxy.IsRunning)
                HTTPProxy.Stop();

            // Unset Proxy
            Network.UnsetProxy(false);

            // Kill processes and set DNS to dynamic
            KillAll();

            // Flush DNS On Exit
            FlushDnsOnExit();

            // Select Control type and properties to save
            AppSettings.AddSelectedControlAndProperty(typeof(CustomCheckBox), "Checked");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomNumericUpDown), "Value");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomRadioButton), "Checked");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomTextBox), "Text");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomTextBox), "Texts");

            // Add Settings to save
            AppSettings.AddSelectedSettings(this);

            // Save Application Settings
            await AppSettings.SaveAsync(SecureDNS.SettingsXmlPath);

            // Exit
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