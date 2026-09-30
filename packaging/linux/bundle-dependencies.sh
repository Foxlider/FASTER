#!/usr/bin/env bash
set -euo pipefail
publish=$(realpath "${1:?Usage: bundle-dependencies.sh PUBLISH_DIRECTORY}")
script_dir=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
install -m755 "$script_dir/faster-arma" "$publish/faster-arma"
mkdir -p "$publish/licenses/system"

# These libraries are loaded by name, so ldd cannot discover them from the apphost.
for name in libX11.so.6 libICE.so.6 libSM.so.6 libfontconfig.so.1 libfreetype.so.6 libicuuc.so.70 libicui18n.so.70 libssl.so.3 libcrypto.so.3 libXrandr.so.2 libXi.so.6 libXcursor.so.1; do
    library=$(ldconfig -p | awk -v name="$name" '$1 == name && /x86-64/ && !found { print $NF; found=1 }')
    test -n "$library" || { echo "Missing build dependency: $name" >&2; exit 1; }
    cp -L "$library" "$publish/"
done

# Keep glibc and the graphics driver on the host; bundle the remaining ELF dependencies.
while :; do
    added=0
    while IFS= read -r library; do
        name=$(basename "$library")
        case "$name" in
            libc.so.*|libm.so.*|libpthread.so.*|libdl.so.*|librt.so.*|libresolv.so.*|ld-linux*|libGL.so.*|libEGL.so.*|libGLX.so.*|libGLdispatch.so.*) continue ;;
        esac
        if [[ ! -e "$publish/$name" ]]; then
            cp -L "$library" "$publish/$name"
            added=1
        fi
    done < <(find "$publish" -maxdepth 1 -type f -name '*.so*' -exec ldd {} \; | awk '/=> \/.* \(0x/ { print $3 }' | sort -u)
    [[ $added == 1 ]] || break
done

# Preserve the distribution's copyright notices for the bundled system libraries.
for library in "$publish"/*.so*; do
    name=$(basename "$library")
    source=$(ldconfig -p | awk -v name="$name" '$1 == name && /x86-64/ && !found { print $NF; found=1 }')
    [[ -n "$source" ]] || continue
    package=$(dpkg-query -S "*/$name" 2>/dev/null | awk -F': ' 'NR == 1 { print $1 }' || true)
    package=${package%%:*}
    if [[ -f "/usr/share/doc/$package/copyright" ]]; then
        cp "/usr/share/doc/$package/copyright" "$publish/licenses/system/$package.txt"
    fi
done
