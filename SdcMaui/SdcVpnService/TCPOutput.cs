using Android.Util;
using Java.IO;
using Java.Lang;
using Java.Net;
using Java.Nio;
using Java.Nio.Channels;
using Java.Util;
using Java.Util.Concurrent;
using static SdcMaui.Vpn.Packet;
using Exception = Java.Lang.Exception;
using IOException = Java.IO.IOException;
using Random = Java.Util.Random;
using Thread = Java.Lang.Thread;

namespace SdcMaui.Vpn
{
    public class TCPOutput : Java.Lang.Object, Java.Lang.IRunnable
    {
        private const string TAG = "TCPOutput";

        private SdcVpnService vpnService;
        private ConcurrentLinkedQueue inputQueue;
        private ConcurrentLinkedQueue outputQueue;
        private Selector selector;

        private Random random = new Random();

        public TCPOutput(ConcurrentLinkedQueue inputQueue, ConcurrentLinkedQueue outputQueue,
                         Selector selector, SdcVpnService vpnService)
        {
            this.inputQueue = inputQueue;
            this.outputQueue = outputQueue;
            this.selector = selector;
            this.vpnService = vpnService;
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

                    ByteBuffer payloadBuffer = currentPacket.backingBuffer;
                    currentPacket.backingBuffer = null;
                    ByteBuffer responseBuffer = ByteBufferPool.acquire();

                    InetAddress destinationAddress = currentPacket.ip4Header.destinationAddress;

                    TCPHeader tcpHeader = currentPacket.tcpHeader;
                    int destinationPort = tcpHeader.destinationPort;
                    int sourcePort = tcpHeader.sourcePort;

                    Java.Lang.String ipAndPort = new Java.Lang.String(destinationAddress.HostAddress + ":" +
                             destinationPort + ":" + sourcePort);

                    System.Console.WriteLine("TCP Out: " + ipAndPort);

                    TCB tcb = TCB.GetTCB(ipAndPort);
                    if (tcb == null)
                        InitializeConnection(ipAndPort, destinationAddress, destinationPort,
                                currentPacket, tcpHeader, responseBuffer);
                    else if (tcpHeader.isSYN())
                        processDuplicateSYN(tcb, tcpHeader, responseBuffer);
                    else if (tcpHeader.isRST())
                        CloseCleanly(tcb, responseBuffer);
                    else if (tcpHeader.isFIN())
                        processFIN(tcb, tcpHeader, responseBuffer);
                    else if (tcpHeader.isACK())
                        processACK(tcb, tcpHeader, payloadBuffer, responseBuffer);

                    // XXX: cleanup later
                    if (responseBuffer.Position() == 0)
                        ByteBufferPool.Release(responseBuffer);
                    ByteBufferPool.Release(payloadBuffer);
                }
            }
            catch (InterruptedException e)
            {
                Log.Info(TAG, "Stopping");
            }
            catch (IOException e)
            {
                Log.Error(TAG, e.ToString(), e);
            }
            finally
            {
                TCB.CloseAll();
            }
        }

        private void InitializeConnection(Java.Lang.String ipAndPort, InetAddress destinationAddress, int destinationPort,
                                      Packet currentPacket, TCPHeader tcpHeader, ByteBuffer responseBuffer)

        {
            currentPacket.SwapSourceAndDestination();
            if (tcpHeader.isSYN())
            {
                SocketChannel outputChannel = SocketChannel.Open();
                outputChannel.ConfigureBlocking(false);
                vpnService.Protect(outputChannel.Socket());

                TCB tcb = new TCB(ipAndPort, random.NextInt(Short.MaxValue + 1), tcpHeader.sequenceNumber, tcpHeader.sequenceNumber + 1,
                        tcpHeader.acknowledgementNumber, outputChannel, currentPacket);
                TCB.PutTCB(ipAndPort, tcb);

                try
                {
                    outputChannel.Connect(new InetSocketAddress(destinationAddress, destinationPort));
                    if (outputChannel.FinishConnect())
                    {
                        tcb.status = TCB.TCBStatus.SYN_RECEIVED;
                        // TODO: Set MSS for receiving larger packets from the device
                        currentPacket.updateTCPBuffer(responseBuffer, (byte)(TCPHeader.SYN | TCPHeader.ACK),
                                tcb.mySequenceNum, tcb.myAcknowledgementNum, 0);
                        tcb.mySequenceNum++; // SYN counts as a byte
                    }
                    else
                    {
                        tcb.status = TCB.TCBStatus.SYN_SENT;
                        selector.Wakeup();
                        tcb.selectionKey = outputChannel.Register(selector, Operations.Connect, tcb);
                        return;
                    }
                }
                catch (IOException e)
                {
                    Log.Error(TAG, "Connection error: " + ipAndPort, e);
                    currentPacket.updateTCPBuffer(responseBuffer, (byte)TCPHeader.RST, 0, tcb.myAcknowledgementNum, 0);
                    TCB.CloseTCB(tcb);
                }
            }
            else
            {
                currentPacket.updateTCPBuffer(responseBuffer, (byte)TCPHeader.RST,
                        0, tcpHeader.sequenceNumber + 1, 0);
            }

            outputQueue.Offer(responseBuffer);
        }

        private void processDuplicateSYN(TCB tcb, TCPHeader tcpHeader, ByteBuffer responseBuffer)
        {
            lock (tcb)
            {
                if (tcb.status == TCB.TCBStatus.SYN_SENT)
                {
                    tcb.myAcknowledgementNum = tcpHeader.sequenceNumber + 1;
                    return;
                }
            }

            SendRST(tcb, 1, responseBuffer);
        }

        private void processFIN(TCB tcb, TCPHeader tcpHeader, ByteBuffer responseBuffer)
        {
            lock (tcb)
            {
                Packet referencePacket = tcb.referencePacket;
                tcb.myAcknowledgementNum = tcpHeader.sequenceNumber + 1;
                tcb.theirAcknowledgementNum = tcpHeader.acknowledgementNumber;

                if (tcb.waitingForNetworkData)
                {
                    tcb.status = TCB.TCBStatus.CLOSE_WAIT;
                    referencePacket.updateTCPBuffer(responseBuffer, (byte)TCPHeader.ACK,
                            tcb.mySequenceNum, tcb.myAcknowledgementNum, 0);
                }
                else
                {
                    tcb.status = TCB.TCBStatus.LAST_ACK;
                    referencePacket.updateTCPBuffer(responseBuffer, (byte)(TCPHeader.FIN | TCPHeader.ACK),
                            tcb.mySequenceNum, tcb.myAcknowledgementNum, 0);
                    tcb.mySequenceNum++; // FIN counts as a byte
                }
            }

            outputQueue.Offer(responseBuffer);
        }

        private void processACK(TCB tcb, TCPHeader tcpHeader, ByteBuffer payloadBuffer, ByteBuffer responseBuffer)
        {
            try
            {
                int payloadSize = payloadBuffer.Limit() - payloadBuffer.Position();

                lock (tcb)
                {
                    SocketChannel outputChannel = tcb.channel;
                    if (tcb.status == TCB.TCBStatus.SYN_RECEIVED)
                    {
                        tcb.status = TCB.TCBStatus.ESTABLISHED;

                        selector.Wakeup();
                        tcb.selectionKey = outputChannel.Register(selector, Operations.Read, tcb);
                        tcb.waitingForNetworkData = true;
                    }
                    else if (tcb.status == TCB.TCBStatus.LAST_ACK)
                    {
                        CloseCleanly(tcb, responseBuffer);
                        return;
                    }

                    if (payloadSize == 0) return; // Empty ACK, ignore

                    if (!tcb.waitingForNetworkData)
                    {
                        selector.Wakeup();
                        // tcb.selectionKey.InterestOps(SelectionKey.OpRead);
                        tcb.selectionKey.InterestOps();
                        tcb.waitingForNetworkData = true;
                    }

                    // Forward to remote server
                    try
                    {
                        while (payloadBuffer.HasRemaining)
                            outputChannel.Write(payloadBuffer);
                    }
                    catch (IOException e)
                    {
                        Log.Error(TAG, "Network write error: " + tcb.ipAndPort, e);
                        SendRST(tcb, payloadSize, responseBuffer);
                        return;
                    }

                    // TODO: We don't expect out-of-order packets, but verify
                    tcb.myAcknowledgementNum = tcpHeader.sequenceNumber + payloadSize;
                    tcb.theirAcknowledgementNum = tcpHeader.acknowledgementNumber;
                    Packet referencePacket = tcb.referencePacket;
                    referencePacket.updateTCPBuffer(responseBuffer, (byte)TCPHeader.ACK, tcb.mySequenceNum, tcb.myAcknowledgementNum, 0);
                }

                outputQueue.Offer(responseBuffer);
            }
            catch(Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        private void SendRST(TCB tcb, int prevPayloadSize, ByteBuffer buffer)
        {
            tcb.referencePacket.updateTCPBuffer(buffer, (byte)TCPHeader.RST, 0, tcb.myAcknowledgementNum + prevPayloadSize, 0);
            outputQueue.Offer(buffer);
            TCB.CloseTCB(tcb);
        }

        private void CloseCleanly(TCB tcb, ByteBuffer buffer)
        {
            ByteBufferPool.Release(buffer);
            TCB.CloseTCB(tcb);
        }
    }
}
