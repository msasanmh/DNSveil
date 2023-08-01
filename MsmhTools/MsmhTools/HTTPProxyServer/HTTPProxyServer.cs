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
        public static Program.DPIBypass StaticDPIBypassProgram = new();
        public void EnableStaticDPIBypass(Program.DPIBypass dpiBypassProgram)
        {
            StaticDPIBypassProgram = dpiBypassProgram;
        }

        //--- Constant
        public Program.DPIBypass DPIBypassProgram = new();
        public void EnableDPIBypass(Program.DPIBypass dpiBypassProgram)
        {
            DPIBypassProgram = dpiBypassProgram;
        }

        //======================================= UpStream Proxy Support
        public Program.UpStreamProxy UpStreamProxyProgram = new();
        public void EnableUpStreamProxy(Program.UpStreamProxy upStreamProxyProgram)
        {
            UpStreamProxyProgram = upStreamProxyProgram;
        }

        //======================================= DNS Support
        public Program.Dns DNSProgram = new();
        public void EnableDNS(Program.Dns dnsProgram)
        {
            DNSProgram = dnsProgram;
        }

        //======================================= Fake DNS Support
        public Program.FakeDns FakeDNSProgram = new();
        public void EnableFakeDNS(Program.FakeDns fakeDnsProgram)
        {
            FakeDNSProgram = fakeDnsProgram;
        }

        //======================================= Black White List Support
        public Program.BlackWhiteList BWListProgram = new();
        public void EnableBlackWhiteList(Program.BlackWhiteList blackWhiteListProgram)
        {
            BWListProgram = blackWhiteListProgram;
        }

        //======================================= DontBypass Support
        public Program.DontBypass DontBypassProgram = new();
        public void EnableDontBypass(Program.DontBypass dontBypassProgram)
        {
            DontBypassProgram = dontBypassProgram;
        }

        //======================================= Start HTTP Proxy
        internal ProxySettings _Settings = new();

        private TunnelManager _TunnelManager = new();
        private TcpListener? _TcpListener;

        private CancellationTokenSource? _CancelTokenSource;
        private CancellationToken _CancelToken;

        private System.Timers.Timer KillOnOverloadTimer { get; set; } = new(5000);
        private PerformanceCounter PFC { get; set; } = new();
        private float CpuUsage { get; set; } = 0;

        private bool Cancel = false;

        //private readonly EventWaitHandle Terminator = new(false, EventResetMode.ManualReset);

        private Thread? MainThread;
        private List<string> BlackList { get; set; } = new();

        public event EventHandler<EventArgs>? OnRequestReceived;
        public event EventHandler<EventArgs>? OnErrorOccurred;
        public event EventHandler<EventArgs>? OnDebugInfoReceived;
        public readonly int MaxDataSize = 65536;
        public bool IsRunning { get; private set; } = false;
        public bool BlockPort80 { get; set; } = false;

        /// <summary>
        /// Kill Requests If CPU Usage is Higher than this Value.
        /// </summary>
        public float KillOnCpuUsage { get; set; } = 25;

        /// <summary>
        /// Kill Request if didn't receive data for n seconds. Default: 0 Sec (Disabled)
        /// </summary>
        public int RequestTimeoutSec { get; set; } = 0;

        public HTTPProxyServer()
        {
            // Captive Portal and others
            BlackList.Add("ipv6.msftconnecttest.com:80");
            BlackList.Add("msedge.b.tlu.dl.delivery.mp.microsoft.com:80");
            BlackList.Add("edgedl.me.gvt1.com:80");
            BlackList.Add("detectportal.firefox.com:80");
            BlackList.Add("gstatic.com:80");
            BlackList = BlackList.Distinct().ToList();

            // CPU
            PFC.CategoryName = "Process";
            PFC.CounterName = "% Processor Time";
            PFC.InstanceName = Process.GetCurrentProcess().ProcessName;
            PFC.ReadOnly = true;
        }

        public void Start(IPAddress ipAddress, int port, int maxThreads)
        {
            if (IsRunning) return;
            IsRunning = true;

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

            //Task.Run(() => AcceptConnections(), _CancelToken);

            ThreadStart threadStart = new(AcceptConnections);
            MainThread = new(threadStart);
            MainThread.SetApartmentState(ApartmentState.STA);
            MainThread.Start();

            //Task.Run(() =>
            //{
            //    Task.Run(() => AcceptConnections(), _CancelToken);

            //    Terminator.Reset();
            //    Terminator.WaitOne();
            //});

        }

        private void KillOnOverloadTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            CpuUsage = PFC.NextValue() / Environment.ProcessorCount;
            if (AllRequests >= _Settings.MaxThreads || CpuUsage > KillOnCpuUsage)
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
                //Terminator.Set();
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

        public int ActiveTunnels
        {
            get => _TunnelManager.Count;
        }

        public int AllRequests { get; private set; } = 0;

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
                        TcpClient tcpClient = await _TcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
                        if (tcpClient.Connected) ProcessConnectionSync(tcpClient);
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

        private void ProcessConnectionSync(TcpClient tcpClient)
        {
            Task.Run(() => ProcessConnection(tcpClient));
        }

        private async void ProcessConnection(TcpClient client)
        {
            if (Cancel) return;

            AllRequests++;

            // Generate unique int
            int connectionId = Guid.NewGuid().GetHashCode() + BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);
            Debug.WriteLine($"Active Requests: {AllRequests} of {_Settings.MaxThreads}");
            
            try
            {
                // Check if Max Exceeded
                if (AllRequests >= _Settings.MaxThreads || CpuUsage > KillOnCpuUsage)
                {
                    // Event
                    string msgEventErr = $"AcceptConnections Max Exceeded, {AllRequests} of {_Settings.MaxThreads} Requests. CPU: {CpuUsage} of {KillOnCpuUsage}.";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                    Debug.WriteLine(msgEventErr);

                    // Kill em
                    try
                    {
                        client.Close();
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }
                    
                    if (AllRequests > 0) AllRequests--;
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
                    if (client.Connected) client.Close();
                    if (AllRequests > 0) AllRequests--;
                    return;
                }

                if (string.IsNullOrEmpty(req.DestHostname) || req.DestHostPort == 0)
                {
                    // Event Error
                    string msgEventErr = "Hostname is empty or Port is 0.";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                    if (client.Connected) client.Close();
                    if (AllRequests > 0) AllRequests--;
                    return;
                }

                if (req.Data != null && req.Data.Length < 1)
                {
                    // Event Error
                    string msgEventErr = "Data length is 0.";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                    if (client.Connected) client.Close();
                    if (AllRequests > 0) AllRequests--;
                    return;
                }

                req.SourceIp = clientIp;
                req.SourcePort = clientPort;
                req.DestIp = serverIp;
                req.DestPort = serverPort;
                req.TimeoutSec = RequestTimeoutSec;

                // Block Port 80
                if (BlockPort80 && req.DestHostPort == 80)
                {
                    // Event
                    string msgEvent = $"Block Port 80: {req.FullUrl}, request denied.";
                    OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                    if (client.Connected) client.Close();
                    if (AllRequests > 0) AllRequests--;
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

                        if (client.Connected) client.Close();
                        if (AllRequests > 0) AllRequests--;
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

                        if (client.Connected) client.Close();
                        if (AllRequests > 0) AllRequests--;
                        return;
                    }
                }

                //// DontBypass Program
                req.ApplyDpiBypass = true;
                if (DontBypassProgram.DontBypassMode != Program.DontBypass.Mode.Disable)
                {
                    bool isMatch = DontBypassProgram.IsMatch(req.DestHostname);
                    if (isMatch) req.ApplyDpiBypass = false;
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

                // Event
                string msgReqEvent = $"{origHostname}:{req.DestHostPort}";
                if (!origHostname.Equals(req.DestHostname))
                    msgReqEvent = $"{origHostname}:{req.DestHostPort} => {req.DestHostname}";

                // Is Upstream Active
                bool isUpStreamProgramActive = UpStreamProxyProgram.UpStreamMode != Program.UpStreamProxy.Mode.Disable;

                // Check if Dest Host or IP is blocked
                bool isIp = IPAddress.TryParse(req.DestHostname, out IPAddress _);
                if (isIp)
                    req.IsDestBlocked = await CommonTools.IsIpBlocked(req.DestHostname, req.DestHostPort, req.Method, 2000);
                else
                    req.IsDestBlocked = await CommonTools.IsHostBlocked(req.DestHostname, req.DestHostPort, req.Method, 2000);

                if (req.IsDestBlocked && isIp)
                    msgReqEvent += " (IP is blocked)";
                if (req.IsDestBlocked && !isIp)
                    msgReqEvent += " (Host is blocked)";

                // Apply upstream?
                bool applyUpStreamProxy = false;
                if ((isUpStreamProgramActive && !UpStreamProxyProgram.OnlyApplyToBlockedIps) ||
                    (isUpStreamProgramActive && UpStreamProxyProgram.OnlyApplyToBlockedIps && req.IsDestBlocked))
                    applyUpStreamProxy = true;

                if (applyUpStreamProxy)
                    msgReqEvent += " (Bypassing through Upstream Proxy)";

                //Debug.WriteLine(msgReqEvent);
                OnRequestReceived?.Invoke(msgReqEvent, EventArgs.Empty);

                // Not a good idea
                //if (req.IsDestBlocked && !applyUpStreamProxy)
                //{
                //    if (client.Connected) client.Close();
                //    if (AllRequests > 0) AllRequests--;
                //    return;
                //}

                // Begin Connect
                if (req.Method == HttpMethod.Post)
                {
                    // Event
                    string msgEvent = $"{clientEndpoint} proxying request via CONNECT to {req.FullUrl}";
                    OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                    ConnectHttpsRequest(connectionId, client, req, applyUpStreamProxy);
                    //await ConnectHttpsRequest(connectionId, client, req, applyUpStreamProxy);
                }
                else
                {
                    // Event
                    string msgEvent = $"{clientEndpoint} proxying request to {req.FullUrl}";
                    OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                    ConnectHttpRequestAsync(req, client);
                    //await ConnectHttpRequestAsync(req, client);
                }

                //if (client.Connected) client.Close();
                //if (AllRequests > 0) AllRequests--;
            }
            catch (IOException)
            {
                if (client.Connected) client.Close();
                if (AllRequests > 0) AllRequests--;
            }
            catch (Exception ex)
            {
                // Event Error
                string msgEventErr = $"Process Connection: {ex.Message}";
                OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);

                if (client.Connected) client.Close();
                if (AllRequests > 0) AllRequests--;
            }
        }

        //======================================== Connect HTTPS Request

        private async void ConnectHttpsRequest(int connectionId, TcpClient client, Request req, bool applyUpStreamProxy = false)
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
                        disposeIt();
                        return;
                    }
                }
                catch (Exception)
                {
                    // Event Error
                    string msgEventErr = $"Connect Request failed to {req.DestHostname}:{req.DestHostPort}";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                    disposeIt();
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
                if (client.Client == null)
                {
                    disposeIt();
                    return;
                }
                client.Client.Send(connectResponse);

                if (string.IsNullOrEmpty(req.SourceIp) || string.IsNullOrEmpty(req.DestIp))
                {
                    // Event Error
                    string msgEventErr = $"Source or dest IP were null or empty. SourceIp: {req.SourceIp} DestIp: {req.DestIp}";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                    disposeIt();
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
                    await Task.Delay(100);
                }
            }
            catch (SocketException)
            {
                disposeIt();
            }
            catch (Exception e)
            {
                // Event Error
                string msgEventErr = $"Connect Request: {e.Message}";
                OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                disposeIt();
            }
            finally
            {
                disposeIt();
            }

            void disposeIt()
            {
                if (_TunnelManager != null) _TunnelManager.Remove(connectionId);
                if (client != null) client.Dispose();
                if (server != null) server.Dispose();
                if (AllRequests > 0) AllRequests--;
            }
        }

        //======================================== Connect HTTP Request

        private async void ConnectHttpRequestAsync(Request req, TcpClient client)
        {
            if (Cancel) return;

            RestResponse? resp = await proxyRequest(req);
            if (resp != null)
            {
                NetworkStream ns = client.GetStream();
                await sendRestResponse(resp, ns);
                await ns.FlushAsync();
                ns.Close();

                if (client != null) client.Dispose();
                if (AllRequests > 0) AllRequests--;
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
