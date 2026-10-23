using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using DNSveil.Platform.Abstractions;

namespace DNSveil.Platform.Linux.Network;

[SupportedOSPlatform("linux")]
public sealed class LinuxNetworkConfigurator : INetworkConfigurator
{
    public string ConfigRoot =>
        Directory.Exists("/etc/dnsveil") ? "/etc/dnsveil" :
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "dnsveil");

    public string DataRoot =>
        Directory.Exists("/var/lib/dnsveil") ? "/var/lib/dnsveil" :
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "dnsveil");

    public string LogRoot =>
        Directory.Exists("/var/log/dnsveil") ? "/var/log/dnsveil" :
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "state", "dnsveil", "log");

    public Task<IReadOnlyList<NetworkInterfaceInfo>> GetInterfacesAsync(CancellationToken ct = default)
    {
        var list = new List<NetworkInterfaceInfo>();
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
            var props = ni.GetIPProperties();
            list.Add(new NetworkInterfaceInfo
            {
                Name = ni.Name,
                Description = ni.Description,
                IsUp = ni.OperationalStatus == OperationalStatus.Up,
                DnsServers = props.DnsAddresses.Select(a => a.ToString()).ToList(),
                Addresses = props.UnicastAddresses.Select(a => a.Address.ToString()).ToList()
            });
        }
        return Task.FromResult<IReadOnlyList<NetworkInterfaceInfo>>(list);
    }

    public async Task SetInterfaceDnsAsync(string interfaceName, IReadOnlyList<string> servers, CancellationToken ct = default)
    {
        // Prefer resolvectl when available
        if (File.Exists("/usr/bin/resolvectl") || File.Exists("/bin/resolvectl"))
        {
            await RunAsync("resolvectl", $"dns {interfaceName} {string.Join(' ', servers)}", ct);
            return;
        }

        if (File.Exists("/usr/bin/nmcli"))
        {
            await RunAsync("nmcli", $"device modify {interfaceName} ipv4.dns \"{string.Join(' ', servers)}\"", ct);
            return;
        }

        throw new PlatformNotSupportedException("Neither resolvectl nor nmcli is available.");
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
        if (p == null) return;
        await p.WaitForExitAsync(ct);
    }
}
