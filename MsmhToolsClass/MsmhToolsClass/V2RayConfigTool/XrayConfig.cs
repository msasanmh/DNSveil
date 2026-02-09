using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MsmhToolsClass.V2RayConfigTool;

public class XrayConfig
{
    [JsonIgnore]
    public bool IsSuccess { get; set; } = false;

    /// <summary>
    /// The Name Of This Config.
    /// </summary>
    [JsonPropertyName("remarks")]
    public string Remarks { get; set; } = "Xray Config By DNSveil";

    /// <summary>
    /// Log configuration, control the way Xray output logs are.
    /// </summary>
    [JsonPropertyName("log")]
    public ConfigLog Log { get; set; } = new();

    /// <summary>
    /// Some API interfaces are available for remote calls.
    /// </summary>
    [JsonPropertyName("api")]
    public ConfigApi? Api { get; set; } = null;

    /// <summary>
    /// Built-in DNS server. If this is not configured, the system's DNS settings are used.
    /// </summary>
    [JsonPropertyName("dns")]
    public ConfigDns Dns { get; set; } = new();

    /// <summary>
    /// Local policies that allow different user levels and corresponding policy settings.
    /// </summary>
    [JsonPropertyName("policy")]
    public ConfigPolicy Policy { get; set; } = new();

    /// <summary>
    /// An array, each element is an inbound connection configuration.
    /// </summary>
    [JsonPropertyName("inbounds")]
    public List<ConfigInbound> Inbounds { get; set; } = new();

    /// <summary>
    /// An array, each element is an outbound connection configuration.
    /// </summary>
    [JsonPropertyName("outbounds")]
    public List<ConfigOutbound> Outbounds { get; set; } = new();

    /// <summary>
    /// The routing function. You can set the rules to shunt data from different outbounds.
    /// </summary>
    [JsonPropertyName("routing")]
    public ConfigRouting Routing { get; set; } = new();

    /// <summary>
    /// Used to configure Xray for other servers to establish and use network connections.
    /// </summary>
    [JsonPropertyName("transport")]
    public Dictionary<string, string> Transport { get; set; } = new();

    /// <summary>
    /// Statistics for configuring traffic data.
    /// </summary>
    [JsonPropertyName("stats")]
    public Dictionary<string, string> Stats { get; set; } = new();

    /// <summary>
    /// Reverse agent. Server-side traffic can be forwarded to the client, that is, reverse traffic forwarding.
    /// </summary>
    [JsonPropertyName("reverse")]
    public Dictionary<string, string> Reverse { get; set; } = new();

    /// <summary>
    /// FakeDNS configuration. Can be used with a transparent proxy to obtain the actual domain name.
    /// </summary>
    [JsonPropertyName("fakedns")]
    public Dictionary<string, string> Fakedns { get; set; } = new();

    /// <summary>
    /// Measures are configured. More direct (hopefully) a more direct (hopefully better) way of exporting statistics.
    /// </summary>
    [JsonPropertyName("metrics")]
    public Dictionary<string, string> Metrics { get; set; } = new();

    /// <summary>
    /// Background connection observation. Detects the connection status of the outbound agent.
    /// </summary>
    [JsonPropertyName("observatory")]
    public Dictionary<string, string> Observatory { get; set; } = new();

    /// <summary>
    /// Concurrent connection observation. Detects the connection status of the outbound agent.
    /// </summary>
    [JsonPropertyName("burstObservatory")]
    public Dictionary<string, string> BurstObservatory { get; set; } = new();

    // Log
    public class ConfigLog
    {
        /// <summary>
        /// Access Log File Path. If this option is not specified or is empty, the log will be write to stdout.
        /// </summary>
        [JsonPropertyName("access")]
        public string? Access { get; set; } = null;

        /// <summary>
        /// Error Log File Path. If this option is not specified or is empty, the log will be write to stdout.
        /// </summary>
        [JsonPropertyName("error")]
        public string? Error { get; set; } = null;

        /// <summary>
        /// // "debug" | "info" | "warning" | "error" | "none"
        /// </summary>
        [JsonPropertyName("loglevel")]
        public string LogLevel { get; set; } = Get.LogLevel.Warning;

        [JsonPropertyName("dnsLog")]
        public bool DnsLog { get; set; } = false;

        /// <summary>
        /// "quarter" | "half" | "full" - IP address mask, when enabled, will automatically replace the IP address that appears in the log, used to protect privacy when sharing logs. The default is empty.
        /// </summary>
        [JsonPropertyName("maskAddress")]
        public string MaskAddress { get; set; } = Get.MaskAddress.None;

        public class Get
        {
            public readonly struct LogLevel
            {
                public static readonly string None = "none";
                public static readonly string Error = "error";
                public static readonly string Warning = "warning";
                public static readonly string Info = "info";
                public static readonly string Debug = "debug";
            }

            public readonly struct MaskAddress
            {
                public static readonly string None = string.Empty;
                public static readonly string Quarter = "quarter";
                public static readonly string Half = "half";
                public static readonly string Full = "full";
            }
        }
    }

    // API
    public class ConfigApi
    {
        /// <summary>
        /// Outbound Proxy ID.
        /// </summary>
        [JsonPropertyName("tag")]
        public string Tag { get; set; } = "api";

        /// <summary>
        /// The IP and port that the API service listens on. This is an optional configuration item.
        /// </summary>
        [JsonPropertyName("listen")]
        public string? Listen { get; set; } = null;

        /// <summary>
        /// The IP and port that the API service listens on. This is an optional configuration item.
        /// </summary>
        [JsonPropertyName("services")]
        public List<string> Services { get; set; } = new()
        {
            "HandlerService",
            "RoutingService",
            "LoggerService",
            "StatsService",
            "ReflectionService",
        };
    }

    // DNS
    public class ConfigDns
    {
        /// <summary>
        /// Query traffic sent by the built-in DNS, except localhost, fakedns, TCPL, DOHL and DOQL This flag can be used in routing except for mode inboundTag to make a match.
        /// </summary>
        [JsonPropertyName("tag")]
        public string Tag { get; set; } = "dns";

        /// <summary>
        /// "domain name - domain name" or "domain name - IP" or "domain name - IP List"
        /// </summary>
        [JsonPropertyName("hosts")]
        public Dictionary<string, List<string>> Hosts { get; set; } = new();

        /// <summary>
        /// A DNS Server
        /// </summary>
        [JsonPropertyName("servers")]
        public List<Server> Servers { get; set; } = new();

        /// <summary>
        /// Used to notify the server to specify the IP location when querying DNS. It cannot be a private address.
        /// </summary>
        [JsonPropertyName("clientIP")]
        public string? ClientIP { get; set; } = null;

        [JsonPropertyName("queryStrategy")]
        public string QueryStrategy { get; set; } = Get.QueryStrategy.UseIP;

        [JsonPropertyName("disableCache")]
        public bool DisableCache { get; set; } = false;

        [JsonPropertyName("disableFallback")]
        public bool DisableFallback { get; set; } = false;

        /// <summary>
        /// true When the DNS server's priority matching domain name list is hit, disable fallback query, the default is false, that is, not disabled.
        /// </summary>
        [JsonPropertyName("disableFallbackIfMatch")]
        public bool DisableFallbackIfMatch { get; set; } = false;

        public class Get
        {
            public readonly struct QueryStrategy
            {
                public static readonly string UseIP = "UseIP";
                public static readonly string UseIPv4 = "UseIPv4";
                public static readonly string UseIPv6 = "UseIPv6";
            }
        }

        public class Server
        {
            /// <summary>
            /// The tag of this DNS server. If set, it will use this tag as the inbound tag to initiate requests (non-local mode), overwriting the global tag option.
            /// </summary>
            [JsonPropertyName("tag")]
            public string? Tag { get; set; } = null;

            /// <summary>
            /// DNS Address
            /// </summary>
            [JsonPropertyName("address")]
            public string Address { get; set; } = "tcp://8.8.8.8";

            /// <summary>
            /// DNS Port
            /// </summary>
            [JsonPropertyName("port")]
            public int Port { get; set; } = 53;

            /// <summary>
            /// A domain name list. The domain names included in this list will be queried using this server first.
            /// </summary>
            [JsonPropertyName("domains")]
            public List<string> Domains { get; set; } = new();

            /// <summary>
            /// A list of IP ranges. CIDR and geoip:ir Format.
            /// When this option is configured, Xray DNS will verify the returned IP addresses and only return the addresses included in the expectIPs list.
            /// If this is not configured, the IP address is returned unchanged.
            /// </summary>
            [JsonPropertyName("expectIPs")]
            public List<string> ExpectIPs { get; set; } = new();

            /// <summary>
            /// true, this server will be skipped when performing DNS fallback queries, the default is false, that is, not skipped.
            /// </summary>
            [JsonPropertyName("skipFallback")]
            public bool SkipFallback { get; set; } = true;

            /// <summary>
            /// DNS server timeout, default 4000 ms
            /// </summary>
            [JsonPropertyName("timeoutMs")]
            public int TimeoutMs { get; set; } = 6000;

            /// <summary>
            /// If enabled, expectIPs After filtering the IP, if all IPs do not meet the conditions and are filtered, the IPs are still returned, otherwise it is considered that the query failed
            /// </summary>
            [JsonPropertyName("allowUnexpectedIPs")]
            public bool AllowUnexpectedIPs { get; set; } = true;

            /// <summary>
            /// Used to notify the server to specify the IP location when querying DNS. It cannot be a private address.
            /// </summary>
            [JsonPropertyName("clientIP")]
            public string? ClientIP { get; set; } = null;

            [JsonPropertyName("queryStrategy")]
            public string QueryStrategy { get; set; } = Get.QueryStrategy.UseIP;
        }
    }

    // Policy
    public class ConfigPolicy
    {
        /// <summary>
        /// A set of key-value pairs, each of which is a number in the form of a string (JSON's requirements), such as "0", "1" So, double quotes can not be omitted.
        /// This number corresponds to the user level. Each value is a levelPolicyObject.
        /// </summary>
        [JsonPropertyName("levels")]
        public Dictionary<string, LevelPolicy> Levels { get; set; } = new();

        [JsonPropertyName("system")]
        public SystemPolicy System { get; set; } = new();

        public class LevelPolicy
        {
            /// <summary>
            /// The time limit for shaking hands when the connection is established. Units in seconds.
            /// The default value is 4.
            /// </summary>
            [JsonPropertyName("handshake")]
            public int Handshake { get; set; } = 4;

            /// <summary>
            /// Time limit for connecting idle. Units in seconds.
            /// The default value is 300.
            /// </summary>
            [JsonPropertyName("connIdle")]
            public int ConnIdle { get; set; } = 300;

            /// <summary>
            /// Time limit after the connection is closed down. Units in seconds.
            /// The default value is 2.
            /// </summary>
            [JsonPropertyName("uplinkOnly")]
            public int UplinkOnly { get; set; } = 2;

            /// <summary>
            /// Time limit when the connection is closed after the line is closed. Units in seconds.
            /// The default value is 5.
            /// </summary>
            [JsonPropertyName("downlinkOnly")]
            public int DownlinkOnly { get; set; } = 5;

            /// <summary>
            /// Turn on the uplink traffic statistics for all users at the current level
            /// </summary>
            [JsonPropertyName("statsUserUplink")]
            public bool StatsUserUplink { get; set; } = false;

            /// <summary>
            /// Turn on the downlink traffic statistics for all users at the current level.
            /// </summary>
            [JsonPropertyName("statsUserDownlink")]
            public bool StatsUserDownlink { get; set; } = false;

            /// <summary>
            /// The internal cache size of each request is KB.
            /// When the internal cache is greater than that value, the next write is made only after the internal cache is issued until less than or equal to the value.
            /// Note that for a UDP request, if the cache is full when the write is attempted, the write operation is not blocked, but is discarded, and if set to be too low or 0 may cause unexpected broadband waste.
            /// </summary>
            [JsonPropertyName("bufferSize")]
            public int BufferSize { get; set; } = 10240; // 10 MB in binary
        }

        public class SystemPolicy
        {
            /// <summary>
            /// Turn on the uplink traffic statistics of all inbound agents.
            /// </summary>
            [JsonPropertyName("statsInboundUplink")]
            public bool StatsInboundUplink { get; set; } = false;

            /// <summary>
            /// Turn on the downstream traffic statistics of all inbound agents.
            /// </summary>
            [JsonPropertyName("statsInboundDownlink")]
            public bool StatsInboundDownlink { get; set; } = false;

            /// <summary>
            /// Turn on the uplink traffic statistics of all outbound agents.
            /// </summary>
            [JsonPropertyName("statsOutboundUplink")]
            public bool StatsOutboundUplink { get; set; } = true;

            /// <summary>
            /// Turn on the downstream traffic statistics of all outbound agents.
            /// </summary>
            [JsonPropertyName("statsOutboundDownlink")]
            public bool StatsOutboundDownlink { get; set; } = true;
        }
    }

    // Inbounds
    public class ConfigInbound
    {
        /// <summary>
        /// The tag of this Inbound
        /// </summary>
        [JsonPropertyName("tag")]
        public string Tag { get; set; } = "socks-in";

        /// <summary>
        /// Listening address, IP address or Unix domain socket, the default value is "0.0.0.0", indicating receiving connections on all network cards.
        /// </summary>
        [JsonPropertyName("listen")]
        public string Listen { get; set; } = IPAddress.Any.ToString();

        /// <summary>
        /// Listening Port
        /// </summary>
        [JsonPropertyName("port")]
        public int Port { get; set; } = 10808;

        /// <summary>
        /// Connection protocol name.
        /// </summary>
        [JsonPropertyName("protocol")]
        public string Protocol { get; set; } = Get.Protocol.Mixed;

        /// <summary>
        /// The specific configuration content varies depending on the protocol.
        /// </summary>
        public object? Settings { get; set; }

        /// <summary>
        /// The underlying transport mode is the way the current Xray node connects with other nodes.
        /// </summary>
        public InboundStreamSettings StreamSettings { get; set; } = new();

        /// <summary>
        /// Traffic detection is mainly used in transparent proxy and other purposes.
        /// </summary>
        public InboundSniffing? Sniffing { get; set; } = null;

        /// <summary>
        /// When multiple ports are set, the specific settings for port allocation.
        /// Discard: I'm Using One Int As Port.
        /// </summary>
        public InboundAllocate? Allocate { get; set; } = null;

        public class Get
        {
            public readonly struct Protocol
            {
                public static readonly string Mixed = "mixed";
                public static readonly string Dokodemo_door = "dokodemo-door";
                public static readonly string Http = "http";
                public static readonly string Shadowsocks = "shadowsocks";
                public static readonly string Socks = "socks";
                public static readonly string Vless = "vless";
                public static readonly string Vmess = "vmess";
                public static readonly string Trojan = "trojan";
                public static readonly string Wireguard = "wireguard";
            }
        }

        public class InboundSettings : object
        {
            // The specific content of the configuration varies depending on the protocol.
        }

        public class InboundStreamSettings
        {
            // Default Is Ok. (This Is Advanced Settings For Server Implementation)
        }

        public class InboundSniffing
        {
            /// <summary>
            /// Whether to enable flow detection.
            /// </summary>
            [JsonPropertyName("enabled")]
            public bool Enabled { get; set; } = true;

            /// <summary>
            /// When traffic is of a specified type, resets the destination of the current connection by the destination address included in the traffic.
            /// </summary>
            [JsonPropertyName("destOverride")]
            public List<string> DestOverride { get; set; } = new();

            /// <summary>
            /// When enabled, only the connection metadata will be used to sniff the destination address.
            /// FakednsOther sniffers will not be activated (including fakedns+others）
            /// </summary>
            [JsonPropertyName("metadataOnly")]
            public bool MetadataOnly { get; set; } = false;

            /// <summary>
            /// A domain name list. If the traffic detection result is in this list, the target address will not be reset.
            /// </summary>
            [JsonPropertyName("domainsExcluded")]
            public List<string> DomainsExcluded { get; set; } = new();

            /// <summary>
            /// The sniffed domain name is only used for routing, and the proxy target address is still IP.
            /// The default value is false.
            /// This needs to be turned on destOverride use.
            /// </summary>
            [JsonPropertyName("routeOnly")]
            public bool RouteOnly { get; set; } = false;

            public class Get
            {
                public readonly struct DestOverride
                {
                    public static readonly string Http = "http";
                    public static readonly string Tls = "tls";
                    public static readonly string Quic = "quic";
                    public static readonly string Shadowsocks = "shadowsocks";
                    public static readonly string Fakedns = "fakedns";
                    /// <summary>
                    /// ["fakedns+others"] Is Equivalent to ["http", "tls", "quic", "fakedns"]
                    /// </summary>
                    public static readonly string FakednsAndOthers = "fakedns+others";
                }
            }

            public InboundSniffing()
            {
                DestOverride = new()
                {
                    Get.DestOverride.Http,
                    Get.DestOverride.Tls,
                };
            }
        }

        public class InboundAllocate
        {
            // Discard: I'm Using One Int As Port.
        }
    }

    // Outbounds
    public class ConfigOutbound
    {
        /// <summary>
        /// This outbound connection is the identity used to locate this connection in other configurations.
        /// </summary>
        [JsonPropertyName("tag")]
        public string Tag { get; set; } = "proxy-out";

        /// <summary>
        /// IP address for sending data, valid when the host has multiple IP addresses, the default value is "0.0.0.0".
        /// </summary>
        [JsonPropertyName("sendThrough")]
        public string SendThrough { get; set; } = IPAddress.Any.ToString();

        /// <summary>
        /// Connection Protocol
        /// </summary>
        [JsonPropertyName("protocol")]
        public string Protocol { get; set; } = Get.Protocol.Vless;

        /// <summary>
        /// The specific content of the configuration varies depending on the protocol.
        /// </summary>
        [JsonPropertyName("settings")]
        public object? Settings { get; set; }

        /// <summary>
        /// The underlying transport is the way the current Xray node and other nodes are docked.
        /// </summary>
        [JsonPropertyName("streamSettings")]
        public OutboundStreamSettings StreamSettings { get; set; } = new();

        /// <summary>
        /// Outbound agent configuration. When the outbound agent takes effect, this outbound streamSettings It will not work.
        /// </summary>
        [JsonPropertyName("proxySettings")]
        public OutboundProxySettings? ProxySettings { get; set; } = null;

        /// <summary>
        /// Mux-related specific configuration.
        /// </summary>
        [JsonPropertyName("mux")]
        public OutboundMux Mux { get; set; } = new();

        public class Get
        {
            public readonly struct Protocol
            {
                public static readonly string Blackhole = "blackhole";
                public static readonly string Dns = "dns";
                public static readonly string Freedom = "freedom";
                public static readonly string Http = "http";
                public static readonly string Loopback = "loopback";
                public static readonly string Shadowsocks = "shadowsocks";
                public static readonly string Socks = "socks";
                public static readonly string Trojan = "trojan";
                public static readonly string Vless = "vless";
                public static readonly string Vmess = "vmess";
                public static readonly string Wireguard = "wireguard";
            }
        }

        public class OutboundSettings : object
        {
            // The specific content of the configuration varies depending on the protocol.
        }

        public class OutboundStreamSettings
        {
            /// <summary>
            /// The type of transmission method used by the connected data stream, the default value is "tcp".
            /// </summary>
            [JsonPropertyName("network")]
            public string Network { get; set; } = Get.Network.Tcp;

            /// <summary>
            /// Whether transport layer encryption is enabled.
            /// </summary>
            [JsonPropertyName("security")]
            public string Security { get; set; } = Get.Security.None;

            /// <summary>
            /// The TLS configuration. TLS is provided by Golang, and usually TLS is negotiated as using TLS 1.3 and does not support DTLS.
            /// </summary>
            [JsonPropertyName("tlsSettings")]
            public StreamTlsSettings? TlsSettings { get; set; } = null;

            /// <summary>
            /// Reality configuration. Reality is Xray’s original black technology. Reality is more secure than TLS and is configured in a manner consistent with TLS.
            /// </summary>
            [JsonPropertyName("realitySettings")]
            public StreamRealitySettings? RealitySettings { get; set; } = null;

            /// <summary>
            /// The TCP (RAW) configuration of the current connection is valid only if the connection is using TCP.
            /// </summary>
            [JsonPropertyName("tcpSettings")]
            public StreamTcpSettings? TcpSettings { get; set; } = null;

            /// <summary>
            /// h2 Network.
            /// </summary>
            [JsonPropertyName("httpSettings")]
            public StreamHttpSettings? HttpSettings { get; set; } = null;

            /// <summary>
            /// The XHTTP configuration of the current connection is valid only if the connection uses XHTTP.
            /// </summary>
            [JsonPropertyName("xhttpSettings")]
            public StreamXHttpSettings? XHttpSettings { get; set; } = null;

            [JsonPropertyName("quicSettings")]
            public StreamQuicSettings? QuicSettings { get; set; } = null;

            /// <summary>
            /// The mKCP configuration of the current connection is valid only if the connection uses mKCP.
            /// </summary>
            [JsonPropertyName("kcpSettings")]
            public StreamKcpSettings? KcpSettings { get; set; } = null;

            /// <summary>
            /// The current gRPC configuration of the connection is valid only if the connection is using gRPC.
            /// </summary>
            [JsonPropertyName("grpcSettings")]
            public StreamGrpcSettings? GrpcSettings { get; set; } = null;

            /// <summary>
            /// The WebSocket configuration for the current connection is valid only if the connection uses WebSocket.
            /// </summary>
            [JsonPropertyName("wsSettings")]
            public StreamWsSettings? WsSettings { get; set; } = null;

            /// <summary>
            /// The HTTPUpgrade configuration for the current connection is valid only if the connection is using HTTPUpgrade.
            /// </summary>
            [JsonPropertyName("httpupgradeSettings")]
            public StreamHttpUpgradeSettings? HttpUpgradeSettings { get; set; } = null;

            /// <summary>
            /// Specific configuration related to transparent agent.
            /// </summary>
            [JsonPropertyName("sockopt")]
            public StreamSockopt Sockopt { get; set; } = new();

            public class Get
            {
                public class Network
                {
                    public static readonly string Tcp = "tcp"; // raw
                    public static readonly string H2 = "h2"; // In URL: type=http
                    public static readonly string Xhttp = "xhttp";
                    public static readonly string Kcp = "kcp";
                    public static readonly string Grpc = "grpc";
                    public static readonly string Ws = "ws";
                    public static readonly string HttpUpgrade = "httpupgrade";
                }

                public class Security
                {
                    public static readonly string None = "none";
                    public static readonly string Tls = "tls";
                    public static readonly string Reality = "reality";
                }
            }

            public class StreamTlsSettings
            {
                /// <summary>
                /// Specify the domain name of the server-side certificate, which is useful when the connection is established by the IP.
                /// </summary>
                [JsonPropertyName("serverName")]
                public string? ServerName { get; set; } = null;

                /// <summary>
                /// Only the client, the SNI used for the school verification certificate, will override the text used for the verification serverName For special purposes such as domain front.
                /// </summary>
                [JsonPropertyName("verifyPeerCertInNames")]
                public List<string> VerifyPeerCertInNames { get; set; } = new();

                /// <summary>
                /// When the value is trueWhen the SNI received by the server does not match the certificate domain name, it rejects the TLS handshake, which defaults to false.
                /// </summary>
                [JsonPropertyName("rejectUnknownSni")]
                public bool RejectUnknownSni { get; set; } = false;

                /// <summary>
                /// Whether an insecure connection is allowed (only for the client). The default value is false.
                /// </summary>
                [JsonPropertyName("allowInsecure")]
                public bool AllowInsecure { get; set; } = false;

                /// <summary>
                /// Deprecated In Xray.
                /// An array of strings that specifies the ALPN value specified when the TLS handshake is held.
                /// The default value is ["h2", "http/1.1"]
                /// </summary>
                [JsonPropertyName("alpn")]
                public List<string> Alpn { get; set; } = new();

                /// <summary>
                /// minVersion is the minimum acceptable version of TLS.
                /// </summary>
                [JsonPropertyName("minVersion")]
                public string MinVersion { get; set; } = "1.2";

                /// <summary>
                /// maxVersion is the maximum acceptable version of TLS.
                /// </summary>
                [JsonPropertyName("maxVersion")]
                public string MaxVersion { get; set; } = "1.3";

                /// <summary>
                /// CipherSuites is used to configure a list of supported cipher suites, separated between the names of each package.
                /// </summary>
                [JsonPropertyName("cipherSuites")]
                public string? CipherSuites { get; set; } = null;

                /// <summary>
                /// A list of certificates, each of which represents a certificate (fullchain is recommended).
                /// </summary>
                [JsonPropertyName("certificates")]
                public List<Certificate> Certificates { get; set; } = new();

                /// <summary>
                /// Disable the CA certificate that comes with the operating system. The default value is false.
                /// </summary>
                [JsonPropertyName("disableSystemRoot")]
                public bool DisableSystemRoot { get; set; } = false;

                /// <summary>
                /// Whether session recovery is enabled, disabled by default, and try to negotiate session recovery only when both the server and the client are enabled.
                /// </summary>
                [JsonPropertyName("enableSessionResumption")]
                public bool EnableSessionResumption { get; set; } = false;

                /// <summary>
                /// This parameter is used to configure the designation TLS Client Hello fingerprints.
                /// When its value is empty, this feature is not enabled.
                /// </summary>
                [JsonPropertyName("fingerprint")]
                public string Fingerprint { get; set; } = Get.Fingerprint.Firefox;

                /// <summary>
                /// 
                /// </summary>
                [JsonPropertyName("pinnedPeerCertificateChainSha256")]
                public List<string> PinnedPeerCertificateChainSha256 { get; set; } = new();

                /// <summary>
                /// An array of strings that specifies the curves preferred when the TLS handshake is executed.
                /// </summary>
                [JsonPropertyName("curvePreferences")]
                public List<string> CurvePreferences { get; set; } = new();

                /// <summary>
                /// (Pre)-Master-Secret log file path, which can be used to decrypt TLS connections sent by Xray by software such as Wireshark, is not supported for use with utls.
                /// </summary>
                [JsonPropertyName("masterKeyLog")]
                public string? MasterKeyLog { get; set; } = null;

                public class Get
                {
                    public class Alpn
                    {
                        public static readonly string XHTTP = "xhttp";
                        public static readonly string H3 = "h3";
                        public static readonly string H2 = "h2";
                        public static readonly string Http11 = "http/1.1";
                    }

                    public class Fingerprint
                    {
                        public static readonly string Random = "random";
                        public static readonly string Randomized = "randomized";
                        public static readonly string Chrome = "chrome";
                        public static readonly string Firefox = "firefox";
                        public static readonly string Safari = "safari";
                        public static readonly string Ios = "ios";
                        public static readonly string Android = "android";
                        public static readonly string Edge = "edge";
                        public static readonly string F360 = "360";
                        public static readonly string Qq = "qq";
                    }

                    public class CurvePreferences
                    {
                        public static readonly string CurveP256 = "CurveP256";
                        public static readonly string CurveP384 = "CurveP384";
                        public static readonly string CurveP521 = "CurveP521";
                        public static readonly string X25519 = "X25519";
                        public static readonly string X25519Kyber768Draft00 = "x25519Kyber768Draft00";
                    }
                }

                public class Certificate
                {
                    /// <summary>
                    /// OCSP binding updates, time interval with certificate hot overload. Unit: Seconds. The default value is 3600 That is, an hour.
                    /// </summary>
                    [JsonPropertyName("ocspStapling")]
                    public int OcspStapling { get; set; } = 3600;

                    /// <summary>
                    /// Loaded only once. Value is trueThe certificate thermal overload function and ocspSappling function are turned off.
                    /// </summary>
                    [JsonPropertyName("oneTimeLoading")]
                    public bool OneTimeLoading { get; set; } = false;

                    /// <summary>
                    /// Certificate use, default value is "encipherment"
                    /// </summary>
                    [JsonPropertyName("usage")]
                    public string Usage { get; set; } = Get.Usage.Encipherment;

                    /// <summary>
                    /// Only if the certificate is used for issueWhen it comes into effect, if the value is trueEmbedle the CA certificate in the certificate chain when issuing the certificate.
                    /// </summary>
                    [JsonPropertyName("buildChain")]
                    public bool BuildChain { get; set; } = false;

                    /// <summary>
                    /// Certificate file paths, such as those generated using OpenSSL, are suffixed with .crt.
                    /// </summary>
                    [JsonPropertyName("certificateFile")]
                    public string? CertificateFile { get; set; } = null;

                    /// <summary>
                    /// Key file paths, such as those generated using OpenSSL, are suffixed with .key.
                    /// Key files that require a password are not currently supported.
                    /// </summary>
                    [JsonPropertyName("keyFile")]
                    public string? KeyFile { get; set; } = null;

                    /// <summary>
                    /// An array of strings, representing the contents of the certificate.
                    /// certificate and certificateFile Choose one of the two.
                    /// </summary>
                    [JsonPropertyName("certificate")]
                    public List<string> Certificates { get; set; } = new();

                    /// <summary>
                    /// An array of strings, representing the contents of the key.
                    /// keyand keyFile Choose one of the two.
                    /// </summary>
                    [JsonPropertyName("key")]
                    public List<string> Key { get; set; } = new();

                    public class Get
                    {
                        public class Usage
                        {
                            /// <summary>
                            /// "encipherment" Certificates are used for TLS authentication and encryption.
                            /// </summary>
                            public static readonly string Encipherment = "Encipherment";
                            /// <summary>
                            /// "verify" The certificate is used to verify the certificate of remote TLS. When this is used, the current certificate must be a CA certificate.
                            /// </summary>
                            public static readonly string Verify = "Verify";
                            /// <summary>
                            /// "issue" The certificate is used to issue other certificates. When this is used, the current certificate must be a CA certificate.
                            /// </summary>
                            public static readonly string Issue = "issue";
                        }
                    }
                }
            }

            public class StreamRealitySettings
            {
                /// <summary>
                /// When the value is true show output debugging information.
                /// </summary>
                [JsonPropertyName("show")]
                public bool Show { get; set; } = false;

                /// <summary>
                /// Mandatory, formatd with Vless fallbacks.
                /// </summary>
                [JsonPropertyName("target")]
                public string? Target { get; set; } = null;

                /// <summary>
                /// Select, format with Vless fallbacks.
                /// </summary>
                [JsonPropertyName("xver")]
                public int Xver { get; set; } = 0;

                /// <summary>
                /// Required, client available serverName The * wildcard is not supported for the list.
                /// </summary>
                [JsonPropertyName("serverNames")]
                public List<string> ServerNames { get; set; } = new();

                /// <summary>
                /// Required, Executed ./xray x25519Generated.
                /// </summary>
                [JsonPropertyName("privateKey")]
                public string? PrivateKey { get; set; } = null;

                /// <summary>
                /// Select, client Xray minimum version, format is x.y.z.
                /// </summary>
                [JsonPropertyName("minClientVer")]
                public string? MinClientVer { get; set; } = null;

                /// <summary>
                /// Select, client Xray maximum version, format is x.y.z.
                /// </summary>
                [JsonPropertyName("maxClientVer")]
                public string? MaxClientVer { get; set; } = null;

                /// <summary>
                /// Select, the maximum allowable time difference, in milliseconds.
                /// </summary>
                [JsonPropertyName("maxTimeDiff")]
                public int MaxTimeDiff { get; set; } = 0;

                /// <summary>
                /// Required, client available shortIdLists can be used to distinguish between different clients.
                /// </summary>
                [JsonPropertyName("shortIds")]
                public List<string> ShortIds { get; set; } = new();

                /// <summary>
                /// Composing with TlsSettings.
                /// </summary>
                [JsonPropertyName("fingerprint")]
                public string Fingerprint { get; set; } = StreamTlsSettings.Get.Fingerprint.Firefox;

                /// <summary>
                /// One of the server names.
                /// </summary>
                [JsonPropertyName("serverName")]
                public string? ServerName { get; set; } = null;

                /// <summary>
                /// Whether an insecure connection is allowed (only for the client). The default value is false.
                /// </summary>
                [JsonPropertyName("allowInsecure")]
                public bool AllowInsecure { get; set; } = false;

                /// <summary>
                /// Deprecated In Xray.
                /// An array of strings that specifies the ALPN value specified when the TLS handshake is held.
                /// The default value is ["h2", "http/1.1"]
                /// </summary>
                [JsonPropertyName("alpn")]
                public List<string> Alpn { get; set; } = new();

                /// <summary>
                /// The public key corresponding to the private key of the server is required.
                /// </summary>
                [JsonPropertyName("publicKey")]
                public string? PublicKey { get; set; } = null;

                /// <summary>
                /// One of the server shortIds.
                /// </summary>
                [JsonPropertyName("shortId")]
                public string? ShortId { get; set; } = null;

                /// <summary>
                /// The initial path and parameters of the crawler are recommended for each client differently.
                /// </summary>
                [JsonPropertyName("spiderX")]
                public string? SpiderX { get; set; } = null;

                [JsonPropertyName("mldsa65Verify")]
                public string? Mldsa65Verify { get; set; } = null;
            }

            public class StreamTcpSettings
            {
                ///// <summary>
                ///// Used only for inbound, indicating whether to receive PROXY protocol.
                ///// </summary>
                //[JsonPropertyName("acceptProxyProtocol")]
                //public bool AcceptProxyProtocol { get; set; } = false;

                /// <summary>
                /// Packet header camouflage settings, default value NoneHeaderObject. 
                /// </summary>
                [JsonPropertyName("header")]
                public TcpHeader Header { get; set; } = new();

                public class TcpHeader
                {
                    /// <summary>
                    /// Camouflage type.
                    /// </summary>
                    [JsonPropertyName("type")]
                    public string Type { get; set; } = Get.Type.None;

                    /// <summary>
                    /// HTTP Request.
                    /// </summary>
                    [JsonPropertyName("request")]
                    public HeaderRequest? Request { get; set; } = null;

                    /// <summary>
                    /// HTTP Response.
                    /// </summary>
                    [JsonPropertyName("response")]
                    public HeaderResponse? Response { get; set; } = null;

                    public class Get
                    {
                        public class Type
                        {
                            /// <summary>
                            /// Designation not to camouflage.
                            /// </summary>
                            public static readonly string None = "none";
                            /// <summary>
                            /// Specify HTTP camouflage.
                            /// </summary>
                            public static readonly string Http = "http";
                        }
                    }

                    public class HeaderRequest
                    {
                        /// <summary>
                        /// HTTP version, the default value is "1.1".
                        /// </summary>
                        [JsonPropertyName("version")]
                        public string Version { get; set; } = "1.1";

                        /// <summary>
                        /// HTTP method, the default value is "GET".
                        /// </summary>
                        [JsonPropertyName("method")]
                        public string Method { get; set; } = "GET";

                        /// <summary>
                        /// An array of strings. The default value is ["/"]. When there are multiple values, a value is chosen at random each request.
                        /// </summary>
                        [JsonPropertyName("path")]
                        public List<string> Path { get; set; } = new();

                        /// <summary>
                        /// The HTTP header, a key-value pair, each key represents the name of an HTTP header, and the corresponding value is an array.
                        /// </summary>
                        [JsonPropertyName("headers")]
                        public RequestHeaders Headers { get; set; } = new();

                        public class RequestHeaders
                        {
                            /// <summary>
                            /// A List of Hosts
                            /// </summary>
                            [JsonPropertyName("Host")]
                            public List<string> Host { get; set; } = new();

                            /// <summary>
                            /// A List of User-Agents
                            /// </summary>
                            [JsonPropertyName("User-Agent")]
                            public List<string> UserAgent { get; set; } = new();

                            /// <summary>
                            /// Default: ["gzip, deflate"]
                            /// </summary>
                            [JsonPropertyName("Accept-Encoding")]
                            public List<string> AcceptEncoding { get; set; } = new();

                            /// <summary>
                            /// Default: ["keep-alive"]
                            /// </summary>
                            [JsonPropertyName("Connection")]
                            public List<string> Connection { get; set; } = new();

                            /// <summary>
                            /// Default: "no-cache"
                            /// </summary>
                            [JsonPropertyName("Pragma")]
                            public string Pragma { get; set; } = "no-cache";
                        }
                    }

                    public class HeaderResponse
                    {
                        /// <summary>
                        /// HTTP version, the default value is "1.1".
                        /// </summary>
                        [JsonPropertyName("version")]
                        public string Version { get; set; } = "1.1";

                        /// <summary>
                        /// HTTP status, default value is "200".
                        /// </summary>
                        [JsonPropertyName("status")]
                        public string Status { get; set; } = "200";

                        /// <summary>
                        /// HTTP status, default value is "OK".
                        /// </summary>
                        [JsonPropertyName("reason")]
                        public string Reason { get; set; } = "OK";

                        /// <summary>
                        /// The HTTP header, a key-value pair, each key represents the name of an HTTP header, and the corresponding value is an array.
                        /// </summary>
                        [JsonPropertyName("headers")]
                        public ResponseHeaders Headers { get; set; } = new();

                        public class ResponseHeaders
                        {
                            /// <summary>
                            /// Default: ["application/octet-stream", "video/mpeg"]
                            /// </summary>
                            [JsonPropertyName("Content-Type")]
                            public List<string> ContentType { get; set; } = new();

                            /// <summary>
                            /// Default: ["chunked"]
                            /// </summary>
                            [JsonPropertyName("Transfer-Encoding")]
                            public List<string> TransferEncoding { get; set; } = new();

                            /// <summary>
                            /// Default: ["keep-alive"]
                            /// </summary>
                            [JsonPropertyName("Connection")]
                            public List<string> Connection { get; set; } = new();

                            /// <summary>
                            /// Default: "no-cache"
                            /// </summary>
                            [JsonPropertyName("Pragma")]
                            public string Pragma { get; set; } = "no-cache";
                        }
                    }
                }
            }

            public class StreamHttpSettings
            {
                /// <summary>
                /// A List of Hosts
                /// </summary>
                [JsonPropertyName("Host")]
                public List<string> Host { get; set; } = new();

                [JsonPropertyName("path")]
                public string? Path { get; set; } = null;
            }

            // Doc 1: https://xtls.github.io/config/transport.html#streamsettingsobject
            // Doc 2: https://github.com/XTLS/Xray-core/discussions/4113
            // Although XHTTP has many parameters, they are all set to default values. If you just want to use XHTTP, you only need to following steps:
            // 1. Whether it is TLS or REALITY, generally speaking, XHTTP configuration only needs to fill in path, leave the rest blank.
            // 2. If the server supports QUIC H3, the client alpn Select "h3" to use QUIC.
            // 3. If CDN prefers IP, the client address Fill in IP address, serverName (SNI) Just fill in the domain name.
            public class StreamXHttpSettings
            {
                [JsonPropertyName("host")]
                public string? Host { get; set; } = null;

                [JsonPropertyName("path")]
                public string? Path { get; set; } = null;

                [JsonPropertyName("mode")]
                public string Mode { get; set; } = Get.Mode.Auto;

                [JsonPropertyName("extra")]
                public JsonDocument? Extra { get; set; } = null;

                public class Get
                {
                    public class Mode
                    {
                        public static readonly string Auto = "auto";
                        public static readonly string PacketUp = "packet-up";
                        public static readonly string StreamUp = "stream-up";
                        public static readonly string StreamOne = "stream-one";
                    }
                }
            }

            public class StreamQuicSettings
            {
                [JsonPropertyName("security")]
                public string? Security { get; set; } = Get.Security.None;

                [JsonPropertyName("key")]
                public string? Key { get; set; } = null;

                [JsonPropertyName("header")]
                public QuicHeader Header { get; set; } = new();

                public class Get
                {
                    public class Security
                    {
                        public static readonly string None = "none";
                        public static readonly string Aes128Gcm = "aes-128-gcm";
                        public static readonly string Chacha20Poly1305 = "chacha20-poly1305";
                    }
                }

                public class QuicHeader
                {
                    /// <summary>
                    /// Camouflage type.
                    /// </summary>
                    [JsonPropertyName("type")]
                    public string Type { get; set; } = Get.Type.None;

                    public class Get
                    {
                        public class Type
                        {
                            /// <summary>
                            /// Designation not to camouflage.
                            /// </summary>
                            public static readonly string None = "none";
                            /// <summary>
                            /// Obfuscated as SRTP traffic. It may be recognized as video calls such as Facetime.
                            /// </summary>
                            public static readonly string Srtp = "srtp";
                            /// <summary>
                            /// Obfuscated as uTP traffic. It may be recognized as Bittorrent traffic.
                            /// </summary>
                            public static readonly string Utp = "utp";
                            /// <summary>
                            /// Obfuscated to WeChat traffic.
                            /// </summary>
                            public static readonly string WechatVideo = "wechat-video";
                            /// <summary>
                            /// Obfuscated as DTLS 1.2 packets.
                            /// </summary>
                            public static readonly string DTLS = "dtls";
                            /// <summary>
                            /// Obfuscated as WireGuard packets. (NOT true WireGuard protocol)
                            /// </summary>
                            public static readonly string Wireguard = "wireguard";
                        }
                    }
                }
            }

            public class StreamKcpSettings
            {
                /// <summary>
                /// Maximum transmission unit Select a value between 576 and 1460.
                /// The default value is 1350.
                /// </summary>
                [JsonPropertyName("mtu")]
                public int Mtu { get; set; } = 1350;

                /// <summary>
                /// The transmission time interval, per millisecond (ms), mKCP will send data at this time frequency. Please select a value between 10 and 100.
                /// The default value is 50.
                /// </summary>
                [JsonPropertyName("tti")]
                public int Tti { get; set; } = 50;

                /// <summary>
                /// Uplink capacity, that is, the maximum bandwidth used by the host to send data, per unit MB/s, Note that Byte is not bit.
                /// It can be set to 0, indicating a very small bandwidth.
                /// Default value 5.
                /// </summary>
                [JsonPropertyName("uplinkCapacity")]
                public int UplinkCapacity { get; set; } = 5;

                /// <summary>
                /// Downlink capacity, the maximum bandwidth used by the host to receive data, per unit MB/s, is Note to Byte rather than bit.
                /// It can be set to 0, indicating a very small bandwidth.
                /// Default value 20.
                /// </summary>
                [JsonPropertyName("downlinkCapacity")]
                public int DownlinkCapacity { get; set; } = 20;

                /// <summary>
                /// Whether to enable congestion control.
                /// The default value is false.
                /// </summary>
                [JsonPropertyName("congestion")]
                public bool Congestion { get; set; } = false;

                /// <summary>
                /// The read buffer size for a single connection is in MB.
                /// The default value is 2.
                /// </summary>
                [JsonPropertyName("readBufferSize")]
                public int ReadBufferSize { get; set; } = 2;

                /// <summary>
                /// The write buffer size of a single connection is in MB.
                /// The default value is 2.
                /// </summary>
                [JsonPropertyName("writeBufferSize")]
                public int WriteBufferSize { get; set; } = 2;

                /// <summary>
                /// Packet header camouflage settings.
                /// </summary>
                [JsonPropertyName("header")]
                public KcpHeader Header { get; set; } = new();

                /// <summary>
                /// Optional obfuscation of passwords, using the AES-128-GCM algorithm to confuse traffic data, and clients and clients need to be consistent.
                /// </summary>
                [JsonPropertyName("seed")]
                public string? Seed { get; set; } = null;

                public class KcpHeader
                {
                    /// <summary>
                    /// The camouflage type.
                    /// </summary>
                    [JsonPropertyName("type")]
                    public string Type { get; set; } = Get.Type.None;

                    /// <summary>
                    /// Type of match "dns" You can use it with a domain name.
                    /// </summary>
                    [JsonPropertyName("domain")]
                    public string? Domain { get; set; } = null;

                    public class Get
                    {
                        public class Type
                        {
                            public static readonly string None = "none";
                            public static readonly string Srtp = "srtp";
                            public static readonly string Utp = "utp";
                            public static readonly string Wechat_video = "wechat-video";
                            public static readonly string Dtls = "dtls";
                            public static readonly string Ios = "ios";
                            public static readonly string Wireguard = "wireguard";
                            public static readonly string Dns = "dns";
                            public static readonly string F360 = "360";
                            public static readonly string Qq = "qq";
                        }
                    }
                }
            }

            public class StreamGrpcSettings
            {
                /// <summary>
                /// A string can be used as a host to achieve some other purpose.
                /// </summary>
                [JsonPropertyName("authority")]
                public string? Authority { get; set; } = null;

                /// <summary>
                /// A string that specifies the service name, similar to Path in HTTP/2.
                /// The client communicates with this name, and the server verifies whether the service name matches.
                /// </summary>
                [JsonPropertyName("serviceName")]
                public string? ServiceName { get; set; } = null;

                /// <summary>
                /// true Enabled multiMode
                /// The default value is: false.
                /// </summary>
                [JsonPropertyName("multiMode")]
                public bool MultiMode { get; set; } = false;

                /// <summary>
                /// Custom user-agent
                /// </summary>
                [JsonPropertyName("user_agent")]
                public string? User_agent { get; set; } = null;

                /// <summary>
                /// Units of seconds, when there is no data transmission during this period, a health check will be carried out.
                /// If this value is set to 10The following will be used. 10That is, the minimum value.
                /// </summary>
                [JsonPropertyName("idle_timeout")]
                public int Idle_timeout { get; set; } = 60;

                /// <summary>
                /// Units of seconds, timeout for health checks. If the health check is not completed during this period and there is still no data transmission, the health check is considered to have failed.
                /// The default value is 20.
                /// </summary>
                [JsonPropertyName("health_check_timeout")]
                public int Health_check_timeout { get; set; } = 20;

                /// <summary>
                /// true Allows health checks when there is no subconnection.
                /// The default value is false.
                /// </summary>
                [JsonPropertyName("permit_without_stream")]
                public bool Permit_without_stream { get; set; } = false;

                /// <summary>
                /// h2 Stream Initial window size. When the value is less than or equal to 0 This function does not take effect.
                /// When the value is greater than 65535 The Dynamic Window mechanism is disabled.
                /// The default value is 0 That is, it does not enter into force.
                /// </summary>
                [JsonPropertyName("initial_windows_size")]
                public int Initial_windows_size { get; set; } = 0;
            }

            public class StreamWsSettings
            {
                /// <summary>
                /// Used only for inbound, indicating whether to receive PROXY protocol.
                /// </summary>
                [JsonPropertyName("acceptProxyProtocol")]
                public bool AcceptProxyProtocol { get; set; } = false;

                /// <summary>
                /// The HTTP protocol path used by WebSocket.
                /// The default value is "/".
                /// </summary>
                [JsonPropertyName("path")]
                public string Path { get; set; } = "/";

                /// <summary>
                /// The host sent in the HTTP request of WebSocket is empty by default.
                /// If the server value is empty, the host value sent by the client is not verified.
                /// </summary>
                [JsonPropertyName("host")]
                public string? Host { get; set; } = null;

                /// <summary>
                /// Only the client, the custom HTTP header, a key-value pair, each key represents the name of an HTTP header, and the corresponding value is the string.
                /// The default value is empty.
                /// Deprecated In Xray.
                /// </summary>
                [JsonPropertyName("headers")]
                public Dictionary<string, string> Headers { get; set; } = new();

                /// <summary>
                /// Specify a fixed interval to send a ping message to keep the connection.
                /// Do not send a ping message when not specified or specified as 0, as the current default behavior.
                /// </summary>
                [JsonPropertyName("heartbeatPeriod")]
                public int HeartbeatPeriod { get; set; } = 0;
            }

            public class StreamHttpUpgradeSettings
            {
                /// <summary>
                /// Used only for inbound, indicating whether to receive PROXY protocol.
                /// </summary>
                [JsonPropertyName("acceptProxyProtocol")]
                public bool AcceptProxyProtocol { get; set; } = false;

                /// <summary>
                /// The HTTP protocol path used by HTTP.
                /// The default value is "/".
                /// </summary>
                [JsonPropertyName("path")]
                public string Path { get; set; } = "/";

                /// <summary>
                /// The host sent in the HTTP request of HTTPUpgrade is empty by default.
                /// If the server value is empty, the host value sent by the client is not verified.
                /// </summary>
                [JsonPropertyName("host")]
                public string? Host { get; set; } = null;

                /// <summary>
                /// Only the client, the custom HTTP header, a key-value pair, each key represents the name of an HTTP header, and the corresponding value is the string.
                /// The default value is empty.
                /// </summary>
                [JsonPropertyName("headers")]
                public Dictionary<string, string> Headers { get; set; } = new();
            }

            public class StreamSockopt
            {
                /// <summary>
                /// A whole number. When its value is not zero, SO_MARK is marked with this value on the outbound connection.
                /// It is only available on Linux systems.
                /// </summary>
                [JsonPropertyName("mark")]
                public int Mark { get; set; } = 0;

                /// <summary>
                /// The maximum transport unit for setting up TCP packets.
                /// </summary>
                [JsonPropertyName("tcpMaxSeg")]
                public int TcpMaxSeg { get; set; } = 1440;

                /// <summary>
                /// Enable TCP Fast Open.
                /// </summary>
                [JsonPropertyName("tcpFastOpen")]
                public bool TcpFastOpen { get; set; } = false;

                /// <summary>
                /// Open Transparent Agents (Linux only).
                /// </summary>
                [JsonPropertyName("tproxy")]
                public string TProxy { get; set; } = Get.TProxy.Off;

                /// <summary>
                /// In previous versions, when Xray attempted to establish a system connection using a domain name,
                /// the DNS resolution was done by the system and not controlled by Xray.
                /// This has resulted in the inability to resolve domain names in non-standard Linux environments.
                /// To this end, Xray 1.3.1 addresses this issue by introducing domainStrategy in Freedom for Sockopt.
                /// Default value "AsIs".
                /// </summary>
                [JsonPropertyName("domainStrategy")]
                public string DomainStrategy { get; set; } = Get.DomainStrategy.AsIs;

                [JsonPropertyName("happyEyeballs")]
                public HappyEyeball HappyEyeballs { get; set; } = new();

                /// <summary>
                /// The tag of an outbound agent. When the value is not empty, the connection is sent using the specified outbound.
                /// This option can be used to support chain forwarding of the underlying transport mode.
                /// </summary>
                [JsonPropertyName("dialerProxy")]
                public string? DialerProxy { get; set; } = null;

                /// <summary>
                /// Used only for inbound, indicating whether to receive PROXY protocol.
                /// </summary>
                [JsonPropertyName("acceptProxyProtocol")]
                public bool AcceptProxyProtocol { get; set; } = false;

                /// <summary>
                /// TCP maintains an active packet delivery interval in seconds.
                /// Default Is 0.
                /// </summary>
                [JsonPropertyName("tcpKeepAliveInterval")]
                public int TcpKeepAliveInterval { get; set; } = 0;

                /// <summary>
                /// TCP idle time thresholds in seconds. When the TCP connection idles to reach this threshold, the Keep-Alive probe packet will begin.
                /// Default Is 0.
                /// </summary>
                [JsonPropertyName("tcpKeepAliveIdle")]
                public int TcpKeepAliveIdle { get; set; } = 300;

                /// <summary>
                /// Units in milliseconds. Details: https://github.com/grpc/proposal/blob/master/A18-tcp-user-timeout.md
                /// </summary>
                [JsonPropertyName("tcpUserTimeout")]
                public int TcpUserTimeout { get; set; } = 10000;

                /// <summary>
                /// TCP congestion control algorithm. It only supports Linux. This does not configure this to use the system default.
                /// bbr recommended.
                /// </summary>
                [JsonPropertyName("tcpCongestion")]
                public string TcpCongestion { get; set; } = Get.TcpCongestion.Bbr;

                /// <summary>
                /// Specify the binding export network card name and support linux/iOS/MacOS/Windows.
                /// </summary>
                [JsonPropertyName("interface")]
                public string? Interface { get; set; } = null;

                /// <summary>
                /// Filled true In other words, listen. :: The address only accepts IPv6 connections.
                /// It only supports Linux.
                /// </summary>
                [JsonPropertyName("v6only")]
                public bool V6only { get; set; } = false;

                /// <summary>
                /// The size of the window for the binding announcement is this value. The kernel selects a maximum value between it and SOCK_MIN_RCVBUF/2.
                /// </summary>
                [JsonPropertyName("tcpWindowClamp")]
                public int TcpWindowClamp { get; set; } = 600;

                /// <summary>
                /// Default value false, fill in. true Enable Multipath TCP, client-side parameter only, because golang in version 1.24+ has enabled MPTCP by default when listening.
                /// Currently only Linux is supported, and Linux Kernel 5.6 and above is required.
                /// </summary>
                [JsonPropertyName("tcpMptcp")]
                public bool TcpMptcp { get; set; } = false;

                /// <summary>
                /// This option has been removed because Golann has TCP no delay enabled by default.
                /// Conversely, if you want to disable it, disable it by using customSockopt.
                /// </summary>
                [JsonPropertyName("tcpNoDelay")]
                public bool TcpNoDelay { get; set; } = true;

                /// <summary>
                /// Specifies the destination address/port used by the outgoing using SRV record or TXT record, default none That is, closed.
                /// </summary>
                [JsonPropertyName("addressPortStrategy")]
                public string AddressPortStrategy { get; set; } = Get.AddressPortStrategy.None;

                /// <summary>
                /// An array for any sockopts specified by the advanced user, in theory, all of the above-mentioned connection-related settings can be set in this equivalence,
                /// and naturally other options that exist but are not added to the core.
                /// It currently supports the Linux Winows Darwin operating system.
                /// </summary>
                [JsonPropertyName("customSockopt")]
                public List<CustomSockopt> CustomSockopts { get; set; } = new();

                public class Get
                {
                    public class TProxy
                    {
                        public static readonly string Redirect = "redirect";
                        public static readonly string Tproxy = "tproxy";
                        public static readonly string Off = "off";
                    }

                    public class DomainStrategy
                    {
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

                    public class TcpCongestion
                    {
                        public static readonly string Bbr = "bbr";
                        public static readonly string Cubic = "cubic";
                        public static readonly string Reno = "reno";
                    }

                    public class AddressPortStrategy
                    {
                        public static readonly string None = "none";
                        public static readonly string SrvPortOnly = "SrvPortOnly";
                        public static readonly string SrvAddressOnly = "SrvAddressOnly";
                        public static readonly string SrvPortAndAddress = "SrvPortAndAddress";
                        public static readonly string TxtPortOnly = "TxtPortOnly";
                        public static readonly string TxtAddressOnly = "TxtAddressOnly";
                        public static readonly string TxtPortAndAddress = "TxtPortAndAddress";
                    }
                }

                public class HappyEyeball
                {
                    /// <summary>
                    /// Delay time between each attempt in millisecond, RFC-8305 recommend 250, default is 0.
                    /// (if it is 0, happy-eyeballs is disabled)
                    /// </summary>
                    [JsonPropertyName("tryDelayMs")]
                    public int TryDelayMs { get; set; } = 250;

                    /// <summary>
                    /// Indicate "First Address Family" in RFC-8305, default is false (= prioritizeIPv4)
                    /// </summary>
                    [JsonPropertyName("prioritizeIPv6")]
                    public bool PrioritizeIPv6 { get; set; } = false;

                    /// <summary>
                    /// Indicate "First Address Family count" in RFC-8305, default is 1.
                    /// </summary>
                    [JsonPropertyName("interleave")]
                    public int Interleave { get; set; } = 2;

                    /// <summary>
                    /// Maximum concurrent attempt.
                    /// Default is 4. if it is 0, happy-eyeballs is disabled.
                    /// </summary>
                    [JsonPropertyName("maxConcurrentTry")]
                    public int MaxConcurrentTry { get; set; } = 4;
                }

                public class CustomSockopt
                {
                    /// <summary>
                    /// Optionally, specify the system that works, and skip the sockopt if the running system does not match.
                    /// Currently Optional linux windows darwin (All lowercase). Execute directly if left empty
                    /// </summary>
                    [JsonPropertyName("system")]
                    public string System { get; set; } = Get.System.Windows;

                    /// <summary>
                    /// Mandatory, set of type, currently optional int or str.
                    /// </summary>
                    [JsonPropertyName("type")]
                    public string Type { get; set; } = Get.Type.Str;

                    /// <summary>
                    /// Optional, protocol level, used to specify the effective range, by default to 6, i.e. TCP.
                    /// </summary>
                    [JsonPropertyName("level")]
                    public string Level { get; set; } = "6";

                    /// <summary>
                    /// The option name of the operation, using decimal (in this case, the value of TCP_CONGESTION is defined as 0xd to be converted to decimal, i.e. 13).
                    /// </summary>
                    [JsonPropertyName("opt")]
                    public string Opt { get; set; } = "13";

                    /// <summary>
                    /// To set the option value, the example here is set to bbr.
                    /// When type is specified as int, decimal numbers are required.
                    /// </summary>
                    [JsonPropertyName("value")]
                    public string Value { get; set; } = "bbr";

                    public class Get
                    {
                        public class System
                        {
                            public static readonly string Windows = "windows";
                            public static readonly string Linux = "linux";
                            public static readonly string Darwin = "darwin";
                        }

                        public class Type
                        {
                            public static readonly string Str = "str";
                            public static readonly string Int = "int";
                        }
                    }
                }
            }
        }

        public class OutboundProxySettings
        {
            /// <summary>
            /// When another outbound identifier is specified, the data emitted by this outbound is forwarded to the specified outbound.
            /// </summary>
            [JsonPropertyName("tag")]
            public string? Tag { get; set; } = null;
        }

        public class OutboundMux
        {
            /// <summary>
            /// Whether to enable Mux forwarding requests, default false.
            /// </summary>
            [JsonPropertyName("enabled")]
            public bool Enabled { get; set; } = false;

            /// <summary>
            /// Maximum number of concurrent connections. Minimum value 1 The maximum value 128 omit or fill in. 0 All equals. 8 Greater than 128 The value will be considered as 128.
            /// </summary>
            [JsonPropertyName("concurrency")]
            public int Concurrency { get; set; } = 8;

            /// <summary>
            /// Use the new XUDP Aggregation Tunnel (another Mux connection) to proxy UDP traffic and fill in the maximum number of concurrency UoTs.
            /// Minimum value 1 The maximum value 1024.
            /// 0 When it comes to TCP traffic, it will take the same path as traditional behavior.
            /// </summary>
            [JsonPropertyName("xudpConcurrency")]
            public int XudpConcurrency { get; set; } = 16;

            /// <summary>
            /// Control how Mux handles the UDP/443 (QUIC)
            /// </summary>
            [JsonPropertyName("xudpProxyUDP443")]
            public string XudpProxyUDP443 { get; set; } = Get.XudpProxyUDP443.Reject;

            public class Get
            {
                public class XudpProxyUDP443
                {
                    /// <summary>
                    /// Default reject Rejects traffic (generally, the browser automatically drops back to TCP HTTP2)
                    /// </summary>
                    public static readonly string Reject = "reject";
                    /// <summary>
                    /// Allows the Mux connection.
                    /// </summary>
                    public static readonly string Allow = "allow";
                    /// <summary>
                    /// When you do not use a Mux module to carry UDP 443 traffic. The original UDP transmission method of the proxy protocol will be used.
                    /// </summary>
                    public static readonly string Skip = "skip";
                }
            }
        }
    }

    // Routing
    public class ConfigRouting
    {
        /// <summary>
        /// Used to notify the server to specify the IP location when querying DNS. It cannot be a private address.
        /// </summary>
        [JsonPropertyName("domainStrategy")]
        public string DomainStrategy { get; set; } = Get.DomainStrategy.AsIs;

        /// <summary>
        /// Domain name matching algorithm, different algorithms are used according to different settings. This option will affect all RuleObject.
        /// </summary>
        [JsonPropertyName("domainMatcher")]
        public string DomainMatcher { get; set; } = Get.DomainMatcher.Hybrid;

        /// <summary>
        /// Corresponding to an array, each item in the array is a rule.
        /// For each connection, the router will judge according to these rules from top to bottom.
        /// When it encounters the first effective rule, it will forward the connection to the specified outboundTagor balancerTag.
        /// </summary>
        [JsonPropertyName("rules")]
        public List<Rule> Rules { get; set; } = new();

        /// <summary>
        /// An array, each item in the array is a load balancer configuration.
        /// When a rule points to a load balancer, Xray selects an outbound through this load balancer and forwards the traffic through it.
        /// </summary>
        [JsonPropertyName("balancers")]
        public List<Balancer> Balancers { get; set; } = new();

        public class Get
        {
            public readonly struct DomainStrategy
            {
                /// <summary>
                /// Use only the domain name for routing. The default value.
                /// </summary>
                public static readonly string AsIs = "AsIs";
                /// <summary>
                /// When the domain name does not match any rule, resolve the domain name into IP (A record or AAAA record) and match again.
                /// </summary>
                public static readonly string IPIfNonMatch = "IPIfNonMatch";
                /// <summary>
                /// When any IP-based rule is encountered during matching, the domain name is immediately resolved to IP for matching.
                /// </summary>
                public static readonly string IPOnDemand = "IPOnDemand";
            }

            public readonly struct DomainMatcher
            {
                /// <summary>
                /// Use a new domain name matching algorithm that is faster and less intensive. The default value.
                /// </summary>
                public static readonly string Hybrid = "hybrid";
                /// <summary>
                /// Use the original domain name matching algorithm.
                /// </summary>
                public static readonly string Linear = "linear";
            }
        }

        public class Rule
        {
            /// <summary>
            /// Domain name matching algorithm, different algorithms are used according to different settings. This option will affect all RuleObject.
            /// </summary>
            [JsonPropertyName("domainMatcher")]
            public string DomainMatcher { get; set; } = ConfigRouting.Get.DomainMatcher.Hybrid;

            /// <summary>
            /// Currently only supports "field" This one option.
            /// </summary>
            [JsonPropertyName("type")]
            public string Type { get; set; } = "field";

            /// <summary>
            /// An array, each item in the array is a match of a domain name.
            /// </summary>
            [JsonPropertyName("domain")]
            public List<string> Domain { get; set; } = new();

            /// <summary>
            /// When an item matches the target IP, this rule takes effect. e.g. "127.0.0.1", "10.0.0.0/8", "fc00::/7", "geoip:cn"
            /// </summary>
            [JsonPropertyName("ip")]
            public List<string> IP { get; set; } = new();

            /// <summary>
            /// Target port range separated by commas. e.g. "53,443,1000-2000"
            /// </summary>
            [JsonPropertyName("port")]
            public string? Port { get; set; } = null;

            /// <summary>
            /// Source port range separated by commas. e.g. "53,443,1000-2000"
            /// </summary>
            [JsonPropertyName("sourcePort")]
            public string? SourcePort { get; set; } = null;

            /// <summary>
            /// This rule takes effect when the connection method is the specified one.
            /// </summary>
            [JsonPropertyName("network")]
            public string Network { get; set; } = Get.Network.TcpUdp;

            /// <summary>
            /// An array, each item in the array represents an IP range, in the form of IP, CIDR, GeoIP and IP loaded from a file. When an item matches the source IP, this rule takes effect.
            /// </summary>
            [JsonPropertyName("source")]
            public List<string> Source { get; set; } = new();

            /// <summary>
            /// An array, each item in the array is an email address. This rule takes effect when an item matches the source user.
            /// </summary>
            [JsonPropertyName("user")]
            public List<string> User { get; set; } = new();

            /// <summary>
            /// An array, each item in the array is an identifier. When an item matches the identifier of the inbound protocol, this rule takes effect.
            /// </summary>
            [JsonPropertyName("inboundTag")]
            public List<string> InboundTag { get; set; } = new();

            /// <summary>
            /// An array, each item in the array represents a protocol.
            /// When a protocol matches the protocol type of the current connection, this rule takes effect.
            /// </summary>
            [JsonPropertyName("protocol")]
            public List<string> Protocol { get; set; } = new();

            /// <summary>
            /// keys and values, used to detect attribute values ​​in HTTP traffic (for obvious reasons, only 1.0 and 1.1 are supported).
            /// This rule is hit when HTTP headers contain all specified keys and the value contains the specified substring.
            /// Keys are case insensitive. Values ​​support regular expressions.
            /// Detecting HTTP GET: {":method": "GET"}
            /// Detect HTTP Path: {":path": "/test"}
            /// Detecting Content Type: {"accept": "text/html"}
            /// </summary>
            [JsonPropertyName("attrs")]
            public Dictionary<string, string> Attrs { get; set; } = new();

            /// <summary>
            /// Corresponds to an outbound identifier.
            /// </summary>
            [JsonPropertyName("outboundTag")]
            public string? OutboundTag { get; set; } = null;

            /// <summary>
            /// Corresponds to the identifier of a Balancer.
            /// </summary>
            [JsonPropertyName("balancerTag")]
            public string? BalancerTag { get; set; } = null;

            /// <summary>
            /// Optional, no actual effect, only used to identify the name of this rule.
            /// </summary>
            [JsonPropertyName("ruleTag")]
            public string? RuleTag { get; set; } = null;

            public class Get
            {
                public readonly struct Network
                {
                    public static readonly string Tcp = "tcp";
                    public static readonly string Udp = "udp";
                    public static readonly string TcpUdp = "tcp,udp";
                }

                public readonly struct Protocol
                {
                    public static readonly string Http = "http";
                    public static readonly string Tls = "tls";
                    public static readonly string Quic = "quic";
                    public static readonly string Bittorrent = "bittorrent";
                }
            }
        }

        public class Balancer
        {
            /// <summary>
            /// The ID of this load balancer, used for matching RuleObject In balancerTag.
            /// </summary>
            [JsonPropertyName("tag")]
            public string? Tag { get; set; } = null;

            /// <summary>
            /// An array of strings, each of which will be used to match the prefix of the outbound identifier.
            /// In the following outbound identifiers: [ "a", "ab", "c", "ba" ]， "selector": ["a"] Will match to [ "a", "ab" ]
            /// </summary>
            [JsonPropertyName("selector")]
            public List<string> Selector { get; set; } = new();

            /// <summary>
            /// If all outbound connections cannot be connected according to the connection observation results, the outbound connection specified by this configuration item will be used.
            /// </summary>
            [JsonPropertyName("fallbackTag")]
            public string? FallbackTag { get; set; } = null;

            [JsonPropertyName("strategy")]
            public BalancerStrategy Strategy { get; set; } = new();

            public class BalancerStrategy
            {
                /// <summary>
                /// type : "random" | "roundRobin" | "leastPing" | "leastLoad"
                /// </summary>
                [JsonPropertyName("type")]
                public string Type { get; set; } = Get.Type.Random;

                /// <summary>
                /// This is an optional configuration item. The configuration format of different load balancing strategies is different.
                /// leastLoad This configuration item can be added to the load balancing strategy.
                /// </summary>
                [JsonPropertyName("type")]
                public StrategySettings Settings { get; set; } = new();

                public class Get
                {
                    public readonly struct Type
                    {
                        public static readonly string Random = "random";
                        public static readonly string RoundRobin = "roundRobin";
                        public static readonly string LeastPing = "leastPing";
                        public static readonly string LeastLoad = "leastLoad";
                    }
                }

                public class StrategySettings
                {
                    /// <summary>
                    /// The load balancer selects the optimal number of nodes, and the traffic will be randomly distributed among these nodes.
                    /// </summary>
                    [JsonPropertyName("expected")]
                    public int Expected { get; set; } = 0;

                    /// <summary>
                    /// The maximum acceptable RTT duration for speed measurement.
                    /// </summary>
                    [JsonPropertyName("maxRTT")]
                    public string MaxRTT { get; set; } = "1s";

                    /// <summary>
                    /// The maximum acceptable rate of speed test failures.
                    /// For example 0.01 means that one percent of speed test failures are acceptable. (seems not implemented)
                    /// </summary>
                    [JsonPropertyName("tolerance")]
                    public float Tolerance { get; set; } = 0.01f;

                    /// <summary>
                    /// The maximum acceptable RTT standard deviation of the speed measurement.
                    /// </summary>
                    [JsonPropertyName("baselines")]
                    public List<string> Baselines { get; set; } = new();

                    [JsonPropertyName("costs")]
                    public List<Cost> Costs { get; set; } = new();

                    public class Cost
                    {
                        /// <summary>
                        /// Whether to use regular expressions to select outbound Tag.
                        /// </summary>
                        [JsonPropertyName("regexp")]
                        public bool Regexp { get; set; } = false;

                        /// <summary>
                        /// Match Outbound Tag.
                        /// </summary>
                        [JsonPropertyName("match")]
                        public string? Match { get; set; } = null;

                        /// <summary>
                        /// The weight value, the larger the value, the less likely the corresponding node is to be selected.
                        /// </summary>
                        [JsonPropertyName("value")]
                        public float Value { get; set; } = 0.5f;
                    }

                    public StrategySettings()
                    {
                        Baselines = new()
                        {
                            "1s"
                        };
                    }
                }
            }
        }
    }

    // Transport
    // Stats
    // Reverse
    // FakeDns
    // Metrics
    // Observatory
    // BurstObservatory

}