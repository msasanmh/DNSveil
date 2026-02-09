using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using MsmhToolsClass;
using MsmhToolsClass.V2RayConfigTool;
using static DNSveil.Logic.UpstreamServers.UpstreamModel;

namespace DNSveil.Logic.UpstreamServers;

public class ScanUpstream
{
    public struct ParallelResult
    {
        public int OnlineServers { get; set; }
        public int SelectedServers { get; set; }
        public int SumLatencyMS { get; set; }
        public int AverageLatencyMS { get; set; }
        public int ParallelLatencyMS { get; set; }
    }

    public static async Task<(ObservableCollection<UpstreamItem> Items, ParallelResult ParallelResult)> ScanUpstreamInParallelAsync(GroupSettings gs, ObservableCollection<UpstreamItem> items, RegionCache regionCache, CancellationToken ct)
    {
        ParallelResult pr = new();

        try
        {
            // Parallel Latency
            Stopwatch sw_Parallel = Stopwatch.StartNew();

            int gsTimeoutMS = Convert.ToInt32(gs.ScanTimeoutSec * 1000);
            string fragmentStr = $"tlshello,{gs.Fragment.Size},{gs.Fragment.Delay}";
            if (string.IsNullOrEmpty(gs.Fragment.Size) || string.IsNullOrEmpty(gs.Fragment.Delay)) fragmentStr = string.Empty;

            // Parallel Options
            ParallelOptions parallelOptions = new() { CancellationToken = ct };

            // Convert ObservableCollection To Concurrent
            ConcurrentDictionary<uint, UpstreamItem> itemConcurrentDict = items.ToConcurrentDictionary();
            Task parallel = Parallel.ForEachAsync(itemConcurrentDict, parallelOptions, async (itemKV, pCT) =>
            {
                if (pCT.IsCancellationRequested) return;
                UpstreamItem item = itemKV.Value;
                item.IsSelected = UpstreamServersManager.IsUpstreamItemSelectedBySettings(item, gs);

                if (item.IsSelected)
                {
                    if (pCT.IsCancellationRequested) return;
                    item = await ScanUpstreamAsync(gs.TestURL, item, fragmentStr, gsTimeoutMS, gs.BootstrapIP, gs.BootstrapPort, gs.AllowInsecure, gs.TestSpeed, gs.GetRegionInfo, regionCache, pCT).ConfigureAwait(false);
                    if (pCT.IsCancellationRequested) return;
                    
                    if (item.StatusCode == HttpStatusCode.OK)
                    {
                        pr.OnlineServers++;
                        pr.SelectedServers++;
                        pr.SumLatencyMS += item.Latency;

                        // Save DateTime To Description
                        if (string.IsNullOrWhiteSpace(item.Description))
                            item.Description = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}";
                        else
                        {
                            bool isDateTime = DateTime.TryParse(item.Description, out _);
                            if (isDateTime) item.Description = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}";
                        }
                    }
                    else
                    {
                        item.IsSelected = false;
                    }
                }
                else
                {
                    item.StatusShortDescription = "Skipped";
                    item.IsSelected = false;
                }

                itemConcurrentDict.TryUpdate(itemKV.Key, item);
            });
            try { await parallel.WaitAsync(ct).ConfigureAwait(false); } catch (Exception) { }

            // Convert Back To ObservableCollection
            items = itemConcurrentDict.Select(_ => _.Value).ToObservableCollection();
            itemConcurrentDict.Clear();

            // Set Average Latency
            pr.AverageLatencyMS = pr.OnlineServers > 0 ? pr.SumLatencyMS / pr.OnlineServers : 0;

            sw_Parallel.Stop();
            pr.ParallelLatencyMS = Convert.ToInt32(sw_Parallel.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ScanUpstream ScanUpstreamInParallelAsync: " + ex.GetInnerExceptions());
        }

        return (items, pr);
    }

    public static async Task<UpstreamItem> ScanUpstreamAsync(string testUrl, UpstreamItem upstreamItem, string fragmentStr, int timeoutMS, IPAddress bootstrapIP, int bootstrapPort, bool allowInsecure, bool testSpeed, bool getRegionInfo, RegionCache regionCache, CancellationToken ct)
    {
        try
        {
            bool is_Http_Socks5 = false;
            string urlOrJson = upstreamItem.ConfigInfo.UrlOrJson;
            if (urlOrJson.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || urlOrJson.StartsWith("socks5://", StringComparison.OrdinalIgnoreCase))
            {
                NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(urlOrJson, 80);
                if (NetworkTool.IsIP(urid.Host, out _)) is_Http_Socks5 = true;
            }

            if (is_Http_Socks5)
                return await ScanUpstream_Internal_Async(testUrl, upstreamItem, timeoutMS, allowInsecure, testSpeed, getRegionInfo, regionCache, ct).ConfigureAwait(false);
            else return await ScanUpstream_Xray_Async(testUrl, upstreamItem, fragmentStr, timeoutMS, bootstrapIP, bootstrapPort, allowInsecure, testSpeed, getRegionInfo, regionCache, ct).ConfigureAwait(false);

        }
        catch (Exception ex)
        {
            upstreamItem.StatusDescription = ex.GetInnerExceptions();
            Debug.WriteLine("ScanUpstream ScanUpstreamAsync: " + upstreamItem.StatusDescription);
        }

        return upstreamItem;
    }

    private static async Task<UpstreamItem> ScanUpstream_Internal_Async(string testUrl, UpstreamItem upstreamItem, int timeoutMS, bool allowInsecure, bool testSpeed, bool getRegionInfo, RegionCache regionCache, CancellationToken ct)
    {
        try
        {
            // HTTP And SOCKS5
            string proxyScheme = upstreamItem.ConfigInfo.UrlOrJson;

            Stopwatch sw = Stopwatch.StartNew();
            HttpRequestResponse hrr = await NetworkTool.GetHttpRequestResponseAsync(testUrl, null, timeoutMS, allowInsecure, false, false, proxyScheme, null, null, ct);
            sw.Stop();

            upstreamItem.StatusCode = hrr.StatusCode;
            upstreamItem.StatusCodeNumber = hrr.StatusCodeNumber;
            upstreamItem.Latency = upstreamItem.StatusCode == HttpStatusCode.OK ? Convert.ToInt32(sw.ElapsedMilliseconds) : -1;
            if (upstreamItem.StatusCode == HttpStatusCode.OK)
            {
                // Stability
                int stabilityPercentRate = 25;
                upstreamItem.StabilityPercent = stabilityPercentRate;
                if (upstreamItem.Latency <= 4000)
                {
                    Task<HttpRequestResponse> task1 = NetworkTool.GetHttpRequestResponseAsync(testUrl, null, upstreamItem.Latency + 100, allowInsecure, false, false, proxyScheme, null, null, ct);
                    Task<HttpRequestResponse> task2 = NetworkTool.GetHttpRequestResponseAsync(testUrl, null, upstreamItem.Latency + 150, allowInsecure, false, false, proxyScheme, null, null, ct);
                    Task<HttpRequestResponse> task3 = NetworkTool.GetHttpRequestResponseAsync(testUrl, null, upstreamItem.Latency + 200, allowInsecure, false, false, proxyScheme, null, null, ct);
                    HttpRequestResponse[] hrrs = await Task.WhenAll(task1, task2, task3).ConfigureAwait(false);
                    foreach (HttpRequestResponse hrr2 in hrrs)
                    {
                        if (hrr2.StatusCode == HttpStatusCode.OK) upstreamItem.StabilityPercent += stabilityPercentRate;
                    }
                }

                if (testSpeed && upstreamItem.StabilityPercent >= 50)
                {
                    // Test Speed DL
                    upstreamItem.DLSpeed = await WebAPI.Cloudflare_TestDownloadSpeed_Async(proxyScheme, null, null, 1000000, ct).ConfigureAwait(false);

                    // Test Speed UL
                    upstreamItem.ULSpeed = await WebAPI.Cloudflare_TestUploadSpeed_Async(proxyScheme, null, null, 500000, ct).ConfigureAwait(false);
                }
                else
                {
                    upstreamItem.DLSpeed = 0;
                    upstreamItem.ULSpeed = 0;
                }

                // Get Region
                if (getRegionInfo && upstreamItem.StabilityPercent >= 50)
                    upstreamItem.Region = await GetRegionInfoAsync(upstreamItem.IDUniqueWithRemarks, proxyScheme, regionCache, ct).ConfigureAwait(false);
            }
            else
            {
                upstreamItem.StabilityPercent = 0;
                upstreamItem.DLSpeed = 0;
                upstreamItem.ULSpeed = 0;
            }

            upstreamItem.StatusShortDescription = $"{upstreamItem.StatusCodeNumber} {upstreamItem.StatusCode}";
            upstreamItem.StatusDescription = $"{upstreamItem.StatusCodeNumber} {upstreamItem.StatusCode}, Latency: {upstreamItem.Latency} ms.";
            if (hrr.StatusCode != HttpStatusCode.OK) upstreamItem.StatusDescription += $"{Environment.NewLine}{hrr.StatusDescription}";
        }
        catch (Exception ex)
        {
            upstreamItem.StatusDescription = ex.GetInnerExceptions();
            Debug.WriteLine("ScanUpstream ScanUpstream_Internal_Async: " + upstreamItem.StatusDescription);
        }

        return upstreamItem;
    }

    private static async Task<UpstreamItem> ScanUpstream_Xray_Async(string testUrl, UpstreamItem upstreamItem, string fragmentStr, int timeoutMS, IPAddress bootstrapIP, int bootstrapPort, bool allowInsecure, bool testSpeed, bool getRegionInfo, RegionCache regionCache, CancellationToken ct)
    {
        try
        {
            string urlOrJson = upstreamItem.ConfigInfo.UrlOrJson;

            // Get Protocol By URL
            ConfigBuilder.Protocol protocol = ConfigBuilder.GetProtocolByUrl(urlOrJson);

            // Get 3 Free Ports
            List<int> freePorts = await NetworkTool.GetFreePortsAsync(3);
            int listeningDnsPort = -1, listeningHttpPort = -1, listeningSocksPort = -1;
            if (freePorts.Count == 3)
            {
                listeningDnsPort = freePorts[0];
                listeningHttpPort = freePorts[1];
                listeningSocksPort = freePorts[2];
            }

            // Check We Have 3 Free Ports
            if (listeningDnsPort == -1 || listeningHttpPort == -1 || listeningSocksPort == -1)
            {
                string msg = "Couldn't Get 3 Free Ports!";
                upstreamItem.StatusDescription = msg;
                return upstreamItem;
            }

            string json;
            if (protocol == ConfigBuilder.Protocol.Unknown)
            {
                json = urlOrJson;
                // Check JSON Is Valid
                bool isJsonValid1 = JsonTool.IsValid(json);
                if (!isJsonValid1)
                {
                    string msg = "JSON Is Not Valid.";
                    upstreamItem.StatusDescription = msg;
                    return upstreamItem;
                }

                XrayConfig? xrayConfig = await ConfigBuilder.BuildFromJsonAsync(json);
                if (xrayConfig != null)
                {
                    for (int n = 0; n < xrayConfig.Inbounds.Count; n++)
                    {
                        XrayConfig.ConfigInbound inbound = xrayConfig.Inbounds[n];
                        if (inbound.Protocol.Equals(XrayConfig.ConfigInbound.Get.Protocol.Dokodemo_door, StringComparison.OrdinalIgnoreCase))
                        {
                            inbound.Port = listeningDnsPort;
                        }
                        else if (inbound.Protocol.Equals(XrayConfig.ConfigInbound.Get.Protocol.Http, StringComparison.OrdinalIgnoreCase))
                        {
                            inbound.Port = listeningHttpPort;
                        }
                        else if (inbound.Protocol.Equals(XrayConfig.ConfigInbound.Get.Protocol.Socks, StringComparison.OrdinalIgnoreCase))
                        {
                            inbound.Port = listeningSocksPort;
                        }
                        else if (inbound.Protocol.Equals(XrayConfig.ConfigInbound.Get.Protocol.Mixed, StringComparison.OrdinalIgnoreCase))
                        {
                            inbound.Port = listeningSocksPort;
                        }
                    }

                    // Convert Back To JSON
                    json = await ConfigBuilder.BuildJsonAsync(xrayConfig);
                }
            }
            else
            {
                XrayConfig xrayConfig = ConfigBuilder.Build(urlOrJson, listeningDnsPort, listeningSocksPort, fragmentStr, bootstrapIP, bootstrapPort, false, "https://every1dns.com/dns-query");
                json = await ConfigBuilder.BuildJsonAsync(xrayConfig);
            }

            // Check JSON Is Valid
            bool isJsonValid2 = JsonTool.IsValid(json);
            if (!isJsonValid2)
            {
                string msg = "JSON Is Not Valid.";
                upstreamItem.StatusDescription = msg;
                return upstreamItem;
            }

            string configTempPath = Pathes.RandomJsonPath;

            // Write JSON To File
            await File.WriteAllTextAsync(configTempPath, json, ct);

            string args = $"run -config=\"{configTempPath}\"";
            int pid = ProcessManager.ExecuteOnly(Pathes.Xray, null, args, true, true, Pathes.BinaryDir, ProcessPriorityClass.Normal);
            await Task.Delay(500, ct);
            string proxyScheme = $"socks5://{IPAddress.Loopback}:{listeningSocksPort}";
            Stopwatch sw = Stopwatch.StartNew();
            HttpRequestResponse hrr = await NetworkTool.GetHttpRequestResponseAsync(testUrl, null, timeoutMS, allowInsecure, false, false, proxyScheme, null, null, ct);
            sw.Stop();

            // Kill Xray And Delete Config File On Cancellation
            if (ct.IsCancellationRequested)
            {
                await ProcessManager.KillProcessByPidAsync(pid);
                try { File.Delete(configTempPath); } catch (Exception) { }
            }

            upstreamItem.StatusCode = hrr.StatusCode;
            upstreamItem.StatusCodeNumber = hrr.StatusCodeNumber;
            upstreamItem.Latency = upstreamItem.StatusCode == HttpStatusCode.OK ? Convert.ToInt32(sw.ElapsedMilliseconds) : -1;
            if (upstreamItem.StatusCode == HttpStatusCode.OK)
            {
                // Stability
                int stabilityPercentRate = 25;
                upstreamItem.StabilityPercent = stabilityPercentRate;
                if (upstreamItem.Latency <= 4000)
                {
                    Task<HttpRequestResponse> task1 = NetworkTool.GetHttpRequestResponseAsync(testUrl, null, upstreamItem.Latency + 100, allowInsecure, false, false, proxyScheme, null, null, ct);
                    Task<HttpRequestResponse> task2 = NetworkTool.GetHttpRequestResponseAsync(testUrl, null, upstreamItem.Latency + 150, allowInsecure, false, false, proxyScheme, null, null, ct);
                    Task<HttpRequestResponse> task3 = NetworkTool.GetHttpRequestResponseAsync(testUrl, null, upstreamItem.Latency + 200, allowInsecure, false, false, proxyScheme, null, null, ct);
                    HttpRequestResponse[] hrrs = await Task.WhenAll(task1, task2, task3).ConfigureAwait(false);
                    foreach (HttpRequestResponse hrr2 in hrrs)
                    {
                        if (hrr2.StatusCode == HttpStatusCode.OK) upstreamItem.StabilityPercent += stabilityPercentRate;
                    }
                }

                if (testSpeed && upstreamItem.StabilityPercent >= 50)
                {
                    // Test Speed DL
                    upstreamItem.DLSpeed = await WebAPI.Cloudflare_TestDownloadSpeed_Async(proxyScheme, null, null, 1000000, ct).ConfigureAwait(false);

                    // Test Speed UL
                    upstreamItem.ULSpeed = await WebAPI.Cloudflare_TestUploadSpeed_Async(proxyScheme, null, null, 500000, ct).ConfigureAwait(false);
                }
                else
                {
                    upstreamItem.DLSpeed = 0;
                    upstreamItem.ULSpeed = 0;
                }
                
                // Get Region
                if (getRegionInfo && upstreamItem.StabilityPercent >= 50)
                    upstreamItem.Region = await GetRegionInfoAsync(upstreamItem.IDUniqueWithRemarks, proxyScheme, regionCache, ct).ConfigureAwait(false);
            }
            else
            {
                upstreamItem.StabilityPercent = 0;
                upstreamItem.DLSpeed = 0;
                upstreamItem.ULSpeed = 0;
            }

            upstreamItem.StatusShortDescription = $"{upstreamItem.StatusCodeNumber} {upstreamItem.StatusCode}";
            upstreamItem.StatusDescription = $"{upstreamItem.StatusCodeNumber} {upstreamItem.StatusCode}, Latency: {upstreamItem.Latency} ms.";
            if (hrr.StatusCode != HttpStatusCode.OK) upstreamItem.StatusDescription += $"{Environment.NewLine}{hrr.StatusDescription}";

            // Kill Xray And Delete Config File
            await ProcessManager.KillProcessByPidAsync(pid);
            try { File.Delete(configTempPath); } catch (Exception) { }
        }
        catch (Exception ex)
        {
            upstreamItem.StatusDescription = ex.GetInnerExceptions();
            Debug.WriteLine("ScanUpstream ScanUpstream_Xray_Async: " + upstreamItem.StatusDescription);
        }

        return upstreamItem;
    }

    private static async Task<CultureTool.RegionResult> GetRegionInfoAsync(string idUnique, string proxyScheme, RegionCache regionCache, CancellationToken ct)
    {
        CultureTool.RegionResult rr = new();
        try
        {
            bool iscached = regionCache.TryGet(idUnique, out CultureTool.RegionResult? cachedRI);
            if (iscached && cachedRI != null)
            {
                // Use Cache
                rr = cachedRI;
            }
            else
            {
                // Get Online
                if (!rr.IsFilledWithData)
                {
                    RegionInfo ri = await WebAPI.GetRegionInfoAsync(5000, proxyScheme, null, null, ct).ConfigureAwait(false);
                    rr = new CultureTool.RegionResult(ri);
                }

                // Add To Cache
                if (rr.IsFilledWithData)
                    regionCache.TryAdd(idUnique, rr);
            }
        }
        catch (Exception) { }
        return rr;
    }

}