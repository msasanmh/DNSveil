using System.Text.Json.Serialization;

namespace MsmhToolsClass.V2RayConfigTool.Outbounds;

public class VlessSettings
{
    /// <summary>
    /// An array representing a list of Vless servers.
    /// </summary>
    [JsonPropertyName("vnext")]
    public List<VlessVnext> Vnext { get; set; } = new();

    public class VlessVnext
    {
        /// <summary>
        /// The server address, pointing to the server, supports domain name, IPv4, IPv6.
        /// </summary>
        [JsonPropertyName("address")]
        public string? Address { get; set; } = null;

        /// <summary>
        /// The server port is usually the same as the port that the server listens on.
        /// </summary>
        [JsonPropertyName("port")]
        public int Port { get; set; }

        /// <summary>
        /// A list of server-approved users, each of which is a user configuration.
        /// </summary>
        [JsonPropertyName("users")]
        public List<User> Users { get; set; } = new();

        public class User
        {
            /// <summary>
            /// Vless user ID can be any string less than 30 bytes, or it can be a valid UUID.
            /// </summary>
            [JsonPropertyName("id")]
            public string? ID { get; set; } = null;

            /// <summary>
            /// Need to fill "none" Can't leave empty.
            /// </summary>
            [JsonPropertyName("encryption")]
            public string Encryption { get; set; } = "none";

            /// <summary>
            /// Fluid mode, an algorithm for selecting XTLS.
            /// </summary>
            [JsonPropertyName("flow")]
            public string? Flow { get; set; } = null;

            /// <summary>
            /// At the user level, the connection uses the local policy corresponding to this user level.
            /// The default is 0.
            /// </summary>
            [JsonPropertyName("level")]
            public int Level { get; set; } = 0;
        }
    }
}