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

        // Benchmark Start
        QuickConnectBenchmark.Restart();

        // Get Connect Mode Settings
        ConnectMode connectMode = GetConnectModeByName(CustomComboBoxSettingQcConnectMode.SelectedItem.ToString());
        if (!string.IsNullOrEmpty(groupName)) connectMode = ConnectMode.ConnectToWorkingServers;
        bool useSavedServers = false;
        this.InvokeIt(() => useSavedServers = CustomCheckBoxSettingQcUseSavedServers.Checked && SavedDnsList.Any() && WorkingDnsList.Any() && groupName == null);
        bool checkAllServers = false;
        this.InvokeIt(() => checkAllServers = CustomCheckBoxSettingQcCheckAllServers.Checked && CustomCheckBoxSettingQcCheckAllServers.Enabled);
        bool setDns = true;
        this.InvokeIt(() => setDns = CustomCheckBoxSettingQcSetDnsTo.Checked);
        string? nicName = string.Empty;
        this.InvokeIt(() => nicName = CustomComboBoxSettingQcNics.SelectedItem.ToString());
        bool startProxy = false;
        this.InvokeIt(() => startProxy = CustomCheckBoxSettingQcStartProxyServer.Checked);
        bool setProxy = false;
        this.InvokeIt(() => setProxy = CustomCheckBoxSettingQcSetProxy.Checked);
        bool startGoodbyeDpi = false;
        this.InvokeIt(() => startGoodbyeDpi = CustomCheckBoxSettingQcStartGoodbyeDpi.Checked);
        bool startGoodbyeDpiBasic = false;
        this.InvokeIt(() => startGoodbyeDpiBasic = startGoodbyeDpi && CustomRadioButtonSettingQcGdBasic.Checked);
        bool startGoodbyeDpiAdvanced = false;
        this.InvokeIt(() => startGoodbyeDpiAdvanced = startGoodbyeDpi && CustomRadioButtonSettingQcGdAdvanced.Checked);
        DPIBasicBypassMode goodbyeDpiBasicMode = DPIBasicBypass.GetGoodbyeDpiModeBasicByName(CustomComboBoxSettingQcGdBasic.SelectedItem.ToString());

        // Begin
        QuickConnectTimeout.Restart();

        // Cancel
        if (IsExiting || StopQuickConnect || !IsInternetAlive(true))
        {
            endOfQuickConnect();
            return;
        }

        // Cancel
        bool stopP = !startProxy || (startProxy && !setProxy);
        bool stopGd = (!startGoodbyeDpiBasic && IsGoodbyeDPIBasicActive) ||
                      (!startGoodbyeDpiAdvanced && IsGoodbyeDPIAdvancedActive);
        await QuickDisconnectInternal(IsCheckingStarted, IsConnecting, !setDns, !startProxy, stopP, stopGd);

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
                    if (IsExiting || StopQuickConnect || !IsInternetAlive(false)) break;
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
                    if (IsExiting || StopQuickConnect || !IsInternetAlive(false)) break;
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
                // Cancel
                if (IsExiting || StopQuickConnect || !IsInternetAlive(true))
                {
                    endOfQuickConnect();
                    return;
                }

                StartCheck(null, true);

                // Wait until check is done
                Task wait6 = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsExiting || StopQuickConnect || !IsInternetAlive(false)) break;
                        if (!IsCheckingStarted) break;
                        await Task.Delay(100);
                    }
                });
                await wait6.WaitAsync(CancellationToken.None);
            }
        }

        //== Connect
        // Cancel
        if (IsExiting || StopQuickConnect || !IsInternetAlive(true))
        {
            endOfQuickConnect();
            return;
        }

        // Return if Connecting to WorkingServers but there is no server
        if (connectMode == ConnectMode.ConnectToWorkingServers && WorkingDnsList.Count < 1)
        {
            endOfQuickConnect();
            return;
        }

        // Connect to new checked servers
        bool isConnectSuccess = await StartConnect(connectMode, true);

        // Wait for connect
        QuickConnectTimeout.Restart();
        Task wait7 = Task.Run(async () =>
        {
            while (true)
            {
                if (IsExiting || StopQuickConnect || !IsInternetAlive(false)) break;
                if (!isConnectSuccess) break;
                if (IsConnected || QuickConnectTimeout.ElapsedMilliseconds > 30000) break;
                await Task.Delay(100);
            }
        });
        await wait7.WaitAsync(CancellationToken.None);

        if (IsConnected)
        {
            // Wait until DNS gets online
            QuickConnectTimeout.Restart();
            Task wait8 = Task.Run(async () =>
            {
                while (true)
                {
                    if (!IsConnected) break;
                    if (IsExiting || StopQuickConnect || !IsInternetAlive(false)) break;
                    if (IsDNSConnected || QuickConnectTimeout.ElapsedMilliseconds > 60000) break;
                    await Task.Delay(200);
                }
            });
            await wait8.WaitAsync(CancellationToken.None);

            //== Set Dns
            if (setDns && IsDNSConnected)
            {
                // Cancel
                if (IsExiting || StopQuickConnect || !IsInternetAlive(true))
                {
                    endOfQuickConnect();
                    return;
                }

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
                    if (IsExiting || StopQuickConnect || !IsInternetAlive(true))
                    {
                        endOfQuickConnect();
                        return;
                    }

                    if (!isNicOk1 && isNicOk2)
                    {
                        string msgNicNotAvailable = $"Trying to Set DNS on \"{nicName}\"...{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgNicNotAvailable, Color.Orange));
                    }

                    await SetDNS(nicName);

                    // Wait until DNS is set
                    QuickConnectTimeout.Restart();
                    Task wait9 = Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (IsExiting || StopQuickConnect || !IsInternetAlive(false)) break;
                            isDnsSetOn = SetDnsOnNic_.IsDnsSet(LastNicName);
                            if (isDnsSetOn || QuickConnectTimeout.ElapsedMilliseconds > 40000) break;
                            await Task.Delay(100);
                        }
                    });
                    await wait9.WaitAsync(CancellationToken.None);
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
                        if (IsExiting || StopQuickConnect || !IsInternetAlive(false)) break;
                        if (!IsProxyDeactivating || QuickConnectTimeout.ElapsedMilliseconds > 10000) break;
                        await Task.Delay(100);
                    }
                });
                await waitStopProxy.WaitAsync(CancellationToken.None);

                // Cancel
                if (IsExiting || StopQuickConnect || !IsInternetAlive(true))
                {
                    endOfQuickConnect();
                    return;
                }

                // Start Proxy
                await StartProxy();

                // Wait for Proxy
                QuickConnectTimeout.Restart();
                Task waitProxy = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsExiting || StopQuickConnect || !IsInternetAlive(false)) break;
                        if (IsProxyActivated || QuickConnectTimeout.ElapsedMilliseconds > 30000) break;
                        await Task.Delay(100);
                    }
                });
                await waitProxy.WaitAsync(CancellationToken.None);
            }

            //== Set Proxy
            if (setProxy && !IsProxySet && IsProxyActivated)
            {
                // Cancel
                if (IsExiting || StopQuickConnect || !IsInternetAlive(true))
                {
                    endOfQuickConnect();
                    return;
                }

                SetProxy();

                // Wait for Proxy to get Set
                QuickConnectTimeout.Restart();
                Task waitProxySet = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsExiting || StopQuickConnect || !IsInternetAlive(false)) break;
                        if (IsProxySet || QuickConnectTimeout.ElapsedMilliseconds > 10000) break;
                        await Task.Delay(100);
                    }
                });
                await waitProxySet.WaitAsync(CancellationToken.None);
            }

            //== Start GoodbyeDPI
            if (startGoodbyeDpiBasic)
            {
                // Cancel
                if (IsExiting || StopQuickConnect || !IsInternetAlive(true))
                {
                    endOfQuickConnect();
                    return;
                }

                GoodbyeDPIBasic(goodbyeDpiBasicMode);

                // Wait for GoodbyeDpi Basic
                QuickConnectTimeout.Restart();
                Task waitGoodbyeDpiBasic = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsExiting || StopQuickConnect || !IsInternetAlive(false)) break;
                        if (IsGoodbyeDPIBasicActive || QuickConnectTimeout.ElapsedMilliseconds > 10000) break;
                        await Task.Delay(100);
                    }
                });
                await waitGoodbyeDpiBasic.WaitAsync(CancellationToken.None);
            }

            if (startGoodbyeDpiAdvanced)
            {
                // Cancel
                if (IsExiting || StopQuickConnect || !IsInternetAlive(true))
                {
                    endOfQuickConnect();
                    return;
                }

                GoodbyeDPIAdvanced();

                // Wait for GoodbyeDpi Advanced
                QuickConnectTimeout.Restart();
                Task waitGoodbyeDpiAdvanced = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsExiting || StopQuickConnect || !IsInternetAlive(false)) break;
                        if (IsGoodbyeDPIAdvancedActive || QuickConnectTimeout.ElapsedMilliseconds > 10000) break;
                        await Task.Delay(100);
                    }
                });
                await waitGoodbyeDpiAdvanced.WaitAsync(CancellationToken.None);
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

        // End Of Quick Connect
        endOfQuickConnect(true);
        void endOfQuickConnect(bool showWarning = true)
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
                else if (setProxy != IsProxySet) msgNotifyUser += $"Couldn't Set Proxy.{NL}";
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