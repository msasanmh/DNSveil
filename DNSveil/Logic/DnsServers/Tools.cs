using System.Net;
using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using static DNSveil.Logic.DnsServers.EnumsAndStructs;

namespace DNSveil.Logic.DnsServers;

public static class Tools
{
    public static List<DnsItem> Convert_DNSs_To_DnsItem(List<string> dnss)
    {
        List<DnsItem> result = new();

        try
        {
            dnss = dnss.Distinct().ToList();
            for (int n = 0; n < dnss.Count; n++)
            {
                string dns = dnss[n];
                DnsReader dr = new(dns);
                DnsItem di = new() { DNS_URL = dr.DnsWithRelay, Protocol = dr.ProtocolName };
                if (dr.IsDnsCryptStamp)
                {
                    IPAddress dns_IP = dr.StampReader.IP;
                    if (!NetworkTool.IsLocalIP(dns_IP)) di.DNS_IP = dns_IP;
                }
                result.Add(di);
            }
        }
        catch (Exception) { }

        return result;
    }

    public static List<DnsItem> Convert_DNSs_To_DnsItem_ForFragmentDoH(List<string> dnss, List<string> maliciousDomains)
    {
        List<DnsItem> result = new();

        try
        {
            dnss = dnss.Distinct().ToList();
            for (int n = 0; n < dnss.Count; n++)
            {
                string dns = dnss[n];
                DnsReader dr = new(dns);
                if (dr.IsDnsCryptStamp && dr.Protocol == DnsEnums.DnsProtocol.DoH)
                {
                    if (!dr.IsHostIP && !NetworkTool.IsLocalIP(dr.IP))
                    {
                        // Ignore Malicious Domains
                        if (maliciousDomains.IsContain(dr.Host)) continue;
                        NetworkTool.URL url = NetworkTool.GetUrlOrDomainDetails(dr.Host, dr.Port);
                        if (maliciousDomains.IsContain(url.BaseHost)) continue;

                        string dns_URL = $"{dr.Scheme}{dr.Host}{dr.Path}";
                        if (dr.Port != 443) dns_URL = $"{dr.Scheme}{dr.Host}:{dr.Port}{dr.Path}";
                        DnsItem di = new() { DNS_URL = dns_URL, DNS_IP = dr.IP, Protocol = dr.ProtocolName };
                        result.Add(di);
                    }
                }
            }
            result = result.DistinctBy(_ => _.DNS_URL).ToList(); // DeDup
        }
        catch (Exception) { }

        return result;
    }

}