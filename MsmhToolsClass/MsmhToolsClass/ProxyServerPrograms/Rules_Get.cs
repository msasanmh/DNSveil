using System.Diagnostics;

namespace MsmhToolsClass.ProxyServerPrograms;

public partial class ProxyProgram
{
    public partial class Rules
    {
        public class RulesResult
        {
            public bool IsMatch { get; set; } = false;
            public bool IsBlackList { get; set; } = false;
            public bool IsPortBlock { get; set; } = false;
            public bool ApplyDpiBypass { get; set; } = true;
            public string DnsServer { get; set; } = string.Empty;
            public string DnsCustomDomain { get; set; } = string.Empty;
            public string DnsProxyScheme { get; set; } = string.Empty;
            public string Dns { get; set; } = string.Empty;
            public string Sni { get; set; } = string.Empty;
            public bool ApplyUpStreamProxy { get; set; } = false;
            public string ProxyScheme { get; set; } = string.Empty;
            public bool ApplyUpStreamProxyToBlockedIPs { get; set; } = false;
            public string ProxyUser { get; set; } = string.Empty;
            public string ProxyPass { get; set; } = string.Empty;
        }

        public async Task<RulesResult> GetAsync(string client, string host, int port)
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
                    bool isClientMatch = !string.IsNullOrEmpty(mr.Client) && (mr.Client.Equals(KEYS.AllClients) || mr.Client.Equals(client));
                    bool isDomainMatch = IsDomainMatch(host, mr.Domain, out bool isWildcard, out string hostNoWww, out string ruleHostNoWww);
                    bool isMatch = isClientMatch && isDomainMatch;
                    if (!isMatch) continue;
                    
                    // Set Match
                    rr.IsMatch = isMatch;

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

                    // Apply DPI Bypass (Fragment & Change SNI)
                    rr.ApplyDpiBypass = !mr.NoBypass;
                    
                    // DNS
                    if (!string.IsNullOrEmpty(mr.FakeDns))
                    {
                        // Fake DNS
                        rr.Dns = mr.FakeDns;
                    }
                    else
                    {
                        // Get IP By Custom DNS
                        if (mr.Dnss.Any() && !NetworkTool.IsIp(host, out _))
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

                            for (int i = 0; i < mr.Dnss.Count; i++)
                            {
                                string dns = mr.Dnss[i].ToLower().Trim();
                                if (string.IsNullOrEmpty(dns)) continue;
                                
                                if (dns.Equals("system")) // System
                                {
                                    rr.Dns = DnsTool.GetIP.GetIpFromSystem(rr.DnsCustomDomain);
                                }
                                else
                                {
                                    DnsTool.DnsReader dnsReader = new(dns, null);
                                    if (dnsReader.Protocol == DnsTool.DnsReader.DnsProtocol.PlainDNS) // Plain DNS
                                    {
                                        DnsTool.GetIP.PlainDnsProtocol plainDnsProtocol = DnsTool.GetIP.PlainDnsProtocol.Both;
                                        if (dns.StartsWith("udp://")) plainDnsProtocol = DnsTool.GetIP.PlainDnsProtocol.UDP;
                                        if (dns.StartsWith("tcp://")) plainDnsProtocol = DnsTool.GetIP.PlainDnsProtocol.TCP;
                                        if (!string.IsNullOrEmpty(dnsReader.IP))
                                            rr.Dns = await DnsTool.GetIP.GetIpFromPlainDNS(rr.DnsCustomDomain, dnsReader.IP, dnsReader.Port, 3, plainDnsProtocol);
                                    }
                                    else if (dnsReader.Protocol == DnsTool.DnsReader.DnsProtocol.DoH) // DoH
                                    {
                                        bool dohHasProxy = false;
                                        if (!string.IsNullOrEmpty(mr.DnsProxyScheme))
                                        {
                                            mr.DnsProxyScheme = mr.DnsProxyScheme.ToLower().Trim();
                                            if (mr.DnsProxyScheme.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                                mr.DnsProxyScheme.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                                                mr.DnsProxyScheme.StartsWith("socks5://", StringComparison.OrdinalIgnoreCase))
                                                dohHasProxy = true;
                                        }
                                        
                                        if (dohHasProxy)
                                        {
                                            rr.Dns = await DnsTool.GetIP.GetIpFromDohUsingWireFormat(rr.DnsCustomDomain, dns, 3, mr.DnsProxyScheme, mr.DnsProxyUser, mr.DnsProxyPass);
                                            if (!string.IsNullOrEmpty(rr.Dns))
                                                rr.DnsProxyScheme = mr.DnsProxyScheme;
                                        }
                                        else
                                            rr.Dns = await DnsTool.GetIP.GetIpFromDohUsingWireFormat(rr.DnsCustomDomain, dns, 3);
                                    }
                                }

                                // Break If We Have IP
                                if (!string.IsNullOrEmpty(rr.Dns))
                                {
                                    rr.DnsServer = dns;
                                    break;
                                }
                            }

                            // If All DNSs Failed To Get IP Set To Original Host
                            if (string.IsNullOrEmpty(rr.Dns)) rr.Dns = host;
                        }
                    }

                    // SNI
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

                    // Upstream Proxy
                    if (!string.IsNullOrEmpty(mr.ProxyScheme))
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

        private static bool IsDomainMatch(string host, string ruleHost, out bool isWildcard, out string hostNoWWW, out string ruleHostNoWWW)
        {
            isWildcard = false;
            hostNoWWW = host.ToLower().Trim();
            ruleHostNoWWW = ruleHost.ToLower().Trim();

            try
            {
                if (hostNoWWW.StartsWith("www."))
                    hostNoWWW = hostNoWWW.TrimStart("www.");
                if (hostNoWWW.EndsWith('/')) hostNoWWW = hostNoWWW[0..^1];

                if (ruleHostNoWWW.StartsWith("www."))
                    ruleHostNoWWW = ruleHostNoWWW.TrimStart("www.");
                if (ruleHostNoWWW.EndsWith('/')) ruleHostNoWWW = ruleHostNoWWW[0..^1];

                if (!string.IsNullOrEmpty(ruleHostNoWWW))
                {
                    if (ruleHostNoWWW.Equals("*")) return true; // No Wildcard

                    if (!ruleHostNoWWW.StartsWith("*."))
                    {
                        // No Wildcard
                        if (ruleHostNoWWW.Equals(hostNoWWW)) return true;
                    }
                    else
                    {
                        // Wildcard
                        isWildcard = true;
                        if (!hostNoWWW.Equals(ruleHostNoWWW[2..]) && hostNoWWW.EndsWith(ruleHostNoWWW[1..])) return true;
                    }
                }
            }
            catch (Exception) { }

            return false;
        }

    }
}
