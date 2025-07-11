using System.Text.Json.Serialization;

namespace MsmhToolsClass.V2RayConfigTool.Outbounds;

public class FreedomSettings
{
    /// <summary>
    /// Domain Strategy.
    /// Default value "AsIs".
    /// </summary>
    [JsonPropertyName("domainStrategy")]
    public string DomainStrategy { get; set; } = Get.DomainStrategy.AsIs;

    /// <summary>
    /// Freedom forces all data to be sent to the specified address (not the address specified inbound).
    /// Its value is a string, as in example: "127.0.0.1:80", ,":1234".
    /// When the address is not specified, ":443" Freedom does not change the original destination address.
    /// When the port is 0 When, such as "xray.com:0" Freedom does not modify the original port.
    /// </summary>
    [JsonPropertyName("redirect")]
    public string? Redirect { get; set; }

    /// <summary>
    /// At the user level, the connection uses the local policy corresponding to this user level.
    /// The value of userLevel, corresponding to policy levelThe value.
    /// If not specified, the default is 0.
    /// </summary>
    [JsonPropertyName("userLevel")]
    public int UserLevel { get; set; } = 0;

    /// <summary>
    /// Some key-value configuration items are used to control the issued TCP shards, and in some cases can be spoofed to the review system, such as bypassing the SNI blacklist.
    /// </summary>
    public FreedomFragment? Fragment { get; set; } = null;

    /// <summary>
    /// UDP noise, used to emit random data as "noise" before issuing a UDP connection, appears that the structure is considered enabled.
    /// May be able to deceive the sniffer, or may break the normal connection.
    /// For this reason, it bypasses port 53 because it will break DNS.
    /// </summary>
    public List<Noise> Noises { get; set; } = new();

    /// <summary>
    /// PROXY protocol is usually use "redirect", Redirect to Nginx or other backend services where the PROXY protocol is turned on.
    /// The value of proxyProtocol is the PROXY protocol version number, optional 1 or 2.
    /// If not specified, the default is 0 not activated.
    /// </summary>
    [JsonPropertyName("proxyProtocol")]
    public int ProxyProtocol { get; set; } = 0;

    public class Get
    {
        public readonly struct DomainStrategy
        {
            /// <summary>
            /// Use only the domain name for routing. The default value.
            /// </summary>
            public static readonly string AsIs = "AsIs";
            public static readonly string UseIP = "UseIP";
            public static readonly string UseIPv6v4 = "UseIPv6v4";
            public static readonly string UseIPv6 = "UseIPv6";
            public static readonly string UseIPv4v6 = "UseIPv4v6";
            public static readonly string UseIPv4 = "UseIPv4";
            public static readonly string ForceIP = "ForceIP";
            public static readonly string ForceIPv6v4 = "ForceIPv6v4";
            public static readonly string ForceIPv6 = "ForceIPv6";
            public static readonly string ForceIPv4v6 = "ForceIPv4v6";
            public static readonly string ForceIPv4 = "ForceIPv4";
        }
    }

    public class FreedomFragment
    {
        /// <summary>
        /// Supports two sharding methods:
        /// "1-3" is a stream slice of TCP that is applied to client-side write data 1 to 3.
        /// "tlshello" is a slice of the TLS handshake package.
        /// When it's set to "tlshello" and Interval is 0, Fragment is off;
        /// </summary>
        [JsonPropertyName("packets")]
        public string Packets { get; set; } = "tlshello";

        /// <summary>
        /// Sharded package length (byte).
        /// </summary>
        [JsonPropertyName("length")]
        public string Length { get; set; } = "2-4";

        /// <summary>
        /// Sharding interval (ms).
        /// When it's set to 0 and Packets is "tlshello", fragment is off;
        /// </summary>
        [JsonPropertyName("interval")]
        public string Interval { get; set; } = "3-5";
    }

    public class Noise
    {
        /// <summary>
        /// Noise packet type.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = Get.Type.Base64;

        /// <summary>
        /// Based on the "type" The content of the packet to send.
        /// When type is rand, the length of the random data is specified here, which can be a fixed value. "100" Or floating value. "50-150".
        /// When type is str, the string to be sent is specified here.
        /// When type is hex, the binary data represented in hex is specified here.
        /// When type is base64, base64 binary data is specified here.
        /// </summary>
        [JsonPropertyName("packet")]
        public string Packet { get; set; } = "7nQBAAABAAAAAAAABnQtcmluZwZtc2VkZ2UDbmV0AAABAAE=";

        /// <summary>
        /// Latency, unit of milliseconds.
        /// After sending the noise packet, the core waits for the time to send the next noise packet or real data.
        /// </summary>
        [JsonPropertyName("delay")]
        public string Delay { get; set; } = "10-15";

        public class Get
        {
            public class Type
            {
                /// <summary>
                /// The random data.
                /// </summary>
                public static readonly string Rand = "rand";
                /// <summary>
                /// User-defined strings.
                /// </summary>
                public static readonly string Base64 = "base64";
                /// <summary>
                /// Base64 encoded custom binary data.
                /// </summary>
                public static readonly string Str = "str";
            }
        }
    }
}