using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Diagnostics;

namespace SecureDNSClient;

public partial class FormMain
{
    private async Task WriteSavedServersDelayToLogAsync()
    {
        try
        {
            List<string> savedDnsList = SavedDnsList.ToList();
            if (savedDnsList.Any() && !IsCheckingStarted && !IsConnecting && !IsDisconnecting)
            {
                if (!IsInternetOnline) return;

                if (savedDnsList.Count > 0)
                {
                    // Get blocked domain
                    string blockedDomain = GetBlockedDomainSetting(out string blockedDomainNoWww);
                    if (string.IsNullOrEmpty(blockedDomain)) return;

                    // Get Check timeout value
                    decimal timeoutSec = 1;
                    this.InvokeIt(() => timeoutSec = CustomNumericUpDownSettingCheckTimeout.Value);
                    int timeoutMS = decimal.ToInt32(timeoutSec * 1000);
                    if (timeoutMS < 500) timeoutMS = 1000;

                    // Add start msg
                    string plural = savedDnsList.Count > 1 ? "s" : string.Empty;
                    string msgStart = $"{NL}Contains {savedDnsList.Count} Server{plural}:{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgStart, Color.LightGray));

                    // Get insecure state
                    bool insecure = CustomCheckBoxInsecure.Checked;

                    await Task.Run(async () =>
                    {
                        var lists = savedDnsList.SplitToLists(3);
                        int nt = 1;

                        for (int n = 0; n < lists.Count; n++)
                        {
                            if (IsCheckingStarted || IsConnecting || IsDisconnecting) break;

                            List<string> list = lists[n];
                            var parallelLoopResult = Parallel.For(0, list.Count, async i =>
                            {
                                await getDelay(i, nt++);
                            });

                            await Task.Run(async () =>
                            {
                                while (!parallelLoopResult.IsCompleted)
                                {
                                    if (parallelLoopResult.IsCompleted) return Task.CompletedTask;
                                    await Task.Delay(500);
                                }
                                return Task.CompletedTask;
                            });
                        }
                    });

                    async Task getDelay(int n, int nt)
                    {
                        string dns = savedDnsList[n];

                        // Get Status and Latency
                        CheckDns checkDns = new(insecure, false);
                        CheckDns.CheckDnsResult cdr = await checkDns.CheckDnsAsync(blockedDomainNoWww, dns, timeoutMS);

                        string msg = $"DNS {nt}: {cdr.DnsLatency} ms.{NL}";
                        Color color = (cdr.DnsLatency == -1) ? Color.IndianRed : Color.MediumSeaGreen;
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, color));
                        await Task.Delay(10);
                    }
                }
            }
            else
            {
                string msg = $"There Is No Saved Server.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SavedDnsServers WriteSavedServersDelayToLogAsync: " + ex.Message);
        }
    }

    private async void SavedDnsLoad()
    {
        await Task.Run(async () =>
        {
            try
            {
                FileDirectory.CreateEmptyFile(SecureDNS.SavedEncodedDnsPath);

                List<string> savedEncodedDnsList = new();
                await savedEncodedDnsList.LoadFromFileAsync(SecureDNS.SavedEncodedDnsPath, true, true);
                savedEncodedDnsList = savedEncodedDnsList.Distinct().ToList();

                if (savedEncodedDnsList.Any())
                {
                    List<string> dnss = await BuiltInDecryptAsync(savedEncodedDnsList);
                    for (int n = 0; n < dnss.Count; n++)
                    {
                        string dns = dnss[n];
                        SavedDnsList.Add(dns);
                        WorkingDnsList.Add(new DnsInfo { DNS = dns, Latency = 0, CheckMode = CheckMode.SavedServers });
                    }

                    // Update Status Long
                    await UpdateStatusLongAsync();

                    // DeDup
                    SavedDnsList = SavedDnsList.Distinct().ToList();
                }

                SavedDnsUpdateAuto();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SavedDnsServers SavedDnsLoad: " + ex.Message);
            }
        });
    }

    private async void SavedDnsUpdateAuto()
    {
        try
        {
            await Task.Delay(2000);
            await SavedDnsUpdateAsync();
            System.Timers.Timer savedDnsUpdateTimer = new();
            savedDnsUpdateTimer.Interval = TimeSpan.FromMinutes(60).TotalMilliseconds;
            savedDnsUpdateTimer.Elapsed += async (s, e) =>
            {
                await SavedDnsUpdateAsync();
            };
            savedDnsUpdateTimer.Start();
        }
        catch (Exception) { }
    }

    private bool UpdateWorkingServers => !(IsCheckedCustomServers && WorkingDnsList.Count > 0);

    private async Task SavedDnsUpdateAsync()
    {
        try
        {
            if (Program.IsStartup && CustomCheckBoxSettingQcOnStartup.Checked) return;

            // Wait Until App Is Ready
            Task waitNet = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsAppReady) break;
                    await Task.Delay(1000);
                }
            });
            try { await waitNet.WaitAsync(TimeSpan.FromMinutes(1)); } catch (Exception) { }

            // Get blocked domain
            string blockedDomain = GetBlockedDomainSetting(out string blockedDomainNoWww);
            if (string.IsNullOrEmpty(blockedDomain)) return;

            // Get Check timeout value
            decimal timeoutSec = 1;
            this.InvokeIt(() => timeoutSec = CustomNumericUpDownSettingCheckTimeout.Value);
            int timeoutMS = decimal.ToInt32(timeoutSec * 1000);

            // Get insecure state
            bool insecure = CustomCheckBoxInsecure.Checked;

            // Get number of max servers
            int maxServers = decimal.ToInt32(CustomNumericUpDownSettingMaxServers.Value);

            // New Check
            CheckDns checkDns = new(insecure, false);

            List<string> newSavedDnsList = new();
            List<DnsInfo> newWorkingDnsList = new();

            // Check saved dns servers can work
            if (SavedDnsList.Any())
            {
                if (!IsInternetOnline) return;
                List<string> savedDnsList = SavedDnsList.ToList();

                await Task.Run(async () =>
                {
                    var lists = savedDnsList.SplitToLists(3);

                    for (int n = 0; n < lists.Count; n++)
                    {
                        if (!IsInternetOnline) break;
                        if (IsCheckingStarted) break;

                        List<string> list = lists[n];

                        await Parallel.ForEachAsync(list, async (dns, cancellationToken) =>
                        {
                            CheckDns.CheckDnsResult cdr = await checkDns.CheckDnsAsync(blockedDomainNoWww, dns, timeoutMS + 500);
                            if (cdr.IsDnsOnline)
                            {
                                lock (newSavedDnsList)
                                {
                                    newSavedDnsList.Add(dns);
                                }
                                
                                lock (newWorkingDnsList)
                                {
                                    if (UpdateWorkingServers)
                                        newWorkingDnsList.Add(new DnsInfo { DNS = dns, Latency = cdr.DnsLatency, CheckMode = CheckMode.SavedServers });
                                }
                            }
                        });
                    }
                });
            }

            if (newSavedDnsList.Any())
            {
                SavedDnsList.Clear();
                SavedDnsList = new(newSavedDnsList);

                SavedEncodedDnsList.Clear();
                SavedEncodedDnsList = new(await BuiltInEncryptAsync(newSavedDnsList));
                await SavedEncodedDnsList.SaveToFileAsync(SecureDNS.SavedEncodedDnsPath);

                if (UpdateWorkingServers && !IsCheckingStarted)
                {
                    // Add
                    WorkingDnsList = new(WorkingDnsList.Concat(newWorkingDnsList));

                    // Remove Duplicates
                    WorkingDnsList = new(WorkingDnsList.DistinctBy(x => x.DNS));
                }
            }

            if (newSavedDnsList.Count >= maxServers) return;

            // There is not enough working server lets find some
            // Built-in or Custom
            bool builtInMode = CustomRadioButtonBuiltIn.Checked;

            List<ReadDnsResult> rdrList = new();

            if (builtInMode)
            {
                // Get Built-In Servers
                CheckRequest checkRequest = new() { CheckMode = CheckMode.BuiltIn };
                rdrList = await ReadBuiltInServersAsync(checkRequest, false, true);
                if (insecure)
                {
                    List<ReadDnsResult> rdrListInsecure = await ReadBuiltInServersAsync(checkRequest, true, true);
                    if (rdrListInsecure.Any()) rdrList.AddRange(rdrListInsecure);
                }
            }
            else
            {
                // Get Custom Servers
                if (XmlTool.IsValidXMLFile(SecureDNS.CustomServersXmlPath))
                {
                    rdrList = await ReadCustomServersXml(SecureDNS.CustomServersXmlPath, new CheckRequest { CheckMode = CheckMode.CustomServers });
                }

                // If There is no Custom Servers Get Built-In
                if (!rdrList.Any())
                {
                    // Get Built-In Servers
                    CheckRequest checkRequest = new() { CheckMode = CheckMode.BuiltIn };
                    rdrList = await ReadBuiltInServersAsync(checkRequest, false, true);
                    if (insecure)
                    {
                        List<ReadDnsResult> rdrListInsecure = await ReadBuiltInServersAsync(checkRequest, true, true);
                        if (rdrListInsecure.Any()) rdrList.AddRange(rdrListInsecure);
                    }
                }
            }

            if (!rdrList.Any()) return;

            int currentServers = newSavedDnsList.Count;
            for (int n = 0; n < rdrList.Count; n++)
            {
                if (!IsInternetOnline) break;
                if (IsCheckingStarted) break;

                string dns = rdrList[n].DNS.Trim();
                if (!string.IsNullOrEmpty(dns))
                {
                    if (IsDnsProtocolSupported(dns))
                    {
                        // Get DNS Details
                        DnsReader dnsReader = new(dns, null);

                        // Apply ListenerProtocol Selection
                        bool matchRules = CheckDnsMatchRules(dnsReader);
                        if (!matchRules) continue;

                        // Get Status and Latency
                        CheckDns.CheckDnsResult cdr = await checkDns.CheckDnsAsync(blockedDomainNoWww, dns, timeoutMS);

                        if (cdr.IsDnsOnline)
                        {
                            if (!newSavedDnsList.Contains(dns))
                            {
                                newSavedDnsList.Add(dns);

                                if (UpdateWorkingServers)
                                    newWorkingDnsList.Add(new DnsInfo { DNS = dns, Latency = cdr.DnsLatency, CheckMode = CheckMode.SavedServers });

                                currentServers++;
                                if (currentServers >= maxServers) break;
                            }
                        }
                    }
                }
            }

            if (newSavedDnsList.Any())
            {
                SavedDnsList.Clear();
                SavedDnsList = new(newSavedDnsList);

                SavedEncodedDnsList.Clear();
                SavedEncodedDnsList = new(await BuiltInEncryptAsync(newSavedDnsList));
                await SavedEncodedDnsList.SaveToFileAsync(SecureDNS.SavedEncodedDnsPath);

                if (UpdateWorkingServers && !IsCheckingStarted)
                {
                    // Add
                    WorkingDnsList = WorkingDnsList.Concat(newWorkingDnsList).ToList();

                    // Remove Duplicates
                    WorkingDnsList = WorkingDnsList.DistinctBy(x => x.DNS).ToList();

                    // Update Status Long
                    await UpdateStatusLongAsync();
                }

                return;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SavedDnsServers SavedDnsUpdate: " + ex.Message);
        }
    }

}