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

        public async Task<RulesResult> GetAsync(string client, string host, int port, AgnosticSettings settings, Endless endless, bool resolveDomain, bool useServerDns)
        {
            RulesResult rr = new();
            if (string.IsNullOrEmpty(host)) return rr;

            try
            {
                rr.Dns = host;

                for (int n = 0; n < RuleList.Count; n++)
                {
                    Rule rule = RuleList[n];

                    // Check If Match
                    bool isClientMatch = !string.IsNullOrEmpty(rule.Client) && (rule.Client.Equals(Rules_Init.KEYS.AllClients) || client.Equals(rule.Client) || client.EndsWith(rule.Client));
                    if (!isClientMatch) continue;

                    bool isWildcard = false;
                    string hostNoWww = string.Empty, ruleHostNoWww = string.Empty;
                    AddressType addressType = rule.AddressType;
                    if (addressType == AddressType.Domain)
                    {
                        bool isDomainMatch = Rules_Init.IsDomainMatch(host, rule.Address, out isWildcard, out hostNoWww, out ruleHostNoWww);
                        if (!isDomainMatch) continue;
                    }
                    else if (addressType == AddressType.IP)
                    {
                        bool isHostIP = NetworkTool.IsIP(host, out _);
                        if (!isHostIP) continue;
                        bool isIpMatch = host.Equals(rule.Address);
                        if (!isIpMatch) continue;
                    }
                    else if (addressType == AddressType.CIDR)
                    {
                        bool isHostIP = NetworkTool.IsIP(host, out _);
                        if (!isHostIP) continue;
                        bool isCidrMatch = NetworkTool.IsIpInRange(host, rule.Address);
                        if (!isCidrMatch) continue;
                    }
                    else continue;

                    // Set Match
                    rr.IsMatch = true;

                    // Is Black List
                    rr.IsBlackList = rule.IsBlock;
                    if (rr.IsBlackList) break;

                    // Is Port Block
                    List<int> blockedPorts = rule.BlockPort.ToList();
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
                    rr.IsDirect = rule.IsDirect;
                    
                    // DNS
                    if (NetworkTool.IsIP(rule.FakeDnsIP, out _))
                    {
                        // Fake DNS (Domain => IP, IP => IP)
                        rr.Dns = rule.FakeDnsIP;
                    }
                    else
                    {
                        if (addressType == AddressType.Domain)
                        {
                            // Get Dns Servers And Upstream Proxy
                            rr.Dnss = rule.Dnss.Any() ? rule.Dnss : settings.DNSs;
                            List<string> dnss = new(rr.Dnss); // Creating A New List Is Necessary Because Of dnss.Clear();

                            string? dnsProxyScheme = null, dnsProxyUser = null, dnsProxyPass = null;
                            if (!string.IsNullOrEmpty(rule.DnsProxyScheme))
                            {
                                dnsProxyScheme = rule.DnsProxyScheme;
                                dnsProxyUser = rule.DnsProxyUser;
                                dnsProxyPass = rule.DnsProxyPass;
                            }
                            else
                            {
                                dnsProxyScheme = settings.UpstreamProxyScheme;
                                dnsProxyUser = settings.UpstreamProxyUser;
                                dnsProxyPass = settings.UpstreamProxyPass;
                            }
                            
                            if (useServerDns) // By This Way We Cache Custom DNS Server Response And Make Proxy Faster.
                            {
                                // DNS Custom Servers Will Be Applied On The DNS-Server Side. (useServerDns Is False)
                                dnss.Clear();
                                dnss.Add(settings.ServerUdpDnsAddress);
                                
                                // DNS Upstream Proxy Will Be Applied On The DNS-Server Side. (Except UDP)
                                dnsProxyScheme = null;
                                dnsProxyUser = null;
                                dnsProxyPass = null;
                            }

                            // Get IP By Custom DNS
                            if (dnss.Any() && !NetworkTool.IsIP(host, out _))
                            {
                                // Get Custom DNS Domain
                                rr.DnsCustomDomain = host;
                                if (!string.IsNullOrEmpty(rule.DnsDomain))
                                {
                                    if (!rule.DnsDomain.StartsWith("*."))
                                    {
                                        rr.DnsCustomDomain = rule.DnsDomain;
                                    }
                                    else
                                    {
                                        // Support: xxxx.example.com -> xxxx.domain.com
                                        if (isWildcard) // ruleHostNoWww.StartsWith("*.")
                                        {
                                            if (hostNoWww.EndsWith(ruleHostNoWww[1..])) // Just In Case
                                            {
                                                rr.DnsCustomDomain = hostNoWww.Replace(ruleHostNoWww[1..], rule.DnsDomain[1..]);
                                            }
                                        }
                                    }
                                }

                                IPAddress bootstrap = settings.BootstrapIpAddress;
                                if (resolveDomain) // We Don't Need To Get A/AAAA Record For Other DNS Record Types. (TXT, CNAME, etc)
                                {
                                    // Avoid Endless Loop
                                    if (endless.IsUpstreamEqualToServerAddress(dnsProxyScheme))
                                    {
                                        if (dnss.IsContainPartial(rr.DnsCustomDomain)) // Allow DNS To Use Its Own Proxy Fragment
                                        {
                                            dnsProxyScheme = null;
                                            dnsProxyUser = null;
                                            dnsProxyPass = null;
                                        }
                                        else
                                        {
                                            // Can Stuck In An Endless Loop If
                                            // Upstream Is Equal To Server Address
                                            // And
                                            // A DNS In The List Doesn't Have A FakeDnsIP
                                            // Because Of DnsClient Upstream And Bootstrap TCP Upstream.
                                            // I Refuse To Detect It And Set ProxyScheme To NULL For The Sake Of Performance.
                                            // I Set Bootstrap To IPAddress.Any To Avoid Bootstrap TCP Upstream.
                                            bootstrap = IPAddress.Any;
                                        }
                                    }

                                    IPAddress ipAddr = IPAddress.None;
                                    ipAddr = await GetIP.GetIpFromDnsAddressAsync(rr.DnsCustomDomain, dnss, settings.AllowInsecure, settings.DnsTimeoutSec, false, bootstrap, settings.BootstrapPort, RuleList, dnsProxyScheme, dnsProxyUser, dnsProxyPass);
                                    if (ipAddr.Equals(IPAddress.None) || ipAddr.Equals(IPAddress.IPv6None))
                                    {
                                        ipAddr = await GetIP.GetIpFromDnsAddressAsync(rr.DnsCustomDomain, dnss, settings.AllowInsecure, settings.DnsTimeoutSec, true, bootstrap, settings.BootstrapPort, RuleList, dnsProxyScheme, dnsProxyUser, dnsProxyPass);
                                    }

                                    if (!ipAddr.Equals(IPAddress.None) && !ipAddr.Equals(IPAddress.IPv6None))
                                    {
                                        rr.Dns = ipAddr.ToStringNoScopeId();
                                    }
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
                    if (addressType == AddressType.Domain)
                    {
                        if (rr.IsDirect)
                        {
                            rr.Sni = host; // Set SNI To Original Host
                        }
                        else
                        {
                            rr.Sni = rule.Sni;
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
                    if (!string.IsNullOrEmpty(rule.ProxyScheme))
                    {
                        if (!rr.IsDirect) // If Not Direct
                        {
                            rule.ProxyScheme = rule.ProxyScheme.ToLower().Trim();
                            if (rule.ProxyScheme.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                rule.ProxyScheme.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                                rule.ProxyScheme.StartsWith("socks5://", StringComparison.OrdinalIgnoreCase))
                            {
                                rr.ApplyUpStreamProxy = true;
                                rr.ProxyScheme = rule.ProxyScheme;
                                rr.ApplyUpStreamProxyToBlockedIPs = rule.ProxyIfBlock;
                                rr.ProxyUser = rule.ProxyUser;
                                rr.ProxyPass = rule.ProxyPass;
                            }
                        }
                    }

                    // Break If Match
                    break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Rules_Get GetAsync: " + ex.Message);
            }

            return rr;
        }

    }
}
