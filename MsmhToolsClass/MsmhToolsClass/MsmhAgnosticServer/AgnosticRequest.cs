using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

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
    public string Message { get; internal set; } = string.Empty; // Any Message
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

    public async Task<AgnosticResult> GetResultAsync(AgnosticSettings settings, AgnosticSettingsSSL settingsSSL)
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
                // UDP Plain DNS
                ar.Socket = Udp_Socket;
                ar.FirstBuffer = Peeked_Buffer;
                ar.Protocol = RequestProtocol.UDP;
            }
            else if (Protocol == ListenerProtocol.TCP && Tcp_Client != null)
            {
                ByteArrayTool.TryConvertBytesToUInt16(Peeked_Buffer[..2], out ushort tcpLength);
                if (tcpLength == Peeked_Buffer.Length - 2 && Peeked_Buffer[0] != 0x16)
                {
                    // TCP Plain DNS
                    ar.Socket = Tcp_Client.Client;
                    ar.FirstBuffer = await HandleNonSslAsync().ConfigureAwait(false);
                    ar.Protocol = RequestProtocol.TCP;
                }
                else
                {
                    if (Peeked_Buffer[0] == 0x04)
                    {
                        // SOCKS4_4A Proxy
                        if (settings.Working_Mode == AgnosticSettings.WorkingMode.Proxy ||
                            settings.Working_Mode == AgnosticSettings.WorkingMode.DnsAndProxy)
                        {
                            ar.Socket = Tcp_Client.Client;
                            ar.FirstBuffer = await HandleNonSslAsync().ConfigureAwait(false);
                            ar.Protocol = RequestProtocol.SOCKS4_4A;
                        }
                    }
                    else if (Peeked_Buffer[0] == 0x05)
                    {
                        // SOCKS5 Proxy
                        if (settings.Working_Mode == AgnosticSettings.WorkingMode.Proxy ||
                            settings.Working_Mode == AgnosticSettings.WorkingMode.DnsAndProxy)
                        {
                            ar.Socket = Tcp_Client.Client;
                            ar.FirstBuffer = await HandleNonSslAsync().ConfigureAwait(false);
                            ar.Protocol = RequestProtocol.SOCKS5;
                        }
                    }
                    else
                    {
                        // DnsAndProxy (Except UDP/TCP - SOCKS 4/4A/5) - Can Be DoH/HTTPS/SNI-Proxy Request
                        if (Peeked_Buffer[0] == 0x16)
                        {
                            // Check If Request Is An SNI-Proxy
                            bool is_SNI_Proxy = false, is_SNI_Proxy_Valid = false;
                            SniReader? sniReader = null;
                            string sni = string.Empty;
                            if (settings.Working_Mode == AgnosticSettings.WorkingMode.Proxy ||
                                settings.Working_Mode == AgnosticSettings.WorkingMode.DnsAndProxy)
                            {
                                sniReader = new(Peeked_Buffer);
                                if (sniReader.HasSni)
                                {
                                    // Has SNI
                                    if (sniReader.SniList.Count > 0) sni = sniReader.SniList[0].ServerName;

                                    bool isSniEqualToServerAddress = sni.Equals(settingsSSL.ServerDomainName, StringComparison.OrdinalIgnoreCase) ||
                                                                     sni.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                                                                     NetworkTool.IsLocalIP(sni);

                                    bool is_DoH_Or_HTTPS = settingsSSL.EnableSSL && isSniEqualToServerAddress;
                                    if (is_DoH_Or_HTTPS) ar.Message += $"{sniReader.ReasonPhrase}{Environment.NewLine}";
                                    else is_SNI_Proxy = true;
                                }
                                else
                                {
                                    // No SNI
                                    bool is_DoH_Or_HTTPS = settingsSSL.EnableSSL && sniReader.IsHandshakeWithoutSNI;
                                    if (is_DoH_Or_HTTPS) ar.Message += $"{sniReader.ReasonPhrase}{Environment.NewLine}";
                                    else is_SNI_Proxy = true;
                                }
                            }
                            is_SNI_Proxy_Valid = !string.IsNullOrEmpty(sni);

                            if (is_SNI_Proxy)
                            {
                                if (is_SNI_Proxy_Valid)
                                {
                                    // SNI Proxy
                                    ar.SNI = sni;
                                    ar.Socket = Tcp_Client.Client;
                                    ar.FirstBuffer = Peeked_Buffer;
                                    ar.Protocol = RequestProtocol.SniProxy;
                                }
                                else
                                {
                                    // Invalid SNI Proxy
                                    ar.SNI = string.Empty; // Drop Connection
                                    ar.Socket = Tcp_Client.Client;
                                    ar.FirstBuffer = Peeked_Buffer;
                                    ar.Protocol = RequestProtocol.SniProxy;
                                }
                            }
                            else
                            {
                                // DoH/HTTPS Proxy
                                ar.Message += $"Request: {ar.Protocol} {ar.Ssl_Kind}{Environment.NewLine}";
                                byte[] firstBuffer = await HandleSslAsync(settingsSSL, ar, sniReader).ConfigureAwait(false);
                                HttpRequestResult hrResult = new();

                                // Check If Request Is A DoH
                                bool is_DoH = false, is_DoH_GET = false, is_DoH_POST = false;
                                if (settings.Working_Mode == AgnosticSettings.WorkingMode.Dns ||
                                    settings.Working_Mode == AgnosticSettings.WorkingMode.DnsAndProxy)
                                {
                                    hrResult = HttpRequest.Read(firstBuffer);
                                    if (hrResult.IsSuccess)
                                    {
                                        if (hrResult.ContentType.Equals(MsmhAgnosticServer.DnsMessageContentType))
                                        {
                                            is_DoH = true;
                                            if (hrResult.Method == HttpMethod.Get && hrResult.RawURL.Contains("dns=", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                is_DoH_GET = true;
                                            }
                                            else if (hrResult.Method == HttpMethod.Post && hrResult.PayLoad.Length > 0)
                                            {
                                                is_DoH_POST = true;
                                            }
                                        }
                                    }
                                }

                                if (is_DoH)
                                {
                                    // DoH Request
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

                                    if (is_DoH_GET)
                                    {
                                        // DoH Get Method
                                        string[] split = hrResult.RawURL.Split("dns=", StringSplitOptions.RemoveEmptyEntries);
                                        if (split.Length >= 2)
                                        {
                                            string base64UrlQuery = split[1];
                                            byte[] base64QueryBuffer = EncodingTool.Base64UrlDecode(base64UrlQuery);
                                            if (base64QueryBuffer.Length > 0)
                                            {
                                                ar.Ssl_Kind = SslKind.SSL;
                                                ar.Socket = Tcp_Client.Client;
                                                ar.Ssl_Stream = Ssl_Stream;
                                                ar.FirstBuffer = base64QueryBuffer;
                                                ar.Protocol = RequestProtocol.DoH;
                                                ar.DoHPath = dohPath;
                                            }
                                        }
                                    }

                                    if (is_DoH_POST)
                                    {
                                        // DoH Post Method
                                        ar.Ssl_Kind = SslKind.SSL;
                                        ar.Socket = Tcp_Client.Client;
                                        ar.Ssl_Stream = Ssl_Stream;
                                        ar.FirstBuffer = hrResult.PayLoad;
                                        ar.Protocol = RequestProtocol.DoH;
                                        ar.DoHPath = dohPath;
                                    }
                                }
                                else
                                {
                                    // HTTPS Proxy
                                    if (settings.Working_Mode == AgnosticSettings.WorkingMode.Proxy ||
                                        settings.Working_Mode == AgnosticSettings.WorkingMode.DnsAndProxy)
                                    {
                                        ar.Ssl_Kind = SslKind.SSL;
                                        ar.FirstBuffer = firstBuffer;
                                        ar.Socket = Tcp_Client.Client;
                                        ar.Ssl_Stream = Ssl_Stream;
                                        ar.Protocol = RequestProtocol.HTTP_S;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // HTTP_S Proxy
                            if (settings.Working_Mode == AgnosticSettings.WorkingMode.Proxy ||
                                settings.Working_Mode == AgnosticSettings.WorkingMode.DnsAndProxy)
                            {
                                ar.Socket = Tcp_Client.Client;
                                ar.FirstBuffer = await HandleNonSslAsync().ConfigureAwait(false);
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
        //Debug.WriteLine($"==============: {ar.Protocol} {ar.Ssl_Kind}");
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

    private async Task<byte[]> HandleSslAsync(AgnosticSettingsSSL settingsSSL, AgnosticResult aResult, SniReader? sniReader)
    {
        if (Tcp_Client != null && settingsSSL.EnableSSL && settingsSSL.Cert != null)
        {
            sniReader ??= new(Peeked_Buffer);
            
            try
            {
                if (!sniReader.IsAClientHello)
                {
                    aResult.Message = string.Empty;
                    return Array.Empty<byte>();
                }

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
                stopwatch.Stop();
                if (received > 0)
                {
                    buffer = buffer[..received];
                    aResult.Message = string.Empty;
                    //string msg = $"AgnosticRequest HandleSslAsync: SSL Handshake As Server Success. Took: {stopwatch.ElapsedMilliseconds}ms.";
                    //Debug.WriteLine(msg);
                    return buffer;
                }
                else
                {
                    string msg = $"AgnosticRequest HandleSslAsync: Received 0 Bytes. Took: {stopwatch.ElapsedMilliseconds}{Environment.NewLine}ms.";
                    aResult.Message += msg;
                    Debug.WriteLine(msg);
                }
            }
            catch (Exception ex)
            {
                string msg = string.Empty, sni = string.Empty;
                try
                {
                    if (sniReader.SniList.Count > 0) sni = sniReader.SniList[0].ServerName;
                    msg += $"SNI: {sni}, IsAClientHello: {sniReader.IsAClientHello}, IsHandshakeWithoutSNI: {sniReader.IsHandshakeWithoutSNI}{Environment.NewLine}";
                    if (!string.IsNullOrEmpty(sni) && sniReader.IsAClientHello && !sniReader.IsHandshakeWithoutSNI)
                        msg += $"Can Be An SNI-Proxy Request - Wrong Request To A Wrong Server!{Environment.NewLine}";
                    msg += $"AgnosticRequest HandleSslAsync:{Environment.NewLine}{ex.GetInnerExceptions()}{Environment.NewLine}";
                    aResult.Message += msg;
                }
                catch (Exception) { }
                Debug.WriteLine(msg);
            }
        }
        return Array.Empty<byte>();
    }

    public void Disconnect()
    {
        try
        {
            Tcp_Client?.Close();
            Tcp_Client?.Dispose();
            Ssl_Stream?.Close();
            Ssl_Stream?.Dispose();
        }
        catch (Exception) { }
    }

}