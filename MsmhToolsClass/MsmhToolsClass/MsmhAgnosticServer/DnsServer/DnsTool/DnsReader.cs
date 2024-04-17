using System.Diagnostics;
using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class DnsReader
{
    public string Dns { get; private set; } = string.Empty;
    public string Scheme { get; private set; } = string.Empty;
    public string Host { get; private set; } = string.Empty;
    public bool IsHostIP { get; private set; } = false;
    public IPAddress IP { get; private set; } = IPAddress.None;
    public int Port { get; private set; }
    public string Path { get; private set; } = string.Empty;
    public string CompanyName { get; private set; } = string.Empty;
    private string CompanyNameDataFileContent { get; set; } = string.Empty;
    public DnsEnums.DnsProtocol Protocol { get; private set; } = DnsEnums.DnsProtocol.Unknown;
    public string ProtocolName { get; private set; } = string.Empty;
    public bool IsDnsCryptStamp { get; private set; } = false;
    public DNSCryptStampReader StampReader { get; private set; } = new(string.Empty);

    public DnsReader() { }

    /// <summary>
    /// Read any DNS
    /// </summary>
    /// <param name="dns">DNS Address</param>
    /// <param name="companyNameDataFileContent">File content to get company name. each line e.g. 8.8.8.8|Google Inc.</param>
    public DnsReader(string dns, string? companyNameDataFileContent = null)
    {
        Dns = dns;

        if (!string.IsNullOrEmpty(companyNameDataFileContent))
            CompanyNameDataFileContent = companyNameDataFileContent;

        Protocol = DnsEnums.DnsProtocol.Unknown;
        ProtocolName = DnsEnums.DnsProtocolName.Unknown;

        if (string.IsNullOrEmpty(Dns)) return;

        try
        {
            if (dns.ToLower().StartsWith("sdns://"))
            {
                IsDnsCryptStamp = true;

                // Decode Stamp
                DNSCryptStampReader stamp = new(dns);
                if (stamp != null && stamp.IsDecryptionSuccess)
                {
                    Scheme = "stamp://";
                    Host = stamp.Host;
                    if (!string.IsNullOrEmpty(Host))
                        IsHostIP = IPAddress.TryParse(Host, out _);
                    IP = stamp.IP;
                    Port = stamp.Port;
                    Path = stamp.Path;
                    Protocol = ParseProtocol(stamp.Protocol);

                    if (Protocol == DnsEnums.DnsProtocol.DoH || Protocol == DnsEnums.DnsProtocol.ObliviousDohTarget) Scheme = "https://";
                    else if (Protocol == DnsEnums.DnsProtocol.DoQ) Scheme = "quic://";
                    else if (Protocol == DnsEnums.DnsProtocol.DoT) Scheme = "tls://";
                    else if (Protocol == DnsEnums.DnsProtocol.TCP) Scheme = "tcp://";
                    else if (Protocol == DnsEnums.DnsProtocol.UDP) Scheme = "udp://";

                    ProtocolName = stamp.ProtocolName;
                    StampReader = stamp;

                    // Get Company Name (SDNS)
                    string stampHost = stamp.Host;
                    if (string.IsNullOrEmpty(stampHost)) stampHost = stamp.IP.ToString();
                    if (string.IsNullOrEmpty(stampHost)) stampHost = stamp.ProviderName;
                    if (!string.IsNullOrEmpty(CompanyNameDataFileContent))
                        CompanyName = GetCompanyName.HostToCompanyOffline(stampHost, CompanyNameDataFileContent);
                }
            }
            else
            {
                if (dns.ToLower().StartsWith("https://") || dns.ToLower().StartsWith("http://"))
                {
                    // DoH
                    SetIpPortHostPath(dns, 443);

                    Protocol = DnsEnums.DnsProtocol.DoH;
                    ProtocolName = DnsEnums.DnsProtocolName.DoH;
                }
                else if (dns.ToLower().StartsWith("tls://"))
                {
                    // TLS
                    SetIpPortHostPath(dns, 853);

                    Protocol = DnsEnums.DnsProtocol.DoT;
                    ProtocolName = DnsEnums.DnsProtocolName.DoT;
                }
                else if (dns.ToLower().StartsWith("quic://"))
                {
                    // DoQ
                    SetIpPortHostPath(dns, 853);

                    Protocol = DnsEnums.DnsProtocol.DoQ;
                    ProtocolName = DnsEnums.DnsProtocolName.DoQ;
                }
                else if (dns.ToLower().StartsWith("udp://"))
                {
                    // Plain DNS UDP
                    SetIpPortHostPath(dns, 53);

                    Protocol = DnsEnums.DnsProtocol.UDP;
                    ProtocolName = DnsEnums.DnsProtocolName.UDP;
                }
                else if (dns.ToLower().StartsWith("tcp://"))
                {
                    // Plain DNS TCP
                    SetIpPortHostPath(dns, 53);

                    Protocol = DnsEnums.DnsProtocol.TCP;
                    ProtocolName = DnsEnums.DnsProtocolName.TCP;
                }
                else
                {
                    // Plain DNS UDP
                    SetIpPortHostPath(dns, 53);

                    Protocol = DnsEnums.DnsProtocol.UDP;
                    ProtocolName = DnsEnums.DnsProtocolName.UDP;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsReader: " + ex.Message);
        }
    }

    private void SetIpPortHostPath(string dns, int defaultPort)
    {
        try
        {
            NetworkTool.GetUrlDetails(dns, defaultPort, out string scheme, out string host, out _, out _, out int port, out string path, out bool isIPv6);
            Scheme = scheme;
            Host = host;
            IsHostIP = IPAddress.TryParse(Host, out _);
            Port = port;
            Path = path;

            if (!string.IsNullOrEmpty(CompanyNameDataFileContent))
            {
                string? ipOrHost = Host;
                if (string.IsNullOrEmpty(ipOrHost)) ipOrHost = IP.ToString();
                if (string.IsNullOrEmpty(ipOrHost)) ipOrHost = host;
                CompanyName = GetCompanyName.HostToCompanyOffline(ipOrHost, CompanyNameDataFileContent);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsReader SetIpPortHostPath: " + ex.Message);
        }
    }

    private static DnsEnums.DnsProtocol ParseProtocol(DNSCryptStampReader.StampProtocol stampProtocol)
    {
        var protocol = stampProtocol switch
        {
            DNSCryptStampReader.StampProtocol.PlainDNS => DnsEnums.DnsProtocol.UDP,
            DNSCryptStampReader.StampProtocol.DnsCrypt => DnsEnums.DnsProtocol.DnsCrypt,
            DNSCryptStampReader.StampProtocol.DoH => DnsEnums.DnsProtocol.DoH,
            DNSCryptStampReader.StampProtocol.DoT => DnsEnums.DnsProtocol.DoT,
            DNSCryptStampReader.StampProtocol.DoQ => DnsEnums.DnsProtocol.DoQ,
            DNSCryptStampReader.StampProtocol.ObliviousDohTarget => DnsEnums.DnsProtocol.ObliviousDohTarget,
            DNSCryptStampReader.StampProtocol.AnonymizedDNSCryptRelay => DnsEnums.DnsProtocol.AnonymizedDNSCryptRelay,
            DNSCryptStampReader.StampProtocol.ObliviousDohRelay => DnsEnums.DnsProtocol.ObliviousDohRelay,
            DNSCryptStampReader.StampProtocol.Unknown => DnsEnums.DnsProtocol.Unknown,
            _ => DnsEnums.DnsProtocol.Unknown,
        };
        return protocol;
    }

}