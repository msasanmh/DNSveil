using System;

namespace MsmhTools.HTTPProxyServer
{    
    internal class TunnelManager
    {
        private Dictionary<int, Tunnel> _Tunnels = new();
        private readonly object _TunnelsLock = new();

        /// <summary>
        /// Construct the tunnel manager.
        /// </summary>
        public TunnelManager()
        {

        }

        internal void Add(int threadId, Tunnel curr)
        {
            lock (_TunnelsLock)
            {
                if (_Tunnels.ContainsKey(threadId)) _Tunnels.Remove(threadId);
                _Tunnels.Add(threadId, curr);
            }
        }
         
        internal void Remove(int threadId)
        {
            lock (_TunnelsLock)
            {
                if (_Tunnels.ContainsKey(threadId))
                {
                    Tunnel curr = _Tunnels[threadId];
                    _Tunnels.Remove(threadId);
                    curr.Dispose();
                }
            }
        }
         
        internal Dictionary<int, Tunnel> GetMetadata()
        {
            Dictionary<int, Tunnel> ret = new();

            lock (_TunnelsLock)
            {
                foreach (KeyValuePair<int, Tunnel> curr in _Tunnels)
                {
                    ret.Add(curr.Key, curr.Value.Metadata());
                }
            }

            return ret;
        }
         
        internal Dictionary<int, Tunnel> GetFull()
        {
            Dictionary<int, Tunnel> ret = new();

            lock (_TunnelsLock)
            {
                ret = new Dictionary<int, Tunnel>(_Tunnels);
            }

            return ret;
        }
         
        internal bool Active(int threadId)
        {
            lock (_TunnelsLock)
            {
                if (_Tunnels.ContainsKey(threadId)) return true;
            }

            return false;
        }
         
        internal int Count()
        {
            lock (_TunnelsLock)
            {
                return _Tunnels.Count;
            }
        }
    }
}
