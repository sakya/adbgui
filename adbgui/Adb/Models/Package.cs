using System.Text.Json;

namespace adbgui.Adb.Models;

public class Package
{
    public string? Uid { get; set; }
    public string? Name { get; set; }
    public string? File { get; set; }
    public bool Enabled { get; set; }
    public bool System { get; set; }
    public bool ThirdParty { get; set; }
}