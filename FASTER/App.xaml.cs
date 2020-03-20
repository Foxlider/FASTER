using MahApps.Metro;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using FASTER.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

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

            TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
            configuration.InstrumentationKey = AppInsights.AzureInsightsKey;
            configuration.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());

            AppInsights.Client = new TelemetryClient(configuration);
            AppInsights.Client.Context.Component.Version = Functions.GetVersion();
            AppInsights.Client.Context.Device.Type = "PC";
            AppInsights.Client.Context.Device.Id = Environment.MachineName + Environment.UserDomainName;
            AppInsights.Client.Context.User.Id = Environment.MachineName + Environment.UserDomainName;
            try
            {
                using System.Management.ManagementObjectSearcher searcher =
                    new System.Management.ManagementObjectSearcher(
                        new System.Management.SelectQuery(@"Select * from Win32_ComputerSystem"));
                //execute the query
                foreach (System.Management.ManagementObject process in searcher.Get())
                {
                    process.Get();
                    AppInsights.Client.Context.Device.Model = process["Model"].ToString();
                }
            }
            catch
            { AppInsights.Client.Context.Device.Model = "Unknown"; }
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
