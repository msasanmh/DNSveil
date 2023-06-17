using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsmhTools.DnsTool.DnsWireformatTools
{
    internal static class DnsByteExtensions
    {
        public static short ReadInt16(byte a, byte b)
        {
            return (short)(a << 0 | b << 8);
        }

        public static ushort ReadUInt16(byte a, byte b)
        {
            return (ushort)(a << 0 | b << 8);
        }

        public static short ReadInt16(ReadOnlyMemory<byte> bytes, ref int offset)
        {
            var value = ReadInt16(bytes.Span[offset + 1], bytes.Span[offset + 0]);
            offset += sizeof(short);
            return value;
        }

        public static ushort ReadUInt16(ReadOnlyMemory<byte> bytes, ref int offset)
        {
            var value = ReadUInt16(bytes.Span[offset + 1], bytes.Span[offset + 0]);
            offset += sizeof(ushort);
            return value;
        }

        public static int ReadInt32(ReadOnlyMemory<byte> bytes, ref int offset)
        {
            var value = bytes.Span[offset + 3] << 0 |
                        bytes.Span[offset + 2] << 8 |
                        bytes.Span[offset + 1] << 16 |
                        bytes.Span[offset + 0] << 24;
            offset += sizeof(int);
            return value;
        }

        public static uint ReadUInt32(ReadOnlyMemory<byte> bytes, ref int offset)
        {
            var value = bytes.Span[offset + 3] << 0 |
                        bytes.Span[offset + 2] << 8 |
                        bytes.Span[offset + 1] << 16 |
                        bytes.Span[offset + 0] << 24;
            offset += sizeof(uint);
            return (uint)value;
        }

        public static string ToDebugString(ReadOnlyMemory<byte> bytes)
        {
            if (bytes.IsEmpty)
            {
                return "<empty>";
            }

            return $"new byte [] {{{string.Join(",", bytes.ToArray())}}}";
        }

        public static ReadOnlyMemory<byte> ReadBytes(ReadOnlyMemory<byte> bytes, int length, ref int offset)
        {
            var data = bytes.Slice(offset, length);
            offset += length;
            return data;
        }

        public static string ReadStringSimple(ReadOnlyMemory<byte> bytes, ref int offset)
        {
            byte labelLength = bytes.Span[offset];
            offset += 1;
#if NETSTANDARD2_0
            var str = Encoding.ASCII.GetString(bytes.Slice(offset, labelLength).ToArray());
#else
            var str = Encoding.ASCII.GetString(bytes.Slice(offset, labelLength).Span);
#endif
            offset += labelLength;
            return str;
        }

        public static string[] ReadString(ReadOnlyMemory<byte> bytes, ref int offset, bool compressionPermitted = true)
        {
            // Assume most labels consist of 3 parts
            var parts = new List<string>(3);

            int? preCompressionOffset = null;
            while (offset < bytes.Length)
            {
                if (bytes.Span[offset] == 0)
                {
                    offset++;
                    break;
                }

                // Pointers are always 192 or more, because the first two bits are 1s
                if (bytes.Span[offset] >= 192 && compressionPermitted)
                {
                    if (!preCompressionOffset.HasValue)
                    {
                        preCompressionOffset = offset + 2;
                    }

                    // Read the 14 bit pointer as our new offset
                    offset = ReadUInt16(bytes.Span[offset + 1], (byte)(bytes.Span[offset] & (1 << 6) - 1));
                }

                parts.Add(ReadStringSimple(bytes, ref offset));
            }

            if (preCompressionOffset.HasValue)
            {
                offset = preCompressionOffset.Value;
            }

            return parts.ToArray();
        }

        public static ReadOnlyMemory<byte> AllocateAndWrite(IDnsByteArrayWriter writer)
        {
            var buffer = AllocatePinnedNetworkBuffer();
            var offset = 0;
            writer.WriteBytes(buffer, ref offset);
#if NETSTANDARD2_0
            return buffer.AsMemory().Slice(0, offset);
#else
            return buffer.Slice(0, offset);
#endif
        }

#if NETSTANDARD2_0
        public static ArraySegment<byte> Slice(this ArraySegment<byte> buffer, int start, int length)
        {
            return new ArraySegment<byte>(buffer.Array, start, length);
        }

        public static ArraySegment<byte> Slice(this ArraySegment<byte> buffer, int start)
        {
            return Slice(buffer, start, buffer.Count - start);
        }
#endif

        private const int NetworkBufferSize = 65527;

#if NETSTANDARD2_0 || NETSTANDARD2_1
        public static ArraySegment<byte> AllocatePinnedNetworkBuffer() => new ArraySegment<byte>(new byte[NetworkBufferSize]);
#else
        // Allocate a buffer which will be used for the incoming query, and re-used to send the answer.
        // Also make it pinned, see https://enclave.io/high-performance-udp-sockets-net6/
        public static Memory<byte> AllocatePinnedNetworkBuffer() => GC.AllocateArray<byte>(NetworkBufferSize, true);
#endif

        public static TReader FromBytes<TReader>(ReadOnlyMemory<byte> bytes) where TReader : IDnsByteArrayReader, new()
        {
            var offset = 0;

            var reader = new TReader();
            reader.ReadBytes(bytes, ref offset);

            if (offset != bytes.Length)
            {
                throw new InvalidOperationException($"Should have read {bytes.Length} bytes, only read {offset} bytes");
            }
            return reader;
        }

        public static void ToBytes(ReadOnlySpan<string> strings, Memory<byte> buffer, ref int offset)
        {
            for (int i = 0; i < strings.Length; i++)
            {
                // First write the value 1 byte from the offset to leave room for the length byte
#if NETSTANDARD2_0
                var stringBytes = Encoding.ASCII.GetBytes(strings[i]);
                stringBytes.CopyTo(buffer.Slice(offset + 1, strings[i].Length));
                var length = stringBytes.Length;
#else
                var length = Encoding.ASCII.GetBytes(strings[i], buffer.Slice(offset + 1, strings[i].Length).Span);
#endif

                // Then write the length before the value
                buffer.Span[offset] = (byte)length;

                // Finally advance the offset past the length and value
                offset += 1 + length;
            }

            buffer.Span[offset++] = 0;
        }

        public static void ToBytes(int value, Memory<byte> buffer, ref int offset)
        {
            buffer.Span[offset++] = (byte)(value >> 24);
            buffer.Span[offset++] = (byte)(value >> 16);
            buffer.Span[offset++] = (byte)(value >> 8);
            buffer.Span[offset++] = (byte)(value >> 0);
        }

        public static void ToBytes(uint value, Memory<byte> buffer, ref int offset)
        {
            buffer.Span[offset++] = (byte)(value >> 24);
            buffer.Span[offset++] = (byte)(value >> 16);
            buffer.Span[offset++] = (byte)(value >> 8);
            buffer.Span[offset++] = (byte)(value >> 0);
        }

        public static void ToBytes(short value, Memory<byte> buffer, ref int offset)
        {
            buffer.Span[offset++] = (byte)(value >> 8);
            buffer.Span[offset++] = (byte)(value >> 0);
        }

        public static void ToBytes(ushort value, Memory<byte> buffer, ref int offset)
        {
            buffer.Span[offset++] = (byte)(value >> 8);
            buffer.Span[offset++] = (byte)(value >> 0);
        }
    }
}
