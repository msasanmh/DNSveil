using System.Diagnostics;
using System.Text;

namespace MsmhToolsClass.MsmhAgnosticServer;

public static class DNSCryptStampGenerator
{
    // More info: https://dnscrypt.info/stamps-specifications/

    /// <summary>
    /// Generate Plain DNS Stamp
    /// </summary>
    /// <param name="ip">IP Address (IPv6 addresses must be in brackets)</param>
    /// <param name="isDNSSec">Is DNSSec</param>
    /// <param name="isNoLog">Is no log</param>
    /// <param name="isNoFilter">Is no filter</param>
    /// <returns>Returns stamp or string.Empty if fail</returns>
    public static string GeneratePlainDns(string ipPort, bool isDNSSec, bool isNoLog, bool isNoFilter)
    {
        ipPort = ipPort.Trim();
        string sdns = string.Empty;

        try
        {
            byte[] bDns = new byte[] { 0x00 }; // Plain DNS
            bool bPropsBool = TryGet_Properties(isDNSSec, isNoLog, isNoFilter, out byte[] bProps);
            if (!bPropsBool) return sdns;
            bool bDnsIpBool = TryGet_LP(ipPort, out byte[] bDnsIp);
            if (!bDnsIpBool) return sdns;

            byte[] main = bDns.Concat(bProps).Concat(bDnsIp).ToArray();
            sdns = GetSdnsUrl(main);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNSCryptStampGenerator GeneratePlainDns: " + ex.Message);
        }

        return sdns;
    }

    /// <summary>
    /// Generate DNSCrypt Stamp
    /// </summary>
    /// <param name="ipPort">IP Address with optional port (IPv6 addresses must be in brackets)</param>
    /// <param name="publicKey">Public Key</param>
    /// <param name="providerName">Provider Name</param>
    /// <param name="isDNSSec">Is DNSSec</param>
    /// <param name="isNoLog">Is no log</param>
    /// <param name="isNoFilter">Is no filter</param>
    /// <returns>Returns stamp or string.Empty if fail</returns>
    public static string GenerateDNSCrypt(string ipPort, string publicKey, string providerName, bool isDNSSec, bool isNoLog, bool isNoFilter)
    {
        ipPort = ipPort.Trim();
        publicKey = publicKey.Trim();
        providerName = providerName.Trim();
        string sdns = string.Empty;

        try
        {
            byte[] bDns = new byte[] { 0x01 }; // DNSCrypt
            bool bPropsBool = TryGet_Properties(isDNSSec, isNoLog, isNoFilter, out byte[] bProps);
            if (!bPropsBool) return sdns;
            bool bIpPortBool = TryGet_LP(ipPort, out byte[] bIpPort);
            if (!bIpPortBool) return sdns;
            bool bPKBool = TryGet_LPPublicKey(publicKey, out byte[] bPK);
            if (!bPKBool) return sdns;
            bool bPNBool = TryGet_LP(providerName, out byte[] bPN);
            if (!bPNBool) return sdns;

            byte[] main = bDns.Concat(bProps).Concat(bIpPort).Concat(bPK).Concat(bPN).ToArray();
            sdns = GetSdnsUrl(main);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNSCryptStampGenerator GenerateDNSCrypt: " + ex.Message);
        }

        return sdns;
    }

    /// <summary>
    /// Generate DoH Stamp
    /// </summary>
    /// <param name="ip">IP Address (IPv6 addresses must be in brackets)</param>
    /// <param name="hashes">Hashes (comma-separated) - Optional</param>
    /// <param name="hostPort">Host name (vHost+SNI) and optional port number</param>
    /// <param name="path">Path</param>
    /// <param name="bootstraps">Bootstraps (comma-separated) - Optional</param>
    /// <param name="isDNSSec">Is DNSSec</param>
    /// <param name="isNoLog">Is no log</param>
    /// <param name="isNoFilter">Is no filter</param>
    /// <returns>Returns stamp or string.Empty if fail</returns>
    public static string GenerateDoH(string ip, string? hashes, string hostPort, string path, string? bootstraps, bool isDNSSec, bool isNoLog, bool isNoFilter)
    {
        ip = ip.Trim();
        if (!string.IsNullOrEmpty(hashes)) hashes = hashes.Trim();
        hostPort = hostPort.Trim();
        path = string.IsNullOrEmpty(path) ? "/" : path.Trim();
        if (!string.IsNullOrEmpty(bootstraps)) bootstraps = bootstraps.Trim();
        string sdns = string.Empty;

        try
        {
            byte[] bDoh = new byte[] { 0x02 }; // DoH
            bool bPropsBool = TryGet_Properties(isDNSSec, isNoLog, isNoFilter, out byte[] bProps);
            if (!bPropsBool) return sdns;
            bool bDohIpBool = TryGet_LP(ip, out byte[] bDohIp);
            if (!bDohIpBool) return sdns;
            bool bHashBool = TryGet_VLPHash(hashes, out byte[] bHash);
            if (!bHashBool) return sdns;
            bool bhostPortBool = TryGet_LP(hostPort, out byte[] bhostPort);
            if (!bhostPortBool) return sdns;
            bool bPathBool = TryGet_LP(path, out byte[] bPath);
            if (!bPathBool) return sdns;
            bool bBootstrapBool = TryGet_VLPBootstrap(bootstraps, out byte[] bBootstrap);
            if (!bBootstrapBool) return sdns;

            byte[] main = bDoh.Concat(bProps).Concat(bDohIp).Concat(bHash).Concat(bhostPort).Concat(bPath).ToArray();
            if (!string.IsNullOrEmpty(bootstraps))
                main = main.Concat(bBootstrap).ToArray();

            sdns = GetSdnsUrl(main);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNSCryptStampGenerator GenerateDoH: " + ex.Message);
        }

        return sdns;
    }

    /// <summary>
    /// Generate DoT Stamp
    /// </summary>
    /// <param name="ip">IP Address (IPv6 addresses must be in brackets)</param>
    /// <param name="hashes">Hashes (comma-separated) - Optional</param>
    /// <param name="hostPort">Host name (vHost+SNI) and optional port number</param>
    /// <param name="bootstraps">Bootstraps (comma-separated) - Optional</param>
    /// <param name="isDNSSec">Is DNSSec</param>
    /// <param name="isNoLog">Is no log</param>
    /// <param name="isNoFilter">Is no filter</param>
    /// <returns>Returns stamp or string.Empty if fail</returns>
    public static string GenerateDoT(string ip, string? hashes, string hostPort, string? bootstraps, bool isDNSSec, bool isNoLog, bool isNoFilter)
    {
        ip = ip.Trim();
        if (!string.IsNullOrEmpty(hashes)) hashes = hashes.Trim();
        hostPort = hostPort.Trim();
        if (!string.IsNullOrEmpty(bootstraps)) bootstraps = bootstraps.Trim();
        string sdns = string.Empty;

        try
        {
            byte[] bDot = new byte[] { 0x03 }; // DoT
            bool bPropsBool = TryGet_Properties(isDNSSec, isNoLog, isNoFilter, out byte[] bProps);
            if (!bPropsBool) return sdns;
            bool bDotIpBool = TryGet_LP(ip, out byte[] bDotIp);
            if (!bDotIpBool) return sdns;
            bool bHashBool = TryGet_VLPHash(hashes, out byte[] bHash);
            if (!bHashBool) return sdns;
            bool bhostPortBool = TryGet_LP(hostPort, out byte[] bhostPort);
            if (!bhostPortBool) return sdns;
            bool bBootstrapBool = TryGet_VLPBootstrap(bootstraps, out byte[] bBootstrap);
            if (!bBootstrapBool) return sdns;

            byte[] main = bDot.Concat(bProps).Concat(bDotIp).Concat(bHash).Concat(bhostPort).ToArray();
            if (!string.IsNullOrEmpty(bootstraps))
                main = main.Concat(bBootstrap).ToArray();

            sdns = GetSdnsUrl(main);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNSCryptStampGenerator GenerateDoT: " + ex.Message);
        }

        return sdns;
    }

    /// <summary>
    /// Generate DoQ Stamp
    /// </summary>
    /// <param name="ip">IP Address (IPv6 addresses must be in brackets)</param>
    /// <param name="hashes">Hashes (comma-separated) - Optional</param>
    /// <param name="hostPort">Host name (vHost+SNI) and optional port number</param>
    /// <param name="bootstraps">Bootstraps (comma-separated) - Optional</param>
    /// <param name="isDNSSec">Is DNSSec</param>
    /// <param name="isNoLog">Is no log</param>
    /// <param name="isNoFilter">Is no filter</param>
    /// <returns>Returns stamp or string.Empty if fail</returns>
    public static string GenerateDoQ(string ip, string? hashes, string hostPort, string? bootstraps, bool isDNSSec, bool isNoLog, bool isNoFilter)
    {
        ip = ip.Trim();
        if (!string.IsNullOrEmpty(hashes)) hashes = hashes.Trim();
        hostPort = hostPort.Trim();
        if (!string.IsNullOrEmpty(bootstraps)) bootstraps = bootstraps.Trim();
        string sdns = string.Empty;

        try
        {
            byte[] bDoq = new byte[] { 0x04 }; // DoQ
            bool bPropsBool = TryGet_Properties(isDNSSec, isNoLog, isNoFilter, out byte[] bProps);
            if (!bPropsBool) return sdns;
            bool bDoqIpBool = TryGet_LP(ip, out byte[] bDoqIp);
            if (!bDoqIpBool) return sdns;
            bool bHashBool = TryGet_VLPHash(hashes, out byte[] bHash);
            if (!bHashBool) return sdns;
            bool bhostPortBool = TryGet_LP(hostPort, out byte[] bhostPort);
            if (!bhostPortBool) return sdns;
            bool bBootstrapBool = TryGet_VLPBootstrap(bootstraps, out byte[] bBootstrap);
            if (!bBootstrapBool) return sdns;

            byte[] main = bDoq.Concat(bProps).Concat(bDoqIp).Concat(bHash).Concat(bhostPort).ToArray();
            if (!string.IsNullOrEmpty(bootstraps))
                main = main.Concat(bBootstrap).ToArray();

            sdns = GetSdnsUrl(main);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNSCryptStampGenerator GenerateDoQ: " + ex.Message);
        }

        return sdns;
    }

    /// <summary>
    /// Generate Oblivious DoH Target Stamp
    /// </summary>
    /// <param name="hostPort">Host name (vHost+SNI) and optional port number</param>
    /// <param name="path">Path</param>
    /// <param name="isDNSSec">Is DNSSec</param>
    /// <param name="isNoLog">Is no log</param>
    /// <param name="isNoFilter">Is no filter</param>
    /// <returns>Returns stamp or string.Empty if fail</returns>
    public static string GenerateObliviousDohTarget(string hostPort, string path, bool isDNSSec, bool isNoLog, bool isNoFilter)
    {
        hostPort = hostPort.Trim();
        path = string.IsNullOrEmpty(path) ? "/" : path.Trim();
        string sdns = string.Empty;

        try
        {
            byte[] bDns = new byte[] { 0x05 }; // Oblivious DoH Target
            bool bPropsBool = TryGet_Properties(isDNSSec, isNoLog, isNoFilter, out byte[] bProps);
            if (!bPropsBool) return sdns;
            bool bhostPortBool = TryGet_LP(hostPort, out byte[] bhostPort);
            if (!bhostPortBool) return sdns;
            bool bPathBool = TryGet_LP(path, out byte[] bPath);
            if (!bPathBool) return sdns;

            byte[] main = bDns.Concat(bProps).Concat(bhostPort).Concat(bPath).ToArray();
            sdns = GetSdnsUrl(main);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNSCryptStampGenerator GenerateObliviousDohTarget: " + ex.Message);
        }

        return sdns;
    }

    /// <summary>
    /// Generate Anonymized DNSCrypt Relay Stamp
    /// </summary>
    /// <param name="ipPort">IP address and port, as a string. IPv6 strings must be included in square brackets.</param>
    /// <returns>Returns stamp or string.Empty if fail</returns>
    public static string GenerateAnonymizedDNSCryptRelay(string ipPort)
    {
        ipPort = ipPort.Trim();
        string sdns = string.Empty;

        try
        {
            byte[] bDns = new byte[] { 0x81 }; // Anonymized DNSCrypt Relay
            bool bIpPortBool = TryGet_LP(ipPort, out byte[] bIpPort);
            if (!bIpPortBool) return sdns;

            byte[] main = bDns.Concat(bIpPort).ToArray();
            sdns = GetSdnsUrl(main);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNSCryptStampGenerator GenerateAnonymizedDNSCryptRelay: " + ex.Message);
        }

        return sdns;
    }

    /// <summary>
    /// Generate Oblivious DoH Relay Stamp
    /// </summary>
    /// <param name="ip">IP Address (IPv6 addresses must be in brackets)</param>
    /// <param name="hashes">Hashes (comma-separated) - Optional</param>
    /// <param name="hostPort">Host name (vHost+SNI) and optional port number</param>
    /// <param name="path">Path</param>
    /// <param name="bootstraps">Bootstraps (comma-separated) - Optional</param>
    /// <param name="isDNSSec">Is DNSSec</param>
    /// <param name="isNoLog">Is no log</param>
    /// <param name="isNoFilter">Is no filter</param>
    /// <returns>Returns stamp or string.Empty if fail</returns>
    public static string GenerateObliviousDohRelay(string ip, string? hashes, string hostPort, string path, string? bootstraps, bool isDNSSec, bool isNoLog, bool isNoFilter)
    {
        ip = ip.Trim();
        if (!string.IsNullOrEmpty(hashes)) hashes = hashes.Trim();
        hostPort = hostPort.Trim();
        path = string.IsNullOrEmpty(path) ? "/" : path.Trim();
        if (!string.IsNullOrEmpty(bootstraps)) bootstraps = bootstraps.Trim();
        string sdns = string.Empty;

        try
        {
            byte[] bDns = new byte[] { 0x85 }; // Oblivious DoH Relay
            bool bPropsBool = TryGet_Properties(isDNSSec, isNoLog, isNoFilter, out byte[] bProps);
            if (!bPropsBool) return sdns;
            bool bDnsIpBool = TryGet_LP(ip, out byte[] bDnsIp);
            if (!bDnsIpBool) return sdns;
            bool bHashBool = TryGet_VLPHash(hashes, out byte[] bHash);
            if (!bHashBool) return sdns;
            bool bhostPortBool = TryGet_LP(hostPort, out byte[] bhostPort);
            if (!bhostPortBool) return sdns;
            bool bPathBool = TryGet_LP(path, out byte[] bPath);
            if (!bPathBool) return sdns;
            bool bBootstrapBool = TryGet_VLPBootstrap(bootstraps, out byte[] bBootstrap);
            if (!bBootstrapBool) return sdns;

            byte[] main = bDns.Concat(bProps).Concat(bDnsIp).Concat(bHash).Concat(bhostPort).Concat(bPath).ToArray();
            if (!string.IsNullOrEmpty(bootstraps))
                main = main.Concat(bBootstrap).ToArray();

            sdns = GetSdnsUrl(main);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNSCryptStampGenerator GenerateObliviousDohRelay: " + ex.Message);
        }

        return sdns;
    }

    private static bool TryGet_Properties(bool isDNSSec, bool isNoLog, bool isNoFilter, out byte[] bProps)
    {
        bProps = Array.Empty<byte>();

        try
        {
            // 1: the server supports DNSSEC
            // 2: the server doesn't keep logs
            // 4: the server doesn't intentionally block domains
            int p = 0;
            if (isDNSSec) p += 1;
            if (isNoLog) p += 2;
            if (isNoFilter) p += 4;

            bProps = new byte[] { Convert.ToByte(p), 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNSCryptStampGenerator TryGet_Properties: " + ex.Message);
            return false;
        }
    }

    private static bool TryGet_LP(string input, out byte[] bLP)
    {
        bLP = Array.Empty<byte>();

        try
        {
            input = input.Trim();
            byte[] bInputLength = new byte[] { Convert.ToByte(input.Length) };
            byte[] bInput = Encoding.UTF8.GetBytes(input);
            bLP = bInputLength.Concat(bInput).ToArray();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNSCryptStampGenerator TryGet_LP: " + ex.Message);
            return false;
        }
    }

    private static bool TryGet_LPPublicKey(string input, out byte[] bLPPublicKey)
    {
        bLPPublicKey = Array.Empty<byte>();

        try
        {
            input = input.ToLower().Trim();
            byte[] bInput = Convert.FromHexString(input);
            byte[] bInputLength = new byte[] { Convert.ToByte(bInput.Length) };
            bLPPublicKey = bInputLength.Concat(bInput).ToArray();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNSCryptStampGenerator TryGet_LPPublicKey: " + ex.Message);
            return false;
        }
    }

    private static bool TryGet_VLPHash(string? input, out byte[] bVLPHash)
    {
        bVLPHash = Array.Empty<byte>();

        try
        {
            if (string.IsNullOrEmpty(input))
            {
                bVLPHash = new byte[] { 0x00 };
                return true;
            }

            input = input.Replace(" ", string.Empty).ToLower();
            if (input.Contains(','))
            {
                string[] split = input.Split(',', StringSplitOptions.TrimEntries);
                for (int n = 0; n < split.Length; n++)
                {
                    string oneInput = split[n].Trim();
                    if (n == split.Length - 1) // Last Line
                    {
                        byte[] bInput = Convert.FromHexString(oneInput);
                        byte[] bInputLength = new byte[] { Convert.ToByte(bInput.Length) };
                        bVLPHash = bVLPHash.Concat(bInputLength).Concat(bInput).ToArray();
                    }
                    else
                    {
                        byte[] bInput = Convert.FromHexString(oneInput);
                        int length = 0x80 | bInput.Length;
                        byte[] bInputLength = new byte[] { Convert.ToByte(length) };
                        bVLPHash = bVLPHash.Concat(bInputLength).Concat(bInput).ToArray();
                    }
                }
            }
            else
            {
                byte[] bInput = Convert.FromHexString(input);
                byte[] bInputLength = new byte[] { Convert.ToByte(bInput.Length) };
                bVLPHash = bVLPHash.Concat(bInputLength).Concat(bInput).ToArray();
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNSCryptStampGenerator TryGet_VLPHash: " + ex.Message);
            return false;
        }
    }

    private static bool TryGet_VLPBootstrap(string? input, out byte[] bVLPBootstrap)
    {
        bVLPBootstrap = Array.Empty<byte>();

        try
        {
            if (string.IsNullOrEmpty(input))
            {
                bVLPBootstrap = new byte[] { 0x00 };
                return true;
            }

            input = input.Replace(" ", string.Empty);
            if (input.Contains(','))
            {
                string[] split = input.Split(',', StringSplitOptions.TrimEntries);
                for (int n = 0; n < split.Length; n++)
                {
                    string oneInput = split[n].Trim();
                    if (n == split.Length - 1) // Last Line
                    {
                        byte[] bInputLength = new byte[] { Convert.ToByte(oneInput.Length) };
                        byte[] bInput = Encoding.UTF8.GetBytes(oneInput);
                        bVLPBootstrap = bVLPBootstrap.Concat(bInputLength).Concat(bInput).ToArray();
                    }
                    else
                    {
                        int length = 0x80 | oneInput.Length;
                        byte[] bInputLength = new byte[] { Convert.ToByte(length) };
                        byte[] bInput = Encoding.UTF8.GetBytes(oneInput);
                        bVLPBootstrap = bVLPBootstrap.Concat(bInputLength).Concat(bInput).ToArray();
                    }
                }
            }
            else
            {
                byte[] bInputLength = new byte[] { Convert.ToByte(input.Length) };
                byte[] bInput = Encoding.UTF8.GetBytes(input);
                bVLPBootstrap = bVLPBootstrap.Concat(bInputLength).Concat(bInput).ToArray();

            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNSCryptStampGenerator TryGet_VLPBootstrap: " + ex.Message);
            return false;
        }
    }

    private static string GetSdnsUrl(byte[] wholeBytes)
    {
        string sdnsScheme = "sdns://";
        string mainBase64 = EncodingTool.UrlEncode(wholeBytes);
        return sdnsScheme + mainBase64;
    }

}