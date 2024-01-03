using System;
using System.Collections.Generic;
using Android.Util;
using Java.IO;
using Java.Lang;
using Java.Net;
using Java.Nio;
using Java.Nio.Channels;
using Java.Util;
using Java.Util.Concurrent;
using IOException = Java.IO.IOException;
using Thread = Java.Lang.Thread;

namespace SdcMaui.Vpn
{
    public class UDPOutput : Java.Lang.Object, Java.Lang.IRunnable
    {
        private const string TAG = "UDPOutput";

        private SdcVpnService vpnService;
        private ConcurrentLinkedQueue inputQueue;
        private Selector selector;

        private const int MAX_CACHE_SIZE = 50;

        private LRUChannelCache channelCache;

        public UDPOutput(ConcurrentLinkedQueue inputQueue, Selector selector, SdcVpnService vpnService)
        {
            this.inputQueue = inputQueue;
            this.selector = selector;
            this.vpnService = vpnService;
            this.channelCache = new LRUChannelCache(this, MAX_CACHE_SIZE);
        }

        public void Run()
        {
            Log.Info(TAG, "Started");
            try
            {
                Thread currentThread = Thread.CurrentThread();
                while (true)
                {
                    Packet currentPacket;
                    // TODO: Block when not connected
                    do
                    {
                        currentPacket = (Packet)inputQueue.Poll();
                        if (currentPacket != null)
                            break;
                        Thread.Sleep(10);
                    } while (!currentThread.IsInterrupted);

                    if (currentThread.IsInterrupted)
                        break;

                    InetAddress destinationAddress = currentPacket.ip4Header.destinationAddress;
                    int destinationPort = currentPacket.udpHeader.destinationPort;
                    int sourcePort = currentPacket.udpHeader.sourcePort;

                    Java.Lang.String ipAndPort = new Java.Lang.String(destinationAddress.HostAddress + ":" + destinationPort + ":" + sourcePort);

                    System.Console.WriteLine("UDP Out: " + ipAndPort);

                    DatagramChannel outputChannel = (DatagramChannel)channelCache.Get(ipAndPort);

                    if (outputChannel == null)
                    {
                        outputChannel = DatagramChannel.Open();
                        vpnService.Protect(outputChannel.Socket());
                        try
                        {
                            outputChannel.Connect(new InetSocketAddress(destinationAddress, destinationPort));
                        }
                        catch (IOException e)
                        {
                            Log.Error(TAG, "Connection error: " + ipAndPort, e);
                            closeChannel(outputChannel);
                            ByteBufferPool.Release(currentPacket.backingBuffer);
                            continue;
                        }

                        outputChannel.ConfigureBlocking(false);
                        currentPacket.SwapSourceAndDestination();

                        selector.Wakeup();
                        outputChannel.Register(selector, Java.Nio.Channels.Operations.Read, currentPacket);
                        
                        channelCache.Put(ipAndPort, outputChannel);
                    }

                    try
                    {
                        ByteBuffer payloadBuffer = currentPacket.backingBuffer;
                        while (payloadBuffer.HasRemaining)
                            outputChannel.Write(payloadBuffer);
                    }
                    catch (IOException e)
                    {
                        Log.Error(TAG, "Network write error: " + ipAndPort, e);
                        channelCache.Remove(ipAndPort);
                        closeChannel(outputChannel);
                    }

                    ByteBufferPool.Release(currentPacket.backingBuffer);
                }
            }
            catch (InterruptedException e)
            {
                Log.Info(TAG, "Stopping");
            }
            catch (IOException e)
            {
                Log.Info(TAG, e.ToString(), e);
            }
            finally
            {
                closeAll();
            }
        }

        public void Dispose()
        {
            closeAll();
        }

        private void closeAll()
        {
            var enumerator = channelCache.EntrySet().GetEnumerator();

            while (enumerator.MoveNext())
            {
                try
                {
                    var o = (DatagramChannel)enumerator.Current;
                    closeChannel(o);
                }
                catch { }
            }
        }

        private void closeChannel(DatagramChannel channel)
        {
            try
            {
                channel.Close();
            }
            catch (IOException e)
            {
                // Ignore
            }
        }

        public class LRUChannelCache : LRUCache<string, DatagramChannel>
        {
            UDPOutput output;

            public LRUChannelCache(UDPOutput output, int maxSize) : base(maxSize)
            {
                this.output = output;
            }

            public override void Cleanup(IMapEntry eldest)
            {
                output.closeChannel((DatagramChannel)eldest.Value);
            }
        }
    }
}
