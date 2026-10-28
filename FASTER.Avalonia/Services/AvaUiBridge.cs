using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using FASTER.Models;
using FASTER.Services;
using FASTER.ViewModel;

namespace FASTER.Avalonia.Services;

internal sealed class AvaUiBridge : IUiBridge
{
    private readonly ViewModels.MainViewModel _main;
    private bool _attached;

    public AvaUiBridge(ViewModels.MainViewModel main) => _main = main;
    public void MarkAttached() => _attached = true;

    public void DisplayMessage(string message) => _main.ShowStatus(message);
    public void NavigateToConsole() => _main.ShowUpdater();
    public Task<int> RunModUpdaterAsync(ulong workshopId, string path)
        => SteamUpdaterViewModel.Instance.RunModUpdater(workshopId, path);
    public Task<int> RunModsUpdaterAsync(IEnumerable<ArmaMod> mods)
        => SteamUpdaterViewModel.Instance.RunModsUpdater(new ObservableCollection<ArmaMod>(mods));
    public bool IsUiLoaded() => _attached;
    public void SyncProfileMenuName(string profileId, string name) { }
    public void RemoveProfileUi(string profileId) => _main.RemoveProfile(profileId);
    public void ReloadServerProfiles() => _main.LoadServerProfiles();
    public void AppendUpdaterOutput(string text) => _main.AppendUpdaterOutput(text);

    public Task<string> GetSteamGuardCodeAsync(string prompt)
        => SteamUpdaterViewModel.Instance.SteamGuardInput();

    public Task<bool> ConfirmPhoneAuthAsync()
        => SteamUpdaterViewModel.Instance.SteamGuardInputPhone();
}
