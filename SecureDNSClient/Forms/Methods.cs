using MsmhTools;
using MsmhTools.HTTPProxyServer;
using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;

namespace SecureDNSClient
{
    public partial class FormMain
    {

        public enum ConnectMode
        {
            ConnectToWorkingServers,
            ConnectToFakeProxyDohViaProxyDPI,
            ConnectToFakeProxyDohViaGoodbyeDPI,
            ConnectToPopularServersWithProxy
        }

        public ConnectMode GetConnectMode()
        {
            // Get Connect modes
            bool a = CustomRadioButtonConnectCheckedServers.Checked;
            bool b = CustomRadioButtonConnectFakeProxyDohViaProxyDPI.Checked;
            bool c = CustomRadioButtonConnectFakeProxyDohViaGoodbyeDPI.Checked;
            bool d = CustomRadioButtonConnectDNSCrypt.Checked;

            ConnectMode connectMode = ConnectMode.ConnectToWorkingServers;
            if (a) connectMode = ConnectMode.ConnectToWorkingServers;
            else if (b) connectMode = ConnectMode.ConnectToFakeProxyDohViaProxyDPI;
            else if (c) connectMode = ConnectMode.ConnectToFakeProxyDohViaGoodbyeDPI;
            else if (d) connectMode = ConnectMode.ConnectToPopularServersWithProxy;
            return connectMode;
        }

        private static void MoveToNewLocation()
        {
            try
            {
                if (File.Exists(SecureDNS.OldSettingsXmlPath)) File.Move(SecureDNS.OldSettingsXmlPath, SecureDNS.SettingsXmlPath, false);
                if (File.Exists(SecureDNS.OldSettingsXmlDnsLookup)) File.Move(SecureDNS.OldSettingsXmlDnsLookup, SecureDNS.SettingsXmlDnsLookup, false);
                if (File.Exists(SecureDNS.OldSettingsXmlIpScanner)) File.Move(SecureDNS.OldSettingsXmlIpScanner, SecureDNS.SettingsXmlIpScanner, false);
                if (File.Exists(SecureDNS.OldUserIdPath)) File.Move(SecureDNS.OldUserIdPath, SecureDNS.UserIdPath, false);
                if (File.Exists(SecureDNS.OldFakeDnsRulesPath)) File.Move(SecureDNS.OldFakeDnsRulesPath, SecureDNS.FakeDnsRulesPath, false);
                if (File.Exists(SecureDNS.OldBlackWhiteListPath)) File.Move(SecureDNS.OldBlackWhiteListPath, SecureDNS.BlackWhiteListPath, false);
                if (File.Exists(SecureDNS.OldDontBypassListPath)) File.Move(SecureDNS.OldDontBypassListPath, SecureDNS.DontBypassListPath, false);
                if (File.Exists(SecureDNS.OldCustomServersPath)) File.Move(SecureDNS.OldCustomServersPath, SecureDNS.CustomServersPath, false);
                if (File.Exists(SecureDNS.OldWorkingServersPath)) File.Move(SecureDNS.OldWorkingServersPath, SecureDNS.WorkingServersPath, false);
                if (File.Exists(SecureDNS.OldDPIBlacklistPath)) File.Move(SecureDNS.OldDPIBlacklistPath, SecureDNS.DPIBlacklistPath, false);
                if (File.Exists(SecureDNS.OldNicNamePath)) File.Move(SecureDNS.OldNicNamePath, SecureDNS.NicNamePath, false);
                if (File.Exists(SecureDNS.OldSavedEncodedDnsPath)) File.Move(SecureDNS.OldSavedEncodedDnsPath, SecureDNS.SavedEncodedDnsPath, false);
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        private void FixScreenDPI(Form form)
        {
            using Graphics g = form.CreateGraphics();

            int x1 = 120; int y1 = 21;
            int splitMainD = SplitContainerMain.SplitterDistance;
            int splitTopD = SplitContainerTop.SplitterDistance;

            if (form.AutoScaleDimensions == form.CurrentAutoScaleDimensions)
            {
                // 96 = 100%
                // 120 = 125%
                // 144 = 150%
                if (g.DpiX == 120) // 125%
                {
                    setSize(x1 + 35, y1 + 10, splitMainD, splitTopD + 100);
                }
                else if (g.DpiX == 144) // 150%
                {
                    setSize(x1 + 80, y1 + 20, splitMainD, splitTopD + 450);
                }

                void setSize(int x1, int y1, int splitMainD, int splitTopD)
                {
                    CustomTabControlMain.SizeMode = TabSizeMode.Fixed;
                    CustomTabControlMain.ItemSize = new Size(x1, y1);
                    CustomTabControlSecureDNS.SizeMode = TabSizeMode.Fixed;
                    CustomTabControlSecureDNS.ItemSize = new Size(x1, y1);
                    CustomTabControlDPIBasicAdvanced.SizeMode = TabSizeMode.Fixed;
                    CustomTabControlDPIBasicAdvanced.ItemSize = new Size(x1, y1);
                    CustomTabControlSettings.SizeMode = TabSizeMode.Fixed;
                    CustomTabControlSettings.ItemSize = new Size(y1 + 9, x1);

                    SplitContainerMain.SplitterDistance = splitMainD;
                    SplitContainerTop.SplitterDistance = splitTopD;
                }
            }
        }

        private bool IsInternetAlive(bool writeToLog = true)
        {
            if (!Network.IsInternetAlive())
            {
                string msgNet = "There is no Internet connectivity." + NL;
                if (writeToLog)
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgNet, Color.IndianRed));
                return false;
            }
            else
                return true;
        }

        private void FlushDNS(bool writeToLog = true)
        {
            string? flushDNS = ProcessManager.Execute(out Process _, "ipconfig", "/flushdns");
            if (!string.IsNullOrWhiteSpace(flushDNS) && writeToLog)
            {
                // Write flush DNS message to log
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(flushDNS + NL, Color.LightGray));
            }
        }

        private static void FlushDnsOnExit()
        {
            ProcessManager.Execute(out Process _, "ipconfig", "/flushdns", true, true);
            ProcessManager.Execute(out Process _, "ipconfig", "/registerdns", true, true);
            ProcessManager.Execute(out Process _, "ipconfig", "/release", true, true);
            ProcessManager.ExecuteOnly(out Process _, "ipconfig", "/renew", true, true);
            //ProcessManager.Execute("netsh", "winsock reset"); // Needs PC Restart
        }

        private void KillAll(bool killByName = false)
        {
            if (killByName)
            {
                ProcessManager.KillProcessByName("SDCHttpProxy");
                ProcessManager.KillProcessByName("dnslookup");
                ProcessManager.KillProcessByName("dnsproxy");
                ProcessManager.KillProcessByName("dnscrypt-proxy");
                ProcessManager.KillProcessByName("goodbyedpi");
            }
            else
            {
                ProcessManager.KillProcessByPID(PIDHttpProxy);
                ProcessManager.KillProcessByPID(PIDFakeProxy);
                ProcessManager.KillProcessByName("dnslookup");
                ProcessManager.KillProcessByPID(PIDDNSProxy);
                ProcessManager.KillProcessByPID(PIDDNSProxyBypass);
                ProcessManager.KillProcessByPID(PIDDNSCrypt);
                ProcessManager.KillProcessByPID(PIDDNSCryptBypass);
                ProcessManager.KillProcessByPID(PIDGoodbyeDPI);
                ProcessManager.KillProcessByPID(PIDGoodbyeDPIBypass);
            }
        }

        public ProcessPriorityClass GetCPUPriority()
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

        public bool CheckNecessaryFiles(bool showMessage = true)
        {
            if (!File.Exists(SecureDNS.DnsLookup) || !File.Exists(SecureDNS.DnsProxy) || !File.Exists(SecureDNS.DNSCrypt) ||
                !File.Exists(SecureDNS.DNSCryptConfigPath) || !File.Exists(SecureDNS.DNSCryptConfigCloudflarePath) ||
                !File.Exists(SecureDNS.GoodbyeDpi) || !File.Exists(SecureDNS.HttpProxyPath) || !File.Exists(SecureDNS.WinDivert) ||
                !File.Exists(SecureDNS.WinDivert32) || !File.Exists(SecureDNS.WinDivert64))
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
            string sdchttpproxyNewVer = SecureDNS.GetBinariesVersionFromResource("sdchttpproxy");
            string goodbyedpiNewVer = SecureDNS.GetBinariesVersionFromResource("goodbyedpi");

            // Get Old Versions
            string dnslookupOldVer = SecureDNS.GetBinariesVersion("dnslookup");
            string dnsproxyOldVer = SecureDNS.GetBinariesVersion("dnsproxy");
            string dnscryptOldVer = SecureDNS.GetBinariesVersion("dnscrypt-proxy");
            string sdchttpproxyOldVer = SecureDNS.GetBinariesVersion("sdchttpproxy");
            string goodbyedpiOldVer = SecureDNS.GetBinariesVersion("goodbyedpi");

            // Get Version Result
            int dnslookupResult = Info.VersionCompare(dnslookupNewVer, dnslookupOldVer);
            int dnsproxyResult = Info.VersionCompare(dnsproxyNewVer, dnsproxyOldVer);
            int dnscryptResult = Info.VersionCompare(dnscryptNewVer, dnscryptOldVer);
            int sdchttpproxyResult = Info.VersionCompare(sdchttpproxyNewVer, sdchttpproxyOldVer);
            int goodbyedpiResult = Info.VersionCompare(goodbyedpiNewVer, goodbyedpiOldVer);

            // Check Missing/Update Binaries
            if (!CheckNecessaryFiles(false) || dnslookupResult == 1 || dnsproxyResult == 1 || dnscryptResult == 1 ||
                                               sdchttpproxyResult == 1 || goodbyedpiResult == 1)
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

                if (!File.Exists(SecureDNS.DNSCryptConfigCloudflarePath))
                    await Resource.WriteResourceToFileAsync(NecessaryFiles.Resource1.dnscrypt_proxy_fakeproxyTOML, SecureDNS.DNSCryptConfigCloudflarePath);

                if (!File.Exists(SecureDNS.HttpProxyPath) || sdchttpproxyResult == 1)
                    await Resource.WriteResourceToFileAsync(NecessaryFiles.Resource1.SDCHttpProxy, SecureDNS.HttpProxyPath);

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

                string msg2 = $"{Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductName} is ready.{NL}";
                CustomRichTextBoxLog.AppendText(msg2, Color.LightGray);
            }
        }

        public static bool IsDnsProtocolSupported(string dns)
        {
            dns = dns.Trim();
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
        
        private async void CheckDPIWorks(string host, int timeoutSec = 30) //Default timeout: 100 sec
        {
            if (string.IsNullOrWhiteSpace(host)) return;

            // If DNS is Setting or Unsetting Return
            if (IsDNSSetting || IsDNSUnsetting) return;

            // If user changing DPI mode fast, return.
            if (StopWatchCheckDPIWorks.IsRunning) return;

            // Start StopWatch
            if (!StopWatchCheckDPIWorks.IsRunning)
                StopWatchCheckDPIWorks.Start();

            // Write start DPI checking to log
            string msgDPI = $"Checking DPI Bypass ({host})...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI, Color.LightGray));

            // Update Bools
            //UpdateBools();
            //await UpdateBoolHttpProxy();

            // Wait for IsDPIActive
            Task wait1 = Task.Run(async () =>
            {
                while (!IsDPIActive)
                {
                    if (IsDPIActive) break;
                    await Task.Delay(100);
                }
            });
            await wait1.WaitAsync(TimeSpan.FromSeconds(5));

            try
            {
                if (!IsDPIActive)
                {
                    // Write activate DPI first to log
                    string msgDPI1 = $"DPI Check: ";
                    string msgDPI2 = $"Activate DPI Bypass to check.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI2, Color.IndianRed));
                    StopWatchCheckDPIWorks.Stop();
                    StopWatchCheckDPIWorks.Reset();

                    return;
                }

                // Is HTTP Proxy Direct DNS Set?!
                bool isProxyDnsSet = false;
                if (ProcessManager.FindProcessByPID(PIDFakeProxy) &&
                    IsSharing &&
                    ProxyDNSMode != HTTPProxyServer.Program.Dns.Mode.Disable &&
                    ProxyStaticDPIBypassMode != HTTPProxyServer.Program.DPIBypass.Mode.Disable)
                    isProxyDnsSet = true;

                if (!IsDNSSet && !isProxyDnsSet)
                {
                    // Write set DNS first to log
                    string msgDPI1 = $"DPI Check: ";
                    string msgDPI2 = $"Set DNS to check.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI2, Color.IndianRed));
                    StopWatchCheckDPIWorks.Stop();
                    StopWatchCheckDPIWorks.Reset();

                    return;
                }

                string url = $"https://{host}/";
                Uri uri = new(url, UriKind.Absolute);

                bool isProxyPortOpen = Network.IsPortOpen(IPAddress.Loopback.ToString(), ProxyPort, 5);
                Debug.WriteLine($"Is Proxy Port Open: {isProxyPortOpen}, Port: {ProxyPort}");

                if (IsProxyDPIActive && isProxyPortOpen && ProxyProcess != null)
                {
                    Debug.WriteLine("Proxy");

                    UpdateProxyBools = false;
                    // Kill all requests before check
                    ProcessManager.SendCommand(ProxyProcess, "killall");

                    string proxyScheme = $"http://{IPAddress.Loopback}:{ProxyPort}";

                    using SocketsHttpHandler socketsHttpHandler = new();
                    socketsHttpHandler.Proxy = new WebProxy(proxyScheme, true);

                    using HttpClient httpClientWithProxy = new(socketsHttpHandler);
                    httpClientWithProxy.Timeout = TimeSpan.FromSeconds(timeoutSec);

                    StopWatchCheckDPIWorks.Restart();
                    
                    HttpResponseMessage r = await httpClientWithProxy.GetAsync(uri);

                    if (r.IsSuccessStatusCode || r.StatusCode == HttpStatusCode.NotFound || r.StatusCode == HttpStatusCode.Forbidden)
                    {
                        msgSuccess();
                        r.Dispose();
                    }
                    else
                        msgFailed(r);
                }
                else
                {
                    Debug.WriteLine("No Proxy");
                    using HttpClient httpClient = new();
                    httpClient.Timeout = TimeSpan.FromSeconds(timeoutSec);

                    StopWatchCheckDPIWorks.Restart();
                    
                    HttpResponseMessage r = await httpClient.GetAsync(uri);

                    if (r.IsSuccessStatusCode || r.StatusCode == HttpStatusCode.NotFound || r.StatusCode == HttpStatusCode.Forbidden)
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
                    if (IsDPIActive)
                    {
                        TimeSpan eTime = StopWatchCheckDPIWorks.Elapsed;
                        eTime = TimeSpan.FromMilliseconds(Math.Round(eTime.TotalMilliseconds, 2));
                        string eTimeStr = eTime.Seconds > 9 ? $"{eTime:ss\\.ff}" : $"{eTime:s\\.ff}";
                        string msgDPI1 = $"DPI Check: ";
                        string msgDPI2 = $"Successfully opened {host} in {eTimeStr} seconds.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI1, Color.LightGray));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI2, Color.MediumSeaGreen));
                    }
                    else
                    {
                        string msgCancel = $"DPI Check: Canceled.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCancel, Color.LightGray));
                    }
                }

                void msgFailed(HttpResponseMessage r)
                {
                    // Write Status to log
                    if (IsDPIActive)
                    {
                        string msgDPI1 = $"DPI Check: ";
                        string msgDPI2 = $"Status {r.StatusCode}: {r.ReasonPhrase}.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI1, Color.LightGray));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI2, Color.DodgerBlue));
                    }
                    else
                    {
                        string msgCancel = $"DPI Check: Canceled.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCancel, Color.LightGray));
                    }

                    r.Dispose();
                }

                StopWatchCheckDPIWorks.Stop();
                StopWatchCheckDPIWorks.Reset();
            }
            catch (Exception ex)
            {
                // Write Failed to log
                if (IsDPIActive)
                {
                    string msgDPI1 = $"DPI Check: ";
                    string msgDPI2 = $"{ex.Message}{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI2, Color.IndianRed));
                }
                else
                {
                    string msgCancel = $"DPI Check: Canceled.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCancel, Color.LightGray));
                }

                StopWatchCheckDPIWorks.Stop();
                StopWatchCheckDPIWorks.Reset();
            }

            UpdateProxyBools = true;
        }

    }
}
