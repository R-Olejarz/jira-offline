using System;
using CommunityToolkit.Mvvm.ComponentModel;
using ToDoListApp.Models;

namespace ToDoListApp.ViewModels;

public partial class TodoDetailsViewModel : ViewModelBase
{
    private readonly TodoItem _item;

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private TodoPriority _priority;

    public TodoPriority[] Priorities { get; } =
        Enum.GetValues<TodoPriority>();

    public string CreatedAtText => _item.CreatedAt.ToString();

    public TodoDetailsViewModel(TodoItem item)
    {
        _item = item;

        _title = item.Title;
        _description = item.Description;
        _priority = item.Priority;
    }

    public bool ApplyChanges()
    {
        if (string.IsNullOrWhiteSpace(Title))
            return false;

        _item.Title = Title.Trim();
        _item.Description = Description.Trim();
        _item.Priority = Priority;

        return true;
    }
}