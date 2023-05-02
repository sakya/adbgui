using adbgui.Adb.Models;
using Avalonia.Interactivity;
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

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_device != null) {
            await Adb.Adb.Instance!.ListPackages(_device.Id);
        }
    }
}