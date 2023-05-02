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

        WindowTitle = "adb GUI";
        Container = ContainerGrid;
        Localizer.Localizer.Instance.LoadLanguage("en-US");
        Adb.Adb.Instance = new Adb.Adb(@"c:\Users\paolo.iommarini\Downloads\ADB\adb.exe");
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