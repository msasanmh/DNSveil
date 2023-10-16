using System;

namespace MsmhToolsClass;

public class ByteArrayTool
{
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

    /// <summary>
    /// Fully read a stream into a byte array.
    /// </summary>
    /// <param name="input">The input stream.</param>
    /// <returns>A byte array containing the data read from the stream.</returns>
    public static byte[] StreamToBytes(Stream input)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
        if (!input.CanRead) throw new InvalidOperationException("Input stream is not readable");

        byte[] buffer = new byte[16 * 1024];
        using MemoryStream ms = new();
        int read;

        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            ms.Write(buffer, 0, read);
        }

        return ms.ToArray();
    }

}
