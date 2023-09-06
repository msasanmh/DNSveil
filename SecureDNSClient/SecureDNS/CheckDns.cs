using MsmhToolsClass;
using System;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace SecureDNSClient
{
    public class CheckDns
    {
        public bool IsDnsOnline { get; private set; } = false;

        /// <summary>
        /// Returns -1 if DNS fail
        /// </summary>
        public int DnsLatency { get; private set; } = -1;
        public bool IsSafeSearch { get; private set; } = false;
        public bool IsAdultFilter { get; private set; } = false;
        public bool CheckForFilters { get; private set; }
        public string AdultDomainToCheck { get; set; } = "pornhub.com";
        private List<string> SafeSearchIpList { get; set; } = new();
        private List<string> AdultIpList { get; set; } = new();
        private string DNS = string.Empty;

        private static ProcessPriorityClass ProcessPriority;

        /// <summary>
        /// Check DNS Servers
        /// </summary>
        /// <param name="checkForFilters">Check for Filters (Takes more time)</param>
        /// <param name="processPriorityClass">Process Priority</param>
        public CheckDns(bool checkForFilters, ProcessPriorityClass processPriorityClass)
        {
            ProcessPriority = processPriorityClass;
            CheckForFilters = checkForFilters;
        }

        //================================= Check DNS

        /// <summary>
        /// Check DNS and get latency (ms)
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="dnsServer"></param>
        /// <param name="timeoutMS"></param>
        public void CheckDNS(string domain, string dnsServer, int timeoutMS)
        {
            DNS = dnsServer;
            IsDnsOnline = false;
            Stopwatch stopwatch = new();
            stopwatch.Start();
            IsDnsOnline = CheckDnsWork(domain, DNS, timeoutMS, ProcessPriority);
            stopwatch.Stop();

            DnsLatency = IsDnsOnline ? Convert.ToInt32(stopwatch.ElapsedMilliseconds) : -1;

            IsSafeSearch = false;
            IsAdultFilter = false;
            if (CheckForFilters && IsDnsOnline)
            {
                CheckDnsFilters(DNS, out bool isSafeSearch, out bool isAdultFilter);
                IsSafeSearch = isSafeSearch;
                IsAdultFilter = isAdultFilter;
            }
        }

        /// <summary>
        /// Check DNS and get latency (ms)
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="dnsServer"></param>
        /// <param name="timeoutMS"></param>
        public void CheckDNS(bool insecure, string domain, string dnsServer, int timeoutMS, int localPort, string bootstrap, int bootsratPort)
        {
            DNS = dnsServer;
            IsDnsOnline = false;
            Stopwatch stopwatch = new();
            stopwatch.Start();
            IsDnsOnline = CheckDnsWork(insecure, domain, DNS, timeoutMS, localPort, bootstrap, bootsratPort, ProcessPriority);
            stopwatch.Stop();

            DnsLatency = IsDnsOnline ? Convert.ToInt32(stopwatch.ElapsedMilliseconds) : -1;

            IsSafeSearch = false;
            IsAdultFilter = false;
            if (CheckForFilters && IsDnsOnline)
            {
                CheckDnsFilters(DNS, out bool isSafeSearch, out bool isAdultFilter);
                IsSafeSearch = isSafeSearch;
                IsAdultFilter = isAdultFilter;
            }
        }

        private static bool CheckDnsWork(string domain, string dnsServer, int timeoutMS, ProcessPriorityClass processPriorityClass)
        {
            var task = Task.Run(() =>
            {
                string args = domain + " " + dnsServer;
                string? result = ProcessManager.Execute(out Process _, SecureDNS.DnsLookup, args, true, false, SecureDNS.CurrentPath, processPriorityClass);

                if (!string.IsNullOrEmpty(result))
                {
                    return result.Contains("ANSWER SECTION");
                }
                else
                    return false;
            });

            if (task.Wait(TimeSpan.FromMilliseconds(timeoutMS)))
                return task.Result;
            else
                return false;
        }

        private static bool CheckDnsWork(bool insecure, string domain, string dnsServer, int timeoutMS, int localPort, string bootstrap, int bootsratPort, ProcessPriorityClass processPriorityClass)
        {
            Task<bool> task = Task.Run(() =>
            {
                // Start local server
                string dnsProxyArgs = $"-l {IPAddress.Loopback} -p {localPort} ";
                if (insecure) dnsProxyArgs += "--insecure ";
                dnsProxyArgs += $"-u {dnsServer} -b {bootstrap}:{bootsratPort}";
                int localServerPID = ProcessManager.ExecuteOnly(out Process _, SecureDNS.DnsProxy, dnsProxyArgs, true, false, SecureDNS.CurrentPath, processPriorityClass);

                // Wait for DNSProxy
                SpinWait.SpinUntil(() => ProcessManager.FindProcessByPID(localServerPID), timeoutMS + 500);

                bool isOK = CheckDnsWork(domain, $"{IPAddress.Loopback}:{localPort}", timeoutMS, processPriorityClass);

                ProcessManager.KillProcessByPID(localServerPID);

                // Wait for DNSProxy to exit
                SpinWait.SpinUntil(() => !ProcessManager.FindProcessByPID(localServerPID), timeoutMS + 1000);

                return isOK;
            });

            if (task.Wait(TimeSpan.FromMilliseconds(timeoutMS)))
                return task.Result;
            else
                return false;
        }

        //================================= Check DNS Async

        /// <summary>
        /// Check DNS and get latency (ms)
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="dnsServer"></param>
        /// <param name="timeoutMS"></param>
        /// <param name="processPriorityClass"></param>
        public async Task CheckDnsAsync(string domain, string dnsServer, int timeoutMS)
        {
            DNS = dnsServer;
            IsDnsOnline = false;
            Stopwatch stopwatch = new();
            stopwatch.Start();
            IsDnsOnline = await CheckDnsWorkAsync(domain, DNS, timeoutMS, ProcessPriority);
            stopwatch.Stop();

            DnsLatency = IsDnsOnline ? Convert.ToInt32(stopwatch.ElapsedMilliseconds) : -1;

            IsSafeSearch = false;
            IsAdultFilter = false;
            if (CheckForFilters && IsDnsOnline)
            {
                CheckDnsFilters(DNS, out bool isSafeSearch, out bool isAdultFilter);
                IsSafeSearch = isSafeSearch;
                IsAdultFilter = isAdultFilter;
            }
        }

        /// <summary>
        /// Check DNS and get latency (ms)
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="dnsServer"></param>
        /// <param name="timeoutMS"></param>
        /// <param name="processPriorityClass"></param>
        public async Task CheckDnsAsync(bool insecure, string domain, string dnsServer, int timeoutMS, int localPort, string bootstrap, int bootsratPort)
        {
            DNS = dnsServer;
            IsDnsOnline = false;
            Stopwatch stopwatch = new();
            stopwatch.Start();
            IsDnsOnline = await CheckDnsWorkAsync(insecure, domain, DNS, timeoutMS, localPort, bootstrap, bootsratPort, ProcessPriority);
            stopwatch.Stop();
            
            DnsLatency = IsDnsOnline ? Convert.ToInt32(stopwatch.ElapsedMilliseconds) : -1;

            IsSafeSearch = false;
            IsAdultFilter = false;
            if (CheckForFilters && IsDnsOnline)
            {
                CheckDnsFilters(DNS, out bool isSafeSearch, out bool isAdultFilter);
                IsSafeSearch = isSafeSearch;
                IsAdultFilter = isAdultFilter;
            }
        }

        private static async Task<bool> CheckDnsWorkAsync(string domain, string dnsServer, int timeoutMS, ProcessPriorityClass processPriorityClass)
        {
            try
            {
                Task<bool> task = Task.Run(() =>
                {
                    string args = domain + " " + dnsServer;
                    string? result = ProcessManager.Execute(out Process _, SecureDNS.DnsLookup, args, true, false, SecureDNS.CurrentPath, processPriorityClass);
                    
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result.Contains("ANSWER SECTION");
                    }
                    else
                        return false;
                });

                return await task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMS));
            }
            catch (TimeoutException)
            {
                return false;
            }
        }

        private static async Task<bool> CheckDnsWorkAsync(bool insecure, string domain, string dnsServer, int timeoutMS, int localPort, string bootstrap, int bootsratPort, ProcessPriorityClass processPriorityClass)
        {
            // Start local server
            string dnsProxyArgs = $"-l {IPAddress.Loopback} -p {localPort} ";
            if (insecure) dnsProxyArgs += "--insecure ";
            dnsProxyArgs += $"-u {dnsServer} -b {bootstrap}:{bootsratPort}";
            int localServerPID = ProcessManager.ExecuteOnly(out Process _, SecureDNS.DnsProxy, dnsProxyArgs, true, false, SecureDNS.CurrentPath, processPriorityClass);

            //Wait for DNSProxy
            SpinWait.SpinUntil(() => ProcessManager.FindProcessByPID(localServerPID), timeoutMS + 500);

            bool isOK = await CheckDnsWorkAsync(domain, $"{IPAddress.Loopback}:{localPort}", timeoutMS, processPriorityClass);

            ProcessManager.KillProcessByPID(localServerPID);

            // Wait for DNSProxy to exit
            SpinWait.SpinUntil(() => !ProcessManager.FindProcessByPID(localServerPID), timeoutMS + 1000);

            return isOK;
        }

        //================================= Check Dns as SmartDns

        public bool CheckAsSmartDns(string uncensoredDns, string domain)
        {
            bool smart = false;
            string realArg = $"{domain} {uncensoredDns}";
            string realJson = GetDnsLookupJson(realArg).Result;
            List<string> realDomainIPs = GetDnsLookupAnswerRecordList(realJson);

            string arg = $"{domain} {DNS}";
            string json = GetDnsLookupJson(arg).Result;
            List<string> domainIPs = GetDnsLookupAnswerRecordList(json);

            string jsonIpv6 = GetDnsLookupJson(arg, "AAAA").Result;
            List<string> domainIPv6s = GetDnsLookupAnswerRecordList(jsonIpv6, "AAAA");

            bool isIpMatch = false, isChecked = false;
            for (int a = 0; a < domainIPs.Count; a++)
            {
                string domainIP = domainIPs[a];

                if (!NetworkTool.IsLocalIP(domainIP))
                {
                    for (int b = 0; b < realDomainIPs.Count; b++)
                    {
                        string realDomainIP = realDomainIPs[b];
                        if (domainIP.Equals(realDomainIP)) isIpMatch = true;
                        if (b == realDomainIPs.Count - 1) isChecked = true;
                    }
                }
            }

            bool realPseudo = HasExtraRecord(realJson);
            bool pseudo = HasExtraRecord(json);

            if (isChecked && !isIpMatch && !domainIPv6s.Any() && realPseudo != pseudo)
                smart = true;

            return smart;
        }

        //================================= Generate IPs

        public void GenerateGoogleSafeSearchIps(string uncensoredDns)
        {
            if (!SafeSearchIpList.Any())
            {
                string websiteSS = "forcesafesearch.google.com";
                string argsSS = $"{websiteSS} {uncensoredDns}";
                string jsonStrSS = GetDnsLookupJson(argsSS).Result;
                SafeSearchIpList = GetDnsLookupAnswerRecordList(jsonStrSS);
                Debug.WriteLine("Safe Search IPs Generated, Count: " + SafeSearchIpList.Count);
            }
        }

        public void GenerateAdultDomainIps(string uncensoredDns)
        {
            if (!AdultIpList.Any())
            {
                string argsAD = $"{AdultDomainToCheck} {uncensoredDns}";
                string status = GetDnsLookupJson(argsAD, "TXT", false).Result; // RRTYPE: TXT, Format: Text
                if (status.Contains("status: NOERROR") && status.Contains("verification"))
                {
                    string jsonStrAD = GetDnsLookupJson(argsAD).Result; // RRTYPE: A, Format: Json
                    AdultIpList = GetDnsLookupAnswerRecordList(jsonStrAD);
                    if (AdultIpList.IsContain("0.0.0.0"))
                        AdultIpList.Clear();
                    else
                        Debug.WriteLine("Adult IPs Generated, Count: " + AdultIpList.Count);
                }
            }
        }

        //================================= Check DNS Filters

        private void CheckDnsFilters(string dnsServer, out bool isSafeSearch, out bool isAdultFilter)
        {
            bool isSafeSearchOut = false;
            isSafeSearch = false;
            bool isAdultFilterOut = false;
            isAdultFilter = false;

            Task task = Task.Run(() =>
            {
                if (!SafeSearchIpList.Any())
                {
                    string websiteSS = "forcesafesearch.google.com";
                    string argsSS = $"{websiteSS} {dnsServer}";
                    string jsonStrSS = GetDnsLookupJson(argsSS).Result;
                    SafeSearchIpList = GetDnsLookupAnswerRecordList(jsonStrSS);
                }

                // Check Google Force Safe Search
                string args = $"google.com {dnsServer}";
                string jsonStr = GetDnsLookupJson(args).Result;
                string googleIp = GetDnsLookupARecord(jsonStr).Trim();
                for (int i = 0; i < SafeSearchIpList.Count; i++)
                {
                    string safeIp = SafeSearchIpList[i].Trim();
                    if (googleIp.Equals(safeIp))
                    {
                        isSafeSearchOut = true;
                        break;
                    }
                }

                // Check Adult Filter
                args = $"{AdultDomainToCheck} {dnsServer}";
                if (AdultIpList.Any())
                {
                    jsonStr = GetDnsLookupJson(args).Result; // RRTYPE: A, Format: Json
                    string adultIpNew = GetDnsLookupARecord(jsonStr).Trim();
                    int jj = 0;
                    for (int j = 0; j < AdultIpList.Count; j++)
                    {
                        string adultIp = AdultIpList[j].Trim();
                        if (adultIpNew.Equals(adultIp))
                        {
                            jj++; break;
                        }
                    }
                    if (jj == 0) isAdultFilterOut = true;
                }
                else
                {
                    string status = GetDnsLookupJson(args, "TXT", false).Result; // RRTYPE: TXT, Format: Text
                    if ((status.Contains("status:") && !status.Contains("status: NOERROR")) ||
                        (status.Contains("status: NOERROR") && !status.Contains("verification")))
                        isAdultFilterOut = true;
                }
            });

            try { task.Wait(); } catch (Exception) { }

            isSafeSearch = isSafeSearchOut;
            isAdultFilter = isAdultFilterOut;
        }

        private static async Task<string> GetDnsLookupJson(string dnsLookupArgs, string rrtype = "A", bool json = true)
        {
            string result = string.Empty;
            Task task = Task.Run(async () =>
            {
                using Process process = new();
                process.StartInfo.FileName = SecureDNS.DnsLookup;
                process.StartInfo.Arguments = dnsLookupArgs;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.WorkingDirectory = SecureDNS.CurrentPath;

                // Set RRTYPE
                process.StartInfo.EnvironmentVariables["RRTYPE"] = rrtype;

                // Set JSON Output
                if (json)
                    process.StartInfo.EnvironmentVariables["JSON"] = "1";
                else
                    process.StartInfo.EnvironmentVariables["JSON"] = "";

                process.Start();
                process.PriorityClass = ProcessPriority;
                // Faster than process.WaitForExit();
                await Task.Run(() => result = process.StandardOutput.ReadToEnd().ReplaceLineEndings(Environment.NewLine));
            });

            try
            {
                await task.WaitAsync(TimeSpan.FromSeconds(15));
            }
            catch (Exception)
            {
                // timeout. do nothing
            }

            return result;
        }

        private static string GetDnsLookupARecord(string jsonStr)
        {
            string aRecStr = string.Empty;
            if (!string.IsNullOrEmpty(jsonStr))
            {
                try
                {
                    JsonDocumentOptions jsonDocumentOptions = new();
                    jsonDocumentOptions.AllowTrailingCommas = true;
                    JsonDocument jsonDocument = JsonDocument.Parse(jsonStr, jsonDocumentOptions);
                    JsonElement json = jsonDocument.RootElement;

                    bool hasAnswer = json.TryGetProperty("Answer", out JsonElement answerE);
                    if (hasAnswer)
                    {
                        if (answerE.ValueKind == JsonValueKind.Array)
                        {
                            JsonElement.ArrayEnumerator answerEArray = answerE.EnumerateArray();
                            for (int n2 = 0; n2 < answerEArray.Count(); n2++)
                            {
                                JsonElement answerEV = answerEArray.ToArray()[n2];
                                bool hasARec = answerEV.TryGetProperty("A", out JsonElement aRec);
                                if (hasARec)
                                {
                                    string? aRecStrOut = aRec.GetString();
                                    if (!string.IsNullOrEmpty(aRecStrOut))
                                        aRecStr = aRecStrOut.Trim();
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("GetDnsLookupARecord: " + ex.Message);
                }
            }
            return aRecStr;
        }

        private static List<string> GetDnsLookupAnswerRecordList(string jsonStr, string rrtype = "A")
        {
            List<string> aRecStr = new();
            if (!string.IsNullOrEmpty(jsonStr))
            {
                try
                {
                    JsonDocumentOptions jsonDocumentOptions = new();
                    jsonDocumentOptions.AllowTrailingCommas = true;
                    JsonDocument jsonDocument = JsonDocument.Parse(jsonStr, jsonDocumentOptions);
                    JsonElement json = jsonDocument.RootElement;

                    bool hasAnswer = json.TryGetProperty("Answer", out JsonElement answerE);
                    if (hasAnswer)
                    {
                        if (answerE.ValueKind == JsonValueKind.Array)
                        {
                            JsonElement.ArrayEnumerator answerEArray = answerE.EnumerateArray();
                            for (int n2 = 0; n2 < answerEArray.Count(); n2++)
                            {
                                JsonElement answerEV = answerEArray.ToArray()[n2];
                                bool hasARec = answerEV.TryGetProperty(rrtype, out JsonElement aRec);
                                if (hasARec)
                                {
                                    string? aRecStrOut = aRec.GetString();
                                    if (!string.IsNullOrEmpty(aRecStrOut))
                                        aRecStr.Add(aRecStrOut.Trim());
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("GetDnsLookupARecordList: " + ex.Message);
                }
            }
            return aRecStr;
        }

        private static bool HasExtraRecord(string jsonStr)
        {
            if (!string.IsNullOrEmpty(jsonStr))
            {
                try
                {
                    JsonDocumentOptions jsonDocumentOptions = new();
                    jsonDocumentOptions.AllowTrailingCommas = true;
                    JsonDocument jsonDocument = JsonDocument.Parse(jsonStr, jsonDocumentOptions);
                    JsonElement json = jsonDocument.RootElement;

                    bool hasExtra = json.TryGetProperty("Extra", out JsonElement extraE);
                    if (hasExtra)
                    {
                        if (extraE.ValueKind == JsonValueKind.Array)
                        {
                            JsonElement.ArrayEnumerator extraEArray = extraE.EnumerateArray();
                            return extraEArray.Any();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("GetDnsLookupARecord: " + ex.Message);
                }
            }
            return false;
        }

    }
}
