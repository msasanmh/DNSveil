using Java.Nio;
using Java.Util.Concurrent;

namespace XamarinAndroidVPNExample.VPNService
{
    public class ByteBufferPool: Java.Lang.Object
    {
        private const int BUFFER_SIZE = 16384; // XXX: Is this ideal?
        private static ConcurrentLinkedQueue pool = new ConcurrentLinkedQueue();

        public static ByteBuffer acquire()
        {
            var buffer = (ByteBuffer)pool.Poll();
            if (buffer == null)
                buffer = ByteBuffer.AllocateDirect(BUFFER_SIZE); // Using DirectBuffer for zero-copy
            return buffer;
        }

        public static void Release(ByteBuffer buffer)
        {
            buffer.Clear();
            pool.Offer(buffer);
        }

        public static void Clear()
        {
            pool.Clear();
        }
    }
}
