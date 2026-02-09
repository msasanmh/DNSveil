using System.Text.Json.Serialization;

namespace MsmhToolsClass.V2RayConfigTool.Inbounds;

public class DokoDemoDoorSettingsIn
{
    /// <summary>
    /// Forward the traffic to this address.
    /// It can be an IP address, as it is "1.2.3.4" Or a domain name, as "xray.com". The string type.
    /// </summary>
    [JsonPropertyName("address")]
    public string Address { get; set; } = "8.8.8.8";

    /// <summary>
    /// Forward traffic to the specified port of the destination address.
    /// Required Parameters.
    /// </summary>
    [JsonPropertyName("port")]
    public int Port { get; set; } = 53;

    /// <summary>
    /// The type of network protocol that can be received.
    /// For example, when designated as "tcp" Only TCP traffic is received.
    /// The default value is "tcp".
    /// </summary>
    [JsonPropertyName("network")]
    public string Network { get; set; } = Get.Network.TcpUdp;

    /// <summary>
    /// When the value is true dokodemo-door recognizes the data forwarded by iptables and forwards it to the corresponding destination address.
    /// </summary>
    [JsonPropertyName("followRedirect")]
    public bool FollowRedirect { get; set; } = false;

    /// <summary>
    /// At the user level, the connection uses the local policy corresponding to this user level.
    /// The value of userLevel, corresponding to policy "level" The value. If not specified, the default is 0.
    /// </summary>
    [JsonPropertyName("userLevel")]
    public int UserLevel { get; set; } = 0;

    public class Get
    {
        public readonly struct Network
        {
            public static readonly string Tcp = "tcp";
            public static readonly string Udp = "udp";
            public static readonly string TcpUdp = "tcp,udp";
        }
    }
}