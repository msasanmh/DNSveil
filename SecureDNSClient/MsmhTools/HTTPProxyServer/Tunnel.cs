using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MsmhTools.HTTPProxyServer
{
    public class Tunnel : IDisposable
    {
        private bool Disposed = false;
        private Request Request;
        private Socket Client;
        public string Host = string.Empty;
        public int Port = 0;
        public bool TunnelDestroyed { get; private set; } = false;
        public event EventHandler<EventArgs>? OnErrorOccurred;

        public Tunnel(Request r, Socket client, EventHandler<EventArgs>? onErrorOccurred = null)
        {
            Request = r;
            Client = client;

            string hostPort = r.Headers["Host"];
            GetHostAndPort(hostPort, 443, out string outHost, out int outPort);
            Host = outHost;
            Port = outPort;
            GenerateVerify();

            OnErrorOccurred += Tunnel_OnErrorOccurred;
            void Tunnel_OnErrorOccurred(object? sender, EventArgs e)
            {
                if (sender is string msg && onErrorOccurred != null)
                    onErrorOccurred?.Invoke(msg, EventArgs.Empty);
            }
        }

        public void SendData()
        {
            try
            {
                Socket bridge;
                if (IsHostIPv6(Host))
                {
                    bridge = new(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                    bridge.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false); // (SocketOptionName)27 Microsoft suggestion
                }
                else
                    bridge = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    bridge.Connect(Host, Port);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("HTTP(S) Socket: " + ex.Message);
                    Debug.WriteLine(Host + ":" + Port);
                    string msgEvent = $"HTTP(S) Socket: {ex.Message}{Environment.NewLine}{Host}:{Port}";
                    OnErrorOccurred?.Invoke(msgEvent, EventArgs.Empty);
                    return;
                }

                RawSSLObj rso = new()
                {
                    FullText = string.Empty,
                    Request = null,
                    RawData = new RawObj
                    {
                        Data = new byte[2048],
                        Client = Client,
                        Bridge = bridge
                    }
                };

                RawObj ro = new()
                {
                    Client = Client,
                    Data = new byte[2048],
                    Bridge = bridge
                };

                bridge.BeginReceive(ro.Data, 0, 2048, SocketFlags.None, new AsyncCallback(ForwardRawHTTP), ro);
                if (Client.Connected)
                {
                    Client.BeginReceive(rso.RawData.Data, 0, 2048, SocketFlags.None, new AsyncCallback(ReadClient), rso);
                    GenerateVerify(Client);
                }

                // For HTTP
                if (Request.Method != "CONNECT" || Port == 80)
                {
                    string? code = FormatRequest(Request);
                    if (code != null)
                        bridge.Send(Encoding.ASCII.GetBytes(code));
                }
            }
            catch (SocketException socketException)
            {
                Debug.WriteLine($"Failed to tunnel HTTP(S) traffic for {Host}: {socketException.Message}");
                Debug.WriteLine(Host + ":" + Port);
                string msgEvent = $"Failed to tunnel HTTP(S) traffic: {socketException.Message}{Environment.NewLine}{Host}:{Port}";
                OnErrorOccurred?.Invoke(msgEvent, EventArgs.Empty);
            }
        }

        private static void GetHostAndPort(string hostPort, int defaultPort, out string outHost, out int outPort)
        {
            outHost = hostPort.Trim();
            outPort = defaultPort;

            if (outHost.Contains("]:"))
            {
                // IPv6 domain
                string[] split1 = outHost.Split("]:");
                if (split1.Length == 2)
                {
                    outHost = split1[0] + "]";
                    outPort = Convert.ToInt32(split1[1]);
                }
            }
            else if (!outHost.Contains(']') && outHost.Contains(':'))
            {
                // Domain or IPv4
                string[] split2 = outHost.Split(':');
                if (split2.Length == 2)
                {
                    outHost = split2[0];
                    outPort = Convert.ToInt32(split2[1]);
                }
            }
        }

        private static bool IsHostIPv6(string host)
        {
            bool isIPv6 = false;
            if (host.Contains(']'))
            {
                if (host.Contains(':'))
                    isIPv6 = true;
            }
            return isIPv6;
        }

        private static IPAddress? HostToIP(string hostname)
        {
            if (!IPAddress.TryParse(hostname, out IPAddress? address))
            {
                IPAddress[] ips = Dns.GetHostAddresses(hostname);
                return (ips.Length > 0) ? ips[0] : null;
            }
            else return address;
        }

        private static void GenerateVerify(Socket? clientSocket = null)
        {
            string verifyResponse = "HTTP/1.1 200 OK Tunnel Created\r\nTimestamp: " + DateTime.Now + "\r\nProxy-Agent: ah101\r\n\r\n";
            byte[] resp = Encoding.ASCII.GetBytes(verifyResponse);

            if (clientSocket != null)
            {
                clientSocket.Send(resp, 0, resp.Length, SocketFlags.None);
                return;
            }
        }

        public string? FormatRequest(Request r)
        {
            if (TunnelDestroyed) return null;

            if (Host == null)
            {
                Generate404();
                return null;
            }

            string toSend = r.Deserialize();
            List<string> lines = toSend.Split('\n').ToList();
            lines[0] = lines[0].Replace("http://", string.Empty);
            lines[0] = lines[0].Replace("https://", string.Empty);
            lines[0] = lines[0].Replace(Host, string.Empty);
            toSend = string.Empty;

            foreach (string line in lines)
            {
                toSend += line + "\n";
            }

            return toSend;
        }

        private void Generate404()
        {
            string text = "HTTP/1.1 404 Not Found\r\nTimestamp: " + DateTime.Now + "\r\nProxy-Agent: ah101\r\n\r\n";
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            Client.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private struct RawObj
        {
            public byte[] Data;
            public Socket? Client;
            public Socket? Bridge;
        }

        private struct RawSSLObj
        {
            public RawObj RawData;
            public Request? Request;
            public string FullText;
        }

        private void ForwardRawHTTP(IAsyncResult ar)
        {
            try
            {
                var asyncState = ar.AsyncState;
                if (asyncState == null) return;

                RawObj data = (RawObj)asyncState;
                if (data.Client == null || data.Bridge == null) return;
                if (!data.Client.Connected || !data.Bridge.Connected) return;
                
                int bytesRead = data.Bridge.EndReceive(ar);

                if (bytesRead > 0)
                {
                    byte[] toSend = new byte[bytesRead];
                    Array.Copy(data.Data, toSend, bytesRead);
                    data.Client.Send(toSend, 0, bytesRead, SocketFlags.None);
                    Array.Clear(toSend, 0, bytesRead);
                }
                else
                {
                    if (data.Client != null)
                    {
                        data.Client.Close();
                        data.Client.Dispose();
                        data.Client = null;
                    }

                    if (data.Bridge != null)
                    {
                        data.Bridge.Close();
                        data.Bridge.Dispose();
                        data.Bridge = null;
                    }

                    return;
                }
                
                data.Data = new byte[2048];
                data.Bridge.BeginReceive(data.Data, 0, 2048, SocketFlags.None, new AsyncCallback(ForwardRawHTTP), data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Forawrd RAW HTTP failed: {ex.Message}");
                Debug.WriteLine(Host + ":" + Port);
                string msgEvent = $"Forawrd RAW HTTP failed: {ex.Message}{Environment.NewLine}{Host}:{Port}";
                OnErrorOccurred?.Invoke(msgEvent, EventArgs.Empty);
            }
        }

        private void ReadClient(IAsyncResult ar)
        {
            try
            {
                var asyncState = ar.AsyncState;
                if (asyncState == null) return;

                RawSSLObj rso = (RawSSLObj)asyncState;
                if (rso.RawData.Client == null || rso.RawData.Bridge == null) return;
                if (!rso.RawData.Client.Connected || !rso.RawData.Bridge.Connected) return;

                int bytesRead = rso.RawData.Client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    byte[] req = new byte[bytesRead];
                    Array.Copy(rso.RawData.Data, req, bytesRead);
                    rso.RawData.Bridge.Send(req, 0, bytesRead, SocketFlags.None);
                    Array.Clear(req, 0, bytesRead);
                }
                else
                {
                    if (rso.RawData.Client != null)
                    {
                        rso.RawData.Client.Close();
                        rso.RawData.Client.Dispose();
                        rso.RawData.Client = null;
                    }

                    if (rso.RawData.Bridge != null)
                    {
                        rso.RawData.Bridge.Close();
                        rso.RawData.Bridge.Dispose();
                        rso.RawData.Bridge = null;
                    }

                    return;
                }

                rso.RawData.Data = new byte[2048];
                rso.RawData.Client.BeginReceive(rso.RawData.Data, 0, 2048, SocketFlags.None, new AsyncCallback(ReadClient), rso);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to read RAW HTTP from client: {ex.Message}");
                Debug.WriteLine(Host + ":" + Port);
                string msgEvent = $"Failed to read RAW HTTP from client: {ex.Message}{Environment.NewLine}{Host}:{Port}";
                OnErrorOccurred?.Invoke(msgEvent, EventArgs.Empty);
            }
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
                Host = string.Empty;
                TunnelDestroyed = true;
            }

            Disposed = true;
        }
    }
}
