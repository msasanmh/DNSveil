using System.Net;

namespace MsmhToolsClass.DnsTool;

public class DnsReader
{
    public string? IP { get; private set; }
    public int Port { get; private set; }
    public string Dns { get; private set; }
    public string Host { get; private set; } = string.Empty;
    public string Path { get; private set; } = string.Empty;
    public string CompanyName { get; private set; } = string.Empty;
    private string CompanyNameDataFileContent { get; set; } = string.Empty;
    public DnsProtocol Protocol { get; private set; }
    public string ProtocolName { get; private set; }
    public bool IsDnsCryptStamp { get; private set; } = false;
    public DnsCryptStamp StampProperties = new();

    public class DnsCryptStamp
    {
        public bool IsDnsSec { get; set; } = false;
        public bool IsNoLog { get; set; } = false;
        public bool IsNoFilter { get; set; } = false;
    }

    /// <summary>
    /// Read any DNS
    /// </summary>
    /// <param name="dns">DNS Address</param>
    /// <param name="companyNameDataFileContent">File content to get company name. each line e.g. 8.8.8.8|Google Inc.</param>
    public DnsReader(string dns, string? companyNameDataFileContent)
    {
        Dns = dns;

        if (!string.IsNullOrEmpty(companyNameDataFileContent))
            CompanyNameDataFileContent = companyNameDataFileContent;

        Protocol = DnsProtocol.Unknown;
        ProtocolName = DnsProtocolName.Unknown;

        if (dns.ToLower().StartsWith("sdns://"))
        {
            IsDnsCryptStamp = true;

            // Decode Stamp
            DNSCryptStampReader stamp = new(dns);
            if (stamp != null && stamp.IsDecryptionSuccess)
            {
                IP = stamp.IP;
                Port = stamp.Port;
                Host = stamp.Host;
                Path = stamp.Path;
                Protocol = ParseProtocol(stamp.Protocol);
                ProtocolName = stamp.ProtocolName;
                StampProperties.IsDnsSec = stamp.IsDnsSec;
                StampProperties.IsNoLog = stamp.IsNoLog;
                StampProperties.IsNoFilter = stamp.IsNoFilter;

                // Get Company Name (SDNS)
                string stampHost = stamp.Host;
                if (string.IsNullOrEmpty(stampHost)) stampHost = stamp.IP;
                if (string.IsNullOrEmpty(stampHost)) stampHost = stamp.ProviderName;
                if (!string.IsNullOrEmpty(CompanyNameDataFileContent))
                    CompanyName = GetCompanyName(stampHost, CompanyNameDataFileContent);
            }
        }
        else
        {
            if (dns.ToLower().StartsWith("https://"))
            {
                // DoH
                SetIpPortHostPath(dns, 443);

                Protocol = DnsProtocol.DoH;
                ProtocolName = DnsProtocolName.DoH;
            }
            else if (dns.ToLower().StartsWith("tls://"))
            {
                // TLS
                SetIpPortHostPath(dns, 853);

                Protocol = DnsProtocol.DoT;
                ProtocolName = DnsProtocolName.DoT;
            }
            else if (dns.ToLower().StartsWith("quic://"))
            {
                // DoQ
                SetIpPortHostPath(dns, 853);

                Protocol = DnsProtocol.DoQ;
                ProtocolName = DnsProtocolName.DoQ;
            }
            else if (dns.ToLower().StartsWith("tcp://") || dns.ToLower().StartsWith("udp://"))
            {
                // Plain DNS
                SetIpPortHostPath(dns, 53);

                Protocol = DnsProtocol.PlainDNS;
                ProtocolName = DnsProtocolName.PlainDNS;
            }
            else
            {
                // Plain DNS
                SetIpPortHost(dns, 53);

                Protocol = DnsProtocol.PlainDNS;
                ProtocolName = DnsProtocolName.PlainDNS;
            }
        }
    }

    private void SetIpPortHostPath(string dns, int defaultPort)
    {
        NetworkTool.GetUrlDetails(dns, defaultPort, out _, out string host, out _, out _, out int port, out string path, out bool isIPv6);
        Port = port;
        Path = path;
        bool isIPv4 = NetworkTool.IsIPv4Valid(host, out IPAddress? _);
        if (isIPv6 || isIPv4)
        {
            IP = host;
        }
        else
        {
            Host = host;
            IP = GetIP.GetIpFromSystem(host, false, false);
        }
        if (!string.IsNullOrEmpty(CompanyNameDataFileContent))
        {
            string? ipOrHost = Host;
            if (string.IsNullOrEmpty(ipOrHost)) ipOrHost = IP;
            if (string.IsNullOrEmpty(ipOrHost)) ipOrHost = host;
            CompanyName = GetCompanyName(ipOrHost, CompanyNameDataFileContent);
        }
    }

    private void SetIpPortHost(string hostIpPort, int defaultPort)
    {
        NetworkTool.GetHostDetails(hostIpPort, defaultPort, out string host, out _, out _, out int port, out string _, out bool isIPv6);
        Port = port;
        bool isIPv4 = NetworkTool.IsIPv4Valid(host, out IPAddress? _);
        if (isIPv6 || isIPv4)
        {
            IP = host;
        }
        else
        {
            Host = host;
            IP = GetIP.GetIpFromSystem(host, false, false);
        }
        if (!string.IsNullOrEmpty(CompanyNameDataFileContent))
        {
            string? ipOrHost = Host;
            if (string.IsNullOrEmpty(ipOrHost)) ipOrHost = IP;
            if (string.IsNullOrEmpty(ipOrHost)) ipOrHost = host;
            CompanyName = GetCompanyName(ipOrHost, CompanyNameDataFileContent);
        }
    }

    private static string GetCompanyName(string host, string fileContent)
    {
        return DnsTool.GetCompanyName.HostToCompanyOffline(host, fileContent);
    }

    private static DnsProtocol ParseProtocol(DNSCryptStampReader.StampProtocol stampProtocol)
    {
        var protocol = stampProtocol switch
        {
            DNSCryptStampReader.StampProtocol.PlainDNS => DnsProtocol.PlainDNS,
            DNSCryptStampReader.StampProtocol.DnsCrypt => DnsProtocol.DnsCrypt,
            DNSCryptStampReader.StampProtocol.DoH => DnsProtocol.DoH,
            DNSCryptStampReader.StampProtocol.DoT => DnsProtocol.DoT,
            DNSCryptStampReader.StampProtocol.DoQ => DnsProtocol.DoQ,
            DNSCryptStampReader.StampProtocol.ObliviousDohTarget => DnsProtocol.ObliviousDohTarget,
            DNSCryptStampReader.StampProtocol.AnonymizedDNSCryptRelay => DnsProtocol.AnonymizedDNSCryptRelay,
            DNSCryptStampReader.StampProtocol.ObliviousDohRelay => DnsProtocol.ObliviousDohRelay,
            DNSCryptStampReader.StampProtocol.Unknown => DnsProtocol.Unknown,
            _ => DnsProtocol.Unknown,
        };
        return protocol;
    }

    public enum DnsProtocol
    {
        PlainDNS,
        DnsCrypt,
        DoH,
        DoT,
        DoQ,
        ObliviousDohTarget,
        AnonymizedDNSCryptRelay,
        ObliviousDohRelay,
        Unknown
    }

    private struct DnsProtocolName
    {
        public static string PlainDNS = "Plain DNS";
        public static string DnsCrypt = "DNSCrypt";
        public static string DoH = "DNS-Over-HTTPS";
        public static string DoT = "DNS-Over-TLS";
        public static string DoQ = "DNS-Over-Quic";
        public static string ObliviousDohTarget = "Oblivious DoH Target";
        public static string AnonymizedDNSCryptRelay = "Anonymized DNSCrypt Relay";
        public static string ObliviousDohRelay = "Oblivious DoH Relay";
        public static string Unknown = "Unknown";
    }

}