using System;

namespace MsmhToolsClass.DnsTool.DnsWireformatTools;

/// <summary>
/// Represents a type of DNS resource.
/// </summary>
public interface IDnsResource : IDnsByteArrayWriter
{
    /// <summary>
    /// Read from the specified byte array, starting at the specified offset.
    /// </summary>
    /// <param name="bytes">The byte array to read from.</param>
    /// <param name="offset">The offset to start at.</param>
    /// <param name="length">The maximum number of bytes to read.</param>
    void ReadBytes(ReadOnlyMemory<byte> bytes, ref int offset, int length);
}