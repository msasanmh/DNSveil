#!/bin/bash
set -e

BASE_DIR="$HOME/Downloads/DNSveil-3.3.3"
EXT_FILE="$BASE_DIR/MsmhToolsClass/MsmhToolsClass/ExtensionsMethods.cs"
SSL_FILE="$BASE_DIR/MsmhToolsClass/MsmhToolsClass/MsmhAgnosticServer/ProxyServer/SSLDataEventArgs.cs"

echo "Backing up ExtensionsMethods.cs..."
cp "$EXT_FILE" "$EXT_FILE.bak"

echo "Normalizing namespace in ExtensionsMethods.cs..."
# Remove file-scoped namespace
sed -i 's/^namespace MsmhToolsClass;/namespace MsmhToolsClass {/' "$EXT_FILE"
# Ensure file ends with closing brace
if ! tail -n1 "$EXT_FILE" | grep -q '}'; then
    echo "}" >> "$EXT_FILE"
fi

echo "Removing duplicate Methods class in ExtensionsMethods.cs..."
# This assumes the duplicate class is marked with 'partial' or can be identified. Adjust if necessary.
# We'll keep the first definition and remove subsequent ones
awk '/class Methods/ && !found {found=1; print; next} /class Methods/ && found {skip=1} skip && /^\}/ {skip=0; next} !skip {print}' "$EXT_FILE" > "$EXT_FILE.tmp" && mv "$EXT_FILE.tmp" "$EXT_FILE"

echo "Creating SSLDataEventArgs stub..."
mkdir -p "$(dirname "$SSL_FILE")"
cat << 'EOF' > "$SSL_FILE"
namespace MsmhToolsClass.MsmhAgnosticServer.ProxyServer
{
    public class SSLDataEventArgs : System.EventArgs
    {
        public byte[] Data { get; set; }
        public SSLDataEventArgs(byte[] data) { Data = data; }
    }
}
EOF

echo "Staging and committing changes..."
git add "$EXT_FILE"
git add "$SSL_FILE"
git commit -m "Fix Linux build: normalize namespace, remove duplicate Methods, add SSLDataEventArgs stub"

echo "Patch applied and committed successfully!"
