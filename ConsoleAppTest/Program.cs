using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Diagnostics;
using System.Net;

namespace ConsoleAppTest;

internal class Program
{
    static async Task Main(string[] args)
    {
        MsmhAgnosticServer server1 = new();

        server1.OnRequestReceived += Server_OnRequestReceived;

        List<string> dnsServers1 = new()
        {
            //"sdns://AQMAAAAAAAAAEjEwMy44Ny42OC4xOTQ6ODQ0MyAxXDKkdrOao8ZeLyu7vTnVrT0C7YlPNNf6trdMkje7QR8yLmRuc2NyeXB0LWNlcnQuZG5zLmJlYmFzaWQuY29t",
            //"tls://free.shecan.ir",
            //"https://free.shecan.ir/dns-query",
            //"tls://dns.google",
            //"https://notworking.com/dns-query",
            "tcp://8.8.8.8:53",
            //"udp://8.8.8.8:53",
            "tcp://1.1.1.1:53",
            //"https://every1dns.com/dns-query",
            //"https://dns.cloudflare.com/dns-query",
            //"https://dns.google/dns-query",
            //"https://45.90.29.204:443/dns-query",
            //"udp://208.67.222.222:5353",
            //"9.9.9.9:9953",
        };

        AgnosticSettings settings1 = new()
        {
            Working_Mode = AgnosticSettings.WorkingMode.DnsAndProxy,
            ListenerPort = 8080,
            DnsTimeoutSec = 10,
            ProxyTimeoutSec = 1000,
            MaxRequests = 1000000,
            KillOnCpuUsage = 40,
            DNSs = dnsServers1,
            BootstrapIpAddress = IPAddress.Parse("8.8.8.8"),
            BootstrapPort = 53,
            AllowInsecure = false,
            BlockPort80 = false,
            UpstreamProxyScheme = $"http://fodev.org:8118", // http://fodev.org:8118
            ApplyUpstreamOnlyToBlockedIps = false
        };

        AgnosticProgram.Fragment fragment = new();
        fragment.Set(AgnosticProgram.Fragment.Mode.Program, 50, AgnosticProgram.Fragment.ChunkMode.SNI, 5, 2, 1);
        server1.EnableFragment(fragment);

        await server1.StartAsync(settings1);



        Console.ReadLine();
    }

    private static void Server_OnRequestReceived(object? sender, EventArgs e)
    {
        if (sender is not string msg) return;
        Console.WriteLine(msg);
    }
}