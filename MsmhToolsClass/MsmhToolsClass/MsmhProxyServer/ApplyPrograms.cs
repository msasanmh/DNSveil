using MsmhToolsClass.ProxyServerPrograms;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace MsmhToolsClass.MsmhProxyServer;

public partial class MsmhProxyServer
{
    public async Task<ProxyRequest?> ApplyPrograms(ProxyRequest? req)
    {
        if (req == null) return null;
        if (string.IsNullOrEmpty(req.Address)) return null;
        if (NetworkTool.IsLocalIP(req.Address)) return null;

        req.TimeoutSec = ProxySettings_.RequestTimeoutSec;
        
        // Block Port 80
        if (ProxySettings_.BlockPort80 && req.Port == 80)
        {
            // Event
            string msgEvent = $"Block Port 80: {req.Address}:{req.Port}, request denied.";
            OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);
            return null;
        }

        //// Black White List Program
        if (BWListProgram.ListMode != ProxyProgram.BlackWhiteList.Mode.Disable)
        {
            bool isMatch = BWListProgram.IsMatch(req.Address);
            if (isMatch)
            {
                // Event
                string msgEvent = $"Black White list: {req.Address}:{req.Port}, request denied.";
                OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);
                return null;
            }
        }

        //// DontBypass Program
        req.ApplyDpiBypass = true;
        if (DontBypassProgram.DontBypassMode != ProxyProgram.DontBypass.Mode.Disable)
        {
            bool isMatch = DontBypassProgram.IsMatch(req.Address);
            if (isMatch) req.ApplyDpiBypass = false;
        }

        // Save Orig Hostname
        string origHostname = req.Address;

        //// FakeDNS Program
        if (FakeDNSProgram.FakeDnsMode != ProxyProgram.FakeDns.Mode.Disable)
            req.Address = FakeDNSProgram.Get(req.Address);

        // Check If Address Is An IP
        bool isIp = IPAddress.TryParse(req.Address, out IPAddress? _);

        //// DNS Program
        if (origHostname.Equals(req.Address))
        {
            if (DNSProgram.DNSMode != ProxyProgram.Dns.Mode.Disable && !isIp)
            {
                string ipStr = await DNSProgram.Get(req.Address);
                if (!string.IsNullOrEmpty(ipStr) && !NetworkTool.IsLocalIP(ipStr))
                    req.Address = ipStr;
            }
        }

        // Event
        string msgReqEvent = $"[{req.ProxyName}] ";

        if (req.ProxyName == Proxy.Name.HTTP || req.ProxyName == Proxy.Name.HTTPS)
            msgReqEvent += $"[{req.HttpMethod}] ";

        if (req.ProxyName == Proxy.Name.Socks4 || req.ProxyName == Proxy.Name.Socks4A || req.ProxyName == Proxy.Name.Socks5)
            msgReqEvent += $"[{req.Command}] ";

        if (origHostname.Equals(req.Address))
            msgReqEvent += $"{origHostname}:{req.Port}";
        else
            msgReqEvent += $"{origHostname}:{req.Port} => {req.Address}:{req.Port}";

        // Is Upstream Active
        bool isUpStreamProgramActive = UpStreamProxyProgram.UpStreamMode != ProxyProgram.UpStreamProxy.Mode.Disable;

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

        // Apply upstream?
        if ((isUpStreamProgramActive && !UpStreamProxyProgram.OnlyApplyToBlockedIps) ||
            (isUpStreamProgramActive && UpStreamProxyProgram.OnlyApplyToBlockedIps && req.IsDestBlocked))
            req.ApplyUpStreamProxy = true;

        if (req.ApplyUpStreamProxy)
            msgReqEvent += " (Bypassing through Upstream Proxy)";

        Debug.WriteLine(msgReqEvent);
        OnRequestReceived?.Invoke(msgReqEvent, EventArgs.Empty);

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
            req.ApplyDpiBypass = false;

        return req;
    }
}