namespace DNSveil.Platform.Abstractions;

/// <summary>
/// Platform abstraction for packet interception / modification (WinDivert on Windows,
/// NFQUEUE / nftables / TUN on Linux).
/// </summary>
public interface IPacketInterceptor : IAsyncDisposable
{
    bool IsRunning { get; }

    /// <summary>Start intercepting packets matching the given filter.</summary>
    Task StartAsync(PacketFilter filter, CancellationToken ct = default);

    /// <summary>Stop interception and release resources.</summary>
    Task StopAsync(CancellationToken ct = default);

    /// <summary>Raised when a packet is captured (before reinjection).</summary>
    event EventHandler<PacketEventArgs>? PacketCaptured;
}

public class PacketFilter
{
    public string? Expression { get; set; }
    public int QueueNumber { get; set; } = 0;
    public bool CaptureInbound { get; set; } = true;
    public bool CaptureOutbound { get; set; } = true;
}

public class PacketEventArgs : EventArgs
{
    public byte[] Data { get; init; } = Array.Empty<byte>();
    public int Length { get; init; }
    public bool IsInbound { get; init; }
    public bool Drop { get; set; }
    public byte[]? ModifiedData { get; set; }
}
