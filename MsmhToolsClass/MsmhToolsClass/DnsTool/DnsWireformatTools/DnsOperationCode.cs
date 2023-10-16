using System;

namespace MsmhToolsClass.DnsTool.DnsWireformatTools;

/// <summary>
/// DNS operation codes. See https://www.iana.org/assignments/dns-parameters/dns-parameters.xml#dns-parameters-5
/// </summary>
public enum DnsOperationCode : byte
{
    /// <summary>
    /// Query
    /// </summary>
    QUERY = 0,
    /// <summary>
    /// IQuery (Inverse Query, OBSOLETE)
    /// </summary>
    IQUERY = 1,
    /// <summary>
    /// Status
    /// </summary>
    STATUS = 2,
    /// <summary>
    /// Notify
    /// </summary>
    NOTIFY = 4,
    /// <summary>
    /// Update
    /// </summary>
    UPDATE = 5,
    /// <summary>
    /// DNS Stateful Operations (DSO)
    /// </summary>
    DSO = 6,
}