using CustomControls;
using MsmhTools;
using System;
using System.Net;

namespace SecureDNSClient
{
    public partial class FormMain
    {
        private async void StartConnect()
        {
            // Return if binary files are missing
            if (!CheckNecessaryFiles()) return;

            // Update Bools
            UpdateBools();

            if (!IsConnected && !IsConnecting)
            {
                try
                {
                    // Connect
                    if (IsConnecting) return;

                    // Check Internet Connectivity
                    if (!IsInternetAlive()) return;

                    // Create uid
                    SecureDNS.GenerateUid(this);

                    // Update NICs
                    SecureDNS.UpdateNICs(CustomComboBoxNICs);

                    Task taskConnect = Task.Run(async () =>
                    {
                        // Stop Check
                        if (IsCheckingStarted)
                        {
                            CustomButtonCheck_Click(null, null);

                            // Wait until check is done
                            while (!IsCheckDone)
                                Task.Delay(100).Wait();
                        }

                        IsConnecting = true;
                        await Connect();
                    });

                    await taskConnect.ContinueWith(_ =>
                    {
                        IsConnecting = false;
                        string msg = $"{NL}Connect Task: {taskConnect.Status}{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));

                        if (taskConnect.Status == TaskStatus.Faulted)
                        {
                            if (GetConnectMode() == ConnectMode.ConnectToWorkingServers)
                            {
                                string faulted = $"Current DNS Servers are not stable, please check servers.{NL}";
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(faulted, Color.IndianRed));
                            }
                        }

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
                    if (IsDisconnecting) return;

                    IsDisconnecting = true;

                    // Write Disconnecting message to log
                    string msgDisconnecting = NL + "Disconnecting..." + NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDisconnecting, Color.MediumSeaGreen));

                    Task taskWait1 = await Task.Run(async () =>
                    {
                        while (IsConnecting || IsConnected)
                        {
                            if (!IsConnecting && !IsConnected)
                                return Task.CompletedTask;
                            disconnect();
                            await Task.Delay(1000);
                        }
                        return Task.CompletedTask;
                    });

                    async void disconnect()
                    {
                        // Unset DNS
                        if (IsDNSSet)
                            await UnsetSavedDNS();

                        // Deactivate DPI
                        DPIDeactive();

                        // Kill processes (DNSProxy, DNSCrypt)
                        ProcessManager.KillProcessByPID(PIDDNSProxy);
                        ProcessManager.KillProcessByPID(PIDDNSProxyBypass);
                        ProcessManager.KillProcessByPID(PIDDNSCrypt);
                        ProcessManager.KillProcessByPID(PIDDNSCryptBypass);

                        // Stop Cloudflare Bypass
                        BypassFakeProxyDohStop(true, true, true, false);

                        //// Stop Fake Proxy
                        //if (FakeProxy != null && FakeProxy.IsRunning)
                        //    FakeProxy.Stop();

                        // Unset Proxy
                        if (IsProxySet && !IsSharing)
                            Network.UnsetProxy(false, false);

                        // Update Groupbox Status
                        UpdateStatusLong();

                        // To see offline status immediately
                        Parallel.Invoke(
                            () => UpdateBoolDnsOnce(1000),
                            () => UpdateBoolDohOnce(1000)
                        );
                    }

                    // Write Disconnected message to log
                    string msgDisconnected = "Disconnected." + NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDisconnected, Color.MediumSeaGreen));

                    IsDisconnecting = false;
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool CheckBypassWorks(int timeoutMS, int attempts, int pid)
        {
            if (!IsConnected || IsDisconnecting) return false;

            // Get loopback
            string loopback = IPAddress.Loopback.ToString();

            // Get blocked domain
            string blockedDomain = GetBlockedDomainSetting(out string blockedDomainNoWww);
            if (string.IsNullOrEmpty(blockedDomain)) return false;

            // Message
            string msg1 = "Bypassing";
            string msg2 = "...";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg1, Color.MediumSeaGreen));

            // New Check
            CheckDns checkDns = new(false, GetCPUPriority());

            for (int n = 0; n < attempts; n++)
            {
                if (!IsConnected || IsDisconnecting) return false;
                if (!ProcessManager.FindProcessByPID(pid)) return false;

                // Message before
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg2, Color.MediumSeaGreen));

                // Delay
                checkDns.CheckDNS(blockedDomainNoWww, loopback, timeoutMS);
                
                Task.Delay(500).Wait(); // Wait a moment
                if (checkDns.IsDnsOnline)
                {
                    // Update bool
                    IsConnected = true;
                    IsDNSConnected = true;

                    // Message add NL on success
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg2 + NL, Color.MediumSeaGreen));

                    // Write delay to log
                    string msgDelay1 = "Server delay: ";
                    string msgDelay2 = $" ms.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDelay1, Color.Orange));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(checkDns.DnsLatency.ToString(), Color.DodgerBlue));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDelay2, Color.Orange));
                    return true;
                }

                Task.Delay(500).Wait();
            }

            // Message add NL on failure
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg2 + NL, Color.MediumSeaGreen));

            return false;
        }

        private async Task Connect()
        {
            // Write Connecting message to log
            string msgConnecting = "Connecting... Please wait..." + NL + NL;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnecting, Color.MediumSeaGreen));

            // Check Plain DNS port
            bool portDns = GetListeningPort(53, "You need to resolve the conflict.", Color.IndianRed);
            if (!portDns)
            {
                IsConnecting = false;
                return;
            }

            // Check DoH port if working mode is set to Plain DNS and DoH
            if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
            {
                bool portDoH = GetListeningPort(GetDohPortSetting(), "You need to resolve the conflict.", Color.IndianRed);
                if (!portDoH)
                {
                    IsConnecting = false;
                    return;
                }
            }
            
            // Flush DNS
            FlushDNS(false);

            // Generate Certificate for DoH
            if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
                GenerateCertificate();

            // Get Connect mode
            ConnectMode connectMode = GetConnectMode();

            // Connect modes
            if (connectMode == ConnectMode.ConnectToWorkingServers)
            {
                //=== Connect DNSProxy (With working servers)
                int countUsingServers = ConnectToWorkingServers();

                // Wait for DNSProxy
                Task wait1 = Task.Run(async () =>
                {
                    while (!ProcessManager.FindProcessByPID(PIDDNSProxy))
                    {
                        if (ProcessManager.FindProcessByPID(PIDDNSProxy))
                            break;
                        await Task.Delay(100);
                    }
                });
                await wait1.WaitAsync(TimeSpan.FromSeconds(5));

                // Wait Until DNS gets Online
                Task wait2 = Task.Run(async () =>
                {
                    while (!IsDNSConnected)
                    {
                        if (IsDNSConnected)
                            break;
                        await Task.Delay(100);
                    }
                });
                await wait2.WaitAsync(TimeSpan.FromSeconds(30));
                
                // Write dnsproxy message to log
                string msgDnsProxy = string.Empty;
                if (ProcessManager.FindProcessByPID(PIDDNSProxy))
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
                }
                else
                {
                    msgDnsProxy = "Error: Couldn't start DNSProxy!" + NL;
                    if (!IsDisconnecting)
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDnsProxy, Color.IndianRed));
                }
            }
            else if (connectMode == ConnectMode.ConnectToFakeProxyDohViaProxyDPI || connectMode == ConnectMode.ConnectToFakeProxyDohViaGoodbyeDPI)
            {
                //=== Connect Cloudflare
                int camouflagePort = -1;
                int fakeProxyPort = GetFakeProxyPortSetting();
                int camouflageDNSPort = GetCamouflageDnsPortSetting();
                camouflagePort = connectMode == ConnectMode.ConnectToFakeProxyDohViaProxyDPI ? fakeProxyPort : camouflageDNSPort;

                bool connected = await BypassFakeProxyDohStart(camouflagePort);
                if (connected)
                {
                    // Write Connected message to log
                    connectMessage();
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
                    while (!ProcessManager.FindProcessByPID(PIDDNSCrypt))
                    {
                        if (ProcessManager.FindProcessByPID(PIDDNSCrypt))
                            break;
                        await Task.Delay(100);
                    }
                });
                await wait1.WaitAsync(TimeSpan.FromSeconds(5));

                if (ProcessManager.FindProcessByPID(PIDDNSCrypt))
                {
                    // Connected with DNSCrypt
                    internalConnect();
                }
                else
                {
                    // Write DNSCryptProxy Error to log
                    string msg = "DNSCryptProxy couldn't connect, try again.";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed));
                }
            }

            void internalConnect()
            {
                // To see online status immediately
                Parallel.Invoke(
                    () => UpdateBoolDnsOnce(10000),
                    () => UpdateBoolDohOnce(10000)
                );

                // Update Groupbox Status
                UpdateStatusLong();

                // Go to DPI Tab if DPI is not already active
                if (ConnectAllClicked && !IsDPIActive)
                {
                    this.InvokeIt(() => CustomTabControlMain.SelectedIndex = 0);
                    this.InvokeIt(() => CustomTabControlSecureDNS.SelectedIndex = 2);
                    this.InvokeIt(() => CustomTabControlDPIBasicAdvanced.SelectedIndex = 2);
                }
            }

            void connectMessage()
            {
                // Update Local IP
                LocalIP = Network.GetLocalIPv4();

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
                    
                }
            }
        }
    }
}
