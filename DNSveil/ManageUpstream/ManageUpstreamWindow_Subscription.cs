using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using MsmhToolsClass;
using MsmhToolsWpfClass;
using static DNSveil.Logic.UpstreamServers.UpstreamModel;

namespace DNSveil.ManageUpstream;

public partial class ManageUpstreamWindow : WpfWindow
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

    private void Subscription_Settings_AddFragment_ToggleSwitch_CheckedChanged(object sender, WpfToggleSwitch.CheckedChangedEventArgs e)
    {
        if (e.IsChecked.HasValue)
        {
            Subscription_Settings_FragmentSize_TextBox.IsEnabled = e.IsChecked.Value;
            Subscription_Settings_FragmentDelay_TextBox.IsEnabled = e.IsChecked.Value;
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

    private void Subscription_ToggleAutoScanSelect(bool? value)
    {
        if (value.HasValue)
        {
            if (value.Value)
            {
                // AutoScanSelect ON
                SubscriptionScanServersTextBlock.IsEnabled = false;
                SubscriptionScanServersNumericUpDown.IsEnabled = false;
                SubscriptionLastScanServersTextBlock.IsEnabled = false;
            }
            else
            {
                // AutoScanSelect OFF
                SubscriptionScanServersTextBlock.IsEnabled = true;
                SubscriptionScanServersNumericUpDown.IsEnabled = true;
                SubscriptionLastScanServersTextBlock.IsEnabled = true;
            }
        }
    }

    private void SubscriptionAutoScanSelectToggleSwitch_CheckedChanged(object sender, WpfToggleSwitch.CheckedChangedEventArgs e)
    {
        Subscription_ToggleAutoScanSelect(e.IsChecked);
    }

    private async void DGS_Subscription_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (sender is not DataGrid dg) return;
            if (e.MouseDevice.DirectlyOver is DependencyObject dependencyObject)
            {
                DataGridRow? row = dependencyObject.GetParentOfType<DataGridRow>();
                if (row != null && row.Item is UpstreamItem)
                {
                    bool isMultiSelect = dg.SelectedItems.Count > 1;
                    if (isMultiSelect)
                    {
                        MenuItem_Subscription_CopyImportedConfig.IsEnabled = false;
                    }
                    else
                    {
                        MenuItem_Subscription_CopyImportedConfig.IsEnabled = true;
                        dg.SelectedIndex = row.GetIndex();
                    }
                    
                    ObservableCollection<UpstreamItem> selectedItems = new();
                    for (int n = 0; n < dg.SelectedItems.Count; n++)
                    {
                        if (dg.SelectedItems[n] is UpstreamItem selectedItem)
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
        DGS_UpstreamServers_PreviewKeyDown(dg, e);
    }

    private void DGS_Subscription_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not DataGrid dg) return;
        DGS_UpstreamServers_SelectionChanged(dg);
    }

    private void MenuItem_Subscription_CopyImportedConfig_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ContextMenu_Subscription.Tag is ObservableCollection<UpstreamItem> items && items.Count > 0)
            {
                UpstreamItem item = items[0];
                Clipboard.SetText(item.ConfigInfo.UrlOrJson, TextDataFormat.Text);
                WpfToastDialog.Show(this, "Copied To Clipboard.", MessageBoxImage.None, 2);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow_Subscription MenuItem_Subscription_CopyImportedConfig_Click: " + NL + ex.Message);
        }
    }

    private void MenuItem_Subscription_Scan_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGG.SelectedItem is not UpstreamGroup group) return;
            if (ContextMenu_Subscription.Tag is ObservableCollection<UpstreamItem> items && items.Count > 0)
            {
                MainWindow.UpstreamManager.ScanServers(group, items);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow_Subscription MenuItem_Subscription_Scan_Click: " + NL + ex.Message);
        }
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

            if (DGG.SelectedItem is not UpstreamGroup group) return;

            // Get Group Settings
            bool enabled = Subscription_EnableGroup_ToggleSwitch.IsChecked.HasValue && Subscription_EnableGroup_ToggleSwitch.IsChecked.Value;
            string testURL = Subscription_Settings_LookupDomain_TextBox.Text.Trim();
            double timeoutSec = Subscription_Settings_TimeoutSec_NumericUpDown.Value;
            int parallelSize = Subscription_Settings_ParallelSize_NumericUpDown.Value.ToInt();
            string bootstrapIpStr = Subscription_Settings_BootstrapIP_TextBox.Text.Trim();
            int bootstrapPort = Subscription_Settings_BootstrapPort_NumericUpDown.Value.ToInt();
            bool addFragment = Subscription_Settings_AddFragment_ToggleSwitch.IsChecked.HasValue && Subscription_Settings_AddFragment_ToggleSwitch.IsChecked.Value;
            string fragmentSize = Subscription_Settings_FragmentSize_TextBox.Text.Trim();
            string fragmentDelay = Subscription_Settings_FragmentDelay_TextBox.Text.Trim();
            bool getRegionInfo = Subscription_Settings_GetRegionInfo_ToggleSwitch.IsChecked.HasValue && Subscription_Settings_GetRegionInfo_ToggleSwitch.IsChecked.Value;
            bool getSpeedTest = Subscription_Settings_SpeedTest_ToggleSwitch.IsChecked.HasValue && Subscription_Settings_SpeedTest_ToggleSwitch.IsChecked.Value;
            bool dontuseWithoutSecurity = Subscription_Settings_DontUseWithoutSecurity_ToggleSwitch.IsChecked.HasValue && Subscription_Settings_DontUseWithoutSecurity_ToggleSwitch.IsChecked.Value;
            bool allowInsecure = Subscription_Settings_AllowInsecure_ToggleSwitch.IsChecked.HasValue && Subscription_Settings_AllowInsecure_ToggleSwitch.IsChecked.Value;

            bool isTestUrlInvalid = string.IsNullOrWhiteSpace(testURL) || !testURL.Contains('.') || !testURL.Contains("://");
            if (!isTestUrlInvalid)
            {
                NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(testURL, 443);
                isTestUrlInvalid = urid.Uri == null;
            }

            if (isTestUrlInvalid)
            {
                string msg = $"Test URL Is Invalid.{NL}{LS}e.g. https://captive.apple.com/hotspot-detect.html";
                WpfMessageBox.Show(this, msg, "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Stop);
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
            XrayFragment xrayFragment = new() { IsEnabled = addFragment, Size = fragmentSize, Delay = fragmentDelay };
            GroupSettings gs = new(enabled, testURL, timeoutSec, parallelSize, bootstrapIpStr, bootstrapPort, xrayFragment, getRegionInfo, getSpeedTest, dontuseWithoutSecurity, allowInsecure);
            bool isSuccess = await MainWindow.UpstreamManager.Update_GroupSettings_Async(group.Name, gs, false);

            // Select UpstreamItems
            await MainWindow.UpstreamManager.Select_UpstreamItems_Async(group.Name, true);

            await LoadSelectedGroupAsync(); // Refresh
            await Subscription_Settings_WpfFlyoutPopup.CloseFlyAsync();

            if (isSuccess)
                WpfToastDialog.Show(this, "Saved.", MessageBoxImage.None, 2);
            else
                WpfToastDialog.Show(this, "ERROR: Couldn't Save Settings.", MessageBoxImage.Error, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow_Subscription Subscription_Settings_Save_Async: " + ex.Message);
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

            if (DGG.SelectedItem is not UpstreamGroup group) return;

            // Update URLs
            List<string> urlsOrFiles = SubscriptionSourceTextBox.Text.SplitToLines();
            bool isSuccess = await MainWindow.UpstreamManager.Update_Source_URLs_Async(group.Name, urlsOrFiles, true);
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
            Debug.WriteLine("ManageUpstreamWindow_Subscription SubscriptionSaveSourceAsync: " + ex.Message);
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

        if (DGG.SelectedItem is not UpstreamGroup group) return;
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

            List<string> allItems = new();
            for (int n = 0; n < urlsOrFiles.Count; n++)
            {
                string urlOrFile = urlsOrFiles[n];
                List<string> upstreams = await WebAPI.GetLinesFromTextLinkAsync(urlOrFile, 30000, CancellationToken.None);
                allItems.AddRange(upstreams);
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
                string msg = "Couldn't Find Any Server!";
                WpfMessageBox.Show(this, msg);
                end();
                return;
            }

            // Add To Group
            bool isSuccess = await MainWindow.UpstreamManager.Add_UpstreamItems_Async(group.Name, items, false);

            // Get Group Options
            SubscriptionOptions options = MainWindow.UpstreamManager.Get_Subscription_Options(group.Name);
            options.AutoUpdate.LastUpdateSource = DateTime.Now; // Update LastUpdateSource
            await MainWindow.UpstreamManager.Update_Subscription_Options_Async(group.Name, options, true);
            await LoadSelectedGroupAsync(); // Refresh

            if (isSuccess)
                WpfToastDialog.Show(this, $"Fetched {items.Count} Servers.", MessageBoxImage.None, 2);
            else
                WpfToastDialog.Show(this, "ERROR: Couldn't Add Fetched Servers.", MessageBoxImage.Error, 2);

            // Dispose
            items.Clear();
            allItems.Clear();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow_Subscription SubscriptionFetchSourceButton_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not UpstreamGroup group) return;
            ChangeControlsState_Subscription(false);

            // Get Options
            int updateSource = SubscriptionUpdateSourceNumericUpDown.Value.ToInt();
            bool autoSelect = SubscriptionAutoScanSelectToggleSwitch.IsChecked.HasValue && SubscriptionAutoScanSelectToggleSwitch.IsChecked.Value;
            int scanServers = SubscriptionScanServersNumericUpDown.Value.ToInt();

            // Update Options
            SubscriptionOptions options = MainWindow.UpstreamManager.Get_Subscription_Options(group.Name);
            options.AutoUpdate.UpdateSource = updateSource;
            options.AutoUpdate.AutoScanSelect = autoSelect;
            options.AutoUpdate.ScanServers = scanServers;

            // Save/Update
            bool isSuccess = await MainWindow.UpstreamManager.Update_Subscription_Options_Async(group.Name, options, true);
            await LoadSelectedGroupAsync(); // Refresh

            if (isSuccess)
                WpfToastDialog.Show(this, "Saved.", MessageBoxImage.None, 2);
            else
                WpfToastDialog.Show(this, "ERROR: Couldn't Save Options.", MessageBoxImage.Error, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow_Subscription SubscriptionSaveOptionsButton_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not UpstreamGroup group) return;
            ChangeControlsState_Subscription(false);

            List<string> exportList = new();
            ObservableCollection<UpstreamItem> items = MainWindow.UpstreamManager.Get_UpstreamItems(group.Name);
            for (int n = 0; n < items.Count; n++)
            {
                UpstreamItem item = items[n];
                if (item.IsSelected) exportList.Add(item.ConfigInfo.UrlOrJson);
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
                Filter = "TXT Upstream Servers (*.txt)|*.txt",
                DefaultExt = ".txt",
                AddExtension = true,
                RestoreDirectory = true,
                FileName = $"DNSveil_Export_{group.Name}_Selected_Upstream_Servers_{DateTime.Now:yyyy.MM.dd-HH.mm.ss}"
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
            Debug.WriteLine("ManageUpstreamWindow_Subscription SubscriptionExportAsTextButton_Click: " + ex.Message);
        }

        ChangeControlsState_Subscription(true);
    }

    private void SubscriptionScanButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGG.SelectedItem is not UpstreamGroup group) return;
            SubscriptionScanButton.IsEnabled = false;

            // Check For Fetch: Get UpstreamItems Info
            UpstreamItemsInfo info = MainWindow.UpstreamManager.Get_UpstreamItems_Info(group.Name);
            if (info.TotalServers == 0)
            {
                string msg = "Fetch Servers To Scan.";
                WpfMessageBox.Show(this, msg, "No Server To Scan!", MessageBoxButton.OK, MessageBoxImage.Information);
                SubscriptionScanButton.IsEnabled = true;
                return;
            }

            if (DGS_Subscription.SelectedItems.Count > 1)
            {
                ObservableCollection<UpstreamItem> selectedItems = new();
                for (int n = 0; n < DGS_Subscription.SelectedItems.Count; n++)
                {
                    if (DGS_Subscription.SelectedItems[n] is UpstreamItem selectedItem)
                        selectedItems.Add(selectedItem);
                }
                MainWindow.UpstreamManager.ScanServers(group, selectedItems);
            }
            else
            {
                MainWindow.UpstreamManager.ScanServers(group, null);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow_Subscription SubscriptionScanButton_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not UpstreamGroup group) return;
            if (sender is not WpfButton b) return;
            ChangeControlsState_Subscription(false);
            b.DispatchIt(() => b.Content = "Sorting...");

            // Sort
            await MainWindow.UpstreamManager.Sort_UpstreamItems_Async(group.Name, true);
            await DGS_Subscription.ScrollIntoViewAsync(0); // Scroll

            b.DispatchIt(() => b.Content = "Sort");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow_Subscription SubscriptionSortButton_Click: " + ex.Message);
        }

        ChangeControlsState_Subscription(true);
    }

}