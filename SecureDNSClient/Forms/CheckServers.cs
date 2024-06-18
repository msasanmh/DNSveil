using CustomControls;
using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Net;
using System.Reflection;

namespace SecureDNSClient;

public partial class FormMain
{
    private async void StartCheck(CheckRequest checkRequest, bool stop = false, bool limitLog = false)
    {
        // Return if binary files are missing
        if (!CheckNecessaryFiles()) return;

        if (!IsCheckingStarted)
        {
            // Start Checking
            if (stop) return;
            IsCheckingStarted = true;
            await UpdateStatusShortOnBoolsChangedAsync();

            // Check Internet Connectivity
            if (!IsInternetOnline)
            {
                IsCheckingStarted = false;
                await UpdateStatusShortOnBoolsChangedAsync();
                return;
            }

            await CheckServers(checkRequest, limitLog);

            // Save working servers to file
            if (WorkingDnsListToFile.Any())
            {
                // Sort By Latency
                WorkingDnsListToFile = WorkingDnsListToFile.OrderBy(x => x.Latency).ToList();
                // Get Only Servers & Remove Duplicates
                List<string> exportList = WorkingDnsListToFile
                                          .Where(x => x.CheckMode != CheckMode.BuiltIn || x.CheckMode != CheckMode.SavedServers)
                                          .Select(x => x.DNS).Distinct().ToList();
                // Save to File
                if (exportList.Any()) await exportList.SaveToFileAsync(SecureDNS.WorkingServersPath);
            }

            string msg = $"{NL}Check Task: Finished{NL}";
            CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue);
            CustomButtonCheck.Enabled = true;

            IsCheckingStarted = false;
            StopChecking = false;
            await UpdateStatusShortOnBoolsChangedAsync();
            await UpdateStatusLongAsync();
        }
        else
        {
            // Stop Checking
            StopChecking = true;
            await UpdateStatusShortOnBoolsChangedAsync();
            this.InvokeIt(() => CustomProgressBarCheck.StopTimer = true);
            this.InvokeIt(() => CustomButtonCheck.Enabled = false);
        }
    }

    private bool CheckDnsMatchRules(DnsReader dnsReader)
    {
        bool matchRules = true;

        // Get Protocol Selection
        bool pDoH = CustomCheckBoxSettingProtocolDoH.Checked;
        bool pTLS = CustomCheckBoxSettingProtocolTLS.Checked;
        bool pDNSCrypt = CustomCheckBoxSettingProtocolDNSCrypt.Checked;
        bool pAnonymizedDNSCrypt = CustomCheckBoxSettingProtocolAnonymizedDNSCrypt.Checked;
        bool pDoQ = CustomCheckBoxSettingProtocolDoQ.Checked;
        bool pPlainDNS = CustomCheckBoxSettingProtocolPlainDNS.Checked;

        // Get SDNS Properties
        bool sdnsDNSSec = CustomCheckBoxSettingSdnsDNSSec.Checked;
        bool sdnsNoLog = CustomCheckBoxSettingSdnsNoLog.Checked;
        bool sdnsNoFilter = CustomCheckBoxSettingSdnsNoFilter.Checked;

        // Apply Protocol Selection
        if ((!pDoH && dnsReader.Protocol == DnsEnums.DnsProtocol.DoH) ||
            (!pTLS && dnsReader.Protocol == DnsEnums.DnsProtocol.DoT) ||
            (!pDNSCrypt && dnsReader.Protocol == DnsEnums.DnsProtocol.DnsCrypt) ||
            (!pAnonymizedDNSCrypt && dnsReader.Protocol == DnsEnums.DnsProtocol.AnonymizedDNSCrypt) ||
            (!pDoQ && dnsReader.Protocol == DnsEnums.DnsProtocol.DoQ) ||
            (!pPlainDNS && dnsReader.Protocol == DnsEnums.DnsProtocol.UDP) ||
            (!pPlainDNS && dnsReader.Protocol == DnsEnums.DnsProtocol.TCP)) matchRules = false;

        if (dnsReader.IsDnsCryptStamp)
        {
            // Apply SDNS rules
            if ((sdnsDNSSec && !dnsReader.StampReader.IsDnsSec) ||
                (sdnsNoLog && !dnsReader.StampReader.IsNoLog) ||
                (sdnsNoFilter && !dnsReader.StampReader.IsNoFilter)) matchRules = false;
        }

        return matchRules;
    }

    /// <summary>
    /// Check DNS Servers
    /// </summary>
    /// <returns>Returns True if find any working server</returns>
    private async Task<bool> CheckServers(CheckRequest checkRequest, bool limitLog = false)
    {
        // Get Clear Working Servers on new check
        bool clearOnNewCheck = true;
        this.InvokeIt(() => clearOnNewCheck = CustomCheckBoxSettingCheckClearWorkingServers.Checked);

        // Clear Log on new Check
        if (!Program.IsStartup)
            this.InvokeIt(() => CustomRichTextBoxLog.ResetText());

        // Check servers comment
        string msg = $"Checking Servers (Group: {checkRequest.GroupName}):{NL}{NL}";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));

        // Check If Another Proxy Is Set
        // Update bool IsProxySet
        IsProxySet = UpdateBoolIsProxySet(out bool isAnotherProxySet, out string currentSystemProxy);
        IsAnotherProxySet = isAnotherProxySet;
        CurrentSystemProxy = currentSystemProxy;
        if (IsAnotherProxySet && !Program.IsStartup)
        {
            string url = "https://www.google.com";
            bool canOpenUrl = await NetworkTool.IsWebsiteOnlineAsync(url, null, 5000, true);
            if (!canOpenUrl)
            {
                msg = $"Another Proxy ({CurrentSystemProxy}) is set to your System and cannot open {url}";
                msg += $"{NL}Unset the Proxy?";
                DialogResult dr = CustomMessageBox.Show(this, msg, "A Proxy Is Set", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dr == DialogResult.Yes)
                {
                    // Unset The Proxy
                    NetworkTool.UnsetProxy(false, true);
                }
            }
        }

        // Get blocked domain
        string blockedDomain = GetBlockedDomainSetting(out string blockedDomainNoWww);
        if (string.IsNullOrEmpty(blockedDomain)) return false;

        // Get Bootstrap IP and Port
        IPAddress bootstrapIP = GetBootstrapSetting(out int bootstrapPort);

        // Series or Parallel
        int parallelSize = GetParallelSizeSetting();
        bool checkInParallel = parallelSize > 1;

        // Get Insecure
        bool insecure = CustomCheckBoxInsecure.Checked;

        List<ReadDnsResult> rdrList = new();
        
        if (checkRequest.CheckMode == CheckMode.BuiltIn)
        {
            msg = $"Downloading And Reading Built-In Servers...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));

            rdrList = await ReadBuiltInServersAsync(checkRequest, false);
            if (insecure)
            {
                List<ReadDnsResult> rdrListInsecure = await ReadBuiltInServersAsync(checkRequest, true);
                if (rdrListInsecure.Any()) rdrList.AddRange(rdrListInsecure);
            }
        }

        if (checkRequest.CheckMode == CheckMode.CustomServers)
        {
            // Check If Custom Servers XML Is NOT Valid
            if (!XmlTool.IsValidXMLFile(SecureDNS.CustomServersXmlPath))
            {
                msg = $"Custom Servers XML File Is Not Valid.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return false;
            }

            msg = $"Reading Custom Servers...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));

            rdrList = await ReadCustomServersXml(SecureDNS.CustomServersXmlPath, checkRequest);

            // Load Saved Custom Working Servers
            WorkingDnsListToFile.Clear();
            List<string> import = new();
            await import.LoadFromFileAsync(SecureDNS.WorkingServersPath, true, true);
            for (int n = 0; n < import.Count; n++)
            {
                string workingCustomServer = import[n];
                WorkingDnsListToFile.Add(new DnsInfo { DNS = workingCustomServer, Latency = n, CheckMode = CheckMode.CustomServers });
            }
        }

        if (checkRequest.CheckMode == CheckMode.MixedServers)
        {
            if (WorkingDnsList.Any())
            {
                List<DnsInfo> workingDnsList = WorkingDnsList.ToList();
                if (workingDnsList.Any())
                {
                    // Msg Adding Saved Servers With No Latency
                    msg = $"Reading Current Working Servers...{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));

                    for (int n = 0; n < workingDnsList.Count; n++)
                    {
                        DnsInfo di = workingDnsList[n];
                        rdrList.Add(new ReadDnsResult { DNS = di.DNS, CheckMode = di.CheckMode, GroupName = di.GroupName });
                    }

                    WorkingDnsList.Clear();
                }
            }
        }

        // Check If Servers Exist 1
        
        if (!rdrList.Any())
        {
            msg = $"There Is No Server.{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
            return false;
        }

        if (!clearOnNewCheck && checkRequest.CheckMode != CheckMode.MixedServers && WorkingDnsList.Any())
        {
            List<DnsInfo> savedList = WorkingDnsList.Where(x => x.Latency < 1).ToList();
            if (savedList.Any())
            {
                // Msg Adding Saved Servers With No Latency
                msg = $"Adding Saved Servers With No Latency...{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));

                for (int n = 0; n < savedList.Count; n++)
                {
                    DnsInfo di = savedList[n];
                    rdrList.Add(new ReadDnsResult { DNS = di.DNS, CheckMode = CheckMode.SavedServers, GroupName = di.GroupName });
                }
            }
        }

        // Msg Remove Duplicates
        msg = $"Removing Duplicates...{NL}";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));

        // DeDup Dns List
        rdrList = rdrList.DistinctBy(x => x.DNS).ToList();
        WorkingDnsList = WorkingDnsList.DistinctBy(x => x.DNS).ToList();

        int dnsCount = rdrList.Count;

        // Check if servers exist 2
        if (dnsCount < 1)
        {
            msg = $"There Is No Server.{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
            return false;
        }

        // Init Check DNS
        CheckDns checkDns = new(insecure, false);

        // FlushDNS(); // Flush DNS makes first line always return failed without bootstrap

        // Clear Working Servers on new check
        if (clearOnNewCheck || checkRequest.ClearWorkingServers) WorkingDnsList.Clear();

        // Create A New List
        List<DnsInfo> newWorkingDnsList = new();

        // Get Company Data Content
        string? companyDataContent = await ResourceTool.GetResourceTextFileAsync("SecureDNSClient.HostToCompany.txt", Assembly.GetExecutingAssembly()); // Load from Embedded Resource

        // Set number of servers
        int numberOfAllServers = 0;
        int numberOfAllSupportedServers = 0;
        int numberOfCheckedServers = 0;

        // Dummy check to fix first run
        if (rdrList.Count > 0)
            await checkDns.CheckDnsAsync(blockedDomainNoWww, rdrList[0].DNS, 500);
        if (rdrList.Count > 1)
            await checkDns.CheckDnsAsync(blockedDomainNoWww, rdrList[1].DNS, 500);
        if (rdrList.Count > 2)
            await checkDns.CheckDnsAsync(blockedDomainNoWww, rdrList[2].DNS, 500);

        if (checkInParallel) await checkParallel();
        else await checkSeries();

        async Task checkSeries()
        {
            this.InvokeIt(() => CustomProgressBarCheck.StopTimer = false);
            this.InvokeIt(() => CustomProgressBarCheck.ChunksColor = Color.DodgerBlue);
            this.InvokeIt(() => CustomProgressBarCheck.Maximum = dnsCount);
            for (int n = 0; n < dnsCount; n++)
            {
                if (!IsInternetOnline) StopChecking = true;
                if (StopChecking)
                {
                    msg = NL + "Canceling Check Operation..." + NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));
                    break;
                }

                // Percentage
                string checkDetail = $"{n + 1} Of {dnsCount}";
                this.InvokeIt(() => CustomProgressBarCheck.CustomText = checkDetail);
                this.InvokeIt(() => CustomProgressBarCheck.Value = n);

                ReadDnsResult rdr = rdrList[n];
                await checkOne(rdr, n + 1);

                // Percentage (100%)
                if (n == dnsCount - 1)
                {
                    checkDetail = $"{n + 1} Of {dnsCount}";
                    this.InvokeIt(() => CustomProgressBarCheck.CustomText = checkDetail);
                    this.InvokeIt(() => CustomProgressBarCheck.Value = dnsCount);
                }
            }
        }

        async Task checkParallel()
        {
            await Task.Run(async () =>
            {
                int splitSize = parallelSize;
                var lists = rdrList.SplitToLists(splitSize);
                int count = 0;

                this.InvokeIt(() => CustomProgressBarCheck.StopTimer = false);
                this.InvokeIt(() => CustomProgressBarCheck.ChunksColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomProgressBarCheck.Minimum = 0);
                this.InvokeIt(() => CustomProgressBarCheck.Value = 0);
                this.InvokeIt(() => CustomProgressBarCheck.Maximum = dnsCount);
                for (int n = 0; n < lists.Count; n++)
                {
                    if (!IsInternetOnline) StopChecking = true;
                    if (StopChecking)
                    {
                        msg = NL + "Canceling Check Operation..." + NL;
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));
                        break;
                    }

                    List<ReadDnsResult> list = lists[n];

                    // Percentage
                    count += list.Count;
                    string checkDetail = $"{count} Of {dnsCount}";
                    this.InvokeIt(() => CustomProgressBarCheck.CustomText = checkDetail);

                    await Parallel.ForEachAsync(list, async (rdr, cancellationToken) =>
                    {
                        if (!IsInternetOnline) StopChecking = true;
                        if (StopChecking) return;
                        this.InvokeIt(() => CustomProgressBarCheck.Value += 1);
                        await checkOne(rdr, -1);
                    });
                    
                    // Percentage (100%)
                    if (n == lists.Count - 1)
                    {
                        checkDetail = $"{dnsCount} Of {dnsCount}";
                        this.InvokeIt(() => CustomProgressBarCheck.CustomText = checkDetail);
                        this.InvokeIt(() => CustomProgressBarCheck.Value = dnsCount);
                    }
                }
            });
        }

        async Task checkOne(ReadDnsResult rdr, int lineNumber)
        {
            bool isPrivate = rdr.CheckMode == CheckMode.BuiltIn || rdr.CheckMode == CheckMode.SavedServers;
            
            if (!string.IsNullOrEmpty(rdr.DNS))
            {
                // All Servers ++
                numberOfAllServers++;
                
                if (IsDnsProtocolSupported(rdr.DNS))
                {
                    // All Supported Servers ++
                    numberOfAllSupportedServers++;
                    
                    // Get DNS Details
                    DnsReader dnsReader = new(rdr.DNS, companyDataContent);
                    
                    //// Get Unknown Companies (Debug Only)
                    //if (dnsReader.CompanyName.ToLower().Contains("couldn't retrieve"))
                    //{
                    //    string p = @"C:\Users\msasa\OneDrive\Desktop\getInfo.txt";
                    //    FileDirectory.CreateEmptyFile(p);
                    //    string serv = dnsReader.Host;
                    //    if (string.IsNullOrEmpty(serv) && !string.IsNullOrEmpty(dnsReader.IP)) serv = dnsReader.IP;
                    //    FileDirectory.AppendTextLine(p, serv, new System.Text.UTF8Encoding(false));
                    //}
                    //if (dnsReader.CompanyName.ToLower().Contains("china"))
                    //    System.Diagnostics.Debug.WriteLine("CHINA: " + dns);

                    // Apply Protocol Selection
                    bool matchRules = CheckDnsMatchRules(dnsReader);
                    if (!matchRules) return;

                    // Get Check Timeout Setting
                    int timeoutMS = GetCheckTimeoutSetting();
                    if (checkRequest.CheckMode == CheckMode.MixedServers) timeoutMS += 300;

                    // Is This Being Called By Parallel?
                    bool isParallel = lineNumber == -1;

                    CheckDns.CheckDnsResult cdr = await checkDns.CheckDnsAsync(blockedDomainNoWww, rdr.DNS, timeoutMS, bootstrapIP, bootstrapPort);

                    // Get Status and Latency
                    bool dnsOK = cdr.IsDnsOnline;
                    int latency = cdr.DnsLatency;

                    // Add Working DNS To List
                    if (dnsOK)
                    {
                        DnsInfo dnsInfo = new()
                        {
                            DNS = rdr.DNS, Latency = latency, CheckMode = rdr.CheckMode, GroupName = rdr.GroupName
                        };

                        lock (WorkingDnsList)
                        {
                            int index = WorkingDnsList.FindIndex(x => x.DNS == rdr.DNS);
                            if (index != -1) WorkingDnsList[index] = dnsInfo; // Update If Exist
                            else WorkingDnsList.Add(dnsInfo); // Add If Not Exist
                        }
                        
                        lock (newWorkingDnsList)
                        {
                            newWorkingDnsList.Add(dnsInfo);
                        }
                        
                        lock (WorkingDnsListToFile)
                        {
                            // Add Working DNS To List To Export
                            if (!isPrivate)
                                WorkingDnsListToFile.Add(dnsInfo);
                        }
                        
                        // Don't Let Working Servers List Get Updated In The Background If User Want To Use Custom Servers Only
                        IsCheckedCustomServers = dnsInfo.CheckMode == CheckMode.CustomServers;
                    }

                    // Checked Servers ++
                    numberOfCheckedServers++;

                    if (isParallel) writeStatusToLogParallel();
                    else writeStatusToLogSeries();

                    void writeStatusToLogParallel()
                    {
                        if (limitLog) return;

                        // Write Short Status To Log
                        string status = dnsOK ? "OK" : "Failed";
                        Color color = dnsOK ? Color.MediumSeaGreen : Color.IndianRed;

                        // Write host to log
                        string host = string.Empty;
                        string protocol = string.Empty;
                        if (dnsReader.IsDnsCryptStamp)
                        {
                            string sdnsHostMsg1 = $" [SDNS: ";
                            string sdnsHostMsg2 = $"{dnsReader.ProtocolName}";
                            string sdnsHostMsg3 = "]";
                            if (dnsReader.StampReader.IsDnsSec)
                                sdnsHostMsg2 += $", DNSSec";
                            if (dnsReader.StampReader.IsNoLog)
                                sdnsHostMsg2 += $", No Log";
                            if (dnsReader.StampReader.IsNoFilter)
                                sdnsHostMsg2 += $", No Filter";

                            host = sdnsHostMsg1 + sdnsHostMsg2 + sdnsHostMsg3;
                        }
                        else
                        {
                            // Write Host To Log
                            if (!isPrivate)
                            {
                                host = $" [Host: {rdr.DNS}]";
                            }

                            // Write Protocol Name To Log
                            protocol = $" [Protocol: {dnsReader.ProtocolName}]";
                        }

                        // Write Company Name To Log
                        string resultCompany = string.IsNullOrEmpty(dnsReader.CompanyName) ? string.Empty : $" [{dnsReader.CompanyName}]";

                        // Write All
                        msg = $"[{status}] [{latency}]{host}{protocol}{resultCompany}{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, color));
                    }

                    void writeStatusToLogSeries()
                    {
                        if (limitLog) return;

                        // Write Line Number To Log
                        if (lineNumber != -1)
                        {
                            msg = $"{lineNumber}. ";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.LightGray));
                        }

                        // Write Status To Log
                        string status = dnsOK ? "OK" : "Failed";
                        Color color = dnsOK ? Color.MediumSeaGreen : Color.IndianRed;
                        msg = $"[{status}]";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, color));

                        // Write Latency To Log
                        msg = $" [{latency}]";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));

                        // Write Host To Log
                        if (dnsReader.IsDnsCryptStamp)
                        {
                            string sdnsHostMsg1 = $" [SDNS: ";
                            string sdnsHostMsg2 = $"{dnsReader.ProtocolName}";
                            string sdnsHostMsg3 = "]";
                            if (dnsReader.StampReader.IsDnsSec)
                                sdnsHostMsg2 += $", DNSSec";
                            if (dnsReader.StampReader.IsNoLog)
                                sdnsHostMsg2 += $", No Log";
                            if (dnsReader.StampReader.IsNoFilter)
                                sdnsHostMsg2 += $", No Filter";

                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(sdnsHostMsg1, Color.LightGray));
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(sdnsHostMsg2, Color.Orange));
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(sdnsHostMsg3, Color.LightGray));
                        }
                        else
                        {
                            // Write Host To Log
                            if (!isPrivate)
                            {
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(" [Host: ", Color.LightGray));
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(rdr.DNS, Color.DodgerBlue));
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText("]", Color.LightGray));
                            }

                            // Write Protocol Name To Log
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(" [Protocol: ", Color.LightGray));
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(dnsReader.ProtocolName, Color.Orange));
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText("]", Color.LightGray));
                        }

                        // Write Company Name To Log
                        if (!string.IsNullOrEmpty(dnsReader.CompanyName))
                        {
                            string resultCompany = $" [{dnsReader.CompanyName}]";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultCompany, Color.Gray));
                        }

                        // Write New Line
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(NL));
                    }
                }
            }

        }

        // Return If There Is No Working Server
        if (!WorkingDnsList.Any() && !newWorkingDnsList.Any())
        {
            string noWorkingServer = NL + "There is no working server." + NL;
            if (StopChecking || IsDisconnecting) noWorkingServer = NL + "Task Canceled." + NL;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(noWorkingServer, Color.IndianRed));
            return false;
        }

        // Get Number Of Max Servers
        int maxServers = GetMaxServersToConnectSetting();

        // Sort By Latency
        if (WorkingDnsList.Count > 1)
            WorkingDnsList = WorkingDnsList.OrderByDescending(x => x.Latency).ToList();
        if (newWorkingDnsList.Count > 1)
            newWorkingDnsList = newWorkingDnsList.OrderByDescending(x => x.Latency).ToList();

        // Check Status - List
        List<DnsInfo> checkStatusList = newWorkingDnsList.OrderBy(x => x.Latency).ToList();
        
        // Check Status - Clear
        this.InvokeIt(() => CustomRichTextBoxCheckStatus.ResetText());
        this.InvokeIt(() => CustomRichTextBoxCheckStatus.AppendText($"Last Scan: ({DateTime.Now:yyyy/MM/dd HH:mm:ss}){NL}{NL}"));

        for (int i = 0; i < checkStatusList.Count; i++)
        {
            DnsInfo dnsInfo = checkStatusList[i];
            // Check Status - Group Name
            this.InvokeIt(() => CustomRichTextBoxCheckStatus.AppendText($"{i + 1}. "));
            this.InvokeIt(() => CustomRichTextBoxCheckStatus.AppendText(dnsInfo.GroupName + NL, Color.DodgerBlue));

            if (dnsInfo.Latency > 0)
            {
                this.InvokeIt(() => CustomRichTextBoxCheckStatus.AppendText("Latency: "));
                this.InvokeIt(() => CustomRichTextBoxCheckStatus.AppendText($"{dnsInfo.Latency}", Color.DodgerBlue));
                this.InvokeIt(() => CustomRichTextBoxCheckStatus.AppendText($" ms{NL}"));
            }

            string reducted = "Reducted";
            string dns = dnsInfo.CheckMode == CheckMode.BuiltIn || dnsInfo.CheckMode == CheckMode.SavedServers ? reducted : dnsInfo.DNS;
            string msgDnsAddress = dns.Equals(reducted) ? "DNS Address: " : $"DNS Address:{NL}";
            this.InvokeIt(() => CustomRichTextBoxCheckStatus.AppendText(msgDnsAddress));
            this.InvokeIt(() => CustomRichTextBoxCheckStatus.AppendText($"{dns}{NL}{NL}", Color.DarkGray));

            if (i + 1 >= maxServers) break;
        }

        // Sort By Latency Comment
        if (!limitLog)
        {
            string allWorkingServers = NL + "Working servers sorted by latency:" + NL + NL;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(allWorkingServers, Color.MediumSeaGreen));

            // All Latencies (To Get Average Delay)
            long allLatencies = 0;
            bool once = true;
            string fastest = $"Fastest Servers:{NL}";

            int workingDnsCount = newWorkingDnsList.Count;

            for (int n = 0; n < newWorkingDnsList.Count; n++)
            {
                DnsInfo dnsInfo = newWorkingDnsList[n];

                bool isPrivate = dnsInfo.CheckMode == CheckMode.BuiltIn || dnsInfo.CheckMode == CheckMode.SavedServers;

                long latency = dnsInfo.Latency;
                string host = dnsInfo.DNS;
                string dns = host;

                allLatencies += latency;

                // Write Slowest Server To Log
                if (n == 0 && workingDnsCount >= 2)
                {
                    string slowest = $"Slowest Server:{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(slowest, Color.MediumSeaGreen));

                    // Get DNS Details
                    DnsReader dnsReader = new(dns, companyDataContent);

                    // Write Sorted Result To Log
                    writeSortedStatusToLog(dnsReader);
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(NL));
                }
                else if (workingDnsCount <= maxServers + 1)
                {
                    if (once)
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(fastest, Color.MediumSeaGreen));
                    once = false;

                    // Get DNS Details
                    DnsReader dnsReader = new(dns, companyDataContent);

                    // Write Sorted Result To Log
                    writeSortedStatusToLog(dnsReader);
                }
                else if (workingDnsCount - n <= maxServers)
                {
                    if (once)
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(fastest, Color.MediumSeaGreen));
                    once = false;

                    // Get DNS Details
                    DnsReader dnsReader = new(dns, companyDataContent);

                    // Write Sorted Result To Log
                    writeSortedStatusToLog(dnsReader);
                }

                void writeSortedStatusToLog(DnsReader dnsReader)
                {
                    // Write Latency To Log
                    if (latency > 0)
                    {
                        string resultLatency1 = "[Latency:";
                        string resultLatency2 = $" {latency}";
                        string resultLatency3 = " ms]";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultLatency1, Color.LightGray));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultLatency2, Color.DodgerBlue));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultLatency3, Color.LightGray));
                    }

                    // Write Host To Log
                    if (dnsReader.IsDnsCryptStamp)
                    {
                        string sdnsHostMsg1 = $" [SDNS: ";
                        string sdnsHostMsg2 = $"{dnsReader.ProtocolName}";
                        string sdnsHostMsg3 = "]";
                        if (dnsReader.StampReader.IsDnsSec)
                            sdnsHostMsg2 += $", DNSSec";
                        if (dnsReader.StampReader.IsNoLog)
                            sdnsHostMsg2 += $", No Log";
                        if (dnsReader.StampReader.IsNoFilter)
                            sdnsHostMsg2 += $", No Filter";

                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(sdnsHostMsg1, Color.LightGray));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(sdnsHostMsg2, Color.Orange));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(sdnsHostMsg3, Color.LightGray));
                    }
                    else
                    {
                        // Write Host To Log
                        if (!isPrivate)
                        {
                            string resultHost1 = " [Host:";
                            string resultHost2 = $" {host}";
                            string resultHost3 = "]";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultHost1, Color.LightGray));
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultHost2, Color.DodgerBlue));
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultHost3, Color.LightGray));
                        }

                        // Write Protocol To Log
                        string resultProtocol1 = " [Protocol:";
                        string resultProtocol2 = $" {dnsReader.ProtocolName}";
                        string resultProtocol3 = "]";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultProtocol1, Color.LightGray));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultProtocol2, Color.Orange));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultProtocol3, Color.LightGray));
                    }

                    // Write Company Name To Log
                    if (!string.IsNullOrEmpty(dnsReader.CompanyName))
                    {
                        string resultCompany1 = " [Company:";
                        string resultCompany2 = $" {dnsReader.CompanyName}";
                        string resultCompany3 = "]";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultCompany1, Color.LightGray));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultCompany2, Color.DodgerBlue));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultCompany3, Color.LightGray));
                    }

                    // Write New Line
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(NL));
                }
            }

            // Write Count To Log
            string msgCount1 = "Found "; // workingServers
            string msgCount2 = " Working Server"; if (workingDnsCount > 1) msgCount2 += "s";
            string msgCount3 = " Out Of "; // numberOfAllSupportedServers
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCount1, Color.LightGray));
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(workingDnsCount.ToString(), Color.MediumSeaGreen));
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCount2, Color.LightGray));
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCount3, Color.LightGray));
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(numberOfAllSupportedServers.ToString() + NL, Color.MediumSeaGreen));

            // Number Of Failed Servers
            int numberOfFailedServers = numberOfCheckedServers - workingDnsCount;
            if (numberOfFailedServers > 0)
            {
                string msgFailed = "Failed Servers: ";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgFailed, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(numberOfFailedServers.ToString() + NL, Color.IndianRed));
            }

            // Ignored By Settings
            int numberOfIgnoredBySettings = numberOfAllSupportedServers - numberOfCheckedServers;
            if (numberOfIgnoredBySettings > 0)
            {
                string msgIgnoredBySettings = "Ignored By Settings: ";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgIgnoredBySettings, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(numberOfIgnoredBySettings.ToString() + NL, Color.MediumSeaGreen));
            }

            // Ignored By Not Supported Protocol
            int numberOfNotSupported = numberOfAllServers - numberOfAllSupportedServers;
            if (numberOfNotSupported > 0)
            {
                string msgIgnoredByWrongProtocol = "Not Supported Protocol: ";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgIgnoredByWrongProtocol, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(numberOfNotSupported.ToString() + NL, Color.MediumSeaGreen));
            }

            // Write Average Delay To Log
            if (workingDnsCount > 1)
            {
                string msgAL1 = "Average Latency: ";
                string msgAL2 = " ms.";
                long averageLatency = allLatencies / workingDnsCount;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgAL1, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"{averageLatency}", Color.DodgerBlue));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgAL2 + NL, Color.LightGray));
            }
        }
        
        return true;
    }
}