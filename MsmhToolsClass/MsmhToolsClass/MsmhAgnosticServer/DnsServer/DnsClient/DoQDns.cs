using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class DoQDns
{
    private byte[] QueryBuffer { get; set; } = Array.Empty<byte>();
    private DnsReader Reader { get; set; } = new();
    private IPAddress BootstrapIP { get; set; }
    private int BootstrapPort { get; set; }
    private int TimeoutSec { get; set; } = 5;
    private CancellationToken CT { get; set; }
    private string? ProxyScheme { get; set; }
    private string? ProxyUser { get; set; }
    private string? ProxyPass { get; set; }

    public DoQDns(byte[] queryBuffer, DnsReader reader, IPAddress bootstrapIP, int bootstrapPort, int timeoutSec, CancellationToken cT, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null)
    {
        QueryBuffer = queryBuffer;
        Reader = reader;
        BootstrapIP = bootstrapIP;
        BootstrapPort = bootstrapPort;
        TimeoutSec = timeoutSec;
        CT = cT;
        ProxyScheme = proxyScheme;
        ProxyUser = proxyUser;
        ProxyPass = proxyPass;
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