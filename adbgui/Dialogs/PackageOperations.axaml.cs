using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using adbgui.Adb.Models;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.SingleWindow.Abstracts;
using Avalonia.Threading;

namespace adbgui.Dialogs;

public partial class PackageOperations : BaseDialog
{
    private readonly CancellationTokenSource _tokenSource = new();
    private bool _running;
    private readonly StringBuilder _log = new();

    public enum Operations
    {
        Disable,
        Enable,
        Uninstall
    }

    public PackageOperations()
    {
        InitializeComponent();

        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;

        Closing += OnClosing;
    }

    public Operations Operation { get; set; }
    public string[]? PackageNames { get; set; }

    protected override void Opened()
    {
        base.Opened();

        Log.Text = string.Empty;
        if (PackageNames?.Length > 0) {
            Task.Run(() => Uninstall(_tokenSource.Token));
        }
    }


    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        if (_running) {
            _tokenSource.Cancel();
        } else {
            Close();
        }
    }

    private async void Uninstall(CancellationToken token)
    {
        if (PackageNames == null)
            return;

        _running = true;
        foreach (var pkg in PackageNames) {
            if (token.IsCancellationRequested) {
                _log.AppendLine("User aborted");
                UpdateLog();
                break;
            }

            PackageOperationResult? res = null;
            switch (Operation) {
                case Operations.Enable:
                    _log.AppendLine($"Enabling {pkg}");
                    UpdateLog();
                    res = await Adb.Adb.Instance!.EnablePackage(App.SelectedDevice!.Id, pkg);
                    break;
                case Operations.Disable:
                    _log.AppendLine($"Disabling {pkg}");
                    UpdateLog();
                    res = await Adb.Adb.Instance!.DisablePackage(App.SelectedDevice!.Id, pkg);
                    break;
                case Operations.Uninstall:
                    _log.AppendLine($"Uninstalling {pkg}");
                    UpdateLog();
                    res = await Adb.Adb.Instance!.UninstallPackage(App.SelectedDevice!.Id, pkg);
                    break;
            }

            if (res != null) {
                if (!string.IsNullOrEmpty(res.Output))
                    _log.AppendLine(res.Output);
                if (!string.IsNullOrEmpty(res.Error))
                    _log.AppendLine(res.Error);

                _log.AppendLine(res.Result ? "SUCCESS" : "FAILED");
                _log.AppendLine();
                UpdateLog();
            }
        }

        _running = false;
    }

    private void UpdateLog()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Log.Text = _log.ToString();
        });
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_running)
            e.Cancel = true;
        else
            _tokenSource.Dispose();
    }
}