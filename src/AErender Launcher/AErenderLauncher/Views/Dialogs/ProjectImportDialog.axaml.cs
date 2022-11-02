using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Project;
using AErenderLauncher.Classes.Rendering;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using DynamicData;
using static AErenderLauncher.App;

namespace AErenderLauncher.Views.Dialogs; 

public partial class ProjectImportDialog : Window {
    public ObservableCollection<ProjectItem> Items { get; set; } = new ObservableCollection<ProjectItem>();

    public RenderTask? Result { get; set; } = null;

    public ProjectImportDialog() {
        InitializeComponent();
        
        ExtendClientAreaToDecorationsHint = Helpers.Platform != OperatingSystemType.OSX;
        Root.RowDefinitions = Helpers.Platform == OperatingSystemType.OSX ? new RowDefinitions("0,32,*,32") : new RowDefinitions("32,32,*,32");
    }

    public ProjectImportDialog(ProjectItem[] items) {
        InitializeComponent();

        ExtendClientAreaToDecorationsHint = Helpers.Platform != OperatingSystemType.OSX;
        Root.RowDefinitions = Helpers.Platform == OperatingSystemType.OSX ? new RowDefinitions("0,32,*,32") : new RowDefinitions("32,32,*,32");
        
        Items.AddRange(items);
    }
    
    private void CloseButton_OnClick(object sender, RoutedEventArgs e) {
        Close();
    }

    private void TopLevel_OnClosed(object sender, EventArgs e) {
        List<Composition> compositions = new();
        foreach (ProjectItem? item in CompositionsList.SelectedItems) {
            if (item != null) {
                compositions.Add(new() {
                    CompositionName = item.Name,
                    Frames = new() {
                        StartFrame = item.Frames[0],
                        EndFrame = item.Frames[1]
                    },
                });
            }
        }
        
        Result = new() {
            Project = ApplicationSettings.LastProjectPath,
            Output = ApplicationSettings.LastOutputPath,
            Multiprocessing = ApplicationSettings.Multithreaded,
            MissingFiles = ApplicationSettings.MissingFiles,
            Sound = ApplicationSettings.Sound,
            CacheLimit = ApplicationSettings.CacheLimit,
            MemoryLimit = ApplicationSettings.MemoryLimit,
            CustomProperties = ApplicationSettings.CustomProperties,
            OutputModule = ApplicationSettings.ActiveOutputModule?.Module ?? "Lossless",
            RenderSettings = ApplicationSettings.RenderSettings,
            Compositions = compositions
        };
    }

    private void CompositionsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
        ProjectItem? added   = e.AddedItems.Count > 0 ? e.AddedItems[e.AddedItems.Count - 1] as ProjectItem ?? null : null;
        ProjectItem? removed = e.RemovedItems.Count > 0 ? e.RemovedItems[e.RemovedItems.Count - 1] as ProjectItem ?? null : null;

        ProjectItem? item = added ?? removed ?? null;

        if (item != null) {
            CompLabel.Content = $"Composition: {item.Name}";
            ResLabel.Content = $"Resolution: {item.FootageDimensions[0]}x{item.FootageDimensions[1]}";
            FramerateLabel.Content = $"Framerate: {Math.Round(item.FootageFramerate, 3)}";
            SFrameLabel.Content = $"Range start: {item.Frames[0]}";
            EFrameLabel.Content = $"Range end: {item.Frames[1]}"; 
        }

    }
}