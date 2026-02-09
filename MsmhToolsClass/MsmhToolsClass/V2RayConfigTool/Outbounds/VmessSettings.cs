using System.Text.Json.Serialization;

namespace MsmhToolsClass.V2RayConfigTool.Outbounds;

public class VmessSettings
{
    /// <summary>
    /// An array representing a list of Vmess servers.
    /// </summary>
    [JsonPropertyName("vnext")]
    public List<VmessVnext> Vnext { get; set; } = new();

    public class VmessVnext
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
            /// 
            /// </summary>
            [JsonPropertyName("security")]
            public string Security { get; set; } = Get.Security.Auto;

            /// <summary>
            /// At the user level, the connection uses the local policy corresponding to this user level.
            /// The default is 0.
            /// </summary>
            [JsonPropertyName("level")]
            public int Level { get; set; } = 0;

            public class Get
            {
                public class Security
                {
                    /// <summary>
                    /// Unencrypted, maintain the vmess message structure
                    /// </summary>
                    public static readonly string None = "none";
                    /// <summary>
                    /// Unencrypted, direct copy of data flow (similar to vless)
                    /// </summary>
                    public static readonly string Zero = "zero";
                    /// <summary>
                    /// Default, auto-select (aes-128-gcm encryption when running the frame for AMD64, ARM64, or s390x
                    /// and Chacha20-Poly1305 encryption in other cases)
                    /// </summary>
                    public static readonly string Auto = "auto";
                    /// <summary>
                    /// Using the Chacha20-Poly1305 algorithm
                    /// </summary>
                    public static readonly string Chacha20Poly1305 = "chacha20-poly1305";
                    /// <summary>
                    /// Using the AES-128-GCM algorithm
                    /// </summary>
                    public static readonly string Aes128Gcm = "aes-128-gcm";
                }
            }
        }
    }
}