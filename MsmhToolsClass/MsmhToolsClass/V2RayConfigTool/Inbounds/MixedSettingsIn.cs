using System.Net;
using System.Text.Json.Serialization;

namespace MsmhToolsClass.V2RayConfigTool.Inbounds;

public class MixedSettingsIn // Mixed: Only For Socks (Compatible with HTTP)
{
    /// <summary>
    /// Socks protocol authentication method, support "noauth" Anonymous and "password" User password mode.
    /// </summary>
    [JsonPropertyName("auth")]
    public string Auth { get; set; } = "noauth";

    /// <summary>
    /// Whether to enable support for UDP protocol.
    /// </summary>
    [JsonPropertyName("udp")]
    public bool Udp { get; set; } = true;

    /// <summary>
    /// When UDP is enabled, Xray needs to know the local IP address.
    /// </summary>
    [JsonPropertyName("ip")]
    public string IP { get; set; } = IPAddress.Any.ToString();

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
}