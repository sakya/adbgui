using System;
using System.Globalization;
using adbgui.Adb.Models;
using Avalonia.Data.Converters;

namespace adbgui.Converters;

public class PackageNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Package pkg) {
            if (!string.IsNullOrEmpty(pkg.Version))
                return $"{pkg.Name} ({pkg.Version})";
            return pkg.Name;
        }

        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}