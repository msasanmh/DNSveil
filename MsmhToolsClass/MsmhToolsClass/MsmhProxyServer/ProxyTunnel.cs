using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MsmhToolsClass.ProxyServerPrograms;

namespace MsmhToolsClass.MsmhProxyServer;

internal class ProxyTunnel
{
    public readonly int ConnectionId;
    public ProxyClient Client;
    public ProxyClient RemoteClient;
    public ProxyRequest Req;

    private readonly ProxySettings ProxySettings_;
    private TcpClient? ProxifiedTcpClient_;

    // Handle SSL
    public SettingsSSL SettingsSSL_ { get; private set; } = new(false);
    public ProxyClientSSL? ClientSSL;

    public event EventHandler<EventArgs>? OnTunnelDisconnected;
    public event EventHandler<EventArgs>? OnDataReceived;

    public readonly Stopwatch KillOnTimeout = new();
    public bool Disconnect { get; set; } = false;

    public ProxyTunnel(int connectionId, ProxyClient sc, ProxyRequest req, ProxySettings proxySettings, SettingsSSL settingsSSL)
    {
        ConnectionId = connectionId;
        Client = sc;
        Req = req;
        ProxySettings_ = proxySettings;
        SettingsSSL_ = settingsSSL;

        try
        {
            // Default Remote / Socks4 Remote / Socks4A
            Socket remoteSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // HTTP & HTTPS Remote
            if (Req.ProxyName == Proxy.Name.HTTP || Req.ProxyName == Proxy.Name.HTTPS)
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
                if (ProxySettings_.RequestTimeoutSec != 0 &&
                    KillOnTimeout.ElapsedMilliseconds > TimeSpan.FromSeconds(ProxySettings_.RequestTimeoutSec).TotalMilliseconds)
                {
                    string msg = $"Killed Request On Timeout({Req.TimeoutSec} Sec): {Req.Address}:{Req.Port}";
                    Debug.WriteLine(msg);

                    OnTunnelDisconnected?.Invoke(this, EventArgs.Empty);
                    ProxifiedTcpClient_?.Close();
                    break;
                }

                // Manual Disconnect
                if (Disconnect)
                {
                    OnTunnelDisconnected?.Invoke(this, EventArgs.Empty);
                    ProxifiedTcpClient_?.Close();
                    break;
                }
            }
        });
    }

    public async void Open(ProxyProgram.Rules proxyRules, ProxyProgram.UpStreamProxy upStreamProxyProgram)
    {
        try
        {
            if (string.IsNullOrEmpty(Req.Address) || Req.Port <= -1)
            {
                OnTunnelDisconnected?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (Req.ProxyName == Proxy.Name.HTTP)
                await HttpHandler();

            if (Req.ProxyName == Proxy.Name.Socks5 && Req.Status != Socks.Status.Granted)
            {
                // Send Connection Request Frame
                await Client.SendAsync(Req.GetConnectionRequestFrameData());
                OnTunnelDisconnected?.Invoke(this, EventArgs.Empty);
                return;
            }
            
            // Connect
            if (Req.ProxyName == Proxy.Name.HTTPS ||
                (Req.ProxyName == Proxy.Name.Socks4 && Req.Command == Socks.Commands.Connect) ||
                (Req.ProxyName == Proxy.Name.Socks4A && Req.Command == Socks.Commands.Connect) ||
                (Req.ProxyName == Proxy.Name.Socks5 && Req.Command == Socks.Commands.Connect))
            {
                // Only Connect Can Support Upstream
                bool applyUpStreamProxy = false;
                if (Req.ApplyUpStreamProxy)
                {
                    if (!string.IsNullOrEmpty(Req.RulesResult.ProxyScheme))
                        ProxifiedTcpClient_ = await proxyRules.ConnectToUpStream(Req);
                    else
                        ProxifiedTcpClient_ = await upStreamProxyProgram.Connect(Req.Address, Req.Port);

                    if (ProxifiedTcpClient_ != null)
                    {
                        applyUpStreamProxy = true;
                        RemoteClient.Socket_ = ProxifiedTcpClient_.Client;
                        ConnectHandler();
                    }
                }

                if (!applyUpStreamProxy)
                {
                    await RemoteClient.Socket_.ConnectAsync(Req.Address, Req.Port);
                    ConnectHandler();
                }
            }

            // Bind
            if ((Req.ProxyName == Proxy.Name.Socks4 && Req.Command == Socks.Commands.Bind) ||
                (Req.ProxyName == Proxy.Name.Socks4A && Req.Command == Socks.Commands.Bind) ||
                (Req.ProxyName == Proxy.Name.Socks5 && Req.Command == Socks.Commands.Bind))
            {
                if (RemoteClient.Socket_.AddressFamily == AddressFamily.InterNetworkV6)
                    RemoteClient.Socket_.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));
                else
                    RemoteClient.Socket_.Bind(new IPEndPoint(IPAddress.Any, 0));
                ConnectHandler();
            }

            // UDP (Only Socks5 Supports UDP)
            if (Req.ProxyName == Proxy.Name.Socks5 && Req.Command == Socks.Commands.UDP)
            {
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
            if (Req.ProxyName == Proxy.Name.HTTPS)
            {
                string resp = "HTTP/1.1 200 Connection Established\r\nConnection: close\r\n\r\n";
                byte[] httpsResponse = Encoding.UTF8.GetBytes(resp);

                await Client.SendAsync(httpsResponse);
            }

            // Socks5 Response
            if (Req.ProxyName == Proxy.Name.Socks5)
            {
                // Send Connection Request Frame to Server
                byte[] request = Req.GetConnectionRequestFrameData();
                await Client.SendAsync(request);
            }

            // Receive Data from Both EndPoints
            if (SettingsSSL_.EnableSSL && !Req.AddressIsIp) // Cert Can't Be Valid When There's An IP Without A Domain. Like SOCKS4
            {
                ClientSSL = new(this);
                OnDataReceived?.Invoke(this, EventArgs.Empty);
                await ClientSSL.Execute();
            }
            else
            {
                OnDataReceived?.Invoke(this, EventArgs.Empty);
                Task ct = Client.StartReceiveAsync();
                Task rt = RemoteClient.StartReceiveAsync();
                await Task.WhenAll(ct, rt); // Both Must Receive at the Same Time
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
        RestResponse? rr = Req.RestResponse;
        if (rr != null)
        {
            byte[]? buffer = Array.Empty<byte>();
            string statusLine = rr.ProtocolVersion + " " + rr.StatusCode + " " + rr.StatusDescription + "\r\n";
            buffer = CommonTools.AppendBytes(buffer, Encoding.UTF8.GetBytes(statusLine));

            if (!string.IsNullOrEmpty(rr.ContentType))
            {
                string contentTypeLine = "Content-Type: " + rr.ContentType + "\r\n";
                buffer = CommonTools.AppendBytes(buffer, Encoding.UTF8.GetBytes(contentTypeLine));
            }

            if (rr.ContentLength > 0)
            {
                string contentLenLine = "Content-Length: " + rr.ContentLength + "\r\n";
                buffer = CommonTools.AppendBytes(buffer, Encoding.UTF8.GetBytes(contentLenLine));
            }

            if (rr.Headers != null && rr.Headers.Count > 0)
            {
                for (int i = 0; i < rr.Headers.Count; i++)
                {
                    string? key = rr.Headers.GetKey(i);
                    string? val = rr.Headers.Get(i);

                    if (string.IsNullOrEmpty(key)) continue;
                    if (string.IsNullOrEmpty(val)) continue;

                    if (key.ToLower().Trim().Equals("content-type")) continue;
                    if (key.ToLower().Trim().Equals("content-length")) continue;

                    string headerLine = key + ": " + val + "\r\n";
                    buffer = CommonTools.AppendBytes(buffer, Encoding.UTF8.GetBytes(headerLine));
                }
            }

            buffer = CommonTools.AppendBytes(buffer, Encoding.UTF8.GetBytes("\r\n"));

            // Merge Headers and Body
            buffer = buffer.Concat(rr.DataAsBytes ?? Array.Empty<byte>()).ToArray();

            // Send
            await Client.SendAsync(buffer);

            // Receive
            await Client.StartReceiveAsync();
        }
    }

}