using System.Threading.Tasks;
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

    public override void OnNavigatedTo(NavigationDirection direction)
    {
        base.OnNavigatedTo(direction);
        App.SelectedDevice = null;
        OnRefreshClick(null, new RoutedEventArgs());
    }

    public override Task<bool> OnNavigatingFrom(NavigationDirection direction)
    {
        List.ItemsSource = null;
        return Task.FromResult(true);
    }

    private async void OnRefreshClick(object? sender, RoutedEventArgs e)
    {
        RefreshBtn.IsEnabled = false;
        List.ItemsSource = null;
        OkBtn.IsEnabled = false;
        var devices = await Adb.Adb.Instance!.ListDevices();
        List.ItemsSource = devices;
        RefreshBtn.IsEnabled = true;
    }

    private void OnListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var dev = List.SelectedItems?.Count > 0 ? List.SelectedItems[0] as Device : null;
        OkBtn.IsEnabled = dev?.Authorized == true;
    }

    private async void OnOkClick(object? sender, RoutedEventArgs e)
    {
        if (List.SelectedItems == null)
            return;

        if (List.SelectedItems[0] is Device { Authorized: true } dev) {
            App.SelectedDevice = dev;
            await MainWindowBase.Instance.NavigateTo(new MainPage(dev));
        }
    }
}