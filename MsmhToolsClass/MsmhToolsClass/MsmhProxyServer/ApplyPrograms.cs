using MsmhToolsClass.ProxyServerPrograms;
using System.Diagnostics;
using System.Net;

namespace MsmhToolsClass.MsmhProxyServer;

public partial class MsmhProxyServer
{
    public async Task<ProxyRequest?> ApplyPrograms(IPAddress clientIP, ProxyRequest? req)
    {
        if (Cancel) return null;
        if (req == null) return null;
        if (string.IsNullOrEmpty(req.Address)) return null;
        if (req.Address.Equals("0.0.0.0")) return null;
        if (req.Address.StartsWith("10.")) return null;
        req.ClientIP = clientIP;
        
        // Count Max Requests
        MaxRequestsQueue2.Enqueue(DateTime.UtcNow);
        if (ProxySettings_.MaxRequests >= MaxRequestsDivide)
            if (MaxRequestsQueue2.Count >= ProxySettings_.MaxRequests / MaxRequestsDivide) // Check for 50 ms (1000 / 20)
            {
                // Event
                string blockEvent = $"Recevied {MaxRequestsQueue2.Count * MaxRequestsDivide} Requests Per Second - Request Denied ({req.AddressOrig}:{req.Port}) Due To Max Requests of {ProxySettings_.MaxRequests}.";
                Debug.WriteLine(blockEvent);
                OnRequestReceived?.Invoke(blockEvent, EventArgs.Empty);
                return null;
            }

        // Apply Programs
        req.TimeoutSec = ProxySettings_.RequestTimeoutSec;
        
        // Block Port 80
        if (ProxySettings_.BlockPort80 && req.Port == 80)
        {
            // Event
            string msgEvent = $"Block Port 80: {req.Address}:{req.Port}, Request Denied.";
            OnRequestReceived?.Invoke(msgEvent, EventArgs.Empty);
            return null;
        }

        //// Proxy Rules Program
        ProxyProgram.Rules.RulesResult rr = new();
        if (RulesProgram.RulesMode != ProxyProgram.Rules.Mode.Disable)
        {
            rr = await RulesProgram.GetAsync(req.ClientIP.ToString(), req.Address, req.Port);
        }

        if (rr.IsMatch)
        {
            // Black List
            if (rr.IsBlackList)
            {
                // Event
                string msgEvent = $"Black List: {req.Address}:{req.Port}, Request Denied.";
                OnRequestReceived?.Invoke(msgEvent, EventArgs.Empty);
                return null;
            }

            // Block Port
            if (rr.IsPortBlock)
            {
                // Event
                string msgEvent = $"Block Port {req.Port}: {req.Address}:{req.Port}, Request Denied.";
                OnRequestReceived?.Invoke(msgEvent, EventArgs.Empty);
                return null;
            }

            // Apply DPI Bypass
            req.ApplyFragment = rr.ApplyDpiBypass;
            req.ApplyChangeSNI = rr.ApplyDpiBypass;

            // Fake DNS Or DNS
            bool isDnsIp = NetworkTool.IsIp(rr.Dns, out _);
            if (isDnsIp) req.Address = rr.Dns;
        }

        //// FakeSNI Program
        if (req.ApplyChangeSNI && SettingsSSL_.EnableSSL && SettingsSSL_.ChangeSni)
        {
            if (rr.IsMatch)
                if (!string.IsNullOrEmpty(rr.Sni) && !rr.Sni.Equals(req.AddressOrig))
                    req.AddressSNI = rr.Sni;

            if (req.AddressSNI.Equals(req.AddressOrig))
            {
                string defaultSni = SettingsSSL_.DefaultSni;
                if (!string.IsNullOrEmpty(defaultSni) && !string.IsNullOrWhiteSpace(defaultSni))
                {
                    req.AddressSNI = defaultSni;
                }
            }
        }
        
        // Check If Address Is An IP
        bool isIp = NetworkTool.IsIp(req.Address, out _);

        //// DNS Program
        if (req.AddressOrig.Equals(req.Address))
        {
            if (DNSProgram.DNSMode != ProxyProgram.Dns.Mode.Disable && !isIp)
            {
                string ipStr = await DNSProgram.Get(req.Address);
                if (!string.IsNullOrEmpty(ipStr) && !NetworkTool.IsLocalIP(ipStr))
                    req.Address = ipStr;
            }
        }

        // Event
        string msgReqEvent = $"[{req.ClientIP}] [{req.ProxyName}] ";

        if (req.ProxyName == Proxy.Name.HTTP || req.ProxyName == Proxy.Name.HTTPS)
            msgReqEvent += $"[{req.HttpMethod}] ";

        if (req.ProxyName == Proxy.Name.Socks4 || req.ProxyName == Proxy.Name.Socks4A || req.ProxyName == Proxy.Name.Socks5)
            msgReqEvent += $"[{req.Command}] ";

        if (req.AddressOrig.Equals(req.Address))
            msgReqEvent += $"{req.AddressOrig}:{req.Port}";
        else
        {
            if (!string.IsNullOrEmpty(rr.DnsCustomDomain) && !req.AddressOrig.Equals(rr.DnsCustomDomain))
                msgReqEvent += $"{req.AddressOrig}:{req.Port} => {rr.DnsCustomDomain} => {req.Address}:{req.Port}";
            else
                msgReqEvent += $"{req.AddressOrig}:{req.Port} => {req.Address}:{req.Port}";
        }

        if (!req.AddressOrig.Equals(req.AddressSNI) && req.ApplyChangeSNI && SettingsSSL_.EnableSSL && SettingsSSL_.ChangeSni)
            msgReqEvent += $" => {req.AddressSNI}:{req.Port}";

        // Check if Dest Host or IP is blocked
        isIp = NetworkTool.IsIp(req.Address, out IPAddress? ip);
        bool isIpv6 = false;
        if (isIp && ip != null)
        {
            isIpv6 = NetworkTool.IsIPv6(ip);
            if (req.ProxyName == Proxy.Name.Socks5 && req.Command == Socks.Commands.UDP)
                req.IsDestBlocked = !await NetworkTool.CanUdpConnect(req.Address, req.Port, 5000);
            else
            {
                bool canPing = await NetworkTool.CanPing(req.Address, 5000);
                bool canTcpConnect = await NetworkTool.CanTcpConnect(req.Address, req.Port, 5000);
                req.IsDestBlocked = !canPing || !canTcpConnect;
            }
        }
        else
            req.IsDestBlocked = await NetworkTool.IsHostBlocked(req.Address, req.Port, 5000);

        if (req.IsDestBlocked && isIp)
        {
            bool isIpProtocolReachable = NetworkTool.IsIpProtocolReachable(req.Address); // Returns True for Non Windows
            if (isIpProtocolReachable)
                msgReqEvent += " (IP is blocked)";
            else
            {
                string ipP = isIpv6 ? "Ipv6" : "Ipv4";
                msgReqEvent += $" (your network does not support {ipP})";
            }
        }
        if (req.IsDestBlocked && !isIp)
            msgReqEvent += " (Host is blocked)";

        // Apply Upstream?
        if ((rr.IsMatch && rr.ApplyUpStreamProxy && !rr.ApplyUpStreamProxyToBlockedIPs) ||
            (rr.IsMatch && rr.ApplyUpStreamProxy && rr.ApplyUpStreamProxyToBlockedIPs && req.IsDestBlocked))
        {
            req.ApplyUpStreamProxy = true;
            msgReqEvent += $" (Bypassing through Upstream Proxy: {rr.ProxyScheme})";
        }

        if (!req.ApplyUpStreamProxy)
        {
            bool isUpStreamProgramActive = UpStreamProxyProgram.UpStreamMode != ProxyProgram.UpStreamProxy.Mode.Disable;
            if ((isUpStreamProgramActive && !UpStreamProxyProgram.OnlyApplyToBlockedIps) ||
                (isUpStreamProgramActive && UpStreamProxyProgram.OnlyApplyToBlockedIps && req.IsDestBlocked))
            {
                req.ApplyUpStreamProxy = true;
                msgReqEvent += $" (Bypassing through Upstream Proxy: {UpStreamProxyProgram.UpStreamMode.ToString().ToLower()}://{UpStreamProxyProgram.ProxyHost}:{UpStreamProxyProgram.ProxyPort})";
            }
        }
        
        // Block Blocked Hosts Without DNS IP
        bool blockReq = req.IsDestBlocked && !req.AddressIsIp && req.AddressOrig.Equals(req.Address) && !req.ApplyUpStreamProxy;
        if (blockReq)
            msgReqEvent += " Request Denied (It's Blocked and has no DNS IP)";

        Debug.WriteLine(msgReqEvent);
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

        // UDP Does Not Support Fragmentation
        if (req.ProxyName == Proxy.Name.Socks5 && req.Command == Socks.Commands.UDP)
            req.ApplyFragment = false;

        return req;
    }
}