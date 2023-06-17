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
            if (!string.IsNullOrEmpty(companyNameDataFileContent))
                CompanyNameDataFileContent = companyNameDataFileContent;

            DnsProtocolName protocolName = new();
            Protocol = DnsProtocol.Unknown;
            ProtocolName = protocolName.Unknown;

            if (dns.ToLower().StartsWith("sdns://"))
            {
                IsDnsCryptStamp = true;

                // Decode Stamp
                DNSCryptStampReader stamp = new(dns);
                if (stamp != null)
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
                    if (string.IsNullOrEmpty(stampHost))
                        stampHost = stamp.IP;
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
                    ProtocolName = protocolName.DoH;
                }
                else if (dns.ToLower().StartsWith("tls://"))
                {
                    // TLS
                    SetIpPortHostPath(dns, 853);

                    Protocol = DnsProtocol.TLS;
                    ProtocolName = protocolName.TLS;
                }
                else if (dns.ToLower().StartsWith("quic://"))
                {
                    // DoQ
                    SetIpPortHostPath(dns, 853);

                    Protocol = DnsProtocol.DoQ;
                    ProtocolName = protocolName.DoQ;
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
                                    ProtocolName = protocolName.PlainDNS;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SetIpPortHostPath(string dns, int defaultPort)
        {
            string host = Network.UrlToHostAndPort(dns, defaultPort, out int port, out string path, out bool isIPv6);
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
                DNSCryptStampReader.StampProtocol.DoH => DnsProtocol.DoH,
                DNSCryptStampReader.StampProtocol.TLS => DnsProtocol.TLS,
                DNSCryptStampReader.StampProtocol.DnsCrypt => DnsProtocol.DnsCrypt,
                DNSCryptStampReader.StampProtocol.DNSCryptRelay => DnsProtocol.DNSCryptRelay,
                DNSCryptStampReader.StampProtocol.PlainDNS => DnsProtocol.PlainDNS,
                DNSCryptStampReader.StampProtocol.Unknown => DnsProtocol.Unknown,
                _ => DnsProtocol.Unknown,
            };
            return protocol;
        }

        public enum DnsProtocol
        {
            DoH,
            TLS,
            DnsCrypt,
            DNSCryptRelay,
            DoQ,
            PlainDNS,
            Unknown
        }

        private struct DnsProtocolName
        {
            public string DoH = "DNS-Over-HTTPS";
            public string TLS = "DNS-Over-TLS";
            public string DnsCrypt = "DNSCrypt";
            public string DNSCryptRelay = "DNSCrypt Relay";
            public string DoQ = "DNS-Over-Quic";
            public string PlainDNS = "Plain DNS";
            public string Unknown = "Unknown";
        }


    }
}
