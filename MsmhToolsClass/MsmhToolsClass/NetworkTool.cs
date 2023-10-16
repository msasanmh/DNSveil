using Microsoft.Diagnostics.Tracing.Parsers.JSDumpHeap;
using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace MsmhToolsClass;

public static class NetworkTool
{
    /// <summary>
    /// IP to Host using Nslookup (Windows Only)
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    public static string IpToHost(string ip, out string baseHost)
    {
        string result = string.Empty;
        baseHost = string.Empty;
        if (!OperatingSystem.IsWindows()) return result;
        string content = ProcessManager.Execute(out _, "nslookup", ip, true, true);
        if (string.IsNullOrEmpty(content)) return result;
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
                    GetHostDetails(result, 0, out _, out baseHost, out _, out _, out _);
                }
                break;
            }
        }

        return result;
    }

    public static async Task<string> GetHeaders(string host, string? ip, string? proxyScheme, int timeoutSec, CancellationToken ct)
    {
        string result = string.Empty;
        host = host.Trim();
        string url = $"https://{host}";
        if (!string.IsNullOrEmpty(ip))
        {
            ip = ip.Trim();
            url = $"https://{ip}";
        }
        Uri uri = new(url, UriKind.Absolute);

        using HttpClientHandler handler = new();
        handler.AllowAutoRedirect = true;
        if (string.IsNullOrEmpty(proxyScheme))
            handler.UseProxy = false;
        else
        {
            proxyScheme = proxyScheme.Trim();
            handler.Proxy = new WebProxy(proxyScheme, true);
        }
        
        // Ignore Cert Check To Make It Faster
        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true;

        using HttpClient httpClient = new(handler);
        httpClient.Timeout = TimeSpan.FromSeconds(timeoutSec);

        // Get Only Header
        using HttpRequestMessage message = new(HttpMethod.Get, uri); // "Head" returns Forbidden for some websites
        message.Headers.TryAddWithoutValidation("User-Agent", "Other");
        message.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
        message.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
        message.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");
        
        if (!string.IsNullOrEmpty(ip))
        {
            message.Headers.TryAddWithoutValidation("host", host);
        }

        HttpResponseMessage? response = null;

        try
        {
            response = await httpClient.SendAsync(message, ct);
        }
        catch (Exception)
        {
            // do nothing
        }

        if (response != null)
        {
            result += response.StatusCode.ToString() + Environment.NewLine;
            result += response.Headers.ToString();
            response.Dispose();
        }

        return result;
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
            await ProcessManager.ExecuteAsync("net", "stop winnat", true, true);
            await ProcessManager.ExecuteAsync("net", "start winnat", true, true);
        }
        catch (Exception)
        {
            // do nothing
        }
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

    public static void GetUrlDetails(string url, int defaultPort, out string scheme, out string host, out string baseHost, out int port, out string path, out bool isIPv6)
    {
        url = url.Trim();
        scheme = string.Empty;

        // Strip xxxx://
        if (url.Contains("//"))
        {
            string[] split = url.Split("//");

            if (!string.IsNullOrEmpty(split[0]))
                scheme = $"{split[0]}//";

            if (!string.IsNullOrEmpty(split[1]))
                url = split[1];
        }

        GetHostDetails(url, defaultPort, out host, out baseHost, out port, out path, out isIPv6);
    }

    public static void GetHostDetails(string hostIpPort, int defaultPort, out string host, out string baseHost, out int port, out string path, out bool isIPv6)
    {
        hostIpPort = hostIpPort.Trim();
        baseHost = string.Empty;
        path = string.Empty;
        isIPv6 = false;

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
                if (n != 0)
                    outPath += split[n] + "/";
            }
            if (outPath.Length > 1 && outPath.EndsWith("/")) outPath = outPath.TrimEnd(slash.ToCharArray());
            if (!outPath.Equals("/")) path = outPath;
        }

        string host0 = hostIpPort;
        port = defaultPort;

        // Split Host and Port
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

        host = host0;

        // Get Base Host
        if (!IsIp(host, out _) && host.Contains('.'))
        {
            string[] dotSplit = host.Split('.');
            for (int i = 0; i < dotSplit.Length; i++)
            {
                if (i >= dotSplit.Length - 2)
                    baseHost += $"{dotSplit[i]}.";
            }
            if (baseHost.EndsWith('.')) baseHost = baseHost[..^1];
        }
    }

    public static bool IsLocalIP(string ipv4)
    {
        string ip = ipv4.Trim();
        return ip.ToLower().Equals("localhost") || ip.Equals("0.0.0.0") || ip.StartsWith("10.") || ip.StartsWith("127.") || ip.StartsWith("192.168.") ||
               ip.StartsWith("172.16.") || ip.StartsWith("172.17.") || ip.StartsWith("172.18.") || ip.StartsWith("172.19.") ||
               ip.StartsWith("172.20.") || ip.StartsWith("172.21.") || ip.StartsWith("172.22.") || ip.StartsWith("172.23.") ||
               ip.StartsWith("172.24.") || ip.StartsWith("172.25.") || ip.StartsWith("172.26.") || ip.StartsWith("172.27.") ||
               ip.StartsWith("172.28.") || ip.StartsWith("172.29.") || ip.StartsWith("172.30.") || ip.StartsWith("172.31.");
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

    public static bool IsIp(string ipStr, out IPAddress? ip)
    {
        return IPAddress.TryParse(ipStr, out ip);
    }

    public static bool IsIPv4(IPAddress iPAddress)
    {
        return iPAddress.AddressFamily == AddressFamily.InterNetwork;
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
        return iPAddress.AddressFamily == AddressFamily.InterNetworkV6;
    }

    /// <summary>
    /// Windows Only
    /// </summary>
    /// <param name="ipStr">Ipv4 Or Ipv6</param>
    /// <returns></returns>
    public static bool IsIpProtocolReachable(string ipStr)
    {
        if (!OperatingSystem.IsWindows()) return true;
        string args = $"-n 2 {ipStr}";
        string content = ProcessManager.Execute(out _, "ping", args, true, false);
        return !content.Contains("transmit failed") && !content.Contains("General failure");
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
        catch (Exception)
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
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        for (int n = 0; n < networkInterfaces.Length; n++)
        {
            NetworkInterface nic = networkInterfaces[n];
            if (nic.Name.Equals(name)) return nic;
        }
        return null;
    }

    public static NetworkInterface? GetNICByDescription(string description)
    {
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        for (int n = 0; n < networkInterfaces.Length; n++)
        {
            NetworkInterface nic = networkInterfaces[n];
            if (nic.Description.Equals(description)) return nic;
        }
        return null;
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
        // Only netsh can set DNS on Windows 7
        if (nic == null) return;

        try
        {
            string dnsServer1 = dnsServers;
            string dnsServer2 = string.Empty;
            if (dnsServers.Contains(','))
            {
                string[] split = dnsServers.Split(',');
                dnsServer1 = split[0];
                dnsServer2 = split[1];
            }

            string processName = "netsh";
            string processArgs1 = $"interface ipv4 delete dnsservers {nic.Name} all";
            string processArgs2 = $"interface ipv4 set dnsservers {nic.Name} static {dnsServer1} primary";
            string processArgs3 = $"interface ipv4 add dnsservers {nic.Name} {dnsServer2} index=2";
            await ProcessManager.ExecuteAsync(processName, processArgs1, true, true);
            await ProcessManager.ExecuteAsync(processName, processArgs2, true, true);
            if (!string.IsNullOrEmpty(dnsServer2))
                await ProcessManager.ExecuteAsync(processName, processArgs3, true, true);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }

        Task.Delay(200).Wait();

        try
        {
            using ManagementClass managementClass = new("Win32_NetworkAdapterConfiguration");
            using ManagementObjectCollection moc = managementClass.GetInstances();
            foreach (ManagementObject mo in moc.Cast<ManagementObject>())
            {
                if ((bool)mo["IPEnabled"] && mo["Description"].Equals(nic.Description))
                {
                    using ManagementBaseObject newDNS = mo.GetMethodParameters("SetDNSServerSearchOrder");
                    if (newDNS != null)
                    {
                        newDNS["DNSServerSearchOrder"] = dnsServers.Split(',');
                        mo.InvokeMethod("SetDNSServerSearchOrder", newDNS, new InvokeMethodOptions());
                    }
                }
            }
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
        // NetSh Command: netsh interface ip set dns "nic.Name" source=dhcp
        if (nic == null) return;

        try
        {
            string processName = "netsh";
            string processArgs1 = $"interface ipv4 delete dnsservers {nic.Name} all";
            string processArgs2 = $"interface ipv4 set dnsservers {nic.Name} source=dhcp";
            await ProcessManager.ExecuteAsync(processName, processArgs1, true, true);
            await ProcessManager.ExecuteAsync(processName, processArgs2, true, true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

        try
        {
            using ManagementClass managementClass = new("Win32_NetworkAdapterConfiguration");
            using ManagementObjectCollection moc = managementClass.GetInstances();
            foreach (ManagementObject mo in moc.Cast<ManagementObject>())
            {
                if (mo["Description"].Equals(nic.Description))
                {
                    using ManagementBaseObject newDNS = mo.GetMethodParameters("SetDNSServerSearchOrder");
                    if (newDNS != null)
                    {
                        newDNS["DNSServerSearchOrder"] = null;
                        mo.InvokeMethod("SetDNSServerSearchOrder", newDNS, new InvokeMethodOptions());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Unset IPv4 DNS by seting DNS to Static
    /// </summary>
    /// <param name="nic">Network Interface</param>
    /// <param name="dns1">Primary</param>
    /// <param name="dns2">Secondary</param>
    public static async Task UnsetDnsIPv4(NetworkInterface nic, string dns1, string dns2)
    {
        string dnsServers = $"{dns1},{dns2}";
        await SetDnsIPv4(nic, dnsServers);
    }

    /// <summary>
    /// Unset IPv4 DNS to DHCP (Windows Only)
    /// </summary>
    /// <param name="nic">Network Interface</param>
    public static async Task UnsetDnsIPv6(NetworkInterface nic)
    {
        if (!OperatingSystem.IsWindows()) return;
        // Requires Elevation - Can't Unset DNS when there is no Internet connectivity but netsh can :)
        // NetSh Command: netsh interface ip set dns "nic.Name" source=dhcp
        if (nic == null) return;

        try
        {
            string processName = "netsh";
            string processArgs1 = $"interface ipv6 delete dnsservers {nic.Name} all";
            string processArgs2 = $"interface ipv6 set dnsservers {nic.Name} source=dhcp";
            await ProcessManager.ExecuteAsync(processName, processArgs1, true, true);
            await ProcessManager.ExecuteAsync(processName, processArgs2, true, true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Is DNS Set to 127.0.0.1 - Using Nslookup (Windows Only)
    /// </summary>
    public static bool IsDnsSetToLocal(out string host, out string ip)
    {
        bool result = false;
        host = ip = string.Empty;
        if (!OperatingSystem.IsWindows()) return result;
        string content = ProcessManager.Execute(out _, "nslookup", "0.0.0.0", true, true);
        if (string.IsNullOrEmpty(content)) return result;
        content = content.ToLower();
        string[] split = content.Split(Environment.NewLine);
        for (int n = 0; n < split.Length; n++)
        {
            string line = split[n];
            if (line.Contains("server:"))
            {
                line = line.Replace("server:", string.Empty).Trim();
                host = line;
                if (line.Equals("localhost")) result = true;
            }
            else if (line.Contains("address:"))
            {
                line = line.Replace("address:", string.Empty).Trim();
                ip = line;
            }
        }
        return result;
    }

    /// <summary>
    /// Check if DNS is set to Static or DHCP (Windows Only)
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
        string stdout = ProcessManager.Execute(out Process _, processName, processArgs, true, false);

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

            return attempt1 || attempt2(url, timeoutMs);
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
                    { Name: string n } when n.StartsWith("fa") => // Iran
                        "http://www.google.com",
                    { Name: string n } when n.StartsWith("zh") => // China
                        "http://www.baidu.com",
                    _ =>
                        "http://www.gstatic.com/generate_204",
                };

                using HttpClient httpClient = new();
                httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMs);
                var req = httpClient.GetAsync(url);
                return req.Result.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("IsInternetAlive: " + ex.Message);
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
                { Name: string n } when n.StartsWith("fa") => // Iran
                    "http://www.google.com",
                { Name: string n } when n.StartsWith("zh") => // China
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

        }
        return isProxyEnable;
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
                    registry.SetValue("ProxyEnable", 1, RegistryValueKind.DWord);
                    registry.SetValue("ProxyServer", proxyServer, RegistryValueKind.String);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Set Http Proxy: {ex.Message}");
            }

            RegistryTool.ApplyRegistryChanges();
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
                registry.SetValue("ProxyEnable", 0, RegistryValueKind.DWord);
                if (clearIpPort)
                    registry.SetValue("ProxyServer", "", RegistryValueKind.String);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unset Proxy: {ex.Message}");
            }

            if (applyRegistryChanges)
                RegistryTool.ApplyRegistryChanges();
        }
    }

    /// <summary>
    /// Only the 'http', 'socks4', 'socks4a' and 'socks5' schemes are allowed for proxies.
    /// </summary>
    /// <returns></returns>
    public static async Task<bool> CheckProxyWorks(string websiteToCheck, string proxyScheme, int timeoutSec)
    {
        try
        {
            Uri uri = new(websiteToCheck, UriKind.Absolute);

            using SocketsHttpHandler socketsHttpHandler = new();
            socketsHttpHandler.Proxy = new WebProxy(proxyScheme);
            using HttpClient httpClientWithProxy = new(socketsHttpHandler);
            httpClientWithProxy.Timeout = new TimeSpan(0, 0, timeoutSec);

            HttpResponseMessage checkingResponse = await httpClientWithProxy.GetAsync(uri);
            Task.Delay(200).Wait();

            return checkingResponse.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Check Proxy: {ex.Message}");
            return false;
        }
    }

    public static async Task<bool> IsHostBlocked(string host, int port, int timeoutMS)
    {
        try
        {
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromMilliseconds(timeoutMS);
            if (port == 80)
                await client.GetAsync(new Uri($"http://{host}:{port}"));
            else
                await client.GetAsync(new Uri($"https://{host}:{port}"));
            return false;
        }
        catch (Exception)
        {
            return true;
        }
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
                bool isIp = IsIp(host, out IPAddress? ip);
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

        try { await task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMS + 500)); } catch (Exception) { }
        return task.Result;
    }

    public static async Task<bool> CanTcpConnect(string host, int port, int timeoutMS)
    {
        var task = Task.Run(() =>
        {
            try
            {
                using TcpClient client = new(host, port);
                client.SendTimeout = timeoutMS;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        });

        try { await task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMS + 500)); } catch (Exception) { }
        return task.Result;
    }

    public static async Task<bool> CanUdpConnect(string host, int port, int timeoutMS)
    {
        var task = Task.Run(() =>
        {
            try
            {
                using UdpClient client = new(host, port);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        });

        try { await task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMS + 500)); } catch (Exception) { }
        return task.Result;
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

        try { await task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMS + 500)); } catch (Exception) { }
        return task.Result;
    }

}