using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToDoListApp.Models;
using ToDoListApp.Services;

namespace ToDoListApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly TodoListStorageService _storageService = new();
    private CancellationTokenSource? _saveDelayCancellation;
    private bool _isLoading;
    
    public ObservableCollection<TodoItem> Items { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddItemCommand))]
    private string _newItemTitle = string.Empty;

    [ObservableProperty]
    private TodoPriority _newItemPriority = TodoPriority.Low;

    [ObservableProperty]
    private TodoItem? _selectedItem;

    public TodoPriority[] Priorities { get; } =
    [
        TodoPriority.Low,
        TodoPriority.Medium,
        TodoPriority.High
    ];

    public MainWindowViewModel()
    {
        Items.CollectionChanged += (_, _) =>
        {
            if (!_isLoading)
                RequestSave();
        };
    }

    private bool CanAddItem() => !string.IsNullOrWhiteSpace(NewItemTitle);

    [RelayCommand(CanExecute = nameof(CanAddItem))]
    private void AddItem()
    {
        var title = NewItemTitle.Trim();

        Items.Add(new TodoItem(title, string.Empty, NewItemPriority));

        NewItemTitle = string.Empty;
    }

    [RelayCommand]
    private void RemoveItem(TodoItem? item)
    {
        if (item is null)
            return;

        Items.Remove(item);
    }

    public void MoveItem(int oldIndex, int newIndex)
    {
        if (oldIndex < 0 || newIndex < 0)
            return;

        if (oldIndex >= Items.Count || newIndex >= Items.Count)
            return;

        if (oldIndex == newIndex)
            return;

        Items.Move(oldIndex, newIndex);
    }

    private void RequestSave()
    {
        _saveDelayCancellation?.Cancel();
        _saveDelayCancellation = new CancellationTokenSource();

        _ = SaveAfterDelayAsync(_saveDelayCancellation.Token);
    }

    private async Task SaveAfterDelayAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(300, cancellationToken);
            await _storageService.SaveAsync(Items);

            Debug.WriteLine("Task list saved.");
        }
        catch (OperationCanceledException)
        {

        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Saving failed: {exception}");
        }
    }

    public async Task SaveNowAsync()
    {
        _saveDelayCancellation?.Cancel();

        try
        {
            await _storageService.SaveAsync(Items);

            Debug.WriteLine("Task list saved immediately.");
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Saving failed: {exception}");
        }
    }
    
    public async Task LoadAsync()
    {
        _isLoading = true;

        try
        {
            var loadedItems = await _storageService.LoadAsync();

            Items.Clear();

            foreach (var item in loadedItems)
                Items.Add(item);

            Debug.WriteLine($"Loaded {Items.Count} tasks.");
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Loading failed: {exception}");
        }
        finally
        {
            _isLoading = false;
        }
    }
}