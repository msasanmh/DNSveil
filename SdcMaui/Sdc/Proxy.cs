using MsmhToolsClass.MsmhProxyServer;
using MsmhToolsClass.ProxyServerPrograms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SdcMaui
{
    public partial class MainPage
    {
        public void StartProxy()
        {
            // Apply Proxy Programs
            ProxyProgram.Dns dnsProgram = new();
            dnsProgram.Set(ProxyProgram.Dns.Mode.PlainDNS, $"{IPAddress.Loopback}:{DnsPort}", null, 2, null);
            ProxyServer.EnableDNS(dnsProgram);

            ProxyProgram.DPIBypass dpiBypassProgram = new();
            dpiBypassProgram.Set(ProxyProgram.DPIBypass.Mode.Program, DpiBeforeSniChunks, DpiChunkMode, DpiSniChunks, DpiAntiPattern, DpiFragmentDelay);
            ProxyServer.EnableStaticDPIBypass(dpiBypassProgram);

            // Start Proxy
            if (!ProxyServer.IsRunning)
                ProxyServer.Start(IPAddress.Any, ProxyPort, ProxyMaxRequests, 40, 0, true);
        }
    }
}
