using System;
using System.IO;
using Newtonsoft.Json;

namespace adbgui.Models;

public class Settings
{
    public static readonly string Path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adbgui");

    public string Language { get; set; } = "en-US";
    public string AdbFullPath { get; set; } = "adb";

    public static Settings? Load()
    {
        var file = System.IO.Path.Combine(Path, "settings.json");
        if (!File.Exists(file))
            return null;

        using var sr = new StreamReader(file);
        var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects };
        return JsonConvert.DeserializeObject<Settings>(sr.ReadToEnd(), settings);
    }

    public void Save()
    {
        using var sw = new StreamWriter(System.IO.Path.Combine(Path, "settings.json"));
        var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects };
        sw.Write(JsonConvert.SerializeObject(this, Formatting.Indented, settings));
    }
}