using System.Collections.Generic;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

using FASTER.Services;

namespace FASTER.Avalonia.Services;

internal sealed class AvaDialogService : IDialogService
{
    private readonly Window _owner;
    public AvaDialogService(Window owner) => _owner = owner;

    public Task<string?> ShowInputAsync(object context, string title, string message)
        => OnUiAsync(async () =>
        {
            _owner.Activate();
            var dialog = new InputDialog { Title = title, Message = message };
            return await dialog.ShowDialog<string?>(_owner);
        });

    public Task<bool> ShowConfirmationAsync(object context, string title, string message)
        => OnUiAsync(async () =>
        {
            _owner.Activate();
            var dialog = new ConfirmDialog { Title = title, Message = message };
            return await dialog.ShowDialog<bool>(_owner);
        });

    public Task ShowMessageAsync(object context, string title, string message)
        => OnUiAsync(async () =>
        {
            _owner.Activate();
            var dialog = new MessageDialog { Title = title, Message = message };
            await dialog.ShowDialog<bool>(_owner);
            return true;
        });

    public Task<IProgressDialog> ShowProgressAsync(object context, string title, string message)
        => OnUiAsync<IProgressDialog>(async () =>
        {
            var dialog = new ProgressDialog { Title = title };
            dialog.SetMessage(message);
            dialog.Show(_owner);
            await Task.CompletedTask;
            return dialog;
        });

    // SteamKit invokes auth callbacks on its own network thread, and Avalonia
    // windows must be created and shown on the UI thread. Calling ShowDialog
    // from a background thread never shows the popup, which left users staring
    // at the prompt text in the read-only console box with no way to answer.
    private static Task<T> OnUiAsync<T>(Func<Task<T>> fn)
    {
        if (Dispatcher.UIThread.CheckAccess())
            return fn();
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                tcs.SetResult(await fn().ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }
}

internal sealed class AvaClipboardService : IClipboardService
{
    private readonly Window _owner;
    public AvaClipboardService(Window owner) => _owner = owner;

    public Task SetTextAsync(string text)
    {
        if (Dispatcher.UIThread.CheckAccess())
            return CopyAsync();
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                await CopyAsync().ConfigureAwait(false);
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;

        async Task CopyAsync()
        {
            var clipboard = TopLevel.GetTopLevel(_owner)?.Clipboard;
            if (clipboard != null)
                await clipboard.SetTextAsync(text);
        }
    }
}

internal sealed class AvaFilePickerService : IFilePickerService
{
    private readonly Window _owner;
    public AvaFilePickerService(Window owner) => _owner = owner;

    private IStorageProvider Storage => _owner.StorageProvider;

    public async Task<string?> PickServerExecutableAsync()
    {
        var filter = Platform.Current.ServerExecutableExtensionFilter;
        var options = new FilePickerOpenOptions
        {
            Title = "Select the arma server executable",
            AllowMultiple = false
        };
        if (!string.IsNullOrEmpty(filter))
            options.FileTypeFilter = new List<FilePickerFileType>
            {
                new("Arma 3 Server Executable") { Patterns = new[] { "*" + filter } }
            };
        var files = await Storage.OpenFilePickerAsync(options);
        return files.Count > 0 ? files[0].TryGetLocalPath() : null;
    }

    public async Task<string?> PickModPresetFileAsync()
    {
        var files = await Storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select the Arma3 mod preset",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new("Arma 3 Mod Preset") { Patterns = new[] { "*.html" } }
            }
        });
        return files.Count > 0 ? files[0].TryGetLocalPath() : null;
    }

    public async Task<string?> PickFolderAsync(string current)
    {
        var folders = await Storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select the folder",
            AllowMultiple = false
        });
        return folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
    }
}
