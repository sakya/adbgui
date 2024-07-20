﻿using Avalonia;
using Avalonia.Platform;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace adbgui.Localizer;

public class Localizer : INotifyPropertyChanged
{
    private const string IndexerName = "Item";
    private const string IndexerArrayName = "Item[]";
    private Dictionary<string, string>? _strings;

    public bool LoadLanguage(string language)
    {
        Uri uri = new Uri($"avares://adbgui/Assets/i18n/{language}.json");
        if (AssetLoader.Exists(uri)) {
            using (StreamReader sr = new StreamReader(AssetLoader.Open(uri), Encoding.UTF8)) {
                _strings = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());
            }
            Language = language;
            Invalidate();

            return true;
        }
        return false;
    } // LoadLanguage

    public string? Language { get; private set; }

    public string this[string key]
    {
        get
        {
            if (_strings != null && _strings.TryGetValue(key, out var res))
                return res.Replace("\\n", "\n");

            return $"{Language}:{key}";
        }
    }

    public static Localizer Instance { get; set; } = new Localizer();
    public event PropertyChangedEventHandler? PropertyChanged;

    public void Invalidate()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
    }
}