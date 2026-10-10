# NixOS package candidate

This Nix recipe installs an existing FASTER AppImage into the Nix store.
It patches the ELF interpreter and library paths.
It creates the `faster-arma` command and a desktop entry.
The application needs neither FUSE, `appimage-run`, nor a global `nix-ld` setup.
The package targets `x86_64-linux`.

The package does not enable the optional .NET LTTng tracing provider.
Normal application logging and process monitoring remain available.
Use Nix to update the package. Do not replace files in the Nix store.

## Build the package

1. Open the FASTER repository root.
2. Place the tested AppImage at `packages/FASTER.AppImage`.
3. Build the package:

   ```sh
   nix-build packaging/nix/default.nix \
     --arg appimage ./packages/FASTER.AppImage
   ```

4. Start FASTER:

   ```sh
   ./result/bin/faster-arma
   ```

## Configure NixOS

1. Copy the `packaging/` directory beside your `configuration.nix` file.
2. Copy the tested `FASTER.AppImage` beside that file.
3. Add this module configuration:

   ```nix
   { ... }: {
     imports = [ ./packaging/nix/module.nix ];
     programs.faster-arma = {
       enable = true;
       appimage = ./FASTER.AppImage;
     };
   }
   ```

The application includes patches for NixOS. Downloaded Arma and Steam executables do not include those patches.
The module enables `programs.nix-ld` with the server's base libraries for those executables.
If you manage those executables separately, set `programs.faster-arma.serverCompatibility = false`.

Live Steam downloads and Arma server operation still need tests with credentials.
The Docker test covers the application's UI and Nix package.

The recipe accepts a local AppImage from the [fork's releases](https://github.com/milutinke/FASTER/releases).
After publication, you can use `pkgs.fetchurl` instead of the local file.
Use the release's actual URL and checksum. The recipe does not accept placeholder hashes.

## Test with Docker

Start from the repository root with the tested AppImage at `packages/FASTER.AppImage`.
Run this command:

```sh
docker run --rm \
  -v "$PWD/packaging:/work/packaging:ro" \
  -v "$PWD/packages:/artifacts:ro" \
  nixos/nix:latest \
  nix-build /work/packaging/nix/smoke.nix \
    --arg appimage /artifacts/FASTER.AppImage \
    --option sandbox false --no-out-link
```

This test builds the package in a Nix environment without the standard Linux filesystem layout.
It starts the application under Xvfb, a virtual display server.
It uses neither the host's .NET runtime nor the host's desktop libraries.
Docker shares the host kernel.
This test does not replace a full NixOS desktop test or a live Arma server test.
