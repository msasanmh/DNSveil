using CustomControls;
using MsmhToolsClass;
using MsmhToolsWinFormsClass;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MsmhToolsClass.ProxyServerPrograms;
using IPAddress = System.Net.IPAddress;

namespace SecureDNSClient;

public partial class FormMain : Form
{
    public static bool IsThemeApplied { get; set; } = false;
    public static bool IsScreenHighDpiScaleApplied { get; set; } = false;
    public static bool IsAppReady { get; set; } = false;
    public static readonly Stopwatch AppUpTime = new();
    public static readonly Architecture ArchOs = RuntimeInformation.OSArchitecture;
    public static readonly Architecture ArchProcess = RuntimeInformation.ProcessArchitecture;
    public static int UpdateAutoDelayMS { get; set; } = 500; // Default
    public static int UpdateDnsDohDelayMS { get; set; } = 500; // Default
    private readonly CheckDns ScanDns = new(false, false, ProcessPriorityClass.Normal);
    private string BoolsChangedText { get; set; } = string.Empty;

    public static ToolTip MainToolTip { get; set; } = new();
    private FormWindowState LastWindowState;
    public static readonly CustomLabel LabelScreen = new();
    private static float BaseScreenDpi = 96f; // 100%
    private readonly Stopwatch LabelMainStopWatch = new();
    private static readonly CustomLabel LabelMain = new();
    public List<DnsInfo> WorkingDnsList { get; set; } = new();
    private List<DnsInfo> WorkingDnsListToFile { get; set; } = new();
    public List<DnsInfo> CurrentUsingCustomServersList { get; set; } = new();
    public List<string> SavedDnsList { get; set; } = new();
    public List<string> SavedEncodedDnsList { get; set; } = new();
    public static ProcessMonitor MonitorProcess { get; set; } = new();
    private bool InternetOnline = false;
    private bool InternetOffline = true;
    public static bool IsInternetOnline { get; set; } = false;
    private bool Once { get; set; } = true;
    private bool Once2 { get; set; } = true;
    public static bool IsInActionState { get; set; } = false;
    private bool IsOnStartup { get; set; } = false;
    private bool IsStartupPathOk { get; set; } = false;
    private bool StartupTaskExecuted { get; set; } = false;
    private bool IsCheckingForUpdate { get; set; } = false;
    private int NumberOfWorkingServers { get; set; } = 0;
    public static bool IsCheckingStarted { get; set; } = false;
    private static bool StopChecking { get; set; } = false;
    private bool IsConnecting { get; set; } = false;
    private bool IsDisconnecting { get; set; } = false;
    private bool IsDisconnectingAll { get; set; } = false;
    private bool IsReconnecting { get; set; } = false;
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
    private bool IsDNSSetOn { get; set; } = false;
    private List<string> LastNicNameList { get; set; } = new();
    private SetDnsOnNic SetDnsOnNic_ { get; set; } = new();
    private bool DoesDNSSetOnce { get; set; } = false;
    private bool IsFlushingDns { get; set; } = false;
    private bool IsDnsFlushed { get; set; } = false; // On Unset
    private bool IsDnsFullFlushed { get; set; } = false; // On Exit and Tools when Dns is not set.
    private bool IsDPIActive { get; set; } = false;
    private bool IsGoodbyeDPIBasicActive { get; set; } = false;
    private bool IsGoodbyeDPIAdvancedActive { get; set; } = false;
    public static IPAddress? LocalIP { get; set; } = IPAddress.Loopback; // as default
    public Settings? AppSettings { get; set; }
    private readonly ToolStripMenuItem ToolStripMenuItemIcon = new();
    private bool AudioAlertOnline = true;
    private bool AudioAlertOffline = false;
    private bool AudioAlertRequestsExceeded = false;
    private readonly Stopwatch StopWatchCheckDPIWorks = new();
    private readonly Stopwatch StopWatchAudioAlertDelay = new();
    private readonly Stopwatch StopWatchWriteProxyOutputDelay = new();
    private string TheDll = string.Empty;
    private static readonly string NL = Environment.NewLine;
    private static bool IsExiting { get; set; } = false;

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
    private bool IsProxyFragmentActive { get; set; } = false;
    private ProxyProgram.Dns.Mode ProxyDNSMode { get; set; } = ProxyProgram.Dns.Mode.Disable;
    private ProxyProgram.Fragment.Mode ProxyStaticFragmentMode { get; set; } = ProxyProgram.Fragment.Mode.Disable;
    private ProxyProgram.Fragment.Mode ProxyFragmentMode { get; set; } = ProxyProgram.Fragment.Mode.Disable;
    private ProxyProgram.UpStreamProxy.Mode ProxyUpStreamMode { get; set; } = ProxyProgram.UpStreamProxy.Mode.Disable;
    private ProxyProgram.Rules.Mode ProxyRulesMode { get; set; } = ProxyProgram.Rules.Mode.Disable;
    private bool IsProxySSLDecryptionActive { get; set; } = false;
    private bool IsProxySSLChangeSniActive { get; set; } = false;
    private bool IsProxySet { get; set; } = false;
    private bool IsAnotherProxySet { get; set; } = false;
    private string CurrentSystemProxy { get; set; } = string.Empty;
    private static bool UpdateProxyBools { get; set; } = true;
    private string LastFragmentProgramCommand { get; set; } = string.Empty;
    private string LastDefaultSni { get; set; } = string.Empty;
    private ProxyProgram.Rules CheckProxyRules { get; set; } = new();
    private string LastProxyRulesPath { get; set; } = string.Empty;
    private string LastProxyRulesContent { get; set; } = string.Empty;

    // Fake Proxy
    private ProcessConsole FakeProxyConsole { get; set; } = new();
    private static int PIDFakeProxy { get; set; } = -1;
    private bool IsFakeProxyActivated { get; set; } = false;

    // Check DPI Bypass Cancel Token
    private Task? CheckDpiBypass;
    private CancellationTokenSource CheckDpiBypassCTS = new();

    // Menus
    private readonly CustomContextMenuStrip CMS = new();

    // Menu: Custom Servers
    private readonly ToolStripMenuItem TsiEdit = new("Manage custom servers");
    private readonly ToolStripMenuItem TsiViewWorkingServers = new("View working servers");
    private readonly ToolStripMenuItem TsiClearWorkingServers = new("Clear working servers");

    // Menu: Scan
    private readonly ToolStripMenuItem TsiClearCheckedServers = new("Clear Checked Servers");
    private readonly ToolStripMenuItem TsiRescanCheckedServers = new("Rescan Checked Servers");
    private readonly ToolStripMenuItem TsiScanBuiltIn = new("Built-In Servers");

    // Menu: Main Context Menu & Quick Connect Button
    private readonly ToolStripMenuItem TsiGoodbyeDpiBasic = new("Activate GoodbyeDPI Basic");
    private readonly ToolStripMenuItem TsiGoodbyeDpiAdvanced = new();
    private readonly ToolStripMenuItem TsiGoodbyeDpiDeactive = new("Deactive GoodbyeDPI");
    private readonly ToolStripMenuItem TsiProxy = new();
    private readonly ToolStripMenuItem TsiProxySet = new();
    private readonly ToolStripMenuItem TsiQuickConnectTo = new("Quick Connect To");
    private readonly ToolStripMenuItem TsiQcToUserSetting = new("User Settings");
    private readonly ToolStripMenuItem TsiQcToBuiltIn = new("Built-In Servers");
}