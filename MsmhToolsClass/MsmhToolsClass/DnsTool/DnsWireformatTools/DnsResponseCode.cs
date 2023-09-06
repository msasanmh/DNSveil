using System;

namespace MsmhToolsClass.DnsTool.DnsWireformatTools
{
    /// <summary>
    /// DNS response codes. See https://www.iana.org/assignments/dns-parameters/dns-parameters.xml#dns-parameters-6
    /// </summary>
    public enum DnsResponseCode : byte
    {
        /// <summary>
        /// No Error
        /// </summary>
        NoError = 0,
        /// <summary>
        /// Format Error
        /// </summary>
        FormErr = 1,
        /// <summary>
        /// Server Failure
        /// </summary>
        ServFail = 2,
        /// <summary>
        /// Non-Existent Domain
        /// </summary>
        NXDomain = 3,
        /// <summary>
        /// Not Implemented
        /// </summary>
        NotImp = 4,
        /// <summary>
        /// Query Refused
        /// </summary>
        Refused = 5,
        /// <summary>
        /// Name Exists when it should not
        /// </summary>
        YXDomain = 6,
        /// <summary>
        /// RR Set Exists when it should not
        /// </summary>
        YXRRSet = 7,
        /// <summary>
        /// RR Set that should exist does not
        /// </summary>
        NXRRSet = 8,
        /// <summary>
        /// Server Not Authoritative for zone or Not Authorized
        /// </summary>
        NotAuth = 9,
        /// <summary>
        /// Name not contained in zone
        /// </summary>
        NotZone = 10,
        /// <summary>
        /// DSO-TYPE Not Implemented
        /// </summary>
        DSOTYPENI = 11,
        /// <summary>
        /// BADVERS or TSIG Signature Failure
        /// </summary>
        BADVERS = 16,
        /// <summary>
        /// Key not recognized
        /// </summary>
        BADKEY = 17,
        /// <summary>
        /// Signature out of time window
        /// </summary>
        BADTIME = 18,
        /// <summary>
        /// Bad TKEY Mode
        /// </summary>
        BADMODE = 19,
        /// <summary>
        /// Duplicate key name
        /// </summary>
        BADNAME = 20,
        /// <summary>
        /// Algorithm not supported
        /// </summary>
        BADALG = 21,
        /// <summary>
        /// Bad Truncation
        /// </summary>
        BADTRUNC = 22,
        /// <summary>
        /// Bad/missing Server Cookie
        /// </summary>
        BADCOOKIE = 23
    }
}
