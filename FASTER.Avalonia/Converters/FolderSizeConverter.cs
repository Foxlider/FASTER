using System;
using System.Globalization;

using Avalonia.Data.Converters;

namespace FASTER.Avalonia.Converters;

public sealed class FolderSizeConverter : IValueConverter
{
    public static readonly FolderSizeConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is long size)
        {
            double fullSize = size;
            string[] sizes = [" B", "KB", "MB", "GB", "TB"];
            var order = 0;
            while (fullSize >= 1024 && order < sizes.Length - 1)
            {
                order++;
                fullSize /= 1024.0;
            }
            return $"{fullSize,7:F} {sizes[order],-2}";
        }
        return "0 B";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
