using System;
using System.Linq;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using FASTER.Avalonia.ViewModels;

namespace FASTER.Avalonia;

public partial class App : Application
{
    public static MainViewModel Main { get; } = new MainViewModel();

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new MainWindow { DataContext = Main };
            Main.Attach(window);
            desktop.MainWindow = window;

            if (desktop.Args.Contains("--smoke", StringComparer.Ordinal))
            {
                SmokeTour(Main, desktop);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void SmokeTour(MainViewModel main, IClassicDesktopStyleApplicationLifetime desktop)
    {
        var tour = new object[]
        {
            main.SetupView,
            main.UpdaterView,
            main.ModsView,
            main.DeploymentView,
            main.ServerStatusView,
            main.SettingsView,
            main.AboutView,
            new Views.ProfileView
            {
                DataContext = new FASTER.ViewModel.ProfileViewModel(
                    new FASTER.Models.ServerProfile("smoke", createFolder: false))
            }
        };
        int step = 0;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        timer.Tick += (_, _) =>
        {
            if (step < tour.Length)
            {
                main.CurrentView = tour[step];
                Console.WriteLine($"SMOKE-VIEW {step}");
                step++;
                return;
            }
            timer.Stop();
            Console.WriteLine("SMOKE-OK");
            desktop.Shutdown();
        };
        timer.Start();
    }
}
