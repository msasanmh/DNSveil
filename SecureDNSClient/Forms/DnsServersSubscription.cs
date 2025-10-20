using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Diagnostics;
using System.Text;

namespace SecureDNSClient;

public partial class FormMain
{
    public static async Task<List<string>> GetServersFromContentAsync(string content)
    {
        List<string> dnss = new();

        try
        {
            List<string> links = new();
            string[] lines = content.ReplaceLineEndings(NL).Split(NL, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            for (int n = 0; n < lines.Length; n++)
            {
                string line = lines[n];
                if (string.IsNullOrEmpty(line)) continue;

                // Support IP:Port
                bool isIP = false;
                try
                {
                    if (!line.Contains("://") && line.Contains(':'))
                    {
                        string temp = line;
                        int index = temp.LastIndexOf(':');
                        if (index != -1) temp = temp[..index];
                        if (NetworkTool.IsIP(temp, out _)) isIP = true;
                    }
                }
                catch (Exception) { }
                
                if (isIP) links.Add(line);
                else
                {
                    // Support For Anonymized DNSCrypt
                    bool isWithRelay = false;
                    List<string> urls = await TextTool.GetLinksAsync(line);
                    if (urls.Count == 1)
                    {
                        try
                        {
                            string url1 = urls[0];
                            DnsReader dr1 = new(url1);
                            if (dr1.Protocol == DnsEnums.DnsProtocol.DnsCrypt)
                            {
                                string tempWithPort = line.Replace(url1, string.Empty).Trim();
                                string temp = tempWithPort;
                                int index = temp.LastIndexOf(':');
                                if (index != -1) temp = temp[..index];
                                if (NetworkTool.IsIP(temp, out _))
                                {
                                    isWithRelay = true;
                                    urls.Add(tempWithPort);
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                    if (urls.Count == 2)
                    {
                        try
                        {
                            string url1 = urls[0];
                            string url2 = urls[1];
                            DnsReader dr1 = new(url1);
                            DnsReader dr2 = new(url2);
                            if (dr1.Protocol == DnsEnums.DnsProtocol.DnsCrypt)
                            {
                                if (dr2.Protocol == DnsEnums.DnsProtocol.AnonymizedDNSCryptRelay ||
                                    dr2.Protocol == DnsEnums.DnsProtocol.UDP ||
                                    dr2.Protocol == DnsEnums.DnsProtocol.TCP ||
                                    dr2.Protocol == DnsEnums.DnsProtocol.TcpOverUdp)
                                    isWithRelay = true;
                            }
                        }
                        catch (Exception) { }
                    }
                    //Debug.WriteLine("-=-==-= " + urls.ToString(" "));
                    if (isWithRelay) links.Add(urls.ToString(" "));
                    else links.AddRange(urls);
                }
            }

            for (int n = 0; n < links.Count; n++)
            {
                string dns = links[n];
                if (dns.StartsWith("http://") || dns.StartsWith("https://"))
                {
                    if (dns.EndsWith(".html", StringComparison.OrdinalIgnoreCase)) continue;
                    if (dns.Contains("github.com", StringComparison.OrdinalIgnoreCase)) continue;
                    if (dns.Contains("support.google.com", StringComparison.OrdinalIgnoreCase)) continue;
                    if (dns.Contains("learn.microsoft.com", StringComparison.OrdinalIgnoreCase)) continue;
                    NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(dns, 443);
                    if (urid.Path.Length < 2) continue;
                }
                if (DnsTools.IsDnsProtocolSupported(dns)) dnss.Add(dns);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetServersFromContentAsync: " + ex.Message);
        }

        return dnss;
    }

    public static async Task<List<string>> GetServersFromLinkAsync(string url)
    {
        try
        {
            Uri uri = new(url, UriKind.Absolute);

            HttpRequest hr = new()
            {
                AllowAutoRedirect = true,
                AllowInsecure = true,
                TimeoutMS = 20000,
                URI = uri
            };

            string content = string.Empty;
            HttpRequestResponse hrr = await HttpRequest.SendAsync(hr);
            if (hrr.IsSuccess)
            {
                content = Encoding.UTF8.GetString(hrr.Data);
                List<string> contentToLines = await TextTool.RemoveHtmlAndMarkDownTagsAsync(content, true);
                content = contentToLines.ToString(Environment.NewLine);
            }
            else
            {
                // Try With System Proxy
                string systemProxyScheme = NetworkTool.GetSystemProxy();
                if (!string.IsNullOrWhiteSpace(systemProxyScheme))
                {
                    hr.ProxyScheme = systemProxyScheme;
                    hrr = await HttpRequest.SendAsync(hr);
                    if (hrr.IsSuccess)
                    {
                        content = Encoding.UTF8.GetString(hrr.Data);
                        List<string> contentToLines = await TextTool.RemoveHtmlAndMarkDownTagsAsync(content, true);
                        content = contentToLines.ToString(Environment.NewLine);
                    }
                }
            }
            
            return await GetServersFromContentAsync(content);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetServersFromLinkAsync: " + ex.Message);
            return new List<string>();
        }
    }
}