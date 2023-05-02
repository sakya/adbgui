namespace adbgui.Adb.Models;

public class Device
{
    public string? Id { get; set; }
    public bool Authorized { get; set; }
    public string? Product { get; set; }
    public string? Model { get; set; }
    public string? Name { get; set; }
    public string? TransportId { get; set; }
}