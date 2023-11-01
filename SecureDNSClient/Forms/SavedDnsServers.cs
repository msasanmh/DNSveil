using MsmhToolsClass;
using MsmhToolsClass.DnsTool;
using System.Net;
using System.Reflection;

namespace SecureDNSClient;

public partial class FormMain
{
    private async Task WriteSavedServersDelayToLog()
    {
        List<string> savedDnsList = SavedDnsList.ToList();
        if (savedDnsList.Any() && !IsCheckingStarted && !IsConnecting && !IsDisconnecting)
        {
            if (!IsInternetAlive(false)) return;

            if (savedDnsList.Count > 1)
            {
                // Get blocked domain
                string blockedDomain = GetBlockedDomainSetting(out string blockedDomainNoWww);
                if (string.IsNullOrEmpty(blockedDomain)) return;

                // Get Check timeout value
                decimal timeoutSec = 1;
                this.InvokeIt(() => timeoutSec = CustomNumericUpDownSettingCheckTimeout.Value);
                int timeoutMS = decimal.ToInt32(timeoutSec * 1000);
                if (timeoutMS < 500) timeoutMS = 1000;

                // Get Bootstrap IP and Port
                string bootstrap = GetBootstrapSetting(out int bootstrapPort).ToString();

                // Add start msg
                string msgStart = $"{NL}Contains {savedDnsList.Count} servers:{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgStart, Color.LightGray));

                // Get insecure state
                bool insecure = CustomCheckBoxInsecure.Checked;
                int localPortInsecure = 5390;
                if (insecure)
                {
                    // Check open ports
                    bool isPortOk = GetListeningPort(localPortInsecure, "You need to resolve the conflict.", Color.IndianRed);
                    if (!isPortOk) return;

                    int nt = 1;

                    for (int n = 0; n < savedDnsList.Count; n++)
                    {
                        if (IsCheckingStarted || IsConnecting || IsDisconnecting) break;
                        await getDelay(n, nt++);
                    }
                }
                else
                {
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
                                    if (parallelLoopResult.IsCompleted)
                                        return Task.CompletedTask;
                                    await Task.Delay(500);
                                }
                                return Task.CompletedTask;
                            });
                        }
                    });
                }

                async Task getDelay(int n, int nt)
                {
                    string dns = savedDnsList[n];

                    // Get Status and Latency
                    CheckDns checkDns = new(false, GetCPUPriority());
                    if (insecure)
                        checkDns.CheckDNS(true, blockedDomainNoWww, dns, timeoutMS, localPortInsecure, bootstrap, bootstrapPort);
                    else
                        checkDns.CheckDNS(blockedDomainNoWww, dns, timeoutMS);

                    string msg = $"DNS {nt}: {checkDns.DnsLatency} ms.{NL}";
                    Color color = (checkDns.DnsLatency == -1) ? Color.IndianRed : Color.MediumSeaGreen;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, color));
                    await Task.Delay(10);
                }
            }
            else
            {
                string msg = $"There is only one saved server.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
            }
        }
        else
        {
            string msg = $"There is no saved server.{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
        }
    }

    private async void SavedDnsLoad()
    {
        await Task.Run(async () =>
        {
            FileDirectory.CreateEmptyFile(SecureDNS.SavedEncodedDnsPath);

            List<string> savedEncodedDnsList = new();
            savedEncodedDnsList.LoadFromFile(SecureDNS.SavedEncodedDnsPath, true, true);
            savedEncodedDnsList = savedEncodedDnsList.RemoveDuplicates();

            if (savedEncodedDnsList.Any())
            {
                // Built-in or Custom
                bool builtInMode = CustomRadioButtonBuiltIn.Checked;

                string? fileContent = string.Empty;
                if (builtInMode)
                    fileContent = await ResourceTool.GetResourceTextFileAsync("SecureDNSClient.DNS-Servers.txt", Assembly.GetExecutingAssembly());
                else
                {
                    // Check if Custom Servers XML is NOT Valid
                    if (!XmlTool.IsValidXMLFile(SecureDNS.CustomServersXmlPath)) return;

                    fileContent = await ReadCustomServersXml(SecureDNS.CustomServersXmlPath, null);
                }

                if (!string.IsNullOrEmpty(fileContent) && !string.IsNullOrWhiteSpace(fileContent))
                {
                    List<string> dnsList = fileContent.SplitToLines();

                    for (int n = 0; n < savedEncodedDnsList.Count; n++)
                    {
                        string encodedDns = savedEncodedDnsList[n];
                        for (int i = 0; i < dnsList.Count; i++)
                        {
                            string dns = dnsList[i];
                            if (EncodingTool.GetSHA512(dns).Equals(encodedDns))
                            {
                                SavedDnsList.Add(dns);
                                WorkingDnsList.Add(new Tuple<long, string>(100, dns));
                                break;
                            }
                        }
                    }

                    WorkingDnsList = WorkingDnsList.RemoveDuplicates(); // not important
                    SavedDnsList = SavedDnsList.RemoveDuplicates();
                }
            }

            SavedDnsUpdateAuto();
        });
    }

    private async void SavedDnsUpdateAuto()
    {
        await Task.Delay(2000);
        await SavedDnsUpdate();
        System.Timers.Timer savedDnsUpdateTimer = new();
        savedDnsUpdateTimer.Interval = TimeSpan.FromMinutes(60).TotalMilliseconds;
        savedDnsUpdateTimer.Elapsed += async (s, e) =>
        {
            await SavedDnsUpdate();
        };
        savedDnsUpdateTimer.Start();
    }

    private async Task SavedDnsUpdate()
    {
        // Update Working Servers?
        bool updateWorkingServers = true;

        // Get blocked domain
        string blockedDomain = GetBlockedDomainSetting(out string blockedDomainNoWww);
        if (string.IsNullOrEmpty(blockedDomain)) return;

        // Get Check timeout value
        decimal timeoutSec = 1;
        this.InvokeIt(() => timeoutSec = CustomNumericUpDownSettingCheckTimeout.Value);
        int timeoutMS = decimal.ToInt32(timeoutSec * 1000);

        // Get Bootstrap IP and Port
        string bootstrap = GetBootstrapSetting(out int bootstrapPort).ToString();

        // Get insecure state
        bool insecure = CustomCheckBoxInsecure.Checked;
        int localPortInsecure = 5390;

        // Get number of max servers
        int maxServers = decimal.ToInt32(CustomNumericUpDownSettingMaxServers.Value);

        // New Check
        CheckDns checkDns = new(false, GetCPUPriority());

        List<string> newSavedDnsList = new();
        List<string> newSavedEncodedDnsList = new();
        List<Tuple<long, string>> newWorkingDnsList = new();

        // Check saved dns servers can work
        if (SavedDnsList.Any())
        {
            if (!IsInternetAlive(false)) return;
            List<string> savedDnsList = SavedDnsList.ToList();

            if (insecure)
            {
                int origPort = localPortInsecure;
                bool isPortOpen1 = NetworkTool.IsPortOpen(IPAddress.Loopback.ToString(), localPortInsecure, 3);
                if (isPortOpen1)
                {
                    localPortInsecure = NetworkTool.GetNextPort(localPortInsecure);
                    bool isPortOpen2 = NetworkTool.IsPortOpen(IPAddress.Loopback.ToString(), localPortInsecure, 3);
                    if (isPortOpen2)
                    {
                        localPortInsecure = NetworkTool.GetNextPort(localPortInsecure);
                        bool isPortOpen3 = NetworkTool.IsPortOpen(IPAddress.Loopback.ToString(), localPortInsecure, 3);
                        if (isPortOpen3)
                        {
                            localPortInsecure = NetworkTool.GetNextPort(localPortInsecure);
                            bool isPortOpen4 = NetworkTool.IsPortOpen(IPAddress.Loopback.ToString(), localPortInsecure, 3);
                            if (isPortOpen4)
                            {
                                string existingProcessName = ProcessManager.GetProcessNameByListeningPort(origPort);
                                existingProcessName = existingProcessName == string.Empty ? "Unknown" : existingProcessName;
                                string msg = $"{NL}Cannot auto update Saved DNS List, port {origPort} is occupied by \"{existingProcessName}\".{NL}";
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                                return;
                            }
                        }
                    }
                }

                for (int n = 0; n < savedDnsList.Count; n++)
                {
                    if (!IsInternetAlive(false)) break;
                    if (IsCheckingStarted) break;

                    string dns = savedDnsList[n];
                    await checkDns.CheckDnsAsync(true, blockedDomainNoWww, dns, timeoutMS + 500, localPortInsecure, bootstrap, bootstrapPort);
                    if (checkDns.DnsLatency != -1)
                    {
                        newSavedDnsList.Add(dns);
                        newSavedEncodedDnsList.Add(EncodingTool.GetSHA512(dns));

                        if (updateWorkingServers)
                            newWorkingDnsList.Add(new Tuple<long, string>(checkDns.DnsLatency, dns));
                    }
                }
            }
            else
            {
                // If Secure
                await Task.Run(async () =>
                {
                    var lists = savedDnsList.SplitToLists(3);

                    for (int n = 0; n < lists.Count; n++)
                    {
                        if (!IsInternetAlive(false)) break;
                        if (IsCheckingStarted) break;

                        List<string> list = lists[n];

                        await Parallel.ForEachAsync(list, async (dns, cancellationToken) =>
                        {
                            await checkDns.CheckDnsAsync(blockedDomainNoWww, dns, timeoutMS + 500);
                            if (checkDns.DnsLatency != -1)
                            {
                                newSavedDnsList.Add(dns);
                                newSavedEncodedDnsList.Add(EncodingTool.GetSHA512(dns));
                                if (updateWorkingServers)
                                    newWorkingDnsList.Add(new Tuple<long, string>(checkDns.DnsLatency, dns));
                            }
                        });
                    }
                });
            }
        }

        if (newSavedDnsList.Any())
        {
            SavedDnsList.Clear();
            SavedDnsList = new(newSavedDnsList);

            SavedEncodedDnsList.Clear();
            SavedEncodedDnsList = new(newSavedEncodedDnsList);
            SavedEncodedDnsList.SaveToFile(SecureDNS.SavedEncodedDnsPath);

            if (updateWorkingServers && !IsCheckingStarted)
            {
                // Add
                WorkingDnsList = new(WorkingDnsList.Concat(newWorkingDnsList));

                // Remove Duplicates
                WorkingDnsList = new(WorkingDnsList.DistinctBy(x => x.Item2));
            }
        }

        if (newSavedDnsList.Count >= maxServers) return;

        // There is not enough working server lets find some
        // Built-in or Custom
        bool builtInMode = CustomRadioButtonBuiltIn.Checked;

        string? fileContent = string.Empty;
        if (builtInMode)
            fileContent = await ResourceTool.GetResourceTextFileAsync("SecureDNSClient.DNS-Servers.txt", Assembly.GetExecutingAssembly());
        else
        {
            FileDirectory.CreateEmptyFile(SecureDNS.CustomServersPath);
            fileContent = await File.ReadAllTextAsync(SecureDNS.CustomServersPath);
        }

        if (string.IsNullOrEmpty(fileContent) || string.IsNullOrWhiteSpace(fileContent)) return;

        List<string> dnsList = fileContent.SplitToLines();

        int currentServers = newSavedDnsList.Count;
        for (int n = 0; n < dnsList.Count; n++)
        {
            if (!IsInternetAlive(false)) break;
            if (IsCheckingStarted) break;

            string dns = dnsList[n].Trim();
            if (!string.IsNullOrEmpty(dns) && !string.IsNullOrWhiteSpace(dns))
            {
                if (IsDnsProtocolSupported(dns))
                {
                    // Get DNS Details
                    DnsReader dnsReader = new(dns, null);

                    // Apply Protocol Selection
                    bool matchRules = CheckDnsMatchRules(dnsReader);
                    if (!matchRules) continue;

                    // Get Status and Latency
                    if (insecure)
                        await checkDns.CheckDnsAsync(true, blockedDomainNoWww, dns, timeoutMS, localPortInsecure, bootstrap, bootstrapPort);
                    else
                        await checkDns.CheckDnsAsync(blockedDomainNoWww, dns, timeoutMS);

                    if (checkDns.IsDnsOnline)
                    {
                        if (!newSavedDnsList.Contains(dns))
                        {
                            newSavedDnsList.Add(dns);
                            newSavedEncodedDnsList.Add(EncodingTool.GetSHA512(dns));

                            if (updateWorkingServers)
                                newWorkingDnsList.Add(new Tuple<long, string>(checkDns.DnsLatency, dns));

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
            SavedEncodedDnsList = new(newSavedEncodedDnsList);
            SavedEncodedDnsList.SaveToFile(SecureDNS.SavedEncodedDnsPath);

            if (updateWorkingServers && !IsCheckingStarted)
            {
                // Add
                WorkingDnsList = WorkingDnsList.Concat(newWorkingDnsList).ToList();

                // Remove Duplicates
                WorkingDnsList = WorkingDnsList.DistinctBy(x => x.Item2).ToList();
            }

            return;
        }
    }

}