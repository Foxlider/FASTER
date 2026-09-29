using FASTER.Models;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

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

            var countryCode = RegionInfo.CurrentRegion.TwoLetterISORegionName;
            var userID = await AppCenter.GetInstallIdAsync();

            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
            ThemeManager.Current.ChangeTheme(Current, AppSettings.Current.Theme);

            var analyticsEnabled = AppSettings.Current.EnableAnalytics;
            _ = Analytics.SetEnabledAsync(analyticsEnabled);
            _ = Crashes.SetEnabledAsync(analyticsEnabled);
            if (!analyticsEnabled)
                return;

            AppCenter.SetCountryCode(countryCode);
            AppCenter.SetUserId($"{Environment.UserName}_{Environment.MachineName}_{Environment.UserDomainName}_{userID}");
            AppCenter.Start("257a7dac-e53c-4bec-b672-b6b939ed5d1e", typeof(Analytics), typeof(Crashes));
        }
    }
}
