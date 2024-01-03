using CustomControls;
using MsmhToolsClass;
using MsmhToolsWinFormsClass;
using System.Net;

namespace SecureDNSClient;

public partial class FormMain
{
    private async Task<bool> StartConnect(ConnectMode connectMode, bool reconnect = false)
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
                SecureDNS.UpdateNICs(CustomComboBoxNICs, out _);

                Task taskConnect = Task.Run(async () =>
                {
                    // Stop Check
                    if (IsCheckingStarted)
                    {
                        StartCheck(null, true);

                        // Wait until check is done
                        while (IsCheckingStarted)
                            Task.Delay(100).Wait();
                    }

                    if (connectMode != ConnectMode.Unknown)
                        isConnectSuccess = await Connect(connectMode);
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
                        if (!IsConnecting && !IsConnected) break;
                        disconnect();
                        await Task.Delay(500);
                    }
                });
                try { await wait1.WaitAsync(TimeSpan.FromSeconds(30)); } catch (Exception) { }

                async void disconnect()
                {
                    // Kill processes (DNSProxy, DNSCrypt)
                    ProcessManager.KillProcessByPID(PIDDNSProxy);
                    ProcessManager.KillProcessByPID(PIDDNSProxyBypass);
                    ProcessManager.KillProcessByPID(PIDDNSCrypt);
                    ProcessManager.KillProcessByPID(PIDDNSCryptBypass);

                    // Stop Cloudflare Bypass
                    BypassFakeProxyDohStop(true, true, true, false);

                    // Unset Proxy if Proxy is Not Running
                    if (IsProxySet && !IsProxyRunning)
                        NetworkTool.UnsetProxy(false, true);

                    // Unset DNS
                    if (IsDNSSet && !reconnect)
                        await UnsetAllDNSs();

                    // Update Bools
                    await UpdateBools();
                }

                // To See Status Immediately
                LocalDnsLatency = -1;
                IsDNSConnected = LocalDnsLatency != -1;
                LocalDohLatency = -1;
                IsDoHConnected = LocalDohLatency != -1;

                // Update Groupbox Status
                await UpdateStatusLong();

                // Write Disconnected message to log
                string msgDisconnected = "Disconnected." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDisconnected, Color.MediumSeaGreen));

                // Remove Connect ToolTip
                this.InvokeIt(() => CustomButtonConnect.SetToolTip(MainToolTip, string.Empty, string.Empty));

                PIDDNSProxy = -1;
                PIDDNSProxyBypass = -1;
                PIDDNSCrypt = -1;
                PIDDNSCryptBypass = -1;

                IsDisconnecting = false;
                await UpdateStatusShortOnBoolsChanged();

                // Reconnect
                if (!StopQuickConnect) // Make Quick Connect Cancel faster
                    if (reconnect) isConnectSuccess = await StartConnect(connectMode);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(this, ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        return isConnectSuccess;
    }

    private async Task<bool> Connect(ConnectMode connectMode)
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

                // Copy Local DoH to Clipboard
                string localDoH;
                if (ConnectedDohPort == 443)
                    localDoH = $"https://{IPAddress.Loopback}/dns-query";
                else
                    localDoH = $"https://{IPAddress.Loopback}:{ConnectedDohPort}/dns-query";
                this.InvokeIt(() => Clipboard.SetText(localDoH));

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

        void connectMessage()
        {
            // Set LastConnectMode
            LastConnectMode = connectMode;

            // Update Current Using Custom Servers Tooltip
            if (LastConnectMode == ConnectMode.ConnectToWorkingServers && !IsBuiltinMode)
            {
                string tooltip = string.Empty;
                for (int n = 0; n < CurrentUsingCustomServersList.Count; n++)
                    tooltip += CurrentUsingCustomServersList[n] + NL;
                if (tooltip.EndsWith(tooltip)) tooltip = tooltip.TrimEnd(NL.ToCharArray());
                this.InvokeIt(() => CustomButtonConnect.SetToolTip(MainToolTip, "Using Servers", tooltip));
            }
            else
                this.InvokeIt(() => CustomButtonConnect.SetToolTip(MainToolTip, string.Empty, string.Empty));

            // Update Local IP
            LocalIP = NetworkTool.GetLocalIPv4();

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

                string msgLocalDoH2;
                if (ConnectedDohPort == 443)
                    msgLocalDoH2 = $"{NL}https://{loopbackIP}/dns-query";
                else
                    msgLocalDoH2 = $"{NL}https://{loopbackIP}:{ConnectedDohPort}/dns-query";
                msgLocalDoH2 += NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDoH2, Color.DodgerBlue));

                if (LocalIP != null)
                {
                    string msgLocalDoH3;
                    if (ConnectedDohPort == 443)
                        msgLocalDoH3 = $"https://{LocalIP}/dns-query";
                    else
                        msgLocalDoH3 = $"https://{LocalIP}:{ConnectedDohPort}/dns-query";
                    msgLocalDoH3 += NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDoH3, Color.DodgerBlue));
                }

                // Copy Local DoH to Clipboard
                this.InvokeIt(() => Clipboard.SetText(msgLocalDoH2.Trim(NL.ToCharArray())));
            }
        }
    }
}