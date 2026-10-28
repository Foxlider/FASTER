using System;
using System.Collections.Generic;
using System.IO;

using Avalonia.Controls;
using Avalonia.Interactivity;

using FASTER.Avalonia.ViewModels;
using FASTER.Models;
using FASTER.Services;

namespace FASTER.Avalonia.Views;

public partial class SetupView : UserControl
{
    private string _loadedPassword = string.Empty;

    public SetupView()
    {
        InitializeComponent();
        Loaded += (_, _) => InitializeDefaults();
    }

    private void InitializeDefaults()
    {
        try
        {
            if (string.IsNullOrEmpty(AppSettings.Current.ModStagingDirectory))
            {
                AppSettings.Current.ModStagingDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ModStagingDirectory");
                AppSettings.Current.Save();
            }

            _loadedPassword = Encryption.Instance.DecryptData(AppSettings.Current.SteamPassword) ?? string.Empty;
            SteamPassBox.Text = _loadedPassword;
            ApiKeyBox.Text = AppSettings.Current.SteamAPIKey;
            SteamUserBox.Text = AppSettings.Current.SteamUserName;
            ModStagingBox.Text = AppSettings.Current.ModStagingDirectory;
            ServerDirBox.Text = AppSettings.Current.ServerPath;
        }
        catch (Exception e)
        {
            MessageText.Text = "Could not read your configuration file: " + e.Message;
        }
    }

    private async void ServerDirButton_Click(object? sender, RoutedEventArgs e)
    {
        string? path = await AppServices.Files.PickFolderAsync(ServerDirBox.Text ?? string.Empty);
        if (!string.IsNullOrEmpty(path))
            ServerDirBox.Text = path;
    }

    private async void ModStagingButton_Click(object? sender, RoutedEventArgs e)
    {
        string? path = await AppServices.Files.PickFolderAsync(ModStagingBox.Text ?? string.Empty);
        if (!string.IsNullOrEmpty(path))
            ModStagingBox.Text = path;
    }

    private async void Continue_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(ModStagingBox.Text))
        {
            MessageText.Text = "Please enter a valid Mod Staging Directory";
            return;
        }

        if (string.IsNullOrEmpty(ServerDirBox.Text) || !Directory.Exists(ServerDirBox.Text))
        {
            MessageText.Text = "Please enter a valid Arma Server Directory";
            return;
        }

        var settings = AppSettings.Current;
        settings.ServerPath = ServerDirBox.Text ?? string.Empty;
        settings.ModStagingDirectory = ModStagingBox.Text ?? string.Empty;
        settings.SteamUserName = SteamUserBox.Text ?? string.Empty;
        settings.SteamAPIKey = ApiKeyBox.Text ?? string.Empty;
        settings.CompleteSetup(SteamPassBox.Text ?? string.Empty,
            (SteamPassBox.Text ?? string.Empty) != _loadedPassword);

        if (DataContext is MainViewModel main)
        {
            main.Updater.Parameters.ModStagingDirectory = settings.ModStagingDirectory;
            main.Updater.Parameters.ApiKey = settings.SteamAPIKey;
            main.Updater.Parameters.InstallDirectory = settings.ServerPath;
            main.Updater.Parameters.Username = settings.SteamUserName;
            main.Updater.Parameters.Password = settings.SteamPassword;
            main.ShowUpdater();
            await main.RunStartupChecksAsync();
        }
    }
}
