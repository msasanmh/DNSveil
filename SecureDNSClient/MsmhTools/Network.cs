using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MsmhTools
{
    public static class Network
    {
        public static string? UrlToHost(string url)
        {
            string? host = null;
            try
            {
                string[] split1 = url.Split("//");
                string prefix = "https://";
                for (int n1 = 0; n1 < split1.Length; n1++)
                {
                    if (n1 > 0)
                    {
                        prefix += split1[n1];
                        if (n1 < split1.Length - 1)
                            prefix += "//";
                    }
                }

                Uri uri = new(prefix);
                host = uri.Host;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return host;
        }

        public static IPAddress? HostToIP(string host, bool getIPv6 = false)
        {
            IPAddress? result = null;

            try
            {
                //IPAddress[] ipAddresses = Dns.GetHostEntry(host).AddressList;
                IPAddress[] ipAddresses = Dns.GetHostAddresses(host);

                if (ipAddresses == null || ipAddresses.Length == 0)
                    return null;

                if (!getIPv6)
                {
                    for (int n = 0; n < ipAddresses.Length; n++)
                    {
                        var addressFamily = ipAddresses[n].AddressFamily;
                        if (addressFamily == AddressFamily.InterNetwork)
                        {
                            result = ipAddresses[n];
                            break;
                        }
                    }
                }
                else
                {
                    for (int n = 0; n < ipAddresses.Length; n++)
                    {
                        var addressFamily = ipAddresses[n].AddressFamily;
                        if (addressFamily == AddressFamily.InterNetworkV6)
                        {
                            result = ipAddresses[n];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return result;
        }

        public static List<IPAddress>? HostToIPs(string host, bool getIPv6 = false)
        {
            List<IPAddress>? result = new();

            try
            {
                //IPAddress[] ipAddresses = Dns.GetHostEntry(host).AddressList;
                IPAddress[] ipAddresses = Dns.GetHostAddresses(host);

                if (ipAddresses == null || ipAddresses.Length == 0)
                    return null;

                if (!getIPv6)
                {
                    for (int n = 0; n < ipAddresses.Length; n++)
                    {
                        var addressFamily = ipAddresses[n].AddressFamily;
                        if (addressFamily == AddressFamily.InterNetwork)
                        {
                            result.Add(ipAddresses[n]);
                        }
                    }
                }
                else
                {
                    for (int n = 0; n < ipAddresses.Length; n++)
                    {
                        var addressFamily = ipAddresses[n].AddressFamily;
                        if (addressFamily == AddressFamily.InterNetworkV6)
                        {
                            result.Add(ipAddresses[n]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return result.RemoveDuplicates();
        }

        public static async Task<string?> IpToCompanyAsync(IPAddress iPAddress, string? proxyScheme = null)
        {
            string? company = null;
            try
            {
                using SocketsHttpHandler socketsHttpHandler = new();
                if (proxyScheme != null)
                    socketsHttpHandler.Proxy = new WebProxy(proxyScheme, true);
                using HttpClient httpClient2 = new(socketsHttpHandler);
                company = await httpClient2.GetStringAsync("https://ipinfo.io/" + iPAddress.ToString() + "/org");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return company;
        }

        public static void GenerateCertificate(string folderPath, IPAddress gateway, string issuerSubjectName = "CN=MSasanMH Authority", string subjectName = "CN=MSasanMH")
        {
            const string CRT_HEADER = "-----BEGIN CERTIFICATE-----\n";
            const string CRT_FOOTER = "\n-----END CERTIFICATE-----";

            const string KEY_HEADER = "-----BEGIN RSA PRIVATE KEY-----\n";
            const string KEY_FOOTER = "\n-----END RSA PRIVATE KEY-----";

            // Create X509KeyUsageFlags
            const X509KeyUsageFlags x509KeyUsageFlags = X509KeyUsageFlags.CrlSign |
                                                        X509KeyUsageFlags.DataEncipherment |
                                                        X509KeyUsageFlags.DigitalSignature |
                                                        X509KeyUsageFlags.KeyAgreement |
                                                        X509KeyUsageFlags.KeyCertSign |
                                                        X509KeyUsageFlags.KeyEncipherment |
                                                        X509KeyUsageFlags.NonRepudiation;

            // Create SubjectAlternativeNameBuilder
            SubjectAlternativeNameBuilder sanBuilder = new();
            sanBuilder.AddDnsName("localhost");
            sanBuilder.AddIpAddress(IPAddress.Parse("127.0.0.1"));
            sanBuilder.AddIpAddress(IPAddress.Parse("0.0.0.0"));
            sanBuilder.AddIpAddress(IPAddress.Loopback);
            sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);

            // Generate IP range for gateway
            if (IsIPv4(gateway))
            {
                string ipString = gateway.ToString();
                string[] ipSplit = ipString.Split('.');
                string ip1 = ipSplit[0] + "." + ipSplit[1] + "." + ipSplit[2] + ".";
                for (int n = 0; n <= 255; n++)
                {
                    string ip2 = ip1 + n.ToString();
                    sanBuilder.AddIpAddress(IPAddress.Parse(ip2));
                }
                // Generate local IP range in case a VPN is active.
                if (!ip1.Equals("192.168.1."))
                {
                    string ipLocal1 = "192.168.1.";
                    for (int n = 0; n <= 255; n++)
                    {
                        string ipLocal2 = ipLocal1 + n.ToString();
                        sanBuilder.AddIpAddress(IPAddress.Parse(ipLocal2));
                    }
                }
            }

            sanBuilder.AddUri(new Uri("https://127.0.0.1"));
            sanBuilder.Build();

            // Create Oid Collection
            OidCollection oidCollection = new();
            oidCollection.Add(new Oid("2.5.29.37.0")); // Any Purpose
            oidCollection.Add(new Oid("1.3.6.1.5.5.7.3.1")); // Server Authentication
            oidCollection.Add(new Oid("1.3.6.1.5.5.7.3.2")); // Client Authentication
            oidCollection.Add(new Oid("1.3.6.1.5.5.7.3.3")); // Code Signing
            oidCollection.Add(new Oid("1.3.6.1.5.5.7.3.4")); // Email Protection
            oidCollection.Add(new Oid("1.3.6.1.5.5.7.3.5")); // IPSEC End System Certificate
            oidCollection.Add(new Oid("1.3.6.1.5.5.7.3.6")); // IPSEC Tunnel
            oidCollection.Add(new Oid("1.3.6.1.5.5.7.3.7")); // IPSEC User Certificate
            oidCollection.Add(new Oid("1.3.6.1.5.5.7.3.8")); // Time Stamping
            oidCollection.Add(new Oid("1.3.6.1.4.1.311.10.3.2")); // Microsoft Time Stamping
            oidCollection.Add(new Oid("1.3.6.1.4.1.311.10.5.1")); // Digital Rights
            oidCollection.Add(new Oid("1.3.6.1.4.1.311.64.1.1")); // Domain Name System (DNS) Server Trust

            // Create Issuer RSA Private Key
            using RSA issuerRsaKey = RSA.Create(4096);

            // Create Issuer Request
            CertificateRequest issuerReq = new(issuerSubjectName, issuerRsaKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            issuerReq.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
            issuerReq.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(issuerReq.PublicKey, false));
            issuerReq.CertificateExtensions.Add(new X509KeyUsageExtension(x509KeyUsageFlags, false));
            issuerReq.CertificateExtensions.Add(sanBuilder.Build());
            issuerReq.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(oidCollection, true));

            // Create Issuer Certificate
            using X509Certificate2 issuerCert = issuerReq.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(10));

            // Create RSA Private Key
            using RSA rsaKey = RSA.Create(2048);

            // Create Request
            CertificateRequest req = new(subjectName, rsaKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            req.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
            req.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(req.PublicKey, false));
            req.CertificateExtensions.Add(new X509KeyUsageExtension(x509KeyUsageFlags, false));
            req.CertificateExtensions.Add(sanBuilder.Build());
            req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(oidCollection, true));

            // Create Certificate
            using X509Certificate2 cert = req.Create(issuerCert, DateTimeOffset.Now, DateTimeOffset.Now.AddYears(9), new byte[] { 1, 2, 3, 4 });

            // Export
            // Export Issuer Private Key
            var issuerPrivateKeyExport = issuerRsaKey.ExportRSAPrivateKey();
            var issuerPrivateKeyData = Convert.ToBase64String(issuerPrivateKeyExport, Base64FormattingOptions.InsertLineBreaks);
            File.WriteAllText(Path.Combine(folderPath, "rootCA.key"), KEY_HEADER + issuerPrivateKeyData + KEY_FOOTER);

            // Export Issuer Certificate
            var issuerCertExport = issuerCert.Export(X509ContentType.Cert);
            var issuerCertData = Convert.ToBase64String(issuerCertExport, Base64FormattingOptions.InsertLineBreaks);
            File.WriteAllText(Path.Combine(folderPath, "rootCA.crt"), CRT_HEADER + issuerCertData + CRT_FOOTER);

            // Export Private Key
            var privateKeyExport = rsaKey.ExportRSAPrivateKey();
            var privateKeyData = Convert.ToBase64String(privateKeyExport, Base64FormattingOptions.InsertLineBreaks);
            File.WriteAllText(Path.Combine(folderPath, "localhost.key"), KEY_HEADER + privateKeyData + KEY_FOOTER);

            // Export Certificate
            var certExport = cert.Export(X509ContentType.Cert);
            var certData = Convert.ToBase64String(certExport, Base64FormattingOptions.InsertLineBreaks);
            File.WriteAllText(Path.Combine(folderPath, "localhost.crt"), CRT_HEADER + certData + CRT_FOOTER);

        }

        public static void CreateP12(string certPath, string keyPath, string password = "")
        {
            string? folderPath = Path.GetDirectoryName(certPath);
            string fileName = Path.GetFileNameWithoutExtension(certPath);
            using X509Certificate2 certWithKey = X509Certificate2.CreateFromPemFile(certPath, keyPath);
            var certWithKeyExport = certWithKey.Export(X509ContentType.Pfx, password);
            if (!string.IsNullOrEmpty(folderPath))
                File.WriteAllBytes(Path.Combine(folderPath, fileName + ".p12"), certWithKeyExport);
        }

        /// <summary>
        /// Returns false if user don't install certificate, otherwise true.
        /// </summary>
        public static bool InstallCertificate(string certPath, StoreName storeName, StoreLocation storeLocation)
        {
            try
            {
                X509Certificate2 certificate = new(certPath, "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
                X509Store store = new(storeName, storeLocation);
                store.Open(OpenFlags.ReadWrite);
                store.Add(certificate);
                store.Close();
                return true;
            }
            catch (Exception ex) // Internal.Cryptography.CryptoThrowHelper.WindowsCryptographicException
            {
                Debug.WriteLine(ex.Message);
                // If ex.Message: (The operation was canceled by the user.)
                return false;
            }
        }

        public static bool IsCertificateInstalled(string subjectName, StoreName storeName, StoreLocation storeLocation)
        {
            X509Store store = new(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);

            var certificates = store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false);

            if (certificates != null && certificates.Count > 0)
            {
                Debug.WriteLine("Certificate is already installed.");
                return true;
            }
            else
                return false;
        }

        public static IPAddress? GetLocalIPv4(string remoteHostToCheck = "8.8.8.8")
        {
            try
            {
                IPAddress? localIP;
                using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                socket.Connect(remoteHostToCheck, 80);
                IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint?.Address;
                return localIP;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public static IPAddress? GetLocalIPv6(string remoteHostToCheck = "8.8.8.8")
        {
            try
            {
                IPAddress? localIP;
                using Socket socket = new(AddressFamily.InterNetworkV6, SocketType.Dgram, 0);
                socket.Connect(remoteHostToCheck, 80);
                IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint?.Address;
                return localIP;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public static IPAddress? GetDefaultGateway()
        {
            IPAddress? gateway = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n?.GetIPProperties()?.GatewayAddresses)
                .Select(g => g?.Address)
                .Where(a => a != null)
                .Where(a => a?.AddressFamily == AddressFamily.InterNetwork)
                //.Where(a => Array.FindIndex(a.GetAddressBytes(), b => b != 0) >= 0) // Filter out 0.0.0.0
                .FirstOrDefault();
            return gateway;
        }

        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto)]
        private static extern int GetBestInterface(uint destAddr, out uint bestIfIndex);
        public static IPAddress? GetGatewayForDestination(IPAddress destinationAddress)
        {
            uint destaddr = BitConverter.ToUInt32(destinationAddress.GetAddressBytes(), 0);

            int result = GetBestInterface(destaddr, out uint interfaceIndex);
            if (result != 0)
            {
                Debug.WriteLine(new Win32Exception(result));
                return null;
            }

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                var niprops = ni.GetIPProperties();
                if (niprops == null)
                    continue;

                var gateway = niprops.GatewayAddresses?.FirstOrDefault()?.Address;
                if (gateway == null)
                    continue;

                if (ni.Supports(NetworkInterfaceComponent.IPv4))
                {
                    var v4props = niprops.GetIPv4Properties();
                    if (v4props == null)
                        continue;

                    if (v4props.Index == interfaceIndex)
                        return gateway;
                }

                if (ni.Supports(NetworkInterfaceComponent.IPv6))
                {
                    var v6props = niprops.GetIPv6Properties();
                    if (v6props == null)
                        continue;

                    if (v6props.Index == interfaceIndex)
                        return gateway;
                }
            }

            return null;
        }

        public static bool IsIPv4(IPAddress iPAddress)
        {
            if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
                return true;
            else
                return false;
        }

        public static bool IsIPv4Valid(string ipString, out IPAddress? iPAddress)
        {
            iPAddress = null;
            if (string.IsNullOrWhiteSpace(ipString)) return false;
            if (!ipString.Contains('.')) return false;
            if (ipString.Count(c => c == '.') != 3) return false;
            if (ipString.StartsWith('.')) return false;
            if (ipString.EndsWith('.')) return false;
            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4) return false;

            foreach (string splitValue in splitValues)
            {
                // 0x and 0xx are not valid
                if (splitValue.Length > 1)
                {
                    bool isInt1 = int.TryParse(splitValue.AsSpan(0, 1), out int first);
                    if (isInt1 && first == 0) return false;
                }

                bool isInt2 = int.TryParse(splitValue, out int testInt);
                if (!isInt2) return false;
                if (testInt < 0 || testInt > 255) return false;
            }

            bool isIP = IPAddress.TryParse(ipString, out IPAddress? outIP);
            if (!isIP) return false;
            iPAddress = outIP;
            return true;
        }

        public static bool IsIPv6(IPAddress iPAddress)
        {
            if (iPAddress.AddressFamily == AddressFamily.InterNetworkV6)
                return true;
            else
                return false;
        }

        public static bool IsPortOpen(string host, int port, double timeoutSeconds)
        {
            try
            {
                using TcpClient client = new();
                var result = client.BeginConnect(host, port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(timeoutSeconds));
                client.EndConnect(result);
                return success;
            }
            catch
            {
                return false;
            }
        }

        public static List<NetworkInterface> GetNetworkInterfacesIPv4(bool upAndRunning = true)
        {
            List<NetworkInterface> nicList = new();
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            for (int n1 = 0; n1 < networkInterfaces.Length; n1++)
            {
                NetworkInterface nic = networkInterfaces[n1];
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    var unicastAddresses = nic.GetIPProperties().UnicastAddresses;
                    for (int n2 = 0; n2 < unicastAddresses.Count; n2++)
                    {
                        var unicastAddress = unicastAddresses[n2];
                        if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            if (upAndRunning)
                            {
                                if (nic.OperationalStatus == OperationalStatus.Up)
                                {
                                    nicList.Add(nic);
                                    break;
                                }
                            }
                            else
                            {
                                nicList.Add(nic);
                                break;
                            }
                        }
                    }
                }
            }
            return nicList;
        }

        public static NetworkInterface? GetNICByName(string name)
        {
            NetworkInterface? nic = null;
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            for (int n = 0; n < networkInterfaces.Length; n++)
            {
                nic = networkInterfaces[n];
                if (nic.Name.Equals(name))
                    return nic;
            }
            return nic;
        }

        public static NetworkInterface? GetNICByDescription(string description)
        {
            NetworkInterface? nic = null;
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            for (int n = 0; n < networkInterfaces.Length; n++)
            {
                nic = networkInterfaces[n];
                if (nic.Description.Equals(description))
                    return nic;
            }
            return nic;
        }

        /// <summary>
        /// Set's the DNS Server of the local machine
        /// </summary>
        /// <param name="nic">NIC address</param>
        /// <param name="dnsServers">Comma seperated list of DNS server addresses</param>
        /// <remarks>Requires a reference to the System.Management namespace</remarks>
        public static void SetDNS(NetworkInterface nic, string dnsServers)
        {
            // Requires Elevation
            if (nic == null) return;
            try
            {
                using ManagementClass managementClass = new("Win32_NetworkAdapterConfiguration");
                using ManagementObjectCollection moc = managementClass.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] && mo["Description"].Equals(nic.Description))
                    {
                        using ManagementBaseObject newDNS = mo.GetMethodParameters("SetDNSServerSearchOrder");
                        if (newDNS != null)
                        {
                            newDNS["DNSServerSearchOrder"] = dnsServers.Split(',');
                            mo.InvokeMethod("SetDNSServerSearchOrder", newDNS, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public static void UnsetDNS(NetworkInterface nic)
        {
            // Requires Elevation - Can't Unset DNS when there is no Internet connectivity but netsh can :)
            // NetSh Command: netsh interface ip set dns "nic.Name" source=dhcp
            if (nic == null) return;

            string processName = "netsh";
            string processArgs = "interface ip set dns \"" + nic.Name + "\" source=dhcp";
            ProcessManager.ExecuteOnly(processName, processArgs, true, true);

            try
            {
                using ManagementClass managementClass = new("Win32_NetworkAdapterConfiguration");
                using ManagementObjectCollection moc = managementClass.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if (mo["Description"].Equals(nic.Description))
                    {
                        using ManagementBaseObject newDNS = mo.GetMethodParameters("SetDNSServerSearchOrder");
                        if (newDNS != null)
                        {
                            newDNS["DNSServerSearchOrder"] = null;
                            mo.InvokeMethod("SetDNSServerSearchOrder", newDNS, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public static bool IsInternetAlive(string? url = null, int timeoutMs = 5000)
        {
            // Attempt 1
            // only recognizes changes related to Internet adapters
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // however, this will include all adapters -- filter by opstatus and activity
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                bool attempt1 = (from face in interfaces
                        where face.OperationalStatus == OperationalStatus.Up
                        where (face.NetworkInterfaceType != NetworkInterfaceType.Tunnel) && (face.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                        select face.GetIPv4Statistics()).Any(statistics => (statistics.BytesReceived > 0) && (statistics.BytesSent > 0));

                return attempt1 ? true : attempt2(url, timeoutMs);
            }
            else
            {
                return attempt2(url, timeoutMs);
            }

            // Attempt 2
            static bool attempt2(string? url = null, int timeoutMs = 5000)
            {
                try
                {
                    url ??= CultureInfo.InstalledUICulture switch
                    {
                        { Name: var n } when n.StartsWith("fa") => // Iran
                            "http://www.google.com",
                        { Name: var n } when n.StartsWith("zh") => // China
                            "http://www.baidu.com",
                        _ =>
                            "http://www.gstatic.com/generate_204",
                    };
                    
                    using HttpClient httpClient = new();
                    httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMs);
                    var req = httpClient.GetAsync(url);
                    return req.Result.IsSuccessStatusCode;
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return false;
                }
            }
        }

        public static bool IsInternetAlive2(string? url = null, int timeoutMs = 5000)
        {
            try
            {
                url ??= CultureInfo.InstalledUICulture switch
                {
                    { Name: var n } when n.StartsWith("fa") => // Iran
                        "http://www.google.com",
                    { Name: var n } when n.StartsWith("zh") => // China
                        "http://www.baidu.com",
                    _ =>
                        "http://www.gstatic.com/generate_204",
                };

                using HttpClient httpClient = new();
                httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMs);
                var req = httpClient.GetAsync(url);
                return req.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsProxySet(out string httpProxy, out string httpsProxy, out string ftpProxy, out string socksProxy)
        {
            bool isProxyEnable = false;
            httpProxy = httpsProxy = ftpProxy = socksProxy = string.Empty;
            RegistryKey? registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", false);
            if (registry != null)
            {
                // ProxyServer
                object? proxyServerObj = registry.GetValue("ProxyServer");
                if (proxyServerObj != null)
                {
                    string? proxyServers = proxyServerObj.ToString();
                    if (proxyServers != null)
                    {
                        if (proxyServers.Contains(';'))
                        {
                            string[] split = proxyServers.Split(';');
                            for (int n = 0; n < split.Length; n++)
                            {
                                string server = split[n];
                                if (server.StartsWith("http=")) httpProxy = server[5..];
                                else if (server.StartsWith("https=")) httpsProxy = server[6..];
                                else if (server.StartsWith("ftp=")) ftpProxy = server[4..];
                                else if (server.StartsWith("socks=")) socksProxy = server[6..];
                            }
                        }
                        else if (proxyServers.Contains('='))
                        {
                            string[] split = proxyServers.Split('=');
                            if (split[0] == "http") httpProxy = split[1];
                            else if (split[0] == "https") httpsProxy = split[1];
                            else if (split[0] == "ftp") ftpProxy = split[1];
                            else if (split[0] == "socks") socksProxy = split[1];
                        }
                        else if (proxyServers.Contains("://"))
                        {
                            string[] split = proxyServers.Split("://");
                            if (split[0] == "http") httpProxy = split[1];
                            else if (split[0] == "https") httpsProxy = split[1];
                            else if (split[0] == "ftp") ftpProxy = split[1];
                            else if (split[0] == "socks") socksProxy = split[1];
                        }
                        else if (!string.IsNullOrEmpty(proxyServers)) httpProxy = proxyServers;
                    }
                }

                // ProxyEnable
                object? proxyEnableObj = registry.GetValue("ProxyEnable");
                if (proxyEnableObj != null)
                {
                    string? proxyEnable = proxyEnableObj.ToString();
                    if (proxyEnable != null)
                    {
                        bool isInt = int.TryParse(proxyEnable, out int value);
                        if (isInt)
                            isProxyEnable = value == 1;
                    }
                }

            }
            return isProxyEnable;
        }

        [DllImport("wininet.dll")]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        private const int INTERNET_OPTION_REFRESH = 37;
        static bool settingsReturn, refreshReturn;
        public static void SetHttpProxy(string ip, int port)
        {
            RegistryKey? registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
            if (registry != null)
            {
                string proxyServer = $"{ip}:{port}";

                try
                {
                    registry.SetValue("ProxyEnable", 1, RegistryValueKind.DWord);
                    registry.SetValue("ProxyServer", proxyServer, RegistryValueKind.String);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Set Http Proxy: {ex.Message}");
                }

                // They cause the OS to refresh the settings, causing IP to realy update
                settingsReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
                refreshReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
            }
        }

        public static void UnsetProxy(bool clearIpPort)
        {
            RegistryKey? registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
            if (registry != null)
            {
                try
                {
                    registry.SetValue("ProxyEnable", 0, RegistryValueKind.DWord);
                    if (clearIpPort)
                        registry.SetValue("ProxyServer", "", RegistryValueKind.String);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unset Proxy: {ex.Message}");
                }

                // They cause the OS to refresh the settings, causing IP to realy update
                settingsReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
                refreshReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
            }
        }



    }
}
