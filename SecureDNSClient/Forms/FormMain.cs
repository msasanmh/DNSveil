using System.Net;
using System.Diagnostics;
using CustomControls;
using System.Reflection;
using System.Text;
using System.IO.Compression;
using System.Globalization;
using Microsoft.Win32;
using MsmhToolsClass;
using MsmhToolsClass.ProxyServerPrograms;
using MsmhToolsWinFormsClass;
using MsmhToolsWinFormsClass.Themes;
using SecureDNSClient.DPIBasic;
using System.Runtime.InteropServices;
// https://github.com/msasanmh/SecureDNSClient

namespace SecureDNSClient;

public partial class FormMain : Form
{
    public static ToolTip MainToolTip { get; set; } = new();
    private FormWindowState LastWindowState;
    public static readonly CustomLabel LabelScreen = new();
    private Stopwatch LabelMovingStopWatch = new();
    private static readonly CustomLabel LabelMoving = new();
    public List<Tuple<long, string>> WorkingDnsList = new();
    public List<string> SavedDnsList = new();
    public List<string> SavedEncodedDnsList = new();
    public List<string> CurrentUsingCustomServersList = new();
    private List<string> WorkingDnsListToFile = new();
    private List<Tuple<long, string>> WorkingDnsAndLatencyListToFile = new();
    public static ProcessMonitor MonitorProcess { get; set; } = new();
    private bool InternetOnline = false;
    private bool InternetOffline = true;
    public static bool IsInternetOnline { get; set; } = false;
    private bool Once { get; set; } = true;
    public static bool IsInActionState { get; set; } = false;
    private bool IsOnStartup { get; set; } = false;
    private bool IsStartupPathOk { get; set; } = false;
    private bool IsCheckingForUpdate { get; set; } = false;
    private int NumberOfWorkingServers { get; set; } = 0;
    public static bool IsCheckingStarted { get; set; } = false;
    public static bool IsBuiltinMode { get; set; } = true;
    private static bool StopChecking { get; set; } = false;
    private bool IsConnecting { get; set; } = false;
    private bool IsDisconnecting { get; set; } = false;
    private bool IsDisconnectingAll { get; set; } = false;
    private bool IsConnected { get; set; } = false;
    private ConnectMode LastConnectMode { get; set; } = ConnectMode.ConnectToWorkingServers;
    public static bool IsDNSConnected { get; set; } = false;
    private int LocalDnsLatency { get; set; } = -1;
    public static bool IsDoHConnected { get; set; } = false;
    private int LocalDohLatency { get; set; } = -1;
    public static int ConnectedDohPort { get; set; } = 443; // as default
    private bool IsDNSSetting { get; set; } = false;
    private bool IsDNSUnsetting { get; set; } = false;
    private bool IsDNSSet { get; set; } = false;
    private string LastNicName { get; set; } = string.Empty;
    private SetDnsOnNic SetDnsOnNic_ { get; set; } = new();
    private bool DoesDNSSetOnce { get; set; } = false;
    private bool IsFlushingDns { get; set; } = false;
    private bool IsDPIActive { get; set; } = false;
    private bool IsGoodbyeDPIBasicActive { get; set; } = false;
    private bool IsGoodbyeDPIAdvancedActive { get; set; } = false;
    private bool ConnectAllClicked { get; set; } = false;
    public static IPAddress? LocalIP { get; set; } = IPAddress.Loopback; // as default
    public Settings? AppSettings { get; set; }
    private readonly ToolStripMenuItem ToolStripMenuItemIcon = new();
    private bool AudioAlertOnline = true;
    private bool AudioAlertOffline = false;
    private bool AudioAlertRequestsExceeded = false;
    private readonly Stopwatch StopWatchCheckDPIWorks = new();
    private readonly Stopwatch StopWatchAudioAlertDelay = new();
    private string TheDll = string.Empty;
    private static readonly string NL = Environment.NewLine;
    private bool IsExiting = false;

    // PIDs
    public static int PIDDNSProxy { get; set; } = -1;
    public static int PIDDNSCrypt { get; set; } = -1;
    private static int PIDGoodbyeDPIBasic { get; set; } = -1;
    private static int PIDGoodbyeDPIAdvanced { get; set; } = -1;

    // Camouflage Proxy
    private ProcessConsole CamouflageProxyConsole { get; set; } = new();
    private static int PIDCamouflageProxy { get; set; } = -1;
    private static int PIDDNSCryptBypass { get; set; } = -1;
    private bool IsBypassProxyActive { get; set; } = false;

    // Camouflage GoodbyeDPI
    private CamouflageDNSServer? CamouflageDNSServer { get; set; }
    private static int PIDGoodbyeDPIBypass { get; set; } = -1;
    private static int PIDDNSProxyBypass { get; set; } = -1;
    private bool IsBypassDNSActive { get; set; } = false;
    private bool IsBypassGoodbyeDpiActive { get; set; } = false;

    // Msmh Proxy
    private ProcessConsole ProxyConsole { get; set; } = new();
    private static int PIDProxy { get; set; } = -1;
    private bool IsProxyActivated { get; set; } = false;
    private bool IsProxyActivating { get; set; } = false;
    private bool IsProxyDeactivating { get; set; } = false;
    public static bool IsProxyRunning { get; set; } = false;
    public static int ProxyPort { get; set; } = -1;
    private int ProxyRequests { get; set; } = 0;
    private int ProxyMaxRequests { get; set; } = 250;
    private bool IsProxyDpiBypassActive { get; set; } = false;
    private ProxyProgram.DPIBypass.Mode ProxyStaticDPIBypassMode { get; set; } = ProxyProgram.DPIBypass.Mode.Disable;
    private ProxyProgram.DPIBypass.Mode ProxyDPIBypassMode { get; set; } = ProxyProgram.DPIBypass.Mode.Disable;
    private ProxyProgram.UpStreamProxy.Mode ProxyUpStreamMode { get; set; } = ProxyProgram.UpStreamProxy.Mode.Disable;
    private ProxyProgram.Dns.Mode ProxyDNSMode { get; set; } = ProxyProgram.Dns.Mode.Disable;
    private ProxyProgram.FakeDns.Mode ProxyFakeDnsMode { get; set; } = ProxyProgram.FakeDns.Mode.Disable;
    private ProxyProgram.BlackWhiteList.Mode ProxyBWListMode { get; set; } = ProxyProgram.BlackWhiteList.Mode.Disable;
    private ProxyProgram.DontBypass.Mode DontBypassMode { get; set; } = ProxyProgram.DontBypass.Mode.Disable;
    private bool IsProxySet { get; set; } = false;
    private static bool UpdateProxyBools { get; set; } = true;

    // Fake Proxy
    private ProcessConsole FakeProxyConsole { get; set; } = new();
    private static int PIDFakeProxy { get; set; } = -1;

    // Check DPI Bypass Cancel Token
    private Task? CheckDpiBypass;
    private CancellationTokenSource CheckDpiBypassCTS = new();

    public FormMain()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        InitializeComponent();

        Start();

        Shown += FormMain_Shown;
        Move += FormMain_Move;
        Resize += FormMain_Resize;
        LocationChanged += FormMain_LocationChanged;
        SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
    }

    private async void Start()
    {
        // Invariant Culture
        Info.SetCulture(CultureInfo.InvariantCulture);

        // Add SDC Binaries to Windows Firewall if Firewall is Enabled
        AddOrUpdateFirewallRulesNoLog();

        // Load Theme
        Theme.LoadTheme(this, Theme.Themes.Dark);
        CustomMessageBox.FormIcon = Properties.Resources.SecureDNSClient_Icon_Multi;

        // Update NICs
        SecureDNS.UpdateNICs(CustomComboBoxNICs, false, out _);

        // Update Connect Modes (Quick Connect Settings)
        UpdateConnectModes(CustomComboBoxSettingQcConnectMode);

        // Update NICs (Quick Connect Settings)
        SecureDNS.UpdateNICs(CustomComboBoxSettingQcNics, false, out _);

        // Update GoodbyeDPI Basic Modes (Quick Connect Settings)
        DPIBasicBypass.UpdateGoodbyeDpiBasicModes(CustomComboBoxSettingQcGdBasic);

        // Label Screen
        LabelScreen.Text = "MSasanMH";
        LabelScreen.Font = Font;

        // Startup Defaults
        Architecture archOs = RuntimeInformation.OSArchitecture;
        Architecture archProcess = RuntimeInformation.ProcessArchitecture;
        string productName = Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductName ?? "SDC - Secure DNS Client";
        string productVersion = Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductVersion ?? "0.0.0";
        string archText = getArchText(archOs, archProcess);

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
        DefaultSettings();

        // Set NotifyIcon Text
        NotifyIconMain.Text = Text;

        // Create User Dir if not Exist
        FileDirectory.CreateEmptyDirectory(SecureDNS.UserDataDirPath);

        // Move User Data and Certificate to the new location
        await MoveToNewLocation();

        // Add Tooltips
        string msgCheckInParallel = "a) Don't use parallel on slow network.";
        msgCheckInParallel += $"{NL}b) Parallel doesn't support bootstrap.";
        msgCheckInParallel += $"{NL}c) Parallel doesn't support insecure mode.";
        CustomCheckBoxCheckInParallel.SetToolTip(MainToolTip, "Info", msgCheckInParallel);

        // Add Tooltips to advanced DPI
        string msgP = "Block passive DPI.";
        CustomCheckBoxDPIAdvP.SetToolTip(MainToolTip, "Info", msgP);
        string msgR = "Replace Host with hoSt.";
        CustomCheckBoxDPIAdvR.SetToolTip(MainToolTip, "Info", msgR);
        string msgS = "Remove space between host header and its value.";
        CustomCheckBoxDPIAdvS.SetToolTip(MainToolTip, "Info", msgS);
        string msgM = "Mix Host header case (test.com -> tEsT.cOm).";
        CustomCheckBoxDPIAdvM.SetToolTip(MainToolTip, "Info", msgM);
        string msgF = "Set HTTP fragmentation to value";
        CustomCheckBoxDPIAdvF.SetToolTip(MainToolTip, "Info", msgF);
        string msgK = "Enable HTTP persistent (keep-alive) fragmentation and set it to value.";
        CustomCheckBoxDPIAdvK.SetToolTip(MainToolTip, "Info", msgK);
        string msgN = "Do not wait for first segment ACK when -k is enabled.";
        CustomCheckBoxDPIAdvN.SetToolTip(MainToolTip, "Info", msgN);
        string msgE = "Set HTTPS fragmentation to value.";
        CustomCheckBoxDPIAdvE.SetToolTip(MainToolTip, "Info", msgE);
        string msgA = "Additional space between Method and Request-URI (enables -s, may break sites).";
        CustomCheckBoxDPIAdvA.SetToolTip(MainToolTip, "Info", msgA);
        string msgW = "Try to find and parse HTTP traffic on all processed ports (not only on port 80).";
        CustomCheckBoxDPIAdvW.SetToolTip(MainToolTip, "Info", msgW);
        string msgPort = "Additional TCP port to perform fragmentation on (and HTTP tricks with -w).";
        CustomCheckBoxDPIAdvPort.SetToolTip(MainToolTip, "Info", msgPort);
        string msgIpId = "Handle additional IP ID (decimal, drop redirects and TCP RSTs with this ID).";
        CustomCheckBoxDPIAdvIpId.SetToolTip(MainToolTip, "Info", msgIpId);
        string msgAllowNoSni = "Perform circumvention if TLS SNI can't be detected with --blacklist enabled.";
        CustomCheckBoxDPIAdvAllowNoSNI.SetToolTip(MainToolTip, "Info", msgAllowNoSni);
        string msgSetTtl = "Activate Fake Request Mode and send it with supplied TTL value.\nDANGEROUS! May break websites in unexpected ways. Use with care(or--blacklist).";
        CustomCheckBoxDPIAdvSetTTL.SetToolTip(MainToolTip, "Info", msgSetTtl);
        string msgAutoTtl = "Activate Fake Request Mode, automatically detect TTL and decrease\nit based on a distance. If the distance is shorter than a2, TTL is decreased\nby a2. If it's longer, (a1; a2) scale is used with the distance as a weight.\nIf the resulting TTL is more than m(ax), set it to m.\nDefault (if set): --auto-ttl 1-4-10. Also sets --min-ttl 3.\nDANGEROUS! May break websites in unexpected ways. Use with care (or --blacklist).";
        CustomCheckBoxDPIAdvAutoTTL.SetToolTip(MainToolTip, "[a1-a2-m]", msgAutoTtl);
        string msgMinTtl = "Minimum TTL distance (128/64 - TTL) for which to send Fake Request\nin --set - ttl and--auto - ttl modes.";
        CustomCheckBoxDPIAdvMinTTL.SetToolTip(MainToolTip, "Info", msgMinTtl);
        string msgWrongChksum = "Activate Fake Request Mode and send it with incorrect TCP checksum.\nMay not work in a VM or with some routers, but is safer than set - ttl.";
        CustomCheckBoxDPIAdvWrongChksum.SetToolTip(MainToolTip, "Info", msgWrongChksum);
        string msgWrongSeq = "Activate Fake Request Mode and send it with TCP SEQ/ACK in the past.";
        CustomCheckBoxDPIAdvWrongSeq.SetToolTip(MainToolTip, "Info", msgWrongSeq);
        string msgNativeFrag = "Fragment (split) the packets by sending them in smaller packets, without\nshrinking the Window Size. Works faster(does not slow down the connection)\nand better.";
        CustomCheckBoxDPIAdvNativeFrag.SetToolTip(MainToolTip, "Info", msgNativeFrag);
        string msgReverseFrag = "Fragment (split) the packets just as --native-frag, but send them in the\nreversed order. Works with the websites which could not handle segmented\nHTTPS TLS ClientHello(because they receive the TCP flow \"combined\").";
        CustomCheckBoxDPIAdvReverseFrag.SetToolTip(MainToolTip, "Info", msgReverseFrag);
        string msgMaxPayload = "Packets with TCP payload data more than [value] won't be processed.\nUse this option to reduce CPU usage by skipping huge amount of data\n(like file transfers) in already established sessions.\nMay skip some huge HTTP requests from being processed.\nDefault(if set): --max-payload 1200.";
        CustomCheckBoxDPIAdvMaxPayload.SetToolTip(MainToolTip, "Info", msgMaxPayload);
        string msgBlacklist = "Perform circumvention tricks only to host names and subdomains from\nsupplied text file(HTTP Host / TLS SNI).";
        CustomCheckBoxDPIAdvBlacklist.SetToolTip(MainToolTip, "Info", msgBlacklist);

        // Add colors and texts to About page
        CustomLabelAboutThis.ForeColor = Color.DodgerBlue;
        string aboutVer = $"v{Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductVersion} ({archProcess.ToString().ToLower()})";
        CustomLabelAboutVersion.Text = aboutVer;
        CustomLabelAboutThis2.ForeColor = Color.IndianRed;

        // Initialize and load Settings
        if (File.Exists(SecureDNS.SettingsXmlPath) && XmlTool.IsValidXMLFile(SecureDNS.SettingsXmlPath))
            AppSettings = new(this, SecureDNS.SettingsXmlPath);
        else
            AppSettings = new(this);

        // Initialize Status
        InitializeStatus(CustomDataGridViewStatus);

        // Initialize NIC Status
        InitializeNicStatus(CustomDataGridViewNicStatus);

        IsInternetOnline = IsInternetAlive(false);
        UpdateNotifyIconIconAuto();
        CheckUpdateAuto();
        LogClearAuto();
        UpdateBoolsAuto();
        UpdateBoolDnsDohAuto();
        UpdateBoolProxyAuto();
        UpdateStatusShortAuto();
        UpdateStatusLongAuto();
        UpdateStatusNicAuto();
        UpdateStatusCpuUsageAuto();

        // Auto Save Settings (Timer)
        SaveSettingsAuto();

        // Set Window State
        LastWindowState = WindowState;

        // Label Moving
        Controls.Add(LabelMoving);
        LabelMoving.Text = "Now Moving...";
        LabelMoving.Size = new(300, 150);
        LabelMoving.Location = new((ClientSize.Width / 2) - (LabelMoving.Width / 2), (ClientSize.Height / 2) - (LabelMoving.Height / 2));
        LabelMoving.TextAlign = ContentAlignment.MiddleCenter;
        LabelMoving.Font = new(FontFamily.GenericSansSerif, 12);
        Theme.SetColors(LabelMoving);
        LabelMoving.Visible = false;
        LabelMoving.SendToBack();

        // Save Log to File
        CustomRichTextBoxLog.TextAppended += CustomRichTextBoxLog_TextAppended;

        // Write binaries if not exist or needs update
        await WriteNecessaryFilesToDisk();

        // Load Saved Servers
        SavedDnsLoad();

        // In case application closed unexpectedly Kill processes and set DNS to dynamic
        if (!DoesAppClosedNormally())
        {
            await KillAll(true);
            await UnsetSavedDNS();
        }
        AppClosedNormally(false);

        // Delete Log File on > 500KB
        DeleteFileOnSize(SecureDNS.LogWindowPath, 500);

        // Load Proxy Port
        ProxyPort = GetProxyPortSetting();

        // Start Trace Event Session
        MonitorProcess.SetPID(); // Measure Whole System
        MonitorProcess.Start(true);

        // Drag bar color
        SplitContainerMain.BackColor = Color.IndianRed;

        // Startup
        if (Program.Startup) StartupTask();
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

    private void HideLabelMoving()
    {
        LabelMoving.Visible = false;
        LabelMoving.SendToBack();
        SplitContainerMain.Visible = true;
    }

    private void LabelMovingHide()
    {
        if (LabelMovingStopWatch.ElapsedMilliseconds > 300)
        {
            HideLabelMoving();
            LabelMovingStopWatch.Reset();
        }
    }

    private async void FormMain_Shown(object? sender, EventArgs e)
    {
        if (Once)
        {
            // Startup
            if (Program.Startup) Hide();

            Once = false;
        }

        // Fix Microsoft bugs. Like always!
        await ScreenHighDpiScaleStartup(this);
    }

    private void FormMain_Move(object? sender, EventArgs e)
    {
        SplitContainerMain.Visible = false;
        LabelMoving.Text = "Now Moving...";
        LabelMoving.Location = new((ClientSize.Width / 2) - (LabelMoving.Width / 2), (ClientSize.Height / 2) - (LabelMoving.Height / 2));
        LabelMoving.Visible = true;
        LabelMoving.BringToFront();
        LabelMovingStopWatch.Restart();
    }

    private void FormMain_Resize(object? sender, EventArgs e)
    {
        if (WindowState != LastWindowState)
        {
            LastWindowState = WindowState;
        }
        else
        {
            SplitContainerMain.Visible = false;
            LabelMoving.Text = "Now Resizing...";
            LabelMoving.Location = new((ClientSize.Width / 2) - (LabelMoving.Width / 2), (ClientSize.Height / 2) - (LabelMoving.Height / 2));
            LabelMoving.Visible = true;
            LabelMoving.BringToFront();
            LabelMovingStopWatch.Restart();
        }
    }

    private void FormMain_LocationChanged(object? sender, EventArgs e)
    {
        LabelMovingStopWatch.Restart();
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

        HideLabelMoving();
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

    private void CustomButtonEditCustomServers_Click(object sender, EventArgs e)
    {
        ToolStripItem tsiEdit = new ToolStripMenuItem("Manage custom servers");
        tsiEdit.Font = Font;
        tsiEdit.Click -= tsiEdit_Click;
        tsiEdit.Click += tsiEdit_Click;
        void tsiEdit_Click(object? sender, EventArgs e)
        {
            edit();
        }

        ToolStripItem tsiViewWorkingServers = new ToolStripMenuItem("View working servers");
        tsiViewWorkingServers.Font = Font;
        tsiViewWorkingServers.Click -= tsiViewWorkingServers_Click;
        tsiViewWorkingServers.Click += tsiViewWorkingServers_Click;
        void tsiViewWorkingServers_Click(object? sender, EventArgs e)
        {
            viewWorkingServers();
        }

        ToolStripItem tsiClearWorkingServers = new ToolStripMenuItem("Clear working servers");
        tsiClearWorkingServers.Font = Font;
        tsiClearWorkingServers.Click -= tsiClearWorkingServers_Click;
        tsiClearWorkingServers.Click += tsiClearWorkingServers_Click;
        void tsiClearWorkingServers_Click(object? sender, EventArgs e)
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

        CustomContextMenuStrip cms = new();
        cms.Font = Font;
        cms.Items.Clear();
        cms.Items.Add(tsiEdit);
        cms.Items.Add(tsiViewWorkingServers);
        cms.Items.Add(tsiClearWorkingServers);
        Theme.SetColors(cms);
        Control b = CustomButtonEditCustomServers;
        cms.RoundedCorners = 5;
        cms.Show(b, 0, 0);

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
            int notepad = ProcessManager.ExecuteOnly(out Process _, "notepad", SecureDNS.WorkingServersPath, false, false, SecureDNS.CurrentPath);
            if (notepad == -1)
            {
                string msg = "Notepad is not installed on your system.";
                CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
            }
        }
    }

    private void CustomButtonCheck_Click(object? sender, EventArgs? e)
    {
        if (IsInAction(true, true, true, false, true, true, true, true, true, false, true)) return;
        StartCheck(null);
    }

    private async void CustomButtonQuickConnect_Click(object sender, EventArgs e)
    {
        await StartQuickConnect(null);
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

    private void CustomButtonEnableDisableNic_Click(object sender, EventArgs e)
    {
        if (CustomComboBoxNICs.SelectedItem == null) return;
        string? nicName = CustomComboBoxNICs.SelectedItem.ToString();
        if (string.IsNullOrEmpty(nicName)) return;
        if (CustomButtonEnableDisableNic.Text.Contains("Enable"))
            NetworkTool.EnableNIC(nicName);
        else
            NetworkTool.DisableNIC(nicName);
    }

    private async void CustomButtonSetDNS_Click(object sender, EventArgs e)
    {
        if (IsInAction(true, true, true, true, true, true, true, true, true, false, true)) return;
        await SetDNS();
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
        UpdateProxyBools = false;
        if (ProcessManager.FindProcessByPID(PIDFakeProxy))
            await ApplyPDpiChangesFakeProxy();
        await ApplyPDpiChanges();
        UpdateProxyBools = true;
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
        int notepad = ProcessManager.ExecuteOnly(out Process _, "notepad", SecureDNS.DPIBlacklistPath, false, false, SecureDNS.CurrentPath);
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
        await FlushDnsOnExit(true);
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
        int notepad = ProcessManager.ExecuteOnly(out Process _, "notepad", SecureDNS.FakeDnsRulesPath, false, false, SecureDNS.CurrentPath);
        if (notepad == -1)
        {
            string msg = "Notepad is not installed on your system.";
            CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
        }
    }

    private void CustomButtonSettingProxyBlackWhiteList_Click(object sender, EventArgs e)
    {
        FileDirectory.CreateEmptyFile(SecureDNS.BlackWhiteListPath);
        int notepad = ProcessManager.ExecuteOnly(out Process _, "notepad", SecureDNS.BlackWhiteListPath, false, false, SecureDNS.CurrentPath);
        if (notepad == -1)
        {
            string msg = "Notepad is not installed on your system.";
            CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
        }
    }

    private void CustomButtonSettingProxyDontBypass_Click(object sender, EventArgs e)
    {
        FileDirectory.CreateEmptyFile(SecureDNS.DontBypassListPath);
        int notepad = ProcessManager.ExecuteOnly(out Process _, "notepad", SecureDNS.DontBypassListPath, false, false, SecureDNS.CurrentPath);
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

    //============================== Closing
    private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            //ShowInTaskbar = false; // Makes Titlebar white (I use Show and Hide instead)
            NotifyIconMain.BalloonTipText = "Minimized to tray.";
            NotifyIconMain.BalloonTipIcon = ToolTipIcon.Info;
            NotifyIconMain.ShowBalloonTip(500);
        }
        else if (e.CloseReason == CloseReason.WindowsShutDown)
        {
            e.Cancel = true;
            Exit_Click(null, null);
        }
    }

    private void NotifyIconMain_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            this.SetDarkTitleBar(true); // Just in case
            Show();
            Opacity = 1;
            BringToFront();
        }
        else if (e.Button == MouseButtons.Right)
        {
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