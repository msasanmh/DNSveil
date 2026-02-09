using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MsmhToolsClass;
using MsmhToolsWpfClass;
using MsmhToolsClass.MsmhAgnosticServer;
using static DNSveil.Logic.DnsServers.DnsModel;
using System.Collections.ObjectModel;
using DNSveil.Logic.DnsServers;

namespace DNSveil.ManageDns;

public partial class ManageDnsWindow : WpfWindow
{
    private void ChangeControlsState_AnonDNSCrypt(bool enable)
    {
        this.DispatchIt(() =>
        {
            if (PART_Button1 != null) PART_Button1.IsEnabled = enable;
            AnonDNSCrypt_Settings_WpfFlyoutPopup.IsHitTestVisible = enable;
            AnonDNSCryptRelayBrowseButton.IsEnabled = enable;
            AnonDNSCryptTargetBrowseButton.IsEnabled = enable;
            AnonDNSCryptSaveSourceButton.IsEnabled = enable;
            AnonDNSCryptFetchSourceButton.IsEnabled = enable;
            AnonDNSCryptSaveOptionsButton.IsEnabled = enable;
            AnonDNSCryptExportAsTextButton.IsEnabled = enable;
            if (enable || (!enable && !IsScanning))
                AnonDNSCryptScanButton.IsEnabled = enable;
            AnonDNSCryptSortButton.IsEnabled = enable;
            DGS_AnonDNSCrypt.IsHitTestVisible = enable;

            NewGroupButton.IsEnabled = enable;
            ResetBuiltInButton.IsEnabled = enable;
            Import_FlyoutOverlay.IsHitTestVisible = enable;
            Export_FlyoutOverlay.IsHitTestVisible = enable;
            ImportButton.IsEnabled = enable;
            ExportButton.IsEnabled = enable;
            DGG.IsHitTestVisible = enable;
        });
    }

    private async void AnonDNSCrypt_Settings_WpfFlyoutPopup_FlyoutChanged(object sender, WpfFlyoutPopup.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            await LoadSelectedGroupAsync(true);
        }
    }

    private void Flyout_AnonDNSCrypt_Source_FlyoutChanged(object sender, WpfFlyoutGroupBox.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            Flyout_AnonDNSCrypt_Options.IsOpen = false;
        }
    }

    private void Flyout_AnonDNSCrypt_Options_FlyoutChanged(object sender, WpfFlyoutGroupBox.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            Flyout_AnonDNSCrypt_Source.IsOpen = false;
        }
    }

    private void AnonDNSCrypt_RelayByUrlByFileToggleSwitch(bool? value)
    {
        if (value.HasValue)
        {
            if (value.Value)
            {
                // Relay By File
                AnonDNSCryptRelayBrowseButton.IsEnabled = true;
                AnonDNSCryptRelayTextBox.IsEnabled = false;
            }
            else
            {
                // Relay By URL
                AnonDNSCryptRelayBrowseButton.IsEnabled = false;
                AnonDNSCryptRelayTextBox.IsEnabled = true;
            }
        }
    }

    private void AnonDNSCryptRelayByUrlByFileToggleSwitch_CheckedChanged(object sender, WpfToggleSwitch.CheckedChangedEventArgs e)
    {
        AnonDNSCrypt_RelayByUrlByFileToggleSwitch(e.IsChecked);
    }

    private void AnonDNSCrypt_TargetByUrlByFileToggleSwitch(bool? value)
    {
        if (value.HasValue)
        {
            if (value.Value)
            {
                // Target By File
                AnonDNSCryptTargetBrowseButton.IsEnabled = true;
                AnonDNSCryptTargetTextBox.IsEnabled = false;
            }
            else
            {
                // Target By URL
                AnonDNSCryptTargetBrowseButton.IsEnabled = false;
                AnonDNSCryptTargetTextBox.IsEnabled = true;
            }
        }
    }

    private void AnonDNSCryptTargetByUrlByFileToggleSwitch_CheckedChanged(object sender, WpfToggleSwitch.CheckedChangedEventArgs e)
    {
        AnonDNSCrypt_TargetByUrlByFileToggleSwitch(e.IsChecked);
    }

    private async void DGS_AnonDNSCrypt_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (sender is not DataGrid dg) return;
            if (e.MouseDevice.DirectlyOver is DependencyObject dependencyObject)
            {
                DataGridRow? row = dependencyObject.GetParentOfType<DataGridRow>();
                if (row != null && row.Item is DnsItem)
                {
                    bool isMultiSelect = dg.SelectedItems.Count > 1;
                    if (isMultiSelect)
                    {
                        MenuItem_AnonDNSCrypt_CopyDnsAddress.IsEnabled = false;
                    }
                    else
                    {
                        MenuItem_AnonDNSCrypt_CopyDnsAddress.IsEnabled = true;
                        dg.SelectedIndex = row.GetIndex();
                    }

                    ObservableCollection<DnsItem> selectedItems = new();
                    for (int n = 0; n < dg.SelectedItems.Count; n++)
                    {
                        if (dg.SelectedItems[n] is DnsItem selectedItem)
                            selectedItems.Add(selectedItem);
                    }
                    ContextMenu_AnonDNSCrypt.Tag = selectedItems;

                    MenuItem_All_CopyToCustom_Handler(MenuItem_AnonDNSCrypt_CopyToCustom, selectedItems);

                    await Task.Delay(RightClickMenuDelayMS); // Let Row Get Selected When Clicking On A Non-Selected Row
                    ContextMenu_AnonDNSCrypt.IsOpen = true;
                }
            }
        }
        catch (Exception) { }
    }

    private void DGS_AnonDNSCrypt_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not DataGrid dg) return;
        dg.FillCustomColumn(1, 200);
    }

    private void DGS_AnonDNSCrypt_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not DataGrid dg) return;
        DGS_DnsServers_PreviewKeyDown(dg, e);
    }

    private void DGS_AnonDNSCrypt_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not DataGrid dg) return;
        DGS_DnsServers_SelectionChanged(dg);
    }

    private void MenuItem_AnonDNSCrypt_CopyDnsAddress_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ContextMenu_AnonDNSCrypt.Tag is ObservableCollection<DnsItem> items && items.Count > 0)
            {
                DnsItem item = items[0];
                Clipboard.SetText(item.DNS_URL, TextDataFormat.Text);
                WpfToastDialog.Show(this, "Copied To Clipboard.", MessageBoxImage.None, 2);
            }
        }
        catch (Exception) { }
    }

    private void MenuItem_AnonDNSCrypt_Scan_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGG.SelectedItem is not DnsGroup group) return;
            if (ContextMenu_AnonDNSCrypt.Tag is ObservableCollection<DnsItem> items && items.Count > 0)
            {
                MainWindow.DnsManager.ScanServers(group, items);
            }
        }
        catch (Exception) { }
    }

    private async Task AnonDNSCrypt_Settings_Save_Async()
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Save While Scanning.", MessageBoxImage.Stop, 2);
                await AnonDNSCrypt_Settings_WpfFlyoutPopup.CloseFlyAsync();
                return;
            }

            if (DGG.SelectedItem is not DnsGroup group) return;

            // Get Group Settings
            bool enabled = AnonDNSCrypt_EnableGroup_ToggleSwitch.IsChecked.HasValue && AnonDNSCrypt_EnableGroup_ToggleSwitch.IsChecked.Value;
            string lookupDomain = AnonDNSCrypt_Settings_LookupDomain_TextBox.Text.Trim();
            double timeoutSec = AnonDNSCrypt_Settings_TimeoutSec_NumericUpDown.Value;
            int parallelSize = AnonDNSCrypt_Settings_ParallelSize_NumericUpDown.Value.ToInt();
            string bootstrapIpStr = AnonDNSCrypt_Settings_BootstrapIP_TextBox.Text.Trim();
            int bootstrapPort = AnonDNSCrypt_Settings_BootstrapPort_NumericUpDown.Value.ToInt();
            int maxServersToConnect = AnonDNSCrypt_Settings_MaxServersToConnect_NumericUpDown.Value.ToInt();
            bool allowInsecure = AnonDNSCrypt_Settings_AllowInsecure_ToggleSwitch.IsChecked.HasValue && AnonDNSCrypt_Settings_AllowInsecure_ToggleSwitch.IsChecked.Value;

            if (lookupDomain.StartsWith("http://")) lookupDomain = lookupDomain.TrimStart("http://");
            if (lookupDomain.StartsWith("https://")) lookupDomain = lookupDomain.TrimStart("https://");

            if (string.IsNullOrWhiteSpace(lookupDomain) || !lookupDomain.Contains('.') || lookupDomain.Contains("//"))
            {
                string msg = $"Domain Is Invalid.{NL}{LS}e.g. example.com";
                WpfMessageBox.Show(this, msg, "Invalid Domain", MessageBoxButton.OK, MessageBoxImage.Stop);
                await AnonDNSCrypt_Settings_WpfFlyoutPopup.OpenFlyAsync();
                return;
            }

            bool isBoostrapIP = NetworkTool.IsIP(bootstrapIpStr, out _);
            if (!isBoostrapIP)
            {
                string msg = $"Bootstrap IP Is Invalid.{NL}{LS}e.g.  8.8.8.8  Or  2001:4860:4860::8888";
                WpfMessageBox.Show(this, msg, "Invalid IP", MessageBoxButton.OK, MessageBoxImage.Stop);
                await AnonDNSCrypt_Settings_WpfFlyoutPopup.OpenFlyAsync();
                return;
            }

            // Update Group Settings
            GroupSettings gs = new(enabled, lookupDomain, timeoutSec, parallelSize, bootstrapIpStr, bootstrapPort, maxServersToConnect, allowInsecure);
            bool isSuccess = await MainWindow.DnsManager.Update_GroupSettings_Async(group.Name, gs, true);

            await LoadSelectedGroupAsync(); // Refresh
            await AnonDNSCrypt_Settings_WpfFlyoutPopup.CloseFlyAsync();

            if (isSuccess)
                WpfToastDialog.Show(this, "Saved.", MessageBoxImage.None, 2);
            else
                WpfToastDialog.Show(this, "ERROR: Couldn't Save Settings.", MessageBoxImage.Error, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_AnonymizedDNSCrypt AnonDNSCrypt_Settings_Save_Async: " + ex.Message);
        }
    }

    private async void AnonDNSCrypt_Settings_Save_Button_Click(object sender, RoutedEventArgs e)
    {
        ChangeControlsState_AnonDNSCrypt(false);
        await AnonDNSCrypt_Settings_Save_Async();
        ChangeControlsState_AnonDNSCrypt(true);
    }

    private void AnonDNSCryptRelayBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        SourceBrowseButton(AnonDNSCryptRelayTextBox);
    }

    private void AnonDNSCryptTargetBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        SourceBrowseButton(AnonDNSCryptTargetTextBox);
    }

    private async Task AnonDNSCryptSaveSourceAsync(bool showToast)
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
            List<string> relayURLs = AnonDNSCryptRelayTextBox.Text.SplitToLines();
            List<string> targetURLs = AnonDNSCryptTargetTextBox.Text.SplitToLines();
            bool isSuccess = await MainWindow.DnsManager.Update_Source_URLs_Async(group.Name, relayURLs, targetURLs, true);
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
            Debug.WriteLine("ManageDnsWindow_AnonymizedDNSCrypt AnonDNSCryptSaveSourceAsync: " + ex.Message);
        }
    }

    private async void AnonDNSCryptSaveSourceButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeControlsState_AnonDNSCrypt(false);
        await AnonDNSCryptSaveSourceAsync(true);
        ChangeControlsState_AnonDNSCrypt(true);
    }

    private async void AnonDNSCryptFetchSourceButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsScanning)
        {
            WpfToastDialog.Show(this, "Can't Fetch While Scanning.", MessageBoxImage.Stop, 2);
            return;
        }

        if (DGG.SelectedItem is not DnsGroup group) return;
        if (sender is not WpfButton b) return;
        ChangeControlsState_AnonDNSCrypt(false);
        b.DispatchIt(() => b.Content = "Fetching...");

        try
        {
            WpfToastDialog.Show(this, "Do Not Close This Window.", MessageBoxImage.None, 2);

            // Save First
            await AnonDNSCryptSaveSourceAsync(false);

            List<string> relayUrlsOrFiles = AnonDNSCryptRelayTextBox.Text.SplitToLines(StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            List<string> targetUrlsOrFiles = AnonDNSCryptTargetTextBox.Text.SplitToLines(StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (relayUrlsOrFiles.Count == 0 || targetUrlsOrFiles.Count == 0)
            {
                string msg = "Add URL Or File Path.";
                WpfMessageBox.Show(this, msg);
                end();
                return;
            }

            // Get Malicious Domains
            DnsSettings settings = MainWindow.DnsManager.Get_Settings();
            List<string> maliciousDomains = settings.MaliciousDomains.Get_Malicious_Domains;

            // Get Relays
            List<string> allRelays = new();
            for (int n = 0; n < relayUrlsOrFiles.Count; n++)
            {
                string relayUrlOrFile = relayUrlsOrFiles[n];
                List<string> relays = await DnsTools.GetServersFromLinkAsync(relayUrlOrFile, 30000, CancellationToken.None);
                allRelays.AddRange(Tools.Find_Relays_ForAnonDNSCrypt(relays, maliciousDomains));
            }

            // DeDup Relays
            allRelays = allRelays.Distinct().ToList();
            if (allRelays.Count == 0)
            {
                maliciousDomains.Clear();
                string msg = "Couldn't Find Any Relay.";
                WpfMessageBox.Show(this, msg);
                end();
                return;
            }

            // Get Targets
            List<string> allTargets = new();
            for (int n = 0; n < targetUrlsOrFiles.Count; n++)
            {
                string targetUrlOrFile = targetUrlsOrFiles[n];
                List<string> targets = await DnsTools.GetServersFromLinkAsync(targetUrlOrFile, 20000, CancellationToken.None);
                allTargets.AddRange(Tools.Find_Targets_ForAnonDNSCrypt(targets, maliciousDomains));
            }

            // DeDup Targets
            allTargets = allTargets.Distinct().ToList();

            if (allTargets.Count == 0)
            {
                allRelays.Clear();
                maliciousDomains.Clear();
                string msg = "Couldn't Find Any Target.";
                WpfMessageBox.Show(this, msg);
                end();
                return;
            }

            // Add To Group => RelayItems
            await MainWindow.DnsManager.Add_AnonDNSCrypt_RelayItems_Async(group.Name, allRelays, false);
            // Add To Group => TargetItems
            await MainWindow.DnsManager.Add_AnonDNSCrypt_TargetItems_Async(group.Name, allTargets, false);
            // Add To Group => DnsItems
            ObservableCollection<DnsItem> anonDNSCrypts = Tools.Create_DnsItem_ForAnonDNSCrypt(allTargets, allRelays);
            bool isSuccess = await MainWindow.DnsManager.Add_DnsItems_Async(group.Name, anonDNSCrypts, false);

            // Get Group Options
            AnonymizedDNSCryptOptions options = MainWindow.DnsManager.Get_AnonDNSCrypt_Options(group.Name);
            options.AutoUpdate.LastUpdateSource = DateTime.Now; // Update LastUpdateSource
            await MainWindow.DnsManager.Update_AnonDNSCrypt_Options_Async(group.Name, options, true);
            await LoadSelectedGroupAsync(); // Refresh

            if (isSuccess)
                WpfToastDialog.Show(this, $"Fetched {allRelays.Count} Relays And {allTargets.Count} Targets.", MessageBoxImage.None, 2);
            else
                WpfToastDialog.Show(this, "ERROR: Couldn't Add Fetched Servers.", MessageBoxImage.Error, 2);

            // Dispose
            allRelays.Clear();
            allTargets.Clear();
            anonDNSCrypts.Clear();
            maliciousDomains.Clear();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_AnonymizedDNSCrypt AnonDNSCryptFetchSourceButton_Click: " + ex.Message);
        }

        end();
        void end()
        {
            b.DispatchIt(() => b.Content = "Fetch Servers");
            ChangeControlsState_AnonDNSCrypt(true);
        }
    }

    private async void AnonDNSCryptSaveOptionsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Save While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not DnsGroup group) return;
            ChangeControlsState_AnonDNSCrypt(false);

            // Update Options
            // AutoUpdate
            int updateSource = AnonDNSCryptUpdateSourceNumericUpDown.Value.ToInt();
            int scanServers = AnonDNSCryptScanServersNumericUpDown.Value.ToInt();

            // FilterByProperties
            DnsFilter google = Tools.BoolToDnsFilter(AnonDNSCryptFilter_Google_CheckBox.IsChecked);
            DnsFilter bing = Tools.BoolToDnsFilter(AnonDNSCryptFilter_Bing_CheckBox.IsChecked);
            DnsFilter youtube = Tools.BoolToDnsFilter(AnonDNSCryptFilter_Youtube_CheckBox.IsChecked);
            DnsFilter adult = Tools.BoolToDnsFilter(AnonDNSCryptFilter_Adult_CheckBox.IsChecked);
            FilterByProperties filterByProperties = new(google, bing, youtube, adult);

            // Update Options
            AnonymizedDNSCryptOptions options = MainWindow.DnsManager.Get_AnonDNSCrypt_Options(group.Name);
            options.AutoUpdate.UpdateSource = updateSource;
            options.AutoUpdate.ScanServers = scanServers;
            options.FilterByProperties = filterByProperties;

            // Save/Update
            bool isSuccess = await MainWindow.DnsManager.Update_AnonDNSCrypt_Options_Async(group.Name, options, false);

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
            Debug.WriteLine("ManageDnsWindow_AnonymizedDNSCrypt AnonDNSCryptSaveOptionsButton_Click: " + ex.Message);
        }

        ChangeControlsState_AnonDNSCrypt(true);
    }

    private async void AnonDNSCryptExportAsTextButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Export While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not DnsGroup group) return;
            ChangeControlsState_AnonDNSCrypt(false);

            List<string> exportList = new();
            ObservableCollection<DnsItem> items = MainWindow.DnsManager.Get_DnsItems(group.Name);
            for (int n = 0; n < items.Count; n++)
            {
                DnsItem item = items[n];
                if (item.IsSelected) exportList.Add(item.DNS_URL);
            }

            if (exportList.Count == 0)
            {
                items.Clear();
                WpfToastDialog.Show(this, "There Is Nothing To Export.", MessageBoxImage.Stop, 2);
                ChangeControlsState_AnonDNSCrypt(true);
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
            Debug.WriteLine("ManageDnsWindow_AnonymizedDNSCrypt AnonDNSCryptExportAsTextButton_Click: " + ex.Message);
        }

        ChangeControlsState_AnonDNSCrypt(true);
    }

    private void AnonDNSCryptScanButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGG.SelectedItem is not DnsGroup group) return;
            AnonDNSCryptScanButton.IsEnabled = false;

            // Check For Fetch: Get DnsItems Info
            DnsItemsInfo info = MainWindow.DnsManager.Get_DnsItems_Info(group.Name);
            if (info.TotalServers == 0)
            {
                string msg = "Fetch Servers To Scan.";
                WpfMessageBox.Show(this, msg, "No Server To Scan!", MessageBoxButton.OK, MessageBoxImage.Information);
                SubscriptionScanButton.IsEnabled = true;
                return;
            }

            // Check For Fetch: Get Relays And Targets Info
            List<string> relays = MainWindow.DnsManager.Get_AnonDNSCrypt_Relays(group.Name);
            List<string> targets = MainWindow.DnsManager.Get_AnonDNSCrypt_Targets(group.Name);
            if (relays.Count == 0 || targets.Count == 0)
            {
                string msg = "Fetch Relays And Targets To Scan.";
                WpfMessageBox.Show(this, msg, "No Server To Scan!", MessageBoxButton.OK, MessageBoxImage.Information);
                AnonDNSCryptScanButton.IsEnabled = true;
                return;
            }

            if (DGS_AnonDNSCrypt.SelectedItems.Count > 1)
            {
                ObservableCollection<DnsItem> selectedItems = new();
                for (int n = 0; n < DGS_AnonDNSCrypt.SelectedItems.Count; n++)
                {
                    if (DGS_AnonDNSCrypt.SelectedItems[n] is DnsItem selectedItem)
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
            Debug.WriteLine("ManageDnsWindow_AnonymizedDNSCrypt AnonDNSCryptScanButton_Click: " + ex.Message);
        }
    }

    private async void AnonDNSCryptSortButton_Click(object sender, RoutedEventArgs e)
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
            ChangeControlsState_AnonDNSCrypt(false);
            b.DispatchIt(() => b.Content = "Sorting...");

            // Sort
            await MainWindow.DnsManager.Sort_DnsItems_Async(group.Name, true);
            await DGS_AnonDNSCrypt.ScrollIntoViewAsync(0); // Scroll

            b.DispatchIt(() => b.Content = "Sort");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_AnonymizedDNSCrypt AnonDNSCryptSortButton_Click: " + ex.Message);
        }

        ChangeControlsState_AnonDNSCrypt(true);
    }

}