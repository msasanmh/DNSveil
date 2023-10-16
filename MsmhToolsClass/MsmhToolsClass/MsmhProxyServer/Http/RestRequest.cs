using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Diagnostics;
using System.Security.Authentication;
using System.Net.Http.Headers;
using System.Collections.Specialized;
// https://github.com/jchristn/RestWrapper/blob/master/src/RestWrapper/RestRequest.cs

#nullable enable
namespace MsmhToolsClass.MsmhProxyServer;

/// <summary>
/// Authorization header options.
/// </summary>
public class AuthorizationHeader
{
    /// <summary>
    /// The username to use in the authorization header, if any.
    /// </summary>
    public string? User = null;

    /// <summary>
    /// The password to use in the authorization header, if any.
    /// </summary>
    public string? Password = null;

    /// <summary>
    /// The bearer token to use in the authorization header, if any.
    /// </summary>
    public string? BearerToken = null;

    /// <summary>
    /// Enable to encode credentials in the authorization header.
    /// </summary>
    public bool EncodeCredentials = true;

    /// <summary>
    /// Instantiate the object.
    /// </summary>
    public AuthorizationHeader()
    {

    }
}

/// <summary>
/// RESTful HTTP request to be sent to a server.
/// </summary>
public class RestRequest
{
    /// <summary>
    /// Method to invoke when sending log messages.
    /// </summary>
    [JsonIgnore]
    public Action<string>? Logger { get; set; } = null;

    /// <summary>
    /// The URL to which the request should be directed.
    /// </summary>
    public string? Url { get; set; } = null;

    /// <summary>
    /// The HTTP method to use, also known as a verb (GET, PUT, POST, DELETE, etc).
    /// </summary>
    public HttpMethod Method = HttpMethod.Get;

    private AuthorizationHeader _Authorization = new();

    /// <summary>
    /// Authorization header parameters.
    /// </summary>
    public AuthorizationHeader Authorization
    {
        get
        {
            return _Authorization;
        }
        set
        {
            if (value == null) _Authorization = new AuthorizationHeader();
            else _Authorization = value;
        }
    }

    /// <summary>
    /// Ignore certificate errors such as expired certificates, self-signed certificates, or those that cannot be validated.
    /// </summary>
    public bool IgnoreCertificateErrors { get; set; } = false;

    /// <summary>
    /// The filename of the file containing the certificate.
    /// </summary>
    public string? CertificateFilename { get; set; } = null;

    /// <summary>
    /// The password to the certificate file.
    /// </summary>
    public string? CertificatePassword { get; set; } = null;

    /// <summary>
    /// The query elements attached to the URL.
    /// </summary>
    public NameValueCollection Query
    {
        get
        {
            NameValueCollection ret = new(StringComparer.InvariantCultureIgnoreCase);

            if (!string.IsNullOrEmpty(Url))
            {
                if (Url.Contains('?'))
                {
                    string query = Url[(Url.IndexOf("?") + 1)..];

                    if (!string.IsNullOrEmpty(query))
                    {
                        string[] elements = query.Split('&');

                        if (elements != null && elements.Length > 0)
                        {
                            for (int i = 0; i < elements.Length; i++)
                            {
                                string[] elementParts = elements[i].Split(new char[] { '=' }, 2, StringSplitOptions.None);

                                if (elementParts.Length == 1)
                                {
                                    ret.Add(elementParts[0], null);
                                }
                                else
                                {
                                    ret.Add(elementParts[0], elementParts[1]);
                                }
                            }
                        }
                    }
                }
            }

            return ret;
        }
    }

    /// <summary>
    /// The HTTP headers to attach to the request.
    /// </summary>
    public NameValueCollection? Headers
    {
        get
        {
            return _Headers;
        }
        set
        {
            if (value == null) _Headers = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
            else _Headers = value;
        }
    }

    /// <summary>
    /// The content type of the payload (i.e. Data or DataStream).
    /// </summary>
    public string? ContentType { get; set; } = null;

    /// <summary>
    /// The content length of the payload (i.e. Data or DataStream).
    /// </summary>
    public long ContentLength { get; private set; } = 0;

    /// <summary>
    /// The size of the buffer to use while reading from the DataStream and the response stream from the server.
    /// </summary>
    public int BufferSize
    {
        get
        {
            return _StreamReadBufferSize;
        }
        set
        {
            if (value < 1) throw new ArgumentException("StreamReadBufferSize must be at least one byte in size.");
            _StreamReadBufferSize = value;
        }
    }

    /// <summary>
    /// The number of milliseconds to wait before assuming the request has timed out.
    /// </summary>
    public int TimeoutMilliseconds
    {
        get
        {
            return _TimeoutMilliseconds;
        }
        set
        {
            if (value < 1) throw new ArgumentException("Timeout must be greater than 1ms.");
            _TimeoutMilliseconds = value;
        }
    }

    /// <summary>
    /// The user agent header to set on outbound requests.
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;

    /// <summary>
    /// Enable or disable support for automatically handling redirects.
    /// </summary>
    public bool AllowAutoRedirect { get; set; } = true;

    private readonly string _Header = string.Empty;
    private int _StreamReadBufferSize = 65536;
    private int _TimeoutMilliseconds = 30000;
    private NameValueCollection _Headers = new(StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// A simple RESTful HTTP client.
    /// </summary>
    /// <param name="url">URL to access on the server.</param> 
    public RestRequest(string url)
    {
        if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

        Url = url;
    }

    /// <summary>
    /// A simple RESTful HTTP client.
    /// </summary>
    /// <param name="url">URL to access on the server.</param> 
    /// <param name="method">HTTP method to use.</param>
    public RestRequest(string url, HttpMethod method)
    {
        if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

        Url = url;
        Method = method;
    }

    /// <summary>
    /// A simple RESTful HTTP client.
    /// </summary>
    /// <param name="url">URL to access on the server.</param>
    /// <param name="method">HTTP method to use.</param> 
    /// <param name="contentType">Content type to use.</param>
    public RestRequest(string url, HttpMethod method, string contentType)
    {
        if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

        Url = url;
        Method = method;
        ContentType = contentType;
    }

    /// <summary>
    /// A simple RESTful HTTP client.
    /// </summary>
    /// <param name="url">URL to access on the server.</param>
    /// <param name="method">HTTP method to use.</param>
    /// <param name="headers">HTTP headers to use.</param>
    /// <param name="contentType">Content type to use.</param>
    public RestRequest(string url,
                       HttpMethod method,
                       NameValueCollection? headers,
                       string? contentType)
    {
        //if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

        Url = url;
        Method = method;
        Headers = headers;
        ContentType = contentType;
    }

    /// <summary>
    /// Send the HTTP request with no data.
    /// </summary>
    /// <returns>RestResponse.</returns>
    public RestResponse? Send()
    {
        return SendInternal(0, null);
    }

    /// <summary>
    /// Send the HTTP request using form-encoded data.
    /// This method will automatically set the content-type header to 'application/x-www-form-urlencoded' if it is not already set.
    /// </summary>
    /// <param name="form">Dictionary.</param>
    /// <returns></returns>
    public RestResponse? Send(Dictionary<string, string> form)
    {
        // refer to https://github.com/dotnet/runtime/issues/22811
        form ??= new Dictionary<string, string>();
        var items = form.Select(i => WebUtility.UrlEncode(i.Key) + "=" + WebUtility.UrlEncode(i.Value));
        var content = new StringContent(string.Join("&", items), null, "application/x-www-form-urlencoded");
        byte[] bytes = Encoding.UTF8.GetBytes(content.ReadAsStringAsync().Result);
        ContentLength = bytes.Length;
        if (string.IsNullOrEmpty(ContentType)) ContentType = "application/x-www-form-urlencoded";
        return Send(bytes);
    }

    /// <summary>
    /// Send the HTTP request with the supplied data.
    /// </summary>
    /// <param name="data">A string containing the data you wish to send to the server (does not work with GET requests).</param>
    /// <returns>RestResponse.</returns>
    public RestResponse? Send(string data)
    {
        if (string.IsNullOrEmpty(data)) return Send();
        return Send(Encoding.UTF8.GetBytes(data));
    }

    /// <summary>
    /// Send the HTTP request with the supplied data.
    /// </summary>
    /// <param name="data">A byte array containing the data you wish to send to the server (does not work with GET requests).</param>
    /// <returns>RestResponse.</returns>
    public RestResponse? Send(byte[] data)
    {
        long contentLength = 0;
        MemoryStream stream = new(Array.Empty<byte>());

        if (data != null && data.Length > 0)
        {
            contentLength = data.Length;
            stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
        }

        return SendInternal(contentLength, stream);
    }

    /// <summary>
    /// Send the HTTP request with the supplied data.
    /// </summary>
    /// <param name="contentLength">The number of bytes to read from the input stream.</param>
    /// <param name="stream">Stream containing the data you wish to send to the server (does not work with GET requests).</param>
    /// <returns>RestResponse.</returns>
    public RestResponse? Send(long contentLength, Stream? stream)
    {
        return SendInternal(contentLength, stream);
    }

    /// <summary>
    /// Send the HTTP request with no data.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns>RestResponse.</returns>
    public Task<RestResponse?> SendAsync(CancellationToken token = default)
    {
        return SendInternalAsync(0, null, token);
    }

    /// <summary>
    /// Send the HTTP request using form-encoded data.
    /// This method will automatically set the content-type header.
    /// </summary>
    /// <param name="form">Dictionary.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>RestResponse.</returns>
    public Task<RestResponse?> SendAsync(Dictionary<string, string> form, CancellationToken token = default)
    {
        // refer to https://github.com/dotnet/runtime/issues/22811
        form ??= new Dictionary<string, string>();
        var items = form.Select(i => WebUtility.UrlEncode(i.Key) + "=" + WebUtility.UrlEncode(i.Value));
        var content = new StringContent(string.Join("&", items), null, "application/x-www-form-urlencoded");
        byte[] bytes = Encoding.UTF8.GetBytes(content.ReadAsStringAsync(token).Result);
        ContentLength = bytes.Length;
        if (string.IsNullOrEmpty(ContentType)) ContentType = "application/x-www-form-urlencoded";
        return SendAsync(bytes, token);
    }

    /// <summary>
    /// Send the HTTP request with the supplied data.
    /// </summary>
    /// <param name="data">A string containing the data you wish to send to the server (does not work with GET requests).</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>RestResponse.</returns>
    public Task<RestResponse?> SendAsync(string data, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(data)) return SendAsync(token);
        return SendAsync(Encoding.UTF8.GetBytes(data), token);
    }

    /// <summary>
    /// Send the HTTP request with the supplied data.
    /// </summary>
    /// <param name="data">A byte array containing the data you wish to send to the server (does not work with GET requests).</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>RestResponse.</returns>
    public Task<RestResponse?> SendAsync(byte[] data, CancellationToken token = default)
    {
        long contentLength = 0;
        MemoryStream stream = new(Array.Empty<byte>());

        if (data != null && data.Length > 0)
        {
            contentLength = data.Length;
            stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
        }

        return SendInternalAsync(contentLength, stream, token);
    }

    /// <summary>
    /// Send the HTTP request with the supplied data.
    /// </summary>
    /// <param name="contentLength">The number of bytes to read from the input stream.</param>
    /// <param name="stream">A stream containing the data you wish to send to the server (does not work with GET requests).</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>RestResponse.</returns>
    public Task<RestResponse?> SendAsync(long contentLength, Stream? stream, CancellationToken token = default)
    {
        return SendInternalAsync(contentLength, stream, token);
    }

    private bool Validator(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

    private RestResponse? SendInternal(long contentLength, Stream? stream)
    {
        RestResponse? resp = SendInternalAsync(contentLength, stream, CancellationToken.None).Result;
        return resp;
    }

    private async Task<RestResponse?> SendInternalAsync(long contentLength, Stream? stream, CancellationToken token)
    {
        //if (string.IsNullOrEmpty(Url)) throw new ArgumentNullException(nameof(Url));

        //Timestamps ts = new Timestamps();

        Logger?.Invoke(_Header + Method.ToString() + " " + Url);

        try
        {
            // Setup-Webrequest
            Logger?.Invoke(_Header + "setting up web request");

            if (IgnoreCertificateErrors) ServicePointManager.ServerCertificateValidationCallback = Validator;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpClientHandler handler = new();
            handler.AllowAutoRedirect = AllowAutoRedirect;

            if (!string.IsNullOrEmpty(CertificateFilename))
            {
                X509Certificate2? cert = null;

                if (!string.IsNullOrEmpty(CertificatePassword))
                {
                    Logger?.Invoke(_Header + "adding certificate including password");
                    cert = new X509Certificate2(CertificateFilename, CertificatePassword);
                }
                else
                {
                    Logger?.Invoke(_Header + "adding certificate without password");
                    cert = new X509Certificate2(CertificateFilename);
                }

                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.SslProtocols = SslProtocols.Tls12;
                handler.ClientCertificates.Add(cert);
            }

            HttpClient client = new(handler);
            client.Timeout = TimeSpan.FromMilliseconds(_TimeoutMilliseconds);
            client.DefaultRequestHeaders.ExpectContinue = false;
            client.DefaultRequestHeaders.ConnectionClose = true;

            HttpRequestMessage? message = null;

            if (Method == HttpMethod.Delete)
            {
                message = new HttpRequestMessage(HttpMethod.Delete, Url);
            }
            else if (Method == HttpMethod.Get)
            {
                message = new HttpRequestMessage(HttpMethod.Get, Url);
            }
            else if (Method == HttpMethod.Head)
            {
                message = new HttpRequestMessage(HttpMethod.Head, Url);
            }
            else if (Method == HttpMethod.Options)
            {
                message = new HttpRequestMessage(HttpMethod.Options, Url);
            }
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            else if (Method == HttpMethod.Patch)
            {
                message = new HttpRequestMessage(HttpMethod.Patch, Url);
            }
#endif
            else if (Method == HttpMethod.Post)
            {
                message = new HttpRequestMessage(HttpMethod.Post, Url);
            }
            else if (Method == HttpMethod.Put)
            {
                message = new HttpRequestMessage(HttpMethod.Put, Url);
            }
            else if (Method == HttpMethod.Trace)
            {
                message = new HttpRequestMessage(HttpMethod.Trace, Url);
            }
            else
            {
                throw new ArgumentException("HTTP method '" + Method.ToString() + "' is not supported.");
            }

            // Write-Request-Body-Data
            HttpContent? content = null;

            if (Method != HttpMethod.Get && Method != HttpMethod.Head)
            {
                if (contentLength > 0 && stream != null)
                {
                    Logger?.Invoke(_Header + "adding " + contentLength + " bytes to request");
                    content = new StreamContent(stream, _StreamReadBufferSize);
                    //content.Headers.ContentLength = ContentLength;
                    if (!string.IsNullOrEmpty(ContentType))
                        content.Headers.ContentType = new MediaTypeHeaderValue(ContentType);
                }
            }

            message.Content = content;

            if (Headers != null && Headers.Count > 0)
            {
                for (int i = 0; i < Headers.Count; i++)
                {
                    string? key = Headers.GetKey(i);
                    string? val = Headers.Get(i);

                    if (string.IsNullOrEmpty(key)) continue;
                    if (string.IsNullOrEmpty(val)) continue;

                    Logger?.Invoke(_Header + "adding header " + key + ": " + val);

                    if (key.ToLower().Trim().Equals("close"))
                    {
                        // do nothing
                    }
                    else if (key.ToLower().Trim().Equals("connection"))
                    {
                        // do nothing
                    }
                    else if (key.ToLower().Trim().Equals("content-length"))
                    {
                        // do nothing
                    }
                    else if (key.ToLower().Trim().Equals("content-type"))
                    {
                        if (message.Content != null)
                            message.Content.Headers.ContentType = new MediaTypeHeaderValue(val);
                    }
                    else
                    {
                        client.DefaultRequestHeaders.Add(key, val);
                    }
                }
            }

            // Add-Auth-Info
            if (!string.IsNullOrEmpty(_Authorization.User))
            {
                if (_Authorization.EncodeCredentials)
                {
                    Logger?.Invoke(_Header + "adding encoded credentials for user " + _Authorization.User);

                    string authInfo = _Authorization.User + ":" + _Authorization.Password;
                    authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                    client.DefaultRequestHeaders.Add("Authorization", "Basic " + authInfo);
                }
                else
                {
                    Logger?.Invoke(_Header + "adding plaintext credentials for user " + _Authorization.User);
                    client.DefaultRequestHeaders.Add("Authorization", "Basic " + _Authorization.User + ":" + _Authorization.Password);
                }
            }
            else if (!string.IsNullOrEmpty(_Authorization.BearerToken))
            {
                Logger?.Invoke(_Header + "adding authorization bearer token " + _Authorization.BearerToken);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _Authorization.BearerToken);
            }

            // Submit-Request-and-Build-Response
            HttpResponseMessage? response = null;
            try
            {
                response = await client.SendAsync(message, token).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // do nothing
            }

            if (response != null)
            {
                Logger?.Invoke(_Header + response.StatusCode + " response received after " + DateTime.Now);

                RestResponse ret = new();
                ret.ProtocolVersion = "HTTP/" + response.Version.ToString();
                ret.StatusCode = (int)response.StatusCode;
                ret.StatusDescription = response.StatusCode.ToString();

                if (response.Content != null && response.Content.Headers != null)
                {
                    ret.ContentEncoding = string.Join(",", response.Content.Headers.ContentEncoding);
                    if (response.Content.Headers.ContentType != null)
                        ret.ContentType = response.Content.Headers.ContentType.ToString();

                    if (response.Content.Headers.ContentLength != null)
                        ret.ContentLength = response.Content.Headers.ContentLength.Value;
                }

                Logger?.Invoke(_Header + "processing response headers after " + DateTime.Now);

                foreach (var header in response.Headers)
                {
                    string key = header.Key;
                    string val = string.Join(",", header.Value);
                    ret.Headers?.Add(key, val);
                }

                if (ret.ContentLength > 0)
                {
                    if (response.Content != null)
                        ret.Data = await response.Content.ReadAsStreamAsync(token);
                }

                return ret;
            }

            return null;
        }
        catch (TaskCanceledException)
        {
            Logger?.Invoke(_Header + "task canceled");
            return null;
        }
        catch (OperationCanceledException)
        {
            Logger?.Invoke(_Header + "operation canceled");
            return null;
        }
        catch (WebException we)
        {
            // WebException
            Logger?.Invoke(_Header + "web exception: " + we.Message);

            RestResponse ret = new();
            ret.Headers = new();
            ret.ContentEncoding = null;
            ret.ContentType = null;
            ret.ContentLength = 0;
            ret.StatusCode = 0;
            ret.StatusDescription = null;
            ret.Data = null;

            if (we.Response is HttpWebResponse exceptionResponse)
            {
                ret.ProtocolVersion = "HTTP/" + exceptionResponse.ProtocolVersion.ToString();
                ret.ContentEncoding = exceptionResponse.ContentEncoding;
                ret.ContentType = exceptionResponse.ContentType;
                ret.ContentLength = exceptionResponse.ContentLength;
                ret.StatusCode = (int)exceptionResponse.StatusCode;
                ret.StatusDescription = exceptionResponse.StatusDescription;

                Logger?.Invoke(_Header + "server returned status code " + ret.StatusCode);

                if (exceptionResponse.Headers != null && exceptionResponse.Headers.Count > 0)
                {
                    for (int i = 0; i < exceptionResponse.Headers.Count; i++)
                    {
                        string key = exceptionResponse.Headers.GetKey(i);
                        string val = "";
                        int valCount = 0;

                        string[]? getValues = exceptionResponse.Headers.GetValues(i);
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
                                    val += "," + value;
                                    valCount++;
                                }
                            }
                        }

                        Logger?.Invoke(_Header + "adding exception header " + key + ": " + val);
                        ret.Headers?.Add(key, val);
                    }
                }

                if (exceptionResponse.ContentLength > 0)
                {
                    Logger?.Invoke(_Header + "attaching exception response stream to response with content length " + exceptionResponse.ContentLength + " bytes");
                    ret.ContentLength = exceptionResponse.ContentLength;
                    ret.Data = exceptionResponse.GetResponseStream();
                }
                else
                {
                    ret.ContentLength = 0;
                    ret.Data = null;
                }
            }

            return ret;
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            Logger?.Invoke(_Header + "complete (" + DateTime.Now + ")");
        }
    }
}
