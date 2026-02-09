using System.Diagnostics;
using CustomControls;
using System.Reflection;
using System.Globalization;
using Microsoft.Win32;
using MsmhToolsClass;
using MsmhToolsWinFormsClass;
using System.Runtime.InteropServices;
// https://github.com/msasanmh/SecureDNSClient

namespace SecureDNSClient;

public partial class FormMain : Form
{
    public FormMain()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        InitializeComponent();

        if (Program.IsStartup)
        {
            Hide();
            Opacity = 0;
        }

        // Start App Up Time Timer
        AppUpTime.Start();

        // Write Debug Info About DNS Server To Log
        DnsConsole.StandardDataReceived += DnsConsole_StandardDataReceived;

        // Write Debug Info About Proxy Server To Log
        ProxyConsole.StandardDataReceived += ProxyConsole_StandardDataReceived;

        // Write DNS Or DoH Requests To Log
        DnsConsole.ErrorDataReceived += DnsConsole_ErrorDataReceived;
        StopWatchWriteDnsOutputDelay.Start();

        // Write Proxy Requests And Chunk Details To Log
        ProxyConsole.ErrorDataReceived += ProxyConsole_ErrorDataReceived;
        StopWatchWriteProxyOutputDelay.Start();

        // Save Log To File
        CustomRichTextBoxLog.TextAppended += CustomRichTextBoxLog_TextAppended;

        Start();

        VisibleChanged += FormMain_VisibleChanged;
        Move += FormMain_Move;
        Resize += FormMain_Resize;
        LocationChanged += FormMain_LocationChanged;
        SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        SystemEvents.SessionEnding += SystemEvents_SessionEnding;
    }

    private async void Start()
    {
        // App Beta Stat
        bool isBeta = false;

        // Startup MSG
        string msgStartup = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss} Initializing...{NL}";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgStartup, Color.Gray));

        // Invariant Culture
        Info.SetCulture(CultureInfo.InvariantCulture);

        // Add SDC Binaries to Windows Firewall If Firewall Is Enabled
        AddOrUpdateFirewallRulesNoLog();

        // Label Main
        Controls.Add(LabelMain);
        LabelMain.Text = "Getting Ready...";
        LabelMain.Dock = DockStyle.Fill;
        LabelMain.TextAlign = ContentAlignment.MiddleCenter;
        LabelMain.Font = new Font(Font.FontFamily, Font.Size * 1.5f);
        SplitContainerMain.Visible = false;
        LabelMain.Visible = true;
        LabelMain.BringToFront();

        // Load Theme
        await LoadThemeAsync();

        // Label Screen to Fix Screen DPI
        LabelScreen.Text = "MSasanMH";
        LabelScreen.Font = Font;

        // Fill ComboBoxes
        await FillComboBoxesAsync();

        // App Startup Defaults
        string productName = Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductName ?? "SDC - Secure DNS Client";
        string productVersion = Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductVersion ?? "0.0.0";
        string archText = getArchText(ArchOs, ArchProcess);

        static string getArchText(Architecture archOs, Architecture archProcess)
        {
            string archOsStr = archOs.ToString().ToLower();
            string archProcessStr = archProcess.ToString().ToLower();
            if (archOs == archProcess) return archProcessStr;
            else if (archOs == Architecture.X64 && archProcess == Architecture.X86)
                return $"{archProcessStr} on {archOsStr} OS";
            else
                return $"{archProcessStr} on {archOsStr} OS (Experimental)";
        }

        Text = $"{productName} {archText} v{productVersion}";
        if (Program.IsPortable) Text += " Portable";
        if (isBeta) Text += " Beta";
        CustomButtonSetDNS.Enabled = false;
        CustomButtonSetProxy.Enabled = false;
        CustomTabControlSettings.HideTabHeader = true;

        // Set NotifyIcon Text
        NotifyIconMain.Text = Text;

        // Create UserData & Assets Dir If Not Exist
        FileDirectory.CreateEmptyDirectory(SecureDNS.UserDataDirPath);
        FileDirectory.CreateEmptyDirectory(SecureDNS.AssetDirPath);

        // Move User Data and Certificate to the new location
        await MoveToNewLocationAsync();

        // Initialize and Load Settings
        if (File.Exists(SecureDNS.SettingsXmlPath) && XmlTool.IsValidFile(SecureDNS.SettingsXmlPath))
        {
            await DefaultSettingsAsync();
            AppSettings = new(this, SecureDNS.SettingsXmlPath);
        }
        else
        {
            await DefaultSettingsAsync();
            AppSettings = new(this);
        }

        // Logics After Load Settings
        ProxyPort = GetProxyPortSetting(); // Load Proxy Port
        SplitContainerMain.BackColor = Color.IndianRed; // Drag Bar Color
        CustomTextBoxHTTPProxy.Enabled = true; // Connect -> Method 4
        CustomCheckBoxSettingQcSetProxy.Enabled = CustomCheckBoxSettingQcStartProxyServer.Checked; // Setting -> Qc -> Set Proxy

        // Convert Old Proxy ProxyRules To New
        await OldProxyRulesToNewAsync();
        await MergeOldDnsAndProxyRulesAsync();

        // Add Default Malicious Servers
        await AddDefaultMaliciousServers_Async();

        // Initialize Status
        InitializeStatus(CustomDataGridViewStatus);

        // Initialize NIC Status
        InitializeNicStatus(CustomDataGridViewNicStatus);

        // Write binaries if not exist or needs update
        bool successWrite = await WriteNecessaryFilesToDisk();
        if (!successWrite)
        {
            string msgWB = $"Couldn't Write Binaries To Disk.{NL}";
            msgWB += $"Restart Application.{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgWB, Color.IndianRed));
        }

        // Load UpdateAutoDelay
        UpdateAutoDelayMS = GetUpdateAutoDelaySetting();

        UpdateAllAuto(); // CPU Friendly
        UpdateNotifyIconIconAuto();
        CheckUpdateAuto();
        LogClearAuto();
        UpdateBoolDnsDohAuto();
        UpdateStatusShortAuto(); // 2
        SaveSettingsAuto();

        // Load Saved Servers
        SavedDnsLoad();

        // Update IsDNSSet
        IsDNSSet = SetDnsOnNic_.IsDnsSet(CustomComboBoxNICs, out bool isDnsSetOn, out _);
        IsDNSSetOn = isDnsSetOn;

        // In case application closed unexpectedly Kill processes and Unset DNS and Proxy
        bool closedNormally = await DoesAppClosedNormallyAsync();
        if (!closedNormally)
        {
            string msg = $"Last Time App Didn't Close Normally!{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.Gray));
        }
        AppClosedNormally(false);

        if (IsThereAnyLeftovers())
        {
            string msg = $"Killing Leftovers...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.Gray));
            await KillAllAsync(true);
        }

        if (IsDNSSet)
        {
            string msg = $"Unsetting DNS...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.Gray));
            await UnsetAllDNSs();
            await UpdateStatusNicAsync();
        }

        IsProxySet = UpdateBoolIsProxySet(out bool isAnotherProxySet, out string currentSystemProxy);
        IsAnotherProxySet = isAnotherProxySet;
        CurrentSystemProxy = currentSystemProxy;
        if (IsProxySet)
        {
            string msg = $"Unsetting Proxy...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.Gray));
            NetworkTool.UnsetProxy(false, true);
        }

        // Get Ready
        GetAppReady();

        //================================================================

        // Startup
        bool exeOnStartup = false;
        this.InvokeIt(() => exeOnStartup = CustomCheckBoxSettingQcOnStartup.Checked);
        if (Program.IsStartup && exeOnStartup) StartupTask();

        // Set Window State
        LastWindowState = WindowState;

        // Set Tooltips
        SetToolTips();

        // Check Certificate
        IsSSLDecryptionEnable();

        // Start Trace Event Session
        MonitorProcess.Start(true);
    }

    private void ShowLabelMain(string text)
    {
        this.InvokeIt(() =>
        {
            if (!IsAppReady) text = "Getting Ready...";
            LabelMain.Text = text;
            LabelMain.Visible = true;
            LabelMain.BringToFront();
            LabelMainStopWatch.Restart();
            SplitContainerMain.Visible = false;
        });
    }

    private void HideLabelMain()
    {
        if (!IsThemeApplied || !IsScreenHighDpiScaleApplied) return;

        bool isStatusEmpty = false;
        this.InvokeIt(() =>
        {
            try
            {
                string? status = CustomDataGridViewStatus.Rows[12].Cells[1].Value.ToString();
                if (string.IsNullOrEmpty(status)) isStatusEmpty = true;
            }
            catch (Exception) { }
        });
        if (isStatusEmpty) return;

        if (!LabelMain.Visible) return;
        this.InvokeIt(() =>
        {
            SplitContainerMain.Visible = true;
            LabelMain.Visible = false;
            LabelMain.SendToBack();
        });
    }

    private void LabelMainHide()
    {
        if (LabelMainStopWatch.ElapsedMilliseconds > 300)
        {
            HideLabelMain();
            LabelMainStopWatch.Restart();
        }
    }

    private async void FormMain_VisibleChanged(object? sender, EventArgs e)
    {
        if (Visible)
        {
            // Startup
            if (Program.IsStartup)
            {
                Hide();
                Opacity = 0;
                return;
            }
            else
            {
                if (Once)
                {
                    UpdateMinSizeOfStatus();
                    Once = false;
                }
            }

            // Load Theme
            if (AppUpTime.ElapsedMilliseconds > 20000)
                await LoadThemeAsync();

            // Delete Log Files On > 500KB
            DeleteFileOnSize(SecureDNS.LogWindowPath, 500);
            DeleteFileOnSize(SecureDNS.ErrorLogPath, 500);
        }
    }

    private void FormMain_Move(object? sender, EventArgs e)
    {
        if (LabelMainStopWatch.IsRunning)
        {
            ShowLabelMain("Now Moving...");
        }
    }

    private void FormMain_Resize(object? sender, EventArgs e)
    {
        if (WindowState != LastWindowState)
        {
            LastWindowState = WindowState;
        }
        else
        {
            if (LabelMainStopWatch.IsRunning)
            {
                ShowLabelMain("Now Resizing...");
            }
        }
    }

    private void FormMain_LocationChanged(object? sender, EventArgs e)
    {
        if (LabelMainStopWatch.IsRunning)
            LabelMainStopWatch.Restart();
    }

    private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
    {
        UnsetDnsOnShutdown(e);
    }

    //============================== Events (Controls)
    private void CustomRichTextBoxLog_SizeChanged(object sender, EventArgs e)
    {
        UpdateMinSizeOfStatus();
    }

    private async void SecureDNSClient_CheckedChanged(object sender, EventArgs e)
    {
        string msgCommandError = $"Couldn't Send Command To Proxy Server, Try Again.{NL}";
        if (sender is CustomCheckBox checkBoxR && checkBoxR.Name == CustomCheckBoxProxyEventShowRequest.Name)
        {
            UpdateProxyBools = false;
            if (checkBoxR.Checked)
            {
                string command = "Requests True";
                if (IsProxyActivated)
                {
                    bool isSent = await ProxyConsole.SendCommandAsync(command);
                    if (!isSent)
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCommandError, Color.IndianRed));
                }
            }
            else
            {
                string command = "Requests False";
                if (IsProxyActivated)
                {
                    bool isSent = await ProxyConsole.SendCommandAsync(command);
                    if (!isSent)
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCommandError, Color.IndianRed));
                }
            }
            UpdateProxyBools = true;
        }

        if (sender is CustomCheckBox checkBoxC && checkBoxC.Name == CustomCheckBoxProxyEventShowChunkDetails.Name)
        {
            UpdateProxyBools = false;
            if (checkBoxC.Checked)
            {
                string command = "FragmentDetails True";
                if (IsProxyActivated)
                {
                    bool isSent = await ProxyConsole.SendCommandAsync(command);
                    if (!isSent)
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCommandError, Color.IndianRed));
                }
            }
            else
            {
                string command = "FragmentDetails False";
                if (IsProxyActivated)
                {
                    bool isSent = await ProxyConsole.SendCommandAsync(command);
                    if (!isSent)
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCommandError, Color.IndianRed));
                }
            }
            UpdateProxyBools = true;
        }

        if (AppSettings == null) return;

        if (sender is CustomRadioButton crbBuiltIn && crbBuiltIn.Name == CustomRadioButtonBuiltIn.Name)
        {
            AppSettings.AddSetting(CustomRadioButtonBuiltIn, nameof(CustomRadioButtonBuiltIn.Checked), CustomRadioButtonBuiltIn.Checked);
        }

        if (sender is CustomRadioButton crbCustom && crbCustom.Name == CustomRadioButtonCustom.Name)
        {
            AppSettings.AddSetting(CustomRadioButtonCustom, nameof(CustomRadioButtonCustom.Checked), CustomRadioButtonCustom.Checked);
        }
    }

    // Secure DNS (Tab)
    private void CustomTabControlSecureDNS_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (sender is not CustomTabControl ctc) return;

        if (ctc.SelectedIndex == 3) // Share + Bypass DPI
        {
            UpdateApplyDpiBypassChangesButton();
        }
    }

    // Secure DNS -> Set DNS
    private void CustomComboBoxNICs_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!Visible) return;
        Task.Run(async () =>
        {
            await UpdateStatusShortOnBoolsChangedAsync();
            await UpdateStatusNicAsync();
        });
    }

    // Secure DNS -> Share
    private void CustomCheckBoxPDpiEnableFragment_TextChanged(object sender, EventArgs e)
    {
        if (sender is not CustomCheckBox ccb) return;
        string pad = "MSI";
        Size size = TextRenderer.MeasureText(ccb.Text + pad, ccb.Font);
        ccb.Width = size.Width;
    }

    private void CustomCheckBoxPDpiEnableFragment_CheckedChanged(object sender, EventArgs e)
    {
        if (sender is not CustomCheckBox cb) return;
        UpdateApplyDpiBypassChangesButton();

        // Open Fragment Options
        if (cb.Checked)
        {
            try
            {
                this.InvokeIt(() => CustomTabControlShareDpiBypassOptions.SelectedIndex = 0);
            }
            catch (Exception) { }
        }
    }

    private void CustomNumericUpDownPDpiFragment_ValueChanged(object sender, EventArgs e)
    {
        UpdateApplyDpiBypassChangesButton();
    }

    private void CustomComboBoxPDpiSniChunkMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateApplyDpiBypassChangesButton();
    }

    private async void CustomCheckBoxProxyEnableSSL_CheckedChanged(object sender, EventArgs e)
    {
        if (Program.IsStartup) return;
        if (!Visible) return;
        if (AppUpTime.ElapsedMilliseconds < 20000) return; // Don't Do This On App Startup
        if (sender is not CustomCheckBox cb) return;
        this.InvokeIt(() => cb.Tag = "fired");
        UpdateApplyDpiBypassChangesButton();
        if (!cb.Checked) return;
        if (IsInAction(true, true, true, true, true, false, true, true, true, false, true, out _))
        {
            this.InvokeIt(() => cb.Checked = !cb.Checked);
            return;
        }
        this.InvokeIt(() => cb.Enabled = false);
        this.InvokeIt(() => cb.Text = "Enabling...");
        await ApplySSLDecryption();
        this.InvokeIt(() => cb.Text = "Enable SSL Decryption");
        this.InvokeIt(() => cb.Enabled = true);
        UpdateApplyDpiBypassChangesButton();

        // Open SSL Decryption Options
        if (cb.Checked)
        {
            try
            {
                this.InvokeIt(() => CustomTabControlShareDpiBypassOptions.SelectedIndex = 1);
            }
            catch (Exception) { }
        }
    }

    private void CustomCheckBoxProxySSLChangeSni_CheckedChanged(object sender, EventArgs e)
    {
        UpdateApplyDpiBypassChangesButton();
    }

    private void CustomTextBoxProxySSLDefaultSni_KeyUp(object sender, KeyEventArgs e)
    {
        UpdateApplyDpiBypassChangesButton();
    }

    // Settings ListBox Menu
    private void CustomListBoxSettingsMenu_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (sender is not CustomListBox clb) return;
        try
        {
            if (clb.SelectedIndex < CustomTabControlSettings.TabPages.Count)
            {
                this.InvokeIt(() => CustomTabControlSettings.SelectedIndex = clb.SelectedIndex);
                this.InvokeIt(() => CustomTabControlSettings.TabPages[clb.SelectedIndex].Focus());
            }
        }
        catch (Exception) { }
    }

    private void CustomTabControlSettings_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (sender is not CustomTabControl ctb) return;
        try
        {
            if (ctb.SelectedIndex < CustomListBoxSettingsMenu.Items.Count)
                if (ctb.SelectedIndex != CustomListBoxSettingsMenu.SelectedIndex)
                    CustomListBoxSettingsMenu.SelectedIndex = ctb.SelectedIndex;
        }
        catch (Exception) { }
    }

    // Settings -> Check
    private void CustomTextBoxSettingCheckDPIHost_Leave(object sender, EventArgs e)
    {
        GetBlockedDomainSetting(out string _); // To Correct BlockedDomain Input
    }

    // Settings -> Quick Connect
    private void SettingQcConnectModePropertyChanged()
    {
        try
        {
            this.InvokeIt(() =>
            {
                ConnectMode cMode = ConnectMode.ConnectToWorkingServers;
                if (CustomComboBoxSettingQcConnectMode.SelectedItem != null)
                    cMode = GetConnectModeByName(CustomComboBoxSettingQcConnectMode.SelectedItem.ToString());
                CustomCheckBoxSettingQcUseSavedServers.Enabled = cMode == ConnectMode.ConnectToWorkingServers;
                CustomCheckBoxSettingQcCheckAllServers.Enabled = cMode == ConnectMode.ConnectToWorkingServers && !CustomCheckBoxSettingQcUseSavedServers.Checked;
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SettingQcConnectModePropertyChanged: " + ex.Message);
        }
    }

    private void CustomComboBoxSettingQcConnectMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        SettingQcConnectModePropertyChanged();
    }

    private void CustomCheckBoxSettingQcUseSavedServers_CheckedChanged(object sender, EventArgs e)
    {
        SettingQcConnectModePropertyChanged();
    }

    private void CustomCheckBoxSettingQcStartProxyServer_CheckedChanged(object sender, EventArgs e)
    {
        this.InvokeIt(() => CustomCheckBoxSettingQcSetProxy.Enabled = CustomCheckBoxSettingQcStartProxyServer.Checked);
    }

    // Settings -> Rules
    private void LinkLabelSettingRulesHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenLinks.OpenUrl("https://github.com/msasanmh/SecureDNSClient/blob/main/README.md#sdc-text-based-rules");
    }

    // Settings -> Geo Assets
    private async void CustomCheckBoxGeoAsset_GeoAssets_CheckedChanged(object sender, EventArgs e)
    {
        try
        {
            if (sender is not CustomCheckBox ccb) return;
            if (ccb.Checked)
            {
                // Get Controls
                List<Control> controls = Controllers.GetChildControls(FlowLayoutPanelGeoAssets);

                this.InvokeIt(() =>
                {
                    foreach (Control control in controls)
                        if (control is CustomCheckBox cb) cb.Enabled = false;
                });

                await Assets_Download_Async();

                this.InvokeIt(() =>
                {
                    foreach (Control control in controls)
                        if (control is CustomCheckBox cb) cb.Enabled = true;
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CustomCheckBoxGeoAsset_GeoAssets_CheckedChanged: " + ex.Message);
        }
    }

    // Settings -> CPU
    private void CustomNumericUpDownUpdateAutoDelayMS_ValueChanged(object sender, EventArgs e)
    {
        UpdateAutoDelayMS = GetUpdateAutoDelaySetting();
    }

    // Settings -> Others
    private void CustomTextBoxSettingBootstrapDnsIP_Leave(object sender, EventArgs e)
    {
        GetBootstrapSetting(out _); // To Correct Bootstrap Input
    }

    //============================== Closing
    private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        Debug.WriteLine(e.CloseReason);
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            //ShowInTaskbar = false; // Makes Titlebar white (I use Show and Hide instead)

            if (!File.Exists(SecureDNS.FirstRun))
            {
                NotifyIconMain.BalloonTipText = "Minimized to tray.";
                NotifyIconMain.BalloonTipIcon = ToolTipIcon.Info;
                NotifyIconMain.ShowBalloonTip(100);
            }
        }
        else
        {
            UnsetDnsOnShutdown(e);
        }
    }

    private void NotifyIconMain_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            if (Program.IsStartup)
            {
                Program.IsStartup = false;
                string msgStartUpExited = $"Startup Mode Exited.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgStartUpExited, Color.MediumSeaGreen));

                bool exeOnStartup = false;
                this.InvokeIt(() => exeOnStartup = CustomCheckBoxSettingQcOnStartup.Checked);
                if (exeOnStartup && !StartupTaskExecuted)
                {
                    string msgQcCanceled = $"Startup Task Canceled.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgQcCanceled, Color.MediumSeaGreen));
                }

                Task.Run(async () => await UpdateStatusLongAsync());
                Task.Run(async () => await UpdateStatusNicAsync());
                Task.Run(async () => await CheckUpdateAsync());
            }

            this.SetDarkTitleBar(true); // Just in case
            Show();
            Opacity = 1;
            BringToFront();
        }
        else if (e.Button == MouseButtons.Right)
        {
            if (IsExiting)
            {
                CustomContextMenuStripIcon.Hide();
                return; // Return doesn't work on Exit!!
            }
            ShowMainContextMenu();
        }
    }

    //============================== About
    private void CustomLabelAboutThis_Click(object sender, EventArgs e)
    {
        OpenLinks.OpenUrl("https://github.com/msasanmh/SecureDNSClient/");
    }

    private void LinkLabelDNSLookup_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenLinks.OpenUrl("https://github.com/ameshkov/dnslookup");
    }

    private void LinkLabelGoodbyeDPI_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenLinks.OpenUrl("https://github.com/ValdikSS/GoodbyeDPI");
    }

    private void LinkLabelStAlidxdydz_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenLinks.OpenUrl("https://github.com/alidxdydz");
    }

    private void LinkLabelStWolfkingal2000_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenLinks.OpenUrl("https://github.com/wolfkingal2000");
    }

    private void LinkLabelStNonbarbari_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenLinks.OpenUrl("https://github.com/nonbarbari");
    }

    
}