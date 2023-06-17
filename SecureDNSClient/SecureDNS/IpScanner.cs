using MsmhTools;
using MsmhTools.HTTPProxyServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SecureDNSClient
{
    public class IpScannerResult
    {
        public string? IP { get; set; }
        public int RealDelay { get; set; }
        public int TcpDelay { get; set; }
        public int PingDelay { get; set; }

        public IpScannerResult() { }
    }
    public class IpScanner
    {
        private List<IpScannerResult> WorkingIPs { get; set; } = new();
        private List<string> IpRangeList { get; set; } = new();
        private List<string> AllIPs { get; set; } = new();
        private bool StopScan { get; set; } = false;
        private static HTTPProxyServer ProxyServer = new();

        // Public
        public bool IsRunning { get; private set; } = false;
        public int CheckPort { get; set; } = 443;

        /// <summary>
        /// An open website with chosen CDN to check. e.g. https://www.cloudflare.com
        /// </summary>
        public string CheckWebsite { get; set; } = "https://www.cloudflare.com";

        /// <summary>
        /// Timeout/Delay (ms)
        /// </summary>
        public int Timeout { get; set; }
        public bool RandomScan { get; set; } = true;
        public int ProxyServerPort { get; set; } = 8090;

        /// <summary>
        /// Sender is IpScannerResult
        /// </summary>
        public event EventHandler<EventArgs>? OnWorkingIpReceived;
        public event EventHandler<EventArgs>? OnNewIpCheck;
        public event EventHandler<EventArgs>? OnNumberOfCheckedIpChanged;
        public event EventHandler<EventArgs>? OnPercentChanged;
        public event EventHandler<EventArgs>? OnFullReportChanged;

        public IpScanner()
        {

        }

        /// <summary>
        /// Find Clean IPs
        /// </summary>
        /// <param name="ipRange">e.g. 103.21.244.0 - 103.21.244.255\n198.41.128.0 - 198.41.143.255</param>
        public void SetIpRange(string ipRange)
        {
            if (!string.IsNullOrEmpty(ipRange))
                ipRange += Environment.NewLine;
            IpRangeList = ipRange.SplitToLines();
        }

        public List<IpScannerResult> GetWorkingIPs
        {
            get => WorkingIPs;
        }

        public int GetAllIpsCount
        {
            get => AllIPs.Count;
        }

        public void Stop()
        {
            StopScan = true;
        }
        
        public void Start()
        {
            IsRunning = true;
            StopScan = false;
            if (AllIPs.Any()) AllIPs.Clear();
            if (!ProxyServer.IsRunning) ProxyServer.Start(IPAddress.Loopback, ProxyServerPort, 50000);
            ProxyServer.BlockPort80 = true;
            Random random = new();

            Task.Run(async () =>
            {
                for (int n = 0; n < IpRangeList.Count; n++)
                {
                    string ipRange = IpRangeList[n].Trim();

                    if (!string.IsNullOrEmpty(ipRange))
                    {
                        string[] split = ipRange.Split('-');
                        string ipMin = split[0].Trim();
                        string ipMax = split[1].Trim();

                        string[] ipMins = ipMin.Split('.');
                        int ipMin1 = 0, ipMin2 = 0, ipMin3 = 0, ipMin4 = 0;

                        try
                        {
                            ipMin1 = int.Parse(ipMins[0]);
                            ipMin2 = int.Parse(ipMins[1]);
                            ipMin3 = int.Parse(ipMins[2]);
                            ipMin4 = int.Parse(ipMins[3]);
                        }
                        catch (Exception)
                        {
                            // do nothing
                        }

                        string[] ipMaxs = ipMax.Split('.');
                        int ipMax1 = 0, ipMax2 = 0, ipMax3 = 0, ipMax4 = 0;

                        try
                        {
                            ipMax1 = int.Parse(ipMaxs[0]);
                            ipMax2 = int.Parse(ipMaxs[1]);
                            ipMax3 = int.Parse(ipMaxs[2]);
                            ipMax4 = int.Parse(ipMaxs[3]);
                        }
                        catch (Exception)
                        {
                            // do nothing
                        }

                        for (int ipOut1 = ipMin1; ipOut1 <= ipMax1; ipOut1++)
                        {
                            for (int ipOut2 = ipMin2; ipOut2 <= ipMax2; ipOut2++)
                            {
                                for (int ipOut3 = ipMin3; ipOut3 <= ipMax3; ipOut3++)
                                {
                                    for (int ipOut4 = ipMin4; ipOut4 <= ipMax4; ipOut4++)
                                    {
                                        if (StopScan)
                                        {
                                            IsRunning = false;
                                            if (ProxyServer.IsRunning)
                                                ProxyServer.Stop();
                                            return;
                                        }

                                        string ipOut = $"{ipOut1}.{ipOut2}.{ipOut3}.{ipOut4}";

                                        AllIPs.Add(ipOut);
                                    }
                                }
                            }
                        }
                    }
                }

                for (int n = 0; n < AllIPs.Count; n++)
                {
                    if (StopScan)
                    {
                        IsRunning = false;
                        if (ProxyServer.IsRunning)
                            ProxyServer.Stop();
                        return;
                    }

                    OnNumberOfCheckedIpChanged?.Invoke(n, EventArgs.Empty);

                    int percent = 0;
                    if (n > 0 && n < AllIPs.Count - 1)
                        percent = (n * 100) / AllIPs.Count;
                    if (n == AllIPs.Count - 1)
                        percent = 100;
                    OnPercentChanged?.Invoke(percent, EventArgs.Empty);

                    string ipOut = AllIPs[n];

                    if (RandomScan)
                    {
                        int rn = random.Next(AllIPs.Count);
                        ipOut = AllIPs[rn];
                    }
                    
                    OnNewIpCheck?.Invoke(ipOut, EventArgs.Empty);
                    OnFullReportChanged?.Invoke($"Checking: {ipOut} ({n} of {AllIPs.Count}) {percent}%", EventArgs.Empty);

                    try
                    {
                        Stopwatch pingDelay = new();
                        pingDelay.Start();
                        bool canPing = Network.CanPing(ipOut, Timeout);
                        pingDelay.Stop();

                        if (canPing)
                        {
                            Stopwatch tcpDelay = new();
                            tcpDelay.Start();
                            bool canConnectToPort = Network.CanPing(ipOut, CheckPort, Timeout);
                            tcpDelay.Stop();

                            if (canConnectToPort)
                            {
                                string url = CheckWebsite;
                                var host = Network.UrlToHostAndPort(url, CheckPort, out int _, out string _, out bool _);
                                Uri uri = new(url, UriKind.Absolute);

                                HTTPProxyServer.Program.BlackWhiteList wList = new();
                                wList.Set(HTTPProxyServer.Program.BlackWhiteList.Mode.WhiteListText, host);

                                ProxyServer.EnableBlackWhiteList(wList);

                                HTTPProxyServer.Program.FakeDns fakeDns = new();
                                fakeDns.Set(HTTPProxyServer.Program.FakeDns.Mode.Text, $"{host}|{ipOut}");

                                ProxyServer.EnableFakeDNS(fakeDns);

                                ProxyServer.KillAll();
                                await Task.Delay(100);

                                string proxyScheme = $"http://{IPAddress.Loopback}:{ProxyServerPort}";

                                using SocketsHttpHandler socketsHttpHandler = new();
                                socketsHttpHandler.Proxy = new WebProxy(proxyScheme, true);

                                using HttpClient httpClientWithProxy = new(socketsHttpHandler);
                                httpClientWithProxy.Timeout = TimeSpan.FromMilliseconds(Timeout);

                                Stopwatch realDelay = new();
                                realDelay.Start();
                                HttpResponseMessage r = await httpClientWithProxy.GetAsync(uri);
                                realDelay.Stop();

                                if (r.IsSuccessStatusCode || r.StatusCode == HttpStatusCode.Forbidden ||
                                                             r.StatusCode == HttpStatusCode.NotFound)
                                {
                                    IpScannerResult scannerResult = new();
                                    scannerResult.IP = ipOut;
                                    scannerResult.RealDelay = Convert.ToInt32(realDelay.ElapsedMilliseconds);
                                    scannerResult.TcpDelay = Convert.ToInt32(tcpDelay.ElapsedMilliseconds);
                                    scannerResult.PingDelay = Convert.ToInt32(pingDelay.ElapsedMilliseconds);

                                    OnWorkingIpReceived?.Invoke(scannerResult, EventArgs.Empty);
                                    WorkingIPs.Add(scannerResult);
                                }
                                realDelay.Reset();
                            }
                            tcpDelay.Reset();
                        }
                        pingDelay.Reset();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"IP Scanner: {ex.Message}");
                    }
                }

            });
        }
    }
}
