using System.Net.Sockets;
using System.Net;
using MsmhToolsClass.ProxifiedClients;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class TcpPlainClient
{
    private byte[] QueryBuffer { get; set; } = Array.Empty<byte>();
    private DnsReader Reader { get; set; } = new();
    private int TimeoutMS { get; set; } = 5;
    private string? ProxyScheme { get; set; }
    private string? ProxyUser { get; set; }
    private string? ProxyPass { get; set; }
    private CancellationToken CT { get; set; }

    public TcpPlainClient(byte[] queryBuffer, DnsReader reader, int timeoutMS, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null, CancellationToken ct = default)
    {
        QueryBuffer = queryBuffer;
        Reader = reader;
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
            try
            {
                IPEndPoint ep = new(IPAddress.Parse(Reader.Host), Reader.Port);

                TcpClient tcpClient = new();
                tcpClient.SendTimeout = TimeoutMS;
                tcpClient.ReceiveTimeout = TimeoutMS;
                tcpClient.Client.NoDelay = true;

                // Support Upstream Proxy
                ProxifiedTcpClient proxifiedTcpClient = new(ProxyScheme, ProxyUser, ProxyPass);
                var upstream = await proxifiedTcpClient.TryGetConnectedProxifiedTcpClient(ep);
                if (upstream.isSuccess && upstream.proxifiedTcpClient != null) tcpClient = upstream.proxifiedTcpClient;

                try
                {
                    if (!upstream.isSuccess)
                        await tcpClient.Client.ConnectAsync(ep, CT).ConfigureAwait(false);

                    await tcpClient.Client.SendAsync(QueryBuffer, SocketFlags.None, CT).ConfigureAwait(false);

                    byte[] buffer = new byte[MsmhAgnosticServer.MaxDataSize];
                    int receivedLength = await tcpClient.Client.ReceiveAsync(buffer, SocketFlags.None, CT).ConfigureAwait(false);

                    ByteArrayTool.TryConvertBytesToUInt16(buffer[0..2], out ushort answerLength);

                    while (receivedLength < answerLength)
                    {
                        receivedLength += await tcpClient.Client.ReceiveAsync(buffer.AsMemory()[receivedLength..], SocketFlags.None, CT).ConfigureAwait(false);
                    }

                    if (receivedLength > 0) result = buffer[..receivedLength];
                }
                catch (Exception) { }

                tcpClient.Client.Shutdown(SocketShutdown.Both);
                tcpClient.Client.Close();
                tcpClient.Dispose();
            }
            catch (Exception) { }
        });
        try { await task.WaitAsync(TimeSpan.FromMilliseconds(TimeoutMS), CT).ConfigureAwait(false); } catch (Exception) { }

        return result;
    }
}