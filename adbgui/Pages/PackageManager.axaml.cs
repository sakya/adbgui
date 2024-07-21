using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using adbgui.Adb.Models;
using adbgui.Dialogs;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.SingleWindow;
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
        Spinner.IsVisible = true;
        List.ItemsSource = null;
        _allPackages = await Adb.Adb.Instance!.ListPackages(App.SelectedDevice!.Id);
        FilterPackages();
        Spinner.IsVisible = false;
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
        List.ItemsSource = filtered?.OrderBy(p => p.Name);;
    }

    private void OnListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        EnableBtn.IsEnabled = List.SelectedItems?.Count > 0;
        DisableBtn.IsEnabled = List.SelectedItems?.Count > 0;
        UninstallBtn.IsEnabled = List.SelectedItems?.Count > 0;
    }

    private async void OnInstallBtnClick(object? sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog();
        dlg.Filters = new List<FileDialogFilter>()
        {
            new()
            {
                Name = "apk",
                Extensions = new List<string>() { "apk" }
            }

        };

        var files = await dlg.ShowAsync(MainWindowBase.Instance);
        if (files?.Length > 0) {
            var iDlg = new InstallApk();
            iDlg.ApkFiles = files;

            await iDlg.Show();
        }
    }

    private async void OnUninstallBtnClick(object? sender, RoutedEventArgs e)
    {
        if (List.SelectedItems == null || List.SelectedItems.Count == 0)
            return;

        var dlg = new PackageOperations();
        var pkgs = new List<string>();
        foreach (var si in List.SelectedItems) {
            if (si is Package pkg)
                pkgs.Add(pkg.Name!);
        }

        dlg.Operation = PackageOperations.Operations.Uninstall;
        dlg.PackageNames = pkgs.ToArray();
        await dlg.Show();
    }

    private async void OnDisableClick(object? sender, RoutedEventArgs e)
    {
        if (List.SelectedItems == null || List.SelectedItems.Count == 0)
            return;

        var dlg = new PackageOperations();
        var pkgs = new List<string>();
        foreach (var si in List.SelectedItems) {
            if (si is Package pkg)
                pkgs.Add(pkg.Name!);
        }

        dlg.Operation = PackageOperations.Operations.Disable;
        dlg.PackageNames = pkgs.ToArray();
        await dlg.Show();
    }

    private async void OnEnableClick(object? sender, RoutedEventArgs e)
    {
        if (List.SelectedItems == null || List.SelectedItems.Count == 0)
            return;

        var dlg = new PackageOperations();
        var pkgs = new List<string>();
        foreach (var si in List.SelectedItems) {
            if (si is Package pkg)
                pkgs.Add(pkg.Name!);
        }

        dlg.Operation = PackageOperations.Operations.Enable;
        dlg.PackageNames = pkgs.ToArray();
        await dlg.Show();
    }
}