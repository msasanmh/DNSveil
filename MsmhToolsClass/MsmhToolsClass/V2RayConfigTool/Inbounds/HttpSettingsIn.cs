using System.Text.Json.Serialization;

namespace MsmhToolsClass.V2RayConfigTool.Inbounds;

public class HttpSettingsIn
{
    /// <summary>
    /// An array with each element of an account. The default value is empty.
    /// </summary>
    [JsonPropertyName("accounts")]
    public List<HttpAccount> Accounts { get; set; } = new();

    /// <summary>
    /// Only For HTTP.
    /// When true, all HTTP requests are forwarded, not just proxy requests.
    /// If not configured correctly, turning this option on can result in an infinite loop.
    /// </summary>
    [JsonPropertyName("allowTransparent")]
    public bool AllowTransparent { get; set; } = false;

    /// <summary>
    /// At the user level, the connection uses the local policy corresponding to this user level.
    /// The value of userLevel, corresponding to policy "level" The value. If not specified, the default is 0.
    /// </summary>
    [JsonPropertyName("userLevel")]
    public int UserLevel { get; set; } = 0;

    public class HttpAccount
    {
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
    }
}