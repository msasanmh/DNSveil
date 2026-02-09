using System.Text.Json.Serialization;

namespace MsmhToolsClass.V2RayConfigTool.Outbounds;

public class HttpSettings
{
    /// <summary>
    /// An array representing a list of Http servers.
    /// </summary>
    [JsonPropertyName("servers")]
    public List<HttpServer> Servers { get; set; } = new();

    public class HttpServer
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
        /// Password. Optional, any string.
        /// </summary>
        [JsonPropertyName("user")]
        public string? Username { get; set; } = null;

        /// <summary>
        /// Password. Optional, any string.
        /// </summary>
        [JsonPropertyName("pass")]
        public string? Password { get; set; } = null;

        /// <summary>
        /// HTTP headers are key-value pairs, where each key represents the name of an HTTP header, and all key-value pairs are included with each request.
        /// </summary>
        [JsonPropertyName("headers")]
        public Dictionary<string, string> Headers { get; set; } = new();

        /// <summary>
        /// At the user level, the connection uses the local policy corresponding to this user level.
        /// The default is 0.
        /// </summary>
        [JsonPropertyName("level")]
        public int Level { get; set; } = 0;
    }
}