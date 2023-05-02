using adbgui.Abstracts;

namespace adbgui.Controls.Models;

public class SpinnerModel : BaseModel
{
    private string? _message;
    public string? Message
    {
        get => _message;
        set => SetField(ref _message, value);
    }
}