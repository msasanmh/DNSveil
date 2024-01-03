using Java.IO;
using Java.Lang;
using Java.Nio.Channels;
using Java.Util;
using System.Collections.Generic;
using IOException = Java.IO.IOException;
using Object = Java.Lang.Object;
using String = Java.Lang.String;

namespace XamarinAndroidVPNExample.VPNService
{
    public class TCB : Object
    {
        public String ipAndPort;

        public long mySequenceNum, theirSequenceNum;
        public long myAcknowledgementNum, theirAcknowledgementNum;
        public TCBStatus status;

        // TCP has more states, but we need only these
        public enum TCBStatus
        {
            SYN_SENT,
            SYN_RECEIVED,
            ESTABLISHED,
            CLOSE_WAIT,
            LAST_ACK,
        }

        public Packet referencePacket;

        public SocketChannel channel;
        public bool waitingForNetworkData;
        public SelectionKey selectionKey;

        private const int MAX_CACHE_SIZE = 50; // XXX: Is this ideal?

        private static TCBLRUCache tcbCache = new TCBLRUCache(MAX_CACHE_SIZE);

        public static TCB GetTCB(String ipAndPort)
        {
            lock (tcbCache)
            {
                return (TCB)tcbCache.Get(ipAndPort);
            }
        }

        public static void PutTCB(String ipAndPort, TCB tcb)
        {
            lock (tcbCache)
            {
                tcbCache.Put(ipAndPort, tcb);
            }
        }

        public TCB(String ipAndPort, long mySequenceNum, long theirSequenceNum, long myAcknowledgementNum, long theirAcknowledgementNum,
                   SocketChannel channel, Packet referencePacket)
        {
            this.ipAndPort = ipAndPort;

            this.mySequenceNum = mySequenceNum;
            this.theirSequenceNum = theirSequenceNum;
            this.myAcknowledgementNum = myAcknowledgementNum;
            this.theirAcknowledgementNum = theirAcknowledgementNum;

            this.channel = channel;
            this.referencePacket = referencePacket;
        }

        public static void CloseTCB(TCB tcb)
        {
            tcb.CloseChannel();
            lock (tcbCache)
            {
                tcbCache.Remove(tcb.ipAndPort);
            }
        }

        public static void CloseAll()
        {
            lock (tcbCache)
            {
                var it = tcbCache.EntrySet().GetEnumerator();
                while (it.MoveNext())
                {
                    try
                    {
                        var tcb = (TCB)((IMapEntry)it.Current).Value;
                        tcb.CloseChannel();
                        tcbCache.Remove(tcb);
                    }
                    catch { }
                }
            }
        }

        private void CloseChannel()
        {
            try
            {
                channel.Close();
            }
            catch (IOException)
            {
                // Ignore
            }
        }

        public class TCBLRUCache : LRUCache<string, TCB>
        {
            public TCBLRUCache(int maxSize) : base(maxSize)
            {
            }

            public override void Cleanup(IMapEntry eldest)
            {
                ((TCB)eldest).CloseChannel();
            }
        }
    }
}
