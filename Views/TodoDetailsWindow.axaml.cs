using Avalonia.Controls;
using Avalonia.Interactivity;
using JiraOffline.ViewModels;

namespace JiraOffline.Views;

public partial class TodoDetailsWindow : Window
{
    public TodoDetailsWindow()
    {
        InitializeComponent();
    }

    private void Save_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TodoDetailsViewModel viewModel)
            return;

        var saved = viewModel.ApplyChanges();

        if (!saved)
            return;

        Close(true);
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}