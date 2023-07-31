using MsmhTools.DnsTool;
using MsmhTools.ProxifiedTcpClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MsmhTools.HTTPProxyServer
{
    public partial class HTTPProxyServer
    {
        public partial class Program
        {
            //======================================= UpStream Proxy Support
            public class UpStreamProxy
            {
                public enum Mode
                {
                    HTTP,
                    SOCKS5,
                    Disable
                }

                public Mode UpStreamMode { get; private set; } = Mode.Disable;
                public string? ProxyHost { get; private set; }
                public int ProxyPort { get; private set; }
                public string? ProxyUsername { get; private set; }
                public string? ProxyPassword { get; private set; }
                public bool OnlyApplyToBlockedIps { get; set; }

                public UpStreamProxy() { }

                public void Set(Mode mode, string proxyHost, int proxyPort, bool onlyApplyToBlockedIps)
                {
                    //Set
                    UpStreamMode = mode;
                    ProxyHost = proxyHost;
                    ProxyPort = proxyPort;
                    OnlyApplyToBlockedIps = onlyApplyToBlockedIps;
                }

                public void Set(Mode mode, string proxyHost, int proxyPort, string proxyUsername, string proxyPassword, bool onlyApplyToBlockedIps)
                {
                    //Set
                    UpStreamMode = mode;
                    ProxyHost = proxyHost;
                    ProxyPort = proxyPort;
                    ProxyUsername = proxyUsername;
                    ProxyPassword = proxyPassword;
                    OnlyApplyToBlockedIps = onlyApplyToBlockedIps;
                }

                /// <summary>
                /// Connect TcpClient through Proxy
                /// </summary>
                /// <param name="destHostname"></param>
                /// <param name="destHostPort"></param>
                /// <returns>Returns null if cannot connect to proxy</returns>
                internal TcpClient? Connect(string destHostname, int destHostPort)
                {
                    if (string.IsNullOrEmpty(ProxyHost)) return null;

                    if (UpStreamMode == Mode.HTTP)
                    {
                        HttpProxyClient httpProxyClient = new(ProxyHost, ProxyPort, ProxyUsername, ProxyPassword);
                        return httpProxyClient.CreateConnection(destHostname, destHostPort);
                    }
                    else if (UpStreamMode == Mode.SOCKS5)
                    {
                        Socks5ProxyClient socks5ProxyClient = new(ProxyHost, ProxyPort, ProxyUsername, ProxyPassword);
                        return socks5ProxyClient.CreateConnection(destHostname, destHostPort);
                    }
                    else
                        return null;
                }
            }

            //======================================= DNS Support
            public class Dns
            {
                public enum Mode
                {
                    System,
                    DoH,
                    PlainDNS,
                    Disable
                }

                public Mode DNSMode { get; private set; } = Mode.Disable;
                public string? DNS { get; private set; }
                public string? DnsCleanIp { get; private set; }
                public int Timeout { get; private set; } = 10;
                public bool ChangeCloudflareIP { get; set; } = false;

                /// <summary>
                /// Only for DoH Mode
                /// </summary>
                public string? ProxyScheme { get; private set; }
                public string? Host { get; protected set; }
                private string? CloudflareCleanIP { get; set; }
                private List<string> CloudflareIPs { get; set; } = new();

                public Dns() { }

                public void Set(Mode mode, string? dns, string? dnsCleanIp, int timeoutSec, string? proxyScheme = null)
                {
                    // Set
                    DNSMode = mode;
                    DNS = dns;
                    DnsCleanIp = dnsCleanIp ?? string.Empty;
                    Timeout = timeoutSec;
                    ProxyScheme = proxyScheme;

                    if (DNSMode == Mode.Disable) return;
                    if (string.IsNullOrEmpty(dns)) return;

                    // Get Host
                    string host = dns;
                    if (DNSMode == Mode.DoH)
                    {
                        if (host.StartsWith("https://")) host = host[8..];
                        if (host.Contains('/'))
                        {
                            string[] split = host.Split('/');
                            host = split[0];
                        }
                    }
                    else if (DNSMode == Mode.PlainDNS)
                    {
                        if (host.Contains(':'))
                        {
                            string[] split = host.Split(':');
                            host = split[0];
                        }
                    }
                    Host = host;
                }

                /// <summary>
                /// Redirect all Cloudflare IPs to a clean IP
                /// </summary>
                /// <param name="cfCleanIP">CF Clean IP</param>
                /// <param name="cfIpRange">e.g. 103.21.244.0 - 103.21.244.255\n198.41.128.0 - 198.41.143.255</param>
                public void SetCloudflareIPs(string cfCleanIP, string? cfIpRange = null)
                {
                    if (!string.IsNullOrEmpty(cfIpRange))
                        cfIpRange += Environment.NewLine;
                    ChangeCloudflareIP = true;
                    CloudflareCleanIP = cfCleanIP;

                    // Built-in CF IPs
                    string defaultCfIPs = "103.21.244.0 - 103.21.244.255\n";
                    defaultCfIPs += "103.22.200.0 - 103.22.200.255\n";
                    defaultCfIPs += "103.31.4.0 - 103.31.5.255\n";
                    defaultCfIPs += "104.16.0.0 - 104.31.255.255\n";
                    defaultCfIPs += "108.162.192.0 - 108.162.207.255\n";
                    defaultCfIPs += "131.0.72.0 - 131.0.75.255\n";
                    defaultCfIPs += "141.101.64.0 - 141.101.65.255\n";
                    defaultCfIPs += "162.158.0.0 - 162.158.3.255\n";
                    defaultCfIPs += "172.64.0.0 - 172.67.255.255\n";
                    defaultCfIPs += "173.245.48.0 - 173.245.48.255\n";
                    defaultCfIPs += "188.114.96.0 - 188.114.99.255\n";
                    defaultCfIPs += "190.93.240.0 - 190.93.243.255\n";
                    defaultCfIPs += "197.234.240.0 - 197.234.243.255\n";
                    defaultCfIPs += "198.41.128.0 - 198.41.143.255";

                    if (string.IsNullOrEmpty(cfIpRange) || string.IsNullOrWhiteSpace(cfIpRange))
                        CloudflareIPs = defaultCfIPs.SplitToLines();
                    else
                        CloudflareIPs = cfIpRange.SplitToLines();
                }

                private bool IsCfIP(string ipString)
                {
                    try
                    {
                        string[] ips = ipString.Split('.');
                        int ip1 = int.Parse(ips[0]);
                        int ip2 = int.Parse(ips[1]);
                        int ip3 = int.Parse(ips[2]);
                        int ip4 = int.Parse(ips[3]);
                        
                        for (int n = 0; n < CloudflareIPs.Count; n++)
                        {
                            string ipRange = CloudflareIPs[n].Trim();
                            
                            if (!string.IsNullOrEmpty(ipRange))
                            {
                                string[] split = ipRange.Split('-');
                                string ipMin = split[0].Trim();
                                string ipMax = split[1].Trim();
                                
                                string[] ipMins = ipMin.Split('.');
                                int ipMin1 = int.Parse(ipMins[0]);
                                int ipMin2 = int.Parse(ipMins[1]);
                                int ipMin3 = int.Parse(ipMins[2]);
                                int ipMin4 = int.Parse(ipMins[3]);

                                string[] ipMaxs = ipMax.Split('.');
                                int ipMax1 = int.Parse(ipMaxs[0]);
                                int ipMax2 = int.Parse(ipMaxs[1]);
                                int ipMax3 = int.Parse(ipMaxs[2]);
                                int ipMax4 = int.Parse(ipMaxs[3]);

                                if (ip1 >= ipMin1 && ip1 <= ipMax1)
                                    if (ip2 >= ipMin2 && ip2 <= ipMax2)
                                        if (ip3 >= ipMin3 && ip3 <= ipMax3)
                                            if (ip4 >= ipMin4 && ip4 <= ipMax4)
                                                return true;
                            }
                        }
                        return false;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }

                internal async Task<string> Get(string destHostname)
                {
                    if (string.IsNullOrEmpty(destHostname)) return string.Empty;

                    // Don't resolve current Dns to avoid loop
                    if (destHostname.Equals(Host)) return destHostname;
                    if (destHostname.Equals(DnsCleanIp)) return destHostname;

                    // Get
                    if (DNSMode == Mode.System)
                    {
                        string ipString = GetIP.GetIpFromSystem(destHostname, false);
                        if (!ChangeCloudflareIP)
                            return string.IsNullOrEmpty(ipString) ? destHostname : ipString;
                        else
                        {
                            if (string.IsNullOrEmpty(ipString)) return destHostname;
                            else
                            {
                                return IsCfIP(ipString) ? CloudflareCleanIP ?? ipString : ipString;
                            }
                        }
                    }
                    else if (DNSMode == Mode.DoH)
                    {
                        if (string.IsNullOrEmpty(DNS)) return string.Empty;

                        string ipString = await GetIP.GetIpFromDoH(destHostname, DNS, Timeout, ProxyScheme);
                        if (!ChangeCloudflareIP)
                            return string.IsNullOrEmpty(ipString) ? destHostname : ipString;
                        else
                        {
                            if (string.IsNullOrEmpty(ipString)) return destHostname;
                            else
                            {
                                return IsCfIP(ipString) ? CloudflareCleanIP ?? ipString : ipString;
                            }
                        }
                    }
                    else if (DNSMode == Mode.PlainDNS)
                    {
                        if (string.IsNullOrEmpty(DNS)) return string.Empty;

                        string plainDnsIP = DNS;
                        int plainDnsPort = 53;

                        if (DNS.Contains(':'))
                        {
                            string[] dnsIpPort = DNS.Split(':');
                            plainDnsIP = dnsIpPort[0];
                            plainDnsPort = int.Parse(dnsIpPort[1]);
                        }

                        string ipString = GetIP.GetIpFromPlainDNS(destHostname, plainDnsIP, plainDnsPort, Timeout);
                        if (!ChangeCloudflareIP)
                            return string.IsNullOrEmpty(ipString) ? destHostname : ipString;
                        else
                        {
                            if (string.IsNullOrEmpty(ipString)) return destHostname;
                            else
                            {
                                return IsCfIP(ipString) ? CloudflareCleanIP ?? ipString : ipString;
                            }
                        }
                    }
                    else if (DNSMode == Mode.Disable) return destHostname;
                    else return destHostname;
                }
            }

            //======================================= Fake DNS Support
            public class FakeDns
            {
                public enum Mode
                {
                    File,
                    Text,
                    Disable
                }

                public Mode FakeDnsMode { get; private set; } = Mode.Disable;
                public string TextContent { get; private set; } = string.Empty;
                private List<string> HostIpList { get; set; } = new();

                public FakeDns() { }

                /// <summary>
                /// Set Fake DNS Database
                /// </summary>
                /// <param name="mode">Mode</param>
                /// <param name="filePathOrText">e.g. Each line: dns.google.com|8.8.8.8</param>
                public void Set(Mode mode, string filePathOrText)
                {
                    FakeDnsMode = mode;

                    if (FakeDnsMode == Mode.Disable) return;

                    if (FakeDnsMode == Mode.File)
                    {
                        try
                        {
                            TextContent = File.ReadAllText(Path.GetFullPath(filePathOrText));
                        }
                        catch (Exception)
                        {
                            // do nothing
                        }
                    }
                    else if (FakeDnsMode == Mode.Text)
                        TextContent = filePathOrText;

                    if (!string.IsNullOrEmpty(TextContent) || !string.IsNullOrWhiteSpace(TextContent))
                    {
                        TextContent += Environment.NewLine;
                        HostIpList = TextContent.SplitToLines();
                    }
                }

                internal string Get(string destHostname)
                {
                    string destHostnameNoWWW = destHostname;
                    if (destHostnameNoWWW.StartsWith("www."))
                        destHostnameNoWWW = destHostnameNoWWW.Replace("www.", string.Empty);

                    if (HostIpList.Any())
                    {
                        for (int n = 0; n < HostIpList.Count; n++)
                        {
                            string hostIP = HostIpList[n].Trim();
                            if (!string.IsNullOrEmpty(hostIP))
                                if (split(hostIP, out string destIP))
                                    return destIP;
                        }
                    }

                    return destHostname;

                    bool split(string hostIP, out string destIP)
                    {
                        // Add Support Comment //
                        if (hostIP.StartsWith("//"))
                        {
                            destIP = destHostname; return false;
                        }
                        else
                        {
                            if (hostIP.Contains('|'))
                            {
                                string[] split = hostIP.Split('|');
                                string host = split[0].Trim();
                                if (host.StartsWith("www."))
                                    host = host.Replace("www.", string.Empty);
                                string ip = split[1].Trim();

                                if (!host.StartsWith("*."))
                                {
                                    // No Wildcard
                                    if (destHostnameNoWWW.Equals(host))
                                    {
                                        destIP = ip; return true;
                                    }
                                    else
                                    {
                                        destIP = destHostname; return false;
                                    }
                                }
                                else
                                {
                                    // Wildcard
                                    string destMainHost = string.Empty;
                                    string[] splitByDot = destHostnameNoWWW.Split('.');

                                    if (splitByDot.Length >= 3)
                                    {
                                        host = host[2..];

                                        for (int n = 1; n < splitByDot.Length; n++)
                                            destMainHost += $"{splitByDot[n]}.";
                                        if (destMainHost.EndsWith('.')) destMainHost = destMainHost[0..^1];

                                        if (destMainHost.Equals(host))
                                        {
                                            destIP = ip;
                                            return true;
                                        }
                                        else
                                        {
                                            destIP = destHostname; return false;
                                        }
                                    }
                                    else
                                    {
                                        destIP = destHostname; return false;
                                    }
                                }
                            }
                            else
                            {
                                destIP = destHostname; return false;
                            }
                        }
                    }
                }
            }

            //======================================= Black White List Support
            public class BlackWhiteList
            {
                public enum Mode
                {
                    BlackListFile,
                    BlackListText,
                    WhiteListFile,
                    WhiteListText,
                    Disable
                }

                public Mode ListMode { get; private set; } = Mode.Disable;
                public string TextContent { get; private set; } = string.Empty;
                private List<string> BWList { get; set; } = new();

                public BlackWhiteList() { }

                /// <summary>
                /// Set Black White List Database
                /// </summary>
                /// <param name="mode">Mode</param>
                /// <param name="filePathOrText">e.g. Each line: google.com</param>
                public void Set(Mode mode, string filePathOrText)
                {
                    ListMode = mode;

                    if (ListMode == Mode.Disable) return;

                    if (ListMode == Mode.BlackListFile || ListMode == Mode.WhiteListFile)
                    {
                        try
                        {
                            TextContent = File.ReadAllText(Path.GetFullPath(filePathOrText));
                        }
                        catch (Exception)
                        {
                            // do nothing
                        }
                    }
                    else if (ListMode == Mode.BlackListText || ListMode == Mode.WhiteListText)
                        TextContent = filePathOrText;

                    if (!string.IsNullOrEmpty(TextContent) || !string.IsNullOrWhiteSpace(TextContent))
                    {
                        TextContent += Environment.NewLine;
                        BWList = TextContent.SplitToLines();
                    }
                }

                // If True Return, If false Continue
                internal bool IsMatch(string destHostname)
                {
                    string destHostnameNoWWW = destHostname;
                    if (destHostnameNoWWW.StartsWith("www."))
                        destHostnameNoWWW = destHostnameNoWWW.Replace("www.", string.Empty);

                    if (BWList.Any())
                    {
                        for (int n = 0; n < BWList.Count; n++)
                        {
                            string host = BWList[n].Trim();
                            if (!string.IsNullOrEmpty(host) && !host.StartsWith("//")) // Add Support Comment //
                            {
                                if (host.StartsWith("www."))
                                    host = host.Replace("www.", string.Empty);

                                // If Match
                                if (destHostnameNoWWW.Equals(host)) return match();
                            }
                        }
                    }

                    // If Not Match
                    return notMatch();

                    bool match()
                    {
                        return ListMode == Mode.BlackListFile || ListMode == Mode.BlackListText;
                    }

                    bool notMatch()
                    {
                        return ListMode == Mode.WhiteListFile || ListMode == Mode.WhiteListText;
                    }
                }
            }

            public class DontBypass
            {
                public enum Mode
                {
                    File,
                    Text,
                    Disable
                }

                public Mode DontBypassMode { get; private set; } = Mode.Disable;
                public string TextContent { get; private set; } = string.Empty;
                private List<string> DontBypassList { get; set; } = new();

                public DontBypass() { }

                /// <summary>
                /// Set DontBypass Database
                /// </summary>
                /// <param name="mode">Mode</param>
                /// <param name="filePathOrText">e.g. Each line: google.com</param>
                public void Set(Mode mode, string filePathOrText)
                {
                    DontBypassMode = mode;

                    if (DontBypassMode == Mode.Disable) return;

                    if (DontBypassMode == Mode.File)
                    {
                        try
                        {
                            TextContent = File.ReadAllText(Path.GetFullPath(filePathOrText));
                        }
                        catch (Exception)
                        {
                            // do nothing
                        }
                    }
                    else if (DontBypassMode == Mode.Text)
                        TextContent = filePathOrText;

                    if (!string.IsNullOrEmpty(TextContent) || !string.IsNullOrWhiteSpace(TextContent))
                    {
                        TextContent += Environment.NewLine;
                        DontBypassList = TextContent.SplitToLines();
                    }
                }

                // If True Don't Bypass, If false Bypass
                internal bool IsMatch(string destHostname)
                {
                    string destHostnameNoWWW = destHostname;
                    if (destHostnameNoWWW.StartsWith("www."))
                        destHostnameNoWWW = destHostnameNoWWW.Replace("www.", string.Empty);

                    if (DontBypassList.Any())
                    {
                        for (int n = 0; n < DontBypassList.Count; n++)
                        {
                            string host = DontBypassList[n].Trim();
                            if (!string.IsNullOrEmpty(host) && !host.StartsWith("//")) // Add Support Comment //
                            {
                                if (host.StartsWith("www."))
                                    host = host.Replace("www.", string.Empty);

                                // If Match
                                if (destHostnameNoWWW.Equals(host)) return true;
                            }
                        }
                    }

                    // If Not Match
                    return false;
                }

            }




        }
    }
    
}
