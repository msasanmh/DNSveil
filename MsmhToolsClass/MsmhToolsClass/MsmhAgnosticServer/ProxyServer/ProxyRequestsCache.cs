using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

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
        public string Comment { get; set; } = string.Empty;
    }

    public class ApplyFragment
    {
        public bool Apply { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class IsDestBlocked
    {
        public bool Apply { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class ProxyRequestsCacheResult
    {
        public OrigValues OrigValues = new();
        public ApplyChangeSNI ApplyChangeSNI = new();
        public ApplyFragment ApplyFragment = new();
        public IsDestBlocked IsDestBlocked = new();
    }

    private readonly MemoryCache Caches = new(new MemoryCacheOptions());

    public ProxyRequestsCacheResult? Get(string key, ProxyRequest req)
    {
        try
        {
            bool isCached = Caches.TryGetValue(key, out Lazy<ProxyRequestsCacheResult>? cachedReq);
            if (isCached && cachedReq != null)
            {
                ProxyRequestsCacheResult prcr = cachedReq.Value;

                if (req.ApplyChangeSNI == prcr.OrigValues.ApplyChangeSNI &&
                    req.ApplyFragment == prcr.OrigValues.ApplyFragment &&
                    req.IsDestBlocked == prcr.OrigValues.IsDestBlocked)
                {
                    return prcr;
                }
                else
                {
                    Caches.TryRemove(key);
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
            // Cache For 1 Hour
            Caches.Add(key, new Lazy<ProxyRequestsCacheResult>(() => prcr, LazyThreadSafetyMode.ExecutionAndPublication), TimeSpan.FromHours(1));
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