using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Xml.Linq;
using static DNSveil.Logic.DnsServers.EnumsAndStructs;

namespace DNSveil.Logic.DnsServers;

public class ScanDns
{
    private string FilterDomainGoogle { get; set; } = "google.com";
    private string FilterDomainGoogleS { get; set; } = "forcesafesearch.google.com";
    private string FilterDomainBing { get; set; } = "bing.com";
    private string FilterDomainBingS { get; set; } = "strict.bing.com";
    private string FilterDomainYoutube { get; set; } = "youtube.com";
    private string FilterDomainYoutubeR { get; set; } = "restrict.youtube.com";
    private string FilterDomainYoutubeRM { get; set; } = "restrictmoderate.youtube.com";
    private string FilterDomainAdult { get; set; } = "pornhub.com";
    private string FilterDomainAdultAlter { get; set; } = "reflected.net";
    private List<IPAddress> GoogleSafeSearchIpList { get; set; } = new();
    private List<IPAddress> BingSafeSearchIpList { get; set; } = new();
    private List<IPAddress> YoutubeRestrictIpList { get; set; } = new();
    private List<IPAddress> AdultIpList { get; set; } = new();
    private List<string> AdultIpCidrList { get; set; } = new();
    private bool Insecure { get; set; } = false;
    private bool CheckForFilters { get; set; } = false;
    private string TEMP_AnonDNSCrypt_Relay { get; set; } = string.Empty;

    public ScanDns(bool insecure, bool checkForFilters)
    {
        Insecure = insecure;
        CheckForFilters = checkForFilters;

        if (FilterDomainAdult.ToLower().Contains("pornhub"))
        {
            AdultIpCidrList.Add("66.254.114.0/24");
        }
    }

    public void SetGoogle(string googleDomain, string googleSafeSearchDomain)
    {
        googleDomain = googleDomain.Trim();
        googleSafeSearchDomain = googleSafeSearchDomain.Trim();
        if (string.IsNullOrEmpty(googleDomain) || string.IsNullOrEmpty(googleSafeSearchDomain)) return;
        FilterDomainGoogle = googleDomain;
        FilterDomainGoogleS = googleSafeSearchDomain;
    }

    public void SetBing(string bingDomain, string bingStrictDomain)
    {
        bingDomain = bingDomain.Trim();
        bingStrictDomain = bingStrictDomain.Trim();
        if (string.IsNullOrEmpty(bingDomain) || string.IsNullOrEmpty(bingStrictDomain)) return;
        FilterDomainBing = bingDomain;
        FilterDomainBingS = bingStrictDomain;
    }

    public void SetYoutube(string youtubeDomain, string youtubeRestrictDomain, string youtubeRestrictModerateDomain)
    {
        youtubeDomain = youtubeDomain.Trim();
        youtubeRestrictDomain = youtubeRestrictDomain.Trim();
        youtubeRestrictModerateDomain = youtubeRestrictModerateDomain.Trim();
        if (string.IsNullOrEmpty(youtubeDomain) || string.IsNullOrEmpty(youtubeRestrictDomain) || string.IsNullOrEmpty(youtubeRestrictModerateDomain)) return;
        FilterDomainYoutube = youtubeDomain;
        FilterDomainYoutubeR = youtubeRestrictDomain;
        FilterDomainYoutubeRM = youtubeRestrictModerateDomain;
    }

    public void SetAdult(string filterDomainAdult, List<string> adultIpCidrList)
    {
        filterDomainAdult = filterDomainAdult.Trim();
        if (string.IsNullOrEmpty(filterDomainAdult)) return;
        FilterDomainAdult = filterDomainAdult;
        AdultIpCidrList = adultIpCidrList;
    }

    public struct ParallelResult
    {
        public int OnlineServers { get; set; }
        public int SelectedServers { get; set; }
        public int SumLatencyMS { get; set; }
        public int AverageLatencyMS { get; set; }
        public int ParallelLatencyMS { get; set; }
    }

    public async Task<(List<XElement> DnsItemElementsList, ParallelResult ParallelResult)> CheckDNSCryptInParallelAsync(string domain, List<DnsItem> targetListAsDnsItem, List<string> relayList, int parallelSize, FilterByProperties filterByProperties, int timeoutMS, IPAddress bootstrapIP, int bootstrapPort, bool useExternal, CancellationToken ct, string? proxyScheme = null)
    {
        List<XElement> dnsItemElementsList = new();
        ParallelResult pr = new();
        
        try
        {
            // Parallel Latency
            Stopwatch sw_Parallel = Stopwatch.StartNew();

            // Parallel Options
            ParallelOptions parallelOptions = new() { CancellationToken = ct };

            // Convert List To Concurrent
            Task parallel = Parallel.ForEachAsync(targetListAsDnsItem, parallelOptions, async (target, pCT) =>
            {
                if (pCT.IsCancellationRequested) return;
                var scanResult = await CheckDNSCryptAsync(domain, target, relayList, parallelSize, timeoutMS, bootstrapIP, bootstrapPort, useExternal, pCT, proxyScheme).ConfigureAwait(false);
                if (pCT.IsCancellationRequested) return;

                scanResult.DnsItem.Enabled = DnsServersManager.IsDnsItemEnabledByOptions(scanResult.DnsItem, filterByProperties);
                if (scanResult.DnsItem.Status == DnsStatus.Online)
                {
                    pr.OnlineServers++;
                    if (scanResult.DnsItem.Enabled) pr.SelectedServers++;
                    pr.SumLatencyMS += scanResult.DnsItem.Latency;

                    if (string.IsNullOrWhiteSpace(scanResult.DnsItem.Description))
                        scanResult.DnsItem.Description = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}";
                    else
                    {
                        bool isDateTime = DateTime.TryParse(scanResult.DnsItem.Description, out _);
                        if (isDateTime) scanResult.DnsItem.Description = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}";
                    }
                }

                if (!string.IsNullOrEmpty(scanResult.DnsItem.DNS_URL))
                {
                    XElement dnsItemElementNew = DnsServersManager.Create_DnsItem_Element(scanResult.DnsItem);
                    dnsItemElementsList.Add(dnsItemElementNew);
                }
            });
            try { await parallel.WaitAsync(ct).ConfigureAwait(false); } catch (Exception) { }

            // Set Average Latency
            pr.AverageLatencyMS = pr.OnlineServers > 0 ? pr.SumLatencyMS / pr.OnlineServers : 0;

            sw_Parallel.Stop();
            pr.ParallelLatencyMS = Convert.ToInt32(sw_Parallel.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ScanDns CheckDNSCryptInParallelAsync: " + ex.Message);
        }

        return (dnsItemElementsList, pr);
    }

    public async Task<(DnsItem DnsItem, DnsMessage DnsMessage)> CheckDNSCryptAsync(string domain, DnsItem targetAsDnsItem, List<string> relayList, int parallelSize, int timeoutMS, IPAddress bootstrapIP, int bootstrapPort, bool useExternal, CancellationToken ct, string? proxyScheme = null)
    {
        return await Task.Run(async () =>
        {
            (DnsItem DnsItem, DnsMessage DnsMessage) resultOut = new(new(), new());
            
            try
            {
                if (parallelSize < 5) parallelSize = 5;
                Random random1 = new();
                Random random2 = new();
                int maxRelaysToCheck = 20;
                var relaysList = relayList.SplitToLists(parallelSize);

                for (int n = 0; n < relaysList.Count; n++)
                {
                    if (ct.IsCancellationRequested) break;
                    if (maxRelaysToCheck <= 0) break;

                    List<Task<(DnsItem DnsItem, DnsMessage DnsMessage)>> tasks = new();
                    CancellationTokenSource cts = new();
                    int randomInt1 = random1.Next(0, relaysList.Count - 1);
                    List<string> relays = relaysList[randomInt1];
                    for (int i = 0; i < relays.Count; i++)
                    {
                        int randomInt2 = random2.Next(0, relays.Count - 1);
                        string relay = relays[randomInt2];
                        string anonymizedDNSCrypt = $"{targetAsDnsItem.DNS_URL} {relay}";
                        Task<(DnsItem DnsItem, DnsMessage DnsMessage)> task = CheckDnsAsync(domain, anonymizedDNSCrypt, timeoutMS, bootstrapIP, bootstrapPort, useExternal, cts.Token, proxyScheme);
                        tasks.Add(task);
                        maxRelaysToCheck--;
                        if (maxRelaysToCheck <= 0) break;
                    }

                    // Add Working Relay
                    if (!string.IsNullOrEmpty(TEMP_AnonDNSCrypt_Relay))
                    {
                        string anonymizedDNSCrypt = $"{targetAsDnsItem.DNS_URL} {TEMP_AnonDNSCrypt_Relay}";
                        Task<(DnsItem DnsItem, DnsMessage DnsMessage)> task = CheckDnsAsync(domain, anonymizedDNSCrypt, timeoutMS, bootstrapIP, bootstrapPort, useExternal, cts.Token, proxyScheme);
                        tasks.Add(task);
                        maxRelaysToCheck = 0;
                    }
                    
                    while (true)
                    {
                        if (ct.IsCancellationRequested) break;
                        if (tasks.Count == 0) break;
                        Task<(DnsItem DnsItem, DnsMessage DnsMessage)>? taskResult = null;
                        try { taskResult = await Task.WhenAny(tasks).WaitAsync(ct).ConfigureAwait(false); } catch (Exception) { }
                        if (taskResult == null) break;
                        (DnsItem DnsItem, DnsMessage DnsMessage) result = await taskResult.ConfigureAwait(false);
                        resultOut = result;
                        if (result.DnsItem.Status == DnsStatus.Online) break;
                        tasks.Remove(taskResult);
                    }
                    tasks.Clear();
                    cts.CancelAfter(1000);
                    if (ct.IsCancellationRequested) break;

                    if (resultOut.DnsItem.Status == DnsStatus.Online)
                    {
                        // Find And Save Working Relay
                        string[] targetRelay = resultOut.DnsItem.DNS_URL.Split(' ', StringSplitOptions.TrimEntries);
                        if (targetRelay.Length > 1)
                        {
                            string relay = targetRelay[1];
                            if (!string.IsNullOrEmpty(relay))
                            {
                                lock (TEMP_AnonDNSCrypt_Relay)
                                {
                                    TEMP_AnonDNSCrypt_Relay = relay;
                                }
                            }
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ScanDns CheckDNSCryptAsync: " + ex.Message);
            }
            
            // Clone To Copy Unique ID
            DnsItem currentDnsItem = resultOut.DnsItem;
            DnsMessage currentDnsMessage = resultOut.DnsMessage;
            DnsItem cloneDnsItem = Clone_DnsItem(targetAsDnsItem, currentDnsItem);
            
            return (cloneDnsItem, currentDnsMessage);
        }).ConfigureAwait(false);
    }

    public async Task<(List<XElement> DnsItemElementsList, ParallelResult ParallelResult)> CheckDnsInParallelAsync(string domain, List<XElement> dnsItemElementsList, FilterByProperties filterByProperties, int timeoutMS, IPAddress bootstrapIP, int bootstrapPort, bool useExternal, CancellationToken ct, string? proxyScheme = null)
    {
        ParallelResult pr = new();

        try
        {
            // Parallel Latency
            Stopwatch sw_Parallel = Stopwatch.StartNew();

            // Parallel Options
            ParallelOptions parallelOptions = new() { CancellationToken = ct };

            // Convert List To Concurrent
            ConcurrentDictionary<uint, XElement> dnsItemElementsConcurrentList = dnsItemElementsList.ToConcurrentDictionary();
            Task parallel = Parallel.ForEachAsync(dnsItemElementsConcurrentList, parallelOptions, async (dnsItemElement, pCT) =>
            {
                if (pCT.IsCancellationRequested) return;
                DnsItem dnsItem = DnsServersManager.Create_DnsItem(dnsItemElement.Value);

                if (pCT.IsCancellationRequested) return;
                var scanResult = await CheckDnsAsync(domain, dnsItem.DNS_URL, timeoutMS, bootstrapIP, bootstrapPort, useExternal, pCT, proxyScheme).ConfigureAwait(false);
                if (pCT.IsCancellationRequested) return;

                dnsItem = Clone_DnsItem(dnsItem, scanResult.DnsItem);
                dnsItem.Enabled = DnsServersManager.IsDnsItemEnabledByOptions(dnsItem, filterByProperties);
                if (dnsItem.Status == DnsStatus.Online)
                {
                    pr.OnlineServers++;
                    if (dnsItem.Enabled) pr.SelectedServers++;
                    pr.SumLatencyMS += dnsItem.Latency;

                    if (string.IsNullOrWhiteSpace(dnsItem.Description))
                        dnsItem.Description = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}";
                    else
                    {
                        bool isDateTime = DateTime.TryParse(dnsItem.Description, out _);
                        if (isDateTime) dnsItem.Description = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}";
                    }
                }

                XElement dnsItemElementNew = DnsServersManager.Create_DnsItem_Element(dnsItem);
                dnsItemElementsConcurrentList.TryUpdate(dnsItemElement.Key, dnsItemElementNew);
            });
            try { await parallel.WaitAsync(ct).ConfigureAwait(false); } catch (Exception) { }

            // Convert Back To List
            dnsItemElementsList = dnsItemElementsConcurrentList.Select(x => x.Value).ToList();

            // Set Average Latency
            pr.AverageLatencyMS = pr.OnlineServers > 0 ? pr.SumLatencyMS / pr.OnlineServers : 0;

            sw_Parallel.Stop();
            pr.ParallelLatencyMS = Convert.ToInt32(sw_Parallel.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ScanDns CheckDnsInParallelAsync: " + ex.Message);
        }

        return (dnsItemElementsList, pr);
    }

    public async Task<(List<DnsItem> DnsItemList, ParallelResult ParallelResult)> CheckDnsInParallelAsync(string domain, List<DnsItem> dnsItemList, FilterByProperties filterByProperties, int timeoutMS, IPAddress bootstrapIP, int bootstrapPort, bool useExternal, CancellationToken ct, string? proxyScheme = null)
    {
        ParallelResult pr = new();

        try
        {
            // Parallel Latency
            Stopwatch sw_Parallel = Stopwatch.StartNew();

            // Parallel Options
            ParallelOptions parallelOptions = new() { CancellationToken = ct };

            // Convert List To Concurrent
            ConcurrentDictionary<uint, DnsItem> dnsItemConcurrentList = dnsItemList.ToConcurrentDictionary();
            Task parallel = Parallel.ForEachAsync(dnsItemConcurrentList, parallelOptions, async (dnsItemKV, pCT) =>
            {
                if (pCT.IsCancellationRequested) return;
                DnsItem dnsItem = dnsItemKV.Value;

                if (pCT.IsCancellationRequested) return;
                var scanResult = await CheckDnsAsync(domain, dnsItem.DNS_URL, timeoutMS, bootstrapIP, bootstrapPort, useExternal, pCT, proxyScheme).ConfigureAwait(false);
                if (pCT.IsCancellationRequested) return;

                dnsItem = Clone_DnsItem(dnsItem, scanResult.DnsItem);
                dnsItem.Enabled = DnsServersManager.IsDnsItemEnabledByOptions(dnsItem, filterByProperties);
                if (dnsItem.Status == DnsStatus.Online)
                {
                    pr.OnlineServers++;
                    if (dnsItem.Enabled) pr.SelectedServers++;
                    pr.SumLatencyMS += dnsItem.Latency;

                    if (string.IsNullOrWhiteSpace(dnsItem.Description))
                        dnsItem.Description = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}";
                    else
                    {
                        bool isDateTime = DateTime.TryParse(dnsItem.Description, out _);
                        if (isDateTime) dnsItem.Description = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}";
                    }
                }

                dnsItemConcurrentList.TryUpdate(dnsItemKV.Key, dnsItem);
            });
            try { await parallel.WaitAsync(ct).ConfigureAwait(false); } catch (Exception) { }

            // Convert Back To List
            dnsItemList = dnsItemConcurrentList.Select(x => x.Value).ToList();

            // Set Average Latency
            pr.AverageLatencyMS = pr.OnlineServers > 0 ? pr.SumLatencyMS / pr.OnlineServers : 0;

            sw_Parallel.Stop();
            pr.ParallelLatencyMS = Convert.ToInt32(sw_Parallel.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ScanDns CheckDnsInParallelAsync: " + ex.Message);
        }

        return (dnsItemList, pr);
    }

    public async Task<(List<XElement> DnsItemElementsList, ParallelResult ParallelResult)> CheckDnsInParallelAsync(string domain, List<XElement> dnsItemElementsList, FilterByProtocols filterByProtocols, FilterByProperties filterByProperties, int timeoutMS, IPAddress bootstrapIP, int bootstrapPort, bool useExternal, CancellationToken ct, string? proxyScheme = null)
    {
        ParallelResult pr = new();

        try
        {
            // Parallel Latency
            Stopwatch sw_Parallel = Stopwatch.StartNew();

            // Parallel Options
            ParallelOptions parallelOptions = new() { CancellationToken = ct };

            // Convert List To Concurrent
            ConcurrentDictionary<uint, XElement> dnsItemElementsConcurrentList = dnsItemElementsList.ToConcurrentDictionary();
            Task parallel = Parallel.ForEachAsync(dnsItemElementsConcurrentList, parallelOptions, async (dnsItemElement, pCT) =>
            {
                if (pCT.IsCancellationRequested) return;
                DnsItem dnsItem = DnsServersManager.Create_DnsItem(dnsItemElement.Value);
                dnsItem.Enabled = DnsServersManager.IsDnsItemEnabledByProtocols(dnsItem, filterByProtocols);

                if (dnsItem.Enabled) // Protocol Selection Is Match
                {
                    if (pCT.IsCancellationRequested) return;
                    var scanResult = await CheckDnsAsync(domain, dnsItem.DNS_URL, timeoutMS, bootstrapIP, bootstrapPort, useExternal, pCT, proxyScheme).ConfigureAwait(false);
                    if (pCT.IsCancellationRequested) return;

                    dnsItem = Clone_DnsItem(dnsItem, scanResult.DnsItem);
                    dnsItem.Enabled = DnsServersManager.IsDnsItemEnabledByOptions(dnsItem, filterByProtocols, filterByProperties);
                    if (dnsItem.Status == DnsStatus.Online)
                    {
                        pr.OnlineServers++;
                        if (dnsItem.Enabled) pr.SelectedServers++;
                        pr.SumLatencyMS += dnsItem.Latency;

                        if (string.IsNullOrWhiteSpace(dnsItem.Description))
                            dnsItem.Description = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}";
                        else
                        {
                            bool isDateTime = DateTime.TryParse(dnsItem.Description, out _);
                            if (isDateTime) dnsItem.Description = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}";
                        }
                    }
                }
                else
                {
                    dnsItem.Status = DnsStatus.Skipped;
                    dnsItem.Enabled = false;
                }

                XElement dnsItemElementNew = DnsServersManager.Create_DnsItem_Element(dnsItem);
                dnsItemElementsConcurrentList.TryUpdate(dnsItemElement.Key, dnsItemElementNew);
            });
            try { await parallel.WaitAsync(ct).ConfigureAwait(false); } catch (Exception) { }

            // Convert Back To List
            dnsItemElementsList = dnsItemElementsConcurrentList.Select(x => x.Value).ToList();

            // Set Average Latency
            pr.AverageLatencyMS = pr.OnlineServers > 0 ? pr.SumLatencyMS / pr.OnlineServers : 0;

            sw_Parallel.Stop();
            pr.ParallelLatencyMS = Convert.ToInt32(sw_Parallel.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ScanDns CheckDnsInParallelAsync: " + ex.Message);
        }

        return (dnsItemElementsList, pr);
    }

    public async Task<(List<DnsItem> DnsItemList, ParallelResult ParallelResult)> CheckDnsInParallelAsync(string domain, List<DnsItem> dnsItemList, FilterByProtocols filterByProtocols, FilterByProperties filterByProperties, int timeoutMS, IPAddress bootstrapIP, int bootstrapPort, bool useExternal, CancellationToken ct, string? proxyScheme = null)
    {
        ParallelResult pr = new();

        try
        {
            // Parallel Latency
            Stopwatch sw_Parallel = Stopwatch.StartNew();

            // Parallel Options
            ParallelOptions parallelOptions = new() { CancellationToken = ct };

            // Convert List To Concurrent
            ConcurrentDictionary<uint, DnsItem> dnsItemConcurrentList = dnsItemList.ToConcurrentDictionary();
            Task parallel = Parallel.ForEachAsync(dnsItemConcurrentList, parallelOptions, async (dnsItemKV, pCT) =>
            {
                if (pCT.IsCancellationRequested) return;
                DnsItem dnsItem = dnsItemKV.Value;
                dnsItem.Enabled = DnsServersManager.IsDnsItemEnabledByProtocols(dnsItem, filterByProtocols);

                if (dnsItem.Enabled) // Protocol Selection Is Match
                {
                    if (pCT.IsCancellationRequested) return;
                    var scanResult = await CheckDnsAsync(domain, dnsItem.DNS_URL, timeoutMS, bootstrapIP, bootstrapPort, useExternal, pCT, proxyScheme).ConfigureAwait(false);
                    if (pCT.IsCancellationRequested) return;

                    dnsItem = Clone_DnsItem(dnsItem, scanResult.DnsItem);
                    dnsItem.Enabled = DnsServersManager.IsDnsItemEnabledByOptions(dnsItem, filterByProtocols, filterByProperties);
                    if (dnsItem.Status == DnsStatus.Online)
                    {
                        pr.OnlineServers++;
                        if (dnsItem.Enabled) pr.SelectedServers++;
                        pr.SumLatencyMS += dnsItem.Latency;

                        if (string.IsNullOrWhiteSpace(dnsItem.Description))
                            dnsItem.Description = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}";
                        else
                        {
                            bool isDateTime = DateTime.TryParse(dnsItem.Description, out _);
                            if (isDateTime) dnsItem.Description = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}";
                        }
                    }
                }
                else
                {
                    dnsItem.Status = DnsStatus.Skipped;
                    dnsItem.Enabled = false;
                }

                dnsItemConcurrentList.TryUpdate(dnsItemKV.Key, dnsItem);
            });
            try { await parallel.WaitAsync(ct).ConfigureAwait(false); } catch (Exception) { }

            // Convert Back To List
            dnsItemList = dnsItemConcurrentList.Select(x => x.Value).ToList();

            // Set Average Latency
            pr.AverageLatencyMS = pr.OnlineServers > 0 ? pr.SumLatencyMS / pr.OnlineServers : 0;

            sw_Parallel.Stop();
            pr.ParallelLatencyMS = Convert.ToInt32(sw_Parallel.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ScanDns CheckDnsInParallelAsync: " + ex.Message);
        }

        return (dnsItemList, pr);
    }

    /// <summary>
    /// Scan DNS
    /// </summary>
    /// <param name="domain">Domain To Scan</param>
    /// <param name="dnsServer">DNS Address</param>
    /// <param name="timeoutMS">TimeoutMS</param>
    /// <param name="bootstrapIP">Bootstrap IP</param>
    /// <param name="bootstrapPort">Bootstrap Port</param>
    /// <param name="useExternal">Scan DNS By External SDCLookup (External Returns Empty DnsMessage)</param>
    /// <returns></returns>
    public async Task<(DnsItem DnsItem, DnsMessage DnsMessage)> CheckDnsAsync(string domain, string dnsServer, int timeoutMS, IPAddress bootstrapIP, int bootstrapPort, bool useExternal, CancellationToken ct, string? proxyScheme = null)
    {
        DnsItem dnsItemOut = new();
        DnsMessage dnsMessageOut = new();

        try
        {
            // Var
            DnsStatus status = DnsStatus.Unknown;
            int latency = -1;

            // Read
            DnsReader dnsReader = new(dnsServer);
            dnsItemOut.Protocol = dnsReader.ProtocolName;
            if (dnsReader.Protocol == DnsEnums.DnsProtocol.Unknown) return (dnsItemOut, dnsMessageOut);

            // Scan
            if (useExternal)
            {
                bool isDnsOnline = false;
                string args = $"-Domain={domain} -DNSs=\"{dnsServer}\" -TimeoutMS={timeoutMS} -Insecure={Insecure} -BootstrapIP={bootstrapIP} -BootstrapPort={bootstrapPort} -DoubleCheck=True";
                if (!string.IsNullOrEmpty(proxyScheme)) args += $" -ProxyScheme={proxyScheme}";
                var p = await ProcessManager.ExecuteAsync(Pathes.SDCLookup, null, args, true, true, Pathes.BinaryDirPath);
                if (p.IsSeccess)
                {
                    string dnsLookupResult = p.Output;
                    if (!string.IsNullOrEmpty(dnsLookupResult))
                    {
                        try
                        {
                            List<string> lines = dnsLookupResult.SplitToLines();
                            if (lines.Count >= 2)
                            {
                                bool isBool = bool.TryParse(lines[1].Trim(), out bool isDnsOnlineValue);
                                if (isBool) isDnsOnline = isDnsOnlineValue;

                                bool isInt = int.TryParse(lines[0].Trim(), out int latencyValue);
                                if (isInt) latency = isDnsOnline ? latencyValue : -1;
                            }
                        }
                        catch (Exception) { }
                    }
                }
                status = isDnsOnline ? DnsStatus.Online : DnsStatus.Offline;
            }
            else
            {
                bool hasLocalIp = false;
                int aRecordCount = 0;
                DnsMessage dmQ = DnsMessage.CreateQuery(DnsEnums.DnsProtocol.UDP, domain, DnsEnums.RRType.A, DnsEnums.CLASS.IN);
                bool isWriteSuccess = DnsMessage.TryWrite(dmQ, out byte[] dmQBuffer);
                if (isWriteSuccess)
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    byte[] dmABuffer = await DnsClient.QueryAsync(dmQBuffer, DnsEnums.DnsProtocol.UDP, dnsServer, Insecure, bootstrapIP, bootstrapPort, timeoutMS, ct, proxyScheme).ConfigureAwait(false);
                    sw.Stop();
                    latency = Convert.ToInt32(sw.ElapsedMilliseconds);
                    if (dmABuffer.Length >= 12) // 12 Header Length
                    {
                        dnsMessageOut = DnsMessage.Read(dmABuffer, DnsEnums.DnsProtocol.UDP);
                        if (dnsMessageOut.IsSuccess)
                        {
                            //Debug.WriteLine("==========-------------> " + dmA.ToString());
                            if (dnsMessageOut.Header.AnswersCount > 0 && dnsMessageOut.Answers.AnswerRecords.Count > 0)
                            {
                                for (int n = 0; n < dnsMessageOut.Answers.AnswerRecords.Count; n++)
                                {
                                    IResourceRecord irr = dnsMessageOut.Answers.AnswerRecords[n];
                                    if (irr is not ARecord aRecord) continue;
                                    if (NetworkTool.IsLocalIP(aRecord.IP)) hasLocalIp = true;
                                    aRecordCount++;
                                }
                            }
                        }
                    }
                }
                status = !hasLocalIp && aRecordCount > 0 ? DnsStatus.Online : DnsStatus.Offline;
            }

            // Filters
            DnsFilter isGoogleSafeSearchEnabled = DnsFilter.Unknown;
            DnsFilter isBingSafeSearchEnabled = DnsFilter.Unknown;
            DnsFilter isYoutubeRestricted = DnsFilter.Unknown;
            DnsFilter isAdultBlocked = DnsFilter.Unknown;
            if (CheckForFilters && status == DnsStatus.Online)
            {
                var (IsGoogleSafeSearch, IsBingSafeSearch, IsYoutubeRestricted, IsAdultBlocked) = await CheckDnsFiltersAsync(dnsServer, timeoutMS, bootstrapIP, bootstrapPort, ct, proxyScheme).ConfigureAwait(false);
                isGoogleSafeSearchEnabled = IsGoogleSafeSearch;
                isBingSafeSearchEnabled = IsBingSafeSearch;
                isYoutubeRestricted = IsYoutubeRestricted;
                isAdultBlocked = IsAdultBlocked;
            }

            dnsItemOut = new()
            {
                DNS_URL = dnsServer,
                Protocol = dnsReader.ProtocolName,
                Status = status,
                Latency = status == DnsStatus.Online ? latency : -1,
                IsGoogleSafeSearchEnabled = isGoogleSafeSearchEnabled,
                IsBingSafeSearchEnabled = isBingSafeSearchEnabled,
                IsYoutubeRestricted = isYoutubeRestricted,
                IsAdultBlocked = isAdultBlocked
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ScanDns CheckDnsAsync: " + ex.Message);
        }
        
        return (dnsItemOut, dnsMessageOut);
    }

    public async Task GenerateFilterIPsAsync(string uncensoredDNS, int timeoutMS, IPAddress bootstrapIP, int bootstrapPort, string? proxyScheme = null)
    {
        try
        {
            int filterTimeoutMS = timeoutMS + 1000;

            Task google = Task.Run(async () =>
            {
                var google = await GetARecordIPsAsync(FilterDomainGoogle, uncensoredDNS, filterTimeoutMS, bootstrapIP, bootstrapPort, CancellationToken.None, proxyScheme).ConfigureAwait(false);
                if (google.IPv4List.Count > 0 && !HasLocalIP(google.IPv4List))
                {
                    lock (GoogleSafeSearchIpList)
                    {
                        GoogleSafeSearchIpList = google.IPv4List;
                    }
                }
            });

            Task bing = Task.Run(async () =>
            {
                var bing = await GetARecordIPsAsync(FilterDomainBing, uncensoredDNS, filterTimeoutMS, bootstrapIP, bootstrapPort, CancellationToken.None, proxyScheme).ConfigureAwait(false);
                if (bing.IPv4List.Count > 0 && !HasLocalIP(bing.IPv4List))
                {
                    lock (BingSafeSearchIpList)
                    {
                        BingSafeSearchIpList = bing.IPv4List;
                    }
                }
            });

            Task youtube = Task.Run(async () =>
            {
                var youtube = await GetARecordIPsAsync(FilterDomainYoutube, uncensoredDNS, filterTimeoutMS, bootstrapIP, bootstrapPort, CancellationToken.None, proxyScheme).ConfigureAwait(false);
                if (youtube.IPv4List.Count > 0 && !HasLocalIP(youtube.IPv4List))
                {
                    lock (YoutubeRestrictIpList)
                    {
                        YoutubeRestrictIpList = youtube.IPv4List;
                    }
                }
            });

            Task adult = Task.Run(async () =>
            {
                var adult = await GetARecordIPsAsync(FilterDomainAdult, uncensoredDNS, filterTimeoutMS, bootstrapIP, bootstrapPort, CancellationToken.None, proxyScheme).ConfigureAwait(false);
                if (adult.IPv4List.Count > 0 && !HasLocalIP(adult.IPv4List))
                {
                    lock (AdultIpList)
                    {
                        AdultIpList = adult.IPv4List;
                    }
                }
            });

            await Task.WhenAll(google, bing, youtube, adult).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ScanDns GenerateFilterIPsAsync: " + ex.Message);
        }
    }

    private async Task<(DnsFilter IsGoogleSafeSearch, DnsFilter IsBingSafeSearch, DnsFilter IsYoutubeRestricted, DnsFilter IsAdultBlocked)> CheckDnsFiltersAsync(string dnsServer, int timeoutMS, IPAddress bootstrapIP, int bootstrapPort, CancellationToken ct, string? proxyScheme = null)
    {
        DnsFilter isGoogleSafeSearchOut = DnsFilter.Unknown;
        DnsFilter isBingSafeSearchOut = DnsFilter.Unknown;
        DnsFilter isYoutubeRestrictedOut = DnsFilter.Unknown;
        DnsFilter isAdultBlockedOut = DnsFilter.Unknown;

        try
        {
            int filterTimeoutMS = timeoutMS + 1000;

            async Task google()
            {
                // Google
                var google = await GetARecordIPsAsync(FilterDomainGoogle, dnsServer, filterTimeoutMS, bootstrapIP, bootstrapPort, ct, proxyScheme).ConfigureAwait(false);
                if (google.IPv4List.Count > 0)
                {
                    if (GoogleSafeSearchIpList.Count == 0)
                    {
                        var googleS = await GetARecordIPsAsync(FilterDomainGoogleS, dnsServer, filterTimeoutMS, bootstrapIP, bootstrapPort, ct, proxyScheme).ConfigureAwait(false);
                        if (googleS.IPv4List.Count > 0)
                        {
                            bool hasSame = HasSameItem(google.IPv4List, googleS.IPv4List, true);
                            isGoogleSafeSearchOut = hasSame ? DnsFilter.Yes : DnsFilter.No;
                            if (!HasLocalIP(googleS.IPv4List))
                            {
                                lock (GoogleSafeSearchIpList)
                                {
                                    GoogleSafeSearchIpList = googleS.IPv4List;
                                }
                            }
                        }
                    }
                    else
                    {
                        bool hasSame = HasSameItem(google.IPv4List, GoogleSafeSearchIpList, true);
                        isGoogleSafeSearchOut = hasSame ? DnsFilter.Yes : DnsFilter.No;
                    }
                }
            }

            async Task bing()
            {
                // Bing
                var bing = await GetARecordIPsAsync(FilterDomainBing, dnsServer, filterTimeoutMS, bootstrapIP, bootstrapPort, ct, proxyScheme).ConfigureAwait(false);
                if (bing.IPv4List.Count > 0)
                {
                    if (BingSafeSearchIpList.Count == 0)
                    {
                        var bingS = await GetARecordIPsAsync(FilterDomainBingS, dnsServer, filterTimeoutMS, bootstrapIP, bootstrapPort, ct, proxyScheme).ConfigureAwait(false);
                        if (bingS.IPv4List.Count > 0)
                        {
                            bool hasSame = HasSameItem(bing.IPv4List, bingS.IPv4List, true);
                            isBingSafeSearchOut = hasSame ? DnsFilter.Yes : DnsFilter.No;
                            if (!HasLocalIP(bingS.IPv4List))
                            {
                                lock (BingSafeSearchIpList)
                                {
                                    BingSafeSearchIpList = bingS.IPv4List;
                                }
                            }
                        }
                    }
                    else
                    {
                        bool hasSame = HasSameItem(bing.IPv4List, BingSafeSearchIpList, true);
                        isBingSafeSearchOut = hasSame ? DnsFilter.Yes : DnsFilter.No;
                    }
                }
            }

            async Task youtube()
            {
                // Youtube
                var youtube = await GetARecordIPsAsync(FilterDomainYoutube, dnsServer, filterTimeoutMS, bootstrapIP, bootstrapPort, ct, proxyScheme).ConfigureAwait(false);
                if (youtube.IPv4List.Count > 0)
                {
                    if (YoutubeRestrictIpList.Count == 0)
                    {
                        var youtubeR = await GetARecordIPsAsync(FilterDomainYoutubeR, dnsServer, filterTimeoutMS, bootstrapIP, bootstrapPort, ct, proxyScheme).ConfigureAwait(false);
                        var youtubeRM = await GetARecordIPsAsync(FilterDomainYoutubeRM, dnsServer, filterTimeoutMS, bootstrapIP, bootstrapPort, ct, proxyScheme).ConfigureAwait(false);
                        List<IPAddress> youtubeRRMIpList = youtubeR.IPv4List.Concat(youtubeRM.IPv4List).ToList();
                        if (youtubeRRMIpList.Count > 0)
                        {
                            bool hasSame = HasSameItem(youtube.IPv4List, youtubeRRMIpList, true);
                            isYoutubeRestrictedOut = hasSame ? DnsFilter.Yes : DnsFilter.No;
                            if (!HasLocalIP(youtubeRRMIpList))
                            {
                                lock (YoutubeRestrictIpList)
                                {
                                    YoutubeRestrictIpList = youtubeRRMIpList;
                                }
                            }
                        }
                    }
                    else
                    {
                        bool hasSame = HasSameItem(youtube.IPv4List, YoutubeRestrictIpList, true);
                        isYoutubeRestrictedOut = hasSame ? DnsFilter.Yes : DnsFilter.No;
                    }
                }
            }

            async Task adult()
            {
                // Adult
                var adult = await GetARecordIPsAsync(FilterDomainAdult, dnsServer, filterTimeoutMS, bootstrapIP, bootstrapPort, ct, proxyScheme).ConfigureAwait(false);
                if (adult.IPv4List.Count > 0)
                {
                    isAdultBlockedOut = HasLocalIP(adult.IPv4List) ? DnsFilter.Yes : DnsFilter.No;
                    if (isAdultBlockedOut == DnsFilter.No)
                    {
                        bool isIpInRange = false;
                        for (int i = 0; i < adult.IPv4List.Count; i++)
                        {
                            if (isIpInRange) break;
                            IPAddress ip = adult.IPv4List[i];
                            for (int n = 0; n < AdultIpCidrList.Count; n++)
                            {
                                string cidr = AdultIpCidrList[n];
                                if (NetworkTool.IsIpInRange(ip, cidr))
                                {
                                    isIpInRange = true;
                                    break;
                                }
                            }
                        }
                        if (!isIpInRange && AdultIpCidrList.Count > 0) isAdultBlockedOut = DnsFilter.Yes;
                    }
                    if (isAdultBlockedOut == DnsFilter.No)
                    {
                        if (AdultIpList.Count == 0)
                        {
                            string uncensoredDNS = "tcp://8.8.8.8:53";
                            var adultUncensored = await GetARecordIPsAsync(FilterDomainAdult, uncensoredDNS, filterTimeoutMS, bootstrapIP, bootstrapPort, ct, proxyScheme).ConfigureAwait(false);
                            if (adultUncensored.IPv4List.Count > 0)
                            {
                                isAdultBlockedOut = !HasSameItem(adult.IPv4List, adultUncensored.IPv4List, false) ? DnsFilter.Yes : DnsFilter.No;
                                if (!HasLocalIP(adultUncensored.IPv4List))
                                {
                                    lock (AdultIpList)
                                    {
                                        AdultIpList = adultUncensored.IPv4List;
                                    }
                                }
                            }
                        }
                        else
                        {
                            isAdultBlockedOut = !HasSameItem(adult.IPv4List, AdultIpList, false) ? DnsFilter.Yes : DnsFilter.No;
                        }
                    }
                }
                else
                {
                    if (adult.IsSuccess)
                    {
                        // It's Success But Returned No IP
                        isAdultBlockedOut = DnsFilter.Yes;
                    }
                }
            }

            await Task.WhenAll(google(), bing(), youtube(), adult()).ConfigureAwait(false);

            // Recheck If Unknown
            if (!ct.IsCancellationRequested)
            {
                List<Task> recheckList = new();
                if (isGoogleSafeSearchOut == DnsFilter.Unknown) recheckList.Add(google());
                if (isBingSafeSearchOut == DnsFilter.Unknown) recheckList.Add(bing());
                if (isYoutubeRestrictedOut == DnsFilter.Unknown) recheckList.Add(youtube());
                if (isAdultBlockedOut == DnsFilter.Unknown) recheckList.Add(adult());
                if (recheckList.Count > 0) await Task.WhenAll(recheckList).ConfigureAwait(false);
            }

            if (!ct.IsCancellationRequested)
            {
                if (isGoogleSafeSearchOut == DnsFilter.Unknown) await Task.Run(google).ConfigureAwait(false);
                if (isBingSafeSearchOut == DnsFilter.Unknown) await Task.Run(bing).ConfigureAwait(false);
                if (isYoutubeRestrictedOut == DnsFilter.Unknown) await Task.Run(youtube).ConfigureAwait(false);
                if (isAdultBlockedOut == DnsFilter.Unknown) await Task.Run(adult).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ScanDns CheckDnsFiltersAsync: " + ex.Message);
        }

        return (isGoogleSafeSearchOut, isBingSafeSearchOut, isYoutubeRestrictedOut, isAdultBlockedOut);
    }

    private async Task<(bool IsSuccess, List<IPAddress> IPv4List)> GetARecordIPsAsync(string domain, string dnsServer, int timeoutMS, IPAddress bootstrapIP, int bootstrapPort, CancellationToken ct, string? proxyScheme = null)
    {
        bool isSuccess = false;
        List<IPAddress> ips = new();

        try
        {
            DnsMessage dmQ = DnsMessage.CreateQuery(DnsEnums.DnsProtocol.UDP, domain, DnsEnums.RRType.A, DnsEnums.CLASS.IN);
            bool isWriteSuccess = DnsMessage.TryWrite(dmQ, out byte[] dmQBuffer);
            if (isWriteSuccess)
            {
                byte[] dmABuffer = await DnsClient.QueryAsync(dmQBuffer, DnsEnums.DnsProtocol.UDP, dnsServer, Insecure, bootstrapIP, bootstrapPort, timeoutMS, ct, proxyScheme).ConfigureAwait(false);
                DnsMessage dmA = DnsMessage.Read(dmABuffer, DnsEnums.DnsProtocol.UDP);
                isSuccess = dmA.IsSuccess;
                if (isSuccess)
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
            Debug.WriteLine("ScanDns GetARecordIPsAsync: " + ex.Message);
        }

        return (isSuccess, ips);
    }

    private static bool HasSameItem(List<IPAddress> list1, List<IPAddress> list2, bool checkForLocalIPs)
    {
        bool hasSameItem = false;

        try
        {
            for (int i = 0; i < list2.Count; i++)
            {
                if (hasSameItem) break;
                IPAddress ip2 = list2[i];
                for (int j = 0; j < list1.Count; j++)
                {
                    IPAddress ip1 = list1[j];
                    if (ip1.Equals(ip2))
                    {
                        hasSameItem = true;
                        break;
                    }
                    if (checkForLocalIPs && NetworkTool.IsLocalIP(ip1))
                    {
                        hasSameItem = true;
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ScanDns HasSameItem: " + ex.Message);
        }

        return hasSameItem;
    }

    private static bool HasLocalIP(List<IPAddress> list)
    {
        for (int n = 0; n < list.Count; n++)
        {
            IPAddress ip = list[n];
            if (NetworkTool.IsLocalIP(ip)) return true;
        }
        return false;
    }

}