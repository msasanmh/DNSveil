using System.Diagnostics;
using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

// https://datatracker.ietf.org/doc/rfc8484
public class DoHClient
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

    public DoHClient(byte[] queryBuffer, DnsReader reader, bool allowInsecure, IPAddress bootstrapIP, int bootstrapPort, int timeoutMS, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null, CancellationToken ct = default)
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
            try
            {
                string dnsServerIP = await Bootstrap.GetDnsIpAsync(Reader.Host, BootstrapIP, BootstrapPort, 3, ProxyScheme, ProxyUser, ProxyPass);

                string scheme = Reader.Scheme;
                if (Reader.Scheme.Equals("h3://")) scheme = "https://";

                UriBuilder uriBuilder = new()
                {
                    Scheme = scheme,
                    Host = Reader.Host,
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
                    ProxyPass = ProxyPass
                };
                hr.Headers.Add("host", Reader.Host); // In Case Of Using Bootstrap
                if (NetworkTool.IsIP(dnsServerIP, out IPAddress? ip) && ip != null) hr.AddressIP = ip;

                if (Reader.Scheme.Equals("h3://")) hr.IsHttp3 = true;

                HttpRequestResponse hrr = await HttpRequest.SendAsync(hr).ConfigureAwait(false);
                result = hrr.Data;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DoHClient: " + ex.GetInnerExceptions());
            }
        });
        try { await task.WaitAsync(TimeSpan.FromMilliseconds(TimeoutMS), CT).ConfigureAwait(false); } catch (Exception) { }
        
        return result;
    }
}