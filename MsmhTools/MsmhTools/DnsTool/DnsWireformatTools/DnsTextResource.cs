using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsmhTools.DnsTool.DnsWireformatTools
{
    /// <summary>
    /// Represents a DNS text resource containing a string.
    /// </summary>
    public sealed class DnsTextResource : DnsStringResource
    {
        /// <inheritdoc/>
        protected override bool CanUseCompression => false;

        /// <inheritdoc/>
        public override string ToString() => '"' + string.Join("\", \"", Entries) + '"';
    }
}
