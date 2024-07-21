using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using adbgui.Adb.Models;
using adbgui.Dialogs;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
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
        TxtStatus.Text = string.Empty;
        if (filtered == null)
            return;

        if (ChkShowDisabled.IsChecked == false) {
            filtered = filtered.Where(p => p.Enabled)
                .ToList();
        }
        if (ChkShowSystem.IsChecked == false) {
            filtered = filtered.Where(p => !p.System)
                .ToList();
        }
        if (ChkShowThirdParty.IsChecked == false) {
            filtered = filtered.Where(p => !p.ThirdParty)
                .ToList();
        }

        if (!string.IsNullOrEmpty(SearchText.Text)) {
            filtered = filtered
                .Where(p => p.Name != null && p.Name.Contains(SearchText.Text, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
        }
        List.ItemsSource = filtered.OrderBy(p => p.Name);

        TxtStatus.Text = $"Packages: {filtered.Count}";
    }

    private void OnListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        DownloadBtn.IsEnabled = List.SelectedItems?.Count > 0;
        EnableBtn.IsEnabled = List.SelectedItems?.Count > 0;
        DisableBtn.IsEnabled = List.SelectedItems?.Count > 0;
        UninstallBtn.IsEnabled = List.SelectedItems?.Count > 0;
    }

    private async void OnInstallBtnClick(object? sender, RoutedEventArgs e)
    {
        var files = await MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            AllowMultiple = false,
            FileTypeFilter = new []
            {
                new FilePickerFileType("Android Package File")
                {
                    Patterns = new []{ "*.apk" }
                },
            }
        });
        if (files.Count <= 0)
            return;

        var iDlg = new InstallApk();
        iDlg.ApkFiles = files.Select(f =>  HttpUtility.UrlDecode(f.Path.AbsolutePath)).ToArray();

        await iDlg.Show();
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
        OnRefreshClick(null, new RoutedEventArgs());
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
        OnRefreshClick(null, new RoutedEventArgs());
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
        OnRefreshClick(null, new RoutedEventArgs());
    }

    private async void OnDownloadClick(object? sender, RoutedEventArgs e)
    {
        if (List.SelectedItems == null || List.SelectedItems.Count == 0)
            return;

        var dlg = new PackageOperations();
        var pkgs = new List<string>();
        foreach (var si in List.SelectedItems) {
            if (si is Package pkg)
                pkgs.Add(pkg.Name!);
        }

        dlg.Operation = PackageOperations.Operations.Download;
        dlg.PackageNames = pkgs.ToArray();
        await dlg.Show();
        OnRefreshClick(null, new RoutedEventArgs());
    }

    private void OnTypeFilterChanged(object? sender, RoutedEventArgs e)
    {
        FilterPackages();
    }
}