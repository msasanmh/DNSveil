using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsmhTools.DnsTool.DnsWireformatTools
{
    /// <summary>
    /// Represents a DNS text resource containing a domain name.
    /// </summary>
    public sealed class DnsDomainResource : DnsStringResource
    {
        /// <inheritdoc/>
        protected override bool CanUseCompression => true;

        /// <summary>
        /// Get the value of this entry as a domain name.
        /// </summary>
        public string Domain => string.Join(".", Entries);

        /// <inheritdoc/>
        public override string ToString() => Domain;
    }
}
