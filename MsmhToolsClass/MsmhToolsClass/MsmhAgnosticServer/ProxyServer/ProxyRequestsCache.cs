using System.Collections.Concurrent;
using System.Diagnostics;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class ProxyRequestsCache
{
    private readonly ConcurrentDictionary<string, (DateTime dt, ProxyRequest pr, string eventMsg)> Caches = new();

    public (ProxyRequest pReq, string eventMsg)? Get(string key, ProxyRequest req)
    {
        try
        {
            bool isCached = Caches.TryGetValue(key, out (DateTime dt, ProxyRequest pr, string eventMsg) cachedReq);
            if (isCached)
            {
                DateTime now = DateTime.UtcNow;
                TimeSpan ts = now - cachedReq.dt;
                if (ts >= TimeSpan.FromMinutes(10))
                {
                    Caches.TryRemove(key, out _);
                }
                else
                {
                    if (req.ProxyName != Proxy.Name.Test)
                    {
                        req.TimeoutSec = cachedReq.pr.TimeoutSec;
                        req.ApplyFragment = cachedReq.pr.ApplyFragment;
                        req.ApplyChangeSNI = cachedReq.pr.ApplyChangeSNI;
                        req.AddressSNI = cachedReq.pr.AddressSNI;
                        req.Address = cachedReq.pr.Address;
                        req.IsDestBlocked = cachedReq.pr.IsDestBlocked;
                        req.ApplyUpStreamProxy = cachedReq.pr.ApplyUpStreamProxy;
                        req.AddressType = cachedReq.pr.AddressType;

                        return (req, cachedReq.eventMsg);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProxyRequestsCache Get: " + ex.Message);
        }

        return null;
    }

    public void Add(string key, ProxyRequest req, string msgReqEvent)
    {
        try
        {
            if (req.ProxyName != Proxy.Name.Test)
                Caches.TryAdd(key, (DateTime.UtcNow, req, msgReqEvent));
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProxyRequestsCache Add: " + ex.Message);
        }
    }

    public void Clear()
    {
        try
        {
            Caches.Clear();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProxyRequestsCache Clear: " + ex.Message);
        }
    }

}