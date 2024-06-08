using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reflection;

namespace SDCAgnosticServer;

public static partial class Program
{
    // Defaults AgnosticSettings
    private const int DefaultPort = 8080;
    private const int DefaultPortMin = 53;
    private const int DefaultPortMax = 65535;
    private const AgnosticSettings.WorkingMode DefaultWorkingMode = AgnosticSettings.WorkingMode.DnsAndProxy;
    private const int DefaultMaxRequests = 5000;
    private const int DefaultMaxRequestsMin = 20;
    private const int DefaultMaxRequestsMax = int.MaxValue;
    private const int DefaultDnsTimeoutSec = 5;
    private const int DefaultDnsTimeoutSecMin = 3;
    private const int DefaultDnsTimeoutSecMax = 10;
    private const int DefaultProxyTimeoutSec = 40;
    private const int DefaultProxyTimeoutSecMin = 0;
    private const int DefaultProxyTimeoutSecMax = 600;
    private const float DefaultKillOnCpuUsage = 40;
    private const int DefaultKillOnCpuUsageMin = 10;
    private const int DefaultKillOnCpuUsageMax = 95;
    private const bool DefaultBlockPort80 = true;
    private const bool DefaultAllowInsecure = false;
    private static readonly IPAddress DefaultBootstrapIp = IPAddress.None;
    private const int DefaultBootstrapPort = 53;
    private const bool DefaultApplyUpstreamOnlyToBlockedIps = true;
    // Defaults SSL AgnosticSettings
    private const bool DefaultSSLEnable = false;
    private const bool DefaultSSLChangeSni = false;
    // Defaults Fragment
    private const int DefaultFragmentBeforeSniChunks = 50;
    private const int DefaultFragmentBeforeSniChunksMin = 1;
    private const int DefaultFragmentBeforeSniChunksMax = 500;
    private static readonly string DefaultFragmentChunkModeStr = Key.Programs.Fragment.Mode.Program.ChunkMode.SNI;
    private const AgnosticProgram.Fragment.ChunkMode DefaultFragmentChunkMode = AgnosticProgram.Fragment.ChunkMode.SNI;
    private const int DefaultFragmentSniChunks = 5;
    private const int DefaultFragmentSniChunksMin = 1;
    private const int DefaultFragmentSniChunksMax = 500;
    private const int DefaultFragmentAntiPatternOffset = 2;
    private const int DefaultFragmentAntiPatternOffsetMin = 0;
    private const int DefaultFragmentAntiPatternOffsetMax = 50;
    private const int DefaultFragmentFragmentDelay = 1;
    private const int DefaultFragmentFragmentDelayMin = 0;
    private const int DefaultFragmentFragmentDelayMax = 500;

    private static string Profile = string.Empty;
    private static readonly List<string> LoadCommands = new();

    private static readonly Stopwatch StopWatchShowRequests = new();
    private static readonly Stopwatch StopWatchShowChunkDetails = new();

    private static bool WriteRequestsToLog { get; set; } = false;
    private static uint CountRequests { get; set; } = 0;
    private static bool WriteFragmentDetailsToLog { get; set; } = false;
    private static int ParentPID { get; set; } = -1;
    private static ConcurrentDictionary<uint, string> WaitingInputCommands { get; set; } = new();

    public static async void ProxyServer_OnRequestReceived(object? sender, EventArgs e)
    {
        if (WriteRequestsToLog)
            if (sender is string msg)
            {
                CountRequests++;
                if (!StopWatchShowRequests.IsRunning) StopWatchShowRequests.Start();
                if (CountRequests < 5 || StopWatchShowRequests.ElapsedMilliseconds > 40)
                {
                    await WriteToStderrAsync(msg, ConsoleColor.DarkGray, false);

                    StopWatchShowRequests.Restart();
                }
            }
    }

    public static async void FragmentStaticProgram_OnChunkDetailsReceived(object? sender, EventArgs e)
    {
        if (WriteFragmentDetailsToLog)
            if (sender is string msg)
            {
                if (!StopWatchShowChunkDetails.IsRunning) StopWatchShowChunkDetails.Start();
                if (StopWatchShowChunkDetails.ElapsedMilliseconds > 50)
                {
                    await WriteToStderrAsync(msg, ConsoleColor.DarkCyan, false);

                    StopWatchShowChunkDetails.Restart();
                }
            }
    }

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static async Task Main()
    {
        // Title
        string title = $"Msmh Agnostic Server v{Assembly.GetExecutingAssembly().GetName().Version}";
        if (OperatingSystem.IsWindows()) Console.Title = title;
        
        // Invariant Culture
        Info.SetCulture(CultureInfo.InvariantCulture);

        // First Help
        await WriteToStdoutAsync(title);
        await WriteToStdoutAsync("Type \"Help\" To Get Help.");

        // Exit When Parent Terminated
        ExitAuto();

        // Execute Wainting Commands
        ExecuteCommands();

        // Read Commands
        await ReadCommandsAsync();
    }

    private static async void ExitAuto()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(2000);
                if (ParentPID == -1 || ParentPID == 0) continue;
                bool isParentExist = ProcessManager.FindProcessByPID(ParentPID);
                if (!isParentExist)
                {
                    Environment.Exit(0);
                    await ProcessManager.KillProcessByPidAsync(Environment.ProcessId);
                }
            }
        });
    }

    private static async Task ReadCommandsAsync()
    {
        await Task.Run(() =>
        {
            uint n = 0;
            while (true)
            {
                try
                {
                    string? input = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        n++;
                        WaitingInputCommands.TryAdd(n, input);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ReadCommandsAsync: " + ex.Message);
                }
            }
        });
    }

    private static async void ExecuteCommands()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    if (WaitingInputCommands.IsEmpty) await Task.Delay(50);
                    else
                    {
                        while (true)
                        {
                            int n1 = WaitingInputCommands.Count;
                            await Task.Delay(200);
                            int n2 = WaitingInputCommands.Count;
                            if (n1 == n2) break;
                        }

                        List<KeyValuePair<uint, string>> commandsList = WaitingInputCommands.ToList();
                        var sortedCommandsList = commandsList.OrderBy(x => x.Key);
                        foreach (KeyValuePair<uint, string> command in sortedCommandsList)
                        {
                            await ExecuteCommandsAsync(command.Value);
                            WaitingInputCommands.TryRemove(command.Key, out _);
                            //Debug.WriteLine($"{command.Key}");
                            await Task.Delay(25);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ExecuteCommands: " + ex.Message);
                }
            }
        });
    }

    private static async Task MainDetailsAsync(string profileName)
    {
        try
        {
            // Main Details - Machine Read
            bool isMatch = false;
            foreach (ServerProfile sf in ServerProfiles)
            {
                if (sf.AgnosticServer != null && sf.Settings != null && !string.IsNullOrEmpty(sf.Name))
                {
                    if (profileName.Equals(sf.Name))
                    {
                        string mainDetails = $"details|{sf.AgnosticServer.IsRunning}|"; // 1
                        mainDetails += $"{sf.AgnosticServer.ListeningPort}|"; // 2
                        mainDetails += $"{sf.AgnosticServer.ActiveProxyTunnels}|"; // 3
                        mainDetails += $"{sf.AgnosticServer.MaxRequests}|"; // 4
                        mainDetails += $"{sf.AgnosticServer.SettingsSSL_.EnableSSL}|"; // 5
                        mainDetails += $"{sf.AgnosticServer.SettingsSSL_.ChangeSni}|"; // 6
                        mainDetails += $"{sf.AgnosticServer.IsFragmentActive}|"; // 7
                        mainDetails += $"{sf.AgnosticServer.FragmentProgram.FragmentMode}|"; // 8
                        mainDetails += $"{sf.AgnosticServer.DnsRulesProgram.RulesMode}|"; // 9
                        mainDetails += $"{sf.AgnosticServer.ProxyRulesProgram.RulesMode}|"; // 10
                        await WriteToStdoutAsync(mainDetails);
                        isMatch = true;
                        break;
                    }
                }
            }

            if (!isMatch) await WriteToStdoutAsync($"Wrong Profile Name", ConsoleColor.DarkRed);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("MainDetailsAsync: " + ex.Message);
        }
    }

    private static async Task ShowProfileMsgAsync(bool save = true)
    {
        try
        {
            Profile = Profile.Trim();
            if (!string.IsNullOrEmpty(Profile))
            {
                await WriteToStdoutAsync($"{Environment.NewLine}{nameof(Profile)} Set To {Profile}", ConsoleColor.Cyan);

                if (save)
                {
                    // Add Or Update Server Profile
                    ServerProfile sf = new()
                    {
                        Name = Profile
                    };
                    AddProfile(sf);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ShowProfileMsgAsync: " + ex.Message);
        }
    }

    private static async Task ShowParentProcessMsgAsync()
    {
        string pid = ParentPID != -1 && ParentPID != 0 ? $"{ParentPID}" : "No Parent";
        string msg = $"\nParent Process ID: {pid}";
        await WriteToStdoutAsync(msg, ConsoleColor.Green);
    }

    private static async Task ShowSettingsMsgAsync(AgnosticSettings settings, bool save = true)
    {
        try
        {
            string msg = "\nSettings:";
            msg += $"\nPort: {settings.ListenerPort}";
            msg += $"\nWorking Mode: {settings.Working_Mode}";
            msg += $"\nMax Requests: {settings.MaxRequests}";
            msg += $"\nDNS Timeout: {settings.DnsTimeoutSec} Seconds";
            msg += $"\nProxy Timeout: {settings.ProxyTimeoutSec} Seconds";
            msg += $"\nKill On Cpu Usage: {settings.KillOnCpuUsage}%";
            msg += $"\nBlock Port 80: {settings.BlockPort80}";
            msg += $"\nAllow Insecure: {settings.AllowInsecure}";
            msg += $"\nDNS Servers Count: {settings.DNSs.Count}";
            if (!string.IsNullOrEmpty(settings.CloudflareCleanIP))
                msg += $"\nCloudflare Clean IP: {settings.CloudflareCleanIP}";
            msg += $"\nBootstrap IP Address: {settings.BootstrapIpAddress}";
            msg += $"\nBootstrap Port: {settings.BootstrapPort}";
            if (!string.IsNullOrEmpty(settings.UpstreamProxyScheme))
            {
                msg += $"\nUpstream Proxy Scheme: {settings.UpstreamProxyScheme}";
                if (!string.IsNullOrEmpty(settings.UpstreamProxyUser))
                {
                    msg += $"\nUpstream Proxy User: {settings.UpstreamProxyUser}";
                    if (!string.IsNullOrEmpty(settings.UpstreamProxyPass))
                        msg += $"\nUpstream Proxy Pass: {settings.UpstreamProxyPass}";
                }
                msg += $"\nApply Upstream Only To Blocked IPs: {settings.ApplyUpstreamOnlyToBlockedIps}";
            }
            await WriteToStdoutAsync(msg, ConsoleColor.Blue);

            if (save)
            {
                // Add Or Update Server Profile
                ServerProfile sf = new()
                {
                    Name = Profile,
                    Settings = settings
                };
                AddProfile(sf);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ShowSettingsMsgAsync: " + ex.Message);
        }
    }

    private static async Task ShowSettingsSSLMsgAsync(AgnosticSettingsSSL settingsSSL, bool save = true)
    {
        try
        {
            string msg = "\nSSL Settings:";
            msg += $"\nEnabled: {settingsSSL.EnableSSL}";
            if (settingsSSL.EnableSSL)
            {
                if (!string.IsNullOrEmpty(settingsSSL.RootCA_Path))
                {
                    msg += $"\nRootCA_Path:";
                    msg += $"\n{settingsSSL.RootCA_Path}";
                }
                if (!string.IsNullOrEmpty(settingsSSL.RootCA_KeyPath))
                {
                    msg += $"\nRootCA_KeyPath:";
                    msg += $"\n{settingsSSL.RootCA_KeyPath}";
                }
                if (!string.IsNullOrEmpty(settingsSSL.Cert_Path))
                {
                    msg += $"\nCert_Path:";
                    msg += $"\n{settingsSSL.Cert_Path}";
                }
                if (!string.IsNullOrEmpty(settingsSSL.Cert_KeyPath))
                {
                    msg += $"\nCert_KeyPath:";
                    msg += $"\n{settingsSSL.Cert_KeyPath}";
                }
                msg += $"\nChange SNI: {settingsSSL.ChangeSni}";
                if (!string.IsNullOrEmpty(settingsSSL.DefaultSni) && !string.IsNullOrWhiteSpace(settingsSSL.DefaultSni))
                    msg += $"\nDefault SNI: {settingsSSL.DefaultSni}";
                else
                    msg += "\nDefault SNI: Empty (Original SNI Will Be Used)";
            }
            await WriteToStdoutAsync(msg, ConsoleColor.Blue);

            if (save)
            {
                // Add Or Update Server Profile
                ServerProfile sf = new()
                {
                    Name = Profile,
                    SettingsSSL = settingsSSL
                };
                AddProfile(sf);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ShowSettingsSSLMsgAsync: " + ex.Message);
        }
    }

    private static async Task ShowFragmentMsgAsync(AgnosticProgram.Fragment fragmentProgram, bool save = true)
    {
        try
        {
            string result = $"\n{Key.Programs.Fragment.Name} Mode: {fragmentProgram.FragmentMode}";
            if (fragmentProgram.FragmentMode == AgnosticProgram.Fragment.Mode.Program)
            {
                result += $"\nBefore Sni Chunks: {fragmentProgram.BeforeSniChunks}";
                result += $"\nChunks Mode: {fragmentProgram.DPIChunkMode}";
                result += $"\n\"{fragmentProgram.DPIChunkMode}\" Chunks: {fragmentProgram.SniChunks}";
                result += $"\nAnti-Pattern Offset: {fragmentProgram.AntiPatternOffset} Chunks";
                result += $"\nFragment Delay: {fragmentProgram.FragmentDelay} ms";
            }
            await WriteToStdoutAsync(result, ConsoleColor.Green);

            if (save)
            {
                // Add Or Update Server Profile
                ServerProfile sf = new()
                {
                    Name = Profile,
                    Fragment = fragmentProgram
                };
                AddProfile(sf);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ShowFragmentMsgAsync: " + ex.Message);
        }
    }

    private static async Task ShowDnsRulesMsgAsync(AgnosticProgram.DnsRules dnsRulesProgram, bool save = true)
    {
        try
        {
            await WriteToStdoutAsync($"\n{Key.Programs.DnsRules.Name} Mode: {dnsRulesProgram.RulesMode}", ConsoleColor.Green);
            if (dnsRulesProgram.RulesMode != AgnosticProgram.DnsRules.Mode.Disable)
                await WriteToStdoutAsync($"Dns Rules:\n{dnsRulesProgram.PathOrText}", ConsoleColor.Green);

            if (save)
            {
                // Add Or Update Server Profile
                ServerProfile sf = new()
                {
                    Name = Profile,
                    DnsRules = dnsRulesProgram
                };
                AddProfile(sf);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ShowDnsRulesMsgAsync: " + ex.Message);
        }
    }

    private static async Task ShowProxyRulesMsgAsync(AgnosticProgram.ProxyRules proxyRulesProgram, bool save = true)
    {
        try
        {
            await WriteToStdoutAsync($"\n{Key.Programs.ProxyRules.Name} Mode: {proxyRulesProgram.RulesMode}", ConsoleColor.Green);
            if (proxyRulesProgram.RulesMode != AgnosticProgram.ProxyRules.Mode.Disable)
                await WriteToStdoutAsync($"Proxy Rules:\n{proxyRulesProgram.PathOrText}", ConsoleColor.Green);

            if (save)
            {
                // Add Or Update Server Profile
                ServerProfile sf = new()
                {
                    Name = Profile,
                    ProxyRules = proxyRulesProgram
                };
                AddProfile(sf);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ShowProxyRulesMsgAsync: " + ex.Message);
        }
    }

    private static async Task ShowDnsLimitMsgAsync(AgnosticProgram.DnsLimit dnsLimitProgram, bool save = true)
    {
        try
        {
            await WriteToStdoutAsync($"\n{Key.Programs.DnsLimit.Name} Enable: {dnsLimitProgram.EnableDnsLimit}", ConsoleColor.Green);
            if (dnsLimitProgram.EnableDnsLimit)
            {
                await WriteToStdoutAsync($"Disable Plain DNS: {dnsLimitProgram.DisablePlainDns}", ConsoleColor.Green);
                await WriteToStdoutAsync($"{Key.Programs.DnsLimit.DoHPathLimitMode.Name} Mode: {dnsLimitProgram.LimitDoHMode}", ConsoleColor.Green);
                if (dnsLimitProgram.LimitDoHMode != AgnosticProgram.DnsLimit.LimitDoHPathsMode.Disable)
                    await WriteToStdoutAsync($"DoH Paths:\n{dnsLimitProgram.PathOrText}", ConsoleColor.Green);
            }
            
            if (save)
            {
                // Add Or Update Server Profile
                ServerProfile sf = new()
                {
                    Name = Profile,
                    DnsLimit = dnsLimitProgram
                };
                AddProfile(sf);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ShowDnsRulesMsgAsync: " + ex.Message);
        }
    }

    private static async Task ShowStatusAsync()
    {
        foreach (ServerProfile sf in ServerProfiles)
        {
            if (sf.AgnosticServer != null && sf.Settings != null && !string.IsNullOrEmpty(sf.Name))
            {
                string result = $"\nServer {sf.Name} Running: {sf.AgnosticServer.IsRunning}";
                await WriteToStdoutAsync(result, ConsoleColor.Blue);

                if (sf.AgnosticServer.IsRunning)
                {
                    string addressMsg = string.Empty;
                    bool isIPv4Supported = NetworkTool.IsIPv4Supported();
                    bool isIPv6Supported = NetworkTool.IsIPv6Supported();

                    if (isIPv4Supported || isIPv6Supported)
                        addressMsg += $"\nListning On:";

                    if (isIPv4Supported)
                    {
                        addressMsg += $"\n{IPAddress.Loopback}:{sf.Settings.ListenerPort}";

                        IPAddress? localIPv4 = NetworkTool.GetLocalIPv4();
                        if (localIPv4 != null)
                            addressMsg += $"\n{localIPv4}:{sf.Settings.ListenerPort}";
                    }

                    if (isIPv6Supported)
                    {
                        addressMsg += $"\n{IPAddress.IPv6Loopback}:{sf.Settings.ListenerPort}";

                        IPAddress? localIPv6 = NetworkTool.GetLocalIPv6();
                        if (localIPv6 != null)
                            addressMsg += $"\n{localIPv6}:{sf.Settings.ListenerPort}";
                    }

                    await WriteToStdoutAsync(addressMsg, ConsoleColor.Blue);
                }

                if (sf.Settings != null) await ShowSettingsMsgAsync(sf.Settings, false);
                if (sf.SettingsSSL != null) await ShowSettingsSSLMsgAsync(sf.SettingsSSL, false);
                if (sf.Fragment != null) await ShowFragmentMsgAsync(sf.Fragment, false);
                if (sf.DnsRules != null) await ShowDnsRulesMsgAsync(sf.DnsRules, false);
                if (sf.ProxyRules != null) await ShowProxyRulesMsgAsync(sf.ProxyRules, false);
                if (sf.DnsLimit != null) await ShowDnsLimitMsgAsync(sf.DnsLimit, false);
            }
        }

        await ShowParentProcessMsgAsync();
    }

    private static async Task FlushDnsAsync()
    {
        try
        {
            foreach (ServerProfile sf in ServerProfiles)
            {
                if (sf.AgnosticServer != null && sf.AgnosticServer.IsRunning)
                {
                    sf.AgnosticServer.FlushDnsCache();
                }
            }

            if (OperatingSystem.IsWindows())
                await ProcessManager.ExecuteAsync("ipconfig", null, "/flushdns", true, true);
            await WriteToStdoutAsync("DNS Flushed", ConsoleColor.Green);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("FlushDnsAsync: " + ex.Message);
        }
    }

    private static async Task SaveCommandsToFileAsync()
    {
        try
        {
            string? p = ConsoleTools.GetCommandsPath();
            if (!string.IsNullOrEmpty(p))
            {
                List<string> commands = new();

                foreach (ServerProfile sf in ServerProfiles)
                {
                    if (sf.AgnosticServer != null && !string.IsNullOrEmpty(sf.Name))
                    {
                        string baseCmd = nameof(Profile);
                        string cmd = $"{baseCmd} {sf.Name}";
                        commands.Add(cmd);

                        if (sf.Settings != null)
                        {
                            baseCmd = Key.Setting.Name;
                            cmd = $"{baseCmd} -{Key.Setting.Port}={sf.Settings.ListenerPort}";
                            cmd += $" -{Key.Setting.WorkingMode}={sf.Settings.Working_Mode}";
                            cmd += $" -{Key.Setting.MaxRequests}={sf.Settings.MaxRequests}";
                            cmd += $" -{Key.Setting.DnsTimeoutSec}={sf.Settings.DnsTimeoutSec}";
                            cmd += $" -{Key.Setting.ProxyTimeoutSec}={sf.Settings.ProxyTimeoutSec}";
                            cmd += $" -{Key.Setting.KillOnCpuUsage}={sf.Settings.KillOnCpuUsage}";
                            cmd += $" -{Key.Setting.BlockPort80}={sf.Settings.BlockPort80}";
                            cmd += $" -{Key.Setting.AllowInsecure}={sf.Settings.AllowInsecure}";
                            if (sf.Settings.DNSs.Any())
                            {
                                cmd += $" -{Key.Setting.DNSs}=";
                                foreach (string dns in sf.Settings.DNSs)
                                    cmd += $"{dns},";
                                if (cmd.EndsWith(',')) cmd = cmd.TrimEnd(',');
                            }
                            if (!string.IsNullOrEmpty(sf.Settings.CloudflareCleanIP))
                                cmd += $" -{Key.Setting.CfCleanIP}={sf.Settings.CloudflareCleanIP}";
                            cmd += $" -{Key.Setting.BootstrapIp}={sf.Settings.BootstrapIpAddress}";
                            cmd += $" -{Key.Setting.BootstrapPort}={sf.Settings.BootstrapPort}";
                            if (!string.IsNullOrEmpty(sf.Settings.UpstreamProxyScheme))
                            {
                                cmd += $" -{Key.Setting.ProxyScheme}={sf.Settings.UpstreamProxyScheme}";
                                cmd += $" -{Key.Setting.ProxyUser}={sf.Settings.UpstreamProxyUser}";
                                cmd += $" -{Key.Setting.ProxyPass}={sf.Settings.UpstreamProxyPass}";
                                cmd += $" -{Key.Setting.OnlyBlockedIPs}={sf.Settings.ApplyUpstreamOnlyToBlockedIps}";
                            }
                            commands.Add(cmd);
                        }

                        if (sf.SettingsSSL != null)
                        {
                            baseCmd = Key.SSLSetting.Name;
                            cmd = $"{baseCmd} -{Key.SSLSetting.Enable}={sf.SettingsSSL.EnableSSL}";
                            cmd += $" -{Key.SSLSetting.RootCA_Path}=\"{sf.SettingsSSL.RootCA_Path}\"";
                            cmd += $" -{Key.SSLSetting.RootCA_KeyPath}=\"{sf.SettingsSSL.RootCA_KeyPath}\"";
                            cmd += $" -{Key.SSLSetting.Cert_Path}=\"{sf.SettingsSSL.Cert_Path}\"";
                            cmd += $" -{Key.SSLSetting.Cert_KeyPath}=\"{sf.SettingsSSL.Cert_KeyPath}\"";
                            cmd += $" -{Key.SSLSetting.ChangeSni}={sf.SettingsSSL.ChangeSni}";
                            cmd += $" -{Key.SSLSetting.DefaultSni}={sf.SettingsSSL.DefaultSni}";
                            commands.Add(cmd);
                        }

                        if (sf.Fragment != null)
                        {
                            baseCmd = $"{Key.Programs.Name} {Key.Programs.Fragment.Name}";
                            cmd = $"{baseCmd} -{Key.Programs.Fragment.Mode.Name}={sf.Fragment.FragmentMode}";
                            cmd += $" -{Key.Programs.Fragment.Mode.Program.BeforeSniChunks}={sf.Fragment.BeforeSniChunks}";
                            cmd += $" -{Key.Programs.Fragment.Mode.Program.ChunkMode.Name}={sf.Fragment.DPIChunkMode}";
                            cmd += $" -{Key.Programs.Fragment.Mode.Program.SniChunks}={sf.Fragment.SniChunks}";
                            cmd += $" -{Key.Programs.Fragment.Mode.Program.AntiPatternOffset}={sf.Fragment.AntiPatternOffset}";
                            cmd += $" -{Key.Programs.Fragment.Mode.Program.FragmentDelay}={sf.Fragment.FragmentDelay}";
                            commands.Add(cmd);
                        }

                        if (sf.DnsRules != null)
                        {
                            baseCmd = $"{Key.Programs.Name} {Key.Programs.DnsRules.Name}";
                            cmd = $"{baseCmd} -{Key.Programs.DnsRules.Mode.Name}={sf.DnsRules.RulesMode}";
                            cmd += $" -{Key.Programs.DnsRules.PathOrText}=\"{sf.DnsRules.PathOrText.Replace(Environment.NewLine, "\\n")}\"";
                            commands.Add(cmd);
                        }

                        if (sf.ProxyRules != null)
                        {
                            baseCmd = $"{Key.Programs.Name} {Key.Programs.ProxyRules.Name}";
                            cmd = $"{baseCmd} -{Key.Programs.ProxyRules.Mode.Name}={sf.ProxyRules.RulesMode}";
                            cmd += $" -{Key.Programs.ProxyRules.PathOrText}=\"{sf.ProxyRules.PathOrText.Replace(Environment.NewLine, "\\n")}\"";
                            commands.Add(cmd);
                        }

                        if (sf.DnsLimit != null)
                        {
                            baseCmd = $"{Key.Programs.Name} {Key.Programs.DnsLimit.Name}";
                            cmd = $"{baseCmd} -{Key.Programs.DnsLimit.Enable}={sf.DnsLimit.EnableDnsLimit}";
                            cmd += $" -{Key.Programs.DnsLimit.DisablePlain}={sf.DnsLimit.DisablePlainDns}";
                            cmd += $" -{Key.Programs.DnsLimit.DoHPathLimitMode.Name}={sf.DnsLimit.LimitDoHMode}";
                            cmd += $" -{Key.Programs.DnsLimit.PathOrText}=\"{sf.DnsLimit.PathOrText.Replace(Environment.NewLine, "\\n")}\"";
                            commands.Add(cmd);
                        }

                        // Add A New Line
                        commands.Add(string.Empty);
                    }
                }

                if (LoadCommands.Any()) commands.AddRange(LoadCommands);

                if (commands.Any())
                {
                    await commands.SaveToFileAsync(p);
                    await WriteToStdoutAsync("Saved To:", ConsoleColor.Green);
                    await WriteToStdoutAsync(p, ConsoleColor.Green);
                }
                else
                    await WriteToStdoutAsync("There Is Nothing To Save.", ConsoleColor.Blue);
            }
            else await WriteToStdoutAsync("Failed To Find The Path.", ConsoleColor.Red);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SaveCommandsToFileAsync: " + ex.Message);
        }
    }

    private static async Task LoadCommandsFromFileAsync()
    {
        try
        {
            string? p = ConsoleTools.GetCommandsPath();
            if (!string.IsNullOrEmpty(p))
            {
                if (File.Exists(p))
                {
                    List<string> commands = new();
                    await commands.LoadFromFileAsync(p, true, true);

                    if (commands.Any())
                    {
                        LoadCommands.Clear();
                        for (int n = 0; n < commands.Count; n++)
                        {
                            string command = commands[n];
                            if (!command.StartsWith("//") || !command.ToLower().StartsWith("load"))
                                await ExecuteCommandsAsync(command);
                        }
                    }

                    await WriteToStdoutAsync($"\nLoaded From:", ConsoleColor.Green);
                    await WriteToStdoutAsync(p, ConsoleColor.Green);
                }
                else
                    await WriteToStdoutAsync("File Not Exist.", ConsoleColor.Blue);
            }
            else await WriteToStdoutAsync("Failed To Find The Path.", ConsoleColor.Red);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("LoadCommandsFromFileAsync: " + ex.Message);
        }
    }

    private static async Task WriteToStdoutAsync(string msg, ConsoleColor consoleColor = ConsoleColor.White, bool resetColor = true)
    {
        try
        {
            Console.ForegroundColor = consoleColor;
            List<string> lines = msg.ReplaceLineEndings().Split(Environment.NewLine).ToList();
            foreach (string line in lines)
                await Console.Out.WriteLineAsync(line);
            if (resetColor) Console.ResetColor();
        }
        catch (Exception ex)
        {
            try
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
            catch (Exception ex2)
            {
                Debug.WriteLine("WriteToStdoutAsync: " + ex2.Message);
            }
        }
    }

    private static async Task WriteToStderrAsync(string msg, ConsoleColor consoleColor = ConsoleColor.White, bool resetColor = true)
    {
        try
        {
            Console.ForegroundColor = consoleColor;
            List<string> lines = msg.ReplaceLineEndings().Split(Environment.NewLine).ToList();
            foreach (string line in lines)
                await Console.Error.WriteLineAsync(line);
            if (resetColor) Console.ResetColor();
        }
        catch (Exception) { }
    }
}