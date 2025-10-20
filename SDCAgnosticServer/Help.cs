using System.Diagnostics;
using System.Net;
using System.Reflection;
using MsmhToolsClass.MsmhAgnosticServer;

namespace SDCAgnosticServer;

public class Help
{
    public static async Task GetHelpAsync()
    {
        string help;

        // Title
        help = $"\nMsmh Agnostic Server v{Assembly.GetExecutingAssembly().GetName().Version}, Author: msasanmh@gmail.com";
        help += "\nServer Supports:";
        help += "\n    UDP Plain DNS";
        help += "\n    TCP Plain DNS";
        help += "\n    DNS Over HTTPS (DoH)";
        help += "\n    HTTP (Domain, IPv4, IPv6) (Get, Post, etc)";
        help += "\n    HTTPS (Domain, IPv4, IPv6) (Post, etc)";
        help += "\n    SOCKS4 (IPv4) (Connect, Bind)";
        help += "\n    SOCKS4A (Domain, IPv4) (Connect, Bind)";
        help += "\n    SOCKS5 (Domain, IPv4, IPv6) (Connect, Bind, UDP)";
        help += "\n    SSL Decryption";
        await WriteToStdoutAsync(help);

        // Commands
        await WriteToStdoutAsync("\nCommands:");

        // Exit
        help = $"\n{Key.Common.Exit}";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Exit The App.";
        await WriteToStdoutAsync(help);

        // C
        help = $"\n{Key.Common.C}";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Clear Screen And Stop Writing.";
        await WriteToStdoutAsync(help);

        // CLS
        help = $"\n{Key.Common.CLS}";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Clear Screen.";
        await WriteToStdoutAsync(help);

        // OUT
        help = $"\n{Key.Common.Out} <ProfileName>";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Get Status In Machine Read.";
        await WriteToStdoutAsync(help);

        // STATUS
        help = $"\n{Key.Common.Status}";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Get Status In Human Read.";
        await WriteToStdoutAsync(help);

        // Flush DNS
        help = $"\n{Key.Common.Flush}";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Flush DNS Cache.";
        await WriteToStdoutAsync(help);

        // Start
        help = $"\n{Key.Common.Start}";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Start Server.";
        await WriteToStdoutAsync(help);

        // Stop
        help = $"\n{Key.Common.Stop}";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Stop Server.";
        await WriteToStdoutAsync(help);

        // Update
        help = $"\n{Key.Common.Stop}";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Update Server Programs Settings (File Mode Only).";
        await WriteToStdoutAsync(help);

        // Parent Process ID
        help = $"\n{Key.ParentProcess.Name} <Option>";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Exit When Parent Terminated.";
        await WriteToStdoutAsync(help);

        // Option Of ParentProcess
        help = $"\n  <Option>";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);

        help = $"    -{Key.ParentProcess.PID}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      The ID Of The Parent Process. (Set To 0 To Disable)";
        await WriteToStdoutAsync(help);

        // Setting Interactive Mode
        help = $"\n{Key.Setting.Name}";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Modify Server Settings In Interactive Mode.";
        await WriteToStdoutAsync(help);

        // Setting Command Mode
        // setting -Port=m -WorkingMode= -MaxRequests= -DnsTimeoutSec= -ProxyTimeoutSec= -KillOnCpuUsage= -BlockPort80=
        // -AllowInsecure= -DNSs="" -CfCleanIP= -BootstrapIp= -BootstrapPort=
        // -ProxyScheme= -ProxyUser= -ProxyPass= -OnlyBlockedIPs=
        help = $"\n{Key.Setting.Name} <Option>";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Modify Server Settings In Command Mode.";
        await WriteToStdoutAsync(help);

        // Option Of AgnosticSettings
        help = $"\n  <Option>";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);

        help = $"    -{Key.Setting.Port}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Server Listener Port.";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.Setting.WorkingMode}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      {AgnosticSettings.WorkingMode.Dns}: Only DNS Servers (UDP/TCP/DoH).";
        help += $"      {AgnosticSettings.WorkingMode.Proxy}: UDP/TCP DNS Servers And Proxies.";
        help += $"      {AgnosticSettings.WorkingMode.DnsAndProxy}: All DNS And Proxy Servers (Not Recommended).";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.Setting.MaxRequests}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Max Requests To Handle Per Second.";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.Setting.DnsTimeoutSec}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Limit TTL Of DNS Requests.";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.Setting.ProxyTimeoutSec}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Limit TTL Of Proxy Requests.";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.Setting.KillOnCpuUsage}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Kill All Requests If CPU Gets Higher Than Specified Value (Windows Only).";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.Setting.BlockPort80}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Block Port 80 To Avoid HSTS and Bad Cert Error.";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.Setting.AllowInsecure}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Allow Insecure (Ignore Certificate Errors When Connecting To DNS Servers).";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.Setting.DNSs}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      DNS Servers To Connect To (Require Double Quotes).";
        help += $"      Format 1: (Comma Separated) e.g. \"tcp://8.8.8.8,udp://9.9.9.9:9953,https://dns.google/dns-query\"";
        help += $"      Format 2: (File Path - Each Line One DNS) e.g. \"PathToFile.txt\"";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.Setting.CfCleanIP}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Cloudflare CDN Clean IPv4 Or IPv6 To Redirect All Cloudflare IPs To This One.";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.Setting.BootstrapIp}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Bootstrap (A Plain DNS).";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.Setting.BootstrapPort}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Bootstrap Port.";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.Setting.ProxyScheme}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Upstream Proxy Scheme.";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.Setting.ProxyUser}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Upstream Proxy Username.";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.Setting.ProxyPass}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Upstream Proxy Pass.";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.Setting.OnlyBlockedIPs}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Use Upstream Proxy Only For Blocked Servers.";
        await WriteToStdoutAsync(help);

        // Example Of Setting
        help = $"\n  Example:";
        help += $"\n    Setting -Port=8080 -MaxRequests=500 -ProxyTimeoutSec=40 -KillOnCpuUsage=35.6 -BlockPort80=True";
        await WriteToStdoutAsync(help);

        // SSLSetting Interactive Mode
        help = $"\n{Key.SSLSetting.Name}";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Modify SSL AgnosticSettings In Interactive Mode.";
        await WriteToStdoutAsync(help);

        // SSLSetting Command Mode - sslsetting -Enable=m -RootCA_Path="" -RootCA_KeyPath="" -Cert_Path="" -Cert_KeyPath="" -ChangeSni= -DefaultSni=
        help = $"\n{Key.SSLSetting.Name} <Option>";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Modify SSL AgnosticSettings In Command Mode.";
        await WriteToStdoutAsync(help);

        // Option Of SSLSettings
        help = $"\n  <Option>";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);

        help = $"    -{Key.SSLSetting.Enable}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Use Certificate To Enable DoH/HTTPS Proxy And Decrypt SSL (True/False).";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.SSLSetting.ServerDomainName}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Your Server Domain Name To Distinguish DoH/HTTPS/SNI-Proxy Requests If Running On A VPS.";
        help += $"\n      Leave Empty If Your 'WorkingMode' Is 'DNS'.";
        help += $"\n      Leave Empty If You're Running The Server On A Local PC.";
        help += $"\n      Leave Empty If You're Working Directly With Server IP.";
        help += $"\n      * Your Server Cert Must Match Your IP Or Support SAN With IP, Unless It's A Local IP.";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.SSLSetting.ChangeSni}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Change SNI To Bypass DPI (Only On A Local PC, Not A VPS).";
        help += $"\n      * If Active, Certificates Will Be Generated Automatically.";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.SSLSetting.DefaultSni}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Change All SNIs To This SNI (Leave Empty To Use Original SNI).";
        help += $"\n      * Original SNI Can't Bypass DPI.";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.SSLSetting.RootCA_Path}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Path To Your Root Certificate File (e.g. C:\\RootCA.crt) - Leave Empty If Not Running On A VPS.";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.SSLSetting.RootCA_KeyPath}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Path To Your Root Certificate Private Key File If It's Not Included In The 'Root Certificate' File. (e.g. C:\\RootCA.key).";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.SSLSetting.Cert_Path}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Path To Your Certificate File Issued With Your RootCA (e.g. C:\\Local.crt) - Leave Empty If Not Running On A VPS.";
        await WriteToStdoutAsync(help);

        help = $"    -{Key.SSLSetting.Cert_KeyPath}=";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Path To Your Certificate Private Key File If It's Not Included In The 'Certificate' File. (e.g. C:\\Local.key).";
        await WriteToStdoutAsync(help);

        // Programs Interactive Mode
        help = $"\n{Key.Programs.Name}";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Modify Programs (Plug-Ins) In Interactive Mode.";
        await WriteToStdoutAsync(help);

        // Programs Command Mode
        help = $"\n{Key.Programs.Name} <Program>";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Modify Programs (Plug-Ins) In Command Mode.";
        await WriteToStdoutAsync(help);

        // Programs
        help = $"\n  <Program>";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);

        help = $"    {Key.Programs.Fragment.Name}";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Activate Fragment To Circumvent Censorship (Requires Dns To Be Set To A DoH).";
        await WriteToStdoutAsync(help);

        help = $"    {Key.Programs.Rules.Name}";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Set Per Domain/IP/CIDR Rules. e.g. Block, DNS, FakeDNS, Direct, FakeSNI, ETC.";
        await WriteToStdoutAsync(help);

        help = $"    {Key.Programs.DnsLimit.Name}";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Block PlainDns Server Or Only Allow DoH Server With Specific Paths.";
        await WriteToStdoutAsync(help);

        help = $"\n  Type Help Program <ProgramName> To Get More Info.";
        await WriteToStdoutAsync(help, ConsoleColor.DarkYellow);

        // KillAll
        help = $"\n{Key.Common.KillAll}";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Kill All Active Proxy Requests.";
        await WriteToStdoutAsync(help);

        // Set DNS
        help = $"\n{Key.Common.SetDNS}";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Set DNS To Loopback. (A DNS Server Must Be Run On {IPAddress.Loopback}:53)";
        await WriteToStdoutAsync(help);

        // Unset DNS
        help = $"\n{Key.Common.UnsetDNS}";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Set System DNS To DHCP.";
        await WriteToStdoutAsync(help);

        // Write Requests
        help = $"\n{Key.Common.Requests} <Option>";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);

        help = $"\n  <Option>";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);

        help = $"    True";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Write Requests To Stderr.";
        await WriteToStdoutAsync(help);

        help = $"    False";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Stop Writing Requests.";
        await WriteToStdoutAsync(help);

        // Write Chunk Details
        help = $"\n{Key.Common.FragmentDetails} <Option>";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);

        help = $"\n  <Option>";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);

        help = $"    True";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Write Fragment Details To Stderr.";
        await WriteToStdoutAsync(help);

        help = $"    False";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Stop Writing Chunk Details.";
        await WriteToStdoutAsync(help);

        // Write DebugInfo
        help = $"\n{Key.Common.DebugInfo} <Option>";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);

        help = $"\n  <Option>";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);

        help = $"    True";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Write DebugInfo To Stderr.";
        await WriteToStdoutAsync(help);

        help = $"    False";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Stop Writing DebugInfo.";
        await WriteToStdoutAsync(help);

        // LogToFile
        help = $"\n{Key.Common.LogToFile} <Option>";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);

        help = $"\n  <Option>";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);

        help = $"    True";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Write Requests And DebugInfo To File.";
        await WriteToStdoutAsync(help);

        help = $"    False";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Stop Writing To File.";
        await WriteToStdoutAsync(help);

        // Save
        help = $"\n{Key.Common.Save}";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Save Commands To File.";
        await WriteToStdoutAsync(help);

        // Load
        help = $"\n{Key.Common.Load}";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Load Commands From File.";
        await WriteToStdoutAsync(help);
    }

    public static async Task GetHelpProgramsAsync()
    {
        string msg = "Help Program <Program>:\n";
        await WriteToStdoutAsync(msg, ConsoleColor.Blue);
        msg = "  <Program>";
        await WriteToStdoutAsync(msg, ConsoleColor.Blue);
        msg = $"    {Key.Programs.Fragment.Name}\n";
        msg += $"    {Key.Programs.Rules.Name}\n";
        msg += $"    {Key.Programs.DnsLimit.Name}\n";
        await WriteToStdoutAsync(msg, ConsoleColor.Cyan);
    }

    public static async Task GetHelpFragmentAsync()
    {
        // Help Program Fragment
        // Programs Fragment -Mode=m -BeforeSniChunks=m -ChunkMode=m -SniChunks=m -AntiPatternOffset=m -FragmentDelay=m
        string help;
        help = $"\nPrograms {Key.Programs.Fragment.Name}";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Activate Fragment To Circumvent Censorship (Requires Dns To Be Set To A DoH).";
        await WriteToStdoutAsync(help);

        // -Mode=
        help = "\n  -Mode=";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);

        help = $"    {Key.Programs.Fragment.Mode.Program.Name}";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      To Activate Fragmentation.";
        await WriteToStdoutAsync(help);

        help = $"    {Key.Programs.Fragment.Mode.Disable}";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      To Disable Fragmentation.";
        await WriteToStdoutAsync(help);

        // -BeforeSniChunks=
        help = $"\n  -{Key.Programs.Fragment.Mode.Program.BeforeSniChunks}=";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"    Number Of Buffer Chunks Before SNI Extension.";
        await WriteToStdoutAsync(help);

        // -ChunkMode=
        help = $"\n  -{Key.Programs.Fragment.Mode.Program.ChunkMode.Name}=";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);

        help = $"    {Key.Programs.Fragment.Mode.Program.ChunkMode.SNI}";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Find SNI And Chunk Based On It.";
        await WriteToStdoutAsync(help);

        help = $"    {Key.Programs.Fragment.Mode.Program.ChunkMode.SniExtension}";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Find Sni Extension And Chunk Based On It.";
        await WriteToStdoutAsync(help);

        help = $"    {Key.Programs.Fragment.Mode.Program.ChunkMode.AllExtensions}";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Find All Extensions And Chunk Based On It.";
        await WriteToStdoutAsync(help);

        // -SniChunks=
        help = $"\n  -{Key.Programs.Fragment.Mode.Program.SniChunks}=";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"    Number Of SNI/SniExtension/AllExtensions Chunks.";
        await WriteToStdoutAsync(help);

        // -AntiPatternOffset=
        help = $"\n  -{Key.Programs.Fragment.Mode.Program.AntiPatternOffset}=";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"    Set An Offset To Chunks To Avoid Pattern Detection.";
        await WriteToStdoutAsync(help);

        // -FragmentDelay=
        help = $"\n  -{Key.Programs.Fragment.Mode.Program.FragmentDelay}=";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"    Set Delay Between Sending Chunks In Milliseconds.";
        await WriteToStdoutAsync(help);

        // Examples
        help = $"\nExamples:";
        help += $"\n  Programs Fragment -Mode=Disable";
        help += $"\n  Programs Fragment -Mode=Program -BeforeSniChunks=50 -ChunkMode=SNI -SniChunks=5 -AntiPatternOffset=2 -FragmentDelay=1";
        help += $"\n  Programs Fragment -Mode=Program -BeforeSniChunks=50 -ChunkMode=AllExtensions -SniChunks=20 -AntiPatternOffset=2 -FragmentDelay=1";
        await WriteToStdoutAsync(help);
    }

    public static async Task GetHelpRulesAsync()
    {
        // Help Program Rules
        // Programs Rules -Mode=m -PathOrText="m"
        string help;
        help = $"\nPrograms {Key.Programs.Rules.Name}";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Set Per Domain Rules. e.g. Block, NoBypass, FakeDNS, DNS, FakeSNI, ETC.";
        await WriteToStdoutAsync(help);

        // -Mode=
        help = "\n  -Mode=";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);

        help = $"    {Key.Programs.Rules.Mode.File}";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Rules As File.";
        await WriteToStdoutAsync(help);

        help = $"    {Key.Programs.Rules.Mode.Text}";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Rules As Text.";
        await WriteToStdoutAsync(help);

        help = $"    {Key.Programs.Rules.Mode.Disable}";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      To Disable {Key.Programs.Rules.Name}.";
        await WriteToStdoutAsync(help);

        // -PathOrText=
        help = $"\n  -{Key.Programs.Rules.PathOrText}=";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"    The File Path Or Text Depends On The Mode (Require Double Quotes).";
        help += $"\n    File Mode: The Path Of File. (Each Line One Rule).";
        help += $"\n    Text Mode: e.g. \"Google.com|8.8.8.8;\\nCloudflare.com|dns:tcp://8.8.8.8;\"";
        await WriteToStdoutAsync(help);

        // Examples
        help = $"\nExamples:";
        help += $"\n  Programs {Key.Programs.Rules.Name} -Mode=Disable";
        help += $"\n  Programs {Key.Programs.Rules.Name} -Mode=File -PathOrText=\"C:\\list.txt\"";
        help += $"\n  Programs {Key.Programs.Rules.Name} -Mode=Text -PathOrText=\"Google.com|8.8.8.8\\nCloudflare.com|1.1.1.1\"";
        await WriteToStdoutAsync(help);

        // Rules Syntax Example
        help = $"\n{Key.Programs.Rules.Name} Syntax Example:";
        help += $"\n  dns:8.8.8.8:53;";
        help += $"\n  instagram.com|163.70.128.174;sni:speedtest.com;";
        help += $"\n  youtube.com|dns:8.8.8.8:53;dnsdomain:google.com;sni:google.com;";
        help += $"\n  ytimg.com|dns:8.8.8.8:53;dnsdomain:google.com;";
        help += $"\n  *.ytimg.com|dns:8.8.8.8:53;dnsdomain:google.com;";
        help += $"\n  ggpht.com|dns:8.8.8.8:53;dnsdomain:google.com;";
        help += $"\n  *.ggpht.com|dns:8.8.8.8:53;dnsdomain:*.googleusercontent.com;";
        help += $"\n  *.googleapis|dns:8.8.8.8:53;dnsdomain:google.com;";
        help += $"\n  *.googlevideo.com|dns:8.8.8.8:53;dnsdomain:*.c.docs.google.com;sni:google.com;";
        help += $"\n\n  More Help: https://github.com/msasanmh/SecureDNSClient";
        await WriteToStdoutAsync(help);
    }

    public static async Task GetHelpDnsLimitAsync()
    {
        // Help Program DnsLimit
        // Programs DnsLimit -Enable=m -DisablePlain=m -DoHPathLimitMode=m -PathOrText="m"
        string help;
        help = $"\nPrograms {Key.Programs.DnsLimit.Name}";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"  Block PlainDns Server Or Only Allow DoH Server With Specific Paths.";
        await WriteToStdoutAsync(help);

        // -Enable=
        help = $"\n  -{Key.Programs.DnsLimit.Enable}=";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"      Enable Or Disable {Key.Programs.DnsLimit.Name} Program.";
        await WriteToStdoutAsync(help);

        // -DisablePlain=
        help = $"\n  -{Key.Programs.DnsLimit.DisablePlain}=";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"      Enable Or Disable PlainDns Server (UDP And TCP Protocols).";
        await WriteToStdoutAsync(help);

        // -DoHPathLimitMode=
        help = $"\n  -{Key.Programs.DnsLimit.DoHPathLimitMode.Name}=";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);

        help = $"    {Key.Programs.DnsLimit.DoHPathLimitMode.File}";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Allowed DoH Paths As File.";
        await WriteToStdoutAsync(help);

        help = $"    {Key.Programs.DnsLimit.DoHPathLimitMode.Text}";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      Allowed DoH Paths As Text.";
        await WriteToStdoutAsync(help);

        help = $"    {Key.Programs.DnsLimit.DoHPathLimitMode.Disable}";
        await WriteToStdoutAsync(help, ConsoleColor.Cyan);
        help = $"      To Disable {Key.Programs.DnsLimit.DoHPathLimitMode.Name}.";
        await WriteToStdoutAsync(help);

        // -PathOrText=
        help = $"\n  -{Key.Programs.DnsLimit.PathOrText}=";
        await WriteToStdoutAsync(help, ConsoleColor.Blue);
        help = $"    The File Path Or Text Depends On The {Key.Programs.DnsLimit.DoHPathLimitMode.Name} (Require Double Quotes).";
        help += $"\n    File Mode: The Path Of File. (Each Line One Path).";
        help += $"\n    Text Mode: e.g. \"dns-query\\nUserName1\"";
        await WriteToStdoutAsync(help);

        // Examples
        help = $"\nExamples:";
        help += $"\n  Programs {Key.Programs.DnsLimit.Name} -{Key.Programs.DnsLimit.Enable}=False";
        help += $"\n  Programs {Key.Programs.DnsLimit.Name} -{Key.Programs.DnsLimit.Enable}=True -{Key.Programs.DnsLimit.DisablePlain}=False -{Key.Programs.DnsLimit.DoHPathLimitMode.Name}=File -PathOrText=\"C:\\list.txt\"";
        help += $"\n  Programs {Key.Programs.DnsLimit.Name} -{Key.Programs.DnsLimit.Enable}=True -{Key.Programs.DnsLimit.DisablePlain}=True -{Key.Programs.DnsLimit.DoHPathLimitMode.Name}=Text -PathOrText=\"dns-query\\nUserName1\"";
        await WriteToStdoutAsync(help);

        // DnsLimit Syntax Example
        help = $"\nDoH Path Limit File Syntax Example:";
        help += $"\n  dns-query";
        help += $"\n  Username1";
        help += $"\n  userX";
        await WriteToStdoutAsync(help);
    }

    private static async Task WriteToStdoutAsync(string msg, ConsoleColor consoleColor = ConsoleColor.White)
    {
        try
        {
            Console.ForegroundColor = consoleColor;
            List<string> lines = msg.ReplaceLineEndings().Split(Environment.NewLine).ToList();
            foreach (string line in lines)
                await Console.Out.WriteLineAsync(line);
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Help WriteToStdout: " + ex.Message);
        }
    }
}