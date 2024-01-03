using System.Diagnostics;
using CustomControls;
using System.Reflection;
using System.Text;
using System.Globalization;
using Microsoft.Win32;
using MsmhToolsClass;
using MsmhToolsWinFormsClass;
using MsmhToolsWinFormsClass.Themes;
using SecureDNSClient.DPIBasic;
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

        // Save Log to File
        CustomRichTextBoxLog.TextAppended += CustomRichTextBoxLog_TextAppended;

        Start();

        VisibleChanged += FormMain_VisibleChanged;
        Move += FormMain_Move;
        Resize += FormMain_Resize;
        LocationChanged += FormMain_LocationChanged;
        SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
    }

    private async void Start()
    {
        // Load Theme
        Theme.LoadTheme(this, Theme.Themes.Dark);
        Theme.SetColors(LabelMain);
        CustomMessageBox.FormIcon = Properties.Resources.SecureDNSClient_Icon_Multi;

        string msgStartup = $"Initializing...{NL}";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgStartup, Color.Gray));

        // Invariant Culture
        Info.SetCulture(CultureInfo.InvariantCulture);

        // Add SDC Binaries to Windows Firewall if Firewall is Enabled
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

        // Label Screen to Fix Screen DPI
        LabelScreen.Text = "MSasanMH";
        LabelScreen.Font = Font;

        // Fill ComboBoxes
        FillComboBoxes();

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
        CustomButtonSetDNS.Enabled = false;
        CustomButtonSetProxy.Enabled = false;
        CustomTextBoxHTTPProxy.Enabled = false;
        LinkLabelCheckUpdate.Text = string.Empty;

        // Set NotifyIcon Text
        NotifyIconMain.Text = Text;

        // Create User Dir if not Exist
        FileDirectory.CreateEmptyDirectory(SecureDNS.UserDataDirPath);

        // Move User Data and Certificate to the new location
        await MoveToNewLocation();

        // Initialize and load Settings
        if (File.Exists(SecureDNS.SettingsXmlPath) && XmlTool.IsValidXMLFile(SecureDNS.SettingsXmlPath))
            AppSettings = new(this, SecureDNS.SettingsXmlPath);
        else
        {
            DefaultSettings();
            AppSettings = new(this);
        }

        // Logics After Load Settings
        ProxyPort = GetProxyPortSetting(); // Load Proxy Port
        SplitContainerMain.BackColor = Color.IndianRed; // Drag Bar Color
        CustomTextBoxHTTPProxy.Enabled = CustomRadioButtonConnectDNSCrypt.Checked; // Connect -> Method 4
        CustomCheckBoxSettingQcSetProxy.Enabled = CustomCheckBoxSettingQcStartProxyServer.Checked; // Setting -> Qc -> Set Proxy
        CustomTextBoxSettingProxyCfCleanIP.Enabled = CustomCheckBoxSettingProxyCfCleanIP.Checked; // Setting -> Share -> Advanced -> Cf Clean IP

        // Initialize Status
        InitializeStatus(CustomDataGridViewStatus);

        // Initialize NIC Status
        InitializeNicStatus(CustomDataGridViewNicStatus);

        // Write binaries if not exist or needs update
        bool successWrite = await WriteNecessaryFilesToDisk();
        if (!successWrite)
        {
            string msgWB = $"Couldn't write binaries to disk.{NL}";
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
        UpdateStatusVShortAuto();
        UpdateStatusShortAuto(); // 2
        SaveSettingsAuto();

        // Load Saved Servers
        SavedDnsLoad();

        // In case application closed unexpectedly Kill processes and Unset DNS and Proxy
        bool closedNormally = await DoesAppClosedNormallyAsync();
        if (!closedNormally)
        {
            string msg = $"Last time App didn't close normally!{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.Gray));

            msg = $"Killing Leftovers...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.Gray));
            await KillAll(true);

            bool isDnsSet = NetworkTool.IsDnsSetToLocal(out _, out _);
            IsProxySet = UpdateBoolIsProxySet(out bool isAnotherProxySet, out string currentSystemProxy);
            IsAnotherProxySet = isAnotherProxySet;
            CurrentSystemProxy = currentSystemProxy;
            if (isDnsSet || IsProxySet)
            {
                if (isDnsSet)
                {
                    msg = $"Unsetting Saved DNS...{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.Gray));
                    await UnsetSavedDNS();
                    await Task.Run(() => UpdateStatusNic());
                }

                if (IsProxySet)
                {
                    msg = $"Unsetting Proxy...{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.Gray));
                    NetworkTool.UnsetProxy(false, true);
                }
            }
        }
        AppClosedNormally(false);

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
        MonitorProcess.SetPID(); // Measure Whole System
        MonitorProcess.Start(true);

        // Fix Microsoft bugs. Like always!
        if (!Program.IsStartup)
            if (!IsScreenHighDpiScaleApplied)
            {
                await ScreenHighDpiScaleStartup(this);
                FillComboBoxes();
            }
    }

    private void CustomRichTextBoxLog_TextAppended(object? sender, EventArgs e)
    {
        if (sender is string text)
        {
            // Write to file
            try
            {
                if (CustomCheckBoxSettingWriteLogWindowToFile.Checked)
                    FileDirectory.AppendText(SecureDNS.LogWindowPath, text, new UTF8Encoding(false));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Write Log to file: {ex.Message}");
            }
        }
    }

    private void ShowLabelMain(string text)
    {
        this.InvokeIt(() =>
        {
            SplitContainerMain.Visible = false;
            LabelMain.Text = text;
            LabelMain.Visible = true;
            LabelMain.BringToFront();
            LabelMainStopWatch.Restart();
        });
    }

    private void HideLabelMain()
    {
        bool isStatusEmpty = false;
        this.InvokeIt(() =>
        {
            try
            {
                object status = CustomDataGridViewStatus.Rows[11].Cells[1].Value;
                if (status is null) isStatusEmpty = true;
                else if (status.Equals("")) isStatusEmpty = true;
            }
            catch (Exception) { }
        });
        if (isStatusEmpty) return;

        if (!LabelMain.Visible) return;
        this.InvokeIt(() =>
        {
            LabelMain.Visible = false;
            LabelMain.SendToBack();
            SplitContainerMain.Visible = true;
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
                return;
            }

            // Update Connect Modes (Quick Connect Settings)
            UpdateConnectModes(CustomComboBoxSettingQcConnectMode);

            // Update GoodbyeDPI Basic Modes (Quick Connect Settings)
            DPIBasicBypass.UpdateGoodbyeDpiBasicModes(CustomComboBoxSettingQcGdBasic);

            // Add colors and texts to About page
            CustomLabelAboutThis.ForeColor = Color.DodgerBlue;
            string aboutVer = $"v{Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductVersion} ({ArchProcess.ToString().ToLower()})";
            CustomLabelAboutVersion.Text = aboutVer;
            CustomLabelAboutThis2.ForeColor = Color.IndianRed;

            // Delete Log File on > 500KB
            DeleteFileOnSize(SecureDNS.LogWindowPath, 500);

            // Fix Microsoft bugs. Like always!
            if (!IsScreenHighDpiScaleApplied)
            {
                await ScreenHighDpiScaleStartup(this);
                FillComboBoxes();
            }

            if (Once)
            {
                // Check If Another Proxy Is Set
                await UpdateBools();
                if (IsAnotherProxySet)
                {
                    string url = "https://www.google.com";
                    bool canOpenUrl = await NetworkTool.IsWebsiteOnlineAsync(url, 5000, true);
                    if (!canOpenUrl)
                    {
                        string msg = $"Another Proxy ({NetworkTool.GetSystemProxy()}) is set to your System and cannot open {url}";
                        msg += $"{NL}Unset the Proxy?";
                        DialogResult dr = CustomMessageBox.Show(this, msg, "A Proxy Is Set", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (dr == DialogResult.Yes)
                        {
                            // Unset The Proxy
                            NetworkTool.UnsetProxy(false, true);
                        }
                        Once = false;
                    }
                }
            }
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

    private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
    {
        if (ScreenDPI.GetSystemDpi() != 96)
        {
            using Graphics g = CreateGraphics();
            PaintEventArgs args = new(g, DisplayRectangle);
            OnPaint(args);
            string msg = "Display Settings Changed.\n";
            msg += "You may need restart the app to fix display blurriness.";
            CustomMessageBox.Show(this, msg);
        }

        if (LabelMainStopWatch.IsRunning) HideLabelMain();
    }

    //============================== Events (Controls)
    private void CustomRichTextBoxLog_SizeChanged(object sender, EventArgs e)
    {
        // Two Times: To Get VScrollbar Of CustomDataGridViewStatus Updated
        UpdateMinSizeOfStatus();
        UpdateMinSizeOfStatus();
    }

    private async void SecureDNSClient_CheckedChanged(object sender, EventArgs e)
    {
        string msgCommandError = $"Couldn't send command to Proxy Server, try again.{NL}";
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
                string command = "ChunkDetails True";
                if (IsProxyActivated)
                {
                    bool isSent = await ProxyConsole.SendCommandAsync(command);
                    if (!isSent)
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCommandError, Color.IndianRed));
                }
            }
            else
            {
                string command = "ChunkDetails False";
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
        if (sender is not TabControl tc) return;
        if (tc.SelectedIndex == 3) // Share + Bypass DPI
        {
            UpdateApplyDpiBypassChangesButton();
        }
    }

    // Secure DNS -> Connect
    private void CustomRadioButtonConnectDNSCrypt_CheckedChanged(object sender, EventArgs e)
    {
        // Connect to popular servers using proxy Textbox
        this.InvokeIt(() => CustomTextBoxHTTPProxy.Enabled = CustomRadioButtonConnectDNSCrypt.Checked);
    }

    // Secure DNS -> Set DNS
    private async void CustomComboBoxNICs_SelectedIndexChanged(object sender, EventArgs e)
    {
        await UpdateStatusShortOnBoolsChanged();
        await Task.Run(() => UpdateStatusNic());
    }

    // Secure DNS -> Share
    private void CustomCheckBoxPDpiEnableDpiBypass_TextChanged(object sender, EventArgs e)
    {
        if (sender is not CustomCheckBox ccb) return;
        string pad = "MSI";
        Size size = TextRenderer.MeasureText(ccb.Text + pad, ccb.Font);
        ccb.Width = size.Width;
    }

    private void CustomCheckBoxPDpiEnableDpiBypass_CheckedChanged(object sender, EventArgs e)
    {
        UpdateApplyDpiBypassChangesButton();
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
        if (AppUpTime.ElapsedMilliseconds < 10000) return; // Don't Do This On App Startup
        if (sender is not CustomCheckBox cb) return;
        this.InvokeIt(() => cb.Tag = "fired");
        UpdateApplyDpiBypassChangesButton();
        if (!cb.Checked) return;
        if (IsInAction(true, true, true, true, true, false, true, true, true, false, true))
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
    }

    private void CustomCheckBoxProxySSLChangeSniToIP_CheckedChanged(object sender, EventArgs e)
    {
        UpdateApplyDpiBypassChangesButton();
    }

    private void CustomTextBoxProxySSLDefaultSni_KeyUp(object sender, KeyEventArgs e)
    {
        UpdateApplyDpiBypassChangesButton();
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

    // Settings -> Share -> Advanced
    private void CustomCheckBoxSettingProxyCfCleanIP_CheckedChanged(object sender, EventArgs e)
    {
        this.InvokeIt(() => CustomTextBoxSettingProxyCfCleanIP.Enabled = CustomCheckBoxSettingProxyCfCleanIP.Checked);
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
            NotifyIconMain.BalloonTipText = "Minimized to tray.";
            NotifyIconMain.BalloonTipIcon = ToolTipIcon.Info;
            NotifyIconMain.ShowBalloonTip(100);
        }
        else
        {
            if (!string.IsNullOrEmpty(LastNicName))
            {
                e.Cancel = true; // We Have Zero Time To Unset DNS On Shutdown
                string processName = "netsh";
                string processArgs1 = $"interface ipv4 delete dnsservers \"{LastNicName}\" all";
                ProcessManager.ExecuteOnly(processName, processArgs1, true, true);
                bool unsetToDHCP = CustomRadioButtonSettingUnsetDnsToDhcp.Checked;
                if (unsetToDHCP)
                {
                    // DHCP
                    string processArgs2 = $"interface ipv4 set dnsservers \"{LastNicName}\" source=dhcp";
                    ProcessManager.ExecuteOnly(processName, processArgs2, true, true);
                }
                else
                {
                    // STATIC
                    string dns1 = CustomTextBoxSettingUnsetDns1.Text;
                    string processArgs2 = $"interface ipv4 set dnsservers \"{LastNicName}\" static {dns1} primary";
                    ProcessManager.ExecuteOnly(processName, processArgs2, true, true);
                }
            }
        }
    }

    private void NotifyIconMain_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            if (Program.IsStartup)
            {
                Program.IsStartup = false;
                Task.Run(async () => await UpdateStatusLong());
                Task.Run(() => UpdateStatusNic());
                Task.Run(async () => await CheckUpdate());
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

    private void LinkLabelDNSProxy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenLinks.OpenUrl("https://github.com/AdguardTeam/dnsproxy");
    }

    private void LinkLabelDNSCrypt_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenLinks.OpenUrl("https://github.com/DNSCrypt/dnscrypt-proxy");
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

}