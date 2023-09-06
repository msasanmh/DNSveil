using System;

namespace MsmhToolsClass.DnsTool.DnsWireformatTools
{
    /// <summary>
    /// Represents a class which can read from the specified byte array.
    /// </summary>
    public interface IDnsByteArrayReader
    {
        /// <summary>
        /// Read from the specified byte array, starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The byte array to read from.</param>
        /// <param name="offset">The offset to start at.</param>
        void ReadBytes(ReadOnlyMemory<byte> bytes, ref int offset);
    }
}
