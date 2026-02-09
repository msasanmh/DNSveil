using Microsoft.Extensions.Caching.Memory;
using MsmhToolsClass;

namespace DNSveil.Logic.UpstreamServers;

public class RegionCache
{
    private readonly MemoryCache Caches = new(new MemoryCacheOptions());

    public bool TryAdd(string idUnique, CultureTool.RegionResult regionResult)
    {
        try
        {
            return Caches.TryAdd(idUnique, new Lazy<CultureTool.RegionResult>(() => regionResult, LazyThreadSafetyMode.ExecutionAndPublication));
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool TryGet(string idUnique, out CultureTool.RegionResult regionResult)
    {
        regionResult = new();
        bool success = false;

        try
        {
            success = Caches.TryGetValue(idUnique, out Lazy<CultureTool.RegionResult>? lrr);
            if (success && lrr != null) regionResult = lrr.Value;
        }
        catch (Exception) { }

        return success;
    }

    public bool TryRemove(string idUnique)
    {
        try
        {
            return Caches.TryRemove(idUnique);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public int CachedRequests
    {
        get
        {
            try
            {
                return Caches.Count;
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }

    public void Flush()
    {
        try
        {
            Caches.Clear();
        }
        catch (Exception) { }
    }

}