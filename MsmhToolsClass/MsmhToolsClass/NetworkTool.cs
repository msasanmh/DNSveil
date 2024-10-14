using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Management.Automation;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MsmhToolsClass;

public static class NetworkTool
{
    public static bool IsIPv4Supported()
    {
        bool result = false;
        Socket? socket = null;

        try
        {
            socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (Socket.OSSupportsIPv4) result = true;
        }
        catch (Exception)
        {
            result = false;
        }

        socket?.Dispose();
        return result;
    }

    public static bool IsIPv6Supported()
    {
        bool result = false;
        Socket? socket = null;

        try
        {
            socket = new(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            if (Socket.OSSupportsIPv6) result = true;
        }
        catch (Exception)
        {
            result = false;
        }

        socket?.Dispose();
        return result;
    }

    public static HttpMethod ParseHttpMethod(string method)
    {
        method = method.Trim().ToLower();
        var httpMethod = method switch
        {
            "get" => HttpMethod.Get,
            "head" => HttpMethod.Head,
            "put" => HttpMethod.Put,
            "post" => HttpMethod.Post,
            "connect" => HttpMethod.Post,
            "delete" => HttpMethod.Delete,
            "patch" => HttpMethod.Patch,
            "options" => HttpMethod.Options,
            "trace" => HttpMethod.Trace,
            _ => HttpMethod.Get,
        };
        return httpMethod;
    }

    /// <summary>
    /// IP to Host using Nslookup (Windows Only)
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    public static async Task<(string Host, string BaseHost)> IpToHostAsync(string ip)
    {
        string result = string.Empty;
        string baseHost = string.Empty;
        if (!OperatingSystem.IsWindows()) return (result, baseHost);
        if (!await IsInternetAliveByNicAsync()) return (result, baseHost); // nslookup takes time when there is no internet access

        string content = await ProcessManager.ExecuteAsync("nslookup", null, ip, true, true);
        if (string.IsNullOrEmpty(content)) return (result, baseHost);
        content = content.ToLower();
        string[] split = content.Split(Environment.NewLine);
        for (int n = 0; n < split.Length; n++)
        {
            string line = split[n];
            if (line.Contains("name:"))
            {
                result = line.Replace("name:", string.Empty).Trim();
                if (result.Contains('.'))
                {
                    GetHostDetails(result, 0, out _, out _, out baseHost, out _, out _, out _);
                }
                break;
            }
        }

        return (result, baseHost);
    }

    /// <summary>
    /// Restart NAT Driver - Windows Only
    /// </summary>
    /// <returns></returns>
    public static async Task RestartNATDriver()
    {
        if (!OperatingSystem.IsWindows()) return;
        // Solve: "bind: An attempt was made to access a socket in a way forbidden by its access permissions"
        // Windows 10 above
        try
        {
            await ProcessManager.ExecuteAsync("net", null, "stop winnat", true, true);
            await ProcessManager.ExecuteAsync("net", null, "start winnat", true, true);
        }
        catch (Exception) { }
    }

    public static int GetNextPort(int currentPort)
    {
        currentPort = currentPort < 65535 ? currentPort + 1 : currentPort - 1;
        return currentPort;
    }

    public static Uri? UrlToUri(string url)
    {
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
            return uri;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        return null;
    }

    public static void GetUrlDetails(string url, int defaultPort, out string scheme, out string host, out string subHost, out string baseHost, out int port, out string path, out bool isIPv6)
    {
        url = url.Trim();
        scheme = string.Empty;

        // Strip xxxx://
        if (url.Contains("://"))
        {
            string[] split = url.Split("://");

            if (split.Length > 0)
                if (!string.IsNullOrEmpty(split[0]))
                    scheme = $"{split[0]}://";

            if (split.Length > 1)
                if (!string.IsNullOrEmpty(split[1]))
                    url = split[1];
        }

        GetHostDetails(url, defaultPort, out host, out subHost, out baseHost, out port, out path, out isIPv6);
    }

    public static void GetHostDetails(string hostIpPort, int defaultPort, out string host, out string subHost, out string baseHost, out int port, out string path, out bool isIPv6)
    {
        hostIpPort = hostIpPort.Trim();
        host = hostIpPort;
        subHost = string.Empty;
        baseHost = host;
        port = defaultPort;
        path = string.Empty;
        isIPv6 = false;

        try
        {
            // Strip /xxxx (Path)
            if (!hostIpPort.Contains("//") && hostIpPort.Contains('/'))
            {
                string[] split = hostIpPort.Split('/');
                if (!string.IsNullOrEmpty(split[0]))
                    hostIpPort = split[0];

                // Get Path
                string slash = "/";
                string outPath = slash;
                for (int n = 0; n < split.Length; n++)
                {
                    if (n != 0) outPath += split[n] + "/";
                }
                if (outPath.Length > 1 && outPath.EndsWith("/")) outPath = outPath.TrimEnd(slash.ToCharArray());
                if (!outPath.Equals("/")) path = outPath;
            }

            // Split Host and Port
            string host0 = hostIpPort;
            if (hostIpPort.Contains('[') && hostIpPort.Contains("]:")) // IPv6 + Port
            {
                string[] split = hostIpPort.Split("]:");
                if (split.Length == 2)
                {
                    isIPv6 = true;
                    host0 = $"{split[0]}]";
                    bool isInt = int.TryParse(split[1], out int result);
                    if (isInt) port = result;
                }
            }
            else if (hostIpPort.Contains('[') && hostIpPort.Contains(']')) // IPv6
            {
                string[] split = hostIpPort.Split(']');
                if (split.Length == 2)
                {
                    isIPv6 = true;
                    host0 = $"{split[0]}]";
                }
            }
            else if (!hostIpPort.Contains('[') && !hostIpPort.Contains(']') && hostIpPort.Contains(':')) // Host + Port OR IPv4 + Port
            {
                string[] split = hostIpPort.Split(':');
                if (split.Length == 2)
                {
                    host0 = split[0];
                    bool isInt = int.TryParse(split[1], out int result);
                    if (isInt) port = result;
                }
            }
            else if (hostIpPort.Contains('.')) // Host OR IPv4
            {
                host0 = hostIpPort;
            }
            else
            {
                // There Is No Host
                host0 = string.Empty;
            }

            host = host0;
            
            // Get Base Host
            if (!IsIP(host, out _) && host.Contains('.'))
            {
                baseHost = host;
                string[] dotSplit = host.Split('.');
                int realLength = dotSplit.Length;
                if (realLength >= 3)
                {
                    // e.g. *.co.uk, *.org.us
                    if (dotSplit[^2].Length <= 3 && dotSplit[^1].Length <= 2) realLength--;

                    if (realLength >= 3)
                    {
                        if (realLength == 3 && dotSplit[0].Equals("www"))
                            baseHost = baseHost.TrimStart("www.");
                        else
                        {
                            int domainLength = realLength < dotSplit.Length ? 3 : 2;

                            baseHost = string.Empty;
                            for (int i = 0; i < dotSplit.Length; i++)
                            {
                                if (i >= dotSplit.Length - domainLength)
                                    baseHost += $"{dotSplit[i]}.";
                            }
                            if (baseHost.EndsWith('.')) baseHost = baseHost[..^1];
                        }
                    }
                }
            }

            // Get Sub Host (Subdomain)
            if (!baseHost.Equals(host))
            {
                string baseHostWithDot = $".{baseHost}";
                if (host.Contains(baseHostWithDot))
                    subHost = host.Replace(baseHostWithDot, string.Empty);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetHostDetails: " + ex.Message);
        }
    }

    /// <summary>
    /// IsLocalIP
    /// </summary>
    /// <param name="ipStr">IPv4 Or IPv6</param>
    public static bool IsLocalIP(string ipStr)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ipStr)) return false;
            bool isIp = IsIP(ipStr, out IPAddress? ip);
            if (!isIp) return false;

            if (isIp && ip != null)
            {
                if (IPAddress.IsLoopback(ip)) return true;
                if (ip.IsIPv6LinkLocal) return true;
                if (ip.IsIPv6SiteLocal) return true;
                if (ip.IsIPv6UniqueLocal) return true;
            }
            
            List<string> localCIDRs = new()
            {
                "0.0.0.0/8",
                "10.0.0.0/8",
                "100.64.0.0/10",
                "127.0.0.0/8",
                "169.254.0.0/16",
                "172.16.0.0/12",
                "192.0.0.0/24",
                "192.0.2.0/24",
                "192.88.99.0/24",
                "192.168.0.0/16",
                "198.18.0.0/15",
                "198.51.100.0/24",
                "203.0.113.0/24",
                "224.0.0.0/3",
                "::/127",
                "fc00::/7",
                "fe80::/10",
                "ff00::/8"
            };

            for (int n = 0; n < localCIDRs.Count; n++)
            {
                string cidr = localCIDRs[n].Trim();
                bool isInRange = IsIpInRange(ipStr, cidr);
                if (isInRange) return true;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Uses ipinfo.io to get result
    /// </summary>
    /// <param name="iPAddress">IP to check</param>
    /// <param name="proxyScheme">Use proxy to connect</param>
    /// <returns>Company name</returns>
    public static async Task<string?> IpToCompanyAsync(string iPStr, string? proxyScheme = null)
    {
        string? company = null;
        try
        {
            using SocketsHttpHandler socketsHttpHandler = new();
            if (proxyScheme != null)
                socketsHttpHandler.Proxy = new WebProxy(proxyScheme, true);
            using HttpClient httpClient2 = new(socketsHttpHandler);
            company = await httpClient2.GetStringAsync("https://ipinfo.io/" + iPStr + "/org");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        return company;
    }

    public static IPAddress? GetLocalIPv4(string ipv4ToCheck = "8.8.8.8", int portToCheck = 53)
    {
        try
        {
            IPAddress? localIP;
            using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(ipv4ToCheck, portToCheck);
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

    public static IPAddress? GetLocalIPv6(string ipv6ToCheck = "2001:4860:4860::8888", int portToCheck = 53)
    {
        try
        {
            IPAddress? localIP;
            using Socket socket = new(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(ipv6ToCheck, portToCheck);
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

    public static IPAddress? GetDefaultGateway(bool ipv6 = false)
    {
        IPAddress? gateway = null;
        try
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            for (int n = 0; n < nics.Length; n++)
            {
                NetworkInterface nic = nics[n];
                if (nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    IPInterfaceProperties ipProperties = nic.GetIPProperties();
                    GatewayIPAddressInformationCollection gatewayAddresses = ipProperties.GatewayAddresses;
                    foreach (GatewayIPAddressInformation gatewayAddress in gatewayAddresses)
                    {
                        IPAddress address = gatewayAddress.Address;
                        if (!ipv6)
                        {
                            if (address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                gateway = address;
                                Debug.WriteLine("GetDefaultGateway: " + gateway);
                                return gateway;
                            }
                        }
                        else
                        {
                            if (address.AddressFamily == AddressFamily.InterNetworkV6)
                            {
                                gateway = address;
                                Debug.WriteLine("GetDefaultGateway: " + gateway);
                                return gateway;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception) { }
        return gateway;
    }

    [DllImport("iphlpapi.dll", CharSet = CharSet.Auto)]
    private static extern int GetBestInterface(uint destAddr, out uint bestIfIndex);
    public static IPAddress? GetGatewayForDestination(IPAddress destinationAddress)
    {
        try
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
                if (niprops == null) continue;

                var gateway = niprops.GatewayAddresses?.FirstOrDefault()?.Address;
                if (gateway == null) continue;

                if (ni.Supports(NetworkInterfaceComponent.IPv4))
                {
                    var v4props = niprops.GetIPv4Properties();
                    if (v4props == null) continue;

                    if (v4props.Index == interfaceIndex) return gateway;
                }

                if (ni.Supports(NetworkInterfaceComponent.IPv6))
                {
                    var v6props = niprops.GetIPv6Properties();
                    if (v6props == null) continue;

                    if (v6props.Index == interfaceIndex) return gateway;
                }
            }
        }
        catch (Exception) { }

        return null;
    }

    public static bool IsDomainNameValid(string domain)
    {
        return Uri.CheckHostName(domain) != UriHostNameType.Unknown;
    }

    public static bool IsIP(string ipStr, out IPAddress? ip)
    {
        ip = null;
        if (!string.IsNullOrEmpty(ipStr))
            return IPAddress.TryParse(ipStr, out ip);
        return false;
    }

    public static bool IsIPv4(IPAddress iPAddress)
    {
        return iPAddress.AddressFamily == AddressFamily.InterNetwork;
    }

    public static bool IsIPv4Valid(string ipString, out IPAddress? iPAddress)
    {
        iPAddress = null;

        try
        {
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
        catch (Exception ex)
        {
            Debug.WriteLine("NetworkTool IsIPv4Valid: " + ex.Message);
            return false;
        }
    }

    public static bool IsIPv6(IPAddress iPAddress)
    {
        return iPAddress.AddressFamily == AddressFamily.InterNetworkV6;
    }

    /// <summary>
    /// Is Ip In Range
    /// </summary>
    /// <param name="ipStr">IPv4 Or IPv6</param>
    /// <param name="cidr">IPv4 CIDR Or IPv6 CIDR</param>
    /// <returns>True Or False</returns>
    public static bool IsIpInRange(string ipStr, string cidr)
    {
        bool isInRange = false;

        try
        {
            // Split CIDR Into Base IP And Prefix Length
            if (!string.IsNullOrWhiteSpace(ipStr) && cidr.Contains('/'))
            {
                string[] split = cidr.Split('/', StringSplitOptions.TrimEntries);
                if (split.Length == 2)
                {
                    string cidrBase = split[0];
                    string prefix = split[1];
                    bool isInt = int.TryParse(prefix, out int prefixLength);
                    if (isInt)
                    {
                        // Convert Input IP And CIDR Base To Byte Array
                        bool isInputIP = IPAddress.TryParse(ipStr, out IPAddress? ip);
                        if (isInputIP && ip != null)
                        {
                            bool isCidrBaseIP = IPAddress.TryParse(cidrBase, out IPAddress? cidrIP);
                            if (isCidrBaseIP && cidrIP != null)
                            {
                                byte[] ipBytes = ip.GetAddressBytes();
                                byte[] cidrBytes = cidrIP.GetAddressBytes();

                                // If IP Address Families Match
                                if (ipBytes.Length == cidrBytes.Length)
                                {
                                    // Calculate The Mask From The Prefix Length
                                    int maskBits = prefixLength;
                                    byte[] maskBytes = new byte[ipBytes.Length];

                                    for (int n = 0; n < maskBytes.Length; n++)
                                    {
                                        int remainingBits = Math.Min(maskBits, 8);
                                        maskBytes[n] = (byte)(255 << (8 - remainingBits));
                                        maskBits -= remainingBits;
                                    }

                                    // Apply The Mask And Compare
                                    for (int n = 0; n < ipBytes.Length; n++)
                                    {
                                        if ((ipBytes[n] & maskBytes[n]) != (cidrBytes[n] & maskBytes[n])) return false;
                                    }

                                    // If All Matches It's In Range
                                    isInRange = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("NetworkTool IsIpInRange: " + ex.Message);
        }

        return isInRange;
    }

    /// <summary>
    /// Is IP Protocol Supported By ISP (Windows Only)
    /// </summary>
    /// <param name="ipStr">Ipv4 Or Ipv6</param>
    /// <returns></returns>
    public static bool IsIpProtocolReachable(string ipStr)
    {
        if (!OperatingSystem.IsWindows()) return true;
        string args = $"-n 1 {ipStr}";
        string content = ProcessManager.Execute(out _, "ping", null, args, true, false);
        return !content.Contains("transmit failed") && !content.Contains("General failure");
    }

    /// <summary>
    /// All Platforms
    /// </summary>
    /// <param name="port">Port</param>
    public static bool IsPortOpen(int port)
    {
        if (OperatingSystem.IsWindows()) return ProcessManager.GetProcessPidsByUsingPort(port).Any();
        else return !IsPortAvailable(port);
    }

    /// <summary>
    /// All Platforms
    /// </summary>
    /// <param name="port">Port</param>
    public static bool IsPortAvailable(int port)
    {
        bool isAvailable = true;

        try
        {
            IPGlobalProperties iPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            try
            {
                TcpConnectionInformation[] tcps = iPGlobalProperties.GetActiveTcpConnections();
                for (int n = 0; n < tcps.Length; n++)
                {
                    TcpConnectionInformation tcp = tcps[n];
                    if (tcp.LocalEndPoint.Port == port)
                    {
                        isAvailable = false;
                        break;
                    }
                }
            }
            catch (Exception) { }

            if (isAvailable)
            {
                try
                {
                    IPEndPoint[] udps = iPGlobalProperties.GetActiveUdpListeners();
                    for (int n = 0; n < udps.Length; n++)
                    {
                        IPEndPoint ep = udps[n];
                        if (ep.Port == port)
                        {
                            isAvailable = false;
                            break;
                        }
                    }
                }
                catch (Exception) { }
            }
        }
        catch (Exception) { }

        return isAvailable;
    }

    public static bool IsPortOpen(string host, int port, double timeoutSeconds)
    {
        try
        {
            using TcpClient client = new();
            IAsyncResult result = client.BeginConnect(host, port, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(timeoutSeconds));
            client.EndConnect(result);
            return success;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public class NICResult
    {
        public string NIC_Name { get; set; } = string.Empty;
        private NetworkInterface? pNIC;
        public NetworkInterface? NIC
        {
            get
            {
                if (pNIC != null) return pNIC;
                return GetNICByName(NIC_Name);
            }
            set
            {
                if (pNIC != value) pNIC = value;
            }
        }
        public bool IsUpAndRunning { get; set; } = false;
        public bool IsDnsSetToLoopback { get; set; } = false;
        public bool IsDnsSetToAny { get; set; } = false;
        public bool NonLocalDnsDetected { get; set; } = false;
    }

    /// <summary>
    /// Get All Network Interfaces
    /// </summary>
    /// <returns>A List of NIC Names (NetConnectionID)</returns>
    public static List<NICResult> GetAllNetworkInterfaces()
    {
        List<NICResult> nicsList = new();
        List<NICResult> nicsList1 = new();
        if (!OperatingSystem.IsWindows()) return nicsList;

        // API 1
        try
        {
            ObjectQuery? query = new("SELECT * FROM Win32_NetworkAdapter");

            using ManagementObjectSearcher searcher = new(query);
            ManagementObjectCollection queryCollection = searcher.Get();

            foreach (ManagementBaseObject m in queryCollection)
            {
                object netIdObj0 = m["NetConnectionID"];
                if (netIdObj0 == null) continue;
                string netId0 = netIdObj0.ToString() ?? string.Empty;
                netId0 = netId0.Trim();
                if (string.IsNullOrEmpty(netId0)) continue;

                // Get NIC
                NetworkInterface? nic = GetNICByName(netId0);

                // Get Up And Running
                ushort up = 0;
                try { up = Convert.ToUInt16(m["NetConnectionStatus"]); } catch (Exception) { }
                bool isUpAndRunning = up == 2; // Connected

                // Get DNS Addresses
                bool isDnsSetToLoopback = false;
                bool isDnsSetToAny = false;
                bool nonLocalDnsDetected = false;

                if (nic != null)
                {
                    IPAddressCollection dnss = nic.GetIPProperties().DnsAddresses;
                    for (int n = 0; n < dnss.Count; n++)
                    {
                        IPAddress dns = dnss[n];
                        if (dns.Equals(IPAddress.Loopback)) isDnsSetToLoopback = true;
                        if (dns.Equals(IPAddress.Any)) isDnsSetToAny = true;
                        if (!dns.Equals(IPAddress.Loopback) && !dns.Equals(IPAddress.IPv6Loopback) &&
                            !dns.Equals(IPAddress.Any) && !dns.Equals(IPAddress.IPv6Any)) nonLocalDnsDetected = true;
                    }
                }

                NICResult nicr = new()
                {
                    NIC_Name = netId0,
                    NIC = nic,
                    IsUpAndRunning = isUpAndRunning,
                    IsDnsSetToLoopback = isDnsSetToLoopback,
                    IsDnsSetToAny = isDnsSetToAny,
                    NonLocalDnsDetected = nonLocalDnsDetected
                };
                nicsList1.Add(nicr);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetAllNetworkInterfaces: {ex.Message}");
        }

        // API 2
        List<NICResult> nicsList2 = GetNetworkInterfaces();

        // Merge API1 & API2
        try
        {
            nicsList = nicsList1.Concat(nicsList2).ToList();
            nicsList = nicsList.DistinctBy(x => x.NIC_Name).ToList();
        }
        catch (Exception) { }

        return nicsList;
    }

    /// <summary>
    /// Does not contain disabled NICs
    /// </summary>
    /// <returns></returns>
    public static List<NICResult> GetNetworkInterfaces()
    {
        List<NICResult> nicsList = new();

        try
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            for (int n1 = 0; n1 < networkInterfaces.Length; n1++)
            {
                NetworkInterface nic = networkInterfaces[n1];
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    IPInterfaceStatistics statistics = nic.GetIPStatistics();
                    if (statistics.BytesReceived > 0 && statistics.BytesSent > 0)
                    {
                        bool isUpAndRunning = nic.OperationalStatus == OperationalStatus.Up;

                        // Get DNS Addresses
                        bool isDnsSetToLoopback = false;
                        bool isDnsSetToAny = false;
                        bool nonLocalDnsDetected = false;

                        IPAddressCollection dnss = nic.GetIPProperties().DnsAddresses;
                        for (int n = 0; n < dnss.Count; n++)
                        {
                            IPAddress dns = dnss[n];
                            if (dns.Equals(IPAddress.Loopback) || dns.Equals(IPAddress.IPv6Loopback)) isDnsSetToLoopback = true;
                            if (dns.Equals(IPAddress.Any) || dns.Equals(IPAddress.IPv6Any)) isDnsSetToAny = true;
                            if (!dns.Equals(IPAddress.Loopback) && !dns.Equals(IPAddress.IPv6Loopback) &&
                                !dns.Equals(IPAddress.Any) && !dns.Equals(IPAddress.IPv6Any)) nonLocalDnsDetected = true;
                        }

                        NICResult nicr = new()
                        {
                            NIC_Name = nic.Name,
                            NIC = nic,
                            IsUpAndRunning = isUpAndRunning,
                            IsDnsSetToLoopback = isDnsSetToLoopback,
                            IsDnsSetToAny = isDnsSetToAny,
                            NonLocalDnsDetected = nonLocalDnsDetected
                        };
                        nicsList.Add(nicr);
                    }
                }
            }
        }
        catch (Exception) { }

        return nicsList;
    }

    public static async Task EnableNICAsync(string nicName)
    {
        string args = $"interface set interface \"{nicName}\" enable";
        await ProcessManager.ExecuteAsync("netsh", null, args, true, true);
    }

    public static void EnableNIC(string nicName)
    {
        string args = $"interface set interface \"{nicName}\" enable";
        ProcessManager.ExecuteOnly("netsh", null, args, true, true);
    }

    public static async Task DisableNICAsync(string nicName)
    {
        string args = $"interface set interface \"{nicName}\" disable";
        await ProcessManager.ExecuteAsync("netsh", null, args, true, true);
    }

    public static void DisableNIC(string nicName)
    {
        string args = $"interface set interface \"{nicName}\" disable";
        ProcessManager.ExecuteOnly("netsh", null, args, true, true);
    }

    public static async Task<bool> EnableNicIPv6(string nicName)
    {
        bool success = false;
        string args = $"Enable-NetAdapterBinding -Name '{nicName}' -ComponentID ms_tcpip6";
        try
        {
            await Task.Run(() =>
            {
                using PowerShell ps = PowerShell.Create(RunspaceMode.NewRunspace);

                // Run As Admin
                ps.AddCommand("Set-ExecutionPolicy")
                  .AddParameter("Scope", "Process")
                  .AddParameter("ExecutionPolicy", "Bypass")
                  .Invoke();

                ps.AddScript(args).Invoke();
                success = !ps.HadErrors;
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine("EnableNicIPv6: " + ex.Message);
        }
        return success;
    }

    public static async Task<bool> DisableNicIPv6(string nicName)
    {
        bool success = false;
        string args = $"Disable-NetAdapterBinding -Name \"{nicName}\" -ComponentID ms_tcpip6";
        try
        {
            await Task.Run(() =>
            {
                using PowerShell ps = PowerShell.Create(RunspaceMode.NewRunspace);

                // Run As Admin
                ps.AddCommand("Set-ExecutionPolicy")
                  .AddParameter("Scope", "Process")
                  .AddParameter("ExecutionPolicy", "Bypass")
                  .Invoke();

                ps.AddScript(args).Invoke();
                success = !ps.HadErrors;
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DisableNicIPv6: " + ex.Message);
        }
        return success;
    }

    public static NetworkInterface? GetNICByName(string name)
    {
        try
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            for (int n = 0; n < networkInterfaces.Length; n++)
            {
                NetworkInterface nic = networkInterfaces[n];
                if (nic.Name.Equals(name)) return nic;
            }
        }
        catch (Exception) { }
        return null;
    }

    public static NetworkInterface? GetNICByDescription(string description)
    {
        try
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            for (int n = 0; n < networkInterfaces.Length; n++)
            {
                NetworkInterface nic = networkInterfaces[n];
                if (nic.Description.Equals(description)) return nic;
            }
        }
        catch (Exception) { }
        return null;
    }

    /// <summary>
    /// Set's the IPv4 DNS Server of the local machine (Windows Only)
    /// </summary>
    /// <param name="nicName">NIC Name</param>
    /// <param name="dnsServers">Comma seperated list of DNS server addresses</param>
    /// <remarks>Requires a reference to the System.Management namespace</remarks>
    public static async Task SetDnsIPv4(string nicName, string dnsServers)
    {
        if (!OperatingSystem.IsWindows()) return;
        // Requires Elevation
        // Only netsh can set DNS on Windows 7
        if (string.IsNullOrEmpty(nicName)) return;

        try
        {
            string dnsServer1 = dnsServers;
            string dnsServer2 = string.Empty;
            if (dnsServers.Contains(','))
            {
                string[] split = dnsServers.Split(',');
                dnsServer1 = split[0].Trim();
                dnsServer2 = split[1].Trim();
            }

            string processName = "netsh";
            string processArgs1 = $"interface ipv4 delete dnsservers \"{nicName}\" all";
            string processArgs2 = $"interface ipv4 set dnsservers \"{nicName}\" static {dnsServer1} primary";
            string processArgs3 = $"interface ipv4 add dnsservers \"{nicName}\" {dnsServer2} index=2";
            await ProcessManager.ExecuteAsync(processName, null, processArgs1, true, true);
            await ProcessManager.ExecuteAsync(processName, null, processArgs2, true, true);
            if (!string.IsNullOrEmpty(dnsServer2))
                await ProcessManager.ExecuteAsync(processName, null, processArgs3, true, true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SetDnsIPv4: " + ex.Message);
        }
    }

    /// <summary>
    /// Set's the IPv4 DNS Server of the local machine (Windows Only)
    /// </summary>
    /// <param name="nic">NIC address</param>
    /// <param name="dnsServers">Comma seperated list of DNS server addresses</param>
    /// <remarks>Requires a reference to the System.Management namespace</remarks>
    public static async Task SetDnsIPv4(NetworkInterface nic, string dnsServers)
    {
        if (!OperatingSystem.IsWindows()) return;
        // Requires Elevation
        if (nic == null) return;

        await SetDnsIPv4(nic.Name, dnsServers);
    }

    /// <summary>
    /// Set's the IPv6 DNS Server of the local machine (Windows Only)
    /// </summary>
    /// <param name="nicName">NIC Name</param>
    /// <param name="dnsServers">Comma seperated list of DNS server addresses</param>
    /// <remarks>Requires a reference to the System.Management namespace</remarks>
    public static async Task SetDnsIPv6(string nicName, string dnsServers)
    {
        if (!OperatingSystem.IsWindows()) return;
        // Requires Elevation
        // Only netsh can set DNS on Windows 7
        if (string.IsNullOrEmpty(nicName)) return;

        try
        {
            string dnsServer1 = dnsServers;
            string dnsServer2 = string.Empty;
            if (dnsServers.Contains(','))
            {
                string[] split = dnsServers.Split(',');
                dnsServer1 = split[0].Trim();
                dnsServer2 = split[1].Trim();
            }

            string processName = "netsh";
            string processArgs1 = $"interface ipv6 delete dnsservers \"{nicName}\" all";
            string processArgs2 = $"interface ipv6 set dnsservers \"{nicName}\" static {dnsServer1} primary";
            string processArgs3 = $"interface ipv6 add dnsservers \"{nicName}\" {dnsServer2} index=2";
            await ProcessManager.ExecuteAsync(processName, null, processArgs1, true, true);
            await ProcessManager.ExecuteAsync(processName, null, processArgs2, true, true);
            if (!string.IsNullOrEmpty(dnsServer2))
                await ProcessManager.ExecuteAsync(processName, null, processArgs3, true, true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SetDnsIPv6: " + ex.Message);
        }
    }

    /// <summary>
    /// Set's the IPv6 DNS Server of the local machine (Windows Only)
    /// </summary>
    /// <param name="nic">NIC address</param>
    /// <param name="dnsServers">Comma seperated list of DNS server addresses</param>
    /// <remarks>Requires a reference to the System.Management namespace</remarks>
    public static async Task SetDnsIPv6(NetworkInterface nic, string dnsServers)
    {
        if (!OperatingSystem.IsWindows()) return;
        // Requires Elevation
        if (nic == null) return;

        await SetDnsIPv6(nic.Name, dnsServers);
    }

    /// <summary>
    /// Unset IPv4 DNS to DHCP (Windows Only)
    /// </summary>
    /// <param name="nicName">Network Interface Name</param>
    public static async Task UnsetDnsIPv4(string nicName)
    {
        if (!OperatingSystem.IsWindows()) return;
        // Requires Elevation - Can't Unset DNS when there is no Internet connectivity but netsh can :)
        // NetSh Command: netsh interface ip set dns "nicName" source=dhcp
        if (string.IsNullOrEmpty(nicName)) return;

        try
        {
            string processName = "netsh";
            string processArgs1 = $"interface ipv4 delete dnsservers \"{nicName}\" all";
            string processArgs2 = $"interface ipv4 set dnsservers \"{nicName}\" source=dhcp";
            await ProcessManager.ExecuteAsync(processName, null, processArgs1, true, true);
            await ProcessManager.ExecuteAsync(processName, null, processArgs2, true, true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Unset IPv4 DNS to DHCP (Windows Only)
    /// </summary>
    /// <param name="nic">Network Interface</param>
    public static async Task UnsetDnsIPv4(NetworkInterface nic)
    {
        if (!OperatingSystem.IsWindows()) return;
        // Requires Elevation - Can't Unset DNS when there is no Internet connectivity but netsh can :)
        if (nic == null) return;

        await UnsetDnsIPv4(nic.Name);
    }

    /// <summary>
    /// Unset IPv4 DNS by seting DNS to Static
    /// </summary>
    /// <param name="nic">Network Interface</param>
    /// <param name="dns1">Primary</param>
    /// <param name="dns2">Secondary</param>
    public static async Task UnsetDnsIPv4(NetworkInterface nic, string dns1, string? dns2)
    {
        string dnsServers = dns1;
        if (!string.IsNullOrEmpty(dns2)) dnsServers += $",{dns2}";
        await SetDnsIPv4(nic, dnsServers);
    }

    /// <summary>
    /// Unset IPv4 DNS by seting DNS to Static
    /// </summary>
    /// <param name="nicName">Network Interface Name</param>
    /// <param name="dns1">Primary</param>
    /// <param name="dns2">Secondary</param>
    public static async Task UnsetDnsIPv4(string nicName, string dns1, string? dns2)
    {
        string dnsServers = dns1;
        if (!string.IsNullOrEmpty(dns2)) dnsServers += $",{dns2}";
        await SetDnsIPv4(nicName, dnsServers);
    }

    /// <summary>
    /// Unset IPv6 DNS to DHCP (Windows Only)
    /// </summary>
    /// <param name="nicName">Network Interface Name</param>
    public static async Task UnsetDnsIPv6(string nicName)
    {
        if (!OperatingSystem.IsWindows()) return;
        // Requires Elevation - Can't Unset DNS when there is no Internet connectivity but netsh can :)
        // NetSh Command: netsh interface ip set dns "nicName" source=dhcp
        if (string.IsNullOrEmpty(nicName)) return;

        try
        {
            string processName = "netsh";
            string processArgs1 = $"interface ipv6 delete dnsservers \"{nicName}\" all";
            string processArgs2 = $"interface ipv6 set dnsservers \"{nicName}\" source=dhcp";
            await ProcessManager.ExecuteAsync(processName, null, processArgs1, true, true);
            await ProcessManager.ExecuteAsync(processName, null, processArgs2, true, true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("UnsetDnsIPv6: " + ex.Message);
        }
    }

    /// <summary>
    /// Unset IPv6 DNS to DHCP (Windows Only)
    /// </summary>
    /// <param name="nic">Network Interface</param>
    public static async Task UnsetDnsIPv6(NetworkInterface nic)
    {
        if (!OperatingSystem.IsWindows()) return;
        // Requires Elevation - Can't Unset DNS when there is no Internet connectivity but netsh can :)
        if (nic == null) return;
        
        await UnsetDnsIPv6(nic.Name);
    }

    /// <summary>
    /// Unset IPv6 DNS by seting DNS to Static
    /// </summary>
    /// <param name="nic">Network Interface</param>
    /// <param name="dns1">Primary</param>
    /// <param name="dns2">Secondary</param>
    public static async Task UnsetDnsIPv6(NetworkInterface nic, string dns1, string? dns2)
    {
        string dnsServers = dns1;
        if (!string.IsNullOrEmpty(dns2)) dnsServers += $",{dns2}";
        await SetDnsIPv6(nic, dnsServers);
    }

    /// <summary>
    /// Unset IPv6 DNS by seting DNS to Static
    /// </summary>
    /// <param name="nicName">Network Interface Name</param>
    /// <param name="dns1">Primary</param>
    /// <param name="dns2">Secondary</param>
    public static async Task UnsetDnsIPv6(string nicName, string dns1, string? dns2)
    {
        string dnsServers = dns1;
        if (!string.IsNullOrEmpty(dns2)) dnsServers += $",{dns2}";
        await SetDnsIPv6(nicName, dnsServers);
    }

    /// <summary>
    /// Is DNS Set to 127.0.0.1 - Using Nslookup (Windows Only)
    /// </summary>
    public static async Task<(bool IsSet, string Host, string IP)> IsDnsSetToLocalAsync()
    {
        bool result = false;
        string host = string.Empty, ip = string.Empty;
        if (!OperatingSystem.IsWindows()) return (result, host, ip);
        if (!await IsInternetAliveByNicAsync()) return (result, host, ip); // nslookup takes time when there is no internet access

        string content = await ProcessManager.ExecuteAsync("nslookup", null, "0.0.0.0", true, true);
        //string content = ProcessManager.Execute(out _, "nslookup", null, "0.0.0.0", true, true);
        if (string.IsNullOrEmpty(content)) return (result, host, ip);
        content = content.ToLower();
        string[] split = content.Split(Environment.NewLine);
        for (int n = 0; n < split.Length; n++)
        {
            string line = split[n];
            if (line.Contains("server:"))
            {
                line = line.Replace("server:", string.Empty).Trim();
                host = line;
                if (host.Equals("localhost")) result = true;
            }
            else if (line.Contains("address:"))
            {
                line = line.Replace("address:", string.Empty).Trim();
                ip = line;
                if (ip.Equals(IPAddress.Loopback.ToString())) result = true;
                if (ip.Equals(IPAddress.IPv6Loopback.ToString())) result = true;
            }
        }
        return (result, host, ip);
    }

    /// <summary>
    /// Check if DNS is set to Static or DHCP using netsh (Windows Only)
    /// </summary>
    /// <param name="nic">Network Interface</param>
    /// <param name="dnsServer1">Primary DNS Server</param>
    /// <param name="dnsServer2">Secondary DNS Server</param>
    /// <returns>True = Static, False = DHCP</returns>
    public static bool IsDnsSet(NetworkInterface nic, out string dnsServer1, out string dnsServer2)
    {
        dnsServer1 = dnsServer2 = string.Empty;
        if (!OperatingSystem.IsWindows()) return false;
        if (nic == null) return false;

        string processName = "netsh";
        string processArgs = $"interface ipv4 show dnsservers {nic.Name}";
        string stdout = ProcessManager.Execute(out _, processName, null, processArgs, true, true);

        List<string> lines = stdout.SplitToLines();
        for (int n = 0; n < lines.Count; n++)
        {
            string line = lines[n];
            // Get Primary
            if (line.Contains(": ") && line.Contains('.') && line.Count(c => c == '.') == 3)
            {
                string[] split = line.Split(": ");
                if (split.Length > 1)
                {
                    dnsServer1 = split[1].Trim();
                    Debug.WriteLine($"DNS 1: {dnsServer1}");
                }
            }

            // Get Secondary
            if (!line.Contains(": ") && line.Contains('.') && line.Count(c => c == '.') == 3)
            {
                dnsServer2 = line.Trim();
                Debug.WriteLine($"DNS 2: {dnsServer2}");
            }
        }
        //Debug.WriteLine(stdout);
        return !stdout.Contains("DHCP");
    }

    /// <summary>
    /// Check Internet Access Based On NIC Send And Receive
    /// </summary>
    public static async Task<bool> IsInternetAliveByNicAsync(IPAddress? ip = null, int timeoutMS = 2000)
    {
        try
        {
            ip ??= CultureInfo.InstalledUICulture switch
            {
                { Name: string n } when n.ToLower().StartsWith("fa") => IPAddress.Parse("8.8.8.8"), // Iran
                { Name: string n } when n.ToLower().StartsWith("ru") => IPAddress.Parse("77.88.8.7"), // Russia
                { Name: string n } when n.ToLower().StartsWith("zh") => IPAddress.Parse("223.6.6.6"), // China
                _ => IPAddress.Parse("1.1.1.1") // Others
            };

            // Only recognizes changes related to Internet adapters
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            for (int n = 0; n < nics.Length; n++)
            {
                NetworkInterface nic = nics[n];
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    if (nic.OperationalStatus == OperationalStatus.Up)
                    {
                        IPInterfaceStatistics statistics = nic.GetIPStatistics();
                        long bytesSent1 = statistics.BytesSent;
                        long bytesReceived1 = statistics.BytesReceived;

                        try
                        {
                            using Ping ping = new();
                            await ping.SendPingAsync(ip, timeoutMS);
                        }
                        catch (Exception) { }

                        statistics = nic.GetIPStatistics();
                        long bytesSent2 = statistics.BytesSent;
                        long bytesReceived2 = statistics.BytesReceived;

                        if (bytesSent2 > bytesSent1 && bytesReceived2 > bytesReceived1) return true;
                    }
                }
            }
            return false;
        }
        catch (Exception)
        {
            // NetworkInformationException The system cannot find the file specified
            return false;
        }
    }

    /// <summary>
    /// Check Internet Access Based On Pinging A DNS IP
    /// </summary>
    public static async Task<bool> IsInternetAliveByPingAsync(IPAddress? ip, int timeoutMS = 3000)
    {
        try
        {
            ip ??= CultureInfo.InstalledUICulture switch
            {
                { Name: string n } when n.ToLower().StartsWith("fa") => IPAddress.Parse("8.8.8.8"), // Iran
                { Name: string n } when n.ToLower().StartsWith("ru") => IPAddress.Parse("77.88.8.7"), // Russia
                { Name: string n } when n.ToLower().StartsWith("zh") => IPAddress.Parse("223.6.6.6"), // China
                _ => IPAddress.Parse("1.1.1.1") // Others
            };

            Ping ping = new();
            PingReply reply = await ping.SendPingAsync(ip, timeoutMS);
            ping.Dispose();
            return reply.Status == IPStatus.Success;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public enum InternetState
    {
        Online,
        Unstable,
        Offline,
        Unknown
    }

    public static async Task<InternetState> GetInternetStateAsync(IPAddress? ip, int timeoutMS = 3000)
    {
        try
        {
            ip ??= CultureInfo.InstalledUICulture switch
            {
                { Name: string n } when n.ToLower().StartsWith("fa") => IPAddress.Parse("8.8.8.8"), // Iran
                { Name: string n } when n.ToLower().StartsWith("ru") => IPAddress.Parse("77.88.8.7"), // Russia
                { Name: string n } when n.ToLower().StartsWith("zh") => IPAddress.Parse("223.6.6.6"), // China
                _ => IPAddress.Parse("1.1.1.1") // Others
            };

            bool isAliveByPing = await IsInternetAliveByPingAsync(ip, timeoutMS);
            if (isAliveByPing)
            {
                return InternetState.Online;
            }
            else
            {
                bool isAliveByNic = await IsInternetAliveByNicAsync(ip, timeoutMS);
                return isAliveByNic ? InternetState.Unstable : InternetState.Offline;
            }
        }
        catch (Exception)
        {
            return InternetState.Offline;
        }
    }

    public static async Task<HttpStatusCode> GetHttpStatusCodeAsync(string urlOrDomain, string? ip, int timeoutMs, bool useSystemProxy, bool isAgnosticProxyTest = false, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null, CancellationToken ct = default)
    {
        HttpStatusCode result = HttpStatusCode.RequestTimeout;
        if (string.IsNullOrWhiteSpace(urlOrDomain)) return result;

        try
        {
            GetUrlDetails(urlOrDomain.Trim(), 443, out string scheme, out string host, out _, out _, out int port, out string path, out _);
            string origHost = host;
            if (string.IsNullOrEmpty(scheme)) scheme = "https://";
            if (!string.IsNullOrWhiteSpace(ip)) host = ip.Trim();

            UriBuilder uriBuilder = new()
            {
                Scheme = scheme,
                Host = host,
                Port = port,
                Path = path
            };

            Uri uri = uriBuilder.Uri;
            
            if (useSystemProxy)
            {
                string systemProxyScheme = GetSystemProxy(); // Reading from Registry
                if (!string.IsNullOrEmpty(systemProxyScheme))
                {
                    proxyScheme = systemProxyScheme;
                    proxyUser = string.Empty;
                    proxyPass = string.Empty;
                }
            }

            HttpRequest hr = new()
            {
                CT = ct,
                URI = uri,
                Method = HttpMethod.Get,
                TimeoutMS = timeoutMs,
                AllowInsecure = true, // Ignore Cert Check To Make It Faster
                AllowAutoRedirect = true,
                ProxyScheme = proxyScheme,
                ProxyUser = proxyUser,
                ProxyPass = proxyPass,
            };
            hr.Headers.Add("host", origHost); // In Case Of Using IP
            if (isAgnosticProxyTest) hr.UserAgent = "SDC - Secure DNS Client"; // Proxy Test Protocol Depends On This

            HttpRequestResponse hrr = await HttpRequest.SendAsync(hr).ConfigureAwait(false);

            result = hrr.StatusCode;
        }
        catch (Exception) { }

        return result;
    }

    public static async Task<string> GetHeadersAsync(string urlOrDomain, string? ip, int timeoutMs, bool useSystemProxy, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null)
    {
        string result = string.Empty;
        if (string.IsNullOrWhiteSpace(urlOrDomain)) return result;

        try
        {
            GetUrlDetails(urlOrDomain.Trim(), 443, out string scheme, out string host, out _, out _, out int port, out string path, out _);
            string origHost = host;
            if (string.IsNullOrEmpty(scheme)) scheme = "https://";
            if (!string.IsNullOrWhiteSpace(ip)) host = ip.Trim();
            bool firstTrySuccess = false;

            try
            {
                UriBuilder uriBuilder = new()
                {
                    Scheme = scheme,
                    Host = host,
                    Port = port,
                    Path = path
                };

                Uri uri = uriBuilder.Uri;

                if (useSystemProxy)
                {
                    string systemProxyScheme = GetSystemProxy(); // Reading from Registry
                    if (!string.IsNullOrEmpty(systemProxyScheme))
                    {
                        proxyScheme = systemProxyScheme;
                        proxyUser = string.Empty;
                        proxyPass = string.Empty;
                    }
                }

                HttpRequest hr = new()
                {
                    URI = uri,
                    Method = HttpMethod.Get,
                    UserAgent = "Other",
                    TimeoutMS = timeoutMs,
                    AllowInsecure = true, // Ignore Cert Check To Make It Faster
                    AllowAutoRedirect = true,
                    ProxyScheme = proxyScheme,
                    ProxyUser = proxyUser,
                    ProxyPass = proxyPass,
                };
                hr.Headers.Add("host", origHost); // In Case Of Using IP

                HttpRequestResponse hrr = await HttpRequest.SendAsync(hr).ConfigureAwait(false);

                List<string> resultList = new()
                {
                    hrr.StatusCode.ToString()
                };

                for (int n = 0; n < hrr.Headers.Count; n++)
                {
                    string? key = hrr.Headers.GetKey(n);
                    string? val = hrr.Headers.Get(n);

                    if (string.IsNullOrEmpty(key)) continue;
                    if (string.IsNullOrEmpty(val)) continue;

                    string kv = $"{key}: {val}";
                    resultList.Add(kv);
                    firstTrySuccess = true;
                }

                if (resultList.Count > 0)
                    result = resultList.ToString(Environment.NewLine);
            }
            catch (Exception) { }

            try
            {
                if (!firstTrySuccess && !urlOrDomain.Contains("://www."))
                {
                    urlOrDomain = $"{scheme}www.{origHost}:{port}{path}";
                    result = await GetHeadersAsync(urlOrDomain, ip, timeoutMs, useSystemProxy, proxyScheme, proxyUser, proxyPass);
                }
            }
            catch (Exception) { }
        }
        catch (Exception) { }

        return result;
    }

    public static async Task<string> GetHeadersOLD(string urlOrDomain, string? ip, int timeoutMs, bool useSystemProxy, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null)
    {
        if (string.IsNullOrWhiteSpace(urlOrDomain)) return string.Empty;
        HttpResponseMessage? response = null;

        urlOrDomain = urlOrDomain.Trim();
        GetUrlDetails(urlOrDomain, 443, out string scheme, out string host, out _, out _, out int port, out string path, out _);
        if (string.IsNullOrEmpty(scheme))
        {
            scheme = "https://";
            urlOrDomain = $"{scheme}{host}:{port}{path}";
        }
        string url = urlOrDomain;
        //Debug.WriteLine("GetHeaders: " + url);
        if (!string.IsNullOrEmpty(ip))
        {
            ip = ip.Trim();
            url = $"{scheme}{ip}:{port}{path}";
            //Debug.WriteLine("GetHeaders: " + url);
        }

        try
        {
            Uri uri = new(url, UriKind.Absolute);

            using HttpClientHandler handler = new();
            handler.AllowAutoRedirect = true;
            if (useSystemProxy)
            {
                // WebRequest.GetSystemWebProxy() Can't always detect System Proxy
                proxyScheme = GetSystemProxy(); // Reading from Registry
                if (!string.IsNullOrEmpty(proxyScheme))
                {
                    //Debug.WriteLine("GetHeaders: " + proxyScheme);
                    NetworkCredential credential = CredentialCache.DefaultNetworkCredentials;
                    handler.Proxy = new WebProxy(proxyScheme, true, null, credential);
                    handler.Credentials = credential;
                    handler.UseProxy = true;
                }
                else
                {
                    Debug.WriteLine("GetHeaders: System Proxy Is Null.");
                    handler.UseProxy = false;
                }
            }
            else if (!string.IsNullOrEmpty(proxyScheme))
            {
                //Debug.WriteLine("GetHeaders: " + proxyScheme);
                NetworkCredential credential = new(proxyUser, proxyPass);
                handler.Proxy = new WebProxy(proxyScheme, true, null, credential);
                handler.Credentials = credential;
                handler.UseProxy = true;
            }
            else handler.UseProxy = false;

            // Ignore Cert Check To Make It Faster
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true;

            // Get Only Header
            using HttpRequestMessage message = new(HttpMethod.Get, uri);
            message.Headers.TryAddWithoutValidation("User-Agent", "Other");
            message.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
            message.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            message.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

            if (!string.IsNullOrEmpty(ip))
            {
                message.Headers.TryAddWithoutValidation("host", host);
            }

            using HttpClient httpClient = new(handler);
            httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMs);
            response = await httpClient.SendAsync(message, CancellationToken.None);
        }
        catch (Exception) { }

        string result = string.Empty;

        try
        {
            if (response != null)
            {
                result += response.StatusCode.ToString();
                //Debug.WriteLine("GetHeaders: " + result);
                result += Environment.NewLine + response.Headers.ToString();
                try { response.Dispose(); } catch (Exception) { }
            }
            result = result.ReplaceLineEndings();
            if (result.StartsWith(Environment.NewLine)) result = result.TrimStart(Environment.NewLine);
            result = result.Trim();

            if (string.IsNullOrEmpty(result) && !urlOrDomain.Contains("://www."))
            {
                urlOrDomain = $"{scheme}www.{host}:{port}{path}";
                result = await GetHeadersAsync(urlOrDomain, ip, timeoutMs, useSystemProxy, proxyScheme, proxyUser, proxyPass);
            }
        }
        catch (Exception) { }

        return result;
    }

    /// <summary>
    /// IsWebsiteOnlineAsync
    /// </summary>
    /// <param name="url">URL or Domain to check</param>
    /// <param name="timeoutMs">Timeout (Ms)</param>
    /// <param name="useSystemProxy">Use System Proxy (will override proxyScheme, proxyUser and proxyPass)</param>
    /// <param name="proxyScheme">Only the 'http', 'socks4', 'socks4a' and 'socks5' schemes are allowed for proxies.</param>
    /// <returns></returns>
    public static async Task<bool> IsWebsiteOnlineAsync(string urlOrDomain, string? ip, int timeoutMs, bool useSystemProxy, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null)
    {
        string headers = await GetHeadersAsync(urlOrDomain, ip, timeoutMs, useSystemProxy, proxyScheme, proxyUser, proxyPass);
        return !string.IsNullOrEmpty(headers);
    }

    /// <summary>
    /// Check if Proxy is Set (Windows Only)
    /// </summary>
    /// <param name="httpProxy"></param>
    /// <param name="httpsProxy"></param>
    /// <param name="ftpProxy"></param>
    /// <param name="socksProxy"></param>
    /// <returns></returns>
    public static bool IsProxySet(out string httpProxy, out string httpsProxy, out string ftpProxy, out string socksProxy)
    {
        bool isProxyEnable = false;
        httpProxy = httpsProxy = ftpProxy = socksProxy = string.Empty;
        if (!OperatingSystem.IsWindows()) return false;
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

            try { registry.Dispose(); } catch (Exception) { }
        }
        return isProxyEnable;
    }

    public static string GetSystemProxy()
    {
        string result = string.Empty;
        bool isProxySet = IsProxySet(out string httpProxy, out string httpsProxy, out _, out string socksProxy);
        if (isProxySet)
        {
            if (!string.IsNullOrEmpty(httpProxy)) result = $"http://{httpProxy}";
            else if (!string.IsNullOrEmpty(httpsProxy)) result = $"https://{httpsProxy}";
            else if (!string.IsNullOrEmpty(socksProxy)) result = $"socks5://{socksProxy}";
        }
        return result;
    }

    /// <summary>
    /// Set Proxy to System (Windows Only)
    /// </summary>
    public static void SetProxy(string? httpIpPort, string? httpsIpPort, string? ftpIpPort, string? socksIpPort, bool useHttpForAll)
    {
        if (!OperatingSystem.IsWindows()) return;
        RegistryKey? registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
        if (registry != null)
        {
            string proxyServer = string.Empty;
            if (useHttpForAll)
            {
                if (!string.IsNullOrEmpty(httpIpPort)) proxyServer += $"http://{httpIpPort}";
            }
            else
            {
                if (!string.IsNullOrEmpty(httpIpPort)) proxyServer += $"http={httpIpPort};";
                if (!string.IsNullOrEmpty(httpsIpPort)) proxyServer += $"https={httpsIpPort};";
                if (!string.IsNullOrEmpty(ftpIpPort)) proxyServer += $"ftp={ftpIpPort};";
                if (!string.IsNullOrEmpty(socksIpPort)) proxyServer += $"socks={socksIpPort};";
                if (proxyServer.EndsWith(';')) proxyServer = proxyServer.TrimEnd(';');
            }

            try
            {
                if (!string.IsNullOrEmpty(proxyServer))
                {
                    registry.SetValue("AutoDetect", 0, RegistryValueKind.DWord);
                    registry.SetValue("ProxyEnable", 1, RegistryValueKind.DWord);
                    registry.SetValue("ProxyServer", proxyServer, RegistryValueKind.String);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Set Http Proxy: {ex.Message}");
            }

            RegistryTool.ApplyRegistryChanges();
            try { registry.Dispose(); } catch (Exception) { }
        }
    }

    /// <summary>
    /// Unset Internet Options Proxy (Windows Only)
    /// </summary>
    /// <param name="clearIpPort">Clear IP and Port</param>
    /// <param name="applyRegistryChanges">Don't apply registry changes on app exit</param>
    public static void UnsetProxy(bool clearIpPort, bool applyRegistryChanges)
    {
        if (!OperatingSystem.IsWindows()) return;
        RegistryKey? registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
        if (registry != null)
        {
            try
            {
                registry.SetValue("AutoDetect", 1, RegistryValueKind.DWord);
                registry.SetValue("ProxyEnable", 0, RegistryValueKind.DWord);
                if (clearIpPort)
                    registry.SetValue("ProxyServer", "", RegistryValueKind.String);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unset Proxy: {ex.Message}");
            }

            if (applyRegistryChanges) RegistryTool.ApplyRegistryChanges();
            try { registry.Dispose(); } catch (Exception) { }
        }
    }

    public static async Task<bool> IsHostBlocked(string host, int port, int timeoutMS)
    {
        string url;
        if (port == 80) url = $"http://{host}:{port}";
        else url = $"https://{host}:{port}";
        return !await IsWebsiteOnlineAsync(url, null, timeoutMS, false);
    }

    public static async Task<bool> CanPing(string host, int timeoutMS)
    {
        host = host.Trim();
        if (string.IsNullOrEmpty(host)) return false;
        if (host.Equals("0.0.0.0")) return false;
        if (host.Equals("::0")) return false;
        Task<bool> task = Task.Run(() =>
        {
            try
            {
                Ping ping = new();
                PingReply? reply;
                bool isIp = IsIP(host, out IPAddress? ip);
                if (isIp && ip != null)
                    reply = ping.Send(ip, timeoutMS);
                else
                    reply = ping.Send(host, timeoutMS);

                if (reply == null) return false;

                ping.Dispose();
                return reply.Status == IPStatus.Success;
            }
            catch (Exception)
            {
                return false;
            }
        });

        try { return await task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMS + 100)); } catch (Exception) { return false; }
    }

    public static async Task<bool> CanTcpConnect(IPAddress ip, int port, int timeoutMS)
    {
        var task = Task.Run(async () =>
        {
            try
            {
                IPEndPoint ep = new(ip, port);
                using TcpClient client = new();
                client.SendTimeout = timeoutMS;
                client.ReceiveTimeout = timeoutMS;
                client.Client.NoDelay = true;
                await client.Client.ConnectAsync(ep).ConfigureAwait(false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        });

        try { return await task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMS + 100)); } catch (Exception) { return false; }
    }

    public static async Task<bool> CanTcpConnect(string host, int port, int timeoutMS)
    {
        var task = Task.Run(async () =>
        {
            try
            {
                using TcpClient client = new();
                client.SendTimeout = timeoutMS;
                client.ReceiveTimeout = timeoutMS;
                client.Client.NoDelay = true;
                await client.Client.ConnectAsync(host, port).ConfigureAwait(false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        });
        
        try { return await task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMS + 100)); } catch (Exception) { return false; }
    }

    public static async Task<bool> CanUdpConnect(IPAddress ip, int port, int timeoutMS)
    {
        var task = Task.Run(async () =>
        {
            try
            {
                IPEndPoint ep = new(ip, port);
                using UdpClient client = new();
                await client.Client.ConnectAsync(ep).ConfigureAwait(false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        });

        try { return await task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMS + 100)); } catch (Exception) { return false; }
    }

    public static async Task<bool> CanUdpConnect(string host, int port, int timeoutMS)
    {
        var task = Task.Run(async () =>
        {
            try
            {
                using UdpClient client = new();
                await client.Client.ConnectAsync(host, port).ConfigureAwait(false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        });

        try { return await task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMS + 100)); } catch (Exception) { return false; }
    }

    public static async Task<bool> CanConnect(string host, int port, int timeoutMS)
    {
        var task = Task.Run(async () =>
        {
            try
            {
                string url = $"https://{host}:{port}";
                Uri uri = new(url, UriKind.Absolute);

                using HttpClient httpClient = new();
                httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMS);

                await httpClient.GetAsync(uri);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        });

        try { return await task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMS + 100)); } catch (Exception) { return false; }
    }

}