#!/usr/bin/env bash
set -euo pipefail
image=$(realpath "${1:?Usage: smoke-appimage.sh APPIMAGE}")
config=$(mktemp -d)
trap 'rm -rf "$config"' EXIT
chmod +x "$image"
# Extraction mode also covers machines without FUSE, including unprivileged CI containers.
timeout 60s env -i PATH=/usr/bin:/bin HOME="$config" XDG_CONFIG_HOME="$config/config" \
    APPIMAGE_EXTRACT_AND_RUN=1 xvfb-run -a "$image" --smoke | tee "$config/output.log"
grep -q '^SMOKE-OK$' "$config/output.log"
