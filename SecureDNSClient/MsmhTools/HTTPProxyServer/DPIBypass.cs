using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MsmhTools.HTTPProxyServer
{
    public class DPIBypass
    {
        // Max Data Length = 65536
        public enum Mode
        {
            Program,
            Random,
            Disable
        }

        public class ProgramMode : DPIBypass
        {
            private byte[] Data;
            private Socket Socket;
            public ProgramMode(byte[] data, Socket socket)
            {
                Data = data;
                Socket = socket;
            }

            public void Send(int firstPartOfDataLength, int programFragmentSize, int divideBy, int fragmentDelay = 0)
            {
                int fragmentSize;
                if (Data.Length < firstPartOfDataLength)
                    fragmentSize = programFragmentSize;
                else
                    fragmentSize = Data.Length / divideBy;

                SendDataInNormalFragment1(Data, Socket, fragmentSize, fragmentDelay);
            }
        }

        public class RandomMode : DPIBypass
        {
            private byte[] Data;
            private Socket Socket;
            public RandomMode(byte[] data, Socket socket)
            {
                Data = data;
                Socket = socket;
            }

            public void Send(int fragmentLength, int fragmentDelay = 0)
            {
                SendDataInRandomFragment(Data, Socket, fragmentLength, fragmentDelay);
            }
        }

        private static void SendDataInNormalFragment1(byte[] data, Socket socket, int fragmentSize, int fragmentDelay)
        {
            // Send data
            int prevIndex = 0;
            for (int n = 0; n < data.Length; n += fragmentSize)
            {
                try
                {
                    fragmentSize = Math.Min(fragmentSize, data.Length - n);
                    byte[] fragmentData = new byte[fragmentSize];
                    prevIndex = n;
                    Buffer.BlockCopy(data, n, fragmentData, 0, fragmentSize);
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
            
            int outLength = prevIndex + fragmentSize;
            Debug.WriteLine($"{outLength} = {data.Length}");
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

        private static void SendDataInRandomFragment(byte[] data, Socket socket, int fragmentLength, int fragmentDelay)
        {
            // Send data
            fragmentLength = Math.Min(fragmentLength, data.Length);
            List<int> indices = GenerateRandomIndices(1, data.Length - 1, fragmentLength - 1);
            indices.Sort();

            int prevIndex = 0;
            for (int n = 0; n < indices.Count; n++)
            {
                try
                {
                    int index = indices[n];
                    byte[] fragmentData = new byte[index - prevIndex];
                    Buffer.BlockCopy(data, prevIndex, fragmentData, 0, fragmentData.Length);
                    prevIndex = index;
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

            try
            {
                byte[] lastFragmentData = new byte[data.Length - prevIndex];
                Buffer.BlockCopy(data, prevIndex, lastFragmentData, 0, lastFragmentData.Length);
                if (socket == null) return;
                socket.Send(lastFragmentData);

                int outLength = prevIndex + lastFragmentData.Length;
                Debug.WriteLine($"{outLength} = {data.Length}");
            }
            catch (Exception ex)
            {
                string msgEvent = $"Error, SendDataInRandomFragment Last Fragment: {ex.Message}";
                Console.WriteLine(msgEvent);
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
