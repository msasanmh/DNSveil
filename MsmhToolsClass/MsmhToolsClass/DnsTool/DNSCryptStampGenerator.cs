using System;
using System.Diagnostics;
using System.Text;

namespace MsmhToolsClass.DnsTool
{
    public class DNSCryptStampGenerator
    {
        // More info: https://dnscrypt.info/stamps-specifications/
        public DNSCryptStampGenerator()
        {

        }

        /// <summary>
        /// Generate Plain DNS Stamp
        /// </summary>
        /// <param name="ip">IP Address (IPv6 addresses must be in brackets)</param>
        /// <param name="isDNSSec">Is DNSSec</param>
        /// <param name="isNoLog">Is no log</param>
        /// <param name="isNoFilter">Is no filter</param>
        /// <returns>Returns stamp or string.Empty if fail</returns>
        public string GeneratePlainDns(string ipPort, bool isDNSSec, bool isNoLog, bool isNoFilter)
        {
            ipPort = ipPort.Trim();
            string sdns = string.Empty;

            try
            {
                byte[] bDns = new byte[] { 0x00 }; // Plain DNS
                byte[] bProps = GetProperties(isDNSSec, isNoLog, isNoFilter);
                byte[] bDnsIp = LP(ipPort);

                byte[] main = bDns.Concat(bProps).Concat(bDnsIp).ToArray();
                sdns = GetSdnsUrl(main);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Convert Plain DNS to Stamp: " + ex.Message);
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
        public string GenerateDNSCrypt(string ipPort, string publicKey, string providerName, bool isDNSSec, bool isNoLog, bool isNoFilter)
        {
            ipPort = ipPort.Trim();
            publicKey = publicKey.Trim();
            providerName = providerName.Trim();
            string sdns = string.Empty;

            try
            {
                byte[] bDns = new byte[] { 0x01 }; // DNSCrypt
                byte[] bProps = GetProperties(isDNSSec, isNoLog, isNoFilter);
                byte[] bIpPort = LP(ipPort);
                byte[] bPK = LPPublicKey(publicKey);
                byte[] bPN = LP(providerName);

                byte[] main = bDns.Concat(bProps).Concat(bIpPort).Concat(bPK).Concat(bPN).ToArray();
                sdns = GetSdnsUrl(main);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Convert DNSCrypt to Stamp: " + ex.Message);
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
        public string GenerateDoH(string ip, string? hashes, string hostPort, string path, string? bootstraps, bool isDNSSec, bool isNoLog, bool isNoFilter)
        {
            ip = ip.Trim();
            if (!string.IsNullOrEmpty(hashes))
                hashes = hashes.Trim();
            hostPort = hostPort.Trim();
            path = string.IsNullOrEmpty(path) ? "/" : path.Trim();
            if (!string.IsNullOrEmpty(bootstraps))
                bootstraps = bootstraps.Trim();
            string sdns = string.Empty;

            try
            {
                byte[] bDoh = new byte[] { 0x02 }; // DoH
                byte[] bProps = GetProperties(isDNSSec, isNoLog, isNoFilter);
                byte[] bDohIp = LP(ip);
                byte[] bHash = VLPHash(hashes);
                byte[] bhostPort = LP(hostPort);
                byte[] bPath = LP(path);
                byte[] bBootstrap = VLPBootstrap(bootstraps);

                byte[] main = bDoh.Concat(bProps).Concat(bDohIp).Concat(bHash).Concat(bhostPort).Concat(bPath).ToArray();
                if (!string.IsNullOrEmpty(bootstraps))
                    main = main.Concat(bBootstrap).ToArray();
                
                sdns = GetSdnsUrl(main);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Convert DoH to Stamp: " + ex.Message);
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
        public string GenerateDoT(string ip, string? hashes, string hostPort, string? bootstraps, bool isDNSSec, bool isNoLog, bool isNoFilter)
        {
            ip = ip.Trim();
            if (!string.IsNullOrEmpty(hashes))
                hashes = hashes.Trim();
            hostPort = hostPort.Trim();
            if (!string.IsNullOrEmpty(bootstraps))
                bootstraps = bootstraps.Trim();
            string sdns = string.Empty;

            try
            {
                byte[] bDot = new byte[] { 0x03 }; // DoT
                byte[] bProps = GetProperties(isDNSSec, isNoLog, isNoFilter);
                byte[] bDotIp = LP(ip);
                byte[] bHash = VLPHash(hashes);
                byte[] bhostPort = LP(hostPort);
                byte[] bBootstrap = VLPBootstrap(bootstraps);

                byte[] main = bDot.Concat(bProps).Concat(bDotIp).Concat(bHash).Concat(bhostPort).ToArray();
                if (!string.IsNullOrEmpty(bootstraps))
                    main = main.Concat(bBootstrap).ToArray();

                sdns = GetSdnsUrl(main);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Convert DoT to Stamp: " + ex.Message);
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
        public string GenerateDoQ(string ip, string? hashes, string hostPort, string? bootstraps, bool isDNSSec, bool isNoLog, bool isNoFilter)
        {
            ip = ip.Trim();
            if (!string.IsNullOrEmpty(hashes))
                hashes = hashes.Trim();
            hostPort = hostPort.Trim();
            if (!string.IsNullOrEmpty(bootstraps))
                bootstraps = bootstraps.Trim();
            string sdns = string.Empty;

            try
            {
                byte[] bDoq = new byte[] { 0x04 }; // DoQ
                byte[] bProps = GetProperties(isDNSSec, isNoLog, isNoFilter);
                byte[] bDoqIp = LP(ip);
                byte[] bHash = VLPHash(hashes);
                byte[] bhostPort = LP(hostPort);
                byte[] bBootstrap = VLPBootstrap(bootstraps);

                byte[] main = bDoq.Concat(bProps).Concat(bDoqIp).Concat(bHash).Concat(bhostPort).ToArray();
                if (!string.IsNullOrEmpty(bootstraps))
                    main = main.Concat(bBootstrap).ToArray();

                sdns = GetSdnsUrl(main);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Convert DoQ to Stamp: " + ex.Message);
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
        public string GenerateObliviousDohTarget(string hostPort, string path, bool isDNSSec, bool isNoLog, bool isNoFilter)
        {
            hostPort = hostPort.Trim();
            path = string.IsNullOrEmpty(path) ? "/" : path.Trim();
            string sdns = string.Empty;

            try
            {
                byte[] bDns = new byte[] { 0x05 }; // Oblivious DoH Target
                byte[] bProps = GetProperties(isDNSSec, isNoLog, isNoFilter);
                byte[] bhostPort = LP(hostPort);
                byte[] bPath = LP(path);

                byte[] main = bDns.Concat(bProps).Concat(bhostPort).Concat(bPath).ToArray();
                sdns = GetSdnsUrl(main);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Convert Oblivious DoH Target to Stamp: " + ex.Message);
            }

            return sdns;
        }

        /// <summary>
        /// Generate Anonymized DNSCrypt Relay Stamp
        /// </summary>
        /// <param name="ipPort">IP address and port, as a string. IPv6 strings must be included in square brackets.</param>
        /// <returns>Returns stamp or string.Empty if fail</returns>
        public string GenerateAnonymizedDNSCryptRelay(string ipPort)
        {
            ipPort = ipPort.Trim();
            string sdns = string.Empty;

            try
            {
                byte[] bDns = new byte[] { 0x81 }; // Anonymized DNSCrypt Relay
                byte[] bIpPort = LP(ipPort);

                byte[] main = bDns.Concat(bIpPort).ToArray();
                sdns = GetSdnsUrl(main);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Convert Anonymized DNSCrypt Relay to Stamp: " + ex.Message);
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
        public string GenerateObliviousDohRelay(string ip, string? hashes, string hostPort, string path, string? bootstraps, bool isDNSSec, bool isNoLog, bool isNoFilter)
        {
            ip = ip.Trim();
            if (!string.IsNullOrEmpty(hashes))
                hashes = hashes.Trim();
            hostPort = hostPort.Trim();
            path = string.IsNullOrEmpty(path) ? "/" : path.Trim();
            if (!string.IsNullOrEmpty(bootstraps))
                bootstraps = bootstraps.Trim();
            string sdns = string.Empty;

            try
            {
                byte[] bDns = new byte[] { 0x85 }; // Oblivious DoH Relay
                byte[] bProps = GetProperties(isDNSSec, isNoLog, isNoFilter);
                byte[] bDnsIp = LP(ip);
                byte[] bHash = VLPHash(hashes);
                byte[] bhostPort = LP(hostPort);
                byte[] bPath = LP(path);
                byte[] bBootstrap = VLPBootstrap(bootstraps);

                byte[] main = bDns.Concat(bProps).Concat(bDnsIp).Concat(bHash).Concat(bhostPort).Concat(bPath).ToArray();
                if (!string.IsNullOrEmpty(bootstraps))
                    main = main.Concat(bBootstrap).ToArray();

                sdns = GetSdnsUrl(main);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Convert Oblivious DoH Relay to Stamp: " + ex.Message);
            }

            return sdns;
        }

        private static byte[] GetProperties(bool isDNSSec, bool isNoLog, bool isNoFilter)
        {
            // 1: the server supports DNSSEC
            // 2: the server doesn't keep logs
            // 4: the server doesn't intentionally block domains
            int p = 0;
            if (isDNSSec) p += 1;
            if (isNoLog) p += 2;
            if (isNoFilter) p += 4;

            byte[] bProps = new byte[] { Convert.ToByte(p), 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            return bProps;
        }

        private static byte[] LP(string input)
        {
            input = input.Trim();
            byte[] bInputLength = new byte[] { Convert.ToByte(input.Length) };
            byte[] bInput = Encoding.UTF8.GetBytes(input);
            return bInputLength.Concat(bInput).ToArray();
        }

        private static byte[] LPPublicKey(string input)
        {
            input = input.ToLower().Trim();
            byte[] bInput = Convert.FromHexString(input);
            byte[] bInputLength = new byte[] { Convert.ToByte(bInput.Length) };
            return bInputLength.Concat(bInput).ToArray();
        }

        private static byte[] VLPHash(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return new byte[] { 0x00 };

            input = input.Replace(" ", string.Empty).ToLower();

            byte[] bInputOut = Array.Empty<byte>();
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
                        bInputOut = bInputOut.Concat(bInputLength).Concat(bInput).ToArray();
                    }
                    else
                    {
                        byte[] bInput = Convert.FromHexString(oneInput);
                        int length = 0x80 | bInput.Length;
                        byte[] bInputLength = new byte[] { Convert.ToByte(length) };
                        bInputOut = bInputOut.Concat(bInputLength).Concat(bInput).ToArray();
                    }
                }
            }
            else
            {
                byte[] bInput = Convert.FromHexString(input);
                byte[] bInputLength = new byte[] { Convert.ToByte(bInput.Length) };
                bInputOut = bInputOut.Concat(bInputLength).Concat(bInput).ToArray();

            }
            return bInputOut;
        }

        private static byte[] VLPBootstrap(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return new byte[] { 0x00 };

            input = input.Replace(" ", string.Empty);

            byte[] bInputOut = Array.Empty<byte>();
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
                        bInputOut = bInputOut.Concat(bInputLength).Concat(bInput).ToArray();
                    }
                    else
                    {
                        int length = 0x80 | oneInput.Length;
                        byte[] bInputLength = new byte[] { Convert.ToByte(length) };
                        byte[] bInput = Encoding.UTF8.GetBytes(oneInput);
                        bInputOut = bInputOut.Concat(bInputLength).Concat(bInput).ToArray();
                    }
                }
            }
            else
            {
                byte[] bInputLength = new byte[] { Convert.ToByte(input.Length) };
                byte[] bInput = Encoding.UTF8.GetBytes(input);
                bInputOut = bInputOut.Concat(bInputLength).Concat(bInput).ToArray();

            }
            return bInputOut;
        }

        private static string GetSdnsUrl(byte[] wholeBytes)
        {
            string sdnsScheme = "sdns://";
            string mainBase64 = EncodingTool.UrlEncode(wholeBytes);
            return sdnsScheme + mainBase64;
        }

    }
}
