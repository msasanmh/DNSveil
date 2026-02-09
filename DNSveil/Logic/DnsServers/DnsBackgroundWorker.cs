using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using static DNSveil.Logic.DnsServers.DnsModel;

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
    private bool IsBackgroundScan { get; set; } = false;
    private CancellationTokenSource CTS_Scan = new();
    private CancellationTokenSource CTS_BackgroundTask = new();

    public BackgroundWorkerEventArgs GetBackgroundWorkerStatus { get; private set; } = new();

    public class BackgroundWorkerEventArgs : EventArgs
    {
        public bool IsWorking { get; set; } = false;
        public string ButtonText { get; set; } = "Scan";
        public bool ButtonEnable { get; set; } = true;
        public string Description { get; set; } = "Ready To Work";
        public DnsGroup Group { get; set; } = new();
        public int ProgressMin { get; set; } = 0;
        public int ProgressMax { get; set; } = 0;
        public int ProgressValue { get; set; } = 0;
        public int ProgressPercentage => ProgressMax > 0 ? (ProgressValue * 100 / ProgressMax) : 0;
        public int OnlineServers { get; set; } = 0;
        public int SelectedServers { get; set; } = 0;
        public int SumLatencyMS { get; set; } = 0;
        public int AverageLatencyMS => OnlineServers > 0 ? (SumLatencyMS / OnlineServers) : (-1);
        public int LastIndex { get; set; } = -1;
        public int ParallelLatencyMS { get; set; } = 0;
    }

    /// <summary>
    /// Scan DnsItems In A Group
    /// </summary>
    /// <param name="group">DnsGroup</param>
    /// <param name="selectedItemList">NULL: All DnsItems In The Group</param>
    /// <param name="isBackground">True: Do Not Update UI</param>
    /// <returns></returns>
    private async Task ScanServersAsync(DnsGroup group, ObservableCollection<DnsItem>? selectedItemList, bool isBackground)
    {
        try
        {
            IsBackgroundScan = isBackground;

            BackgroundWorkerEventArgs bw = new()
            {
                IsWorking = true,
                Group = group,
                LastIndex = -1
            };
            bool isCustomRange = selectedItemList != null;

            // Use SDCLookup To Scan
            bool useExternal = false;

            if (!IsScanning && group.Mode != GroupMode.None)
            {
                IsScanning = true;
                CTS_Scan = new();
                bw.ButtonText = "Stop";
                bw.ButtonEnable = true;
                bw.Description = "Scanning...";

                // Get Group Settings And Clone (We Need To Modify It)
                GroupSettings gs = Get_GroupSettings(group.Name).Clone_GroupSettings();
                if (isBackground && gs.ScanParallelSize > 5) gs.ScanParallelSize = 5; // Limit Parallel Size For Background Task
                if (isBackground && gs.ScanParallelSize > gs.MaxServersToConnect) gs.ScanParallelSize = gs.MaxServersToConnect;

                if (group.Mode == GroupMode.Subscription)
                {
                    // Clear Previous Result
                    if (!isCustomRange) await Clear_DnsItems_Result_Async(group.Name, true, false);

                    selectedItemList ??= Get_DnsItems(group.Name);
                    
                    if (!isBackground)
                    {
                        bw.ProgressMax = selectedItemList.Count;
                        GetBackgroundWorkerStatus = bw;
                        POnBackgroundUpdateReceived?.Invoke(Owner, bw);
                    }
                    
                    // Get Group Options
                    SubscriptionOptions options = Get_Subscription_Options(group.Name);

                    var selectedItemLists = selectedItemList.SplitToLists(gs.ScanParallelSize);

                    ScanDns scanDns = new(gs.AllowInsecure, true);
                    for (int n = 0; n < selectedItemLists.Count; n++)
                    {
                        if (StopScanning) break;
                        if (isBackground && bw.SelectedServers >= gs.MaxServersToConnect) break;
                        if (isBackground && PauseBackgroundTask) break;

                        ObservableCollection<DnsItem> itemList = selectedItemLists[n];
                        var spr = await scanDns.ScanDnsInParallelAsync(gs, itemList, options.FilterByProtocols, options.FilterByProperties, null, useExternal, CTS_Scan.Token);
                        itemList = spr.Items;
                        bw.OnlineServers += spr.ParallelResult.OnlineServers;
                        bw.SelectedServers += spr.ParallelResult.SelectedServers;
                        bw.SumLatencyMS += spr.ParallelResult.SumLatencyMS;
                        bw.ProgressValue += itemList.Count;
                        bw.ParallelLatencyMS = spr.ParallelResult.ParallelLatencyMS;

                        // Update LastScanServers
                        if (bw.OnlineServers > 0)
                        {
                            options.AutoUpdate.LastScanServers = DateTime.Now;
                            await Update_Subscription_Options_Async(group.Name, options, false);
                        }

                        await Update_DnsItems_Async(group.Name, itemList, false);
                        await Sort_DnsItems_Async(group.Name, true);

                        if (!isBackground)
                        {
                            if (n == selectedItemLists.Count - 1) bw.ProgressValue = bw.ProgressMax;
                            try
                            {
                                if (isCustomRange && itemList.Count > 0)
                                    bw.LastIndex = Get_IndexOf_DnsItem(group.Name, itemList[^1]);
                                else bw.LastIndex = bw.ProgressValue - 1;
                            }
                            catch (Exception) { }

                            if (!StopScanning && n != selectedItemLists.Count - 1) // If It's Not Last Round
                            {
                                GetBackgroundWorkerStatus = bw;
                                POnBackgroundUpdateReceived?.Invoke(Owner, bw);
                            }
                        }
                    }
                }
                else if (group.Mode == GroupMode.AnonymizedDNSCrypt)
                {
                    // Clear Previous Result
                    if (!isCustomRange) await Clear_DnsItems_Result_Async(group.Name, true, false);

                    selectedItemList ??= Get_DnsItems(group.Name);
                    selectedItemList = selectedItemList.RemoveRelays();

                    // Get Relays
                    List<string> relays = Get_AnonDNSCrypt_Relays(group.Name);

                    if (!isBackground)
                    {
                        bw.ProgressMax = selectedItemList.Count;
                        GetBackgroundWorkerStatus = bw;
                        POnBackgroundUpdateReceived?.Invoke(Owner, bw);
                    }
                    
                    // Get Group Options
                    AnonymizedDNSCryptOptions options = Get_AnonDNSCrypt_Options(group.Name);

                    var selectedItemLists = selectedItemList.SplitToLists(gs.ScanParallelSize);

                    ScanDns scanDns = new(gs.AllowInsecure, true);
                    for (int n = 0; n < selectedItemLists.Count; n++)
                    {
                        if (StopScanning) break;
                        if (isBackground && bw.SelectedServers >= gs.MaxServersToConnect) break;
                        if (isBackground && PauseBackgroundTask) break;

                        ObservableCollection<DnsItem> itemList = selectedItemLists[n];
                        var spr = await scanDns.CheckDNSCryptInParallelAsync(gs, itemList, relays, options.FilterByProperties, null, useExternal, CTS_Scan.Token);
                        itemList = spr.Items;
                        bw.OnlineServers += spr.ParallelResult.OnlineServers;
                        bw.SelectedServers += spr.ParallelResult.SelectedServers;
                        bw.SumLatencyMS += spr.ParallelResult.SumLatencyMS;
                        bw.ProgressValue += itemList.Count;
                        bw.ParallelLatencyMS = spr.ParallelResult.ParallelLatencyMS;

                        // Update LastScanServers
                        if (bw.OnlineServers > 0)
                        {
                            options.AutoUpdate.LastScanServers = DateTime.Now;
                            await Update_AnonDNSCrypt_Options_Async(group.Name, options, false);
                        }

                        await Update_DnsItems_Async(group.Name, itemList, false);
                        await Sort_DnsItems_Async(group.Name, true);

                        if (!isBackground)
                        {
                            if (n == selectedItemLists.Count - 1) bw.ProgressValue = bw.ProgressMax;
                            try
                            {
                                if (isCustomRange && itemList.Count > 0)
                                    bw.LastIndex = Get_IndexOf_DnsItem(group.Name, itemList[^1]);
                                else bw.LastIndex = bw.ProgressValue - 1;
                            }
                            catch (Exception) { }

                            if (!StopScanning && n != selectedItemLists.Count - 1) // If It's Not Last Round
                            {
                                GetBackgroundWorkerStatus = bw;
                                POnBackgroundUpdateReceived?.Invoke(Owner, bw);
                            }
                        }
                    }
                }
                else if (group.Mode == GroupMode.FragmentDoH)
                {
                    // Clear Previous Result
                    if (!isCustomRange) await Clear_DnsItems_Result_Async(group.Name, true, false);
                    
                    selectedItemList ??= Get_DnsItems(group.Name);

                    if (!isBackground)
                    {
                        bw.Description = "Starting Fragment Proxy Server...";
                        bw.ProgressMax = selectedItemList.Count;
                        GetBackgroundWorkerStatus = bw;
                        POnBackgroundUpdateReceived?.Invoke(Owner, bw);
                    }
                    
                    // Get Group Options
                    FragmentDoHOptions options = Get_FragmentDoH_Options(group.Name);

                    // Create Proxy Server: Create Fragment Program
                    AgnosticProgram.Fragment serverFragment = new();
                    serverFragment.Set(AgnosticProgram.Fragment.Mode.Program, options.FragmentSettings.ChunksBeforeSNI, options.FragmentSettings.SniChunkMode, options.FragmentSettings.ChunksSNI, options.FragmentSettings.AntiPatternOffset, options.FragmentSettings.FragmentDelayMS);

                    // Create Proxy Server: Create Rules
                    string rulesStr = string.Empty;
                    for (int n = 0; n < selectedItemList.Count; n++)
                    {
                        DnsItem di = selectedItemList[n];
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

                    gs.BootstrapIP = IPAddress.Loopback;
                    gs.BootstrapPort = serverSettings.ListenerPort;
                    string proxyScheme = $"socks5://{IPAddress.Loopback}:{serverSettings.ListenerPort}";

                    if (!isBackground)
                    {
                        bw.Description = "Scanning...";
                        GetBackgroundWorkerStatus = bw;
                        POnBackgroundUpdateReceived?.Invoke(Owner, bw);
                    }
                    
                    var selectedItemLists = selectedItemList.SplitToLists(gs.ScanParallelSize);

                    ScanDns scanDns = new(gs.AllowInsecure, true);
                    for (int n = 0; n < selectedItemLists.Count; n++)
                    {
                        if (StopScanning) break;
                        if (isBackground && bw.SelectedServers >= gs.MaxServersToConnect) break;
                        if (isBackground && PauseBackgroundTask) break;

                        ObservableCollection<DnsItem> itemList = selectedItemLists[n];
                        var spr = await scanDns.ScanDnsInParallelAsync(gs, itemList, null, options.FilterByProperties, proxyScheme, useExternal, CTS_Scan.Token);
                        itemList = spr.Items;
                        bw.OnlineServers += spr.ParallelResult.OnlineServers;
                        bw.SelectedServers += spr.ParallelResult.SelectedServers;
                        bw.SumLatencyMS += spr.ParallelResult.SumLatencyMS;
                        bw.ProgressValue += itemList.Count;
                        bw.ParallelLatencyMS = spr.ParallelResult.ParallelLatencyMS;

                        // Update LastScanServers
                        if (bw.OnlineServers > 0)
                        {
                            options.AutoUpdate.LastScanServers = DateTime.Now;
                            await Update_FragmentDoH_Options_Async(group.Name, options, false);
                        }

                        await Update_DnsItems_Async(group.Name, itemList, false);
                        await Sort_DnsItems_Async(group.Name, true);

                        if (!isBackground)
                        {
                            if (n == selectedItemLists.Count - 1) bw.ProgressValue = bw.ProgressMax;
                            try
                            {
                                if (isCustomRange && itemList.Count > 0)
                                    bw.LastIndex = Get_IndexOf_DnsItem(group.Name, itemList[^1]);
                                else bw.LastIndex = bw.ProgressValue - 1;
                            }
                            catch (Exception) { }

                            if (!StopScanning && n != selectedItemLists.Count - 1) // If It's Not Last Round
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
                else if (group.Mode == GroupMode.Custom)
                {
                    // Clear Previous Result
                    if (!isCustomRange) await Clear_DnsItems_Result_Async(group.Name, false, false);

                    selectedItemList ??= Get_DnsItems(group.Name);

                    if (!isBackground)
                    {
                        bw.ProgressMax = selectedItemList.Count;
                        GetBackgroundWorkerStatus = bw;
                        POnBackgroundUpdateReceived?.Invoke(Owner, bw);
                    }
                    
                    // Get Group Options
                    CustomOptions options = Get_Custom_Options(group.Name);

                    var selectedItemLists = selectedItemList.SplitToLists(gs.ScanParallelSize);

                    ScanDns scanDns = new(gs.AllowInsecure, true);
                    for (int n = 0; n < selectedItemLists.Count; n++)
                    {
                        if (StopScanning) break;
                        if (isBackground && bw.SelectedServers >= gs.MaxServersToConnect) break;
                        if (isBackground && PauseBackgroundTask) break;

                        ObservableCollection<DnsItem> itemList = selectedItemLists[n];
                        var spr = await scanDns.ScanDnsInParallelAsync(gs, itemList, options.FilterByProtocols, options.FilterByProperties, null, useExternal, CTS_Scan.Token);
                        itemList = spr.Items;
                        bw.OnlineServers += spr.ParallelResult.OnlineServers;
                        bw.SelectedServers += spr.ParallelResult.SelectedServers;
                        bw.SumLatencyMS += spr.ParallelResult.SumLatencyMS;
                        bw.ProgressValue += itemList.Count;
                        bw.ParallelLatencyMS = spr.ParallelResult.ParallelLatencyMS;

                        // Update LastScanServers
                        if (bw.OnlineServers > 0)
                        {
                            options.AutoUpdate.LastScanServers = DateTime.Now;
                            await Update_Custom_Options_Async(group.Name, options, false);
                        }

                        await Update_DnsItems_Async(group.Name, itemList, false);
                        await Sort_DnsItems_Async(group.Name, true);

                        if (!isBackground)
                        {
                            if (n == selectedItemLists.Count - 1) bw.ProgressValue = bw.ProgressMax;
                            try
                            {
                                if (isCustomRange && itemList.Count > 0)
                                    bw.LastIndex = Get_IndexOf_DnsItem(group.Name, itemList[^1]);
                                else bw.LastIndex = bw.ProgressValue - 1;
                            }
                            catch (Exception) { }

                            if (!StopScanning && n != selectedItemLists.Count - 1) // If It's Not Last Round
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
                    if (bw.Group.Mode == GroupMode.None)
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
    /// <param name="group">DnsGroup</param>
    /// <param name="selectedItemList">NULL: All DnsItems In The Group</param>
    public async void ScanServers(DnsGroup group, ObservableCollection<DnsItem>? selectedItemList)
    {
        await ScanServersAsync(group, selectedItemList, false);
    }

    private async Task<bool> MaliciousDomainsFetchSourceAsync(DnsSettings settings, CancellationToken ct)
    {
        try
        {
            if (settings.MaliciousDomains.Source_URLs.Count == 0) return false;

            List<string> serverDomains = new();
            for (int n = 0; n < settings.MaliciousDomains.Source_URLs.Count; n++)
            {
                string urlOrFile = settings.MaliciousDomains.Source_URLs[n];
                List<string> domains = await WebAPI.GetLinesFromTextLinkAsync(urlOrFile, 20000, ct);
                if (PauseBackgroundTask)
                {
                    domains.Clear();
                    serverDomains.Clear();
                    return false;
                }

                serverDomains.AddRange(domains);
            }

            if (PauseBackgroundTask)
            {
                serverDomains.Clear();
                return false;
            }

            // DeDup
            serverDomains = serverDomains.Distinct().ToList();
            if (serverDomains.Count == 0) return false;

            // Add To MaliciousDomains => ServerItems
            settings.MaliciousDomains.Items_Server = serverDomains;
            settings.MaliciousDomains.LastUpdateSource = DateTime.Now; // Update LastUpdateSource
            return await Update_Settings_Async(settings, true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServers DnsBackgroundWorker MaliciousDomainsFetchSourceAsync: " + ex.Message);
            return false;
        }
    }

    private async Task<bool> SubscriptionFetchSourceAsync(DnsGroup group, CancellationToken ct)
    {
        try
        {
            if (group.Subscription == null) return false;
            if (group.Subscription.Source.URLs.Count == 0) return false;

            // Get Malicious Domains
            DnsSettings settings = Get_Settings();
            List<string> maliciousDomains = settings.MaliciousDomains.Get_Malicious_Domains;

            ObservableCollection<string> allDNSs = new();
            for (int i = 0; i < group.Subscription.Source.URLs.Count; i++)
            {
                string urlOrFile = group.Subscription.Source.URLs[i];
                List<string> dnss = await DnsTools.GetServersFromLinkAsync(urlOrFile, 20000, ct);
                dnss = await DnsTools.DecodeStampAsync(dnss);
                if (PauseBackgroundTask)
                {
                    dnss.Clear();
                    allDNSs.Clear();
                    maliciousDomains.Clear();
                    return false;
                }

                for (int j = 0; j < dnss.Count; j++)
                {
                    string dns = dnss[j];
                    if (MaliciousDomains.IsMalicious(maliciousDomains, dns)) continue; // Ignore Malicious Domains
                    allDNSs.Add(dns);
                }
            }

            if (PauseBackgroundTask)
            {
                allDNSs.Clear();
                maliciousDomains.Clear();
                return false;
            }

            // DeDup
            allDNSs = allDNSs.Distinct().ToObservableCollection();
            if (allDNSs.Count == 0)
            {
                maliciousDomains.Clear();
                return false;
            }

            // Add To Group => DnsItems
            bool isSuccess = await Add_DnsItems_Async(group.Name, allDNSs, false);

            // Get Group Options
            SubscriptionOptions options = Get_Subscription_Options(group.Name);
            options.AutoUpdate.LastUpdateSource = DateTime.Now; // Update LastUpdateSource
            await Update_Subscription_Options_Async(group.Name, options, true);

            // Dispose - Return
            allDNSs.Clear();
            maliciousDomains.Clear();
            return isSuccess;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServers DnsBackgroundWorker SubscriptionFetchSourceAsync: " + ex.Message);
            return false;
        }
    }

    private async Task<bool> AnonDNSCryptFetchSourceAsync(DnsGroup group, CancellationToken ct)
    {
        try
        {
            if (group.AnonymizedDNSCrypt == null) return false;
            if (group.AnonymizedDNSCrypt.Source.Relay_URLs.Count == 0) return false;
            if (group.AnonymizedDNSCrypt.Source.Target_URLs.Count == 0) return false;

            // Get Malicious Domains
            DnsSettings settings = Get_Settings();
            List<string> maliciousDomains = settings.MaliciousDomains.Get_Malicious_Domains;

            // Get Relays
            List<string> allRelays = new();
            for (int n = 0; n < group.AnonymizedDNSCrypt.Source.Relay_URLs.Count; n++)
            {
                string relayUrlOrFile = group.AnonymizedDNSCrypt.Source.Relay_URLs[n];
                List<string> relays = await DnsTools.GetServersFromLinkAsync(relayUrlOrFile, 20000, ct);
                if (PauseBackgroundTask)
                {
                    relays.Clear();
                    allRelays.Clear();
                    maliciousDomains.Clear();
                    return false;
                }

                allRelays.AddRange(Tools.Find_Relays_ForAnonDNSCrypt(relays, maliciousDomains));
            }

            if (PauseBackgroundTask)
            {
                allRelays.Clear();
                maliciousDomains.Clear();
                return false;
            }

            // DeDup Relays
            allRelays = allRelays.Distinct().ToList();
            if (allRelays.Count == 0)
            {
                maliciousDomains.Clear();
                return false;
            }

            // Get Targets
            List<string> allTargets = new();
            for (int n = 0; n < group.AnonymizedDNSCrypt.Source.Target_URLs.Count; n++)
            {
                string targetUrlOrFile = group.AnonymizedDNSCrypt.Source.Target_URLs[n];
                List<string> targets = await DnsTools.GetServersFromLinkAsync(targetUrlOrFile, 20000, ct);
                if (PauseBackgroundTask)
                {
                    targets.Clear();
                    allRelays.Clear();
                    allTargets.Clear();
                    maliciousDomains.Clear();
                    return false;
                }

                allTargets.AddRange(Tools.Find_Targets_ForAnonDNSCrypt(targets, maliciousDomains));
            }

            if (PauseBackgroundTask)
            {
                allRelays.Clear();
                allTargets.Clear();
                maliciousDomains.Clear();
                return false;
            }

            // DeDup Targets
            allTargets = allTargets.Distinct().ToList();
            if (allTargets.Count == 0)
            {
                allRelays.Clear();
                maliciousDomains.Clear();
                return false;
            }

            // Add To Group => RelayItems
            await Add_AnonDNSCrypt_RelayItems_Async(group.Name, allRelays, false);
            // Add To Group => TargetItems
            await Add_AnonDNSCrypt_TargetItems_Async(group.Name, allTargets, false);
            // Add To Group => DnsItems
            ObservableCollection<DnsItem> anonDNSCrypts = Tools.Create_DnsItem_ForAnonDNSCrypt(allTargets, allRelays);
            bool isSuccess = await Add_DnsItems_Async(group.Name, anonDNSCrypts, false);

            // Get Group Options
            AnonymizedDNSCryptOptions options = Get_AnonDNSCrypt_Options(group.Name);
            options.AutoUpdate.LastUpdateSource = DateTime.Now; // Update LastUpdateSource
            await Update_AnonDNSCrypt_Options_Async(group.Name, options, true);

            // Dispose - Return
            allRelays.Clear();
            allTargets.Clear();
            anonDNSCrypts.Clear();
            maliciousDomains.Clear();
            return isSuccess;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServers DnsBackgroundWorker AnonDNSCryptFetchSourceAsync: " + ex.Message);
            return false;
        }
    }

    private async Task<bool> FragmentDoHFetchSourceAsync(DnsGroup group, CancellationToken ct)
    {
        try
        {
            if (group.FragmentDoH == null) return false;
            if (group.FragmentDoH.Source.URLs.Count == 0) return false;

            List<string> allDNSs = new();
            for (int n = 0; n < group.FragmentDoH.Source.URLs.Count; n++)
            {
                string urlOrFile = group.FragmentDoH.Source.URLs[n];
                List<string> dnss = await DnsTools.GetServersFromLinkAsync(urlOrFile, 20000, ct);
                if (PauseBackgroundTask)
                {
                    dnss.Clear();
                    allDNSs.Clear();
                    return false;
                }

                allDNSs.AddRange(dnss);
            }

            if (PauseBackgroundTask)
            {
                allDNSs.Clear();
                return false;
            }

            // Get Malicious Domains
            DnsSettings settings = Get_Settings();
            List<string> maliciousDomains = settings.MaliciousDomains.Get_Malicious_Domains;

            // Convert To DnsItem
            ObservableCollection<DnsItem> allDoHStamps = Tools.Convert_DNSs_To_DnsItem_ForFragmentDoH(allDNSs, maliciousDomains);
            if (allDoHStamps.Count == 0)
            {
                allDNSs.Clear();
                return false;
            }

            // Add To Group => DnsItems
            bool isSuccess = await Add_DnsItems_Async(group.Name, allDoHStamps, false);

            // Get Group Options
            FragmentDoHOptions options = Get_FragmentDoH_Options(group.Name);
            options.AutoUpdate.LastUpdateSource = DateTime.Now; // Update LastUpdateSource
            await Update_FragmentDoH_Options_Async(group.Name, options, true);

            // Dispose - Return
            allDoHStamps.Clear();
            allDNSs.Clear();
            return isSuccess;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServers DnsBackgroundWorker FragmentDoHFetchSourceAsync: " + ex.Message);
            return false;
        }
    }

    private async Task<bool> AutoUpdate_SettingsAndGroups_InternalAsync(CancellationToken ct)
    {
        bool updated = false;
        try
        {
            // Update Settings: Malicious Domains
            DnsSettings settings = Get_Settings();
            if (settings.MaliciousDomains.UpdateSource > 0)
            {
                bool isUpdateTime_ByDate = (DateTime.Now - settings.MaliciousDomains.LastUpdateSource) >= new TimeSpan(settings.MaliciousDomains.UpdateSource, 0, 0);
                if (isUpdateTime_ByDate)
                {
                    updated = await MaliciousDomainsFetchSourceAsync(settings, ct);
                    if (PauseBackgroundTask) return updated;
                }
            }

            // Update Groups
            List<DnsGroup> groups = Get_Groups(true);
            for (int n = 0; n < groups.Count; n++)
            {
                if (PauseBackgroundTask) return updated;
                DnsGroup group = groups[n];
                DnsItemsInfo info = Get_DnsItems_Info(group.Name);
                if (group.Mode == GroupMode.Subscription && group.Subscription != null)
                {
                    // Update Source
                    if (group.Subscription.Options.AutoUpdate.UpdateSource > 0)
                    {
                        bool isUpdateTime_ByItems = info.TotalServers == 0;
                        bool isUpdateTime_ByDate = (DateTime.Now - group.Subscription.Options.AutoUpdate.LastUpdateSource) >= new TimeSpan(group.Subscription.Options.AutoUpdate.UpdateSource, 0, 0);
                        if (isUpdateTime_ByItems || isUpdateTime_ByDate)
                        {
                            updated = await SubscriptionFetchSourceAsync(group, ct);
                            if (PauseBackgroundTask) return updated;
                        }
                    }
                    
                    // Scan
                    if (group.Subscription.Options.AutoUpdate.ScanServers > 0 && info.TotalServers > 0)
                    {
                        bool isScanTime_ByItems = info.SelectedServers == 0;
                        bool isScanTime_ByDate = (DateTime.Now - group.Subscription.Options.AutoUpdate.LastScanServers) >= new TimeSpan(group.Subscription.Options.AutoUpdate.ScanServers, 0, 0);
                        if (isScanTime_ByItems || isScanTime_ByDate)
                        {
                            await ScanServersAsync(group, null, true);
                            updated = true;
                            if (PauseBackgroundTask) return updated;
                            await Sort_DnsItems_Async(group.Name, true); // Sort By Latency
                            if (PauseBackgroundTask) return updated;
                        }
                    }
                }
                else if (group.Mode == GroupMode.AnonymizedDNSCrypt && group.AnonymizedDNSCrypt != null)
                {
                    // Update Source
                    if (group.AnonymizedDNSCrypt.Options.AutoUpdate.UpdateSource > 0)
                    {
                        bool isUpdateTime_ByItems = info.TotalServers == 0;
                        bool isUpdateTime_ByDate = (DateTime.Now - group.AnonymizedDNSCrypt.Options.AutoUpdate.LastUpdateSource) >= new TimeSpan(group.AnonymizedDNSCrypt.Options.AutoUpdate.UpdateSource, 0, 0);
                        if (isUpdateTime_ByItems || isUpdateTime_ByDate)
                        {
                            updated = await AnonDNSCryptFetchSourceAsync(group, ct);
                            if (PauseBackgroundTask) return updated;
                        }
                    }

                    // Scan
                    List<string> relays = Get_AnonDNSCrypt_Relays(group.Name);
                    List<string> targets = Get_AnonDNSCrypt_Targets(group.Name);
                    bool hasTotalServers = relays.Count > 0 && targets.Count > 0;
                    if (group.AnonymizedDNSCrypt.Options.AutoUpdate.ScanServers > 0 && hasTotalServers)
                    {
                        bool isScanTime_ByItems = info.SelectedServers == 0;
                        bool isScanTime_ByDate = (DateTime.Now - group.AnonymizedDNSCrypt.Options.AutoUpdate.LastScanServers) >= new TimeSpan(group.AnonymizedDNSCrypt.Options.AutoUpdate.ScanServers, 0, 0);
                        if (isScanTime_ByItems || isScanTime_ByDate)
                        {
                            await ScanServersAsync(group, null, true);
                            updated = true;
                            if (PauseBackgroundTask) return updated;
                            await Sort_DnsItems_Async(group.Name, true); // Sort By Latency
                            if (PauseBackgroundTask) return updated;
                        }
                    }
                }
                else if (group.Mode == GroupMode.FragmentDoH && group.FragmentDoH != null)
                {
                    // Update Source
                    if (group.FragmentDoH.Options.AutoUpdate.UpdateSource > 0)
                    {
                        bool isUpdateTime_ByItems = info.TotalServers == 0;
                        bool isUpdateTime_ByDate = (DateTime.Now - group.FragmentDoH.Options.AutoUpdate.LastUpdateSource) >= new TimeSpan(group.FragmentDoH.Options.AutoUpdate.UpdateSource, 0, 0);
                        if (isUpdateTime_ByItems || isUpdateTime_ByDate)
                        {
                            updated = await FragmentDoHFetchSourceAsync(group, ct);
                            if (PauseBackgroundTask) return updated;
                        }
                    }
                    
                    // Scan
                    if (group.FragmentDoH.Options.AutoUpdate.ScanServers > 0 && info.TotalServers > 0)
                    {
                        bool isScanTime_ByItems = info.SelectedServers == 0;
                        bool isScanTime_ByDate = (DateTime.Now - group.FragmentDoH.Options.AutoUpdate.LastScanServers) >= new TimeSpan(group.FragmentDoH.Options.AutoUpdate.ScanServers, 0, 0);
                        if (isScanTime_ByItems || isScanTime_ByDate)
                        {
                            await ScanServersAsync(group, null, true);
                            updated = true;
                            if (PauseBackgroundTask) return updated;
                            await Sort_DnsItems_Async(group.Name, true); // Sort By Latency
                            if (PauseBackgroundTask) return updated;
                        }
                    }
                }
                else if (group.Mode == GroupMode.Custom && group.Custom != null)
                {
                    // Update Source: We Don't Have Source In Custom Mode

                    // Scan
                    if (group.Custom.Options.AutoUpdate.ScanServers > 0 && info.TotalServers > 0)
                    {
                        bool isScanTime_ByItems = info.SelectedServers == 0;
                        bool isScanTime_ByDate = (DateTime.Now - group.Custom.Options.AutoUpdate.LastScanServers) >= new TimeSpan(group.Custom.Options.AutoUpdate.ScanServers, 0, 0);
                        if (isScanTime_ByItems || isScanTime_ByDate)
                        {
                            await ScanServersAsync(group, null, true);
                            updated = true;
                            if (PauseBackgroundTask) return updated;
                            await Sort_DnsItems_Async(group.Name, true); // Sort By Latency
                            if (PauseBackgroundTask) return updated;
                        }
                    }
                }
                else
                {
                    await Task.Delay(1000, ct);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServers DnsBackgroundWorker AutoUpdate_SettingsAndGroups_InternalAsync: " + ex.Message);
        }
        return updated;
    }

    private async void CheckPauseBackground()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(1000);
                try
                {
                    if (StopScanning || (PauseBackgroundTask && IsScanning && IsBackgroundScan))
                    {
                        if (!CTS_Scan.IsCancellationRequested)
                        {
                            CTS_Scan.Cancel();
                            Debug.WriteLine("DNS Scan Cancellation Request Sent.");
                        }
                    }
                    if (PauseBackgroundTask)
                    {
                        if (!CTS_BackgroundTask.IsCancellationRequested)
                        {
                            CTS_BackgroundTask.Cancel();
                            Debug.WriteLine("DNS Background Cancellation Request Sent.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("DnsServers DnsBackgroundWorker CheckPauseBackground: " + Environment.NewLine + ex.GetInnerExceptions());
                }
            }
        });
    }

    private async void BackgroundWorker_AutoUpdate_SettingsAndGroups()
    {
        CheckPauseBackground();
        await Task.Run(async () =>
        {
            bool updated = true;
            while (true)
            {
                if (IsInitialized && !PauseBackgroundTask)
                {
                    IsBackgroundTaskWorking = true;

                    // Create Backup
                    if (updated && await JsonTool.IsValidFileAsync(DocFilePath))
                    {
                        try
                        {
                            await File.WriteAllBytesAsync(DocFilePath_Backup, await File.ReadAllBytesAsync(DocFilePath));
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("DnsServers DnsBackgroundWorker AutoUpdate_SettingsAndGroups (Create Backup): " + Environment.NewLine + ex.GetInnerExceptions());
                        }
                    }

                    CTS_BackgroundTask = new();
                    updated = await AutoUpdate_SettingsAndGroups_InternalAsync(CTS_BackgroundTask.Token);
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