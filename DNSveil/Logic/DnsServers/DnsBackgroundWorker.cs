using System.Diagnostics;
using System.IO;
using System.Net;
using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using static DNSveil.Logic.DnsServers.EnumsAndStructs;
using GroupItem = DNSveil.Logic.DnsServers.EnumsAndStructs.GroupItem;

namespace DNSveil.Logic.DnsServers;

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
    private CancellationTokenSource StopCTS = new();

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

            // Use SDCLookup To Scan
            bool useExternal = false;

            if (!IsScanning && groupItem.Mode != GroupMode.None)
            {
                IsScanning = true;
                StopCTS = new();
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
                        var cdp = await scanDns.CheckDnsInParallelAsync(gs.LookupDomain, dnsItemList, options.FilterByProtocols, options.FilterByProperties, gsTimeoutMS, gs.BootstrapIP, gs.BootstrapPort, useExternal, StopCTS.Token);
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
                                if (isCustomRange && dnsItemList.Count > 0)
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
                        var cdp = await scanDns.CheckDNSCryptInParallelAsync(gs.LookupDomain, targetAsDnsItemList, relays, gs.ParallelSize, options.FilterByProperties, gsTimeoutMS, gs.BootstrapIP, gs.BootstrapPort, useExternal, StopCTS.Token);
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
                                if (isCustomRange && cdp.DnsItemElementsList.Count > 0)
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
                        NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(di.DNS_URL, 443);
                        rulesStr += $"\n{urid.Host}|{di.DNS_IP};";
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
                        Working_Mode = AgnosticSettings.WorkingMode.Proxy
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
                        var cdp = await scanDns.CheckDnsInParallelAsync(gs.LookupDomain, dnsItemList, options.FilterByProperties, gsTimeoutMS, IPAddress.Loopback, serverSettings.ListenerPort, useExternal, StopCTS.Token, proxyScheme);
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
                                if (isCustomRange && dnsItemList.Count > 0)
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
                        var cdp = await scanDns.CheckDnsInParallelAsync(gs.LookupDomain, dnsItemList, options.FilterByProtocols, options.FilterByProperties, gsTimeoutMS, gs.BootstrapIP, gs.BootstrapPort, useExternal, StopCTS.Token);
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
                                if (isCustomRange && dnsItemList.Count > 0)
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
                StopCTS.CancelAfter(500);
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

    private async Task<bool> MaliciousDomainsFetchSourceAsync(List<string> urlsOrFiles)
    {
        try
        {
            if (urlsOrFiles.Count == 0) return false;

            List<string> serverDomains = new();
            for (int n = 0; n < urlsOrFiles.Count; n++)
            {
                string urlOrFile = urlsOrFiles[n];
                List<string> domains = await WebAPI.GetLinesFromTextLinkAsync(urlOrFile, 20000);
                if (PauseBackgroundTask) return false;
                serverDomains.AddRange(domains);
            }
            if (PauseBackgroundTask) return false;

            // DeDup
            serverDomains = serverDomains.Distinct().ToList();
            if (serverDomains.Count == 0) return false;

            // Add To MaliciousDomains => ServerItems Element
            await MaliciousDomains_Update_ServerItems_Async(serverDomains, false);
            // Update Last AutoUpdate
            await MaliciousDomains_Update_UpdateDetails_Async(new SettingsUpdateDetails(-1, DateTime.Now), true);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServers DnsBackgroundWorker MaliciousDomainsFetchSourceAsync: " + ex.Message);
            return false;
        }
    }

    private async Task<bool> SubscriptionFetchSourceAsync(GroupItem gi, List<string> urlsOrFiles)
    {
        try
        {
            if (urlsOrFiles.Count == 0) return false;

            // Get Malicious Domains
            List<string> maliciousDomains = MaliciousDomains_Get_TotalItems();

            List<string> allDNSs = new();
            for (int i = 0; i < urlsOrFiles.Count; i++)
            {
                string urlOrFile = urlsOrFiles[i];
                List<string> dnss = await DnsTools.GetServersFromLinkAsync(urlOrFile, 20000);
                dnss = await DnsTools.DecodeStampAsync(dnss);
                if (PauseBackgroundTask) return false;
                for (int j = 0; j < dnss.Count; j++)
                {
                    string dns = dnss[j];

                    // Ignore Malicious Domains
                    DnsReader dr = new(dns);
                    if (maliciousDomains.IsContain(dr.Host)) continue;
                    NetworkTool.URL url = NetworkTool.GetUrlOrDomainDetails(dr.Host, dr.Port);
                    if (maliciousDomains.IsContain(url.BaseHost)) continue;

                    allDNSs.Add(dns);
                }
            }
            if (PauseBackgroundTask) return false;

            // DeDup
            allDNSs = allDNSs.Distinct().ToList();
            if (allDNSs.Count == 0) return false;

            // Add To Group => DnsItems Element
            await Add_DnsItems_Async(gi.Name, allDNSs, false);
            // Update Last AutoUpdate
            await Update_LastAutoUpdate_Async(gi.Name, new LastAutoUpdate(DateTime.Now, DateTime.MinValue), true);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServers DnsBackgroundWorker SubscriptionFetchSourceAsync: " + ex.Message);
            return false;
        }
    }

    private async Task<bool> AnonDNSCryptFetchSourceAsync(GroupItem gi, List<string> relayUrlsOrFiles, List<string> targetUrlsOrFiles)
    {
        try
        {
            if (relayUrlsOrFiles.Count == 0 || targetUrlsOrFiles.Count == 0) return false;

            // Get Malicious Domains
            List<string> maliciousDomains = MaliciousDomains_Get_TotalItems();

            List<string> allRelays = new();
            for (int n = 0; n < relayUrlsOrFiles.Count; n++)
            {
                string relayUrlOrFile = relayUrlsOrFiles[n];
                List<string> relays = await DnsTools.GetServersFromLinkAsync(relayUrlOrFile, 20000);
                if (PauseBackgroundTask) return false;
                for (int i = 0; i < relays.Count; i++)
                {
                    string relay = relays[i];
                    DnsReader dr = new(relay);
                    if (dr.Protocol == DnsEnums.DnsProtocol.AnonymizedDNSCryptRelay ||
                        dr.Protocol == DnsEnums.DnsProtocol.UDP ||
                        dr.Protocol == DnsEnums.DnsProtocol.TCP ||
                        dr.Protocol == DnsEnums.DnsProtocol.TcpOverUdp)
                    {
                        // Ignore Malicious Domains
                        if (maliciousDomains.IsContain(dr.Host)) continue;
                        NetworkTool.URL url = NetworkTool.GetUrlOrDomainDetails(dr.Host, dr.Port);
                        if (maliciousDomains.IsContain(url.BaseHost)) continue;

                        allRelays.Add(dr.Dns);
                    }
                }
            }
            if (PauseBackgroundTask) return false;

            // DeDup Relays
            allRelays = allRelays.Distinct().ToList();
            if (allRelays.Count == 0) return false;

            List<string> allTargets = new();
            for (int n = 0; n < targetUrlsOrFiles.Count; n++)
            {
                string targetUrlOrFile = targetUrlsOrFiles[n];
                List<string> targets = await DnsTools.GetServersFromLinkAsync(targetUrlOrFile, 20000);
                if (PauseBackgroundTask) return false;
                for (int i = 0; i < targets.Count; i++)
                {
                    string target = targets[i];
                    DnsReader dr = new(target);
                    if (dr.Protocol == DnsEnums.DnsProtocol.DnsCrypt)
                    {
                        // Ignore Malicious Domains
                        if (maliciousDomains.IsContain(dr.Host)) continue;
                        NetworkTool.URL url = NetworkTool.GetUrlOrDomainDetails(dr.Host, dr.Port);
                        if (maliciousDomains.IsContain(url.BaseHost)) continue;

                        allTargets.Add(dr.Dns);
                    }
                }
            }
            if (PauseBackgroundTask) return false;

            // DeDup Targets
            allTargets = allTargets.Distinct().ToList();
            if (allTargets.Count == 0) return false;

            // Add To Group => RelayItems Element
            await Add_AnonDNSCrypt_RelayItems_Async(gi.Name, allRelays, false);
            // Add To Group => TargetItems Element
            await Add_AnonDNSCrypt_TargetItems_Async(gi.Name, allTargets, false);
            // Update Last AutoUpdate
            await Update_LastAutoUpdate_Async(gi.Name, new LastAutoUpdate(DateTime.Now, DateTime.MinValue), true);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServers DnsBackgroundWorker AnonDNSCryptFetchSourceAsync: " + ex.Message);
            return false;
        }
    }

    private async Task<bool> FragmentDoHFetchSourceAsync(GroupItem gi, List<string> urlsOrFiles)
    {
        try
        {
            if (urlsOrFiles.Count == 0) return false;

            List<string> allDNSs = new();
            for (int n = 0; n < urlsOrFiles.Count; n++)
            {
                string urlOrFile = urlsOrFiles[n];
                List<string> dnss = await DnsTools.GetServersFromLinkAsync(urlOrFile, 20000);
                if (PauseBackgroundTask) return false;
                allDNSs.AddRange(dnss);
            }
            if (PauseBackgroundTask) return false;

            // Get Malicious Domains
            List<string> maliciousDomains = MaliciousDomains_Get_TotalItems();

            // Convert To DnsItem
            List<DnsItem> allDoHStamps = Tools.Convert_DNSs_To_DnsItem_ForFragmentDoH(allDNSs, maliciousDomains);
            if (allDoHStamps.Count == 0) return false;

            // Add To Group => DnsItems Element
            await Add_DnsItems_Async(gi.Name, allDoHStamps, false);
            // Update Last AutoUpdate
            await Update_LastAutoUpdate_Async(gi.Name, new LastAutoUpdate(DateTime.Now, DateTime.MinValue), true);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServers DnsBackgroundWorker FragmentDoHFetchSourceAsync: " + ex.Message);
            return false;
        }
    }

    private async Task<bool> AutoUpdate_SettingsAndGroups_InternalAsync()
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

            // Update Settings: Malicious Domains
            SettingsUpdateDetails sudMD = MaliciousDomains_Get_UpdateDetails();
            if (sudMD.UpdateSource > 0)
            {
                bool isUpdateTime_ByDate = (DateTime.Now - sudMD.LastUpdateSource) >= new TimeSpan(sudMD.UpdateSource, 0, 0);
                if (isUpdateTime_ByDate)
                {
                    List<string> urlsOrFiles = MaliciousDomains_Get_Source_URLs();
                    bool hasSourcePath = urlsOrFiles.Count > 0;
                    if (hasSourcePath)
                    {
                        updated = await MaliciousDomainsFetchSourceAsync(urlsOrFiles);
                        if (PauseBackgroundTask) return updated;
                    }
                }
            }

            // Update Groups
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
                            bool isUpdateTime_ByItems = dii.TotalServers == 0;
                            bool isUpdateTime_ByDate = (DateTime.Now - lau.LastUpdateSource) >= new TimeSpan(au.UpdateSource, 0, 0);
                            if (isUpdateTime_ByItems || isUpdateTime_ByDate)
                            {
                                List<string> urlsOrFiles = Get_Source_URLs(gi.Name);
                                bool hasSourcePath = urlsOrFiles.Count > 0;
                                if (hasSourcePath)
                                {
                                    debug(true, isUpdateTime_ByItems, gi, au, lau);
                                    updated = await SubscriptionFetchSourceAsync(gi, urlsOrFiles);
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
                            bool isUpdateTime_ByItems = dii.TotalServers == 0;
                            bool isUpdateTime_ByDate = (DateTime.Now - lau.LastUpdateSource) >= new TimeSpan(au.UpdateSource, 0, 0);
                            if (isUpdateTime_ByItems || isUpdateTime_ByDate)
                            {
                                List<string> relayUrlsOrFiles = Get_AnonDNSCrypt_Relay_URLs(gi.Name);
                                List<string> targetUrlsOrFiles = Get_AnonDNSCrypt_Target_URLs(gi.Name);
                                bool hasSourcePath = relayUrlsOrFiles.Count > 0 && targetUrlsOrFiles.Count > 0;
                                if (isSourceByPath)
                                {
                                    debug(true, isUpdateTime_ByItems, gi, au, lau);
                                    updated = await AnonDNSCryptFetchSourceAsync(gi, relayUrlsOrFiles, targetUrlsOrFiles);
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
                            bool isUpdateTime_ByItems = dii.TotalServers == 0;
                            bool isUpdateTime_ByDate = (DateTime.Now - lau.LastUpdateSource) >= new TimeSpan(au.UpdateSource, 0, 0);
                            if (isUpdateTime_ByItems || isUpdateTime_ByDate)
                            {
                                List<string> urlsOrFiles = Get_Source_URLs(gi.Name);
                                bool hasSourcePath = urlsOrFiles.Count > 0;
                                if (hasSourcePath)
                                {
                                    debug(true, isUpdateTime_ByItems, gi, au, lau);
                                    updated = await FragmentDoHFetchSourceAsync(gi, urlsOrFiles);
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
            Debug.WriteLine("DnsServers DnsBackgroundWorker AutoUpdate_SettingsAndGroups_InternalAsync: " + ex.Message);
        }
        return updated;
    }

    private async void BackgroundWorker_AutoUpdate_SettingsAndGroups()
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
                    if (updated && await XmlTool.IsValidFileAsync(XmlFilePath))
                    {
                        try
                        {
                            await File.WriteAllBytesAsync(XmlFilePath_Backup, await File.ReadAllBytesAsync(XmlFilePath));
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("DnsServers DnsBackgroundWorker AutoUpdate_SettingsAndGroups (Create Backup): " + ex.Message);
                        }
                    }

                    updated = await AutoUpdate_SettingsAndGroups_InternalAsync();
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