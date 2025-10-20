using System.Diagnostics;
using System.Net.Sockets;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class ProxyRelay
{
    private Socket ClientSocket { get; set; }
    private Socket RemoteSocket { get; set; }

    public event EventHandler<ProxyRelayEventArgs>? OnClientDataReceived;
    public event EventHandler<ProxyRelayEventArgs>? OnClientDataSent;
    public event EventHandler<ProxyRelayEventArgs>? OnRemoteDataReceived;
    public event EventHandler<ProxyRelayEventArgs>? OnRemoteDataSent;

    private readonly ProxyTunnel? ProxyTunnel_ = null;
    private ProxyRequest Request { get; set; }
    private AgnosticProgram.Fragment FP { get; set; }
    private readonly Stopwatch KillOnTimeout = new();
    private bool Disposed_ { get; set; } = false;

    internal ProxyRelay(ProxyTunnel proxyTunnel)
    {
        ProxyTunnel_ = proxyTunnel;
        Request = ProxyTunnel_.Req;
        FP = ProxyTunnel_.FragmentProgram;
        KillOnTimeout = ProxyTunnel_.KillOnTimeout;

        ClientSocket = ProxyTunnel_.Client.Socket_;
        RemoteSocket = ProxyTunnel_.Remote.Socket_;
    }

    public async Task ExecuteAsync()
    {
        // Start Data Exchange.
        try
        {
            Task c = ReadClientAsync();
            Task r = ReadRemoteAsync();
            await Task.WhenAll(c, r).ConfigureAwait(false);
            Disconnect();
        }
        catch (Exception)
        {
            Disconnect();
        }
    }

    private async Task ReadClientAsync()
    {
        await Task.Run(async () =>
        {
            while (!IsTimeOut())
            {
                if (Disposed_) break;
                if (!ClientSocket.Connected) break;

                byte[] clientBufferInit = new byte[MsmhAgnosticServer.MaxDataSize];
                byte[] clientBuffer = Array.Empty<byte>();

                try
                {
                    int clientRead = await ClientSocket.ReceiveAsync(clientBufferInit, SocketFlags.None).ConfigureAwait(false);
                    if (clientRead == 0) break;
                    clientBuffer = new byte[clientRead];
                    Buffer.BlockCopy(clientBufferInit, 0, clientBuffer, 0, clientRead);
                    clientBufferInit = Array.Empty<byte>();

                    // Client Received
                    RestartTimeoutTimer();
                    ProxyRelayEventArgs ea = new(this, clientBuffer);
                    OnClientDataReceived?.Invoke(this, ea);
                }
                catch (Exception)
                {
                    break;
                }

                if (!RemoteSocket.Connected) break;
                if (clientBuffer.Length == 0) break;

                try
                {
                    // Fragment Will Be Applied Here
                    if (Request.ApplyFragment && FP.FragmentMode == AgnosticProgram.Fragment.Mode.Program)
                    {
                        await SendFragmentedAsync(clientBuffer);
                    }
                    else
                    {
                        int remoteWrite = await RemoteSocket.SendAsync(clientBuffer, SocketFlags.None).ConfigureAwait(false);
                        if (remoteWrite == 0) break;
                    }

                    // Remote Sent
                    RestartTimeoutTimer();
                    ProxyRelayEventArgs ea = new(this, clientBuffer);
                    OnRemoteDataSent?.Invoke(this, ea);
                }
                catch (Exception)
                {
                    break;
                }
            }
        });
    }

    private async Task ReadRemoteAsync()
    {
        await Task.Run(async () =>
        {
            while (!IsTimeOut())
            {
                if (Disposed_) break;
                if (!RemoteSocket.Connected) break;

                byte[] remoteBufferInit = new byte[MsmhAgnosticServer.MaxDataSize];
                byte[] remoteBuffer = Array.Empty<byte>();

                try
                {
                    int remoteRead = await RemoteSocket.ReceiveAsync(remoteBufferInit, SocketFlags.None).ConfigureAwait(false);
                    if (remoteRead == 0) break;
                    remoteBuffer = new byte[remoteRead];
                    Buffer.BlockCopy(remoteBufferInit, 0, remoteBuffer, 0, remoteRead);
                    remoteBufferInit = Array.Empty<byte>();

                    // Remote Received
                    RestartTimeoutTimer();
                    ProxyRelayEventArgs ea = new(this, remoteBuffer);
                    OnRemoteDataReceived?.Invoke(this, ea);
                }
                catch (Exception)
                {
                    break;
                }

                if (!ClientSocket.Connected) break;
                if (remoteBuffer.Length == 0) break;

                try
                {
                    int clientWrite = await ClientSocket.SendAsync(remoteBuffer, SocketFlags.None).ConfigureAwait(false);
                    if (clientWrite == 0) break;

                    // Client Sent
                    RestartTimeoutTimer();
                    ProxyRelayEventArgs ea = new(this, remoteBuffer);
                    OnClientDataSent?.Invoke(this, ea);
                }
                catch (Exception)
                {
                    break;
                }
            }
        });
    }

    private async Task SendFragmentedAsync(byte[] data)
    {
        FP.DestHostname = Request.Address;
        FP.DestPort = Request.Port;
        if (FP.FragmentMode == AgnosticProgram.Fragment.Mode.Program)
        {
            AgnosticProgram.Fragment.ProgramMode programMode = new(data, RemoteSocket);
            await programMode.SendAsync(FP);
        }
    }

    private bool IsTimeOut()
    {
        bool isTimeOut = false;
        if (Request.TimeoutSec != 0 &&
            KillOnTimeout.ElapsedMilliseconds > TimeSpan.FromSeconds(Request.TimeoutSec).TotalMilliseconds)
        {
            isTimeOut = true;
            KillOnTimeout.Stop();
        }
        return isTimeOut;
    }

    private void RestartTimeoutTimer()
    {
        try
        {
            if (Request.TimeoutSec > 0)
            {
                if (!KillOnTimeout.IsRunning) KillOnTimeout.Start();
                KillOnTimeout.Restart();
            }
        }
        catch (Exception) { }
    }

    public void Disconnect()
    {
        try
        {
            if (!Disposed_)
            {
                KillOnTimeout.Reset();
                KillOnTimeout.Stop();

                ClientSocket?.Close();
                ClientSocket?.Dispose();

                RemoteSocket?.Close();
                RemoteSocket?.Dispose();

                Disposed_ = true;
                if (ProxyTunnel_ != null) ProxyTunnel_.ManualDisconnect = true;

                return;
            }
        }
        catch (Exception) { }
    }

}