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

    private async Task StartQuickConnect(QuickConnectRequest qcRequest, bool reconnect = false)
    {
        if (IsInAction(true, false, false, false, false, false, false, false, false, false, true, out _)) return;

        if (!IsQuickConnecting)
        {
            // Quick Connect
            IsQuickConnectWorking = true;

            await QuickConnect(qcRequest);

            // Benchmark Stop
            QuickConnectBenchmark.Stop();
            IsQuickConnectWorking = false;
        }
        else
        {
            IsQuickConnectWorking = true;

            // Skip Check
            if (!reconnect && IsCheckingStarted)
            {
                StopChecking = true;
                this.InvokeIt(() => CustomProgressBarCheck.StopTimer = true);
                return;
            }

            // Stop Quick Connect
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

            if (!reconnect) await DisconnectAll();

            StopQuickConnect = false;

            if (reconnect) await QuickConnect(qcRequest);

            IsQuickConnectWorking = false;
        }
    }

    private async Task QuickConnect(QuickConnectRequest qcRequest)
    {
        if (IsQuickConnecting) return;
        IsQuickConnecting = true;
        StopQuickConnect = false;

        // Delay Between tasks
        int delay = 50;

        // Benchmark Start
        QuickConnectBenchmark.Restart();

        bool useSavedServers = false;
        this.InvokeIt(() => useSavedServers = CustomCheckBoxSettingQcUseSavedServers.Checked && SavedDnsList.Any() && WorkingDnsList.Any() && qcRequest.CanUseSavedServers);
        bool checkAllServers = false;
        this.InvokeIt(() => checkAllServers = CustomCheckBoxSettingQcCheckAllServers.Checked && CustomCheckBoxSettingQcCheckAllServers.Enabled);
        bool setDns = true;
        this.InvokeIt(() => setDns = CustomCheckBoxSettingQcSetDnsTo.Checked);
        List<string> nicNameList = GetNicNameSetting(CustomComboBoxSettingQcNics).NICs;
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

        // ManualDisconnect 1
        if (setDns) await QuickDisconnect(true, false, false, true, true, true);
        else await QuickDisconnect(true, true, true, true, true, true);

        // Cancel
        if (await cancelOnCondition()) return;

        // === Check Servers
        if (qcRequest.ConnectMode == ConnectMode.ConnectToWorkingServers && !useSavedServers)
        {
            // ManualDisconnect 2
            if ((IsDNSSet && !IsDNSConnected) || (!IsDNSSet && IsDNSConnected))
            {
                if (setDns) await QuickDisconnect(true, true, true, true, true, true);
            }
            
            WorkingDnsList.Clear(); // Clear Working Servers If User Is Not Using Saved Servers
            StartCheck(qcRequest.CheckRequest, false, false);

            // Wait until Check starts
            QuickConnectTimeout.Restart();
            Task wait4 = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsExiting || StopQuickConnect || (!Program.IsStartup && !IsInternetOnline)) break;
                    if (IsCheckingStarted || QuickConnectTimeout.ElapsedMilliseconds > 60000) break;
                    await Task.Delay(100);
                }
            });
            await wait4.WaitAsync(CancellationToken.None);

            // Get number of max servers
            int maxServers = GetMaxServersToConnectSetting();

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
                StartCheck(qcRequest.CheckRequest, true, false);

                // Cancel
                if (await cancelOnCondition()) return;

                // Wait until check is done
                Task wait6 = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsExiting || StopQuickConnect || (!Program.IsStartup && !IsInternetOnline)) break;
                        if (!IsCheckingStarted) break;
                        await Task.Delay(100);
                    }
                });
                await wait6.WaitAsync(CancellationToken.None);
            }
        }

        // ManualDisconnect 3
        if (setDns) await QuickDisconnect(true, true, !setDns, true, true, true);

        // === Connect
        // Cancel
        if (await cancelOnCondition()) return;

        // Return if Connecting to WorkingServers but there is no server
        bool canConnect = true;
        if (qcRequest.ConnectMode == ConnectMode.ConnectToWorkingServers && WorkingDnsList.Count < 1)
        {
            canConnect = false;
        }

        if (canConnect)
        {
            IsConnected = await runStartConnect(); // 1
            await Task.Delay(delay); // 2
            await UpdateBoolsAsync();
            if (!IsConnected)
            {
                if (await cancelOnCondition()) return;
                await runStartConnect();
                await Task.Delay(delay); // 3
                await UpdateBoolsAsync();
                if (!IsConnected)
                {
                    if (await cancelOnCondition()) return;
                    await runStartConnect();
                }
            }

            async Task<bool> runStartConnect()
            {
                // Connect to new checked servers
                bool isConnectSuccess = await StartConnect(qcRequest.ConnectMode, false, false);

                // Wait for connect
                QuickConnectTimeout.Restart();
                Task wait7 = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsExiting || StopQuickConnect || (!Program.IsStartup && !IsInternetOnline)) break;
                        if (!isConnectSuccess) break;
                        await UpdateBoolsAsync();
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
                        if (IsExiting || StopQuickConnect || (!Program.IsStartup && !IsInternetOnline)) break;
                        if (IsDNSConnected || QuickConnectTimeout.ElapsedMilliseconds > 60000) break;
                        await Task.Delay(100);
                    }
                });
                await wait8.WaitAsync(CancellationToken.None);

                // === Set Dns
                if (setDns && IsDNSConnected)
                {
                    // Cancel
                    if (await cancelOnCondition()) return;

                    // Check if NIC is Ok
                    bool canSetDns = true;

                    int count1 = 0;
                    foreach (string nicName in nicNameList)
                    {
                        bool isNicOk = IsNicOk(nicName, out _);
                        if (isNicOk) count1++;
                    }
                    bool isNicOk1 = nicNameList.Any() && count1 == nicNameList.Count;

                    if (!isNicOk1)
                    {
                        // MSG
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Trying \"Set DNS\" Tab Adapters...{NL}", Color.DarkOrange));

                        nicNameList = GetNicNameSetting(CustomComboBoxNICs).NICs;

                        int count2 = 0;
                        foreach (string nicName in nicNameList)
                        {
                            bool isNicOk = IsNicOk(nicName, out _);
                            if (isNicOk) count2++;
                        }
                        bool isNicOk2 = nicNameList.Any() && count2 == nicNameList.Count;

                        if (!isNicOk2)
                        {
                            // MSG
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Finding Available Adapters...{NL}", Color.DarkOrange));

                            // Update NICs
                            nicNameList = await SetDnsOnNic_.UpdateNICs(CustomComboBoxNICs, GetBootstrapSetting(out int port), port, false, true);

                            int count3 = 0;
                            foreach (string nicName in nicNameList)
                            {
                                bool isNicOk = IsNicOk(nicName, out _);
                                if (isNicOk) count3++;
                            }
                            bool isNicOk3 = nicNameList.Any() && count3 == nicNameList.Count;

                            if (!isNicOk3)
                            {
                                canSetDns = false;
                            }
                        }
                    }

                    if (canSetDns)
                    {
                        // Set DNS
                        bool isDnsSetOn = SetDnsOnNic_.IsDnsSet(nicNameList);
                        if (!isDnsSetOn && !IsDNSSetting)
                        {
                            // Cancel
                            if (await cancelOnCondition()) return;

                            isDnsSetOn = await runSetDns(); // 1
                            await Task.Delay(delay); // 2
                            isDnsSetOn = SetDnsOnNic_.IsDnsSet(nicNameList);
                            if (!isDnsSetOn)
                            {
                                if (await cancelOnCondition()) return;
                                await runSetDns();
                                await Task.Delay(delay); // 3
                                isDnsSetOn = SetDnsOnNic_.IsDnsSet(nicNameList);
                                if (!isDnsSetOn)
                                {
                                    if (await cancelOnCondition()) return;
                                    await runSetDns();
                                }
                            }

                            async Task<bool> runSetDns()
                            {
                                await SetDNS(nicNameList);

                                // Wait until DNS is set
                                QuickConnectTimeout.Restart();
                                Task wait9 = Task.Run(async () =>
                                {
                                    while (true)
                                    {
                                        if (IsExiting || StopQuickConnect || (!Program.IsStartup && !IsInternetOnline)) break;
                                        isDnsSetOn = SetDnsOnNic_.IsDnsSet(nicNameList);
                                        if (isDnsSetOn || QuickConnectTimeout.ElapsedMilliseconds > 60000) break;
                                        await Task.Delay(100);
                                    }
                                });
                                await wait9.WaitAsync(CancellationToken.None);
                                return isDnsSetOn;
                            }
                        }
                        else
                        {
                            // MSG
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"DNS is already set.{NL}", Color.MediumSeaGreen));
                        }
                    }
                }
            }
        }

        // === Start Proxy
        if (startProxy && !IsProxyActivated && !IsProxyActivating)
        {
            // If Proxy is Deactivating wait for it
            QuickConnectTimeout.Restart();
            Task waitStopProxy = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsExiting || StopQuickConnect || (!Program.IsStartup && !IsInternetOnline)) break;
                    if (!IsProxyDeactivating || QuickConnectTimeout.ElapsedMilliseconds > 10000) break;
                    await Task.Delay(100);
                }
            });
            await waitStopProxy.WaitAsync(CancellationToken.None);

            // Cancel
            if (await cancelOnCondition()) return;

            IsProxyActivated = await runStartProxy(); // 1
            await Task.Delay(delay); // 2
            IsProxyActivated = ProcessManager.FindProcessByPID(PIDProxyServer);
            if (!IsProxyActivated)
            {
                if (await cancelOnCondition()) return;
                await runStartProxy();
                await Task.Delay(delay); // 3
                IsProxyActivated = ProcessManager.FindProcessByPID(PIDProxyServer);
                if (!IsProxyActivated)
                {
                    if (await cancelOnCondition()) return;
                    await runStartProxy();
                }
            }

            async Task<bool> runStartProxy()
            {
                // Start Proxy
                await StartProxy(false, false);

                // Wait for Proxy
                QuickConnectTimeout.Restart();
                Task waitProxy = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsExiting || StopQuickConnect || (!Program.IsStartup && !IsInternetOnline)) break;
                        IsProxyActivated = ProcessManager.FindProcessByPID(PIDProxyServer);
                        if (IsProxyActivated || QuickConnectTimeout.ElapsedMilliseconds > 30000) break;
                        await Task.Delay(100);
                    }
                });
                await waitProxy.WaitAsync(CancellationToken.None);
                return IsProxyActivated;
            }
        }

        // === Set Proxy
        if (setProxy && !IsProxySet && IsProxyActivated)
        {
            // Cancel
            if (await cancelOnCondition()) return;

            IsProxySet = await runSetProxy(); // 1
            await Task.Delay(delay); // 2
            IsProxySet = UpdateBoolIsProxySet(out bool isAnotherProxySet, out string currentSystemProxy);
            IsAnotherProxySet = isAnotherProxySet;
            CurrentSystemProxy = currentSystemProxy;
            if (!IsProxySet)
            {
                if (await cancelOnCondition()) return;
                await runSetProxy();
                await Task.Delay(delay); // 3
                IsProxySet = UpdateBoolIsProxySet(out isAnotherProxySet, out currentSystemProxy);
                IsAnotherProxySet = isAnotherProxySet;
                CurrentSystemProxy = currentSystemProxy;
                if (!IsProxySet)
                {
                    if (await cancelOnCondition()) return;
                    await runSetProxy();
                }
            }

            async Task<bool> runSetProxy()
            {
                await SetProxyAsync(false, false);

                // Wait for Proxy to get Set
                QuickConnectTimeout.Restart();
                Task waitProxySet = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsExiting || StopQuickConnect || (!Program.IsStartup && !IsInternetOnline)) break;
                        IsProxySet = UpdateBoolIsProxySet(out isAnotherProxySet, out currentSystemProxy);
                        IsAnotherProxySet = isAnotherProxySet;
                        CurrentSystemProxy = currentSystemProxy;
                        if (IsProxySet || QuickConnectTimeout.ElapsedMilliseconds > 10000) break;
                        await Task.Delay(100);
                    }
                });
                await waitProxySet.WaitAsync(CancellationToken.None);
                return IsProxySet;
            }
        }

        // === Start GoodbyeDpi Basic
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
                GoodbyeDPIBasic(goodbyeDpiBasicMode, false);

                // Wait for GoodbyeDpi Basic
                QuickConnectTimeout.Restart();
                Task waitGoodbyeDpiBasic = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsExiting || StopQuickConnect || (!Program.IsStartup && !IsInternetOnline)) break;
                        IsGoodbyeDPIBasicActive = ProcessManager.FindProcessByPID(PIDGoodbyeDPIBasic);
                        if (IsGoodbyeDPIBasicActive || QuickConnectTimeout.ElapsedMilliseconds > 10000) break;
                        await Task.Delay(100);
                    }
                });
                await waitGoodbyeDpiBasic.WaitAsync(CancellationToken.None);
                return IsGoodbyeDPIBasicActive;
            }
        }

        // === Start GoodbyeDpi Advanced
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
                GoodbyeDPIAdvanced(false);

                // Wait for GoodbyeDpi Advanced
                QuickConnectTimeout.Restart();
                Task waitGoodbyeDpiAdvanced = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsExiting || StopQuickConnect || (!Program.IsStartup && !IsInternetOnline)) break;
                        IsGoodbyeDPIAdvancedActive = ProcessManager.FindProcessByPID(PIDGoodbyeDPIAdvanced);
                        if (IsGoodbyeDPIAdvancedActive || QuickConnectTimeout.ElapsedMilliseconds > 10000) break;
                        await Task.Delay(100);
                    }
                });
                await waitGoodbyeDpiAdvanced.WaitAsync(CancellationToken.None);
                return IsGoodbyeDPIAdvancedActive;
            }
        }

        // Benchmark Stop
        string msg = $"Quick Connect finished";
        if (qcRequest.ConnectMode == ConnectMode.ConnectToWorkingServers)
        {
            if (useSavedServers && CurrentUsingCustomServersList.Any())
                msg += $" (using \"{CurrentUsingCustomServersList[0].GroupName}\")";
            else
                msg += $" (using \"{qcRequest.CheckRequest.GroupName}\")";
        }
        else msg += $" (using \"{GetConnectModeNameByConnectMode(qcRequest.ConnectMode)}\")";

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
            if (IsExiting || StopQuickConnect || IsDisconnectingAll)
            {
                endOfQuickConnect(); // Only In Series
                return true;
            }
            if (!IsInternetOnline)
            {
                if (!Program.IsStartup)
                {
                    endOfQuickConnect(); // Only In Series
                    return true;
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
                        endOfQuickConnect(); // Only In Series
                        return true;
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
            
            if (showWarning && !IsExiting && !StopQuickConnect)
            {
                // Update Bools
                await UpdateBoolsAsync();
                await UpdateBoolProxyAsync();
                await UpdateStatusLongAsync();

                string msgNotifyUser = string.Empty;
                if (!IsInternetOnline) msgNotifyUser += $"There is no Internet connectivity.{NL}";
                else if (!IsConnected) msgNotifyUser += $"Couldn't Connect.{NL}";
                else if (!IsDNSConnected) msgNotifyUser += $"Couldn't Connect to DNS Server.{NL}";
                else if (setDns != IsDNSSet) msgNotifyUser += $"Couldn't Set DNS.{NL}";
                else if (startProxy != IsProxyActivated) msgNotifyUser += $"Couldn't Start Proxy Server.{NL}";
                else if (setProxy != IsProxySet)
                {
                    await SetProxyAsync();
                    await Task.Delay(500);
                    // Update bool IsProxySet
                    IsProxySet = UpdateBoolIsProxySet(out bool isAnotherProxySet, out string currentSystemProxy);
                    IsAnotherProxySet = isAnotherProxySet;
                    CurrentSystemProxy = currentSystemProxy;
                    if (setProxy != IsProxySet)
                        msgNotifyUser += $"Couldn't Set Proxy.{NL}";
                }
                else if (startGoodbyeDpiBasic != IsGoodbyeDPIBasicActive) msgNotifyUser += $"Couldn't Activate GoodbyeDPI Basic.{NL}";
                else if (startGoodbyeDpiAdvanced != IsGoodbyeDPIAdvancedActive) msgNotifyUser += $"Couldn't Activate GoodbyeDPI Advanced.{NL}";
                if (!string.IsNullOrEmpty(msgNotifyUser) && !IsDisconnectingAll)
                {
                    msgNotifyUser += "Check your Quick Connect Settings.";
                    CustomMessageBox.Show(this, msgNotifyUser, "Quick Connect Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            IsQuickConnecting = false;
            await UpdateStatusShortOnBoolsChangedAsync();
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
            StartCheck(new CheckRequest(), true);

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

        // ManualDisconnect
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
            await SetProxyAsync(true);

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