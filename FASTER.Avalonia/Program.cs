using Avalonia;

namespace FASTER.Avalonia;

internal static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Velopack.VelopackApp.Build().Run();
        }
        catch
        {
            // Unpackaged dev runs have nothing to apply; start normally.
        }

        AppDomain.CurrentDomain.UnhandledException += (_, e) => FASTER.Models.Logger.LogCritical(e.ExceptionObject.ToString() ?? "Unhandled exception");
        TaskScheduler.UnobservedTaskException += (_, e) => FASTER.Models.Logger.LogCritical(e.Exception.ToString());
        try { BuildAvaloniaApp().StartWithClassicDesktopLifetime(args); }
        catch (Exception ex) { FASTER.Models.Logger.LogCritical(ex.ToString()); throw; }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
}
