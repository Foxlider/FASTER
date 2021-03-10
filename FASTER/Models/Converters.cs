using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FASTER.Models
{
    [ValueConversion(typeof(long), typeof(string))]
    public class FolderSizeConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is long size)
            {
                double   fullSize = size;
                string[] sizes    = {" B", "KB", "MB", "GB", "TB"};
                var      order    = 0;
                while (fullSize >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    fullSize /= 1024.0;
                }

                // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
                // show a single decimal place, and no space.
                return $"{fullSize,7:F} {sizes[order],-2}";
            }
            else
            {
                Console.WriteLine("WAT");
            }

            return "0 B";
        }

        public object ConvertBack(object                           value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        { return null; }

    #endregion
    }


    public class NotBooleanToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? isVisible = value as bool?;
            if (parameter != null)
                isVisible = !isVisible;
            if (isVisible.HasValue && isVisible.Value)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public sealed class ValueConverterCollection : Collection<IValueConverter> { }
}
