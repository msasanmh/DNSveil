using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Windows.Input;
using MsmhToolsClass;
using MsmhToolsWpfClass;
using MsmhToolsClass.MsmhAgnosticServer;
using DNSveil.Logic.DnsServers;
using static DNSveil.Logic.DnsServers.DnsModel;
using System.Collections.ObjectModel;

namespace DNSveil.ManageDns;

public partial class ManageDnsWindow : WpfWindow
{
    private void ChangeControlsState_Subscription(bool enable)
    {
        this.DispatchIt(() =>
        {
            if (PART_Button1 != null) PART_Button1.IsEnabled = enable;
            Subscription_Settings_WpfFlyoutPopup.IsHitTestVisible = enable;
            if (enable || (!enable && IsScanning))
                SubscriptionSourceStackPanel.IsEnabled = enable;
            SubscriptionSourceToggleSwitchBrowseButton.IsEnabled = enable;
            SubscriptionSaveSourceButton.IsEnabled = enable;
            SubscriptionFetchSourceButton.IsEnabled = enable;
            SubscriptionSaveOptionsButton.IsEnabled = enable;
            SubscriptionExportAsTextButton.IsEnabled = enable;
            if (enable || (!enable && !IsScanning))
                SubscriptionScanButton.IsEnabled = enable;
            SubscriptionSortButton.IsEnabled = enable;
            DGS_Subscription.IsHitTestVisible = enable;

            NewGroupButton.IsEnabled = enable;
            ResetBuiltInButton.IsEnabled = enable;
            Import_FlyoutOverlay.IsHitTestVisible = enable;
            Export_FlyoutOverlay.IsHitTestVisible = enable;
            ImportButton.IsEnabled = enable;
            ExportButton.IsEnabled = enable;
            DGG.IsHitTestVisible = enable;
        });
    }

    private async void Subscription_Settings_WpfFlyoutPopup_FlyoutChanged(object sender, WpfFlyoutPopup.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            await LoadSelectedGroupAsync(true);
        }
    }

    private void Flyout_Subscription_Source_FlyoutChanged(object sender, WpfFlyoutGroupBox.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            Flyout_Subscription_Options.IsOpen = false;
        }
    }

    private void Flyout_Subscription_Options_FlyoutChanged(object sender, WpfFlyoutGroupBox.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            Flyout_Subscription_Source.IsOpen = false;
        }
    }

    private void Subscription_ToggleSourceByUrlByFile(bool? value)
    {
        if (value.HasValue)
        {
            if (value.Value)
            {
                // By File
                SubscriptionSourceToggleSwitchBrowseButton.IsEnabled = true;
                SubscriptionSourceTextBox.IsEnabled = false;
            }
            else
            {
                // By URL
                SubscriptionSourceToggleSwitchBrowseButton.IsEnabled = false;
                SubscriptionSourceTextBox.IsEnabled = true;
            }
        }
    }

    private void SubscriptionSourceToggleSwitch_CheckedChanged(object sender, WpfToggleSwitch.CheckedChangedEventArgs e)
    {
        Subscription_ToggleSourceByUrlByFile(e.IsChecked);
    }

    private async void DGS_Subscription_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
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
                        MenuItem_Subscription_CopyDnsAddress.IsEnabled = false;
                    }
                    else
                    {
                        MenuItem_Subscription_CopyDnsAddress.IsEnabled = true;
                        dg.SelectedIndex = row.GetIndex();
                    }

                    ObservableCollection<DnsItem> selectedItems = new();
                    for (int n = 0; n < dg.SelectedItems.Count; n++)
                    {
                        if (dg.SelectedItems[n] is DnsItem selectedItem)
                            selectedItems.Add(selectedItem);
                    }
                    ContextMenu_Subscription.Tag = selectedItems;

                    MenuItem_All_CopyToCustom_Handler(MenuItem_Subscription_CopyToCustom, selectedItems);

                    await Task.Delay(RightClickMenuDelayMS); // Let Row Get Selected When Clicking On A Non-Selected Row
                    ContextMenu_Subscription.IsOpen = true;
                }
            }
        }
        catch (Exception) { }
    }

    private void DGS_Subscription_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not DataGrid dg) return;
        dg.FillCustomColumn(1, 200);
    }

    private void DGS_Subscription_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not DataGrid dg) return;
        DGS_DnsServers_PreviewKeyDown(dg, e);
    }

    private void DGS_Subscription_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not DataGrid dg) return;
        DGS_DnsServers_SelectionChanged(dg);
    }

    private void MenuItem_Subscription_CopyDnsAddress_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ContextMenu_Subscription.Tag is ObservableCollection<DnsItem> items && items.Count > 0)
            {
                DnsItem item = items[0];
                Clipboard.SetText(item.DNS_URL, TextDataFormat.Text);
                WpfToastDialog.Show(this, "Copied To Clipboard.", MessageBoxImage.None, 2);
            }
        }
        catch (Exception) { }
    }

    private void MenuItem_Subscription_Scan_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGG.SelectedItem is not DnsGroup group) return;
            if (ContextMenu_Subscription.Tag is ObservableCollection<DnsItem> items && items.Count > 0)
            {
                MainWindow.DnsManager.ScanServers(group, items);
            }
        }
        catch (Exception) { }
    }

    private async Task Subscription_Settings_Save_Async()
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Save While Scanning.", MessageBoxImage.Stop, 2);
                await Subscription_Settings_WpfFlyoutPopup.CloseFlyAsync();
                return;
            }

            if (DGG.SelectedItem is not DnsGroup group) return;

            // Get Group Settings
            bool enabled = Subscription_EnableGroup_ToggleSwitch.IsChecked.HasValue && Subscription_EnableGroup_ToggleSwitch.IsChecked.Value;
            string lookupDomain = Subscription_Settings_LookupDomain_TextBox.Text.Trim();
            double timeoutSec = Subscription_Settings_TimeoutSec_NumericUpDown.Value;
            int parallelSize = Subscription_Settings_ParallelSize_NumericUpDown.Value.ToInt();
            string bootstrapIpStr = Subscription_Settings_BootstrapIP_TextBox.Text.Trim();
            int bootstrapPort = Subscription_Settings_BootstrapPort_NumericUpDown.Value.ToInt();
            int maxServersToConnect = Subscription_Settings_MaxServersToConnect_NumericUpDown.Value.ToInt();
            bool allowInsecure = Subscription_Settings_AllowInsecure_ToggleSwitch.IsChecked.HasValue && Subscription_Settings_AllowInsecure_ToggleSwitch.IsChecked.Value;
            
            if (lookupDomain.StartsWith("http://")) lookupDomain = lookupDomain.TrimStart("http://");
            if (lookupDomain.StartsWith("https://")) lookupDomain = lookupDomain.TrimStart("https://");

            if (string.IsNullOrWhiteSpace(lookupDomain) || !lookupDomain.Contains('.') || lookupDomain.Contains("//"))
            {
                string msg = $"Domain Is Invalid.{NL}{LS}e.g. example.com";
                WpfMessageBox.Show(this, msg, "Invalid Domain", MessageBoxButton.OK, MessageBoxImage.Stop);
                await Subscription_Settings_WpfFlyoutPopup.OpenFlyAsync();
                return;
            }

            bool isBoostrapIP = NetworkTool.IsIP(bootstrapIpStr, out _);
            if (!isBoostrapIP)
            {
                string msg = $"Bootstrap IP Is Invalid.{NL}{LS}e.g.  8.8.8.8  Or  2001:4860:4860::8888";
                WpfMessageBox.Show(this, msg, "Invalid IP", MessageBoxButton.OK, MessageBoxImage.Stop);
                await Subscription_Settings_WpfFlyoutPopup.OpenFlyAsync();
                return;
            }

            // Update Group Settings
            GroupSettings gs = new(enabled, lookupDomain, timeoutSec, parallelSize, bootstrapIpStr, bootstrapPort, maxServersToConnect, allowInsecure);
            bool isSuccess = await MainWindow.DnsManager.Update_GroupSettings_Async(group.Name, gs, true);

            await LoadSelectedGroupAsync(); // Refresh
            await Subscription_Settings_WpfFlyoutPopup.CloseFlyAsync();

            if (isSuccess)
                WpfToastDialog.Show(this, "Saved.", MessageBoxImage.None, 2);
            else
                WpfToastDialog.Show(this, "ERROR: Couldn't Save Settings.", MessageBoxImage.Error, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_Subscription Subscription_Settings_Save_Async: " + ex.Message);
        }
    }

    private async void Subscription_Settings_Save_Button_Click(object sender, RoutedEventArgs e)
    {
        ChangeControlsState_Subscription(false);
        await Subscription_Settings_Save_Async();
        ChangeControlsState_Subscription(true);
    }

    private void SubscriptionSourceToggleSwitchBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        SourceBrowseButton(SubscriptionSourceTextBox);
    }

    private async Task SubscriptionSaveSourceAsync(bool showToast)
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
            List<string> urlsOrFiles = SubscriptionSourceTextBox.Text.SplitToLines();
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
            Debug.WriteLine("ManageDnsWindow_Subscription SubscriptionSaveSourceAsync: " + ex.Message);
        }
    }

    private async void SubscriptionSaveSourceButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeControlsState_Subscription(false);
        await SubscriptionSaveSourceAsync(true);
        ChangeControlsState_Subscription(true);
    }

    private async void SubscriptionFetchSourceButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsScanning)
        {
            WpfToastDialog.Show(this, "Can't Fetch While Scanning.", MessageBoxImage.Stop, 2);
            return;
        }

        if (DGG.SelectedItem is not DnsGroup group) return;
        if (sender is not WpfButton b) return;
        ChangeControlsState_Subscription(false);
        b.DispatchIt(() => b.Content = "Fetching...");

        try
        {
            WpfToastDialog.Show(this, "Do Not Close This Window.", MessageBoxImage.None, 2);

            // Save First
            await SubscriptionSaveSourceAsync(false);

            List<string> urlsOrFiles = SubscriptionSourceTextBox.Text.SplitToLines(StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

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
                string msg = "Couldn't Find Any Server!";
                WpfMessageBox.Show(this, msg);
                end();
                return;
            }

            // Add To Group => DnsItems
            bool isSuccess = await MainWindow.DnsManager.Add_DnsItems_Async(group.Name, allDNSs, false);

            // Get Group Options
            SubscriptionOptions options = MainWindow.DnsManager.Get_Subscription_Options(group.Name);
            options.AutoUpdate.LastUpdateSource = DateTime.Now; // Update LastUpdateSource
            await MainWindow.DnsManager.Update_Subscription_Options_Async(group.Name, options, true);
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
            Debug.WriteLine("ManageDnsWindow_Subscription SubscriptionFetchButton_Click: " + ex.Message);
        }

        end();
        void end()
        {
            b.DispatchIt(() => b.Content = "Fetch Servers");
            ChangeControlsState_Subscription(true);
        }
    }

    private async void SubscriptionSaveOptionsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Save While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not DnsGroup group) return;
            ChangeControlsState_Subscription(false);

            // Get Options
            // AutoUpdate
            int updateSource = SubscriptionUpdateSourceNumericUpDown.Value.ToInt();
            int scanServers = SubscriptionScanServersNumericUpDown.Value.ToInt();

            // FilterByProtocols
            bool udp = SubscriptionFilter_UDP_CheckBox.IsChecked ?? false;
            bool tcp = SubscriptionFilter_TCP_CheckBox.IsChecked ?? false;
            bool tcpOverUdp = SubscriptionFilter_TcpOverUdp_CheckBox.IsChecked ?? false;
            bool dnsCrypt = SubscriptionFilter_DNSCrypt_CheckBox.IsChecked ?? false;
            bool anonymizedDNSCrypt = SubscriptionFilter_AnonymizedDNSCrypt_CheckBox.IsChecked ?? false;
            bool doT = SubscriptionFilter_DoT_CheckBox.IsChecked ?? false;
            bool doH = SubscriptionFilter_DoH_CheckBox.IsChecked ?? false;
            bool oDoH = SubscriptionFilter_ODoH_CheckBox.IsChecked ?? false;
            bool doQ = SubscriptionFilter_DoQ_CheckBox.IsChecked ?? false;
            FilterByProtocols filterByProtocols = new(udp, tcp, tcpOverUdp, dnsCrypt, doT, doH, doQ, anonymizedDNSCrypt, oDoH);

            // FilterByProperties
            DnsFilter google = Tools.BoolToDnsFilter(SubscriptionFilter_Google_CheckBox.IsChecked);
            DnsFilter bing = Tools.BoolToDnsFilter(SubscriptionFilter_Bing_CheckBox.IsChecked);
            DnsFilter youtube = Tools.BoolToDnsFilter(SubscriptionFilter_Youtube_CheckBox.IsChecked);
            DnsFilter adult = Tools.BoolToDnsFilter(SubscriptionFilter_Adult_CheckBox.IsChecked);
            FilterByProperties filterByProperties = new(google, bing, youtube, adult);

            // Update Options
            SubscriptionOptions options = MainWindow.DnsManager.Get_Subscription_Options(group.Name);
            options.AutoUpdate.UpdateSource = updateSource;
            options.AutoUpdate.ScanServers = scanServers;
            options.FilterByProtocols = filterByProtocols;
            options.FilterByProperties = filterByProperties;

            // Save/Update
            bool isSuccess = await MainWindow.DnsManager.Update_Subscription_Options_Async(group.Name, options, false);

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
            Debug.WriteLine("ManageDnsWindow_Subscription SubscriptionSaveOptionsButton_Click: " + ex.Message);
        }

        ChangeControlsState_Subscription(true);
    }

    private async void SubscriptionExportAsTextButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Export While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not DnsGroup group) return;
            ChangeControlsState_Subscription(false);

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
                ChangeControlsState_Subscription(true);
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
            Debug.WriteLine("ManageDnsWindow_Subscription SubscriptionExportAsTextButton_Click: " + ex.Message);
        }

        ChangeControlsState_Subscription(true);
    }

    private void SubscriptionScanButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGG.SelectedItem is not DnsGroup group) return;
            SubscriptionScanButton.IsEnabled = false;

            // Check For Fetch: Get DnsItems Info
            DnsItemsInfo info = MainWindow.DnsManager.Get_DnsItems_Info(group.Name);
            if (info.TotalServers == 0)
            {
                string msg = "Fetch Servers To Scan.";
                WpfMessageBox.Show(this, msg, "No Server To Scan!", MessageBoxButton.OK, MessageBoxImage.Information);
                SubscriptionScanButton.IsEnabled = true;
                return;
            }

            if (DGS_Subscription.SelectedItems.Count > 1)
            {
                ObservableCollection<DnsItem> selectedItems = new();
                for (int n = 0; n < DGS_Subscription.SelectedItems.Count; n++)
                {
                    if (DGS_Subscription.SelectedItems[n] is DnsItem selectedItem)
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
            Debug.WriteLine("ManageDnsWindow_Subscription SubscriptionScanButton_Click: " + ex.Message);
        }
    }

    private async void SubscriptionSortButton_Click(object sender, RoutedEventArgs e)
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
            ChangeControlsState_Subscription(false);
            b.DispatchIt(() => b.Content = "Sorting...");

            // Sort
            await MainWindow.DnsManager.Sort_DnsItems_Async(group.Name, true);
            await DGS_Subscription.ScrollIntoViewAsync(0); // Scroll

            b.DispatchIt(() => b.Content = "Sort");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow_Subscription SubscriptionSortButton_Click: " + ex.Message);
        }

        ChangeControlsState_Subscription(true);
    }

}