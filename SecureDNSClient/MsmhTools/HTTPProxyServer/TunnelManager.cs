using System;
using System.Net.Sockets;

namespace MsmhTools.HTTPProxyServer
{    
    internal class TunnelManager
    {
        private Dictionary<int, Tunnel> Tunnels = new();

        /// <summary>
        /// Construct the tunnel manager.
        /// </summary>
        public TunnelManager()
        {

        }

        internal void Add(int threadId, Tunnel curr)
        {
            if (!Tunnels.ContainsKey(threadId))
                Tunnels.Add(threadId, curr);
        }
         
        internal void Remove(int threadId)
        {
            if (Tunnels.ContainsKey(threadId))
            {
                Tunnel curr = Tunnels[threadId];
                if (curr.ClientTcpClient.Connected)
                    curr.ClientTcpClient.Dispose();
                if (curr.ServerTcpClient.Connected)
                    curr.ServerTcpClient.Dispose();
                Tunnels.Remove(threadId);
                curr.Dispose();
            }
        }
        
        internal Dictionary<int, Tunnel> GetTunnels()
        {
            return Tunnels;
        }
        
        public int Count
        {
            get => Tunnels.ToList().Count;
        }
    }
}
