using adbgui.Adb.Models;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.SingleWindow;
using Avalonia.SingleWindow.Abstracts;

namespace adbgui.Pages;

public partial class DeviceSelect : BasePage
{
    public DeviceSelect()
    {
        InitializeComponent();

        PageTitle = Localizer.Localizer.Instance["Title_DeviceSelect"];
    }

    private async void OnRefreshClick(object? sender, RoutedEventArgs e)
    {
        RefreshBtn.IsEnabled = false;
        List.Items = null;
        OkBtn.IsEnabled = false;
        var devices = await Adb.Adb.Instance!.ListDevices();
        List.Items = devices;
        RefreshBtn.IsEnabled = true;
    }

    private void OnListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        OkBtn.IsEnabled = List.SelectedItems.Count > 0;
    }

    private async void OnOkClick(object? sender, RoutedEventArgs e)
    {
        var dev = List.SelectedItems[0] as Device;
        await MainWindowBase.Instance.NavigateTo(new MainPage(dev));
    }
}