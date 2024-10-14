using MsmhToolsClass.ProxifiedClients;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class DoTClient
{
    private byte[] QueryBuffer { get; set; } = Array.Empty<byte>();
    private DnsReader Reader { get; set; } = new();
    private bool AllowInsecure { get; set; }
    private IPAddress BootstrapIP { get; set; }
    private int BootstrapPort { get; set; }
    private int TimeoutMS { get; set; } = 5;
    private string? ProxyScheme { get; set; }
    private string? ProxyUser { get; set; }
    private string? ProxyPass { get; set; }
    private CancellationToken CT { get; set; }

    public DoTClient(byte[] queryBuffer, DnsReader reader, bool allowInsecure, IPAddress bootstrapIP, int bootstrapPort, int timeoutMS, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null, CancellationToken ct = default)
    {
        QueryBuffer = queryBuffer;
        Reader = reader;
        AllowInsecure = allowInsecure;
        BootstrapIP = bootstrapIP;
        BootstrapPort = bootstrapPort;
        TimeoutMS = timeoutMS;
        ProxyScheme = proxyScheme;
        ProxyUser = proxyUser;
        ProxyPass = proxyPass;
        CT = ct;
    }

    public async Task<byte[]> GetResponseAsync()
    {
        byte[] result = Array.Empty<byte>();

        Task task = Task.Run(async () =>
        {
            TcpClient? tcpClient = null;
            SslStream? sslStream = null;

            try
            {
                string dnsServerIP = await Bootstrap.GetDnsIpAsync(Reader.Host, BootstrapIP, BootstrapPort, 3, false, ProxyScheme, ProxyUser, ProxyPass);
                if (dnsServerIP.Equals(Reader.Host))
                    dnsServerIP = await Bootstrap.GetDnsIpAsync(Reader.Host, BootstrapIP, BootstrapPort, 3, true, ProxyScheme, ProxyUser, ProxyPass);
                
                if (NetworkTool.IsIP(dnsServerIP, out IPAddress? ip) && ip != null)
                {
                    IPEndPoint ep = new(ip, Reader.Port);
                    tcpClient = new(ep.AddressFamily);
                }
                else
                    tcpClient = new();
                tcpClient.SendTimeout = TimeoutMS;
                tcpClient.ReceiveTimeout = TimeoutMS;
                tcpClient.Client.NoDelay = true;

                // Support Upstream Proxy
                ProxifiedTcpClient proxifiedTcpClient = new(ProxyScheme, ProxyUser, ProxyPass);
                var upstream = await proxifiedTcpClient.TryGetConnectedProxifiedTcpClient(dnsServerIP, Reader.Port);
                if (upstream.isSuccess && upstream.proxifiedTcpClient != null) tcpClient = upstream.proxifiedTcpClient;

                if (!upstream.isSuccess)
                    await tcpClient.Client.ConnectAsync(dnsServerIP, Reader.Port, CT).ConfigureAwait(false);

                SslClientAuthenticationOptions optionsClient = new();
                optionsClient.TargetHost = Reader.Host;
                optionsClient.EnabledSslProtocols = MsmhAgnosticServer.SSL_Protocols;
                if (AllowInsecure)
                {
                    optionsClient.CertificateRevocationCheckMode = X509RevocationMode.NoCheck;
                    optionsClient.RemoteCertificateValidationCallback = MsmhAgnosticServer.Callback;
                }

                sslStream = new(tcpClient.GetStream(), false, MsmhAgnosticServer.Callback, null);
                await sslStream.AuthenticateAsClientAsync(optionsClient, CT).ConfigureAwait(false);

                if (sslStream.IsAuthenticated && sslStream.CanWrite)
                {
                    await sslStream.WriteAsync(QueryBuffer, CT).ConfigureAwait(false);

                    if (sslStream.CanRead)
                    {
                        byte[] buffer = new byte[MsmhAgnosticServer.MaxDataSize];
                        int receivedLength = await sslStream.ReadAsync(buffer, CT).ConfigureAwait(false);

                        if (receivedLength > 0) result = buffer[..receivedLength];
                    }
                }
            }
            catch (Exception) { }
            finally
            {
                try
                {
                    tcpClient?.Client.Shutdown(SocketShutdown.Both);
                    tcpClient?.Client.Close();
                    tcpClient?.Dispose();
                    sslStream?.Dispose();
                }
                catch (Exception) { }
            }
        });
        try { await task.WaitAsync(TimeSpan.FromMilliseconds(TimeoutMS), CT).ConfigureAwait(false); } catch (Exception) { }

        return result;
    }
}