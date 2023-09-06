using System;

namespace MsmhToolsClass.DnsTool.DnsWireformatTools
{
    /// <summary>
    /// Represents a mail server DNS record. See <see cref="DnsQueryType.MX"/>.
    /// </summary>
    public sealed class DnsMxResource : DnsStringResource, IEquatable<DnsMxResource>
    {
        /// <summary>
        /// The priority or preference field identifies which mailserver should be preferred.
        /// </summary>
        /// <value>
        /// The priority field identifies which mailserver should be preferred - if multiple
        /// values are the same, mail would be expected to flow evenly to all hosts.
        /// </value>
        public ushort Preference { get; set; }

        /// <summary>
        /// The domain name of a mailserver. 
        /// </summary>
        /// <value>
        /// The host name must map directly to one or more address records (A, or AAAA) in the DNS, and must not point to any CNAME records.
        /// </value>
        public string Exchange => string.Join(".", Entries);

        /// <inheritdoc/>
        protected override bool CanUseCompression => true;

        /// <inheritdoc/>
        public bool Equals(DnsMxResource? other) => Preference == other?.Preference && Exchange == other.Exchange;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is DnsMxResource record ? Equals(record) : base.Equals(obj);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Preference, Exchange);

        /// <inheritdoc/>
        public override void ReadBytes(ReadOnlyMemory<byte> bytes, ref int offset, int length)
        {
            Preference = DnsByteExtensions.ReadUInt16(bytes, ref offset);
            base.ReadBytes(bytes, ref offset, length - sizeof(ushort));
        }

        /// <inheritdoc/>
        public override string ToString() => Exchange;

        /// <inheritdoc/>
        public override void WriteBytes(Memory<byte> bytes, ref int offset)
        {
            DnsByteExtensions.ToBytes(Preference, bytes, ref offset);
            base.WriteBytes(bytes, ref offset);
        }
    }
}
