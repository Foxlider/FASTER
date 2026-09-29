using FASTER.Models;
using FASTER.Services;

using MahApps.Metro.Controls.Dialogs;

using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace FASTER.Services
{
    internal sealed class WpfUiBridge : IUiBridge
    {
        public void DisplayMessage(string message) => MainWindow.Instance.DisplayMessage(message);

        public void NavigateToConsole() => MainWindow.Instance.NavigateToConsole();

        public Task<int> RunModUpdaterAsync(ulong workshopId, string path)
            => MainWindow.Instance.SteamUpdaterViewModel.RunModUpdater(workshopId, path);

        public Task<int> RunModsUpdaterAsync(IEnumerable<ArmaMod> mods)
            => MainWindow.Instance.SteamUpdaterViewModel.RunModsUpdater(new System.Collections.ObjectModel.ObservableCollection<ArmaMod>(mods));

        public bool IsUiLoaded() => MainWindow.HasLoaded();

        public void SyncProfileMenuName(string profileId, string name)
        {
            var menuItem = MainWindow.Instance.IServerProfilesMenu.Items.Cast<ToggleButton>().FirstOrDefault(p => p.Name == profileId);
            if (menuItem != null)
                menuItem.Content = name;
        }

        public void RemoveProfileUi(string profileId)
        {
            var window = MainWindow.Instance;
            window.ContentProfileViews.Remove(window.ContentProfileViews.Find(p => p.Profile.Id == profileId));
            var menuItem = window.IServerProfilesMenu.Items.Cast<ToggleButton>().FirstOrDefault(p => p.Name == profileId);
            if (menuItem != null)
                window.IServerProfilesMenu.Items.Remove(menuItem);
        }

        public void AppendUpdaterOutput(string text)
            => MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += text;

        public async Task<string> GetSteamGuardCodeAsync(string prompt)
        {
            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += prompt;
            var input = await MainWindow.Instance.SteamUpdaterViewModel.SteamGuardInput();
            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\nRetrying... ";
            return input;
        }

        public Task<bool> ConfirmPhoneAuthAsync()
            => MainWindow.Instance.SteamUpdaterViewModel.SteamGuardInputPhone();

        public void ReloadServerProfiles() => MainWindow.Instance.LoadServerProfiles();
    }
}
