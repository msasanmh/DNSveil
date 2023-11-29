using Ae.Dns.Client;
using Ae.Dns.Protocol;
using MsmhToolsClass;
using MsmhToolsClass.DnsTool;
using MsmhToolsClass.MsmhProxyServer;
using MsmhToolsClass.ProxyServerPrograms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace SdcMaui
{
    public partial class MainPage
    {
        // Settings Check Servers
        public static readonly int CheckTimeoutMS = 3000;

        // Settings Connect
        public static readonly int MaxServersToConnect = 5;

        // Settings Local DNS Server // can't bind to ports < 1024 without administrative privileges. Use a port >= 1024
        public static readonly int DnsPort = 5390;

        // Settings Proxy
        public static readonly int ProxyPort = 8080;

        // Settings Proxy DPI Bypass
        public static readonly int DpiBeforeSniChunks = 50;
        public static readonly ProxyProgram.DPIBypass.ChunkMode DpiChunkMode = ProxyProgram.DPIBypass.ChunkMode.SNI;
        public static readonly int DpiSniChunks = 5;
        public static readonly int DpiAntiPattern = 2;
        public static readonly int DpiFragmentDelay = 5;

        // Settings General
        public static readonly string DomainToCheck = "youtube.com";

        // SDC
        public static readonly string NL = Environment.NewLine;
        public List<Tuple<int, string>> WorkingDnsList = new();
        public IDnsClient[] DnsClients = Array.Empty<IDnsClient>();
        public static bool IsCheckingStarted { get; set; } = false;
        public static bool StopChecking { get; set; } = false;
        public static bool IsConnecting { get; set; } = false;
        public static bool IsDisconnecting { get; set; } = false;
        public static bool IsDnsConnected { get; set; } = false;
        public static int LocalDnsLatency { get; set; } = -1;
        public static int ConnectedDnsPort { get; set; } = -1;
        public static bool IsProxyActive { get; set; } = false;
        public static int ConnectedProxyPort { get; set; } = -1;
        private static int ProxyRequests { get; set; } = 0;
        private static int ProxyMaxRequests { get; set; } = 250;
        private static bool IsProxyDpiBypassActive { get; set; } = false;

        // Local DNS Server
        public CancellationTokenSource CancelTokenDnsServer = new();
        public static IDnsServer? DnsUdpServer { get; set; }
        public static IDnsServer? DnsTcpServer { get; set; }
        //public static IDnsRawClient? dnsRawClient { get; set; }

        // Local HTTP Proxy Server
        public static MsmhProxyServer ProxyServer { get; set; } = new();

    }
}
