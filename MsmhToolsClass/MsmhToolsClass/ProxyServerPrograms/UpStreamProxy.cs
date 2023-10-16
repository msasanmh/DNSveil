using MsmhToolsClass.ProxifiedTcpClient;
using System;
using System.Net;
using System.Net.Sockets;

namespace MsmhToolsClass.ProxyServerPrograms;

public partial class ProxyProgram
{
    public class UpStreamProxy
    {
        public enum Mode
        {
            HTTP,
            SOCKS5,
            Disable
        }

        public Mode UpStreamMode { get; private set; } = Mode.Disable;
        public string? ProxyHost { get; private set; }
        public int ProxyPort { get; private set; }
        public string? ProxyUsername { get; private set; }
        public string? ProxyPassword { get; private set; }
        public bool OnlyApplyToBlockedIps { get; set; }

        public UpStreamProxy() { }

        public void Set(Mode mode, string proxyHost, int proxyPort, bool onlyApplyToBlockedIps)
        {
            //Set
            UpStreamMode = mode;
            ProxyHost = proxyHost;
            ProxyPort = proxyPort;
            OnlyApplyToBlockedIps = onlyApplyToBlockedIps;
        }

        public void Set(Mode mode, string proxyHost, int proxyPort, string proxyUsername, string proxyPassword, bool onlyApplyToBlockedIps)
        {
            //Set
            UpStreamMode = mode;
            ProxyHost = proxyHost;
            ProxyPort = proxyPort;
            ProxyUsername = proxyUsername;
            ProxyPassword = proxyPassword;
            OnlyApplyToBlockedIps = onlyApplyToBlockedIps;
        }

        public async Task<TcpClient?> Connect(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            if (string.IsNullOrEmpty(ProxyHost)) return null;

            if (socketAsyncEventArgs.RemoteEndPoint is not IPEndPoint ipEndPoint) return null;

            return await Connect(ipEndPoint.Address.ToString(), ipEndPoint.Port);
        }

        /// <summary>
        /// Connect TcpClient through Proxy
        /// </summary>
        /// <param name="destHostname"></param>
        /// <param name="destHostPort"></param>
        /// <returns>Returns null if cannot connect to proxy</returns>
        public async Task<TcpClient?> Connect(string destHostname, int destHostPort)
        {
            if (string.IsNullOrEmpty(ProxyHost)) return null;

            if (UpStreamMode == Mode.HTTP)
            {
                HttpProxyClient httpProxyClient = new(ProxyHost, ProxyPort, ProxyUsername, ProxyPassword);
                return await httpProxyClient.CreateConnection(destHostname, destHostPort);
            }
            else if (UpStreamMode == Mode.SOCKS5)
            {
                Socks5ProxyClient socks5ProxyClient = new(ProxyHost, ProxyPort, ProxyUsername, ProxyPassword);
                return await socks5ProxyClient.CreateConnection(destHostname, destHostPort);
            }
            else
                return null;
        }
    }
}