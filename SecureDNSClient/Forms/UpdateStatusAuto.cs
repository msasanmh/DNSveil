using CustomControls;
using MsmhToolsClass;
using MsmhToolsClass.ProxyServerPrograms;
using MsmhToolsWinFormsClass;
using System.Diagnostics;
using System.Media;
using System.Net;
using System.Reflection;
using File = System.IO.File;

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
                UpdateAllAutoBenchmark = AppUpTime.Elapsed;
                int delay = UpdateAutoDelayMS;

                if (IsAppReady)
                {
                    await UpdateBoolInternetAccess();
                    await Task.Delay(delay);
                }

                await UpdateBools();
                await Task.Delay(delay);

                await UpdateBoolProxy();
                await Task.Delay(delay);

                await UpdateStatusLong();
                await Task.Delay(delay);

                if (Visible)
                {
                    await UpdateStatusCpuUsage();
                    await Task.Delay(delay);
                }

                TimeSpan eTime = AppUpTime.Elapsed - UpdateAllAutoBenchmark;
                UpdateDnsDohDelayMS = Convert.ToInt32(eTime.TotalMilliseconds);
                //Debug.WriteLine("UpdateAllAuto: " + ConvertTool.TimeSpanToHumanRead(eTime, true));
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
                    if (!IsInActionState) break;

                    NotifyIconMain.Text = "In Action...";

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
            if (IsProxyRunning & !IsProxySet) msg += $"Proxy Server: Online{NL}";
            else if (IsProxyRunning & IsProxySet) msg += $"Proxy Server: Online & Set{NL}";
            if (IsDPIActive) msg += $"DPI Bypass: Active{NL}";

            if (msg.EndsWith(NL)) msg = msg.TrimEnd(NL.ToCharArray());
            if (string.IsNullOrEmpty(msg)) msg = Text;

            try { NotifyIconMain.Text = msg; } catch (Exception) { }
        }
    }

    private void CheckUpdateAuto()
    {
        if (!Program.Startup) Task.Run(async () => await CheckUpdate());

        System.Timers.Timer savedDnsUpdateTimer = new();
        savedDnsUpdateTimer.Interval = TimeSpan.FromHours(6).TotalMilliseconds;
        savedDnsUpdateTimer.Elapsed += (s, e) =>
        {
            Task.Run(async () => await CheckUpdate());
        };
        savedDnsUpdateTimer.Start();
    }

    private async Task CheckUpdate(bool showMsg = false)
    {
        if (IsCheckingStarted) return;
        if (!IsInternetOnline) return;
        if (IsCheckingForUpdate) return;
        if (showMsg) IsCheckingForUpdate = true;

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

        // Link Label Check Update
        this.InvokeIt(() =>
        {
            LinkLabelCheckUpdate.ForeColor = Color.MediumSeaGreen;
            LinkLabelCheckUpdate.ActiveLinkColor = Color.MediumSeaGreen;
            LinkLabelCheckUpdate.LinkColor = Color.MediumSeaGreen;
            LinkLabelCheckUpdate.Text = labelUpdate;
            LinkLabelCheckUpdate.Links.Clear();
            LinkLabelCheckUpdate.Links.Add(new LinkLabel.Link(0, LinkLabelCheckUpdate.Text.Length, downloadUrl));

            LinkLabelCheckUpdate.LinkClicked -= LinkLabelCheckUpdate_LinkClicked;
            LinkLabelCheckUpdate.LinkClicked += LinkLabelCheckUpdate_LinkClicked;

            void LinkLabelCheckUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            {
                string? dlUrl = e.Link.LinkData.ToString();
                if (!string.IsNullOrEmpty(dlUrl)) OpenLinks.OpenUrl(dlUrl);
            }
        });

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

    private async void UpdateBoolsAuto()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(1000);
                await UpdateBools();
            }
        });
    }

    private async Task UpdateBools()
    {
        int delay = 30;

        // Update Is In Action
        IsInActionState = IsInAction(false, true, true, true, true, true, true, true, true, true, true);
        await Task.Delay(delay);

        // Update Startup Info
        IsOnStartup = IsAppOnWindowsStartup(out bool isStartupPathOk);
        IsStartupPathOk = isStartupPathOk;
        await Task.Delay(delay);

        // Update Camouflage Bools
        IsBypassProxyActive = ProcessManager.FindProcessByPID(PIDCamouflageProxy);
        IsBypassDNSActive = CamouflageDNSServer != null && CamouflageDNSServer.IsRunning && !IsConnecting;
        IsBypassGoodbyeDpiActive = ProcessManager.FindProcessByPID(PIDGoodbyeDPIBypass);
        await Task.Delay(delay);

        // In Case Camouflage Proxy Terminated
        if (LastConnectMode == ConnectMode.ConnectToFakeProxyDohViaProxyDPI && !IsBypassProxyActive)
            ProcessManager.KillProcessByPID(PIDDNSCryptBypass);
        await Task.Delay(delay);

        // In Case Camouflage GoodbyeDPI Terminated
        if (LastConnectMode == ConnectMode.ConnectToFakeProxyDohViaGoodbyeDPI && !IsBypassGoodbyeDpiActive &&
            IsBypassDNSActive)
            ProcessManager.KillProcessByPID(PIDDNSProxyBypass);
        await Task.Delay(delay);

        // Update bool IsConnected
        IsConnected = ProcessManager.FindProcessByPID(PIDDNSProxy) ||
                      ProcessManager.FindProcessByPID(PIDDNSCryptBypass) ||
                      ProcessManager.FindProcessByPID(PIDDNSProxyBypass) ||
                      ProcessManager.FindProcessByPID(PIDDNSCrypt);
        await Task.Delay(delay);

        // In Case Dnsproxy or Dnscrypt Processes Terminated
        if (!IsConnected && !IsConnecting && !IsDisconnecting &&
            !IsQuickConnectWorking && !IsQuickConnecting && !IsQuickDisconnecting && !StopQuickConnect)
        {
            IsDNSConnected = IsDoHConnected = IsConnected;
            LocalDnsLatency = LocalDohLatency = -1;
            BypassFakeProxyDohStop(true, true, true, false);
            if (IsDNSSet) await UnsetAllDNSs();
        }
        await Task.Delay(delay);

        // In Case Fake Proxy Terminated
        if (!ProcessManager.FindProcessByPID(PIDFakeProxy) &&
            ProxyDNSMode == ProxyProgram.Dns.Mode.DoH &&
            IsProxyRunning)
            ProcessManager.KillProcessByPID(PIDProxy);
        await Task.Delay(delay);

        // In Case Proxy Server Terminated
        if (!IsProxyActivated && !IsProxyActivating && !IsProxyDeactivating && !IsProxyRunning)
        {
            ProcessManager.KillProcessByPID(PIDFakeProxy);
            if (IsProxySet)
                NetworkTool.UnsetProxy(false, true);
        }
        await Task.Delay(delay);

        // Update bool IsDnsSet
        IsDNSSet = SetDnsOnNic_.IsDnsSet();
        await Task.Delay(delay);

        // Update bool IsProxySet
        IsProxySet = UpdateBoolIsProxySet();
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
        return IsProxyDpiBypassActive || IsProxySSLChangeSniToIpActive || IsGoodbyeDPIBasicActive || IsGoodbyeDPIAdvancedActive;
    }

    private async void UpdateBoolInternetAccessAuto()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(1000);
                if (!IsAppReady) continue;

                await UpdateBoolInternetAccess();
            }
        });
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

                int timeoutMS = 5000;
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
                CheckDns checkDns = new(true, false, GetCPUPriority());
                await checkDns.CheckDnsAsync(host, IPAddress.Loopback.ToString(), timeoutMS);
                LocalDnsLatency = checkDns.DnsLatency;
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
                string dohServer;
                if (ConnectedDohPort == 443)
                    dohServer = $"https://{IPAddress.Loopback}/dns-query";
                else
                    dohServer = $"https://{IPAddress.Loopback}:{ConnectedDohPort}/dns-query";
                CheckDns checkDns = new(false, false, GetCPUPriority());
                await checkDns.CheckDnsAsync(host, dohServer, timeoutMS);
                LocalDohLatency = checkDns.DnsLatency;
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

    private async void UpdateBoolProxyAuto()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(500);
                await UpdateBoolProxy();
            }
        });
    }

    private async Task UpdateBoolProxy()
    {
        if (!IsAppReady) return;
        await Task.Run(async () =>
        {
            IsProxyActivated = ProcessManager.FindProcessByPID(PIDProxy);

            string line = string.Empty;
            if (!UpdateProxyBools) return;
            if (IsProxyActivating) return;

            bool isCmdSent = await ProxyConsole.SendCommandAsync("out");

            line = ProxyConsole.GetStdout;
#if DEBUG
            //Debug.WriteLine($"Line({isCmdSent}): " + line);
#endif

            if (!isCmdSent || string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
            {
                IsProxyRunning = false; ProxyRequests = 0; ProxyMaxRequests = 250; IsProxyDpiBypassActive = false;
                IsProxySSLDecryptionActive = false; IsProxySSLChangeSniToIpActive = false;
            }
            else if (line.StartsWith("details"))
            {
                string[] split = line.Split('|');
                if (split.Length == 15)
                {
                    if (bool.TryParse(split[1].ToLower(), out bool sharing)) IsProxyRunning = sharing;
                    if (int.TryParse(split[2].ToLower(), out int port)) ProxyPort = port;
                    if (int.TryParse(split[3].ToLower(), out int requests)) ProxyRequests = requests;
                    if (int.TryParse(split[4].ToLower(), out int maxRequests)) ProxyMaxRequests = maxRequests;
                    if (bool.TryParse(split[5].ToLower(), out bool dpiActive))
                    {
                        IsProxyDpiBypassActive = !IsProxySSLDecryptionActive && dpiActive;
                    }

                    if (split[6].ToLower().Equals("disable")) ProxyStaticDPIBypassMode = ProxyProgram.DPIBypass.Mode.Disable;
                    else if (split[6].ToLower().Equals("program")) ProxyStaticDPIBypassMode = ProxyProgram.DPIBypass.Mode.Program;

                    if (split[7].ToLower().Equals("disable")) ProxyDPIBypassMode = ProxyProgram.DPIBypass.Mode.Disable;
                    else if (split[7].ToLower().Equals("program")) ProxyDPIBypassMode = ProxyProgram.DPIBypass.Mode.Program;

                    if (split[8].ToLower().Equals("disable")) ProxyUpStreamMode = ProxyProgram.UpStreamProxy.Mode.Disable;
                    else if (split[8].ToLower().Equals("http")) ProxyUpStreamMode = ProxyProgram.UpStreamProxy.Mode.HTTP;
                    else if (split[8].ToLower().Equals("socks5")) ProxyUpStreamMode = ProxyProgram.UpStreamProxy.Mode.SOCKS5;

                    if (split[9].ToLower().Equals("disable")) ProxyDNSMode = ProxyProgram.Dns.Mode.Disable;
                    else if (split[9].ToLower().Equals("system")) ProxyDNSMode = ProxyProgram.Dns.Mode.System;
                    else if (split[9].ToLower().Equals("doh")) ProxyDNSMode = ProxyProgram.Dns.Mode.DoH;
                    else if (split[9].ToLower().Equals("plaindns")) ProxyDNSMode = ProxyProgram.Dns.Mode.PlainDNS;

                    if (split[10].ToLower().Equals("disable")) ProxyFakeDnsMode = ProxyProgram.FakeDns.Mode.Disable;
                    else if (split[10].ToLower().Equals("file")) ProxyFakeDnsMode = ProxyProgram.FakeDns.Mode.File;
                    else if (split[10].ToLower().Equals("text")) ProxyFakeDnsMode = ProxyProgram.FakeDns.Mode.Text;

                    if (split[11].ToLower().Equals("disable")) ProxyBWListMode = ProxyProgram.BlackWhiteList.Mode.Disable;
                    else if (split[11].ToLower().Equals("blacklistfile")) ProxyBWListMode = ProxyProgram.BlackWhiteList.Mode.BlackListFile;
                    else if (split[11].ToLower().Equals("blacklisttext")) ProxyBWListMode = ProxyProgram.BlackWhiteList.Mode.BlackListText;
                    else if (split[11].ToLower().Equals("whitelistfile")) ProxyBWListMode = ProxyProgram.BlackWhiteList.Mode.WhiteListFile;
                    else if (split[11].ToLower().Equals("whitelisttext")) ProxyBWListMode = ProxyProgram.BlackWhiteList.Mode.WhiteListText;

                    if (split[12].ToLower().Equals("disable")) DontBypassMode = ProxyProgram.DontBypass.Mode.Disable;
                    else if (split[12].ToLower().Equals("file")) DontBypassMode = ProxyProgram.DontBypass.Mode.File;
                    else if (split[12].ToLower().Equals("text")) DontBypassMode = ProxyProgram.DontBypass.Mode.Text;

                    if (bool.TryParse(split[13].ToLower(), out bool sslDecryptionActive)) IsProxySSLDecryptionActive = sslDecryptionActive;
                    if (bool.TryParse(split[14].ToLower(), out bool sslChangeSniToIpActive)) IsProxySSLChangeSniToIpActive = sslDecryptionActive && sslChangeSniToIpActive;
                }
            }

            // Update Proxy PID (Rare case)
            if (IsProxyRunning) PIDProxy = ProxyConsole.GetPid;
        });
    }

    private bool UpdateBoolIsProxySet()
    {
        bool isAnyProxySet = NetworkTool.IsProxySet(out string proxy, out _, out _, out _); // HTTP
        if (string.IsNullOrEmpty(proxy))
            isAnyProxySet = NetworkTool.IsProxySet(out _, out proxy, out _, out _); // HTTPS
        if (string.IsNullOrEmpty(proxy))
            isAnyProxySet = NetworkTool.IsProxySet(out _, out _, out _, out proxy); // SOCKS
        if (isAnyProxySet)
            if (!string.IsNullOrEmpty(proxy))
                if (proxy.Contains(':'))
                {
                    string[] split = proxy.Split(':');
                    string ip = split[0];
                    string portS = split[1];
                    bool isPortInt = int.TryParse(portS, out int port);
                    if (isPortInt)
                        if (ip == IPAddress.Loopback.ToString() && port == ProxyPort) return true;
                }
        return false;
    }

    private async void UpdateStatusVShortAuto()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(250);
                try
                {
                    if (File.Exists(TheDll) && !IsConnecting)
                    {
                        File.Delete(TheDll);
                        Debug.WriteLine("DLL Deleted.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        });
    }

    private async void UpdateStatusShortAuto()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(500);
                if (Visible) await UpdateStatusShort();
            }
        });
    }

    private async Task UpdateStatusShort()
    {
        int delay = 5;
        
        // Hide Label Main After a period of time
        LabelMainHide();
        await Task.Delay(delay);

        // Update Min Size of Main Container Panel 1
        this.InvokeIt(() => SplitContainerMain.Panel1MinSize = PictureBoxFarvahar.Bottom + PictureBoxFarvahar.Height);
        await Task.Delay(delay);

        // Update Height of Vertical Separator in Share Tab
        int spaceBottom = 6;
        this.InvokeIt(() => CustomLabelShareSeparator2.Height = CustomButtonShare.Top - CustomLabelShareSeparator2.Top - spaceBottom);
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

        // Update Size Of GroupBoxNicStatus
        this.InvokeIt(() =>
        {
            int space = 6;
            CustomGroupBoxNicStatus.Width = TabPageSetDNS.Width - CustomGroupBoxNicStatus.Left - space;
            CustomGroupBoxNicStatus.Height = TabPageSetDNS.Height - CustomGroupBoxNicStatus.Top - space;
        });
        await Task.Delay(delay);

        // Check Button Text
        if (StopChecking) this.InvokeIt(() => CustomButtonCheck.Text = "Stopping...");
        else
            this.InvokeIt(() => CustomButtonCheck.Text = IsCheckingStarted ? "Stop" : "Scan");
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

        // Connect to popular servers using proxy Textbox
        this.InvokeIt(() => CustomTextBoxHTTPProxy.Enabled = CustomRadioButtonConnectDNSCrypt.Checked);
        await Task.Delay(delay);

        // Connect Button Text
        this.InvokeIt(() =>
        {
            if (IsDisconnecting) CustomButtonConnect.Text = "Disconnecting...";
            else if (IsConnecting) CustomButtonConnect.Text = "Stop";
            else CustomButtonConnect.Text = IsConnected ? "Disconnect" : "Connect";
        });
        await Task.Delay(delay);

        // Connect Button Enable
        if (CustomRadioButtonConnectCheckedServers.Checked)
        {
            if (WorkingDnsList.Any())
                this.InvokeIt(() => CustomButtonConnect.Enabled = !IsDisconnecting);
            else
                this.InvokeIt(() => CustomButtonConnect.Enabled = IsConnected);
        }
        else
            this.InvokeIt(() => CustomButtonConnect.Enabled = !IsDisconnecting);
        await Task.Delay(delay);

        // SetDNS Button Text
        this.InvokeIt(() =>
        {
            bool isDnsSetOn = SetDnsOnNic_.IsDnsSet(CustomComboBoxNICs);
            if (IsDNSUnsetting) CustomButtonSetDNS.Text = "Unsetting...";
            else if (IsDNSSetting) CustomButtonSetDNS.Text = "Setting...";
            else CustomButtonSetDNS.Text = isDnsSetOn ? "Unset DNS" : "Set DNS";
        });
        
        await Task.Delay(delay);

        // SetDNS Button Enable
        this.InvokeIt(() => CustomButtonSetDNS.Enabled = (IsConnected && (IsDNSConnected || IsDoHConnected) && !IsDNSSetting && !IsDNSUnsetting));
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
        this.InvokeIt(() => CustomButtonSetProxy.Text = IsProxySet ? "Unset Proxy" : "Set Proxy");
        await Task.Delay(delay);

        // SetProxy Button Enable & Color
        this.InvokeIt(() =>
        {
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
        });
        await Task.Delay(delay);

        // ApplyDpiBypassChanges Button Enable & Color
        this.InvokeIt(() =>
        {
            if (IsProxyActivated && IsProxyRunning && !IsProxyActivating && !IsProxyDeactivating)
            {
                bool proxyChanged = (IsProxyDpiBypassActive != CustomCheckBoxPDpiEnableDpiBypass.Checked && CustomCheckBoxPDpiEnableDpiBypass.Enabled) ||
                                    (CustomCheckBoxPDpiEnableDpiBypass.Checked && CustomCheckBoxPDpiEnableDpiBypass.Enabled && !LastDpiBypassProgramCommand.Equals(GetDpiBypassProgramCommand())) ||
                                    (IsProxySSLDecryptionActive != CustomCheckBoxProxyEnableSSL.Checked) ||
                                    (IsProxySSLChangeSniToIpActive != CustomCheckBoxProxySSLChangeSniToIP.Checked && CustomCheckBoxProxyEnableSSL.Checked);

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

        // Settings -> Quick Connect
        this.InvokeIt(() =>
        {
            ConnectMode cMode = ConnectMode.ConnectToWorkingServers;
            if (CustomComboBoxSettingQcConnectMode.SelectedItem != null)
                cMode = GetConnectModeByName(CustomComboBoxSettingQcConnectMode.SelectedItem.ToString());
            CustomCheckBoxSettingQcUseSavedServers.Enabled = cMode == ConnectMode.ConnectToWorkingServers;
            CustomCheckBoxSettingQcCheckAllServers.Enabled = cMode == ConnectMode.ConnectToWorkingServers && !CustomCheckBoxSettingQcUseSavedServers.Checked;
            CustomCheckBoxSettingQcSetProxy.Enabled = CustomCheckBoxSettingQcStartProxyServer.Checked;
        });
        await Task.Delay(delay);

        // Settings -> Quick Connect -> Startup Button Text
        if (IsOnStartup)
            this.InvokeIt(() => CustomButtonSettingQcStartup.Text = IsStartupPathOk ? "Remove from Stratup" : "Fix Startup");
        else
            this.InvokeIt(() => CustomButtonSettingQcStartup.Text = "Apply to Startup");
        await Task.Delay(delay);

        // Settings -> Share -> Advanced
        this.InvokeIt(() => CustomTextBoxSettingProxyCfCleanIP.Enabled = CustomCheckBoxSettingProxyCfCleanIP.Checked);
        await Task.Delay(delay);
    }

    private void UpdateMinSizeOfStatus()
    {
        // Update Min Size of Status
        SplitContainerTop.IsSplitterFixed = true;
        try
        {
            this.InvokeIt(() =>
            {
                CustomDataGridViewStatus.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                CustomDataGridViewStatus.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                CustomDataGridViewStatus.Left = 5;
                CustomDataGridViewStatus.Top = LabelScreen.Height;
                CustomDataGridViewStatus.Width = CustomDataGridViewStatus.Columns[0].Width + CustomDataGridViewStatus.Columns[1].Width + 3;
                CustomDataGridViewStatus.Height = CustomGroupBoxStatus.Height - LabelScreen.Height - CustomButtonProcessMonitor.Height - 10;
            });

            int minS = CustomDataGridViewStatus.Width + (CustomDataGridViewStatus.Left * 2) + 2;
            int distance = SplitContainerTop.Width - SplitContainerTop.SplitterIncrement - minS;
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

        // Regular DPI Bypass Enable
        this.InvokeIt(() =>
        {
            if (isSSLDecryptionEnable)
            {
                if (CustomCheckBoxPDpiEnableDpiBypass.Enabled)
                {
                    CustomCheckBoxPDpiEnableDpiBypass.Text = "Enable DPI Bypass (Ignored. SSL Decryption is active.)";
                    CustomCheckBoxPDpiEnableDpiBypass.ForeColor = Color.DodgerBlue;
                    CustomCheckBoxPDpiEnableDpiBypass.Enabled = false;
                }
            }
            else
            {
                if (!CustomCheckBoxPDpiEnableDpiBypass.Enabled)
                {
                    CustomCheckBoxPDpiEnableDpiBypass.Text = "Enable DPI Bypass";
                    CustomCheckBoxPDpiEnableDpiBypass.ForeColor = Color.MediumSeaGreen;
                    CustomCheckBoxPDpiEnableDpiBypass.Enabled = true;

                    // Uncheck SSL
                    if (CustomCheckBoxProxyEnableSSL.Checked) CustomCheckBoxProxyEnableSSL.Checked = false;
                }
            }
        });
    }

    private async void UpdateStatusLongAuto()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(1500);
                await Task.Run(async () => await UpdateStatusLong());
            }
        });
    }

    private async Task UpdateStatusLong()
    {
        // Update Int Working Servers
        NumberOfWorkingServers = WorkingDnsList.Count;

        if (CustomDataGridViewStatus.RowCount == 14 && Visible)
        {
            int delay = 50;
            // Update Status Working Servers
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[0].Cells[1].Style.ForeColor = Color.DodgerBlue);
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[0].Cells[1].Value = NumberOfWorkingServers);
            await Task.Delay(delay);
            
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
            string statusLocalDnsLatency = LocalDnsLatency != -1 ? $"{LocalDnsLatency}" : "-1";
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
            string statusLocalDoHLatency = LocalDohLatency != -1 ? $"{LocalDohLatency}" : "-1";
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

            // Update Status IsProxyDPIActive (Fragment Or SSL)
            string textProxyDPI = IsProxyDpiBypassActive || IsProxySSLChangeSniToIpActive ? "Active" : "Inactive";
            Color colorProxyDPI = IsProxyDpiBypassActive || IsProxySSLChangeSniToIpActive ? Color.MediumSeaGreen : Color.IndianRed;
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
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[13].Cells[1].Value = "               ");

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

        // Remove Connect ToolTip
        if (!IsConnected)
            this.InvokeIt(() => CustomButtonConnect.SetToolTip(MainToolTip, string.Empty, string.Empty));
    }

    private void UpdateStatusNic() // Auto Update will increase CPU usage "WmiPrvSE.exe"
    {
        if (Program.Startup) return;
        // Variables
        bool isNicDisabled = false;
        string e = string.Empty;
        string name = e, description = e, adapterType = e, availability = e, status = e, netStatus = e, dnsAddresses = e;
        bool isPhysicalAdapter = false;
        string guid = e, macAddress = e, manufacturer = e, serviceName = e, speed = e, timeOfLastReset = e;

        string? nicName = null;
        object? selectedItem = null;
        this.InvokeIt(() => selectedItem = CustomComboBoxNICs.SelectedItem);
        if (selectedItem != null)
        {
            try { this.InvokeIt(() => nicName = selectedItem.ToString()); } catch (Exception) { }
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
                guid = nis.GUID;
                macAddress = nis.MACAddress;
                manufacturer = nis.Manufacturer;
                serviceName = nis.ServiceName;
                speed = $"{ConvertTool.ConvertByteToHumanRead(nis.Speed / 8)}/s";
                timeOfLastReset = $"{nis.TimeOfLastReset:yyyy/MM/dd HH:mm:ss}";

                this.InvokeIt(() => CustomButtonEnableDisableNic.Enabled = true);
            }
            else
                this.InvokeIt(() => CustomButtonEnableDisableNic.Enabled = false);
        }
        else
            this.InvokeIt(() => CustomButtonEnableDisableNic.Enabled = false);

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

                // Update GUID
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[7].Cells[1].Style.ForeColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomDataGridViewNicStatus.Rows[7].Cells[1].Value = guid);

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
        catch (Exception)
        {
            // do nothing
        }
    }

    private async void UpdateStatusCpuUsageAuto()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(5000);
                if (!Visible) continue;

                await UpdateStatusCpuUsage();
            }
        });
    }

    private async Task UpdateStatusCpuUsage()
    {
        int delay = 1000;
        double cpu = await GetCpuUsage(delay);
        // Update Status CPU Usage
        Color colorCPU = cpu <= 35 ? Color.MediumSeaGreen : Color.IndianRed;

        if (CustomDataGridViewStatus.RowCount == 14 && Visible)
        {
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[12].Cells[1].Style.ForeColor = colorCPU);
            this.InvokeIt(() => CustomDataGridViewStatus.Rows[12].Cells[1].Value = $"{cpu}%");
            UpdateMinSizeOfStatus();
        }
    }

    private async Task<double> GetCpuUsage(int delay)
    {
        if (Program.Startup) return 0;

        float sdc = -1, sdcProxyServer = -1, sdcFakeProxy = -1;
        float dnsproxy1 = -1, dnscrypt1 = -1, goodbyeDpiBasic = -1, goodbyeDpiAdvanced = -1;
        float dnsproxy2 = -1, dnscrypt2 = -1, goodbyeDpiBypass = -1;

        Task a = Task.Run(async () => sdc = await ProcessManager.GetCpuUsage(Process.GetCurrentProcess(), delay));
        Task b = Task.Run(async () => sdcProxyServer = await ProcessManager.GetCpuUsage(PIDProxy, delay));
        Task c = Task.Run(async () => sdcFakeProxy = await ProcessManager.GetCpuUsage(PIDFakeProxy, delay));
        Task d = Task.Run(async () => dnsproxy1 = await ProcessManager.GetCpuUsage(PIDDNSProxy, delay));
        Task e = Task.Run(async () => dnsproxy2 = await ProcessManager.GetCpuUsage(PIDDNSProxyBypass, delay));
        Task f = Task.Run(async () => dnscrypt1 = await ProcessManager.GetCpuUsage(PIDDNSCrypt, delay));
        Task g = Task.Run(async () => dnscrypt2 = await ProcessManager.GetCpuUsage(PIDDNSCryptBypass, delay));
        Task h = Task.Run(async () => goodbyeDpiBasic = await ProcessManager.GetCpuUsage(PIDGoodbyeDPIBasic, delay));
        Task i = Task.Run(async () => goodbyeDpiAdvanced = await ProcessManager.GetCpuUsage(PIDGoodbyeDPIAdvanced, delay));
        Task j = Task.Run(async () => goodbyeDpiBypass = await ProcessManager.GetCpuUsage(PIDGoodbyeDPIBypass, delay));
        
        await Task.WhenAll(a, b, c, d, e, f, g, h, i, j);

        float sum = 0;
        List<float> list = new();
        list.Clear();
        if (sdc != -1) list.Add(sdc);
        if (sdcProxyServer != -1) list.Add(sdcProxyServer);
        if (sdcFakeProxy != -1) list.Add(sdcFakeProxy);
        if (dnsproxy1 != -1) list.Add(dnsproxy1);
        if (dnsproxy2 != -1) list.Add(dnsproxy2);
        if (dnscrypt1 != -1) list.Add(dnscrypt1);
        if (dnscrypt2 != -1) list.Add(dnscrypt2);
        if (goodbyeDpiBasic != -1) list.Add(goodbyeDpiBasic);
        if (goodbyeDpiAdvanced != -1) list.Add(goodbyeDpiAdvanced);
        if (goodbyeDpiBypass != -1) list.Add(goodbyeDpiBypass);

        for (int n = 0; n < list.Count; n++) sum += list[n];
        double result = Math.Round(Convert.ToDouble(sum), 2);
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
            if (Program.Startup)
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