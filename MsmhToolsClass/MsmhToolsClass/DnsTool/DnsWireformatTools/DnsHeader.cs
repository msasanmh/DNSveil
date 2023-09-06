using System;

namespace MsmhToolsClass.DnsTool.DnsWireformatTools
{
    /// <summary>
    /// Represents a DNS header, the first entry in a <see cref="DnsMessage"/>.
    /// See <see cref="DnsQueryFactory"/> for methods to create DNS headers for specific purposes.
    /// </summary>
    public sealed class DnsHeader : IEquatable<DnsHeader>, IDnsByteArrayReader, IDnsByteArrayWriter
    {
        /// <summary>
        /// Gets or sets the unique ID of this DNS query.
        /// </summary>
        /// <value>
        /// A unique ID which allows the sender/receiver to
        /// identify this DNS message (for example if the transport protocol is stateless).
        /// </value>
        public ushort Id { get; set; }

        internal ushort Flags { get; set; }

        /// <summary>
        /// The number of questions in this header.
        /// </summary>
        /// <value>
        /// A count of the number of questions asked of the DNS server.
        /// </value>
        public short QuestionCount { get; set; }

        /// <summary>
        /// The number of <see cref="DnsMessage"/> records in this header.
        /// </summary>
        /// <value>
        /// A count of the number of answers contained within this DNS message.
        /// </value>
        public short AnswerRecordCount { get; set; }

        /// <summary>
        /// The number of name server records in this header.
        /// </summary>
        /// <value>
        /// A count of the number of DNS name server records in this header.
        /// </value>
        public short NameServerRecordCount { get; set; }

        /// <summary>
        /// The number of additional records in this header.
        /// </summary>
        /// <value>
        /// A count of the number of additional records in this header.
        /// </value>
        public short AdditionalRecordCount { get; set; }

        internal string[] Labels { get; set; } = Array.Empty<string>();

        /// <summary>
        /// The <see cref="DnsQueryType"/> of this header.
        /// </summary>
        /// <value>
        /// If the type of query is <see cref="DnsQueryType.A"/>, it means the query is for IP address records.
        /// </value>
        public DnsQueryType QueryType { get; set; }

        /// <summary>
        /// The <see cref="DnsQueryClass"/> of this header, normally <see cref="DnsQueryClass.IN"/> (Internet).
        /// </summary>
        /// <value>
        /// The class of query, most likely <see cref="DnsQueryClass.IN"/> to represent Internet DNS queries.
        /// </value>
        public DnsQueryClass QueryClass { get; set; }

        /// <summary>
        /// Determines whether this is a DNS query or answer.
        /// </summary>
        /// <value>
        /// For DNS queries this is false, for DNS responses this is true.
        /// </value>
        public bool IsQueryResponse
        {
            get => (Flags & 0x8000) == 0x8000;
            set => Flags = value ? (ushort)(Flags | 0x8000) : (ushort)(Flags & (~0x8000));
        }

        /// <summary>
        /// Determines the operation code for this DNS header.
        /// </summary>
        /// <value>
        /// The DNS operation code for this DNS answer. For example <see cref="DnsOperationCode.QUERY"/>.
        /// </value>
        public DnsOperationCode OperationCode
        {
            get => (DnsOperationCode)((Flags & 0x7800) >> 11);
            set => Flags = (ushort)((Flags & ~0x7800) | ((int)value << 11));
        }

        /// <summary>
        /// Determines whether the answer was from an authoritative DNS server.
        /// </summary>
        /// <value>
        /// If true, this DNS answer is authoritative.
        /// </value>
        public bool AuthoritativeAnswer
        {
            get => (Flags & 0x0400) == 0x0400;
            set => Flags = value ? (ushort)(Flags | 0x0400) : (ushort)(Flags & (~0x0400));
        }

        /// <summary>
        /// Determines whether this DNS answer was truncated due to size.
        /// </summary>
        public bool Truncation
        {
            get => (Flags & 0x0200) == 0x0200;
            set => Flags = value ? (ushort)(Flags | 0x0200) : (ushort)(Flags & (~0x0200));
        }

        /// <summary>
        /// Indicates whether recursion is desired on this DNS query.
        /// If this is an answer, indicates whether the original query requested recursion.
        /// </summary>
        public bool RecursionDesired
        {
            get => (Flags & 0x0100) == 0x0100;
            set => Flags = value ? (ushort)(Flags | 0x0100) : (ushort)(Flags & (~0x0100));
        }

        /// <summary>
        /// Indicates whether recursion is available.
        /// </summary>
        public bool RecursionAvailable
        {
            get => (Flags & 0x0080) == 0x0080;
            set => Flags = value ? (ushort)(Flags | 0x0080) : (ushort)(Flags & (~0x0080));
        }

        /// <summary>
        /// The <see cref="DnsResponseCode"/> for this result.
        /// </summary>
        public DnsResponseCode ResponseCode
        {
            get => (DnsResponseCode)(Flags & 0x000F);
            set => Flags = (ushort)((Flags & ~0x000F) | (byte)value);
        }

        /// <summary>
        /// The DNS host to use, for example "example.com".
        /// </summary>
        public string Host
        {
            get => string.Join(".", Labels);
            set => Labels = value.Split('.');
        }

        /// <inheritdoc/>
        public override string ToString() => $"{(IsQueryResponse ? "RES" : "QRY")}: {Id} Domain: {Host} Type: {QueryType} Class: {QueryClass}";

        /// <inheritdoc/>
        public bool Equals(DnsHeader? other)
        {
            return Id == other?.Id &&
                   Flags == other.Flags &&
                   QuestionCount == other.QuestionCount &&
                   AnswerRecordCount == other.AnswerRecordCount &&
                   NameServerRecordCount == other.NameServerRecordCount &&
                   AdditionalRecordCount == other.AdditionalRecordCount &&
                   Host == other.Host &&
                   QueryType == other.QueryType &&
                   QueryClass == other.QueryClass;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Id);
            hash.Add(Flags);
            hash.Add(QuestionCount);
            hash.Add(AnswerRecordCount);
            hash.Add(NameServerRecordCount);
            hash.Add(AdditionalRecordCount);
            hash.Add(QueryType);
            hash.Add(QueryClass);
            hash.Add(Host);
            return hash.ToHashCode();
        }

        /// <inheritdoc/>
        public void ReadBytes(ReadOnlyMemory<byte> bytes, ref int offset)
        {
            Id = DnsByteExtensions.ReadUInt16(bytes, ref offset);
            Flags = DnsByteExtensions.ReadUInt16(bytes, ref offset);
            QuestionCount = DnsByteExtensions.ReadInt16(bytes, ref offset);
            AnswerRecordCount = DnsByteExtensions.ReadInt16(bytes, ref offset);
            NameServerRecordCount = DnsByteExtensions.ReadInt16(bytes, ref offset);
            AdditionalRecordCount = DnsByteExtensions.ReadInt16(bytes, ref offset);
            Labels = DnsByteExtensions.ReadString(bytes, ref offset);
            QueryType = (DnsQueryType)DnsByteExtensions.ReadUInt16(bytes, ref offset);
            QueryClass = (DnsQueryClass)DnsByteExtensions.ReadUInt16(bytes, ref offset);

            if (!IsQueryResponse && (AnswerRecordCount > 0 || NameServerRecordCount > 0))
            {
                throw new InvalidOperationException($"Header states that this message is a query, yet there are answer and nameserver records.");
            }
        }

        /// <inheritdoc/>
        public void WriteBytes(Memory<byte> bytes, ref int offset)
        {
            DnsByteExtensions.ToBytes(Id, bytes, ref offset);
            DnsByteExtensions.ToBytes(Flags, bytes, ref offset);
            DnsByteExtensions.ToBytes(QuestionCount, bytes, ref offset);
            DnsByteExtensions.ToBytes(AnswerRecordCount, bytes, ref offset);
            DnsByteExtensions.ToBytes(NameServerRecordCount, bytes, ref offset);
            DnsByteExtensions.ToBytes(AdditionalRecordCount, bytes, ref offset);
            DnsByteExtensions.ToBytes(Labels, bytes, ref offset);
            DnsByteExtensions.ToBytes((ushort)QueryType, bytes, ref offset);
            DnsByteExtensions.ToBytes((ushort)QueryClass, bytes, ref offset);
        }

        /// <summary>
        /// A generic bag of tags associated with this object.
        /// Will not be serialised and/or passed over the wire.
        /// </summary>
        public IDictionary<string, object> Tags { get; } = new Dictionary<string, object>();
    }
}
