using System.Diagnostics;
using System.Runtime.Versioning;
using DNSveil.Platform.Abstractions;

namespace DNSveil.Platform.Linux.Service;

[SupportedOSPlatform("linux")]
public sealed class SystemdServiceManager : IServiceManager
{
    private readonly string _unitName;

    public SystemdServiceManager(string unitName = "dnsveil.service")
    {
        _unitName = unitName;
    }

    public bool IsSupported => File.Exists("/bin/systemctl") || File.Exists("/usr/bin/systemctl");

    public Task StartAsync(CancellationToken ct = default)
        => RunSystemctlAsync("start", ct);

    public Task StopAsync(CancellationToken ct = default)
        => RunSystemctlAsync("stop", ct);

    public Task RestartAsync(CancellationToken ct = default)
        => RunSystemctlAsync("restart", ct);

    public Task EnableAsync(CancellationToken ct = default)
        => RunSystemctlAsync("enable", ct);

    public Task DisableAsync(CancellationToken ct = default)
        => RunSystemctlAsync("disable", ct);

    public async Task<ServiceStatus> GetStatusAsync(CancellationToken ct = default)
    {
        var output = await RunAsync("systemctl", $"is-active {_unitName}", ct);
        return output.Trim() switch
        {
            "active" => ServiceStatus.Running,
            "inactive" => ServiceStatus.Stopped,
            "failed" => ServiceStatus.Failed,
            "activating" => ServiceStatus.Activating,
            "deactivating" => ServiceStatus.Deactivating,
            _ => ServiceStatus.Unknown
        };
    }

    private Task RunSystemctlAsync(string action, CancellationToken ct)
        => RunAsync("systemctl", $"{action} {_unitName}", ct);

    private static async Task<string> RunAsync(string file, string args, CancellationToken ct)
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
        if (p == null) return "";
        var o = await p.StandardOutput.ReadToEndAsync();
        await p.WaitForExitAsync(ct);
        return o;
    }
}
