using CustomControls;
using MsmhToolsClass;
using SecureDNSClient.DPIBasic;
using System.Diagnostics;

namespace SecureDNSClient;

public partial class FormMain
{
    private readonly Stopwatch QuickConnectBenchmark = new();
    private readonly Stopwatch QuickConnectTimeout = new();
    private bool IsQuickConnectWorking = false;
    private bool IsQuickConnecting = false;
    private bool IsQuickDisconnecting = false;
    private bool StopQuickConnect = false;

    private async Task StartQuickConnect(string? groupName = null, bool reconnect = false)
    {
        if (IsInAction(true, false, false, false, false, false, false, false, false, false, true)) return;

        if (!IsQuickConnecting)
        {
            // Quick Connect
            IsQuickConnectWorking = true;

            await QuickConnect(groupName);

            // Benchmark Stop
            QuickConnectBenchmark.Stop();
            IsQuickConnectWorking = false;
        }
        else
        {
            // Cancel Quick Connect
            IsQuickConnectWorking = true;

            // Skip Check
            if (!reconnect && IsCheckingStarted)
            {
                StopChecking = true;
                this.InvokeIt(() => CustomProgressBarCheck.StopTimer = true);
                return;
            }

            if (StopQuickConnect) return;
            StopQuickConnect = true;
            IsDisconnecting = true;

            // Wait for Quick Connect to Cancel
            QuickConnectTimeout.Restart();
            Task wait1 = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsExiting) break;
                    if (!IsQuickConnecting) break;
                    if (QuickConnectTimeout.ElapsedMilliseconds > 60000) break;
                    await Task.Delay(100);
                    IsQuickConnectWorking = true;
                }
            });
            await wait1.WaitAsync(CancellationToken.None);

            IsDisconnecting = false;

            if (!reconnect)
            {
                Debug.WriteLine("It's Not Reconnect 1");
                await DisconnectAll();

                // Wait for full disconnect
                QuickConnectTimeout.Restart();
                Task wait2 = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsExiting) break;
                        if (!IsQuickConnecting && !IsCheckingStarted && !IsConnected && !IsConnecting && !IsDNSSet && !IsDNSSetting) break;
                        if (QuickConnectTimeout.ElapsedMilliseconds > 60000) break;
                        await Task.Delay(100);
                        IsQuickConnectWorking = true;
                    }
                });
                await wait2.WaitAsync(CancellationToken.None);
            }

            StopQuickConnect = false;

            if (!reconnect)
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Quick Connect Stopped.{NL}", Color.MediumSeaGreen));

            if (reconnect)
                await QuickConnect(groupName);

            IsQuickConnectWorking = false;
        }
    }

    private async Task QuickConnect(string? groupName = null)
    {
        if (IsQuickConnecting) return;
        IsQuickConnecting = true;
        StopQuickConnect = false;

        // Delay Between tasks
        int delay = 200;

        // Benchmark Start
        QuickConnectBenchmark.Restart();

        // Get Connect Mode Settings
        ConnectMode connectMode = ConnectMode.ConnectToWorkingServers;
        if (CustomComboBoxSettingQcConnectMode.SelectedItem != null)
            connectMode = GetConnectModeByName(CustomComboBoxSettingQcConnectMode.SelectedItem.ToString());
        if (!string.IsNullOrEmpty(groupName)) connectMode = ConnectMode.ConnectToWorkingServers;
        bool useSavedServers = false;
        this.InvokeIt(() => useSavedServers = CustomCheckBoxSettingQcUseSavedServers.Checked && SavedDnsList.Any() && WorkingDnsList.Any() && groupName == null);
        bool checkAllServers = false;
        this.InvokeIt(() => checkAllServers = CustomCheckBoxSettingQcCheckAllServers.Checked && CustomCheckBoxSettingQcCheckAllServers.Enabled);
        bool setDns = true;
        this.InvokeIt(() => setDns = CustomCheckBoxSettingQcSetDnsTo.Checked);
        string? nicName = string.Empty;
        if (CustomComboBoxSettingQcNics.SelectedItem != null)
            this.InvokeIt(() => nicName = CustomComboBoxSettingQcNics.SelectedItem.ToString());
        bool startProxy = false;
        this.InvokeIt(() => startProxy = CustomCheckBoxSettingQcStartProxyServer.Checked);
        bool setProxy = false;
        this.InvokeIt(() => setProxy = CustomCheckBoxSettingQcSetProxy.Checked);
        setProxy = setProxy && startProxy;
        bool startGoodbyeDpi = false;
        this.InvokeIt(() => startGoodbyeDpi = CustomCheckBoxSettingQcStartGoodbyeDpi.Checked);
        bool startGoodbyeDpiBasic = false;
        this.InvokeIt(() => startGoodbyeDpiBasic = startGoodbyeDpi && CustomRadioButtonSettingQcGdBasic.Checked);
        bool startGoodbyeDpiAdvanced = false;
        this.InvokeIt(() => startGoodbyeDpiAdvanced = startGoodbyeDpi && CustomRadioButtonSettingQcGdAdvanced.Checked);
        DPIBasicBypassMode goodbyeDpiBasicMode = DPIBasicBypassMode.Light;
        if (CustomComboBoxSettingQcGdBasic.SelectedItem != null)
            goodbyeDpiBasicMode = DPIBasicBypass.GetGoodbyeDpiModeBasicByName(CustomComboBoxSettingQcGdBasic.SelectedItem.ToString());

        // Begin
        QuickConnectTimeout.Restart();

        // Cancel
        if (await cancelOnCondition()) return;

        // Disconnect
        await QuickDisconnect(IsCheckingStarted, IsConnecting, !setDns, !startProxy, true, true);

        // Cancel
        if (await cancelOnCondition()) return;

        //== Check Servers
        if (connectMode == ConnectMode.ConnectToWorkingServers && !useSavedServers)
        {
            WorkingDnsList.Clear();
            StartCheck(groupName);

            // Wait until Check starts
            QuickConnectTimeout.Restart();
            Task wait4 = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsExiting || StopQuickConnect || (!Program.Startup && !IsInternetOnline)) break;
                    if (IsCheckingStarted || QuickConnectTimeout.ElapsedMilliseconds > 60000) break;
                    await Task.Delay(100);
                }
            });
            await wait4.WaitAsync(CancellationToken.None);

            // Get number of max servers
            int maxServers = decimal.ToInt32(CustomNumericUpDownSettingMaxServers.Value);

            // Wait until we have enough servers or check is done
            QuickConnectTimeout.Restart();
            Task wait5 = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsExiting || StopQuickConnect || !IsInternetOnline) break;
                    if (!checkAllServers)
                    {
                        if (WorkingDnsList.Count >= maxServers || !IsCheckingStarted) break;
                        if (QuickConnectTimeout.ElapsedMilliseconds > 60000) break;
                    }
                    else
                    {
                        if (!IsCheckingStarted) break;
                    }
                    await Task.Delay(100);
                }
            });
            await wait5.WaitAsync(CancellationToken.None);

            // Stop Checking
            if (IsCheckingStarted)
            {
                StartCheck(null, true);

                // Cancel
                if (await cancelOnCondition()) return;

                // Wait until check is done
                Task wait6 = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsExiting || StopQuickConnect || (!Program.Startup && !IsInternetOnline)) break;
                        if (!IsCheckingStarted) break;
                        await Task.Delay(100);
                    }
                });
                await wait6.WaitAsync(CancellationToken.None);
            }
        }

        //== Connect
        // Cancel
        if (await cancelOnCondition()) return;

        // Return if Connecting to WorkingServers but there is no server
        if (connectMode == ConnectMode.ConnectToWorkingServers && WorkingDnsList.Count < 1)
        {
            endOfQuickConnect();
            return;
        }

        IsConnected = await runStartConnect(); // 1
        await Task.Delay(delay); // 2
        await UpdateBools();
        if (!IsConnected)
        {
            if (await cancelOnCondition()) return;
            await runStartConnect();
            await Task.Delay(delay); // 3
            await UpdateBools();
            if (!IsConnected)
            {
                if (await cancelOnCondition()) return;
                await runStartConnect();
            }
        }

        async Task<bool> runStartConnect()
        {
            // Connect to new checked servers
            bool isConnectSuccess = await StartConnect(connectMode, true);

            // Wait for connect
            QuickConnectTimeout.Restart();
            Task wait7 = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsExiting || StopQuickConnect || (!Program.Startup && !IsInternetOnline)) break;
                    if (!isConnectSuccess) break;
                    if (IsConnected || QuickConnectTimeout.ElapsedMilliseconds > 30000) break;
                    await Task.Delay(100);
                }
            });
            await wait7.WaitAsync(CancellationToken.None);
            return IsConnected;
        }

        if (IsConnected)
        {
            // Wait until DNS gets online
            QuickConnectTimeout.Restart();
            Task wait8 = Task.Run(async () =>
            {
                while (true)
                {
                    if (!IsConnected) break;
                    if (IsExiting || StopQuickConnect || (!Program.Startup && !IsInternetOnline)) break;
                    if (IsDNSConnected || QuickConnectTimeout.ElapsedMilliseconds > 60000) break;
                    await Task.Delay(200);
                }
            });
            await wait8.WaitAsync(CancellationToken.None);

            //== Set Dns
            if (setDns && IsDNSConnected)
            {
                // Cancel
                if (await cancelOnCondition()) return;

                // Check if NIC is Ok
                bool isNicOk1 = IsNicOk(nicName, out _);
                if (!isNicOk1)
                    this.InvokeIt(() => nicName = CustomComboBoxNICs.SelectedItem as string);

                bool isNicOk2 = IsNicOk(nicName, out _);
                if (!isNicOk2)
                {
                    endOfQuickConnect();
                    return;
                }

                // Set DNS
                bool isDnsSetOn = SetDnsOnNic_.IsDnsSet(LastNicName);
                if (!isDnsSetOn && !IsDNSSetting)
                {
                    // Cancel
                    if (await cancelOnCondition()) return;

                    if (!isNicOk1 && isNicOk2)
                    {
                        string msgNicNotAvailable = $"Trying to Set DNS on \"{nicName}\"...{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgNicNotAvailable, Color.Orange));
                    }

                    isDnsSetOn = await runSetDns(); // 1
                    await Task.Delay(delay); // 2
                    isDnsSetOn = SetDnsOnNic_.IsDnsSet(LastNicName);
                    if (!isDnsSetOn)
                    {
                        if (await cancelOnCondition()) return;
                        await runSetDns();
                        await Task.Delay(delay); // 3
                        isDnsSetOn = SetDnsOnNic_.IsDnsSet(LastNicName);
                        if (!isDnsSetOn)
                        {
                            if (await cancelOnCondition()) return;
                            await runSetDns();
                        }
                    }

                    async Task<bool> runSetDns()
                    {
                        await SetDNS(nicName);

                        // Wait until DNS is set
                        QuickConnectTimeout.Restart();
                        Task wait9 = Task.Run(async () =>
                        {
                            while (true)
                            {
                                if (IsExiting || StopQuickConnect || (!Program.Startup && !IsInternetOnline)) break;
                                isDnsSetOn = SetDnsOnNic_.IsDnsSet(LastNicName);
                                if (isDnsSetOn || QuickConnectTimeout.ElapsedMilliseconds > 40000) break;
                                await Task.Delay(100);
                            }
                        });
                        await wait9.WaitAsync(CancellationToken.None);
                        return isDnsSetOn;
                    }
                }
            }

            //== Start Proxy
            if (startProxy && !IsProxyActivated && !IsProxyActivating)
            {
                // If Proxy is Deactivating wait for it
                QuickConnectTimeout.Restart();
                Task waitStopProxy = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsExiting || StopQuickConnect || (!Program.Startup && !IsInternetOnline)) break;
                        if (!IsProxyDeactivating || QuickConnectTimeout.ElapsedMilliseconds > 10000) break;
                        await Task.Delay(100);
                    }
                });
                await waitStopProxy.WaitAsync(CancellationToken.None);

                // Cancel
                if (await cancelOnCondition()) return;

                IsProxyActivated = await runStartProxy(); // 1
                await Task.Delay(delay); // 2
                IsProxyActivated = ProcessManager.FindProcessByPID(PIDProxy);
                if (!IsProxyActivated)
                {
                    if (await cancelOnCondition()) return;
                    await runStartProxy();
                    await Task.Delay(delay); // 3
                    IsProxyActivated = ProcessManager.FindProcessByPID(PIDProxy);
                    if (!IsProxyActivated)
                    {
                        if (await cancelOnCondition()) return;
                        await runStartProxy();
                    }
                }

                async Task<bool> runStartProxy()
                {
                    // Start Proxy
                    await StartProxy();

                    // Wait for Proxy
                    QuickConnectTimeout.Restart();
                    Task waitProxy = Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (IsExiting || StopQuickConnect || (!Program.Startup && !IsInternetOnline)) break;
                            if (IsProxyActivated || QuickConnectTimeout.ElapsedMilliseconds > 30000) break;
                            await Task.Delay(100);
                        }
                    });
                    await waitProxy.WaitAsync(CancellationToken.None);
                    return IsProxyActivated;
                }
            }

            //== Set Proxy
            if (setProxy && !IsProxySet && IsProxyActivated)
            {
                // Cancel
                if (await cancelOnCondition()) return;

                IsProxySet = await runSetProxy(); // 1
                await Task.Delay(delay); // 2
                IsProxySet = UpdateBoolIsProxySet();
                if (!IsProxySet)
                {
                    if (await cancelOnCondition()) return;
                    await runSetProxy();
                    await Task.Delay(delay); // 3
                    IsProxySet = UpdateBoolIsProxySet();
                    if (!IsProxySet)
                    {
                        if (await cancelOnCondition()) return;
                        await runSetProxy();
                    }
                }

                async Task<bool> runSetProxy()
                {
                    SetProxy();

                    // Wait for Proxy to get Set
                    QuickConnectTimeout.Restart();
                    Task waitProxySet = Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (IsExiting || StopQuickConnect || (!Program.Startup && !IsInternetOnline)) break;
                            if (IsProxySet || QuickConnectTimeout.ElapsedMilliseconds > 10000) break;
                            await Task.Delay(100);
                        }
                    });
                    await waitProxySet.WaitAsync(CancellationToken.None);
                    return IsProxySet;
                }
            }

            //== Start GoodbyeDPI
            if (startGoodbyeDpiBasic)
            {
                // Cancel
                if (await cancelOnCondition()) return;

                IsGoodbyeDPIBasicActive = await runGoodbyeDpiBasic(); // 1
                await Task.Delay(delay); // 2
                IsGoodbyeDPIBasicActive = ProcessManager.FindProcessByPID(PIDGoodbyeDPIBasic);
                if (!IsGoodbyeDPIBasicActive)
                {
                    if (await cancelOnCondition()) return;
                    await runGoodbyeDpiBasic();
                    await Task.Delay(delay); // 3
                    IsGoodbyeDPIBasicActive = ProcessManager.FindProcessByPID(PIDGoodbyeDPIBasic);
                    if (!IsGoodbyeDPIBasicActive)
                    {
                        if (await cancelOnCondition()) return;
                        await runGoodbyeDpiBasic();
                    }
                }

                async Task<bool> runGoodbyeDpiBasic()
                {
                    GoodbyeDPIBasic(goodbyeDpiBasicMode);

                    // Wait for GoodbyeDpi Basic
                    QuickConnectTimeout.Restart();
                    Task waitGoodbyeDpiBasic = Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (IsExiting || StopQuickConnect || (!Program.Startup && !IsInternetOnline)) break;
                            if (IsGoodbyeDPIBasicActive || QuickConnectTimeout.ElapsedMilliseconds > 10000) break;
                            await Task.Delay(100);
                        }
                    });
                    await waitGoodbyeDpiBasic.WaitAsync(CancellationToken.None);
                    return IsGoodbyeDPIBasicActive;
                }
            }

            if (startGoodbyeDpiAdvanced)
            {
                // Cancel
                if (await cancelOnCondition()) return;

                IsGoodbyeDPIAdvancedActive = await runGoodbyeDpiAdv(); // 1
                await Task.Delay(delay); // 2
                IsGoodbyeDPIAdvancedActive = ProcessManager.FindProcessByPID(PIDGoodbyeDPIAdvanced);
                if (!IsGoodbyeDPIAdvancedActive)
                {
                    if (await cancelOnCondition()) return;
                    await runGoodbyeDpiAdv();
                    await Task.Delay(delay); // 3
                    IsGoodbyeDPIAdvancedActive = ProcessManager.FindProcessByPID(PIDGoodbyeDPIAdvanced);
                    if (!IsGoodbyeDPIAdvancedActive)
                    {
                        if (await cancelOnCondition()) return;
                        await runGoodbyeDpiAdv();
                    }
                }

                async Task<bool> runGoodbyeDpiAdv()
                {
                    GoodbyeDPIAdvanced();

                    // Wait for GoodbyeDpi Advanced
                    QuickConnectTimeout.Restart();
                    Task waitGoodbyeDpiAdvanced = Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (IsExiting || StopQuickConnect || (!Program.Startup && !IsInternetOnline)) break;
                            if (IsGoodbyeDPIAdvancedActive || QuickConnectTimeout.ElapsedMilliseconds > 10000) break;
                            await Task.Delay(100);
                        }
                    });
                    await waitGoodbyeDpiAdvanced.WaitAsync(CancellationToken.None);
                    return IsGoodbyeDPIAdvancedActive;
                }
            }
        }

        // Benchmark Stop
        string msg = $"Quick Connect finished";
        if (!string.IsNullOrEmpty(groupName))
        {
            msg += $" (using {groupName})";
            if (groupName.Equals("builtin"))
                msg = msg.Replace(groupName, "Built-In");
        }

        TimeSpan eTime = QuickConnectBenchmark.Elapsed;
        string eTimeStr = ConvertTool.TimeSpanToHumanRead(eTime, true);

        msg += $" in {eTimeStr}{NL}";
        if (!StopQuickConnect)
        {
            if (IsDNSConnected &&
                setDns == IsDNSSet &&
                startProxy == IsProxyActivated &&
                setProxy == IsProxySet &&
                startGoodbyeDpiBasic == IsGoodbyeDPIBasicActive &&
                startGoodbyeDpiAdvanced == IsGoodbyeDPIAdvancedActive)
            {
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
            }
        }

        QuickConnectBenchmark.Reset();

        // Reset Quick Connect Timer
        QuickConnectTimeout.Reset();

        async Task<bool> cancelOnCondition()
        {
            // Cancel
            if (IsExiting || StopQuickConnect)
            {
                endOfQuickConnect(); return true;
            }
            if (!IsInternetOnline)
            {
                if (!Program.Startup)
                {
                    endOfQuickConnect(); return true;
                }
                else
                {
                    // There is no Internet but we are on startup
                    // Wait for Internet (1Min)
                    bool go = false;
                    QuickConnectTimeout.Restart();
                    Task waitNet = Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (IsExiting || StopQuickConnect) break;
                            if (QuickConnectTimeout.Elapsed > TimeSpan.FromMinutes(1)) break;
                            if (IsInternetOnline)
                            {
                                go = true;
                                break;
                            }
                            await Task.Delay(100);
                        }
                    });
                    await waitNet.WaitAsync(CancellationToken.None);

                    if (go) return false;
                    else
                    {
                        endOfQuickConnect(); return true;
                    }
                }
            }
            return false;
        }

        // End Of Quick Connect
        endOfQuickConnect();
        async void endOfQuickConnect(bool showWarning = true)
        {
            QuickConnectTimeout.Reset();
            IsQuickConnecting = false;

            if (showWarning && !IsExiting && !StopQuickConnect)
            {
                string msgNotifyUser = string.Empty;
                if (!IsInternetOnline) msgNotifyUser += $"There is no Internet connectivity.{NL}";
                else if (!IsConnected) msgNotifyUser += $"Couldn't Connect.{NL}";
                else if (!IsDNSConnected) msgNotifyUser += $"Couldn't Connect to DNS Server.{NL}";
                else if (setDns != IsDNSSet) msgNotifyUser += $"Couldn't Set DNS.{NL}";
                else if (startProxy != IsProxyActivated) msgNotifyUser += $"Couldn't Start Proxy Server.{NL}";
                else if (setProxy != IsProxySet)
                {
                    SetProxy();
                    await Task.Delay(500);
                    if (setProxy != IsProxySet)
                        msgNotifyUser += $"Couldn't Set Proxy.{NL}";
                }
                else if (startGoodbyeDpiBasic != IsGoodbyeDPIBasicActive) msgNotifyUser += $"Couldn't Activate GoodbyeDPI Basic.{NL}";
                else if (startGoodbyeDpiAdvanced != IsGoodbyeDPIAdvancedActive) msgNotifyUser += $"Couldn't Activate GoodbyeDPI Advanced.{NL}";
                if (!string.IsNullOrEmpty(msgNotifyUser))
                {
                    msgNotifyUser += "Check your Quick Connect Settings.";
                    CustomMessageBox.Show(this, msgNotifyUser, "Quick Connect Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }

    private async Task QuickDisconnect(bool stopCheck, bool disconnect, bool unsetDns, bool stopProxy, bool unsetProxy, bool stopGoodbyeDpi)
    {
        if (IsQuickDisconnecting) return;
        IsQuickDisconnecting = true;

        await QuickDisconnectInternal(stopCheck, disconnect, unsetDns, stopProxy, unsetProxy, stopGoodbyeDpi);

        IsQuickDisconnecting = false;
    }

    private async Task QuickDisconnectInternal(bool stopCheck, bool disconnect, bool unsetDns, bool stopProxy, bool unsetProxy, bool stopGoodbyeDpi)
    {
        // Stop Checking
        if (stopCheck && IsCheckingStarted)
        {
            StartCheck(null, true);

            // Wait until check stops
            QuickConnectTimeout.Restart();
            Task wait = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsExiting) break;
                    if (!IsCheckingStarted || QuickConnectTimeout.ElapsedMilliseconds > 60000) break;
                    await Task.Delay(100);
                }
            });
            await wait.WaitAsync(CancellationToken.None);
        }

        // Disconnect
        if (disconnect && (IsConnected || IsConnecting))
        {
            await StartConnect(ConnectMode.Unknown);

            // Wait
            QuickConnectTimeout.Restart();
            Task wait = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsExiting) break;
                    if (!IsConnected && !IsConnecting) break;
                    if (QuickConnectTimeout.ElapsedMilliseconds > 60000) break;
                    await Task.Delay(100);
                    Debug.WriteLine("HHHHHHHHHHHH");
                }
            });
            await wait.WaitAsync(CancellationToken.None);
        }

        // Unset DNS
        if (unsetDns && (IsDNSSet || IsDNSSetting))
        {
            await UnsetAllDNSs();
            
            // Wait
            QuickConnectTimeout.Restart();
            Task wait = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsExiting) break;
                    if (!IsDNSSet && !IsDNSSetting && !IsDNSUnsetting) break;
                    if (QuickConnectTimeout.ElapsedMilliseconds > 60000) break;
                    await Task.Delay(100);
                }
            });
            await wait.WaitAsync(CancellationToken.None);
        }

        // Stop Proxy
        if (stopProxy && (IsProxyActivated || IsProxyActivating))
        {
            await StartProxy(true);

            // Wait
            QuickConnectTimeout.Restart();
            Task wait = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsExiting) break;
                    if (!IsProxyActivated && !IsProxyActivating && !IsProxyDeactivating) break;
                    if (QuickConnectTimeout.ElapsedMilliseconds > 10000) break;
                    await Task.Delay(100);
                }
            });
            await wait.WaitAsync(CancellationToken.None);
        }

        // Unset Proxy
        if (unsetProxy && IsProxySet)
        {
            SetProxy(true);

            // Wait
            QuickConnectTimeout.Restart();
            Task wait = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsExiting) break;
                    if (!IsProxySet) break;
                    if (QuickConnectTimeout.ElapsedMilliseconds > 10000) break;
                    await Task.Delay(100);
                }
            });
            await wait.WaitAsync(CancellationToken.None);
        }

        // Stop GoodbyeDpi Basic
        if (stopGoodbyeDpi && IsGoodbyeDPIBasicActive)
        {
            GoodbyeDPIDeactive(true, false);

            // Wait
            QuickConnectTimeout.Restart();
            Task wait = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsExiting) break;
                    if (!IsGoodbyeDPIBasicActive) break;
                    if (QuickConnectTimeout.ElapsedMilliseconds > 10000) break;
                    await Task.Delay(100);
                }
            });
            await wait.WaitAsync(CancellationToken.None);
        }

        // Stop GoodbyeDpi Advanced
        if (stopGoodbyeDpi && IsGoodbyeDPIAdvancedActive)
        {
            GoodbyeDPIDeactive(false, true);

            // Wait
            QuickConnectTimeout.Restart();
            Task wait = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsExiting) break;
                    if (!IsGoodbyeDPIAdvancedActive) break;
                    if (QuickConnectTimeout.ElapsedMilliseconds > 10000) break;
                    await Task.Delay(100);
                }
            });
            await wait.WaitAsync(CancellationToken.None);
        }

        QuickConnectTimeout.Stop();
        QuickConnectTimeout.Reset();
    }

}