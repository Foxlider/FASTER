using BytexDigital.Steam.Core;

using MahApps.Metro.Controls.Dialogs;

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FASTER.Models
{
    internal class AuthCodeProvider : SteamAuthenticator
    {
        private readonly string _persistenceDirectory;
        private readonly string _uniqueStorageName;
        public string AccessToken { get; protected set; } = string.Empty;
        public string GuardData { get; protected set; } = string.Empty;

        public AuthCodeProvider(string uniqueStorageName, string persistenceDirectory)
        {
            _uniqueStorageName = uniqueStorageName;
            _persistenceDirectory = persistenceDirectory;
        }

        public override async Task<string> GetEmailAuthenticationCodeAsync(string accountEmail, bool previousCodeWasIncorrect, CancellationToken cancellationToken = default)
        {
            if (previousCodeWasIncorrect)
                MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\nPreviously entered email code was incorrect!";

            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\nPlease enter your 2FA code: ";

            var input = await MainWindow.Instance.SteamUpdaterViewModel.SteamGuardInput();

            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\nRetrying... ";

            return input;
        }

        public override async Task<string> GetTwoFactorAuthenticationCodeAsync(bool previousCodeWasIncorrect, CancellationToken cancellationToken = default)
        {
            if (previousCodeWasIncorrect)
                MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\nPreviously entered 2FA code was incorrect!";

            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\nPlease enter your 2FA code: ";

            var input = await MainWindow.Instance.SteamUpdaterViewModel.SteamGuardInput();

            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\nRetrying... ";

            return input;
        }

        public override async Task<bool> NotifyMobileNotificationAsync(CancellationToken cancellationToken = default)
        {
            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\nMobile notification sent. Answer \"OK\" once you've authorized this login. If no notification was received or you'd like to enter a traditional 2FA code, press \"Cancel\": ";

            MessageDialogResult response;

            do
            {
                response = await MainWindow.Instance.SteamUpdaterViewModel.SteamGuardInputPhone();
            } while (response != MessageDialogResult.Affirmative && response != MessageDialogResult.Negative);

            MainWindow.Instance.SteamUpdaterViewModel.Parameters.Output += "\n\tAuth : Authorizing...";

            return response == MessageDialogResult.Affirmative;
        }

        public override Task PersistAccessTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            AccessToken = token;

            if (string.IsNullOrEmpty(_persistenceDirectory)) return Task.CompletedTask;

            Directory.CreateDirectory(_persistenceDirectory);
            File.WriteAllText(Path.Combine(_persistenceDirectory, $"{_uniqueStorageName}_accesstoken"), AccessToken);

            return Task.CompletedTask;
        }

        public override Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(_persistenceDirectory))
            {
                return Task.FromResult(AccessToken);
            }

            var path = Path.Combine(_persistenceDirectory, $"{_uniqueStorageName}_accesstoken");

            return Task.FromResult(File.Exists(path) ? File.ReadAllText(path) : AccessToken);
        }

        public override Task PersistGuardDataAsync(string data, CancellationToken cancellationToken = default)
        {
            GuardData = data;

            if (string.IsNullOrEmpty(_persistenceDirectory)) return Task.CompletedTask;

            Directory.CreateDirectory(_persistenceDirectory);
            File.WriteAllText(Path.Combine(_persistenceDirectory, $"{_uniqueStorageName}_guarddata"), GuardData);

            return Task.CompletedTask;
        }

        public override Task<string> GetGuardDataAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(_persistenceDirectory))
            {
                return Task.FromResult(GuardData);
            }

            var path = Path.Combine(_persistenceDirectory, $"{_uniqueStorageName}_guarddata");

            return Task.FromResult(File.Exists(path) ? File.ReadAllText(path) : GuardData);
        }
    }
}
