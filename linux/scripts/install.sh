#!/usr/bin/env bash
# DNSveil Linux installer
#
# Usage:
#   ./linux/scripts/install.sh              # user install (~/.local/bin), build if needed
#   ./linux/scripts/install.sh --user       # same as default
#   sudo ./linux/scripts/install.sh --system  # system-wide (/usr/local + systemd)
#   ./linux/scripts/install.sh --help
#
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
MODE="user"
SKIP_BUILD=0
PREFIX="${PREFIX:-/usr/local}"

usage() {
  cat <<'HELP'
DNSveil Linux installer

Usage:
  ./linux/scripts/install.sh [options]

Options:
  --user        Install for current user only (default)
                - Builds CLI if missing
                - Installs launcher to ~/.local/bin/dnsveil
                - Adds PATH lines to ~/.zshrc and ~/.bashrc

  --system      System-wide install (run with sudo)
                - Installs dnsveil to $PREFIX/bin (default /usr/local/bin)
                - Installs dnsveil-server if SDCAgnosticServer was built
                - Installs systemd unit: dnsveil.service
                - Creates /etc/dnsveil /var/lib/dnsveil /var/log/dnsveil

  --skip-build  Do not run dotnet build (use existing binaries)
  --prefix DIR  Install prefix for --system (default: /usr/local)
  -h, --help    Show this help

Examples:
  ./linux/scripts/install.sh
  ./linux/scripts/install.sh --user
  sudo ./linux/scripts/install.sh --system
  PREFIX=/opt/dnsveil sudo ./linux/scripts/install.sh --system

After user install, open a new terminal or run:
  export PATH="$HOME/.local/bin:$HOME/.dotnet:$PATH"
  dnsveil status
HELP
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --user) MODE="user"; shift ;;
    --system) MODE="system"; shift ;;
    --skip-build) SKIP_BUILD=1; shift ;;
    --prefix) PREFIX="$2"; shift 2 ;;
    -h|--help) usage; exit 0 ;;
    *) echo "Unknown option: $1" >&2; usage >&2; exit 1 ;;
  esac
done

export PATH="${HOME}/.dotnet:${PATH}"

need_dotnet() {
  if ! command -v dotnet >/dev/null 2>&1; then
    echo "error: 'dotnet' not found." >&2
    echo "Install .NET 6 SDK:" >&2
    echo "  curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 6.0" >&2
    echo "  export PATH=\"\$HOME/.dotnet:\$PATH\"" >&2
    exit 1
  fi
}

build_cli() {
  local dll="$ROOT/DNSveil.Cli/bin/Release/net6.0/dnsveil.dll"
  if [[ -f "$dll" && "$SKIP_BUILD" -eq 1 ]]; then
    echo "==> Using existing CLI build"
    return
  fi
  if [[ -f "$dll" && "$SKIP_BUILD" -eq 0 ]]; then
    echo "==> CLI already built (rebuilding Release)"
  else
    echo "==> Building DNSveil.Cli (Release)"
  fi
  need_dotnet
  (cd "$ROOT" && dotnet build DNSveil.Cli/DNSveil.Cli.csproj -c Release)
}

build_server_optional() {
  local dll="$ROOT/SDCAgnosticServer/bin/Release/net6.0/SDCAgnosticServer.dll"
  if [[ -f "$dll" ]]; then
    echo "==> SDCAgnosticServer already built"
    return 0
  fi
  if [[ "$SKIP_BUILD" -eq 1 ]]; then
    echo "==> Skipping server build (--skip-build)"
    return 0
  fi
  echo "==> Building SDCAgnosticServer (optional)"
  need_dotnet
  (cd "$ROOT" && dotnet build SDCAgnosticServer/SDCAgnosticServer.csproj -c Release) || {
    echo "warning: SDCAgnosticServer build failed; continuing without server" >&2
    return 0
  }
}

write_launcher() {
  local target="$1"
  local dll="$2"
  cat > "$target" <<EOS
#!/usr/bin/env bash
set -e
export PATH="\$HOME/.dotnet:\$PATH"
DLL="$dll"
if [[ ! -f "\$DLL" ]]; then
  echo "dnsveil: DLL not found at \$DLL" >&2
  echo "Rebuild: cd $ROOT && ./linux/scripts/install.sh" >&2
  exit 1
fi
if ! command -v dotnet >/dev/null 2>&1; then
  echo "dnsveil: dotnet not found. Install .NET 6 SDK under \$HOME/.dotnet" >&2
  exit 1
fi
exec dotnet "\$DLL" "\$@"
EOS
  chmod +x "$target"
}

add_path_line() {
  local file="$1"
  local line="$2"
  [[ -f "$file" ]] || touch "$file"
  grep -qxF "$line" "$file" 2>/dev/null || echo "$line" >> "$file"
}

install_user() {
  echo "==> User install"
  build_cli

  local dll="$ROOT/DNSveil.Cli/bin/Release/net6.0/dnsveil.dll"
  if [[ ! -f "$dll" ]]; then
    echo "error: CLI DLL missing after build: $dll" >&2
    exit 1
  fi

  local bin_dir="${XDG_BIN_HOME:-$HOME/.local/bin}"
  mkdir -p "$bin_dir"
  write_launcher "$bin_dir/dnsveil" "$dll"

  local path_local='export PATH="$HOME/.local/bin:$PATH"'
  local path_dotnet='export PATH="$HOME/.dotnet:$PATH"'
  for rc in "$HOME/.zshrc" "$HOME/.bashrc"; do
    add_path_line "$rc" "$path_local"
    add_path_line "$rc" "$path_dotnet"
  done

  echo ""
  echo "Installed: $bin_dir/dnsveil"
  echo "PATH updated in ~/.zshrc and ~/.bashrc"
  echo ""
  echo "Activate in this terminal:"
  echo "  export PATH=\"\$HOME/.local/bin:\$HOME/.dotnet:\$PATH\""
  echo "  dnsveil status"
  echo ""
  echo "New terminals will pick this up automatically."
}

install_system() {
  echo "==> System install (prefix: $PREFIX)"
  if [[ "${EUID:-$(id -u)}" -ne 0 ]]; then
    echo "error: --system requires root. Run: sudo $0 --system" >&2
    exit 1
  fi

  # Build as the invoking user when possible (sudo preserves HOME sometimes)
  local real_user="${SUDO_USER:-$USER}"
  local real_home
  real_home="$(getent passwd "$real_user" 2>/dev/null | cut -d: -f6 || echo "$HOME")"
  export PATH="${real_home}/.dotnet:${PATH}"

  if [[ "$SKIP_BUILD" -eq 0 ]]; then
    if [[ -n "${SUDO_USER:-}" ]]; then
      echo "==> Building as $SUDO_USER"
      sudo -u "$SUDO_USER" env PATH="${real_home}/.dotnet:${PATH}" bash -c "
        cd '$ROOT' &&
        dotnet build DNSveil.Cli/DNSveil.Cli.csproj -c Release &&
        (dotnet build SDCAgnosticServer/SDCAgnosticServer.csproj -c Release || true)
      "
    else
      build_cli
      build_server_optional
    fi
  fi

  local dll="$ROOT/DNSveil.Cli/bin/Release/net6.0/dnsveil.dll"
  if [[ ! -f "$dll" ]]; then
    echo "error: CLI not built. Build first or omit --skip-build." >&2
    exit 1
  fi

  mkdir -p "$PREFIX/bin"
  mkdir -p /etc/dnsveil /var/lib/dnsveil /var/log/dnsveil

  write_launcher "$PREFIX/bin/dnsveil" "$dll"

  local server_dll="$ROOT/SDCAgnosticServer/bin/Release/net6.0/SDCAgnosticServer.dll"
  if [[ -f "$server_dll" ]]; then
    cat > "$PREFIX/bin/dnsveil-server" <<EOS
#!/usr/bin/env bash
set -e
export PATH="${real_home}/.dotnet:\$HOME/.dotnet:\$PATH"
exec dotnet "$server_dll" "\$@"
EOS
    chmod +x "$PREFIX/bin/dnsveil-server"
    echo "Installed: $PREFIX/bin/dnsveil-server"
  else
    echo "warning: SDCAgnosticServer not built; systemd service may fail until it is."
  fi

  if [[ -f "$ROOT/linux/systemd/dnsveil.service" ]]; then
    install -m 644 "$ROOT/linux/systemd/dnsveil.service" /etc/systemd/system/dnsveil.service
    # Point ExecStart at our prefix if different
    if [[ "$PREFIX" != "/usr/local" ]]; then
      sed -i "s|/usr/local/bin/dnsveil-server|$PREFIX/bin/dnsveil-server|g" /etc/systemd/system/dnsveil.service
    fi
    systemctl daemon-reload
    echo "Installed: /etc/systemd/system/dnsveil.service"
  fi

  echo ""
  echo "System install complete."
  echo "  $PREFIX/bin/dnsveil status"
  echo "  sudo systemctl enable --now dnsveil   # if dnsveil-server is present"
}

echo "DNSveil Linux installer"
echo "  repo: $ROOT"
echo "  mode: $MODE"
echo ""

case "$MODE" in
  user) install_user ;;
  system) install_system ;;
  *) echo "error: unknown mode $MODE" >&2; exit 1 ;;
esac
