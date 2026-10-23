# DNSveil on Linux — Complete Guide

Step-by-step guide to clone, build, run, and install DNSveil on Linux.

> **Important:** The Windows GUI (WinForms / WPF) does **not** run on Linux.
> On Linux you use the **CLI** (`dnsveil`) and optionally **systemd** + `SDCAgnosticServer`.

| Item | Value |
|------|--------|
| Repository | https://github.com/seyed-ali-002/DNSveil-Linux |
| Linux branch | `linux` |
| Upstream (Windows) | https://github.com/msasanmh/DNSveil |

---

## Table of contents

1. [Prerequisites](#1-prerequisites)
2. [Clone the repository](#2-clone-the-repository)
3. [Install the .NET 6 SDK](#3-install-the-net-6-sdk)
4. [Build](#4-build)
5. [How to run the CLI (read this)](#5-how-to-run-the-cli-read-this)
6. [CLI commands](#6-cli-commands)
7. [System DNS management](#7-system-dns-management)
8. [System-wide install and systemd](#8-system-wide-install-and-systemd)
9. [Self-contained publish](#9-self-contained-publish)
10. [DNS / Proxy server (SDCAgnosticServer)](#10-dns--proxy-server-sdcagnosticserver)
11. [Config and log paths](#11-config-and-log-paths)
12. [DPI limitations on Linux](#12-dpi-limitations-on-linux)
13. [Troubleshooting](#13-troubleshooting)
14. [Architecture (developers)](#14-architecture-developers)
15. [FAQ](#15-faq)

---

## 1. Prerequisites

### OS

- A systemd-based Linux distribution (Ubuntu, Debian, Fedora, Arch, …)
- `sudo` for changing system DNS and installing the service

### Packages

```bash
# Debian / Ubuntu
sudo apt update
sudo apt install -y git curl ca-certificates

# Optional but useful
sudo apt install -y nftables
```

```bash
# Fedora
sudo dnf install -y git curl ca-certificates
```

```bash
# Arch
sudo pacman -S git curl ca-certificates
```

### Architecture

- `x64` (`linux-x64`) is the primary target
- For ARM64 use `-r linux-arm64` when publishing

---

## 2. Clone the repository

### HTTPS

```bash
git clone https://github.com/seyed-ali-002/DNSveil-Linux.git
cd DNSveil-Linux
git checkout linux
```

### SSH

```bash
git clone git@github.com:seyed-ali-002/DNSveil-Linux.git
cd DNSveil-Linux
git checkout linux
```

Verify:

```bash
git status
git branch -vv
# You should be on branch "linux"
```

---

## 3. Install the .NET 6 SDK

This branch targets **.NET 6**.

### Official install script (recommended)

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 6.0
```

Add to `PATH` for the **current terminal session**:

```bash
export PATH="$HOME/.dotnet:$PATH"
```

Make it permanent (bash):

```bash
echo 'export PATH="$HOME/.dotnet:$PATH"' >> ~/.bashrc
source ~/.bashrc
```

Check:

```bash
dotnet --version
# Expected: 6.0.x
```

Optional — disable telemetry:

```bash
export DOTNET_CLI_TELEMETRY_OPTOUT=1
```

> **Note:** Installing the SDK under `$HOME/.dotnet` does **not** automatically register a system-wide runtime.
> Always use `export PATH="$HOME/.dotnet:$PATH"` before `dotnet` commands, or use a [self-contained publish](#9-self-contained-publish).

---

## 4. Build

From the repository root:

```bash
cd /path/to/DNSveil-Linux
export PATH="$HOME/.dotnet:$PATH"
```

### Platform + CLI

```bash
dotnet build DNSveil.Platform/DNSveil.Platform.csproj -c Release
dotnet build DNSveil.Platform.Linux/DNSveil.Platform.Linux.csproj -c Release
dotnet build DNSveil.Cli/DNSveil.Cli.csproj -c Release
```

You should see `Build succeeded` for each.

### Optional — DNS/Proxy server

```bash
dotnet build SDCAgnosticServer/SDCAgnosticServer.csproj -c Release
```

Warnings about NuGet packages (e.g. `doesn't support net6.0`) can be ignored for now; they are not build errors.

### Do not build the GUI projects on Linux

These are Windows-only and will fail on Linux (expected):

- `SecureDNSClient` (WinForms)
- `DNSveil` (WPF)
- `MsmhToolsWinFormsClass` / `MsmhToolsWpfClass`

### Build outputs

```text
DNSveil.Cli/bin/Release/net6.0/dnsveil.dll   ← run with: dotnet .../dnsveil.dll
DNSveil.Cli/bin/Release/net6.0/dnsveil       ← apphost; needs runtime on PATH
```

---

## 5. How to run the CLI (read this)

This is the part that confuses most people.

### Correct ways to run

**Option A — via `dotnet` (simplest after a normal build)**

```bash
export PATH="$HOME/.dotnet:$PATH"

dotnet DNSveil.Cli/bin/Release/net6.0/dnsveil.dll help
dotnet DNSveil.Cli/bin/Release/net6.0/dnsveil.dll status
dotnet DNSveil.Cli/bin/Release/net6.0/dnsveil.dll dns list
```

**Option B — alias**

```bash
export PATH="$HOME/.dotnet:$PATH"
alias dnsveil='dotnet /path/to/DNSveil-Linux/DNSveil.Cli/bin/Release/net6.0/dnsveil.dll'

dnsveil status
dnsveil help
```

Permanent alias (bash):

```bash
echo 'export PATH="$HOME/.dotnet:$PATH"' >> ~/.bashrc
echo "alias dnsveil='dotnet $HOME/DNSveil-Linux/DNSveil.Cli/bin/Release/net6.0/dnsveil.dll'" >> ~/.bashrc
source ~/.bashrc
```

(Adjust the path if your clone is elsewhere.)

**Option C — self-contained binary** (no `dotnet` needed at runtime)

See [section 9](#9-self-contained-publish).

### Wrong ways (common mistakes)

| What you run | What happens |
|--------------|----------------|
| `./dnsveil.dll` or `dnsveil.dll` | May be picked up by **Mono** → `TypeLoadException` / `AsyncTaskMethodBuilder` errors |
| `./dnsveil` without `PATH` including `$HOME/.dotnet` | `You must install .NET` / `.NET location: Not found` |
| Forgetting `export PATH="$HOME/.dotnet:$PATH"` in a new terminal | Same "runtime not found" errors |

**Rule of thumb after a normal `dotnet build`:**

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet DNSveil.Cli/bin/Release/net6.0/dnsveil.dll <command>
```

Do **not** execute the `.dll` as a bare program.

### Example successful `status` output

```text
Platform : Linux
Config   : /home/USER/.config/dnsveil
Data     : /home/USER/.local/share/dnsveil
Log      : /home/USER/.local/state/dnsveil/log
DNS backend: SystemdResolved
Privileged : False
DNS servers: ...
Service  : Stopped
```

---


## 5b. Make `dnsveil` permanent (recommended)

After a Release build, install a launcher into `~/.local/bin` so you can type `dnsveil` in **every new terminal**:

```bash
export PATH="$HOME/.dotnet:$PATH"
cd /path/to/DNSveil-Linux

# Build once
dotnet build DNSveil.Cli -c Release

# Install user launcher (no sudo)
./linux/scripts/install-user.sh

# Load PATH in this session (or open a new terminal)
export PATH="$HOME/.local/bin:$HOME/.dotnet:$PATH"

dnsveil status
dnsveil proxy status
```

What this does:

- Writes `~/.local/bin/dnsveil` (wrapper that calls `dotnet` + your built DLL)
- Appends to `~/.zshrc` and `~/.bashrc`:
  - `export PATH="$HOME/.local/bin:$PATH"`
  - `export PATH="$HOME/.dotnet:$PATH"`

After that, **new terminals** already have `dnsveil` on `PATH`. You do not need to re-run `export` every time.

If the current terminal still says `command not found`:

```bash
export PATH="$HOME/.local/bin:$HOME/.dotnet:$PATH"
# or:
source ~/.zshrc
```

To reinstall after moving the repo or rebuilding:

```bash
./linux/scripts/install-user.sh
```

## 6. CLI commands

| Command | Description |
|---------|-------------|
| `dnsveil help` | Show help |
| `dnsveil version` | Print version |
| `dnsveil status` | Platform, DNS backend, service status |
| `dnsveil dns list` | List current system DNS servers |
| `dnsveil dns set <ip> [ip...]` | Set system DNS (**requires root**) |
| `dnsveil dns restore` | Restore previous DNS (**requires root**) |
| `dnsveil dns scan` | Placeholder (not implemented yet) |
| `dnsveil config show` | Show config paths and file |
| `dnsveil config set <key> <value>` | Save a key in `dnsveil.json` |
| `dnsveil proxy status` | Informational proxy status |
| `dnsveil start` | `systemctl start dnsveil` |
| `dnsveil stop` | `systemctl stop dnsveil` |
| `dnsveil restart` | `systemctl restart dnsveil` |

Examples (with `dotnet` + PATH):

```bash
export PATH="$HOME/.dotnet:$PATH"
DLL=DNSveil.Cli/bin/Release/net6.0/dnsveil.dll

dotnet "$DLL" status
dotnet "$DLL" dns list
sudo env PATH="$PATH" dotnet "$DLL" dns set 1.1.1.1 8.8.8.8
sudo env PATH="$PATH" dotnet "$DLL" dns restore
dotnet "$DLL" config show
dotnet "$DLL" config set upstream "https://cloudflare-dns.com/dns-query"
```

> When using `sudo`, pass `PATH` explicitly (`sudo env PATH="$PATH" ...`) so root still finds the user-installed `dotnet`.

---

## 7. System DNS management

The CLI detects the DNS backend automatically, in this order:

1. **systemd-resolved** (`resolvectl`) — preferred
2. **NetworkManager** (`nmcli`)
3. **/etc/resolv.conf** — fallback

### Suggested flow with a local resolver

1. Run a local DNS server on `127.0.0.1` (e.g. SDCAgnosticServer).
2. Point system DNS at it:

```bash
sudo env PATH="$PATH" dotnet DNSveil.Cli/bin/Release/net6.0/dnsveil.dll dns set 127.0.0.1
```

3. When finished:

```bash
sudo env PATH="$PATH" dotnet DNSveil.Cli/bin/Release/net6.0/dnsveil.dll dns restore
```

### Manual checks

```bash
resolvectl status          # if systemd-resolved is active
cat /etc/resolv.conf
nmcli device show          # if NetworkManager is active
```

---

## 8. System-wide install and systemd

### Step 1 — Release build

```bash
export PATH="$HOME/.dotnet:$PATH"
cd /path/to/DNSveil-Linux

dotnet build DNSveil.Cli -c Release
# Optional:
dotnet build SDCAgnosticServer -c Release
```

### Step 2 — Install script

```bash
sudo ./linux/scripts/install.sh
```

This script:

- Creates `/etc/dnsveil`, `/var/lib/dnsveil`, `/var/log/dnsveil`
- Installs the CLI to `/usr/local/bin/dnsveil` when a built binary is found
- Installs `SDCAgnosticServer` as `/usr/local/bin/dnsveil-server` when built
- Copies `linux/systemd/dnsveil.service` to `/etc/systemd/system/`
- Runs `systemctl daemon-reload`

### Step 3 — Enable the service

Only useful when `dnsveil-server` is installed:

```bash
sudo systemctl enable dnsveil
sudo systemctl start dnsveil
sudo systemctl status dnsveil
```

Common commands:

```bash
sudo systemctl stop dnsveil
sudo systemctl restart dnsveil
journalctl -u dnsveil -f
```

Unit file location in the repo: `linux/systemd/dnsveil.service`

- Starts after `network-online.target`
- `Restart=on-failure`
- Logs to the journal
- Writable paths: `/etc/dnsveil`, `/var/lib/dnsveil`, `/var/log/dnsveil`, `/etc/resolv.conf`

---

## 9. Self-contained publish

Use this if you want a binary that runs **without** calling `dotnet` and without relying on a system-wide runtime.

```bash
export PATH="$HOME/.dotnet:$PATH"
cd /path/to/DNSveil-Linux

mkdir -p publish/cli publish/server

dotnet publish DNSveil.Cli -c Release -r linux-x64 --self-contained true -o ./publish/cli
dotnet publish SDCAgnosticServer -c Release -r linux-x64 --self-contained true -o ./publish/server
```

Run:

```bash
./publish/cli/dnsveil status
./publish/cli/dnsveil help
./publish/server/SDCAgnosticServer
```

Framework-dependent publish (smaller; still needs `dotnet` runtime on the machine):

```bash
dotnet publish DNSveil.Cli -c Release -r linux-x64 --self-contained false -o ./publish/cli-fx
```

---

## 10. DNS / Proxy server (SDCAgnosticServer)

`SDCAgnosticServer` is the multi-protocol core (plain DNS, DoH, HTTP/HTTPS proxy, SOCKS4/4A/5). It targets `net6.0` and builds on Linux.

```bash
export PATH="$HOME/.dotnet:$PATH"

dotnet build SDCAgnosticServer -c Release
dotnet SDCAgnosticServer/bin/Release/net6.0/SDCAgnosticServer.dll
```

After the local server is listening (e.g. on `127.0.0.1`):

```bash
sudo env PATH="$PATH" dotnet DNSveil.Cli/bin/Release/net6.0/dnsveil.dll dns set 127.0.0.1
```

Configure browser or system proxy to the HTTP/SOCKS port exposed by the server (ports depend on server settings).

---

## 11. Config and log paths

| Path | Role |
|------|------|
| `/etc/dnsveil/` | System-wide config |
| `/var/lib/dnsveil/` | State / data |
| `/var/log/dnsveil/` | File logs |
| `~/.config/dnsveil/` | Per-user config fallback |
| `~/.local/share/dnsveil/` | Per-user data |
| `~/.local/state/dnsveil/log` | Per-user logs |

Config file used by the CLI:

```text
~/.config/dnsveil/dnsveil.json
# or
/etc/dnsveil/dnsveil.json
```

```bash
dotnet DNSveil.Cli/bin/Release/net6.0/dnsveil.dll config show
```

---

## 12. DPI limitations on Linux

On Windows, DNSveil uses **GoodbyeDPI + WinDivert**. There is no 1:1 equivalent on Linux.

| Approach | Status |
|----------|--------|
| Userspace proxy (`SDCAgnosticServer`) | Recommended path today |
| nftables + NFQUEUE (`LinuxDpiBypass`) | Scaffold for future work |
| GUI | Windows-only for now |

Optional tools:

```bash
sudo apt install -y nftables   # Debian/Ubuntu
```

---

## 13. Troubleshooting

### `dotnet: command not found`

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet --version
```

If still missing, reinstall the SDK (section 3).

### `You must install .NET` / `.NET location: Not found`

You ran the `dnsveil` apphost without a runtime on `PATH`.

Fix:

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet DNSveil.Cli/bin/Release/net6.0/dnsveil.dll status
```

Or use a [self-contained publish](#9-self-contained-publish).

### `TypeLoadException` / `AsyncTaskMethodBuilder` when running `dnsveil.dll`

The `.dll` was executed directly (often via **Mono**), not through `dotnet`.

Wrong:

```bash
./DNSveil.Cli/bin/Release/net6.0/dnsveil.dll
DNSveil.Cli/bin/Release/net6.0/dnsveil.dll
```

Right:

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet DNSveil.Cli/bin/Release/net6.0/dnsveil.dll status
```

### GUI projects fail to build on Linux

Expected. Only build:

- `DNSveil.Platform`
- `DNSveil.Platform.Linux`
- `DNSveil.Cli`
- `SDCAgnosticServer` (optional)
- `MsmhToolsClass` (pulled in as a dependency of the server)

### `dnsveil dns set` fails

- Use `sudo` and preserve `PATH`:

```bash
sudo env PATH="$PATH" dotnet DNSveil.Cli/bin/Release/net6.0/dnsveil.dll dns set 1.1.1.1
```

- Check backend: `dotnet ... dnsveil.dll status`
- For systemd-resolved: `resolvectl status`
- For NetworkManager: ensure an active connection exists

### Service will not start

```bash
sudo systemctl status dnsveil
journalctl -u dnsveil -n 50 --no-pager
ls -la /usr/local/bin/dnsveil-server
```

The unit expects `/usr/local/bin/dnsveil-server` (from SDCAgnosticServer install).

### DNS change has no effect

```bash
resolvectl flush-caches
# or on older systems:
# sudo systemd-resolve --flush-caches
```

Fully quit and reopen the browser.

### NuGet warnings during SDCAgnosticServer build

Messages like `Package X doesn't support net6.0` are warnings. If the build ends with `Build succeeded` and `0 Error(s)`, you can ignore them for local use.

---

## 14. Architecture (developers)

```text
DNSveil.Platform              # Shared interfaces
  └── Abstractions/
        ISystemDnsManager
        IServiceManager
        IPacketInterceptor
        IDpiBypass
        INetworkConfigurator
  └── PlatformFactory.cs

DNSveil.Platform.Linux        # Linux implementations
  ├── Dns/LinuxSystemDnsManager.cs
  ├── Service/SystemdServiceManager.cs
  ├── Network/LinuxNetworkConfigurator.cs
  ├── Dpi/LinuxDpiBypass.cs
  ├── Dpi/LinuxPacketInterceptor.cs
  └── LinuxBootstrap.cs

DNSveil.Cli                   # CLI (AssemblyName: dnsveil)
linux/systemd/dnsveil.service
linux/scripts/install.sh
```

Register platform services at startup:

```csharp
if (OperatingSystem.IsLinux())
    DNSveil.Platform.Linux.LinuxBootstrap.Initialize();

var dns = PlatformFactory.CreateDnsManager();
var svc = PlatformFactory.CreateServiceManager();
```

Windows-specific code (WinDivert, Registry, WinForms, WPF) is unchanged.

---

## 15. FAQ

**Is there a Linux GUI?**  
Not yet. Only CLI and service infrastructure. GUI is a later priority.

**Does DNSveil change my IP?**  
No. It is not a VPN. It manages secure DNS and, when configured, local proxy / SNI-related bypass paths.

**Quick start**

```bash
git clone https://github.com/seyed-ali-002/DNSveil-Linux.git
cd DNSveil-Linux && git checkout linux

curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 6.0
export PATH="$HOME/.dotnet:$PATH"

dotnet build DNSveil.Cli -c Release
dotnet DNSveil.Cli/bin/Release/net6.0/dnsveil.dll status
```

**Update to the latest `linux` branch**

```bash
git fetch origin
git checkout linux
git pull origin linux
```

---

## License

Same as the main DNSveil project — see `LICENSE` in the repository root.

---

*This guide applies to the `linux` branch of DNSveil-Linux.*
