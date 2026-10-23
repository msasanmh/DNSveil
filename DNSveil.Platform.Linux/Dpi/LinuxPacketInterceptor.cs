using System.Runtime.Versioning;
using DNSveil.Platform.Abstractions;

namespace DNSveil.Platform.Linux.Dpi;

/// <summary>
/// Placeholder NFQUEUE-based packet interceptor.
/// Full implementation requires P/Invoke to libnetfilter_queue or a native helper.
/// This class defines the contract and lifecycle so Core/CLI can integrate now.
/// </summary>
[SupportedOSPlatform("linux")]
public sealed class LinuxPacketInterceptor : IPacketInterceptor
{
    private bool _running;

    public bool IsRunning => _running;

    public event EventHandler<PacketEventArgs>? PacketCaptured;

    public Task StartAsync(PacketFilter filter, CancellationToken ct = default)
    {
        if (_running) return Task.CompletedTask;

        // TODO: bind NFQUEUE (filter.QueueNumber) via native library.
        // Until then, mark as running so higher layers can proceed with
        // proxy-based DPI bypass paths.
        _running = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _running = false;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() => new(StopAsync());

    // Helper for future native callback
    internal void RaisePacket(byte[] data, int length, bool inbound)
    {
        var args = new PacketEventArgs
        {
            Data = data,
            Length = length,
            IsInbound = inbound
        };
        PacketCaptured?.Invoke(this, args);
    }
}
