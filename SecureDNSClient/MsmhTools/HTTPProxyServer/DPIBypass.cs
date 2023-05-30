using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MsmhTools.HTTPProxyServer
{
    public class DPIBypass
    {
        // Max Data Length = 65536
        private readonly int MaxDataLength = 65536;
        public enum Mode
        {
            Program,
            Disable
        }

        public class ProgramMode : DPIBypass
        {
            private byte[] Data;
            private Socket Socket;

            /// <summary>
            /// Don't chunk the request when size is 65536.
            /// </summary>
            public bool DontChunkTheBiggestRequest { get; set; } = false;

            public bool SendInRandom { get; set; } = false;

            public event EventHandler<EventArgs>? OnChunkDetailsReceived;

            public ProgramMode(byte[] data, Socket socket)
            {
                Data = data;
                Socket = socket;
            }

            public void Send(int firstPartOfDataLength, int fragmentSize, int fragmentChunks, int fragmentDelay = 0)
            {
                if (!SendInRandom)
                {
                    int fragmentSizeOut;
                    if (Data.Length <= firstPartOfDataLength)
                        fragmentSizeOut = Math.Min(fragmentSize, Data.Length);
                    else
                        fragmentSizeOut = Data.Length / fragmentChunks;

                    if (fragmentSizeOut == 0) fragmentSizeOut = 1;
                    if (fragmentSizeOut > MaxDataLength) fragmentSizeOut = MaxDataLength;

                    if (DontChunkTheBiggestRequest)
                        if (Data.Length == MaxDataLength) fragmentSizeOut = MaxDataLength;

                    SendDataInNormalFragment1(Data, Socket, fragmentSizeOut, fragmentDelay, OnChunkDetailsReceived);
                }
                else
                {
                    if (Data.Length <= firstPartOfDataLength)
                    {
                        int fragmentSizeOut = Math.Min(fragmentSize, Data.Length);
                        SendDataInNormalFragment1(Data, Socket, fragmentSizeOut, fragmentDelay, OnChunkDetailsReceived);
                    }
                    else
                    {
                        int offset = 5;
                        Random random = new();
                        fragmentChunks = random.Next(fragmentChunks - offset, fragmentChunks + offset);

                        if (fragmentChunks == 0) fragmentChunks = 1;
                        if (fragmentChunks > Data.Length) fragmentChunks = Data.Length;

                        SendDataInRandomFragment(Data, Socket, fragmentChunks, fragmentDelay, OnChunkDetailsReceived);
                    }
                }
            }
        }

        private static void SendDataInNormalFragment1(byte[] data, Socket socket, int fragmentSize, int fragmentDelay, EventHandler<EventArgs>? onChunkDetailsReceived)
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
                    if (fragmentDelay > 0)
                        Task.Delay(fragmentDelay).Wait();
                }
                catch (Exception ex)
                {
                    string msgEvent = $"Error, SendDataInFragment1: {ex.Message}";
                    Console.WriteLine(msgEvent);
                    return;
                }
            }

            string chunkDetailsEvent = $"Length: {data.Length}, Chunks: {packets.Count}";
            onChunkDetailsReceived?.Invoke(chunkDetailsEvent, EventArgs.Empty);

            int outLength = prevIndex + fragmentSize;
            Debug.WriteLine($"{outLength} = {data.Length}, Chunks: {packets.Count}");
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

        private static void SendDataInRandomFragment(byte[] data, Socket socket, int fragmentChunks, int fragmentDelay, EventHandler<EventArgs>? onChunkDetailsReceived)
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
                Debug.WriteLine($"{outLength} = {data.Length}, Chunks: {packets.Count}");
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
                    if (fragmentDelay > 0)
                        Task.Delay(fragmentDelay).Wait();
                }
                catch (Exception ex)
                {
                    string msgEvent = $"Error, SendDataInRandomFragment: {ex.Message}";
                    Console.WriteLine(msgEvent);
                    return;
                }
            }

            string chunkDetailsEvent = $"Length: {data.Length}, Chunks: {packets.Count}";
            onChunkDetailsReceived?.Invoke(chunkDetailsEvent, EventArgs.Empty);
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
