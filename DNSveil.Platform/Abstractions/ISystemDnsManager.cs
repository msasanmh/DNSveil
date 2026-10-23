namespace DNSveil.Platform.Abstractions;

/// <summary>
/// Manages system DNS configuration in a platform-agnostic way.
/// </summary>
public interface ISystemDnsManager
{
    /// <summary>Detect which DNS backend the OS is using.</summary>
    DnsBackend DetectBackend();

    /// <summary>Get current system DNS servers.</summary>
    Task<IReadOnlyList<string>> GetCurrentDnsAsync(CancellationToken ct = default);

    /// <summary>Set system DNS servers (typically requires elevated privileges).</summary>
    Task SetDnsAsync(IReadOnlyList<string> servers, CancellationToken ct = default);

    /// <summary>Restore original system DNS settings.</summary>
    Task RestoreDnsAsync(CancellationToken ct = default);

    /// <summary>Whether the current process has privileges to change DNS.</summary>
    bool HasPrivilege();
}

public enum DnsBackend
{
    Unknown,
    SystemdResolved,
    NetworkManager,
    ResolvConf,
    Windows,
    Other
}
