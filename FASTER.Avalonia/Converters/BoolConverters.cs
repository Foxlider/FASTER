using System;
using System.Globalization;

using Avalonia.Data.Converters;

namespace FASTER.Avalonia.Converters;

public sealed class BoolNegationConverter : IValueConverter
{
    public static readonly BoolNegationConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;
}
