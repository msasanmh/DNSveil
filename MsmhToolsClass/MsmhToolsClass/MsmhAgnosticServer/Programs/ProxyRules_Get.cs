using System.Diagnostics;
using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

public partial class AgnosticProgram
{
    public partial class ProxyRules
    {
        public class ProxyRulesResult
        {
            public bool IsMatch { get; set; } = false;
            public bool IsBlackList { get; set; } = false;
            public bool IsPortBlock { get; set; } = false;
            public bool ApplyDpiBypass { get; set; } = true;
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

        public async Task<ProxyRulesResult> GetAsync(string client, string host, int port, AgnosticSettings settings)
        {
            ProxyRulesResult prr = new();
            if (string.IsNullOrEmpty(host)) return prr;

            try
            {
                prr.Dns = host;

                for (int n = 0; n < MainRules_List.Count; n++)
                {
                    ProxyMainRules pmr = MainRules_List[n];

                    // Check If Match
                    bool isClientMatch = !string.IsNullOrEmpty(pmr.Client) && (pmr.Client.Equals(Rules.KEYS.AllClients) || client.Equals(pmr.Client) || client.EndsWith(pmr.Client));
                    bool isDomainMatch = Rules.IsDomainMatch(host, pmr.Domain, out bool isWildcard, out string hostNoWww, out string ruleHostNoWww);
                    bool isMatch = isClientMatch && isDomainMatch;
                    if (!isMatch) continue;
                    
                    // Set Match
                    prr.IsMatch = isMatch;

                    // Is Black List
                    prr.IsBlackList = pmr.IsBlock;
                    if (prr.IsBlackList) break;

                    // Is Port Block
                    List<int> blockedPorts = pmr.BlockPort.ToList();
                    for (int i = 0; i < blockedPorts.Count; i++)
                    {
                        int blockedPort = blockedPorts[i];
                        if (port == blockedPort)
                        {
                            prr.IsPortBlock = true;
                            break;
                        }
                    }
                    if (prr.IsPortBlock) break;

                    // Apply DPI Bypass (Fragment & Change SNI)
                    prr.ApplyDpiBypass = !pmr.NoBypass;
                    
                    // DNS
                    if (!string.IsNullOrEmpty(pmr.FakeDns))
                    {
                        // Fake DNS
                        prr.Dns = pmr.FakeDns;
                    }
                    else
                    {
                        // Get Dns Servers And Upstream Proxy
                        List<string> dnss = pmr.Dnss.Any() ? pmr.Dnss : settings.DNSs;
                        prr.Dnss = dnss;
                        string? dnsProxyScheme = null, dnsProxyUser = null, dnsProxyPass = null;
                        if (!string.IsNullOrEmpty(pmr.DnsProxyScheme))
                        {
                            dnsProxyScheme = pmr.DnsProxyScheme;
                            dnsProxyUser = pmr.DnsProxyUser;
                            dnsProxyPass = pmr.DnsProxyPass;
                        }
                        else
                        {
                            dnsProxyScheme = settings.UpstreamProxyScheme;
                            dnsProxyUser = settings.UpstreamProxyUser;
                            dnsProxyPass = settings.UpstreamProxyPass;
                        }

                        // Get IP By Custom DNS
                        if (dnss.Any() && !NetworkTool.IsIp(host, out _))
                        {
                            // Get Custom DNS Domain
                            prr.DnsCustomDomain = host;
                            if (!string.IsNullOrEmpty(pmr.DnsDomain))
                            {
                                if (!pmr.DnsDomain.StartsWith("*."))
                                {
                                    prr.DnsCustomDomain = pmr.DnsDomain;
                                }
                                else
                                {
                                    // Support: xxxx.example.com -> xxxx.domain.com
                                    if (isWildcard) // ruleHostNoWww.StartsWith("*.")
                                    {
                                        if (hostNoWww.EndsWith(ruleHostNoWww[1..])) // Just In Case
                                        {
                                            prr.DnsCustomDomain = hostNoWww.Replace(ruleHostNoWww[1..], pmr.DnsDomain[1..]);
                                        }
                                    }
                                }
                            }

                            IPAddress ipv4Addr = IPAddress.None;
                            if (settings.IsIPv4SupportedByISP)
                            {
                                ipv4Addr = await GetIP.GetIpFromDnsAddressAsync(prr.DnsCustomDomain, dnss, settings.AllowInsecure, settings.DnsTimeoutSec, false, settings.BootstrapIpAddress, settings.BootstrapPort, dnsProxyScheme, dnsProxyUser, dnsProxyPass);
                                if (ipv4Addr.Equals(IPAddress.None) && !settings.IsIPv6SupportedByISP) // Retry If IPv6 Is Not Supported
                                    ipv4Addr = await GetIP.GetIpFromDnsAddressAsync(prr.DnsCustomDomain, dnss, settings.AllowInsecure, settings.DnsTimeoutSec, false, settings.BootstrapIpAddress, settings.BootstrapPort, dnsProxyScheme, dnsProxyUser, dnsProxyPass);
                            }

                            if (ipv4Addr.Equals(IPAddress.None))
                            {
                                IPAddress ipv6Addr = await GetIP.GetIpFromDnsAddressAsync(prr.DnsCustomDomain, dnss, settings.AllowInsecure, settings.DnsTimeoutSec, true, settings.BootstrapIpAddress, settings.BootstrapPort, dnsProxyScheme, dnsProxyUser, dnsProxyPass);
                                if (!ipv6Addr.Equals(IPAddress.IPv6None))
                                    prr.Dns = ipv6Addr.ToString();
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(settings.CloudflareCleanIP))
                                    prr.Dns = ipv4Addr.ToString();
                                else
                                    prr.Dns = CommonTools.IsCfIP(ipv4Addr) ? settings.CloudflareCleanIP : ipv4Addr.ToString();
                            }
                        }
                    }

                    // SNI
                    prr.Sni = pmr.Sni;
                    if (!string.IsNullOrEmpty(prr.Sni) && prr.Sni.StartsWith("*."))
                    {
                        // Support: xxxx.example.com -> xxxx.domain.com
                        if (isWildcard) // ruleHostNoWww.StartsWith("*.")
                        {
                            if (hostNoWww.EndsWith(ruleHostNoWww[1..])) // Just In Case
                            {
                                prr.Sni = hostNoWww.Replace(ruleHostNoWww[1..], prr.Sni[1..]);
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(prr.Sni)) prr.Sni = host; // Set SNI To Original Host If Not Defined

                    // Upstream Proxy
                    if (!string.IsNullOrEmpty(pmr.ProxyScheme))
                    {
                        pmr.ProxyScheme = pmr.ProxyScheme.ToLower().Trim();
                        if (pmr.ProxyScheme.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                            pmr.ProxyScheme.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                            pmr.ProxyScheme.StartsWith("socks5://", StringComparison.OrdinalIgnoreCase))
                        {
                            prr.ApplyUpStreamProxy = true;
                            prr.ProxyScheme = pmr.ProxyScheme;
                            prr.ApplyUpStreamProxyToBlockedIPs = pmr.ProxyIfBlock;
                            prr.ProxyUser = pmr.ProxyUser;
                            prr.ProxyPass = pmr.ProxyPass;
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

            return prr;
        }

    }
}
