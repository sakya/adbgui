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

        WindowTitle = $"adbGUI - v{Assembly.GetExecutingAssembly().GetName().Version!.ToString()}";
        Container = ContainerGrid;
        Localizer.Localizer.Instance.LoadLanguage("en-US");
        Adb.Adb.Instance = new Adb.Adb(@"c:\Users\paolo.iommarini\Downloads\ADB\adb.exe");
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        await NavigateTo(new DeviceSelect());
    }
}