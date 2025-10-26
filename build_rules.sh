#!/usr/bin/env bash
set -euo pipefail

urls_file="urls.txt"
out="malicious_rules.txt"
tmpdir="$(mktemp -d)"
trap 'rm -rf "$tmpdir"' EXIT

: > "$out"
echo "# Aggregated malicious_rules.txt — generated on $(date -u +"%Y-%m-%dT%H:%M:%SZ")" >> "$out"

while IFS= read -r url || [ -n "$url" ]; do
  # skip empty or comment lines
  [[ -z "${url//[[:space:]]/}" ]] && continue
  [[ "$url" =~ ^[[:space:]]*# ]] && continue

  echo "# from $url" >> "$out"
  # Fetch (curl). Try to follow redirects and time out if unreachable.
  if ! curl -fsSL --max-time 60 "$url" -o "$tmpdir/cur"; then
    echo "Warning: failed to fetch $url" >&2
    continue
  fi

  # Normalize and extract hostnames:
  # - remove CR
  # - remove comments
  # - handle hosts format (0.0.0.0 domain) and plain domain lists
  # - skip IP-only, URLs, mailto:, entries with slash/colon
  sed 's/\r//g' "$tmpdir/cur" \
    | sed 's/#.*$//' \
    | awk '
      /^[[:space:]]*$/ { next }
      {
        # split into fields
        n = split($0, f, /[[:space:]]+/)
        if (n == 0) next
        # choose last field if first is an IP, else first field
        if (f[1] ~ /^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$/ || f[1] ~ /^0\.0\.0\.0$/) {
          host = f[n]
        } else {
          host = f[1]
        }
        # lower-case
        for(i=1;i<=length(host);i++) host=tolower(host)
        # skip if contains slash, colon, @, or looks like an IP
        if (host ~ /[\/:]/) next
        if (host ~ /^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$/) next
        # strip leading dots
        gsub(/^\\.+/, "", host)
        # basic sanity: host must contain a dot
        if (host !~ /\\./) next
        print host "|-;"
      }' >> "$out"

done < "$urls_file"

# dedupe while preserving comment/context blocks
awk '
  /^#/ { print; next }
  { if (!seen[$0]++) print }
' "$out" > "${out}.uniq" && mv "${out}.uniq" "$out"

echo "Generated $out with $(grep -c \"|-;\" "$out") rule lines."
echo "Review the file before importing into DNSveil."
