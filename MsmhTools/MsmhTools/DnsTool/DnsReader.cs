using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;

namespace MsmhTools.DnsTool
{
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
                else
                {
                    if (dns.Contains(':'))
                    {
                        string[] split = dns.Split(':');
                        string ip = split[0];
                        string port = split[1];
                        if (Network.IsIPv4Valid(ip, out IPAddress _))
                        {
                            if (!string.IsNullOrEmpty(port))
                            {
                                bool isPortValid = int.TryParse(port, out int outPort);
                                if (isPortValid && outPort >= 1 && outPort <= 65535)
                                {
                                    // Plain DNS
                                    IP = ip;
                                    if (!string.IsNullOrEmpty(CompanyNameDataFileContent))
                                        CompanyName = GetCompanyName(ip, CompanyNameDataFileContent);
                                    Port = outPort;
                                    Protocol = DnsProtocol.PlainDNS;
                                    ProtocolName = DnsProtocolName.PlainDNS;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SetIpPortHostPath(string dns, int defaultPort)
        {
            Network.GetUrlDetails(dns, defaultPort, out string host, out int port, out string path, out bool isIPv6);
            Port = port;
            Path = path;
            bool isIPv4 = Network.IsIPv4Valid(host, out IPAddress? _);
            if (isIPv6 || isIPv4)
            {
                IP = host;
            }
            else
            {
                Host = host;
                IPAddress? ip = Network.HostToIP(host);
                if (ip != null)
                    IP = ip.ToString();
            }
            if (!string.IsNullOrEmpty(CompanyNameDataFileContent))
                CompanyName = GetCompanyName(host, CompanyNameDataFileContent);
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
}
