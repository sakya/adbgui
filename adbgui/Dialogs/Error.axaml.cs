using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.SingleWindow.Abstracts;

namespace adbgui.Dialogs;

public partial class Error : BaseDialog
{
    public Error()
    {
        Exception = new Exception();
    }

    public Error(Exception exception)
    {
        Exception = exception;
        InitializeComponent();
        TxtErrorMessage.Text = Exception.Message;
    }

    public Exception Exception { get; init; }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}