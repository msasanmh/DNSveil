using System.Text.Json.Serialization;

namespace MsmhToolsClass.V2RayConfigTool.Outbounds;

public class DnsSettings
{
    /// <summary>
    /// Modify the transport layer protocol for DNS traffic with optional values "tcp" and "udp".
    /// When not specified, keep the transmission mode of the source unchanged.
    /// </summary>
    [JsonPropertyName("network")]
    public string Network { get; set; } = Get.Network.Tcp;

    /// <summary>
    /// Change the DNS server address.
    /// When not specified, keep the address specified in the source unchanged.
    /// </summary>
    [JsonPropertyName("address")]
    public string Address { get; set; } = "8.8.8.8";

    /// <summary>
    /// Modify the DNS server port.
    /// When not specified, keep the port specified in the source unchanged.
    /// </summary>
    [JsonPropertyName("port")]
    public int Port { get; set; } = 53;

    /// <summary>
    /// Control of non-IP queries (non-A and AAAA).
    /// </summary>
    [JsonPropertyName("nonIPQuery")]
    public string NonIPQuery { get; set; } = Get.NonIPQuery.Skip;

    /// <summary>
    /// For an int array that blocks the query type in the array, such as "blockTypes":[65,28] Indicates shielding type 65 (HTTPS) and 28 (AAAA)
    /// Due to "nonIPQuery" Drop all non-A and AAAA queries by default, so you need to set it to skip this option to work further.
    /// </summary>
    [JsonPropertyName("blockTypes")]
    public List<string> BlockTypes { get; set; } = new();

    public class Get
    {
        public readonly struct Network
        {
            public static readonly string Udp = "udp";
            public static readonly string Tcp = "tcp";
        }

        public readonly struct NonIPQuery
        {
            /// <summary>
            /// It is not handled by a built-in DNS server and is forwarded to the target.
            /// </summary>
            public static readonly string Skip = "skip";
            /// <summary>
            /// Abandoned.
            /// </summary>
            public static readonly string Drop = "drop";
        }
    }
}