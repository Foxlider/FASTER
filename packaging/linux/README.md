# Linux AppImage

The `Package Avalonia` workflow creates an x86-64 AppImage.
The AppImage includes .NET 10 and the application's desktop, font, ICU, and OpenSSL libraries.
It does not need a system .NET installation.

The build uses Ubuntu 22.04. The supported baseline is an x86-64 desktop with glibc 2.35 or newer.
The host must supply graphics drivers, a display server, and desktop fonts.
This binary does not support musl or Alpine Linux.

## Start the AppImage

1. Download `FASTER.AppImage` and `FASTER.AppImage.sha256` from the same release or CI artifact.
2. Open the directory that contains both files.
3. Check the checksum:

   ```sh
   sha256sum -c FASTER.AppImage.sha256
   ```

4. Give the AppImage execute permission:

   ```sh
   chmod +x FASTER.AppImage
   ```

5. Start the AppImage:

   ```sh
   ./FASTER.AppImage
   ```

If FUSE is unavailable, use extraction mode:

```sh
APPIMAGE_EXTRACT_AND_RUN=1 ./FASTER.AppImage
```

You can keep the AppImage in any directory that your user can write to.
FASTER keeps settings in the user's XDG configuration directory.
Moving the AppImage does not reset settings.
The updater can replace a writable AppImage.
Do not run the server manager as root.

## Test the pipeline with Act

Act runs GitHub Actions workflows locally.
Use a disposable repository copy with initialized submodules and your local edits.
Act skips the checkout step to preserve those edits.
The bind mount gives the container access to the local files.

Run this command from the repository root:

```sh
act workflow_dispatch -W .github/workflows/package-avalonia.yml -j package \
  --matrix runtime:linux-x64 --bind \
  -P ubuntu-22.04=catthehacker/ubuntu:act-22.04
```

The workflow completes these steps:

1. Run the Core tests.
2. Publish the self-contained application.
3. Include the native libraries.
4. Create the AppImage and update manifest.
5. Test every page with `--smoke` under Xvfb, a virtual display server.

Check that the smoke test prints `SMOKE-OK`.
Act skips artifact upload because its server does not support the artifact v7 API.
The repository's `packages/` directory contains the AppImage, checksum, and update manifests.
GitHub runs upload these files normally.

FASTER projects enforce `TreatWarningsAsErrors` in their project files.
The external BytexDigital submodule keeps its own warning policy.
Command-line `-warnaserror` would also promote the submodule's existing warnings to errors.

## Cross-compile the Windows solution

Run this command from the repository root:

```sh
act workflow_dispatch -W .github/workflows/build.yml -j build --bind \
  -P windows-latest=catthehacker/ubuntu:act-22.04
```

This test checks the build commands and WPF compilation.
It does not check Windows runtime behavior.
Native Windows packaging and execution must also pass on the GitHub Windows runner.
