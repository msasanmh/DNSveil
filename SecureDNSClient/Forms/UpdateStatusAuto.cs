using CustomControls;
using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using MsmhToolsWinFormsClass;
using System.Diagnostics;
using System.Media;
using System.Net;
using System.Reflection;

namespace SecureDNSClient;

public partial class FormMain
{
    private TimeSpan UpdateAllAutoBenchmark = TimeSpan.Zero;
    private async void UpdateAllAuto()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    UpdateAllAutoBenchmark = AppUpTime.Elapsed;
                    int delay = UpdateAutoDelayMS;
                    
                    if (IsAppReady)
                    {
                        await UpdateBoolInternetAccess();
                        await Task.Delay(delay);
                    }
                    
                    await UpdateBools(90);
                    await Task.Delay(delay);

                    await UpdateBoolProxy();
                    await Task.Delay(delay);
                    
                    await UpdateStatusLong(50);
                    await Task.Delay(delay);

                    if (Visible)
                    {
                        await UpdateStatusCpuUsage();
                        await Task.Delay(delay);
                    }

                    TimeSpan eTime = AppUpTime.Elapsed - UpdateAllAutoBenchmark;
                    UpdateDnsDohDelayMS = Convert.ToInt32(eTime.TotalMilliseconds);
                    //Debug.WriteLine("UpdateAllAuto: " + ConvertTool.TimeSpanToHumanRead(eTime, true));

                    await Task.Delay(delay / 2);
                }
                catch (Exception) { }
            }
        });
    }

    private async void UpdateNotifyIconIconAuto()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(2000);
                await UpdateNotifyIconIcon();
            }
        });
    }

    private async Task UpdateNotifyIconIcon()
    {
        try
        {
            if (!IsAppReady)
            {
                NotifyIconMain.Icon = Properties.Resources.SecureDNSClient_R_Multi;
                NotifyIconMain.Text = "Waiting For Network...";
                return;
            }

            if (IsInActionState)
            {
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        // Update Is In Action
                        IsInActionState = IsInAction(false, true, true, true, true, true, true, true, true, true, true, out string reason);

                        if (!IsInActionState) break;

                        try { NotifyIconMain.Text = reason; } catch (Exception) { }

                        // Loading
                        NotifyIconMain.Icon = Properties.Resources.SecureDNSClient_B_Multi;
                        await Task.Delay(200);
                        NotifyIconMain.Icon = Properties.Resources.SecureDNSClient_BR_Multi;
                        await Task.Delay(200);
                        NotifyIconMain.Icon = Properties.Resources.SecureDNSClient_R_Multi;
                        await Task.Delay(200);
                        NotifyIconMain.Icon = Properties.Resources.SecureDNSClient_RB_Multi;
                        await Task.Delay(200);
                    }
                });
            }
            else
            {
                if (!IsDNSConnected & !IsDNSSet & !IsProxyRunning & !IsProxySet & !IsDPIActive)
                    NotifyIconMain.Icon = Properties.Resources.SecureDNSClient_R_Multi;
                else if (IsDNSConnected & IsDNSSet & IsProxyRunning & IsProxySet & IsDPIActive)
                    NotifyIconMain.Icon = Properties.Resources.SecureDNSClient_B_Multi;
                else if (!IsDNSConnected & !IsDNSSet & IsProxyRunning & IsProxySet & IsDPIActive)
                    NotifyIconMain.Icon = Properties.Resources.SecureDNSClient_RB_Multi;
                else if (!IsDNSConnected & !IsDNSSet & IsProxyRunning & IsDPIActive)
                    NotifyIconMain.Icon = Properties.Resources.SecureDNSClient_RB_Multi;
                else if (IsDNSConnected & IsDNSSet & !IsProxyRunning & !IsProxySet & !IsDPIActive)
                    NotifyIconMain.Icon = Properties.Resources.SecureDNSClient_RB_Multi;
                else if (IsDNSConnected & IsDNSSet & !IsProxyRunning & !IsDPIActive)
                    NotifyIconMain.Icon = Properties.Resources.SecureDNSClient_RB_Multi;
                else
                    NotifyIconMain.Icon = Properties.Resources.SecureDNSClient_BR_Multi;

                // Message (Max 128 chars)
                string msg = string.Empty;
                if (IsDNSConnected & !IsDNSSet) msg += $"DNS: Online{NL}";
                else if (IsDNSConnected & IsDNSSet) msg += $"DNS: Online & Set{NL}";
                if (IsDoHConnected) msg += $"DoH: Online{NL}";
                if (IsDNSConnected)
                {
                    string firstGroup;
                    if (LastConnectMode == ConnectMode.ConnectToWorkingServers && CurrentUsingCustomServersList.Any())
                        firstGroup = CurrentUsingCustomServersList[0].GroupName;
                    else
                        firstGroup = GetConnectModeNameByConnectMode(LastConnectMode);
                    int charN = 25;
                    string group = firstGroup.Length <= charN ? firstGroup : $"{firstGroup[..charN]}...";
                    msg += $"Group: {group}{NL}";
                }
                if (IsProxyRunning & !IsProxySet) msg += $"Proxy: Online{NL}";
                else if (IsProxyRunning & IsProxySet) msg += $"Proxy: Online & Set{NL}";
                if (IsDPIActive) msg += $"DPI Bypass: Active{NL}";

                if (msg.EndsWith(NL)) msg = msg.TrimEnd(NL.ToCharArray());
                if (string.IsNullOrEmpty(msg)) msg = Text;

                if (msg.Length <= 128) try { NotifyIconMain.Text = msg; } catch (Exception) { }

                await Task.Delay(1000);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("UpdateNotifyIconIcon: " + ex.Message);
        }
    }

    private void CheckUpdateAuto()
    {
        if (!Program.IsStartup) Task.Run(async () => await CheckUpdate());

        System.Timers.Timer timer = new();
        timer.Interval = TimeSpan.FromHours(1).TotalMilliseconds;
        timer.Elapsed += (s, e) =>
        {
            Task.Run(async () => await CheckUpdate());
        };
        timer.Start();
    }

    private async Task CheckUpdate(bool showMsg = false)
    {
        if (IsCheckingStarted) return;
        if (IsCheckingForUpdate) return;
        if (showMsg) IsCheckingForUpdate = true;

        await UpdateBoolInternetAccess();
        if (!IsInternetOnline)
        {
            IsCheckingForUpdate = false;
            return;
        }
        
        string updateUrl = "https://github.com/msasanmh/SecureDNSClient/raw/main/update";
        string update = string.Empty;
        string labelUpdate = string.Empty;
        string downloadUrl = string.Empty;

        if (showMsg)
        {
            string checking = $"{NL}Checking update...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(checking, Color.LightGray));
        }

        try
        {
            // Without System Proxy
            using HttpClientHandler handler = new();
            handler.UseProxy = false;
            using HttpClient httpClient = new(handler);
            httpClient.Timeout = TimeSpan.FromSeconds(20);
            update = await httpClient.GetStringAsync(new Uri(updateUrl));
        }
        catch (Exception)
        {
            try
            {
                // With System Proxy
                using HttpClientHandler handler = new();
                handler.UseProxy = true;
                using HttpClient httpClient = new(handler);
                httpClient.Timeout = TimeSpan.FromSeconds(20);
                update = await httpClient.GetStringAsync(new Uri(updateUrl));
            }
            catch (Exception ex)
            {
                update = string.Empty;
                Debug.WriteLine("Check Update: " + ex.Message);
            }
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

                if (showMsg)
                {
                    string productName = Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductName ?? "Secure DNS Client";

                    string msg = $"There is a new version of {productName}{NL}";
                    msg += $"New version: {newVersion}, Current version: {currentVersion}{NL}";
                    msg += "Open download webpage?";
                    DialogResult dr = CustomMessageBox.Show(this, msg, "New Version", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (dr == DialogResult.OK)
                        OpenLinks.OpenUrl(downloadUrl);
                }
            }
            else
            {
                // Link Label Check Update
                labelUpdate = string.Empty;

                if (showMsg)
                {
                    string uptodate = $"You are using the latest version.";
                    CustomMessageBox.Show(this, uptodate, "UpToDate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        else
        {
            if (showMsg)
            {
                string err = $"Error connecting to update server.";
                CustomMessageBox.Show(this, err, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        if (string.IsNullOrEmpty(labelUpdate))
        {
            // No Update
            this.InvokeIt(() => CustomButtonCheckUpdate.Text = "Check Update");
            this.InvokeIt(() => CustomButtonCheckUpdate.BackColor = BackColor);
        }
        else
        {
            this.InvokeIt(() => CustomButtonCheckUpdate.Text = "New Ver");
            this.InvokeIt(() => CustomButtonCheckUpdate.BackColor = Color.MediumSeaGreen);
        }

        IsCheckingForUpdate = false;
    }

    private void LogClearAuto()
    {
        System.Timers.Timer logAutoClearTimer = new(10000);
        logAutoClearTimer.Elapsed += (s, e) =>
        {
            int length = 0;
            this.InvokeIt(() => length = CustomRichTextBoxLog.Text.Length);
            if (length > 90000)
            {
                this.InvokeIt(() => CustomRichTextBoxLog.ResetText());
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"{NL}Log Auto Clear.{NL}", Color.MediumSeaGreen));
            }
        };
        logAutoClearTimer.Start();
    }

    private async Task UpdateBools(int delay = 0)
    {
        // Update Is In Action
        IsInActionState = IsInAction(false, true, true, true, true, true, true, true, true, true, true, out _);
        await Task.Delay(delay);

        // Update Startup Info
        IsOnStartup = IsAppOnWindowsStartup(out bool isStartupPathOk);
        IsStartupPathOk = isStartupPathOk;
        await Task.Delay(delay);
        
        // Update Camouflage Bools
        IsBypassGoodbyeDpiActive = ProcessManager.FindProcessByPID(PIDGoodbyeDPIBypass);
        await Task.Delay(delay);

        // Update bool IsConnected
        IsConnected = ProcessManager.FindProcessByPID(PIDDnsServer);
        await Task.Delay(delay);

        // In Case Dnsproxy Or Dnscrypt Terminated
        if (!IsReconnecting && !IsConnected && !IsConnecting &&
            !IsQuickConnectWorking && !IsQuickConnecting && !IsQuickDisconnecting && !StopQuickConnect)
        {
            IsDNSConnected = IsDoHConnected = IsConnected;
            LocalDnsLatency = LocalDohLatency = -1;
            if (IsDNSSet)
            {
                await UnsetAllDNSs();
                if (Visible) await UpdateStatusNic();
            }
            // Write Connect Status
            this.InvokeIt(() => CustomRichTextBoxConnectStatus.ResetText());
            this.InvokeIt(() => CustomRichTextBoxConnectStatus.AppendText($"Disconnected.{NL}"));
        }
        await Task.Delay(delay);

        // In Case Proxy Server Terminated
        if (!IsProxyActivated && !IsProxyActivating && !IsProxyDeactivating && !IsProxyRunning)
        {
            if (IsProxySet) NetworkTool.UnsetProxy(false, true);
        }
        await Task.Delay(delay);
        
        // Update bool IsDnsSet
        IsDNSSet = SetDnsOnNic_.IsDnsSet(CustomComboBoxNICs, out bool isDnsSetOn, out _);
        IsDNSSetOn = isDnsSetOn;
        await Task.Delay(delay);

        // Update bool IsProxySet
        IsProxySet = UpdateBoolIsProxySet(out bool isAnotherProxySet, out string currentSystemProxy);
        IsAnotherProxySet = isAnotherProxySet;
        CurrentSystemProxy = currentSystemProxy;
        await Task.Delay(delay);
        
        // Update bool IsGoodbyeDPIActive
        IsGoodbyeDPIBasicActive = ProcessManager.FindProcessByPID(PIDGoodbyeDPIBasic);
        IsGoodbyeDPIAdvancedActive = ProcessManager.FindProcessByPID(PIDGoodbyeDPIAdvanced);
        await Task.Delay(delay);
        
        // Update bool IsDPIActive
        IsDPIActive = UpdateBoolIsDpiActive();
        await Task.Delay(delay);
    }

    private bool UpdateBoolIsDpiActive()
    {
        return IsProxyFragmentActive || IsProxySSLChangeSniActive || IsGoodbyeDPIBasicActive || IsGoodbyeDPIAdvancedActive;
    }

    private async Task UpdateBoolInternetAccess()
    {
        int timeoutMS = 5000;
        bool isNetOn = await IsInternetAlive(false, timeoutMS);
        IsInternetOnline = isNetOn ? isNetOn : await IsInternetAlive(false, timeoutMS);
    }

    private async void UpdateBoolDnsDohAuto()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                if (!IsAppReady || !IsConnected)
                {
                    await Task.Delay(500);
                    continue;
                }

                int timeoutMS = 2000;
                string blockedDomain = GetBlockedDomainSetting(out string _);
                await UpdateBoolDnsOnce(timeoutMS, blockedDomain);
                await Task.Delay(UpdateDnsDohDelayMS / 2);
                await UpdateBoolDohOnce(timeoutMS, blockedDomain);
                await Task.Delay(UpdateDnsDohDelayMS / 2);
            }
        });
    }

    private async Task UpdateBoolDnsOnce(int timeoutMS, string host = "google.com")
    {
        if (IsConnected)
        {
            // DNS
            if (IsInternetOnline)
            {
                string dnsServer = $"udp://{IPAddress.Loopback}:53";
                await ScanDns.CheckDnsAsync(host, dnsServer, timeoutMS);
                if (ScanDns.DnsLatency > 40)
                    await ScanDns.CheckDnsAsync(host, dnsServer, timeoutMS);
                LocalDnsLatency = ScanDns.DnsLatency;
                IsDNSConnected = LocalDnsLatency != -1;
            }
            else
            {
                LocalDnsLatency = -1;
                IsDNSConnected = LocalDnsLatency != -1;
            }
        }
        else
        {
            LocalDnsLatency = -1;
            IsDNSConnected = LocalDnsLatency != -1;
        }
    }

    private async Task UpdateBoolDohOnce(int timeoutMS, string host = "google.com")
    {
        if (IsConnected && CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
        {
            // DoH
            if (IsInternetOnline)
            {
                string dohServer = $"https://{IPAddress.Loopback}:{ConnectedDohPort}/dns-query";
                await ScanDns.CheckDnsAsync(host, dohServer, timeoutMS);
                if (ScanDns.DnsLatency > 80)
                    await ScanDns.CheckDnsAsync(host, dohServer, timeoutMS);
                LocalDohLatency = ScanDns.DnsLatency;
                IsDoHConnected = LocalDohLatency != -1;
            }
            else
            {
                LocalDohLatency = -1;
                IsDoHConnected = LocalDohLatency != -1;
            }
        }
        else
        {
            LocalDohLatency = -1;
            IsDoHConnected = LocalDohLatency != -1;
        }
    }

    private async Task UpdateBoolProxy()
    {
        if (!IsAppReady) return;
        await Task.Run(async () =>
        {
            IsProxyActivated = ProcessManager.FindProcessByPID(PIDProxyServer);

            string line = string.Empty;
            if (!UpdateProxyBools) return;
            if (IsProxyActivating) return;

            bool isCmdSent = await ProxyConsole.SendCommandAsync("Out Proxy");

            line = ProxyConsole.GetStdout;
#if DEBUG
            //Debug.WriteLine($"Line({isCmdSent}): " + line);
#endif
            if (!isCmdSent || string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
            {
                IsProxyRunning = false; ProxyRequests = 0; ProxyMaxRequests = 1000; IsProxyFragmentActive = false;
                IsProxySSLDecryptionActive = false; IsProxySSLChangeSniActive = false;
                ProxyFragmentMode = AgnosticProgram.Fragment.Mode.Disable;
                ProxyRulesMode = AgnosticProgram.ProxyRules.Mode.Disable;
            }
            else if (line.StartsWith("details"))
            {
                string[] split = line.Split('|');
                if (split.Length > 10)
                {
                    if (bool.TryParse(split[1].ToLower(), out bool sharing)) IsProxyRunning = sharing;
                    if (int.TryParse(split[2].ToLower(), out int port)) ProxyPort = port;
                    if (int.TryParse(split[3].ToLower(), out int requests)) ProxyRequests = requests;
                    if (int.TryParse(split[4].ToLower(), out int maxRequests)) ProxyMaxRequests = maxRequests;

                    if (bool.TryParse(split[5].ToLower(), out bool sslDecryptionActive)) IsProxySSLDecryptionActive = sslDecryptionActive;
                    if (bool.TryParse(split[6].ToLower(), out bool sslChangeSniActive)) IsProxySSLChangeSniActive = sslDecryptionActive && sslChangeSniActive;

                    if (bool.TryParse(split[7].ToLower(), out bool isFragmentActive)) IsProxyFragmentActive = isFragmentActive;
                    
                    if (split[8].ToLower().Equals("disable")) ProxyFragmentMode = AgnosticProgram.Fragment.Mode.Disable;
                    else if (split[8].ToLower().Equals("program")) ProxyFragmentMode = AgnosticProgram.Fragment.Mode.Program;

                    if (split[10].ToLower().Equals("disable")) ProxyRulesMode = AgnosticProgram.ProxyRules.Mode.Disable;
                    else if (split[10].ToLower().Equals("file")) ProxyRulesMode = AgnosticProgram.ProxyRules.Mode.File;
                    else if (split[10].ToLower().Equals("text")) ProxyRulesMode = AgnosticProgram.ProxyRules.Mode.Text;
                }
            }

            IsProxyDpiBypassActive = IsProxyFragmentActive || IsProxySSLChangeSniActive;

            // Update Proxy PID (Rare case)
            if (IsProxyRunning) PIDProxyServer = ProxyConsole.GetPid;
        });
    }

    private bool UpdateBoolIsProxySet(out bool isAnotherProxySet, out string proxies)
    {
        bool isProxySet = false;
        isAnotherProxySet = false;
        string proxiesOut = string.Empty;
        List<string> proxiesList = new();

        bool isAnyProxySet = NetworkTool.IsProxySet(out string httpProxyIpPort, out string httpsProxyIpPort, out string ftpProxyIpPort, out string socksProxyIpPort);
        
        if (!string.IsNullOrEmpty(httpProxyIpPort))
        {
            proxiesOut += $"http://{httpProxyIpPort}";
            if (!proxiesList.IsContain(httpProxyIpPort)) proxiesList.Add(httpProxyIpPort);
        }

        if (!string.IsNullOrEmpty(httpsProxyIpPort))
        {
            if (!string.IsNullOrEmpty(proxiesOut)) proxiesOut += ", ";
            proxiesOut += $"https://{httpsProxyIpPort}";
            if (!proxiesList.IsContain(httpsProxyIpPort)) proxiesList.Add(httpsProxyIpPort);
        }

        if (!string.IsNullOrEmpty(ftpProxyIpPort))
        {
            if (!string.IsNullOrEmpty(proxiesOut)) proxiesOut += ", ";
            proxiesOut += $"ftp://{ftpProxyIpPort}";
            if (!proxiesList.IsContain(ftpProxyIpPort)) proxiesList.Add(ftpProxyIpPort);
        }

        if (!string.IsNullOrEmpty(socksProxyIpPort))
        {
            if (!string.IsNullOrEmpty(proxiesOut)) proxiesOut += ", ";
            proxiesOut += $"socks://{socksProxyIpPort}";
            if (!proxiesList.IsContain(socksProxyIpPort)) proxiesList.Add(socksProxyIpPort);
        }

        if (isAnyProxySet)
        {
            for (int n = 0; n < proxiesList.Count; n++)
            {
                string proxyIpPort = proxiesList[n];
                if (!string.IsNullOrEmpty(proxyIpPort))
                    if (proxyIpPort.Contains(':'))
                    {
                        string[] split = proxyIpPort.Split(':');
                        string ip = split[0];
                        string portS = split[1];
                        bool isPortInt = int.TryParse(portS, out int port);
                        if (isPortInt)
                        {
                            if (ip == IPAddress.Loopback.ToString() && port == ProxyPort)
                                isProxySet = true;
                            else
                                isAnotherProxySet = true;
                        }
                    }
            }
        }

        proxies = proxiesOut;
        return isProxySet;
    }

    private async void UpdateStatusShortAuto()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(500);

                string boolsText = $"{IsScreenHighDpiScaleApplied}" +
                                   $"{IsAppReady}" +
                                   $"{IsInternetOnline}" +
                                   $"{IsInActionState}" +
                                   $"{IsOnStartup}" +
                                   $"{IsStartupPathOk}" +
                                   $"{IsCheckingForUpdate}" +
                                   $"{IsCheckingStarted}" +
                                   $"{StopChecking}" +
                                   $"{IsConnecting}" +
                                   $"{IsDisconnecting}" +
                                   $"{IsDisconnectingAll}" +
                                   $"{IsConnected}" +
                                   $"{IsDNSConnected}" +
                                   $"{IsDoHConnected}" +
                                   $"{IsDNSSetting}" +
                                   $"{IsDNSUnsetting}" +
                                   $"{IsDNSSet}" +
                                   $"{IsDNSSetOn}" +
                                   $"{IsFlushingDns}" +
                                   $"{IsDPIActive}" +
                                   $"{IsGoodbyeDPIBasicActive}" +
                                   $"{IsGoodbyeDPIAdvancedActive}" +
                                   $"{IsBypassGoodbyeDpiActive}" +
                                   $"{IsProxyActivated}" +
                                   $"{IsProxyActivating}" +
                                   $"{IsProxyDeactivating}" +
                                   $"{IsProxyRunning}" +
                                   $"{IsProxyFragmentActive}" +
                                   $"{IsProxySSLDecryptionActive}" +
                                   $"{IsProxySSLChangeSniActive}" +
                                   $"{IsProxySet}" +
                                   $"{IsAnotherProxySet}";
                if (Visible && !BoolsChangedText.Equals(boolsText))
                {
                    BoolsChangedText = boolsText;
                    Debug.WriteLine("Bools Changed");
                    await UpdateStatusShortOnBoolsChanged();
                }

                if (Visible) await UpdateStatusShort();
            }
        });
    }

    private async Task UpdateStatusShortOnBoolsChanged()
    {
        try
        {
            int delay = 0;

            // Check Button Text
            if (StopChecking) this.InvokeIt(() => CustomButtonCheck.Text = "Stopping...");
            else this.InvokeIt(() => CustomButtonCheck.Text = IsCheckingStarted ? "Stop" : "Scan");
            await Task.Delay(delay);

            // Check Button Enable
            this.InvokeIt(() => CustomButtonCheck.Enabled = !IsConnecting && !StopChecking);
            await Task.Delay(delay);

            // Quick Connect Button Text
            this.InvokeIt(() => CustomButtonQuickConnect.Text = StopQuickConnect ? "Stopping QC" : (IsQuickConnecting && IsCheckingStarted && StopChecking) ? "Skipping" : (IsQuickConnecting && IsCheckingStarted) ? "Skip Scan" : IsQuickConnecting ? "Stop QC" : "Quick Connect");
            await Task.Delay(delay);

            // Quick Connect Button Enable
            this.InvokeIt(() => CustomButtonQuickConnect.Enabled = !StopQuickConnect && !(IsQuickConnecting && IsCheckingStarted && StopChecking));
            await Task.Delay(delay);

            // Connect Button Text
            this.InvokeIt(() =>
            {
                if (IsDisconnecting) CustomButtonConnect.Text = "Disconnecting...";
                else if (IsConnecting) CustomButtonConnect.Text = "Stop";
                else CustomButtonConnect.Text = IsConnected ? "Disconnect" : "Connect";
            });
            await Task.Delay(delay);

            // Reconnect Button Enable
            this.InvokeIt(() => CustomButtonReconnect.Enabled = (IsConnected || IsConnecting) && !IsDisconnecting);
            await Task.Delay(delay);

            // SetDNS Button Text
            this.InvokeIt(() =>
            {
                IsDNSSet = SetDnsOnNic_.IsDnsSet(CustomComboBoxNICs, out bool isDnsSetOn, out _);
                IsDNSSetOn = isDnsSetOn;
                if (IsDNSUnsetting) CustomButtonSetDNS.Text = "Unsetting...";
                else if (IsDNSSetting) CustomButtonSetDNS.Text = "Setting...";
                else CustomButtonSetDNS.Text = IsDNSSetOn ? "Unset DNS" : "Set DNS";
            });
            await Task.Delay(delay);

            // SetDNS Button Enable
            this.InvokeIt(() => CustomButtonSetDNS.Enabled = IsConnected && !IsDNSSetting && !IsDNSUnsetting);
            await Task.Delay(delay);

            // StartProxy Buttom Text
            this.InvokeIt(() =>
            {
                if (IsProxyDeactivating) CustomButtonShare.Text = "Stopping...";
                else if (IsProxyActivating) CustomButtonShare.Text = "Starting...";
                else CustomButtonShare.Text = IsProxyActivated ? "Stop Proxy" : "Start Proxy";
            });
            await Task.Delay(delay);

            // StartProxy Buttom Enable
            this.InvokeIt(() => CustomButtonShare.Enabled = !IsProxyActivating && !IsProxyDeactivating);
            await Task.Delay(delay);

            // SetProxy Button Text
            this.InvokeIt(() => CustomButtonSetProxy.Text = IsProxySet || IsAnotherProxySet ? "Unset Proxy" : "Set Proxy");
            await Task.Delay(delay);

            // SetProxy Button Enable & Color
            this.InvokeIt(() =>
            {
                if (IsAnotherProxySet)
                {
                    CustomButtonSetProxy.Enabled = true;
                    CustomButtonSetProxy.SetToolTip(MainToolTip, string.Empty, "Another Proxy Is Set To System");
                    CustomButtonSetProxy.ForeColor = ForeColor;
                    CustomButtonSetProxy.BackColor = Color.DodgerBlue;
                    CustomButtonSetProxy.Font = new(Font.FontFamily, Font.Size, FontStyle.Bold);
                }
                else
                {
                    CustomButtonSetProxy.SetToolTip(MainToolTip, string.Empty, string.Empty);
                    if (IsProxyActivated && IsProxyRunning)
                    {
                        CustomButtonSetProxy.Enabled = true;
                        if (!IsProxyDeactivating)
                        {
                            CustomButtonSetProxy.ForeColor = !IsProxySet ? BackColor : ForeColor;
                            CustomButtonSetProxy.BackColor = !IsProxySet ? Color.MediumSeaGreen : BackColor;
                            CustomButtonSetProxy.Font = !IsProxySet ? new(Font.FontFamily, Font.Size, FontStyle.Bold) : new(Font.FontFamily, Font.Size, FontStyle.Regular);
                        }
                    }
                    else
                    {
                        CustomButtonSetProxy.ForeColor = ForeColor;
                        CustomButtonSetProxy.BackColor = BackColor;
                        CustomButtonSetProxy.Font = new(Font.FontFamily, Font.Size, FontStyle.Regular);
                        CustomButtonSetProxy.Enabled = false;
                    }
                }
            });
            await Task.Delay(delay);

            // GoodbyeDPI Basic Activate/Reactivate Button Text
            this.InvokeIt(() => CustomButtonDPIBasicActivate.Text = IsGoodbyeDPIBasicActive ? "Reactivate" : "Activate");
            await Task.Delay(delay);

            // GoodbyeDPI Basic Deactivate Button Enable
            this.InvokeIt(() => CustomButtonDPIBasicDeactivate.Enabled = IsGoodbyeDPIBasicActive);
            await Task.Delay(delay);

            // GoodbyeDPI Advanced Activate/Reactivate Button Text
            this.InvokeIt(() => CustomButtonDPIAdvActivate.Text = IsGoodbyeDPIAdvancedActive ? "Reactivate" : "Activate");
            await Task.Delay(delay);

            // GoodbyeDPI Advanced Deactivate Button Enable
            this.InvokeIt(() => CustomButtonDPIAdvDeactivate.Enabled = IsGoodbyeDPIAdvancedActive);
            await Task.Delay(delay);

            // Settings -> Quick Connect -> Startup Button Text
            if (IsOnStartup)
                this.InvokeIt(() => CustomButtonSettingQcStartup.Text = IsStartupPathOk ? "Remove from Stratup" : "Fix Startup");
            else
                this.InvokeIt(() => CustomButtonSettingQcStartup.Text = "Apply to Startup");
            await Task.Delay(delay);

            // Update ApplyDpiBypassChanges Button
            UpdateApplyDpiBypassChangesButton();
            await Task.Delay(delay);

            // Update Is In Action
            IsInActionState = IsInAction(false, true, true, true, true, true, true, true, true, true, true, out _);
            await Task.Delay(delay);
        }
        catch (Exception) { }
    }

    private async Task UpdateStatusShort()
    {
        try
        {
            int delay = 5;
            
            // Hide Label Main After a period of time
            LabelMainHide();
            await Task.Delay(delay);

            // Update Min Size of Main Container Panel 1
            int offset = 20; // 20
            this.InvokeIt(() => SplitContainerMain.Panel1MinSize = PictureBoxFarvahar.Bottom + PictureBoxFarvahar.Height + offset);
            await Task.Delay(delay);

            // Stop Check Timer
            if (!IsCheckingStarted)
                this.InvokeIt(() => CustomProgressBarCheck.StopTimer = true);
            else
            {
                // Update Int Working Servers
                NumberOfWorkingServers = WorkingDnsList.Count;

                if (CustomDataGridViewStatus.RowCount > 0 && Visible)
                    this.InvokeIt(() => CustomDataGridViewStatus.Rows[0].Cells[1].Value = NumberOfWorkingServers);
            }
            await Task.Delay(delay);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("UpdateStatusShort: " + ex.Message);
        }
    }

    private void UpdateMinSizeOfStatus()
    {
        // Two Times: To Get VScrollbar Of CustomDataGridViewStatus Updated
        UpdateMinSizeOfStatusInternal();
        UpdateMinSizeOfStatusInternal();
    }

    private void UpdateMinSizeOfStatusInternal()
    {
        // Update Min Size of Status
        try
        {
            int scrollbarWidth = 0;
            this.InvokeIt(() =>
            {
                try
                {
                    foreach (VScrollBar scrollbar in CustomDataGridViewStatus.Controls.OfType<VScrollBar>())
                    {
                        if (scrollbar.Visible) scrollbarWidth = SystemInformation.VerticalScrollBarWidth;
                    }
                }
                catch (Exception) { }
            });

            int offset = 10;
            int statusWidth = CustomDataGridViewStatus.Columns[0].Width + CustomDataGridViewStatus.Columns[1].Width + scrollbarWidth + offset;
            int distance = SplitContainerTop.Width - SplitContainerTop.SplitterIncrement - statusWidth;
            if (distance > SplitContainerTop.Panel1MinSize && distance < SplitContainerTop.Width - SplitContainerTop.Panel2MinSize)
            {
                this.InvokeIt(() => SplitContainerTop.SplitterDistance = distance);
                if (!LabelMainStopWatch.IsRunning) LabelMainStopWatch.Start();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Splitter Distance: " + ex.Message);
        }
    }

    private void UpdateApplyDpiBypassChangesButton()
    {
        bool isSSLDecryptionEnable = IsSSLDecryptionEnable() || IsProxySSLDecryptionActive;

        // Don't Do This On App Startup
        if (Program.IsStartup) return;
        if (!Visible) return;
        if (AppUpTime.ElapsedMilliseconds < 20000) return;

        // ApplyDpiBypassChanges Button Enable & Color
        this.InvokeIt(() =>
        {
            if (IsProxyActivated && IsProxyRunning && !IsProxyActivating && !IsProxyDeactivating)
            {
                bool proxyChanged = (IsProxyFragmentActive != CustomCheckBoxPDpiEnableFragment.Checked) ||
                                    (CustomCheckBoxPDpiEnableFragment.Checked && !LastFragmentProgramCommand.Equals(GetFragmentProgramCommand())) ||
                                    (IsProxySSLDecryptionActive != CustomCheckBoxProxyEnableSSL.Checked) ||
                                    (IsProxySSLChangeSniActive != CustomCheckBoxProxySSLChangeSni.Checked && CustomCheckBoxProxyEnableSSL.Checked) ||
                                    (CustomCheckBoxProxyEnableSSL.Checked && CustomCheckBoxProxySSLChangeSni.Checked && !LastDefaultSni.Equals(GetDefaultSniSetting()));
                
                CustomButtonPDpiApplyChanges.ForeColor = proxyChanged ? BackColor : ForeColor;
                CustomButtonPDpiApplyChanges.BackColor = proxyChanged ? Color.MediumSeaGreen : BackColor;
                CustomButtonPDpiApplyChanges.Font = proxyChanged ? new(Font.FontFamily, Font.Size, FontStyle.Bold) : new(Font.FontFamily, Font.Size, FontStyle.Regular);
                CustomButtonPDpiApplyChanges.Enabled = proxyChanged;
            }
            else
            {
                CustomButtonPDpiApplyChanges.ForeColor = ForeColor;
                CustomButtonPDpiApplyChanges.BackColor = BackColor;
                CustomButtonPDpiApplyChanges.Font = new(Font.FontFamily, Font.Size, FontStyle.Regular);
                CustomButtonPDpiApplyChanges.Enabled = false;
            }
        });
    }

    private async Task UpdateStatusLong(int delay = 0)
    {
        // Update Int Working Servers
        NumberOfWorkingServers = WorkingDnsList.Count;

        if (CustomDataGridViewStatus.RowCount == 14 && Visible)
        {
            // Update Status Working Servers
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[0].Cells[1].Style.ForeColor = Color.DodgerBlue);
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[0].Cells[1].Value = NumberOfWorkingServers);
            await Task.Delay(delay);

            UpdateMinSizeOfStatus();

            // Update Status IsConnected
            string textConnect = IsConnected ? "Yes" : "No";
            Color colorConnect = IsConnected ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[1].Cells[1].Style.ForeColor = colorConnect);
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[1].Cells[1].Value = textConnect);
            await Task.Delay(delay);

            // Update Status IsDNSConnected
            string statusLocalDNS = IsDNSConnected ? "Online" : "Offline";
            Color colorStatusLocalDNS = IsDNSConnected ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[2].Cells[1].Style.ForeColor = colorStatusLocalDNS);
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[2].Cells[1].Value = statusLocalDNS);
            await Task.Delay(delay);

            // Update Status LocalDnsLatency
            string statusLocalDnsLatency = LocalDnsLatency != -1 ? $"{LocalDnsLatency} ms" : "-1";
            Color colorStatusLocalDnsLatency = LocalDnsLatency != -1 ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[3].Cells[1].Style.ForeColor = colorStatusLocalDnsLatency);
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[3].Cells[1].Value = statusLocalDnsLatency);
            await Task.Delay(delay);

            // Update Status IsDoHConnected
            string statusLocalDoH = IsDoHConnected ? "Online" : "Offline";
            Color colorStatusLocalDoH = IsDoHConnected ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[4].Cells[1].Style.ForeColor = colorStatusLocalDoH);
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[4].Cells[1].Value = statusLocalDoH);
            await Task.Delay(delay);

            // Update Status LocalDohLatency
            string statusLocalDoHLatency = LocalDohLatency != -1 ? $"{LocalDohLatency} ms" : "-1";
            Color colorStatusLocalDoHLatency = LocalDohLatency != -1 ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[5].Cells[1].Style.ForeColor = colorStatusLocalDoHLatency);
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[5].Cells[1].Value = statusLocalDoHLatency);
            await Task.Delay(delay);

            // Update Status IsDnsSet
            string textDNS = IsDNSSet ? "Yes" : "No";
            Color colorDNS = IsDNSSet ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[6].Cells[1].Style.ForeColor = colorDNS);
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[6].Cells[1].Value = textDNS);
            await Task.Delay(delay);

            // Update Status IsSharing
            string textSharing = IsProxyRunning ? "Yes" : "No";
            Color colorSharing = IsProxyRunning ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[7].Cells[1].Style.ForeColor = colorSharing);
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[7].Cells[1].Value = textSharing);
            await Task.Delay(delay);

            // Update Status ProxyRequests
            string textProxyRequests = "0";// "0 of 0"
            Color colorProxyRequests = Color.MediumSeaGreen;
            textProxyRequests = $"{ProxyRequests}"; // $"{ProxyRequests} of {ProxyMaxRequests}"
            colorProxyRequests = ProxyRequests < ProxyMaxRequests ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[8].Cells[1].Style.ForeColor = colorProxyRequests);
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[8].Cells[1].Value = textProxyRequests);
            await Task.Delay(delay);

            // Update Status IsProxySet
            string textProxySet = IsProxySet ? "Yes" : "No";
            Color colorProxySet = IsProxySet ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[9].Cells[1].Style.ForeColor = colorProxySet);
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[9].Cells[1].Value = textProxySet);
            await Task.Delay(delay);

            // Update Status IsProxyDpiBypassActive (Fragment Or SSL)
            string textProxyDPI = IsProxyDpiBypassActive ? "Active" : "Inactive";
            Color colorProxyDPI = IsProxyDpiBypassActive ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[10].Cells[1].Style.ForeColor = colorProxyDPI);
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[10].Cells[1].Value = textProxyDPI);
            await Task.Delay(delay);

            // Update Status IsGoodbyeDPIActive
            string textGoodbyeDPI = (IsGoodbyeDPIBasicActive || IsGoodbyeDPIAdvancedActive) ? "Active" : "Inactive";
            Color colorGoodbyeDPI = (IsGoodbyeDPIBasicActive || IsGoodbyeDPIAdvancedActive) ? Color.MediumSeaGreen : Color.IndianRed;
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[11].Cells[1].Style.ForeColor = colorGoodbyeDPI);
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[11].Cells[1].Value = textGoodbyeDPI);
            await Task.Delay(delay);

            // 12: CPU

            // Empty Status to Keep Width Fixed
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[13].Cells[1].Value = "                  ");

            UpdateMinSizeOfStatus();
            await Task.Delay(delay);
        }
        
        // Internet Status
        WriteNetworkStatus();

        // Play Audio Alert
        if (!CustomCheckBoxSettingDisableAudioAlert.Checked && !IsCheckingStarted && !IsExiting)
        {
            if (!StopWatchAudioAlertDelay.IsRunning) StopWatchAudioAlertDelay.Start();
            if (StopWatchAudioAlertDelay.ElapsedMilliseconds > 2000)
                PlayAudioAlert();
        }

        // Bar Color
        if (!IsInActionState)
            this.InvokeIt(() => SplitContainerMain.BackColor = IsDNSConnected ? Color.MediumSeaGreen : Color.IndianRed);
        else
            this.InvokeIt(() => SplitContainerMain.BackColor = Color.DodgerBlue);
    }

    private async Task UpdateStatusNic() // Auto Update will increase CPU usage of "WmiPrvSE.exe"
    {
        if (Program.IsStartup) return;
        // Variables
        bool isNicDisabled = false;
        string e = string.Empty;
        string name = e, description = e, adapterType = e, availability = e, status = e, netStatus = e, dnsAddresses = e;
        bool isPhysicalAdapter = false, isIPv6Enabled = false;
        string macAddress = e, manufacturer = e, serviceName = e, speed = e, timeOfLastReset = e;
        
        SetDnsOnNic.ActiveNICs nicsList = GetNicNameSetting(CustomComboBoxNICs);
        
        string nicName = string.Empty;
        if (IsInternetOnline) nicName = await nicsList.PrimaryNic(GetBootstrapSetting(out int port), port);
        else if (nicsList.NICs.Any()) nicName = nicsList.NICs[0];
        
        if (!string.IsNullOrEmpty(nicName))
        {
            NetworkInterfaces nis = new(nicName);
            isNicDisabled = nis.ConfigManagerErrorCode == 22;
            name = nis.NetConnectionID;
            description = nis.Description;
            if (string.IsNullOrEmpty(description)) description = nis.Name;
            if (string.IsNullOrEmpty(description)) description = nis.ProductName;
            adapterType = nis.AdapterTypeIDMessage;
            availability = nis.AvailabilityMessage;
            status = nis.ConfigManagerErrorCodeMessage;
            netStatus = nis.NetConnectionStatusMessage;
            
            for (int n = 0; n < nis.DnsAddresses.Count; n++)
                dnsAddresses += $"{nis.DnsAddresses[n]}, ";
            dnsAddresses = dnsAddresses.Trim();
            if (dnsAddresses.EndsWith(',')) dnsAddresses = dnsAddresses.TrimEnd(',');

            isPhysicalAdapter = nis.PhysicalAdapter;
            isIPv6Enabled = nis.IsIPv6ProtocolSupported;
            macAddress = nis.MACAddress;
            manufacturer = nis.Manufacturer;
            serviceName = nis.ServiceName;
            speed = $"{ConvertTool.ConvertByteToHumanRead(nis.Speed / 8)}/s";
            timeOfLastReset = $"{nis.TimeOfLastReset:yyyy/MM/dd HH:mm:ss}";

            this.InvokeIt(() => CustomButtonEnableDisableNic.Enabled = true);
        }
        else this.InvokeIt(() => CustomButtonEnableDisableNic.Enabled = false);

        // Update CustomButtonEnableDisableNicIPv6 Text
        this.InvokeIt(() => CustomButtonEnableDisableNicIPv6.Text = isIPv6Enabled ? "Disable IPv6" : "Enable IPv6");

        // Update CustomButtonEnableDisableNic Text
        this.InvokeIt(() => CustomButtonEnableDisableNic.Text = isNicDisabled ? "Enable NIC" : "Disable NIC");

        try
        {
            if (CustomDataGridViewNicStatus.RowCount == 14)
            {
                // Update Name
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[0].Cells[1].Style.ForeColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[0].Cells[1].Value = name);

                // Update Description
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[1].Cells[1].Style.ForeColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[1].Cells[1].Value = description);
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[1].Cells[1].ToolTipText = description);

                // Update AdapterType
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[2].Cells[1].Style.ForeColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[2].Cells[1].Value = adapterType);

                // Update Availability
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[3].Cells[1].Style.ForeColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[3].Cells[1].Value = availability);

                // Update Status
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[4].Cells[1].Style.ForeColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[4].Cells[1].Value = status);

                // Update Net Status
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[5].Cells[1].Style.ForeColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[5].Cells[1].Value = netStatus);

                // Update DNSAddresses
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[6].Cells[1].Style.ForeColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[6].Cells[1].Value = dnsAddresses);
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[6].Cells[1].ToolTipText = dnsAddresses);

                // Update IsIPv6Enabled
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[7].Cells[1].Style.ForeColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[7].Cells[1].Value = isIPv6Enabled);

                // Update MACAddress
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[8].Cells[1].Style.ForeColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[8].Cells[1].Value = macAddress);

                // Update Manufacturer
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[9].Cells[1].Style.ForeColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[9].Cells[1].Value = manufacturer);

                // Update IsPhysicalAdapter
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[10].Cells[1].Style.ForeColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[10].Cells[1].Value = isPhysicalAdapter);

                // Update ServiceName
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[11].Cells[1].Style.ForeColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[11].Cells[1].Value = serviceName);

                // Update Speed
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[12].Cells[1].Style.ForeColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[12].Cells[1].Value = speed);

                // Update TimeOfLastReset
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[13].Cells[1].Style.ForeColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[13].Cells[1].Value = timeOfLastReset);
            }
        }
        catch (Exception) { }
    }

    private async Task UpdateStatusCpuUsage()
    {
        int delay = 1000;
        double cpu = await GetCpuUsage(delay);

        if (!IsExiting && cpu > 95)
        {
            string msg = $"{NL}Closed On CPU Overload.{NL}";
            Debug.WriteLine(msg);
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg));
            try { Environment.Exit(0); } catch (Exception) { }
            Application.Exit();
            ProcessManager.KillProcessByPID(Environment.ProcessId, true);
            return;
        }

        // Update Status CPU Usage
        Color colorCPU = cpu <= 35 ? Color.MediumSeaGreen : Color.IndianRed;

        if (CustomDataGridViewStatus.RowCount == 14 && Visible)
        {
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[12].Cells[1].Style.ForeColor = colorCPU);
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[12].Cells[1].Value = $"{cpu}%");
            UpdateMinSizeOfStatus();
            if (Once2)
            {
                await FillComboBoxes(); Once2 = false;
            }
        }
    }

    private async Task<double> GetCpuUsage(int delay)
    {
        if (Program.IsStartup) return 0;
        
        float sdc = -1, dnsserver = -1, proxyServer = -1;
        float goodbyeDpiBasic = -1, goodbyeDpiAdvanced = -1, goodbyeDpiBypass = -1;

        Task a = Task.Run(async () => sdc = await ProcessManager.GetCpuUsage(Environment.ProcessId, delay));
        Task b = Task.Run(async () => dnsserver = await ProcessManager.GetCpuUsage(PIDDnsServer, delay));
        Task c = Task.Run(async () => proxyServer = await ProcessManager.GetCpuUsage(PIDProxyServer, delay));
        Task d = Task.Run(async () => goodbyeDpiBasic = await ProcessManager.GetCpuUsage(PIDGoodbyeDPIBasic, delay));
        Task e = Task.Run(async () => goodbyeDpiAdvanced = await ProcessManager.GetCpuUsage(PIDGoodbyeDPIAdvanced, delay));
        Task f = Task.Run(async () => goodbyeDpiBypass = await ProcessManager.GetCpuUsage(PIDGoodbyeDPIBypass, delay));

        List<Task> tasksList = new();
        tasksList.Add(a);
        if (PIDDnsServer != -1) tasksList.Add(b);
        if (PIDProxyServer != -1) tasksList.Add(c);
        if (PIDGoodbyeDPIBasic != -1) tasksList.Add(d);
        if (PIDGoodbyeDPIAdvanced != -1) tasksList.Add(e);
        if (PIDGoodbyeDPIBypass != -1) tasksList.Add(f);

        await Task.WhenAll(tasksList);

        float sum = 0;
        List<float> list = new();
        list.Clear();
        if (sdc != -1) list.Add(sdc);
        if (dnsserver != -1) list.Add(dnsserver);
        if (proxyServer != -1) list.Add(proxyServer);
        if (goodbyeDpiBasic != -1) list.Add(goodbyeDpiBasic);
        if (goodbyeDpiAdvanced != -1) list.Add(goodbyeDpiAdvanced);
        if (goodbyeDpiBypass != -1) list.Add(goodbyeDpiBypass);
        
        for (int n = 0; n < list.Count; n++) sum += list[n];
        double result = Math.Round(Convert.ToDouble(sum), 2, MidpointRounding.AwayFromZero);
        return result > 100 ? 100 : result;
    }

    private void SaveSettingsAuto()
    {
        // Using System.Timers.Timer needs Invoke.
        System.Windows.Forms.Timer autoSaveTimer = new();
        autoSaveTimer.Interval = Convert.ToInt32(TimeSpan.FromMinutes(2).TotalMilliseconds);
        autoSaveTimer.Tick += async (s, e) =>
        {
            await SaveSettings();
        };
        autoSaveTimer.Start();
    }

    private async Task SaveSettings()
    {
        // Select Control type and properties to save
        if (AppSettings == null) return;
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
    }

    private async void WriteNetworkStatus()
    {
        if (!IsAppReady) return;
        if (IsExiting) return;
        if (InternetOffline && !IsInternetOnline)
        {
            InternetOffline = false;
            InternetOnline = true;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"{NL}There is no Internet connectivity.{NL}", Color.IndianRed));

            // Help System To Connect
            if (Program.IsStartup)
            {
                await ProcessManager.ExecuteAsync("ipconfig", null, "/release", true, true);
                await ProcessManager.ExecuteAsync("ipconfig", null, "/renew", true, true);
            }
        }

        if (InternetOnline && IsInternetOnline)
        {
            InternetOnline = false;
            InternetOffline = true;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"{NL}Back Online.{NL}", Color.MediumSeaGreen));
        }
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

        if (IsProxyRunning && (ProxyRequests >= ProxyMaxRequests) && !AudioAlertRequestsExceeded)
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