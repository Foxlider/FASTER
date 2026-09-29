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
        // Core models raise this from background threads (e.g. failed Steam API lookups), so marshal to the UI thread here instead of at every call site.
        public void DisplayMessage(string message)
        {
            var window = MainWindow.Instance;
            if (window.Dispatcher.CheckAccess())
                window.DisplayMessage(message);
            else
                window.Dispatcher.Invoke(() => window.DisplayMessage(message));
        }

        public void NavigateToConsole() => MainWindow.Instance.NavigateToConsole();

        public Task<int> RunModUpdaterAsync(ulong workshopId, string path)
            => MainWindow.Instance.SteamUpdaterViewModel.RunModUpdater(workshopId, path);

        public Task<int> RunModsUpdaterAsync(IEnumerable<ArmaMod> mods)
            => MainWindow.Instance.SteamUpdaterViewModel.RunModsUpdater(new System.Collections.ObjectModel.ObservableCollection<ArmaMod>(mods));

        public bool IsUiLoaded() => MainWindow.HasLoaded();

        public void SyncProfileMenuName(string profileId, string name)
        {
            var button = FindProfileMenuEntry(profileId).Button;
            if (button != null)
                button.Content = name;
        }

        public void RemoveProfileUi(string profileId)
        {
            var window = MainWindow.Instance;
            var existing = window.ContentProfileViews.Find(p => p.Profile.Id == profileId);
            if (existing != null)
                window.ContentProfileViews.Remove(existing);
            var outer = FindProfileMenuEntry(profileId).Outer;
            if (outer != null)
                window.IServerProfilesMenu.Items.Remove(outer);
        }

        // Profile menu rows are DockPanels wrapping the toggle button (with reorder buttons), or bare toggle buttons, so match the button but hand back the outer row for removal.
        private static (object? Outer, ToggleButton? Button) FindProfileMenuEntry(string profileId)
        {
            foreach (var item in MainWindow.Instance.IServerProfilesMenu.Items)
            {
                var button = item is System.Windows.Controls.DockPanel dp
                    ? dp.Children.OfType<ToggleButton>().FirstOrDefault()
                    : item as ToggleButton;
                if (button?.Name == profileId) return (item, button);
            }
            return (null, null);
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
