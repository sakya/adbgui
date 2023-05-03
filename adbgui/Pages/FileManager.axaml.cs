using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.SingleWindow.Abstracts;

namespace adbgui.Pages;

public partial class FileManager : BasePage
{
    public FileManager()
    {
        InitializeComponent();

        PageTitle = Localizer.Localizer.Instance["FileManager"];
    }
}