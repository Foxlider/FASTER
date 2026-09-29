# Restore functional parity

User-approved plan, 2026-09-30. Reference: upstream d2f885c. Preserve .NET 10, Linux, existing configuration formats and restored appearance. Commit and push existing PR branch; no release publishing.

## Progress Tracker
| Phase | Status |
| --- | --- |
| Startup and platform | done |
| Profile and mod controls | done |
| Monitoring and output | done |
| Settings, updates and telemetry | done |
| Validation and delivery | done |

## Tasks
- [x] Initialize settings before views; setup completion/password retention; confirmed pending reset.
- [x] Platform depots, executable permissions, safe URL/file opening.
- [x] Missing profile options, maintenance actions, ordering, Tools, console follow.
- [x] Shared bounded process monitoring/output, temperature, lifecycle and controls.
- [x] Appearance/debug settings; startup checks; shared updates and temporary telemetry.
- [x] .NET 10 Windows/Linux packaging while preserving WPF artifacts.
- [x] Regression tests, warnings-as-errors builds, isolated UI/package verification.
- [x] Final audit checklist prepared; commit and push to the existing PR branch authorized.

## Validation Commands
Use rtk proxy dotnet build for each affected project, Release, -m:1 -p:UseSharedCompilation=false -warnaserror. WPF also -p:EnableWindowsTargeting=true. Run FASTER.Core.Tests. Exercise Avalonia with isolated settings under Xvfb. Validate packages without publishing. Record unavailable Windows/live Steam checks.

## Completion
Implementation and local validation complete. Native Windows and live Steam/release-feed checks remain explicitly listed in the report. The delivery commit is recorded in the final response.
