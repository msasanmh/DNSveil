using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class DnsRequest
{
    public Socket? Socket_ { get; set; }
    public SslStream? Ssl_Stream { get; set; }
    public SslKind Ssl_Kind { get; set; } = SslKind.NonSSL;
    public EndPoint? LocalEndPoint { get; set; }
    public EndPoint? RemoteEndPoint { get; set; }
    public byte[] Buffer { get; set; } = Array.Empty<byte>();
    public DnsEnums.DnsProtocol Protocol { get; set; }

    public DnsRequest() { }

    public DnsRequest(Socket? socket, SslStream? sslStream, SslKind sslKind, EndPoint? localEndPoint, EndPoint? remoteEndPoint, byte[] buffer, DnsEnums.DnsProtocol protocol)
    {
        Socket_ = socket;
        Ssl_Stream = sslStream;
        Ssl_Kind = sslKind;
        LocalEndPoint = localEndPoint;
        RemoteEndPoint = remoteEndPoint;
        Buffer = buffer;
        Protocol = protocol;
    }

    public async Task SendToAsync(byte[] aBuffer)
    {
        try
        {
            if (Buffer.Length == 0 || LocalEndPoint == null || RemoteEndPoint == null)
            {
                Disconnect();
                return;
            }

            if (Ssl_Kind == SslKind.NonSSL && Socket_ != null)
                await Socket_.SendToAsync(aBuffer, SocketFlags.None, RemoteEndPoint);

            if (Ssl_Kind == SslKind.SSL && Ssl_Stream != null && Protocol == DnsEnums.DnsProtocol.DoH)
            {
                bool isDohWriteSuccess = DnsMessage.TryWriteDoHResponse(aBuffer, out byte[] result);
                if (isDohWriteSuccess) await Ssl_Stream.WriteAsync(result);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS DnsRequest SendToAsync: " + ex.Message);
        }
    }

    public async Task SendFailedResponseAsync()
    {
        try
        {
            if (Socket_ == null || Buffer.Length == 0 || LocalEndPoint == null || RemoteEndPoint == null)
            {
                Disconnect();
                return;
            }
            DnsMessage dm = DnsMessage.Read(Buffer, Protocol);
            dm = DnsMessage.CreateFailedResponse(dm);
            DnsMessage.TryWrite(dm, out byte[] failedBuffer);
            await SendToAsync(failedBuffer);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS ProxyRequest SendFailedResponseAsync: " + ex.Message);
        }
    }

    public void Disconnect()
    {
        if (Protocol != DnsEnums.DnsProtocol.UDP)
        {
            try
            {
                Socket_?.Shutdown(SocketShutdown.Both);
                Socket_?.Dispose();
            }
            catch (Exception) { }
        }

        try { Ssl_Stream?.Dispose(); } catch (Exception) { }
    }
}
