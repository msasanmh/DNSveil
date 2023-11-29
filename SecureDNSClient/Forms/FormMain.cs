using System.Diagnostics;
using CustomControls;
using System.Reflection;
using System.Text;
using System.IO.Compression;
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

        if (Program.Startup)
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

        // Label Moving
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

        // Update NICs
        SecureDNS.UpdateNICs(CustomComboBoxNICs, false, out _);

        // Update Connect Modes (Quick Connect Settings)
        UpdateConnectModes(CustomComboBoxSettingQcConnectMode);

        // Update NICs (Quick Connect Settings)
        SecureDNS.UpdateNICs(CustomComboBoxSettingQcNics, false, out _);

        // Update GoodbyeDPI Basic Modes (Quick Connect Settings)
        DPIBasicBypass.UpdateGoodbyeDpiBasicModes(CustomComboBoxSettingQcGdBasic);

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
        //UpdateBoolInternetAccessAuto(); // 3 Included In UpdateAllAuto
        UpdateNotifyIconIconAuto();
        CheckUpdateAuto();
        LogClearAuto();
        //UpdateBoolsAuto(); // 2 Included In UpdateAllAuto
        UpdateBoolDnsDohAuto();
        //UpdateBoolProxyAuto(); // 1 Included In UpdateAllAuto
        UpdateStatusVShortAuto();
        UpdateStatusShortAuto(); // 2
        //UpdateStatusLongAuto(); Included In UpdateAllAuto
        //UpdateStatusCpuUsageAuto(); // 2 Included In UpdateAllAuto
        SaveSettingsAuto();

        // Load Saved Servers
        SavedDnsLoad();

        // Set Dpi Bypass Text
        UpdateApplyDpiBypassChangesButton();

        // Load Proxy Port
        ProxyPort = GetProxyPortSetting();

        // In case application closed unexpectedly Kill processes and Unset DNS and Proxy
        bool closedNormally = await DoesAppClosedNormallyAsync();
        if (!closedNormally)
        {
            await KillAll(true);
            bool isDnsSet = NetworkTool.IsDnsSetToLocal(out _, out _);
            bool isProxySet = UpdateBoolIsProxySet();
            if (isDnsSet || isProxySet)
            {
                string msgUnsetOnStart = $"Last time App didn't close normally!{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgUnsetOnStart, Color.Gray));

                if (isDnsSet)
                {
                    msgUnsetOnStart = $"Unsetting Saved DNS...{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgUnsetOnStart, Color.Gray));
                    await UnsetSavedDNS();
                    UpdateStatusNic();
                }

                if (isProxySet)
                {
                    msgUnsetOnStart = $"Unsetting Proxy...{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgUnsetOnStart, Color.Gray));
                    NetworkTool.UnsetProxy(false, true);
                }
            }
        }
        AppClosedNormally(false);

        // Drag bar color
        SplitContainerMain.BackColor = Color.IndianRed;

        // Get Ready
        GetAppReady();

        //================================================================

        // Startup
        bool exeOnStartup = false;
        this.InvokeIt(() => exeOnStartup = CustomCheckBoxSettingQcOnStartup.Checked);
        if (Program.Startup && exeOnStartup) StartupTask();

        // Set Window State
        LastWindowState = WindowState;

        // Set Tooltips
        SetToolTips();

        // Start Trace Event Session
        MonitorProcess.SetPID(); // Measure Whole System
        MonitorProcess.Start(true);
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
            if (Program.Startup)
            {
                Hide();
                return;
            }

            // Add colors and texts to About page
            CustomLabelAboutThis.ForeColor = Color.DodgerBlue;
            string aboutVer = $"v{Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductVersion} ({ArchProcess.ToString().ToLower()})";
            CustomLabelAboutVersion.Text = aboutVer;
            CustomLabelAboutThis2.ForeColor = Color.IndianRed;

            // Delete Log File on > 500KB
            DeleteFileOnSize(SecureDNS.LogWindowPath, 500);

            // Fix Microsoft bugs. Like always!
            if (!IsScreenHighDpiScaleApplied)
                await ScreenHighDpiScaleStartup(this);
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

    //============================== Buttons

    private void CustomButtonProcessMonitor_Click(object sender, EventArgs e)
    {
        // Check if it's already open
        Form f = Application.OpenForms[nameof(FormProcessMonitor)];
        if (f != null)
        {
            f.BringToFront();
            return;
        }

        FormProcessMonitor formProcessMonitor = new();
        formProcessMonitor.StartPosition = FormStartPosition.Manual;
        formProcessMonitor.Location = new Point(MousePosition.X - formProcessMonitor.Width, MousePosition.Y - formProcessMonitor.Height);
        formProcessMonitor.FormClosing += (s, e) => { formProcessMonitor.Dispose(); };
        formProcessMonitor.Show();
    }

    private void CustomButtonEditCustomServers_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
        {
            TsiEdit.Font = Font;
            TsiEdit.Click -= tsiEdit_Click;
            TsiEdit.Click += tsiEdit_Click;
            void tsiEdit_Click(object? sender, EventArgs e) => edit();

            TsiViewWorkingServers.Font = Font;
            TsiViewWorkingServers.Click -= TsiViewWorkingServers_Click;
            TsiViewWorkingServers.Click += TsiViewWorkingServers_Click;
            void TsiViewWorkingServers_Click(object? sender, EventArgs e) => viewWorkingServers();

            TsiClearWorkingServers.Font = Font;
            TsiClearWorkingServers.Click -= TsiClearWorkingServers_Click;
            TsiClearWorkingServers.Click += TsiClearWorkingServers_Click;
            void TsiClearWorkingServers_Click(object? sender, EventArgs e) => clearWorkingServers();

            CMS.Font = Font;
            CMS.Items.Clear();
            CMS.Items.Add(TsiEdit);
            CMS.Items.Add(TsiViewWorkingServers);
            CMS.Items.Add(TsiClearWorkingServers);
            Theme.SetColors(CMS);
            CMS.RoundedCorners = 5;
            CMS.Show(CustomButtonEditCustomServers, 0, 0);

            void edit()
            {
                if (IsInAction(true, true, true, true, true, true, true, true, false, false, true)) return;

                FormCustomServers formCustomServers = new();
                formCustomServers.StartPosition = FormStartPosition.CenterParent;
                formCustomServers.FormClosing += (s, e) => { formCustomServers.Dispose(); };
                formCustomServers.ShowDialog(this);
            }

            void viewWorkingServers()
            {
                FileDirectory.CreateEmptyFile(SecureDNS.WorkingServersPath);
                int notepad = ProcessManager.ExecuteOnly("notepad", SecureDNS.WorkingServersPath, false, false, SecureDNS.CurrentPath);
                if (notepad == -1)
                {
                    string msg = "Notepad is not installed on your system.";
                    CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
                }
            }

            void clearWorkingServers()
            {
                try
                {
                    File.Delete(SecureDNS.WorkingServersPath);
                    string msg = $"{NL}Working Servers Cleared.{NL}{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    private void CustomButtonCheck_Click(object? sender, EventArgs? e)
    {
        if (IsInAction(true, true, true, false, true, true, true, true, true, false, true)) return;
        StartCheck(null);
    }

    private async void CustomButtonQuickConnect_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            await StartQuickConnect(null);
        }
        else if (e.Button == MouseButtons.Right)
        {
            List<string> groupList = await ReadCustomServersXmlGroups(SecureDNS.CustomServersXmlPath);
            ToolStripItem[] subMenuItems = new ToolStripItem[groupList.Count];

            // Add Built-In to DropDown Items
            TsiQcToBuiltIn.Font = Font;
            TsiQcToBuiltIn.Click -= QcToBuiltIn_Click;
            TsiQcToBuiltIn.Click += QcToBuiltIn_Click;

            // Add Custom Groups to DropDown Items
            for (int n = 0; n < groupList.Count; n++)
            {
                string groupName = groupList[n];
                subMenuItems[n] = new ToolStripMenuItem(groupName);
                subMenuItems[n].Font = Font;
                subMenuItems[n].Name = groupName;
                subMenuItems[n].Click -= QcToCustomGroups_Click;
                subMenuItems[n].Click += QcToCustomGroups_Click;
            }

            CMS.Font = Font;
            CMS.Items.Clear();
            CMS.Items.Add(TsiQcToBuiltIn);
            CMS.Items.AddRange(subMenuItems);
            Theme.SetColors(CMS);
            CMS.RoundedCorners = 5;
            CMS.Show(CustomButtonQuickConnect, 0, 0);
        }
    }

    private async void CustomButtonDisconnectAll_Click(object sender, EventArgs e)
    {
        await DisconnectAll();
    }

    private async void CustomButtonCheckUpdate_Click(object sender, EventArgs e)
    {
        if (IsInAction(true, true, false, false, false, false, false, false, false, false, true)) return;
        await CheckUpdate(true);
    }

    private async void CustomButtonWriteSavedServersDelay_Click(object sender, EventArgs e)
    {
        if (IsInAction(true, true, true, true, true, true, true, true, true, false, true)) return;
        await WriteSavedServersDelayToLog();
    }

    private async void CustomButtonConnect_Click(object? sender, EventArgs? e)
    {
        if (IsInAction(true, true, true, true, false, false, true, true, true, true, true)) return;
        await StartConnect(GetConnectMode());
    }

    private void CustomButtonUpdateNICs_Click(object sender, EventArgs e)
    {
        // Update NICs
        SecureDNS.UpdateNICs(CustomComboBoxNICs, false, out _);
    }

    private async void CustomButtonEnableDisableNic_Click(object sender, EventArgs e)
    {
        if (CustomComboBoxNICs.SelectedItem == null) return;
        string? nicName = CustomComboBoxNICs.SelectedItem.ToString();
        if (string.IsNullOrEmpty(nicName)) return;

        // Disable Button
        CustomButtonEnableDisableNic.Enabled = false;

        if (CustomButtonEnableDisableNic.Text.Contains("Enable"))
        {
            CustomButtonEnableDisableNic.Text = "Enabling...";
            await NetworkTool.EnableNICAsync(nicName);
        }
        else
        {
            CustomButtonEnableDisableNic.Text = "Disabling...";
            await NetworkTool.DisableNICAsync(nicName);
        }

        // Update NIC Status
        await Task.Run(() => UpdateStatusNic());

        // Enable Button
        CustomButtonEnableDisableNic.Enabled = true;
    }

    private async void CustomButtonSetDNS_Click(object sender, EventArgs e)
    {
        if (IsInAction(true, true, true, true, true, true, true, true, true, false, true)) return;
        await SetDNS();

        // Update NIC Status
        await Task.Run(() => UpdateStatusNic());
    }

    private async void CustomButtonShare_Click(object sender, EventArgs e)
    {
        if (IsInAction(true, true, true, true, true, true, true, true, true, true, true)) return;
        await StartProxy();
    }

    private void CustomButtonSetProxy_Click(object sender, EventArgs e)
    {
        if (IsInAction(true, false, false, false, false, false, false, false, true, true, true)) return;
        SetProxy();
    }

    private async void CustomButtonPDpiApplyChanges_Click(object sender, EventArgs e)
    {
        this.InvokeIt(() => CustomButtonPDpiApplyChanges.Text = "Applying");
        UpdateProxyBools = false;
        if (ProcessManager.FindProcessByPID(PIDFakeProxy))
            await ApplyPDpiChangesFakeProxy();
        await ApplyPDpiChanges();
        UpdateProxyBools = true;

        await UpdateBoolProxy();
        UpdateApplyDpiBypassChangesButton();

        await UpdateBoolProxy();
        IsProxySet = UpdateBoolIsProxySet();
        if (IsProxySet)
            await SetProxyInternalAsync(); // Change Proxy HTTP <==> SOCKS
        this.InvokeIt(() => CustomButtonPDpiApplyChanges.Text = "Apply DPI bypass changes");
    }

    private async void CustomButtonPDpiCheck_Click(object sender, EventArgs e)
    {
        if (IsInAction(true, true, true, true, true, true, true, true, true, true, true)) return;
        // Get blocked domain
        string blockedDomain = GetBlockedDomainSetting(out _);
        await CheckDPIWorks(blockedDomain);
    }

    private void CustomButtonPDpiPresetDefault_Click(object sender, EventArgs e)
    {
        CustomNumericUpDownPDpiBeforeSniChunks.Value = (decimal)50;
        CustomComboBoxPDpiSniChunkMode.SelectedIndex = 0;
        CustomNumericUpDownPDpiSniChunks.Value = (decimal)5;
        CustomNumericUpDownPDpiAntiPatternOffset.Value = (decimal)2;
        CustomNumericUpDownPDpiFragDelay.Value = (decimal)1;

        if (IsCheckingStarted) return;
        string msg1 = "Proxy DPI Bypass Mode: ";
        string msg2 = $"Default{NL}";
        CustomRichTextBoxLog.AppendText(msg1, Color.LightGray);
        CustomRichTextBoxLog.AppendText(msg2, Color.DodgerBlue);
    }

    private void CustomButtonDPIBasic_Click(object sender, EventArgs e)
    {
        if (IsInAction(true, true, true, true, true, true, true, true, true, true, true)) return;
        // Activate/Reactivate GoodbyeDPI Basic
        GoodbyeDPIBasic();
    }

    private void CustomButtonDPIBasicDeactivate_Click(object sender, EventArgs e)
    {
        //Deactivate GoodbyeDPI Basic
        GoodbyeDPIDeactive(true, false);
    }

    private void CustomButtonDPIAdvBlacklist_Click(object sender, EventArgs e)
    {
        // Edit GoodbyeDPI Advanced Blacklist
        FileDirectory.CreateEmptyFile(SecureDNS.DPIBlacklistPath);
        int notepad = ProcessManager.ExecuteOnly("notepad", SecureDNS.DPIBlacklistPath, false, false, SecureDNS.CurrentPath);
        if (notepad == -1)
        {
            string msg = "Notepad is not installed on your system.";
            CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
        }
    }

    private void CustomButtonDPIAdvActivate_Click(object sender, EventArgs e)
    {
        if (IsInAction(true, true, true, true, true, true, true, true, true, true, true)) return;
        // Activate/Reactivate GoodbyeDPI Advanced
        GoodbyeDPIAdvanced();
    }

    private void CustomButtonDPIAdvDeactivate_Click(object sender, EventArgs e)
    {
        // Deactivate GoodbyeDPI Advanced
        GoodbyeDPIDeactive(false, true);
    }

    private void CustomButtonToolsDnsScanner_Click(object sender, EventArgs e)
    {
        // Check if it's already open
        Form f = Application.OpenForms[nameof(FormDnsScanner)];
        if (f != null)
        {
            f.BringToFront();
            return;
        }

        FormDnsScanner formDnsScanner = new();
        formDnsScanner.StartPosition = FormStartPosition.CenterParent;
        formDnsScanner.FormClosing += (s, e) => { formDnsScanner.Dispose(); };
        formDnsScanner.ShowDialog(this);
    }

    private void CustomButtonToolsDnsLookup_Click(object sender, EventArgs e)
    {
        FormDnsLookup formDnsLookup = new();
        formDnsLookup.StartPosition = FormStartPosition.Manual;
        formDnsLookup.Location = new Point(MousePosition.X + 50, MousePosition.Y - CustomButtonToolsDnsLookup.Top);
        formDnsLookup.FormClosing += (s, e) => { formDnsLookup.Dispose(); };
        formDnsLookup.Show(this);
    }

    private void CustomButtonToolsStampReader_Click(object sender, EventArgs e)
    {
        // Check if it's already open
        Form f = Application.OpenForms[nameof(FormStampReader)];
        if (f != null)
        {
            f.BringToFront();
            return;
        }

        FormStampReader formStampReader = new();
        formStampReader.StartPosition = FormStartPosition.Manual;
        formStampReader.Location = new Point(MousePosition.X + 50, MousePosition.Y - CustomButtonToolsStampReader.Top);
        formStampReader.FormClosing += (s, e) => { formStampReader.Dispose(); };
        formStampReader.Show();
    }

    private void CustomButtonToolsStampGenerator_Click(object sender, EventArgs e)
    {
        // Check if it's already open
        Form f = Application.OpenForms[nameof(FormStampGenerator)];
        if (f != null)
        {
            f.BringToFront();
            return;
        }

        FormStampGenerator formStampGenerator = new();
        formStampGenerator.StartPosition = FormStartPosition.Manual;
        formStampGenerator.Location = new Point(MousePosition.X + 50, MousePosition.Y - CustomButtonToolsStampGenerator.Top);
        formStampGenerator.FormClosing += (s, e) => { formStampGenerator.Dispose(); };
        formStampGenerator.Show();
    }

    private void CustomButtonToolsIpScanner_Click(object sender, EventArgs e)
    {
        // Check if it's already open
        Form f = Application.OpenForms[nameof(FormIpScanner)];
        if (f != null)
        {
            f.BringToFront();
            return;
        }

        FormIpScanner formIpScanner = new();
        formIpScanner.StartPosition = FormStartPosition.Manual;
        formIpScanner.Location = new Point(MousePosition.X + 50, MousePosition.Y - CustomButtonToolsIpScanner.Top);
        formIpScanner.FormClosing += (s, e) => { formIpScanner.Dispose(); };
        formIpScanner.Show();
    }

    private async void CustomButtonToolsFlushDns_Click(object sender, EventArgs e)
    {
        // Flush Dns
        if (IsInAction(true, false, true, true, true, true, true, true, true, false, true)) return;
        CustomButtonToolsFlushDns.Enabled = false;
        CustomButtonToolsFlushDns.Text = "Flushing...";
        await FlushDnsOnExit(true);
        CustomButtonToolsFlushDns.Text = "Flush DNS";
        CustomButtonToolsFlushDns.Enabled = true;
    }

    private void CustomButtonSettingUninstallCertificate_Click(object sender, EventArgs e)
    {
        UninstallCertificate();
    }

    private void CustomButtonSettingQcUpdateNics_Click(object sender, EventArgs e)
    {
        // Update NICs (Quick Connect Settings)
        SecureDNS.UpdateNICs(CustomComboBoxSettingQcNics, false, out _);
    }

    private void CustomButtonSettingQcStartup_Click(object sender, EventArgs e)
    {
        if (IsOnStartup)
        {
            if (IsStartupPathOk)
            {
                // Remove From Startup
                ActivateWindowsStartup(false);
                IsOnStartup = IsAppOnWindowsStartup(out bool isStartupPathOk);
                IsStartupPathOk = isStartupPathOk;
                if (!IsOnStartup)
                    CustomMessageBox.Show(this, "Successfully removed from Startup.", "Startup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    CustomMessageBox.Show(this, "Couldn't remove from Startup!", "Startup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                // Fix Startup Path
                ActivateWindowsStartup(true);
                IsOnStartup = IsAppOnWindowsStartup(out bool isStartupPathOk);
                IsStartupPathOk = isStartupPathOk;
                if (IsStartupPathOk)
                    CustomMessageBox.Show(this, "Successfully fixed Startup path.", "Startup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    CustomMessageBox.Show(this, "Couldn't fix Startup path!", "Startup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            // Apply to Startup
            ActivateWindowsStartup(true);
            IsOnStartup = IsAppOnWindowsStartup(out bool isStartupPathOk);
            IsStartupPathOk = isStartupPathOk;
            if (IsOnStartup)
                CustomMessageBox.Show(this, "Successfully applied to Startup.", "Startup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                CustomMessageBox.Show(this, "Couldn't apply to Startup!", "Startup", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CustomButtonSettingProxyFakeDNS_Click(object sender, EventArgs e)
    {
        FileDirectory.CreateEmptyFile(SecureDNS.FakeDnsRulesPath);
        int notepad = ProcessManager.ExecuteOnly("notepad", SecureDNS.FakeDnsRulesPath, false, false, SecureDNS.CurrentPath);
        if (notepad == -1)
        {
            string msg = "Notepad is not installed on your system.";
            CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
        }
    }

    private void CustomButtonSettingProxyBlackWhiteList_Click(object sender, EventArgs e)
    {
        FileDirectory.CreateEmptyFile(SecureDNS.BlackWhiteListPath);
        int notepad = ProcessManager.ExecuteOnly("notepad", SecureDNS.BlackWhiteListPath, false, false, SecureDNS.CurrentPath);
        if (notepad == -1)
        {
            string msg = "Notepad is not installed on your system.";
            CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
        }
    }

    private void CustomButtonSettingProxyDontBypass_Click(object sender, EventArgs e)
    {
        FileDirectory.CreateEmptyFile(SecureDNS.DontBypassListPath);
        int notepad = ProcessManager.ExecuteOnly("notepad", SecureDNS.DontBypassListPath, false, false, SecureDNS.CurrentPath);
        if (notepad == -1)
        {
            string msg = "Notepad is not installed on your system.";
            CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
        }
    }

    private void CustomButtonSettingRestoreDefault_Click(object sender, EventArgs e)
    {
        if (IsCheckingStarted)
        {
            string msgChecking = "Stop check operation first." + NL;
            CustomRichTextBoxLog.AppendText(msgChecking, Color.IndianRed);
            return;
        }

        if (IsConnected)
        {
            string msgConnected = "Disconnect first." + NL;
            CustomRichTextBoxLog.AppendText(msgConnected, Color.IndianRed);
            return;
        }

        if (IsDNSSet)
        {
            string msgDNSIsSet = "Unset DNS first." + NL;
            CustomRichTextBoxLog.AppendText(msgDNSIsSet, Color.IndianRed);
            return;
        }

        DefaultSettings();

        string msgDefault = "Settings restored to default." + NL;
        CustomRichTextBoxLog.AppendText(msgDefault, Color.MediumSeaGreen);
    }

    private async void CustomButtonExportUserData_Click(object sender, EventArgs e)
    {
        using SaveFileDialog sfd = new();
        sfd.Filter = "SDC User Data (*.sdcud)|*.sdcud";
        sfd.DefaultExt = ".sdcud";
        sfd.AddExtension = true;
        sfd.RestoreDirectory = true;
        sfd.FileName = $"sdc_user_data_{DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss", CultureInfo.InvariantCulture)}";
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                // Save Settings
                await SaveSettings();
                await Task.Delay(200);

                ZipFile.CreateFromDirectory(SecureDNS.UserDataDirPath, sfd.FileName);
                CustomMessageBox.Show(this, "Data exported successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(this, ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async void CustomButtonImportUserData_Click(object sender, EventArgs e)
    {
        using OpenFileDialog ofd = new();
        ofd.Filter = "SDC User Data (*.sdcud)|*.sdcud";
        ofd.DefaultExt = ".sdcud";
        ofd.AddExtension = true;
        ofd.RestoreDirectory = true;
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                ZipFile.ExtractToDirectory(ofd.FileName, SecureDNS.UserDataDirPath, true);
                await Task.Delay(1000);

                try
                {
                    // Load Settings
                    if (File.Exists(SecureDNS.SettingsXmlPath) && XmlTool.IsValidXMLFile(SecureDNS.SettingsXmlPath))
                        AppSettings = new(this, SecureDNS.SettingsXmlPath);
                    else
                        AppSettings = new(this);

                    string msg = "Data imported seccessfully.";
                    CustomMessageBox.Show(this, msg, "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception)
                {
                    string msg = "Failed importing user data.";
                    CustomMessageBox.Show(this, msg, "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(this, ex.Message, "Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    //============================== Events
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

    private async void CustomComboBoxNICs_SelectedIndexChanged(object sender, EventArgs e)
    {
        await Task.Run(() => UpdateStatusNic());
    }

    private async void CustomCheckBoxProxyEnableSSL_CheckedChanged(object sender, EventArgs e)
    {
        if (Program.Startup) return;
        if (!Visible) return;
        if (AppUpTime.ElapsedMilliseconds < 6000) return; // Don't Do This On App Startup
        if (sender is not CustomCheckBox cb) return;
        UpdateApplyDpiBypassChangesButton();
        if (!cb.Checked) return;
        if (IsInAction(true, true, true, true, true, false, true, true, true, false, true)) return;
        this.InvokeIt(() => cb.Enabled = false);
        this.InvokeIt(() => cb.Text = "Enabling...");
        await ApplySSLDecryption();
        this.InvokeIt(() => cb.Text = "Enable SSL Decryption");
        this.InvokeIt(() => cb.Enabled = true);
        UpdateApplyDpiBypassChangesButton();
    }

    private void CustomTextBoxSettingCheckDPIHost_Leave(object sender, EventArgs e)
    {
        GetBlockedDomainSetting(out string _); // To Correct BlockedDomain Input
    }

    private void CustomNumericUpDownUpdateAutoDelayMS_ValueChanged(object sender, EventArgs e)
    {
        UpdateAutoDelayMS = GetUpdateAutoDelaySetting();
    }

    private void CustomTextBoxSettingBootstrapDnsIP_Leave(object sender, EventArgs e)
    {
        GetBootstrapSetting(out _); // To Correct Bootstrap Input
    }

    //============================== Closing
    private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing)
        {
            e.Cancel = true; // We Have Zero Time To Unset DNS On Shutdown
            string processName = "netsh";
            string processArgs1 = $"interface ipv4 delete dnsservers \"{LastNicName}\" all";
            ProcessManager.ExecuteOnly(processName, processArgs1, true, true);
        }
        else if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            //ShowInTaskbar = false; // Makes Titlebar white (I use Show and Hide instead)
            NotifyIconMain.BalloonTipText = "Minimized to tray.";
            NotifyIconMain.BalloonTipIcon = ToolTipIcon.Info;
            NotifyIconMain.ShowBalloonTip(100);
        }
    }

    private void NotifyIconMain_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            if (Program.Startup)
            {
                Program.Startup = false;
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

}