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
using static DNSveil.DnsServers.EnumsAndStructs;
using GroupItem = DNSveil.DnsServers.EnumsAndStructs.GroupItem;
using System.Net;

namespace DNSveil.ManageServers;

public partial class ManageServersWindow : WpfWindow
{
    private void ChangeControlsState_AnonDNSCrypt(bool enable)
    {
        this.DispatchIt(() =>
        {
            AnonDNSCrypt_Settings_WpfFlyoutOverlay.IsHitTestVisible = enable;
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
            Import_FlyoutOverlay.IsHitTestVisible = enable;
            Export_FlyoutOverlay.IsHitTestVisible = enable;
            ImportButton.IsEnabled = enable;
            ExportButton.IsEnabled = enable;
            DGG.IsHitTestVisible = enable;
        });
    }

    private async void Flyout_AnonDNSCrypt_Source_FlyoutChanged(object sender, WpfFlyoutGroupBox.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            await Flyout_AnonDNSCrypt_Options.CloseFlyAsync();
        }
    }

    private async void Flyout_AnonDNSCrypt_Options_FlyoutChanged(object sender, WpfFlyoutGroupBox.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            await Flyout_AnonDNSCrypt_Source.CloseFlyAsync();
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

    private void DGS_AnonDNSCrypt_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (sender is not DataGrid dg) return;
            if (e.MouseDevice.DirectlyOver is DependencyObject dependencyObject)
            {
                DataGridRow? row = dependencyObject.GetParentOfType<DataGridRow>();
                if (row != null && row.Item is DnsItem dnsItem)
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

                    List<DnsItem> selectedDnsItems = new();
                    for (int n = 0; n < dg.SelectedItems.Count; n++)
                    {
                        if (dg.SelectedItems[n] is DnsItem selectedDnsItem)
                            selectedDnsItems.Add(selectedDnsItem);
                    }
                    ContextMenu_AnonDNSCrypt.Tag = selectedDnsItems;

                    MenuItem_All_CopyToCustom_Handler(MenuItem_AnonDNSCrypt_CopyToCustom, selectedDnsItems);

                    ContextMenu_AnonDNSCrypt.IsOpen = true;
                }
            }
        }
        catch (Exception) { }
    }

    private void DGS_AnonDNSCrypt_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not DataGrid dg) return;
        SetDataGridDnsServersSize(dg);
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
            if (ContextMenu_AnonDNSCrypt.Tag is List<DnsItem> dnsItemList && dnsItemList.Count > 0)
            {
                DnsItem dnsItem = dnsItemList[0];
                Clipboard.SetText(dnsItem.DNS_URL, TextDataFormat.Text);
                WpfToastDialog.Show(this, "Copied To Clipboard.", MessageBoxImage.None, 2);
            }
        }
        catch (Exception) { }
    }

    private void MenuItem_AnonDNSCrypt_Scan_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGG.SelectedItem is not GroupItem groupItem) return;
            if (ContextMenu_AnonDNSCrypt.Tag is List<DnsItem> dnsItemList && dnsItemList.Count > 0)
            {
                MainWindow.ServersManager.ScanServers(groupItem, dnsItemList);
            }
        }
        catch (Exception) { }
    }

    private async void BindDataSource_AnonDNSCrypt()
    {
        try
        {
            await CreateDnsItemColumns_Async(DGS_AnonDNSCrypt); // Create Columns
            DGS_AnonDNSCrypt.ItemsSource = MainWindow.ServersManager.BindDataSource_AnonDNSCrypt; // Bind
            if (DGS_AnonDNSCrypt.Items.Count > 0) DGS_AnonDNSCrypt.SelectedIndex = 0; // Select
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow BindDataSource_AnonDNSCrypt: " + ex.Message);
        }
    }

    private async Task AnonDNSCrypt_Settings_Save_Async()
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Save While Scanning.", MessageBoxImage.Stop, 2);
                await AnonDNSCrypt_Settings_WpfFlyoutOverlay.CloseFlyAsync();
                return;
            }

            if (DGG.SelectedItem is not GroupItem groupItem) return;

            // Get Group Settings
            string lookupDomain = AnonDNSCrypt_Settings_LookupDomain_TextBox.Text.Trim();
            double timeoutSec = AnonDNSCrypt_Settings_TimeoutSec_NumericUpDown.Value;
            int parallelSize = AnonDNSCrypt_Settings_ParallelSize_NumericUpDown.Value.ToInt();
            string bootstrapIpStr = AnonDNSCrypt_Settings_BootstrapIP_TextBox.Text.Trim();
            int bootstrapPort = AnonDNSCrypt_Settings_BootstrapPort_NumericUpDown.Value.ToInt();
            int maxServersToConnect = AnonDNSCrypt_Settings_MaxServersToConnect_NumericUpDown.Value.ToInt();
            bool allowInsecure = AnonDNSCrypt_Settings_AllowInsecure_CheckBox.IsChecked.HasValue && AnonDNSCrypt_Settings_AllowInsecure_CheckBox.IsChecked.Value;

            if (lookupDomain.StartsWith("http://")) lookupDomain = lookupDomain.TrimStart("http://");
            if (lookupDomain.StartsWith("https://")) lookupDomain = lookupDomain.TrimStart("https://");

            if (string.IsNullOrWhiteSpace(lookupDomain) || !lookupDomain.Contains('.') || lookupDomain.Contains("//"))
            {
                string msg = $"Domain Is Invalid.{NL}{LS}e.g. example.com";
                WpfMessageBox.Show(this, msg, "Invalid Domain", MessageBoxButton.OK, MessageBoxImage.Stop);
                await AnonDNSCrypt_Settings_WpfFlyoutOverlay.OpenFlyAsync();
                return;
            }

            bool isBoostrapIP = IPAddress.TryParse(bootstrapIpStr, out IPAddress? bootstrapIP);
            if (!isBoostrapIP || bootstrapIP == null)
            {
                string msg = $"Bootstrap IP Is Invalid.{NL}{LS}e.g.  8.8.8.8  Or  2001:4860:4860::8888";
                WpfMessageBox.Show(this, msg, "Invalid IP", MessageBoxButton.OK, MessageBoxImage.Stop);
                await AnonDNSCrypt_Settings_WpfFlyoutOverlay.OpenFlyAsync();
                return;
            }

            // Update Group Settings
            GroupSettings groupSettings = new(lookupDomain, timeoutSec, parallelSize, bootstrapIP, bootstrapPort, maxServersToConnect, allowInsecure);
            await MainWindow.ServersManager.Update_GroupSettings_Async(groupItem.Name, groupSettings, true);
            await LoadSelectedGroupAsync(false); // Refresh
            await AnonDNSCrypt_Settings_WpfFlyoutOverlay.CloseFlyAsync();
            WpfToastDialog.Show(this, "Saved", MessageBoxImage.None, 3, WpfToastDialog.Location.BottomCenter);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow AnonDNSCrypt_Settings_Save_Async: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;

            // Update URLs
            List<string> relayURLs = AnonDNSCryptRelayTextBox.Text.ReplaceLineEndings().Split(NL).ToList();
            List<string> targetURLs = AnonDNSCryptTargetTextBox.Text.ReplaceLineEndings().Split(NL).ToList();
            await MainWindow.ServersManager.Update_Source_URLs_Async(groupItem.Name, relayURLs, targetURLs, true);
            await LoadSelectedGroupAsync(false); // Refresh
            if (showToast) WpfToastDialog.Show(this, "Saved", MessageBoxImage.None, 3, WpfToastDialog.Location.BottomCenter);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow AnonDNSCryptSaveSourceAsync: " + ex.Message);
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

        if (DGG.SelectedItem is not GroupItem groupItem) return;
        if (sender is not WpfButton b) return;
        ChangeControlsState_AnonDNSCrypt(false);
        b.DispatchIt(() => b.Content = "Fetching...");

        try
        {
            WpfToastDialog.Show(this, "Do Not Close This Window.", MessageBoxImage.None, 4);

            // Save First
            await AnonDNSCryptSaveSourceAsync(false);

            List<string> relayUrlsOrFiles = AnonDNSCryptRelayTextBox.Text.ReplaceLineEndings().Split(NL, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            List<string> targetUrlsOrFiles = AnonDNSCryptTargetTextBox.Text.ReplaceLineEndings().Split(NL, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            if (relayUrlsOrFiles.Count == 0 || targetUrlsOrFiles.Count == 0)
            {
                string msg = "Add URL Or File Path.";
                WpfMessageBox.Show(this, msg);
                end();
                return;
            }

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

            // DeDup Relays
            allRelays = allRelays.Distinct().ToList();

            if (allRelays.Count == 0)
            {
                string msg = "Couldn't Find Any Relay.";
                WpfMessageBox.Show(this, msg);
                end();
                return;
            }

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

            // DeDup Targets
            allTargets = allTargets.Distinct().ToList();

            if (allTargets.Count == 0)
            {
                string msg = "Couldn't Find Any Target.";
                WpfMessageBox.Show(this, msg);
                end();
                return;
            }

            // Add To Group => RelayItems Element
            await MainWindow.ServersManager.Add_AnonDNSCrypt_RelayItems_Async(groupItem.Name, allRelays, false);
            // Add To Group => TargetItems Element
            await MainWindow.ServersManager.Add_AnonDNSCrypt_TargetItems_Async(groupItem.Name, allTargets, false);
            // Clear Previous Result
            await MainWindow.ServersManager.Clear_DnsItems_Async(groupItem.Name, false);
            // Update Last AutoUpdate
            await MainWindow.ServersManager.Update_LastAutoUpdate_Async(groupItem.Name, new LastAutoUpdate(DateTime.Now, DateTime.MinValue), true);
            await LoadSelectedGroupAsync(true); // Refresh

            WpfToastDialog.Show(this, $"Fetched {allRelays.Count} Relays And {allTargets.Count} Targets.", MessageBoxImage.None, 5);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow AnonDNSCryptFetchSourceButton_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;
            ChangeControlsState_AnonDNSCrypt(false);

            // Update Options
            // AutoUpdate
            int updateSource = AnonDNSCryptUpdateSourceNumericUpDown.Value.ToInt();
            int scanServers = AnonDNSCryptScanServersNumericUpDown.Value.ToInt();
            AutoUpdate autoUpdate = new(updateSource, scanServers);

            // FilterByProperties
            DnsFilter google = BoolToDnsFilter(AnonDNSCryptFilter_Google_CheckBox.IsChecked);
            DnsFilter bing = BoolToDnsFilter(AnonDNSCryptFilter_Bing_CheckBox.IsChecked);
            DnsFilter youtube = BoolToDnsFilter(AnonDNSCryptFilter_Youtube_CheckBox.IsChecked);
            DnsFilter adult = BoolToDnsFilter(AnonDNSCryptFilter_Adult_CheckBox.IsChecked);
            FilterByProperties filterByProperties = new(google, bing, youtube, adult);

            // Options
            AnonDNSCryptOptions options = new(autoUpdate, filterByProperties);

            // Save/Update
            await MainWindow.ServersManager.Update_AnonDNSCrypt_Options_Async(groupItem.Name, options, false);

            // Update DnsItem Selection By Options
            await MainWindow.ServersManager.Select_DnsItems_ByOptions_Async(groupItem.Name, options.FilterByProperties, true);

            await LoadSelectedGroupAsync(true); // Refresh

            WpfToastDialog.Show(this, "Saved", MessageBoxImage.None, 3, WpfToastDialog.Location.BottomCenter);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow AnonDNSCryptSaveOptionsButton_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;
            ChangeControlsState_AnonDNSCrypt(false);

            List<string> exportList = new();
            List<DnsItem> dnsItemList = MainWindow.ServersManager.Get_DnsItems(groupItem.Name);
            for (int n = 0; n < dnsItemList.Count; n++)
            {
                DnsItem dnsItem = dnsItemList[n];
                if (dnsItem.Enabled) exportList.Add(dnsItem.DNS_URL);
            }

            if (exportList.Count == 0)
            {
                WpfToastDialog.Show(this, "There Is Nothing To Export.", MessageBoxImage.Stop, 4);
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
                FileName = $"DNSveil_Export_{groupItem.Name}_Selected_Servers_{DateTime.Now:yyyy.MM.dd-HH.mm.ss}"
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
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow AnonDNSCryptExportAsTextButton_Click: " + ex.Message);
        }

        ChangeControlsState_AnonDNSCrypt(true);
    }

    private void AnonDNSCryptScanButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGG.SelectedItem is not GroupItem groupItem) return;
            AnonDNSCryptScanButton.IsEnabled = false;

            List<DnsItem> selectedDnsItems = new();
            if (DGS_AnonDNSCrypt.SelectedItems.Count > 1)
            {
                for (int n = 0; n < DGS_AnonDNSCrypt.SelectedItems.Count; n++)
                {
                    if (DGS_AnonDNSCrypt.SelectedItems[n] is DnsItem selectedDnsItem)
                        selectedDnsItems.Add(selectedDnsItem);
                }
                MainWindow.ServersManager.ScanServers(groupItem, selectedDnsItems);
            }
            else
            {
                MainWindow.ServersManager.ScanServers(groupItem, null);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow AnonDNSCryptScanButton_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;
            if (sender is not WpfButton b) return;
            ChangeControlsState_AnonDNSCrypt(false);
            b.DispatchIt(() => b.Content = "Sorting...");

            // Sort By Latency
            await MainWindow.ServersManager.Sort_DnsItems_ByLatency_Async(groupItem.Name, true);
            await LoadSelectedGroupAsync(true); // Refresh
            await DGS_AnonDNSCrypt.ScrollIntoViewAsync(0); // Scroll

            b.DispatchIt(() => b.Content = "Sort");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow AnonDNSCryptSortButton_Click: " + ex.Message);
        }

        ChangeControlsState_AnonDNSCrypt(true);
    }

}