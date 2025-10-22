using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace ConsoleAppTest;

internal class Program
{
    static async Task Main(string[] args)
    {
        // This Page Has Nothing To Do With The Project, I Just Test Some Stuff Here.

        //// Test HTTP Proxy
        //string website = "https://google.com";
        //string proxy = "http://216.205.52.246:80";

        //HttpRequest hr = new()
        //{
        //    URI = new Uri(website, UriKind.Absolute),
        //    Method = HttpMethod.Get,
        //    TimeoutMS = 20000,
        //    AllowInsecure = true, // Ignore Cert Check To Make It Faster
        //    AllowAutoRedirect = true,
        //    ProxyScheme = proxy
        //};

        //HttpRequestResponse hrr = await HttpRequest.SendAsync(hr).ConfigureAwait(false);
        //Console.WriteLine("========> " + hrr.StatusDescription);
        //Console.ReadLine();
        //return;


        //// Test TcpOverUdp
        //string tcp = "8.8.8.8:53";
        //var ttcp = await GetIP.GetIpFromDnsAddressAsync("google.com", tcp, false, 9, false, IPAddress.Parse("8.8.8.8"), 53);
        //Console.WriteLine("========> " + ttcp);
        //Console.ReadLine();
        //return;


        //// Test DoH TLSv1.3
        //string doh = "https://protective.joindns4.eu/dns-query";
        //var tt = await GetIP.GetIpFromDnsAddressAsync("google.com", doh, false, 9, false, IPAddress.Parse("8.8.8.8"), 53);
        //Console.WriteLine("========> " + tt);
        //Console.ReadLine();
        //return;


        //// Create DNS List (Stamp Decode)
        //List<string> urlsOrFiles = new()
        //{
        //    "https://github.com/DNSCrypt/dnscrypt-resolvers/blob/master/v3/public-resolvers.md",
        //    "https://github.com/curl/curl/wiki/DNS-over-HTTPS",
        //    "https://adguard-dns.io/kb/general/dns-providers",
        //    "https://github.com/NiREvil/vless/blob/main/DNS%20over%20HTTPS/any%20DNS-over-HTTPS%20server%20you%20want.md",
        //    "https://dns.sb/doh"
        //};

        //List<string> allDNSs = new();
        //for (int n = 0; n < urlsOrFiles.Count; n++)
        //{
        //    string urlOrFile = urlsOrFiles[n];
        //    List<string> dnss = await DnsTools.GetServersFromLinkAsync(urlOrFile, 20000);
        //    Debug.WriteLine($"{dnss.Count} <= {urlOrFile}");
        //    allDNSs.AddRange(dnss);
        //}

        //allDNSs = allDNSs.Distinct().ToList();
        //List<string> allDoHsByIP = new();
        //for (int n = 0; n < allDNSs.Count; n++)
        //{
        //    string dns = allDNSs[n];
        //    DnsReader dnsReader = new(dns);
        //    if (dnsReader.Protocol == DnsEnums.DnsProtocol.DoH)
        //    {
        //        if (dnsReader.IsDnsCryptStamp)
        //        {
        //            if (!NetworkTool.IsLocalIP(dnsReader.StampReader.IP.ToString()))
        //            {
        //                if (NetworkTool.IsIPv4(dnsReader.StampReader.IP))
        //                {
        //                    string dns_URL = $"{dnsReader.Scheme}{dnsReader.StampReader.IP}{dnsReader.Path}";
        //                    if (dnsReader.Port != 443) dns_URL = $"{dnsReader.Scheme}{dnsReader.StampReader.IP}:{dnsReader.Port}{dnsReader.Path}";
        //                    Debug.WriteLine(n);
        //                    allDoHsByIP.Add(dns_URL);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            IPAddress ip = await GetIP.GetIpFromDnsAddressAsync(dnsReader.Host, "udp://127.0.0.1", true, 10, false, IPAddress.None, 0);
        //            if (!NetworkTool.IsLocalIP(ip.ToString()))
        //            {
        //                string dns_URL = $"{dnsReader.Scheme}{ip}{dnsReader.Path}";
        //                if (dnsReader.Port != 443) dns_URL = $"{dnsReader.Scheme}{ip}:{dnsReader.Port}{dnsReader.Path}";
        //                Debug.WriteLine(n);
        //                allDoHsByIP.Add(dns_URL);
        //            }
        //        }
        //    }
        //}

        //Debug.WriteLine($"TOTAL: {allDoHsByIP.Count}{Environment.NewLine}");

        //for (int n = 0; n < allDoHsByIP.Count; n++)
        //{
        //    string dohByIP = allDoHsByIP[n];
        //    Debug.WriteLine(dohByIP);
        //}

        //return;


        // Test Server
        MsmhAgnosticServer server1 = new();

        server1.OnRequestReceived += Server_OnRequestReceived;
        server1.OnDebugInfoReceived += Server1_OnDebugInfoReceived;

        List<string> dnsServers1 = new()
        {
            //"sdns://AQMAAAAAAAAAEjEwMy44Ny42OC4xOTQ6ODQ0MyAxXDKkdrOao8ZeLyu7vTnVrT0C7YlPNNf6trdMkje7QR8yLmRuc2NyeXB0LWNlcnQuZG5zLmJlYmFzaWQuY29t",
            //"tls://free.shecan.ir",
            //"https://free.shecan.ir/dns-query",
            //"tls://dns.google",
            //"https://notworking.com/dns-query",
            //"udp://8.8.8.8:53",
            //"udp://1.1.1.1:53",
            //"udp://9.9.9.9:53",
            //"https://every1dns.com/dns-query",
            "https://dns.cloudflare.com/dns-query",
            //"https://dns.google/dns-query",
            //"https://45.90.29.204:443/dns-query",
            //"udp://208.67.222.222:5353",
            //"9.9.9.9:9953",
        };

        AgnosticSettings settings1 = new()
        {
            Working_Mode = AgnosticSettings.WorkingMode.DnsAndProxy,
            ListenerPort = 443,
            DnsTimeoutSec = 10,
            ProxyTimeoutSec = 0,
            MaxRequests = 1000000,
            KillOnCpuUsage = 40,
            DNSs = dnsServers1,
            BootstrapIpAddress = IPAddress.Any, // IPAddress.Parse("8.8.8.8")
            BootstrapPort = 53,
            AllowInsecure = false,
            BlockPort80 = false,
            //UpstreamProxyScheme = $"socks5://127.0.0.1:443", // http://fodev.org:8118
            ApplyUpstreamOnlyToBlockedIps = false
        };

        AgnosticProgram.Fragment fragment = new();
        fragment.Set(AgnosticProgram.Fragment.Mode.Program, 50, AgnosticProgram.Fragment.ChunkMode.SNI, 5, 2, 3);
        server1.EnableFragment(fragment);

        AgnosticProgram.Rules rules = new(AgnosticProgram.Rules.Mode.Text)
        {
            RuleList = new()
            {
                new AgnosticProgram.Rules.Rule()
                {
                    Address = "dns.cloudflare.com",
                    FakeDnsIP = "104.16.132.229", // 104.16.132.229 - 146.112.61.106
                    //Dnss = new()
                    //{
                    //    "https://dns.cloudflare.com/dns-query"
                    //},
                    //DnsProxyScheme = "socks5://127.0.0.1:443"
                },
                //new AgnosticProgram.Rules.Rule()
                //{
                //    Address = IPAddress.Loopback.ToString(),
                //    IsBlock = true,
                //},
                //new AgnosticProgram.Rules.Rule()
                //{
                //    Address = IPAddress.IPv6Loopback.ToStringNoScopeId(),
                //    IsBlock = true,
                //},
                new AgnosticProgram.Rules.Rule()
                {
                    Address = "*",
                    //Dnss = new()
                    //{
                    //    "https://dns.cloudflare.com/dns-query"
                    //},
                    DnsProxyScheme = "socks5://127.0.0.1:443"
                },
            }
        };
        server1.EnableRules(rules);
        
        AgnosticSettingsSSL settingsSSL = new(true)
        {
            ChangeSni = true,
            DefaultSni = "speedtest.net"
        };
        //await server1.EnableSSLAsync(settingsSSL);

        await server1.StartAsync(settings1);

        //await Task.Delay(TimeSpan.FromSeconds(90));
        //server1.Stop();
        //await Task.Delay(TimeSpan.FromSeconds(10));
        //await server1.StartAsync(settings1);


        Console.ReadLine();
    }

    private static void Server_OnRequestReceived(object? sender, EventArgs e)
    {
        if (sender is not string msg) return;
        Console.WriteLine($"Request: {msg}");
    }

    private static void Server1_OnDebugInfoReceived(object? sender, EventArgs e)
    {
        if (sender is not string msg) return;
        Console.WriteLine($"Debug: {msg}");
    }

}