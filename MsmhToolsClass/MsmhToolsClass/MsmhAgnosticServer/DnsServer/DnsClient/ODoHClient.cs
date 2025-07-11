using System.Diagnostics;
using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

// https://datatracker.ietf.org/doc/rfc9230/
public class ODoHClient
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

    public ODoHClient(byte[] queryBuffer, DnsReader reader, bool allowInsecure, IPAddress bootstrapIP, int bootstrapPort, int timeoutMS, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null, CancellationToken ct = default)
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

        //return result; // Not Implemented Yet

        Task task = Task.Run(async () =>
        {
            try
            {
                if (!string.IsNullOrEmpty(Reader.ODoHRelayAddress))
                {
                    DnsReader relayReader = new(Reader.ODoHRelayAddress);

                    string relayServerIP = relayReader.Host;
                    if (relayReader.IsDnsCryptStamp) relayServerIP = relayReader.IP.ToString();
                    else
                    {
                        relayServerIP = await Bootstrap.GetDnsIpAsync(relayReader.Host, BootstrapIP, BootstrapPort, 3, ProxyScheme, ProxyUser, ProxyPass);
                    }

                    string scheme = relayReader.Scheme;
                    if (relayReader.Scheme.Equals("h3://")) scheme = "https://";

                    string path = $"{relayReader.Path}?targethost={Reader.Host}&targetpath={Reader.Path}";

                    string oDoHConfigPath = $"/.well-known/odohconfigs"; // ODoH Config can be also in https/svcb records

                    // Get ODoH Config
                    UriBuilder oDoHConfigUriBuilder = new()
                    {
                        Scheme = scheme,
                        Host = relayReader.Host,
                        Port = relayReader.Port,
                        Path = oDoHConfigPath
                    };

                    Uri oDoHConfigUri = oDoHConfigUriBuilder.Uri;

                    HttpRequest oDoHConfigHr = new()
                    {
                        CT = CT,
                        URI = oDoHConfigUri,
                        Method = HttpMethod.Get,
                        TimeoutMS = TimeoutMS,
                        AllowInsecure = AllowInsecure,
                        ProxyScheme = ProxyScheme,
                        ProxyUser = ProxyUser,
                        ProxyPass = ProxyPass,
                    };
                    oDoHConfigHr.Headers.Add("host", relayReader.Host); // In Case Of Using Bootstrap
                    if (NetworkTool.IsIP(relayServerIP, out IPAddress? rsIP) && rsIP != null) oDoHConfigHr.AddressIP = rsIP;

                    HttpRequestResponse oDoHConfigHrr = await HttpRequest.SendAsync(oDoHConfigHr).ConfigureAwait(false);

                    byte[] buffer = oDoHConfigHrr.Data;

                    // Read ODoH Config
                    var config = ObliviousDoHConfigParser.ParseODoHConfigs(buffer);

                    Debug.WriteLine(oDoHConfigHrr.ContentType);
                    foreach (var c in config.Configs)
                    {
                        Debug.WriteLine("-----> Version: " + c.Version);
                        Debug.WriteLine("-----> KemID: " + c.KemID);
                        Debug.WriteLine("-----> KdfID: " + c.KdfID);
                        Debug.WriteLine("-----> AeadID: " + c.AeadID);
                        Debug.WriteLine("-----> " + Convert.ToHexString(c.PublicKeyBytes));
                    }

                    if (config.Configs.Count > 0)
                    {
                        // CreateObliviousDNSQuery
                        ObliviousDNSQuery oDoHQuery = ObliviousDNSQuery.CreateObliviousDNSQuery(QueryBuffer, 0);

                        // Need HPKE Cryptography to encrypt the message which is not available for .NET yet.

                        // Go Src
                        // https://github.com/cloudflare/odoh-go/blob/f39fa019b017510690599e895c20cd02ae138742/odoh.go#L456
                        // https://github.com/cloudflare/odoh-client-go/blob/master/commands/helper.go#L27







                        UriBuilder uriBuilder = new()
                        {
                            Scheme = scheme,
                            Host = relayReader.Host,
                            Port = relayReader.Port,
                            Path = path
                        };

                        Uri uri = uriBuilder.Uri;
                        //Debug.WriteLine("============== " + uri);
                        HttpRequest hr = new()
                        {
                            CT = CT,
                            URI = uri,
                            Method = HttpMethod.Post,
                            ContentType = MsmhAgnosticServer.ODnsMessageContentType,
                            DataToSend = QueryBuffer,
                            TimeoutMS = TimeoutMS,
                            AllowInsecure = AllowInsecure,
                            ProxyScheme = ProxyScheme,
                            ProxyUser = ProxyUser,
                            ProxyPass = ProxyPass,
                        };
                        hr.Headers.Add("host", relayReader.Host); // In Case Of Using Bootstrap
                        if (NetworkTool.IsIP(relayServerIP, out IPAddress? rsIP2) && rsIP2 != null) oDoHConfigHr.AddressIP = rsIP2;

                        if (relayReader.Scheme.Equals("h3://")) hr.IsHttp3 = true;

                        //HttpRequestResponse hrr = await HttpRequest.SendAsync(hr).ConfigureAwait(false);
                        //result = hrr.Data;
                    }
                }
            }
            catch (Exception) { }
        });
        try { await task.WaitAsync(TimeSpan.FromMilliseconds(TimeoutMS), CT).ConfigureAwait(false); } catch (Exception) { }

        return result;
    }
}