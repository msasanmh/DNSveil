using CustomControls;
using MsmhTools;
using MsmhTools.HTTPProxyServer;
using System;
using System.Diagnostics;
using System.Media;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;

namespace SecureDNSClient
{
    public partial class FormMain
    {
        private void CheckUpdateAuto()
        {
            Task.Run(async () => await CheckUpdate());
            System.Timers.Timer savedDnsUpdateTimer = new();
            savedDnsUpdateTimer.Interval = TimeSpan.FromHours(6).TotalMilliseconds;
            savedDnsUpdateTimer.Elapsed += (s, e) =>
            {
                Task.Run(async () => await CheckUpdate());
            };
            savedDnsUpdateTimer.Start();
        }

        private async Task CheckUpdate(bool writeToLog = false)
        {
            if (IsCheckingStarted) return;
            if (!IsInternetAlive(false)) return;
            if (IsCheckingForUpdate) return;
            if (writeToLog)
                IsCheckingForUpdate = true;

            string updateUrl = "https://github.com/msasanmh/SecureDNSClient/raw/main/update";
            string update = string.Empty;
            string labelUpdate = string.Empty;
            string downloadUrl = string.Empty;

            if (writeToLog)
            {
                string checking = $"Checking update...{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(checking, Color.LightGray));
            }

            try
            {
                using HttpClient httpClient = new();
                httpClient.Timeout = TimeSpan.FromSeconds(20);
                update = await httpClient.GetStringAsync(new Uri(updateUrl));
            }
            catch (Exception)
            {
                update = string.Empty;
            }

            update = update.Trim();
            if (!string.IsNullOrEmpty(update) && update.Contains('|'))
            {
                string[] split = update.Split('|');
                if (split.Length != 2)
                {
                    IsCheckingForUpdate = false;
                    return;
                }
                string newVersion = split[0].Trim();
                string currentVersion = Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductVersion ?? "99.99.99";
                downloadUrl = split[1].Trim();
                if (string.IsNullOrEmpty(downloadUrl)) downloadUrl = "https://github.com/msasanmh/SecureDNSClient/releases/latest";

                int versionResult = Info.VersionCompare(newVersion, currentVersion);
                if (versionResult == 1)
                {
                    // Link Label Check Update
                    labelUpdate = $"There is a new version v{newVersion}";

                    if (writeToLog)
                    {
                        string productName = Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductName ?? "Secure DNS Client";
                        string msg1 = $"{NL}There is a new version of {productName}{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg1, Color.MediumSeaGreen));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText("New version: ", Color.LightGray));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(newVersion, Color.DodgerBlue));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(", Current version: ", Color.LightGray));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(currentVersion + NL, Color.DodgerBlue));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText("Download: ", Color.LightGray));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(downloadUrl + NL + NL, Color.MediumSeaGreen));
                    }
                }
                else
                {
                    // Link Label Check Update
                    labelUpdate = string.Empty;

                    if (writeToLog)
                    {
                        string uptodate = $"{NL}You are using the latest version.{NL}{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(uptodate, Color.MediumSeaGreen));
                    }
                }
            }
            else
            {
                if (writeToLog)
                {
                    string err = $"{NL}Error connecting to update server.{NL}{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(err, Color.IndianRed));
                }
            }

            // Link Label Check Update
            this.InvokeIt(() =>
            {
                LinkLabelCheckUpdate.ForeColor = Color.MediumSeaGreen;
                LinkLabelCheckUpdate.ActiveLinkColor = Color.MediumSeaGreen;
                LinkLabelCheckUpdate.LinkColor = Color.MediumSeaGreen;
                LinkLabelCheckUpdate.Text = labelUpdate;
            });

            LinkLabelCheckUpdate.LinkClicked -= LinkLabelCheckUpdate_LinkClicked;
            LinkLabelCheckUpdate.LinkClicked += LinkLabelCheckUpdate_LinkClicked;
            void LinkLabelCheckUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            {
                if (!string.IsNullOrEmpty(downloadUrl)) OpenLinks.OpenUrl(downloadUrl);
            }

            IsCheckingForUpdate = false;
        }

        private void LogClearAuto()
        {
            System.Timers.Timer logAutoClearTimer = new();
            logAutoClearTimer.Interval = 5000;
            logAutoClearTimer.Elapsed += (s, e) =>
            {
                int length = 0;
                this.InvokeIt(() => length = CustomRichTextBoxLog.Text.Length);
                if (length > 90000)
                {
                    this.InvokeIt(() => CustomRichTextBoxLog.ResetText());
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Log Auto Clear.{NL}", Color.MediumSeaGreen));
                }
            };
            logAutoClearTimer.Start();
        }

        private void UpdateBoolsAuto()
        {
            System.Timers.Timer updateBoolsTimer = new();
            updateBoolsTimer.Interval = 4000;
            updateBoolsTimer.Elapsed += (s, e) =>
            {
                UpdateBools();
            };
            updateBoolsTimer.Start();
        }

        private async void UpdateBools()
        {
            // Update bool IsConnected
            IsConnected = ProcessManager.FindProcessByPID(PIDDNSProxy) ||
                          ProcessManager.FindProcessByPID(PIDDNSProxyBypass) ||
                          ProcessManager.FindProcessByPID(PIDDNSCrypt) ||
                          ProcessManager.FindProcessByPID(PIDDNSCryptBypass);

            // In case dnsproxy or dnscrypt processes terminated
            if (!IsConnected)
            {
                IsDNSConnected = IsDoHConnected = IsConnected;
                LocalDnsLatency = LocalDohLatency = -1;
                if (CamouflageDNSServer != null && CamouflageDNSServer.IsRunning)
                    CamouflageDNSServer.Stop();
                if (IsDNSSet) await UnsetSavedDNS();
            }

            // In case SDCHttpProxy terminated
            if (!IsProxyActivated && !IsSharing && IsProxySet)
            {
                Network.UnsetProxy(false, false);
            }

            // Update bool IsDnsSet
            //IsDNSSet = UpdateBoolIsDnsSet(out bool _); // I need to test this on Win7 myself!

            // Update bool IsProxySet
            IsProxySet = UpdateBoolIsProxySet();

            // Update bool IsGoodbyeDPIActive
            IsGoodbyeDPIActive = ProcessManager.FindProcessByPID(PIDGoodbyeDPI);

            // Update bool IsDPIActive
            IsDPIActive = (IsProxyDPIActive || IsGoodbyeDPIActive);
        }

        private void UpdateBoolDnsDohAuto()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    int timeout = 10000;
                    Task.Delay(timeout + 500).Wait();
                    Parallel.Invoke(
                    () => UpdateBoolDnsOnce(timeout),
                    () => UpdateBoolDohOnce(timeout)
                    );
                }
            });
        }

        private void UpdateBoolDnsOnce(int timeout)
        {
            if (IsConnected)
            {
                // DNS
                CheckDns checkDns = new(false, GetCPUPriority());
                checkDns.CheckDNS("google.com", IPAddress.Loopback.ToString(), timeout);
                LocalDnsLatency = checkDns.DnsLatency;
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
                string dohServer;
                if (ConnectedDohPort == 443)
                    dohServer = $"https://{IPAddress.Loopback}/dns-query";
                else
                    dohServer = $"https://{IPAddress.Loopback}:{ConnectedDohPort}/dns-query";
                CheckDns checkDns = new(false, GetCPUPriority());
                checkDns.CheckDNS("google.com", dohServer, timeout);
                LocalDohLatency = checkDns.DnsLatency;
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
                        bool isDnsSet = Network.IsDnsSet(nic, out string dnsServer1, out string _);
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

        private void UpdateBoolHttpProxyAuto()
        {
            Task task = Task.Run(() =>
            {
                while (true)
                {
                    Task.Delay(2000).Wait();
                    if (!UpdateProxyBools) continue;

                    IsProxyActivated = ProcessManager.FindProcessByPID(PIDHttpProxy);

                    string line = string.Empty;
                    if (ProxyProcess != null)
                    {
                        if (!UpdateProxyBools) continue;

                        try
                        {
                            ProxyProcess.StandardOutput.DiscardBufferedData();
                            ProxyProcess.StandardInput.WriteLine("out");
                        }
                        catch (Exception)
                        {
                            // do nothing
                        }

                        while (ProcessManager.FindProcessByPID(PIDHttpProxy))
                        {
                            try
                            {
                                string l = ProxyProcess.StandardOutput.ReadLine() ?? string.Empty;
                                Task.Delay(200).Wait();
                                if (l.StartsWith("details")) line = l;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }

                            if (line.StartsWith("details"))
                            {
                                //Debug.WriteLine("Done");
                                break;
                            }
                        }
                    }

                    //Debug.WriteLine("Line: " + line);

                    if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
                    {
                        IsSharing = false; ProxyRequests = 0; ProxyMaxRequests = 250; IsProxyDPIActive = false;
                    }
                    else if (line.StartsWith("details"))
                    {
                        string[] split = line.Split('|');
                        if (bool.TryParse(split[1].ToLower(), out bool sharing)) IsSharing = sharing;
                        if (int.TryParse(split[2].ToLower(), out int port)) ProxyPort = port;
                        if (int.TryParse(split[3].ToLower(), out int requests)) ProxyRequests = requests;
                        if (int.TryParse(split[4].ToLower(), out int maxRequests)) ProxyMaxRequests = maxRequests;
                        if (bool.TryParse(split[5].ToLower(), out bool dpiActive)) IsProxyDPIActive = dpiActive;

                        if (split[6].ToLower().Equals("disable")) ProxyStaticDPIBypassMode = HTTPProxyServer.Program.DPIBypass.Mode.Disable;
                        else if (split[6].ToLower().Equals("program")) ProxyStaticDPIBypassMode = HTTPProxyServer.Program.DPIBypass.Mode.Program;

                        if (split[7].ToLower().Equals("disable")) ProxyDPIBypassMode = HTTPProxyServer.Program.DPIBypass.Mode.Disable;
                        else if (split[7].ToLower().Equals("program")) ProxyDPIBypassMode = HTTPProxyServer.Program.DPIBypass.Mode.Program;

                        if (split[8].ToLower().Equals("disable")) ProxyUpStreamMode = HTTPProxyServer.Program.UpStreamProxy.Mode.Disable;
                        else if (split[8].ToLower().Equals("http")) ProxyUpStreamMode = HTTPProxyServer.Program.UpStreamProxy.Mode.HTTP;
                        else if (split[8].ToLower().Equals("socks5")) ProxyUpStreamMode = HTTPProxyServer.Program.UpStreamProxy.Mode.SOCKS5;

                        if (split[9].ToLower().Equals("disable")) ProxyDNSMode = HTTPProxyServer.Program.Dns.Mode.Disable;
                        else if (split[9].ToLower().Equals("system")) ProxyDNSMode = HTTPProxyServer.Program.Dns.Mode.System;
                        else if (split[9].ToLower().Equals("doh")) ProxyDNSMode = HTTPProxyServer.Program.Dns.Mode.DoH;
                        else if (split[9].ToLower().Equals("plaindns")) ProxyDNSMode = HTTPProxyServer.Program.Dns.Mode.PlainDNS;

                        if (split[10].ToLower().Equals("disable")) ProxyFakeDnsMode = HTTPProxyServer.Program.FakeDns.Mode.Disable;
                        else if (split[10].ToLower().Equals("file")) ProxyFakeDnsMode = HTTPProxyServer.Program.FakeDns.Mode.File;
                        else if (split[10].ToLower().Equals("text")) ProxyFakeDnsMode = HTTPProxyServer.Program.FakeDns.Mode.Text;

                        if (split[11].ToLower().Equals("disable")) ProxyBWListMode = HTTPProxyServer.Program.BlackWhiteList.Mode.Disable;
                        else if (split[11].ToLower().Equals("blacklistfile")) ProxyBWListMode = HTTPProxyServer.Program.BlackWhiteList.Mode.BlackListFile;
                        else if (split[11].ToLower().Equals("blacklisttext")) ProxyBWListMode = HTTPProxyServer.Program.BlackWhiteList.Mode.BlackListText;
                        else if (split[11].ToLower().Equals("whitelistfile")) ProxyBWListMode = HTTPProxyServer.Program.BlackWhiteList.Mode.WhiteListFile;
                        else if (split[11].ToLower().Equals("whitelisttext")) ProxyBWListMode = HTTPProxyServer.Program.BlackWhiteList.Mode.WhiteListText;

                        if (split[12].ToLower().Equals("disable")) DontBypassMode = HTTPProxyServer.Program.DontBypass.Mode.Disable;
                        else if (split[12].ToLower().Equals("file")) DontBypassMode = HTTPProxyServer.Program.DontBypass.Mode.File;
                        else if (split[12].ToLower().Equals("text")) DontBypassMode = HTTPProxyServer.Program.DontBypass.Mode.Text;

                    }
                }
            });
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
                                if (ip == IPAddress.Loopback.ToString() && port == ProxyPort)
                                    return true;
                        }
            }
            return false;
        }

        private void UpdateStatusShortAuto()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Task.Delay(300).Wait();
                    this.InvokeIt(() => UpdateStatusShort());
                }
            });
        }

        private void UpdateStatusShort()
        {
            // Update Status Working Servers
            NumberOfWorkingServers = WorkingDnsList.Count;
            CustomRichTextBoxStatusWorkingServers.ResetText();
            CustomRichTextBoxStatusWorkingServers.AppendText("Working Servers: ", ForeColor);
            CustomRichTextBoxStatusWorkingServers.AppendText(NumberOfWorkingServers.ToString(), Color.DodgerBlue);

            // Check Button
            CustomButtonCheck.Enabled = !IsConnecting;

            // Insecure and parallel
            if (CustomCheckBoxInsecure.Checked)
                CustomCheckBoxCheckInParallel.Checked = false;

            // Connect Button
            if (!CustomRadioButtonConnectCheckedServers.Checked)
            {
                CustomButtonConnect.Enabled = true;
            }
            else
            {
                if (WorkingDnsList.Any() && !IsCheckingStarted && !IsConnecting)
                    CustomButtonConnect.Enabled = true;
                else
                    CustomButtonConnect.Enabled = IsConnected;
            }

            // Connect to popular servers using proxy Textbox
            CustomTextBoxHTTPProxy.Enabled = CustomRadioButtonConnectDNSCrypt.Checked;

            // SetDNS Button
            if (IsConnected && (IsDNSConnected || IsDoHConnected))
                CustomButtonSetDNS.Enabled = true;

            // SetProxy Button
            if (IsProxyActivated && IsSharing)
                CustomButtonSetProxy.Enabled = true;

            // Settings -> Share -> Advanced
            CustomTextBoxSettingHTTPProxyCfCleanIP.Enabled = CustomCheckBoxSettingHTTPProxyCfCleanIP.Checked;

        }

        private void UpdateStatusLongAuto()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Task.Delay(2000).Wait();
                    UpdateStatusLong();
                }
            });
        }

        private void UpdateStatusLong()
        {
            // Update Status IsConnected
            string textConnect = IsConnected ? "Yes" : "No";
            Color colorConnect = IsConnected ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomRichTextBoxStatusIsConnected.ResetText());
            this.InvokeIt(() => CustomRichTextBoxStatusIsConnected.AppendText("Is Connected: ", ForeColor));
            this.InvokeIt(() => CustomRichTextBoxStatusIsConnected.AppendText(textConnect, colorConnect));

            // Update Status IsDNSConnected
            string statusLocalDNS = IsDNSConnected ? "Online" : "Offline";
            Color colorStatusLocalDNS = IsDNSConnected ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomRichTextBoxStatusLocalDNS.ResetText());
            this.InvokeIt(() => CustomRichTextBoxStatusLocalDNS.AppendText("Local DNS: ", ForeColor));
            this.InvokeIt(() => CustomRichTextBoxStatusLocalDNS.AppendText(statusLocalDNS, colorStatusLocalDNS));

            // Update Status LocalDnsLatency
            string statusLocalDnsLatency = LocalDnsLatency != -1 ? $"{LocalDnsLatency}" : "-1";
            Color colorStatusLocalDnsLatency = LocalDnsLatency != -1 ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomRichTextBoxStatusLocalDnsLatency.ResetText());
            this.InvokeIt(() => CustomRichTextBoxStatusLocalDnsLatency.AppendText("Local DNS Latency: ", ForeColor));
            this.InvokeIt(() => CustomRichTextBoxStatusLocalDnsLatency.AppendText(statusLocalDnsLatency, colorStatusLocalDnsLatency));

            // Update Status IsDoHConnected
            string statusLocalDoH = IsDoHConnected ? "Online" : "Offline";
            Color colorStatusLocalDoH = IsDoHConnected ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomRichTextBoxStatusLocalDoH.ResetText());
            this.InvokeIt(() => CustomRichTextBoxStatusLocalDoH.AppendText("Local DoH: ", ForeColor));
            this.InvokeIt(() => CustomRichTextBoxStatusLocalDoH.AppendText(statusLocalDoH, colorStatusLocalDoH));

            // Update Status LocalDohLatency
            string statusLocalDoHLatency = LocalDohLatency != -1 ? $"{LocalDohLatency}" : "-1";
            Color colorStatusLocalDoHLatency = LocalDohLatency != -1 ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomRichTextBoxStatusLocalDoHLatency.ResetText());
            this.InvokeIt(() => CustomRichTextBoxStatusLocalDoHLatency.AppendText("Local DoH Latency: ", ForeColor));
            this.InvokeIt(() => CustomRichTextBoxStatusLocalDoHLatency.AppendText(statusLocalDoHLatency, colorStatusLocalDoHLatency));

            // Update Status IsDnsSet
            string textDNS = IsDNSSet ? "Yes" : "No";
            Color colorDNS = IsDNSSet ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomRichTextBoxStatusIsDNSSet.ResetText());
            this.InvokeIt(() => CustomRichTextBoxStatusIsDNSSet.AppendText("Is DNS Set: ", ForeColor));
            this.InvokeIt(() => CustomRichTextBoxStatusIsDNSSet.AppendText(textDNS, colorDNS));

            // Update Status IsSharing
            string textSharing = IsSharing ? "Yes" : "No";
            Color colorSharing = IsSharing ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomRichTextBoxStatusIsSharing.ResetText());
            this.InvokeIt(() => CustomRichTextBoxStatusIsSharing.AppendText("Is Sharing: ", ForeColor));
            this.InvokeIt(() => CustomRichTextBoxStatusIsSharing.AppendText(textSharing, colorSharing));

            // Update Status ProxyRequests
            string textProxyRequests = "0 of 0";
            Color colorProxyRequests = Color.MediumSeaGreen;
            textProxyRequests = $"{ProxyRequests} of {ProxyMaxRequests}";
            colorProxyRequests = ProxyRequests < ProxyMaxRequests ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomRichTextBoxStatusProxyRequests.ResetText());
            this.InvokeIt(() => CustomRichTextBoxStatusProxyRequests.AppendText("Proxy Requests ", ForeColor));
            this.InvokeIt(() => CustomRichTextBoxStatusProxyRequests.AppendText(textProxyRequests, colorProxyRequests));

            // Update Status IsProxySet
            string textProxySet = IsProxySet ? "Yes" : "No";
            Color colorProxySet = IsProxySet ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomRichTextBoxStatusIsProxySet.ResetText());
            this.InvokeIt(() => CustomRichTextBoxStatusIsProxySet.AppendText("Is Proxy Set: ", ForeColor));
            this.InvokeIt(() => CustomRichTextBoxStatusIsProxySet.AppendText(textProxySet, colorProxySet));

            // Update Status IsProxyDPIActive
            string textProxyDPI = IsProxyDPIActive ? "Active" : "Inactive";
            Color colorProxyDPI = IsProxyDPIActive ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomRichTextBoxStatusProxyDpiBypass.ResetText());
            this.InvokeIt(() => CustomRichTextBoxStatusProxyDpiBypass.AppendText("Proxy DPI Bypass: ", ForeColor));
            this.InvokeIt(() => CustomRichTextBoxStatusProxyDpiBypass.AppendText(textProxyDPI, colorProxyDPI));

            // Update Status IsGoodbyeDPIActive
            string textGoodbyeDPI = IsGoodbyeDPIActive ? "Active" : "Inactive";
            Color colorGoodbyeDPI = IsGoodbyeDPIActive ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomRichTextBoxStatusGoodbyeDPI.ResetText());
            this.InvokeIt(() => CustomRichTextBoxStatusGoodbyeDPI.AppendText("GoodbyeDPI: ", ForeColor));
            this.InvokeIt(() => CustomRichTextBoxStatusGoodbyeDPI.AppendText(textGoodbyeDPI, colorGoodbyeDPI));

            // Play Audio Alert
            if (!CustomCheckBoxSettingDisableAudioAlert.Checked && !IsCheckingStarted)
            {
                if (!StopWatchAudioAlertDelay.IsRunning) StopWatchAudioAlertDelay.Start();
                if (StopWatchAudioAlertDelay.ElapsedMilliseconds > 5000)
                    PlayAudioAlert();
            }
        }

        private void UpdateStatusCpuUsageAuto()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    int delay = 1500;
                    double cpu = await GetCpuUsage(delay);
                    await Task.Delay(3000);
                    // Update Status CPU Usage
                    Color colorCPU = cpu <= 35 ? Color.MediumSeaGreen : Color.IndianRed;
                    this.InvokeIt(() => CustomRichTextBoxStatusCpuUsage.ResetText());
                    this.InvokeIt(() => CustomRichTextBoxStatusCpuUsage.AppendText("CPU: ", ForeColor));
                    this.InvokeIt(() => CustomRichTextBoxStatusCpuUsage.AppendText($"{cpu}%", colorCPU));
                }
            });
        }

        private async Task<double> GetCpuUsage(int delay)
        {
            float sdc = -1, sdcHttpProxy = -1, sdcFakeProxy = -1;
            float dnsproxy1 = -1, dnscrypt1 = -1, goodbyedpi1 = -1;
            float dnsproxy2 = -1, dnscrypt2 = -1, goodbyedpi2 = -1;

            Task a = Task.Run(async () => sdc = await ProcessManager.GetCpuUsage(Process.GetCurrentProcess(), delay));
            Task b = Task.Run(async () => sdcHttpProxy = await ProcessManager.GetCpuUsage(PIDHttpProxy, delay));
            Task c = Task.Run(async () => sdcFakeProxy = await ProcessManager.GetCpuUsage(PIDFakeProxy, delay));
            Task d = Task.Run(async () => dnsproxy1 = await ProcessManager.GetCpuUsage(PIDDNSProxy, delay));
            Task e = Task.Run(async () => dnsproxy2 = await ProcessManager.GetCpuUsage(PIDDNSProxyBypass, delay));
            Task f = Task.Run(async () => dnscrypt1 = await ProcessManager.GetCpuUsage(PIDDNSCrypt, delay));
            Task g = Task.Run(async () => dnscrypt2 = await ProcessManager.GetCpuUsage(PIDDNSCryptBypass, delay));
            Task h = Task.Run(async () => goodbyedpi1 = await ProcessManager.GetCpuUsage(PIDGoodbyeDPI, delay));
            Task i = Task.Run(async () => goodbyedpi2 = await ProcessManager.GetCpuUsage(PIDGoodbyeDPIBypass, delay));

            await Task.WhenAll(a, b, c, d, e, f, g, h, i);

            float sum = 0;
            List<float> list = new();
            if (sdc != -1) list.Add(sdc);
            if (sdcHttpProxy != -1) list.Add(sdcHttpProxy);
            if (sdcFakeProxy != -1) list.Add(sdcFakeProxy);
            if (dnsproxy1 != -1) list.Add(dnsproxy1);
            if (dnsproxy2 != -1) list.Add(dnsproxy2);
            if (dnscrypt1 != -1) list.Add(dnscrypt1);
            if (dnscrypt2 != -1) list.Add(dnscrypt2);
            if (goodbyedpi1 != -1) list.Add(goodbyedpi1);
            if (goodbyedpi2 != -1) list.Add(goodbyedpi2);

            for (int n = 0; n < list.Count; n++) sum += list[n];
            double result = Math.Round(Convert.ToDouble(sum), 2);
            return result > 100 ? 100 : result;
        }

        private void AutoSaveSettings()
        {
            // Using System.Timers.Timer needs Invoke.
            System.Windows.Forms.Timer autoSaveTimer = new();
            autoSaveTimer.Interval = int.Parse(TimeSpan.FromMinutes(2).TotalMilliseconds.ToString());
            autoSaveTimer.Tick += async (s, e) =>
            {
                // Select Control type and properties to save
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
            };
            autoSaveTimer.Start();
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
                    soundPlayer.Stop();
                    soundPlayer.Dispose();
                });
            }

            if (!IsDNSConnected && !IsDoHConnected && AudioAlertOffline)
            {
                AudioAlertOffline = false;
                AudioAlertOnline = true;

                int softEtherPID = -1;
                if (ProcessManager.FindProcessByName("vpnclient_x64"))
                    softEtherPID = ProcessManager.GetFirstPidByName("vpnclient_x64");

                if (softEtherPID != -1)
                    ProcessManager.SuspendProcess(softEtherPID); // On net disconnect SoftEther cause noise to audio.

                Task.Run(() =>
                {
                    Task.Delay(1000).Wait();
                    SoundPlayer soundPlayer = new(Audio.Resource1.DNS_Offline);
                    soundPlayer.PlaySync();
                    soundPlayer.Stop();
                    soundPlayer.Dispose();
                    Task.Delay(5000).Wait();
                });

                if (softEtherPID != -1)
                    ProcessManager.ResumeProcess(softEtherPID);
            }

            if (IsSharing && (ProxyRequests >= ProxyMaxRequests) && !AudioAlertRequestsExceeded)
            {
                AudioAlertRequestsExceeded = true;
                Task.Run(() =>
                {
                    SoundPlayer soundPlayer = new(Audio.Resource1.Warning_Handle_Requests_Exceeded);
                    soundPlayer.PlaySync();
                    soundPlayer.Stop();
                    soundPlayer.Dispose();
                });
            }

            if (ProxyRequests < ProxyMaxRequests - 5)
                AudioAlertRequestsExceeded = false;

            StopWatchAudioAlertDelay.Stop();
            StopWatchAudioAlertDelay.Reset();
        }

    }
}
