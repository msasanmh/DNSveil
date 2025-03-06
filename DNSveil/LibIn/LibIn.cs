using MsmhToolsClass;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DNSveil;

public static class LibIn
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
                return isPlainDnsWithUnusualPort(dns);

            static bool isPlainDnsWithUnusualPort(string dns) // Support for plain DNS with unusual port
            {
                if (dns.Contains(':'))
                {
                    NetworkTool.GetUrlDetails(dns, 53, out _, out string ipStr, out _, out _, out int port, out _, out _);
                    if (NetworkTool.IsIP(ipStr, out _)) return port >= 1 && port <= 65535;
                }
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("LibIn IsDnsProtocolSupported: " + ex.Message);
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
                content = await TextTool.RemoveHtmlTagsAsync(content, true);
                List<string> allUrls = new();
                string[] lines = content.ReplaceLineEndings(NL).Split(NL, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                for (int n = 0; n < lines.Length; n++)
                {
                    string line = lines[n];
                    List<string> urls = await TextTool.GetLinksAsync(line);
                    allUrls.AddRange(urls);
                }

                for (int n = 0; n < allUrls.Count; n++)
                {
                    string dns = allUrls[n];
                    if (dns.StartsWith("http://") || dns.StartsWith("https://"))
                    {
                        if (dns.EndsWith(".html", StringComparison.OrdinalIgnoreCase)) continue;
                        if (dns.Contains("github.com", StringComparison.OrdinalIgnoreCase)) continue;
                        if (dns.Contains("support.google.com", StringComparison.OrdinalIgnoreCase)) continue;
                        if (dns.Contains("learn.microsoft.com", StringComparison.OrdinalIgnoreCase)) continue;
                        if (dns.Contains("/news", StringComparison.OrdinalIgnoreCase)) continue;
                        if (dns.Contains("/blog", StringComparison.OrdinalIgnoreCase)) continue;
                        NetworkTool.GetUrlDetails(dns, 443, out _, out _, out _, out _, out _, out string path, out _);
                        if (path.Length < 2) continue;
                    }
                    if (IsDnsProtocolSupported(dns)) dnss.Add(dns);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("LibIn GetServersFromLinkAsync: " + ex.Message);
        }

        return dnss;
    }
}