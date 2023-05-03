using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.SingleWindow.Abstracts;

namespace adbgui.Pages;

public partial class PackageManager : BasePage
{
    public PackageManager()
    {
        InitializeComponent();
    }

    public override void OnNavigatedTo(NavigationDirection direction)
    {
        base.OnNavigatedTo(direction);
        OnRefreshClick(null, new RoutedEventArgs());
    }

    private async void OnRefreshClick(object? sender, RoutedEventArgs e)
    {
        List.Items = null;
        var pkgs = await Adb.Adb.Instance!.ListPackages(App.SelectedDevice!.Id);
        List.Items = pkgs.OrderBy(p => p.Name);

    }
}