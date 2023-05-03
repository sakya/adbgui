using System;
using System.Globalization;
using adbgui.Adb.Models;
using Avalonia.Data.Converters;

namespace adbgui.Converters;

public class PackageIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Package pkg) {
            if (!pkg.Enabled)
                return "fas fa-ban";
        }

        return "fas fa-cube";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}