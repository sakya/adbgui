using System.IO;
using adbgui.Adb.Models;
using adbgui.Models;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace adbgui;

public class App : Application
{
    public static Settings Settings { get; set; } = new();
    public static Device? SelectedDevice { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        if (!Directory.Exists(Settings.Path))
            Directory.CreateDirectory(Settings.Path);
        var settings = Settings.Load();
        if (settings == null) {
            settings = new Settings();
            settings.Save();
        }
        Settings = settings;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}