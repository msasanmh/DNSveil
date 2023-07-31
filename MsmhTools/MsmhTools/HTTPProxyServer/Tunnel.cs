using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MsmhTools.HTTPProxyServer
{ 
    /// <summary>
    /// CONNECT tunnel.
    /// </summary>
    public class Tunnel : IDisposable
    {
        /// <summary>
        /// UTC timestamp when the session was started.
        /// </summary>
        public DateTime TimestampUtc { get; set; }

        public Request Request { get; set; }

        /// <summary>
        /// The TCP client instance for the requestor.
        /// </summary>
        public TcpClient ClientTcpClient { get; set; }

        /// <summary>
        /// The TCP client instance for the server.
        /// </summary>
        public TcpClient ServerTcpClient { get; set; }

        /// <summary>
        /// The data stream for the client.
        /// </summary>
        public Stream ClientStream { get; set; }

        /// <summary>
        /// The data stream for the server.
        /// </summary>
        public Stream ServerStream { get; set; }

        private bool _Active = true;
        private readonly Stopwatch KillOnTimeout = new();

        public event EventHandler<EventArgs>? OnErrorOccurred;
        public event EventHandler<EventArgs>? OnDebugInfoReceived;

        /// <summary>
        /// Construct a Tunnel object.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <param name="client">TCP client instance of the client.</param>
        /// <param name="server">TCP client instance of the server.</param>
        public Tunnel(
            Request request,
            TcpClient client, 
            TcpClient server)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.SourceIp)) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.DestIp)) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.DestHostname)) throw new ArgumentNullException(nameof(request));
            if (request.SourcePort < 0) throw new ArgumentOutOfRangeException(nameof(request));
            if (request.DestPort < 0) throw new ArgumentOutOfRangeException(nameof(request));
            if (request.DestHostPort < 0) throw new ArgumentOutOfRangeException(nameof(request));

            Request = request;
            TimestampUtc = DateTime.Now.ToUniversalTime();

            ClientTcpClient = client ?? throw new ArgumentNullException(nameof(client));
            ClientTcpClient.NoDelay = true;
            ClientTcpClient.Client.NoDelay = true;

            ServerTcpClient = server ?? throw new ArgumentNullException(nameof(server));
            ServerTcpClient.NoDelay = true;
            ServerTcpClient.Client.NoDelay = true;
            
            ClientStream = client.GetStream();
            ServerStream = server.GetStream();

            Parallel.Invoke(
                () => ClientReaderAsync(),
                () => ServerReaderAsync()
                );

            //Task.Run(async () => await ClientReaderAsync());
            //Task.Run(async () => await ServerReaderAsync());

            _Active = true;
        }

        /// <summary>
        /// Human-readable string.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            return TimestampUtc.ToString("MM/dd/yyyy HH:mm:ss") + " " + Source() + " to " + Destination();
        }

        /// <summary>
        /// Returns the source IP and port.
        /// </summary>
        /// <returns>String.</returns>
        public string Source()
        {
            return Request.SourceIp + ":" + Request.SourcePort;
        }

        /// <summary>
        /// Returns the destination IP and port along wit destination hostname and port.
        /// </summary>
        /// <returns>String.</returns>
        public string Destination()
        {
            return Request.DestIp + ":" + Request.DestPort + " [" + Request.DestHostname + ":" + Request.DestHostPort + "]";
        }

        /// <summary>
        /// Determines whether or not the tunnel is active.
        /// </summary>
        /// <returns>True if both connections are active.</returns>
        public bool IsActive()
        {
            bool clientActive = false;
            bool serverActive = false;
            bool clientSocketActive = false;
            bool serverSocketActive = false;

            if (ClientTcpClient != null)
            {
                clientActive = ClientTcpClient.Connected;

                if (ClientTcpClient.Client != null)
                {
                    TcpState clientState = GetTcpRemoteState(ClientTcpClient);

                    if (clientState == TcpState.Established
                        || clientState == TcpState.Listen
                        || clientState == TcpState.SynReceived
                        || clientState == TcpState.SynSent
                        || clientState == TcpState.TimeWait)
                    {
                        clientSocketActive = true;
                    }
                }
            }

            if (ServerTcpClient != null)
            {
                serverActive = ServerTcpClient.Connected;

                if (ServerTcpClient.Client != null)
                {
                    // see https://github.com/jchristn/PuppyProxy/compare/master...waldekmastykarz:PuppyProxy:master

                    /*
                    TcpState serverState = GetTcpRemoteState(ServerTcpClient);

                    if (serverState == TcpState.Established
                        || serverState == TcpState.Listen
                        || serverState == TcpState.SynReceived
                        || serverState == TcpState.SynSent
                        || serverState == TcpState.TimeWait)
                    {
                        serverSocketActive = true;
                    }
                    */

                    serverSocketActive = true;
                }
            }

            if (Request.TimeoutSec != 0 &&
                KillOnTimeout.ElapsedMilliseconds > TimeSpan.FromSeconds(Request.TimeoutSec).TotalMilliseconds)
            {
                string msg = $"Killed Request On Timeout({Request.TimeoutSec} Sec): {Request.DestHostname}:{Request.DestHostPort}";
                OnDebugInfoReceived?.Invoke(msg, EventArgs.Empty);
                Debug.WriteLine(msg);
                _Active = false;
                KillOnTimeout.Stop();
            }
            else
                _Active = clientActive && clientSocketActive && serverActive && serverSocketActive;
            return _Active;
        }

        /// <summary>
        /// Tear down the tunnel object and resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (ClientStream != null)
            {
                ClientStream.Close();
                ClientStream.Dispose();
            }

            if (ServerStream != null)
            {
                ServerStream.Close();
                ServerStream.Dispose();
            }

            if (ClientTcpClient != null)
                ClientTcpClient.Dispose();

            if (ServerTcpClient != null)
                ServerTcpClient.Dispose();
        }

        private bool StreamReadSync(TcpClient client, out byte[]? data)
        {
            data = null;
            try
            {
                Stream stream = client.GetStream();

                int read = 0;
                long bufferSize = 65536;
                byte[] buffer = new byte[bufferSize];

                read = stream.Read(buffer, 0, buffer.Length);
                if (read > 0)
                {
                    if (read == bufferSize)
                    {
                        data = buffer;
                        return true;
                    }
                    else
                    {
                        data = new byte[read];
                        Buffer.BlockCopy(buffer, 0, data, 0, read);
                        return true;
                    }
                }
                else
                {
                    data = null;
                    return true;
                }
            }
            catch (InvalidOperationException)
            {
                _Active = false;
                return false;
            }
            catch (IOException)
            {
                _Active = false;
                return false;
            }
            catch (Exception e)
            {
                // Event Error
                string msgEventErr = $"Stream Read Sync: {e.Message}";
                OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                _Active = false;
                return false;
            }
            finally
            {
                // do nothing
            }
        }

        private async Task<byte[]?> StreamReadAsync(TcpClient client)
        {
            var task = await Task.Run(async () =>
            {
                try
                {
                    Stream stream = client.GetStream();
                    if (stream == null) return null;

                    byte[] buffer = new byte[65536];

                    using MemoryStream memStream = new();
                    int read = await stream.ReadAsync(buffer);
                    if (read > 0)
                    {
                        if (read == buffer.Length)
                        {
                            return buffer;
                        }
                        else
                        {
                            byte[] data = new byte[read];
                            Buffer.BlockCopy(buffer, 0, data, 0, read);
                            return data;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (InvalidOperationException)
                {
                    _Active = false;
                    return null;
                }
                catch (IOException)
                {
                    _Active = false;
                    return null;
                }
                catch (Exception e)
                {
                    // Event Error
                    string msgEventErr = $"Stream Read Async: {e.Message}";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                    _Active = false;
                    return null;
                }
                finally
                {
                    // do nothing
                }
            });

            return task;
        }

        private TcpState GetTcpLocalState(TcpClient tcpClient)
        {
            try
            {
                if (tcpClient.Client == null)
                    return TcpState.Unknown;

                IPGlobalProperties ipgp = IPGlobalProperties.GetIPGlobalProperties();
                if (ipgp != null)
                {
                    TcpConnectionInformation[]? tcis = ipgp.GetActiveTcpConnections();
                    if (tcis != null)
                    {
                        for (int n = 0; n < tcis.Length; n++)
                        {
                            TcpConnectionInformation? tci = tcis[n];
                            if (tci != null)
                            {
                                if (tcpClient.Client != null)
                                {
                                    if (tci.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint))
                                        return tci.State;
                                }
                            }
                        }
                    }
                }

                return TcpState.Unknown;
            }
            catch (Exception)
            {
                return TcpState.Unknown;
            }
        }

        private TcpState GetTcpRemoteState(TcpClient tcpClient)
        {
            try
            {
                if (tcpClient.Client == null)
                    return TcpState.Unknown;

                IPGlobalProperties ipgp = IPGlobalProperties.GetIPGlobalProperties();
                if (ipgp != null)
                {
                    TcpConnectionInformation[]? tcis = ipgp.GetActiveTcpConnections();
                    if (tcis != null)
                    {
                        for (int n = 0; n < tcis.Length; n++)
                        {
                            TcpConnectionInformation? tci = tcis[n];
                            if (tci != null)
                            {
                                if (tcpClient.Client != null)
                                {
                                    if (tci.RemoteEndPoint.Equals(tcpClient.Client.RemoteEndPoint))
                                        return tci.State;
                                }
                            }
                        }
                    }
                }

                return TcpState.Unknown;
            }
            catch (Exception)
            {
                return TcpState.Unknown;
            }
        }

        private void ClientReaderSync()
        {
            try
            {
                // Event
                string msgEvent = $"ClientReaderSync started for {Source()} to {Destination()}";
                OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                byte[]? data = null;
                while (true)
                {
                    if (StreamReadSync(ClientTcpClient, out data))
                    {
                        if (data != null && data.Length > 0)
                        {
                            // Event
                            msgEvent = $"ClientReaderSync {Source()} to {Destination()} read {data.Length} bytes.";
                            OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                            ServerSend(data);

                            data = null;
                        }
                        else
                        {
                            // Event
                            msgEvent = "ClientReaderSync no data returned.";
                            OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);
                        }
                    }
                    else
                    { 
                        _Active = false;
                        return;
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                _Active = false;
            }
            catch (SocketException)
            {
                _Active = false;
            }
            catch (Exception e)
            {
                // Event Error
                string msgEventErr = $"ClientReaderSync: {e.Message}";
                OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                _Active = false;
            }
        }

        private void ServerReaderSync()
        {
            try
            {
                // Event
                string msgEvent = $"ServerReaderSync started for {Source()} to {Destination()}";
                OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                byte[]? data = null;
                while (true)
                {
                    if (StreamReadSync(ServerTcpClient, out data))
                    {
                        if (data != null && data.Length > 0)
                        {
                            // Event
                            msgEvent = $"ServerReaderSync {Destination()} to {Source()} read {data.Length} bytes.";
                            OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                            ClientSend(data);

                            data = null;
                        }
                        else
                        {
                            // Event
                            msgEvent = "ServerReaderSync no data returned.";
                            OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);
                        }
                    }
                    else
                    { 
                        _Active = false;
                        return;
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                _Active = false;
            }
            catch (SocketException)
            {
                _Active = false;
            }
            catch (Exception e)
            {
                // Event Error
                string msgEventErr = $"ServerReaderSync: {e.Message}";
                OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                _Active = false;
            }
        }
        
        private async void ClientReaderAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    // Event
                    string msgEvent = $"ClientReaderAsync started for {Source()} to {Destination()}";
                    OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                    byte[]? data = null;
                    while (true)
                    {
                        data = await StreamReadAsync(ClientTcpClient);

                        if (data != null && data.Length > 0)
                        {
                            if (!KillOnTimeout.IsRunning) KillOnTimeout.Start();
                            KillOnTimeout.Restart();

                            // Event
                            msgEvent = $"ClientReaderAsync {Source()} to {Destination()} read {data.Length} bytes.";
                            OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                            if (ClientTcpClient.Connected)
                                ServerSend(data);
                            
                            data = null;
                        }
                        
                        if (!_Active) break;
                    }
                }
                catch (ObjectDisposedException)
                {
                    _Active = false;
                }
                catch (SocketException)
                {
                    _Active = false;
                }
                catch (Exception e)
                {
                    // Event Error
                    string msgEventErr = $"ClientReaderAsync: {e.Message}";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                    _Active = false;
                }
            });
        }

        private async void ServerReaderAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    // Event
                    string msgEvent = $"ServerReaderAsync started for {Source()} to {Destination()}";
                    OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                    byte[]? data = null;
                    while (true)
                    {
                        data = await StreamReadAsync(ServerTcpClient);

                        if (data != null && data.Length > 0)
                        {
                            // Event
                            msgEvent = $"ServerReaderAsync {Destination()} to {Source()} read {data.Length} bytes.";
                            OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

                            if (ServerTcpClient.Connected)
                                ClientSend(data);

                            data = null;
                        }

                        if (!_Active) break;
                    }
                }
                catch (ObjectDisposedException)
                {
                    _Active = false;
                }
                catch (SocketException)
                {
                    _Active = false;
                }
                catch (Exception e)
                {
                    // Event Error
                    string msgEventErr = $"ServerReaderAsync: {e.Message}";
                    OnErrorOccurred?.Invoke(msgEventErr, EventArgs.Empty);
                    _Active = false;
                }
            });
        }

        private void ClientSend(byte[] data)
        {
            // We don't have TLS Handshake here
            if (ClientTcpClient.Client == null) return;
            if (!ClientTcpClient.Connected) return;
            ClientTcpClient.Client.Send(data);
        }

        private void ServerSend(byte[] data)
        {
            // We do have TLS Handshake here
            if (ServerTcpClient.Client == null) return;
            if (!ServerTcpClient.Connected) return;
            if (Request.ApplyDpiBypass)
                Send(data, ServerTcpClient.Client);
            else
                ServerTcpClient.Client.Send(data);
        }

        public HTTPProxyServer.Program.DPIBypass ConstantDPIBypass { get; set; } = new();
        public void EnableDPIBypass(HTTPProxyServer.Program.DPIBypass dpiBypass)
        {
            ConstantDPIBypass = dpiBypass;
            ConstantDPIBypass.DestHostname = Request.DestHostname;
            ConstantDPIBypass.DestPort = Request.DestHostPort;
        }

        private void Send(byte[] data, Socket? socket)
        {
            if (socket != null)
            {
                if (ConstantDPIBypass.DPIBypassMode == HTTPProxyServer.Program.DPIBypass.Mode.Disable)
                {
                    HTTPProxyServer.Program.DPIBypass bp = HTTPProxyServer.StaticDPIBypassProgram;
                    bp.DestHostname = Request.DestHostname;
                    bp.DestPort = Request.DestHostPort;
                    if (bp.DPIBypassMode == HTTPProxyServer.Program.DPIBypass.Mode.Program)
                    {
                        HTTPProxyServer.Program.DPIBypass.ProgramMode programMode = new(data, socket);
                        programMode.Send(bp);
                    }
                    else
                        socket.Send(data);
                }
                else
                {
                    HTTPProxyServer.Program.DPIBypass.ProgramMode programMode = new(data, socket);
                    programMode.Send(ConstantDPIBypass);
                }
                
            }
        }

    }
}
