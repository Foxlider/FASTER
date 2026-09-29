using MahApps.Metro.Controls.Dialogs;

using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

using System.Threading.Tasks;
using System.Windows;

namespace FASTER.Services
{
    internal sealed class WpfDialogService : IDialogService
    {
        private static IDialogCoordinator Coordinator => DialogCoordinator.Instance;

        public async Task<string?> ShowInputAsync(object context, string title, string message)
            => await Coordinator.ShowInputAsync(context, title, message);

        public async Task<bool> ShowConfirmationAsync(object context, string title, string message)
        {
            var response = await Coordinator.ShowMessageAsync(context, title, message, MessageDialogStyle.AffirmativeAndNegative);
            return response == MessageDialogResult.Affirmative;
        }

        public async Task ShowMessageAsync(object context, string title, string message)
            => await Coordinator.ShowMessageAsync(context, title, message);

        public async Task<IProgressDialog> ShowProgressAsync(object context, string title, string message)
        {
            var controller = await Coordinator.ShowProgressAsync(context, title, message);
            return new WpfProgressDialog(controller);
        }

        private sealed class WpfProgressDialog : IProgressDialog
        {
            private readonly ProgressDialogController _controller;
            public WpfProgressDialog(ProgressDialogController controller) => _controller = controller;
            public double Maximum { get => _controller.Maximum; set => _controller.Maximum = value; }
            public void SetMessage(string message) => _controller.SetMessage(message);
            public void SetProgress(double value) => _controller.SetProgress(value);
            public Task CloseAsync() => _controller.CloseAsync();
        }
    }

    internal sealed class WpfClipboardService : IClipboardService
    {
        public Task SetTextAsync(string text)
        {
            try
            { Clipboard.SetText(text); }
            catch (System.Runtime.InteropServices.COMException)
            {
                try
                { Clipboard.SetDataObject(text); }
                catch (System.Runtime.InteropServices.COMException) { }
            }
            return Task.CompletedTask;
        }
    }

    internal sealed class WpfFilePickerService : IFilePickerService
    {
        public Task<string?> PickServerExecutableAsync()
        {
            var dialog = new CommonOpenFileDialog
            {
                Title = "Select the arma server executable",
                IsFolderPicker = false,
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };
            var filter = Platform.Current.ServerExecutableExtensionFilter;
            if (!string.IsNullOrEmpty(filter))
                dialog.Filters.Add(new CommonFileDialogFilter("Arma 3 Server Executable", filter));

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return Task.FromResult<string?>(null);
            return Task.FromResult(dialog.FileName);
        }

        public Task<string?> PickModPresetFileAsync()
        {
            OpenFileDialog dialog = new() { Filter = "Arma 3 Mod Preset|*.html" };
            bool picked = dialog.ShowDialog().GetValueOrDefault();
            return Task.FromResult(picked ? dialog.FileName : null);
        }

        public Task<string?> PickFolderAsync(string current)
            => Task.FromResult(MainWindow.Instance.SelectFolder(current));
    }
}
