using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MsmhTools
{
    public class DNSCryptStampReader
    {
        public string IP { get; private set; }
        public int Port { get; private set; }
        public string Host { get; private set; }
        public string Path { get; private set; }
        public StampProtocol Protocol { get; private set; }
        public string ProtocolName { get; private set; }
        public bool IsDnsSec { get; set; }
        public bool IsNoLog { get; set; }
        public bool IsNoFilter { get; set; }

        public enum StampProtocol
        {
            PlainDNS,
            DnsCrypt,
            DoH,
            TLS,
            DNSCryptRelay,
            Unknown
        }

        private struct StampProtocolName
        {
            public string PlainDNS = "Plain DNS";
            public string DnsCrypt = "DNSCrypt";
            public string DoH = "DNS-Over-HTTPS";
            public string TLS = "TLS";
            public string DNSCryptRelay = "DNSCrypt Relay";
            public string Unknown = "Unknown";
        }

        public DNSCryptStampReader(string stamp)
        {
            if (stamp.StartsWith("sdns://"))
            {
                stamp = stamp[7..];
                byte[] stampBinary = EncodingTool.UrlDecode(stamp);

                Protocol = GetProtocol(stampBinary, out string protocolName);
                ProtocolName = protocolName;

                GetStampProperties(stampBinary, out bool isDNSSec, out bool isNoLog, out bool isNoFilter);
                IsDnsSec = isDNSSec;
                IsNoLog = isNoLog;
                IsNoFilter = isNoFilter;

                IP = GetIpPortFromStamp(stampBinary, out int port1);
                
                Host = GetHostPathFromStamp(stampBinary, out int port2, out string path);
                Path = path;

                // Get Port
                if (port1 != 0)
                    Port = port1;
                else
                {
                    if (port2 != 0)
                        Port = port2;
                    else
                    {
                        if (Protocol == StampProtocol.PlainDNS)
                            Port = 53;
                        else
                            Port = 443;
                    }
                }
            }
            else
            {
                throw new ArgumentException("\"sdns://\" is missing.");
            }
        }

        private static StampProtocol GetProtocol(byte[] stampBinary, out string protocolName)
        {
            StampProtocolName stampProtocolName = new();

            if (stampBinary[0] == 0x00)
            {
                protocolName = stampProtocolName.PlainDNS;
                return StampProtocol.PlainDNS;
            }
            else if (stampBinary[0] == 0x01)
            {
                protocolName = stampProtocolName.DnsCrypt;
                return StampProtocol.DnsCrypt;
            }
            else if (stampBinary[0] == 0x02)
            {
                protocolName = stampProtocolName.DoH;
                return StampProtocol.DoH;
            }
            else if (stampBinary[0] == 0x03)
            {
                protocolName = stampProtocolName.TLS;
                return StampProtocol.TLS;
            }
            else if (stampBinary[0] == 0x81)
            {
                protocolName = stampProtocolName.DNSCryptRelay;
                return StampProtocol.DNSCryptRelay;
            }
            else
            {
                protocolName = stampProtocolName.Unknown;
                return StampProtocol.Unknown;
            }
        }

        private static void GetStampProperties(byte[] stampBinary, out bool isDNSSec, out bool isNoLog, out bool isNoFilter)
        {
            byte dnsCryptProperties = stampBinary[1];

            isDNSSec = Convert.ToBoolean((dnsCryptProperties >> 0) & 1);
            isNoLog = Convert.ToBoolean((dnsCryptProperties >> 1) & 1);
            isNoFilter = Convert.ToBoolean((dnsCryptProperties >> 2) & 1);
        }

        private static string GetIpPortFromStamp(byte[] stampBinary, out int port)
        {
            byte[] data = EncodingTool.SubArray(stampBinary, 9, stampBinary.Length - 9);
            string result = Encoding.UTF8.GetString(data);
            StringBuilder sb = new();
            char[] characters = result.ToCharArray();
            for (int n1 = 0; n1 < characters.Length; n1++)
            {
                char c = characters[n1];
                if (char.IsLetter(c) || char.IsDigit(c) || c == '.' || c == ':' || c == '[' || c == ']')
                {
                    sb.Append(c);
                    if (n1 < characters.Length - 1) // Checking the next char
                    {
                        char cNext = characters[n1 + 1];
                        if (!char.IsLetter(cNext) && !char.IsDigit(cNext) && cNext != '.' && cNext != ':' && cNext != '[' && cNext != ']')
                            break;
                    }
                }
            }
            string ipPort = sb.ToString();
            string ipPortOut = GetHostIPv4IPv6(ipPort, out int port0);
            port = port0;
            bool isIpv6 = IPAddress.TryParse(ipPortOut.Replace("[", string.Empty).Replace("]", string.Empty), out IPAddress _);
            if (isIpv6)
                return ipPortOut;
            else
            {
                bool isIpv4Valid = Network.IsIPv4Valid(ipPortOut, out IPAddress _);
                if (isIpv4Valid)
                    return ipPortOut;
                else
                {
                    port = 0;
                    return string.Empty;
                }
            }
        }

        private static string GetHostPathFromStamp(byte[] stampBinary, out int port, out string path)
        {
            bool goForIt = false;
            bool hasPath = false;
            int countNotAllowedChars = 0;
            char[] allowedChars1 = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            char[] allowedChars2 = { '.', ':', '[', ']', '/', '-', '?', '='};
            char[] allowedChars = allowedChars1.Concat(allowedChars2).ToArray();
            byte[] data = EncodingTool.SubArray(stampBinary, 9, stampBinary.Length - 9);
            string result = Encoding.UTF8.GetString(data);
            char[] characters = result.ToCharArray();

            StringBuilder sb = new();
            for (int n = characters.Length - 1; n >= 0; n--)
            {
                char c = characters[n];

                if (!goForIt)
                {
                    for (int n1 = 0; n1 < allowedChars1.Length; n1++)
                    {
                        char allowedChar1 = allowedChars1[n1];
                        if (c == allowedChar1)
                        {
                            goForIt = true;
                            break;
                        }
                    }
                }

                if (goForIt)
                {
                    bool isAllowdChar = false;
                    for (int n1 = 0; n1 < allowedChars.Length; n1++)
                    {
                        char allowedChar = allowedChars[n1];
                        if (c == allowedChar)
                        {
                            isAllowdChar = true;
                            break;
                        }
                    }

                    if (isAllowdChar)
                        sb.Append(c);
                    else
                        countNotAllowedChars++;

                    if (c == '/')
                        hasPath = true;

                    if (hasPath)
                    {
                        if (countNotAllowedChars > 1)
                            break;
                    }
                    else
                    {
                        if (countNotAllowedChars > 0)
                            break;
                    }
                }
            }
            char[] reverse = sb.ToString().ToArray();
            Array.Reverse(reverse);
            string hostPath = new(reverse);
            if (hostPath.Contains('/')) // Host + Path
            {
                string[] split = hostPath.Split('/');
                path = $"/{split[1]}";
                string hostPathOut = GetHostIPv4IPv6(split[0], out int port0);
                port = port0;
                return hostPathOut;
            }
            else // Host
            {
                path = string.Empty;
                string hostPathOut = GetHostIPv4IPv6(hostPath, out int port0);
                port = port0;
                return hostPathOut;
            }
        }

        private static string GetHostIPv4IPv6(string hostIpPort, out int port)
        {
            if (hostIpPort.Contains('[') && hostIpPort.Contains("]:")) // IPv6 + Port
            {
                string[] split = hostIpPort.Split("]:");
                port = int.Parse(split[1]);
                return $"{split[0]}]";
            }
            else if (hostIpPort.Contains('[') && hostIpPort.Contains(']')) // IPv6
            {
                string[] split = hostIpPort.Split(']');
                port = 0;
                return $"{split[0]}]";
            }
            else if (hostIpPort.Contains(':')) // Host + Port OR IPv4 + Port
            {
                string[] split = hostIpPort.Split(':');
                port = int.Parse(split[1]);
                return split[0];
            }
            else // Host OR IPv4
            {
                port = 0;
                return hostIpPort;
            }
        }


    }
}
