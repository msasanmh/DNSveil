using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

public static class Bootstrap
{
    /// <summary>
    /// Get IP Of A Domain (IPv4 Is Preferred)
    /// </summary>
    /// <returns>If Fail: Returns Input Domain Name.</returns>
    public static async Task<string> GetDnsIpAsync(string domain, IPAddress bootstrapIP, int bootstrapPort, int timeoutSec, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null)
    {
        // Try IPv4
        string domainIP = await GetDnsIpInternalAsync(domain, bootstrapIP, bootstrapPort, timeoutSec, false, proxyScheme, proxyUser, proxyPass);
        if (domainIP.Equals(domain, StringComparison.InvariantCultureIgnoreCase))
        {
            // Try IPv6
            domainIP = await GetDnsIpInternalAsync(domain, bootstrapIP, bootstrapPort, timeoutSec, true, proxyScheme, proxyUser, proxyPass);
        }
        return domainIP;
    }

    /// <summary>
    /// Get IP Of A Domain
    /// </summary>
    /// <returns>If Fail: Returns Input Domain Name.</returns>
    private static async Task<string> GetDnsIpInternalAsync(string domain, IPAddress bootstrapIP, int bootstrapPort, int timeoutSec, bool getIPv6, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null)
    {
        string domainIP = domain;
        bool isIP = NetworkTool.IsIP(domain, out _);
        if (!isIP)
        {
            if (bootstrapIP == IPAddress.None || bootstrapIP == IPAddress.IPv6None || bootstrapPort < 1)
            {
                IPAddress ip = GetIP.GetIpFromSystem(domain, getIPv6, true);
                if (ip != IPAddress.None && ip != IPAddress.IPv6None) domainIP = ip.ToStringNoScopeId();
            }
            else
            {
                // Try UDP
                string bootstrap = NetworkTool.IpToUrl("udp", bootstrapIP, bootstrapPort, string.Empty);
                IPAddress ip = await GetIP.GetIpFromDnsAddressAsync(domain, bootstrap, false, timeoutSec, getIPv6, IPAddress.None, 0, proxyScheme, proxyUser, proxyPass);

                if (ip == IPAddress.None || ip == IPAddress.IPv6None)
                {
                    // Try TCP - TCP Usually Don't Get Hijack!
                    bootstrap = NetworkTool.IpToUrl("tcp", bootstrapIP, bootstrapPort, string.Empty);
                    ip = await GetIP.GetIpFromDnsAddressAsync(domain, bootstrap, false, timeoutSec, getIPv6, IPAddress.None, 0, proxyScheme, proxyUser, proxyPass);
                }

                if (ip != IPAddress.None && ip != IPAddress.IPv6None) domainIP = ip.ToStringNoScopeId();
            }
        }
        return domainIP;
    }

}