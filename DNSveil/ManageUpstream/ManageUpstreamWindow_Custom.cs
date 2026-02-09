using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using MsmhToolsClass;
using MsmhToolsClass.V2RayConfigTool;
using MsmhToolsWpfClass;
using static DNSveil.Logic.UpstreamServers.UpstreamModel;

namespace DNSveil.ManageUpstream;

public partial class ManageUpstreamWindow : WpfWindow
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

    private void Custom_Settings_AddFragment_ToggleSwitch_CheckedChanged(object sender, WpfToggleSwitch.CheckedChangedEventArgs e)
    {
        if (e.IsChecked.HasValue)
        {
            Custom_Settings_FragmentSize_TextBox.IsEnabled = e.IsChecked.Value;
            Custom_Settings_FragmentDelay_TextBox.IsEnabled = e.IsChecked.Value;
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

    private void Custom_ToggleAutoScanSelect(bool? value)
    {
        if (value.HasValue)
        {
            if (value.Value)
            {
                // AutoScanSelect ON
                CustomScanServersTextBlock.IsEnabled = false;
                CustomScanServersNumericUpDown.IsEnabled = false;
                CustomLastScanServersTextBlock.IsEnabled = false;
            }
            else
            {
                // AutoScanSelect OFF
                CustomScanServersTextBlock.IsEnabled = true;
                CustomScanServersNumericUpDown.IsEnabled = true;
                CustomLastScanServersTextBlock.IsEnabled = true;
            }
        }
    }

    private void CustomAutoScanSelectToggleSwitch_CheckedChanged(object sender, WpfToggleSwitch.CheckedChangedEventArgs e)
    {
        Custom_ToggleAutoScanSelect(e.IsChecked);
    }

    private async void DGS_Custom_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
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
                        MenuItem_Custom_CopyImportedConfig.IsEnabled = false;
                    }
                    else
                    {
                        MenuItem_Custom_CopyImportedConfig.IsEnabled = true;
                        dg.SelectedIndex = row.GetIndex();
                    }

                    ObservableCollection<UpstreamItem> selectedItems = new();
                    for (int n = 0; n < dg.SelectedItems.Count; n++)
                    {
                        if (dg.SelectedItems[n] is UpstreamItem selectedItem)
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
        if (sender is not DataGrid dg) return;
        dg.FillCustomColumn(1, 200);
    }

    private void DGS_Custom_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not DataGrid dg) return;
        DGS_UpstreamServers_PreviewKeyDown(dg, e);
    }

    private void DGS_Custom_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not DataGrid dg) return;
        DGS_UpstreamServers_SelectionChanged(dg);
    }

    private void MenuItem_Custom_CopyImportedConfig_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ContextMenu_Custom.Tag is ObservableCollection<UpstreamItem> items && items.Count > 0)
            {
                UpstreamItem item = items[0];
                Clipboard.SetText(item.ConfigInfo.UrlOrJson, TextDataFormat.Text);
                WpfToastDialog.Show(this, "Copied To Clipboard.", MessageBoxImage.None, 2);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow_Custom MenuItem_Custom_CopyImportedConfig_Click: " + NL + ex.Message);
        }
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

            if (DGG.SelectedItem is not UpstreamGroup group) return;

            // DeDup
            int nextIndex = GetPreviousOrNextIndex(DGS_Custom, false); // Get Next Index
            bool isSuccess = await MainWindow.UpstreamManager.DeDup_UpstreamItems_Async(group.Name, true);
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
            Debug.WriteLine("ManageUpstreamWindow_Custom Custom_RemoveDuplicates_Async: " + NL + ex.Message);
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

            if (DGG.SelectedItem is not UpstreamGroup group) return;

            List<UpstreamItem> selectedItems = new();
            for (int n = 0; n < DGS_Custom.SelectedItems.Count; n++)
            {
                if (DGS_Custom.SelectedItems[n] is UpstreamItem selectedItem)
                    selectedItems.Add(selectedItem);
            }

            // Confirm Delete
            string plural = selectedItems.Count > 1 ? "s" : string.Empty;
            string msg = $"Deleting {selectedItems.Count} Selected Upstream Item{plural}...{NL}Continue?";
            MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbr != MessageBoxResult.Yes) return;

            // Delete
            int nextIndex = GetPreviousOrNextIndex(DGS_Custom, true); // Get Next Index
            bool isSuccess = await MainWindow.UpstreamManager.Remove_UpstreamItems_Async(group.Name, selectedItems, true);
            await LoadSelectedGroupAsync(); // Refresh
            await DGS_Custom.ScrollIntoViewAsync(nextIndex); // Scroll To Next
            
            if (isSuccess)
                WpfToastDialog.Show(this, "Deleted.", MessageBoxImage.None, 2);
            else
                WpfToastDialog.Show(this, "ERROR: Couldn't Delete Items.", MessageBoxImage.Error, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow_Custom MenuItem_Custom_DeleteItem_Click: " + NL + ex.Message);
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

            if (DGG.SelectedItem is not UpstreamGroup group) return;

            // Confirm Delete
            string msg = $"Deleting All Upstreams...{NL}Continue?";
            MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Delete All", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbr != MessageBoxResult.Yes) return;

            // Delete All
            bool isSuccess = await MainWindow.UpstreamManager.Clear_UpstreamItems_Async(group.Name, true);
            await LoadSelectedGroupAsync(); // Refresh

            if (isSuccess)
                WpfToastDialog.Show(this, "All Items Deleted.", MessageBoxImage.None, 2);
            else
                WpfToastDialog.Show(this, "ERROR: Couldn't Delete Items.", MessageBoxImage.Error, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow_Custom MenuItem_Custom_DeleteAll_Click: " + NL + ex.Message);
        }
    }

    private void MenuItem_Custom_Scan_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGG.SelectedItem is not UpstreamGroup group) return;
            if (ContextMenu_Custom.Tag is ObservableCollection<UpstreamItem> items && items.Count > 0)
            {
                MainWindow.UpstreamManager.ScanServers(group, items);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow_Custom MenuItem_Custom_Scan_Click: " + NL + ex.Message);
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

            if (DGG.SelectedItem is not UpstreamGroup group) return;

            // Get Group Settings
            bool enabled = Custom_EnableGroup_ToggleSwitch.IsChecked.HasValue && Custom_EnableGroup_ToggleSwitch.IsChecked.Value;
            string testURL = Custom_Settings_LookupDomain_TextBox.Text.Trim();
            double timeoutSec = Custom_Settings_TimeoutSec_NumericUpDown.Value;
            int parallelSize = Custom_Settings_ParallelSize_NumericUpDown.Value.ToInt();
            string bootstrapIpStr = Custom_Settings_BootstrapIP_TextBox.Text.Trim();
            int bootstrapPort = Custom_Settings_BootstrapPort_NumericUpDown.Value.ToInt();
            bool addFragment = Custom_Settings_AddFragment_ToggleSwitch.IsChecked.HasValue && Custom_Settings_AddFragment_ToggleSwitch.IsChecked.Value;
            string fragmentSize = Custom_Settings_FragmentSize_TextBox.Text.Trim();
            string fragmentDelay = Custom_Settings_FragmentDelay_TextBox.Text.Trim();
            bool getRegionInfo = Custom_Settings_GetRegionInfo_ToggleSwitch.IsChecked.HasValue && Custom_Settings_GetRegionInfo_ToggleSwitch.IsChecked.Value;
            bool getSpeedTest = Custom_Settings_SpeedTest_ToggleSwitch.IsChecked.HasValue && Custom_Settings_SpeedTest_ToggleSwitch.IsChecked.Value;
            bool dontuseWithoutSecurity = Custom_Settings_DontUseWithoutSecurity_ToggleSwitch.IsChecked.HasValue && Custom_Settings_DontUseWithoutSecurity_ToggleSwitch.IsChecked.Value;
            bool allowInsecure = Custom_Settings_AllowInsecure_ToggleSwitch.IsChecked.HasValue && Custom_Settings_AllowInsecure_ToggleSwitch.IsChecked.Value;

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
            XrayFragment xrayFragment = new() { IsEnabled = addFragment, Size = fragmentSize, Delay = fragmentDelay };
            GroupSettings gs = new(enabled, testURL, timeoutSec, parallelSize, bootstrapIpStr, bootstrapPort, xrayFragment, getRegionInfo, getSpeedTest, dontuseWithoutSecurity, allowInsecure);
            bool isSuccess = await MainWindow.UpstreamManager.Update_GroupSettings_Async(group.Name, gs, false);

            // Select UpstreamItems
            await MainWindow.UpstreamManager.Select_UpstreamItems_Async(group.Name, true);

            await LoadSelectedGroupAsync(); // Refresh
            await Custom_Settings_WpfFlyoutPopup.CloseFlyAsync();

            if (isSuccess)
                WpfToastDialog.Show(this, "Saved.", MessageBoxImage.None, 2);
            else
                WpfToastDialog.Show(this, "ERROR: Couldn't Save Settings.", MessageBoxImage.Error, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow_Custom Custom_Settings_Save_Async: " + ex.Message);
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

            if (DGG.SelectedItem is not UpstreamGroup group) return;

            UpstreamItem currentItem = new();
            if (!TrueAdd_FalseModify)
            {
                if (DGS_Custom.SelectedItem is UpstreamItem currentItemOut) currentItem = currentItemOut;
                else
                {
                    WpfToastDialog.Show(this, $"Select An Item To Modify.", MessageBoxImage.Stop, 2);
                    return;
                }
            }
            string remarks = string.Empty, urlOrJson = string.Empty;

            this.DispatchIt(() =>
            {
                remarks = CustomByManual_Remarks_TextBox.Text.Trim();
                urlOrJson = CustomByManual_UrlOrJson_TextBox.Text.Trim();
            });

            UpstreamItem newItem = new(urlOrJson);
            if (newItem.ConfigInfo.Protocol == ConfigBuilder.Protocol.Unknown)
            {
                WpfMessageBox.Show(this, "Upstream Is Invalid.", "Unknown", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            // Set Remarks
            if (!string.IsNullOrEmpty(remarks)) newItem.ConfigInfo.Remarks = remarks;

            if (TrueAdd_FalseModify)
            {
                // Add
                ObservableCollection<UpstreamItem> newItemList = new() { newItem };
                bool isSuccess = await MainWindow.UpstreamManager.Append_UpstreamItems_Async(group.Name, newItemList, true);
                await LoadSelectedGroupAsync(); // Refresh
                await DGS_Custom.ScrollIntoViewAsync(DGS_Custom.Items.Count - 1); // Scroll To Last Item

                if (isSuccess)
                    WpfToastDialog.Show(this, "Added.", MessageBoxImage.None, 2);
                else
                    WpfToastDialog.Show(this, "ERROR: Couldn't Add The Item.", MessageBoxImage.Error, 2);
            }
            else
            {
                // Modidfy
                bool isSuccess = await MainWindow.UpstreamManager.Update_UpstreamItems_Async(group.Name, currentItem.IDUniqueWithRemarks, newItem, true);
                await LoadSelectedGroupAsync(); // Refresh

                if (isSuccess)
                    WpfToastDialog.Show(this, "Modified.", MessageBoxImage.None, 2);
                else
                    WpfToastDialog.Show(this, "ERROR: Couldn't Modify The Item.", MessageBoxImage.Error, 2);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow_Custom Custom_ByManualAddOrModify_Async: " + ex.Message);
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

    private void CustomByManualClearFieldsButton_Click(object sender, RoutedEventArgs e)
    {
        this.DispatchIt(() =>
        {
            CustomByManual_Remarks_TextBox.Text = string.Empty;
            CustomByManual_UrlOrJson_TextBox.Text = string.Empty;
        });
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

            if (DGG.SelectedItem is not UpstreamGroup group) return;

            // Update URLs
            List<string> urlsOrFiles = CustomSourceTextBox.Text.SplitToLines();
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
            Debug.WriteLine("ManageUpstreamWindow_Custom CustomSaveSourceAsync: " + ex.Message);
        }
    }

    private async void CustomFetchSourceButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsScanning)
        {
            WpfToastDialog.Show(this, "Can't Fetch While Scanning.", MessageBoxImage.Stop, 2);
            return;
        }

        if (DGG.SelectedItem is not UpstreamGroup group) return;
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
            items = items.DistinctBy(_ => _.IDUniqueWithRemarks).ToObservableCollection();

            if (items.Count == 0)
            {
                string msg = "Couldn't Find Any Server!";
                WpfMessageBox.Show(this, msg);
                end();
                return;
            }

            // Add To Group
            bool isSuccess = await MainWindow.UpstreamManager.Add_UpstreamItems_Async(group.Name, items, false);
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
            Debug.WriteLine("ManageUpstreamWindow_Custom CustomFetchSourceButton_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not UpstreamGroup group) return;
            ChangeControlsState_Custom(false);

            // Get Options
            bool autoSelect = CustomAutoScanSelectToggleSwitch.IsChecked.HasValue && CustomAutoScanSelectToggleSwitch.IsChecked.Value;
            int scanServers = CustomScanServersNumericUpDown.Value.ToInt();

            // Update Options
            CustomOptions options = MainWindow.UpstreamManager.Get_Custom_Options(group.Name);
            options.AutoUpdate.AutoScanSelect = autoSelect;
            options.AutoUpdate.ScanServers = scanServers;

            // Save/Update
            bool isSuccess = await MainWindow.UpstreamManager.Update_Custom_Options_Async(group.Name, options, true);
            await LoadSelectedGroupAsync(); // Refresh

            if (isSuccess)
                WpfToastDialog.Show(this, "Saved.", MessageBoxImage.None, 2);
            else
                WpfToastDialog.Show(this, "ERROR: Couldn't Save Options.", MessageBoxImage.Error, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow_Custom CustomSaveOptionsButton_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not UpstreamGroup group) return;
            ChangeControlsState_Custom(false);

            List<string> exportList = new();
            ObservableCollection<UpstreamItem> items = MainWindow.UpstreamManager.Get_UpstreamItems(group.Name);
            for (int n = 0; n < items.Count; n++)
            {
                UpstreamItem item = items[n];
                if (item.IsSelected) exportList.Add(item.ConfigInfo.UrlOrJson);
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
            Debug.WriteLine("ManageUpstreamWindow_Custom CustomExportAsTextButton_Click: " + ex.Message);
        }

        ChangeControlsState_Custom(true);
    }

    private void CustomScanButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGG.SelectedItem is not UpstreamGroup group) return;
            CustomScanButton.IsEnabled = false;

            // Check For Fetch: Get UpstreamItems Info
            UpstreamItemsInfo info = MainWindow.UpstreamManager.Get_UpstreamItems_Info(group.Name);
            if (info.TotalServers == 0)
            {
                string msg = "Fetch Servers To Scan.";
                WpfMessageBox.Show(this, msg, "No Server To Scan!", MessageBoxButton.OK, MessageBoxImage.Information);
                CustomScanButton.IsEnabled = true;
                return;
            }

            if (DGS_Custom.SelectedItems.Count > 1)
            {
                ObservableCollection<UpstreamItem> selectedItems = new();
                for (int n = 0; n < DGS_Custom.SelectedItems.Count; n++)
                {
                    if (DGS_Custom.SelectedItems[n] is UpstreamItem selectedItem)
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
            Debug.WriteLine("ManageUpstreamWindow_Custom CustomScanButton_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not UpstreamGroup group) return;
            if (sender is not WpfButton b) return;
            ChangeControlsState_Custom(false);
            b.DispatchIt(() => b.Content = "Sorting...");
            
            // Sort
            await MainWindow.UpstreamManager.Sort_UpstreamItems_Async(group.Name, true);
            await DGS_Custom.ScrollIntoViewAsync(0); // Scroll

            b.DispatchIt(() => b.Content = "Sort");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow_Custom CustomSortButton_Click: " + ex.Message);
        }

        ChangeControlsState_Custom(true);
    }

}