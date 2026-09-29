using System;
using System.Linq;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace FASTER.Avalonia;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();

            // Spike harness: --smoke shows the window, then exits on its own.
            if (desktop.Args.Contains("--smoke", StringComparer.Ordinal))
            {
                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
                timer.Tick += (_, _) =>
                {
                    timer.Stop();
                    Console.WriteLine("SMOKE-OK");
                    desktop.Shutdown();
                };
                timer.Start();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
