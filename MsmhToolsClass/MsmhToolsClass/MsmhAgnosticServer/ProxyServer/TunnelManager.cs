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
            Tunnels.TryAdd(pt.ConnectionId, new Lazy<ProxyTunnel>(() => pt, LazyThreadSafetyMode.ExecutionAndPublication));
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
                Tunnels.TryRemove(connectionId, out _);
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

    internal void KillAllRequests()
    {
        try
        {
            var dic = GetTunnels();
            Debug.WriteLine(dic.Count);
            foreach (var item in dic)
            {
                Debug.WriteLine(item.Key);
                Remove(item.Value.Value);
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
                return Tunnels.ToList().Count;
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }
}