using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MsmhTools.HTTPProxyServer
{    
    internal class TunnelManager
    {
        private readonly ConcurrentDictionary<int, Lazy<Tunnel>> Tunnels = new();

        /// <summary>
        /// Construct the Tunnel Manager.
        /// </summary>
        public TunnelManager()
        {

        }

        internal void Add(int threadId, Tunnel curr)
        {
            try
            {
                Tunnels.GetOrAdd(threadId, id => new Lazy<Tunnel>(curr));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TunnelManager Add: " + ex.Message);
            }
        }
         
        internal void Remove(int threadId)
        {
            try
            {
                if (Tunnels.ContainsKey(threadId))
                {
                    Tunnel curr = Tunnels[threadId].Value;
                    if (curr != null)
                    {
                        if (curr.ClientTcpClient.Connected)
                            curr.ClientTcpClient.Dispose();
                        if (curr.ServerTcpClient.Connected)
                            curr.ServerTcpClient.Dispose();
                        Tunnels.TryRemove(threadId, out Lazy<Tunnel> _);
                        curr.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TunnelManager Remove: " + ex.Message);
            }
        }
        
        internal Dictionary<int, Lazy<Tunnel>> GetTunnels()
        {
            Dictionary<int, Lazy<Tunnel>> tempDic = new(Tunnels);
            return tempDic;
        }
        
        public int Count
        {
            get => Tunnels.ToList().Count;
        }
    }
}
