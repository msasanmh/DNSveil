using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;

namespace MsmhToolsClass.MsmhProxyServer;

/// <summary>
/// Serialization helper.
/// </summary>
public interface ISerializationHelper
{
    /// <summary>
    /// Deserialize from JSON to an object of the specified type.
    /// </summary>
    /// <typeparam name="T">Type.</typeparam>
    /// <param name="json">JSON string.</param>
    /// <returns>Instance.</returns>
    T DeserializeJson<T>(string json);

    /// <summary>
    /// Serialize from object to JSON.
    /// </summary>
    /// <param name="obj">Object.</param>
    /// <param name="pretty">Pretty print.</param>
    /// <returns>JSON.</returns>
    string SerializeJson(object obj, bool pretty = true);
}

/// <summary>
/// A chunk of data, used when reading from a request where the Transfer-Encoding header includes 'chunked'.
/// </summary>
public class Chunk
{
    /// <summary>
    /// Length of the data.
    /// </summary>
    public int Length = 0;

    /// <summary>
    /// Data.
    /// </summary>
    public byte[]? Data = null;

    /// <summary>
    /// Any additional metadata that appears on the length line after the length hex value and semicolon.
    /// </summary>
    public string? Metadata = null;

    /// <summary>
    /// Indicates whether or not this is the final chunk, i.e. the chunk length received was zero.
    /// </summary>
    public bool IsFinalChunk = false;

    internal Chunk()
    {

    }
}

/// <summary>
/// HTTP request.
/// </summary>
public class HttpRequest
{
    /// <summary>
    /// UTC timestamp from when the request was received.
    /// </summary>
    [JsonPropertyOrder(-10)]
    public DateTime TimestampUtc { get; private set; } = DateTime.Now.ToUniversalTime();

    /// <summary>
    /// Thread ID on which the request exists.
    /// </summary>
    [JsonPropertyOrder(-9)]
    public int ThreadId { get; private set; } = Environment.CurrentManagedThreadId;

    /// <summary>
    /// The protocol and version.
    /// </summary>
    [JsonPropertyOrder(-9)]
    public string? ProtocolVersion { get; set; } = null;

    /// <summary>
    /// Source (requestor) IP and port information.
    /// </summary>
    [JsonPropertyOrder(-8)]
    public SourceDetails Source { get; set; } = new SourceDetails();

    /// <summary>
    /// Destination IP and port information.
    /// </summary>
    [JsonPropertyOrder(-7)]
    public DestinationDetails Destination { get; set; } = new DestinationDetails();

    /// <summary>
    /// The HTTP method used in the request.
    /// </summary>
    [JsonPropertyOrder(-6)]
    public HttpMethod Method { get; set; } = HttpMethod.Get;

    /// <summary>
    /// The string version of the HTTP method, useful if Method is UNKNOWN.
    /// </summary>
    [JsonPropertyOrder(-5)]
    public string? MethodRaw { get; set; } = null;

    /// <summary>
    /// URL details.
    /// </summary>
    [JsonPropertyOrder(-4)]
    public UrlDetails Url { get; set; } = new UrlDetails();

    /// <summary>
    /// Query details.
    /// </summary>
    [JsonPropertyOrder(-3)]
    public QueryDetails Query { get; set; } = new QueryDetails();

    /// <summary>
    /// The headers found in the request.
    /// </summary>
    [JsonPropertyOrder(-2)]
    public NameValueCollection Headers
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
    /// Specifies whether or not the client requested HTTP keepalives.
    /// </summary>
    public bool Keepalive { get; set; } = false;

    /// <summary>
    /// Indicates whether or not chunked transfer encoding was detected.
    /// </summary>
    public bool ChunkedTransfer { get; set; } = false;

    /// <summary>
    /// Indicates whether or not the payload has been gzip compressed.
    /// </summary>
    public bool Gzip { get; set; } = false;

    /// <summary>
    /// Indicates whether or not the payload has been deflate compressed.
    /// </summary>
    public bool Deflate { get; set; } = false;

    /// <summary>
    /// The useragent specified in the request.
    /// </summary>
    public string? Useragent { get; set; } = null;

    /// <summary>
    /// The content type as specified by the requestor (client).
    /// </summary>
    [JsonPropertyOrder(990)]
    public string? ContentType { get; set; } = null;

    /// <summary>
    /// The number of bytes in the request body.
    /// </summary>
    [JsonPropertyOrder(991)]
    public long ContentLength { get; private set; } = 0;

    /// <summary>
    /// The stream from which to read the request body sent by the requestor (client).
    /// </summary>
    [JsonIgnore]
    public Stream? Data;

    /// <summary>
    /// Retrieve the request body as a byte array.  This will fully read the stream. 
    /// </summary>
    [JsonIgnore]
    public byte[]? DataAsBytes
    {
        get
        {
            if (_DataAsBytes != null) return _DataAsBytes;
            if (Data != null && ContentLength > 0)
            {
                _DataAsBytes = ReadStreamFully(Data);
                return _DataAsBytes;
            }
            return null;
        }
    }

    /// <summary>
    /// Retrieve the request body as a string.  This will fully read the stream.
    /// </summary>
    [JsonIgnore]
    public string? DataAsString
    {
        get
        {
            if (_DataAsBytes != null) return Encoding.UTF8.GetString(_DataAsBytes);
            if (Data != null && ContentLength > 0)
            {
                _DataAsBytes = ReadStreamFully(Data);
                if (_DataAsBytes != null) return Encoding.UTF8.GetString(_DataAsBytes);
            }
            return null;
        }
    }

    /// <summary>
    /// The original HttpListenerContext from which the HttpRequest was constructed.
    /// </summary>
    [JsonIgnore]
    public HttpListenerContext? ListenerContext;

    /// <summary>
    /// Close request if didn't receive data for n seconds. Default: 0 Sec (Disabled)
    /// </summary>
    public int TimeoutSec { get; set; } = 0;

    /// <summary>
    /// Apply DPI Bypass to this Request if DPI Bypass Program is available.
    /// </summary>
    public bool ApplyDpiBypass { get; set; } = false;

    public bool IsDestBlocked { get; set; } = false;

    public bool ApplyUpStreamProxy { get; set; } = false;

    private readonly int _StreamBufferSize = 65536;
    private readonly Uri? _Uri = null;
    private byte[]? _DataAsBytes = null;
    private readonly ISerializationHelper? _Serializer = null;
    private NameValueCollection _Headers = new(StringComparer.InvariantCultureIgnoreCase);
    private static readonly int _TimeoutDataReadMs = 2000;
    private static readonly int _DataReadSleepMs = 10;

    /// <summary>
    /// HTTP request.
    /// </summary>
    public HttpRequest()
    {

    }

    public HttpRequest(TcpClient client)
    {
        try
        {
            IPEndPoint? clientIpEndpoint = client.Client.RemoteEndPoint as IPEndPoint;
            IPEndPoint? serverIpEndpoint = client.Client.LocalEndPoint as IPEndPoint;

            string clientEndpoint = clientIpEndpoint != null ? clientIpEndpoint.ToString() : string.Empty;
            string serverEndpoint = serverIpEndpoint != null ? serverIpEndpoint.ToString() : string.Empty;

            string clientIp = clientIpEndpoint != null ? clientIpEndpoint.Address.ToString() : string.Empty;
            int clientPort = clientIpEndpoint != null ? clientIpEndpoint.Port : 0;

            string serverIp = serverIpEndpoint != null ? serverIpEndpoint.Address.ToString() : string.Empty;
            int serverPort = serverIpEndpoint != null ? serverIpEndpoint.Port : 0;

            byte[]? headerBytes = null;
            byte[] lastFourBytes = new byte[4];
            lastFourBytes[0] = 0x00;
            lastFourBytes[1] = 0x00;
            lastFourBytes[2] = 0x00;
            lastFourBytes[3] = 0x00;

            // Attach-Stream
            NetworkStream stream = client.GetStream();

            if (!stream.CanRead) return;

            // Read-Headers
            using (MemoryStream headerMs = new())
            {
                // Read-Header-Bytes
                byte[] headerBuffer = new byte[1];
                int read = 0;
                int headerBytesRead = 0;

                while ((read = stream.Read(headerBuffer, 0, headerBuffer.Length)) > 0)
                {
                    if (read > 0)
                    {
                        // Initialize-Header-Bytes-if-Needed
                        headerBytesRead += read;
                        headerBytes ??= new byte[1];

                        // Update-Last-Four
                        if (read == 1)
                        {
                            lastFourBytes[0] = lastFourBytes[1];
                            lastFourBytes[1] = lastFourBytes[2];
                            lastFourBytes[2] = lastFourBytes[3];
                            lastFourBytes[3] = headerBuffer[0];
                        }

                        // Append-to-Header-Buffer
                        byte[] tempHeader = new byte[headerBytes.Length + 1];
                        Buffer.BlockCopy(headerBytes, 0, tempHeader, 0, headerBytes.Length);
                        tempHeader[headerBytes.Length] = headerBuffer[0];
                        headerBytes = tempHeader;

                        // Check-for-End-of-Headers
                        if ((int)(lastFourBytes[0]) == 13
                            && (int)(lastFourBytes[1]) == 10
                            && (int)(lastFourBytes[2]) == 13
                            && (int)(lastFourBytes[3]) == 10)
                        {
                            break;
                        }
                    }
                }
            }

            // Process-Headers
            if (headerBytes == null || headerBytes.Length < 1)
            {
                // No header data read from the stream
                return;
            }
            //Debug.WriteLine("== " + BitConverter.ToString(headerBytes).Replace("-", " "));
            HttpRequest ret = BuildHeaders(headerBytes);
            ContentLength = ret.ContentLength;
            ContentType = ret.ContentType;
            Headers = ret.Headers;
            Keepalive = ret.Keepalive;
            Method = ret.Method;
            ProtocolVersion = ret.ProtocolVersion;
            ThreadId = ret.ThreadId;
            TimestampUtc = ret.TimestampUtc;
            Useragent = ret.Useragent;

            // Read-Data
            byte[]? data = null;
            if (ContentLength > 0)
            {
                // Read-from-Stream
                data = new byte[ContentLength];

                using (MemoryStream dataMs = new())
                {
                    long bytesRemaining = ContentLength;
                    long bytesRead = 0;
                    bool timeout = false;
                    int currentTimeout = 0;

                    int read = 0;
                    byte[] buffer;
                    long bufferSize = 2048;
                    if (bufferSize > bytesRemaining) bufferSize = bytesRemaining;
                    buffer = new byte[bufferSize];

                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if (read > 0)
                        {
                            dataMs.Write(buffer, 0, read);
                            bytesRead += read;
                            bytesRemaining -= read;

                            // reduce buffer size if number of bytes remaining is
                            // less than the pre-defined buffer size of 2KB
                            if (bytesRemaining < bufferSize)
                            {
                                bufferSize = bytesRemaining;
                            }

                            buffer = new byte[bufferSize];

                            // check if read fully
                            if (bytesRemaining == 0) break;
                            if (bytesRead == ContentLength) break;
                        }
                        else
                        {
                            if (currentTimeout >= _TimeoutDataReadMs)
                            {
                                timeout = true;
                                break;
                            }
                            else
                            {
                                currentTimeout += _DataReadSleepMs;
                                Task.Delay(_DataReadSleepMs).Wait();
                            }
                        }
                    }

                    if (timeout)
                    {
                        throw new IOException("Timeout reading data from stream.");
                    }

                    Data = dataMs;
                }

                // Validate-Data
                if (Data == null || Data.Length < 1)
                {
                    throw new IOException("Unable to read data from stream.");
                }

                if (Data.Length != ContentLength)
                {
                    throw new IOException("Data read does not match specified content length.");
                }
            }

            // serverIp is 127.0.0.1 unless we use DNS resolver
            Destination = new(serverIp, ret.Destination.Port, ret.Destination.Hostname);
            Source = new(clientIp, clientPort);
            Url = new(ret.Url.Full, ret.Url.RawWithQuery);
        }
        catch (Exception)
        {
            // do nothing
        }
    }

    private static HttpRequest BuildHeaders(byte[] bytes)
    {
        // Initial-Values
        HttpRequest ret = new();
        ret.TimestampUtc = DateTime.Now.ToUniversalTime();
        ret.ThreadId = Environment.CurrentManagedThreadId;
        ret.Headers = new();

        // Convert-to-String-List
        string str = Encoding.UTF8.GetString(bytes);
        string[] headers = str.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        // Process-Each-Line
        for (int i = 0; i < headers.Length; i++)
        {
            if (i == 0)
            {
                // First-Line
                string[] requestLine = headers[i].Trim().Trim('\0').Split(' ');
                if (requestLine.Length < 3) throw new ArgumentException("Request line does not contain at least three parts (method, raw URL, protocol/version).");

                ret.Method = CommonTools.GetHttpMethod.Parse(requestLine[0]);
                ret.Url.Full = requestLine[1];
                ret.ProtocolVersion = requestLine[2];
                ret.Url.RawWithQuery = ret.Url.Full;

                try
                {
                    string ipPort = ret.Url.Full;
                    if (!ipPort.ToLower().StartsWith("http://") && !ipPort.ToLower().StartsWith("https://"))
                    {
                        Uri tempUri = new($"https://{ret.Url.Full}");
                        if (tempUri.Port == 80)
                            ret.Url.Full = $"http://{ret.Url.Full}";
                        else
                            ret.Url.Full = $"https://{ret.Url.Full}";
                    }
                    Uri uri = new(ret.Url.Full);
                    ret.Destination.Hostname = uri.Host;
                    ret.Destination.Port = uri.Port;
                }
                catch (Exception)
                {
                    // do nothing
                }

                if (string.IsNullOrEmpty(ret.Destination.Hostname))
                {
                    if (!ret.Url.Full.Contains("://") & ret.Url.Full.Contains(':'))
                    {
                        string[] hostAndPort = ret.Url.Full.Split(':');
                        if (hostAndPort.Length == 2)
                        {
                            ret.Destination.Hostname = hostAndPort[0];
                            bool isInt = int.TryParse(hostAndPort[1], out int port);
                            if (isInt)
                                ret.Destination.Port = port;
                            else
                            {
                                Debug.WriteLine("Unable to parse destination hostname and port.");
                            }
                        }
                    }
                }
            }
            else
            {
                // Subsequent-Line
                string[] headerLine = headers[i].Split(':');
                if (headerLine.Length == 2)
                {
                    string key = headerLine[0].Trim();
                    string val = headerLine[1].Trim();
                    if (string.IsNullOrEmpty(key)) continue;
                    string keyEval = key.ToLower();

                    if (keyEval.Equals("keep-alive"))
                        ret.Keepalive = Convert.ToBoolean(val);
                    else if (keyEval.Equals("user-agent"))
                        ret.Useragent = val;
                    else if (keyEval.Equals("content-length"))
                        ret.ContentLength = Convert.ToInt64(val);
                    else if (keyEval.Equals("content-type"))
                        ret.ContentType = val;
                    else
                        ret.Headers = CommonTools.AddToDict(key, val, ret.Headers);
                }

            }
        }

        return ret;
    }

    //========================================================================================

    /// <summary>
    /// HTTP request.
    /// Instantiate the object using an HttpListenerContext.
    /// </summary>
    /// <param name="ctx">HttpListenerContext.</param>
    /// <param name="serializer">Serialization helper.</param>
    public HttpRequest(HttpListenerContext ctx, ISerializationHelper serializer)
    {
        if (ctx == null) return;
        if (ctx.Request == null) return;
        if (serializer == null) return;

        if (ctx.Request.Url == null) return;
        if (ctx.Request.RawUrl == null) return;

        _Serializer = serializer;

        ListenerContext = ctx;
        Keepalive = ctx.Request.KeepAlive;
        ContentLength = ctx.Request.ContentLength64;
        Useragent = ctx.Request.UserAgent;
        ContentType = ctx.Request.ContentType;

        _Uri = new Uri(ctx.Request.Url.ToString().Trim());

        ThreadId = Environment.CurrentManagedThreadId;
        TimestampUtc = DateTime.Now.ToUniversalTime();
        ProtocolVersion = "HTTP/" + ctx.Request.ProtocolVersion.ToString();
        Source = new SourceDetails(ctx.Request.RemoteEndPoint.Address.ToString(), ctx.Request.RemoteEndPoint.Port);
        Destination = new DestinationDetails(ctx.Request.LocalEndPoint.Address.ToString(), ctx.Request.LocalEndPoint.Port, _Uri.Host);
        Url = new UrlDetails(ctx.Request.Url.ToString().Trim(), ctx.Request.RawUrl.ToString().Trim());
        if (string.IsNullOrEmpty(Url.Full)) return;
        Query = new QueryDetails(Url.Full);
        MethodRaw = ctx.Request.HttpMethod;

        try
        {
            Method = (HttpMethod)Enum.Parse(typeof(HttpMethod), ctx.Request.HttpMethod, true);
        }
        catch (Exception)
        {
            Method = HttpMethod.Get; // Default
        }

        Headers = ctx.Request.Headers;

        for (int i = 0; i < Headers.Count; i++)
        {
            string? key = Headers.GetKey(i);
            string[]? vals = Headers.GetValues(i);

            if (string.IsNullOrEmpty(key)) continue;
            if (vals == null || vals.Length < 1) continue;

            if (key.ToLower().Equals("transfer-encoding"))
            {
                if (vals.Contains("chunked", StringComparer.InvariantCultureIgnoreCase))
                    ChunkedTransfer = true;
                if (vals.Contains("gzip", StringComparer.InvariantCultureIgnoreCase))
                    Gzip = true;
                if (vals.Contains("deflate", StringComparer.InvariantCultureIgnoreCase))
                    Deflate = true;
            }
            else if (key.ToLower().Equals("x-amz-content-sha256"))
            {
                if (vals.Contains("streaming", StringComparer.InvariantCultureIgnoreCase))
                {
                    ChunkedTransfer = true;
                }
            }
        }

        Data = ctx.Request.InputStream;
    }

    /// <summary>
    /// For chunked transfer-encoded requests, read the next chunk.
    /// It is strongly recommended that you use the ChunkedTransfer parameter before invoking this method.
    /// </summary>
    /// <param name="token">Cancellation token useful for canceling the request.</param>
    /// <returns>Chunk.</returns>
    public async Task<Chunk> ReadChunk(CancellationToken token = default)
    {
        Chunk chunk = new();

        // Get-Length-and-Metadata
        byte[] buffer = new byte[1];
        byte[]? lenBytes = null;
        int bytesRead;

        while (true)
        {
            if (Data == null) break;
            bytesRead = await Data.ReadAsync(buffer, token).ConfigureAwait(false);
            if (bytesRead > 0)
            {
                lenBytes = AppendBytes(lenBytes, buffer);
                if (lenBytes == null) break;
                string lenStr = Encoding.UTF8.GetString(lenBytes);

                if (lenBytes[^1] == 10)
                {
                    lenStr = lenStr.Trim();

                    if (lenStr.Contains(';'))
                    {
                        string[] lenParts = lenStr.Split(new char[] { ';' }, 2);
                        chunk.Length = int.Parse(lenParts[0], NumberStyles.HexNumber);
                        if (lenParts.Length >= 2) chunk.Metadata = lenParts[1];
                    }
                    else
                    {
                        chunk.Length = int.Parse(lenStr, NumberStyles.HexNumber);
                    }

                    break;
                }
            }
        }

        // Get-Data
        int bytesRemaining = chunk.Length;

        if (chunk.Length > 0)
        {
            chunk.IsFinalChunk = false;
            using MemoryStream ms = new();
            while (true)
            {
                if (bytesRemaining > _StreamBufferSize) buffer = new byte[_StreamBufferSize];
                else buffer = new byte[bytesRemaining];

                if (Data == null) break;
                bytesRead = await Data.ReadAsync(buffer, token).ConfigureAwait(false);

                if (bytesRead > 0)
                {
                    await ms.WriteAsync(buffer.AsMemory(0, bytesRead), token);
                    bytesRemaining -= bytesRead;
                }

                if (bytesRemaining == 0) break;
            }

            ms.Seek(0, SeekOrigin.Begin);
            chunk.Data = ms.ToArray();
        }
        else
        {
            chunk.IsFinalChunk = true;
        }

        // Get-Trailing-CRLF
        buffer = new byte[1];

        while (true)
        {
            if (Data == null) break;
            bytesRead = await Data.ReadAsync(buffer, token).ConfigureAwait(false);
            if (bytesRead > 0)
            {
                if (buffer[0] == 10) break;
            }
        }

        return chunk;
    }

    /// <summary>
    /// Read the data stream fully and convert the data to the object type specified using JSON deserialization.
    /// Note: if you use this method, you will not be able to read from the data stream afterward.
    /// </summary>
    /// <typeparam name="T">Type.</typeparam>
    /// <returns>Object of type specified.</returns>
    public T? DataAsJsonObject<T>() where T : class
    {
        if (_Serializer == null) return null;
        string? json = DataAsString;
        if (string.IsNullOrEmpty(json)) return null;
        return _Serializer.DeserializeJson<T>(json);
    }

    /// <summary>
    /// Determine if a header exists.
    /// </summary>
    /// <param name="key">Header key.</param>
    /// <returns>True if exists.</returns>
    public bool HeaderExists(string key)
    {
        try
        {
            return Headers.AllKeys.Any(k => k.ToLower().Equals(key.ToLower()));
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Determine if a querystring entry exists.
    /// </summary>
    /// <param name="key">Querystring key.</param>
    /// <returns>True if exists.</returns>
    public bool QuerystringExists(string key)
    {
        try
        {
            return Query.Elements.AllKeys.Any(k => k.ToLower().Equals(key.ToLower()));
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Retrieve a header (or querystring) value.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <returns>Value.</returns>
    public string? RetrieveHeaderValue(string key)
    {
        string? headerValue = Headers?.Get(key);
        return !string.IsNullOrEmpty(headerValue) ? headerValue : null;
    }

    /// <summary>
    /// Retrieve a querystring value.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <returns>Value.</returns>
    public string? RetrieveQueryValue(string key)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

        if (Query != null && Query.Elements != null)
        {
            string? val = Query.Elements.Get(key);
            val = !string.IsNullOrEmpty(val) ? WebUtility.UrlDecode(val) : null;
            return val;
        }

        return null;
    }

    // Private-Methods
    private byte[]? AppendBytes(byte[]? orig, byte[]? append)
    {
        byte[]? ret;
        if (orig != null && append == null)
        {
            ret = new byte[orig.Length];
            Buffer.BlockCopy(orig, 0, ret, 0, orig.Length);
            return ret;
        }

        if (orig == null && append != null)
        {
            ret = new byte[append.Length];
            Buffer.BlockCopy(append, 0, ret, 0, append.Length);
            return ret;
        }

        if (orig != null && append != null)
        {
            ret = new byte[orig.Length + append.Length];
            Buffer.BlockCopy(orig, 0, ret, 0, orig.Length);
            Buffer.BlockCopy(append, 0, ret, orig.Length, append.Length);
            return ret;
        }

        return null;
    }

    private byte[]? StreamToBytes(Stream input)
    {
        if (input == null) return null;
        if (!input.CanRead) return null;

        byte[] buffer = new byte[16 * 1024];
        using MemoryStream ms = new();
        int read;

        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            ms.Write(buffer, 0, read);
        }

        return ms.ToArray();
    }

    private void ReadStreamFully()
    {
        if (Data == null) return;
        if (!Data.CanRead) return;

        if (_DataAsBytes == null)
        {
            if (!ChunkedTransfer)
            {
                _DataAsBytes = StreamToBytes(Data);
            }
            else
            {
                while (true)
                {
                    Chunk chunk = ReadChunk().Result;
                    if (chunk.Data != null && chunk.Data.Length > 0) _DataAsBytes = AppendBytes(_DataAsBytes, chunk.Data);
                    if (chunk.IsFinalChunk) break;
                }
            }
        }
    }

    private byte[]? ReadStreamFully(Stream input)
    {
        if (input == null) return null;
        if (!input.CanRead) return null;

        byte[] buffer = new byte[16 * 1024];
        using MemoryStream ms = new();
        int read;

        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            ms.Write(buffer, 0, read);
        }

        byte[] ret = ms.ToArray();
        return ret;
    }

    // Embedded-Classes

    /// <summary>
    /// Source details.
    /// </summary>
    public class SourceDetails
    {
        /// <summary>
        /// IP address of the requestor.
        /// </summary>
        public string? IpAddress { get; set; } = null;

        /// <summary>
        /// TCP port from which the request originated on the requestor.
        /// </summary>
        public int Port { get; set; } = 0;

        /// <summary>
        /// Source details.
        /// </summary>
        public SourceDetails()
        {

        }

        /// <summary>
        /// Source details.
        /// </summary>
        /// <param name="ip">IP address of the requestor.</param>
        /// <param name="port">TCP port from which the request originated on the requestor.</param>
        public SourceDetails(string ip, int port)
        {
            if (string.IsNullOrEmpty(ip)) return;
            if (port < 0) return;

            IpAddress = ip;
            Port = port;
        }
    }

    /// <summary>
    /// Destination details.
    /// </summary>
    public class DestinationDetails
    {
        /// <summary>
        /// IP address to which the request was made.
        /// </summary>
        public string? IpAddress { get; set; } = null;

        /// <summary>
        /// TCP port on which the request was received.
        /// </summary>
        public int Port { get; set; } = 0;

        /// <summary>
        /// Hostname to which the request was directed.
        /// </summary>
        public string? Hostname { get; set; } = null;

        /// <summary>
        /// Hostname elements.
        /// </summary>
        public string[] HostnameElements
        {
            get
            {
                string? hostname = Hostname;
                string[] ret;

                if (!string.IsNullOrEmpty(hostname))
                {
                    if (!IPAddress.TryParse(hostname, out _))
                    {
                        ret = hostname.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                        return ret;
                    }
                    else
                    {
                        ret = new string[1];
                        ret[0] = hostname;
                        return ret;
                    }
                }

                ret = Array.Empty<string>();
                return ret;
            }
        }

        /// <summary>
        /// Destination details.
        /// </summary>
        public DestinationDetails()
        {

        }

        /// <summary>
        /// Source details.
        /// </summary>
        /// <param name="ip">IP address to which the request was made.</param>
        /// <param name="port">TCP port on which the request was received.</param>
        /// <param name="hostname">Hostname.</param>
        public DestinationDetails(string ip, int port, string? hostname)
        {
            if (port < 0) return;

            IpAddress = ip;
            Port = port;
            Hostname = hostname;
        }
    }

    /// <summary>
    /// URL details.
    /// </summary>
    public class UrlDetails
    {
        /// <summary>
        /// Full URL.
        /// </summary>
        public string? Full { get; set; } = null;

        /// <summary>
        /// Raw URL with query.
        /// </summary>
        public string? RawWithQuery { get; set; } = null;

        /// <summary>
        /// Raw URL without query.
        /// </summary>
        public string? RawWithoutQuery
        {
            get
            {
                if (!string.IsNullOrEmpty(RawWithQuery))
                {
                    if (RawWithQuery.Contains('?')) return RawWithQuery[..RawWithQuery.IndexOf("?")];
                    else return RawWithQuery;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Raw URL elements.
        /// </summary>
        public string[] Elements
        {
            get
            {
                string? rawUrl = RawWithoutQuery;

                if (!string.IsNullOrEmpty(rawUrl))
                {
                    while (rawUrl.Contains("//")) rawUrl = rawUrl.Replace("//", "/");
                    while (rawUrl.StartsWith("/")) rawUrl = rawUrl[1..];
                    while (rawUrl.EndsWith("/")) rawUrl = rawUrl[..^1];
                    string[] encoded = rawUrl.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (encoded != null && encoded.Length > 0)
                    {
                        string[] decoded = new string[encoded.Length];
                        for (int i = 0; i < encoded.Length; i++)
                        {
                            decoded[i] = WebUtility.UrlDecode(encoded[i]);
                        }

                        return decoded;
                    }
                }

                string[] ret = Array.Empty<string>();
                return ret;
            }
        }

        /// <summary>
        /// Parameters found within the URL, if using parameter routes.
        /// </summary>
        public NameValueCollection Parameters
        {
            get
            {
                return _Parameters;
            }
            set
            {
                if (value == null) _Parameters = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
                else _Parameters = value;
            }
        }

        /// <summary>
        /// URL details.
        /// </summary>
        public UrlDetails()
        {

        }

        /// <summary>
        /// URL details.
        /// </summary>
        /// <param name="fullUrl">Full URL.</param>
        /// <param name="rawUrl">Raw URL.</param>
        public UrlDetails(string? fullUrl, string? rawUrl)
        {
            Full = fullUrl;
            RawWithQuery = rawUrl;
        }

        private NameValueCollection _Parameters = new(StringComparer.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Query details.
    /// </summary>
    public class QueryDetails
    {
        /// <summary>
        /// Querystring, excluding the leading '?'.
        /// </summary>
        public string? Querystring
        {
            get
            {
                if (!string.IsNullOrEmpty(_FullUrl) && _FullUrl.Contains('?'))
                {
                    return _FullUrl.Substring(_FullUrl.IndexOf("?") + 1, (_FullUrl.Length - _FullUrl.IndexOf("?") - 1));
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Query elements.
        /// </summary>
        public NameValueCollection Elements
        {
            get
            {
                NameValueCollection ret = new(StringComparer.InvariantCultureIgnoreCase);
                string? qs = Querystring;
                if (!string.IsNullOrEmpty(qs))
                {
                    string[] queries = qs.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                    if (queries.Length > 0)
                    {
                        for (int i = 0; i < queries.Length; i++)
                        {
                            string[] queryParts = queries[i].Split('=');
                            if (queryParts != null && queryParts.Length == 2)
                            {
                                ret.Add(queryParts[0], queryParts[1]);
                            }
                            else if (queryParts != null && queryParts.Length == 1)
                            {
                                ret.Add(queryParts[0], null);
                            }
                        }
                    }
                }

                return ret;
            }
        }

        /// <summary>
        /// Query details.
        /// </summary>
        public QueryDetails()
        {

        }

        /// <summary>
        /// Query details.
        /// </summary>
        /// <param name="fullUrl">Full URL.</param>
        public QueryDetails(string fullUrl)
        {
            if (string.IsNullOrEmpty(fullUrl)) return;

            _FullUrl = fullUrl;
        }

        private readonly string? _FullUrl = null;
    }

}