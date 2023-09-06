using System;
using System.Net;
using CustomControls;
using MsmhToolsClass;
using MsmhToolsClass.DnsTool;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

namespace SecureDNSClient
{
    public partial class FormMain
    {
        private async void StartCheck(string? groupName = null)
        {
            // Return if binary files are missing
            if (!CheckNecessaryFiles()) return;

            if (!IsCheckingStarted)
            {
                // Start Checking
                IsCheckingStarted = true;

                // Check Internet Connectivity
                if (!IsInternetAlive())
                {
                    IsCheckingStarted = false;
                    return;
                }

                // Unset DNS if it's not connected before checking.
                if (!IsDNSConnected && IsDNSSet)
                {
                    SetDNS(); // Unset DNS
                }

                try
                {
                    Task taskCheck = Task.Run(async () => await CheckServers(groupName));

                    await taskCheck.ContinueWith(_ =>
                    {
                        // Save working servers to file
                        if (!CustomRadioButtonBuiltIn.Checked && WorkingDnsAndLatencyListToFile.Any())
                        {
                            // Sort By Latency
                            WorkingDnsAndLatencyListToFile = WorkingDnsAndLatencyListToFile.OrderBy(t => t.Item1).ToList();
                            // Add Servers to WorkingDnsListToFile
                            WorkingDnsListToFile = WorkingDnsListToFile.Concat(WorkingDnsAndLatencyListToFile.Select(t => t.Item2).ToList()).ToList();
                            // Remove Duplicates
                            WorkingDnsListToFile = WorkingDnsListToFile.RemoveDuplicates();
                            // Save to File
                            WorkingDnsListToFile.SaveToFile(SecureDNS.WorkingServersPath);
                        }

                        IsCheckingStarted = false;

                        string msg = $"{NL}Check Task: {taskCheck.Status}{NL}";
                        CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue);
                        CustomButtonCheck.Enabled = true;

                        StopChecking = false;
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(this, ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Stop Checking
                StopChecking = true;
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
            bool pDNSCryptRelay = CustomCheckBoxSettingProtocolDNSCryptRelay.Checked;
            bool pDoQ = CustomCheckBoxSettingProtocolDoQ.Checked;
            bool pPlainDNS = CustomCheckBoxSettingProtocolPlainDNS.Checked;

            // Get SDNS Properties
            bool sdnsDNSSec = CustomCheckBoxSettingSdnsDNSSec.Checked;
            bool sdnsNoLog = CustomCheckBoxSettingSdnsNoLog.Checked;
            bool sdnsNoFilter = CustomCheckBoxSettingSdnsNoFilter.Checked;

            // Apply Protocol Selection
            if ((!pDoH && dnsReader.Protocol == DnsReader.DnsProtocol.DoH) ||
                (!pTLS && dnsReader.Protocol == DnsReader.DnsProtocol.DoT) ||
                (!pDNSCrypt && dnsReader.Protocol == DnsReader.DnsProtocol.DnsCrypt) ||
                (!pDNSCryptRelay && dnsReader.Protocol == DnsReader.DnsProtocol.AnonymizedDNSCryptRelay) ||
                (!pDoQ && dnsReader.Protocol == DnsReader.DnsProtocol.DoQ) ||
                (!pPlainDNS && dnsReader.Protocol == DnsReader.DnsProtocol.PlainDNS)) matchRules = false;

            if (dnsReader.IsDnsCryptStamp)
            {
                // Apply SDNS rules
                if ((sdnsDNSSec && !dnsReader.StampProperties.IsDnsSec) ||
                    (sdnsNoLog && !dnsReader.StampProperties.IsNoLog) ||
                    (sdnsNoFilter && !dnsReader.StampProperties.IsNoFilter)) matchRules = false;
            }

            return matchRules;
        }

        /// <summary>
        /// Check DNS Servers
        /// </summary>
        /// <param name="groupName">Custom Servers Group Name To Check</param>
        /// <returns>Returns True if find any working server</returns>
        private async Task<bool> CheckServers(string? groupName = null)
        {
            // Get blocked domain
            string blockedDomain = GetBlockedDomainSetting(out string blockedDomainNoWww);
            if (string.IsNullOrEmpty(blockedDomain)) return false;

            // Clear Log on new Check
            this.InvokeIt(() => CustomRichTextBoxLog.ResetText());

            // Check servers comment
            string checkingServers = "Checking servers:" + NL + NL;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(checkingServers, Color.MediumSeaGreen));

            // Get Bootstrap IP and Port
            string bootstrap = GetBootstrapSetting(out int bootstrapPort).ToString();

            // Series or Parallel
            bool checkInParallel = CustomCheckBoxCheckInParallel.Checked;

            // Get insecure state
            bool insecure = CustomCheckBoxInsecure.Checked;
            int localPort = 5390;

            // Check open ports
            if (!checkInParallel)
            {
                bool isPortOpen = NetworkTool.IsPortOpen(IPAddress.Loopback.ToString(), localPort, 3);
                if (isPortOpen)
                {
                    localPort = NetworkTool.GetNextPort(localPort);
                    isPortOpen = NetworkTool.IsPortOpen(IPAddress.Loopback.ToString(), localPort, 3);
                    if (isPortOpen)
                    {
                        localPort = NetworkTool.GetNextPort(localPort);
                        bool isPortOk = GetListeningPort(localPort, "You need to resolve the conflict.", Color.IndianRed);
                        if (!isPortOk) return false;
                    }
                }
            }
            
            // Built-in or Custom
            bool builtInMode = CustomRadioButtonBuiltIn.Checked;

            // Override Built-in or Custom
            if (!string.IsNullOrEmpty(groupName))
            {
                builtInMode = groupName.Equals("builtin");
            }

            string? fileContent = string.Empty;
            if (builtInMode)
            {
                string? xmlContent = await ResourceTool.GetResourceTextFileAsync("SecureDNSClient.DNS-Servers.sdcs", Assembly.GetExecutingAssembly());
                fileContent = await ReadCustomServersXml(xmlContent, groupName, false); // Built-In based on custom
            }
            else
            {
                // Check if Custom Servers XML is NOT Valid
                if (!XmlTool.IsValidXMLFile(SecureDNS.CustomServersXmlPath))
                {
                    string notValid = $"Custom Servers XML file is not valid.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(notValid, Color.IndianRed));
                    return false;
                }

                string msg = $"Reading Custom Servers...{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));

                fileContent = await ReadCustomServersXml(SecureDNS.CustomServersXmlPath, groupName);
                
                // Load saved working servers
                WorkingDnsListToFile.Clear();
                WorkingDnsListToFile.LoadFromFile(SecureDNS.WorkingServersPath, true, true);
            }

            // Clear working list to file on new check
            WorkingDnsAndLatencyListToFile.Clear();

            // Check if servers exist 1
            string custom = builtInMode ? "built-in" : "custom";
            string msgNoServers = $"There is no {custom} server.{NL}";
            if (string.IsNullOrEmpty(fileContent) || string.IsNullOrWhiteSpace(fileContent))
            {
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgNoServers, Color.IndianRed));
                return false;
            }

            // Add Servers to list
            List<string> dnsList = fileContent.SplitToLines();
            int dnsCount = dnsList.Count;

            // Check if servers exist 2
            if (dnsCount < 1)
            {
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgNoServers, Color.IndianRed));
                return false;
            }

            // init Check DNS
            CheckDns checkDns = new(false, GetCPUPriority());

            // FlushDNS(); // Flush DNS makes first line always return failed

            // Clear temp working list on new check
            WorkingDnsList.Clear();

            // Get Company Data Content
            string? companyDataContent = await ResourceTool.GetResourceTextFileAsync("SecureDNSClient.HostToCompany.txt", Assembly.GetExecutingAssembly()); // Load from Embedded Resource

            // Set number of servers
            int numberOfAllServers = 0;
            int numberOfAllSupportedServers = 0;
            int numberOfCheckedServers = 0;

            // Dummy check to fix first run
            checkDns.CheckDNS(blockedDomainNoWww, dnsList[0], 100);
            checkDns.CheckDNS(blockedDomainNoWww, dnsList[0], 100);
            checkDns.CheckDNS(blockedDomainNoWww, dnsList[0], 100);

            if (insecure)
                await checkSeries();
            else
            {
                if (checkInParallel)
                    await checkParallel();
                else
                    await checkSeries();
            }

            async Task checkSeries()
            {
                this.InvokeIt(() => CustomProgressBarCheck.StopTimer = false);
                this.InvokeIt(() => CustomProgressBarCheck.ChunksColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomProgressBarCheck.Maximum = dnsCount);
                for (int n = 0; n < dnsCount; n++)
                {
                    if (StopChecking)
                    {
                        string msg = NL + "Canceling Check operation..." + NL;
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));
                        break;
                    }

                    // Percentage
                    string checkDetail = $"{n + 1} of {dnsCount}";
                    this.InvokeIt(() => CustomProgressBarCheck.CustomText = checkDetail);
                    this.InvokeIt(() => CustomProgressBarCheck.Value = n);

                    string dns = dnsList[n].Trim();
                    await checkOne(dns, n + 1);

                    // Percentage (100%)
                    if (n == dnsCount - 1)
                    {
                        checkDetail = $"{n + 1} of {dnsCount}";
                        this.InvokeIt(() => CustomProgressBarCheck.CustomText = checkDetail);
                        this.InvokeIt(() => CustomProgressBarCheck.Value = dnsCount);
                    }
                }
            }

            async Task checkParallel()
            {
                await Task.Run(async () =>
                {
                    int splitSize = 5;
                    var lists = dnsList.SplitToLists(splitSize);
                    int count = 0;

                    this.InvokeIt(() => CustomProgressBarCheck.StopTimer = false);
                    this.InvokeIt(() => CustomProgressBarCheck.ChunksColor = Color.DodgerBlue);
                    this.InvokeIt(() => CustomProgressBarCheck.Maximum = lists.Count);
                    for (int n = 0; n < lists.Count; n++)
                    {
                        if (StopChecking)
                        {
                            string msg = NL + "Canceling Check operation..." + NL;
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));
                            break;
                        }

                        List<string> list = lists[n];

                        // Percentage
                        count += list.Count;
                        string checkDetail = $"{count} of {dnsCount}";
                        this.InvokeIt(() => CustomProgressBarCheck.CustomText = checkDetail);
                        this.InvokeIt(() => CustomProgressBarCheck.Value = n);

                        var parallelLoopResult = Parallel.For(0, list.Count, async i =>
                        {
                            string dns = list[i].Trim();
                            await checkOne(dns, -1);
                        });

                        await Task.Run(async () =>
                        {
                            while (!parallelLoopResult.IsCompleted)
                            {
                                await Task.Delay(10);
                                if (parallelLoopResult.IsCompleted) break;
                            }
                        });

                        // Percentage (100%)
                        if (n == lists.Count - 1)
                        {
                            checkDetail = $"{dnsCount} of {dnsCount}";
                            this.InvokeIt(() => CustomProgressBarCheck.CustomText = checkDetail);
                            this.InvokeIt(() => CustomProgressBarCheck.Value = lists.Count);
                        }
                    }
                });
            }

            async Task checkOne(string dns, int lineNumber)
            {
                if (!string.IsNullOrEmpty(dns) && !string.IsNullOrWhiteSpace(dns))
                {
                    // All servers ++
                    numberOfAllServers++;

                    if (IsDnsProtocolSupported(dns))
                    {
                        // All supported servers ++
                        numberOfAllSupportedServers++;
                        
                        // Get DNS Details
                        DnsReader dnsReader = new(dns, companyDataContent);
                        
                        // Get Unknown Companies (Debug Only)
                        //if (dnsReader.CompanyName.ToLower().Contains("couldn't retrieve"))
                        //{
                        //    string p = @"C:\Users\msasa\OneDrive\Desktop\getInfo.txt";
                        //    FileDirectory.CreateEmptyFile(p);
                        //    string serv = dnsReader.Host;
                        //    if (string.IsNullOrEmpty(serv) && !string.IsNullOrEmpty(dnsReader.IP)) serv = dnsReader.IP;
                        //    FileDirectory.AppendTextLine(p, serv, new UTF8Encoding(false));
                        //}
                        //if (dnsReader.CompanyName.ToLower().Contains("china"))
                        //    Debug.WriteLine("CHINA: " + dns);

                        // Apply Protocol Selection
                        bool matchRules = CheckDnsMatchRules(dnsReader);
                        if (!matchRules) return;
                        
                        // Get Check timeout value
                        decimal timeoutSec = 1;
                        this.InvokeIt(() =>timeoutSec = CustomNumericUpDownSettingCheckTimeout.Value);
                        int timeoutMS = decimal.ToInt32(timeoutSec * 1000);

                        // Is this being called by parallel?
                        bool isParallel = lineNumber == -1;

                        if (checkDns.CheckForFilters)
                        {
                            // Generate Google Force Safe Search IPs
                            checkDns.GenerateGoogleSafeSearchIps(dns);

                            // Generate Adult IPs
                            checkDns.GenerateAdultDomainIps(dns);
                        }
                        
                        if (insecure)
                            await checkDns.CheckDnsAsync(true, blockedDomainNoWww, dns, timeoutMS, localPort, bootstrap, bootstrapPort);
                        else
                        {
                            if (isParallel)
                                checkDns.CheckDNS(blockedDomainNoWww, dns, timeoutMS);
                            else
                                await checkDns.CheckDnsAsync(false, blockedDomainNoWww, dns, timeoutMS, localPort, bootstrap, bootstrapPort);
                        }
                        
                        // Get Status and Latency
                        bool dnsOK = checkDns.IsDnsOnline;
                        int latency = checkDns.DnsLatency;

                        if (checkDns.IsSafeSearch || checkDns.IsAdultFilter)
                        {
                            Debug.WriteLine("==== Debug ====");
                            Debug.WriteLine(dns);
                            Debug.WriteLine("IsSafeSearch: " + checkDns.IsSafeSearch);
                            Debug.WriteLine("IsAdultFilter: " + checkDns.IsAdultFilter);
                        }
                        
                        // Add working DNS to list
                        if (dnsOK)
                        {
                            if (!checkDns.CheckForFilters ||
                                (checkDns.CheckForFilters && !checkDns.IsSafeSearch && !checkDns.IsAdultFilter))
                            {
                                WorkingDnsList.Add(new Tuple<long, string>(latency, dns));

                                // Add working DNS to list to export
                                if (!builtInMode)
                                    WorkingDnsAndLatencyListToFile.Add(new Tuple<long, string>(latency, dns));
                            }
                            else
                            {
                                // Has Filter
                                return;
                            }
                        }

                        // Checked servers ++
                        numberOfCheckedServers++;

                        if (checkInParallel)
                        {
                            // Write short status to log
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
                                if (dnsReader.StampProperties.IsDnsSec)
                                    sdnsHostMsg2 += $", DNSSec";
                                if (dnsReader.StampProperties.IsNoLog)
                                    sdnsHostMsg2 += $", No Log";
                                if (dnsReader.StampProperties.IsNoFilter)
                                    sdnsHostMsg2 += $", No Filter";

                                host = sdnsHostMsg1 + sdnsHostMsg2 + sdnsHostMsg3;
                            }
                            else
                            {
                                // Write host to log
                                if (!builtInMode)
                                {
                                    host = $" [Host: {dns}]";
                                }

                                // Write protocol name to log
                                protocol = $" [Protocol: {dnsReader.ProtocolName}]";
                            }

                            // Write company name to log
                            string resultCompany = $" [{dnsReader.CompanyName}]";

                            string msgShort = $"[{status}] [{latency}]{host}{protocol}{resultCompany}{NL}";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgShort, color));
                        }
                        else
                            writeStatusToLog();

                        void writeStatusToLog()
                        {
                            // Write line number to log
                            if (lineNumber != -1)
                            {
                                string ln = $"{lineNumber}. ";
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(ln, Color.LightGray));
                            }

                            // Write status to log
                            string status = dnsOK ? "OK" : "Failed";
                            Color color = dnsOK ? Color.MediumSeaGreen : Color.IndianRed;
                            string resultStatus = $"[{status}]";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultStatus, color));

                            // Write latency to log
                            string resultLatency = $" [{latency}]";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultLatency, Color.DodgerBlue));

                            // Write host to log
                            if (dnsReader.IsDnsCryptStamp)
                            {
                                string sdnsHostMsg1 = $" [SDNS: ";
                                string sdnsHostMsg2 = $"{dnsReader.ProtocolName}";
                                string sdnsHostMsg3 = "]";
                                if (dnsReader.StampProperties.IsDnsSec)
                                    sdnsHostMsg2 += $", DNSSec";
                                if (dnsReader.StampProperties.IsNoLog)
                                    sdnsHostMsg2 += $", No Log";
                                if (dnsReader.StampProperties.IsNoFilter)
                                    sdnsHostMsg2 += $", No Filter";

                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(sdnsHostMsg1, Color.LightGray));
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(sdnsHostMsg2, Color.Orange));
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(sdnsHostMsg3, Color.LightGray));
                            }
                            else
                            {
                                // Write host to log
                                if (!builtInMode)
                                {
                                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(" [Host: ", Color.LightGray));
                                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(dns, Color.DodgerBlue));
                                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText("]", Color.LightGray));
                                }

                                // Write protocol name to log
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(" [Protocol: ", Color.LightGray));
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(dnsReader.ProtocolName, Color.Orange));
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText("]", Color.LightGray));
                            }

                            // Write company name to log
                            string resultCompany = $" [{dnsReader.CompanyName}]";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultCompany + NL, Color.Gray));
                        }
                    }
                }

            }

            // Return if there is no working server
            if (!WorkingDnsList.Any())
            {
                string noWorkingServer = NL + "There is no working server." + NL;
                if (StopChecking || IsDisconnecting) noWorkingServer = NL + "Task Canceled." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(noWorkingServer, Color.IndianRed));
                return false;
            }

            // Sort by latency comment
            string allWorkingServers = NL + "Working servers sorted by latency:" + NL + NL;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(allWorkingServers, Color.MediumSeaGreen));

            // Sort by latency
            if (WorkingDnsList.Count > 1)
                WorkingDnsList = WorkingDnsList.OrderByDescending(t => t.Item1).ToList();

            // All Latencies (to get average delay)
            long allLatencies = 0;
            bool once = true;
            string fastest = $"Fastest Servers:{NL}";

            for (int n = 0; n < WorkingDnsList.Count; n++)
            {
                var latencyHost = WorkingDnsList[n];
                long latency = latencyHost.Item1;
                string host = latencyHost.Item2;
                string dns = host;

                allLatencies += latency;

                // Write slowest server to log
                if (n == 0 && WorkingDnsList.Count >= 2)
                {
                    string slowest = $"Slowest Server:{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(slowest, Color.MediumSeaGreen));

                    // Get DNS Details
                    DnsReader dnsReader = new(dns, companyDataContent);

                    // write sorted result to log
                    writeSortedStatusToLog(dnsReader);
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(NL));
                }
                else if (WorkingDnsList.Count <= 11)
                {
                    if (once)
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(fastest, Color.MediumSeaGreen));
                    once = false;

                    // Get DNS Details
                    DnsReader dnsReader = new(dns, companyDataContent);

                    // write sorted result to log
                    writeSortedStatusToLog(dnsReader);
                }
                else if (WorkingDnsList.Count - n <= 10)
                {
                    if (once)
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(fastest, Color.MediumSeaGreen));
                    once = false;

                    // Get DNS Details
                    DnsReader dnsReader = new(dns, companyDataContent);

                    // write sorted result to log
                    writeSortedStatusToLog(dnsReader);
                }
                
                void writeSortedStatusToLog(DnsReader dnsReader)
                {
                    // Write latency to log
                    string resultLatency1 = "[Latency:";
                    string resultLatency2 = $" {latency}";
                    string resultLatency3 = " ms]";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultLatency1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultLatency2, Color.DodgerBlue));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultLatency3, Color.LightGray));

                    // Write host to log
                    if (dnsReader.IsDnsCryptStamp)
                    {
                        string sdnsHostMsg1 = $" [SDNS: ";
                        string sdnsHostMsg2 = $"{dnsReader.ProtocolName}";
                        string sdnsHostMsg3 = "]";
                        if (dnsReader.StampProperties.IsDnsSec)
                            sdnsHostMsg2 += $", DNSSec";
                        if (dnsReader.StampProperties.IsNoLog)
                            sdnsHostMsg2 += $", No Log";
                        if (dnsReader.StampProperties.IsNoFilter)
                            sdnsHostMsg2 += $", No Filter";

                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(sdnsHostMsg1, Color.LightGray));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(sdnsHostMsg2, Color.Orange));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(sdnsHostMsg3, Color.LightGray));
                    }
                    else
                    {
                        // Write host to log
                        if (!builtInMode)
                        {
                            string resultHost1 = " [Host:";
                            string resultHost2 = $" {host}";
                            string resultHost3 = "]";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultHost1, Color.LightGray));
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultHost2, Color.DodgerBlue));
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultHost3, Color.LightGray));
                        }

                        // Write protocol to log
                        string resultProtocol1 = " [Protocol:";
                        string resultProtocol2 = $" {dnsReader.ProtocolName}";
                        string resultProtocol3 = "]";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultProtocol1, Color.LightGray));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultProtocol2, Color.Orange));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultProtocol3, Color.LightGray));
                    }

                    // Write company name to log
                    string resultCompany1 = " [Company:";
                    string resultCompany2 = $" {dnsReader.CompanyName}";
                    string resultCompany3 = "]";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultCompany1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultCompany2, Color.DodgerBlue));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(resultCompany3, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(NL));
                }
            }

            // Write Count to log
            string msgCount1 = "Found "; // workingServers
            string msgCount2 = " working server"; if (WorkingDnsList.Count > 1) msgCount2 += "s";
            string msgCount3 = " out of "; // numberOfAllSupportedServers
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCount1, Color.LightGray));
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(WorkingDnsList.Count.ToString(), Color.MediumSeaGreen));
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCount2, Color.LightGray));
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCount3, Color.LightGray));
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(numberOfAllSupportedServers.ToString() + NL, Color.MediumSeaGreen));

            // Number of failed servers
            int numberOfFailedServers = numberOfCheckedServers - WorkingDnsList.Count;
            if (numberOfFailedServers > 0)
            {
                string msgFailed = "Failed servers: ";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgFailed, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(numberOfFailedServers.ToString() + NL, Color.IndianRed));
            }

            // Ignored by settings
            int numberOfIgnoredBySettings = numberOfAllSupportedServers - numberOfCheckedServers;
            if (numberOfIgnoredBySettings > 0)
            {
                string msgIgnoredBySettings = "Ignored by settings: ";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgIgnoredBySettings, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(numberOfIgnoredBySettings.ToString() + NL, Color.MediumSeaGreen));
            }

            // Ignored by not supported protocol
            int numberOfNotSupported = numberOfAllServers - numberOfAllSupportedServers;
            if (numberOfNotSupported > 0)
            {
                string msgIgnoredByWrongProtocol = "Not supported protocol: ";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgIgnoredByWrongProtocol, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(numberOfNotSupported.ToString() + NL, Color.MediumSeaGreen));
            }

            // Wite average delay to log
            if (WorkingDnsList.Count > 1)
            {
                string msgAL1 = "Average Latency: ";
                string msgAL2 = " ms.";
                long averageLatency = allLatencies / WorkingDnsList.Count;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgAL1, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"{averageLatency}", Color.DodgerBlue));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgAL2 + NL, Color.LightGray));
            }

            return true;
        }
    }
}
