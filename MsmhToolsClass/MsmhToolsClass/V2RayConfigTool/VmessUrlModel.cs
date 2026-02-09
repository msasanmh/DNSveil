using System.Text.Json.Serialization;

namespace MsmhToolsClass.V2RayConfigTool;

public class VmessUrlModel
{
    [JsonPropertyName("add")]
    public object? Address { get; set; } = null;

    [JsonPropertyName("aid")]
    public object? AID { get; set; } = null; // Can Be String Or Int

    [JsonPropertyName("alpn")]
    public object? Alpn { get; set; } = null;

    [JsonPropertyName("fp")]
    public object? Fingerprint { get; set; } = null;

    [JsonPropertyName("host")]
    public object? Host { get; set; } = null;

    [JsonPropertyName("id")]
    public object? ID { get; set; } = null;

    [JsonPropertyName("net")]
    public object? Network { get; set; } = null;

    [JsonPropertyName("path")]
    public object? Path { get; set; } = null;

    [JsonPropertyName("port")]
    public object? Port { get; set; } = null; // Can Be String Or Int

    [JsonPropertyName("ps")]
    public object? Remarks { get; set; } = null;

    [JsonPropertyName("scy")]
    public object? VmessSecurity { get; set; } = null;

    [JsonPropertyName("skip-cert-verify")]
    public object? SkipCertVerify { get; set; } = null; // Bool

    [JsonPropertyName("sni")]
    public object? Sni { get; set; } = null;

    [JsonPropertyName("tls")]
    public object? Security { get; set; } = null;

    [JsonPropertyName("type")]
    public object? Type { get; set; } = null;

    [JsonPropertyName("v")]
    public object? V { get; set; } = null;
}
