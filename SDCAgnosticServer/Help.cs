using System.Diagnostics;
using System.Reflection;

namespace SDCAgnosticServer;

public class Help
{
    public static void GetHelp()
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
        WriteToStdout(help);

        // Commands
        WriteToStdout("\nCommands:");

        // C
        help = $"\n{Key.Common.C}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Clear Screen And Stop Writing.";
        WriteToStdout(help);

        // CLS
        help = $"\n{Key.Common.CLS}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Clear Screen.";
        WriteToStdout(help);

        // OUT
        help = $"\n{Key.Common.Out} <ProfileName>";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Get Status In Machine Read.";
        WriteToStdout(help);

        // STATUS
        help = $"\n{Key.Common.Status}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Get Status In Human Read.";
        WriteToStdout(help);

        // Flush DNS
        help = $"\n{Key.Common.Flush}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Flush DNS Cache.";
        WriteToStdout(help);

        // Start
        help = $"\n{Key.Common.Start}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Start Server.";
        WriteToStdout(help);

        // Stop
        help = $"\n{Key.Common.Stop}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Stop Server.";
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
        help = $"  Modify Server Settings In Interactive Mode.";
        WriteToStdout(help);

        // Setting Command Mode
        // setting -Port=m -WorkingMode= -MaxRequests= -DnsTimeoutSec= -ProxyTimeoutSec= -KillOnCpuUsage= -BlockPort80=
        // -AllowInsecure= -DNSs= -CfCleanIP= -BootstrapIp= -BootstrapPort=
        // -ProxyScheme= -ProxyUser= -ProxyPass= -OnlyBlockedIPs=
        help = $"\n{Key.Setting.Name} <Option>";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Modify Server Settings In Command Mode.";
        WriteToStdout(help);

        // Option Of AgnosticSettings
        help = "\n  <Option>";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    -{Key.Setting.Port}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Server Listener Port.";
        WriteToStdout(help);

        help = $"    -{Key.Setting.MaxRequests}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Max Requests To Handle Per Second.";
        WriteToStdout(help);

        help = $"    -{Key.Setting.DnsTimeoutSec}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Limit TTL Of DNS Requests.";
        WriteToStdout(help);

        help = $"    -{Key.Setting.ProxyTimeoutSec}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Limit TTL Of Proxy Requests.";
        WriteToStdout(help);

        help = $"    -{Key.Setting.KillOnCpuUsage}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Kill All Requests If CPU Gets Higher Than Specified Value (Windows Only).";
        WriteToStdout(help);

        help = $"    -{Key.Setting.BlockPort80}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Block Port 80 To Avoid HSTS and Bad Cert Error.";
        WriteToStdout(help);

        help = $"    -{Key.Setting.AllowInsecure}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Allow Insecure (Ignore Certificate Errors When Connecting To DNS Servers).";
        WriteToStdout(help);

        help = $"    -{Key.Setting.DNSs}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      DNS Servers To Connect To (Comma Separated).";
        WriteToStdout(help);

        help = $"    -{Key.Setting.CfCleanIP}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Cloudflare CDN Clean IP To Redirect All Cloudflare IPs To This One.";
        WriteToStdout(help);

        help = $"    -{Key.Setting.BootstrapIp}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Bootstrap IPv4 (A Plain DNS).";
        WriteToStdout(help);

        help = $"    -{Key.Setting.BootstrapPort}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Bootstrap Port.";
        WriteToStdout(help);

        help = $"    -{Key.Setting.ProxyScheme}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Upstream Proxy Scheme.";
        WriteToStdout(help);

        help = $"    -{Key.Setting.ProxyUser}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Upstream Proxy Username.";
        WriteToStdout(help);

        help = $"    -{Key.Setting.ProxyPass}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Upstream Proxy Pass.";
        WriteToStdout(help);

        help = $"    -{Key.Setting.OnlyBlockedIPs}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Use Upstream Proxy Only For Blocked Servers.";
        WriteToStdout(help);

        // Example Of Setting
        help = $"\n  Example:";
        help += $"\n    Setting -Port=8080 -MaxRequests=500 -ProxyTimeoutSec=40 -KillOnCpuUsage=35.6 -BlockPort80=True";
        WriteToStdout(help);

        // SSLSetting Interactive Mode
        help = $"\n{Key.SSLSetting.Name}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Modify SSL AgnosticSettings In Interactive Mode.";
        WriteToStdout(help);

        // SSLSetting Command Mode - sslsetting -Enable=m -RootCA_Path="" -RootCA_KeyPath="" -Cert_Path="" -Cert_KeyPath="" -ChangeSni= -DefaultSni=
        help = $"\n{Key.SSLSetting.Name} <Option>";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Modify SSL AgnosticSettings In Command Mode.";
        WriteToStdout(help);

        // Option Of SSLSettings
        help = "\n  <Option>";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    -{Key.SSLSetting.Enable}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Use Certificate To Enable DoH/HTTPS Proxy And Decrypt SSL (True/False).";
        WriteToStdout(help);

        help = $"    -{Key.SSLSetting.RootCA_Path}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Path To Your Root Certificate File (e.g. C:\\RootCA.crt) - Leave Empty To Generate.";
        WriteToStdout(help);

        help = $"    -{Key.SSLSetting.RootCA_KeyPath}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Path To Your Root Certificate Private Key File. (e.g. C:\\RootCA.key).";
        WriteToStdout(help);

        help = $"    -{Key.SSLSetting.Cert_Path}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Path To Your Certificate File Issued With Your RootCA (e.g. C:\\Local.crt) - Leave Empty To Generate.";
        WriteToStdout(help);

        help = $"    -{Key.SSLSetting.Cert_KeyPath}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Path To Your Certificate Private Key File. (e.g. C:\\Local.key).";
        WriteToStdout(help);

        help = $"    -{Key.SSLSetting.ChangeSni}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Change SNI To Bypass DPI.";
        WriteToStdout(help);

        help = $"    -{Key.SSLSetting.DefaultSni}=";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Change All SNIs To This SNI (Leave Empty To Use Original SNI).";
        help += $"\n      You Can Also Use FakeSNI To Set A Custom SNI For A Domain.";
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

        help = $"    {Key.Programs.Fragment.Name}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Activate Fragment To Circumvent Censorship (Requires Dns To Be Set To A DoH).";
        WriteToStdout(help);

        help = $"    {Key.Programs.DnsRules.Name}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Set Per Domain DNS Rules. e.g. Block, FakeDNS, DNS, DnsDomain.";
        WriteToStdout(help);

        help = $"    {Key.Programs.ProxyRules.Name}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Set Per Domain Proxy Rules. e.g. Block, NoBypass, FakeDNS, DNS, FakeSNI, ETC.";
        WriteToStdout(help);

        help = $"\n  Type Help Program <ProgramName> To Get More Info.";
        WriteToStdout(help, ConsoleColor.DarkYellow);

        // KillAll
        help = $"\n{Key.Common.KillAll}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Kill All Requests.";
        WriteToStdout(help);

        // Write Requests
        help = $"\n{Key.Common.Requests} <Option>";
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
        help = $"\n{Key.Common.FragmentDetails} <Option>";
        WriteToStdout(help, ConsoleColor.Blue);

        help = "\n  <Option>";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    True";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Write Fragment Details To Stderr.";
        WriteToStdout(help);

        help = $"    False";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Stop Writing Chunk Details.";
        WriteToStdout(help);

        // Save
        help = $"\n{Key.Common.Save}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Save Commands To File.";
        WriteToStdout(help);

        // Load
        help = $"\n{Key.Common.Load}";
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
        msg = $"    {Key.Programs.Fragment.Name}\n";
        msg += $"    {Key.Programs.DnsRules.Name}\n";
        msg += $"    {Key.Programs.ProxyRules.Name}\n";
        WriteToStdout(msg, ConsoleColor.Cyan);
    }

    public static void GetHelpFragment()
    {
        // Help Program Fragment
        // Programs Fragment -Mode=m -BeforeSniChunks=m -ChunkMode=m -SniChunks=m -AntiPatternOffset=m -FragmentDelay=m
        string help;
        help = $"\nPrograms {Key.Programs.Fragment.Name}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Activate Fragment To Circumvent Censorship (Requires Dns To Be Set To A DoH).";
        WriteToStdout(help);

        // -Mode=
        help = "\n  -Mode=";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    {Key.Programs.Fragment.Mode.Program.Name}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      To Activate Fragmentation.";
        WriteToStdout(help);

        help = $"    {Key.Programs.Fragment.Mode.Disable}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      To Disable Fragmentation.";
        WriteToStdout(help);

        // -BeforeSniChunks=
        help = $"\n  -{Key.Programs.Fragment.Mode.Program.BeforeSniChunks}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    Number Of Buffer Chunks Before SNI Extension.";
        WriteToStdout(help);

        // -ChunkMode=
        help = $"\n  -{Key.Programs.Fragment.Mode.Program.ChunkMode.Name}=";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    {Key.Programs.Fragment.Mode.Program.ChunkMode.SNI}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Find SNI And Chunk Based On It.";
        WriteToStdout(help);

        help = $"    {Key.Programs.Fragment.Mode.Program.ChunkMode.SniExtension}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Find Sni Extension And Chunk Based On It.";
        WriteToStdout(help);

        help = $"    {Key.Programs.Fragment.Mode.Program.ChunkMode.AllExtensions}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Find All Extensions And Chunk Based On It.";
        WriteToStdout(help);

        // -SniChunks=
        help = $"\n  -{Key.Programs.Fragment.Mode.Program.SniChunks}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    Number Of SNI/SniExtension/AllExtensions Chunks.";
        WriteToStdout(help);

        // -AntiPatternOffset=
        help = $"\n  -{Key.Programs.Fragment.Mode.Program.AntiPatternOffset}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    Set An Offset To Chunks To Avoid Pattern Detection.";
        WriteToStdout(help);

        // -FragmentDelay=
        help = $"\n  -{Key.Programs.Fragment.Mode.Program.FragmentDelay}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    Set Delay Between Sending Chunks In Milliseconds.";
        WriteToStdout(help);

        // Examples
        help = $"\nExamples:";
        help += $"\n  Programs Fragment -Mode=Disable";
        help += $"\n  Programs Fragment -Mode=Program -BeforeSniChunks=50 -ChunkMode=SNI -SniChunks=5 -AntiPatternOffset=2 -FragmentDelay=1";
        help += $"\n  Programs Fragment -Mode=Program -BeforeSniChunks=50 -ChunkMode=AllExtensions -SniChunks=20 -AntiPatternOffset=2 -FragmentDelay=1";
        WriteToStdout(help);
    }

    public static void GetHelpDnsRules()
    {
        // Help Program DnsRules
        // Programs DnsRules -Mode=m -PathOrText="m"
        string help;
        help = $"\nPrograms {Key.Programs.DnsRules.Name}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Set Per Domain DnsRules. e.g. Block, FakeDNS, DNS, DnsDomain.";
        WriteToStdout(help);

        // -Mode=
        help = "\n  -Mode=";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    {Key.Programs.DnsRules.Mode.File}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Dns Rules As File.";
        WriteToStdout(help);

        help = $"    {Key.Programs.DnsRules.Mode.Text}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      Dns Rules As Text.";
        WriteToStdout(help);

        help = $"    {Key.Programs.DnsRules.Mode.Disable}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      To Disable {Key.Programs.DnsRules.Name}.";
        WriteToStdout(help);

        // -PathOrText=
        help = $"\n  -{Key.Programs.DnsRules.PathOrText}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    The File Path Or Text Depends On The Mode (Require Double Quotes).";
        help += $"\n    File Mode: The Path Of File. (Each Line One Rule).";
        help += $"\n    Text Mode: e.g. \"Google.com|8.8.8.8;\\nCloudflare.com|dns:tcp://8.8.8.8;\"";
        WriteToStdout(help);

        // Examples
        help = $"\nExamples:";
        help += $"\n  Programs Dns -Mode=Disable";
        help += $"\n  Programs Dns -Mode=File -PathOrText=\"C:\\list.txt\"";
        help += $"\n  Programs Dns -Mode=Text -PathOrText=\"Google.com|8.8.8.8\\nCloudflare.com|1.1.1.1\"";
        WriteToStdout(help);

        // DnsRules Syntax Example
        help = $"\nDnsRules Syntax Example:";
        help += $"\n  instagram.com|163.70.128.174;";
        help += $"\n  youtube.com|dnsdomain:google.com;";
        help += $"\n  ytimg.com|dnsdomain:google.com;";
        help += $"\n  *.ytimg.com|dnsdomain:google.com;";
        help += $"\n  ggpht.com|dnsdomain:google.com;";
        help += $"\n  *.ggpht.com|dns:8.8.8.8:53;dnsdomain:*.googleusercontent.com;";
        help += $"\n  *.googleapis|dns:8.8.8.8:53;dnsdomain:google.com;";
        help += $"\n  *.googlevideo.com|dns:8.8.8.8:53;dnsdomain:*.c.docs.google.com;";
        help += $"\n\n  More Help: https://github.com/msasanmh/SecureDNSClient/tree/main/Help";
        WriteToStdout(help);
    }

    public static void GetHelpProxyRules()
    {
        // Help Program ProxyRules
        // Programs ProxyRules -Mode=m -PathOrText="m"
        string help;
        help = $"\nPrograms {Key.Programs.ProxyRules.Name}";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"  Set Per Domain ProxyRules. e.g. Block, NoBypass, FakeDNS, DNS, FakeSNI, ETC.";
        WriteToStdout(help);

        // -Mode=
        help = "\n  -Mode=";
        WriteToStdout(help, ConsoleColor.Blue);

        help = $"    {Key.Programs.ProxyRules.Mode.File}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      ProxyRules As File.";
        WriteToStdout(help);

        help = $"    {Key.Programs.ProxyRules.Mode.Text}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      ProxyRules As Text.";
        WriteToStdout(help);

        help = $"    {Key.Programs.ProxyRules.Mode.Disable}";
        WriteToStdout(help, ConsoleColor.Cyan);
        help = $"      To Disable {Key.Programs.ProxyRules.Name}.";
        WriteToStdout(help);

        // -PathOrText=
        help = $"\n  -{Key.Programs.ProxyRules.PathOrText}=";
        WriteToStdout(help, ConsoleColor.Blue);
        help = $"    The File Path Or Text Depends On The Mode (Require Double Quotes).";
        help += $"\n    File Mode: The Path Of File. (Each Line One Rule).";
        help += $"\n    Text Mode: e.g. \"Google.com|8.8.8.8;\\nCloudflare.com|dns:tcp://8.8.8.8;\"";
        WriteToStdout(help);

        // Examples
        help = $"\nExamples:";
        help += $"\n  Programs Dns -Mode=Disable";
        help += $"\n  Programs Dns -Mode=File -PathOrText=\"C:\\list.txt\"";
        help += $"\n  Programs Dns -Mode=Text -PathOrText=\"Google.com|8.8.8.8\\nCloudflare.com|1.1.1.1\"";
        WriteToStdout(help);

        // ProxyRules Syntax Example
        help = $"\nProxyRules Syntax Example:";
        help += $"\n  dns:8.8.8.8:53;";
        help += $"\n  instagram.com|163.70.128.174;sni:speedtest.com;";
        help += $"\n  youtube.com|dns:8.8.8.8:53;dnsdomain:google.com;sni:google.com;";
        help += $"\n  ytimg.com|dns:8.8.8.8:53;dnsdomain:google.com;";
        help += $"\n  *.ytimg.com|dns:8.8.8.8:53;dnsdomain:google.com;";
        help += $"\n  ggpht.com|dns:8.8.8.8:53;dnsdomain:google.com;";
        help += $"\n  *.ggpht.com|dns:8.8.8.8:53;dnsdomain:*.googleusercontent.com;";
        help += $"\n  *.googleapis|dns:8.8.8.8:53;dnsdomain:google.com;";
        help += $"\n  *.googlevideo.com|dns:8.8.8.8:53;dnsdomain:*.c.docs.google.com;sni:google.com;";
        help += $"\n\n  More Help: https://github.com/msasanmh/SecureDNSClient/tree/main/Help";
        WriteToStdout(help);
    }

    private static void WriteToStdout(string msg, ConsoleColor consoleColor = ConsoleColor.White)
    {
        try
        {
            Console.ForegroundColor = consoleColor;
            Console.Out.WriteLine(msg);
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Help WriteToStdout: " + ex.Message);
        }
    }
}