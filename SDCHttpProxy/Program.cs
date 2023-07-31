using MsmhTools;
using MsmhTools.HTTPProxyServer;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;

namespace SDCHttpProxy
{
    internal static class Program
    {
        private static HTTPProxyServer HTTPProxy { get; set; } = new();
        private static HTTPProxyServer.Program.DPIBypass DpiBypassStaticProgram { get; set; } = new();
        private static HTTPProxyServer.Program.DPIBypass DpiBypassProgram { get; set; } = new();
        private static HTTPProxyServer.Program.UpStreamProxy UpStreamProxyProgram { get; set; } = new();
        private static HTTPProxyServer.Program.Dns DnsProgram { get; set; } = new();
        private static HTTPProxyServer.Program.FakeDns FakeDnsProgram { get; set; } = new();
        private static HTTPProxyServer.Program.BlackWhiteList BWListProgram { get; set; } = new();
        private static HTTPProxyServer.Program.DontBypass DontBypassProgram { get; set; } = new();

        private static readonly Stopwatch StopWatchShowRequests = new();

        private static readonly Stopwatch StopWatchShowChunkDetails = new();

        private static bool WriteRequestsToLog { get; set; } = false;
        private static bool WriteChunkDetailsToLog { get; set; } = false;


        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Title
            Console.Title = $"{Assembly.GetExecutingAssembly().GetName().Name} v{Assembly.GetExecutingAssembly().GetName().Version}";

            // Main Details
            static void MainDetails()
            {
                string mainDetails = $"details|{HTTPProxy.IsRunning}|{HTTPProxy.ListeningPort}|"; // 1, 2
                mainDetails += $"{HTTPProxy.AllRequests}|{HTTPProxy.MaxRequests}|{HTTPProxy.IsDpiActive}|"; // 3, 4, 5
                mainDetails += $"{HTTPProxyServer.StaticDPIBypassProgram.DPIBypassMode}|"; // 6
                mainDetails += $"{HTTPProxy.DPIBypassProgram.DPIBypassMode}|"; // 7
                mainDetails += $"{HTTPProxy.UpStreamProxyProgram.UpStreamMode}|"; // 8
                mainDetails += $"{HTTPProxy.DNSProgram.DNSMode}|"; // 9
                mainDetails += $"{HTTPProxy.FakeDNSProgram.FakeDnsMode}|"; // 10
                mainDetails += $"{HTTPProxy.BWListProgram.ListMode}|"; // 11
                mainDetails += $"{HTTPProxy.DontBypassProgram.DontBypassMode}"; // 12

                WriteToStdout(mainDetails);
            }

            // OnRequestReceived
            HTTPProxy.OnRequestReceived -= HTTPProxy_OnRequestReceived;
            HTTPProxy.OnRequestReceived += HTTPProxy_OnRequestReceived;
            static void HTTPProxy_OnRequestReceived(object? sender, EventArgs e)
            {
                if (WriteRequestsToLog && HTTPProxy.IsRunning)
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
            DpiBypassStaticProgram.OnChunkDetailsReceived -= DpiBypassStaticProgram_OnChunkDetailsReceived;
            DpiBypassStaticProgram.OnChunkDetailsReceived += DpiBypassStaticProgram_OnChunkDetailsReceived;
            static void DpiBypassStaticProgram_OnChunkDetailsReceived(object? sender, EventArgs e)
            {
                if (WriteChunkDetailsToLog && HTTPProxy.IsRunning && HTTPProxy.IsDpiActive)
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

            while (true)
            {
                string? input = Console.ReadLine();
                if (input == null) continue;
                if (string.IsNullOrWhiteSpace(input)) continue;
                input = input.Trim();
                
                // Clear - Stop Writing
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

                // Get Status
                else if (input.ToLower().Equals("out") || input.ToLower().Equals("status"))
                    MainDetails();

                // Get Help
                else if (input.ToLower().Equals("/?") || input.ToLower().Equals("help") || input.ToLower().Equals("/help") || input.ToLower().Equals("-help"))
                    Help();

                // Static DPI Bypass Program
                else if (input.ToLower().StartsWith("staticdpibypassprogram"))
                {
                    // staticdpibypassprogram -Disable/-Program -BeforeSniChunks -ChunkMode(-sni/-sniextension/-allextensions) -SniChunks -AntiPatternOffset -FragmentDelay
                    string[] split = input.ToLower().Split(" -", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (split.Length != 2 && split.Length != 7)
                    {
                        WrongCommand();
                        continue;
                    }

                    HTTPProxyServer.Program.DPIBypass.Mode mode = HTTPProxyServer.Program.DPIBypass.Mode.Disable;
                    if (split[1].Equals("program")) mode = HTTPProxyServer.Program.DPIBypass.Mode.Program;
                    else if (split[1].Equals("disable")) mode = HTTPProxyServer.Program.DPIBypass.Mode.Disable;

                    int beforeSniChunks = int.Parse(split[2]);

                    HTTPProxyServer.Program.DPIBypass.ChunkMode chunkMode = HTTPProxyServer.Program.DPIBypass.ChunkMode.AllExtensions;
                    if (split[3].Equals("sni")) chunkMode = HTTPProxyServer.Program.DPIBypass.ChunkMode.SNI;
                    else if (split[3].Equals("sniextension")) chunkMode = HTTPProxyServer.Program.DPIBypass.ChunkMode.SniExtension;
                    else if (split[3].Equals("allextensions")) chunkMode = HTTPProxyServer.Program.DPIBypass.ChunkMode.AllExtensions;

                    int sniChunks = int.Parse(split[4]);
                    int antiPatternOffset = int.Parse(split[5]);
                    int fragmentDelay = int.Parse(split[6]);

                    DpiBypassStaticProgram.Set(mode, beforeSniChunks, chunkMode, sniChunks, antiPatternOffset, fragmentDelay);
                    HTTPProxy.EnableStaticDPIBypass(DpiBypassStaticProgram);
                    WriteToStdout($"Done (DpiBypassStaticProgram). Mode: {mode}. ChunkMode: {chunkMode}.");
                }

                // DPIBypassProgram
                else if (input.ToLower().StartsWith("dpibypassprogram"))
                {
                    // dpibypassprogram -Disable/-Program -BeforeSniChunks -ChunkMode(-sni/-sniextension/-allextensions) -SniChunks -AntiPatternOffset -FragmentDelay
                    string[] split = input.ToLower().Split(" -", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (split.Length != 2 && split.Length != 7)
                    {
                        WrongCommand();
                        continue;
                    }

                    HTTPProxyServer.Program.DPIBypass.Mode mode = HTTPProxyServer.Program.DPIBypass.Mode.Disable;
                    if (split[1].Equals("program")) mode = HTTPProxyServer.Program.DPIBypass.Mode.Program;
                    else if (split[1].Equals("disable")) mode = HTTPProxyServer.Program.DPIBypass.Mode.Disable;

                    int beforeSniChunks = int.Parse(split[2]);

                    HTTPProxyServer.Program.DPIBypass.ChunkMode chunkMode = HTTPProxyServer.Program.DPIBypass.ChunkMode.AllExtensions;
                    if (split[3].Equals("sni")) chunkMode = HTTPProxyServer.Program.DPIBypass.ChunkMode.SNI;
                    else if (split[3].Equals("sniextension")) chunkMode = HTTPProxyServer.Program.DPIBypass.ChunkMode.SniExtension;
                    else if (split[3].Equals("allextensions")) chunkMode = HTTPProxyServer.Program.DPIBypass.ChunkMode.AllExtensions;

                    int sniChunks = int.Parse(split[4]);
                    int antiPatternOffset = int.Parse(split[5]);
                    int fragmentDelay = int.Parse(split[6]);

                    DpiBypassProgram.Set(mode, beforeSniChunks, chunkMode, sniChunks, antiPatternOffset, fragmentDelay);
                    HTTPProxy.EnableDPIBypass(DpiBypassProgram);
                    WriteToStdout($"Done (DpiBypassProgram). Mode: {mode}. ChunkMode: {chunkMode}.");
                }

                // UpStreamProxyProgram
                else if (input.ToLower().StartsWith("upstreamproxyprogram"))
                {
                    // upstreamproxyprogram -disable/-http/-socks5 -proxyHost -proxyPort -onlyApplyToBlockedIPs(-true/-false)
                    string[] split = input.ToLower().Split(" -", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (split.Length != 5)
                    {
                        WrongCommand();
                        continue;
                    }

                    HTTPProxyServer.Program.UpStreamProxy.Mode proxyMode = HTTPProxyServer.Program.UpStreamProxy.Mode.Disable;
                    if (split[1].Equals("http")) proxyMode = HTTPProxyServer.Program.UpStreamProxy.Mode.HTTP;
                    else if (split[1].Equals("socks5")) proxyMode = HTTPProxyServer.Program.UpStreamProxy.Mode.SOCKS5;
                    else if (split[1].Equals("disable")) proxyMode = HTTPProxyServer.Program.UpStreamProxy.Mode.Disable;

                    string proxyHost = split[2];
                    int proxyPort = int.Parse(split[3]);
                    bool onlyApplyToBlockedIPs = bool.Parse(split[4]);

                    UpStreamProxyProgram.Set(proxyMode, proxyHost, proxyPort, onlyApplyToBlockedIPs);
                    HTTPProxy.EnableUpStreamProxy(UpStreamProxyProgram);
                    WriteToStdout($"Done (UpStreamProxyProgram). Mode: {proxyMode}.");
                }

                // DNSProgram
                else if (input.ToLower().StartsWith("dnsprogram"))
                {
                    // dnsprogram -disable/-doh/-plaindns/-system -dns/-null -dnsCleanIP/-null -timeoutSec -proxyScheme/-null
                    // dnsprogram -setcloudflareips -true -cfCleanIP -cfIpRange/-null
                    // dnsprogram -setcloudflareips -false
                    string[] split = input.ToLower().Split(" -", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (split.Length != 6 && split.Length != 5 && split.Length != 3)
                    {
                        WrongCommand();
                        continue;
                    }

                    if (split.Length == 6)
                    {
                        HTTPProxyServer.Program.Dns.Mode mode = HTTPProxyServer.Program.Dns.Mode.Disable;
                        if (split[1].Equals("doh")) mode = HTTPProxyServer.Program.Dns.Mode.DoH;
                        else if (split[1].Equals("plaindns")) mode = HTTPProxyServer.Program.Dns.Mode.PlainDNS;
                        else if (split[1].Equals("system")) mode = HTTPProxyServer.Program.Dns.Mode.System;
                        else if (split[1].Equals("disable")) mode = HTTPProxyServer.Program.Dns.Mode.Disable;

                        string? dns = split[2];
                        if (split[2].Equals("null")) dns = null;

                        string? dnsCleanIP = split[3];
                        if (split[3].Equals("null")) dnsCleanIP = null;

                        int timeoutSec = int.Parse(split[4]);

                        string? proxyScheme = split[5];
                        if (split[5].Equals("null")) proxyScheme = null;

                        DnsProgram.Set(mode, dns, dnsCleanIP, timeoutSec, proxyScheme);
                        WriteToStdout($"Done (DNSProgram). Mode: {mode}.");
                    }
                    else if (split[1].Equals("setcloudflareips"))
                    {
                        if (split[2].Equals("true"))
                        {
                            string cfCleanIP = split[3];
                            string? cfIpRange = split[4].Replace(@"\n", Environment.NewLine);
                            if (split[4].Equals("null")) cfIpRange = null;

                            DnsProgram.SetCloudflareIPs(cfCleanIP, cfIpRange);
                            DnsProgram.ChangeCloudflareIP = true;
                            WriteToStdout($"Done. CF Enabled. Clean IP: {cfCleanIP}");
                        }
                        else if (split[2].Equals("false"))
                        {
                            DnsProgram.ChangeCloudflareIP = false;
                            WriteToStdout($"Done. CF Disabled.");
                        }
                        
                    }

                    HTTPProxy.EnableDNS(DnsProgram);
                }

                // FakeDNSProgram
                else if (input.ToLower().StartsWith("fakednsprogram"))
                {
                    // fakednsprogram -disable/-text/-file -filePathOrText
                    string separator = " -";
                    string[] split = input.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    
                    HTTPProxyServer.Program.FakeDns.Mode mode = HTTPProxyServer.Program.FakeDns.Mode.Disable;
                    if (split[1].ToLower().Equals("text")) mode = HTTPProxyServer.Program.FakeDns.Mode.Text;
                    else if (split[1].ToLower().Equals("file")) mode = HTTPProxyServer.Program.FakeDns.Mode.File;
                    else if (split[1].ToLower().Equals("disable")) mode = HTTPProxyServer.Program.FakeDns.Mode.Disable;
                    
                    string filePathOrText = split[2];
                    if (mode == HTTPProxyServer.Program.FakeDns.Mode.File)
                    {
                        string filePath = string.Empty;
                        for (int n = 2; n < split.Length; n++)
                            filePath += $"{split[n]}{separator}";
                        if (filePath.EndsWith(separator)) filePath = filePath[0..^2];
                        filePathOrText = Path.GetFullPath(filePath);
                    }
                    if (mode == HTTPProxyServer.Program.FakeDns.Mode.Text)
                        filePathOrText = split[2].ToLower().Replace(@"\n", Environment.NewLine);
                    
                    FakeDnsProgram.Set(mode, filePathOrText);
                    HTTPProxy.EnableFakeDNS(FakeDnsProgram);
                    WriteToStdout($"Done (FakeDNSProgram). Mode: {mode}.");
                }

                // BWListProgram
                else if (input.ToLower().StartsWith("bwlistprogram"))
                {
                    // bwlistprogram -disable/-blacklistfile/-blacklisttext/-whitelistfile/-whitelisttext -filePathOrText
                    string separator = " -";
                    string[] split = input.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    
                    HTTPProxyServer.Program.BlackWhiteList.Mode mode = HTTPProxyServer.Program.BlackWhiteList.Mode.Disable;
                    if (split[1].ToLower().Equals("blacklistfile")) mode = HTTPProxyServer.Program.BlackWhiteList.Mode.BlackListFile;
                    else if (split[1].ToLower().Equals("blacklisttext")) mode = HTTPProxyServer.Program.BlackWhiteList.Mode.BlackListText;
                    else if (split[1].ToLower().Equals("whitelistfile")) mode = HTTPProxyServer.Program.BlackWhiteList.Mode.WhiteListFile;
                    else if (split[1].ToLower().Equals("whitelisttext")) mode = HTTPProxyServer.Program.BlackWhiteList.Mode.WhiteListText;
                    else if (split[1].ToLower().Equals("disable")) mode = HTTPProxyServer.Program.BlackWhiteList.Mode.Disable;

                    string filePathOrText = split[2];
                    if (mode == HTTPProxyServer.Program.BlackWhiteList.Mode.BlackListFile || mode == HTTPProxyServer.Program.BlackWhiteList.Mode.WhiteListFile)
                    {
                        string filePath = string.Empty;
                        for (int n = 2; n < split.Length; n++)
                            filePath += $"{split[n]}{separator}";
                        if (filePath.EndsWith(separator)) filePath = filePath[0..^2];
                        filePathOrText = Path.GetFullPath(filePath);
                    }
                    else if (mode == HTTPProxyServer.Program.BlackWhiteList.Mode.BlackListText || mode == HTTPProxyServer.Program.BlackWhiteList.Mode.WhiteListText)
                        filePathOrText = split[2].ToLower().Replace(@"\n", Environment.NewLine); // Text

                    BWListProgram.Set(mode, filePathOrText);
                    HTTPProxy.EnableBlackWhiteList(BWListProgram);
                    WriteToStdout($"Done (BWListProgram). Mode: {mode}.");
                }

                // DontBypassProgram
                else if (input.ToLower().StartsWith("dontbypassprogram"))
                {
                    // dontbypassprogram -disable/-text/-file -filePathOrText
                    string separator = " -";
                    string[] split = input.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    
                    HTTPProxyServer.Program.DontBypass.Mode mode = HTTPProxyServer.Program.DontBypass.Mode.Disable;
                    if (split[1].ToLower().Equals("file")) mode = HTTPProxyServer.Program.DontBypass.Mode.File;
                    else if (split[1].ToLower().Equals("text")) mode = HTTPProxyServer.Program.DontBypass.Mode.Text;
                    else if (split[1].ToLower().Equals("disable")) mode = HTTPProxyServer.Program.DontBypass.Mode.Disable;

                    string filePathOrText = split[2];
                    if (mode == HTTPProxyServer.Program.DontBypass.Mode.File)
                    {
                        string filePath = string.Empty;
                        for (int n = 2; n < split.Length; n++)
                            filePath += $"{split[n]}{separator}";
                        if (filePath.EndsWith(separator)) filePath = filePath[0..^2];
                        filePathOrText = Path.GetFullPath(filePath);
                    }
                    else if (mode == HTTPProxyServer.Program.DontBypass.Mode.Text)
                        filePathOrText = split[2].ToLower().Replace(@"\n", Environment.NewLine); // Text

                    DontBypassProgram.Set(mode, filePathOrText);
                    HTTPProxy.EnableDontBypass(DontBypassProgram);
                    WriteToStdout($"Done (DontBypassProgram). Mode: {mode}.");
                }

                // BlockPort80
                else if (input.ToLower().StartsWith("blockport80"))
                {
                    // blockport80 -true/-false
                    string[] split = input.ToLower().Split(" -", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (split.Length != 2)
                    {
                        WrongCommand();
                        continue;
                    }

                    bool bp80 = bool.Parse(split[1]);

                    HTTPProxy.BlockPort80 = bp80;
                    WriteToStdout($"Done (BlockPort80). Enabled: {bp80}");
                }

                // Kill All Requests
                else if (input.ToLower().Equals("killall"))
                {
                    HTTPProxy.KillAll();
                    WriteToStdout("Done. Killed All Requests.");
                }

                // KillOnCpuUsage
                else if (input.ToLower().StartsWith("killoncpuusage"))
                {
                    // killoncpuusage -percent
                    string[] split = input.ToLower().Split(" -", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (split.Length != 2)
                    {
                        WrongCommand();
                        continue;
                    }

                    int percent = int.Parse(split[1]);
                    HTTPProxy.KillOnCpuUsage = percent;
                    WriteToStdout($"Done (KillOnCpuUsage). {percent}%.");
                }

                else if (input.ToLower().StartsWith("requesttimeout"))
                {
                    // requesttimeout -sec
                    string[] split = input.ToLower().Split(" -", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (split.Length != 2)
                    {
                        WrongCommand();
                        continue;
                    }

                    int reqTimeoutSec = int.Parse(split[1]);
                    HTTPProxy.RequestTimeoutSec = reqTimeoutSec;
                    WriteToStdout($"Done (RequestTimeout). {reqTimeoutSec} Second.");
                }

                // WriteRequestsToLog
                else if (input.ToLower().StartsWith("writerequests"))
                {
                    // writerequests -true/-false
                    string[] split = input.ToLower().Split(" -", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (split.Length != 2)
                    {
                        WrongCommand();
                        continue;
                    }

                    bool enable = bool.Parse(split[1]);
                    WriteRequestsToLog = enable;
                    WriteToStdout($"Done. WriteRequestsToLog: {enable}");
                }

                // WriteChunkDetailsToLog
                else if (input.ToLower().StartsWith("writechunkdetails"))
                {
                    // writechunkdetails -true/-false
                    string[] split = input.ToLower().Split(" -", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (split.Length != 2)
                    {
                        WrongCommand();
                        continue;
                    }

                    bool enable = bool.Parse(split[1]);
                    WriteChunkDetailsToLog = enable;
                    WriteToStdout($"Done. WriteChunkDetailsToLog: {enable}");
                }

                // Start
                else if (input.ToLower().StartsWith("start"))
                {
                    // start -loopback/-any -port -requests
                    string[] split = input.ToLower().Split(" -", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (split.Length != 4)
                    {
                        WrongCommand();
                        continue;
                    }

                    IPAddress ip = IPAddress.Any;
                    if (split[1].Equals("loopback"))
                        ip = IPAddress.Loopback;

                    int port = int.Parse(split[2]);
                    int requests = int.Parse(split[3]);

                    // Check Port
                    bool isPortOpen = Network.IsPortOpen(IPAddress.Loopback.ToString(), port, 3);
                    if (isPortOpen)
                    {
                        WriteToStdout($"Port {port} is occupied.");
                        continue;
                    }

                    HTTPProxy.Start(ip, port, requests);
                    WriteToStdout($"HTTP Proxy Server Started {ip}:{port}.");
                }

                // Stop
                else if (input.ToLower().Equals("stop"))
                {
                    // stop
                    if (HTTPProxy.IsRunning)
                    {
                        HTTPProxy.Stop();
                        WriteToStdout("HTTP Proxy Server Stopped.");
                    }
                    else
                    {
                        WriteToStdout("HTTP Proxy Server Already Stopped.");
                    }
                }

                // Wrong Command
                else if (input.Length > 0)
                    WrongCommand();
            }

        }

        private static void WriteToStdout(string msg)
        {
            Console.Out.WriteLine(msg);
        }

        private static void WriteToStderr(string msg)
        {
            Console.Error.WriteLine(msg);
        }

        private static void WrongCommand()
        {
            Console.WriteLine("Wrong command.");
        }

        // Help
        static void Help()
        {
            string NL = Environment.NewLine;
            string help = string.Empty;

            help += $"{NL}{Console.Title}{NL}";

            help += $"{NL}Stop Writing and Clear. Command:{NL}";
            help += $"c{NL}";

            help += $"{NL}Clear Screen. Command:{NL}";
            help += $"cls{NL}";

            help += $"{NL}Get Proxy Variables. Command:{NL}";
            help += $"out/status{NL}";

            help += $"{NL}Activate Static DPI Bypass Program. Command:{NL}";
            help += $"staticdpibypassprogram -Disable/-Program -BeforeSniChunks -ChunkMode(-sni/-sniextension/-allextensions) -SniChunks -AntiPatternOffset -FragmentDelay{NL}";
            
            help += $"{NL}Activate DPI Bypass Program. Command:{NL}";
            help += $"dpibypassprogram -Disable/-Program -BeforeSniChunks -ChunkMode(-sni/-sniextension/-allextensions) -SniChunks -AntiPatternOffset -FragmentDelay{NL}";
            
            help += $"{NL}Activate Upstream Proxy Program. Command:{NL}";
            help += $"upstreamproxyprogram -disable/-http/-socks5 -proxyHost -proxyPort -onlyApplyToBlockedIPs(-true/-false){NL}";

            help += $"{NL}Activate DNS Program (Use \\n for NewLine). Commands:{NL}";
            help += $"dnsprogram -disable/-doh/-plaindns/-system -dns/null -dnsCleanIP/-null -timeoutSec -proxyScheme/-null{NL}";
            help += $"dnsprogram -setcloudflareips -true -cfCleanIP -cfIpRange/-null{NL}";
            help += $"dnsprogram -setcloudflareips -false{NL}";

            help += $"{NL}Activate Fake DNS Program (Use \\n for NewLine). Command:{NL}";
            help += $"fakednsprogram -disable/-text/-file -filePathOrText{NL}";

            help += $"{NL}Activate Black White List Program (Use \\n for NewLine). Command:{NL}";
            help += $"bwlistprogram -disable/-blacklistfile/-blacklisttext/-whitelistfile/-whitelisttext -filePathOrText{NL}";
            
            help += $"{NL}Activate DontBypass Program (Use \\n for NewLine). Command:{NL}";
            help += $"dontbypassprogram -disable/-text/-file -filePathOrText{NL}";

            help += $"{NL}Block Port 80. Command:{NL}";
            help += $"blockport80 -true/-false{NL}";

            help += $"{NL}Kill All Requests. Command:{NL}";
            help += $"killall{NL}";

            help += $"{NL}Kill On CPU Usage. Command:{NL}";
            help += $"killoncpuusage -percent{NL}";
            
            help += $"{NL}Kill Request On Timeout (Sec). Command:{NL}";
            help += $"requesttimeout -sec{NL}";

            help += $"{NL}Write Requests to Stderr. Command:{NL}";
            help += $"writerequests -true/-false{NL}";

            help += $"{NL}Write Chunk Details to Stderr. Command:{NL}";
            help += $"writechunkdetails -true/-false{NL}";

            help += $"{NL}Start Proxy. Command:{NL}";
            help += $"start -loopback/-any -port -requests{NL}";

            help += $"{NL}Stop Proxy. Command:{NL}";
            help += $"stop{NL}";

            WriteToStdout(help);
        }

    }
}