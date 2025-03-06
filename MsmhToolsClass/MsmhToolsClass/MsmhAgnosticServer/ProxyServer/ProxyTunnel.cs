using MsmhToolsClass.ProxifiedClients;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MsmhToolsClass.MsmhAgnosticServer;

internal class ProxyTunnel
{
    public readonly int ConnectionId;
    public ProxyClient Client;
    public ProxyClient RemoteClient;
    public ProxyRequest Req;

    private TcpClient? ProxifiedTcpClient_;

    // Handle SSL
    public readonly AgnosticSettingsSSL SettingsSSL_;
    public ProxyClientSSL? ClientSSL;

    public event EventHandler<EventArgs>? OnTunnelDisconnected;
    public event EventHandler<EventArgs>? OnDataReceived;

    public readonly Stopwatch KillOnTimeout = new();
    public bool ManualDisconnect { get; set; } = false;

    public ProxyTunnel(int connectionId, ProxyClient sc, ProxyRequest req, AgnosticSettingsSSL settingsSSL)
    {
        ConnectionId = connectionId;
        Client = sc;
        Req = req;
        SettingsSSL_ = settingsSSL;

        try
        {
            // Default Remote / Socks4 Remote / Socks4A
            Socket remoteSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // HTTP & HTTPS Remote
            if (Req.ProxyName == Proxy.Name.Test || Req.ProxyName == Proxy.Name.HTTP || Req.ProxyName == Proxy.Name.HTTPS || Req.ProxyName == Proxy.Name.SniProxy)
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
                    // UDP Ipv4
                    if (Req.AddressType == Socks.AddressType.Domain || Req.AddressType == Socks.AddressType.Ipv4)
                        remoteSocket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    // UDP Ipv6
                    if (Req.AddressType == Socks.AddressType.Ipv6)
                        remoteSocket = new(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                }
            }

            RemoteClient = new ProxyClient(remoteSocket);

            KillOnTimeoutCheck();
        }
        catch (Exception ex)
        {
            RemoteClient = new ProxyClient(Client.Socket_);
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

                // Manual ManualDisconnect
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

            if (!KillOnTimeout.IsRunning) KillOnTimeout.Start();

            if (Req.ProxyName == Proxy.Name.HTTP)
                await HttpHandler().ConfigureAwait(false);
            
            if (Req.ProxyName == Proxy.Name.Socks5 && Req.Status != Socks.Status.Granted)
            {
                // Send Connection Request Frame
                await Client.SendAsync(Req.GetConnectionRequestFrameData()).ConfigureAwait(false);
                OnTunnelDisconnected?.Invoke(this, EventArgs.Empty);
                return;
            }
            
            // Connect
            if (Req.ProxyName == Proxy.Name.Test ||
                Req.ProxyName == Proxy.Name.HTTPS ||
                Req.ProxyName == Proxy.Name.SniProxy ||
                (Req.ProxyName == Proxy.Name.Socks4 && Req.Command == Socks.Commands.Connect) ||
                (Req.ProxyName == Proxy.Name.Socks4A && Req.Command == Socks.Commands.Connect) ||
                (Req.ProxyName == Proxy.Name.Socks5 && Req.Command == Socks.Commands.Connect))
            {
                // Only Connect Can Support Upstream
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
                        RemoteClient.Socket_ = ProxifiedTcpClient_.Client;
                        ConnectHandler();
                    }
                }

                if (!upStreamProxyApplied)
                {
                    await RemoteClient.Socket_.ConnectAsync(Req.Address, Req.Port).ConfigureAwait(false);
                    ConnectHandler();
                }
            }

            // Bind
            if ((Req.ProxyName == Proxy.Name.Socks4 && Req.Command == Socks.Commands.Bind) ||
                (Req.ProxyName == Proxy.Name.Socks4A && Req.Command == Socks.Commands.Bind) ||
                (Req.ProxyName == Proxy.Name.Socks5 && Req.Command == Socks.Commands.Bind))
            {
                SocketOptionName socketOptionName = SocketOptionName.ReuseAddress | SocketOptionName.ReuseUnicastPort;
                RemoteClient.Socket_.SetSocketOption(SocketOptionLevel.Socket, socketOptionName, true);

                if (RemoteClient.Socket_.AddressFamily == AddressFamily.InterNetworkV6)
                    RemoteClient.Socket_.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));
                else
                    RemoteClient.Socket_.Bind(new IPEndPoint(IPAddress.Any, 0));

                ConnectHandler();
            }

            // UDP (Only Socks5 Supports UDP)
            if (Req.ProxyName == Proxy.Name.Socks5 && Req.Command == Socks.Commands.UDP)
            {
                SocketOptionName socketOptionName = SocketOptionName.ReuseAddress | SocketOptionName.ReuseUnicastPort;
                RemoteClient.Socket_.SetSocketOption(SocketOptionLevel.Socket, socketOptionName, true);

                if (RemoteClient.Socket_.AddressFamily == AddressFamily.InterNetworkV6)
                    RemoteClient.Socket_.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));
                else
                    RemoteClient.Socket_.Bind(new IPEndPoint(IPAddress.Any, 0));

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
            // Https Response
            if (Req.ProxyName == Proxy.Name.Test || Req.ProxyName == Proxy.Name.HTTPS)
            {
                string resp = "HTTP/1.1 200 Connection Established\r\nConnection: close\r\n\r\n";
                byte[] httpsResponse = Encoding.UTF8.GetBytes(resp);

                await Client.SendAsync(httpsResponse).ConfigureAwait(false);
            }

            // Socks5 Response
            if (Req.ProxyName == Proxy.Name.Socks5)
            {
                // Send Connection Request Frame to Server
                byte[] request = Req.GetConnectionRequestFrameData();
                await Client.SendAsync(request).ConfigureAwait(false);
            }

            // Receive Data From Both EndPoints
            bool ssl = Req.ApplyChangeSNI && !string.IsNullOrWhiteSpace(Req.AddressSNI) && !Req.AddressSNI.Equals(Req.AddressOrig) && !Req.AddressIsIp; // Cert Can't Be Valid When There's An IP Without A Domain. Like SOCKS4
            if (ssl)
            {
                ClientSSL = new(this);
                OnDataReceived?.Invoke(this, EventArgs.Empty);
                await ClientSSL.Execute().ConfigureAwait(false);
            }
            else
            {
                OnDataReceived?.Invoke(this, EventArgs.Empty);

                Task ct = Client.StartReceiveAsync();
                Task rt = RemoteClient.StartReceiveAsync();
                await Task.WhenAll(ct, rt).ConfigureAwait(false); // Both Must Receive At The Same Time
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
                
                // Merge Headers and Body
                bufferList.AddRange(hrr.Data);

                // Send
                bool isSent = await Client.SendAsync(bufferList.ToArray()).ConfigureAwait(false);

                if (isSent)
                {
                    // Receive
                    Task ct = Client.StartReceiveAsync();
                    Task rt = RemoteClient.StartReceiveAsync();
                    await Task.WhenAll(ct, rt).ConfigureAwait(false); // Both Must Receive at the Same Time
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ProxyServer HttpHandler: " + ex.Message);
            }
        }
    }

    public void Disconnect()
    {
        Client?.Disconnect();
        RemoteClient?.Disconnect();
    }

}