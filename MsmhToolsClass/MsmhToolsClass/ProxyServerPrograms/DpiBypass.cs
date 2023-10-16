using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace MsmhToolsClass.ProxyServerPrograms;

public partial class ProxyProgram
{
    public class DPIBypass
    {
        public Mode DPIBypassMode { get; private set; } = Mode.Disable;
        public ChunkMode DPIChunkMode { get; private set; }
        public int BeforeSniChunks { get; private set; } = 0;
        public int SniChunks { get; private set; } = 0;
        public int AntiPatternOffset { get; private set; } = 2;
        public int FragmentDelay { get; private set; } = 0;
        public string? DestHostname { get; set; }
        public int DestPort { get; set; }

        public event EventHandler<EventArgs>? OnChunkDetailsReceived;

        public DPIBypass() { }

        public void Set(Mode mode, int beforeSniChunks, ChunkMode chunkMode, int sniChunks, int antiPatternOffset, int fragmentDelay)
        {
            DPIBypassMode = mode;
            BeforeSniChunks = beforeSniChunks;
            DPIChunkMode = chunkMode;
            SniChunks = sniChunks;
            AntiPatternOffset = antiPatternOffset;
            FragmentDelay = fragmentDelay;
        }

        // Max Data Length = 65536
        private const int MaxDataLength = 65536;
        public enum Mode
        {
            Program,
            Disable
        }

        public enum ChunkMode
        {
            SNI,
            SniExtension,
            AllExtensions
        }

        public class ProgramMode
        {
            private byte[] Data { get; set; }
            private Socket Socket { get; set; }

            public ProgramMode(byte[] data, Socket socket)
            {
                Data = data;
                Socket = socket;
            }

            public void Send(DPIBypass bp)
            {
                int offset = bp.AntiPatternOffset;
                Random random = new();

                // Anti Pattern Fragment Chunks
                int beforeSniChunks = random.Next(bp.BeforeSniChunks - offset, bp.BeforeSniChunks + offset);
                if (beforeSniChunks <= 0) beforeSniChunks = 1;
                if (beforeSniChunks > Data.Length) beforeSniChunks = Data.Length;

                int sniChunks = random.Next(bp.SniChunks - offset, bp.SniChunks + offset);
                if (sniChunks <= 0) sniChunks = 1;
                if (sniChunks > Data.Length) sniChunks = Data.Length;

                //Test(Data, Socket, beforeSniChunks, sniChunks, offset, bp);

                if (bp.DPIChunkMode == ChunkMode.AllExtensions)
                    SendDataInFragmentAllExtensions(Data, Socket, beforeSniChunks, sniChunks, offset, bp);
                else if (bp.DPIChunkMode == ChunkMode.SniExtension)
                    SendDataInFragmentSniExtension(Data, Socket, beforeSniChunks, sniChunks, offset, bp);
                else if (bp.DPIChunkMode == ChunkMode.SNI)
                    SendDataInFragmentSNI(Data, Socket, beforeSniChunks, sniChunks, offset, bp);
            }

            private void Test(byte[] data, Socket socket, int beforeSniChunks, int sniChunks, int offset, DPIBypass bp)
            {
                Debug.WriteLine("Send Data in TEST");
                // Create packets
                List<byte[]> packets = new();
                packets.Clear();

                SniModifire sniModifire = new(data);
                if (sniModifire.HasSni)
                {
                    packets.Add(sniModifire.ModifiedData);
                    SendPackets(sniModifire.ModifiedData, socket, bp, packets);
                }
                else
                {
                    packets.Add(data);
                    SendPackets(data, socket, bp, packets);
                }

            }

            private void SendDataInFragmentAllExtensions(byte[] data, Socket socket, int beforeSniChunks, int sniChunks, int offset, DPIBypass bp)
            {
                //Debug.WriteLine("SendDataInFragmentAllExtensions");
                // Create packets
                List<byte[]> packets = new();
                packets.Clear();

                SniReader sniReader = new(data);

                if (beforeSniChunks == 1 && sniChunks == 1)
                {
                    packets.Add(data);
                }
                else
                {
                    if (sniReader.HasTlsExtensions)
                    {
                        int prevIndex;
                        int pos = 0;
                        SniReader.TlsExtensions allExtensions = sniReader.AllExtensions;

                        pos += allExtensions.StartIndex;
                        prevIndex = pos - allExtensions.StartIndex;

                        // Create packet before SNI
                        int beforeSniLength = allExtensions.StartIndex - prevIndex;
                        if (beforeSniLength > 0)
                        {
                            byte[] beforeSNI = new byte[beforeSniLength];
                            Buffer.BlockCopy(data, prevIndex, beforeSNI, 0, beforeSniLength);

                            List<byte[]> chunkedbeforeSNI = ChunkDataNormal(beforeSNI, beforeSniChunks, offset);
                            packets = packets.Concat(chunkedbeforeSNI).ToList();
                            //Debug.WriteLine($"{prevIndex} ======> {beforeSniLength}");
                        }

                        // Create SNI packet
                        List<byte[]> chunkedSNI = ChunkDataNormal(allExtensions.Data, sniChunks, offset);
                        packets = packets.Concat(chunkedSNI).ToList();

                        //Debug.WriteLine($"{beforeSniLength} ====== {sni.SniStartIndex}");
                        //Debug.WriteLine($"{sni.SniStartIndex} ======> {sni.SniStartIndex + sni.SniLength}");
                        Debug.WriteLine("==-----== " + (sniReader.AllExtensions.StartIndex + sniReader.AllExtensions.Length) + " of " + data.Length);
                        pos = allExtensions.StartIndex + allExtensions.Length;

                        // Create packet after SNI
                        if (pos < data.Length)
                        {
                            int afterSniStartIndex = pos;
                            int afterSniLength = data.Length - pos;
                            byte[] afterSni = new byte[afterSniLength];
                            Buffer.BlockCopy(data, afterSniStartIndex, afterSni, 0, afterSniLength);
                            packets.Add(afterSni);

                            //Debug.WriteLine($"{sni.SniStartIndex + sni.SniLength} ====== {afterSniStartIndex}");
                            //Debug.WriteLine($"{afterSniStartIndex} ======> {afterSniStartIndex + afterSniLength}");
                            //Debug.WriteLine($"{afterSniStartIndex + afterSniLength} ====== {data.Length}");
                        }
                    }
                    else
                    {
                        packets.Add(data);
                    }
                }

                SendPackets(data, socket, bp, packets);
            }

            private void SendDataInFragmentSniExtension(byte[] data, Socket socket, int beforeSniChunks, int sniChunks, int offset, DPIBypass bp)
            {
                //Debug.WriteLine("SendDataInFragmentSniExtension");
                // Create packets
                List<byte[]> packets = new();
                packets.Clear();

                SniReader sniReader = new(data);
                if (sniReader.SniExtensionList.Count > 1) Debug.WriteLine($"=======================> We Have {sniReader.SniExtensionList.Count} SNI Extensions.");

                if (beforeSniChunks == 1 && sniChunks == 1)
                {
                    packets.Add(data);
                }
                else
                {
                    if (sniReader.HasSniExtension)
                    {
                        int prevIndex;
                        int pos = 0;
                        for (int n = 0; n < sniReader.SniExtensionList.Count; n++)
                        {
                            SniReader.SniExtension sniExtension = sniReader.SniExtensionList[n];

                            pos += sniExtension.StartIndex;
                            prevIndex = pos - sniExtension.StartIndex;

                            // Create packet before SNI
                            int beforeSniLength = sniExtension.StartIndex - prevIndex;
                            if (beforeSniLength > 0)
                            {
                                byte[] beforeSNI = new byte[beforeSniLength];
                                Buffer.BlockCopy(data, prevIndex, beforeSNI, 0, beforeSniLength);

                                List<byte[]> chunkedbeforeSNI = ChunkDataNormal(beforeSNI, beforeSniChunks, offset);
                                packets = packets.Concat(chunkedbeforeSNI).ToList();
                                //Debug.WriteLine($"{prevIndex} ======> {beforeSniLength}");
                            }

                            // Create SNI packet
                            List<byte[]> chunkedSNI = ChunkDataNormal(sniExtension.Data, sniChunks, offset);
                            packets = packets.Concat(chunkedSNI).ToList();

                            //Debug.WriteLine($"{beforeSniLength} ====== {sni.SniStartIndex}");
                            //Debug.WriteLine($"{sni.SniStartIndex} ======> {sni.SniStartIndex + sni.SniLength}");

                            pos = sniExtension.StartIndex + sniExtension.Length;

                            // Last round
                            if (n == sniReader.SniExtensionList.Count - 1)
                            {
                                // Create packet after SNI
                                if (pos < data.Length)
                                {
                                    int afterSniStartIndex = pos;
                                    int afterSniLength = data.Length - pos;
                                    byte[] afterSni = new byte[afterSniLength];
                                    Buffer.BlockCopy(data, afterSniStartIndex, afterSni, 0, afterSniLength);
                                    packets.Add(afterSni);

                                    //Debug.WriteLine($"{sni.SniStartIndex + sni.SniLength} ====== {afterSniStartIndex}");
                                    //Debug.WriteLine($"{afterSniStartIndex} ======> {afterSniStartIndex + afterSniLength}");
                                    //Debug.WriteLine($"{afterSniStartIndex + afterSniLength} ====== {data.Length}");
                                }
                            }
                        }
                    }
                    else
                    {
                        packets.Add(data);
                    }
                }

                SendPackets(data, socket, bp, packets);
            }

            private void SendDataInFragmentSNI(byte[] data, Socket socket, int beforeSniChunks, int sniChunks, int offset, DPIBypass bp)
            {
                //Debug.WriteLine("SendDataInFragmentSNI");
                // Create packets
                List<byte[]> packets = new();
                packets.Clear();

                SniReader sniReader = new(data);
                if (sniReader.SniList.Count > 1) Debug.WriteLine($"=======================> We Have {sniReader.SniList.Count} SNIs.");

                if (beforeSniChunks == 1 && sniChunks == 1)
                {
                    packets.Add(data);
                }
                else
                {
                    if (sniReader.HasSni)
                    {
                        int prevIndex;
                        int pos = 0;
                        for (int n = 0; n < sniReader.SniList.Count; n++)
                        {
                            SniReader.SNI sni = sniReader.SniList[n];

                            pos += sni.StartIndex;
                            prevIndex = pos - sni.StartIndex;

                            // Create packet before SNI
                            int beforeSniLength = sni.StartIndex - prevIndex;
                            if (beforeSniLength > 0)
                            {
                                byte[] beforeSNI = new byte[beforeSniLength];
                                Buffer.BlockCopy(data, prevIndex, beforeSNI, 0, beforeSniLength);

                                List<byte[]> chunkedbeforeSNI = ChunkDataNormal(beforeSNI, beforeSniChunks, offset);
                                packets = packets.Concat(chunkedbeforeSNI).ToList();
                                //Debug.WriteLine($"{prevIndex} ======> {beforeSniLength}");
                            }

                            // Create SNI packet
                            List<byte[]> chunkedSNI = ChunkDataNormal(sni.Data, sniChunks, offset);
                            packets = packets.Concat(chunkedSNI).ToList();

                            //Debug.WriteLine($"{beforeSniLength} ====== {sni.SniStartIndex}");
                            //Debug.WriteLine($"{sni.SniStartIndex} ======> {sni.SniStartIndex + sni.SniLength}");

                            pos = sni.StartIndex + sni.Length;

                            // Last round
                            if (n == sniReader.SniList.Count - 1)
                            {
                                // Create packet after SNI
                                if (pos < data.Length)
                                {
                                    int afterSniStartIndex = pos;
                                    int afterSniLength = data.Length - pos;
                                    byte[] afterSni = new byte[afterSniLength];
                                    Buffer.BlockCopy(data, afterSniStartIndex, afterSni, 0, afterSniLength);
                                    packets.Add(afterSni);

                                    //Debug.WriteLine($"{sni.SniStartIndex + sni.SniLength} ====== {afterSniStartIndex}");
                                    //Debug.WriteLine($"{afterSniStartIndex} ======> {afterSniStartIndex + afterSniLength}");
                                    //Debug.WriteLine($"{afterSniStartIndex + afterSniLength} ====== {data.Length}");
                                }
                            }
                        }
                    }
                    else
                    {
                        packets.Add(data);
                    }
                }

                SendPackets(data, socket, bp, packets);
            }

            private List<byte[]> ChunkDataNormal(byte[] data, int chunks, int offset)
            {
                //Debug.WriteLine("ChunkDataNormal");
                // Create chunk packets
                Random random = new();
                List<byte[]> chunkPackets = new();
                chunkPackets.Clear();
                int prevIndex;
                int nn = 0;
                int sum = 0;
                for (int n = 0; n < data.Length; n++)
                {
                    try
                    {
                        // Anti Pattern Fragment Size
                        int fragmentSize = data.Length / chunks;

                        int fragmentSizeOut = random.Next(fragmentSize - offset, fragmentSize + offset);
                        if (fragmentSizeOut <= 0) fragmentSizeOut = 1;
                        if (fragmentSizeOut > data.Length) fragmentSizeOut = data.Length;
                        nn += fragmentSizeOut;

                        if (nn > data.Length)
                        {
                            fragmentSizeOut = data.Length - (nn - fragmentSizeOut);
                            //Debug.WriteLine(fragmentSizeOut);
                        }
                        //Debug.WriteLine(fragmentSizeOut);

                        sum += fragmentSizeOut;
                        byte[] fragmentData = new byte[fragmentSizeOut];
                        prevIndex = sum - fragmentSizeOut;
                        Buffer.BlockCopy(data, prevIndex, fragmentData, 0, fragmentSizeOut);
                        chunkPackets.Add(fragmentData);

                        if (sum >= data.Length) break;
                    }
                    catch (Exception ex)
                    {
                        chunkPackets.Clear();
                        string msgEvent = $"Error, Creating normal packets: {ex.Message}";
                        Debug.WriteLine(msgEvent);
                        return chunkPackets;
                    }
                }

                return chunkPackets;
            }

            private List<byte[]> ChunkDataNormal2(byte[] data, int fragmentSize)
            {
                Debug.WriteLine("ChunkDataNormal2");
                // Create chunk packets
                List<byte[]> chunkPackets = new();
                chunkPackets.Clear();
                var fragments = data.Chunk(fragmentSize);
                for (int n = 0; n < fragments.Count(); n++)
                {
                    try
                    {
                        byte[] fragment = fragments.ToArray()[n];
                        chunkPackets.Add(fragment);
                    }
                    catch (Exception ex)
                    {
                        chunkPackets.Clear();
                        string msgEvent = $"Error, Creating normal2 packets: {ex.Message}";
                        Debug.WriteLine(msgEvent);
                        return chunkPackets;
                    }
                }

                return chunkPackets;
            }

            private List<byte[]> ChunkDataNormal3(byte[] data, int fragmentSize)
            {
                Debug.WriteLine("ChunkDataNormal3");
                // Create chunk packets
                List<byte[]> chunkPackets = new();
                chunkPackets.Clear();
                var fragments = ChunkViaMemory(data, fragmentSize);
                for (int n = 0; n < fragments.Count(); n++)
                {
                    try
                    {
                        byte[] fragment = fragments.ToArray()[n].ToArray();
                        chunkPackets.Add(fragment);
                    }
                    catch (Exception ex)
                    {
                        chunkPackets.Clear();
                        string msgEvent = $"Error, Creating normal3 packets: {ex.Message}";
                        Debug.WriteLine(msgEvent);
                        return chunkPackets;
                    }
                }

                return chunkPackets;
            }

            private List<byte[]> ChunkDataRandom(byte[] data, int fragmentChunks)
            {
                Debug.WriteLine("ChunkDataRandom");
                //// Calculate fragment chunks from size
                //int fragmentChunks = data.Length / fragmentSize;
                //if (fragmentChunks <= 0) fragmentChunks = 1;
                //if (fragmentChunks > data.Length) fragmentChunks = data.Length;

                // Create chunk packets
                List<byte[]> packets = new();
                packets.Clear();
                fragmentChunks = Math.Min(fragmentChunks, data.Length);
                List<int> indices;
                if (fragmentChunks < data.Length)
                    indices = GenerateRandomIndices(1, data.Length - 1, fragmentChunks - 1);
                else
                    indices = Enumerable.Range(0, data.Length - 1).ToList();
                indices.Sort();

                int prevIndex = 0;
                for (int n = 0; n < indices.Count; n++)
                {
                    try
                    {
                        int index = indices[n];
                        byte[] fragmentData = new byte[index - prevIndex];
                        Buffer.BlockCopy(data, prevIndex, fragmentData, 0, fragmentData.Length);
                        packets.Add(fragmentData);
                        prevIndex = index;
                    }
                    catch (Exception ex)
                    {
                        packets.Clear();
                        string msgEvent = $"Error, Creating random packets: {ex.Message}";
                        Debug.WriteLine(msgEvent);
                        return packets;
                    }
                }

                try
                {
                    byte[] lastFragmentData = new byte[data.Length - prevIndex];
                    Buffer.BlockCopy(data, prevIndex, lastFragmentData, 0, lastFragmentData.Length);
                    packets.Add(lastFragmentData);
                }
                catch (Exception ex)
                {
                    packets.Clear();
                    string msgEvent = $"Error, Creating last random packet: {ex.Message}";
                    Debug.WriteLine(msgEvent);
                    return packets;
                }

                return packets;
            }

            private void SendPackets(byte[] data, Socket socket, DPIBypass bp, List<byte[]> packets)
            {
                // Check packets
                int allLength = 0;
                for (int i = 0; i < packets.Count; i++)
                    allLength += packets[i].Length;

                if (allLength != data.Length)
                {
                    Debug.WriteLine($"{allLength} == {data.Length}, Chunks: {packets.Count}");
                    packets.Clear();
                    return;
                }

                // Send packets
                for (int i = 0; i < packets.Count; i++)
                {
                    try
                    {
                        byte[] fragmentData = packets[i];
                        if (socket == null) return;
                        socket.Send(fragmentData);
                        if (bp.FragmentDelay > 0)
                            Task.Delay(bp.FragmentDelay).Wait();
                    }
                    catch (Exception ex)
                    {
                        string msgEvent = $"Error, Send Packets: {ex.Message}";
                        Debug.WriteLine(msgEvent);
                        return;
                    }
                }

                string chunkDetailsEvent = $"{bp.DestHostname}:{bp.DestPort} Length: {data.Length}";
                if (packets.Count > 1)
                    chunkDetailsEvent += $", Chunks: {packets.Count}";
                bp.OnChunkDetailsReceived?.Invoke(chunkDetailsEvent, EventArgs.Empty);
            }
        }

        private static List<int> GenerateRandomIndices(int minValue, int maxValue, int count)
        {
            Random random = new();
            HashSet<int> indicesSet = new();

            while (indicesSet.Count < count)
            {
                indicesSet.Add(random.Next(minValue, maxValue));
            }

            return new List<int>(indicesSet);
        }

        private static IEnumerable<Memory<T>> ChunkViaMemory<T>(T[] data, int size)
        {
            var chunks = data.Length / size;
            for (int i = 0; i < chunks; i++)
            {
                yield return data.AsMemory(i * size, size);
            }
            var leftOver = data.Length % size;
            if (leftOver > 0)
            {
                yield return data.AsMemory(chunks * size, leftOver);
            }
        }

    }
}