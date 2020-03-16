using MahApps.Metro;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.Globalization;
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
            AppCenter.SetCountryCode(countryCode);

            AppCenter.Start("257a7dac-e53c-4bec-b672-b6b939ed5d1e", typeof(Analytics), typeof(Crashes));
        }

        private void AprilFoolsCheck()
        {
            if (DateTime.Today != new DateTime(DateTime.Today.Year, 4, 1)) return;

            var themeThread = new Thread(() =>
                {
                    var r      = new Random();
                    var themes = ThemeManager.Themes;
                    while (true)
                    {
                        Dispatcher.BeginInvoke(new Action(() => ThemeManager.ChangeTheme(Current, themes[r.Next(themes.Count)])));
                        Thread.Sleep(5000);
                    }
                })
                { IsBackground = true };

            themeThread.Start();
        }
    }
}
