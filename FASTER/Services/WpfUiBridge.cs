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

        public bool IsUiLoaded() => MainWindow.HasLoaded();

        public void SyncProfileMenuName(string profileId, string name)
        {
            var menuItem = MainWindow.Instance.IServerProfilesMenu.Items.Cast<ToggleButton>().FirstOrDefault(p => p.Name == profileId);
            if (menuItem != null)
                menuItem.Content = name;
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

        public async Task<bool> ConfirmPhoneAuthAsync()
        {
            var response = await MainWindow.Instance.SteamUpdaterViewModel.SteamGuardInputPhone();
            return response == MessageDialogResult.Affirmative;
        }

        public void ReloadServerProfiles() => MainWindow.Instance.LoadServerProfiles();
    }
}
