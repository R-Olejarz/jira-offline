using System;
using System.IO;
using System.Text.Json;
using ToDoListApp.Models;

namespace ToDoListApp.Services;

public sealed class WindowSettingsService
{
    private readonly string _filePath;

    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    public WindowSettingsService()
    {
        var appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);
        var applicationDirectory = Path.Combine(appDataDirectory, "ToDoListApp");

        Directory.CreateDirectory(applicationDirectory);

        _filePath = Path.Combine(applicationDirectory, "window-settings.json");
    }

    public WindowSettings Load()
    {
        try
        {
            if (!File.Exists(_filePath))
                return new WindowSettings();

            var json = File.ReadAllText(_filePath);

            return JsonSerializer.Deserialize<WindowSettings>(json, _options) ?? new WindowSettings();
        }
        catch
        {
            return new WindowSettings();
        }
    }

    public void Save(double width, double height)
    {
        var settings = new WindowSettings
        {
            Width = width,
            Height = height
        };

        var json = JsonSerializer.Serialize(settings, _options);

        File.WriteAllText(_filePath, json);
    }
}