using System.Collections.Concurrent;
using System.Diagnostics;

#nullable enable
namespace MsmhToolsClass.MsmhAgnosticServer;

internal class TunnelManager
{
    private readonly ConcurrentDictionary<int, Lazy<ProxyTunnel>> Tunnels = new();

    /// <summary>
    /// Construct the Tunnel Manager.
    /// </summary>
    public TunnelManager() { }

    internal void Add(ProxyTunnel pt)
    {
        try
        {
            Tunnels.GetOrAdd(pt.ConnectionId, id => new Lazy<ProxyTunnel>(pt));
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
            if (Tunnels.ContainsKey(connectionId))
            {
                ProxyTunnel curr = Tunnels[connectionId].Value;
                if (curr != null)
                {
                    curr.Disconnect();
                    Tunnels.TryRemove(connectionId, out Lazy<ProxyTunnel>? _);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("TunnelManager Remove: " + ex.Message);
        }
    }

    internal Dictionary<int, Lazy<ProxyTunnel>> GetTunnels()
    {
        Dictionary<int, Lazy<ProxyTunnel>> tempDic = new(Tunnels);
        return tempDic;
    }

    public int Count
    {
        get
        {
            try
            {
                return Tunnels.ToList().Count;
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }
}