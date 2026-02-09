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
using DNSveil.Logic.DnsServers;
using Microsoft.Win32;
using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using MsmhToolsWpfClass;
using MsmhToolsWpfClass.Themes;
using static DNSveil.Logic.DnsServers.DnsModel;
using static DNSveil.Logic.DnsServers.DnsServersManager;

namespace DNSveil.ManageDns;

/// <summary>
/// Interaction logic for ManageDnsWindow.xaml
/// </summary>
public partial class ManageDnsWindow : WpfWindow
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

    public ManageDnsWindow()
    {
        InitializeComponent();

        // Invariant Culture
        Info.SetCulture(CultureInfo.InvariantCulture);

        Flyout_Subscription_Options.IsOpen = false;
        Flyout_AnonDNSCrypt_Options.IsOpen = false;
        Flyout_FragmentDoH_Options.IsOpen = false;
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
            for (int n = 0; n < MainWindow.DnsManager.Model.Groups.Count; n++)
            {
                if (n == selectedGroupIndex)
                {
                    BindingOperations.ClearBinding(DGS_Custom, DataGrid.ItemsSourceProperty);
                    await Task.Delay(1);
                    DnsGroup group = MainWindow.DnsManager.Model.Groups[n];
                    BindingOperations.EnableCollectionSynchronization(group.Items, SyncLock);
                    if (group.Mode == GroupMode.Subscription)
                    {
                        DGS_Subscription.ItemsSource = group.Items;
                        DGS_Subscription.DataContext = group.Items;
                        await Task.Delay(1);
                        break;
                    }
                    else if (group.Mode == GroupMode.AnonymizedDNSCrypt)
                    {
                        DGS_AnonDNSCrypt.ItemsSource = group.Items;
                        DGS_AnonDNSCrypt.DataContext = group.Items;
                        await Task.Delay(1);
                        break;
                    }
                    else if (group.Mode == GroupMode.FragmentDoH)
                    {
                        DGS_FragmentDoH.ItemsSource = group.Items;
                        DGS_FragmentDoH.DataContext = group.Items;
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
            Debug.WriteLine("ManageDnsWindow BindItemsAsync: " + ex.Message);
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
                    if (item is not DnsGroup group1) continue;
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
            if (DGG.SelectedItem is DnsGroup group2)
            {
                string header = $"Group: \"{group2.Name}\", Mode: \"{group2.Mode}\"";
                this.DispatchIt(() => ServersTitleGroupBox.Header = header);
            }

            DGG.UpdateColumnsWidthToAuto();
    }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow SelectGroupAsync By Name: " + ex.Message);
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
            Debug.WriteLine("ManageDnsWindow SelectGroupAsync By Index: " + ex.Message);
        }
    }

    private async Task LoadSelectedGroupAsync(bool readOnlyGroupSettings = false)
    {
        try
        {
            if (DGG.SelectedItem is not DnsGroup group)
            {
                ServersTabControl.SelectedIndex = 0;
                return;
            }

            // Set Header
            string header = $"Group: \"{group.Name}\", Mode: \"{Get_GroupModeName(group.Mode)}\"";
            this.DispatchIt(() => ServersTitleGroupBox.Header = header);

            // Select Tab
            this.DispatchIt(() =>
            {
                ServersTabControl.SelectedIndex = group.Mode switch
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

            if (group.Mode == GroupMode.Subscription)
            {
                // Get Group Settings
                GroupSettings groupSettings = MainWindow.DnsManager.Get_GroupSettings(group.Name);
                this.DispatchIt(() =>
                {
                    Subscription_EnableGroup_ToggleSwitch.IsChecked = groupSettings.IsEnabled;
                    Subscription_Settings_LookupDomain_TextBox.Text = groupSettings.LookupDomain;
                    Subscription_Settings_TimeoutSec_NumericUpDown.Value = groupSettings.ScanTimeoutSec;
                    Subscription_Settings_ParallelSize_NumericUpDown.Value = groupSettings.ScanParallelSize;
                    Subscription_Settings_BootstrapIP_TextBox.Text = groupSettings.BootstrapIP.ToString();
                    Subscription_Settings_BootstrapPort_NumericUpDown.Value = groupSettings.BootstrapPort;
                    Subscription_Settings_AllowInsecure_ToggleSwitch.IsChecked = groupSettings.AllowInsecure;

                    Flyout_Subscription_Source.IsEnabled = groupSettings.IsEnabled;
                    Flyout_Subscription_Options.IsEnabled = groupSettings.IsEnabled;
                    Flyout_Subscription_DnsServers.IsEnabled = groupSettings.IsEnabled;
                    Flyout_Subscription_Scan.IsEnabled = groupSettings.IsEnabled;
                });
                if (readOnlyGroupSettings) return;

                // Get URLs
                List<string> subs = MainWindow.DnsManager.Get_Source_URLs(group.Name);
                this.DispatchIt(() => SubscriptionSourceTextBox.Text = subs.ToString(NL));

                // Get Options
                SubscriptionOptions options = MainWindow.DnsManager.Get_Subscription_Options(group.Name);
                SubscriptionUpdateSourceTextBlock.Clear();
                SubscriptionUpdateSourceTextBlock.AppendText("Last Update: ", null, $"{options.AutoUpdate.LastUpdateSource:yyyy/MM/dd HH:mm:ss}", greenBrush);
                SubscriptionScanServersTextBlock.Clear();
                SubscriptionScanServersTextBlock.AppendText("Last Scan: ", null, $"{options.AutoUpdate.LastScanServers:yyyy/MM/dd HH:mm:ss}", greenBrush);

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
                    SubscriptionFilter_Google_CheckBox.IsChecked = Tools.DnsFilterToBool(options.FilterByProperties.GoogleSafeSearch);
                    SubscriptionFilter_Bing_CheckBox.IsChecked = Tools.DnsFilterToBool(options.FilterByProperties.BingSafeSearch);
                    SubscriptionFilter_Youtube_CheckBox.IsChecked = Tools.DnsFilterToBool(options.FilterByProperties.YoutubeRestricted);
                    SubscriptionFilter_Adult_CheckBox.IsChecked = Tools.DnsFilterToBool(options.FilterByProperties.AdultBlocked);
                });

                // Get DnsItems Info
                DnsItemsInfo info = MainWindow.DnsManager.Get_DnsItems_Info(group.Name);
                SubscriptionSource_Info_TextBlock.Clear();
                SubscriptionSource_Info_TextBlock.AppendText("Total Servers: ", null, $"{info.TotalServers}", Brushes.Orange);
                SubscriptionFilter_Info_TextBlock.Clear();
                SubscriptionFilter_Info_TextBlock.AppendText("Total Servers: ", null, $"{info.TotalServers}", Brushes.Orange, $",{LS}Online Servers: ", null, $"{info.OnlineServers}", Brushes.Orange);
                SubscriptionFilter_Info_TextBlock.AppendText($",{LS}Selected Servers: ", null, $"{info.SelectedServers}", Brushes.Orange, $",{LS}Average Latency: ", null, $"{info.AverageLatency}", Brushes.Orange, "ms", null);
            }
            else if (group.Mode == GroupMode.AnonymizedDNSCrypt)
            {
                // Get Group Settings
                GroupSettings groupSettings = MainWindow.DnsManager.Get_GroupSettings(group.Name);
                this.DispatchIt(() =>
                {
                    AnonDNSCrypt_EnableGroup_ToggleSwitch.IsChecked = groupSettings.IsEnabled;
                    AnonDNSCrypt_Settings_LookupDomain_TextBox.Text = groupSettings.LookupDomain;
                    AnonDNSCrypt_Settings_TimeoutSec_NumericUpDown.Value = groupSettings.ScanTimeoutSec;
                    AnonDNSCrypt_Settings_ParallelSize_NumericUpDown.Value = groupSettings.ScanParallelSize;
                    AnonDNSCrypt_Settings_BootstrapIP_TextBox.Text = groupSettings.BootstrapIP.ToString();
                    AnonDNSCrypt_Settings_BootstrapPort_NumericUpDown.Value = groupSettings.BootstrapPort;
                    AnonDNSCrypt_Settings_AllowInsecure_ToggleSwitch.IsChecked = groupSettings.AllowInsecure;

                    Flyout_AnonDNSCrypt_Source.IsEnabled = groupSettings.IsEnabled;
                    Flyout_AnonDNSCrypt_Options.IsEnabled = groupSettings.IsEnabled;
                    Flyout_AnonDNSCrypt_DnsServers.IsEnabled = groupSettings.IsEnabled;
                    Flyout_AnonDNSCrypt_Scan.IsEnabled = groupSettings.IsEnabled;
                });
                if (readOnlyGroupSettings) return;

                // Get Relays Source
                List<string> relaysURL = MainWindow.DnsManager.Get_AnonDNSCrypt_Relay_URLs(group.Name);
                this.DispatchIt(() => AnonDNSCryptRelayTextBox.Text = relaysURL.ToString(NL));

                // Get Targets Source
                List<string> targetsURL = MainWindow.DnsManager.Get_AnonDNSCrypt_Target_URLs(group.Name);
                this.DispatchIt(() => AnonDNSCryptTargetTextBox.Text = targetsURL.ToString(NL));

                // Get Relays And Targets
                List<string> relays = MainWindow.DnsManager.Get_AnonDNSCrypt_Relays(group.Name);
                List<string> targets = MainWindow.DnsManager.Get_AnonDNSCrypt_Targets(group.Name);

                // Update AnonDNSCryptSource_Info_TextBlock
                AnonDNSCryptSource_Info_TextBlock.Clear();
                AnonDNSCryptSource_Info_TextBlock.AppendText("Relays: ", null, $"{relays.Count}", Brushes.Orange, $",{LS}Targets: ", null, $"{targets.Count}", Brushes.Orange);

                // Get Options
                AnonymizedDNSCryptOptions options = MainWindow.DnsManager.Get_AnonDNSCrypt_Options(group.Name);
                AnonDNSCryptUpdateSourceTextBlock.Clear();
                AnonDNSCryptUpdateSourceTextBlock.AppendText("Last Update: ", null, $"{options.AutoUpdate.LastUpdateSource:yyyy/MM/dd HH:mm:ss}", greenBrush);
                AnonDNSCryptScanServersTextBlock.Clear();
                AnonDNSCryptScanServersTextBlock.AppendText("Last Scan: ", null, $"{options.AutoUpdate.LastScanServers:yyyy/MM/dd HH:mm:ss}", greenBrush);

                this.DispatchIt(() =>
                {
                    // AutoUpdate
                    AnonDNSCryptUpdateSourceNumericUpDown.Value = options.AutoUpdate.UpdateSource;
                    AnonDNSCryptScanServersNumericUpDown.Value = options.AutoUpdate.ScanServers;

                    // FilterByProperties
                    AnonDNSCryptFilter_Google_CheckBox.IsChecked = Tools.DnsFilterToBool(options.FilterByProperties.GoogleSafeSearch);
                    AnonDNSCryptFilter_Bing_CheckBox.IsChecked = Tools.DnsFilterToBool(options.FilterByProperties.BingSafeSearch);
                    AnonDNSCryptFilter_Youtube_CheckBox.IsChecked = Tools.DnsFilterToBool(options.FilterByProperties.YoutubeRestricted);
                    AnonDNSCryptFilter_Adult_CheckBox.IsChecked = Tools.DnsFilterToBool(options.FilterByProperties.AdultBlocked);
                });

                // Get DnsItems Info
                DnsItemsInfo info = MainWindow.DnsManager.Get_DnsItems_Info(group.Name);
                AnonDNSCryptFilter_Info_TextBlock.Clear();
                AnonDNSCryptFilter_Info_TextBlock.AppendText("Total Servers: ", null, $"{info.TotalServers}", Brushes.Orange, $",{LS}Online Servers: ", null, $"{info.OnlineServers}", Brushes.Orange);
                AnonDNSCryptFilter_Info_TextBlock.AppendText($",{LS}Selected Servers: ", null, $"{info.SelectedServers}", Brushes.Orange, $",{LS}Average Latency: ", null, $"{info.AverageLatency}", Brushes.Orange, "ms", null);
            }
            else if (group.Mode == GroupMode.FragmentDoH)
            {
                // Get Group Settings
                GroupSettings groupSettings = MainWindow.DnsManager.Get_GroupSettings(group.Name);
                this.DispatchIt(() =>
                {
                    FragmentDoH_EnableGroup_ToggleSwitch.IsChecked = groupSettings.IsEnabled;
                    FragmentDoH_Settings_LookupDomain_TextBox.Text = groupSettings.LookupDomain;
                    FragmentDoH_Settings_TimeoutSec_NumericUpDown.Value = groupSettings.ScanTimeoutSec;
                    FragmentDoH_Settings_ParallelSize_NumericUpDown.Value = groupSettings.ScanParallelSize;
                    FragmentDoH_Settings_BootstrapIP_TextBox.Text = groupSettings.BootstrapIP.ToString();
                    FragmentDoH_Settings_BootstrapPort_NumericUpDown.Value = groupSettings.BootstrapPort;
                    FragmentDoH_Settings_AllowInsecure_ToggleSwitch.IsChecked = groupSettings.AllowInsecure;

                    Flyout_FragmentDoH_Source.IsEnabled = groupSettings.IsEnabled;
                    Flyout_FragmentDoH_Options.IsEnabled = groupSettings.IsEnabled;
                    Flyout_FragmentDoH_DnsServers.IsEnabled = groupSettings.IsEnabled;
                    Flyout_FragmentDoH_Scan.IsEnabled = groupSettings.IsEnabled;
                });
                if (readOnlyGroupSettings) return;

                // Get Source Enable
                bool isEnable = MainWindow.DnsManager.Get_Source_EnableDisable(group.Name);
                this.DispatchIt(() => FragmentDoHSourceByUrlByManualToggleSwitch.IsChecked = !isEnable);

                // Get URLs
                List<string> subs = MainWindow.DnsManager.Get_Source_URLs(group.Name);
                this.DispatchIt(() => FragmentDoHSourceTextBox.Text = subs.ToString(NL));

                // Get Options
                FragmentDoHOptions options = MainWindow.DnsManager.Get_FragmentDoH_Options(group.Name);
                FragmentDoHUpdateSourceTextBlock.Clear();
                FragmentDoHUpdateSourceTextBlock.AppendText("Last Update: ", null, $"{options.AutoUpdate.LastUpdateSource:yyyy/MM/dd HH:mm:ss}", greenBrush);
                FragmentDoHScanServersTextBlock.Clear();
                FragmentDoHScanServersTextBlock.AppendText("Last Scan: ", null, $"{options.AutoUpdate.LastScanServers:yyyy/MM/dd HH:mm:ss}", greenBrush);

                this.DispatchIt(() =>
                {
                    // AutoUpdate
                    FragmentDoHUpdateSourceNumericUpDown.Value = options.AutoUpdate.UpdateSource;
                    FragmentDoHScanServersNumericUpDown.Value = options.AutoUpdate.ScanServers;

                    // FilterByProperties
                    FragmentDoHFilter_Google_CheckBox.IsChecked = Tools.DnsFilterToBool(options.FilterByProperties.GoogleSafeSearch);
                    FragmentDoHFilter_Bing_CheckBox.IsChecked = Tools.DnsFilterToBool(options.FilterByProperties.BingSafeSearch);
                    FragmentDoHFilter_Youtube_CheckBox.IsChecked = Tools.DnsFilterToBool(options.FilterByProperties.YoutubeRestricted);
                    FragmentDoHFilter_Adult_CheckBox.IsChecked = Tools.DnsFilterToBool(options.FilterByProperties.AdultBlocked);

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
                DnsItemsInfo info = MainWindow.DnsManager.Get_DnsItems_Info(group.Name);
                FragmentDoHSource_Info_TextBlock.Clear();
                FragmentDoHSource_Info_TextBlock.AppendText("Total Servers: ", null, $"{info.TotalServers}", Brushes.Orange);
                FragmentDoHFilter_Info_TextBlock.Clear();
                FragmentDoHFilter_Info_TextBlock.AppendText("Total Servers: ", null, $"{info.TotalServers}", Brushes.Orange, $",{LS}Online Servers: ", null, $"{info.OnlineServers}", Brushes.Orange);
                FragmentDoHFilter_Info_TextBlock.AppendText($",{LS}Selected Servers: ", null, $"{info.SelectedServers}", Brushes.Orange, $",{LS}Average Latency: ", null, $"{info.AverageLatency}", Brushes.Orange, "ms", null);
            }
            else if (group.Mode == GroupMode.Custom)
            {
                // Get Group Settings
                GroupSettings groupSettings = MainWindow.DnsManager.Get_GroupSettings(group.Name);
                this.DispatchIt(() =>
                {
                    Custom_EnableGroup_ToggleSwitch.IsChecked = groupSettings.IsEnabled;
                    Custom_Settings_LookupDomain_TextBox.Text = groupSettings.LookupDomain;
                    Custom_Settings_TimeoutSec_NumericUpDown.Value = groupSettings.ScanTimeoutSec;
                    Custom_Settings_ParallelSize_NumericUpDown.Value = groupSettings.ScanParallelSize;
                    Custom_Settings_BootstrapIP_TextBox.Text = groupSettings.BootstrapIP.ToString();
                    Custom_Settings_BootstrapPort_NumericUpDown.Value = groupSettings.BootstrapPort;
                    Custom_Settings_AllowInsecure_ToggleSwitch.IsChecked = groupSettings.AllowInsecure;

                    Flyout_Custom_Source.IsEnabled = groupSettings.IsEnabled;
                    Flyout_Custom_Options.IsEnabled = groupSettings.IsEnabled;
                    Flyout_Custom_DnsServers.IsEnabled = groupSettings.IsEnabled;
                    Flyout_Custom_Scan.IsEnabled = groupSettings.IsEnabled;
                });
                if (readOnlyGroupSettings) return;

                // Get URLs
                List<string> subs = MainWindow.DnsManager.Get_Source_URLs(group.Name);
                this.DispatchIt(() => CustomSourceTextBox.Text = subs.ToString(NL));

                // Get Options
                CustomOptions options = MainWindow.DnsManager.Get_Custom_Options(group.Name);
                CustomScanServersTextBlock.Clear();
                CustomScanServersTextBlock.AppendText("Last Scan: ", null, $"{options.AutoUpdate.LastScanServers:yyyy/MM/dd HH:mm:ss}", greenBrush);

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
                    CustomFilter_Google_CheckBox.IsChecked = Tools.DnsFilterToBool(options.FilterByProperties.GoogleSafeSearch);
                    CustomFilter_Bing_CheckBox.IsChecked = Tools.DnsFilterToBool(options.FilterByProperties.BingSafeSearch);
                    CustomFilter_Youtube_CheckBox.IsChecked = Tools.DnsFilterToBool(options.FilterByProperties.YoutubeRestricted);
                    CustomFilter_Adult_CheckBox.IsChecked = Tools.DnsFilterToBool(options.FilterByProperties.AdultBlocked);
                });

                // Get DnsItems Info
                DnsItemsInfo info = MainWindow.DnsManager.Get_DnsItems_Info(group.Name);
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
            Debug.WriteLine("ManageDnsWindow LoadSelectedGroupAsync: " + ex.Message);
        }
    }

    private async void WpfWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Enable Cross Access To This Collection
            BindingOperations.EnableCollectionSynchronization(MainWindow.DnsManager.Model.Groups, SyncLock);
            DGG.ItemsSource = MainWindow.DnsManager.Model.Groups;

            // Hide ServersTabControl Header
            ServersTabControl.HideHeader();

            // Load Theme

            // Wait For Background Task
            await Task.Run(async () =>
            {
                while (true)
                {
                    bool isBackgroundTaskWorking = MainWindow.DnsManager.IsBackgroundTaskWorking;
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

            if (PART_Button1 != null)
            {
                PART_Button1.Content = "Malicious Domains";
                PART_Button1.Visibility = Visibility.Visible;
                PART_Button1.Click -= PART_Button1_Click;
                PART_Button1.Click += PART_Button1_Click;
            }
            
            // Set Helps
            Help_Groups_Import.Content = $"Restore Your Backup.{NL}Import Groups From File.";
            Help_Groups_Export.Content = $"Backup Your Groups.{NL}Export Groups To File.";
            string helpScan = $"\u2022 You Really Don't Need To Scan Servers!{NL}\u2022 The App Will Automatically Scan Them In The Background.{NL}\u2022 You Can Adjust Scan Time In";
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

            // Create DnsItem Columns: DGS
            await CreateDnsItemColumns_Async(DGS_Subscription);
            await CreateDnsItemColumns_Async(DGS_AnonDNSCrypt);
            await CreateDnsItemColumns_FragmentDoH_Async(DGS_FragmentDoH);
            await CreateDnsItemColumns_Async(DGS_Custom);

            // Load Groups: DGG
            await SelectGroupAsync(0);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow WpfWindow_Loaded: " + ex.Message);
        }
    }

    private void PART_Button1_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (IsScanning)
            {
                WpfToastDialog.Show(this, "Can't Do This While Scanning.", MessageBoxImage.Stop, 2);
                return;
            }

            MaliciousWindow mw = new()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Title = "Malicious Domains"
            };
            mw.ShowDialog();
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show(this, ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void WpfWindow_ContentRendered(object sender, EventArgs e)
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

            MainWindow.DnsManager.OnBackgroundUpdateReceived -= DnsManager_OnBackgroundUpdateReceived;
            MainWindow.DnsManager.OnBackgroundUpdateReceived += DnsManager_OnBackgroundUpdateReceived;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow WpfWindow_ContentRendered: " + ex.Message);
        }
    }

    private async void DnsManager_OnBackgroundUpdateReceived(object? sender, BackgroundWorkerEventArgs e)
    {
        //Debug.WriteLine("On Background Update Received");
        await UpdateUIByBackgroundWorkerAsync(e);
    }
    
    private async Task UpdateUIByBackgroundWorkerAsync(BackgroundWorkerEventArgs bw)
    {
        try
        {
            if (DGG.SelectedItem is not DnsGroup group) return;
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
                    await LoadSelectedGroupAsync(); // Refresh
                    if (bw.LastIndex != -1) await DGS_Subscription.ScrollIntoViewAsync(bw.LastIndex); // Scroll
                }

                DGS_Subscription.FillCustomColumn(1, 200);
            }
            else if (bw.Group.Mode == GroupMode.AnonymizedDNSCrypt)
            {
                // Scan AnonymizedDNSCrypt
                ChangeControlsState_AnonDNSCrypt(!bw.IsWorking);
                this.DispatchIt(() =>
                {
                    if (bw.IsWorking)
                    {
                        //DGG.IsHitTestVisible = !bw.IsWorking;
                        //DGS_AnonDNSCrypt.IsHitTestVisible = !bw.IsWorking;
                        AnonDNSCryptRelayTextBox.IsEnabled = !bw.IsWorking;
                        AnonDNSCryptTargetTextBox.IsEnabled = !bw.IsWorking;
                    }
                    else
                    {
                        //DGG.IsHitTestVisible = !bw.IsWorking;
                        //DGS_AnonDNSCrypt.IsHitTestVisible = !bw.IsWorking;
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
                    await LoadSelectedGroupAsync(); // Refresh
                    if (bw.LastIndex != -1) await DGS_AnonDNSCrypt.ScrollIntoViewAsync(bw.LastIndex); // Scroll
                }

                DGS_AnonDNSCrypt.FillCustomColumn(1, 200);
            }
            else if (bw.Group.Mode == GroupMode.FragmentDoH)
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
                    await LoadSelectedGroupAsync(); // Refresh
                    if (bw.LastIndex != -1) await DGS_FragmentDoH.ScrollIntoViewAsync(bw.LastIndex); // Scroll
                }

                DGS_FragmentDoH.FillCustomColumn(1, 200);
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
            Debug.WriteLine("ManageDnsWindow UpdateUIByBackgroundWorker: " + ex.Message);
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
            Debug.WriteLine("ManageDnsWindow NewGroupButton_PreviewMouseUp: " + ex.Message);
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
            bool imported = await MainWindow.DnsManager.Reset_BuiltIn_Groups_Async(true);
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
            Debug.WriteLine("ManageDnsWindow ResetBuiltInButton_Click: " + ex.Message);
            IsEnabled = true;
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
            Debug.WriteLine("ManageDnsWindow Import_FlyoutOverlay_FlyoutChanged: " + ex.Message);
        }
    }

    private async void ImportBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ImportBrowseButton.IsEnabled = false;

            OpenFileDialog ofd = new()
            {
                Filter = "DNSveil DNS Servers (*.dvls)|*.dvls",
                DefaultExt = ".dvls",
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
                DnsExportImport? exportImport = await JsonTool.DeserializeAsync<DnsExportImport>(json);
                if (exportImport == null)
                {
                    WpfToastDialog.Show(this, "ERROR: JSON Deserialization Failed.", MessageBoxImage.Error, 2);
                    ImportBrowseButton.IsEnabled = true;
                    return;
                }

                if (exportImport.Settings == null && exportImport.Groups.Count == 0)
                {
                    string msg = $"{Path.GetExtension(filePath).ToUpperInvariant()} File Has No Settings Or Groups!";
                    WpfMessageBox.Show(this, msg, "No Settings/Groups", MessageBoxButton.OK, MessageBoxImage.Stop);
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
            Debug.WriteLine("ManageDnsWindow ImportBrowseButton_Click: " + ex.Message);
        }
    }

    private void Import_CheckBox_CheckedUnchecked(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not CheckBox cb) return;
            if (!cb.IsChecked.HasValue) return;
            if (Import_ListBox.ItemsSource is not List<DnsGroup> groups) return;
            if (Import_ListBox.Tag is not DnsExportImport exportImport) return;

            // Modify Based On User Selection
            for (int n = 0; n < groups.Count; n++)
            {
                DnsGroup group = groups[n];
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
            Debug.WriteLine("ManageDnsWindow Import_CheckBox_CheckedUnchecked: " + ex.Message);
        }
    }

    private async void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Import_ListBox.Tag is not DnsExportImport exportImport)
            {
                WpfToastDialog.Show(this, "Browse Your Backup File.", MessageBoxImage.Stop, 2);
                return;
            }

            ImportButton.IsEnabled = false;

            bool importSettings = Import_Settings_CheckBox.IsChecked.HasValue && Import_Settings_CheckBox.IsChecked.Value;

            List<DnsGroup> groupsToImport = new();
            for (int n = 0; n < exportImport.Groups.Count; n++)
            {
                DnsGroup group = exportImport.Groups[n];
                if (group.IsSelected)
                {
                    group.IsBuiltIn = false; // Imported Groups Can't Be Built-In If Modified On File.
                    groupsToImport.Add(group);
                }
            }

            if (!importSettings && groupsToImport.Count == 0)
            {
                WpfToastDialog.Show(this, "Select Settings Or At Least One Group To Import.", MessageBoxImage.Stop, 3);
                ImportButton.IsEnabled = true;
                return;
            }

            // Import Settings
            bool isImportSettingsSuccess = false;
            if (importSettings && exportImport.Settings != null)
                isImportSettingsSuccess = await MainWindow.DnsManager.Update_Settings_Async(exportImport.Settings, false);

            // Import Groups
            string lastImportedGroupName = string.Empty;
            for (int n = 0; n < groupsToImport.Count; n++)
            {
                DnsGroup groupToImport = groupsToImport[n];
                lastImportedGroupName = await MainWindow.DnsManager.Add_Group_Async(groupToImport, false, false);
            }

            if (isImportSettingsSuccess || !string.IsNullOrEmpty(lastImportedGroupName))
            {
                await MainWindow.DnsManager.SaveAsync(); // Save
                await SelectGroupAsync(lastImportedGroupName); // Refresh

                ImportButton.IsEnabled = true;
                await Import_FlyoutOverlay.CloseFlyAsync();

                string msg = "Settings Or Selected Groups Imported Successfully.";
                WpfMessageBox.Show(this, msg, "Imported", MessageBoxButton.OK);
            }
            else
            {
                ImportButton.IsEnabled = true;
                await Import_FlyoutOverlay.CloseFlyAsync();

                string msg = "Settings Or A Group Didn't Import!";
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
            Debug.WriteLine("ManageDnsWindow ImportButton_Click: " + ex.Message);
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
                List<DnsGroup> groups = MainWindow.DnsManager.Get_Groups(false);
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
            Debug.WriteLine("ManageDnsWindow Export_FlyoutOverlay_FlyoutChanged: " + ex.Message);
        }
    }

    private void Export_CheckBox_CheckedUnchecked(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not CheckBox cb) return;
            if (!cb.IsChecked.HasValue) return;
            if (Export_ListBox.ItemsSource is not List<DnsGroup> groups) return;

            // Modify Based On User Selection
            for (int n = 0; n < groups.Count; n++)
            {
                DnsGroup group = groups[n];
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
            Debug.WriteLine("ManageDnsWindow Export_CheckBox_CheckedUnchecked: " + ex.Message);
        }
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Export_ListBox.ItemsSource is not List<DnsGroup> groups) return;
            ExportButton.IsEnabled = false;

            bool exportSettings = Export_Settings_CheckBox.IsChecked.HasValue && Export_Settings_CheckBox.IsChecked.Value;

            List<DnsGroup> groupsToExport = new();
            for (int n = 0; n < groups.Count; n++)
            {
                DnsGroup group = groups[n];
                if (group.IsSelected)
                {
                    group.IsBuiltIn = false; // Export As User
                    groupsToExport.Add(group);
                }
            }

            if (!exportSettings && groupsToExport.Count == 0)
            {
                WpfToastDialog.Show(this, "Select Settings Or At Least One Group To Export.", MessageBoxImage.Stop, 3);
                ExportButton.IsEnabled = true;
                return;
            }

            // Create Model
            DnsExportImport exportImport = new()
            {
                Settings = exportSettings ? MainWindow.DnsManager.Model.Settings : null,
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
                Filter = "DNSveil DNS Servers (*.dvls)|*.dvls",
                DefaultExt = ".dvls",
                AddExtension = true,
                RestoreDirectory = true,
                FileName = $"DNSveil_DNS_Servers_{DateTime.Now:yyyy.MM.dd-HH.mm.ss}"
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
            Debug.WriteLine("ManageDnsWindow ExportButton_Click: " + ex.Message);
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

            List<string> groupNames = MainWindow.DnsManager.Get_Group_Names(false);
            if (groupNames.IsContain(newGroupName))
            {
                string msg = $"\"{newGroupName}\" Is Already Exist, Choose Another Name.";
                WpfMessageBox.Show(this, msg);
                return;
            }

            await MainWindow.DnsManager.Add_Group_Async(new DnsGroup(newGroupName, groupMode), false, true);
            await SelectGroupAsync(newGroupName);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow MenuNewGroup_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not DnsGroup group) return;

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

            List<string> groupNames = MainWindow.DnsManager.Get_Group_Names(false);
            if (groupNames.IsContain(newGroupName))
            {
                string msg = $"\"{newGroupName}\" Is Already Exist, Choose Another Name.";
                WpfMessageBox.Show(this, msg);
                return;
            }

            // Get Old Name
            await MainWindow.DnsManager.Rename_Group_Async(group.Name, newGroupName, true);
            await SelectGroupAsync(newGroupName);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow MenuItem_Group_Rename_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not DnsGroup group) return;

            string msg = $"Deleting \"{group.Name}\" Group...{NL}Continue?";
            MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbr != MessageBoxResult.Yes) return;

            int nextIndex = GetPreviousOrNextIndex(DGG, true); // Get Next Index
            await MainWindow.DnsManager.Remove_Group_Async(group.Name, true); // Remove
            await SelectGroupAsync(nextIndex); // Refresh
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow MenuItem_Group_Remove_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not DnsGroup group) return;

            // Get Previous Index
            int index = -1;
            if (DGG.SelectedIndex > 0) index = DGG.SelectedIndex - 1;
            if (index != -1)
            {
                await MainWindow.DnsManager.Move_Group_Async(group.Name, index, true); // Move
                await SelectGroupAsync(group.Name); // Select
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow MenuItem_Group_MoveUp_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not DnsGroup group) return;

            // Get Next Index
            int index = -1;
            if (DGG.SelectedIndex < DGG.Items.Count - 1) index = DGG.SelectedIndex + 1;
            if (index != -1)
            {
                await MainWindow.DnsManager.Move_Group_Async(group.Name, index, true); // Move
                await SelectGroupAsync(group.Name); // Select
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow MenuItem_Group_MoveDown_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not DnsGroup group) return;

            if (DGG.Items.Count > 0 && DGG.SelectedIndex != 0)
            {
                await MainWindow.DnsManager.Move_Group_Async(group.Name, 0, true); // Move
                await SelectGroupAsync(group.Name); // Select
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow MenuItem_Group_MoveToTop_Click: " + ex.Message);
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

            if (DGG.SelectedItem is not DnsGroup group) return;

            if (DGG.Items.Count > 0 && DGG.SelectedIndex != DGG.Items.Count - 1)
            {
                await MainWindow.DnsManager.Move_Group_Async(group.Name, DGG.Items.Count - 1, true); // Move
                await SelectGroupAsync(group.Name); // Select
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow MenuItem_Group_MoveToBottom_Click: " + ex.Message);
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
                    if (DGG.SelectedItem is DnsGroup group)
                    {
                        string msg = $"Deleting All DNS Items/Targets/Relays...{NL}Continue?";
                        MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Delete All", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (mbr == MessageBoxResult.Yes)
                        {
                            if (group.Mode == GroupMode.AnonymizedDNSCrypt)
                            {
                                await MainWindow.DnsManager.Clear_AnonDNSCrypt_RelayItems_Async(group.Name, false);
                                await MainWindow.DnsManager.Clear_AnonDNSCrypt_TargetItems_Async(group.Name, false);
                            }
                            await MainWindow.DnsManager.Clear_DnsItems_Async(group.Name, true);
                            await LoadSelectedGroupAsync(); // Refresh
                        }
                    }
                }
                // Set Group As Built-In
                else if (e.Key == Key.B)
                {
                    if (DGG.SelectedItem is DnsGroup group)
                    {
                        string msg = $"Setting Group \"{group.Name}\" As Built-In...{NL}Continue?";
                        MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Set As Built-In", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (mbr == MessageBoxResult.Yes)
                        {
                            await MainWindow.DnsManager.Update_Group_As_BuiltIn_Async(group.Name, true, true);
                        }
                    }
                }
                // Set Group As User
                else if (e.Key == Key.U)
                {
                    if (DGG.SelectedItem is DnsGroup group)
                    {
                        string msg = $"Setting Group \"{group.Name}\" As User...{NL}Continue?";
                        MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Set As User", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (mbr == MessageBoxResult.Yes)
                        {
                            await MainWindow.DnsManager.Update_Group_As_BuiltIn_Async(group.Name, false, true);
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
            Debug.WriteLine("ManageDnsWindow DGG_PreviewKeyDown: " + ex.Message);
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
            Debug.WriteLine("ManageDnsWindow DGG_PreviewMouseRightButtonDown: " + ex.Message);
        }
    }

    private async void DGG_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        await BindItemsAsync(DGG.SelectedIndex);
        await LoadSelectedGroupAsync();
    }

    private async Task CreateDnsItemColumns_Async(DataGrid dg)
    {
        try
        {
            DnsItem di;
            dg.Columns.Clear();
            dg.AutoGenerateColumns = false;

            DataGridCheckBoxColumn c_Selected = new()
            {
                Header = '\u2714',
                Binding = new Binding(nameof(di.IsSelected)),
                IsReadOnly = true,
                CanUserResize = false,
                ElementStyle = TryFindResource("DataGridCheckBoxColumn_ReadOnlyStyle") as Style,
                EditingElementStyle = TryFindResource("DataGridCheckBoxColumn_ReadOnlyStyle") as Style
            };
            dg.Columns.Add(c_Selected);

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
                Binding = new Binding(nameof(di.ProtocolName)),
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
            c_DNS.Width = dg.FillCustomColumn(1, 200);
            await Task.Delay(50);
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show(this, ex.Message, "Something Went Wrong!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task CreateDnsItemColumns_FragmentDoH_Async(DataGrid dg)
    {
        try
        {
            DnsItem fi;
            dg.Columns.Clear();
            dg.AutoGenerateColumns = false;

            DataGridCheckBoxColumn c_Selected = new()
            {
                Header = '\u2714',
                Binding = new Binding(nameof(fi.IsSelected)),
                IsReadOnly = true,
                CanUserResize = false,
                ElementStyle = TryFindResource("DataGridCheckBoxColumn_ReadOnlyStyle") as Style,
                EditingElementStyle = TryFindResource("DataGridCheckBoxColumn_ReadOnlyStyle") as Style
            };
            dg.Columns.Add(c_Selected);

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
                Binding = new Binding(nameof(fi.ProtocolName)),
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
            c_DoH_URL.Width = dg.FillCustomColumn(1, 200);
            await Task.Delay(50);
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show(this, ex.Message, "Something Went Wrong!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
            Debug.WriteLine("ManageDnsWindow DGS_DnsServers_PreviewKeyDown: " + ex.Message);
        }
    }

    private void DGS_DnsServers_SelectionChanged(DataGrid dg)
    {
        if (DGG.SelectedItem is not DnsGroup group) return;
        if (dg.SelectedItem is not DnsItem item) return;

        try
        {
            // Set Header
            string header = $"Group: \"{group.Name}\", Mode: \"{Get_GroupModeName(group.Mode)}\" - DNS No: {dg.SelectedIndex + 1}/{dg.Items.Count}";
            this.DispatchIt(() => ServersTitleGroupBox.Header = header);

            // Set URL And IP To Manual Edit
            if (group.Mode == GroupMode.FragmentDoH)
            {
                this.DispatchIt(() =>
                {
                    FragmentDoHByManualUrlTextBox.Text = item.DNS_URL;
                    FragmentDoHByManualIpTextBox.Text = item.DNS_IP_Str;
                });
            }
            else if (group.Mode == GroupMode.Custom)
            {
                this.DispatchIt(() =>
                {
                    CustomByManual_DnsAddress_TextBox.Text = item.DNS_URL;
                    CustomByManual_DnsDescription_TextBox.Text = item.Description;
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow DGS_DnsServers_SelectionChanged: " + ex.Message);
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
            Debug.WriteLine("ManageDnsWindow SourceBrowseButton: " + ex.Message);
        }
    }

    private void MenuItem_All_CopyToCustom_Handler(MenuItem menuItem_CopyToCustom, ObservableCollection<DnsItem> selectedItems)
    {
        try
        {
            if (DGG.SelectedItem is not DnsGroup group) return;
            List<string> customGroups = new();
            List<string> allGroups = MainWindow.DnsManager.Get_Group_Names(false);
            for (int n = 0; n < allGroups.Count; n++)
            {
                string groupName = allGroups[n];
                GroupMode mode = MainWindow.DnsManager.Get_GroupMode_ByName(groupName);
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
            Debug.WriteLine("ManageDnsWindow MenuItem_All_CopyToCustom_Handler: " + ex.Message);
        }
    }

    private async void MenuItem_CopyToCustom_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem menuItem) return;
            if (menuItem.Header is not string toGroup) return;
            if (menuItem.Tag is not ObservableCollection<DnsItem> selectedItems) return;

            await MainWindow.DnsManager.Append_DnsItems_Async(toGroup, selectedItems, true);

            WpfToastDialog.Show(this, $"Copied To {toGroup}.", MessageBoxImage.None, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageDnsWindow MenuItem_CopyToCustom_Click: " + ex.Message);
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
                if (MainWindow.DnsManager.IsScanning)
                {
                    MainWindow.DnsManager.ScanServers(new DnsGroup(), null);
                    await Task.Run(async () =>
                    {
                        while (true)
                        {
                            this.DispatchIt(() => IsHitTestVisible = false);
                            NoGroupText_TextBlock.SetText("Stopping Scan...");
                            ServersTabControl.DispatchIt(() => ServersTabControl.SelectedIndex = 0);
                            await Task.Delay(100);
                            if (!MainWindow.DnsManager.IsScanning) break;
                        }
                    });
                }
                // Save
                await MainWindow.DnsManager.SaveAsync();
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
            Debug.WriteLine("ManageDnsWindow WpfWindow_Closing: " + ex.Message);
        }
    }

    
}