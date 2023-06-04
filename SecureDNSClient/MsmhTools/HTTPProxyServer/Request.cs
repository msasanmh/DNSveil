using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MsmhTools.HTTPProxyServer
{
    /// <summary>
    /// Data extracted from an incoming HTTP request.
    /// </summary>
    public class Request
    {
        /// <summary>
        /// UTC timestamp from when the request was received.
        /// </summary>
        public DateTime TimestampUtc;

        /// <summary>
        /// Thread ID on which the request exists.
        /// </summary>
        public int ThreadId;

        /// <summary>
        /// The protocol and version.
        /// </summary>
        public string? ProtocolVersion;

        /// <summary>
        /// IP address of the requestor (client).
        /// </summary>
        public string? SourceIp;

        /// <summary>
        /// TCP port from which the request originated on the requestor (client).
        /// </summary>
        public int SourcePort;

        /// <summary>
        /// IP address of the recipient (server).
        /// </summary>
        public string? DestIp;

        /// <summary>
        /// TCP port on which the request was received by the recipient (server).
        /// </summary>
        public int DestPort;

        /// <summary>
        /// The destination hostname as found in the request line, if present.
        /// </summary>
        public string? DestHostname;

        /// <summary>
        /// The destination host port as found in the request line, if present.
        /// </summary>
        public int DestHostPort;

        /// <summary>
        /// Specifies whether or not the client requested HTTP keepalives.
        /// </summary>
        public bool Keepalive;

        /// <summary>
        /// The HTTP method used in the request.
        /// </summary>
        public HttpMethod Method;

        /// <summary>
        /// The full URL as sent by the requestor (client).
        /// </summary>
        public string? FullUrl;

        /// <summary>
        /// The raw (relative) URL with the querystring attached.
        /// </summary>
        public string? RawUrlWithQuery;

        /// <summary>
        /// The raw (relative) URL without the querystring attached.
        /// </summary>
        public string? RawUrlWithoutQuery;

        /// <summary>
        /// List of items found in the raw URL.
        /// </summary>
        public List<string>? RawUrlEntries;

        /// <summary>
        /// The querystring attached to the URL.
        /// </summary>
        public string? Querystring;

        /// <summary>
        /// Dictionary containing key-value pairs from items found in the querystring.
        /// </summary>
        public Dictionary<string, string>? QuerystringEntries;

        /// <summary>
        /// The useragent specified in the request.
        /// </summary>
        public string? Useragent;

        /// <summary>
        /// The number of bytes in the request body.
        /// </summary>
        public long ContentLength;

        /// <summary>
        /// The content type as specified by the requestor (client).
        /// </summary>
        public string? ContentType;

        /// <summary>
        /// The headers found in the request.
        /// </summary>
        public Dictionary<string, string>? Headers;

        /// <summary>
        /// The request body as sent by the requestor (client).
        /// </summary>
        public byte[]? Data;

        /// <summary>
        /// The stream from which to read the request body sent by the requestor (client).
        /// </summary>
        public Stream? DataStream;

        /// <summary>
        /// The original HttpListenerContext from which the HttpRequest was constructed.
        /// </summary>
        public HttpListenerContext? ListenerContext;

        private Uri? _Uri;
        private static int _TimeoutDataReadMs = 2000;
        private static int _DataReadSleepMs = 10;

        private ChunkDecoder _Decoder = new();

        /// <summary>
        /// Construct a new HTTP request.
        /// </summary>
        public Request()
        {
            ThreadId = Environment.CurrentManagedThreadId;
            TimestampUtc = DateTime.Now.ToUniversalTime();
            QuerystringEntries = new Dictionary<string, string>();
            Headers = new Dictionary<string, string>();
        }

        /// <summary>
        /// Construct a new HTTP request from a given HttpListenerContext.
        /// </summary>
        /// <param name="ctx">The HttpListenerContext for the request.</param>
        /// <param name="readStreamFully">Indicate whether or not the input stream should be read and converted to a byte array.</param>
        public Request(HttpListenerContext ctx, bool readStreamFully)
        {
            // Check-for-Null-Values
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (ctx.Request == null) throw new ArgumentNullException(nameof(ctx));

            // Parse-Variables
            int position = 0;
            int inQuery = 0;
            string tempString = "";
            string queryString = "";

            int inKey = 0;
            int inVal = 0;
            string tempKey = "";
            string tempVal = "";

            // Standard-Request-Items
            ThreadId = Environment.CurrentManagedThreadId;
            TimestampUtc = DateTime.Now.ToUniversalTime();
            ProtocolVersion = "HTTP/" + ctx.Request.ProtocolVersion.ToString();
            SourceIp = ctx.Request.RemoteEndPoint.Address.ToString();
            SourcePort = ctx.Request.RemoteEndPoint.Port;
            DestIp = ctx.Request.LocalEndPoint.Address.ToString();
            DestPort = ctx.Request.LocalEndPoint.Port;
            Method = (HttpMethod)Enum.Parse(typeof(HttpMethod), ctx.Request.HttpMethod, true);
            FullUrl = ctx.Request.Url != null ? new string(ctx.Request.Url.ToString().Trim()) : null;
            RawUrlWithQuery = ctx.Request.RawUrl != null ? new string(ctx.Request.RawUrl.ToString().Trim()) : null;
            RawUrlWithoutQuery = ctx.Request.RawUrl != null ? new string(ctx.Request.RawUrl.ToString().Trim()) : null;
            Keepalive = ctx.Request.KeepAlive;
            ContentLength = ctx.Request.ContentLength64;
            Useragent = ctx.Request.UserAgent;
            ContentType = ctx.Request.ContentType;
            ListenerContext = ctx;

            RawUrlEntries = new List<string>();
            QuerystringEntries = new Dictionary<string, string>();
            Headers = new Dictionary<string, string>();

            // Raw-URL-and-Querystring
            if (!string.IsNullOrEmpty(RawUrlWithoutQuery))
            {
                // Initialize-Variables
                RawUrlEntries = new List<string>();
                QuerystringEntries = new Dictionary<string, string>();

                // Process-Raw-URL-and-Populate-Raw-URL-Elements
                while (RawUrlWithoutQuery.Contains("//"))
                {
                    RawUrlWithoutQuery = RawUrlWithoutQuery.Replace("//", "/");
                }

                foreach (char c in RawUrlWithoutQuery)
                {
                    if (inQuery == 1)
                    {
                        queryString += c;
                        continue;
                    }

                    if ((position == 0) &&
                        (string.Compare(tempString, "") == 0) &&
                        (c == '/'))
                    {
                        // skip the first slash
                        continue;
                    }

                    if ((c != '/') && (c != '?'))
                    {
                        tempString += c;
                    }

                    if ((c == '/') || (c == '?'))
                    {
                        if (!string.IsNullOrEmpty(tempString))
                        {
                            // add to raw URL entries list
                            RawUrlEntries.Add(tempString);
                        }

                        position++;
                        tempString = "";
                    }

                    if (c == '?')
                    {
                        inQuery = 1;
                    }
                }

                if (!string.IsNullOrEmpty(tempString))
                {
                    // add to raw URL entries list
                    RawUrlEntries.Add(tempString);
                }

                // Populate-Querystring
                if (queryString.Length > 0) Querystring = queryString;
                else Querystring = null;

                // Parse-Querystring
                if (!string.IsNullOrEmpty(Querystring))
                {
                    inKey = 1;
                    inVal = 0;
                    position = 0;
                    tempKey = "";
                    tempVal = "";

                    foreach (char c in Querystring)
                    {
                        if (inKey == 1)
                        {
                            if (c == '&')
                            {
                                // key with no value
                                if (!string.IsNullOrEmpty(tempKey))
                                {
                                    inKey = 1;
                                    inVal = 0;

                                    tempKey = WebUtility.UrlDecode(tempKey);
                                    QuerystringEntries = CommonTools.AddToDict(tempKey, null, QuerystringEntries);

                                    tempKey = "";
                                    tempVal = "";
                                    position++;
                                    continue;
                                }
                            }
                            else if (c != '=')
                            {
                                tempKey += c;
                            }
                            else
                            {
                                inKey = 0;
                                inVal = 1;
                                continue;
                            }
                        }

                        if (inVal == 1)
                        {
                            if (c != '&')
                            {
                                tempVal += c;
                            }
                            else
                            {
                                inKey = 1;
                                inVal = 0;

                                tempKey = WebUtility.UrlDecode(tempKey);
                                if (!string.IsNullOrEmpty(tempVal)) tempVal = WebUtility.UrlDecode(tempVal);
                                QuerystringEntries = CommonTools.AddToDict(tempKey, tempVal, QuerystringEntries);

                                tempKey = "";
                                tempVal = "";
                                position++;
                                continue;
                            }
                        }
                    }

                    if (inVal == 0)
                    {
                        // val will be null
                        if (!string.IsNullOrEmpty(tempKey))
                        {
                            tempKey = WebUtility.UrlDecode(tempKey);
                            QuerystringEntries = CommonTools.AddToDict(tempKey, null, QuerystringEntries);
                        }
                    }

                    if (inVal == 1)
                    {
                        if (!string.IsNullOrEmpty(tempKey))
                        {
                            tempKey = WebUtility.UrlDecode(tempKey);
                            if (!string.IsNullOrEmpty(tempVal)) tempVal = WebUtility.UrlDecode(tempVal);
                            QuerystringEntries = CommonTools.AddToDict(tempKey, tempVal, QuerystringEntries);
                        }
                    }
                }
            }

            // Remove-Querystring-from-Raw-URL
            if (RawUrlWithoutQuery != null && RawUrlWithoutQuery.Contains('?'))
            {
                RawUrlWithoutQuery = RawUrlWithoutQuery[..RawUrlWithoutQuery.IndexOf("?")];
            }

            // Check-for-Full-URL
            try
            {
                if (FullUrl != null)
                {
                    _Uri = new Uri(FullUrl);
                    DestHostname = _Uri.Host;
                    DestHostPort = _Uri.Port;
                }
            }
            catch (Exception)
            {
                // do nothing
            }

            // Headers
            Headers = new Dictionary<string, string>();
            for (int i = 0; i < ctx.Request.Headers.Count; i++)
            {
                string key = new(ctx.Request.Headers.GetKey(i));
                string val = new(ctx.Request.Headers.Get(i));
                Headers = CommonTools.AddToDict(key, val, Headers);
            }

            // Payload
            bool chunkedXfer = false;
            bool gzip = false;
            bool deflate = false;
            string? xferEncodingHeader = RetrieveHeaderValue("Transfer-Encoding");
            if (!string.IsNullOrEmpty(xferEncodingHeader))
            {
                chunkedXfer = xferEncodingHeader.ToLower().Contains("chunked");
                gzip = xferEncodingHeader.ToLower().Contains("gzip");
                deflate = xferEncodingHeader.ToLower().Contains("deflate");
            }

            if (chunkedXfer
                && Method != HttpMethod.GET
                && Method != HttpMethod.HEAD)
            {
                Stream bodyStream = ctx.Request.InputStream;

                if (!readStreamFully)
                {
                    MemoryStream ms = new();

                    if (!_Decoder.Decode(bodyStream, out long contentLength, out ms))
                    {
                        throw new IOException("Unable to decode chunk-encoded stream");
                    }

                    ContentLength = contentLength;
                    DataStream = ms;
                }
                else
                {
                    byte[] encodedData = CommonTools.StreamToBytes(bodyStream);

                    if (!_Decoder.Decode(encodedData, out byte[]? decodedData))
                    {
                        throw new IOException("Unable to decode chunk-encoded stream");
                    }

                    if (decodedData != null)
                    {
                        ContentLength = decodedData.Length;
                        Data = new byte[ContentLength];
                        Buffer.BlockCopy(decodedData, 0, Data, 0, decodedData.Length);
                    }
                }
            }
            else if (ContentLength > 0)
            {
                if (readStreamFully)
                {
                    if (Method != HttpMethod.GET && Method != HttpMethod.HEAD)
                    {
                        try
                        {
                            Data = new byte[ContentLength];
                            Stream bodyStream = ctx.Request.InputStream;
                            Data = CommonTools.StreamToBytes(bodyStream);
                        }
                        catch (Exception)
                        {
                            Data = null;
                        }
                    }
                }
                else
                {
                    Data = null;
                    DataStream = ctx.Request.InputStream;
                }
            }
        }

        /// <summary>
        /// Create an HttpRequest object from a TcpClient.
        /// </summary>
        /// <param name="client">TcpClient.</param>
        /// <returns>A populated HttpRequest.</returns>
        public static Request? FromTcpClient(TcpClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            try
            {
                Request ret;
                byte[]? headerBytes = null;
                byte[] lastFourBytes = new byte[4];
                lastFourBytes[0] = 0x00;
                lastFourBytes[1] = 0x00;
                lastFourBytes[2] = 0x00;
                lastFourBytes[3] = 0x00;

                // Attach-Stream
                NetworkStream stream = client.GetStream();

                if (!stream.CanRead)
                {
                    throw new IOException("Unable to read from stream.");
                }

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
                            if (headerBytes == null) headerBytes = new byte[1];

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
                    //throw new IOException("No header data read from the stream.");
                    return null;
                }

                ret = BuildHeaders(headerBytes);

                // Read-Data
                ret.Data = null;
                if (ret.ContentLength > 0)
                {
                    // Read-from-Stream
                    ret.Data = new byte[ret.ContentLength];

                    using (MemoryStream dataMs = new())
                    {
                        long bytesRemaining = ret.ContentLength;
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
                                if (bytesRead == ret.ContentLength) break;
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
                                    Thread.Sleep(_DataReadSleepMs);
                                }
                            }
                        }

                        if (timeout)
                        {
                            throw new IOException("Timeout reading data from stream.");
                        }

                        ret.Data = dataMs.ToArray();
                    }

                    // Validate-Data
                    if (ret.Data == null || ret.Data.Length < 1)
                    {
                        throw new IOException("Unable to read data from stream.");
                    }

                    if (ret.Data.Length != ret.ContentLength)
                    {
                        throw new IOException("Data read does not match specified content length.");
                    }
                }
                else
                {
                    // do nothing
                }

                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Create an HttpRequest object from a byte array.
        /// </summary>
        /// <param name="bytes">Byte data.</param>
        /// <returns>A populated HttpRequest.</returns>
        public static Request FromBytes(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length < 4) throw new ArgumentException("Too few bytes supplied to form a valid HTTP request.");

            bool endOfHeader = false;
            byte[] headerBytes = new byte[1];

            Request ret = new();

            for (int i = 0; i < bytes.Length; i++)
            {
                if (headerBytes.Length == 1)
                {
                    // First-Byte
                    headerBytes[0] = bytes[i];
                    continue;
                }

                if (!endOfHeader && headerBytes.Length < 4)
                {
                    // Fewer-Than-Four-Bytes
                    byte[] tempHeader = new byte[i + 1];
                    Buffer.BlockCopy(headerBytes, 0, tempHeader, 0, headerBytes.Length);
                    tempHeader[i] = bytes[i];
                    headerBytes = tempHeader;
                    continue;
                }

                if (!endOfHeader)
                {
                    // Check-for-End-of-Header
                    // check if end of headers reached
                    if (
                        (int)headerBytes[(^1)] == 10
                        && (int)headerBytes[(^2)] == 13
                        && (int)headerBytes[(^3)] == 10
                        && (int)headerBytes[(^4)] == 13
                        )
                    {
                        // End-of-Header
                        // end of headers reached
                        endOfHeader = true;
                        ret = BuildHeaders(headerBytes);
                    }
                    else
                    {
                        // Still-Reading-Header
                        byte[] tempHeader = new byte[i + 1];
                        Buffer.BlockCopy(headerBytes, 0, tempHeader, 0, headerBytes.Length);
                        tempHeader[i] = bytes[i];
                        headerBytes = tempHeader;
                        continue;
                    }
                }
                else
                {
                    if (ret.ContentLength > 0)
                    {
                        // Append-Data
                        if (ret.ContentLength != (bytes.Length - i))
                        {
                            throw new ArgumentException("Content-Length header does not match the number of data bytes.");
                        }

                        ret.Data = new byte[ret.ContentLength];
                        Buffer.BlockCopy(bytes, i, ret.Data, 0, (int)ret.ContentLength);
                        break;
                    }
                    else
                    {
                        // No-Data
                        ret.Data = null;
                        break;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Create an HttpRequest object from a Stream.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <returns>A populated HttpRequest.</returns>
        public static Request FromStream(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            try
            {
                Request ret;
                byte[]? headerBytes = null;
                byte[] lastFourBytes = new byte[4];
                lastFourBytes[0] = 0x00;
                lastFourBytes[1] = 0x00;
                lastFourBytes[2] = 0x00;
                lastFourBytes[3] = 0x00;

                // Check-Stream
                if (!stream.CanRead)
                {
                    throw new IOException("Unable to read from stream.");
                }

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
                            if (headerBytes == null) headerBytes = new byte[1];

                            //Update-Last-Four
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
                if (headerBytes == null || headerBytes.Length < 1) throw new IOException("No header data read from the stream.");
                ret = BuildHeaders(headerBytes);

                // Read-Data
                ret.Data = null;
                if (ret.ContentLength > 0)
                {
                    // Read-from-Stream
                    ret.Data = new byte[ret.ContentLength];

                    using (MemoryStream dataMs = new())
                    {
                        long bytesRemaining = ret.ContentLength;
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
                                if (bytesRead == ret.ContentLength) break;
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
                                    Thread.Sleep(_DataReadSleepMs);
                                }
                            }
                        }

                        if (timeout)
                        {
                            throw new IOException("Timeout reading data from stream.");
                        }

                        ret.Data = dataMs.ToArray();
                    }

                    // Validate-Data
                    if (ret.Data == null || ret.Data.Length < 1)
                    {
                        throw new IOException("Unable to read data from stream.");
                    }

                    if (ret.Data.Length != ret.ContentLength)
                    {
                        throw new IOException("Data read does not match specified content length.");
                    }
                }
                else
                {
                    // do nothing
                }

                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Create an HttpRequest object from a NetworkStream.
        /// </summary>
        /// <param name="stream">NetworkStream.</param>
        /// <returns>A populated HttpRequest.</returns>
        public static Request FromStream(NetworkStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            try
            {
                Request ret;
                byte[]? headerBytes = null;
                byte[] lastFourBytes = new byte[4];
                lastFourBytes[0] = 0x00;
                lastFourBytes[1] = 0x00;
                lastFourBytes[2] = 0x00;
                lastFourBytes[3] = 0x00;

                // Check-Stream
                if (!stream.CanRead)
                {
                    throw new IOException("Unable to read from stream.");
                }

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
                            if (headerBytes == null) headerBytes = new byte[1];

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
                if (headerBytes == null || headerBytes.Length < 1) throw new IOException("No header data read from the stream.");
                ret = BuildHeaders(headerBytes);

                // Read-Data
                ret.Data = null;
                if (ret.ContentLength > 0)
                {
                    // Read-from-Stream
                    ret.Data = new byte[ret.ContentLength];

                    using (MemoryStream dataMs = new())
                    {
                        long bytesRemaining = ret.ContentLength;
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
                                if (bytesRead == ret.ContentLength) break;
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
                                    Thread.Sleep(_DataReadSleepMs);
                                }
                            }
                        }

                        if (timeout)
                        {
                            throw new IOException("Timeout reading data from stream.");
                        }

                        ret.Data = dataMs.ToArray();
                    }

                    // Validate-Data
                    if (ret.Data == null || ret.Data.Length < 1)
                    {
                        throw new IOException("Unable to read data from stream.");
                    }

                    if (ret.Data.Length != ret.ContentLength)
                    {
                        throw new IOException("Data read does not match specified content length.");
                    }
                }
                else
                {
                    // do nothing
                }

                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //==================================================== Methods Public

        /// <summary>
        /// Retrieve a specified header value from either the headers or the querystring.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string? RetrieveHeaderValue(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (Headers != null && Headers.Count > 0)
            {
                foreach (KeyValuePair<string, string> curr in Headers)
                {
                    if (string.IsNullOrEmpty(curr.Key)) continue;
                    if (string.Compare(curr.Key.ToLower(), key.ToLower()) == 0) return curr.Value;
                }
            }

            if (QuerystringEntries != null && QuerystringEntries.Count > 0)
            {
                foreach (KeyValuePair<string, string> curr in QuerystringEntries)
                {
                    if (string.IsNullOrEmpty(curr.Key)) continue;
                    if (string.Compare(curr.Key.ToLower(), key.ToLower()) == 0) return curr.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieve the integer value of the last raw URL element, if found.
        /// </summary>
        /// <returns>A nullable integer.</returns>
        public int? RetrieveIdValue()
        {
            if (RawUrlEntries == null || RawUrlEntries.Count < 1) return null;
            string[] entries = RawUrlEntries.ToArray();
            int len = entries.Length;
            string entry = entries[(len - 1)];
            if (int.TryParse(entry, out int ret))
            {
                return ret;
            }
            return null;
        }

        //==================================================== Methods Private

        private static Request BuildHeaders(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            // Initial-Values
            Request ret = new();
            ret.TimestampUtc = DateTime.Now.ToUniversalTime();
            ret.ThreadId = Environment.CurrentManagedThreadId;
            ret.SourceIp = "unknown";
            ret.SourcePort = 0;
            ret.DestIp = "unknown";
            ret.DestPort = 0;
            ret.Headers = new Dictionary<string, string>();

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

                    // (HttpMethod)Enum.Parse(typeof(HttpMethod), requestLine[0], true);
                    ret.Method = GetHttpMethod.Parse(requestLine[0]);
                    ret.FullUrl = requestLine[1];
                    ret.ProtocolVersion = requestLine[2];
                    ret.RawUrlWithQuery = ret.FullUrl;
                    ret.RawUrlWithoutQuery = ExtractRawUrlWithoutQuery(ret.RawUrlWithQuery);
                    ret.RawUrlEntries = ExtractRawUrlEntries(ret.RawUrlWithoutQuery);
                    ret.Querystring = ExtractQuerystring(ret.RawUrlWithQuery);
                    ret.QuerystringEntries = ExtractQuerystringEntries(ret.Querystring);

                    try
                    {
                        Uri uri = new(ret.FullUrl);
                        ret.DestHostname = uri.Host;
                        ret.DestHostPort = uri.Port;
                    }
                    catch (Exception)
                    {
                    }

                    if (string.IsNullOrEmpty(ret.DestHostname))
                    {
                        if (!ret.FullUrl.Contains("://") & ret.FullUrl.Contains(':'))
                        {
                            string[] hostAndPort = ret.FullUrl.Split(':');
                            if (hostAndPort.Length == 2)
                            {
                                ret.DestHostname = hostAndPort[0];
                                if (!int.TryParse(hostAndPort[1], out ret.DestHostPort))
                                {
                                    throw new Exception("Unable to parse destination hostname and port.");
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

        private static string? ExtractRawUrlWithoutQuery(string rawUrlWithQuery)
        {
            if (string.IsNullOrEmpty(rawUrlWithQuery)) return null;
            if (!rawUrlWithQuery.Contains('?')) return rawUrlWithQuery;
            return rawUrlWithQuery[..rawUrlWithQuery.IndexOf("?")];
        }

        private static List<string>? ExtractRawUrlEntries(string? rawUrlWithoutQuery)
        {
            if (string.IsNullOrEmpty(rawUrlWithoutQuery)) return null;

            int position = 0;
            string tempString = "";
            List<string> ret = new();

            foreach (char c in rawUrlWithoutQuery)
            {
                if ((position == 0) &&
                    (string.Compare(tempString, "") == 0) &&
                    (c == '/'))
                {
                    // skip the first slash
                    continue;
                }

                if ((c != '/') && (c != '?'))
                {
                    tempString += c;
                }

                if ((c == '/') || (c == '?'))
                {
                    if (!string.IsNullOrEmpty(tempString))
                    {
                        // add to raw URL entries list
                        ret.Add(tempString);
                    }

                    position++;
                    tempString = "";
                }
            }

            if (!string.IsNullOrEmpty(tempString))
            {
                // add to raw URL entries list
                ret.Add(tempString);
            }

            return ret;
        }

        private static string? ExtractQuerystring(string rawUrlWithQuery)
        {
            if (string.IsNullOrEmpty(rawUrlWithQuery)) return null;
            if (!rawUrlWithQuery.Contains('?')) return null;

            int qsStartPos = rawUrlWithQuery.IndexOf("?");
            if (qsStartPos >= (rawUrlWithQuery.Length - 1)) return null;
            return rawUrlWithQuery.Substring(qsStartPos + 1);
        }

        private static Dictionary<string, string>? ExtractQuerystringEntries(string? query)
        {
            if (string.IsNullOrEmpty(query)) return null;

            Dictionary<string, string> ret = new();

            int inKey = 1;
            int inVal = 0;
            int position = 0;
            string tempKey = "";
            string tempVal = "";

            for (int i = 0; i < query.Length; i++)
            {
                char c = query[i];
                if (inKey == 1)
                {
                    if (c != '=')
                    {
                        tempKey += c;
                    }
                    else
                    {
                        inKey = 0;
                        inVal = 1;
                        continue;
                    }
                }

                if (inVal == 1)
                {
                    if (c != '&')
                    {
                        tempVal += c;
                    }
                    else
                    {
                        inKey = 1;
                        inVal = 0;

                        if (!string.IsNullOrEmpty(tempVal)) tempVal = WebUtility.UrlEncode(tempVal);
                        ret = CommonTools.AddToDict(tempKey, tempVal, ret);

                        tempKey = "";
                        tempVal = "";
                        position++;
                        continue;
                    }
                }

                if (inVal == 1)
                {
                    if (!string.IsNullOrEmpty(tempVal)) tempVal = WebUtility.UrlEncode(tempVal);
                    ret = CommonTools.AddToDict(tempKey, tempVal, ret);
                }
            }

            return ret;
        }
    }
}
