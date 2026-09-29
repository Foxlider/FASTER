# Functional parity restoration

Reference: upstream `master` at `d2f885c`. Branch: `feat/avalonia-spike`, PR #270. No core configuration version change, release publication, or merge.

## Audit checklist

| Finding | Result |
| --- | --- |
| Settings loaded after view models; pending reset ignored | Fixed: load/import/reset and collection initialization run before Avalonia view models. Reset changes configuration only. |
| Setup marked complete merely by opening it; conflicting completion flags | Fixed: FirstRun is authoritative; successful submission saves both flags false. Existing completed installations retain settings. |
| Saved setup values/password omitted | Fixed: restore saved values; unchanged password retains its ciphertext; changed passwords are encrypted. |
| Legacy mod collections | Fixed: convert saved Workshop/local entries without moving or deleting their files. |
| Windows-only public/profiling depots | Fixed: select 233782/233784 on Windows and 233783/233785 on Linux; shared and DLC selections remain. |
| Linux downloaded binaries not executable | Fixed: preserve existing mode bits and add executable permissions to server binaries after successful installation. |
| Windows-only URL/folder opening and unsafe argument strings | Fixed: shared platform opening; Unix desktop openers receive one argument; Windows uses shell association directly. |
| Missing profile controls | Fixed: anti-flood controls, mission download URL, language, huge pages, mission preloading, Steam logs, extra threads, FPS limit, BattlEye and keys paths. Existing generators and browse services remain in use. |
| Unknown profile values | Fixed: preserve unknown JSON values in profiles/basic/server configuration, including profile cloning. |
| Missing maintenance actions | Fixed: purge/reinstall all and selected, purge unused, confirmations, local-mod exclusions and cancellation between deletions. Imports/bulk deletion/maintenance block conflicting UI actions and app-update restart. |
| Profile ordering, Tools, updater follow | Fixed: persisted move up/down; server/staging/settings-directory shortcuts; follow output at the bottom and allow scrolling back. |
| Process CPU/memory and history | Fixed: shared framework-independent service; refreshed working set; elapsed CPU divided by elapsed wall time and processor count; bounded history. |
| Process identity and controls | Fixed: PID + start time, pause/resume per process and globally, rescan, confirmed termination, selected-process charts. |
| Process output | Fixed: shared launch capture for servers/headless clients, bounded output view. External processes explicitly report unavailable capture. Linux uses an asynchronously read log so closing FASTER does not close the server's output pipe. |
| Temperature/lifecycle | Fixed: Linux CPU hwmon/thermal sources and Windows WMI; invalid/missing readings show Unavailable. View timers stop on unload; services release resources at exit without killing servers. |
| Debug logging and Open Log File | Fixed. Fatal diagnostics remain local even when debug logging or telemetry is disabled. |
| Appearance preferences | Fixed: font/accent selection and reset controls below Program Settings; persisted startup application; installed-font fallback. |
| Startup checks | Fixed: once after initialization/setup completion; mod checks update metadata without downloading mods. |
| App updates | Fixed: shared service with serialized checks, error/cancellation handling, packaged download and confirmed restart, busy-operation deferral, release-page offer for development installations. |
| Build/release workflows | Fixed: .NET 10; matching Velopack 1.2.158; Windows/Linux packages and manifests; WPF artifacts and current release repository retained. |
| Temporary App Center | Intentionally limited: shared opt-out boundary, Windows adapter and unavailable Linux status; initialization/send failures isolated. Remote delivery is unverified. Legacy custom payloads are discarded; detailed automatic crash uploads are not enabled because they can contain paths or credentials. |

## Validation

- Core regression suite: **50 passed, 1 Windows-only test skipped** on Linux.
- Avalonia and WPF Release builds: warnings treated as errors; zero warnings/errors.
- Self-contained Avalonia publishes for `linux-x64` and `win-x64` succeeded.
- Velopack generated Linux AppImage/full package/update manifest and Windows installer/portable/full package/update manifest locally. Windows packaging used the explicit `[win]` cross-compilation directive. Packages are unsigned, matching the absence of signing credentials in this environment.
- The real Velopack adapter correctly identifies an unpackaged run after normal application initialization.
- The generated Linux AppImage completed the eight-page smoke tour. Package manifest sizes and SHA256 hashes match both platforms’ generated assets.
- Workflow YAML parsed successfully; the package workflow has no publishing step. The existing release workflow consumes its artifacts only when the existing release triggers run.
- Isolated Avalonia UI checks exercised all pages and four Profile tabs, populated mod/process tables, navigation, theme/accent persistence and collapsed message-log behavior. Screens captured at 1000×700 and 1100×700 in both themes at 100% scaling, and 1000×700, 1100×700 and 1600×900 at 150% scaling.
- Tests cover depot selection, permissions, setup/password/reset, legacy migration, ordering, unknown values, restored configuration arguments, maintenance confirmation/cancellation/local preservation, process sampling/capture/pause/exit/bounds, fake update feeds, restart deferral, and telemetry opt-out/failures.
- A fresh Linux run with a nonexistent XDG configuration directory writes settings to that directory, not the working directory.
- `git diff --check` passes.

## Awaiting runtime verification

- Native Windows UI, WPF execution, WMI temperature availability and installed Windows update/restart behavior. Cross-compilation and package generation do not substitute for Windows runtime testing.
- Real Steam depot downloads, profiling/DLC installation, authentication flows, server/headless-client launch and game behavior with suitable credentials/installations.
- Real release-feed download/apply across installed versions. Fake-feed tests exercise update outcomes and restart deferral; no release was published for this task.
- App Center delivery is deliberately not claimed. The retained service is retired; no provider migration was made.

## Implementation notes

Settings and configuration formats remain compatible. Unknown profile values are retained as JSON extension data. Linux capture logs live beside application settings under `ProcessOutput`; in-memory output is bounded and active logs are trimmed while FASTER runs. Servers can continue writing those logs after FASTER exits.

Validation was run after coherent changes across related C#/XAML files, rather than rebuilding after each individual file, to keep execution efficient. The original functionality audit remains in `.agent-state/research` as the pre-change record.
