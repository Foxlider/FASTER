namespace FASTER.Services;

public interface IDialogService
{
    Task<string?> ShowInputAsync(object context, string title, string message);
    Task<bool> ShowConfirmationAsync(object context, string title, string message);
    Task ShowMessageAsync(object context, string title, string message);
    Task<IProgressDialog> ShowProgressAsync(object context, string title, string message);
}

public interface IProgressDialog
{
    double Maximum { get; set; }
    void SetMessage(string message);
    void SetProgress(double value);
    Task CloseAsync();
}

public interface IClipboardService
{
    Task SetTextAsync(string text);
}

public interface IFilePickerService
{
    Task<string?> PickServerExecutableAsync();
    Task<string?> PickModPresetFileAsync();
    Task<string?> PickFolderAsync(string current);
}

public static class AppServices
{
    public static IDialogService Dialogs { get; set; } = new NullDialogService();
    public static IClipboardService Clipboard { get; set; } = new NullClipboardService();
    public static IFilePickerService Files { get; set; } = new NullFilePickerService();

    private sealed class NullDialogService : IDialogService
    {
        public Task<string?> ShowInputAsync(object context, string title, string message) => Task.FromResult<string?>(null);
        public Task<bool> ShowConfirmationAsync(object context, string title, string message) => Task.FromResult(false);
        public Task ShowMessageAsync(object context, string title, string message) => Task.CompletedTask;
        public Task<IProgressDialog> ShowProgressAsync(object context, string title, string message)
            => Task.FromResult<IProgressDialog>(new NullProgressDialog());

        private sealed class NullProgressDialog : IProgressDialog
        {
            public double Maximum { get; set; }
            public void SetMessage(string message) { }
            public void SetProgress(double value) { }
            public Task CloseAsync() => Task.CompletedTask;
        }
    }

    private sealed class NullClipboardService : IClipboardService
    {
        public Task SetTextAsync(string text) => Task.CompletedTask;
    }

    private sealed class NullFilePickerService : IFilePickerService
    {
        public Task<string?> PickServerExecutableAsync() => Task.FromResult<string?>(null);
        public Task<string?> PickModPresetFileAsync() => Task.FromResult<string?>(null);
        public Task<string?> PickFolderAsync(string current) => Task.FromResult<string?>(null);
    }
}
