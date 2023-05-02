using System.IO;
using adbgui.Models;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace adbgui;

public partial class App : Application
{
    public static Settings Settings { get; set; } = new Settings();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        if (!Directory.Exists(Settings.Path))
            Directory.CreateDirectory(Settings.Path);
        var settings = Settings.Load();
        if (settings == null) {
            settings = new Settings();
            settings.Save();
        } else {
            Settings = settings;
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}