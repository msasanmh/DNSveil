using System.Reflection;

namespace SDCProxyServer;

public class Help
{
    public static void GetHelp()
    {
        string help;

        // Title
        help = $"\nMsmh Proxy Server v{Assembly.GetExecutingAssembly().GetName().Version}, Author: msasanmh@gmail.com";
        help += "\nSupports:";
        help += "\n    HTTP (Domain, IPv4, IPv6) (Get, Post, etc)";
        help += "\n    HTTPS (Domain, IPv4, IPv6) (Post, etc)";
        help += "\n    SOCKS4 (IPv4) (Connect, Bind)";
        help += "\n    SOCKS4A (Domain, IPv4) (Connect, Bind)";
        help += "\n    SOCKS5 (Domain, IPv4, IPv6) (Connect, Bind, UDP)";
        help += "\n    SSL Decryption";
        WriteToStdout(help);

        // Commands
        WriteToStdout("\nCommands:");

        // C
        help = "\nc";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Clear Screen And Stop Writing.";
        WriteToStdout(help);

        // CLS
        help = "\ncls";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Clear Screen.";
        WriteToStdout(help);

        // OUT
        help = "\nout";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Get Status In Machine Read.";
        WriteToStdout(help);

        // STATUS
        help = "\nStatus";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Get Status In Human Read.";
        WriteToStdout(help);

        // Start
        help = "\nStart";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Start Proxy Server.";
        WriteToStdout(help);

        // Stop
        help = "\nStop";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Stop Proxy Server.";
        WriteToStdout(help);

        // Parent Process ID
        help = $"\n{Key.ParentProcess.Name} <Option>";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Exit When Parent Terminated.";
        WriteToStdout(help);

        // Option Of ParentProcess
        help = "\n  <Option>";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    -{Key.ParentProcess.PID}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      The ID Of The Parent Process. (Set To 0 To Disable)";
        WriteToStdout(help);

        // Setting Interactive Mode
        help = $"\n{Key.Setting.Name}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Modify Proxy Settings In Interactive Mode.";
        WriteToStdout(help);

        // Setting Command Mode - setting -Port=m -MaxThreads=m -RequestTimeoutSec=m -KillOnCpuUsage=m -BlockPort80=m
        help = $"\n{Key.Setting.Name} <Option>";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Modify Proxy Settings In Command Mode.";
        WriteToStdout(help);

        // Option Of Settings
        help = "\n  <Option>";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    -{Key.Setting.Port}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Proxy Server listener Port.";
        WriteToStdout(help);

        help = $"    -{Key.Setting.MaxRequests}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Max Requests To Handle Per Second.";
        WriteToStdout(help);

        help = $"    -{Key.Setting.RequestTimeoutSec}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Limit TTL Of Requests.";
        WriteToStdout(help);

        help = $"    -{Key.Setting.KillOnCpuUsage}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Kill All Requests If CPU Gets Higher Than Specified Value (Windows Only).";
        WriteToStdout(help);

        help = $"    -{Key.Setting.BlockPort80}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Block Port 80 To Avoid HSTS and Bad Cert Error.";
        WriteToStdout(help);

        // Example Of Setting
        help = $"\n  Example:";
        help += $"\n    Setting -Port=8080 -MaxRequests=500 -RequestTimeoutSec=40 -KillOnCpuUsage=35.6 -BlockPort80=True";
        WriteToStdout(help);

        // SSLSetting Interactive Mode
        help = $"\n{Key.SSLSetting.Name}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Modify SSL Settings In Interactive Mode.";
        WriteToStdout(help);

        // SSLSetting Command Mode - sslsetting -Enable=m -RootCA_Path="" -RootCA_KeyPath=""
        help = $"\n{Key.SSLSetting.Name} <Option>";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Modify SSL Settings In Command Mode.";
        WriteToStdout(help);

        // Option Of SSLSettings
        help = "\n  <Option>";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    -{Key.SSLSetting.Enable}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Use Self-Signed Certificate To Decrypt SSL (True/False).";
        WriteToStdout(help);

        help = $"    -{Key.SSLSetting.RootCA_Path}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Path To Your Root Certificate File (e.g. C:\\RootCA.crt) - Leave Empty To Generate.";
        WriteToStdout(help);

        help = $"    -{Key.SSLSetting.RootCA_KeyPath}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Path To Your Private Key File. (e.g. C:\\RootCA.key) - Leave Empty To Generate.";
        WriteToStdout(help);

        help = $"    -{Key.SSLSetting.ChangeSniToIP}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Change SNI To IP To Bypass DPI.";
        WriteToStdout(help);

        // Programs Interactive Mode
        help = $"\n{Key.Programs.Name}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Modify Programs (Plug-Ins) In Interactive Mode.";
        WriteToStdout(help);

        // Programs Command Mode
        help = $"\n{Key.Programs.Name} <Program>";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Modify Programs (Plug-Ins) In Command Mode.";
        WriteToStdout(help);

        // Programs
        help = "\n  <Program>";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    {Key.Programs.BwList.Name}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      A Black Or White List Of Domains.";
        WriteToStdout(help);

        help = $"    {Key.Programs.Dns.Name}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Resolve Domains Through Specified DNS Address.";
        WriteToStdout(help);

        help = $"    {Key.Programs.DontBypass.Name}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      A List Of Domains To Be Excluded From DPI Bypassing.";
        WriteToStdout(help);

        help = $"    {Key.Programs.DpiBypass.Name}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Activate DPI Bypass To Circumvent Censorship (Requires Dns To Be Set To A DoH).";
        WriteToStdout(help);

        help = $"    {Key.Programs.FakeDns.Name}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Resolve Domains To Desired IP Addresses.";
        WriteToStdout(help);

        help = $"    {Key.Programs.UpStreamProxy.Name}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Use Upstream Proxy To Resolve Domains.";
        WriteToStdout(help);

        help = $"\n  Type Help Program <ProgramName> To Get More Info.";
        WriteToStdout(help, ConsoleColor.DarkYellow);

        // KillAll
        help = "\nKillAll";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Kill All Requests.";
        WriteToStdout(help);

        // Write Requests
        help = "\nRequests <Option>";
        WriteToStdout(help, ConsoleColor.Blue);

        help = "\n  <Option>";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    True";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Write Requests To Stdout.";
        WriteToStdout(help);

        help = $"    False";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Stop Writing Requests.";
        WriteToStdout(help);

        // Write Chunk Details
        help = "\nChunkDetails <Option>";
        WriteToStdout(help, ConsoleColor.Blue);

        help = "\n  <Option>";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    True";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Write Chunk Details To Stderr.";
        WriteToStdout(help);

        help = $"    False";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Stop Writing Chunk Details.";
        WriteToStdout(help);

        // Save
        help = "\nSave";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Save Commands To File.";
        WriteToStdout(help);

        // Load
        help = "\nLoad";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Load Commands From File.";
        WriteToStdout(help);
    }

    public static void GetHelpPrograms()
    {
        string msg = "Help Program <Program>:\n";
        WriteToStdout(msg, ConsoleColor.Blue);
        msg = "  <Program>";
        WriteToStdout(msg, ConsoleColor.Blue);
        msg = $"    {Key.Programs.BwList.Name}\n";
        msg += $"    {Key.Programs.Dns.Name}\n";
        msg += $"    {Key.Programs.DontBypass.Name}\n";
        msg += $"    {Key.Programs.DpiBypass.Name}\n";
        msg += $"    {Key.Programs.FakeDns.Name}\n";
        msg += $"    {Key.Programs.UpStreamProxy.Name}\n";
        WriteToStdout(msg, ConsoleColor.Cyan);
    }

    public static void GetHelpBwList()
    {
        // Help Program BwList
        // Programs BwList -Mode=m -PathOrText="m"
        string help;
        help = $"\nPrograms {Key.Programs.BwList.Name}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  A Black Or White List Of Domains.";
        WriteToStdout(help);

        // -Mode=
        help = "\n  -Mode=";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    {Key.Programs.BwList.Mode.BlackListFile}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      A Black List File.";
        WriteToStdout(help);

        help = $"    {Key.Programs.BwList.Mode.BlackListText}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      A Black List As Text.";
        WriteToStdout(help);

        help = $"    {Key.Programs.BwList.Mode.WhiteListFile}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      A White List File.";
        WriteToStdout(help);

        help = $"    {Key.Programs.BwList.Mode.WhiteListText}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      A White List As Text.";
        WriteToStdout(help);

        help = $"    {Key.Programs.BwList.Mode.Disable}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      To Disable {Key.Programs.BwList.Name}.";
        WriteToStdout(help);

        // -PathOrText=
        help = "\n  -PathOrText=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    The File Path Or Text Depends On The Mode (Require Double Quotes).";
        help += $"\n    File Mode: The Path Of File. (Each Line One Domain).";
        help += $"\n    Text Mode: e.g. \"Domain1.com\\nDomain2.com\"";
        WriteToStdout(help);

        // Examples
        help = $"\nExamples:";
        help += $"\n  Programs BwList -Mode=Disable";
        help += $"\n  Programs BwList -Mode=BlackListFile -PathOrText=\"C:\\list.txt\"";
        help += $"\n  Programs BwList -Mode=WhiteListText -PathOrText=\"Domain1.com\\nDomain2.com\"";
        WriteToStdout(help);
    }

    public static void GetHelpDns()
    {
        // Help Program Dns
        // Programs Dns -Mode=m -TimeoutSec=m -DnsAddr=m -DnsCleanIp= -ProxyScheme= -CfCleanIp= -CfIpRange=""
        string help;
        help = $"\nPrograms {Key.Programs.Dns.Name}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Resolve Domains Through Specified DNS Address.";
        WriteToStdout(help);

        // -Mode=
        help = "\n  -Mode=";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    {Key.Programs.Dns.Mode.DoH}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Use A DoH To Resolve Domains.";
        WriteToStdout(help);

        help = $"    {Key.Programs.Dns.Mode.PlainDNS}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Use A Plain DNS To Resolve Domains.";
        WriteToStdout(help);

        help = $"    {Key.Programs.Dns.Mode.System}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Use System DNS To Resolve Domains.";
        WriteToStdout(help);

        help = $"    {Key.Programs.Dns.Mode.Disable}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      To Disable {Key.Programs.Dns.Name}.";
        WriteToStdout(help);

        // -TimeoutSec=
        help = $"\n  -{Key.Programs.Dns.TimeoutSec}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    Resolve Timeout In Seconds.";
        WriteToStdout(help);

        // -DnsAddr=
        help = $"\n  -{Key.Programs.Dns.DnsAddr}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    The DoH Or Plain DNS Address.";
        WriteToStdout(help);

        // -DnsCleanIp=
        help = $"\n  -{Key.Programs.Dns.DnsCleanIp}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    Set The DNS Clean IP (Optional).";
        WriteToStdout(help);

        // -ProxyScheme=
        help = $"\n  -{Key.Programs.Dns.ProxyScheme}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    Use A Proxy Server Only For DoH (Optional).";
        WriteToStdout(help);

        // -CfCleanIp=
        help = $"\n  -{Key.Programs.Dns.CfCleanIp}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    Set A Clean Cloudflare IP To Replace All CF IPs (Optional).";
        WriteToStdout(help);

        // -CfIpRange=
        help = $"\n  -{Key.Programs.Dns.CfIpRange}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    The Range Of Cloudflare IPs To Get Replaced (Require Double Quotes) (Optional - Leave Empty To Use Default IP Range).";
        WriteToStdout(help);

        // Examples
        help = $"\nExamples:";
        help += $"\n  Programs Dns -Mode=Disable";
        help += $"\n  Programs Dns -Mode=System -TimeoutSec=2";
        help += $"\n  Programs Dns -Mode=System -TimeoutSec=2 -CfCleanIp=103.21.244.5 -CfIpRange=\"103.21.244.0 - 103.21.244.255\\n103.22.200.0 - 103.22.200.255\"";
        help += $"\n  Programs Dns -Mode=DoH  -TimeoutSec=2 -DnsAddr=https://dns.cloudflare.com/dns-query";
        help += $"\n  Programs Dns -Mode=DoH  -TimeoutSec=2 -DnsAddr=https://dns.cloudflare.com/dns-query -ProxyScheme=socks5://myproxy.net:1080";
        help += $"\n  Programs Dns -Mode=PlainDNS -TimeoutSec=2 -DnsAddr=8.8.8.8:53";
        WriteToStdout(help);
    }

    public static void GetHelpDontBypass()
    {
        // Help Program DontBypass
        // Programs DontBypass -Mode=m -PathOrText="m"
        string help;
        help = $"\nPrograms {Key.Programs.DontBypass.Name}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  A List Of Domains To Be Excluded From DPI Bypassing (Fragmentation & SSL Decryption).";
        WriteToStdout(help);

        // -Mode=
        help = "\n  -Mode=";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    {Key.Programs.DontBypass.Mode.File}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      A DontBypass List File.";
        WriteToStdout(help);

        help = $"    {Key.Programs.DontBypass.Mode.Text}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      A DontBypass List As Text.";
        WriteToStdout(help);

        help = $"    {Key.Programs.DontBypass.Mode.Disable}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      To Disable {Key.Programs.DontBypass.Name}.";
        WriteToStdout(help);

        // -PathOrText=
        help = $"\n  -{Key.Programs.DontBypass.PathOrText}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    The File Path Or Text Depends On The Mode (Require Double Quotes).";
        help += $"\n    File Mode: The Path Of File. (Each Line One Domain).";
        help += $"\n    Text Mode: e.g. \"Domain1.com\\nDomain2.com\"";
        WriteToStdout(help);

        // Examples
        help = $"\nExamples:";
        help += $"\n  Programs DontBypass -Mode=Disable";
        help += $"\n  Programs DontBypass -Mode=File -PathOrText=\"C:\\list.txt\"";
        help += $"\n  Programs DontBypass -Mode=Text -PathOrText=\"Domain1.com\\nDomain2.com\"";
        WriteToStdout(help);
    }

    public static void GetHelpDpiBypass()
    {
        // Help Program DpiBypass
        // Programs DpiBypass -Mode=m -BeforeSniChunks=m -ChunkMode=m -SniChunks=m -AntiPatternOffset=m -FragmentDelay=m
        string help;
        help = $"\nPrograms {Key.Programs.DpiBypass.Name}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Activate DPI Bypass To Circumvent Censorship (Requires Dns To Be Set To A DoH).";
        WriteToStdout(help);

        // -Mode=
        help = "\n  -Mode=";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    {Key.Programs.DpiBypass.Mode.Program.Name}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      To Activate DPI Bypass.";
        WriteToStdout(help);

        help = $"    {Key.Programs.DpiBypass.Mode.Disable}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      To Disable DPI Bypass.";
        WriteToStdout(help);

        // -BeforeSniChunks=
        help = $"\n  -{Key.Programs.DpiBypass.Mode.Program.BeforeSniChunks}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    Number Of Buffer Chunks Before SNI Extension.";
        WriteToStdout(help);

        // -ChunkMode=
        help = $"\n  -{Key.Programs.DpiBypass.Mode.Program.ChunkMode.Name}=";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    {Key.Programs.DpiBypass.Mode.Program.ChunkMode.SNI}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Find SNI And Chunk Based On It.";
        WriteToStdout(help);

        help = $"    {Key.Programs.DpiBypass.Mode.Program.ChunkMode.SniExtension}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Find Sni Extension And Chunk Based On It.";
        WriteToStdout(help);

        help = $"    {Key.Programs.DpiBypass.Mode.Program.ChunkMode.AllExtensions}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Find All Extensions And Chunk Based On It.";
        WriteToStdout(help);

        // -SniChunks=
        help = $"\n  -{Key.Programs.DpiBypass.Mode.Program.SniChunks}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    Number Of SNI/SniExtension/AllExtensions Chunks.";
        WriteToStdout(help);

        // -AntiPatternOffset=
        help = $"\n  -{Key.Programs.DpiBypass.Mode.Program.AntiPatternOffset}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    Set An Offset To Chunks To Avoid Pattern Detection.";
        WriteToStdout(help);

        // -FragmentDelay=
        help = $"\n  -{Key.Programs.DpiBypass.Mode.Program.FragmentDelay}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    Set Delay Between Sending Chunks In Milliseconds.";
        WriteToStdout(help);

        // Examples
        help = $"\nExamples:";
        help += $"\n  Programs DpiBypass -Mode=Disable";
        help += $"\n  Programs DpiBypass -Mode=Program -BeforeSniChunks=50 -ChunkMode=SNI -SniChunks=5 -AntiPatternOffset=2 -FragmentDelay=1";
        help += $"\n  Programs DpiBypass -Mode=Program -BeforeSniChunks=50 -ChunkMode=AllExtensions -SniChunks=20 -AntiPatternOffset=2 -FragmentDelay=1";
        WriteToStdout(help);
    }

    public static void GetHelpFakeDns()
    {
        // Help Program FakeDns
        // Programs FakeDns -Mode=m -PathOrText="m"
        string help;
        help = $"\nPrograms {Key.Programs.FakeDns.Name}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Resolve Domains To Desired IP Addresses.";
        WriteToStdout(help);

        // -Mode=
        help = "\n  -Mode=";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    {Key.Programs.FakeDns.Mode.File}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      A FakeDns List File.";
        WriteToStdout(help);

        help = $"    {Key.Programs.FakeDns.Mode.Text}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      A FakeDns List As Text.";
        WriteToStdout(help);

        help = $"    {Key.Programs.FakeDns.Mode.Disable}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      To Disable {Key.Programs.FakeDns.Name}.";
        WriteToStdout(help);

        // -PathOrText=
        help = $"\n  -{Key.Programs.FakeDns.PathOrText}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    The File Path Or Text Depends On The Mode (Require Double Quotes).";
        help += $"\n    File Mode: The Path Of File. (Each Line One Rule).";
        help += $"\n    Text Mode: e.g. \"Google.com|8.8.8.8\\nCloudflare.com|1.1.1.1\"";
        WriteToStdout(help);

        // Examples
        help = $"\nExamples:";
        help += $"\n  Programs FakeDns -Mode=Disable";
        help += $"\n  Programs FakeDns -Mode=File -PathOrText=\"C:\\list.txt\"";
        help += $"\n  Programs FakeDns -Mode=Text -PathOrText=\"Google.com|8.8.8.8\\nCloudflare.com|1.1.1.1\"";
        WriteToStdout(help);
    }

    public static void GetHelpUpStreamProxy()
    {
        // Help Program UpStreamProxy
        // Programs UpStreamProxy -Mode=m -Host=m -Port=m -OnlyApplyToBlockedIPs=m
        string help;
        help = $"\nPrograms {Key.Programs.UpStreamProxy.Name}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Use Upstream Proxy To Resolve Domains.";
        WriteToStdout(help);

        // -Mode=
        help = "\n  -Mode=";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    {Key.Programs.UpStreamProxy.Mode.HTTP}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Use An HTTP Proxy As Upstream.";
        WriteToStdout(help);

        help = $"    {Key.Programs.UpStreamProxy.Mode.SOCKS5}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Use A SOCKS5 Proxy As Upstream.";
        WriteToStdout(help);

        help = $"    {Key.Programs.UpStreamProxy.Mode.Disable}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      To Disable {Key.Programs.UpStreamProxy.Name}.";
        WriteToStdout(help);

        // -Host=
        help = $"\n  -{Key.Programs.UpStreamProxy.Host}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    The Host Or IP Address Of Upstream Proxy.";
        WriteToStdout(help);

        // -Port=
        help = $"\n  -{Key.Programs.UpStreamProxy.Port}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    The Port Number Of Upstream Proxy.";
        WriteToStdout(help);

        // -OnlyApplyToBlockedIPs
        help = $"\n  -{Key.Programs.UpStreamProxy.OnlyApplyToBlockedIPs}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    Apply Upstream Proxy Only To Blocked IPs Or Websites (True Or False).";
        WriteToStdout(help);

        // Examples
        help = $"\nExamples:";
        help += $"\n  Programs UpStreamProxy -Mode=Disable";
        help += $"\n  Programs UpStreamProxy -Mode=HTTP -Host=127.0.0.1 -Port=8080 -OnlyApplyToBlockedIPs=True";
        help += $"\n  Programs UpStreamProxy -Mode=SOCKS5 -Host=myproxy.net -Port=1080 -OnlyApplyToBlockedIPs=False";
        WriteToStdout(help);
    }

    private static void WriteToStdout(string msg, ConsoleColor consoleColor = ConsoleColor.White)
    {
        Console.ForegroundColor = consoleColor;
        Console.Out.WriteLine(msg);
        Console.ResetColor();
    }
}