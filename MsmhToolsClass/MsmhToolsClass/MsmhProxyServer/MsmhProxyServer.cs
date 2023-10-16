using MsmhToolsClass.ProxyServerPrograms;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
// CopyRight GPLv3 MSasanMH 2023

namespace MsmhToolsClass.MsmhProxyServer;

public partial class MsmhProxyServer
{
    //======================================= DPI Bypass Support: Static
    public static ProxyProgram.DPIBypass StaticDPIBypassProgram { get; set; } = new();
    public void EnableStaticDPIBypass(ProxyProgram.DPIBypass dpiBypassProgram)
    {
        StaticDPIBypassProgram = dpiBypassProgram;
    }

    //--- Constant
    public ProxyProgram.DPIBypass DPIBypassProgram = new();
    public void EnableDPIBypass(ProxyProgram.DPIBypass dpiBypassProgram)
    {
        DPIBypassProgram = dpiBypassProgram;
    }

    //======================================= UpStream Proxy Support
    public ProxyProgram.UpStreamProxy UpStreamProxyProgram = new();
    public void EnableUpStreamProxy(ProxyProgram.UpStreamProxy upStreamProxyProgram)
    {
        UpStreamProxyProgram = upStreamProxyProgram;
    }

    //======================================= DNS Support
    public ProxyProgram.Dns DNSProgram = new();
    public void EnableDNS(ProxyProgram.Dns dnsProgram)
    {
        DNSProgram = dnsProgram;
    }

    //======================================= Fake DNS Support
    public ProxyProgram.FakeDns FakeDNSProgram = new();
    public void EnableFakeDNS(ProxyProgram.FakeDns fakeDnsProgram)
    {
        FakeDNSProgram = fakeDnsProgram;
    }

    //======================================= Black White List Support
    public ProxyProgram.BlackWhiteList BWListProgram = new();
    public void EnableBlackWhiteList(ProxyProgram.BlackWhiteList blackWhiteListProgram)
    {
        BWListProgram = blackWhiteListProgram;
    }

    //======================================= DontBypass Support
    public ProxyProgram.DontBypass DontBypassProgram = new();
    public void EnableDontBypass(ProxyProgram.DontBypass dontBypassProgram)
    {
        DontBypassProgram = dontBypassProgram;
    }

    //======================================= Start Proxy
    internal ProxySettings ProxySettings_ = new();

    internal TunnelManager TunnelManager_ = new();
    private TcpListener? TcpListener_;

    private CancellationTokenSource? CancelTokenSource_;
    private CancellationToken CancelToken_;

    private System.Timers.Timer KillOnOverloadTimer { get; set; } = new(5000);
    private float CpuUsage { get; set; } = 0;

    private bool Cancel { get; set; } = false;

    private Thread? MainThread;

    public event EventHandler<EventArgs>? OnRequestReceived;
    public event EventHandler<EventArgs>? OnErrorOccurred;
    public event EventHandler<EventArgs>? OnDebugInfoReceived;
    public static readonly int MaxDataSize = 65536;
    public Stats Stats { get; private set; } = new();
    public bool IsRunning { get; private set; } = false;

    public MsmhProxyServer()
    {
        // Set Defult DNS to System DNS
        ProxyProgram.Dns defaultDns = new();
        defaultDns.Set(ProxyProgram.Dns.Mode.System, null, null, 2);
        DNSProgram = defaultDns;
    }

    public void Start(IPAddress ipAddress, int port, int maxThreads, int requestTimeoutSec, float killOnCpuUsage, bool blockPort80)
    {
        ProxySettings_ = new();
        ProxySettings_.ListenerIpAddress = ipAddress;
        ProxySettings_.ListenerPort = port;
        ProxySettings_.MaxThreads = maxThreads;
        ProxySettings_.RequestTimeoutSec = requestTimeoutSec;
        ProxySettings_.KillOnCpuUsage = killOnCpuUsage;
        ProxySettings_.BlockPort80 = blockPort80;

        Start(ProxySettings_);
    }

    public void Start(ProxySettings proxySettings)
    {
        if (IsRunning) return;
        IsRunning = true;

        ProxySettings_ = proxySettings;

        Stats = new Stats();

        Welcome();

        TunnelManager_ = new();

        CancelTokenSource_ = new();
        CancelToken_ = CancelTokenSource_.Token;

        Cancel = false;

        KillOnOverloadTimer.Elapsed += KillOnOverloadTimer_Elapsed;
        KillOnOverloadTimer.Start();

        ThreadStart threadStart = new(AcceptConnections);
        MainThread = new(threadStart);
        if (OperatingSystem.IsWindows())
            MainThread.SetApartmentState(ApartmentState.STA);
        MainThread.Start();
    }

    private async void KillOnOverloadTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (OperatingSystem.IsWindows() && typeof(PerformanceCounter) != null)
            CpuUsage = await ProcessManager.GetCpuUsage(Environment.ProcessId, 1000);

        if (ActiveTunnels >= ProxySettings_.MaxThreads || CpuUsage > ProxySettings_.KillOnCpuUsage)
        {
            KillAll();
        }
    }

    /// <summary>
    /// Kill all active requests
    /// </summary>
    public void KillAll()
    {
        if (TunnelManager_ != null)
        {
            var dic = TunnelManager_.GetTunnels();
            Debug.WriteLine(dic.Count);
            foreach (var item in dic)
            {
                Debug.WriteLine(item.Key);
                TunnelManager_.Remove(item.Key);
            }
        }
    }

    public void Stop()
    {
        if (IsRunning && TcpListener_ != null && CancelTokenSource_ != null)
        {
            IsRunning = false;
            CancelTokenSource_.Cancel(true);
            Cancel = true;
            TcpListener_.Stop();

            KillAll();

            KillOnOverloadTimer.Stop();

            IsRunning = TcpListener_.Server.IsBound;
            Goodbye();
        }
    }

    public int ListeningPort
    {
        get => ProxySettings_.ListenerPort;
    }

    public bool IsDpiBypassActive
    {
        get => DPIBypassProgram.DPIBypassMode != ProxyProgram.DPIBypass.Mode.Disable || StaticDPIBypassProgram.DPIBypassMode != ProxyProgram.DPIBypass.Mode.Disable;
    }

    public int ActiveTunnels
    {
        get => TunnelManager_.Count;
    }

    public int MaxRequests
    {
        get => ProxySettings_ != null ? ProxySettings_.MaxThreads : 0;
    }

    private void Welcome()
    {
        // Event
        string msgEvent = $"Proxy Server starting on {ProxySettings_.ListenerIpAddress}:{ProxySettings_.ListenerPort}";
        OnRequestReceived?.Invoke(msgEvent, EventArgs.Empty);
        OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);
    }

    private void Goodbye()
    {
        // Event
        string msgEvent = "Proxy Server stopped.";
        OnRequestReceived?.Invoke(msgEvent, EventArgs.Empty);
        OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);
    }

    private async void AcceptConnections()
    {
        if (Cancel) return;

        try
        {
            TcpListener_ = new(ProxySettings_.ListenerIpAddress, ProxySettings_.ListenerPort);
            TcpListener_.Start();
            Task.Delay(200).Wait();

            if (TcpListener_ != null)
            {
                IsRunning = TcpListener_.Server.IsBound;

                while (!Cancel)
                {
                    TcpClient tcpClient = await TcpListener_.AcceptTcpClientAsync().ConfigureAwait(false);
                    if (tcpClient.Connected) ProcessConnectionSync(tcpClient);
                    if (CancelToken_.IsCancellationRequested || Cancel)
                        break;
                }
            }
            else
            {
                IsRunning = false;
            }
        }
        catch (Exception ex)
        {
            // Event Error
            if (!CancelToken_.IsCancellationRequested || !Cancel)
            {
                string msgEventErr = $"Accept Connections: {ex.Message}";
                OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
            }
        }
    }

    private void ProcessConnectionSync(TcpClient tcpClient)
    {
        Task.Run(() => ClientConnected(tcpClient));
    }

    private async void ClientConnected(TcpClient tcpClient)
    {
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

        tcpClient.NoDelay = true;

        // Create Client
        ProxyClient proxyClient = new(tcpClient.Client);

        // Get First Packet
        GetFirstPacket getFirstPacket = new();
        await getFirstPacket.Get(proxyClient);

        if (getFirstPacket.ProxyName == null)
        {
            proxyClient.Disconnect();
            return;
        }

        // Create Request
        ProxyRequest? req = null;
        if (getFirstPacket.ProxyName == Proxy.Name.HTTP || getFirstPacket.ProxyName == Proxy.Name.HTTPS)
            req = await ProxyRequest.RequestHttpRemote(getFirstPacket.Packet);
        if (getFirstPacket.ProxyName == Proxy.Name.Socks4)
            req = await ProxyRequest.RequestSocks4Remote(proxyClient, getFirstPacket.Packet);
        if (getFirstPacket.ProxyName == Proxy.Name.Socks5)
            req = await ProxyRequest.RequestSocks5Remote(proxyClient, getFirstPacket.Packet);

        req = await ApplyPrograms(req);

        if (req == null)
        {
            proxyClient.Disconnect();
            return;
        }

        // Create Tunnel
        ProxyTunnel proxyTunnel = new(connectionId, proxyClient, req, ProxySettings_);
        proxyTunnel.Open(UpStreamProxyProgram);

        proxyTunnel.OnTunnelDisconnected += ProxyTunnel_OnTunnelDisconnected;
        proxyTunnel.OnDataReceived += ProxyTunnel_OnDataReceived;

        TunnelManager_.Add(connectionId, proxyTunnel);
    }

    private void ProxyTunnel_OnTunnelDisconnected(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not ProxyTunnel st) return;

            if (st.KillOnTimeout.IsRunning)
            {
                st.KillOnTimeout.Reset();
                st.KillOnTimeout.Stop();
            }

            st.OnTunnelDisconnected -= ProxyTunnel_OnTunnelDisconnected;
            st.OnDataReceived -= ProxyTunnel_OnDataReceived;

            TunnelManager_.Remove(st.ConnectionId);
            Debug.WriteLine($"{st.Req.Address} disconnected");
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
                    if (t.Req.ApplyDpiBypass)
                        Send(e.Buffer, t);
                    else
                        await t.RemoteClient.SendAsync(e.Buffer);

                    lock (Stats)
                    {
                        Stats.AddBytes(e.Buffer.Length, ByteType.Sent);
                    }
                }

                t.KillOnTimeout.Restart();
                await t.Client.StartReceiveAsync();
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
                    await t.Client.SendAsync(e.Buffer);

                t.KillOnTimeout.Restart();
                await t.RemoteClient.StartReceiveAsync();
                t.KillOnTimeout.Restart();
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProxyTunnel_OnDataReceived: " + ex.Message);
        }
    }

    private void Send(byte[] data, ProxyTunnel t)
    {
        try
        {
            if (t.RemoteClient.Socket_ != null && t.RemoteClient.Socket_.Connected)
            {
                if (DPIBypassProgram.DPIBypassMode == ProxyProgram.DPIBypass.Mode.Disable)
                {
                    // Static
                    ProxyProgram.DPIBypass bp = StaticDPIBypassProgram;
                    bp.DestHostname = t.Req.Address;
                    bp.DestPort = t.Req.Port;
                    if (bp.DPIBypassMode == ProxyProgram.DPIBypass.Mode.Program)
                    {
                        ProxyProgram.DPIBypass.ProgramMode programMode = new(data, t.RemoteClient.Socket_);
                        programMode.Send(bp);
                    }
                    else
                        t.RemoteClient.Socket_.Send(data);
                }
                else
                {
                    // Const
                    ProxyProgram.DPIBypass bp = DPIBypassProgram;
                    bp.DestHostname = t.Req.Address;
                    bp.DestPort = t.Req.Port;
                    ProxyProgram.DPIBypass.ProgramMode programMode = new(data, t.RemoteClient.Socket_);
                    programMode.Send(bp);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Send: " + ex.Message);
        }
    }

}