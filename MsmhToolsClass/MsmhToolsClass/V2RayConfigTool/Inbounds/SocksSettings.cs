using System.Net;
using System.Text.Json.Serialization;

namespace MsmhToolsClass.V2RayConfigTool.Inbounds;

public class SocksSettings
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
    /// At the user level, the connection uses the local policy corresponding to this user level.
    /// The value of userLevel, corresponding to policy "level" The value. If not specified, the default is 0.
    /// </summary>
    [JsonPropertyName("userLevel")]
    public int UserLevel { get; set; } = 0;
}