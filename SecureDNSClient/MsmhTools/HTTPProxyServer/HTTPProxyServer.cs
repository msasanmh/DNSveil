using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RestWrapper;
using WatsonWebserver;
using System.Text;
using System.Diagnostics;

namespace MsmhTools.HTTPProxyServer
{
    public class HTTPProxyServer
    {
        private static ProxySettings _Settings = new();

        private static TunnelManager? _Tunnels;
        private static TcpListener? _TcpListener;

        private static CancellationTokenSource? _CancelTokenSource;
        private static CancellationToken _CancelToken;
        private static int _ActiveThreads = 0;

        public bool IsRunning = false;
        public static bool Cancel = false;

        public static readonly int MaxDataSize = 65536;
        private DPIBypass.Mode DpiBypassMode = DPIBypass.Mode.Disable;
        private int FirstPartOfDataLength = 100;
        private int ProgramFragmentSize = 1;
        private int DivideBy = 100;
        private int FragmentLength = 7;
        private int FragmentDelay = 0;

        public event EventHandler<EventArgs>? OnRequestReceived;
        public event EventHandler<EventArgs>? OnErrorOccurred;
        public event EventHandler<EventArgs>? OnDebugInfoReceived;
        private static readonly EventWaitHandle Terminator = new(false, EventResetMode.ManualReset);

        public HTTPProxyServer()
        {
            
        }

        public void Start(IPAddress ipAddress, int port, int maxThreads, bool ssl)
        {
            Task.Run(() =>
            {
                if (IsRunning) return;
                IsRunning = true;
                
                _Settings = new ProxySettings();
                _Settings.Proxy.ListenerIpAddress = ipAddress;
                _Settings.Proxy.ListenerPort = port;
                _Settings.Proxy.MaxThreads = maxThreads;
                _Settings.Proxy.Ssl = ssl;

                Welcome();

                _Tunnels = new TunnelManager();

                _CancelTokenSource = new CancellationTokenSource();
                _CancelToken = _CancelTokenSource.Token;

                Cancel = false;
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
                _TcpListener.Stop();
                IsRunning = _TcpListener.Server.IsBound;
                Goodbye();
            }
        }

        public int ListeningPort
        {
            get => _Settings.Proxy.ListenerPort;
        }

        public Dictionary<int, Tunnel> GetCurrentConnectTunnels()
        {
            return _Tunnels != null ? _Tunnels.GetMetadata() : new Dictionary<int, Tunnel>();
        }

        #region Setup-Methods

        private void Welcome()
        {
            // Event
            string msgEvent = $"HTTP Proxy Server starting on {_Settings.Proxy.ListenerIpAddress}:{_Settings.Proxy.ListenerPort}";
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

        public void EnableDpiBypassProgram(DPIBypass.Mode mode, int firstPartOfDataLength, int programFragmentSize, int divideBy, int fragmentDelay = 0)
        {
            DpiBypassMode = mode;
            FirstPartOfDataLength = firstPartOfDataLength;
            ProgramFragmentSize = programFragmentSize;
            DivideBy = divideBy;
            FragmentDelay = fragmentDelay;
        }
        
        public void EnableDpiBypassRandom(DPIBypass.Mode mode, int fragmentLength, int fragmentDelay = 0)
        {
            DpiBypassMode = mode;
            FragmentLength = fragmentLength;
            FragmentDelay = fragmentDelay;
        }

        #endregion

        #region Connection-Handler

        private void AcceptConnections()
        {
            try
            {
                _TcpListener = new TcpListener(_Settings.Proxy.ListenerIpAddress, _Settings.Proxy.ListenerPort);

                _TcpListener.Start();
                Task.Delay(200).Wait();

                if (_TcpListener != null)
                {
                    IsRunning = _TcpListener.Server.IsBound;

                    while (!Cancel)
                    {
                        TcpClient client = _TcpListener.AcceptTcpClient();
                        Task.Run(() => ProcessConnection(client), _CancelToken);
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
                string msgEventErr = $"Accept Connections: {eOuter.Message}";
                OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
            }
        }
        
        private async Task ProcessConnection(TcpClient client)
        {
            string clientIp = string.Empty;
            int clientPort = 0;
            int connectionId = Environment.CurrentManagedThreadId;
            _ActiveThreads++;
            Debug.WriteLine($"Active Requests: {_ActiveThreads}");

            try
            {
                // Check-if-Max-Exceeded
                if (_ActiveThreads >= _Settings.Proxy.MaxThreads)
                {
                    // Event
                    string msgEventErr = $"AcceptConnections connection count {_ActiveThreads} exceeds configured max {_Settings.Proxy.MaxThreads}, waiting";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);

                    while (_ActiveThreads >= _Settings.Proxy.MaxThreads)
                    {
                        Task.Delay(100).Wait();
                    }
                }

                IPEndPoint? clientIpEndpoint = client.Client.RemoteEndPoint as IPEndPoint;
                IPEndPoint? serverIpEndpoint = client.Client.LocalEndPoint as IPEndPoint;

                string clientEndpoint = clientIpEndpoint != null ? clientIpEndpoint.ToString() : string.Empty;
                string serverEndpoint = serverIpEndpoint != null ? serverIpEndpoint.ToString() : string.Empty;

                clientIp = clientIpEndpoint != null ? clientIpEndpoint.Address.ToString() : string.Empty;
                clientPort = clientIpEndpoint != null ? clientIpEndpoint.Port : 0;

                string serverIp = serverIpEndpoint != null ? serverIpEndpoint.Address.ToString() : string.Empty;
                int serverPort = serverIpEndpoint != null ? serverIpEndpoint.Port : 0;

                HttpRequest req = HttpRequest.FromTcpClient(client);
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

                // Event
                string msgEvent = $"{req.DestHostname}:{req.DestHostPort}";
                OnRequestReceived?.Invoke(msgEvent, EventArgs.Empty);

                if (req.Method == WatsonWebserver.HttpMethod.CONNECT)
                {
                    // Event
                    msgEvent = $"{clientEndpoint} proxying request via CONNECT to {req.FullUrl}";
                    OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                    ConnectRequest(connectionId, client, req);
                }
                else
                {
                    // Event
                    msgEvent = $"{clientEndpoint} proxying request to {req.FullUrl}";
                    OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                    RestResponse? resp = ProxyRequest(req).Result;
                    if (resp != null)
                    {
                        NetworkStream ns = client.GetStream();
                        await SendRestResponse(resp, ns);
                        await ns.FlushAsync();
                        ns.Close();
                    }
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
                string msgEventErr = $"Accept Connections: {eInner.Message}";
                OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
            }
        }

        private async Task<RestResponse?> ProxyRequest(HttpRequest request)
        {
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

                RestRequest req = new(
                    request.FullUrl,
                    (RestWrapper.HttpMethod)(Enum.Parse(typeof(RestWrapper.HttpMethod), request.Method.ToString())),
                    request.Headers,
                    request.ContentType);

                if (request.ContentLength > 0)
                {
                    return await req.SendAsync(request.ContentLength, request.Data);
                }
                else
                {
                    return await req.SendAsync();
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

        private void ConnectRequest(int connectionId, TcpClient client, HttpRequest req)
        {
            Tunnel? currTunnel = null;
            TcpClient? server = null;

            try
            {
                client.NoDelay = true;
                client.Client.NoDelay = true;

                server = new TcpClient();

                try
                {
                    server.Connect(req.DestHostname, req.DestHostPort);
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
                client.Client.Send(connectResponse);

                currTunnel = new Tunnel(
                    req.SourceIp,
                    req.SourcePort,
                    req.DestIp,
                    req.DestPort,
                    req.DestHostname,
                    req.DestHostPort,
                    client,
                    server);

                // Pass the events
                currTunnel.PassTheEvents(OnRequestReceived, OnErrorOccurred);

                // Pass DPI bypass
                if (DpiBypassMode == DPIBypass.Mode.Program)
                    currTunnel.EnableDpiBypassProgram(DPIBypass.Mode.Program, FirstPartOfDataLength, ProgramFragmentSize, DivideBy, FragmentDelay);
                else if (DpiBypassMode == DPIBypass.Mode.Random)
                    currTunnel.EnableDpiBypassRandom(DPIBypass.Mode.Random, FragmentLength, FragmentDelay);

                if (_Tunnels != null)
                    _Tunnels.Add(connectionId, currTunnel);

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
                if (_Tunnels != null)
                    _Tunnels.Remove(connectionId);

                if (client != null)
                {
                    client.Dispose();
                }

                if (server != null)
                {
                    server.Dispose();
                }
            }
        }

        private static byte[] ConnectResponse()
        {
            string resp = "HTTP/1.1 200 Connection Established\r\nConnection: close\r\n\r\n";
            return Encoding.UTF8.GetBytes(resp);
        }

        private async Task SendRestResponse(RestResponse resp, NetworkStream ns)
        {
            try
            {
                byte[]? ret = null;
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

        #endregion
    }
}
