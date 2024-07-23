using System;
using System.Collections.Generic;
using adbgui.Adb.Models;
using adbgui.Dialogs;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.SingleWindow.Abstracts;
using Avalonia.Threading;

namespace adbgui.Pages;

public partial class FileManager : BasePage
{
    private List<FileSystemItem>? _files;
    private string _currentPath;
    private string _previousPath;

    public FileManager()
    {
        InitializeComponent();

        _currentPath = "/";
        PageTitle = Localizer.Localizer.Instance["FileManager"];
    }

    public override void OnNavigatedTo(NavigationDirection direction)
    {
        base.OnNavigatedTo(direction);
        Dispatcher.UIThread.Post(() => { OnRefreshClick(null, new RoutedEventArgs()); }, DispatcherPriority.ApplicationIdle);
    }

    private async void OnRefreshClick(object? sender, RoutedEventArgs e)
    {
        Spinner.IsVisible = true;
        List.ItemsSource = null;
        try {
            var files = await Adb.Adb.Instance!.ListDirectoryContent(App.SelectedDevice!.Id, _currentPath);
            _files = await Adb.Adb.Instance!.ListDirectoryContent(App.SelectedDevice!.Id, _currentPath);
        } catch (Exception ex) {
            if (!string.IsNullOrEmpty(_previousPath))
                _currentPath = _previousPath;
            var iDlg = new Error(ex);
            await iDlg.Show();
        }
        List.ItemsSource = _files;
        Spinner.IsVisible = false;
    }

    private void OnElementDoubleTapped(object? sender, TappedEventArgs e)
    {
        var ctrl = sender as Control;
        if (ctrl?.DataContext is FileSystemItem item) {
            if (item.Type == FileSystemItem.FileTypes.Directory) {
                _previousPath = _currentPath;
                _currentPath = JoinPath(_currentPath, item.Name);
                Dispatcher.UIThread.Post(() => { OnRefreshClick(null, new RoutedEventArgs()); }, DispatcherPriority.ApplicationIdle);
            }
        }
    }

    private string JoinPath(string part1, string part2)
    {
        if (string.IsNullOrEmpty(part1))
            return part2;

        if (part2 == ".")
            return part1;
        if (part2 == "..") {
            // To up one level
            if (part1.EndsWith("/"))
                part1 = part1.Remove(part1.Length - 1);
            var idx = part1.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase);
            if (idx >= 0) {
                return part1.Substring(0, idx + 1);
            }
        }

        if (!part1.EndsWith("/"))
            part1 = $"{part1}/";
        return $"{part1}{part2}";
    }
}