using System.Diagnostics;
using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

public partial class AgnosticProgram
{
    public partial class DnsRules
    {
        public class DnsRulesResult
        {
            public bool IsMatch { get; set; } = false;
            public bool IsBlackList { get; set; } = false;
            public string DnsCustomDomain { get; set; } = string.Empty;
            public string Dns { get; set; } = string.Empty;
            public List<string> Dnss { get; set; } = new();
        }

        public async Task<DnsRulesResult> GetAsync(string client, string host, AgnosticSettings settings)
        {
            DnsRulesResult drr = new();
            if (string.IsNullOrEmpty(host)) return drr;

            try
            {
                for (int n = 0; n < MainRules_List.Count; n++)
                {
                    DnsMainRules dmr = MainRules_List[n];

                    // Check If Match
                    bool isClientMatch = !string.IsNullOrEmpty(dmr.Client) && (dmr.Client.Equals(Rules.KEYS.AllClients) || client.Equals(dmr.Client) || client.EndsWith(dmr.Client));
                    bool isDomainMatch = Rules.IsDomainMatch(host, dmr.Domain, out bool isWildcard, out string hostNoWww, out string ruleHostNoWww);
                    bool isMatch = isClientMatch && isDomainMatch;
                    if (!isMatch) continue;

                    // Set Match
                    drr.IsMatch = isMatch;

                    // Is Black List
                    drr.IsBlackList = dmr.IsBlock;
                    if (drr.IsBlackList) break;

                    // DNS
                    if (!string.IsNullOrEmpty(dmr.FakeDns))
                    {
                        // Fake DNS
                        drr.Dns = dmr.FakeDns;
                    }
                    else
                    {
                        // Get Dns Servers And Upstream Proxy
                        List<string> dnss = dmr.Dnss.Any() ? dmr.Dnss : settings.DNSs;
                        drr.Dnss = dnss;
                        string? dnsProxyScheme = null, dnsProxyUser = null, dnsProxyPass = null;
                        if (!string.IsNullOrEmpty(dmr.DnsProxyScheme))
                        {
                            dnsProxyScheme = dmr.DnsProxyScheme;
                            dnsProxyUser = dmr.DnsProxyUser;
                            dnsProxyPass = dmr.DnsProxyPass;
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
                            drr.DnsCustomDomain = host;
                            if (!string.IsNullOrEmpty(dmr.DnsDomain))
                            {
                                if (!dmr.DnsDomain.StartsWith("*."))
                                {
                                    drr.DnsCustomDomain = dmr.DnsDomain;
                                }
                                else
                                {
                                    // Support: xxxx.example.com -> xxxx.domain.com
                                    if (isWildcard) // ruleHostNoWww.StartsWith("*.")
                                    {
                                        if (hostNoWww.EndsWith(ruleHostNoWww[1..])) // Just In Case
                                        {
                                            drr.DnsCustomDomain = hostNoWww.Replace(ruleHostNoWww[1..], dmr.DnsDomain[1..]);
                                        }
                                    }
                                }
                            }

                            IPAddress ipv4Addr = IPAddress.None;
                            if (settings.IsIPv4SupportedByISP)
                            {
                                ipv4Addr = await GetIP.GetIpFromDnsAddressAsync(drr.DnsCustomDomain, dnss, settings.AllowInsecure, settings.DnsTimeoutSec, false, settings.BootstrapIpAddress, settings.BootstrapPort, dnsProxyScheme, dnsProxyUser, dnsProxyPass);
                                if (ipv4Addr.Equals(IPAddress.None) && !settings.IsIPv6SupportedByISP) // Retry If IPv6 Is Not Supported
                                    ipv4Addr = await GetIP.GetIpFromDnsAddressAsync(drr.DnsCustomDomain, dnss, settings.AllowInsecure, settings.DnsTimeoutSec, false, settings.BootstrapIpAddress, settings.BootstrapPort, dnsProxyScheme, dnsProxyUser, dnsProxyPass);
                            }

                            if (ipv4Addr.Equals(IPAddress.None))
                            {
                                IPAddress ipv6Addr = await GetIP.GetIpFromDnsAddressAsync(drr.DnsCustomDomain, dnss, settings.AllowInsecure, settings.DnsTimeoutSec, true, settings.BootstrapIpAddress, settings.BootstrapPort, dnsProxyScheme, dnsProxyUser, dnsProxyPass);
                                if (!ipv6Addr.Equals(IPAddress.IPv6None))
                                    drr.Dns = ipv6Addr.ToString();
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(settings.CloudflareCleanIP))
                                    drr.Dns = ipv4Addr.ToString();
                                else
                                    drr.Dns = CommonTools.IsCfIP(ipv4Addr) ? settings.CloudflareCleanIP : ipv4Addr.ToString();
                            }
                        }
                    }

                    // Break If Match
                    break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DnsRules_GetAsync: " + ex.Message);
            }

            return drr;
        }

    }
}
