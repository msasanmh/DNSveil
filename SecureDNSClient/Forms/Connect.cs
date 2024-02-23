using CustomControls;
using MsmhToolsClass;
using System.Net;

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

                // Create uid
                SecureDNS.GenerateUid(this);

                // Update NICs
                await SetDnsOnNic_.UpdateNICs(CustomComboBoxNICs, GetBootstrapSetting(out int port), port);

                Task taskConnect = Task.Run(async () =>
                {
                    if (connectMode != ConnectMode.Unknown)
                        isConnectSuccess = await Connect(connectMode, limitLog);
                });

                await taskConnect.ContinueWith(async _ =>
                {
                    IsConnecting = false;
                    await UpdateStatusShortOnBoolsChanged();

                    string msg = $"{NL}Connect Task: {taskConnect.Status}{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));

                    if (taskConnect.Status == TaskStatus.Faulted)
                    {
                        if (connectMode == ConnectMode.ConnectToWorkingServers)
                        {
                            string faulted = $"Current DNS Servers are not stable, please check servers.{NL}";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(faulted, Color.IndianRed));
                        }
                    }

                }, TaskScheduler.FromCurrentSynchronizationContext());
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
                        disconnect();
                        await Task.Delay(200);
                        await UpdateBools();
                        if (!IsConnecting && !IsConnected) break;
                    }
                });
                try { await wait1.WaitAsync(TimeSpan.FromSeconds(30)); } catch (Exception) { }

                void disconnect()
                {
                    // Kill processes (DNSProxy, DNSCrypt)
                    ProcessManager.KillProcessByPID(PIDDNSProxy);
                    ProcessManager.KillProcessByPID(PIDDNSProxyBypass);
                    ProcessManager.KillProcessByPID(PIDDNSCrypt);
                    ProcessManager.KillProcessByPID(PIDDNSCryptBypass);

                    // Stop Cloudflare Bypass
                    BypassFakeProxyDohStop(true, true, true, false);
                }

                // To See Status Immediately
                PIDDNSProxy = -1;
                PIDDNSProxyBypass = -1;
                PIDDNSCrypt = -1;
                PIDDNSCryptBypass = -1;
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
                if (!StopQuickConnect) // Make Quick Connect Cancel faster
                    if (reconnect) isConnectSuccess = await StartConnect(connectMode, false, limitLog);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(this, ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        return isConnectSuccess;
    }

    private async Task<bool> Connect(ConnectMode connectMode, bool limitLog = false)
    {
        // Write Connecting message to log
        string msgConnecting = "Connecting... Please wait..." + NL + NL;
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnecting, Color.MediumSeaGreen));

        // Solve: "bind: An attempt was made to access a socket in a way forbidden by its access permissions"
        if (!Program.IsStartup) await NetworkTool.RestartNATDriver();

        // Check Plain DNS port
        bool portDns = GetListeningPort(53, "You need to resolve the conflict.", Color.IndianRed);
        if (!portDns)
        {
            IsConnecting = false;
            return false;
        }

        // Check DoH port if working mode is set to Plain DNS and DoH
        if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
        {
            bool portDoH = GetListeningPort(GetDohPortSetting(), "You need to resolve the conflict.", Color.IndianRed);
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

        // Connect modes
        if (connectMode == ConnectMode.Unknown) return false;
        if (connectMode == ConnectMode.ConnectToWorkingServers)
        {
            //=== Connect DNSProxy (With working servers)
            int countUsingServers = await ConnectToWorkingServersAsync();

            // Wait Until DNS gets Online
            Task wait2 = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsDisconnecting) break;
                    if (IsDNSConnected) break;
                    if (!ProcessManager.FindProcessByPID(PIDDNSProxy)) break;
                    if (!IsInternetOnline) break;
                    await UpdateBoolDnsOnce(GetCheckTimeoutSetting() * 2, GetBlockedDomainSetting(out string _));
                    await Task.Delay(50);
                }
            });
            try { await wait2.WaitAsync(TimeSpan.FromSeconds(30)); } catch (Exception) { }
            countUsingServers = IsDNSConnected ? countUsingServers : -1;

            // Write dnsproxy message to log
            string msgDnsProxy = string.Empty;
            if (countUsingServers != -1)
            {
                if (countUsingServers > 1)
                    msgDnsProxy = "Local DNS Server started using " + countUsingServers + " fastest servers in parallel." + NL;
                else
                    msgDnsProxy = "Local DNS Server started using " + countUsingServers + " server." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDnsProxy, Color.MediumSeaGreen));

                // Connected with DNSProxy
                internalConnect();

                // Write Connected message to log
                connectMessage();

                return true;
            }
            else
            {
                msgDnsProxy = "Error: Couldn't start DNSProxy!";
                if (ProcessManager.FindProcessByPID(PIDDNSProxy) && !IsDNSConnected)
                    msgDnsProxy = "DNS can't get online. Check servers.";
                if (IsDisconnecting) msgDnsProxy = "Task Canceled.";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDnsProxy + NL, Color.IndianRed));
            }
        }
        else if (connectMode == ConnectMode.ConnectToFakeProxyDohViaProxyDPI || connectMode == ConnectMode.ConnectToFakeProxyDohViaGoodbyeDPI)
        {
            //=== Connect To Fake Proxy
            int camouflagePort = -1;
            int fakeProxyPort = GetFakeProxyPortSetting();
            int camouflageDNSPort = GetCamouflageDnsPortSetting();
            camouflagePort = connectMode == ConnectMode.ConnectToFakeProxyDohViaProxyDPI ? fakeProxyPort : camouflageDNSPort;

            bool connected = await BypassFakeProxyDohStart(connectMode, camouflagePort);
            if (connected)
            {
                // Connected with DNSProxy
                internalConnect();

                // Write Connected message to log
                connectMessage();

                return true;
            }
            else
            {
                BypassFakeProxyDohStop(true, true, true, true);
            }
        }
        else if (connectMode == ConnectMode.ConnectToPopularServersWithProxy)
        {
            //=== Connect To Servers Using Proxy
            await ConnectToServersUsingProxy();

            // Wait for DNSCrypt
            Task wait1 = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsDisconnecting) break;
                    if (ProcessManager.FindProcessByPID(PIDDNSCrypt)) break;
                    await Task.Delay(100);
                }
            });
            try { await wait1.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

            if (ProcessManager.FindProcessByPID(PIDDNSCrypt))
            {
                // Connected with DNSCrypt
                internalConnect();

                // Write Connected message to Status
                connectMessage(false);

                return true;
            }
            else
            {
                // Write DNSCryptProxy Error to log
                string msg = "DNSCryptProxy couldn't connect, try again.";
                if (IsDisconnecting) msg = "Task Canceled.";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed));
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

            // Update Local IP
            LocalIP = NetworkTool.GetLocalIPv4();

            // Get Loopback IP
            IPAddress loopbackIP = IPAddress.Loopback;

            // Write local DNS addresses to log
            string msgLocalDNS1 = "Local DNS:";
            if (!limitLog)
                if (writeToLog) this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDNS1, Color.LightGray));
            string msgLocalDNS2 = NL + loopbackIP;
            if (LocalIP != null)
                msgLocalDNS2 += NL + LocalIP.ToString();
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
                    msgLocalDoH2 = $"{NL}https://{loopbackIP}/dns-query";
                else
                    msgLocalDoH2 = $"{NL}https://{loopbackIP}:{ConnectedDohPort}/dns-query";
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

                // Copy Local DoH to Clipboard
                this.InvokeIt(() => Clipboard.SetText(msgLocalDoH2.Trim(NL.ToCharArray())));
            }
        }
    }
}