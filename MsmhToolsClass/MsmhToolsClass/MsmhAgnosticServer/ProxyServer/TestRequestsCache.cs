using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class TestRequestsCache
{
    public class TestRequestsCacheResult
    {
        public bool ApplyChangeSNI { get; set; }
        public bool ApplyFragment { get; set; }

        public TestRequestsCacheResult(bool applyChangeSNI, bool applyFragment)
        {
            ApplyChangeSNI = applyChangeSNI;
            ApplyFragment = applyFragment;
        }
    }

    private readonly MemoryCache Caches = new(new MemoryCacheOptions());

    public TestRequestsCacheResult? Get(string key)
    {
        try
        {
            bool isCached = Caches.TryGetValue(key, out Lazy<TestRequestsCacheResult>? ltrcr);
            if (isCached && ltrcr != null) return ltrcr.Value;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("TestRequestsCache Get: " + ex.Message);
        }

        return null;
    }

    public void AddOrUpdate(string key, TestRequestsCacheResult trcr)
    {
        try
        {
            // Cache For 2 Minutes
            Caches.Set(key, new Lazy<TestRequestsCacheResult>(() => trcr, LazyThreadSafetyMode.ExecutionAndPublication), TimeSpan.FromMinutes(2));
        }
        catch (Exception ex)
        {
            Debug.WriteLine("TestRequestsCache AddOrUpdate: " + ex.Message);
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
            Debug.WriteLine("TestRequestsCache Clear: " + ex.Message);
        }
    }

}