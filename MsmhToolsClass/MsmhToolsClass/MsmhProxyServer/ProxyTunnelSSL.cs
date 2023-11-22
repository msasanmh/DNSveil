using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

#nullable enable
namespace MsmhToolsClass.MsmhProxyServer;

/// <summary>
/// CONNECT tunnel.
/// </summary>
public class ProxyTunnelSSL : IDisposable
{
    public ProxyRequest Request { get; set; }

    /// <summary>
    /// The TCP client instance for the requestor.
    /// </summary>
    public TcpClient ClientTcpClient { get; set; }

    /// <summary>
    /// The TCP client instance for the server.
    /// </summary>
    public TcpClient RemoteTcpClient { get; set; }

    /// <summary>
    /// The data stream for the client.
    /// </summary>
    public Stream ClientStream { get; set; }

    /// <summary>
    /// The data stream for the server.
    /// </summary>
    public Stream RemoteStream { get; set; }

    private readonly Stopwatch KillOnTimeout = new();

    /// <summary>
    /// Construct a Tunnel object.
    /// </summary>
    /// <param name="request">Request.</param>
    /// <param name="client">TCP client instance of the client.</param>
    /// <param name="server">TCP client instance of the server.</param>
    public ProxyTunnelSSL(ProxyRequest request, TcpClient client, TcpClient remote)
    {
        Request = request;

        ClientTcpClient = client;
        //ClientTcpClient.NoDelay = true;
        //ClientTcpClient.Client.NoDelay = true;

        RemoteTcpClient = remote;
        //RemoteTcpClient.NoDelay = true;
        //RemoteTcpClient.Client.NoDelay = true;

        ClientStream = client.GetStream();
        RemoteStream = remote.GetStream();
    }

    public async Task Execute()
    {
        await Task.Run(async () =>
        {
            bool isDecryptSuccess = await DecryptHttpsTrafficAsync(ClientStream, RemoteStream, Request);
            if (!isDecryptSuccess) return;

            while (IsActive())
            {
                Task c = ReadClient();
                Task r = ReadRemote();
                await Task.WhenAll(c, r);
            }
        });
    }

    private async Task ReadClient() // We do have TLS Handshake here
    {
        await Task.Run(async () =>
        {
            while(IsActive())
            {
                if (!ClientStream.CanRead) break;

                byte[] clientBuffer = new byte[65536];
                try
                {
                    int remoteRead = await ClientStream.ReadAsync(clientBuffer, CancellationToken.None);
                    Array.Resize(ref clientBuffer, remoteRead);
                }
                catch (Exception)
                {
                    break;
                }

                if (!RemoteStream.CanWrite) break;

                try
                {
                    await RemoteStream.WriteAsync(clientBuffer, CancellationToken.None);
                }
                catch (Exception)
                {

                }
            }
        });
    }

    private async Task ReadRemote() // We don't have TLS Handshake here
    {
        await Task.Run(async () =>
        {
            while(IsActive())
            {
                if (!RemoteStream.CanRead) break;

                byte[] remoteBuffer = new byte[65536];
                try
                {
                    int remoteRead = await RemoteStream.ReadAsync(remoteBuffer, CancellationToken.None);
                    Array.Resize(ref remoteBuffer, remoteRead);
                }
                catch (Exception)
                {
                    break;
                }

                if (!ClientStream.CanWrite) break;

                try
                {
                    await ClientStream.WriteAsync(remoteBuffer, CancellationToken.None);
                }
                catch (Exception)
                {

                }
            }
        });

        Dispose();
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
    /// Determines whether or not the tunnel is active.
    /// </summary>
    /// <returns>True if both connections are active.</returns>
    public bool IsActive()
    {
        return IsClientActive() && IsRemoteActive();
        //if (Request.TimeoutSec != 0 &&
        //    KillOnTimeout.ElapsedMilliseconds > TimeSpan.FromSeconds(Request.TimeoutSec).TotalMilliseconds)
        //{
        //    string msg = $"Killed Request On Timeout({Request.TimeoutSec} Sec): {Request.AddressOrig}:{Request.Port}";
        //    OnDebugInfoReceived?.Invoke(msg, EventArgs.Empty);
        //    Debug.WriteLine(msg);
        //    _Active = false;
        //    KillOnTimeout.Stop();
        //}
        //else
        //    _Active = IsClientActive() && IsRemoteActive();
        //return _Active;
    }

    /// <summary>
    /// Tear down the tunnel object and resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        ClientStream?.Close();
        ClientStream?.Dispose();

        RemoteStream?.Close();
        RemoteStream?.Dispose();

        ClientTcpClient?.Dispose();
        RemoteTcpClient?.Dispose();
    }

    private TcpState GetTcpRemoteState(TcpClient tcpClient)
    {
        try
        {
            if (tcpClient.Client == null)
                return TcpState.Unknown;

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
            if (!clientStream.CanRead || !remoteStream.CanRead) return false;

            //ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            static bool callback(object sender, X509Certificate? cert, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true;
            SslProtocols protocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;

            //===== Server
            string rootCACertPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "certificate", "rootCA.crt"));
            string rootCAKeyPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "certificate", "rootCA.key"));
            X509Certificate2 rootCACert = new(X509Certificate2.CreateFromCertFile(rootCACertPath));
            RSA rootCAKey = RSA.Create();
            rootCAKey.ImportFromPem(File.ReadAllText(rootCAKeyPath).ToCharArray());

            string pass = Guid.NewGuid().ToString();
            X509Certificate2 rootCert = rootCACert.CopyWithPrivateKey(rootCAKey);
            rootCert = new(rootCert.Export(X509ContentType.Pfx, pass), pass);

            List<string> domains = new()
            {
                req.AddressOrig, req.Address
            };
            string certSubject = MsmhProxyServer.GetWildCardDomainName(req.AddressOrig);
            X509Certificate2 certificate = CertificateTool.GenerateCertificateByIssuer(rootCert, domains, certSubject, out RSA privateKey);
            X509Certificate2 reqServerCert = certificate.CopyWithPrivateKey(privateKey);
            reqServerCert = new(reqServerCert.Export(X509ContentType.Pfx, pass), pass);

            //Debug.WriteLine("Serial Number: " + reqServerCert.SerialNumber);

            SslStream sslStreamClient = new(clientStream, false, callback, null);
            SslServerAuthenticationOptions optionsServer = new();
            optionsServer.ServerCertificate = reqServerCert;
            optionsServer.ClientCertificateRequired = false;
            optionsServer.EnabledSslProtocols = protocols;
            optionsServer.CertificateRevocationCheckMode = X509RevocationMode.NoCheck;

            await sslStreamClient.AuthenticateAsServerAsync(optionsServer, CancellationToken.None);

            // Update Client Stream
            ClientStream = sslStreamClient;

            //===== Client
            SslStream sslStreamRemote = new(remoteStream, false, callback, null);
            SslClientAuthenticationOptions optionsClient = new();
            optionsClient.TargetHost = req.AddressOrig;
            optionsClient.EnabledSslProtocols = protocols;
            optionsClient.CertificateRevocationCheckMode = X509RevocationMode.NoCheck;

            await sslStreamRemote.AuthenticateAsClientAsync(optionsClient, CancellationToken.None);

            // Update Remote Stream
            RemoteStream = sslStreamRemote;

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DecryptHttpsTrafficAsync: " + ex.Message);
            if (ex.InnerException != null)
                Debug.WriteLine("InnerException: " + ex.InnerException);
            return false;
        }
    }

}