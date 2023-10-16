using System;

namespace MsmhToolsClass.DnsTool.DnsWireformatTools;

/// <summary>
/// Describes an EDNS0 psuedo-resource.
/// </summary>
public sealed class DnsOptResource : IDnsResource, IEquatable<DnsOptResource>
{
    /// <summary>
    /// The raw bytes recieved for this DNS resource.
    /// </summary>
    /// <value>
    /// The raw set of bytes representing the value fo this DNS resource.
    /// For string values, due to compression the whole packet may be needed.
    /// </value>
    public ReadOnlyMemory<byte> Raw { get; set; }

    /// <inheritdoc/>
    public bool Equals(DnsOptResource? other)
    {
        return other != null && Raw.Span.SequenceEqual(other.Raw.Span);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is DnsOptResource record ? Equals(record) : base.Equals(obj);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Raw);

    /// <inheritdoc/>
    public void ReadBytes(ReadOnlyMemory<byte> bytes, ref int offset, int length)
    {
        Raw = DnsByteExtensions.ReadBytes(bytes, length, ref offset);
    }

    /// <inheritdoc/>
    public void WriteBytes(Memory<byte> bytes, ref int offset)
    {
        // Payloadsize / flags is serialised at a higher level
        Raw.CopyTo(bytes[offset..]);
        offset += Raw.Length;
    }

    /// <inheritdoc/>
    public override string ToString() => $"Raw: {Raw.Length} bytes";
}