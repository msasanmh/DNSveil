using MsmhTools;
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
        private List<string> SafeSearchIpList { get; set; } = new();
        private List<string> AdultIpList { get; set; } = new();

        private static ProcessPriorityClass ProcessPriority;
        private string AdultDomainToCheck { get; set; } = "pornhub.com";

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

        /// <summary>
        /// Check DNS and get latency (ms)
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="dnsServer"></param>
        /// <param name="timeoutMS"></param>
        /// <returns>Returns -1 if DNS fail</returns>
        public void CheckDNS(string domain, string dnsServer, int timeoutMS)
        {
            IsDnsOnline = false;
            Stopwatch stopwatch = new();
            stopwatch.Start();
            IsDnsOnline = CheckDnsWork(domain, dnsServer, timeoutMS, ProcessPriority);
            stopwatch.Stop();

            DnsLatency = IsDnsOnline ? Convert.ToInt32(stopwatch.ElapsedMilliseconds) : -1;

            IsSafeSearch = false;
            IsAdultFilter = false;
            if (CheckForFilters && IsDnsOnline)
            {
                CheckDnsFilters(dnsServer, timeoutMS + 500, out bool isSafeSearch, out bool isAdultFilter);
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
        /// <returns>Returns -1 if DNS fail</returns>
        public void CheckDNS(bool insecure, string domain, string dnsServer, int timeoutMS, int localPort, string bootstrap, int bootsratPort)
        {
            IsDnsOnline = false;
            Stopwatch stopwatch = new();
            stopwatch.Start();
            IsDnsOnline = CheckDnsWork(insecure, domain, dnsServer, timeoutMS, localPort, bootstrap, bootsratPort, ProcessPriority);
            stopwatch.Stop();

            DnsLatency = IsDnsOnline ? Convert.ToInt32(stopwatch.ElapsedMilliseconds) : -1;

            IsSafeSearch = false;
            IsAdultFilter = false;
            if (CheckForFilters && IsDnsOnline)
            {
                CheckDnsFilters(dnsServer, timeoutMS + 500, out bool isSafeSearch, out bool isAdultFilter);
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
        /// <returns>Returns -1 if DNS fail</returns>
        public async Task CheckDnsAsync(string domain, string dnsServer, int timeoutMS)
        {
            IsDnsOnline = false;
            Stopwatch stopwatch = new();
            stopwatch.Start();
            IsDnsOnline = await CheckDnsWorkAsync(domain, dnsServer, timeoutMS, ProcessPriority);
            stopwatch.Stop();

            DnsLatency = IsDnsOnline ? Convert.ToInt32(stopwatch.ElapsedMilliseconds) : -1;

            IsSafeSearch = false;
            IsAdultFilter = false;
            if (CheckForFilters && IsDnsOnline)
            {
                CheckDnsFilters(dnsServer, timeoutMS + 500, out bool isSafeSearch, out bool isAdultFilter);
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
        /// <returns>Returns -1 if DNS fail</returns>
        public async Task CheckDnsAsync(bool insecure, string domain, string dnsServer, int timeoutMS, int localPort, string bootstrap, int bootsratPort)
        {
            IsDnsOnline = false;
            Stopwatch stopwatch = new();
            stopwatch.Start();
            IsDnsOnline = await CheckDnsWorkAsync(insecure, domain, dnsServer, timeoutMS, localPort, bootstrap, bootsratPort, ProcessPriority);
            stopwatch.Stop();

            DnsLatency = IsDnsOnline ? Convert.ToInt32(stopwatch.ElapsedMilliseconds) : -1;

            IsSafeSearch = false;
            IsAdultFilter = false;
            if (CheckForFilters && IsDnsOnline)
            {
                CheckDnsFilters(dnsServer, timeoutMS + 500, out bool isSafeSearch, out bool isAdultFilter);
                IsSafeSearch = isSafeSearch;
                IsAdultFilter = isAdultFilter;
            }
        }

        public void GenerateGoogleSafeSearchIps(string dnsServer)
        {
            if (!SafeSearchIpList.Any())
            {
                string websiteSS = "forcesafesearch.google.com";
                string argsSS = $"{websiteSS} {dnsServer}";
                string jsonStrSS = GetDnsLookupJson(argsSS).Result;
                SafeSearchIpList = GetDnsLookupARecordList(jsonStrSS);
                Debug.WriteLine("Safe Search IPs Generated, Count: " + SafeSearchIpList.Count);
            }
        }

        public void GenerateAdultDomainIps(string dnsServer)
        {
            if (!AdultIpList.Any())
            {
                string argsAD = $"{AdultDomainToCheck} {dnsServer}";
                string status = GetDnsLookupJson(argsAD, true, false).Result; // RRTYPE: TXT, Format: Text
                if (status.Contains("status: NOERROR") && status.Contains("verification"))
                {
                    string jsonStrAD = GetDnsLookupJson(argsAD).Result; // RRTYPE: A, Format: Json
                    AdultIpList = GetDnsLookupARecordList(jsonStrAD);
                    if (AdultIpList.Contains("0.0.0.0"))
                        AdultIpList.Clear();
                    else
                        Debug.WriteLine("Adult IPs Generated, Count: " + AdultIpList.Count);
                }
            }
        }

        private void CheckDnsFilters(string dnsServer, int timeoutMS, out bool isSafeSearch, out bool isAdultFilter)
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
                    SafeSearchIpList = GetDnsLookupARecordList(jsonStrSS);
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
                    string status = GetDnsLookupJson(args, true, false).Result; // RRTYPE: TXT, Format: Text
                    if ((status.Contains("status:") && !status.Contains("status: NOERROR")) ||
                        (status.Contains("status: NOERROR") && !status.Contains("verification")))
                        isAdultFilterOut = true;
                }
            });

            task.Wait(timeoutMS);
            isSafeSearch = isSafeSearchOut;
            isAdultFilter = isAdultFilterOut;
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
                catch (Exception ex)
                {
                    Debug.WriteLine("GetDnsLookupARecord: " + ex.Message);
                }
            }
            return aRecStr;
        }

        private static List<string> GetDnsLookupARecordList(string jsonStr)
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
                        JsonElement.ArrayEnumerator answerEArray = answerE.EnumerateArray();
                        for (int n2 = 0; n2 < answerEArray.Count(); n2++)
                        {
                            JsonElement answerEV = answerEArray.ToArray()[n2];
                            bool hasARec = answerEV.TryGetProperty("A", out JsonElement aRec);
                            if (hasARec)
                            {
                                string? aRecStrOut = aRec.GetString();
                                if (!string.IsNullOrEmpty(aRecStrOut))
                                    aRecStr.Add(aRecStrOut.Trim());
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

        private static async Task<string> GetDnsLookupJson(string dnsLookupArgs, bool rrtypeTXT = false, bool json = true)
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
                if (rrtypeTXT)
                    process.StartInfo.EnvironmentVariables["RRTYPE"] = "TXT";
                else
                    process.StartInfo.EnvironmentVariables["RRTYPE"] = "A";

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
            Task<bool> task = Task.Run(async () =>
            {
                // Start local server
                string dnsProxyArgs = $"-l {IPAddress.Loopback} -p {localPort} ";
                if (insecure) dnsProxyArgs += "--insecure ";
                dnsProxyArgs += $"-u {dnsServer} -b {bootstrap}:{bootsratPort}";
                int localServerPID = ProcessManager.ExecuteOnly(out Process _, SecureDNS.DnsProxy, dnsProxyArgs, true, false, SecureDNS.CurrentPath, processPriorityClass);

                // Wait for DNSProxy
                Task wait1 = Task.Run(() =>
                {
                    while (!ProcessManager.FindProcessByPID(localServerPID))
                    {
                        if (ProcessManager.FindProcessByPID(localServerPID))
                            break;
                    }
                    return Task.CompletedTask;
                });
                await wait1.WaitAsync(TimeSpan.FromMilliseconds(timeoutMS));

                string args = $"{domain} {IPAddress.Loopback}:{localPort}";
                string? result = ProcessManager.Execute(out Process _, SecureDNS.DnsLookup, args, true, false, SecureDNS.CurrentPath, processPriorityClass);

                if (!string.IsNullOrEmpty(result))
                {
                    ProcessManager.KillProcessByPID(localServerPID);

                    // Wait for DNSProxy to exit
                    Task wait2 = Task.Run(() =>
                    {
                        while (ProcessManager.FindProcessByPID(localServerPID))
                        {
                            if (!ProcessManager.FindProcessByPID(localServerPID))
                                break;
                        }
                        return Task.CompletedTask;
                    });
                    await wait2.WaitAsync(TimeSpan.FromMilliseconds(timeoutMS));

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

        private static async Task<bool> CheckDnsWorkAsync(string domain, string dnsServer, int timeoutMS, ProcessPriorityClass processPriorityClass)
        {
            try
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

                if (await task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMS)))
                    return task.Result;
                else
                    return false;
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

            // Wait for DNSProxy
            await Task.Run(() =>
            {
                while (!ProcessManager.FindProcessByPID(localServerPID))
                {
                    if (ProcessManager.FindProcessByPID(localServerPID))
                        break;
                }
            });

            string args = $"{domain} {IPAddress.Loopback}:{localPort}";
            string? result = ProcessManager.Execute(out Process _, SecureDNS.DnsLookup, args, true, false, SecureDNS.CurrentPath, processPriorityClass);

            if (!string.IsNullOrEmpty(result))
            {
                ProcessManager.KillProcessByPID(localServerPID);

                // Wait for DNSProxy to exit
                await Task.Run(() =>
                {
                    while (ProcessManager.FindProcessByPID(localServerPID))
                    {
                        if (!ProcessManager.FindProcessByPID(localServerPID))
                            break;
                    }
                });

                return result.Contains("ANSWER SECTION");
            }
            else
                return false;
        }
    }
}
