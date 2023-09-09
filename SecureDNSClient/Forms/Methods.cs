using CustomControls;
using MsmhToolsClass;
using MsmhToolsClass.HTTPProxyServer;
using MsmhToolsWinFormsClass;
using System;
using System.Diagnostics;
using System.Drawing;
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

        public bool IsInAction(bool showMsg, bool isCheckingForUpdate, bool isQuickConnectWorking, bool isCheckingStarted,
                               bool isConnecting, bool isDisconnecting, bool isDNSSetting, bool isDNSUnsetting,
                               bool isProxyActivating, bool isProxyDeactivating)
        {
            bool isInAction = (isCheckingForUpdate && IsCheckingForUpdate) ||
                              (isQuickConnectWorking && IsQuickConnectWorking) ||
                              (isCheckingStarted && IsCheckingStarted) ||
                              (isConnecting && IsConnecting) ||
                              (isDisconnecting && IsDisconnecting) ||
                              (isDNSSetting && IsDNSSetting) ||
                              (isDNSUnsetting && IsDNSUnsetting) ||
                              (isProxyActivating && IsHttpProxyActivating) ||
                              (isProxyDeactivating && IsHttpProxyDeactivating);

            if (isInAction && showMsg)
            {
                if (IsCheckingForUpdate)
                    CustomMessageBox.Show(this, "App is checking for update.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);

                else if (IsQuickConnectWorking)
                    CustomMessageBox.Show(this, "Quick Connect is in action.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);

                else if (IsCheckingStarted)
                    CustomMessageBox.Show(this, "App is checking DNS servers.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);

                else if (IsConnecting)
                    CustomMessageBox.Show(this, "App is connecting.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);

                else if (IsDisconnecting)
                    CustomMessageBox.Show(this, "App is disconnecting.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);

                else if (IsDNSSetting)
                    CustomMessageBox.Show(this, "Let DNS set.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);

                else if (IsDNSUnsetting)
                    CustomMessageBox.Show(this, "Let DNS unset.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);

                else if (IsHttpProxyActivating)
                    CustomMessageBox.Show(this, "Let HTTP Proxy activate.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);

                else if (IsHttpProxyDeactivating)
                    CustomMessageBox.Show(this, "Let HTTP Proxy deactivate.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            return isInAction;
        }

        private static void DeleteFileOnSize(string filePath, int sizeKB)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    long lenth = new FileInfo(filePath).Length;
                    if (ConvertTool.ConvertSize(lenth, ConvertTool.SizeUnits.Byte, ConvertTool.SizeUnits.KB, out _) > sizeKB)
                        File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Delete {Path.GetFileName(filePath)} File: {ex.Message}");
            }
        }

        private bool IsInternetAlive(bool writeToLog = true)
        {
            if (!NetworkTool.IsInternetAlive())
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

        public static List<int> GetPids(bool includeGoodbyeDpi)
        {
            List<int> list = new();
            int[] pids = { Environment.ProcessId, PIDHttpProxy, PIDFakeHttpProxy, PIDDNSProxy, PIDDNSProxyBypass, PIDDNSCrypt, PIDDNSCryptBypass };
            int[] pidsGD = { PIDGoodbyeDPIBasic, PIDGoodbyeDPIAdvanced, PIDGoodbyeDPIBypass };
            list.AddRange(pids);
            if (includeGoodbyeDpi) list.AddRange(pidsGD);
            return list.Distinct().ToList();
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
                ProcessManager.KillProcessByPID(PIDFakeHttpProxy);
                ProcessManager.KillProcessByName("dnslookup");
                ProcessManager.KillProcessByPID(PIDDNSProxy);
                ProcessManager.KillProcessByPID(PIDDNSProxyBypass);
                ProcessManager.KillProcessByPID(PIDDNSCrypt);
                ProcessManager.KillProcessByPID(PIDDNSCryptBypass);
                ProcessManager.KillProcessByPID(PIDGoodbyeDPIBasic);
                ProcessManager.KillProcessByPID(PIDGoodbyeDPIAdvanced);
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
                    await ResourceTool.WriteResourceToFileAsync(NecessaryFiles.Resource1.dnslookup, SecureDNS.DnsLookup);

                if (!File.Exists(SecureDNS.DnsProxy) || dnsproxyResult == 1)
                    await ResourceTool.WriteResourceToFileAsync(NecessaryFiles.Resource1.dnsproxy, SecureDNS.DnsProxy);

                if (!File.Exists(SecureDNS.DNSCrypt) || dnscryptResult == 1)
                    await ResourceTool.WriteResourceToFileAsync(NecessaryFiles.Resource1.dnscrypt_proxyEXE, SecureDNS.DNSCrypt);

                if (!File.Exists(SecureDNS.DNSCryptConfigPath))
                    await ResourceTool.WriteResourceToFileAsync(NecessaryFiles.Resource1.dnscrypt_proxyTOML, SecureDNS.DNSCryptConfigPath);

                if (!File.Exists(SecureDNS.DNSCryptConfigCloudflarePath))
                    await ResourceTool.WriteResourceToFileAsync(NecessaryFiles.Resource1.dnscrypt_proxy_fakeproxyTOML, SecureDNS.DNSCryptConfigCloudflarePath);

                if (!File.Exists(SecureDNS.HttpProxyPath) || sdchttpproxyResult == 1)
                    await ResourceTool.WriteResourceToFileAsync(NecessaryFiles.Resource1.SDCHttpProxy, SecureDNS.HttpProxyPath);

                if (!File.Exists(SecureDNS.GoodbyeDpi) || goodbyedpiResult == 1)
                    await ResourceTool.WriteResourceToFileAsync(NecessaryFiles.Resource1.goodbyedpi, SecureDNS.GoodbyeDpi);

                if (!File.Exists(SecureDNS.WinDivert))
                    await ResourceTool.WriteResourceToFileAsync(NecessaryFiles.Resource1.WinDivert, SecureDNS.WinDivert);

                if (!File.Exists(SecureDNS.WinDivert32))
                    await ResourceTool.WriteResourceToFileAsync(NecessaryFiles.Resource1.WinDivert32, SecureDNS.WinDivert32);

                if (!File.Exists(SecureDNS.WinDivert64))
                    await ResourceTool.WriteResourceToFileAsync(NecessaryFiles.Resource1.WinDivert64, SecureDNS.WinDivert64);

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
                    if (NetworkTool.IsIPv4Valid(ip, out IPAddress? _))
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

            // If there is no internet conectivity return
            if (!IsInternetAlive()) return;

            // If In Action return
            if (IsInAction(false, false, false, true, true, true, true, true, true, true)) return;

            // If user changing DPI mode fast, return.
            if (StopWatchCheckDPIWorks.IsRunning) return;

            // Start StopWatch
            if (!StopWatchCheckDPIWorks.IsRunning)
            {
                StopWatchCheckDPIWorks.Start();
                StopWatchCheckDPIWorks.Restart();
            }

            // Write start DPI checking to log
            string msgDPI = $"Checking DPI Bypass ({host})...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI, Color.LightGray));

            // Don't Update Bools Here!!

            // Wait for IsDPIActive
            Task wait1 = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsDPIActive) break;
                    await Task.Delay(100);
                }
            });
            try { await wait1.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

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
                if (ProcessManager.FindProcessByPID(PIDFakeHttpProxy) &&
                    IsHttpProxyRunning &&
                    ProxyDNSMode != HTTPProxyServer.Program.Dns.Mode.Disable &&
                    ProxyStaticDPIBypassMode != HTTPProxyServer.Program.DPIBypass.Mode.Disable)
                    isProxyDnsSet = true;

                if (!IsDNSSet && !isProxyDnsSet)
                {
                    // Write set DNS first to log
                    string msgDPI1 = $"DPI Check: ";
                    string msgDPI2 = $"Set DNS to check.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI2, Color.Orange));
                    StopWatchCheckDPIWorks.Stop();
                    StopWatchCheckDPIWorks.Reset();

                    return;
                }

                string url = $"https://{host}/";
                Uri uri = new(url, UriKind.Absolute);

                bool isProxyPortOpen = NetworkTool.IsPortOpen(IPAddress.Loopback.ToString(), HttpProxyPort, 5);
                Debug.WriteLine($"Is Proxy Port Open: {isProxyPortOpen}, Port: {HttpProxyPort}");

                if (IsHttpProxyDpiBypassActive && isProxyPortOpen && HttpProxyProcess != null)
                {
                    Debug.WriteLine("Proxy");

                    UpdateHttpProxyBools = false;
                    // Kill all requests before check
                    ProcessManager.SendCommand(HttpProxyProcess, "killall");

                    string proxyScheme = $"http://{IPAddress.Loopback}:{HttpProxyPort}";

                    using SocketsHttpHandler socketsHttpHandler = new();
                    socketsHttpHandler.Proxy = new WebProxy(proxyScheme, true);

                    using HttpClient httpClientWithProxy = new(socketsHttpHandler);
                    httpClientWithProxy.Timeout = TimeSpan.FromSeconds(timeoutSec);

                    StopWatchCheckDPIWorks.Restart();
                    HttpResponseMessage r = await httpClientWithProxy.GetAsync(uri);
                    StopWatchCheckDPIWorks.Stop();

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
                    using HttpClientHandler handler = new();
                    handler.UseProxy = false;
                    using HttpClient httpClient = new(handler);
                    httpClient.Timeout = TimeSpan.FromSeconds(timeoutSec);

                    StopWatchCheckDPIWorks.Restart();
                    HttpResponseMessage r = await httpClient.GetAsync(uri);
                    StopWatchCheckDPIWorks.Stop();

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

            UpdateHttpProxyBools = true;
        }

    }
}
