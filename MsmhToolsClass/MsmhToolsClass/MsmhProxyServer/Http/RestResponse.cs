using System;
using System.Collections.Specialized;
using System.Text;

#nullable enable
namespace MsmhToolsClass.MsmhProxyServer;

/// <summary>
/// RESTful response from the server.
/// </summary>
public class RestResponse
{
    /// <summary>
    /// The protocol and version.
    /// </summary>
    public string? ProtocolVersion { get; internal set; } = null;

    private NameValueCollection Headers_ = new(StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// User-supplied headers.
    /// </summary>
    public NameValueCollection Headers
    {
        get
        {
            return Headers_;
        }
        set
        {
            if (value == null) Headers_ = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
            else Headers_ = value;
        }
    }

    /// <summary>
    /// The content encoding returned from the server.
    /// </summary>
    public string? ContentEncoding { get; internal set; } = null;

    /// <summary>
    /// The content type returned from the server.
    /// </summary>
    public string? ContentType { get; internal set; } = null;

    /// <summary>
    /// The number of bytes contained in the response body byte array.
    /// </summary>
    public long ContentLength { get; internal set; } = 0;

    /// <summary>
    /// The response URI of the responder.
    /// </summary>
    public string? ResponseURI { get; internal set; } = null;

    /// <summary>
    /// The HTTP status code returned with the response.
    /// </summary>
    public int StatusCode { get; internal set; } = 0;

    /// <summary>
    /// The HTTP status description associated with the HTTP status code.
    /// </summary>
    public string? StatusDescription { get; internal set; } = null;

    /// <summary>
    /// The stream containing the response data returned from the server.
    /// </summary>
    public Stream? Data { get; internal set; } = null;

    /// <summary>
    /// Read the data stream fully into a byte array.
    /// If you use this property, the 'Data' property will be fully read.
    /// </summary>
    public byte[]? DataAsBytes
    {
        get
        {
            if (_Data == null && ContentLength > 0 && Data != null && Data.CanRead)
            {
                _Data = StreamToBytes(Data);
            }

            return _Data;
        }
    }

    /// <summary>
    /// Read the data stream fully into a string.
    /// If you use this property, the 'Data' property will be fully read.
    /// </summary>
    public string? DataAsString
    {
        get
        {
            if (_Data == null && ContentLength > 0 && Data != null && Data.CanRead)
            {
                _Data = StreamToBytes(Data);
            }

            if (_Data != null)
            {
                return Encoding.UTF8.GetString(_Data);
            }

            return null;
        }
    }

    private byte[]? _Data = null;

    /// <summary>
    /// An organized object containing frequently used response parameters from a RESTful HTTP request.
    /// </summary>
    public RestResponse()
    {

    }

    private byte[] StreamToBytes(Stream input)
    {
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

    /// <summary>
    /// Creates a human-readable string of the object.
    /// </summary>
    /// <returns>String.</returns>
    public override string ToString()
    {
        string ret = "";
        ret += "REST Response" + Environment.NewLine;

        if (Headers != null && Headers.Count > 0)
        {
            ret += "  Headers" + Environment.NewLine;
            for (int i = 0; i < Headers.Count; i++)
            {
                string? key = Headers.GetKey(i);
                string? val = Headers.Get(i);

                if (string.IsNullOrEmpty(key)) continue;
                if (string.IsNullOrEmpty(val)) continue;

                ret += "  | " + key + ": " + val + Environment.NewLine;
            }
        }

        if (!string.IsNullOrEmpty(ContentEncoding))
            ret += "  Content Encoding   : " + ContentEncoding + Environment.NewLine;
        if (!string.IsNullOrEmpty(ContentType))
            ret += "  Content Type       : " + ContentType + Environment.NewLine;
        if (!string.IsNullOrEmpty(ResponseURI))
            ret += "  Response URI       : " + ResponseURI + Environment.NewLine;

        ret += "  Status Code        : " + StatusCode + Environment.NewLine;
        ret += "  Status Description : " + StatusDescription + Environment.NewLine;
        ret += "  Content Length     : " + ContentLength + Environment.NewLine;

        ret += "  Data               : ";
        if (Data != null && ContentLength > 0) ret += "[stream]";
        else ret += "[none]";
        ret += Environment.NewLine;

        return ret;
    }
}