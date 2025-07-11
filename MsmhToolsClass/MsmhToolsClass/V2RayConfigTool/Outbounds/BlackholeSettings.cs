using System.Text.Json.Serialization;

namespace MsmhToolsClass.V2RayConfigTool.Outbounds;

public class BlackholeSettings
{
    /// <summary>
    /// The black hole's response data.
    /// </summary>
    [JsonPropertyName("type")]
    public ResponseSettings Response { get; set; } = new();

    public class ResponseSettings
    {
        /// <summary>
        /// None: Blackhole closes the connection directly.
        /// Http: Blackhole sends back a simple HTTP 403 packet, then closes the connection.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = Get.Type.Http;

        public class Get
        {
            public readonly struct Type
            {
                public static readonly string None = "none";
                public static readonly string Http = "http";
            }
        }
    }
}