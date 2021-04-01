using ControlzEx.Theming;

using FASTER.Models;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

using System;
using System.Globalization;
using System.Management;
using System.Threading;
using System.Windows;

namespace FASTER
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AprilFoolsCheck();
            
            base.OnStartup(e);

            var countryCode = RegionInfo.CurrentRegion.TwoLetterISORegionName;
            var userID = AppCenter.GetInstallIdAsync();
            AppCenter.SetCountryCode(countryCode);
            AppCenter.SetUserId(Environment.UserName + Environment.MachineName + Environment.UserDomainName + userID);
            Analytics.SetEnabledAsync(true);
            AppCenter.Start("257a7dac-e53c-4bec-b672-b6b939ed5d1e", typeof(Analytics), typeof(Crashes));
        }

        private void AprilFoolsCheck()
        {
            if (DateTime.Today != new DateTime(DateTime.Today.Year, 4, 1)) return;

            var themeThread = new Thread(() =>
                {
                    var r      = new Random();
                    var themes = ThemeManager.Current.ColorSchemes;
                    while (true)
                    {
                        Dispatcher.BeginInvoke(new Action(() => ThemeManager.Current.ChangeTheme(Current, themes[r.Next(themes.Count)])));
                        Thread.Sleep(5000);
                    }
                    // ReSharper disable once FunctionNeverReturns
                })
                { IsBackground = true };

            themeThread.Start();
        }
    }
}
