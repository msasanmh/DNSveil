using System.Net;
using SecureDNSClient;
using System.Diagnostics;
using System.Net.Sockets;
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

namespace SecureDNSClient
{
    public partial class FormMain : Form
    {
        public List<Tuple<long, string>> WorkingDnsList = new();
        private List<string> WorkingDnsListToFile = new();
        private bool CheckingStarted = false;
        private bool StopChecking = false;
        private bool CheckIsDone = false;
        private bool IsConnecting = false;
        private bool IsDNSSet = false;
        private bool IsConnected = false;
        private bool IsDPIActive = false;
        private bool IsDNSConnected = false;
        private bool IsDoHConnected = false;
        private bool ConnectAllClicked = false;
        private int ConnectPID = -1;
        private StringBuilder LiveStatusDNSResult = new();
        private StringBuilder LiveStatusDoHResult = new();
        private int NumberOfWorkingServers = 0;
        private IPAddress? LocalIP = IPAddress.Loopback; // as default
        public Settings AppSettings;
        private ToolStripMenuItem ToolStripMenuItemIcon = new();
        private HTTPProxyServer? HTTPProxy;
        private bool AudioAlertOnline = true;
        private bool AudioAlertOffline = false;

        public FormMain()
        {
            InitializeComponent();
            CustomStatusStrip1.SizingGrip = false;
            
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
            string msgHTTPProxy = "To share DPI + DNS over network (experimental).";
            CustomCheckBoxHTTPProxy.SetToolTip("Info", msgHTTPProxy);

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

            // Initialize Settings
            if (File.Exists(SecureDNS.SettingsXmlPath) && Xml.IsValidXMLFile(SecureDNS.SettingsXmlPath))
                AppSettings = new(SecureDNS.SettingsXmlPath);
            else
                AppSettings = new();

            // Load Settings
            AppSettings.LoadAllSettings(this);

            // Update NICs
            SecureDNS.UpdateNICs(CustomComboBoxNICs);

            UpdateBools();
            UpdateStatusShort();
            UpdateStatusLong();

            Shown += FormMain_Shown;
        }

        private async void FormMain_Shown(object? sender, EventArgs e)
        {
            // Write binaries if not exist
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
            CustomCheckBoxDNSCrypt.Checked = false;
            CustomTextBoxHTTPProxy.Text = string.Empty;
            CustomCheckBoxHTTPProxy.Checked = false;
            CustomNumericUpDownHTTPProxyPort.Value = (decimal)8080;
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
            CustomNumericUpDownSSLFragment.Value = (decimal)40;
            // DPI Advanced
            CustomCheckBoxDPIAdvP.Checked = true;
            CustomCheckBoxDPIAdvR.Checked = true;
            CustomCheckBoxDPIAdvS.Checked = true;
            CustomCheckBoxDPIAdvM.Checked = true;
            CustomCheckBoxDPIAdvF.Checked = true;
            CustomNumericUpDownDPIAdvF.Value = (decimal)2;
            CustomCheckBoxDPIAdvK.Checked = false;
            CustomNumericUpDownDPIAdvK.Value = (decimal)2;
            CustomCheckBoxDPIAdvN.Checked = false;
            CustomCheckBoxDPIAdvE.Checked = true;
            CustomNumericUpDownDPIAdvE.Value = (decimal)40;
            CustomCheckBoxDPIAdvA.Checked = false;
            CustomCheckBoxDPIAdvW.Checked = true;
            CustomCheckBoxDPIAdvPort.Checked = false;
            CustomNumericUpDownDPIAdvPort.Value = (decimal)8085;
            CustomCheckBoxDPIAdvIpId.Checked = false;
            CustomTextBoxDPIAdvIpId.Text = string.Empty;
            CustomCheckBoxDPIAdvAllowNoSNI.Checked = true;
            CustomCheckBoxDPIAdvSetTTL.Checked = false;
            CustomNumericUpDownDPIAdvSetTTL.Value = (decimal)1;
            CustomCheckBoxDPIAdvAutoTTL.Checked = true;
            CustomTextBoxDPIAdvAutoTTL.Text = string.Empty;
            CustomCheckBoxDPIAdvMinTTL.Checked = false;
            CustomNumericUpDownDPIAdvMinTTL.Value = (decimal)2;
            CustomCheckBoxDPIAdvWrongChksum.Checked = true;
            CustomCheckBoxDPIAdvWrongSeq.Checked = true;
            CustomCheckBoxDPIAdvNativeFrag.Checked = false;
            CustomCheckBoxDPIAdvReverseFrag.Checked = false;
            CustomCheckBoxDPIAdvMaxPayload.Checked = true;
            CustomNumericUpDownDPIAdvMaxPayload.Value = (decimal)1200;

            // Settings
            CustomNumericUpDownCheckTimeout.Value = (decimal)5;
            CustomTextBoxBootstrapDNS.Text = "8.8.8.8";
            CustomCheckBoxDontAskCertificate.Checked = false;
            CustomCheckBoxSettingDisableAudioAlert.Checked = false;
        }

        //============================== Methods

        private bool IsInternetAlive()
        {
            if (!Network.IsInternetAlive())
            {
                string msgNet = "There is no Internet connectivity." + Environment.NewLine;
                CustomRichTextBoxLog.AppendText(msgNet, Color.IndianRed);
                return false;
            }
            else
                return true;
        }

        private void UpdateBools()
        {
            System.Timers.Timer updateBoolsTimer = new();
            updateBoolsTimer.Interval = 5000;
            updateBoolsTimer.Elapsed += (s, e) =>
            {
                // Update bool IsConnected
                if (!CustomCheckBoxDNSCrypt.Checked)
                    IsConnected = ProcessManager.FindProcessByName("dnsproxy");
                else
                    IsConnected = ProcessManager.FindProcessByName("dnscrypt-proxy");

                // Update bool IsDPIActive
                IsDPIActive = ProcessManager.FindProcessByName("goodbyedpi");

                // Update bool IsDNSConnected and IsDoHConnected
                if (IsConnected)
                    UpdateBoolDNSDoH();
                else
                {
                    // In case dnsproxy process terminated
                    IsDNSConnected = IsConnected;
                    IsDoHConnected = IsConnected;
                    if (HTTPProxy != null && HTTPProxy.IsRunning)
                        HTTPProxy.Stop();
                }
            };
            updateBoolsTimer.Start();
        }

        private void UpdateBoolDNSDoH()
        {
            // Update bool IsDNSConnected
            string dnsArgs = "google.com " + IPAddress.Loopback.ToString();
            process(dnsArgs, LiveStatusDNSResult);
            IsDNSConnected = LiveStatusDNSResult.ToString().Contains("ANSWER SECTION");
            LiveStatusDNSResult.Clear();
            
            // Update bool IsDoHConnected
            string dohArgs = "google.com https://" + IPAddress.Loopback.ToString() + "/dns-query";
            process(dohArgs, LiveStatusDoHResult);
            IsDoHConnected = LiveStatusDoHResult.ToString().Contains("ANSWER SECTION");
            LiveStatusDoHResult.Clear();

            void process(string args, StringBuilder stringBuilder)
            {
                Task.Run(() =>
                {
                    Process process = new();
                    process.StartInfo.FileName = SecureDNS.DnsLookup;
                    process.StartInfo.Arguments = args;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.WorkingDirectory = Info.CurrentPath;

                    process.OutputDataReceived += (sender, args) =>
                    {
                        stringBuilder.AppendLine(args.Data);
                    };

                    try
                    {
                        process.Start();
                        process.BeginOutputReadLine();
                        process.WaitForExit(2000);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                });
            }
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
                if (!CustomCheckBoxDNSCrypt.Checked)
                {
                    if (WorkingDnsList.Any() && CheckIsDone && !IsConnecting)
                    {
                        CustomButtonConnect.Enabled = true;
                    }
                    else
                    {
                        CustomButtonConnect.Enabled = IsConnected;
                    }
                }
                else
                {
                    CustomButtonConnect.Enabled = true;
                }

                // Check Button
                CustomButtonCheck.Enabled = !IsConnecting;

                // SetDNS Button
                if (IsConnected)
                    CustomButtonSetDNS.Enabled = true;

                // Live Status
                ToolStripStatusLabelDNS.Text = IsDNSConnected ? "DNS status: Online." : "DNS status: Offline.";
                ToolStripStatusLabelDoH.Text = IsDoHConnected ? "DoH status: Online." : "DoH status: Offline.";
                
                if (!CustomCheckBoxSettingDisableAudioAlert.Checked)
                    PlayAudioAlert();
            };
            timer.Start();
        }

        private void UpdateStatusLong()
        {
            System.Windows.Forms.Timer timer = new();
            timer.Interval = 5000;
            timer.Tick += (s, e) =>
            {
                UpdateStatus();
            };
            timer.Start();
        }

        private void UpdateStatus()
        {
            // Update Status Connected
            string textConnect = IsConnected ? "Yes" : "No";
            Color colorConnect = IsConnected ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomRichTextBoxStatusIsConnected.ResetText());
            this.InvokeIt(() => CustomRichTextBoxStatusIsConnected.AppendText("Is Connected: ", ForeColor));
            this.InvokeIt(() => CustomRichTextBoxStatusIsConnected.AppendText(textConnect, colorConnect));

            // Update Status DPI
            string textDPI = IsDPIActive ? "Yes" : "No";
            Color colorDPI = IsDPIActive ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomRichTextBoxStatusIsDPIActive.ResetText());
            this.InvokeIt(() => CustomRichTextBoxStatusIsDPIActive.AppendText("Is DPI Active: ", ForeColor));
            this.InvokeIt(() => CustomRichTextBoxStatusIsDPIActive.AppendText(textDPI, colorDPI));

            // Update Status DNS
            string textDNS = IsDNSSet ? "Yes" : "No";
            Color colorDNS = IsDNSSet ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomRichTextBoxStatusIsDNSSet.ResetText());
            this.InvokeIt(() => CustomRichTextBoxStatusIsDNSSet.AppendText("Is DNS Set: ", ForeColor));
            this.InvokeIt(() => CustomRichTextBoxStatusIsDNSSet.AppendText(textDNS, colorDNS));

            // Update Status HTTP Proxy Server
            bool isHTTPProxyRunning = false;
            void updateStatusHTTPProxyServer(bool isHTTPProxyRunning)
            {
                string textHTTPProxy = isHTTPProxyRunning ? "Yes" : "No";
                Color colorHTTPProxy = isHTTPProxyRunning ? Color.MediumSeaGreen : Color.IndianRed;
                this.InvokeIt(() => CustomRichTextBoxStatusIsProxyServerRunning.ResetText());
                this.InvokeIt(() => CustomRichTextBoxStatusIsProxyServerRunning.AppendText("Is Proxy Server Running: ", ForeColor));
                this.InvokeIt(() => CustomRichTextBoxStatusIsProxyServerRunning.AppendText(textHTTPProxy, colorHTTPProxy));
            }
            if (HTTPProxy != null)
            {
                if (HTTPProxy.IsRunning) isHTTPProxyRunning = true;
                updateStatusHTTPProxyServer(isHTTPProxyRunning);
            }
            else
                updateStatusHTTPProxyServer(isHTTPProxyRunning);
        }

        private void PlayAudioAlert()
        {
            if (IsDNSConnected && AudioAlertOnline)
            {
                AudioAlertOnline = false;
                AudioAlertOffline = true;

                SoundPlayer soundPlayer = new(Audio.Resource1.DNS_Online);
                soundPlayer.Play();
                soundPlayer.Dispose();
            }

            if (!IsDNSConnected && AudioAlertOffline)
            {
                AudioAlertOffline = false;
                AudioAlertOnline = true;

                int softEtherPID = -1;
                if (ProcessManager.FindProcessByName("vpnclient_x64"))
                    softEtherPID = ProcessManager.GetFirstPIDByName("vpnclient_x64");

                if (softEtherPID != -1)
                    ProcessManager.SuspendProcess(softEtherPID); // On net disconnect SoftEther cause noise to audio.

                Task.Delay(1000).Wait();
                SoundPlayer soundPlayer = new(Audio.Resource1.DNS_Offline);
                soundPlayer.Play();
                soundPlayer.Dispose();
                Task.Delay(5000).Wait();

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
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(flushDNS + Environment.NewLine, Color.LightGray));
            }
        }

        private void KillAll()
        {
            UnsetSavedDNS();
            if (ProcessManager.FindProcessByName("dnslookup"))
                ProcessManager.KillProcessByName("dnslookup");
            if (ProcessManager.FindProcessByName("dnsproxy"))
                ProcessManager.KillProcessByName("dnsproxy");
            if (ProcessManager.FindProcessByName("dnscrypt-proxy"))
                ProcessManager.KillProcessByName("dnscrypt-proxy");
            if (ProcessManager.FindProcessByName("goodbyedpi"))
                ProcessManager.KillProcessByName("goodbyedpi");
        }

        private void UnsetSavedDNS()
        {
            if (File.Exists(SecureDNS.NicNamePath))
            {
                string nicName = File.ReadAllText(SecureDNS.NicNamePath).Replace(Environment.NewLine, string.Empty);
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

        private bool CheckNecessaryFiles(bool showMessage = true)
        {
            if (!File.Exists(SecureDNS.DnsLookup) || !File.Exists(SecureDNS.DnsProxy) || !File.Exists(SecureDNS.DNSCrypt) ||
                !File.Exists(SecureDNS.DNSCryptConfigPath) || !File.Exists(SecureDNS.GoodbyeDpi) || !File.Exists(SecureDNS.WinDivert)
                 || !File.Exists(SecureDNS.WinDivert32) || !File.Exists(SecureDNS.WinDivert64))
            {
                if (showMessage)
                {
                    string msg = "ERROR: Some of binary files are missing!" + Environment.NewLine;
                    CustomRichTextBoxLog.AppendText(msg, Color.IndianRed);
                }
                return false;
            }
            else
                return true;
        }

        private async Task WriteNecessaryFilesToDisk()
        {
            if (!CheckNecessaryFiles(false))
            {
                string msg1 = "Creating binaries. Please Wait..." + Environment.NewLine;
                CustomRichTextBoxLog.AppendText(msg1, Color.LightGray);

                if (!Directory.Exists(SecureDNS.BinaryDirPath))
                    Directory.CreateDirectory(SecureDNS.BinaryDirPath);
                if (!File.Exists(SecureDNS.DnsLookup))
                    await Resource.WriteResourceToFileAsync(NecessaryFiles.Resource1.dnslookup, SecureDNS.DnsLookup);
                if (!File.Exists(SecureDNS.DnsProxy))
                    await Resource.WriteResourceToFileAsync(NecessaryFiles.Resource1.dnsproxy, SecureDNS.DnsProxy);
                if (!File.Exists(SecureDNS.DNSCrypt))
                    await Resource.WriteResourceToFileAsync(NecessaryFiles.Resource1.dnscrypt_proxyEXE, SecureDNS.DNSCrypt);
                if (!File.Exists(SecureDNS.DNSCryptConfigPath))
                    await Resource.WriteResourceToFileAsync(NecessaryFiles.Resource1.dnscrypt_proxyTOML, SecureDNS.DNSCryptConfigPath);
                if (!File.Exists(SecureDNS.GoodbyeDpi))
                    await Resource.WriteResourceToFileAsync(NecessaryFiles.Resource1.goodbyedpi, SecureDNS.GoodbyeDpi);
                if (!File.Exists(SecureDNS.WinDivert))
                    await Resource.WriteResourceToFileAsync(NecessaryFiles.Resource1.WinDivert, SecureDNS.WinDivert);
                if (!File.Exists(SecureDNS.WinDivert32))
                    await Resource.WriteResourceToFileAsync(NecessaryFiles.Resource1.WinDivert32, SecureDNS.WinDivert32);
                if (!File.Exists(SecureDNS.WinDivert64))
                    await Resource.WriteResourceToFileAsync(NecessaryFiles.Resource1.WinDivert64, SecureDNS.WinDivert64);

                string msg2 = $"{Info.InfoExecutingAssembly.ProductName} is ready.{Environment.NewLine}";
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
            if (File.Exists(SecureDNS.IssuerCertPath) && !SettingsWindow.GetDontAskAboutCertificate(CustomCheckBoxDontAskCertificate))
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

        //============================== Check
        private async Task CheckServers()
        {
            // Check servers comment
            string checkingServers = "Checking servers:" + Environment.NewLine + Environment.NewLine;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(checkingServers, Color.MediumSeaGreen));

            // Built-in or Custom
            bool builtInMode = CustomRadioButtonBuiltIn.Checked;

            string? fileContent = string.Empty;
            if (builtInMode)
                fileContent = Resource.GetResourceTextFile("SecureDNSClient.DNS-Servers.txt");
            else
            {
                FileDirectory.CreateEmptyFile(SecureDNS.CustomServersPath);
                fileContent = await File.ReadAllTextAsync(SecureDNS.CustomServersPath);

                // Load saved working servers
                WorkingDnsListToFile.LoadFromFile(SecureDNS.WorkingServersPath);
            }

            // Check if servers exist 1
            if (string.IsNullOrEmpty(fileContent) || string.IsNullOrWhiteSpace(fileContent))
            {
                string msg = "Servers list is empty." + Environment.NewLine;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return;
            }

            // Clear working list on new check
            WorkingDnsList.Clear();

            // Add Servers to list
            List<string> dnsList = fileContent.SplitToLines();
            int dnsCount = dnsList.Count;

            // Check if servers exist 2
            if (dnsCount < 1)
            {
                string msg = "Servers list is empty." + Environment.NewLine;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return;
            }

            for (int n = 0; n < dnsCount; n++)
            {
                if (StopChecking) return;

                // Percentage
                int persent = n * 100 / dnsCount;
                this.InvokeIt(() => ToolStripStatusLabelPercent.Text = persent.ToString() + "%");

                string dns = dnsList[n];

                if (!string.IsNullOrEmpty(dns) && !string.IsNullOrWhiteSpace(dns) && dns.Contains("//"))
                {
                    // Get Check timeout value
                    int timeout = int.Parse(CustomNumericUpDownCheckTimeout.Value.ToString());

                    // Get Status and Latency
                    Stopwatch stopwatch = new();
                    stopwatch.Start();
                    bool dnsOK = false;
                    if (!CustomCheckBoxInsecure.Checked)
                        dnsOK = SecureDNS.CheckDoH("facebook.com", dns, timeout);
                    else
                        dnsOK = SecureDNS.CheckDoHInsecure("facebook.com", dns, timeout, CustomTextBoxBootstrapDNS);
                    stopwatch.Stop();
                    var latency = stopwatch.ElapsedMilliseconds;

                    if (StopChecking) return;

                    // Write status to log
                    string status = dnsOK ? "OK" : "Faild";
                    Color color = dnsOK ? Color.MediumSeaGreen : Color.IndianRed;
                    object resultStatus = "[" + status + "]";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultStatus.ToString().IsNotNull(), color));

                    // Write latency to log
                    object resultLatency = " " + "[" + latency + "]";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultLatency.ToString().IsNotNull(), Color.DodgerBlue));

                    // Write host to log
                    object resultHost = " " + dns;
                    if (!builtInMode)
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultHost.ToString().IsNotNull(), Color.LightGray));

                    // Write company name to log
                    object company = await SecureDNS.UrlToCompanyOffline(dns);
                    object resultCompany = " " + "[" + company + "]";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultCompany.ToString().IsNotNull(), Color.Gray));

                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(Environment.NewLine));

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
                string noWorkingServer = Environment.NewLine + "There is no working server." + Environment.NewLine;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(noWorkingServer, Color.IndianRed));
                return;
            }

            if (StopChecking) return;

            // Sort by latency comment
            string allWorkingServers = Environment.NewLine + "All working servers sorted by latency:" + Environment.NewLine + Environment.NewLine;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(allWorkingServers, Color.MediumSeaGreen));

            // Sort by latency
            if (WorkingDnsList.Count > 1)
                WorkingDnsList = WorkingDnsList.OrderByDescending(t => t.Item1).ToList();

            // write sorted result to log
            for (int n = 0; n < WorkingDnsList.Count; n++)
            {
                if (StopChecking) return;

                var dns = WorkingDnsList[n];
                long latency = dns.Item1;
                object host = dns.Item2;
                object company = await SecureDNS.UrlToCompanyOffline(dns.Item2);

                // Write latency to log
                object resultLatency1 = "[Latency:";
                object resultLatency2 = " " + latency;
                object resultLatency3 = " ms]";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultLatency1.ToString().IsNotNull(), Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultLatency2.ToString().IsNotNull(), Color.DodgerBlue));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultLatency3.ToString().IsNotNull(), Color.LightGray));

                // Write host to log
                if (!builtInMode)
                {
                    object resultHost1 = " [Host:";
                    object resultHost2 = " " + host;
                    object resultHost3 = "]";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultHost1.ToString().IsNotNull(), Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultHost2.ToString().IsNotNull(), Color.DodgerBlue));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultHost3.ToString().IsNotNull(), Color.LightGray));
                }

                // Write company name to log
                object resultCompany1 = " [Company:";
                object resultCompany2 = " " + company;
                object resultCompany3 = "]";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultCompany1.ToString().IsNotNull(), Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultCompany2.ToString().IsNotNull(), Color.DodgerBlue));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultCompany3.ToString().IsNotNull(), Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(Environment.NewLine));
            }
        }

        //============================== Connect
        private void Connect()
        {
            // Write Connecting message to log
            string msgConnecting = "Connecting... Please wait..." + Environment.NewLine;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnecting, Color.MediumSeaGreen));

            bool startHTTPProxy = CustomCheckBoxHTTPProxy.Checked;

            // Check open ports
            bool port53 = Network.IsPortOpen(IPAddress.Loopback.ToString(), 53, 3);
            bool port443 = Network.IsPortOpen(IPAddress.Loopback.ToString(), 443, 3);
            if (port53)
            {
                string msg = "Port 53 is occupied. Is SecureDNS already running?!" + Environment.NewLine;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                IsConnecting = false;
                return;
            }
            if (port443)
            {
                string msg = "Port 443 is occupied. Is SecureDNS already running?!" + Environment.NewLine;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                IsConnecting = false;
                return;
            }

            if (startHTTPProxy)
            {
                bool portHTTPProxy = Network.IsPortOpen(IPAddress.Loopback.ToString(), SettingsWindow.GetHTTPProxyPort(CustomNumericUpDownHTTPProxyPort), 3);
                if (portHTTPProxy)
                {
                    string msg = "Port " + SettingsWindow.GetHTTPProxyPort(CustomNumericUpDownHTTPProxyPort) + " is occupied. Change the port from Connect tab." + Environment.NewLine;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                    IsConnecting = false;
                    return;
                }
            }
            
            // Flush DNS
            FlushDNS();

            // Generate Certificate
            GenerateCertificate();

            // Connect DNSProxy or DNSCryptProxy
            if (CustomCheckBoxDNSCrypt.Checked)
            {
                // Connect DNSCryptProxy
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
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + Environment.NewLine, Color.IndianRed));
                }
            }
            else
            {
                // Connect DNSProxy
                ConnectDNSProxy();
                if (ProcessManager.FindProcessByName("dnsproxy"))
                {
                    // Connected with DNSProxy
                    internalConnect();

                    // Write Connected message to log
                    ConnectMessage();
                }
                else
                {
                    // Write DNSProxy Error to log
                    string msg = "DNSProxy couldn't connect, try again.";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + Environment.NewLine, Color.IndianRed));
                }
            }
            
            void internalConnect()
            {
                // Start HTTP Proxy
                if (startHTTPProxy)
                {
                    HTTPProxy = new(IPAddress.Any, SettingsWindow.GetHTTPProxyPort(CustomNumericUpDownHTTPProxyPort), 3);
                    HTTPProxy.AutoClean = true;
                    if (!HTTPProxy.IsRunning)
                    {
                        HTTPProxy.Start();
                        HTTPProxy.OnErrorOccurred += HTTPProxy_OnErrorOccurred;

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
                                Debug.WriteLine($"Delete log file: {ex.Message}");
                            }
                        }

                        void HTTPProxy_OnErrorOccurred(object? sender, EventArgs e)
                        {
                            if (sender is string error)
                            {
                                error += Environment.NewLine; // Adding an additional line break.
                                FileDirectory.AppendTextLine(SecureDNS.HTTPProxyServerErrorLogPath, error, new UTF8Encoding(false));
                            }
                        }

                        // Write HTTP Proxy Server to log
                        LocalIP = Network.GetLocalIPv4(); // Update Local IP
                        IPAddress localIP = LocalIP ?? IPAddress.Loopback;
                        string msgHTTPProxy1 = "Local HTTP Proxy:" + Environment.NewLine;
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgHTTPProxy1, Color.LightGray));
                        string msgHTTPProxy2 = $"http://{localIP}:{SettingsWindow.GetHTTPProxyPort(CustomNumericUpDownHTTPProxyPort)}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgHTTPProxy2 + Environment.NewLine, Color.DodgerBlue));
                    }
                }

                // To see online status immediately
                UpdateBoolDNSDoH();

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

            void ConnectMessage()
            {
                // Update Local IP
                LocalIP = Network.GetLocalIPv4();

                // Get Loopback IP
                IPAddress loopbackIP = IPAddress.Loopback;

                // Write local DNS addresses to log
                string msgLocalDNS1 = "Local DNS Proxy:";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDNS1, Color.LightGray));
                string msgLocalDNS2 = Environment.NewLine + loopbackIP;
                if (LocalIP != null)
                    msgLocalDNS2 += Environment.NewLine + LocalIP.ToString();
                msgLocalDNS2 += Environment.NewLine;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDNS2, Color.DodgerBlue));

                // Write local DoH addresses to log
                string msgLocalDoH1 = "Local DNS Over HTTPS:";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDoH1, Color.LightGray));
                string msgLocalDoH2 = Environment.NewLine + "https://" + loopbackIP.ToString() + "/dns-query";
                if (LocalIP != null)
                    msgLocalDoH2 += Environment.NewLine + "https://" + LocalIP.ToString() + "/dns-query";
                msgLocalDoH2 += Environment.NewLine;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDoH2, Color.DodgerBlue));
            }
        }

        private void ConnectDNSProxy()
        {
            // Write Check first to log
            if (NumberOfWorkingServers < 1)
            {
                string msgCheck = "Check servers first." + Environment.NewLine;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCheck, Color.IndianRed));
                return;
            }

            string bootstrap = SettingsWindow.GetBootstrapDNS(CustomTextBoxBootstrapDNS).ToString();
            string hosts = string.Empty;
            int countUsingServers = 0;

            // Sort by latency
            if (WorkingDnsList.Count > 1)
                WorkingDnsList = WorkingDnsList.OrderByDescending(t => t.Item1).ToList();

            // Find fastest servers, max 10
            for (int n = 0; n < WorkingDnsList.Count; n++)
            {
                Tuple<long, string> latencyHost = WorkingDnsList[n];
                long latency = latencyHost.Item1;
                string host = latencyHost.Item2;

                hosts += " -u " + host;

                countUsingServers = n + 1;
                if (n >= 9) break;
            }

            // Start dnsproxy
            string dnsproxyArgs = "-l 0.0.0.0 -p 53";

            // Add DoH args
            if (File.Exists(SecureDNS.CertPath) && File.Exists(SecureDNS.KeyPath) && !CustomCheckBoxDontAskCertificate.Checked)
                dnsproxyArgs += " --https-port=443 --tls-crt=\"" + SecureDNS.CertPath + "\" --tls-key=\"" + SecureDNS.KeyPath + "\"";

            // Add Insecure
            if (CustomCheckBoxInsecure.Checked)
                dnsproxyArgs += " --insecure";

            // Add hosts
            dnsproxyArgs += hosts;
            if (countUsingServers > 1)
                dnsproxyArgs += " --all-servers -b " + bootstrap + ":53";
            else
                dnsproxyArgs += " -b " + bootstrap + ":53";

            // Execute DnsProxy
            ConnectPID = ProcessManager.ExecuteOnly(SecureDNS.DnsProxy, dnsproxyArgs, true, false, Info.CurrentPath, ProcessPriorityClass.AboveNormal);

            // Write dnsproxy message to log
            object MsgDnsProxy = string.Empty;
            if (countUsingServers > 1)
                MsgDnsProxy = "Local DNS Server started using " + countUsingServers + " fastest servers in parallel." + Environment.NewLine;
            else
                MsgDnsProxy = "Local DNS Server started using " + countUsingServers + " server." + Environment.NewLine;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(MsgDnsProxy.ToString().IsNotNull(), Color.MediumSeaGreen));

        }

        private async void ConnectDNSCrypt()
        {
            if (!CustomCheckBoxDNSCrypt.Checked) return;
            string? proxyScheme = CustomTextBoxHTTPProxy.Text;

            // Check if proxy scheme is correct
            if (string.IsNullOrWhiteSpace(proxyScheme) || !proxyScheme.Contains("//"))
            {
                string msgWrongProxy = "HTTP(S) proxy scheme must be like: \"https://myproxy.net:8080\"";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgWrongProxy + Environment.NewLine, Color.IndianRed));
                return;
            }

            // Check if config file exist
            if (!File.Exists(SecureDNS.DNSCryptConfigPath))
            {
                string msg = "Error: Configuration file doesn't exist";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + Environment.NewLine, Color.IndianRed));
                return;
            }

            IPAddress bootstrap = SettingsWindow.GetBootstrapDNS(CustomTextBoxBootstrapDNS);

            // Edit DNSCrypt Config File
            DNSCryptConfigEditor dnsCryptConfig = new(SecureDNS.DNSCryptConfigPath);
            dnsCryptConfig.EditHTTPProxy(proxyScheme);
            dnsCryptConfig.EditBootstrapDNS(bootstrap);
            if (File.Exists(SecureDNS.CertPath) && File.Exists(SecureDNS.KeyPath) && !CustomCheckBoxDontAskCertificate.Checked)
            {
                dnsCryptConfig.EnableDoH();
                dnsCryptConfig.EditCertKeyPath(SecureDNS.KeyPath);
                dnsCryptConfig.EditCertPath(SecureDNS.CertPath);
            }
            else
            {
                dnsCryptConfig.DisableDoH();
            }
                
            await dnsCryptConfig.WriteAsync();

            // Execute DNSCrypt
            Process process = new();
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
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(data + Environment.NewLine, Color.LightGray));
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                // DNSCrypt writes its output data in error event!
                string? data = args.Data;
                if (data != null)
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(data + Environment.NewLine, Color.LightGray));
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                ConnectPID = process.Id;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        //============================== DPI
        private void DPIBasic()
        {
            // Write Connect first to log
            if (!IsConnected)
            {
                string msgConnect = "Connect first." + Environment.NewLine;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnect, Color.IndianRed));
                return;
            }

            // Check Internet Connectivity
            if (!IsInternetAlive()) return;

            // Kill GoodbyeDPI
            if (ProcessManager.FindProcessByName("goodbyedpi"))
                ProcessManager.KillProcessByName("goodbyedpi");

            string args = string.Empty;
            string text = string.Empty;
            string fallBackDNS = SettingsWindow.GetBootstrapDNS(CustomTextBoxBootstrapDNS).ToString();

            if (CustomRadioButtonDPIMode1.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Mode1, CustomNumericUpDownSSLFragment.Value, fallBackDNS);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIMode2.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Mode2, CustomNumericUpDownSSLFragment.Value, fallBackDNS);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIMode3.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Mode3, CustomNumericUpDownSSLFragment.Value, fallBackDNS);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIMode4.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Mode4, CustomNumericUpDownSSLFragment.Value, fallBackDNS);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIMode5.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Mode5, CustomNumericUpDownSSLFragment.Value, fallBackDNS);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIMode6.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Mode6, CustomNumericUpDownSSLFragment.Value, fallBackDNS);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIModeLight.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Light, CustomNumericUpDownSSLFragment.Value, fallBackDNS);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIModeMedium.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Medium, CustomNumericUpDownSSLFragment.Value, fallBackDNS);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIModeHigh.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.High, CustomNumericUpDownSSLFragment.Value, fallBackDNS);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIModeExtreme.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Extreme, CustomNumericUpDownSSLFragment.Value, fallBackDNS);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }

            // Execute GoodByeDPI
            ProcessManager.ExecuteOnly(SecureDNS.GoodbyeDpi, args, true, true, Path.GetFullPath(Path.Combine(Info.CurrentPath, "binary")), ProcessPriorityClass.High);

            if (ProcessManager.FindProcessByName("goodbyedpi"))
            {
                // Write DPI Mode to log
                string msg = "DPI bypass is active, mode: ";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(text + Environment.NewLine, Color.DodgerBlue));

                // Update Groupbox Status
                UpdateStatus();

                // Go to SetDNS Tab if it's not already set
                if (ConnectAllClicked && !IsDNSSet)
                {
                    this.InvokeIt(() => CustomTabControlMain.SelectedIndex = 0);
                    this.InvokeIt(() => CustomTabControlSecureDNS.SelectedIndex = 3);
                }
            }
            else
            {
                // Write DPI Error to log
                string msg = "DPI bypass couldn't connect, try again.";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + Environment.NewLine, Color.IndianRed));
            }
        }

        private void DPIAdvanced()
        {
            // Write Connect first to log
            if (!IsConnected)
            {
                string msgConnect = "Connect first." + Environment.NewLine;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnect, Color.IndianRed));
                return;
            }

            // Check Internet Connectivity
            if (!IsInternetAlive()) return;

            // Write IP Error to log
            if (CustomCheckBoxDPIAdvIpId.Checked)
            {
                bool isIpValid = Network.IsIPv4Valid(CustomTextBoxDPIAdvIpId.Text, out IPAddress? tempIP);
                if (!isIpValid)
                {
                    string msgIp = "IP Address is not valid." + Environment.NewLine;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgIp, Color.IndianRed));
                    return;
                }
            }
            
            // Write Blacklist file Error to log
            if (CustomCheckBoxDPIAdvBlacklist.Checked)
            {
                if (!File.Exists(SecureDNS.DPIBlacklistPath))
                {
                    string msgError = "Blacklist file not exist." + Environment.NewLine;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgError, Color.IndianRed));
                    return;
                }
                else
                {
                    string content = File.ReadAllText(SecureDNS.DPIBlacklistPath);
                    if (content.Length < 1 || string.IsNullOrWhiteSpace(content))
                    {
                        string msgError = "Blacklist file is empty." + Environment.NewLine;
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
                args += "-f " + CustomNumericUpDownDPIAdvF.Value.ToString() + " "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvK.Checked)
            {
                args += "-k " + CustomNumericUpDownDPIAdvK.Value.ToString() + " "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvN.Checked)
            {
                args += "-n "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvE.Checked)
            {
                args += "-e " + CustomNumericUpDownDPIAdvE.Value.ToString() + " "; checkCount++;
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
                args += "--port " + CustomNumericUpDownDPIAdvPort.Value.ToString() + " "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvIpId.Checked)
            {
                IPAddress ip = IPAddress.Parse(CustomTextBoxDPIAdvIpId.Text);
                args += "--ip-id " + ip.ToString() + " "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvAllowNoSNI.Checked)
            {
                args += "--allow-no-sni "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvSetTTL.Checked)
            {
                args += "--set-ttl " + CustomNumericUpDownDPIAdvSetTTL.Value.ToString() + " "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvAutoTTL.Checked)
            {
                args += "--auto-ttl "; checkCount++;
                if (CustomTextBoxDPIAdvAutoTTL.Text.Length > 0 && !string.IsNullOrWhiteSpace(CustomTextBoxDPIAdvAutoTTL.Text))
                    args += CustomTextBoxDPIAdvAutoTTL.Text + " ";
            }
            if (CustomCheckBoxDPIAdvMinTTL.Checked)
            {
                args += "--min-ttl " + CustomNumericUpDownDPIAdvMinTTL.Value.ToString() + " "; checkCount++;
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
                args += "--max-payload " + CustomNumericUpDownDPIAdvMaxPayload.Value.ToString() + " "; checkCount++;
            }
            if (CustomCheckBoxDPIAdvBlacklist.Checked)
            {
                args += "--blacklist " + SecureDNS.DPIBlacklistPath; checkCount++;
            }

            string fallBackDNS = SettingsWindow.GetBootstrapDNS(CustomTextBoxBootstrapDNS).ToString();

            if (checkCount > 0)
            {
                args += "--dns-addr " + fallBackDNS + " --dns-port 53 --dnsv6-addr 2001:4860:4860::8888 --dnsv6-port 53";
            }

            // Write Args Error to log
            if (args.Length < 1 && string.IsNullOrWhiteSpace(args))
            {
                string msgError = "Error occurred: Arguments." + Environment.NewLine;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgError, Color.IndianRed));
                return;
            }

            // Kill GoodbyeDPI
            if (ProcessManager.FindProcessByName("goodbyedpi"))
                ProcessManager.KillProcessByName("goodbyedpi");

            string text = "Advanced";

            // Execute GoodByeDPI
            ProcessManager.ExecuteOnly(SecureDNS.GoodbyeDpi, args, true, true, Path.GetFullPath(Path.Combine(Info.CurrentPath, "binary")), ProcessPriorityClass.High);

            if (ProcessManager.FindProcessByName("goodbyedpi"))
            {
                // Write DPI Mode to log
                string msg = "DPI bypass is active, mode: ";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(text + Environment.NewLine, Color.DodgerBlue));

                // Update Groupbox Status
                UpdateStatus();

                // Go to SetDNS Tab if it's not already set
                if (ConnectAllClicked && !IsDNSSet)
                {
                    this.InvokeIt(() => CustomTabControlMain.SelectedIndex = 0);
                    this.InvokeIt(() => CustomTabControlSecureDNS.SelectedIndex = 3);
                }
            }
            else
            {
                // Write DPI Error to log
                string msg = "DPI bypass couldn't connect, try again.";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + Environment.NewLine, Color.IndianRed));
            }
        }

        private void DPIDeactive()
        {
            if (ProcessManager.FindProcessByName("goodbyedpi"))
            {
                // Kill GoodbyeDPI
                ProcessManager.KillProcessByName("goodbyedpi");

                // Update Groupbox Status
                UpdateStatus();

                // Write to log
                string msgDC = "DPI bypass deactivated." + Environment.NewLine;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDC, Color.LightGray));
            }
        }

        //============================== DNS
        private void SetDNS()
        {
            string? nicName = CustomComboBoxNICs.SelectedItem as string;
            // Check if NIC Name is empty
            if (string.IsNullOrEmpty(nicName))
            {
                string msg = "Select a Network Interface first." + Environment.NewLine;
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
                if (!IsConnected)
                {
                    string msgConnect = "Connect first." + Environment.NewLine;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnect, Color.IndianRed));
                    return;
                }

                // Check Internet Connectivity
                if (!IsInternetAlive()) return;

                // Set DNS
                Network.SetDNS(nic, dnss);
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
                CustomRichTextBoxLog.AppendText(msg4 + Environment.NewLine, Color.DodgerBlue);

                
                // Go to Check Tab
                if (ConnectAllClicked && IsConnected)
                {
                    this.InvokeIt(() => CustomTabControlMain.SelectedIndex = 0);
                    this.InvokeIt(() => CustomTabControlSecureDNS.SelectedIndex = 0);
                    ConnectAllClicked = false;
                }
            }
            else
            {
                // Unset DNS
                Network.UnsetDNS(nic);
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
                CustomRichTextBoxLog.AppendText(msg4 + Environment.NewLine, Color.DodgerBlue);
            }
        }

        //============================== Buttons
        private void CustomButtonEditCustomServers_Click(object sender, EventArgs e)
        {
            FileDirectory.CreateEmptyFile(SecureDNS.CustomServersPath);
            ProcessManager.ExecuteOnly("notepad", SecureDNS.CustomServersPath, false, false, Info.CurrentPath);
        }

        private void CustomButtonViewWorkingServers_Click(object sender, EventArgs e)
        {
            FileDirectory.CreateEmptyFile(SecureDNS.WorkingServersPath);
            ProcessManager.ExecuteOnly("notepad", SecureDNS.WorkingServersPath, false, false, Info.CurrentPath);
        }

        private void CustomButtonCheck_Click(object? sender, EventArgs? e)
        {
            // Return if binary files are missing
            if (!CheckNecessaryFiles()) return;

            if (!CheckingStarted)
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
                                CheckingStarted = true;
                                CheckIsDone = false;
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

                          CheckingStarted = false;
                          CheckIsDone = true;

                          string msg = Environment.NewLine + "Check operation finished." + Environment.NewLine;
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
                string msg = Environment.NewLine + "Canceling Check operation..." + Environment.NewLine;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));
            }
        }

        private async void CustomButtonConnectAll_Click(object sender, EventArgs e)
        {
            if (!CheckingStarted && !IsConnected && !ProcessManager.FindProcessByName("goodbyedpi") && !IsDNSSet)
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
                    while (CheckingStarted)
                    {
                        if (!CheckingStarted)
                            return Task.CompletedTask;
                        await Task.Delay(1000);
                    }
                    return Task.CompletedTask;
                });
                await Task.Delay(1000);
                if (!CheckingStarted)
                {
                    CustomButtonConnect_Click(null, null);
                    Task taskWait2 = await Task.Run(async () =>
                    {
                        while (!IsConnected)
                        {
                            if (NumberOfWorkingServers < 1)
                                return Task.CompletedTask;
                            await Task.Delay(1000);
                        }
                        return Task.CompletedTask;
                    });
                    await Task.Delay(1000);
                    if (IsConnected)
                    {
                        UpdateStatus();
                        if (!ProcessManager.FindProcessByName("goodbyedpi"))
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
                if (CheckingStarted)
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
                        if (CheckingStarted)
                        {
                            CustomButtonCheck_Click(null, null);

                            // Wait until check is done
                            while (!CheckIsDone)
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
                    string msgDisconnecting = "Disconnecting..." + Environment.NewLine;
                    CustomRichTextBoxLog.AppendText(msgDisconnecting, Color.MediumSeaGreen);

                    // Unset DNS
                    if (IsDNSSet)
                        SetDNS();

                    // Deactivate DPI
                    DPIDeactive();

                    // Kill processes (DNSProxy, DNSCrypt)
                    if (ProcessManager.FindProcessByName("dnsproxy"))
                        ProcessManager.KillProcessByName("dnsproxy");
                    if (ProcessManager.FindProcessByName("dnscrypt-proxy"))
                        ProcessManager.KillProcessByName("dnscrypt-proxy");

                    // Stop HTTP Proxy
                    if (HTTPProxy != null && HTTPProxy.IsRunning)
                        HTTPProxy.Stop();

                    Task.Delay(500).Wait();

                    // Write Disconnected message to log
                    string msgDisconnected = "Disconnected." + Environment.NewLine;
                    CustomRichTextBoxLog.AppendText(msgDisconnected, Color.MediumSeaGreen);

                    // Update Groupbox Status
                    UpdateStatus();

                    // Clear Live Status StringBuilder
                    LiveStatusDNSResult.Clear();
                    IsDNSConnected = LiveStatusDNSResult.ToString().Contains("ANSWER SECTION");
                    LiveStatusDoHResult.Clear();
                    IsDoHConnected = LiveStatusDoHResult.ToString().Contains("ANSWER SECTION");
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
            ProcessManager.ExecuteOnly("notepad", SecureDNS.DPIBlacklistPath, false, false, Info.CurrentPath);
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

        private void CustomButtonRestoreDefault_Click(object sender, EventArgs e)
        {
            if (CheckingStarted)
            {
                string msgChecking = "Stop check operation first." + Environment.NewLine;
                CustomRichTextBoxLog.AppendText(msgChecking, Color.IndianRed);
                return;
            }
            
            if (IsConnected)
            {
                string msgConnected = "Disconnect first." + Environment.NewLine;
                CustomRichTextBoxLog.AppendText(msgConnected, Color.IndianRed);
                return;
            }
            
            if (IsDNSSet)
            {
                string msgDNSIsSet = "Unset DNS first." + Environment.NewLine;
                CustomRichTextBoxLog.AppendText(msgDNSIsSet, Color.IndianRed);
                return;
            }

            DefaultSettings();

            string msgDefault = "Settings restored to default." + Environment.NewLine;
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

            if (sender is CustomCheckBox ccbDNSCrypt && ccbDNSCrypt.Name == CustomCheckBoxDNSCrypt.Name)
            {
                CustomTextBoxHTTPProxy.Enabled = ccbDNSCrypt.Checked;

                AppSettings.AddSetting(CustomCheckBoxDNSCrypt, nameof(CustomCheckBoxDNSCrypt.Checked), CustomCheckBoxDNSCrypt.Checked);
                AppSettings.AddSetting(CustomTextBoxHTTPProxy, nameof(CustomTextBoxHTTPProxy.Text), CustomTextBoxHTTPProxy.Text);
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
            string msg = "Exiting..." + Environment.NewLine;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.LightGray));

            // Hide NotifyIcon
            NotifyIconMain.Visible = false;

            // Disconnect
            if (IsConnected)
                CustomButtonConnect_Click(null, null);

            // Stop HTTP Proxy
            if (HTTPProxy != null && HTTPProxy.IsRunning)
                HTTPProxy.Stop();

            // Kill processes and set DNS to dynamic
            KillAll();

            // Select Control type and properties to save
            AppSettings.AddSelectedControlAndProperty(typeof(CustomCheckBox), "Checked");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomNumericUpDown), "Value");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomRadioButton), "Checked");
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
            OpenLinks.OpenUrl("https://github.com/msasanmh/msasanmh.github.io");
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

    }
}