{ config, lib, pkgs, ... }:
let
  cfg = config.programs.faster-arma;
in {
  options.programs.faster-arma = {
    enable = lib.mkEnableOption "FASTER Arma server manager";
    appimage = lib.mkOption {
      type = lib.types.path;
      description = "Path to the verified FASTER AppImage to install.";
    };
    serverCompatibility = lib.mkOption {
      type = lib.types.bool;
      default = true;
      description = "Enable nix-ld for unpatched Arma and Steam binaries downloaded by FASTER.";
    };
  };
  config = lib.mkIf cfg.enable {
    environment.systemPackages = [
      (import ./default.nix { inherit pkgs; appimage = cfg.appimage; })
    ];
    programs.nix-ld = lib.mkIf cfg.serverCompatibility {
      enable = true;
      libraries = with pkgs; [ stdenv.cc.cc.lib zlib openssl ];
    };
  };
}
