using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class DoHDns
{
    private byte[] QueryBuffer { get; set; } = Array.Empty<byte>();
    private DnsReader Reader { get; set; } = new();
    private bool AllowInsecure { get; set; }
    private IPAddress BootstrapIP { get; set; }
    private int BootstrapPort { get; set; }
    private int TimeoutMS { get; set; } = 5;
    private CancellationToken CT { get; set; }
    private string? ProxyScheme { get; set; }
    private string? ProxyUser { get; set; }
    private string? ProxyPass { get; set; }

    public DoHDns(byte[] queryBuffer, DnsReader reader, bool allowInsecure, IPAddress bootstrapIP, int bootstrapPort, int timeoutMS, CancellationToken ct, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null)
    {
        QueryBuffer = queryBuffer;
        Reader = reader;
        AllowInsecure = allowInsecure;
        BootstrapIP = bootstrapIP;
        BootstrapPort = bootstrapPort;
        TimeoutMS = timeoutMS;
        CT = ct;
        ProxyScheme = proxyScheme;
        ProxyUser = proxyUser;
        ProxyPass = proxyPass;
    }

    public async Task<byte[]> GetResponseAsync()
    {
        byte[] result = Array.Empty<byte>();

        Task task = Task.Run(async () =>
        {
            try
            {
                string dnsServerIP = await Bootstrap.GetDnsIpAsync(Reader.Host, BootstrapIP, BootstrapPort, 3, false, ProxyScheme, ProxyUser, ProxyPass);
                if (dnsServerIP.Equals(Reader.Host))
                    dnsServerIP = await Bootstrap.GetDnsIpAsync(Reader.Host, BootstrapIP, BootstrapPort, 3, true, ProxyScheme, ProxyUser, ProxyPass);

                UriBuilder uriBuilder = new()
                {
                    Scheme = Reader.Scheme,
                    Host = dnsServerIP,
                    Port = Reader.Port,
                    Path = Reader.Path
                };

                Uri uri = uriBuilder.Uri;
                
                HttpRequest hr = new()
                {
                    CT = CT,
                    URI = uri,
                    Method = HttpMethod.Post,
                    ContentType = MsmhAgnosticServer.DnsMessageContentType,
                    DataToSend = QueryBuffer,
                    TimeoutMS = TimeoutMS,
                    AllowInsecure = AllowInsecure,
                    ProxyScheme = ProxyScheme,
                    ProxyUser = ProxyUser,
                    ProxyPass = ProxyPass,
                };
                hr.Headers.Add("host", Reader.Host); // In Case Of Using Bootstrap
                
                HttpRequestResponse hrr = await HttpRequest.SendAsync(hr).ConfigureAwait(false);
                result = hrr.Data;
            }
            catch (Exception) { }
        });
        try { await task.WaitAsync(TimeSpan.FromMilliseconds(TimeoutMS), CT).ConfigureAwait(false); } catch (Exception) { }
        
        return result;
    }
}