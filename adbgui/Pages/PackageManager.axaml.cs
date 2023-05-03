using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using adbgui.Adb.Models;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.SingleWindow.Abstracts;

namespace adbgui.Pages;

public partial class PackageManager : BasePage
{
    private List<Package>? _allPackages;

    public PackageManager()
    {
        InitializeComponent();

        PageTitle = Localizer.Localizer.Instance["PackageManager"];
    }

    public override void OnNavigatedTo(NavigationDirection direction)
    {
        base.OnNavigatedTo(direction);
        OnRefreshClick(null, new RoutedEventArgs());
    }

    private async void OnRefreshClick(object? sender, RoutedEventArgs e)
    {
        List.Items = null;
        _allPackages = await Adb.Adb.Instance!.ListPackages(App.SelectedDevice!.Id);
        FilterPackages();
    }

    private void OnSearchClick(object? sender, RoutedEventArgs e)
    {
        SearchBtn.IsEnabled = false;
        FilterPackages();
        SearchBtn.IsEnabled = true;
    }

    private void OnSearchTextKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return)
            OnSearchClick(null, new RoutedEventArgs());
    }

    private void FilterPackages()
    {
        var filtered = _allPackages;
        if (filtered != null && !string.IsNullOrEmpty(SearchText.Text)) {
            filtered = filtered
                .Where(p => p.Name != null && p.Name.Contains(SearchText.Text, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
        }
        List.Items = filtered?.OrderBy(p => p.Name);;
    }
}