using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Net;

namespace ConsoleAppTest;

internal class Program
{
    static async Task Main(string[] args)
    {
        MsmhAgnosticServer server1 = new();
        MsmhAgnosticServer server2 = new();

        server1.OnRequestReceived += Server_OnRequestReceived;
        server2.OnRequestReceived += Server_OnRequestReceived;

        string dohUrl = "https://dns.cloudflare.com/dns-query";
        string dohHost = "dns.cloudflare.com";
        string dohCleanIP = "104.16.132.229";
        string cfClenIP = "172.66.192.140";

        // Set DnsRules Content
        string dnsRulesContent = $"{dohHost}|{dohCleanIP};";

        // Set ProxyRules Content (Apply Fake DNS and White List Program)
        string proxyRulesContent = $"{dohHost}|{dohCleanIP};";
        proxyRulesContent += $"\n{dohCleanIP}|+;";
        proxyRulesContent += $"\n*|-;"; // Block Other Requests

        List<string> dnsServers1 = new()
        {
            //"sdns://AQMAAAAAAAAAEjEwMy44Ny42OC4xOTQ6ODQ0MyAxXDKkdrOao8ZeLyu7vTnVrT0C7YlPNNf6trdMkje7QR8yLmRuc2NyeXB0LWNlcnQuZG5zLmJlYmFzaWQuY29t",
            //"tls://free.shecan.ir",
            //"https://free.shecan.ir/dns-query",
            //"tls://dns.google",
            //"https://notworking.com/dns-query",
            //"tcp://8.8.8.8:53",
            //"udp://8.8.8.8:53",
            //"tcp://1.1.1.1:53",
            //"https://every1dns.com/dns-query",
            //"https://dns.cloudflare.com/dns-query",
            //"https://dns.google/dns-query",
            //"https://45.90.29.204:443/dns-query",
            //"udp://208.67.222.222:5353",
            //"9.9.9.9:9953",
            dohUrl
        };

        AgnosticSettings settings1 = new()
        {
            Working_Mode = AgnosticSettings.WorkingMode.DnsAndProxy,
            ListenerPort = 53,
            DnsTimeoutSec = 10,
            ProxyTimeoutSec = 40,
            MaxRequests = 1000000,
            KillOnCpuUsage = 40,
            DNSs = dnsServers1,
            BootstrapIpAddress = IPAddress.Loopback,
            BootstrapPort = 53,
            AllowInsecure = false,
            BlockPort80 = true,
            CloudflareCleanIP = cfClenIP,
            UpstreamProxyScheme = $"socks5://{IPAddress.Loopback}:53",
            //ApplyUpstreamOnlyToBlockedIps = true
        };

        AgnosticProgram.Fragment fragment = new();
        fragment.Set(AgnosticProgram.Fragment.Mode.Program, 50, AgnosticProgram.Fragment.ChunkMode.SNI, 5, 2, 1);
        server1.EnableFragment(fragment);

        AgnosticProgram.DnsRules dnsRules1 = new();
        dnsRules1.Set(AgnosticProgram.DnsRules.Mode.Text, dnsRulesContent);
        server1.EnableDnsRules(dnsRules1);

        AgnosticProgram.ProxyRules proxyRules1 = new();
        proxyRules1.Set(AgnosticProgram.ProxyRules.Mode.Text, proxyRulesContent);
        server1.EnableProxyRules(proxyRules1);


        List<string> dnsServers2 = new()
        {
            $"udp://{IPAddress.Loopback}:53"
        };

        AgnosticSettings settings2 = new()
        {
            Working_Mode = AgnosticSettings.WorkingMode.Dns,
            ListenerPort = 443,
            DnsTimeoutSec = 10,
            DNSs = dnsServers2,
            MaxRequests = 1000000,
            BootstrapIpAddress = IPAddress.Loopback,
            BootstrapPort = 53,
            AllowInsecure = false,
            //UpstreamProxyScheme = "socks5://192.168.1.120:10808",
            //ApplyUpstreamOnlyToBlockedIps = true
        };

        AgnosticSettingsSSL settingsSSL = new(true)
        {
            EnableSSL = true,
            //ChangeSni = true,
            //DefaultSni = "speedtest.net",
        };
        
        await server2.EnableSSL(settingsSSL);



        server1.Start(settings1);
        server2.Start(settings2);

        

        DnsMessage dmQ1 = DnsMessage.CreateQuery(DnsEnums.DnsProtocol.UDP, "youtube.com", DnsEnums.RRType.A, DnsEnums.CLASS.IN);
        DnsMessage.TryWrite(dmQ1, out byte[] dmQBytes1);
        DnsMessage dmQ2 = DnsMessage.CreateQuery(DnsEnums.DnsProtocol.DoH, "mail.yahoo.com", DnsEnums.RRType.A, DnsEnums.CLASS.IN);
        DnsMessage.TryWrite(dmQ2, out byte[] dmQBytes2);
        string dns = "udp://127.0.0.1:53";
        string doh = "https://127.0.0.1:443/dns-query";
        //doh = "https://dns-cloudflare.com/dns-query";

        IPAddress bootIP = IPAddress.Parse("8.8.8.8");
        int bootPort = 53;



        HttpRequest httpRequest = new()
        {
            URI = new Uri("https://google.com"),
            ProxyScheme = "socks5://127.0.0.1:443",
            TimeoutMS = 5000
        };


        int n = 0;


        tt1();
        //tt1();
        //tt1();
        tt2();
        //tt2();
        //tt2();


        async void tt1()
        {
            while (true)
            {
                //await Task.Delay(50);
                n++;
                await DnsClient.QueryAsync(dmQBytes1, DnsEnums.DnsProtocol.UDP, dns, true, bootIP, bootPort, 1000, CancellationToken.None);
                //HttpRequest.SendAsync(httpRequest);
                //if (n == 5000) break;
            }
        }

        async void tt2()
        {
            while (true)
            {
                //await Task.Delay(50);
                n++;
                await DnsClient.QueryAsync(dmQBytes2, DnsEnums.DnsProtocol.DoH, doh, true, bootIP, bootPort, 1000, CancellationToken.None);
                //HttpRequest.SendAsync(httpRequest);
                if (n == 5000) break;
            }
        }


        Console.ReadLine();
    }

    private static void Server_OnRequestReceived(object? sender, EventArgs e)
    {
        if (sender is not string msg) return;
        Console.WriteLine(msg);
    }
}