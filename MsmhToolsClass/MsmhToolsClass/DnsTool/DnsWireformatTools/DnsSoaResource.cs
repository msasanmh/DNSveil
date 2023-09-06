using System;

namespace MsmhToolsClass.DnsTool.DnsWireformatTools
{
    /// <summary>
    /// A Start of Authority record (abbreviated as SOA record) is a type of
    /// resource record in the Domain Name System (DNS) containing administrative
    /// information about the zone, especially regarding zone transfers.
    /// The SOA record format is specified in RFC 1035.
    /// </summary>
    public sealed class DnsSoaResource : IDnsResource, IEquatable<DnsSoaResource>
    {
        /// <summary>
        /// Primary master name server for this zone 
        /// </summary>
        public string MName { get; set; } = string.Empty;
        /// <summary>
        /// Email address of the administrator responsible for this zone.
        /// (As usual, the email address is encoded as a name. The part of the
        /// email address before the @ becomes the first label of the name; the
        /// domain name after the @ becomes the rest of the name. In zone-file
        /// format, dots in labels are escaped with backslashes; thus the email
        /// address john.doe@example.com would be represented in a zone file
        /// as john\.doe.example.com.)
        /// </summary>
        public string RName { get; set; } = string.Empty;
        /// <summary>
        /// Serial number for this zone. If a secondary name server slaved to
        /// this one observes an increase in this number, the slave will assume
        /// that the zone has been updated and initiate a zone transfer.
        /// </summary>
        public uint Serial { get; set; }
        /// <summary>
        /// Number of seconds after which secondary name servers should query
        /// the master for the SOA record, to detect zone changes. Recommendation
        /// for small and stable zones:[4] 86400 seconds (24 hours).
        /// </summary>
        public TimeSpan Refresh { get; set; }
        /// <summary>
        /// Number of seconds after which secondary name servers should retry to
        /// request the serial number from the master if the master does not respond.
        /// It must be less than Refresh. Recommendation for small and stable
        /// zones: 7200 seconds (2 hours).
        /// </summary>
        public TimeSpan Retry { get; set; }
        /// <summary>
        /// Number of seconds after which secondary name servers should stop answering
        /// request for this zone if the master does not respond. This value must be
        /// bigger than the sum of Refresh and Retry. Recommendation for small and
        /// stable zones: 3600000 seconds (1000 hours).
        /// </summary>
        public TimeSpan Expire { get; set; }
        /// <summary>
        /// Time to live for purposes of negative caching. Recommendation for
        /// small and stable zones: 3600 seconds (1 hour). Originally this
        /// field had the meaning of a minimum TTL value for resource records
        /// in this zone; it was changed to its current meaning by RFC 2308.
        /// </summary>
        public TimeSpan Minimum { get; set; }

        /// <inheritdoc/>
        public bool Equals(DnsSoaResource? other) => MName == other?.MName && RName == other.RName && Serial == other.Serial && Refresh == other.Refresh && Retry == other.Retry && Expire == other.Expire && Minimum == other.Minimum;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is DnsSoaResource record ? Equals(record) : base.Equals(obj);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(MName, RName, Serial, Refresh, Retry, Expire, Minimum);

        /// <inheritdoc/>
        public void ReadBytes(ReadOnlyMemory<byte> bytes, ref int offset, int length)
        {
            MName = string.Join(".", DnsByteExtensions.ReadString(bytes, ref offset));
            RName = string.Join(".", DnsByteExtensions.ReadString(bytes, ref offset));
            Serial = DnsByteExtensions.ReadUInt32(bytes, ref offset);
            Refresh = TimeSpan.FromSeconds(DnsByteExtensions.ReadInt32(bytes, ref offset));
            Retry = TimeSpan.FromSeconds(DnsByteExtensions.ReadInt32(bytes, ref offset));
            Expire = TimeSpan.FromSeconds(DnsByteExtensions.ReadInt32(bytes, ref offset));
            Minimum = TimeSpan.FromSeconds(DnsByteExtensions.ReadUInt32(bytes, ref offset));
        }

        /// <inheritdoc/>
        public override string ToString() => MName;

        /// <inheritdoc/>
        public void WriteBytes(Memory<byte> bytes, ref int offset)
        {
            DnsByteExtensions.ToBytes(MName.Split('.'), bytes, ref offset);
            DnsByteExtensions.ToBytes(RName.Split('.'), bytes, ref offset);
            DnsByteExtensions.ToBytes(Serial, bytes, ref offset);
            DnsByteExtensions.ToBytes((int)Refresh.TotalSeconds, bytes, ref offset);
            DnsByteExtensions.ToBytes((int)Retry.TotalSeconds, bytes, ref offset);
            DnsByteExtensions.ToBytes((int)Expire.TotalSeconds, bytes, ref offset);
            DnsByteExtensions.ToBytes((uint)Minimum.TotalSeconds, bytes, ref offset);
        }
    }
}
