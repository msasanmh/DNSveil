using System.Net;
using System.Net.Sockets;

namespace MsmhToolsClass.ProxifiedClients;

public class ProxifiedTcpClient
{
    private string? ProxyScheme { get; set; }
    private string? ProxyUser { get; set; }
    private string? ProxyPass { get; set; }

    public ProxifiedTcpClient(string? proxyScheme, string? proxyUser, string? proxyPass)
    {
        ProxyScheme = proxyScheme;
        ProxyUser = proxyUser;
        ProxyPass = proxyPass;
    }

    public async Task<(bool isSuccess, TcpClient? proxifiedTcpClient)> TryGetConnectedProxifiedTcpClient(string host, int port)
    {
        if (!string.IsNullOrEmpty(ProxyScheme))
        {
            NetworkTool.GetUrlDetails(ProxyScheme, 443, out _, out string proxyHost, out _, out _, out int proxyPort, out _, out _);

            try
            {
                if (ProxyScheme.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) || ProxyScheme.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                {
                    HttpTcpClient httpTcpClient = new(proxyHost, proxyPort, ProxyUser, ProxyPass);
                    TcpClient? proxifiedClient = await httpTcpClient.CreateConnectionAsync(host, port).ConfigureAwait(false);
                    if (proxifiedClient != null)
                    {
                        return (true, proxifiedClient);
                    }
                }
                else if (ProxyScheme.StartsWith("socks5://", StringComparison.InvariantCultureIgnoreCase))
                {
                    Socks5TcpClient socks5TcpClient = new(proxyHost, proxyPort, ProxyUser, ProxyPass);
                    TcpClient? proxifiedClient = await socks5TcpClient.CreateConnectionAsync(host, port).ConfigureAwait(false);
                    if (proxifiedClient != null)
                    {
                        return (true, proxifiedClient);
                    }
                }
            }
            catch (Exception) { }
        }
        return (false, null);
    }

    public async Task<(bool isSuccess, TcpClient? proxifiedTcpClient)> TryGetConnectedProxifiedTcpClient(IPEndPoint ep)
    {
        return await TryGetConnectedProxifiedTcpClient(ep.Address.ToString(), ep.Port).ConfigureAwait(false);
    }
}