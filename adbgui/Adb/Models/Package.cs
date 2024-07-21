using System.Text.Json;

namespace adbgui.Adb.Models;

public class Package
{
    public string? Uid { get; init; }
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? File { get; set; }
    public bool Enabled { get; set; }
    public bool System { get; set; }
    public bool ThirdParty { get; set; }

    public string Type
    {
        get
        {
            if (System)
                return "System";
            if (ThirdParty)
                return "Third Party";
            return string.Empty;
        }
    }
}