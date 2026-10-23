#!/usr/bin/env bash
set -euo pipefail

PREFIX="${PREFIX:-/usr/local}"
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"

echo "==> DNSveil Linux install"
echo "    prefix: $PREFIX"
echo "    source: $ROOT"

if [[ $EUID -ne 0 ]]; then
  echo "Please run as root (sudo)." >&2
  exit 1
fi

mkdir -p /etc/dnsveil /var/lib/dnsveil /var/log/dnsveil
mkdir -p "$PREFIX/bin"

# Publish CLI if built
if [[ -f "$ROOT/DNSveil.Cli/bin/Release/net6.0/dnsveil" ]]; then
  install -m 755 "$ROOT/DNSveil.Cli/bin/Release/net6.0/dnsveil" "$PREFIX/bin/dnsveil"
elif [[ -f "$ROOT/DNSveil.Cli/bin/Debug/net6.0/dnsveil" ]]; then
  install -m 755 "$ROOT/DNSveil.Cli/bin/Debug/net6.0/dnsveil" "$PREFIX/bin/dnsveil"
else
  echo "CLI binary not found. Build first:"
  echo "  dotnet publish DNSveil.Cli -c Release -r linux-x64 --self-contained false"
fi

# Optional: SDCAgnosticServer as backend
if [[ -f "$ROOT/SDCAgnosticServer/bin/Release/net6.0/SDCAgnosticServer" ]]; then
  install -m 755 "$ROOT/SDCAgnosticServer/bin/Release/net6.0/SDCAgnosticServer" "$PREFIX/bin/dnsveil-server"
fi

install -m 644 "$ROOT/linux/systemd/dnsveil.service" /etc/systemd/system/dnsveil.service
systemctl daemon-reload

echo "Installed. Commands:"
echo "  systemctl enable --now dnsveil"
echo "  dnsveil status"
echo "  dnsveil dns list"
