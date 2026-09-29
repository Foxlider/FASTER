using FASTER.Models;

namespace FASTER.Services;

public interface IUiBridge
{
    void DisplayMessage(string message);
    void NavigateToConsole();
    Task<int> RunModUpdaterAsync(ulong workshopId, string path);
    bool IsUiLoaded();
    void SyncProfileMenuName(string profileId, string name);
    void ReloadServerProfiles();
    void AppendUpdaterOutput(string text);
    Task<string> GetSteamGuardCodeAsync(string prompt);
    Task<bool> ConfirmPhoneAuthAsync();
}

public static class Ui
{
    public static IUiBridge Current { get; set; } = new NullUiBridge();

    private sealed class NullUiBridge : IUiBridge
    {
        public void DisplayMessage(string message) => Console.WriteLine(message);
        public void NavigateToConsole() { }
        public Task<int> RunModUpdaterAsync(ulong workshopId, string path) => Task.FromResult(UpdateState.Error);
        public bool IsUiLoaded() => false;
        public void SyncProfileMenuName(string profileId, string name) { }
        public void ReloadServerProfiles() { }
        public void AppendUpdaterOutput(string text) => Console.WriteLine(text);
        public Task<string> GetSteamGuardCodeAsync(string prompt) => Task.FromResult(string.Empty);
        public Task<bool> ConfirmPhoneAuthAsync() => Task.FromResult(false);
    }
}
