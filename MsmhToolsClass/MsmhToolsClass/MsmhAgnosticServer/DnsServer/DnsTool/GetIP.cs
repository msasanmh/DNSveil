using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

public static class GetIP
{
    /// <summary>
    /// Get First IP in Answer Section
    /// </summary>
    /// <param name="host">Host</param>
    /// <param name="getIPv6">Look for IPv6</param>
    /// <returns>Returns IPAddress.None/IPAddress.IPv6None If Fail</returns>
    public static IPAddress GetIpFromSystem(string host, bool getIPv6 = false, bool alsoUsePing = true)
    {
        List<IPAddress> ips = GetIpsFromSystem(host, getIPv6);
        if (ips.Count != 0) return ips[0];
        else
        {
            if (alsoUsePing)
            {
                try
                {
                    using Ping ping = new();
                    PingReply reply = ping.Send(host, 3000);
                    IPAddress ip = reply.Address;
                    bool isIpv6 = NetworkTool.IsIPv6(ip);

                    if (getIPv6 && isIpv6 && !IPAddress.IsLoopback(ip)) return ip;
                    if (!getIPv6 && !isIpv6 && !IPAddress.IsLoopback(ip)) return ip;
                }
                catch (Exception) { }
            }
        }

        return getIPv6 ? IPAddress.IPv6None : IPAddress.None;
    }

    /// <summary>
    /// Get a List of IPs
    /// </summary>
    /// <param name="host">Host</param>
    /// <param name="getIPv6">Look for IPv6</param>
    /// <returns>Returns Empty List If Fail</returns>
    public static List<IPAddress> GetIpsFromSystem(string host, bool getIPv6 = false)
    {
        List<IPAddress> ips = new();

        try
        {
            //IPAddress[] ipAddresses = System.Net.Dns.GetHostAddresses(host);
            IPHostEntry ipHostEntry = Dns.GetHostEntry(host);
            IPAddress[] ipAddresses = ipHostEntry.AddressList;

            if (ipAddresses == null || ipAddresses.Length == 0) return ips;

            if (!getIPv6)
            {
                for (int n = 0; n < ipAddresses.Length; n++)
                {
                    AddressFamily addressFamily = ipAddresses[n].AddressFamily;
                    IPAddress ip = ipAddresses[n];
                    if (addressFamily != AddressFamily.InterNetworkV6)
                        if (!string.IsNullOrEmpty(ip.ToString()) && !NetworkTool.IsLocalIP(ip.ToString()) && !IPAddress.IsLoopback(ip))
                            ips.Add(ip);
                }
            }
            else
            {
                for (int n = 0; n < ipAddresses.Length; n++)
                {
                    AddressFamily addressFamily = ipAddresses[n].AddressFamily;
                    IPAddress ip = ipAddresses[n];
                    if (addressFamily == AddressFamily.InterNetworkV6)
                        if (!string.IsNullOrEmpty(ip.ToString()) && !IPAddress.IsLoopback(ip))
                            ips.Add(ip);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

        return ips;
    }

    /// <summary>
    /// Get IPs in Answer Section (Using Wire Format)
    /// </summary>
    /// <param name="host">Host</param>
    /// <param name="dnss">A List Of DNS Servers</param>
    /// <param name="timeoutSec">Timeout (Sec)</param>
    /// <param name="proxyScheme">Use Proxy to Get IP</param>
    /// <returns>Returns An Empty List If Fail</returns>
    public static async Task<List<IPAddress>> GetIPsFromDnsAddressAsync(string host, List<string> dnss, bool allowInsecure, int timeoutSec, bool getIPv6, IPAddress bootstrapIP, int bootstrapPort, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null)
    {
        List<IPAddress> ips = new();
        DnsEnums.RRType rrType = getIPv6 ? DnsEnums.RRType.AAAA : DnsEnums.RRType.A;
        DnsMessage dmQ = DnsMessage.CreateQuery(DnsEnums.DnsProtocol.UDP, host, rrType, DnsEnums.CLASS.IN);
        bool isWriteSuccess = DnsMessage.TryWrite(dmQ, out byte[] dmQBuffer);
        if (isWriteSuccess)
        {
            byte[] dmABuffer = await DnsClient.QueryAsync(dmQBuffer, DnsEnums.DnsProtocol.UDP, dnss, allowInsecure, bootstrapIP, bootstrapPort, timeoutSec * 1000, proxyScheme, proxyUser, proxyPass);
            DnsMessage dmA = DnsMessage.Read(dmABuffer, DnsEnums.DnsProtocol.UDP);

            if (dmA.IsSuccess && dmA.Header.AnswersCount > 0)
            {
                foreach (IResourceRecord irr in dmA.Answers.AnswerRecords)
                {
                    if (getIPv6)
                    {
                        if (irr is not AaaaRecord aaaaRecord) continue;
                        bool isLoopbackIP = IPAddress.IsLoopback(aaaaRecord.IP);
                        if (!isLoopbackIP) ips.Add(aaaaRecord.IP);
                    }
                    else
                    {
                        if (irr is not ARecord aRecord) continue;
                        string ipStr = aRecord.IP.ToString();
                        bool isLocalIP = NetworkTool.IsLocalIP(ipStr);
                        bool isLoopbackIP = IPAddress.IsLoopback(aRecord.IP);
                        if (!isLocalIP && !isLoopbackIP) ips.Add(aRecord.IP);
                    }
                }
            }
        }
        return ips;
    }

    /// <summary>
    /// Get IPs in Answer Section (Using Wire Format)
    /// </summary>
    /// <param name="host">Host</param>
    /// <param name="dnss">A List Of DNS Servers</param>
    /// <returns>Returns An Empty List If Fail</returns>
    public static async Task<List<IPAddress>> GetIPsFromDnsAddressAsync(string host, List<string> dnss, bool getIPv6, AgnosticSettings s)
    {
        return await GetIPsFromDnsAddressAsync(host, dnss, s.AllowInsecure, s.DnsTimeoutSec, getIPv6, s.BootstrapIpAddress, s.BootstrapPort, s.UpstreamProxyScheme, s.UpstreamProxyUser, s.UpstreamProxyPass);
    }

    /// <summary>
    /// Get IPs in Answer Section (Using Wire Format)
    /// </summary>
    /// <param name="host">Host</param>
    /// <param name="dns">A DNS Server</param>
    /// <param name="timeoutSec">Timeout (Sec)</param>
    /// <param name="proxyScheme">Use Proxy to Get IP</param>
    /// <returns>Returns An Empty List If Fail</returns>
    public static async Task<List<IPAddress>> GetIPsFromDnsAddressAsync(string host, string dns, bool allowInsecure, int timeoutSec, bool getIPv6, IPAddress bootstrapIP, int bootstrapPort, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null)
    {
        return await GetIPsFromDnsAddressAsync(host, new List<string>() { dns }, allowInsecure, timeoutSec, getIPv6, bootstrapIP, bootstrapPort, proxyScheme, proxyUser, proxyPass);
    }

    /// <summary>
    /// Get IPs in Answer Section (Using Wire Format)
    /// </summary>
    /// <param name="host">Host</param>
    /// <param name="dns">A DNS Server</param>
    /// <returns>Returns An Empty List If Fail</returns>
    public static async Task<List<IPAddress>> GetIPsFromDnsAddressAsync(string host, string dns, bool getIPv6, AgnosticSettings s)
    {
        return await GetIPsFromDnsAddressAsync(host, new List<string>() { dns }, s.AllowInsecure, s.DnsTimeoutSec, getIPv6, s.BootstrapIpAddress, s.BootstrapPort, s.UpstreamProxyScheme, s.UpstreamProxyUser, s.UpstreamProxyPass);
    }

    /// <summary>
    /// Get First IP in Answer Section (Using Wire Format)
    /// </summary>
    /// <param name="host">Host</param>
    /// <param name="dnss">A List Of DNS Servers</param>
    /// <param name="timeoutSec">Timeout (Sec)</param>
    /// <param name="proxyScheme">Use Proxy to Get IP</param>
    /// <returns>Returns IPAddress.None/IPAddress.IPv6None If Fail</returns>
    public static async Task<IPAddress> GetIpFromDnsAddressAsync(string host, List<string> dnss, bool allowInsecure, int timeoutSec, bool getIPv6, IPAddress bootstrapIP, int bootstrapPort, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null)
    {
        DnsEnums.RRType rrType = getIPv6 ? DnsEnums.RRType.AAAA : DnsEnums.RRType.A;
        DnsMessage dmQ = DnsMessage.CreateQuery(DnsEnums.DnsProtocol.UDP, host, rrType, DnsEnums.CLASS.IN);
        bool isWriteSuccess = DnsMessage.TryWrite(dmQ, out byte[] dmQBuffer);
        if (isWriteSuccess)
        {
            byte[] dmABuffer = await DnsClient.QueryAsync(dmQBuffer, DnsEnums.DnsProtocol.UDP, dnss, allowInsecure, bootstrapIP, bootstrapPort, timeoutSec * 1000, proxyScheme, proxyUser, proxyPass);
            DnsMessage dmA = DnsMessage.Read(dmABuffer, DnsEnums.DnsProtocol.UDP);

            if (dmA.IsSuccess && dmA.Header.AnswersCount > 0)
            {
                foreach (IResourceRecord irr in dmA.Answers.AnswerRecords)
                {
                    if (getIPv6)
                    {
                        if (irr is not AaaaRecord aaaaRecord) continue;
                        bool isLoopbackIP = IPAddress.IsLoopback(aaaaRecord.IP);
                        if (!isLoopbackIP) return aaaaRecord.IP;
                    }
                    else
                    {
                        if (irr is not ARecord aRecord) continue;
                        string ipStr = aRecord.IP.ToString();
                        bool isLocalIP = NetworkTool.IsLocalIP(ipStr);
                        bool isLoopbackIP = IPAddress.IsLoopback(aRecord.IP);
                        if (!isLocalIP && !isLoopbackIP) return aRecord.IP;
                    }
                }
            }
        }
        return getIPv6 ? IPAddress.IPv6None : IPAddress.None;
    }

    /// <summary>
    /// Get First IP in Answer Section (Using Wire Format)
    /// </summary>
    /// <param name="host">Host</param>
    /// <param name="dnss">A List Of DNS Servers</param>
    /// <returns>Returns IPAddress.None/IPAddress.IPv6None If Fail</returns>
    public static async Task<IPAddress> GetIpFromDnsAddressAsync(string host, List<string> dnss, bool getIPv6, AgnosticSettings s)
    {
        return await GetIpFromDnsAddressAsync(host, dnss, s.AllowInsecure, s.DnsTimeoutSec, getIPv6, s.BootstrapIpAddress, s.BootstrapPort, s.UpstreamProxyScheme, s.UpstreamProxyUser, s.UpstreamProxyPass);
    }

    /// <summary>
    /// Get First IP in Answer Section (Using Wire Format)
    /// </summary>
    /// <param name="host">Host</param>
    /// <param name="dns">A DNS Server</param>
    /// <param name="timeoutSec">Timeout (Sec)</param>
    /// <param name="proxyScheme">Use Proxy to Get IP</param>
    /// <returns>Returns IPAddress.None/IPAddress.IPv6None If Fail</returns>
    public static async Task<IPAddress> GetIpFromDnsAddressAsync(string host, string dns, bool allowInsecure, int timeoutSec, bool getIPv6, IPAddress bootstrapIP, int bootstrapPort, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null)
    {
        return await GetIpFromDnsAddressAsync(host, new List<string>() { dns }, allowInsecure, timeoutSec, getIPv6, bootstrapIP, bootstrapPort, proxyScheme, proxyUser, proxyPass);
    }

    /// <summary>
    /// Get First IP in Answer Section (Using Wire Format)
    /// </summary>
    /// <param name="host">Host</param>
    /// <param name="dns">A DNS Server</param>
    /// <returns>Returns IPAddress.None/IPAddress.IPv6None If Fail</returns>
    public static async Task<IPAddress> GetIpFromDnsAddressAsync(string host, string dns, bool getIPv6, AgnosticSettings s)
    {
        return await GetIpFromDnsAddressAsync(host, new List<string>() { dns }, s.AllowInsecure, s.DnsTimeoutSec, getIPv6, s.BootstrapIpAddress, s.BootstrapPort, s.UpstreamProxyScheme, s.UpstreamProxyUser, s.UpstreamProxyPass);
    }

}