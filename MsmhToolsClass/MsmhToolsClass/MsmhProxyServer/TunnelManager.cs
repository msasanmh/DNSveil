using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;

#nullable enable
namespace MsmhToolsClass.MsmhProxyServer;

internal class TunnelManager
{
    private readonly ConcurrentDictionary<int, Lazy<ProxyTunnel>> Tunnels = new();

    /// <summary>
    /// Construct the Tunnel Manager.
    /// </summary>
    public TunnelManager()
    {

    }

    internal void Add(int threadId, ProxyTunnel curr)
    {
        try
        {
            Tunnels.GetOrAdd(threadId, id => new Lazy<ProxyTunnel>(curr));
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
                ProxyTunnel curr = Tunnels[threadId].Value;
                if (curr != null)
                {
                    if (curr.Client.Socket_ != null && curr.Client.Socket_.Connected)
                    {
                        curr.Client.Socket_.Shutdown(SocketShutdown.Both);
                        curr.Client.Socket_.Close();
                    }
                    if (curr.RemoteClient.Socket_ != null && curr.RemoteClient.Socket_.Connected)
                    {
                        curr.RemoteClient.Socket_.Shutdown(SocketShutdown.Both);
                        curr.RemoteClient.Socket_.Close();
                    }
                    Tunnels.TryRemove(threadId, out Lazy<ProxyTunnel>? _);
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
            int count = 0;
            lock (Tunnels)
            {
                count = Tunnels.Count;
            }
            return count;
        }
    }
}