# Fox's Arma Server Tool Extended Rewrite (FASTER)

> [!WARNING]
> The Linux port is a work in progress and may contain bugs. Test it with your server configuration before you rely on it.

#### Badges 
***GitHub***  
[![GitHub issues](https://img.shields.io/github/issues/Foxlider/FASTER.svg?logo=github&style=flat-square)](https://github.com/Foxlider/FASTER/issues)
![GitHub](https://img.shields.io/github/license/Foxlider/FASTER.svg?style=flat-square)
[![GitHub release](https://img.shields.io/github/release/Foxlider/FASTER.svg?logo=github&style=flat-square)](https://GitHub.com/Foxlider/FASTER/releases/)  
[![Github total downloads](https://img.shields.io/github/downloads/Foxlider/FASTER/total.svg?logo=github&style=flat-square)](https://GitHub.com/Foxlider/FASTER/releases/)
[![Github latest downloads](https://img.shields.io/github/downloads/Foxlider/FASTER/latest/total.svg?logo=github&style=flat-square)](https://GitHub.com/Foxlider/FASTER/releases/)

***Azure***  
[![Build Status](https://dev.azure.com/keelah/FASTER/_apis/build/status/Faster%20Release%20Builder?branchName=master)](https://dev.azure.com/keelah/FASTER/_build/latest?definitionId=8&branchName=master)
[![Build Status](https://vsrm.dev.azure.com/keelah/_apis/public/Release/badge/4b51eb35-4363-4038-8d99-543c01a3578f/2/2)](https://dev.azure.com/keelah/FASTER/_release)

***Code quality***  
[![Sonar Quality Gate](https://img.shields.io/sonar/quality_gate/Foxlider_FASTER?label=Code%20quality&logo=sonarcloud&logoColor=white&server=https%3A%2F%2Fsonarcloud.io&style=flat-square)](https://sonarcloud.io/dashboard?id=Foxlider_FASTER)
[![Sonar Violations (long format)](https://img.shields.io/sonar/violations/Foxlider_FASTER?format=long&label=Issues&logo=sonarcloud&logoColor=white&server=https%3A%2F%2Fsonarcloud.io&style=flat-square)](https://sonarcloud.io/project/issues?id=Foxlider_FASTER&resolved=false)


[![Discord](https://img.shields.io/discord/366955806777671681?label=Discord&logo=discord&logoColor=white&style=for-the-badge)](https://discord.gg/SJxnTNuNJN)

#### **INTRO**

FASTER is a C# rewrite of FAST2 and FAST. This branch uses .NET 10 and adds an Avalonia frontend for Windows and Linux.
Thanks go out to all the guys who helped the developpment and those who will test it. Also, to BI for giving us an awesome game to play with and break.


##### **PREREQUISITES**

- Steam account with valid copy of Arma 3.
- Basic understanding of Arma 3 dedicated servers.


## Linux installation

Linux builds use the Avalonia frontend. The AppImage includes .NET 10, so you do not need to install a .NET runtime.
The current AppImage targets x86-64 desktops with glibc 2.35 or newer.

### AppImage

1. Open the [Linux preview releases in this fork](https://github.com/milutinke/FASTER/releases).
2. Download `FASTER.AppImage` and `FASTER.AppImage.sha256` from the same release's **Assets** section.
3. Open the download directory and run:

   ```sh
   sha256sum -c FASTER.AppImage.sha256
   chmod +x FASTER.AppImage
   ./FASTER.AppImage
   ```

If your system does not have FUSE, use extraction mode:

```sh
APPIMAGE_EXTRACT_AND_RUN=1 ./FASTER.AppImage
```

Run FASTER as your normal user. Settings stay in your user's XDG configuration directory when you move or replace the AppImage.
See the [AppImage guide](packaging/linux/README.md) for build and test instructions.

### Arch Linux

The packages are not in the AUR yet. Both provide `faster-arma`, so install only one at a time.

**Prebuilt package: `faster-arma-bin`**

Download `faster-arma-bin.tar.gz` from the same [preview release](https://github.com/milutinke/FASTER/releases).
The archive contains a PKGBUILD that downloads that release's AppImage and checks its SHA-256 checksum.

```sh
sudo pacman -S --needed base-devel squashfs-tools
tar -xzf faster-arma-bin.tar.gz
cd faster-arma-bin
makepkg -si
faster-arma
```

This package installs the extracted application under `/opt/faster-arma`. It does not need FUSE or a system .NET runtime.
Use pacman to update or remove it.

**Source package: `faster-arma-git`**

This package builds the current port branch. It requires the .NET 10 SDK during the build.

```sh
sudo pacman -S --needed base-devel git dotnet-sdk
git clone --recurse-submodules --branch feat/avalonia-spike https://github.com/milutinke/FASTER.git
cd FASTER/packaging/arch/faster-arma-git
makepkg -si
faster-arma
```

See the [Arch guide](packaging/arch/README.md) for package checks and clean-chroot builds.

### NixOS

Download the AppImage from the [preview release](https://github.com/milutinke/FASTER/releases) and check its checksum as shown above.
Clone the port branch for the Nix recipes:

```sh
git clone --branch feat/avalonia-spike https://github.com/milutinke/FASTER.git
```

Copy the repository's `packaging/` directory beside your `configuration.nix` file.
Copy the checked `FASTER.AppImage` beside that file too.
Add this configuration:

```nix
{ ... }: {
  imports = [ ./packaging/nix/module.nix ];
  programs.faster-arma = {
    enable = true;
    appimage = ./FASTER.AppImage;
  };
}
```

Apply the configuration:

```sh
sudo nixos-rebuild switch
faster-arma
```

The module installs the manager and enables `nix-ld` for downloaded Arma and Steam executables.
Set `programs.faster-arma.serverCompatibility = false` if you manage those executables separately.
Use Nix to update the application. See the [NixOS guide](packaging/nix/README.md) for a standalone build and Docker tests.


##### **_FEATURES_**

- General Features
  - Theming System and Metro UI
  - Easy to read and share config files
  - Automated Update process

- SteamCMD Automation
  - Install and update Arma 3 Server (Stable, Performance, DLCs)
  - Install, update and manage Arma 3 Workshop mods
  - Import installed Steam Mods
  - Supports Steam Guard and Mobile Auth
  - Import mod presets from Arma 3 Launcher
  - Check for mod updates on app launch

- Multiple Server Profiles
  - Save and load multiple server presets
  - Supports all server config options
  - Supports all server command line options
  - Custom mission params
  - Custom difficulty
  - Headless Client support and auto launch
  - Correctly displays mods in Server Browser
  - Load Steam Mod Presets (html presets) to your profiles
  - Manually editable config files

- Local Mod Support
  - Reads local mods from server folder
  - Include additional folders to search


##### **_ISSUES and FEEDBACK_**

As always, best place to report issues is on the [GitHub Repo](https://github.com/Foxlider/FASTER/issues). As for general discussion I'll keep an eye on the BI forum thread but I'll be more active on [Discord](https://discord.gg/2BUuZa3).


##### **_DOCUMENTATION_**

A complete Documentation is available on the [GitHub Wiki](https://github.com/Foxlider/FASTER/wiki)


##### **_SCREENSHOTS_**
<details>
  <summary>Screenshots below</summary> 

  Main Menu
  ![MainMenu](https://imgur.com/Izf9Kmm.png)

  Profile Mods Menu
  ![Profile Mods Menu](https://imgur.com/D8LqSO6.png)

  Mods Menu
  ![Mods Menu](https://imgur.com/VivCLM3.png)

  Server Menu
  ![Profile Menu](https://imgur.com/1GuLqu2.png)

</details>

##### **_SOCIAL_**  
Twitter :
[@FoxliderAtom](https://twitter.com/FoxliderAtom)  
[![Twitter Follow](https://img.shields.io/twitter/follow/FoxliderAtom.svg?label=Follow&logo=twitter&style=for-the-badge)](https://twitter.com/FoxliderAtom)

Bohemia Interactive Forums :  
[Fox's Arma Server Tool Extended Rewrite (FASTER)](https://forums.bohemia.net/forums/topic/224359-foxs-arma-server-tool-extended-rewrite-faster/)

##### **_SUPPORT_**
Support the dev by making a donation here :
[![Donate](https://img.shields.io/badge/Donate-PayPal-blue.svg?style=for-the-badge&logo=paypal)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=49H6MZNFUJYWA)
  
