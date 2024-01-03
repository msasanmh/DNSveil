using Java.Util;

namespace XamarinAndroidVPNExample.VPNService
{
    public abstract class LRUCache<K, V> : LinkedHashMap
    {
        private int maxSize;

        public LRUCache(int maxSize)
            : base(maxSize + 1, 1, true)
        {
            this.maxSize = maxSize;
        }

        protected override bool RemoveEldestEntry(IMapEntry eldest)
        {
            if (this.Size() > maxSize)
            {
                Cleanup(eldest);
                return true;
            }

            return false;
        }

        public virtual void Cleanup(IMapEntry eldest)
        {
        }
    }
}
