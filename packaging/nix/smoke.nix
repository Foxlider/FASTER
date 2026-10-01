{ pkgs ? import <nixpkgs> { }, appimage }:
let
  faster = import ./default.nix { inherit pkgs appimage; };
in pkgs.runCommand "faster-arma-smoke" {
  nativeBuildInputs = [ faster pkgs.xvfb-run pkgs.fontconfig ];
  FONTCONFIG_FILE = pkgs.makeFontsConf { fontDirectories = [ pkgs.dejavu_fonts ]; };
} ''
  export HOME="$TMPDIR/home" XDG_CONFIG_HOME="$TMPDIR/config"
  mkdir -p "$HOME" "$XDG_CONFIG_HOME"
  timeout 60s xvfb-run -a faster-arma --smoke | tee smoke.log
  grep -q '^SMOKE-OK$' smoke.log
  mkdir -p $out
  cp smoke.log $out/
''
