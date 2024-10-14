using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Diagnostics;
using System.Net;

namespace SDCAgnosticServer;

public static partial class Program
{
    private static async Task ExecuteCommandsAsync(string? input)
    {
        try
        {
            await Task.Run(async () =>
            {
                if (string.IsNullOrEmpty(input)) return;
                if (string.IsNullOrWhiteSpace(input)) return;
                input = input.Trim();

                // Comment
                if (input.ToLower().StartsWith("//")) return;

                // Exit
                else if (input.ToLower().Equals(Key.Common.Exit.ToLower()))
                {
                    await WriteToStdoutAsync("Exiting...");
                    Environment.Exit(0);
                    await ProcessManager.KillProcessByPidAsync(Environment.ProcessId);
                }

                // Clear Screen - Stop Writing
                else if (input.ToLower().Equals(Key.Common.C.ToLower()))
                {
                    WriteRequestsToLog = false;
                    WriteFragmentDetailsToLog = false;
                    try
                    {
                        Console.Clear();
                    }
                    catch (Exception ex)
                    {
                        await WriteToStdoutAsync($"Error Console: {ex.Message}");
                    }
                }

                // Clear Screen
                else if (input.ToLower().Equals(Key.Common.CLS.ToLower()))
                {
                    try
                    {
                        Console.Clear();
                    }
                    catch (Exception ex)
                    {
                        await WriteToStdoutAsync($"Error Console: {ex.Message}");
                    }
                }

                // Get Help
                else if (input.ToLower().Equals("/?") || input.ToLower().Equals("/h") || input.ToLower().Equals("-h") ||
                         input.ToLower().Equals("help") || input.ToLower().Equals("/help") || input.ToLower().Equals("-help"))
                    await Help.GetHelpAsync();

                // Get Help Programs
                else if (input.ToLower().StartsWith("help program"))
                {
                    string prefix = "help program";
                    if (input.ToLower().Equals($"{prefix} {Key.Programs.Fragment.Name.ToLower()}")) await Help.GetHelpFragmentAsync();
                    else if (input.ToLower().Equals($"{prefix} {Key.Programs.Rules.Name.ToLower()}")) await Help.GetHelpRulesAsync();
                    else if (input.ToLower().Equals($"{prefix} {Key.Programs.DnsLimit.Name.ToLower()}")) await Help.GetHelpDnsLimitAsync();
                    else await Help.GetHelpProgramsAsync();
                }

                // Load Commands
                else if (input.ToLower().Equals(Key.Common.Load.ToLower()))
                    await LoadCommandsFromFileAsync();

                // Create Profile
                else if (input.ToLower().StartsWith(Key.Common.Profile.ToLower()))
                {
                    string[] split = input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (split.Length > 1)
                    {
                        Profile = split[1];
                        await ShowProfileMsgAsync();
                    }
                    else await WriteToStdoutAsync($"Wrong Profile Name", ConsoleColor.DarkRed);
                }

                // Check Profile Name
                else if (string.IsNullOrEmpty(Profile.Trim()))
                {
                    await WriteToStdoutAsync($"Set Profile Name First. Command: Profile <ProfileName>", ConsoleColor.Cyan);
                    return;
                }

                // Get Status Machine Read
                else if (input.ToLower().StartsWith(Key.Common.Out.ToLower()))
                {
                    string[] split = input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (split.Length > 1)
                    {
                        string profile = split[1];
                        await MainDetailsAsync(profile);
                    }
                    else await WriteToStdoutAsync($"Wrong Profile Name", ConsoleColor.DarkRed);
                }

                // Get Status Human Read
                else if (input.ToLower().Equals(Key.Common.Status.ToLower()))
                    await ShowStatusAsync();

                // Flush DNS
                else if (input.ToLower().Equals(Key.Common.Flush.ToLower()))
                    await FlushDnsAsync();

                // Parent Process
                else if (input.ToLower().StartsWith(Key.ParentProcess.Name.ToLower()))
                {
                    // ParentProcess -PID=m

                    string key = Key.ParentProcess.PID;
                    bool isValueOk = ConsoleTools.TryGetValueByKey(input, key, true, false, out string value);
                    if (!isValueOk) return;
                    isValueOk = ConsoleTools.TryGetInt(key, value, true, 0, int.MaxValue, out int pid);
                    if (!isValueOk) return;

                    ParentPID = pid;
                    await ShowParentProcessMsgAsync();
                }

                // AgnosticSettings
                else if (input.ToLower().StartsWith(Key.Setting.Name.ToLower()))
                {
                    int port = DefaultPort;
                    AgnosticSettings.WorkingMode workingMode = DefaultWorkingMode;
                    int maxRequests = DefaultMaxRequests;
                    int dnsTimeoutSec = DefaultDnsTimeoutSec;
                    int proxyTimeoutSec = DefaultProxyTimeoutSec;
                    float killOnCpuUsage = DefaultKillOnCpuUsage;
                    bool blockPort80 = DefaultBlockPort80;
                    bool allowInsecure = DefaultAllowInsecure;
                    List<string> dnss = new();
                    string cloudflareCleanIP = string.Empty;
                    string bootstrapIp = DefaultBootstrapIp.ToString();
                    int bootstrapPort = DefaultBootstrapPort;
                    string proxyScheme = string.Empty;
                    string proxyUser = string.Empty;
                    string proxyPass = string.Empty;
                    bool applyUpstreamOnlyToBlockedIps = DefaultApplyUpstreamOnlyToBlockedIps;

                    // Interactive Mode
                    if (input.ToLower().Equals(Key.Setting.Name.ToLower()))
                    {
                        // Port
                        while (true)
                        {
                            object value = await ConsoleTools.ReadValueAsync($"Enter Port Number (Default: {DefaultPort})", port, typeof(int));
                            bool isInt = int.TryParse(value.ToString(), out int n);
                            if (isInt && n >= DefaultPortMin && n <= DefaultPortMax)
                            {
                                port = n;
                                await WriteToStdoutAsync($"Port Set To {port}", ConsoleColor.Green);
                                break;
                            }
                            else
                            {
                                if (isInt)
                                    await WriteToStdoutAsync($"Port Number Must Be Between {DefaultPortMin} and {DefaultPortMax}", ConsoleColor.Red);
                                else
                                    await WriteToStdoutAsync("Error Parsing The Int Number!", ConsoleColor.Red);
                            }
                        }

                        // Working Mode
                        while (true)
                        {
                            object value = await ConsoleTools.ReadValueAsync($"Enter Working Mode (Default: {DefaultWorkingMode})", workingMode, typeof(string));
                            string workingModeStr = value.ToString() ?? string.Empty;
                            workingModeStr = workingModeStr.ToLower().Trim();

                            if (workingModeStr.Equals(nameof(AgnosticSettings.WorkingMode.Dns).ToLower()))
                                workingMode = AgnosticSettings.WorkingMode.Dns;
                            else if (workingModeStr.Equals(nameof(AgnosticSettings.WorkingMode.DnsAndProxy).ToLower()))
                                workingMode = AgnosticSettings.WorkingMode.DnsAndProxy;

                            await WriteToStdoutAsync($"Working Mode Set to {workingMode}", ConsoleColor.Green);
                            break;
                        }

                        // Max Requests
                        while (true)
                        {
                            object value = await ConsoleTools.ReadValueAsync($"Enter Maximum Requests To Handle Per Second (Default: {DefaultMaxRequests})", maxRequests, typeof(int));
                            bool isInt = int.TryParse(value.ToString(), out int n);
                            if (isInt && n >= DefaultMaxRequestsMin && n <= DefaultMaxRequestsMax)
                            {
                                maxRequests = n;
                                await WriteToStdoutAsync($"Maximum Requests Set to {maxRequests}", ConsoleColor.Green);
                                break;
                            }
                            else
                            {
                                if (isInt)
                                    await WriteToStdoutAsync($"Maximum Requests Must Be Between {DefaultMaxRequestsMin} and {DefaultMaxRequestsMax}", ConsoleColor.Red);
                                else
                                    await WriteToStdoutAsync("Error Parsing The Int Number!", ConsoleColor.Red);
                            }
                        }

                        // Dns Timeout Sec
                        while (true)
                        {
                            object value = await ConsoleTools.ReadValueAsync($"Enter Dns Timeout in Sec (Default: {DefaultDnsTimeoutSec} Sec)", dnsTimeoutSec, typeof(int));
                            bool isInt = int.TryParse(value.ToString(), out int n);
                            if (isInt && n >= DefaultDnsTimeoutSecMin && n <= DefaultDnsTimeoutSecMax)
                            {
                                dnsTimeoutSec = n;
                                await WriteToStdoutAsync($"Dns Timeout Set to {dnsTimeoutSec} Seconds", ConsoleColor.Green);
                                break;
                            }
                            else
                            {
                                if (isInt)
                                    await WriteToStdoutAsync($"Dns Timeout Must Be Between {DefaultDnsTimeoutSecMin} and {DefaultDnsTimeoutSecMax}", ConsoleColor.Red);
                                else
                                    await WriteToStdoutAsync("Error Parsing The Int Number!", ConsoleColor.Red);
                            }
                        }

                        if (workingMode == AgnosticSettings.WorkingMode.DnsAndProxy)
                        {
                            // Proxy Timeout Sec
                            while (true)
                            {
                                object value = await ConsoleTools.ReadValueAsync($"Enter Proxy Timeout in Sec (Default: {DefaultProxyTimeoutSec} Sec)", proxyTimeoutSec, typeof(int));
                                bool isInt = int.TryParse(value.ToString(), out int n);
                                if (isInt && n >= DefaultProxyTimeoutSecMin && n <= DefaultProxyTimeoutSecMax)
                                {
                                    proxyTimeoutSec = n;
                                    await WriteToStdoutAsync($"Proxy Timeout Set to {proxyTimeoutSec} Seconds", ConsoleColor.Green);
                                    break;
                                }
                                else
                                {
                                    if (isInt)
                                        await WriteToStdoutAsync($"Proxy Timeout Must Be Between {DefaultProxyTimeoutSecMin} and {DefaultProxyTimeoutSecMax}", ConsoleColor.Red);
                                    else
                                        await WriteToStdoutAsync("Error Parsing The Int Number!", ConsoleColor.Red);
                                }
                            }

                            // Kill On Cpu Usage
                            while (true)
                            {
                                object value = await ConsoleTools.ReadValueAsync($"Kill On Cpu Usage, Enter Percentage (Default: {DefaultKillOnCpuUsage}%)", killOnCpuUsage, typeof(float));
                                bool isFloat = float.TryParse(value.ToString(), out float f);
                                if (isFloat && f >= DefaultKillOnCpuUsageMin && f <= DefaultKillOnCpuUsageMax)
                                {
                                    killOnCpuUsage = f;
                                    await WriteToStdoutAsync($"Kill On Cpu Usage Set to {killOnCpuUsage}%", ConsoleColor.Green);
                                    break;
                                }
                                else
                                {
                                    if (isFloat)
                                        await WriteToStdoutAsync($"Percentage Must Be Between {DefaultKillOnCpuUsageMin} and {DefaultKillOnCpuUsageMax}", ConsoleColor.Red);
                                    else
                                        await WriteToStdoutAsync("Error Parsing The Float Number!", ConsoleColor.Red);
                                }
                            }

                            // Block Port 80
                            while (true)
                            {
                                object value = await ConsoleTools.ReadValueAsync($"Block Port 80, Enter True/False (Default: {DefaultBlockPort80})", blockPort80, typeof(bool));
                                bool b = Convert.ToBoolean(value);

                                blockPort80 = b;
                                await WriteToStdoutAsync($"Block Port 80: {blockPort80}", ConsoleColor.Green);
                                break;
                            }
                        }

                        // Allow Insecure
                        while (true)
                        {
                            object value = await ConsoleTools.ReadValueAsync($"Allow Insecure, Enter True/False (Default: {DefaultAllowInsecure})", allowInsecure, typeof(bool));
                            bool b = Convert.ToBoolean(value);

                            allowInsecure = b;
                            await WriteToStdoutAsync($"Allow Insecure: {allowInsecure}", ConsoleColor.Green);
                            break;
                        }

                        // DNSs
                        while (true)
                        {
                            string dnssStr = string.Empty;
                            object value = await ConsoleTools.ReadValueAsync("Enter DNS Addresses (Comma Separate) (Default: Google, Quad9)", dnssStr, typeof(string));
                            dnssStr = value.ToString() ?? string.Empty;
                            dnssStr = dnssStr.Trim();

                            if (!string.IsNullOrEmpty(dnssStr))
                            {
                                string[] dnssArray = dnssStr.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                                dnss.AddRange(dnssArray);
                            }

                            await WriteToStdoutAsync($"DNS Addresses Set. Count: {dnss.Count}", ConsoleColor.Green);
                            break;
                        }

                        // Cloudflare Clean IP
                        while (true)
                        {
                            string cfCleanIp = string.Empty;
                            object value = await ConsoleTools.ReadValueAsync("Enter Cloudflare Clean IPv4 Or IPv6 (Default: Empty)", cfCleanIp, typeof(string));
                            cfCleanIp = value.ToString() ?? string.Empty;
                            cfCleanIp = cfCleanIp.Trim();

                            if (!string.IsNullOrEmpty(cfCleanIp))
                            {
                                bool isIp = NetworkTool.IsIP(cfCleanIp, out _);
                                if (isIp) cloudflareCleanIP = cfCleanIp;
                            }

                            if (!string.IsNullOrEmpty(cloudflareCleanIP))
                                await WriteToStdoutAsync($"Cloudflare Clean IP Set To: {cloudflareCleanIP}", ConsoleColor.Green);
                            else
                                await WriteToStdoutAsync($"Cloudflare Clean IP Disabled", ConsoleColor.Green);
                            break;
                        }

                        // Bootstrap IP
                        while (true)
                        {
                            string getBootstrapIp = string.Empty;
                            object value = await ConsoleTools.ReadValueAsync($"Enter Bootstrap Ip (Plain DNS) (Default: None)", bootstrapIp, typeof(string));
                            getBootstrapIp = value.ToString() ?? string.Empty;
                            getBootstrapIp = getBootstrapIp.Trim();

                            if (!string.IsNullOrEmpty(getBootstrapIp))
                            {
                                bool isIp = IPAddress.TryParse(getBootstrapIp, out _);
                                if (isIp) bootstrapIp = getBootstrapIp;
                            }

                            await WriteToStdoutAsync($"Bootstrap IP Set To: {bootstrapIp}", ConsoleColor.Green);
                            break;
                        }

                        // Bootstrap Port
                        while (true)
                        {
                            object value = await ConsoleTools.ReadValueAsync($"Enter Bootstrap Port (Default: {DefaultBootstrapPort})", bootstrapPort, typeof(int));
                            bool isInt = int.TryParse(value.ToString(), out int n);
                            if (isInt && n >= 1 && n <= 65535)
                            {
                                bootstrapPort = n;
                                await WriteToStdoutAsync($"Bootstrap Port Set to {bootstrapPort}", ConsoleColor.Green);
                                break;
                            }
                            else
                            {
                                if (isInt)
                                    await WriteToStdoutAsync("Bootstrap Port Must Be Between 1 and 65535", ConsoleColor.Red);
                                else
                                    await WriteToStdoutAsync("Error Parsing The Int Number!", ConsoleColor.Red);
                            }
                        }

                        // Upstream Proxy Scheme
                        while (true)
                        {
                            object value = await ConsoleTools.ReadValueAsync($"Enter Upstream Proxy Scheme (HTTP Or SOCKS5) (Default: Empty)", proxyScheme, typeof(string));
                            proxyScheme = value.ToString() ?? string.Empty;
                            proxyScheme = proxyScheme.Trim();

                            if (!string.IsNullOrEmpty(proxyScheme))
                                await WriteToStdoutAsync($"Upstream Proxy Scheme Set To: {proxyScheme}", ConsoleColor.Green);
                            else
                                await WriteToStdoutAsync("Upstream Proxy Scheme Disabled", ConsoleColor.Green);
                            break;
                        }

                        if (!string.IsNullOrEmpty(proxyScheme))
                        {
                            // Upstream Proxy Username
                            while (true)
                            {
                                object value = await ConsoleTools.ReadValueAsync($"Enter Upstream Proxy Username (Default: Empty)", proxyUser, typeof(string));
                                proxyUser = value.ToString() ?? string.Empty;
                                proxyUser = proxyUser.Trim();

                                if (!string.IsNullOrEmpty(proxyUser))
                                    await WriteToStdoutAsync($"Upstream Proxy Username Set To: {proxyUser}", ConsoleColor.Green);
                                else
                                    await WriteToStdoutAsync("Upstream Proxy Username Set To: None", ConsoleColor.Green);
                                break;
                            }

                            // Upstream Proxy Password
                            while (true)
                            {
                                object value = await ConsoleTools.ReadValueAsync($"Enter Upstream Proxy Password (Default: Empty)", proxyPass, typeof(string));
                                proxyPass = value.ToString() ?? string.Empty;
                                proxyPass = proxyPass.Trim();

                                if (!string.IsNullOrEmpty(proxyPass))
                                    await WriteToStdoutAsync($"Upstream Proxy Password Set To: {proxyPass}", ConsoleColor.Green);
                                else
                                    await WriteToStdoutAsync("Upstream Proxy Password Set To: None", ConsoleColor.Green);
                                break;
                            }

                            // Apply Upstream Only To Blocked IPs
                            while (true)
                            {
                                object value = await ConsoleTools.ReadValueAsync($"Apply Upstream Only To Blocked IPs, Enter True/False (Default: {DefaultApplyUpstreamOnlyToBlockedIps})", applyUpstreamOnlyToBlockedIps, typeof(bool));
                                bool b = Convert.ToBoolean(value);

                                applyUpstreamOnlyToBlockedIps = b;
                                await WriteToStdoutAsync($"Apply Upstream Only To Blocked IPs: {applyUpstreamOnlyToBlockedIps}", ConsoleColor.Green);
                                break;
                            }
                        }
                    }
                    else // Command Mode
                    {
                        // setting -Port=m -WorkingMode= -MaxRequests= -DnsTimeoutSec= -ProxyTimeoutSec= -KillOnCpuUsage= -BlockPort80=
                        // -AllowInsecure= -DNSs= -CfCleanIP= -BootstrapIp= -BootstrapPort=
                        // -ProxyScheme= -ProxyUser= -ProxyPass= -OnlyBlockedIPs=

                        KeyValues keyValues = new();
                        keyValues.Add(Key.Setting.Port, true, false, typeof(int), DefaultPortMin, DefaultPortMax);
                        keyValues.Add(Key.Setting.WorkingMode, false, false, typeof(string));
                        keyValues.Add(Key.Setting.MaxRequests, false, false, typeof(int), DefaultMaxRequestsMin, DefaultMaxRequestsMax);
                        keyValues.Add(Key.Setting.DnsTimeoutSec, false, false, typeof(int), DefaultDnsTimeoutSecMin, DefaultDnsTimeoutSecMax);
                        keyValues.Add(Key.Setting.ProxyTimeoutSec, false, false, typeof(int), DefaultProxyTimeoutSecMin, DefaultProxyTimeoutSecMax);
                        keyValues.Add(Key.Setting.KillOnCpuUsage, false, false, typeof(float), DefaultKillOnCpuUsageMin, DefaultKillOnCpuUsageMax);
                        keyValues.Add(Key.Setting.BlockPort80, false, false, typeof(bool));
                        keyValues.Add(Key.Setting.AllowInsecure, false, false, typeof(bool));
                        keyValues.Add(Key.Setting.DNSs, false, false, typeof(string));
                        keyValues.Add(Key.Setting.CfCleanIP, false, false, typeof(string));
                        keyValues.Add(Key.Setting.BootstrapIp, false, false, typeof(string));
                        keyValues.Add(Key.Setting.BootstrapPort, false, false, typeof(int), 1, 65535);
                        keyValues.Add(Key.Setting.ProxyScheme, false, false, typeof(string));
                        keyValues.Add(Key.Setting.ProxyUser, false, false, typeof(string));
                        keyValues.Add(Key.Setting.ProxyPass, false, false, typeof(string));
                        keyValues.Add(Key.Setting.OnlyBlockedIPs, false, false, typeof(bool));

                        bool isListOk = keyValues.TryGetValuesByKeys(input, out List<KeyValue> list);
                        if (!isListOk) return;
                        for (int n = 0; n < list.Count; n++)
                        {
                            KeyValue kv = list[n];
                            if (kv.Key.Equals(Key.Setting.Port)) port = kv.ValueInt;
                            if (kv.Key.Equals(Key.Setting.WorkingMode))
                            {
                                if (kv.ValueString.Equals(nameof(AgnosticSettings.WorkingMode.Dns)))
                                    workingMode = AgnosticSettings.WorkingMode.Dns;
                                else if (kv.ValueString.Equals(nameof(AgnosticSettings.WorkingMode.DnsAndProxy)))
                                    workingMode = AgnosticSettings.WorkingMode.DnsAndProxy;
                            }
                            if (kv.Key.Equals(Key.Setting.MaxRequests)) maxRequests = kv.ValueInt;
                            if (kv.Key.Equals(Key.Setting.DnsTimeoutSec)) dnsTimeoutSec = kv.ValueInt;
                            if (kv.Key.Equals(Key.Setting.ProxyTimeoutSec)) proxyTimeoutSec = kv.ValueInt;
                            if (kv.Key.Equals(Key.Setting.KillOnCpuUsage)) killOnCpuUsage = kv.ValueFloat;
                            if (kv.Key.Equals(Key.Setting.BlockPort80)) blockPort80 = kv.ValueBool;
                            if (kv.Key.Equals(Key.Setting.AllowInsecure)) allowInsecure = kv.ValueBool;
                            if (kv.Key.Equals(Key.Setting.DNSs))
                            {
                                if (!string.IsNullOrEmpty(kv.ValueString))
                                {
                                    string[] dnssArray = kv.ValueString.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                                    dnss.AddRange(dnssArray);
                                }
                            }
                            if (kv.Key.Equals(Key.Setting.CfCleanIP)) cloudflareCleanIP = kv.ValueString;
                            if (kv.Key.Equals(Key.Setting.BootstrapIp)) bootstrapIp = kv.ValueString;
                            if (kv.Key.Equals(Key.Setting.BootstrapPort)) bootstrapPort = kv.ValueInt;
                            if (kv.Key.Equals(Key.Setting.ProxyScheme)) proxyScheme = kv.ValueString;
                            if (kv.Key.Equals(Key.Setting.ProxyUser)) proxyUser = kv.ValueString;
                            if (kv.Key.Equals(Key.Setting.ProxyPass)) proxyPass = kv.ValueString;
                            if (kv.Key.Equals(Key.Setting.OnlyBlockedIPs)) applyUpstreamOnlyToBlockedIps = kv.ValueBool;
                        }
                    }

                    AgnosticSettings settings = new()
                    {
                        ListenerPort = port,
                        Working_Mode = workingMode,
                        MaxRequests = maxRequests,
                        DnsTimeoutSec = dnsTimeoutSec,
                        ProxyTimeoutSec = proxyTimeoutSec,
                        KillOnCpuUsage = killOnCpuUsage,
                        BlockPort80 = blockPort80,
                        AllowInsecure = allowInsecure,
                        DNSs = dnss,
                        CloudflareCleanIP = cloudflareCleanIP
                    };
                    bool isbootstrapIp = IPAddress.TryParse(bootstrapIp, out IPAddress? bootstrapIpOut);
                    if (isbootstrapIp && bootstrapIpOut != null)
                        settings.BootstrapIpAddress = bootstrapIpOut;
                    settings.BootstrapPort = bootstrapPort;
                    if (!string.IsNullOrEmpty(proxyScheme))
                    {
                        settings.UpstreamProxyScheme = proxyScheme;
                        settings.UpstreamProxyUser = proxyUser;
                        settings.UpstreamProxyPass = proxyPass;
                        settings.ApplyUpstreamOnlyToBlockedIps = applyUpstreamOnlyToBlockedIps;
                    }

                    await ShowSettingsMsgAsync(settings);
                }

                // SSLSetting
                else if (input.ToLower().StartsWith(Key.SSLSetting.Name.ToLower()))
                {
                    bool enable = DefaultSSLEnable;
                    string? rootCA_Path = null;
                    string? rootCA_KeyPath = null;
                    string? cert_Path = null;
                    string? cert_KeyPath = null;
                    bool changeSni = false;
                    string defaultSni = string.Empty;

                    // Interactive Mode
                    if (input.ToLower().Equals(Key.SSLSetting.Name.ToLower()))
                    {
                        // Enable
                        string msgRV = $"Enable SSL Decryption, Enter True/False (Default: {DefaultSSLEnable})";
                        while (true)
                        {
                            object value = await ConsoleTools.ReadValueAsync(msgRV, enable, typeof(bool));
                            enable = Convert.ToBoolean(value);

                            await WriteToStdoutAsync($"Enable Set to {enable}", ConsoleColor.Green);
                            break;
                        }

                        if (enable)
                        {
                            // RootCA_Path
                            bool doesUserSetCAPath = false;
                            while (true)
                            {
                                msgRV = $"Enter The Path Of Root Certificate (Leave Empty To Generate)";
                                object value = await ConsoleTools.ReadValueAsync(msgRV, string.Empty, typeof(string));
                                string path = value.ToString() ?? string.Empty;
                                if (string.IsNullOrEmpty(path))
                                {
                                    rootCA_Path = null;
                                    break;
                                }
                                else
                                {
                                    path = Path.GetFullPath(path);
                                    if (!File.Exists(path))
                                    {
                                        string msgNotExist = $"{path}\nFile Not Exist.";
                                        await WriteToStdoutAsync(msgNotExist, ConsoleColor.Red);
                                        continue;
                                    }
                                    else
                                    {
                                        rootCA_Path = path;
                                        doesUserSetCAPath = true;
                                        break;
                                    }
                                }
                            }

                            // RootCA_KeyPath
                            if (doesUserSetCAPath)
                            {
                                while (true)
                                {
                                    msgRV = $"Enter The Path Of Root Private Key (Leave Empty If Certificate Contains Private Key)";
                                    object value = await ConsoleTools.ReadValueAsync(msgRV, string.Empty, typeof(string));
                                    string path = value.ToString() ?? string.Empty;
                                    if (string.IsNullOrEmpty(path))
                                    {
                                        rootCA_KeyPath = null;
                                        break;
                                    }
                                    else
                                    {
                                        path = Path.GetFullPath(path);
                                        if (!File.Exists(path))
                                        {
                                            string msgNotExist = $"{path}\nFile Not Exist.";
                                            await WriteToStdoutAsync(msgNotExist, ConsoleColor.Red);
                                            continue;
                                        }
                                        else
                                        {
                                            rootCA_KeyPath = path;
                                            break;
                                        }
                                    }
                                }
                            }

                            // Cert_Path
                            bool doesUserSetCertPath = false;
                            while (true)
                            {
                                msgRV = $"Enter The Path Of Certificate (Leave Empty To Generate)";
                                object value = await ConsoleTools.ReadValueAsync(msgRV, string.Empty, typeof(string));
                                string path = value.ToString() ?? string.Empty;
                                if (string.IsNullOrEmpty(path))
                                {
                                    cert_Path = null;
                                    break;
                                }
                                else
                                {
                                    path = Path.GetFullPath(path);
                                    if (!File.Exists(path))
                                    {
                                        string msgNotExist = $"{path}\nFile Not Exist.";
                                        await WriteToStdoutAsync(msgNotExist, ConsoleColor.Red);
                                        continue;
                                    }
                                    else
                                    {
                                        cert_Path = path;
                                        doesUserSetCertPath = true;
                                        break;
                                    }
                                }
                            }

                            // Cert_KeyPath
                            if (doesUserSetCertPath)
                            {
                                while (true)
                                {
                                    msgRV = $"Enter The Path Of Cert Private Key (Leave Empty If Certificate Contains Private Key)";
                                    object value = await ConsoleTools.ReadValueAsync(msgRV, string.Empty, typeof(string));
                                    string path = value.ToString() ?? string.Empty;
                                    if (string.IsNullOrEmpty(path))
                                    {
                                        cert_KeyPath = null;
                                        break;
                                    }
                                    else
                                    {
                                        path = Path.GetFullPath(path);
                                        if (!File.Exists(path))
                                        {
                                            string msgNotExist = $"{path}\nFile Not Exist.";
                                            await WriteToStdoutAsync(msgNotExist, ConsoleColor.Red);
                                            continue;
                                        }
                                        else
                                        {
                                            cert_KeyPath = path;
                                            break;
                                        }
                                    }
                                }
                            }

                            // Change SNI To IP
                            msgRV = $"Change SNI To Bypass DPI, Enter True/False (Default: {DefaultSSLChangeSni})";
                            while (true)
                            {
                                object value = await ConsoleTools.ReadValueAsync(msgRV, changeSni, typeof(bool));
                                changeSni = Convert.ToBoolean(value);

                                await WriteToStdoutAsync($"ChangeSni Set to {changeSni}", ConsoleColor.Green);
                                break;
                            }

                            // Default SNI
                            if (changeSni)
                            {
                                msgRV = $"Set Default SNI, E.G. speedtest.net (Default: Empty)";
                                while (true)
                                {
                                    object value = await ConsoleTools.ReadValueAsync(msgRV, string.Empty, typeof(string));
                                    string defaultSniOut = value.ToString() ?? string.Empty;
                                    if (!string.IsNullOrEmpty(defaultSniOut))
                                    {
                                        defaultSni = defaultSniOut;
                                        await WriteToStdoutAsync($"DefaultSni Set to {defaultSni}", ConsoleColor.Green);
                                        break;
                                    }

                                    await WriteToStdoutAsync($"DefaultSni Set to Empty", ConsoleColor.Green);
                                    break;
                                }
                            }
                        }
                    }
                    else // Command Mode
                    {
                        // sslsetting -Enable=m -RootCA_Path="" -RootCA_KeyPath="" -Cert_Path="" -Cert_KeyPath="" -ChangeSni= -DefaultSni=

                        KeyValues keyValues = new();
                        keyValues.Add(Key.SSLSetting.Enable, true, false, typeof(bool));
                        keyValues.Add(Key.SSLSetting.RootCA_Path, false, true, typeof(string));
                        keyValues.Add(Key.SSLSetting.RootCA_KeyPath, false, true, typeof(string));
                        keyValues.Add(Key.SSLSetting.Cert_Path, false, true, typeof(string));
                        keyValues.Add(Key.SSLSetting.Cert_KeyPath, false, true, typeof(string));
                        keyValues.Add(Key.SSLSetting.ChangeSni, false, false, typeof(bool));
                        keyValues.Add(Key.SSLSetting.DefaultSni, false, false, typeof(string));

                        bool isListOk = keyValues.TryGetValuesByKeys(input, out List<KeyValue> list);
                        if (!isListOk) return;
                        for (int n = 0; n < list.Count; n++)
                        {
                            KeyValue kv = list[n];
                            if (kv.Key.Equals(Key.SSLSetting.Enable)) enable = kv.ValueBool;
                            if (kv.Key.Equals(Key.SSLSetting.RootCA_Path)) rootCA_Path = kv.ValueString;
                            if (kv.Key.Equals(Key.SSLSetting.RootCA_KeyPath)) rootCA_KeyPath = kv.ValueString;
                            if (kv.Key.Equals(Key.SSLSetting.Cert_Path)) cert_Path = kv.ValueString;
                            if (kv.Key.Equals(Key.SSLSetting.Cert_KeyPath)) cert_KeyPath = kv.ValueString;
                            if (kv.Key.Equals(Key.SSLSetting.ChangeSni)) changeSni = kv.ValueBool;
                            if (kv.Key.Equals(Key.SSLSetting.DefaultSni)) defaultSni = kv.ValueString;
                        }
                    }

                    AgnosticSettingsSSL settingsSSL = new(enable)
                    {
                        EnableSSL = enable,
                        RootCA_Path = rootCA_Path,
                        RootCA_KeyPath = rootCA_KeyPath,
                        Cert_Path = cert_Path,
                        Cert_KeyPath = cert_KeyPath,
                        ChangeSni = changeSni,
                        DefaultSni = defaultSni
                    };

                    await ShowSettingsSSLMsgAsync(settingsSSL);
                }

                // Programs
                else if (input.ToLower().StartsWith(Key.Programs.Name.ToLower()))
                {
                    string msg = "Available Programs:\n\n";
                    msg += $"{Key.Programs.Fragment.Name}\n";
                    msg += $"{Key.Programs.Rules.Name}\n";
                    msg += $"{Key.Programs.DnsLimit.Name}\n";

                    // Interactive Mode
                    if (input.ToLower().Equals(Key.Programs.Name.ToLower()))
                    {
                        await WriteToStdoutAsync(msg, ConsoleColor.Cyan);

                        while (true)
                        {
                            string programName = string.Empty;
                            object valueP = await ConsoleTools.ReadValueAsync("Enter One Of Programs Name (Default: None):", programName, typeof(string));
                            programName = valueP.ToString() ?? string.Empty;
                            if (string.IsNullOrEmpty(programName) || string.IsNullOrWhiteSpace(programName))
                            {
                                await WriteToStdoutAsync($"Exited From {Key.Programs.Name}.");
                                break;
                            }

                            // DPI Bypass
                            else if (programName.ToLower().Equals(Key.Programs.Fragment.Name.ToLower()))
                            {
                                string msgAm = $"Available {Key.Programs.Fragment.Name} Modes:\n\n";
                                msgAm += $"{Key.Programs.Fragment.Mode.Program.Name}\n";
                                msgAm += $"{Key.Programs.Fragment.Mode.Disable}\n";

                                await WriteToStdoutAsync(msgAm, ConsoleColor.Cyan);

                                string modeStr = Key.Programs.Fragment.Mode.Disable;
                                AgnosticProgram.Fragment.Mode mode = AgnosticProgram.Fragment.Mode.Disable;
                                string msgRV = $"Enter One Of Modes (Default: {mode}):";
                                while (true)
                                {
                                    object value = await ConsoleTools.ReadValueAsync(msgRV, modeStr, typeof(string));
                                    modeStr = value.ToString() ?? string.Empty;
                                    if (modeStr.ToLower().Equals(Key.Programs.Fragment.Mode.Program.Name.ToLower()))
                                        mode = AgnosticProgram.Fragment.Mode.Program;
                                    else if (modeStr.ToLower().Equals(Key.Programs.Fragment.Mode.Disable.ToLower()))
                                        mode = AgnosticProgram.Fragment.Mode.Disable;
                                    else
                                    {
                                        await WriteToStdoutAsync("Wrong Mode.", ConsoleColor.Red);
                                        continue;
                                    }
                                    break;
                                }

                                int beforeSniChunks = DefaultFragmentBeforeSniChunks;
                                AgnosticProgram.Fragment.ChunkMode chunkMode = DefaultFragmentChunkMode;
                                int sniChunks = DefaultFragmentSniChunks;
                                int antiPatternOffset = DefaultFragmentAntiPatternOffset;
                                int fragmentDelay = DefaultFragmentFragmentDelay;

                                if (mode == AgnosticProgram.Fragment.Mode.Program)
                                {
                                    // Get Before Sni Chunks
                                    msgRV = $"Enter Number Of Chunks Before SNI (Default: {DefaultFragmentBeforeSniChunks}):";
                                    while (true)
                                    {
                                        object value = await ConsoleTools.ReadValueAsync(msgRV, beforeSniChunks, typeof(int));
                                        int n = Convert.ToInt32(value);
                                        if (n >= DefaultFragmentBeforeSniChunksMin && n <= DefaultFragmentBeforeSniChunksMax)
                                        {
                                            beforeSniChunks = n;
                                            break;
                                        }
                                        else
                                        {
                                            await WriteToStdoutAsync($"Chunks Number Must Be Between {DefaultFragmentBeforeSniChunksMin} and {DefaultFragmentBeforeSniChunksMax}", ConsoleColor.Red);
                                            continue;
                                        }
                                    }

                                    // Get Chunk Mode
                                    msgAm = $"Available {Key.Programs.Fragment.Mode.Program.ChunkMode.Name} Modes:\n\n";
                                    msgAm += $"{Key.Programs.Fragment.Mode.Program.ChunkMode.SNI}\n";
                                    msgAm += $"{Key.Programs.Fragment.Mode.Program.ChunkMode.SniExtension}\n";
                                    msgAm += $"{Key.Programs.Fragment.Mode.Program.ChunkMode.AllExtensions}\n";

                                    await WriteToStdoutAsync(msgAm, ConsoleColor.Cyan);

                                    modeStr = Key.Programs.Fragment.Mode.Program.ChunkMode.SNI;
                                    msgRV = $"Enter One Of Modes (Default: {chunkMode}):";
                                    while (true)
                                    {
                                        object value = await ConsoleTools.ReadValueAsync(msgRV, modeStr, typeof(string));
                                        modeStr = value.ToString() ?? string.Empty;
                                        if (modeStr.ToLower().Equals(Key.Programs.Fragment.Mode.Program.ChunkMode.SNI.ToLower()))
                                            chunkMode = AgnosticProgram.Fragment.ChunkMode.SNI;
                                        else if (modeStr.ToLower().Equals(Key.Programs.Fragment.Mode.Program.ChunkMode.SniExtension.ToLower()))
                                            chunkMode = AgnosticProgram.Fragment.ChunkMode.SniExtension;
                                        else if (modeStr.ToLower().Equals(Key.Programs.Fragment.Mode.Program.ChunkMode.AllExtensions.ToLower()))
                                            chunkMode = AgnosticProgram.Fragment.ChunkMode.AllExtensions;
                                        else
                                        {
                                            await WriteToStdoutAsync("Wrong Mode.", ConsoleColor.Red);
                                            continue;
                                        }
                                        break;
                                    }

                                    // Get Sni Chunks
                                    msgRV = $"Enter Number Of \"{chunkMode}\" Chunks (Default: {DefaultFragmentSniChunks}):";
                                    while (true)
                                    {
                                        object value = await ConsoleTools.ReadValueAsync(msgRV, sniChunks, typeof(int));
                                        int n = Convert.ToInt32(value);
                                        if (n >= DefaultFragmentSniChunksMin && n <= DefaultFragmentSniChunksMax)
                                        {
                                            sniChunks = n;
                                            break;
                                        }
                                        else
                                        {
                                            await WriteToStdoutAsync($"Chunks Number Must Be Between {DefaultFragmentSniChunksMin} and {DefaultFragmentSniChunksMax}", ConsoleColor.Red);
                                            continue;
                                        }
                                    }

                                    // Get Anti-Pattern Offset
                                    msgRV = $"Enter Number Of Anti-Pattern Offset (Default: {DefaultFragmentAntiPatternOffset}):";
                                    while (true)
                                    {
                                        object value = await ConsoleTools.ReadValueAsync(msgRV, antiPatternOffset, typeof(int));
                                        int n = Convert.ToInt32(value);
                                        if (n >= DefaultFragmentAntiPatternOffsetMin && n <= DefaultFragmentAntiPatternOffsetMax)
                                        {
                                            antiPatternOffset = n;
                                            break;
                                        }
                                        else
                                        {
                                            await WriteToStdoutAsync($"Chunks Number Must Be Between {DefaultFragmentAntiPatternOffsetMin} and {DefaultFragmentAntiPatternOffsetMin}", ConsoleColor.Red);
                                            continue;
                                        }
                                    }

                                    // Get Fragment Delay
                                    msgRV = $"Enter Milliseconds Of Fragment Delay (Default: {DefaultFragmentFragmentDelay}):";
                                    while (true)
                                    {
                                        object value = await ConsoleTools.ReadValueAsync(msgRV, fragmentDelay, typeof(int));
                                        int n = Convert.ToInt32(value);
                                        if (n >= DefaultFragmentFragmentDelayMin && n <= DefaultFragmentFragmentDelayMax)
                                        {
                                            fragmentDelay = n;
                                            break;
                                        }
                                        else
                                        {
                                            await WriteToStdoutAsync($"Fragment Delay Must Be Between {DefaultFragmentFragmentDelayMin} and {DefaultFragmentFragmentDelayMax} Milliseconds", ConsoleColor.Red);
                                            continue;
                                        }
                                    }
                                }

                                AgnosticProgram.Fragment FragmentStaticProgram = new();
                                FragmentStaticProgram.Set(mode, beforeSniChunks, chunkMode, sniChunks, antiPatternOffset, fragmentDelay);

                                await ShowFragmentMsgAsync(FragmentStaticProgram);
                            }

                            // Rules
                            else if (programName.ToLower().Equals(Key.Programs.Rules.Name.ToLower()))
                            {
                                string msgAm = $"Available {Key.Programs.Rules.Name} Modes:\n\n";
                                msgAm += $"{Key.Programs.Rules.Mode.File}\n";
                                msgAm += $"{Key.Programs.Rules.Mode.Text}\n";
                                msgAm += $"{Key.Programs.Rules.Mode.Disable}\n";

                                await WriteToStdoutAsync(msgAm, ConsoleColor.Cyan);

                                string modeStr = Key.Programs.Rules.Mode.Disable;
                                AgnosticProgram.Rules.Mode mode = AgnosticProgram.Rules.Mode.Disable;
                                string msgRV = $"Enter One Of Modes (Default: {mode}):";
                                while (true)
                                {
                                    object value = await ConsoleTools.ReadValueAsync(msgRV, modeStr, typeof(string));
                                    modeStr = value.ToString() ?? string.Empty;
                                    if (modeStr.ToLower().Equals(Key.Programs.Rules.Mode.File.ToLower()))
                                        mode = AgnosticProgram.Rules.Mode.File;
                                    else if (modeStr.ToLower().Equals(Key.Programs.Rules.Mode.Text.ToLower()))
                                        mode = AgnosticProgram.Rules.Mode.Text;
                                    else if (modeStr.ToLower().Equals(Key.Programs.Rules.Mode.Disable.ToLower()))
                                        mode = AgnosticProgram.Rules.Mode.Disable;
                                    else
                                    {
                                        await WriteToStdoutAsync("Wrong Mode.", ConsoleColor.Red);
                                        continue;
                                    }
                                    break;
                                }

                                string filePathOrText = string.Empty;

                                if (mode == AgnosticProgram.Rules.Mode.File)
                                {
                                    while (true)
                                    {
                                        msgRV = $"Enter The Path Of {mode} (Default: Cancel):";
                                        object valuePath = await ConsoleTools.ReadValueAsync(msgRV, string.Empty, typeof(string));
                                        filePathOrText = valuePath.ToString() ?? string.Empty;
                                        if (string.IsNullOrEmpty(filePathOrText))
                                        {
                                            mode = AgnosticProgram.Rules.Mode.Disable;
                                            break;
                                        }
                                        else
                                        {
                                            filePathOrText = Path.GetFullPath(filePathOrText);
                                            if (!File.Exists(filePathOrText))
                                            {
                                                string msgNotExist = $"{filePathOrText}\nFile Not Exist.";
                                                await WriteToStdoutAsync(msgNotExist, ConsoleColor.Red);
                                                continue;
                                            }
                                            else break;
                                        }
                                    }
                                }

                                if (mode == AgnosticProgram.Rules.Mode.Text)
                                {
                                    msgRV = $"Enter {Key.Programs.Rules.Name} As Text (Default: Cancel):";
                                    msgRV += "\n    e.g. Google.com|8.8.8.8;\\nCloudflare.com|dns:tcp://8.8.8.8;";
                                    object valueText = await ConsoleTools.ReadValueAsync(msgRV, string.Empty, typeof(string));
                                    filePathOrText = valueText.ToString() ?? string.Empty;
                                    if (string.IsNullOrEmpty(filePathOrText))
                                    {
                                        mode = AgnosticProgram.Rules.Mode.Disable;
                                    }
                                    else
                                    {
                                        filePathOrText = filePathOrText.ToLower().Replace(@"\n", Environment.NewLine);
                                    }
                                }

                                AgnosticProgram.Rules RulesProgram = new();
                                await RulesProgram.SetAsync(mode, filePathOrText);

                                await ShowRulesMsgAsync(RulesProgram);
                            }

                            // Dns Limit
                            else if (programName.ToLower().Equals(Key.Programs.DnsLimit.Name.ToLower()))
                            {
                                bool enable = false;
                                bool disablePlain = false;
                                AgnosticProgram.DnsLimit.LimitDoHPathsMode mode = AgnosticProgram.DnsLimit.LimitDoHPathsMode.Disable;
                                string filePathOrText = string.Empty;

                                string msgRV = $"Enable {Key.Programs.DnsLimit.Name} Program (Default: False):";
                                while (true)
                                {
                                    object value = await ConsoleTools.ReadValueAsync(msgRV, enable, typeof(bool));
                                    enable = Convert.ToBoolean(value);

                                    await WriteToStdoutAsync($"Enable {Key.Programs.DnsLimit.Name} Set to {enable}", ConsoleColor.Green);
                                    break;
                                }

                                if (enable)
                                {
                                    msgRV = $"Disable Plain DNS (Default: False):";
                                    while (true)
                                    {
                                        object value = await ConsoleTools.ReadValueAsync(msgRV, disablePlain, typeof(bool));
                                        disablePlain = Convert.ToBoolean(value);

                                        await WriteToStdoutAsync($"Disable Plain DNS Set to {disablePlain}", ConsoleColor.Green);
                                        break;
                                    }

                                    string msgAm = $"Available {Key.Programs.DnsLimit.DoHPathLimitMode.Name} Modes:\n\n";
                                    msgAm += $"{Key.Programs.DnsLimit.DoHPathLimitMode.File}\n";
                                    msgAm += $"{Key.Programs.DnsLimit.DoHPathLimitMode.Text}\n";
                                    msgAm += $"{Key.Programs.DnsLimit.DoHPathLimitMode.Disable}\n";

                                    await WriteToStdoutAsync(msgAm, ConsoleColor.Cyan);

                                    string modeStr = Key.Programs.DnsLimit.DoHPathLimitMode.Disable;
                                    msgRV = $"Enter One Of Modes (Default: {mode}):";
                                    while (true)
                                    {
                                        object value = await ConsoleTools.ReadValueAsync(msgRV, modeStr, typeof(string));
                                        modeStr = value.ToString() ?? string.Empty;
                                        if (modeStr.ToLower().Equals(Key.Programs.DnsLimit.DoHPathLimitMode.File.ToLower()))
                                            mode = AgnosticProgram.DnsLimit.LimitDoHPathsMode.File;
                                        else if (modeStr.ToLower().Equals(Key.Programs.DnsLimit.DoHPathLimitMode.Text.ToLower()))
                                            mode = AgnosticProgram.DnsLimit.LimitDoHPathsMode.Text;
                                        else if (modeStr.ToLower().Equals(Key.Programs.DnsLimit.DoHPathLimitMode.Disable.ToLower()))
                                            mode = AgnosticProgram.DnsLimit.LimitDoHPathsMode.Disable;
                                        else
                                        {
                                            await WriteToStdoutAsync("Wrong Mode.", ConsoleColor.Red);
                                            continue;
                                        }
                                        break;
                                    }

                                    if (mode == AgnosticProgram.DnsLimit.LimitDoHPathsMode.File)
                                    {
                                        while (true)
                                        {
                                            msgRV = $"Enter The Path Of {mode} (Default: Cancel):";
                                            object valuePath = await ConsoleTools.ReadValueAsync(msgRV, string.Empty, typeof(string));
                                            filePathOrText = valuePath.ToString() ?? string.Empty;
                                            if (string.IsNullOrEmpty(filePathOrText))
                                            {
                                                mode = AgnosticProgram.DnsLimit.LimitDoHPathsMode.Disable;
                                                break;
                                            }
                                            else
                                            {
                                                filePathOrText = Path.GetFullPath(filePathOrText);
                                                if (!File.Exists(filePathOrText))
                                                {
                                                    string msgNotExist = $"{filePathOrText}\nFile Not Exist.";
                                                    await WriteToStdoutAsync(msgNotExist, ConsoleColor.Red);
                                                    continue;
                                                }
                                                else break;
                                            }
                                        }
                                    }

                                    if (mode == AgnosticProgram.DnsLimit.LimitDoHPathsMode.Text)
                                    {
                                        msgRV = $"Enter {Key.Programs.DnsLimit.DoHPathLimitMode.Name} As Text (Default: Cancel):";
                                        msgRV += "\n    e.g. dns-query\\nUserName1";
                                        object valueText = await ConsoleTools.ReadValueAsync(msgRV, string.Empty, typeof(string));
                                        filePathOrText = valueText.ToString() ?? string.Empty;
                                        if (string.IsNullOrEmpty(filePathOrText))
                                        {
                                            mode = AgnosticProgram.DnsLimit.LimitDoHPathsMode.Disable;
                                        }
                                        else
                                        {
                                            filePathOrText = filePathOrText.Replace(@"\n", Environment.NewLine);
                                        }
                                    }
                                }

                                AgnosticProgram.DnsLimit dnsLimitProgram = new();
                                dnsLimitProgram.Set(enable, disablePlain, mode, filePathOrText);

                                await ShowDnsLimitMsgAsync(dnsLimitProgram);
                            }

                            else
                            {
                                await WriteToStdoutAsync("Wrong Program Name.", ConsoleColor.Red);
                                continue;
                            }
                        }
                    }
                    else // Command Mode
                    {
                        bool isValueOk = false;

                        // Fragment
                        if (input.ToLower().StartsWith($"{Key.Programs.Name.ToLower()} {Key.Programs.Fragment.Name.ToLower()}"))
                        {
                            // Programs Fragment -Mode=m -BeforeSniChunks=m -ChunkMode=m -SniChunks=m -AntiPatternOffset=m -FragmentDelay=m

                            // Get ModeStr
                            string modeStr = Key.Programs.Fragment.Mode.Disable;
                            string key = Key.Programs.Fragment.Mode.Name;
                            isValueOk = ConsoleTools.TryGetValueByKey(input, key, true, false, out string value);
                            if (!isValueOk) return;

                            KeyValues modes = new();
                            modes.Add(Key.Programs.Fragment.Mode.Program.Name, true, false, typeof(string));
                            modes.Add(Key.Programs.Fragment.Mode.Disable, true, false, typeof(string));

                            isValueOk = ConsoleTools.TryGetString(key, value, true, modes, out value);
                            if (!isValueOk) return;
                            modeStr = value;

                            // Get -Mode
                            AgnosticProgram.Fragment.Mode mode = AgnosticProgram.Fragment.Mode.Disable;
                            if (modeStr.ToLower().Equals(Key.Programs.Fragment.Mode.Program.Name.ToLower()))
                                mode = AgnosticProgram.Fragment.Mode.Program;
                            else if (modeStr.ToLower().Equals(Key.Programs.Fragment.Mode.Disable.ToLower()))
                                mode = AgnosticProgram.Fragment.Mode.Disable;

                            int beforeSniChunks = DefaultFragmentBeforeSniChunks;
                            string chunkModeStr = DefaultFragmentChunkModeStr;
                            int sniChunks = DefaultFragmentSniChunks;
                            int antiPatternOffset = DefaultFragmentAntiPatternOffset;
                            int fragmentDelay = DefaultFragmentFragmentDelay;

                            // Get The Rest
                            if (mode == AgnosticProgram.Fragment.Mode.Program)
                            {
                                KeyValues keyValues = new();
                                keyValues.Add(Key.Programs.Fragment.Mode.Program.BeforeSniChunks, true, false, typeof(int), DefaultFragmentBeforeSniChunksMin, DefaultFragmentBeforeSniChunksMax);
                                keyValues.Add(Key.Programs.Fragment.Mode.Program.ChunkMode.Name, true, false, typeof(string));
                                keyValues.Add(Key.Programs.Fragment.Mode.Program.SniChunks, true, false, typeof(int), DefaultFragmentSniChunksMin, DefaultFragmentSniChunksMax);
                                keyValues.Add(Key.Programs.Fragment.Mode.Program.AntiPatternOffset, true, false, typeof(int), DefaultFragmentAntiPatternOffsetMin, DefaultFragmentAntiPatternOffsetMax);
                                keyValues.Add(Key.Programs.Fragment.Mode.Program.FragmentDelay, true, false, typeof(int), DefaultFragmentFragmentDelayMin, DefaultFragmentFragmentDelayMax);

                                bool isListOk = keyValues.TryGetValuesByKeys(input, out List<KeyValue> list);

                                bool isChunkModeOk = false;

                                for (int n = 0; n < list.Count; n++)
                                {
                                    KeyValue kv = list[n];
                                    if (kv.Key.Equals(Key.Programs.Fragment.Mode.Program.BeforeSniChunks)) beforeSniChunks = kv.ValueInt;
                                    if (kv.Key.Equals(Key.Programs.Fragment.Mode.Program.ChunkMode.Name))
                                    {
                                        chunkModeStr = kv.ValueString;

                                        KeyValues chunkModes = new();
                                        chunkModes.Add(Key.Programs.Fragment.Mode.Program.ChunkMode.SNI, true, false, typeof(string));
                                        chunkModes.Add(Key.Programs.Fragment.Mode.Program.ChunkMode.SniExtension, true, false, typeof(string));
                                        chunkModes.Add(Key.Programs.Fragment.Mode.Program.ChunkMode.AllExtensions, true, false, typeof(string));

                                        string chunkKey = Key.Programs.Fragment.Mode.Program.ChunkMode.Name;
                                        isChunkModeOk = ConsoleTools.TryGetString(chunkKey, chunkModeStr, true, chunkModes, out chunkModeStr);
                                        if (!isChunkModeOk) break;
                                    }
                                    if (kv.Key.Equals(Key.Programs.Fragment.Mode.Program.SniChunks)) sniChunks = kv.ValueInt;
                                    if (kv.Key.Equals(Key.Programs.Fragment.Mode.Program.AntiPatternOffset)) antiPatternOffset = kv.ValueInt;
                                    if (kv.Key.Equals(Key.Programs.Fragment.Mode.Program.FragmentDelay)) fragmentDelay = kv.ValueInt;
                                }

                                if (!isChunkModeOk) return;
                                if (!isListOk) return;
                            }

                            // Get Chunk Mode
                            AgnosticProgram.Fragment.ChunkMode chunkMode = AgnosticProgram.Fragment.ChunkMode.SNI;
                            if (chunkModeStr.ToLower().Equals(Key.Programs.Fragment.Mode.Program.ChunkMode.SNI.ToLower()))
                                chunkMode = AgnosticProgram.Fragment.ChunkMode.SNI;
                            else if (chunkModeStr.ToLower().Equals(Key.Programs.Fragment.Mode.Program.ChunkMode.SniExtension.ToLower()))
                                chunkMode = AgnosticProgram.Fragment.ChunkMode.SniExtension;
                            else if (chunkModeStr.ToLower().Equals(Key.Programs.Fragment.Mode.Program.ChunkMode.AllExtensions.ToLower()))
                                chunkMode = AgnosticProgram.Fragment.ChunkMode.AllExtensions;

                            AgnosticProgram.Fragment fragmentStaticProgram = new();
                            fragmentStaticProgram.Set(mode, beforeSniChunks, chunkMode, sniChunks, antiPatternOffset, fragmentDelay);

                            await ShowFragmentMsgAsync(fragmentStaticProgram);
                        }

                        // Rules
                        if (input.ToLower().StartsWith($"{Key.Programs.Name.ToLower()} {Key.Programs.Rules.Name.ToLower()}"))
                        {
                            // Programs Rules -Mode=m -PathOrText="m"

                            // Get ModeStr
                            string modeStr = Key.Programs.Rules.Mode.Disable;
                            string key = Key.Programs.Rules.Mode.Name;
                            isValueOk = ConsoleTools.TryGetValueByKey(input, key, true, false, out string value);
                            if (!isValueOk) return;

                            KeyValues modes = new();
                            modes.Add(Key.Programs.Rules.Mode.File, true, false, typeof(string));
                            modes.Add(Key.Programs.Rules.Mode.Text, true, false, typeof(string));
                            modes.Add(Key.Programs.Rules.Mode.Disable, true, false, typeof(string));

                            isValueOk = ConsoleTools.TryGetString(key, value, true, modes, out value);
                            if (!isValueOk) return;
                            modeStr = value;

                            // Get -Mode
                            AgnosticProgram.Rules.Mode mode = AgnosticProgram.Rules.Mode.Disable;
                            if (modeStr.ToLower().Equals(Key.Programs.Rules.Mode.File.ToLower()))
                                mode = AgnosticProgram.Rules.Mode.File;
                            else if (modeStr.ToLower().Equals(Key.Programs.Rules.Mode.Text.ToLower()))
                                mode = AgnosticProgram.Rules.Mode.Text;
                            else if (modeStr.ToLower().Equals(Key.Programs.Rules.Mode.Disable.ToLower()))
                                mode = AgnosticProgram.Rules.Mode.Disable;

                            // Get -PathOrText
                            string pathOrText = string.Empty;
                            key = Key.Programs.Rules.PathOrText;
                            if (mode != AgnosticProgram.Rules.Mode.Disable)
                            {
                                isValueOk = ConsoleTools.TryGetValueByKey(input, key, true, true, out value);
                                if (!isValueOk) return;
                                isValueOk = ConsoleTools.TryGetString(key, value, true, out value);
                                if (!isValueOk) return;
                                pathOrText = value;
                            }

                            if (mode == AgnosticProgram.Rules.Mode.File)
                            {
                                pathOrText = Path.GetFullPath(pathOrText);
                                if (!File.Exists(pathOrText))
                                {
                                    string msgNotExist = $"{pathOrText}\nFile Not Exist.";
                                    await WriteToStdoutAsync(msgNotExist, ConsoleColor.Red);
                                    return;
                                }
                            }

                            if (mode == AgnosticProgram.Rules.Mode.Text)
                            {
                                pathOrText = pathOrText.ToLower().Replace(@"\n", Environment.NewLine);
                            }

                            AgnosticProgram.Rules RulesProgram = new();
                            await RulesProgram.SetAsync(mode, pathOrText);

                            await ShowRulesMsgAsync(RulesProgram);
                        }

                        // Dns Limit
                        if (input.ToLower().StartsWith($"{Key.Programs.Name.ToLower()} {Key.Programs.DnsLimit.Name.ToLower()}"))
                        {
                            // Programs DnsLimit -Enable=m -DisablePlain=m -DoHPathLimitMode=m -PathOrText="m"

                            // Get Enable
                            bool enable = false;
                            string key = Key.Programs.DnsLimit.Enable;
                            isValueOk = ConsoleTools.TryGetValueByKey(input, key, true, false, out string value);
                            if (!isValueOk) return;
                            isValueOk = ConsoleTools.TryGetBool(key, value, true, out bool valueBool);
                            if (!isValueOk) return;
                            enable = valueBool;

                            // Get DisablePlain
                            bool disablePlain = false;
                            if (enable)
                            {
                                key = Key.Programs.DnsLimit.DisablePlain;
                                isValueOk = ConsoleTools.TryGetValueByKey(input, key, true, false, out value);
                                if (!isValueOk) return;
                                isValueOk = ConsoleTools.TryGetBool(key, value, true, out valueBool);
                                if (!isValueOk) return;
                                disablePlain = valueBool;
                            }
                            
                            // Get ModeStr
                            string modeStr = Key.Programs.DnsLimit.DoHPathLimitMode.Disable;
                            if (enable)
                            {
                                key = Key.Programs.DnsLimit.DoHPathLimitMode.Name;
                                isValueOk = ConsoleTools.TryGetValueByKey(input, key, true, false, out value);
                                if (!isValueOk) return;

                                KeyValues modes = new();
                                modes.Add(Key.Programs.DnsLimit.DoHPathLimitMode.File, true, false, typeof(string));
                                modes.Add(Key.Programs.DnsLimit.DoHPathLimitMode.Text, true, false, typeof(string));
                                modes.Add(Key.Programs.DnsLimit.DoHPathLimitMode.Disable, true, false, typeof(string));

                                isValueOk = ConsoleTools.TryGetString(key, value, true, modes, out value);
                                if (!isValueOk) return;
                                modeStr = value;
                            }
                            
                            // Get -DoHPathLimitMode
                            AgnosticProgram.DnsLimit.LimitDoHPathsMode mode = AgnosticProgram.DnsLimit.LimitDoHPathsMode.Disable;
                            if (modeStr.ToLower().Equals(Key.Programs.DnsLimit.DoHPathLimitMode.File.ToLower()))
                                mode = AgnosticProgram.DnsLimit.LimitDoHPathsMode.File;
                            else if (modeStr.ToLower().Equals(Key.Programs.DnsLimit.DoHPathLimitMode.Text.ToLower()))
                                mode = AgnosticProgram.DnsLimit.LimitDoHPathsMode.Text;
                            else if (modeStr.ToLower().Equals(Key.Programs.DnsLimit.DoHPathLimitMode.Disable.ToLower()))
                                mode = AgnosticProgram.DnsLimit.LimitDoHPathsMode.Disable;

                            // Get -PathOrText
                            string pathOrText = string.Empty;
                            key = Key.Programs.DnsLimit.PathOrText;
                            if (mode != AgnosticProgram.DnsLimit.LimitDoHPathsMode.Disable)
                            {
                                isValueOk = ConsoleTools.TryGetValueByKey(input, key, true, true, out value);
                                if (!isValueOk) return;
                                isValueOk = ConsoleTools.TryGetString(key, value, true, out value);
                                if (!isValueOk) return;
                                pathOrText = value;
                            }

                            if (mode == AgnosticProgram.DnsLimit.LimitDoHPathsMode.File)
                            {
                                pathOrText = Path.GetFullPath(pathOrText);
                                if (!File.Exists(pathOrText))
                                {
                                    string msgNotExist = $"{pathOrText}\nFile Not Exist.";
                                    await WriteToStdoutAsync(msgNotExist, ConsoleColor.Red);
                                    return;
                                }
                            }

                            if (mode == AgnosticProgram.DnsLimit.LimitDoHPathsMode.Text)
                            {
                                pathOrText = pathOrText.Replace(@"\n", Environment.NewLine);
                            }

                            AgnosticProgram.DnsLimit dnsLimitProgram = new();
                            dnsLimitProgram.Set(enable, disablePlain, mode, pathOrText);

                            await ShowDnsLimitMsgAsync(dnsLimitProgram);
                        }

                    }
                }

                // Kill All Requests
                else if (input.ToLower().Equals(Key.Common.KillAll.ToLower()))
                {
                    // KillAll
                    bool killed = false;
                    foreach (ServerProfile sf in ServerProfiles)
                    {
                        if (sf.AgnosticServer != null && sf.Settings != null && !string.IsNullOrEmpty(sf.Name))
                        {
                            if (sf.AgnosticServer.IsRunning && sf.Settings.Working_Mode == AgnosticSettings.WorkingMode.DnsAndProxy)
                            {
                                sf.AgnosticServer.KillAll();
                                await WriteToStdoutAsync($"Done. Killed All Requests Of {sf.Name}.", ConsoleColor.Green);
                                killed = true;
                            }
                        }
                    }

                    if (!killed) await WriteToStdoutAsync($"There Is No Running Server.", ConsoleColor.Green);
                }

                // Write Requests To Log: True
                else if (input.ToLower().Equals($"{Key.Common.Requests.ToLower()} true"))
                {
                    // WriteRequests True
                    WriteRequestsToLog = true;
                    await WriteToStdoutAsync($"WriteRequestsToLog: True", ConsoleColor.Green, true, $"{Key.Common.Requests} {WriteRequestsToLog.ToString().CapitalizeFirstLetter()}");

                    // Save Command To List
                    string baseCmd = Key.Common.Requests;
                    string cmd = $"{baseCmd} True";
                    LoadCommands.AddOrUpdateCommand(baseCmd, cmd);
                }

                // Write Requests To Log: False
                else if (input.ToLower().Equals($"{Key.Common.Requests.ToLower()} false"))
                {
                    // WriteRequests False
                    WriteRequestsToLog = false;
                    await WriteToStdoutAsync($"WriteRequestsToLog: False", ConsoleColor.Green, true, $"{Key.Common.Requests} {WriteRequestsToLog.ToString().CapitalizeFirstLetter()}");

                    // Save Command To List
                    string baseCmd = Key.Common.Requests;
                    string cmd = $"{baseCmd} False";
                    LoadCommands.AddOrUpdateCommand(baseCmd, cmd);
                }

                // Write Chunk Details To Log: True
                else if (input.ToLower().Equals($"{Key.Common.FragmentDetails.ToLower()} true"))
                {
                    // ChunkDetails True
                    WriteFragmentDetailsToLog = true;
                    await WriteToStdoutAsync($"WriteFragmentDetailsToLog: True", ConsoleColor.Green, true, $"{Key.Common.FragmentDetails} {WriteFragmentDetailsToLog.ToString().CapitalizeFirstLetter()}");

                    // Save Command To List
                    string baseCmd = Key.Common.FragmentDetails;
                    string cmd = $"{baseCmd} True";
                    LoadCommands.AddOrUpdateCommand(baseCmd, cmd);
                }

                // Write Chunk Details To Log: False
                else if (input.ToLower().Equals($"{Key.Common.FragmentDetails.ToLower()} false"))
                {
                    // ChunkDetails False
                    WriteFragmentDetailsToLog = false;
                    await WriteToStdoutAsync($"WriteFragmentDetailsToLog: False", ConsoleColor.Green, true, $"{Key.Common.FragmentDetails} {WriteFragmentDetailsToLog.ToString().CapitalizeFirstLetter()}");

                    // Save Command To List
                    string baseCmd = Key.Common.FragmentDetails;
                    string cmd = $"{baseCmd} False";
                    LoadCommands.AddOrUpdateCommand(baseCmd, cmd);
                }

                // Start Server
                else if (input.ToLower().Equals(Key.Common.Start.ToLower()))
                {
                    // Start
                    bool confirmed = false;
                    bool flushNeeded = false;
                    foreach (ServerProfile sf in ServerProfiles)
                    {
                        if (sf.AgnosticServer != null && sf.Settings != null && !string.IsNullOrEmpty(sf.Name))
                        {
                            await WriteToStdoutAsync(string.Empty);
                            if (!sf.AgnosticServer.IsRunning)
                            {
                                await WriteToStdoutAsync($"Starting {sf.Name}...", ConsoleColor.Cyan);

                                // Check For Duplicate Ports
                                bool isTheSamePort = false;
                                foreach (ServerProfile sfPort in ServerProfiles)
                                    if (!sf.Name.Equals(sfPort.Name) && sfPort.Settings != null)
                                        if (sf.Settings.ListenerPort == sfPort.Settings.ListenerPort)
                                        {
                                            isTheSamePort = true;
                                            break;
                                        }

                                if (isTheSamePort)
                                {
                                    await WriteToStdoutAsync($"Cannot Use One Port For Multiple Servers.", ConsoleColor.Red);
                                    continue;
                                }

                                // Kill PIDs
                                if (OperatingSystem.IsWindows())
                                {
                                    List<int> pids = ProcessManager.GetProcessPidsByUsingPort(sf.Settings.ListenerPort);
                                    foreach (int pid in pids) await ProcessManager.KillProcessByPidAsync(pid);
                                    await Task.Delay(5);
                                    pids = ProcessManager.GetProcessPidsByUsingPort(sf.Settings.ListenerPort);
                                    foreach (int pid in pids) await ProcessManager.KillProcessByPidAsync(pid);
                                }

                                // Check Port
                                bool isPortOpen = NetworkTool.IsPortOpen(sf.Settings.ListenerPort);

                                if (isPortOpen)
                                {
                                    await WriteToStdoutAsync($"Port {sf.Settings.ListenerPort} Is Occupied, Choose Another.", ConsoleColor.Red);
                                    continue;
                                }

                                sf.AgnosticServer.OnRequestReceived -= ProxyServer_OnRequestReceived;
                                sf.AgnosticServer.OnRequestReceived += ProxyServer_OnRequestReceived;

                                if (sf.SettingsSSL != null) await sf.AgnosticServer.EnableSSL(sf.SettingsSSL);
                                if (sf.Fragment != null) sf.AgnosticServer.EnableFragment(sf.Fragment);
                                if (sf.Rules != null) sf.AgnosticServer.EnableRules(sf.Rules);
                                if (sf.DnsLimit != null) sf.AgnosticServer.EnableDnsLimit(sf.DnsLimit);
                                sf.AgnosticServer.Start(sf.Settings);
                                await Task.Delay(50);
                                if (sf.AgnosticServer.IsRunning)
                                {
                                    if (sf.Fragment != null)
                                    {
                                        sf.Fragment.OnChunkDetailsReceived -= FragmentStaticProgram_OnChunkDetailsReceived;
                                        sf.Fragment.OnChunkDetailsReceived += FragmentStaticProgram_OnChunkDetailsReceived;
                                    }
                                    await WriteToStdoutAsync($"{sf.Name} Started", ConsoleColor.Green);
                                    confirmed = true;

                                    if (sf.Settings.Working_Mode == AgnosticSettings.WorkingMode.DnsAndProxy)
                                        flushNeeded = true;
                                }
                                else
                                {
                                    sf.AgnosticServer.OnRequestReceived -= ProxyServer_OnRequestReceived;
                                    if (sf.Fragment != null)
                                        sf.Fragment.OnChunkDetailsReceived -= FragmentStaticProgram_OnChunkDetailsReceived;
                                    await WriteToStdoutAsync($"Couldn't Start {sf.Name}", ConsoleColor.Red);
                                }
                            }
                            else await WriteToStdoutAsync($"{sf.Name} Already Started", ConsoleColor.Gray);
                        }
                    }

                    if (flushNeeded)
                    {
                        await WriteToStdoutAsync(string.Empty);
                        await WriteToStdoutAsync("Flushing System DNS...", ConsoleColor.Cyan);
                        ProcessManager.ExecuteOnly("ipconfig", null, "/flushdns", true, true);
                        await WriteToStdoutAsync("System DNS Flushed", ConsoleColor.Green);
                    }

                    if (confirmed)
                    {
                        await WriteToStdoutAsync("Confirmed", ConsoleColor.Green, true, Key.Common.Start);
                    }
                }

                // Stop Server
                else if (input.ToLower().Equals(Key.Common.Stop.ToLower()))
                {
                    // Stop
                    foreach (ServerProfile sf in ServerProfiles)
                    {
                        if (sf.AgnosticServer != null && sf.Settings != null && !string.IsNullOrEmpty(sf.Name))
                        {
                            await WriteToStdoutAsync(string.Empty);
                            if (sf.AgnosticServer.IsRunning)
                            {
                                await WriteToStdoutAsync($"Stopping {sf.Name}...", ConsoleColor.Cyan);

                                sf.AgnosticServer.OnRequestReceived -= ProxyServer_OnRequestReceived;
                                if (sf.Fragment != null)
                                    sf.Fragment.OnChunkDetailsReceived -= FragmentStaticProgram_OnChunkDetailsReceived;

                                sf.AgnosticServer.Stop();
                                await Task.Delay(50);
                                if (!sf.AgnosticServer.IsRunning)
                                    await WriteToStdoutAsync($"{sf.Name} Stopped", ConsoleColor.Green);
                                else
                                    await WriteToStdoutAsync($"Couldn't Stop {sf.Name}", ConsoleColor.Red);
                            }
                            else await WriteToStdoutAsync($"{sf.Name} Already Stopped", ConsoleColor.Gray);
                        }
                    }
                }

                // Update Server Programs Settings
                else if (input.ToLower().Equals(Key.Common.Update.ToLower()))
                {
                    // Update
                    foreach (ServerProfile sf in ServerProfiles)
                    {
                        if (sf.AgnosticServer != null && sf.Settings != null && !string.IsNullOrEmpty(sf.Name))
                        {
                            await WriteToStdoutAsync(string.Empty);
                            await WriteToStdoutAsync($"Updating {sf.Name}...", ConsoleColor.Cyan);

                            if (sf.Fragment != null) sf.AgnosticServer.EnableFragment(sf.Fragment);
                            if (sf.Rules != null) sf.AgnosticServer.EnableRules(sf.Rules);
                            if (sf.DnsLimit != null) sf.AgnosticServer.EnableDnsLimit(sf.DnsLimit);

                            await WriteToStdoutAsync($"{sf.Name} Updated", ConsoleColor.Green);
                        }
                    }
                }

                // Save Commands
                else if (input.ToLower().Equals(Key.Common.Save.ToLower()))
                    await SaveCommandsToFileAsync();

                // Wrong Command
                else if (input.Length > 0)
                    await WriteToStdoutAsync("Wrong Command. Type \"Help\" To Get More Info.", ConsoleColor.Red);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExecuteCommandsAsync: " + ex.Message);
        }
    }
}