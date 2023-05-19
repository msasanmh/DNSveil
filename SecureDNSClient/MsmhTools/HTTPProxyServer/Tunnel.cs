using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        /// <summary>
        /// Source IP address.
        /// </summary>
        public string SourceIp { get; set; }

        /// <summary>
        /// Source TCP port.
        /// </summary>
        public int SourcePort { get; set; } 

        /// <summary>
        /// Destination IP address.
        /// </summary>
        public string DestIp { get; set; }

        /// <summary>
        /// Destination TCP port.
        /// </summary>
        public int DestPort { get; set; }

        /// <summary>
        /// Destination hostname.
        /// </summary>
        public string DestHostname { get; set; }

        /// <summary>
        /// Destination host port.
        /// </summary>
        public int DestHostPort { get; set; }

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

        private DPIBypass.Mode DpiBypassMode = DPIBypass.Mode.Disable;
        private int FirstPartOfDataLength = 100;
        private int ProgramFragmentSize = 1;
        private int DivideBy = 100;
        private int FragmentLength = 7;
        private int FragmentDelay = 0;

        public event EventHandler<EventArgs>? OnErrorOccurred;
        public event EventHandler<EventArgs>? OnDebugInfoReceived;

        #region Constructors-and-Factories

        /// <summary>
        /// Construct a Tunnel object.
        /// </summary>
        public Tunnel()
        {

        }

        /// <summary>
        /// Construct a Tunnel object.
        /// </summary>
        /// <param name="logging">Logging module instance.</param>
        /// <param name="sourceIp">Source IP address.</param>
        /// <param name="sourcePort">Source TCP port.</param>
        /// <param name="destIp">Destination IP address.</param>
        /// <param name="destPort">Destination TCP port.</param>
        /// <param name="destHostname">Destination hostname.</param>
        /// <param name="destHostPort">Destination host port.</param>
        /// <param name="client">TCP client instance of the client.</param>
        /// <param name="server">TCP client instance of the server.</param>
        public Tunnel(
            string sourceIp, 
            int sourcePort, 
            string destIp, 
            int destPort, 
            string destHostname,
            int destHostPort,
            TcpClient client, 
            TcpClient server)
        {
            if (string.IsNullOrEmpty(sourceIp)) throw new ArgumentNullException(nameof(sourceIp));
            if (string.IsNullOrEmpty(destIp)) throw new ArgumentNullException(nameof(destIp));
            if (string.IsNullOrEmpty(destHostname)) throw new ArgumentNullException(nameof(destHostname));
            if (sourcePort < 0) throw new ArgumentOutOfRangeException(nameof(sourcePort));
            if (destPort < 0) throw new ArgumentOutOfRangeException(nameof(destPort));
            if (destHostPort < 0) throw new ArgumentOutOfRangeException(nameof(destHostPort));

            TimestampUtc = DateTime.Now.ToUniversalTime();
            SourceIp = sourceIp;
            SourcePort = sourcePort; 
            DestIp = destIp;
            DestPort = destPort;
            DestHostname = destHostname;
            DestHostPort = destHostPort;

            ClientTcpClient = client ?? throw new ArgumentNullException(nameof(client));
            ClientTcpClient.NoDelay = true;
            ClientTcpClient.Client.NoDelay = true;

            ServerTcpClient = server ?? throw new ArgumentNullException(nameof(server));
            ServerTcpClient.NoDelay = true;
            ServerTcpClient.Client.NoDelay = true;
            
            ClientStream = client.GetStream();
            ServerStream = server.GetStream();
             
            Task.Run(() => ClientReaderAsync());
            Task.Run(() => ServerReaderAsync());

            _Active = true;
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

        public void PassTheEvents(EventHandler<EventArgs>? onRequestReceived = null,
                                  EventHandler<EventArgs>? onErrorOccurred = null,
                                  EventHandler<EventArgs>? onDebugInfoReceived = null)
        {
            OnErrorOccurred += Tunnel_OnErrorOccurred;
            void Tunnel_OnErrorOccurred(object? sender, EventArgs e)
            {
                if (sender is string msg && onErrorOccurred != null)
                    onErrorOccurred?.Invoke(msg, EventArgs.Empty);
            }

            OnDebugInfoReceived += Tunnel_OnDebugInfoReceived;
            void Tunnel_OnDebugInfoReceived(object? sender, EventArgs e)
            {
                if (sender is string msg && onDebugInfoReceived != null)
                    onDebugInfoReceived?.Invoke(msg, EventArgs.Empty);
            }
        }

        #endregion

        #region Public-Methods

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
            return SourceIp + ":" + SourcePort;
        }

        /// <summary>
        /// Returns the destination IP and port along wit destination hostname and port.
        /// </summary>
        /// <returns>String.</returns>
        public string Destination()
        {
            return DestIp + ":" + DestPort + " [" + DestHostname + ":" + DestHostPort + "]";
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

            _Active = _Active && clientActive && clientSocketActive && serverActive && serverSocketActive;
            return _Active;
        }

        /// <summary>
        /// Returns the metadata of the tunnel.
        /// </summary>
        /// <returns>Tunnel object without TCP instances or streams.</returns>
        public Tunnel Metadata()
        {
            Tunnel ret = new();
            ret.DestHostname = DestHostname;
            ret.DestHostPort = DestHostPort;
            ret.DestIp = DestIp;
            ret.DestPort = DestPort;
            ret.SourceIp = SourceIp;
            ret.SourcePort = SourcePort;
            ret.TimestampUtc = TimestampUtc;
            return ret;
        }

        /// <summary>
        /// Tear down the tunnel object and resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Private-Methods

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
            }
        }

        private async Task<byte[]?> StreamReadAsync(TcpClient client)
        {
            try
            { 
                Stream stream = client.GetStream();
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
            }
        }

        private TcpState GetTcpLocalState(TcpClient tcpClient)
        {
            var state = IPGlobalProperties.GetIPGlobalProperties()
              .GetActiveTcpConnections()
              .FirstOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint));
            return state != null ? state.State : TcpState.Unknown;
        }

        private TcpState GetTcpRemoteState(TcpClient tcpClient)
        {
            var state = IPGlobalProperties.GetIPGlobalProperties()
              .GetActiveTcpConnections()
              .FirstOrDefault(x => x.RemoteEndPoint.Equals(tcpClient.Client.RemoteEndPoint));
            return state != null ? state.State : TcpState.Unknown;
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
                        // Event
                        msgEvent = $"ClientReaderAsync {Source()} to {Destination()} read {data.Length} bytes.";
                        OnDebugInfoReceived?.Invoke(msgEvent, EventArgs.Empty);

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
        }

        private async void ServerReaderAsync()
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
        }

        private void ClientSend(byte[] data)
        {
            Send(data, ClientTcpClient.Client);
        }

        private void ServerSend(byte[] data)
        {
            Send(data, ServerTcpClient.Client);
        }

        private void Send(byte[] data, Socket? socket)
        {
            if (socket != null)
            {
                if (DpiBypassMode == DPIBypass.Mode.Program)
                {
                    DPIBypass.ProgramMode programMode = new(data, socket);
                    programMode.Send(FirstPartOfDataLength, ProgramFragmentSize, DivideBy, FragmentDelay);
                }
                else if (DpiBypassMode == DPIBypass.Mode.Random)
                {
                    DPIBypass.RandomMode randomMode = new(data, socket);
                    randomMode.Send(FragmentLength, FragmentDelay);
                }
                else
                    socket.Send(data);
            }
        }

        #endregion
    }
}
