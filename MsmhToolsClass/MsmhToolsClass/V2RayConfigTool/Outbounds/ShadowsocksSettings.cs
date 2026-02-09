using System.Text.Json.Serialization;

namespace MsmhToolsClass.V2RayConfigTool.Outbounds;

public class ShadowsocksSettings
{
    /// <summary>
    /// An array representing a list of Shadowsocks servers.
    /// </summary>
    [JsonPropertyName("servers")]
    public List<ShadowsocksServer> Servers { get; set; } = new();

    public class ShadowsocksServer
    {
        /// <summary>
        /// Mail address, optional, used to identify users.
        /// </summary>
        [JsonPropertyName("email")]
        public string? Email { get; set; } = null;

        /// <summary>
        /// Shadowsocks server addresses that support IPv4, IPv6, and domain names. Required.
        /// </summary>
        [JsonPropertyName("address")]
        public string? Address { get; set; } = null;

        /// <summary>
        /// Shadowsocks server port. Required.
        /// </summary>
        [JsonPropertyName("port")]
        public int Port { get; set; }

        /// <summary>
        /// Shadowsocks Encryption, required.
        /// </summary>
        [JsonPropertyName("method")]
        public string Method { get; set; } = Get.Method.None;

        /// <summary>
        /// Shadowsocks Password, required.
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
        /// UDP Over TCP
        /// </summary>
        [JsonPropertyName("uot")]
        public bool UOT { get; set; } = true;

        /// <summary>
        /// The current optional value: 1, 2
        /// </summary>
        [JsonPropertyName("UoTVersion")]
        public int UoTVersion { get; set; } = 2;

        /// <summary>
        /// At the user level, the connection uses the local policy corresponding to this user level.
        /// The default is 0.
        /// </summary>
        [JsonPropertyName("level")]
        public int Level { get; set; } = 0;

        public class Get
        {
            public class Method
            {
                /// <summary>
                /// Unencrypted traffic will be transmitted plaintext.
                /// </summary>
                public static readonly string None = "none";
                /// <summary>
                /// Unencrypted traffic will be transmitted plaintext.
                /// </summary>
                public static readonly string Plain = "plain";
                public static readonly string Aes_128_Gcm = "aes-128-gcm";
                public static readonly string Aes_256_Gcm = "aes-256-gcm";
                public static readonly string Chacha20_Poly1305 = "chacha20-poly1305";
                public static readonly string Chacha20_Ietf_Poly1305 = "chacha20-ietf-poly1305";
                public static readonly string XChacha20_Poly1305 = "xchacha20-poly1305";
                public static readonly string XChacha20_Ietf_Poly1305 = "xchacha20-ietf-poly1305";
                public static readonly string Blake3_Aes_128_Gcm_2022 = "2022-blake3-aes-128-gcm"; // Recommended
                public static readonly string Blake3_Aes_256_Gcm_2022 = "2022-blake3-aes-256-gcm"; // Recommended
                public static readonly string Blake3_Chacha20_Poly1305_2022 = "2022-blake3-chacha20-poly1305"; // Recommended
            }
        }
    }
}