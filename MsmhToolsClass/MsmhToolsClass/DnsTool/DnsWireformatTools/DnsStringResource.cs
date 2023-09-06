using System;

namespace MsmhToolsClass.DnsTool.DnsWireformatTools
{
    /// <summary>
    /// Represents a DNS resource containing a string.
    /// </summary>
    public abstract class DnsStringResource : IDnsResource, IEquatable<DnsStringResource>
    {
        /// <summary>
        /// The text entry contained within this resource.
        /// </summary>
        /// <value>
        /// The text values of this resource as an array of strings.
        /// </value>
        public string[] Entries { get; set; } = Array.Empty<string>();

        /// <inheritdoc/>
        public bool Equals(DnsStringResource? other) => Entries.SequenceEqual(other?.Entries ?? Array.Empty<string>());

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is DnsStringResource record ? Equals(record) : base.Equals(obj);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Entries);

        /// <summary>
        /// Describes whether this string resource can use string compression.
        /// </summary>
        protected abstract bool CanUseCompression { get; }

        /// <inheritdoc/>
        public virtual void ReadBytes(ReadOnlyMemory<byte> bytes, ref int offset, int length)
        {
            // Provide the ReadString method with a sliced buffer, which ends when this resource ends
            // It must start where the packet starts, since there are often pointers back to the beginning
            Entries = DnsByteExtensions.ReadString(bytes.Slice(0, offset + length), ref offset, CanUseCompression);
        }

        /// <inheritdoc/>
        public virtual void WriteBytes(Memory<byte> bytes, ref int offset)
        {
            DnsByteExtensions.ToBytes(Entries, bytes, ref offset);
        }
    }
}
