using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using ToDoListApp.Models;
using ToDoListApp.ViewModels;

namespace ToDoListApp.Views;

public partial class TodoItemView : UserControl
{
    public TodoItemView()
    {
        InitializeComponent();
    }

    public void SetDragging(bool isDragging)
    {
        RootCard.Classes.Set("dragging", isDragging);
    }

    public void SetDropPosition(bool dropAfter)
    {
        DropBeforeIndicator.Classes.Set("active", !dropAfter);
        DropAfterIndicator.Classes.Set("active", dropAfter);
    }

    public void ClearDropPosition()
    {
        DropBeforeIndicator.Classes.Set("active", false);
        DropAfterIndicator.Classes.Set("active", false);
    }

    private async void ShowDetails_Click(object? sender, RoutedEventArgs e)
    {
        await OpenDetailsAsync();
    }

    private async void RootCard_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Source is Control sourceControl && (sourceControl.FindAncestorOfType<Button>(includeSelf: true) is not null || sourceControl.FindAncestorOfType<CheckBox>(includeSelf: true) is not null))
            return;

        e.Handled = true;

        await OpenDetailsAsync();
    }

    private async Task OpenDetailsAsync()
    {
        if (DataContext is not TodoItem item)
            return;

        var owner = TopLevel.GetTopLevel(this) as Window;

        if (owner is null)
            return;

        var window = new TodoDetailsWindow
        {
            DataContext = new TodoDetailsViewModel(item)
        };

        await window.ShowDialog<bool>(owner);
    }

    private async void Delete_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TodoItem item)
            return;

        var owner = TopLevel.GetTopLevel(this) as Window;

        if (owner is null)
            return;

        var dialog = new ConfirmDeleteWindow
        {
            DataContext = item
        };

        var confirmed = await dialog.ShowDialog<bool>(owner);

        if (!confirmed)
            return;

        if (owner.DataContext is MainWindowViewModel viewModel)
            viewModel.RemoveItemCommand.Execute(item);
    }
}