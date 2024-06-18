using System.Collections.Concurrent;
using System.Diagnostics;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class ProxyRequestsCache
{
    public class OrigValues
    {
        public bool ApplyChangeSNI { get; set; }
        public bool ApplyFragment { get; set; }
        public bool IsDestBlocked { get; set; }
    }

    public class ApplyChangeSNI
    {
        public bool Apply { get; set; }
        public string Event_ApplyChangeSNI { get; set; } = string.Empty;
    }

    public class ApplyFragment
    {
        public bool Apply { get; set; }
        public string Event_ApplyFragment { get; set; } = string.Empty;
    }

    public class IsDestBlocked
    {
        public bool Apply { get; set; }
        public string Event_IsDestBlocked { get; set; } = string.Empty;
    }

    public class ProxyRequestsCacheResult
    {
        public OrigValues OrigValues = new();
        public ApplyChangeSNI ApplyChangeSNI = new();
        public ApplyFragment ApplyFragment = new();
        public IsDestBlocked IsDestBlocked = new();
    }

    private readonly ConcurrentDictionary<string, (DateTime dt, ProxyRequestsCacheResult prcr)> Caches = new();

    public ProxyRequestsCacheResult? Get(string key, ProxyRequest req)
    {
        try
        {
            bool isCached = Caches.TryGetValue(key, out (DateTime dt, ProxyRequestsCacheResult prcr) cachedReq);
            if (isCached)
            {
                DateTime now = DateTime.UtcNow;
                TimeSpan ts = now - cachedReq.dt;
                if (ts >= TimeSpan.FromMinutes(30))
                {
                    Caches.TryRemove(key, out _);
                }
                else
                {
                    if (req.ApplyChangeSNI == cachedReq.prcr.OrigValues.ApplyChangeSNI &&
                        req.ApplyFragment == cachedReq.prcr.OrigValues.ApplyFragment &&
                        req.IsDestBlocked == cachedReq.prcr.OrigValues.IsDestBlocked)
                    {
                        return cachedReq.prcr;
                    }
                    else
                    {
                        Caches.TryRemove(key, out _);
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

    public void Add(string key, ProxyRequestsCacheResult prcr)
    {
        try
        {
            Caches.TryAdd(key, (DateTime.UtcNow, prcr));
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