using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MsmhTools.DnsTool.DnsWireformatTools
{
    /// <summary>
    /// Represents a DNS resource record which contains an Internet Protocol address.
    /// See <see cref="DnsQueryType.A"/> and <see cref="DnsQueryType.AAAA"/>.
    /// </summary>
    public sealed class DnsIpAddressResource : IDnsResource, IEquatable<DnsIpAddressResource>
    {
        /// <summary>
        /// The Internet Protocol address.
        /// </summary>
        /// <value>
        /// May be an IPv4 or IPv6 address, depending on the record type.
        /// </value>
        public IPAddress IPAddress { get; set; }

        /// <inheritdoc/>
        public bool Equals(DnsIpAddressResource other) => IPAddress.Equals(other.IPAddress);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is DnsIpAddressResource record ? Equals(record) : base.Equals(obj);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(IPAddress);

        /// <inheritdoc/>
        public void ReadBytes(ReadOnlyMemory<byte> bytes, ref int offset, int length)
        {
            var raw = DnsByteExtensions.ReadBytes(bytes, length, ref offset);
            IPAddress = new IPAddress(raw.ToArray());
        }

        /// <inheritdoc/>
        public IEnumerable<IEnumerable<byte>> WriteBytes()
        {
            yield return IPAddress.GetAddressBytes();
        }

        /// <inheritdoc/>
        public override string ToString() => IPAddress.ToString();

        /// <inheritdoc/>
        public void WriteBytes(Memory<byte> bytes, ref int offset)
        {
            Span<byte> address = IPAddress.GetAddressBytes();

            address.CopyTo(bytes.Slice(offset).Span);
            offset += address.Length;
        }
    }
}
