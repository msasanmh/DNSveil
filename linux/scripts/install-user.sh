#!/usr/bin/env bash
# Install dnsveil into ~/.local/bin (no root required).
# Works after: dotnet build DNSveil.Cli -c Release
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
DLL="$ROOT/DNSveil.Cli/bin/Release/net6.0/dnsveil.dll"
BIN_DIR="${XDG_BIN_HOME:-$HOME/.local/bin}"
LAUNCHER="$BIN_DIR/dnsveil"

if [[ ! -f "$DLL" ]]; then
  echo "Build the CLI first:" >&2
  echo "  export PATH=\"\$HOME/.dotnet:\$PATH\"" >&2
  echo "  dotnet build DNSveil.Cli -c Release" >&2
  exit 1
fi

mkdir -p "$BIN_DIR"

cat > "$LAUNCHER" << EOS
#!/usr/bin/env bash
set -e
export PATH="\$HOME/.dotnet:\$PATH"
DLL="$DLL"
if [[ ! -f "\$DLL" ]]; then
  echo "dnsveil: DLL not found at \$DLL" >&2
  echo "Rebuild: cd $ROOT && dotnet build DNSveil.Cli -c Release" >&2
  exit 1
fi
if ! command -v dotnet >/dev/null 2>&1; then
  echo "dnsveil: dotnet not found. Install .NET 6 SDK under \$HOME/.dotnet" >&2
  exit 1
fi
exec dotnet "\$DLL" "\$@"
EOS
chmod +x "$LAUNCHER"

add_path_line() {
  local file="$1"
  local line="$2"
  [[ -f "$file" ]] || touch "$file"
  grep -qxF "$line" "$file" 2>/dev/null || echo "$line" >> "$file"
}

PATH_LOCAL='export PATH="$HOME/.local/bin:$PATH"'
PATH_DOTNET='export PATH="$HOME/.dotnet:$PATH"'

for rc in "$HOME/.zshrc" "$HOME/.bashrc"; do
  add_path_line "$rc" "$PATH_LOCAL"
  add_path_line "$rc" "$PATH_DOTNET"
done

echo "Installed: $LAUNCHER"
echo "Open a new terminal, or run:"
echo "  export PATH=\"\$HOME/.local/bin:\$HOME/.dotnet:\$PATH\""
echo "  dnsveil status"
