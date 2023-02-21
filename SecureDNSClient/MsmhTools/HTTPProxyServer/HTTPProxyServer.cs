using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace MsmhTools.HTTPProxyServer
{
    public class HTTPProxyServer : IDisposable
    {
        private bool Disposed = false;
        private Socket Server;
        private IPAddress IpAddress;
        private int Port;
        private int PCLimit;
        private List<Socket> ClientList = new();
        private bool IsStopping = false;
        public bool IsRunning { get; private set; } = false;
        private System.Timers.Timer TimerAutoClean = new();
        public bool AutoClean { get; set; } = false;
        public event EventHandler<EventArgs>? OnRequestReceived;
        public event EventHandler<EventArgs>? OnErrorOccurred;

        struct ReadObj
        {
            public Socket Socket;
            public byte[] Buffer;
            public Request Request;
        }

        public HTTPProxyServer(IPAddress ipAddress, int portNumber, int pendingConnectionLimit, bool ipv6Server = false)
        {
            if (ipv6Server)
            {
                Server = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                Server.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false); // (SocketOptionName)27 Microsoft suggestion
            }
            else
                Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IpAddress = ipAddress;
            Port = portNumber;
            PCLimit = pendingConnectionLimit;

            if (AutoClean)
            {
                TimerAutoClean.Elapsed += TimerAutoClean_Elapsed;
                TimerAutoClean.Interval = TimeSpan.FromMinutes(5).TotalMilliseconds;
                TimerAutoClean.Start();
            }
        }

        private void TimerAutoClean_Elapsed(object? sender, ElapsedEventArgs e)
        {
            CleanSockets();
        }

        public void Start()
        {
            IPEndPoint iPEndPoint = new(IpAddress, Port);
            byte[] buffer = new byte[1024];

            try
            {
                Server.Bind(iPEndPoint);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            try
            {
                Server.Listen(PCLimit);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            try
            {
                Server.BeginAccept(new AsyncCallback(AcceptClient), null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            IsRunning = true;
        }

        public void Stop()
        {
            IsStopping = true;

            foreach (Socket s in ClientList)
            {
                KillSocket(s, false);
            }

            Debug.WriteLine("Client shutdown ok");

            ClientList.Clear();

            if (IsRunning)
            {
                if (Server.Connected)
                    Server.Shutdown(SocketShutdown.Both);
                Server.Close();
                Server.Dispose();
            }

            Debug.WriteLine("Server Stopped.");

            IsStopping = false;
            IsRunning = false;
        }

        private void AcceptClient(IAsyncResult ar)
        {
            Socket? client = null;
            try
            {
                client = Server.EndAccept(ar);
            }
            catch (Exception)
            {
                return;
            }

            if (client != null)
            {
                IPEndPoint? client_ep = (IPEndPoint?)client.RemoteEndPoint;
                if (client_ep != null)
                {
                    string remoteAddress = client_ep.Address.ToString();
                    string remotePort = client_ep.Port.ToString();
                    Debug.WriteLine($"A client added: {remoteAddress}:{remotePort}");
                }

                ClientList.Add(client);

                ReadObj obj = new()
                {
                    Buffer = new byte[1024],
                    Socket = client
                };

                client.BeginReceive(obj.Buffer, 0, obj.Buffer.Length, SocketFlags.None, new AsyncCallback(ReadPackets), obj);
            }

            if (!IsStopping)
                Server.BeginAccept(new AsyncCallback(AcceptClient), null);
        }

        private void ReadPackets(IAsyncResult ar)
        {
            var asyncState = ar.AsyncState;
            if (asyncState == null) return;

            ReadObj obj = (ReadObj)asyncState;
            Socket client = obj.Socket;
            byte[] buffer = obj.Buffer;
            int read = -1;

            try
            {
                read = client.EndReceive(ar);
            }
            catch (Exception)
            {
                KillSocket(client, !IsStopping);
                Debug.WriteLine("Client Disconnected.");
                return;
            }

            if (read < 0) return;
            else if (read == 0)
            {
                KillSocket(client, !IsStopping);
                Debug.WriteLine("Client aborted session.");
                return;
            }

            string text = Encoding.ASCII.GetString(buffer, 0, read);
            Request r;

            if (obj.Request != null)
            {
                if (obj.Request.NotEnded)
                {
                    string? des = obj.Request.Full;
                    if (!string.IsNullOrEmpty(des))
                    {
                        des += text;
                        r = new Request(des);
                    }
                    else r = new Request(text);
                }
                else r = new Request(text);
            }
            else r = new Request(text);

            if (!r.NotEnded && !r.Deception)
            {
                OnRequestReceived?.Invoke(r, EventArgs.Empty);

                try
                {
                    Tunnel tunnel = new(r, client, OnErrorOccurred);
                    tunnel.SendData();
                    return;
                }
                catch (Exception ex)
                {
                    string msgError = $"Error on creating tunnel: {ex.Message}";
                    OnErrorOccurred?.Invoke(msgError, EventArgs.Empty);
                }
            }
            else if (r.NotEnded)
                obj.Request = r;

            Array.Clear(buffer, 0, buffer.Length);

            KillSocket(client, !IsStopping);
            Debug.WriteLine("Client aborted session.");
        }

        private void CleanSockets()
        {
            List<Socket> copy = ListCopy(ClientList);
            bool result = true;
            foreach (Socket socket in copy)
            {
                try
                {
                    KillSocket(socket);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Clean Sockets failed: {ex.Message}");
                    result = false;
                }
            }

            if (result)
            {
                Debug.WriteLine("All clients disconnected.");
            }
            else
            {
                Debug.WriteLine("Some clients failed to disconnect.");
            }

            Array.Clear(copy.ToArray(), 0, copy.Count);
        }

        public static List<Socket> ListCopy(List<Socket> input)
        {
            List<Socket> result = new();
            foreach (Socket item in input)
                result.Add(item);
            return result;
        }

        private void KillSocket(Socket client, bool autoRemove = true)
        {
            if (autoRemove && ClientList != null)
                ClientList.Remove(client);

            try
            {
                client.Shutdown(SocketShutdown.Both);
                client.Disconnect(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Killsocket failed: {ex.Message}");
            }
            client.Close();
            client.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed) return;
            if (disposing)
            {
                if (IsRunning)
                {
                    Stop();
                    Server.Dispose();
                }

                if (TimerAutoClean != null)
                {
                    TimerAutoClean.Stop();
                    TimerAutoClean.Dispose();
                }

                IpAddress = IPAddress.None;
                ClientList.Clear();
            }

            Disposed = true;
        }
    }
}
