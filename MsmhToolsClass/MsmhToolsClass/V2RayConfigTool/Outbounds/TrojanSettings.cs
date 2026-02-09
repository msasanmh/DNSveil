using System.Text.Json.Serialization;

namespace MsmhToolsClass.V2RayConfigTool.Outbounds;

public class TrojanSettings
{
    /// <summary>
    /// An array representing a list of Trojan servers.
    /// </summary>
    [JsonPropertyName("servers")]
    public List<TrojanServer> Servers { get; set; } = new();

    public class TrojanServer
    {
        /// <summary>
        /// Mail address, optional, used to identify users.
        /// </summary>
        [JsonPropertyName("email")]
        public string? Email { get; set; } = null;

        /// <summary>
        /// Server address, supports IPv4, IPv6 and domain name. Required.
        /// </summary>
        [JsonPropertyName("address")]
        public string? Address { get; set; } = null;

        /// <summary>
        /// The server port is usually the same as the port that the server listens on.
        /// </summary>
        [JsonPropertyName("port")]
        public int Port { get; set; }

        /// <summary>
        /// Password. Required, any string.
        /// </summary>
        [JsonPropertyName("password")]
        public string? Password { get; set; } = null;

        /// <summary>
        /// One-Time-Auth
        /// Whether or not to force OTA. If true and the incoming connection doesn't enable OTA, V2Ray will reject this connection. Vice versa.
        /// If this field is not specified, V2Ray auto detects OTA settings from incoming connections.
        /// </summary>
        [JsonPropertyName("ota")]
        public bool OTA { get; set; } = false;

        /// <summary>
        /// At the user level, the connection uses the local policy corresponding to this user level.
        /// The default is 0.
        /// </summary>
        [JsonPropertyName("level")]
        public int Level { get; set; } = 0;
    }
}