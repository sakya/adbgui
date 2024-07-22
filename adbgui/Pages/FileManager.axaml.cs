using System.Collections.Generic;
using adbgui.Adb.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
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
        //Spinner.IsVisible = true;
        //List.ItemsSource = null;
        _files = await Adb.Adb.Instance!.ListDirectoryContent(App.SelectedDevice!.Id, _currentPath);
        //Spinner.IsVisible = false;
    }
}