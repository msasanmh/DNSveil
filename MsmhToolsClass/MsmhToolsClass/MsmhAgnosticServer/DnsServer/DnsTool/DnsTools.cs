using System.Diagnostics;
using System.Net;
using System.Text;

namespace MsmhToolsClass.MsmhAgnosticServer;

public static class DnsTools
{
    public static bool IsDnsProtocolSupported(string dns)
    {
        try
        {
            dns = dns.Trim();

            DnsReader dr = new(dns);
            return dr.Protocol != DnsEnums.DnsProtocol.Unknown && dr.Port >= 1 && dr.Port <= 65535;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsTools IsDnsProtocolSupported: " + ex.Message);
            return false;
        }
    }

    public static async Task<List<string>> GetServersFromLinkAsync(string urlOrFile, int timeoutMs, CancellationToken ct)
    {
        List<string> dnss = new();

        try
        {
            byte[] bytes = Array.Empty<byte>();

            if (File.Exists(urlOrFile))
                bytes = await File.ReadAllBytesAsync(urlOrFile, ct);
            else
                bytes = await WebAPI.DownloadFileAsync(urlOrFile, timeoutMs, ct).ConfigureAwait(false);

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
                    for (int i = 0; i < endPoints.Count; i++) // Add EndPoints Which Is Not In The URLs
                    {
                        string endPoint = endPoints[i];
                        bool isInUrl = false;
                        for (int j = 0; j < urls.Count; j++)
                        {
                            string url = urls[j];
                            if (url.Contains(endPoint, StringComparison.InvariantCultureIgnoreCase))
                            {
                                isInUrl = true;
                                break;
                            }
                        }
                        if (isInUrl) continue;
                        urlsAndEndPoints.Add(endPoint);
                    }
                }
                
                for (int n = 0; n < urlsAndEndPoints.Count; n++)
                {
                    string dns = urlsAndEndPoints[n];
                    if (dns.StartsWith("http://") || dns.StartsWith("https://"))
                    {
                        if (dns.EndsWith(".html", StringComparison.OrdinalIgnoreCase)) continue;
                        if (dns.EndsWith(".htm", StringComparison.OrdinalIgnoreCase)) continue;
                        if (dns.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) continue;
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
        List<string> result = new();

        await Task.Run(() =>
        {
            try
            {
                for (int n = 0; n < dnss.Count; n++)
                {
                    string dns = dnss[n];
                    DnsReader dr = new(dns);
                    bool added = false;
                    if (dr.IsDnsCryptStamp)
                    {
                        if (dr.Protocol == DnsEnums.DnsProtocol.UDP || dr.Protocol == DnsEnums.DnsProtocol.TCP || dr.Protocol == DnsEnums.DnsProtocol.TcpOverUdp)
                        {
                            if (dr.IsHostIP)
                            {
                                if (!NetworkTool.IsLocalIP(dr.Host))
                                {
                                    bool isIP = NetworkTool.IsIP(dr.Host, out IPAddress? ipOut);
                                    if (isIP && ipOut != null)
                                    {
                                        string dns_IP_URL = NetworkTool.IpToUrl(dr.Scheme, ipOut, dr.Port, string.Empty);
                                        result.Add(dns_IP_URL);
                                        added = true;
                                    }
                                }
                            }
                            else
                            {
                                if (!NetworkTool.IsLocalIP(dr.IP))
                                {
                                    string dns_IP_URL = NetworkTool.IpToUrl(dr.Scheme, dr.IP, dr.Port, string.Empty);
                                    result.Add(dns_IP_URL);
                                    added = true;
                                }
                            }
                        }
                        else if (dr.Protocol == DnsEnums.DnsProtocol.DoT)
                        {
                            string dns_URL = $"{dr.Scheme}{dr.Host}";
                            if (dr.Port != 853) dns_URL = $"{dr.Scheme}{dr.Host}:{dr.Port}";
                            result.Add(dns_URL);
                            
                            if (!dr.IsHostIP && !NetworkTool.IsLocalIP(dr.IP))
                            {
                                string dns_IP_URL = NetworkTool.IpToUrl(dr.Scheme, dr.IP, dr.Port, string.Empty);
                                result.Add(dns_IP_URL);
                            }

                            added = true;
                        }
                        else if (dr.Protocol == DnsEnums.DnsProtocol.DoH)
                        {
                            string dns_URL = $"{dr.Scheme}{dr.Host}{dr.Path}";
                            if (dr.Port != 443) dns_URL = $"{dr.Scheme}{dr.Host}:{dr.Port}{dr.Path}";
                            result.Add(dns_URL);

                            if (!dr.IsHostIP && !NetworkTool.IsLocalIP(dr.IP))
                            {
                                string dns_IP_URL = NetworkTool.IpToUrl(dr.Scheme, dr.IP, dr.Port, dr.Path);
                                result.Add(dns_IP_URL);
                            }

                            added = true;
                        }
                    }

                    if (!added) result.Add(dns);
                }

                result = result.Distinct().ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DnsTools DecodeStampAsync: " + ex.Message);
            }
        });

        return result;
    }

    public static async Task<string> DecodeStampAsync(string dns)
    {
        string result = dns;

        try
        {
            List<string> dnss = new() { dns };
            List<string> decoded = await DecodeStampAsync(dnss);
            if (decoded.Count > 0)
            {
                result = decoded[0];
            }
        }
        catch (Exception) { }

        return result;
    }

}