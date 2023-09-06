using MsmhToolsClass.HTTPProxyServer;
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
        public void StartHttpProxy()
        {
            // Apply HTTP Proxy Programs
            HTTPProxyServer.Program.Dns dnsProgram = new();
            dnsProgram.Set(HTTPProxyServer.Program.Dns.Mode.PlainDNS, $"{IPAddress.Loopback}:{DnsPort}", null, 2, null);
            HttpProxy.EnableDNS(dnsProgram);

            HTTPProxyServer.Program.DPIBypass dpiBypassProgram = new();
            dpiBypassProgram.Set(HTTPProxyServer.Program.DPIBypass.Mode.Program, DpiBeforeSniChunks, DpiChunkMode, DpiSniChunks, DpiAntiPattern, DpiFragmentDelay);
            HttpProxy.EnableStaticDPIBypass(dpiBypassProgram);

            // Start HTTP Proxy
            if (!HttpProxy.IsRunning)
                HttpProxy.Start(IPAddress.Any, HttpProxyPort, HttpProxyMaxRequests);
        }
    }
}
