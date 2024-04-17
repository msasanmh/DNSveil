using System.Diagnostics;
using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

public partial class MsmhAgnosticServer
{
    public async Task<ProxyRequest?> ApplyPrograms(IPAddress clientIP, ProxyRequest? req)
    {
        try
        {
            if (Cancel) return null;
            if (req == null) return null;
            if (string.IsNullOrEmpty(req.Address)) return null;
            if (req.Address.Equals("0.0.0.0")) return null;
            if (req.Address.StartsWith("10.")) return null;
            req.ClientIP = clientIP;

            // Event
            string msgReqEvent = $"[{req.ClientIP}] [{req.ProxyName}] ";

            if (req.ProxyName == Proxy.Name.HTTP || req.ProxyName == Proxy.Name.HTTPS)
                msgReqEvent += $"[{req.HttpMethod}] ";

            if (req.ProxyName == Proxy.Name.Socks4 || req.ProxyName == Proxy.Name.Socks4A || req.ProxyName == Proxy.Name.Socks5)
                msgReqEvent += $"[{req.Command}] ";

            // Count Max Requests
            string checkRequest = $"{clientIP}_{req.AddressOrig}";
            bool isKeyExist = DelinquentRequests.TryGetValue(checkRequest, out DateTime lastTime);
            if (isKeyExist)
            {
                DateTime now = DateTime.UtcNow;
                TimeSpan ts = now - lastTime;
                if (ts <= TimeSpan.FromMilliseconds(0.4))
                {
                    DelinquentRequests.TryUpdate(checkRequest, now, lastTime);

                    // Event
                    msgReqEvent += $"Delinquent Request Detected - Request Denied ({req.AddressOrig}:{req.Port})";
                    OnRequestReceived?.Invoke(msgReqEvent, EventArgs.Empty);
                    return null;
                }
                else DelinquentRequests.TryRemove(checkRequest, out _);
            }
            else DelinquentRequests.TryAdd(checkRequest, DateTime.UtcNow);

            // Cache Requests
            var cachedReq = ProxyRequestsCaches.Get(checkRequest, req);
            if (cachedReq != null)
            {
                OnRequestReceived?.Invoke(cachedReq.Value.eventMsg, EventArgs.Empty);
                return cachedReq.Value.pReq;
            }

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

            // Apply Programs
            req.TimeoutSec = Settings_.ProxyTimeoutSec;

            // Block Port 80
            if (Settings_.BlockPort80 && req.Port == 80)
            {
                // Event
                msgReqEvent += $"Block Port 80: {req.Address}:{req.Port}, Request Denied.";
                OnRequestReceived?.Invoke(msgReqEvent, EventArgs.Empty);
                return null;
            }

            //// ProxyRules Program
            AgnosticProgram.ProxyRules.ProxyRulesResult rr = new();
            if (ProxyRulesProgram.RulesMode != AgnosticProgram.ProxyRules.Mode.Disable)
            {
                rr = await ProxyRulesProgram.GetAsync(req.ClientIP.ToString(), req.Address, req.Port, Settings_);
            }

            if (rr.IsMatch)
            {
                // Black List
                if (rr.IsBlackList)
                {
                    // Event
                    msgReqEvent += $"Black List: {req.Address}:{req.Port}, Request Denied.";
                    OnRequestReceived?.Invoke(msgReqEvent, EventArgs.Empty);
                    return null;
                }

                // Block Port
                if (rr.IsPortBlock)
                {
                    // Event
                    msgReqEvent += $"Block Port {req.Port}: {req.Address}:{req.Port}, Request Denied.";
                    OnRequestReceived?.Invoke(msgReqEvent, EventArgs.Empty);
                    return null;
                }

                // Apply DPI Bypass If Is Match
                req.ApplyFragment = rr.ApplyDpiBypass && IsFragmentActive;
                req.ApplyChangeSNI = rr.ApplyDpiBypass && IsFakeSniActive;

                // Fake DNS Or Custom DNS
                bool isDnsIp = NetworkTool.IsIp(rr.Dns, out _);
                if (isDnsIp) req.Address = rr.Dns;
            }
            else
            {
                // Apply DPI Bypass If Is Not Match
                req.ApplyFragment = IsFragmentActive;
                req.ApplyChangeSNI = IsFakeSniActive;
            }

            // UDP Does Not Support Fragmentation
            if (req.ProxyName == Proxy.Name.Socks5 && req.Command == Socks.Commands.UDP)
                req.ApplyFragment = false;

            // Override For Test
            if (req.ProxyName == Proxy.Name.Test && isTestRequestExist)
            {
                req.ApplyFragment = testReq.applyFragment;
                req.ApplyChangeSNI = testReq.applyFakeSNI;
            }

            //// FakeSNI Program
            if (req.ApplyChangeSNI)
            {
                if (rr.IsMatch)
                    if (!string.IsNullOrEmpty(rr.Sni) && !rr.Sni.Equals(req.AddressOrig))
                        req.AddressSNI = rr.Sni;

                if (req.AddressSNI.Equals(req.AddressOrig))
                {
                    string defaultSni = SettingsSSL_.DefaultSni;
                    if (!string.IsNullOrWhiteSpace(defaultSni))
                    {
                        req.AddressSNI = defaultSni;
                    }
                }

                if (string.IsNullOrWhiteSpace(req.AddressSNI) || req.AddressSNI.Equals(req.AddressOrig))
                    req.ApplyChangeSNI = false;
            }

            // Check If Address Is An IP
            bool isIp = NetworkTool.IsIp(req.Address, out _);

            //// Apply DNS To Proxy Request
            if (req.AddressOrig.Equals(req.Address) && !isIp)
            {
                string dnsServer = Settings_.ServerUdpDnsAddress;

                IPAddress ipv4Addr = IPAddress.None;
                if (Settings_.IsIPv4SupportedByISP)
                {
                    ipv4Addr = await GetIP.GetIpFromDnsAddressAsync(req.Address, dnsServer, false, Settings_);
                    if (ipv4Addr.Equals(IPAddress.None) && !Settings_.IsIPv6SupportedByISP) // Retry If IPv6 Is Not Supported
                        ipv4Addr = await GetIP.GetIpFromDnsAddressAsync(req.Address, dnsServer, false, Settings_);
                }

                if (ipv4Addr.Equals(IPAddress.None))
                {
                    IPAddress ipv6Addr = await GetIP.GetIpFromDnsAddressAsync(req.Address, dnsServer, true, Settings_);
                    if (!ipv6Addr.Equals(IPAddress.IPv6None))
                        req.Address = ipv6Addr.ToString();
                }
                else
                {
                    if (string.IsNullOrEmpty(Settings_.CloudflareCleanIP))
                        req.Address = ipv4Addr.ToString();
                    else
                        req.Address = CommonTools.IsCfIP(ipv4Addr) ? Settings_.CloudflareCleanIP : ipv4Addr.ToString();
                }
            }

            // Event
            if (req.AddressOrig.Equals(req.Address))
                msgReqEvent += $"{req.AddressOrig}:{req.Port}";
            else
            {
                if (!string.IsNullOrEmpty(rr.DnsCustomDomain) && !req.AddressOrig.Equals(rr.DnsCustomDomain))
                    msgReqEvent += $"{req.AddressOrig}:{req.Port} => {rr.DnsCustomDomain} => {req.Address}";
                else
                    msgReqEvent += $"{req.AddressOrig}:{req.Port} => {req.Address}";
            }

            HttpStatusCode httpStatus = HttpStatusCode.RequestTimeout;
            if (req.ApplyChangeSNI)
            {
                msgReqEvent += $" => {req.AddressSNI}";
                if (req.ProxyName != Proxy.Name.Test)
                {
                    HttpStatusCode hsc = await NetworkTool.GetHttpStatusCode($"https://{req.AddressOrig}:{req.Port}", null, 5000, false, Settings_.ServerHttpProxyAddress).ConfigureAwait(false);
                    if (hsc == HttpStatusCode.Forbidden || hsc == HttpStatusCode.MisdirectedRequest || hsc == HttpStatusCode.InternalServerError || hsc == HttpStatusCode.RequestTimeout)
                    {
                        HttpStatusCode hsc2 = await NetworkTool.GetHttpStatusCode($"https://{req.AddressOrig}:{req.Port}", null, 5000, false).ConfigureAwait(false);
                        if (hsc2 != HttpStatusCode.RequestTimeout)
                        {
                            req.ApplyChangeSNI = false;
                            req.ApplyFragment = false;
                            msgReqEvent += $" ({hsc2}) => OFF";
                            httpStatus = hsc2;
                        }
                        else if (hsc != HttpStatusCode.RequestTimeout)
                        {
                            req.ApplyChangeSNI = false;
                            msgReqEvent += $" ({hsc})";
                            TestRequests.AddOrUpdate(req.AddressOrig, (DateTime.UtcNow, req.ApplyChangeSNI, req.ApplyFragment));
                            httpStatus = hsc;
                        }
                    }
                }
            }

            if (!req.ApplyChangeSNI && req.ApplyFragment)
            {
                msgReqEvent += " => Fragmented";
                if (req.ProxyName != Proxy.Name.Test)
                {
                    HttpStatusCode hsc = await NetworkTool.GetHttpStatusCode($"https://{req.AddressOrig}:{req.Port}", null, 3000, false, Settings_.ServerHttpProxyAddress).ConfigureAwait(false);
                    if (hsc != HttpStatusCode.OK && hsc != HttpStatusCode.NotFound && hsc != HttpStatusCode.RequestTimeout)
                    {
                        req.ApplyFragment = false;
                        msgReqEvent += $" ({hsc}) => OFF";
                        TestRequests.AddOrUpdate(req.AddressOrig, (DateTime.UtcNow, req.ApplyChangeSNI, req.ApplyFragment));
                        httpStatus = hsc;
                    }
                }
            }

            // Check If IP Is Blocked
            isIp = NetworkTool.IsIp(req.Address, out IPAddress? ip);
            bool isIpv6 = false;
            if (isIp && ip != null)
            {
                isIpv6 = NetworkTool.IsIPv6(ip);
                if ((!isIpv6 && Settings_.IsIPv4SupportedByISP) || (isIpv6 && Settings_.IsIPv6SupportedByISP))
                {
                    if (req.ProxyName != Proxy.Name.Test)
                    {
                        if (httpStatus == HttpStatusCode.RequestTimeout || httpStatus == HttpStatusCode.Forbidden)
                        {
                            httpStatus = await NetworkTool.GetHttpStatusCode($"https://{req.AddressOrig}:{req.Port}", null, 5000, false, Settings_.ServerHttpProxyAddress).ConfigureAwait(false);
                            req.IsDestBlocked = httpStatus == HttpStatusCode.RequestTimeout || httpStatus == HttpStatusCode.Forbidden;

                            //if (req.ProxyName == Proxy.Name.Socks5 && req.Command == Socks.Commands.UDP)
                            //    req.IsDestBlocked = !await NetworkTool.CanUdpConnect(req.Address, req.Port, 5000);
                            //else
                            //{
                            //    bool canPing = await NetworkTool.CanPing(req.Address, 5000);
                            //    bool canTcpConnect = await NetworkTool.CanTcpConnect(req.Address, req.Port, 5000);
                            //    Debug.WriteLine("===--> " + req.AddressOrig + " -> " + httpStatus + " -> " + canPing + " -> " + canTcpConnect);
                            //    req.IsDestBlocked = !canPing || !canTcpConnect;
                            //}

                            if (req.IsDestBlocked) msgReqEvent += $" ({httpStatus})";
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

            // Apply Upstream?
            if ((rr.IsMatch && rr.ApplyUpStreamProxy && !rr.ApplyUpStreamProxyToBlockedIPs) ||
                (rr.IsMatch && rr.ApplyUpStreamProxy && rr.ApplyUpStreamProxyToBlockedIPs && req.IsDestBlocked))
            {
                if (!IsUpstreamEqualToServerAddress(rr.ProxyScheme))
                {
                    req.ApplyUpStreamProxy = true;
                    req.ApplyChangeSNI = false;
                    req.ApplyFragment = false;
                    msgReqEvent += $" (Using Upstream: {rr.ProxyScheme})";
                }
            }

            if (!req.ApplyUpStreamProxy && !string.IsNullOrWhiteSpace(Settings_.UpstreamProxyScheme))
            {
                if ((!Settings_.ApplyUpstreamOnlyToBlockedIps) ||
                    (Settings_.ApplyUpstreamOnlyToBlockedIps && req.IsDestBlocked))
                {
                    if (!IsUpstreamEqualToServerAddress(Settings_.UpstreamProxyScheme))
                    {
                        req.ApplyUpStreamProxy = true;
                        req.ApplyChangeSNI = false;
                        req.ApplyFragment = false;
                        msgReqEvent += $" (Using Upstream: {Settings_.UpstreamProxyScheme.ToLower()})";
                    }
                }
            }

            // Block Request Without DNS IP
            bool blockReq = !req.AddressIsIp && req.AddressOrig.Equals(req.Address) && !req.ApplyUpStreamProxy;
            if (blockReq) msgReqEvent += " Request Denied (Has No DNS IP)";

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
            ProxyRequestsCaches.Add(checkRequest, req, msgReqEvent);

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
                    bool isIP = NetworkTool.IsIp(host, out IPAddress? ip);
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