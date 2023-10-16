using System;
using System.Diagnostics;
using System.Text;

namespace MsmhToolsClass.DnsTool;

public class DNSCryptStampReader
{
    // More info: https://dnscrypt.info/stamps-specifications/
    public bool IsDecryptionSuccess { get; private set; } = false;
    public string Stamp { get; private set; } = string.Empty;
    public string IP { get; private set; } = string.Empty;
    public int Port { get; private set; } = -1;
    public string Host { get; private set; } = string.Empty;
    public string Path { get; private set; } = string.Empty;
    public string PublicKey { get; private set; } = string.Empty;
    public string ProviderName { get; private set; } = string.Empty;
    public List<string> Hashi { get; private set; } = new();
    public List<string> Bootstraps { get; private set; } = new();
    public StampProtocol Protocol { get; private set; } = StampProtocol.Unknown;
    public string ProtocolName { get; private set; } = StampProtocolName.Unknown;
    public bool IsDnsSec { get; set; } = false;
    public bool IsNoLog { get; set; } = false;
    public bool IsNoFilter { get; set; } = false;

    public static class DefaultPort
    {
        public static readonly int PlainDNS = 53;
        public static readonly int DnsCrypt = 443;
        public static readonly int DoH = 443;
        public static readonly int DoT = 853;
        public static readonly int DoQ = 853;
        public static readonly int ObliviousDohTarget = 443;
        public static readonly int AnonymizedDNSCryptRelay = 443;
        public static readonly int ObliviousDohRelay = 443;
    }

    public enum StampProtocol
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

    private struct StampProtocolName
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

    public DNSCryptStampReader(string stamp)
    {
        Stamp = stamp;

        if (stamp.StartsWith("sdns://"))
        {
            try
            {
                // Strip sdns://
                stamp = stamp[7..];

                // Get Stamp Binary
                byte[] stampBinary = EncodingTool.UrlDecode(stamp);

                // Get Protocol
                if (stampBinary.Length > 0)
                {
                    Protocol = GetProtocol(stampBinary, out string protocolName);
                    ProtocolName = protocolName;
                }

                // Get Properties
                if (Protocol != StampProtocol.AnonymizedDNSCryptRelay) // Anonymized DNSCrypt Relay doesn't have properties
                {
                    if (stampBinary.Length > 1)
                    {
                        GetStampProperties(stampBinary, out bool isDNSSec, out bool isNoLog, out bool isNoFilter);
                        IsDnsSec = isDNSSec;
                        IsNoLog = isNoLog;
                        IsNoFilter = isNoFilter;
                    }
                }

                // Get IP, Port, Host, Path, PublicKey, ProviderName, Hashi, Bootstraps
                bool isOk = DecryptTheRest(stampBinary, out string ip, out int port, out string host, out string path, out string publicKey,
                                            out string providerName, out List<string> hashi, out List<string> bootstraps);

                if (!isOk) return;

                IP = ip;
                Port = port;
                Host = host;
                Path = path;
                PublicKey = publicKey;
                ProviderName = providerName;
                Hashi = new(hashi);
                Bootstraps = new(bootstraps);

                IsDecryptionSuccess = true;
            }
            catch (Exception)
            {
                // do nothing
            }
        }
        else
        {
            Debug.WriteLine("\"sdns://\" is missing.");
        }
    }

    private static StampProtocol GetProtocol(byte[] stampBinary, out string protocolName)
    {
        byte stampProtocol = stampBinary[0];

        if (stampProtocol == 0x00)
        {
            protocolName = StampProtocolName.PlainDNS;
            return StampProtocol.PlainDNS;
        }
        else if (stampProtocol == 0x01)
        {
            protocolName = StampProtocolName.DnsCrypt;
            return StampProtocol.DnsCrypt;
        }
        else if (stampProtocol == 0x02)
        {
            protocolName = StampProtocolName.DoH;
            return StampProtocol.DoH;
        }
        else if (stampProtocol == 0x03)
        {
            protocolName = StampProtocolName.DoT;
            return StampProtocol.DoT;
        }
        else if (stampProtocol == 0x04)
        {
            protocolName = StampProtocolName.DoQ;
            return StampProtocol.DoQ;
        }
        else if (stampProtocol == 0x05)
        {
            protocolName = StampProtocolName.ObliviousDohTarget;
            return StampProtocol.ObliviousDohTarget;
        }
        else if (stampProtocol == 0x81)
        {
            protocolName = StampProtocolName.AnonymizedDNSCryptRelay;
            return StampProtocol.AnonymizedDNSCryptRelay;
        }
        else if (stampProtocol == 0x85)
        {
            protocolName = StampProtocolName.ObliviousDohRelay;
            return StampProtocol.ObliviousDohRelay;
        }
        else
        {
            protocolName = StampProtocolName.Unknown;
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

    private bool DecryptTheRest(byte[] stampBinary, out string ip, out int port, out string host, out string path, out string publicKey,
                                                    out string providerName, out List<string> hashi, out List<string> bootstraps)
    {
        ip = string.Empty;
        port = -1;
        host = string.Empty;
        path = string.Empty;
        publicKey = string.Empty;
        providerName = string.Empty;
        hashi = new List<string>();
        bootstraps = new List<string>();

        int position = 0;
        position += 1; // Skip Protocol
        if (Protocol == StampProtocol.PlainDNS)
        {
            position += 8; // Skip Properties

            // LP(addr [:port])
            position = LPHostIpPort(position, stampBinary, DefaultPort.PlainDNS, out ip, out port);
            if (position == -1) return false;
            return true;
        }
        else if (Protocol == StampProtocol.DnsCrypt)
        {
            position += 8; // Skip Properties

            // LP(addr [:port])
            position = LPHostIpPort(position, stampBinary, DefaultPort.DnsCrypt, out ip, out port);
            if (position == -1) return false;

            // LP(pk)
            position = LPPublicKey(position, stampBinary, out publicKey);
            if (position == -1) return false;

            // LP(providerName))
            position = LP(position, stampBinary, out providerName);
            if (position == -1) return false;
            return true;
        }
        else if (Protocol == StampProtocol.DoH)
        {
            position += 8; // Skip Properties

            // LP(addr)
            position = LPHostIpPort(position, stampBinary, DefaultPort.DoH, out ip, out port);
            if (position == -1) return false;

            // VLP(hash1, hash2, ...hashn)
            position = VLPHash(position, stampBinary, out hashi);
            if (position == -1) return false;

            // LP(hostname[:port])
            position = LPHostIpPort(position, stampBinary, port, out host, out port);
            if (position == -1) return false;

            // LP(path)
            position = LP(position, stampBinary, out path);
            if (position == -1) return false;

            // VLP(bootstrap_ip1, bootstrap_ip2, ...bootstrap_ipn) - Optional
            position = VLPBootstrap(position, stampBinary, out bootstraps);
            if (position == -1) return false;
            return true;
        }
        else if (Protocol == StampProtocol.DoT)
        {
            position += 8; // Skip Properties

            // LP(addr)
            position = LPHostIpPort(position, stampBinary, DefaultPort.DoT, out ip, out port);
            if (position == -1) return false;

            // VLP(hash1, hash2, ...hashn)
            position = VLPHash(position, stampBinary, out hashi);
            if (position == -1) return false;

            // LP(hostname [:port])
            position = LPHostIpPort(position, stampBinary, port, out host, out port);
            if (position == -1) return false;

            // VLP(bootstrap_ip1, bootstrap_ip2, ...bootstrap_ipn) - Optional
            position = VLPBootstrap(position, stampBinary, out bootstraps);
            if (position == -1) return false;
            return true;
        }
        else if (Protocol == StampProtocol.DoQ)
        {
            position += 8; // Skip Properties

            // LP(addr)
            position = LPHostIpPort(position, stampBinary, DefaultPort.DoQ, out ip, out port);
            if (position == -1) return false;

            // VLP(hash1, hash2, ...hashn)
            position = VLPHash(position, stampBinary, out hashi);
            if (position == -1) return false;

            // LP(hostname [:port])
            position = LPHostIpPort(position, stampBinary, port, out host, out port);
            if (position == -1) return false;

            // VLP(bootstrap_ip1, bootstrap_ip2, ...bootstrap_ipn) - Optional
            position = VLPBootstrap(position, stampBinary, out bootstraps);
            if (position == -1) return false;
            return true;
        }
        else if (Protocol == StampProtocol.ObliviousDohTarget)
        {
            position += 8; // Skip Properties

            // LP(hostname [:port])
            position = LPHostIpPort(position, stampBinary, DefaultPort.ObliviousDohTarget, out host, out port);
            if (position == -1) return false;

            // LP(path)
            position = LP(position, stampBinary, out path);
            if (position == -1) return false;
            return true;
        }
        else if (Protocol == StampProtocol.AnonymizedDNSCryptRelay)
        {
            // Anonymized DNSCrypt Relay doesn't have properties to skip

            // LP(addr)
            position = LPHostIpPort(position, stampBinary, DefaultPort.AnonymizedDNSCryptRelay, out ip, out port);
            if (position == -1) return false;
            return true;
        }
        else if (Protocol == StampProtocol.ObliviousDohRelay)
        {
            position += 8; // Skip Properties

            // LP(addr)
            position = LPHostIpPort(position, stampBinary, DefaultPort.ObliviousDohRelay, out ip, out port);
            if (position == -1) return false;

            // VLP(hash1, hash2, ...hashn)
            position = VLPHash(position, stampBinary, out hashi);
            if (position == -1) return false;

            // LP(hostname [:port])
            position = LPHostIpPort(position, stampBinary, port, out host, out port);
            if (position == -1) return false;

            // LP(path)
            position = LP(position, stampBinary, out path);
            if (position == -1) return false;

            // VLP(bootstrap_ip1, bootstrap_ip2, ...bootstrap_ipn) - Optional
            position = VLPBootstrap(position, stampBinary, out bootstraps);
            if (position == -1) return false;
            return true;
        }

        return true;
    }

    private int LPHostIpPort(int position, byte[] stampBinary, int defaultPort, out string host, out int port)
    {
        try
        {
            if (stampBinary.Length <= position)
            {
                host = string.Empty;
                port = -1;
                return -1;
            }

            int hostPortLength = Convert.ToInt32(stampBinary[position]);
            position += 1; // Skip Host:Port Length

            byte[] bHostPort = new byte[hostPortLength];
            if (stampBinary.Length >= position + hostPortLength)
                Buffer.BlockCopy(stampBinary, position, bHostPort, 0, hostPortLength);

            string hostPort = Encoding.UTF8.GetString(bHostPort);
            NetworkTool.GetHostDetails(hostPort, defaultPort, out host, out _, out port, out _, out bool _);
            position += hostPortLength; // Skip Host:Port

            return position;
        }
        catch (Exception)
        {
            host = string.Empty;
            port = -1;
            return -1;
        }
    }

    private int LP(int position, byte[] stampBinary, out string outResult)
    {
        try
        {
            if (stampBinary.Length <= position)
            {
                outResult = string.Empty;
                return -1;
            }

            int inputLength = Convert.ToInt32(stampBinary[position]);
            position += 1; // Skip Input Length

            byte[] bInput = new byte[inputLength];
            if (stampBinary.Length >= position + inputLength)
                Buffer.BlockCopy(stampBinary, position, bInput, 0, inputLength);

            outResult = Encoding.UTF8.GetString(bInput);
            position += inputLength; // Skip Input

            return position;
        }
        catch (Exception)
        {
            outResult = string.Empty;
            return -1;
        }
    }

    private int LPPublicKey(int position, byte[] stampBinary, out string outResult)
    {
        try
        {
            if (stampBinary.Length <= position)
            {
                outResult = string.Empty;
                return -1;
            }

            int inputLength = Convert.ToInt32(stampBinary[position]);
            position += 1; // Skip Input Length

            byte[] bInput = new byte[inputLength];
            if (stampBinary.Length >= position + inputLength)
                Buffer.BlockCopy(stampBinary, position, bInput, 0, inputLength);

            outResult = Convert.ToHexString(bInput).ToLower();
            position += inputLength; // Skip Input

            return position;
        }
        catch (Exception)
        {
            outResult = string.Empty;
            return -1;
        }
    }

    private int VLPHash(int position, byte[] stampBinary, out List<string> hashi)
    {
        try
        {
            List<string> hashi0 = new();
            while (position < stampBinary.Length - 1)
            {
                bool last = false;

                if (stampBinary.Length <= position)
                {
                    hashi = new(hashi0);
                    return -1;
                }

                int vlen = Convert.ToInt32(stampBinary[position]);

                if ((vlen & 0x80) == 0x80) vlen ^= 0x80;
                else last = true;

                position += 1; // Skip Hash Length

                if (vlen > 0)
                {
                    byte[] bHash = new byte[vlen];
                    if (stampBinary.Length >= position + vlen)
                        Buffer.BlockCopy(stampBinary, position, bHash, 0, vlen);

                    string hash = Convert.ToHexString(bHash).ToLower();
                    hashi0.Add(hash);
                }

                position += vlen; // Skip Hash
                if (last) break;
            }

            hashi = new(hashi0);

            return position;
        }
        catch (Exception)
        {
            hashi = new();
            return -1;
        }
    }

    private int VLPBootstrap(int position, byte[] stampBinary, out List<string> bootstraps)
    {
        try
        {
            List<string> bootstraps0 = new();
            while (position < stampBinary.Length - 1)
            {
                bool last = false;

                if (stampBinary.Length <= position)
                {
                    bootstraps = new(bootstraps0);
                    return -1;
                }

                int vlen = Convert.ToInt32(stampBinary[position]);

                if ((vlen & 0x80) == 0x80) vlen ^= 0x80;
                else last = true;

                position += 1; // Skip Bootstrap Length

                if (vlen > 0)
                {
                    byte[] bBootstrap = new byte[vlen];
                    if (stampBinary.Length >= position + vlen)
                        Buffer.BlockCopy(stampBinary, position, bBootstrap, 0, vlen);

                    string bootstrap = Encoding.UTF8.GetString(bBootstrap);
                    bootstraps0.Add(bootstrap);
                }

                position += vlen; // Skip Bootstrap
                if (last) break;
            }

            bootstraps = new(bootstraps0);

            return position;
        }
        catch (Exception)
        {
            bootstraps = new();
            return -1;
        }
    }

}