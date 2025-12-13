#!/usr/bin/env bash
# build_rules.sh — robust blocklist aggregator for DNSveil
# Produces:
#   - raw_domains.txt  (one domain per line, deduped)
#   - malicious_rules.txt (DNSveil format: domain|-;)
#   - failed_fetches.txt (sources that failed or were skipped)
#
# Usage: put urls.txt in the same folder, then run ./build_rules.sh
# Requires: bash 4+, curl, gunzip (optional)
# Use "dos2unix build_rules.sh" if needed.

set -euo pipefail
IFS=$'\n\t'

urls_file="urls.txt"
out_rules="malicious_rules.txt"
out_raw="raw_domains.txt"
tmpdir="$(mktemp -d)"
failed_log="failed_fetches.txt"
max_parallel=8
trap 'rm -rf "$tmpdir"' EXIT

: > "$out_rules"
: > "$out_raw"
: > "$failed_log"

# sanitize a line: remove BOM, inline comment, surrounding whitespace
sanitize_line() {
  local s="$1"
  s="${s//$'\xef\xbb\xbf'/}"      # remove BOM
  s="${s%%#*}"                    # drop inline comment
  s="$(printf '%s' "$s" | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//')"
  printf '%s' "$s"
}

# convert github.com blob URLs to raw.githubusercontent.com
github_to_raw() {
  local url="$1"
  if [[ "$url" =~ https://github.com/([^/]+)/([^/]+)/blob/([^/]+)/(.*) ]]; then
    printf 'https://raw.githubusercontent.com/%s/%s/%s/%s' \
      "${BASH_REMATCH[1]}" "${BASH_REMATCH[2]}" "${BASH_REMATCH[3]}" "${BASH_REMATCH[4]}"
  else
    printf '%s' "$url"
  fi
}

# fetch + normalize one URL
fetch_one() {
  local url="$1"
  local dest="$2"
  local tmpf="$tmpdir/$(printf '%s' "$url" | sha1sum | awk '{print $1}').dat"

  # attempt download
  if ! curl -sS --fail --location --max-time 60 -A "blocklist-fetcher/1.0" "$url" -o "$tmpf"; then
    if [[ "$url" == *"github.com/"* && "$url" == *"/blob/"* ]]; then
      local rawurl
      rawurl="$(github_to_raw "$url")"
      if [ "$rawurl" != "$url" ] && curl -sS --fail --location --max-time 60 -A "blocklist-fetcher/1.0" "$rawurl" -o "$tmpf"; then
        url="$rawurl"
      else
        printf '%s\n' "$url" >> "$failed_log"
        echo "Warning: failed to fetch $url" >&2
        return 1
      fi
    else
      printf '%s\n' "$url" >> "$failed_log"
      echo "Warning: failed to fetch $url" >&2
      return 1
    fi
  fi

  # Skip HTML pages
  if head -c512 "$tmpf" | tr -d '\r\n' | grep -qiE '<(html|!doctype|head|body)'; then
    printf 'HTML page (skipped): %s\n' "$url" >> "$failed_log"
    echo "Warning: $url returned HTML and was skipped" >&2
    return 1
  fi

  # Decompress if gzip
  if head -c2 "$tmpf" | od -An -t x1 | grep -q '1f 8b'; then
    if command -v gunzip >/dev/null 2>&1; then
      gunzip -c "$tmpf" > "${tmpf}.txt" || { printf '%s\n' "$url" >> "$failed_log"; return 1; }
      mv "${tmpf}.txt" "$tmpf"
    fi
  fi

  # Normalize and append to destination
  sed -e 's/\r//g' \
      -e 's/^[[:space:]]*#.*$//' \
      -e 's/^[[:space:]]*//; s/[[:space:]]*$//' \
      -e 's/^\(0\.0\.0\.0\|127\.0\.0\.1\|::1\)[[:space:]]\+//I' \
      "$tmpf" \
    | sed -E 's#https?://##I; s#/.*$##' \
    | sed -E 's/^[[:space:]]*//; s/[[:space:]]*$//' \
    >> "$dest"

  return 0
}

# Read urls.txt and fetch each in parallel
urls=()
while IFS= read -r line || [ -n "$line" ]; do
  line="$(sanitize_line "$line")"
  [[ -z "$line" ]] && continue
  [[ "$line" == *"github.com/"* && "$line" == *"/blob/"* ]] && line="$(github_to_raw "$line")"
  urls+=("$line")
done < "$urls_file"

# Launch parallel fetches with background jobs
for url in "${urls[@]}"; do
  (
    echo "Fetching: $url"
    fetch_one "$url" "$out_raw" || echo "Failed: $url" >&2
  ) &
  while (( $(jobs -rp | wc -l) >= max_parallel )); do
    wait -n
  done
done
wait

# Extract hostnames, normalize, dedupe
grep -Eo '([A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?\.)+[A-Za-z]{2,}' "$out_raw" \
  | tr '[:upper:]' '[:lower:]' \
  | sed -E 's/^\.+//; s/\.+$//' \
  | awk 'length($0)>=4' \
  | sort -u > "${tmpdir}/domains_sorted.txt"

# Write final DNSveil rules
awk '{print $0"|-;"}' "${tmpdir}/domains_sorted.txt" > "$out_rules"
cp "${tmpdir}/domains_sorted.txt" "$out_raw"

echo "Generated $out_rules with $(wc -l < "$out_rules") rule lines."
if [ -s "$failed_log" ]; then
  echo "Some sources failed or were skipped. See $failed_log"
fi
