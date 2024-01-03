using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Lang;
using Java.Nio;
using Java.Nio.Channels;
using Java.Util.Concurrent;
using static SdcMaui.Vpn.TCB;
using IOException = Java.IO.IOException;
using Thread = Java.Lang.Thread;

namespace SdcMaui.Vpn
{
    public class TCPInput : Java.Lang.Object, Java.Lang.IRunnable
    {
        private const string TAG = "TCPInput";
        private const int HEADER_SIZE = Packet.IP4_HEADER_SIZE + Packet.TCP_HEADER_SIZE;

        private ConcurrentLinkedQueue outputQueue;
        private Selector selector;

        public TCPInput(ConcurrentLinkedQueue outputQueue, Selector selector)
        {
            this.outputQueue = outputQueue;
            this.selector = selector;
        }

        public void Run()
        {
            try
            {
                Log.Debug(TAG, "Started");
                while (!Thread.Interrupted())
                {
                    int readyChannels = selector.Select();

                    if (readyChannels == 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    var keys = selector.SelectedKeys();
                    List<SelectionKey> keyIterator = keys.ToList();

                    foreach (var key in keys)
                    {
                        if (!Thread.Interrupted())
                        {
                            if (key.IsValid)
                            {
                                if (key.IsConnectable)
                                    ProcessConnect(key, keyIterator);
                                else if (key.IsReadable)
                                    processInput(key, keyIterator);
                            }
                        }
                    }
                }
            }
            catch (InterruptedException e)
            {
                Log.Info(TAG, "Stopping");
            }
            catch (IOException e)
            {
                Log.Warn(TAG, e.ToString(), e);
            }
        }

        private void ProcessConnect(SelectionKey key, List<SelectionKey> keyIterator)
        {
            TCB tcb = (TCB)key.Attachment();
            Packet referencePacket = tcb.referencePacket;
            try
            {
                if (tcb.channel.FinishConnect())
                {
                    if (keyIterator[0] != null)
                    {
                        keyIterator.RemoveAt(0);
                    }
                    tcb.status = TCBStatus.SYN_RECEIVED;

                    // TODO: Set MSS for receiving larger packets from the device
                    ByteBuffer responseBuffer = ByteBufferPool.acquire();
                    referencePacket.updateTCPBuffer(responseBuffer, (byte)(Packet.TCPHeader.SYN | Packet.TCPHeader.ACK),
                            tcb.mySequenceNum, tcb.myAcknowledgementNum, 0);
                    outputQueue.Offer(responseBuffer);

                    tcb.mySequenceNum++; // SYN counts as a byte
                    //key.InterestOps(Operations.Read);
                    key.InterestOps();
                }
            }
            catch (IOException e)
            {
                Log.Error(TAG, "Connection error: " + tcb.ipAndPort, e);
                ByteBuffer responseBuffer = ByteBufferPool.acquire();
                referencePacket.updateTCPBuffer(responseBuffer, (byte)Packet.TCPHeader.RST, 0, tcb.myAcknowledgementNum, 0);
                outputQueue.Offer(responseBuffer);
                TCB.CloseTCB(tcb);
            }
        }

        private void processInput(SelectionKey key, List<SelectionKey> keyIterator)
        {
            System.Console.WriteLine("TCP In");

            try
            {

                if (keyIterator[0] != null)
                {
                    keyIterator.RemoveAt(0);
                }

                ByteBuffer receiveBuffer = ByteBufferPool.acquire();
                // Leave space for the header
                receiveBuffer.Position(HEADER_SIZE);

                TCB tcb = (TCB)key.Attachment();
                lock (tcb)
                {
                    Packet referencePacket = tcb.referencePacket;
                    SocketChannel inputChannel = (SocketChannel)key.Channel();
                    int readBytes;
                    try
                    {
                        readBytes = inputChannel.Read(receiveBuffer);
                    }
                    catch (IOException e)
                    {
                        Log.Error(TAG, "Network read error: " + tcb.ipAndPort, e);
                        referencePacket.updateTCPBuffer(receiveBuffer, (byte)Packet.TCPHeader.RST, 0, tcb.myAcknowledgementNum, 0);
                        outputQueue.Offer(receiveBuffer);
                        TCB.CloseTCB(tcb);
                        return;
                    }

                    if (readBytes == -1)
                    {
                        // End of stream, stop waiting until we push more data
                        //key.InterestOps(0);
                        key.InterestOps();
                        tcb.waitingForNetworkData = false;

                        if (tcb.status != TCBStatus.CLOSE_WAIT)
                        {
                            ByteBufferPool.Release(receiveBuffer);
                            return;
                        }

                        tcb.status = TCBStatus.LAST_ACK;
                        referencePacket.updateTCPBuffer(receiveBuffer, (byte)Packet.TCPHeader.FIN, tcb.mySequenceNum, tcb.myAcknowledgementNum, 0);
                        tcb.mySequenceNum++; // FIN counts as a byte
                    }
                    else
                    {
                        // XXX: We should ideally be splitting segments by MTU/MSS, but this seems to work without
                        referencePacket.updateTCPBuffer(receiveBuffer, (byte)(Packet.TCPHeader.PSH | Packet.TCPHeader.ACK),
                                tcb.mySequenceNum, tcb.myAcknowledgementNum, readBytes);
                        tcb.mySequenceNum += readBytes; // Next sequence number
                        receiveBuffer.Position(HEADER_SIZE + readBytes);
                    }
                }

                outputQueue.Offer(receiveBuffer);
            }
            catch (Java.Lang.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
    }
}
