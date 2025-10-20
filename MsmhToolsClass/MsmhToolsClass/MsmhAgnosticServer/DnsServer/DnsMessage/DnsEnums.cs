namespace MsmhToolsClass.MsmhAgnosticServer;

public class DnsEnums
{
    public enum DnsProtocol
    {
        System,
        UDP,
        TCP,
        TcpOverUdp,
        DnsCrypt,
        DoT,
        DoH,
        DoQ,
        AnonymizedDNSCrypt, // DnsCrypt + AnonymizedDNSCryptRelay
        ObliviousDoH, // ObliviousDohTarget + ObliviousDohRelay
        AnonymizedDNSCryptRelay, // Relay For DnsCrypt
        ObliviousDohTarget, // A DoH That Supports Relay
        ObliviousDohRelay, // Relay For ODoH
        Unknown
    }

    public readonly struct DnsProtocolName
    {
        public const string System = "Operating System";
        public const string UDP = "UDP Plain DNS";
        public const string TCP = "TCP Plain DNS";
        public const string TcpOverUdp = "TCP Over UDP";
        public const string DnsCrypt = "DNSCrypt";
        public const string DoT = "DNS-Over-TLS";
        public const string DoH = "DNS-Over-HTTPS";
        public const string DoQ = "DNS-Over-Quic";
        public const string AnonymizedDNSCrypt = "Anonymized DNSCrypt";
        public const string ObliviousDoH = "Oblivious DoH";
        public const string AnonymizedDNSCryptRelay = "Anonymized DNSCrypt Relay";
        public const string ObliviousDohTarget = "Oblivious DoH Target";
        public const string ObliviousDohRelay = "Oblivious DoH Relay";
        public const string Unknown = "Unknown";
    }

    public static DnsProtocol GetDnsProtocolByName(string dnsProtocolName)
    {
        return dnsProtocolName switch
        {
            DnsProtocolName.System => DnsProtocol.System,
            DnsProtocolName.UDP => DnsProtocol.UDP,
            DnsProtocolName.TCP => DnsProtocol.TCP,
            DnsProtocolName.TcpOverUdp => DnsProtocol.TcpOverUdp,
            DnsProtocolName.DnsCrypt => DnsProtocol.DnsCrypt,
            DnsProtocolName.DoT => DnsProtocol.DoT,
            DnsProtocolName.DoH => DnsProtocol.DoH,
            DnsProtocolName.DoQ => DnsProtocol.DoQ,
            DnsProtocolName.AnonymizedDNSCrypt => DnsProtocol.AnonymizedDNSCrypt,
            DnsProtocolName.ObliviousDoH => DnsProtocol.ObliviousDoH,
            DnsProtocolName.AnonymizedDNSCryptRelay => DnsProtocol.AnonymizedDNSCryptRelay,
            DnsProtocolName.ObliviousDohTarget => DnsProtocol.ObliviousDohTarget,
            DnsProtocolName.ObliviousDohRelay => DnsProtocol.ObliviousDohRelay,
            _ => DnsProtocol.Unknown
        };
    }

    public enum QR : ushort // 1 Bit
    {
        Query = 0,
        Response = 1
    }

    /// <summary>
    /// DNS Operation Codes. See https://www.iana.org/assignments/dns-parameters/dns-parameters.xml#dns-parameters-5
    /// </summary>
    public enum OperationalCode : ushort // 4 Bits
    {
        /// <summary>
        /// Query
        /// </summary>
        QUERY = 0000, // 0
        /// <summary>
        /// IQuery (Inverse Query, OBSOLETE)
        /// </summary>
        IQUERY = 0001, // 1
        /// <summary>
        /// Status
        /// </summary>
        STATUS = 0010, // 2
        /// <summary>
        /// Notify
        /// </summary>
        NOTIFY = 0100, // 4
        /// <summary>
        /// Update
        /// </summary>
        UPDATE = 0101, // 5
        /// <summary>
        /// DNS Stateful Operations (DSO)
        /// </summary>
        DSO = 0110 // 6
        // 3 and  7-15 Unassigned
    }

    /// <summary>
    ///  Authoritative Answer. This flag is only valid for responses. 
    /// </summary>
    public enum AA : ushort // 1 Bit
    {
        NonAuthoritive = 0,
        Authoritive = 1
    }

    public enum TC : ushort // 1 Bit
    {
        NotTruncated = 0,
        Truncated = 1
    }

    public enum RD : ushort // 1 Bit
    {
        RecursionIsNotDesired = 0,
        RecursionIsDesired = 1
    }

    public enum RA : ushort // 1 Bit
    {
        RecursionIsNotAvailable = 0,
        RecursionIsAvailable = 1
    }

    public enum Z : ushort // 2 Bits
    {
        Reserved = 00 // Reserved for future use. Must be zero in all queries and responses
    }

    public enum AnswerAuthenticated : ushort // 1 Bit
    {
        False = 0,
        True = 1
    }

    public enum NonAuthenticatedData : ushort // 1 Bit
    {
        False = 0,
        True = 1
    }

    public enum ResponseCode : ushort // 4 Bits
    {
        NoError = 0000, // 0
        FormatError = 0001, // 1
        ServerFailure = 0010, // 2
        NameError = 0011, // 3
        NotImplemented = 0100, // 4
        Refused = 0101 // 5
        // 6-15 Reserved for future use
    }

    /// <summary>
    /// DNS Resource Record Type. See https://www.iana.org/assignments/dns-parameters/dns-parameters.xml#dns-parameters-4
    /// </summary>
    public enum RRType : ushort
    {
        Unknown = 0,
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
        /// Host Identity ListenerProtocol
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

    public static RRType ParseRRType(ushort n)
    {
        return n switch
        {
            1 => RRType.A,
            2 => RRType.NS,
            3 => RRType.MD,
            4 => RRType.MF,
            5 => RRType.CNAME,
            6 => RRType.SOA,
            7 => RRType.MB,
            8 => RRType.MG,
            9 => RRType.MR,
            10 => RRType.NULL,
            11 => RRType.WKS,
            12 => RRType.PTR,
            13 => RRType.HINFO,
            14 => RRType.MINFO,
            15 => RRType.MX,
            16 => RRType.TEXT,
            17 => RRType.RP,
            18 => RRType.AFSDB,
            19 => RRType.X25,
            20 => RRType.ISDN,
            21 => RRType.RT,
            22 => RRType.NSAP,
            23 => RRType.NSAPPTR,
            24 => RRType.SIG,
            25 => RRType.KEY,
            26 => RRType.PX,
            27 => RRType.GPOS,
            28 => RRType.AAAA,
            29 => RRType.LOC,
            30 => RRType.NXT,
            31 => RRType.EID,
            32 => RRType.NIMLOC,
            33 => RRType.SRV,
            34 => RRType.ATMA,
            35 => RRType.NAPTR,
            36 => RRType.KX,
            37 => RRType.CERT,
            38 => RRType.A6,
            39 => RRType.DNAME,
            40 => RRType.SINK,
            41 => RRType.OPT,
            42 => RRType.APL,
            43 => RRType.DS,
            44 => RRType.SSHFP,
            45 => RRType.IPSECKEY,
            46 => RRType.RRSIG,
            47 => RRType.NSEC,
            48 => RRType.DNSKEY,
            49 => RRType.DHCID,
            50 => RRType.NSEC3,
            51 => RRType.NSEC3PARAM,
            52 => RRType.TLSA,
            53 => RRType.SMIMEA,
            55 => RRType.HIP,
            56 => RRType.NINFO,
            57 => RRType.RKEY,
            58 => RRType.TALINK,
            59 => RRType.CDS,
            60 => RRType.CDNSKEY,
            61 => RRType.OPENPGPKEY,
            62 => RRType.CSYNC,
            63 => RRType.ZONEMD,
            64 => RRType.SVCB,
            65 => RRType.HTTPS,
            99 => RRType.SPF,
            100 => RRType.UINFO,
            101 => RRType.UID,
            102 => RRType.GID,
            103 => RRType.UNSPEC,
            104 => RRType.NID,
            105 => RRType.L32,
            106 => RRType.L64,
            107 => RRType.LP,
            108 => RRType.EUI48,
            109 => RRType.EUI64,
            249 => RRType.TKEY,
            250 => RRType.TSIG,
            251 => RRType.IXFR,
            252 => RRType.AXFR,
            253 => RRType.MAILB,
            254 => RRType.MAILA,
            255 => RRType.ANY,
            256 => RRType.URI,
            257 => RRType.CAA,
            258 => RRType.AVC,
            259 => RRType.DOA,
            260 => RRType.AMTRELAY,
            32768 => RRType.TA,
            32769 => RRType.DLV,
            _ => RRType.Unknown
        };
    }

    public static RRType ParseRRType(string rrType)
    {
        rrType = rrType.ToUpper();
        return rrType switch
        {
            nameof(RRType.A) => RRType.A,
            nameof(RRType.NS) => RRType.NS,
            nameof(RRType.MD) => RRType.MD,
            nameof(RRType.MF) => RRType.MF,
            nameof(RRType.CNAME) => RRType.CNAME,
            nameof(RRType.SOA) => RRType.SOA,
            nameof(RRType.MB) => RRType.MB,
            nameof(RRType.MG) => RRType.MG,
            nameof(RRType.MR) => RRType.MR,
            nameof(RRType.NULL) => RRType.NULL,
            nameof(RRType.WKS) => RRType.WKS,
            nameof(RRType.PTR) => RRType.PTR,
            nameof(RRType.HINFO) => RRType.HINFO,
            nameof(RRType.MINFO) => RRType.MINFO,
            nameof(RRType.MX) => RRType.MX,
            nameof(RRType.TEXT) => RRType.TEXT,
            nameof(RRType.RP) => RRType.RP,
            nameof(RRType.AFSDB) => RRType.AFSDB,
            nameof(RRType.X25) => RRType.X25,
            nameof(RRType.ISDN) => RRType.ISDN,
            nameof(RRType.RT) => RRType.RT,
            nameof(RRType.NSAP) => RRType.NSAP,
            nameof(RRType.NSAPPTR) => RRType.NSAPPTR,
            nameof(RRType.SIG) => RRType.SIG,
            nameof(RRType.KEY) => RRType.KEY,
            nameof(RRType.PX) => RRType.PX,
            nameof(RRType.GPOS) => RRType.GPOS,
            nameof(RRType.AAAA) => RRType.AAAA,
            nameof(RRType.LOC) => RRType.LOC,
            nameof(RRType.NXT) => RRType.NXT,
            nameof(RRType.EID) => RRType.EID,
            nameof(RRType.NIMLOC) => RRType.NIMLOC,
            nameof(RRType.SRV) => RRType.SRV,
            nameof(RRType.ATMA) => RRType.ATMA,
            nameof(RRType.NAPTR) => RRType.NAPTR,
            nameof(RRType.KX) => RRType.KX,
            nameof(RRType.CERT) => RRType.CERT,
            nameof(RRType.A6) => RRType.A6,
            nameof(RRType.DNAME) => RRType.DNAME,
            nameof(RRType.SINK) => RRType.SINK,
            nameof(RRType.OPT) => RRType.OPT,
            nameof(RRType.APL) => RRType.APL,
            nameof(RRType.DS) => RRType.DS,
            nameof(RRType.SSHFP) => RRType.SSHFP,
            nameof(RRType.IPSECKEY) => RRType.IPSECKEY,
            nameof(RRType.RRSIG) => RRType.RRSIG,
            nameof(RRType.NSEC) => RRType.NSEC,
            nameof(RRType.DNSKEY) => RRType.DNSKEY,
            nameof(RRType.DHCID) => RRType.DHCID,
            nameof(RRType.NSEC3) => RRType.NSEC3,
            nameof(RRType.NSEC3PARAM) => RRType.NSEC3PARAM,
            nameof(RRType.TLSA) => RRType.TLSA,
            nameof(RRType.SMIMEA) => RRType.SMIMEA,
            nameof(RRType.HIP) => RRType.HIP,
            nameof(RRType.NINFO) => RRType.NINFO,
            nameof(RRType.RKEY) => RRType.RKEY,
            nameof(RRType.TALINK) => RRType.TALINK,
            nameof(RRType.CDS) => RRType.CDS,
            nameof(RRType.CDNSKEY) => RRType.CDNSKEY,
            nameof(RRType.OPENPGPKEY) => RRType.OPENPGPKEY,
            nameof(RRType.CSYNC) => RRType.CSYNC,
            nameof(RRType.ZONEMD) => RRType.ZONEMD,
            nameof(RRType.SVCB) => RRType.SVCB,
            nameof(RRType.HTTPS) => RRType.HTTPS,
            nameof(RRType.SPF) => RRType.SPF,
            nameof(RRType.UINFO) => RRType.UINFO,
            nameof(RRType.UID) => RRType.UID,
            nameof(RRType.GID) => RRType.GID,
            nameof(RRType.UNSPEC) => RRType.UNSPEC,
            nameof(RRType.NID) => RRType.NID,
            nameof(RRType.L32) => RRType.L32,
            nameof(RRType.L64) => RRType.L64,
            nameof(RRType.LP) => RRType.LP,
            nameof(RRType.EUI48) => RRType.EUI48,
            nameof(RRType.EUI64) => RRType.EUI64,
            nameof(RRType.TKEY) => RRType.TKEY,
            nameof(RRType.TSIG) => RRType.TSIG,
            nameof(RRType.IXFR) => RRType.IXFR,
            nameof(RRType.AXFR) => RRType.AXFR,
            nameof(RRType.MAILB) => RRType.MAILB,
            nameof(RRType.MAILA) => RRType.MAILA,
            nameof(RRType.ANY) => RRType.ANY,
            nameof(RRType.URI) => RRType.URI,
            nameof(RRType.CAA) => RRType.CAA,
            nameof(RRType.AVC) => RRType.AVC,
            nameof(RRType.DOA) => RRType.DOA,
            nameof(RRType.AMTRELAY) => RRType.AMTRELAY,
            nameof(RRType.TA) => RRType.TA,
            nameof(RRType.DLV) => RRType.DLV,
            _ => RRType.Unknown
        };
    }

    public enum CLASS : ushort
    {
        Unknown = 0,
        IN = 1, // The Internet system
        CS = 2, // The CSNET class (Obsolete - used only for examples in some obsolete RFCs)
        CH = 3, // The Chaos system
        HS = 4, // Hesiod [Dyer 87]
    }

    public static CLASS ParseClass(ushort n)
    {
        return n switch
        {
            1 => CLASS.IN,
            2 => CLASS.CS,
            3 => CLASS.CH,
            4 => CLASS.HS,
            _ => CLASS.Unknown
        };
    }

    public static CLASS ParseClass(string qClass)
    {
        qClass = qClass.ToUpper();
        return qClass switch
        {
            nameof(CLASS.IN) => CLASS.IN,
            nameof(CLASS.CS) => CLASS.CS,
            nameof(CLASS.CH) => CLASS.CH,
            nameof(CLASS.HS) => CLASS.HS,
            _ => CLASS.Unknown
        };
    }

}