using System.Diagnostics;
using System.Net;
using System.Text;

namespace MsmhToolsClass.MsmhAgnosticServer;

public static class DnsTools
{
    private static readonly string NL = Environment.NewLine;

    public static bool IsDnsProtocolSupported(string dns)
    {
        try
        {
            dns = dns.Trim();
            StringComparison sc = StringComparison.OrdinalIgnoreCase;
            if (dns.StartsWith("udp://", sc) || dns.StartsWith("tcp://", sc) || dns.StartsWith("http://", sc) || dns.StartsWith("https://", sc) ||
                dns.StartsWith("h3://", sc) || dns.StartsWith("tls://", sc) || dns.StartsWith("quic://", sc) || dns.StartsWith("sdns://", sc))
                return true;
            else
                return isPlainDns(dns);

            static bool isPlainDns(string dns) // Support For Plain DNS
            {
                NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(dns, 53);
                if (NetworkTool.IsIP(urid.Host, out _)) return urid.Port >= 1 && urid.Port <= 65535;
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsTools IsDnsProtocolSupported: " + ex.Message);
            return false;
        }
    }

    public static async Task<List<string>> GetServersFromLinkAsync(string urlOrFile, int timeoutMs)
    {
        List<string> dnss = new();

        try
        {
            byte[] bytes = Array.Empty<byte>();

            if (File.Exists(urlOrFile))
                bytes = await File.ReadAllBytesAsync(urlOrFile);
            else
                bytes = await WebAPI.DownloadFileAsync(urlOrFile, timeoutMs).ConfigureAwait(false);

            if (bytes.Length > 0)
            {
                string content = Encoding.UTF8.GetString(bytes);
                
                List<string> urlsAndEndPoints = new();
                List<string> lines = await TextTool.RemoveHtmlAndMarkDownTagsAsync(content, true);

                for (int n = 0; n < lines.Count; n++)
                {
                    string line = lines[n].Trim();
                    
                    List<string> urls = await TextTool.GetLinksAsync(line);
                    urlsAndEndPoints.AddRange(urls);

                    List<string> endPoints = TextTool.GetEndPoints(line);
                    for (int n2 = 0; n2 < endPoints.Count; n2++)
                    {
                        string tcpPlainDNS = $"tcp://{endPoints[n2]}";
                        urlsAndEndPoints.Add(tcpPlainDNS);
                    }
                }

                for (int n = 0; n < urlsAndEndPoints.Count; n++)
                {
                    string dns = urlsAndEndPoints[n];
                    if (dns.StartsWith("http://") || dns.StartsWith("https://"))
                    {
                        if (dns.EndsWith(".html", StringComparison.OrdinalIgnoreCase)) continue;
                        if (dns.EndsWith(".htm", StringComparison.OrdinalIgnoreCase)) continue;
                        if (dns.EndsWith(".php", StringComparison.OrdinalIgnoreCase)) continue;
                        if (dns.Contains("github.com", StringComparison.OrdinalIgnoreCase)) continue;
                        if (dns.Contains("githubusercontent.com", StringComparison.OrdinalIgnoreCase)) continue;
                        if (dns.Contains("ietf.org", StringComparison.OrdinalIgnoreCase)) continue;
                        if (dns.Contains("learn.microsoft.com", StringComparison.OrdinalIgnoreCase)) continue;
                        if (dns.Contains("support.google.com", StringComparison.OrdinalIgnoreCase)) continue;
                        if (dns.Contains("/blog", StringComparison.OrdinalIgnoreCase)) continue;
                        if (dns.Contains("/faq", StringComparison.OrdinalIgnoreCase)) continue;
                        if (dns.Contains("/news", StringComparison.OrdinalIgnoreCase)) continue;
                        if (dns.Contains("/wiki", StringComparison.OrdinalIgnoreCase)) continue;
                        NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(dns, 443);
                        if (urid.Path.Length < 2) continue;
                    }
                    dns = dns.TrimEnd('/');
                    if (IsDnsProtocolSupported(dns)) dnss.Add(dns);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsTools GetServersFromLinkAsync: " + ex.Message);
        }

        return dnss.Distinct().ToList();
    }

    public static async Task<List<string>> DecodeStampAsync(List<string> dnss)
    {
        List<string> temp = new();

        await Task.Run(() =>
        {
            try
            {
                for (int n = 0; n < dnss.Count; n++)
                {
                    string dns = dnss[n];
                    DnsReader dnsReader = new(dns);
                    bool added = false;
                    if (dnsReader.IsDnsCryptStamp)
                    {
                        if (dnsReader.Protocol == DnsEnums.DnsProtocol.UDP || dnsReader.Protocol == DnsEnums.DnsProtocol.TCP)
                        {
                            if (dnsReader.IsHostIP)
                            {
                                if (!NetworkTool.IsLocalIP(dnsReader.Host))
                                {
                                    bool isIP = NetworkTool.IsIP(dnsReader.Host, out IPAddress? ipOut);
                                    if (isIP && ipOut != null)
                                    {
                                        string dns_IP_URL = NetworkTool.IpToUrl(dnsReader.Scheme, ipOut, dnsReader.Port, string.Empty);
                                        temp.Add(dns_IP_URL);
                                        added = true;
                                    }
                                }
                            }
                            else
                            {
                                if (!NetworkTool.IsLocalIP(dnsReader.IP.ToString()))
                                {
                                    string dns_IP_URL = NetworkTool.IpToUrl(dnsReader.Scheme, dnsReader.IP, dnsReader.Port, string.Empty);
                                    temp.Add(dns_IP_URL);
                                    added = true;
                                }
                            }
                        }
                        else if (dnsReader.Protocol == DnsEnums.DnsProtocol.DoT)
                        {
                            string dns_URL = $"{dnsReader.Scheme}{dnsReader.Host}";
                            if (dnsReader.Port != 853) dns_URL = $"{dnsReader.Scheme}{dnsReader.Host}:{dnsReader.Port}";
                            temp.Add(dns_URL);

                            if (!dnsReader.IsHostIP && !NetworkTool.IsLocalIP(dnsReader.IP.ToString()))
                            {
                                string dns_IP_URL = NetworkTool.IpToUrl(dnsReader.Scheme, dnsReader.IP, dnsReader.Port, string.Empty);
                                temp.Add(dns_IP_URL);
                            }

                            added = true;
                        }
                        else if (dnsReader.Protocol == DnsEnums.DnsProtocol.DoH)
                        {
                            string dns_URL = $"{dnsReader.Scheme}{dnsReader.Host}{dnsReader.Path}";
                            if (dnsReader.Port != 443) dns_URL = $"{dnsReader.Scheme}{dnsReader.Host}:{dnsReader.Port}{dnsReader.Path}";
                            temp.Add(dns_URL);

                            if (!dnsReader.IsHostIP && !NetworkTool.IsLocalIP(dnsReader.IP.ToString()))
                            {
                                string dns_IP_URL = NetworkTool.IpToUrl(dnsReader.Scheme, dnsReader.IP, dnsReader.Port, dnsReader.Path);
                                Debug.WriteLine(dns_IP_URL);
                                temp.Add(dns_IP_URL);
                            }

                            added = true;
                        }
                    }

                    if (!added) temp.Add(dns);
                }

                temp = temp.Distinct().ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DnsTools DecodeStampAsync: " + ex.Message);
            }
        });

        return temp;
    }
}