using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MsmhTools.HTTPProxyServer
{
    public partial class HTTPProxyServer
    {
        public partial class Program
        {
            //======================================= DPI Bypass Support
            public class DPIBypass
            {
                public Mode DPIBypassMode { get; private set; } = Mode.Disable;
                public int FirstPartOfDataLength { get; private set; } = 0;
                public int FragmentSize { get; private set; } = 0;
                public int FragmentChunks { get; private set; } = 0;
                public int AntiPatternOffset { get; set; } = 5;
                public int FragmentDelay { get; private set; } = 0;

                /// <summary>
                /// Don't chunk the request when size is 65536.
                /// </summary>
                public bool DontChunkTheBiggestRequest { get; set; } = false;
                public bool SendInRandom { get; set; } = false;
                public string? DestHostname { get; set; }
                public int DestPort { get; set; }

                public event EventHandler<EventArgs>? OnChunkDetailsReceived;

                public DPIBypass() { }

                public void Set(Mode mode, int firstPartOfDataLength, int fragmentSize, int fragmentChunks, int fragmentDelay)
                {
                    DPIBypassMode = mode;
                    FirstPartOfDataLength = firstPartOfDataLength;
                    FragmentSize = fragmentSize;
                    FragmentChunks = fragmentChunks;
                    FragmentDelay = fragmentDelay;
                }

                // Max Data Length = 65536
                private readonly int MaxDataLength = 65536;
                public enum Mode
                {
                    Program,
                    Disable
                }

                public class ProgramMode
                {
                    private byte[] Data;
                    private Socket Socket;

                    public ProgramMode(byte[] data, Socket socket)
                    {
                        Data = data;
                        Socket = socket;
                    }

                    public void Send(DPIBypass bp)
                    {
                        int offset = bp.AntiPatternOffset;
                        Random random = new();

                        if (!bp.SendInRandom)
                        {
                            // Normal Mode
                            int fragmentSizeOut;

                            if (Data.Length <= bp.FirstPartOfDataLength)
                            {
                                // Anti Pattern Fragment Size
                                int fragmentSize = 1;
                                if (bp.FragmentSize > 1)
                                    fragmentSize = random.Next(bp.FragmentSize - offset, bp.FragmentSize + offset);
                                else
                                    offset = 0;

                                if (fragmentSize <= 0) fragmentSize = 1;
                                if (fragmentSize > Data.Length) fragmentSize = Data.Length;

                                fragmentSizeOut = Math.Min(fragmentSize, Data.Length);
                            }
                            else
                            {
                                // Anti Pattern Fragment Chunks
                                int fragmentChunks = random.Next(bp.FragmentChunks - offset, bp.FragmentChunks + offset);

                                if (fragmentChunks <= 0) fragmentChunks = 1;
                                if (fragmentChunks > Data.Length) fragmentChunks = Data.Length;

                                fragmentSizeOut = Data.Length / fragmentChunks;
                            }

                            if (fragmentSizeOut <= 0) fragmentSizeOut = 1;
                            if (fragmentSizeOut > bp.MaxDataLength) fragmentSizeOut = bp.MaxDataLength;

                            if (bp.DontChunkTheBiggestRequest)
                                if (Data.Length == bp.MaxDataLength) fragmentSizeOut = bp.MaxDataLength;

                            SendDataInNormalFragment1(Data, Socket, fragmentSizeOut, offset, bp);
                        }
                        else
                        {
                            // Random Mode
                            if (Data.Length <= bp.FirstPartOfDataLength)
                            {
                                // Anti Pattern Fragment Size
                                int fragmentSize = 1;
                                if (bp.FragmentSize > 1)
                                    fragmentSize = random.Next(bp.FragmentSize - offset, bp.FragmentSize + offset);
                                else
                                    offset = 0;

                                if (fragmentSize <= 0) fragmentSize = 1;
                                if (fragmentSize > Data.Length) fragmentSize = Data.Length;

                                int fragmentSizeOut = Math.Min(fragmentSize, Data.Length);

                                if (fragmentSizeOut <= 0) fragmentSizeOut = 1;
                                if (fragmentSizeOut > bp.MaxDataLength) fragmentSizeOut = bp.MaxDataLength;

                                SendDataInNormalFragment1(Data, Socket, fragmentSizeOut, offset, bp);
                            }
                            else
                            {
                                // Anti Pattern Fragment Chunks
                                int fragmentChunks = random.Next(bp.FragmentChunks - offset, bp.FragmentChunks + offset);

                                if (fragmentChunks <= 0) fragmentChunks = 1;
                                if (fragmentChunks > Data.Length) fragmentChunks = Data.Length;

                                SendDataInRandomFragment(Data, Socket, fragmentChunks, bp);
                            }
                        }
                    }
                }

                private static void SendDataInNormalFragment1(byte[] data, Socket socket, int fragmentSize, int offset, DPIBypass bp)
                {
                    // Create packets
                    Random random = new();
                    List<byte[]> packets = new();
                    packets.Clear();
                    int prevIndex = 0;
                    int nn = 0;
                    int sum = 0;
                    for (int n = 0; n < data.Length; n++)
                    {
                        try
                        {
                            // Anti Pattern Fragment Size
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
                            packets.Add(fragmentData);

                            if (sum >= data.Length) break;
                        }
                        catch (Exception ex)
                        {
                            packets.Clear();
                            string msgEvent = $"Error, Creating normal packets: {ex.Message}";
                            Debug.WriteLine(msgEvent);
                            return;
                        }
                    }

                    // Check packets
                    int allLength = 0;
                    for (int i = 0; i < packets.Count; i++)
                        allLength += packets[i].Length;
                    Debug.WriteLine($"{allLength} == {data.Length}, Chunks: {packets.Count}");
                    if (allLength != data.Length)
                    {
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
                            string msgEvent = $"Error, SendDataInFragment1: {ex.Message}";
                            Debug.WriteLine(msgEvent);
                            return;
                        }
                    }

                    string chunkDetailsEvent = $"{bp.DestHostname}:{bp.DestPort} Length: {data.Length}, Chunks: {packets.Count}";
                    bp.OnChunkDetailsReceived?.Invoke(chunkDetailsEvent, EventArgs.Empty);
                }

                private static void SendDataInNormalFragment2(byte[] data, Socket socket, int fragmentSize, int fragmentDelay)
                {
                    // Send data
                    int prevIndex = 0;
                    var fragments = data.Chunk(fragmentSize);
                    for (int n = 0; n < fragments.Count(); n++)
                    {
                        try
                        {
                            byte[] fragment = fragments.ToArray()[n];
                            prevIndex += fragment.Length;
                            if (socket == null) return;
                            socket.Send(fragment);
                            if (fragmentDelay > 0)
                                Task.Delay(fragmentDelay).Wait();
                        }
                        catch (Exception ex)
                        {
                            string msgEvent = $"Error, SendDataInFragment2: {ex.Message}";
                            Console.WriteLine(msgEvent);
                            return;
                        }
                    }

                    int outLength = prevIndex;
                    Debug.WriteLine($"{outLength} = {data.Length}");
                }

                private static void SendDataInNormalFragment3(byte[] data, Socket socket, int fragmentSize, int fragmentDelay)
                {
                    // Send data
                    int prevIndex = 0;
                    var fragments = ChunkViaMemory(data, fragmentSize);
                    for (int n = 0; n < fragments.Count(); n++)
                    {
                        try
                        {
                            byte[] fragment = fragments.ToArray()[n].ToArray();
                            prevIndex += fragment.Length;
                            if (socket == null) return;
                            socket.Send(fragment);
                            if (fragmentDelay > 0)
                                Task.Delay(fragmentDelay).Wait();
                        }
                        catch (Exception ex)
                        {
                            string msgEvent = $"Error, SendDataInFragment3: {ex.Message}";
                            Console.WriteLine(msgEvent);
                            return;
                        }
                    }

                    int outLength = prevIndex;
                    Debug.WriteLine($"{outLength} = {data.Length}");
                }

                private static void SendDataInRandomFragment(byte[] data, Socket socket, int fragmentChunks, DPIBypass bp)
                {
                    // Create packets
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
                            return;
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
                        return;
                    }

                    // Check packets
                    int allLength = 0;
                    for (int i = 0; i < packets.Count; i++)
                        allLength += packets[i].Length;
                    Debug.WriteLine($"{allLength} == {data.Length}, Chunks: {packets.Count}");
                    if (allLength != data.Length)
                    {
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
                            string msgEvent = $"Error, SendDataInRandomFragment: {ex.Message}";
                            Debug.WriteLine(msgEvent);
                            return;
                        }
                    }

                    string chunkDetailsEvent = $"{bp.DestHostname}:{bp.DestPort} Length: {data.Length}, Chunks: {packets.Count}";
                    bp.OnChunkDetailsReceived?.Invoke(chunkDetailsEvent, EventArgs.Empty);
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
    }
}
