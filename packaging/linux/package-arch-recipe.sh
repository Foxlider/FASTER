#!/usr/bin/env bash
set -euo pipefail
image=$(realpath "${1:?Usage: package-arch-recipe.sh APPIMAGE ARCHIVE}")
archive=$(realpath -m "${2:?Archive path is required}")
script_dir=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
[[ "${PACKAGE_REPOSITORY:?Repository is required}" =~ ^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$ ]]
[[ "${PACKAGE_TAG:?Release tag is required}" =~ ^v?[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z]+([.-][0-9A-Za-z]+)*)?$ ]]
work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
mkdir "$work/faster-arma-bin"
cp "$script_dir/../arch/faster-arma-bin/"{PKGBUILD,faster-arma.desktop,faster-arma.png} "$work/faster-arma-bin/"
checksum=$(sha256sum "$image" | cut -d' ' -f1)
url="https://github.com/$PACKAGE_REPOSITORY/releases/download/$PACKAGE_TAG/FASTER.AppImage"
recipe="$work/faster-arma-bin/PKGBUILD"
sed -i "s|source=('FASTER.AppImage'|source=('FASTER.AppImage::$url'|" "$recipe"
sed -i -E "s/^sha256sums=\('[[:xdigit:]]{64}'/sha256sums=('$checksum'/" "$recipe"
sed -i 's/# Local candidate: replace this with the immutable release URL after publication./# Download the AppImage from the same release as this recipe./' "$recipe"
grep -Fq "$url" "$recipe"
grep -Fq "$checksum" "$recipe"
tar -C "$work" -czf "$archive" faster-arma-bin
