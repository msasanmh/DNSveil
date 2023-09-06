using System;

namespace MsmhToolsClass.DnsTool.DnsWireformatTools
{
    /// <summary>
    /// DNS resource record type. See https://www.iana.org/assignments/dns-parameters/dns-parameters.xml#dns-parameters-4
    /// </summary>
    public enum DnsQueryType : ushort
    {
        /// <summary>
        /// A host address
        /// </summary>
        A = 1,
        /// <summary>
        /// An authoritative name server
        /// </summary>
        NS = 2,
        /// <summary>
        /// A mail destination (OBSOLETE - use MX)
        /// </summary>
        MD = 3,
        /// <summary>
        /// A mail forwarder (OBSOLETE - use MX)
        /// </summary>
        MF = 4,
        /// <summary>
        /// The canonical name for an alias
        /// </summary>
        CNAME = 5,
        /// <summary>
        /// Marks the start of a zone of authority
        /// </summary>
        SOA = 6,
        /// <summary>
        /// A mailbox domain name (EXPERIMENTAL)
        /// </summary>
        MB = 7,
        /// <summary>
        /// A mail group member (EXPERIMENTAL)
        /// </summary>
        MG = 8,
        /// <summary>
        /// A mail rename domain name (EXPERIMENTAL)
        /// </summary>
        MR = 9,
        /// <summary>
        /// A null RR (EXPERIMENTAL)
        /// </summary>
        NULL = 10,
        /// <summary>
        /// A well known service description
        /// </summary>
        WKS = 11,
        /// <summary>
        /// A domain name pointer
        /// </summary>
        PTR = 12,
        /// <summary>
        /// Host information
        /// </summary>
        HINFO = 13,
        /// <summary>
        /// Mailbox or mail list information
        /// </summary>
        MINFO = 14,
        /// <summary>
        /// Mail exchange
        /// </summary>
        MX = 15,
        /// <summary>
        /// Text strings
        /// </summary>
        TEXT = 16,
        /// <summary>
        /// For Responsible Person
        /// </summary>
        RP = 17,
        /// <summary>
        /// For AFS Data Base location
        /// </summary>
        AFSDB = 18,
        /// <summary>
        /// For X.25 PSDN address
        /// </summary>
        X25 = 19,
        /// <summary>
        /// For ISDN address
        /// </summary>
        ISDN = 20,
        /// <summary>
        /// For Route Through
        /// </summary>
        RT = 21,
        /// <summary>
        /// For NSAP address, NSAP style A record
        /// </summary>
        NSAP = 22,
        /// <summary>
        /// For domain name pointer, NSAP style
        /// </summary>
        NSAPPTR = 23,
        /// <summary>
        /// For security signature
        /// </summary>
        SIG = 24,
        /// <summary>
        /// For security key
        /// </summary>
        KEY = 25,
        /// <summary>
        /// X.400 mail mapping information
        /// </summary>
        PX = 26,
        /// <summary>
        /// Geographical Position
        /// </summary>
        GPOS = 27,
        /// <summary>
        /// IP6 Address
        /// </summary>
        AAAA = 28,
        /// <summary>
        /// Location Information
        /// </summary>
        LOC = 29,
        /// <summary>
        /// Next Domain (OBSOLETE)
        /// </summary>
        NXT = 30,
        /// <summary>
        /// Endpoint Identifier
        /// </summary>
        EID = 31,
        /// <summary>
        /// Nimrod Locator
        /// </summary>
        NIMLOC = 32,
        /// <summary>
        /// Server Selection
        /// </summary>
        SRV = 33,
        /// <summary>
        /// ATM Address
        /// </summary>
        ATMA = 34,
        /// <summary>
        /// Naming Authority Pointer
        /// </summary>
        NAPTR = 35,
        /// <summary>
        /// Key Exchanger
        /// </summary>
        KX = 36,
        CERT = 37,
        /// <summary>
        /// A6 (OBSOLETE - use AAAA)
        /// </summary>
        A6 = 38,
        DNAME = 39,
        SINK = 40,
        OPT = 41,
        APL = 42,
        /// <summary>
        /// Delegation Signer
        /// </summary>
        DS = 43,
        /// <summary>
        /// SSH Key Fingerprint
        /// </summary>
        SSHFP = 44,
        IPSECKEY = 45,
        RRSIG = 46,
        NSEC = 47,
        DNSKEY = 48,
        DHCID = 49,
        NSEC3 = 50,
        NSEC3PARAM = 51,
        TLSA = 52,
        /// <summary>
        /// S/MIME cert association
        /// </summary>
        SMIMEA = 53,
        /// <summary>
        /// Host Identity Protocol
        /// </summary>
        HIP = 55,
        NINFO = 56,
        RKEY = 57,
        /// <summary>
        /// Trust Anchor LINK
        /// </summary>
        TALINK = 58,
        /// <summary>
        /// Child DS
        /// </summary>
        CDS = 59,
        /// <summary>
        /// DNSKEY(s) the Child wants reflected in DS
        /// </summary>
        CDNSKEY = 60,
        /// <summary>
        /// OpenPGP Key
        /// </summary>
        OPENPGPKEY = 61,
        /// <summary>
        /// Child-To-Parent Synchronization
        /// </summary>
        CSYNC = 62,
        /// <summary>
        /// Message digest for DNS zone
        /// </summary>
        ZONEMD = 63,
        /// <summary>
        /// Service Binding
        /// </summary>
        SVCB = 64,
        /// <summary>
        /// HTTPS Binding
        /// </summary>
        HTTPS = 65,
        SPF = 99,
        UINFO = 100,
        UID = 101,
        GID = 102,
        UNSPEC = 103,
        NID = 104,
        L32 = 105,
        L64 = 106,
        LP = 107,
        /// <summary>
        /// An EUI-48 address
        /// </summary>
        EUI48 = 108,
        /// <summary>
        /// An EUI-64 address
        /// </summary>
        EUI64 = 109,
        /// <summary>
        /// Transaction Key
        /// </summary>
        TKEY = 249,
        /// <summary>
        /// Transaction Signature
        /// </summary>
        TSIG = 250,
        /// <summary>
        /// Incremental transfer
        /// </summary>
        IXFR = 251,
        /// <summary>
        /// Transfer of an entire zone
        /// </summary>
        AXFR = 252,
        /// <summary>
        /// Mailbox-related RRs (MB, MG or MR)
        /// </summary>
        MAILB = 253,
        /// <summary>
        /// Mail agent RRs (OBSOLETE - see MX)
        /// </summary>
        MAILA = 254,
        /// <summary>
        /// A request for some or all records the server has available
        /// </summary>
        ANY = 255,
        URI = 256,
        /// <summary>
        /// Certification Authority Restriction
        /// </summary>
        CAA = 257,
        /// <summary>
        /// Application Visibility and Control
        /// </summary>
        AVC = 258,
        /// <summary>
        /// Digital Object Architecture
        /// </summary>
        DOA = 259,
        /// <summary>
        /// Automatic Multicast Tunneling Relay
        /// </summary>
        AMTRELAY = 260,
        /// <summary>
        /// DNSSEC Trust Authorities
        /// </summary>
        TA = 32768,
        /// <summary>
        /// DNSSEC Lookaside Validation (OBSOLETE)
        /// </summary>
        DLV = 32769
    }
}
