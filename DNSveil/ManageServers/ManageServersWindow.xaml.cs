using Microsoft.Win32;
using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using MsmhToolsWpfClass;
using MsmhToolsWpfClass.Themes;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using static DNSveil.Logic.DnsServers.DnsServersManager;
using static DNSveil.Logic.DnsServers.EnumsAndStructs;
using GroupItem = DNSveil.Logic.DnsServers.EnumsAndStructs.GroupItem;

namespace DNSveil.ManageServers;

/// <summary>
/// Interaction logic for ManageServersWindow.xaml
/// </summary>
public partial class ManageServersWindow : WpfWindow
{
    private static readonly string NL = Environment.NewLine;
    private static readonly string LS = "      ";

    private static readonly int LimitGroupNameLength = 50;
    private static readonly char[] LimitGroupNameChars = "\\/:*?\"<>|".ToCharArray(); // new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };

    public static bool IsScanning { get; set; } = false;

    // Context Menu New Group
    private MenuItem MenuItem_Subscription = new();
    private MenuItem MenuItem_AnonymizedDNSCrypt = new();
    private MenuItem MenuItem_Fragment = new();
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
    private MenuItem MenuItem_Subscription_CopyDnsAddress = new();
    private MenuItem MenuItem_Subscription_CopyToCustom = new();
    private MenuItem MenuItem_Subscription_Scan = new();
    private ContextMenu ContextMenu_Subscription = new();

    // Context Menu AnonDNSCrypt
    private MenuItem MenuItem_AnonDNSCrypt_CopyDnsAddress = new();
    private MenuItem MenuItem_AnonDNSCrypt_CopyToCustom = new();
    private MenuItem MenuItem_AnonDNSCrypt_Scan = new();
    private ContextMenu ContextMenu_AnonDNSCrypt = new();

    // Context Menu FragmentDoH
    private MenuItem MenuItem_FragmentDoH_CopyDohAddress = new();
    private MenuItem MenuItem_FragmentDoH_RemoveDuplicates = new();
    private MenuItem MenuItem_FragmentDoH_DeleteItem = new();
    private MenuItem MenuItem_FragmentDoH_Scan = new();
    private ContextMenu ContextMenu_FragmentDoH = new();

    // Context Menu Custom
    private MenuItem MenuItem_Custom_CopyDnsAddress = new();
    private MenuItem MenuItem_Custom_CopyToCustom = new();
    private MenuItem MenuItem_Custom_RemoveDuplicates = new();
    private MenuItem MenuItem_Custom_DeleteItem = new();
    private MenuItem MenuItem_Custom_DeleteAll = new();
    private MenuItem MenuItem_Custom_Scan = new();
    private ContextMenu ContextMenu_Custom = new();

    public ManageServersWindow()
    {
        InitializeComponent();

        // Invariant Culture
        Info.SetCulture(CultureInfo.InvariantCulture);

        Flyout_Subscription_Options.IsOpen = false;
        Flyout_AnonDNSCrypt_Options.IsOpen = false;
        Flyout_FragmentDoH_Options.IsOpen = false;
        Flyout_Custom_Options.IsOpen = false;
    }

    private static int GetPreviousOrNextIndex(DataGrid dg)
    {
        int r = dg.SelectedIndex;
        try
        {
            if (r > 0) r--;
            else if (r < dg.Items.Count - 1) r++;
        }
        catch (Exception) { }
        return r;
    }

    private async Task LoadGroupsAsync(string? selectGroupByName)
    {
        try
        {
            // Read Groups
            List<GroupItem> groupItems = MainWindow.ServersManager.Get_GroupItems(false);

            int previousSelectedIndex = DGG.SelectedIndex;
            DGG.ItemsSource = groupItems;
            await Task.Delay(50);

            bool isGroupSelected = false;
            if (!string.IsNullOrEmpty(selectGroupByName))
            {
                for (int n = 0; n < DGG.Items.Count; n++)
                {
                    object? item = DGG.Items[n];
                    if (item is not GroupItem groupItem) continue;
                    if (groupItem.Name.Equals(selectGroupByName))
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

            DGG.UpdateColumnsWidthToAuto();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow LoadGroupsAsync By Name: " + ex.Message);
        }
    }

    private async Task LoadGroupsAsync(int selectGroupByIndex)
    {
        try
        {
            // Read Groups
            List<GroupItem> groupItems = MainWindow.ServersManager.Get_GroupItems(false);

            int previousSelectedIndex = DGG.SelectedIndex;
            DGG.ItemsSource = groupItems;
            await Task.Delay(50);

            if (DGG.Items.Count > 0)
            {
                if (0 <= selectGroupByIndex && selectGroupByIndex <= DGG.Items.Count - 1)
                    DGG.SelectedIndex = selectGroupByIndex;
                else
                    DGG.SelectedIndex = (previousSelectedIndex != -1 && previousSelectedIndex < DGG.Items.Count) ? previousSelectedIndex : 0;
            }

            DGG.UpdateColumnsWidthToAuto();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow LoadGroupsAsync By Index: " + ex.Message);
        }
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Hide ServersTabControl Header
            ServersTabControl.HideHeader();

            // Load Theme

            await Task.Run(async () =>
            {
                while (true)
                {
                    bool isBackgroundTaskWorking = MainWindow.ServersManager.IsBackgroundTaskWorking;
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
            string helpScan = $"You Really Don't Need To Scan Servers!{NL}The App Will Automatically Scan Them In The Background.{NL}You Can Adjust Scan Time In";
            Help_Subscription_ScanButton.Content = $"{helpScan} \"Subscription Options\".";
            Help_AnonDNSCrypt_ScanButton.Content = $"{helpScan} \"Anonymized DNSCrypt Options\".";
            Help_FragmentDoH_ScanButton.Content = $"{helpScan} \"Fragment DoH Options\".";
            Help_Custom_ScanButton.Content = $"{helpScan} \"Custom Options\".";

            // Set Max Size Of Import/Export
            Import_ListBox.MaxWidth = SystemParameters.PrimaryScreenWidth * 70 / 100;
            Import_ListBox.MaxHeight = SystemParameters.PrimaryScreenHeight * 70 / 100;
            Export_ListBox.MaxWidth = Import_ListBox.MaxWidth;
            Export_ListBox.MaxHeight = Import_ListBox.MaxHeight;

            // Update Toggles By Value
            Subscription_ToggleSourceByUrlByFile(SubscriptionSourceToggleSwitch.IsChecked);
            AnonDNSCrypt_RelayByUrlByFileToggleSwitch(AnonDNSCryptRelayByUrlByFileToggleSwitch.IsChecked);
            AnonDNSCrypt_TargetByUrlByFileToggleSwitch(AnonDNSCryptTargetByUrlByFileToggleSwitch.IsChecked);
            FragmentDoH_ToggleSourceByUrlByManual(FragmentDoHSourceByUrlByManualToggleSwitch.IsChecked);
            Custom_ToggleSourceByUrlByFileByManual(CustomSourceToggleSwitch.IsChecked);

            // Set Max Lines Of TextBoxes
            SubscriptionSourceTextBox.SetMaxLines(5, this);
            AnonDNSCryptRelayTextBox.SetMaxLines(2, this);
            AnonDNSCryptTargetTextBox.SetMaxLines(2, this);
            FragmentDoHSourceTextBox.SetMaxLines(5, this);
            CustomSourceTextBox.SetMaxLines(5, this);

            // Load Groups: DGG
            await LoadGroupsAsync(0);

            // Bind Data Source: DGS
            BindDataSource_Subscription();
            BindDataSource_AnonDNSCrypt();
            BindDataSource_FragmentDoH();
            BindDataSource_Custom();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow Window_Loaded: " + ex.Message);
        }
    }

    private void Window_ContentRendered(object sender, EventArgs e)
    {
        try
        {
            // Context Menu New Group
            MenuItem_Subscription = new()
            {
                Header = Get_GroupModeName(GroupMode.Subscription),
                Tag = GroupMode.Subscription
            };
            MenuItem_Subscription.Click -= MenuNewGroup_Click;
            MenuItem_Subscription.Click += MenuNewGroup_Click;

            MenuItem_AnonymizedDNSCrypt = new()
            {
                Header = Get_GroupModeName(GroupMode.AnonymizedDNSCrypt),
                Tag = GroupMode.AnonymizedDNSCrypt
            };
            MenuItem_AnonymizedDNSCrypt.Click -= MenuNewGroup_Click;
            MenuItem_AnonymizedDNSCrypt.Click += MenuNewGroup_Click;

            MenuItem_Fragment = new()
            {
                Header = Get_GroupModeName(GroupMode.FragmentDoH),
                Tag = GroupMode.FragmentDoH
            };
            MenuItem_Fragment.Click -= MenuNewGroup_Click;
            MenuItem_Fragment.Click += MenuNewGroup_Click;

            MenuItem_Custom = new()
            {
                Header = Get_GroupModeName(GroupMode.Custom),
                Tag = GroupMode.Custom
            };
            MenuItem_Custom.Click -= MenuNewGroup_Click;
            MenuItem_Custom.Click += MenuNewGroup_Click;

            ContextMenu_NewGroup = new();
            ContextMenu_NewGroup.Items.Add(MenuItem_Subscription);
            ContextMenu_NewGroup.Items.Add(MenuItem_AnonymizedDNSCrypt);
            ContextMenu_NewGroup.Items.Add(MenuItem_Fragment);
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
            MenuItem_Subscription_CopyDnsAddress = new()
            {
                Header = "Copy DNS Address To Clipboard"
            };
            MenuItem_Subscription_CopyDnsAddress.Click -= MenuItem_Subscription_CopyDnsAddress_Click;
            MenuItem_Subscription_CopyDnsAddress.Click += MenuItem_Subscription_CopyDnsAddress_Click;

            MenuItem_Subscription_CopyToCustom = new()
            {
                Header = "Copy Selected DNS To..."
            };

            MenuItem_Subscription_Scan = new()
            {
                Header = "Scan Selected Servers"
            };
            MenuItem_Subscription_Scan.Click -= MenuItem_Subscription_Scan_Click;
            MenuItem_Subscription_Scan.Click += MenuItem_Subscription_Scan_Click;

            ContextMenu_Subscription = new();
            ContextMenu_Subscription.Items.Add(MenuItem_Subscription_CopyDnsAddress);
            ContextMenu_Subscription.Items.Add(MenuItem_Subscription_CopyToCustom);
            ContextMenu_Subscription.Items.Add(MenuItem_Subscription_Scan);
            ContextMenu_Subscription.Closed += (s, e) =>
            {
                foreach (MenuItem menuItem in MenuItem_Subscription_CopyToCustom.Items)
                    menuItem.Click -= MenuItem_CopyToCustom_Click;
            };

            // Context Menu AnonDNSCrypt
            MenuItem_AnonDNSCrypt_CopyDnsAddress = new()
            {
                Header = "Copy DNS Address To Clipboard"
            };
            MenuItem_AnonDNSCrypt_CopyDnsAddress.Click -= MenuItem_AnonDNSCrypt_CopyDnsAddress_Click;
            MenuItem_AnonDNSCrypt_CopyDnsAddress.Click += MenuItem_AnonDNSCrypt_CopyDnsAddress_Click;

            MenuItem_AnonDNSCrypt_CopyToCustom = new()
            {
                Header = "Copy Selected DNS To..."
            };

            MenuItem_AnonDNSCrypt_Scan = new()
            {
                Header = "Scan Selected Servers"
            };
            MenuItem_AnonDNSCrypt_Scan.Click -= MenuItem_AnonDNSCrypt_Scan_Click;
            MenuItem_AnonDNSCrypt_Scan.Click += MenuItem_AnonDNSCrypt_Scan_Click;

            ContextMenu_AnonDNSCrypt = new();
            ContextMenu_AnonDNSCrypt.Items.Add(MenuItem_AnonDNSCrypt_CopyDnsAddress);
            ContextMenu_AnonDNSCrypt.Items.Add(MenuItem_AnonDNSCrypt_CopyToCustom);
            ContextMenu_AnonDNSCrypt.Items.Add(MenuItem_AnonDNSCrypt_Scan);
            ContextMenu_AnonDNSCrypt.Closed += (s, e) =>
            {
                foreach (MenuItem menuItem in MenuItem_AnonDNSCrypt_CopyToCustom.Items)
                    menuItem.Click -= MenuItem_CopyToCustom_Click;
            };

            // Context Menu FragmentDoH
            MenuItem_FragmentDoH_CopyDohAddress = new()
            {
                Header = "Copy DoH Address And IP To Clipboard"
            };
            MenuItem_FragmentDoH_CopyDohAddress.Click -= MenuItem_FragmentDoH_CopyDohAddress_Click;
            MenuItem_FragmentDoH_CopyDohAddress.Click += MenuItem_FragmentDoH_CopyDohAddress_Click;

            MenuItem_FragmentDoH_RemoveDuplicates = new()
            {
                Header = "Remove Duplicates"
            };
            MenuItem_FragmentDoH_RemoveDuplicates.Click -= MenuItem_FragmentDoH_RemoveDuplicates_Click;
            MenuItem_FragmentDoH_RemoveDuplicates.Click += MenuItem_FragmentDoH_RemoveDuplicates_Click;

            MenuItem_FragmentDoH_DeleteItem = new()
            {
                Header = "Delete Selected Servers"
            };
            MenuItem_FragmentDoH_DeleteItem.Click -= MenuItem_FragmentDoH_DeleteItem_Click;
            MenuItem_FragmentDoH_DeleteItem.Click += MenuItem_FragmentDoH_DeleteItem_Click;

            MenuItem_FragmentDoH_Scan = new()
            {
                Header = "Scan Selected Servers"
            };
            MenuItem_FragmentDoH_Scan.Click -= MenuItem_FragmentDoH_Scan_Click;
            MenuItem_FragmentDoH_Scan.Click += MenuItem_FragmentDoH_Scan_Click;

            ContextMenu_FragmentDoH = new();
            ContextMenu_FragmentDoH.Items.Add(MenuItem_FragmentDoH_CopyDohAddress);
            ContextMenu_FragmentDoH.Items.Add(MenuItem_FragmentDoH_RemoveDuplicates);
            ContextMenu_FragmentDoH.Items.Add(MenuItem_FragmentDoH_DeleteItem);
            ContextMenu_FragmentDoH.Items.Add(MenuItem_FragmentDoH_Scan);

            // Context Menu Custom
            MenuItem_Custom_CopyDnsAddress = new()
            {
                Header = "Copy DNS Address To Clipboard"
            };
            MenuItem_Custom_CopyDnsAddress.Click -= MenuItem_Custom_CopyDnsAddress_Click;
            MenuItem_Custom_CopyDnsAddress.Click += MenuItem_Custom_CopyDnsAddress_Click;

            MenuItem_Custom_CopyToCustom = new()
            {
                Header = "Copy Selected DNS To..."
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
            ContextMenu_Custom.Items.Add(MenuItem_Custom_CopyDnsAddress);
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

            MainWindow.ServersManager.OnBackgroundUpdateReceived -= ServersManager_OnBackgroundUpdateReceived;
            MainWindow.ServersManager.OnBackgroundUpdateReceived += ServersManager_OnBackgroundUpdateReceived;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow Window_ContentRendered: " + ex.Message);
        }
    }

    private async void ServersManager_OnBackgroundUpdateReceived(object? sender, BackgroundWorkerEventArgs e)
    {
        //Debug.WriteLine("On Background Update Received");
        await UpdateUIByBackgroundWorkerAsync(e);
    }
    
    private async Task UpdateUIByBackgroundWorkerAsync(BackgroundWorkerEventArgs bw)
    {
        try
        {
            if (DGG.SelectedItem is not GroupItem groupItem) return;
            IsScanning = bw.IsWorking;
            await Import_FlyoutOverlay.CloseFlyAsync();

            if (bw.GroupItem.Mode == GroupMode.Subscription && bw.GroupItem.Name.Equals(groupItem.Name))
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
                    await LoadSelectedGroupAsync(true); // Refresh
                    if (bw.LastIndex != -1) await DGS_Subscription.ScrollIntoViewAsync(bw.LastIndex); // Scroll
                }
            }
            else if (bw.GroupItem.Mode == GroupMode.AnonymizedDNSCrypt && bw.GroupItem.Name.Equals(groupItem.Name))
            {
                // Scan AnonymizedDNSCrypt
                ChangeControlsState_AnonDNSCrypt(!bw.IsWorking);
                this.DispatchIt(() =>
                {
                    if (bw.IsWorking)
                    {
                        DGG.IsHitTestVisible = !bw.IsWorking;
                        DGS_AnonDNSCrypt.IsHitTestVisible = !bw.IsWorking;
                        AnonDNSCryptRelayTextBox.IsEnabled = !bw.IsWorking;
                        AnonDNSCryptTargetTextBox.IsEnabled = !bw.IsWorking;
                    }
                    else
                    {
                        DGG.IsHitTestVisible = !bw.IsWorking;
                        DGS_AnonDNSCrypt.IsHitTestVisible = !bw.IsWorking;
                        AnonDNSCryptRelayTextBox.IsEnabled = AnonDNSCryptRelayByUrlByFileToggleSwitch.IsChecked.HasValue && !AnonDNSCryptRelayByUrlByFileToggleSwitch.IsChecked.Value;
                        AnonDNSCryptTargetTextBox.IsEnabled = AnonDNSCryptTargetByUrlByFileToggleSwitch.IsChecked.HasValue && !AnonDNSCryptTargetByUrlByFileToggleSwitch.IsChecked.Value;
                    }

                    AnonDNSCryptScanButton.IsEnabled = bw.ButtonEnable;
                    AnonDNSCryptScanButton.Content = bw.ButtonText;
                    AnonDNSCryptScanInfoTextBlock.Text = bw.Description;

                    if (bw.ButtonEnable)
                    {
                        AnonDNSCryptScanProgressBar.Minimum = bw.ProgressMin;
                        AnonDNSCryptScanProgressBar.Maximum = bw.ProgressMax;
                        AnonDNSCryptScanProgressBar.Value = bw.ProgressValue;
                    }
                });

                if (bw.ButtonEnable)
                {
                    AnonDNSCryptScanProgressBarTextBlock.Clear();
                    AnonDNSCryptScanProgressBarTextBlock.AppendText($"{bw.ProgressPercentage}% - {bw.ProgressValue} Of {bw.ProgressMax}{LS}{LS}Online Servers: ", null, $"{bw.OnlineServers}", Brushes.Orange);
                    AnonDNSCryptScanProgressBarTextBlock.AppendText($"{LS}{LS}Average Latency: ", null, $"{bw.AverageLatencyMS}", Brushes.Orange, "ms", null);
                }

                if (bw.ParallelLatencyMS > 1000 || !bw.IsWorking) // Smooth UI
                {
                    await LoadSelectedGroupAsync(true); // Refresh
                    if (bw.LastIndex != -1) await DGS_AnonDNSCrypt.ScrollIntoViewAsync(bw.LastIndex); // Scroll
                }
            }
            else if (bw.GroupItem.Mode == GroupMode.FragmentDoH && bw.GroupItem.Name.Equals(groupItem.Name))
            {
                // Scan FragmentDoH
                ChangeControlsState_FragmentDoH(!bw.IsWorking);
                this.DispatchIt(() =>
                {
                    FragmentDoHScanButton.IsEnabled = bw.ButtonEnable;
                    FragmentDoHScanButton.Content = bw.ButtonText;
                    FragmentDoHScanInfoTextBlock.Text = bw.Description;

                    if (bw.ButtonEnable)
                    {
                        FragmentDoHScanProgressBar.Minimum = bw.ProgressMin;
                        FragmentDoHScanProgressBar.Maximum = bw.ProgressMax;
                        FragmentDoHScanProgressBar.Value = bw.ProgressValue;
                    }
                });

                if (bw.ButtonEnable)
                {
                    FragmentDoHScanProgressBarTextBlock.Clear();
                    FragmentDoHScanProgressBarTextBlock.AppendText($"{bw.ProgressPercentage}% - {bw.ProgressValue} Of {bw.ProgressMax}{LS}{LS}Online Servers: ", null, $"{bw.OnlineServers}", Brushes.Orange);
                    FragmentDoHScanProgressBarTextBlock.AppendText($"{LS}{LS}Average Latency: ", null, $"{bw.AverageLatencyMS}", Brushes.Orange, "ms", null);
                }

                if (bw.ParallelLatencyMS > 1000 || !bw.IsWorking) // Smooth UI
                {
                    await LoadSelectedGroupAsync(true); // Refresh
                    if (bw.LastIndex != -1) await DGS_FragmentDoH.ScrollIntoViewAsync(bw.LastIndex); // Scroll
                }
            }
            else if (bw.GroupItem.Mode == GroupMode.Custom && bw.GroupItem.Name.Equals(groupItem.Name))
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
                    await LoadSelectedGroupAsync(true); // Refresh
                    if (bw.LastIndex != -1) await DGS_Custom.ScrollIntoViewAsync(bw.LastIndex); // Scroll
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow UpdateUIByBackgroundWorker: " + ex.Message);
        }
    }

    private void NewGroupButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (IsScanning)
        {
            WpfToastDialog.Show(this, "Can't Do This While Scanning.", MessageBoxImage.Stop, 2);
            return;
        }

        ContextMenu_NewGroup.IsOpen = true;
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
            bool imported = await MainWindow.ServersManager.Reset_BuiltIn_Groups_Async(true);
            IsHitTestVisible = true;

            if (imported)
            {
                await LoadGroupsAsync(0); // Refresh

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
            Debug.WriteLine("ManageServersWindow ResetBuiltInButton_Click: " + ex.Message);
            IsEnabled = true;
        }
    }

    private async void Import_FlyoutOverlay_FlyoutChanged(object sender, WpfFlyoutPopup.FlyoutChangedEventArgs e)
    {
        try
        {
            if (Export_FlyoutOverlay.IsOpen) await Export_FlyoutOverlay.CloseFlyAsync();

            // Clear Import List
            if (e.IsFlyoutOpen)
            {
                Import_ListBox.ItemsSource = new List<GroupItem>();
                Import_ListBox.Tag = new List<XElement>();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow Import_FlyoutOverlay_FlyoutChanged: " + ex.Message);
        }
    }

    private async void ImportBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ImportBrowseButton.IsEnabled = false;

            OpenFileDialog ofd = new()
            {
                Filter = "DNSveil Servers (*.dvls)|*.dvls|DNSveil Servers (*.xml)|*.xml",
                DefaultExt = ".dvls",
                AddExtension = true,
                Multiselect = false,
                RestoreDirectory = true
            };

            bool? dr = ofd.ShowDialog(this);
            if (dr.HasValue && dr.Value)
            {
                string filePath = ofd.FileName;
                string xmlContent = await File.ReadAllTextAsync(filePath);
                if (!XmlTool.IsValid(xmlContent))
                {
                    string msg = $"{Path.GetExtension(filePath).ToUpperInvariant()} File Is Not Valid!";
                    WpfMessageBox.Show(this, msg, "Not Valid", MessageBoxButton.OK, MessageBoxImage.Error);
                    ImportBrowseButton.IsEnabled = true;
                    return;
                }

                XDocument? importedXML = XDocument.Load(filePath);
                if (importedXML.Root == null)
                {
                    ImportBrowseButton.IsEnabled = true;
                    return;
                }
                var groupElements = importedXML.Root.Elements();

                if (!groupElements.Any())
                {
                    string msg = $"{Path.GetExtension(filePath).ToUpperInvariant()} File Has No Groups!";
                    WpfMessageBox.Show(this, msg, "No Groups", MessageBoxButton.OK, MessageBoxImage.Stop);
                    ImportBrowseButton.IsEnabled = true;
                    return;
                }

                // Remove Groups Without Name Element Or Empty Name
                List<XElement> groupElementList = new();
                foreach (XElement groupElement in groupElements)
                {
                    GroupItem groupItem = Get_GroupItem(groupElement); // Get Name
                    if (!string.IsNullOrWhiteSpace(groupItem.Name)) groupElementList.Add(groupElement);
                }

                importedXML = null;
                if (groupElementList.Count == 0)
                {
                    string msg = $"There Is No Valid Groups!";
                    WpfMessageBox.Show(this, msg, "No Groups", MessageBoxButton.OK, MessageBoxImage.Stop);
                    ImportBrowseButton.IsEnabled = true;
                    return;
                }

                // Get GroupItems To List
                List<GroupItem> groupItems = new();
                for (int n = 0; n < groupElementList.Count; n++)
                {
                    XElement groupElement = groupElementList[n];
                    GroupItem groupItem = new();
                    XElement? nameElement = groupElement.Element(nameof(groupItem.Name));
                    if (nameElement != null)
                    {
                        groupItem.Selected = true;
                        groupItem.Name = nameElement.Value;
                        groupItems.Add(groupItem);
                    }
                }

                // Open Fly
                await Import_FlyoutOverlay.OpenFlyAsync();

                Import_ListBox.ItemsSource = groupItems;
                Import_ListBox.Tag = groupElementList;
                ImportBrowseButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow ImportBrowseButton_Click: " + ex.Message);
        }
    }

    private void Import_CheckBox_CheckedUnchecked(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not CheckBox cb) return;
            if (!cb.IsChecked.HasValue) return;
            if (Import_ListBox.ItemsSource is not List<GroupItem> groupItems) return;

            // Modify ItemsSource Based On User Selection
            for (int n = 0; n < groupItems.Count; n++)
            {
                GroupItem groupItem = groupItems[n];
                if (groupItem.Name.Equals(cb.Content.ToString()))
                {
                    groupItem.Selected = cb.IsChecked.Value;
                    groupItems[n] = groupItem;
                    break;
                }
            }

            Import_ListBox.ItemsSource = groupItems;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow Import_CheckBox_CheckedUnchecked: " + ex.Message);
        }
    }

    private async void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Import_ListBox.ItemsSource is not List<GroupItem> groupItems) return;
            if (Import_ListBox.Tag is not List<XElement> groupElementList) return;

            ImportButton.IsEnabled = false;
            
            if (groupItems.Count == 0)
            {
                WpfToastDialog.Show(this, "Browse Your Backup File.", MessageBoxImage.Stop, 3);
                ImportButton.IsEnabled = true;
                return;
            }

            List<string> groupNamesToImport = new();
            for (int n = 0; n < groupItems.Count; n++)
            {
                GroupItem groupItem = groupItems[n];
                if (groupItem.Selected) groupNamesToImport.Add(groupItem.Name);
            }

            if (groupNamesToImport.Count == 0)
            {
                WpfToastDialog.Show(this, "Select At Least One Group To Import.", MessageBoxImage.Stop, 3);
                ImportButton.IsEnabled = true;
                return;
            }
            
            string lastImportedGroupName = string.Empty;
            for (int n = 0; n < groupElementList.Count; n++)
            {
                XElement groupElement = groupElementList[n];
                GroupItem groupItem = Get_GroupItem(groupElement); // Get Name
                if (groupNamesToImport.IsContain(groupItem.Name))
                {
                    lastImportedGroupName = await MainWindow.ServersManager.Add_Group_Async(groupElement, false, false);
                }
            }

            if (!string.IsNullOrEmpty(lastImportedGroupName))
            {
                await MainWindow.ServersManager.SaveAsync(); // Save
                await LoadGroupsAsync(lastImportedGroupName); // Refresh

                ImportButton.IsEnabled = true;
                await Import_FlyoutOverlay.CloseFlyAsync();

                string msg = "Selected Groups Imported Successfully.";
                WpfMessageBox.Show(this, msg, "Imported", MessageBoxButton.OK);
            }
            else
            {
                ImportButton.IsEnabled = true;
                await Import_FlyoutOverlay.CloseFlyAsync();

                string msg = "Group Name Is Empty.";
                WpfMessageBox.Show(this, msg, "Something Went Wrong!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow ImportButton_Click: " + ex.Message);
        }
    }

    private async void Export_FlyoutOverlay_FlyoutChanged(object sender, WpfFlyoutPopup.FlyoutChangedEventArgs e)
    {
        try
        {
            if (Import_FlyoutOverlay.IsOpen) await Import_FlyoutOverlay.CloseFlyAsync();

            if (e.IsFlyoutOpen)
            {
                List<GroupItem> groupItems = MainWindow.ServersManager.Get_GroupItems(false);
                Export_ListBox.ItemsSource = groupItems;
                Export_ListBox.Tag = groupItems;
                Export_ListBox.SelectedIndex = -1;
            }
            else
            {
                Export_ListBox.ItemsSource = null;
                Export_ListBox.Tag = null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow Export_FlyoutOverlay_FlyoutChanged: " + ex.Message);
        }
    }

    private void Export_CheckBox_CheckedUnchecked(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not CheckBox cb) return;
            if (!cb.IsChecked.HasValue) return;
            if (Export_ListBox.ItemsSource is not List<GroupItem> groupItems) return;

            // Modify Tag Based On User Selection
            for (int n = 0; n < groupItems.Count; n++)
            {
                GroupItem groupItem = groupItems[n];
                if (groupItem.Name.Equals(cb.Content.ToString()))
                {
                    groupItem.Selected = cb.IsChecked.Value;
                    groupItems[n] = groupItem;
                    break;
                }
            }

            Export_ListBox.Tag = groupItems;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow Export_CheckBox_CheckedUnchecked: " + ex.Message);
        }
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Export_ListBox.Tag is not List<GroupItem> groupItems) return;
            ExportButton.IsEnabled = false;
            List<string> groupNamesToExport = new();
            for (int n = 0; n < groupItems.Count; n++)
            {
                GroupItem groupItem = groupItems[n];
                if (groupItem.Selected) groupNamesToExport.Add(groupItem.Name);
            }

            if (groupNamesToExport.Count == 0)
            {
                WpfToastDialog.Show(this, "Select At Least One Group To Export.", MessageBoxImage.Stop, 3);
                ExportButton.IsEnabled = true;
                return;
            }

            XDocument? xDoc_Export = MainWindow.ServersManager.Export_Groups(groupNamesToExport);
            if (xDoc_Export == null || xDoc_Export.Root == null)
            {
                WpfToastDialog.Show(this, "ERROR: Creating XDocument, Root Is NULL.", MessageBoxImage.Error, 3);
                ExportButton.IsEnabled = true;
                return;
            }

            SaveFileDialog sfd = new()
            {
                Filter = "DNSveil Servers (*.dvls)|*.dvls|DNSveil Servers (*.xml)|*.xml",
                DefaultExt = ".dvls",
                AddExtension = true,
                RestoreDirectory = true,
                FileName = $"DNSveil_Servers_{DateTime.Now:yyyy.MM.dd-HH.mm.ss}"
            };

            bool? dr = sfd.ShowDialog(this);
            if (dr.HasValue && dr.Value)
            {
                try
                {
                    await xDoc_Export.SaveAsync(sfd.FileName);
                    await Export_FlyoutOverlay.CloseFlyAsync();

                    string msg = "Selected Groups Exported Successfully.";
                    WpfMessageBox.Show(this, msg, "Exported", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    WpfMessageBox.Show(this, ex.Message, "Something Went Wrong!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            xDoc_Export = null;
            ExportButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow ExportButton_Click: " + ex.Message);
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
            bool dialogResult = WpfInputBox.Show(this, ref newGroupName, $"New \"{Get_GroupModeName(groupMode)}\" Group Name:", "Create Group", false);
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

            List<string> groupNames = MainWindow.ServersManager.Get_Group_Names(false);
            if (groupNames.IsContain(newGroupName))
            {
                string msg = $"\"{newGroupName}\" Is Already Exist, Choose Another Name.";
                WpfMessageBox.Show(this, msg);
                return;
            }

            await MainWindow.ServersManager.Create_Group_Async(newGroupName, groupMode);
            await LoadGroupsAsync(newGroupName);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow MenuNewGroup_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;

            // Get New Name
            string newGroupName = groupItem.Name;
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

            if (newGroupName.Equals(groupItem.Name)) return;

            List<string> groupNames = MainWindow.ServersManager.Get_Group_Names(false);
            if (groupNames.IsContain(newGroupName))
            {
                string msg = $"\"{newGroupName}\" Is Already Exist, Choose Another Name.";
                WpfMessageBox.Show(this, msg);
                return;
            }

            // Get Old Name
            await MainWindow.ServersManager.Rename_Group_Async(groupItem.Name, newGroupName);
            await LoadGroupsAsync(newGroupName);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow MenuItem_Group_Rename_Click: " + ex.Message);
        }
    }

    private async void MenuItem_Group_Remove_Click(object? sender, RoutedEventArgs? e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Do This While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            if (DGG.SelectedItem is not GroupItem groupItem) return;

            string msg = $"Deleting \"{groupItem.Name}\" Group...{NL}Continue?";
            MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbr != MessageBoxResult.Yes) return;

            int nextIndex = GetPreviousOrNextIndex(DGG); // Get Next Index
            await MainWindow.ServersManager.Remove_Group_Async(groupItem.Name); // Remove
            await LoadGroupsAsync(nextIndex); // Refresh
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow MenuItem_Group_Remove_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;

            // Get Previous Index
            int index = -1;
            if (DGG.SelectedIndex > 0) index = DGG.SelectedIndex - 1;
            if (index != -1)
            {
                await MainWindow.ServersManager.Move_Group_Async(groupItem.Name, index); // Move
                await LoadGroupsAsync(groupItem.Name); // Refresh
                Debug.WriteLine("MoveUp");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow MenuItem_Group_MoveUp_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;

            // Get Next Index
            int index = -1;
            if (DGG.SelectedIndex < DGG.Items.Count - 1) index = DGG.SelectedIndex + 1;
            if (index != -1)
            {
                await MainWindow.ServersManager.Move_Group_Async(groupItem.Name, index); // Move
                await LoadGroupsAsync(groupItem.Name); // Refresh
                Debug.WriteLine("MoveDown");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow MenuItem_Group_MoveDown_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;

            if (DGG.Items.Count > 0 && DGG.SelectedIndex != 0)
            {
                await MainWindow.ServersManager.Move_Group_Async(groupItem.Name, 0); // Move
                await LoadGroupsAsync(groupItem.Name); // Refresh
                Debug.WriteLine("MoveToTop");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow MenuItem_Group_MoveToTop_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not GroupItem groupItem) return;

            if (DGG.Items.Count > 0 && DGG.SelectedIndex != DGG.Items.Count - 1)
            {
                await MainWindow.ServersManager.Move_Group_Async(groupItem.Name, DGG.Items.Count - 1); // Move
                await LoadGroupsAsync(groupItem.Name); // Refresh
                Debug.WriteLine("MoveToBottom");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow MenuItem_Group_MoveToBottom_Click: " + ex.Message);
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
                // Delete All DNS Items
                if (e.Key == Key.Delete) // + Del
                {
                    if (DGG.SelectedItem is GroupItem groupItem)
                    {
                        string msg = $"Deleting All DNS Items/Targets/Relays...{NL}Continue?";
                        MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Delete All", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (mbr == MessageBoxResult.Yes)
                        {
                            if (groupItem.Mode == GroupMode.AnonymizedDNSCrypt)
                            {
                                await MainWindow.ServersManager.Clear_AnonDNSCrypt_RelayItems_Async(groupItem.Name, false);
                                await MainWindow.ServersManager.Clear_AnonDNSCrypt_TargetItems_Async(groupItem.Name, false);
                            }
                            await MainWindow.ServersManager.Clear_DnsItems_Async(groupItem.Name, true);
                            await LoadSelectedGroupAsync(true); // Refresh
                        }
                    }
                }
                // Set Group As Built-In
                else if (e.Key == Key.B)
                {
                    if (DGG.SelectedItem is GroupItem groupItem)
                    {
                        string msg = $"Setting Group \"{groupItem.Name}\" As Built-In...{NL}Continue?";
                        MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Set As Built-In", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (mbr == MessageBoxResult.Yes)
                        {
                            await MainWindow.ServersManager.Update_GroupItem_BuiltIn_Async(groupItem.Name, true, true);
                            await LoadSelectedGroupAsync(true); // Refresh
                        }
                    }
                }
                // Set Group As User
                else if (e.Key == Key.U)
                {
                    if (DGG.SelectedItem is GroupItem groupItem)
                    {
                        string msg = $"Setting Group \"{groupItem.Name}\" As User...{NL}Continue?";
                        MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Set As User", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (mbr == MessageBoxResult.Yes)
                        {
                            await MainWindow.ServersManager.Update_GroupItem_BuiltIn_Async(groupItem.Name, false, true);
                            await LoadSelectedGroupAsync(true); // Refresh
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
            Debug.WriteLine("ManageServersWindow DGG_PreviewKeyDown: " + ex.Message);
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
            Debug.WriteLine("ManageServersWindow DGG_PreviewMouseRightButtonDown: " + ex.Message);
        }
    }

    private async Task LoadSelectedGroupAsync(bool loadDnsItems)
    {
        try
        {
            if (DGG.SelectedItem is not GroupItem groupItem)
            {
                ServersTabControl.SelectedIndex = 0;
                return;
            }

            // Set Header
            string header = $"Group: \"{groupItem.Name}\", Mode: \"{Get_GroupModeName(groupItem.Mode)}\"";
            this.DispatchIt(() => ServersTitleGroupBox.Header = header);

            // Select Tab
            this.DispatchIt(() =>
            {
                ServersTabControl.SelectedIndex = groupItem.Mode switch
                {
                    GroupMode.Subscription => 1,
                    GroupMode.AnonymizedDNSCrypt => 2,
                    GroupMode.FragmentDoH => 3,
                    GroupMode.Custom => 4,
                    _ => 0
                };
            });

            // Get Green Brush
            Brush greenBrush = AppTheme.GetBrush(AppTheme.MediumSeaGreenBrush);

            if (groupItem.Mode == GroupMode.Subscription)
            {
                // Get Group Settings
                GroupSettings groupSettings = MainWindow.ServersManager.Get_GroupSettings(groupItem.Name);
                this.DispatchIt(() =>
                {
                    Subscription_EnableGroup_ToggleSwitch.IsChecked = groupSettings.Enabled;
                    Subscription_Settings_LookupDomain_TextBox.Text = groupSettings.LookupDomain;
                    Subscription_Settings_TimeoutSec_NumericUpDown.Value = groupSettings.TimeoutSec;
                    Subscription_Settings_ParallelSize_NumericUpDown.Value = groupSettings.ParallelSize;
                    Subscription_Settings_BootstrapIP_TextBox.Text = groupSettings.BootstrapIP.ToString();
                    Subscription_Settings_BootstrapPort_NumericUpDown.Value = groupSettings.BootstrapPort;
                    Subscription_Settings_AllowInsecure_ToggleSwitch.IsChecked = groupSettings.AllowInsecure;

                    Flyout_Subscription_Source.IsEnabled = groupSettings.Enabled;
                    Flyout_Subscription_Options.IsEnabled = groupSettings.Enabled;
                    Flyout_Subscription_DnsServers.IsEnabled = groupSettings.Enabled;
                    Flyout_Subscription_Scan.IsEnabled = groupSettings.Enabled;
                });

                // Get URLs
                List<string> subs = MainWindow.ServersManager.Get_Source_URLs(groupItem.Name);
                this.DispatchIt(() => SubscriptionSourceTextBox.Text = subs.ToString(NL));

                // Get DateTime
                LastAutoUpdate dateTime = MainWindow.ServersManager.Get_LastAutoUpdate(groupItem.Name);
                SubscriptionUpdateSourceTextBlock.Clear();
                SubscriptionUpdateSourceTextBlock.AppendText("Last Update: ", null, $"{dateTime.LastUpdateSource:yyyy/MM/dd HH:mm:ss}", greenBrush);
                SubscriptionScanServersTextBlock.Clear();
                SubscriptionScanServersTextBlock.AppendText("Last Scan: ", null, $"{dateTime.LastScanServers:yyyy/MM/dd HH:mm:ss}", greenBrush);

                // Get Options
                SubscriptionOptions options = MainWindow.ServersManager.Get_Subscription_Options(groupItem.Name);
                this.DispatchIt(() =>
                {
                    // AutoUpdate
                    SubscriptionUpdateSourceNumericUpDown.Value = options.AutoUpdate.UpdateSource;
                    SubscriptionScanServersNumericUpDown.Value = options.AutoUpdate.ScanServers;

                    // FilterByProtocols
                    SubscriptionFilter_UDP_CheckBox.IsChecked = options.FilterByProtocols.UDP;
                    SubscriptionFilter_TCP_CheckBox.IsChecked = options.FilterByProtocols.TCP;
                    SubscriptionFilter_DNSCrypt_CheckBox.IsChecked = options.FilterByProtocols.DnsCrypt;
                    SubscriptionFilter_AnonymizedDNSCrypt_CheckBox.IsChecked = options.FilterByProtocols.AnonymizedDNSCrypt;
                    SubscriptionFilter_DoT_CheckBox.IsChecked = options.FilterByProtocols.DoT;
                    SubscriptionFilter_DoH_CheckBox.IsChecked = options.FilterByProtocols.DoH;
                    SubscriptionFilter_ODoH_CheckBox.IsChecked = options.FilterByProtocols.ObliviousDoH;
                    SubscriptionFilter_DoQ_CheckBox.IsChecked = options.FilterByProtocols.DoQ;

                    // FilterByProperties
                    SubscriptionFilter_Google_CheckBox.IsChecked = DnsFilterToBool(options.FilterByProperties.GoogleSafeSearch);
                    SubscriptionFilter_Bing_CheckBox.IsChecked = DnsFilterToBool(options.FilterByProperties.BingSafeSearch);
                    SubscriptionFilter_Youtube_CheckBox.IsChecked = DnsFilterToBool(options.FilterByProperties.YoutubeRestricted);
                    SubscriptionFilter_Adult_CheckBox.IsChecked = DnsFilterToBool(options.FilterByProperties.AdultBlocked);
                });

                // Get DnsItems Info
                DnsItemsInfo info = MainWindow.ServersManager.Get_DnsItems_Info(groupItem.Name);
                SubscriptionSource_Info_TextBlock.Clear();
                SubscriptionSource_Info_TextBlock.AppendText("Total Servers: ", null, $"{info.TotalServers}", Brushes.Orange);
                SubscriptionFilter_Info_TextBlock.Clear();
                SubscriptionFilter_Info_TextBlock.AppendText("Total Servers: ", null, $"{info.TotalServers}", Brushes.Orange, $",{LS}Online Servers: ", null, $"{info.OnlineServers}", Brushes.Orange);
                SubscriptionFilter_Info_TextBlock.AppendText($",{LS}Selected Servers: ", null, $"{info.SelectedServers}", Brushes.Orange, $",{LS}Average Latency: ", null, $"{info.AverageLatency}", Brushes.Orange, "ms", null);

                // Get DNS Items
                if (loadDnsItems) await LoadDnsItemsAsync(DGS_Subscription, null);
            }
            else if (groupItem.Mode == GroupMode.AnonymizedDNSCrypt)
            {
                // Get Group Settings
                GroupSettings groupSettings = MainWindow.ServersManager.Get_GroupSettings(groupItem.Name);
                this.DispatchIt(() =>
                {
                    AnonDNSCrypt_EnableGroup_ToggleSwitch.IsChecked = groupSettings.Enabled;
                    AnonDNSCrypt_Settings_LookupDomain_TextBox.Text = groupSettings.LookupDomain;
                    AnonDNSCrypt_Settings_TimeoutSec_NumericUpDown.Value = groupSettings.TimeoutSec;
                    AnonDNSCrypt_Settings_ParallelSize_NumericUpDown.Value = groupSettings.ParallelSize;
                    AnonDNSCrypt_Settings_BootstrapIP_TextBox.Text = groupSettings.BootstrapIP.ToString();
                    AnonDNSCrypt_Settings_BootstrapPort_NumericUpDown.Value = groupSettings.BootstrapPort;
                    AnonDNSCrypt_Settings_AllowInsecure_ToggleSwitch.IsChecked = groupSettings.AllowInsecure;

                    Flyout_AnonDNSCrypt_Source.IsEnabled = groupSettings.Enabled;
                    Flyout_AnonDNSCrypt_Options.IsEnabled = groupSettings.Enabled;
                    Flyout_AnonDNSCrypt_DnsServers.IsEnabled = groupSettings.Enabled;
                    Flyout_AnonDNSCrypt_Scan.IsEnabled = groupSettings.Enabled;
                });

                // Get Relays Source
                List<string> relaysURL = MainWindow.ServersManager.Get_AnonDNSCrypt_Relay_URLs(groupItem.Name);
                this.DispatchIt(() => AnonDNSCryptRelayTextBox.Text = relaysURL.ToString(NL));

                // Get Targets Source
                List<string> targetsURL = MainWindow.ServersManager.Get_AnonDNSCrypt_Target_URLs(groupItem.Name);
                this.DispatchIt(() => AnonDNSCryptTargetTextBox.Text = targetsURL.ToString(NL));

                // Get Relays And Targets
                List<string> relays = MainWindow.ServersManager.Get_AnonDNSCrypt_Relays(groupItem.Name);
                List<string> targets = MainWindow.ServersManager.Get_AnonDNSCrypt_Targets(groupItem.Name);

                // Update AnonDNSCryptSource_Info_TextBlock
                AnonDNSCryptSource_Info_TextBlock.Clear();
                AnonDNSCryptSource_Info_TextBlock.AppendText("Relays: ", null, $"{relays.Count}", Brushes.Orange, $",{LS}Targets: ", null, $"{targets.Count}", Brushes.Orange);

                // Get DateTime
                LastAutoUpdate dateTime = MainWindow.ServersManager.Get_LastAutoUpdate(groupItem.Name);
                AnonDNSCryptUpdateSourceTextBlock.Clear();
                AnonDNSCryptUpdateSourceTextBlock.AppendText("Last Update: ", null, $"{dateTime.LastUpdateSource:yyyy/MM/dd HH:mm:ss}", greenBrush);
                AnonDNSCryptScanServersTextBlock.Clear();
                AnonDNSCryptScanServersTextBlock.AppendText("Last Scan: ", null, $"{dateTime.LastScanServers:yyyy/MM/dd HH:mm:ss}", greenBrush);

                // Get Options
                AnonDNSCryptOptions options = MainWindow.ServersManager.Get_AnonDNSCrypt_Options(groupItem.Name);
                this.DispatchIt(() =>
                {
                    // AutoUpdate
                    AnonDNSCryptUpdateSourceNumericUpDown.Value = options.AutoUpdate.UpdateSource;
                    AnonDNSCryptScanServersNumericUpDown.Value = options.AutoUpdate.ScanServers;

                    // FilterByProperties
                    AnonDNSCryptFilter_Google_CheckBox.IsChecked = DnsFilterToBool(options.FilterByProperties.GoogleSafeSearch);
                    AnonDNSCryptFilter_Bing_CheckBox.IsChecked = DnsFilterToBool(options.FilterByProperties.BingSafeSearch);
                    AnonDNSCryptFilter_Youtube_CheckBox.IsChecked = DnsFilterToBool(options.FilterByProperties.YoutubeRestricted);
                    AnonDNSCryptFilter_Adult_CheckBox.IsChecked = DnsFilterToBool(options.FilterByProperties.AdultBlocked);
                });

                // Get DnsItems Info
                DnsItemsInfo info = MainWindow.ServersManager.Get_DnsItems_Info(groupItem.Name);
                AnonDNSCryptFilter_Info_TextBlock.Clear();
                AnonDNSCryptFilter_Info_TextBlock.AppendText("Total Servers: ", null, $"{info.TotalServers}", Brushes.Orange, $",{LS}Online Servers: ", null, $"{info.OnlineServers}", Brushes.Orange);
                AnonDNSCryptFilter_Info_TextBlock.AppendText($",{LS}Selected Servers: ", null, $"{info.SelectedServers}", Brushes.Orange, $",{LS}Average Latency: ", null, $"{info.AverageLatency}", Brushes.Orange, "ms", null);

                // Get DNS Items
                if (loadDnsItems) await LoadDnsItemsAsync(DGS_AnonDNSCrypt, null);
            }
            else if (groupItem.Mode == GroupMode.FragmentDoH)
            {
                // Get Group Settings
                GroupSettings groupSettings = MainWindow.ServersManager.Get_GroupSettings(groupItem.Name);
                this.DispatchIt(() =>
                {
                    FragmentDoH_EnableGroup_ToggleSwitch.IsChecked = groupSettings.Enabled;
                    FragmentDoH_Settings_LookupDomain_TextBox.Text = groupSettings.LookupDomain;
                    FragmentDoH_Settings_TimeoutSec_NumericUpDown.Value = groupSettings.TimeoutSec;
                    FragmentDoH_Settings_ParallelSize_NumericUpDown.Value = groupSettings.ParallelSize;
                    FragmentDoH_Settings_BootstrapIP_TextBox.Text = groupSettings.BootstrapIP.ToString();
                    FragmentDoH_Settings_BootstrapPort_NumericUpDown.Value = groupSettings.BootstrapPort;
                    FragmentDoH_Settings_AllowInsecure_ToggleSwitch.IsChecked = groupSettings.AllowInsecure;

                    Flyout_FragmentDoH_Source.IsEnabled = groupSettings.Enabled;
                    Flyout_FragmentDoH_Options.IsEnabled = groupSettings.Enabled;
                    Flyout_FragmentDoH_DnsServers.IsEnabled = groupSettings.Enabled;
                    Flyout_FragmentDoH_Scan.IsEnabled = groupSettings.Enabled;
                });

                // Get Source Enable
                bool isEnable = MainWindow.ServersManager.Get_Source_EnableDisable(groupItem.Name);
                this.DispatchIt(() => FragmentDoHSourceByUrlByManualToggleSwitch.IsChecked = !isEnable);

                // Get URLs
                List<string> subs = MainWindow.ServersManager.Get_Source_URLs(groupItem.Name);
                this.DispatchIt(() => FragmentDoHSourceTextBox.Text = subs.ToString(NL));

                // Get DateTime
                LastAutoUpdate dateTime = MainWindow.ServersManager.Get_LastAutoUpdate(groupItem.Name);
                FragmentDoHUpdateSourceTextBlock.Clear();
                FragmentDoHUpdateSourceTextBlock.AppendText("Last Update: ", null, $"{dateTime.LastUpdateSource:yyyy/MM/dd HH:mm:ss}", greenBrush);
                FragmentDoHScanServersTextBlock.Clear();
                FragmentDoHScanServersTextBlock.AppendText("Last Scan: ", null, $"{dateTime.LastScanServers:yyyy/MM/dd HH:mm:ss}", greenBrush);

                // Get Options
                FragmentDoHOptions options = MainWindow.ServersManager.Get_FragmentDoH_Options(groupItem.Name);
                this.DispatchIt(() =>
                {
                    // AutoUpdate
                    FragmentDoHUpdateSourceNumericUpDown.Value = options.AutoUpdate.UpdateSource;
                    FragmentDoHScanServersNumericUpDown.Value = options.AutoUpdate.ScanServers;

                    // FilterByProperties
                    FragmentDoHFilter_Google_CheckBox.IsChecked = DnsFilterToBool(options.FilterByProperties.GoogleSafeSearch);
                    FragmentDoHFilter_Bing_CheckBox.IsChecked = DnsFilterToBool(options.FilterByProperties.BingSafeSearch);
                    FragmentDoHFilter_Youtube_CheckBox.IsChecked = DnsFilterToBool(options.FilterByProperties.YoutubeRestricted);
                    FragmentDoHFilter_Adult_CheckBox.IsChecked = DnsFilterToBool(options.FilterByProperties.AdultBlocked);

                    // FragmentSettings
                    FragmentDoHSettings_ChunksBeforeSNI_NumericUpDown.Value = options.FragmentSettings.ChunksBeforeSNI;
                    int sniChunkModeInt = options.FragmentSettings.SniChunkMode switch
                    {
                        AgnosticProgram.Fragment.ChunkMode.SNI => 0,
                        AgnosticProgram.Fragment.ChunkMode.SniExtension => 1,
                        AgnosticProgram.Fragment.ChunkMode.AllExtensions => 2,
                        _ => 0
                    };
                    FragmentDoHSettings_SniChunkMode_ComboBox.SelectedIndex = sniChunkModeInt;
                    FragmentDoHSettings_ChunksSNI_NumericUpDown.Value = options.FragmentSettings.ChunksSNI;
                    FragmentDoHSettings_AntiPatternOffset_NumericUpDown.Value = options.FragmentSettings.AntiPatternOffset;
                    FragmentDoHSettings_FragmentDelayMS_NumericUpDown.Value = options.FragmentSettings.FragmentDelayMS;
                });

                // Get DnsItems Info
                DnsItemsInfo info = MainWindow.ServersManager.Get_DnsItems_Info(groupItem.Name);
                FragmentDoHSource_Info_TextBlock.Clear();
                FragmentDoHSource_Info_TextBlock.AppendText("Total Servers: ", null, $"{info.TotalServers}", Brushes.Orange);
                FragmentDoHFilter_Info_TextBlock.Clear();
                FragmentDoHFilter_Info_TextBlock.AppendText("Total Servers: ", null, $"{info.TotalServers}", Brushes.Orange, $",{LS}Online Servers: ", null, $"{info.OnlineServers}", Brushes.Orange);
                FragmentDoHFilter_Info_TextBlock.AppendText($",{LS}Selected Servers: ", null, $"{info.SelectedServers}", Brushes.Orange, $",{LS}Average Latency: ", null, $"{info.AverageLatency}", Brushes.Orange, "ms", null);

                // Get DNS Items
                if (loadDnsItems) await LoadDnsItemsAsync(DGS_FragmentDoH, null);
            }
            else if (groupItem.Mode == GroupMode.Custom)
            {
                // Get Group Settings
                GroupSettings groupSettings = MainWindow.ServersManager.Get_GroupSettings(groupItem.Name);
                this.DispatchIt(() =>
                {
                    Custom_EnableGroup_ToggleSwitch.IsChecked = groupSettings.Enabled;
                    Custom_Settings_LookupDomain_TextBox.Text = groupSettings.LookupDomain;
                    Custom_Settings_TimeoutSec_NumericUpDown.Value = groupSettings.TimeoutSec;
                    Custom_Settings_ParallelSize_NumericUpDown.Value = groupSettings.ParallelSize;
                    Custom_Settings_BootstrapIP_TextBox.Text = groupSettings.BootstrapIP.ToString();
                    Custom_Settings_BootstrapPort_NumericUpDown.Value = groupSettings.BootstrapPort;
                    Custom_Settings_AllowInsecure_ToggleSwitch.IsChecked = groupSettings.AllowInsecure;

                    Flyout_Custom_Source.IsEnabled = groupSettings.Enabled;
                    Flyout_Custom_Options.IsEnabled = groupSettings.Enabled;
                    Flyout_Custom_DnsServers.IsEnabled = groupSettings.Enabled;
                    Flyout_Custom_Scan.IsEnabled = groupSettings.Enabled;
                });

                // Get URLs
                List<string> subs = MainWindow.ServersManager.Get_Source_URLs(groupItem.Name);
                this.DispatchIt(() => CustomSourceTextBox.Text = subs.ToString(NL));

                // Get DateTime
                LastAutoUpdate dateTime = MainWindow.ServersManager.Get_LastAutoUpdate(groupItem.Name);
                CustomScanServersTextBlock.Clear();
                CustomScanServersTextBlock.AppendText("Last Scan: ", null, $"{dateTime.LastScanServers:yyyy/MM/dd HH:mm:ss}", greenBrush);

                // Get Options
                CustomOptions options = MainWindow.ServersManager.Get_Custom_Options(groupItem.Name);
                this.DispatchIt(() =>
                {
                    // AutoUpdate
                    CustomScanServersNumericUpDown.Value = options.AutoUpdate.ScanServers;

                    // FilterByProtocols
                    CustomFilter_UDP_CheckBox.IsChecked = options.FilterByProtocols.UDP;
                    CustomFilter_TCP_CheckBox.IsChecked = options.FilterByProtocols.TCP;
                    CustomFilter_DNSCrypt_CheckBox.IsChecked = options.FilterByProtocols.DnsCrypt;
                    CustomFilter_AnonymizedDNSCrypt_CheckBox.IsChecked = options.FilterByProtocols.AnonymizedDNSCrypt;
                    CustomFilter_DoT_CheckBox.IsChecked = options.FilterByProtocols.DoT;
                    CustomFilter_DoH_CheckBox.IsChecked = options.FilterByProtocols.DoH;
                    CustomFilter_ODoH_CheckBox.IsChecked = options.FilterByProtocols.ObliviousDoH;
                    CustomFilter_DoQ_CheckBox.IsChecked = options.FilterByProtocols.DoQ;

                    // FilterByProperties
                    CustomFilter_Google_CheckBox.IsChecked = DnsFilterToBool(options.FilterByProperties.GoogleSafeSearch);
                    CustomFilter_Bing_CheckBox.IsChecked = DnsFilterToBool(options.FilterByProperties.BingSafeSearch);
                    CustomFilter_Youtube_CheckBox.IsChecked = DnsFilterToBool(options.FilterByProperties.YoutubeRestricted);
                    CustomFilter_Adult_CheckBox.IsChecked = DnsFilterToBool(options.FilterByProperties.AdultBlocked);
                });

                // Get DnsItems Info
                DnsItemsInfo info = MainWindow.ServersManager.Get_DnsItems_Info(groupItem.Name);
                CustomSource_Info_TextBlock.Clear();
                CustomSource_Info_TextBlock.AppendText("Total Servers: ", null, $"{info.TotalServers}", Brushes.Orange);
                CustomFilter_Info_TextBlock.Clear();
                CustomFilter_Info_TextBlock.AppendText("Total Servers: ", null, $"{info.TotalServers}", Brushes.Orange, $",{LS}Online Servers: ", null, $"{info.OnlineServers}", Brushes.Orange);
                CustomFilter_Info_TextBlock.AppendText($",{LS}Selected Servers: ", null, $"{info.SelectedServers}", Brushes.Orange, $",{LS}Average Latency: ", null, $"{info.AverageLatency}", Brushes.Orange, "ms", null);

                // Get DNS Items
                if (loadDnsItems) await LoadDnsItemsAsync(DGS_Custom, null);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow LoadSelectedGroup: " + ex.Message);
        }
    }

    private async void DGG_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        await LoadSelectedGroupAsync(true);
    }

    private static DataGridLength SetDataGridDnsServersSize(DataGrid dg)
    {
        try
        {
            // Set DNS Address Column To Fill Space
            if (dg.Columns.Count > 1)
            {
                double others = 0;
                for (int n = 0; n < dg.Columns.Count; n++)
                {
                    DataGridColumn column = dg.Columns[n];
                    if (n != 1) others += column.ActualWidth;
                }

                dg.Columns[1].MinWidth = 200;
                dg.Columns[1].MaxWidth = double.MaxValue;
                double width = dg.ActualWidth - others - SystemParameters.VerticalScrollBarWidth;
                dg.Columns[1].Width = new DataGridLength(width, DataGridLengthUnitType.Pixel);
                return dg.Columns[1].Width;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow SetDataGridDnsServersSize: " + ex.Message);
        }
        return DataGridLength.Auto;
    }

    private async Task CreateDnsItemColumns_Async(DataGrid dg)
    {
        DnsItem di;
        dg.Columns.Clear();
        dg.AutoGenerateColumns = false;

        DataGridCheckBoxColumn c_Enabled = new()
        {
            Header = "Enabled",
            Binding = new Binding(nameof(di.Enabled)),
            IsReadOnly = true,
            CanUserResize = false,
            ElementStyle = TryFindResource("DataGridCheckBoxColumn_ReadOnlyStyle") as Style,
            EditingElementStyle = TryFindResource("DataGridCheckBoxColumn_ReadOnlyStyle") as Style
        };
        dg.Columns.Add(c_Enabled);

        DataGridTextColumn c_DNS = new()
        {
            Header = "DNS Address",
            Binding = new Binding(nameof(di.DNS_URL)),
            IsReadOnly = true,
            CanUserResize = true,
            FontWeight = FontWeights.UltraLight
        };
        dg.Columns.Add(c_DNS);

        DataGridTextColumn c_Protocol = new()
        {
            Header = "Protocol",
            Binding = new Binding(nameof(di.Protocol)),
            IsReadOnly = true,
            CanUserResize = false
        };
        dg.Columns.Add(c_Protocol);

        DataGridTextColumn c_Status = new()
        {
            Header = "Status",
            Binding = new Binding(nameof(di.Status)),
            MinWidth = 70,
            IsReadOnly = true,
            CanUserResize = false,
            CellStyle = TryFindResource("DataGridTextColumnStatus_Style") as Style
        };
        dg.Columns.Add(c_Status);

        DataGridTextColumn c_Latency = new()
        {
            Header = "Latency",
            Binding = new Binding(nameof(di.Latency)),
            IsReadOnly = true,
            CanUserResize = false
        };
        dg.Columns.Add(c_Latency);

        DataGridTextColumn c_IsGoogleSafeSearchEnabled = new()
        {
            Header = "Google Safe Search",
            Binding = new Binding(nameof(di.IsGoogleSafeSearchEnabled)),
            IsReadOnly = true,
            CanUserResize = false,
            CellStyle = TryFindResource("DataGridTextColumnHasFilter_Style") as Style
        };
        dg.Columns.Add(c_IsGoogleSafeSearchEnabled);

        DataGridTextColumn c_IsBingSafeSearchEnabled = new()
        {
            Header = "Bing Safe Search",
            Binding = new Binding(nameof(di.IsBingSafeSearchEnabled)),
            IsReadOnly = true,
            CanUserResize = false,
            CellStyle = TryFindResource("DataGridTextColumnHasFilter_Style") as Style
        };
        dg.Columns.Add(c_IsBingSafeSearchEnabled);

        DataGridTextColumn c_IsYoutubeRestricted = new()
        {
            Header = "Youtube Restricted",
            Binding = new Binding(nameof(di.IsYoutubeRestricted)),
            IsReadOnly = true,
            CanUserResize = false,
            CellStyle = TryFindResource("DataGridTextColumnHasFilter_Style") as Style
        };
        dg.Columns.Add(c_IsYoutubeRestricted);

        DataGridTextColumn c_IsAdultBlocked = new()
        {
            Header = "Adult Blocked",
            Binding = new Binding(nameof(di.IsAdultBlocked)),
            IsReadOnly = true,
            CanUserResize = false,
            CellStyle = TryFindResource("DataGridTextColumnHasFilter_Style") as Style
        };
        dg.Columns.Add(c_IsAdultBlocked);

        DataGridTextColumn c_Description = new()
        {
            Header = "Description",
            Binding = new Binding(nameof(di.Description)),
            IsReadOnly = true,
            CanUserResize = false,
            FontWeight = FontWeights.Normal
        };
        dg.Columns.Add(c_Description);

        // Set DNS Address Column's Width To Avoid Blinking On Update Items
        c_DNS.Width = SetDataGridDnsServersSize(dg);
        await Task.Delay(50);
    }

    private async Task CreateDnsItemColumns_FragmentDoH_Async(DataGrid dg)
    {
        DnsItem fi;
        dg.Columns.Clear();
        dg.AutoGenerateColumns = false;

        DataGridCheckBoxColumn c_Enabled = new()
        {
            Header = "Enabled",
            Binding = new Binding(nameof(fi.Enabled)),
            IsReadOnly = true,
            CanUserResize = false,
            ElementStyle = TryFindResource("DataGridCheckBoxColumn_ReadOnlyStyle") as Style,
            EditingElementStyle = TryFindResource("DataGridCheckBoxColumn_ReadOnlyStyle") as Style
        };
        dg.Columns.Add(c_Enabled);

        DataGridTextColumn c_DoH_URL = new()
        {
            Header = "DoH Address",
            Binding = new Binding(nameof(fi.DNS_URL)),
            IsReadOnly = true,
            CanUserResize = true,
            FontWeight = FontWeights.UltraLight
        };
        dg.Columns.Add(c_DoH_URL);

        DataGridTextColumn c_DoH_IP = new()
        {
            Header = "DoH IP Address",
            Binding = new Binding(nameof(fi.DNS_IP)),
            IsReadOnly = true,
            CanUserResize = true,
            FontWeight = FontWeights.UltraLight
        };
        dg.Columns.Add(c_DoH_IP);

        DataGridTextColumn c_Protocol = new()
        {
            Header = "Protocol",
            Binding = new Binding(nameof(fi.Protocol)),
            IsReadOnly = true,
            CanUserResize = false
        };
        dg.Columns.Add(c_Protocol);

        DataGridTextColumn c_Status = new()
        {
            Header = "Status",
            Binding = new Binding(nameof(fi.Status)),
            MinWidth = 70,
            IsReadOnly = true,
            CanUserResize = false,
            CellStyle = TryFindResource("DataGridTextColumnStatus_Style") as Style
        };
        dg.Columns.Add(c_Status);

        DataGridTextColumn c_Latency = new()
        {
            Header = "Latency",
            Binding = new Binding(nameof(fi.Latency)),
            IsReadOnly = true,
            CanUserResize = false
        };
        dg.Columns.Add(c_Latency);

        DataGridTextColumn c_IsGoogleSafeSearchEnabled = new()
        {
            Header = "Google Safe Search",
            Binding = new Binding(nameof(fi.IsGoogleSafeSearchEnabled)),
            IsReadOnly = true,
            CanUserResize = false,
            CellStyle = TryFindResource("DataGridTextColumnHasFilter_Style") as Style
        };
        dg.Columns.Add(c_IsGoogleSafeSearchEnabled);

        DataGridTextColumn c_IsBingSafeSearchEnabled = new()
        {
            Header = "Bing Safe Search",
            Binding = new Binding(nameof(fi.IsBingSafeSearchEnabled)),
            IsReadOnly = true,
            CanUserResize = false,
            CellStyle = TryFindResource("DataGridTextColumnHasFilter_Style") as Style
        };
        dg.Columns.Add(c_IsBingSafeSearchEnabled);

        DataGridTextColumn c_IsYoutubeRestricted = new()
        {
            Header = "Youtube Restricted",
            Binding = new Binding(nameof(fi.IsYoutubeRestricted)),
            IsReadOnly = true,
            CanUserResize = false,
            CellStyle = TryFindResource("DataGridTextColumnHasFilter_Style") as Style
        };
        dg.Columns.Add(c_IsYoutubeRestricted);

        DataGridTextColumn c_IsAdultBlocked = new()
        {
            Header = "Adult Blocked",
            Binding = new Binding(nameof(fi.IsAdultBlocked)),
            IsReadOnly = true,
            CanUserResize = false,
            CellStyle = TryFindResource("DataGridTextColumnHasFilter_Style") as Style
        };
        dg.Columns.Add(c_IsAdultBlocked);

        DataGridTextColumn c_Description = new()
        {
            Header = "Description",
            Binding = new Binding(nameof(fi.Description)),
            IsReadOnly = true,
            CanUserResize = false,
            FontWeight = FontWeights.Normal
        };
        dg.Columns.Add(c_Description);

        // Set DNS Address Column's Width To Avoid Blinking On Update Items
        c_DoH_URL.Width = SetDataGridDnsServersSize(dg);
        await Task.Delay(50);
    }

    private static void DGS_DnsServers_PreviewKeyDown(DataGrid dg, KeyEventArgs e)
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
            Debug.WriteLine("ManageServersWindow DGS_DnsServers_PreviewKeyDown: " + ex.Message);
        }
    }

    private void DGS_DnsServers_SelectionChanged(DataGrid dg)
    {
        if (DGG.SelectedItem is not GroupItem groupItem) return;
        if (dg.SelectedItem is not DnsItem dnsItem) return;

        try
        {
            // Set Header
            string header = $"Group: \"{groupItem.Name}\", Mode: \"{Get_GroupModeName(groupItem.Mode)}\" - DNS No: {dg.SelectedIndex + 1}";
            ServersTitleGroupBox.Header = header;

            // Set URL And IP To Manual Edit
            if (groupItem.Mode == GroupMode.FragmentDoH)
            {
                this.DispatchIt(() =>
                {
                    FragmentDoHByManualUrlTextBox.Text = dnsItem.DNS_URL;
                    FragmentDoHByManualIpTextBox.Text = dnsItem.DNS_IP.ToString();
                });
            }
            else if (groupItem.Mode == GroupMode.Custom)
            {
                this.DispatchIt(() =>
                {
                    CustomByManual_DnsAddress_TextBox.Text = dnsItem.DNS_URL;
                    CustomByManual_DnsDescription_TextBox.Text = dnsItem.Description;
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow DGS_DnsServers_SelectionChanged: " + ex.Message);
        }
    }

    private async Task LoadDnsItemsAsync(DataGrid dg, string? selectDnsItemByDnsAddress)
    {
        try
        {
            // Read DNS Items
            if (DGG.SelectedItem is not GroupItem groupItem) return;

            // Save Previous Selected Index
            int previousSelectedIndex = dg.SelectedIndex;

            await Task.Delay(10); // It's Necessary To Save Previous Selected Index
            if (groupItem.Mode == GroupMode.Subscription)
            {
                List<DnsItem> dnsItems = MainWindow.ServersManager.Get_DnsItems(groupItem.Name);
                MainWindow.ServersManager.BindDataSource_Subscription.Clear();
                for (int n = 0; n < dnsItems.Count; n++)
                {
                    DnsItem dnsItem = dnsItems[n];
                    MainWindow.ServersManager.BindDataSource_Subscription.Add(dnsItem);
                }
            }
            else if (groupItem.Mode == GroupMode.AnonymizedDNSCrypt)
            {
                List<DnsItem> dnsItems = MainWindow.ServersManager.Get_DnsItems(groupItem.Name);
                MainWindow.ServersManager.BindDataSource_AnonDNSCrypt.Clear();
                for (int n = 0; n < dnsItems.Count; n++)
                {
                    DnsItem dnsItem = dnsItems[n];
                    MainWindow.ServersManager.BindDataSource_AnonDNSCrypt.Add(dnsItem);
                }
            }
            else if (groupItem.Mode == GroupMode.FragmentDoH)
            {
                List<DnsItem> dnsItems = MainWindow.ServersManager.Get_DnsItems(groupItem.Name);
                MainWindow.ServersManager.BindDataSource_FragmentDoH.Clear();
                for (int n = 0; n < dnsItems.Count; n++)
                {
                    DnsItem dnsItem = dnsItems[n];
                    MainWindow.ServersManager.BindDataSource_FragmentDoH.Add(dnsItem);
                }
            }
            else if (groupItem.Mode == GroupMode.Custom)
            {
                List<DnsItem> dnsItems = MainWindow.ServersManager.Get_DnsItems(groupItem.Name);
                MainWindow.ServersManager.BindDataSource_Custom.Clear();
                for (int n = 0; n < dnsItems.Count; n++)
                {
                    DnsItem dnsItem = dnsItems[n];
                    MainWindow.ServersManager.BindDataSource_Custom.Add(dnsItem);
                }
            }
            await Task.Delay(10);

            bool isDnsItemSelected = false;
            if (!string.IsNullOrEmpty(selectDnsItemByDnsAddress))
            {
                for (int n = 0; n < dg.Items.Count; n++)
                {
                    object? item = dg.Items[n];
                    if (item is not DnsItem dnsItem) continue;
                    if (dnsItem.DNS_URL.Equals(selectDnsItemByDnsAddress))
                    {
                        dg.SelectedIndex = n;
                        dg.ScrollIntoViewByIndex(dg.SelectedIndex);
                        isDnsItemSelected = true;
                        break;
                    }
                }
            }

            if (!isDnsItemSelected && dg.Items.Count > 0)
            {
                dg.SelectedIndex = (previousSelectedIndex != -1 && previousSelectedIndex < dg.Items.Count) ? previousSelectedIndex : 0;
                dg.ScrollIntoViewByIndex(dg.SelectedIndex); // Scroll
            }

            // Wait For Setting Selected Index
            if (dg.Items.Count > 0)
            {
                Task wait = Task.Run(async () =>
                {
                    while (true)
                    {
                        int selectedIndex = -1;
                        this.DispatchIt(() => selectedIndex = dg.SelectedIndex);
                        if (selectedIndex >= 0) break;
                        await Task.Delay(20);
                    }
                });
                try { await wait.WaitAsync(TimeSpan.FromMilliseconds(500)); } catch (Exception) { }
                if (dg.SelectedIndex < 0) dg.SelectedIndex = 0;
            }

            SetDataGridDnsServersSize(dg);
            await Task.Delay(100); // Wait For Animations
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow LoadDnsItemsAsync: " + ex.Message);
        }
    }

    private void SourceBrowseButton(TextBox textBox)
    {
        try
        {
            OpenFileDialog ofd = new()
            {
                Filter = "TXT DNS Servers (*.txt)|*.txt|HTML (*.html)|*.html|Markdown (*.md)|*.md",
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
            Debug.WriteLine("ManageServersWindow SourceBrowseButton: " + ex.Message);
        }
    }

    private void MenuItem_All_CopyToCustom_Handler(MenuItem menuItem_CopyToCustom, List<DnsItem> selectedDnsItems)
    {
        try
        {
            if (DGG.SelectedItem is not GroupItem groupItem) return;
            List<string> customGroups = new();
            List<string> allEnabledGroups = MainWindow.ServersManager.Get_Group_Names(false);
            for (int n = 0; n < allEnabledGroups.Count; n++)
            {
                string groupName = allEnabledGroups[n];
                GroupMode mode = MainWindow.ServersManager.Get_GroupMode_ByName(groupName);
                if (mode == GroupMode.Custom && !groupItem.Name.Equals(groupName)) customGroups.Add(groupName);
            }

            menuItem_CopyToCustom.Items.Clear();
            for (int n = 0; n < customGroups.Count; n++)
            {
                string groupName = customGroups[n];
                MenuItem subMenuItem = new()
                {
                    Header = groupName,
                    Tag = selectedDnsItems
                };
                subMenuItem.Click += MenuItem_CopyToCustom_Click;
                menuItem_CopyToCustom.Items.Add(subMenuItem);
            }

            if (customGroups.Count == 0) menuItem_CopyToCustom.IsEnabled = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow MenuItem_All_CopyToCustom_Handler: " + ex.Message);
        }
    }

    private async void MenuItem_CopyToCustom_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem menuItem) return;
            if (menuItem.Header is not string toGroup) return;
            if (menuItem.Tag is not List<DnsItem> selectedDnsItems) return;

            await MainWindow.ServersManager.Append_DnsItems_Async(toGroup, selectedDnsItems, true);

            WpfToastDialog.Show(this, $"Copied To {toGroup}.", MessageBoxImage.None, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServersWindow MenuItem_CopyToCustom_Click: " + ex.Message);
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
                if (MainWindow.ServersManager.IsScanning)
                {
                    MainWindow.ServersManager.ScanServers(new GroupItem(), null);
                    await Task.Run(async () =>
                    {
                        while (true)
                        {
                            this.DispatchIt(() => IsHitTestVisible = false);
                            NoGroupText_TextBlock.SetText("Stopping Scan...");
                            ServersTabControl.DispatchIt(() => ServersTabControl.SelectedIndex = 0);
                            await Task.Delay(100);
                            if (!MainWindow.ServersManager.IsScanning) break;
                        }
                    });
                }
                // Save
                await MainWindow.ServersManager.SaveAsync();
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
        catch (Exception) { }
    }

}