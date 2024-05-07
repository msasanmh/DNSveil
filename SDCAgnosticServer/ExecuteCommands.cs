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
                if (input.ToLower().StartsWith('/')) return;

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
                        WriteToStdout($"Error Console: {ex.Message}");
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
                        WriteToStdout($"Error Console: {ex.Message}");
                    }
                }

                // Get Help
                else if (input.ToLower().Equals("/?") || input.ToLower().Equals("/h") || input.ToLower().Equals("-h") ||
                         input.ToLower().Equals("help") || input.ToLower().Equals("/help") || input.ToLower().Equals("-help"))
                    Help.GetHelp();

                // Get Help Programs
                else if (input.ToLower().StartsWith("help program"))
                {
                    string prefix = "help program";
                    if (input.ToLower().Equals($"{prefix} {Key.Programs.Fragment.Name.ToLower()}")) Help.GetHelpFragment();
                    else if (input.ToLower().Equals($"{prefix} {Key.Programs.DnsRules.Name.ToLower()}")) Help.GetHelpDnsRules();
                    else if (input.ToLower().Equals($"{prefix} {Key.Programs.ProxyRules.Name.ToLower()}")) Help.GetHelpProxyRules();
                    else Help.GetHelpPrograms();
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
                        ShowProfileMsg();
                    }
                    else WriteToStdout($"Wrong Profile Name", ConsoleColor.DarkRed);
                }

                // Check Profile Name
                else if (string.IsNullOrEmpty(Profile.Trim()))
                {
                    WriteToStdout($"Set Profile Name First. Command: Profile <ProfileName>", ConsoleColor.Cyan);
                    return;
                }

                // Get Status Machine Read
                else if (input.ToLower().StartsWith(Key.Common.Out.ToLower()))
                {
                    string[] split = input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (split.Length > 1)
                    {
                        string profile = split[1];
                        MainDetails(profile);
                    }
                    else WriteToStdout($"Wrong Profile Name", ConsoleColor.DarkRed);
                }

                // Get Status Human Read
                else if (input.ToLower().Equals(Key.Common.Status.ToLower()))
                    ShowStatus();

                // Flush DNS
                else if (input.ToLower().Equals(Key.Common.Flush.ToLower()))
                    FlushDns();

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
                    ShowParentProcessMsg();
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
                                WriteToStdout($"Port Set To {port}", ConsoleColor.Green);
                                break;
                            }
                            else
                            {
                                if (isInt)
                                    WriteToStdout($"Port Number Must Be Between {DefaultPortMin} and {DefaultPortMax}", ConsoleColor.Red);
                                else
                                    WriteToStdout("Error Parsing The Int Number!", ConsoleColor.Red);
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

                            WriteToStdout($"Working Mode Set to {workingMode}", ConsoleColor.Green);
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
                                WriteToStdout($"Maximum Requests Set to {maxRequests}", ConsoleColor.Green);
                                break;
                            }
                            else
                            {
                                if (isInt)
                                    WriteToStdout($"Maximum Requests Must Be Between {DefaultMaxRequestsMin} and {DefaultMaxRequestsMax}", ConsoleColor.Red);
                                else
                                    WriteToStdout("Error Parsing The Int Number!", ConsoleColor.Red);
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
                                WriteToStdout($"Dns Timeout Set to {dnsTimeoutSec} Seconds", ConsoleColor.Green);
                                break;
                            }
                            else
                            {
                                if (isInt)
                                    WriteToStdout($"Dns Timeout Must Be Between {DefaultDnsTimeoutSecMin} and {DefaultDnsTimeoutSecMax}", ConsoleColor.Red);
                                else
                                    WriteToStdout("Error Parsing The Int Number!", ConsoleColor.Red);
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
                                    WriteToStdout($"Proxy Timeout Set to {proxyTimeoutSec} Seconds", ConsoleColor.Green);
                                    break;
                                }
                                else
                                {
                                    if (isInt)
                                        WriteToStdout($"Proxy Timeout Must Be Between {DefaultProxyTimeoutSecMin} and {DefaultProxyTimeoutSecMax}", ConsoleColor.Red);
                                    else
                                        WriteToStdout("Error Parsing The Int Number!", ConsoleColor.Red);
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
                                    WriteToStdout($"Kill On Cpu Usage Set to {killOnCpuUsage}%", ConsoleColor.Green);
                                    break;
                                }
                                else
                                {
                                    if (isFloat)
                                        WriteToStdout($"Percentage Must Be Between {DefaultKillOnCpuUsageMin} and {DefaultKillOnCpuUsageMax}", ConsoleColor.Red);
                                    else
                                        WriteToStdout("Error Parsing The Float Number!", ConsoleColor.Red);
                                }
                            }

                            // Block Port 80
                            while (true)
                            {
                                object value = await ConsoleTools.ReadValueAsync($"Block Port 80, Enter True/False (Default: {DefaultBlockPort80})", blockPort80, typeof(bool));
                                bool b = Convert.ToBoolean(value);

                                blockPort80 = b;
                                WriteToStdout($"Block Port 80: {blockPort80}", ConsoleColor.Green);
                                break;
                            }
                        }

                        // Allow Insecure
                        while (true)
                        {
                            object value = await ConsoleTools.ReadValueAsync($"Allow Insecure, Enter True/False (Default: {DefaultAllowInsecure})", allowInsecure, typeof(bool));
                            bool b = Convert.ToBoolean(value);

                            allowInsecure = b;
                            WriteToStdout($"Allow Insecure: {allowInsecure}", ConsoleColor.Green);
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

                            WriteToStdout($"DNS Addresses Set. Count: {dnss.Count}", ConsoleColor.Green);
                            break;
                        }

                        // Cloudflare Clean IP
                        while (true)
                        {
                            string cfCleanIp = string.Empty;
                            object value = await ConsoleTools.ReadValueAsync("Enter Cloudflare Clean IPv4 (Default: Empty)", cfCleanIp, typeof(string));
                            cfCleanIp = value.ToString() ?? string.Empty;
                            cfCleanIp = cfCleanIp.Trim();

                            if (!string.IsNullOrEmpty(cfCleanIp))
                            {
                                bool isIpv4Valid = NetworkTool.IsIPv4Valid(cfCleanIp, out _);
                                if (isIpv4Valid) cloudflareCleanIP = cfCleanIp;
                            }

                            if (!string.IsNullOrEmpty(cloudflareCleanIP))
                                WriteToStdout($"Cloudflare Clean IP Set To: {cloudflareCleanIP}", ConsoleColor.Green);
                            else
                                WriteToStdout($"Cloudflare Clean IP Disabled", ConsoleColor.Green);
                            break;
                        }

                        // Bootstrap Ip
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

                            WriteToStdout($"Bootstrap IP Set To: {bootstrapIp}", ConsoleColor.Green);
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
                                WriteToStdout($"Bootstrap Port Set to {bootstrapPort}", ConsoleColor.Green);
                                break;
                            }
                            else
                            {
                                if (isInt)
                                    WriteToStdout("Bootstrap Port Must Be Between 1 and 65535", ConsoleColor.Red);
                                else
                                    WriteToStdout("Error Parsing The Int Number!", ConsoleColor.Red);
                            }
                        }

                        // Upstream Proxy Scheme
                        while (true)
                        {
                            object value = await ConsoleTools.ReadValueAsync($"Enter Upstream Proxy Scheme (HTTP Or SOCKS5) (Default: Empty)", proxyScheme, typeof(string));
                            proxyScheme = value.ToString() ?? string.Empty;
                            proxyScheme = proxyScheme.Trim();

                            if (!string.IsNullOrEmpty(proxyScheme))
                                WriteToStdout($"Upstream Proxy Scheme Set To: {proxyScheme}", ConsoleColor.Green);
                            else
                                WriteToStdout("Upstream Proxy Scheme Disabled", ConsoleColor.Green);
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
                                    WriteToStdout($"Upstream Proxy Username Set To: {proxyUser}", ConsoleColor.Green);
                                else
                                    WriteToStdout("Upstream Proxy Username Set To: None", ConsoleColor.Green);
                                break;
                            }

                            // Upstream Proxy Password
                            while (true)
                            {
                                object value = await ConsoleTools.ReadValueAsync($"Enter Upstream Proxy Password (Default: Empty)", proxyPass, typeof(string));
                                proxyPass = value.ToString() ?? string.Empty;
                                proxyPass = proxyPass.Trim();

                                if (!string.IsNullOrEmpty(proxyPass))
                                    WriteToStdout($"Upstream Proxy Password Set To: {proxyPass}", ConsoleColor.Green);
                                else
                                    WriteToStdout("Upstream Proxy Password Set To: None", ConsoleColor.Green);
                                break;
                            }

                            // Apply Upstream Only To Blocked IPs
                            while (true)
                            {
                                object value = await ConsoleTools.ReadValueAsync($"Apply Upstream Only To Blocked IPs, Enter True/False (Default: {DefaultApplyUpstreamOnlyToBlockedIps})", applyUpstreamOnlyToBlockedIps, typeof(bool));
                                bool b = Convert.ToBoolean(value);

                                applyUpstreamOnlyToBlockedIps = b;
                                WriteToStdout($"Apply Upstream Only To Blocked IPs: {applyUpstreamOnlyToBlockedIps}", ConsoleColor.Green);
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

                    ShowSettingsMsg(settings);
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

                            WriteToStdout($"Enable Set to {enable}", ConsoleColor.Green);
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
                                        WriteToStdout(msgNotExist, ConsoleColor.Red);
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
                                            WriteToStdout(msgNotExist, ConsoleColor.Red);
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
                                        WriteToStdout(msgNotExist, ConsoleColor.Red);
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
                                            WriteToStdout(msgNotExist, ConsoleColor.Red);
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

                                WriteToStdout($"ChangeSni Set to {changeSni}", ConsoleColor.Green);
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
                                        WriteToStdout($"DefaultSni Set to {defaultSni}", ConsoleColor.Green);
                                        break;
                                    }

                                    WriteToStdout($"DefaultSni Set to Empty", ConsoleColor.Green);
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

                    ShowSettingsSSLMsg(settingsSSL);
                }

                // Programs
                else if (input.ToLower().StartsWith(Key.Programs.Name.ToLower()))
                {
                    string msg = "Available Programs:\n\n";
                    msg += $"{Key.Programs.Fragment.Name}\n";
                    msg += $"{Key.Programs.DnsRules.Name}\n";
                    msg += $"{Key.Programs.ProxyRules.Name}\n";

                    // Interactive Mode
                    if (input.ToLower().Equals(Key.Programs.Name.ToLower()))
                    {
                        WriteToStdout(msg, ConsoleColor.Cyan);

                        while (true)
                        {
                            string programName = string.Empty;
                            object valueP = await ConsoleTools.ReadValueAsync("Enter One Of Programs Name (Default: None):", programName, typeof(string));
                            programName = valueP.ToString() ?? string.Empty;
                            if (string.IsNullOrEmpty(programName) || string.IsNullOrWhiteSpace(programName))
                            {
                                WriteToStdout($"Exited From {Key.Programs.Name}.");
                                break;
                            }

                            // DPI Bypass
                            else if (programName.ToLower().Equals(Key.Programs.Fragment.Name.ToLower()))
                            {
                                string msgAm = $"Available {Key.Programs.Fragment.Name} Modes:\n\n";
                                msgAm += $"{Key.Programs.Fragment.Mode.Program.Name}\n";
                                msgAm += $"{Key.Programs.Fragment.Mode.Disable}\n";

                                WriteToStdout(msgAm, ConsoleColor.Cyan);

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
                                        WriteToStdout("Wrong Mode.", ConsoleColor.Red);
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
                                            WriteToStdout($"Chunks Number Must Be Between {DefaultFragmentBeforeSniChunksMin} and {DefaultFragmentBeforeSniChunksMax}", ConsoleColor.Red);
                                            continue;
                                        }
                                    }

                                    // Get Chunk Mode
                                    msgAm = $"Available {Key.Programs.Fragment.Mode.Program.ChunkMode.Name} Modes:\n\n";
                                    msgAm += $"{Key.Programs.Fragment.Mode.Program.ChunkMode.SNI}\n";
                                    msgAm += $"{Key.Programs.Fragment.Mode.Program.ChunkMode.SniExtension}\n";
                                    msgAm += $"{Key.Programs.Fragment.Mode.Program.ChunkMode.AllExtensions}\n";

                                    WriteToStdout(msgAm, ConsoleColor.Cyan);

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
                                            WriteToStdout("Wrong Mode.", ConsoleColor.Red);
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
                                            WriteToStdout($"Chunks Number Must Be Between {DefaultFragmentSniChunksMin} and {DefaultFragmentSniChunksMax}", ConsoleColor.Red);
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
                                            WriteToStdout($"Chunks Number Must Be Between {DefaultFragmentAntiPatternOffsetMin} and {DefaultFragmentAntiPatternOffsetMin}", ConsoleColor.Red);
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
                                            WriteToStdout($"Fragment Delay Must Be Between {DefaultFragmentFragmentDelayMin} and {DefaultFragmentFragmentDelayMax} Milliseconds", ConsoleColor.Red);
                                            continue;
                                        }
                                    }
                                }

                                AgnosticProgram.Fragment FragmentStaticProgram = new();
                                FragmentStaticProgram.Set(mode, beforeSniChunks, chunkMode, sniChunks, antiPatternOffset, fragmentDelay);

                                ShowFragmentMsg(FragmentStaticProgram);
                            }

                            // Dns Rules
                            else if (programName.ToLower().Equals(Key.Programs.DnsRules.Name.ToLower()))
                            {
                                string msgAm = $"Available {Key.Programs.DnsRules.Name} Modes:\n\n";
                                msgAm += $"{Key.Programs.DnsRules.Mode.File}\n";
                                msgAm += $"{Key.Programs.DnsRules.Mode.Text}\n";
                                msgAm += $"{Key.Programs.DnsRules.Mode.Disable}\n";

                                WriteToStdout(msgAm, ConsoleColor.Cyan);

                                string modeStr = Key.Programs.DnsRules.Mode.Disable;
                                AgnosticProgram.DnsRules.Mode mode = AgnosticProgram.DnsRules.Mode.Disable;
                                string msgRV = $"Enter One Of Modes (Default: {mode}):";
                                while (true)
                                {
                                    object value = await ConsoleTools.ReadValueAsync(msgRV, modeStr, typeof(string));
                                    modeStr = value.ToString() ?? string.Empty;
                                    if (modeStr.ToLower().Equals(Key.Programs.DnsRules.Mode.File.ToLower()))
                                        mode = AgnosticProgram.DnsRules.Mode.File;
                                    else if (modeStr.ToLower().Equals(Key.Programs.DnsRules.Mode.Text.ToLower()))
                                        mode = AgnosticProgram.DnsRules.Mode.Text;
                                    else if (modeStr.ToLower().Equals(Key.Programs.DnsRules.Mode.Disable.ToLower()))
                                        mode = AgnosticProgram.DnsRules.Mode.Disable;
                                    else
                                    {
                                        WriteToStdout("Wrong Mode.", ConsoleColor.Red);
                                        continue;
                                    }
                                    break;
                                }

                                string filePathOrText = string.Empty;

                                if (mode == AgnosticProgram.DnsRules.Mode.File)
                                {
                                    while (true)
                                    {
                                        msgRV = $"Enter The Path Of {mode} (Default: Cancel):";
                                        object valuePath = await ConsoleTools.ReadValueAsync(msgRV, string.Empty, typeof(string));
                                        filePathOrText = valuePath.ToString() ?? string.Empty;
                                        if (string.IsNullOrEmpty(filePathOrText))
                                        {
                                            mode = AgnosticProgram.DnsRules.Mode.Disable;
                                            break;
                                        }
                                        else
                                        {
                                            filePathOrText = Path.GetFullPath(filePathOrText);
                                            if (!File.Exists(filePathOrText))
                                            {
                                                string msgNotExist = $"{filePathOrText}\nFile Not Exist.";
                                                WriteToStdout(msgNotExist, ConsoleColor.Red);
                                                continue;
                                            }
                                            else break;
                                        }
                                    }
                                }

                                if (mode == AgnosticProgram.DnsRules.Mode.Text)
                                {
                                    msgRV = $"Enter {Key.Programs.DnsRules.Name} As Text (Default: Cancel):";
                                    msgRV += "\n    e.g. Google.com|8.8.8.8;\\nCloudflare.com|dns:tcp://8.8.8.8;";
                                    object valueText = await ConsoleTools.ReadValueAsync(msgRV, string.Empty, typeof(string));
                                    filePathOrText = valueText.ToString() ?? string.Empty;
                                    if (string.IsNullOrEmpty(filePathOrText))
                                    {
                                        mode = AgnosticProgram.DnsRules.Mode.Disable;
                                    }
                                    else
                                    {
                                        filePathOrText = filePathOrText.ToLower().Replace(@"\n", Environment.NewLine);
                                    }
                                }

                                AgnosticProgram.DnsRules dnsRulesProgram = new();
                                dnsRulesProgram.Set(mode, filePathOrText);

                                ShowDnsRulesMsg(dnsRulesProgram);
                            }

                            // ProxyRules
                            else if (programName.ToLower().Equals(Key.Programs.ProxyRules.Name.ToLower()))
                            {
                                string msgAm = $"Available {Key.Programs.ProxyRules.Name} Modes:\n\n";
                                msgAm += $"{Key.Programs.ProxyRules.Mode.File}\n";
                                msgAm += $"{Key.Programs.ProxyRules.Mode.Text}\n";
                                msgAm += $"{Key.Programs.ProxyRules.Mode.Disable}\n";

                                WriteToStdout(msgAm, ConsoleColor.Cyan);

                                string modeStr = Key.Programs.ProxyRules.Mode.Disable;
                                AgnosticProgram.ProxyRules.Mode mode = AgnosticProgram.ProxyRules.Mode.Disable;
                                string msgRV = $"Enter One Of Modes (Default: {mode}):";
                                while (true)
                                {
                                    object value = await ConsoleTools.ReadValueAsync(msgRV, modeStr, typeof(string));
                                    modeStr = value.ToString() ?? string.Empty;
                                    if (modeStr.ToLower().Equals(Key.Programs.ProxyRules.Mode.File.ToLower()))
                                        mode = AgnosticProgram.ProxyRules.Mode.File;
                                    else if (modeStr.ToLower().Equals(Key.Programs.ProxyRules.Mode.Text.ToLower()))
                                        mode = AgnosticProgram.ProxyRules.Mode.Text;
                                    else if (modeStr.ToLower().Equals(Key.Programs.ProxyRules.Mode.Disable.ToLower()))
                                        mode = AgnosticProgram.ProxyRules.Mode.Disable;
                                    else
                                    {
                                        WriteToStdout("Wrong Mode.", ConsoleColor.Red);
                                        continue;
                                    }
                                    break;
                                }

                                string filePathOrText = string.Empty;

                                if (mode == AgnosticProgram.ProxyRules.Mode.File)
                                {
                                    while (true)
                                    {
                                        msgRV = $"Enter The Path Of {mode} (Default: Cancel):";
                                        object valuePath = await ConsoleTools.ReadValueAsync(msgRV, string.Empty, typeof(string));
                                        filePathOrText = valuePath.ToString() ?? string.Empty;
                                        if (string.IsNullOrEmpty(filePathOrText))
                                        {
                                            mode = AgnosticProgram.ProxyRules.Mode.Disable;
                                            break;
                                        }
                                        else
                                        {
                                            filePathOrText = Path.GetFullPath(filePathOrText);
                                            if (!File.Exists(filePathOrText))
                                            {
                                                string msgNotExist = $"{filePathOrText}\nFile Not Exist.";
                                                WriteToStdout(msgNotExist, ConsoleColor.Red);
                                                continue;
                                            }
                                            else break;
                                        }
                                    }
                                }

                                if (mode == AgnosticProgram.ProxyRules.Mode.Text)
                                {
                                    msgRV = $"Enter {Key.Programs.ProxyRules.Name} As Text (Default: Cancel):";
                                    msgRV += "\n    e.g. Google.com|8.8.8.8;\\nCloudflare.com|dns:tcp://8.8.8.8;";
                                    object valueText = await ConsoleTools.ReadValueAsync(msgRV, string.Empty, typeof(string));
                                    filePathOrText = valueText.ToString() ?? string.Empty;
                                    if (string.IsNullOrEmpty(filePathOrText))
                                    {
                                        mode = AgnosticProgram.ProxyRules.Mode.Disable;
                                    }
                                    else
                                    {
                                        filePathOrText = filePathOrText.ToLower().Replace(@"\n", Environment.NewLine);
                                    }
                                }

                                AgnosticProgram.ProxyRules proxyRulesProgram = new();
                                proxyRulesProgram.Set(mode, filePathOrText);

                                ShowProxyRulesMsg(proxyRulesProgram);
                            }

                            else
                            {
                                WriteToStdout("Wrong Program Name.", ConsoleColor.Red);
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

                            ShowFragmentMsg(fragmentStaticProgram);
                        }

                        // DnsRules
                        if (input.ToLower().StartsWith($"{Key.Programs.Name.ToLower()} {Key.Programs.DnsRules.Name.ToLower()}"))
                        {
                            // Programs DnsRules -Mode=m -PathOrText="m"

                            // Get ModeStr
                            string modeStr = Key.Programs.DnsRules.Mode.Disable;
                            string key = Key.Programs.DnsRules.Mode.Name;
                            isValueOk = ConsoleTools.TryGetValueByKey(input, key, true, false, out string value);
                            if (!isValueOk) return;

                            KeyValues modes = new();
                            modes.Add(Key.Programs.DnsRules.Mode.File, true, false, typeof(string));
                            modes.Add(Key.Programs.DnsRules.Mode.Text, true, false, typeof(string));
                            modes.Add(Key.Programs.DnsRules.Mode.Disable, true, false, typeof(string));

                            isValueOk = ConsoleTools.TryGetString(key, value, true, modes, out value);
                            if (!isValueOk) return;
                            modeStr = value;

                            // Get -Mode
                            AgnosticProgram.DnsRules.Mode mode = AgnosticProgram.DnsRules.Mode.Disable;
                            if (modeStr.ToLower().Equals(Key.Programs.DnsRules.Mode.File.ToLower()))
                                mode = AgnosticProgram.DnsRules.Mode.File;
                            else if (modeStr.ToLower().Equals(Key.Programs.DnsRules.Mode.Text.ToLower()))
                                mode = AgnosticProgram.DnsRules.Mode.Text;
                            else if (modeStr.ToLower().Equals(Key.Programs.DnsRules.Mode.Disable.ToLower()))
                                mode = AgnosticProgram.DnsRules.Mode.Disable;

                            // Get -PathOrText
                            string pathOrText = string.Empty;
                            key = Key.Programs.DnsRules.PathOrText;
                            if (mode != AgnosticProgram.DnsRules.Mode.Disable)
                            {
                                isValueOk = ConsoleTools.TryGetValueByKey(input, key, true, true, out value);
                                if (!isValueOk) return;
                                isValueOk = ConsoleTools.TryGetString(key, value, true, out value);
                                if (!isValueOk) return;
                                pathOrText = value;
                            }

                            if (mode == AgnosticProgram.DnsRules.Mode.File)
                            {
                                pathOrText = Path.GetFullPath(pathOrText);
                                if (!File.Exists(pathOrText))
                                {
                                    string msgNotExist = $"{pathOrText}\nFile Not Exist.";
                                    WriteToStdout(msgNotExist, ConsoleColor.Red);
                                    return;
                                }
                            }

                            if (mode == AgnosticProgram.DnsRules.Mode.Text)
                            {
                                pathOrText = pathOrText.ToLower().Replace(@"\n", Environment.NewLine);
                            }

                            AgnosticProgram.DnsRules dnsRulesProgram = new();
                            dnsRulesProgram.Set(mode, pathOrText);

                            ShowDnsRulesMsg(dnsRulesProgram);
                        }

                        // ProxyRules
                        if (input.ToLower().StartsWith($"{Key.Programs.Name.ToLower()} {Key.Programs.ProxyRules.Name.ToLower()}"))
                        {
                            // Programs ProxyRules -Mode=m -PathOrText="m"

                            // Get ModeStr
                            string modeStr = Key.Programs.ProxyRules.Mode.Disable;
                            string key = Key.Programs.ProxyRules.Mode.Name;
                            isValueOk = ConsoleTools.TryGetValueByKey(input, key, true, false, out string value);
                            if (!isValueOk) return;

                            KeyValues modes = new();
                            modes.Add(Key.Programs.ProxyRules.Mode.File, true, false, typeof(string));
                            modes.Add(Key.Programs.ProxyRules.Mode.Text, true, false, typeof(string));
                            modes.Add(Key.Programs.ProxyRules.Mode.Disable, true, false, typeof(string));

                            isValueOk = ConsoleTools.TryGetString(key, value, true, modes, out value);
                            if (!isValueOk) return;
                            modeStr = value;

                            // Get -Mode
                            AgnosticProgram.ProxyRules.Mode mode = AgnosticProgram.ProxyRules.Mode.Disable;
                            if (modeStr.ToLower().Equals(Key.Programs.ProxyRules.Mode.File.ToLower()))
                                mode = AgnosticProgram.ProxyRules.Mode.File;
                            else if (modeStr.ToLower().Equals(Key.Programs.ProxyRules.Mode.Text.ToLower()))
                                mode = AgnosticProgram.ProxyRules.Mode.Text;
                            else if (modeStr.ToLower().Equals(Key.Programs.ProxyRules.Mode.Disable.ToLower()))
                                mode = AgnosticProgram.ProxyRules.Mode.Disable;

                            // Get -PathOrText
                            string pathOrText = string.Empty;
                            key = Key.Programs.ProxyRules.PathOrText;
                            if (mode != AgnosticProgram.ProxyRules.Mode.Disable)
                            {
                                isValueOk = ConsoleTools.TryGetValueByKey(input, key, true, true, out value);
                                if (!isValueOk) return;
                                isValueOk = ConsoleTools.TryGetString(key, value, true, out value);
                                if (!isValueOk) return;
                                pathOrText = value;
                            }

                            if (mode == AgnosticProgram.ProxyRules.Mode.File)
                            {
                                pathOrText = Path.GetFullPath(pathOrText);
                                if (!File.Exists(pathOrText))
                                {
                                    string msgNotExist = $"{pathOrText}\nFile Not Exist.";
                                    WriteToStdout(msgNotExist, ConsoleColor.Red);
                                    return;
                                }
                            }

                            if (mode == AgnosticProgram.ProxyRules.Mode.Text)
                            {
                                pathOrText = pathOrText.ToLower().Replace(@"\n", Environment.NewLine);
                            }

                            AgnosticProgram.ProxyRules proxyRulesProgram = new();
                            proxyRulesProgram.Set(mode, pathOrText);

                            ShowProxyRulesMsg(proxyRulesProgram);
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
                                WriteToStdout($"Done. Killed All Requests Of {sf.Name}.", ConsoleColor.Green);
                                killed = true;
                            }
                        }
                    }

                    if (!killed) WriteToStdout($"There Is No Running Server.", ConsoleColor.Green);
                }

                // Write Requests To Log: True
                else if (input.ToLower().Equals($"{Key.Common.Requests.ToLower()} true"))
                {
                    // WriteRequests True
                    WriteRequestsToLog = true;
                    WriteToStdout($"WriteRequestsToLog: True", ConsoleColor.Green);

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
                    WriteToStdout($"WriteRequestsToLog: False", ConsoleColor.Green);

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
                    WriteToStdout($"WriteFragmentDetailsToLog: True", ConsoleColor.Green);

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
                    WriteToStdout($"WriteFragmentDetailsToLog: False", ConsoleColor.Green);

                    // Save Command To List
                    string baseCmd = Key.Common.FragmentDetails;
                    string cmd = $"{baseCmd} False";
                    LoadCommands.AddOrUpdateCommand(baseCmd, cmd);
                }

                // Start Proxy Server
                else if (input.ToLower().Equals(Key.Common.Start.ToLower()))
                {
                    // Start
                    bool flushNeeded = false;
                    foreach (ServerProfile sf in ServerProfiles)
                    {
                        if (sf.AgnosticServer != null && sf.Settings != null && !string.IsNullOrEmpty(sf.Name))
                        {
                            WriteToStdout(string.Empty);
                            if (!sf.AgnosticServer.IsRunning)
                            {
                                WriteToStdout($"Starting {sf.Name}...", ConsoleColor.Cyan);

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
                                    WriteToStdout($"Cannot Use One Port For Multiple Servers.", ConsoleColor.Red);
                                    continue;
                                }

                                // Kill PIDs
                                if (OperatingSystem.IsWindows())
                                {
                                    List<int> pids = ProcessManager.GetProcessPidsByUsingPort(sf.Settings.ListenerPort);
                                    if (pids.Any())
                                    {
                                        foreach (int pid in pids) ProcessManager.KillProcessByPID(pid);
                                        await Task.Delay(5);
                                    }
                                }

                                // Check Port
                                bool isPortOpen = NetworkTool.IsPortOpen(sf.Settings.ListenerPort);

                                if (isPortOpen)
                                {
                                    WriteToStdout($"Port {sf.Settings.ListenerPort} Is Occupied, Choose Another.", ConsoleColor.Red);
                                    continue;
                                }

                                sf.AgnosticServer.OnRequestReceived -= ProxyServer_OnRequestReceived;
                                sf.AgnosticServer.OnRequestReceived += ProxyServer_OnRequestReceived;

                                if (sf.SettingsSSL != null) await sf.AgnosticServer.EnableSSL(sf.SettingsSSL);
                                if (sf.Fragment != null) sf.AgnosticServer.EnableFragment(sf.Fragment);
                                if (sf.DnsRules != null) sf.AgnosticServer.EnableDnsRules(sf.DnsRules);
                                if (sf.ProxyRules != null) sf.AgnosticServer.EnableProxyRules(sf.ProxyRules);
                                sf.AgnosticServer.Start(sf.Settings);
                                await Task.Delay(50);
                                if (sf.AgnosticServer.IsRunning)
                                {
                                    if (sf.Fragment != null)
                                    {
                                        sf.Fragment.OnChunkDetailsReceived -= FragmentStaticProgram_OnChunkDetailsReceived;
                                        sf.Fragment.OnChunkDetailsReceived += FragmentStaticProgram_OnChunkDetailsReceived;
                                    }
                                    WriteToStdout($"{sf.Name} Started", ConsoleColor.Green);
                                    flushNeeded = true;
                                }
                                else
                                {
                                    CountRequests = 0;
                                    sf.AgnosticServer.OnRequestReceived -= ProxyServer_OnRequestReceived;
                                    if (sf.Fragment != null)
                                        sf.Fragment.OnChunkDetailsReceived -= FragmentStaticProgram_OnChunkDetailsReceived;
                                    WriteToStdout($"Couldn't Start {sf.Name}", ConsoleColor.Red);
                                }
                            }
                            else WriteToStdout($"{sf.Name} Already Started", ConsoleColor.Gray);
                        }
                    }

                    if (flushNeeded)
                    {
                        WriteToStdout(string.Empty);
                        WriteToStdout("Flushing System DNS...", ConsoleColor.Cyan);
                        ProcessManager.ExecuteOnly("ipconfig", null, "/flushdns", true, true);
                        WriteToStdout("System DNS Flushed", ConsoleColor.Green);
                    }
                }

                // Stop Proxy Server
                else if (input.ToLower().Equals(Key.Common.Stop.ToLower()))
                {
                    // Stop
                    CountRequests = 0;
                    foreach (ServerProfile sf in ServerProfiles)
                    {
                        if (sf.AgnosticServer != null && sf.Settings != null && !string.IsNullOrEmpty(sf.Name))
                        {
                            WriteToStdout(string.Empty);
                            if (sf.AgnosticServer.IsRunning)
                            {
                                WriteToStdout($"Stopping {sf.Name}...", ConsoleColor.Cyan);

                                sf.AgnosticServer.OnRequestReceived -= ProxyServer_OnRequestReceived;
                                if (sf.Fragment != null)
                                    sf.Fragment.OnChunkDetailsReceived -= FragmentStaticProgram_OnChunkDetailsReceived;

                                sf.AgnosticServer.Stop();
                                await Task.Delay(50);
                                if (!sf.AgnosticServer.IsRunning)
                                    WriteToStdout($"{sf.Name} Stopped", ConsoleColor.Green);
                                else
                                    WriteToStdout($"Couldn't Stop {sf.Name}", ConsoleColor.Red);
                            }
                            else WriteToStdout($"{sf.Name} Already Stopped", ConsoleColor.Gray);
                        }
                    }
                }

                // Save Commands
                else if (input.ToLower().Equals(Key.Common.Save.ToLower()))
                    SaveCommandsToFile();

                // Wrong Command
                else if (input.Length > 0)
                    WriteToStdout("Wrong Command. Type \"Help\" To Get More Info.", ConsoleColor.Red);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExecuteCommandsAsync: " + ex.Message);
        }
    }
}