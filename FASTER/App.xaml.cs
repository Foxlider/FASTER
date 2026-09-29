using FASTER.Models;

using System.Windows;
using ControlzEx.Theming;

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

            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
            ThemeManager.Current.ChangeTheme(Current, AppSettings.Current.Theme);
        }
    }
}
