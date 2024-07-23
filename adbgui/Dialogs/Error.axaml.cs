using System;
using Avalonia.Interactivity;
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

    private Exception Exception { get; }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}