using System.Diagnostics;
using System.Net.Sockets;

namespace MsmhToolsClass.MsmhAgnosticServer;

public partial class AgnosticProgram
{
    public class Fragment
    {
        public Mode FragmentMode { get; private set; } = Mode.Disable;
        public ChunkMode SniChunkMode { get; private set; }
        public int ChunksBeforeSNI { get; private set; } = 0;
        public int ChunksSNI { get; private set; } = 0;
        public int AntiPatternOffset { get; private set; } = 2;
        public int FragmentDelayMS { get; private set; } = 0;
        public string? DestHostname { get; set; }
        public int DestPort { get; set; }

        public event EventHandler<EventArgs>? OnChunkDetailsReceived;

        public Fragment() { }

        public void Set(Mode mode, int chunksBeforeSNI, ChunkMode sniChunkMode, int chunksSNI, int antiPatternOffset, int fragmentDelayMS)
        {
            FragmentMode = mode;
            ChunksBeforeSNI = chunksBeforeSNI;
            SniChunkMode = sniChunkMode;
            ChunksSNI = chunksSNI;
            AntiPatternOffset = antiPatternOffset;
            FragmentDelayMS = fragmentDelayMS;
        }

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

        public static ChunkMode GetChunkModeByName(string name)
        {
            name = name.Trim();
            if (name.Equals(nameof(ChunkMode.SNI), StringComparison.OrdinalIgnoreCase)) return ChunkMode.SNI;
            if (name.Equals(nameof(ChunkMode.SniExtension), StringComparison.OrdinalIgnoreCase)) return ChunkMode.SniExtension;
            if (name.Equals("Sni Extension", StringComparison.OrdinalIgnoreCase)) return ChunkMode.SniExtension;
            if (name.Equals(nameof(ChunkMode.AllExtensions), StringComparison.OrdinalIgnoreCase)) return ChunkMode.AllExtensions;
            if (name.Equals("All Extensions", StringComparison.OrdinalIgnoreCase)) return ChunkMode.AllExtensions;
            return ChunkMode.SNI; // Default
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

            public async Task SendAsync(Fragment bp)
            {
                try
                {
                    int offset = bp.AntiPatternOffset;
                    Random random = new();

                    // Anti Pattern Fragment Chunks
                    int beforeSniChunks = random.Next(bp.ChunksBeforeSNI - offset, bp.ChunksBeforeSNI + offset);
                    if (beforeSniChunks <= 0) beforeSniChunks = 1;
                    if (beforeSniChunks > Data.Length) beforeSniChunks = Data.Length;

                    int sniChunks = random.Next(bp.ChunksSNI - offset, bp.ChunksSNI + offset);
                    if (sniChunks <= 0) sniChunks = 1;
                    if (sniChunks > Data.Length) sniChunks = Data.Length;

                    //await TestAsync(Data, Socket, beforeSniChunks, sniChunks, offset, bp);

                    if (bp.SniChunkMode == ChunkMode.AllExtensions)
                        await SendDataInFragmentAllExtensionsAsync(Data, Socket, beforeSniChunks, sniChunks, offset, bp);
                    else if (bp.SniChunkMode == ChunkMode.SniExtension)
                        await SendDataInFragmentSniExtensionAsync(Data, Socket, beforeSniChunks, sniChunks, offset, bp);
                    else if (bp.SniChunkMode == ChunkMode.SNI)
                        await SendDataInFragmentSNIAsync(Data, Socket, beforeSniChunks, sniChunks, offset, bp);
                }
                catch (Exception) { }
            }

            private static async Task TestAsync(byte[] data, Socket socket, int beforeSniChunks, int sniChunks, int offset, Fragment bp)
            {
                //Debug.WriteLine("Send Data in TEST");
                // Create packets
                List<byte[]> packets = new();
                packets.Clear();

                SniReader sniReader = new(data);
                if (sniReader.HasSniExtension)
                {
                    int paddingSize = 2;
                    if (sniReader.HasSniPaddingExtension) paddingSize = 0;

                    SniModifire sniModifire = new(sniReader, paddingSize);
                    Debug.WriteLine($"------ {sniReader.HasSniExtension} L1: {sniReader.Data.Length} L2: {sniModifire.ModifiedData.Length}");

                    SniReader sniReader2 = new(sniModifire.ModifiedData);
                    Debug.WriteLine($"------ S1: {sniReader.HasSniPaddingExtension} S2: {sniReader2.HasSniPaddingExtension} R2: {sniReader2.ReasonPhrase}");

                    packets.Add(sniModifire.ModifiedData);
                    await SendPacketsAsync(sniModifire.ModifiedData, socket, bp, packets, string.Empty);
                }
                else
                {
                    packets.Add(data);
                    await SendPacketsAsync(data, socket, bp, packets, string.Empty);
                }
            }

            private static async Task SendDataInFragmentAllExtensionsAsync(byte[] data, Socket socket, int beforeSniChunks, int sniChunks, int offset, Fragment bp)
            {
                //Debug.WriteLine("SendDataInFragmentAllExtensionsAsync");
                // Create packets
                List<byte[]> packets = new();
                string serverName = string.Empty;

                try
                {
                    packets.Clear();
                    SniReader sniReader = new(data);

                    // Set Server Name For Event
                    if (sniReader.HasSni && sniReader.SniList.Count > 0) serverName = sniReader.SniList[0].ServerName;

                    if (beforeSniChunks == 1 && sniChunks == 1)
                    {
                        packets.Add(data);
                    }
                    else
                    {
                        if (sniReader.HasTlsExtensions && sniReader.AllExtensions.Data.Length > 0)
                        {
                            int sniStartIndex = sniReader.AllExtensions.StartIndex;
                            int sniEndIndex = sniReader.AllExtensions.StartIndex + sniReader.AllExtensions.Length;

                            // Create Packet Before SNI
                            if (sniStartIndex > 0)
                            {
                                byte[] beforeSNI = data[..sniStartIndex];
                                List<byte[]> chunkedbeforeSNI = ChunkDataNormal(beforeSNI, beforeSniChunks, offset);
                                packets.AddRange(chunkedbeforeSNI);
                            }

                            // Create SNI Packet
                            byte[] sni = data[sniStartIndex..sniEndIndex];
                            List<byte[]> chunkedSNI = ChunkDataNormal(sni, sniChunks, offset);
                            packets.AddRange(chunkedSNI);

                            // Create Packet After SNI
                            if (data.Length > sniEndIndex)
                            {
                                byte[] afterSni = data[sniEndIndex..];
                                packets.Add(afterSni);
                            }
                        }
                        else
                        {
                            packets.Add(data);
                        }
                    }
                }
                catch (Exception) { }

                await SendPacketsAsync(data, socket, bp, packets, serverName);
            }

            private static async Task SendDataInFragmentSniExtensionAsync(byte[] data, Socket socket, int beforeSniChunks, int sniChunks, int offset, Fragment bp)
            {
                //Debug.WriteLine("SendDataInFragmentSniExtensionAsync");
                // Create packets
                List<byte[]> packets = new();
                string serverName = string.Empty;

                try
                {
                    packets.Clear();
                    SniReader sniReader = new(data);

                    // Set Server Name For Event
                    if (sniReader.HasSni && sniReader.SniList.Count > 0) serverName = sniReader.SniList[0].ServerName;

                    if (beforeSniChunks == 1 && sniChunks == 1)
                    {
                        packets.Add(data);
                    }
                    else
                    {
                        if (sniReader.HasSniExtension && sniReader.SniExtensionList.Count > 0)
                        {
                            SniReader.SniExtension sniF = sniReader.SniExtensionList[0];
                            SniReader.SniExtension sniL = sniReader.SniExtensionList[^1];

                            int sniStartIndex = sniF.StartIndex;
                            int sniEndIndex = sniL.StartIndex + sniL.Length;

                            // Create Packet Before SNI
                            if (sniStartIndex > 0)
                            {
                                byte[] beforeSNI = data[..sniStartIndex];
                                List<byte[]> chunkedbeforeSNI = ChunkDataNormal(beforeSNI, beforeSniChunks, offset);
                                packets.AddRange(chunkedbeforeSNI);
                            }

                            // Create SNI Packet
                            byte[] sni = data[sniStartIndex..sniEndIndex];
                            List<byte[]> chunkedSNI = ChunkDataNormal(sni, sniChunks, offset);
                            packets.AddRange(chunkedSNI);

                            // Create Packet After SNI
                            if (data.Length > sniEndIndex)
                            {
                                byte[] afterSni = data[sniEndIndex..];
                                packets.Add(afterSni);
                            }
                        }
                        else
                        {
                            packets.Add(data);
                        }
                    }
                }
                catch (Exception) { }

                await SendPacketsAsync(data, socket, bp, packets, serverName);
            }

            private static async Task SendDataInFragmentSNIAsync(byte[] data, Socket socket, int beforeSniChunks, int sniChunks, int offset, Fragment bp)
            {
                //Debug.WriteLine("SendDataInFragmentSNIAsync");
                // Create packets
                List<byte[]> packets = new();
                string serverName = string.Empty;
                
                try
                {
                    packets.Clear();
                    SniReader sniReader = new(data);

                    if (beforeSniChunks == 1 && sniChunks == 1)
                    {
                        packets.Add(data);
                    }
                    else
                    {
                        if (sniReader.HasSni && sniReader.SniList.Count > 0)
                        {
                            SniReader.SNI sniF = sniReader.SniList[0];
                            SniReader.SNI sniL = sniReader.SniList[^1];

                            // Set Server Name For Event
                            serverName = sniF.ServerName;

                            int sniStartIndex = sniF.StartIndex;
                            int sniEndIndex = sniL.StartIndex + sniL.Length;

                            // Create Packet Before SNI
                            if (sniStartIndex > 0)
                            {
                                byte[] beforeSNI = data[..sniStartIndex];
                                List<byte[]> chunkedbeforeSNI = ChunkDataNormal(beforeSNI, beforeSniChunks, offset);
                                packets.AddRange(chunkedbeforeSNI);
                            }

                            // Create SNI Packet
                            byte[] sni = data[sniStartIndex..sniEndIndex];
                            List<byte[]> chunkedSNI = ChunkDataNormal(sni, sniChunks, offset);
                            packets.AddRange(chunkedSNI);

                            // Create Packet After SNI
                            if (data.Length > sniEndIndex)
                            {
                                byte[] afterSni = data[sniEndIndex..];
                                packets.Add(afterSni);
                            }
                        }
                        else
                        {
                            packets.Add(data);
                        }
                    }
                }
                catch (Exception) { }

                await SendPacketsAsync(data, socket, bp, packets, serverName);
            }

            private static List<byte[]> ChunkDataNormal(byte[] data, int chunks, int offset)
            {
                //Debug.WriteLine("ChunkDataNormal");
                // Create chunk packets
                Random random = new();
                List<byte[]> chunkPackets = new();

                try
                {
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
                            string msgEvent = $"Error, Creating Normal Packets: {ex.Message}";
                            Debug.WriteLine(msgEvent);
                            return chunkPackets;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Fragment ChunkDataNormal: " + ex.Message);
                }

                return chunkPackets;
            }

            private static List<byte[]> ChunkDataRandom(byte[] data, int fragmentChunks)
            {
                Debug.WriteLine("ChunkDataRandom");
                //// Calculate fragment chunks from size
                //int fragmentChunks = data.Length / fragmentSize;
                //if (fragmentChunks <= 0) fragmentChunks = 1;
                //if (fragmentChunks > data.Length) fragmentChunks = data.Length;

                // Create chunk packets
                List<byte[]> packets = new();
                
                try
                {
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
                }
                catch (Exception) { }

                return packets;
            }

            private static async Task SendPacketsAsync(byte[] data, Socket socket, Fragment bp, List<byte[]> packets, string serverName)
            {
                try
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
                            await socket.SendAsync(fragmentData, SocketFlags.None);
                            if (bp.FragmentDelayMS > 0) await Task.Delay(bp.FragmentDelayMS);
                        }
                        catch (Exception ex)
                        {
                            string msgEvent = $"Error, Send Packets: {ex.Message}";
                            Debug.WriteLine(msgEvent);
                            return;
                        }
                    }

                    if (packets.Count > 1 && !string.IsNullOrEmpty(serverName))
                    {
                        string chunkDetailsEvent = $"{bp.DestHostname}:{bp.DestPort} Length: {data.Length}, Chunks: {packets.Count}";
                        chunkDetailsEvent += $", SNI: {serverName}";
                        bp.OnChunkDetailsReceived?.Invoke(chunkDetailsEvent, EventArgs.Empty);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Fragment SendPacketsAsync: " + ex.Message);
                }
            }
        }

        private static List<int> GenerateRandomIndices(int minValue, int maxValue, int count)
        {
            Random random = new();
            HashSet<int> indicesSet = new();

            try
            {
                while (indicesSet.Count < count)
                {
                    indicesSet.Add(random.Next(minValue, maxValue));
                }
            }
            catch (Exception) { }

            return new List<int>(indicesSet);
        }

    }
}