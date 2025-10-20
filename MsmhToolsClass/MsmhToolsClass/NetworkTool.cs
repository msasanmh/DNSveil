using Microsoft.Win32;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Management.Automation;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace MsmhToolsClass;

public static class NetworkTool
{
    public static string IpToUrl(string scheme, IPAddress ip, int port, string path)
    {
        string url = string.Empty;
        
        try
        {
            scheme = scheme.Trim();
            if (!string.IsNullOrEmpty(scheme) && !scheme.EndsWith("://")) scheme = $"{scheme}://";
            if (scheme.Equals("://")) scheme = string.Empty;

            bool isIPv6 = IsIPv6(ip);
            string ipStr = isIPv6 ? $"[{ip.ToStringNoScopeId()}]" : ip.ToString();

            port = port > 0 && port <= 65535 ? port : 443;

            path = path.Trim().TrimEnd('/');
            if (!string.IsNullOrEmpty(path) && !path.StartsWith('/')) path = $"/{path}";
            if (path.Equals('/')) path = string.Empty;

            url = $"{scheme}{ipStr}:{port}{path}";
        }
        catch (Exception ex)
        {
            Debug.WriteLine("NetworkTool IpToUrl: " + ex.Message);
        }

        return url;
    }

    /// <summary>
    /// Int To IPv4
    /// </summary>
    /// <param name="ipInt"></param>
    /// <returns>Returns IPv4, If Fail: IPAddress.None</returns>
    public static IPAddress IntToIPv4(int ipInt)
    {
        IPAddress ip = IPAddress.None;

        try
        {
            string ipStr = $"{(ipInt & 0xFF)}.{(ipInt >> 8 & 0xFF)}.{(ipInt >> 16 & 0xFF)}.{(ipInt >> 24 & 0xFF)}";
            bool isValid = NetworkTool.IsIPv4Valid(ipStr, out IPAddress? ipOut);
            if (isValid && ipOut != null) ip = ipOut;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("IntToIPv4: " + ex.Message);
        }

        return ip;
    }

    public static bool IsIPv4SupportedByOS()
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

    public static bool IsIPv6SupportedByOS()
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

    /// <summary>
    /// Is IP Protocol Supported By ISP (By Ping)
    /// </summary>
    /// <param name="ipStr">Ipv4 Or Ipv6</param>
    /// <returns></returns>
    public static async Task<bool> IsIpProtocolSupportedByISPAsync(string ipStr, int timeoutMS)
    {
        return await CanPingAsync(ipStr, timeoutMS);
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

        try
        {
            if (!OperatingSystem.IsWindows()) return (result, baseHost);
            if (!await IsInternetAliveByNicAsync()) return (result, baseHost); // nslookup takes time when there is no internet access

            var p = await ProcessManager.ExecuteAsync("nslookup", null, ip, true, true);
            if (!p.IsSeccess) return (result, baseHost);
            string content = p.Output.ToLower();
            string[] split = content.Split(Environment.NewLine);
            for (int n = 0; n < split.Length; n++)
            {
                string line = split[n];
                if (line.Contains("name:"))
                {
                    result = line.Replace("name:", string.Empty).Trim();
                    if (result.Contains('.'))
                    {
                        URL urid = GetUrlOrDomainDetails(result, 0);
                        baseHost = urid.BaseHost;
                    }
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("NetworkTool IpToHostAsync: " + ex.Message);
        }

        return (result, baseHost);
    }

    /// <summary>
    /// Restart NAT Driver - Windows Only
    /// </summary>
    /// <returns></returns>
    public static async Task RestartNATDriverAsync()
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

    public class URL
    {
        public Uri? Uri { get; set; } = null;
        public string Scheme { get; set; } = string.Empty;
        public string SchemeName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public string SubHost { get; set; } = string.Empty;
        public string BaseHost { get; set; } = string.Empty;
        public string DnsSafeHost { get; set; } = string.Empty;
        public string IdnHost { get; set; } = string.Empty;
        public bool IsHostLoopback { get; set; } = false;
        public int Port { get; set; } = -1;
        public string Path { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public string Fragment { get; set; } = string.Empty;
        public UriHostNameType HostNameType { get; set; } = UriHostNameType.Unknown;

        public override string ToString()
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(Scheme)) result += Scheme;
            if (!string.IsNullOrEmpty(Username))
            {
                result += Username;
                if (!string.IsNullOrEmpty(Password)) result += $":{Password}";
                result += "@";
            }
            else
            {
                if (!string.IsNullOrEmpty(Password))
                {
                    result += $":{Password}";
                    result += "@";
                }
            }
            if (!string.IsNullOrEmpty(Host))
            {
                result += Host;
                if (Port != -1 && Port != 0) result += $":{Port}";
            }
            if (!string.IsNullOrEmpty(Path)) result += Path;
            if (!string.IsNullOrEmpty(Query)) result += Query;
            if (!string.IsNullOrEmpty(Fragment)) result += Fragment;

            return WebUtility.UrlDecode(result);
        }

        public string ToStringDebug()
        {
            string nl = Environment.NewLine;
            string result = string.Empty;
            result += $"{nameof(Scheme)}: {Scheme}{nl}";
            result += $"{nameof(SchemeName)}: {SchemeName}{nl}";
            result += $"{nameof(Username)}: {Username}{nl}";
            result += $"{nameof(Password)}: {Password}{nl}";
            result += $"{nameof(Host)}: {Host}{nl}";
            result += $"{nameof(SubHost)}: {SubHost}{nl}";
            result += $"{nameof(BaseHost)}: {BaseHost}{nl}";
            result += $"{nameof(DnsSafeHost)}: {DnsSafeHost}{nl}";
            result += $"{nameof(IdnHost)}: {IdnHost}{nl}";
            result += $"{nameof(IsHostLoopback)}: {IsHostLoopback}{nl}";
            result += $"{nameof(Port)}: {Port}{nl}";
            result += $"{nameof(Path)}: {Path}{nl}";
            result += $"{nameof(Query)}: {Query}{nl}";
            result += $"{nameof(Fragment)}: {Fragment}{nl}";
            result += $"{nameof(HostNameType)}: {HostNameType}{nl}";
            return WebUtility.UrlDecode(result);
        }
    }

    public static URL GetUrlOrDomainDetails(string urlOrDomain, int defaultPort)
    {
        URL url = new();

        try
        {
            urlOrDomain = urlOrDomain.Trim();
            urlOrDomain = WebUtility.UrlDecode(urlOrDomain);
            url.Host = urlOrDomain;
            url.Port = defaultPort;

            string scheme = string.Empty;

            // Strip xxxx://
            string separator_Scheme = "://";
            int indexOfScheme = urlOrDomain.IndexOf(separator_Scheme);
            if (indexOfScheme != -1)
            {
                indexOfScheme += separator_Scheme.Length;
                scheme = urlOrDomain[..indexOfScheme];
                urlOrDomain = urlOrDomain[indexOfScheme..];

                url.Scheme = scheme;
                url.SchemeName = scheme[..(indexOfScheme - separator_Scheme.Length)];
            }

            // Has Domain
            bool hasDomain = true;

            // Get Port
            string tempGetPort = urlOrDomain.Trim();
            int pathIndex = tempGetPort.IndexOf('/');
            if (pathIndex != -1)
            {
                // Strip Path
                tempGetPort = tempGetPort[..pathIndex];
                hasDomain = !string.IsNullOrEmpty(tempGetPort);
            }
            int portIndex = tempGetPort.LastIndexOf(':'); // LastIndexOf (Domain May Be IPv6)
            if (portIndex != -1)
            {
                string domainOnly = tempGetPort[..portIndex];
                hasDomain = !string.IsNullOrEmpty(domainOnly);

                string portStr = tempGetPort[(portIndex + 1)..];
                bool isPortInt = int.TryParse(portStr, out int portOut);
                if (isPortInt)
                {
                    defaultPort = portOut;
                    url.Port = defaultPort;
                }
            }
            
            urlOrDomain = urlOrDomain.Trim();
            if (string.IsNullOrEmpty(urlOrDomain)) return url;

            // e.g. DoH Get Would Be Like: /dns-query?dns=AAABAAABAAAAAAAABXlhaG9vA2NvbQAAAQAB
            string readURI = hasDomain ? $"https://{urlOrDomain}" : urlOrDomain.StartsWith('/') ? $"https://{IPAddress.Loopback}{urlOrDomain}" : $"https://{IPAddress.Loopback}/{urlOrDomain}";
            
            Uri? uri = null;
            try { uri = new(readURI, UriKind.Absolute); }
            catch (Exception ex)
            {
                Debug.WriteLine("NetworkTool GetUrlOrDomainDetails Domain ==> " + urlOrDomain);
                Debug.WriteLine(ex.Message);
            }
            url.Uri = uri;
            
            if (uri != null)
            {
                // Get Username And Password
                string userInfo = uri.UserInfo;
                url.Username = userInfo;
                char separator_Username = ':';
                int indexOfUsername = userInfo.IndexOf(separator_Username);
                if (indexOfUsername != -1)
                {
                    url.Username = userInfo[..indexOfUsername];
                    url.Password = userInfo[(indexOfUsername + 1)..];
                }

                // Get Host
                string host = uri.Host;
                int indexOfHost = urlOrDomain.IndexOf(host, StringComparison.OrdinalIgnoreCase);
                if (indexOfHost != -1)
                {
                    host = urlOrDomain[indexOfHost..(indexOfHost + host.Length)]; // To Get Mixed Case Host
                }
                url.Host = host;

                bool isIP = IsIP(host, out _);

                // Get SubHost And BaseHost
                if (!isIP)
                {
                    url.SubHost = host;
                    url.BaseHost = host;

                    if (host.Contains('.'))
                    {
                        // Get Base Host
                        string baseHost = host;
                        string[] dotSplit = host.Split('.');
                        int realLength = dotSplit.Length;
                        if (realLength >= 3)
                        {
                            // e.g. *.co.uk, *.org.us
                            if (dotSplit[^2].Length <= 3 && dotSplit[^1].Length <= 2) realLength--;

                            if (realLength >= 3)
                            {
                                if (realLength == 3 && dotSplit[0].Equals("www", StringComparison.OrdinalIgnoreCase))
                                    baseHost = baseHost[4..];
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
                        url.BaseHost = baseHost;

                        // Get Sub Host (Subdomain)
                        if (!baseHost.Equals(host))
                        {
                            string baseHostWithDot = $".{baseHost}";
                            if (host.Contains(baseHostWithDot))
                                url.SubHost = host.Replace(baseHostWithDot, string.Empty);
                        }
                    }
                }

                url.DnsSafeHost = uri.DnsSafeHost; // Get DnsSafeHost
                url.IdnHost = uri.IdnHost; // Get IdnHost
                url.IsHostLoopback = uri.IsLoopback || host.Equals("localhost", StringComparison.OrdinalIgnoreCase); // Get IsHostLoopback
                url.Port = uri.Port == 443 ? defaultPort : uri.Port; // Get Port
                url.Path = uri.AbsolutePath; // Get Path
                url.Query = uri.Query; // Get Query
                url.Fragment = uri.Fragment; // Get Fragment

                // Get HostType
                if (isIP)
                {
                    if (uri.HostNameType == UriHostNameType.IPv4 || uri.HostNameType == UriHostNameType.IPv6)
                        url.HostNameType = uri.HostNameType;
                }
                else
                {
                    if (uri.HostNameType != UriHostNameType.IPv4 && uri.HostNameType != UriHostNameType.IPv6)
                        url.HostNameType = uri.HostNameType;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("NetworkTool GetUrlOrDomainDetails: " + ex.Message);
        }
        
        return url;
    }

    public static void GetUrlDetails_OLD(string url, int defaultPort, out string scheme, out string host, out string subHost, out string baseHost, out int port, out string path, out string query, out string fragment, out bool isIPv6)
    {
        url = url.Trim();
        scheme = string.Empty;
        
        try
        {
            url = WebUtility.UrlDecode(url);

            // Strip xxxx://
            string separator_Scheme = "://";
            int indexOfScheme = url.IndexOf(separator_Scheme);
            if (indexOfScheme != -1)
            {
                indexOfScheme += separator_Scheme.Length;
                scheme = url[..indexOfScheme];
                url = url[indexOfScheme..];
            }
        }
        catch (Exception) { }

        GetHostDetails_OLD(url, defaultPort, out host, out subHost, out baseHost, out port, out path, out query, out fragment, out isIPv6);

    }

    private static void GetHostDetails_OLD(string hostIpPort, int defaultPort, out string host, out string subHost, out string baseHost, out int port, out string path, out string query, out string fragment, out bool isIPv6)
    {
        hostIpPort = hostIpPort.Trim();
        host = hostIpPort;
        subHost = string.Empty;
        baseHost = host;
        port = defaultPort;
        path = string.Empty;
        query = string.Empty;
        fragment = string.Empty;
        isIPv6 = false;

        try
        {
            // Strip /xxxx (Path) Or ?xx= (Query)
            if (!hostIpPort.Contains("//") && (hostIpPort.Contains('/') || hostIpPort.Contains('?') || hostIpPort.Contains('#')))
            {
                // Set Fragment Portion (#...)
                int indexOfFragment = hostIpPort.LastIndexOf('#');
                if (indexOfFragment != -1)
                {
                    fragment = hostIpPort[indexOfFragment..];
                    hostIpPort = hostIpPort[..indexOfFragment]; // Remove Fragment From hostIpPort
                }

                // Set Query
                int indexOfQuestion = hostIpPort.IndexOf('?');
                if (indexOfQuestion != -1) query = hostIpPort[indexOfQuestion..];

                // Set Path
                int indexOfSlash = hostIpPort.IndexOf('/');
                if (indexOfSlash != -1)
                {
                    if (indexOfQuestion != -1)
                    {
                        if (indexOfQuestion > indexOfSlash)
                        {
                            path = hostIpPort[indexOfSlash..indexOfQuestion];
                        }
                    }
                    else
                    {
                        path = hostIpPort[indexOfSlash..];
                    }
                }

                // Strip Path And Query
                int index = indexOfSlash;
                if (index == -1) index = indexOfQuestion;
                else if (indexOfQuestion != -1) index = Math.Min(indexOfSlash, indexOfQuestion);
                if (index != -1) hostIpPort = hostIpPort[..index];
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
            Debug.WriteLine("NetworkTool GetHostDetails: " + ex.Message);
        }
    }

    public static NameValueCollection ParseUriQuery(string query, bool keysToLower = true)
    {
        NameValueCollection nvc = new(StringComparer.OrdinalIgnoreCase);

        try
        {
            if (string.IsNullOrEmpty(query)) return nvc;
            if (query.StartsWith('?')) query = query[1..];
            string[] split = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
            for (int n = 0; n < split.Length; n++)
            {
                string part = split[n];
                int index = part.IndexOf('=');
                if (index == -1) continue;
                string keyStr = part[..index];
                string valStr = part[(index + 1)..];

                if (string.IsNullOrEmpty(keyStr)) continue;
                if (string.IsNullOrEmpty(valStr)) continue;

                string key = Uri.UnescapeDataString(keyStr);
                if (keysToLower) key = key.ToLower();
                string val = Uri.UnescapeDataString(valStr);

                // Add If Not Already Exist
                if (nvc[key] is null) nvc.Add(key, val);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("NetworkTool ParseUriQuery: " + ex.Message);
        }

        return nvc;
    }

    public static Uri? UrlToUri(string url)
    {
        try
        {
            return new Uri(url.Trim(), UriKind.Absolute);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("NetworkTool UrlToUri: " + ex.Message);
        }
        return null;
    }

    public static bool IsUrlValid(string url)
    {
        return UrlToUri(url) != null;
    }

    /// <summary>
    /// Domain, IPv4, IPv6
    /// </summary>
    public static bool IsDomainNameValid(string domain)
    {
        try
        {
            if (string.IsNullOrEmpty(domain)) return false;
            if (IsIP(domain, out _)) return true; // Return True If It's An IP
            if (domain.StartsWith("http:", StringComparison.OrdinalIgnoreCase)) return false;
            if (domain.StartsWith("https:", StringComparison.OrdinalIgnoreCase)) return false;
            if (domain.Contains('/', StringComparison.OrdinalIgnoreCase)) return false;
            if (!domain.Contains('.', StringComparison.OrdinalIgnoreCase)) return false;
            return Uri.CheckHostName(domain) != UriHostNameType.Unknown;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("NetworkTool IsDomainNameValid: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Only Domain. e.g. example.com
    /// </summary>
    public static bool IsDomain(string domain)
    {
        try
        {
            if (string.IsNullOrEmpty(domain)) return false;
            if (IsIP(domain, out _)) return false;
            if (domain.StartsWith("http:", StringComparison.OrdinalIgnoreCase)) return false;
            if (domain.StartsWith("https:", StringComparison.OrdinalIgnoreCase)) return false;
            if (domain.Contains('/', StringComparison.OrdinalIgnoreCase)) return false;
            if (!domain.Contains('.', StringComparison.OrdinalIgnoreCase)) return false;
            return Uri.CheckHostName(domain) != UriHostNameType.Unknown;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("NetworkTool IsDomain: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// IsLocalIP
    /// </summary>
    /// <param name="ip">IPv4 Or IPv6</param>
    public static bool IsLocalIP(IPAddress ip)
    {
        try
        {
            if (IPAddress.IsLoopback(ip)) return true;
            if (ip.IsIPv6LinkLocal) return true;
            if (ip.IsIPv6SiteLocal) return true;
            if (ip.IsIPv6UniqueLocal) return true;
            if (ip.Equals(IPAddress.None)) return true;
            if (ip.Equals(IPAddress.IPv6None)) return true;
            if (ip.Equals(IPAddress.Any)) return true;
            if (ip.Equals(IPAddress.IPv6Any)) return true;
            if (ip.Equals(IPAddress.Loopback)) return true;
            if (ip.Equals(IPAddress.IPv6Loopback)) return true;

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
                "1::/127",
                "fc00::/7",
                "fe80::/10",
                "ff00::/8"
            };

            for (int n = 0; n < localCIDRs.Count; n++)
            {
                string cidr = localCIDRs[n].Trim();
                bool isInRange = IsIpInRange(ip, cidr);
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
    /// IsLocalIP
    /// </summary>
    /// <param name="ipStr">IPv4 Or IPv6</param>
    public static bool IsLocalIP(string ipStr)
    {
        bool isIP = IsIP(ipStr, out IPAddress? ip);
        if (isIP && ip != null) return IsLocalIP(ip);
        return false;
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
            Debug.WriteLine("NetworkTool IpToCompanyAsync: " + ex.Message);
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
            Debug.WriteLine("NetworkTool GetLocalIPv4: " + ex.Message);
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
            Debug.WriteLine("NetworkTool GetLocalIPv6: " + ex.Message);
            return null;
        }
    }

    public static IPAddress? GetLocalIP(IPAddress dnsIP, int dnsPort)
    {
        try
        {
            bool isIPv6 = IsIPv6(dnsIP);
            return isIPv6 ? GetLocalIPv6(dnsIP.ToStringNoScopeId(), dnsPort) : GetLocalIPv4(dnsIP.ToString(), dnsPort);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("NetworkTool GetLocalIP: " + ex.Message);
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
                                Debug.WriteLine("NetworkTool GetDefaultGateway: " + gateway);
                                return gateway;
                            }
                        }
                        else
                        {
                            if (address.AddressFamily == AddressFamily.InterNetworkV6)
                            {
                                gateway = address;
                                Debug.WriteLine("NetworkTool GetDefaultGateway: " + gateway);
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

    public static bool IsEndPoint(string endPointStr, out IPEndPoint? endPoint)
    {
        endPoint = null;

        try
        {
            if (!string.IsNullOrEmpty(endPointStr))
            {
                endPointStr = endPointStr.Trim();
                // IPEndPoint.TryParse() Is Not Reliable
                if (endPointStr.Count(c => c == '.') == 3) // IPv4
                {
                    string ipv4Str = endPointStr;
                    int port = 0;

                    if (ipv4Str.Contains(':'))
                    {
                        string[] split = ipv4Str.Split(':');
                        if (split.Length == 2)
                        {
                            ipv4Str = split[0];
                            string portStr = split[1];
                            bool isPortInt = int.TryParse(portStr, out int p);
                            if (isPortInt) port = p;
                        }
                    }

                    if (IsIPv4Valid(ipv4Str, out IPAddress? ip) && ip != null)
                    {
                        endPoint = new(ip, port);
                        return true;
                    }
                }
                else
                {
                    int numberOfColons = endPointStr.Count(c => c == ':');
                    if (numberOfColons >= 2 && numberOfColons <= 7)
                    {
                        if (endPointStr.StartsWith('[') && endPointStr.Contains(']'))
                        {
                            int lastIndex1 = endPointStr.LastIndexOf(']');
                            int lastIndex2 = endPointStr.LastIndexOf(':');
                            if (lastIndex2 == lastIndex1 + 1) // [IPv6]:Port
                            {
                                string ipv6Str = endPointStr;
                                int port = 0;

                                if (ipv6Str.Contains("]:"))
                                {
                                    string[] split = ipv6Str.Split("]:");
                                    if (split.Length == 2)
                                    {
                                        ipv6Str = split[0];
                                        ipv6Str = ipv6Str.TrimStart('[');
                                        string portStr = split[1];
                                        bool isPortInt = int.TryParse(portStr, out int p);
                                        if (isPortInt) port = p;
                                    }
                                }

                                if (IsIPv6Valid(ipv6Str, out IPAddress? ip) && ip != null)
                                {
                                    endPoint = new(ip, port);
                                    return true;
                                }
                            }
                            else
                            {
                                if (endPointStr.EndsWith(']')) // [IPv6]
                                {
                                    string ipv6Str = endPointStr;
                                    int port = 0;

                                    if (IsIPv6Valid(ipv6Str, out IPAddress? ip) && ip != null)
                                    {
                                        endPoint = new(ip, port);
                                        return true;
                                    }
                                }
                            }
                        }
                        else // IPv6
                        {
                            string ipv6Str = endPointStr;
                            int port = 0;

                            if (IsIPv6Valid(ipv6Str, out IPAddress? ip) && ip != null)
                            {
                                endPoint = new(ip, port);
                                return true;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception) { }

        return false;
    }

    public static bool IsIP(string ipStr, out IPAddress? ip)
    {
        ip = null;

        try
        {
            if (!string.IsNullOrEmpty(ipStr))
            {
                bool isIP = IPAddress.TryParse(ipStr, out IPAddress? ipOut);
                if (isIP && ipOut != null)
                {
                    int byteLength = ipOut.GetAddressBytes().Length;
                    bool isIPv4 = ipOut.AddressFamily == AddressFamily.InterNetwork && byteLength == 4;
                    bool isIPv6 = ipOut.AddressFamily == AddressFamily.InterNetworkV6 && byteLength == 16;
                    if (isIPv4 || isIPv6)
                    {
                        ip = ipOut;
                        return true;
                    }
                }
            }
        }
        catch (Exception) { }

        return false;
    }

    public static bool IsIPv4(IPAddress iPAddress)
    {
        try
        {
            int byteLength = iPAddress.GetAddressBytes().Length;
            return iPAddress.AddressFamily == AddressFamily.InterNetwork && byteLength == 4;
        }
        catch (Exception) { }
        return false;
    }

    public static bool IsIPv4Valid(string ipv4Str, out IPAddress? ipv4)
    {
        ipv4 = null;

        try
        {
            if (string.IsNullOrWhiteSpace(ipv4Str)) return false;
            if (!ipv4Str.Contains('.')) return false;
            if (ipv4Str.Count(c => c == '.') != 3) return false;
            if (ipv4Str.StartsWith('.')) return false;
            if (ipv4Str.EndsWith('.')) return false;
            string[] splitValues = ipv4Str.Split('.');
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

            bool isIP = IPAddress.TryParse(ipv4Str, out IPAddress? outIP);
            if (!isIP) return false;
            ipv4 = outIP;
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
        try
        {
            int byteLength = iPAddress.GetAddressBytes().Length;
            return iPAddress.AddressFamily == AddressFamily.InterNetworkV6 && byteLength == 16;
        }
        catch (Exception) { }
        return false;
    }

    public static bool IsIPv6Valid(string ipv6Str, out IPAddress? ipv6)
    {
        ipv6 = null;

        try
        {
            int numberOfColons = ipv6Str.Count(c => c == ':');
            if (numberOfColons >= 2 && numberOfColons <= 7)
            {
                if (Uri.CheckHostName(ipv6Str) == UriHostNameType.IPv6)
                {
                    bool isIP = IPAddress.TryParse(ipv6Str, out IPAddress? outIP);
                    if (isIP && outIP != null)
                    {
                        int byteLength = outIP.GetAddressBytes().Length;
                        if (outIP.AddressFamily == AddressFamily.InterNetworkV6 && byteLength == 16)
                        {
                            ipv6 = outIP;
                            return true;
                        }
                    }
                }
            }
        }
        catch (Exception) { }
        return false;
    }

    /// <summary>
    /// Is Ip In Range
    /// </summary>
    /// <param name="ip">IPv4 Or IPv6</param>
    /// <param name="cidr">IPv4 CIDR Or IPv6 CIDR</param>
    /// <returns>True Or False</returns>
    public static bool IsIpInRange(IPAddress ip, string cidr)
    {
        bool isInRange = false;

        try
        {
            // Split CIDR Into Base IP And Prefix Length
            if (cidr.Contains('/'))
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
        catch (Exception ex)
        {
            Debug.WriteLine("NetworkTool IsIpInRange: " + ex.Message);
        }

        return isInRange;
    }

    /// <summary>
    /// Is Ip In Range
    /// </summary>
    /// <param name="ipStr">IPv4 Or IPv6</param>
    /// <param name="cidr">IPv4 CIDR Or IPv6 CIDR</param>
    /// <returns>True Or False</returns>
    public static bool IsIpInRange(string ipStr, string cidr)
    {
        bool isIP = IsIP(ipStr, out IPAddress? ip);
        if (isIP && ip != null) return IsIpInRange(ip, cidr);
        return false;
    }

    public static int GetNextPort(int currentPort)
    {
        try
        {
            if (currentPort < 1) currentPort = 1;
            if (currentPort > 65535) currentPort = 65535;
            currentPort = currentPort < 65535 ? currentPort + 1 : currentPort - 1;
        }
        catch (Exception) { }
        return currentPort;
    }

    /// <summary>
    /// Is Port Free To Listen (All Platforms)
    /// </summary>
    /// <param name="port">Port</param>
    public static bool IsPortFree(int port)
    {
        bool isAvailable = true;

        try
        {
            IPGlobalProperties iPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            if (isAvailable)
            {
                try
                {
                    TcpConnectionInformation[] tcps = iPGlobalProperties.GetActiveTcpConnections();
                    for (int n = 0; n < tcps.Length; n++)
                    {
                        TcpConnectionInformation tcp = tcps[n];
                        if (tcp.LocalEndPoint.Port == port && tcp.State == TcpState.Listen)
                        {
                            isAvailable = false;
                            break;
                        }
                    }
                }
                catch (Exception) { isAvailable = false; }
            }

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
                catch (Exception) { isAvailable = false; }
            }

            if (isAvailable)
            {
                try
                {
                    IPEndPoint[] tcps = iPGlobalProperties.GetActiveTcpListeners();
                    for (int n = 0; n < tcps.Length; n++)
                    {
                        IPEndPoint ep = tcps[n];
                        if (ep.Port == port)
                        {
                            isAvailable = false;
                            break;
                        }
                    }
                }
                catch (Exception) { isAvailable = false; }
            }
        }
        catch (Exception) { isAvailable = false; }

        return isAvailable;
    }

    /// <summary>
    /// All Platforms
    /// </summary>
    /// <param name="port">Port</param>
    public static bool IsPortOpen(int port)
    {
        return !IsPortFree(port);
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

    /// <summary>
    /// Get A Free Random Port
    /// </summary>
    /// <returns>Returns 0 If Fail</returns>
    public static async Task<int> GetAFreePortAsync()
    {
        return await Task.Run(async () =>
        {
            try
            {
                Random rnd = new();
                int port;
                while (true)
                {
                    port = rnd.Next(0, 65535);
                    if (IsPortFree(port)) break;
                }
                return port;
            }
            catch (Exception)
            {
                int port = 0;
                try
                {
                    Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
                    if (socket.LocalEndPoint is IPEndPoint endPoint) port = endPoint.Port;
                    try
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                        socket.Dispose();
                    }
                    catch (Exception) { }
                    await Task.Delay(100);
                }
                catch (Exception) { }
                return port;
            }
        });
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
            Debug.WriteLine("NetworkTool GetAllNetworkInterfaces: " + ex.Message);
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

    public static async Task<bool> EnableNicIPv6Async(string nicName)
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
            Debug.WriteLine("NetworkTool EnableNicIPv6: " + ex.Message);
        }
        return success;
    }

    public static async Task<bool> DisableNicIPv6Async(string nicName)
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
            Debug.WriteLine("NetworkTool DisableNicIPv6: " + ex.Message);
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
    public static async Task SetDnsIPv4Async(string nicName, string dnsServers)
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
            Debug.WriteLine("NetworkTool SetDnsIPv4Async: " + ex.Message);
        }
    }

    /// <summary>
    /// Set's the IPv4 DNS Server of the local machine (Windows Only)
    /// </summary>
    /// <param name="nic">NIC address</param>
    /// <param name="dnsServers">Comma seperated list of DNS server addresses</param>
    /// <remarks>Requires a reference to the System.Management namespace</remarks>
    public static async Task SetDnsIPv4Async(NetworkInterface nic, string dnsServers)
    {
        if (!OperatingSystem.IsWindows()) return;
        // Requires Elevation
        if (nic == null) return;

        await SetDnsIPv4Async(nic.Name, dnsServers);
    }

    /// <summary>
    /// Set's the IPv6 DNS Server of the local machine (Windows Only)
    /// </summary>
    /// <param name="nicName">NIC Name</param>
    /// <param name="dnsServers">Comma seperated list of DNS server addresses</param>
    /// <remarks>Requires a reference to the System.Management namespace</remarks>
    public static async Task SetDnsIPv6Async(string nicName, string dnsServers)
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
            Debug.WriteLine("NetworkTool SetDnsIPv6Async: " + ex.Message);
        }
    }

    /// <summary>
    /// Set's the IPv6 DNS Server of the local machine (Windows Only)
    /// </summary>
    /// <param name="nic">NIC address</param>
    /// <param name="dnsServers">Comma seperated list of DNS server addresses</param>
    /// <remarks>Requires a reference to the System.Management namespace</remarks>
    public static async Task SetDnsIPv6Async(NetworkInterface nic, string dnsServers)
    {
        if (!OperatingSystem.IsWindows()) return;
        // Requires Elevation
        if (nic == null) return;

        await SetDnsIPv6Async(nic.Name, dnsServers);
    }

    /// <summary>
    /// Unset IPv4 DNS to DHCP (Windows Only)
    /// </summary>
    /// <param name="nicName">Network Interface Name</param>
    public static async Task UnsetDnsIPv4Async(string nicName)
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
            Debug.WriteLine("NetworkTool UnsetDnsIPv4Async: " + ex.Message);
        }
    }

    /// <summary>
    /// Unset IPv4 DNS to DHCP (Windows Only)
    /// </summary>
    /// <param name="nic">Network Interface</param>
    public static async Task UnsetDnsIPv4Async(NetworkInterface nic)
    {
        if (!OperatingSystem.IsWindows()) return;
        // Requires Elevation - Can't Unset DNS when there is no Internet connectivity but netsh can :)
        if (nic == null) return;

        await UnsetDnsIPv4Async(nic.Name);
    }

    /// <summary>
    /// Unset IPv4 DNS by seting DNS to Static
    /// </summary>
    /// <param name="nic">Network Interface</param>
    /// <param name="dns1">Primary</param>
    /// <param name="dns2">Secondary</param>
    public static async Task UnsetDnsIPv4Async(NetworkInterface nic, string dns1, string? dns2)
    {
        string dnsServers = dns1;
        if (!string.IsNullOrEmpty(dns2)) dnsServers += $",{dns2}";
        await SetDnsIPv4Async(nic, dnsServers);
    }

    /// <summary>
    /// Unset IPv4 DNS by seting DNS to Static
    /// </summary>
    /// <param name="nicName">Network Interface Name</param>
    /// <param name="dns1">Primary</param>
    /// <param name="dns2">Secondary</param>
    public static async Task UnsetDnsIPv4Async(string nicName, string dns1, string? dns2)
    {
        string dnsServers = dns1;
        if (!string.IsNullOrEmpty(dns2)) dnsServers += $",{dns2}";
        await SetDnsIPv4Async(nicName, dnsServers);
    }

    /// <summary>
    /// Unset IPv6 DNS to DHCP (Windows Only)
    /// </summary>
    /// <param name="nicName">Network Interface Name</param>
    public static async Task UnsetDnsIPv6Async(string nicName)
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
            Debug.WriteLine("NetworkTool UnsetDnsIPv6Async: " + ex.Message);
        }
    }

    /// <summary>
    /// Unset IPv6 DNS to DHCP (Windows Only)
    /// </summary>
    /// <param name="nic">Network Interface</param>
    public static async Task UnsetDnsIPv6Async(NetworkInterface nic)
    {
        if (!OperatingSystem.IsWindows()) return;
        // Requires Elevation - Can't Unset DNS when there is no Internet connectivity but netsh can :)
        if (nic == null) return;
        
        await UnsetDnsIPv6Async(nic.Name);
    }

    /// <summary>
    /// Unset IPv6 DNS by seting DNS to Static
    /// </summary>
    /// <param name="nic">Network Interface</param>
    /// <param name="dns1">Primary</param>
    /// <param name="dns2">Secondary</param>
    public static async Task UnsetDnsIPv6Async(NetworkInterface nic, string dns1, string? dns2)
    {
        string dnsServers = dns1;
        if (!string.IsNullOrEmpty(dns2)) dnsServers += $",{dns2}";
        await SetDnsIPv6Async(nic, dnsServers);
    }

    /// <summary>
    /// Unset IPv6 DNS by seting DNS to Static
    /// </summary>
    /// <param name="nicName">Network Interface Name</param>
    /// <param name="dns1">Primary</param>
    /// <param name="dns2">Secondary</param>
    public static async Task UnsetDnsIPv6Async(string nicName, string dns1, string? dns2)
    {
        string dnsServers = dns1;
        if (!string.IsNullOrEmpty(dns2)) dnsServers += $",{dns2}";
        await SetDnsIPv6Async(nicName, dnsServers);
    }

    /// <summary>
    /// Set Loopback To System. (Windows And Linux)
    /// </summary>
    public static async Task SetDnsToLoopbackAutoAsync()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Find Active NICs
                List<NICResult> nics = GetAllNetworkInterfaces();
                for (int n = 0; n < nics.Count; n++)
                {
                    NICResult nicR = nics[n];
                    if (nicR.IsUpAndRunning)
                    {
                        await SetDnsIPv4Async(nicR.NIC_Name, IPAddress.Loopback.ToString());
                        await SetDnsIPv6Async(nicR.NIC_Name, IPAddress.IPv6Loopback.ToStringNoScopeId());
                    }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    // 1. Try Systemd (resolvectl)
                    // Find Active NICs
                    List<NICResult> nics = GetNetworkInterfaces();
                    for (int n = 0; n < nics.Count; n++)
                    {
                        NICResult nicR = nics[n];
                        if (nicR.IsUpAndRunning)
                        {
                            string command = "resolvectl";
                            string args1 = $"dns {nicR.NIC_Name} 127.0.0.1";
                            string args2 = $"domain {nicR.NIC_Name} ~.";
                            await ProcessManager.ExecuteAsync(command, null, args1, true, true);
                            await ProcessManager.ExecuteAsync(command, null, args2, true, true);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("NetworkTool SetDnsToLoopbackAutoAsync Linux Systemd: " + e.Message);
                }

                try
                {
                    // 2. Try No Systemd (resolv.conf)
                    string resolveConfPath = "/etc/resolv.conf";

                    // Backup First
                    string backupPath = "/etc/resolv.conf.backup";

                    bool isBackupSetToLoopback = false;
                    if (File.Exists(backupPath))
                    {
                        string backupContent = await File.ReadAllTextAsync(backupPath);
                        if (backupContent.Contains(IPAddress.Loopback.ToString())) isBackupSetToLoopback = true;
                    }

                    if (File.Exists(resolveConfPath) && !isBackupSetToLoopback)
                    {
                        byte[] bytes = await File.ReadAllBytesAsync(resolveConfPath);
                        await File.WriteAllBytesAsync(backupPath, bytes);
                    }

                    // Set DNS To Loopback
                    string[] lines =
                    {
                        "nameserver 127.0.0.1",
                        "options edns0 trust-ad", // Support DNSSEC/EDNS
                        "search local" // Resolve Short Hostnames With Suffix
                    };

                    await File.WriteAllLinesAsync(resolveConfPath, lines);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("NetworkTool SetDnsToLoopbackAutoAsync Linux No Systemd: " + e.Message);
                }

                try
                {
                    // 3. Try Disable System DNS (systemctl)
                    string command = "systemctl";
                    string args1 = $"stop systemd-resolved.service";
                    string args2 = $"disable systemd-resolved.service";
                    await ProcessManager.ExecuteAsync(command, null, args1, true, true);
                    await ProcessManager.ExecuteAsync(command, null, args2, true, true);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("NetworkTool SetDnsToLoopbackAutoAsync Linux Disable System DNS: " + e.Message);
                }

                try
                {
                    // Restart Service
                    string command = "systemctl";
                    string args1 = $"daemon-reexec";
                    string args2 = $"daemon-reload";
                    await ProcessManager.ExecuteAsync(command, null, args1, true, true);
                    await ProcessManager.ExecuteAsync(command, null, args2, true, true);
                }
                catch (Exception) { }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("NetworkTool SetDnsToLoopbackAutoAsync: " + ex.Message);
        }
    }

    /// <summary>
    /// Unset DNS To DHCP. (Windows And Linux)
    /// </summary>
    public static async Task UnsetDnsAutoAsync()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Find Active NICs
                List<NICResult> nics = GetAllNetworkInterfaces();
                for (int n = 0; n < nics.Count; n++)
                {
                    NICResult nicR = nics[n];
                    if (nicR.IsUpAndRunning)
                    {
                        // Unset To DHCP
                        await UnsetDnsIPv4Async(nicR.NIC_Name);
                        await UnsetDnsIPv6Async(nicR.NIC_Name);
                    }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    // 1. Try Systemd (resolvectl)
                    // Find Active NICs
                    List<NICResult> nics = GetNetworkInterfaces();
                    for (int n = 0; n < nics.Count; n++) // Revert All Interfaces
                    {
                        NICResult nicR = nics[n];
                        if (nicR.IsUpAndRunning)
                        {
                            string command = "resolvectl";
                            string args = $"revert -i {nicR.NIC_Name}";
                            await ProcessManager.ExecuteAsync(command, null, args, true, true);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("NetworkTool UnsetDnsAutoAsync Linux Systemd: " + e.Message);
                }

                try
                {
                    // 2. Try No Systemd (resolv.conf)
                    string resolveConfPath = "/etc/resolv.conf";

                    // Backup Path
                    string backupPath = "/etc/resolv.conf.backup";

                    bool isConfSetToLoopback = false;
                    if (File.Exists(resolveConfPath))
                    {
                        string confContent = await File.ReadAllTextAsync(resolveConfPath);
                        if (confContent.Contains(IPAddress.Loopback.ToString())) isConfSetToLoopback = true;
                    }
                    else
                    {
                        isConfSetToLoopback = true; // It Doesn't Exist And Must Be Rewrite.
                    }

                    if (isConfSetToLoopback)
                    {
                        bool isBackupExist = false;
                        if (File.Exists(backupPath))
                        {
                            string backupContent = await File.ReadAllTextAsync(backupPath);
                            if (!string.IsNullOrWhiteSpace(backupContent) && !backupContent.Contains(IPAddress.Loopback.ToString())) isBackupExist = true;
                        }

                        if (isBackupExist)
                        {
                            // Revert To Backup
                            byte[] bytes = await File.ReadAllBytesAsync(backupPath);
                            await File.WriteAllBytesAsync(resolveConfPath, bytes);
                        }
                        else
                        {
                            // Backup Not Exist. Set To Google
                            string line = "nameserver 8.8.8.8\n";
                            await File.WriteAllTextAsync(resolveConfPath, line);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("NetworkTool UnsetDnsAutoAsync Linux No Systemd: " + e.Message);
                }

                try
                {
                    // 3. Try Enable System DNS (systemctl)
                    string command = "systemctl";
                    string args1 = $"enable systemd-resolved.service";
                    string args2 = $"start systemd-resolved.service";
                    string args3 = $"restart systemd-resolved.service"; // Must Be Last Command
                    await ProcessManager.ExecuteAsync(command, null, args1, true, true);
                    await ProcessManager.ExecuteAsync(command, null, args2, true, true);
                    await ProcessManager.ExecuteAsync(command, null, args3, true, true);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("NetworkTool UnsetDnsAutoAsync Linux Disable System DNS: " + e.Message);
                }

                try
                {
                    // Restart Service
                    string command = "systemctl";
                    string args1 = $"daemon-reexec";
                    string args2 = $"daemon-reload";
                    await ProcessManager.ExecuteAsync(command, null, args1, true, true);
                    await ProcessManager.ExecuteAsync(command, null, args2, true, true);
                }
                catch (Exception) { }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("NetworkTool UnsetDnsAutoAsync: " + ex.Message);
        }
    }

    /// <summary>
    /// Is DNS Set to 127.0.0.1 - Using Nslookup (Windows Only) // Hangs
    /// </summary>
    public static async Task<(bool IsSet, string Host, string IP)> IsDnsSetToLocalAsync()
    {
        bool result = false;
        string host = string.Empty, ip = string.Empty;
        if (!OperatingSystem.IsWindows()) return (result, host, ip);
        if (!await IsInternetAliveByNicAsync()) return (result, host, ip); // nslookup takes time when there is no internet access

        var p = await ProcessManager.ExecuteAsync("nslookup", null, "0.0.0.0", true, true);
        //string content = ProcessManager.Execute(out _, "nslookup", null, "0.0.0.0", true, true);
        if (!p.IsSeccess) return (result, host, ip);
        string content = p.Output.ToLower();
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
    /// Check Internet Access Based On NIC Send And Receive
    /// </summary>
    public static async Task<bool> IsInternetAliveByNicAsync(IPAddress? ip = null, int timeoutMS = 3000)
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

                        Debug.WriteLine("Sent:" + (bytesSent2 - bytesSent1));
                        Debug.WriteLine("Received: " + (bytesReceived2 - bytesReceived1));

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
        PingOnly,
        DnsOnly,
        Unstable,
        Offline,
        Unknown
    }

    public static async Task<InternetState> GetInternetStateAsync(IPAddress? ip, string? nonBlockedForeignDomain = "google.com", int timeoutMS = 6000)
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

            if (string.IsNullOrWhiteSpace(nonBlockedForeignDomain)) nonBlockedForeignDomain = "google.com";

            if (timeoutMS < 1000) timeoutMS = 1000;
            int timeoutSec = timeoutMS / 1000;

            bool byPing = false, byDnsIPv4 = false, byDnsIPv6 = false, byDnsIP = false;

            async Task byPingAsync()
            {
                byPing = await IsInternetAliveByPingAsync(ip, timeoutMS);
            }

            async Task byDnsIPv4Async()
            {
                IPAddress domainIPv4 = await GetIP.GetIpFromDnsAddressAsync(nonBlockedForeignDomain, $"udp://{ip}", false, timeoutSec, false, IPAddress.None, 0);
                byDnsIPv4 = domainIPv4 != IPAddress.None && domainIPv4 != IPAddress.IPv6None;
            }

            async Task byDnsIPv6Async()
            {
                IPAddress domainIPv6 = await GetIP.GetIpFromDnsAddressAsync(nonBlockedForeignDomain, $"udp://{ip}", false, timeoutSec, true, IPAddress.None, 0);
                byDnsIPv6 = domainIPv6 != IPAddress.None && domainIPv6 != IPAddress.IPv6None;
            }
            
            await Task.WhenAll(byPingAsync(), byDnsIPv4Async(), byDnsIPv6Async());
            byDnsIP = byDnsIPv4 || byDnsIPv6;
            
            if (byPing && byDnsIP) return InternetState.Online;
            if (byPing && !byDnsIP) return InternetState.PingOnly;
            if (!byPing && byDnsIP) return InternetState.DnsOnly;
            
            bool isAliveByNic = await IsInternetAliveByNicAsync(ip, timeoutMS);
            return isAliveByNic ? InternetState.Unstable : InternetState.Offline;
        }
        catch (Exception)
        {
            return InternetState.Offline;
        }
    }

    public static async Task<HttpStatusCode> GetHttpStatusCodeAsync(string urlOrDomain, string? ipStr, int timeoutMs, bool useSystemProxy, bool isAgnosticProxyTest = false, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null, CancellationToken ct = default)
    {
        HttpStatusCode result = HttpStatusCode.RequestTimeout;
        if (string.IsNullOrWhiteSpace(urlOrDomain)) return result;

        try
        {
            URL urid = GetUrlOrDomainDetails(urlOrDomain.Trim(), 443);
            if (urid.Uri ==  null) return result;

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
                URI = urid.Uri,
                Method = HttpMethod.Get,
                TimeoutMS = timeoutMs,
                AllowInsecure = true, // Ignore Cert Check To Make It Faster
                AllowAutoRedirect = true,
                ProxyScheme = proxyScheme,
                ProxyUser = proxyUser,
                ProxyPass = proxyPass,
            };
            hr.Headers.Add("host", urid.Host); // In Case Of Using IP
            if (!string.IsNullOrEmpty(ipStr) && IsIP(ipStr, out IPAddress? ip) && ip != null) hr.AddressIP = ip;
            if (isAgnosticProxyTest) hr.UserAgent = "DNSveil - A Secure DNS Client"; // Proxy Test Protocol Depends On This

            HttpRequestResponse hrr = await HttpRequest.SendAsync(hr).ConfigureAwait(false);

            result = hrr.StatusCode;
        }
        catch (Exception) { }

        return result;
    }

    public static async Task<string> GetHeadersAsync(string urlOrDomain, string? ipStr, int timeoutMs, bool useSystemProxy, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null)
    {
        string result = string.Empty;
        if (string.IsNullOrWhiteSpace(urlOrDomain)) return result;

        try
        {
            URL urid = GetUrlOrDomainDetails(urlOrDomain.Trim(), 443);
            bool firstTrySuccess = false;

            try
            {
                if (urid.Uri != null)
                {
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
                        URI = urid.Uri,
                        Method = HttpMethod.Get,
                        UserAgent = "Other",
                        TimeoutMS = timeoutMs,
                        AllowInsecure = true, // Ignore Cert Check To Make It Faster
                        AllowAutoRedirect = true,
                        ProxyScheme = proxyScheme,
                        ProxyUser = proxyUser,
                        ProxyPass = proxyPass,
                    };
                    hr.Headers.Add("host", urid.Host); // In Case Of Using IP
                    if (!string.IsNullOrEmpty(ipStr) && IsIP(ipStr, out IPAddress? ip) && ip != null) hr.AddressIP = ip;
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
            }
            catch (Exception) { }

            try
            {
                if (!firstTrySuccess && !urlOrDomain.Contains("://www."))
                {
                    urlOrDomain = $"{urid.Scheme}www.{urid.Host}:{urid.Port}{urid.Path}{urid.Query}{urid.Fragment}";
                    result = await GetHeadersAsync(urlOrDomain, ipStr, timeoutMs, useSystemProxy, proxyScheme, proxyUser, proxyPass);
                }
            }
            catch (Exception) { }
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
                Debug.WriteLine("NetworkTool SetProxy SetHttpProxy: " + ex.Message);
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
                Debug.WriteLine("NetworkTool UnsetProxy: " + ex.Message);
            }

            if (applyRegistryChanges) RegistryTool.ApplyRegistryChanges();
            try { registry.Dispose(); } catch (Exception) { }
        }
    }

    public static async Task<bool> IsHostBlockedAsync(string host, int port, int timeoutMS)
    {
        string url;
        if (port == 80) url = $"http://{host}:{port}";
        else url = $"https://{host}:{port}";
        return !await IsWebsiteOnlineAsync(url, null, timeoutMS, false);
    }

    public static async Task<bool> CanPingAsync(string host, int timeoutMS)
    {
        host = host.Trim();
        if (string.IsNullOrEmpty(host)) return false;
        if (host.Equals(IPAddress.Any.ToString())) return false;
        if (host.Equals(IPAddress.IPv6Any.ToStringNoScopeId())) return false;
        Task<bool> task = Task.Run(async () =>
        {
            try
            {
                Ping ping = new();
                PingReply? reply;
                bool isIp = IsIP(host, out IPAddress? ip);
                if (isIp && ip != null)
                    reply = await ping.SendPingAsync(ip, timeoutMS);
                else
                    reply = await ping.SendPingAsync(host, timeoutMS);

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

    public static async Task<bool> CanTcpConnectAsync(IPAddress ip, int port, int timeoutMS)
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

    public static async Task<bool> CanTcpConnectAsync(string host, int port, int timeoutMS)
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

    public static async Task<bool> CanUdpConnectAsync(IPAddress ip, int port, int timeoutMS)
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

    public static async Task<bool> CanUdpConnectAsync(string host, int port, int timeoutMS)
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

    public static TcpState GetTcpRemoteState(TcpClient tcpClient)
    {
        try
        {
            if (tcpClient.Client == null) return TcpState.Unknown;

            IPGlobalProperties ipgp = IPGlobalProperties.GetIPGlobalProperties();
            if (ipgp != null)
            {
                TcpConnectionInformation[]? tcis = ipgp.GetActiveTcpConnections();
                if (tcis != null)
                {
                    for (int n = 0; n < tcis.Length; n++)
                    {
                        TcpConnectionInformation? tci = tcis[n];
                        if (tci != null)
                        {
                            if (tcpClient.Client != null)
                            {
                                if (tcpClient.Client.RemoteEndPoint is IPEndPoint tcpClientEndPoint)
                                {
                                    if (tcpClientEndPoint.Address.Equals(tci.RemoteEndPoint.Address) || tcpClientEndPoint.Address.ToString().EndsWith(tci.RemoteEndPoint.Address.ToString()))
                                        if (tci.RemoteEndPoint.Port.Equals(tcpClientEndPoint.Port))
                                        {
                                            return tci.State;
                                        }

                                }
                            }
                        }
                    }
                }
            }

            return TcpState.Unknown;
        }
        catch (Exception)
        {
            return TcpState.Unknown;
        }
    }

}