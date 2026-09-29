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

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
}
