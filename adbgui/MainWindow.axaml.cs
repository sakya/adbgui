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
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        await NavigateTo(new MainPage());
    }
}