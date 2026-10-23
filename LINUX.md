# DNSveil on Linux

This branch adds a **maintainable Linux foundation** for DNSveil without breaking the existing Windows WinForms/WPF application.

## Architecture

```
DNSveil.Platform          # OS-agnostic interfaces
DNSveil.Platform.Linux    # systemd, resolvectl/nmcli, nftables hooks
DNSveil.Cli               # `dnsveil` command-line tool
linux/systemd/            # dnsveil.service unit
linux/scripts/            # install helper
```

Core DNS/proxy logic lives in `MsmhToolsClass` / `SDCAgnosticServer` (already `net6.0`).
GUI projects remain `net6.0-windows` (WinForms / WPF).

### Abstractions

| Interface | Linux implementation |
|-----------|----------------------|
| `ISystemDnsManager` | `LinuxSystemDnsManager` (systemd-resolved → NetworkManager → resolv.conf) |
| `IServiceManager` | `SystemdServiceManager` |
| `IPacketInterceptor` | `LinuxPacketInterceptor` (NFQUEUE scaffold) |
| `IDpiBypass` | `LinuxDpiBypass` (nftables table + queue) |
| `INetworkConfigurator` | `LinuxNetworkConfigurator` |

## Build

Requirements: .NET 6 SDK.

```bash
# Core + Linux platform + CLI
dotnet restore DNSveil.Platform/DNSveil.Platform.csproj
dotnet restore DNSveil.Platform.Linux/DNSveil.Platform.Linux.csproj
dotnet restore DNSveil.Cli/DNSveil.Cli.csproj

dotnet build DNSveil.Cli/DNSveil.Cli.csproj -c Release

# Optional: DNS/proxy server (cross-platform)
dotnet build SDCAgnosticServer/SDCAgnosticServer.csproj -c Release
```

Publish self-contained (optional):

```bash
dotnet publish DNSveil.Cli -c Release -r linux-x64 --self-contained true -o ./publish/cli
dotnet publish SDCAgnosticServer -c Release -r linux-x64 --self-contained true -o ./publish/server
```

## Install

```bash
sudo ./linux/scripts/install.sh
sudo systemctl enable --now dnsveil   # when server binary is present
```

Paths:

| Path | Purpose |
|------|---------|
| `/etc/dnsveil/` | Configuration |
| `/var/lib/dnsveil/` | State |
| `/var/log/dnsveil/` | Logs (or journald) |
| `~/.config/dnsveil/` | Per-user fallback |

## CLI

```bash
dnsveil status
dnsveil dns list
sudo dnsveil dns set 127.0.0.1
sudo dnsveil dns restore
dnsveil config show
sudo systemctl start|stop|restart|status dnsveil
```

## DPI / packet filtering

Windows uses **GoodbyeDPI + WinDivert**. On Linux there is no 1:1 equivalent.

Current approach:

1. **Proxy path** (recommended): run `SDCAgnosticServer` as local HTTP/SOCKS proxy and point apps at it — works for Fragment / Fake SNI style bypasses that the agnostic server already supports in userspace.
2. **nftables + NFQUEUE**: `LinuxDpiBypass` installs an `inet dnsveil` table that queues outbound TCP/443 for userspace handling. A full fragmentation engine (native helper or managed P/Invoke to libnetfilter_queue) can be plugged into `LinuxPacketInterceptor` later.

## GUI

The main GUI (`SecureDNSClient` WinForms, `DNSveil` WPF) is Windows-only.
Priority order for Linux: **Core → Network → DNS → DPI → CLI → systemd → GUI**.
A future Avalonia or web UI can sit on the same platform interfaces.

## Windows compatibility

- No changes to `net6.0-windows` projects required for this branch.
- Windows code paths and embedded WinDivert/GoodbyeDPI binaries are untouched.
- Conditional project reference: CLI only pulls `DNSveil.Platform.Linux` when building on Linux.

## Development notes

Register platform services at startup:

```csharp
if (OperatingSystem.IsLinux())
    DNSveil.Platform.Linux.LinuxBootstrap.Initialize();
```

Then use `PlatformFactory.CreateDnsManager()` etc.
