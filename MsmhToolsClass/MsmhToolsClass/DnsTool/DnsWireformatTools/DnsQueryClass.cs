using System;

namespace MsmhToolsClass.DnsTool.DnsWireformatTools
{
    /// <summary>
    /// DNS class. See https://www.iana.org/assignments/dns-parameters/dns-parameters.xml#dns-parameters-2
    /// </summary>
    public enum DnsQueryClass : ushort
    {
        /// <summary>
        /// Internet (IN)
        /// </summary>
        IN = 1,
        /// <summary>
        /// Chaos (CH)
        /// </summary>
        CH = 3,
        /// <summary>
        /// Hesiod (HS)
        /// </summary>
        HS = 4,
        /// <summary>
        /// QCLASS NONE
        /// </summary>
        QCLASS_NONE = 254,
        /// <summary>
        /// QCLASS * (ANY)
        /// </summary>
        QCLASS_ANY = 255,
        /// <summary>
        /// Standards Action
        /// </summary>
        StandardsAction = 65535
    }
}
