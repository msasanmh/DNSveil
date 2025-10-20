using System.Diagnostics;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class TcpOverUdpPlainClient
{
    private byte[] QueryBuffer { get; set; } = Array.Empty<byte>();
    private DnsReader Reader { get; set; } = new();
    private int TimeoutMS { get; set; } = 5;
    private CancellationToken CT { get; set; }

    public TcpOverUdpPlainClient(byte[] queryBuffer, DnsReader reader, int timeoutMS, CancellationToken ct = default)
    {
        QueryBuffer = queryBuffer;
        Reader = reader;
        TimeoutMS = timeoutMS;
        CT = ct;
    }

    public async Task<byte[]> GetResponseAsync()
    {
        byte[] result = Array.Empty<byte>();

        try
        {
            bool isTcp = QueryBuffer.Length > 512;
            if (isTcp)
            {
                // Send Over TCP
                result = await SendOverTcpAsync().ConfigureAwait(false);
                if (result.Length > 0)
                {
                    return result;
                }
                else
                {
                    // Server May Support EDNS0 So Try UDP
                    result = await SendOverUdpAsync().ConfigureAwait(false);
                }
            }
            else
            {
                // Send Over UDP
                result = await SendOverUdpAsync().ConfigureAwait(false);
                DnsMessage dmOut = DnsMessage.Read(result, DnsEnums.DnsProtocol.UDP);
                if (dmOut.IsSuccess)
                {
                    if (dmOut.Header.TC == DnsEnums.TC.Truncated)
                    {
                        // Response Is Truncated We Should Retry Over TCP To Get Full Message
                        byte[] tcpResult = await SendOverTcpAsync().ConfigureAwait(false);
                        if (tcpResult.Length > 0) result = tcpResult;
                    }
                }
                else
                {
                    // UDP Failed Try TCP
                    result = await SendOverTcpAsync().ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("TcpOverUdpPlainClient: " + ex.GetInnerExceptions());
        }

        return result;
    }

    private static byte[] AddTcpMessageLength(byte[] buffer)
    {
        try
        {
            ushort tcpMessageLength = Convert.ToUInt16(buffer.Length);
            ByteArrayTool.TryConvertUInt16ToBytes(tcpMessageLength, out byte[] tcpMessageLengthBytes);
            return tcpMessageLengthBytes.Concat(buffer).ToArray();
        }
        catch (Exception)
        {
            return buffer;
        }
    }

    private async Task<byte[]> SendOverTcpAsync()
    {
        byte[] tcpQuery = AddTcpMessageLength(QueryBuffer);
        TcpPlainClient tcpPlainClient = new(tcpQuery, Reader, TimeoutMS, null, null, null, CT);
        return await tcpPlainClient.GetResponseAsync().ConfigureAwait(false);
    }

    private async Task<byte[]> SendOverUdpAsync()
    {
        UdpPlainClient udpPlainClient = new(QueryBuffer, Reader, TimeoutMS, CT);
        return await udpPlainClient.GetResponseAsync().ConfigureAwait(false);
    }
}