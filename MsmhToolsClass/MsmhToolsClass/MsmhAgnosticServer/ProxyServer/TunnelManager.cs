using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

#nullable enable
namespace MsmhToolsClass.MsmhAgnosticServer;

internal class TunnelManager
{
    private readonly MemoryCache Tunnels = new(new MemoryCacheOptions());

    /// <summary>
    /// Construct the Tunnel Manager.
    /// </summary>
    public TunnelManager() { }

    internal void Add(ProxyTunnel pt)
    {
        try
        {
            Tunnels.Add(pt.ConnectionId, new Lazy<ProxyTunnel>(() => pt, LazyThreadSafetyMode.ExecutionAndPublication));
        }
        catch (Exception ex)
        {
            Debug.WriteLine("TunnelManager Add: " + ex.Message);
        }
    }

    internal void Remove(ProxyTunnel pt)
    {
        try
        {
            int connectionId = pt.ConnectionId;
            bool keyExist = Tunnels.TryGetValue(connectionId, out Lazy<ProxyTunnel>? lpt);
            if (keyExist && lpt != null)
            {
                ProxyTunnel curr = lpt.Value;
                curr.Disconnect();
                Tunnels.TryRemove(connectionId);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("TunnelManager Remove: " + ex.Message);
        }
    }

    internal void KillAllRequests()
    {
        try
        {
            foreach (object key in Tunnels.Keys)
            {
                bool exist = Tunnels.TryGetValue(key, out Lazy<ProxyTunnel>? lpt);
                if (exist && lpt != null)
                {
                    Debug.WriteLine(key);
                    Remove(lpt.Value);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("TunnelManager KillAllRequests: " + ex.Message);
        }
    }

    public int Count
    {
        get
        {
            try
            {
                return Tunnels.Count;
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }
}