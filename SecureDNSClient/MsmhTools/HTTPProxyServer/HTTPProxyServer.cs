using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;
using MsmhTools.DnsTool;
using MsmhTools.ProxifiedTcpClient;

namespace MsmhTools.HTTPProxyServer
{
    public partial class HTTPProxyServer
    {
        //======================================= DPI Bypass Support: Static
        internal static Program.DPIBypass StaticDPIBypassProgram = new();
        public void EnableStaticDPIBypass(Program.DPIBypass dpiBypassProgram)
        {
            StaticDPIBypassProgram = dpiBypassProgram;
        }

        //--- Constant
        internal Program.DPIBypass DPIBypassProgram = new();
        public void EnableDPIBypass(Program.DPIBypass dpiBypassProgram)
        {
            DPIBypassProgram = dpiBypassProgram;
        }

        //======================================= UpStream Proxy Support
        internal Program.UpStreamProxy UpStreamProxyProgram = new();
        public void EnableUpStreamProxy(Program.UpStreamProxy upStreamProxyProgram)
        {
            UpStreamProxyProgram = upStreamProxyProgram;
        }

        //======================================= DNS Support
        internal Program.Dns DNSProgram = new();
        public void EnableDNS(Program.Dns dnsProgram)
        {
            DNSProgram = dnsProgram;
        }
        
        //======================================= Fake DNS Support
        internal Program.FakeDns FakeDNSProgram = new();
        public void EnableFakeDNS(Program.FakeDns fakeDnsProgram)
        {
            FakeDNSProgram = fakeDnsProgram;
        }
        
        //======================================= Black White List Support
        internal Program.BlackWhiteList BWListProgram = new();
        public void EnableBlackWhiteList(Program.BlackWhiteList blackWhiteListProgram)
        {
            BWListProgram = blackWhiteListProgram;
        }

        //======================================= Start HTTP Proxy
        internal ProxySettings _Settings = new();

        private TunnelManager _TunnelManager = new();
        private TcpListener? _TcpListener;

        private CancellationTokenSource? _CancelTokenSource;
        private CancellationToken _CancelToken;
        private System.Timers.Timer KillOnOverloadTimer = new(5000);

        public bool IsRunning { get; private set; } = false;
        private bool Cancel = false;

        public readonly int MaxDataSize = 65536;

        public event EventHandler<EventArgs>? OnRequestReceived;
        public event EventHandler<EventArgs>? OnErrorOccurred;
        public event EventHandler<EventArgs>? OnDebugInfoReceived;
        private readonly EventWaitHandle Terminator = new(false, EventResetMode.ManualReset);

        private List<string> BlackList = new();

        public bool BlockPort80 { get; set; } = false;

        public HTTPProxyServer()
        {
            // Captive Portal and others
            BlackList.Add("ipv6.msftconnecttest.com:80");
            BlackList.Add("msedge.b.tlu.dl.delivery.mp.microsoft.com:80");
            BlackList.Add("edgedl.me.gvt1.com:80");
            BlackList.Add("detectportal.firefox.com:80");
            BlackList.Add("gstatic.com:80");
            BlackList = BlackList.Distinct().ToList();
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

                KillOnOverloadTimer.Elapsed += KillOnOverloadTimer_Elapsed;
                KillOnOverloadTimer.Start();

                Task.Run(() => AcceptConnections(), _CancelToken);

                Terminator.Reset();
                Terminator.WaitOne();
            });
        }

        private void KillOnOverloadTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (ActiveRequests >= _Settings.MaxThreads)
            {
                KillAll();
            }
        }

        /// <summary>
        /// Kill all active requests
        /// </summary>
        public void KillAll()
        {
            if (_TunnelManager != null)
            {
                var dic = _TunnelManager.GetTunnels();
                Debug.WriteLine(dic.Count);
                foreach (var item in dic)
                {
                    Debug.WriteLine(item.Key);
                    _TunnelManager.Remove(item.Key);
                }
            }
        }

        public void Stop()
        {
            if (IsRunning && _TcpListener != null && _CancelTokenSource != null)
            {
                IsRunning = false;
                Terminator.Set();
                _CancelTokenSource.Cancel(true);
                Cancel = true;
                _TcpListener.Stop();

                KillAll();

                KillOnOverloadTimer.Stop();

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
            get => DPIBypassProgram.DPIBypassMode != Program.DPIBypass.Mode.Disable || StaticDPIBypassProgram.DPIBypassMode != Program.DPIBypass.Mode.Disable;
        }

        public int ActiveRequests
        {
            get => _TunnelManager.Count;
        }

        public int MaxRequests
        {
            get => _Settings != null ? _Settings.MaxThreads : 0;
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

        private async void AcceptConnections()
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
                        if (tcpClient.Connected)
                            ProcessConnection(tcpClient);
                        await Task.Delay(1000);
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

        private async void ProcessConnection(TcpClient client)
        {
            if (Cancel) return;
            
            // Generate unique int
            int connectionId = Guid.NewGuid().GetHashCode() + BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);
            Debug.WriteLine($"Active Requests: {ActiveRequests}");
            
            try
            {
                // Check if Max Exceeded
                if (ActiveRequests >= _Settings.MaxThreads)
                {
                    // Event
                    string msgEventErr = $"AcceptConnections connection count {ActiveRequests} exceeds configured max {_Settings.MaxThreads}.";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);

                    // Kill em
                    try
                    {
                        client.Dispose();
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }
                    Debug.WriteLine($"Active Requests2: {ActiveRequests}");
                    return;
                }

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
                    return;
                }

                if (string.IsNullOrEmpty(req.DestHostname) || req.DestHostPort == 0)
                {
                    // Event Error
                    string msgEventErr = "Hostname is empty or Port is 0.";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                    return;
                }

                if (req.Data != null && req.Data.Length < 1)
                {
                    // Event Error
                    string msgEventErr = "Data length is 0.";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                    return;
                }

                req.SourceIp = clientIp;
                req.SourcePort = clientPort;
                req.DestIp = serverIp;
                req.DestPort = serverPort;

                // Block Port 80
                if (BlockPort80 && req.DestHostPort == 80)
                {
                    // Event
                    string msgEvent = $"Block Port 80: {req.FullUrl}, request denied.";
                    OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                    client.Close();
                    return;
                }

                //// Block Built-in Black List
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
                        return;
                    }
                }

                //// Black White List Program
                if (BWListProgram.ListMode != Program.BlackWhiteList.Mode.Disable)
                {
                    bool isMatch = BWListProgram.IsMatch(req.DestHostname);
                    if (isMatch)
                    {
                        // Event
                        string msgEvent = $"Black White list: {req.FullUrl}, request denied.";
                        OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                        client.Close();
                        return;
                    }
                }

                // Save Orig Hostname
                string origHostname = req.DestHostname;

                //// FakeDNS Program
                if (FakeDNSProgram.FakeDnsMode != Program.FakeDns.Mode.Disable)
                    req.DestHostname = FakeDNSProgram.Get(req.DestHostname);

                //// DNS Program
                if (origHostname.Equals(req.DestHostname))
                {
                    if (DNSProgram.DNSMode != Program.Dns.Mode.Disable)
                    {
                        string ipStr = await DNSProgram.Get(req.DestHostname);
                        if (!ipStr.StartsWith("10.") && !ipStr.StartsWith("127.0.") && !ipStr.StartsWith("172.16.") && !ipStr.StartsWith("192.168."))
                            req.DestHostname = ipStr;
                    }
                }

                // Check if Dest Host or IP is blocked
                bool isIp = IPAddress.TryParse(req.DestHostname, out IPAddress _);
                if (isIp)
                    req.IsDestBlocked = await CommonTools.IsIpBlocked(req.DestHostname, req.DestHostPort, 2000);
                else
                    req.IsDestBlocked = await CommonTools.IsHostBlocked(req.DestHostname, req.DestHostPort, 2000);

                // Event
                string msgReqEvent = $"{origHostname}:{req.DestHostPort}";
                if (!origHostname.Equals(req.DestHostname))
                    msgReqEvent = $"{origHostname}:{req.DestHostPort} => {req.DestHostname}";
                if (req.IsDestBlocked && isIp)
                    msgReqEvent += " (IP is blocked)";
                if (req.IsDestBlocked && !isIp)
                    msgReqEvent += " (Host is blocked)";

                // Apply upstream?
                bool applyUpStreamProxy = false;
                bool isUpStreamProgramActive = UpStreamProxyProgram.UpStreamMode != Program.UpStreamProxy.Mode.Disable;

                if ((isUpStreamProgramActive && !UpStreamProxyProgram.OnlyApplyToBlockedIps) ||
                    (isUpStreamProgramActive && UpStreamProxyProgram.OnlyApplyToBlockedIps && req.IsDestBlocked))
                    applyUpStreamProxy = true;

                if (applyUpStreamProxy)
                    msgReqEvent += " (Bypassing through Upstream Proxy)";

                Debug.WriteLine(msgReqEvent);
                OnRequestReceived?.Invoke(msgReqEvent, EventArgs.Empty);

                // Begin Connect
                if (req.Method == HttpMethodReq.CONNECT)
                {
                    // Event
                    string msgEvent = $"{clientEndpoint} proxying request via CONNECT to {req.FullUrl}";
                    OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                    await Task.Run(() => ConnectHttpsRequest(connectionId, client, req, applyUpStreamProxy));
                }
                else
                {
                    // Event
                    string msgEvent = $"{clientEndpoint} proxying request to {req.FullUrl}";
                    OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                    await ConnectHttpRequestAsync(req, client);
                }

                try
                {
                    client.Dispose();
                }
                catch (NullReferenceException)
                {
                    // do nothing
                }
            }
            catch (IOException)
            {
                // Do nothing
            }
            catch (Exception ex)
            {
                // Event Error
                string msgEventErr = $"Process Connection: {ex.Message}";
                OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
            }
        }

        //======================================== Connect HTTPS Request

        private void ConnectHttpsRequest(int connectionId, TcpClient client, Request req, bool applyUpStreamProxy = false)
        {
            if (Cancel) return;
            if (client.Client == null) return;
            
            TcpClient server = new();

            try
            {
                client.NoDelay = true;
                client.Client.NoDelay = true;

                try
                {
                    if (!string.IsNullOrEmpty(req.DestHostname))
                    {
                        if (applyUpStreamProxy)
                        {
                            TcpClient? proxifiedTcpClient = UpStreamProxyProgram.Connect(req.DestHostname, req.DestHostPort);
                            if (proxifiedTcpClient != null)
                            {
                                server = proxifiedTcpClient;
                            }
                            else
                            {
                                // Event Error
                                string msgEventErr = $"Couldn't connect to upstream proxy.";
                                OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);

                                server.Connect(req.DestHostname, req.DestHostPort);
                            }
                        }
                        else
                        {
                            server.Connect(req.DestHostname, req.DestHostPort);
                        }
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
                if (client.Client == null) return;
                client.Client.Send(connectResponse);

                if (string.IsNullOrEmpty(req.SourceIp) || string.IsNullOrEmpty(req.DestIp))
                {
                    // Event Error
                    string msgEventErr = $"Source or dest IP were null or empty. SourceIp: {req.SourceIp} DestIp: {req.DestIp}";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                    return;
                }

                // Create Tunnel
                Tunnel currentTunnel = new(req, client, server);
                
                currentTunnel.EnableDPIBypass(DPIBypassProgram);

                // Tunnel Event OnDebugInfoReceived
                currentTunnel.OnDebugInfoReceived -= CurrentTunnel_OnDebugInfoReceived;
                currentTunnel.OnDebugInfoReceived += CurrentTunnel_OnDebugInfoReceived;
                void CurrentTunnel_OnDebugInfoReceived(object? sender, EventArgs e)
                {
                    if (sender is string debugInfo)
                        OnDebugInfoReceived?.Invoke(debugInfo, EventArgs.Empty);
                }

                // Tunnel Event OnErrorOccurred
                currentTunnel.OnErrorOccurred -= CurrentTunnel_OnErrorOccurred;
                currentTunnel.OnErrorOccurred += CurrentTunnel_OnErrorOccurred;
                void CurrentTunnel_OnErrorOccurred(object? sender, EventArgs e)
                {
                    if (sender is string error)
                        OnErrorOccurred?.Invoke(error, EventArgs.Empty);
                }

                if (_TunnelManager != null)
                    _TunnelManager.Add(connectionId, currentTunnel);

                while (currentTunnel.IsActive())
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
                    ret = CommonTools.AppendBytes(ret, Encoding.UTF8.GetBytes(statusLine));

                    if (!string.IsNullOrEmpty(resp.ContentType))
                    {
                        string contentTypeLine = "Content-Type: " + resp.ContentType + "\r\n";
                        ret = CommonTools.AppendBytes(ret, Encoding.UTF8.GetBytes(contentTypeLine));
                    }

                    if (resp.ContentLength > 0)
                    {
                        string contentLenLine = "Content-Length: " + resp.ContentLength + "\r\n";
                        ret = CommonTools.AppendBytes(ret, Encoding.UTF8.GetBytes(contentLenLine));
                    }

                    if (resp.Headers != null && resp.Headers.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> currHeader in resp.Headers)
                        {
                            if (string.IsNullOrEmpty(currHeader.Key)) continue;
                            if (currHeader.Key.ToLower().Trim().Equals("content-type")) continue;
                            if (currHeader.Key.ToLower().Trim().Equals("content-length")) continue;

                            string headerLine = currHeader.Key + ": " + currHeader.Value + "\r\n";
                            ret = CommonTools.AppendBytes(ret, Encoding.UTF8.GetBytes(headerLine));
                        }
                    }

                    ret = CommonTools.AppendBytes(ret, Encoding.UTF8.GetBytes("\r\n"));

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
