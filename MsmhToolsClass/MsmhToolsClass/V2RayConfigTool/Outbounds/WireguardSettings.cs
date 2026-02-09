using System.Text.Json.Serialization;

namespace MsmhToolsClass.V2RayConfigTool.Outbounds;

public class WireguardSettings
{
    /// <summary>
    /// User's private key. Required.
    /// </summary>
    [JsonPropertyName("secretKey")]
    public string? SecretKey { get; set; } = null;

    /// <summary>
    /// Optional, default ["10.0.0.1", "fd59:7153:2388:b5fd:0000:0000:0000:0001"]
    /// Wireguard will enable a virtual network adapter, tun, locally. It uses one or more IP addresses and supports IPv6.
    /// </summary>
    [JsonPropertyName("address")]
    public List<string> Address { get; set; } = new() { "10.0.0.1", "fd59:7153:2388:b5fd:0000:0000:0000:0001" };

    /// <summary>
    /// A list of Wireguard servers, where each item represents a server configuration.
    /// </summary>
    [JsonPropertyName("peers")]
    public List<Peer> Peers { get; set; } = new();

    /// <summary>
    /// Determine whether to enable the system virtual network adapter.
    /// If necessary, you should also set this option to disable the system virtual network adapter.
    /// </summary>
    [JsonPropertyName("noKernelTun")]
    public bool NoKernelTun { get; set; } = true;

    /// <summary>
    /// Optional, default 1420
    /// The MTU size of the underlying Wireguard TUN.
    /// </summary>
    [JsonPropertyName("mtu")]
    public int MTU { get; set; } = 1420;

    /// <summary>
    /// Wireguard reserved bytes, fill in as needed. 0,0,0
    /// </summary>
    [JsonPropertyName("reserved")]
    public List<int> Reserved { get; set; } = new();

    /// <summary>
    /// Optional, default runtime.NumCPU()
    /// Wireguard uses the number of threads, which defaults to the number of system cores.
    /// </summary>
    [JsonPropertyName("workers")]
    public int Workers { get; set; } = 2;

    /// <summary>
    /// Unlike most proxy protocols, Wireguard does not allow passing domain names as targets.
    /// Therefore, if the target is a domain name, it needs to be resolved to an IP address before transmission.
    /// </summary>
    [JsonPropertyName("domainStrategy")]
    public string DomainStrategy { get; set; } = Get.DomainStrategy.ForceIP;

    public class Peer
    {
        /// <summary>
        /// Server address (required) Host:Port, IPv4:Port, [IPv6]:Port
        /// </summary>
        [JsonPropertyName("endpoint")]
        public string? Endpoint { get; set; } = null;

        /// <summary>
        /// Additional symmetric encryption key.
        /// </summary>
        [JsonPropertyName("preSharedKey")]
        public string? PreSharedKey { get; set; } = null;

        /// <summary>
        /// Server public key, used for authentication, required.
        /// </summary>
        [JsonPropertyName("publicKey")]
        public string? PublicKey { get; set; } = null;

        /// <summary>
        /// Optional, default 0
        /// Heartbeat interval, in seconds; default is 0 indicating no heartbeat.
        /// </summary>
        [JsonPropertyName("keepAlive")]
        public int KeepAlive { get; set; } = 0;

        /// <summary>
        /// Optional, default ["0.0.0.0/0", "::/0"]
        /// Wireguard only allows traffic from specific source IPs.
        /// </summary>
        [JsonPropertyName("allowedIPs")]
        public List<string> AllowedIPs { get; set; } = new() { "0.0.0.0/0", "::/0" };
    }

    public class Get
    {
        public class DomainStrategy
        {
            public static readonly string ForceIP = "ForceIP";
            public static readonly string ForceIPv6v4 = "ForceIPv6v4";
            public static readonly string ForceIPv6 = "ForceIPv6";
            public static readonly string ForceIPv4v6 = "ForceIPv4v6";
            public static readonly string ForceIPv4 = "ForceIPv4";
        }
    }
}