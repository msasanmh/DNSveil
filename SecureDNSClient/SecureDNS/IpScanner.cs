using MsmhToolsClass;
using System.Diagnostics;
using System.Net;

namespace SecureDNSClient;

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
    private List<string> CIDR_List { get; set; } = new();
    private List<IPAddress> AllIPs { get; set; } = new();
    private bool StopScan { get; set; } = false;

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

    /// <summary>
    /// Sender is IpScannerResult
    /// </summary>
    public event EventHandler<EventArgs>? OnWorkingIpReceived;
    public event EventHandler<EventArgs>? OnNewIpCheck;
    public event EventHandler<EventArgs>? OnNumberOfCheckedIpChanged;
    public event EventHandler<EventArgs>? OnPercentChanged;
    public event EventHandler<EventArgs>? OnFullReportChanged;

    public IpScanner() { }

    /// <summary>
    /// Find Clean IPs
    /// </summary>
    /// <param name="cidrList">A List Of CIDR</param>
    public void SetIpRange(List<string> cidrList)
    {
        CIDR_List = cidrList;
    }

    public List<IpScannerResult> GetWorkingIPs
    {
        get => WorkingIPs;
    }

    public int GetAllIPsCount
    {
        get => AllIPs.Count;
    }

    public void Stop()
    {
        StopScan = true;
    }

    public void Start()
    {
        try
        {
            IsRunning = true;
            StopScan = false;
            if (AllIPs.Any()) AllIPs.Clear();

            Random random = new();

            Task.Run(async () =>
            {
                IPRange ipRange = new(CIDR_List);
                ipRange.StartGenerateIPs();
                await Task.Delay(100);

                int pauseDelayMs = 1;
                int startIndex = 0;
                while (true)
                {
                    if (StopScan)
                    {
                        ipRange.Dispose();
                        IsRunning = false;
                        return;
                    }

                    ipRange.Pause(true);
                    await Task.Delay(pauseDelayMs);

                    AllIPs = ipRange.IPs.GetRange(startIndex, ipRange.IPs.Count - startIndex);
                    startIndex = ipRange.IPs.Count;

                    for (int n = 0; n < AllIPs.Count; n++)
                    {
                        if (StopScan)
                        {
                            ipRange.Dispose();
                            IsRunning = false;
                            return;
                        }

                        OnNumberOfCheckedIpChanged?.Invoke(n, EventArgs.Empty);

                        int percent = 0;
                        if (n > 0 && n < AllIPs.Count - 1)
                            percent = (n * 100) / AllIPs.Count;
                        if (n == AllIPs.Count - 1)
                            percent = 100;
                        OnPercentChanged?.Invoke(percent, EventArgs.Empty);

                        string ipOut = AllIPs[n].ToString();

                        if (RandomScan)
                        {
                            int rn = random.Next(AllIPs.Count);
                            ipOut = AllIPs[rn].ToString();
                        }

                        OnNewIpCheck?.Invoke(ipOut, EventArgs.Empty);
                        OnFullReportChanged?.Invoke($"Checking: {ipOut} ({n} of {AllIPs.Count}) {percent}%", EventArgs.Empty);

                        // Real Delay
                        int realDelayOut = -1;
                        try
                        {
                            // Real Delay
                            string urlScheme = string.Empty;
                            if (CheckWebsite.Contains("://"))
                            {
                                string[] split = CheckWebsite.Split("://");
                                urlScheme = $"{split[0].Trim().ToLower()}://";
                            }
                            NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(CheckWebsite, CheckPort);
                            string url = $"{urlScheme}{urid.Host}:{CheckPort}";

                            Stopwatch realDelay = new();
                            realDelay.Start();
                            HttpStatusCode hsc = await NetworkTool.GetHttpStatusCodeAsync(url, ipOut, Timeout, true, false, false);
                            realDelay.Stop();

                            Debug.WriteLine("HttpStatusCode: " + hsc);

                            if (hsc == HttpStatusCode.OK)
                                realDelayOut = Convert.ToInt32(realDelay.ElapsedMilliseconds);

                            realDelay.Reset();
                        }
                        catch (Exception)
                        {
                            realDelayOut = -1;
                        }

                        Debug.WriteLine("Real Delay: " + realDelayOut);

                        if (realDelayOut != -1)
                        {
                            // Ping Delay
                            int pingDelayOut = -1;
                            try
                            {
                                Stopwatch pingDelay = new();
                                pingDelay.Start();
                                bool canPing = await NetworkTool.CanPingAsync(ipOut, Timeout);
                                pingDelay.Stop();

                                if (canPing) pingDelayOut = Convert.ToInt32(pingDelay.ElapsedMilliseconds);
                                pingDelay.Reset();
                            }
                            catch (Exception)
                            {
                                pingDelayOut = -1;
                            }

                            // Tcp delay
                            int tcpDelayOut = -1;
                            try
                            {
                                Stopwatch tcpDelay = new();
                                tcpDelay.Start();
                                bool canTcpConnect = await NetworkTool.CanTcpConnectAsync(ipOut, CheckPort, Timeout);
                                tcpDelay.Stop();

                                if (canTcpConnect) tcpDelayOut = Convert.ToInt32(tcpDelay.ElapsedMilliseconds);
                                tcpDelay.Reset();
                            }
                            catch (Exception)
                            {
                                tcpDelayOut = -1;
                            }

                            // Result
                            if (tcpDelayOut != -1 && pingDelayOut != -1)
                            {
                                IpScannerResult scannerResult = new()
                                {
                                    IP = ipOut,
                                    RealDelay = realDelayOut,
                                    TcpDelay = tcpDelayOut,
                                    PingDelay = pingDelayOut
                                };

                                OnWorkingIpReceived?.Invoke(scannerResult, EventArgs.Empty);
                                WorkingIPs.Add(scannerResult);
                            }
                        }

                    }

                    AllIPs.Clear();
                    ipRange.Pause(false);
                    await Task.Delay(pauseDelayMs);
                    if (!ipRange.IsRunning) StopScan = true;
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine("IpScanner Start: " + ex.Message);
        }
    }
}