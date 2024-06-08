using System.Net.Sockets;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

namespace MsmhToolsClass.MsmhAgnosticServer;

public enum RequestProtocol
{
    UDP,
    TCP,
    DoH,
    HTTP_S,
    SOCKS4_4A,
    SOCKS5,
    SniProxy,
    Unknown
}

public enum SslKind
{
    NonSSL,
    SSL
}

public class AgnosticResult
{
    public IPEndPoint Local_EndPoint { get; set; } = new(IPAddress.None, 0);
    public IPEndPoint Remote_EndPoint { get; set; } = new(IPAddress.None, 0);
    public Socket? Socket { get; internal set; }
    public SslStream? Ssl_Stream { get; set; }
    public SslKind Ssl_Kind { get; set; } = SslKind.NonSSL;
    public byte[] FirstBuffer { get; internal set; } = Array.Empty<byte>();
    public RequestProtocol Protocol { get; internal set; } = RequestProtocol.Unknown;
    public string SNI { get; internal set; } = string.Empty; // If RequestProtocol Is SniProxy
    public string DoHPath { get; internal set; } = string.Empty; // If RequestProtocol Is DoH
}

public class AgnosticRequest
{
    public enum ListenerProtocol
    {
        UDP,
        TCP
    }

    public Socket? Udp_Socket { get; set; }
    public TcpClient? Tcp_Client { get; set; }
    public SslStream? Ssl_Stream { get; set; }
    public IPEndPoint? Local_EndPoint { get; set; }
    public IPEndPoint? Remote_EndPoint { get; set; }
    public byte[] Peeked_Buffer { get; set; } = Array.Empty<byte>();
    public ListenerProtocol Protocol { get; set; }

    public async Task<AgnosticResult> GetResultAsync(AgnosticSettingsSSL settingsSSL)
    {
        AgnosticResult ar = new();
        if (Local_EndPoint == null) return ar;
        if (Remote_EndPoint == null) return ar;
        if (Peeked_Buffer.Length < 2) return ar;

        try
        {
            ar.Local_EndPoint = Local_EndPoint;
            ar.Remote_EndPoint = Remote_EndPoint;

            if (Protocol == ListenerProtocol.UDP && Udp_Socket != null)
            {
                ar.Socket = Udp_Socket;
                ar.FirstBuffer = Peeked_Buffer;
                ar.Protocol = RequestProtocol.UDP;
            }
            else if (Protocol == ListenerProtocol.TCP && Tcp_Client != null)
            {
                ByteArrayTool.TryConvertBytesToUInt16(Peeked_Buffer[..2], out ushort tcpLength);
                if (tcpLength == Peeked_Buffer.Length - 2)
                {
                    ar.Socket = Tcp_Client.Client;
                    ar.FirstBuffer = await HandleNonSslAsync().ConfigureAwait(false);
                    ar.Protocol = RequestProtocol.TCP;
                }
                else if (Peeked_Buffer[0] == 0x04)
                {
                    ar.Socket = Tcp_Client.Client;
                    ar.FirstBuffer = await HandleNonSslAsync().ConfigureAwait(false);
                    ar.Protocol = RequestProtocol.SOCKS4_4A;
                }
                else if (Peeked_Buffer[0] == 0x05)
                {
                    ar.Socket = Tcp_Client.Client;
                    ar.FirstBuffer = await HandleNonSslAsync().ConfigureAwait(false);
                    ar.Protocol = RequestProtocol.SOCKS5;
                }
                else
                {
                    byte[] firstBuffer = Array.Empty<byte>();
                    if (Peeked_Buffer[0] == 0x16) // SSL
                    {
                        SniReader sniReader = new(Peeked_Buffer);
                        if (sniReader.HasSni)
                        {
                            if (sniReader.SniList.Count != 0)
                            {
                                string sni = sniReader.SniList[0].ServerName;
                                ar.SNI = sni;
                                ar.Socket = Tcp_Client.Client;
                                ar.FirstBuffer = Peeked_Buffer;
                                ar.Protocol = RequestProtocol.SniProxy;
                            }
                        }
                        else
                        {
                            firstBuffer = await HandleSslAsync(settingsSSL).ConfigureAwait(false);
                            ar.Ssl_Kind = SslKind.SSL;
                        }
                    }
                    else
                    {
                        firstBuffer = await HandleNonSslAsync().ConfigureAwait(false);
                    }
                    
                    if (firstBuffer.Length > 0)
                    {
                        HttpRequestResult hrResult = HttpRequest.Read(firstBuffer);
                        if (hrResult.IsSuccess)
                        {
                            if (hrResult.ContentType.Equals(MsmhAgnosticServer.DnsMessageContentType)) // DoH Request
                            {
                                string dohPath = hrResult.RawURL;
                                try
                                {
                                    int start = dohPath.IndexOf('/');
                                    if (start != -1 && dohPath.Length > start) dohPath = dohPath[(start + 1)..];
                                    int middle = dohPath.IndexOf('/');
                                    if (middle != -1 && middle > start) dohPath = dohPath[..middle];
                                    int end = dohPath.IndexOf("?dns=");
                                    if (end != -1 && end > start) dohPath = dohPath[..end];
                                }
                                catch (Exception) { }

                                if (hrResult.Method == HttpMethod.Get && hrResult.RawURL.Contains("dns=", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    // DoH Get Method
                                    string[] split = hrResult.RawURL.Split("dns=", StringSplitOptions.RemoveEmptyEntries);
                                    if (split.Length >= 2)
                                    {
                                        string base64UrlQuery = split[1];
                                        byte[] base64QueryBuffer = EncodingTool.UrlDecode(base64UrlQuery);
                                        if (base64QueryBuffer.Length > 0)
                                        {
                                            ar.Socket = Tcp_Client.Client;
                                            ar.Ssl_Stream = Ssl_Stream;
                                            ar.FirstBuffer = base64QueryBuffer;
                                            ar.Protocol = RequestProtocol.DoH;
                                            ar.DoHPath = dohPath;
                                        }
                                    }
                                }
                                else if (hrResult.Method == HttpMethod.Post && hrResult.PayLoad.Length > 0)
                                {
                                    // DoH Post Method
                                    ar.Socket = Tcp_Client.Client;
                                    ar.Ssl_Stream = Ssl_Stream;
                                    ar.FirstBuffer = hrResult.PayLoad;
                                    ar.Protocol = RequestProtocol.DoH;
                                    ar.DoHPath = dohPath;
                                }
                            }
                            else // Proxy HTTP/S Request
                            {
                                ar.Socket = Tcp_Client.Client;
                                ar.FirstBuffer = firstBuffer;
                                ar.Protocol = RequestProtocol.HTTP_S;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AgnosticRequest GetResultAsync: " + ex.Message);
        }

        return ar;
    }

    private async Task<byte[]> HandleNonSslAsync()
    {
        if (Tcp_Client != null)
        {
            try
            {
                byte[] buffer = new byte[MsmhAgnosticServer.MaxDataSize];
                int received = await Tcp_Client.Client.ReceiveAsync(buffer, SocketFlags.None).ConfigureAwait(false);
                if (received > 0)
                {
                    buffer = buffer[..received];
                    return buffer;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AgnosticRequest HandleNonSslAsync: " + ex.Message);
            }
        }
        return Array.Empty<byte>();
    }

    private async Task<byte[]> HandleSslAsync(AgnosticSettingsSSL settingsSSL)
    {
        if (Tcp_Client != null && settingsSSL.EnableSSL && settingsSSL.Cert != null)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                SslServerAuthenticationOptions optionsServer = new()
                {
                    ServerCertificate = settingsSSL.Cert,
                    ClientCertificateRequired = false,
                    EnabledSslProtocols = MsmhAgnosticServer.SSL_Protocols,
                    CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                    RemoteCertificateValidationCallback = MsmhAgnosticServer.Callback
                };

                Ssl_Stream = new(Tcp_Client.GetStream(), false, MsmhAgnosticServer.Callback, null);
                await Ssl_Stream.AuthenticateAsServerAsync(optionsServer, CancellationToken.None).ConfigureAwait(false);

                byte[] buffer = new byte[MsmhAgnosticServer.MaxDataSize];
                int received = await Ssl_Stream.ReadAsync(buffer, CancellationToken.None).ConfigureAwait(false);
                if (received > 0)
                {
                    buffer = buffer[..received];
                    Debug.WriteLine($"SSL Handshake As Server Success. Took: {stopwatch.ElapsedMilliseconds}");
                    stopwatch.Stop();
                    return buffer;
                }
                stopwatch.Stop();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AgnosticRequest HandleSslAsServerAsync: " +  ex.GetInnerExceptions());
            }
        }
        return Array.Empty<byte>();
    }

    public void Disconnect()
    {
        try
        {
            Tcp_Client?.Client.Shutdown(SocketShutdown.Both);
            Tcp_Client?.Dispose();
            Ssl_Stream?.Dispose();
        }
        catch (Exception) { }
    }

}