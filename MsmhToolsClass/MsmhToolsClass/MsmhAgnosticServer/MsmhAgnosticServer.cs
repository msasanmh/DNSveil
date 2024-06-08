using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
// MSMH Agnostic Server - CopyRight GPLv3 MSasanMH (msasanmh@gmail.com) 2023 - 2024

namespace MsmhToolsClass.MsmhAgnosticServer;

public partial class MsmhAgnosticServer
{
    //======================================= Fragment Support
    public AgnosticProgram.Fragment FragmentProgram = new();
    public void EnableFragment(AgnosticProgram.Fragment fragmentProgram)
    {
        FragmentProgram = fragmentProgram;
    }

    //======================================= DnsRules Support
    public AgnosticProgram.DnsRules DnsRulesProgram = new();
    public void EnableDnsRules(AgnosticProgram.DnsRules dnsRules)
    {
        DnsRulesProgram = dnsRules;
    }

    //======================================= ProxyRules Support
    public AgnosticProgram.ProxyRules ProxyRulesProgram = new();
    public void EnableProxyRules(AgnosticProgram.ProxyRules proxyRules)
    {
        ProxyRulesProgram = proxyRules;
    }

    //======================================= DnsLimit Support
    public AgnosticProgram.DnsLimit DnsLimitProgram = new();
    public void EnableDnsLimit(AgnosticProgram.DnsLimit dnsLimit)
    {
        DnsLimitProgram = dnsLimit;
    }

    // ====================================== Const
    internal static readonly int MaxDataSize = 65536;
    internal static readonly string DnsMessageContentType = "application/dns-message";
    internal static readonly string ODnsMessageContentType = "application/oblivious-dns-message";
    internal static readonly int DNS_HEADER_LENGTH = 12;
    internal static readonly SslProtocols SSL_Protocols = SslProtocols.None | SslProtocols.Tls12 | SslProtocols.Tls13;
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    internal static bool Callback(object sender, X509Certificate? cert, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true;

    //======================================= Start Server
    internal AgnosticSettings Settings_ = new();
    public AgnosticSettingsSSL SettingsSSL_ { get; internal set; } = new(false);

    private Socket? UdpSocket_;
    private readonly DnsCache DnsCaches = new();
    private readonly ProxyRequestsCache ProxyRequestsCaches = new();
    private TcpListener? TcpListener_;
    internal TunnelManager TunnelManager_ = new();

    private CancellationTokenSource? CancelTokenSource_;
    private CancellationToken CancelToken_;
    private CancellationTokenSource CTS_PR = new();

    private System.Timers.Timer KillOnOverloadTimer { get; set; } = new(5000);
    private float CpuUsage { get; set; } = -1;

    private bool Cancel { get; set; } = false;

    private Thread? MainThread;

    public event EventHandler<EventArgs>? OnRequestReceived;
    public event EventHandler<EventArgs>? OnDebugInfoReceived;
    
    public Stats Stats { get; private set; } = new();
    public bool IsRunning { get; private set; } = false;

    private readonly ConcurrentQueue<DateTime> MaxRequestsQueue = new();
    private readonly ConcurrentDictionary<string, DateTime> DelinquentRequests = new();
    private readonly ConcurrentDictionary<string, (DateTime dt, bool applyFakeSNI, bool applyFragment)> TestRequests = new();
    internal static readonly int MaxRequestsDelay = 50;
    internal static readonly int MaxRequestsDivide = 20; // 20 * 50 = 1000 ms

    public MsmhAgnosticServer() { }

    public async Task EnableSSL(AgnosticSettingsSSL settingsSSL)
    {
        SettingsSSL_ = settingsSSL;
        await SettingsSSL_.Build().ConfigureAwait(false);
    }

    public void Start(AgnosticSettings settings)
    {
        if (IsRunning) return;
        IsRunning = true;

        Settings_ = settings;
        Settings_.Initialize();

        // Set Default DNSs
        if (Settings_.DNSs.Count == 0) Settings_.DNSs = AgnosticSettings.DefaultDNSs();

        Stats = new Stats();

        Welcome();

        TunnelManager_ = new();

        CancelTokenSource_ = new();
        CancelToken_ = CancelTokenSource_.Token;

        Cancel = false;

        MaxRequestsTimer();

        KillOnOverloadTimer.Elapsed += KillOnOverloadTimer_Elapsed;
        KillOnOverloadTimer.Start();

        ThreadStart threadStart = new(AcceptConnections);
        MainThread = new(threadStart);
        if (OperatingSystem.IsWindows()) MainThread.SetApartmentState(ApartmentState.STA);
        MainThread.Start();
    }

    private void Welcome()
    {
        // Event
        string msgEvent = $"Server Starting On Port: {Settings_.ListenerPort}";
        OnRequestReceived?.Invoke(msgEvent, EventArgs.Empty);
        OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);
    }

    private async void MaxRequestsTimer()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                if (Cancel) break;
                await Task.Delay(MaxRequestsDelay);

                try
                {
                    bool peek = MaxRequestsQueue.TryPeek(out DateTime dt);
                    if (peek)
                    {
                        double diff = (DateTime.UtcNow - dt).TotalMilliseconds;
                        if (diff > 1000 / MaxRequestsDivide) // Dequeue If Older Than 50 ms (1000 / 20)
                            MaxRequestsQueue.TryDequeue(out _);
                    }
                }
                catch (Exception) { }
            }
        });
    }

    private async void KillOnOverloadTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (OperatingSystem.IsWindows() && typeof(PerformanceCounter) != null)
            CpuUsage = await ProcessManager.GetCpuUsage(Environment.ProcessId, 1000);

        if (CpuUsage >= Settings_.KillOnCpuUsage && Settings_.KillOnCpuUsage > 0)
        {
            KillAll();
        }

        if (CpuUsage >= 95f)
        {
            try { Environment.Exit(0); } catch (Exception) { }
            await ProcessManager.KillProcessByPidAsync(Environment.ProcessId);
        }
    }

    /// <summary>
    /// Kill all active requests
    /// </summary>
    public void KillAll()
    {
        try
        {
            CTS_PR.Cancel();
            ProxyRequestsCaches.Clear();
            if (TunnelManager_ != null)
            {
                var dic = TunnelManager_.GetTunnels();
                Debug.WriteLine(dic.Count);
                foreach (var item in dic)
                {
                    Debug.WriteLine(item.Key);
                    TunnelManager_.Remove(item.Value.Value);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("MsmhAgnosticServer KillAll: " + ex.Message);
        }
    }

    public void Stop()
    {
        if (IsRunning && CancelTokenSource_ != null)
        {
            try
            {
                IsRunning = false;
                CancelTokenSource_.Cancel(true);
                Cancel = true;
                UdpSocket_?.Shutdown(SocketShutdown.Both);
                UdpSocket_?.Dispose();
                TcpListener_?.Stop();
                TcpListener_ = null;

                KillAll();

                MaxRequestsQueue.Clear();
                DelinquentRequests.Clear();
                TestRequests.Clear();

                KillOnOverloadTimer.Stop();
                Goodbye();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Stop MsmhAgnosticServer: " + ex.GetInnerExceptions());
            }
        }
    }

    private void Goodbye()
    {
        // Event
        string msgEvent = "Proxy Server Stopped.";
        OnRequestReceived?.Invoke(msgEvent, EventArgs.Empty);
        OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);
    }

    public int ListeningPort => Settings_.ListenerPort;
    public bool IsFragmentActive => FragmentProgram.FragmentMode != AgnosticProgram.Fragment.Mode.Disable;
    public bool IsFakeSniActive => SettingsSSL_.EnableSSL && SettingsSSL_.ChangeSni;
    public int ActiveProxyTunnels => TunnelManager_.Count;
    public int MaxRequests => Settings_.MaxRequests;
    public int CachedDnsRequests => DnsCaches.CachedRequests;

    public void FlushDnsCache()
    {
        DnsCaches.Flush();
    }

    private async void AcceptConnections()
    {
        if (Cancel) return;

        try
        {
            if (Settings_.ListenerIP == null)
            {
                string msg = "Neither IPv4 Nor IPv6 Is Supported By Your OS!";
                OnRequestReceived?.Invoke(msg, EventArgs.Empty);
                Stop();
                return;
            }

            // EndPoint
            IPEndPoint ipEndPoint = Settings_.ServerEndPoint;

            // Make Port Free
            if (OperatingSystem.IsWindows())
            {
                List<int> pids = ProcessManager.GetProcessPidsByUsingPort(Settings_.ListenerPort);
                foreach (int pid in pids) await ProcessManager.KillProcessByPidAsync(pid);
                await Task.Delay(5);
                pids = ProcessManager.GetProcessPidsByUsingPort(Settings_.ListenerPort);
                foreach (int pid in pids) await ProcessManager.KillProcessByPidAsync(pid);
            }

            // UDP
            UdpSocket_ = new(ipEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            if (ipEndPoint.Address.Equals(IPAddress.IPv6Any))
            {
                UdpSocket_.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
                UdpSocket_.DualMode = true;
            }
            UdpSocket_.Bind(ipEndPoint);

            // TCP
            TcpListener_ = new(ipEndPoint);
            SocketOptionName socketOptionName = SocketOptionName.ReuseAddress | SocketOptionName.ReuseUnicastPort;
            TcpListener_.Server.SetSocketOption(SocketOptionLevel.Socket, socketOptionName, true);
            if (ipEndPoint.Address.Equals(IPAddress.IPv6Any))
            {
                TcpListener_.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
                TcpListener_.Server.DualMode = true;
            }
            TcpListener_.Start();

            await Task.Delay(200);

            if (UdpSocket_ != null && TcpListener_ != null)
            {
                IsRunning = TcpListener_.Server.IsBound && UdpSocket_.IsBound;
                if (!IsRunning)
                {
                    UdpSocket_.Dispose();
                    TcpListener_.Stop();
                    Stop();
                    return;
                }

                // Run UDP & TCP Listener In Parallel
                udp(UdpSocket_);
                tcp(TcpListener_);

                // UDP
                async void udp(Socket udpSocket)
                {
                    while (!Cancel)
                    {
                        try
                        {
                            byte[] buffer = new byte[MaxDataSize];
                            SocketReceiveMessageFromResult udpMessage = await udpSocket.ReceiveMessageFromAsync(buffer, SocketFlags.None, ipEndPoint).ConfigureAwait(false);
                            if (CpuUsage < Settings_.KillOnCpuUsage || Settings_.KillOnCpuUsage <= 0)
                            {
                                if (udpMessage.ReceivedBytes > 0)
                                {
                                    buffer = buffer[..udpMessage.ReceivedBytes];
                                    //Debug.WriteLine(" UDP ===> " + BitConverter.ToString(buffer));
                                    AgnosticRequest agnosticRequest = new()
                                    {
                                        Udp_Socket = udpSocket,
                                        Local_EndPoint = udpSocket.LocalEndPoint as IPEndPoint,
                                        Remote_EndPoint = udpMessage.RemoteEndPoint as IPEndPoint,
                                        Peeked_Buffer = buffer,
                                        Protocol = AgnosticRequest.ListenerProtocol.UDP
                                    };

                                    ProcessConnectionSync(agnosticRequest);
                                }
                            }
                            if (CancelToken_.IsCancellationRequested || Cancel) break;
                        }
                        catch (Exception ex)
                        {
                            // Event Error
                            if (!CancelToken_.IsCancellationRequested || !Cancel)
                            {
                                string msgEventErr = $"ERROR: Accept Connections UDP: {ex.GetInnerExceptions()}";
                                //OnRequestReceived?.Invoke(msgEventErr, EventArgs.Empty);
                                Debug.WriteLine(msgEventErr);
                            }
                        }
                    }

                    Stop();
                }

                // TCP
                async void tcp(TcpListener tcpListener)
                {
                    while (!Cancel)
                    {
                        try
                        {
                            TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
                            if (CpuUsage < Settings_.KillOnCpuUsage || Settings_.KillOnCpuUsage <= 0)
                            {
                                byte[] buffer = new byte[MaxDataSize];
                                int received = await tcpClient.Client.ReceiveAsync(buffer, SocketFlags.Peek).ConfigureAwait(false);
                                if (received > 0)
                                {
                                    tcpClient.NoDelay = true;
                                    buffer = buffer[..received];
                                    //Debug.WriteLine(" TCP ===> " + BitConverter.ToString(buffer));
                                    AgnosticRequest agnosticRequest = new()
                                    {
                                        Tcp_Client = tcpClient,
                                        Local_EndPoint = tcpClient.Client.LocalEndPoint as IPEndPoint,
                                        Remote_EndPoint = tcpClient.Client.RemoteEndPoint as IPEndPoint,
                                        Peeked_Buffer = buffer,
                                        Protocol = AgnosticRequest.ListenerProtocol.TCP
                                    };

                                    ProcessConnectionSync(agnosticRequest);
                                }
                            }
                            if (CancelToken_.IsCancellationRequested || Cancel) break;
                        }
                        catch (Exception ex)
                        {
                            // Event Error
                            if (!CancelToken_.IsCancellationRequested || !Cancel)
                            {
                                string msgEventErr = $"ERROR: Accept Connections TCP: {ex.GetInnerExceptions()}";
                                //OnRequestReceived?.Invoke(msgEventErr, EventArgs.Empty);
                                Debug.WriteLine(msgEventErr);
                            }
                        }
                    }

                    Stop();
                }
            }
            else
            {
                IsRunning = false;
                UdpSocket_?.Dispose();
                TcpListener_?.Stop();
            }
        }
        catch (Exception ex)
        {
            // Event Error
            if (!CancelToken_.IsCancellationRequested || !Cancel)
            {
                string msgEventErr = $"ERROR: Accept Connections: {ex.GetInnerExceptions()}";
                Debug.WriteLine(msgEventErr);
                OnRequestReceived?.Invoke(msgEventErr, EventArgs.Empty);
                TcpListener_?.Stop();
                Stop();
            }
        }
    }

    private void ProcessConnectionSync(AgnosticRequest agnosticRequest)
    {
        Task.Run(() => ClientConnected(agnosticRequest));
    }

    private async void ClientConnected(AgnosticRequest aRequest)
    {
        if (aRequest.Local_EndPoint == null || aRequest.Remote_EndPoint == null)
        {
            aRequest.Disconnect();
            return;
        }

        // Count Max Requests
        try
        {
            MaxRequestsQueue.Enqueue(DateTime.UtcNow);
            if (Settings_.MaxRequests >= MaxRequestsDivide)
            {
                if (MaxRequestsQueue.Count >= Settings_.MaxRequests / MaxRequestsDivide) // Check for 50 ms (1000 / 20)
                {
                    // Event
                    string blockEvent = $"Recevied {MaxRequestsQueue.Count * MaxRequestsDivide} Requests Per Second - Request Denied Due To Max Requests of {Settings_.MaxRequests}.";
                    Debug.WriteLine("====================> " + blockEvent);
                    OnRequestReceived?.Invoke(blockEvent, EventArgs.Empty);
                    aRequest.Disconnect();
                    return;
                }
            }
        }
        catch (Exception) { }

        // Generate unique int
        int connectionId;
        try
        {
            connectionId = Guid.NewGuid().GetHashCode() + BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);
        }
        catch (Exception)
        {
            connectionId = Guid.NewGuid().GetHashCode();
        }
        
        AgnosticResult aResult = await aRequest.GetResultAsync(SettingsSSL_).ConfigureAwait(false);
        
        if (aResult.Socket == null || aResult.FirstBuffer.Length == 0 || aResult.Protocol == RequestProtocol.Unknown)
        {
            aRequest.Disconnect();
            return;
        }

        //Debug.WriteLine("Request ListenerProtocol ===> " + aResult.Protocol);
        //Debug.WriteLine("Request Buffer ===> " + BitConverter.ToString(aResult.FirstBuffer));
        
        if (aResult.Protocol == RequestProtocol.UDP ||
            aResult.Protocol == RequestProtocol.TCP ||
            aResult.Protocol == RequestProtocol.DoH)
        {
            // ===== Process DNS
            await DnsTunnel.Process(aResult, DnsRulesProgram, DnsLimitProgram, DnsCaches, Settings_, OnRequestReceived);
            aRequest.Disconnect();
        }
        else
        {
            // ===== Process Proxy
            if (Settings_.Working_Mode == AgnosticSettings.WorkingMode.DnsAndProxy)
            {
                // Create Client
                ProxyClient proxyClient = new(aResult.Socket);

                // Create Request
                CTS_PR = new();
                ProxyRequest? req = null;
                if (aResult.Protocol == RequestProtocol.HTTP_S)
                    req = await ProxyRequest.RequestHTTP_S(aResult.FirstBuffer, CTS_PR.Token).ConfigureAwait(false);
                else if (aResult.Protocol == RequestProtocol.SOCKS4_4A)
                    req = await ProxyRequest.RequestSocks4_4A(proxyClient, aResult.FirstBuffer, CTS_PR.Token).ConfigureAwait(false);
                else if (aResult.Protocol == RequestProtocol.SOCKS5)
                    req = await ProxyRequest.RequestSocks5(proxyClient, aResult.FirstBuffer, CTS_PR.Token).ConfigureAwait(false);
                else if (aResult.Protocol == RequestProtocol.SniProxy)
                    req = await ProxyRequest.RequestSniProxy(aResult.SNI, Settings_.ListenerPort, CTS_PR.Token).ConfigureAwait(false);

                // Apply Programs
                req = await ApplyPrograms(aResult.Local_EndPoint.Address, req).ConfigureAwait(false);

                if (req == null)
                {
                    aRequest.Disconnect();
                    return;
                }

                // Create Tunnel
                ProxyTunnel proxyTunnel = new(connectionId, proxyClient, req, Settings_, SettingsSSL_);
                proxyTunnel.Open(ProxyRulesProgram);

                proxyTunnel.OnTunnelDisconnected += ProxyTunnel_OnTunnelDisconnected;
                proxyTunnel.OnDataReceived += ProxyTunnel_OnDataReceived;

                TunnelManager_.Add(proxyTunnel);
            }
        }
    }

    private void ProxyTunnel_OnTunnelDisconnected(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not ProxyTunnel pt) return;

            if (pt.KillOnTimeout.IsRunning)
            {
                pt.KillOnTimeout.Reset();
                pt.KillOnTimeout.Stop();
            }

            pt.OnTunnelDisconnected -= ProxyTunnel_OnTunnelDisconnected;
            pt.OnDataReceived -= ProxyTunnel_OnDataReceived;
            pt.ClientSSL?.Disconnect();

            TunnelManager_.Remove(pt);
            Debug.WriteLine($"{pt.Req.Address} Disconnected");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProxyTunnel_OnTunnelDisconnected: " + ex.Message);
        }
    }

    private void ProxyTunnel_OnDataReceived(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not ProxyTunnel t) return;

            t.Client.OnDataReceived += async (s, e) =>
            {
                // Client Received == Remote Sent
                if (!t.KillOnTimeout.IsRunning) t.KillOnTimeout.Start();
                t.KillOnTimeout.Restart();
                if (e.Buffer.Length > 0)
                {
                    if (t.Req.ApplyFragment)
                        await SendAsync(e.Buffer, t);
                    else
                        await t.RemoteClient.SendAsync(e.Buffer).ConfigureAwait(false);

                    lock (Stats)
                    {
                        Stats.AddBytes(e.Buffer.Length, ByteType.Sent);
                    }
                }

                t.KillOnTimeout.Restart();
                await t.Client.StartReceiveAsync().ConfigureAwait(false);
                t.KillOnTimeout.Restart();
            };

            t.Client.OnDataSent += (s, e) =>
            {
                // Client Sent == Remote Received
                if (!t.KillOnTimeout.IsRunning) t.KillOnTimeout.Start();
                t.KillOnTimeout.Restart();
                lock (Stats)
                {
                    Stats.AddBytes(e.Buffer.Length, ByteType.Received);
                }
            };

            t.RemoteClient.OnDataReceived += async (s, e) =>
            {
                t.KillOnTimeout.Restart();

                if (e.Buffer.Length > 0)
                    await t.Client.SendAsync(e.Buffer).ConfigureAwait(false);

                t.KillOnTimeout.Restart();
                await t.RemoteClient.StartReceiveAsync().ConfigureAwait(false);
                t.KillOnTimeout.Restart();
            };
            
            // Handle SSL
            if (t.ClientSSL != null)
            {
                t.ClientSSL.OnClientDataReceived += (s, e) =>
                {
                    t.KillOnTimeout.Restart();

                    if (e.Buffer.Length > 0)
                    {
                        // Can't be implement here. Ex: "The WriteAsync method cannot be called when another write operation is pending"
                        //await t.ClientSSL.RemoteStream.WriteAsync(e.Buffer).ConfigureAwait(false);

                        lock (Stats)
                        {
                            Stats.AddBytes(e.Buffer.Length, ByteType.Sent);
                        }
                    }
                    t.ClientSSL.OnClientDataReceived -= null;
                };

                t.ClientSSL.OnRemoteDataReceived += (s, e) =>
                {
                    t.KillOnTimeout.Restart();

                    if (e.Buffer.Length > 0)
                    {
                        // Can't be implement here. Ex: "The WriteAsync method cannot be called when another write operation is pending"
                        //await t.ClientSSL.ClientStream.WriteAsync(e.Buffer).ConfigureAwait(false);

                        lock (Stats)
                        {
                            Stats.AddBytes(e.Buffer.Length, ByteType.Received);
                        }
                    }
                    t.ClientSSL.OnRemoteDataReceived -= null;
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProxyTunnel_OnDataReceived: " + ex.Message);
        }
    }

    private async Task SendAsync(byte[] data, ProxyTunnel t)
    {
        try
        {
            if (t.RemoteClient.Socket_ != null && t.RemoteClient.Socket_.Connected)
            {
                AgnosticProgram.Fragment bp = FragmentProgram;
                bp.DestHostname = t.Req.Address;
                bp.DestPort = t.Req.Port;
                if (bp.FragmentMode == AgnosticProgram.Fragment.Mode.Program)
                {
                    AgnosticProgram.Fragment.ProgramMode programMode = new(data, t.RemoteClient.Socket_);
                    await programMode.SendAsync(bp);
                }
                else
                    await t.RemoteClient.Socket_.SendAsync(data, SocketFlags.None).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Send: " + ex.Message);
        }
    }

}