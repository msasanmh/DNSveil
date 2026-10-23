namespace DNSveil.Platform.Abstractions;

/// <summary>
/// DPI bypass / anti-censorship strategies (Fragment, Fake SNI, etc.).
/// Windows uses GoodbyeDPI + WinDivert; Linux uses equivalent techniques
/// via NFQUEUE or userspace proxy.
/// </summary>
public interface IDpiBypass : IAsyncDisposable
{
    bool IsActive { get; }

    Task StartAsync(DpiBypassOptions options, CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
}

public class DpiBypassOptions
{
    public bool EnableFragment { get; set; } = true;
    public int FragmentSize { get; set; } = 2;
    public bool EnableFakeSni { get; set; }
    public string? FakeSni { get; set; }
    public bool EnableWrongChecksum { get; set; }
    public bool EnableWrongSeq { get; set; }
    public IReadOnlyList<string>? BlacklistHosts { get; set; }
}
