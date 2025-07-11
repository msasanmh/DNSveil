using System.Diagnostics;
using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

public partial class AgnosticProgram
{
    public partial class Rules
    {
        public class RulesResult
        {
            public bool IsMatch { get; set; } = false;
            public bool IsBlackList { get; set; } = false;
            public bool IsPortBlock { get; set; } = false;
            public bool IsDirect { get; set; } = false; // No Fragment, No Fake SNI & No Upstream
            public string DnsCustomDomain { get; set; } = string.Empty;
            public string Dns { get; set; } = string.Empty;
            public List<string> Dnss { get; set; } = new();
            public string Sni { get; set; } = string.Empty;
            public bool ApplyUpStreamProxy { get; set; } = false;
            public string ProxyScheme { get; set; } = string.Empty;
            public bool ApplyUpStreamProxyToBlockedIPs { get; set; } = false;
            public string ProxyUser { get; set; } = string.Empty;
            public string ProxyPass { get; set; } = string.Empty;
        }

        public async Task<RulesResult> GetAsync(string client, string host, int port, AgnosticSettings settings)
        {
            RulesResult rr = new();
            if (string.IsNullOrEmpty(host)) return rr;

            try
            {
                rr.Dns = host;

                for (int n = 0; n < MainRules_List.Count; n++)
                {
                    MainRules mr = MainRules_List[n];

                    // Check If Match
                    bool isClientMatch = !string.IsNullOrEmpty(mr.Client) && (mr.Client.Equals(Rules_Init.KEYS.AllClients) || client.Equals(mr.Client) || client.EndsWith(mr.Client));
                    if (!isClientMatch) continue;

                    bool isWildcard = false;
                    string hostNoWww = string.Empty, ruleHostNoWww = string.Empty;
                    if (mr.AddressType == AddressType.Domain)
                    {
                        bool isDomainMatch = Rules_Init.IsDomainMatch(host, mr.Address, out isWildcard, out hostNoWww, out ruleHostNoWww);
                        if (!isDomainMatch) continue;
                    }
                    else if (mr.AddressType == AddressType.IP)
                    {
                        bool isHostIP = NetworkTool.IsIP(host, out _);
                        if (!isHostIP) continue;
                        bool isIpMatch = host.Equals(mr.Address);
                        if (!isIpMatch) continue;
                    }
                    else if (mr.AddressType == AddressType.CIDR)
                    {
                        bool isHostIP = NetworkTool.IsIP(host, out _);
                        if (!isHostIP) continue;
                        bool isCidrMatch = NetworkTool.IsIpInRange(host, mr.Address);
                        if (!isCidrMatch) continue;
                    }
                    else continue;

                    // Set Match
                    rr.IsMatch = true;

                    // Is Black List
                    rr.IsBlackList = mr.IsBlock;
                    if (rr.IsBlackList) break;

                    // Is Port Block
                    List<int> blockedPorts = mr.BlockPort.ToList();
                    for (int i = 0; i < blockedPorts.Count; i++)
                    {
                        int blockedPort = blockedPorts[i];
                        if (port == blockedPort)
                        {
                            rr.IsPortBlock = true;
                            break;
                        }
                    }
                    if (rr.IsPortBlock) break;

                    // Direct: Don't Apply DPI Bypass (Fragment & Change SNI) & Upstream
                    rr.IsDirect = mr.IsDirect;
                    
                    // DNS
                    if (!string.IsNullOrEmpty(mr.FakeDns))
                    {
                        // Fake DNS (Domain => IP, IP => IP)
                        rr.Dns = mr.FakeDns;
                    }
                    else
                    {
                        if (mr.AddressType == AddressType.Domain)
                        {
                            // Get Dns Servers And Upstream Proxy
                            List<string> dnss = mr.Dnss.Any() ? mr.Dnss : settings.DNSs;
                            rr.Dnss = dnss;
                            string? dnsProxyScheme = null, dnsProxyUser = null, dnsProxyPass = null;
                            if (!string.IsNullOrEmpty(mr.DnsProxyScheme))
                            {
                                dnsProxyScheme = mr.DnsProxyScheme;
                                dnsProxyUser = mr.DnsProxyUser;
                                dnsProxyPass = mr.DnsProxyPass;
                            }
                            else
                            {
                                dnsProxyScheme = settings.UpstreamProxyScheme;
                                dnsProxyUser = settings.UpstreamProxyUser;
                                dnsProxyPass = settings.UpstreamProxyPass;
                            }

                            // Get IP By Custom DNS
                            if (dnss.Any() && !NetworkTool.IsIP(host, out _))
                            {
                                // Get Custom DNS Domain
                                rr.DnsCustomDomain = host;
                                if (!string.IsNullOrEmpty(mr.DnsDomain))
                                {
                                    if (!mr.DnsDomain.StartsWith("*."))
                                    {
                                        rr.DnsCustomDomain = mr.DnsDomain;
                                    }
                                    else
                                    {
                                        // Support: xxxx.example.com -> xxxx.domain.com
                                        if (isWildcard) // ruleHostNoWww.StartsWith("*.")
                                        {
                                            if (hostNoWww.EndsWith(ruleHostNoWww[1..])) // Just In Case
                                            {
                                                rr.DnsCustomDomain = hostNoWww.Replace(ruleHostNoWww[1..], mr.DnsDomain[1..]);
                                            }
                                        }
                                    }
                                }

                                IPAddress ipAddr = IPAddress.None;
                                ipAddr = await GetIP.GetIpFromDnsAddressAsync(rr.DnsCustomDomain, dnss, settings.AllowInsecure, settings.DnsTimeoutSec, false, settings.BootstrapIpAddress, settings.BootstrapPort, dnsProxyScheme, dnsProxyUser, dnsProxyPass);
                                if (ipAddr.Equals(IPAddress.None) || ipAddr.Equals(IPAddress.IPv6None))
                                {
                                    ipAddr = await GetIP.GetIpFromDnsAddressAsync(rr.DnsCustomDomain, dnss, settings.AllowInsecure, settings.DnsTimeoutSec, true, settings.BootstrapIpAddress, settings.BootstrapPort, dnsProxyScheme, dnsProxyUser, dnsProxyPass);
                                }

                                if (!ipAddr.Equals(IPAddress.None) && !ipAddr.Equals(IPAddress.IPv6None))
                                {
                                    rr.Dns = ipAddr.ToString();
                                }
                            }
                        }
                    }

                    // Check If Host Is An IP
                    bool isIp = NetworkTool.IsIP(rr.Dns, out _);

                    // If IP Is Cloudflare IP, Set The Clean IP
                    if (isIp && !string.IsNullOrEmpty(settings.CloudflareCleanIP) && CommonTools.IsCfIP(rr.Dns))
                        rr.Dns = settings.CloudflareCleanIP;

                    // SNI
                    if (mr.AddressType == AddressType.Domain)
                    {
                        if (rr.IsDirect)
                        {
                            rr.Sni = host; // Set SNI To Original Host
                        }
                        else
                        {
                            rr.Sni = mr.Sni;
                            if (!string.IsNullOrEmpty(rr.Sni) && rr.Sni.StartsWith("*."))
                            {
                                // Support: xxxx.example.com -> xxxx.domain.com
                                if (isWildcard) // ruleHostNoWww.StartsWith("*.")
                                {
                                    if (hostNoWww.EndsWith(ruleHostNoWww[1..])) // Just In Case
                                    {
                                        rr.Sni = hostNoWww.Replace(ruleHostNoWww[1..], rr.Sni[1..]);
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(rr.Sni)) rr.Sni = host; // Set SNI To Original Host If Not Defined
                        }
                    }
                    
                    // Upstream Proxy
                    if (!string.IsNullOrEmpty(mr.ProxyScheme))
                    {
                        if (!rr.IsDirect) // If Not Direct
                        {
                            mr.ProxyScheme = mr.ProxyScheme.ToLower().Trim();
                            if (mr.ProxyScheme.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                mr.ProxyScheme.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                                mr.ProxyScheme.StartsWith("socks5://", StringComparison.OrdinalIgnoreCase))
                            {
                                rr.ApplyUpStreamProxy = true;
                                rr.ProxyScheme = mr.ProxyScheme;
                                rr.ApplyUpStreamProxyToBlockedIPs = mr.ProxyIfBlock;
                                rr.ProxyUser = mr.ProxyUser;
                                rr.ProxyPass = mr.ProxyPass;
                            }
                        }
                    }

                    // Break If Match
                    break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ProxyRules_GetAsync: " + ex.Message);
            }

            return rr;
        }

    }
}
