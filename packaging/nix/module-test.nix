let
  system = import <nixpkgs/nixos> {
    configuration = {
      imports = [ ./module.nix ];
      programs.faster-arma = {
        enable = true;
        appimage = /artifacts/FASTER.AppImage;
      };
      system.stateVersion = "26.05";
    };
  };
in {
  compatibilityEnabled = system.config.programs.nix-ld.enable;
  package = (builtins.head (builtins.filter (p: (p.pname or "") == "faster-arma") system.config.environment.systemPackages)).drvPath;
  libraries = map (p: p.name) system.config.programs.nix-ld.libraries;
}
