using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MsmhToolsClass;

public class HttpRequestResult
{
    public bool IsSuccess { get; internal set; } = false;
    public HttpMethod Method { get; internal set; } = HttpMethod.Get;
    public string RawURL { get; internal set; } = string.Empty;
    public Uri? URI { get; internal set; }
    public string ProtocolVersion { get; internal set; } = string.Empty;
    public string ContentType { get; internal set; } = string.Empty;
    public string UserAgent { get; internal set; } = string.Empty;
    public NameValueCollection Headers { get; internal set; } = new(StringComparer.InvariantCultureIgnoreCase);
    public byte[] PayLoad { get; internal set; } = Array.Empty<byte>();

    public override string ToString()
    {
        string result = string.Empty;

        try
        {
            result += $"{nameof(IsSuccess)}: {IsSuccess}\r\n";
            result += $"{nameof(Method)}: {Method}\r\n";
            result += $"{nameof(RawURL)}: {RawURL}\r\n";
            if (URI != null) result += $"{nameof(URI)}: {URI}\r\n";
            result += $"{nameof(ProtocolVersion)}: {ProtocolVersion}\r\n";
            result += $"{nameof(ContentType)}: {ContentType}\r\n";
            result += $"{nameof(UserAgent)}: {UserAgent}\r\n";
            if (Headers.Count > 0) result += $"{nameof(Headers)}:\r\n";
            for (int n = 0; n < Headers.Count; n++)
            {
                string? key = Headers.GetKey(n);
                string? val = Headers.Get(n);
                if (string.IsNullOrEmpty(key)) continue;
                if (string.IsNullOrEmpty(val)) continue;
                result += $"{key}: {val}\r\n";
            }
            if (PayLoad.Length > 0) result += $"{nameof(PayLoad)}: {BitConverter.ToString(PayLoad)}";
            if (result.EndsWith("\r\n")) result = result.TrimEnd("\r\n");
        }
        catch (Exception) { }

        return result;
    }
}


public class HttpRequestResponse
{
    public bool IsSuccess { get; internal set; } = false;
    public string ContentEncoding { get; internal set; } = string.Empty;
    public string ContentType { get; internal set; } = string.Empty;
    public long ContentLength { get; internal set; } = 0;
    public string ProtocolVersion { get; internal set; } = string.Empty;
    public HttpStatusCode StatusCode { get; internal set; } = HttpStatusCode.RequestTimeout;
    public int StatusCodeNumber { get; internal set; } = (int)HttpStatusCode.RequestTimeout;
    public string StatusDescription { get; internal set; } = HttpStatusCode.RequestTimeout.ToString();
    public NameValueCollection Headers { get; internal set; } = new(StringComparer.InvariantCultureIgnoreCase);
    public byte[] Data { get; internal set; } = Array.Empty<byte>();

    public override string ToString()
    {
        string result = string.Empty;

        try
        {
            result += $"{nameof(IsSuccess)}: {IsSuccess}\r\n";
            result += $"{nameof(ContentEncoding)}: {ContentEncoding}\r\n";
            result += $"{nameof(ContentType)}: {ContentType}\r\n";
            result += $"{nameof(ContentLength)}: {ContentLength}\r\n";
            result += $"{nameof(ProtocolVersion)}: {ProtocolVersion}\r\n";
            result += $"{nameof(StatusCode)}: {StatusCode}\r\n";
            result += $"{nameof(StatusCodeNumber)}: {StatusCodeNumber}\r\n";
            result += $"{nameof(StatusDescription)}: {StatusDescription}\r\n";
            if (Headers.Count > 0)
                result += $"{nameof(Headers)}:\r\n";
            for (int n = 0; n < Headers.Count; n++)
            {
                string? key = Headers.GetKey(n);
                string? val = Headers.Get(n);
                if (string.IsNullOrEmpty(key)) continue;
                if (string.IsNullOrEmpty(val)) continue;
                result += $"{key}: {val}\r\n";
            }
            result += $"{nameof(Data)}: {Convert.ToHexString(Data)}";
        }
        catch (Exception) { }

        return result;
    }
}

/// <summary>
/// Authorization Header Options
/// </summary>
public class HttpRequestAuthorizationHeader
{
    /// <summary>
    /// The username to use in the authorization header, if any
    /// </summary>
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// The password to use in the authorization header, if any
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// The bearer token to use in the authorization header, if any
    /// </summary>
    public string BearerToken { get; set; } = string.Empty;

    /// <summary>
    /// Enable to encode credentials in the authorization header
    /// </summary>
    public bool EncodeCredentials { get; set; } = true;

    public override string ToString()
    {
        string result = string.Empty;

        try
        {
            result += $"{nameof(User)}: {User}\r\n";
            result += $"{nameof(Password)}: {Password}\r\n";
            result += $"{nameof(BearerToken)}: {BearerToken}\r\n";
            result += $"{nameof(EncodeCredentials)}: {EncodeCredentials}";
        }
        catch (Exception) { }

        return result;
    }
}

public class HttpRequest
{
    public CancellationToken CT { get; set; } = default;
    public bool IsForHttpProxy { get; set; } = false;
    public Uri? URI { get; set; }
    /// <summary>
    /// Connect To This IP (Host's IP Retrieved From A DNS Server) - Will Be Ignored If ProxyScheme Is Set.
    /// </summary>
    public IPAddress AddressIP { get; set; } = IPAddress.None;
    public byte[] DataToSend { get; set; } = Array.Empty<byte>();
    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public bool IsHttp3 { get; set; } = false;
    public string ContentType { get; set; } = string.Empty;
    /// <summary>
    /// Default is "Other"
    /// </summary>
    public string UserAgent { get; set; } = "Other";
    /// <summary>
    /// User-Agent Will Be Add Automatically (Don't Add To Header)
    /// </summary>
    public NameValueCollection Headers { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);
    public bool AllowAutoRedirect { get; set; } = true;
    public bool AllowInsecure { get; set; } = true;
    public int TimeoutMS { get; set; } = 30000;
    public string? ProxyScheme { get; set; }
    public string? ProxyUser { get; set; }
    public string? ProxyPass { get; set; }
    public HttpRequestAuthorizationHeader Authorization { get; set; } = new();
    public X509Certificate2? Certificate { get; set; }

    public override string ToString()
    {
        string result = string.Empty;

        try
        {
            result += $"{nameof(IsForHttpProxy)}: {IsForHttpProxy}\r\n";
            if (URI != null) result += $"{nameof(URI)}: {URI}\r\n";
            if (AddressIP != IPAddress.None) result += $"{nameof(AddressIP)}: {AddressIP}\r\n";
            if (DataToSend.Length > 0) result += $"{nameof(DataToSend)}: {Convert.ToHexString(DataToSend)}\r\n";
            result += $"{nameof(Method)}: {Method}\r\n";
            result += $"{nameof(ContentType)}: {ContentType}\r\n";
            result += $"{nameof(UserAgent)}: {UserAgent}\r\n";
            if (Headers.Count > 0) result += $"{nameof(Headers)}:\r\n";
            for (int n = 0; n < Headers.Count; n++)
            {
                string? key = Headers.GetKey(n);
                string? val = Headers.Get(n);
                if (string.IsNullOrEmpty(key)) continue;
                if (string.IsNullOrEmpty(val)) continue;
                result += $"{key}: {val}\r\n";
            }
            result += $"{nameof(AllowAutoRedirect)}: {AllowAutoRedirect}\r\n";
            result += $"{nameof(AllowInsecure)}: {AllowInsecure}\r\n";
            result += $"{nameof(TimeoutMS)}: {TimeoutMS}\r\n";
            if (!string.IsNullOrEmpty(ProxyScheme))
                result += $"{nameof(ProxyScheme)}: {ProxyScheme}\r\n";
            if (!string.IsNullOrEmpty(ProxyUser))
                result += $"{nameof(ProxyUser)}: {ProxyUser}\r\n";
            if (!string.IsNullOrEmpty(ProxyPass))
                result += $"{nameof(ProxyPass)}: {ProxyPass}\r\n";
            result += $"{nameof(Authorization)}:\r\n";
            result += Authorization.ToString() + "\r\n";
            if (Certificate != null)
                result += $"{nameof(Certificate)}: {Certificate.SubjectName}\r\n";
            if (result.EndsWith("\r\n")) result = result.TrimEnd("\r\n");
        }
        catch (Exception) { }

        return result;
    }

    public static HttpRequestResult Read(byte[] buffer)
    {
        HttpRequestResult hrr = new();

        try
        {
            string str = Encoding.UTF8.GetString(buffer);
            str.ReplaceLineEndings();
            
            string[] headers = str.Split(Environment.NewLine); // new string[] { "\r\n", "\n" }
            
            string scheme = string.Empty, host = string.Empty, path = string.Empty;
            int port = -1;
            string contentType = string.Empty;
            int contentLength = 0;

            for (int n = 0; n < headers.Length; n++)
            {
                if (n == 0)
                {
                    // First Line (Method, raw URL, ListenerProtocol/Version)
                    string[] requestLine = headers[n].Trim().Trim('\0').Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    
                    if (requestLine.Length >= 3)
                    {
                        // 3 Parts Means Success
                        hrr.IsSuccess = true;

                        // Get Method
                        hrr.Method = NetworkTool.ParseHttpMethod(requestLine[0]);
                        
                        // Get Raw URL
                        string rawUrl = requestLine[1];
                        hrr.RawURL = rawUrl;
                        
                        // Get URI (For HTTP & HTTPS Proxies)
                        // For HTTP: Scheme: Yes, Port: No
                        // For HTTPS: Scheme: No, Port: Yes
                        NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(rawUrl, -1);
                        scheme = urid.Scheme;
                        host = urid.Host;
                        port = urid.Port;
                        path = urid.Path;
                        if (scheme.ToLower().Equals("http://") && port == -1) port = 80;
                        if (string.IsNullOrEmpty(scheme) && port != -1) scheme = "https://";
                        if (string.IsNullOrEmpty(scheme) && port == -1) path = rawUrl; // e.g. DoH Request

                        // If Host Is IPv6
                        if (host.StartsWith('[')) host = host.TrimStart('[');
                        if (host.EndsWith(']')) host = host.TrimEnd(']');

                        // Get ProtocolVersion
                        hrr.ProtocolVersion = requestLine[2];
                    }
                }
                else
                {
                    if (headers[n].Contains(':'))
                    {
                        string[] headerLine = headers[n].Split(':'); // Shouldn't RemoveEmptyEntries [::] IPv6
                        string key = string.Empty;
                        string val = string.Empty;
                        for (int i = 0; i < headerLine.Length; i++)
                        {
                            string kvs = headerLine[i];
                            if (i == 0) key = kvs;
                            else val += $"{kvs}:";
                        }
                        if (val.EndsWith(':')) val = val.TrimEnd(':');

                        key = key.Trim();
                        val = val.Trim();
                        
                        if (string.IsNullOrEmpty(key)) continue;
                        if (string.IsNullOrEmpty(val)) continue;
                        string keyEval = key.ToLower();

                        // Get Host
                        if (keyEval.Equals("host") && string.IsNullOrEmpty(host))
                        {
                            host = val;
                            if (host.Contains(':'))
                            {
                                NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(host, -1);
                                string host2 = urid.Host;
                                int port2 = urid.Port;

                                // If Host Is IPv6
                                if (host2.StartsWith('[')) host2 = host2.TrimStart('[');
                                if (host2.EndsWith(']')) host2 = host2.TrimEnd(']');

                                host = host2;
                                if (port == -1) port = port2;
                            }
                        }

                        // Get ContentType
                        if (keyEval.Equals("accept"))
                        {
                            if (string.IsNullOrEmpty(contentType)) contentType = val;
                        }

                        if (keyEval.Equals("content-type"))
                        {
                            contentType = val;
                        }

                        // Get ContentLength
                        if (keyEval.Equals("content-length"))
                        {
                            bool isIntSuccess = int.TryParse(val, out int cl);
                            if (isIntSuccess) contentLength = cl;
                        }

                        // Get UserAgent
                        if (keyEval.Equals("user-agent"))
                            hrr.UserAgent = val;
                        else hrr.Headers.AddAndUpdate(key, val); // Get Other Headers
                    }
                }
            }

            // Set ContentType
            hrr.ContentType = contentType;

            // Get PayLoad
            if (contentLength > 0 && buffer.Length > contentLength && hrr.Method != HttpMethod.Get && hrr.Method != HttpMethod.Head)
            {
                int startIndex = buffer.Length - contentLength;
                hrr.PayLoad = buffer[startIndex..];
            }

            // Generate URI
            if (string.IsNullOrEmpty(scheme)) scheme = "https://";
            if (!string.IsNullOrEmpty(host) && port != -1)
            {
                UriBuilder uriBuilder = new()
                {
                    Scheme = scheme.ToLower(),
                    Host = host.ToLower(),
                    Port = port,
                    Path = path
                };
                hrr.URI = uriBuilder.Uri;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("HttpRequest Read: " + ex.Message);
        }

        return hrr;
    }

    public static async Task<HttpRequestResponse> SendAsync(HttpRequest hr)
    {
        HttpRequestResponse hrr = new();
        
        Task task = Task.Run(async () =>
        {
            try
            {
                static bool callback(object sender, X509Certificate? cert, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true;
                SslProtocols protocols = SslProtocols.None | SslProtocols.Tls12 | SslProtocols.Tls13;

                HttpClient? httpClient;

                if (hr.AddressIP == IPAddress.None || !string.IsNullOrEmpty(hr.ProxyScheme))
                {
                    HttpClientHandler handler = new()
                    {
                        SslProtocols = protocols,
                        AllowAutoRedirect = hr.AllowAutoRedirect
                    };

                    // Ignore Cert Check
                    if (hr.AllowInsecure)
                    {
                        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                        handler.ServerCertificateCustomValidationCallback = callback;
                    }
                    
                    if (!string.IsNullOrEmpty(hr.ProxyScheme))
                    {
                        NetworkCredential credential = new(hr.ProxyUser, hr.ProxyPass);
                        handler.Proxy = new WebProxy(hr.ProxyScheme, true, null, credential);
                        handler.Credentials = credential;
                        handler.UseProxy = true;
                    }
                    else handler.UseProxy = false;

                    if (hr.Certificate != null)
                    {
                        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                        handler.ClientCertificates.Add(hr.Certificate);
                    }

                    httpClient = new(handler);
                }
                else
                {
                    SocketsHttpHandler handler = new()
                    {
                        ConnectCallback = async (context, ct) =>
                        {
                            // This Doesn't Send SNI To Upstream Proxy
                            Socket socket = new(SocketType.Stream, ProtocolType.Tcp)
                            {
                                NoDelay = true // Turn Off Nagle's Algorithm
                            };

                            await socket.ConnectAsync(hr.AddressIP, context.DnsEndPoint.Port, ct);
                            return new NetworkStream(socket, ownsSocket: true);
                        }
                    };
                    
                    handler.SslOptions.EnabledSslProtocols = protocols;
                    handler.AllowAutoRedirect = hr.AllowAutoRedirect;

                    // Ignore Cert Check
                    if (hr.AllowInsecure)
                    {
                        handler.SslOptions.CertificateRevocationCheckMode = X509RevocationMode.NoCheck;
                        handler.SslOptions.RemoteCertificateValidationCallback = callback;
                    }
                    
                    if (!string.IsNullOrEmpty(hr.ProxyScheme))
                    {
                        NetworkCredential credential = new(hr.ProxyUser, hr.ProxyPass);
                        handler.Proxy = new WebProxy(hr.ProxyScheme, true, null, credential);
                        handler.Credentials = credential;
                        handler.UseProxy = true;
                    }
                    else handler.UseProxy = false;

                    if (hr.Certificate != null)
                    {
                        handler.SslOptions.CertificateRevocationCheckMode = X509RevocationMode.NoCheck;
                        handler.SslOptions.ClientCertificates?.Add(hr.Certificate);
                    }

                    httpClient = new(handler, disposeHandler: true);
                }
                
                HttpContent? content = null;
                if (hr.DataToSend.Length > 0 && hr.Method != HttpMethod.Get && hr.Method != HttpMethod.Head)
                {
                    content = new ReadOnlyMemoryContent(hr.DataToSend);
                    if (!string.IsNullOrEmpty(hr.ContentType))
                        content.Headers.ContentType = new MediaTypeHeaderValue(hr.ContentType);
                }

                httpClient.Timeout = TimeSpan.FromMilliseconds(hr.TimeoutMS);
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                httpClient.DefaultRequestHeaders.ConnectionClose = true;

                HttpRequestMessage message = new(hr.Method, hr.URI)
                {
                    Content = content
                };
                if (!string.IsNullOrEmpty(hr.ContentType))
                    message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(hr.ContentType));
                message.Headers.TryAddWithoutValidation("User-Agent", hr.UserAgent);

                for (int n = 0; n < hr.Headers.Count; n++)
                {
                    string? key = hr.Headers.GetKey(n);
                    string? value = hr.Headers.Get(n);
                    if (string.IsNullOrEmpty(key)) continue;
                    if (string.IsNullOrEmpty(value)) continue;

                    if (key.ToLower().Trim().Equals("close"))
                    {
                        if (!hr.IsForHttpProxy) message.Headers.TryAddWithoutValidation(key, value);
                    }
                    else if (key.ToLower().Trim().Equals("connection"))
                    {
                        if (!hr.IsForHttpProxy) message.Headers.TryAddWithoutValidation(key, value);
                    }
                    else if (key.ToLower().Trim().Equals("content-length"))
                    {
                        if (!hr.IsForHttpProxy) message.Headers.TryAddWithoutValidation(key, value);
                    }
                    else if (key.ToLower().Trim().Equals("accept"))
                    {
                        if (!hr.IsForHttpProxy) message.Headers.TryAddWithoutValidation(key, value);
                    }
                    else if (key.ToLower().Trim().Equals("accept-encoding"))
                    {
                        if (!hr.IsForHttpProxy) message.Headers.TryAddWithoutValidation(key, value);
                    }
                    else if (key.ToLower().Trim().Equals("content-type"))
                    {
                        if (message.Content != null && hr.Method != HttpMethod.Get && hr.Method != HttpMethod.Head)
                            message.Content.Headers.ContentType = new MediaTypeHeaderValue(value);
                    }
                    else
                    {
                        message.Headers.TryAddWithoutValidation(key, value);
                        //Debug.WriteLine("==========> " + key + ": " + value);
                    }
                }

                // Add Auth Info
                if (!string.IsNullOrEmpty(hr.Authorization.User))
                {
                    if (hr.Authorization.EncodeCredentials)
                    {
                        string authInfo = $"{hr.Authorization.User}:{hr.Authorization.Password}";
                        authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                        httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + authInfo);
                    }
                    else
                    {
                        httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {hr.Authorization.User}:{hr.Authorization.Password}");
                    }
                }
                else if (!string.IsNullOrEmpty(hr.Authorization.BearerToken))
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {hr.Authorization.BearerToken}");
                }

                if (hr.IsHttp3) // Only Windows 11 Above
                {
                    httpClient.DefaultRequestVersion = HttpVersion.Version30;
                    httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                }
                
                HttpResponseMessage response = await httpClient.SendAsync(message, hr.CT).ConfigureAwait(false);

                hrr.IsSuccess = response.IsSuccessStatusCode;
                hrr.ProtocolVersion = $"HTTP/{response.Version}";
                hrr.StatusCode = response.StatusCode;
                hrr.StatusCodeNumber = (int)response.StatusCode;
                hrr.StatusDescription = response.StatusCode.ToString();
                
                hrr.ContentEncoding = string.Join(",", response.Content.Headers.ContentEncoding);
                if (response.Content.Headers.ContentType != null)
                    hrr.ContentType = response.Content.Headers.ContentType.ToString();

                if (response.Content.Headers.ContentLength != null)
                    hrr.ContentLength = response.Content.Headers.ContentLength.Value;

                foreach (var header in response.Headers)
                {
                    string key = header.Key;
                    string value = string.Join(",", header.Value);
                    hrr.Headers.AddAndUpdate(key, value);
                }

                if (hrr.IsSuccess)
                {
                    hrr.Data = await response.Content.ReadAsByteArrayAsync(hr.CT).ConfigureAwait(false);
                }

                try
                {
                    _ = Task.Run(() =>
                    {
                        content?.Dispose();
                        message.Dispose();
                        httpClient.Dispose();
                        response.Dispose();
                    });
                }
                catch (Exception) { }
            }
            catch (WebException we)
            {
                if (we.Response is HttpWebResponse exceptionResponse)
                {
                    hrr.IsSuccess = false;
                    hrr.ProtocolVersion = $"HTTP/{exceptionResponse.ProtocolVersion}";
                    hrr.StatusCode = exceptionResponse.StatusCode;
                    hrr.StatusCodeNumber = (int)exceptionResponse.StatusCode;
                    hrr.StatusDescription = exceptionResponse.StatusDescription;
                    hrr.ContentEncoding = exceptionResponse.ContentEncoding;
                    hrr.ContentType = exceptionResponse.ContentType;
                    hrr.ContentLength = exceptionResponse.ContentLength;
                    
                    if (exceptionResponse.Headers != null && exceptionResponse.Headers.Count > 0)
                    {
                        for (int n = 0; n < exceptionResponse.Headers.Count; n++)
                        {
                            string key = exceptionResponse.Headers.GetKey(n);
                            string val = string.Empty;
                            int valCount = 0;

                            string[]? getValues = exceptionResponse.Headers.GetValues(n);
                            if (getValues != null)
                            {
                                foreach (string value in getValues)
                                {
                                    if (valCount == 0)
                                    {
                                        val += value;
                                        valCount++;
                                    }
                                    else
                                    {
                                        val += $",{value}";
                                        valCount++;
                                    }
                                }
                            }

                            hrr.Headers.AddAndUpdate(key, val);
                        }
                    }

                    Stream? stream = null;
                    if (exceptionResponse.ContentLength > 0)
                    {
                        hrr.ContentLength = exceptionResponse.ContentLength;
                        stream = exceptionResponse.GetResponseStream();
                        byte[] data = await ByteArrayTool.StreamToBytes(stream).ConfigureAwait(false);
                        if (data.Length > 0)
                        {
                            hrr.Data = data;
                            hrr.IsSuccess = true;
                        }
                    }

                    try
                    {
                        _ = Task.Run(() =>
                        {
                            exceptionResponse.Dispose();
                            stream?.Dispose();
                        });
                    }
                    catch (Exception) { }
                }
            }
            catch (HttpRequestException hre)
            {
                if (hre.StatusCode != null)
                {
                    hrr.StatusCode = (HttpStatusCode)hre.StatusCode;
                    hrr.StatusCodeNumber = (int)hrr.StatusCode;
                    hrr.StatusDescription = hrr.StatusCode.ToString();
                }
            }
            catch (Exception ex)
            {
                if (hr.URI == null)
                    Debug.WriteLine("HttpRequest SendAsync: " + ex.GetInnerExceptions());
                else
                {
                    try
                    {
                        string host = string.Empty;
                        for (int n = 0; n < hr.Headers.Count; n++)
                        {
                            string? key = hr.Headers.GetKey(n);
                            string? val = hr.Headers.Get(n);

                            if (string.IsNullOrEmpty(key)) continue;
                            if (string.IsNullOrEmpty(val)) continue;

                            if (key.ToLower().Trim().Equals("host"))
                            {
                                host = val;
                                break;
                            }
                        }
                        //Debug.WriteLine($"HttpRequest SendAsync. URL: {hr.URI}, Host Header: {host}");
                        Debug.WriteLine("HttpRequest SendAsync: " + ex.GetInnerExceptions());
                    }
                    catch (Exception) { }
                }
                hrr.IsSuccess = false;
            }
        });
        try { await task.WaitAsync(TimeSpan.FromMilliseconds(hr.TimeoutMS), hr.CT); } catch (Exception) { }

        return hrr;
    }
}