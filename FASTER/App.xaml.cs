using FASTER.Models;


using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using ControlzEx.Theming;

namespace FASTER
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnExit(ExitEventArgs e)
        {
            FASTER.Services.AppServices.Processes.Dispose();
            base.OnExit(e);
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
                Logger.LogCritical($"[FATAL] Unhandled exception (CLR): {args.ExceptionObject}");

            DispatcherUnhandledException += (_, args) =>
            {
                Logger.LogCritical($"[FATAL] Unhandled dispatcher exception: {args.Exception}");
                args.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                Logger.LogCritical($"[FATAL] Unobserved task exception: {args.Exception}");
                args.SetObserved();
            };

            AppSettings.Current.InitializeForStartup();
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
            ThemeManager.Current.ChangeTheme(Current, AppSettings.Current.Theme);
            await FASTER.Services.Telemetry.SetEnabledAsync(AppSettings.Current.EnableAnalytics);
        }
    }
}
