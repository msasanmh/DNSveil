using MsmhToolsClass;
using MsmhToolsClass.MsmhProxyServer;
using MsmhToolsClass.ProxyServerPrograms;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reflection;

namespace SDCProxyServer;

public static partial class Program
{
    // Defaults Settings
    private const int DefaultPort = 8080;
    private const int DefaultPortMin = 1024;
    private const int DefaultPortMax = 65535;
    private const int DefaultMaxRequests = 600;
    private const int DefaultMaxRequestsMin = 20;
    private const int DefaultMaxRequestsMax = 1000000;
    private const int DefaultRequestTimeoutSec = 40;
    private const int DefaultRequestTimeoutSecMin = 0;
    private const int DefaultRequestTimeoutSecMax = 600;
    private const float DefaultKillOnCpuUsage = 40;
    private const int DefaultKillOnCpuUsageMin = 10;
    private const int DefaultKillOnCpuUsageMax = 100;
    private const bool DefaultBlockPort80 = true;
    // Defaults SSL Settings
    private const bool DefaultSSLEnable = false;
    private const bool DefaultSSLChangeSni = false;
    // Defaults Dns
    private const int DefaultDnsTimeoutSec = 3;
    private const int DefaultDnsTimeoutSecMin = 2;
    private const int DefaultDnsTimeoutSecMax = 10;
    // Defaults Fragment
    private const int DefaultFragmentBeforeSniChunks = 50;
    private const int DefaultFragmentBeforeSniChunksMin = 1;
    private const int DefaultFragmentBeforeSniChunksMax = 500;
    private static readonly string DefaultFragmentChunkModeStr = Key.Programs.Fragment.Mode.Program.ChunkMode.SNI;
    private const ProxyProgram.Fragment.ChunkMode DefaultFragmentChunkMode = ProxyProgram.Fragment.ChunkMode.SNI;
    private const int DefaultFragmentSniChunks = 5;
    private const int DefaultFragmentSniChunksMin = 1;
    private const int DefaultFragmentSniChunksMax = 500;
    private const int DefaultFragmentAntiPatternOffset = 2;
    private const int DefaultFragmentAntiPatternOffsetMin = 0;
    private const int DefaultFragmentAntiPatternOffsetMax = 50;
    private const int DefaultFragmentFragmentDelay = 1;
    private const int DefaultFragmentFragmentDelayMin = 0;
    private const int DefaultFragmentFragmentDelayMax = 500;
    // Defaults UpStreamProxy
    private const int DefaultUpStreamProxyPortMin = 1;
    private const int DefaultUpStreamProxyPortMax = 65535;
    private const bool DefaultUpStreamProxyOnlyApplyToBlockedIPs = true;

    private static readonly List<string> LoadCommands = new();
    private static readonly ProxySettings Settings = new();
    private static readonly SettingsSSL SettingsSSL_ = new(false);
    private static MsmhProxyServer ProxyServer { get; set; } = new();
    private static ProxyProgram.Dns DnsProgram { get; set; } = new();
    private static ProxyProgram.Fragment FragmentStaticProgram { get; set; } = new();
    private static ProxyProgram.UpStreamProxy UpStreamProxyProgram { get; set; } = new();
    private static ProxyProgram.Rules RulesProgram { get; set; } = new();

    private static readonly Stopwatch StopWatchShowRequests = new();
    private static readonly Stopwatch StopWatchShowChunkDetails = new();

    private static bool WriteRequestsToLog { get; set; } = false;
    private static bool WriteChunkDetailsToLog { get; set; } = false;
    private static int ParentPID { get; set; } = -1;


    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static async Task Main()
    {
        // Title
        string title = $"Msmh Proxy Server v{Assembly.GetExecutingAssembly().GetName().Version}";
        if (OperatingSystem.IsWindows()) Console.Title = title;

        // Invariant Culture
        Info.SetCulture(CultureInfo.InvariantCulture);

        // First Help
        WriteToStdout(title);
        WriteToStdout("Type \"Help\" To Get Help.");

        // Defaults
        Settings.BlockPort80 = DefaultBlockPort80;
        DnsProgram.Set(ProxyProgram.Dns.Mode.System, null, null, DefaultDnsTimeoutSec);

        // OnRequestReceived
        ProxyServer.OnRequestReceived -= ProxyServer_OnRequestReceived;
        ProxyServer.OnRequestReceived += ProxyServer_OnRequestReceived;
        static void ProxyServer_OnRequestReceived(object? sender, EventArgs e)
        {
            if (WriteRequestsToLog && ProxyServer.IsRunning)
                if (sender is string msg)
                {
                    if (!StopWatchShowRequests.IsRunning) StopWatchShowRequests.Start();
                    if (StopWatchShowRequests.ElapsedMilliseconds > 200)
                    {
                        WriteToStderr(msg);

                        StopWatchShowRequests.Stop();
                        StopWatchShowRequests.Reset();
                    }
                }
        }

        // OnChunkDetailsReceived
        FragmentStaticProgram.OnChunkDetailsReceived -= FragmentStaticProgram_OnChunkDetailsReceived;
        FragmentStaticProgram.OnChunkDetailsReceived += FragmentStaticProgram_OnChunkDetailsReceived;
        static void FragmentStaticProgram_OnChunkDetailsReceived(object? sender, EventArgs e)
        {
            if (WriteChunkDetailsToLog && ProxyServer.IsRunning && ProxyServer.IsFragmentActive)
                if (sender is string msg)
                {
                    if (!StopWatchShowChunkDetails.IsRunning) StopWatchShowChunkDetails.Start();
                    if (StopWatchShowChunkDetails.ElapsedMilliseconds > 200)
                    {
                        WriteToStderr(msg);

                        StopWatchShowChunkDetails.Stop();
                        StopWatchShowChunkDetails.Reset();
                    }
                }
        }

        // Exit When Parent Terminated
        ExitAuto();

        // Read Commands
        await ReadCommands();
    }

    private static async void ExitAuto()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(1000);
                if (ParentPID == -1 || ParentPID == 0) continue;
                bool isParentExist = ProcessManager.FindProcessByPID(ParentPID);
                if (!isParentExist)
                {
                    Environment.Exit(0);
                }
            }
        });
    }

    private static async Task ReadCommands()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                string? input = Console.ReadLine();
                await ExecuteCommands(input);
            }
        });
    }

    private static void MainDetails()
    {
        // Main Details - Machine Read
        string mainDetails = $"details|{ProxyServer.IsRunning}|"; // 1
        mainDetails += $"{ProxyServer.ListeningPort}|"; // 2
        mainDetails += $"{ProxyServer.ActiveTunnels}|"; // 3
        mainDetails += $"{ProxyServer.MaxRequests}|"; // 4
        mainDetails += $"{ProxyServer.SettingsSSL_.EnableSSL}|"; // 5
        mainDetails += $"{ProxyServer.SettingsSSL_.ChangeSni}|"; // 6
        mainDetails += $"{ProxyServer.DNSProgram.DNSMode}|"; // 7
        mainDetails += $"{ProxyServer.IsFragmentActive}|"; // 8
        mainDetails += $"{MsmhProxyServer.StaticFragmentProgram.FragmentMode}|"; // 9
        mainDetails += $"{ProxyServer.FragmentProgram.FragmentMode}|"; // 10
        mainDetails += $"{ProxyServer.UpStreamProxyProgram.UpStreamMode}|"; // 11
        mainDetails += $"{ProxyServer.RulesProgram.RulesMode}|"; // 12

        WriteToStdout(mainDetails);
    }

    private static void ShowParentProcessMsg()
    {
        string pid = ParentPID != -1 && ParentPID != 0 ? $"{ParentPID}" : "No Parent";
        string msg = $"\nParent Process ID: {pid}";
        WriteToStdout(msg, ConsoleColor.Green);
    }

    private static void ShowSettingsMsg()
    {
        string msg = "\nProxy Settings:";
        if (ProxyServer.IsRunning)
        {
            msg += $"\nListning On: {IPAddress.Loopback}:{Settings.ListenerPort}";
            IPAddress? localIp = NetworkTool.GetLocalIPv4();
            if (localIp != null)
                msg += $" And {localIp}:{Settings.ListenerPort}";
            else
                msg += $" And {Settings.ListenerIpAddress}:{Settings.ListenerPort}";
        }
        else
            msg += $"\nPort: {Settings.ListenerPort}";
        msg += $"\nMax Threads: {Settings.MaxRequests}";
        msg += $"\nRequest Timeout: {Settings.RequestTimeoutSec} Seconds";
        msg += $"\nKill On Cpu Usage: {Settings.KillOnCpuUsage}%";
        msg += $"\nBlock Port 80: {Settings.BlockPort80}";
        WriteToStdout(msg, ConsoleColor.Blue);

        // Save Command To List
        string baseCmd = Key.Setting.Name;
        string cmd = $"{baseCmd} -{Key.Setting.Port}={Settings.ListenerPort}";
        cmd += $" -{Key.Setting.MaxRequests}={Settings.MaxRequests}";
        cmd += $" -{Key.Setting.RequestTimeoutSec}={Settings.RequestTimeoutSec}";
        cmd += $" -{Key.Setting.KillOnCpuUsage}={Settings.KillOnCpuUsage}";
        cmd += $" -{Key.Setting.BlockPort80}={Settings.BlockPort80}";
        LoadCommands.AddCommand(baseCmd, cmd);
    }

    private static void ShowSettingsSSLMsg()
    {
        string msg = "\nSSL Settings:";
        msg += $"\nEnabled: {ProxyServer.SettingsSSL_.EnableSSL}";
        if (!string.IsNullOrEmpty(ProxyServer.SettingsSSL_.RootCA_Path))
        {
            msg += $"\nRootCA_Path:";
            msg += $"\n{ProxyServer.SettingsSSL_.RootCA_Path}";
        }
        if (!string.IsNullOrEmpty(ProxyServer.SettingsSSL_.RootCA_KeyPath))
        {
            msg += $"\nRootCA_KeyPath:";
            msg += $"\n{ProxyServer.SettingsSSL_.RootCA_KeyPath}";
        }
        msg += $"\nChange SNI: {ProxyServer.SettingsSSL_.ChangeSni}";
        if (!string.IsNullOrEmpty(ProxyServer.SettingsSSL_.DefaultSni) && !string.IsNullOrWhiteSpace(ProxyServer.SettingsSSL_.DefaultSni))
            msg += $"\nDefault SNI: {ProxyServer.SettingsSSL_.DefaultSni}";
        else
            msg += "\nDefault SNI: Empty (Original SNI Will Be Used)";
        WriteToStdout(msg, ConsoleColor.Blue);

        // Save Command To List
        string baseCmd = Key.SSLSetting.Name;
        string cmd = $"{baseCmd} -{Key.SSLSetting.Enable}={ProxyServer.SettingsSSL_.EnableSSL}";
        cmd += $" -{Key.SSLSetting.RootCA_Path}=\"{ProxyServer.SettingsSSL_.RootCA_Path}\"";
        cmd += $" -{Key.SSLSetting.RootCA_KeyPath}=\"{ProxyServer.SettingsSSL_.RootCA_KeyPath}\"";
        cmd += $" -{Key.SSLSetting.ChangeSni}={ProxyServer.SettingsSSL_.ChangeSni}";
        cmd += $" -{Key.SSLSetting.DefaultSni}={ProxyServer.SettingsSSL_.DefaultSni}";
        LoadCommands.AddCommand(baseCmd, cmd);
    }

    private static void ShowDnsMsg()
    {
        string result = $"\n{Key.Programs.Dns.Name} Mode: {DnsProgram.DNSMode}";
        result += $"\nTimeout: {DnsProgram.TimeoutSec} Seconds";
        if (DnsProgram.DNSMode == ProxyProgram.Dns.Mode.DoH)
        {
            result += $"\nDoH: {DnsProgram.DNS}";
            if (!string.IsNullOrEmpty(DnsProgram.DnsCleanIp))
                result += $"\nDoH Clean IP: {DnsProgram.DnsCleanIp}";
            if (!string.IsNullOrEmpty(DnsProgram.ProxyScheme))
                result += $"\nDoH Is Using Proxy: {DnsProgram.ProxyScheme}";
        }
        if (DnsProgram.DNSMode == ProxyProgram.Dns.Mode.PlainDNS)
        {
            result += $"\nDns Address: {DnsProgram.DNS}";
            if (!string.IsNullOrEmpty(DnsProgram.DnsCleanIp))
                result += $"\nDns Clean IP: {DnsProgram.DnsCleanIp}";
        }
        string cfIpRange = string.Empty;
        if (DnsProgram.ChangeCloudflareIP && !string.IsNullOrEmpty(DnsProgram.CloudflareCleanIP))
        {
            result += $"\nCF Clean IP: {DnsProgram.CloudflareCleanIP}";
            List<string> list = DnsProgram.CloudflareIPs;
            if (list.Any())
            {
                result += $"\nCF IP Range:";
                for (int n = 0; n < list.Count; n++)
                {
                    cfIpRange += $"{list[n]}\\n";
                    result += $"\n{list[n]}";
                }
            }
        }
        WriteToStdout(result, ConsoleColor.Green);

        // Save Command To List
        string baseCmd = $"{Key.Programs.Name} {Key.Programs.Dns.Name}";
        string cmd = $"{baseCmd} -{Key.Programs.Dns.Mode.Name}={DnsProgram.DNSMode}";
        cmd += $" -{Key.Programs.Dns.TimeoutSec}={DnsProgram.TimeoutSec}";
        cmd += $" -{Key.Programs.Dns.DnsAddr}={DnsProgram.DNS}";
        cmd += $" -{Key.Programs.Dns.DnsCleanIp}={DnsProgram.DnsCleanIp}";
        cmd += $" -{Key.Programs.Dns.ProxyScheme}={DnsProgram.ProxyScheme}";
        cmd += $" -{Key.Programs.Dns.CfCleanIp}={DnsProgram.CloudflareCleanIP}";
        cmd += $" -{Key.Programs.Dns.CfIpRange}=\"{cfIpRange.TrimEnd("\\n".ToCharArray())}\"";
        LoadCommands.AddCommand(baseCmd, cmd);
    }

    private static void ShowFragmentMsg()
    {
        string result = $"\n{Key.Programs.Fragment.Name} Mode: {FragmentStaticProgram.FragmentMode}";
        if (FragmentStaticProgram.FragmentMode == ProxyProgram.Fragment.Mode.Program)
        {
            result += $"\nBefore Sni Chunks: {FragmentStaticProgram.BeforeSniChunks}";
            result += $"\nChunks Mode: {FragmentStaticProgram.DPIChunkMode}";
            result += $"\n\"{FragmentStaticProgram.DPIChunkMode}\" Chunks: {FragmentStaticProgram.SniChunks}";
            result += $"\nAnti-Pattern Offset: {FragmentStaticProgram.AntiPatternOffset} Chunks";
            result += $"\nFragment Delay: {FragmentStaticProgram.FragmentDelay} ms";
        }
        WriteToStdout(result, ConsoleColor.Green);

        // Save Command To List
        string baseCmd = $"{Key.Programs.Name} {Key.Programs.Fragment.Name}";
        string cmd = $"{baseCmd} -{Key.Programs.Fragment.Mode.Name}={FragmentStaticProgram.FragmentMode}";
        cmd += $" -{Key.Programs.Fragment.Mode.Program.BeforeSniChunks}={FragmentStaticProgram.BeforeSniChunks}";
        cmd += $" -{Key.Programs.Fragment.Mode.Program.ChunkMode.Name}={FragmentStaticProgram.DPIChunkMode}";
        cmd += $" -{Key.Programs.Fragment.Mode.Program.SniChunks}={FragmentStaticProgram.SniChunks}";
        cmd += $" -{Key.Programs.Fragment.Mode.Program.AntiPatternOffset}={FragmentStaticProgram.AntiPatternOffset}";
        cmd += $" -{Key.Programs.Fragment.Mode.Program.FragmentDelay}={FragmentStaticProgram.FragmentDelay}";
        LoadCommands.AddCommand(baseCmd, cmd);
    }

    private static void ShowRulesMsg()
    {
        WriteToStdout($"\n{Key.Programs.Rules.Name} Mode: {RulesProgram.RulesMode}", ConsoleColor.Green);
        if (RulesProgram.RulesMode != ProxyProgram.Rules.Mode.Disable)
            WriteToStdout($"Rules:\n{RulesProgram.PathOrText}", ConsoleColor.Green);

        // Save Command To List
        string baseCmd = $"{Key.Programs.Name} {Key.Programs.Rules.Name}";
        string cmd = $"{baseCmd} -{Key.Programs.Rules.Mode.Name}={RulesProgram.RulesMode}";
        cmd += $" -{Key.Programs.Rules.PathOrText}=\"{RulesProgram.PathOrText.Replace(Environment.NewLine, "\\n")}\"";
        LoadCommands.AddCommand(baseCmd, cmd);
    }

    private static void ShowUpStreamProxyMsg()
    {
        string result = $"\n{Key.Programs.UpStreamProxy.Name} Mode: {UpStreamProxyProgram.UpStreamMode}";
        if (UpStreamProxyProgram.UpStreamMode != ProxyProgram.UpStreamProxy.Mode.Disable)
        {
            result += $"\nProxy Address: {UpStreamProxyProgram.ProxyHost}:{UpStreamProxyProgram.ProxyPort}";
            result += $"\nApply Only To Blocked IPs: {UpStreamProxyProgram.OnlyApplyToBlockedIps}";
        }
        WriteToStdout(result, ConsoleColor.Green);

        // Save Command To List
        string baseCmd = $"{Key.Programs.Name} {Key.Programs.UpStreamProxy.Name}";
        string cmd = $"{baseCmd} -{Key.Programs.UpStreamProxy.Mode.Name}={UpStreamProxyProgram.UpStreamMode}";
        cmd += $" -{Key.Programs.UpStreamProxy.Host}={UpStreamProxyProgram.ProxyHost}";
        cmd += $" -{Key.Programs.UpStreamProxy.Port}={UpStreamProxyProgram.ProxyPort}";
        cmd += $" -{Key.Programs.UpStreamProxy.OnlyApplyToBlockedIPs}={UpStreamProxyProgram.OnlyApplyToBlockedIps}";
        LoadCommands.AddCommand(baseCmd, cmd);
    }

    private static void ShowStatus()
    {
        string result = $"\nProxy Server Running: {ProxyServer.IsRunning}";
        WriteToStdout(result, ConsoleColor.Blue);
        ShowParentProcessMsg();
        ShowSettingsMsg();
        ShowSettingsSSLMsg();
        ShowDnsMsg();
        ShowFragmentMsg();
        ShowUpStreamProxyMsg();
        ShowRulesMsg();
    }

    private static void WriteToStdout(string msg, ConsoleColor consoleColor = ConsoleColor.White)
    {
        Console.ForegroundColor = consoleColor;
        Console.Out.WriteLine(msg);
        Console.ResetColor();
    }

    private static void WriteToStderr(string msg, ConsoleColor consoleColor = ConsoleColor.White)
    {
        Console.ForegroundColor = consoleColor;
        Console.Error.WriteLine(msg);
        Console.ResetColor();
    }
}