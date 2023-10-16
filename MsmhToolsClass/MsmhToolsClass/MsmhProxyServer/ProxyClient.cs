using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace MsmhToolsClass.MsmhProxyServer;

public class ProxyClient
{
    private byte[] Buffer_ { get; set; }
    private bool Disposed_ { get; set; } = false;

    public Socket Socket_ { get; set; }
    public event EventHandler<DataEventArgs>? OnDataReceived;
    public event EventHandler<DataEventArgs>? OnDataSent;

    public ProxyClient(Socket socket)
    {
        // Start Data Exchange.
        Socket_ = socket;
        int packetSize = MsmhProxyServer.MaxDataSize;
        Buffer_ = new byte[packetSize];
        Socket_.ReceiveBufferSize = packetSize;
    }

    public async Task StartReceiveAsync()
    {
        try
        {
            if (Disposed_ || Socket_ is null) return;

            int received = 0;
            try { received = await Socket_.ReceiveAsync(Buffer_, SocketFlags.None); } catch (Exception) { /* HSTS / Timeout / Done */ }
            
            if (received <= 0)
            {
                Disconnect();
                return;
            }
            
            byte[] buffer = new byte[received];
            Buffer.BlockCopy(Buffer_, 0, buffer, 0, received);

            DataEventArgs data = new(this, buffer);
            OnDataReceived?.Invoke(this, data);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            Disconnect();
        }
    }

    public async Task<int> ReceiveAsync(byte[] data)
    {
        try
        {
            int received = await Socket_.ReceiveAsync(data, SocketFlags.None);
            
            if (received <= 0)
            {
                Disconnect();
                return -1;
            }

            return received;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
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
                int sent = await Socket_.SendAsync(buffer, SocketFlags.None);

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
            Debug.WriteLine(ex.Message);
            Disconnect();
            return false;
        }
    }

    public void Disconnect()
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
                    return;
                }
            }
        }
        catch(Exception)
        {
            // do nothing
        }
    }

}