using System.Linq;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Threading;
using Avalonia.Interactivity;

using FASTER.ViewModel;

namespace FASTER.Avalonia.Views;

public partial class UpdaterView : UserControl
{
    private SteamUpdaterViewModel ViewModel => (SteamUpdaterViewModel)DataContext!;

    private ScrollViewer? _consoleScroll;
    private bool _follow = true;

    public UpdaterView()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            PasswordBox.Text = ViewModel.GetPw() ?? string.Empty;
            var scroll = ConsoleOutput.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
            if (scroll != null && scroll != _consoleScroll)
            {
                if (_consoleScroll != null) _consoleScroll.ScrollChanged -= ConsoleScrolled;
                _consoleScroll = scroll;
                scroll.ScrollChanged += ConsoleScrolled;
            }
        };
        ConsoleOutput.TextChanged += (_, _) =>
        {
            if (_follow) Dispatcher.UIThread.Post(() =>
            {
                if (_follow) _consoleScroll?.ScrollToEnd();
            }, DispatcherPriority.Background);
        };
    }

    private void ConsoleScrolled(object? sender, ScrollChangedEventArgs e)
    {
        if (_consoleScroll == null || e.OffsetDelta.Y == 0) return;
        _follow = _consoleScroll.Offset.Y >= _consoleScroll.Extent.Height - _consoleScroll.Viewport.Height - 2;
    }

    private void UpdateCancel_Click(object? sender, RoutedEventArgs e) => ViewModel.UpdateCancelClick();

    private async void Update_Click(object? sender, RoutedEventArgs e) => await ViewModel.UpdateClick();

    private async void ServerDir_Click(object? sender, RoutedEventArgs e) => await ViewModel.ServerDirClick();

    private async void ModStagingDir_Click(object? sender, RoutedEventArgs e) => await ViewModel.ModStagingDirClick();

    private void PasswordBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox { IsFocused: true } box)
            ViewModel.PasswordChanged(box.Text ?? string.Empty);
    }

    private void PasswordReveal_Click(object? sender, RoutedEventArgs e)
    {
        bool revealed = PasswordBox.PasswordChar == '\0';
        PasswordBox.PasswordChar = revealed ? '*' : '\0';
        PasswordEye.Kind = revealed
            ? Material.Icons.MaterialIconKind.Eye
            : Material.Icons.MaterialIconKind.EyeOff;
    }

    private void ClientReset_OnClick(object? sender, RoutedEventArgs e) => ViewModel.SteamReset();

    private void ClientConnect_OnClick(object? sender, RoutedEventArgs e) => _ = ViewModel.SteamLogin();
}
