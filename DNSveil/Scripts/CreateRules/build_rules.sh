#!/usr/bin/env bash
#
# fast_build_rules.sh
# Parallel + cached blocklist aggregator for DNSveil
# Requires: GNU parallel, curl, awk, sed, grep
#
# Results:
#   - raw_domains.txt       (domain per line raw)
#   - malicious_rules.txt   (DNSveil format: domain|-;)
#   - failed_fetches.txt    (source URLs that repeatedly fail)
#   - cache/                (per-URL HTTP cache)
#

set -euo pipefail
IFS=$'\n\t'

# ----- Config -----
urls_file="urls.txt"
out_raw="raw_domains.txt"
out_rules="malicious_rules.txt"
failed_log="failed_fetches.txt"
cache_dir="cache"
workers=8        # Parallel workers
timeout=30       # curl timeout
user_agent="blocklist-fetcher/2.0"

mkdir -p "$cache_dir"
: > "$out_raw"
: > "$out_rules"
: > "$failed_log"

# ---- Helpers ----

sanitize_line() {
  local s="$1"
  s="${s//$'\xef\xbb\xbf'/}"      # BOM
  s="${s%%#*}"                    # drop inline comment
  s="$(echo "$s" | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//')"
  printf '%s' "$s"
}

convert_github_raw() {
  local u="$1"
  if [[ "$u" =~ https://github.com/([^/]+)/([^/]+)/blob/([^/]+)/(.*) ]]; then
    printf 'https://raw.githubusercontent.com/%s/%s/%s/%s' "${BASH_REMATCH[1]}" "${BASH_REMATCH[2]}" "${BASH_REMATCH[3]}" "${BASH_REMATCH[4]}"
  else
    printf '%s' "$u"
  fi
}

cache_file_for_url() {
  local url="$1"
  # safe filename: sha256sum
  printf "%s" "$url" | sha256sum | awk '{print "'"$cache_dir"'/"$1".dat"}'
}

fetch_and_cache() {
  local url="$1"
  local outf
  outf="$(cache_file_for_url "$url")"

  # If cached and nonempty, use it
  if [[ -s "$outf" ]]; then
    printf "CACHE: %s\n" "$url"
    cat "$outf"
    return 0
  fi

  # Fetch
  printf "FETCH: %s\n" "$url"
  if ! curl --fail --location --silent --max-time "$timeout" \
       -A "$user_agent" "$url" -o "$outf.tmp"; then
    rm -f "$outf.tmp"
    return 1
  fi

  # Detect HTML
  if head -n 1 "$outf.tmp" | grep -qiE '<(html|!doctype|head|body)'; then
    # try GitHub raw conversion once
    if [[ "$url" == *"github.com/"* && "$url" == *"/blob/"* ]]; then
      local raw
      raw="$(convert_github_raw "$url")"
      if [[ "$raw" != "$url" ]]; then
        if curl --fail --location --silent --max-time "$timeout" \
             -A "$user_agent" "$raw" -o "$outf.tmp"; then
          url="$raw"
        else
          rm -f "$outf.tmp"
          return 1
        fi
      else
        rm -f "$outf.tmp"
        return 1
      fi
    else
      rm -f "$outf.tmp"
      return 1
    fi
  fi

  mv "$outf.tmp" "$outf"
  cat "$outf"
}

export -f sanitize_line convert_github_raw fetch_and_cache cache_file_for_url

# ---- Fetching ----

# Build tasks: sanitize, expand github raw if needed
tasks=()
while IFS= read -r raw || [[ -n "$raw" ]]; do
  line="$(sanitize_line "$raw")"
  [[ -z "$line" ]] && continue
  # convert possible GitHub blob
  if [[ "$line" == *"github.com/"* && "$line" == *"/blob/"* ]]; then
    line="$(convert_github_raw "$line")"
  fi
  tasks+=("$line")
done < "$urls_file"

# Write tasks to a temp file
taskfile="$(mktemp)"
printf "%s\n" "${tasks[@]}" > "$taskfile"

# Run parallel fetch + normalization
parallel --jobs "$workers" --keep-order '
  url={1}
  cache_file=$(cache_file_for_url "$url")
  if ! fetch_and_cache "$url" >> "$cache_file"; then
    printf "%s\n" "$url" >> "'"$failed_log"'"
    exit 0
  fi
' :::: "$taskfile"

rm -f "$taskfile"

# ---- Normalization + Domain extraction ----

# Normalize all fetched content
# strip comments, IP prefixes, URLs, extract domains
grep -h -E '([A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?\.)+[A-Za-z]{2,}' "$cache_dir"/*.dat \
  | sed -E 's/^[0-9\.]+[[:space:]]+//' \
  | sed -E 's#https?://##I; s#/.*$##' \
  | tr '[:upper:]' '[:lower:]' \
  | sed -E 's/^[[:space:]]*//; s/[[:space:]]*$//' \
  >> "$out_raw"

# dedupe sorted
sort -u "$out_raw" -o "$out_raw"

# make DNSveil rules
awk '{print $0"|-;"}' "$out_raw" > "$out_rules"

# Summary
echo "=== Summary ==="
echo "Total domains: $(wc -l < "$out_raw")"
echo "Total rules:   $(wc -l < "$out_rules")"
echo "Failed sources (if any):"
cat "$failed_log" || true

