# Linux AppImage

The `Package Avalonia` workflow publishes an x86-64 AppImage with .NET 10 and the
application's desktop, font, ICU, and OpenSSL libraries included. No system .NET
installation is needed. The build uses Ubuntu 22.04, so the supported baseline is
an x86-64 glibc desktop with glibc 2.35 or newer. Host graphics drivers, a display
server, and desktop fonts are still required. This is not a musl/Alpine binary.

Download `FASTER.AppImage` and `FASTER.AppImage.sha256` from the same release or CI
artifact, then run:

```sh
sha256sum -c FASTER.AppImage.sha256
chmod +x FASTER.AppImage
./FASTER.AppImage
```

If FUSE is unavailable:

```sh
APPIMAGE_EXTRACT_AND_RUN=1 ./FASTER.AppImage
```

The file can live anywhere writable by your user. Settings remain in the user's
XDG configuration directory; moving the AppImage does not reset them. The updater
can replace a writable AppImage. Do not run the server manager as root.

## Pipeline verification with Act

Use a disposable checkout with initialized submodules and your local edits. Act
skips the checkout step so it tests those edits instead of fetching the PR head.
The bind mount lets Act see the new packaging files.

```sh
act workflow_dispatch -W .github/workflows/package-avalonia.yml -j package \
  --matrix runtime:linux-x64 --bind \
  -P ubuntu-22.04=catthehacker/ubuntu:act-22.04
```

This runs Core tests, publishes the self-contained application, bundles native
libraries, creates the AppImage and update manifest, and runs every page under
Xvfb through `--smoke`. A successful smoke test must print `SMOKE-OK`.
Act skips artifact upload because its server does not support the artifact v7 API.
The bind-mounted `packages/` directory contains the AppImage, checksum, and update
manifests. GitHub runs upload these files normally.

FASTER projects enforce `TreatWarningsAsErrors` in their project files. The
external BytexDigital submodule retains its own warning policy; command-line
`-warnaserror` would incorrectly promote its existing warnings too.

The Windows solution can be cross-compiled through Act on Linux:

```sh
act workflow_dispatch -W .github/workflows/build.yml -j build --bind \
  -P windows-latest=catthehacker/ubuntu:act-22.04
```

This checks the build commands and WPF compilation, not Windows runtime behavior.
Native Windows packaging and execution must also pass on the GitHub Windows runner.
