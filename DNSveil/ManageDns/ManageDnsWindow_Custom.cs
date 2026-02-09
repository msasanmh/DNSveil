using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using System.Text;
using System.IO;
using MsmhToolsClass;
using MsmhToolsWpfClass;
using MsmhToolsClass.MsmhAgnosticServer;
using DNSveil.Logic.DnsServers;
using static DNSveil.Logic.DnsServers.DnsModel;
using System.Collections.ObjectModel;

namespace DNSveil.ManageDns;

public partial class ManageDnsWindow : WpfWindow
{
    private void ChangeControlsState_Custom(bool enable)
    {
        this.DispatchIt(() =>
        {
            if (PART_Button1 != null) PART_Button1.IsEnabled = enable;
            Custom_Settings_WpfFlyoutPopup.IsHitTestVisible = enable;
            if (enable || (!enable && IsScanning))
                CustomSourceStackPanel.IsEnabled = enable;
            CustomSourceToggleSwitchBrowseButton.IsEnabled = enable;
            CustomFetchSourceButton.IsEnabled = enable;
            CustomByManualAddButton.IsEnabled = enable;
            CustomByManualModifyButton.IsEnabled = enable;
            CustomSaveOptionsButton.IsEnabled = enable;
            CustomExportAsTextButton.IsEnabled = enable;
            if (enable || (!enable && !IsScanning))
                CustomScanButton.IsEnabled = enable;
            CustomSortButton.IsEnabled = enable;
            DGS_Custom.IsHitTestVisible = enable;

            NewGroupButton.IsEnabled = enable;
            ResetBuiltInButton.IsEnabled = enable;
            Import_FlyoutOverlay.IsHitTestVisible = enable;
            Export_FlyoutOverlay.IsHitTestVisible = enable;
            ImportButton.IsEnabled = enable;
            ExportButton.IsEnabled = enable;
            DGG.IsHitTestVisible = enable;
        });
    }

    private async void Custom_Settings_WpfFlyoutPopup_FlyoutChanged(object sender, WpfFlyoutPopup.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            await LoadSelectedGroupAsync(true);
        }
    }

    private void Flyout_Custom_Source_FlyoutChanged(object sender, WpfFlyoutGroupBox.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            Flyout_Custom_Options.IsOpen = false;
        }
    }

    private void Flyout_Custom_Options_FlyoutChanged(object sender, WpfFlyoutGroupBox.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            Flyout_Custom_Source.IsOpen = false;
        }
    }

    private async void Custom_ToggleSourceByUrlByFileByManual(bool? value)
    {
        if (value.HasValue)
        {
            if (value.Value)
            {
                // By File
                CustomSourceToggleSwitchBrowseButton.IsEnabled = true;
                CustomSourceTextBox.IsEnabled = false;
            }
            else
            {
                // By URL
                CustomSourceToggleSwitchBrowseButton.IsEnabled = false;
                CustomSourceTextBox.IsEnabled = true;
            }

            CustomSourceByUrlGroupBox.Visibility = Visibility.Visible;
            try
            {
                DoubleAnimation anim = new(1, new Duration(TimeSpan.FromMilliseconds(200)));
                CustomSourceByUrlGroupBox.BeginAnimation(OpacityProperty, anim);
            }
            catch (Exception) { }
        }
        else
        {
            // By Manual
            CustomSourceToggleSwitchBrowseButton.IsEnabled = false;

            try
            {
                DoubleAnimation anim = new(0, new Duration(TimeSpan.FromMilliseconds(200)));
                CustomSourceByUrlGroupBox.BeginAnimation(OpacityProperty, anim);
            }
            catch (Exception) { }
            await Task.Delay(200);
            CustomSourceByUrlGroupBox.Visibility = Visibility.Collapsed;
        }
    }

    private void CustomSourceToggleSwitch_CheckedChanged(object sender, WpfToggleSwitch.CheckedChangedEventArgs e)
    {
        Custom_ToggleSourceByUrlByFileByManual(e.IsChecked);
    }

    private async void DGS_Custom_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
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
                        MenuItem_Custom_CopyDnsAddress.IsEnabled = false;
                    }
                    else
                    {
                        MenuItem_Custom_CopyDnsAddress.IsEnabled = true;
                        dg.SelectedIndex = row.GetIndex();
                    }

                    ObservableCollection<DnsItem> selectedItems = new();
                    for (int n = 0; n < dg.SelectedItems.Count; n++)
                    {
                        if (dg.SelectedItems[n] is DnsItem selectedItem)
                            selectedItems.Add(selectedItem);
                    }
                    ContextMenu_Custom.Tag = selectedItems;

                    MenuItem_All_CopyToCustom_Handler(MenuItem_Custom_CopyToCustom, selectedItems);

                    await Task.Delay(RightClickMenuDelayMS); // Let Row Get Selected When Clicking On A Non-Selected Row
                    ContextMenu_Custom.IsOpen = true;
                }
            }
        }
        catch (Exception) { }
    }

    private void DGS_Custom_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        DGS_Custom.FillCustomColumn(1, 200);
    }

    private void DGS_Custom_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not DataGrid dg) return;
        DGS_DnsServers_PreviewKeyDown(dg, e);
    }

    private void DGS_Custom_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not DataGrid dg) return;
        DGS_DnsServers_SelectionChanged(dg);
    }

    private void MenuItem_Custom_CopyDnsAddress_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ContextMenu_Custom.Tag is ObservableCollection<DnsItem> items && items.Count > 0)
            {
                DnsItem dnsItem = items[0];
                Clipboard.SetText(dnsItem.DNS_URL, TextDataFormat.Text);
                WpfToastDialog.Show(this, "Copied To Clipboard.", MessageBoxImage.None, 2);
            }
        }
        catch (Exception) { }
    }

    private async Task Custom_RemoveDuplicates_Async(bool showToast)
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
            int nextIndex = GetPreviousOrNextIndex(DGS_Custom, false); // Get Next Index
            bool isSuccess = await MainWindow.DnsManager.DeDup_DnsItems_Async(group.Name, true);
            await LoadSelectedGroupAsync(); // Refresh
            await DGS_Custom.ScrollIntoViewAsync(nextIndex); // Scroll To Next

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
            Debug.WriteLine("ManageDnsWindow_Custom Custom_RemoveDuplicates_Async: " + ex.Message);
        }
    }

    private async void MenuItem_Custom_RemoveDuplicates_Click(object sender, RoutedEventArgs e)
    {
        ChangeControlsState_Custom(false);
        await Custom_RemoveDuplicates_Async(true);
        ChangeControlsState_Custom(true);
    }

    private async void MenuItem_Custom_DeleteItem_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Delete While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not DnsGroup group) return;
            if (DGS_Custom.SelectedItem is not DnsItem currentItem) return;

            List<DnsItem> selectedItems = new();
            for (int n = 0; n < DGS_Custom.SelectedItems.Count; n++)
            {
                if (DGS_Custom.SelectedItems[n] is DnsItem selectedItem)
                    selectedItems.Add(selectedItem);
            }

            // Confirm Delete
            string msg = $"Deleting \"{currentItem.DNS_URL}\"...{NL}Continue?";
            if (selectedItems.Count > 1) msg = $"Deleting Selected DNS Items...{NL}Continue?";
            MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbr != MessageBoxResult.Yes) return;

            // Delete
            int nextIndex = GetPreviousOrNextIndex(DGS_Custom, true); // Get Next Index
            bool isSuccess = await MainWindow.DnsManager.Remove_DnsItems_Async(group.Name, selectedItems, true);
            await LoadSelectedGroupAsync(); // Refresh
            await DGS_Custom.ScrollIntoViewAsync(nextIndex); // Scroll To Next

            if (isSuccess)
                WpfToastDialog.Show(this, "Deleted.", MessageBoxImage.None, 2);
            else
                WpfToastDialog.Show(this, "ERROR: Couldn't Delete Items.", MessageBoxImage.Error, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_Custom MenuItem_Custom_DeleteItem_Click: " + ex.Message);
        }
    }

    private async void MenuItem_Custom_DeleteAll_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Delete While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not DnsGroup group) return;

            // Confirm Delete
            string msg = $"Deleting All DNSs...{NL}Continue?";
            MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Delete All", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbr != MessageBoxResult.Yes) return;

            // Delete All
            bool isSuccess = await MainWindow.DnsManager.Clear_DnsItems_Async(group.Name, true);
            await LoadSelectedGroupAsync(); // Refresh

            if (isSuccess)
                WpfToastDialog.Show(this, "All Items Deleted.", MessageBoxImage.None, 2);
            else
                WpfToastDialog.Show(this, "ERROR: Couldn't Delete Items.", MessageBoxImage.Error, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_Custom MenuItem_Custom_DeleteAll_Click: " + ex.Message);
        }
    }

    private void MenuItem_Custom_Scan_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGG.SelectedItem is not DnsGroup group) return;
            if (ContextMenu_Custom.Tag is ObservableCollection<DnsItem> items && items.Count > 0)
            {
                MainWindow.DnsManager.ScanServers(group, items);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_Custom MenuItem_Custom_Scan_Click: " + NL + ex.Message);
        }
    }

    private async Task Custom_Settings_Save_Async()
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Save While Scanning.", MessageBoxImage.Stop, 2);
                await Custom_Settings_WpfFlyoutPopup.CloseFlyAsync();
                return;
            }

            if (DGG.SelectedItem is not DnsGroup group) return;

            // Get Group Settings
            bool enabled = Custom_EnableGroup_ToggleSwitch.IsChecked.HasValue && Custom_EnableGroup_ToggleSwitch.IsChecked.Value;
            string lookupDomain = Custom_Settings_LookupDomain_TextBox.Text.Trim();
            double timeoutSec = Custom_Settings_TimeoutSec_NumericUpDown.Value;
            int parallelSize = Custom_Settings_ParallelSize_NumericUpDown.Value.ToInt();
            string bootstrapIpStr = Custom_Settings_BootstrapIP_TextBox.Text.Trim();
            int bootstrapPort = Custom_Settings_BootstrapPort_NumericUpDown.Value.ToInt();
            int maxServersToConnect = Custom_Settings_MaxServersToConnect_NumericUpDown.Value.ToInt();
            bool allowInsecure = Custom_Settings_AllowInsecure_ToggleSwitch.IsChecked.HasValue && Custom_Settings_AllowInsecure_ToggleSwitch.IsChecked.Value;

            if (lookupDomain.StartsWith("http://")) lookupDomain = lookupDomain.TrimStart("http://");
            if (lookupDomain.StartsWith("https://")) lookupDomain = lookupDomain.TrimStart("https://");

            if (string.IsNullOrWhiteSpace(lookupDomain) || !lookupDomain.Contains('.') || lookupDomain.Contains("//"))
            {
                string msg = $"Domain Is Invalid.{NL}{LS}e.g. example.com";
                WpfMessageBox.Show(this, msg, "Invalid Domain", MessageBoxButton.OK, MessageBoxImage.Stop);
                await Custom_Settings_WpfFlyoutPopup.OpenFlyAsync();
                return;
            }

            bool isBoostrapIP = NetworkTool.IsIP(bootstrapIpStr, out _);
            if (!isBoostrapIP)
            {
                string msg = $"Bootstrap IP Is Invalid.{NL}{LS}e.g.  8.8.8.8  Or  2001:4860:4860::8888";
                WpfMessageBox.Show(this, msg, "Invalid IP", MessageBoxButton.OK, MessageBoxImage.Stop);
                await Custom_Settings_WpfFlyoutPopup.OpenFlyAsync();
                return;
            }

            // Update Group Settings
            GroupSettings gs = new(enabled, lookupDomain, timeoutSec, parallelSize, bootstrapIpStr, bootstrapPort, maxServersToConnect, allowInsecure);
            bool isSuccess = await MainWindow.DnsManager.Update_GroupSettings_Async(group.Name, gs, true);
            
            await LoadSelectedGroupAsync(); // Refresh
            await Custom_Settings_WpfFlyoutPopup.CloseFlyAsync();

            if (isSuccess)
                WpfToastDialog.Show(this, "Saved.", MessageBoxImage.None, 2);
            else
                WpfToastDialog.Show(this, "ERROR: Couldn't Save Settings.", MessageBoxImage.Error, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_Custom Custom_Settings_Save_Async: " + ex.Message);
        }
    }

    private async void Custom_Settings_Save_Button_Click(object sender, RoutedEventArgs e)
    {
        ChangeControlsState_Custom(false);
        await Custom_Settings_Save_Async();
        ChangeControlsState_Custom(true);
    }

    private void CustomSourceToggleSwitchBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        SourceBrowseButton(CustomSourceTextBox);
    }

    private async Task Custom_ByManualAddOrModify_Async(bool TrueAdd_FalseModify)
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
                if (DGS_Custom.SelectedItem is DnsItem currentItemOut) currentItem = currentItemOut;
                else
                {
                    WpfToastDialog.Show(this, $"Select An Item To Modify.", MessageBoxImage.Stop, 2);
                    return;
                }
            }
            string dns_Address = string.Empty, dns_description = string.Empty;

            this.DispatchIt(() =>
            {
                dns_Address = CustomByManual_DnsAddress_TextBox.Text.Trim();
                dns_description = CustomByManual_DnsDescription_TextBox.Text.Trim();
            });

            // Decode If It's A Stamp
            dns_Address = await DnsTools.DecodeStampAsync(dns_Address);

            DnsItem newItem = new(dns_Address);
            DnsReader dnsReader = newItem.DnsReader;
            if (dnsReader.Protocol == DnsEnums.DnsProtocol.Unknown)
            {
                WpfMessageBox.Show(this, "DNS Address Is Invalid.", "Unknown", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            // Set Description
            if (!string.IsNullOrEmpty(dns_description)) newItem.Description = dns_description;

            if (TrueAdd_FalseModify)
            {
                // Add
                ObservableCollection<DnsItem> newItemList = new() { newItem };
                bool isSuccess = await MainWindow.DnsManager.Append_DnsItems_Async(group.Name, newItemList, true);
                await LoadSelectedGroupAsync(); // Refresh
                await DGS_Custom.ScrollIntoViewAsync(DGS_Custom.Items.Count - 1); // Scroll To Last Item

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
            Debug.WriteLine("ManageDnsWindow_Custom Custom_ByManualAddOrModify_Async: " + ex.Message);
        }
    }

    private async void CustomByManualAddButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeControlsState_Custom(false);
        await Custom_ByManualAddOrModify_Async(true);
        ChangeControlsState_Custom(true);
    }

    private async void CustomByManualModifyButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeControlsState_Custom(false);
        await Custom_ByManualAddOrModify_Async(false);
        ChangeControlsState_Custom(true);
    }

    private async Task CustomSaveSourceAsync(bool showToast)
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
            List<string> urlsOrFiles = CustomSourceTextBox.Text.SplitToLines();
            bool isSuccess = await MainWindow.DnsManager.Update_Source_URLs_Async(group.Name, urlsOrFiles, true);
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
            Debug.WriteLine("ManageServersWindow CustomSaveSourceAsync: " + ex.Message);
        }
    }

    private async void CustomFetchSourceButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsScanning)
        {
            WpfToastDialog.Show(this, "Can't Fetch While Scanning.", MessageBoxImage.Stop, 2);
            return;
        }

        if (DGG.SelectedItem is not DnsGroup group) return;
        if (sender is not WpfButton b) return;
        ChangeControlsState_Custom(false);
        b.DispatchIt(() => b.Content = "Fetching...");

        try
        {
            WpfToastDialog.Show(this, "Do Not Close This Window.", MessageBoxImage.None, 2);

            // Save First
            await CustomSaveSourceAsync(false);

            List<string> urlsOrFiles = CustomSourceTextBox.Text.SplitToLines(StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (urlsOrFiles.Count == 0)
            {
                string msg = "Add URL Or File Path.";
                WpfMessageBox.Show(this, msg);
                end();
                return;
            }

            // Get Malicious Domains
            DnsSettings settings = MainWindow.DnsManager.Get_Settings();
            List<string> maliciousDomains = settings.MaliciousDomains.Get_Malicious_Domains;

            ObservableCollection<string> allDNSs = new();
            for (int i = 0; i < urlsOrFiles.Count; i++)
            {
                string urlOrFile = urlsOrFiles[i];
                List<string> dnss = await DnsTools.GetServersFromLinkAsync(urlOrFile, 30000, CancellationToken.None);
                dnss = await DnsTools.DecodeStampAsync(dnss);
                for (int j = 0; j < dnss.Count; j++)
                {
                    string dns = dnss[j];
                    if (MaliciousDomains.IsMalicious(maliciousDomains, dns)) continue; // Ignore Malicious Domains
                    allDNSs.Add(dns);
                }
            }

            // DeDup
            allDNSs = allDNSs.Distinct().ToObservableCollection();
            if (allDNSs.Count == 0)
            {
                maliciousDomains.Clear();
                return;
            }

            // Add To Group => DnsItems
            bool isSuccess = await MainWindow.DnsManager.Add_DnsItems_Async(group.Name, allDNSs, false);
            await LoadSelectedGroupAsync(); // Refresh

            if (isSuccess)
                WpfToastDialog.Show(this, $"Fetched {allDNSs.Count} Servers.", MessageBoxImage.None, 2);
            else
                WpfToastDialog.Show(this, "ERROR: Couldn't Add Fetched Servers.", MessageBoxImage.Error, 2);

            // Dispose
            allDNSs.Clear();
            maliciousDomains.Clear();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow CustomFetchButton_Click: " + ex.Message);
        }

        end();
        void end()
        {
            b.DispatchIt(() => b.Content = "Fetch Servers");
            ChangeControlsState_Custom(true);
        }
    }

    private async void CustomSaveOptionsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Save While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not DnsGroup group) return;
            ChangeControlsState_Custom(false);

            // Update Options
            // AutoUpdate
            int scanServers = CustomScanServersNumericUpDown.Value.ToInt();

            // FilterByProtocols
            bool udp = CustomFilter_UDP_CheckBox.IsChecked ?? false;
            bool tcp = CustomFilter_TCP_CheckBox.IsChecked ?? false;
            bool tcpOverUdp = CustomFilter_TcpOverUdp_CheckBox.IsChecked ?? false;
            bool dnsCrypt = CustomFilter_DNSCrypt_CheckBox.IsChecked ?? false;
            bool anonymizedDNSCrypt = CustomFilter_AnonymizedDNSCrypt_CheckBox.IsChecked ?? false;
            bool doT = CustomFilter_DoT_CheckBox.IsChecked ?? false;
            bool doH = CustomFilter_DoH_CheckBox.IsChecked ?? false;
            bool oDoH = CustomFilter_ODoH_CheckBox.IsChecked ?? false;
            bool doQ = CustomFilter_DoQ_CheckBox.IsChecked ?? false;
            FilterByProtocols filterByProtocols = new(udp, tcp, tcpOverUdp, dnsCrypt, doT, doH, doQ, anonymizedDNSCrypt, oDoH);

            // FilterByProperties
            DnsFilter google = Tools.BoolToDnsFilter(CustomFilter_Google_CheckBox.IsChecked);
            DnsFilter bing = Tools.BoolToDnsFilter(CustomFilter_Bing_CheckBox.IsChecked);
            DnsFilter youtube = Tools.BoolToDnsFilter(CustomFilter_Youtube_CheckBox.IsChecked);
            DnsFilter adult = Tools.BoolToDnsFilter(CustomFilter_Adult_CheckBox.IsChecked);
            FilterByProperties filterByProperties = new(google, bing, youtube, adult);

            // Update Options
            CustomOptions options = options = MainWindow.DnsManager.Get_Custom_Options(group.Name);
            options.AutoUpdate.ScanServers = scanServers;
            options.FilterByProtocols = filterByProtocols;
            options.FilterByProperties = filterByProperties;

            // Save/Update
            bool isSuccess = await MainWindow.DnsManager.Update_Custom_Options_Async(group.Name, options, false);

            // Update DnsItem Selection By Options
            await MainWindow.DnsManager.Select_DnsItems_ByOptions_Async(group.Name, options.FilterByProtocols, options.FilterByProperties, true);

            await LoadSelectedGroupAsync(); // Refresh

            if (isSuccess)
                WpfToastDialog.Show(this, "Saved.", MessageBoxImage.None, 2);
            else
                WpfToastDialog.Show(this, "ERROR: Couldn't Save Options.", MessageBoxImage.Error, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow CustomSaveOptionsButton_Click: " + ex.Message);
        }

        ChangeControlsState_Custom(true);
    }

    private async void CustomExportAsTextButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Export While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not DnsGroup group) return;
            ChangeControlsState_Custom(false);

            List<string> exportList = new();
            ObservableCollection<DnsItem> items = MainWindow.DnsManager.Get_DnsItems(group.Name);
            for (int n = 0; n < items.Count; n++)
            {
                DnsItem item = items[n];
                if (item.IsSelected) exportList.Add(item.DNS_URL);
            }

            if (exportList.Count == 0)
            {
                WpfToastDialog.Show(this, "There Is Nothing To Export.", MessageBoxImage.Stop, 2);
                ChangeControlsState_Custom(true);
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
            Debug.WriteLine("ManageServersWindow CustomExportAsTextButton_Click: " + ex.Message);
        }

        ChangeControlsState_Custom(true);
    }

    private void CustomScanButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGG.SelectedItem is not DnsGroup group) return;
            CustomScanButton.IsEnabled = false;

            // Check For Fetch: Get DnsItems Info
            DnsItemsInfo info = MainWindow.DnsManager.Get_DnsItems_Info(group.Name);
            if (info.TotalServers == 0)
            {
                string msg = "Fetch Or Add Servers To Scan.";
                WpfMessageBox.Show(this, msg, "No Server To Scan!", MessageBoxButton.OK, MessageBoxImage.Information);
                CustomScanButton.IsEnabled = true;
                return;
            }

            if (DGS_Custom.SelectedItems.Count > 1)
            {
                ObservableCollection<DnsItem> selectedItems = new();
                for (int n = 0; n < DGS_Custom.SelectedItems.Count; n++)
                {
                    if (DGS_Custom.SelectedItems[n] is DnsItem selectedItem)
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
            Debug.WriteLine("ManageServersWindow CustomScanButton_Click: " + ex.Message);
        }
    }

    private async void CustomSortButton_Click(object sender, RoutedEventArgs e)
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
            ChangeControlsState_Custom(false);
            b.DispatchIt(() => b.Content = "Sorting...");

            // Sort
            await MainWindow.DnsManager.Sort_DnsItems_Async(group.Name, true);
            await DGS_Custom.ScrollIntoViewAsync(0); // Scroll

            b.DispatchIt(() => b.Content = "Sort");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow CustomSortButton_Click: " + ex.Message);
        }

        ChangeControlsState_Custom(true);
    }

}