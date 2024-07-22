using System;
using System.Globalization;
using adbgui.Adb.Models;
using Avalonia.Data.Converters;

namespace adbgui.Converters;

public class FileSystemItemIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FileSystemItem item) {
            return item.Type switch
            {
                FileSystemItem.FileTypes.Directory => "fas fa-folder",
                FileSystemItem.FileTypes.File => "fas fa-file",
                FileSystemItem.FileTypes.Symlink => "fas fa-link",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return "fas fa-file";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}