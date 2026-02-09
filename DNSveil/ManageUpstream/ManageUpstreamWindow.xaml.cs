using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using MsmhToolsClass;
using MsmhToolsWpfClass;
using MsmhToolsWpfClass.Themes;
using static DNSveil.Logic.UpstreamServers.UpstreamModel;
using static DNSveil.Logic.UpstreamServers.UpstreamServersManager;

namespace DNSveil.ManageUpstream;

/// <summary>
/// Interaction logic for ManageUpstreamWindow.xaml
/// </summary>
public partial class ManageUpstreamWindow : WpfWindow
{
    private static readonly object SyncLock = new();
    private static readonly string NL = Environment.NewLine;
    private static readonly string LS = "      ";

    private static readonly int RightClickMenuDelayMS = 150;
    private static readonly int LimitGroupNameLength = 50;
    private static readonly char[] LimitGroupNameChars = "\\/:*?\"<>|".ToCharArray(); // new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };

    public static bool IsScanning { get; set; } = false;

    // Context Menu New Group
    private MenuItem MenuItem_Subscription = new();
    private MenuItem MenuItem_Custom = new();
    private ContextMenu ContextMenu_NewGroup = new();

    // Context Menu Group
    private MenuItem MenuItem_Group_Rename = new();
    private MenuItem MenuItem_Group_Remove = new();
    private MenuItem MenuItem_Group_MoveUp = new();
    private MenuItem MenuItem_Group_MoveDown = new();
    private MenuItem MenuItem_Group_MoveToTop = new();
    private MenuItem MenuItem_Group_MoveToBottom = new();
    private ContextMenu ContextMenu_Group = new();

    // Context Menu Subscription
    private MenuItem MenuItem_Subscription_CopyImportedConfig = new();
    private MenuItem MenuItem_Subscription_CopyToCustom = new();
    private MenuItem MenuItem_Subscription_Scan = new();
    private ContextMenu ContextMenu_Subscription = new();

    // Context Menu Custom
    private MenuItem MenuItem_Custom_CopyImportedConfig = new();
    private MenuItem MenuItem_Custom_CopyToCustom = new();
    private MenuItem MenuItem_Custom_RemoveDuplicates = new();
    private MenuItem MenuItem_Custom_DeleteItem = new();
    private MenuItem MenuItem_Custom_DeleteAll = new();
    private MenuItem MenuItem_Custom_Scan = new();
    private ContextMenu ContextMenu_Custom = new();

    public ManageUpstreamWindow()
    {
        InitializeComponent();

        // Invariant Culture
        Info.SetCulture(CultureInfo.InvariantCulture);

        Flyout_Subscription_Options.IsOpen = false;
        Flyout_Custom_Options.IsOpen = false;
    }

    private static int GetPreviousOrNextIndex(DataGrid dg, bool onRemove)
    {
        int i = -1;
        try
        {
            i = dg.SelectedIndex;
            int min = 0, max = dg.Items.Count - 1;
            var indexes = dg.GetSelectedRowsIndexes();
            i = indexes.MinSelectedIndex;
            int removedMax = max - indexes.SelectedCount;
            if (onRemove)
            {
                if (i > min)
                {
                    int otherSelected = indexes.SelectedCount - 1;
                    if (i + otherSelected == max) i = removedMax;
                }
            }
            else
            {
                if (i > min)
                {
                    i--;
                    if (removedMax > i) i++;
                }
                else
                {
                    i++;
                }
            }
        }
        catch (Exception) { }
        return i;
    }

    private async Task BindItemsAsync(int selectedGroupIndex)
    {
        try
        {
            for (int n = 0; n < MainWindow.UpstreamManager.Model.Groups.Count; n++)
            {
                if (n == selectedGroupIndex)
                {
                    BindingOperations.ClearBinding(DGS_Custom, DataGrid.ItemsSourceProperty);
                    await Task.Delay(1);
                    UpstreamGroup group = MainWindow.UpstreamManager.Model.Groups[n];
                    BindingOperations.EnableCollectionSynchronization(group.Items, SyncLock);
                    if (group.Mode == GroupMode.Subscription)
                    {
                        DGS_Subscription.ItemsSource = group.Items;
                        DGS_Subscription.DataContext = group.Items;
                        await Task.Delay(1);
                        break;
                    }
                    else if (group.Mode == GroupMode.Custom)
                    {
                        DGS_Custom.ItemsSource = group.Items;
                        DGS_Custom.DataContext = group.Items;
                        await Task.Delay(1);
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow BindItemsAsync: " + ex.Message);
        }
    }
    
    private async Task SelectGroupAsync(string? selectGroupByName)
    {
        try
        {
            // Select Group
            int previousSelectedIndex = DGG.SelectedIndex;
            await Task.Delay(1);

            bool isGroupSelected = false;
            if (!string.IsNullOrEmpty(selectGroupByName))
            {
                for (int n = 0; n < DGG.Items.Count; n++)
                {
                    object? item = DGG.Items[n];
                    if (item is not UpstreamGroup group1) continue;
                    if (group1.Name.Equals(selectGroupByName))
                    {
                        DGG.SelectedIndex = n;
                        isGroupSelected = true;
                        break;
                    }
                }
            }

            if (!isGroupSelected && DGG.Items.Count > 0)
            {
                DGG.SelectedIndex = (previousSelectedIndex != -1 && previousSelectedIndex < DGG.Items.Count) ? previousSelectedIndex : 0;
            }

            await Task.Delay(1);
            bool groupChanged = previousSelectedIndex != DGG.SelectedIndex && DGG.SelectedIndex != -1;
            if (groupChanged) await BindItemsAsync(DGG.SelectedIndex);

            // Set Header (e.g. On Rename)
            if (DGG.SelectedItem is UpstreamGroup group2)
            {
                string header = $"Group: \"{group2.Name}\", Mode: \"{group2.Mode}\"";
                this.DispatchIt(() => ServersTitleGroupBox.Header = header);
            }

            DGG.UpdateColumnsWidthToAuto();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow SelectGroupAsync By Name: " + ex.Message);
        }
    }

    private async Task SelectGroupAsync(int selectGroupByIndex)
    {
        try
        {
            // Select Group
            int previousSelectedIndex = DGG.SelectedIndex;
            await Task.Delay(1);

            if (DGG.Items.Count > 0)
            {
                if (0 <= selectGroupByIndex && selectGroupByIndex <= DGG.Items.Count - 1)
                    DGG.SelectedIndex = selectGroupByIndex;
                else
                    DGG.SelectedIndex = (previousSelectedIndex != -1 && previousSelectedIndex < DGG.Items.Count) ? previousSelectedIndex : 0;
            }

            await Task.Delay(1);
            bool groupChanged = previousSelectedIndex != DGG.SelectedIndex && DGG.SelectedIndex != -1;
            if (groupChanged) await BindItemsAsync(DGG.SelectedIndex);

            DGG.UpdateColumnsWidthToAuto();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow SelectGroupAsync By Index: " + ex.Message);
        }
    }

    private async Task LoadSelectedGroupAsync(bool readOnlyGroupSettings = false)
    {
        try
        {
            if (DGG.SelectedItem is not UpstreamGroup group)
            {
                ServersTabControl.SelectedIndex = 0;
                return;
            }

            // Set Header
            string header = $"Group: \"{group.Name}\", Mode: \"{group.Mode}\"";
            this.DispatchIt(() => ServersTitleGroupBox.Header = header);

            // Select Tab
            this.DispatchIt(() =>
            {
                ServersTabControl.SelectedIndex = group.Mode switch
                {
                    GroupMode.Subscription => 1,
                    GroupMode.Custom => 2,
                    _ => 0
                };
            });

            // Get Green Brush
            Brush greenBrush = AppTheme.GetBrush(AppTheme.MediumSeaGreenBrush);

            if (group.Mode == GroupMode.Subscription)
            {
                // Get Group Settings
                GroupSettings groupSettings = MainWindow.UpstreamManager.Get_GroupSettings(group.Name);
                this.DispatchIt(() =>
                {
                    Subscription_EnableGroup_ToggleSwitch.IsChecked = groupSettings.IsEnabled;
                    Subscription_Settings_LookupDomain_TextBox.Text = groupSettings.TestURL;
                    Subscription_Settings_TimeoutSec_NumericUpDown.Value = groupSettings.ScanTimeoutSec;
                    Subscription_Settings_ParallelSize_NumericUpDown.Value = groupSettings.ScanParallelSize;
                    Subscription_Settings_BootstrapIP_TextBox.Text = groupSettings.BootstrapIpStr;
                    Subscription_Settings_BootstrapPort_NumericUpDown.Value = groupSettings.BootstrapPort;
                    Subscription_Settings_AddFragment_ToggleSwitch.IsChecked = groupSettings.Fragment.IsEnabled;
                    Subscription_Settings_FragmentSize_TextBox.Text = groupSettings.Fragment.Size;
                    Subscription_Settings_FragmentDelay_TextBox.Text = groupSettings.Fragment.Delay;
                    Subscription_Settings_GetRegionInfo_ToggleSwitch.IsChecked = groupSettings.GetRegionInfo;
                    Subscription_Settings_SpeedTest_ToggleSwitch.IsChecked = groupSettings.TestSpeed;
                    Subscription_Settings_DontUseWithoutSecurity_ToggleSwitch.IsChecked = groupSettings.DontUseServersWithNoSecurity;
                    Subscription_Settings_AllowInsecure_ToggleSwitch.IsChecked = groupSettings.AllowInsecure;

                    Flyout_Subscription_Source.IsEnabled = groupSettings.IsEnabled;
                    Flyout_Subscription_Options.IsEnabled = groupSettings.IsEnabled;
                    Flyout_Subscription_UpstreamServers.IsEnabled = groupSettings.IsEnabled;
                    Flyout_Subscription_Scan.IsEnabled = groupSettings.IsEnabled;
                });
                if (readOnlyGroupSettings) return;

                // Get URLs
                List<string> subs = MainWindow.UpstreamManager.Get_Source_URLs(group.Name);
                this.DispatchIt(() => SubscriptionSourceTextBox.Text = subs.ToString(NL));

                // Get Options
                SubscriptionOptions options = MainWindow.UpstreamManager.Get_Subscription_Options(group.Name);
                this.DispatchIt(() => SubscriptionUpdateSourceNumericUpDown.Value = options.AutoUpdate.UpdateSource);
                SubscriptionUpdateSourceTextBlock.Clear();
                SubscriptionUpdateSourceTextBlock.AppendText("Last Update: ", null, $"{options.AutoUpdate.LastUpdateSource:yyyy/MM/dd HH:mm:ss}", greenBrush);
                this.DispatchIt(() => SubscriptionAutoScanSelectToggleSwitch.IsChecked = options.AutoUpdate.AutoScanSelect);
                this.DispatchIt(() => SubscriptionScanServersNumericUpDown.Value = options.AutoUpdate.ScanServers);
                SubscriptionLastScanServersTextBlock.Clear();
                SubscriptionLastScanServersTextBlock.AppendText("Last Scan: ", null, $"{options.AutoUpdate.LastScanServers:yyyy/MM/dd HH:mm:ss}", greenBrush);

                // Get UpstreamItems Info
                UpstreamItemsInfo info = MainWindow.UpstreamManager.Get_UpstreamItems_Info(group.Name);
                SubscriptionSource_Info_TextBlock.Clear();
                SubscriptionSource_Info_TextBlock.AppendText("Total Servers: ", null, $"{info.TotalServers}", Brushes.Orange);
                SubscriptionFilter_Info_TextBlock.Clear();
                SubscriptionFilter_Info_TextBlock.AppendText("Total Servers: ", null, $"{info.TotalServers}", Brushes.Orange, $",{LS}Online Servers: ", null, $"{info.OnlineServers}", Brushes.Orange);
                SubscriptionFilter_Info_TextBlock.AppendText($",{LS}Selected Servers: ", null, $"{info.SelectedServers}", Brushes.Orange, $",{LS}Average Latency: ", null, $"{info.AverageLatency}", Brushes.Orange, "ms", null);
            }
            else if (group.Mode == GroupMode.Custom)
            {
                // Get Group Settings
                GroupSettings groupSettings = MainWindow.UpstreamManager.Get_GroupSettings(group.Name);
                this.DispatchIt(() =>
                {
                    Custom_EnableGroup_ToggleSwitch.IsChecked = groupSettings.IsEnabled;
                    Custom_Settings_LookupDomain_TextBox.Text = groupSettings.TestURL;
                    Custom_Settings_TimeoutSec_NumericUpDown.Value = groupSettings.ScanTimeoutSec;
                    Custom_Settings_ParallelSize_NumericUpDown.Value = groupSettings.ScanParallelSize;
                    Custom_Settings_BootstrapIP_TextBox.Text = groupSettings.BootstrapIpStr;
                    Custom_Settings_BootstrapPort_NumericUpDown.Value = groupSettings.BootstrapPort;
                    Custom_Settings_AddFragment_ToggleSwitch.IsChecked = groupSettings.Fragment.IsEnabled;
                    Custom_Settings_FragmentSize_TextBox.Text = groupSettings.Fragment.Size;
                    Custom_Settings_FragmentDelay_TextBox.Text = groupSettings.Fragment.Delay;
                    Custom_Settings_GetRegionInfo_ToggleSwitch.IsChecked = groupSettings.GetRegionInfo;
                    Custom_Settings_SpeedTest_ToggleSwitch.IsChecked = groupSettings.TestSpeed;
                    Custom_Settings_DontUseWithoutSecurity_ToggleSwitch.IsChecked = groupSettings.DontUseServersWithNoSecurity;
                    Custom_Settings_AllowInsecure_ToggleSwitch.IsChecked = groupSettings.AllowInsecure;

                    Flyout_Custom_Source.IsEnabled = groupSettings.IsEnabled;
                    Flyout_Custom_Options.IsEnabled = groupSettings.IsEnabled;
                    Flyout_Custom_UpstreamServers.IsEnabled = groupSettings.IsEnabled;
                    Flyout_Custom_Scan.IsEnabled = groupSettings.IsEnabled;
                });
                if (readOnlyGroupSettings) return;

                // Get URLs
                List<string> subs = MainWindow.UpstreamManager.Get_Source_URLs(group.Name);
                this.DispatchIt(() => CustomSourceTextBox.Text = subs.ToString(NL));

                // Get Options
                CustomOptions options = MainWindow.UpstreamManager.Get_Custom_Options(group.Name);
                this.DispatchIt(() => CustomAutoScanSelectToggleSwitch.IsChecked = options.AutoUpdate.AutoScanSelect);
                this.DispatchIt(() => CustomScanServersNumericUpDown.Value = options.AutoUpdate.ScanServers);
                CustomLastScanServersTextBlock.Clear();
                CustomLastScanServersTextBlock.AppendText("Last Scan: ", null, $"{options.AutoUpdate.LastScanServers:yyyy/MM/dd HH:mm:ss}", greenBrush);

                // Get UpstreamItems Info
                UpstreamItemsInfo info = MainWindow.UpstreamManager.Get_UpstreamItems_Info(group.Name);
                CustomSource_Info_TextBlock.Clear();
                CustomSource_Info_TextBlock.AppendText("Total Servers: ", null, $"{info.TotalServers}", Brushes.Orange);
                CustomFilter_Info_TextBlock.Clear();
                CustomFilter_Info_TextBlock.AppendText("Total Servers: ", null, $"{info.TotalServers}", Brushes.Orange, $",{LS}Online Servers: ", null, $"{info.OnlineServers}", Brushes.Orange);
                CustomFilter_Info_TextBlock.AppendText($",{LS}Selected Servers: ", null, $"{info.SelectedServers}", Brushes.Orange, $",{LS}Average Latency: ", null, $"{info.AverageLatency}", Brushes.Orange, "ms", null);
            }

            await Task.Delay(1);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow LoadSelectedGroupAsync: " + ex.Message);
        }
    }

    private async void WpfWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Enable Cross Access To This Collection
            BindingOperations.EnableCollectionSynchronization(MainWindow.UpstreamManager.Model.Groups, SyncLock);
            DGG.ItemsSource = MainWindow.UpstreamManager.Model.Groups;

            // Hide ServersTabControl Header
            ServersTabControl.HideHeader();

            // Load Theme

            // Wait For Background Task
            await Task.Run(async () =>
            {
                while (true)
                {
                    bool isBackgroundTaskWorking = MainWindow.UpstreamManager.IsBackgroundTaskWorking;
                    if (isBackgroundTaskWorking)
                    {
                        this.DispatchIt(() => IsHitTestVisible = false);
                        NoGroupText_TextBlock.SetText("Stopping Background Task...");
                        ServersTabControl.DispatchIt(() => ServersTabControl.SelectedIndex = 0);
                    }
                    else
                    {
                        this.DispatchIt(() => IsHitTestVisible = true);
                        NoGroupText_TextBlock.SetText("Select A Group.");
                        break;
                    }
                    await Task.Delay(100);
                }
            });

            // Set Helps
            Help_Groups_Import.Content = $"Restore Your Backup.{NL}Import Groups From File.";
            Help_Groups_Export.Content = $"Backup Your Groups.{NL}Export Groups To File.";
            string helpRegionInfo = $"\u2022 The Stability Value Must Be At Least 50%.{NL}\u2022 Get Country Location Of The Upstream Server.";
            Help_Subscription_RegionInfo.Content = helpRegionInfo;
            Help_Custom_RegionInfo.Content = helpRegionInfo;
            string helpSpeedTest = $"\u2022 The Stability Value Must Be At Least 50%.{NL}\u2022 Speed Test Uses Your Bandwidth.{NL}\u2022 Enable Only If You have Enough Bandwidth To Spare.{NL}\u2022 Increases Scan Time.{NL}\u2022 Background Scan Will Ignore Speed Test.";
            Help_Subscription_SpeedTest.Content = helpSpeedTest;
            Help_Custom_SpeedTest.Content = helpSpeedTest;
            string helpAutoScanSelect = $"\u2022 Automatically Scan And Selects The Best Upstream Server.{NL}\u2022 If Current Selected Server Stop Responding, Another One Would Be Selected.";
            Help_Subscription_AutoScanSelect.Content = helpAutoScanSelect;
            Help_Custom_AutoScanSelect.Content = helpAutoScanSelect;
            string helpScan = $"\u2022 The App Will Automatically Scan Servers In The Background.{NL}\u2022 Background Scan Will Ignore Speed Test.";
            Help_Subscription_ScanButton.Content = helpScan;
            Help_Custom_ScanButton.Content = helpScan;

            // Set Max Size Of Import/Export
            Import_ListBox.MaxWidth = SystemParameters.PrimaryScreenWidth * 70 / 100;
            Import_ListBox.MaxHeight = SystemParameters.PrimaryScreenHeight * 70 / 100;
            Export_ListBox.MaxWidth = Import_ListBox.MaxWidth;
            Export_ListBox.MaxHeight = Import_ListBox.MaxHeight;

            // Update Toggles By Value
            Subscription_ToggleSourceByUrlByFile(SubscriptionSourceToggleSwitch.IsChecked);
            Subscription_ToggleAutoScanSelect(SubscriptionAutoScanSelectToggleSwitch.IsChecked);
            Custom_ToggleSourceByUrlByFileByManual(CustomSourceToggleSwitch.IsChecked);
            Custom_ToggleAutoScanSelect(CustomAutoScanSelectToggleSwitch.IsChecked);

            // Set Max Lines Of TextBoxes
            SubscriptionSourceTextBox.SetMaxLines(5, this);
            CustomSourceTextBox.SetMaxLines(5, this);

            // Create UpstreamItem Columns: DGS
            await CreateUpstreamItemColumns_Async(DGS_Subscription);
            await CreateUpstreamItemColumns_Async(DGS_Custom);

            // Load Groups: DGG
            await SelectGroupAsync(0);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow WpfWindow_Loaded: " + ex.Message);
        }
    }

    private void WpfWindow_ContentRendered(object sender, EventArgs e)
    {
        try
        {
            // Context Menu New Group
            MenuItem_Subscription = new()
            {
                Header = nameof(GroupMode.Subscription),
                Tag = GroupMode.Subscription
            };
            MenuItem_Subscription.Click -= MenuNewGroup_Click;
            MenuItem_Subscription.Click += MenuNewGroup_Click;

            MenuItem_Custom = new()
            {
                Header = nameof(GroupMode.Custom),
                Tag = GroupMode.Custom
            };
            MenuItem_Custom.Click -= MenuNewGroup_Click;
            MenuItem_Custom.Click += MenuNewGroup_Click;

            ContextMenu_NewGroup = new();
            ContextMenu_NewGroup.Items.Add(MenuItem_Subscription);
            ContextMenu_NewGroup.Items.Add(MenuItem_Custom);
            ContextMenu_NewGroup.PlacementTarget = NewGroupButton;
            ContextMenu_NewGroup.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;

            // Context Menu Group
            MenuItem_Group_Rename = new()
            {
                Header = "Rename Group...",
                InputGestureText = "F2"
            };
            MenuItem_Group_Rename.Click -= MenuItem_Group_Rename_Click;
            MenuItem_Group_Rename.Click += MenuItem_Group_Rename_Click;

            MenuItem_Group_Remove = new()
            {
                Header = "Remove"
            };
            MenuItem_Group_Remove.Click -= MenuItem_Group_Remove_Click;
            MenuItem_Group_Remove.Click += MenuItem_Group_Remove_Click;

            MenuItem_Group_MoveUp = new()
            {
                Header = "Move Up",
                InputGestureText = "Ctrl+Up"
            };
            MenuItem_Group_MoveUp.Click -= MenuItem_Group_MoveUp_Click;
            MenuItem_Group_MoveUp.Click += MenuItem_Group_MoveUp_Click;

            MenuItem_Group_MoveDown = new()
            {
                Header = "Move Down",
                InputGestureText = "Ctrl+Down"
            };
            MenuItem_Group_MoveDown.Click -= MenuItem_Group_MoveDown_Click;
            MenuItem_Group_MoveDown.Click += MenuItem_Group_MoveDown_Click;

            MenuItem_Group_MoveToTop = new()
            {
                Header = "Move To Top",
                InputGestureText = "Ctrl+Home"
            };
            MenuItem_Group_MoveToTop.Click -= MenuItem_Group_MoveToTop_Click;
            MenuItem_Group_MoveToTop.Click += MenuItem_Group_MoveToTop_Click;

            MenuItem_Group_MoveToBottom = new()
            {
                Header = "Move To Bottom",
                InputGestureText = "Ctrl+End"
            };
            MenuItem_Group_MoveToBottom.Click -= MenuItem_Group_MoveToBottom_Click;
            MenuItem_Group_MoveToBottom.Click += MenuItem_Group_MoveToBottom_Click;

            ContextMenu_Group = new();
            ContextMenu_Group.Items.Add(MenuItem_Group_Rename);
            ContextMenu_Group.Items.Add(MenuItem_Group_Remove);
            ContextMenu_Group.Items.Add(MenuItem_Group_MoveUp);
            ContextMenu_Group.Items.Add(MenuItem_Group_MoveDown);
            ContextMenu_Group.Items.Add(MenuItem_Group_MoveToTop);
            ContextMenu_Group.Items.Add(MenuItem_Group_MoveToBottom);

            DGG.ContextMenu = ContextMenu_Group;

            // Context Menu Subscription
            MenuItem_Subscription_CopyImportedConfig = new()
            {
                Header = "Copy Imported Upstream To Clipboard"
            };
            MenuItem_Subscription_CopyImportedConfig.Click -= MenuItem_Subscription_CopyImportedConfig_Click;
            MenuItem_Subscription_CopyImportedConfig.Click += MenuItem_Subscription_CopyImportedConfig_Click;

            MenuItem_Subscription_CopyToCustom = new()
            {
                Header = "Copy Selected Upstream To..."
            };

            MenuItem_Subscription_Scan = new()
            {
                Header = "Scan Selected Servers"
            };
            MenuItem_Subscription_Scan.Click -= MenuItem_Subscription_Scan_Click;
            MenuItem_Subscription_Scan.Click += MenuItem_Subscription_Scan_Click;

            ContextMenu_Subscription = new();
            ContextMenu_Subscription.Items.Add(MenuItem_Subscription_CopyImportedConfig);
            ContextMenu_Subscription.Items.Add(MenuItem_Subscription_CopyToCustom);
            ContextMenu_Subscription.Items.Add(MenuItem_Subscription_Scan);
            ContextMenu_Subscription.Closed += (s, e) =>
            {
                foreach (MenuItem menuItem in MenuItem_Subscription_CopyToCustom.Items)
                    menuItem.Click -= MenuItem_CopyToCustom_Click;
            };

            // Context Menu Custom
            MenuItem_Custom_CopyImportedConfig = new()
            {
                Header = "Copy Imported Upstream To Clipboard"
            };
            MenuItem_Custom_CopyImportedConfig.Click -= MenuItem_Custom_CopyImportedConfig_Click;
            MenuItem_Custom_CopyImportedConfig.Click += MenuItem_Custom_CopyImportedConfig_Click;

            MenuItem_Custom_CopyToCustom = new()
            {
                Header = "Copy Selected Upstream To..."
            };

            MenuItem_Custom_RemoveDuplicates = new()
            {
                Header = "Remove Duplicates"
            };
            MenuItem_Custom_RemoveDuplicates.Click -= MenuItem_Custom_RemoveDuplicates_Click;
            MenuItem_Custom_RemoveDuplicates.Click += MenuItem_Custom_RemoveDuplicates_Click;

            MenuItem_Custom_DeleteItem = new()
            {
                Header = "Delete Selected Servers"
            };
            MenuItem_Custom_DeleteItem.Click -= MenuItem_Custom_DeleteItem_Click;
            MenuItem_Custom_DeleteItem.Click += MenuItem_Custom_DeleteItem_Click;

            MenuItem_Custom_DeleteAll = new()
            {
                Header = "Delete All"
            };
            MenuItem_Custom_DeleteAll.Click -= MenuItem_Custom_DeleteAll_Click;
            MenuItem_Custom_DeleteAll.Click += MenuItem_Custom_DeleteAll_Click;

            MenuItem_Custom_Scan = new()
            {
                Header = "Scan Selected Servers"
            };
            MenuItem_Custom_Scan.Click -= MenuItem_Custom_Scan_Click;
            MenuItem_Custom_Scan.Click += MenuItem_Custom_Scan_Click;

            ContextMenu_Custom = new();
            ContextMenu_Custom.Items.Add(MenuItem_Custom_CopyImportedConfig);
            ContextMenu_Custom.Items.Add(MenuItem_Custom_CopyToCustom);
            ContextMenu_Custom.Items.Add(MenuItem_Custom_RemoveDuplicates);
            ContextMenu_Custom.Items.Add(MenuItem_Custom_DeleteItem);
            ContextMenu_Custom.Items.Add(MenuItem_Custom_DeleteAll);
            ContextMenu_Custom.Items.Add(MenuItem_Custom_Scan);
            ContextMenu_Custom.Closed += (s, e) =>
            {
                foreach (MenuItem menuItem in MenuItem_Custom_CopyToCustom.Items)
                    menuItem.Click -= MenuItem_CopyToCustom_Click;
            };

            MainWindow.UpstreamManager.OnBackgroundUpdateReceived -= UpstreamManager_OnBackgroundUpdateReceived;
            MainWindow.UpstreamManager.OnBackgroundUpdateReceived += UpstreamManager_OnBackgroundUpdateReceived;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow WpfWindow_ContentRendered: " + ex.Message);
        }
    }

    private async void UpstreamManager_OnBackgroundUpdateReceived(object? sender, BackgroundWorkerEventArgs e)
    {
        //Debug.WriteLine("On Background Update Received");
        await UpdateUIByBackgroundWorkerAsync(e);
    }

    private async Task UpdateUIByBackgroundWorkerAsync(BackgroundWorkerEventArgs bw)
    {
        try
        {
            if (DGG.SelectedItem is not UpstreamGroup group) return;
            if (!bw.Group.Name.Equals(group.Name)) return;
            IsScanning = bw.IsWorking;
            await Import_FlyoutOverlay.CloseFlyAsync();

            if (bw.Group.Mode == GroupMode.Subscription)
            {
                // Scan Subscription
                ChangeControlsState_Subscription(!bw.IsWorking);
                this.DispatchIt(() =>
                {
                    SubscriptionScanButton.IsEnabled = bw.ButtonEnable;
                    SubscriptionScanButton.Content = bw.ButtonText;
                    SubscriptionScanInfoTextBlock.Text = bw.Description;

                    if (bw.ButtonEnable)
                    {
                        SubscriptionScanProgressBar.Minimum = bw.ProgressMin;
                        SubscriptionScanProgressBar.Maximum = bw.ProgressMax;
                        SubscriptionScanProgressBar.Value = bw.ProgressValue;
                    }
                });

                if (bw.ButtonEnable)
                {
                    SubscriptionScanProgressBarTextBlock.Clear();
                    SubscriptionScanProgressBarTextBlock.AppendText($"{bw.ProgressPercentage}% - {bw.ProgressValue} Of {bw.ProgressMax}{LS}{LS}Online Servers: ", null, $"{bw.OnlineServers}", Brushes.Orange);
                    SubscriptionScanProgressBarTextBlock.AppendText($"{LS}{LS}Average Latency: ", null, $"{bw.AverageLatencyMS}", Brushes.Orange, "ms", null);
                }
                
                if (bw.ParallelLatencyMS > 1000 || !bw.IsWorking) // Smooth UI
                {
                    await LoadSelectedGroupAsync();
                    if (bw.LastIndex != -1) await DGS_Subscription.ScrollIntoViewAsync(bw.LastIndex); // Scroll
                }

                DGS_Subscription.FillCustomColumn(1, 200);
            }
            else if (bw.Group.Mode == GroupMode.Custom)
            {
                // Scan Custom
                ChangeControlsState_Custom(!bw.IsWorking);
                this.DispatchIt(() =>
                {
                    CustomScanButton.IsEnabled = bw.ButtonEnable;
                    CustomScanButton.Content = bw.ButtonText;
                    CustomScanInfoTextBlock.Text = bw.Description;

                    if (bw.ButtonEnable)
                    {
                        CustomScanProgressBar.Minimum = bw.ProgressMin;
                        CustomScanProgressBar.Maximum = bw.ProgressMax;
                        CustomScanProgressBar.Value = bw.ProgressValue;
                    }
                });

                if (bw.ButtonEnable)
                {
                    CustomScanProgressBarTextBlock.Clear();
                    CustomScanProgressBarTextBlock.AppendText($"{bw.ProgressPercentage}% - {bw.ProgressValue} Of {bw.ProgressMax}{LS}{LS}Online Servers: ", null, $"{bw.OnlineServers}", Brushes.Orange);
                    CustomScanProgressBarTextBlock.AppendText($"{LS}{LS}Average Latency: ", null, $"{bw.AverageLatencyMS}", Brushes.Orange, "ms", null);
                }

                if (bw.ParallelLatencyMS > 1000 || !bw.IsWorking) // Smooth UI
                {
                    await LoadSelectedGroupAsync(); // Refresh
                    if (bw.LastIndex != -1) await DGS_Custom.ScrollIntoViewAsync(bw.LastIndex); // Scroll
                }

                DGS_Custom.FillCustomColumn(1, 200);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow UpdateUIByBackgroundWorkerAsync: " + ex.Message);
        }
    }

    private void NewGroupButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Do This While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            ContextMenu_NewGroup.IsOpen = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow NewGroupButton_PreviewMouseUp: " + ex.Message);
        }
    }

    private async void ResetBuiltInButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Do This While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            IsHitTestVisible = false;
            bool imported = await MainWindow.UpstreamManager.Reset_BuiltIn_Groups_Async(true);
            IsHitTestVisible = true;

            if (imported)
            {
                await SelectGroupAsync(0); // Refresh

                string msg = "Built-In Groups Imported Successfully.";
                WpfMessageBox.Show(this, msg, "Imported", MessageBoxButton.OK);
            }
            else
            {
                string msg = "Couldn't Import Built-In Groups!";
                WpfMessageBox.Show(this, msg, "Something Went Wrong!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow ResetBuiltInButton_Click: " + ex.Message);
        }
    }

    // Import
    private async void Import_FlyoutOverlay_FlyoutChanged(object sender, WpfFlyoutPopup.FlyoutChangedEventArgs e)
    {
        try
        {
            if (Export_FlyoutOverlay.IsOpen) await Export_FlyoutOverlay.CloseFlyAsync();

            // Clear Import List
            if (e.IsFlyoutOpen)
            {
                // Enable Settings CheckBox
                Import_Settings_CheckBox.IsEnabled = false;
                Import_Settings_CheckBox.IsChecked = false;
                Import_ListBox.ItemsSource = null;
                Import_ListBox.Tag = null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow Import_FlyoutOverlay_FlyoutChanged: " + ex.Message);
        }
    }

    private async void ImportBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ImportBrowseButton.IsEnabled = false;

            OpenFileDialog ofd = new()
            {
                Filter = "DNSveil Upstream Servers (*.dvlu)|*.dvlu",
                DefaultExt = ".dvlu",
                AddExtension = true,
                Multiselect = false,
                RestoreDirectory = true
            };

            bool? dr = ofd.ShowDialog(this);
            if (dr.HasValue && dr.Value)
            {
                string filePath = ofd.FileName;
                string json = await File.ReadAllTextAsync(filePath);
                bool isValidJson = JsonTool.IsValid(json);
                if (!isValidJson)
                {
                    string msg = $"{Path.GetExtension(filePath).ToUpperInvariant()} File Is Not Valid!";
                    WpfMessageBox.Show(this, msg, "Not Valid", MessageBoxButton.OK, MessageBoxImage.Error);
                    ImportBrowseButton.IsEnabled = true;
                    return;
                }

                // Create Model
                UpstreamExportImport? exportImport = await JsonTool.DeserializeAsync<UpstreamExportImport>(json);
                if (exportImport == null)
                {
                    WpfToastDialog.Show(this, "ERROR: JSON Deserialization Failed.", MessageBoxImage.Error, 2);
                    ImportBrowseButton.IsEnabled = true;
                    return;
                }
                
                if (exportImport.Settings == null && exportImport.Groups.Count == 0)
                {
                    string msg = $"{Path.GetExtension(filePath).ToUpperInvariant()} File Has No Groups!";
                    WpfMessageBox.Show(this, msg, "No Groups", MessageBoxButton.OK, MessageBoxImage.Stop);
                    ImportBrowseButton.IsEnabled = true;
                    return;
                }

                // Open Fly
                await Import_FlyoutOverlay.OpenFlyAsync();

                // Enable Settings CheckBox
                if (exportImport.Settings != null)
                {
                    Import_Settings_CheckBox.IsEnabled = true;
                    Import_Settings_CheckBox.IsChecked = true;
                }

                Import_ListBox.ItemsSource = exportImport.Groups;
                Import_ListBox.Tag = exportImport;
                ImportBrowseButton.IsEnabled = true;

                // Dispose
                json = string.Empty;
            }
            else
            {
                // Browse Canceled
                Import_ListBox.ItemsSource = null;
                Import_ListBox.Tag = null;
                ImportBrowseButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow ImportBrowseButton_Click: " + ex.Message);
        }
    }

    private void Import_CheckBox_CheckedUnchecked(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not CheckBox cb) return;
            if (!cb.IsChecked.HasValue) return;
            if (Import_ListBox.ItemsSource is not List<UpstreamGroup> groups) return;
            if (Import_ListBox.Tag is not UpstreamExportImport exportImport) return;

            // Modify Based On User Selection
            for (int n = 0; n < groups.Count; n++)
            {
                UpstreamGroup group = groups[n];
                if (group.Name.Equals(cb.Content.ToString()))
                {
                    group.IsSelected = cb.IsChecked.Value;
                    groups[n] = group;
                    break;
                }
            }
            exportImport.Groups = groups;

            Import_ListBox.ItemsSource = groups;
            Import_ListBox.Tag = exportImport;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow Import_CheckBox_CheckedUnchecked: " + ex.Message);
        }
    }

    private async void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Import_ListBox.Tag is not UpstreamExportImport exportImport)
            {
                WpfToastDialog.Show(this, "Browse Your Backup File.", MessageBoxImage.Stop, 2);
                return;
            }

            ImportButton.IsEnabled = false;

            // We Don't Have Settings Yet!
            bool importSettings = Import_Settings_CheckBox.IsChecked.HasValue && Import_Settings_CheckBox.IsChecked.Value;
            importSettings = false; // Override - Placeholder

            List<UpstreamGroup> groupsToImport = new();
            for (int n = 0; n < exportImport.Groups.Count; n++)
            {
                UpstreamGroup group = exportImport.Groups[n];
                if (group.IsSelected)
                {
                    group.IsBuiltIn = false; // Imported Groups Can't Be Built-In If Modified On File.
                    groupsToImport.Add(group);
                }
            }

            if (!importSettings && groupsToImport.Count == 0)
            {
                WpfToastDialog.Show(this, "Select At Least One Group To Import.", MessageBoxImage.Stop, 3);
                ImportButton.IsEnabled = true;
                return;
            }

            // Import Settings
            bool isImportSettingsSuccess = false;
            if (importSettings && exportImport.Settings != null)
                isImportSettingsSuccess = await MainWindow.UpstreamManager.Update_Settings_Async(exportImport.Settings, false);

            // Import Groups
            string lastImportedGroupName = string.Empty;
            for (int n = 0; n < groupsToImport.Count; n++)
            {
                UpstreamGroup groupToImport = groupsToImport[n];
                lastImportedGroupName = await MainWindow.UpstreamManager.Add_Group_Async(groupToImport, false, false);
            }

            if (isImportSettingsSuccess || !string.IsNullOrEmpty(lastImportedGroupName))
            {
                await MainWindow.UpstreamManager.SaveAsync(); // Save
                await SelectGroupAsync(lastImportedGroupName); // Refresh

                ImportButton.IsEnabled = true;
                await Import_FlyoutOverlay.CloseFlyAsync();

                string msg = "Selected Groups Imported Successfully.";
                WpfMessageBox.Show(this, msg, "Imported", MessageBoxButton.OK);
            }
            else
            {
                ImportButton.IsEnabled = true;
                await Import_FlyoutOverlay.CloseFlyAsync();

                string msg = "A Group Didn't Import!";
                WpfMessageBox.Show(this, msg, "Something Went Wrong!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Dispose
            exportImport.Dispose();
            groupsToImport.Clear();
            Import_ListBox.ItemsSource = null;
            Import_ListBox.Tag = null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow ImportButton_Click: " + ex.Message);
        }
    }

    // Export
    private async void Export_FlyoutOverlay_FlyoutChanged(object sender, WpfFlyoutPopup.FlyoutChangedEventArgs e)
    {
        try
        {
            if (Import_FlyoutOverlay.IsOpen) await Import_FlyoutOverlay.CloseFlyAsync();

            if (e.IsFlyoutOpen)
            {
                List<UpstreamGroup> groups = MainWindow.UpstreamManager.Get_Groups(false);
                Export_ListBox.ItemsSource = groups;
                Export_ListBox.SelectedIndex = -1;
            }
            else
            {
                Export_ListBox.ItemsSource = null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow Export_FlyoutOverlay_FlyoutChanged: " + ex.Message);
        }
    }

    private void Export_CheckBox_CheckedUnchecked(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not CheckBox cb) return;
            if (!cb.IsChecked.HasValue) return;
            if (Export_ListBox.ItemsSource is not List<UpstreamGroup> groups) return;

            // Modify Based On User Selection
            for (int n = 0; n < groups.Count; n++)
            {
                UpstreamGroup group = groups[n];
                if (group.Name.Equals(cb.Content.ToString()))
                {
                    group.IsSelected = cb.IsChecked.Value;
                    groups[n] = group;
                    break;
                }
            }

            Export_ListBox.ItemsSource = groups;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow Export_CheckBox_CheckedUnchecked: " + ex.Message);
        }
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Export_ListBox.ItemsSource is not List<UpstreamGroup> groups) return;
            ExportButton.IsEnabled = false;

            // We Don't Have Settings Yet!
            bool exportSettings = Export_Settings_CheckBox.IsChecked.HasValue && Export_Settings_CheckBox.IsChecked.Value;
            exportSettings = false; // Override - Placeholder

            List<UpstreamGroup> groupsToExport = new();
            for (int n = 0; n < groups.Count; n++)
            {
                UpstreamGroup group = groups[n];
                if (group.IsSelected)
                {
                    group.IsBuiltIn = false; // Export As User
                    groupsToExport.Add(group);
                }
            }

            if (!exportSettings && groupsToExport.Count == 0)
            {
                WpfToastDialog.Show(this, "Select At Least One Group To Export.", MessageBoxImage.Stop, 3);
                ExportButton.IsEnabled = true;
                return;
            }

            // Create Model
            UpstreamExportImport exportImport = new()
            {
                Settings = exportSettings ? MainWindow.UpstreamManager.Model.Settings : null,
                Groups = groupsToExport
            };

            // Create JSON
            string json = await JsonTool.SerializeAsync(exportImport, false);

            // Check JSON Is Valid
            if (string.IsNullOrEmpty(json) || !JsonTool.IsValid(json))
            {
                WpfToastDialog.Show(this, "ERROR: JSON Serialization Failed.", MessageBoxImage.Error, 2);
                ExportButton.IsEnabled = true;
                return;
            }

            SaveFileDialog sfd = new()
            {
                Filter = "DNSveil Upstream Servers (*.dvlu)|*.dvlu",
                DefaultExt = ".dvlu",
                AddExtension = true,
                RestoreDirectory = true,
                FileName = $"DNSveil_Upstream_Servers_{DateTime.Now:yyyy.MM.dd-HH.mm.ss}"
            };

            bool? dr = sfd.ShowDialog(this);
            if (dr.HasValue && dr.Value)
            {
                try
                {
                    await File.WriteAllTextAsync(sfd.FileName, json, new UTF8Encoding(false));
                    await Export_FlyoutOverlay.CloseFlyAsync();

                    string msg = "Selected Groups Exported Successfully.";
                    WpfMessageBox.Show(this, msg, "Exported", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    WpfMessageBox.Show(this, ex.GetInnerExceptions(), "Something Went Wrong!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Dispose
            groups.Clear();
            groupsToExport.Clear();
            exportImport.Dispose();
            json = string.Empty;
            ExportButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow ExportButton_Click: " + ex.Message);
        }
    }

    private async void MenuNewGroup_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Do This While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (sender is not MenuItem menuItem) return;
            if (menuItem.Tag is not GroupMode groupMode) return;
            string newGroupName = string.Empty;
            bool dialogResult = WpfInputBox.Show(this, ref newGroupName, $"New \"{groupMode}\" Group Name:", "Create Group", false);
            if (!dialogResult) return;

            if (string.IsNullOrWhiteSpace(newGroupName))
            {
                string msg = "Name Cannot Be Empty Or White Space.";
                WpfMessageBox.Show(this, msg);
                return;
            }

            if (newGroupName.ToLower().Equals("msasanmh"))
            {
                string msg = $"\"{newGroupName}\" Is Predefined, Choose Another Name.";
                WpfMessageBox.Show(this, msg);
                return;
            }

            if (newGroupName.Contains(LimitGroupNameChars))
            {
                string msg = $"Group Name Can't Contain Any Of The Following Characters:\n";
                msg += $"{LS}{new string(LimitGroupNameChars)}";
                WpfMessageBox.Show(this, msg);
                return;
            }

            if (newGroupName.Length >= LimitGroupNameLength)
            {
                string msg = $"Group Name Length Must Be Less Than Or Equal To {LimitGroupNameLength} Characters.";
                WpfMessageBox.Show(this, msg);
                return;
            }

            List<string> groupNames = MainWindow.UpstreamManager.Get_Group_Names(false);
            if (groupNames.IsContain(newGroupName))
            {
                string msg = $"\"{newGroupName}\" Is Already Exist, Choose Another Name.";
                WpfMessageBox.Show(this, msg);
                return;
            }

            await MainWindow.UpstreamManager.Add_Group_Async(new UpstreamGroup(newGroupName, groupMode), false, true);
            await SelectGroupAsync(newGroupName);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow MenuNewGroup_Click: " + ex.Message);
        }
    }

    private async void MenuItem_Group_Rename_Click(object? sender, RoutedEventArgs? e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Do This While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not UpstreamGroup group) return;

            // Get New Name
            string newGroupName = group.Name;
            bool dialogResult = WpfInputBox.Show(this, ref newGroupName, "New Group Name:", "Rename Group", false);
            if (!dialogResult) return;

            if (string.IsNullOrWhiteSpace(newGroupName))
            {
                string msg = "Name Cannot Be Empty Or White Space.";
                WpfMessageBox.Show(this, msg);
                return;
            }

            if (newGroupName.ToLower().Equals("msasanmh"))
            {
                string msg = $"\"{newGroupName}\" Is Predefined, Choose Another Name.";
                WpfMessageBox.Show(this, msg);
                return;
            }

            if (newGroupName.Contains(LimitGroupNameChars))
            {
                string msg = $"Group Name Can't Contain Any Of The Following Characters:\n";
                msg += $"{LS}{new string(LimitGroupNameChars)}";
                WpfMessageBox.Show(this, msg);
                return;
            }

            if (newGroupName.Length >= LimitGroupNameLength)
            {
                string msg = $"Group Name Length Must Be Less Than Or Equal To {LimitGroupNameLength} Characters.";
                WpfMessageBox.Show(this, msg);
                return;
            }

            if (newGroupName.Equals(group.Name)) return;

            List<string> groupNames = MainWindow.UpstreamManager.Get_Group_Names(false);
            if (groupNames.IsContain(newGroupName))
            {
                string msg = $"\"{newGroupName}\" Is Already Exist, Choose Another Name.";
                WpfMessageBox.Show(this, msg);
                return;
            }

            // Get Old Name
            await MainWindow.UpstreamManager.Rename_Group_Async(group.Name, newGroupName, true);
            await SelectGroupAsync(newGroupName);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow MenuItem_Group_Rename_Click: " + ex.Message);
        }
    }

    private async void MenuItem_Group_Remove_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Do This While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not UpstreamGroup group) return;

            string msg = $"Deleting \"{group.Name}\" Group...{NL}Continue?";
            MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbr != MessageBoxResult.Yes) return;

            int nextIndex = GetPreviousOrNextIndex(DGG, true); // Get Next Index
            await MainWindow.UpstreamManager.Remove_Group_Async(group.Name, true); // Remove
            await SelectGroupAsync(nextIndex); // Select
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow MenuItem_Group_Remove_Click: " + ex.Message);
        }
    }

    private async void MenuItem_Group_MoveUp_Click(object? sender, RoutedEventArgs? e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Do This While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not UpstreamGroup group) return;

            // Get Previous Index
            int index = -1;
            if (DGG.SelectedIndex > 0) index = DGG.SelectedIndex - 1;
            if (index != -1)
            {
                await MainWindow.UpstreamManager.Move_Group_Async(group.Name, index, true); // Move
                await SelectGroupAsync(group.Name); // Select
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow MenuItem_Group_MoveUp_Click: " + ex.Message);
        }
    }

    private async void MenuItem_Group_MoveDown_Click(object? sender, RoutedEventArgs? e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Do This While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not UpstreamGroup group) return;

            // Get Next Index
            int index = -1;
            if (DGG.SelectedIndex < DGG.Items.Count - 1) index = DGG.SelectedIndex + 1;
            if (index != -1)
            {
                await MainWindow.UpstreamManager.Move_Group_Async(group.Name, index, true); // Move
                await SelectGroupAsync(group.Name); // Select
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow MenuItem_Group_MoveDown_Click: " + ex.Message);
        }
    }

    private async void MenuItem_Group_MoveToTop_Click(object? sender, RoutedEventArgs? e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Do This While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not UpstreamGroup group) return;

            if (DGG.Items.Count > 0 && DGG.SelectedIndex != 0)
            {
                await MainWindow.UpstreamManager.Move_Group_Async(group.Name, 0, true); // Move
                await SelectGroupAsync(group.Name); // Select
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow MenuItem_Group_MoveToTop_Click: " + ex.Message);
        }
    }

    private async void MenuItem_Group_MoveToBottom_Click(object? sender, RoutedEventArgs? e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Do This While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not UpstreamGroup group) return;

            if (DGG.Items.Count > 0 && DGG.SelectedIndex != DGG.Items.Count - 1)
            {
                await MainWindow.UpstreamManager.Move_Group_Async(group.Name, DGG.Items.Count - 1, true); // Move
                await SelectGroupAsync(group.Name); // Select
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow MenuItem_Group_MoveToBottom_Click: " + ex.Message);
        }
    }

    private async void DGG_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            if (sender is not DataGrid dg) return;
            e.Handled = true;
            if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                // Delete All Upstream Items
                if (e.Key == Key.Delete) // + Del
                {
                    if (DGG.SelectedItem is UpstreamGroup group)
                    {
                        string msg = $"Deleting All Upstream Items...{NL}Continue?";
                        MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Delete All", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (mbr == MessageBoxResult.Yes)
                        {
                            await MainWindow.UpstreamManager.Clear_UpstreamItems_Async(group.Name, true);
                            await LoadSelectedGroupAsync(); // Refresh
                        }
                    }
                }
                // Set Group As Built-In
                else if (e.Key == Key.B)
                {
                    if (DGG.SelectedItem is UpstreamGroup group)
                    {
                        string msg = $"Setting Group \"{group.Name}\" As Built-In...{NL}Continue?";
                        MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Set As Built-In", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (mbr == MessageBoxResult.Yes)
                        {
                            await MainWindow.UpstreamManager.Update_Group_As_BuiltIn_Async(group.Name, true, true);
                        }
                    }
                }
                // Set Group As User
                else if (e.Key == Key.U)
                {
                    if (DGG.SelectedItem is UpstreamGroup group)
                    {
                        string msg = $"Setting Group \"{group.Name}\" As User...{NL}Continue?";
                        MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Set As User", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (mbr == MessageBoxResult.Yes)
                        {
                            await MainWindow.UpstreamManager.Update_Group_As_BuiltIn_Async(group.Name, false, true);
                        }
                    }
                }
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) // Is Ctrl Key Pressed
            {
                if (Keyboard.IsKeyDown(Key.Up)) MenuItem_Group_MoveUp_Click(null, null);
                else if (Keyboard.IsKeyDown(Key.Down)) MenuItem_Group_MoveDown_Click(null, null);
                else if (Keyboard.IsKeyDown(Key.Home)) MenuItem_Group_MoveToTop_Click(null, null);
                else if (Keyboard.IsKeyDown(Key.End)) MenuItem_Group_MoveToBottom_Click(null, null);
            }
            else if (e.Key == Key.Up)
            {
                if (dg.SelectedIndex > 0)
                {
                    dg.SelectedIndex--;
                    dg.ScrollIntoViewByIndex(dg.SelectedIndex);
                }
            }
            else if (e.Key == Key.Down)
            {
                if (dg.SelectedIndex < dg.Items.Count - 1)
                {
                    dg.SelectedIndex++;
                    dg.ScrollIntoViewByIndex(dg.SelectedIndex);
                }
            }
            else if (e.Key == Key.F2)
                MenuItem_Group_Rename_Click(null, null);

            dg.Focus();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow DGG_PreviewKeyDown: " + ex.Message);
        }
    }

    private void DGG_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (sender is not DataGrid dg) return;

            DataGridRow? row = dg.GetRowByMouseEvent(e);
            if (row != null) dg.SelectedIndex = row.GetIndex();

            bool isEnabled = dg.Items.Count > 0;
            MenuItem_Group_Rename.IsEnabled = isEnabled;
            MenuItem_Group_Remove.IsEnabled = isEnabled;
            MenuItem_Group_MoveUp.IsEnabled = isEnabled;
            MenuItem_Group_MoveDown.IsEnabled = isEnabled;
            MenuItem_Group_MoveToTop.IsEnabled = isEnabled;
            MenuItem_Group_MoveToBottom.IsEnabled = isEnabled;

            if (isEnabled)
            {
                bool isFirstItem = dg.SelectedIndex == 0;
                bool isLastItem = dg.SelectedIndex == dg.Items.Count - 1;

                if (isFirstItem)
                {
                    MenuItem_Group_MoveUp.IsEnabled = false;
                    MenuItem_Group_MoveToTop.IsEnabled = false;
                }

                if (isLastItem)
                {
                    MenuItem_Group_MoveDown.IsEnabled = false;
                    MenuItem_Group_MoveToBottom.IsEnabled = false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow DGG_PreviewMouseRightButtonDown: " + ex.Message);
        }
    }

    private async void DGG_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        await BindItemsAsync(DGG.SelectedIndex);
        await LoadSelectedGroupAsync();
    }

    private async Task CreateUpstreamItemColumns_Async(DataGrid dg)
    {
        try
        {
            UpstreamItem ui;
            dg.Columns.Clear();
            dg.AutoGenerateColumns = false;

            DataGridCheckBoxColumn c_Selected = new()
            {
                Header = '\u2714',
                Binding = new Binding(nameof(ui.IsSelected)),
                IsReadOnly = true,
                CanUserResize = false,
                ElementStyle = TryFindResource("DataGridCheckBoxColumn_ReadOnlyStyle") as Style,
                EditingElementStyle = TryFindResource("DataGridCheckBoxColumn_ReadOnlyStyle") as Style
            };
            dg.Columns.Add(c_Selected);

            DataGridTextColumn c_Remarks = new()
            {
                Header = "Remarks",
                Binding = new Binding(nameof(ui.RemarksStr)),
                IsReadOnly = true,
                CanUserResize = true,
                FontWeight = FontWeights.UltraLight
            };
            dg.Columns.Add(c_Remarks);

            DataGridTextColumn c_Protocol = new()
            {
                Header = "Protocol",
                Binding = new Binding(nameof(ui.ProtocolStr)),
                MinWidth = 90,
                IsReadOnly = true,
                CanUserResize = false
            };
            dg.Columns.Add(c_Protocol);

            DataGridTextColumn c_Security = new()
            {
                Header = "Security",
                Binding = new Binding(nameof(ui.SecurityStr)),
                MinWidth = 70,
                IsReadOnly = true,
                CanUserResize = false
            };
            dg.Columns.Add(c_Security);

            DataGridTextColumn c_Transport = new()
            {
                Header = "Transport",
                Binding = new Binding(nameof(ui.TransportStr)),
                MinWidth = 90,
                IsReadOnly = true,
                CanUserResize = false
            };
            dg.Columns.Add(c_Transport);

            DataGridTextColumn c_Status = new()
            {
                Header = "Status",
                Binding = new Binding(nameof(ui.StatusShortDescription)),
                MinWidth = 70,
                IsReadOnly = true,
                CanUserResize = false,
                CellStyle = TryFindResource("DataGridTextColumnStatus_Style") as Style
            };
            dg.Columns.Add(c_Status);

            DataGridTextColumn c_Stability = new()
            {
                Header = "Stability",
                Binding = new Binding(nameof(ui.StabilityStr)),
                IsReadOnly = true,
                CanUserResize = false
            };
            dg.Columns.Add(c_Stability);

            DataGridTextColumn c_Latency = new()
            {
                Header = "Latency",
                Binding = new Binding(nameof(ui.Latency)),
                IsReadOnly = true,
                CanUserResize = false
            };
            dg.Columns.Add(c_Latency);

            DataGridTextColumn c_Country = new()
            {
                Header = "Country",
                Binding = new Binding(nameof(ui.Country)),
                MinWidth = 90,
                IsReadOnly = true,
                CanUserResize = false
            };
            dg.Columns.Add(c_Country);

            DataGridTextColumn c_DLSpeed = new()
            {
                Header = "DL Speed",
                Binding = new Binding(nameof(ui.DLSpeedStr)),
                MinWidth = 90,
                IsReadOnly = true,
                CanUserResize = false
            };
            dg.Columns.Add(c_DLSpeed);

            DataGridTextColumn c_ULSpeed = new()
            {
                Header = "UL Speed",
                Binding = new Binding(nameof(ui.ULSpeedStr)),
                MinWidth = 90,
                IsReadOnly = true,
                CanUserResize = false
            };
            dg.Columns.Add(c_ULSpeed);

            // Set Remarks Column's Width To Avoid Blinking On Update Items
            c_Remarks.Width = dg.FillCustomColumn(1, 200);
            await Task.Delay(50);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow CreateUpstreamItemColumns_Async: " + ex.Message);
        }
    }

    private static void DGS_UpstreamServers_PreviewKeyDown(DataGrid dg, KeyEventArgs e)
    {
        try
        {
            e.Handled = true;
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) // Is Ctrl Key Pressed
            {
                if (Keyboard.IsKeyDown(Key.A))
                {
                    dg.SelectAll();
                }
            }
            else if (e.Key == Key.Up)
            {
                if (dg.SelectedIndex > 0)
                {
                    dg.SelectedIndex--;
                    dg.ScrollIntoViewByIndex(dg.SelectedIndex);
                }
            }
            else if (e.Key == Key.Down)
            {
                if (dg.SelectedIndex < dg.Items.Count - 1)
                {
                    dg.SelectedIndex++;
                    dg.ScrollIntoViewByIndex(dg.SelectedIndex);
                }
            }

            dg.Focus();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow DGS_UpstreamServers_PreviewKeyDown: " + ex.Message);
        }
    }

    private void DGS_UpstreamServers_SelectionChanged(DataGrid dg)
    {
        if (DGG.SelectedItem is not UpstreamGroup group) return;
        if (dg.SelectedItem is not UpstreamItem item) return;
        
        try
        {
            // Set Header
            string header = $"Group: \"{group.Name}\", Mode: \"{group.Mode}\" - Upstream No: {dg.SelectedIndex + 1}/{dg.Items.Count}";
            this.DispatchIt(() => ServersTitleGroupBox.Header = header);

            // Set Remarks And UrlOrJson To Manual Edit
            if (group.Mode == GroupMode.Custom)
            {
                this.DispatchIt(() =>
                {
                    CustomByManual_Remarks_TextBox.Text = item.ConfigInfo.Remarks;
                    CustomByManual_UrlOrJson_TextBox.Text = item.ConfigInfo.UrlOrJson;
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow DGS_UpstreamServers_SelectionChanged: " + ex.Message);
        }
    }

    private void SourceBrowseButton(TextBox textBox)
    {
        try
        {
            OpenFileDialog ofd = new()
            {
                Filter = "TXT Upstream Servers (*.txt)|*.txt",
                DefaultExt = ".txt",
                AddExtension = true,
                Multiselect = true,
                RestoreDirectory = true
            };

            bool? dr = ofd.ShowDialog(this);
            if (dr.HasValue && dr.Value)
            {
                List<string> fileList = new();
                string[] files = ofd.FileNames;
                for (int n = 0; n < files.Length; n++)
                {
                    string file = files[n];
                    if (File.Exists(Path.GetFullPath(file))) fileList.Add(file);
                }

                textBox.Text = fileList.ToString(NL);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow SourceBrowseButton: " + ex.Message);
        }
    }

    private void MenuItem_All_CopyToCustom_Handler(MenuItem menuItem_CopyToCustom, ObservableCollection<UpstreamItem> selectedItems)
    {
        try
        {
            if (DGG.SelectedItem is not UpstreamGroup group) return;
            List<string> customGroups = new();
            List<string> allGroups = MainWindow.UpstreamManager.Get_Group_Names(false);
            for (int n = 0; n < allGroups.Count; n++)
            {
                string groupName = allGroups[n];
                GroupMode mode = MainWindow.UpstreamManager.Get_GroupMode_ByName(groupName);
                if (mode == GroupMode.Custom && !group.Name.Equals(groupName)) customGroups.Add(groupName);
            }

            menuItem_CopyToCustom.Items.Clear();
            for (int n = 0; n < customGroups.Count; n++)
            {
                string groupName = customGroups[n];
                MenuItem subMenuItem = new()
                {
                    Header = groupName,
                    Tag = selectedItems
                };
                subMenuItem.Click += MenuItem_CopyToCustom_Click;
                menuItem_CopyToCustom.Items.Add(subMenuItem);
            }

            menuItem_CopyToCustom.IsEnabled = customGroups.Count > 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow MenuItem_All_CopyToCustom_Handler: " + ex.Message);
        }
    }

    private async void MenuItem_CopyToCustom_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem menuItem) return;
            if (menuItem.Header is not string toGroup) return;
            if (menuItem.Tag is not ObservableCollection<UpstreamItem> selectedItems) return;
            
            await MainWindow.UpstreamManager.Append_UpstreamItems_Async(toGroup, selectedItems, true);

            WpfToastDialog.Show(this, $"Copied To {toGroup}.", MessageBoxImage.None, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow MenuItem_CopyToCustom_Click: " + ex.Message);
        }
    }

    private bool IsClosing = false;
    private bool IsDisposed = false;

    private async void WpfWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            if (IsClosing && !IsDisposed)
            {
                e.Cancel = true;
                return;
            }

            if (!IsClosing)
            {
                e.Cancel = true;
                IsClosing = true;

                // === Start Custom Dispose/Actions
                // Stop Scan
                if (MainWindow.UpstreamManager.IsScanning)
                {
                    MainWindow.UpstreamManager.ScanServers(new UpstreamGroup(), null);
                    await Task.Run(async () =>
                    {
                        while (true)
                        {
                            this.DispatchIt(() => IsHitTestVisible = false);
                            NoGroupText_TextBlock.SetText("Stopping Scan...");
                            ServersTabControl.DispatchIt(() => ServersTabControl.SelectedIndex = 0);
                            await Task.Delay(100);
                            if (!MainWindow.UpstreamManager.IsScanning) break;
                        }
                    });
                }
                // Save
                await MainWindow.UpstreamManager.SaveAsync();
                // === End Custom Dispose/Actions

                GC.Collect();
                await Task.Delay(10);
                IsDisposed = true;
                try
                {
                    Close();
                }
                catch (Exception)
                {
                    await Task.Delay(10);
                    Close();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageUpstreamWindow WpfWindow_Closing: " + ex.Message);
        }
    }

}