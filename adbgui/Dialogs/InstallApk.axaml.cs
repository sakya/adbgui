using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.SingleWindow.Abstracts;
using Avalonia.Threading;

namespace adbgui.Dialogs;

public partial class InstallApk : BaseDialog
{
    private CancellationTokenSource? _tokenSource;
    private readonly StringBuilder _log = new();

    public InstallApk()
    {
        InitializeComponent();

        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;
    }

    public string[]? ApkFiles { get; set; }

    protected override void Opened()
    {
        base.Opened();

        Log.Text = string.Empty;
        if (ApkFiles?.Length > 0) {
            _tokenSource = new CancellationTokenSource();
            Task.Run(() => Install(_tokenSource.Token));
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

    private async void Install(CancellationToken token)
    {
        if (ApkFiles == null)
            return;

        var result = true;
        foreach (var apk in ApkFiles) {
            if (token.IsCancellationRequested) {
                _log.AppendLine("User aborted");
                UpdateLog();
                break;
            }

            _log.AppendLine($"Installing {apk}");
            UpdateLog();

            var res = await Adb.Adb.Instance!.InstallApk(App.SelectedDevice!.Id, apk);
            if (result && !res.Result)
                result = false;

            _log.AppendLine(res.Output);
            _log.AppendLine(res.Result ? "SUCCESS" : "FAILED");
            _log.AppendLine();
            UpdateLog();
        }

        if (_tokenSource != null) {
            _tokenSource.Dispose();
            _tokenSource = null;
        }

        Dispatcher.UIThread.Post(() =>
        {
            TxtButtonText.Text = Localizer.Localizer.Instance["Close"];
            Button.Classes.Remove("Danger");
            Button.Classes.Add(result ? "Success" : "Warning");
        });
    }

    private void UpdateLog()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Log.Text = _log.ToString();
        });
    }
}