using MsmhToolsClass.ProxifiedClients;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class ProxyTunnel
{
    public readonly int ConnectionId;
    public ProxyClient Client;
    public ProxyClient Remote;
    public ProxyRequest Req;

    private TcpClient? ProxifiedTcpClient_;

    public AgnosticProgram.Fragment FragmentProgram { get; set; }
    public readonly AgnosticSettingsSSL SettingsSSL_;

    public ProxyRelay? ProxyRelay;
    public ProxyRelayMITM? ProxyRelayMITM;

    public event EventHandler<ProxyTunnelEventArgs>? OnClientDataReceived;
    public event EventHandler<ProxyTunnelEventArgs>? OnClientDataSent;
    public event EventHandler<ProxyTunnelEventArgs>? OnRemoteDataReceived;
    public event EventHandler<ProxyTunnelEventArgs>? OnRemoteDataSent;
    public event EventHandler<EventArgs>? OnTunnelDisconnected;

    public readonly Stopwatch KillOnTimeout = new();
    public bool ManualDisconnect { get; set; } = false;
    
    public ProxyTunnel(int connectionId, ProxyClient pc, ProxyRequest req, AgnosticProgram.Fragment fp, AgnosticSettingsSSL settingsSSL)
    {
        ConnectionId = connectionId;
        Client = pc;
        Req = req;
        FragmentProgram = fp;
        SettingsSSL_ = settingsSSL;
        
        try
        {
            // Default Remote / SOCKS4/4A Remote
            Socket remoteSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // HTTP/HTTP_S/HTTPS_SSL/SNI Remote
            if (Req.ProxyName == Proxy.Name.Test || Req.ProxyName == Proxy.Name.HTTP || Req.ProxyName == Proxy.Name.HTTP_S || Req.ProxyName == Proxy.Name.HTTPS_SSL || Req.ProxyName == Proxy.Name.SniProxy)
            {
                // TCP Ipv4
                if (Req.AddressType == Socks.AddressType.Domain || Req.AddressType == Socks.AddressType.Ipv4)
                    remoteSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // TCP Ipv6
                if (Req.AddressType == Socks.AddressType.Ipv6)
                    remoteSocket = new(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

                // Only For Stream SocketType
                Client.Socket_.NoDelay = true;
                remoteSocket.NoDelay = true;
            }

            // SOCKS5 Remote
            if (Req.ProxyName == Proxy.Name.Socks5)
            {
                if (Req.Command == Socks.Commands.Connect || Req.Command == Socks.Commands.Bind)
                {
                    // TCP Ipv4
                    if (Req.AddressType == Socks.AddressType.Domain || Req.AddressType == Socks.AddressType.Ipv4)
                        remoteSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    // TCP Ipv6
                    if (Req.AddressType == Socks.AddressType.Ipv6)
                        remoteSocket = new(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

                    // Only For Stream SocketType
                    Client.Socket_.NoDelay = true;
                    remoteSocket.NoDelay = true;
                }

                if (Req.Command == Socks.Commands.UDP)
                {
                    // UDP IPv4
                    if (Req.AddressType == Socks.AddressType.Domain || Req.AddressType == Socks.AddressType.Ipv4)
                        remoteSocket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    // UDP IPv6
                    if (Req.AddressType == Socks.AddressType.Ipv6)
                        remoteSocket = new(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                }
            }

            Remote = new ProxyClient(remoteSocket, null);

            KillOnTimeoutCheck();
        }
        catch (Exception ex)
        {
            Remote = new ProxyClient(Client.Socket_, null);
            Debug.WriteLine("=================> ProxyTunnel: " + ex.Message);
            OnTunnelDisconnected?.Invoke(this, EventArgs.Empty);
            return;
        }
    }

    private async void KillOnTimeoutCheck()
    {
        await Task.Run(async () =>
        {
            while(true)
            {
                await Task.Delay(2000);
                if (Req.TimeoutSec > 0 &&
                    KillOnTimeout.ElapsedMilliseconds > TimeSpan.FromSeconds(Req.TimeoutSec).TotalMilliseconds)
                {
                    //string msg = $"Killed Request On Timeout({Req.TimeoutSec} Sec): {Req.AddressOrig}:{Req.Port}";
                    //Debug.WriteLine(msg);

                    OnTunnelDisconnected?.Invoke(this, EventArgs.Empty);
                    ProxifiedTcpClient_?.Close();
                    break;
                }

                // Manual Disconnect
                if (ManualDisconnect)
                {
                    OnTunnelDisconnected?.Invoke(this, EventArgs.Empty);
                    ProxifiedTcpClient_?.Close();
                    break;
                }
            }
        });
    }

    public async void Open()
    {
        try
        {
            if (string.IsNullOrEmpty(Req.Address) || Req.Port <= -1)
            {
                OnTunnelDisconnected?.Invoke(this, EventArgs.Empty);
                return;
            }
            
            if (Req.TimeoutSec > 0 && !KillOnTimeout.IsRunning) KillOnTimeout.Start();

            if (Req.ProxyName == Proxy.Name.HTTP)
            {
                await HttpHandler().ConfigureAwait(false);
                return;
            }
            
            if (Req.ProxyName == Proxy.Name.Socks5 && Req.Status != Socks.Status.Granted)
            {
                // Send Connection Request Frame
                await Client.SendAsync(Req.GetConnectionRequestFrameData()).ConfigureAwait(false);
                OnTunnelDisconnected?.Invoke(this, EventArgs.Empty);
                return;
            }
            
            // CONNECT
            if (Req.ProxyName == Proxy.Name.Test ||
                Req.ProxyName == Proxy.Name.HTTP_S ||
                Req.ProxyName == Proxy.Name.HTTPS_SSL ||
                Req.ProxyName == Proxy.Name.SniProxy ||
                (Req.ProxyName == Proxy.Name.Socks4 && Req.Command == Socks.Commands.Connect) ||
                (Req.ProxyName == Proxy.Name.Socks4A && Req.Command == Socks.Commands.Connect) ||
                (Req.ProxyName == Proxy.Name.Socks5 && Req.Command == Socks.Commands.Connect))
            {
                // Only CONNECT Can Support Upstream
                bool upStreamProxyApplied = false;
                
                if (Req.ApplyUpstreamProxy && !string.IsNullOrWhiteSpace(Req.UpstreamProxyScheme))
                {
                    ProxifiedTcpClient proxifiedTcpClient = new(Req.UpstreamProxyScheme, Req.UpstreamProxyUser, Req.UpstreamProxyPass);
                    var upstream = await proxifiedTcpClient.TryGetConnectedProxifiedTcpClient(Req.Address, Req.Port).ConfigureAwait(false);
                    if (upstream.isSuccess && upstream.proxifiedTcpClient != null)
                        ProxifiedTcpClient_ = upstream.proxifiedTcpClient;
                    
                    if (ProxifiedTcpClient_ != null)
                    {
                        upStreamProxyApplied = true;
                        Remote.Socket_ = ProxifiedTcpClient_.Client;
                    }
                }

                if (!upStreamProxyApplied)
                {
                    await Remote.Socket_.ConnectAsync(Req.Address, Req.Port).ConfigureAwait(false);
                }

                ConnectHandler();
            }

            // Bind
            if ((Req.ProxyName == Proxy.Name.Socks4 && Req.Command == Socks.Commands.Bind) ||
                (Req.ProxyName == Proxy.Name.Socks4A && Req.Command == Socks.Commands.Bind) ||
                (Req.ProxyName == Proxy.Name.Socks5 && Req.Command == Socks.Commands.Bind))
            {
                // ReuseAddress: We Only Need It If We Want To Bind To Specific Local Port That Might Be In TIME_WAIT.
                // ReuseAddress: No Need To Set For A Temporary Outbound Socket.
                // ReuseUnicastPort: Let Multiple Sockets Share The Same Local Port For Load Balancing On Windows 8+. Does Nothing On Linux/Android.
                // ReuseUnicastPort: It's Not Necessary For Normal Client Sockets.
                SocketOptionName socketOptionName = SocketOptionName.ReuseAddress; // Cross Platform.
                Remote.Socket_.SetSocketOption(SocketOptionLevel.Socket, socketOptionName, true);

                if (Remote.Socket_.AddressFamily == AddressFamily.InterNetworkV6)
                    Remote.Socket_.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));
                else
                    Remote.Socket_.Bind(new IPEndPoint(IPAddress.Any, 0));

                ConnectHandler();
            }

            // UDP (Only SOCKS5 Supports UDP)
            if (Req.ProxyName == Proxy.Name.Socks5 && Req.Command == Socks.Commands.UDP)
            {
                SocketOptionName socketOptionName = SocketOptionName.ReuseAddress | SocketOptionName.ReuseUnicastPort;
                Remote.Socket_.SetSocketOption(SocketOptionLevel.Socket, socketOptionName, true);

                if (Remote.Socket_.AddressFamily == AddressFamily.InterNetworkV6)
                    Remote.Socket_.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));
                else
                    Remote.Socket_.Bind(new IPEndPoint(IPAddress.Any, 0));

                ConnectHandler();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProxyTunnel Open: " + ex.Message);
            OnTunnelDisconnected?.Invoke(this, EventArgs.Empty);
            ProxifiedTcpClient_?.Close();
            return;
        }
    }

    private async void ConnectHandler()
    {
        try
        {
            // HTTP_S/HTTPS_SSL Response
            string resp = "HTTP/1.1 200 Connection Established\r\nConnection: close\r\n\r\n";
            byte[] httpsResponse = Encoding.UTF8.GetBytes(resp);

            if (Req.ProxyName == Proxy.Name.Test || Req.ProxyName == Proxy.Name.HTTP_S)
            {
                await Client.SendAsync(httpsResponse).ConfigureAwait(false);
            }

            if (Req.ProxyName == Proxy.Name.HTTPS_SSL && Client.SslStream_ != null)
            {
                await Client.SslStream_.WriteAsync(httpsResponse).ConfigureAwait(false);
            }

            // SOCKS5 Response
            if (Req.ProxyName == Proxy.Name.Socks5)
            {
                // Send Connection Request Frame To Server
                byte[] request = Req.GetConnectionRequestFrameData();
                await Client.SendAsync(request).ConfigureAwait(false);
            }

            // Receive Data From Both EndPoints
            bool mitm = Req.ApplyChangeSNI && !string.IsNullOrWhiteSpace(Req.AddressSNI) && !Req.AddressSNI.Equals(Req.AddressOrig) && !Req.AddressIsIp; // Cert Can't Be Valid When There's An IP Without A Domain. Like SOCKS4
            
            if (mitm || Req.ProxyName == Proxy.Name.HTTPS_SSL)
            {
                // SSL Relay - With MITM To Support Fake SNI
                ProxyRelayMITM = new(this, mitm);
                ProxyRelayMITM.OnClientDataReceived -= ProxyRelayMITM_OnClientDataReceived;
                ProxyRelayMITM.OnClientDataReceived += ProxyRelayMITM_OnClientDataReceived;
                ProxyRelayMITM.OnClientDataSent -= ProxyRelayMITM_OnClientDataSent;
                ProxyRelayMITM.OnClientDataSent += ProxyRelayMITM_OnClientDataSent;
                ProxyRelayMITM.OnRemoteDataReceived -= ProxyRelayMITM_OnRemoteDataReceived;
                ProxyRelayMITM.OnRemoteDataReceived += ProxyRelayMITM_OnRemoteDataReceived;
                ProxyRelayMITM.OnRemoteDataSent -= ProxyRelayMITM_OnRemoteDataSent;
                ProxyRelayMITM.OnRemoteDataSent += ProxyRelayMITM_OnRemoteDataSent;
                await ProxyRelayMITM.ExecuteAsync().ConfigureAwait(false);
            }
            else
            {
                // HTTP_S Relay - Supports Fragment
                ProxyRelay = new(this);
                ProxyRelay.OnClientDataReceived -= ProxyRelay_OnClientDataReceived;
                ProxyRelay.OnClientDataReceived += ProxyRelay_OnClientDataReceived;
                ProxyRelay.OnClientDataSent -= ProxyRelay_OnClientDataSent;
                ProxyRelay.OnClientDataSent += ProxyRelay_OnClientDataSent;
                ProxyRelay.OnRemoteDataReceived -= ProxyRelay_OnRemoteDataReceived;
                ProxyRelay.OnRemoteDataReceived += ProxyRelay_OnRemoteDataReceived;
                ProxyRelay.OnRemoteDataSent -= ProxyRelay_OnRemoteDataSent;
                ProxyRelay.OnRemoteDataSent += ProxyRelay_OnRemoteDataSent;
                await ProxyRelay.ExecuteAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProxyTunnel ConnectHandler: " + ex.Message);
            OnTunnelDisconnected?.Invoke(this, EventArgs.Empty);
            ProxifiedTcpClient_?.Close();
        }
    }

    private async Task HttpHandler()
    {
        // Support Upstream Proxy For HTTP Get Method
        if (Req.ApplyUpstreamProxy && !string.IsNullOrWhiteSpace(Req.UpstreamProxyScheme))
        {
            Req.HttpRequest.ProxyScheme = Req.UpstreamProxyScheme;
            Req.HttpRequest.ProxyUser = Req.UpstreamProxyUser;
            Req.HttpRequest.ProxyPass = Req.UpstreamProxyPass;
        }
        
        HttpRequestResponse hrr = await HttpRequest.SendAsync(Req.HttpRequest).ConfigureAwait(false);
        
        if (hrr.IsSuccess)
        {
            try
            {
                List<byte> bufferList = new();
                string statusLine = hrr.ProtocolVersion + " " + hrr.StatusCodeNumber + " " + hrr.StatusDescription + "\r\n";
                bufferList.AddRange(Encoding.UTF8.GetBytes(statusLine));

                if (!string.IsNullOrEmpty(hrr.ContentType))
                {
                    string contentTypeLine = "Content-Type: " + hrr.ContentType + "\r\n";
                    bufferList.AddRange(Encoding.UTF8.GetBytes(contentTypeLine));
                }

                if (hrr.ContentLength > 0)
                {
                    string contentLenLine = "Content-Length: " + hrr.ContentLength + "\r\n";
                    bufferList.AddRange(Encoding.UTF8.GetBytes(contentLenLine));
                }

                for (int n = 0; n < hrr.Headers.Count; n++)
                {
                    string? key = hrr.Headers.GetKey(n);
                    string? val = hrr.Headers.Get(n);

                    if (string.IsNullOrEmpty(key)) continue;
                    if (string.IsNullOrEmpty(val)) continue;

                    if (key.ToLower().Trim().Equals("content-type")) continue;
                    if (key.ToLower().Trim().Equals("content-length")) continue;

                    string headerLine = key + ": " + val + "\r\n";
                    bufferList.AddRange(Encoding.UTF8.GetBytes(headerLine));
                }

                bufferList.AddRange(Encoding.UTF8.GetBytes("\r\n"));
                
                // Merge Headers And Body
                bufferList.AddRange(hrr.Data);

                // Send
                bool isSent = await Client.SendAsync(bufferList.ToArray()).ConfigureAwait(false);

                if (isSent)
                {
                    // Relay
                    ProxyRelay = new(this);
                    ProxyRelay.OnClientDataReceived -= ProxyRelay_OnClientDataReceived;
                    ProxyRelay.OnClientDataReceived += ProxyRelay_OnClientDataReceived;
                    ProxyRelay.OnClientDataSent -= ProxyRelay_OnClientDataSent;
                    ProxyRelay.OnClientDataSent += ProxyRelay_OnClientDataSent;
                    ProxyRelay.OnRemoteDataReceived -= ProxyRelay_OnRemoteDataReceived;
                    ProxyRelay.OnRemoteDataReceived += ProxyRelay_OnRemoteDataReceived;
                    ProxyRelay.OnRemoteDataSent -= ProxyRelay_OnRemoteDataSent;
                    ProxyRelay.OnRemoteDataSent += ProxyRelay_OnRemoteDataSent;
                    await ProxyRelay.ExecuteAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ProxyServer HttpHandler: " + ex.Message);
            }
        }
    }

    private void ProxyRelay_OnClientDataReceived(object? sender, ProxyRelayEventArgs e)
    {
        ProxyTunnelEventArgs ea = new(this, e.Buffer);
        OnClientDataReceived?.Invoke(this, ea);
    }

    private void ProxyRelay_OnClientDataSent(object? sender, ProxyRelayEventArgs e)
    {
        ProxyTunnelEventArgs ea = new(this, e.Buffer);
        OnClientDataSent?.Invoke(this, ea);
    }

    private void ProxyRelay_OnRemoteDataReceived(object? sender, ProxyRelayEventArgs e)
    {
        ProxyTunnelEventArgs ea = new(this, e.Buffer);
        OnRemoteDataReceived?.Invoke(this, ea);
    }

    private void ProxyRelay_OnRemoteDataSent(object? sender, ProxyRelayEventArgs e)
    {
        ProxyTunnelEventArgs ea = new(this, e.Buffer);
        OnRemoteDataSent?.Invoke(this, ea);
    }

    private void ProxyRelayMITM_OnClientDataReceived(object? sender, ProxyRelayMITMEventArgs e)
    {
        ProxyTunnelEventArgs ea = new(this, e.Buffer);
        OnClientDataReceived?.Invoke(this, ea);
    }

    private void ProxyRelayMITM_OnClientDataSent(object? sender, ProxyRelayMITMEventArgs e)
    {
        ProxyTunnelEventArgs ea = new(this, e.Buffer);
        OnClientDataSent?.Invoke(this, ea);
    }

    private void ProxyRelayMITM_OnRemoteDataReceived(object? sender, ProxyRelayMITMEventArgs e)
    {
        ProxyTunnelEventArgs ea = new(this, e.Buffer);
        OnRemoteDataReceived?.Invoke(this, ea);
    }

    private void ProxyRelayMITM_OnRemoteDataSent(object? sender, ProxyRelayMITMEventArgs e)
    {
        ProxyTunnelEventArgs ea = new(this, e.Buffer);
        OnRemoteDataSent?.Invoke(this, ea);
    }

    public void Disconnect()
    {
        if (ProxyRelay != null)
        {
            try
            {
                ProxyRelay.OnClientDataReceived -= ProxyRelay_OnClientDataReceived;
                ProxyRelay.OnClientDataSent -= ProxyRelay_OnClientDataSent;
                ProxyRelay.OnRemoteDataReceived -= ProxyRelay_OnRemoteDataReceived;
                ProxyRelay.OnRemoteDataSent -= ProxyRelay_OnRemoteDataSent;
            }
            catch (Exception) { }
        }

        if (ProxyRelayMITM != null)
        {
            try
            {
                ProxyRelayMITM.OnClientDataReceived -= ProxyRelayMITM_OnClientDataReceived;
                ProxyRelayMITM.OnClientDataSent -= ProxyRelayMITM_OnClientDataSent;
                ProxyRelayMITM.OnRemoteDataReceived -= ProxyRelayMITM_OnRemoteDataReceived;
                ProxyRelayMITM.OnRemoteDataSent -= ProxyRelayMITM_OnRemoteDataSent;
            }
            catch (Exception) { }
        }
        
        Client?.Disconnect();
        Remote?.Disconnect();
        ProxyRelay?.Disconnect();
        ProxyRelayMITM?.Disconnect();
    }

}