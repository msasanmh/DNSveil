using System;
using System.Net;
using CustomControls;
using MsmhTools;
using MsmhTools.DnsTool;
using System.Diagnostics;
using System.Reflection;

namespace SecureDNSClient
{
    public partial class FormMain
    {
        private async void StartCheck()
        {
            // Return if binary files are missing
            if (!CheckNecessaryFiles()) return;

            if (!IsCheckingStarted)
            {
                // Start Checking
                IsCheckingStarted = true;
                IsCheckDone = false;

                // Check Internet Connectivity
                if (!IsInternetAlive()) return;

                // Unset DNS if it's not connected before checking.
                if (!IsConnected)
                {
                    if (IsDNSSet)
                        SetDNS(); // Unset DNS
                    else
                        await UnsetSavedDNS();
                }

                try
                {
                    Task task = Task.Run(async () => await CheckServers());

                    await task.ContinueWith(_ =>
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
                        IsCheckDone = true;

                        string msg = NL + "Check operation finished." + NL;
                        CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue);
                        CustomButtonCheck.Enabled = true;

                        // Go to Connect Tab if it's not already connected
                        if (ConnectAllClicked && !IsConnected && NumberOfWorkingServers > 0)
                        {
                            this.InvokeIt(() => CustomTabControlMain.SelectedIndex = 0);
                            this.InvokeIt(() => CustomTabControlSecureDNS.SelectedIndex = 1);
                        }
                        Debug.WriteLine("Checking Task: " + task.Status);
                        StopChecking = false;
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private async Task CheckServers()
        {
            // Get blocked domain
            string blockedDomain = GetBlockedDomainSetting(out string blockedDomainNoWww);
            if (string.IsNullOrEmpty(blockedDomain)) return;

            // Warn users to deactivate DPI before checking servers
            if (IsGoodbyeDPIActive || (IsProxySet && IsProxyDPIActive))
            {
                string msg = "It's better to not check servers while DPI Bypass is active.\nStart checking servers?";
                var resume = CustomMessageBox.Show(msg, "DPI is active", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (resume == DialogResult.No) return;
            }

            // Warn users to Unset DNS before checking servers
            if (IsDNSSet)
            {
                string msg = "It's better to not check servers while DNS is set.\nStart checking servers?";
                var resume = CustomMessageBox.Show(msg, "DNS is set", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (resume == DialogResult.No) return;
            }

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
                bool isPortOpen = Network.IsPortOpen(IPAddress.Loopback.ToString(), localPort, 3);
                if (isPortOpen)
                {
                    localPort = Network.GetNextPort(localPort);
                    isPortOpen = Network.IsPortOpen(IPAddress.Loopback.ToString(), localPort, 3);
                    if (isPortOpen)
                    {
                        localPort = Network.GetNextPort(localPort);
                        bool isPortOk = GetListeningPort(localPort, "You need to resolve the conflict.", Color.IndianRed);
                        if (!isPortOk) return;
                    }
                }
            }
            
            // Built-in or Custom
            bool builtInMode = CustomRadioButtonBuiltIn.Checked;

            // Clear working list to file on new check
            WorkingDnsListToFile.Clear();
            WorkingDnsAndLatencyListToFile.Clear();

            string? fileContent = string.Empty;
            if (builtInMode)
                fileContent = await Resource.GetResourceTextFileAsync("SecureDNSClient.DNS-Servers.txt", Assembly.GetExecutingAssembly());
            else
            {
                FileDirectory.CreateEmptyFile(SecureDNS.CustomServersPath);
                fileContent = await File.ReadAllTextAsync(SecureDNS.CustomServersPath);

                // Load saved working servers
                WorkingDnsListToFile.LoadFromFile(SecureDNS.WorkingServersPath, true, true);
            }

            // Check if servers exist 1
            if (string.IsNullOrEmpty(fileContent) || string.IsNullOrWhiteSpace(fileContent))
            {
                string msg = "Servers list is empty." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return;
            }

            // Add Servers to list
            List<string> dnsList = fileContent.SplitToLines();
            int dnsCount = dnsList.Count;

            // Check if servers exist 2
            if (dnsCount < 1)
            {
                string msg = "Servers list is empty." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return;
            }

            // init Check DNS
            CheckDns checkDns = new(false, GetCPUPriority());

            // FlushDNS(); // Flush DNS makes first line always return failed

            // Clear temp working list on new check
            WorkingDnsList.Clear();

            // Get Company Data Content
            string? companyDataContent = await Resource.GetResourceTextFileAsync("SecureDNSClient.HostToCompany.txt", Assembly.GetExecutingAssembly()); // Load from Embedded Resource

            // Set number of servers
            int numberOfAllServers = 0;
            int numberOfAllSupportedServers = 0;
            int numberOfCheckedServers = 0;

            // Dummy check to fix first run
            checkDns.CheckDNS(blockedDomainNoWww, dnsList[0], 100);
            checkDns.CheckDNS(blockedDomainNoWww, dnsList[0], 100);
            checkDns.CheckDNS(blockedDomainNoWww, dnsList[0], 100);

            if (insecure)
                checkSeries();
            else
            {
                if (checkInParallel)
                    await checkParallel();
                else
                    checkSeries();
            }

            void checkSeries()
            {
                this.InvokeIt(() => CustomProgressBarCheck.StopTimer = false);
                this.InvokeIt(() => CustomProgressBarCheck.ChunksColor = Color.DodgerBlue);
                for (int n = 0; n < dnsCount; n++)
                {
                    if (StopChecking)
                    {
                        string msg = NL + "Canceling Check operation..." + NL;
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));

                        break;
                    }

                    // Percentage
                    int persent = n * 100 / dnsCount;
                    string checkDetail = $"{n + 1} of {dnsCount}";
                    this.InvokeIt(() => CustomProgressBarCheck.CustomText = checkDetail);
                    this.InvokeIt(() => CustomProgressBarCheck.Value = persent);

                    string dns = dnsList[n].Trim();
                    checkOne(dns, n + 1);

                    // Percentage (100%)
                    if (n == dnsCount - 1)
                    {
                        checkDetail = $"{n + 1} of {dnsCount}";
                        this.InvokeIt(() => CustomProgressBarCheck.CustomText = checkDetail);
                        this.InvokeIt(() => CustomProgressBarCheck.Value = 100);
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
                        int persent = n * 100 / lists.Count;
                        count += list.Count;
                        string checkDetail = $"{count} of {dnsCount}";
                        this.InvokeIt(() => CustomProgressBarCheck.CustomText = checkDetail);
                        this.InvokeIt(() => CustomProgressBarCheck.Value = persent);

                        var parallelLoopResult = Parallel.For(0, list.Count, i =>
                        {
                            string dns = list[i].Trim();
                            checkOne(dns, -1);
                        });

                        await Task.Run(async () =>
                        {
                            while (!parallelLoopResult.IsCompleted)
                            {
                                await Task.Delay(10);
                                if (parallelLoopResult.IsCompleted)
                                    break;
                            }
                        });

                        // Percentage (100%)
                        if (n == lists.Count - 1)
                        {
                            checkDetail = $"{dnsCount} of {dnsCount}";
                            this.InvokeIt(() => CustomProgressBarCheck.CustomText = checkDetail);
                            this.InvokeIt(() => CustomProgressBarCheck.Value = 100);
                        }
                    }
                });
            }

            void checkOne(string dns, int lineNumber)
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
                            checkDns.CheckDNS(true, blockedDomainNoWww, dns, timeoutMS, localPort, bootstrap, bootstrapPort);
                        else
                        {
                            if (isParallel)
                                checkDns.CheckDNS(blockedDomainNoWww, dns, timeoutMS);
                            else
                                checkDns.CheckDNS(false, blockedDomainNoWww, dns, timeoutMS, localPort, bootstrap, bootstrapPort);
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
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(noWorkingServer, Color.IndianRed));
                return;
            }

            //if (StopChecking) return;

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

        }
    }
}
