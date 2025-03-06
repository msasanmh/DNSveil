using System.Diagnostics;
using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

public partial class MsmhAgnosticServer
{
    private ProxyRequest? ApplyRulesToRequest(ProxyRequest req, AgnosticProgram.Rules.RulesResult rr, ref string msgReqEvent, bool isCaptivePortal)
    {
        try
        {
            if (rr.IsMatch)
            {
                // Rules Program: Black List (-;)
                if (rr.IsBlackList)
                {
                    // Event
                    msgReqEvent += $"Black List: {req.Address}:{req.Port}, Request Denied.";
                    OnRequestReceived?.Invoke(msgReqEvent, EventArgs.Empty);
                    return null;
                }

                // Rules Program: Block Port
                if (rr.IsPortBlock)
                {
                    if (!isCaptivePortal)
                    {
                        // Event
                        msgReqEvent += $"Block Port {req.Port}: {req.Address}:{req.Port}, Request Denied.";
                        OnRequestReceived?.Invoke(msgReqEvent, EventArgs.Empty);
                        return null;
                    }
                }

                // Rules Program: Fake DNS Or Custom DNS
                bool isDnsIp = NetworkTool.IsIP(rr.Dns, out _);
                if (isDnsIp) req.Address = rr.Dns;

                // Rules Program: Apply DPI Bypass If Is Match And Not Direct (NoBypass --;)
                if (rr.IsDirect)
                {
                    // No Bypass And Upstream For Direct
                    req.ApplyFragment = false;
                    req.ApplyChangeSNI = false;
                    req.AddressSNI = req.AddressOrig;
                    req.ApplyUpstreamProxy = false;
                    req.ApplyUpstreamProxyToBlockedIPs = false;
                    req.UpstreamProxyScheme = string.Empty;
                    req.UpstreamProxyUser = string.Empty;
                    req.UpstreamProxyPass = string.Empty;
                }
                else
                {
                    req.ApplyFragment = IsFragmentActive;
                    req.ApplyChangeSNI = IsFakeSniActive;
                    if (!string.IsNullOrEmpty(rr.Sni) && !rr.Sni.Equals(req.AddressOrig)) req.AddressSNI = rr.Sni;
                    if (rr.ApplyUpStreamProxy && !string.IsNullOrWhiteSpace(rr.ProxyScheme) && !IsUpstreamEqualToServerAddress(rr.ProxyScheme))
                    {
                        req.ApplyUpstreamProxy = true;
                        req.ApplyUpstreamProxyToBlockedIPs = rr.ApplyUpStreamProxyToBlockedIPs;
                        req.UpstreamProxyScheme = rr.ProxyScheme;
                        req.UpstreamProxyUser = rr.ProxyUser;
                        req.UpstreamProxyPass = rr.ProxyPass;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AgnosticServer ApplyPrograms ApplyRulesToRequest: " + ex.Message);
            return null;
        }

        return req;
    }

    public async Task<ProxyRequest?> ApplyPrograms(IPAddress clientIP, ProxyRequest? req)
    {
        try
        {
            if (Cancel) return null;
            if (req == null) return null;
            if (string.IsNullOrEmpty(req.Address)) return null;
            if (req.Address.Equals("0.0.0.0")) return null;
            if (req.Address.StartsWith("10.")) return null;

            // Apply Settings To Request
            req.ClientIP = clientIP;
            req.TimeoutSec = Settings_.ProxyTimeoutSec;
            req.ApplyFragment = IsFragmentActive;
            if (!string.IsNullOrWhiteSpace(SettingsSSL_.DefaultSni))
            {
                req.ApplyChangeSNI = IsFakeSniActive;
                req.AddressSNI = SettingsSSL_.DefaultSni;
            }
            if (!string.IsNullOrWhiteSpace(Settings_.UpstreamProxyScheme) &&
                !IsUpstreamEqualToServerAddress(Settings_.UpstreamProxyScheme))
            {
                req.ApplyUpstreamProxy = true;
                req.ApplyUpstreamProxyToBlockedIPs = Settings_.ApplyUpstreamOnlyToBlockedIps;
                req.UpstreamProxyScheme = Settings_.UpstreamProxyScheme;
                req.UpstreamProxyUser = Settings_.UpstreamProxyUser;
                req.UpstreamProxyPass = Settings_.UpstreamProxyPass;
            }
            
            // Event
            string msgReqEvent = $"[{req.ClientIP}] [{req.ProxyName}] ";

            if (req.ProxyName == Proxy.Name.HTTP || req.ProxyName == Proxy.Name.HTTPS)
                msgReqEvent += $"[{req.HttpMethod}] ";

            if (req.ProxyName == Proxy.Name.Socks4 || req.ProxyName == Proxy.Name.Socks4A || req.ProxyName == Proxy.Name.Socks5)
                msgReqEvent += $"[{req.Command}] ";

            // Captive Portal
            bool isCaptivePortal = CaptivePortals.IsCaptivePortal(req.AddressOrig);
            
            // Test Requests
            bool isTestRequestExist = TestRequests.TryGetValue(req.AddressOrig, out (DateTime dt, bool applyFakeSNI, bool applyFragment) testReq);
            if (isTestRequestExist)
            {
                DateTime now = DateTime.UtcNow;
                TimeSpan ts = now - testReq.dt;
                if (ts >= TimeSpan.FromMinutes(2))
                {
                    TestRequests.TryRemove(req.AddressOrig, out _);
                    isTestRequestExist = false;
                }
            }

            // Settings: Block Port 80
            if (Settings_.BlockPort80 && req.Port == 80)
            {
                if (!isCaptivePortal)
                {
                    // Event
                    msgReqEvent += $"Block Port 80: {req.Address}:{req.Port}, Request Denied.";
                    OnRequestReceived?.Invoke(msgReqEvent, EventArgs.Empty);
                    return null;
                }
            }

            //// Rules Program: Domain
            AgnosticProgram.Rules.RulesResult rr = new();
            if (RulesProgram.RulesMode != AgnosticProgram.Rules.Mode.Disable)
            {
                rr = await RulesProgram.GetAsync(req.ClientIP.ToString(), req.Address, req.Port, Settings_);
            }

            // Apply Rules To Request
            req = ApplyRulesToRequest(req, rr, ref msgReqEvent, isCaptivePortal);
            if (req == null) return null;

            // Check If Address Is An IP (Before Applying DNS)
            bool isIp = NetworkTool.IsIP(req.Address, out _);

            //// Apply DNS To Proxy Request
            if (req.AddressOrig.Equals(req.Address) && !isIp)
            {
                string dnsServer = Settings_.ServerUdpDnsAddress;

                IPAddress ipv4Addr = IPAddress.None;
                if (Settings_.IsIPv4SupportedByISP)
                {
                    ipv4Addr = await GetIP.GetIpFromDnsAddressAsync(req.Address, dnsServer, false, Settings_);
                }

                if (ipv4Addr.Equals(IPAddress.None))
                {
                    if (Settings_.IsIPv6SupportedByISP)
                    {
                        IPAddress ipv6Addr = await GetIP.GetIpFromDnsAddressAsync(req.Address, dnsServer, true, Settings_);

                        if (!ipv6Addr.Equals(IPAddress.IPv6None))
                            req.Address = ipv6Addr.ToString();
                    }
                }
                else
                {
                    req.Address = ipv4Addr.ToString();
                }
            }

            // Check If Address Is An IP (After DNS Applied)
            isIp = NetworkTool.IsIP(req.Address, out IPAddress? ip);

            //// Rules Program: IP
            if (!rr.IsMatch && isIp && !req.AddressIsIp)
            {
                if (RulesProgram.RulesMode != AgnosticProgram.Rules.Mode.Disable)
                {
                    rr = await RulesProgram.GetAsync(req.ClientIP.ToString(), req.Address, req.Port, Settings_);
                }

                // Apply Rules To Request
                req = ApplyRulesToRequest(req, rr, ref msgReqEvent, isCaptivePortal);
                if (req == null) return null;
            }

            // If IP Is Cloudflare IP, Set The Clean IP
            if (isIp && !string.IsNullOrEmpty(Settings_.CloudflareCleanIP) &&
                !req.Address.Equals(Settings_.CloudflareCleanIP) && CommonTools.IsCfIP(req.Address))
                req.Address = Settings_.CloudflareCleanIP;

            // UDP Does Not Support Fragmentation
            if (req.ProxyName == Proxy.Name.Socks5 && req.Command == Socks.Commands.UDP)
                req.ApplyFragment = false;

            // No Bypass For Captive Portal
            if (isCaptivePortal)
            {
                req.ApplyChangeSNI = false;
                req.ApplyFragment = false;
            }

            // Override For Test
            if (req.ProxyName == Proxy.Name.Test && isTestRequestExist)
            {
                req.ApplyFragment = testReq.applyFragment;
                req.ApplyChangeSNI = testReq.applyFakeSNI;
            }

            // Turn ApplyChangeSNI Off If No SNI Is Set
            if (string.IsNullOrWhiteSpace(req.AddressSNI) || req.AddressSNI.Equals(req.AddressOrig))
                req.ApplyChangeSNI = false;

            // Event: Address
            if (req.AddressOrig.Equals(req.Address))
                msgReqEvent += $"{req.AddressOrig}:{req.Port}";
            else
            {
                if (!string.IsNullOrEmpty(rr.DnsCustomDomain) && !req.AddressOrig.Equals(rr.DnsCustomDomain))
                    msgReqEvent += $"{req.AddressOrig}:{req.Port} => {rr.DnsCustomDomain} => {req.Address}";
                else
                    msgReqEvent += $"{req.AddressOrig}:{req.Port} => {req.Address}";
            }

            // Add Orig Values To Cache
            ProxyRequestsCache.ProxyRequestsCacheResult prcr = new();
            prcr.OrigValues.ApplyChangeSNI = req.ApplyChangeSNI;
            prcr.OrigValues.ApplyFragment = req.ApplyFragment;
            prcr.OrigValues.IsDestBlocked = req.IsDestBlocked;

            // Cache Requests
            string checkRequest = $"{req.ClientIP}_{req.ProxyName}_{req.AddressOrig}_{req.Port}";
            var cachedReq = ProxyRequestsCaches.Get(checkRequest, req);

            // Check If IP Is Blocked
            HttpStatusCode hsc = HttpStatusCode.RequestTimeout;
            bool isIpv6 = false;
            if (isIp && ip != null)
            {
                isIpv6 = NetworkTool.IsIPv6(ip);
                if ((!isIpv6 && Settings_.IsIPv4SupportedByISP) || (isIpv6 && Settings_.IsIPv6SupportedByISP))
                {
                    if (req.ProxyName != Proxy.Name.Test)
                    {
                        if (req.ApplyUpstreamProxy && req.ApplyUpstreamProxyToBlockedIPs)
                        {
                            if (cachedReq != null)
                            {
                                req.IsDestBlocked = cachedReq.IsDestBlocked.Apply;
                            }
                            else
                            {
                                if (req.AddressIsIp)
                                {
                                    bool canPing = await NetworkTool.CanPingAsync(req.AddressOrig, 3000);
                                    req.IsDestBlocked = !canPing;
                                }
                                else
                                {
                                    if (hsc == HttpStatusCode.RequestTimeout)
                                        hsc = await NetworkTool.GetHttpStatusCodeAsync($"https://{req.AddressOrig}:{req.Port}", null, 4000, false, true, Settings_.ServerHttpProxyAddress).ConfigureAwait(false);
                                    req.IsDestBlocked = hsc == HttpStatusCode.RequestTimeout || hsc == HttpStatusCode.Forbidden;
                                }
                            }
                        }
                    }
                }
                else
                {
                    req.IsDestBlocked = true; // IP Protocol Is Not Supported
                    string ipP = isIpv6 ? "IPv6" : "IPv4";
                    msgReqEvent += $" (Your Network Does Not Support {ipP})";
                }
            }

            // Apply Upstream
            bool applyUpstream = (req.ApplyUpstreamProxy && !req.ApplyUpstreamProxyToBlockedIPs) ||
                                 (req.ApplyUpstreamProxy && req.ApplyUpstreamProxyToBlockedIPs && req.IsDestBlocked);
            req.ApplyUpstreamProxy = applyUpstream;
            if (req.ApplyUpstreamProxy)
            {
                req.ApplyChangeSNI = false;
                req.ApplyFragment = false;

                // Event: Upstream
                msgReqEvent += $" => Using Upstream: {req.UpstreamProxyScheme}";
            }

            // If Both Anti-DPI Methods Are Active Pick One
            if (req.ApplyChangeSNI && req.ApplyFragment)
            {
                // Check Cached Request
                if (cachedReq != null)
                {
                    req.ApplyChangeSNI = cachedReq.ApplyChangeSNI.Apply;
                    req.ApplyFragment = cachedReq.ApplyFragment.Apply;
                }
                else
                {
                    if (req.ProxyName != Proxy.Name.Test)
                    {
                        TestRequests.AddOrUpdate(req.AddressOrig, (DateTime.UtcNow, req.ApplyChangeSNI, false));

                        hsc = await NetworkTool.GetHttpStatusCodeAsync($"https://{req.AddressOrig}:{req.Port}", null, 4000, false, true, Settings_.ServerHttpProxyAddress).ConfigureAwait(false);
                        if (hsc == HttpStatusCode.OK || hsc == HttpStatusCode.NotFound || hsc == HttpStatusCode.BadRequest || hsc == HttpStatusCode.Forbidden)
                        {
                            // Fake SNI Is Compatible
                            req.ApplyFragment = false; // No Need To Fragment
                            TestRequests.AddOrUpdate(req.AddressOrig, (DateTime.UtcNow, req.ApplyChangeSNI, req.ApplyFragment));
                        }
                        else
                        {
                            // Fake SNI Is Not Compatible
                            req.ApplyChangeSNI = false; // Turn Off Fake SNI
                            TestRequests.AddOrUpdate(req.AddressOrig, (DateTime.UtcNow, req.ApplyChangeSNI, req.ApplyFragment));
                        }
                    }
                }
            }

            // If Both Are Still Active Use Fragment
            if (req.ApplyChangeSNI && req.ApplyFragment) req.ApplyChangeSNI = false;

            // Event: Direct / DPI Bypass Method
            bool isDirect = !req.ApplyFragment && !req.ApplyChangeSNI && !req.ApplyUpstreamProxy;
            if (isDirect) msgReqEvent += " => Direct";
            if (req.ApplyChangeSNI) msgReqEvent += $" => SNI: {req.AddressSNI}";
            else if (req.ApplyFragment) msgReqEvent += " => Fragmented";

            // Block Request Without DNS IP
            bool blockReq = !req.AddressIsIp && req.AddressOrig.Equals(req.Address) && !req.ApplyUpstreamProxy;
            if (blockReq) msgReqEvent += " - Request Denied (Has No DNS IP)";

            // Fire Event
            //Debug.WriteLine(msgReqEvent);
            if (req.ProxyName != Proxy.Name.Test)
                OnRequestReceived?.Invoke(msgReqEvent, EventArgs.Empty);

            if (blockReq) return null;

            // Change AddressType Based On DNS
            if (req.ProxyName == Proxy.Name.HTTP ||
                req.ProxyName == Proxy.Name.HTTPS ||
                (req.ProxyName == Proxy.Name.Socks4A && req.AddressType == Socks.AddressType.Domain) ||
                (req.ProxyName == Proxy.Name.Socks5 && req.AddressType == Socks.AddressType.Domain))
            {
                if (isIp && ip != null)
                {
                    if (isIpv6) req.AddressType = Socks.AddressType.Ipv6;
                    else
                        req.AddressType = Socks.AddressType.Ipv4;
                }
            }

            // Add To Cache
            if (req.ProxyName != Proxy.Name.Test && InternetState == NetworkTool.InternetState.Online)
            {
                prcr.ApplyChangeSNI.Apply = req.ApplyChangeSNI;
                prcr.ApplyFragment.Apply = req.ApplyFragment;
                prcr.IsDestBlocked.Apply = req.IsDestBlocked;

                ProxyRequestsCaches.Add(checkRequest, prcr);
            }
            
            return req;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AgnosticServer ApplyPrograms: " + ex.Message);
            return null;
        }
    }

    private bool IsUpstreamEqualToServerAddress(string? proxyScheme)
    {
        bool result = false;

        try
        {
            if (!string.IsNullOrEmpty(proxyScheme))
            {
                NetworkTool.GetUrlDetails(proxyScheme, 443, out _, out string host, out _, out _, out int port, out _, out _);
                if (Settings_.ListenerPort == port)
                {
                    bool isIP = NetworkTool.IsIP(host, out IPAddress? ip);
                    if (isIP && ip != null)
                    {
                        if (IPAddress.IsLoopback(ip)) result = true;
                    }
                    else
                    {
                        if (host.ToLower().Equals("localhost")) result = true;
                    }
                }
            }
        }
        catch (Exception) { }

        return result;
    }
}