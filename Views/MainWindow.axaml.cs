using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using JiraOffline.Models;
using JiraOffline.ViewModels;
using Avalonia.Interactivity;
using JiraOffline.Services;

namespace JiraOffline.Views;

public partial class MainWindow : Window
{
    private TodoItem? _draggedItem;
    private TodoItem? _dropTargetItem;

    private TodoItemView? _draggedView;
    private TodoItemView? _dropTargetView;

    private Point _pressPosition;
    private bool _isDragging;
    private bool _dropAfter;
    private const double DragThreshold = 7;

    private bool _closeAfterSave;
    private readonly WindowSettingsService _windowSettingsService = new();
    public MainWindow()
    {
        InitializeComponent();

        RestoreWindowSize();
        
        TaskList.AddHandler(
            InputElement.PointerPressedEvent,
            TaskList_OnPointerPressed,
            RoutingStrategies.Tunnel,
            handledEventsToo: true);

        TaskList.AddHandler(
            InputElement.PointerMovedEvent,
            TaskList_OnPointerMoved,
            RoutingStrategies.Tunnel,
            handledEventsToo: true);

        TaskList.AddHandler(
            InputElement.PointerReleasedEvent,
            TaskList_OnPointerReleased,
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
    }
    
    private void RestoreWindowSize()
    {
        var settings = _windowSettingsService.Load();

        Width = Math.Max(MinWidth, settings.Width);
        Height = Math.Max(MinHeight, settings.Height);
    }
    
    private void TaskList_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(TaskList);

        if (!point.Properties.IsLeftButtonPressed)
            return;

        if (e.Source is not Control sourceControl)
            return;
        
        if (sourceControl.FindAncestorOfType<Button>(includeSelf: true) is not null ||
            sourceControl.FindAncestorOfType<CheckBox>(includeSelf: true) is not null)
        {
            return;
        }

        var itemView = sourceControl.FindAncestorOfType<TodoItemView>(includeSelf: true);

        if (itemView?.DataContext is not TodoItem item)
            return;

        _draggedItem = item;
        _draggedView = itemView;

        _pressPosition = point.Position;
        _isDragging = false;
    }
    
    private void TaskList_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_draggedItem is null)
            return;

        var pointerPoint = e.GetCurrentPoint(TaskList);

        if (!pointerPoint.Properties.IsLeftButtonPressed)
        {
            ResetDrag();
            return;
        }

        var offset = pointerPoint.Position - _pressPosition;

        if (!_isDragging)
        {
            var movedEnough = Math.Abs(offset.X) >= DragThreshold || Math.Abs(offset.Y) >= DragThreshold;

            if (!movedEnough)
                return;

            _isDragging = true;
            _draggedView?.SetDragging(true);
            
            e.Pointer.Capture(TaskList);
        }

        var hitControl = TaskList.InputHitTest(pointerPoint.Position) as Control;

        var targetView = hitControl?.FindAncestorOfType<TodoItemView>(includeSelf: true);

        if (targetView?.DataContext is not TodoItem targetItem)
        {
            _dropTargetItem = null;
            ClearDropTarget();
            return;
        }

        if (ReferenceEquals(_draggedItem, targetItem))
        {
            _dropTargetItem = null;
            ClearDropTarget();
            return;
        }

        var positionInTarget = e.GetPosition(targetView);

        var dropAfter = positionInTarget.Y >= targetView.Bounds.Height / 2;

        if (DataContext is not MainWindowViewModel viewModel)
        {
            ClearDropTarget();
            return;
        }

        var oldIndex = viewModel.Items.IndexOf(_draggedItem);
        var targetIndex = viewModel.Items.IndexOf(targetItem);

        if (oldIndex < 0 || targetIndex < 0)
        {
            ClearDropTarget();
            return;
        }

        var proposedIndex = CalculateDropIndex(oldIndex, targetIndex, dropAfter);

        if (proposedIndex == oldIndex)
        {
            ClearDropTarget();
            return;
        }
        
        var gapIndex = targetIndex + (dropAfter ? 1 : 0);
        
        TodoItemView? indicatorView;
        bool indicatorAfter;
        
        if (gapIndex < viewModel.Items.Count)
        {
            indicatorView = GetItemViewAtIndex(gapIndex);
            indicatorAfter = false;
        }
        else
        {
            indicatorView =
                GetItemViewAtIndex(viewModel.Items.Count - 1);

            indicatorAfter = true;
        }

        if (indicatorView is null)
        {
            ClearDropTarget();
            return;
        }

        _dropTargetItem = targetItem;
        _dropAfter = dropAfter;

        SetDropTarget(indicatorView, indicatorAfter);
    }

    private void TaskList_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_draggedItem is null)
            return;

        if (_isDragging && _dropTargetItem is not null && DataContext is MainWindowViewModel viewModel)
        {
            var oldIndex = viewModel.Items.IndexOf(_draggedItem);
            var targetIndex = viewModel.Items.IndexOf(_dropTargetItem);
            var newIndex = CalculateDropIndex(oldIndex, targetIndex, _dropAfter);

            if (oldIndex >= 0 && targetIndex >= 0 && newIndex != oldIndex)
                viewModel.MoveItem(oldIndex, newIndex);
        }
        
        if (ReferenceEquals(e.Pointer.Captured, TaskList))
            e.Pointer.Capture(null);

        ResetDrag();
    }

    private void ResetDrag()
    {
        _draggedView?.SetDragging(false);

        ClearDropTarget();

        _draggedItem = null;
        _dropTargetItem = null;
        _draggedView = null;

        _dropAfter = false;
        _isDragging = false;
    }
    
    private void SetDropTarget(TodoItemView indicatorView, bool indicatorAfter)
    {
        if (!ReferenceEquals(_dropTargetView, indicatorView))
        {
            _dropTargetView?.ClearDropPosition();
            _dropTargetView = indicatorView;
        }

        _dropTargetView.SetDropPosition(indicatorAfter);
    }

    private void ClearDropTarget()
    {
        _dropTargetView?.ClearDropPosition();

        _dropTargetView = null;
        _dropTargetItem = null;
        _dropAfter = false;
    }
    
    private static int CalculateDropIndex(int oldIndex, int targetIndex, bool dropAfter)
    {
        var newIndex = targetIndex + (dropAfter ? 1 : 0);
        
        if (oldIndex < newIndex)
            newIndex--;

        return newIndex;
    }
    
    private TodoItemView? GetItemViewAtIndex(int index)
    {
        if (TaskList.ContainerFromIndex(index) is not Control container)
            return null;

        return container.FindDescendantOfType<TodoItemView>(
            includeSelf: true);
    }
    
    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        if (DataContext is MainWindowViewModel viewModel)
            await viewModel.LoadAsync();
    }
    
    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        if (_closeAfterSave)
        {
            base.OnClosing(e);
            return;
        }

        e.Cancel = true;

        try
        {
            _windowSettingsService.Save(Width, Height);
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Saving window size failed: {exception}");
        }

        if (DataContext is MainWindowViewModel viewModel)
            await viewModel.SaveNowAsync();

        _closeAfterSave = true;
        Close();
    }
}