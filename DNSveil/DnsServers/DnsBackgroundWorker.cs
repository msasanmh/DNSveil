using System.Diagnostics;
using System.IO;
using System.Net;
using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using static DNSveil.DnsServers.EnumsAndStructs;
using GroupItem = DNSveil.DnsServers.EnumsAndStructs.GroupItem;

namespace DNSveil.DnsServers;

public partial class DnsServersManager
{
    // Prevent Duplicate Subscriptions
    private event EventHandler<BackgroundWorkerEventArgs>? POnBackgroundUpdateReceived;
    public event EventHandler<BackgroundWorkerEventArgs>? OnBackgroundUpdateReceived
    {
        add { POnBackgroundUpdateReceived = value; } // Using = Instead Of +=
        remove { if (POnBackgroundUpdateReceived != null) POnBackgroundUpdateReceived -= value; }
    }

    public bool IsScanning { get; set; } = false;
    private bool StopScanning { get; set; } = false;

    public BackgroundWorkerEventArgs GetBackgroundWorkerStatus { get; private set; } = new();

    public class BackgroundWorkerEventArgs : EventArgs
    {
        public bool IsWorking { get; set; } = false;
        public string ButtonText { get; set; } = "Scan";
        public bool ButtonEnable { get; set; } = true;
        public string Description { get; set; } = "Ready To Work";
        public GroupItem GroupItem { get; set; } = new();
        public int ProgressMin { get; set; } = 0;
        public int ProgressMax { get; set; } = 0;
        public int ProgressValue { get; set; } = 0;
        public int ProgressPercentage
        {
            get
            {
                return ProgressMax > 0 ? (ProgressValue * 100 / ProgressMax) : 0;
            }
        }
        public int OnlineServers { get; set; } = 0;
        public int SelectedServers { get; set; } = 0;
        public int SumLatencyMS { get; set; } = 0;
        public int AverageLatencyMS
        {
            get
            {
                return OnlineServers > 0 ? (SumLatencyMS / OnlineServers) : (-1);
            }
        }
        public int LastIndex { get; set; } = -1;
        public int ParallelLatencyMS { get; set; } = 0;
    }

    /// <summary>
    /// Scan DnsItems In A Group
    /// </summary>
    /// <param name="groupItem">Group Item</param>
    /// <param name="selectedDnsItemList">NULL: All DnsItems In The Group</param>
    /// <param name="isBackground">True: Do Not Update UI</param>
    /// <returns></returns>
    private async Task ScanServersAsync(GroupItem groupItem, List<DnsItem>? selectedDnsItemList, bool isBackground)
    {
        try
        {
            BackgroundWorkerEventArgs bw = new()
            {
                IsWorking = true,
                GroupItem = groupItem,
                LastIndex = -1
            };
            bool isCustomRange = selectedDnsItemList != null;

            if (!IsScanning && groupItem.Mode != GroupMode.None)
            {
                IsScanning = true;
                bw.ButtonText = "Stop";
                bw.ButtonEnable = true;
                bw.Description = "Scanning...";

                // Get Group Settings
                GroupSettings gs = Get_GroupSettings(groupItem.Name);
                if (isBackground && gs.ParallelSize > 5) gs.ParallelSize = 5; // Limit Parallel Size For Background Task
                if (isBackground && gs.ParallelSize > gs.MaxServersToConnect) gs.ParallelSize = gs.MaxServersToConnect;
                int gsTimeoutMS = Convert.ToInt32(gs.TimeoutSec * 1000);

                if (groupItem.Mode == GroupMode.Subscription)
                {
                    // Clear Previous Result
                    if (!isCustomRange) await Clear_DnsItems_Result_Async(groupItem.Name, true, false);

                    selectedDnsItemList ??= Get_DnsItems(groupItem.Name);
                    
                    if (!isBackground)
                    {
                        bw.ProgressMax = selectedDnsItemList.Count;
                        GetBackgroundWorkerStatus = bw;
                        POnBackgroundUpdateReceived?.Invoke(Owner, bw);
                    }
                    
                    // Get Group Options
                    SubscriptionOptions options = Get_Subscription_Options(groupItem.Name);

                    var selectedDnsItemLists = selectedDnsItemList.SplitToLists(gs.ParallelSize);

                    ScanDns scanDns = new(gs.AllowInsecure, true);
                    for (int n = 0; n < selectedDnsItemLists.Count; n++)
                    {
                        if (StopScanning) break;
                        if (isBackground && bw.SelectedServers >= gs.MaxServersToConnect) break;
                        if (isBackground && PauseBackgroundTask) break;
                        
                        List<DnsItem> dnsItemList = selectedDnsItemLists[n];
                        var cdp = await scanDns.CheckDnsInParallelAsync(gs.LookupDomain, dnsItemList, options.FilterByProtocols, options.FilterByProperties, gsTimeoutMS, gs.BootstrapIP, gs.BootstrapPort, false);
                        dnsItemList = cdp.DnsItemList;
                        bw.OnlineServers += cdp.ParallelResult.OnlineServers;
                        bw.SelectedServers += cdp.ParallelResult.SelectedServers;
                        bw.SumLatencyMS += cdp.ParallelResult.SumLatencyMS;
                        bw.ProgressValue += dnsItemList.Count;
                        bw.ParallelLatencyMS = cdp.ParallelResult.ParallelLatencyMS;

                        await Update_DnsItems_Async(groupItem.Name, dnsItemList, true);

                        if (bw.OnlineServers > 0)
                            await Update_LastAutoUpdate_Async(groupItem.Name, new LastAutoUpdate(DateTime.MinValue, DateTime.Now), true);

                        if (!isBackground)
                        {
                            if (n == selectedDnsItemLists.Count - 1) bw.ProgressValue = bw.ProgressMax;
                            try
                            {
                                if (isCustomRange)
                                    bw.LastIndex = Get_IndexOf_DnsItem(groupItem.Name, dnsItemList[^1]);
                                else bw.LastIndex = bw.ProgressValue - 1;
                            }
                            catch (Exception) { }

                            if (!StopScanning && n != selectedDnsItemLists.Count - 1) // If It's Not Last Round
                            {
                                GetBackgroundWorkerStatus = bw;
                                POnBackgroundUpdateReceived?.Invoke(Owner, bw);
                            }
                        }
                    }
                }
                else if (groupItem.Mode == GroupMode.AnonymizedDNSCrypt)
                {
                    // Clear Previous Result
                    if (!isCustomRange) await Clear_DnsItems_Async(groupItem.Name, true);

                    // Get Relays
                    List<string> relays = Get_AnonDNSCrypt_Relays(groupItem.Name);

                    // Get Targets
                    List<DnsItem> targetsAsDnsItem = new();
                    if (selectedDnsItemList != null)
                    {
                        for (int n = 0; n < selectedDnsItemList.Count; n++)
                        {
                            // Split Target From Old Relay
                            string[] split = selectedDnsItemList[n].DNS_URL.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            if (split.Length > 0)
                            {
                                string target = split[0].Trim();
                                DnsItem newTargetAsDnsItem = selectedDnsItemList[n];
                                newTargetAsDnsItem.DNS_URL = target;
                                targetsAsDnsItem.Add(newTargetAsDnsItem);
                            }
                        }
                    }
                    else
                    {
                        List<string> targets = Get_AnonDNSCrypt_Targets(groupItem.Name);
                        for (int n = 0; n < targets.Count; n++)
                        {
                            string target = targets[n];
                            DnsItem dnsItem = new() { DNS_URL = target };
                            targetsAsDnsItem.Add(dnsItem);
                        }
                    }

                    if (!isBackground)
                    {
                        bw.ProgressMax = targetsAsDnsItem.Count;
                        GetBackgroundWorkerStatus = bw;
                        POnBackgroundUpdateReceived?.Invoke(Owner, bw);
                    }
                    
                    // Get Group Options
                    AnonDNSCryptOptions options = Get_AnonDNSCrypt_Options(groupItem.Name);

                    var targetsAsDnsItemList = targetsAsDnsItem.SplitToLists(gs.ParallelSize);

                    ScanDns scanDns = new(gs.AllowInsecure, true);
                    for (int n = 0; n < targetsAsDnsItemList.Count; n++)
                    {
                        if (StopScanning) break;
                        if (isBackground && bw.SelectedServers >= gs.MaxServersToConnect) break;
                        if (isBackground && PauseBackgroundTask) break;
                        
                        List<DnsItem> targetAsDnsItemList = targetsAsDnsItemList[n];
                        var cdp = await scanDns.CheckDNSCryptInParallelAsync(gs.LookupDomain, targetAsDnsItemList, relays, gs.ParallelSize, options.FilterByProperties, gsTimeoutMS, gs.BootstrapIP, gs.BootstrapPort, false);
                        bw.OnlineServers += cdp.ParallelResult.OnlineServers;
                        bw.SelectedServers += cdp.ParallelResult.SelectedServers;
                        bw.SumLatencyMS += cdp.ParallelResult.SumLatencyMS;
                        bw.ProgressValue += cdp.DnsItemElementsList.Count;
                        bw.ParallelLatencyMS = cdp.ParallelResult.ParallelLatencyMS;

                        if (isCustomRange) await Update_DnsItems_Async(groupItem.Name, cdp.DnsItemElementsList, true);
                        else await Append_DnsItems_Async(groupItem.Name, cdp.DnsItemElementsList, true);

                        if (bw.OnlineServers > 0)
                            await Update_LastAutoUpdate_Async(groupItem.Name, new LastAutoUpdate(DateTime.MinValue, DateTime.Now), true);

                        if (!isBackground)
                        {
                            if (n == targetsAsDnsItemList.Count - 1) bw.ProgressValue = bw.ProgressMax;
                            try
                            {
                                if (isCustomRange)
                                    bw.LastIndex = Get_IndexOf_DnsItem(groupItem.Name, cdp.DnsItemElementsList[^1]);
                                else bw.LastIndex = bw.ProgressValue - 1;
                            }
                            catch (Exception) { }

                            if (!StopScanning && n != targetsAsDnsItemList.Count - 1) // If It's Not Last Round
                            {
                                GetBackgroundWorkerStatus = bw;
                                POnBackgroundUpdateReceived?.Invoke(Owner, bw);
                            }
                        }
                    }
                }
                else if (groupItem.Mode == GroupMode.FragmentDoH)
                {
                    // Clear Previous Result
                    if (!isCustomRange) await Clear_DnsItems_Result_Async(groupItem.Name, true, false);

                    selectedDnsItemList ??= Get_DnsItems(groupItem.Name);

                    if (!isBackground)
                    {
                        bw.Description = "Starting Fragment Proxy Server...";
                        bw.ProgressMax = selectedDnsItemList.Count;
                        GetBackgroundWorkerStatus = bw;
                        POnBackgroundUpdateReceived?.Invoke(Owner, bw);
                    }
                    
                    // Get Group Options
                    FragmentDoHOptions options = Get_FragmentDoH_Options(groupItem.Name);

                    // Create Proxy Server: Create Fragment Program
                    AgnosticProgram.Fragment serverFragment = new();
                    serverFragment.Set(AgnosticProgram.Fragment.Mode.Program, options.FragmentSettings.ChunksBeforeSNI, options.FragmentSettings.SniChunkMode, options.FragmentSettings.ChunksSNI, options.FragmentSettings.AntiPatternOffset, options.FragmentSettings.FragmentDelayMS);

                    // Create Proxy Server: Create Rules
                    string rulesStr = string.Empty;
                    for (int n = 0; n < selectedDnsItemList.Count; n++)
                    {
                        DnsItem di = selectedDnsItemList[n];
                        NetworkTool.GetUrlDetails(di.DNS_URL, 443, out _, out string host, out _, out _, out _, out _, out _);
                        rulesStr += $"\n{host}|{di.DNS_IP};";
                        rulesStr += $"\n{di.DNS_IP}|+;";
                    }
                    rulesStr += $"\n{IPAddress.Loopback}|-;"; // Block Loopback IPv4
                    rulesStr += $"\n{IPAddress.IPv6Loopback}|-;"; // Block Loopback IPv6
                    AgnosticProgram.Rules serverRules = new();
                    await serverRules.SetAsync(AgnosticProgram.Rules.Mode.Text, rulesStr);

                    // Create Proxy Server: Create Settings
                    AgnosticSettings serverSettings = new()
                    {
                        AllowInsecure = false,
                        DnsTimeoutSec = 10,
                        KillOnCpuUsage = 40,
                        MaxRequests = 1000000,
                        ListenerPort = await NetworkTool.GetAFreePortAsync(),
                        Working_Mode = AgnosticSettings.WorkingMode.DnsAndProxy
                    };

                    // Create Proxy Server
                    MsmhAgnosticServer? server = new();
                    server.EnableFragment(serverFragment);
                    server.EnableRules(serverRules);
                    await server.StartAsync(serverSettings);

                    string proxyScheme = $"socks5://{IPAddress.Loopback}:{serverSettings.ListenerPort}";

                    if (!isBackground)
                    {
                        bw.Description = "Scanning...";
                        GetBackgroundWorkerStatus = bw;
                        POnBackgroundUpdateReceived?.Invoke(Owner, bw);
                    }
                    
                    var selectedDnsItemLists = selectedDnsItemList.SplitToLists(gs.ParallelSize);

                    ScanDns scanDns = new(gs.AllowInsecure, true);
                    for (int n = 0; n < selectedDnsItemLists.Count; n++)
                    {
                        if (StopScanning) break;
                        if (isBackground && bw.SelectedServers >= gs.MaxServersToConnect) break;
                        if (isBackground && PauseBackgroundTask) break;

                        List<DnsItem> dnsItemList = selectedDnsItemLists[n];
                        var cdp = await scanDns.CheckDnsInParallelAsync(gs.LookupDomain, dnsItemList, options.FilterByProperties, gsTimeoutMS, IPAddress.Loopback, serverSettings.ListenerPort, false, proxyScheme);
                        dnsItemList = cdp.DnsItemList;
                        bw.OnlineServers += cdp.ParallelResult.OnlineServers;
                        bw.SelectedServers += cdp.ParallelResult.SelectedServers;
                        bw.SumLatencyMS += cdp.ParallelResult.SumLatencyMS;
                        bw.ProgressValue += dnsItemList.Count;
                        bw.ParallelLatencyMS = cdp.ParallelResult.ParallelLatencyMS;

                        await Update_DnsItems_Async(groupItem.Name, dnsItemList, true);

                        if (bw.OnlineServers > 0)
                            await Update_LastAutoUpdate_Async(groupItem.Name, new LastAutoUpdate(DateTime.MinValue, DateTime.Now), true);

                        if (!isBackground)
                        {
                            if (n == selectedDnsItemLists.Count - 1) bw.ProgressValue = bw.ProgressMax;
                            try
                            {
                                if (isCustomRange)
                                    bw.LastIndex = Get_IndexOf_DnsItem(groupItem.Name, dnsItemList[^1]);
                                else bw.LastIndex = bw.ProgressValue - 1;
                            }
                            catch (Exception) { }

                            if (!StopScanning && n != selectedDnsItemLists.Count - 1) // If It's Not Last Round
                            {
                                GetBackgroundWorkerStatus = bw;
                                POnBackgroundUpdateReceived?.Invoke(Owner, bw);
                            }
                        }
                    }

                    // Stop Fragment Proxy Server
                    server.Stop();
                    server = null;
                }
                else if (groupItem.Mode == GroupMode.Custom)
                {
                    // Clear Previous Result
                    if (!isCustomRange) await Clear_DnsItems_Result_Async(groupItem.Name, false, false);

                    selectedDnsItemList ??= Get_DnsItems(groupItem.Name);

                    if (!isBackground)
                    {
                        bw.ProgressMax = selectedDnsItemList.Count;
                        GetBackgroundWorkerStatus = bw;
                        POnBackgroundUpdateReceived?.Invoke(Owner, bw);
                    }
                    
                    // Get Group Options
                    CustomOptions options = Get_Custom_Options(groupItem.Name);

                    var selectedDnsItemLists = selectedDnsItemList.SplitToLists(gs.ParallelSize);

                    ScanDns scanDns = new(gs.AllowInsecure, true);
                    for (int n = 0; n < selectedDnsItemLists.Count; n++)
                    {
                        if (StopScanning) break;
                        if (isBackground && bw.SelectedServers >= gs.MaxServersToConnect) break;
                        if (isBackground && PauseBackgroundTask) break;

                        List<DnsItem> dnsItemList = selectedDnsItemLists[n];
                        var cdp = await scanDns.CheckDnsInParallelAsync(gs.LookupDomain, dnsItemList, options.FilterByProtocols, options.FilterByProperties, gsTimeoutMS, gs.BootstrapIP, gs.BootstrapPort, false);
                        dnsItemList = cdp.DnsItemList;
                        bw.OnlineServers += cdp.ParallelResult.OnlineServers;
                        bw.SelectedServers += cdp.ParallelResult.SelectedServers;
                        bw.SumLatencyMS += cdp.ParallelResult.SumLatencyMS;
                        bw.ProgressValue += dnsItemList.Count;
                        bw.ParallelLatencyMS = cdp.ParallelResult.ParallelLatencyMS;

                        await Update_DnsItems_Async(groupItem.Name, dnsItemList, true);

                        if (bw.OnlineServers > 0)
                            await Update_LastAutoUpdate_Async(groupItem.Name, new LastAutoUpdate(DateTime.MinValue, DateTime.Now), true);

                        if (!isBackground)
                        {
                            if (n == selectedDnsItemLists.Count - 1) bw.ProgressValue = bw.ProgressMax;
                            try
                            {
                                if (isCustomRange)
                                    bw.LastIndex = Get_IndexOf_DnsItem(groupItem.Name, dnsItemList[^1]);
                                else bw.LastIndex = bw.ProgressValue - 1;
                            }
                            catch (Exception) { }

                            if (!StopScanning && n != selectedDnsItemLists.Count - 1) // If It's Not Last Round
                            {
                                GetBackgroundWorkerStatus = bw;
                                POnBackgroundUpdateReceived?.Invoke(Owner, bw);
                            }
                        }
                    }
                }
                else
                {
                    await Task.Delay(1000);
                }

                if (!isBackground)
                {
                    bw.ButtonText = "Scan";
                    bw.ButtonEnable = true;
                    bw.Description = StopScanning ? "Scan Canceled." : "Scan Finished.";
                    bw.IsWorking = false;
                    GetBackgroundWorkerStatus = bw;
                    POnBackgroundUpdateReceived?.Invoke(Owner, bw);
                }
                
                StopScanning = false;
                IsScanning = false;
            }
            else
            {
                if (!isBackground)
                {
                    if (bw.GroupItem.Mode == GroupMode.None)
                    {
                        bw.ButtonText = "Scan";
                        bw.ButtonEnable = true;
                        bw.Description = StopScanning ? "Scan Canceled." : "Scan Finished.";
                        bw.IsWorking = false;
                    }
                    else
                    {
                        bw.ButtonText = "Stopping...";
                        bw.ButtonEnable = false;
                        bw.Description = "Stopping Scan Operation...";
                    }
                    
                    GetBackgroundWorkerStatus = bw;
                    POnBackgroundUpdateReceived?.Invoke(Owner, bw);
                }
                
                StopScanning = true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServers DnsBackgroundWorker ScanServersAsync: " + ex.Message);
        }
    }

    /// <summary>
    /// Scan DnsItems In A Group
    /// </summary>
    /// <param name="groupItem">Group Item</param>
    /// <param name="selectedDnsItemList">NULL: All DnsItems In The Group</param>
    public async void ScanServers(GroupItem groupItem, List<DnsItem>? selectedDnsItemList)
    {
        await ScanServersAsync(groupItem, selectedDnsItemList, false);
    }

    private async Task SubscriptionFetchSourceAsync(GroupItem gi)
    {
        try
        {
            List<string> urlsOrFiles = Get_Source_URLs(gi.Name);
            if (urlsOrFiles.Count == 0) return;

            List<string> allDNSs = new();
            for (int n = 0; n < urlsOrFiles.Count; n++)
            {
                string urlOrFile = urlsOrFiles[n];
                List<string> dnss = await LibIn.GetServersFromLinkAsync(urlOrFile, 20000);
                allDNSs.AddRange(dnss);
            }
            if (PauseBackgroundTask) return;

            // DeDup
            allDNSs = allDNSs.Distinct().ToList();
            if (allDNSs.Count == 0) return;

            // Add To Group => DnsItems Element
            await Add_DnsItems_Async(gi.Name, allDNSs, false);
            // Update Last AutoUpdate
            await Update_LastAutoUpdate_Async(gi.Name, new LastAutoUpdate(DateTime.Now, DateTime.MinValue), true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServers DnsBackgroundWorker SubscriptionFetchSourceAsync: " + ex.Message);
        }
    }

    private async Task AnonDNSCryptFetchSourceAsync(GroupItem gi)
    {
        try
        {
            List<string> relayUrlsOrFiles = Get_AnonDNSCrypt_Relay_URLs(gi.Name);
            List<string> targetUrlsOrFiles = Get_AnonDNSCrypt_Target_URLs(gi.Name);
            if (relayUrlsOrFiles.Count == 0 || targetUrlsOrFiles.Count == 0) return;

            List<string> allRelays = new();
            for (int n = 0; n < relayUrlsOrFiles.Count; n++)
            {
                string relayUrlOrFile = relayUrlsOrFiles[n];
                List<string> relays = await LibIn.GetServersFromLinkAsync(relayUrlOrFile, 20000);
                for (int i = 0; i < relays.Count; i++)
                {
                    string relay = relays[i];
                    DnsReader dnsReader = new(relay);
                    if (dnsReader.Protocol == DnsEnums.DnsProtocol.AnonymizedDNSCryptRelay ||
                        dnsReader.Protocol == DnsEnums.DnsProtocol.UDP ||
                        dnsReader.Protocol == DnsEnums.DnsProtocol.TCP)
                    {
                        allRelays.Add(dnsReader.Dns);
                    }
                }
            }
            if (PauseBackgroundTask) return;

            // DeDup Relays
            allRelays = allRelays.Distinct().ToList();
            if (allRelays.Count == 0) return;

            List<string> allTargets = new();
            for (int n = 0; n < targetUrlsOrFiles.Count; n++)
            {
                string targetUrlOrFile = targetUrlsOrFiles[n];
                List<string> targets = await LibIn.GetServersFromLinkAsync(targetUrlOrFile, 20000);
                for (int i = 0; i < targets.Count; i++)
                {
                    string target = targets[i];
                    DnsReader dnsReader = new(target);
                    if (dnsReader.Protocol == DnsEnums.DnsProtocol.DnsCrypt)
                    {
                        allTargets.Add(dnsReader.Dns);
                    }
                }
            }
            if (PauseBackgroundTask) return;

            // DeDup Targets
            allTargets = allTargets.Distinct().ToList();
            if (allTargets.Count == 0) return;

            // Add To Group => RelayItems Element
            await Add_AnonDNSCrypt_RelayItems_Async(gi.Name, allRelays, false);
            // Add To Group => TargetItems Element
            await Add_AnonDNSCrypt_TargetItems_Async(gi.Name, allTargets, false);
            // Update Last AutoUpdate
            await Update_LastAutoUpdate_Async(gi.Name, new LastAutoUpdate(DateTime.Now, DateTime.MinValue), true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServers DnsBackgroundWorker AnonDNSCryptFetchSourceAsync: " + ex.Message);
        }
    }

    private async Task FragmentDoHFetchSourceAsync(GroupItem gi)
    {
        try
        {
            List<string> urlsOrFiles = Get_Source_URLs(gi.Name);
            if (urlsOrFiles.Count == 0) return;

            List<DnsItem> allDoHStamps = new();
            for (int n = 0; n < urlsOrFiles.Count; n++)
            {
                string urlOrFile = urlsOrFiles[n];
                List<string> dnss = await LibIn.GetServersFromLinkAsync(urlOrFile, 20000);
                for (int i = 0; i < dnss.Count; i++)
                {
                    string dns = dnss[i];
                    DnsReader dnsReader = new(dns);
                    if (dnsReader.IsDnsCryptStamp && dnsReader.Protocol == DnsEnums.DnsProtocol.DoH)
                    {
                        if (!dnsReader.IsHostIP && !NetworkTool.IsLocalIP(dnsReader.StampReader.IP.ToString()))
                        {
                            string dns_URL = $"{dnsReader.Scheme}{dnsReader.Host}{dnsReader.Path}";
                            if (dnsReader.Port != 443) dns_URL = $"{dnsReader.Scheme}{dnsReader.Host}:{dnsReader.Port}{dnsReader.Path}";
                            IPAddress dns_IP = dnsReader.StampReader.IP;
                            DnsItem di = new() { DNS_URL = dns_URL, DNS_IP = dns_IP, Protocol = dnsReader.ProtocolName };
                            allDoHStamps.Add(di);
                        }
                    }
                }
            }
            if (PauseBackgroundTask) return;

            // DeDup Stamps
            allDoHStamps = allDoHStamps.DistinctBy(x => x.DNS_URL).ToList();
            if (allDoHStamps.Count == 0) return;

            // Add To Group => DnsItems Element
            await Add_DnsItems_Async(gi.Name, allDoHStamps, false);
            // Update Last AutoUpdate
            await Update_LastAutoUpdate_Async(gi.Name, new LastAutoUpdate(DateTime.Now, DateTime.MinValue), true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServers DnsBackgroundWorker FragmentDoHFetchSourceAsync: " + ex.Message);
        }
    }

    private async Task<bool> AutoUpdate_Groups_InternalAsync()
    {
        bool updated = false;
        try
        {
            static void debug(bool isSource, bool isByItems, GroupItem gi, AutoUpdate au, LastAutoUpdate lau)
            {
#if DEBUG
                if (isByItems)
                {
                    if (isSource) Debug.WriteLine($"Background Task =====> UpdateSource ByItems {gi.Name}");
                    else Debug.WriteLine($"Background Task =====> ScanServers ByItems {gi.Name}");
                }
                else
                {
                    if (isSource) Debug.WriteLine($"Background Task =====> UpdateSource ByDate {gi.Name} --- {DateTime.Now - lau.LastUpdateSource} >= {new TimeSpan(au.UpdateSource, 0, 0)}");
                    else Debug.WriteLine($"Background Task =====> ScanServers ByDate {gi.Name} --- {DateTime.Now - lau.LastScanServers} >= {new TimeSpan(au.ScanServers, 0, 0)}");
                }
#endif
            }

            List<GroupItem> groupItems = Get_GroupItems(true);
            for (int n = 0; n < groupItems.Count; n++)
            {
                if (PauseBackgroundTask) return updated;
                GroupItem gi = groupItems[n];
                AutoUpdate au = Get_AutoUpdate(gi.Name);
                LastAutoUpdate lau = Get_LastAutoUpdate(gi.Name);
                DnsItemsInfo dii = Get_DnsItems_Info(gi.Name);
                if (gi.Mode == GroupMode.Subscription)
                {
                    // Update Source
                    if (au.UpdateSource > 0)
                    {
                        bool isSourceByPath = Get_Source_EnableDisable(gi.Name);
                        if (isSourceByPath)
                        {
                            List<string> urlsOrFiles = Get_Source_URLs(gi.Name);
                            bool hasSourcePath = urlsOrFiles.Count > 0;
                            if (hasSourcePath)
                            {
                                bool isUpdateTime_ByItems = dii.TotalServers == 0;
                                bool isUpdateTime_ByDate = (DateTime.Now - lau.LastUpdateSource) >= new TimeSpan(au.UpdateSource, 0, 0);
                                if (isUpdateTime_ByItems || isUpdateTime_ByDate)
                                {
                                    debug(true, isUpdateTime_ByItems, gi, au, lau);
                                    await SubscriptionFetchSourceAsync(gi);
                                    if (PauseBackgroundTask) return updated;
                                }
                            }
                        }
                    }
                    
                    // Update Scan
                    if (au.ScanServers > 0 && dii.TotalServers > 0)
                    {
                        bool isScanTime_ByItems = dii.SelectedServers == 0;
                        bool isScanTime_ByDate = (DateTime.Now - lau.LastScanServers) >= new TimeSpan(au.ScanServers, 0, 0);
                        if (isScanTime_ByItems || isScanTime_ByDate)
                        {
                            debug(false, isScanTime_ByItems, gi, au, lau);
                            await ScanServersAsync(gi, null, true);
                            updated = true;
                            if (PauseBackgroundTask) return updated;
                            await Sort_DnsItems_ByLatency_Async(gi.Name, true); // Sort By Latency
                            updated = true;
                            if (PauseBackgroundTask) return updated;
                        }
                    }
                }
                else if (gi.Mode == GroupMode.AnonymizedDNSCrypt)
                {
                    // Update Source
                    if (au.UpdateSource > 0)
                    {
                        bool isSourceByPath = Get_Source_EnableDisable(gi.Name);
                        if (isSourceByPath)
                        {
                            List<string> relayUrlsOrFiles = Get_AnonDNSCrypt_Relay_URLs(gi.Name);
                            List<string> targetUrlsOrFiles = Get_AnonDNSCrypt_Target_URLs(gi.Name);
                            bool hasSourcePath = relayUrlsOrFiles.Count > 0 && targetUrlsOrFiles.Count > 0;
                            if (hasSourcePath)
                            {
                                bool isUpdateTime_ByItems = dii.TotalServers == 0;
                                bool isUpdateTime_ByDate = (DateTime.Now - lau.LastUpdateSource) >= new TimeSpan(au.UpdateSource, 0, 0);
                                if (isUpdateTime_ByItems || isUpdateTime_ByDate)
                                {
                                    debug(true, isUpdateTime_ByItems, gi, au, lau);
                                    await AnonDNSCryptFetchSourceAsync(gi);
                                    updated = true;
                                    if (PauseBackgroundTask) return updated;
                                }
                            }
                        }
                    }

                    // Update Scan
                    List<string> relays = Get_AnonDNSCrypt_Relays(gi.Name);
                    List<string> targets = Get_AnonDNSCrypt_Targets(gi.Name);
                    bool hasTotalServers = relays.Count > 0 && targets.Count > 0;
                    if (au.ScanServers > 0 && hasTotalServers)
                    {
                        bool isScanTime_ByItems = dii.SelectedServers == 0;
                        bool isScanTime_ByDate = (DateTime.Now - lau.LastScanServers) >= new TimeSpan(au.ScanServers, 0, 0);
                        if (isScanTime_ByItems || isScanTime_ByDate)
                        {
                            debug(false, isScanTime_ByItems, gi, au, lau);
                            await ScanServersAsync(gi, null, true);
                            updated = true;
                            if (PauseBackgroundTask) return updated;
                            await Sort_DnsItems_ByLatency_Async(gi.Name, true); // Sort By Latency
                            updated = true;
                            if (PauseBackgroundTask) return updated;
                        }
                    }
                }
                else if (gi.Mode == GroupMode.FragmentDoH)
                {
                    // Update Source
                    if (au.UpdateSource > 0)
                    {
                        bool isSourceByPath = Get_Source_EnableDisable(gi.Name);
                        if (isSourceByPath)
                        {
                            List<string> urlsOrFiles = Get_Source_URLs(gi.Name);
                            bool hasSourcePath = urlsOrFiles.Count > 0;
                            if (hasSourcePath)
                            {
                                bool isUpdateTime_ByItems = dii.TotalServers == 0;
                                bool isUpdateTime_ByDate = (DateTime.Now - lau.LastUpdateSource) >= new TimeSpan(au.UpdateSource, 0, 0);
                                if (isUpdateTime_ByItems || isUpdateTime_ByDate)
                                {
                                    debug(true, isUpdateTime_ByItems, gi, au, lau);
                                    await FragmentDoHFetchSourceAsync(gi);
                                    updated = true;
                                    if (PauseBackgroundTask) return updated;
                                }
                            }
                        }
                    }
                    
                    // Update Scan
                    if (au.ScanServers > 0 && dii.TotalServers > 0)
                    {
                        bool isScanTime_ByItems = dii.SelectedServers == 0;
                        bool isScanTime_ByDate = (DateTime.Now - lau.LastScanServers) >= new TimeSpan(au.ScanServers, 0, 0);
                        if (isScanTime_ByItems || isScanTime_ByDate)
                        {
                            debug(false, isScanTime_ByItems, gi, au, lau);
                            await ScanServersAsync(gi, null, true);
                            updated = true;
                            if (PauseBackgroundTask) return updated;
                            await Sort_DnsItems_ByLatency_Async(gi.Name, true); // Sort By Latency
                            updated = true;
                            if (PauseBackgroundTask) return updated;
                        }
                    }
                }
                else if (gi.Mode == GroupMode.Custom)
                {
                    // Update Source: We Don't Have Source In Custom Mode

                    // Update Scan
                    if (au.ScanServers > 0 && dii.TotalServers > 0)
                    {
                        bool isScanTime_ByItems = dii.SelectedServers == 0;
                        bool isScanTime_ByDate = (DateTime.Now - lau.LastScanServers) >= new TimeSpan(au.ScanServers, 0, 0);
                        if (isScanTime_ByItems || isScanTime_ByDate)
                        {
                            debug(false, isScanTime_ByItems, gi, au, lau);
                            await ScanServersAsync(gi, null, true);
                            updated = true;
                            if (PauseBackgroundTask) return updated;
                            await Sort_DnsItems_ByLatency_Async(gi.Name, true); // Sort By Latency
                            updated = true;
                            if (PauseBackgroundTask) return updated;
                        }
                    }
                }
                else
                {
                    await Task.Delay(5000);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServers DnsBackgroundWorker AutoUpdate_Groups_InternalAsync: " + ex.Message);
        }
        return updated;
    }

    private async void AutoUpdate_Groups()
    {
        await Task.Run(async () =>
        {
            bool updated = true;
            while (true)
            {
                if (IsInitialized && !PauseBackgroundTask)
                {
                    IsBackgroundTaskWorking = true;

                    // Create Backup
                    if (updated && await XmlTool.IsValidXMLFileAsync(XmlFilePath))
                    {
                        try
                        {
                            await File.WriteAllBytesAsync(XmlFilePath_Backup, await File.ReadAllBytesAsync(XmlFilePath));
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("DnsServers DnsBackgroundWorker AutoUpdate_Groups (Create Backup): " + ex.Message);
                        }
                    }

                    updated = await AutoUpdate_Groups_InternalAsync();
                    IsBackgroundTaskWorking = false;
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
                else
                {
                    await Task.Delay(2000);
                }
            }
        });
    }

}