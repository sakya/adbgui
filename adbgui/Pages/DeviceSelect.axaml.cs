using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.SingleWindow.Abstracts;

namespace adbgui.Pages;

public partial class DeviceSelect : BasePage
{
    public DeviceSelect()
    {
        InitializeComponent();
    }

    private async void OnRefreshClick(object? sender, RoutedEventArgs e)
    {
        List.Items = null;
        var devices = await Adb.Adb.Instance!.ListDevices();
        List.Items = devices;
    }

    private void OnListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        OkBtn.IsEnabled = List.SelectedItems.Count > 0;
    }
}