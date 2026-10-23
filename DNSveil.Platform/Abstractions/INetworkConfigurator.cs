namespace DNSveil.Platform.Abstractions;

/// <summary>
/// Platform network configuration helpers (interfaces, routes, etc.).
/// </summary>
public interface INetworkConfigurator
{
    Task<IReadOnlyList<NetworkInterfaceInfo>> GetInterfacesAsync(CancellationToken ct = default);
    Task SetInterfaceDnsAsync(string interfaceName, IReadOnlyList<string> servers, CancellationToken ct = default);
    string ConfigRoot { get; }
    string DataRoot { get; }
    string LogRoot { get; }
}

public class NetworkInterfaceInfo
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsUp { get; set; }
    public IReadOnlyList<string> DnsServers { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Addresses { get; set; } = Array.Empty<string>();
}
