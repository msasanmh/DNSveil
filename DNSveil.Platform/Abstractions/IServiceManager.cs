namespace DNSveil.Platform.Abstractions;

/// <summary>
/// Manages DNSveil as a system service (systemd on Linux, Windows Service on Windows).
/// </summary>
public interface IServiceManager
{
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
    Task RestartAsync(CancellationToken ct = default);
    Task<ServiceStatus> GetStatusAsync(CancellationToken ct = default);
    Task EnableAsync(CancellationToken ct = default);
    Task DisableAsync(CancellationToken ct = default);
    bool IsSupported { get; }
}

public enum ServiceStatus
{
    Unknown,
    Running,
    Stopped,
    Failed,
    Activating,
    Deactivating
}
