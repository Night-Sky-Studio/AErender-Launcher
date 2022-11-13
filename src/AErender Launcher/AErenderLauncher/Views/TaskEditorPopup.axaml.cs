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
using static AErenderLauncher.App;

namespace AErenderLauncher.Views; 

public partial class TaskEditorPopup : Window {
    private bool _IsEditing { get; set; } = false;

    public RenderTask? Result { get; set; }
    
    public TaskEditorPopup() {
        InitializeComponent();
        Result = new RenderTask();
    }
    
    public TaskEditorPopup(RenderTask task, bool IsEditing = false) {
        InitializeComponent();
        
        ExtendClientAreaToDecorationsHint = Helpers.Platform != OperatingSystemType.OSX;
        Root.RowDefinitions = Helpers.Platform == OperatingSystemType.OSX ? new RowDefinitions("0,*") : new RowDefinitions("32,*");
        
        Result = task;
        _IsEditing = IsEditing;
        
        CreateTaskButton.IsVisible = !_IsEditing;
        SaveTaskButton.IsVisible = _IsEditing;

        ProjectPath.Text = Result.Project;
        OutputPath.Text = Result.Output;
        
        OutputModuleBox.SelectedIndex = ApplicationSettings.OutputModules.IndexOf(Result.OutputModule);
        RenderSettings.Text = Result.RenderSettings;

        MissingCheckBox.IsChecked = Result.MissingFiles;
        SoundCheckBox.IsChecked = Result.Sound;
        ThreadedCheckbox.IsChecked = Result.Multiprocessing;

        CustomCheckBox.IsChecked = Result.CustomProperties != "";
        CustomProperties.Text = Result.CustomProperties;
    }

    private void CompositionsButton_OnClick(object sender, RoutedEventArgs e) => EditorCarousel.Next();

    private void ProjectSetupButton_OnClick(object sender, RoutedEventArgs e) => EditorCarousel.Previous();

    private async void OutputPathButton_OnClick(object sender, RoutedEventArgs e) {
        SaveFileDialog SaveFileBox = new() {
            Title = "Save render...",
            Filters = new List<FileDialogFilter> {
                new() {
                    Extensions = { "[fileExtension]" }, Name = "Output Module file format"
                }
            },
            InitialFileName = "[compName].[fileExtension]",
            DefaultExtension = "[fileExtension]"
        };

        OutputPath.Text = await SaveFileBox.ShowAsync(this);
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e) {
        Close(null);
    }

    private void SaveTaskButton_OnClick(object? sender, RoutedEventArgs e) {
        Close(Result);
    }
}