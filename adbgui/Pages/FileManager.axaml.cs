using System.Collections.Generic;
using adbgui.Adb.Models;
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
        _files = await Adb.Adb.Instance!.ListDirectoryContent(App.SelectedDevice!.Id, _currentPath);
        List.ItemsSource = _files;
        Spinner.IsVisible = false;
    }

    private void OnElementDoubleTapped(object? sender, TappedEventArgs e)
    {
        var ctrl = sender as Control;
        if (ctrl?.DataContext is FileSystemItem item) {
            if (item.Type == FileSystemItem.FileTypes.Directory) {
                if (!_currentPath.EndsWith("/"))
                    _currentPath = $"{_currentPath}/";
                _currentPath = $"{_currentPath}{item.Name}";
                Dispatcher.UIThread.Post(() => { OnRefreshClick(null, new RoutedEventArgs()); }, DispatcherPriority.ApplicationIdle);
            }
        }
    }
}