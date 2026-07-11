using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ToDoListApp.Views;

public partial class ConfirmDeleteWindow : Window
{
    public ConfirmDeleteWindow()
    {
        InitializeComponent();
    }

    private void No_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void Yes_Click(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }
}