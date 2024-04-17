using MsmhToolsClass.ProxifiedClients;
using System.Net.Sockets;

namespace MsmhToolsClass.MsmhAgnosticServer;

public partial class AgnosticProgram
{
    public partial class ProxyRules
    {
        public async Task<TcpClient?> ConnectToUpStream(ProxyRequest req)
        {
            ProxyRulesResult prr = req.RulesResult;
            string destHostname = req.Address;
            int destHostPort = req.Port;

            if (!prr.ApplyUpStreamProxy) return null;
            if (string.IsNullOrEmpty(prr.ProxyScheme)) return null;

            ProxifiedTcpClient proxifiedTcpClient = new(prr.ProxyScheme, prr.ProxyUser, prr.ProxyPass);
            var upstream = await proxifiedTcpClient.TryGetConnectedProxifiedTcpClient(destHostname, destHostPort);
            if (upstream.isSuccess && upstream.proxifiedTcpClient != null) return upstream.proxifiedTcpClient;

            return null;
        }
    }
}
