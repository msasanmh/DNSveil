#!/usr/bin/env bash
# build_rules.sh — robust blocklist aggregator for DNSveil
# Produces: raw_domains.txt (domains only) and malicious_rules.txt (DNSveil format domain|-;)
# Place urls (one per line, inline comments allowed) in urls.txt

set -euo pipefail
IFS=$'\n\t'

urls_file="urls.txt"
out_rules="malicious_rules.txt"
out_raw="raw_domains.txt"
tmpdir="$(mktemp -d)"
failed_log="failed_fetches.txt"
trap 'rm -rf "$tmpdir"' EXIT

# Parallelism (tune to CPU / bandwidth)
PARALLEL=6

# Clear outputs
: > "$out_rules"
: > "$out_raw"
: > "$failed_log"

# Sanitize a line to a URL (strip inline comments + BOM + surrounding whitespace)
sanitize_url() {
  local u="$1"
  # remove BOM
  u="${u//$'\xef\xbb\xbf'/}"
  # strip inline comment
  u="${u%%#*}"
  # trim spaces
  u="$(printf '%s' "$u" | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//')"
  printf '%s' "$u"
}

# Worker function to fetch and normalize single URL
fetch_one() {
  local url="$1"
  local outf="$2"

  # quick validation
  if [[ -z "$url" ]]; then
    return 0
  fi

  # Use curl with retries, user agent, follow redirects
  local tmpf="$tmpdir/$(echo "$url" | sha1sum | awk '{print $1}').dat"
  if ! curl -sS --fail --location --max-time 60 --retry 3 --retry-delay 2 \
        -A "Mozilla/5.0 (compatible; blocklist-fetcher/1.0)" "$url" -o "$tmpf"; then
    printf '%s\n' "$url" >> "$failed_log"
    return 1
  fi

  # If file is gzip, gunzip it safely
  if head -c2 "$tmpf" | od -An -t x1 | grep -q '1f 8b'; then
    if command -v gunzip >/dev/null 2>&1; then
      gunzip -c "$tmpf" > "${tmpf}.txt" || { printf '%s\n' "$url" >> "$failed_log"; return 1; }
      mv "${tmpf}.txt" "$tmpf"
    fi
  fi

  # Detect HTML pages and skip them (they are not raw lists)
  # Check first 512 bytes for opening HTML tag
  if dd if="$tmpf" bs=512 count=1 2>/dev/null | tr -d '\r\n' | sed -n '1,1p' | grep -qiE '<(html|!doctype|head|body)'; then
    printf 'HTML page (skipped): %s\n' "$url" >> "$failed_log"
    return 1
  fi

  # Normalize lines:
  # - remove CR
  # - drop comments
  # - strip common hosts IP prefixes (0.0.0.0 / 127.0.0.1)
  # - remove URL schemes and path parts
  # - output possible host tokens
  sed -e 's/\r//g' \
      -e 's/^[[:space:]]*#.*$//' \
      -e 's/^[[:space:]]*//; s/[[:space:]]*$//' \
      -e 's/^\(0\.0\.0\.0\|127\.0\.0\.1\|::1\)[[:space:]]\+//I' \
      "$tmpf" \
    | sed -E 's#https?://##I; s#/.*$##' \
    | sed -E 's/^[[:space:]]*//; s/[[:space:]]*$//' \
    >> "$outf"

  return 0
}

export -f sanitize_url fetch_one
export tmpdir failed_log

# Read and sanitize URLs into an array
URLS=()
while IFS= read -r raw || [ -n "$raw" ]; do
  line="$(sanitize_url "$raw")"
  [[ -z "$line" ]] && continue
  URLS+=("$line")
done < "$urls_file"

if [ "${#URLS[@]}" -eq 0 ]; then
  echo "No URLs found in $urls_file"
  exit 1
fi

# Fetch in parallel using xargs (safe handling of spaces)
printf "%s\n" "${URLS[@]}" | xargs -n1 -P "$PARALLEL" -I {} bash -c 'fetch_one "$@"' _ {} "$out_raw"

# Extract domains from raw file, dedupe, and output DNSveil format
# Domain regex: allow subdomains, letters, digits, hyphen; TLD >=2 letters (may omit punycode edge cases)
grep -Eo '([A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?\.)+[A-Za-z]{2,}' "$out_raw" \
  | tr '[:upper:]' '[:lower:]' \
  | sed -E 's/^\.+//; s/\.+$//' \
  | awk 'length($0)>=4' \
  | sort -u > "${tmpdir}/domains_sorted.txt"

# Final outputs
awk '{print $0"|-;"}' "${tmpdir}/domains_sorted.txt" > "$out_rules"
cp "${tmpdir}/domains_sorted.txt" "$out_raw"

# Summary
echo "Generated $out_rules with $(wc -l < "$out_rules") rule lines."
if [ -s "$failed_log" ]; then
  echo "Some sources failed or were skipped. See $failed_log"
fi
