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
using static DNSveil.Logic.DnsServers.EnumsAndStructs;
using static MsmhToolsClass.MsmhAgnosticServer.AgnosticProgram;
using GroupItem = DNSveil.Logic.DnsServers.EnumsAndStructs.GroupItem;

namespace DNSveil.ManageServers;

public partial class ManageServersWindow : WpfWindow
{
    private void ChangeControlsState_FragmentDoH(bool enable)
    {
        this.DispatchIt(() =>
        {
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
            if (DGG.SelectedItem is not GroupItem groupItem) return;
            await MainWindow.ServersManager.Update_Source_EnableDisable_Async(groupItem.Name, !e.IsChecked.Value, true);
        }
    }

    private void DGS_FragmentDoH_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
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

                    List<DnsItem> selectedDnsItems = new();
                    for (int n = 0; n < dg.SelectedItems.Count; n++)
                    {
                        if (dg.SelectedItems[n] is DnsItem selectedDnsItem)
                            selectedDnsItems.Add(selectedDnsItem);
                    }
                    ContextMenu_FragmentDoH.Tag = selectedDnsItems;
                    ContextMenu_FragmentDoH.IsOpen = true;
                }
            }
        }
        catch (Exception) { }
    }

    private void DGS_FragmentDoH_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not DataGrid dg) return;
        SetDataGridDnsServersSize(dg);
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
            if (ContextMenu_FragmentDoH.Tag is List<DnsItem> dnsItemList && dnsItemList.Count > 0)
            {
                DnsItem dnsItem = dnsItemList[0];
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;

            // DeDup
            int nextIndex = GetPreviousOrNextIndex(DGS_FragmentDoH); // Get Next Index
            await MainWindow.ServersManager.DeDup_DnsItems_Async(groupItem.Name, true);
            await LoadSelectedGroupAsync(true); // Refresh
            await DGS_FragmentDoH.ScrollIntoViewAsync(nextIndex); // Scroll To Next
            if (showToast) WpfToastDialog.Show(this, "Duplicates Removed", MessageBoxImage.None, 3, WpfToastDialog.Location.BottomCenter);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow FragmentDoH_RemoveDuplicates_Async: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;
            if (DGS_FragmentDoH.SelectedItem is not DnsItem currentDnsItem) return;

            List<DnsItem> selectedDnsItems = new();
            for (int n = 0; n < DGS_FragmentDoH.SelectedItems.Count; n++)
            {
                if (DGS_FragmentDoH.SelectedItems[n] is DnsItem selectedDnsItem)
                    selectedDnsItems.Add(selectedDnsItem);
            }

            // Confirm Delete
            string msg = $"Deleting \"{currentDnsItem.DNS_URL}\"...{NL}Continue?";
            if (selectedDnsItems.Count > 1) msg = $"Deleting Selected DoH Items...{NL}Continue?";
            MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbr != MessageBoxResult.Yes) return;

            // Delete
            int nextIndex = GetPreviousOrNextIndex(DGS_FragmentDoH); // Get Next Index
            await MainWindow.ServersManager.Remove_DnsItems_Async(groupItem.Name, selectedDnsItems, true);
            await LoadSelectedGroupAsync(true); // Refresh
            await DGS_FragmentDoH.ScrollIntoViewAsync(nextIndex); // Scroll To Next
            WpfToastDialog.Show(this, "Deleted", MessageBoxImage.None, 3, WpfToastDialog.Location.BottomCenter);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow MenuItem_FragmentDoH_DeleteItem_Click: " + ex.Message);
        }
    }

    private void MenuItem_FragmentDoH_Scan_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGG.SelectedItem is not GroupItem groupItem) return;
            if (ContextMenu_FragmentDoH.Tag is List<DnsItem> dnsItemList && dnsItemList.Count > 0)
            {
                MainWindow.ServersManager.ScanServers(groupItem, dnsItemList);
            }
        }
        catch (Exception) { }
    }

    private async void BindDataSource_FragmentDoH()
    {
        try
        {
            await CreateDnsItemColumns_FragmentDoH_Async(DGS_FragmentDoH); // Create Columns
            DGS_FragmentDoH.ItemsSource = MainWindow.ServersManager.BindDataSource_FragmentDoH; // Bind
            if (DGS_FragmentDoH.Items.Count > 0) DGS_FragmentDoH.SelectedIndex = 0; // Select
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow BindDataSource_FragmentDoH: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;

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

            bool isBoostrapIP = IPAddress.TryParse(bootstrapIpStr, out IPAddress? bootstrapIP);
            if (!isBoostrapIP || bootstrapIP == null)
            {
                string msg = $"Bootstrap IP Is Invalid.{NL}{LS}e.g.  8.8.8.8  Or  2001:4860:4860::8888";
                WpfMessageBox.Show(this, msg, "Invalid IP", MessageBoxButton.OK, MessageBoxImage.Stop);
                await FragmentDoH_Settings_WpfFlyoutPopup.OpenFlyAsync();
                return;
            }

            // Update Group Settings
            GroupSettings groupSettings = new(enabled, lookupDomain, timeoutSec, parallelSize, bootstrapIP, bootstrapPort, maxServersToConnect, allowInsecure);
            await MainWindow.ServersManager.Update_GroupSettings_Async(groupItem.Name, groupSettings, true);
            await LoadSelectedGroupAsync(false); // Refresh
            await FragmentDoH_Settings_WpfFlyoutPopup.CloseFlyAsync();
            WpfToastDialog.Show(this, "Saved", MessageBoxImage.None, 3, WpfToastDialog.Location.BottomCenter);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow FragmentDoH_Settings_Save_Async: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;
            DnsItem currentDnsItem = new();
            if (!TrueAdd_FalseModify)
            {
                if (DGS_FragmentDoH.SelectedItem is DnsItem currentDnsItemOut) currentDnsItem = currentDnsItemOut;
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

            DnsReader dnsReader = new(doH_URL);
            if (dnsReader.Protocol != DnsEnums.DnsProtocol.DoH)
            {
                WpfMessageBox.Show(this, "DNS Must Be A DoH.", "Not A DoH", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            if (TrueAdd_FalseModify)
            {
                // Add
                List<DnsItem> dnsItems = MainWindow.ServersManager.Get_DnsItems(groupItem.Name);
                bool alreadyExist = dnsItems.Any(x => x.DNS_URL.Equals(doH_URL));
                if (alreadyExist)
                {
                    WpfMessageBox.Show(this, "The DoH Already Exist.", "Exist", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                DnsItem dnsItem = new() { DNS_URL = doH_URL, DNS_IP = doH_IP, Protocol = dnsReader.ProtocolName }; // Create New DnsItem
                await MainWindow.ServersManager.Append_DnsItems_Async(groupItem.Name, new List<DnsItem>() { dnsItem }, true);
                await LoadSelectedGroupAsync(true); // Refresh
                await DGS_FragmentDoH.ScrollIntoViewAsync(DGS_FragmentDoH.Items.Count - 1); // Scroll To Last Item
                WpfToastDialog.Show(this, "Added", MessageBoxImage.None, 3, WpfToastDialog.Location.BottomCenter);
            }
            else
            {
                // Modify
                currentDnsItem.DNS_URL = doH_URL;
                currentDnsItem.DNS_IP = doH_IP;
                currentDnsItem.Protocol = dnsReader.ProtocolName;
                await MainWindow.ServersManager.Update_DnsItems_Async(groupItem.Name, new List<DnsItem>() { currentDnsItem }, true);
                await LoadSelectedGroupAsync(true); // Refresh
                WpfToastDialog.Show(this, "Modified", MessageBoxImage.None, 3, WpfToastDialog.Location.BottomCenter);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow FragmentDoHByManualAddOrModifyAsync: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;

            // Update URLs
            List<string> urls = FragmentDoHSourceTextBox.Text.ReplaceLineEndings().Split(NL).ToList();
            await MainWindow.ServersManager.Update_Source_URLs_Async(groupItem.Name, urls, true);
            await LoadSelectedGroupAsync(false); // Refresh
            if (showToast) WpfToastDialog.Show(this, "Saved", MessageBoxImage.None, 3, WpfToastDialog.Location.BottomCenter);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow FragmentDoHSaveSourceAsync: " + ex.Message);
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

        if (DGG.SelectedItem is not GroupItem groupItem) return;
        if (sender is not WpfButton b) return;
        ChangeControlsState_FragmentDoH(false);
        b.DispatchIt(() => b.Content = "Fetching...");

        try
        {
            WpfToastDialog.Show(this, "Do Not Close This Window.", MessageBoxImage.None, 4);

            // Save First
            await FragmentDoHSaveSourceAsync(false);

            List<string> urlsOrFiles = FragmentDoHSourceTextBox.Text.ReplaceLineEndings().Split(NL, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

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
                List<string> dnss = await DnsTools.GetServersFromLinkAsync(urlOrFile, 20000);
                allDNSs.AddRange(dnss);
            }

            // Convert To DnsItem
            List<DnsItem> allDoHStamps = Tools.Convert_DNSs_To_DnsItem_ForFragmentDoH(allDNSs);

            if (allDoHStamps.Count == 0)
            {
                string msg = "Couldn't Find Any Proper DoH Stamp Server!";
                WpfMessageBox.Show(this, msg);
                end();
                return;
            }
            
            // Add To Group => DnsItems Element
            await MainWindow.ServersManager.Add_DnsItems_Async(groupItem.Name, allDoHStamps, false);
            // Update Last AutoUpdate
            await MainWindow.ServersManager.Update_LastAutoUpdate_Async(groupItem.Name, new LastAutoUpdate(DateTime.Now, DateTime.MinValue), true);
            await LoadSelectedGroupAsync(true); // Refresh

            WpfToastDialog.Show(this, $"Fetched {allDoHStamps.Count} DoH Stamps.", MessageBoxImage.None, 5);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow FragmentDoHFetchSourceButton_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;
            ChangeControlsState_FragmentDoH(false);

            // Update Options
            // AutoUpdate
            int updateSource = FragmentDoHUpdateSourceNumericUpDown.Value.ToInt();
            int scanServers = FragmentDoHScanServersNumericUpDown.Value.ToInt();
            AutoUpdate autoUpdate = new(updateSource, scanServers);

            // FilterByProperties
            DnsFilter google = BoolToDnsFilter(FragmentDoHFilter_Google_CheckBox.IsChecked);
            DnsFilter bing = BoolToDnsFilter(FragmentDoHFilter_Bing_CheckBox.IsChecked);
            DnsFilter youtube = BoolToDnsFilter(FragmentDoHFilter_Youtube_CheckBox.IsChecked);
            DnsFilter adult = BoolToDnsFilter(FragmentDoHFilter_Adult_CheckBox.IsChecked);
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

            // Options
            FragmentDoHOptions options = new(autoUpdate, filterByProperties, fragmentSettings);

            // Save/Update
            await MainWindow.ServersManager.Update_FragmentDoH_Options_Async(groupItem.Name, options, false);

            // Update DnsItem Selection By Options
            await MainWindow.ServersManager.Select_DnsItems_ByOptions_Async(groupItem.Name, options.FilterByProperties, true);

            await LoadSelectedGroupAsync(true); // Refresh

            WpfToastDialog.Show(this, "Saved", MessageBoxImage.None, 3, WpfToastDialog.Location.BottomCenter);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow FragmentDoHSaveOptionsButton_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;
            ChangeControlsState_FragmentDoH(false);

            List<string> exportList = new();
            List<DnsItem> dnsItemList = MainWindow.ServersManager.Get_DnsItems(groupItem.Name);
            for (int n = 0; n < dnsItemList.Count; n++)
            {
                DnsItem dnsItem = dnsItemList[n];
                if (dnsItem.Enabled)
                {
                    // Create SDNS
                    bool isNoFilter = dnsItem.IsGoogleSafeSearchEnabled == DnsFilter.No &&
                                      dnsItem.IsBingSafeSearchEnabled == DnsFilter.No &&
                                      dnsItem.IsYoutubeRestricted == DnsFilter.No &&
                                      dnsItem.IsAdultBlocked == DnsFilter.No;
                    NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(dnsItem.DNS_URL, 443);
                    string stamp = DNSCryptStampGenerator.GenerateDoH(dnsItem.DNS_IP.ToString(), null, $"{urid.Host}:{urid.Port}", urid.Path, null, false, false, isNoFilter);

                    exportList.Add(stamp);
                }
            }

            if (exportList.Count == 0)
            {
                WpfToastDialog.Show(this, "There Is Nothing To Export.", MessageBoxImage.Stop, 4);
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
            Debug.WriteLine("ManageServersWindow FragmentDoHExportAsTextButton_Click: " + ex.Message);
        }

        ChangeControlsState_FragmentDoH(true);
    }

    private void FragmentDoHScanButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGG.SelectedItem is not GroupItem groupItem) return;
            FragmentDoHScanButton.IsEnabled = false;

            // Check For Fetch: Get DnsItems Info
            DnsItemsInfo info = MainWindow.ServersManager.Get_DnsItems_Info(groupItem.Name);
            if (info.TotalServers == 0)
            {
                string msg = "Fetch Or Add Servers To Scan.";
                WpfMessageBox.Show(this, msg, "No Server To Scan!", MessageBoxButton.OK, MessageBoxImage.Information);
                FragmentDoHScanButton.IsEnabled = true;
                return;
            }

            List<DnsItem> selectedDnsItems = new();
            if (DGS_FragmentDoH.SelectedItems.Count > 1)
            {
                for (int n = 0; n < DGS_FragmentDoH.SelectedItems.Count; n++)
                {
                    if (DGS_FragmentDoH.SelectedItems[n] is DnsItem selectedDnsItem)
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
            Debug.WriteLine("ManageServersWindow FragmentDoHScanButton_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;
            if (sender is not WpfButton b) return;
            ChangeControlsState_FragmentDoH(false);
            b.DispatchIt(() => b.Content = "Sorting...");

            // Sort By Latency
            await MainWindow.ServersManager.Sort_DnsItems_ByLatency_Async(groupItem.Name, true);
            await LoadSelectedGroupAsync(true); // Refresh
            await DGS_FragmentDoH.ScrollIntoViewAsync(0); // Scroll

            b.DispatchIt(() => b.Content = "Sort");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow FragmentDoHSortButton_Click: " + ex.Message);
        }

        ChangeControlsState_FragmentDoH(true);
    }

}