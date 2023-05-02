using System;
using System.Reflection;
using adbgui.Pages;
using Avalonia.SingleWindow;

namespace adbgui;

public partial class MainWindow : MainWindowBase
{
    public MainWindow()
    {
        InitializeComponent();

        if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaTitleBarHeightHint = -1;
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
        }

        WindowTitle = $"adbGUI - v{Assembly.GetExecutingAssembly().GetName().Version!.ToString()}";
        Container = ContainerGrid;
        Localizer.Localizer.Instance.LoadLanguage(App.Settings.Language);

        //Adb.Adb.Instance = new Adb.Adb(@"c:\Users\paolo.iommarini\Downloads\ADB\adb.exe");
        Adb.Adb.Instance = new Adb.Adb("/home/sakya/Downloads/platform-tools/adb");
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        await NavigateTo(new DeviceSelect());
    }

    protected override void PageChanged()
    {
        TitleBar.CanGoBack = CanNavigateBack;
    }
}