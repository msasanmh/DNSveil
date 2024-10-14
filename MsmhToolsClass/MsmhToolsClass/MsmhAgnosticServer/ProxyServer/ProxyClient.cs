using System.Diagnostics;
using System.Net.Sockets;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class ProxyClient
{
    private bool Disposed_ { get; set; } = false;

    public Socket Socket_ { get; set; }
    public event EventHandler<DataEventArgs>? OnDataReceived;
    public event EventHandler<DataEventArgs>? OnDataSent;

    public ProxyClient(Socket socket)
    {
        // Start Data Exchange.
        Socket_ = socket;
        Socket_.ReceiveBufferSize = MsmhAgnosticServer.MaxDataSize;
    }

    public async Task StartReceiveAsync()
    {
        try
        {
            if (Disposed_ || Socket_ is null) return;

            byte[] buffer = new byte[MsmhAgnosticServer.MaxDataSize];
            int received = 0;
            try { received = await Socket_.ReceiveAsync(buffer, SocketFlags.None).ConfigureAwait(false); } catch (Exception) { /* HSTS / Timeout / Done */ }
            
            if (received <= 0)
            {
                Disconnect();
                return;
            }
            
            buffer = buffer[..received];

            DataEventArgs data = new(this, buffer);
            OnDataReceived?.Invoke(this, data);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            Disconnect();
        }
    }

    public async Task<int> ReceiveAsync(byte[] data, SocketFlags socketFlags = SocketFlags.None)
    {
        try
        {
            int received = await Socket_.ReceiveAsync(data, socketFlags).ConfigureAwait(false);
            
            if (received <= 0)
            {
                Disconnect();
                return -1;
            }

            return received;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProxyClient ReceiveAsync: " + ex.ToString());
            Disconnect();
            return -1;
        }
    }

    public async Task<bool> SendAsync(byte[] buffer)
    {
        try
        {
            if (Socket_ != null && Socket_.Connected)
            {
                int sent = await Socket_.SendAsync(buffer, SocketFlags.None).ConfigureAwait(false);

                if (sent <= 0)
                {
                    Disconnect();
                    return false;
                }

                DataEventArgs data = new(this, buffer);
                OnDataSent?.Invoke(this, data);
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
                    Socket_.Shutdown(SocketShutdown.Both);
                    Socket_.Close();
                    tcpClient?.Dispose();
                    return;
                }
            }
        }
        catch(Exception) { }
    }

}