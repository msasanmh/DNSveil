using CustomControls;
using MsmhToolsClass;
using System.Net;
using System.ServiceProcess;

namespace SecureDNSClient;

public partial class FormMain
{
    private async Task<bool> StartConnect(ConnectMode connectMode, bool reconnect = false, bool limitLog = false)
    {
        this.InvokeIt(() => CustomButtonConnect.Enabled = false);

        // Return if binary files are missing
        if (!CheckNecessaryFiles())
        {
            this.InvokeIt(() => CustomButtonConnect.Enabled = true);
            return false;
        }

        // Update Bools
        await UpdateBools();

        bool isConnectSuccess = false;

        if (!IsConnected && !IsConnecting && !IsDisconnecting)
        {
            try
            {
                // Connect
                // Check Internet Connectivity
                if (!IsInternetOnline) return false;

                if (IsConnecting) return false;
                IsConnecting = true;
                this.InvokeIt(() => CustomButtonConnect.Enabled = true);
                await UpdateStatusShortOnBoolsChanged();

                // Update NICs
                await SetDnsOnNic_.UpdateNICs(CustomComboBoxNICs, GetBootstrapSetting(out int port), port);

                if (connectMode != ConnectMode.Unknown)
                {
                    string icsMsg = $"Checking For ICS Service Port Conflict...{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(icsMsg, Color.DarkOrange));

                    // Get Svchost Stat
                    string hnsService = "HNS";
                    string icsService = "SharedAccess";
                    ServiceTool.GetStatus(hnsService, out ServiceControllerStatus? hnsStatus, out ServiceStartMode? hnsStartMode);
                    ServiceTool.GetStatus(icsService, out ServiceControllerStatus? icsStatus, out ServiceStartMode? icsStartMode);
                    if (icsStatus != null)
                    {
                        if (icsStatus == ServiceControllerStatus.Running || icsStatus == ServiceControllerStatus.StartPending)
                        {
                            icsMsg = $"Temporary Disabling ICS Service...{NL}";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(icsMsg, Color.DarkOrange));

                            // Stop Svchost
                            await ServiceTool.ChangeStartModeAsync(hnsService, ServiceStartMode.Disabled);
                            await ServiceTool.ChangeStatusAsync(hnsService, ServiceControllerStatus.Stopped);
                            await ServiceTool.ChangeStartModeAsync(icsService, ServiceStartMode.Disabled);
                            await ServiceTool.ChangeStatusAsync(icsService, ServiceControllerStatus.Stopped);
                        }
                    }

                    bool revertBack = false;
                    ServiceTool.GetStatus(icsService, out ServiceControllerStatus? icsStatus2, out _);
                    if (icsStatus != null && icsStatus2 != null)
                    {
                        if (!icsStatus.Equals(icsStatus2))
                        {
                            icsMsg = $"ICS Service Status Changed To: {icsStatus2}{NL}";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(icsMsg, Color.DarkOrange));
                            revertBack = true;
                        }
                    }

                    // Start Connect
                    isConnectSuccess = await ConnectAsync(connectMode, limitLog);
                    bool retry = false;
                    this.InvokeIt(() => retry = CustomCheckBoxSettingConnectRetry.Checked);
                    if (!isConnectSuccess && retry && !IsDisconnecting && !IsDisconnectingAll && !IsQuickDisconnecting)
                    {
                        // Kill Processes
                        await ProcessManager.KillProcessByPidAsync(PIDDnsServer);
                        await ProcessManager.KillProcessByPidAsync(PIDGoodbyeDPIBypass);

                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"{NL}Retying...{NL}"));
                        isConnectSuccess = await ConnectAsync(connectMode, limitLog);
                    }

                    if (revertBack)
                    {
                        icsMsg = $"Enabling ICS Service...{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(icsMsg, Color.DarkOrange));

                        // Set Svchost To Previous State
                        await ServiceTool.ChangeStartModeAsync(hnsService, hnsStartMode);
                        await ServiceTool.ChangeStatusAsync(hnsService, hnsStatus);
                        await ServiceTool.ChangeStartModeAsync(icsService, icsStartMode);
                        await ServiceTool.ChangeStatusAsync(icsService, icsStatus);

                        ServiceTool.GetStatus(icsService, out ServiceControllerStatus? icsStatus3, out ServiceStartMode? icsStartMode3);
                        if (icsStatus2 != null && icsStatus3 != null)
                        {
                            if (!icsStatus2.Equals(icsStatus3))
                            {
                                icsMsg = $"ICS Service Status: {icsStatus3}{NL}";
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(icsMsg, Color.DarkOrange));
                            }
                            else
                            {
                                if (icsStartMode3 != null && icsStartMode3 == ServiceStartMode.Disabled)
                                {
                                    icsMsg = $"ICS Service {icsStatus3}, Because Startup Type Is {icsStartMode3}.{NL}";
                                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(icsMsg, Color.DarkOrange));
                                }
                            }
                        }
                    }
                }

                if (isConnectSuccess)
                {
                    // Create uid
                    SecureDNS.GenerateUid(this);
                }

                IsConnecting = false;
                await UpdateStatusShortOnBoolsChanged();

                string msg = $"{NL}Connect Task: {isConnectSuccess}{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(this, ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            try
            {
                // Disconnect
                if (IsDisconnecting) return false;
                IsDisconnecting = true;
                this.InvokeIt(() => CustomButtonConnect.Enabled = true);
                await UpdateStatusShortOnBoolsChanged();

                // Write Disconnecting message to log
                string msgDisconnecting = $"{NL}Disconnecting...{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDisconnecting, Color.MediumSeaGreen));

                // Wait for Disconnect
                Task wait1 = Task.Run(async () =>
                {
                    while (true)
                    {
                        await disconnectAsync();
                        await Task.Delay(200);
                        await UpdateBools();
                        if (!IsConnecting && !IsConnected) break;
                    }
                });
                try { await wait1.WaitAsync(TimeSpan.FromSeconds(30)); } catch (Exception) { }

                static async Task disconnectAsync()
                {
                    // Kill Processes
                    await ProcessManager.KillProcessByPidAsync(PIDDnsServer);
                    await ProcessManager.KillProcessByPidAsync(PIDGoodbyeDPIBypass);
                }

                // To See Status Immediately
                PIDDnsServer = -1;
                LocalDnsLatency = -1;
                IsDNSConnected = LocalDnsLatency != -1;
                LocalDohLatency = -1;
                IsDoHConnected = LocalDohLatency != -1;
                
                IsDisconnecting = false;
                await UpdateStatusShortOnBoolsChanged();
                await UpdateStatusLong();

                // Write Disconnected message to log
                string msgDisconnected = $"Disconnected.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDisconnected, Color.MediumSeaGreen));

                // Write Connect Status
                this.InvokeIt(() => CustomRichTextBoxConnectStatus.ResetText());
                this.InvokeIt(() => CustomRichTextBoxConnectStatus.AppendText(msgDisconnected));

                // Reconnect
                if (!StopQuickConnect) // Make Quick Connect Cancel Faster
                    if (reconnect) isConnectSuccess = await StartConnect(connectMode, false, limitLog);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(this, ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        return isConnectSuccess;
    }

    private async Task<bool> ConnectAsync(ConnectMode connectMode, bool limitLog = false)
    {
        // Write Connecting Message To Log
        string msgConnecting = "Connecting... Please Wait..." + NL + NL;
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnecting, Color.MediumSeaGreen));

        // Solve: "bind: An attempt was made to access a socket in a way forbidden by its access permissions"
        if (!Program.IsStartup) await NetworkTool.RestartNATDriver();

        // Check Plain DNS Port
        List<int> pids = ProcessManager.GetProcessPidsByUsingPort(53);
        foreach (int pid in pids) await ProcessManager.KillProcessByPidAsync(pid);
        await Task.Delay(5);
        pids = ProcessManager.GetProcessPidsByUsingPort(53);
        foreach (int pid in pids) await ProcessManager.KillProcessByPidAsync(pid);

        bool portDns = GetListeningPort(53, "You Need To Resolve The Conflict.", Color.IndianRed);
        if (!portDns)
        {
            IsEverythingDisconnected(out string stat);
            stat = $"{NL}Port 53 Is Occupied:{NL}{stat}";
            await LogToDebugFileAsync(stat);

            IsConnecting = false;
            return false;
        }

        // Check DoH port if working mode is set to Plain DNS and DoH
        if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
        {
            int dohPort = GetDohPortSetting();
            pids = ProcessManager.GetProcessPidsByUsingPort(dohPort);
            foreach (int pid in pids) await ProcessManager.KillProcessByPidAsync(pid);
            bool portDoH = GetListeningPort(dohPort, "You Need To Resolve The Conflict.", Color.IndianRed);
            if (!portDoH)
            {
                IsConnecting = false;
                return false;
            }
        }

        // Generate and Install Certificate for DoH
        if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
        {
            await GenerateCertificate();
            await InstallCertificateForDoH();
        }

        // Connect Modes
        if (connectMode == ConnectMode.Unknown) return false;
        if (connectMode == ConnectMode.ConnectToWorkingServers)
        {
            //=== Connect To Working Servers
            bool connected = await ConnectToWorkingServersAsync();
            if (connected)
            {
                internalConnect();
                connectMessage();
                return true;
            }
            else
            {
                await ProcessManager.KillProcessByPidAsync(PIDDnsServer);
            }
        }
        else if (connectMode == ConnectMode.ConnectToFakeProxyDohViaProxyDPI)
        {
            //=== Connect To Fake Proxy Via Proxy Fragment
            bool connected = await ConnectToFakeProxyDohUsingProxyDPIAsync();
            if (connected)
            {
                internalConnect();
                connectMessage();
                return true;
            }
            else
            {
                await ProcessManager.KillProcessByPidAsync(PIDDnsServer);
            }
        }
        else if (connectMode == ConnectMode.ConnectToFakeProxyDohViaGoodbyeDPI)
        {
            //=== Connect To Fake Proxy Via GoodbyeDPI
            bool connected = await ConnectToFakeProxyDohUsingGoodbyeDPIAsync();
            if (connected)
            {
                internalConnect();
                connectMessage();
                return true;
            }
            else
            {
                await ProcessManager.KillProcessByPidAsync(PIDDnsServer);
                await ProcessManager.KillProcessByPidAsync(PIDGoodbyeDPIBypass);
            }
        }
        else if (connectMode == ConnectMode.ConnectToPopularServersWithProxy)
        {
            //=== Connect To Servers Using Proxy
            bool connected = await ConnectToServersUsingProxy();
            if (connected)
            {
                internalConnect();
                connectMessage();
                return true;
            }
            else
            {
                await ProcessManager.KillProcessByPidAsync(PIDDnsServer);
            }
        }

        return false;

        void internalConnect()
        {
            // To see online status immediately
            Task.Run(async () => await UpdateBoolDnsOnce(GetCheckTimeoutSetting() * 2, GetBlockedDomainSetting(out string _)));
            Task.Run(async () => await UpdateBoolDohOnce(GetCheckTimeoutSetting() * 2, GetBlockedDomainSetting(out string _)));

            // Update Groupbox Status
            Task.Run(() => UpdateStatusLong());
        }

        void connectMessage(bool writeToLog = true)
        {
            // Set LastConnectMode
            LastConnectMode = connectMode;

            // Connect Status - Clear
            this.InvokeIt(() => CustomRichTextBoxConnectStatus.ResetText());

            // Connect Status - Connect Mode
            string connectModeName = GetConnectModeNameByConnectMode(LastConnectMode);
            this.InvokeIt(() => CustomRichTextBoxConnectStatus.AppendText($"Connect Mode:{NL}"));
            this.InvokeIt(() => CustomRichTextBoxConnectStatus.AppendText($"{connectModeName}{NL}", Color.DodgerBlue));

            // Connect Status - Group Name
            if (LastConnectMode == ConnectMode.ConnectToWorkingServers && CurrentUsingCustomServersList.Any())
            {
                List<DnsInfo> current = CurrentUsingCustomServersList.ToList();
                this.InvokeIt(() => CustomRichTextBoxConnectStatus.AppendText($"{NL}Using Servers:{NL}"));
                for (int n = 0; n < current.Count; n++)
                {
                    DnsInfo dnsInfo = current[n];
                    this.InvokeIt(() => CustomRichTextBoxConnectStatus.AppendText($"{n + 1}. "));
                    this.InvokeIt(() => CustomRichTextBoxConnectStatus.AppendText(dnsInfo.GroupName + NL, Color.DodgerBlue));

                    if (dnsInfo.Latency > 0)
                    {
                        this.InvokeIt(() => CustomRichTextBoxConnectStatus.AppendText("Latency: "));
                        this.InvokeIt(() => CustomRichTextBoxConnectStatus.AppendText($"{dnsInfo.Latency}", Color.DodgerBlue));
                        this.InvokeIt(() => CustomRichTextBoxConnectStatus.AppendText($" ms{NL}"));
                    }
                    
                    string reducted = "Reducted";
                    string dns = dnsInfo.CheckMode == CheckMode.BuiltIn || dnsInfo.CheckMode == CheckMode.SavedServers ? reducted : dnsInfo.DNS;
                    string msgDnsAddress = dns.Equals(reducted) ? "DNS Address: " : $"DNS Address:{NL}";
                    this.InvokeIt(() => CustomRichTextBoxConnectStatus.AppendText(msgDnsAddress));
                    this.InvokeIt(() => CustomRichTextBoxConnectStatus.AppendText($"{dns}{NL}{NL}", Color.DarkGray));
                }
            }

            // Is IPv6 Supported By OS
            bool isIPv6SupportedByOS = NetworkTool.IsIPv6Supported();

            // Update Local IP
            LocalIP = NetworkTool.GetLocalIPv4();

            // Get Loopback IP
            IPAddress loopbackIPv4 = IPAddress.Loopback;
            IPAddress loopbackIPv6 = IPAddress.IPv6Loopback;

            // Write local DNS addresses to log
            string msgLocalDNS1 = "Local DNS:";
            if (!limitLog)
                if (writeToLog) this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDNS1, Color.LightGray));
            string msgLocalDNS2 = $"{NL}udp://{loopbackIPv4}, tcp://{loopbackIPv4}";
            if (isIPv6SupportedByOS)
                msgLocalDNS2 += $"{NL}udp://[{loopbackIPv6}], tcp://[{loopbackIPv6}]";
            if (LocalIP != null)
                msgLocalDNS2 += $"{NL}udp://{LocalIP}, tcp://{LocalIP}";
            msgLocalDNS2 += NL;
            if (!limitLog)
                if (writeToLog) this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDNS2, Color.DodgerBlue));

            // Connect Status - Local DNS
            this.InvokeIt(() => CustomRichTextBoxConnectStatus.AppendText($"{NL}{msgLocalDNS1}"));
            this.InvokeIt(() => CustomRichTextBoxConnectStatus.AppendText($"{msgLocalDNS2}", Color.DodgerBlue));

            // Write local DoH addresses to log
            if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
            {
                string msgLocalDoH1 = "Local DNS Over HTTPS:";
                if (!limitLog)
                    if (writeToLog) this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDoH1, Color.LightGray));

                string msgLocalDoH2;
                if (ConnectedDohPort == 443)
                    msgLocalDoH2 = $"{NL}https://{loopbackIPv4}/dns-query";
                else
                    msgLocalDoH2 = $"{NL}https://{loopbackIPv4}:{ConnectedDohPort}/dns-query";
                if (isIPv6SupportedByOS)
                {
                    if (ConnectedDohPort == 443)
                        msgLocalDoH2 += $"{NL}https://[{loopbackIPv6}]/dns-query";
                    else
                        msgLocalDoH2 += $"{NL}https://[{loopbackIPv6}]:{ConnectedDohPort}/dns-query";
                }
                msgLocalDoH2 += NL;
                if (!limitLog)
                    if (writeToLog) this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDoH2, Color.DodgerBlue));

                // Connect Status - Local DoH Loopback IP
                this.InvokeIt(() => CustomRichTextBoxConnectStatus.AppendText($"{NL}{msgLocalDoH1}"));
                this.InvokeIt(() => CustomRichTextBoxConnectStatus.AppendText($"{msgLocalDoH2}", Color.DodgerBlue));

                if (LocalIP != null)
                {
                    string msgLocalDoH3;
                    if (ConnectedDohPort == 443)
                        msgLocalDoH3 = $"https://{LocalIP}/dns-query";
                    else
                        msgLocalDoH3 = $"https://{LocalIP}:{ConnectedDohPort}/dns-query";
                    msgLocalDoH3 += NL;
                    if (!limitLog)
                        if (writeToLog) this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDoH3, Color.DodgerBlue));

                    // Connect Status - Local DoH Local IP
                    this.InvokeIt(() => CustomRichTextBoxConnectStatus.AppendText($"{msgLocalDoH3}", Color.DodgerBlue));
                }
            }
        }
    }
}