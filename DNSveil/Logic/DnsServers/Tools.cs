using System.Net;
using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using static DNSveil.Logic.DnsServers.EnumsAndStructs;

namespace DNSveil.Logic.DnsServers;

public static class Tools
{
    public static List<DnsItem> Convert_DNSs_To_DnsItem(List<string> dnss)
    {
        List<DnsItem> temp = new();

        try
        {
            dnss = dnss.Distinct().ToList();
            for (int n = 0; n < dnss.Count; n++)
            {
                string dns = dnss[n];
                DnsReader dnsReader = new(dns);
                DnsItem di = new() { DNS_URL = dnsReader.DnsWithRelay, Protocol = dnsReader.ProtocolName };
                if (dnsReader.IsDnsCryptStamp && dnsReader.Protocol == DnsEnums.DnsProtocol.DoH)
                {
                    string dns_URL = $"{dnsReader.Scheme}{dnsReader.Host}{dnsReader.Path}";
                    if (dnsReader.Port != 443) dns_URL = $"{dnsReader.Scheme}{dnsReader.Host}:{dnsReader.Port}{dnsReader.Path}";
                    IPAddress dns_IP = dnsReader.StampReader.IP;
                    di = new() { DNS_URL = dns_URL, Protocol = dnsReader.ProtocolName };
                    if (!NetworkTool.IsLocalIP(dns_IP.ToString())) di.DNS_IP = dns_IP;
                }
                temp.Add(di);
            }
            temp = temp.DistinctBy(x => x.DNS_URL).ToList(); // DeDup
        }
        catch (Exception) { }

        return temp;
    }

    public static List<DnsItem> Convert_DNSs_To_DnsItem_ForFragmentDoH(List<string> dnss)
    {
        List<DnsItem> temp = new();

        try
        {
            dnss = dnss.Distinct().ToList();
            for (int n = 0; n < dnss.Count; n++)
            {
                string dns = dnss[n];
                DnsReader dnsReader = new(dns);
                if (dnsReader.IsDnsCryptStamp && dnsReader.Protocol == DnsEnums.DnsProtocol.DoH)
                {
                    if (!dnsReader.IsHostIP && !NetworkTool.IsLocalIP(dnsReader.IP.ToString()))
                    {
                        string dns_URL = $"{dnsReader.Scheme}{dnsReader.Host}{dnsReader.Path}";
                        if (dnsReader.Port != 443) dns_URL = $"{dnsReader.Scheme}{dnsReader.Host}:{dnsReader.Port}{dnsReader.Path}";
                        DnsItem di = new() { DNS_URL = dns_URL, DNS_IP = dnsReader.IP, Protocol = dnsReader.ProtocolName };
                        temp.Add(di);
                    }
                }
            }
            temp = temp.DistinctBy(x => x.DNS_URL).ToList(); // DeDup
        }
        catch (Exception) { }

        return temp;
    }

}