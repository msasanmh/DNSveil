using System.Diagnostics;
using System.Runtime.Versioning;
using DNSveil.Platform.Abstractions;

namespace DNSveil.Platform.Linux.Dpi;

/// <summary>
/// Linux DPI bypass scaffold. Uses nftables + optional external helpers.
/// Full packet fragmentation equivalent to GoodbyeDPI requires NFQUEUE
/// userspace handler; this provides a maintainable entry point.
/// </summary>
[SupportedOSPlatform("linux")]
public sealed class LinuxDpiBypass : IDpiBypass
{
    private Process? _helper;
    private bool _active;

    public bool IsActive => _active;

    public async Task StartAsync(DpiBypassOptions options, CancellationToken ct = default)
    {
        if (_active) return;

        if (!File.Exists("/usr/sbin/nft") && !File.Exists("/sbin/nft"))
        {
            throw new PlatformNotSupportedException(
                "nftables (nft) is required for Linux DPI bypass. Install package 'nftables'.");
        }

        var rules = BuildNftRules(options);
        await ApplyNftAsync(rules, ct);
        _active = true;
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        if (!_active) return;

        try
        {
            await RunAsync("nft", "delete table inet dnsveil", ct);
        }
        catch { }

        if (_helper != null && !_helper.HasExited)
        {
            _helper.Kill(entireProcessTree: true);
            _helper.Dispose();
            _helper = null;
        }

        _active = false;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }

    private static string BuildNftRules(DpiBypassOptions options)
    {
        return
            "table inet dnsveil {\n" +
            "  chain output {\n" +
            "    type filter hook output priority 0; policy accept;\n" +
            "    tcp dport 443 counter queue num 0 bypass\n" +
            "  }\n" +
            "}\n";
    }

    private static async Task ApplyNftAsync(string rules, CancellationToken ct)
    {
        try { await RunAsync("nft", "delete table inet dnsveil", ct); } catch { }

        var tmp = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tmp, rules, ct);
            await RunAsync("nft", "-f " + tmp, ct);
        }
        finally
        {
            try { File.Delete(tmp); } catch { }
        }
    }

    private static async Task RunAsync(string file, string args, CancellationToken ct)
    {
        using var p = Process.Start(new ProcessStartInfo
        {
            FileName = file,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        });
        if (p == null) throw new InvalidOperationException("Failed to start " + file);
        var err = await p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync(ct);
        if (p.ExitCode != 0 && !string.IsNullOrWhiteSpace(err))
            throw new InvalidOperationException(file + " " + args + ": " + err.Trim());
    }
}
