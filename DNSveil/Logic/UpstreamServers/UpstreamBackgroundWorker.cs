using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using MsmhToolsClass;
using static DNSveil.Logic.UpstreamServers.UpstreamModel;

namespace DNSveil.Logic.UpstreamServers;

public partial class UpstreamServersManager
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
        public UpstreamGroup Group { get; set; } = new();
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
    /// Scan UpstreamItems In A Group
    /// </summary>
    /// <param name="group">Upstream Group</param>
    /// <param name="selectedItemList">NULL: All UpstreamItems In The Group</param>
    /// <param name="isBackground">True: Do Not Update UI</param>
    /// <returns></returns>
    private async Task ScanServersAsync(UpstreamGroup group, ObservableCollection<UpstreamItem>? selectedItemList, bool isBackground)
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

            if (!IsScanning && group.Mode != GroupMode.None)
            {
                IsScanning = true;
                CTS_Scan = new();
                bw.ButtonText = "Stop";
                bw.ButtonEnable = true;
                bw.Description = "Scanning...";

                // Get Group Settings And Clone (We Need To Modify It)
                GroupSettings gs = Get_GroupSettings(group.Name).Clone_GroupSettings();

                if (isBackground)
                {
                    if (gs.ScanParallelSize > 5) gs.ScanParallelSize = 5; // Limit Parallel Size For Background Task
                    gs.TestSpeed = false; // Disable Speed Test For Background Task
                }

                // Clear Previous Result
                bool clearDescription = group.Mode == GroupMode.Subscription;
                if (!isCustomRange) await Clear_UpstreamItems_Result_Async(group.Name, clearDescription, false);

                selectedItemList ??= Get_UpstreamItems(group.Name);

                if (!isBackground)
                {
                    bw.ProgressMax = selectedItemList.Count;
                    GetBackgroundWorkerStatus = bw;
                    POnBackgroundUpdateReceived?.Invoke(Owner, bw);
                }

                var selectedItemLists = selectedItemList.SplitToLists(gs.ScanParallelSize);

                for (int n = 0; n < selectedItemLists.Count; n++)
                {
                    if (StopScanning) break;
                    if (isBackground && bw.SelectedServers >= 20) break;
                    if (isBackground && PauseBackgroundTask) break;

                    ObservableCollection<UpstreamItem> itemList = selectedItemLists[n];
                    var spr = await ScanUpstream.ScanUpstreamInParallelAsync(gs, itemList, RegionCaches, CTS_Scan.Token);
                    itemList = spr.Items;
                    bw.OnlineServers += spr.ParallelResult.OnlineServers;
                    bw.SelectedServers += spr.ParallelResult.SelectedServers;
                    bw.SumLatencyMS += spr.ParallelResult.SumLatencyMS;
                    bw.ProgressValue += itemList.Count;
                    bw.ParallelLatencyMS = spr.ParallelResult.ParallelLatencyMS;

                    // Update LastScanServers
                    if (bw.OnlineServers > 0)
                    {
                        if (group.Mode == GroupMode.Subscription)
                        {
                            SubscriptionOptions options = Get_Subscription_Options(group.Name);
                            options.AutoUpdate.LastScanServers = DateTime.Now;
                            await Update_Subscription_Options_Async(group.Name, options, false);
                        }
                        else if (group.Mode == GroupMode.Custom)
                        {
                            CustomOptions options = Get_Custom_Options(group.Name);
                            options.AutoUpdate.LastScanServers = DateTime.Now;
                            await Update_Custom_Options_Async(group.Name, options, false);
                        }
                    }
                    
                    await Update_UpstreamItems_Async(group.Name, itemList, false);
                    await Sort_UpstreamItems_Async(group.Name, true);

                    if (!isBackground)
                    {
                        if (n == selectedItemLists.Count - 1) bw.ProgressValue = bw.ProgressMax;
                        try
                        {
                            if (isCustomRange && itemList.Count > 0)
                                bw.LastIndex = Get_IndexOf_UpstreamItem(group.Name, itemList[^1]);
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
            Debug.WriteLine("UpstreamServers UpstreamBackgroundWorker ScanServersAsync: " + ex.Message);
        }
    }


    public async void ScanServers(UpstreamGroup group, ObservableCollection<UpstreamItem>? selectedItemList)
    {
        await ScanServersAsync(group, selectedItemList, false);
    }

    private async Task<bool> SubscriptionFetchSourceAsync(UpstreamGroup group, CancellationToken ct)
    {
        try
        {
            if (group.Subscription == null) return false;
            if (group.Subscription.Source.URLs.Count == 0) return false;

            List<string> allItems = new();
            for (int n = 0; n < group.Subscription.Source.URLs.Count; n++)
            {
                string urlOrFile = group.Subscription.Source.URLs[n];
                List<string> upstreams = await WebAPI.GetLinesFromTextLinkAsync(urlOrFile, 20000, ct);
                if (PauseBackgroundTask)
                {
                    upstreams.Clear();
                    allItems.Clear();
                    return false;
                }

                allItems.AddRange(upstreams);
            }

            if (PauseBackgroundTask)
            {
                allItems.Clear();
                return false;
            }

            // Convert To UpstreamItem
            ObservableCollection<UpstreamItem> items = new();
            for (int n = 0; n < allItems.Count; n++)
            {
                string upstreamURL = allItems[n];
                UpstreamItem item = new(upstreamURL);
                if (item.ConfigInfo.IsSuccess) items.Add(item);
            }

            // DeDup By Unique ID
            items = items.DistinctBy(_ => _.ConfigInfo.IDUnique).ToObservableCollection();

            if (items.Count == 0)
            {
                allItems.Clear();
                return false;
            }

            // Add To Group
            bool isSuccess = await Add_UpstreamItems_Async(group.Name, items, false);
            // Update Last AutoUpdate
            SubscriptionOptions options = Get_Subscription_Options(group.Name);
            options.AutoUpdate.LastUpdateSource = DateTime.Now;
            await Update_Subscription_Options_Async(group.Name, options, true);

            allItems.Clear();
            return isSuccess;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("UpstreamServers UpstreamBackgroundWorker SubscriptionFetchSourceAsync: " + ex.Message);
            return false;
        }
    }

    private async Task<bool> AutoUpdate_SettingsAndGroups_InternalAsync(CancellationToken ct)
    {
        bool updated = false;

        try
        {
            // Update Groups
            List<UpstreamGroup> groups = Get_Groups(true);
            for (int n = 0; n < groups.Count; n++)
            {
                if (PauseBackgroundTask) return updated;
                UpstreamGroup group = groups[n];
                UpstreamItemsInfo info = Get_UpstreamItems_Info(group.Name);
                if (group.Mode == GroupMode.Subscription && group.Subscription != null)
                {
                    // Update Source
                    if (group.Subscription.Options.AutoUpdate.UpdateSource > 0)
                    {
                        bool isUpdateTime_ByItems = group.Items.Count == 0;
                        bool isUpdateTime_ByDate = (DateTime.Now - group.Subscription.Options.AutoUpdate.LastUpdateSource) >= new TimeSpan(group.Subscription.Options.AutoUpdate.UpdateSource, 0, 0);
                        if (isUpdateTime_ByItems || isUpdateTime_ByDate)
                        {
                            updated = await SubscriptionFetchSourceAsync(group, ct);
                            if (PauseBackgroundTask) return updated;
                        }
                    }

                    // Scan
                    if (!group.Subscription.Options.AutoUpdate.AutoScanSelect && group.Subscription.Options.AutoUpdate.ScanServers > 0 && info.TotalServers > 0)
                    {
                        bool isScanTime_ByItems = info.SelectedServers == 0;
                        bool isScanTime_ByDate = (DateTime.Now - group.Subscription.Options.AutoUpdate.LastScanServers) >= new TimeSpan(group.Subscription.Options.AutoUpdate.ScanServers, 0, 0);
                        if (isScanTime_ByItems || isScanTime_ByDate)
                        {
                            await ScanServersAsync(group, null, true);
                            updated = true;
                            if (PauseBackgroundTask) return updated;
                            await Sort_UpstreamItems_Async(group.Name, true);
                            if (PauseBackgroundTask) return updated;
                        }
                    }
                }
                else if (group.Mode == GroupMode.Custom && group.Custom != null)
                {
                    // Update Source: We Don't Have Source In Custom Mode

                    // Scan
                    if (!group.Custom.Options.AutoUpdate.AutoScanSelect && group.Custom.Options.AutoUpdate.ScanServers > 0 && info.TotalServers > 0)
                    {
                        bool isScanTime_ByItems = info.SelectedServers == 0;
                        bool isScanTime_ByDate = (DateTime.Now - group.Custom.Options.AutoUpdate.LastScanServers) >= new TimeSpan(group.Custom.Options.AutoUpdate.ScanServers, 0, 0);
                        if (isScanTime_ByItems || isScanTime_ByDate)
                        {
                            await ScanServersAsync(group, null, true);
                            updated = true;
                            if (PauseBackgroundTask) return updated;
                            await Sort_UpstreamItems_Async(group.Name, true);
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
            Debug.WriteLine("UpstreamServers UpstreamBackgroundWorker AutoUpdate_SettingsAndGroups_InternalAsync: " + ex.Message);
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
                            Debug.WriteLine("Upstream Scan Cancellation Request Sent.");
                        }
                    }
                    if (PauseBackgroundTask)
                    {
                        if (!CTS_BackgroundTask.IsCancellationRequested)
                        {
                            CTS_BackgroundTask.Cancel();
                            Debug.WriteLine("Upstream Background Cancellation Request Sent.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("UpstreamServers UpstreamBackgroundWorker CheckPauseBackground: " + Environment.NewLine + ex.GetInnerExceptions());
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
                            Debug.WriteLine("UpstreamServers UpstreamBackgroundWorker AutoUpdate_SettingsAndGroups (Create Backup): " + Environment.NewLine + ex.GetInnerExceptions());
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