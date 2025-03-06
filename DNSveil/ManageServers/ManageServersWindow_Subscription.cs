using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Windows.Input;
using MsmhToolsClass;
using MsmhToolsWpfClass;
using static DNSveil.DnsServers.EnumsAndStructs;
using GroupItem = DNSveil.DnsServers.EnumsAndStructs.GroupItem;
using System.Net;

namespace DNSveil.ManageServers;

public partial class ManageServersWindow : WpfWindow
{
    private void ChangeControlsState_Subscription(bool enable)
    {
        this.DispatchIt(() =>
        {
            Subscription_Settings_WpfFlyoutOverlay.IsHitTestVisible = enable;
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
            Import_FlyoutOverlay.IsHitTestVisible = enable;
            Export_FlyoutOverlay.IsHitTestVisible = enable;
            ImportButton.IsEnabled = enable;
            ExportButton.IsEnabled = enable;
            DGG.IsHitTestVisible = enable;
        });
    }

    private async void Flyout_Subscription_Source_FlyoutChanged(object sender, WpfFlyoutGroupBox.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            await Flyout_Subscription_Options.CloseFlyAsync();
        }
    }

    private async void Flyout_Subscription_Options_FlyoutChanged(object sender, WpfFlyoutGroupBox.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            await Flyout_Subscription_Source.CloseFlyAsync();
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

    private void DGS_Subscription_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
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

                    List<DnsItem> selectedDnsItems = new();
                    for (int n = 0; n < dg.SelectedItems.Count; n++)
                    {
                        if (dg.SelectedItems[n] is DnsItem selectedDnsItem)
                            selectedDnsItems.Add(selectedDnsItem);
                    }
                    ContextMenu_Subscription.Tag = selectedDnsItems;

                    MenuItem_All_CopyToCustom_Handler(MenuItem_Subscription_CopyToCustom, selectedDnsItems);

                    ContextMenu_Subscription.IsOpen = true;
                }
            }
        }
        catch (Exception) { }
    }

    private void DGS_Subscription_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not DataGrid dg) return;
        SetDataGridDnsServersSize(dg);
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
            if (ContextMenu_Subscription.Tag is List<DnsItem> dnsItemList && dnsItemList.Count > 0)
            {
                DnsItem dnsItem = dnsItemList[0];
                Clipboard.SetText(dnsItem.DNS_URL, TextDataFormat.Text);
                WpfToastDialog.Show(this, "Copied To Clipboard.", MessageBoxImage.None, 2);
            }
        }
        catch (Exception) { }
    }

    private void MenuItem_Subscription_Scan_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGG.SelectedItem is not GroupItem groupItem) return;
            if (ContextMenu_Subscription.Tag is List<DnsItem> dnsItemList && dnsItemList.Count > 0)
            {
                MainWindow.ServersManager.ScanServers(groupItem, dnsItemList);
            }
        }
        catch (Exception) { }
    }

    private async void BindDataSource_Subscription()
    {
        try
        {
            await CreateDnsItemColumns_Async(DGS_Subscription); // Create Columns
            DGS_Subscription.ItemsSource = MainWindow.ServersManager.BindDataSource_Subscription; // Bind
            if (DGS_Subscription.Items.Count > 0) DGS_Subscription.SelectedIndex = 0; // Select
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow BindDataSource_Subscription: " + ex.Message);
        }
    }

    private async Task Subscription_Settings_Save_Async()
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Save While Scanning.", MessageBoxImage.Stop, 2);
                await Subscription_Settings_WpfFlyoutOverlay.CloseFlyAsync();
                return;
            }

            if (DGG.SelectedItem is not GroupItem groupItem) return;

            // Get Group Settings
            string lookupDomain = Subscription_Settings_LookupDomain_TextBox.Text.Trim();
            double timeoutSec = Subscription_Settings_TimeoutSec_NumericUpDown.Value;
            int parallelSize = Subscription_Settings_ParallelSize_NumericUpDown.Value.ToInt();
            string bootstrapIpStr = Subscription_Settings_BootstrapIP_TextBox.Text.Trim();
            int bootstrapPort = Subscription_Settings_BootstrapPort_NumericUpDown.Value.ToInt();
            int maxServersToConnect = Subscription_Settings_MaxServersToConnect_NumericUpDown.Value.ToInt();
            bool allowInsecure = Subscription_Settings_AllowInsecure_CheckBox.IsChecked.HasValue && Subscription_Settings_AllowInsecure_CheckBox.IsChecked.Value;

            if (lookupDomain.StartsWith("http://")) lookupDomain = lookupDomain.TrimStart("http://");
            if (lookupDomain.StartsWith("https://")) lookupDomain = lookupDomain.TrimStart("https://");

            if (string.IsNullOrWhiteSpace(lookupDomain) || !lookupDomain.Contains('.') || lookupDomain.Contains("//"))
            {
                string msg = $"Domain Is Invalid.{NL}{LS}e.g. example.com";
                WpfMessageBox.Show(this, msg, "Invalid Domain", MessageBoxButton.OK, MessageBoxImage.Stop);
                await Subscription_Settings_WpfFlyoutOverlay.OpenFlyAsync();
                return;
            }

            bool isBoostrapIP = IPAddress.TryParse(bootstrapIpStr, out IPAddress? bootstrapIP);
            if (!isBoostrapIP || bootstrapIP == null)
            {
                string msg = $"Bootstrap IP Is Invalid.{NL}{LS}e.g.  8.8.8.8  Or  2001:4860:4860::8888";
                WpfMessageBox.Show(this, msg, "Invalid IP", MessageBoxButton.OK, MessageBoxImage.Stop);
                await Subscription_Settings_WpfFlyoutOverlay.OpenFlyAsync();
                return;
            }

            // Update Group Settings
            GroupSettings groupSettings = new(lookupDomain, timeoutSec, parallelSize, bootstrapIP, bootstrapPort, maxServersToConnect, allowInsecure);
            await MainWindow.ServersManager.Update_GroupSettings_Async(groupItem.Name, groupSettings, true);
            await LoadSelectedGroupAsync(false); // Refresh
            await Subscription_Settings_WpfFlyoutOverlay.CloseFlyAsync();
            WpfToastDialog.Show(this, "Saved", MessageBoxImage.None, 3, WpfToastDialog.Location.BottomCenter);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow Subscription_Settings_Save_Async: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;

            // Update URLs
            List<string> urlsOrFiles = SubscriptionSourceTextBox.Text.ReplaceLineEndings().Split(NL).ToList();
            await MainWindow.ServersManager.Update_Source_URLs_Async(groupItem.Name, urlsOrFiles, true);
            await LoadSelectedGroupAsync(false); // Refresh
            if (showToast) WpfToastDialog.Show(this, "Saved", MessageBoxImage.None, 3, WpfToastDialog.Location.BottomCenter);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow SubscriptionSaveSourceAsync: " + ex.Message);
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

        if (DGG.SelectedItem is not GroupItem groupItem) return;
        if (sender is not WpfButton b) return;
        ChangeControlsState_Subscription(false);
        b.DispatchIt(() => b.Content = "Fetching...");

        try
        {
            WpfToastDialog.Show(this, "Do Not Close This Window.", MessageBoxImage.None, 4);

            // Save First
            await SubscriptionSaveSourceAsync(false);

            List<string> urlsOrFiles = SubscriptionSourceTextBox.Text.ReplaceLineEndings().Split(NL, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

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
                List<string> dnss = await LibIn.GetServersFromLinkAsync(urlOrFile, 20000);
                allDNSs.AddRange(dnss);
            }

            // DeDup
            allDNSs = allDNSs.Distinct().ToList();

            if (allDNSs.Count == 0)
            {
                string msg = "Couldn't Find Any Server!";
                WpfMessageBox.Show(this, msg);
                end();
                return;
            }

            // Add To Group => DnsItems Element
            await MainWindow.ServersManager.Add_DnsItems_Async(groupItem.Name, allDNSs, false);
            // Update Last AutoUpdate
            await MainWindow.ServersManager.Update_LastAutoUpdate_Async(groupItem.Name, new LastAutoUpdate(DateTime.Now, DateTime.MinValue), true);
            await LoadSelectedGroupAsync(true); // Refresh

            WpfToastDialog.Show(this, $"Fetched {allDNSs.Count} Servers.", MessageBoxImage.None, 5);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow SubscriptionFetchButton_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;
            ChangeControlsState_Subscription(false);

            // Update Options
            // AutoUpdate
            int updateSource = SubscriptionUpdateSourceNumericUpDown.Value.ToInt();
            int scanServers = SubscriptionScanServersNumericUpDown.Value.ToInt();
            AutoUpdate autoUpdate = new(updateSource, scanServers);

            // FilterByProtocols
            bool udp = SubscriptionFilter_UDP_CheckBox.IsChecked ?? false;
            bool tcp = SubscriptionFilter_TCP_CheckBox.IsChecked ?? false;
            bool dnsCrypt = SubscriptionFilter_DNSCrypt_CheckBox.IsChecked ?? false;
            bool anonymizedDNSCrypt = SubscriptionFilter_AnonymizedDNSCrypt_CheckBox.IsChecked ?? false;
            bool doT = SubscriptionFilter_DoT_CheckBox.IsChecked ?? false;
            bool doH = SubscriptionFilter_DoH_CheckBox.IsChecked ?? false;
            bool oDoH = SubscriptionFilter_ODoH_CheckBox.IsChecked ?? false;
            bool doQ = SubscriptionFilter_DoQ_CheckBox.IsChecked ?? false;
            FilterByProtocols filterByProtocols = new(udp, tcp, dnsCrypt, doT, doH, doQ, anonymizedDNSCrypt, oDoH);

            // FilterByProperties
            DnsFilter google = BoolToDnsFilter(SubscriptionFilter_Google_CheckBox.IsChecked);
            DnsFilter bing = BoolToDnsFilter(SubscriptionFilter_Bing_CheckBox.IsChecked);
            DnsFilter youtube = BoolToDnsFilter(SubscriptionFilter_Youtube_CheckBox.IsChecked);
            DnsFilter adult = BoolToDnsFilter(SubscriptionFilter_Adult_CheckBox.IsChecked);
            FilterByProperties filterByProperties = new(google, bing, youtube, adult);

            // Options
            SubscriptionOptions options = new(autoUpdate, filterByProtocols, filterByProperties);

            // Save/Update
            await MainWindow.ServersManager.Update_Subscription_Options_Async(groupItem.Name, options, false);

            // Update DnsItem Selection By Options
            await MainWindow.ServersManager.Select_DnsItems_ByOptions_Async(groupItem.Name, options.FilterByProtocols, options.FilterByProperties, true);

            await LoadSelectedGroupAsync(true); // Refresh

            WpfToastDialog.Show(this, "Saved", MessageBoxImage.None, 3, WpfToastDialog.Location.BottomCenter);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow SubscriptionSaveOptionsButton_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;
            ChangeControlsState_Subscription(false);

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
            Debug.WriteLine("ManageServersWindow SubscriptionExportAsTextButton_Click: " + ex.Message);
        }

        ChangeControlsState_Subscription(true);
    }

    private void SubscriptionScanButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGG.SelectedItem is not GroupItem groupItem) return;
            SubscriptionScanButton.IsEnabled = false;

            List<DnsItem> selectedDnsItems = new();
            if (DGS_Subscription.SelectedItems.Count > 1)
            {
                for (int n = 0; n < DGS_Subscription.SelectedItems.Count; n++)
                {
                    if (DGS_Subscription.SelectedItems[n] is DnsItem selectedDnsItem)
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
            Debug.WriteLine("ManageServersWindow SubscriptionScanButton_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;
            if (sender is not WpfButton b) return;
            ChangeControlsState_Subscription(false);
            b.DispatchIt(() => b.Content = "Sorting...");

            // Sort By Latency
            await MainWindow.ServersManager.Sort_DnsItems_ByLatency_Async(groupItem.Name, true);
            await LoadSelectedGroupAsync(true); // Refresh
            await DGS_Subscription.ScrollIntoViewAsync(0); // Scroll

            b.DispatchIt(() => b.Content = "Sort");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow SubscriptionSortButton_Click: " + ex.Message);
        }

        ChangeControlsState_Subscription(true);
    }

}