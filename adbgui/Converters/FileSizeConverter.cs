using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace adbgui.Converters
{
    public class FileSizeConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is long size) {
                return FormatSize(size);
            }
            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static string FormatSize(long size)
        {
            if (size < 0)
                size = 0;

            if (size / (1024.0 * 1024.0 * 1024.0) > 0.5)
                return Math.Round(size / (1024.0 * 1024.0 * 1024.0), 1).ToString("0.# Gib");
            if (size / (1024.0 * 1024.0) > 0.5)
                return Math.Round(size / (1024.0 * 1024.0), 1).ToString("0.# Mib");
            if (size / 1024.0 > 0.5)
                return Math.Round(size / 1024.0, 1).ToString("0.# Kib");
            return size.ToString("0 bytes");
        } // FormatSize
    }
}
