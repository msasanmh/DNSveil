using MsmhToolsClass;
using System;
using System.Diagnostics;

namespace SecureDNSClient
{
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
            IsQuickConnectWorking = true;

            if (!IsQuickConnecting)
            {
                // Quick Connect

                // Benchmark
                if (!QuickConnectBenchmark.IsRunning) QuickConnectBenchmark.Start();
                QuickConnectBenchmark.Restart();

                await QuickConnect(groupName);
            }
            else
            {
                // Cancel Quick Connect

                // Benchmark
                if (!QuickConnectBenchmark.IsRunning) QuickConnectBenchmark.Start();
                QuickConnectBenchmark.Restart();

                if (StopQuickConnect) return;
                StopQuickConnect = true;
                CustomButtonQuickConnect.Enabled = false;
                CustomButtonQuickConnect.Text = "Stopping QC";

                await QuickDisconnect(true, false, false);

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
                    }
                });
                await wait1.WaitAsync(CancellationToken.None);

                if (!reconnect)
                {
                    await QuickDisconnect(true, true, true);

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
                        }
                    });
                    await wait2.WaitAsync(CancellationToken.None);
                }

                StopQuickConnect = false;
                CustomButtonQuickConnect.Enabled = true;
                CustomButtonQuickConnect.Text = "Quick Connect";

                if (reconnect)
                    await QuickConnect(groupName);
            }

            IsQuickConnectWorking = false;
        }

        private async Task QuickConnect(string? groupName = null)
        {
            if (IsQuickConnecting) return;
            IsQuickConnecting = true;

            if (!QuickConnectTimeout.IsRunning)
            {
                QuickConnectTimeout.Start();
                QuickConnectTimeout.Restart();
            }

            // QuickConnect Button
            CustomButtonQuickConnect.Text = "Stop QC";

            // Cancel
            if (IsExiting || StopQuickConnect || !IsInternetAlive(true))
            {
                endOfQuicConnect();
                return;
            }

            // Cancel
            if (IsCheckingStarted || IsConnecting)
                await QuickDisconnectInternal(true, true, false);

            // Start New Check
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
                    if (WorkingDnsList.Count >= maxServers || !IsCheckingStarted) break;
                    if (QuickConnectTimeout.ElapsedMilliseconds > 60000) break;
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
                    endOfQuicConnect();
                    return;
                }

                StartCheck();

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

            // Connect to working servers
            if (WorkingDnsList.Count > 0)
            {
                // Cancel
                if (IsExiting || StopQuickConnect || !IsInternetAlive(true))
                {
                    endOfQuicConnect();
                    return;
                }

                // Connect to new checked servers
                await StartConnect(ConnectMode.ConnectToWorkingServers, true);

                // Wait for connect
                QuickConnectTimeout.Restart();
                Task wait7 = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsExiting || StopQuickConnect || !IsInternetAlive(false)) break;
                        if (IsConnected || QuickConnectTimeout.ElapsedMilliseconds > 40000) break;
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
                            if (IsExiting || StopQuickConnect || !IsInternetAlive(false)) break;
                            if (IsDNSConnected || QuickConnectTimeout.ElapsedMilliseconds > 60000) break;
                            await Task.Delay(200);
                        }
                    });
                    await wait8.WaitAsync(CancellationToken.None);

                    if (IsDNSConnected)
                    {
                        // Cancel
                        if (IsExiting || StopQuickConnect || !IsInternetAlive(true))
                        {
                            endOfQuicConnect();
                            return;
                        }

                        // Set DNS
                        if (!IsDNSSet && !IsDNSSetting)
                        {
                            SetDNS();

                            // Wait until DNS is set
                            QuickConnectTimeout.Restart();
                            Task wait9 = Task.Run(async () =>
                            {
                                while (true)
                                {
                                    if (IsExiting || StopQuickConnect || !IsInternetAlive(false)) break;
                                    if (IsDNSSet || QuickConnectTimeout.ElapsedMilliseconds > 40000) break;
                                    await Task.Delay(100);
                                }
                            });
                            await wait9.WaitAsync(CancellationToken.None);
                        }

                        // Benchmark
                        string msg = $"Quick Connected";
                        if (!string.IsNullOrEmpty(groupName))
                        {
                            msg += $" to {groupName}";
                            if (groupName.Equals("builtin"))
                                msg = msg.Replace(groupName, "Built-In");
                        }

                        TimeSpan eTime = QuickConnectBenchmark.Elapsed;
                        eTime = TimeSpan.FromMilliseconds(Math.Round(eTime.TotalMilliseconds, 2));
                        string eTimeStr = eTime.Seconds > 9 ? $"{eTime:ss\\.ff}" : $"{eTime:s\\.ff}";

                        msg += $" in {eTimeStr} seconds.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));

                        QuickConnectBenchmark.Stop();
                        QuickConnectBenchmark.Reset();
                    }
                }
            }

            endOfQuicConnect();
            void endOfQuicConnect()
            {
                QuickConnectTimeout.Stop();
                QuickConnectTimeout.Reset();

                // QuickConnect Button
                if (!StopQuickConnect)
                    CustomButtonQuickConnect.Text = "Quick Connect";

                IsQuickConnecting = false;
            }
        }

        private async Task QuickDisconnect(bool stopCheck, bool disconnect, bool unsetDns)
        {
            if (IsQuickDisconnecting) return;
            IsQuickDisconnecting = true;

            await QuickDisconnectInternal(stopCheck, disconnect, unsetDns);

            IsQuickDisconnecting = false;
        }

        private async Task QuickDisconnectInternal(bool stopCheck, bool disconnect, bool unsetDns)
        {
            // Stop Checking
            if (stopCheck && IsCheckingStarted)
            {
                StartCheck();

                // Wait until check stops
                QuickConnectTimeout.Restart();
                Task wait1 = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsExiting) break;
                        if (!IsCheckingStarted || QuickConnectTimeout.ElapsedMilliseconds > 60000) break;
                        await Task.Delay(100);
                    }
                });
                await wait1.WaitAsync(CancellationToken.None);
            }

            // Disconnect
            if (disconnect && (IsConnected || IsConnecting))
            {
                await StartConnect(GetConnectMode());

                // Wait
                QuickConnectTimeout.Restart();
                Task wait2 = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsExiting) break;
                        if (!IsConnected && !IsConnecting && !IsDisconnecting) break;
                        if (QuickConnectTimeout.ElapsedMilliseconds > 60000) break;
                        await Task.Delay(100);
                    }
                });
                await wait2.WaitAsync(CancellationToken.None);
            }

            // Unset DNS
            if (unsetDns && (IsDNSSet || IsDNSSetting))
            {
                SetDNS();

                // Wait
                QuickConnectTimeout.Restart();
                Task wait3 = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsExiting) break;
                        if (!IsDNSSet && !IsDNSSetting && !IsDNSUnsetting) break;
                        if (QuickConnectTimeout.ElapsedMilliseconds > 60000) break;
                        await Task.Delay(100);
                    }
                });
                await wait3.WaitAsync(CancellationToken.None);
            }
        }

    }
}
