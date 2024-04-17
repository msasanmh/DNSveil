using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Diagnostics;
using System.Net;

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
    private List<IPAddress> GoogleSafeSearchIpList { get; set; } = new();
    private List<IPAddress> BingSafeSearchIpList { get; set; } = new();
    private List<IPAddress> YoutubeRestrictIpList { get; set; } = new();
    private List<IPAddress> AdultIpList { get; set; } = new();
    private string DNS = string.Empty;

    public bool Insecure { get; private set; } = false;
    public bool CheckForFilters { get; private set; } = false;
    private readonly int TimeoutMS = 10000;

    /// <summary>
    /// Check DNS Servers
    /// </summary>
    public CheckDns(bool insecure, bool checkForFilters)
    {
        Insecure = insecure;
        CheckForFilters = checkForFilters;
    }

    /// <summary>
    /// Check DNS and get latency (ms)
    /// </summary>
    public async Task CheckDnsAsync(string domain, string dnsServer, int timeoutMS)
    {
        DNS = dnsServer;
        IsDnsOnline = false;
        DnsLookupResult dlr = await CheckDnsWorkAsync(domain, DNS, timeoutMS, IPAddress.None, 0).ConfigureAwait(false);
        IsDnsOnline = dlr.IsDnsOnline;
        DnsLatency = IsDnsOnline ? dlr.Latency : -1;

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
    /// Check DNS And Get Latency (ms)
    /// </summary>
    public async Task CheckDnsAsync(string domain, string dnsServer, int timeoutMS, IPAddress bootstrapIP, int bootstrapPort)
    {
        DNS = dnsServer;
        IsDnsOnline = false;
        DnsLookupResult dlr = await CheckDnsWorkAsync(domain, DNS, timeoutMS, bootstrapIP, bootstrapPort).ConfigureAwait(false);
        IsDnsOnline = dlr.IsDnsOnline;
        DnsLatency = IsDnsOnline ? dlr.Latency : -1;

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

    private class DnsLookupResult
    {
        public int Latency { get; set; } = -1;
        public bool IsDnsOnline { get; set; } = false;
    }

    private async Task<DnsLookupResult> CheckDnsWorkAsync(string domain, string dnsServer, int timeoutMS, IPAddress bootstrapIP, int bootstrapPort)
    {
        try
        {
            bool hasLocalIp = false;
            int aRecordCount = 0;
            int latency = -1;
            DnsMessage dmQ = DnsMessage.CreateQuery(DnsEnums.DnsProtocol.UDP, domain, DnsEnums.RRType.A, DnsEnums.CLASS.IN);
            bool isWriteSuccess = DnsMessage.TryWrite(dmQ, out byte[] dmQBuffer);
            if (isWriteSuccess)
            {
                Stopwatch sw = Stopwatch.StartNew();
                byte[] dmABuffer = await DnsClient.QueryAsync(dmQBuffer, DnsEnums.DnsProtocol.UDP, dnsServer, Insecure, bootstrapIP, bootstrapPort, timeoutMS, CancellationToken.None).ConfigureAwait(false);
                sw.Stop();
                latency = Convert.ToInt32(sw.ElapsedMilliseconds);
                if (dmABuffer.Length >= 12) // 12 Header Length
                {
                    DnsMessage dmA = DnsMessage.Read(dmABuffer, DnsEnums.DnsProtocol.UDP);
                    if (dmA.IsSuccess)
                    {
                        if (dmA.Header.AnswersCount > 0 && dmA.Answers.AnswerRecords.Count > 0)
                        {
                            for (int n = 0; n < dmA.Answers.AnswerRecords.Count; n++)
                            {
                                IResourceRecord irr = dmA.Answers.AnswerRecords[n];
                                if (irr is not ARecord aRecord) continue;
                                if (NetworkTool.IsLocalIP(aRecord.IP.ToString())) hasLocalIp = true;
                                aRecordCount++;
                            }
                        }
                    }
                }
            }

            DnsLookupResult dlr = new()
            {
                IsDnsOnline = !hasLocalIp && aRecordCount > 0,
                Latency = latency
            };

            return dlr;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CheckDnsWorkAsync: " + ex.Message);
            return new DnsLookupResult();
        }
    }

    //================================= Check Dns as SmartDns

    public async Task<bool> CheckAsSmartDns(string uncensoredDns, string domain, string? dns = null)
    {
        if (!string.IsNullOrEmpty(dns)) DNS = dns;

        bool smart = false;

        List<IPAddress> realDomainIPs = await GetARecordIPsAsync(domain, uncensoredDns).ConfigureAwait(false);
        List<IPAddress> domainIPs = await GetARecordIPsAsync(domain, DNS).ConfigureAwait(false);

        // Method 1: Reverse Dns
        if (realDomainIPs.Any() && domainIPs.Any())
        {
            bool isMatch = false;
            int count = 0;
            for (int n = 0; n < realDomainIPs.Count; n++)
            {
                IPAddress realDomainIP = realDomainIPs[n];
                NetworkTool.IpToHost(realDomainIP.ToString(), out string realHost);
                if (string.IsNullOrEmpty(realHost)) continue;
                for (int n2 = 0; n2 < domainIPs.Count; n2++)
                {

                    IPAddress domainIP = domainIPs[n2];
                    NetworkTool.IpToHost(domainIP.ToString(), out string host);
                    if (string.IsNullOrEmpty(host)) continue;
                    if (NetworkTool.IsLocalIP(domainIP.ToString())) continue;

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
                IPAddress realDomainIP = realDomainIPs[n];
                string readHeader = await NetworkTool.GetHeaders(domain, realDomainIP.ToString(), 5000, false).ConfigureAwait(false);
                if (string.IsNullOrEmpty(readHeader)) continue; // There is nothing to check, continue
                Debug.WriteLine(readHeader);
                if (!readHeader.ToLower().StartsWith("forbidden")) break; // It's not Forbidden, break
                for (int n2 = 0; n2 < domainIPs.Count; n2++)
                {
                    IPAddress domainIP = domainIPs[n2];
                    string header = await NetworkTool.GetHeaders(domain, domainIP.ToString(), 5000, false).ConfigureAwait(false);
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
            GoogleSafeSearchIpList = await GetARecordIPsAsync(websiteSS, uncensoredDns).ConfigureAwait(false);
            Debug.WriteLine("Google Safe Search IPs Generated, Count: " + GoogleSafeSearchIpList.Count);
        }
    }

    public async Task GenerateBingSafeSearchIpsAsync(string uncensoredDns)
    {
        if (!BingSafeSearchIpList.Any())
        {
            string websiteSS = "strict.bing.com";
            BingSafeSearchIpList = await GetARecordIPsAsync(websiteSS, uncensoredDns).ConfigureAwait(false);
            Debug.WriteLine("Bing Safe Search IPs Generated, Count: " + BingSafeSearchIpList.Count);
        }
    }

    public async Task GenerateYoutubeRestrictIpsAsync(string uncensoredDns)
    {
        if (!YoutubeRestrictIpList.Any())
        {
            string websiteR = "restrict.youtube.com";
            string websiteRM = "restrictmoderate.youtube.com";
            List<IPAddress> youtubeR = await GetARecordIPsAsync(websiteR, uncensoredDns).ConfigureAwait(false);
            List<IPAddress> youtubeRM = await GetARecordIPsAsync(websiteRM, uncensoredDns).ConfigureAwait(false);

            YoutubeRestrictIpList = youtubeR.Concat(youtubeRM).ToList();
            Debug.WriteLine("Youtube Restrict IPs Generated, Count: " + YoutubeRestrictIpList.Count);
        }
    }

    public async Task GenerateAdultDomainIpsAsync(string uncensoredDns)
    {
        if (!AdultIpList.Any())
        {
            string websiteAD = AdultDomainToCheck;
            AdultIpList = await GetARecordIPsAsync(websiteAD, uncensoredDns).ConfigureAwait(false);
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
            await GenerateGoogleSafeSearchIpsAsync(dnsServer).ConfigureAwait(false);
            await GenerateBingSafeSearchIpsAsync(dnsServer).ConfigureAwait(false);
            await GenerateYoutubeRestrictIpsAsync(dnsServer).ConfigureAwait(false);
            await GenerateAdultDomainIpsAsync(dnsServer).ConfigureAwait(false);

            // Check Google Force Safe Search
            if (GoogleSafeSearchIpList.Any())
            {
                List<IPAddress> googleIpList = await GetARecordIPsAsync("google.com", dnsServer).ConfigureAwait(false);
                isGoogleSafeSearchOut = HasSameItem(googleIpList, GoogleSafeSearchIpList, true);
            }

            // Check Bing Force Safe Search
            if (BingSafeSearchIpList.Any())
            {
                List<IPAddress> bingIpList = await GetARecordIPsAsync("bing.com", dnsServer).ConfigureAwait(false);
                isBingSafeSearchOut = HasSameItem(bingIpList, BingSafeSearchIpList, true);
            }

            // Check Youtube Restriction
            if (YoutubeRestrictIpList.Any())
            {
                List<IPAddress> youtubeIpList = await GetARecordIPsAsync("youtube.com", dnsServer).ConfigureAwait(false);
                isYoutubeRestrictedOut = HasSameItem(youtubeIpList, YoutubeRestrictIpList, true);
            }
            
            // Check Adult Filter
            if (AdultIpList.Any())
            {
                List<IPAddress> adultIpList = await GetARecordIPsAsync(AdultDomainToCheck, dnsServer).ConfigureAwait(false);
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

    private async Task<List<IPAddress>> GetARecordIPsAsync(string domain, string dnsServer)
    {
        List<IPAddress> ips = new();

        try
        {
            DnsMessage dmQ = DnsMessage.CreateQuery(DnsEnums.DnsProtocol.UDP, domain, DnsEnums.RRType.A, DnsEnums.CLASS.IN);
            bool isWriteSuccess = DnsMessage.TryWrite(dmQ, out byte[] dmQBuffer);
            if (isWriteSuccess)
            {
                byte[] dmABuffer = await DnsClient.QueryAsync(dmQBuffer, DnsEnums.DnsProtocol.UDP, dnsServer, Insecure, IPAddress.None, 0, TimeoutMS, CancellationToken.None).ConfigureAwait(false);
                DnsMessage dmA = DnsMessage.Read(dmABuffer, DnsEnums.DnsProtocol.UDP);
                if (dmA.IsSuccess)
                {
                    if (dmA.Header.AnswersCount > 0 && dmA.Answers.AnswerRecords.Count > 0)
                    {
                        for (int n = 0; n < dmA.Answers.AnswerRecords.Count; n++)
                        {
                            IResourceRecord irr = dmA.Answers.AnswerRecords[n];
                            if (irr is not ARecord aRecord) continue;
                            ips.Add(aRecord.IP);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetARecordIPsAsync: " + ex.Message);
        }

        return ips;
    }

    private static bool HasSameItem(List<IPAddress> list, List<IPAddress> uncensoredList, bool checkForLocalIPs)
    {
        bool hasSameItem = false;
        for (int i = 0; i < uncensoredList.Count; i++)
        {
            if (hasSameItem) break;
            IPAddress uncensoredIp = uncensoredList[i];
            for (int j = 0; j < list.Count; j++)
            {
                IPAddress ip = list[j];
                if (ip.Equals(uncensoredIp))
                {
                    hasSameItem = true;
                    break;
                }
                if (checkForLocalIPs && NetworkTool.IsLocalIP(ip.ToString()))
                {
                    hasSameItem = true;
                    break;
                }
            }
        }
        return hasSameItem;
    }

    private static bool HasLocalIP(List<IPAddress> list)
    {
        for (int n = 0; n < list.Count; n++)
        {
            IPAddress ip = list[n];
            if (NetworkTool.IsLocalIP(ip.ToString())) return true;
        }
        return false;
    }

}