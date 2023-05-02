using Avalonia.Interactivity;
using Avalonia.SingleWindow.Abstracts;

namespace adbgui.Pages;

public partial class MainPage : BasePage
{
    public MainPage()
    {
        InitializeComponent();

        Adb.Adb.Instance = new Adb.Adb(@"c:\Users\paolo.iommarini\Downloads\ADB\adb.exe");
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        await Adb.Adb.Instance!.GetVersion();
        var dev = await Adb.Adb.Instance!.ListDevices();
        if (dev.Count > 0) {
            await Adb.Adb.Instance!.ListPackages(dev[0].Id);
        }
    }
}