using Avalonia.Controls;
using Avalonia.Interactivity;

using FASTER.ViewModel;

namespace FASTER.Avalonia.Views;

public partial class ProfileView : UserControl
{
    private ProfileViewModel ViewModel => (ProfileViewModel)DataContext!;

    public ProfileView()
    {
        InitializeComponent();
        Unloaded += (_, _) => ViewModel.UnloadData();
    }

    private void ClearModOrder(object? sender, RoutedEventArgs e) => ViewModel.ClearModOrder();

    private void CopyFromClientServer(object? sender, RoutedEventArgs e) => ViewModel.ModsCopyFrom("Server Only", "Server + Client");
    private void CopyFromHeadlessServer(object? sender, RoutedEventArgs e) => ViewModel.ModsCopyFrom("Server Only", "HC");
    private void CopyFromOptServer(object? sender, RoutedEventArgs e) => ViewModel.ModsCopyFrom("Server Only", "Opt");
    private void CopyFromServerClient(object? sender, RoutedEventArgs e) => ViewModel.ModsCopyFrom("Server + Client", "Server Only");
    private void CopyFromHeadlessClient(object? sender, RoutedEventArgs e) => ViewModel.ModsCopyFrom("Server + Client", "HC");
    private void CopyFromOptClient(object? sender, RoutedEventArgs e) => ViewModel.ModsCopyFrom("Server + Client", "Opt");
    private void CopyFromServerHc(object? sender, RoutedEventArgs e) => ViewModel.ModsCopyFrom("HC", "Server Only");
    private void CopyFromClientHc(object? sender, RoutedEventArgs e) => ViewModel.ModsCopyFrom("HC", "Server + Client");
    private void CopyFromOptHc(object? sender, RoutedEventArgs e) => ViewModel.ModsCopyFrom("HC", "Opt");
    private void CopyFromServerOpt(object? sender, RoutedEventArgs e) => ViewModel.ModsCopyFrom("Opt", "Server Only");
    private void CopyFromClientOpt(object? sender, RoutedEventArgs e) => ViewModel.ModsCopyFrom("Opt", "Server + Client");
    private void CopyFromHeadlessOpt(object? sender, RoutedEventArgs e) => ViewModel.ModsCopyFrom("Opt", "HC");

    private void ModsSelectAllServer(object? sender, RoutedEventArgs e) => ViewModel.ModsSelectAll("Server Only", true);
    private void ModsSelectNoneServer(object? sender, RoutedEventArgs e) => ViewModel.ModsSelectAll("Server Only", false);
    private void ModsSelectAllClient(object? sender, RoutedEventArgs e) => ViewModel.ModsSelectAll("Server + Client", true);
    private void ModsSelectNoneClient(object? sender, RoutedEventArgs e) => ViewModel.ModsSelectAll("Server + Client", false);
    private void ModsSelectAllHc(object? sender, RoutedEventArgs e) => ViewModel.ModsSelectAll("HC", true);
    private void ModsSelectNoneHc(object? sender, RoutedEventArgs e) => ViewModel.ModsSelectAll("HC", false);
    private void ModsSelectAllOpt(object? sender, RoutedEventArgs e) => ViewModel.ModsSelectAll("Opt", true);
    private void ModsSelectNoneOpt(object? sender, RoutedEventArgs e) => ViewModel.ModsSelectAll("Opt", false);

    private void MissionSelectAll(object? sender, RoutedEventArgs e) => ViewModel.MissionSelectAll(true);
    private void MissionSelectNone(object? sender, RoutedEventArgs e) => ViewModel.MissionSelectAll(false);

    private void MissionRefresh(object? sender, RoutedEventArgs e) => ViewModel.LoadMissions();

    private async void LoadFromFile_Click(object? sender, RoutedEventArgs e) => await ViewModel.LoadModsFromFile();
    private async void CopyModKeys_Click(object? sender, RoutedEventArgs e) => await ViewModel.CopyModKeys();
    private async void ClearModKeys_Click(object? sender, RoutedEventArgs e) => await ViewModel.ClearModKeys();
    private async void SelectServerFile(object? sender, RoutedEventArgs e) => await ViewModel.SelectServerFile();
    private void OpenProfileLocation(object? sender, RoutedEventArgs e) => ViewModel.OpenProfileLocation();
    private void SaveProfile(object? sender, RoutedEventArgs e) => ViewModel.SaveProfile();
    private void DeleteProfile(object? sender, RoutedEventArgs e) => ViewModel.DeleteProfile();
    private void LaunchServer(object? sender, RoutedEventArgs e) => ViewModel.LaunchServer();
    private void LaunchHCs(object? sender, RoutedEventArgs e) => ViewModel.LaunchHCs();
}
