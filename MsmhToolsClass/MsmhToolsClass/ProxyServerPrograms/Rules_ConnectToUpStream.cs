using MsmhToolsClass.MsmhProxyServer;
using MsmhToolsClass.ProxifiedTcpClient;
using System.Diagnostics;
using System.Net.Sockets;

namespace MsmhToolsClass.ProxyServerPrograms;

public partial class ProxyProgram
{
    public partial class Rules
    {
        public async Task<TcpClient?> ConnectToUpStream(ProxyRequest req)
        {
            RulesResult rr = req.RulesResult;
            string destHostname = req.Address;
            int destHostPort = req.Port;

            if (!rr.ApplyUpStreamProxy) return null;
            if (string.IsNullOrEmpty(rr.ProxyScheme)) return null;

            try
            {
                NetworkTool.GetUrlDetails(rr.ProxyScheme, 443, out _, out string proxyHost, out _, out _, out int proxyPort, out _, out _);

                if (rr.ProxyScheme.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) ||
                    rr.ProxyScheme.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                {

                    HttpProxyClient httpProxyClient = new(proxyHost, proxyPort, rr.ProxyUser, rr.ProxyPass);
                    return await httpProxyClient.CreateConnection(destHostname, destHostPort);
                }
                else if (rr.ProxyScheme.StartsWith("socks5://", StringComparison.InvariantCultureIgnoreCase))
                {
                    Socks5ProxyClient socks5ProxyClient = new(proxyHost, proxyPort, rr.ProxyUser, rr.ProxyPass);
                    return await socks5ProxyClient.CreateConnection(destHostname, destHostPort);
                }
                else return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ConnectToUpstream: " + ex.Message);
                return null;
            }
        }
    }
}
