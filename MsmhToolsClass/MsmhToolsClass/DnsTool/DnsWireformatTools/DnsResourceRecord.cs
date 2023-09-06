using System;

namespace MsmhToolsClass.DnsTool.DnsWireformatTools
{
    /// <summary>
    /// Represents metadata around a DNS resource record returned by a DNS server.
    /// </summary>
    public sealed class DnsResourceRecord : IEquatable<DnsResourceRecord>, IDnsByteArrayReader, IDnsByteArrayWriter
    {
        /// <summary>
        /// The type of DNS query.
        /// </summary>
        public DnsQueryType Type { get; set; }
        /// <summary>
        /// The class of DNS query.
        /// </summary>
        public DnsQueryClass Class { get; set; }
        /// <summary>
        /// The time to live entry for this record, in seconds.
        /// </summary>
        public uint TimeToLive { get; set; }

        /// <summary>
        /// The host name associated with this record.
        /// </summary>
        public string Host
        {
            get => string.Join(".", Name);
            set => Name = value.Split('.');
        }

        private string[] Name { get; set; } = Array.Empty<string>();

        /// <summary>
        /// The value of this DNS record, which should be
        /// cast to the appropriate resource record type
        /// class depending on the <see cref="Type"/>.
        /// </summary>
        public IDnsResource? Resource { get; set; }

        private IDnsResource CreateResourceRecord(DnsQueryType recordType)
        {
            return recordType switch
            {
                DnsQueryType.A => new DnsIpAddressResource(),
                DnsQueryType.AAAA => new DnsIpAddressResource(),
                DnsQueryType.TEXT => new DnsTextResource(),
                DnsQueryType.CNAME => new DnsDomainResource(),
                DnsQueryType.NS => new DnsDomainResource(),
                DnsQueryType.PTR => new DnsDomainResource(),
                DnsQueryType.SPF => new DnsTextResource(),
                DnsQueryType.SOA => new DnsSoaResource(),
                DnsQueryType.MX => new DnsMxResource(),
                DnsQueryType.OPT => new DnsOptResource(),
                _ => new DnsUnknownResource(),
            };
        }

        /// <inheritdoc/>
        public override string ToString() => $"Name: {Host} Type: {Type} Class: {Class} TTL: {TimeToLive} Resource: {Resource}";

        /// <inheritdoc/>
        public bool Equals(DnsResourceRecord? other) => Host == other?.Host &&
                                                       Type == other.Type &&
                                                       Class == other.Class &&
                                                       TimeToLive == other.TimeToLive &&
                                                       Resource != null &&
                                                       Resource.Equals(other.Resource);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is DnsResourceRecord record ? Equals(record) : base.Equals(obj);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Name, Type, Class, TimeToLive, Host, Resource);

        /// <inheritdoc/>
        public void ReadBytes(ReadOnlyMemory<byte> bytes, ref int offset)
        {
            Name = DnsByteExtensions.ReadString(bytes, ref offset);
            Type = (DnsQueryType)DnsByteExtensions.ReadUInt16(bytes, ref offset);
            Class = (DnsQueryClass)DnsByteExtensions.ReadUInt16(bytes, ref offset);
            TimeToLive = DnsByteExtensions.ReadUInt32(bytes, ref offset);
            Resource = CreateResourceRecord(Type);
            var dataLength = DnsByteExtensions.ReadUInt16(bytes, ref offset);
            FromBytesKnownLength(Resource, bytes, ref offset, dataLength);
        }

        private static void FromBytesKnownLength(IDnsResource resource, ReadOnlyMemory<byte> bytes, ref int offset, int length)
        {
            var expectedOffset = offset + length;
            resource.ReadBytes(bytes, ref offset, length);
            if (offset != expectedOffset)
            {
                throw new InvalidOperationException($"{resource.GetType().Name}.{nameof(IDnsResource.ReadBytes)} did not read to offset {expectedOffset} (read to {offset})");
            }
        }

        /// <inheritdoc/>
        public void WriteBytes(Memory<byte> bytes, ref int offset)
        {
            DnsByteExtensions.ToBytes(Name, bytes, ref offset);
            DnsByteExtensions.ToBytes((ushort)Type, bytes, ref offset);
            DnsByteExtensions.ToBytes((ushort)Class, bytes, ref offset);
            DnsByteExtensions.ToBytes(TimeToLive, bytes, ref offset);

            // First, write the resource, but save two bytes for the size (and do not advance the offset)
            var resourceSize = 0;
            Resource?.WriteBytes(bytes.Slice(offset + sizeof(ushort)), ref resourceSize);

            // Write the size of the resource in the two bytes preceeding (current offset)
            DnsByteExtensions.ToBytes((ushort)resourceSize, bytes, ref offset);

            // Advance the offset with the size of the resource
            offset += resourceSize;
        }
    }
}
