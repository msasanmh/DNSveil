using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Diagnostics;

namespace MsmhTools.HTTPProxyServer
{
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
        public HttpMethodReq Method = HttpMethodReq.GET;


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
        /// The HTTP headers to attach to the request.
        /// </summary>
        public Dictionary<string, string>? Headers
        {
            get
            {
                return _Headers;
            }
            set
            {
                if (value == null) _Headers = new Dictionary<string, string>();
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
        public int Timeout
        {
            get
            {
                return _Timeout;
            }
            set
            {
                if (value < 1) throw new ArgumentException("Timeout must be greater than 1ms.");
                _Timeout = value;
            }
        }

        /// <summary>
        /// The user agent header to set on outbound requests.
        /// </summary>
        public string UserAgent { get; set; } = string.Empty;

        private string _Header = string.Empty;
        private int _StreamReadBufferSize = 65536;
        private int _Timeout = 30000;
        private Dictionary<string, string> _Headers = new();

        /// <summary>
        /// A simple RESTful HTTP client.
        /// </summary>
        /// <param name="url">URL to access on the server.</param> 
        public RestRequest(string url)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

            Url = url;
            Method = HttpMethodReq.GET;
        }

        /// <summary>
        /// A simple RESTful HTTP client.
        /// </summary>
        /// <param name="url">URL to access on the server.</param> 
        /// <param name="method">HTTP method to use.</param>
        public RestRequest(string url, HttpMethodReq method)
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
        public RestRequest(
            string url,
            HttpMethodReq method,
            string contentType)
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
        public RestRequest(
            string url,
            HttpMethodReq method,
            Dictionary<string, string>? headers,
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
            if (form == null) form = new Dictionary<string, string>();
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
            if (form == null) form = new Dictionary<string, string>();
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

        private RestResponse? SendInternal(long contentLength, Stream? stream)
        {
            RestResponse? resp = SendInternalAsync(contentLength, stream, CancellationToken.None).Result;
            return resp;
        }

        private async Task<RestResponse?> SendInternalAsync(long contentLength, Stream? stream, CancellationToken token)
        {
            if (string.IsNullOrEmpty(Url)) throw new ArgumentNullException(nameof(Url));

            Logger?.Invoke(_Header + Method.ToString() + " " + Url);

            try
            {
                // Setup-Webrequest
                Logger?.Invoke(_Header + "setting up web request");

                //if (IgnoreCertificateErrors) ServicePointManager.ServerCertificateValidationCallback = Validator;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                
                HttpWebRequest client = (HttpWebRequest)WebRequest.Create(Url);
                client.UserAgent = UserAgent;
                client.KeepAlive = false;
                client.Method = Method.ToString();
                client.AllowAutoRedirect = true;
                client.Timeout = _Timeout;
                client.ContentLength = 0;
                client.ContentType = ContentType;
                client.ServicePoint.Expect100Continue = false;
                client.ServicePoint.UseNagleAlgorithm = false;
                client.ServicePoint.ConnectionLimit = 4096;

                // Add-Certificate
                if (!string.IsNullOrEmpty(CertificateFilename))
                {
                    if (!string.IsNullOrEmpty(CertificatePassword))
                    {
                        Logger?.Invoke(_Header + "adding certificate including password");

                        X509Certificate2 cert = new(CertificateFilename, CertificatePassword);
                        client.ClientCertificates.Add(cert);
                    }
                    else
                    {
                        Logger?.Invoke(_Header + "adding certificate without password");

                        X509Certificate2 cert = new(CertificateFilename);
                        client.ClientCertificates.Add(cert);
                    }
                }

                // Add-Headers
                if (Headers != null && Headers.Count > 0)
                {
                    foreach (KeyValuePair<string, string> pair in Headers)
                    {
                        if (string.IsNullOrEmpty(pair.Key)) continue;
                        if (string.IsNullOrEmpty(pair.Value)) continue;

                        Logger?.Invoke(_Header + "adding header " + pair.Key + ": " + pair.Value);

                        if (pair.Key.ToLower().Trim().Equals("accept"))
                        {
                            client.Accept = pair.Value;
                        }
                        else if (pair.Key.ToLower().Trim().Equals("close"))
                        {
                            // do nothing
                        }
                        else if (pair.Key.ToLower().Trim().Equals("connection"))
                        {
                            // do nothing
                        }
                        else if (pair.Key.ToLower().Trim().Equals("content-length"))
                        {
                            client.ContentLength = Convert.ToInt64(pair.Value);
                        }
                        else if (pair.Key.ToLower().Trim().Equals("content-type"))
                        {
                            client.ContentType = pair.Value;
                        }
                        else if (pair.Key.ToLower().Trim().Equals("date"))
                        {
                            client.Date = Convert.ToDateTime(pair.Value);
                        }
                        else if (pair.Key.ToLower().Trim().Equals("expect"))
                        {
                            client.Expect = pair.Value;
                        }
                        else if (pair.Key.ToLower().Trim().Equals("host"))
                        {
                            client.Host = pair.Value;
                        }
                        else if (pair.Key.ToLower().Trim().Equals("if-modified-since"))
                        {
                            client.IfModifiedSince = Convert.ToDateTime(pair.Value);
                        }
                        else if (pair.Key.ToLower().Trim().Equals("keep-alive"))
                        {
                            client.KeepAlive = Convert.ToBoolean(pair.Value);
                        }
                        else if (pair.Key.ToLower().Trim().Equals("proxy-connection"))
                        {
                            // do nothing
                        }
                        else if (pair.Key.ToLower().Trim().Equals("referer"))
                        {
                            client.Referer = pair.Value;
                        }
                        else if (pair.Key.ToLower().Trim().Equals("transfer-encoding"))
                        {
                            client.TransferEncoding = pair.Value;
                        }
                        else if (pair.Key.ToLower().Trim().Equals("user-agent"))
                        {
                            client.UserAgent = pair.Value;
                        }
                        else
                        {
                            client.Headers.Add(pair.Key, pair.Value);
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
                        client.Headers.Add("Authorization", "Basic " + authInfo);
                    }
                    else
                    {
                        Logger?.Invoke(_Header + "adding plaintext credentials for user " + _Authorization.User);
                        client.Headers.Add("Authorization", "Basic " + _Authorization.User + ":" + _Authorization.Password);
                    }
                }
                else if (!string.IsNullOrEmpty(_Authorization.BearerToken))
                {
                    Logger?.Invoke(_Header + "adding authorization bearer token " + _Authorization.BearerToken);
                    client.Headers.Add("Authorization", "Bearer " + _Authorization.BearerToken);
                }

                // Write-Request-Body-Data
                if (Method != HttpMethodReq.GET && Method != HttpMethodReq.HEAD)
                {
                    if (contentLength > 0 && stream != null)
                    {
                        Logger?.Invoke(_Header + "reading data (" + contentLength + " bytes), writing to request");

                        client.ContentLength = contentLength;

                        if (string.IsNullOrEmpty(client.ContentType) && !string.IsNullOrEmpty(ContentType))
                            client.ContentType = ContentType;

                        Stream clientStream = client.GetRequestStream();

                        byte[] buffer = new byte[_StreamReadBufferSize];
                        long bytesRemaining = contentLength;
                        int bytesRead = 0;

                        while (bytesRemaining > 0)
                        {
                            bytesRead = await stream.ReadAsync(buffer, token).ConfigureAwait(false);
                            if (bytesRead > 0)
                            {
                                bytesRemaining -= bytesRead;
                                await clientStream.WriteAsync(buffer.AsMemory(0, bytesRead), token).ConfigureAwait(false);
                            }
                        }

                        clientStream.Close();

                        Logger?.Invoke(_Header + "added " + contentLength + " bytes to request");
                    }
                }

                // Submit-Request-and-Build-Response
                Logger?.Invoke(_Header + "submitting (" + DateTime.Now + ")");
                HttpWebResponse response = (HttpWebResponse)(await client.GetResponseAsync().ConfigureAwait(false));

                Logger?.Invoke(_Header + "server returned " + (int)response.StatusCode + " (" + DateTime.Now + ")");

                RestResponse rResponse = new();
                rResponse.ProtocolVersion = "HTTP/" + response.ProtocolVersion.ToString();
                rResponse.ContentEncoding = response.ContentEncoding;
                rResponse.ContentType = response.ContentType;
                rResponse.ContentLength = response.ContentLength;
                rResponse.ResponseURI = response.ResponseUri.ToString();
                rResponse.StatusCode = (int)response.StatusCode;
                rResponse.StatusDescription = response.StatusDescription;

                // Headers
                Logger?.Invoke(_Header + "processing response headers (" + DateTime.Now + ")");

                if (response.Headers != null && response.Headers.Count > 0)
                {
                    rResponse.Headers = new Dictionary<string, string>();

                    for (int i = 0; i < response.Headers.Count; i++)
                    {
                        string key = response.Headers.GetKey(i);
                        string val = "";
                        int valCount = 0;
                        string[]? array = response.Headers.GetValues(i);
                        if (array != null)
                        {
                            for (int i1 = 0; i1 < array.Length; i1++)
                            {
                                string value = array[i1];
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
                        
                        Logger?.Invoke(_Header + "adding response header " + key + ": " + val);
                        rResponse.Headers.Add(key, val);
                    }
                }

                // Payload
                bool contentLengthHeaderExists = false;
                if (rResponse.Headers != null && rResponse.Headers.Count > 0)
                {
                    foreach (KeyValuePair<string, string> header in rResponse.Headers)
                    {
                        if (string.IsNullOrEmpty(header.Key)) continue;
                        if (header.Key.ToLower().Equals("content-length"))
                        {
                            contentLengthHeaderExists = true;
                            break;
                        }
                    }
                }

                if (!contentLengthHeaderExists)
                {
                    Logger?.Invoke(_Header + "content-length header not supplied");

                    long totalBytesRead = 0;
                    int bytesRead = 0;
                    byte[] buffer = new byte[_StreamReadBufferSize];
                    MemoryStream ms = new();

                    while (true)
                    {
                        bytesRead = await response.GetResponseStream().ReadAsync(buffer, token).ConfigureAwait(false);
                        if (bytesRead > 0)
                        {
                            await ms.WriteAsync(buffer.AsMemory(0, bytesRead), token).ConfigureAwait(false);
                            totalBytesRead += bytesRead;
                            Logger?.Invoke(_Header + "read " + bytesRead + " bytes, " + totalBytesRead + " total bytes");
                        }
                        else
                        {
                            break;
                        }
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    rResponse.ContentLength = totalBytesRead;
                    rResponse.Data = ms;
                }
                else if (response.ContentLength > 0)
                {
                    Logger?.Invoke(_Header + "attaching response stream with content length " + response.ContentLength + " bytes");
                    rResponse.ContentLength = response.ContentLength;
                    rResponse.Data = response.GetResponseStream();
                }
                else
                {
                    rResponse.ContentLength = 0;
                    rResponse.Data = null;
                }

                return rResponse;
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

                RestResponse rResponse = new();
                rResponse.Headers = null;
                rResponse.ContentEncoding = null;
                rResponse.ContentType = null;
                rResponse.ContentLength = 0;
                rResponse.ResponseURI = null;
                rResponse.StatusCode = 0;
                rResponse.StatusDescription = null;
                rResponse.Data = null;

                if (we.Response is HttpWebResponse exceptionResponse)
                {
                    rResponse.ProtocolVersion = "HTTP/" + exceptionResponse.ProtocolVersion.ToString();
                    rResponse.ContentEncoding = exceptionResponse.ContentEncoding;
                    rResponse.ContentType = exceptionResponse.ContentType;
                    rResponse.ContentLength = exceptionResponse.ContentLength;
                    rResponse.ResponseURI = exceptionResponse.ResponseUri.ToString();
                    rResponse.StatusCode = (int)exceptionResponse.StatusCode;
                    rResponse.StatusDescription = exceptionResponse.StatusDescription;

                    Logger?.Invoke(_Header + "server returned status code " + rResponse.StatusCode);

                    if (exceptionResponse.Headers != null && exceptionResponse.Headers.Count > 0)
                    {
                        rResponse.Headers = new Dictionary<string, string>();
                        for (int i = 0; i < exceptionResponse.Headers.Count; i++)
                        {
                            string key = exceptionResponse.Headers.GetKey(i);
                            string val = "";
                            int valCount = 0;

                            string[]? array = exceptionResponse.Headers.GetValues(i);
                            if (array != null)
                            {
                                for (int i1 = 0; i1 < array.Length; i1++)
                                {
                                    string value = array[i1];
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
                            rResponse.Headers.Add(key, val);
                        }
                    }

                    if (exceptionResponse.ContentLength > 0)
                    {
                        Logger?.Invoke(_Header + "attaching exception response stream to response with content length " + exceptionResponse.ContentLength + " bytes");
                        rResponse.ContentLength = exceptionResponse.ContentLength;
                        rResponse.Data = exceptionResponse.GetResponseStream();
                    }
                    else
                    {
                        rResponse.ContentLength = 0;
                        rResponse.Data = null;
                    }
                }

                return rResponse;
            }
            finally
            {
                Logger?.Invoke(_Header + "complete (" + DateTime.Now + ")");
            }
        }
    }
}
