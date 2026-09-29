# Functionality parity audit

Date: 2026-09-30. Branch: `feat/avalonia-spike`. Inspected commit: `37f72556b48694e7e2a8157baf972cbccd51b82b`.
Reference: upstream master `d2f885cd776d956a3b16a38acbbf642ca010d6f0`.

Static comparison of the WPF reference with Avalonia and the shared core. No application code changed. No authenticated Steam downloads, live server operations, or Windows runtime tests were performed during this audit. The earlier build and rendering checks do not establish functional parity.

## Confirmed missing or disconnected behavior

| Area | Finding | Current evidence | Reference evidence at d2f885c |
|---|---|---|---|
| Linux updater | Server downloads unconditionally select Windows public/profiling depots, although Linux IDs are declared. | `FASTER.Core/ViewModel/SteamUpdaterViewModel.cs:148`, `:189`, `:197` | Inherited Windows implementation, not a regression from the appearance commit. |
| Linux Workshop links | Mods and Deployment launch URLs with Process.Start, then fall back to Windows cmd. These paths do not use the portable browser helper. | `FASTER.Core/ViewModel/ModsViewModel.cs:172`; `FASTER.Core/ViewModel/DeploymentViewModel.cs:205` | Inherited Windows implementation, not a removed UI action. |
| Reset settings | Reset writes ClearSettings and promises a reset on restart, but neither Avalonia initialization nor the shared settings loader consumes the flag. | `FASTER.Avalonia/Views/SettingsView.axaml.cs:108`; `FASTER.Avalonia/Views/SetupView.axaml.cs:22`; `FASTER.Core/Models/AppSettings.cs:176` | `FASTER/Views/Setup.xaml.cs:45` resets settings when the flag is set. |
| Setup completion | Startup skips setup only when SetupRun is false. Continue saves FirstRun=false but leaves SetupRun=true, so a fresh installation returns to setup on subsequent starts. Setup also does not reload the saved password; Continue encrypts the textbox contents, including an empty value. | `FASTER.Avalonia/ViewModels/MainViewModel.cs:79`; `FASTER.Avalonia/Views/SetupView.axaml.cs:45`, `:83` | `FASTER/Views/Setup.xaml.cs:26` uses firstRun to decide whether to skip setup; saved password is loaded in the constructor. |
| Profile anti-flood | Enable, cycle time, limits and kick controls are absent. Core configuration generation remains. | `FASTER.Core/Models/ServerCfg.cs:495`, `:1006`; absent from `FASTER.Avalonia/Views/ProfileView.axaml` | `FASTER/Views/Profile.xaml:827` |
| Profile missions | Mission HTTP download base URL has no control. Core configuration generation remains. | `FASTER.Core/Models/ServerCfg.cs:485`, `:996`; Missions UI in `FASTER.Avalonia/Views/ProfileView.axaml:178` | `FASTER/Views/Profile.xaml:495` |
| Profile performance | Language, huge pages, load mission to memory, Steam logging, extra threads and FPS limit controls are absent. | `FASTER.Avalonia/Views/ProfileView.axaml:565`; `FASTER.Core/Models/BasicCfg.cs:35`; `FASTER.Core/Models/ServerProfile.cs:644` | `FASTER/Views/Profile.xaml:915`, `:922` |
| Profile paths | Custom BattlEye and keys-folder inputs and Browse buttons are absent; core browse methods remain. | `FASTER.Core/ViewModel/ProfileViewModel.cs:280`, `:289`; `FASTER.Avalonia/Views/ProfileView.axaml.cs` | `FASTER/Views/Profile.xaml:930` |
| Mod maintenance | Purge & Reinstall All, Purge Unused Mods, and selected-mod Purge & Reinstall have no Avalonia controls/handlers. Core methods remain. | `FASTER.Avalonia/Views/ModsView.axaml:14`, `:43`; `FASTER.Core/ViewModel/ModsViewModel.cs:283`, `:290`, `:347` | `FASTER/Views/Mods.xaml:54`, `:93` |
| Server monitoring | Per-process CPU/memory history graphs, per-process and global monitoring start/pause, output viewer and CPU temperature collection are absent. Current process table is a snapshot refreshed by rescan/kill; the timer updates system-wide metrics only. | `FASTER.Avalonia/Views/ServerStatusView.axaml:23`, `:26`; `FASTER.Avalonia/Views/ServerStatusView.axaml.cs:75`, `:149` | `FASTER/Views/ServerStatus.xaml:110`, `:139`, `:176`, `:191`, `:203`; corresponding handlers in `.xaml.cs:122`, `:138`, `:155` |
| Profile ordering and tools | No Move Up/Down profile controls or Tools shortcuts for app-data, mod-staging and server folders. | `FASTER.Avalonia/MainWindow.axaml:52`, `:60`; `FASTER.Avalonia/MainWindow.axaml.cs:11` | `FASTER/MainWindow.xaml.cs:338`, `:357`, `:376`, `:519`, `:532` |
| Settings controls | Debug logging toggle, Open Log File, font picker and theme accent selection/reset are absent. Appearance only offers Dark/Light. | `FASTER.Avalonia/Views/SettingsView.axaml:4`, `:46` | `FASTER/Views/Settings.xaml.cs:152`, `:159`, `:184`, `:192`, `:200`, `:211` |
| Console scrolling | No equivalent of the old updater console auto-follow behavior. | `FASTER.Avalonia/Views/UpdaterView.axaml:90`; no Avalonia scroll-to-end/caret handler | `FASTER/Views/Updater.xaml:178` AlwaysScrollToEnd=True |

Missing Profile controls do not imply deleted core properties: existing saved values still participate in configuration/launch argument generation.

## Existing limitations, not newly lost functionality

The update-on-launch checkboxes save CheckForModUpdates and CheckForAppUpdates, but no Avalonia/shared-core startup consumer was found (`FASTER.Avalonia/Views/SettingsView.axaml.cs:47`, `:53`; `FASTER.Avalonia/App.axaml.cs:25`). Searching upstream master also finds only settings declarations and Settings-view reads/writes, so this audit does not classify those two checkboxes as new regressions.

The analytics option saves EnableAnalytics (`FASTER.Avalonia/Views/SettingsView.axaml.cs:59`), but Avalonia has no AppCenter initialization or enable/disable call (`FASTER.Avalonia/App.axaml.cs`; `FASTER.Avalonia/Program.cs`). WPF still initializes the SDK (`FASTER/App.xaml.cs:45`). Shared Analytics.TrackEvent calls alone do not establish working Avalonia telemetry.

## Functionality still wired

Steam connect/disconnect/reset and Steam Guard prompts remain (`FASTER.Avalonia/Views/UpdaterView.axaml.cs:41`; `FASTER.Core/ViewModel/SteamUpdaterViewModel.cs:877`; `FASTER.Core/Models/AuthCodeProvider.cs`). Mod add/import/update/delete actions remain (`FASTER.Avalonia/Views/ModsView.axaml.cs`), as do deployment individual/all/clean/folder selection (`FASTER.Avalonia/Views/DeploymentView.axaml.cs`). Main profile save/delete/launch, mod selection/import/key actions and mission/difficulty settings remain (`FASTER.Avalonia/Views/ProfileView.axaml.cs`). These are code-path checks, not live end-to-end verification.

## Source links

Current source: https://github.com/milutinke/FASTER/tree/37f72556b48694e7e2a8157baf972cbccd51b82b

Reference source: https://github.com/Foxlider/FASTER/tree/d2f885cd776d956a3b16a38acbbf642ca010d6f0

## Open questions

Live Steam authentication/download behavior, server launch/process-output behavior on Linux, packaged self-updates, and Windows runtime parity still require end-to-end checks. This audit establishes the source-level gaps above, not that every remaining feature works.
