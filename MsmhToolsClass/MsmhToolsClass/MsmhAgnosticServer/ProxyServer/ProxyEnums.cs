namespace MsmhToolsClass.MsmhAgnosticServer;

public class Proxy
{
    public enum Name
    {
        Test = 0,
        HTTP = 1,
        HTTP_S = 2,
        HTTPS_SSL = 3,
        Socks4 = 4,
        Socks4A = 5,
        Socks5 = 6,
        SniProxy = 7
    }
}

public class Socks
{
    public enum Version
    {
        Socks5 = 0x05,
        Socks4 = 0x04,
        Zero = 0x00
    }

    public enum HandshakeMethods
    {
        NoAuth = 0x00,
        GSSAPI = 0x01,
        LoginUserPass = 0x02,
        SocksCompress = 0x88,
        SocksEncrypt = 0x90,
        NoAuthAndLogin = 0xFE,
        Unsupported = 0xFF,
    }

    public enum AddressType
    {
        Ipv4 = 0x01,
        Domain = 0x03,
        Ipv6 = 0x04,
        Unknown = 0x00
    }

    public enum Commands
    {
        Connect = 0x01, // Socks4, Socks5
        Bind = 0x02, // Socks4, Socks5
        UDP = 0x03, // Socks5
        Unknown = 0x00
    }

    public enum Status
    {
        // Socks4
        GrantedSocks4 = 0x5A,
        RejectedSocks4 = 0x5B,
        UnreachableSocks4 = 0x5C,
        UserIdFailedSocks4 = 0x5D,

        // Socks5
        Granted = 0x00,
        Failure = 0x01,
        NotAllowed = 0x02,
        NetworkUnreachable = 0x03,
        HostUnreachable = 0x04,
        Refused = 0x05,
        TtlExpired = 0x06,
        CommandNotSupported = 0x07,
        AddressNotSupported = 0x08,
        LoginRequired = 0x90
    }
}

public enum ByteType
{
    Sent,
    Received
}
