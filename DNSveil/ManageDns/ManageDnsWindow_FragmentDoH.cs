using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MsmhToolsClass;
using MsmhToolsWpfClass;
using MsmhToolsClass.MsmhAgnosticServer;
using DNSveil.Logic.DnsServers;
using static DNSveil.Logic.DnsServers.DnsModel;
using static MsmhToolsClass.MsmhAgnosticServer.AgnosticProgram;
using System.Collections.ObjectModel;

namespace DNSveil.ManageDns;

public partial class ManageDnsWindow : WpfWindow
{
    private void ChangeControlsState_FragmentDoH(bool enable)
    {
        this.DispatchIt(() =>
        {
            if (PART_Button1 != null) PART_Button1.IsEnabled = enable;
            FragmentDoH_Settings_WpfFlyoutPopup.IsHitTestVisible = enable;
            if (enable || (!enable && IsScanning))
                FragmentDoHSourceStackPanel.IsEnabled = enable;
            FragmentDoHByManualAddButton.IsEnabled = enable;
            FragmentDoHByManualModifyButton.IsEnabled = enable;
            FragmentDoHSaveSourceButton.IsEnabled = enable;
            FragmentDoHFetchSourceButton.IsEnabled = enable;
            FragmentDoHSaveOptionsButton.IsEnabled = enable;
            FragmentDoHExportAsTextButton.IsEnabled = enable;
            if (enable || (!enable && !IsScanning))
                FragmentDoHScanButton.IsEnabled = enable;
            FragmentDoHSortButton.IsEnabled = enable;
            DGS_FragmentDoH.IsHitTestVisible = enable;

            NewGroupButton.IsEnabled = enable;
            ResetBuiltInButton.IsEnabled = enable;
            Import_FlyoutOverlay.IsHitTestVisible = enable;
            Export_FlyoutOverlay.IsHitTestVisible = enable;
            ImportButton.IsEnabled = enable;
            ExportButton.IsEnabled = enable;
            DGG.IsHitTestVisible = enable;
        });
    }

    private async void FragmentDoH_Settings_WpfFlyoutPopup_FlyoutChanged(object sender, WpfFlyoutPopup.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            await LoadSelectedGroupAsync(true);
        }
    }

    private void Flyout_FragmentDoH_Source_FlyoutChanged(object sender, WpfFlyoutGroupBox.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            Flyout_FragmentDoH_Options.IsOpen = false;
        }
    }

    private void Flyout_FragmentDoH_Options_FlyoutChanged(object sender, WpfFlyoutGroupBox.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            Flyout_FragmentDoH_Source.IsOpen = false;
        }
    }

    private void FragmentDoH_ToggleSourceByUrlByManual(bool? byManual)
    {
        if (!byManual.HasValue) return;
        if (byManual.Value)
        {
            // By Manual
            FragmentDoHSourceByUrlGroupBox.IsEnabled = false;
            FragmentDoHSourceByUrlGroupBox.Visibility = Visibility.Collapsed;
            FragmentDoHSourceByManualGroupBox.IsEnabled = true;
            FragmentDoHSourceByManualGroupBox.Visibility = Visibility.Visible;
        }
        else
        {
            // By URL
            FragmentDoHSourceByUrlGroupBox.IsEnabled = true;
            FragmentDoHSourceByUrlGroupBox.Visibility = Visibility.Visible;
            FragmentDoHSourceByManualGroupBox.IsEnabled = false;
            FragmentDoHSourceByManualGroupBox.Visibility = Visibility.Collapsed;
        }
    }

    private async void FragmentDoHSourceByUrlByManualToggleSwitch_CheckedChanged(object sender, WpfToggleSwitch.CheckedChangedEventArgs e)
    {
        if (e.IsChecked.HasValue)
        {
            FragmentDoH_ToggleSourceByUrlByManual(e.IsChecked.Value);

            // Update Source Enable
            if (DGG.SelectedItem is not DnsGroup group) return;
            await MainWindow.DnsManager.Update_Source_EnableDisable_Async(group.Name, !e.IsChecked.Value, true);
        }
    }

    private async void DGS_FragmentDoH_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (sender is not DataGrid dg) return;
            if (e.MouseDevice.DirectlyOver is DependencyObject dependencyObject)
            {
                DataGridRow? row = dependencyObject.GetParentOfType<DataGridRow>();
                if (row != null && row.Item is DnsItem)
                {
                    bool isByManual = FragmentDoHSourceByUrlByManualToggleSwitch.IsChecked.HasValue && FragmentDoHSourceByUrlByManualToggleSwitch.IsChecked.Value;
                    MenuItem_FragmentDoH_RemoveDuplicates.IsEnabled = isByManual;
                    MenuItem_FragmentDoH_DeleteItem.IsEnabled = isByManual;

                    bool isMultiSelect = dg.SelectedItems.Count > 1;
                    if (isMultiSelect)
                    {
                        MenuItem_FragmentDoH_CopyDohAddress.IsEnabled = false;
                    }
                    else
                    {
                        MenuItem_FragmentDoH_CopyDohAddress.IsEnabled = true;
                        dg.SelectedIndex = row.GetIndex();
                    }

                    ObservableCollection<DnsItem> selectedItems = new();
                    for (int n = 0; n < dg.SelectedItems.Count; n++)
                    {
                        if (dg.SelectedItems[n] is DnsItem selectedItem)
                            selectedItems.Add(selectedItem);
                    }
                    ContextMenu_FragmentDoH.Tag = selectedItems;

                    await Task.Delay(RightClickMenuDelayMS); // Let Row Get Selected When Clicking On A Non-Selected Row
                    ContextMenu_FragmentDoH.IsOpen = true;
                }
            }
        }
        catch (Exception) { }
    }

    private void DGS_FragmentDoH_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not DataGrid dg) return;
        dg.FillCustomColumn(1, 200);
    }

    private void DGS_FragmentDoH_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not DataGrid dg) return;
        DGS_DnsServers_PreviewKeyDown(dg, e);
    }

    private void DGS_FragmentDoH_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not DataGrid dg) return;
        DGS_DnsServers_SelectionChanged(dg);
    }

    private void MenuItem_FragmentDoH_CopyDohAddress_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ContextMenu_FragmentDoH.Tag is ObservableCollection<DnsItem> items && items.Count > 0)
            {
                DnsItem dnsItem = items[0];
                string text = $"{dnsItem.DNS_URL} {dnsItem.DNS_IP}";
                Clipboard.SetText(text, TextDataFormat.Text);
                WpfToastDialog.Show(this, "Copied To Clipboard.", MessageBoxImage.None, 2);
            }
        }
        catch (Exception) { }
    }

    private async Task FragmentDoH_RemoveDuplicates_Async(bool showToast)
    {
        try
        {
            if (IsScanning)
            {
                if (showToast) WpfToastDialog.Show(this, "Can't Remove Duplicates While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not DnsGroup group) return;

            // DeDup
            int nextIndex = GetPreviousOrNextIndex(DGS_FragmentDoH, false); // Get Next Index
            bool isSuccess = await MainWindow.DnsManager.DeDup_DnsItems_Async(group.Name, true);
            await LoadSelectedGroupAsync(); // Refresh
            await DGS_FragmentDoH.ScrollIntoViewAsync(nextIndex); // Scroll To Next

            if (showToast)
            {
                if (isSuccess)
                    WpfToastDialog.Show(this, "Duplicates Removed.", MessageBoxImage.None, 2);
                else
                    WpfToastDialog.Show(this, "ERROR: Couldn't Remove Duplicates.", MessageBoxImage.Error, 2);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_FragmentDoH FragmentDoH_RemoveDuplicates_Async: " + ex.Message);
        }
    }

    private async void MenuItem_FragmentDoH_RemoveDuplicates_Click(object sender, RoutedEventArgs e)
    {
        ChangeControlsState_FragmentDoH(false);
        await FragmentDoH_RemoveDuplicates_Async(true);
        ChangeControlsState_FragmentDoH(true);
    }

    private async void MenuItem_FragmentDoH_DeleteItem_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Delete While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not DnsGroup group) return;
            if (DGS_FragmentDoH.SelectedItem is not DnsItem currentItem) return;

            List<DnsItem> selectedItems = new();
            for (int n = 0; n < DGS_FragmentDoH.SelectedItems.Count; n++)
            {
                if (DGS_FragmentDoH.SelectedItems[n] is DnsItem selectedItem)
                    selectedItems.Add(selectedItem);
            }

            // Confirm Delete
            string msg = $"Deleting \"{currentItem.DNS_URL}\"...{NL}Continue?";
            if (selectedItems.Count > 1) msg = $"Deleting Selected DoH Items...{NL}Continue?";
            MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbr != MessageBoxResult.Yes) return;

            // Delete
            int nextIndex = GetPreviousOrNextIndex(DGS_FragmentDoH, true); // Get Next Index
            bool isSuccess = await MainWindow.DnsManager.Remove_DnsItems_Async(group.Name, selectedItems, true);
            await LoadSelectedGroupAsync(); // Refresh
            await DGS_FragmentDoH.ScrollIntoViewAsync(nextIndex); // Scroll To Next

            if (isSuccess)
                WpfToastDialog.Show(this, "Deleted.", MessageBoxImage.None, 2);
            else
                WpfToastDialog.Show(this, "ERROR: Couldn't Delete Items.", MessageBoxImage.Error, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_FragmentDoH MenuItem_FragmentDoH_DeleteItem_Click: " + ex.Message);
        }
    }

    private void MenuItem_FragmentDoH_Scan_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGG.SelectedItem is not DnsGroup group) return;
            if (ContextMenu_FragmentDoH.Tag is ObservableCollection<DnsItem> items && items.Count > 0)
            {
                MainWindow.DnsManager.ScanServers(group, items);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_FragmentDoH MenuItem_Custom_Scan_Click: " + NL + ex.Message);
        }
    }

    private async Task FragmentDoH_Settings_Save_Async()
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Save While Scanning.", MessageBoxImage.Stop, 2);
                await FragmentDoH_Settings_WpfFlyoutPopup.CloseFlyAsync();
                return;
            }

            if (DGG.SelectedItem is not DnsGroup group) return;

            // Get Group Settings
            bool enabled = FragmentDoH_EnableGroup_ToggleSwitch.IsChecked.HasValue && FragmentDoH_EnableGroup_ToggleSwitch.IsChecked.Value;
            string lookupDomain = FragmentDoH_Settings_LookupDomain_TextBox.Text.Trim();
            double timeoutSec = FragmentDoH_Settings_TimeoutSec_NumericUpDown.Value;
            int parallelSize = FragmentDoH_Settings_ParallelSize_NumericUpDown.Value.ToInt();
            string bootstrapIpStr = FragmentDoH_Settings_BootstrapIP_TextBox.Text.Trim();
            int bootstrapPort = FragmentDoH_Settings_BootstrapPort_NumericUpDown.Value.ToInt();
            int maxServersToConnect = FragmentDoH_Settings_MaxServersToConnect_NumericUpDown.Value.ToInt();
            bool allowInsecure = FragmentDoH_Settings_AllowInsecure_ToggleSwitch.IsChecked.HasValue && FragmentDoH_Settings_AllowInsecure_ToggleSwitch.IsChecked.Value;

            if (lookupDomain.StartsWith("http://")) lookupDomain = lookupDomain.TrimStart("http://");
            if (lookupDomain.StartsWith("https://")) lookupDomain = lookupDomain.TrimStart("https://");

            if (string.IsNullOrWhiteSpace(lookupDomain) || !lookupDomain.Contains('.') || lookupDomain.Contains("//"))
            {
                string msg = $"Domain Is Invalid.{NL}{LS}e.g. example.com";
                WpfMessageBox.Show(this, msg, "Invalid Domain", MessageBoxButton.OK, MessageBoxImage.Stop);
                await FragmentDoH_Settings_WpfFlyoutPopup.OpenFlyAsync();
                return;
            }

            bool isBoostrapIP = NetworkTool.IsIP(bootstrapIpStr, out _);
            if (!isBoostrapIP)
            {
                string msg = $"Bootstrap IP Is Invalid.{NL}{LS}e.g.  8.8.8.8  Or  2001:4860:4860::8888";
                WpfMessageBox.Show(this, msg, "Invalid IP", MessageBoxButton.OK, MessageBoxImage.Stop);
                await FragmentDoH_Settings_WpfFlyoutPopup.OpenFlyAsync();
                return;
            }

            // Update Group Settings
            GroupSettings gs = new(enabled, lookupDomain, timeoutSec, parallelSize, bootstrapIpStr, bootstrapPort, maxServersToConnect, allowInsecure);
            bool isSuccess = await MainWindow.DnsManager.Update_GroupSettings_Async(group.Name, gs, true);

            await LoadSelectedGroupAsync(); // Refresh
            await FragmentDoH_Settings_WpfFlyoutPopup.CloseFlyAsync();

            if (isSuccess)
                WpfToastDialog.Show(this, "Saved.", MessageBoxImage.None, 2);
            else
                WpfToastDialog.Show(this, "ERROR: Couldn't Save Settings.", MessageBoxImage.Error, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_FragmentDoH FragmentDoH_Settings_Save_Async: " + ex.Message);
        }
    }

    private async void FragmentDoH_Settings_Save_Button_Click(object sender, RoutedEventArgs e)
    {
        ChangeControlsState_FragmentDoH(false);
        await FragmentDoH_Settings_Save_Async();
        ChangeControlsState_FragmentDoH(true);
    }

    private async Task FragmentDoH_ByManualAddOrModify_Async(bool TrueAdd_FalseModify)
    {
        try
        {
            if (IsScanning)
            {
                string addModify = TrueAdd_FalseModify ? "Add" : "Modify";
                WpfToastDialog.Show(this, $"Can't {addModify} While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not DnsGroup group) return;

            DnsItem currentItem = new();
            if (!TrueAdd_FalseModify)
            {
                if (DGS_FragmentDoH.SelectedItem is DnsItem currentItemOut) currentItem = currentItemOut;
                else
                {
                    WpfToastDialog.Show(this, $"Select An Item To Modify.", MessageBoxImage.Stop, 2);
                    return;
                }
            }
            string doH_URL = string.Empty, doH_IpStr = string.Empty;

            this.DispatchIt(() =>
            {
                doH_URL = FragmentDoHByManualUrlTextBox.Text.Trim();
                doH_IpStr = FragmentDoHByManualIpTextBox.Text.Trim();
            });

            NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(doH_URL, 443);
            bool isHostIP = NetworkTool.IsIP(urid.Host, out _);
            if (isHostIP)
            {
                WpfMessageBox.Show(this, "Host Must Be A Domain.", "Not A Domain", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            bool isIpIP = NetworkTool.IsIP(doH_IpStr, out IPAddress? doH_IP);
            if (!isIpIP || doH_IP == null)
            {
                WpfMessageBox.Show(this, "IP Address Is Not Correct.", "Not An IP", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            bool isLocalIP = NetworkTool.IsLocalIP(doH_IpStr);
            if (isLocalIP)
            {
                WpfMessageBox.Show(this, "IP Address Can't Be Local.", "Local IP", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            DnsItem newItem = new(doH_URL);
            DnsReader dnsReader = newItem.DnsReader;
            if (dnsReader.Protocol != DnsEnums.DnsProtocol.DoH)
            {
                WpfMessageBox.Show(this, "DNS Must Be A DoH.", "Not A DoH", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            // Set DoH_IP
            newItem.DNS_IP = doH_IP;

            if (TrueAdd_FalseModify)
            {
                // Check If Already Exist
                ObservableCollection<DnsItem> dnsItems = MainWindow.DnsManager.Get_DnsItems(group.Name);
                bool alreadyExist = dnsItems.Any(_ => _.DNS_URL.Equals(newItem.DNS_URL) && _.DNS_IP.Equals(newItem.DNS_IP));
                if (alreadyExist)
                {
                    WpfMessageBox.Show(this, "The DoH Already Exist.", "Exist", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
                dnsItems.Clear();

                // Add
                ObservableCollection<DnsItem> newItemList = new() { newItem };
                bool isSuccess = await MainWindow.DnsManager.Append_DnsItems_Async(group.Name, newItemList, true);
                await LoadSelectedGroupAsync(); // Refresh
                await DGS_FragmentDoH.ScrollIntoViewAsync(DGS_FragmentDoH.Items.Count - 1); // Scroll To Last Item

                if (isSuccess)
                    WpfToastDialog.Show(this, "Added.", MessageBoxImage.None, 2);
                else
                    WpfToastDialog.Show(this, "ERROR: Couldn't Add The Item.", MessageBoxImage.Error, 2);
            }
            else
            {
                // Modify
                bool isSuccess = await MainWindow.DnsManager.Update_DnsItems_Async(group.Name, currentItem.IDUnique, newItem, true);
                await LoadSelectedGroupAsync(); // Refresh

                if (isSuccess)
                    WpfToastDialog.Show(this, "Modified.", MessageBoxImage.None, 2);
                else
                    WpfToastDialog.Show(this, "ERROR: Couldn't Modify The Item.", MessageBoxImage.Error, 2);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_FragmentDoH FragmentDoHByManualAddOrModifyAsync: " + ex.Message);
        }
    }

    private async void FragmentDoHByManualAddButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeControlsState_FragmentDoH(false);
        await FragmentDoH_ByManualAddOrModify_Async(true);
        ChangeControlsState_FragmentDoH(true);
    }

    private async void FragmentDoHByManualModifyButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeControlsState_FragmentDoH(false);
        await FragmentDoH_ByManualAddOrModify_Async(false);
        ChangeControlsState_FragmentDoH(true);
    }

    private async Task FragmentDoHSaveSourceAsync(bool showToast)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Save While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not DnsGroup group) return;

            // Update URLs
            List<string> urls = FragmentDoHSourceTextBox.Text.SplitToLines();
            bool isSuccess = await MainWindow.DnsManager.Update_Source_URLs_Async(group.Name, urls, true);
            await LoadSelectedGroupAsync(); // Refresh

            if (showToast)
            {
                if (isSuccess)
                    WpfToastDialog.Show(this, "Saved.", MessageBoxImage.None, 2);
                else
                    WpfToastDialog.Show(this, "ERROR: Couldn't Save Source URLs.", MessageBoxImage.Error, 2);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_FragmentDoH FragmentDoHSaveSourceAsync: " + ex.Message);
        }
    }

    private async void FragmentDoHSaveSourceButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeControlsState_FragmentDoH(false);
        await FragmentDoHSaveSourceAsync(true);
        ChangeControlsState_FragmentDoH(true);
    }

    private async void FragmentDoHFetchSourceButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsScanning)
        {
            WpfToastDialog.Show(this, "Can't Fetch While Scanning.", MessageBoxImage.Stop, 2);
            return;
        }

        if (DGG.SelectedItem is not DnsGroup group) return;
        if (sender is not WpfButton b) return;
        ChangeControlsState_FragmentDoH(false);
        b.DispatchIt(() => b.Content = "Fetching...");

        try
        {
            WpfToastDialog.Show(this, "Do Not Close This Window.", MessageBoxImage.None, 2);

            // Save First
            await FragmentDoHSaveSourceAsync(false);

            List<string> urlsOrFiles = FragmentDoHSourceTextBox.Text.SplitToLines(StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (urlsOrFiles.Count == 0)
            {
                string msg = "Add URL Or File Path.";
                WpfMessageBox.Show(this, msg);
                end();
                return;
            }

            List<string> allDNSs = new();
            for (int n = 0; n < urlsOrFiles.Count; n++)
            {
                string urlOrFile = urlsOrFiles[n];
                List<string> dnss = await DnsTools.GetServersFromLinkAsync(urlOrFile, 30000, CancellationToken.None);
                allDNSs.AddRange(dnss);
            }
            
            // Get Malicious Domains
            DnsSettings settings = MainWindow.DnsManager.Get_Settings();
            List<string> maliciousDomains = settings.MaliciousDomains.Get_Malicious_Domains;

            // Convert To DnsItem
            ObservableCollection<DnsItem> allDoHStamps = Tools.Convert_DNSs_To_DnsItem_ForFragmentDoH(allDNSs, maliciousDomains);
            if (allDoHStamps.Count == 0)
            {
                allDNSs.Clear();
                string msg = "Couldn't Find Any Proper DoH Stamp Server!";
                WpfMessageBox.Show(this, msg);
                end();
                return;
            }

            // Add To Group => DnsItems
            bool isSuccess = await MainWindow.DnsManager.Add_DnsItems_Async(group.Name, allDoHStamps, false);

            // Get Group Options
            FragmentDoHOptions options = MainWindow.DnsManager.Get_FragmentDoH_Options(group.Name);
            options.AutoUpdate.LastUpdateSource = DateTime.Now; // Update LastUpdateSource
            await MainWindow.DnsManager.Update_FragmentDoH_Options_Async(group.Name, options, true);

            await LoadSelectedGroupAsync(); // Refresh

            if (isSuccess)
                WpfToastDialog.Show(this, $"Fetched {allDoHStamps.Count} Servers.", MessageBoxImage.None, 2);
            else
                WpfToastDialog.Show(this, "ERROR: Couldn't Add Fetched Servers.", MessageBoxImage.Error, 2);

            // Dispose - Return
            allDoHStamps.Clear();
            allDNSs.Clear();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_FragmentDoH FragmentDoHFetchSourceButton_Click: " + ex.Message);
        }

        end();
        void end()
        {
            b.DispatchIt(() => b.Content = "Fetch Servers");
            ChangeControlsState_FragmentDoH(true);
        }
    }

    private async void FragmentDoHSaveOptionsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Save While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not DnsGroup group) return;
            ChangeControlsState_FragmentDoH(false);

            // Update Options
            // AutoUpdate
            int updateSource = FragmentDoHUpdateSourceNumericUpDown.Value.ToInt();
            int scanServers = FragmentDoHScanServersNumericUpDown.Value.ToInt();

            // FilterByProperties
            DnsFilter google = Tools.BoolToDnsFilter(FragmentDoHFilter_Google_CheckBox.IsChecked);
            DnsFilter bing = Tools.BoolToDnsFilter(FragmentDoHFilter_Bing_CheckBox.IsChecked);
            DnsFilter youtube = Tools.BoolToDnsFilter(FragmentDoHFilter_Youtube_CheckBox.IsChecked);
            DnsFilter adult = Tools.BoolToDnsFilter(FragmentDoHFilter_Adult_CheckBox.IsChecked);
            FilterByProperties filterByProperties = new(google, bing, youtube, adult);

            // Fragment Settings
            int chunksBeforeSNI = FragmentDoHSettings_ChunksBeforeSNI_NumericUpDown.Value.ToInt();
            int sniChunkModeInt = FragmentDoHSettings_SniChunkMode_ComboBox.SelectedIndex;
            Fragment.ChunkMode sniChunkMode = sniChunkModeInt switch
            {
                0 => Fragment.ChunkMode.SNI,
                1 => Fragment.ChunkMode.SniExtension,
                2 => Fragment.ChunkMode.AllExtensions,
                _ => Fragment.ChunkMode.SNI,
            };
            int chunksSNI = FragmentDoHSettings_ChunksSNI_NumericUpDown.Value.ToInt();
            int antiPatternOffset = FragmentDoHSettings_AntiPatternOffset_NumericUpDown.Value.ToInt();
            int fragmentDelayMS = FragmentDoHSettings_FragmentDelayMS_NumericUpDown.Value.ToInt();
            FragmentSettings fragmentSettings = new(chunksBeforeSNI, sniChunkMode, chunksSNI, antiPatternOffset, fragmentDelayMS);

            // Update Options
            FragmentDoHOptions options = MainWindow.DnsManager.Get_FragmentDoH_Options(group.Name);
            options.AutoUpdate.UpdateSource = updateSource;
            options.AutoUpdate.ScanServers = scanServers;
            options.FilterByProperties = filterByProperties;
            options.FragmentSettings = fragmentSettings;

            // Save/Update
            bool isSuccess = await MainWindow.DnsManager.Update_FragmentDoH_Options_Async(group.Name, options, false);

            // Update DnsItem Selection By Options
            await MainWindow.DnsManager.Select_DnsItems_ByOptions_Async(group.Name, options.FilterByProperties, true);

            await LoadSelectedGroupAsync(); // Refresh

            if (isSuccess)
                WpfToastDialog.Show(this, "Saved.", MessageBoxImage.None, 2);
            else
                WpfToastDialog.Show(this, "ERROR: Couldn't Save Options.", MessageBoxImage.Error, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_FragmentDoH FragmentDoHSaveOptionsButton_Click: " + ex.Message);
        }

        ChangeControlsState_FragmentDoH(true);
    }

    private async void FragmentDoHExportAsTextButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Export While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not DnsGroup group) return;
            ChangeControlsState_FragmentDoH(false);

            List<string> exportList = new();
            ObservableCollection<DnsItem> items = MainWindow.DnsManager.Get_DnsItems(group.Name);
            for (int n = 0; n < items.Count; n++)
            {
                DnsItem item = items[n];
                if (item.IsSelected)
                {
                    // Create SDNS
                    bool isNoFilter = item.IsGoogleSafeSearchEnabled == DnsFilter.No &&
                                      item.IsBingSafeSearchEnabled == DnsFilter.No &&
                                      item.IsYoutubeRestricted == DnsFilter.No &&
                                      item.IsAdultBlocked == DnsFilter.No;
                    NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(item.DNS_URL, 443);
                    string stamp = DNSCryptStampGenerator.GenerateDoH(item.DNS_IP.ToString(), null, $"{urid.Host}:{urid.Port}", urid.Path, null, false, false, isNoFilter);

                    exportList.Add(stamp);
                }
            }

            if (exportList.Count == 0)
            {
                WpfToastDialog.Show(this, "There Is Nothing To Export.", MessageBoxImage.Stop, 2);
                ChangeControlsState_FragmentDoH(true);
                return;
            }

            // Convert To Text
            string export = exportList.ToString(NL);

            // Open Save File Dialog
            SaveFileDialog sfd = new()
            {
                Filter = "TXT DNS Servers (*.txt)|*.txt",
                DefaultExt = ".txt",
                AddExtension = true,
                RestoreDirectory = true,
                FileName = $"DNSveil_Export_{group.Name}_Selected_DNS_Servers_{DateTime.Now:yyyy.MM.dd-HH.mm.ss}"
            };

            bool? dr = sfd.ShowDialog(this);
            if (dr.HasValue && dr.Value)
            {
                try
                {
                    await File.WriteAllTextAsync(sfd.FileName, export, new UTF8Encoding(false));
                    string msg = "Selected Servers Exported Successfully.";
                    WpfMessageBox.Show(this, msg, "Exported", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    WpfMessageBox.Show(this, ex.Message, "Something Went Wrong!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Dispose
            export = string.Empty;
            items.Clear();
            exportList.Clear();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_FragmentDoH FragmentDoHExportAsTextButton_Click: " + ex.Message);
        }

        ChangeControlsState_FragmentDoH(true);
    }

    private void FragmentDoHScanButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGG.SelectedItem is not DnsGroup group) return;
            FragmentDoHScanButton.IsEnabled = false;

            // Check For Fetch: Get DnsItems Info
            DnsItemsInfo info = MainWindow.DnsManager.Get_DnsItems_Info(group.Name);
            if (info.TotalServers == 0)
            {
                string msg = "Fetch Or Add Servers To Scan.";
                WpfMessageBox.Show(this, msg, "No Server To Scan!", MessageBoxButton.OK, MessageBoxImage.Information);
                FragmentDoHScanButton.IsEnabled = true;
                return;
            }

            if (DGS_FragmentDoH.SelectedItems.Count > 1)
            {
                ObservableCollection<DnsItem> selectedItems = new();
                for (int n = 0; n < DGS_FragmentDoH.SelectedItems.Count; n++)
                {
                    if (DGS_FragmentDoH.SelectedItems[n] is DnsItem selectedItem)
                        selectedItems.Add(selectedItem);
                }
                MainWindow.DnsManager.ScanServers(group, selectedItems);
            }
            else
            {
                MainWindow.DnsManager.ScanServers(group, null);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_FragmentDoH FragmentDoHScanButton_Click: " + ex.Message);
        }
    }

    private async void FragmentDoHSortButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Sort While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not DnsGroup group) return;
            if (sender is not WpfButton b) return;
            ChangeControlsState_FragmentDoH(false);
            b.DispatchIt(() => b.Content = "Sorting...");

            // Sort
            await MainWindow.DnsManager.Sort_DnsItems_Async(group.Name, true);
            await DGS_FragmentDoH.ScrollIntoViewAsync(0); // Scroll

            b.DispatchIt(() => b.Content = "Sort");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_FragmentDoH FragmentDoHSortButton_Click: " + ex.Message);
        }

        ChangeControlsState_FragmentDoH(true);
    }

}