using adbgui.Adb.Models;
using Avalonia.Interactivity;
using Avalonia.SingleWindow;
using Avalonia.SingleWindow.Abstracts;

namespace adbgui.Pages;

public partial class MainPage : BasePage
{
    private Device? _device;
    public MainPage()
    {
        InitializeComponent();
    }

    public MainPage(Device? dev)
    {
        InitializeComponent();

        _device = dev;
    }

    private async void OnPackagaManagerClick(object? sender, RoutedEventArgs e)
    {
        await MainWindowBase.Instance.NavigateTo(new PackageManager());
    }

    private async void OnFileManagerClick(object? sender, RoutedEventArgs e)
    {
        await MainWindowBase.Instance.NavigateTo(new FileManager());
    }
}