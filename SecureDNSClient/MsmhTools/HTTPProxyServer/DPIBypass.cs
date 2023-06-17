using System;
using System.Diagnostics;
using System.Net.Sockets;
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
                        if (!bp.SendInRandom)
                        {
                            int fragmentSizeOut;
                            if (Data.Length <= bp.FirstPartOfDataLength)
                                fragmentSizeOut = Math.Min(bp.FragmentSize, Data.Length);
                            else
                                fragmentSizeOut = Data.Length / bp.FragmentChunks;

                            if (fragmentSizeOut == 0) fragmentSizeOut = 1;
                            if (fragmentSizeOut > bp.MaxDataLength) fragmentSizeOut = bp.MaxDataLength;

                            if (bp.DontChunkTheBiggestRequest)
                                if (Data.Length == bp.MaxDataLength) fragmentSizeOut = bp.MaxDataLength;

                            SendDataInNormalFragment1(Data, Socket, fragmentSizeOut, bp);
                        }
                        else
                        {
                            if (Data.Length <= bp.FirstPartOfDataLength)
                            {
                                int fragmentSizeOut = Math.Min(bp.FragmentSize, Data.Length);
                                SendDataInNormalFragment1(Data, Socket, fragmentSizeOut, bp);
                            }
                            else
                            {
                                int offset = 5;
                                Random random = new();
                                bp.FragmentChunks = random.Next(bp.FragmentChunks - offset, bp.FragmentChunks + offset);

                                if (bp.FragmentChunks == 0) bp.FragmentChunks = 1;
                                if (bp.FragmentChunks > Data.Length) bp.FragmentChunks = Data.Length;

                                SendDataInRandomFragment(Data, Socket, bp);
                            }
                        }
                    }
                }

                private static void SendDataInNormalFragment1(byte[] data, Socket socket, int fragmentSize, DPIBypass bp)
                {
                    // Create packets
                    List<byte[]> packets = new();
                    packets.Clear();
                    int prevIndex = 0;
                    for (int n = 0; n < data.Length; n += fragmentSize)
                    {
                        try
                        {
                            fragmentSize = Math.Min(fragmentSize, data.Length - n);
                            byte[] fragmentData = new byte[fragmentSize];
                            prevIndex = n;
                            Buffer.BlockCopy(data, n, fragmentData, 0, fragmentSize);
                            packets.Add(fragmentData);
                        }
                        catch (Exception ex)
                        {
                            packets.Clear();
                            string msgEvent = $"Error, Creating normal packets: {ex.Message}";
                            Console.WriteLine(msgEvent);
                            return;
                        }
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
                            Console.WriteLine(msgEvent);
                            return;
                        }
                    }

                    string chunkDetailsEvent = $"{bp.DestHostname}:{bp.DestPort} Length: {data.Length}, Chunks: {packets.Count}";
                    bp.OnChunkDetailsReceived?.Invoke(chunkDetailsEvent, EventArgs.Empty);

                    int outLength = prevIndex + fragmentSize;
                    //Debug.WriteLine($"{outLength} = {data.Length}, Chunks: {packets.Count}");
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

                private static void SendDataInRandomFragment(byte[] data, Socket socket, DPIBypass bp)
                {
                    // Create packets
                    List<byte[]> packets = new();
                    packets.Clear();
                    bp.FragmentChunks = Math.Min(bp.FragmentChunks, data.Length);
                    List<int> indices;
                    if (bp.FragmentChunks < data.Length)
                        indices = GenerateRandomIndices(1, data.Length - 1, bp.FragmentChunks - 1);
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
                            Console.WriteLine(msgEvent);
                            return;
                        }
                    }

                    try
                    {
                        byte[] lastFragmentData = new byte[data.Length - prevIndex];
                        Buffer.BlockCopy(data, prevIndex, lastFragmentData, 0, lastFragmentData.Length);
                        packets.Add(lastFragmentData);

                        int outLength = prevIndex + lastFragmentData.Length;
                        //Debug.WriteLine($"{outLength} = {data.Length}, Chunks: {packets.Count}");
                    }
                    catch (Exception ex)
                    {
                        packets.Clear();
                        string msgEvent = $"Error, Creating last random packet: {ex.Message}";
                        Console.WriteLine(msgEvent);
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
                            Console.WriteLine(msgEvent);
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
