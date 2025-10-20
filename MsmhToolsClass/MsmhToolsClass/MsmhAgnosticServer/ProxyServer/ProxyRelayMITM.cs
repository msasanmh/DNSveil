using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class ProxyRelayMITM
{
    private TcpClient ClientTcpClient { get; set; }
    private TcpClient RemoteTcpClient { get; set; }
    public Stream? ClientStream { get; set; }
    public Stream? RemoteStream { get; set; }

    public event EventHandler<ProxyRelayMITMEventArgs>? OnClientDataReceived;
    public event EventHandler<ProxyRelayMITMEventArgs>? OnClientDataSent;
    public event EventHandler<ProxyRelayMITMEventArgs>? OnRemoteDataReceived;
    public event EventHandler<ProxyRelayMITMEventArgs>? OnRemoteDataSent;

    private readonly ProxyTunnel? ProxyTunnel_ = null;
    private ProxyRequest Request { get; set; }
    private readonly AgnosticSettingsSSL SettingsSSL_ = new(false);
    private readonly Stopwatch KillOnTimeout = new();
    private bool ActivateMITM { get; set; } = false;
    private bool Disposed_ { get; set; } = false;

    internal ProxyRelayMITM(ProxyTunnel proxyTunnel, bool activateMITM)
    {
        ProxyTunnel_ = proxyTunnel;
        Request = ProxyTunnel_.Req;
        SettingsSSL_ = ProxyTunnel_.SettingsSSL_;
        KillOnTimeout = ProxyTunnel_.KillOnTimeout;
        ActivateMITM = activateMITM;

        ClientTcpClient = new()
        {
            Client = ProxyTunnel_.Client.Socket_
        };
        
        RemoteTcpClient = new()
        {
            Client = ProxyTunnel_.Remote.Socket_
        };

        try
        {
            if (Request.ProxyName == Proxy.Name.HTTPS_SSL && ProxyTunnel_.Client.SslStream_ != null)
            {
                ClientStream = ProxyTunnel_.Client.SslStream_;
            }
            else
            {
                ClientStream = ClientTcpClient.GetStream();
            }

            RemoteStream = RemoteTcpClient.GetStream();
        }
        catch (Exception)
        {
            Disconnect();
        }
    }

    public async Task ExecuteAsync()
    {
        // Start Data Exchange.
        try
        {
            bool isDecryptSuccess = false;

            if (ActivateMITM && ClientStream != null && RemoteStream != null)
            {
                isDecryptSuccess = await DecryptHttpsTrafficAsync(ClientStream, RemoteStream, Request).ConfigureAwait(false);
            }

            if (ActivateMITM && !isDecryptSuccess)
            {
                Disconnect();
                return;
            }

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
                if (ClientStream == null) break;
                if (!ClientStream.CanRead) break;

                byte[] clientBufferInit = new byte[MsmhAgnosticServer.MaxDataSize];
                byte[] clientBuffer = Array.Empty<byte>();

                try
                {
                    int clientRead = await ClientStream.ReadAsync(clientBufferInit, CancellationToken.None).ConfigureAwait(false);
                    if (clientRead == 0) break;
                    clientBuffer = new byte[clientRead];
                    Buffer.BlockCopy(clientBufferInit, 0, clientBuffer, 0, clientRead);
                    clientBufferInit = Array.Empty<byte>();

                    // Client Received
                    RestartTimeoutTimer();
                    ProxyRelayMITMEventArgs ea = new(this, clientBuffer);
                    OnClientDataReceived?.Invoke(this, ea);
                }
                catch (Exception)
                {
                    break;
                }

                if (RemoteStream == null) break;
                if (!RemoteStream.CanWrite) break;
                if (clientBuffer.Length == 0) break;

                try
                {
                    await RemoteStream.WriteAsync(clientBuffer, CancellationToken.None).ConfigureAwait(false);

                    // Remote Sent
                    RestartTimeoutTimer();
                    ProxyRelayMITMEventArgs ea = new(this, clientBuffer);
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
                if (RemoteStream == null) break;
                if (!RemoteStream.CanRead) break;

                byte[] remoteBufferInit = new byte[MsmhAgnosticServer.MaxDataSize];
                byte[] remoteBuffer = Array.Empty<byte>();

                try
                {
                    int remoteRead = await RemoteStream.ReadAsync(remoteBufferInit, CancellationToken.None).ConfigureAwait(false);
                    if (remoteRead == 0) break;
                    remoteBuffer = new byte[remoteRead];
                    Buffer.BlockCopy(remoteBufferInit, 0, remoteBuffer, 0, remoteRead);
                    remoteBufferInit = Array.Empty<byte>();

                    // Remote Received
                    RestartTimeoutTimer();
                    ProxyRelayMITMEventArgs ea = new(this, remoteBuffer);
                    OnRemoteDataReceived?.Invoke(this, ea);
                }
                catch (Exception)
                {
                    break;
                }

                if (ClientStream == null) break;
                if (!ClientStream.CanWrite) break;
                if (remoteBuffer.Length == 0) break;

                try
                {
                    await ClientStream.WriteAsync(remoteBuffer, CancellationToken.None).ConfigureAwait(false);

                    // Client Sent
                    RestartTimeoutTimer();
                    ProxyRelayMITMEventArgs ea = new(this, remoteBuffer);
                    OnClientDataSent?.Invoke(this, ea);
                }
                catch (Exception) { }
            }
        });
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

    private async Task<bool> DecryptHttpsTrafficAsync(Stream clientStream, Stream remoteStream, ProxyRequest req)
    {
        try
        {
            if (ProxyTunnel_ == null) return false;
            if (!clientStream.CanRead || !remoteStream.CanRead) return false;
            if (SettingsSSL_.RootCA == null) return false;

            //ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            List<string> domains = new()
            {
                req.AddressOrig, req.Address
            };
            if (!req.AddressSNI.Equals(req.AddressOrig)) domains.Add(req.AddressSNI);

            string certSubject = CommonTools.GetWildCardDomainName(req.AddressOrig);
            X509Certificate2 certificate = CertificateTool.GenerateCertificateByIssuer(SettingsSSL_.RootCA, domains, certSubject, out RSA privateKey);
            if (!certificate.HasPrivateKey)
                certificate = certificate.CopyWithPrivateKey(privateKey);
            string pass = Guid.NewGuid().ToString();
            certificate = new(certificate.Export(X509ContentType.Pfx, pass), pass);

            //===== Server Authentication
            SslServerAuthenticationOptions optionsServer = new()
            {
                ServerCertificate = certificate,
                ClientCertificateRequired = false,
                EnabledSslProtocols = MsmhAgnosticServer.SSL_Protocols,
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                RemoteCertificateValidationCallback = MsmhAgnosticServer.Callback
            };

            SslStream sslStreamClient = new(clientStream, false, MsmhAgnosticServer.Callback, null);
            await sslStreamClient.AuthenticateAsServerAsync(optionsServer, CancellationToken.None).ConfigureAwait(false);

            // Update Client Stream
            ClientStream = sslStreamClient;

            //===== Client Authentication
            SslClientAuthenticationOptions optionsClient = new()
            {
                // Apply DontBypass Program: Use Fake DNS/SNI List (Change SNI) To Bypass DPI
                TargetHost = req.ApplyChangeSNI ? req.AddressSNI : req.AddressOrig,
                EnabledSslProtocols = MsmhAgnosticServer.SSL_Protocols,
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                RemoteCertificateValidationCallback = MsmhAgnosticServer.Callback
            };

            SslStream sslStreamRemote = new(remoteStream, false, MsmhAgnosticServer.Callback, null);
            await sslStreamRemote.AuthenticateAsClientAsync(optionsClient, CancellationToken.None).ConfigureAwait(false);

            // Update Remote Stream
            RemoteStream = sslStreamRemote;
            certificate?.Dispose();
            optionsServer.ServerCertificate?.Dispose();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"======= DecryptHttpsTrafficAsync ({req.AddressOrig} => {req.AddressSNI}):\n" + ex.GetInnerExceptions());
            return false;
        }
    }

    public void Disconnect()
    {
        try
        {
            if (!Disposed_)
            {
                KillOnTimeout.Reset();
                KillOnTimeout.Stop();

                ClientStream?.Close();
                ClientStream?.Dispose();

                RemoteStream?.Close();
                RemoteStream?.Dispose();

                ClientTcpClient?.Close();
                ClientTcpClient?.Dispose();

                RemoteTcpClient?.Close();
                RemoteTcpClient?.Dispose();

                if (ProxyTunnel_ != null)
                {
                    ProxyTunnel_.Client.SslStream_?.Close();
                    ProxyTunnel_.Client.SslStream_?.Dispose();
                }

                Disposed_ = true;
                if (ProxyTunnel_ != null) ProxyTunnel_.ManualDisconnect = true;

                return;
            }
        }
        catch(Exception) { }
    }

}