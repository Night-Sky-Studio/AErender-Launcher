using System;
using System.Collections.Generic;
using System.IO;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Rendering;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;

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
        
        ExtendClientAreaToDecorationsHint = Helpers.Platform != OperatingSystemType.OSX;
        Root.RowDefinitions = Helpers.Platform == OperatingSystemType.OSX ? new RowDefinitions("0,*") : new RowDefinitions("32,*");
        
        _task = task;
        _IsEditing = IsEditing;
        
        CreateTaskButton.IsVisible = !_IsEditing;
        SaveTaskButton.IsVisible = _IsEditing;

        ProjectPath.Text = _task.Project;
        OutputPath.Text = _task.Output;
        
        // OutputModule.SelectedIndex = _task.OutputModule;
        RenderSettings.Text = _task.RenderSettings;

        MissingCheckBox.IsChecked = _task.MissingFiles;
        SoundCheckBox.IsChecked = _task.Sound;
        ThreadedCheckbox.IsChecked = _task.Multiprocessing;

        CustomCheckBox.IsChecked = _task.CustomProperties != "";
        CustomProperties.Text = _task.CustomProperties;
    }

    private void CompositionsButton_OnClick(object sender, RoutedEventArgs e) {
        EditorCarousel.Next();
    }

    private void ProjectSetupButton_OnClick(object sender, RoutedEventArgs e) {
        EditorCarousel.Previous();
    }

    private async void OutputPathButton_OnClick(object sender, RoutedEventArgs e) {
        SaveFileDialog SaveFileBox = new SaveFileDialog();
        SaveFileBox.Title = "Save Document As...";
        //SaveFileBox.InitialFileName = Path.GetFullPath(DocumentFileName);
        //SaveFileBox.Directory = workdir;

        SaveFileBox.Filters = new List<FileDialogFilter>() {
            new FileDialogFilter() {
                Extensions = { "[fileExtension]" }, Name = "Output Module file format"
            }
        };
        SaveFileBox.InitialFileName = "[compName].[fileExtension]";
        SaveFileBox.DefaultExtension = "[fileExtension]";

        OutputPath.Text = await SaveFileBox.ShowAsync(this);
    }
}