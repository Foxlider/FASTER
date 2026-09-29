using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using FASTER.Models;

namespace FASTER.Avalonia.Services;

internal static class Appearance
{
    public static readonly IReadOnlyDictionary<string, string> Accents = new Dictionary<string, string>
    {
        ["Blue"] = "#119EDA", ["Red"] = "#C42B1C", ["Green"] = "#168144",
        ["Purple"] = "#8354B5", ["Orange"] = "#BD5700", ["Teal"] = "#008575"
    };
    public static string AppliedFont { get; private set; } = "Segoe UI";
    public static string[] Fonts => FontManager.Current.SystemFonts.Select(f => f.Name).Distinct().Order().ToArray();
    public static void Apply()
    {
        var app = Application.Current;
        if (app == null) return;
        var settings = AppSettings.Current;
        var theme = settings.Theme.Split('.');
        app.RequestedThemeVariant = theme[0].Equals("Light", StringComparison.OrdinalIgnoreCase) ? ThemeVariant.Light : ThemeVariant.Dark;
        var accent = theme.Length > 1 && Accents.TryGetValue(theme[1], out var color) ? color : Accents["Blue"];
        var parsed = Color.Parse(accent);
        foreach (var key in new[] { "SystemAccentColor", "SystemAccentColorLight1", "SystemAccentColorDark1" }) app.Resources[key] = parsed;
        foreach (var key in new[] { "FasterAccent", "FasterAccentHover", "FasterAccentPressed" }) app.Resources[key] = new SolidColorBrush(parsed);
        var fonts = Fonts;
        string font = fonts.FirstOrDefault(f => f.Equals(settings.Font, StringComparison.OrdinalIgnoreCase))
            ?? new[] { "Segoe UI", "Noto Sans", "DejaVu Sans" }.FirstOrDefault(fonts.Contains) ?? FontFamily.Default.Name;
        AppliedFont = font;
        app.Resources["FasterFont"] = new FontFamily(font);
    }
}
