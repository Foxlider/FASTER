{ pkgs ? import <nixpkgs> { }, appimage, version ? "1.9.8" }:
let
  pname = "faster-arma";
  contents = pkgs.appimageTools.extract { inherit pname version; src = appimage; };
in
pkgs.stdenv.mkDerivation {
  inherit pname version;
  dontUnpack = true;
  # Stripping ReadyToRun .NET assemblies corrupts their embedded native images.
  dontStrip = true;
  # The bundled .NET LTTng provider is optional; regular execution uses no LTTng session.
  autoPatchelfIgnoreMissingDeps = [ "liblttng-ust.so.0" ];
  nativeBuildInputs = [ pkgs.autoPatchelfHook pkgs.makeWrapper ];
  buildInputs = with pkgs; [
    stdenv.cc.cc.lib openssl icu zlib fontconfig freetype
    libx11 libice libsm libxrandr libxi libxcursor
  ];
  installPhase = ''
    runHook preInstall
    mkdir -p $out/lib/faster-arma $out/bin
    cp -r ${contents}/usr/bin/. $out/lib/faster-arma/
    chmod -R u+w $out/lib/faster-arma
    makeWrapper $out/lib/faster-arma/FASTER.Avalonia $out/bin/faster-arma \
      --prefix LD_LIBRARY_PATH : "$out/lib/faster-arma:${pkgs.lib.makeLibraryPath [ pkgs.libglvnd ]}" \
      --prefix PATH : ${pkgs.lib.makeBinPath [ pkgs.xdg-utils ]}
    install -Dm644 ${../linux/faster-arma.desktop} $out/share/applications/faster-arma.desktop
    install -Dm644 ${../linux/faster-arma.png} $out/share/icons/hicolor/256x256/apps/faster-arma.png
    runHook postInstall
  '';
  # Package-manager installations are unpacked, so Velopack cannot replace the Nix store files.
  meta = with pkgs.lib; {
    description = "Arma 3 server and Workshop mod manager";
    homepage = "https://github.com/Foxlider/FASTER";
    license = licenses.gpl3Plus;
    platforms = [ "x86_64-linux" ];
    mainProgram = "faster-arma";
  };
}
