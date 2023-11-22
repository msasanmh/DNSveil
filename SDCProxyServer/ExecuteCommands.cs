using MsmhToolsClass;
using MsmhToolsClass.MsmhProxyServer;
using MsmhToolsClass.ProxyServerPrograms;
using System.Diagnostics;
using System.Net;

namespace SDCProxyServer;

public static partial class Program
{
    private static async Task ExecuteCommands(string? input)
    {
        try
        {
            await Task.Run(async () =>
            {
                if (string.IsNullOrEmpty(input)) return;
                if (string.IsNullOrWhiteSpace(input)) return;
                input = input.Trim();

                // Clear Screen - Stop Writing
                if (input.ToLower().Equals("c"))
                {
                    WriteRequestsToLog = false;
                    WriteChunkDetailsToLog = false;
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
                else if (input.ToLower().Equals("cls"))
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

                // Get Status Machine Read
                else if (input.ToLower().Equals("out"))
                    MainDetails();

                // Get Status Human Read
                else if (input.ToLower().Equals("status"))
                    ShowStatus();

                // Get Help
                else if (input.ToLower().Equals("/?") || input.ToLower().Equals("/h") || input.ToLower().Equals("-h") ||
                         input.ToLower().Equals("help") || input.ToLower().Equals("/help") || input.ToLower().Equals("-help"))
                    Help.GetHelp();

                // Get Help Programs
                else if (input.ToLower().StartsWith("help program"))
                {
                    string prefix = "help program";
                    if (input.ToLower().Equals($"{prefix} {Key.Programs.BwList.Name.ToLower()}")) Help.GetHelpBwList();
                    else if (input.ToLower().Equals($"{prefix} {Key.Programs.Dns.Name.ToLower()}")) Help.GetHelpDns();
                    else if (input.ToLower().Equals($"{prefix} {Key.Programs.DontBypass.Name.ToLower()}")) Help.GetHelpDontBypass();
                    else if (input.ToLower().Equals($"{prefix} {Key.Programs.DpiBypass.Name.ToLower()}")) Help.GetHelpDpiBypass();
                    else if (input.ToLower().Equals($"{prefix} {Key.Programs.FakeDns.Name.ToLower()}")) Help.GetHelpFakeDns();
                    else if (input.ToLower().Equals($"{prefix} {Key.Programs.UpStreamProxy.Name.ToLower()}")) Help.GetHelpUpStreamProxy();
                    else
                        Help.GetHelpPrograms();
                }

                // Settings
                else if (input.ToLower().StartsWith(Key.Setting.Name.ToLower()))
                {
                    IPAddress ip = IPAddress.Any;
                    int port = DefaultPort;
                    int maxRequests = DefaultMaxRequests;
                    int requestTimeoutSec = DefaultRequestTimeoutSec;
                    float killOnCpuUsage = DefaultKillOnCpuUsage;
                    bool blockPort80 = DefaultBlockPort80;

                    // Interactive Mode
                    if (input.ToLower().Equals(Key.Setting.Name.ToLower()))
                    {
                        // Port
                        while (true)
                        {
                            object value = await ConsoleTools.ReadValue($"Enter Port Number (Default: {DefaultPort})", port, typeof(int));
                            int n = Convert.ToInt32(value);
                            if (n >= DefaultPortMin && n <= DefaultPortMax)
                            {
                                // Check Port
                                bool isPortOpen = NetworkTool.IsPortOpen(IPAddress.Loopback.ToString(), n, 3);
                                if (isPortOpen)
                                {
                                    WriteToStdout($"Port {port} is occupied, Choose Another.");
                                    continue;
                                }

                                port = n;
                                WriteToStdout($"Port Set to {port}", ConsoleColor.Green);
                                break;
                            }
                            else
                            {
                                WriteToStdout($"Port Number Must Be Between {DefaultPortMin} and {DefaultPortMax}", ConsoleColor.Red);
                            }
                        }

                        // Max Threads
                        while (true)
                        {
                            object value = await ConsoleTools.ReadValue($"Enter Maximum Requests To Handle Per Second (Default: {DefaultMaxRequests})", maxRequests, typeof(int));
                            int n = Convert.ToInt32(value);
                            if (n >= DefaultMaxRequestsMin && n <= DefaultMaxRequestsMax)
                            {
                                maxRequests = n;
                                WriteToStdout($"Maximum Requests Set to {maxRequests}", ConsoleColor.Green);
                                break;
                            }
                            else
                            {
                                WriteToStdout($"Maximum Requests Must Be Between {DefaultMaxRequestsMin} and {DefaultMaxRequestsMax}", ConsoleColor.Red);
                            }
                        }

                        // Request Timeout Sec
                        while (true)
                        {
                            object value = await ConsoleTools.ReadValue($"Enter Request Timeout in Sec (Default: {DefaultRequestTimeoutSec} Sec)", requestTimeoutSec, typeof(int));
                            int n = Convert.ToInt32(value);
                            if (n >= DefaultRequestTimeoutSecMin && n <= DefaultRequestTimeoutSecMax)
                            {
                                requestTimeoutSec = n;
                                WriteToStdout($"Request Timeout Set to {requestTimeoutSec} Seconds", ConsoleColor.Green);
                                break;
                            }
                            else
                            {
                                WriteToStdout($"Request Timeout Must Be Between {DefaultRequestTimeoutSecMin} and {DefaultRequestTimeoutSecMax}", ConsoleColor.Red);
                            }
                        }

                        // Kill On Cpu Usage
                        while (true)
                        {
                            object value = await ConsoleTools.ReadValue($"Kill On Cpu Usage, Enter Percentage (Default: {DefaultKillOnCpuUsage}%)", killOnCpuUsage, typeof(float));
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
                                    WriteToStdout($"Error Parsing The Float Number!", ConsoleColor.Red);
                            }
                        }

                        // Block Port 80
                        while (true)
                        {
                            object value = await ConsoleTools.ReadValue("Block Port 80, Enter True/False (Default: True)", blockPort80, typeof(bool));
                            bool b = Convert.ToBoolean(value);

                            blockPort80 = b;
                            WriteToStdout($"Block Port 80: {blockPort80}", ConsoleColor.Green);
                            break;
                        }
                    }
                    else // Command Mode
                    {
                        // setting -Port=m -MaxRequests=m -RequestTimeoutSec=m -KillOnCpuUsage=m -BlockPort80=m

                        KeyValues keyValues = new();
                        keyValues.Add(Key.Setting.Port, true, false, typeof(int), DefaultPortMin, DefaultPortMax);
                        keyValues.Add(Key.Setting.MaxRequests, true, false, typeof(int), DefaultMaxRequestsMin, DefaultMaxRequestsMax);
                        keyValues.Add(Key.Setting.RequestTimeoutSec, true, false, typeof(int), DefaultRequestTimeoutSecMin, DefaultRequestTimeoutSecMax);
                        keyValues.Add(Key.Setting.KillOnCpuUsage, true, false, typeof(float), DefaultKillOnCpuUsageMin, DefaultKillOnCpuUsageMax);
                        keyValues.Add(Key.Setting.BlockPort80, true, false, typeof(bool));

                        bool isListOk = keyValues.GetValuesByKeys(input, out List<KeyValue> list);
                        if (!isListOk) return;
                        for (int n = 0; n < list.Count; n++)
                        {
                            KeyValue kv = list[n];
                            if (kv.Key.Equals(Key.Setting.Port)) port = kv.ValueInt;
                            if (kv.Key.Equals(Key.Setting.MaxRequests)) maxRequests = kv.ValueInt;
                            if (kv.Key.Equals(Key.Setting.RequestTimeoutSec)) requestTimeoutSec = kv.ValueInt;
                            if (kv.Key.Equals(Key.Setting.KillOnCpuUsage)) killOnCpuUsage = kv.ValueFloat;
                            if (kv.Key.Equals(Key.Setting.BlockPort80)) blockPort80 = kv.ValueBool;
                        }
                    }

                    Settings.ListenerIpAddress = IPAddress.Any;
                    Settings.ListenerPort = port;
                    Settings.MaxRequests = maxRequests;
                    Settings.RequestTimeoutSec = requestTimeoutSec;
                    Settings.KillOnCpuUsage = killOnCpuUsage;
                    Settings.BlockPort80 = blockPort80;

                    ShowSettingsMsg();
                }

                // SSLSetting
                else if (input.ToLower().StartsWith(Key.SSLSetting.Name.ToLower()))
                {
                    bool enable = DefaultSSLEnable;
                    string? rootCA_Path = null;
                    string? rootCA_KeyPath = null;
                    bool changeSniToIp = false;

                    // Interactive Mode
                    if (input.ToLower().Equals(Key.SSLSetting.Name.ToLower()))
                    {
                        // Enable
                        string msgRV = $"Enable SSL Decryption, Enter True/False (Default: {DefaultSSLEnable})";
                        while (true)
                        {
                            object value = await ConsoleTools.ReadValue(msgRV, enable, typeof(bool));
                            enable = Convert.ToBoolean(value);

                            WriteToStdout($"Enable Set to {enable}", ConsoleColor.Green);
                            break;
                        }

                        if (enable)
                        {
                            // RootCA_Path
                            bool doesUserSetPath = false;
                            while (true)
                            {
                                msgRV = $"Enter The Path Of Root Certificate (Leave Empty To Generate)";
                                object value = await ConsoleTools.ReadValue(msgRV, string.Empty, typeof(string));
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
                                        doesUserSetPath = true;
                                        break;
                                    }
                                }
                            }

                            // RootCA_KeyPath
                            if (doesUserSetPath)
                            {
                                while (true)
                                {
                                    msgRV = $"Enter The Path Of Root Private Key (Leave Empty If Certificate Contains Private Key)";
                                    object value = await ConsoleTools.ReadValue(msgRV, string.Empty, typeof(string));
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

                            // Change SNI To IP
                            msgRV = $"Change SNI To IP To Bypass DPI, Enter True/False (Default: {DefaultSSLChangeSniToIP})";
                            while (true)
                            {
                                object value = await ConsoleTools.ReadValue(msgRV, changeSniToIp, typeof(bool));
                                changeSniToIp = Convert.ToBoolean(value);

                                WriteToStdout($"ChangeSniToIP Set to {changeSniToIp}", ConsoleColor.Green);
                                break;
                            }
                        }
                    }
                    else // Command Mode
                    {
                        // sslsetting -Enable=m -RootCA_Path="" -RootCA_KeyPath="" -ChangeSniToIP=

                        KeyValues keyValues = new();
                        keyValues.Add(Key.SSLSetting.Enable, true, false, typeof(bool));
                        keyValues.Add(Key.SSLSetting.RootCA_Path, false, true, typeof(string));
                        keyValues.Add(Key.SSLSetting.RootCA_KeyPath, false, true, typeof(string));
                        keyValues.Add(Key.SSLSetting.ChangeSniToIP, false, false, typeof(bool));

                        bool isListOk = keyValues.GetValuesByKeys(input, out List<KeyValue> list);
                        if (!isListOk) return;
                        for (int n = 0; n < list.Count; n++)
                        {
                            KeyValue kv = list[n];
                            if (kv.Key.Equals(Key.SSLSetting.Enable)) enable = kv.ValueBool;
                            if (kv.Key.Equals(Key.SSLSetting.RootCA_Path)) rootCA_Path = kv.ValueString;
                            if (kv.Key.Equals(Key.SSLSetting.RootCA_KeyPath)) rootCA_KeyPath = kv.ValueString;
                            if (kv.Key.Equals(Key.SSLSetting.ChangeSniToIP)) changeSniToIp = kv.ValueBool;
                        }
                    }

                    SettingsSSL_.EnableSSL = enable;
                    SettingsSSL_.RootCA_Path = rootCA_Path;
                    SettingsSSL_.RootCA_KeyPath = rootCA_KeyPath;
                    SettingsSSL_.ChangeSniToIP = changeSniToIp;

                    await ProxyServer.EnableSSL(SettingsSSL_);

                    ShowSettingsSSLMsg();
                }

                // Programs
                else if (input.ToLower().StartsWith(Key.Programs.Name.ToLower()))
                {
                    string msg = "Available Programs:\n\n";
                    msg += $"{Key.Programs.BwList.Name}\n";
                    msg += $"{Key.Programs.Dns.Name}\n";
                    msg += $"{Key.Programs.DontBypass.Name}\n";
                    msg += $"{Key.Programs.DpiBypass.Name}\n";
                    msg += $"{Key.Programs.FakeDns.Name}\n";
                    msg += $"{Key.Programs.UpStreamProxy.Name}\n";

                    // Interactive Mode
                    if (input.ToLower().Equals(Key.Programs.Name.ToLower()))
                    {
                        WriteToStdout(msg, ConsoleColor.Cyan);

                        while (true)
                        {
                            string programName = string.Empty;
                            object valueP = await ConsoleTools.ReadValue("Enter One Of Programs Name (Default: None):", programName, typeof(string));
                            programName = valueP.ToString() ?? string.Empty;
                            if (string.IsNullOrEmpty(programName) || string.IsNullOrWhiteSpace(programName))
                            {
                                WriteToStdout($"Exited From {Key.Programs.Name}.");
                                break;
                            }

                            // BwList
                            if (programName.ToLower().Equals(Key.Programs.BwList.Name.ToLower()))
                            {
                                string msgAm = "Available Black White List Modes:\n\n";
                                msgAm += $"{Key.Programs.BwList.Mode.BlackListFile}\n";
                                msgAm += $"{Key.Programs.BwList.Mode.BlackListText}\n";
                                msgAm += $"{Key.Programs.BwList.Mode.WhiteListFile}\n";
                                msgAm += $"{Key.Programs.BwList.Mode.WhiteListText}\n";
                                msgAm += $"{Key.Programs.BwList.Mode.Disable}\n";

                                WriteToStdout(msgAm, ConsoleColor.Cyan);

                                string modeStr = Key.Programs.BwList.Mode.Disable;
                                ProxyProgram.BlackWhiteList.Mode mode = ProxyProgram.BlackWhiteList.Mode.Disable;
                                string msgRV = $"Enter One Of Modes (Default: {mode}):";
                                while (true)
                                {
                                    object value = await ConsoleTools.ReadValue(msgRV, modeStr, typeof(string));
                                    modeStr = value.ToString() ?? string.Empty;
                                    if (modeStr.ToLower().Equals(Key.Programs.BwList.Mode.BlackListFile.ToLower()))
                                        mode = ProxyProgram.BlackWhiteList.Mode.BlackListFile;
                                    else if (modeStr.ToLower().Equals(Key.Programs.BwList.Mode.BlackListText.ToLower()))
                                        mode = ProxyProgram.BlackWhiteList.Mode.BlackListText;
                                    else if (modeStr.ToLower().Equals(Key.Programs.BwList.Mode.WhiteListFile.ToLower()))
                                        mode = ProxyProgram.BlackWhiteList.Mode.WhiteListFile;
                                    else if (modeStr.ToLower().Equals(Key.Programs.BwList.Mode.WhiteListText.ToLower()))
                                        mode = ProxyProgram.BlackWhiteList.Mode.WhiteListText;
                                    else if (modeStr.ToLower().Equals(Key.Programs.BwList.Mode.Disable.ToLower()))
                                        mode = ProxyProgram.BlackWhiteList.Mode.Disable;
                                    else
                                    {
                                        WriteToStdout("Wrong Mode.", ConsoleColor.Red);
                                        continue;
                                    }
                                    break;
                                }

                                string filePathOrText = string.Empty;

                                if (mode == ProxyProgram.BlackWhiteList.Mode.BlackListFile || mode == ProxyProgram.BlackWhiteList.Mode.WhiteListFile)
                                {
                                    while (true)
                                    {
                                        msgRV = $"Enter The Path Of {mode} (Default: Cancel):";
                                        object valuePath = await ConsoleTools.ReadValue(msgRV, string.Empty, typeof(string));
                                        filePathOrText = valuePath.ToString() ?? string.Empty;
                                        if (string.IsNullOrEmpty(filePathOrText))
                                        {
                                            mode = ProxyProgram.BlackWhiteList.Mode.Disable;
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

                                if (mode == ProxyProgram.BlackWhiteList.Mode.BlackListText || mode == ProxyProgram.BlackWhiteList.Mode.WhiteListText)
                                {
                                    msgRV = $"Enter Rules Of {mode} As Text (Default: Cancel):";
                                    msgRV += "\n    e.g. Example.com\\nExample.net";
                                    object valueText = await ConsoleTools.ReadValue(msgRV, string.Empty, typeof(string));
                                    filePathOrText = valueText.ToString() ?? string.Empty;
                                    if (string.IsNullOrEmpty(filePathOrText) || string.IsNullOrWhiteSpace(filePathOrText))
                                    {
                                        mode = ProxyProgram.BlackWhiteList.Mode.Disable;
                                    }
                                    else
                                    {
                                        filePathOrText = filePathOrText.ToLower().Replace(@"\n", Environment.NewLine);
                                    }
                                }

                                BWListProgram.Set(mode, filePathOrText);
                                ProxyServer.EnableBlackWhiteList(BWListProgram);

                                ShowBwListMsg();
                            }

                            // Dns
                            else if (programName.ToLower().Equals(Key.Programs.Dns.Name.ToLower()))
                            {
                                string msgAm = $"Available {Key.Programs.Dns.Name} Modes:\n\n";
                                msgAm += $"{Key.Programs.Dns.Mode.DoH}\n";
                                msgAm += $"{Key.Programs.Dns.Mode.PlainDNS}\n";
                                msgAm += $"{Key.Programs.Dns.Mode.System}\n";
                                msgAm += $"{Key.Programs.Dns.Mode.Disable}\n";

                                WriteToStdout(msgAm, ConsoleColor.Cyan);

                                string modeStr = Key.Programs.Dns.Mode.System;
                                ProxyProgram.Dns.Mode mode = ProxyProgram.Dns.Mode.System;
                                string msgRV = $"Enter One Of Modes (Default: {mode}):";
                                while (true)
                                {
                                    object value = await ConsoleTools.ReadValue(msgRV, modeStr, typeof(string));
                                    modeStr = value.ToString() ?? string.Empty;
                                    if (modeStr.ToLower().Equals(Key.Programs.Dns.Mode.DoH.ToLower()))
                                        mode = ProxyProgram.Dns.Mode.DoH;
                                    else if (modeStr.ToLower().Equals(Key.Programs.Dns.Mode.PlainDNS.ToLower()))
                                        mode = ProxyProgram.Dns.Mode.PlainDNS;
                                    else if (modeStr.ToLower().Equals(Key.Programs.Dns.Mode.System.ToLower()))
                                        mode = ProxyProgram.Dns.Mode.System;
                                    else if (modeStr.ToLower().Equals(Key.Programs.Dns.Mode.Disable.ToLower()))
                                        mode = ProxyProgram.Dns.Mode.Disable;
                                    else
                                    {
                                        WriteToStdout("Wrong Mode.", ConsoleColor.Red);
                                        continue;
                                    }
                                    break;
                                }

                                int timeoutSec = DefaultDnsTimeoutSec;
                                string? dnsAddr = string.Empty;
                                string? dnsCleanIp = string.Empty;
                                string? proxyScheme = string.Empty;
                                bool cfEnable = false;
                                string? cfIpRange = string.Empty;
                                string cfCleanIp = string.Empty;

                                if (mode != ProxyProgram.Dns.Mode.Disable)
                                {
                                    while (true)
                                    {
                                        msgRV = $"Enter Dns Timeout In Seconds (Default: {DefaultDnsTimeoutSec} Sec):";
                                        object valueTimeout = await ConsoleTools.ReadValue(msgRV, timeoutSec, typeof(int));
                                        int timeoutSecOut = Convert.ToInt32(valueTimeout);
                                        if (timeoutSecOut < DefaultDnsTimeoutSecMin || timeoutSecOut > DefaultDnsTimeoutSecMax)
                                        {
                                            WriteToStdout($"Timeout Must Be Between {DefaultDnsTimeoutSecMin} and {DefaultDnsTimeoutSecMax} Seconds.", ConsoleColor.Red);
                                            continue;
                                        }
                                        else
                                        {
                                            timeoutSec = timeoutSecOut;
                                            break;
                                        }
                                    }

                                    if (mode == ProxyProgram.Dns.Mode.DoH)
                                    {
                                        msgRV = "Enter The DoH Address:";
                                        msgRV += "\n    e.g. https://dns.cloudflare.com/dns-query";
                                        object valueAddr = await ConsoleTools.ReadValue(msgRV, dnsAddr, typeof(string));
                                        dnsAddr = valueAddr.ToString() ?? string.Empty;
                                        if (string.IsNullOrEmpty(dnsAddr))
                                            mode = ProxyProgram.Dns.Mode.System;

                                        while (true)
                                        {
                                            if (mode != ProxyProgram.Dns.Mode.DoH) break;
                                            msgRV = $"Enter The DoH Clean IP (Optional - Press Enter To Skip):";
                                            object valueDnsIp = await ConsoleTools.ReadValue(msgRV, dnsCleanIp, typeof(string));
                                            string dnsCleanIpOut = valueDnsIp.ToString() ?? string.Empty;
                                            if (string.IsNullOrEmpty(dnsCleanIpOut)) break;
                                            bool isIp = NetworkTool.IsIp(dnsCleanIpOut, out _);
                                            if (!isIp)
                                            {
                                                WriteToStdout("This Is Not An IP Address.", ConsoleColor.Red);
                                                continue;
                                            }
                                            else
                                            {
                                                dnsCleanIp = dnsCleanIpOut;
                                                break;
                                            }
                                        }

                                        while (true)
                                        {
                                            if (mode != ProxyProgram.Dns.Mode.DoH) break;
                                            msgRV = $"Use Proxy To Bypass The DoH (Optional - Press Enter To Skip):";
                                            msgRV += "\n    e.g. http://MyProxy.com:8080";
                                            msgRV += "\n    e.g. socks5://MyProxy.com:1080";
                                            object valueProxy = await ConsoleTools.ReadValue(msgRV, proxyScheme, typeof(string));
                                            string proxySchemeOut = valueProxy.ToString() ?? string.Empty;
                                            if (string.IsNullOrEmpty(proxySchemeOut)) break;
                                            else
                                            {
                                                if (proxySchemeOut.StartsWith("http://") ||
                                                    proxySchemeOut.StartsWith("socks4://") ||
                                                    proxySchemeOut.StartsWith("socks4a://") ||
                                                    proxySchemeOut.StartsWith("socks5://"))
                                                {
                                                    proxyScheme = proxySchemeOut;
                                                    break;
                                                }
                                                else
                                                {
                                                    string notSupported = "Only the 'http://', 'socks4://', 'socks4a://' and 'socks5://' schemes are allowed for proxies.";
                                                    WriteToStdout(notSupported, ConsoleColor.Red);
                                                    continue;
                                                }
                                            }
                                        }
                                    }

                                    if (mode == ProxyProgram.Dns.Mode.PlainDNS)
                                    {
                                        msgRV = $"Enter Dns Address:";
                                        msgRV += "\n    e.g. 8.8.8.8:53 Or 1.1.1.1";
                                        object valueAddr = await ConsoleTools.ReadValue(msgRV, dnsAddr, typeof(string));
                                        dnsAddr = valueAddr.ToString() ?? string.Empty;
                                        if (string.IsNullOrEmpty(dnsAddr) || string.IsNullOrWhiteSpace(dnsAddr))
                                            mode = ProxyProgram.Dns.Mode.System;
                                    }

                                    // Cloudflare
                                    msgRV = $"Redirect All Cloudflare IPs To A Clean One (Default: False):";
                                    object valueCfEnable = await ConsoleTools.ReadValue(msgRV, cfEnable, typeof(bool));
                                    cfEnable = Convert.ToBoolean(valueCfEnable);
                                    if (cfEnable)
                                    {
                                        msgRV = $"Enter Cloudflare IP Range (Default: Built-in IPs):";
                                        msgRV += "\n    e.g. 103.21.244.0 - 103.21.244.255\\n103.22.200.0 - 103.22.200.255";
                                        object valueCfIpRange = await ConsoleTools.ReadValue(msgRV, cfIpRange, typeof(string));
                                        cfIpRange = valueCfIpRange.ToString() ?? string.Empty;
                                        if (!string.IsNullOrEmpty(cfIpRange))
                                            cfIpRange = cfIpRange.Replace(@"\n", Environment.NewLine);

                                        msgRV = $"Enter Cloudflare Clean IP (Default: Cancel):";
                                        object valueCfCleanIp = await ConsoleTools.ReadValue(msgRV, string.Empty, typeof(string));
                                        cfCleanIp = valueCfCleanIp.ToString() ?? string.Empty;
                                        if (string.IsNullOrEmpty(cfCleanIp)) cfEnable = false;
                                    }
                                }

                                DnsProgram.Set(mode, dnsAddr, dnsCleanIp, timeoutSec, proxyScheme);
                                if (cfEnable)
                                {
                                    DnsProgram.SetCloudflareIPs(cfCleanIp, cfIpRange);
                                    DnsProgram.ChangeCloudflareIP = true;
                                }
                                else
                                    DnsProgram.ChangeCloudflareIP = false;

                                ProxyServer.EnableDNS(DnsProgram);

                                ShowDnsMsg();
                            }

                            // Dont Bypass
                            else if (programName.ToLower().Equals(Key.Programs.DontBypass.Name.ToLower()))
                            {
                                string msgAm = $"Available {Key.Programs.DontBypass.Name} Modes:\n\n";
                                msgAm += $"{Key.Programs.DontBypass.Mode.File}\n";
                                msgAm += $"{Key.Programs.DontBypass.Mode.Text}\n";
                                msgAm += $"{Key.Programs.DontBypass.Mode.Disable}\n";

                                WriteToStdout(msgAm, ConsoleColor.Cyan);

                                string modeStr = Key.Programs.DontBypass.Mode.Disable;
                                ProxyProgram.DontBypass.Mode mode = ProxyProgram.DontBypass.Mode.Disable;
                                string msgRV = $"Enter One Of Modes (Default: {mode}):";
                                while (true)
                                {
                                    object value = await ConsoleTools.ReadValue(msgRV, modeStr, typeof(string));
                                    modeStr = value.ToString() ?? string.Empty;
                                    if (modeStr.ToLower().Equals(Key.Programs.DontBypass.Mode.File.ToLower()))
                                        mode = ProxyProgram.DontBypass.Mode.File;
                                    else if (modeStr.ToLower().Equals(Key.Programs.DontBypass.Mode.Text.ToLower()))
                                        mode = ProxyProgram.DontBypass.Mode.Text;
                                    else if (modeStr.ToLower().Equals(Key.Programs.DontBypass.Mode.Disable.ToLower()))
                                        mode = ProxyProgram.DontBypass.Mode.Disable;
                                    else
                                    {
                                        WriteToStdout("Wrong Mode.", ConsoleColor.Red);
                                        continue;
                                    }
                                    break;
                                }

                                string filePathOrText = string.Empty;

                                if (mode == ProxyProgram.DontBypass.Mode.File)
                                {
                                    while (true)
                                    {
                                        msgRV = $"Enter The Path Of {mode} (Default: Cancel):";
                                        object valuePath = await ConsoleTools.ReadValue(msgRV, string.Empty, typeof(string));
                                        filePathOrText = valuePath.ToString() ?? string.Empty;
                                        if (string.IsNullOrEmpty(filePathOrText))
                                        {
                                            mode = ProxyProgram.DontBypass.Mode.Disable;
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

                                if (mode == ProxyProgram.DontBypass.Mode.Text)
                                {
                                    msgRV = $"Enter Rules Of {Key.Programs.DontBypass.Name} As Text (Default: Cancel):";
                                    msgRV += "\n    e.g. Example.com\\nExample.net";
                                    object valueText = await ConsoleTools.ReadValue(msgRV, string.Empty, typeof(string));
                                    filePathOrText = valueText.ToString() ?? string.Empty;
                                    if (string.IsNullOrEmpty(filePathOrText) || string.IsNullOrWhiteSpace(filePathOrText))
                                    {
                                        mode = ProxyProgram.DontBypass.Mode.Disable;
                                    }
                                    else
                                    {
                                        filePathOrText = filePathOrText.ToLower().Replace(@"\n", Environment.NewLine);
                                    }
                                }

                                DontBypassProgram.Set(mode, filePathOrText);
                                ProxyServer.EnableDontBypass(DontBypassProgram);

                                ShowDontBypassMsg();
                            }

                            // DPI Bypass
                            else if (programName.ToLower().Equals(Key.Programs.DpiBypass.Name.ToLower()))
                            {
                                string msgAm = $"Available {Key.Programs.DpiBypass.Name} Modes:\n\n";
                                msgAm += $"{Key.Programs.DpiBypass.Mode.Program.Name}\n";
                                msgAm += $"{Key.Programs.DpiBypass.Mode.Disable}\n";

                                WriteToStdout(msgAm, ConsoleColor.Cyan);

                                string modeStr = Key.Programs.DpiBypass.Mode.Disable;
                                ProxyProgram.DPIBypass.Mode mode = ProxyProgram.DPIBypass.Mode.Disable;
                                string msgRV = $"Enter One Of Modes (Default: {mode}):";
                                while (true)
                                {
                                    object value = await ConsoleTools.ReadValue(msgRV, modeStr, typeof(string));
                                    modeStr = value.ToString() ?? string.Empty;
                                    if (modeStr.ToLower().Equals(Key.Programs.DpiBypass.Mode.Program.Name.ToLower()))
                                        mode = ProxyProgram.DPIBypass.Mode.Program;
                                    else if (modeStr.ToLower().Equals(Key.Programs.DpiBypass.Mode.Disable.ToLower()))
                                        mode = ProxyProgram.DPIBypass.Mode.Disable;
                                    else
                                    {
                                        WriteToStdout("Wrong Mode.", ConsoleColor.Red);
                                        continue;
                                    }
                                    break;
                                }


                                int beforeSniChunks = DefaultDPIBypassBeforeSniChunks;
                                ProxyProgram.DPIBypass.ChunkMode chunkMode = DefaultDPIBypassChunkMode;
                                int sniChunks = DefaultDPIBypassSniChunks;
                                int antiPatternOffset = DefaultDPIBypassAntiPatternOffset;
                                int fragmentDelay = DefaultDPIBypassFragmentDelay;

                                if (mode == ProxyProgram.DPIBypass.Mode.Program)
                                {
                                    // Get Before Sni Chunks
                                    msgRV = $"Enter Number Of Chunks Before SNI (Default: {DefaultDPIBypassBeforeSniChunks}):";
                                    while (true)
                                    {
                                        object value = await ConsoleTools.ReadValue(msgRV, beforeSniChunks, typeof(int));
                                        int n = Convert.ToInt32(value);
                                        if (n >= DefaultDPIBypassBeforeSniChunksMin && n <= DefaultDPIBypassBeforeSniChunksMax)
                                        {
                                            beforeSniChunks = n;
                                            break;
                                        }
                                        else
                                        {
                                            WriteToStdout($"Chunks Number Must Be Between {DefaultDPIBypassBeforeSniChunksMin} and {DefaultDPIBypassBeforeSniChunksMax}", ConsoleColor.Red);
                                            continue;
                                        }
                                    }

                                    // Get Chunk Mode
                                    msgAm = $"Available {Key.Programs.DpiBypass.Mode.Program.ChunkMode.Name} Modes:\n\n";
                                    msgAm += $"{Key.Programs.DpiBypass.Mode.Program.ChunkMode.SNI}\n";
                                    msgAm += $"{Key.Programs.DpiBypass.Mode.Program.ChunkMode.SniExtension}\n";
                                    msgAm += $"{Key.Programs.DpiBypass.Mode.Program.ChunkMode.AllExtensions}\n";

                                    WriteToStdout(msgAm, ConsoleColor.Cyan);

                                    modeStr = Key.Programs.DpiBypass.Mode.Program.ChunkMode.SNI;
                                    msgRV = $"Enter One Of Modes (Default: {chunkMode}):";
                                    while (true)
                                    {
                                        object value = await ConsoleTools.ReadValue(msgRV, modeStr, typeof(string));
                                        modeStr = value.ToString() ?? string.Empty;
                                        if (modeStr.ToLower().Equals(Key.Programs.DpiBypass.Mode.Program.ChunkMode.SNI.ToLower()))
                                            chunkMode = ProxyProgram.DPIBypass.ChunkMode.SNI;
                                        else if (modeStr.ToLower().Equals(Key.Programs.DpiBypass.Mode.Program.ChunkMode.SniExtension.ToLower()))
                                            chunkMode = ProxyProgram.DPIBypass.ChunkMode.SniExtension;
                                        else if (modeStr.ToLower().Equals(Key.Programs.DpiBypass.Mode.Program.ChunkMode.AllExtensions.ToLower()))
                                            chunkMode = ProxyProgram.DPIBypass.ChunkMode.AllExtensions;
                                        else
                                        {
                                            WriteToStdout("Wrong Mode.", ConsoleColor.Red);
                                            continue;
                                        }
                                        break;
                                    }

                                    // Get Sni Chunks
                                    msgRV = $"Enter Number Of \"{chunkMode}\" Chunks (Default: {DefaultDPIBypassSniChunks}):";
                                    while (true)
                                    {
                                        object value = await ConsoleTools.ReadValue(msgRV, sniChunks, typeof(int));
                                        int n = Convert.ToInt32(value);
                                        if (n >= DefaultDPIBypassSniChunksMin && n <= DefaultDPIBypassSniChunksMax)
                                        {
                                            sniChunks = n;
                                            break;
                                        }
                                        else
                                        {
                                            WriteToStdout($"Chunks Number Must Be Between {DefaultDPIBypassSniChunksMin} and {DefaultDPIBypassSniChunksMax}", ConsoleColor.Red);
                                            continue;
                                        }
                                    }

                                    // Get Anti-Pattern Offset
                                    msgRV = $"Enter Number Of Anti-Pattern Offset (Default: {DefaultDPIBypassAntiPatternOffset}):";
                                    while (true)
                                    {
                                        object value = await ConsoleTools.ReadValue(msgRV, antiPatternOffset, typeof(int));
                                        int n = Convert.ToInt32(value);
                                        if (n >= DefaultDPIBypassAntiPatternOffsetMin && n <= DefaultDPIBypassAntiPatternOffsetMax)
                                        {
                                            antiPatternOffset = n;
                                            break;
                                        }
                                        else
                                        {
                                            WriteToStdout($"Chunks Number Must Be Between {DefaultDPIBypassAntiPatternOffsetMin} and {DefaultDPIBypassAntiPatternOffsetMin}", ConsoleColor.Red);
                                            continue;
                                        }
                                    }

                                    // Get Fragment Delay
                                    msgRV = $"Enter Milliseconds Of Fragment Delay (Default: {DefaultDPIBypassFragmentDelay}):";
                                    while (true)
                                    {
                                        object value = await ConsoleTools.ReadValue(msgRV, fragmentDelay, typeof(int));
                                        int n = Convert.ToInt32(value);
                                        if (n >= DefaultDPIBypassFragmentDelayMin && n <= DefaultDPIBypassFragmentDelayMax)
                                        {
                                            fragmentDelay = n;
                                            break;
                                        }
                                        else
                                        {
                                            WriteToStdout($"Fragment Delay Must Be Between {DefaultDPIBypassFragmentDelayMin} and {DefaultDPIBypassFragmentDelayMax} Milliseconds", ConsoleColor.Red);
                                            continue;
                                        }
                                    }
                                }

                                DpiBypassStaticProgram.Set(mode, beforeSniChunks, chunkMode, sniChunks, antiPatternOffset, fragmentDelay);
                                ProxyServer.EnableStaticDPIBypass(DpiBypassStaticProgram);

                                ShowDpiBypassMsg();
                            }

                            // Fake Dns
                            else if (programName.ToLower().Equals(Key.Programs.FakeDns.Name.ToLower()))
                            {
                                string msgAm = $"Available {Key.Programs.FakeDns.Name} Modes:\n\n";
                                msgAm += $"{Key.Programs.FakeDns.Mode.File}\n";
                                msgAm += $"{Key.Programs.FakeDns.Mode.Text}\n";
                                msgAm += $"{Key.Programs.FakeDns.Mode.Disable}\n";

                                WriteToStdout(msgAm, ConsoleColor.Cyan);

                                string modeStr = Key.Programs.FakeDns.Mode.Disable;
                                ProxyProgram.FakeDns.Mode mode = ProxyProgram.FakeDns.Mode.Disable;
                                string msgRV = $"Enter One Of Modes (Default: {mode}):";
                                while (true)
                                {
                                    object value = await ConsoleTools.ReadValue(msgRV, modeStr, typeof(string));
                                    modeStr = value.ToString() ?? string.Empty;
                                    if (modeStr.ToLower().Equals(Key.Programs.FakeDns.Mode.File.ToLower()))
                                        mode = ProxyProgram.FakeDns.Mode.File;
                                    else if (modeStr.ToLower().Equals(Key.Programs.FakeDns.Mode.Text.ToLower()))
                                        mode = ProxyProgram.FakeDns.Mode.Text;
                                    else if (modeStr.ToLower().Equals(Key.Programs.FakeDns.Mode.Disable.ToLower()))
                                        mode = ProxyProgram.FakeDns.Mode.Disable;
                                    else
                                    {
                                        WriteToStdout("Wrong Mode.", ConsoleColor.Red);
                                        continue;
                                    }
                                    break;
                                }

                                string filePathOrText = string.Empty;

                                if (mode == ProxyProgram.FakeDns.Mode.File)
                                {
                                    while (true)
                                    {
                                        msgRV = $"Enter The Path Of {mode} (Default: Cancel):";
                                        object valuePath = await ConsoleTools.ReadValue(msgRV, string.Empty, typeof(string));
                                        filePathOrText = valuePath.ToString() ?? string.Empty;
                                        if (string.IsNullOrEmpty(filePathOrText))
                                        {
                                            mode = ProxyProgram.FakeDns.Mode.Disable;
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

                                if (mode == ProxyProgram.FakeDns.Mode.Text)
                                {
                                    msgRV = $"Enter Rules Of {Key.Programs.FakeDns.Name} As Text (Default: Cancel):";
                                    msgRV += "\n    e.g. Google.com|8.8.8.8\\nCloudflare.com|1.1.1.1";
                                    object valueText = await ConsoleTools.ReadValue(msgRV, string.Empty, typeof(string));
                                    filePathOrText = valueText.ToString() ?? string.Empty;
                                    if (string.IsNullOrEmpty(filePathOrText))
                                    {
                                        mode = ProxyProgram.FakeDns.Mode.Disable;
                                    }
                                    else
                                    {
                                        filePathOrText = filePathOrText.ToLower().Replace(@"\n", Environment.NewLine);
                                    }
                                }

                                FakeDnsProgram.Set(mode, filePathOrText);
                                ProxyServer.EnableFakeDNS(FakeDnsProgram);

                                ShowFakeDnsMsg();
                            }

                            // UpStream Proxy
                            else if (programName.ToLower().Equals(Key.Programs.UpStreamProxy.Name.ToLower()))
                            {
                                string msgAm = $"Available {Key.Programs.UpStreamProxy.Name} Modes:\n\n";
                                msgAm += $"{Key.Programs.UpStreamProxy.Mode.HTTP}\n";
                                msgAm += $"{Key.Programs.UpStreamProxy.Mode.SOCKS5}\n";
                                msgAm += $"{Key.Programs.UpStreamProxy.Mode.Disable}\n";

                                WriteToStdout(msgAm, ConsoleColor.Cyan);

                                string modeStr = Key.Programs.UpStreamProxy.Mode.Disable;
                                ProxyProgram.UpStreamProxy.Mode mode = ProxyProgram.UpStreamProxy.Mode.Disable;
                                string msgRV = $"Enter One Of Modes (Default: {mode}):";
                                while (true)
                                {
                                    object value = await ConsoleTools.ReadValue(msgRV, modeStr, typeof(string));
                                    modeStr = value.ToString() ?? string.Empty;
                                    if (modeStr.ToLower().Equals(Key.Programs.UpStreamProxy.Mode.HTTP.ToLower()))
                                        mode = ProxyProgram.UpStreamProxy.Mode.HTTP;
                                    else if (modeStr.ToLower().Equals(Key.Programs.UpStreamProxy.Mode.SOCKS5.ToLower()))
                                        mode = ProxyProgram.UpStreamProxy.Mode.SOCKS5;
                                    else if (modeStr.ToLower().Equals(Key.Programs.UpStreamProxy.Mode.Disable.ToLower()))
                                        mode = ProxyProgram.UpStreamProxy.Mode.Disable;
                                    else
                                    {
                                        WriteToStdout("Wrong Mode.", ConsoleColor.Red);
                                        continue;
                                    }
                                    break;
                                }

                                string proxyHost = string.Empty;
                                int proxyPort = 0;
                                bool onlyApplyToBlockedIPs = DefaultUpStreamProxyOnlyApplyToBlockedIPs;

                                if (mode != ProxyProgram.UpStreamProxy.Mode.Disable)
                                {
                                    // Get Proxy Host Or IP
                                    msgRV = $"Enter Proxy Host Or IP (Default: Cancel):";
                                    object valueHost = await ConsoleTools.ReadValue(msgRV, string.Empty, typeof(string));
                                    proxyHost = valueHost.ToString() ?? string.Empty;
                                    if (string.IsNullOrEmpty(proxyHost))
                                        mode = ProxyProgram.UpStreamProxy.Mode.Disable;

                                    // Get Proxy Port If Host Is Not Empty
                                    msgRV = $"Enter Proxy Port:";
                                    while (true)
                                    {
                                        if (mode == ProxyProgram.UpStreamProxy.Mode.Disable) break;
                                        object value = await ConsoleTools.ReadValue(msgRV, proxyPort, typeof(int));
                                        int n = Convert.ToInt32(value);
                                        if (n >= DefaultUpStreamProxyPortMin && n <= DefaultUpStreamProxyPortMax)
                                        {
                                            proxyPort = n;
                                            break;
                                        }
                                        else
                                        {
                                            WriteToStdout($"Port Number Must Be Between {DefaultUpStreamProxyPortMin} and {DefaultUpStreamProxyPortMax}", ConsoleColor.Red);
                                            continue;
                                        }
                                    }

                                    // Get Only Apply To Blocked IPs
                                    msgRV = $"Apply Upstream Proxy Only To Blocked IPs (Default: {DefaultUpStreamProxyOnlyApplyToBlockedIPs}):";
                                    while (true)
                                    {
                                        if (mode == ProxyProgram.UpStreamProxy.Mode.Disable) break;
                                        object value = await ConsoleTools.ReadValue(msgRV, onlyApplyToBlockedIPs, typeof(bool));
                                        onlyApplyToBlockedIPs = Convert.ToBoolean(value);
                                        break;
                                    }
                                }

                                UpStreamProxyProgram.Set(mode, proxyHost, proxyPort, onlyApplyToBlockedIPs);
                                ProxyServer.EnableUpStreamProxy(UpStreamProxyProgram);

                                ShowUpStreamProxyMsg();
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

                        // BwList
                        if (input.ToLower().StartsWith($"{Key.Programs.Name.ToLower()} {Key.Programs.BwList.Name.ToLower()}"))
                        {
                            // Programs BwList -Mode=m -PathOrText="m"

                            // Get ModeStr
                            string modeStr = Key.Programs.BwList.Mode.Disable;
                            string key = Key.Programs.BwList.Mode.Name;
                            isValueOk = ConsoleTools.GetValueByKey(input, key, true, false, out string value);
                            if (!isValueOk) return;

                            KeyValues modes = new();
                            modes.Add(Key.Programs.BwList.Mode.BlackListFile, true, false, typeof(string));
                            modes.Add(Key.Programs.BwList.Mode.BlackListText, true, false, typeof(string));
                            modes.Add(Key.Programs.BwList.Mode.WhiteListFile, true, false, typeof(string));
                            modes.Add(Key.Programs.BwList.Mode.WhiteListText, true, false, typeof(string));
                            modes.Add(Key.Programs.BwList.Mode.Disable, true, false, typeof(string));

                            isValueOk = ConsoleTools.GetString(key, value, true, modes, out value);
                            if (!isValueOk) return;
                            modeStr = value;

                            // Get -Mode
                            ProxyProgram.BlackWhiteList.Mode mode = ProxyProgram.BlackWhiteList.Mode.Disable;
                            if (modeStr.ToLower().Equals(Key.Programs.BwList.Mode.BlackListFile.ToLower()))
                                mode = ProxyProgram.BlackWhiteList.Mode.BlackListFile;
                            else if (modeStr.ToLower().Equals(Key.Programs.BwList.Mode.BlackListText.ToLower()))
                                mode = ProxyProgram.BlackWhiteList.Mode.BlackListText;
                            else if (modeStr.ToLower().Equals(Key.Programs.BwList.Mode.WhiteListFile.ToLower()))
                                mode = ProxyProgram.BlackWhiteList.Mode.WhiteListFile;
                            else if (modeStr.ToLower().Equals(Key.Programs.BwList.Mode.WhiteListText.ToLower()))
                                mode = ProxyProgram.BlackWhiteList.Mode.WhiteListText;
                            else if (modeStr.ToLower().Equals(Key.Programs.BwList.Mode.Disable.ToLower()))
                                mode = ProxyProgram.BlackWhiteList.Mode.Disable;

                            // Get -PathOrText
                            string pathOrText = string.Empty;
                            if (mode != ProxyProgram.BlackWhiteList.Mode.Disable)
                            {
                                key = Key.Programs.BwList.PathOrText;

                                isValueOk = ConsoleTools.GetValueByKey(input, key, true, true, out value);
                                if (!isValueOk) return;
                                isValueOk = ConsoleTools.GetString(key, value, true, out value);
                                if (!isValueOk) return;
                                pathOrText = value;
                            }

                            if (mode == ProxyProgram.BlackWhiteList.Mode.BlackListFile || mode == ProxyProgram.BlackWhiteList.Mode.WhiteListFile)
                            {
                                pathOrText = Path.GetFullPath(pathOrText);
                                if (!File.Exists(pathOrText))
                                {
                                    string msgNotExist = $"{pathOrText}\nFile Not Exist.";
                                    WriteToStdout(msgNotExist, ConsoleColor.Red);
                                    return;
                                }
                            }

                            if (mode == ProxyProgram.BlackWhiteList.Mode.BlackListText || mode == ProxyProgram.BlackWhiteList.Mode.WhiteListText)
                            {
                                pathOrText = pathOrText.ToLower().Replace(@"\n", Environment.NewLine);
                            }

                            BWListProgram.Set(mode, pathOrText);
                            ProxyServer.EnableBlackWhiteList(BWListProgram);

                            ShowBwListMsg();
                        }

                        // Dns
                        if (input.ToLower().StartsWith($"{Key.Programs.Name.ToLower()} {Key.Programs.Dns.Name.ToLower()}"))
                        {
                            // Programs Dns -Mode=m -TimeoutSec=m -DnsAddr=m -DnsCleanIp= -ProxyScheme= -CfCleanIp= -CfIpRange=""

                            // Get ModeStr
                            string modeStr = Key.Programs.Dns.Mode.System;
                            string key = Key.Programs.Dns.Mode.Name;
                            isValueOk = ConsoleTools.GetValueByKey(input, key, true, false, out string value);
                            if (!isValueOk) return;

                            KeyValues modes = new();
                            modes.Add(Key.Programs.Dns.Mode.DoH, true, false, typeof(string));
                            modes.Add(Key.Programs.Dns.Mode.PlainDNS, true, false, typeof(string));
                            modes.Add(Key.Programs.Dns.Mode.System, true, false, typeof(string));
                            modes.Add(Key.Programs.Dns.Mode.Disable, true, false, typeof(string));

                            isValueOk = ConsoleTools.GetString(key, value, true, modes, out value);
                            if (!isValueOk) return;
                            modeStr = value;

                            // Get -Mode
                            ProxyProgram.Dns.Mode mode = ProxyProgram.Dns.Mode.System;
                            if (modeStr.ToLower().Equals(Key.Programs.Dns.Mode.DoH.ToLower()))
                                mode = ProxyProgram.Dns.Mode.DoH;
                            else if (modeStr.ToLower().Equals(Key.Programs.Dns.Mode.PlainDNS.ToLower()))
                                mode = ProxyProgram.Dns.Mode.PlainDNS;
                            else if (modeStr.ToLower().Equals(Key.Programs.Dns.Mode.System.ToLower()))
                                mode = ProxyProgram.Dns.Mode.System;
                            else if (modeStr.ToLower().Equals(Key.Programs.Dns.Mode.Disable.ToLower()))
                                mode = ProxyProgram.Dns.Mode.Disable;

                            int timeoutSec = DefaultDnsTimeoutSec;
                            string? dnsAddr = string.Empty;
                            string? dnsCleanIp = string.Empty;
                            string? proxyScheme = string.Empty;
                            string? cfIpRange = string.Empty;
                            string cfCleanIp = string.Empty;

                            // Get The Rest
                            if (mode != ProxyProgram.Dns.Mode.Disable)
                            {
                                KeyValues keyValues = new();
                                keyValues.Add(Key.Programs.Dns.TimeoutSec, true, false, typeof(int), DefaultDnsTimeoutSecMin, DefaultDnsTimeoutSecMax);
                                if (mode == ProxyProgram.Dns.Mode.System)
                                    keyValues.Add(Key.Programs.Dns.DnsAddr, false, false, typeof(string));
                                else
                                    keyValues.Add(Key.Programs.Dns.DnsAddr, true, false, typeof(string));
                                keyValues.Add(Key.Programs.Dns.DnsCleanIp, false, false, typeof(string));
                                keyValues.Add(Key.Programs.Dns.ProxyScheme, false, false, typeof(string));
                                keyValues.Add(Key.Programs.Dns.CfCleanIp, false, false, typeof(string));
                                keyValues.Add(Key.Programs.Dns.CfIpRange, false, true, typeof(string));

                                bool isListOk = keyValues.GetValuesByKeys(input, out List<KeyValue> list);
                                if (!isListOk) return;
                                for (int n = 0; n < list.Count; n++)
                                {
                                    KeyValue kv = list[n];
                                    if (kv.Key.Equals(Key.Programs.Dns.TimeoutSec)) timeoutSec = kv.ValueInt;
                                    if (kv.Key.Equals(Key.Programs.Dns.DnsAddr)) dnsAddr = kv.ValueString;
                                    if (kv.Key.Equals(Key.Programs.Dns.DnsCleanIp)) dnsCleanIp = kv.ValueString;
                                    if (kv.Key.Equals(Key.Programs.Dns.ProxyScheme)) proxyScheme = kv.ValueString;
                                    if (kv.Key.Equals(Key.Programs.Dns.CfCleanIp)) cfCleanIp = kv.ValueString;
                                    if (kv.Key.Equals(Key.Programs.Dns.CfIpRange)) cfIpRange = kv.ValueString;
                                }

                                if (!string.IsNullOrEmpty(cfIpRange))
                                    cfIpRange = cfIpRange.Replace(@"\n", Environment.NewLine);
                            }

                            DnsProgram.Set(mode, dnsAddr, dnsCleanIp, timeoutSec, proxyScheme);
                            if (!string.IsNullOrEmpty(cfCleanIp))
                            {
                                DnsProgram.SetCloudflareIPs(cfCleanIp, cfIpRange);
                                DnsProgram.ChangeCloudflareIP = true;
                            }
                            else
                                DnsProgram.ChangeCloudflareIP = false;

                            ProxyServer.EnableDNS(DnsProgram);

                            ShowDnsMsg();
                        }

                        // DontBypass
                        if (input.ToLower().StartsWith($"{Key.Programs.Name.ToLower()} {Key.Programs.DontBypass.Name.ToLower()}"))
                        {
                            // Programs DontBypass -Mode=m -PathOrText="m"

                            // Get ModeStr
                            string modeStr = Key.Programs.DontBypass.Mode.Disable;
                            string key = Key.Programs.DontBypass.Mode.Name;
                            isValueOk = ConsoleTools.GetValueByKey(input, key, true, false, out string value);
                            if (!isValueOk) return;

                            KeyValues modes = new();
                            modes.Add(Key.Programs.DontBypass.Mode.File, true, false, typeof(string));
                            modes.Add(Key.Programs.DontBypass.Mode.Text, true, false, typeof(string));
                            modes.Add(Key.Programs.DontBypass.Mode.Disable, true, false, typeof(string));

                            isValueOk = ConsoleTools.GetString(key, value, true, modes, out value);
                            if (!isValueOk) return;
                            modeStr = value;

                            // Get -Mode
                            ProxyProgram.DontBypass.Mode mode = ProxyProgram.DontBypass.Mode.Disable;
                            if (modeStr.ToLower().Equals(Key.Programs.DontBypass.Mode.File.ToLower()))
                                mode = ProxyProgram.DontBypass.Mode.File;
                            else if (modeStr.ToLower().Equals(Key.Programs.DontBypass.Mode.Text.ToLower()))
                                mode = ProxyProgram.DontBypass.Mode.Text;
                            else if (modeStr.ToLower().Equals(Key.Programs.DontBypass.Mode.Disable.ToLower()))
                                mode = ProxyProgram.DontBypass.Mode.Disable;

                            // Get -PathOrText
                            string pathOrText = string.Empty;
                            key = Key.Programs.DontBypass.PathOrText;
                            if (mode != ProxyProgram.DontBypass.Mode.Disable)
                            {
                                isValueOk = ConsoleTools.GetValueByKey(input, key, true, true, out value);
                                if (!isValueOk) return;
                                isValueOk = ConsoleTools.GetString(key, value, true, out value);
                                if (!isValueOk) return;
                                pathOrText = value;
                            }

                            if (mode == ProxyProgram.DontBypass.Mode.File)
                            {
                                pathOrText = Path.GetFullPath(pathOrText);
                                if (!File.Exists(pathOrText))
                                {
                                    string msgNotExist = $"{pathOrText}\nFile Not Exist.";
                                    WriteToStdout(msgNotExist, ConsoleColor.Red);
                                    return;
                                }
                            }

                            if (mode == ProxyProgram.DontBypass.Mode.Text)
                            {
                                pathOrText = pathOrText.ToLower().Replace(@"\n", Environment.NewLine);
                            }

                            DontBypassProgram.Set(mode, pathOrText);
                            ProxyServer.EnableDontBypass(DontBypassProgram);

                            ShowDontBypassMsg();
                        }

                        // DpiBypass
                        if (input.ToLower().StartsWith($"{Key.Programs.Name.ToLower()} {Key.Programs.DpiBypass.Name.ToLower()}"))
                        {
                            // Programs DpiBypass -Mode=m -BeforeSniChunks=m -ChunkMode=m -SniChunks=m -AntiPatternOffset=m -FragmentDelay=m

                            // Get ModeStr
                            string modeStr = Key.Programs.DpiBypass.Mode.Disable;
                            string key = Key.Programs.DpiBypass.Mode.Name;
                            isValueOk = ConsoleTools.GetValueByKey(input, key, true, false, out string value);
                            if (!isValueOk) return;

                            KeyValues modes = new();
                            modes.Add(Key.Programs.DpiBypass.Mode.Program.Name, true, false, typeof(string));
                            modes.Add(Key.Programs.DpiBypass.Mode.Disable, true, false, typeof(string));

                            isValueOk = ConsoleTools.GetString(key, value, true, modes, out value);
                            if (!isValueOk) return;
                            modeStr = value;

                            // Get -Mode
                            ProxyProgram.DPIBypass.Mode mode = ProxyProgram.DPIBypass.Mode.Disable;
                            if (modeStr.ToLower().Equals(Key.Programs.DpiBypass.Mode.Program.Name.ToLower()))
                                mode = ProxyProgram.DPIBypass.Mode.Program;
                            else if (modeStr.ToLower().Equals(Key.Programs.DpiBypass.Mode.Disable.ToLower()))
                                mode = ProxyProgram.DPIBypass.Mode.Disable;

                            int beforeSniChunks = DefaultDPIBypassBeforeSniChunks;
                            string chunkModeStr = DefaultDPIBypassChunkModeStr;
                            int sniChunks = DefaultDPIBypassSniChunks;
                            int antiPatternOffset = DefaultDPIBypassAntiPatternOffset;
                            int fragmentDelay = DefaultDPIBypassFragmentDelay;

                            // Get The Rest
                            if (mode == ProxyProgram.DPIBypass.Mode.Program)
                            {
                                KeyValues keyValues = new();
                                keyValues.Add(Key.Programs.DpiBypass.Mode.Program.BeforeSniChunks, true, false, typeof(int), DefaultDPIBypassBeforeSniChunksMin, DefaultDPIBypassBeforeSniChunksMax);
                                keyValues.Add(Key.Programs.DpiBypass.Mode.Program.ChunkMode.Name, true, false, typeof(string));
                                keyValues.Add(Key.Programs.DpiBypass.Mode.Program.SniChunks, true, false, typeof(int), DefaultDPIBypassSniChunksMin, DefaultDPIBypassSniChunksMax);
                                keyValues.Add(Key.Programs.DpiBypass.Mode.Program.AntiPatternOffset, true, false, typeof(int), DefaultDPIBypassAntiPatternOffsetMin, DefaultDPIBypassAntiPatternOffsetMax);
                                keyValues.Add(Key.Programs.DpiBypass.Mode.Program.FragmentDelay, true, false, typeof(int), DefaultDPIBypassFragmentDelayMin, DefaultDPIBypassFragmentDelayMax);

                                bool isListOk = keyValues.GetValuesByKeys(input, out List<KeyValue> list);

                                bool isChunkModeOk = false;

                                for (int n = 0; n < list.Count; n++)
                                {
                                    KeyValue kv = list[n];
                                    if (kv.Key.Equals(Key.Programs.DpiBypass.Mode.Program.BeforeSniChunks)) beforeSniChunks = kv.ValueInt;
                                    if (kv.Key.Equals(Key.Programs.DpiBypass.Mode.Program.ChunkMode.Name))
                                    {
                                        chunkModeStr = kv.ValueString;

                                        KeyValues chunkModes = new();
                                        chunkModes.Add(Key.Programs.DpiBypass.Mode.Program.ChunkMode.SNI, true, false, typeof(string));
                                        chunkModes.Add(Key.Programs.DpiBypass.Mode.Program.ChunkMode.SniExtension, true, false, typeof(string));
                                        chunkModes.Add(Key.Programs.DpiBypass.Mode.Program.ChunkMode.AllExtensions, true, false, typeof(string));

                                        string chunkKey = Key.Programs.DpiBypass.Mode.Program.ChunkMode.Name;
                                        isChunkModeOk = ConsoleTools.GetString(chunkKey, chunkModeStr, true, chunkModes, out chunkModeStr);
                                        if (!isChunkModeOk) break;
                                    }
                                    if (kv.Key.Equals(Key.Programs.DpiBypass.Mode.Program.SniChunks)) sniChunks = kv.ValueInt;
                                    if (kv.Key.Equals(Key.Programs.DpiBypass.Mode.Program.AntiPatternOffset)) antiPatternOffset = kv.ValueInt;
                                    if (kv.Key.Equals(Key.Programs.DpiBypass.Mode.Program.FragmentDelay)) fragmentDelay = kv.ValueInt;
                                }

                                if (!isChunkModeOk) return;
                                if (!isListOk) return;
                            }

                            // Get Chunk Mode
                            ProxyProgram.DPIBypass.ChunkMode chunkMode = ProxyProgram.DPIBypass.ChunkMode.SNI;
                            if (chunkModeStr.ToLower().Equals(Key.Programs.DpiBypass.Mode.Program.ChunkMode.SNI.ToLower()))
                                chunkMode = ProxyProgram.DPIBypass.ChunkMode.SNI;
                            else if (chunkModeStr.ToLower().Equals(Key.Programs.DpiBypass.Mode.Program.ChunkMode.SniExtension.ToLower()))
                                chunkMode = ProxyProgram.DPIBypass.ChunkMode.SniExtension;
                            else if (chunkModeStr.ToLower().Equals(Key.Programs.DpiBypass.Mode.Program.ChunkMode.AllExtensions.ToLower()))
                                chunkMode = ProxyProgram.DPIBypass.ChunkMode.AllExtensions;

                            DpiBypassStaticProgram.Set(mode, beforeSniChunks, chunkMode, sniChunks, antiPatternOffset, fragmentDelay);
                            ProxyServer.EnableStaticDPIBypass(DpiBypassStaticProgram);

                            ShowDpiBypassMsg();
                        }

                        // FakeDns
                        if (input.ToLower().StartsWith($"{Key.Programs.Name.ToLower()} {Key.Programs.FakeDns.Name.ToLower()}"))
                        {
                            // Programs FakeDns -Mode=m -PathOrText="m"

                            // Get ModeStr
                            string modeStr = Key.Programs.FakeDns.Mode.Disable;
                            string key = Key.Programs.FakeDns.Mode.Name;
                            isValueOk = ConsoleTools.GetValueByKey(input, key, true, false, out string value);
                            if (!isValueOk) return;

                            KeyValues modes = new();
                            modes.Add(Key.Programs.FakeDns.Mode.File, true, false, typeof(string));
                            modes.Add(Key.Programs.FakeDns.Mode.Text, true, false, typeof(string));
                            modes.Add(Key.Programs.FakeDns.Mode.Disable, true, false, typeof(string));

                            isValueOk = ConsoleTools.GetString(key, value, true, modes, out value);
                            if (!isValueOk) return;
                            modeStr = value;

                            // Get -Mode
                            ProxyProgram.FakeDns.Mode mode = ProxyProgram.FakeDns.Mode.Disable;
                            if (modeStr.ToLower().Equals(Key.Programs.FakeDns.Mode.File.ToLower()))
                                mode = ProxyProgram.FakeDns.Mode.File;
                            else if (modeStr.ToLower().Equals(Key.Programs.FakeDns.Mode.Text.ToLower()))
                                mode = ProxyProgram.FakeDns.Mode.Text;
                            else if (modeStr.ToLower().Equals(Key.Programs.FakeDns.Mode.Disable.ToLower()))
                                mode = ProxyProgram.FakeDns.Mode.Disable;

                            // Get -PathOrText
                            string pathOrText = string.Empty;
                            key = Key.Programs.FakeDns.PathOrText;
                            if (mode != ProxyProgram.FakeDns.Mode.Disable)
                            {
                                isValueOk = ConsoleTools.GetValueByKey(input, key, true, true, out value);
                                if (!isValueOk) return;
                                isValueOk = ConsoleTools.GetString(key, value, true, out value);
                                if (!isValueOk) return;
                                pathOrText = value;
                            }

                            if (mode == ProxyProgram.FakeDns.Mode.File)
                            {
                                pathOrText = Path.GetFullPath(pathOrText);
                                if (!File.Exists(pathOrText))
                                {
                                    string msgNotExist = $"{pathOrText}\nFile Not Exist.";
                                    WriteToStdout(msgNotExist, ConsoleColor.Red);
                                    return;
                                }
                            }

                            if (mode == ProxyProgram.FakeDns.Mode.Text)
                            {
                                pathOrText = pathOrText.ToLower().Replace(@"\n", Environment.NewLine);
                            }

                            FakeDnsProgram.Set(mode, pathOrText);
                            ProxyServer.EnableFakeDNS(FakeDnsProgram);

                            ShowFakeDnsMsg();
                        }

                        // UpStreamProxy
                        if (input.ToLower().StartsWith($"{Key.Programs.Name.ToLower()} {Key.Programs.UpStreamProxy.Name.ToLower()}"))
                        {
                            // Programs UpStreamProxy -Mode=m -Host=m -Port=m -OnlyApplyToBlockedIPs=m

                            // Get ModeStr
                            string modeStr = Key.Programs.UpStreamProxy.Mode.Disable;
                            string key = Key.Programs.UpStreamProxy.Mode.Name;
                            isValueOk = ConsoleTools.GetValueByKey(input, key, true, false, out string value);
                            if (!isValueOk) return;

                            KeyValues modes = new();
                            modes.Add(Key.Programs.UpStreamProxy.Mode.HTTP, true, false, typeof(string));
                            modes.Add(Key.Programs.UpStreamProxy.Mode.SOCKS5, true, false, typeof(string));
                            modes.Add(Key.Programs.UpStreamProxy.Mode.Disable, true, false, typeof(string));

                            isValueOk = ConsoleTools.GetString(key, value, true, modes, out value);
                            if (!isValueOk) return;
                            modeStr = value;

                            // Get -Mode
                            ProxyProgram.UpStreamProxy.Mode mode = ProxyProgram.UpStreamProxy.Mode.Disable;
                            if (modeStr.ToLower().Equals(Key.Programs.UpStreamProxy.Mode.HTTP.ToLower()))
                                mode = ProxyProgram.UpStreamProxy.Mode.HTTP;
                            else if (modeStr.ToLower().Equals(Key.Programs.UpStreamProxy.Mode.SOCKS5.ToLower()))
                                mode = ProxyProgram.UpStreamProxy.Mode.SOCKS5;
                            else if (modeStr.ToLower().Equals(Key.Programs.UpStreamProxy.Mode.Disable.ToLower()))
                                mode = ProxyProgram.UpStreamProxy.Mode.Disable;

                            string host = string.Empty;
                            int port = 0;
                            bool onlyApplyToBlockedIPs = DefaultUpStreamProxyOnlyApplyToBlockedIPs;

                            // Get The Rest
                            if (mode != ProxyProgram.UpStreamProxy.Mode.Disable)
                            {
                                KeyValues keyValues = new();
                                keyValues.Add(Key.Programs.UpStreamProxy.Host, true, false, typeof(string));
                                keyValues.Add(Key.Programs.UpStreamProxy.Port, true, false, typeof(int), DefaultUpStreamProxyPortMin, DefaultUpStreamProxyPortMax);
                                keyValues.Add(Key.Programs.UpStreamProxy.OnlyApplyToBlockedIPs, true, false, typeof(bool));

                                bool isListOk = keyValues.GetValuesByKeys(input, out List<KeyValue> list);
                                if (!isListOk) return;
                                for (int n = 0; n < list.Count; n++)
                                {
                                    KeyValue kv = list[n];
                                    if (kv.Key.Equals(Key.Programs.UpStreamProxy.Host)) host = kv.ValueString;
                                    if (kv.Key.Equals(Key.Programs.UpStreamProxy.Port)) port = kv.ValueInt;
                                    if (kv.Key.Equals(Key.Programs.UpStreamProxy.OnlyApplyToBlockedIPs)) onlyApplyToBlockedIPs = kv.ValueBool;
                                }
                            }

                            UpStreamProxyProgram.Set(mode, host, port, onlyApplyToBlockedIPs);
                            ProxyServer.EnableUpStreamProxy(UpStreamProxyProgram);

                            ShowUpStreamProxyMsg();
                        }
                    }
                }

                // Kill All Requests
                else if (input.ToLower().Equals("killall"))
                {
                    // KillAll
                    if (ProxyServer.IsRunning)
                    {
                        ProxyServer.KillAll();
                        WriteToStdout("Done. Killed All Requests.", ConsoleColor.Green);
                    }
                    else
                        WriteToStdout("Proxy Server Is Not Running.", ConsoleColor.Blue);
                }

                // Write Requests To Log: True
                else if (input.ToLower().Equals("requests true"))
                {
                    // WriteRequests True
                    WriteRequestsToLog = true;
                    WriteToStdout($"WriteRequestsToLog: True", ConsoleColor.Green);

                    // Save Command To List
                    string baseCmd = "Requests";
                    string cmd = $"{baseCmd} True";
                    LoadCommands.AddCommand(baseCmd, cmd);
                }

                // Write Requests To Log: False
                else if (input.ToLower().Equals("requests false"))
                {
                    // WriteRequests False
                    WriteRequestsToLog = false;
                    WriteToStdout($"WriteRequestsToLog: False", ConsoleColor.Green);

                    // Save Command To List
                    string baseCmd = "Requests";
                    string cmd = $"{baseCmd} False";
                    LoadCommands.AddCommand(baseCmd, cmd);
                }

                // Write Chunk Details To Log: True
                else if (input.ToLower().Equals("chunkdetails true"))
                {
                    // ChunkDetails True
                    WriteChunkDetailsToLog = true;
                    WriteToStdout($"WriteChunkDetailsToLog: True", ConsoleColor.Green);

                    // Save Command To List
                    string baseCmd = "ChunkDetails";
                    string cmd = $"{baseCmd} True";
                    LoadCommands.AddCommand(baseCmd, cmd);
                }

                // Write Chunk Details To Log: False
                else if (input.ToLower().Equals("chunkdetails false"))
                {
                    // ChunkDetails False
                    WriteChunkDetailsToLog = false;
                    WriteToStdout($"WriteChunkDetailsToLog: False", ConsoleColor.Green);

                    // Save Command To List
                    string baseCmd = "ChunkDetails";
                    string cmd = $"{baseCmd} False";
                    LoadCommands.AddCommand(baseCmd, cmd);
                }

                // Start Proxy Server
                else if (input.ToLower().Equals("start"))
                {
                    // Start
                    if (!ProxyServer.IsRunning)
                    {
                        // Check Port
                        bool isPortOpen = NetworkTool.IsPortOpen(IPAddress.Loopback.ToString(), Settings.ListenerPort, 3);
                        if (isPortOpen)
                        {
                            WriteToStdout($"Port {Settings.ListenerPort} is occupied.");
                            return;
                        }

                        ProxyServer.Start(Settings);
                        Task.Delay(500).Wait();
                    }

                    string result = $"\nProxy Server Running: {ProxyServer.IsRunning}";
                    WriteToStdout(result, ConsoleColor.Blue);

                    ShowSettingsMsg();
                }

                // Stop Proxy Server
                else if (input.ToLower().Equals("stop"))
                {
                    // Stop
                    if (ProxyServer.IsRunning)
                    {
                        ProxyServer.Stop();
                        Task.Delay(500).Wait();
                    }

                    string result = $"\nProxy Server Running: {ProxyServer.IsRunning}";
                    WriteToStdout(result, ConsoleColor.Blue);
                }

                // Save Commands
                else if (input.ToLower().Equals("save"))
                {
                    string? p = ConsoleTools.GetCommandsPath();
                    if (!string.IsNullOrEmpty(p))
                    {
                        if (LoadCommands.Any())
                        {
                            LoadCommands.SaveToFile(p);
                            WriteToStdout("Saved To:", ConsoleColor.Green);
                            WriteToStdout(p, ConsoleColor.Green);
                        }
                        else
                            WriteToStdout("There Is Nothing To Save.", ConsoleColor.Blue);
                    }
                    else
                        WriteToStdout("Failed To Find The Path.", ConsoleColor.Red);
                }

                // load Commands
                else if (input.ToLower().Equals("load"))
                {
                    string? p = ConsoleTools.GetCommandsPath();
                    if (!string.IsNullOrEmpty(p))
                    {
                        if (File.Exists(p))
                        {
                            LoadCommands.Clear();
                            LoadCommands.LoadFromFile(p, true, true);

                            if (LoadCommands.Any())
                            {
                                for (int n = 0; n < LoadCommands.Count; n++)
                                    await ExecuteCommands(LoadCommands[n]);
                            }

                            WriteToStdout("Loaded From:", ConsoleColor.Green);
                            WriteToStdout(p, ConsoleColor.Green);
                        }
                        else
                            WriteToStdout("File Not Exist.", ConsoleColor.Blue);
                    }
                    else
                        WriteToStdout("Failed To Find The Path.", ConsoleColor.Red);
                }

                // Wrong Command
                else if (input.Length > 0)
                    WriteToStdout("Wrong Command. Type \"Help\" To Get More Info.", ConsoleColor.Red);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExecuteCommands: " + ex.Message);
        }
    }
}