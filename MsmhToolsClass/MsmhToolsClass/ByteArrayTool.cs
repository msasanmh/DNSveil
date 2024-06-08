using System.Diagnostics;

namespace MsmhToolsClass;

public static class ByteArrayTool
{
    public static byte[] Append(this byte[] orig, byte[] append)
    {
        if (append == null) return orig;
        if (orig == null) return append;

        byte[] bytes = new byte[orig.Length + append.Length];
        Buffer.BlockCopy(orig, 0, bytes, 0, orig.Length);
        Buffer.BlockCopy(append, 0, bytes, orig.Length, append.Length);
        return bytes;
    }

    public static bool CanFitInBits(int number, int numberOfBits)
    {
        int maxValue = (1 << numberOfBits) - 1;
        return number >= 0 && number <= maxValue;
    }

    public static byte[] GenerateRandom(int length)
    {
        byte[] bytes = new byte[length];
        try
        {
            Random random = new(length);
            random.NextBytes(bytes);
            // OR: LibSodium.randombytes_buf(bytes, length);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GenerateRandom2: " + ex.Message);
        }
        return bytes;
    }

    public static bool TryConvertBytesToUInt16(byte[] bytes, out ushort result)
    {
        try
        {
            int n = (bytes[0] << 8) + bytes[1];
            if (n < 0)
            {
                result = 0;
                return false;
            }
            result = Convert.ToUInt16(n);
            // OR
            //if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            //result = BitConverter.ToUInt16(bytes, 0);
            return true;
        }
        catch (Exception)
        {
            result = 0;
            return false;
        }
    }

    public static bool TryConvertBytesToUInt24(byte[] bytes, out uint result)
    {
        try
        {
            int n = (bytes[0] << 16) + (bytes[1] << 8) + bytes[2];
            if (n < 0)
            {
                result = 0;
                return false;
            }
            result = Convert.ToUInt32(n);
            return true;
        }
        catch (Exception)
        {
            result = 0;
            return false;
        }
    }

    public static bool TryConvertBytesToUInt32(byte[] bytes, out uint result)
    {
        try
        {
            int n = (bytes[0] << 24) + (bytes[1] << 16) + (bytes[2] << 8) + bytes[3];
            if (n < 0)
            {
                result = 0;
                return false;
            }
            result = Convert.ToUInt32(n);
            return true;
        }
        catch (Exception)
        {
            result = 0;
            return false;
        }
    }

    public static bool TryConvertUInt16ToBytes(ushort value, out byte[] result)
    {
        try
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(value >> 8);
            bytes[1] = (byte)value;
            result = bytes;
            // OR
            //byte[] result0 = BitConverter.GetBytes(value);
            //result = BitConverter.IsLittleEndian ? result0.Reverse().ToArray() : result0;
            return true;
        }
        catch (Exception)
        {
            result = Array.Empty<byte>();
            return false;
        }
    }

    public static bool TryConvertUInt24ToBytes(uint value, out byte[] result)
    {
        try
        {
            byte[] bytes = new byte[3];
            bytes[0] = (byte)(value >> 16);
            bytes[1] = (byte)(value >> 8);
            bytes[2] = (byte)value;
            result = bytes;
            return true;
        }
        catch (Exception)
        {
            result = Array.Empty<byte>();
            return false;
        }
    }

    public static bool TryConvertUInt32ToBytes(uint value, out byte[] result)
    {
        try
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(value >> 24);
            bytes[1] = (byte)(value >> 16);
            bytes[2] = (byte)(value >> 8);
            bytes[3] = (byte)value;
            result = bytes;
            return true;
        }
        catch (Exception)
        {
            result = Array.Empty<byte>();
            return false;
        }
    }

    public static bool TryConvertToBinary(byte[] buffer, out string result)
    {
        try
        {
            result = string.Empty;
            foreach (byte b in buffer) result += Convert.ToString(b, 2).PadLeft(8, '0');
            return true;
        }
        catch (Exception)
        {
            result = string.Empty;
            return false;
        }
    }

    public static bool TryConvertToBinary(byte oneByte, out string result)
    {
        try
        {
            result = Convert.ToString(oneByte, 2).PadLeft(8, '0');
            return true;
        }
        catch (Exception)
        {
            result = string.Empty;
            return false;
        }
    }

    public static bool TryConvertToBinary(ushort value, out string result)
    {
        try
        {
            int binaryBase = 2;
            result = Convert.ToString(value, binaryBase);
            return true;
        }
        catch (Exception)
        {
            result = string.Empty;
            return false;
        }
    }

    public static bool TrySplitBinary(string bits, out bool[] result)
    {
        try
        {
            result = Array.ConvertAll(bits.ToCharArray(), x => x == '1');
            //result = bits.Select(x => x == '1').ToArray(); // Using LINQ
            return true;
        }
        catch (Exception)
        {
            result = Array.Empty<bool>();
            return false;
        }
    }

    public static bool TrySplitBinary(int bits, out bool[] result)
    {
        return TrySplitBinary(bits.ToString(), out result);
    }

    public static bool TryConvertSplittedBinaryToBytes(bool[] bits, out byte[] result)
    {
        try
        {
            if (bits.Length % 8 != 0)
            {
                result = Array.Empty<byte>();
                return false;
            }

            // No need to worry about endianness
            byte[] bytes = new byte[bits.Length / 8];
            for (int i = 0; i < bits.Length; i += 8)
            {
                int value = 0;
                for (int j = 0; j < 8; j++)
                {
                    if (bits[i + j]) value += 1 << (7 - j);
                }
                bytes[i / 8] = (byte)value;
            }

            // OR / No need to worry about endianness
            //int nBytes = bits.Length / 8;
            //var bytesAsBools = Enumerable.Range(0, nBytes).Select(i => bits.Skip(8 * i).Take(8));
            //byte[] bytes = bytesAsBools.Select(b => Convert.ToByte(string.Join("", b.Select(x => x ? "1" : "0")), 2)).ToArray();

            // OR (Don't Use This One)
            //byte[] bytes = new byte[bits.Length / 8]; // 1 Byte = 8 Bit
            //BitArray bitArray = new(bits);
            //bitArray.CopyTo(bytes, 0);

            result = bytes;
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("TryConvertSplittedBinaryToBytes: " + ex.Message);
            result = Array.Empty<byte>();
            return false;
        }
    }

    public static async Task<byte[]> StreamToBytes(Stream stream)
    {
        if (!stream.CanRead) return Array.Empty<byte>();
        using MemoryStream ms = new();

        try
        {
            await stream.CopyToAsync(ms);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ByteArrayTool StreamToBytes: " + ex.Message);
        }

        return ms.ToArray();
    }

    public static int Search(byte[] src, byte[] pattern)
    {
        int maxFirstCharSlot = src.Length - pattern.Length + 1;
        for (int i = 0; i < maxFirstCharSlot; i++)
        {
            if (src[i] != pattern[0]) // compare only first byte
                continue;

            // found a match on first byte, now try to match rest of the pattern
            for (int j = pattern.Length - 1; j >= 1; j--)
            {
                if (src[i + j] != pattern[j]) break;
                if (j == 1) return i;
            }
        }
        return -1;
    }

}
