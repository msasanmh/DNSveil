using System.Diagnostics;
using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class DnsReader
{
    public string Dns { get; private set; } = string.Empty;
    public string DnsWithRelay { get; private set; } = string.Empty;
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
    public IPAddress DNSCryptRelayIP { get; private set; } = IPAddress.None;
    public int DNSCryptRelayPort { get; private set; } = 0;
    public string ODoHRelayAddress { get; private set; } = string.Empty;

    public DnsReader() { }

    /// <summary>
    /// Read any DNS
    /// </summary>
    /// <param name="dns">DNS Address</param>
    /// <param name="companyNameDataFileContent">File content to get company name. each line e.g. 8.8.8.8|Google Inc.</param>
    public DnsReader(string dns, string? companyNameDataFileContent = null)
    {
        Dns = dns.Trim();

        if (!string.IsNullOrEmpty(companyNameDataFileContent))
            CompanyNameDataFileContent = companyNameDataFileContent;

        Protocol = DnsEnums.DnsProtocol.Unknown;
        ProtocolName = DnsEnums.DnsProtocolName.Unknown;

        if (string.IsNullOrEmpty(Dns)) return;
        
        try
        {
            // Support For DNSCrypt Relay, Oblivious DoH Relay
            string relay = string.Empty;
            if (Dns.Contains(' '))
            {
                string[] split = Dns.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (split.Length > 1)
                {
                    Dns = split[0];
                    relay = split[1];
                }
            }

            // Set Dns With Relay Address
            DnsWithRelay = Dns;
            if (!string.IsNullOrEmpty(relay)) DnsWithRelay += $" {relay}";

            if (Dns.ToLower().StartsWith("sdns://"))
            {
                // Decode Stamp
                DNSCryptStampReader stampReader = new(Dns);
                if (stampReader != null && stampReader.IsDecryptionSuccess)
                {
                    IsDnsCryptStamp = true;
                    Scheme = "stamp://";
                    Host = stampReader.Host;
                    if (!string.IsNullOrEmpty(Host))
                        IsHostIP = NetworkTool.IsIP(Host, out _);
                    IP = stampReader.IP;
                    Port = stampReader.Port;
                    Path = stampReader.Path;
                    Protocol = ParseProtocol(stampReader.Protocol);

                    if (Protocol == DnsEnums.DnsProtocol.UDP) Scheme = "udp://";
                    else if (Protocol == DnsEnums.DnsProtocol.TCP) Scheme = "tcp://";
                    else if (Protocol == DnsEnums.DnsProtocol.TcpOverUdp) Scheme = string.Empty; // Just IP:Port
                    else if (Protocol == DnsEnums.DnsProtocol.DoT) Scheme = "tls://";
                    else if (Protocol == DnsEnums.DnsProtocol.DoH) Scheme = "https://";
                    else if (Protocol == DnsEnums.DnsProtocol.DoQ) Scheme = "quic://";
                    else if (Protocol == DnsEnums.DnsProtocol.ObliviousDohTarget) Scheme = "https://";
                    else if (Protocol == DnsEnums.DnsProtocol.ObliviousDohRelay) Scheme = "https://";

                    ProtocolName = stampReader.ProtocolName;
                    StampReader = stampReader;

                    if (Protocol == DnsEnums.DnsProtocol.DnsCrypt)
                    {
                        if (!string.IsNullOrEmpty(relay)) SetDNSCryptRelay(relay);
                    }
                    else if (Protocol == DnsEnums.DnsProtocol.ObliviousDohTarget)
                    {
                        if (!string.IsNullOrEmpty(relay)) SetODoHRelay(relay);
                        else
                        {
                            // If There Is No Relay Treat As DoH
                            Protocol = DnsEnums.DnsProtocol.DoH;
                            ProtocolName = DnsEnums.DnsProtocolName.DoH;
                        }
                    }

                    // Get Company Name (SDNS)
                    string stampHost = stampReader.Host;
                    if (string.IsNullOrEmpty(stampHost)) stampHost = stampReader.IP.ToStringNoScopeId();
                    if (string.IsNullOrEmpty(stampHost)) stampHost = stampReader.ProviderName;
                    if (!string.IsNullOrEmpty(CompanyNameDataFileContent))
                        CompanyName = GetCompanyName.HostToCompanyOffline(stampHost, CompanyNameDataFileContent);
                }
            }
            else
            {
                if (Dns.ToLower().StartsWith("http://") || Dns.ToLower().StartsWith("https://") || Dns.ToLower().StartsWith("h3://"))
                {
                    // DoH
                    SetIpPortHostPath(Dns, 443);

                    Protocol = DnsEnums.DnsProtocol.DoH;
                    ProtocolName = DnsEnums.DnsProtocolName.DoH;

                    if (!string.IsNullOrEmpty(relay)) SetODoHRelay(relay);
                }
                else if (Dns.ToLower().StartsWith("tls://"))
                {
                    // TLS
                    SetIpPortHostPath(Dns, 853);

                    Protocol = DnsEnums.DnsProtocol.DoT;
                    ProtocolName = DnsEnums.DnsProtocolName.DoT;
                }
                else if (Dns.ToLower().StartsWith("quic://"))
                {
                    // DoQ
                    SetIpPortHostPath(Dns, 853);

                    Protocol = DnsEnums.DnsProtocol.DoQ;
                    ProtocolName = DnsEnums.DnsProtocolName.DoQ;
                }
                else if (Dns.ToLower().StartsWith("udp://"))
                {
                    // Plain DNS UDP
                    SetIpPortHostPath(Dns, 53);

                    Protocol = DnsEnums.DnsProtocol.UDP;
                    ProtocolName = DnsEnums.DnsProtocolName.UDP;
                }
                else if (Dns.ToLower().StartsWith("tcp://"))
                {
                    // Plain DNS TCP
                    SetIpPortHostPath(Dns, 53);

                    Protocol = DnsEnums.DnsProtocol.TCP;
                    ProtocolName = DnsEnums.DnsProtocolName.TCP;
                }
                else if (Dns.ToLower().Equals("system"))
                {
                    // System
                    Protocol = DnsEnums.DnsProtocol.System;
                    ProtocolName = DnsEnums.DnsProtocolName.System;
                }
                else
                {
                    NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(Dns, 53);

                    if (NetworkTool.IsIP(urid.Host, out _))
                    {
                        // Plain DNS TCP-Over-UDP
                        Dns = $"{urid.Host}:{urid.Port}";
                        DnsWithRelay = Dns;
                        SetIpPortHostPath(Dns, 53);

                        Protocol = DnsEnums.DnsProtocol.TcpOverUdp;
                        ProtocolName = DnsEnums.DnsProtocolName.TcpOverUdp;
                    }
                    else
                    {
                        // Unknown
                        Protocol = DnsEnums.DnsProtocol.Unknown;
                        ProtocolName = DnsEnums.DnsProtocolName.Unknown;
                    }
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
            NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(dns, defaultPort);
            Scheme = urid.Scheme.ToLower();
            Host = urid.Host;
            IsHostIP = NetworkTool.IsIP(Host, out _);
            Port = urid.Port;
            Path = urid.Path;
            if (!string.IsNullOrEmpty(Path) && !Path.StartsWith('/')) Path = $"/{Path}";

            if (!string.IsNullOrEmpty(CompanyNameDataFileContent))
            {
                string? ipOrHost = Host;
                if (string.IsNullOrEmpty(ipOrHost)) ipOrHost = IP.ToStringNoScopeId();
                if (string.IsNullOrEmpty(ipOrHost)) ipOrHost = urid.Host;
                CompanyName = GetCompanyName.HostToCompanyOffline(ipOrHost, CompanyNameDataFileContent);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsReader SetIpPortHostPath: " + ex.Message);
        }
    }

    private void SetODoHRelay(string relay)
    {
        if (relay.ToLower().StartsWith("sdns://"))
        {
            DNSCryptStampReader sr = new(relay);
            if (sr.Protocol == DNSCryptStampReader.StampProtocol.ObliviousDohRelay)
            {
                Protocol = DnsEnums.DnsProtocol.ObliviousDoH;
                ProtocolName = DnsEnums.DnsProtocolName.ObliviousDoH;
                
                ODoHRelayAddress = relay;
            }
        }
        else if (relay.ToLower().StartsWith("https://"))
        {
            Protocol = DnsEnums.DnsProtocol.ObliviousDoH;
            ProtocolName = DnsEnums.DnsProtocolName.ObliviousDoH;

            ODoHRelayAddress = relay;
        }
    }

    private void SetDNSCryptRelay(string relay)
    {
        if (relay.ToLower().StartsWith("sdns://"))
        {
            DNSCryptStampReader sr = new(relay);
            if (sr.Protocol == DNSCryptStampReader.StampProtocol.AnonymizedDNSCryptRelay)
            {
                Protocol = DnsEnums.DnsProtocol.AnonymizedDNSCrypt;
                ProtocolName = DnsEnums.DnsProtocolName.AnonymizedDNSCrypt;

                DNSCryptRelayIP = sr.IP;
                DNSCryptRelayPort = sr.Port;
            }
        }
        else
        {
            NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(relay, 0);
            if (urid.Port != 0)
            {
                bool isIp = NetworkTool.IsIP(urid.Host, out IPAddress? ip);
                if (isIp && ip != null)
                {
                    Protocol = DnsEnums.DnsProtocol.AnonymizedDNSCrypt;
                    ProtocolName = DnsEnums.DnsProtocolName.AnonymizedDNSCrypt;

                    DNSCryptRelayIP = ip;
                    DNSCryptRelayPort = urid.Port;
                }
            }
        }
    }

    private static DnsEnums.DnsProtocol ParseProtocol(DNSCryptStampReader.StampProtocol stampProtocol)
    {
        var protocol = stampProtocol switch
        {
            DNSCryptStampReader.StampProtocol.PlainDNS => DnsEnums.DnsProtocol.TcpOverUdp,
            DNSCryptStampReader.StampProtocol.DnsCrypt => DnsEnums.DnsProtocol.DnsCrypt,
            DNSCryptStampReader.StampProtocol.DoT => DnsEnums.DnsProtocol.DoT,
            DNSCryptStampReader.StampProtocol.DoH => DnsEnums.DnsProtocol.DoH,
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