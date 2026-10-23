#!/usr/bin/env bash
# Compatibility wrapper — prefer: ./linux/scripts/install.sh
exec "$(cd "$(dirname "$0")" && pwd)/install.sh" --user "$@"
