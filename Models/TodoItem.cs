using System;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;
namespace JiraOffline.Models;

public enum TodoPriority
{
    Low,
    Medium,
    High,
}

public partial class TodoItem : ObservableObject
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private bool _isDone;
    [ObservableProperty] private TodoPriority _priority;
    public DateTime CreatedAt { get; init; } = DateTime.Now;

    public TodoItem(string title, string description, TodoPriority priority)
    {
        _title = title;
        _description = description;
        _priority = priority;
        _isDone = false;
    }
    
    [JsonIgnore]
    public string PriorityFlags => Priority switch
    {
        TodoPriority.Low => "",
        TodoPriority.Medium => " ",
        TodoPriority.High => "  ",
        _ => string.Empty
    };

    [JsonIgnore]
    public bool IsLowPriority => Priority == TodoPriority.Low;

    [JsonIgnore]
    public bool IsMediumPriority => Priority == TodoPriority.Medium;

    [JsonIgnore]
    public bool IsHighPriority => Priority == TodoPriority.High;
    
    partial void OnPriorityChanged(TodoPriority value)
    {
        OnPropertyChanged(nameof(PriorityFlags));
        OnPropertyChanged(nameof(IsLowPriority));
        OnPropertyChanged(nameof(IsMediumPriority));
        OnPropertyChanged(nameof(IsHighPriority));
    }
}