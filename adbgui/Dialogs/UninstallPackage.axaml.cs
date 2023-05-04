using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.SingleWindow.Abstracts;
using Avalonia.Threading;

namespace adbgui.Dialogs;

public partial class UninstallPackage : BaseDialog
{
    private CancellationTokenSource? _tokenSource;
    private readonly StringBuilder _log = new();

    public UninstallPackage()
    {
        InitializeComponent();

        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;
    }

    public string[]? PackageNames { get; set; }

    protected override void Opened()
    {
        base.Opened();

        Log.Text = string.Empty;
        if (PackageNames?.Length > 0) {
            _tokenSource = new CancellationTokenSource();
            Task.Run(() => Uninstall(_tokenSource.Token));
        }
    }


    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        if (_tokenSource != null) {
            _tokenSource.Cancel();
        } else {
            Close();
        }
    }

    private async void Uninstall(CancellationToken token)
    {
        if (PackageNames == null)
            return;

        foreach (var pkg in PackageNames) {
            if (token.IsCancellationRequested) {
                _log.AppendLine("User aborted");
                UpdateLog();
                break;
            }

            _log.AppendLine($"Uninstalling {pkg}");
            UpdateLog();

            var res = await Adb.Adb.Instance!.UninstallPackage(App.SelectedDevice!.Id, pkg);
            _log.AppendLine(res.Output);
            _log.AppendLine(res.Result ? "SUCCESS" : "FAILED");
            _log.AppendLine();
            UpdateLog();
        }
    }

    private void UpdateLog()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Log.Text = _log.ToString();
        });
    }
}