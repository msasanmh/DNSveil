using System.Diagnostics;
using System.Runtime.Versioning;
using DNSveil.Platform.Abstractions;

namespace DNSveil.Platform.Linux.Dns;

/// <summary>
/// Detects and manages DNS via systemd-resolved, NetworkManager, or /etc/resolv.conf.
/// </summary>
[SupportedOSPlatform("linux")]
public sealed class LinuxSystemDnsManager : ISystemDnsManager
{
    private readonly string _backupPath;
    private IReadOnlyList<string>? _originalServers;
    private DnsBackend _detected = DnsBackend.Unknown;

    public LinuxSystemDnsManager(string? backupDir = null)
    {
        var dir = backupDir ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config", "dnsveil");
        Directory.CreateDirectory(dir);
        _backupPath = Path.Combine(dir, "resolv.conf.backup");
    }

    public DnsBackend DetectBackend()
    {
        if (_detected != DnsBackend.Unknown) return _detected;

        if (File.Exists("/run/systemd/resolve/stub-resolv.conf") ||
            Run("systemctl", "is-active systemd-resolved").Trim() == "active")
        {
            _detected = DnsBackend.SystemdResolved;
            return _detected;
        }

        if (Run("systemctl", "is-active NetworkManager").Trim() == "active" ||
            File.Exists("/usr/bin/nmcli"))
        {
            _detected = DnsBackend.NetworkManager;
            return _detected;
        }

        if (File.Exists("/etc/resolv.conf"))
        {
            _detected = DnsBackend.ResolvConf;
            return _detected;
        }

        _detected = DnsBackend.Other;
        return _detected;
    }

    public bool HasPrivilege()
    {
        try
        {
            return Environment.UserName == "root" || Run("id", "-u").Trim() == "0";
        }
        catch
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<string>> GetCurrentDnsAsync(CancellationToken ct = default)
    {
        var backend = DetectBackend();
        return backend switch
        {
            DnsBackend.SystemdResolved => await GetResolvedDnsAsync(ct),
            DnsBackend.NetworkManager => await GetNmcliDnsAsync(ct),
            _ => await GetResolvConfDnsAsync(ct)
        };
    }

    public async Task SetDnsAsync(IReadOnlyList<string> servers, CancellationToken ct = default)
    {
        if (servers == null || servers.Count == 0)
            throw new ArgumentException("At least one DNS server is required.", nameof(servers));

        _originalServers ??= await GetCurrentDnsAsync(ct);

        var backend = DetectBackend();
        switch (backend)
        {
            case DnsBackend.SystemdResolved:
                await SetResolvedDnsAsync(servers, ct);
                break;
            case DnsBackend.NetworkManager:
                await SetNmcliDnsAsync(servers, ct);
                break;
            default:
                await SetResolvConfDnsAsync(servers, ct);
                break;
        }
    }

    public async Task RestoreDnsAsync(CancellationToken ct = default)
    {
        if (_originalServers != null && _originalServers.Count > 0)
        {
            await SetDnsAsync(_originalServers, ct);
            _originalServers = null;
            return;
        }

        if (File.Exists(_backupPath))
        {
            File.Copy(_backupPath, "/etc/resolv.conf", overwrite: true);
        }
    }

    private static async Task<IReadOnlyList<string>> GetResolvedDnsAsync(CancellationToken ct)
    {
        var output = await RunAsync("resolvectl", "status", ct);
        var list = new List<string>();
        foreach (var line in output.Split('\n'))
        {
            var t = line.Trim();
            if (t.StartsWith("DNS Servers:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = t["DNS Servers:".Length..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                list.AddRange(parts);
            }
            else if (t.StartsWith("Current DNS Server:", StringComparison.OrdinalIgnoreCase))
            {
                var s = t["Current DNS Server:".Length..].Trim();
                if (!string.IsNullOrEmpty(s) && !list.Contains(s)) list.Insert(0, s);
            }
        }
        if (list.Count == 0)
            return await GetResolvConfDnsAsync(ct);
        return list;
    }

    private static async Task SetResolvedDnsAsync(IReadOnlyList<string> servers, CancellationToken ct)
    {
        var ifaces = await RunAsync("resolvectl", "interfaces", ct);
        var first = ifaces.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .FirstOrDefault(s => !string.IsNullOrEmpty(s) && s != "lo");

        var args = first != null
            ? $"dns {first} {string.Join(' ', servers)}"
            : $"dns {string.Join(' ', servers)}";

        await RunAsync("resolvectl", args, ct);
        await RunAsync("resolvectl", "flush-caches", ct);
    }

    private static async Task<IReadOnlyList<string>> GetNmcliDnsAsync(CancellationToken ct)
    {
        var output = await RunAsync("nmcli", "-t -f IP4.DNS device show", ct);
        var list = new List<string>();
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var idx = line.IndexOf(':');
            if (idx > 0)
            {
                var v = line[(idx + 1)..].Trim();
                if (!string.IsNullOrEmpty(v)) list.Add(v);
            }
        }
        return list.Count > 0 ? list : await GetResolvConfDnsAsync(ct);
    }

    private static async Task SetNmcliDnsAsync(IReadOnlyList<string> servers, CancellationToken ct)
    {
        var con = (await RunAsync("nmcli", "-t -f NAME,DEVICE connection show --active", ct))
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();
        if (string.IsNullOrEmpty(con)) throw new InvalidOperationException("No active NetworkManager connection.");

        var name = con.Split(':')[0];
        await RunAsync("nmcli", $"connection modify \"{name}\" ipv4.dns \"{string.Join(' ', servers)}\"", ct);
        await RunAsync("nmcli", $"connection modify \"{name}\" ipv4.ignore-auto-dns yes", ct);
        await RunAsync("nmcli", $"connection up \"{name}\"", ct);
    }

    private static async Task<IReadOnlyList<string>> GetResolvConfDnsAsync(CancellationToken ct)
    {
        await Task.Yield();
        var list = new List<string>();
        if (!File.Exists("/etc/resolv.conf")) return list;
        foreach (var line in File.ReadAllLines("/etc/resolv.conf"))
        {
            var t = line.Trim();
            if (t.StartsWith("nameserver ", StringComparison.OrdinalIgnoreCase))
            {
                var s = t["nameserver ".Length..].Trim();
                if (!string.IsNullOrEmpty(s)) list.Add(s);
            }
        }
        return list;
    }

    private async Task SetResolvConfDnsAsync(IReadOnlyList<string> servers, CancellationToken ct)
    {
        await Task.Yield();
        if (File.Exists("/etc/resolv.conf") && !File.Exists(_backupPath))
            File.Copy("/etc/resolv.conf", _backupPath);

        var lines = new List<string> { "# Generated by DNSveil" };
        foreach (var s in servers)
            lines.Add($"nameserver {s}");
        File.WriteAllLines("/etc/resolv.conf", lines);
    }

    private static string Run(string file, string args)
    {
        try
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
            var o = p.StandardOutput.ReadToEnd();
            p.WaitForExit(5000);
            return o;
        }
        catch { return ""; }
    }

    private static async Task<string> RunAsync(string file, string args, CancellationToken ct)
    {
        try
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
        catch { return ""; }
    }
}
