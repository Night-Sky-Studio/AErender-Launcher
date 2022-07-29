using System;
using AErenderLauncher.Classes.Rendering;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace AErenderLauncher.Views; 

public partial class TaskEditorPopup : Window {
    private RenderTask _task { get; set; }
    private bool _IsEditing { get; set; } = false;

    public TaskEditorPopup() {
        InitializeComponent();
        _task = new RenderTask();
    }
    
    public TaskEditorPopup(RenderTask task, bool IsEditing = false) {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        _task = task;
        _IsEditing = IsEditing;
        
        CreateTaskButton.IsVisible = !_IsEditing;
        SaveTaskButton.IsVisible = _IsEditing;

        ProjectPath.Text = _task.Project;
    }

    private void CompositionsButton_OnClick(object sender, RoutedEventArgs e) {
        EditorCarousel.Next();
    }

    private void ProjectSetupButton_OnClick(object sender, RoutedEventArgs e) {
        EditorCarousel.Previous();
    }
}