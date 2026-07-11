using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using JiraOffline.Models;

namespace JiraOffline.Services;

public sealed class TodoListStorageService
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    public string FilePath => _filePath;
    
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public TodoListStorageService()
    {
        var appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var applicationDirectory = Path.Combine(appDataDirectory, "JiraOffline");

        Directory.CreateDirectory(applicationDirectory);
        _filePath = Path.Combine(applicationDirectory, "tasks.json");
    }
    
    public async Task SaveAsync(IEnumerable<TodoItem> items)
    {
        var snapshot = items.ToList();

        await _saveLock.WaitAsync();

        try
        {
            var json = JsonSerializer.Serialize(snapshot, _options);
            await File.WriteAllTextAsync(_filePath, json);
        }
        finally
        {
            _saveLock.Release();
        }
    }
    
    public async Task<List<TodoItem>> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return [];

        await using var stream = File.OpenRead(_filePath);

        var items = await JsonSerializer.DeserializeAsync<List<TodoItem>>(stream, _options);

        return items ?? [];
    }
}