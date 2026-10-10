# Arch package candidates

These packages are test candidates. Neither package is in the AUR, the Arch User Repository.
Both packages provide `faster-arma`. Install only one package at a time.

Start each installation procedure from the FASTER repository root.
Do not run `makepkg` as root.

## Prebuilt package: faster-arma-bin

For a release installation, download `faster-arma-bin.tar.gz` from the [fork's releases](https://github.com/milutinke/FASTER/releases).
Extract the archive and run `makepkg -si` inside its `faster-arma-bin/` directory.
That recipe downloads the release's AppImage with the matching checksum.

The following procedure uses the local test recipe from this repository.

1. Install the build tools:

   ```sh
   sudo pacman -S --needed base-devel squashfs-tools
   ```

2. Place the tested AppImage at `packages/FASTER.AppImage`, relative to the repository root.
3. Copy the AppImage into the recipe directory:

   ```sh
   cp packages/FASTER.AppImage packaging/arch/faster-arma-bin/
   cd packaging/arch/faster-arma-bin
   ```

4. Check the source checksums:

   ```sh
   makepkg --verifysource
   ```

5. Build and install the package:

   ```sh
   makepkg -si
   ```

6. Start FASTER:

   ```sh
   faster-arma
   ```

The recipe contains the SHA-256 checksum for the tested AppImage.
If you replace the AppImage, check its source before you update the checksum.
The recipe does not use `SKIP` for binary sources.

The package installs the application under `/opt/faster-arma`.
It needs neither FUSE nor a system .NET runtime. Use pacman to update this installation.

Before AUR submission, wait for an immutable release.
Replace the local AppImage source with that release's URL.
Update the checksum to match that release.

## Source package: faster-arma-git

1. Install the build tools:

   ```sh
   sudo pacman -S --needed base-devel git dotnet-sdk
   ```

2. Open the recipe directory:

   ```sh
   cd packaging/arch/faster-arma-git
   ```

3. Build and install the package:

   ```sh
   makepkg -si
   ```

4. Start FASTER:

   ```sh
   faster-arma
   ```

This recipe follows the `feat/avalonia-spike` branch in `milutinke/FASTER`.
That branch contains the Avalonia frontend.
After the PR merges, change the source to the `master` branch in `Foxlider/FASTER`.

The recipe uses the exact Steam submodule revision that FASTER records.
It builds a self-contained .NET 10 application under `/usr/lib`.

## Check the installation

1. Run the page tour with a temporary settings directory:

   ```sh
   XDG_CONFIG_HOME="$(mktemp -d)" faster-arma --smoke
   ```

2. Check that the output ends with `SMOKE-OK`.
3. Start FASTER without `--smoke`.
4. Check these functions:

   - Initial setup and folder selection
   - Light and dark themes
   - Settings persistence and profile editing
   - Workshop links and server monitoring
   - The desktop menu entry

Steam downloads and live server tests need your credentials and server installation.

## Check the package before installation

Run these commands from the recipe directory:

```sh
makepkg -s
pacman -Qip ./*.pkg.tar.zst
pacman -Qlp ./*.pkg.tar.zst
```

For a build in a clean chroot, install `devtools`.
Run `extra-x86_64-build` from the recipe directory.
Keep the AppImage beside the `-bin` PKGBUILD.

Before you test the other package, remove the installed package.
Use the command that matches the installed package:

```sh
sudo pacman -Rns faster-arma-bin
```

```sh
sudo pacman -Rns faster-arma-git
```
