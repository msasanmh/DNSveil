using Ae.Dns.Client;
using Ae.Dns.Protocol;
using MsmhToolsClass;
using MsmhToolsClass.DnsTool;
using MsmhToolsClass.HTTPProxyServer;
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
        public static readonly int CheckTimeoutMS = 2000;

        // Settings Connect
        public static readonly int MaxServersToConnect = 5;

        // Settings Local DNS Server // can't bind to ports < 1024 without administrative privileges. Use a port >= 1024
        public static readonly int DnsPort = 5053;

        // Settings HTTP Proxy
        public static readonly int HttpProxyPort = 8080;

        // Settings HTTP Proxy DPI Bypass
        public static readonly int DpiBeforeSniChunks = 50;
        public static readonly HTTPProxyServer.Program.DPIBypass.ChunkMode DpiChunkMode = HTTPProxyServer.Program.DPIBypass.ChunkMode.SNI;
        public static readonly int DpiSniChunks = 5;
        public static readonly int DpiAntiPattern = 2;
        public static readonly int DpiFragmentDelay = 5;

        // Settings General
        public static readonly string DomainToCheck = "youtube.com";

        // SDC
        public static readonly string NL = Environment.NewLine;
        public List<Tuple<int, string>> WorkingDnsList = new();
        public static bool IsCheckingStarted { get; set; } = false;
        public static bool StopChecking { get; set; } = false;
        public static bool IsConnecting { get; set; } = false;
        public static bool IsDisconnecting { get; set; } = false;
        public static bool IsDnsConnected { get; set; } = false;
        public static int LocalDnsLatency { get; set; } = -1;
        public static int ConnectedDnsPort { get; set; } = -1;
        public static bool IsHttpProxyActive { get; set; } = false;
        public static int ConnectedHttpProxyPort { get; set; } = -1;
        private static int HttpProxyRequests { get; set; } = 0;
        private static int HttpProxyMaxRequests { get; set; } = 250;
        private static bool IsHttpProxyDpiBypassActive { get; set; } = false;

        // Local DNS Server
        public CancellationTokenSource CancelTokenDnsServer = new();
        public static IDnsServer? DnsUdpServer { get; set; }
        public static IDnsServer? DnsTcpServer { get; set; }
        //public static IDnsRawClient? dnsRawClient { get; set; }

        // Local HTTP Proxy Server
        public static HTTPProxyServer HttpProxy { get; set; } = new();

    }
}
