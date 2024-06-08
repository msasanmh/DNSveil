using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class DoQClient
{
    private byte[] QueryBuffer { get; set; } = Array.Empty<byte>();
    private DnsReader Reader { get; set; } = new();
    private IPAddress BootstrapIP { get; set; }
    private int BootstrapPort { get; set; }
    private int TimeoutSec { get; set; } = 5;
    private string? ProxyScheme { get; set; }
    private string? ProxyUser { get; set; }
    private string? ProxyPass { get; set; }
    private CancellationToken CT { get; set; }

    public DoQClient(byte[] queryBuffer, DnsReader reader, IPAddress bootstrapIP, int bootstrapPort, int timeoutSec, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null, CancellationToken cT = default)
    {
        QueryBuffer = queryBuffer;
        Reader = reader;
        BootstrapIP = bootstrapIP;
        BootstrapPort = bootstrapPort;
        TimeoutSec = timeoutSec;
        ProxyScheme = proxyScheme;
        ProxyUser = proxyUser;
        ProxyPass = proxyPass;
        CT = cT;
    }

    public async Task<byte[]> GetResponseAsync()
    {
        byte[] result = Array.Empty<byte>();

        Task task = Task.Run(() =>
        {
            try
            {
                // Reserved For .NET 8
                //QuicClientConnectionOptions options = new();
                //options.RemoteEndPoint

                //QuicConnection quic = await QuicConnection.ConnectAsync();

                
            }
            catch (Exception) { }
        });
        try { await task.WaitAsync(TimeSpan.FromSeconds(TimeoutSec), CT).ConfigureAwait(false); } catch (Exception) { }

        return result;
    }
}