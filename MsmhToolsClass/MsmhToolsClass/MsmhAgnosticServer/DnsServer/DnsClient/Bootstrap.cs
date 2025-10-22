using System.Diagnostics;
using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

public static class Bootstrap
{
    /// <summary>
    /// Get IP Of A Domain (IPv4 Is Preferred)
    /// To Use System-DNS Set bootstrapIP To IPAddress.None
    /// To Skip Set bootstrapIP To IPAddress.Any
    /// </summary>
    /// <returns>If Fail: Returns Input Domain Name.</returns>
    public static async Task<string> GetDnsIpAsync(string domain, IPAddress bootstrapIP, int bootstrapPort, int timeoutSec, List<AgnosticProgram.Rules.Rule>? ruleList, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null)
    {
        string domainIP = domain;

        try
        {
            if (bootstrapIP.Equals(IPAddress.Any) || bootstrapIP.Equals(IPAddress.IPv6Any)) return domainIP;

            // Try Rules
            domainIP = GetDnsIpInternal(domain, ruleList);
            if (domainIP.Equals(domain, StringComparison.InvariantCultureIgnoreCase))
            {
                // Try IPv4
                domainIP = await GetDnsIpInternalAsync(domain, bootstrapIP, bootstrapPort, timeoutSec, false, proxyScheme, proxyUser, proxyPass);
                if (domainIP.Equals(domain, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Try IPv6
                    domainIP = await GetDnsIpInternalAsync(domain, bootstrapIP, bootstrapPort, timeoutSec, true, proxyScheme, proxyUser, proxyPass);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Bootstrap GetDnsIpAsync: " + ex.Message);
        }

        return domainIP;
    }

    private static string GetDnsIpInternal(string domain, List<AgnosticProgram.Rules.Rule>? ruleList)
    {
        string domainIP = domain;

        try
        {
            bool isIP = NetworkTool.IsIP(domain, out _);
            if (!isIP)
            {
                // Search In Rules
                if (ruleList != null)
                {
                    for (int n = 0; n < ruleList.Count; n++)
                    {
                        AgnosticProgram.Rules.Rule rule = ruleList[n];
                        if (!rule.Address.Equals(domain, StringComparison.InvariantCultureIgnoreCase)) continue;

                        if (!string.IsNullOrEmpty(rule.FakeDnsIP))
                        {
                            bool itsIP = NetworkTool.IsIP(rule.FakeDnsIP, out _);
                            if (itsIP) domainIP = rule.FakeDnsIP;
                        }

                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Bootstrap GetDnsIpInternal: " + ex.Message);
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

        try
        {
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
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Bootstrap GetDnsIpInternalAsync: " + ex.Message);
        }

        return domainIP;
    }

}