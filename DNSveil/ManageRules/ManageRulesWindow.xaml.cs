using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DNSveil.Logic;
using Microsoft.Win32;
using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using MsmhToolsWpfClass;
using MsmhToolsWpfClass.Themes;

namespace DNSveil.ManageRules;

/// <summary>
/// Interaction logic for ManageRulesWindow.xaml
/// </summary>
public partial class ManageRulesWindow : WpfWindow
{
    private static readonly string NL = Environment.NewLine;
    private bool IsWorking = false;
    public ObservableCollection<AgnosticProgram.Rules.Rule> RuleList_OC { get; set; } = new();

    // Context Menu Variables
    private MenuItem MenuItem_Vari_Copy = new();
    private MenuItem MenuItem_Vari_Delete = new();
    private MenuItem MenuItem_Vari_DeleteAll = new();
    private ContextMenu ContextMenu_Vari = new();

    // Context Menu Rules
    private MenuItem MenuItem_Rules_MoveUp = new();
    private MenuItem MenuItem_Rules_MoveDown = new();
    private MenuItem MenuItem_Rules_MoveToTop = new();
    private MenuItem MenuItem_Rules_MoveToBottom = new();
    private MenuItem MenuItem_Rules_Delete = new();
    private MenuItem MenuItem_Rules_DeleteAll = new();
    private ContextMenu ContextMenu_Rules = new();

    public ManageRulesWindow()
    {
        InitializeComponent();

        // Invariant Culture
        Info.SetCulture(CultureInfo.InvariantCulture);

        SaveAuto();
    }

    private async void SaveAuto()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMinutes(2));
                if (!IsWorking)
                {
                    await MainWindow.Rules.SaveAsync();
                }
            }
        });
    }

    private void Manage_Rule_Set(AgnosticProgram.Rules.Rule rule)
    {
        try
        {
            static string v2n(string v, string separator)
            {
                return AgnosticProgram.Rules_Init.Vari_ValuesToNames(v, MainWindow.Rules.Variables, separator);
            }

            this.DispatchIt(() =>
            {
                Address_TextBox.Text = rule.Address;
                Block_ToggleSwitch.IsChecked = rule.IsBlock;
                BlockPorts_TextBox.Text = v2n(rule.BlockPort.ToString(", "), ", ");
                FakeDNS_TextBox.Text = v2n(rule.FakeDnsIP, string.Empty);
                DNSs_TextBox.Text = v2n(rule.Dnss.ToString(NL), NL);
                DnsDomain_TextBox.Text = v2n(rule.DnsDomain, string.Empty);
                DnsProxyScheme_TextBox.Text = v2n(rule.DnsProxyScheme, string.Empty);
                DnsProxyUser_TextBox.Text = v2n(rule.DnsProxyUser, string.Empty);
                DnsProxyPass_TextBox.Text = v2n(rule.DnsProxyPass, string.Empty);
                Direct_ToggleSwitch.IsChecked = rule.IsDirect;
                SNI_TextBox.Text = v2n(rule.Sni, string.Empty);
                ProxyScheme_TextBox.Text = v2n(rule.ProxyScheme, string.Empty);
                ProxyUser_TextBox.Text = v2n(rule.ProxyUser, string.Empty);
                ProxyPass_TextBox.Text = v2n(rule.ProxyPass, string.Empty);
            });
        }
        catch (Exception) { }
    }

    private AgnosticProgram.Rules.Rule Manage_Rule_Get()
    {
        AgnosticProgram.Rules.Rule rule = new();
        try
        {
            static string n2v(string n, string separator)
            {
                return AgnosticProgram.Rules_Init.Vari_NamesToValues(n, MainWindow.Rules.Variables, separator);
            }

            this.DispatchIt(() =>
            {
                rule.Address = Address_TextBox.Text.Trim();
                rule.IsBlock = Block_ToggleSwitch.IsChecked.HasValue && Block_ToggleSwitch.IsChecked.Value;
                string[] blockPorts = n2v(BlockPorts_TextBox.Text.Trim(), ",").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                for (int n = 0; n < blockPorts.Length; n++)
                {
                    string blockPort = blockPorts[n];
                    bool isInt = int.TryParse(blockPort, out int blockPortOut);
                    if (isInt) rule.BlockPort.Add(blockPortOut);
                }
                rule.FakeDnsIP = n2v(FakeDNS_TextBox.Text.Trim(), string.Empty);
                string[] dnss = n2v(DNSs_TextBox.Text.Trim(), NL).Split(NL, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                rule.Dnss.AddRange(dnss);
                rule.DnsDomain = n2v(DnsDomain_TextBox.Text.Trim(), string.Empty);
                rule.DnsProxyScheme = n2v(DnsProxyScheme_TextBox.Text.Trim(), string.Empty);
                rule.DnsProxyUser = n2v(DnsProxyUser_TextBox.Text.Trim(), string.Empty);
                rule.DnsProxyPass = n2v(DnsProxyPass_TextBox.Text.Trim(), string.Empty);
                rule.IsDirect = Direct_ToggleSwitch.IsChecked.HasValue && Direct_ToggleSwitch.IsChecked.Value;
                rule.Sni = n2v(SNI_TextBox.Text.Trim(), string.Empty);
                rule.ProxyScheme = n2v(ProxyScheme_TextBox.Text.Trim(), string.Empty);
                rule.ProxyUser = n2v(ProxyUser_TextBox.Text.Trim(), string.Empty);
                rule.ProxyPass = n2v(ProxyPass_TextBox.Text.Trim(), string.Empty);
            });
        }
        catch (Exception) { }
        return rule;
    }

    private void ChangeEnable_Rule_Fields()
    {
        try
        {
            AgnosticProgram.Rules.Rule rule = Manage_Rule_Get();
            AgnosticProgram.Rules.AddressType at = rule.AddressType;

            this.DispatchIt(() =>
            {
                bool isAddressEmpty = string.IsNullOrEmpty(Address_TextBox.Text.Trim());
                Block_ToggleSwitch.IsEnabled = !isAddressEmpty;
                BlockPorts_TextBox.IsEnabled = !isAddressEmpty && !rule.IsBlock;
                FakeDNS_TextBox.IsEnabled = !isAddressEmpty && !rule.IsBlock;
                bool isFakeDnsEmpty = string.IsNullOrEmpty(FakeDNS_TextBox.Text.Trim());
                DNSs_TextBox.IsEnabled = !isAddressEmpty && !rule.IsBlock && isFakeDnsEmpty && at == AgnosticProgram.Rules.AddressType.Domain;
                DnsDomain_TextBox.IsEnabled = !isAddressEmpty && !rule.IsBlock && isFakeDnsEmpty && at == AgnosticProgram.Rules.AddressType.Domain;
                DnsProxyScheme_TextBox.IsEnabled = !isAddressEmpty && !rule.IsBlock && isFakeDnsEmpty && at == AgnosticProgram.Rules.AddressType.Domain;
                bool isDnsProxyEmpty = string.IsNullOrEmpty(DnsProxyScheme_TextBox.Text.Trim());
                DnsProxyUser_TextBox.IsEnabled = DnsProxyScheme_TextBox.IsEnabled && !isDnsProxyEmpty;
                DnsProxyPass_TextBox.IsEnabled = DnsProxyScheme_TextBox.IsEnabled && !isDnsProxyEmpty;
                Direct_ToggleSwitch.IsEnabled = !isAddressEmpty && !rule.IsBlock;
                SNI_TextBox.IsEnabled = !isAddressEmpty && !rule.IsBlock && !rule.IsDirect && at == AgnosticProgram.Rules.AddressType.Domain;
                ProxyScheme_TextBox.IsEnabled = !isAddressEmpty && !rule.IsBlock && !rule.IsDirect;
                bool isProxyEmpty = string.IsNullOrEmpty(ProxyScheme_TextBox.Text.Trim());
                ProxyUser_TextBox.IsEnabled = ProxyScheme_TextBox.IsEnabled && !isProxyEmpty;
                ProxyPass_TextBox.IsEnabled = ProxyScheme_TextBox.IsEnabled && !isProxyEmpty;
            });
        }
        catch (Exception) { }
    }

    private async Task Create_DGV_Columns_Async(DataGrid dg)
    {
        try
        {
            dg.Columns.Clear();
            dg.AutoGenerateColumns = false;

            DataGridTextColumn c_Var = new()
            {
                Header = "Name",
                Binding = new Binding("Item1"), // Tuple Item1
                IsReadOnly = true,
                CanUserResize = false
            };
            dg.Columns.Add(c_Var);

            DataGridTextColumn c_Value = new()
            {
                Header = "Value",
                Binding = new Binding("Item2"), // Tuple Item2
                IsReadOnly = true,
                CanUserResize = false
            };
            dg.Columns.Add(c_Value);

            dg.FillLastColumn();
            await Task.Delay(50);
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show(this, ex.Message, "Something Went Wrong!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task Create_DGR_Columns_Async(DataGrid dg)
    {
        try
        {
            dg.Columns.Clear();
            dg.AutoGenerateColumns = false;

            DataGridTextColumn c_Address = new()
            {
                Header = "Address",
                Binding = new Binding(nameof(AgnosticProgram.Rules.Rule.Address)),
                IsReadOnly = true,
                CanUserResize = false
            };
            dg.Columns.Add(c_Address);

            DataGridTextColumn c_AddressType = new()
            {
                Header = "Type",
                Binding = new Binding(nameof(AgnosticProgram.Rules.Rule.AddressType)),
                IsReadOnly = true,
                CanUserResize = false
            };
            dg.Columns.Add(c_AddressType);

            DataGridTextColumn c_Report = new()
            {
                Header = "Report",
                Binding = new Binding(nameof(AgnosticProgram.Rules.Rule.Report)),
                IsReadOnly = true,
                CanUserResize = false
            };
            dg.Columns.Add(c_Report);

            await Task.Delay(50);
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show(this, ex.Message, "Something Went Wrong!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ReadAsync(bool saveToFile)
    {
        try
        {
            // Bind Data Source
            int indexDGV = DGV.SelectedIndex;
            int indexDGR = DGR.SelectedIndex;
            await Task.Delay(2);
            DGV.ItemsSource = MainWindow.Rules.Variables.ToList();

            RuleList_OC = new(MainWindow.Rules.RuleList); //  To Make Move-Up And Move-Down Fast
            DGR.ItemsSource = RuleList_OC;

            if (saveToFile)
            {
                await Task.Delay(5);
                bool isSaveSuccess = await MainWindow.Rules.SaveAsync();
                await Task.Delay(5);
                if (!isSaveSuccess)
                {
                    WpfMessageBox.Show(this, "Couldn't Save To File.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            await Task.Delay(1);
            this.DispatchIt(() =>
            {
                DGV.SelectedIndex = indexDGV;
                DGR.SelectedIndex = indexDGR;
            });
            await Task.Delay(1);
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show(this, ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void WpfWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Load Theme

            // Set Helps

            // Set Max Lines Of TextBoxes
            DNSs_TextBox.SetMaxLines(10, this);

            // Create Columns
            await Create_DGV_Columns_Async(DGV);
            await Create_DGR_Columns_Async(DGR);

            // Read Rules
            await MainWindow.Rules.SetAsync(AgnosticProgram.Rules.Mode.File, Pathes.Rules);

            // Read (Bind Data Source)
            await ReadAsync(false);
            
            Flyout_Variables.IsOpen = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow WpfWindow_Loaded: " + ex.Message);
        }
    }

    private void WpfWindow_ContentRendered(object sender, EventArgs e)
    {
        try
        {
            // Context Menu Variables
            MenuItem_Vari_Copy = new()
            {
                Header = "Copy Name"
            };
            MenuItem_Vari_Copy.Click -= MenuItem_Vari_Copy_Click;
            MenuItem_Vari_Copy.Click += MenuItem_Vari_Copy_Click;

            MenuItem_Vari_Delete = new()
            {
                Header = "Delete"
            };
            MenuItem_Vari_Delete.Click -= MenuItem_Vari_Delete_Click;
            MenuItem_Vari_Delete.Click += MenuItem_Vari_Delete_Click;

            MenuItem_Vari_DeleteAll = new()
            {
                Header = "Delete All"
            };
            MenuItem_Vari_DeleteAll.Click -= MenuItem_Vari_DeleteAll_Click;
            MenuItem_Vari_DeleteAll.Click += MenuItem_Vari_DeleteAll_Click;

            ContextMenu_Vari = new();
            ContextMenu_Vari.Items.Add(MenuItem_Vari_Copy);
            ContextMenu_Vari.Items.Add(MenuItem_Vari_Delete);
            ContextMenu_Vari.Items.Add(MenuItem_Vari_DeleteAll);
            DGV.ContextMenu = ContextMenu_Vari; // Set To DGV

            // Context Menu Rules
            MenuItem_Rules_MoveUp = new()
            {
                Header = "Move Up",
                InputGestureText = "Ctrl+Up"
            };
            MenuItem_Rules_MoveUp.Click -= MenuItem_Rules_MoveUp_Click;
            MenuItem_Rules_MoveUp.Click += MenuItem_Rules_MoveUp_Click;

            MenuItem_Rules_MoveDown = new()
            {
                Header = "Move Down",
                InputGestureText = "Ctrl+Down"
            };
            MenuItem_Rules_MoveDown.Click -= MenuItem_Rules_MoveDown_Click;
            MenuItem_Rules_MoveDown.Click += MenuItem_Rules_MoveDown_Click;

            MenuItem_Rules_MoveToTop = new()
            {
                Header = "Move To Top",
                InputGestureText = "Ctrl+Home"
            };
            MenuItem_Rules_MoveToTop.Click -= MenuItem_Rules_MoveToTop_Click;
            MenuItem_Rules_MoveToTop.Click += MenuItem_Rules_MoveToTop_Click;

            MenuItem_Rules_MoveToBottom = new()
            {
                Header = "Move To Bottom",
                InputGestureText = "Ctrl+End"
            };
            MenuItem_Rules_MoveToBottom.Click -= MenuItem_Rules_MoveToBottom_Click;
            MenuItem_Rules_MoveToBottom.Click += MenuItem_Rules_MoveToBottom_Click;

            MenuItem_Rules_Delete = new()
            {
                Header = "Delete Selected"
            };
            MenuItem_Rules_Delete.Click -= MenuItem_Rules_Delete_Click;
            MenuItem_Rules_Delete.Click += MenuItem_Rules_Delete_Click;

            MenuItem_Rules_DeleteAll = new()
            {
                Header = "Delete All"
            };
            MenuItem_Rules_DeleteAll.Click -= MenuItem_Rules_DeleteAll_Click;
            MenuItem_Rules_DeleteAll.Click += MenuItem_Rules_DeleteAll_Click;

            ContextMenu_Rules = new();
            ContextMenu_Rules.Items.Add(MenuItem_Rules_MoveUp);
            ContextMenu_Rules.Items.Add(MenuItem_Rules_MoveDown);
            ContextMenu_Rules.Items.Add(MenuItem_Rules_MoveToTop);
            ContextMenu_Rules.Items.Add(MenuItem_Rules_MoveToBottom);
            ContextMenu_Rules.Items.Add(MenuItem_Rules_Delete);
            ContextMenu_Rules.Items.Add(MenuItem_Rules_DeleteAll);
            DGR.ContextMenu = ContextMenu_Rules; // Set To DGR
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow WpfWindow_ContentRendered: " + ex.Message);
        }
    }

    private async Task AddModify_Rule_Async(bool addTrue_ModifyFalse)
    {
        try
        {
            AgnosticProgram.Rules.Rule rule = Manage_Rule_Get();

            if (rule.AddressType == AgnosticProgram.Rules.AddressType.None)
            {
                string msg = "Address Is Incorrect.";
                WpfMessageBox.Show(this, msg, "Stop", MessageBoxButton.OK, MessageBoxImage.Stop);
                IsWorking = false;
                return;
            }

            if (!string.IsNullOrEmpty(rule.FakeDnsIP) && !NetworkTool.IsIP(rule.FakeDnsIP, out _))
            {
                string msg = "Fake DNS Must Be An IP Address.";
                WpfMessageBox.Show(this, msg, "Stop", MessageBoxButton.OK, MessageBoxImage.Stop);
                IsWorking = false;
                return;
            }

            if (addTrue_ModifyFalse)
            {
                // Add
                bool isRuleAdded = await MainWindow.Rules.AddAsync(rule);
                if (isRuleAdded)
                {
                    // Save
                    await ReadAsync(true);

                    // Go To Last
                    await DGR.ScrollIntoViewAsync(DGR.Items.Count - 1, 1);

                    // MSG
                    string msg = "Rule Added Successfully.";
                    WpfToastDialog.Show(this, msg, MessageBoxImage.None, 2);
                }
                else
                {
                    string msg = "Address Already Exist, You Can Modify It Or Delete It.";
                    WpfMessageBox.Show(this, msg, "Stop", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }
            else
            {
                // Modify
                bool isRuleModified = await MainWindow.Rules.ModifyAsync(rule);
                if (isRuleModified)
                {
                    // Save
                    await ReadAsync(true);

                    // MSG
                    string msg = "Rule Modified Successfully.";
                    WpfToastDialog.Show(this, msg, MessageBoxImage.None, 2);
                }
                else
                {
                    string msg = "Address Doesn't Exist, You Can Add.";
                    WpfMessageBox.Show(this, msg, "Stop", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow AddModify_Rule_Async: " + ex.Message);
        }
    }


    private async void AddRuleButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsWorking) return;
        IsWorking = true;
        await AddModify_Rule_Async(true);
        IsWorking = false;
    }

    private async void ModifyRuleButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsWorking) return;
        IsWorking = true;
        await AddModify_Rule_Async(false);
        IsWorking = false;
    }

    private void ClearRuleButton_Click(object sender, RoutedEventArgs e)
    {
        Manage_Rule_Set(new AgnosticProgram.Rules.Rule());
    }

    private async void ImportReplaceButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsWorking) return;
        IsWorking = true;

        try
        {
            OpenFileDialog ofd = new()
            {
                Filter = "DNSveil Rules (*.txt)|*.txt",
                DefaultExt = ".txt",
                AddExtension = true,
                Multiselect = false,
                RestoreDirectory = true
            };

            bool? dr = ofd.ShowDialog(this);
            if (dr.HasValue && dr.Value)
            {
                // Read And Replace
                string filePath = ofd.FileName;
                AgnosticProgram.Rules rules = await AgnosticProgram.Rules.ReadAsync(AgnosticProgram.Rules.Mode.File, filePath);
                if (rules.Variables.Count > 0 || rules.RuleList.Count > 0)
                {
                    MainWindow.Rules.Variables = rules.Variables;
                    MainWindow.Rules.Default = rules.Default;
                    MainWindow.Rules.RuleList = rules.RuleList;

                    // Save
                    await ReadAsync(true);

                    // MSG
                    string msg = "Rules Replaced Successfully.";
                    WpfToastDialog.Show(this, msg, MessageBoxImage.None, 3);
                }
                else
                {
                    // MSG
                    string msg = "There Is Nothing To Import.";
                    WpfToastDialog.Show(this, msg, MessageBoxImage.None, 3);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow ImportReplaceButton_Click: " + ex.Message);
        }

        IsWorking = false;
    }

    private async void ImportMergeButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsWorking) return;
        IsWorking = true;

        try
        {
            OpenFileDialog ofd = new()
            {
                Filter = "DNSveil Rules (*.txt)|*.txt",
                DefaultExt = ".txt",
                AddExtension = true,
                Multiselect = false,
                RestoreDirectory = true
            };

            bool? dr = ofd.ShowDialog(this);
            if (dr.HasValue && dr.Value)
            {
                // Merge
                string filePath = ofd.FileName;
                var merg = await AgnosticProgram.Rules.MergeAsync(AgnosticProgram.Rules.Mode.File, Pathes.Rules, AgnosticProgram.Rules.Mode.File, filePath);
                MainWindow.Rules.Variables = merg.Variables;
                MainWindow.Rules.Default = merg.Defaults;
                MainWindow.Rules.RuleList = merg.RuleList;

                // Save
                await ReadAsync(true);

                // MSG
                string msg = "Rules Merged Successfully.";
                WpfToastDialog.Show(this, msg, MessageBoxImage.None, 3);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow ImportMergeButton_Click: " + ex.Message);
        }

        IsWorking = false;
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsWorking) return;
        IsWorking = true;

        try
        {
            SaveFileDialog sfd = new()
            {
                Filter = "DNSveil Rules (*.txt)|*.txt",
                DefaultExt = ".txt",
                AddExtension = true,
                RestoreDirectory = true,
                FileName = $"DNSveil_Rules_{DateTime.Now:yyyy.MM.dd-HH.mm.ss}"
            };

            bool? dr = sfd.ShowDialog(this);
            if (dr.HasValue && dr.Value)
            {
                bool isExportSuccess = await MainWindow.Rules.SaveToAsync(sfd.FileName);
                if (isExportSuccess)
                {
                    // MSG
                    string msg = "Rules Exported Successfully.";
                    WpfToastDialog.Show(this, msg, MessageBoxImage.None, 3);
                }
                else
                {
                    // ERROR
                    string msg = "Failed To Export Rules!";
                    WpfMessageBox.Show(this, msg, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow ExportButton_Click: " + ex.Message);
        }

        IsWorking = false;
    }

    private void Rule_TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            ChangeEnable_Rule_Fields();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow Rule_TextBox_TextChanged: " + ex.Message);
        }
    }

    private void Rule_ToggleSwitch_CheckedChanged(object sender, WpfToggleSwitch.CheckedChangedEventArgs e)
    {
        try
        {
            ChangeEnable_Rule_Fields();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow Rule_ToggleSwitch_CheckedChanged: " + ex.Message);
        }
    }

    private async Task AddModify_Variable_Async(bool addTrue_ModifyFalse)
    {
        try
        {
            string vari = string.Empty, value = string.Empty;
            this.DispatchIt(() =>
            {
                vari = Variable_Name_TextBox.Text.Trim();
                value = Variable_Value_TextBox.Text.Trim();
            });

            if (string.IsNullOrEmpty(vari) || string.IsNullOrEmpty(value))
            {
                string msg = "Variable Can't Be Empty.";
                WpfMessageBox.Show(this, msg, "Stop", MessageBoxButton.OK, MessageBoxImage.Stop);
                IsWorking = false;
                return;
            }

            if (vari.Equals(value))
            {
                string msg = "This Is Meaningless.";
                WpfMessageBox.Show(this, msg, "Stop", MessageBoxButton.OK, MessageBoxImage.Stop);
                IsWorking = false;
                return;
            }

            if (vari.Contains(' ', StringComparison.InvariantCulture) || value.Contains(' ', StringComparison.InvariantCulture))
            {
                string msg = "Variable Can't Contain ' ' (Whitespace).";
                WpfMessageBox.Show(this, msg, "Stop", MessageBoxButton.OK, MessageBoxImage.Stop);
                IsWorking = false;
                return;
            }

            if (vari.Contains('=', StringComparison.InvariantCulture) || value.Contains('=', StringComparison.InvariantCulture))
            {
                string msg = "Variable Can't Contain '='.";
                WpfMessageBox.Show(this, msg, "Stop", MessageBoxButton.OK, MessageBoxImage.Stop);
                IsWorking = false;
                return;
            }

            Tuple<string, string> variable = Tuple.Create(vari, value);

            if (addTrue_ModifyFalse)
            {
                // Add
                int addNumber = await MainWindow.Rules.AddAsync(variable);
                if (addNumber == -1)
                {
                    // Save
                    await ReadAsync(true);

                    // Go To Last
                    await DGV.ScrollIntoViewAsync(DGV.Items.Count - 1, 1);

                    // MSG
                    string msg = "Variable Added Successfully.";
                    WpfToastDialog.Show(this, msg, MessageBoxImage.None, 2);
                }
                else if (addNumber == 0)
                {
                    string msg = "Name Already Exist, You Can Modify It Or Delete It.";
                    WpfMessageBox.Show(this, msg, "Stop", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
                else if (addNumber == 1)
                {
                    string msg = "Value Already Exist, You Can Modify It Or Delete It.";
                    WpfMessageBox.Show(this, msg, "Stop", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
                else
                {
                    string msg = "Something Went Wrong!";
                    WpfMessageBox.Show(this, msg, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Modify
                int modifyNumber = await MainWindow.Rules.ModifyAsync(variable);
                if (modifyNumber == -1)
                {
                    // Save
                    await ReadAsync(true);

                    // Refresh Selected Rule
                    if (DGR.SelectedIndex != -1 && DGR.SelectedItem is AgnosticProgram.Rules.Rule rule)
                    {
                        Manage_Rule_Set(rule);
                    }

                    // MSG
                    string msg = "Variable Modified Successfully.";
                    WpfToastDialog.Show(this, msg, MessageBoxImage.None, 2);
                }
                else if (modifyNumber == 0)
                {
                    string msg = "Name Or Value Already Exist.";
                    WpfMessageBox.Show(this, msg, "Stop", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
                else if (modifyNumber == 1)
                {
                    string msg = "Variable Doesn't Exist, You Can Add It.";
                    WpfMessageBox.Show(this, msg, "Stop", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
                else if (modifyNumber == 2)
                {
                    string msg = "Something Went Wrong!";
                    WpfMessageBox.Show(this, msg, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow AddModify_Variable_Async: " + ex.Message);
        }
    }

    private async void VariAdd_Button_Click(object sender, RoutedEventArgs e)
    {
        if (IsWorking) return;
        IsWorking = true;
        await AddModify_Variable_Async(true);
        IsWorking = false;
    }

    private async void VariModify_Button_Click(object sender, RoutedEventArgs e)
    {
        if (IsWorking) return;
        IsWorking = true;
        await AddModify_Variable_Async(false);
        IsWorking = false;
    }

    private void MenuItem_Vari_Copy_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGV.SelectedItem is Tuple<string, string> tuple)
            {
                Clipboard.SetText(tuple.Item1);
                WpfToastDialog.Show(this, "Copied To Clipboard.", MessageBoxImage.None, 2);
                return;
            }
            WpfToastDialog.Show(this, "Something Went Wrong!", MessageBoxImage.Error, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow MenuItem_Vari_Copy_Click: " + ex.Message);
        }
    }

    private async void MenuItem_Vari_Delete_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DGV.SelectedItem is Tuple<string, string> tuple)
            {
                bool isRemoveSuccess = await MainWindow.Rules.RemoveAsync(tuple.Item1);
                if (isRemoveSuccess)
                {
                    string deleted = tuple.Item1;
                    await ReadAsync(true);
                    WpfToastDialog.Show(this, $"\"{deleted}\" Deleted.", MessageBoxImage.None, 1);
                    return;
                }
            }
            WpfToastDialog.Show(this, "Something Went Wrong!", MessageBoxImage.Error, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow MenuItem_Vari_Delete_Click: " + ex.Message);
        }
    }

    private async void MenuItem_Vari_DeleteAll_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string msg = $"Deleting All Variables...{NL}Continue?";
            MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Delete All", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbr != MessageBoxResult.Yes) return;

            MainWindow.Rules.Variables.Clear();
            await ReadAsync(true);
            WpfToastDialog.Show(this, "All Variables Deleted.", MessageBoxImage.None, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow MenuItem_Vari_DeleteAll_Click: " + ex.Message);
        }
    }

    private void DGV_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (DGV.SelectedIndex == -1 || DGV.SelectedItem is not Tuple<string, string> tuple)
            {
                string title = $"Variables({MainWindow.Rules.Variables.Count}) & Rules({MainWindow.Rules.RuleList.Count})";
                this.DispatchIt(() =>
                {
                    VariablesAndRules_GroupBox.Header = title;
                    Variable_Name_TextBox.Text = string.Empty;
                    Variable_Value_TextBox.Text = string.Empty;
                });
            }
            else
            {
                int index = DGV.SelectedIndex + 1;
                string title = $"Variables({index} Of {MainWindow.Rules.Variables.Count}) & Rules({MainWindow.Rules.RuleList.Count})";
                this.DispatchIt(() =>
                {
                    VariablesAndRules_GroupBox.Header = title;
                    Variable_Name_TextBox.Text = tuple.Item1;
                    Variable_Value_TextBox.Text = tuple.Item2;
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow DGV_SelectionChanged: " + ex.Message);
        }
    }

    private void DGV_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            if (sender is not DataGrid dg) return;

            DataGridRow? row = dg.GetRowByMouseEvent(e);
            if (row != null) dg.SelectedIndex = row.GetIndex();

            bool isEnabled = dg.Items.Count > 0;
            MenuItem_Vari_Copy.IsEnabled = isEnabled;
            MenuItem_Vari_Delete.IsEnabled = isEnabled;
            MenuItem_Vari_DeleteAll.IsEnabled = isEnabled;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow DGV_PreviewMouseRightButtonDown: " + ex.Message);
        }
    }

    private async void MenuItem_Rules_MoveUp_Click(object? sender, RoutedEventArgs? e)
    {
        if (IsWorking) return;
        IsWorking = true;

        try
        {
            if (DGR.SelectedItem is not AgnosticProgram.Rules.Rule rule) return;

            // Get Previous Index
            int index = -1;
            if (DGR.SelectedIndex > 0) index = DGR.SelectedIndex - 1;
            if (index != -1)
            {
                bool moved = await MainWindow.Rules.MoveAsync(rule, index); // Move
                if (moved)
                {
                    RuleList_OC.Move(DGR.SelectedIndex, index);
                }
                DGR.ScrollIntoViewByIndex(index);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow MenuItem_Rules_MoveUp_Click: " + ex.Message);
        }

        IsWorking = false;
    }

    private async void MenuItem_Rules_MoveDown_Click(object? sender, RoutedEventArgs? e)
    {
        if (IsWorking) return;
        IsWorking = true;

        try
        {
            if (DGR.SelectedItem is not AgnosticProgram.Rules.Rule rule) return;

            // Get Next Index
            int index = -1;
            if (DGR.SelectedIndex < DGR.Items.Count - 1) index = DGR.SelectedIndex + 1;
            if (index != -1)
            {
                bool moved = await MainWindow.Rules.MoveAsync(rule, index); // Move
                if (moved)
                {
                    RuleList_OC.Move(DGR.SelectedIndex, index);
                }
                DGR.ScrollIntoViewByIndex(index);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow MenuItem_Rules_MoveDown_Click: " + ex.Message);
        }

        IsWorking = false;
    }

    private async void MenuItem_Rules_MoveToTop_Click(object? sender, RoutedEventArgs? e)
    {
        if (IsWorking) return;
        IsWorking = true;

        try
        {
            if (DGR.SelectedItem is not AgnosticProgram.Rules.Rule rule) return;

            if (DGR.Items.Count > 0 && DGR.SelectedIndex != 0)
            {
                int index = 0;
                await MainWindow.Rules.MoveAsync(rule, index); // Move
                await ReadAsync(true); // Refresh
                await DGR.ScrollIntoViewAsync(index, 25);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow MenuItem_Rules_MoveToTop_Click: " + ex.Message);
        }

        IsWorking = false;
    }

    private async void MenuItem_Rules_MoveToBottom_Click(object? sender, RoutedEventArgs? e)
    {
        if (IsWorking) return;
        IsWorking = true;

        try
        {
            if (DGR.SelectedItem is not AgnosticProgram.Rules.Rule rule) return;

            if (DGR.Items.Count > 0 && DGR.SelectedIndex != DGR.Items.Count - 1)
            {
                int index = DGR.Items.Count - 1;
                await MainWindow.Rules.MoveAsync(rule, index); // Move
                await ReadAsync(true); // Refresh
                await DGR.ScrollIntoViewAsync(index, 25);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow MenuItem_Rules_MoveToBottom_Click: " + ex.Message);
        }

        IsWorking = false;
    }

    private async void MenuItem_Rules_Delete_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var selectedRules = DGR.SelectedItems.Cast<AgnosticProgram.Rules.Rule>();
            int count = selectedRules.Count();
            if (count > 0)
            {
                string rulesStr = count > 1 ? "Rules" : "Rule";
                string msg = $"Deleting {count} {rulesStr}...{NL}Continue?";
                MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (mbr != MessageBoxResult.Yes) return;

                bool isRemoveSuccess = false;
                foreach (AgnosticProgram.Rules.Rule selectedRule in selectedRules)
                {
                    bool success = await MainWindow.Rules.RemoveAsync(selectedRule);
                    if (success) isRemoveSuccess = true;
                }

                if (isRemoveSuccess)
                {
                    await ReadAsync(true);
                    WpfToastDialog.Show(this, "Selected Rules Deleted.", MessageBoxImage.None, 2);
                    return;
                }
            }
            WpfToastDialog.Show(this, "Something Went Wrong!", MessageBoxImage.Error, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow MenuItem_Rules_Delete_Click: " + ex.Message);
        }
    }

    private async void MenuItem_Rules_DeleteAll_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string msg = $"Deleting All Rules...{NL}Continue?";
            MessageBoxResult mbr = WpfMessageBox.Show(this, msg, "Delete All", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbr != MessageBoxResult.Yes) return;

            MainWindow.Rules.RuleList.Clear();
            await ReadAsync(true);
            WpfToastDialog.Show(this, "All Rules Deleted.", MessageBoxImage.None, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow MenuItem_Rules_DeleteAll_Click: " + ex.Message);
        }
    }

    private async void DGR_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (DGR.SelectedIndex == -1 || DGR.SelectedItems.Count > 1)
            {
                string title = $"Variables({MainWindow.Rules.Variables.Count}) & Rules({MainWindow.Rules.RuleList.Count})";
                this.DispatchIt(() => VariablesAndRules_GroupBox.Header = title);
                Manage_Rule_Set(new AgnosticProgram.Rules.Rule()); // Clear
            }
            else
            {
                if (DGR.SelectedItem is not AgnosticProgram.Rules.Rule rule) return;
                int index = DGR.SelectedIndex + 1;
                string title = $"Variables({MainWindow.Rules.Variables.Count}) & Rules({index} Of {MainWindow.Rules.RuleList.Count})";
                this.DispatchIt(() => VariablesAndRules_GroupBox.Header = title);
                Manage_Rule_Set(rule);
            }

            await Task.Delay(20);
            ChangeEnable_Rule_Fields();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow DGR_SelectionChanged: " + ex.Message);
        }
    }

    private void DGR_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            if (sender is not DataGrid dg) return;

            bool isMultiSelect = dg.SelectedItems.Count > 1;

            if (!isMultiSelect)
            {
                DataGridRow? row = dg.GetRowByMouseEvent(e);
                if (row != null) dg.SelectedIndex = row.GetIndex();
            }

            bool isEnabled = dg.Items.Count > 0;
            MenuItem_Rules_MoveUp.IsEnabled = isEnabled && !isMultiSelect;
            MenuItem_Rules_MoveDown.IsEnabled = isEnabled && !isMultiSelect;
            MenuItem_Rules_MoveToTop.IsEnabled = isEnabled && !isMultiSelect;
            MenuItem_Rules_MoveToBottom.IsEnabled = isEnabled && !isMultiSelect;
            MenuItem_Rules_Delete.IsEnabled = isEnabled;
            MenuItem_Rules_DeleteAll.IsEnabled = isEnabled;

            if (isEnabled && !isMultiSelect)
            {
                bool isFirstItem = dg.SelectedIndex == 0;
                bool isLastItem = dg.SelectedIndex == dg.Items.Count - 1;

                if (isFirstItem)
                {
                    MenuItem_Rules_MoveUp.IsEnabled = false;
                    MenuItem_Rules_MoveToTop.IsEnabled = false;
                }

                if (isLastItem)
                {
                    MenuItem_Rules_MoveDown.IsEnabled = false;
                    MenuItem_Rules_MoveToBottom.IsEnabled = false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageRulesWindow DGR_PreviewMouseRightButtonDown: " + ex.Message);
        }
    }

    private void DGR_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        try
        {
            if (sender is not DataGrid dg) return;
            e.Handled = true;

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) // Is Ctrl Key Pressed
            {
                if (e.Key == Key.Up) MenuItem_Rules_MoveUp_Click(null, null);
                else if (e.Key == Key.Down) MenuItem_Rules_MoveDown_Click(null, null);
                else if (e.Key == Key.Home) MenuItem_Rules_MoveToTop_Click(null, null);
                else if (e.Key == Key.End) MenuItem_Rules_MoveToBottom_Click(null, null);
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
            Debug.WriteLine("ManageRulesWindow DGR_PreviewKeyDown: " + ex.Message);
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