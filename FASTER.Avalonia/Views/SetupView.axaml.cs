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
    public SetupView()
    {
        InitializeComponent();
        Loaded += (_, _) => InitializeDefaults();
    }

    private void InitializeDefaults()
    {
        try
        {
            if (AppSettings.Current.FirstRun)
            {
                AppSettings.Current.Upgrade();
                AppSettings.Current.FirstRun = false;
                AppSettings.Current.Save();
            }

            AppSettings.Current.SteamMods ??= new SteamModCollection();
            AppSettings.Current.LocalMods ??= new List<LocalMod>();
            AppSettings.Current.LocalModFolders ??= new List<string>();
            AppSettings.Current.ArmaMods ??= new ArmaModCollection();

            if (string.IsNullOrEmpty(AppSettings.Current.ModStagingDirectory))
            {
                AppSettings.Current.ModStagingDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ModStagingDirectory");
                AppSettings.Current.Save();
            }

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

    private void Continue_Click(object? sender, RoutedEventArgs e)
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
        settings.SteamPassword = Encryption.Instance.EncryptData(SteamPassBox.Text ?? string.Empty) ?? string.Empty;
        if (!string.IsNullOrEmpty(ApiKeyBox.Text))
            settings.SteamAPIKey = ApiKeyBox.Text;
        settings.FirstRun = false;
        settings.Save();

        if (DataContext is MainViewModel main)
        {
            main.Updater.Parameters.ModStagingDirectory = settings.ModStagingDirectory;
            main.Updater.Parameters.ApiKey = settings.SteamAPIKey;
            main.Updater.Parameters.InstallDirectory = settings.ServerPath;
            main.Updater.Parameters.Username = settings.SteamUserName;
            main.Updater.Parameters.Password = settings.SteamPassword;
            main.ShowUpdater();
        }
    }
}
