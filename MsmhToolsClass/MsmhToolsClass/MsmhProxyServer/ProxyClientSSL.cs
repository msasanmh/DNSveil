using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System;

namespace MsmhToolsClass.MsmhProxyServer;

public class ProxyClientSSL
{
    private bool Disposed_ { get; set; } = false;

    public ProxyRequest Request { get; set; }
    private TcpClient ClientTcpClient { get; set; }
    private TcpClient RemoteTcpClient { get; set; }
    public Stream ClientStream { get; set; }
    public Stream RemoteStream { get; set; }

    public event EventHandler<SSLDataEventArgs>? OnClientDataReceived;
    public event EventHandler<SSLDataEventArgs>? OnClientDataSent;

    public event EventHandler<SSLDataEventArgs>? OnRemoteDataReceived;
    public event EventHandler<SSLDataEventArgs>? OnRemoteDataSent;

    private readonly ProxyTunnel? ProxyTunnel_ = null;
    private readonly SettingsSSL SettingsSSL_ = new(false);
    private readonly Stopwatch KillOnTimeout = new();

    internal ProxyClientSSL(ProxyTunnel proxyTunnel)
    {
        ProxyTunnel_ = proxyTunnel;

        ClientTcpClient = new(); ClientTcpClient.Client = ProxyTunnel_.Client.Socket_;
        RemoteTcpClient = new(); RemoteTcpClient.Client = ProxyTunnel_.RemoteClient.Socket_;

        ClientStream = ClientTcpClient.GetStream();
        RemoteStream = RemoteTcpClient.GetStream();

        Request = ProxyTunnel_.Req;
        SettingsSSL_ = ProxyTunnel_.SettingsSSL_;
        KillOnTimeout = ProxyTunnel_.KillOnTimeout;
    }

    /// <summary>
    /// Returns False If Decryption Fails otherwise True
    /// </summary>
    public async Task Execute()
    {
        // Start Data Exchange.
        await Task.Run(async () =>
        {
            bool isDecryptSuccess = await DecryptHttpsTrafficAsync(ClientStream, RemoteStream, Request);
            if (!isDecryptSuccess)
            {
                Disconnect();
                return;
            }

            while (IsActive())
            {
                Task c = ReadClient();
                Task r = ReadRemote();
                await Task.WhenAll(c, r);
            }

            Disconnect();
        });
    }

    private async Task ReadClient()
    {
        await Task.Run(async () =>
        {
            while (IsActive())
            {
                if (Disposed_) break;
                if (!ClientStream.CanRead) break;

                byte[] clientBufferInit = new byte[65536];
                byte[] clientBuffer = Array.Empty<byte>();
                try
                {
                    int clientRead = await ClientStream.ReadAsync(clientBufferInit, CancellationToken.None);
                    if (clientRead == 0) break;
                    clientBuffer = new byte[clientRead];
                    Buffer.BlockCopy(clientBufferInit, 0, clientBuffer, 0, clientRead);
                    
                    // Client Received
                    SSLDataEventArgs data = new(this, clientBuffer);
                    OnClientDataReceived?.Invoke(this, data);
                    
                }
                catch (Exception)
                {
                    break;
                }

                if (!RemoteStream.CanWrite) break;

                try
                {
                    if (clientBuffer.Length == 0) break;

                    await RemoteStream.WriteAsync(clientBuffer, CancellationToken.None);

                    // Remote Sent
                    SSLDataEventArgs data = new(this, clientBuffer);
                    OnRemoteDataSent?.Invoke(this, data);
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
        });
    }

    private async Task ReadRemote()
    {
        await Task.Run(async () =>
        {
            while (IsActive())
            {
                if (Disposed_) break;
                if (!RemoteStream.CanRead) break;

                byte[] remoteBufferInit = new byte[65536];
                byte[] remoteBuffer = Array.Empty<byte>();
                try
                {
                    int remoteRead = await RemoteStream.ReadAsync(remoteBufferInit, CancellationToken.None);
                    if (remoteRead == 0) break;
                    remoteBuffer = new byte[remoteRead];
                    Buffer.BlockCopy(remoteBufferInit, 0, remoteBuffer, 0, remoteRead);

                    // Remote Received
                    SSLDataEventArgs data = new(this, remoteBuffer);
                    OnRemoteDataReceived?.Invoke(this, data);
                    
                }
                catch (Exception)
                {
                    break;
                }

                if (!ClientStream.CanWrite) break;

                try
                {
                    if (remoteBuffer.Length == 0) break;

                    await ClientStream.WriteAsync(remoteBuffer, CancellationToken.None);

                    // Client Sent
                    SSLDataEventArgs data = new(this, remoteBuffer);
                    OnClientDataSent?.Invoke(this, data);
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
        });

        Disconnect();
    }

    private bool IsClientActive()
    {
        bool clientActive = false;
        bool clientSocketActive = false;

        if (ClientTcpClient != null)
        {
            clientActive = ClientTcpClient.Connected;

            if (ClientTcpClient.Client != null)
            {
                TcpState clientState = GetTcpRemoteState(ClientTcpClient);

                if (clientState == TcpState.Established
                    || clientState == TcpState.Listen
                    || clientState == TcpState.SynReceived
                    || clientState == TcpState.SynSent
                    || clientState == TcpState.TimeWait)
                {
                    clientSocketActive = true;
                }
            }
        }

        return clientActive && clientSocketActive;
    }

    private bool IsRemoteActive()
    {
        return RemoteTcpClient != null && RemoteTcpClient.Connected;
    }

    /// <summary>
    /// Determines whether or not the ssl tunnel is active.
    /// </summary>
    /// <returns>True if both connections are active.</returns>
    public bool IsActive()
    {
        bool active;
        if (Request.TimeoutSec != 0 &&
            KillOnTimeout.ElapsedMilliseconds > TimeSpan.FromSeconds(Request.TimeoutSec).TotalMilliseconds)
        {
            active = false;
            KillOnTimeout.Stop();
        }
        else
            active = IsClientActive() && IsRemoteActive();
        return active;
    }

    private TcpState GetTcpRemoteState(TcpClient tcpClient)
    {
        try
        {
            if (tcpClient.Client == null) return TcpState.Unknown;

            IPGlobalProperties ipgp = IPGlobalProperties.GetIPGlobalProperties();
            if (ipgp != null)
            {
                TcpConnectionInformation[]? tcis = ipgp.GetActiveTcpConnections();
                if (tcis != null)
                {
                    for (int n = 0; n < tcis.Length; n++)
                    {
                        TcpConnectionInformation? tci = tcis[n];
                        if (tci != null)
                        {
                            if (tcpClient.Client != null)
                            {
                                if (tci.RemoteEndPoint.Equals(tcpClient.Client.RemoteEndPoint))
                                    return tci.State;
                            }
                        }
                    }
                }
            }

            return TcpState.Unknown;
        }
        catch (Exception)
        {
            return TcpState.Unknown;
        }
    }

    private async Task<bool> DecryptHttpsTrafficAsync(Stream clientStream, Stream remoteStream, ProxyRequest req)
    {
        try
        {
            if (ProxyTunnel_ == null) return false;
            if (!clientStream.CanRead || !remoteStream.CanRead) return false;

            //ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            static bool callback(object sender, X509Certificate? cert, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true;
            SslProtocols protocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
            
            string pass = Guid.NewGuid().ToString();

            List<string> domains = new()
            {
                req.AddressOrig, req.Address
            };
            string certSubject = MsmhProxyServer.GetWildCardDomainName(req.AddressOrig);
            X509Certificate2 certificate = CertificateTool.GenerateCertificateByIssuer(SettingsSSL_.RootCA, domains, certSubject, out RSA privateKey);
            if (!certificate.HasPrivateKey)
                certificate = certificate.CopyWithPrivateKey(privateKey);
            certificate = new(certificate.Export(X509ContentType.Pfx, pass), pass);

            Debug.WriteLine("Serial Number: " + certificate.SerialNumber);

            //===== Server Authentication
            SslStream sslStreamClient = new(clientStream, false, callback, null);
            SslServerAuthenticationOptions optionsServer = new();
            optionsServer.ServerCertificate = certificate;
            optionsServer.ClientCertificateRequired = false;
            optionsServer.EnabledSslProtocols = protocols;
            optionsServer.CertificateRevocationCheckMode = X509RevocationMode.NoCheck;

            await sslStreamClient.AuthenticateAsServerAsync(optionsServer, CancellationToken.None);

            // Update Client Stream
            ClientStream = sslStreamClient;

            //===== Client Authentication
            SslStream sslStreamRemote = new(remoteStream, false, callback, null);
            SslClientAuthenticationOptions optionsClient = new();
            
            // Apply DontBypass Program
            bool changeSniToIp = SettingsSSL_.ChangeSniToIP && req.ApplyDpiBypass;

            // We Use IP Address As SNI Instead Of Website Address To Bypass DPI
            if (changeSniToIp)
                optionsClient.TargetHost = req.Address;
            else
                optionsClient.TargetHost = req.AddressOrig;

            optionsClient.EnabledSslProtocols = protocols;
            optionsClient.CertificateRevocationCheckMode = X509RevocationMode.NoCheck;

            await sslStreamRemote.AuthenticateAsClientAsync(optionsClient, CancellationToken.None);

            // Update Remote Stream
            RemoteStream = sslStreamRemote;

            certificate?.Dispose();
            optionsServer.ServerCertificate?.Dispose();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("======= DecryptHttpsTrafficAsync:\n" + ex.GetInnerExceptions());
            return false;
        }
    }

    public void Disconnect()
    {
        try
        {
            if (!Disposed_)
            {
                ClientStream?.Close();
                ClientStream?.Dispose();

                RemoteStream?.Close();
                RemoteStream?.Dispose();

                ClientTcpClient?.Dispose();
                RemoteTcpClient?.Dispose();

                Disposed_ = true;
                if (ProxyTunnel_ != null) ProxyTunnel_.Disconnect = true;
                return;
            }
        }
        catch(Exception)
        {
            // do nothing
        }
    }

}