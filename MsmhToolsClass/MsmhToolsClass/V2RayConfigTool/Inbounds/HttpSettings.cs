using System.Text.Json.Serialization;

namespace MsmhToolsClass.V2RayConfigTool.Inbounds;

public class HttpSettings
{
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