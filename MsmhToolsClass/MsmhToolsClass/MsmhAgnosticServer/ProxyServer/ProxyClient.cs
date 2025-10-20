using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class ProxyClient
{
    public Socket Socket_ { get; set; }
    public SslStream? SslStream_ { get; set; }
    private bool Disposed_ { get; set; } = false;

    public ProxyClient(Socket socket, SslStream? sslStream)
    {
        // Start Data Exchange.
        Socket_ = socket;
        Socket_.ReceiveBufferSize = MsmhAgnosticServer.MaxDataSize;
        SslStream_ = sslStream;
    }

    public async Task<int> ReceiveAsync(byte[] buffer, SocketFlags socketFlags = SocketFlags.None)
    {
        try
        {
            if (Socket_.Connected)
            {
                int received = await Socket_.ReceiveAsync(buffer, socketFlags).ConfigureAwait(false);
                if (received <= 0)
                {
                    Disconnect();
                    return 0;
                }
                return received;
            }
            return 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProxyClient ReceiveAsync: " + ex.ToString());
            Disconnect();
            return 0;
        }
    }

    public async Task<bool> SendAsync(byte[] buffer)
    {
        try
        {
            if (Socket_.Connected)
            {
                int sent = await Socket_.SendAsync(buffer, SocketFlags.None).ConfigureAwait(false);

                if (sent <= 0)
                {
                    Disconnect();
                    return false;
                }

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProxyClient SendAsync: " + ex.ToString());
            Disconnect();
            return false;
        }
    }

    public void Disconnect(TcpClient? tcpClient = null)
    {
        try
        {
            if (!Disposed_)
            {
                if (Socket_ != null && Socket_.Connected)
                {
                    Disposed_ = true;
                    Socket_.Close();
                    Socket_.Dispose();
                    tcpClient?.Close();
                    tcpClient?.Dispose();
                    return;
                }
            }
        }
        catch(Exception) { }
    }

}