using CustomControls;
using MsmhToolsClass;
using MsmhToolsClass.ProxyServerPrograms;
using MsmhToolsWinFormsClass;
using System.Diagnostics;
using System.Media;
using System.Net;
using System.Reflection;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace SecureDNSClient;

public partial class FormMain
{
    private async void UpdateNotifyIconIconAuto()
    {
        NotifyIcon ni = NotifyIconMain;

        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(2000);
                if (IsInAction(false, true, true, true, true, true, true, true, true, true, true))
                {
                    await Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (!IsInAction(false, true, true, true, true, true, true, true, true, true, true)) break;

                            ni.Text = "In Action...";

                            // Loading
                            ni.Icon = Properties.Resources.SecureDNSClient_B_Multi;
                            await Task.Delay(200);
                            ni.Icon = Properties.Resources.SecureDNSClient_BR_Multi;
                            await Task.Delay(200);
                            ni.Icon = Properties.Resources.SecureDNSClient_R_Multi;
                            await Task.Delay(200);
                            ni.Icon = Properties.Resources.SecureDNSClient_RB_Multi;
                            await Task.Delay(200);
                        }
                    });
                }
                else
                {
                    if (!IsDNSConnected & !IsDNSSet & !IsProxyRunning & !IsProxySet & !IsDPIActive)
                        ni.Icon = Properties.Resources.SecureDNSClient_R_Multi;
                    else if (IsDNSConnected & IsDNSSet & IsProxyRunning & IsProxySet & IsDPIActive)
                        ni.Icon = Properties.Resources.SecureDNSClient_B_Multi;
                    else if (!IsDNSConnected & !IsDNSSet & IsProxyRunning & IsProxySet & IsDPIActive)
                        ni.Icon = Properties.Resources.SecureDNSClient_RB_Multi;
                    else if (!IsDNSConnected & !IsDNSSet & IsProxyRunning & IsDPIActive)
                        ni.Icon = Properties.Resources.SecureDNSClient_RB_Multi;
                    else if (IsDNSConnected & IsDNSSet & !IsProxyRunning & !IsProxySet & !IsDPIActive)
                        ni.Icon = Properties.Resources.SecureDNSClient_RB_Multi;
                    else if (IsDNSConnected & IsDNSSet & !IsProxyRunning & !IsDPIActive)
                        ni.Icon = Properties.Resources.SecureDNSClient_RB_Multi;
                    else
                        ni.Icon = Properties.Resources.SecureDNSClient_BR_Multi;

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

                    try { ni.Text = msg; } catch (Exception) { }
                }
            }
        });
    }

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

    private async Task CheckUpdate(bool showMsg = false)
    {
        if (IsCheckingStarted) return;
        if (!IsInternetAlive(true)) return;
        if (IsCheckingForUpdate) return;
        if (showMsg)
            IsCheckingForUpdate = true;

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
        System.Timers.Timer logAutoClearTimer = new(5000);
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
        // Update Camouflage Bools
        IsBypassProxyActive = ProcessManager.FindProcessByPID(PIDCamouflageProxy);
        IsBypassDNSActive = CamouflageDNSServer != null && CamouflageDNSServer.IsRunning && !IsConnecting;
        IsBypassGoodbyeDpiActive = ProcessManager.FindProcessByPID(PIDGoodbyeDPIBypass);

        // In Case Camouflage Proxy Terminated
        if (LastConnectMode == ConnectMode.ConnectToFakeProxyDohViaProxyDPI && !IsBypassProxyActive)
            ProcessManager.KillProcessByPID(PIDDNSCryptBypass);

        // In Case Camouflage GoodbyeDPI Terminated
        if (LastConnectMode == ConnectMode.ConnectToFakeProxyDohViaGoodbyeDPI && !IsBypassGoodbyeDpiActive &&
            IsBypassDNSActive)
            ProcessManager.KillProcessByPID(PIDDNSProxyBypass);

        // Update bool IsConnected
        IsConnected = ProcessManager.FindProcessByPID(PIDDNSProxy) ||
                      ProcessManager.FindProcessByPID(PIDDNSCryptBypass) ||
                      ProcessManager.FindProcessByPID(PIDDNSProxyBypass) ||
                      ProcessManager.FindProcessByPID(PIDDNSCrypt);

        // In Case Dnsproxy or Dnscrypt Processes Terminated
        if (!IsConnected && !IsConnecting && !IsDisconnecting &&
            !IsQuickConnectWorking && !IsQuickConnecting && !IsQuickDisconnecting && !StopQuickConnect)
        {
            IsDNSConnected = IsDoHConnected = IsConnected;
            LocalDnsLatency = LocalDohLatency = -1;
            BypassFakeProxyDohStop(true, true, true, false);
            if (IsDNSSet) await UnsetAllDNSs();
        }

        // In Case Fake Proxy Terminated
        if (!ProcessManager.FindProcessByPID(PIDFakeProxy) &&
            ProxyDNSMode == ProxyProgram.Dns.Mode.DoH &&
            IsProxyRunning)
            ProcessManager.KillProcessByPID(PIDProxy);

        // In Case Proxy Server Terminated
        if (!IsProxyActivated && !IsProxyActivating && !IsProxyDeactivating && !IsProxyRunning)
        {
            ProcessManager.KillProcessByPID(PIDFakeProxy);
            if (IsProxySet)
                NetworkTool.UnsetProxy(false, true);
        }

        // Update bool IsDnsSet
        IsDNSSet = SetDnsOnNic_.IsDnsSet();

        // Update bool IsProxySet
        IsProxySet = UpdateBoolIsProxySet();

        // Update bool IsGoodbyeDPIActive
        IsGoodbyeDPIBasicActive = ProcessManager.FindProcessByPID(PIDGoodbyeDPIBasic);
        IsGoodbyeDPIAdvancedActive = ProcessManager.FindProcessByPID(PIDGoodbyeDPIAdvanced);

        // Update bool IsDPIActive
        IsDPIActive = IsProxyDpiBypassActive || IsGoodbyeDPIBasicActive || IsGoodbyeDPIAdvancedActive;
    }

    private async void UpdateBoolDnsDohAuto()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                string blockedDomain = GetBlockedDomainSetting(out string _);
                if (string.IsNullOrEmpty(blockedDomain)) blockedDomain = "google.com";
                int timeout = 10000;
                await Task.Delay(timeout + 500);
                Parallel.Invoke(
                () => UpdateBoolDnsOnce(timeout, blockedDomain),
                () => UpdateBoolDohOnce(timeout, blockedDomain)
                );
            }
        });
    }

    private void UpdateBoolDnsOnce(int timeoutMS, string host = "google.com")
    {
        if (IsConnected)
        {
            // DNS
            CheckDns checkDns = new(false, GetCPUPriority());
            checkDns.CheckDNS(host, IPAddress.Loopback.ToString(), timeoutMS);
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

    private void UpdateBoolDohOnce(int timeoutMS, string host = "google.com")
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
            checkDns.CheckDNS(host, dohServer, timeoutMS);
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

    private async void UpdateBoolProxyAuto()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(500);

                IsProxyActivated = ProcessManager.FindProcessByPID(PIDProxy);

                string line = string.Empty;
                if (!UpdateProxyBools) continue;
                if (IsProxyActivating) continue;
                //if (CheckDpiBypassCTS != null && CheckDpiBypassCTS.IsCancellationRequested) continue;

                bool isCmdSent = await ProxyConsole.SendCommandAsync("out");

                line = ProxyConsole.GetStdout;
#if DEBUG
                Debug.WriteLine($"Line({isCmdSent}): " + line);
#endif

                if (!isCmdSent || string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
                {
                    IsProxyRunning = false; ProxyRequests = 0; ProxyMaxRequests = 250; IsProxyDpiBypassActive = false;
                }
                else if (line.StartsWith("details"))
                {
                    string[] split = line.Split('|');
                    if (bool.TryParse(split[1].ToLower(), out bool sharing)) IsProxyRunning = sharing;
                    if (int.TryParse(split[2].ToLower(), out int port)) ProxyPort = port;
                    if (int.TryParse(split[3].ToLower(), out int requests)) ProxyRequests = requests;
                    if (int.TryParse(split[4].ToLower(), out int maxRequests)) ProxyMaxRequests = maxRequests;
                    if (bool.TryParse(split[5].ToLower(), out bool dpiActive)) IsProxyDpiBypassActive = dpiActive;

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

                }

                // Update Proxy PID (Rare case)
                if (IsProxyRunning) PIDProxy = ProxyConsole.GetPid;
            }
        });
    }

    private bool UpdateBoolIsProxySet()
    {
        bool isAnyProxySet = NetworkTool.IsProxySet(out string _, out string _, out string _, out string proxy);
        if (isAnyProxySet)
            if (!string.IsNullOrEmpty(proxy))
                if (proxy.Contains(':'))
                {
                    string[] split = proxy.Split(':');
                    string ip = split[0];
                    string portS = split[1];
                    bool isPortInt = int.TryParse(portS, out int port);
                    if (isPortInt)
                        if (ip == IPAddress.Loopback.ToString() && port == ProxyPort)
                            return true;
                }
        return false;
    }

    private async void UpdateStatusShortAuto()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(300);
                this.InvokeIt(() => UpdateStatusShort());
            }
        });
    }

    private void UpdateStatusShort()
    {
        // Hide Label Moving After a period of time
        LabelMovingHide();

        // Update Min Size of Main Container Panel 1
        SplitContainerMain.Panel1MinSize = PictureBoxFarvahar.Bottom + PictureBoxFarvahar.Height;

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
                SplitContainerTop.SplitterDistance = distance;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Splitter Distance: " + ex.Message);
        }

        // Stop Check Timer
        if (!IsCheckingStarted)
            CustomProgressBarCheck.StopTimer = true;

        // Insecure and parallel CheckBox
        if (CustomCheckBoxInsecure.Checked)
            CustomCheckBoxCheckInParallel.Checked = false;

        // Check Button Text
        if (StopChecking) CustomButtonCheck.Text = "Stopping...";
        else
            CustomButtonCheck.Text = IsCheckingStarted ? "Stop" : "Scan";

        // Check Button Enable
        CustomButtonCheck.Enabled = !IsConnecting && !StopChecking;

        // Quick Connect Button Text
        CustomButtonQuickConnect.Text = StopQuickConnect ? "Stopping QC" : (IsQuickConnecting && IsCheckingStarted && StopChecking) ? "Skipping" : (IsQuickConnecting && IsCheckingStarted) ? "Skip Scan" : IsQuickConnecting ? "Stop QC" : "Quick Connect";

        // Quick Connect Button Enable
        CustomButtonQuickConnect.Enabled = !StopQuickConnect && !(IsQuickConnecting && IsCheckingStarted && StopChecking);

        // Connect to popular servers using proxy Textbox
        CustomTextBoxHTTPProxy.Enabled = CustomRadioButtonConnectDNSCrypt.Checked;

        // Connect Button Text
        if (IsDisconnecting) CustomButtonConnect.Text = "Disconnecting...";
        else if (IsConnecting) CustomButtonConnect.Text = "Stop";
        else CustomButtonConnect.Text = IsConnected ? "Disconnect" : "Connect";

        // Connect Button Enable
        if (CustomRadioButtonConnectCheckedServers.Checked)
        {
            if (WorkingDnsList.Any())
                CustomButtonConnect.Enabled = !IsDisconnecting;
            else
                CustomButtonConnect.Enabled = IsConnected;
        }
        else
        {
            CustomButtonConnect.Enabled = !IsDisconnecting;
        }

        // SetDNS Button Text
        bool isDnsSetOn = SetDnsOnNic_.IsDnsSet(CustomComboBoxNICs);
        if (IsDNSUnsetting) CustomButtonSetDNS.Text = "Unsetting...";
        else if (IsDNSSetting) CustomButtonSetDNS.Text = "Setting...";
        else CustomButtonSetDNS.Text = isDnsSetOn ? "Unset DNS" : "Set DNS";

        // SetDNS Button Enable
        CustomButtonSetDNS.Enabled = (IsConnected && (IsDNSConnected || IsDoHConnected) && !IsDNSSetting && !IsDNSUnsetting);

        // StartProxy Buttom Text
        if (IsProxyDeactivating) CustomButtonShare.Text = "Stopping...";
        else if (IsProxyActivating) CustomButtonShare.Text = "Starting...";
        else CustomButtonShare.Text = IsProxyActivated ? "Stop Proxy" : "Start Proxy";

        // StartProxy Buttom Enable
        CustomButtonShare.Enabled = !IsProxyActivating && !IsProxyDeactivating;

        // SetProxy Button Text
        CustomButtonSetProxy.Text = IsProxySet ? "Unset Proxy" : "Set Proxy";

        // SetProxy Button Enable
        CustomButtonSetProxy.Enabled = IsProxyActivated && IsProxyRunning;

        // GoodbyeDPI Basic Activate/Reactivate Button Text
        CustomButtonDPIBasicActivate.Text = IsGoodbyeDPIBasicActive ? "Reactivate" : "Activate";

        // GoodbyeDPI Basic Deactivate Button Enable
        CustomButtonDPIBasicDeactivate.Enabled = IsGoodbyeDPIBasicActive;

        // GoodbyeDPI Advanced Activate/Reactivate Button Text
        CustomButtonDPIAdvActivate.Text = IsGoodbyeDPIAdvancedActive ? "Reactivate" : "Activate";

        // GoodbyeDPI Advanced Deactivate Button Enable
        CustomButtonDPIAdvDeactivate.Enabled = IsGoodbyeDPIAdvancedActive;

        // Settings -> Quick Connect
        ConnectMode cMode = GetConnectModeByName(CustomComboBoxSettingQcConnectMode.SelectedItem.ToString());
        CustomCheckBoxSettingQcCheckAllServers.Enabled = cMode == ConnectMode.ConnectToWorkingServers;
        CustomCheckBoxSettingQcSetProxy.Enabled = CustomCheckBoxSettingQcStartProxyServer.Checked;

        // Settings -> Share -> Advanced
        CustomTextBoxSettingProxyCfCleanIP.Enabled = CustomCheckBoxSettingProxyCfCleanIP.Checked;
    }

    private async void UpdateStatusLongAuto()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(1000);
                await Task.Run(() => UpdateStatusLong());
            }
        });
    }

    private void UpdateStatusLong()
    {
        // Update Status Working Servers
        NumberOfWorkingServers = WorkingDnsList.Count;
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[0].Cells[1].Style.ForeColor = Color.DodgerBlue);
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[0].Cells[1].Value = NumberOfWorkingServers);

        // Update Status IsConnected
        string textConnect = IsConnected ? "Yes" : "No";
        Color colorConnect = IsConnected ? Color.MediumSeaGreen : Color.IndianRed;
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[1].Cells[1].Style.ForeColor = colorConnect);
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[1].Cells[1].Value = textConnect);

        // Update Status IsDNSConnected
        string statusLocalDNS = IsDNSConnected ? "Online" : "Offline";
        Color colorStatusLocalDNS = IsDNSConnected ? Color.MediumSeaGreen : Color.IndianRed;
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[2].Cells[1].Style.ForeColor = colorStatusLocalDNS);
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[2].Cells[1].Value = statusLocalDNS);

        // Update Status LocalDnsLatency
        string statusLocalDnsLatency = LocalDnsLatency != -1 ? $"{LocalDnsLatency}" : "-1";
        Color colorStatusLocalDnsLatency = LocalDnsLatency != -1 ? Color.MediumSeaGreen : Color.IndianRed;
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[3].Cells[1].Style.ForeColor = colorStatusLocalDnsLatency);
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[3].Cells[1].Value = statusLocalDnsLatency);

        // Update Status IsDoHConnected
        string statusLocalDoH = IsDoHConnected ? "Online" : "Offline";
        Color colorStatusLocalDoH = IsDoHConnected ? Color.MediumSeaGreen : Color.IndianRed;
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[4].Cells[1].Style.ForeColor = colorStatusLocalDoH);
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[4].Cells[1].Value = statusLocalDoH);

        // Update Status LocalDohLatency
        string statusLocalDoHLatency = LocalDohLatency != -1 ? $"{LocalDohLatency}" : "-1";
        Color colorStatusLocalDoHLatency = LocalDohLatency != -1 ? Color.MediumSeaGreen : Color.IndianRed;
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[5].Cells[1].Style.ForeColor = colorStatusLocalDoHLatency);
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[5].Cells[1].Value = statusLocalDoHLatency);

        // Update Status IsDnsSet
        string textDNS = IsDNSSet ? "Yes" : "No";
        Color colorDNS = IsDNSSet ? Color.MediumSeaGreen : Color.IndianRed;
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[6].Cells[1].Style.ForeColor = colorDNS);
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[6].Cells[1].Value = textDNS);

        // Update Status IsSharing
        string textSharing = IsProxyRunning ? "Yes" : "No";
        Color colorSharing = IsProxyRunning ? Color.MediumSeaGreen : Color.IndianRed;
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[7].Cells[1].Style.ForeColor = colorSharing);
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[7].Cells[1].Value = textSharing);

        // Update Status ProxyRequests
        string textProxyRequests = "0 of 0";
        Color colorProxyRequests = Color.MediumSeaGreen;
        textProxyRequests = $"{ProxyRequests} of {ProxyMaxRequests}";
        colorProxyRequests = ProxyRequests < ProxyMaxRequests ? Color.MediumSeaGreen : Color.IndianRed;
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[8].Cells[1].Style.ForeColor = colorProxyRequests);
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[8].Cells[1].Value = textProxyRequests);

        // Update Status IsProxySet
        string textProxySet = IsProxySet ? "Yes" : "No";
        Color colorProxySet = IsProxySet ? Color.MediumSeaGreen : Color.IndianRed;
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[9].Cells[1].Style.ForeColor = colorProxySet);
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[9].Cells[1].Value = textProxySet);

        // Update Status IsProxyDPIActive
        string textProxyDPI = IsProxyDpiBypassActive ? "Active" : "Inactive";
        Color colorProxyDPI = IsProxyDpiBypassActive ? Color.MediumSeaGreen : Color.IndianRed;
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[10].Cells[1].Style.ForeColor = colorProxyDPI);
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[10].Cells[1].Value = textProxyDPI);

        // Update Status IsGoodbyeDPIActive
        string textGoodbyeDPI = (IsGoodbyeDPIBasicActive || IsGoodbyeDPIAdvancedActive) ? "Active" : "Inactive";
        Color colorGoodbyeDPI = (IsGoodbyeDPIBasicActive || IsGoodbyeDPIAdvancedActive) ? Color.MediumSeaGreen : Color.IndianRed;
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[11].Cells[1].Style.ForeColor = colorGoodbyeDPI);
        this.InvokeIt(() => CustomDataGridViewStatus.Rows[11].Cells[1].Value = textGoodbyeDPI);

        // Play Audio Alert
        if (!CustomCheckBoxSettingDisableAudioAlert.Checked && !IsCheckingStarted && !IsExiting)
        {
            if (!StopWatchAudioAlertDelay.IsRunning) StopWatchAudioAlertDelay.Start();
            if (StopWatchAudioAlertDelay.ElapsedMilliseconds > 5000)
                PlayAudioAlert();
        }

        // Bar Color
        if (!IsInAction(false, true, true, true, true, true, true, true, true, true, true))
            this.InvokeIt(() => SplitContainerMain.BackColor = IsDNSConnected ? Color.MediumSeaGreen : Color.IndianRed);
        else
            this.InvokeIt(() => SplitContainerMain.BackColor = Color.DodgerBlue);

        // Remove Connect ToolTip
        if (!IsConnected)
            this.InvokeIt(() => CustomButtonConnect.SetToolTip(MainToolTip, string.Empty, string.Empty));
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

                this.InvokeIt(() => CustomDataGridViewStatus.Rows[12].Cells[1].Style.ForeColor = colorCPU);
                this.InvokeIt(() => CustomDataGridViewStatus.Rows[12].Cells[1].Value = $"{cpu}%");
            }
        });
    }

    private async Task<double> GetCpuUsage(int delay)
    {
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

    private void AutoSaveSettings()
    {
        // Using System.Timers.Timer needs Invoke.
        System.Windows.Forms.Timer autoSaveTimer = new();
        autoSaveTimer.Interval = Convert.ToInt32(TimeSpan.FromMinutes(2).TotalMilliseconds);
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