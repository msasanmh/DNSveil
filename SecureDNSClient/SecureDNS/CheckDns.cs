using MsmhToolsClass;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace SecureDNSClient;

public class CheckDns
{
    public bool IsDnsOnline { get; private set; } = false;

    /// <summary>
    /// Returns -1 if DNS fail
    /// </summary>
    public int DnsLatency { get; private set; } = -1;
    public bool IsGoogleSafeSearchEnabled { get; private set; } = false;
    public bool IsBingSafeSearchEnabled { get; private set; } = false;
    public bool IsYoutubeRestricted { get; private set; } = false;
    public bool IsAdultFilter { get; private set; } = false;
    public string AdultDomainToCheck { get; set; } = "pornhub.com";
    private List<string> GoogleSafeSearchIpList { get; set; } = new();
    private List<string> BingSafeSearchIpList { get; set; } = new();
    private List<string> YoutubeRestrictIpList { get; set; } = new();
    private List<string> AdultIpList { get; set; } = new();
    private string DNS = string.Empty;

    public bool Insecure { get; private set; } = false;
    public bool CheckForFilters { get; private set; } = false;
    private ProcessPriorityClass ProcessPriority { get; set; } = ProcessPriorityClass.Normal;
    private readonly int TimeoutMS = 10000;

    /// <summary>
    /// Check DNS Servers
    /// </summary>
    public CheckDns(bool insecure, bool checkForFilters, ProcessPriorityClass processPriorityClass)
    {
        Insecure = insecure;
        ProcessPriority = processPriorityClass;
        CheckForFilters = checkForFilters;
    }

    //================================= Check DNS

    /// <summary>
    /// Check DNS and get latency (ms)
    /// </summary>
    public void CheckDNS(string domain, string dnsServer, int timeoutMS)
    {
        DNS = dnsServer;
        IsDnsOnline = false;
        Stopwatch stopwatch = new();
        stopwatch.Start();
        IsDnsOnline = CheckDnsWork(domain, DNS, timeoutMS);
        stopwatch.Stop();

        DnsLatency = IsDnsOnline ? Convert.ToInt32(stopwatch.ElapsedMilliseconds) : -1;

        IsGoogleSafeSearchEnabled = false;
        IsBingSafeSearchEnabled = false;
        IsYoutubeRestricted = false;
        IsAdultFilter = false;
        if (CheckForFilters && IsDnsOnline)
        {
            CheckDnsFilters(DNS, out bool isGoogleSafeSearch, out bool isBingSafeSearch, out bool isYoutubeRestricted, out bool isAdultFilter);
            IsGoogleSafeSearchEnabled = isGoogleSafeSearch;
            IsBingSafeSearchEnabled = isBingSafeSearch;
            IsYoutubeRestricted = isYoutubeRestricted;
            IsAdultFilter = isAdultFilter;
        }
    }

    /// <summary>
    /// Check DNS and get latency (ms)
    /// </summary>
    public void CheckDNS(string domain, string dnsServer, int timeoutMS, int localPort, string bootstrap, int bootsratPort)
    {
        DNS = dnsServer;
        IsDnsOnline = false;
        Stopwatch stopwatch = new();
        stopwatch.Start();
        IsDnsOnline = CheckDnsWork(domain, DNS, timeoutMS, localPort, bootstrap, bootsratPort);
        stopwatch.Stop();

        DnsLatency = IsDnsOnline ? Convert.ToInt32(stopwatch.ElapsedMilliseconds) : -1;

        IsGoogleSafeSearchEnabled = false;
        IsBingSafeSearchEnabled = false;
        IsYoutubeRestricted = false;
        IsAdultFilter = false;
        if (CheckForFilters && IsDnsOnline)
        {
            CheckDnsFilters(DNS, out bool isGoogleSafeSearch, out bool isBingSafeSearch, out bool isYoutubeRestricted, out bool isAdultFilter);
            IsGoogleSafeSearchEnabled = isGoogleSafeSearch;
            IsBingSafeSearchEnabled = isBingSafeSearch;
            IsYoutubeRestricted = isYoutubeRestricted;
            IsAdultFilter = isAdultFilter;
        }
    }

    private bool CheckDnsWork(string domain, string dnsServer, int timeoutMS)
    {
        Process? process = null;
        Task<bool> task = Task.Run(async () =>
        {
            string result = await GetDnsLookupJsonAsync(domain, dnsServer, timeoutMS, "A", true);

            if (string.IsNullOrEmpty(result)) return false;
            if (string.IsNullOrWhiteSpace(result)) return false;
            if (result.Contains("[fatal]")) return false;

            List<string> ips = GetDnsLookupAnswerRecordList(result);
            return ips.Any() && !NetworkTool.IsLocalIP(ips[0].Trim());
        });

        if (task.Wait(TimeSpan.FromMilliseconds(timeoutMS)))
        {
            try { process?.Kill(true); } catch (Exception) { }
            return task.Result;
        }
        else
        {
            try { process?.Kill(true); } catch (Exception) { }
            return false;
        }
    }

    private bool CheckDnsWork(string domain, string dnsServer, int timeoutMS, int localPort, string bootstrap, int bootsratPort)
    {
        Task<bool> task = Task.Run(() =>
        {
            // Start local server
            string dnsProxyArgs = $"-l {IPAddress.Loopback} -p {localPort} ";
            if (Insecure) dnsProxyArgs += "--insecure ";
            dnsProxyArgs += $"-u {dnsServer} -b {bootstrap}:{bootsratPort}";
            int localServerPID = ProcessManager.ExecuteOnly(SecureDNS.DnsProxy, dnsProxyArgs, true, false, SecureDNS.CurrentPath, ProcessPriority);

            // Wait for DNSProxy
            SpinWait.SpinUntil(() => ProcessManager.FindProcessByPID(localServerPID), timeoutMS + 500);

            bool isOK = CheckDnsWork(domain, $"{IPAddress.Loopback}:{localPort}", timeoutMS);

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
    public async Task CheckDnsAsync(string domain, string dnsServer, int timeoutMS)
    {
        DNS = dnsServer;
        IsDnsOnline = false;
        Stopwatch stopwatch = new();
        stopwatch.Start();
        IsDnsOnline = await CheckDnsWorkAsync(domain, DNS, timeoutMS);
        stopwatch.Stop();

        DnsLatency = IsDnsOnline ? Convert.ToInt32(stopwatch.ElapsedMilliseconds) : -1;

        IsGoogleSafeSearchEnabled = false;
        IsBingSafeSearchEnabled = false;
        IsYoutubeRestricted = false;
        IsAdultFilter = false;
        if (CheckForFilters && IsDnsOnline)
        {
            CheckDnsFilters(DNS, out bool isGoogleSafeSearch, out bool isBingSafeSearch, out bool isYoutubeRestricted, out bool isAdultFilter);
            IsGoogleSafeSearchEnabled = isGoogleSafeSearch;
            IsBingSafeSearchEnabled = isBingSafeSearch;
            IsYoutubeRestricted = isYoutubeRestricted;
            IsAdultFilter = isAdultFilter;
        }
    }

    /// <summary>
    /// Check DNS and get latency (ms)
    /// </summary>
    public async Task CheckDnsAsync(string domain, string dnsServer, int timeoutMS, int localPort, string bootstrap, int bootsratPort)
    {
        DNS = dnsServer;
        IsDnsOnline = false;
        Stopwatch stopwatch = new();
        stopwatch.Start();
        IsDnsOnline = await CheckDnsWorkAsync(domain, DNS, timeoutMS, localPort, bootstrap, bootsratPort);
        stopwatch.Stop();

        DnsLatency = IsDnsOnline ? Convert.ToInt32(stopwatch.ElapsedMilliseconds) : -1;

        IsGoogleSafeSearchEnabled = false;
        IsBingSafeSearchEnabled = false;
        IsYoutubeRestricted = false;
        IsAdultFilter = false;
        if (CheckForFilters && IsDnsOnline)
        {
            CheckDnsFilters(DNS, out bool isGoogleSafeSearch, out bool isBingSafeSearch, out bool isYoutubeRestricted, out bool isAdultFilter);
            IsGoogleSafeSearchEnabled = isGoogleSafeSearch;
            IsBingSafeSearchEnabled = isBingSafeSearch;
            IsYoutubeRestricted = isYoutubeRestricted;
            IsAdultFilter = isAdultFilter;
        }
    }

    private async Task<bool> CheckDnsWorkAsync(string domain, string dnsServer, int timeoutMS)
    {
        string result = await GetDnsLookupJsonAsync(domain, dnsServer, timeoutMS, "A", true);

        if (string.IsNullOrEmpty(result)) return false;
        if (string.IsNullOrWhiteSpace(result)) return false;
        if (result.Contains("[fatal]")) return false;

        List<string> ips = GetDnsLookupAnswerRecordList(result);
        return ips.Any() && !NetworkTool.IsLocalIP(ips[0].Trim());
    }

    private async Task<bool> CheckDnsWorkAsync(string domain, string dnsServer, int timeoutMS, int localPort, string bootstrap, int bootsratPort)
    {
        // Start local server
        string dnsProxyArgs = $"-l {IPAddress.Loopback} -p {localPort} ";
        if (Insecure) dnsProxyArgs += "--insecure ";
        dnsProxyArgs += $"-u {dnsServer} -b {bootstrap}:{bootsratPort}";
        int localServerPID = ProcessManager.ExecuteOnly(SecureDNS.DnsProxy, dnsProxyArgs, true, false, SecureDNS.CurrentPath, ProcessPriority);

        // Wait for DNSProxy
        SpinWait.SpinUntil(() => ProcessManager.FindProcessByPID(localServerPID), timeoutMS + 500);

        bool isOK = await CheckDnsWorkAsync(domain, $"{IPAddress.Loopback}:{localPort}", timeoutMS);

        ProcessManager.KillProcessByPID(localServerPID);

        // Wait for DNSProxy to exit
        SpinWait.SpinUntil(() => !ProcessManager.FindProcessByPID(localServerPID), timeoutMS + 1000);

        return isOK;
    }

    //================================= Check Dns as SmartDns

    public async Task<bool> CheckAsSmartDns(string uncensoredDns, string domain)
    {
        bool smart = false;

        List<string> realDomainIPs = await GetARecordIPsAsync(domain, uncensoredDns);
        List<string> domainIPs = await GetARecordIPsAsync(domain, DNS);

        // Method 1: Reverse Dns
        if (realDomainIPs.Any() && domainIPs.Any())
        {
            bool isMatch = false;
            int count = 0;
            for (int n = 0; n < realDomainIPs.Count; n++)
            {
                string realDomainIP = realDomainIPs[n];
                NetworkTool.IpToHost(realDomainIP, out string realHost);
                if (string.IsNullOrEmpty(realHost)) continue;
                for (int n2 = 0; n2 < domainIPs.Count; n2++)
                {
                    
                    string domainIP = domainIPs[n2];
                    NetworkTool.IpToHost(domainIP, out string host);
                    if (string.IsNullOrEmpty(host)) continue;
                    if (NetworkTool.IsLocalIP(domainIP)) continue;

                    Debug.WriteLine(realHost + " == " + host);
                    if (!realHost.Equals(host, StringComparison.OrdinalIgnoreCase))
                    {
                        count++;
                        isMatch = true;
                    }
                }
            }

            smart = isMatch && (count == realDomainIPs.Count * domainIPs.Count);
        }
        if (smart) return smart;

        // Method 2: Reading Headers
        if (realDomainIPs.Any() && domainIPs.Any())
        {
            for (int n = 0; n < realDomainIPs.Count; n++)
            {
                if (smart) break;
                string realDomainIP = realDomainIPs[n];
                string readHeader = await NetworkTool.GetHeaders(domain, realDomainIP, null, 5, CancellationToken.None);
                if (string.IsNullOrEmpty(readHeader)) continue; // There is nothing to check, continue
                Debug.WriteLine(readHeader);
                if (!readHeader.ToLower().StartsWith("forbidden")) break; // It's not Forbidden, break
                for (int n2 = 0; n2 < domainIPs.Count; n2++)
                {
                    string domainIP = domainIPs[n2];
                    string header = await NetworkTool.GetHeaders(domain, domainIP, null, 5, CancellationToken.None);
                    if (string.IsNullOrEmpty(header)) continue;
                    Debug.WriteLine(header);

                    if (!header.ToLower().StartsWith("forbidden"))
                    {
                        smart = true;
                        if (smart) break;
                    }
                }
            }
        }
        return smart;
    }

    //================================= Generate IPs

    public async Task GenerateGoogleSafeSearchIpsAsync(string uncensoredDns)
    {
        if (!GoogleSafeSearchIpList.Any())
        {
            string websiteSS = "forcesafesearch.google.com";
            GoogleSafeSearchIpList = await GetARecordIPsAsync(websiteSS, uncensoredDns);
            Debug.WriteLine("Google Safe Search IPs Generated, Count: " + GoogleSafeSearchIpList.Count);
        }
    }

    public async Task GenerateBingSafeSearchIpsAsync(string uncensoredDns)
    {
        if (!BingSafeSearchIpList.Any())
        {
            string websiteSS = "strict.bing.com";
            BingSafeSearchIpList = await GetARecordIPsAsync(websiteSS, uncensoredDns);
            Debug.WriteLine("Bing Safe Search IPs Generated, Count: " + BingSafeSearchIpList.Count);
        }
    }

    public async Task GenerateYoutubeRestrictIpsAsync(string uncensoredDns)
    {
        if (!YoutubeRestrictIpList.Any())
        {
            string websiteR = "restrict.youtube.com";
            string websiteRM = "restrictmoderate.youtube.com";
            List<string> youtubeR = await GetARecordIPsAsync(websiteR, uncensoredDns);
            List<string> youtubeRM = await GetARecordIPsAsync(websiteRM, uncensoredDns);

            YoutubeRestrictIpList = youtubeR.Concat(youtubeRM).ToList();
            Debug.WriteLine("Youtube Restrict IPs Generated, Count: " + YoutubeRestrictIpList.Count);
        }
    }

    public async Task GenerateAdultDomainIpsAsync(string uncensoredDns)
    {
        if (!AdultIpList.Any())
        {
            string websiteAD = AdultDomainToCheck;
            AdultIpList = await GetARecordIPsAsync(websiteAD, uncensoredDns);
            Debug.WriteLine("Adult IPs Generated, Count: " + AdultIpList.Count);
        }
    }

    //================================= Check DNS Filters

    private void CheckDnsFilters(string dnsServer, out bool isGoogleSafeSearch, out bool isBingSafeSearch, out bool isYoutubeRestricted, out bool isAdultFilter)
    {
        bool isGoogleSafeSearchOut = false; isGoogleSafeSearch = false;
        bool isBingSafeSearchOut = false; isBingSafeSearch = false;
        bool isYoutubeRestrictedOut = false; isYoutubeRestricted = false;
        bool isAdultFilterOut = false; isAdultFilter = false;

        Task task = Task.Run(async () =>
        {
            await GenerateGoogleSafeSearchIpsAsync(dnsServer);
            await GenerateBingSafeSearchIpsAsync(dnsServer);
            await GenerateYoutubeRestrictIpsAsync(dnsServer);
            await GenerateAdultDomainIpsAsync(dnsServer);

            // Check Google Force Safe Search
            if (GoogleSafeSearchIpList.Any())
            {
                List<string> googleIpList = await GetARecordIPsAsync("google.com", dnsServer);
                isGoogleSafeSearchOut = HasSameItem(googleIpList, GoogleSafeSearchIpList, true);
            }

            // Check Bing Force Safe Search
            if (BingSafeSearchIpList.Any())
            {
                List<string> bingIpList = await GetARecordIPsAsync("bing.com", dnsServer);
                isBingSafeSearchOut = HasSameItem(bingIpList, BingSafeSearchIpList, true);
            }

            // Check Youtube Restriction
            if (YoutubeRestrictIpList.Any())
            {
                List<string> youtubeIpList = await GetARecordIPsAsync("youtube.com", dnsServer);
                isYoutubeRestrictedOut = HasSameItem(youtubeIpList, YoutubeRestrictIpList, true);
            }
            
            // Check Adult Filter
            if (AdultIpList.Any())
            {
                List<string> adultIpList = await GetARecordIPsAsync(AdultDomainToCheck, dnsServer);
                isAdultFilterOut = HasLocalIP(adultIpList);
                if (!isAdultFilterOut)
                    isAdultFilterOut = !HasSameItem(adultIpList, AdultIpList, false);
            }
        });

        try { task.Wait(); } catch (Exception) { }

        isGoogleSafeSearch = isGoogleSafeSearchOut;
        isBingSafeSearch = isBingSafeSearchOut;
        isYoutubeRestricted = isYoutubeRestrictedOut;
        isAdultFilter = isAdultFilterOut;
    }

    private async Task<List<string>> GetARecordIPsAsync(string domain, string dnsServer)
    {
        string jsonStr = await GetDnsLookupJsonAsync(domain, dnsServer, TimeoutMS);
        return GetDnsLookupAnswerRecordList(jsonStr);
    }

    private bool HasSameItem(List<string> list, List<string> uncensoredList, bool checkForLocalIPs)
    {
        bool hasSameItem = false;
        for (int i = 0; i < uncensoredList.Count; i++)
        {
            if (hasSameItem) break;
            string uncensoredIp = uncensoredList[i].Trim();
            for (int j = 0; j < list.Count; j++)
            {
                string ip = list[j];
                if (ip.Equals(uncensoredIp))
                {
                    hasSameItem = true;
                    break;
                }
                if (checkForLocalIPs && NetworkTool.IsLocalIP(ip))
                {
                    hasSameItem = true;
                    break;
                }
            }
        }
        return hasSameItem;
    }

    private bool HasLocalIP(List<string> list)
    {
        for (int j = 0; j < list.Count; j++)
        {
            string ip = list[j];
            if (NetworkTool.IsLocalIP(ip)) return true;
        }
        return false;
    }

    private async Task<string> GetDnsLookupJsonAsync(string domain, string dnsServer, int timeoutMS, string rrtype = "A", bool json = true)
    {
        string result = string.Empty;

        try
        {
            Task<string> task = Task.Run(async () =>
            {
                Dictionary<string, string> evs = new();
                if (json) evs.Add("JSON", "1");
                else evs.Add("JSON", "");
                evs.Add("RRTYPE", rrtype);
                if (Insecure) evs.Add("VERIFY", "0");
                else evs.Add("VERIFY", "");

                string args = $"{domain} {dnsServer}";
                result = await ProcessManager.ExecuteAsync(SecureDNS.DnsLookup, evs, args, true, true, SecureDNS.CurrentPath, ProcessPriority);

                if (Insecure)
                {
                    string verify = "TLS verification has been disabled";
                    if (result.StartsWith(verify)) result = result.TrimStart(verify);
                }

                return result;
            });

            result = await task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMS));
            return result;
        }
        catch (Exception)
        {
            return result;
        }
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
                Debug.WriteLine("JSON String: " + jsonStr);
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