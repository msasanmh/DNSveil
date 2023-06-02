using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;

namespace MsmhTools.HTTPProxyServer
{
    public class HTTPProxyServer
    {
        public static DPIBypassProgram BypassProgram = new();
        public void EnableDPIBypass(DPIBypassProgram dpiBypassProgram)
        {
            BypassProgram = dpiBypassProgram;
        }

        public class DPIBypassProgram : HTTPProxyServer
        {
            public DPIBypass.Mode DPIBypassMode { get; set; } = DPIBypass.Mode.Disable;
            public int FirstPartOfDataLength { get; set; } = 0;
            public int FragmentSize { get; set; } = 0;
            public int FragmentChunks { get; set; } = 0;
            public int FragmentDelay { get; set; } = 0;
            /// <summary>
            /// Don't chunk the request when size is 65536.
            /// </summary>
            public bool DontChunkTheBiggestRequest { get; set; } = false;
            public bool SendInRandom { get; set; } = false;
            public DPIBypassProgram()
            {

            }

            public void Set(DPIBypass.Mode mode, int firstPartOfDataLength, int fragmentSize, int fragmentChunks, int fragmentDelay)
            {
                DPIBypassMode = mode;
                FirstPartOfDataLength = firstPartOfDataLength;
                FragmentSize = fragmentSize;
                FragmentChunks = fragmentChunks;
                FragmentDelay = fragmentDelay;
            }
        }

        internal static ProxySettings _Settings = new();

        private static TunnelManager? _TunnelManager;
        private static TcpListener? _TcpListener;

        private static CancellationTokenSource? _CancelTokenSource;
        private static CancellationToken _CancelToken;
        internal static int _ActiveThreads = 0;

        public bool IsRunning = false;
        private static bool Cancel = false;

        public static readonly int MaxDataSize = 65536;

        public event EventHandler<EventArgs>? OnRequestReceived;
        public event EventHandler<EventArgs>? OnErrorOccurred;
        public event EventHandler<EventArgs>? OnDebugInfoReceived;
        public event EventHandler<EventArgs>? OnChunkDetailsReceived;
        private static readonly EventWaitHandle Terminator = new(false, EventResetMode.ManualReset);

        private List<string> BlackList = new();

        public HTTPProxyServer()
        {
            // Captive Portal
            BlackList.Add("ipv6.msftconnecttest.com:80");
            BlackList.Add("detectportal.firefox.com:80");
            BlackList.Add("gstatic.com:80");
        }

        public void Start(IPAddress ipAddress, int port, int maxThreads)
        {
            if (IsRunning) return;
            IsRunning = true;

            Task.Run(() =>
            {
                _Settings = new();
                _Settings.ListenerIpAddress = ipAddress;
                _Settings.ListenerPort = port;
                _Settings.MaxThreads = maxThreads;

                Welcome();

                _TunnelManager = new();

                _CancelTokenSource = new();
                _CancelToken = _CancelTokenSource.Token;

                Cancel = false;
                Tunnel.Cancel = false;

                Task.Run(() => AcceptConnections(), _CancelToken);

                Terminator.Reset();
                Terminator.WaitOne();
            });
        }

        public void Stop()
        {
            if (IsRunning && _TcpListener != null && _CancelTokenSource != null)
            {
                IsRunning = false;
                Terminator.Set();
                _CancelTokenSource.Cancel(true);
                Cancel = true;
                Tunnel.Cancel = true;
                _TcpListener.Stop();

                if (_TunnelManager != null)
                {
                    var t = _TunnelManager.GetFull().ToList();
                    Debug.WriteLine(t.Count);
                    for (int n = 0; n < t.Count; n++)
                    {
                        var kvp = t[n];
                        if (_TunnelManager.Active(kvp.Key))
                            _TunnelManager.Remove(kvp.Key);
                    }
                }

                IsRunning = _TcpListener.Server.IsBound;
                Goodbye();
            }
        }

        public int ListeningPort
        {
            get => _Settings.ListenerPort;
        }

        public bool IsDpiActive
        {
            get => BypassProgram.DPIBypassMode != DPIBypass.Mode.Disable;
        }

        public int ActiveRequests
        {
            get => _ActiveThreads;
        }

        public int MaxRequests
        {
            get => _Settings != null ? _Settings.MaxThreads : 0;
        }

        public Dictionary<int, Tunnel> GetCurrentConnectTunnels()
        {
            return _TunnelManager != null ? _TunnelManager.GetMetadata() : new Dictionary<int, Tunnel>();
        }

        private void Welcome()
        {
            // Event
            string msgEvent = $"HTTP Proxy Server starting on {_Settings.ListenerIpAddress}:{_Settings.ListenerPort}";
            OnRequestReceived?.Invoke(msgEvent, EventArgs.Empty);
            OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);
        }

        private void Goodbye()
        {
            // Event
            string msgEvent = "HTTP Proxy Server stopped.";
            OnRequestReceived?.Invoke(msgEvent, EventArgs.Empty);
            OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);
        }

        private void AcceptConnections()
        {
            if (Cancel) return;

            try
            {
                _TcpListener = new(_Settings.ListenerIpAddress, _Settings.ListenerPort);
                _TcpListener.Start();
                Task.Delay(200).Wait();

                if (_TcpListener != null)
                {
                    IsRunning = _TcpListener.Server.IsBound;
                    
                    while (!Cancel)
                    {
                        TcpClient tcpClient = _TcpListener.AcceptTcpClient();
                        Task.Run(() => ProcessConnection(tcpClient), _CancelToken);
                        if (_CancelToken.IsCancellationRequested || Cancel)
                            break;
                    }
                }
                else
                {
                    IsRunning = false;
                }
            }
            catch (Exception eOuter)
            {
                // Event Error
                if (!_CancelToken.IsCancellationRequested || !Cancel)
                {
                    string msgEventErr = $"Accept Connections: {eOuter.Message}";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                }
            }
        }

        private async Task ProcessConnection(TcpClient client)
        {
            if (Cancel) return;

            int connectionId = Environment.CurrentManagedThreadId;
            Debug.WriteLine($"Active Requests: {_ActiveThreads}");
            
            try
            {
                // Check-if-Max-Exceeded
                if (_ActiveThreads >= _Settings.MaxThreads)
                {
                    // Event
                    string msgEventErr = $"AcceptConnections connection count {_ActiveThreads} exceeds configured max {_Settings.MaxThreads}.";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);

                    // Kill em
                    client.Dispose();
                    Debug.WriteLine($"Active Requests2: {_ActiveThreads}");
                    Task.Delay(100).Wait();
                    return;
                }

                _ActiveThreads++;

                IPEndPoint? clientIpEndpoint = client.Client.RemoteEndPoint as IPEndPoint;
                IPEndPoint? serverIpEndpoint = client.Client.LocalEndPoint as IPEndPoint;

                string clientEndpoint = clientIpEndpoint != null ? clientIpEndpoint.ToString() : string.Empty;
                string serverEndpoint = serverIpEndpoint != null ? serverIpEndpoint.ToString() : string.Empty;

                string clientIp = clientIpEndpoint != null ? clientIpEndpoint.Address.ToString() : string.Empty;
                int clientPort = clientIpEndpoint != null ? clientIpEndpoint.Port : 0;

                string serverIp = serverIpEndpoint != null ? serverIpEndpoint.Address.ToString() : string.Empty;
                int serverPort = serverIpEndpoint != null ? serverIpEndpoint.Port : 0;

                Request? req = Request.FromTcpClient(client);
                if (req == null)
                {
                    // Event Error
                    string msgEventErr = $"{clientEndpoint} unable to build HTTP request.";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                    _ActiveThreads--;
                    return;
                }

                req.SourceIp = clientIp;
                req.SourcePort = clientPort;
                req.DestIp = serverIp;
                req.DestPort = serverPort;

                // Block Black List
                for (int n = 0; n < BlackList.Count; n++)
                {
                    string website = BlackList[n];
                    string destWebsite = $"{req.DestHostname}:{req.DestHostPort}";
                    if (destWebsite == website)
                    {
                        // Event
                        string msgEvent = $"Black list: {req.FullUrl}, request denied.";
                        OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                        client.Close();
                        _ActiveThreads--;
                        return;
                    }
                }
                
                // Event
                OnRequestReceived?.Invoke($"{req.DestHostname}:{req.DestHostPort}", EventArgs.Empty);

                if (req.Method == HttpMethod.CONNECT)
                {
                    // Event
                    string msgEvent = $"{clientEndpoint} proxying request via CONNECT to {req.FullUrl}";
                    OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                    ConnectHttpsRequest(connectionId, client, req);
                }
                else
                {
                    // Event
                    string msgEvent = $"{clientEndpoint} proxying request to {req.FullUrl}";
                    OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                    await ConnectHttpRequestAsync(req, client);
                }

                client.Close();
                _ActiveThreads--;
            }
            catch (IOException)
            {
                // Do nothing
            }
            catch (Exception eInner)
            {
                // Event Error
                string msgEventErr = $"Process Connection: {eInner.Message}";
                OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
            }
        }

        //======================================== Connect HTTPS Request

        private void ConnectHttpsRequest(int connectionId, TcpClient client, Request req)
        {
            if (Cancel) return;

            Tunnel? currTunnel = null;
            TcpClient? server = null;

            try
            {
                client.NoDelay = true;
                client.Client.NoDelay = true;

                server = new();

                try
                {
                    if (!string.IsNullOrEmpty(req.DestHostname))
                    {
                        server.Connect(req.DestHostname, req.DestHostPort);
                    }
                    else
                    {
                        // Event Error
                        string msgEventErr = $"Hostname was null or empty.";
                        OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                        return;
                    }
                }
                catch (Exception)
                {
                    // Event Error
                    string msgEventErr = $"Connect Request failed to {req.DestHostname}:{req.DestHostPort}";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                    return;
                }

                server.NoDelay = true;
                server.Client.NoDelay = true;

                byte[] connectResponse = ConnectResponse();
                static byte[] ConnectResponse()
                {
                    string resp = "HTTP/1.1 200 Connection Established\r\nConnection: close\r\n\r\n";
                    return Encoding.UTF8.GetBytes(resp);
                }
                client.Client.Send(connectResponse);

                if (string.IsNullOrEmpty(req.SourceIp) || string.IsNullOrEmpty(req.DestIp))
                {
                    // Event Error
                    string msgEventErr = $"Source or dest IP were null or empty. SourceIp: {req.SourceIp} DestIp: {req.DestIp}";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                    return;
                }

                // Create Tunnel
                currTunnel = new(
                    req.SourceIp,
                    req.SourcePort,
                    req.DestIp,
                    req.DestPort,
                    req.DestHostname,
                    req.DestHostPort,
                    client,
                    server);

                // Tunnel Event OnChunkDetailsReceived
                currTunnel.OnChunkDetailsReceived -= CurrTunnel_OnChunkDetailsReceived;
                currTunnel.OnChunkDetailsReceived += CurrTunnel_OnChunkDetailsReceived;
                void CurrTunnel_OnChunkDetailsReceived(object? sender, EventArgs e)
                {
                    if (sender is string chunkDetails)
                    {
                        string msgEvent = $"{req.DestHostname}:{req.DestHostPort} {chunkDetails}";
                        OnChunkDetailsReceived?.Invoke(msgEvent, EventArgs.Empty);
                    }
                }

                // Tunnel Event OnDebugInfoReceived
                currTunnel.OnDebugInfoReceived -= CurrTunnel_OnDebugInfoReceived;
                currTunnel.OnDebugInfoReceived += CurrTunnel_OnDebugInfoReceived;
                void CurrTunnel_OnDebugInfoReceived(object? sender, EventArgs e)
                {
                    if (sender is string debugInfo)
                        OnDebugInfoReceived?.Invoke(debugInfo, EventArgs.Empty);
                }

                // Tunnel Event OnErrorOccurred
                currTunnel.OnErrorOccurred -= CurrTunnel_OnErrorOccurred;
                currTunnel.OnErrorOccurred += CurrTunnel_OnErrorOccurred;
                void CurrTunnel_OnErrorOccurred(object? sender, EventArgs e)
                {
                    if (sender is string error)
                        OnErrorOccurred?.Invoke(error, EventArgs.Empty);
                }

                if (_TunnelManager != null)
                    _TunnelManager.Add(connectionId, currTunnel);

                while (currTunnel.IsActive())
                {
                    Task.Delay(100).Wait();
                }
            }
            catch (SocketException)
            {
                // do nothing
            }
            catch (Exception e)
            {
                // Event Error
                string msgEventErr = $"Connect Request: {e.Message}";
                OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
            }
            finally
            {
                if (_TunnelManager != null)
                    _TunnelManager.Remove(connectionId);

                if (client != null)
                    client.Dispose();

                if (server != null)
                    server.Dispose();
            }
        }

        //======================================== Connect HTTP Request

        private async Task ConnectHttpRequestAsync(Request req, TcpClient tcpClient)
        {
            if (Cancel) return;

            RestResponse? resp = proxyRequest(req).Result;
            if (resp != null)
            {
                NetworkStream ns = tcpClient.GetStream();
                await sendRestResponse(resp, ns);
                await ns.FlushAsync();
                ns.Close();
            }

            async Task<RestResponse?> proxyRequest(Request request)
            {
                if (Cancel) return null;

                try
                {
                    if (request.Headers != null)
                    {
                        string foundVal = string.Empty;

                        foreach (KeyValuePair<string, string> currKvp in request.Headers)
                        {
                            if (string.IsNullOrEmpty(currKvp.Key)) continue;
                            if (currKvp.Key.ToLower().Equals("expect"))
                            {
                                foundVal = currKvp.Key;
                                break;
                            }
                        }

                        if (!string.IsNullOrEmpty(foundVal)) request.Headers.Remove(foundVal);
                    }

                    if (string.IsNullOrEmpty(request.FullUrl))
                    {
                        // Evemt Error
                        string msgEventErr = $"Full Url was null or empty. FullUrl: {request.FullUrl}";
                        OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                        return null;
                    }

                    //(HttpMethod)(Enum.Parse(typeof(RestWrapper.HttpMethod), request.Method.ToString())),
                    RestRequest rRequest = new(
                        request.FullUrl,
                        request.Method,
                        request.Headers,
                        request.ContentType);
                    
                    if (request.ContentLength > 0)
                    {
                        return await rRequest.SendAsync(request.ContentLength, request.DataStream);
                    }
                    else
                    {
                        return await rRequest.SendAsync();
                    }
                }
                catch (Exception e)
                {
                    // Evemt Error
                    string msgEventErr = $"{e.Message}";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                    return null;
                }
            }

            async Task sendRestResponse(RestResponse resp, NetworkStream ns)
            {
                if (Cancel) return;

                try
                {
                    byte[]? ret = Array.Empty<byte>();
                    string statusLine = resp.ProtocolVersion + " " + resp.StatusCode + " " + resp.StatusDescription + "\r\n";
                    ret = Common.AppendBytes(ret, Encoding.UTF8.GetBytes(statusLine));

                    if (!string.IsNullOrEmpty(resp.ContentType))
                    {
                        string contentTypeLine = "Content-Type: " + resp.ContentType + "\r\n";
                        ret = Common.AppendBytes(ret, Encoding.UTF8.GetBytes(contentTypeLine));
                    }

                    if (resp.ContentLength > 0)
                    {
                        string contentLenLine = "Content-Length: " + resp.ContentLength + "\r\n";
                        ret = Common.AppendBytes(ret, Encoding.UTF8.GetBytes(contentLenLine));
                    }

                    if (resp.Headers != null && resp.Headers.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> currHeader in resp.Headers)
                        {
                            if (string.IsNullOrEmpty(currHeader.Key)) continue;
                            if (currHeader.Key.ToLower().Trim().Equals("content-type")) continue;
                            if (currHeader.Key.ToLower().Trim().Equals("content-length")) continue;

                            string headerLine = currHeader.Key + ": " + currHeader.Value + "\r\n";
                            ret = Common.AppendBytes(ret, Encoding.UTF8.GetBytes(headerLine));
                        }
                    }

                    ret = Common.AppendBytes(ret, Encoding.UTF8.GetBytes("\r\n"));

                    await ns.WriteAsync(ret);
                    await ns.FlushAsync();

                    if (resp.Data != null && resp.ContentLength > 0)
                    {
                        long bytesRemaining = resp.ContentLength;
                        byte[] buffer = new byte[65536];

                        while (bytesRemaining > 0)
                        {
                            int bytesRead = await resp.Data.ReadAsync(buffer);
                            if (bytesRead > 0)
                            {
                                bytesRemaining -= bytesRead;
                                await ns.WriteAsync(buffer.AsMemory(0, bytesRead));
                                await ns.FlushAsync();
                            }
                        }
                    }

                    return;
                }
                catch (Exception e)
                {
                    // Event Error
                    string msgEventErr = $"Send Rest Response: {e.Message}";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                    return;
                }
            }
        }

    }
}
