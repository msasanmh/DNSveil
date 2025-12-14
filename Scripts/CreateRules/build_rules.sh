#!/usr/bin/env bash
# DNSveil build_rules.sh — async + memoized blocklist builder
# Requires: bash 4+, curl, sha1sum, sed, awk, grep, sort

set -euo pipefail
IFS=$'\n\t'

### CONFIG
urls_file="urls.txt"
out_rules="malicious_rules.txt"
out_raw="raw_domains.txt"
failed_log="failed_fetches.txt"

cache_root="$HOME/.cache/dnsveil"
cache_data="$cache_root/data"
cache_meta="$cache_root/meta"

max_parallel=8
user_agent="dnsveil-builder/1.1"

mkdir -p "$cache_data" "$cache_meta"

: > "$out_rules"
: > "$out_raw"
: > "$failed_log"

trap 'wait' EXIT

### HELPERS

sanitize_line() {
  local s="$1"
  s="${s//$'\xef\xbb\xbf'/}"
  s="${s%%#*}"
  printf '%s' "$s" | sed 's/^[[:space:]]*//;s/[[:space:]]*$//'
}

github_to_raw() {
  [[ "$1" =~ https://github.com/([^/]+)/([^/]+)/blob/([^/]+)/(.*) ]] \
    && printf 'https://raw.githubusercontent.com/%s/%s/%s/%s' \
       "${BASH_REMATCH[1]}" "${BASH_REMATCH[2]}" "${BASH_REMATCH[3]}" "${BASH_REMATCH[4]}" \
    || printf '%s' "$1"
}

cache_key() {
  printf '%s' "$1" | sha1sum | awk '{print $1}'
}

### FETCH (memoized, revalidated)

fetch_one() {
  local url="$1"
  local key meta_file data_file tmp

  key="$(cache_key "$url")"
  meta_file="$cache_meta/$key.headers"
  data_file="$cache_data/$key.body"
  tmp="$(mktemp)"

  local curl_args=(
    -sS --fail --location
    -A "$user_agent"
    --max-time 60
    -D "$tmp"
  )

  [[ -f "$meta_file" ]] && {
    grep -qi '^etag:' "$meta_file" && \
      curl_args+=( -H "$(grep -i '^etag:' "$meta_file")" )
    grep -qi '^last-modified:' "$meta_file" && \
      curl_args+=( -H "$(grep -i '^last-modified:' "$meta_file")" )
  }

  if ! curl "${curl_args[@]}" "$url" -o "$data_file.new"; then
    printf '%s\n' "$url" >> "$failed_log"
    rm -f "$tmp" "$data_file.new"
    return 1
  fi

  if grep -q '^HTTP/.* 304' "$tmp"; then
    rm -f "$data_file.new" "$tmp"
    cat "$data_file"
    return 0
  fi

  mv "$data_file.new" "$data_file"
  grep -Ei '^(etag:|last-modified:)' "$tmp" > "$meta_file" || true
  rm -f "$tmp"

  cat "$data_file"
}

### NORMALIZATION PIPELINE

normalize_stream() {
  sed -e 's/\r//g' \
      -e 's/^[[:space:]]*#.*$//' \
      -e 's/^[[:space:]]*//' \
      -e 's/[[:space:]]*$//' \
      -e 's/^\(0\.0\.0\.0\|127\.0\.0\.1\|::1\)[[:space:]]\+//I' \
  | sed -E 's#https?://##I; s#/.*$##'
}

### MAIN

urls=()
while IFS= read -r line || [ -n "$line" ]; do
  line="$(sanitize_line "$line")"
  [[ -z "$line" ]] && continue
  [[ "$line" == *"/blob/"* ]] && line="$(github_to_raw "$line")"
  urls+=("$line")
done < "$urls_file"

for url in "${urls[@]}"; do
  (
    echo "Fetching: $url"
    if fetch_one "$url" | normalize_stream >> "$out_raw"; then
      :
    else
      echo "Failed: $url" >&2
    fi
  ) &
  while (( $(jobs -rp | wc -l) >= max_parallel )); do
    wait -n
  done
done
wait

### DOMAIN EXTRACTION + DEDUPE

grep -Eo '([A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?\.)+[A-Za-z]{2,}' "$out_raw" \
  | tr '[:upper:]' '[:lower:]' \
  | sed 's/^\.+//;s/\.+$//' \
  | awk 'length>=4' \
  | sort -u > "$out_raw.clean"

mv "$out_raw.clean" "$out_raw"
awk '{print $0"|-;"}' "$out_raw" > "$out_rules"

echo "Rules built: $(wc -l < "$out_rules")"
[[ -s "$failed_log" ]] && echo "Some sources failed — see $failed_log"
