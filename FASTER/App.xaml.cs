
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

using System;
using System.Globalization;
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
            base.OnStartup(e);

            var countryCode = RegionInfo.CurrentRegion.TwoLetterISORegionName;
            var userID = AppCenter.GetInstallIdAsync();
            AppCenter.SetCountryCode(countryCode);
            AppCenter.SetUserId(Environment.UserName + Environment.MachineName + Environment.UserDomainName + userID);
            Analytics.SetEnabledAsync(true);
            AppCenter.Start("257a7dac-e53c-4bec-b672-b6b939ed5d1e", typeof(Analytics), typeof(Crashes));
        }
    }
}
