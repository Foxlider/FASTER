using BytexDigital.Steam.Core;

using FASTER.Services;
using FASTER.ViewModel;

namespace FASTER.Models;

internal class AuthCodeProvider : SteamAuthenticator
{
    private readonly string _persistenceDirectory;
    private readonly string _uniqueStorageName;
    private readonly SteamUpdaterViewModel _updater;

    public string AccessToken { get; protected set; } = string.Empty;
    public string GuardData { get; protected set; } = string.Empty;

    public AuthCodeProvider(string uniqueStorageName, string persistenceDirectory, SteamUpdaterViewModel updater)
    {
        _uniqueStorageName = uniqueStorageName;
        _persistenceDirectory = persistenceDirectory;
        _updater = updater;
    }

    public override async Task<string> GetEmailAuthenticationCodeAsync(string accountEmail, bool previousCodeWasIncorrect, CancellationToken cancellationToken = default)
    {
        if (previousCodeWasIncorrect)
            Ui.Current.AppendUpdaterOutput("\nPreviously entered email code was incorrect!");

        Ui.Current.AppendUpdaterOutput("\nPlease enter your 2FA code: ");

        var input = await _updater.SteamGuardInput();

        Ui.Current.AppendUpdaterOutput("\nRetrying... ");

        return input;
    }

    public override async Task<string> GetTwoFactorAuthenticationCodeAsync(bool previousCodeWasIncorrect, CancellationToken cancellationToken = default)
    {
        if (previousCodeWasIncorrect)
            Ui.Current.AppendUpdaterOutput("\nPreviously entered 2FA code was incorrect!");

        Ui.Current.AppendUpdaterOutput("\nPlease enter your 2FA code: ");

        var input = await _updater.SteamGuardInput();

        Ui.Current.AppendUpdaterOutput("\nRetrying... ");

        return input;
    }

    public override async Task<bool> NotifyMobileNotificationAsync(CancellationToken cancellationToken = default)
    {
        Ui.Current.AppendUpdaterOutput("\nMobile notification sent. Answer \"OK\" once you've authorized this login. If no notification was received or you'd like to enter a traditional 2FA code, press \"Cancel\": ");

        bool authorized = await _updater.SteamGuardInputPhone();

        Ui.Current.AppendUpdaterOutput("\n\tAuth : Authorizing...");

        return authorized;
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
